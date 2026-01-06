using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Beep.OilandGas.UserManagement.Security
{
    /// <summary>
    /// Authorization handler for permission-based requirements
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        /// <summary>
        /// Handles the permission requirement
        /// </summary>
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            // Check if user has the required permission in claims
            var permissionClaim = context.User.FindFirst("permission");
            if (permissionClaim != null && permissionClaim.Value == requirement.Permission)
            {
                context.Succeed(requirement);
            }
            else
            {
                // Check multiple permissions claim
                var permissionsClaim = context.User.FindFirst("permissions");
                if (permissionsClaim != null)
                {
                    var permissions = permissionsClaim.Value.Split(',');
                    if (permissions.Contains(requirement.Permission))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
