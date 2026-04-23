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
    /// Module order 70 — declares Production entity types for schema migration.
    /// </summary>
    public sealed class ProductionModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(PDEN),
            typeof(PDEN_VOL_SUMMARY),
            typeof(POOL),
            typeof(WELL_TEST)
        };

        public ProductionModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId   => "PRODUCTION";
        public override string ModuleName => "Production Accounting";
        public override int    Order      => 70;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Production module.";
            return Task.FromResult(result);
        }
    }
}
