#if BEEP_SOURCE_TEST
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.Encodings.Web;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers;
using TheTechIdea.Beep.Security;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupFirstRunHttpIntegrationTests
{
    public sealed class BundledLocalDbFactAttribute : FactAttribute
    {
        public BundledLocalDbFactAttribute()
        {
            if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("OILGAS_TEST_BUNDLED_DRIVER") != "1")
                Skip = "Requires Windows LocalDB and a host with the bundled SQL Server connector.";
        }
    }
    public sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(AuthenticateResult.NoResult());
    }

    [WindowsSetupLocalDbFact]
    public Task HttpSetupCreatesSelectsAndMigratesIdentityAfterHostRestart() => RunHttpSetup(false);

    [BundledLocalDbFact]
    public Task HttpSetupUsesActualBundledDriverAndMigratesIdentityAfterRestart() => RunHttpSetup(true);

    [SetupDriverBeepLoaderTests.IsolatedDriverFeedFact]
    public Task HttpSetupDownloadsConnectorAndMigratesIdentityAfterRestart() => RunHttpSetup(false, true);

    [SetupDriverBeepLoaderTests.IsolatedDriverFeedFact]
    public async Task HttpWizardInstallsPackagedConnectorAndRestoresItOnRestart()
    {
        const string key = "this-is-a-long-setup-test-key-12345";
        var feed = Environment.GetEnvironmentVariable("OILGAS_TEST_ISOLATED_DRIVER_FEED")!;
        var version = Environment.GetEnvironmentVariable("OILGAS_TEST_PACKAGED_VERSION")!;
        var packageCache = Environment.GetEnvironmentVariable("NUGET_PACKAGES")!;
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-http-driver-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = "BeepOilGas_HttpDriver_" + Guid.NewGuid().ToString("N");
        var connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true";
        var encryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        await using var databaseContext = new SqlServerRepositoryDbContext(new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connectionString).Options);

        async Task<(WebApplication App, ConnectionDriversConfig Driver)> Start(bool restore)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = key,
                ["SetupDrivers:Connectors:SQLServerDataSource:Package"] = "TheTechIdea.Beep.SqlServerDataSourceCore",
                ["SetupDrivers:Connectors:SQLServerDataSource:Version"] = version,
                ["SetupDrivers:Providers:SQLServerDataSource:Package"] = "System.Data.SqlClient",
                ["SetupDrivers:Providers:SQLServerDataSource:Version"] = "4.9.1",
                ["SetupDrivers:PackageSources:0"] = feed,
                ["SetupDrivers:PackageSources:1"] = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED"),
                ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = encryptionKey });
            builder.Services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
                options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true;
                options.ConnectionSecretProtector = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(builder.Configuration))); });
            var readiness = new Mock<IRepositoryReadinessService>();
            readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.Unavailable);
            builder.Services.AddSingleton(readiness.Object);
            builder.Services.AddScoped<SetupDriverService>(); builder.Services.AddScoped<SetupBootstrapAccess>();
            builder.Services.AddScoped<SetupRepositoryConnectionCreationService>();
            builder.Services.AddScoped<SetupRepositoryConnectionSelectionService>();
            builder.Services.AddControllers().AddApplicationPart(typeof(SetupBootstrapDriverController).Assembly);
            var app = builder.Build();
            var editor = app.Services.GetRequiredService<IDMEEditor>();
            var driver = editor.ConfigEditor.DataDriversClasses.FirstOrDefault(d => d.DatasourceType == DataSourceType.SqlServer && d.classHandler == "SQLServerDataSource");
            if (driver is null) { driver = ConnectionHelper.CreateSqlServerConfig(); editor.ConfigEditor.DataDriversClasses.Add(driver); }
            if (restore)
            {
                await using var scope = app.Services.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<SetupDriverService>().RestoreCachedAsync(default);
            }
            app.UseMiddleware<SetupGateMiddleware>();
            app.MapWhen(context => SetupBootstrapPaths.IsKeyProtected(context.Request.Path), branch => { branch.UseRouting(); branch.UseEndpoints(endpoints => endpoints.MapControllers()); });
            await app.StartAsync();
            return (app, driver);
        }

        try
        {
            await databaseContext.Database.EnsureCreatedAsync();
            var (first, driver) = await Start(false);
            await using (first)
            using (var client = new HttpClient { BaseAddress = new(first.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) })
            {
                var path = "/api/setup/bootstrap/drivers/" + Uri.EscapeDataString(driver.GuidID) + "/install";
                using var scope = first.Services.CreateScope();
                var reviewed = Assert.Single(scope.ServiceProvider.GetRequiredService<SetupDriverService>().Catalog(), x => x.Id == driver.GuidID);
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(path, reviewed)).StatusCode);
                using var request = new HttpRequestMessage(HttpMethod.Post, path); request.Headers.Add("X-Setup-Key", key);
                request.Content = JsonContent.Create(reviewed);
                using var response = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var item = (await response.Content.ReadFromJsonAsync<SetupDriverItem>())!;
                Assert.True(item.Loaded && item.ConnectorLoaded && item.ProviderLoaded);
                Assert.True(Directory.Exists(Path.Combine(packageCache, "thetechidea.beep.sqlserverdatasourcecore", version.ToLowerInvariant())));
                using var create = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connections");
                create.Headers.Add("X-Setup-Key", key);
                create.Content = JsonContent.Create(new SetupRepositoryConnectionCreate("wizard_database", "SQLSERVER", connectionString, item.Id));
                using var created = await client.SendAsync(create);
                Assert.Equal(HttpStatusCode.OK, created.StatusCode);
                Assert.Equal("wizard_database", (await created.Content.ReadFromJsonAsync<SetupRepositoryConnectionSelection>())!.ConnectionName);
                var selection = new SetupRepositoryConnectionSelection("wizard_database");
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/setup/bootstrap/repository/connections/test", selection)).StatusCode);
                using var test = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connections/test");
                test.Headers.Add("X-Setup-Key", key); test.Content = JsonContent.Create(selection);
                using var tested = await client.SendAsync(test);
                Assert.Equal(HttpStatusCode.OK, tested.StatusCode);
                var result = (await tested.Content.ReadFromJsonAsync<SetupRepositoryConnectionTest>())!;
                Assert.Equal("wizard_database", result.ConnectionName); Assert.True(result.Connected);
                using var createMissing = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connections");
                createMissing.Headers.Add("X-Setup-Key", key);
                createMissing.Content = JsonContent.Create(new SetupRepositoryConnectionCreate("missing_database", "SQLSERVER",
                    $"Server=(localdb)\\MSSQLLocalDB;Database=Missing_{Guid.NewGuid():N};Integrated Security=true;TrustServerCertificate=true;Connect Timeout=3", item.Id));
                using var missingCreated = await client.SendAsync(createMissing);
                Assert.Equal(HttpStatusCode.OK, missingCreated.StatusCode);
                using var testMissing = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connections/test");
                testMissing.Headers.Add("X-Setup-Key", key); testMissing.Content = JsonContent.Create(new SetupRepositoryConnectionSelection("missing_database"));
                using var missingTested = await client.SendAsync(testMissing);
                Assert.Equal(HttpStatusCode.OK, missingTested.StatusCode);
                Assert.False((await missingTested.Content.ReadFromJsonAsync<SetupRepositoryConnectionTest>())!.Connected);
            }

            var (restarted, restoredDriver) = await Start(true);
            await using (restarted)
            {
                var editor = restarted.Services.GetRequiredService<IDMEEditor>();
                editor.ConfigEditor.LoadDataConnectionsValues();
                await using var scope = restarted.Services.CreateAsyncScope();
                Assert.True(Assert.Single(scope.ServiceProvider.GetRequiredService<SetupDriverService>().Catalog(), x => x.Id == restoredDriver.GuidID).Loaded);
                var connector = Assert.Single(editor.ConfigEditor.DataSourcesClasses, x => x.className == "SQLServerDataSource");
                Assert.StartsWith(Path.GetFullPath(packageCache) + Path.DirectorySeparatorChar, Path.GetFullPath(connector.type.Assembly.Location), StringComparison.OrdinalIgnoreCase);
                var saved = Assert.Single(editor.ConfigEditor.DataConnections, x => x.ConnectionName == "wizard_database");
                Assert.Equal(restoredDriver.PackageName, saved.DriverName);
                Assert.Equal(restoredDriver.version, saved.DriverVersion);
                var source = editor.GetDataSource("wizard_database");
                Assert.NotNull(source); Assert.Equal(System.Data.ConnectionState.Open, source.Openconnection());
                var owner = new object(); var owned = Assert.IsAssignableFrom<IIsolationOwnedTransactionDataSource>(source);
                Assert.Equal(Errors.Ok, owned.BeginOwnedTransaction(owner, System.Data.IsolationLevel.Serializable).Flag);
                Assert.Equal(Errors.Ok, owned.RollbackOwnedTransaction(owner).Flag);
                source.Closeconnection();
            }
        }
        finally { await databaseContext.Database.EnsureDeletedAsync(); }
    }

    private static async Task RunHttpSetup(bool bundledDriver, bool downloadedDriver = false)
    {
        const string key = "this-is-a-long-setup-test-key-12345";
        var directory = Path.Combine(Path.GetTempPath(), "oilgas-identity-http-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var database = "BeepOilGas_Setup_" + Guid.NewGuid().ToString("N");
        var encryptionKey = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

        async Task<WebApplication> Start(bool populateDriver)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Configuration.AddJsonFile(Path.Combine(directory, "identity-repository-target.json"), optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = directory, ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = key,
                    ["Beep:CredentialProtection:CurrentKeyId"] = "test", ["Beep:CredentialProtection:Keys:test"] = encryptionKey });
            if (downloadedDriver)
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> {
                    ["SetupDrivers:Connectors:SQLServerDataSource:Package"] = "TheTechIdea.Beep.SqlServerDataSourceCore",
                    ["SetupDrivers:Connectors:SQLServerDataSource:Version"] = Environment.GetEnvironmentVariable("OILGAS_TEST_PACKAGED_VERSION"),
                    ["SetupDrivers:Providers:SQLServerDataSource:Package"] = "System.Data.SqlClient",
                    ["SetupDrivers:Providers:SQLServerDataSource:Version"] = "4.9.1",
                    ["SetupDrivers:PackageSources:0"] = Environment.GetEnvironmentVariable("OILGAS_TEST_ISOLATED_DRIVER_FEED"),
                    ["SetupDrivers:PackageSources:1"] = Environment.GetEnvironmentVariable("OILGAS_TEST_NUGET_FEED") });
            builder.Services.AddSingleton(Mock.Of<IFailureReporter>());
            builder.Services.AddOilGasRepository(builder.Configuration);
            builder.Services.AddScoped<IRepositoryConnectionSource, BeepRepositoryConnectionSource>();
            builder.Services.AddBeepRuntime(options => { options.DirectoryPath = directory; options.ConfigType = BeepConfigType.DataConnector;
                options.EnableAutoMapping = true; options.EnableAssemblyLoading = false; options.EnableConfigurationValidation = true;
                options.ConnectionSecretProtector = new ConnectionCredentialProtection(new AesGcmConnectionCredentialCipher(new BeepCredentialKeyProvider(builder.Configuration))); });
            builder.Services.AddScoped<SetupDriverService>(); builder.Services.AddScoped<SetupBootstrapAccess>();
            builder.Services.AddScoped<SetupRepositoryConnectionCreationService>(); builder.Services.AddScoped<SetupRepositoryConnectionSelectionService>();
            builder.Services.AddScoped<SetupRepositoryMigrationService>();
            builder.Services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("test", _ => { });
            builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
            builder.Services.AddControllers().AddApplicationPart(typeof(SetupBootstrapRepositoryController).Assembly);
            var app = builder.Build();
            var editor = app.Services.GetRequiredService<IDMEEditor>(); editor.ConfigEditor.LoadDataConnectionsValues();
            if (bundledDriver || downloadedDriver)
            {
                if (!editor.ConfigEditor.DataDriversClasses.Any(d => d.DatasourceType == DataSourceType.SqlServer && d.classHandler == "SQLServerDataSource"))
                    editor.ConfigEditor.DataDriversClasses.Add(TheTechIdea.Beep.Helpers.ConnectionHelper.CreateSqlServerConfig());
                if (bundledDriver || !populateDriver)
                {
                    await using var startup = app.Services.CreateAsyncScope();
                    await startup.ServiceProvider.GetRequiredService<SetupDriverService>().RestoreCachedAsync(default);
                }
            }
            else if (populateDriver)
            {
                editor.ConfigEditor.DataDriversClasses.Add(new ConnectionDriversConfig { GuidID = "test-driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false });
                editor.ConfigEditor.DataSourcesClasses.Add(new AssemblyClassDefinition { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() });
            }
            app.UseMiddleware<SetupGateMiddleware>();
            app.MapWhen(context => SetupBootstrapPaths.IsKeyProtected(context.Request.Path), bootstrap => { bootstrap.UseRouting(); bootstrap.UseEndpoints(endpoints => endpoints.MapControllers()); });
            app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
            app.MapGet("/health/repository", async (IRepositoryReadinessService ready) =>
            {
                var status = await ready.CheckAsync();
                return Results.Json(new RepositoryStatusResponse(status.ToString()), statusCode: status == RepositoryReadiness.Ready ? 200 : 503);
            }).AllowAnonymous();
            await app.StartAsync();
            return app;
        }

        static HttpClient Client(WebApplication app) => new() { BaseAddress = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        static HttpRequestMessage Request(HttpMethod method, string path) { var request = new HttpRequestMessage(method, path); request.Headers.Add("X-Setup-Key", key); return request; }

        try
        {
        await using (var first = await Start(true))
        using (var client = Client(first))
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/setup/bootstrap/repository/connections")).StatusCode);
            if (downloadedDriver)
            {
                using var catalogRequest = Request(HttpMethod.Get, "/api/setup/bootstrap/drivers");
                using var catalogResponse = await client.SendAsync(catalogRequest);
                Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);
                var catalog = await catalogResponse.Content.ReadFromJsonAsync<SetupDriverItem[]>();
                var item = Assert.Single(catalog!, x => x.DataSourceType == "SqlServer");
                Assert.False(item.ConnectorLoaded);
                using var install = Request(HttpMethod.Post, "/api/setup/bootstrap/drivers/" + Uri.EscapeDataString(item.Id) + "/install");
                install.Content = JsonContent.Create(item);
                using var installedResponse = await client.SendAsync(install);
                Assert.Equal(HttpStatusCode.OK, installedResponse.StatusCode);
                var installed = (await installedResponse.Content.ReadFromJsonAsync<SetupDriverItem>())!;
                Assert.True(installed.Loaded && installed.ConnectorLoaded && installed.ProviderLoaded);
                Assert.True(Directory.Exists(Path.Combine(Environment.GetEnvironmentVariable("NUGET_PACKAGES")!,
                    "thetechidea.beep.sqlserverdatasourcecore", installed.ConnectorVersion!.ToLowerInvariant())));
            }
            if (bundledDriver || downloadedDriver)
            {
                Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/setup/bootstrap/drivers")).StatusCode);
                using var catalogRequest = Request(HttpMethod.Get, "/api/setup/bootstrap/drivers");
                using var catalogResponse = await client.SendAsync(catalogRequest);
                Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);
                var catalog = await catalogResponse.Content.ReadFromJsonAsync<SetupDriverItem[]>();
                Assert.Contains(catalog!, item => item.DataSourceType == "SqlServer" && item.Loaded && (downloadedDriver || item.BundledConnector));
            }
            using var create = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/connections");
            using var driverScope = first.Services.CreateScope();
            var driverId = driverScope.ServiceProvider.GetRequiredService<SetupDriverService>().Catalog().First(x => x.DataSourceType == "SqlServer" && x.Loaded).Id;
            create.Content = JsonContent.Create(new SetupRepositoryConnectionCreate("identity_db", "SQLSERVER", $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true", driverId));
            using var created = await client.SendAsync(create); Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            using var select = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/connection");
            select.Content = JsonContent.Create(new SetupRepositoryConnectionSelection("identity_db"));
            using var selected = await client.SendAsync(select); Assert.Equal(HttpStatusCode.OK, selected.StatusCode);
        }

        await using (var restarted = await Start(false))
        using (var client = Client(restarted))
        {
            if (bundledDriver || downloadedDriver)
            {
                using var catalogRequest = Request(HttpMethod.Get, "/api/setup/bootstrap/drivers");
                using var catalogResponse = await client.SendAsync(catalogRequest);
                Assert.Equal(HttpStatusCode.OK, catalogResponse.StatusCode);
                var catalog = await catalogResponse.Content.ReadFromJsonAsync<SetupDriverItem[]>();
                Assert.Contains(catalog!, item => item.DataSourceType == "SqlServer" && item.Loaded && (downloadedDriver || item.BundledConnector));
                if (downloadedDriver)
                {
                    var editor = restarted.Services.GetRequiredService<IDMEEditor>();
                    var connector = Assert.Single(editor.ConfigEditor.DataSourcesClasses, x => x.className == "SQLServerDataSource");
                    Assert.StartsWith(Path.GetFullPath(Environment.GetEnvironmentVariable("NUGET_PACKAGES")!) + Path.DirectorySeparatorChar,
                        Path.GetFullPath(connector.type.Assembly.Location), StringComparison.OrdinalIgnoreCase);
                }
            }
            using var review = Request(HttpMethod.Get, "/api/setup/bootstrap/repository/migration");
            using var response = await client.SendAsync(review); Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var plan = (await response.Content.ReadFromJsonAsync<SetupRepositoryMigrationPlan>())!; Assert.True(plan.CreatesDatabase);
            using var apply = Request(HttpMethod.Post, "/api/setup/bootstrap/repository/migration");
            apply.Content = JsonContent.Create(new SetupRepositoryMigrationRequest(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations));
            using var applied = await client.SendAsync(apply); Assert.Equal(HttpStatusCode.OK, applied.StatusCode);
            Assert.Equal(RepositoryReadiness.BootstrapRequired, (await applied.Content.ReadFromJsonAsync<SetupRepositoryMigrationResult>())!.Readiness);
            using var health = await client.GetAsync("/health/repository");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, health.StatusCode);
            Assert.Equal("BootstrapRequired", (await health.Content.ReadFromJsonAsync<RepositoryStatusResponse>())!.Status);
            await using (var scope = restarted.Services.CreateAsyncScope())
            {
                var bootstrap = scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>();
                Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync("https://identity.example", "test-subject"));
            }
            using var ready = await client.GetAsync("/health/repository");
            Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
            Assert.Equal("Ready", (await ready.Content.ReadFromJsonAsync<RepositoryStatusResponse>())!.Status);
            using var revoked = Request(HttpMethod.Get, "/api/setup/bootstrap/repository/connections");
            using var refused = await client.SendAsync(revoked);
            Assert.Equal(HttpStatusCode.Unauthorized, refused.StatusCode);
            if (bundledDriver || downloadedDriver)
            {
                using var driverRequest = Request(HttpMethod.Get, "/api/setup/bootstrap/drivers");
                using var driverRefused = await client.SendAsync(driverRequest);
                Assert.Equal(HttpStatusCode.Unauthorized, driverRefused.StatusCode);
            }
        }
        }
        finally
        {
            var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
                .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
            await using var cleanup = new SqlServerRepositoryDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
#endif
