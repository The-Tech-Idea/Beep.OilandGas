using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentWellTestTests
{
    private const string Json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"where\":{\"well\":{\"ACTIVE_IND\":\"Y\"}}}}}";
    private sealed class Store(CompanyEnvironmentConfiguration value) : ICompanyEnvironmentStore
    {
        public bool Change; private int reads;
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return Change && ++reads > 1 ? null : value; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class Bindings : ICompanyEnvironmentBindingReader
    {
        public bool Change; private int calls;
        public Task<string?> ResolveAsync(string module, CancellationToken token) => Task.FromResult<string?>(new string(Change && ++calls > 1 ? 'B' : 'A', 64));
    }
    private sealed class Reader : ICompanyEnvironmentWellReader
    {
        public bool Missing, Foreign, Change; public int Calls;
        public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(CompanyScope company, string wellId,
            IReadOnlyDictionary<string, string> filters, Func<Task> authority, CancellationToken token)
        {
            await authority(); Calls++; Assert.Equal(wellId, filters["UWI"]); Assert.Equal("Y", filters["ACTIVE_IND"]);
            if (Missing) return [];
            return [new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["UWI"] = Foreign ? "foreign" : wellId, ["WELL_NAME"] = Change && Calls > 1 ? "Changed well" : "North well", ["ASSIGNED_FIELD"] = "field" }];
        }
    }
    private static CompanyEnvironmentConfiguration Saved(CompanyScope company) => CompanyEnvironmentIdentity.Prepare(company,
        new("KW", Json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, TestSystemDefaults.Baseline.Revision, null);
    private static CompanyEnvironmentEffectiveResolver Effective(Store store) => new(store, new TestSystemDefaults());
    private static CompanyEnvironmentWellTestService Service(Store store, Bindings bindings, Reader reader) => new(Effective(store), bindings, reader);
    private static async Task<string> Revision(CompanyEnvironmentConfiguration saved) =>
        (await Effective(new Store(saved)).ResolveAsync(saved.Company, () => Task.CompletedTask)).EffectiveRevision;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RealToolsProfileMapsOnlyTheRequestedVisibleWell(bool missing)
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company); var reader = new Reader { Missing = missing };
        var service = Service(new Store(saved), new Bindings(), reader);
        var result = await service.TestAsync(company, new("wells", "well-1", saved.Revision, await Revision(saved)), () => Task.CompletedTask, default);
        Assert.Equal(missing ? "NotVisible" : "Found", result.Status); Assert.Equal(missing ? null : "North well", result.WellName);
        Assert.Equal(2, reader.Calls); Assert.False(result.RelatedTablesTested); Assert.Equal(32, result.LoaderVersion.Length);
    }

    [Fact]
    public async Task ForeignRecordChangedBindingOrChangedEnvironmentCannotPublish()
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company); var request = new CompanyEnvironmentWellTestRequest("wells", "well-1", saved.Revision, await Revision(saved));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings(), new Reader { Foreign = true }).TestAsync(company, request, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings(), new Reader { Change = true }).TestAsync(company, request, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings { Change = true }, new Reader()).TestAsync(company, request, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved) { Change = true }, new Bindings(), new Reader()).TestAsync(company, request, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings(), new Reader()).TestAsync(new("tenant", "other"), request, () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task StaleRevisionAndRevokedAuthorityStopTheWellTest()
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company); var reader = new Reader();
        var service = Service(new Store(saved), new Bindings(), reader); var effectiveRevision = await Revision(saved);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TestAsync(company, new("wells", "well-1", new string('B', 64), effectiveRevision), () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TestAsync(company, new("wells", "well-1", saved.Revision, effectiveRevision), () => throw new UnauthorizedAccessException(), default));
        Assert.Equal(0, reader.Calls);
    }

    [Fact]
    public async Task UnreviewedSharedSettingsStopTheWellReadEvenWithCurrentEffectiveRevision()
    {
        var company = new CompanyScope("tenant", "company");
        var stale = CompanyEnvironmentIdentity.Prepare(company, new("KW", Json, "", Guid.NewGuid().ToString("N")),
            CompanyEnvironmentPreviewer.ToolsVersion, null, new string('B', 64), null);
        var reader = new Reader();
        var revision = await Revision(stale);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(stale), new Bindings(), reader)
            .TestAsync(company, new("wells", "well-1", stale.Revision, revision), () => Task.CompletedTask, default));
        Assert.Equal(0, reader.Calls);
    }

    [Fact]
    public async Task ConfiguredFilterCannotSubstituteAnotherWell()
    {
        var company = new CompanyScope("tenant", "company");
        var json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"where\":{\"well\":{\"UWI\":\"foreign\"}}}}}";
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, TestSystemDefaults.Baseline.Revision, null);
        var reader = new Reader();
        var effectiveRevision = await Revision(saved);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings(), reader)
            .TestAsync(company, new("wells", "well-1", saved.Revision, effectiveRevision), () => Task.CompletedTask, default));
        Assert.Equal(0, reader.Calls);
    }
}
