using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.UserManagement.Core.Authorization;
using Beep.OilandGas.UserManagement.Core.DataAccess;
using Beep.OilandGas.UserManagement.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.UserManagement.DependencyInjection
{
    /// <summary>
    /// Platform-agnostic dependency injection extensions for UserManagement
    /// </summary>
    public static class UserManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Adds UserManagement services to the service collection
        /// Registers all core services automatically
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserManagement(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            // Register core authorization components
            services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();

            // Register AuthorizationService - implements IAuthorizationService from Models
            services.AddScoped<IAuthorizationService>(sp =>
            {
                var permissionEvaluator = sp.GetRequiredService<IPermissionEvaluator>();
                return new AuthorizationService(permissionEvaluator);
            });

            // Register data access control services
            services.AddScoped<IDataSourceAccessControl, DataSourceAccessControlService>();
            services.AddScoped<IRowLevelSecurityProvider, RowLevelSecurityService>();
            services.AddScoped<ISourceAccessControl, SourceAccessControlService>();
            services.AddScoped<IDataAccessFilterProvider, DataAccessFilterProvider>();

            // Note: IUserService, IRoleService, IPermissionService, IAuthService, and ICurrentUserProvider
            // must be registered separately by the consumer (or use AddUserManagement(connectionName) overload)

            return services;
        }

        /// <summary>
        /// Adds UserManagement services with data access implementations
        /// Registers all services including data access services with the specified connection name
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="connectionName">The connection name for the security database</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddUserManagement(this IServiceCollection services, string connectionName)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(connectionName))
            {
                throw new ArgumentException("Connection name cannot be null or empty", nameof(connectionName));
            }

            // Register core services first
            services.AddUserManagement();

            // Register data access services (internal services)
            services.AddScoped<IUserService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                return new UserService(editor, commonColumnHandler, defaults, metadata, connectionName);
            });

            services.AddScoped<IRoleService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                return new RoleService(editor, commonColumnHandler, defaults, metadata, connectionName);
            });

            services.AddScoped<IPermissionService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                return new PermissionService(editor, commonColumnHandler, defaults, metadata, connectionName);
            });

            services.AddScoped<IAuthService>(sp =>
            {
                var userService = sp.GetRequiredService<IUserService>();
                return new AuthService(userService);
            });

            // Register IUserManagementService facade (only if ICurrentUserProvider is registered)
            // This will be registered by AddUserManagementAspNetCore() or manually by consumer
            // We don't register it here to avoid circular dependency

            return services;
        }
    }
}
