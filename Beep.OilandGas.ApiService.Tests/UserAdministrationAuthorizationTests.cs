using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class UserAdministrationAuthorizationTests
{
    [Theory]
    [InlineData("other", null, null)]
    [InlineData("owner", null, false)]
    [InlineData("other", "Admin", null)]
    [InlineData(null, "Administrator", null)]
    public async Task UnauthorizedUserUpdatesAreRejectedBeforeStorage(string? actor, string? role, bool? active)
    {
        var claims = new List<Claim>();
        if (actor is not null) claims.Add(new(ClaimTypes.NameIdentifier, actor));
        if (role is not null) claims.Add(new(ClaimTypes.Role, role));
        var controller = new UserManagementController(null!)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity(claims, "test")) }
            }
        };
        Assert.IsType<ForbidResult>(await controller.UpdateUser("owner", new("Name", active, "stamp")));
    }
}
