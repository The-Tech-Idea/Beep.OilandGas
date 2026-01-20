using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Access Control DTOs

    /// <summary>
    /// User profile with roles and preferences
    /// </summary>
    public class UserProfile : ModelEntityBase
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
    public class AssetAccess : ModelEntityBase
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
    public class RolePermission : ModelEntityBase
    {
        public string RoleId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
        public string? OrganizationId { get; set; }
        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Organization hierarchy configuration
    /// </summary>
    public class HierarchyConfig : ModelEntityBase
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
    public class AccessCheckRequest : ModelEntityBase
    {
        public string UserId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string? RequiredPermission { get; set; }
    }

    /// <summary>
    /// Access check response
    /// </summary>
    public class AccessCheckResponse : ModelEntityBase
    {
        public bool HasAccess { get; set; }
        public string? AccessLevel { get; set; }
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request to get user's accessible assets
    /// </summary>
    public class GetAccessibleAssetsRequest : ModelEntityBase
    {
        public string UserId { get; set; } = string.Empty;
        public string? AssetType { get; set; }
        public string? OrganizationId { get; set; }
        public bool IncludeInherited { get; set; } = true;
    }

    /// <summary>
    /// Asset hierarchy node
    /// </summary>
    public class AssetHierarchyNode : ModelEntityBase
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
    public class GetAssetHierarchyRequest : ModelEntityBase
    {
        public string? OrganizationId { get; set; }
        public string? RootAssetId { get; set; }
        public string? RootAssetType { get; set; }
        public string? UserId { get; set; } // Optional: filter by user access
    }

    #endregion
}





