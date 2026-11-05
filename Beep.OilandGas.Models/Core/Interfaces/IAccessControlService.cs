using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Core access control service interface
    /// </summary>
    public interface IAccessControlService
    {
        /// <summary>
        /// Checks if a user has access to a specific asset
        /// </summary>
        Task<AccessCheckResponse> CheckAssetAccessAsync(string userId, string assetId, string assetType, string? requiredPermission = null, string? facilityType = null);

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
        Task<bool> GrantAssetAccessAsync(string userId, string assetId, string assetType, string accessLevel = "READ", bool inherit = true, string? organizationId = null, string? facilityType = null);

        /// <summary>
        /// Revokes access to an asset for a user
        /// </summary>
        Task<bool> RevokeAssetAccessAsync(string userId, string assetId, string assetType, string? facilityType = null);

        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantReviewPage> ReviewStoredGrantsAsync(string userId, int offset = 0, int limit = 50, CancellationToken token = default);
        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantRetirement> RetireStoredGrantAsync(string grantId, Beep.OilandGas.Models.Data.AccessControl.RetireAssetGrantRequest request, CancellationToken token = default);
        Task<Beep.OilandGas.Models.Data.AccessControl.AssetGrantRetirementPage> GetGrantRetirementsAsync(string grantId, int offset = 0, int limit = 50, CancellationToken token = default);
        Task<Beep.OilandGas.Models.Data.Lease.FieldInterestReview> GetFieldInterestReviewAsync(string fieldId);

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

}
