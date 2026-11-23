using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
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
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Data.SqlClient;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using JsonLoader = TheTechIdea.Beep.JsonLoaderService.JsonLoader;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.ApiService.Tests;

public class LocalDbDriverTests(ITestOutputHelper output)
{
    private static RepositoryTenantContext TenantScope()
    {
        var context = new RepositoryTenantContext();
        context.SelectTenant("A");
        return context;
    }

    private static void SetCaller(ControllerBase controller) => controller.ControllerContext = new()
    {
        HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "test-admin")], "test"))
        }
    };

    [LocalDbModuleFact]
    public async Task BeepMigrationAndWritesStayOnTheSelectedNamedDatabase()
    {
        var database = $"BeepOilGas_Module_{Guid.NewGuid():N}";
        var otherDatabase = $"BeepOilGas_Module_{Guid.NewGuid():N}";
        output.WriteLine($"Retained module test database: {database}");
        output.WriteLine($"Retained second module test database: {otherDatabase}");
        var receipts = Path.Combine(Path.GetTempPath(), "OilGasModuleMigration", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(receipts);
        foreach (var target in new[] { database, otherDatabase })
        {
            var provider = new SQLServerDatabaseTargetMigrationProvider(SqlClientFactory.Instance,
                $"Server=(localdb)\\MSSQLLocalDB;Database={target};Integrated Security=true;TrustServerCertificate=true",
                "OilGasModuleIntegration");
            var migration = TheTechIdea.Beep.Editor.Migration.MigrationManager.BindDatabaseTarget(provider,
                Path.Combine(receipts, target + ".json"));
            await migration.EnsureCreatedAsync();
            Assert.True(migration.Exists);
        }
        output.WriteLine($"Retained database ownership receipts: {receipts}");
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
        using var config = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(),
            receipts, "beep-migration", BeepConfigType.DataConnector);
        // Put the unselected target first to catch accidental first-connection fallback.
        config.DataConnections = [otherProperties, properties];
        config.DataDriversClasses = [driver];
        var loader = new Mock<IAssemblyHandler>();
        loader.Setup(x => x.GetInstance(It.IsAny<string>())).Returns(() => new SqlConnection());
        var editor = new Mock<TheTechIdea.Beep.DMEEditor>(Mock.Of<IDMLogger>(),
            new TheTechIdea.Beep.Utils.Util(Mock.Of<IDMLogger>(), new ErrorsInfo(), config),
            new ErrorsInfo(), config, loader.Object) { CallBase = true };
        using var editorOwner = editor.Object;
        var source = new SQLServerDataSource(database, Mock.Of<IDMLogger>(), editor.Object, DataSourceType.SqlServer, new ErrorsInfo());
        config.DataTypesMap = TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSqlServerDataTypesMapping();
        editor.Setup(x => x.GetDataSourceClass(database)).Returns(new AssemblyClassDefinition { className = nameof(SQLServerDataSource) });
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
            repository.Set<AppTenant>().Add(new() { TenantId = "A", Name = "Test tenant", IsActive = true });
            await repository.SaveChangesAsync();
            var tenant = TenantScope();
            var resolver = new ModuleConnectionResolver(repository, editor.Object, tenant);
            var failures = new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter();
            var setup = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
                Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),
                Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),
                Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(),
                failures, moduleSetupOrchestrator: new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance),
                migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync,
                migrationTargetIdentity: connection => ModuleDatabaseTarget.Key(editor.Object, connection));
            var controller = new ModuleRepositoryController(repository, editor.Object, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), new ModuleBindingService(repository, editor.Object), tenant);
            SetCaller(controller);
            var bound = Assert.IsType<OkObjectResult>(await controller.Bind(module.ModuleId, new(database, null), default));
            var binding = Assert.IsType<ModuleDatabaseBinding>(bound.Value);
            Assert.Equal(database, await resolver.ResolveAsync(module.ModuleId));
            SchemaMigrationPlanResult plan;
            try { plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp); }
            finally
            {
                foreach (var failure in failures.Reports)
                    output.WriteLine($"Reported during {failure.Operation}: {failure.Exception}");
            }
            output.WriteLine($"Plan result: {plan.Success}: {plan.Message}; {string.Join("; ", plan.DryRunDiagnostics ?? [])}");
            Assert.True(plan.Success, plan.Message);
            var execution = new SchemaMigrationExecuteRequest
            {
                PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash
            };
            Assert.False((await setup.ExecuteSchemaMigrationPlanAsync(execution)).Success);
            Assert.Empty(source.GetEntitesList());
            Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash, ApprovedBy = "test-admin" })).Success);
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
            Assert.Empty(otherSource.GetEntitesList());
            var tables = source.GetEntitesList();
            Assert.Equal(module.EntityTypes.Select(t => t.Name).OrderBy(x => x), tables.OrderBy(x => x));

            var composition = new Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION
            {
                OIL_COMPOSITION_ID = Guid.NewGuid().ToString(),
                COMPOSITION_NAME = "LocalDB persistence verification"
            };
            using var originalWork = new UnitofWork<Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION>(
                editor.Object, database, nameof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION));
            var inserted = await originalWork.InsertAsync(composition);
            Assert.True(inserted.Flag == Errors.Ok, $"Insert failed: {inserted.Message} {inserted.Ex}");
            var persisted = Assert.Single(await originalWork.Get());
            Assert.Equal(composition.OIL_COMPOSITION_ID, persisted.OIL_COMPOSITION_ID);
            Assert.Equal(composition.COMPOSITION_NAME, persisted.COMPOSITION_NAME);

            var rebound = Assert.IsType<OkObjectResult>(await controller.Bind(module.ModuleId,
                new(otherDatabase, binding.ConcurrencyStamp), default));
            var otherBinding = Assert.IsType<ModuleDatabaseBinding>(rebound.Value);
            Assert.Equal(otherDatabase, await resolver.ResolveAsync(module.ModuleId));
            var stale = await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(
                () => setup.ExecuteSchemaMigrationPlanAsync(execution));
            Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Conflict, stale.Kind);
            Assert.Empty(otherSource.GetEntitesList());
            var otherPlanned = Assert.IsType<OkObjectResult>(await controller.Plan(module.ModuleId,
                new(EnvironmentTier: "Development", ConcurrencyStamp: otherBinding.ConcurrencyStamp), default));
            var otherPlan = Assert.IsType<SchemaMigrationPlanResult>(otherPlanned.Value);
            Assert.True(otherPlan.Success, otherPlan.Message);
            Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = otherPlan.PlanId, ExpectedPlanHash = otherPlan.PlanHash, ExpectedManifestHash = otherPlan.ManifestHash, ApprovedBy = "test-admin" })).Success);
            var otherResult = await setup.ExecuteSchemaMigrationPlanAsync(new()
            {
                PlanId = otherPlan.PlanId, ExpectedPlanHash = otherPlan.PlanHash, ExpectedManifestHash = otherPlan.ManifestHash
            });
            Assert.True(otherResult.Success, otherResult.Message);
            ModuleSchemaVerification.Verify(editor.Object, otherSource, module.EntityTypes);
            Assert.Equal(module.EntityTypes.Select(t => t.Name).OrderBy(x => x), otherSource.GetEntitesList().OrderBy(x => x));
            var otherComposition = new Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION
            {
                OIL_COMPOSITION_ID = Guid.NewGuid().ToString(),
                COMPOSITION_NAME = "Second named database"
            };
            using var otherWork = new UnitofWork<Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION>(
                editor.Object, otherDatabase, nameof(Beep.OilandGas.Models.Data.Common.OIL_COMPOSITION));
            var otherInsert = await otherWork.InsertAsync(otherComposition);
            Assert.True(otherInsert.Flag == Errors.Ok, otherInsert.Message);

            var otherRow = Assert.Single(await otherWork.Get());
            Assert.Equal(otherComposition.OIL_COMPOSITION_ID, otherRow.OIL_COMPOSITION_ID);
            Assert.Equal(otherComposition.COMPOSITION_NAME, otherRow.COMPOSITION_NAME);
            var originalRow = Assert.Single(await originalWork.Get());
            Assert.Equal(composition.OIL_COMPOSITION_ID, originalRow.OIL_COMPOSITION_ID);
            Assert.Equal(composition.COMPOSITION_NAME, originalRow.COMPOSITION_NAME);

            using var reopenedRepository = new SqlServerRepositoryDbContext(
                new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(repositoryConnection).Options);
            var persistedBinding = await reopenedRepository.ModuleDatabases.SingleAsync();
            Assert.Equal(module.ModuleId, persistedBinding.ModuleId);
            Assert.Equal(otherDatabase, persistedBinding.ConnectionName);
            Assert.Empty(await reopenedRepository.Users.ToListAsync());
            await reopenedRepository.Database.OpenConnectionAsync();
            var repositoryTables = await reopenedRepository.Database.GetDbConnection().GetSchemaAsync("Tables");
            var repositoryNames = repositoryTables.Rows.Cast<DataRow>().Select(row => (string)row["TABLE_NAME"]).ToList();
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
            migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync,
            migrationTargetIdentity: connection => ModuleDatabaseTarget.Key(editor, connection));
        var controller = new ModuleRepositoryController(repository, editor, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), new ModuleBindingService(repository, editor), TenantScope());
        SetCaller(controller);
        var binding = Assert.IsType<ModuleDatabaseBinding>(Assert.IsType<OkObjectResult>(
            await controller.Bind(module.ModuleId, new(database, null), default)).Value);
        var plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp);
        Assert.True(plan.Success, plan.Message);
        Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash, ApprovedBy = "test-admin" })).Success);
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

        using var independent = new UnitofWork<Beep.OilandGas.Models.Data.HeatMap.HEAT_MAP_CONFIGURATION>(
            editor, database, "HEAT_MAP_CONFIGURATION");
        var persisted = Assert.Single(await independent.Get([new() { FieldName = "HEAT_MAP_ID", Operator = "=", FilterValue = id }]));
        Assert.Equal(loaded.ConfigurationName, persisted.CONFIGURATION_NAME);
        Assert.Equal(loaded.CreatedDate, persisted.ROW_CREATED_DATE);
        Assert.DoesNotContain("HEAT_MAP_CONFIGURATION", editor.GetDataSource(otherDatabase).GetEntitesList());
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
            migrationBindingFingerprint: resolver.GetMigrationBindingFingerprintAsync,
            migrationTargetIdentity: connection => ModuleDatabaseTarget.Key(editor, connection));
        var controller = new ModuleRepositoryController(repository, editor, [module], setup, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter(), new ModuleBindingService(repository, editor), TenantScope());
        SetCaller(controller);
        var binding = Assert.IsType<ModuleDatabaseBinding>(Assert.IsType<OkObjectResult>(
            await controller.Bind(module.ModuleId, new(database, null), default)).Value);
        var plan = await PlanOverHttpAsync(controller, module.ModuleId, binding.ConcurrencyStamp);
        Assert.True(plan.Success, plan.Message);
        Assert.True((await setup.ApproveSchemaMigrationPlanAsync(new() { PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash, ApprovedBy = "test-admin" })).Success);
        var migrated = await setup.ExecuteSchemaMigrationPlanAsync(new()
        {
            PlanId = plan.PlanId, ExpectedPlanHash = plan.PlanHash, ExpectedManifestHash = plan.ManifestHash
        });
        output.WriteLine($"Lifecycle migration: {migrated.Success}: {migrated.Message}");
        Assert.True(migrated.Success, migrated.Message);
        ModuleSchemaVerification.Verify(editor, source, module.EntityTypes);
        var seeded = Assert.IsType<ModuleSeedSummary>(Assert.IsType<OkObjectResult>(
            await controller.Seed(module.ModuleId, new(binding.ConcurrencyStamp), default)).Value);
        Assert.True(seeded.Success, string.Join("; ", seeded.Errors));
        Assert.True(seeded.RecordsInserted > 0);
        var repeatedSeed = Assert.IsType<ModuleSeedSummary>(Assert.IsType<OkObjectResult>(
            await controller.Seed(module.ModuleId, new(binding.ConcurrencyStamp), default)).Value);
        Assert.True(repeatedSeed.Success, string.Join("; ", repeatedSeed.Errors));
        Assert.Equal(0, repeatedSeed.RecordsInserted);
        Assert.DoesNotContain("R_LIFECYCLE_STATE_REFERENCE", editor.GetDataSource(otherDatabase).GetEntitesList());
        using var instanceWork = new UnitofWork<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE>(editor, database, "PROCESS_INSTANCE");
        foreach (var row in new[]
        {
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "well-process", FIELD_ID = "field", ENTITY_ID = "well", ENTITY_TYPE = "WELL" },
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "facility-process", FIELD_ID = "field", ENTITY_ID = "facility", ENTITY_TYPE = "FACILITY" },
            new Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "foreign-process", FIELD_ID = "foreign", ENTITY_ID = "well", ENTITY_TYPE = "WELL" }
        })
        {
            var inserted = await instanceWork.InsertAsync(row);
            Assert.True(inserted.Flag == Errors.Ok, $"Workflow insert failed: {inserted.Message} {inserted.Ex}");
        }
        var process = new Beep.OilandGas.LifeCycle.Services.Processes.BoundProcessService(
            () => resolver.ResolveAsync("LIFECYCLE"), target => new Beep.OilandGas.LifeCycle.Services.Processes.PPDMProcessService(
                editor, common, defaults, metadata, target));
        var rows = await process.GetProcessInstancesForFieldAsync("field");
        Assert.Equal(new[] { "facility", "well" }, rows.Select(x => x.EntityId).OrderBy(x => x));
        Assert.Equal(3, (await instanceWork.Get()).Count);
        var definition = new Beep.OilandGas.Models.Processes.ProcessDefinition
        {
            ProcessId = "localdb-workflow", ProcessName = "LocalDB workflow", ProcessType = "WELL_LIFECYCLE", EntityType = "WELL", IsActive = true,
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
        var filter = new List<TheTechIdea.Beep.Report.AppFilter>
            { new() { FieldName = "PROCESS_INSTANCE_ID", Operator = "=", FilterValue = started.InstanceId } };
        Assert.Single(await instanceWork.Get(filter));
        using var stepWork = new UnitofWork<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>(editor, database, "PROCESS_STEP_INSTANCE");
        using var historyWork = new UnitofWork<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_HISTORY>(editor, database, "PROCESS_HISTORY");
        Assert.Equal(2, (await stepWork.Get(filter)).Count);
        Assert.Single(await historyWork.Get(filter));
        Assert.DoesNotContain("PROCESS_INSTANCE", editor.GetDataSource(otherDatabase).GetEntitesList());
    }

    private static async Task<SchemaMigrationPlanResult> PlanOverHttpAsync(ModuleRepositoryController controller,
        string moduleId, string stamp)
    {
        var originalContext = controller.ControllerContext;
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
        finally
        {
            await app.StopAsync();
            controller.ControllerContext = originalContext;
        }
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
            Assert.Equal("master", connection.DbConn.Database);
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
