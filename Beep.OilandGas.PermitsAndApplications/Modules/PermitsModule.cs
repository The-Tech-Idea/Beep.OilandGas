using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Data.PermitTables;
using TheTechIdea.Beep.Report;

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
            typeof(R_PERMITS_REFERENCE_CODE),
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
            return SeedReferenceCodesAsync(connectionName, userId, cancellationToken, result);
        }

        private async Task<ModuleSetupResult> SeedReferenceCodesAsync(
            string connectionName,
            string userId,
            CancellationToken cancellationToken,
            ModuleSetupResult result)
        {
            var repo = GetRepo<R_PERMITS_REFERENCE_CODE>("R_PERMITS_REFERENCE_CODE", connectionName);

            foreach (var row in PermitsReferenceCodeSeed.GetAllSeedRows())
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

                var seedRow = new R_PERMITS_REFERENCE_CODE
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
