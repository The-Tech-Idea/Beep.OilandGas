using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentRuntimeTests
{
    private sealed class Store(CompanyEnvironmentConfiguration? value) : ICompanyEnvironmentStore
    {
        public CompanyEnvironmentConfiguration? Value = value;
        public bool ChangeAtClosing;
        private int reads;
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority)
        {
            await authority(); await Task.Yield(); reads++;
            return ChangeAtClosing && reads > 1 ? null : Value;
        }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }

    private sealed class Defaults(string json = "{}") : ISystemSetupRecordReader
    {
        private readonly SystemSetupConfiguration baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", json, "", Guid.NewGuid().ToString("N")), null);
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { await authority(); return category == "TOOLS" && key == "DEFAULT" ? baseline : null; }
    }
    private static CompanyEnvironmentRuntime Runtime(Store store) => new(new CompanyEnvironmentEffectiveResolver(store, new Defaults()));

    private static CompanyEnvironmentConfiguration Saved(CompanyScope company, string country, string json = "{}") =>
        CompanyEnvironmentIdentity.Prepare(company, new(country, json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, new string('A', 64), null);

    [Fact]
    public async Task SavedRevisionRunsActualReferenceCasesWithoutClaimingSourcesAreVerified()
    {
        var company = new CompanyScope("tenant", "kuwait");
        var json = "{\"verification\":{\"cases\":[{\"name\":\"unit check\",\"kind\":\"unit\",\"value\":1,\"from\":\"ft\",\"to\":\"m\",\"expected\":0.3048,\"tolerance\":1e-9},{\"name\":\"source check\",\"kind\":\"dataSource\"}]}}";
        var saved = Saved(company, "KW", json); var runtime = Runtime(new Store(saved));
        var result = await runtime.VerifyAsync(company, () => Task.CompletedTask);
        Assert.Equal(saved.Revision, result.Revision); Assert.False(result.SourcesVerified);
        Assert.Equal("Passed", result.ReferenceCases[0].Status); Assert.Equal("Unavailable", result.ReferenceCases[1].Status);
    }

    [Fact]
    public async Task FailedReferenceCaseIsReportedAsFailed()
    {
        var company = new CompanyScope("tenant", "one");
        var json = "{\"verification\":{\"cases\":[{\"name\":\"wrong\",\"kind\":\"unit\",\"value\":1,\"from\":\"ft\",\"to\":\"m\",\"expected\":2,\"tolerance\":1e-9}]}}";
        var result = await Runtime(new Store(Saved(company, "KW", json))).VerifyAsync(company, () => Task.CompletedTask);
        Assert.Equal("Failed", Assert.Single(result.ReferenceCases).Status);
    }

    [Fact]
    public async Task InvalidTimezoneIsAnIssueNotAClaimOfReadiness()
    {
        var company = new CompanyScope("tenant", "one");
        var result = await Runtime(new Store(Saved(company, "KW", "{\"locale\":{\"timeZone\":\"not/a-zone\"}}"))).VerifyAsync(company, () => Task.CompletedTask);
        Assert.Contains(result.ValidationIssues, issue => issue.Contains("timeZone", StringComparison.OrdinalIgnoreCase));
        Assert.False(result.SourcesVerified);
    }

    [Fact]
    public async Task VerificationRunsAgainstSharedDefaultsRatherThanOnlyCompanyOverrides()
    {
        var company = new CompanyScope("tenant", "one"); var saved = Saved(company, "KW");
        var resolver = new CompanyEnvironmentEffectiveResolver(new Store(saved), new Defaults("{\"locale\":{\"timeZone\":\"not/a-zone\"}}"));
        var result = await new CompanyEnvironmentRuntime(resolver).VerifyAsync(company, () => Task.CompletedTask);
        Assert.Contains(result.ValidationIssues, issue => issue.Contains("timeZone", StringComparison.OrdinalIgnoreCase));
        Assert.True(CompanyEnvironmentIdentity.IsRevision(result.EffectiveRevision));
    }

    [Fact]
    public async Task DifferentCompaniesVerifyIndependentlyWithoutChangingAmbientServices()
    {
        using var ambient = OilGasServices.Scope(new OilGasServices(new OilGasEnvironment { Culture = "ambient" }));
        var one = new CompanyScope("tenant", "one"); var two = new CompanyScope("tenant", "two");
        var results = await Task.WhenAll(
            Runtime(new Store(Saved(one, "KW"))).VerifyAsync(one, () => Task.CompletedTask),
            Runtime(new Store(Saved(two, "NO"))).VerifyAsync(two, () => Task.CompletedTask));
        Assert.Equal(one, results[0].Company); Assert.Equal(two, results[1].Company);
        Assert.Equal("ambient", OilGasServices.Current.Environment.Culture);
    }

    [Fact]
    public async Task MissingChangedOrWrongBuildCannotBeVerified()
    {
        var company = new CompanyScope("tenant", "one"); var saved = Saved(company, "KW");
        await Assert.ThrowsAsync<InvalidOperationException>(() => Runtime(new Store(null)).VerifyAsync(company, () => Task.CompletedTask));
        var old = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), "different-build", null, new string('A', 64), null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Runtime(new Store(old)).VerifyAsync(company, () => Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Runtime(new Store(saved) { ChangeAtClosing = true }).VerifyAsync(company, () => Task.CompletedTask));
    }
}
