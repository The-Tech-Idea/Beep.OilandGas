using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryClaimsTransformationTests
{
    [Fact]
    public async Task ExternalAdminAndPermissionClaimsAreReplacedByLocalAccess()
    {
        var service = new Mock<IRepositoryAccessService>();
        service.Setup(x => x.GetAccessAsync("https://issuer", "subject", default))
            .ReturnsAsync(new RepositoryUserAccess("local-id", true, ["Viewer"], ["Read"]));
        var transform = new RepositoryClaimsTransformation(service.Object, NullLogger<RepositoryClaimsTransformation>.Instance);
        var principal = ExternalPrincipal();
        var result = await transform.TransformAsync(principal);
        Assert.False(result.IsInRole("Administrator"));
        Assert.True(result.IsInRole("Viewer"));
        Assert.False(result.HasClaim("permission", "Admin.ManageUsers"));
        Assert.True(result.HasClaim("permission", "Read"));
        Assert.Null(result.FindFirst("permissions"));
        Assert.Null(result.FindFirst("elevated_permissions"));
        Assert.Equal("local-id", result.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.True(principal.IsInRole("Administrator"));
        Assert.Same(result, await transform.TransformAsync(result));
        service.Verify(x => x.GetAccessAsync("https://issuer", "subject", default), Times.Once);
    }

    [Fact]
    public async Task UnknownUserCanRegisterButHasNoExternalRoles()
    {
        var service = new Mock<IRepositoryAccessService>();
        service.Setup(x => x.GetAccessAsync("https://issuer", "subject", default))
            .ReturnsAsync((RepositoryUserAccess?)null);
        var transform = new RepositoryClaimsTransformation(service.Object, NullLogger<RepositoryClaimsTransformation>.Instance);
        var result = await transform.TransformAsync(ExternalPrincipal());
        Assert.True(result.Identity!.IsAuthenticated);
        Assert.False(result.IsInRole("Administrator"));
        Assert.Null(result.FindFirst(ClaimTypes.NameIdentifier));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task OutageOrDisabledAccountFailsClosed(bool outage)
    {
        var service = new Mock<IRepositoryAccessService>();
        var setup = service.Setup(x => x.GetAccessAsync("https://issuer", "subject", default));
        if (outage) setup.ThrowsAsync(new InvalidOperationException("unavailable"));
        else setup.ReturnsAsync(new RepositoryUserAccess("local-id", false, ["Administrator"], []));
        var transform = new RepositoryClaimsTransformation(service.Object, NullLogger<RepositoryClaimsTransformation>.Instance);
        var result = await transform.TransformAsync(ExternalPrincipal());
        Assert.False(result.Identity!.IsAuthenticated);
        Assert.False(result.IsInRole("Administrator"));
    }

    [Fact]
    public async Task TokenMarkerCannotBypassRoleResolution()
    {
        var service = new Mock<IRepositoryAccessService>();
        var principal = ExternalPrincipal();
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim("oilgas:roles-resolved", "true"));
        var transform = new RepositoryClaimsTransformation(service.Object, NullLogger<RepositoryClaimsTransformation>.Instance);
        var result = await transform.TransformAsync(principal);
        Assert.False(result.IsInRole("Administrator"));
        service.Verify(x => x.GetAccessAsync("https://issuer", "subject", default), Times.Once);
    }

    private static ClaimsPrincipal ExternalPrincipal() => new(new ClaimsIdentity(new[]
    {
        new Claim("iss", "https://issuer"), new Claim("sub", "subject"),
        new Claim(ClaimTypes.Role, "Administrator"), new Claim("role", "Administrator"),
        new Claim("permission", "Admin.ManageUsers"),
        new Claim("permissions", "Admin.ManageUsers,Admin.AssignRoles"),
        new Claim("elevated_permissions", "Admin.AssignRoles")
    }, "Bearer"));
}
