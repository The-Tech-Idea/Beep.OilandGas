using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentSourceTests
{
    private sealed class Store(CompanyEnvironmentConfiguration value) : ICompanyEnvironmentStore
    {
        public bool ChangeAtClosing; private int reads;
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority)
        { await authority(); reads++; return ChangeAtClosing && reads > 1 ? null : value; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class Bindings : ICompanyEnvironmentBindingReader
    {
        public string? Result = new string('A', 64); public bool Change; private int calls;
        public int Calls => calls;
        public Task<string?> ResolveAsync(string module, CancellationToken token) { calls++; return Task.FromResult(Change && calls > 1 ? "changed" : Result); }
    }
    private const string Json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"A\":\"Production\"}}},\"where\":{\"well\":{\"ACTIVE_IND\":\"Y\"}}}}}";
    private static CompanyEnvironmentConfiguration Saved(CompanyScope company) => CompanyEnvironmentIdentity.Prepare(company,
        new("KW", Json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, TestSystemDefaults.Baseline.Revision, null);
    private static CompanyEnvironmentSources Service(Store store, Bindings bindings) => new(new CompanyEnvironmentEffectiveResolver(store, new TestSystemDefaults()), bindings);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReviewsNamedModuleBindingWithoutClaimingSourceDataWasTested(bool available)
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company);
        var bindings = new Bindings { Result = available ? new string('A', 64) : null };
        var review = await Service(new Store(saved), bindings).ReviewAsync(company, () => Task.CompletedTask, default);
        var source = Assert.Single(review.Sources);
        Assert.Equal("wells", source.Name); Assert.Equal("PPDM_CORE", source.Module); Assert.Equal("ppdm39", source.Profile);
        Assert.Equal(1, source.MappedCodeValues); Assert.Equal(1, source.RowFilters);
        Assert.Equal(available ? "Bound" : "Unavailable", source.BindingStatus); Assert.False(review.DataTested);
        Assert.Equal(saved.Revision, review.Revision);
        Assert.DoesNotContain(new string('A', 64), System.Text.Json.JsonSerializer.Serialize(review));
    }

    [Fact]
    public async Task ChangedBindingOrEnvironmentAndForeignCompanyAreRefused()
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings { Change = true }).ReviewAsync(company, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved) { ChangeAtClosing = true }, new Bindings()).ReviewAsync(company, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(saved), new Bindings()).ReviewAsync(new("tenant", "other"), () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task RevokedAuthorityCannotPublishSourceReview()
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company); var calls = 0;
        Task Authority() { if (++calls >= 3) throw new UnauthorizedAccessException(); return Task.CompletedTask; }
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Service(new Store(saved), new Bindings()).ReviewAsync(company, Authority, default));
    }

    [Fact]
    public async Task UnreviewedSharedSettingsStopSourceBindingReads()
    {
        var company = new CompanyScope("tenant", "company");
        var stale = CompanyEnvironmentIdentity.Prepare(company, new("KW", Json, "", Guid.NewGuid().ToString("N")),
            CompanyEnvironmentPreviewer.ToolsVersion, null, new string('B', 64), null);
        var bindings = new Bindings();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(new Store(stale), bindings).ReviewAsync(company, () => Task.CompletedTask, default));
        Assert.Equal(0, bindings.Calls);
    }
}
