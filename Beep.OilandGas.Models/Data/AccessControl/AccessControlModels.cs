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
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? PrimaryRoleValue;

        public string? PrimaryRole

        {

            get { return this.PrimaryRoleValue; }

            set { SetProperty(ref PrimaryRoleValue, value); }

        }
        private string? PreferredLayoutValue;

        public string? PreferredLayout

        {

            get { return this.PreferredLayoutValue; }

            set { SetProperty(ref PreferredLayoutValue, value); }

        }
        private string? UserPreferencesValue;

        public string? UserPreferences

        {

            get { return this.UserPreferencesValue; }

            set { SetProperty(ref UserPreferencesValue, value); }

        } // JSON string
        private DateTime? LastLoginDateValue;

        public DateTime? LastLoginDate

        {

            get { return this.LastLoginDateValue; }

            set { SetProperty(ref LastLoginDateValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
        private List<string> RolesValue = new List<string>();

        public List<string> Roles

        {

            get { return this.RolesValue; }

            set { SetProperty(ref RolesValue, value); }

        }
    }

    /// <summary>
    /// Asset access information
    /// </summary>
    public class AssetAccess : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AccessLevelValue = "READ";

        public string AccessLevel

        {

            get { return this.AccessLevelValue; }

            set { SetProperty(ref AccessLevelValue, value); }

        } // READ, WRITE, DELETE
        private bool InheritValue = true;

        public bool Inherit

        {

            get { return this.InheritValue; }

            set { SetProperty(ref InheritValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
    }

    /// <summary>
    /// Role and permission definitions
    /// </summary>
    public class RolePermission : ModelEntityBase
    {
        private string RoleIdValue = string.Empty;

        public string RoleId

        {

            get { return this.RoleIdValue; }

            set { SetProperty(ref RoleIdValue, value); }

        }
        private string PermissionIdValue = string.Empty;

        public string PermissionId

        {

            get { return this.PermissionIdValue; }

            set { SetProperty(ref PermissionIdValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
    }

    /// <summary>
    /// Organization hierarchy configuration
    /// </summary>
    public class HierarchyConfig : ModelEntityBase
    {
        private string OrganizationIdValue = string.Empty;

        public string OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private int HierarchyLevelValue;

        public int HierarchyLevel

        {

            get { return this.HierarchyLevelValue; }

            set { SetProperty(ref HierarchyLevelValue, value); }

        }
        private string? LevelNameValue;

        public string? LevelName

        {

            get { return this.LevelNameValue; }

            set { SetProperty(ref LevelNameValue, value); }

        }
        private string? AssetTypeValue;

        public string? AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private int? ParentLevelValue;

        public int? ParentLevel

        {

            get { return this.ParentLevelValue; }

            set { SetProperty(ref ParentLevelValue, value); }

        }
        private bool ActiveValue = true;

        public bool Active

        {

            get { return this.ActiveValue; }

            set { SetProperty(ref ActiveValue, value); }

        }
    }

    /// <summary>
    /// Access check request
    /// </summary>
    public class AccessCheckRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string? RequiredPermissionValue;

        public string? RequiredPermission

        {

            get { return this.RequiredPermissionValue; }

            set { SetProperty(ref RequiredPermissionValue, value); }

        }
    }

    /// <summary>
    /// Access check response
    /// </summary>
    public class AccessCheckResponse : ModelEntityBase
    {
        private bool HasAccessValue;

        public bool HasAccess

        {

            get { return this.HasAccessValue; }

            set { SetProperty(ref HasAccessValue, value); }

        }
        private string? AccessLevelValue;

        public string? AccessLevel

        {

            get { return this.AccessLevelValue; }

            set { SetProperty(ref AccessLevelValue, value); }

        }
        private string? ReasonValue;

        public string? Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }

    /// <summary>
    /// Request to get user's accessible assets
    /// </summary>
    public class GetAccessibleAssetsRequest : ModelEntityBase
    {
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? AssetTypeValue;

        public string? AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private bool IncludeInheritedValue = true;

        public bool IncludeInherited

        {

            get { return this.IncludeInheritedValue; }

            set { SetProperty(ref IncludeInheritedValue, value); }

        }
    }

    /// <summary>
    /// Asset hierarchy node
    /// </summary>
    public class AssetHierarchyNode : ModelEntityBase
    {
        private string AssetIdValue = string.Empty;

        public string AssetId

        {

            get { return this.AssetIdValue; }

            set { SetProperty(ref AssetIdValue, value); }

        }
        private string AssetTypeValue = string.Empty;

        public string AssetType

        {

            get { return this.AssetTypeValue; }

            set { SetProperty(ref AssetTypeValue, value); }

        }
        private string? AssetNameValue;

        public string? AssetName

        {

            get { return this.AssetNameValue; }

            set { SetProperty(ref AssetNameValue, value); }

        }
        private string? ParentAssetIdValue;

        public string? ParentAssetId

        {

            get { return this.ParentAssetIdValue; }

            set { SetProperty(ref ParentAssetIdValue, value); }

        }
        private string? ParentAssetTypeValue;

        public string? ParentAssetType

        {

            get { return this.ParentAssetTypeValue; }

            set { SetProperty(ref ParentAssetTypeValue, value); }

        }
        private List<AssetHierarchyNode> ChildrenValue = new List<AssetHierarchyNode>();

        public List<AssetHierarchyNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
        private bool UserHasAccessValue;

        public bool UserHasAccess

        {

            get { return this.UserHasAccessValue; }

            set { SetProperty(ref UserHasAccessValue, value); }

        }
    }

    /// <summary>
    /// Request to get asset hierarchy
    /// </summary>
    public class GetAssetHierarchyRequest : ModelEntityBase
    {
        private string? OrganizationIdValue;

        public string? OrganizationId

        {

            get { return this.OrganizationIdValue; }

            set { SetProperty(ref OrganizationIdValue, value); }

        }
        private string? RootAssetIdValue;

        public string? RootAssetId

        {

            get { return this.RootAssetIdValue; }

            set { SetProperty(ref RootAssetIdValue, value); }

        }
        private string? RootAssetTypeValue;

        public string? RootAssetType

        {

            get { return this.RootAssetTypeValue; }

            set { SetProperty(ref RootAssetTypeValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        } // Optional: filter by user access
    }

    #endregion
}







