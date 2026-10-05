using System.Data;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Data.SqlClient;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class LocalDbDriverTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task BeepMigrationAndWritesStayOnTheSelectedNamedDatabase()
    {
        var database = $"BeepOilGas_Module_{Guid.NewGuid():N}";
        var otherDatabase = $"BeepOilGas_Module_{Guid.NewGuid():N}";
        output.WriteLine($"Retained module test database: {database}");
        output.WriteLine($"Retained second module test database: {otherDatabase}");
        using (var master = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true"))
        {
            master.Open();
            using var create = master.CreateCommand();
            create.CommandText = $"CREATE DATABASE [{database}]";
            create.ExecuteNonQuery();
            create.CommandText = $"CREATE DATABASE [{otherDatabase}]";
            create.ExecuteNonQuery();
        }
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        var properties = new ConnectionProperties
        {
            ConnectionName = database, ConnectionString = connectionString,
            DatabaseType = DataSourceType.SqlServer, Category = DatasourceCategory.RDBMS,
            Host = "(localdb)\\MSSQLLocalDB", Database = database, IntegratedSecurity = true
        };
        var driver = ConnectionHelper.CreateSqlServerConfig();
        driver.ConnectionString = connectionString;
        var otherConnectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={otherDatabase};Integrated Security=true;TrustServerCertificate=true";
        var otherProperties = new ConnectionProperties
        {
            ConnectionName = otherDatabase, ConnectionString = otherConnectionString,
            DatabaseType = DataSourceType.SqlServer, Category = DatasourceCategory.RDBMS,
            Host = "(localdb)\\MSSQLLocalDB", Database = otherDatabase, IntegratedSecurity = true
        };
        var otherDriver = ConnectionHelper.CreateSqlServerConfig();
        otherDriver.ConnectionString = otherConnectionString;
        var config = new Mock<IConfigEditor>();
        // Put the unselected target first to catch accidental first-connection fallback.
        config.SetupGet(x => x.DataConnections).Returns([otherProperties, properties]);
        config.SetupGet(x => x.DataDriversClasses).Returns([driver]);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(Mock.Of<IDMLogger>(),
            Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter,
                List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? "dbo", filter, provider));
        var loader = new Mock<IAssemblyHandler>();
        loader.Setup(x => x.GetInstance(It.IsAny<string>())).Returns(() => new SqlConnection());
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.SetupGet(x => x.assemblyHandler).Returns(loader.Object);
        editor.SetupGet(x => x.ErrorObject).Returns(new ErrorsInfo());
        var source = new SQLServerDataSource(database, Mock.Of<IDMLogger>(), editor.Object, DataSourceType.SqlServer, new ErrorsInfo());
        editor.SetupGet(x => x.classCreator).Returns(new ClassCreator(editor.Object));
        editor.SetupGet(x => x.typesHelper).Returns(new DataTypesHelper(editor.Object));
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSqlServerDataTypesMapping());
        editor.Setup(x => x.GetDataSourceClass(database)).Returns(new AssemblyClassDefinition { className = nameof(SQLServerDataSource) });
        editor.SetupGet(x => x.Utilfunction).Returns(new TheTechIdea.Beep.Utils.Util(Mock.Of<IDMLogger>(), new ErrorsInfo(), config.Object) { DME = editor.Object });
        var providerAssembly = typeof(System.Data.SqlClient.SqlConnection).Assembly;
        loader.Setup(x => x.GetType(It.IsAny<string>())).Returns((string name) => providerAssembly.GetType(name, throwOnError: true)!);
        loader.Setup(x => x.GetInstance(It.IsAny<string>())).Returns((string name) =>
            Activator.CreateInstance(providerAssembly.GetType(name, throwOnError: true)!)!);
        source.Dataconnection.ConnectionProp = properties;
        source.Dataconnection.DataSourceDriver = driver;
        editor.Setup(x => x.GetDataSource(database)).Returns(source);
        var otherSource = new SQLServerDataSource(otherDatabase, Mock.Of<IDMLogger>(), editor.Object,
            DataSourceType.SqlServer, new ErrorsInfo());
        otherSource.Dataconnection.ConnectionProp = otherProperties;
        otherSource.Dataconnection.DataSourceDriver = otherDriver;
        editor.Setup(x => x.GetDataSource(otherDatabase)).Returns(otherSource);
        editor.Setup(x => x.GetDataSourceClass(otherDatabase)).Returns(new AssemblyClassDefinition { className = nameof(SQLServerDataSource) });
        try
        {
            MigrationConnectionTarget.Validate(editor.Object, otherSource, otherDatabase);
            Assert.Equal(ConnectionState.Open, otherSource.Openconnection());
            MigrationConnectionTarget.Validate(editor.Object, source, database);
            Assert.Equal(ConnectionState.Open, source.Openconnection());
            var module = new Beep.OilandGas.OilProperties.Modules.OilPropertiesModule();
            ModuleSchemaBoundary.Validate(module.EntityTypes);
            var repositoryDatabase = $"BeepOilGas_Repository_{Guid.NewGuid():N}";
            output.WriteLine($"Retained repository test database: {repositoryDatabase}");
            var repositoryConnection = $"Server=(localdb)\\MSSQLLocalDB;Database={repositoryDatabase};Integrated Security=true;TrustServerCertificate=true";
            using var repository = new SqlServerRepositoryDbContext(
                new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(repositoryConnection).Options);
            await repository.Database.MigrateAsync();
            Assert.Equal(repository.Database.GetMigrations(), await repository.Database.GetAppliedMigrationsAsync());
            var resolver = new ModuleConnectionResolver(repository, editor.Object);
            var setup = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
                Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),
                Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),
                Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),
                new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), moduleSetupOrchestrator: new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance),
                migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync);
            var controller = new ModuleRepositoryController(repository, editor.Object, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter());
            var bound = Assert.IsType<OkObjectResult>(await controller.Bind(module.ModuleId, new(database, null), default));
            var binding = Assert.IsType<ModuleDatabaseBinding>(bound.Value);
            Assert.Equal(database, await resolver.ResolveAsync(module.ModuleId));
            var plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp);
            output.WriteLine($"Plan result: {plan.Success}: {plan.Message}; {string.Join("; ", plan.DryRunDiagnostics ?? [])}");
            Assert.True(plan.Success, plan.Message);
            var execution = new SchemaMigrationExecuteRequest
            {
                PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash
            };
            Assert.False((await setup.ExecuteSchemaMigrationPlanAsync(execution)).Success);
            using (var beforeApproval = new SqlConnection(connectionString))
            {
                beforeApproval.Open();
                using var count = beforeApproval.CreateCommand();
                count.CommandText = "SELECT COUNT(*) FROM sys.tables";
                Assert.Equal(0, Convert.ToInt32(count.ExecuteScalar()));
            }
            Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ApprovedBy = "test-admin" })).Success);
            Assert.False((await setup.ExecuteSchemaMigrationPlanAsync(new()
            {
                PlanId = plan.PlanId, ExpectedPlanHash = "not-the-reviewed-hash", ExpectedManifestHash = plan.ManifestHash
            })).Success);
            var result = await setup.ExecuteSchemaMigrationPlanAsync(execution);
            output.WriteLine($"Migration result: {result.Success}: {result.Message}");
            output.WriteLine($"Datasource error: {source.ErrorObject.Message} {source.ErrorObject.Ex}");
            foreach (var call in editor.Invocations.Where(x => x.Method.Name == "AddLogMessage"))
                output.WriteLine(string.Join(" | ", call.Arguments));
            Assert.True(result.Success, result.Message);
            ModuleSchemaVerification.Verify(editor.Object, source, module.EntityTypes);
            using var otherInspect = new SqlConnection(otherConnectionString);
            otherInspect.Open();
            using var untouched = otherInspect.CreateCommand();
            untouched.CommandText = "SELECT COUNT(*) FROM sys.tables";
            Assert.Equal(0, Convert.ToInt32(untouched.ExecuteScalar()));
            using var inspect = new SqlConnection(connectionString);
            inspect.Open();
            using var query = inspect.CreateCommand();
            query.CommandText = "SELECT name FROM sys.tables ORDER BY name";
            using var reader = query.ExecuteReader();
            var tables = new List<string>();
            while (reader.Read()) tables.Add(reader.GetString(0));
            Assert.Equal(module.EntityTypes.Select(t => t.Name).OrderBy(x => x), tables);
            reader.Close();

            var composition = new Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION
            {
                OIL_COMPOSITION_ID = Guid.NewGuid().ToString(),
                COMPOSITION_NAME = "LocalDB persistence verification"
            };
            var inserted = source.InsertEntity(nameof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION), composition);
            Assert.True(inserted.Flag == Errors.Ok, $"Insert failed: {inserted.Message} {inserted.Ex}");
            using var readRecord = inspect.CreateCommand();
            readRecord.CommandText = "SELECT OIL_COMPOSITION_ID, COMPOSITION_NAME FROM OIL_COMPOSITION";
            using var persisted = readRecord.ExecuteReader();
            Assert.True(persisted.Read(), "BeepDM reported insert success but no row was persisted.");
            Assert.Equal(composition.OIL_COMPOSITION_ID, persisted.GetString(0));
            Assert.Equal(composition.COMPOSITION_NAME, persisted.GetString(1));
            Assert.False(persisted.Read(), "The isolated test should contain exactly one composition.");
            persisted.Close();

            var rebound = Assert.IsType<OkObjectResult>(await controller.Bind(module.ModuleId,
                new(otherDatabase, binding.ConcurrencyStamp), default));
            var otherBinding = Assert.IsType<ModuleDatabaseBinding>(rebound.Value);
            Assert.Equal(otherDatabase, await resolver.ResolveAsync(module.ModuleId));
            var stale = await setup.ExecuteSchemaMigrationPlanAsync(execution);
            Assert.False(stale.Success);
            Assert.Contains("binding", stale.Message, StringComparison.OrdinalIgnoreCase);
            var otherPlanned = Assert.IsType<OkObjectResult>(await controller.Plan(module.ModuleId,
                new(EnvironmentTier: "Development", ConcurrencyStamp: otherBinding.ConcurrencyStamp), default));
            var otherPlan = Assert.IsType<SchemaMigrationPlanResult>(otherPlanned.Value);
            Assert.True(otherPlan.Success, otherPlan.Message);
            Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = otherPlan.PlanId, ApprovedBy = "test-admin" })).Success);
            var otherResult = await setup.ExecuteSchemaMigrationPlanAsync(new()
            {
                PlanId = otherPlan.PlanId, ExpectedPlanHash = otherPlan.PlanHash, ExpectedManifestHash = otherPlan.ManifestHash
            });
            Assert.True(otherResult.Success, otherResult.Message);
            ModuleSchemaVerification.Verify(editor.Object, otherSource, module.EntityTypes);
            using (var otherTables = otherInspect.CreateCommand())
            {
                otherTables.CommandText = "SELECT name FROM sys.tables ORDER BY name";
                using var tableRows = otherTables.ExecuteReader();
                var names = new List<string>();
                while (tableRows.Read()) names.Add(tableRows.GetString(0));
                Assert.Equal(module.EntityTypes.Select(t => t.Name).OrderBy(x => x), names);
            }
            var otherComposition = new Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION
            {
                OIL_COMPOSITION_ID = Guid.NewGuid().ToString(),
                COMPOSITION_NAME = "Second named database"
            };
            var otherInsert = editor.Object.GetDataSource(otherDatabase).InsertEntity(
                nameof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION), otherComposition);
            Assert.True(otherInsert.Flag == Errors.Ok, otherInsert.Message);

            using var otherQuery = otherInspect.CreateCommand();
            otherQuery.CommandText = "SELECT OIL_COMPOSITION_ID, COMPOSITION_NAME FROM OIL_COMPOSITION";
            using var otherRows = otherQuery.ExecuteReader();
            Assert.True(otherRows.Read());
            Assert.Equal(otherComposition.OIL_COMPOSITION_ID, otherRows.GetString(0));
            Assert.Equal(otherComposition.COMPOSITION_NAME, otherRows.GetString(1));
            Assert.False(otherRows.Read());
            using var originalRows = readRecord.ExecuteReader();
            Assert.True(originalRows.Read());
            Assert.Equal(composition.OIL_COMPOSITION_ID, originalRows.GetString(0));
            Assert.Equal(composition.COMPOSITION_NAME, originalRows.GetString(1));
            Assert.False(originalRows.Read(), "Writing to the second target must not affect the first database.");

            using var reopenedRepository = new SqlServerRepositoryDbContext(
                new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(repositoryConnection).Options);
            var persistedBinding = await reopenedRepository.ModuleDatabases.SingleAsync();
            Assert.Equal(module.ModuleId, persistedBinding.ModuleId);
            Assert.Equal(otherDatabase, persistedBinding.ConnectionName);
            Assert.Empty(await reopenedRepository.Users.ToListAsync());
            await reopenedRepository.Database.OpenConnectionAsync();
            using var repositoryTables = reopenedRepository.Database.GetDbConnection().CreateCommand();
            repositoryTables.CommandText = "SELECT name FROM sys.tables ORDER BY name";
            using var repositoryRows = await repositoryTables.ExecuteReaderAsync();
            var repositoryNames = new List<string>();
            while (await repositoryRows.ReadAsync()) repositoryNames.Add(repositoryRows.GetString(0));
            Assert.Contains("AspNetUsers", repositoryNames);
            Assert.Contains("AspNetRoles", repositoryNames);
            Assert.Contains("ModuleDatabaseBindings", repositoryNames);
            foreach (var entity in module.EntityTypes) Assert.DoesNotContain(entity.Name, repositoryNames);
            await VerifyLifecyclePersistenceAsync(editor.Object, source, database, otherDatabase, repository, resolver);
            await VerifyHeatMapPersistenceAsync(editor.Object, source, database, otherDatabase, repository, resolver);
        }
        finally
        {
            source.Closeconnection();
            if (source.Dataconnection is RDBDataConnection connection) connection.DbConn?.Dispose();
            otherSource.Closeconnection();
            if (otherSource.Dataconnection is RDBDataConnection otherConnection) otherConnection.DbConn?.Dispose();
        }
    }

    private async Task VerifyHeatMapPersistenceAsync(IDMEEditor editor, SQLServerDataSource source, string database,
        string otherDatabase, SqlServerRepositoryDbContext repository, ModuleConnectionResolver resolver)
    {
        var common = Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>();
        var defaults = Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>();
        var metadata = Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>();
        var module = new Beep.OilandGas.HeatMap.Modules.HeatMapModule();
        var setup = new PPDM39SetupService(editor, NullLogger<PPDM39SetupService>.Instance, common, defaults, metadata,
            new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), moduleSetupOrchestrator: new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance),
            migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync);
        var controller = new ModuleRepositoryController(repository, editor, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter());
        var binding = Assert.IsType<ModuleDatabaseBinding>(Assert.IsType<OkObjectResult>(
            await controller.Bind(module.ModuleId, new(database, null), default)).Value);
        var plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp);
        Assert.True(plan.Success, plan.Message);
        Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ApprovedBy = "test-admin" })).Success);
        var migrated = await setup.ExecuteSchemaMigrationPlanAsync(new()
        {
            PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash
        });
        Assert.True(migrated.Success, migrated.Message);
        ModuleSchemaVerification.Verify(editor, source, module.EntityTypes);
        var service = new Beep.OilandGas.HeatMap.Services.HeatMapService(editor, common, defaults, metadata,
            () => resolver.ResolveAsync(module.ModuleId));
        var liveSchema = source.GetEntityStructure(new EntityStructure
        {
            EntityName = "HEAT_MAP_CONFIGURATION", DatasourceEntityName = "HEAT_MAP_CONFIGURATION"
        }, true);
        Assert.Equal("datetime2", liveSchema.Fields.Single(field => field.FieldName == "ROW_CREATED_DATE").ColumnTypeName,
            ignoreCase: true);
        var before = DateTime.UtcNow.AddSeconds(-1);
        var id = await service.SaveHeatMapConfigurationAsync(new() { ConfigurationName = "LocalDB heat map" }, "test-admin");
        Assert.False(string.IsNullOrWhiteSpace(id));
        var loaded = await service.GetHeatMapConfigurationAsync(id);
        Assert.NotNull(loaded);
        Assert.Equal(id, loaded.ConfigurationId);
        Assert.Equal("LocalDB heat map", loaded.ConfigurationName);
        Assert.InRange(loaded.CreatedDate, before, DateTime.UtcNow.AddSeconds(1));
        var reloaded = await service.GetHeatMapConfigurationAsync(id);
        Assert.Equal(loaded.CreatedDate, reloaded!.CreatedDate);

        using var independent = new SqlConnection($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true");
        await independent.OpenAsync();
        using var row = independent.CreateCommand();
        row.CommandText = "SELECT CONFIGURATION_NAME, ROW_CREATED_DATE FROM HEAT_MAP_CONFIGURATION WHERE HEAT_MAP_ID = @id";
        row.Parameters.AddWithValue("@id", id);
        using var reader = await row.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(loaded.ConfigurationName, reader.GetString(0));
        Assert.Equal(loaded.CreatedDate, reader.GetDateTime(1));
        Assert.False(await reader.ReadAsync());

        using var other = new SqlConnection($"Server=(localdb)\\MSSQLLocalDB;Database={otherDatabase};Integrated Security=true;TrustServerCertificate=true");
        await other.OpenAsync();
        using var table = other.CreateCommand();
        table.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = 'HEAT_MAP_CONFIGURATION'";
        Assert.Equal(0, Convert.ToInt32(await table.ExecuteScalarAsync()));
    }

    private async Task VerifyLifecyclePersistenceAsync(IDMEEditor editor, SQLServerDataSource source, string database,
        string otherDatabase, SqlServerRepositoryDbContext repository, ModuleConnectionResolver resolver)
    {
        var common = Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>();
        var defaults = Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>();
        var metadata = Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>();
        var module = new Beep.OilandGas.LifeCycle.Modules.LifeCycleModule(new Beep.OilandGas.PPDM39.Core.Interfaces.ModuleSetupContext
        {
            Editor = editor, CommonColumnHandler = common, Defaults = defaults, Metadata = metadata,
            ConnectionName = database, Logger = NullLogger.Instance
        });
        var setup = new PPDM39SetupService(editor, NullLogger<PPDM39SetupService>.Instance, common, defaults, metadata,
            new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), moduleSetupOrchestrator: new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance),
            migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync);
        var controller = new ModuleRepositoryController(repository, editor, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter());
        var binding = Assert.IsType<ModuleDatabaseBinding>(Assert.IsType<OkObjectResult>(
            await controller.Bind(module.ModuleId, new(database, null), default)).Value);
        var plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp);
        Assert.True(plan.Success, plan.Message);
        Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ApprovedBy = "test-admin" })).Success);
        var migrated = await setup.ExecuteSchemaMigrationPlanAsync(new()
        {
            PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash
        });
        output.WriteLine($"Lifecycle migration: {migrated.Success}: {migrated.Message}");
        Assert.True(migrated.Success, migrated.Message);
        ModuleSchemaVerification.Verify(editor, source, module.EntityTypes);
        foreach (var row in new[]
        {
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "well-process", FIELD_ID = "field", ENTITY_ID = "well", ENTITY_TYPE = "WELL" },
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "facility-process", FIELD_ID = "field", ENTITY_ID = "facility", ENTITY_TYPE = "FACILITY" },
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "foreign-process", FIELD_ID = "foreign", ENTITY_ID = "well", ENTITY_TYPE = "WELL" }
        })
        {
            var inserted = source.InsertEntity("PROCESS_INSTANCE", row);
            Assert.True(inserted.Flag == Errors.Ok, $"Workflow insert failed: {inserted.Message} {inserted.Ex}");
        }
        var process = new Beep.OilandGas.LifeCycle.Services.Processes.BoundProcessService(
            () => resolver.ResolveAsync("LIFECYCLE"), target => new Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService(
                editor, common, defaults, metadata, target));
        var rows = await process.GetProcessInstancesForFieldAsync("field");
        Assert.Equal(new[] { "facility", "well" }, rows.Select(x => x.EntityId).OrderBy(x => x));
        using var independent = new SqlConnection($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true");
        independent.Open();
        using var count = independent.CreateCommand();
        count.CommandText = "SELECT COUNT(*) FROM PROCESS_INSTANCE";
        Assert.Equal(3, Convert.ToInt32(count.ExecuteScalar()));
        var definition = new Beep.OilandGas.Models.Processes.ProcessDefinition
        {
            ProcessId = "localdb-workflow", ProcessName = "LocalDB workflow", ProcessType = "WELL_LIFECYCLE", IsActive = true,
            Steps = new()
            {
                new() { StepId = "review", StepName = "Review", SequenceNumber = 1, RequiredRoles = new() { "Reviewer" } },
                new() { StepId = "approve", StepName = "Approve", SequenceNumber = 2, RequiredRoles = new() { "Approver" }, RequiresApproval = true }
            }
        };
        await process.CreateProcessDefinitionAsync(definition, "test-admin");
        var started = await process.StartProcessAsync(definition.ProcessId, "well-start", "WELL", "field", "test-admin");
        var reloaded = await process.GetProcessInstanceAsync(started.InstanceId);
        Assert.NotNull(reloaded);
        Assert.Equal("field", reloaded.FieldId);
        Assert.Equal("test-admin", reloaded.StartedBy);
        Assert.Equal("review", reloaded.CurrentStepId);
        Assert.Equal(new[] { "Reviewer", "Approver" }, reloaded.StepInstances.Select(x => x.RequiredRole));
        Assert.Equal(new[] { Beep.OilandGas.Models.Processes.StepStatus.PENDING, Beep.OilandGas.Models.Processes.StepStatus.BLOCKED },
            reloaded.StepInstances.Select(x => x.Status));
        Assert.Contains(reloaded.History, x => x.Action == "PROCESS_STARTED" && x.PerformedBy == "test-admin");
        using var workflowRows = independent.CreateCommand();
        workflowRows.Parameters.AddWithValue("@instance", started.InstanceId);
        workflowRows.CommandText = "SELECT COUNT(*) FROM PROCESS_INSTANCE WHERE PROCESS_INSTANCE_ID = @instance";
        Assert.Equal(1, Convert.ToInt32(workflowRows.ExecuteScalar()));
        workflowRows.CommandText = "SELECT COUNT(*) FROM PROCESS_STEP_INSTANCE WHERE PROCESS_INSTANCE_ID = @instance";
        Assert.Equal(2, Convert.ToInt32(workflowRows.ExecuteScalar()));
        workflowRows.CommandText = "SELECT COUNT(*) FROM PROCESS_HISTORY WHERE PROCESS_INSTANCE_ID = @instance";
        Assert.Equal(1, Convert.ToInt32(workflowRows.ExecuteScalar()));
        using var other = new SqlConnection($"Server=(localdb)\\MSSQLLocalDB;Database={otherDatabase};Integrated Security=true;TrustServerCertificate=true");
        other.Open();
        using var isolated = other.CreateCommand();
        isolated.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = 'PROCESS_INSTANCE'";
        Assert.Equal(0, Convert.ToInt32(isolated.ExecuteScalar()));
    }

    private static async Task<SchemaMigrationPlanResult> PlanOverHttpAsync(ModuleRepositoryController controller,
        string moduleId, string stamp)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(controller);
        builder.Services.AddAuthorization(RepositoryAuthorization.Configure);
        builder.Services.AddControllers().AddApplicationPart(typeof(ModuleRepositoryController).Assembly)
            .ConfigureApplicationPartManager(parts => parts.FeatureProviders.Add(new OnlyModuleController()))
            .AddControllersAsServices();
        await using var app = builder.Build();
        // Authentication is tested separately; this isolated host exercises MVC binding and JSON formatting.
        app.Use(async (context, next) =>
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                // An active OilGas administrator as the API's own resolution leaves one: party_id and the active-account
                // marker issued here (RepositoryRolesClaimsTransformation), the repository's role.
                new Claim(Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaimsTransformation<string>.ClaimType, "test-admin"),
                new Claim(RepositoryRolesClaimsTransformation.ActiveAccount, "true"), new Claim(ClaimTypes.Role, "Administrator")
            }, "test"));
            await next();
        });
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using var response = await client.PostAsJsonAsync($"/api/setup/modules/{Uri.EscapeDataString(moduleId)}/plan",
                new ModulePlanRequest(EnvironmentTier: "Development", ConcurrencyStamp: stamp));
            var body = await response.Content.ReadAsStringAsync();
            Assert.True(response.IsSuccessStatusCode, $"HTTP plan failed ({response.StatusCode}): {body}");
            var plan = await response.Content.ReadFromJsonAsync<SchemaMigrationPlanResult>();
            Assert.NotNull(plan);
            Assert.False(string.IsNullOrWhiteSpace(plan.PlanHash));
            Assert.False(string.IsNullOrWhiteSpace(plan.ManifestHash));
            Assert.NotEmpty(plan.DryRunOperations);
            return plan;
        }
        finally { await app.StopAsync(); }
    }

    private sealed class OnlyModuleController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var controller in feature.Controllers.Where(x => x.AsType() != typeof(ModuleRepositoryController)).ToArray())
                feature.Controllers.Remove(controller);
        }
    }

    [LocalDbDriverFact]
    public void ShippedSqlServerDriverOpensLocalDbAndReadsItsTarget()
    {
        const string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=master;Integrated Security=true;TrustServerCertificate=true";
        var properties = new ConnectionProperties
        {
            ConnectionName = "localdb-driver-probe", ConnectionString = connectionString,
            DatabaseType = DataSourceType.SqlServer, Category = DatasourceCategory.RDBMS,
            Host = "(localdb)\\MSSQLLocalDB", Database = "master", IntegratedSecurity = true
        };
        var driver = ConnectionHelper.CreateSqlServerConfig();
        driver.ConnectionString = connectionString;
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns([properties]);
        config.SetupGet(x => x.DataDriversClasses).Returns([driver]);
        var loader = new Mock<IAssemblyHandler>();
        loader.Setup(x => x.GetInstance(It.IsAny<string>())).Returns(() => new SqlConnection());
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.SetupGet(x => x.assemblyHandler).Returns(loader.Object);
        editor.SetupGet(x => x.ErrorObject).Returns(new ErrorsInfo());
        var source = new SQLServerDataSource(properties.ConnectionName, Mock.Of<IDMLogger>(),
            editor.Object, DataSourceType.SqlServer, new ErrorsInfo());
        output.WriteLine($"SQL Server driver: {source.GetType().Assembly.FullName}");
        source.Dataconnection.ConnectionProp = properties;
        source.Dataconnection.DataSourceDriver = driver;
        try
        {
            MigrationConnectionTarget.Validate(editor.Object, source, properties.ConnectionName);
            Assert.Equal(ConnectionState.Open, source.Openconnection());
            var connection = Assert.IsType<RDBDataConnection>(source.Dataconnection);
            using var command = connection.DbConn.CreateCommand();
            command.CommandText = "SELECT DB_NAME()";
            Assert.Equal("master", command.ExecuteScalar());
            Assert.Equal(Errors.Ok, source.ErrorObject.Flag);
        }
        finally
        {
            source.Closeconnection();
            if (source.Dataconnection is RDBDataConnection connection) connection.DbConn?.Dispose();
        }
    }
}

public sealed class LocalDbDriverFactAttribute : FactAttribute
{
    public LocalDbDriverFactAttribute()
    {
        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("OILGAS_TEST_LOCALDB_DRIVER") != "1")
            Skip = "Set OILGAS_TEST_LOCALDB_DRIVER=1 on Windows to probe the installed SQL Server BeepDM driver against LocalDB.";
    }
}

public sealed class LocalDbModuleFactAttribute : FactAttribute
{
    public LocalDbModuleFactAttribute()
    {
        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("OILGAS_TEST_LOCALDB_MODULE") != "1")
            Skip = "Set OILGAS_TEST_LOCALDB_MODULE=1 on Windows to create and retain an isolated module test database.";
    }
}
