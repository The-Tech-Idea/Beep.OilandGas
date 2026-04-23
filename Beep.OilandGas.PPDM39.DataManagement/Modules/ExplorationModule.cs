using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules
{
    /// <summary>
    /// Module order 50 — declares Exploration entity types for schema migration
    /// and seeds Exploration reference data.
    /// </summary>
    public sealed class ExplorationModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(PROSPECT)
        };

        public ExplorationModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId   => "EXPLORATION";
        public override string ModuleName => "Exploration";
        public override int    Order      => 50;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            // No reference data to seed for exploration at this time.
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Exploration module.";
            return Task.FromResult(result);
        }
    }
}
