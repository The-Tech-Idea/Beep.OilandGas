#if RECORDED_APPROVAL_HTTP_FOCUSED
using System.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using Beep.OilandGas.PermitsAndApplications.Services;
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
public sealed class PermitProviderQualificationTests
{
    [Fact]
    public Task SetupInstalledSqliteMigratesPermitApplicationLedger() => WithInstalledLedgerAsync((_, _) => Task.CompletedTask);

    private async Task WithInstalledLedgerAsync(Func<IDMEEditor, IDataSource, Task> qualify)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-permit-provider-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "permits.db");
        var config = new Mock<IConfigEditor>();
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var classes = new List<AssemblyClassDefinition>();
        var connection = new ConnectionProperties { ConnectionName = "permit-provider", DatabaseType = DataSourceType.SqlLite,
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
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            Type[] types = [typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND)];
            var plan = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
            Assert.Equal(3, plan.Operations.Count(x => x.Kind == MigrationPlanOperationKind.CreateEntity));
            migration.ApproveMigrationPlan(plan, "permit-provider-qualification");
            var applied = migration.ExecuteMigrationPlan(plan);
            Assert.True(applied.Success, applied.Message + " | " + errors.Message);
            foreach (var type in types)
            {
                Assert.True(source.CheckEntityExist(type.Name));
                var shape = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
                Assert.NotNull(shape);
                var key = Assert.Single(shape.Fields.Where(field => field.IsKey));
                Assert.Equal(type.Name + "_ID", key.FieldName, ignoreCase: true);
                foreach (var name in new[] { "OWNER_TENANT_ID", "OWNER_COMPANY_ID", "FIELD_ID" }.Where(name => type != typeof(PERMIT_STATUS_HISTORY)))
                    Assert.Single(shape.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            var header = source.GetEntityStructure("PERMIT_APPLICATION", true);
            foreach (var name in new[] { "STATUS", "SUBMITTED_BY_USER_ID", "FACILITY_TYPE" })
                Assert.Single(header.Fields, field => field.FieldName.Equals(name, StringComparison.OrdinalIgnoreCase));
            var replay = migration.BuildMigrationPlanForTypes(types, detectRelationships: false);
            // Beep retains one explicit UpToDate operation per unchanged entity.
            Assert.Equal(types.Length, replay.Operations.Count);
            Assert.All(replay.Operations, operation => Assert.Equal(MigrationPlanOperationKind.UpToDate, operation.Kind));
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

    [Fact]
    public Task InstalledPermitProviderRollsBackThenRecoversExactNativeSubmission() => WithInstalledLedgerAsync(async (editor, source) =>
    {
        var rowType = source.GetEntityType(nameof(PERMIT_APPLICATION));
        var underscoreType = DMTypeBuilder.GetOrCreateType(editor, "Beep.permit_provider", null, nameof(PERMIT_APPLICATION), source.GetEntityStructure(nameof(PERMIT_APPLICATION), true).Fields);
        Assert.NotEqual(rowType, underscoreType);
        var scope = new PermitApplicationDataScope(new(new("tenant", "company"), new("core", "core-binding"), "author", "selection", "field-a"),
            new(source.DatasourceName, "permit-binding"));
        var metadata = new Mock<IPPDMMetadataRepository>();
        foreach (var type in new[] { typeof(PERMIT_APPLICATION), typeof(PERMIT_STATUS_HISTORY), typeof(PERMIT_APPLICATION_COMMAND) })
        {
            var key = type.Name + "_ID";
            metadata.Setup(x => x.GetTableMetadataAsync(type.Name)).ReturnsAsync(new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(type.Name)).ReturnsAsync([key]);
            metadata.Setup(x => x.GetPrimaryKeyColumnAsync(type.Name)).ReturnsAsync(key);
        }
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(x => x.GetActiveIndicatorYes()).Returns("Y");
        defaults.Setup(x => x.FormatIdForTable(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object id) => Convert.ToString(id, System.Globalization.CultureInfo.InvariantCulture)!);
        var access = new Mock<ICompanyPermitApplicationDataAccess>();
        access.Setup(x => x.RequireReadScopeAsync()).ReturnsAsync(scope);
        access.Setup(x => x.ForRead(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<string?>())).Returns(new ActivePermitFixtureReads(scope));
        var allowWrites = true;
        access.Setup(x => x.WithCommandAuthorityAsync(It.IsAny<PermitApplicationCommand>(), It.IsAny<Func<Func<Task>, Task<bool>>>()))
            .Returns((PermitApplicationCommand _, Func<Func<Task>, Task<bool>> execute) => allowWrites ? execute(() => Task.CompletedTask) : throw new UnauthorizedAccessException("Fixture author permission removed."));
        var actualColumns = new CommonColumnHandler();
        var columns = new Mock<ICommonColumnHandler>();
        var interruptReceipt = true;
        var interrupted = false;
        columns.Setup(x => x.PrepareForUpdate(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) => actualColumns.PrepareForUpdate(row, actor));
        columns.Setup(x => x.PrepareForInsert(It.IsAny<IPPDMEntity>(), It.IsAny<string>())).Callback((IPPDMEntity row, string actor) =>
        {
            actualColumns.PrepareForInsert(row, actor);
            if (row is PERMIT_APPLICATION_COMMAND && interruptReceipt) { interrupted = true; throw new InvalidOperationException("Controlled interruption before receipt persistence."); }
        });
        var seed = new PERMIT_APPLICATION { PERMIT_APPLICATION_ID = "physical-permit", OWNER_TENANT_ID = "tenant", OWNER_COMPANY_ID = "company",
            FIELD_ID = "field-a", APPLICANT_ID = "Recorded applicant", ACTIVE_IND = "Y" };
        actualColumns.PrepareForInsert(seed, "author");
        Assert.Equal(Errors.Ok, source.InsertEntity(nameof(PERMIT_APPLICATION), seed).Flag);
        var service = new PermitApplicationLifecycleService(editor, columns.Object, defaults.Object, metadata.Object, access.Object);
        var review = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        var original = new PermitApplicationCommand(Guid.NewGuid(), review, "SUBMIT", "Reviewed the recorded filing before submission");
        Assert.Null(await service.RecoverCommandAsync(original));
        var interruptedCommand = await Assert.ThrowsAnyAsync<Exception>(() => service.RecordCommandAsync(original));
        Assert.True(interrupted, "The failure must occur after the header and history writes, at receipt preparation. Actual: " + interruptedCommand);
        PPDMDataSourceTransaction.RequireIdle(source);
        var rolledBack = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Equal(review, rolledBack);
        Assert.Null(await service.RecoverCommandAsync(original));
        interruptReceipt = false;
        var saved = await service.RecordCommandAsync(original);
        saved.Require();
        Assert.Equal(original, saved.Original);
        Assert.Equal(Beep.OilandGas.Models.Data.PermitsAndApplications.PermitApplicationStatus.Submitted, saved.ReadApplication().STATUS);
        allowWrites = false;
        Assert.Equal(saved, await service.RecoverCommandAsync(original));
        Assert.Equal(saved, await service.RecordCommandAsync(original));
        var current = await service.ReviewSourceAsync(seed.PERMIT_APPLICATION_ID);
        Assert.Single(current.ReadHistory());
        var receiptRepository = new PPDMGenericRepository(editor, actualColumns, defaults.Object, metadata.Object, typeof(PERMIT_APPLICATION_COMMAND), source.DatasourceName,
            nameof(PERMIT_APPLICATION_COMMAND), rowAccess: new ActivePermitFixtureReads(scope));
        Assert.Single(await receiptRepository.GetAsync([]));
        PPDMDataSourceTransaction.RequireIdle(source);
    });

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
