using Beep.OilandGas.ApiService.Controllers.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class RoleAdministrationAuthorizationTests
{
    [Fact]
    public async Task MutationsRequireLocalActorBeforeCallingStorage()
    {
        var controller = new RoleAssignmentController(null!, NullLogger<RoleAssignmentController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        Assert.IsType<ForbidResult>(await controller.AssignRole("user", new("role", null)));
        Assert.IsType<ForbidResult>(await controller.RevokeRole("assignment"));
        Assert.IsType<ForbidResult>(await controller.GrantPermission("role", new("permission")));
        Assert.IsType<ForbidResult>(await controller.RevokePermission("grant"));
    }
}
