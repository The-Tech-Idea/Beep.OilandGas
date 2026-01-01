using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.UserManagement.Core.Authorization;

namespace Beep.OilandGas.UserManagement.AspNetCore.Authorization
{
    /// <summary>
    /// ASP.NET Core-specific permission requirement that implements IAuthorizationRequirement
    /// Wraps the platform-agnostic PermissionRequirement
    /// </summary>
    public class AspNetCorePermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// Gets the underlying platform-agnostic permission requirement
        /// </summary>
        public PermissionRequirement Requirement { get; }

        /// <summary>
        /// Gets the permission code required
        /// </summary>
        public string Permission => Requirement.Permission;

        /// <summary>
        /// Gets the optional resource ID this permission applies to
        /// </summary>
        public string? ResourceId => Requirement.ResourceId;

        /// <summary>
        /// Gets the required access level (Read, Write, Delete, etc.)
        /// </summary>
        public string? RequiredAccessLevel => Requirement.RequiredAccessLevel;

        /// <summary>
        /// Initializes a new instance of AspNetCorePermissionRequirement
        /// </summary>
        /// <param name="permission">The permission code required</param>
        /// <param name="resourceId">Optional resource ID</param>
        /// <param name="requiredAccessLevel">Optional access level</param>
        public AspNetCorePermissionRequirement(string permission, string? resourceId = null, string? requiredAccessLevel = null)
        {
            Requirement = new PermissionRequirement(permission, resourceId, requiredAccessLevel);
        }

        /// <summary>
        /// Initializes from a platform-agnostic PermissionRequirement
        /// </summary>
        public AspNetCorePermissionRequirement(PermissionRequirement requirement)
        {
            Requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
        }
    }
}
