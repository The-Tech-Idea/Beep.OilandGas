using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    /// <summary>
    /// Unified facade interface for user management operations
    /// Combines IUserService, IRoleService, IPermissionService, and IAuthService
    /// Methods with optional userId parameter will use current user context if not provided
    /// </summary>
    public interface IUserManagementService
    {
        // ============================================
        // USER OPERATIONS
        // ============================================
        
        Task<USER?> GetUserByIdAsync(string id);
        Task<USER?> GetUserByUsernameAsync(string username);
        Task<IEnumerable<USER>> GetAllUsersAsync();
        Task<USER> CreateUserAsync(USER user, string password);
        Task<bool> UpdateUserAsync(USER user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> CheckPasswordAsync(USER user, string password);
        
        // ============================================
        // ROLE OPERATIONS
        // ============================================
        
        Task<ROLE?> GetRoleByIdAsync(string id);
        Task<ROLE?> GetRoleByNameAsync(string name);
        Task<IEnumerable<ROLE>> GetAllRolesAsync();
        Task<ROLE> CreateRoleAsync(ROLE role);
        Task<bool> UpdateRoleAsync(ROLE role);
        Task<bool> DeleteRoleAsync(string id);
        
        // ============================================
        // PERMISSION OPERATIONS
        // ============================================
        
        Task<PERMISSION?> GetPermissionByIdAsync(string id);
        Task<PERMISSION?> GetPermissionByCodeAsync(string code);
        Task<IEnumerable<PERMISSION>> GetAllPermissionsAsync();
        Task<PERMISSION> CreatePermissionAsync(PERMISSION permission);
        Task<bool> DeletePermissionAsync(string id);
        
        // ============================================
        // USER-ROLE RELATIONSHIPS
        // ============================================
        
        /// <summary>
        /// Adds a user to a role
        /// </summary>
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        
        /// <summary>
        /// Removes a user from a role
        /// </summary>
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        
        /// <summary>
        /// Gets all roles for a user
        /// If userId is null, uses current user context
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(string? userId = null);
        
        /// <summary>
        /// Gets all permissions for a user (from all their roles)
        /// If userId is null, uses current user context
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(string? userId = null);
        
        // ============================================
        // ROLE-PERMISSION RELATIONSHIPS
        // ============================================
        
        Task<bool> AddPermissionToRoleAsync(string roleId, string permissionCode);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionCode);
        
        // ============================================
        // AUTHENTICATION OPERATIONS
        // ============================================
        
        Task<USER?> ValidateCredentialsAsync(string username, string password);
        Task<string> GenerateJwtAsync(USER user);
        Task<bool> SignOutAsync(string userId);
    }
}
