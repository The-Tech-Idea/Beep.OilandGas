using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.ApiService.Attributes
{
    /// <summary>
    /// Authorization attribute that requires a specific role
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public RequireRoleAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get user ID from claims or context
            var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.HttpContext.User?.Identity?.Name;
            var observability = context.HttpContext.RequestServices
                .GetService(typeof(IAuthorizationObservabilityService))
                as IAuthorizationObservabilityService;
            var correlationId = context.HttpContext.TraceIdentifier;
            var endpoint = context.HttpContext.Request.Path.ToString();
            var method = context.HttpContext.Request.Method;
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireRoleAttribute),
                    UserId = null,
                    AssetType = "ROLE",
                    RequiredPermission = string.Join(",", _requiredRoles),
                    Decision = "Denied",
                    Reason = "Missing user identity",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            // Get access control service from DI
            var accessControlService = context.HttpContext.RequestServices
                .GetService(typeof(IAccessControlService)) 
                as IAccessControlService;

            if (accessControlService == null)
            {
                context.Result = new StatusCodeResult(500);
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireRoleAttribute),
                    UserId = userId,
                    AssetType = "ROLE",
                    RequiredPermission = string.Join(",", _requiredRoles),
                    Decision = "Error",
                    Reason = "Access control service unavailable",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            // Get user's roles
            var userRoles = await accessControlService.GetUserRolesAsync(userId);

            // Check if user has any of the required roles
            bool hasRequiredRole = _requiredRoles.Any(role => userRoles.Contains(role));

            if (!hasRequiredRole)
            {
                context.Result = new ForbidResult();
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireRoleAttribute),
                    UserId = userId,
                    AssetType = "ROLE",
                    RequiredPermission = string.Join(",", _requiredRoles),
                    Decision = "Denied",
                    Reason = "User missing required role",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireRoleAttribute),
                UserId = userId,
                AssetType = "ROLE",
                RequiredPermission = string.Join(",", _requiredRoles),
                Decision = "Allowed",
                Reason = "User has required role",
                Endpoint = endpoint,
                HttpMethod = method,
                CorrelationId = correlationId,
                ClientIp = clientIp
            });
        }

        private static Task EmitObservationAsync(
            IAuthorizationObservabilityService? observability,
            AuthorizationObservation observation)
        {
            return observability == null
                ? Task.CompletedTask
                : observability.RecordPolicyEvaluationAsync(observation);
        }
    }
}
