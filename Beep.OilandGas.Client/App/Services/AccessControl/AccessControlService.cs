using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.AccessControl
{
    /// <summary>
    /// Unified service for Access Control operations
    /// </summary>
    internal class AccessControlService : ServiceBase, IAccessControlService
    {
        public AccessControlService(BeepOilandGasApp app, ILogger<AccessControlService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<bool> CheckAssetAccessAsync(string userId, string assetId, string assetType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentException("Asset ID is required", nameof(assetId));
            if (string.IsNullOrEmpty(assetType)) throw new ArgumentException("Asset type is required", nameof(assetType));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["userId"] = userId,
                    ["assetId"] = assetId,
                    ["assetType"] = assetType
                };
                var endpoint = BuildRequestUriWithParams("/api/accesscontrol/check-access", queryParams);
                return await GetAsync<bool>(endpoint, null, cancellationToken);
            }
            var localService = GetLocalService<IAccessControlLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccessControlLocalService not available");
            return await localService.CheckAssetAccessAsync(userId, assetId, assetType);
        }

        public async Task<List<object>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string> { ["userId"] = userId };
                if (!string.IsNullOrEmpty(assetType)) queryParams["assetType"] = assetType;
                var endpoint = BuildRequestUriWithParams("/api/accesscontrol/accessible-assets", queryParams);
                return await GetAsync<List<object>>(endpoint, null, cancellationToken);
            }
            var localService = GetLocalService<IAccessControlLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccessControlLocalService not available");
            return await localService.GetUserAccessibleAssetsAsync(userId, assetType);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));

            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<string>>($"/api/accesscontrol/user/{Uri.EscapeDataString(userId)}/roles", null, cancellationToken);
            var localService = GetLocalService<IAccessControlLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccessControlLocalService not available");
            return await localService.GetUserRolesAsync(userId);
        }

        public async Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));
            if (string.IsNullOrEmpty(permission)) throw new ArgumentException("Permission is required", nameof(permission));

            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["userId"] = userId,
                    ["permission"] = permission
                };
                var endpoint = BuildRequestUriWithParams("/api/accesscontrol/has-permission", queryParams);
                return await GetAsync<bool>(endpoint, null, cancellationToken);
            }
            var localService = GetLocalService<IAccessControlLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccessControlLocalService not available");
            return await localService.HasPermissionAsync(userId, permission);
        }

        public async Task<object> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required", nameof(userId));

            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/userprofile/{Uri.EscapeDataString(userId)}", null, cancellationToken);
            var localService = GetLocalService<IAccessControlLocalService>();
            if (localService == null) throw new InvalidOperationException("IAccessControlLocalService not available");
            return await localService.GetUserProfileAsync(userId);
        }
    }

    public interface IAccessControlLocalService
    {
        Task<bool> CheckAssetAccessAsync(string userId, string assetId, string assetType);
        Task<List<object>> GetUserAccessibleAssetsAsync(string userId, string? assetType);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> HasPermissionAsync(string userId, string permission);
        Task<object> GetUserProfileAsync(string userId);
    }
}

