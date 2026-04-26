using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;

namespace Beep.OilandGas.PermitsAndApplications.Modules
{
    /// <summary>
    /// Module order 110 — owns permits and applications extension schema registration.
    /// PPDM39 foundation tables such as APPLIC_BA, APPLIC_DESC, APPLIC_REMARK,
    /// BA_PERMIT, FACILITY_LICENSE, WELL_PERMIT_TYPE, and APPLICATION_COMPONENT
    /// are installed by the required PPDM39 foundation module, not by this optional module.
    /// </summary>
    public sealed class PermitsModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            // ── Core permit extension table (owned by this module) ─────────────────────
            typeof(PERMIT_APPLICATION),
            // ── Supporting permit extension tables ─────────────────────────────────────
            typeof(PERMIT_STATUS_HISTORY),
            typeof(DRILLING_PERMIT_APPLICATION),
            typeof(ENVIRONMENTAL_PERMIT_APPLICATION),
            typeof(INJECTION_PERMIT_APPLICATION),
            typeof(JURISDICTION_REQUIREMENTS),
            typeof(MIT_RESULT),
            typeof(REQUIRED_FORM),
            typeof(APPLICATION_ATTACHMENT),
            // Note: All PPDM39 foundation tables (e.g., APPLICATION_COMPONENT, APPLIC_BA, etc.)
            // are registered by the PPDM39 foundation module, not here.
        };

        public PermitsModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "PERMITS";
        public override string ModuleName => "Permits & Applications";
        public override int    Order      => 110;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Permits module yet.";
            return Task.FromResult(result);
        }
    }
}
