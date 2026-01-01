using System.Threading;

namespace Beep.OilandGas.UserManagement.Core.Authentication
{
    /// <summary>
    /// Implementation of ICurrentUserProvider for desktop applications
    /// Uses AsyncLocal to store user context per async execution context
    /// </summary>
    public class DesktopCurrentUserProvider : ICurrentUserProvider
    {
        private static readonly AsyncLocal<UserPrincipal?> _currentUser = new AsyncLocal<UserPrincipal?>();

        /// <summary>
        /// Sets the current user for the current async context
        /// Call this after user logs in
        /// </summary>
        public void SetCurrentUser(UserPrincipal? user)
        {
            _currentUser.Value = user;
        }

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        public Task<UserPrincipal?> GetCurrentUserAsync()
        {
            return Task.FromResult(_currentUser.Value);
        }

        /// <summary>
        /// Gets the current user's ID
        /// </summary>
        public Task<string?> GetCurrentUserIdAsync()
        {
            var user = _currentUser.Value;
            return Task.FromResult(user?.UserId);
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return _currentUser.Value?.IsAuthenticated == true;
        }
    }
}
