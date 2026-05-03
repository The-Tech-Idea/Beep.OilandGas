using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Constants;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.NodalAnalysis.Modules;

/// <summary>
/// Auto-discovered module for nodal analysis schema entity registration and reference seed loading.
/// </summary>
public sealed class NodalAnalysisModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(NODAL_ANALYSIS_RESULT),
        typeof(NODAL_ANALYSIS_RUN_METADATA),
        typeof(NODAL_IPR_POINT),
        typeof(NODAL_VLP_POINT),
        typeof(NODAL_OPERATING_POINT),
        typeof(NODAL_RESERVOIR_PROPERTIES),
        typeof(NODAL_WELLBORE_PROPERTIES),
        typeof(R_NODAL_ANALYSIS_REFERENCE_CODE)
    };

    public NodalAnalysisModule(ModuleSetupContext context)
        : base(context) { }

    public override string ModuleId => "NODAL_ANALYSIS";
    public override string ModuleName => "Nodal Analysis";
    public override int Order => 76;
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
        var repo = GetRepo<R_NODAL_ANALYSIS_REFERENCE_CODE>("R_NODAL_ANALYSIS_REFERENCE_CODE", connectionName);
        foreach (var row in NodalAnalysisReferenceCodeSeed.GetAllSeedRows())
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

            var entity = new R_NODAL_ANALYSIS_REFERENCE_CODE
            {
                REFERENCE_SET = row.ReferenceSet,
                REFERENCE_CODE = row.ReferenceCode,
                LONG_NAME = row.LongName,
                ACTIVE_IND = row.ActiveInd,
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };

            await TryInsertAsync(repo, entity, userId, result, $"{row.ReferenceSet}/{row.ReferenceCode}");
        }

        result.Success = result.Errors.Count == 0;
        if (result.RecordsInserted == 0 && result.Errors.Count == 0)
            result.SkipReason = "Reference codes already seeded.";
        return result;
    }
}
