using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Attributes
{
    /// <summary>
    /// Authorization attribute that requires a specific role
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _requiredRoles;

        public RequireRoleAttribute(params string[] requiredRoles)
        {
            _requiredRoles = requiredRoles ?? throw new ArgumentNullException(nameof(requiredRoles));
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get user ID from claims or context
            var userId = context.HttpContext.User?.Identity?.Name;
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get access control service from DI
            var accessControlService = context.HttpContext.RequestServices
                .GetService(typeof(Beep.OilandGas.Models.DTOs.IAccessControlService)) 
                as Beep.OilandGas.Models.DTOs.IAccessControlService;

            if (accessControlService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Get user's roles
            var userRoles = await accessControlService.GetUserRolesAsync(userId);

            // Check if user has any of the required roles
            bool hasRequiredRole = _requiredRoles.Any(role => userRoles.Contains(role));

            if (!hasRequiredRole)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
