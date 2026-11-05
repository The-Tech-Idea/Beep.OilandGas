using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
#if BEEP_SOURCE_TEST
using System.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Beep.OilandGas.Repository;
using Moq;
using TheTechIdea.Beep;
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
using TheTechIdea.Beep.Helpers.DataTypesHelpers;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyFieldObligationSqliteTests
{
    [Fact]
    public void BeepTypeLookupRefreshesWhenAnEditorChangesBindingOrMappings()
    {
        var editor = new Mock<IDMEEditor>(); var config = new Mock<IConfigEditor>();
        var first = new DatatypeMapping { DataSourceName = "First", DataType = "custom", NetDataType = "System.String" };
        var second = new DatatypeMapping { DataSourceName = "Second", DataType = "custom", NetDataType = "System.DateTime" };
        config.SetupGet(x => x.DataTypesMap).Returns(new List<DatatypeMapping> { first, second });
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.Setup(x => x.GetDataSource("company_core")).Returns(Mock.Of<IDataSource>());
        var binding = "First";
        editor.Setup(x => x.GetDataSourceClass("company_core")).Returns(() => new AssemblyClassDefinition { className = binding });
        Assert.Equal("System.String", DataTypeMappingLookup.GetDataType("company_core", "custom", editor.Object));
        binding = "Second";
        Assert.Equal("System.DateTime", DataTypeMappingLookup.GetDataType("company_core", "custom", editor.Object));
        second.NetDataType = "System.Decimal";
        Assert.Equal("System.Decimal", DataTypeMappingLookup.GetDataType("company_core", "custom", editor.Object));
    }

    [Fact]
    public async Task RequiredProjectionNeverFallsBackToAnUnacknowledgedDatasourceRead()
    {
        var source = new Mock<IDataSource>(); var editor = new Mock<IDMEEditor>();
        editor.Setup(x => x.GetDataSource("core")).Returns(source.Object);
        var repository = new PPDMGenericRepository(editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), typeof(R_OBLIG_TYPE), "core", "R_OBLIG_TYPE", null, null,
            ["LAND_OBLIG_TYPE", "ACTIVE_IND"], ["EFFECTIVE_DATE", "EXPIRY_DATE"]);
        await Assert.ThrowsAsync<NotSupportedException>(() => repository.GetAsync([new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }]));
        source.Verify(x => x.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()), Times.Never);
    }

    [Fact]
    public async Task PairedCommandUsesBeepMigratedSqliteDatabaseAndRecoversExactRetry()
        => await RunPairedCommand(false);

    [SetupDriverBeepLoaderTests.BundledSqlServerFact]
    public async Task PairedCommandUsesBeepMigratedSqlServerDatabaseAndRecoversExactRetry()
        => await RunPairedCommand(true);

    private static async Task RunPairedCommand(bool sqlServer)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-obligation-native-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "company.db"); var driver = sqlServer ? ConnectionHelper.CreateSqlServerConfig() : ConnectionHelper.CreateSQLiteConfig();
        var databaseName = "BeepOilGas_Obligation_" + Guid.NewGuid().ToString("N");
        var connectionString = sqlServer ? $"Server=(localdb)\\MSSQLLocalDB;Database={databaseName};Integrated Security=true;TrustServerCertificate=true" : "Data Source=" + path;
        var connection = new ConnectionProperties { ConnectionName = "company_core", DatabaseType = sqlServer ? DataSourceType.SqlServer : DataSourceType.SqlLite,
            ConnectionString = connectionString, Database = sqlServer ? databaseName : path, GuidID = Guid.NewGuid().ToString() };
        await using var creation = sqlServer ? new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connectionString).Options) : null;
        if (creation is not null) await creation.Database.EnsureCreatedAsync();
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(directory, new TheTechIdea.Beep.JsonLoaderService.JsonLoader()));
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes); config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(sqlServer ? TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSqlServerDataTypesMapping() : TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
        try
        {
            await new SetupDriverService(editor, new ConfigurationBuilder().Build(), NullLogger<SetupDriverService>.Instance).RestoreCachedAsync(default);
            var source = editor.GetDataSource(connection.ConnectionName); Assert.NotNull(source); Assert.Equal(ConnectionState.Open, source.Openconnection());
            var definitions = new (Type Type, string[] Keys, string[] Fields)[] {
                (typeof(FIELD), ["FIELD_ID"], ["FIELD_ID", "ACTIVE_IND"]),
                (typeof(R_OBLIG_TYPE), ["LAND_OBLIG_TYPE"], ["LAND_OBLIG_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_OBLIG_COMPONENT_TYPE), ["OBLIGATION_COMPONENT_TYPE"], ["OBLIGATION_COMPONENT_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(HSE_INCIDENT_SET), ["INCIDENT_SET_ID"], ["INCIDENT_SET_ID", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(HSE_INCIDENT_TYPE), ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID"], ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(HSE_INCIDENT_SEVERITY), ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID"], ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_HSE_INCIDENT_DETAIL), ["DETAIL_TYPE"], ["DETAIL_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_HSE_COMP_ROLE), ["COMPONENT_ROLE"], ["COMPONENT_ROLE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_HSE_INCIDENT_COMP_TYPE), ["COMPONENT_TYPE"], ["COMPONENT_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_HSE_RESPONSE_TYPE), ["ACTION_TYPE"], ["ACTION_TYPE", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(R_INCIDENT_RESP_RESULT), ["RESPONSE_RESULT"], ["RESPONSE_RESULT", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]),
                (typeof(HSE_INCIDENT), ["INCIDENT_ID"], ["INCIDENT_ID", "INCIDENT_DATE", "REMARK", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                (typeof(HSE_INCIDENT_DETAIL), ["INCIDENT_ID", "DETAIL_OBS_NO"], ["INCIDENT_ID", "DETAIL_OBS_NO", "DETAIL_TYPE", "INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID", "INCIDENT_DATE", "REMARK", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                (typeof(HSE_INCIDENT_COMPONENT), ["INCIDENT_ID", "COMPONENT_OBS_NO"], ["INCIDENT_ID", "COMPONENT_OBS_NO", "FIELD_ID", "COMPONENT_ROLE", "COMPONENT_TYPE", "REMARK", "JURISDICTION", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                (typeof(HSE_INCIDENT_RESPONSE), ["INCIDENT_ID", "RESPONSE_OBS_NO"], ["INCIDENT_ID", "RESPONSE_OBS_NO", "RESPONSE_TYPE", "RESPONSE_RESULT", "EFFECTIVE_DATE", "ACTIVE_IND", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                (typeof(OBLIGATION), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO"], ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "ACTIVE_IND", "OBLIGATION_TYPE", "DESCRIPTION", "CRITICAL_DATE", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]),
                (typeof(OBLIGATION_COMPONENT), ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO"], ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO", "FIELD_ID", "ACTIVE_IND", "OBLIGATION_COMPONENT_TYPE", "SOURCE", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]) };
            var model = new MigrationModel { Source = "FocusedTest", SourceId = "field-obligation-command" };
            foreach (var (type, keys, fields) in definitions)
                model.Entities.Add(type.FullName!, new MigrationModelEntity { ClrTypeFullName = type.FullName!, TableName = type.Name,
                    Properties = fields.Select(name => new MigrationModelProperty { PropertyName = name, ColumnName = name,
                        FieldType = (Nullable.GetUnderlyingType(type.GetProperty(name)!.PropertyType) ?? type.GetProperty(name)!.PropertyType).FullName!,
                        IsPrimaryKey = keys.Contains(name), IsNullable = !keys.Contains(name), MaxLength = keys.Contains(name) ? 128 : 512 }).ToList() });
            var intentType = typeof(HSE_INCIDENT_REPORT_INTENT);
            var intentStructure = new ClassCreator(editor).ConvertToEntityStructure(intentType);
            model.Entities.Add(intentType.FullName!, new MigrationModelEntity { ClrTypeFullName = intentType.FullName!, TableName = intentType.Name,
                Properties = intentStructure.Fields.Select(field => new MigrationModelProperty { PropertyName = field.FieldName, ColumnName = field.FieldName,
                    FieldType = field.Fieldtype, IsPrimaryKey = field.IsKey, IsNullable = field.AllowDBNull,
                    MaxLength = Math.Max(field.Size, field.Size1) > 0 ? Math.Max(field.Size, field.Size1) : null }).ToList() });
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            var plan = migration.BuildMigrationPlanForModel(model, detectRelationships: false); migration.ApproveMigrationPlan(plan, "local-test");
            var result = migration.ExecuteMigrationPlan(plan); Assert.True(result.Success, result.Message + " | " + errors.Message);
            Assert.Equal(Errors.Ok, source.InsertEntity("FIELD", new FIELD { FIELD_ID = "field", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("R_OBLIG_TYPE", new R_OBLIG_TYPE { LAND_OBLIG_TYPE = "REPORT", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("R_OBLIG_COMPONENT_TYPE", new R_OBLIG_COMPONENT_TYPE { OBLIGATION_COMPONENT_TYPE = "FIELD", ACTIVE_IND = "Y" }).Flag);
            var projection = Assert.IsAssignableFrom<IOwnedEntityProjectionDataSource>(source);
            Assert.True(projection.SupportsOwnedEntityProjections);
            Assert.Equal(Errors.Ok, source.InsertEntity("HSE_INCIDENT_SET", new HSE_INCIDENT_SET { INCIDENT_SET_ID = "KUWAIT", ACTIVE_IND = "Y", EFFECTIVE_DATE = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified) }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("HSE_INCIDENT_TYPE", new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "KUWAIT", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("HSE_INCIDENT_SEVERITY", new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "KUWAIT", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_2", ACTIVE_IND = "Y" }).Flag);
            var incidentProjection = await projection.ReadEntityProjectionAsync("HSE_INCIDENT_SEVERITY", [new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }],
                ["INCIDENT_SET_ID", "INCIDENT_TYPE_ID", "INCIDENT_SEVERITY_ID", "ACTIVE_IND", "EFFECTIVE_DATE", "EXPIRY_DATE"]);
            Assert.Equal(Errors.Ok, incidentProjection.Outcome.Flag);
            var incidentValues = Assert.IsAssignableFrom<IDictionary<string, object>>(Assert.Single(incidentProjection.Rows));
            Assert.Equal("KUWAIT", incidentValues["INCIDENT_SET_ID"]); Assert.Equal("SPILL", incidentValues["INCIDENT_TYPE_ID"]);
            Assert.Equal("TIER_2", incidentValues["INCIDENT_SEVERITY_ID"]); Assert.True(incidentValues.ContainsKey("EXPIRY_DATE")); Assert.Null(incidentValues["EXPIRY_DATE"]);
            var intentContext = new FieldSelectionReview(new CompanyScope("tenant", "company"), new AssetDatabaseScope(connection.ConnectionName, new string('A', 64)), "test-admin", "revision", "field");
            var originalIntent = new IncidentReportIntent(Guid.NewGuid(), intentContext, new("SET", "SPILL", "TIER_2"), new string('A', 64),
                new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "Site", "Description", "الكويت");
            IncidentReportSchema.Validate(source, originalIntent, "Beep.OilandGas.HSE");
            var missingJurisdiction = new Mock<IDataSource>();
            missingJurisdiction.Setup(s => s.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure request, bool refresh) => {
                var actual = source.GetEntityStructure(request, refresh);
                return request.EntityName == nameof(HSE_INCIDENT_COMPONENT)
                    ? new EntityStructure { Fields = actual.Fields.Where(field => field.FieldName != "JURISDICTION").ToList() }
                    : actual;
            });
            Assert.Throws<NativeSchemaCompatibilityException>(() => IncidentReportSchema.Validate(missingJurisdiction.Object, originalIntent, "Beep.OilandGas.HSE"));
            var intentRow = HSE_INCIDENT_REPORT_INTENT.Create(originalIntent); new CommonColumnHandler().PrepareForInsert(intentRow, "test-admin");
            Assert.Equal(Errors.Ok, source.InsertEntity(intentType.Name, intentRow).Flag);
            var intentMetadata = new Mock<IPPDMMetadataRepository>();
            intentMetadata.Setup(x => x.GetTableMetadataAsync(intentType.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = intentType.Name, EntityTypeName = intentType.FullName!, PrimaryKeyColumn = "HSE_INCIDENT_REPORT_INTENT_ID" });
            intentMetadata.Setup(x => x.GetPrimaryKeyColumnsAsync(intentType.Name)).ReturnsAsync(["HSE_INCIDENT_REPORT_INTENT_ID"]);
            var intentRepository = new PPDMGenericRepository(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), intentMetadata.Object, intentType,
                connection.ConnectionName, intentType.Name, null, null, ["HSE_INCIDENT_REPORT_INTENT_ID"],
                ["OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "INCIDENT_ID", "COMMAND_JSON", "COMMAND_HASH", "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE"]);
            var retained = Assert.Single((await intentRepository.GetAsync([new AppFilter { FieldName = "HSE_INCIDENT_REPORT_INTENT_ID", Operator = "=", FilterValue = intentRow.HSE_INCIDENT_REPORT_INTENT_ID }])).Cast<HSE_INCIDENT_REPORT_INTENT>());
            Assert.Equal(intentRow.OWNER_TENANT_ID, retained.OWNER_TENANT_ID); Assert.Equal(intentRow.OWNER_COMPANY_BA_ID, retained.OWNER_COMPANY_BA_ID);
            Assert.Equal("Y", retained.ACTIVE_IND); Assert.Equal("test-admin", retained.ROW_CREATED_BY); Assert.Equal("test-admin", retained.ROW_CHANGED_BY);
            Assert.Equal(intentRow.COMMAND_JSON, retained.COMMAND_JSON); Assert.Equal(intentRow.COMMAND_HASH, retained.COMMAND_HASH);
            Assert.NotNull(retained.ROW_CREATED_DATE); Assert.NotNull(retained.ROW_CHANGED_DATE);
            if (!sqlServer)
            {
                Assert.Equal(DateTimeKind.Utc, retained.ROW_CREATED_DATE.Value.Kind); Assert.Equal(DateTimeKind.Utc, retained.ROW_CHANGED_DATE.Value.Kind);
                Assert.Equal(originalIntent, retained.Restore(intentContext.Company, "test-admin"));
                var unmarkedProjection = await projection.ReadEntityProjectionAsync("HSE_INCIDENT_SET", [new AppFilter { FieldName = "INCIDENT_SET_ID", Operator = "=", FilterValue = "KUWAIT" }], ["INCIDENT_SET_ID", "EFFECTIVE_DATE"]);
                Assert.Equal(Errors.Ok, unmarkedProjection.Outcome.Flag);
                var unmarked = Assert.IsAssignableFrom<IDictionary<string, object>>(Assert.Single(unmarkedProjection.Rows));
                Assert.Equal(DateTimeKind.Unspecified, Assert.IsType<DateTime>(unmarked["EFFECTIVE_DATE"]).Kind);
            }
            else if (retained.ROW_CREATED_DATE.Value.Kind == DateTimeKind.Unspecified || retained.ROW_CHANGED_DATE.Value.Kind == DateTimeKind.Unspecified)
                Assert.Throws<InvalidOperationException>(() => retained.Restore(intentContext.Company, "test-admin"));
            else Assert.Equal(originalIntent, retained.Restore(intentContext.Company, "test-admin"));
            var rollbackIntent = HSE_INCIDENT_REPORT_INTENT.Create(originalIntent with { IntentId = Guid.NewGuid() });
            new CommonColumnHandler().PrepareForInsert(rollbackIntent, "test-admin");
            await Assert.ThrowsAsync<InvalidOperationException>(() => PPDMDataSourceTransaction.ExecuteAsync(source, async () => {
                PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                Assert.Equal(Errors.Ok, source.InsertEntity(intentType.Name, rollbackIntent).Flag);
                var inside = await intentRepository.GetAsync([new AppFilter { FieldName = "HSE_INCIDENT_REPORT_INTENT_ID", Operator = "=", FilterValue = rollbackIntent.HSE_INCIDENT_REPORT_INTENT_ID }]);
                Assert.Single(inside);
                throw new InvalidOperationException("Exercise rollback.");
            }));
            Assert.Empty(await intentRepository.GetAsync([new AppFilter { FieldName = "HSE_INCIDENT_REPORT_INTENT_ID", Operator = "=", FilterValue = rollbackIntent.HSE_INCIDENT_REPORT_INTENT_ID }]));
            var observed = await projection.ReadEntityProjectionAsync("R_OBLIG_TYPE", [new AppFilter { FieldName = "LAND_OBLIG_TYPE", Operator = "=", FilterValue = "REPORT" }], ["LAND_OBLIG_TYPE", "EFFECTIVE_DATE", "EXPIRY_DATE"]);
            Assert.Equal(Errors.Ok, observed.Outcome.Flag);
            var raw = Assert.IsAssignableFrom<IDictionary<string, object>>(Assert.Single(observed.Rows));
            Assert.Equal("REPORT", raw["LAND_OBLIG_TYPE"]); Assert.True(raw.ContainsKey("EXPIRY_DATE")); Assert.Null(raw["EXPIRY_DATE"]);
            var missing = await projection.ReadEntityProjectionAsync("R_OBLIG_TYPE", [], ["LAND_OBLIG_TYPE", "NOT_A_COLUMN"]);
            Assert.Equal(Errors.Failed, missing.Outcome.Flag); Assert.Empty(missing.Rows);
            var duplicated = await projection.ReadEntityProjectionAsync("R_OBLIG_TYPE", [], ["LAND_OBLIG_TYPE", "land_oblig_type"]);
            Assert.Equal(Errors.Failed, duplicated.Outcome.Flag); Assert.Empty(duplicated.Rows);
            await PPDMDataSourceTransaction.ExecuteAsync(source, async () => {
                PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                var inside = await projection.ReadEntityProjectionAsync("R_OBLIG_TYPE", [], ["LAND_OBLIG_TYPE", "EXPIRY_DATE"]);
                Assert.Equal(Errors.Ok, inside.Outcome.Flag); Assert.Single(inside.Rows);
                PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            });
            var metadata = new Mock<IPPDMMetadataRepository>();
            metadata.Setup(x => x.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name) => new PPDMTableMetadata { TableName = name, EntityTypeName = name, PrimaryKeyColumn = string.Join(",", definitions.Single(d => d.Type.Name == name).Keys) });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string name) => definitions.Single(d => d.Type.Name == name).Keys);
            var scope = new CompanyScope("tenant", "company"); var database = new AssetDatabaseScope(connection.ConnectionName, new string('A', 64));
            var review = new FieldSelectionReview(scope, database, "test-admin", "revision", "field");
            var boundary = new CompanyModuleConnection(scope, database); var rowPolicy = new Mock<IPPDMRowAccessPolicy>();
            rowPolicy.Setup(p => p.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", It.IsAny<object>(), true)).ReturnsAsync("WRITE");
            if (!sqlServer)
            {
                Assert.Equal(Errors.Ok, source.InsertEntity("R_HSE_INCIDENT_DETAIL", new R_HSE_INCIDENT_DETAIL { DETAIL_TYPE = "PRIMARY", ACTIVE_IND = "Y" }).Flag);
                Assert.Equal(Errors.Ok, source.InsertEntity("R_HSE_COMP_ROLE", new R_HSE_COMP_ROLE { COMPONENT_ROLE = Beep.OilandGas.Models.Constants.HseReferenceCodes.FieldContext.PrimaryRole, ACTIVE_IND = "Y" }).Flag);
                Assert.Equal(Errors.Ok, source.InsertEntity("R_HSE_INCIDENT_COMP_TYPE", new R_HSE_INCIDENT_COMP_TYPE { COMPONENT_TYPE = Beep.OilandGas.Models.Constants.HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" }).Flag);
                Assert.Equal(Errors.Ok, source.InsertEntity("R_HSE_RESPONSE_TYPE", new R_HSE_RESPONSE_TYPE { ACTION_TYPE = "STATUS", ACTIVE_IND = "Y" }).Flag);
                Assert.Equal(Errors.Ok, source.InsertEntity("R_INCIDENT_RESP_RESULT", new R_INCIDENT_RESP_RESULT { RESPONSE_RESULT = Beep.OilandGas.Models.Data.HSE.IncidentState.Reported, ACTIVE_IND = "Y" }).Flag);
                PPDMTableMetadata Definition(string name)
                {
                    if (name == intentType.Name) return new() { TableName = name, EntityTypeName = intentType.FullName!, PrimaryKeyColumn = "HSE_INCIDENT_REPORT_INTENT_ID" };
                    var value = new PPDMTableMetadata { TableName = name, EntityTypeName = name, PrimaryKeyColumn = string.Join(",", definitions.Single(d => d.Type.Name == name).Keys) };
                    void Foreign(string column, string table, string key) => value.ForeignKeys.Add(new PPDMForeignKey { ForeignKeyColumn = column, ReferencedTable = table, ReferencedPrimaryKey = key });
                    if (name == "HSE_INCIDENT_DETAIL") { Foreign("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"); Foreign("DETAIL_TYPE", "R_HSE_INCIDENT_DETAIL", "DETAIL_TYPE"); }
                    if (name == "HSE_INCIDENT_COMPONENT") { Foreign("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"); Foreign("FIELD_ID", "FIELD", "FIELD_ID"); }
                    if (name == "HSE_INCIDENT_RESPONSE") { Foreign("INCIDENT_ID", "HSE_INCIDENT", "INCIDENT_ID"); Foreign("RESPONSE_TYPE", "R_HSE_RESPONSE_TYPE", "ACTION_TYPE"); Foreign("RESPONSE_RESULT", "R_INCIDENT_RESP_RESULT", "RESPONSE_RESULT"); }
                    return value;
                }
                metadata.Setup(x => x.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name) => Definition(name));
                metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string name) => name == intentType.Name ? ["HSE_INCIDENT_REPORT_INTENT_ID"] : definitions.Single(d => d.Type.Name == name).Keys);
                var report = originalIntent with { IntentId = Guid.NewGuid() };
                var reportWriter = new PPDMIncidentReportWriter(boundary, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, rowPolicy.Object);
                Assert.Equal(report.IncidentId(), await reportWriter.CreateAsync(report, () => Task.CompletedTask));
                Assert.Equal(report.IncidentId(), await reportWriter.CreateAsync(report, () => Task.CompletedTask));
                await Assert.ThrowsAnyAsync<Exception>(() => reportWriter.CreateAsync(report with { Description = "Changed" }, () => Task.CompletedTask));
                var interruptedReport = report with { IntentId = Guid.NewGuid() }; var authorityChecks = 0;
                await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reportWriter.CreateAsync(interruptedReport, () => ++authorityChecks == 4
                    ? Task.FromException(new UnauthorizedAccessException("Authority revoked before the second incident record.")) : Task.CompletedTask));
                Assert.Equal(4, authorityChecks);
                foreach (var table in new[] { "HSE_INCIDENT", "HSE_INCIDENT_DETAIL", "HSE_INCIDENT_COMPONENT", "HSE_INCIDENT_RESPONSE", "HSE_INCIDENT_REPORT_INTENT" })
                {
                    var remaining = await projection.ReadEntityProjectionAsync(table, [new AppFilter { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = interruptedReport.IncidentId() }], ["INCIDENT_ID"]);
                    Assert.Equal(Errors.Ok, remaining.Outcome.Flag); Assert.Empty(remaining.Rows);
                }
                Assert.Equal(interruptedReport.IncidentId(), await reportWriter.CreateAsync(interruptedReport, () => Task.CompletedTask));
            }
            var writer = new CompanyFieldObligationWriter(boundary, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, rowPolicy.Object);
            var request = new CreateFieldObligation(review, Guid.NewGuid(), "REPORT", "FIELD", "Recorded FIELD obligation", new DateTime(2030, 1, 2));
            var authority = new CompanyFieldObligationCommandScope(review, () => Task.CompletedTask, () => Task.CompletedTask);
            var options = await writer.OptionsAsync(authority);
            Assert.Equal(new[] { "REPORT" }, options.ObligationTypes); Assert.Equal(new[] { "FIELD" }, options.ComponentTypes);
            var created = await writer.CreateAsync(request, authority); var retry = await writer.CreateAsync(request, authority);
            Assert.Equal(created, retry);
            await Assert.ThrowsAnyAsync<Exception>(() => writer.CreateAsync(request with { Description = "Changed" }, authority));
            var interrupted = request with { IntentId = Guid.NewGuid() }; var checks = 0;
            var revoked = new CompanyFieldObligationCommandScope(review, () => ++checks >= 6
                ? Task.FromException(new UnauthorizedAccessException("Authority revoked after the first insert.")) : Task.CompletedTask, () => Task.CompletedTask);
            await Assert.ThrowsAnyAsync<Exception>(() => writer.CreateAsync(interrupted, revoked));
            Assert.True(checks >= 6);
            var recovered = await writer.CreateAsync(interrupted, authority);
            Assert.Equal("FO-" + interrupted.IntentId.ToString("N").ToUpperInvariant(), recovered.ObligationId);
        }
        finally
        {
            editor.GetDataSource(connection.ConnectionName)?.Closeconnection();
            handler.GetType(driver.DbConnectionType)?.GetMethod("ClearAllPools", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            if (creation is not null) await creation.Database.EnsureDeletedAsync();
            try { Directory.Delete(directory, true); } catch (IOException error) { Console.WriteLine("Temporary SQLite directory remains locked: " + error.Message); }
        }
    }
}
#endif
