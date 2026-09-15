using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class AssetHierarchyAuthorizationTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("member", false)]
    [InlineData(null, true)]
    public async Task UnfilteredHierarchyAndConfigurationRequireLocalAdministrator(string? userId, bool administrator)
    {
        var service = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        var controller = Create(service.Object, userId, administrator);
        Assert.IsType<ForbidResult>((await controller.GetAssetHierarchy("organization")).Result);
        Assert.IsType<ForbidResult>((await controller.GetAssetChildren("field", "FIELD")).Result);
        Assert.IsType<ForbidResult>((await controller.GetAssetPath("well", "WELL")).Result);
        Assert.IsType<ForbidResult>((await controller.GetHierarchyConfig("organization")).Result);
        Assert.IsType<ForbidResult>((await controller.UpdateHierarchyConfig("organization", [])).Result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("other")]
    public async Task UserSpecificOperationsCannotChooseAnotherIdentity(string? userId)
    {
        var service = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        var controller = Create(service.Object, userId, false);
        Assert.IsType<ForbidResult>((await controller.GetAssetHierarchyForUser("owner")).Result);
        Assert.IsType<ForbidResult>((await controller.ValidateAccess(new ValidateAccessRequest { UserId = "owner", AssetPath = [] })).Result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("owner", false)]
    [InlineData("admin", true)]
    public async Task OwnerAndAdministratorCanReadUserFilteredHierarchy(string userId, bool administrator)
    {
        var service = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        service.Setup(x => x.GetAssetHierarchyForUserAsync("owner", null, null, null))
            .ReturnsAsync(new AssetHierarchyNode { AssetId = "well", AssetType = "WELL", UserHasAccess = true });
        service.Setup(x => x.ValidateAccessAsync("owner", It.IsAny<List<AssetHierarchyNode>>())).ReturnsAsync(true);
        var controller = Create(service.Object, userId, administrator);
        Assert.IsType<OkObjectResult>((await controller.GetAssetHierarchyForUser("owner")).Result);
        Assert.IsType<OkObjectResult>((await controller.ValidateAccess(new ValidateAccessRequest { UserId = "owner", AssetPath = [] })).Result);
        service.VerifyAll();
    }

    [Fact]
    public async Task AdministratorCanUpdateHierarchyConfiguration()
    {
        var service = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        service.Setup(x => x.UpdateHierarchyConfigAsync("organization", It.IsAny<List<HierarchyConfig>>())).ReturnsAsync(true);
        Assert.IsType<OkObjectResult>((await Create(service.Object, "admin", true).UpdateHierarchyConfig("organization", [])).Result);
        service.VerifyAll();
    }

    private static AssetHierarchyController Create(IAssetHierarchyService service, string? userId, bool administrator)
    {
        var claims = new List<Claim>();
        if (userId is not null) claims.Add(new(ClaimTypes.NameIdentifier, userId));
        if (administrator) claims.Add(new(ClaimTypes.Role, "Administrator"));
        return new(service) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, userId is not null || administrator ? "test" : null))
        } } };
    }
}
