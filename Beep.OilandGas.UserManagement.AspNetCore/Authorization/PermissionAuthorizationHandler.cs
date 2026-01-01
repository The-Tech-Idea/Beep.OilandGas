using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.UserManagement.Core.Authorization;
using Beep.OilandGas.UserManagement.Core.Authentication;

namespace Beep.OilandGas.UserManagement.AspNetCore.Authorization
{
    /// <summary>
    /// ASP.NET Core authorization handler that adapts IPermissionEvaluator to ASP.NET Core's authorization pipeline
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<AspNetCorePermissionRequirement>
    {
        private readonly IPermissionEvaluator _permissionEvaluator;

        public PermissionAuthorizationHandler(IPermissionEvaluator permissionEvaluator)
        {
            _permissionEvaluator = permissionEvaluator ?? throw new ArgumentNullException(nameof(permissionEvaluator));
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AspNetCorePermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return;
            }

            // Convert ClaimsPrincipal to UserPrincipal
            var userPrincipal = UserPrincipal.FromClaimsPrincipal(context.User);
            
            if (!userPrincipal.IsAuthenticated || string.IsNullOrWhiteSpace(userPrincipal.UserId))
            {
                context.Fail();
                return;
            }

            // Check permission using the platform-agnostic evaluator
            var result = await _permissionEvaluator.EvaluateAsync(userPrincipal.UserId, requirement.Permission);

            if (result.IsAuthorized)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}
