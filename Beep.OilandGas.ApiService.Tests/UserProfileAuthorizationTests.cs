using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.AccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class UserProfileAuthorizationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task UnauthenticatedUnregisteredAndOtherUsersCannotAccessProfiles(bool authenticated, bool localId)
    {
        var service = new Mock<IUserProfileService>(MockBehavior.Strict);
        var claims = localId ? new[] { new Claim("party_id", "other") } : Array.Empty<Claim>();
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticated ? "test" : null))
            } }
        };
        Assert.IsType<ForbidResult>((await controller.GetUserProfile("target")).Result);
        Assert.IsType<ForbidResult>((await controller.GetUserRoles("target")).Result);
        Assert.IsType<ForbidResult>((await controller.GetUserDefaultLayout("target")).Result);
        Assert.IsType<ForbidResult>((await controller.UpdateUserPreferences("target", new())).Result);
        Assert.IsType<ForbidResult>((await controller.UpdateUserPrimaryRole("target", new())).Result);
        Assert.IsType<ForbidResult>((await controller.UpdateUserPreferredLayout("target", new())).Result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoginIsRecordedOnlyForTheSignedInAccount(bool administrator)
    {
        var service = new Mock<IUserProfileService>(MockBehavior.Strict);
        service.Setup(x => x.RecordUserLoginAsync("owner")).Returns(Task.CompletedTask);
        var claims = new List<Claim> { new("party_id", "owner") };
        if (administrator) claims.Add(new(ClaimTypes.Role, "Administrator"));
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            } }
        };
        Assert.IsType<OkObjectResult>(await controller.RecordUserLogin());
        service.VerifyAll();
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginWithoutAnAccountIsRefusedBeforeStorage()
    {
        var service = new Mock<IUserProfileService>(MockBehavior.Strict);
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "owner") }, "test"))
            } }
        };
        await Refusals.ForbiddenAsync(() => controller.RecordUserLogin());
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task OwnerCannotUseProfileEndpointToChangePrimaryRole()
    {
        var service = new Mock<IUserProfileService>(MockBehavior.Strict);
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("party_id", "owner") }, "test"))
            } }
        };
        Assert.IsType<ForbidResult>((await controller.UpdateUserPrimaryRole("owner", new())).Result);
        service.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OwnerAndAdministratorCanUpdatePreferences(bool administrator)
    {
        var service = new Mock<IUserProfileService>(MockBehavior.Strict);
        service.Setup(x => x.UpdateUserPreferencesAsync("target", "{}")).ReturnsAsync(true);
        var claims = new List<Claim> { new("party_id", administrator ? "admin" : "target") };
        if (administrator) claims.Add(new(ClaimTypes.Role, "Administrator"));
        var controller = new UserProfileController(service.Object)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            } }
        };
        var response = await controller.UpdateUserPreferences("target", new() { PreferencesJson = "{}" });
        Assert.Equal(true, Assert.IsType<OkObjectResult>(response.Result).Value);
        service.VerifyAll();
    }
}
