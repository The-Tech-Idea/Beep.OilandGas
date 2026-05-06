using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Profile;
using Beep.OilandGas.UserManagement.Models.Scope;

namespace Beep.OilandGas.UserManagement.Modules
{
    /// <summary>
    /// Module order 40 — seeds the security bootstrap:
    /// BUSINESS_ASSOCIATE, BA_ORGANIZATION, USER, ROLE, PERMISSION,
    /// USER_ROLE, ROLE_PERMISSION, PERSONA_DEFINITION, ORGANIZATION_SCOPE,
    /// USER_SCOPE_ASSIGNMENT, and USER_ASSET_ACCESS.
    ///
    /// Lives in the UserManagement project because it depends on
    /// <see cref="IDefaultSecuritySeedService"/>.
    /// Register as <c>IModuleSetup</c> in the ApiService DI setup.
    /// </summary>
    public sealed class SecurityModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            // ── Identity tables (PPDM security schema) ────────────────────────
            typeof(USER),
            typeof(ROLE),
            typeof(USER_ROLE),
            typeof(PERMISSION),
            typeof(ROLE_PERMISSION),
            // ── Scope tables ─────────────────────────────────────────────────
            typeof(UserScopeAssignment),
            typeof(OrganizationScope),
            typeof(UserAssetAccess),
            // ── Persona / profile tables ──────────────────────────────────────
            typeof(PersonaDefinition),
            typeof(PersonaViewPreference),
            typeof(UserPersonaProfile),
            // ── Audit tables ─────────────────────────────────────────────────
            typeof(Beep.OilandGas.UserManagement.Models.Audit.UserProfileAuditEvent),
            typeof(Beep.OilandGas.UserManagement.Models.Audit.UserAccessAuditEvent),
            typeof(Beep.OilandGas.UserManagement.Models.Audit.SetupWizardLog),
            typeof(Beep.OilandGas.UserManagement.Models.Audit.AuthorizationDecisionTrace),
            // ── MFA, Password History, Session Management ────────────────────
            typeof(Beep.OilandGas.UserManagement.Contracts.Services.UserMfaConfig),
            typeof(Beep.OilandGas.UserManagement.Contracts.Services.PasswordHistory),
            typeof(Beep.OilandGas.UserManagement.Contracts.Services.UserSession),
        };

        private readonly IDefaultSecuritySeedService _securitySeeder;

        public SecurityModule(ModuleSetupContext context, IDefaultSecuritySeedService securitySeeder)
            : base(context)
        {
            _securitySeeder = securitySeeder
                ?? throw new ArgumentNullException(nameof(securitySeeder));
        }

        public override string ModuleId   => "SECURITY";
        public override string ModuleName => "Security Bootstrap (BA, Users, Roles)";
        public override int    Order      => 40;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var secResult = await _securitySeeder.SeedDefaultsAsync(userId);

                result.Success         = secResult.Success;
                result.RecordsInserted = secResult.BusinessAssociatesInserted
                                       + secResult.BaOrganizationsInserted
                                       + secResult.UsersInserted
                                       + secResult.RolesInserted
                                       + secResult.PermissionsInserted
                                       + secResult.RolePermissionsInserted
                                       + secResult.UserRolesInserted
                                       + secResult.PersonasInserted
                                       + secResult.OrganizationScopesInserted
                                       + secResult.UserScopeAssignmentsInserted
                                       + secResult.UserAssetAccessInserted;
                result.TablesSeeded    = 11;

                foreach (var err in secResult.Errors)
                    result.Errors.Add(err);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                result.Success = false;
                result.Errors.Add(ex.Message);
            }

            return result;
        }
    }
}
