using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetAccessControllerAuthorizationTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public async Task MutationEndpointsRejectCallersWithoutLocalAdministratorAccess(bool authenticated, bool localId, bool adminRole)
    {
        var service = new Mock<IAccessControlService>(MockBehavior.Strict);
        var controller = Create(service.Object, authenticated, localId ? "member" : null, adminRole);
        Assert.IsType<ForbidResult>((await controller.GrantAssetAccess(new() { UserId = "member" })).Result);
        Assert.IsType<ForbidResult>((await controller.RevokeAssetAccess(new() { UserId = "member" })).Result);
        Assert.IsType<ForbidResult>((await controller.AssignPermissionToRole("role", "permission")).Result);
        Assert.IsType<ForbidResult>((await controller.RemovePermissionFromRole("role", "permission")).Result);
        Assert.IsType<ForbidResult>((await controller.GetRolePermissions("role")).Result);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CrossUserReadsAreRejectedBeforeServiceAccess()
    {
        var service = new Mock<IAccessControlService>(MockBehavior.Strict);
        var controller = Create(service.Object, true, "member", false);
        Assert.IsType<ForbidResult>((await controller.CheckAssetAccess(new() { UserId = "other" })).Result);
        Assert.IsType<ForbidResult>((await controller.GetUserAccessibleAssets("other")).Result);
        Assert.IsType<ForbidResult>((await controller.GetUserRoles("other")).Result);
        Assert.IsType<ForbidResult>((await controller.HasPermission("other", "read")).Result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OwnerAndAdministratorCanReadUserRoles(bool administrator)
    {
        var service = new Mock<IAccessControlService>(MockBehavior.Strict);
        service.Setup(x => x.GetUserRolesAsync("member", null)).ReturnsAsync(new List<string> { "Reader" });
        var controller = Create(service.Object, true, administrator ? "admin" : "member", administrator);
        Assert.Equal(new[] { "Reader" }, Assert.IsType<List<string>>(Assert.IsType<OkObjectResult>(
            (await controller.GetUserRoles("member")).Result).Value));
        service.VerifyAll();
    }

    [Fact]
    public async Task LocalAdministratorCanGrantAssetAccess()
    {
        var service = new Mock<IAccessControlService>(MockBehavior.Strict);
        service.Setup(x => x.GrantAssetAccessAsync("member", "field", "FIELD", "READ", true, null)).ReturnsAsync(true);
        var controller = Create(service.Object, true, "admin", true);
        var response = await controller.GrantAssetAccess(new()
        {
            UserId = "member", AssetId = "field", AssetType = "FIELD", AccessLevel = "READ", Inherit = true
        });
        Assert.Equal(true, Assert.IsType<OkObjectResult>(response.Result).Value);
        service.VerifyAll();
    }

    private static AccessControlController Create(IAccessControlService service, bool authenticated, string? userId, bool admin)
    {
        var claims = new List<Claim>();
        if (userId is not null) claims.Add(new("party_id", userId));
        if (admin) claims.Add(new(ClaimTypes.Role, "Administrator"));
        return new(service) { ControllerContext = new() { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "test" : null))
        } } };
    }
}
