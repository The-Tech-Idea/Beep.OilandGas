using Beep.OilandGas.UserManagement.Core.Authentication;

namespace Beep.OilandGas.UserManagement.Core.Authentication
{
    /// <summary>
    /// Platform-agnostic interface to get the current authenticated user
    /// Abstracts how user context is obtained (HTTP context, AsyncLocal, etc.)
    /// </summary>
    public interface ICurrentUserProvider
    {
        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        /// <returns>UserPrincipal if authenticated, null otherwise</returns>
        Task<UserPrincipal?> GetCurrentUserAsync();

        /// <summary>
        /// Gets the current user's ID (convenience method)
        /// </summary>
        /// <returns>User ID if authenticated, null otherwise</returns>
        Task<string?> GetCurrentUserIdAsync();

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();
    }
}
