using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.DevelopmentPlanning.Constants;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using TheTechIdea.Beep.Report;

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
            typeof(WELL_MAINTENANCE_PLAN),
            typeof(WELL_SERVICE_JOB),
            typeof(R_DEVELOPMENT_PLANNING_REFERENCE_CODE),
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
            return SeedReferenceCodesAsync(connectionName, userId, result, cancellationToken);
        }

        private async Task<ModuleSetupResult> SeedReferenceCodesAsync(
            string connectionName,
            string userId,
            ModuleSetupResult result,
            CancellationToken cancellationToken)
        {
            var repo = GetRepo<R_DEVELOPMENT_PLANNING_REFERENCE_CODE>("R_DEVELOPMENT_PLANNING_REFERENCE_CODE", connectionName);
            foreach (var row in DevelopmentPlanningReferenceCodeSeed.GetAllSeedRows())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entity = new R_DEVELOPMENT_PLANNING_REFERENCE_CODE
                {
                    REFERENCE_SET = row.ReferenceSet,
                    REFERENCE_CODE = row.ReferenceCode,
                    LONG_NAME = row.LongName,
                    ACTIVE_IND = row.ActiveInd,
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };

                var existing = await repo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "REFERENCE_SET", Operator = "=", FilterValue = row.ReferenceSet },
                    new AppFilter { FieldName = "REFERENCE_CODE", Operator = "=", FilterValue = row.ReferenceCode }
                });
                var hasExisting = false;
                foreach (var _ in existing)
                {
                    hasExisting = true;
                    break;
                }

                if (hasExisting)
                    continue;

                await TryInsertAsync(repo, entity, userId, result, $"{row.ReferenceSet}/{row.ReferenceCode}");
            }

            result.Success = result.Errors.Count == 0;
            if (result.RecordsInserted == 0 && result.Errors.Count == 0)
                result.SkipReason = "Reference codes already seeded.";
            return result;
        }
    }
}
