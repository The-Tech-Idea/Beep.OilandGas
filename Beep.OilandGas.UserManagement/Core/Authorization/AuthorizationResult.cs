namespace Beep.OilandGas.UserManagement.Core.Authorization
{
    /// <summary>
    /// Result of an authorization check
    /// </summary>
    public class AuthorizationResult
    {
        /// <summary>
        /// Gets or sets whether the authorization was successful
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// Gets or sets the reason for authorization failure (if not authorized)
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the permission that was checked
        /// </summary>
        public string? RequiredPermission { get; set; }

        /// <summary>
        /// Gets or sets the user ID that was checked
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the roles the user has (for debugging/auditing)
        /// </summary>
        public IEnumerable<string> UserRoles { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Creates a successful authorization result
        /// </summary>
        public static AuthorizationResult Success(string userId, string permission)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                UserId = userId,
                RequiredPermission = permission,
                Reason = "Permission granted"
            };
        }

        /// <summary>
        /// Creates a failed authorization result
        /// </summary>
        public static AuthorizationResult Fail(string userId, string permission, string reason)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                UserId = userId,
                RequiredPermission = permission,
                Reason = reason
            };
        }
    }
}
