#if WORKFLOW_INBOX_FOCUSED
using Moq;
using System.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.Migration;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
#if BEEP_SOURCE_TEST
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.Container;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using ConnectionConfig = Beep.OilandGas.Models.Data.DataManagement.ConnectionConfig;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.Modules;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using TheTechIdeaWeb.Diagnostics;
#endif

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupDriverBeepLoaderTests
{
#if BEEP_SOURCE_TEST
    [Fact]
    public async Task IdentityTargetSavedByAnotherProcessIsReloadedAndCannotBeRetargeted()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-identity-recovery-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "identity-repository-target.json");
            IConfigurationRoot Settings() => new ConfigurationBuilder().AddJsonFile(path, optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            var first = Settings(); var stale = Settings();
            var connection = new ConnectionProperties { ConnectionName = "identity", DatabaseType = DataSourceType.SqlServer,
                ConnectionString = "Server=db.example;Database=identity;Integrated Security=true" };
            var other = new ConnectionProperties { ConnectionName = "other", DatabaseType = DataSourceType.SqlServer,
                ConnectionString = "Server=db.example;Database=other;Integrated Security=true" };
            var config = new Mock<IConfigEditor>(); config.SetupGet(x => x.DataConnections).Returns([connection, other]);
            var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
            await new SetupRepositoryConnectionSelectionService(first, editor.Object).SelectAsync("identity", () => Task.CompletedTask, default);
            Assert.Null(stale["Repository:BeepConnectionName"]);
            using var services = new ServiceCollection().AddSingleton(editor.Object).BuildServiceProvider();
            Assert.Equal(connection.ConnectionString, new BeepRepositoryConnectionSource(stale, services).Resolve()!.ConnectionString);
            Assert.Equal("identity", stale["Repository:BeepConnectionName"]);
            Assert.Equal("identity", (await new SetupRepositoryConnectionSelectionService(stale, editor.Object)
                .SelectAsync("identity", () => Task.CompletedTask, default)).ConnectionName);
            await Assert.ThrowsAsync<InvalidOperationException>(() => new SetupRepositoryConnectionSelectionService(stale, editor.Object)
                .SelectAsync("other", () => Task.CompletedTask, default));
            connection.ConnectionString = "Server=db.example;Database=changed;Integrated Security=true";
            Assert.Null(new BeepRepositoryConnectionSource(stale, services).Resolve());
        }
        finally { Directory.Delete(directory, true); }
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Repository\":{\"BeepConnectionName\":\"identity\"}}")]
    [InlineData("{ invalid")]
    public void IncompleteOrInvalidIdentityTargetRemainsUnavailable(string contents)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-identity-invalid-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var path = Path.Combine(directory, "identity-repository-target.json");
            var settings = new ConfigurationBuilder().AddJsonFile(path, optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            File.WriteAllText(path, contents);
            using var services = new ServiceCollection().AddSingleton(Mock.Of<IDMEEditor>()).BuildServiceProvider();
            Assert.Null(new BeepRepositoryConnectionSource(settings, services).Resolve());
            Assert.Equal(contents, File.ReadAllText(path));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void SystemRepositoryCatalogListsOnlyUniqueConcreteSupportedDatabaseTargets()
    {
        var config = new Mock<IConfigEditor>();
        var root = Path.Combine(Path.GetTempPath(), "oilgas-system-catalog-" + Guid.NewGuid().ToString("N"));
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties>
        {
            new() { ConnectionName = "sqlite", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + Path.Combine(root, "setup.db") },
            new() { ConnectionName = "sqlserver", DatabaseType = DataSourceType.SqlServer, ConnectionString = "Server=db.example;Database=setup" },
            new() { ConnectionName = "unsupported", DatabaseType = DataSourceType.MongoDB },
            new() { ConnectionName = "incomplete", DatabaseType = DataSourceType.SqlServer, ConnectionString = "Server=db.example" },
            new() { ConnectionName = "duplicate", DatabaseType = DataSourceType.SqlServer, ConnectionString = "Server=db.example;Database=one" },
            new() { ConnectionName = "DUPLICATE", DatabaseType = DataSourceType.SqlServer, ConnectionString = "Server=db.example;Database=two" }
        });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new SystemSetupConnectionSelectionService(new ConfigurationBuilder().Build(), editor.Object);
        Assert.Equal(new[] { "sqlite", "sqlserver" }, service.Connections());
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void IdentityProbeCopiesConnectionIdentityAndSecurityWithoutSharingMutableSettings()
    {
        var original = new ConnectionProperties { ConnectionName = "identity", GuidID = "guid", DatabaseType = DataSourceType.SqlServer,
            Category = DatasourceCategory.RDBMS, IsDatabase = true, DriverName = "driver", DriverVersion = "2",
            Host = "db.example", Port = 1433, Database = "identity", SchemaName = "schema", ConnectionString = "Server=db.example;Database=identity",
            UserID = "user", Password = "secret", IntegratedSecurity = true, TrustedConnection = true,
            UseWindowsAuthentication = true, ReadOnly = true, UseSSL = true, RequireSSL = true,
            SSLMode = "verify", EncryptConnection = true, TrustServerCertificate = true,
            BypassServerCertificateValidation = true, ParameterList = new() { ["option"] = "value" } };
        var copy = SetupRepositoryConnectionSelectionService.CopyForProbe(original);
        Assert.NotSame(original, copy); Assert.NotSame(original.ParameterList, copy.ParameterList);
        Assert.Equal(MigrationConnectionTarget.Fingerprint(original), MigrationConnectionTarget.Fingerprint(copy));
        copy.ConnectionName = "probe"; copy.ParameterList["option"] = "changed";
        Assert.Equal("identity", original.ConnectionName); Assert.Equal("value", original.ParameterList["option"]);
    }

    [Fact]
    public void FreshBeepRuntimeExposesConfiguredDatabaseDriversWithoutManualRegistration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-fresh-driver-catalog-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
                options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true; });
            using var root = services.BuildServiceProvider();
            var editor = root.GetRequiredService<IDMEEditor>();
            var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!).Build();
            var catalog = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance).Catalog();
            foreach (var handler in new[] { "SQLServerDataSource", "PostgreDataSource", "OracleDataSource", "MySQLDataSource", "SQLiteDataSource" })
            {
                var registration = Assert.Single(editor.ConfigEditor.DataDriversClasses, item => string.Equals(item.classHandler, handler, StringComparison.OrdinalIgnoreCase));
                var driver = Assert.Single(catalog, item => item.Id == registration.GuidID);
                Assert.True(driver.CanInstall || driver.Loaded, $"{handler} must be installable or already loaded.");
            }
        }
        finally
        {
            try { Directory.Delete(directory, true); }
            catch (IOException error) { Console.WriteLine("Temporary Beep catalog test directory remains locked: " + error.Message); }
        }
    }

    public sealed class BundledSqlServerFactAttribute : FactAttribute
    {
        public BundledSqlServerFactAttribute()
        {
            if (Environment.GetEnvironmentVariable("OILGAS_TEST_BUNDLED_DRIVER") != "1")
                Skip = "Requires a host that references the bundled SQL Server connector.";
        }
    }

    [BundledSqlServerFact]
    public async Task StartupRegistersBundledSqlServerConnectorWithoutDownloading()
    {
        var driver = ConnectionHelper.CreateSqlServerConfig();
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        var log = new Mock<IDMLogger>();
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:SQLServerDataSource:Package"] = "not-a-real-package", ["SetupDrivers:Connectors:SQLServerDataSource:Version"] = "0.0.0" }).Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        await service.RestoreCachedAsync(default);
        var item = Assert.Single(service.Catalog());
        Assert.True(item.Loaded); Assert.True(item.BundledConnector);
        Assert.True(item.SqlServerOwnedTransactionContract);
        var connector = Assert.Single(classes, x => x.className == "SQLServerDataSource");
        Assert.True(typeof(IIsolationOwnedTransactionDataSource).IsAssignableFrom(connector.type));
        Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory)), Path.GetDirectoryName(connector.type.Assembly.Location));
    }

    [BundledSqlServerFact]
    public async Task ActualBeepRuntimeRegistersBundledConnectorOnStartup()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-bundled-runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var services = new ServiceCollection(); services.AddLogging();
        services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
            options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true; });
        using var root = services.BuildServiceProvider();
        var editor = root.GetRequiredService<IDMEEditor>();
        var driver = editor.ConfigEditor.DataDriversClasses.FirstOrDefault(x => x.DatasourceType == DataSourceType.SqlServer && x.classHandler == "SQLServerDataSource")
            ?? ConnectionHelper.CreateSqlServerConfig();
        if (!editor.ConfigEditor.DataDriversClasses.Contains(driver)) editor.ConfigEditor.DataDriversClasses.Add(driver);
        var service = new SetupDriverService(editor, new ConfigurationBuilder().Build(), NullLogger<SetupDriverService>.Instance);
        await service.RestoreCachedAsync(default);
        var item = Assert.Single(service.Catalog(), x => x.Id == driver.GuidID);
        Assert.True(item.Loaded); Assert.True(item.BundledConnector);
    }

    [Fact]
    public async Task StartupRegistersBundledSqliteWithoutAConnectorFeed()
    {
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        var log = new Mock<IDMLogger>();
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "not-a-real-package", ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "0.0.0" }).Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        await service.RestoreCachedAsync(default);
        var item = Assert.Single(service.Catalog());
        Assert.True(item.ConnectorLoaded); Assert.True(item.BundledConnector);
        var connector = Assert.Single(classes, x => x.className == "SQLiteDataSource");
        Assert.Equal(Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory)), Path.GetDirectoryName(connector.type.Assembly.Location));
    }

    [Fact]
    public async Task CompanyOrganizationReaderUsesActualBeepSqliteDatasource()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-org-native-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "company.db");
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        var storage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(directory, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(storage);
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var connection = new ConnectionProperties { ConnectionName = "company_core", DatabaseType = DataSourceType.SqlLite,
            ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
        try
        {
            var networkDownload = Environment.GetEnvironmentVariable("OILGAS_TEST_NETWORK_DOWNLOAD") == "1";
            var overrides = networkDownload
                ? new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = "https://api.nuget.org/v3/index.json" }
                : new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED"),
                    ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.58" };
            var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!)
                .AddInMemoryCollection(overrides).Build();
            var installer = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance);
            Assert.False(Assert.Single(installer.Catalog()).ConnectorLoaded);
            SetupDriverItem installed;
            try { installed = await installer.InstallAsync(driver.GuidID, Assert.Single(installer.Catalog(), x => x.Id == driver.GuidID), () => Task.CompletedTask, default); }
            catch (Exception error) { throw new InvalidOperationException(string.Join(" | ", messages.TakeLast(30)), error); }
            Assert.True(installed.Loaded && installed.ConnectorLoaded && installed.ProviderLoaded);
            if (networkDownload)
                Assert.True(Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("NUGET_PACKAGES")!,
                    "thetechidea.beep.sqlitedatasourcecore", installed.ConnectorVersion!)), "The configured connector must be downloaded into the isolated cache.");
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.Equal(ConnectionState.Open, source.Openconnection());
            var type = typeof(Beep.OilandGas.PPDM39.Models.BUSINESS_ASSOCIATE);
            var model = new MigrationModel { Source = "FocusedTest", SourceId = "organization-review-projection" };
            model.Entities.Add(type.FullName!, new MigrationModelEntity { ClrTypeFullName = type.FullName!, TableName = "BUSINESS_ASSOCIATE",
                Properties = new[] { "BUSINESS_ASSOCIATE_ID", "ACTIVE_IND" }.Select(name => new MigrationModelProperty
                    { PropertyName = name, ColumnName = name, FieldType = typeof(string).FullName!, IsPrimaryKey = name == "BUSINESS_ASSOCIATE_ID", IsNullable = name != "BUSINESS_ASSOCIATE_ID", MaxLength = 128 }).ToList() });
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            var plan = migration.BuildMigrationPlanForModel(model, detectRelationships: false);
            migration.ApproveMigrationPlan(plan, "local-test"); var result = migration.ExecuteMigrationPlan(plan);
            Assert.True(result.Success, result.Message + " | " + errors.Message + " | " + string.Join(" | ", messages.TakeLast(15)));
            Assert.Equal(Errors.Ok, source.InsertEntity("BUSINESS_ASSOCIATE", new Beep.OilandGas.PPDM39.Models.BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("BUSINESS_ASSOCIATE", new Beep.OilandGas.PPDM39.Models.BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "other", ACTIVE_IND = "N" }).Flag);
            var metadata = new Mock<IPPDMMetadataRepository>();
            metadata.Setup(x => x.GetTableMetadataAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = "BUSINESS_ASSOCIATE", TableName = "BUSINESS_ASSOCIATE", PrimaryKeyColumn = "BUSINESS_ASSOCIATE_ID" });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("BUSINESS_ASSOCIATE")).ReturnsAsync(["BUSINESS_ASSOCIATE_ID"]);
            var company = new CompanyScope("tenant", "company");
            var boundary = new CompanyModuleConnection(company, new AssetDatabaseScope(connection.ConnectionName, new string('A', 64)));
            var reader = new CompanyOrganizationReader(boundary, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
            Assert.Equal("Active", await reader.ReadAsync(company, () => Task.CompletedTask, default));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reader.ReadAsync(new("tenant", "other"), () => Task.CompletedTask, default));
            async Task<string> Observe(string companyId)
            {
                var scope = new CompanyScope("tenant", companyId);
                var scoped = new CompanyModuleConnection(scope, new AssetDatabaseScope(connection.ConnectionName, new string('A', 64)));
                return await new CompanyOrganizationReader(scoped, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object)
                    .ReadAsync(scope, () => Task.CompletedTask, default);
            }
            Assert.Equal("Inactive", await Observe("other"));
            Assert.Equal("NotFound", await Observe("missing"));
            var bindings = new Mock<ICompanyEnvironmentBindingReader>();
            bindings.Setup(x => x.ResolveAsync("PPDM_CORE", It.IsAny<CancellationToken>())).ReturnsAsync(new string('A', 64));
            var verified = await new CompanyOrganizationVerifier(bindings.Object, reader).ReviewAsync(company, () => Task.CompletedTask, default);
            Assert.Equal("Active", verified.Status);

            // Exercise the app-owned incident intent schema on the actual downloaded datasource.
            var intentType = typeof(HSE_INCIDENT_REPORT_INTENT);
            var intentStructure = new ClassCreator(editor).ConvertToEntityStructure(intentType);
            var intentModel = new MigrationModel { Source = "FocusedTest", SourceId = "incident-report-intent" };
            intentModel.Entities.Add(intentType.FullName!, new MigrationModelEntity { ClrTypeFullName = intentType.FullName!, TableName = intentType.Name,
                Properties = intentStructure.Fields.Select(field => new MigrationModelProperty { PropertyName = field.FieldName, ColumnName = field.FieldName,
                    FieldType = field.Fieldtype, IsPrimaryKey = field.IsKey, IsNullable = field.AllowDBNull,
                    MaxLength = Math.Max(field.Size, field.Size1) > 0 ? Math.Max(field.Size, field.Size1) : null }).ToList() });
            var intentPlan = migration.BuildMigrationPlanForModel(intentModel, detectRelationships: false);
            migration.ApproveMigrationPlan(intentPlan, "local-test");
            var intentMigration = migration.ExecuteMigrationPlan(intentPlan);
            Assert.True(intentMigration.Success, intentMigration.Message + " | " + errors.Message + " | " + string.Join(" | ", messages.TakeLast(15)));
            var context = new FieldSelectionReview(company, new AssetDatabaseScope(connection.ConnectionName, new string('A', 64)), "actor", "revision", "field");
            var original = new IncidentReportIntent(Guid.NewGuid(), context, new("KUWAIT", "SPILL", "TIER_2"), new string('A', 64),
                new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), "Site", "Description", "الكويت");
            var intentRow = HSE_INCIDENT_REPORT_INTENT.Create(original);
            new CommonColumnHandler().PrepareForInsert(intentRow, "actor");
            Assert.Equal(Errors.Ok, source.InsertEntity(intentType.Name, intentRow).Flag);
            metadata.Setup(x => x.GetTableMetadataAsync(intentType.Name)).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = intentType.FullName!, TableName = intentType.Name, PrimaryKeyColumn = "HSE_INCIDENT_REPORT_INTENT_ID" });
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(intentType.Name)).ReturnsAsync(["HSE_INCIDENT_REPORT_INTENT_ID"]);
            var intentRepository = new PPDMGenericRepository(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, intentType, connection.ConnectionName, intentType.Name);
            var saved = Assert.Single((await intentRepository.GetAsync([new AppFilter { FieldName = "HSE_INCIDENT_REPORT_INTENT_ID", Operator = "=", FilterValue = intentRow.HSE_INCIDENT_REPORT_INTENT_ID }])).Cast<HSE_INCIDENT_REPORT_INTENT>());
            Assert.Equal(intentRow.COMMAND_JSON, saved.COMMAND_JSON); Assert.Equal(intentRow.COMMAND_HASH, saved.COMMAND_HASH);
            Assert.Equal(original, saved.Restore(company, "actor"));
        }
        finally
        {
            editor.GetDataSource(connection.ConnectionName)?.Closeconnection();
            handler.GetType(driver.DbConnectionType)?.GetMethod("ClearAllPools", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            try { Directory.Delete(directory, true); }
            catch (IOException error) { Console.WriteLine("Temporary Beep organization test directory remains locked: " + error.Message); }
        }
    }

    public sealed class IsolatedDriverFeedFactAttribute : FactAttribute
    {
        public IsolatedDriverFeedFactAttribute()
        {
            if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OILGAS_TEST_ISOLATED_DRIVER_FEED")))
                Skip = "Requires Windows LocalDB and an explicitly prepared isolated Beep package feed.";
        }
    }

    [IsolatedDriverFeedFact]
    public async Task WizardInstallsPackagedSqlServerConnectorWithSerializableOwnership()
    {
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_ISOLATED_DRIVER_FEED")!;
        var version = Environment.GetEnvironmentVariable("OILGAS_TEST_PACKAGED_VERSION")
            ?? throw new InvalidOperationException("The isolated package version is required.");
        var packageCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES")!;
        var database = "BeepOilGas_PackagedDriver_" + Guid.NewGuid().ToString("N");
        var setupPath = Path.Combine(Path.GetTempPath(), "oilgas-packaged-system-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(setupPath);
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        await using var creation = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connectionString).Options);
        try
        {
            await creation.Database.EnsureCreatedAsync();
            creation.Users.Add(new OilGasUser { Id = "setup-test-user", UserName = "setup-test", Email = null });
            await creation.SaveChangesAsync();
            var driver = ConnectionHelper.CreateSqlServerConfig();
            var connection = new ConnectionProperties { ConnectionName = "packaged_driver", DatabaseType = DataSourceType.SqlServer, ConnectionString = connectionString, Database = database, GuidID = Guid.NewGuid().ToString() };
            var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
            var storage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(setupPath, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
            config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(storage);
            config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
            config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
            config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
            config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSqlServerDataTypesMapping());
            var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
            var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
            config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
                .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
            var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
            using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
            using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
            var settings = new ConfigurationBuilder().AddJsonFile(Path.Combine(setupPath, "system-setup-target.json"), optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
            { ["Beep:ConfigPath"] = setupPath,
              ["SetupDrivers:Connectors:SQLServerDataSource:Package"] = "TheTechIdea.Beep.SqlServerDataSourceCore",
              ["SetupDrivers:Connectors:SQLServerDataSource:Version"] = version,
              ["SetupDrivers:Providers:SQLServerDataSource:Package"] = "System.Data.SqlClient", ["SetupDrivers:Providers:SQLServerDataSource:Version"] = "4.9.1",
              ["SetupDrivers:PackageSources:0"] = feed, ["SetupDrivers:PackageSources:1"] = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED") }).Build();
            var service = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance);
            var result = await service.InstallAsync(driver.GuidID, Assert.Single(service.Catalog(), x => x.Id == driver.GuidID), () => Task.CompletedTask, default);
            Assert.True(result.Loaded, string.Join(" | ", messages.TakeLast(30)));
            var downloaded = Path.Combine(packageCache, "thetechidea.beep.sqlserverdatasourcecore", version.ToLowerInvariant());
            Assert.True(Directory.Exists(downloaded), "The connector must be installed into the isolated package cache.");
            var connector = Assert.Single(classes, x => x.className == "SQLServerDataSource");
            Assert.StartsWith(Path.GetFullPath(downloaded) + Path.DirectorySeparatorChar, Path.GetFullPath(connector.type.Assembly.Location), StringComparison.OrdinalIgnoreCase);
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.Equal(ConnectionState.Open, source.Openconnection());
            var projection = Assert.IsAssignableFrom<IOwnedEntityProjectionDataSource>(source);
            Assert.True(projection.SupportsOwnedEntityProjections);
            var owned = Assert.IsAssignableFrom<IIsolationOwnedTransactionDataSource>(source); var owner = new object();
            Assert.Equal(Errors.Ok, owned.BeginOwnedTransaction(owner, IsolationLevel.Serializable).Flag);
            Assert.Equal(IsolationLevel.Serializable, ((IDbTransaction)source.GetType().GetProperty("ActiveTransaction")!.GetValue(source)!).IsolationLevel);
            var empty = await projection.ReadEntityProjectionAsync("RepositoryBootstrap", [], ["Id", "AdministratorUserId"]);
            Assert.True(empty.Outcome.Flag == Errors.Ok, empty.Outcome.Message + " | " + string.Join(" | ", messages.TakeLast(20)));
            Assert.Empty(empty.Rows);
            var populated = await projection.ReadEntityProjectionAsync("AspNetUsers", [], ["Id", "UserName", "Email"]);
            Assert.True(populated.Outcome.Flag == Errors.Ok, populated.Outcome.Message + " | " + string.Join(" | ", messages.TakeLast(20)));
            var row = Assert.IsAssignableFrom<IDictionary<string, object>>(Assert.Single(populated.Rows));
            Assert.Equal("setup-test-user", row["Id"]);
            Assert.Equal("setup-test", row["UserName"]);
            Assert.True(row.ContainsKey("Email"));
            Assert.Null(row["Email"]);
            var missing = await projection.ReadEntityProjectionAsync("RepositoryBootstrap", [], ["Id", "NOT_A_COLUMN"]);
            Assert.Equal(Errors.Failed, missing.Outcome.Flag);
            Assert.Equal(Errors.Ok, owned.RollbackOwnedTransaction(owner).Flag);

            var selection = await new SystemSetupConnectionSelectionService(settings, editor)
                .SelectAsync(connection.ConnectionName, () => Task.CompletedTask, default);
            Assert.Equal(connection.ConnectionName, selection.ConnectionName);
            var target = new SystemSetupTarget(settings, editor);
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            var plan = migration.BuildMigrationPlanForTypes([typeof(SYSTEM_SETUP_RECORD)], detectRelationships: false);
            Assert.Single(plan.Operations, x => x.Kind == MigrationPlanOperationKind.CreateEntity);
            migration.ApproveMigrationPlan(plan, "setup-test");
            var applied = migration.ExecuteMigrationPlan(plan);
            Assert.True(applied.Success, applied.Message + " | " + errors.Message + " | " + string.Join(" | ", messages.TakeLast(20)));
            var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("SYSTEM_SETUP_RECORD")).ReturnsAsync(["SYSTEM_SETUP_RECORD_ID"]);
            var records = new SystemSetupRecordStore(target, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
            var draft = SystemSetupToolsDefaults.Draft();
            var saved = await records.SaveAsync(new("TOOLS", "DEFAULT", draft.ToolsVersion, draft.SettingsJson, "", Guid.NewGuid().ToString("N")), "setup-test", () => Task.CompletedTask);
            Assert.Equal(draft.SettingsJson, (await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask))!.SettingsJson);
            Assert.Equal(saved, await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask));
            source.Closeconnection();
        }
        finally { await creation.Database.EnsureDeletedAsync(); }
    }

    [IsolatedDriverFeedFact]
    public async Task FreshProcessRestoresDownloadedSqlServerConnectorFromCache()
    {
        var version = Environment.GetEnvironmentVariable("OILGAS_TEST_PACKAGED_VERSION")!;
        var cache = Environment.GetEnvironmentVariable("NUGET_PACKAGES")!;
        var driver = ConnectionHelper.CreateSqlServerConfig();
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        var log = new Mock<IDMLogger>(); var messages = new List<string>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:SQLServerDataSource:Package"] = "TheTechIdea.Beep.SqlServerDataSourceCore",
          ["SetupDrivers:Connectors:SQLServerDataSource:Version"] = version,
          ["SetupDrivers:Providers:SQLServerDataSource:Package"] = "System.Data.SqlClient", ["SetupDrivers:Providers:SQLServerDataSource:Version"] = "4.9.1" }).Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        Assert.False(Assert.Single(service.Catalog()).ConnectorLoaded);
        await service.RestoreCachedAsync(default);
        var restored = Assert.Single(service.Catalog());
        Assert.True(restored.Loaded, string.Join(" | ", messages.TakeLast(30)));
        Assert.False(restored.BundledConnector);
        var connector = Assert.Single(classes, x => x.className == "SQLServerDataSource");
        var expected = Path.Combine(cache, "thetechidea.beep.sqlserverdatasourcecore", version.ToLowerInvariant());
        Assert.StartsWith(Path.GetFullPath(expected) + Path.DirectorySeparatorChar, Path.GetFullPath(connector.type.Assembly.Location), StringComparison.OrdinalIgnoreCase);
        Assert.True(typeof(IIsolationOwnedTransactionDataSource).IsAssignableFrom(connector.type));
    }

    [WindowsSetupLocalDbFact]
    public async Task SystemTargetRejectsOutdatedSqlServerConnectorBeforeBeginningATransaction()
    {
        var database = "BeepOilGas_OldDriver_" + Guid.NewGuid().ToString("N");
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        await using var creation = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connectionString).Options);
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-old-driver-target-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await creation.Database.EnsureCreatedAsync();
            var driver = ConnectionHelper.CreateSqlServerConfig();
            var connection = new ConnectionProperties { ConnectionName = "system_old", DatabaseType = DataSourceType.SqlServer, ConnectionString = connectionString, Database = database, GuidID = Guid.NewGuid().ToString() };
            var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
            config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
            config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
            config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
            var log = new Mock<IDMLogger>(); var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
            using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
            using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
            var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            var drivers = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance);
            await drivers.RestoreCachedAsync(default);
            Assert.True(Assert.Single(drivers.Catalog()).Loaded);
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.IsNotAssignableFrom<IIsolationOwnedTransactionDataSource>(source);
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new SystemSetupConnectionSelectionService(settings, editor)
                .SelectAsync(connection.ConnectionName, () => Task.CompletedTask, default));
            Assert.Contains("owned serializable transactions", error.Message);
            Assert.Null(source.GetType().GetProperty("ActiveTransaction")!.GetValue(source));
            Assert.False(File.Exists(SystemSetupConnectionSelectionService.SettingsPath(settings)));
            source.Closeconnection();
        }
        finally { await creation.Database.EnsureDeletedAsync(); }
    }

    [WindowsSetupLocalDbFact]
    public async Task SqlServerSystemSetupMigrationStoresTheFullToolTemplate()
    {
        var database = "BeepOilGas_SystemSetup_" + Guid.NewGuid().ToString("N");
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connectionString).Options;
        await using var creation = new SqlServerRepositoryDbContext(options);
        var ownershipPath = Path.Combine(Path.GetTempPath(), "oilgas-sqlserver-migration-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ownershipPath);
        try
        {
            await creation.Database.EnsureCreatedAsync();
            var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
            var storage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(ownershipPath, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
            config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(storage);
            var driver = ConnectionHelper.CreateSqlServerConfig();
            var connection = new ConnectionProperties { ConnectionName = "system_sqlserver", DatabaseType = DataSourceType.SqlServer, ConnectionString = connectionString, Database = database, GuidID = Guid.NewGuid().ToString() };
            config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
            config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
            config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
            config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSqlServerDataTypesMapping());
            var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
            var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
            config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
                .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
            var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
            using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
            using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
            var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["SystemSetup:ConnectionName"] = connection.ConnectionName, ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor, connection) }).Build();
            var sourceAssembly = Environment.GetEnvironmentVariable("OILGAS_TEST_SQL_SOURCE")
                ?? throw new InvalidOperationException("The source-built SQL Server connector path is required.");
            handler.ScanAssembly(System.Reflection.Assembly.LoadFrom(sourceAssembly));
            await new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance).RestoreCachedAsync(default);
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.Equal(ConnectionState.Open, source.Openconnection());
            var selection = await new SystemSetupConnectionSelectionService(settings, editor).SelectAsync(connection.ConnectionName, () => Task.CompletedTask, default);
            Assert.Equal(connection.ConnectionName, selection.ConnectionName);
            var migration = new MigrationManager(editor, source) { ExecutionTargetIdentity = ModuleDatabaseTarget.Key(editor, connection) };
            var plan = migration.BuildMigrationPlanForTypes([typeof(SYSTEM_SETUP_RECORD)], detectRelationships: false);
            Assert.Single(plan.Operations, x => x.Kind == MigrationPlanOperationKind.CreateEntity);
            migration.ApproveMigrationPlan(plan, "local-test");
            var applied = migration.ExecuteMigrationPlan(plan);
            Assert.True(applied.Success, applied.Message + " | " + errors.Message + " | " + string.Join(" | ", messages.TakeLast(20)));
            var metadata = new Mock<IPPDMMetadataRepository>(); metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("SYSTEM_SETUP_RECORD")).ReturnsAsync(["SYSTEM_SETUP_RECORD_ID"]);
            var records = new SystemSetupRecordStore(new SystemSetupTarget(settings, editor), editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
            var nativeOwner = Assert.IsAssignableFrom<IIsolationOwnedTransactionDataSource>(source);
            var owner = new object();
            Assert.Equal(Errors.Ok, nativeOwner.BeginOwnedTransaction(owner, IsolationLevel.Serializable).Flag);
            Assert.True(nativeOwner.OwnsTransaction(owner));
            Assert.Equal(Errors.Failed, nativeOwner.BeginOwnedTransaction(new object(), IsolationLevel.Serializable).Flag);
            Assert.Equal(Errors.Failed, nativeOwner.BeginOwnedTransaction(owner, IsolationLevel.ReadCommitted).Flag);
            Assert.True(nativeOwner.OwnsTransaction(owner));
            Assert.Equal(Errors.Ok, nativeOwner.RollbackOwnedTransaction(owner).Flag);
            Assert.False(nativeOwner.OwnsTransaction(owner));
            var discarded = SystemSetupIdentity.Prepare(new("PPDM", "ROLLBACK", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
            var row = new SYSTEM_SETUP_RECORD { SYSTEM_SETUP_RECORD_ID = SystemSetupIdentity.ForKey(discarded.Category, discarded.Key), CATEGORY = discarded.Category,
                SETUP_KEY = discarded.Key, VERSION = discarded.Version, SETTINGS_JSON = discarded.SettingsJson, REVISION = discarded.Revision,
                PREVIOUS_REVISION = discarded.PreviousRevision, INTENT_ID = discarded.IntentId };
            await Assert.ThrowsAsync<InvalidOperationException>(() => PPDMDataSourceTransaction.ExecuteAsync(source, () =>
            {
                PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
                Assert.Equal(Errors.Ok, source.InsertEntity("SYSTEM_SETUP_RECORD", row).Flag);
                throw new InvalidOperationException("Exercise rollback after the write.");
            }));
            Assert.Null(await records.ReadAsync("PPDM", "ROLLBACK", () => Task.CompletedTask));
            var draft = SystemSetupToolsDefaults.Draft();
            var saved = await records.SaveAsync(new("TOOLS", "DEFAULT", draft.ToolsVersion, draft.SettingsJson, "", Guid.NewGuid().ToString("N")), "local-admin", () => Task.CompletedTask);
            Assert.Equal(draft.SettingsJson, (await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask))!.SettingsJson);
            Assert.Equal(saved, await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask));
            source.Closeconnection();
        }
        finally { await creation.Database.EnsureDeletedAsync(); }
    }

    [Fact]
    public void SetupDatabaseTypesComeOnlyFromTheCurrentBeepCatalog()
    {
        var drivers = new List<ConnectionDriversConfig>();
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(drivers);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        Assert.Empty(service.GetAvailableDatabaseTypes());
        drivers.Add(ConnectionHelper.CreateSQLiteConfig());
        drivers.Add(ConnectionHelper.CreateSQLiteConfig());
        drivers.Add(ConnectionHelper.CreateSqlServerConfig());
        Assert.Equal(new[] { "SqlLite", "SqlServer" }, service.GetAvailableDatabaseTypes());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SavedConnectionDoesNotReportSuccessWhenDatasourceTestFails(bool throws)
    {
        var config = new Mock<IConfigEditor>();
        config.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns(true);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        if (throws) editor.Setup(x => x.GetDataSource("setup_connection")).Throws(new InvalidOperationException("test failure"));
        else editor.Setup(x => x.GetDataSource("setup_connection")).Returns((IDataSource)null!);
        var failures = new Mock<IFailureReporter>();
        failures.Setup(x => x.ReportHandled(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FailureSeverity>())).Returns("test-reference");
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), failures.Object);
        var result = service.SaveConnection(new ConnectionConfig { ConnectionName = "setup_connection", DatabaseType = "SqlLite" }, testAfterSave: true);
        Assert.False(result.Success);
        Assert.Equal("setup_connection", result.ConnectionName);
        Assert.Contains("saved", result.Message, StringComparison.OrdinalIgnoreCase);
        config.Verify(x => x.SaveDataconnectionsValues(), Times.Once);
    }

    [Theory]
    [InlineData(ConnectionState.Open, true)]
    [InlineData(ConnectionState.Closed, false)]
    public void SavedConnectionTestRequiresAnOpenDatasource(ConnectionState state, bool expected)
    {
        var config = new Mock<IConfigEditor>();
        config.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns(true);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var datasource = new Mock<IDataSource>();
        datasource.Setup(x => x.Openconnection()).Returns(state);
        editor.Setup(x => x.GetDataSource("setup_connection")).Returns(datasource.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        var result = service.SaveConnection(new ConnectionConfig { ConnectionName = "setup_connection", DatabaseType = "SqlLite" }, testAfterSave: true);
        Assert.Equal(expected, result.Success);
        datasource.Verify(x => x.Closeconnection(), Times.Once);
    }

    [Fact]
    public void RefusedConnectionIsNotSavedOrReportedAsCreated()
    {
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        config.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns(false);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        var result = service.SaveConnection(new ConnectionConfig { ConnectionName = "setup_connection", DatabaseType = "SqlLite" }, testAfterSave: false);
        Assert.False(result.Success);
        Assert.Null(result.ConnectionName);
        config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
        editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RefusedSqliteConnectionDoesNotReportDatabaseCreation()
    {
        var path = Path.Combine(Path.GetTempPath(), "oilgas-refused-sqlite-" + Guid.NewGuid().ToString("N"));
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        config.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns(false);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        try
        {
            var result = await service.CreateSqliteAsync(new CreateSqliteRequest { ConnectionName = "setup_connection", SavePath = path });
            Assert.False(result.Success);
            config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
            editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
        }
        finally { if (Directory.Exists(path)) Directory.Delete(path); }
    }

    [Fact]
    public async Task ExistingSqliteConnectionIsNotReplaced()
    {
        var path = Path.Combine(Path.GetTempPath(), "oilgas-existing-sqlite-" + Guid.NewGuid().ToString("N"));
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        config.Setup(x => x.DataConnectionExist("setup_connection")).Returns(true);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        try
        {
            var result = await service.CreateSqliteAsync(new CreateSqliteRequest { ConnectionName = "setup_connection", SavePath = path });
            Assert.False(result.Success);
            Assert.Contains("already exists", result.Message);
            Assert.False(Directory.Exists(path));
            config.Verify(x => x.AddDataConnection(It.IsAny<ConnectionProperties>()), Times.Never);
            config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
        }
        finally { if (Directory.Exists(path)) Directory.Delete(path); }
    }

    [Fact]
    public void UpdatingConnectionUsesItsExistingRegistryIdentity()
    {
        var original = new ConnectionProperties { ConnectionName = "old_connection", GuidID = "existing-id", ID = 42 };
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { original });
        ConnectionProperties? replacement = null;
        config.Setup(x => x.UpdateDataConnection(It.IsAny<ConnectionProperties>(), "existing-id"))
            .Callback<ConnectionProperties, string>((value, _) => replacement = value).Returns(true);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        var result = service.UpdateConnection("old_connection", new ConnectionConfig { ConnectionName = "new_connection", DatabaseType = "SqlLite" }, false);
        Assert.True(result.Success);
        Assert.Equal("existing-id", replacement!.GuidID);
        Assert.Equal(42, replacement.ID);
        config.Verify(x => x.RemoveDataConnection(It.IsAny<string>()), Times.Never);
        config.Verify(x => x.SaveDataconnectionsValues(), Times.Once);
    }

    [Theory]
    [InlineData("invalid name", false)]
    [InlineData("other_connection", false)]
    [InlineData("new_connection", true)]
    public void InvalidOrUnverifiedUpdateDoesNotRemoveExistingConnection(string name, bool testConnection)
    {
        var original = new ConnectionProperties { ConnectionName = "old_connection", GuidID = "existing-id" };
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>());
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { original, new() { ConnectionName = "other_connection", GuidID = "other-id" } });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var failures = new Mock<IFailureReporter>();
        failures.Setup(x => x.ReportHandled(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FailureSeverity>())).Returns("test-reference");
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), failures.Object);
        var result = service.UpdateConnection("old_connection", new ConnectionConfig { ConnectionName = name, DatabaseType = "SqlLite" }, testConnection);
        Assert.False(result.Success);
        Assert.Equal("old_connection", original.ConnectionName);
        config.Verify(x => x.RemoveDataConnection(It.IsAny<string>()), Times.Never);
        config.Verify(x => x.UpdateDataConnection(It.IsAny<ConnectionProperties>(), It.IsAny<string>()), Times.Never);
        config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
    }

    [Fact]
    public void AmbiguousConnectionIdentityIsNotUpdated()
    {
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> {
            new() { ConnectionName = "old_connection", GuidID = "same-id" },
            new() { ConnectionName = "other_connection", GuidID = "same-id" } });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        var result = service.UpdateConnection("old_connection", new ConnectionConfig { ConnectionName = "new_connection", DatabaseType = "SqlLite" }, false);
        Assert.False(result.Success);
        Assert.Contains("ambiguous", result.Message);
        config.Verify(x => x.UpdateDataConnection(It.IsAny<ConnectionProperties>(), It.IsAny<string>()), Times.Never);
        config.Verify(x => x.SaveDataconnectionsValues(), Times.Never);
    }

    [Fact]
    public void ConnectionDetailsDoNotReturnStoredCredentials()
    {
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> {
            new() { ConnectionName = "system_connection", DatabaseType = DataSourceType.SqlServer,
                UserID = "operator", Password = "separate-secret", ConnectionString = "Server=db;Password=string-secret" } });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
            Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
        var result = service.GetConnectionByName("system_connection");
        Assert.NotNull(result);
        Assert.True(string.IsNullOrEmpty(result.Password));
        Assert.True(string.IsNullOrEmpty(result.ConnectionString));
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.DoesNotContain("separate-secret", json);
        Assert.DoesNotContain("string-secret", json);
    }

    [Fact]
    public async Task RegisteredConnectionTestUsesStoredCredentialsOnlyOnServer()
    {
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> {
            new() { ConnectionName = "system_connection", DatabaseType = DataSourceType.SqlServer,
                UserID = "operator", Password = "separate-secret", ConnectionString = "Server=db;Password=string-secret" } });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        var setup = new Mock<IPPDM39SetupService>();
        ConnectionConfig? tested = null;
        setup.Setup(x => x.TestConnectionAsync(It.IsAny<ConnectionConfig>())).Callback<ConnectionConfig>(value => tested = value)
            .ReturnsAsync(new Beep.OilandGas.Models.Data.DataManagement.ConnectionTestResult { Success = true, Message = "Connection successful" });
        var service = new ConnectionService(editor.Object, setup.Object, NullLogger<ConnectionService>.Instance);
        var result = await service.TestConnectionAsync("system_connection");
        Assert.True(result.Success);
        Assert.Equal("separate-secret", tested!.Password);
        Assert.Contains("string-secret", tested.ConnectionString);
        var json = System.Text.Json.JsonSerializer.Serialize(result);
        Assert.DoesNotContain("separate-secret", json);
        Assert.DoesNotContain("string-secret", json);
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("postgres")]
    [InlineData("mysql")]
    [InlineData("oracle")]
    [InlineData("sqlite")]
    public async Task CachedRelationalDriverRestoresWithoutDownloading(string database)
    {
        var driver = database switch
        {
            "sqlserver" => ConnectionHelper.CreateSqlServerConfig(),
            "postgres" => ConnectionHelper.CreatePostgreConfig(),
            "mysql" => ConnectionHelper.CreateMySqlConfig(),
            "oracle" => ConnectionHelper.CreateOracleConfig(),
            "sqlite" => ConnectionHelper.CreateSQLiteConfig(),
            _ => throw new ArgumentOutOfRangeException(nameof(database))
        };
        var config = new Mock<IConfigEditor>();
        var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        var messages = new List<string>(); var log = new Mock<IDMLogger>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!).Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        await service.RestoreCachedAsync(default);
        var result = Assert.Single(service.Catalog());
        Assert.True(result.ConnectorLoaded && result.ProviderLoaded && result.Loaded,
            $"{database}: connector={result.ConnectorLoaded}, provider={result.ProviderLoaded}. " + string.Join(" | ", messages.TakeLast(20)));
    }

    [Theory]
    [InlineData("sqlserver")]
    [InlineData("postgres")]
    [InlineData("mysql")]
    [InlineData("oracle")]
    public async Task ConfiguredRelationalInstallerLoadsConnectorAndProvider(string database)
    {
        var driver = database switch
        {
            "sqlserver" => ConnectionHelper.CreateSqlServerConfig(),
            "postgres" => ConnectionHelper.CreatePostgreConfig(),
            "mysql" => ConnectionHelper.CreateMySqlConfig(),
            "oracle" => ConnectionHelper.CreateOracleConfig(),
            _ => throw new ArgumentOutOfRangeException(nameof(database))
        };
        var config = new Mock<IConfigEditor>();
        var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        var messages = new List<string>(); var log = new Mock<IDMLogger>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var networkDownload = Environment.GetEnvironmentVariable("OILGAS_TEST_NETWORK_DOWNLOAD") == "1";
        if (networkDownload)
            handler.AddNuGetSource("nuget.org", "https://api.nuget.org/v3/index.json");
        else
        {
            handler.AddNuGetSource("test-local", Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED")!);
            handler.DisableNuGetSource("nuget.org");
        }
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!)
            .AddInMemoryCollection(networkDownload
                ? new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = "https://api.nuget.org/v3/index.json" }
                : new Dictionary<string, string?> { ["SetupDrivers:PackageSources:0"] = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED") })
            .Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        try
        {
            var result = await service.InstallAsync(driver.GuidID, Assert.Single(service.Catalog(), x => x.Id == driver.GuidID), () => Task.CompletedTask, default);
            Assert.True(result.ConnectorLoaded && result.ProviderLoaded && result.Loaded);
            if (networkDownload)
                Assert.True(Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("NUGET_PACKAGES")!, result.ConnectorPackage!.ToLowerInvariant(), result.ConnectorVersion!.ToLowerInvariant())),
                    "The connector must be downloaded into the isolated package cache.");
            using var connection = (IDbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!;
            Assert.NotNull(connection);
        }
        catch (Exception error) { throw new InvalidOperationException(string.Join(" | ", messages.TakeLast(20)), error); }
    }
