using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using System.Text.Json;
using System.Data;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SystemSetupRecordStoreTests
{
    private sealed class Fixture
    {
        public readonly CompanyGLAccountReviewTests.Fixture Source = new();
        public readonly SystemSetupRecordStore Store;
        public Fixture()
        {
            Source.Native.Rows.Clear();
            Source.Native.Source.SetupGet(x => x.ConnectionStatus).Returns(ConnectionState.Open);
            var properties = new ConnectionProperties { ConnectionName = "lifecycle", DatabaseType = DataSourceType.SqlLite, ConnectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(Path.GetTempPath(), "system.db") }.ConnectionString };
            Mock.Get(Source.Native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(properties);
            var registry = new Mock<IConfigEditor>(); registry.SetupGet(x => x.DataConnections).Returns([properties]);
            Source.Native.Editor.SetupGet(x => x.ConfigEditor).Returns(registry.Object);
            Source.Native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure _, bool _) =>
            { var schema = new ClassCreator(Source.Native.Editor.Object).ConvertToEntityStructure(typeof(SYSTEM_SETUP_RECORD)); Source.ChangeSchema?.Invoke(schema); return schema; });
            var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("SYSTEM_SETUP_RECORD")).ReturnsAsync(["SYSTEM_SETUP_RECORD_ID"]);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = "lifecycle", ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(Source.Native.Editor.Object, properties) }).Build();
            Store = new(new SystemSetupTarget(configuration, Source.Native.Editor.Object), Source.Native.Editor.Object, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
            Source.Native.Source.Setup(x => x.InsertEntity("SYSTEM_SETUP_RECORD", It.IsAny<object>())).Returns((string _, object value) =>
            { Source.Native.Writes++; Source.Native.Rows.Add(Clone(value)); return new ErrorsInfo { Flag = Errors.Ok }; });
            Source.Native.Source.Setup(x => x.UpdateEntity("SYSTEM_SETUP_RECORD", It.IsAny<object>())).Returns((string _, object value) =>
            { Source.Native.Writes++; Source.Native.Rows.Clear(); Source.Native.Rows.Add(Clone(value)); return new ErrorsInfo { Flag = Errors.Ok }; });
        }
        private static SYSTEM_SETUP_RECORD Clone(object value) => JsonSerializer.Deserialize<SYSTEM_SETUP_RECORD>(JsonSerializer.Serialize(value))!;
        public Task<SystemSetupConfiguration?> Read() => Store.ReadAsync("PPDM", "DEFAULT", Source.Authority);
        public Task<SystemSetupConfiguration> Save(SystemSetupRequest request) => Store.SaveAsync(request, "admin", Source.Authority);
    }
    private static SystemSetupRequest Request(string expected = "") => new("PPDM", "DEFAULT", "1", "{\"schema\":\"PPDM39\"}", expected, Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task SharedRecordIsRevisionCheckedAndExactRetryIsIdempotent()
    {
        var fixture = new Fixture(); Assert.Null(await fixture.Read()); var request = Request();
        var first = await fixture.Save(request); Assert.Equal(first, await fixture.Read()); Assert.Equal(first, await fixture.Save(request)); Assert.Equal(1, fixture.Source.Native.Writes);
        var second = await fixture.Save(Request(first.Revision) with { Version = "2" }); Assert.Equal(first.Revision, second.PreviousRevision);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Save(Request(first.Revision))); Assert.Equal(2, fixture.Source.Native.Writes);
        Assert.Equal(second, await fixture.Read());
    }

    [Fact]
    public async Task SecretFieldsAndChangedStoredDataAreRefused()
    {
        var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save(Request() with { SettingsJson = "{\"password\":\"secret\"}" }));
        Assert.Equal(0, fixture.Source.Native.Writes); await fixture.Save(Request());
        Assert.IsType<SYSTEM_SETUP_RECORD>(Assert.Single(fixture.Source.Native.Rows)).SETTINGS_JSON = "{}";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Read());
    }

    [Fact]
    public async Task GhgFactorCatalogRequiresExplicitJurisdictionSourcesAndNonOverlappingPeriods()
    {
        var fixture = new Fixture();
        var factor = new GhgFactorDefinition("factor-1", "flare", "volume-unit", "mass-unit", 1.25m,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "reviewed-test-source");
        var catalog = new GhgFactorCatalog("KW", [factor]);
        var request = Request() with { Category = "GHG_FACTORS", Key = "KW", SettingsJson = JsonSerializer.Serialize(catalog) };
        var saved = await fixture.Save(request);
        Assert.Equal(request.SettingsJson, saved.SettingsJson);
        var next = request with { ExpectedRevision = saved.Revision, IntentId = Guid.NewGuid().ToString("N") };
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save(next with { SettingsJson = JsonSerializer.Serialize(catalog with { Jurisdiction = "NO" }) }));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save(next with { SettingsJson = JsonSerializer.Serialize(catalog with { Factors = [factor, factor with { Id = "factor-2" }] }) }));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save(next with { SettingsJson = JsonSerializer.Serialize(catalog with { Factors = [factor with { SourceReference = "" }] }) }));
        await Assert.ThrowsAsync<JsonException>(() => fixture.Save(next with { SettingsJson = request.SettingsJson.Replace("\"Factor\":1.25,", "") }));
        Assert.Equal(1, fixture.Source.Native.Writes);
        var zero = await fixture.Save(next with { SettingsJson = JsonSerializer.Serialize(catalog with { Factors = [factor with { Factor = 0m }] }) });
        Assert.NotEqual(saved.Revision, zero.Revision);
    }

    [Fact]
    public async Task SharedRecordCanBeReadOnlyInsideTheSameOwnedSerializableTransaction()
    {
        var fixture = new Fixture();
        var saved = await fixture.Save(Request());
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Store.ReadOwnedAsync("PPDM", "DEFAULT", fixture.Source.Native.Source.Object, fixture.Source.Authority));
        SystemSetupConfiguration? reviewed = null;
        await PPDMDataSourceTransaction.ExecuteAsync(fixture.Source.Native.Source.Object, async () =>
        {
            reviewed = await fixture.Store.ReadOwnedAsync("PPDM", "DEFAULT", fixture.Source.Native.Source.Object, fixture.Source.Authority);
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Store.ReadOwnedAsync("PPDM", "DEFAULT", new Mock<TheTechIdea.Beep.IDataSource>().Object, fixture.Source.Authority));
        });
        Assert.Equal(saved, reviewed);
    }

    [Fact]
    public async Task ReviewedDraftRejectsChangedSharedRevisionsAndForeignCompanyInsideTransaction()
    {
        var fixture = new Fixture();
        var baseline = await fixture.Save(Request() with { Category = "TOOLS", SettingsJson = "{}" });
        var company = new CompanyScope("tenant", "company");
        var draft = new CompanyEnvironmentPreviewRequest("KW", "{}");
        var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
        var version = CompanyEnvironmentPreviewer.ToolsVersion;
        var review = new CompanyEnvironmentDraftReview(settings, baseline.Revision, null, version,
            CompanyEnvironmentDraftIdentity.ForReview(company, draft, baseline.Revision, null, version));
        async Task Check(CompanyScope scope, CompanyEnvironmentDraftReview item) => await PPDMDataSourceTransaction.ExecuteAsync(fixture.Source.Native.Source.Object,
            () => fixture.Store.RequireReviewedDraftAsync(scope, draft, item, fixture.Source.Native.Source.Object, fixture.Source.Authority));
        await Check(company, review);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Check(new("tenant", "other"), review));
        var changed = await fixture.Save(Request(baseline.Revision) with { Category = "TOOLS", Version = "2", SettingsJson = "{}" });
        Assert.NotEqual(baseline.Revision, changed.Revision);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Check(company, review));
    }

    [Fact]
    public async Task OwnedDraftGuardRejectsInvalidInheritedTimeZone()
    {
        var fixture = new Fixture(); var baseline = await fixture.Save(Request() with { Category = "TOOLS", SettingsJson = "{\"locale\":{\"timeZone\":\"not/a-real-zone\"}}" });
        var company = new CompanyScope("tenant", "company"); var draft = new CompanyEnvironmentPreviewRequest("KW", "{}");
        var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
        var version = CompanyEnvironmentPreviewer.ToolsVersion;
        var review = new CompanyEnvironmentDraftReview(settings, baseline.Revision, null, version, CompanyEnvironmentDraftIdentity.ForReview(company, draft, baseline.Revision, null, version));
        await Assert.ThrowsAsync<ArgumentException>(() => PPDMDataSourceTransaction.ExecuteAsync(fixture.Source.Native.Source.Object,
            () => fixture.Store.RequireReviewedDraftAsync(company, draft, review, fixture.Source.Native.Source.Object, fixture.Source.Authority)));
    }

    [Fact]
    public async Task OwnedDraftGuardRefusesInvalidInheritedMappingsEvenWhenClientClaimsNoWarnings()
    {
        var fixture = new Fixture();
        var invalid = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"unknown\":{\"A\":\"B\"}}}}}}";
        var baseline = await fixture.Save(Request() with { Category = "TOOLS", SettingsJson = invalid });
        var company = new CompanyScope("tenant", "company"); var draft = new CompanyEnvironmentPreviewRequest("KW", "{}");
        var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
        var version = CompanyEnvironmentPreviewer.ToolsVersion;
        var review = new CompanyEnvironmentDraftReview(settings, baseline.Revision, null, version, CompanyEnvironmentDraftIdentity.ForReview(company, draft, baseline.Revision, null, version));
        await Assert.ThrowsAsync<InvalidOperationException>(() => PPDMDataSourceTransaction.ExecuteAsync(fixture.Source.Native.Source.Object,
            () => fixture.Store.RequireReviewedDraftAsync(company, draft, review, fixture.Source.Native.Source.Object, fixture.Source.Authority)));
    }

    [Fact]
    public async Task OwnedDraftGuardRefusesInvalidToolSettingsEvenWhenClientClaimsNoWarnings()
    {
        var fixture = new Fixture();
        var baseline = await fixture.Save(Request() with { Category = "TOOLS", SettingsJson = "{\"units\":{\"forceUnit\":\"not-a-unit\"}}" });
        var company = new CompanyScope("tenant", "company"); var draft = new CompanyEnvironmentPreviewRequest("KW", "{}");
        var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
        var version = CompanyEnvironmentPreviewer.ToolsVersion;
        var review = new CompanyEnvironmentDraftReview(settings, baseline.Revision, null, version, CompanyEnvironmentDraftIdentity.ForReview(company, draft, baseline.Revision, null, version));
        await Assert.ThrowsAsync<InvalidOperationException>(() => PPDMDataSourceTransaction.ExecuteAsync(fixture.Source.Native.Source.Object,
            () => fixture.Store.RequireReviewedDraftAsync(company, draft, review, fixture.Source.Native.Source.Object, fixture.Source.Authority)));
    }

    [Fact]
    public async Task ToolDefaultsAcceptBeepModuleSelectorsAndRejectFileSelectors()
    {
        var fixture = new Fixture();
        var request = Request() with { Category = "TOOLS", SettingsJson = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}" };
        var saved = await fixture.Save(request);
        Assert.Equal(request.SettingsJson, saved.SettingsJson);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save(request with { SettingsJson = request.SettingsJson.Replace("PPDM_CORE", "C:/private"), ExpectedRevision = saved.Revision, IntentId = Guid.NewGuid().ToString("N") }));
        Assert.Equal(1, fixture.Source.Native.Writes);
    }

    [Fact]
    public async Task ActiveUsersCanReadButOnlyCurrentAdministratorsCanWrite()
    {
        var fixture = new Fixture(); await fixture.Save(Request()); var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.IsAccountActiveAsync("reader", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "reader")], "test"));
        var controller = new SystemSetupRecordController(fixture.Store, access.Object, NullLogger<SystemSetupRecordController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var read = Assert.IsType<SystemSetupReadResult>(Assert.IsType<OkObjectResult>(await controller.Read("PPDM", "DEFAULT", default)).Value);
        Assert.NotNull(read.Configuration); Assert.IsType<ForbidResult>(await controller.Save("PPDM", "DEFAULT", Request(), default));
        access.Setup(x => x.IsAccountActiveAsync("reader", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Assert.IsType<ForbidResult>(await controller.Read("PPDM", "DEFAULT", default));
    }

    [Fact]
    public async Task ToolDefaultDraftRequiresCurrentAdministratorThroughoutReview()
    {
        var fixture = new Fixture(); var access = new Mock<IRepositoryAccessService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "admin"), new(ClaimTypes.Role, "Administrator")], "test"));
        var controller = new SystemSetupRecordController(fixture.Store, access.Object, NullLogger<SystemSetupRecordController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("admin", true, [], []));
        Assert.IsType<ForbidResult>(await controller.ToolsDefaultDraft(default));
        var calls = 0;
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            new RepositoryUserAccess("admin", true, ++calls == 1 ? ["Administrator"] : [], []));
        Assert.IsType<ForbidResult>(await controller.ToolsDefaultDraft(default));
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("admin", true, ["Administrator"], []));
        Assert.IsType<SystemSetupToolsDefaultDraft>(Assert.IsType<OkObjectResult>(await controller.ToolsDefaultDraft(default)).Value);
        Assert.Equal(0, fixture.Source.Native.Writes);
    }
}
#endif
