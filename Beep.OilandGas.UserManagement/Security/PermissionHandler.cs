using Microsoft.AspNetCore.Authorization;

namespace Beep.OilandGas.UserManagement.Security
{
    /// <summary>
    /// Authorization handler for permission-based requirements.
    /// Checks JWT claims: "permission" (single), "permissions" (comma-separated),
    /// and "elevated_permissions" (comma-separated, from temporary role elevations).
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (requirement == null)
                throw new ArgumentNullException(nameof(requirement));

            if (context.User.Identity?.IsAuthenticated != true)
                return Task.CompletedTask;

            // Identity emits one claim per permission; every grant must be considered.
            if (context.User.HasClaim("permission", requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2. Check comma-separated permissions claim (includes inherited from role hierarchy)
            if (CheckPermissionsClaim(context, "permissions", requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 3. Check elevated permissions claim (from temporary role elevations)
            if (CheckPermissionsClaim(context, "elevated_permissions", requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private static bool CheckPermissionsClaim(
            AuthorizationHandlerContext context, string claimType, string requiredPermission)
        {
            var claim = context.User.FindFirst(claimType);
            if (claim == null) return false;

            var permissions = claim.Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return permissions.Contains(requiredPermission, StringComparer.OrdinalIgnoreCase);
        }
    }
}
