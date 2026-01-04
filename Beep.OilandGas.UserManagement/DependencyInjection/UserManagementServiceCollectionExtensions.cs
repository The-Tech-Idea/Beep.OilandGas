using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.UserManagement.Security;
using Beep.OilandGas.Models.Core.Interfaces.Security;

namespace Beep.OilandGas.UserManagement.DependencyInjection
{
    public static class UserManagementServiceCollectionExtensions
    {
        public static IServiceCollection AddUserManagement(this IServiceCollection services)
        {
            // Register core services - implementations must be provided by the consumer
            // e.g., services.AddScoped<IUserService, UserService>();
            // e.g., services.AddScoped<IRoleService, RoleService>();
            // e.g., services.AddScoped<IPermissionService, PermissionService>();
            // e.g., services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthorizationService, Beep.OilandGas.Models.Core.Interfaces.Security.IAuthorizationService>();

            // Authorization handler
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            return services;
        }

        public static IServiceCollection AddPermissionPolicy(this IServiceCollection services, string policyName, string permission)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(policyName, policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            });

            return services;
        }
    }
}
