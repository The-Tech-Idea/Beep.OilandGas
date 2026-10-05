using Beep.OilandGas.Models.Core.Refusals;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The caller's own account and the repository's setup are not asked which assets the caller may reach. They read no
/// asset data, and asking made a fresh repository impossible to set up: resolving asset access needs the PPDM_CORE binding,
/// binding it is a setup request, and every setup and account request failed until it was bound.
/// </summary>
/// <remarks>
/// <b>Blind spot:</b> drives the middleware alone with a service that fails as an unbound repository does; that it sits
/// in <c>Program.cs</c>'s pipeline where it did is compilation and the pipeline tests.
/// </remarks>
public sealed class AssetAccessMiddlewareTests
{
    [Theory]
    [InlineData("/api/auth/repository/me")]
    [InlineData("/api/auth/repository/me/deletion")]
    [InlineData("/api/setup/modules/PPDM_CORE/connection")]
    [InlineData("/api/setup/repository")]
    public async Task Account_and_setup_requests_are_not_asked_about_assets(string path)
    {
        var (context, access) = Request(path);
        var reached = false;

        await new AssetAccessMiddleware(_ => { reached = true; return Task.CompletedTask; }).InvokeAsync(context);

        Assert.True(reached);
        access.Verify(service => service.GetUserAccessibleAssetsAsync(
            It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>Everything else still is: the same unbound repository fails an asset request as it did.</summary>
    [Fact]
    public async Task An_asset_request_is_still_asked()
    {
        var (context, _) = Request("/api/production/fields");

        // OILGAS-CATCH-01: the unbound module is the resolver's refusal (409), passed through untouched.
        await Refusals.RefusedAsync(RefusalKind.Conflict, () =>
            new AssetAccessMiddleware(_ => Task.CompletedTask).InvokeAsync(context));
    }

    private static (HttpContext Context, Mock<IAccessControlService> Access) Request(string path)
    {
        var access = new Mock<IAccessControlService>();
        access.Setup(service => service.GetUserAccessibleAssetsAsync(
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ThrowsAsync(RefusalException.Conflict("Configure a database binding for module PPDM_CORE before accessing its data."));

        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddSingleton(access.Object).BuildServiceProvider(),
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "person-1")], "test")),
        };
        context.Request.Path = path;
        return (context, access);
    }
}
