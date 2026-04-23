using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.UserManagement.Modules
{
    /// <summary>
    /// Module order 40 — seeds the security bootstrap:
    /// BUSINESS_ASSOCIATE, BA_ORGANIZATION, USER, ROLE, PERMISSION,
    /// USER_ROLE, and ROLE_PERMISSION.
    ///
    /// Lives in the UserManagement project because it depends on
    /// <see cref="IDefaultSecuritySeedService"/>.
    /// Register as <c>IModuleSetup</c> in the ApiService DI setup.
    /// </summary>
    public sealed class SecurityModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(BUSINESS_ASSOCIATE),
            typeof(BA_ORGANIZATION),
            typeof(USER),
            typeof(ROLE),
            typeof(PERMISSION),
            typeof(USER_ROLE),
            typeof(ROLE_PERMISSION)
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
                                       + secResult.UserRolesInserted;
                result.TablesSeeded    = 7;

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
