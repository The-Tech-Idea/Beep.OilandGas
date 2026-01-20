using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly string _assetTypeParameterName;
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
            var userId = context.HttpContext.User?.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
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
                .GetService(typeof(Beep.OilandGas.Models.Data.IAccessControlService))
                as Beep.OilandGas.Models.Data.IAccessControlService;

            if (accessControlService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Check access
            var accessCheck = await accessControlService.CheckAssetAccessAsync(userId, assetId, assetType, _requiredPermission);

            if (!accessCheck.HasAccess)
            {
                context.Result = new ForbidResult();
            }
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
