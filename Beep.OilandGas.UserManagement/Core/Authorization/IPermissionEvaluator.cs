namespace Beep.OilandGas.UserManagement.Core.Authorization
{
    /// <summary>
    /// Interface for evaluating permissions without platform dependencies
    /// </summary>
    public interface IPermissionEvaluator
    {
        /// <summary>
        /// Evaluates if a user has a specific permission
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="permission">The permission code to check</param>
        /// <returns>Authorization result</returns>
        Task<AuthorizationResult> EvaluateAsync(string userId, string permission);

        /// <summary>
        /// Evaluates if a user has any of the specified permissions
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="permissions">The permission codes to check</param>
        /// <returns>Authorization result</returns>
        Task<AuthorizationResult> EvaluateAnyAsync(string userId, IEnumerable<string> permissions);

        /// <summary>
        /// Evaluates if a user has all of the specified permissions
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="permissions">The permission codes to check</param>
        /// <returns>Authorization result</returns>
        Task<AuthorizationResult> EvaluateAllAsync(string userId, IEnumerable<string> permissions);

        /// <summary>
        /// Evaluates if a user is in a specific role
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="roleName">The role name to check</param>
        /// <returns>True if user is in the role</returns>
        Task<bool> IsInRoleAsync(string userId, string roleName);
    }
}