#endif

    [Fact]
    public async Task SystemSetupSqliteMigrationHonorsPreflightAndTargetOwnership()
    {
        var path = Path.Combine(Path.GetTempPath(), "oilgas-system-migration-" + Guid.NewGuid().ToString("N") + ".db");
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
#if BEEP_SOURCE_TEST
        var ownershipPath = Path.Combine(Path.GetTempPath(), "oilgas-migration-ownership-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ownershipPath);
        var migrationStorage = new TheTechIdea.Beep.Services.Persistence.FileMigrationExecutionStorage(ownershipPath, new TheTechIdea.Beep.JsonLoaderService.JsonLoader());
        config.As<IMigrationExecutionStorageProvider>().Setup(x => x.CaptureMigrationExecutionStorage()).Returns(migrationStorage);
#endif
        var driver = ConnectionHelper.CreateSQLiteConfig();
        var connection = new ConnectionProperties { ConnectionName = "system_migration", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties> { connection });
        config.SetupGet(x => x.DataTypesMap).Returns(TheTechIdea.Beep.Helpers.DataTypesHelpers.DatabaseTypeMappingRepository.GenerateSQLiteDataTypesMapping());
        var log = new Mock<IDMLogger>(); var messages = new List<string>(); log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var queries = new TheTechIdea.Beep.ConfigUtil.Managers.QueryManager(log.Object, Mock.Of<IJsonLoader>(), string.Empty);
        config.Setup(x => x.GetSql(It.IsAny<Sqlcommandtype>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<QuerySqlRepo>>(), It.IsAny<DataSourceType>()))
            .Returns((Sqlcommandtype kind, string table, string schema, string filter, List<QuerySqlRepo> catalog, DataSourceType provider) => queries.GetSql(kind, table, schema ?? string.Empty, filter, provider));
        var errors = new ErrorsInfo(); var util = new TheTechIdea.Beep.Utils.Util(log.Object, errors, config.Object);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, util);
        using var editor = new DMEEditor(log.Object, util, errors, config.Object, handler); util.DME = editor;
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "TheTechIdea.Beep.SqliteDatasourceCore", ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.58",
          ["SetupDrivers:Providers:SQLiteDataSource:Package"] = "Stub.System.Data.SQLite.Core.NetStandard", ["SetupDrivers:Providers:SQLiteDataSource:Version"] = "1.0.119", ["SystemSetup:ConnectionName"] = connection.ConnectionName, ["SystemSetup:ConnectionTarget"] = ModuleDatabaseTarget.Key(editor, connection) }).Build();
        try
        {
#if BEEP_SOURCE_TEST
            handler.ScanAssembly(typeof(SQLiteDataSource).Assembly);
#endif
            var drivers = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance);
            await drivers.RestoreCachedAsync(default);
            Assert.True(Assert.Single(drivers.Catalog()).Loaded, string.Join(" | ", messages.TakeLast(30)));
            using (var db = (IDbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!)
            { db.ConnectionString = connection.ConnectionString; db.Open(); }
            var source = editor.GetDataSource(connection.ConnectionName);
            Assert.NotNull(source); Assert.True(source.Openconnection() == ConnectionState.Open, errors.Message + " | " + string.Join(" | ", messages.TakeLast(12)));
            var migration = new MigrationManager(editor, source);
#if BEEP_SOURCE_TEST
            var physicalIdentity = ModuleDatabaseTarget.Key(editor, connection);
            var alias = new ConnectionProperties { ConnectionName = "alias", DatabaseType = DataSourceType.SqlLite, ConnectionString = connection.ConnectionString, Database = path };
            Assert.Equal(physicalIdentity, ModuleDatabaseTarget.Key(editor, alias));
            migration.ExecutionTargetIdentity = physicalIdentity;
#endif
            var types = new[] { typeof(SYSTEM_SETUP_RECORD), typeof(COMPANY_OILGAS_ENVIRONMENT), typeof(COMPANY_GHG_CATALOG_SELECTION) };
#if BEEP_SOURCE_TEST
            Assert.Throws<ModuleSchemaMismatchException>(() => ModuleSchemaVerification.Verify(editor, source, types));
#endif
            var plan = migration.BuildMigrationPlanForTypes(types, detectRelationships: true);
            Assert.NotNull(plan);
            Assert.Equal(3, plan.Operations.Count(x => x.Kind == MigrationPlanOperationKind.CreateEntity));
            source.CheckEntityExist("__beep_preflight_probe__");
            Assert.Equal(Errors.Ok, source.ErrorObject.Flag);
#if BEEP_SOURCE_TEST
            var target = new SystemSetupTarget(settings, editor);
            var module = new SystemSetupModule(new ModuleSetupContext());
            var orchestrator = new ModuleSetupOrchestrator([module], NullLogger<ModuleSetupOrchestrator>.Instance);
            var failures = new Mock<IFailureReporter>();
            failures.Setup(x => x.ReportHandled(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<FailureSeverity>())).Returns("test-reference");
            var app = new PPDM39SetupService(editor, NullLogger<PPDM39SetupService>.Instance,
                Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), failures.Object,
                moduleSetupOrchestrator: orchestrator, migrationBindingFingerprint: (ids, name) => Task.FromResult(target.FingerprintForMigration(ids, name)),
                migrationTargetIdentity: name => ModuleDatabaseTarget.Key(editor, name));
            var reviewed = await app.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest { ConnectionName = connection.ConnectionName, ModuleIds = ["SYSTEM_SETUP"], EnvironmentTier = "Development" });
            Assert.True(reviewed.Success && reviewed.CanApply, reviewed.Message + " | " + string.Join(" | ", reviewed.PreflightChecks.Select(x => x.Code + ":" + x.Decision)));
            var execution = new SchemaMigrationExecuteRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = reviewed.PlanHash, ExpectedManifestHash = reviewed.ManifestHash };
            Assert.False((await app.ExecuteSchemaMigrationPlanAsync(execution)).Success);
            var originalGuid = connection.GuidID;
            connection.GuidID = Guid.NewGuid().ToString();
            Assert.False((await app.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = reviewed.PlanHash, ExpectedManifestHash = reviewed.ManifestHash, ApprovedBy = "local-test" })).Success);
            connection.GuidID = originalGuid;
            Assert.False((await app.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = "changed", ExpectedManifestHash = reviewed.ManifestHash, ApprovedBy = "local-test" })).Success);
            Assert.False((await app.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = reviewed.PlanHash, ExpectedManifestHash = "changed", ApprovedBy = "local-test" })).Success);
            Assert.False((await app.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest { PlanId = reviewed.PlanId, ApprovedBy = "local-test" })).Success);
            Assert.False((await app.ExecuteSchemaMigrationPlanAsync(execution)).Success);
            Assert.True((await app.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = reviewed.PlanHash, ExpectedManifestHash = reviewed.ManifestHash, ApprovedBy = "local-test" })).Success);
            Assert.False((await app.ExecuteSchemaMigrationPlanAsync(new SchemaMigrationExecuteRequest { PlanId = reviewed.PlanId, ExpectedPlanHash = "changed", ExpectedManifestHash = reviewed.ManifestHash })).Success);
            using (var before = (System.Data.Common.DbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!)
            {
                before.ConnectionString = connection.ConnectionString; before.Open();
                var existing = before.GetSchema("Tables").Rows.Cast<DataRow>().Select(row => row["TABLE_NAME"].ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var type in types) Assert.DoesNotContain(type.Name, existing);
            }
            var result = await app.ExecuteSchemaMigrationPlanAsync(execution);
            Assert.True(result.Success, result.Message + " | " + errors.Message);
            ModuleSchemaVerification.Verify(editor, source, types);
            using (var verification = (System.Data.Common.DbConnection)Activator.CreateInstance(handler.GetType(driver.DbConnectionType))!)
            {
                verification.ConnectionString = connection.ConnectionString;
                verification.Open();
                var names = verification.GetSchema("Tables").Rows.Cast<DataRow>().Select(row => row["TABLE_NAME"].ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var type in types) Assert.Contains(type.Name, names);
            }
            foreach (var type in types)
            {
                var expected = editor.classCreator.ConvertToEntityStructure(type);
                var actual = source.GetEntityStructure(new EntityStructure { EntityName = expected.EntityName, DatasourceEntityName = expected.EntityName }, true);
                Assert.NotNull(actual);
                foreach (var field in expected.Fields)
                {
                    var column = Assert.Single(actual.Fields, item => string.Equals(item.FieldName, field.FieldName, StringComparison.OrdinalIgnoreCase));
                    if (field.IsKey) { Assert.True(column.IsKey); Assert.False(column.AllowDBNull); }
                    if (!field.AllowDBNull) Assert.False(column.AllowDBNull);
                }
            }
            var metadata = new Mock<IPPDMMetadataRepository>();
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("SYSTEM_SETUP_RECORD")).ReturnsAsync(["SYSTEM_SETUP_RECORD_ID"]);
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("COMPANY_OILGAS_ENVIRONMENT")).ReturnsAsync(["COMPANY_OILGAS_ENVIRONMENT_ID"]);
            var records = new SystemSetupRecordStore(target, editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
            Assert.Null(await records.ReadAsync("PPDM", "DEFAULT", () => Task.CompletedTask));
            var request = new SystemSetupRequest("PPDM", "DEFAULT", "1", "{\"schema\":\"PPDM39\"}", "", Guid.NewGuid().ToString("N"));
            var saved = await records.SaveAsync(request, "local-admin", () => Task.CompletedTask);
            Assert.Equal(saved, await records.ReadAsync("PPDM", "DEFAULT", () => Task.CompletedTask));
            Assert.Equal(saved, await records.SaveAsync(request, "local-admin", () => Task.CompletedTask));
            var update = request with { Version = "2", ExpectedRevision = saved.Revision, IntentId = Guid.NewGuid().ToString("N") };
            var revised = await records.SaveAsync(update, "local-admin", () => Task.CompletedTask);
            Assert.Equal(saved.Revision, revised.PreviousRevision);
            await Assert.ThrowsAsync<InvalidOperationException>(() => records.SaveAsync(update with { IntentId = Guid.NewGuid().ToString("N") }, "local-admin", () => Task.CompletedTask));
            var samplePolicy = new Mock<IPPDMRowAccessPolicy>();
            samplePolicy.Setup(x => x.GetReadConjunctionsAsync(typeof(SYSTEM_SETUP_RECORD), "SYSTEM_SETUP_RECORD"))
                .ReturnsAsync(new List<IReadOnlyList<TheTechIdea.Beep.Report.AppFilter>> { new[] { new TheTechIdea.Beep.Report.AppFilter { FieldName = "CATEGORY", Operator = "=", FilterValue = "PPDM" } } });
            samplePolicy.Setup(x => x.GetReadConstraintsAsync(typeof(SYSTEM_SETUP_RECORD), "SYSTEM_SETUP_RECORD"))
                .ReturnsAsync(Array.Empty<TheTechIdea.Beep.Report.AppFilter>());
            samplePolicy.Setup(x => x.ValidateRowsAsync(typeof(SYSTEM_SETUP_RECORD), "SYSTEM_SETUP_RECORD", It.IsAny<IReadOnlyList<object>>()))
                .Returns((Type _, string _, IReadOnlyList<object> rows) => rows.Cast<SYSTEM_SETUP_RECORD>().All(row => row.CATEGORY == "PPDM")
                    ? Task.CompletedTask : Task.FromException(new UnauthorizedAccessException("Foreign sample row")));
            var sampleRepo = new Beep.OilandGas.PPDM39.Core.PPDMGenericRepository(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(),
                metadata.Object, typeof(SYSTEM_SETUP_RECORD), connection.ConnectionName, "SYSTEM_SETUP_RECORD", rowAccess: samplePolicy.Object,
                requiredReadColumns: ["SYSTEM_SETUP_RECORD_ID"]);
            var sample = await sampleRepo.GetScopedSampleAsync(1);
            Assert.Equal("PPDM", Assert.IsType<SYSTEM_SETUP_RECORD>(Assert.Single(sample)).CATEGORY);
            // WELL is generated without CLR key annotations; describe the reviewed test projection explicitly to Beep Migration.
            var wellType = typeof(Beep.OilandGas.PPDM39.Models.WELL);
            var wellModel = new MigrationModel { Source = "FocusedTest", SourceId = "WELL-review-projection" };
            wellModel.Entities.Add(wellType.FullName!, new MigrationModelEntity { ClrTypeFullName = wellType.FullName!, TableName = "WELL",
                Properties = new[] { "UWI", "OPERATOR", "ASSIGNED_FIELD", "WELL_NAME", "WELL_NUM" }.Select(name => new MigrationModelProperty
                    { PropertyName = name, ColumnName = name, FieldType = typeof(string).FullName!, IsPrimaryKey = name == "UWI", IsNullable = name != "UWI", MaxLength = 128 }).ToList() });
            var wellPlan = migration.BuildMigrationPlanForModel(wellModel, detectRelationships: false);
            migration.ApproveMigrationPlan(wellPlan, "local-test");
            var wellMigration = migration.ExecuteMigrationPlan(wellPlan);
            Assert.True(wellMigration.Success, wellMigration.Message + " | " + errors.Message);
            var companyWell = new Beep.OilandGas.PPDM39.Models.WELL { UWI = "well-company", OPERATOR = "company", ASSIGNED_FIELD = "field", WELL_NAME = "Alpha", WELL_NUM = "1" };
            var foreignWell = new Beep.OilandGas.PPDM39.Models.WELL { UWI = "well-foreign", OPERATOR = "other", ASSIGNED_FIELD = "field", WELL_NAME = "Beta", WELL_NUM = "2" };
            Assert.Equal(Errors.Ok, source.InsertEntity("WELL", companyWell).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("WELL", foreignWell).Flag);
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("WELL")).ReturnsAsync(["UWI"]);
            var wellPolicy = new Mock<IPPDMRowAccessPolicy>();
            wellPolicy.Setup(x => x.GetReadConjunctionsAsync(typeof(Beep.OilandGas.PPDM39.Models.WELL), "WELL"))
                .ReturnsAsync(new List<IReadOnlyList<TheTechIdea.Beep.Report.AppFilter>> { new[] { new TheTechIdea.Beep.Report.AppFilter { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = "field" } } });
            wellPolicy.Setup(x => x.GetReadConstraintsAsync(typeof(Beep.OilandGas.PPDM39.Models.WELL), "WELL"))
                .ReturnsAsync(new[] { new TheTechIdea.Beep.Report.AppFilter { FieldName = "OPERATOR", Operator = "=", FilterValue = "company" } });
            wellPolicy.Setup(x => x.ValidateRowsAsync(typeof(Beep.OilandGas.PPDM39.Models.WELL), "WELL", It.IsAny<IReadOnlyList<object>>()))
                .Returns((Type _, string _, IReadOnlyList<object> rows) => rows.Cast<Beep.OilandGas.PPDM39.Models.WELL>().All(row => row.OPERATOR == "company" && row.ASSIGNED_FIELD == "field")
                    ? Task.CompletedTask : Task.FromException(new UnauthorizedAccessException("Foreign well")));
            var wellRepo = new Beep.OilandGas.PPDM39.Core.PPDMGenericRepository(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object,
                typeof(Beep.OilandGas.PPDM39.Models.WELL), connection.ConnectionName, "WELL", rowAccess: wellPolicy.Object, requiredReadColumns: ["UWI"]);
            var visibleWells = await wellRepo.GetScopedSampleAsync(20);
            Assert.Equal("well-company", Assert.IsType<Beep.OilandGas.PPDM39.Models.WELL>(Assert.Single(visibleWells)).UWI);
            // These are deliberately small test projections, not the full production PPDM schema.
            var orderType = typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER);
            var componentType = typeof(Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT);
            var orderModel = new MigrationModel { Source = "FocusedTest", SourceId = "work-order-sample-projection" };
            orderModel.Entities.Add(orderType.FullName!, new MigrationModelEntity { ClrTypeFullName = orderType.FullName!, TableName = "WORK_ORDER",
                Properties = new[] { "WORK_ORDER_ID", "WORK_ORDER_NUMBER", "WORK_ORDER_TYPE", "ACTIVE_IND" }.Select(name => new MigrationModelProperty
                { PropertyName = name, ColumnName = name, FieldType = typeof(string).FullName!, IsPrimaryKey = name == "WORK_ORDER_ID", IsNullable = name != "WORK_ORDER_ID", MaxLength = 128 }).ToList() });
            orderModel.Entities.Add(componentType.FullName!, new MigrationModelEntity { ClrTypeFullName = componentType.FullName!, TableName = "WORK_ORDER_COMPONENT",
                Properties = new[] { "WORK_ORDER_ID", "COMPONENT_ID", "FIELD_ID", "ACTIVE_IND" }.Select(name => new MigrationModelProperty
                { PropertyName = name, ColumnName = name, FieldType = typeof(string).FullName!, IsPrimaryKey = name is "WORK_ORDER_ID" or "COMPONENT_ID", IsNullable = name is not ("WORK_ORDER_ID" or "COMPONENT_ID"), MaxLength = 128 }).ToList() });
            var orderPlan = migration.BuildMigrationPlanForModel(orderModel, detectRelationships: false);
            migration.ApproveMigrationPlan(orderPlan, "local-test");
            var orderMigration = migration.ExecuteMigrationPlan(orderPlan);
            Assert.True(orderMigration.Success, orderMigration.Message + " | " + errors.Message);
            Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER", new Beep.OilandGas.PPDM39.Models.WORK_ORDER { WORK_ORDER_ID = "order-a", WORK_ORDER_NUMBER = "A", WORK_ORDER_TYPE = "INSPECTION", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER", new Beep.OilandGas.PPDM39.Models.WORK_ORDER { WORK_ORDER_ID = "order-b", WORK_ORDER_NUMBER = "B", WORK_ORDER_TYPE = "INSPECTION", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER_COMPONENT", new Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order-a", COMPONENT_ID = "one", FIELD_ID = "field-a", ACTIVE_IND = "Y" }).Flag);
            Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER_COMPONENT", new Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order-b", COMPONENT_ID = "two", FIELD_ID = "field-b", ACTIVE_IND = "Y" }).Flag);
            for (var i = 0; i < 24; i++)
            {
                var id = $"order-extra-{i:D2}";
                Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER", new Beep.OilandGas.PPDM39.Models.WORK_ORDER { WORK_ORDER_ID = id, WORK_ORDER_NUMBER = id, WORK_ORDER_TYPE = "INSPECTION", ACTIVE_IND = "Y" }).Flag);
                Assert.Equal(Errors.Ok, source.InsertEntity("WORK_ORDER_COMPONENT", new Beep.OilandGas.PPDM39.Models.WORK_ORDER_COMPONENT { WORK_ORDER_ID = id, COMPONENT_ID = "one", FIELD_ID = "field-a", ACTIVE_IND = "Y" }).Flag);
            }
            var workOrders = new Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderService(editor, new CommonColumnHandler(),
                Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IInspectionService>(),
                () => Task.FromResult((new CompanyScope("tenant_one", "company_kw"), "actor", new AssetDatabaseScope(connection.ConnectionName, "fixture-fingerprint"))),
                NullLogger<Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderService>.Instance, (_, _, _, _) => Task.CompletedTask);
            var sampledOrders = await workOrders.GetByFieldSampleAsync("field-a", 20);
            Assert.Equal(20, sampledOrders.Count); Assert.DoesNotContain(sampledOrders, row => row.InstanceId == "order-b");
            Assert.All(sampledOrders, row => Assert.Equal("field-a", row.FieldId));
            source.Closeconnection();
            Assert.Equal(revised, await records.ReadAsync("PPDM", "DEFAULT", () => Task.CompletedTask));
            var companies = new CompanyEnvironmentStore(editor, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object,
                () => Task.FromResult(target.Resolve().ConnectionName), records.RequireReviewedDraftAsync);
            var effective = new CompanyEnvironmentEffectiveResolver(companies, records);
            var kuwait = new CompanyScope("tenant_one", "company_kw"); var norway = new CompanyScope("tenant_two", "company_no");
            var kwDraft = new CompanyEnvironmentPreviewRequest("KW", "{\"culture\":\"ar-KW\"}");
            var noDraft = new CompanyEnvironmentPreviewRequest("NO", "{\"culture\":\"nb-NO\"}");
            var missingDefaults = await Assert.ThrowsAsync<InvalidOperationException>(() => effective.PreviewDraftAsync(kuwait, kwDraft, () => Task.CompletedTask));
            Assert.Contains("TOOLS/DEFAULT", missingDefaults.Message);
            var generatedDefaults = SystemSetupToolsDefaults.Draft();
            Assert.True(generatedDefaults.SettingsJson.Length > 65536);
            var shared = await records.SaveAsync(new SystemSetupRequest("TOOLS", "DEFAULT", generatedDefaults.ToolsVersion, generatedDefaults.SettingsJson, "", Guid.NewGuid().ToString("N")), "local-admin", () => Task.CompletedTask);
            Assert.Equal(generatedDefaults.SettingsJson, (await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask))!.SettingsJson);
            var kwReview = await effective.PreviewDraftAsync(kuwait, kwDraft, () => Task.CompletedTask);
            var noReview = await effective.PreviewDraftAsync(norway, noDraft, () => Task.CompletedTask);
            var kwCommand = new CompanyEnvironmentReviewedSaveRequest(new("KW", kwDraft.OverridesJson, "", Guid.NewGuid().ToString("N")), kwReview);
            var noCommand = new CompanyEnvironmentReviewedSaveRequest(new("NO", noDraft.OverridesJson, "", Guid.NewGuid().ToString("N")), noReview);
            var kwSaved = await companies.SaveAsync(kuwait, "local-admin", kwCommand, () => Task.CompletedTask);
            Assert.Null(await companies.ReadAsync(norway, () => Task.CompletedTask));
            var noSaved = await companies.SaveAsync(norway, "local-admin", noCommand, () => Task.CompletedTask);
            Assert.Equal(kwSaved, await companies.ReadAsync(kuwait, () => Task.CompletedTask));
            Assert.Equal(noSaved, await companies.ReadAsync(norway, () => Task.CompletedTask));
            Assert.Equal("KW", (await effective.ResolveAsync(kuwait, () => Task.CompletedTask)).Settings.CountryCode);
            Assert.Equal("NO", (await effective.ResolveAsync(norway, () => Task.CompletedTask)).Settings.CountryCode);
            Assert.Equal(shared.Revision, kwSaved.ReviewedSystemRevision);
            await records.SaveAsync(new SystemSetupRequest("TOOLS", "DEFAULT", "2", "{}", shared.Revision, Guid.NewGuid().ToString("N")), "local-admin", () => Task.CompletedTask);
            Assert.Equal("ReviewRequired", (await effective.ResolveAsync(kuwait, () => Task.CompletedTask)).ReviewStatus);
            Assert.Equal("ReviewRequired", (await effective.ResolveAsync(norway, () => Task.CompletedTask)).ReviewStatus);
            var change = new CompanyEnvironmentSaveRequest("KW", kwDraft.OverridesJson, kwSaved.Revision, Guid.NewGuid().ToString("N"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => companies.SaveAsync(kuwait, "local-admin", new(change, kwReview), () => Task.CompletedTask));
            Assert.Equal(kwSaved, await companies.ReadAsync(kuwait, () => Task.CompletedTask));
            var refreshed = await effective.PreviewDraftAsync(kuwait, kwDraft, () => Task.CompletedTask);
            var kwRefreshed = await companies.SaveAsync(kuwait, "local-admin", new(change, refreshed), () => Task.CompletedTask);
            Assert.Equal("Current", (await effective.ResolveAsync(kuwait, () => Task.CompletedTask)).ReviewStatus);
            Assert.Equal("ReviewRequired", (await effective.ResolveAsync(norway, () => Task.CompletedTask)).ReviewStatus);
            var sharedCurrent = await records.ReadAsync("TOOLS", "DEFAULT", () => Task.CompletedTask);
            const string sharedSource = "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"X\":\"Tubing\"}}}}}}";
            await records.SaveAsync(new("TOOLS", "DEFAULT", "3", sharedSource, sharedCurrent!.Revision, Guid.NewGuid().ToString("N")), "local-admin", () => Task.CompletedTask);
            const string companyOverlay = "{\"dataSources\":{\"well\":{\"codes\":{\"string\":{\"kind\":{\"X\":\"Casing\"}}}}}}";
            var overlayDraft = new CompanyEnvironmentPreviewRequest("KW", companyOverlay);
            var overlayReview = await effective.PreviewDraftAsync(kuwait, overlayDraft, () => Task.CompletedTask);
            Assert.Empty(overlayReview.Settings.Warnings);
            var overlaySaved = await companies.SaveAsync(kuwait, "local-admin", new(new("KW", companyOverlay, kwRefreshed.Revision, Guid.NewGuid().ToString("N")), overlayReview), () => Task.CompletedTask);
            Assert.Equal(overlaySaved, await companies.ReadAsync(kuwait, () => Task.CompletedTask));
            var runtime = await effective.ResolveRuntimeAsync(kuwait, () => Task.CompletedTask);
            Assert.Equal("PPDM_CORE", runtime.Environment.DataSources["well"].Path);
            Assert.Equal("Casing", runtime.Environment.DataSources["well"].Codes["string"]["kind"]["X"]);
            Assert.Equal("ReviewRequired", (await effective.ResolveAsync(norway, () => Task.CompletedTask)).ReviewStatus);
#else
            migration.ApproveMigrationPlan(plan, "local-test");
            var result = migration.ExecuteMigrationPlan(plan);
            Assert.True(result.Success, result.Message);
            foreach (var type in types) Assert.True(source.CheckEntityExist(type.Name), type.Name);
#endif
        }
        finally
        {
            editor.GetDataSource(connection.ConnectionName)?.Closeconnection();
            handler.GetType(driver.DbConnectionType)?.GetMethod("ClearAllPools", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            editor.Dispose();
            if (File.Exists(path))
            {
                try { File.Delete(path); }
                catch (IOException) { Console.WriteLine("Temporary SQLite file remains locked until the test process exits: " + path); }
            }
#if BEEP_SOURCE_TEST
            Directory.Delete(ownershipPath, true);
#endif
        }
    }

    [Fact]
    public async Task StartupLoadsAvailableSqliteAndOpensDatabase()
    {
        var config = new Mock<IConfigEditor>(); var classes = new List<AssemblyClassDefinition>();
        var driver = ConnectionHelper.CreateSQLiteConfig();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties>());
        var messages = new List<string>(); var log = new Mock<IDMLogger>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var settingsDirectory = Path.Combine(Path.GetTempPath(), "oilgas-system-selection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(settingsDirectory);
        var settingsPath = Path.Combine(settingsDirectory, "system-setup-target.json");
        var settings = new ConfigurationBuilder().AddJsonFile(settingsPath, optional: true).AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "TheTechIdea.Beep.SqliteDatasourceCore", ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.58",
          ["SetupDrivers:Providers:SQLiteDataSource:Package"] = "Stub.System.Data.SQLite.Core.NetStandard", ["SetupDrivers:Providers:SQLiteDataSource:Version"] = "1.0.119", ["Beep:ConfigPath"] = settingsDirectory }).Build();
        var otherProcessSettings = new ConfigurationBuilder().AddJsonFile(settingsPath, optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = settingsDirectory }).Build();
        using var editor = new DMEEditor(log.Object, Mock.Of<IUtil>(), new ErrorsInfo(), config.Object, handler);
        var service = new SetupDriverService(editor, settings, NullLogger<SetupDriverService>.Instance);
        await service.RestoreCachedAsync(default);
        Assert.True(Assert.Single(service.Catalog()).Loaded, string.Join(" | ", messages.TakeLast(25)));
