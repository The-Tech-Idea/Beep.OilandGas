using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.UserManagement.Core.Authorization;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of IAuthorizationService that uses IPermissionEvaluator
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IPermissionEvaluator _permissionEvaluator;

        public AuthorizationService(IPermissionEvaluator permissionEvaluator)
        {
            _permissionEvaluator = permissionEvaluator ?? throw new ArgumentNullException(nameof(permissionEvaluator));
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(permissionCode))
            {
                return false;
            }

            var result = await _permissionEvaluator.EvaluateAsync(userId, permissionCode);
            return result.IsAuthorized;
        }

        public async Task<bool> UserIsInRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
            {
                return false;
            }

            return await _permissionEvaluator.IsInRoleAsync(userId, roleName);
        }
    }
}
