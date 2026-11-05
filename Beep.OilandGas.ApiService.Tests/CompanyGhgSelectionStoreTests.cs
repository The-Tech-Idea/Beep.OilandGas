using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Configuration;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyGhgSelectionStoreTests
{
    private static readonly CompanyScope Company = new("tenant", "company");
    private sealed class Companies(CompanyEnvironmentConfiguration saved) : ICompanyEnvironmentStore
    {
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return saved; }
        public Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority) => throw new NotSupportedException();
    }
    private sealed class Records(SystemSetupConfiguration baseline, SystemSetupConfiguration catalog) : ISystemSetupRecordReader
    {
        public bool Changed;
        public SystemSetupConfiguration? ReplacementBaseline;
        public SystemSetupConfiguration? ChangeOnCatalogRead;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { await authority(); if (category == "GHG_FACTORS" && ChangeOnCatalogRead is not null) ReplacementBaseline = ChangeOnCatalogRead; return category switch { "TOOLS" => ReplacementBaseline ?? baseline, "COUNTRY" => null, "GHG_FACTORS" => Changed ? null : catalog, _ => null }; }
    }
    private sealed class Fixture
    {
        public readonly CompanyGLAccountReviewTests.Fixture Native = new();
        public readonly CompanyGhgSelectionStore Store;
        public readonly CompanyGhgCatalogReader Reader;
        public readonly Records Shared;
        public Action? OnWrite;
        public Fixture()
        {
            Native.Native.Rows.Clear(); Native.Native.Source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
            var properties = new ConnectionProperties { ConnectionName = "lifecycle", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
            Mock.Get(Native.Native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
            var registry = new Mock<IConfigEditor>(); registry.SetupGet(x => x.DataConnections).Returns([properties]); Native.Native.Editor.SetupGet(x => x.ConfigEditor).Returns(registry.Object);
            Native.Native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure _, bool _) => new ClassCreator(Native.Native.Editor.Object).ConvertToEntityStructure(typeof(COMPANY_GHG_CATALOG_SELECTION)));
            var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("COMPANY_GHG_CATALOG_SELECTION")).ReturnsAsync(["COMPANY_GHG_CATALOG_SELECTION_ID"]);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "lifecycle", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(Native.Native.Editor.Object, properties) }).Build();
            var baseline = Record("TOOLS", "DEFAULT", "{}");
            var factor = new GhgFactorDefinition("test", "flare", "volume", "mass", 1m, new(2026, 1, 1), new(2026, 12, 31), "source");
            var catalog = Record("GHG_FACTORS", "KW", JsonSerializer.Serialize(new GhgFactorCatalog("KW", [factor])));
            Shared = new(baseline, catalog);
            var environment = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
            Reader = new(new(new Companies(environment), Shared), Shared);
            Store = new(new SystemSetupTarget(configuration, Native.Native.Editor.Object), Native.Native.Editor.Object, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, Reader);
            Native.Native.Source.Setup(x => x.InsertEntity("COMPANY_GHG_CATALOG_SELECTION", It.IsAny<object>())).Returns((string _, object value) =>
            { Native.Native.Writes++; Native.Native.Rows.Add(JsonSerializer.Deserialize<COMPANY_GHG_CATALOG_SELECTION>(JsonSerializer.Serialize(value))!); OnWrite?.Invoke(); return new ErrorsInfo { Flag = Errors.Ok }; });
        }
        private static SystemSetupConfiguration Record(string category, string key, string json) => SystemSetupIdentity.Prepare(new(category, key, "1", json, "", Guid.NewGuid().ToString("N")), null);
        public async Task<CompanyGhgSelectionRequest> Request()
        { var review = await Reader.ReviewAsync(Company, "KW", Native.Authority); return new("KW", review.Catalog.Revision, review.EffectiveRevision, "Reviewed for this company", "", Guid.NewGuid().ToString("N")); }
    }

    private sealed class EnvironmentLayers(SystemSetupConfiguration baseline, SystemSetupConfiguration? country) : ISystemSetupRecordReader
    {
        public SystemSetupConfiguration Baseline = baseline;
        public SystemSetupConfiguration? Country = country;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { await authority(); return category == "TOOLS" ? Baseline : category == "COUNTRY" ? Country : null; }
    }

    [Theory]
    [InlineData("entity")]
    [InlineData("field")]
    [InlineData("value")]
    [InlineData("filter")]
    public void SourceProfileReviewIdentifiesInvalidMappings(string fault)
    {
        var source = new TheTechIdea.OilGas.Core.DataSourceDefinition { Kind = "beep", Path = "PPDM_CORE", Profile = "ppdm39" };
        source.Codes["string"] = new(StringComparer.OrdinalIgnoreCase) { ["kind"] = new(StringComparer.OrdinalIgnoreCase) { ["A"] = "Production" } };
        Assert.Empty(CompanyEnvironmentSources.ReviewMapping(source));
        if (fault == "entity") source.Codes["missing"] = new();
        else if (fault == "field") source.Codes["string"]["missing"] = new();
        else if (fault == "value") source.Codes["string"]["kind"]["A"] = "";
        else source.Where["missing"] = new();
        Assert.NotEmpty(CompanyEnvironmentSources.ReviewMapping(source));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DraftReviewShowsInvalidCompanyOrInheritedSourceMappings(bool inherited)
    {
        var invalid = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"unknown\":{\"A\":\"B\"}}}}}}";
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", inherited ? invalid : "{}", "", Guid.NewGuid().ToString("N")), null);
        var validSaved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(validSaved), new EnvironmentLayers(baseline, null));
        var draft = new CompanyEnvironmentPreviewRequest("KW", inherited ? "{}" : invalid);
        var review = await resolver.PreviewDraftAsync(Company, draft, () => Task.CompletedTask);
        Assert.Contains(review.Settings.Warnings, warning => warning.Contains("string.unknown"));
        if (!inherited)
            Assert.Throws<ArgumentException>(() => CompanyEnvironmentPreviewer.ValidateForSave(Company, new("KW", invalid, "", Guid.NewGuid().ToString("N"))));
        else
            Assert.NotEmpty(CompanyEnvironmentSources.ReviewEnvironment(CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", "{}", baseline, null)));
    }

    [Theory]
    [InlineData(false, "{\"units\":{\"forceUnit\":\"not-a-unit\"}}", "units.forceUnit")]
    [InlineData(true, "{\"locale\":{\"calendar\":\"invalid\"}}", "locale.calendar")]
    public async Task DraftReviewReportsToolValidationIssuesFromCompanyAndSharedLayers(bool inherited, string invalid, string expected)
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", inherited ? invalid : "{}", "", Guid.NewGuid().ToString("N")), null);
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, null));
        var review = await resolver.PreviewDraftAsync(Company, new("KW", inherited ? "{}" : invalid), () => Task.CompletedTask);
        Assert.Contains(review.Settings.Warnings, warning => warning.Contains(expected));
    }

    [Fact]
    public async Task InvalidSavedEnvironmentIsNotReportedCurrent()
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{\"units\":{\"forceUnit\":\"not-a-unit\"}}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, null));
        var review = await resolver.ResolveAsync(Company, () => Task.CompletedTask);
        Assert.Equal("ReviewRequired", review.ReviewStatus);
        Assert.Contains(review.Settings.Warnings, warning => warning.Contains("units.forceUnit"));
    }

    [Fact]
    public async Task EffectiveDraftReviewReturnsTheResolvedDisplayLocale()
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{\"locale\":{\"culture\":\"en-GB\",\"dateFormat\":\"dd/MM/yyyy\"}}", "", Guid.NewGuid().ToString("N")), null);
        var country = SystemSetupIdentity.Prepare(new("COUNTRY", "KW", "1", "{\"locale\":{\"dateFormat\":\"dd/MM/yyyy\"}}", "", Guid.NewGuid().ToString("N")), null);
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, country));
        var review = await resolver.PreviewDraftAsync(Company, new("KW", "{\"locale\":{\"culture\":\"ar-KW\",\"dateTimeFormat\":\"dd/MM/yyyy HH:mm\",\"calendar\":\"Gregorian\"}}"), () => Task.CompletedTask);
        Assert.Empty(review.Settings.Warnings); Assert.Equal("ar-KW", review.Settings.DisplayCulture);
        Assert.Equal("dd/MM/yyyy", review.Settings.DateFormat); Assert.Equal("dd/MM/yyyy HH:mm", review.Settings.DateTimeFormat);
        Assert.Equal("Gregorian", review.Settings.Calendar);
    }

    [Fact]
    public async Task SourceReviewUsesActualProfileAndKeepsBindingSeparateFromCoverage()
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var overrides = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}";
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", overrides, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, null));
        var bindings = new Mock<ICompanyEnvironmentBindingReader>(); bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A', 64));
        var result = await new CompanyEnvironmentSources(resolver, bindings.Object).ReviewAsync(Company, () => Task.CompletedTask, default);
        var item = Assert.Single(result.Sources); Assert.Equal("Bound", item.BindingStatus); Assert.Equal("ProfileChecked", item.MappingStatus); Assert.Empty(item.MappingIssues); Assert.False(result.DataTested);
    }

    [Fact]
    public async Task SourceReviewReturnsAllEffectiveMappingsFromCompanyAndSharedLayers()
    {
        var sharedCodes = Enumerable.Range(0, 205).ToDictionary(i => $"C{i:D3}", _ => "Tubing");
        sharedCodes.Add("Z/~", "Tubing");
        var baselineJson = JsonSerializer.Serialize(new { dataSources = new { wells = new { kind = "beep", path = "PPDM_CORE", profile = "ppdm39", codes = new { @string = new { kind = sharedCodes } } } } });
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", baselineJson, "", Guid.NewGuid().ToString("N")), null);
        var country = SystemSetupIdentity.Prepare(new("COUNTRY", "KW", "1", "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"C001\":\"Casing\"}}}}}}", "", Guid.NewGuid().ToString("N")), null);
        var overrides = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"C000\":\"Liner\"}}}}}}";
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", overrides, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, country.Revision);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, country));
        var bindings = new Mock<ICompanyEnvironmentBindingReader>(); bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A', 64));
        var result = await new CompanyEnvironmentSources(resolver, bindings.Object).ReviewAsync(Company, () => Task.CompletedTask, default);
        var item = Assert.Single(result.Sources);
        Assert.Equal(206, item.MappedCodeValues);
        Assert.Equal(206, item.CodeMappings.Length);
        Assert.Equal(new CompanyEnvironmentCodeMapping("string", "kind", "C000", "Liner", "Company"), item.CodeMappings[0]);
        Assert.Equal(new CompanyEnvironmentCodeMapping("string", "kind", "C001", "Casing", "Country"), item.CodeMappings[1]);
        Assert.Equal(new CompanyEnvironmentCodeMapping("string", "kind", "C002", "Tubing", "System"), item.CodeMappings[2]);
        Assert.Equal(new CompanyEnvironmentCodeMapping("string", "kind", "Z/~", "Tubing", "System"), item.CodeMappings[^1]);
        Assert.False(result.DataTested);
    }

    [Fact]
    public async Task SourceReviewRejectsEarlierBindingChangedDuringAnotherSourceRead()
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var overrides = "{\"dataSources\":{\"first\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"},\"second\":{\"kind\":\"beep\",\"path\":\"PPDM_OTHER\",\"profile\":\"ppdm39\"}}}";
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", overrides, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(baseline, null));
        var otherSeen = false; var bindings = new Mock<ICompanyEnvironmentBindingReader>();
        bindings.Setup(x => x.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string module, CancellationToken _) =>
            {
                if (module == "PPDM_OTHER") otherSeen = true;
                return new string(module == "PPDM_CORE" && otherSeen ? 'B' : 'A', 64);
            });
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentSources(resolver, bindings.Object)
            .ReviewAsync(Company, () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task ConcurrentCompaniesResolveTheirOwnTimeZonesWithoutGlobalLeakage()
    {
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        CompanyEnvironmentEffectiveResolver Resolver(CompanyScope company, string country, string zone)
        {
            var json = JsonSerializer.Serialize(new { locale = new { timeZone = zone } });
            var saved = CompanyEnvironmentIdentity.Prepare(company, new(country, json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, null);
            return new(new Companies(saved), new EnvironmentLayers(baseline, null));
        }
        var kuwait = new CompanyScope("tenant", "kuwait"); var norway = new CompanyScope("tenant", "norway");
        var left = Resolver(kuwait, "KW", "Asia/Kuwait"); var right = Resolver(norway, "NO", "Europe/Oslo");
        var results = await Task.WhenAll(Enumerable.Range(0, 8).Select(async i => i % 2 == 0 ? await left.ResolveAsync(kuwait, () => Task.CompletedTask) : await right.ResolveAsync(norway, () => Task.CompletedTask)));
        foreach (var item in results) Assert.Equal(item.Company == kuwait ? "Asia/Kuwait" : "Europe/Oslo", item.Settings.TimeZoneId);
    }

    [Theory]
    [InlineData("baseline")]
    [InlineData("country-added")]
    [InlineData("country-removed")]
    public async Task EffectiveEnvironmentReportsChangedReviewedLayers(string changed)
    {
        SystemSetupConfiguration Record(string category, string key, string json) => SystemSetupIdentity.Prepare(new(category, key, "1", json, "", Guid.NewGuid().ToString("N")), null);
        var baseline = Record("TOOLS", "DEFAULT", "{}"); var country = Record("COUNTRY", "KW", "{}");
        var reviewedCountry = changed == "country-removed" ? country : null;
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, baseline.Revision, reviewedCountry?.Revision);
        var layers = new EnvironmentLayers(baseline, reviewedCountry); var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), layers);
        Assert.Equal("Current", (await resolver.ResolveAsync(Company, () => Task.CompletedTask)).ReviewStatus);
        if (changed == "baseline") layers.Baseline = Record("TOOLS", "DEFAULT", "{}");
        else if (changed == "country-added") layers.Country = country;
        else layers.Country = null;
        var result = await resolver.ResolveAsync(Company, () => Task.CompletedTask);
        Assert.Equal("ReviewRequired", result.ReviewStatus); Assert.Equal(Company, result.Company);
    }

    [Fact]
    public async Task VerificationRefusesEnvironmentWhenSharedSettingsNeedCompanyReview()
    {
        var reviewed = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var changed = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "2", "{}", reviewed.Revision, Guid.NewGuid().ToString("N")), reviewed);
        var saved = CompanyEnvironmentIdentity.Prepare(Company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, reviewed.Revision, null);
        var resolver = new CompanyEnvironmentEffectiveResolver(new Companies(saved), new EnvironmentLayers(changed, null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CompanyEnvironmentRuntime(resolver).VerifyAsync(Company, () => Task.CompletedTask));
    }

    [Fact]
    public async Task SharedChangeDuringCatalogReviewDoesNotProduceReviewEvidence()
    {
        var fixture = new Fixture();
        fixture.Shared.ChangeOnCatalogRead = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "2", "{}", "", Guid.NewGuid().ToString("N")), null);
        await Assert.ThrowsAsync<CompanyEnvironmentReviewRequiredException>(() => fixture.Reader.ReviewAsync(Company, "KW", fixture.Native.Authority));
        Assert.Equal(0, fixture.Native.Native.Writes);
    }

    [Fact]
    public async Task CatalogSelectionRequiresTheCompanyEnvironmentReviewToRemainCurrent()
    {
        var fixture = new Fixture(); var request = await fixture.Request();
        var selection = await fixture.Store.SaveAsync(Company, "reviewer", request, fixture.Native.Authority);
        var selectionStore = new Mock<ICompanyGhgSelectionStore>();
        selectionStore.Setup(x => x.ReadAsync(Company, It.IsAny<Func<Task>>())).ReturnsAsync(selection);
        var status = new CompanyGhgSelectionStatusReader(selectionStore.Object, fixture.Reader);
        Assert.Equal("Current", (await status.ReadAsync(Company, fixture.Native.Authority)).Status);
        fixture.Shared.ReplacementBaseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "2", "{}", "", Guid.NewGuid().ToString("N")), null);
        await Assert.ThrowsAsync<CompanyEnvironmentReviewRequiredException>(() => fixture.Reader.ReviewAsync(Company, "KW", fixture.Native.Authority));
        Assert.Equal("ReviewRequired", (await status.ReadAsync(Company, fixture.Native.Authority)).Status);
        Assert.Equal(1, fixture.Native.Native.Writes);
    }

    [Fact]
    public async Task CompanySelectionCanBeSavedReadAndRetried()
    {
        var fixture = new Fixture(); var request = await fixture.Request();
        var saved = await fixture.Store.SaveAsync(Company, "reviewer", request, fixture.Native.Authority);
        Assert.Equal(saved, await fixture.Store.ReadAsync(Company, fixture.Native.Authority));
        fixture.Shared.Changed = true;
        Assert.Equal(saved, await fixture.Store.SaveAsync(Company, "reviewer", request, fixture.Native.Authority));
        Assert.Equal(1, fixture.Native.Native.Writes);
    }

    [Fact]
    public async Task ChangedCatalogBeforeOrDuringWriteRollsBack()
    {
        foreach (var changeDuringWrite in new[] { false, true })
        {
            var fixture = new Fixture(); var request = await fixture.Request();
            if (changeDuringWrite) fixture.OnWrite = () => fixture.Shared.Changed = true; else fixture.Shared.Changed = true;
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Store.SaveAsync(Company, "reviewer", request, fixture.Native.Authority));
            Assert.Empty(fixture.Native.Native.Rows);
            Assert.Equal(changeDuringWrite ? 1 : 0, fixture.Native.Native.Writes);
        }
    }
}
#endif
