namespace Beep.OilandGas.UserManagement.Core.Authorization
{
    /// <summary>
    /// Platform-agnostic permission requirement (no ASP.NET Core dependency)
    /// </summary>
    public class PermissionRequirement
    {
        /// <summary>
        /// Gets the permission code required
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Gets the optional resource ID this permission applies to
        /// </summary>
        public string? ResourceId { get; }

        /// <summary>
        /// Gets the required access level (Read, Write, Delete, etc.)
        /// </summary>
        public string? RequiredAccessLevel { get; }

        /// <summary>
        /// Initializes a new instance of PermissionRequirement
        /// </summary>
        /// <param name="permission">The permission code required</param>
        /// <param name="resourceId">Optional resource ID</param>
        /// <param name="requiredAccessLevel">Optional access level</param>
        public PermissionRequirement(string permission, string? resourceId = null, string? requiredAccessLevel = null)
        {
            Permission = permission ?? throw new ArgumentNullException(nameof(permission));
            ResourceId = resourceId;
            RequiredAccessLevel = requiredAccessLevel;
        }
    }
}
