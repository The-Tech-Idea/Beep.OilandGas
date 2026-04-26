using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.Lease;

namespace Beep.OilandGas.LeaseAcquisition.Modules
{
    /// <summary>
    /// Module order 45 — owns lease acquisition schema registration and seeding.
    /// Declares project-local table classes so the setup orchestrator creates the
    /// land/lease schema on the target connection.
    ///
    /// Order 45 places it after Security (40) and before Exploration (50) because
    /// lease rights are typically acquired before exploration begins.
    /// </summary>
    public sealed class LeaseAcquisitionModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(LEASE_ACQUISITION),
            typeof(FEE_MINERAL_LEASE),
            typeof(GOVERNMENT_LEASE),
            typeof(NET_PROFIT_LEASE),
        };

        public LeaseAcquisitionModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "LEASE_ACQUISITION";
        public override string ModuleName => "Lease Acquisition";
        public override int    Order      => 45;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Lease Acquisition module yet.";
            return Task.FromResult(result);
        }
    }
}
