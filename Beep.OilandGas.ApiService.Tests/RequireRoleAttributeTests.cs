using System.Security.Claims;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RequireRoleAttributeTests
{
    [Fact]
    public async Task OnAuthorizationAsync_WhenNameIdentifierDiffersFromIdentity_UsesNameIdentifier()
    {
        var access = new TestAccessControlService
        {
            Roles = new List<string> { "Admin" }
        };

        var context = CreateContext(CreateServices(access), new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "trusted-user"),
            new Claim(ClaimTypes.Name, "display-name")
        ], "TestAuth", ClaimTypes.Name, ClaimTypes.Role)));

        var attribute = new RequireRoleAttribute("Admin");
        await attribute.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal("trusted-user", access.LastUserId);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenRoleMissing_SetsForbid()
    {
        var access = new TestAccessControlService
        {
            Roles = new List<string> { "Viewer" }
        };

        var context = CreateContext(CreateServices(access), new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-1")
        ], "TestAuth", ClaimTypes.Name, ClaimTypes.Role)));

        var attribute = new RequireRoleAttribute("Admin");
        await attribute.OnAuthorizationAsync(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    private static AuthorizationFilterContext CreateContext(IServiceProvider services, ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
            User = principal
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static IServiceProvider CreateServices(TestAccessControlService accessControl)
    {
        return new DictionaryServiceProvider(new Dictionary<Type, object>
        {
            { typeof(IAccessControlService), accessControl }
        });
    }

    private sealed class DictionaryServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services;

        public DictionaryServiceProvider(Dictionary<Type, object> services)
        {
            _services = services;
        }

        public object? GetService(Type serviceType)
        {
            return _services.TryGetValue(serviceType, out var service) ? service : null;
        }
    }

    private sealed class TestAccessControlService : IAccessControlService
    {
        public List<string> Roles { get; set; } = new();
        public string? LastUserId { get; private set; }

        public Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null)
            => Task.FromResult(new AccessCheckResponse { HasAccess = true });

        public Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
            => Task.FromResult(new List<AssetAccess>());

        public Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            LastUserId = userId;
            return Task.FromResult(Roles);
        }

        public Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null)
            => Task.FromResult(true);

        public Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null)
            => Task.FromResult(true);

        public Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType)
            => Task.FromResult(true);

        public Task<List<string>> GetRolePermissionsAsync(string roleId, string? organizationId = null)
            => Task.FromResult(new List<string>());

        public Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string? organizationId = null)
            => Task.FromResult(true);

        public Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string? organizationId = null)
            => Task.FromResult(true);
    }
}
