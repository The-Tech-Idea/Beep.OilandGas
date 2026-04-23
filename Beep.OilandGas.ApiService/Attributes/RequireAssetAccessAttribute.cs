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
    /// Authorization attribute that requires access to a specific asset
    /// The asset ID and type are extracted from route parameters or query strings
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequireAssetAccessAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _assetIdParameterName;
        private readonly string? _assetTypeParameterName;
        private readonly string? _requiredPermission;

        /// <summary>
        /// Constructor for RequireAssetAccess attribute
        /// </summary>
        /// <param name="assetIdParameterName">Name of the route/query parameter containing the asset ID (e.g., "fieldId", "wellId")</param>
        /// <param name="assetTypeParameterName">Name of the route/query parameter containing the asset type, or fixed asset type if null</param>
        /// <param name="requiredPermission">Optional permission required (e.g., "ViewProduction", "EditWells")</param>
        public RequireAssetAccessAttribute(
            string assetIdParameterName, 
            string? assetTypeParameterName = null,
            string? requiredPermission = null)
        {
            _assetIdParameterName = assetIdParameterName ?? throw new ArgumentNullException(nameof(assetIdParameterName));
            _assetTypeParameterName = assetTypeParameterName;
            _requiredPermission = requiredPermission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get user ID from claims
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
                    PolicyName = nameof(RequireAssetAccessAttribute),
                    UserId = null,
                    RequiredPermission = _requiredPermission,
                    Decision = "Denied",
                    Reason = "Missing user identity",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            // Extract asset ID from route or query parameters
            string? assetId = null;
            if (context.RouteData.Values.ContainsKey(_assetIdParameterName))
            {
                assetId = context.RouteData.Values[_assetIdParameterName]?.ToString();
            }
            else if (context.HttpContext.Request.Query.ContainsKey(_assetIdParameterName))
            {
                assetId = context.HttpContext.Request.Query[_assetIdParameterName].ToString();
            }

            if (string.IsNullOrEmpty(assetId))
            {
                context.Result = new BadRequestObjectResult(new { error = $"Asset ID parameter '{_assetIdParameterName}' is required" });
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireAssetAccessAttribute),
                    UserId = userId,
                    RequiredPermission = _requiredPermission,
                    Decision = "Denied",
                    Reason = $"Missing asset ID parameter '{_assetIdParameterName}'",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            // Determine asset type
            string assetType = _assetTypeParameterName ?? InferAssetTypeFromParameterName(_assetIdParameterName);
            
            if (!string.IsNullOrEmpty(_assetTypeParameterName))
            {
                if (context.RouteData.Values.ContainsKey(_assetTypeParameterName))
                {
                    assetType = context.RouteData.Values[_assetTypeParameterName]?.ToString() ?? assetType;
                }
                else if (context.HttpContext.Request.Query.ContainsKey(_assetTypeParameterName))
                {
                    assetType = context.HttpContext.Request.Query[_assetTypeParameterName].ToString();
                }
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
                    PolicyName = nameof(RequireAssetAccessAttribute),
                    UserId = userId,
                    AssetId = assetId,
                    AssetType = assetType,
                    RequiredPermission = _requiredPermission,
                    Decision = "Error",
                    Reason = "Access control service unavailable",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            // Check access
            var accessCheck = await accessControlService.CheckAssetAccessAsync(userId, assetId, assetType, _requiredPermission);

            if (!accessCheck.HasAccess)
            {
                context.Result = new ForbidResult();
                await EmitObservationAsync(observability, new AuthorizationObservation
                {
                    PolicyName = nameof(RequireAssetAccessAttribute),
                    UserId = userId,
                    AssetId = assetId,
                    AssetType = assetType,
                    RequiredPermission = _requiredPermission,
                    Decision = "Denied",
                    Reason = accessCheck.Reason ?? "Access denied",
                    Endpoint = endpoint,
                    HttpMethod = method,
                    CorrelationId = correlationId,
                    ClientIp = clientIp
                });
                return;
            }

            await EmitObservationAsync(observability, new AuthorizationObservation
            {
                PolicyName = nameof(RequireAssetAccessAttribute),
                UserId = userId,
                AssetId = assetId,
                AssetType = assetType,
                RequiredPermission = _requiredPermission,
                Decision = "Allowed",
                Reason = accessCheck.Reason ?? "Access granted",
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

        private string InferAssetTypeFromParameterName(string parameterName)
        {
            // Infer asset type from parameter name
            return parameterName.ToUpper() switch
            {
                var name when name.Contains("FIELD") => "FIELD",
                var name when name.Contains("POOL") => "POOL",
                var name when name.Contains("WELL") => "WELL",
                var name when name.Contains("FACILITY") => "FACILITY",
                var name when name.Contains("AREA") => "AREA",
                _ => "UNKNOWN"
            };
        }
    }
}
