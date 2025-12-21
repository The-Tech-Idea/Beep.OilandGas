using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.Report;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.DTOs
{
    #region Access Control DTOs

    /// <summary>
    /// User profile with roles and preferences
    /// </summary>
    public class UserProfileDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string? PrimaryRole { get; set; }
        public string? PreferredLayout { get; set; }
        public string? UserPreferences { get; set; } // JSON string
        public DateTime? LastLoginDate { get; set; }
        public bool Active { get; set; } = true;
        public List<string> Roles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Asset access information
    /// </summary>
    public class AssetAccessDTO
    {
        public string UserId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = "READ"; // READ, WRITE, DELETE
        public bool Inherit { get; set; } = true;
        public string? OrganizationId { get; set; }
        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Role and permission definitions
    /// </summary>
    public class RolePermissionDTO
    {
        public string RoleId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
        public string? OrganizationId { get; set; }
        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Organization hierarchy configuration
    /// </summary>
    public class HierarchyConfigDTO
    {
        public string OrganizationId { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string? LevelName { get; set; }
        public string? AssetType { get; set; }
        public int? ParentLevel { get; set; }
        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Access check request
    /// </summary>
    public class AccessCheckRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string? RequiredPermission { get; set; }
    }

    /// <summary>
    /// Access check response
    /// </summary>
    public class AccessCheckResponse
    {
        public bool HasAccess { get; set; }
        public string? AccessLevel { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request to get user's accessible assets
    /// </summary>
    public class GetAccessibleAssetsRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public string? OrganizationId { get; set; }
        public bool IncludeInherited { get; set; } = true;
    }

    /// <summary>
    /// Asset hierarchy node
    /// </summary>
    public class AssetHierarchyNode
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string? AssetName { get; set; }
        public string? ParentAssetId { get; set; }
        public string? ParentAssetType { get; set; }
        public List<AssetHierarchyNode> Children { get; set; } = new List<AssetHierarchyNode>();
        public bool UserHasAccess { get; set; }
    }

    /// <summary>
    /// Request to get asset hierarchy
    /// </summary>
    public class GetAssetHierarchyRequest
    {
        public string? OrganizationId { get; set; }
        public string? RootAssetId { get; set; }
        public string? RootAssetType { get; set; }
        public string? UserId { get; set; } // Optional: filter by user access
    }

    #endregion

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
        Task<List<AssetAccessDTO>> GetUserAccessibleAssetsAsync(string userId, string? assetType = null, string? organizationId = null, bool includeInherited = true);

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
        Task<List<HierarchyConfigDTO>> GetHierarchyConfigAsync(string organizationId);

        /// <summary>
        /// Updates the hierarchy configuration for an organization
        /// </summary>
        Task<bool> UpdateHierarchyConfigAsync(string organizationId, List<HierarchyConfigDTO> config);
    }

    /// <summary>
    /// User profile service interface
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets a user's profile
        /// </summary>
        Task<UserProfileDTO?> GetUserProfileAsync(string userId);

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
