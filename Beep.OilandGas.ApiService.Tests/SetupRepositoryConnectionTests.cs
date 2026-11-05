#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using Xunit;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using TheTechIdea.Data.OilGas;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Logger;
using TheTechIdea.Beep.JsonLoaderService;
using TheTechIdea.Beep.Tools;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupRepositoryConnectionTests
{
    [Theory]
    [InlineData("Password")]
    [InlineData("Pwd")]
    [InlineData("Passfile")]
    [InlineData("Access Token")]
    [InlineData("SSL Password")]
    public async Task BootstrapNeverPersistsSecretBearingOptions(string key)
    {
        var config = new ConfigurationBuilder().Build();
        var editor = new Mock<IDMEEditor>();
        var service = new SetupRepositoryConnectionCreationService(config, editor.Object,
            new SetupDriverService(editor.Object, config, NullLogger<SetupDriverService>.Instance));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new("identity_db", "SQLSERVER", $"Server=localhost;Database=IdentityDb;{key}=secret", "driver"), () => Task.CompletedTask, default));
        editor.VerifyGet(x => x.ConfigEditor, Times.Never);
    }

    [Fact]
    public async Task RealBeepConfigRefusesSecretsAndRestoresCredentialFreeConnections()
    {
        var folder = Path.Combine(Path.GetTempPath(), "oilgas-beep-config-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var secret = "oilgas-test-secret-" + Guid.NewGuid().ToString("N");
        var connectionString = "Server=localhost;Database=IdentityDb;Integrated Security=true";
        var targetPath = Path.Combine(folder, "identity-repository-target.json");
        var settings = new ConfigurationBuilder().AddJsonFile(targetPath, optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = folder }).Build();
        string configPath;
        using (var config = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), folder, "test", BeepConfigType.DataConnector))
        {
            configPath = config.ConfigPath;
            config.DataDriversClasses.Add(new ConnectionDriversConfig { GuidID = "driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false });
            config.DataSourcesClasses.Add(new AssemblyClassDefinition { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() });
            var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config);
            var drivers = new SetupDriverService(editor.Object, settings, NullLogger<SetupDriverService>.Instance);
            var service = new SetupRepositoryConnectionCreationService(settings, editor.Object, drivers);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(new("identity_db", "SQLSERVER", "Server=localhost;Database=IdentityDb;Password=" + secret, "driver"), () => Task.CompletedTask, default));
            Assert.Empty(config.DataConnections.Where(x => x.ConnectionName == "identity_db"));
            Assert.Equal("identity_db", (await service.CreateAsync(new("identity_db", "SQLSERVER", connectionString, "driver"), () => Task.CompletedTask, default)).ConnectionName);
            Assert.Equal("identity_db", (await new SetupRepositoryConnectionSelectionService(settings, editor.Object).SelectAsync("identity_db", () => Task.CompletedTask, default)).ConnectionName);
        }
        var disk = await File.ReadAllTextAsync(Path.Combine(configPath, "DataConnections.json"));
        Assert.DoesNotContain(secret, disk);
        using var reopened = new ConfigEditor(Mock.Of<IDMLogger>(), new ErrorsInfo(), new JsonLoader(), folder, "test", BeepConfigType.DataConnector);
        var saved = Assert.Single(reopened.DataConnections.Where(x => x.ConnectionName == "identity_db"));
        Assert.Equal(connectionString, saved.ConnectionString);
        var reopenedEditor = new Mock<IDMEEditor>(); reopenedEditor.SetupGet(x => x.ConfigEditor).Returns(reopened);
        using var services = new ServiceCollection().AddSingleton(reopenedEditor.Object).BuildServiceProvider();
        var reopenedSettings = new ConfigurationBuilder().AddJsonFile(targetPath).Build();
        Assert.Equal(connectionString, new BeepRepositoryConnectionSource(reopenedSettings, services).Resolve()!.ConnectionString);
    }

    [Fact]
    public async Task CreateRequiresLoadedDriverAndNeverOverwritesAnotherConnection()
    {
        var values = new List<ConnectionProperties>(); var loaded = false;
        var driver = new ConnectionDriversConfig { GuidID = "driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false };
        var beep = new Mock<IConfigEditor>(); beep.SetupGet(x => x.DataConnections).Returns(values);
        beep.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { driver });
        beep.SetupGet(x => x.DataSourcesClasses).Returns(() => loaded ? new List<AssemblyClassDefinition> { new() { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() } } : []);
        beep.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns((ConnectionProperties value) => { values.Add(value); return true; });
        var saves = 0;
        beep.Setup(x => x.SaveDataconnectionsValues()).Callback(() => { if (++saves == 1) throw new IOException("save failed"); });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(beep.Object);
        var config = new ConfigurationBuilder().Build();
        var drivers = new SetupDriverService(editor.Object, config, NullLogger<SetupDriverService>.Instance);
        var creation = new SetupRepositoryConnectionCreationService(config, editor.Object, drivers);
        var request = new SetupRepositoryConnectionCreate("identity_db", "SQLSERVER", "Server=localhost;Database=IdentityDb;Integrated Security=true", "driver");
        Assert.DoesNotContain("not-a-real-secret", (request with { ConnectionString = "Password=not-a-real-secret" }).ToString());
        await Assert.ThrowsAsync<InvalidOperationException>(() => creation.CreateAsync(request, () => Task.CompletedTask, default));
        Assert.Empty(values);
        loaded = true;
        await Assert.ThrowsAsync<IOException>(() => creation.CreateAsync(request, () => Task.CompletedTask, default));
        Assert.Single(values);
        Assert.Equal("identity_db", (await creation.CreateAsync(request, () => Task.CompletedTask, default)).ConnectionName);
        Assert.Equal(request.ConnectionString, Assert.Single(values).ConnectionString);
        Assert.Equal("identity_db", (await creation.CreateAsync(request, () => Task.CompletedTask, default)).ConnectionName);
        await Assert.ThrowsAsync<InvalidOperationException>(() => creation.CreateAsync(request with { ConnectionString = "Server=localhost;Database=OtherDb;Integrated Security=true" }, () => Task.CompletedTask, default));
        Assert.Single(values);
        beep.Verify(x => x.SaveDataconnectionsValues(), Times.Exactly(3));
    }

    [Fact]
    public async Task CreateUsesTheSelectedLoadedDriverAndRejectsAnotherDriverOnRetry()
    {
        var connections = new List<ConnectionProperties>();
        var beep = new Mock<IConfigEditor>();
        beep.SetupGet(x => x.DataConnections).Returns(connections);
        beep.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig>
        {
            new() { GuidID = "first", PackageName = "first-provider", version = "1", classHandler = "First", DatasourceType = DataSourceType.SqlServer, ADOType = false },
            new() { GuidID = "selected", PackageName = "selected-provider", version = "2", classHandler = "Second", DatasourceType = DataSourceType.SqlServer, ADOType = false },
            new() { GuidID = "wrong-type", PackageName = "postgres", classHandler = "Other", DatasourceType = DataSourceType.Postgre, ADOType = false },
            new() { GuidID = "unloaded", PackageName = "missing", classHandler = "Missing", DatasourceType = DataSourceType.SqlServer, ADOType = false }
        });
        beep.SetupGet(x => x.DataSourcesClasses).Returns(new List<AssemblyClassDefinition>
        {
            new() { className = "First", type = Mock.Of<IDataSource>().GetType() },
            new() { className = "Second", type = Mock.Of<IDataSource>().GetType() },
            new() { className = "Other", type = Mock.Of<IDataSource>().GetType() }
        });
        beep.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns((ConnectionProperties value) => { connections.Add(value); return true; });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(beep.Object);
        var config = new ConfigurationBuilder().Build();
        var service = new SetupRepositoryConnectionCreationService(config, editor.Object, new SetupDriverService(editor.Object, config, NullLogger<SetupDriverService>.Instance));
        var request = new SetupRepositoryConnectionCreate("identity_db", "SQLSERVER", "Server=localhost;Database=IdentityDb;Integrated Security=true", "selected");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request with { DriverId = "wrong-type" }, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request with { DriverId = "unloaded" }, () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request with { DriverId = "unknown" }, () => Task.CompletedTask, default));
        Assert.Empty(connections);
        await service.CreateAsync(request, () => Task.CompletedTask, default);
        Assert.Equal("selected-provider", Assert.Single(connections).DriverName);
        Assert.Equal("2", connections[0].DriverVersion);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request with { DriverId = "first" }, () => Task.CompletedTask, default));
        Assert.Single(connections);
    }

    [Fact]
    public async Task BootstrapHttpSelectsARegisteredConnectionOnlyWithTheKey()
    {
        const string key = "this-is-a-long-setup-test-key-12345";
        var folder = Path.Combine(Path.GetTempPath(), "oilgas-identity-selection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddJsonFile(Path.Combine(folder, "identity-repository-target.json"), optional: true)
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = folder, ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = key });
        var readiness = new Mock<IRepositoryReadinessService>(); readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.Unavailable);
        var connection = new ConnectionProperties { ConnectionName = "identity_db", DatabaseType = DataSourceType.SqlServer, ConnectionString = "Server=localhost;Database=IdentityDb;Integrated Security=true" };
        var beep = new Mock<IConfigEditor>(); beep.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { connection });
        beep.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { new() { GuidID = "driver", PackageName = "provider", classHandler = "SqlImpl", DatasourceType = DataSourceType.SqlServer, ADOType = false } });
        beep.SetupGet(x => x.DataSourcesClasses).Returns(new List<AssemblyClassDefinition> { new() { className = "SqlImpl", type = Mock.Of<IDataSource>().GetType() } });
        beep.Setup(x => x.AddDataConnection(It.IsAny<ConnectionProperties>())).Returns((ConnectionProperties value) => { beep.Object.DataConnections.Add(value); return true; });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(beep.Object);
        builder.Services.AddSingleton(editor.Object); builder.Services.AddSingleton(readiness.Object);
        builder.Services.AddScoped<SetupBootstrapAccess>(); builder.Services.AddScoped<SetupRepositoryConnectionSelectionService>();
        builder.Services.AddSingleton(new SetupDriverService(editor.Object, builder.Configuration, NullLogger<SetupDriverService>.Instance));
        builder.Services.AddScoped<SetupRepositoryConnectionCreationService>();
        builder.Services.AddSingleton<Func<ClaimsPrincipal, string?>>(_ => _ => null);
        builder.Services.AddControllers().AddApplicationPart(typeof(SetupBootstrapRepositoryController).Assembly);
        await using var app = builder.Build();
        app.UseMiddleware<SetupGateMiddleware>(); app.UseMiddleware<CompanyRequestMiddleware>(); app.MapControllers(); await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/setup/bootstrap/repository/connections")).StatusCode);
        var proposed = new SetupRepositoryConnectionCreate("other_db", "SQLSERVER", "Server=localhost;Database=OtherDb;Integrated Security=true", "driver");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/setup/bootstrap/repository/connections", proposed)).StatusCode);
        using var create = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connections"); create.Headers.Add("X-Setup-Key", key);
        create.Content = JsonContent.Create(proposed);
        using var created = await client.SendAsync(create);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.DoesNotContain("not-a-real-secret", await created.Content.ReadAsStringAsync());
        using var list = new HttpRequestMessage(HttpMethod.Get, "/api/setup/bootstrap/repository/connections"); list.Headers.Add("X-Setup-Key", key);
        using var response = await client.SendAsync(list);
        Assert.Contains((await response.Content.ReadFromJsonAsync<List<SetupRepositoryConnectionOption>>())!, item => item.ConnectionName == "identity_db");
        using var select = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/repository/connection"); select.Headers.Add("X-Setup-Key", key);
        select.Content = JsonContent.Create(new SetupRepositoryConnectionSelection("identity_db"));
        using var saved = await client.SendAsync(select);
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal("identity_db", (await saved.Content.ReadFromJsonAsync<SetupRepositoryConnectionSelection>())!.ConnectionName);
        Assert.True(File.Exists(Path.Combine(folder, "identity-repository-target.json")));
    }

    [Fact]
    public async Task SelectionPersistsNoCredentialsAndRejectsChangedPhysicalTarget()
    {
        var folder = Path.Combine(Path.GetTempPath(), "oilgas-identity-selection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, "identity-repository-target.json");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = folder }).AddJsonFile(file, optional: true).Build();
        var connection = new ConnectionProperties { ConnectionName = "identity_db", DatabaseType = DataSourceType.SqlServer,
            ConnectionString = "Server=localhost;Database=IdentityDb;User ID=user;Password=not-a-real-secret" };
        var beep = new Mock<IConfigEditor>(); beep.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { connection });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(beep.Object);
        using var services = new ServiceCollection().AddSingleton(editor.Object).BuildServiceProvider();
        var selection = new SetupRepositoryConnectionSelectionService(config, editor.Object);
        Assert.Equal("identity_db", Assert.Single(selection.Connections()).ConnectionName);
        await selection.SelectAsync("identity_db", () => Task.CompletedTask, default);
        var saved = await File.ReadAllTextAsync(file);
        Assert.DoesNotContain("not-a-real-secret", saved);
        Assert.DoesNotContain("User ID", saved);
        var reopenedConfig = new ConfigurationBuilder().AddJsonFile(file).Build();
        var reopened = new BeepRepositoryConnectionSource(reopenedConfig, services);
        Assert.Equal(connection.ConnectionString, reopened.Resolve()!.ConnectionString);
        connection.ConnectionString = "Server=localhost;Database=OtherDb;User ID=user;Password=not-a-real-secret";
        Assert.Null(reopened.Resolve());
        await Assert.ThrowsAsync<InvalidOperationException>(() => selection.SelectAsync("identity_db", () => Task.CompletedTask, default));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public async Task SelectionRefusesLostAuthorityAndExistingServerConfiguration(int revokeAt)
    {
        var folder = Path.Combine(Path.GetTempPath(), "oilgas-identity-selection-" + Guid.NewGuid().ToString("N"));
        var connection = new ConnectionProperties { ConnectionName = "identity_db", DatabaseType = DataSourceType.SqlServer,
            ConnectionString = "Server=localhost;Database=IdentityDb;Integrated Security=true" };
        var beep = new Mock<IConfigEditor>(); beep.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties> { connection });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(beep.Object);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Beep:ConfigPath"] = folder }).Build();
        var selection = new SetupRepositoryConnectionSelectionService(config, editor.Object);
        var calls = 0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => selection.SelectAsync("identity_db", () => ++calls == revokeAt ? throw new UnauthorizedAccessException() : Task.CompletedTask, default));
        Assert.False(File.Exists(Path.Combine(folder, "identity-repository-target.json")));
        config["Repository:Provider"] = "SqlServer";
        await Assert.ThrowsAsync<InvalidOperationException>(() => selection.SelectAsync("identity_db", () => Task.CompletedTask, default));
        Assert.False(File.Exists(Path.Combine(folder, "identity-repository-target.json")));
    }
}
#endif
