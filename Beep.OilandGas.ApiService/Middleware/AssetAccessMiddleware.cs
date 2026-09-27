using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Middleware
{
    /// <summary>
    /// Middleware to filter API requests based on user's asset access and inject asset filters
    /// </summary>
    public class AssetAccessMiddleware
    {
        /// <summary>
        /// The caller's own account (<c>/api/auth</c>) and the repository's setup (<c>/api/setup</c>) read no asset data,
        /// so their requests are not asked which assets the caller may reach. Asking there made a fresh repository impossible
        /// to set up: resolving asset access needs the PPDM_CORE binding, binding it is an <c>/api/setup</c> request, and
        /// every such request failed until it was bound — as did the account endpoints, so nobody's roles loaded and account
        /// deletion refused (found 2026-09-25). <see cref="SetupGateMiddleware"/> runs earlier and answers a different
        /// question — whether the repository itself is installed — so it still gates <c>/api/setup/modules</c>.
        /// </summary>
        private static readonly PathString[] NoAssetDataPaths = ["/api/auth", "/api/setup"];

        private readonly RequestDelegate _next;

        public AssetAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindActingUserId();

            if (!string.IsNullOrEmpty(userId)
                && !NoAssetDataPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
            {
                // Get access control service from DI
                var accessControlService = context.RequestServices
                    .GetService(typeof(IAccessControlService)) as IAccessControlService;

                if (accessControlService != null)
                {
                    // Store accessible assets in HttpContext.Items for use by controllers/services
                    var accessibleAssets = await accessControlService.GetUserAccessibleAssetsAsync(userId);
                    
                    // Group by asset type for easy filtering
                    var assetsByType = accessibleAssets
                        .Where(a => a.Active)
                        .GroupBy(a => a.AssetType)
                        .ToDictionary(g => g.Key, g => g.Select(a => a.AssetId).ToList());

                    context.Items["AccessibleAssets"] = accessibleAssets;
                    context.Items["AccessibleAssetsByType"] = assetsByType;
                    context.Items["UserId"] = userId;

                    // For GET requests that return asset lists, inject filters automatically
                    if (context.Request.Method == "GET" && !(context.Request.Path.Value?.Contains("/api/accesscontrol") ?? false))
                    {
                        // Extract asset type from path (e.g., /api/production/fields -> FIELD)
                        var assetType = InferAssetTypeFromPath(context.Request.Path.Value ?? string.Empty);

                        if (!string.IsNullOrEmpty(assetType) && assetsByType.ContainsKey(assetType))
                        {
                            // Add asset filter to query string if not already present
                            var queryParams = context.Request.Query;
                            
                            if (!queryParams.ContainsKey($"{assetType}_ID"))
                            {
                                var assetIds = assetsByType[assetType];
                                
                                // If user has limited access, inject filter
                                // Note: This is a simplified approach - full implementation would
                                // need to modify the query more carefully
                                context.Items["AssetFilter"] = new AppFilter
                                {
                                    FieldName = $"{assetType}_ID",
                                    Operator = "IN",
                                    FilterValue = string.Join(",", assetIds)
                                };
                            }
                        }
                    }
                }
            }

            await _next(context);
        }

        private string? InferAssetTypeFromPath(string path)
        {
            // Infer asset type from API path
            var pathLower = path.ToLower();

            if (pathLower.Contains("/fields")) return "FIELD";
            if (pathLower.Contains("/pools")) return "POOL";
            if (pathLower.Contains("/wells")) return "WELL";
            if (pathLower.Contains("/facilities")) return "FACILITY";
            if (pathLower.Contains("/areas")) return "AREA";

            return null;
        }
    }
}
