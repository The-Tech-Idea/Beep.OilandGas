using Beep.OilandGas.UserManagement.Models.Identity;

namespace Beep.OilandGas.UserManagement.Contracts.Services;

public interface IRoleAssignmentService
{
    // Role assignments
    Task<AppUserRole> AssignRoleAsync(string userId, string roleId, string grantedByUserId, string? reason = null);
    Task<bool> RevokeRoleAsync(string userRoleId, string revokedByUserId);
    Task<List<AppUserRole>> GetUserRoleAssignmentsAsync(string userId);

    // Permission grants on roles
    Task<AppRolePermission> GrantPermissionToRoleAsync(string roleId, string permissionId, string approvedByUserId);
    Task<bool> RevokePermissionFromRoleAsync(string rolePermissionId, string revokedByUserId);
    Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleId);

    // Catalog queries
    Task<List<AppRole>> GetRoleCatalogAsync();
    Task<List<AppPermission>> GetPermissionCatalogAsync();
}
