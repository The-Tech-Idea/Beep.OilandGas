using Beep.OilandGas.Models.Core.Interfaces.Security;

namespace Beep.OilandGas.UserManagement.Core.Authorization
{
    /// <summary>
    /// Default implementation of IPermissionEvaluator
    /// </summary>
    public class PermissionEvaluator : IPermissionEvaluator
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;

        public PermissionEvaluator(
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<AuthorizationResult> EvaluateAsync(string userId, string permission)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthorizationResult.Fail(userId, permission, "User ID is required");
            }

            if (string.IsNullOrWhiteSpace(permission))
            {
                return AuthorizationResult.Fail(userId, permission, "Permission is required");
            }

            try
            {
                var hasPermission = await _authorizationService.UserHasPermissionAsync(userId, permission);
                
                if (hasPermission)
                {
                    var roles = await _userService.GetRolesAsync(userId);
                    return AuthorizationResult.Success(userId, permission)
                        .WithRoles(roles);
                }

                return AuthorizationResult.Fail(userId, permission, "User does not have the required permission");
            }
            catch (Exception ex)
            {
                return AuthorizationResult.Fail(userId, permission, $"Error checking permission: {ex.Message}");
            }
        }

        public async Task<AuthorizationResult> EvaluateAnyAsync(string userId, IEnumerable<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthorizationResult.Fail(userId, string.Join(", ", permissions), "User ID is required");
            }

            var permissionList = permissions?.ToList() ?? new List<string>();
            if (!permissionList.Any())
            {
                return AuthorizationResult.Fail(userId, "", "At least one permission is required");
            }

            foreach (var permission in permissionList)
            {
                var result = await EvaluateAsync(userId, permission);
                if (result.IsAuthorized)
                {
                    return result;
                }
            }

            return AuthorizationResult.Fail(userId, string.Join(", ", permissionList), 
                "User does not have any of the required permissions");
        }

        public async Task<AuthorizationResult> EvaluateAllAsync(string userId, IEnumerable<string> permissions)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return AuthorizationResult.Fail(userId, string.Join(", ", permissions), "User ID is required");
            }

            var permissionList = permissions?.ToList() ?? new List<string>();
            if (!permissionList.Any())
            {
                return AuthorizationResult.Fail(userId, "", "At least one permission is required");
            }

            var failedPermissions = new List<string>();
            foreach (var permission in permissionList)
            {
                var result = await EvaluateAsync(userId, permission);
                if (!result.IsAuthorized)
                {
                    failedPermissions.Add(permission);
                }
            }

            if (failedPermissions.Any())
            {
                return AuthorizationResult.Fail(userId, string.Join(", ", permissionList),
                    $"User does not have the following permissions: {string.Join(", ", failedPermissions)}");
            }

            var roles = await _userService.GetRolesAsync(userId);
            return AuthorizationResult.Success(userId, string.Join(", ", permissionList))
                .WithRoles(roles);
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            try
            {
                return await _authorizationService.UserIsInRoleAsync(userId, roleName);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Extension methods for AuthorizationResult
    /// </summary>
    public static class AuthorizationResultExtensions
    {
        public static AuthorizationResult WithRoles(this AuthorizationResult result, IEnumerable<string> roles)
        {
            result.UserRoles = roles?.ToList() ?? new List<string>();
            return result;
        }
    }
}
