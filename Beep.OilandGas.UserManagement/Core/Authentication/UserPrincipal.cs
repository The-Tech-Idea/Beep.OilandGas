namespace Beep.OilandGas.UserManagement.Core.Authentication
{
    /// <summary>
    /// Platform-agnostic representation of an authenticated user
    /// Can be created from ClaimsPrincipal, WindowsIdentity, or custom implementations
    /// </summary>
    public class UserPrincipal
    {
        /// <summary>
        /// Gets or sets the user ID
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the username
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string? Email { get; set; }

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

        /// <summary>
        /// Creates a UserPrincipal from ASP.NET Core ClaimsPrincipal
        /// </summary>
        public static UserPrincipal FromClaimsPrincipal(System.Security.Claims.ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null || !claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                return new UserPrincipal { IsAuthenticated = false };
            }

            var userId = claimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? claimsPrincipal.FindFirst("sub")?.Value
                ?? claimsPrincipal.Identity.Name
                ?? string.Empty;

            var roles = claimsPrincipal.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var permissions = claimsPrincipal.FindAll("permission")
                .Select(c => c.Value)
                .ToList();

            var claims = claimsPrincipal.Claims
                .ToDictionary(c => c.Type, c => c.Value);

            return new UserPrincipal
            {
                UserId = userId,
                Username = claimsPrincipal.Identity.Name ?? string.Empty,
                Email = claimsPrincipal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Roles = roles,
                Permissions = permissions,
                IsAuthenticated = true,
                Claims = claims
            };
        }

        /// <summary>
        /// Converts to AuthorizationContext
        /// </summary>
        public Core.Authorization.AuthorizationContext ToAuthorizationContext()
        {
            return new Core.Authorization.AuthorizationContext
            {
                UserId = UserId,
                Username = Username,
                Roles = Roles,
                Permissions = Permissions,
                TenantId = TenantId,
                IsAuthenticated = IsAuthenticated,
                Claims = Claims
            };
        }
    }
}
