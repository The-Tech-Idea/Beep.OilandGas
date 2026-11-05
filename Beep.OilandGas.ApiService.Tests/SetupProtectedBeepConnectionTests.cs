using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if BEEP_SOURCE_TEST
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.JsonLoaderService;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.Security;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using Xunit;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using ConnectionConfig = Beep.OilandGas.Models.Data.DataManagement.ConnectionConfig;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class WindowsSetupLocalDbFactAttribute : FactAttribute
{
    public WindowsSetupLocalDbFactAttribute() { if (!OperatingSystem.IsWindows()) Skip = "SQL Server LocalDB is available only on Windows."; }
}

public sealed class SetupProtectedBeepConnectionTests
{
    [Fact]
    public void ConnectionUpdateSurvivesConfigEditorRestart()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-connection-update-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) }).Build();
        var protection = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(configuration)));
        using (var config = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, protection))
        {
            Assert.True(config.AddDataConnection(new ConnectionProperties { ConnectionName = "old_connection", GuidID = "existing-id", DatabaseType = DataSourceType.SqlLite }));
            config.SaveDataconnectionsValues();
            var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config);
            var service = new PPDM39SetupService(editor.Object, NullLogger<PPDM39SetupService>.Instance,
                Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), Mock.Of<IFailureReporter>());
            var result = service.UpdateConnection("old_connection", new ConnectionConfig { ConnectionName = "new_connection", DatabaseType = "SqlLite", Database = "updated.db" }, false);
            Assert.True(result.Success, result.Message);
        }
        using var restarted = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, protection);
        restarted.LoadDataConnectionsValues();
        var saved = Assert.Single(restarted.DataConnections);
        Assert.Equal("new_connection", saved.ConnectionName);
        Assert.Equal("existing-id", saved.GuidID);
        Assert.Equal("updated.db", saved.Database);
    }

    [WindowsSetupLocalDbFact]
    public async Task ProtectedBeepSelectionSurvivesRestartAndInstallsRealIdentityRepository()
    {
        const string setupKey = "this-is-a-long-setup-test-key-12345";
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-protected-identity-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = "BeepOilGas_Setup_" + Guid.NewGuid().ToString("N");
        var target = Path.Combine(directory, "identity-repository-target.json");
        var configuration = new ConfigurationBuilder().AddJsonFile(target, optional: true).AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:ConfigPath"] = directory, ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = setupKey,
          ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) }).Build();

        ServiceProvider Host()
        {
            var services = new ServiceCollection(); services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton(Mock.Of<IFailureReporter>());
            services.AddOilGasRepository(configuration);
            services.AddScoped<IRepositoryConnectionSource, BeepRepositoryConnectionSource>();
            services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
                options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true;
                options.ConnectionSecretProtector = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(configuration))); });
            services.AddScoped<SetupBootstrapAccess>();
            services.AddScoped<SetupRepositoryMigrationService>();
            var host = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
            host.GetRequiredService<IDMEEditor>().ConfigEditor.LoadDataConnectionsValues();
            return host;
        }

        using (var firstHost = Host())
        using (var scope = firstHost.CreateScope())
        {
            var editor = scope.ServiceProvider.GetRequiredService<IDMEEditor>();
            editor.ConfigEditor.DataDriversClasses.Add(new ConnectionDriversConfig { GuidID = "test-driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false });
            editor.ConfigEditor.DataSourcesClasses.Add(new AssemblyClassDefinition { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() });
            var access = scope.ServiceProvider.GetRequiredService<SetupBootstrapAccess>();
            Task Guard() => access.RequireAsync(setupKey, default);
            var drivers = new SetupDriverService(editor, configuration, NullLogger<SetupDriverService>.Instance);
            var creation = new SetupRepositoryConnectionCreationService(configuration, editor, drivers);
            var cs = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
            await creation.CreateAsync(new SetupRepositoryConnectionCreate("identity_db", "SQLSERVER", cs, "test-driver"), Guard, default);
            await new SetupRepositoryConnectionSelectionService(configuration, editor).SelectAsync("identity_db", Guard, default);
        }

        using var restartedHost = Host();
        using var restartedScope = restartedHost.CreateScope();
        var restartedEditor = restartedScope.ServiceProvider.GetRequiredService<IDMEEditor>();
        Assert.Equal("identity_db", configuration["Repository:BeepConnectionName"]);
        Assert.Single(restartedEditor.ConfigEditor.DataConnections.Where(x => x.ConnectionName == "identity_db"));
        var actualCandidate = BeepRepositoryConnectionSource.Candidate(restartedEditor, "identity_db");
        Assert.Equal(actualCandidate.Target, configuration["Repository:BeepConnectionTarget"]);
        Assert.NotNull(restartedScope.ServiceProvider.GetRequiredService<IRepositoryConnectionSource>().Resolve());
        var ready = restartedScope.ServiceProvider.GetRequiredService<IRepositoryReadinessService>();
        Assert.Equal(RepositoryReadiness.Unavailable, await ready.CheckAsync());
        var accessAfterRestart = restartedScope.ServiceProvider.GetRequiredService<SetupBootstrapAccess>();
        Task Recheck() => accessAfterRestart.RequireAsync(setupKey, default);
        var migration = restartedScope.ServiceProvider.GetRequiredService<SetupRepositoryMigrationService>();
        var plan = await migration.PlanAsync(Recheck, default);
        Assert.True(plan.CreatesDatabase);
        var result = await migration.ApplyAsync(new(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations), Recheck, default);
        Assert.Equal(RepositoryReadiness.BootstrapRequired, result.Readiness);
        var db = restartedScope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
    }

    [Fact]
    public void BeepRuntimeRegistrationCapturesTheConfiguredProtector()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-protected-runtime-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) }).Build();
        var protector = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(configuration)));
        var services = new ServiceCollection();
        services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
            options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true; options.ConnectionSecretProtector = protector; });
        using var root = services.BuildServiceProvider();
        var editor = root.GetRequiredService<IDMEEditor>();
        Assert.Same(protector, Assert.IsAssignableFrom<IConnectionProtectionContext>(editor.ConfigEditor).ConnectionSecretProtector);
        Assert.Same(editor, root.GetRequiredService<IDMEEditor>());
    }

    [Fact]
    public async Task MissingKeyRefusesSecretBeforeAddingConnection()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-protected-beep-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:CredentialProtection:CurrentKeyId"] = "missing" }).Build();
        var protection = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(configuration)));
        using var config = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, protection);
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config);
        var drivers = new SetupDriverService(editor.Object, configuration, NullLogger<SetupDriverService>.Instance);
        var service = new SetupRepositoryConnectionCreationService(configuration, editor.Object, drivers);
        await Assert.ThrowsAnyAsync<System.Security.Cryptography.CryptographicException>(() => service.CreateAsync(
            new("identity_db", "SQLSERVER", "Server=localhost;Database=IdentityDb;Password=secret", "test-driver"), () => Task.CompletedTask, default));
        Assert.DoesNotContain(config.DataConnections, item => item.ConnectionName == "identity_db");
    }

    [Fact]
    public void KeyProviderRejectsInvalidIdentitiesAndKeyMaterial()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = "not base64" }).Build();
        var keys = new BeepCredentialKeyProvider(config);
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => keys.GetKey("../outside"));
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => keys.GetKey("test"));
        config["Beep:CredentialProtection:Keys:test"] = Convert.ToBase64String(new byte[16]);
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => keys.GetKey("test"));
    }

    [Fact]
    public async Task BootstrapPersistsEncryptedConnectionAndResolvesItAfterReopening()
    {
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-protected-beep-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var targetPath = Path.Combine(directory, "identity-repository-target.json");
        var secret = "test-secret-" + Guid.NewGuid().ToString("N");
        var value = "Server=localhost;Database=IdentityDb;User ID=user;Password=" + secret;
        var configuration = new ConfigurationBuilder().AddJsonFile(targetPath, optional: true).AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:ConfigPath"] = directory, ["Beep:CredentialProtection:CurrentKeyId"] = "test_key", ["Beep:CredentialProtection:Keys:test_key"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) }).Build();
        var protection = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(configuration)));
        string configPath;
        using (var config = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, protection))
        {
            configPath = config.ConfigPath;
            config.DataDriversClasses.Add(new ConnectionDriversConfig { GuidID = "test-driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false });
            config.DataSourcesClasses.Add(new AssemblyClassDefinition { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() });
            var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config);
            var drivers = new SetupDriverService(editor.Object, configuration, NullLogger<SetupDriverService>.Instance);
            var creation = new SetupRepositoryConnectionCreationService(configuration, editor.Object, drivers);
            await creation.CreateAsync(new SetupRepositoryConnectionCreate("identity_db", "SQLSERVER", value, "test-driver"), () => Task.CompletedTask, default);
            await new SetupRepositoryConnectionSelectionService(configuration, editor.Object).SelectAsync("identity_db", () => Task.CompletedTask, default);
        }
        var persisted = await File.ReadAllTextAsync(Path.Combine(configPath, "DataConnections.json"));
        Assert.DoesNotContain(secret, persisted);
        Assert.Contains("__beepenc__:1:aesgcm:", persisted);
        using var reopened = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, protection);
        var editorAfterRestart = new Mock<IDMEEditor>(); editorAfterRestart.SetupGet(x => x.ConfigEditor).Returns(reopened);
        using var services = new ServiceCollection().AddSingleton(editorAfterRestart.Object).BuildServiceProvider();
        var settingsAfterRestart = new ConfigurationBuilder().AddJsonFile(targetPath).Build();
        Assert.Equal(value, new BeepRepositoryConnectionSource(settingsAfterRestart, services).Resolve()!.ConnectionString);
        var wrongKey = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Beep:CredentialProtection:CurrentKeyId"] = "test_key", ["Beep:CredentialProtection:Keys:test_key"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)) }).Build();
        var wrongProtection = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(wrongKey)));
        Assert.ThrowsAny<System.Security.Cryptography.CryptographicException>(() =>
        {
            using var invalid = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), directory, "test", BeepConfigType.DataConnector, wrongProtection);
            invalid.LoadDataConnectionsValues();
        });
        Assert.Equal(persisted, await File.ReadAllTextAsync(Path.Combine(configPath, "DataConnections.json")));
    }
}
#endif
