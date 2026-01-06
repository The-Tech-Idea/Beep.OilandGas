using Microsoft.AspNetCore.Authorization;

namespace Beep.OilandGas.UserManagement.Security
{
    /// <summary>
    /// Represents a permission requirement for authorization policies
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the permission code required
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Initializes a new instance of PermissionRequirement
        /// </summary>
        /// <param name="permission">The permission code that must be satisfied</param>
        public PermissionRequirement(string permission)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        }
    }
}