#if BEEP_SOURCE_TEST
        Assert.True(Assert.Single(service.Catalog()).BundledConnector);
#endif
        var path = Path.Combine(Path.GetTempPath(), "oilgas-restart-" + Guid.NewGuid().ToString("N") + ".db");
        try
        {
            var proposed = new ConnectionProperties { ConnectionName = "startup_probe", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
            Assert.Equal(ConnectionState.Open, ConnectionProbe.Test(editor, proposed));
            Assert.Empty(config.Object.DataConnections);
            var registered = new ConnectionProperties { ConnectionName = "system_test", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
            config.Object.DataConnections.Add(registered);
            var selection = new SystemSetupConnectionSelectionService(settings, editor);
            Assert.Equal("system_test", (await selection.SelectAsync("system_test", () => Task.CompletedTask, default)).ConnectionName);
            Assert.Equal("system_test", settings["SystemSetup:ConnectionName"]);
            Assert.Equal(ModuleDatabaseTarget.Key(editor, registered), settings["SystemSetup:ConnectionTarget"]);
            Assert.Null(otherProcessSettings["SystemSetup:ConnectionName"]);
            SystemSetupConnectionSelectionService.ReloadPersistedSelectionIfNeeded(otherProcessSettings);
            Assert.Equal("system_test", otherProcessSettings["SystemSetup:ConnectionName"]);
            Assert.Equal("system_test", (await new SystemSetupConnectionSelectionService(otherProcessSettings, editor)
                .SelectAsync("system_test", () => Task.CompletedTask, default)).ConnectionName);
#if BEEP_SOURCE_TEST
            var target = new SystemSetupTarget(settings, editor);
            Assert.Equal("system_test", target.Resolve().ConnectionName);
            registered.ConnectionString = "Data Source=" + Path.Combine(Path.GetTempPath(), "other-" + Guid.NewGuid().ToString("N") + ".db");
            Assert.Throws<InvalidOperationException>(() => target.Resolve());
#endif
        }
        finally { editor.GetDataSource("system_test")?.Closeconnection(); if (File.Exists(path)) File.Delete(path); Directory.Delete(settingsDirectory, true); }
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{not-json}")]
    public void IncompleteOrInvalidSavedSystemTargetDoesNotLookUnconfigured(string contents)
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-invalid-system-target-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "system-setup-target.json");
        try
        {
            var settings = new ConfigurationBuilder().AddJsonFile(path, optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory }).Build();
            File.WriteAllText(path, contents);
            Assert.Throws<InvalidOperationException>(() => SystemSetupConnectionSelectionService.ReloadPersistedSelectionIfNeeded(settings));
            Assert.Equal(contents, File.ReadAllText(path));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public async Task InstallerLoadsConnectorAndProviderFromLocalNuGetFeed()
    {
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED")!;
        var config = new Mock<IConfigEditor>();
        var classes = new List<AssemblyClassDefinition>();
        var driver = ConnectionHelper.CreateSQLiteConfig();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        config.SetupProperty(x => x.DataConnections, new List<ConnectionProperties>());
        var messages = new List<string>(); var log = new Mock<IDMLogger>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        using var handler = new AssemblyHandler(config.Object, new ErrorsInfo(), log.Object, Mock.Of<IUtil>());
        var networkDownload = Environment.GetEnvironmentVariable("OILGAS_TEST_NETWORK_DOWNLOAD") == "1";
        if (networkDownload)
            handler.AddNuGetSource("nuget.org", "https://api.nuget.org/v3/index.json");
        var editor = new Mock<IDMEEditor>();
        editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
        editor.SetupGet(x => x.assemblyHandler).Returns(handler);
        var settings = networkDownload
            ? new ConfigurationBuilder().AddJsonFile(Environment.GetEnvironmentVariable("OILGAS_TEST_APPSETTINGS")!).Build()
            : new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["SetupDrivers:Connectors:SQLiteDataSource:Package"] = "TheTechIdea.Beep.SqliteDatasourceCore", ["SetupDrivers:Connectors:SQLiteDataSource:Version"] = "1.0.58",
              ["SetupDrivers:PackageSources:0"] = feed,
              ["SetupDrivers:Providers:SQLiteDataSource:Package"] = "Stub.System.Data.SQLite.Core.NetStandard", ["SetupDrivers:Providers:SQLiteDataSource:Version"] = "1.0.119" }).Build();
        var service = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
        try
        {
            var result = await service.InstallAsync(driver.GuidID, Assert.Single(service.Catalog(), x => x.Id == driver.GuidID), () => Task.CompletedTask, default);
            Assert.Equal("Stub.System.Data.SQLite.Core.NetStandard", result.Package);
            Assert.True(result.ConnectorLoaded); Assert.True(result.ProviderLoaded); Assert.True(result.Loaded);
            if (networkDownload)
                Assert.True(Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("NUGET_PACKAGES")!, "thetechidea.beep.sqlitedatasourcecore", "1.0.51")),
                    "The connector must be downloaded into the isolated package cache.");
            var type = handler.GetType(driver.DbConnectionType);
            Assert.Equal("System.Data.SQLite.SQLiteConnection", type.FullName);
            var path = Path.Combine(Path.GetTempPath(), "oilgas-installer-" + Guid.NewGuid().ToString("N") + ".db");
            try
            {
                using var connection = (IDbConnection)Activator.CreateInstance(type)!;
                connection.ConnectionString = "Data Source=" + path;
                connection.Open();
                Assert.Equal(ConnectionState.Open, connection.State);
                connection.Close();
                using var actualEditor = new DMEEditor(log.Object, Mock.Of<IUtil>(), new ErrorsInfo(), config.Object, handler);
                var proposed = new ConnectionProperties { ConnectionName = "installer-probe", DatabaseType = DataSourceType.SqlLite, ConnectionString = "Data Source=" + path, Database = path, GuidID = Guid.NewGuid().ToString() };
                Assert.Equal(ConnectionState.Open, ConnectionProbe.Test(actualEditor, proposed));
                Assert.Empty(config.Object.DataConnections);
            }
            finally { if (File.Exists(path)) File.Delete(path); }
        }
        catch (Exception error) { throw new InvalidOperationException(string.Join(" | ", messages.TakeLast(20)), error); }
    }

    [Fact]
    public async Task NuGetLoaderRegistersSqliteConnectorWithoutGlobalRescan()
    {
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED")
            ?? throw new InvalidOperationException("The isolated test feed was not configured.");
        var config = new Mock<IConfigEditor>();
        var classes = new List<AssemblyClassDefinition>();
        config.SetupGet(x => x.DataSourcesClasses).Returns(classes);
        var messages = new List<string>();
        var log = new Mock<IDMLogger>();
        log.Setup(x => x.WriteLog(It.IsAny<string>())).Callback<string>(messages.Add);
        var errors = new ErrorsInfo();
        Assert.Equal(new Version(2, 0, 22, 0), typeof(RDBSource).Assembly.GetName().Version);
        using var handler = new AssemblyHandler(config.Object, errors, log.Object, Mock.Of<IUtil>());
        var loaded = await handler.LoadNuggetFromNuGetAsync("TheTechIdea.Beep.SqliteDatasourceCore", "1.0.58", [feed], true, null, false);
        Assert.True(loaded.Count > 0, string.Join(" | ", messages.TakeLast(12)) + " " + errors.Message);
        Assert.True(classes.Any(x => x.className == "SQLiteDataSource"),
            "Loaded: " + string.Join(", ", loaded.Select(x => x.FullName)) + " | Catalog: " + string.Join(", ", classes.Select(x => x.className)) + " | " + string.Join(" | ", messages.TakeLast(30)) + " | " + errors.Message);
        var connector = Assert.Single(classes, x => x.className == "SQLiteDataSource");
        Assert.NotNull(connector.type);
        Assert.True(typeof(IDataSource).IsAssignableFrom(connector.type));
    }
}
#endif
