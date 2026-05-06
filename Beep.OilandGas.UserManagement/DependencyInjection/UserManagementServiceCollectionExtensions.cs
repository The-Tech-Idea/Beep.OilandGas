using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.UserManagement.Security;
using Beep.OilandGas.UserManagement.Services;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.UserManagement.DependencyInjection
{
    public static class UserManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all UserManagement services with the DI container.
        /// Must be called after AddBeepServices (which provides IDMEEditor, ICommonColumnHandler, etc.).
        /// </summary>
        public static IServiceCollection AddUserManagement(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionName = configuration.GetValue("BeepOg:DatabaseConnectionName", "PPDM39");

            // Register UserManagementService as IUserService
            services.AddScoped<IUserService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<UserManagementService>>();

                return new UserManagementService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register RoleAssignmentService
            services.AddScoped<IRoleAssignmentService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RoleAssignmentService>>();

                return new RoleAssignmentService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register PersonaProfileService
            services.AddScoped<IPersonaProfileService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<PersonaProfileService>>();

                return new PersonaProfileService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register DefaultSecuritySeedService
            services.AddScoped<IDefaultSecuritySeedService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DefaultSecuritySeedService>>();

                return new DefaultSecuritySeedService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register AuthService
            services.AddScoped<Contracts.Services.IAuthService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuthService>>();
                var config = sp.GetRequiredService<IConfiguration>();
                var userService = sp.GetRequiredService<IUserService>();

                return new AuthService(editor, commonColumnHandler, defaults, metadata, connectionName, logger, config, userService);
            });

            // Register RowLevelSecurityService
            services.AddScoped<IRowLevelSecurityService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RowLevelSecurityService>>();
                var userService = sp.GetRequiredService<IUserService>();

                return new RowLevelSecurityService(editor, commonColumnHandler, defaults, metadata, connectionName, logger, userService);
            });

            return services;
        }

        public static IServiceCollection AddPermissionPolicy(this IServiceCollection services, string policyName, string permission)
        {
            services.AddAuthorization(options =>
                options.AddPolicy(policyName, policy =>
                    policy.RequireClaim("permission", permission)));
            return services;
        }
    }
}
