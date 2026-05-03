using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.CompressorAnalysis.Modules;

/// <summary>
/// Registers extension compressor analysis tables (entity types in <c>Beep.OilandGas.CompressorAnalysis.Data</c>).
/// Physical DDL comes from entity-driven tooling — see root <c>CLAUDE.md</c> (*Schema for extension tables*).
/// </summary>
public sealed class CompressorAnalysisModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(COMPRESSOR_OPERATING_CONDITIONS),
        typeof(CENTRIFUGAL_COMPRESSOR_PROPERTIES),
        typeof(RECIPROCATING_COMPRESSOR_PROPERTIES),
        typeof(COMPRESSOR_POWER_RESULT),
        typeof(COMPRESSOR_PRESSURE_RESULT),
        typeof(R_COMPRESSOR_ANALYSIS_REFERENCE_CODE)
    };

    public CompressorAnalysisModule(ModuleSetupContext context)
        : base(context) { }

    public override string ModuleId => "COMPRESSOR_ANALYSIS";
    public override string ModuleName => "Compressor Analysis";
    public override int Order => 78;
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
        var repo = GetRepo<R_COMPRESSOR_ANALYSIS_REFERENCE_CODE>("R_COMPRESSOR_ANALYSIS_REFERENCE_CODE", connectionName);
        foreach (var row in CompressorAnalysisReferenceCodeSeed.GetAllSeedRows())
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

            var entity = new R_COMPRESSOR_ANALYSIS_REFERENCE_CODE
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
