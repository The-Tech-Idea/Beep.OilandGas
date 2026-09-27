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
        // No account on the request: refused by the acting-user accessor before the (null) storage is reached.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.AssignRole("user", new("role", null)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.RevokeRole("assignment"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GrantPermission("role", new("permission")));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.RevokePermission("grant"));
    }
}
