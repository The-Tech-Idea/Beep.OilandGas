using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositorySetupGateTests
{
    [Theory]
    [InlineData("/api/identity/users")]
    [InlineData("/api/setup/modules")]
    [InlineData("/api/ppdm39/setup")]
    [InlineData("/api/connections")]
    public async Task ReadyRepositoryDoesNotRequireHardcodedModuleConnection(string path)
    {
        var next = false;
        var middleware = new SetupGateMiddleware(_ => { next = true; return Task.CompletedTask; });
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(default)).ReturnsAsync(RepositoryReadiness.Ready);
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        await middleware.InvokeAsync(context, readiness.Object);
        Assert.True(next);
    }

    [Theory]
    [InlineData(RepositoryReadiness.Unavailable)]
    [InlineData(RepositoryReadiness.MigrationRequired)]
    [InlineData(RepositoryReadiness.BootstrapRequired)]
    public async Task IncompleteInstallationBlocksBusinessRoutes(RepositoryReadiness status)
    {
        var next = false;
        var middleware = new SetupGateMiddleware(_ => { next = true; return Task.CompletedTask; });
        var readiness = new Mock<IRepositoryReadinessService>();
        readiness.Setup(x => x.CheckAsync(default)).ReturnsAsync(status);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/identity/users";
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context, readiness.Object);
        Assert.False(next);
        Assert.Equal(503, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/health/repository")]
    [InlineData("/api/setup/repository/register")]
    [InlineData("/api/auth/repository/me")]
    public async Task BootstrapAndAuthRemainReachableWithoutGateLookup(string path)
    {
        var next = false;
        var middleware = new SetupGateMiddleware(_ => { next = true; return Task.CompletedTask; });
        var readiness = new Mock<IRepositoryReadinessService>(MockBehavior.Strict);
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        await middleware.InvokeAsync(context, readiness.Object);
        Assert.True(next);
    }
}
