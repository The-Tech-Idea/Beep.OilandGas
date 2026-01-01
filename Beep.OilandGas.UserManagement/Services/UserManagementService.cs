using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.UserManagement.Core.Authentication;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Unified facade service for user management operations
    /// Wraps IUserService, IRoleService, IPermissionService, and IAuthService
    /// Methods with optional userId parameter use current user context if not provided
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IAuthService _authService;
        private readonly ICurrentUserProvider _currentUserProvider;

        public UserManagementService(
            IUserService userService,
            IRoleService roleService,
            IPermissionService permissionService,
            IAuthService authService,
            ICurrentUserProvider currentUserProvider)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
            _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _currentUserProvider = currentUserProvider ?? throw new ArgumentNullException(nameof(currentUserProvider));
        }

        // ============================================
        // USER OPERATIONS
        // ============================================

        public Task<USER?> GetUserByIdAsync(string id) => _userService.GetByIdAsync(id);
        public Task<USER?> GetUserByUsernameAsync(string username) => _userService.GetByUsernameAsync(username);
        public Task<IEnumerable<USER>> GetAllUsersAsync() => _userService.GetAllAsync();
        public Task<USER> CreateUserAsync(USER user, string password) => _userService.CreateAsync(user, password);
        public Task<bool> UpdateUserAsync(USER user) => _userService.UpdateAsync(user);
        public Task<bool> DeleteUserAsync(string id) => _userService.DeleteAsync(id);
        public Task<bool> CheckPasswordAsync(USER user, string password) => _userService.CheckPasswordAsync(user, password);

        // ============================================
        // ROLE OPERATIONS
        // ============================================

        public Task<ROLE?> GetRoleByIdAsync(string id) => _roleService.GetByIdAsync(id);
        public Task<ROLE?> GetRoleByNameAsync(string name) => _roleService.GetByNameAsync(name);
        public Task<IEnumerable<ROLE>> GetAllRolesAsync() => _roleService.GetAllAsync();
        public Task<ROLE> CreateRoleAsync(ROLE role) => _roleService.CreateAsync(role);
        public Task<bool> UpdateRoleAsync(ROLE role) => _roleService.UpdateAsync(role);
        public Task<bool> DeleteRoleAsync(string id) => _roleService.DeleteAsync(id);

        // ============================================
        // PERMISSION OPERATIONS
        // ============================================

        public Task<PERMISSION?> GetPermissionByIdAsync(string id) => _permissionService.GetByIdAsync(id);
        public Task<PERMISSION?> GetPermissionByCodeAsync(string code) => _permissionService.GetByCodeAsync(code);
        public Task<IEnumerable<PERMISSION>> GetAllPermissionsAsync() => _permissionService.GetAllAsync();
        public Task<PERMISSION> CreatePermissionAsync(PERMISSION permission) => _permissionService.CreateAsync(permission);
        public Task<bool> DeletePermissionAsync(string id) => _permissionService.DeleteAsync(id);

        // ============================================
        // USER-ROLE RELATIONSHIPS
        // ============================================

        public Task<bool> AddUserToRoleAsync(string userId, string roleName) => _userService.AddToRoleAsync(userId, roleName);
        public Task<bool> RemoveUserFromRoleAsync(string userId, string roleName) => _userService.RemoveFromRoleAsync(userId, roleName);

        public async Task<IEnumerable<string>> GetUserRolesAsync(string? userId = null)
        {
            var actualUserId = userId ?? await _currentUserProvider.GetCurrentUserIdAsync()
                ?? throw new UnauthorizedAccessException("No user context available");
            return await _userService.GetRolesAsync(actualUserId);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string? userId = null)
        {
            var actualUserId = userId ?? await _currentUserProvider.GetCurrentUserIdAsync()
                ?? throw new UnauthorizedAccessException("No user context available");
            
            // Get user's roles
            var roleIds = await _userService.GetRolesAsync(actualUserId);
            
            // Get permissions for each role
            var allPermissions = new HashSet<string>();
            foreach (var roleId in roleIds)
            {
                var rolePermissions = await _roleService.GetRolePermissionsAsync(roleId);
                foreach (var permission in rolePermissions)
                {
                    allPermissions.Add(permission);
                }
            }
            
            return allPermissions;
        }

        // ============================================
        // ROLE-PERMISSION RELATIONSHIPS
        // ============================================

        public Task<bool> AddPermissionToRoleAsync(string roleId, string permissionCode) => _roleService.AddPermissionToRoleAsync(roleId, permissionCode);
        public Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionCode) => _roleService.RemovePermissionFromRoleAsync(roleId, permissionCode);

        // ============================================
        // AUTHENTICATION OPERATIONS
        // ============================================

        public Task<USER?> ValidateCredentialsAsync(string username, string password) => _authService.ValidateCredentialsAsync(username, password);
        public Task<string> GenerateJwtAsync(USER user) => _authService.GenerateJwtAsync(user);
        public Task<bool> SignOutAsync(string userId) => _authService.SignOutAsync(userId);
    }
}
