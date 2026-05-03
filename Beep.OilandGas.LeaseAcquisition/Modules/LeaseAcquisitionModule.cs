using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.LeaseAcquisition.Constants;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

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
            typeof(R_LEASE_REFERENCE_CODE),
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
            return SeedReferenceCodesAsync(connectionName, userId, cancellationToken);
        }

        private async Task<ModuleSetupResult> SeedReferenceCodesAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken)
        {
            var result = NewResult();
            var repo = GetRepo<R_LEASE_REFERENCE_CODE>("R_LEASE_REFERENCE_CODE", connectionName);

            foreach (var row in LeaseReferenceCodeSeed.GetAllSeedRows())
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

                var seedRow = new R_LEASE_REFERENCE_CODE
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
                result.SkipReason = "Lease reference codes already seeded.";

            return result;
        }
    }
}
