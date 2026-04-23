using System.Security.Claims;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RequireCurrentFieldAccessAttributeTests
{
    [Fact]
    public async Task OnAuthorizationAsync_WhenNoIdentity_SetsUnauthorized()
    {
        var attribute = new RequireCurrentFieldAccessAttribute();
        var context = CreateContext(null, new ClaimsPrincipal(new ClaimsIdentity()));

        await attribute.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenServicesMissing_Sets500()
    {
        var attribute = new RequireCurrentFieldAccessAttribute();
        var principal = CreatePrincipal(userId: "user-1");
        var context = CreateContext(null, principal);

        await attribute.OnAuthorizationAsync(context);

        var status = Assert.IsType<StatusCodeResult>(context.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenNoActiveField_SetsBadRequest()
    {
        var access = new TestAccessControlService();
        var orchestrator = new TestFieldOrchestrator { CurrentFieldId = null };
        var services = CreateServices(access, orchestrator);

        var attribute = new RequireCurrentFieldAccessAttribute();
        var context = CreateContext(services, CreatePrincipal(userId: "user-1"));

        await attribute.OnAuthorizationAsync(context);

        Assert.IsType<BadRequestObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenAccessDenied_SetsForbid()
    {
        var access = new TestAccessControlService
        {
            NextResponse = new AccessCheckResponse { HasAccess = false, Reason = "Denied" }
        };
        var observability = new TestAuthorizationObservabilityService();
        var orchestrator = new TestFieldOrchestrator { CurrentFieldId = "FIELD-123" };
        var services = CreateServices(access, orchestrator, observability);

        var attribute = new RequireCurrentFieldAccessAttribute("ViewProduction");
        var context = CreateContext(services, CreatePrincipal(userId: "user-1"));

        await attribute.OnAuthorizationAsync(context);

        Assert.IsType<ForbidResult>(context.Result);
        Assert.Equal("user-1", access.LastUserId);
        Assert.Equal("FIELD-123", access.LastAssetId);
        Assert.Equal("FIELD", access.LastAssetType);
        Assert.Equal("ViewProduction", access.LastPermission);
        Assert.Equal("Denied", observability.LastObservation?.Decision);
        Assert.Equal("FIELD-123", observability.LastObservation?.AssetId);
        Assert.Equal(nameof(RequireCurrentFieldAccessAttribute), observability.LastObservation?.PolicyName);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenAccessGranted_AllowsRequest()
    {
        var access = new TestAccessControlService
        {
            NextResponse = new AccessCheckResponse { HasAccess = true }
        };
        var observability = new TestAuthorizationObservabilityService();
        var orchestrator = new TestFieldOrchestrator { CurrentFieldId = "FIELD-999" };
        var services = CreateServices(access, orchestrator, observability);

        var attribute = new RequireCurrentFieldAccessAttribute();
        var context = CreateContext(services, CreatePrincipal(userId: "user-2"));

        await attribute.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal("user-2", access.LastUserId);
        Assert.Equal("Allowed", observability.LastObservation?.Decision);
        Assert.Equal("FIELD-999", observability.LastObservation?.AssetId);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenClaimAndIdentityNameDiffer_UsesNameIdentifierClaim()
    {
        var access = new TestAccessControlService
        {
            NextResponse = new AccessCheckResponse { HasAccess = true }
        };
        var orchestrator = new TestFieldOrchestrator { CurrentFieldId = "FIELD-ABC" };
        var services = CreateServices(access, orchestrator);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "trusted-user-id"),
            new(ClaimTypes.Name, "display-name")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth", ClaimTypes.Name, ClaimTypes.Role));

        var attribute = new RequireCurrentFieldAccessAttribute();
        var context = CreateContext(services, principal);

        await attribute.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal("trusted-user-id", access.LastUserId);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WhenNameIdentifierMissing_FallsBackToIdentityName()
    {
        var access = new TestAccessControlService
        {
            NextResponse = new AccessCheckResponse { HasAccess = true }
        };
        var orchestrator = new TestFieldOrchestrator { CurrentFieldId = "FIELD-FALLBACK" };
        var services = CreateServices(access, orchestrator);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "fallback-name") },
            "TestAuth",
            ClaimTypes.Name,
            ClaimTypes.Role));

        var attribute = new RequireCurrentFieldAccessAttribute();
        var context = CreateContext(services, principal);

        await attribute.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
        Assert.Equal("fallback-name", access.LastUserId);
    }

    private static AuthorizationFilterContext CreateContext(IServiceProvider? services, ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services ?? new EmptyServiceProvider(),
            User = principal
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    private static ClaimsPrincipal CreatePrincipal(string? userId)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth", nameType: ClaimTypes.NameIdentifier, roleType: ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    private static IServiceProvider CreateServices(
        TestAccessControlService accessControl,
        TestFieldOrchestrator fieldOrchestrator,
        TestAuthorizationObservabilityService? observability = null)
    {
        var services = new Dictionary<Type, object>
        {
            { typeof(IAccessControlService), accessControl },
            { typeof(IFieldOrchestrator), fieldOrchestrator }
        };

        if (observability != null)
        {
            services.Add(typeof(IAuthorizationObservabilityService), observability);
        }

        return new DictionaryServiceProvider(services);
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
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
        public AccessCheckResponse NextResponse { get; set; } = new() { HasAccess = true };
        public string? LastUserId { get; private set; }
        public string? LastAssetId { get; private set; }
        public string? LastAssetType { get; private set; }
        public string? LastPermission { get; private set; }

        public Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null)
        {
            LastUserId = userId;
            LastAssetId = assetId;
            LastAssetType = assetType;
            LastPermission = requiredPermission;
            return Task.FromResult(NextResponse);
        }

        public Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true)
            => Task.FromResult(new List<AssetAccess>());

        public Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
            => Task.FromResult(new List<string>());

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

    private sealed class TestFieldOrchestrator : IFieldOrchestrator
    {
        public string? CurrentFieldId { get; set; }
        public bool IsFieldActive => !string.IsNullOrWhiteSpace(CurrentFieldId);

        public Task<bool> SetActiveFieldAsync(string fieldId)
        {
            CurrentFieldId = fieldId;
            return Task.FromResult(true);
        }

        public void ClearActiveField() => CurrentFieldId = null;

        public Task<object?> GetCurrentFieldAsync() => Task.FromResult<object?>(null);
        public Task<FieldLifecycleSummary> GetFieldLifecycleSummaryAsync() => Task.FromResult(new FieldLifecycleSummary());
        public Task<List<WELL>> GetFieldWellsAsync() => Task.FromResult(new List<WELL>());
        public Task<FieldStatistics> GetFieldStatisticsAsync() => Task.FromResult(new FieldStatistics());
        public Task<FieldTimeline> GetFieldTimelineAsync() => Task.FromResult(new FieldTimeline());
        public Task<FieldDashboard> GetFieldDashboardAsync() => Task.FromResult(new FieldDashboard());
        public IFieldExplorationService GetExplorationService() => throw new NotImplementedException();
        public IFieldDevelopmentService GetDevelopmentService() => throw new NotImplementedException();
        public IFieldProductionService GetProductionService() => throw new NotImplementedException();
        public IFieldDecommissioningService GetDecommissioningService() => throw new NotImplementedException();
        public IProcessService GetProcessService() => throw new NotImplementedException();
    }

    private sealed class TestAuthorizationObservabilityService : IAuthorizationObservabilityService
    {
        public AuthorizationObservation? LastObservation { get; private set; }

        public Task RecordPolicyEvaluationAsync(AuthorizationObservation observation)
        {
            LastObservation = observation;
            return Task.CompletedTask;
        }
    }
}
