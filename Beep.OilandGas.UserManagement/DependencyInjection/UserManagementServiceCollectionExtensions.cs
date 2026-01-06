using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.UserManagement.Security;
using Beep.OilandGas.Models.Core.Interfaces;
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
            
            return services;
        }

        public static IServiceCollection AddPermissionPolicy(this IServiceCollection services, string policyName, string permission)
        {
            // TODO: Implement permission policy registration when AspNetCore.Authorization is available
            return services;
        }
    }
}
