#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
using Beep.OilandGas.PermitsAndApplications.Modules;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PermitsAndApplications.Forms;
using Beep.OilandGas.PermitsAndApplications.Validation;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

/// <summary>Plan 354 physical schema gate. Does not qualify API/OIDC or company authorization.</summary>
public sealed partial class PermitProviderQualificationTests
{
    [Fact]
    public Task SetupInstalledSqliteMigratesPermitApplicationLedger() => WithInstalledLedgerAsync((_, _) => Task.CompletedTask);

    [Theory]
    [InlineData("drilling", "ar-KW")]
    [InlineData("drilling", "fr-FR")]
    [InlineData("environmental", "ar-KW")]
    [InlineData("environmental", "fr-FR")]
    [InlineData("injection", "ar-KW")]
    [InlineData("injection", "fr-FR")]
    public void PermitSupportingProjectionUsesExactParentAcrossCultures(string kind, string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var parent = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "owned-parent", FIELD_ID = "owned-field", WELL_ID = "owned-well",
                COUNTRY = Country.Canada, STATE_PROVINCE = StateProvince.Alberta, REGULATORY_AUTHORITY = RegulatoryAuthority.AER,
                STATUS = PermitApplicationStatus.UnderReview, EFFECTIVE_DATE = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                FEES_PAID = true, SUBMISSION_COMPLETE = true, REMARKS = "Parent note" };
            DRILLING_PERMIT_APPLICATION? drilling = kind == "drilling" ? new() { PERMIT_APPLICATION_ID = parent.PERMIT_APPLICATION_ID, PROPOSED_DEPTH = 1234.5m, REMARKS = "Supporting note" } : null;
            ENVIRONMENTAL_PERMIT_APPLICATION? environmental = kind == "environmental" ? new() { PERMIT_APPLICATION_ID = parent.PERMIT_APPLICATION_ID, WASTE_VOLUME = 1234.5m, REMARKS = "Supporting note" } : null;
            INJECTION_PERMIT_APPLICATION? injection = kind == "injection" ? new() { PERMIT_APPLICATION_ID = parent.PERMIT_APPLICATION_ID, MAXIMUM_INJECTION_PRESSURE = 1234.5m, REMARKS = "Supporting note" } : null;
            parent.APPLICATION_TYPE = kind switch { "drilling" => PermitApplicationType.Drilling, "environmental" => PermitApplicationType.Environmental, "injection" => PermitApplicationType.Injection, _ => throw new ArgumentException(nameof(kind)) };
            var child = (ModelEntityBase?)drilling ?? environmental ?? (ModelEntityBase)injection!;
            child.EFFECTIVE_DATE = new DateTime(2040, 1, 1, 0, 0, 0, DateTimeKind.Utc); // Row validity cannot replace filing dates.
            var numeric = kind switch { "drilling" => "PROPOSED_DEPTH", "environmental" => "WASTE_VOLUME", _ => "MAXIMUM_INJECTION_PRESSURE" };
            var fields = new[] { "COUNTRY", "STATE_PROVINCE", "REGULATORY_AUTHORITY", "STATUS", "EFFECTIVE_DATE", "RECEIVED_DATE", "WELL_UWI", "INJECTION_WELL_UWI", "FEES_PAID_IND", "SUBMISSION_COMPLETE_IND", "REMARKS", numeric };
            var context = new PermitFormRenderContext(parent, drilling, environmental, injection);
            var request = new PermitValidationRequest(parent, [], [], null, drilling, environmental, injection);
            var renderer = new PermitFormRenderer();
            var template = new PermitFormTemplate("RECORDED", "Recorded source", "AER", parent.APPLICATION_TYPE.ToString(), fields);
            var rendered = renderer.Render(context, template);
            Assert.Equal("Canada", rendered["COUNTRY"]); Assert.Equal("Alberta", rendered["STATE_PROVINCE"]); Assert.Equal("AER", rendered["REGULATORY_AUTHORITY"]);
            Assert.Equal("UnderReview", rendered["STATUS"]);
            Assert.Equal("2026-10-01T00:00:00.0000000Z", rendered["EFFECTIVE_DATE"]);
            Assert.Equal("", rendered["RECEIVED_DATE"]);
            Assert.Equal("owned-well", rendered["WELL_UWI"]); Assert.Equal("owned-well", rendered["INJECTION_WELL_UWI"]);
            Assert.Equal("Y", rendered["FEES_PAID_IND"]); Assert.Equal("Y", rendered["SUBMISSION_COMPLETE_IND"]);
            Assert.Equal("Parent note", rendered["REMARKS"]); Assert.Equal("1234.5", rendered[numeric]);
            Assert.Equal(parent.APPLICATION_TYPE, request.ApplicationType); Assert.Equal(RegulatoryAuthority.AER, request.RegulatoryAuthority);
            Assert.True(PermitFieldValueResolver.TryResolve(request, numeric, out var number)); Assert.Equal("1234.5", number);
            Assert.True(PermitFieldValueResolver.TryResolve(request, "WELL_UWI", out var well)); Assert.Equal("owned-well", well);
            if (drilling is not null) drilling.PERMIT_APPLICATION_ID = "foreign";
            if (environmental is not null) environmental.PERMIT_APPLICATION_ID = "foreign";
            if (injection is not null) injection.PERMIT_APPLICATION_ID = "foreign";
            Assert.Throws<InvalidOperationException>(() => new PermitFormRenderContext(parent, drilling, environmental, injection));
            Assert.Throws<InvalidOperationException>(() => new PermitValidationRequest(parent, [], [], null, drilling, environmental, injection));
            Assert.Throws<InvalidOperationException>(() => renderer.Render(context, template));
            Assert.Throws<InvalidOperationException>(() => PermitFieldValueResolver.TryResolve(request, "COUNTRY", out _));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public Task SetupInstalledSqliteMigratesEntirePermitsManifest() => WithInstalledProviderAsync(async (editor, source) =>
    {
        var types = PermitsModule.OwnedEntities;
        Assert.Equal(12, types.Count);
        Assert.Equal(types.Count, types.Distinct().Count());
        Assert.All(types, type => Assert.Equal(typeof(PERMIT_APPLICATION).Assembly, type.Assembly));
        var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, source.DatasourceName) };
        var plan = migration.BuildMigrationPlanForTypes(types, detectRelationships: true);
        Assert.Equal(types.Count, plan.Operations.Count);
        Assert.All(plan.Operations, operation => Assert.Equal(MigrationPlanOperationKind.CreateEntity, operation.Kind));
        migration.ApproveMigrationPlan(plan, "full-permits-provider-qualification");
        var applied = migration.ExecuteMigrationPlan(plan);
        Assert.True(applied.Success, applied.Message + " | " + source.ErrorObject.Message);
        foreach (var type in types)
        {
            var shape = source.GetEntityStructure(type.Name, true);
            Assert.NotNull(shape);
            var expectedKeys = type.Name switch
            {
                "DRILLING_PERMIT_APPLICATION" or "ENVIRONMENTAL_PERMIT_APPLICATION" or "INJECTION_PERMIT_APPLICATION" => new[] { "PERMIT_APPLICATION_ID" },
                "R_PERMITS_REFERENCE_CODE" => new[] { "REFERENCE_CODE", "REFERENCE_SET" },
                _ => new[] { type.Name + "_ID" }
            };
            Assert.Equal(expectedKeys, shape.Fields.Where(field => field.IsKey).Select(field => field.FieldName).Order(StringComparer.Ordinal));
            if (type.Name is "DRILLING_PERMIT_APPLICATION" or "ENVIRONMENTAL_PERMIT_APPLICATION" or "INJECTION_PERMIT_APPLICATION")
                Assert.DoesNotContain(shape.Fields, field => new[] { "STATUS", "COUNTRY", "STATE_PROVINCE", "REGULATORY_AUTHORITY", "APPLICATION_TYPE", "FEES_PAID", "SUBMITTED_DATE", "SUBMISSION_COMPLETE" }.Contains(field.FieldName, StringComparer.Ordinal));
            var canonical = editor.classCreator.ConvertToEntityStructure(type);
            Assert.Equal(canonical.Fields.Count, canonical.Fields.Select(field => field.FieldName).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(canonical.Fields.Select(field => field.FieldName).Order(StringComparer.Ordinal), shape.Fields.Select(field => field.FieldName).Order(StringComparer.Ordinal));
        }
        var repeat = migration.BuildMigrationPlanForTypes(types, detectRelationships: true);
        Assert.Equal(types.Count, repeat.Operations.Count);
        Assert.All(repeat.Operations, operation => Assert.Equal(MigrationPlanOperationKind.UpToDate, operation.Kind));
        // The same code in two reference sets is valid; the exact tuple is unique.
        foreach (var set in new[] { "set-a", "set-b" })
            Assert.Equal(Errors.Ok, source.InsertEntity(nameof(R_PERMITS_REFERENCE_CODE), new R_PERMITS_REFERENCE_CODE { REFERENCE_SET = set, REFERENCE_CODE = "SAME", LONG_NAME = set, ACTIVE_IND = "Y" }).Flag);
        Assert.Equal(Errors.Failed, source.InsertEntity(nameof(R_PERMITS_REFERENCE_CODE), new R_PERMITS_REFERENCE_CODE { REFERENCE_SET = "set-a", REFERENCE_CODE = "SAME", LONG_NAME = "Duplicate", ACTIVE_IND = "Y" }).Flag);
        var rows = await source.GetEntityAsync(nameof(R_PERMITS_REFERENCE_CODE), []);
        var count = rows is DataTable table ? table.Rows.Count : ((System.Collections.IEnumerable)rows).Cast<object>().Count();
        Assert.Equal(2, count);
    });

    [Theory]
    [InlineData("valid")]
    [InlineData("insert-failure")]
    [InlineData("cancel")]
    [InlineData("label-drift")]
    [InlineData("inactive")]
    [InlineData("target-drift")]
    public Task InstalledPermitSetupSeedsAtomicallyAndVerifiesExistingCatalog(string mode) => WithInstalledProviderAsync(async (editor, source) =>
    {
        var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, source.DatasourceName) };
        var plan = migration.BuildMigrationPlanForTypes(PermitsModule.OwnedEntities, detectRelationships: true);
        migration.ApproveMigrationPlan(plan, "permit-seed-qualification");
        Assert.True(migration.ExecuteMigrationPlan(plan).Success);
        var actual = new CommonColumnHandler();
        var columns = new Mock<ICommonColumnHandler>();
        var defaults = Mock.Of<IPPDM39DefaultsRepository>();
        var metadata = Mock.Of<IPPDMMetadataRepository>();
        using var cancel = new CancellationTokenSource();
        var attempts = 0;
        var inject = true;
        var connection = source.Dataconnection.ConnectionProp;
        var originalGuid = connection.GuidID;
        columns.Setup(handler => handler.PrepareForInsert(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) =>
        {
            actual.PrepareForInsert(row, actor);
            if (++attempts != 3 || !inject) return;
            if (mode == "insert-failure") throw new InvalidOperationException("Controlled third seed preparation failure after two native writes.");
            if (mode == "cancel") cancel.Cancel();
            if (mode == "target-drift") connection.GuidID = Guid.NewGuid().ToString();
        });
        var expected = PermitsReferenceCodeSeed.GetAllSeedRows().ToArray();
        if (mode is "label-drift" or "inactive" or "valid")
        {
            var first = expected[0];
            var row = new R_PERMITS_REFERENCE_CODE { REFERENCE_SET = mode == "valid" ? "CUSTOM" : first.ReferenceSet,
                REFERENCE_CODE = mode == "valid" ? "UNRELATED" : first.ReferenceCode,
                LONG_NAME = mode == "label-drift" ? "Changed managed label" : mode == "valid" ? "Retain custom reference" : first.LongName,
                PPDM_GUID = Guid.NewGuid().ToString(), ACTIVE_IND = "Y" };
            actual.PrepareForInsert(row, "existing-admin");
            if (mode == "inactive") row.ACTIVE_IND = "N";
            Assert.Equal(Errors.Ok, source.InsertEntity(nameof(R_PERMITS_REFERENCE_CODE), row).Flag);
        }
        var reader = new PPDMGenericRepository(editor, actual, defaults, metadata, typeof(R_PERMITS_REFERENCE_CODE), source.DatasourceName, nameof(R_PERMITS_REFERENCE_CODE));
        async Task<R_PERMITS_REFERENCE_CODE[]> Read() => (await reader.GetAsync([])).Cast<R_PERMITS_REFERENCE_CODE>().OrderBy(row => row.REFERENCE_SET, StringComparer.Ordinal).ThenBy(row => row.REFERENCE_CODE, StringComparer.Ordinal).ToArray();
        static string Snapshot(R_PERMITS_REFERENCE_CODE[] rows) => JsonSerializer.Serialize(rows.Select(row => new { row.REFERENCE_SET, row.REFERENCE_CODE, row.LONG_NAME, row.ACTIVE_IND, row.PPDM_GUID, row.ROW_CREATED_BY, row.ROW_CHANGED_BY, row.ROW_CREATED_DATE, row.ROW_CHANGED_DATE }));
        var before = Snapshot(await Read());
        var module = new PermitsModule(new ModuleSetupContext { Editor = editor, CommonColumnHandler = columns.Object, Defaults = defaults,
            Metadata = metadata, ConnectionName = source.DatasourceName, Logger = NullLogger<PermitsModule>.Instance });
        var orchestrator = new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance);
        if (mode == "cancel")
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => orchestrator.RunSeedAsync(source.DatasourceName, "setup-account", cancel.Token));
        else
        {
            var seeded = await orchestrator.RunSeedAsync(source.DatasourceName, "setup-account");
            Assert.Equal(mode == "valid", seeded.AllSucceeded);
            Assert.Equal(mode == "valid" ? expected.Length : 0, seeded.TotalRecordsInserted);
            if (mode == "valid") Assert.Equal(1, Assert.Single(seeded.ModuleResults).TablesSeeded);
            else Assert.NotEmpty(Assert.Single(seeded.ModuleResults).Errors);
        }
        PPDMDataSourceTransaction.RequireIdle(source);
        connection.GuidID = originalGuid;
        if (mode != "valid") Assert.Equal(before, Snapshot(await Read()));
        if (mode is "label-drift" or "inactive") { Assert.Equal(0, attempts); return; }
        if (mode != "valid")
        {
            Assert.Equal(3, attempts);
            inject = false;
            var retry = await orchestrator.RunSeedAsync(source.DatasourceName, "setup-account");
            Assert.True(retry.AllSucceeded);
            Assert.Equal(expected.Length, retry.TotalRecordsInserted);
        }
        var confirmed = Snapshot(await Read());
        var repeat = await orchestrator.RunSeedAsync(source.DatasourceName, "setup-account");
        Assert.True(repeat.AllSucceeded);
        Assert.Equal(0, repeat.TotalRecordsInserted);
        Assert.Equal(0, Assert.Single(repeat.ModuleResults).TablesSeeded);
        Assert.NotNull(Assert.Single(repeat.ModuleResults).SkipReason);
        Assert.Equal(confirmed, Snapshot(await Read()));
        Assert.Equal(expected.Length + (mode == "valid" ? 1 : 0), (await Read()).Length);
        PPDMDataSourceTransaction.RequireIdle(source);
    });

    internal Task WithInstalledLedgerAsync(Func<IDMEEditor, IDataSource, Task> qualify, string? retainedDirectory = null, bool installSchema = true) =>
        WithInstalledProviderAsync(async (editor, source) =>
        {
            if (installSchema) InstallLedgerSchema(editor, source);
            await qualify(editor, source);
        }, retainedDirectory, requireExisting: !installSchema);

    private async Task WithInstalledProviderAsync(Func<IDMEEditor, IDataSource, Task> qualify, string? retainedDirectory = null, bool requireExisting = false, string connectionName = "permit-provider")
    {
        var directory = retainedDirectory ?? Path.Combine(Path.GetTempPath(), "oilgas-permit-provider-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "permits.db");
        if (requireExisting) Assert.True(File.Exists(path), "Recovery must open an existing physical permit database.");
        var config = new Mock<IConfigEditor>();
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var classes = new List<AssemblyClassDefinition>();
        var connection = new ConnectionProperties { ConnectionName = connectionName, DatabaseType = DataSourceType.SqlLite,
            ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        var storage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(directory, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(storage);
        var log = new Mock<IDMLogger>();
        var messages = new List<string>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo();
        var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler);
        util.DME = editor;
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
            ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "TheTechIdea.Beep.SqliteDatasourceCore",
            ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.59",
            ["SetupDrivers:Providers:SQLiteDataSource:Package"] = "Stub.System.Data.SQLite.Core.NetStandard",
            ["SetupDrivers:Providers:SQLiteDataSource:Version"] = "1.0.119"
        });
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_PERMIT_DRIVER_FEED");
        if (!string.IsNullOrWhiteSpace(feed))
            settings.AddInMemoryCollection(new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = feed });
        var configuration = settings.Build();
        IDataSource? source = null;
        try
        {
            // Use the wizard's package installer, without manually scanning a connector assembly.
            var installer = new SetupDriverService(editor, configuration, NullLogger<SetupDriverService>.Instance);
            var authorityChecks = 0;
            var reviewed = Assert.Single(installer.Catalog());
            var installed = await installer.InstallAsync(driver.GuidID, reviewed, () => { authorityChecks++; return Task.CompletedTask; }, default);
            Assert.True(installed.ConnectorLoaded && installed.ProviderLoaded && installed.Loaded);
            Assert.True(authorityChecks >= 2);
            using (var physical = (IDbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!)
            { physical.ConnectionString = connection.ConnectionString; physical.Open(); }
            source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source);
            Assert.Equal(ConnectionState.Open, source.Openconnection());
            await qualify(editor, source);
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Permit provider gate failed: " + string.Join(" | ", messages.TakeLast(12)), error);
        }
        finally
        {
            source?.Closeconnection();
            // Keep this isolated database/checkpoint directory as diagnostic evidence.
        }
    }

    private static void InstallLedgerSchema(IDMEEditor editor, IDataSource source, bool supporting = true, bool attachments = true)
    {
        var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, source.DatasourceName) };
        Type[] types = [typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND), typeof(DRILLING_PERMIT_APPLICATION), typeof(ENVIRONMENTAL_PERMIT_APPLICATION), typeof(INJECTION_PERMIT_APPLICATION), typeof(APPLICATION_ATTACHMENT)];
        if(!supporting)types=types.Take(3).ToArray();
        if(!attachments)types=types.Where(type=>type!=typeof(APPLICATION_ATTACHMENT)).ToArray();
        var plan = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
        Assert.Equal(types.Length, plan.Operations.Count(x => x.Kind == MigrationPlanOperationKind.CreateEntity));
        migration.ApproveMigrationPlan(plan, "permit-provider-qualification");
        var applied = migration.ExecuteMigrationPlan(plan);
        Assert.True(applied.Success, applied.Message + " | " + source.ErrorObject.Message);
        foreach (var type in types)
        {
            Assert.True(source.CheckEntityExist(type.Name));
            var shape = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
            Assert.NotNull(shape);
            var key = Assert.Single(shape.Fields.Where(field => field.IsKey));
            Assert.Equal(type.Name.EndsWith("_PERMIT_APPLICATION",StringComparison.Ordinal) ? "PERMIT_APPLICATION_ID" : type.Name + "_ID", key.FieldName, ignoreCase: true);
            foreach (var name in new[] { "OWNER_TENANT_ID", "OWNER_COMPANY_ID", "FIELD_ID" }.Where(name => type == typeof(PERMIT_APPLICATION) || type == typeof(PERMIT_APPLICATION_COMMAND)))
                Assert.Single(shape.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        var header = source.GetEntityStructure("PERMIT_APPLICATION", true);
        foreach (var name in new[] { "STATUS", "SUBMITTED_BY_USER_ID", "FACILITY_TYPE" })
            Assert.Single(header.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
        var replay = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
        // Beep retains one explicit UpToDate operation per unchanged entity.
        Assert.Equal(types.Length, replay.Operations.Count);
        Assert.All(replay.Operations, operation => Assert.Equal(MigrationPlanOperationKind.UpToDate, operation.Kind));
    }

    [Fact]
    public Task InstalledPermitProviderRollsBackThenRecoversExactNativeSubmission() => WithInstalledLedgerAsync(async (editor, source) =>
    {
        var rowType = source.GetEntityType(nameof(PERMIT_APPLICATION));
        var underscoreType = DMTypeBuilder.GetOrCreateType(editor, "Beep.permit_provider", null, nameof(PERMIT_APPLICATION), source.GetEntityStructure(nameof(PERMIT_APPLICATION), true).Fields);
        Assert.NotEqual(rowType, underscoreType);
        var fixture = CreateNativeFixture(editor, source);
        fixture.InterruptReceipt = true;
        var service = fixture.Service;
        var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "physical-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
            FIELD_ID = "field-a", APPLICANT_ID = "Recorded applicant", ACTIVE_IND = "Y" };
        new CommonColumnHandler().PrepareForInsert(seed, "author");
        Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
        var review = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        var original = new PermitApplicationCommand(Guid.NewGuid(), review, "SUBMIT", "Reviewed the recorded filing before submission");
        Assert.Null(await service.RecoverCommandAsync(original));
        var interruptedCommand = await Assert.ThrowsAnyAsync<Exception>(() => service.RecordCommandAsync(original));
        Assert.True(fixture.Interrupted, "The failure must occur after the header and history writes, at receipt preparation. Actual: " + interruptedCommand);
        PPDMDataSourceTransaction.RequireIdle(source);
        var rolledBack = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Equal(review, rolledBack);
        Assert.Null(await service.RecoverCommandAsync(original));
        fixture.InterruptReceipt = false;
        var saved = await service.RecordCommandAsync(original);
        saved.Require();
        Assert.Equal(original, saved.Original);
        Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, saved.ReadApplication().STATUS);
        fixture.AllowWrites = false;
        Assert.Equal(saved, await service.RecoverCommandAsync(original));
        Assert.Equal(saved, await service.RecordCommandAsync(original));
        var current = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Single(current.ReadHistory());
        Assert.Single(await fixture.ReceiptRepository.GetAsync([]));
        PPDMDataSourceTransaction.RequireIdle(source);
    });

    internal static PermitApplicationCommand SupportingOriginal(PermitApplicationSourceReview review,string kind,bool second=false)
    {
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"AMEND_"+kind.ToUpperInvariant(),"Reviewed explicit supporting details and paired units");
        return kind=="environmental"?original with { EnvironmentalChanges=new("Waste handling","Produced water",second?2000.25m:123.5m,"m3","Recorded location",
            new string('m',2048),"Recorded impact","Mitigation","Treatment","Water source",null,second?"Amended notes":"Recorded notes") }:
            original with { InjectionChanges=new("Disposal","Recorded zone","Water",second?2600.5m:2500.5m,"kPa",100.25m,"m3/day",new string('m',2048),null,"N","N",second?"Amended notes":"Recorded notes") };
    }
    [Theory]
    [InlineData("environmental","create-update-recovery")][InlineData("environmental","rollback")][InlineData("environmental","stale-support")]
    [InlineData("injection","create-update-recovery")][InlineData("injection","rollback")][InlineData("injection","stale-support")]
    public Task InstalledPermitSupportingAmendmentUsesReviewedOwnedLedger(string kind,string mode)=>WithInstalledLedgerAsync(async (editor,source)=>
    {
        var fixture=CreateNativeFixture(editor,source);var service=fixture.Service;
        var seed=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="supporting-amendment",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",WELL_ID="well-a",
            APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,APPLICATION_TYPE=kind=="environmental"?PermitApplicationType.Environmental:PermitApplicationType.Injection,STATUS=PermitApplicationStatus.Draft,ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(seed,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(PERMIT_APPLICATION),seed).Flag);
        var review=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);var original=SupportingOriginal(review,kind);original.Require();
        var expected=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);
        if(mode=="stale-support")
        {
            var late=PermitSupportingAmendments.Read(expected.Supporting,original.Action)!;new CommonColumnHandler().PrepareForInsert(late,"another-author");
            Assert.Equal(Errors.Ok,source.InsertEntity(late.GetType().Name,late).Flag);
            var changed=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.NotEqual(review.Revision,changed.Revision);
            await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(original));Assert.Null(await service.RecoverCommandAsync(original));
            Assert.Empty(changed.ReadHistory());Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));return;
        }
        if(mode=="rollback")
        {
            fixture.InterruptReceipt=true;await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(original));Assert.True(fixture.Interrupted);
            Assert.Equal(review,await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));Assert.Null(await service.RecoverCommandAsync(original));fixture.InterruptReceipt=false;
        }
        var saved=await service.RecordCommandAsync(original);saved.Require();Assert.Equal(original,saved.Original);
        Assert.Equal(PermitApplicationStatus.Draft,saved.ReadApplication().STATUS);Assert.Equal("well-a",saved.ReadApplication().WELL_ID);
        var child=PermitSupportingAmendments.Read(saved.Supporting,original.Action)!;
        if(kind=="environmental")Assert.Equal(original.EnvironmentalChanges,PermitEnvironmentalDraftChanges.From((ENVIRONMENTAL_PERMIT_APPLICATION)child));
        else Assert.Equal(original.InjectionChanges,PermitInjectionDraftChanges.From((INJECTION_PERMIT_APPLICATION)child));
        var current=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.Single(current.ReadHistory());Assert.Single(await fixture.ReceiptRepository.GetAsync([]));
        Assert.Throws<InvalidOperationException>(()=>(original with { IntentId=Guid.NewGuid(),Review=current }).Require());
        if(mode=="create-update-recovery")
        {
            var second=SupportingOriginal(current,kind,true);var next=await service.RecordCommandAsync(second);next.Require();
            Assert.Equal(child.ROW_CREATED_DATE,PermitSupportingAmendments.Read(next.Supporting,second.Action)!.ROW_CREATED_DATE);
            var latest=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.Equal(2,latest.ReadHistory().Count);Assert.Equal(2,(await fixture.ReceiptRepository.GetAsync([])).Count());
            fixture.AllowWrites=false;Assert.Equal(saved,await service.RecoverCommandAsync(original));Assert.Equal(saved,await service.RecordCommandAsync(original));
            Assert.Equal(latest,await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));
        }
        var shape=source.GetEntityStructure(child.GetType().Name,true);
        if(kind=="injection")
        {Assert.DoesNotContain(shape.Fields,x=>x.FieldName is "MAX_INJECTION_PRESSURE" or "MAX_INJECTION_RATE" or "FLUID_TYPE");Assert.Contains(shape.Fields,x=>x.FieldName=="INJECTION_PRESSURE_UNIT");}
        else Assert.DoesNotContain(shape.Fields,x=>x.FieldName=="WASTE_DISPOSAL_METHOD");
        PPDMDataSourceTransaction.RequireIdle(source);
    });
    [Theory]
    [InlineData("environmental","foreign")][InlineData("environmental","unit")][InlineData("environmental","orphan-unit")][InlineData("environmental","terminal")]
    [InlineData("environmental","mixed")][InlineData("environmental","flag")][InlineData("environmental","audit")][InlineData("environmental","changed")]
    [InlineData("injection","foreign")][InlineData("injection","unit")][InlineData("injection","orphan-unit")][InlineData("injection","terminal")]
    [InlineData("injection","mixed")][InlineData("injection","flag")][InlineData("injection","audit")][InlineData("injection","changed")]
    public void PermitSupportingOriginalRefusesForeignInvalidAndChangedEvidence(string kind,string mode)
    {
        var scope=new PermitApplicationDataScope(new(new("tenant","company"),new("core","binding"),"author","selection","field-a"),new("permits","binding"));
        var parent=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="permit",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",WELL_ID="well-a",APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,
            APPLICATION_TYPE=kind=="environmental"?PermitApplicationType.Environmental:PermitApplicationType.Injection,STATUS=mode=="terminal"?PermitApplicationStatus.Approved:PermitApplicationStatus.Draft,ACTIVE_IND="Y" };
        var d=new ENVIRONMENTAL_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=mode=="foreign"?"foreign":"permit",ACTIVE_IND="Y",ENVIRONMENTAL_PERMIT_TYPE="Recorded" };
        var i=new INJECTION_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=mode=="foreign"?"foreign":"permit",ACTIVE_IND="Y",INJECTION_TYPE="Recorded",INJECTION_FLUID="Water" };
        PermitApplicationSupportingSnapshot Snapshot()=>PermitApplicationSupportingSnapshot.Capture(parent,null,kind=="environmental"?d:null,kind=="injection"?i:null);
        if(mode=="foreign"){Assert.Throws<InvalidOperationException>(()=>Snapshot());return;}
        var review=PermitApplicationSourceReview.Capture(scope,parent,[],Snapshot());var original=SupportingOriginal(review,kind);
        if(mode is "unit" or "orphan-unit" or "flag")
        {
            if(kind=="environmental")original=original with { EnvironmentalChanges=original.EnvironmentalChanges! with {
                WasteVolumeUnit=mode=="unit"?null:"m3",WasteVolume=mode=="orphan-unit"?null:1,NormInvolved=mode=="flag"?"unknown":null } };
            else original=original with { InjectionChanges=original.InjectionChanges! with {
                PressureUnit=mode=="unit"?null:"kPa",MaximumPressure=mode=="orphan-unit"?null:1,Co2Storage=mode=="flag"?"unknown":null } };
        }
        if(mode=="mixed")original=original with { Action="SUBMIT" };
        if(mode is "unit" or "orphan-unit" or "terminal" or "mixed" or "flag"){Assert.Throws<InvalidOperationException>(original.Require);return;}
        if(mode=="changed")
        {
            d.REMARKS="Changed";i.REMARKS="Changed";var changed=PermitApplicationSourceReview.Capture(scope,parent,[],Snapshot());
            Assert.Throws<InvalidOperationException>(()=>review.RequireCurrent(changed));Assert.Throws<InvalidOperationException>(()=>(review with { Supporting=changed.Supporting }).Require());return;
        }
        var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);outcome.Require();
        var child=PermitSupportingAmendments.Read(outcome.Supporting,original.Action)!;child.ROW_CHANGED_BY="foreign";
        Assert.Throws<InvalidOperationException>(()=>(outcome with { Supporting=PermitSupportingAmendments.Replace(outcome.Supporting,child) }).Require());
    }

    [Theory]
    [InlineData("environmental","correction-unknown")]
    [InlineData("environmental","wrong-type")]
    [InlineData("environmental","well")]
    [InlineData("environmental","negative")]
    [InlineData("environmental","zero")]
    [InlineData("environmental","length")]
    [InlineData("environmental","mixed-type")]
    [InlineData("injection","correction-unknown")]
    [InlineData("injection","wrong-type")]
    [InlineData("injection","well")]
    [InlineData("injection","negative")]
    [InlineData("injection","zero")]
    [InlineData("injection","length")]
    [InlineData("injection","mixed-type")]
    public void SupportingDraftRequiresExactStageTypeUnitsAndBounds(string kind,string mode)
    {
        var scope=new PermitApplicationDataScope(new(new("tenant","company"),new("core","binding"),"author","selection","field-a"),new("permits","binding"));
        var parent=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="supporting-contract",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,
            WELL_ID=mode=="well"?null:"well-a",APPLICATION_TYPE=mode=="wrong-type"?PermitApplicationType.Other:kind=="environmental"?PermitApplicationType.Environmental:PermitApplicationType.Injection,
            STATUS=mode=="correction-unknown"?PermitApplicationStatus.AdditionalInformationRequired:PermitApplicationStatus.Draft,ACTIVE_IND="Y" };
        var review=PermitApplicationSourceReview.Capture(scope,parent,[]);var original=SupportingOriginal(review,kind);
        if(kind=="environmental")original=original with { EnvironmentalChanges=original.EnvironmentalChanges! with {
            WasteVolume=mode=="correction-unknown"?null:mode=="negative"?-1:mode=="zero"?0:123.5m,
            WasteVolumeUnit=mode=="correction-unknown"?null:"m3",MonitoringPlan=mode=="length"?new string('x',4097):null } };
        else original=original with { InjectionChanges=original.InjectionChanges! with {
            MaximumPressure=mode=="correction-unknown"?null:mode=="negative"?-1:mode=="zero"?0:2500.5m,
            PressureUnit=mode=="correction-unknown"?null:"kPa",MaximumRate=null,RateUnit=null,MonitoringRequirements=mode=="length"?new string('x',4097):null } };
        if(mode=="mixed-type")original=kind=="environmental"?original with { InjectionChanges=SupportingOriginal(review,"injection").InjectionChanges }:
            original with { EnvironmentalChanges=SupportingOriginal(review,"environmental").EnvironmentalChanges };
        var allowed=mode=="correction-unknown" || kind=="environmental" && mode is "well" or "zero";
        if(!allowed){Assert.Throws<InvalidOperationException>(original.Require);return;}
        original.Require();Assert.True(PermitApplicationCommand.IsAuthorAction(original.Action));
        var saved=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);saved.Require();Assert.Equal(parent.STATUS,saved.ReadApplication().STATUS);
        if(mode=="correction-unknown")
        {
            if(kind=="environmental"){Assert.Null(saved.Supporting.Environmental!.WASTE_VOLUME);Assert.Null(saved.Supporting.Environmental.WASTE_VOLUME_UNIT);}
            else {Assert.Null(saved.Supporting.Injection!.MAXIMUM_INJECTION_PRESSURE);Assert.Null(saved.Supporting.Injection.INJECTION_PRESSURE_UNIT);Assert.Null(saved.Supporting.Injection.MAXIMUM_INJECTION_RATE);Assert.Null(saved.Supporting.Injection.INJECTION_RATE_UNIT);}
        }
    }

    [Theory][InlineData("stale-submission")][InlineData("correction")][InlineData("historical")][InlineData("unavailable-schema")]
    public Task InstalledPermitAttachmentMetadataBindsReviewedPopulation(string mode)=>WithInstalledProviderAsync(async (editor,source)=>
    {
        InstallLedgerSchema(editor,source,attachments:mode!="unavailable-schema");
        var fixture=CreateNativeFixture(editor,source);var service=fixture.Service;
        var seed=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="attachment-application",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,
            STATUS=mode=="correction"?PermitApplicationStatus.Submitted:PermitApplicationStatus.Draft,SUBMITTED_BY_USER_ID=mode=="correction"?"issuer":string.Empty,ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(seed,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(PERMIT_APPLICATION),seed).Flag);
        if(mode=="unavailable-schema")
        {
            await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));
            Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));return;
        }
        var shape=source.GetEntityStructure(nameof(APPLICATION_ATTACHMENT),true);Assert.DoesNotContain(shape.Fields,x=>x.FieldName=="FILE_PATH");
        var originalReview=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.Empty(originalReview.Supporting.Attachments);
        PermitApplicationCommandOutcome? historical=null;
        if(mode=="historical")historical=await service.RecordCommandAsync(new(Guid.NewGuid(),originalReview,"SUBMIT","Reviewed before any attachment metadata was recorded"));
        var attachment=new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="attachment-1",PERMIT_APPLICATION_ID=seed.PERMIT_APPLICATION_ID,FILE_NAME="recorded.pdf",FILE_TYPE="application/pdf",DOCUMENT_TYPE="Plan",FILE_SIZE=123,DESCRIPTION="Recorded metadata only",UPLOADED_BY="recorded-uploader",ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(attachment,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(APPLICATION_ATTACHMENT),attachment).Flag);
        var foreign=new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="foreign-attachment",PERMIT_APPLICATION_ID="another-application",FILE_NAME="foreign.pdf",ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(foreign,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(APPLICATION_ATTACHMENT),foreign).Flag);
        var current=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.NotEqual(originalReview.Revision,current.Revision);
        Assert.Equal("attachment-1",Assert.Single(current.Supporting.Attachments).APPLICATION_ATTACHMENT_ID);
        var positive=mode=="correction"?"APPROVE":"SUBMIT";Assert.DoesNotContain(positive,PermitApplicationCommand.AllowedActions(current));
        Assert.Throws<InvalidOperationException>(()=>new PermitApplicationCommand(Guid.NewGuid(),current,positive,"Review recorded attachment metadata").Require());
        if(mode=="historical")
        {
            fixture.AllowWrites=false;Assert.Equal(historical,await service.RecoverCommandAsync(historical!.Original));Assert.Equal(historical,await service.RecordCommandAsync(historical.Original));
            Assert.Single((await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID)).ReadHistory());Assert.Single(await fixture.ReceiptRepository.GetAsync([]));
        }
        else if(mode=="stale-submission")
        {
            var stale=new PermitApplicationCommand(Guid.NewGuid(),originalReview,"SUBMIT","Original captured before attachment appeared");
            await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(stale));Assert.Null(await service.RecoverCommandAsync(stale));
            Assert.Empty((await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID)).ReadHistory());Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));
        }
        else
        {
            var correction=new PermitApplicationCommand(Guid.NewGuid(),current,"RETURN_FOR_CORRECTION","Native content has not been confirmed; provide reviewed document evidence");
            var outcome=await service.RecordCommandAsync(correction);outcome.Require();Assert.Equal(PermitApplicationStatus.AdditionalInformationRequired,outcome.ReadApplication().STATUS);
            Assert.Equal(current.Supporting.AttachmentsJson,outcome.Supporting.AttachmentsJson);Assert.Single((await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID)).ReadHistory());
            fixture.AllowWrites=false;Assert.Equal(outcome,await service.RecoverCommandAsync(correction));Assert.Equal(outcome,await service.RecordCommandAsync(correction));
        }
        PPDMDataSourceTransaction.RequireIdle(source);
    });
    [Theory][InlineData("foreign")][InlineData("duplicate")][InlineData("inactive")][InlineData("negative")][InlineData("oversized")][InlineData("fingerprint")][InlineData("order-copy")]
    public void PermitAttachmentSnapshotRequiresExactBoundedFrozenPopulation(string mode)
    {
        var scope=new PermitApplicationDataScope(new(new("tenant","company"),new("core","binding"),"author","selection","field-a"),new("permits","binding"));
        var parent=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="permit",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,ACTIVE_IND="Y" };
        var row=new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="B",PERMIT_APPLICATION_ID=mode=="foreign"?"foreign":"permit",FILE_NAME="<untrusted>.pdf",ACTIVE_IND=mode=="inactive"?"N":"Y",FILE_SIZE=mode=="negative"?-1:null };
        APPLICATION_ATTACHMENT[] rows=mode=="duplicate"?[row,row]:mode=="oversized"?Enumerable.Range(0,129).Select(i=>new APPLICATION_ATTACHMENT{APPLICATION_ATTACHMENT_ID="A"+i,PERMIT_APPLICATION_ID="permit",FILE_NAME="recorded.pdf",ACTIVE_IND="Y"}).ToArray():[row];
        if(mode is "foreign" or "duplicate" or "inactive" or "negative" or "oversized")
        {Assert.Throws<InvalidOperationException>(()=>PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,rows));return;}
        var first=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,rows));
        if(mode=="fingerprint")
        {
            row.DESCRIPTION="Changed";var changed=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,rows));
            Assert.Throws<InvalidOperationException>(()=>first.RequireCurrent(changed));Assert.Throws<InvalidOperationException>(()=>(first with { Supporting=changed.Supporting }).Require());return;
        }
        var another=new APPLICATION_ATTACHMENT { APPLICATION_ATTACHMENT_ID="A",PERMIT_APPLICATION_ID="permit",FILE_NAME="second.pdf",ACTIVE_IND="Y" };
        var a=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,[row,another]));
        var b=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,null,null,null,[another,row]));Assert.Equal(a,b);
        row.FILE_NAME="Mutated after capture";var detached=a.Supporting.Attachments;detached[0].FILE_NAME="Mutated parsed copy";
        Assert.Equal("second.pdf",a.Supporting.Attachments[0].FILE_NAME);Assert.Equal("<untrusted>.pdf",a.Supporting.Attachments[1].FILE_NAME);
    }

    [Fact]
    public Task InstalledPermitReviewRefusesUnavailableSupportingSchema() => WithInstalledProviderAsync(async (editor,source) =>
    {
        InstallLedgerSchema(editor,source,supporting:false);
        var fixture=CreateNativeFixture(editor,source);
        var seed=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="schema-unavailable",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(seed,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(PERMIT_APPLICATION),seed).Flag);
        await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>fixture.Service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));
        Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));
    });

    [Theory]
    [InlineData("create-update-recovery")]
    [InlineData("rollback")]
    [InlineData("stale-support")]
    public Task InstalledPermitDrillingAmendmentUsesReviewedOwnedLedger(string mode) => WithInstalledLedgerAsync(async (editor,source) =>
    {
        var fixture=CreateNativeFixture(editor,source);var service=fixture.Service;
        var seed=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="drilling-amendment", OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",
            FIELD_ID="field-a",WELL_ID="well-a",APPLICANT_ID="Applicant",COUNTRY=Country.Kuwait,APPLICATION_TYPE=PermitApplicationType.Drilling,
            STATUS=PermitApplicationStatus.Draft,ACTIVE_IND="Y" };
        new CommonColumnHandler().PrepareForInsert(seed,"author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(PERMIT_APPLICATION),seed).Flag);
        var review=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        var proposal=new PermitDrillingDraftChanges(1234.5m,"m","Formation","Directional",null,new string('c',2048),"Recorded land",null,"N",null,"Spacing","Drilling","Supporting proposal");
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"AMEND_DRILLING","Reviewed the exact drilling details and explicit unit") { DrillingChanges=proposal };
        original.Require();
        if(mode=="stale-support")
        {
            var late=new DRILLING_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=seed.PERMIT_APPLICATION_ID,ACTIVE_IND="Y",PROPOSED_DEPTH=9,PROPOSED_DEPTH_UNIT="ft" };
            new CommonColumnHandler().PrepareForInsert(late,"another-author");Assert.Equal(Errors.Ok,source.InsertEntity(nameof(DRILLING_PERMIT_APPLICATION),late).Flag);
            var changed=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.NotEqual(review.Revision,changed.Revision);
            await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(original));Assert.Null(await service.RecoverCommandAsync(original));
            Assert.Empty(changed.ReadHistory());Assert.Empty(await fixture.ReceiptRepository.GetAsync([]));return;
        }
        if(mode=="rollback")
        {
            fixture.InterruptReceipt=true;await Assert.ThrowsAnyAsync<Exception>(()=>service.RecordCommandAsync(original));Assert.True(fixture.Interrupted);
            Assert.Equal(review,await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));Assert.Null(await service.RecoverCommandAsync(original));fixture.InterruptReceipt=false;
        }
        var saved=await service.RecordCommandAsync(original);saved.Require();Assert.Equal(proposal,PermitDrillingDraftChanges.From(saved.Supporting.Drilling!));
        Assert.Equal(PermitApplicationStatus.Draft,saved.ReadApplication().STATUS);Assert.Equal("well-a",saved.ReadApplication().WELL_ID);
        var current=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.Single(current.ReadHistory());Assert.Single(await fixture.ReceiptRepository.GetAsync([]));
        Assert.Throws<InvalidOperationException>(()=>proposal.Require(current));
        if(mode=="create-update-recovery")
        {
            var amended=proposal with { Remarks="Revised supporting notes",ProposedDepth=2000.25m };
            var second=new PermitApplicationCommand(Guid.NewGuid(),current,"AMEND_DRILLING","Reviewed changed drilling evidence after the first amendment") { DrillingChanges=amended };
            var next=await service.RecordCommandAsync(second);next.Require();Assert.Equal(amended,PermitDrillingDraftChanges.From(next.Supporting.Drilling!));
            var latest=await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);Assert.Equal(2,latest.ReadHistory().Count);Assert.Equal(2,(await fixture.ReceiptRepository.GetAsync([])).Count());
            Assert.Equal(saved.Supporting.Drilling!.ROW_CREATED_DATE,next.Supporting.Drilling!.ROW_CREATED_DATE);
            fixture.AllowWrites=false;Assert.Equal(saved,await service.RecoverCommandAsync(original));Assert.Equal(saved,await service.RecordCommandAsync(original));
            Assert.Equal(latest,await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID));
        }
        PPDMDataSourceTransaction.RequireIdle(source);
    });

    [Theory]
    [InlineData("foreign")][InlineData("unit")][InlineData("terminal")][InlineData("audit-tamper")][InlineData("unreviewed-change")][InlineData("length")]
    public void PermitDrillingOriginalRejectsForeignInvalidAndChangedEvidence(string mode)
    {
        var scope=new PermitApplicationDataScope(new(new("tenant","company"),new("core","binding"),"author","selection","field-a"),new("permits","binding"));
        var parent=new PERMIT_APPLICATION { PERMIT_APPLICATION_ID="permit",OWNER_TENANT_ID="tenant",OWNER_COMPANY_ID="company",FIELD_ID="field-a",
            WELL_ID="well-a",COUNTRY=Country.Kuwait,APPLICANT_ID="Applicant",APPLICATION_TYPE=PermitApplicationType.Drilling,STATUS=PermitApplicationStatus.Draft,ACTIVE_IND="Y" };
        var child=new DRILLING_PERMIT_APPLICATION { PERMIT_APPLICATION_ID=mode=="foreign"?"foreign":"permit",ACTIVE_IND="Y",PROPOSED_DEPTH=1,PROPOSED_DEPTH_UNIT="ft" };
        if(mode=="foreign"){Assert.Throws<InvalidOperationException>(()=>PermitApplicationSupportingSnapshot.Capture(parent,child,null,null));return;}
        if(mode=="terminal")parent.STATUS=PermitApplicationStatus.Approved;
        var review=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,child,null,null));
        var proposal=PermitDrillingDraftChanges.From(child) with { ProposedDepth=2000,DepthUnit=mode=="unit"?"":"m",Remarks=mode=="length"?new string('x',4097):null };
        var original=new PermitApplicationCommand(Guid.NewGuid(),review,"AMEND_DRILLING","Reviewed supporting details") { DrillingChanges=proposal };
        if(mode is "unit" or "terminal" or "length"){Assert.Throws<InvalidOperationException>(original.Require);return;}
        if(mode=="unreviewed-change")
        {
            child.REMARKS="Changed after review";
            var changed=PermitApplicationSourceReview.Capture(scope,parent,[],PermitApplicationSupportingSnapshot.Capture(parent,child,null,null));
            Assert.Throws<InvalidOperationException>(()=>review.RequireCurrent(changed));
            Assert.Throws<InvalidOperationException>(()=>(review with { Supporting=changed.Supporting }).Require());return;
        }
        var outcome=PermitApplicationCommandOutcome.Prepare(original,DateTimeOffset.UtcNow);outcome.Require();
        var tampered=outcome.Supporting.Drilling!;tampered.ROW_CHANGED_BY="foreign";
        Assert.Throws<InvalidOperationException>(()=>(outcome with { Supporting=PermitApplicationSupportingSnapshot.Capture(parent,tampered,null,null) }).Require());
    }

    [Fact]
    public async Task InstalledPermitProviderRecoversUnacknowledgedSubmissionAcrossProcesses()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-permit-restart-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var commit = await RunWorkerAsync("commit", directory, 73);
        var originalBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "original.json"));
        var databaseHash = SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db")));
        var recovered = await RunWorkerAsync("recover", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        var replayed = await RunWorkerAsync("replay", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "original.json")));
        Assert.Equal(3, new[] { commit.ProcessId, recovered.ProcessId, replayed.ProcessId }.Distinct().Count());
        Assert.DoesNotContain(Environment.ProcessId, new[] { commit.ProcessId, recovered.ProcessId, replayed.ProcessId });
        Assert.Null(commit.OutcomeHash);
        Assert.False(string.IsNullOrEmpty(recovered.OutcomeHash));
        Assert.Equal(recovered.OutcomeHash, replayed.OutcomeHash);
        Assert.Equal(commit.IntentId, recovered.IntentId);
        Assert.Equal(commit.IntentId, replayed.IntentId);
        Assert.All(new[] { recovered, replayed }, report =>
        {
            Assert.Equal(1, report.HistoryCount);
            Assert.Equal(1, report.ReceiptCount);
        });
    }

    private sealed record RestartReport(int ProcessId, Guid IntentId, string? OutcomeHash, int HistoryCount, int ReceiptCount);

    private static Task<RestartReport> RunWorkerAsync(string stage, string directory, int expectedExit) => RunWorkerAsync<RestartReport>(stage, directory, expectedExit);

    private static async Task<T> RunWorkerAsync<T>(string stage, string directory, int expectedExit) where T : class
    {
        var worker = Path.Combine(AppContext.BaseDirectory, "Beep.OilandGas.PermitProviderWorker.dll");
        Assert.True(File.Exists(worker), "Build tools/PermitProviderWorker and copy its DLL into the focused test output before this gate.");
        var tests = typeof(PermitProviderQualificationTests).Assembly.Location;
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var argument in new[] { "exec", "--runtimeconfig", Path.ChangeExtension(tests, ".runtimeconfig.json"), "--depsfile", Path.ChangeExtension(tests, ".deps.json"), worker, stage, directory })
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("The permit qualification worker could not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try { await process.WaitForExitAsync(deadline.Token); }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw;
        }
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stdout.log"), await stdout);
        await File.WriteAllTextAsync(Path.Combine(directory, stage + ".stderr.log"), await stderr);
        Assert.True(process.ExitCode == expectedExit, $"Permit {stage} worker exited {process.ExitCode}: {await stderr} {await stdout}");
        return JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(Path.Combine(directory, stage + ".json")))
            ?? throw new InvalidOperationException("The permit worker report is absent.");
    }

    // Entry point for the separate qualification executable; this is not an API or production authorization path.
    public static Task RunRestartStageAsync(string stage, string directory)
    {
        if (stage is not ("commit" or "recover" or "replay")) throw new ArgumentException("Unknown permit qualification stage.", nameof(stage));
        return new PermitProviderQualificationTests().WithInstalledLedgerAsync(async (editor, source) =>
        {
            var fixture = CreateNativeFixture(editor, source);
            var service = fixture.Service;
            var originalPath = Path.Combine(directory, "original.json");
            PermitApplicationCommand original;
            if (stage == "commit")
            {
                var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "restart-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
                    FIELD_ID = "field-a", APPLICANT_ID = "Restart qualification applicant", ACTIVE_IND = "Y" };
                new CommonColumnHandler().PrepareForInsert(seed, "author");
                Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
                original = new(Guid.NewGuid(), await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID), "SUBMIT", "Retained before transport; response deliberately withheld");
                await File.WriteAllTextAsync(originalPath, JsonSerializer.Serialize(original));
                // Do not pass the returned result to the caller. Only fresh-process recovery can confirm it.
                await service.RecordCommandAsync(original);
                await File.WriteAllTextAsync(Path.Combine(directory, "commit.json"), JsonSerializer.Serialize(new RestartReport(Environment.ProcessId, original.IntentId, null, 0, 0)));
            }
            else
            {
                original = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(originalPath))
                    ?? throw new InvalidOperationException("The retained permit original is absent.");
                original.Require();
                fixture.AllowWrites = false;
                var saved = stage == "recover" ? await service.RecoverCommandAsync(original) : await service.RecordCommandAsync(original);
                Assert.NotNull(saved);
                saved.Require();
                Assert.Equal(original, saved.Original);
                Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, saved.ReadApplication().STATUS);
                var current = await service.ReviewSourceAsync(original.Review.ReadApplication().PERMIT_APPLICATION_ID);
                var receipts = (await fixture.ReceiptRepository.GetAsync([])).ToArray();
                Assert.Single(receipts);
                Assert.Equal(saved, Assert.IsType<PERMIT_APPLICATION_COMMAND>(receipts[0]).Restore(original.Review.Context));
                var report = new RestartReport(Environment.ProcessId, original.IntentId, ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), current.ReadHistory().Count, receipts.Length);
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(report));
            }
            PPDMDataSourceTransaction.RequireIdle(source);
        }, directory, installSchema: stage == "commit");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InstalledPermitProviderSerializesConcurrentOriginalsAcrossProcesses(bool sameOriginal)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-permit-race-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        await RunWorkerAsync<RaceReport>(sameOriginal ? "race-prepare-same" : "race-prepare-conflict", directory, 0);
        var aBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "a.original.json"));
        var bBytes = await File.ReadAllBytesAsync(Path.Combine(directory, "b.original.json"));
        var a = RunWorkerAsync<RaceReport>("race-a", directory, 0);
        var b = RunWorkerAsync<RaceReport>("race-b", directory, 0);
        RaceReport[] contenders;
        try
        {
            await WaitForFilesAsync(directory, "a.ready", "b.ready");
            await File.WriteAllTextAsync(Path.Combine(directory, "release"), "Both originals reached the pre-transaction barrier.");
            contenders = await Task.WhenAll(a, b);
        }
        catch (Exception primary)
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "release"), "Release owned workers for diagnostic cleanup.");
            try { await Task.WhenAll(a, b); }
            catch (Exception cleanup) { throw new AggregateException("Permit race failed; both workers were awaited.", primary, cleanup); }
            throw;
        }
        var databaseHash = SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db")));
        var audit = await RunWorkerAsync<RaceReport>("race-audit", directory, 0);
        Assert.Equal(databaseHash, SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(directory, "permits.db"))));
        Assert.Equal(aBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "a.original.json")));
        Assert.Equal(bBytes, await File.ReadAllBytesAsync(Path.Combine(directory, "b.original.json")));
        Assert.NotEqual(contenders[0].ProcessId, contenders[1].ProcessId);
        Assert.DoesNotContain(audit.ProcessId, contenders.Select(report => report.ProcessId));
        Assert.Equal(1, audit.HistoryCount);
        Assert.Equal(1, audit.ReceiptCount);
        var held = new[] { "a", "b" }.Select(role => JsonSerializer.Deserialize<OwnedWindow>(File.ReadAllText(Path.Combine(directory, role + ".owned.json")))!).OrderBy(window => window.StartTicks).ToArray();
        Assert.All(held, window => Assert.Equal(IsolationLevel.Serializable.ToString(), window.Isolation));
        Assert.True(held[0].ReleasedTicks <= held[1].StartTicks, "Owned serializable transaction windows overlapped across processes.");
        if (sameOriginal)
        {
            Assert.All(contenders, report => { Assert.Equal("recorded", report.Decision); Assert.Equal(audit.OutcomeHash, report.OutcomeHash); });
            Assert.Equal(contenders[0].IntentId, contenders[1].IntentId);
        }
        else
        {
            var winner = Assert.Single(contenders, report => report.Decision == "recorded");
            var loser = Assert.Single(contenders, report => report.Decision == "source-conflict");
            Assert.NotEqual(winner.IntentId, loser.IntentId);
            Assert.Equal(winner.IntentId, audit.IntentId);
            Assert.Equal(winner.OutcomeHash, audit.OutcomeHash);
            Assert.Null(loser.OutcomeHash);
        }
    }

    private sealed record RaceReport(int ProcessId, Guid IntentId, string Decision, string? OutcomeHash, int HistoryCount, int ReceiptCount);
    private sealed record OwnedWindow(int ProcessId, string Isolation, long StartTicks, long ReleasedTicks);

    private static async Task WaitForFilesAsync(string directory, params string[] names)
    {
        var timer = Stopwatch.StartNew();
        while (names.Any(name => !File.Exists(Path.Combine(directory, name))))
        {
            if (timer.Elapsed > TimeSpan.FromSeconds(30)) throw new TimeoutException("Permit race barrier was not reached: " + string.Join(", ", names));
            await Task.Delay(25);
        }
    }

    public static Task RunRaceStageAsync(string stage, string directory)
    {
        if (stage is not ("race-prepare-same" or "race-prepare-conflict" or "race-a" or "race-b" or "race-audit"))
            throw new ArgumentException("Unknown permit race qualification stage.", nameof(stage));
        var prepare = stage.StartsWith("race-prepare-", StringComparison.Ordinal);
        return new PermitProviderQualificationTests().WithInstalledLedgerAsync(async (editor, source) =>
        {
            var fixture = CreateNativeFixture(editor, source);
            var service = fixture.Service;
            if (prepare)
            {
                var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "race-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
                    FIELD_ID = "field-a", APPLICANT_ID = "Concurrent filing applicant", ACTIVE_IND = "Y" };
                new CommonColumnHandler().PrepareForInsert(seed, "author");
                Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
                var a = new PermitApplicationCommand(Guid.NewGuid(), await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID), "SUBMIT", "Reviewed shared source before concurrent delivery");
                var b = stage == "race-prepare-same" ? a : a with { IntentId = Guid.NewGuid(), Reason = "Separately reviewed the same frozen source" };
                await File.WriteAllTextAsync(Path.Combine(directory, "a.original.json"), JsonSerializer.Serialize(a));
                await File.WriteAllTextAsync(Path.Combine(directory, "b.original.json"), JsonSerializer.Serialize(b));
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(new RaceReport(Environment.ProcessId, a.IntentId, "prepared", null, 0, 0)));
            }
            else if (stage is "race-a" or "race-b")
            {
                var role = stage == "race-a" ? "a" : "b";
                var other = role == "a" ? "b" : "a";
                var original = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, role + ".original.json")))!;
                original.Require();
                fixture.BeforeCommand = async () =>
                {
                    PPDMDataSourceTransaction.RequireIdle(source);
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".ready"), Environment.ProcessId.ToString());
                    await WaitForFilesAsync(directory, "release");
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".attempting"), Environment.ProcessId.ToString());
                };
                var guarded = false;
                fixture.CommandGuard = async () =>
                {
                    PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                    if (guarded) return;
                    guarded = true;
                    var start = DateTimeOffset.UtcNow.UtcTicks;
                    await WaitForFilesAsync(directory, other + ".attempting");
                    await Task.Delay(250); // Hold a verified transaction while both workers attempt entry.
                    await File.WriteAllTextAsync(Path.Combine(directory, role + ".owned.json"), JsonSerializer.Serialize(new OwnedWindow(Environment.ProcessId, IsolationLevel.Serializable.ToString(), start, DateTimeOffset.UtcNow.UtcTicks)));
                };
                RaceReport report;
                try
                {
                    var saved = await service.RecordCommandAsync(original);
                    saved.Require();
                    Assert.Equal(original, saved.Original);
                    report = new(Environment.ProcessId, original.IntentId, "recorded", ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), 0, 0);
                }
                catch (RefusalException refusal) when (refusal.Kind == RefusalKind.Conflict)
                {
                    Assert.Null(await service.RecoverCommandAsync(original));
                    report = new(Environment.ProcessId, original.IntentId, "source-conflict", null, 0, 0);
                }
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(report));
            }
            else
            {
                fixture.AllowWrites = false;
                var a = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, "a.original.json")))!;
                var b = JsonSerializer.Deserialize<PermitApplicationCommand>(await File.ReadAllTextAsync(Path.Combine(directory, "b.original.json")))!;
                var receipts = (await fixture.ReceiptRepository.GetAsync([])).ToArray();
                var receipt = Assert.IsType<PERMIT_APPLICATION_COMMAND>(Assert.Single(receipts));
                var saved = receipt.Restore(a.Review.Context);
                Assert.True(saved.Original == a || saved.Original == b);
                Assert.Equal(saved, await service.RecoverCommandAsync(saved.Original));
                if (a.IntentId != b.IntentId) Assert.Null(await service.RecoverCommandAsync(saved.Original == a ? b : a));
                var current = await service.ReviewSourceAsync(saved.Original.Review.ReadApplication().PERMIT_APPLICATION_ID);
                Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, current.ReadApplication().STATUS);
                await File.WriteAllTextAsync(Path.Combine(directory, stage + ".json"), JsonSerializer.Serialize(new RaceReport(Environment.ProcessId, saved.Original.IntentId, "audited", ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(saved)), current.ReadHistory().Count, receipts.Length)));
            }
            PPDMDataSourceTransaction.RequireIdle(source);
        }, directory, installSchema: prepare);
    }

    private sealed class NativeFixture
    {
        public bool AllowWrites = true;
        public bool InterruptReceipt;
        public bool Interrupted;
        public bool InterruptDocument;
        public bool InterruptDocumentProof;
        public bool DocumentInterrupted;
        public Func<Task>? BeforeCommand;
        public Func<Task>? CommandGuard;
        public PermitApplicationLifecycleService Service = null!;
        public PPDMGenericRepository ReceiptRepository = null!;
    }

    private static NativeFixture CreateNativeFixture(IDMEEditor editor, IDataSource source, string coreConnection = "core")
    {
        var scope = new PermitApplicationDataScope(new(new("tenant", "company"), new(coreConnection, "core-binding"), "author", "selection", "field-a"),
            new(source.DatasourceName, "permit-binding"));
        var metadata = new Mock<IPPDMMetadataRepository>();
        foreach (var type in new[] { typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND), typeof(DRILLING_PERMIT_APPLICATION), typeof(ENVIRONMENTAL_PERMIT_APPLICATION), typeof(INJECTION_PERMIT_APPLICATION), typeof(APPLICATION_ATTACHMENT),typeof(PERMIT_DOCUMENT_STAGE) })
        {
            var key = type.Name.EndsWith("_PERMIT_APPLICATION",StringComparison.Ordinal) ? "PERMIT_APPLICATION_ID" : type.Name + "_ID";
            metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
            metadata.Setup(x => x.GetPrimaryKeyColumnAsync(type.Name)).ReturnsAsync(key);
        }
        foreach(var type in new[]{typeof(Beep.OilandGas.PPDM39.Models.SOURCE_DOCUMENT),typeof(Beep.OilandGas.PPDM39.Models.RM_INFORMATION_ITEM),typeof(Beep.OilandGas.PPDM39.Models.RM_FILE_CONTENT)})
        {
            var keys=type.Name=="RM_INFORMATION_ITEM"?new[]{"INFORMATION_ITEM_ID","INFO_ITEM_SUBTYPE"}:type.Name=="RM_FILE_CONTENT"?new[]{"FILE_ID"}:new[]{"SOURCE_DOCUMENT_ID"};
            metadata.Setup(x=>x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata {TableName=type.Name,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)});
            metadata.Setup(x=>x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync(keys);
            metadata.Setup(x=>x.GetPrimaryKeyColumnAsync(type.Name)).ReturnsAsync(string.Join(',',keys));
        }
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(x => x.GetActiveIndicatorYes()).Returns("Y");
        defaults.Setup(x => x.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => Convert.ToString(id, System.Globalization.CultureInfo.InvariantCulture)!);
        var access = new Mock<ICompanyPermitApplicationDataAccess>();
        access.Setup(x => x.RequireReadScopeAsync()).ReturnsAsync(scope);
        access.Setup(x => x.ForRead(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(new ActivePermitFixtureReads(scope));
        var fixture = new NativeFixture();
        access.Setup(x => x.WithCommandAuthorityAsync(It.IsAny<PermitApplicationCommand>(), It.IsAny<Func<Func<Task>, Task<bool>>>()))
            .Returns(async (PermitApplicationCommand _, Func<Func<Task>, Task<bool>> execute) =>
            {
                if (!fixture.AllowWrites) throw new UnauthorizedAccessException("Fixture author permission removed.");
                if (fixture.BeforeCommand is not null) await fixture.BeforeCommand();
                return await execute(async () => { if (fixture.CommandGuard is not null) await fixture.CommandGuard(); });
            });
        access.Setup(x=>x.WithDocumentStageAuthorityAsync(It.IsAny<PermitDocumentStageOriginal>(),It.IsAny<Func<Func<Task>,Task<bool>>>()))
            .Returns(async (PermitDocumentStageOriginal _,Func<Func<Task>,Task<bool>> execute)=>
            {
                if(!fixture.AllowWrites)throw new UnauthorizedAccessException("Fixture author permission removed.");
                if(fixture.BeforeCommand is not null)await fixture.BeforeCommand();
                return await execute(async()=>{if(!fixture.AllowWrites)throw new UnauthorizedAccessException("Fixture author permission revoked.");if(fixture.CommandGuard is not null)await fixture.CommandGuard();});
            });
        var actualColumns = new CommonColumnHandler();
        var columns = new Mock<ICommonColumnHandler>();
        columns.Setup(x => x.PrepareForUpdate(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) => {
            actualColumns.PrepareForUpdate(row,actor);
            if(row is PERMIT_DOCUMENT_STAGE { STAGED_JSON:not null } && fixture.InterruptDocumentProof){fixture.DocumentInterrupted=true;throw new InvalidOperationException("Controlled loss before staging confirmation write.");}
        });
        columns.Setup(x => x.PrepareForInsert(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) =>
        {
            actualColumns.PrepareForInsert(row, actor);
            if(row is Beep.OilandGas.PPDM39.Models.RM_FILE_CONTENT && fixture.InterruptDocument){fixture.DocumentInterrupted=true;throw new InvalidOperationException("Controlled failure after two native document writes.");}
            if (row is PERMIT_APPLICATION_COMMAND && fixture.InterruptReceipt) { fixture.Interrupted = true; throw new InvalidOperationException("Controlled interruption before receipt persistence."); }
        });
        fixture.Service = new PermitApplicationLifecycleService(editor, columns.Object, defaults.Object, metadata.Object, access.Object);
        fixture.ReceiptRepository = new PPDMGenericRepository(editor, actualColumns, defaults.Object, metadata.Object, typeof(PERMIT_APPLICATION_COMMAND), source.DatasourceName,
            nameof(PERMIT_APPLICATION_COMMAND), rowAccess: new ActivePermitFixtureReads(scope));
        return fixture;
    }

    // This isolated storage fixture supplies read authority; actual API/RBAC is qualified separately.
    private sealed class ActivePermitFixtureReads(PermitApplicationDataScope scope) : IPPDMRowAccessPolicy
    {
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type, string table) =>
            Task.FromResult<IReadOnlyList<IReadOnlyList<AppFilter>>>([[new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }]]);
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type, string table) => Task.FromResult<IReadOnlyList<AppFilter>>([]);
        public Task ValidateRowsAsync(Type type, string table, IReadOnlyList<object> rows)
        {
            Assert.All(rows, row =>
            {
                Assert.Equal(type, row.GetType());
                Assert.Equal("Y", ((IPPDMEntity)row).ACTIVE_IND);
                if (row is PERMIT_APPLICATION application)
                {
                    Assert.Equal(scope.Field.Company.TenantId, application.OWNER_TENANT_ID);
                    Assert.Equal(scope.Field.Company.BusinessAssociateId, application.OWNER_COMPANY_ID);
                    Assert.Equal(scope.Field.FieldId, application.FIELD_ID);
                }
            });
            return Task.CompletedTask;
        }
        public Task<string?> GetEffectiveAccessLevelAsync(Type type, string table, object row, bool inherited = true) => Task.FromResult<string?>("READ");
        public Task ValidateMutationAsync(Type type, string table, object row, object? previous, PPDMRowMutation mutation, string? actor) => throw new UnauthorizedAccessException("Read-only fixture policy.");
    }
}
#endif
