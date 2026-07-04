using Beep.OilandGas.PPDM39.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.UserManagement.Security;
using Beep.OilandGas.UserManagement.Services;
using Beep.OilandGas.UserManagement.Models.Identity;
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

            // Register RoleAssignmentService (Phase 4: with SoD conflict detection)
            services.AddScoped<IRoleAssignmentService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RoleAssignmentService>>();
                var sodDetector = sp.GetService<ISodConflictDetector>();

                return new RoleAssignmentService(editor, commonColumnHandler, defaults, metadata, connectionName, logger, sodDetector);
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
                var roleHierarchy = sp.GetService<IRoleHierarchyService>();
                var tempElevation = sp.GetService<ITempRoleElevationService>();
                var fieldAccess = sp.GetService<IFieldAccessService>();

                return new AuthService(editor, commonColumnHandler, defaults, metadata, connectionName,
                    logger, config, userService, roleHierarchy, tempElevation, fieldAccess);
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

            // ── Phase 1 RBAC Hardening ─────────────────────────────────────

            // Register RoleHierarchyService (singleton — hierarchy changes rarely)
            services.AddSingleton<IRoleHierarchyService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RoleHierarchyService>>();
                return new RoleHierarchyService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register TempRoleElevationService
            services.AddScoped<ITempRoleElevationService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TempRoleElevationService>>();
                return new TempRoleElevationService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register FieldAccessService
            services.AddScoped<IFieldAccessService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<FieldAccessService>>();
                return new FieldAccessService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
            });

            // Register FieldScopeAuthorizationHandler
            services.AddScoped<IAuthorizationHandler, FieldScopeAuthorizationHandler>();

            // Phase 4: SoD conflict detector
            services.AddScoped<ISodConflictDetector>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var sodEngine = sp.GetRequiredService<Beep.OilandGas.LifeCycle.Services.Processes.ISodEvaluationEngine>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<SodConflictDetector>>();
                return new SodConflictDetector(editor, commonColumnHandler, defaults, metadata, connectionName, sodEngine, logger);
            });

            // Phase 4: Audit chain service
            services.AddScoped<IAuditChainService>(sp =>
            {
                var editor = sp.GetRequiredService<IDMEEditor>();
                var commonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>();
                var defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>();
                var metadata = sp.GetRequiredService<IPPDMMetadataRepository>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuditChainService>>();
                return new AuditChainService(editor, commonColumnHandler, defaults, metadata, connectionName, logger);
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
