using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdea.OilGas.Core;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentEffectiveTests
{
    private sealed class Companies(CompanyEnvironmentConfiguration value) : ICompanyEnvironmentStore
    {
        public bool Change; private int reads;
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority)
        { await authority(); return Change && ++reads > 1 ? null : value; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class SystemRecords(SystemSetupConfiguration? baseline, SystemSetupConfiguration? country = null) : ISystemSetupRecordReader
    {
        public bool Change; private int baselineReads;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        {
            await authority(); await Task.Yield();
            if (category == "TOOLS") return Change && ++baselineReads > 1 ? null : baseline;
            return country;
        }
    }
    private static SystemSetupConfiguration Record(string category, string key, string json) =>
        SystemSetupIdentity.Prepare(new(category, key, "1", json, "", Guid.NewGuid().ToString("N")), null);
    private static CompanyEnvironmentConfiguration Saved(CompanyScope company, string country, string json) =>
        CompanyEnvironmentIdentity.Prepare(company, new(country, json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, new string('A', 64), null);

    [Fact]
    public async Task ResolvesSharedThenCountryThenCompanyWithoutChangingAmbientServices()
    {
        var company = new CompanyScope("tenant", "company");
        var baseline = Record("TOOLS", "DEFAULT", "{\"culture\":\"en-GB\",\"units\":{\"depthUnit\":\"m\",\"pressureUnit\":\"bar\"}}");
        var country = Record("COUNTRY", "KW", "{\"culture\":\"ar-KW\",\"units\":{\"pressureUnit\":\"kPa\"}}");
        var saved = Saved(company, "KW", "{\"culture\":\"en-US\",\"units\":{\"pressureUnit\":\"psi\"}}");
        using var ambient = OilGasServices.Scope(new OilGasServices(new OilGasEnvironment { Culture = "ambient" }));
        var result = await new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline, country)).ResolveAsync(company, () => Task.CompletedTask);
        Assert.Equal("en-US", result.Settings.Culture); Assert.Equal("m", result.Settings.DepthUnit); Assert.Equal("psi", result.Settings.PressureUnit);
        Assert.Equal(saved.Revision, result.CompanyRevision); Assert.Equal(baseline.Revision, result.SystemRevision); Assert.Equal(country.Revision, result.CountryRevision);
        Assert.True(CompanyEnvironmentIdentity.IsRevision(result.EffectiveRevision)); Assert.Equal("ambient", OilGasServices.Current.Environment.Culture);
    }

    [Fact]
    public async Task MissingChangedForeignOrUnsafeLayersNeverProduceAnEffectiveEnvironment()
    {
        var company = new CompanyScope("tenant", "company"); var saved = Saved(company, "KW", "{}");
        var baseline = Record("TOOLS", "DEFAULT", "{}");
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(null)).ResolveAsync(company, () => Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline) { Change = true }).ResolveAsync(company, () => Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved) { Change = true }, new SystemRecords(baseline)).ResolveAsync(company, () => Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline)).ResolveAsync(new("tenant", "foreign"), () => Task.CompletedTask));
        var unsafeBaseline = Record("TOOLS", "DEFAULT", "{\"extends\":\"C:/private.json\"}");
        await Assert.ThrowsAsync<ArgumentException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(unsafeBaseline)).ResolveAsync(company, () => Task.CompletedTask));
    }

    [Fact]
    public async Task DraftReviewUsesTheSameLayersAsSavedConfigurationAndRejectsChangingDefaults()
    {
        var company = new CompanyScope("tenant", "company");
        var saved = Saved(company, "KW", "{\"units\":{\"pressureUnit\":\"psi\"}}");
        var baseline = Record("TOOLS", "DEFAULT", "{\"culture\":\"en-GB\"}");
        var country = Record("COUNTRY", "KW", "{\"culture\":\"ar-KW\"}");
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline, country));
        var draft = await resolver.PreviewDraftAsync(company, new(saved.CountryCode, saved.OverridesJson), () => Task.CompletedTask);
        var effective = await resolver.ResolveAsync(company, () => Task.CompletedTask);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(effective.Settings), System.Text.Json.JsonSerializer.Serialize(draft.Settings));
        Assert.Equal("ar-KW", draft.Settings.Culture);
        Assert.Equal(baseline.Revision, draft.SystemRevision);
        Assert.Equal(country.Revision, draft.CountryRevision);
        Assert.Equal(CompanyEnvironmentDraftIdentity.ForReview(company, new(saved.CountryCode, saved.OverridesJson), baseline.Revision, country.Revision, draft.ToolsVersion), draft.DraftRevision);
        Assert.NotEqual(draft.DraftRevision, CompanyEnvironmentDraftIdentity.ForReview(company, new("KW", "{}"), baseline.Revision, country.Revision, draft.ToolsVersion));
        var changing = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline, country) { Change = true });
        await Assert.ThrowsAsync<InvalidOperationException>(() => changing.PreviewDraftAsync(company, new("KW", "{}"), () => Task.CompletedTask));
    }

    [Fact]
    public async Task SharedDefaultsCanDefineBeepSourcesButNotFilesystemPaths()
    {
        var company = new CompanyScope("tenant", "company");
        var saved = Saved(company, "KW", "{}");
        var baseline = Record("TOOLS", "DEFAULT", "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}");
        var result = await new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(baseline)).ResolveRuntimeAsync(company, () => Task.CompletedTask);
        Assert.Equal("PPDM_CORE", result.Environment.DataSources["wells"].Path);
        var invalid = Record("TOOLS", "DEFAULT", "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"C:/private\",\"profile\":\"ppdm39\"}}}");
        await Assert.ThrowsAsync<ArgumentException>(() => new CompanyEnvironmentEffectiveResolver(new Companies(saved), new SystemRecords(invalid)).ResolveAsync(company, () => Task.CompletedTask));
    }

    [Fact]
    public async Task CountryDefaultsAreIsolatedAndSharedChangesProduceNewEffectiveRevision()
    {
        var one = new CompanyScope("tenant", "one"); var two = new CompanyScope("tenant", "two");
        var kw = Saved(one, "KW", "{}"); var no = Saved(two, "NO", "{}");
        var baseline = Record("TOOLS", "DEFAULT", "{\"culture\":\"en-GB\"}");
        var changed = Record("TOOLS", "DEFAULT", "{\"culture\":\"fr-FR\"}");
        var results = await Task.WhenAll(
            new CompanyEnvironmentEffectiveResolver(new Companies(kw), new SystemRecords(baseline)).ResolveAsync(one, () => Task.CompletedTask),
            new CompanyEnvironmentEffectiveResolver(new Companies(no), new SystemRecords(baseline)).ResolveAsync(two, () => Task.CompletedTask));
        Assert.Equal("KW", results[0].Settings.CountryCode); Assert.Equal("NO", results[1].Settings.CountryCode);
        var later = await new CompanyEnvironmentEffectiveResolver(new Companies(kw), new SystemRecords(changed)).ResolveAsync(one, () => Task.CompletedTask);
        Assert.NotEqual(results[0].EffectiveRevision, later.EffectiveRevision); Assert.Equal("fr-FR", later.Settings.Culture);
    }
}
