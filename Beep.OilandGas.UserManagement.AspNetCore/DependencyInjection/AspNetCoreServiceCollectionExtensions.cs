using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Beep.OilandGas.UserManagement.Core.Authorization;
using Beep.OilandGas.UserManagement.Core.Authentication;
using Beep.OilandGas.UserManagement.AspNetCore.Authorization;
using Beep.OilandGas.UserManagement.AspNetCore.Authentication;
using Beep.OilandGas.UserManagement.Services;
using Beep.OilandGas.Models.Core.Interfaces.Security;

namespace Beep.OilandGas.UserManagement.AspNetCore.DependencyInjection
{
    /// <summary>
    /// ASP.NET Core-specific dependency injection extensions
    /// </summary>
    public static class AspNetCoreServiceCollectionExtensions
    {
        /// <summary>
        /// Adds ASP.NET Core integration for UserManagement
        /// This should be called after AddUserManagement() from the core library
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserManagementAspNetCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Register ASP.NET Core authorization handler
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // Register ICurrentUserProvider for ASP.NET Core (if not already registered)
            if (!services.Any(s => s.ServiceType == typeof(ICurrentUserProvider)))
            {
                services.AddScoped<ICurrentUserProvider, AspNetCoreCurrentUserProvider>();
            }

            // Register IUserManagementService facade (only if all dependencies are registered)
            // This allows the facade to be used when AddUserManagement(connectionName) was called
            services.TryAddScoped<IUserManagementService>(sp =>
            {
                // Check if all required services are registered
                var userService = sp.GetService<IUserService>();
                var roleService = sp.GetService<IRoleService>();
                var permissionService = sp.GetService<IPermissionService>();
                var authService = sp.GetService<IAuthService>();
                var currentUserProvider = sp.GetService<ICurrentUserProvider>();

                if (userService == null || roleService == null || permissionService == null || 
                    authService == null || currentUserProvider == null)
                {
                    throw new InvalidOperationException(
                        "IUserManagementService requires IUserService, IRoleService, IPermissionService, " +
                        "IAuthService, and ICurrentUserProvider to be registered. " +
                        "Call AddUserManagement(connectionName) before AddUserManagementAspNetCore().");
                }

                return new UserManagementService(userService, roleService, permissionService, authService, currentUserProvider);
            });

            return services;
        }

        /// <summary>
        /// Adds a permission-based authorization policy
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="policyName">The policy name</param>
        /// <param name="permission">The required permission code</param>
        /// <param name="resourceId">Optional resource ID</param>
        /// <param name="requiredAccessLevel">Optional access level</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddPermissionPolicy(
            this IServiceCollection services,
            string policyName,
            string permission,
            string? resourceId = null,
            string? requiredAccessLevel = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(policyName))
            {
                throw new ArgumentException("Policy name cannot be null or empty", nameof(policyName));
            }

            if (string.IsNullOrWhiteSpace(permission))
            {
                throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
            }

            services.Configure<AuthorizationOptions>(options =>
            {
                options.AddPolicy(policyName, policy =>
                {
                    policy.Requirements.Add(new AspNetCorePermissionRequirement(permission, resourceId, requiredAccessLevel));
                });
            });

            return services;
        }

        /// <summary>
        /// Adds multiple permission-based authorization policies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="policies">Dictionary of policy names to permission codes</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddPermissionPolicies(
            this IServiceCollection services,
            Dictionary<string, string> policies)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (policies == null)
            {
                throw new ArgumentNullException(nameof(policies));
            }

            foreach (var policy in policies)
            {
                services.AddPermissionPolicy(policy.Key, policy.Value);
            }

            return services;
        }
    }
}
