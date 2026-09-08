using System.Security.Claims;
using Beep.OilandGas.UserManagement.Security;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class IdentityPermissionHandlerTests
{
    [Theory]
    [InlineData("Read", true)]
    [InlineData("Write", true)]
    [InlineData("Delete", false)]
    public async Task EvaluatesEveryIdentityPermission(string permission, bool allowed)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "Read"), new Claim("permission", "Write")
        }, "Bearer"));
        var requirement = new PermissionRequirement(permission);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await new PermissionHandler().HandleAsync(context);

        Assert.Equal(allowed, context.HasSucceeded);
    }

    [Fact]
    public async Task UnauthenticatedPrincipalCannotUsePermissionClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("permission", "Write")]));
        var requirement = new PermissionRequirement("Write");
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await new PermissionHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
