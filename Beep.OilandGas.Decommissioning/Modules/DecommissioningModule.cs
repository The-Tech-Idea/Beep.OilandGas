using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Decommissioning.Constants;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.Decommissioning;
using TheTechIdea.Beep.Report;

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
            typeof(R_DECOMMISSIONING_REFERENCE_CODE),
        };

        public DecommissioningModule(ModuleSetupContext context)
            : base(context) { }

        public override string ModuleId   => "DECOMMISSIONING";
        public override string ModuleName => "Decommissioning & Abandonment";
        public override int    Order      => 100;
        public override IReadOnlyList<Type> EntityTypes => _entityTypes;

        public override async Task<ModuleSetupResult> SeedAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken = default)
        {
            var result = NewResult();
            var repo = GetRepo<R_DECOMMISSIONING_REFERENCE_CODE>("R_DECOMMISSIONING_REFERENCE_CODE", connectionName);

            foreach (var row in DecommissioningReferenceCodeSeed.GetAllSeedRows())
            {
                cancellationToken.ThrowIfCancellationRequested();
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

                var seedRow = new R_DECOMMISSIONING_REFERENCE_CODE
                {
                    REFERENCE_SET = row.ReferenceSet,
                    REFERENCE_CODE = row.ReferenceCode,
                    LONG_NAME = row.LongName,
                    ACTIVE_IND = row.ActiveInd,
                    PPDM_GUID = Guid.NewGuid().ToString(),
                    ROW_CREATED_BY = userId,
                    ROW_CREATED_DATE = DateTime.UtcNow
                };
                await TryInsertAsync(repo, seedRow, userId, result, $"{row.ReferenceSet}/{row.ReferenceCode}");
            }

            result.Success = result.Errors.Count == 0;
            if (result.RecordsInserted == 0 && result.Errors.Count == 0)
                result.SkipReason = "Reference codes already seeded.";
            return result;
        }
    }
}
