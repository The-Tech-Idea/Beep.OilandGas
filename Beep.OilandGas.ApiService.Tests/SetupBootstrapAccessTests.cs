using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Http.Json;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.Tools;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupBootstrapAccessTests
{
    public sealed class AuthenticationCounter { public int Calls; }
    public sealed class CountingAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, AuthenticationCounter counter)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() { counter.Calls++; return Task.FromResult(AuthenticateResult.NoResult()); }
    }
    private const string Key = "this-is-a-long-setup-test-key-12345";
    private static (SetupBootstrapAccess Access, Mock<IRepositoryReadinessService> Readiness) Create(bool enabled, string? configuredKey, RepositoryReadiness status)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupBootstrap:Enabled"] = enabled.ToString(), ["SetupBootstrap:Key"] = configuredKey }).Build();
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(status);
        return (new(config, readiness.Object), readiness);
    }
    [Theory]
    [InlineData(RepositoryReadiness.Unavailable)]
    [InlineData(RepositoryReadiness.MigrationRequired)]
    [InlineData(RepositoryReadiness.BootstrapRequired)]
    public async Task EnabledKeyAllowsOnlyIncompleteSetup(RepositoryReadiness status)
    {
        var f = Create(true, Key, status); await f.Access.RequireAsync(Key, default);
        f.Readiness.Verify(x => x.CheckAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    [Theory]
    [InlineData(RepositoryReadiness.Ready)]
    [InlineData(RepositoryReadiness.RecoveryRequired)]
    public async Task ReadyOrRecoveryNeverAcceptsBootstrapKey(RepositoryReadiness status)
    {
        var f = Create(true, Key, status);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Access.RequireAsync(Key, default));
    }
    [Fact]
    public async Task DisabledShortOrIncorrectKeyFailsBeforeReadinessLookup()
    {
        foreach (var f in new[] { Create(false, Key, RepositoryReadiness.Unavailable), Create(true, "short", RepositoryReadiness.Unavailable), Create(true, Key, RepositoryReadiness.Unavailable) })
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Access.RequireAsync("wrong-key-that-is-at-least-32-characters", default));
            f.Readiness.Verify(x => x.CheckAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(false, "/api/setup/bootstrap/drivers")]
    [InlineData(true, "/api/setup/bootstrap/drivers")]
    [InlineData(false, "/api/setup/bootstrap/repository/migration")]
    [InlineData(true, "/api/setup/bootstrap/repository/migration")]
    public async Task BootstrapDoesNotResolveRepositoryServices(bool authenticated, string path)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Path = path;
        http.User = new ClaimsPrincipal(authenticated ? new ClaimsIdentity([], "test") : new ClaimsIdentity());
        var called = false;
        var middleware = new CompanyRequestMiddleware(_ => { called = true; return Task.CompletedTask; }, _ => throw new InvalidOperationException());
        await middleware.InvokeAsync(http);
        Assert.True(called);
    }

    [Fact]
    public async Task UnavailableRepositoryAllowsOnlyKeyProtectedDriverCatalogThroughHttpPipeline()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        { ["SetupBootstrap:Enabled"] = "true", ["SetupBootstrap:Key"] = Key, ["SetupDrivers:Connectors:DriverImpl:Package"] = "Connector", ["SetupDrivers:Connectors:DriverImpl:Version"] = "1.0" });
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.Unavailable);
        var config = new Mock<IConfigEditor>();
        config.SetupGet(x => x.DataConnections).Returns(new List<ConnectionProperties>());
        config.SetupGet(x => x.DataDriversClasses).Returns(new List<ConnectionDriversConfig> { new() { GuidID = "driver", PackageName = "Provider", classHandler = "DriverImpl", ADOType = false } });
        var loaded = false; var downloads = 0;
        config.SetupGet(x => x.DataSourcesClasses).Returns(() => loaded ? new List<AssemblyClassDefinition> { new() { className = "DriverImpl", type = Mock.Of<IDataSource>().GetType() } } : []);
        var loader = new Mock<IAssemblyHandler>();
        loader.Setup(x => x.LoadNuggetFromNuGetAsync("Connector", "1.0", null, true, null, false)).ReturnsAsync(() =>
        { downloads++; loaded = true; return new List<System.Reflection.Assembly> { typeof(SetupDriverService).Assembly }; });
        var editor = new Mock<IDMEEditor>(); editor.SetupGet(x => x.ConfigEditor).Returns(config.Object); editor.SetupGet(x => x.assemblyHandler).Returns(loader.Object);
        builder.Services.AddSingleton(readiness.Object);
        var counter = new AuthenticationCounter(); builder.Services.AddSingleton(counter);
        builder.Services.AddAuthentication("counting").AddScheme<AuthenticationSchemeOptions, CountingAuthentication>("counting", _ => { });
        builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        builder.Services.AddSingleton<Func<ClaimsPrincipal, string?>>(_ => _ => null);
        builder.Services.AddSingleton(new SetupDriverService(editor.Object, builder.Configuration, NullLogger<SetupDriverService>.Instance));
        builder.Services.AddSingleton(editor.Object);
        builder.Services.AddScoped<SetupRepositoryConnectionSelectionService>();
        builder.Services.AddScoped<SetupBootstrapAccess>();
        builder.Services.AddControllers().AddApplicationPart(typeof(SetupBootstrapDriverController).Assembly);
        await using var app = builder.Build();
        app.UseMiddleware<SetupGateMiddleware>();
        app.MapWhen(context => SetupBootstrapPaths.IsKeyProtected(context.Request.Path), bootstrap =>
        {
            bootstrap.UseRouting();
            bootstrap.UseEndpoints(endpoints => endpoints.MapControllers());
        });
        app.UseAuthentication();
        app.UseMiddleware<CompanyRequestMiddleware>();
        app.UseAuthorization();
        app.MapControllers(); app.MapGet("/api/operational", () => Results.Ok()).RequireAuthorization();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient { BaseAddress = new Uri(address) };
        using var unauthorized = await client.GetAsync("/api/setup/bootstrap/drivers");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/setup/bootstrap/drivers"); request.Headers.Add("X-Setup-Key", Key);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "untrusted-token");
        using var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var drivers = await response.Content.ReadFromJsonAsync<List<SetupDriverItem>>();
        Assert.Equal("driver", Assert.Single(drivers!).Id);
        using var deniedInstall = await client.PostAsJsonAsync("/api/setup/bootstrap/drivers/driver/install", Assert.Single(drivers!));
        Assert.Equal(HttpStatusCode.Unauthorized, deniedInstall.StatusCode); Assert.Equal(0, downloads);
        using var install = new HttpRequestMessage(HttpMethod.Post, "/api/setup/bootstrap/drivers/driver/install"); install.Headers.Add("X-Setup-Key", Key);
        install.Content = JsonContent.Create(Assert.Single(drivers!));
        using var installed = await client.SendAsync(install);
        Assert.Equal(HttpStatusCode.OK, installed.StatusCode);
        Assert.True((await installed.Content.ReadFromJsonAsync<SetupDriverItem>())!.Loaded); Assert.Equal(1, downloads);
        using var repositoryWithoutKey = await client.GetAsync("/api/setup/bootstrap/repository/connections");
        Assert.Equal(HttpStatusCode.Unauthorized, repositoryWithoutKey.StatusCode);
        using var repository = new HttpRequestMessage(HttpMethod.Get, "/api/setup/bootstrap/repository/connections");
        repository.Headers.Add("X-Setup-Key", Key);
        repository.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "untrusted-token");
        using var repositoryResult = await client.SendAsync(repository);
        Assert.Equal(HttpStatusCode.OK, repositoryResult.StatusCode);
        Assert.Equal(0, counter.Calls);
        using var blocked = await client.GetAsync("/api/operational");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, blocked.StatusCode);
        readiness.Setup(x => x.CheckAsync(It.IsAny<CancellationToken>())).ReturnsAsync(RepositoryReadiness.Ready);
        using var ordinary = await client.GetAsync("/api/operational");
        Assert.Equal(HttpStatusCode.Unauthorized, ordinary.StatusCode);
        Assert.True(counter.Calls > 0);
    }

    [Theory]
    [InlineData("/api/setup/bootstrap/drivers", true)]
    [InlineData("/api/setup/bootstrap/repository/migration", true)]
    [InlineData("/api/setup/bootstrap/drivers-other", false)]
    [InlineData("/api/setup/repository", false)]
    public void OnlyKeyProtectedBootstrapRoutesBypassIdentityAuthentication(string path, bool expected) =>
        Assert.Equal(expected, SetupBootstrapPaths.IsKeyProtected(new PathString(path)));
}
