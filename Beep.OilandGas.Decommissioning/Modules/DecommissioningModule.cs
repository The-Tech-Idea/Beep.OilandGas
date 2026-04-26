using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.Decommissioning;

namespace Beep.OilandGas.Decommissioning.Modules
{
    /// <summary>
    /// Module order 100 — owns decommissioning schema registration and seeding.
    /// Declares project-local table classes so the setup orchestrator creates the
    /// decommissioning schema on the target connection.
    /// </summary>
    public sealed class DecommissioningModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(DECOMMISSIONING_STATUS),
            typeof(ABANDONMENT_STATUS),
        };

        public DecommissioningModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "DECOMMISSIONING";
        public override string ModuleName => "Decommissioning & Abandonment";
        public override int    Order      => 100;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Decommissioning module yet.";
            return Task.FromResult(result);
        }
    }
}
