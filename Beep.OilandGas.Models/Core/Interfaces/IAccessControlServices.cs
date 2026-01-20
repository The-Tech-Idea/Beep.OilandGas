using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    #region Access Control Service Interfaces

    /// <summary>
    /// Core access control service interface
    /// </summary>
    public interface IAccessControlService
    {
        /// <summary>
        /// Checks if a user has access to a specific asset
        /// </summary>
        Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null);

        /// <summary>
        /// Gets all assets a user can access
        /// </summary>
        Task<List<AssetAccess>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true);

        /// <summary>
        /// Gets all roles assigned to a user
        /// </summary>
        Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null);

        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        Task<bool> HasPermissionAsync(string userId, string permissionId, string? organizationId = null);

        /// <summary>
        /// Grants access to an asset for a user
        /// </summary>
        Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null);

        /// <summary>
        /// Revokes access to an asset for a user
        /// </summary>
        Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType);

        /// <summary>
        /// Gets all permissions for a role
        /// </summary>
        Task<List<string>> GetRolePermissionsAsync(string roleId, string? organizationId = null);

        /// <summary>
        /// Assigns a permission to a role
        /// </summary>
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId, string? organizationId = null);

        /// <summary>
        /// Removes a permission from a role
        /// </summary>
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId, string? organizationId = null);
    }

    /// <summary>
    /// Asset hierarchy service interface
    /// </summary>
    public interface IAssetHierarchyService
    {
        /// <summary>
        /// Gets the asset hierarchy for an organization
        /// </summary>
        Task<AssetHierarchyNode?> GetAssetHierarchyAsync(string organizationId, string? rootAssetId = null, string? rootAssetType = null);

        /// <summary>
        /// Gets the asset hierarchy filtered by user access
        /// </summary>
        Task<AssetHierarchyNode?> GetAssetHierarchyForUserAsync(string userId, string? organizationId = null, string? rootAssetId = null, string? rootAssetType = null);

        /// <summary>
        /// Gets child assets of a given asset
        /// </summary>
        Task<List<AssetHierarchyNode>> GetAssetChildrenAsync(string assetId, string assetType, string? organizationId = null);

        /// <summary>
        /// Gets the full path from root to a given asset
        /// </summary>
        Task<List<AssetHierarchyNode>> GetAssetPathAsync(string assetId, string assetType, string? organizationId = null);

        /// <summary>
        /// Validates if a user has access to an asset path
        /// </summary>
        Task<bool> ValidateAccessAsync(string userId, List<AssetHierarchyNode> assetPath);

        /// <summary>
        /// Gets the hierarchy configuration for an organization
        /// </summary>
        Task<List<HierarchyConfig>> GetHierarchyConfigAsync(string organizationId);

        /// <summary>
        /// Updates the hierarchy configuration for an organization
        /// </summary>
        Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfig> config);
    }

    /// <summary>
    /// User profile service interface
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets a user's profile
        /// </summary>
        Task<UserProfile?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null);

        /// <summary>
        /// Gets the default layout for a user based on their primary role
        /// </summary>
        Task<string?> GetUserDefaultLayoutAsync(string userId);

        /// <summary>
        /// Updates user preferences
        /// </summary>
        Task<bool> UpdateUserPreferencesAsync(string userId, string preferencesJson);

        /// <summary>
        /// Updates user's primary role
        /// </summary>
        Task<bool> UpdateUserPrimaryRoleAsync(string userId, string primaryRole);

        /// <summary>
        /// Updates user's preferred layout
        /// </summary>
        Task<bool> UpdateUserPreferredLayoutAsync(string userId, string preferredLayout);

        /// <summary>
        /// Records user login
        /// </summary>
        Task RecordUserLoginAsync(string userId);
    }

    #endregion
}




