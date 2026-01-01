namespace Beep.OilandGas.UserManagement.Core.Authorization
{
    /// <summary>
    /// Platform-agnostic authorization context containing user information
    /// </summary>
    public class AuthorizationContext
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the roles the user belongs to
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the permissions the user has
        /// </summary>
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// Gets or sets the tenant ID (for multi-tenant scenarios)
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets additional claims/attributes
        /// </summary>
        public Dictionary<string, string> Claims { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        public bool IsInRole(string roleName)
        {
            return Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if the user has a specific permission
        /// </summary>
        public bool HasPermission(string permission)
        {
            return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }
    }
}
