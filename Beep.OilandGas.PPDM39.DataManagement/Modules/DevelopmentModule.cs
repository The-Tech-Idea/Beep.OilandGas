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
    /// Module order 60 — declares Development (Well lifecycle) entity types
    /// for schema migration. Seed data for this module consists of
    /// well-status facets (handled by <see cref="WellStatusFacetModule"/> at order 20)
    /// so this module only registers entity types.
    /// </summary>
    public sealed class DevelopmentModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(WELL),
            typeof(WELL_STATUS),
            typeof(WELL_XREF),
            typeof(WELL_ACTIVITY),
            typeof(WELL_EQUIPMENT),
            typeof(WELL_TUBULAR),
            typeof(WELL_TEST),
            typeof(WELL_ABANDONMENT),
            typeof(FACILITY)
        };

        public DevelopmentModule(ModuleSetupContext context) : base(context) { }

        public override string ModuleId   => "DEVELOPMENT";
        public override string ModuleName => "Development (Well Lifecycle)";
        public override int    Order      => 60;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "Well status reference data is seeded by WellStatusFacetModule (order 20).";
            return Task.FromResult(result);
        }
    }
}
