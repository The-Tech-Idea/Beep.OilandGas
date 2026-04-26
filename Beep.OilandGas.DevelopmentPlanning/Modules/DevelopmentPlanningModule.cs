using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.DevelopmentPlanning.Modules
{
    /// <summary>
    /// Module order 61 — owns development planning schema registration and seeding.
    /// Declares project-local table classes so the setup orchestrator creates the
    /// planning schema on the target connection.
    ///
    /// Planning ownership lives here; execution ownership lives in
    /// DrillingAndConstruction/Modules/DevelopmentModule (ModuleId: DRILLING_EXECUTION).
    /// </summary>
    public sealed class DevelopmentPlanningModule : ModuleSetupBase
    {
        private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
        {
            typeof(DEVELOPMENT_COSTS),
            typeof(FIELD_DEVELOPMENT_PLAN),
            typeof(DEVELOPMENT_WELL_SCHEDULE),
            typeof(FACILITY_INVESTMENT),
        };

        public DevelopmentPlanningModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "DEVELOPMENT_PLANNING";
        public override string ModuleName => "Development Planning";
        public override int    Order      => 61;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            result.Success    = true;
            result.SkipReason = "No reference seed data defined for Development Planning module yet.";
            return Task.FromResult(result);
        }
    }
}
