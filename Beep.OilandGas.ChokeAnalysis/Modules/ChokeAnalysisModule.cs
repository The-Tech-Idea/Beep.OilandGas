using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ChokeAnalysis.Modules;

/// <summary>
/// Auto-discovered module for <b>project-specific</b> choke analysis tables and reference seeding.
/// <see cref="EntityTypes"/> lists only extension tables under <c>Beep.OilandGas.Models.Data.ChokeAnalysis</c> that are
/// not standard PPDM 3.9 (choke calc inputs/outputs and app reference list). Standard PPDM tables (for example
/// <c>EQUIPMENT</c>, <c>PDEN_FLOW_MEASUREMENT</c>, <c>WELL_TEST</c> choke fields) are not registered here — they are part of the core PPDM model.
/// Table DDL for these extension types is produced by PPDM/migration tooling from the entity classes — do not add feature DDL as hand-written SQL in the repo.
/// </summary>
/// <remarks>
/// <para><b>Why these tables are not replaced by a single PPDM table</b> (same behaviour end-to-end):</para>
/// <list type="bullet">
/// <item>
/// <description><c>CHOKE_PROPERTIES</c> — PPDM carries choke size/position on operational rows (e.g. <c>PDEN_FLOW_MEASUREMENT.CHOKE_SIZE</c>,
/// <c>WELL_TEST_FLOW_MEAS.SURFACE_CHOKE_DIAMETER</c>) and text on <c>WELL_TEST</c>; there is no standard PPDM entity that stores the engineering bundle
/// (bean diameter, discharge coefficient, throat area) used by the calculator as one persisted unit.</description>
/// </item>
/// <item>
/// <description><c>GAS_CHOKE_PROPERTIES</c> — boundary conditions (upstream/downstream pressure, T, Z, gas specific gravity) appear across production/test/wellhead contexts in PPDM;
/// there is no dedicated “gas choke boundary conditions” table matching this projection.</description>
/// </item>
/// <item>
/// <description><c>CHOKE_FLOW_RESULT</c> — correlation outputs (FLOW_RATE, FLOW_REGIME, PRESSURE_RATIO, CRITICAL_PRESSURE_RATIO) are not a first-class PPDM 3.9 table;
/// operational volumes/pressures live elsewhere (<c>PDEN_VOL_SUMMARY</c>, etc.), not as choke-correlation run results.</description>
/// </item>
/// <item>
/// <description><c>R_CHOKE_ANALYSIS_REFERENCE_CODE</c> — PPDM uses many narrow <c>R_*</c> reference lists; there is no standard <c>R_CHOKE_*</c> with this app’s
/// REFERENCE_SET / REFERENCE_CODE surface for correlation and regime picklists, so this extension supports LOV seeding for the choke feature.</description>
/// </item>
/// </list>
/// <para>Integrations should <b>map</b> to/from PPDM operational entities where appropriate; they do not replace these extension tables without a schema redesign.</para>
/// </remarks>
public sealed class ChokeAnalysisModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(CHOKE_PROPERTIES),
        typeof(GAS_CHOKE_PROPERTIES),
        typeof(CHOKE_FLOW_RESULT),
        typeof(R_CHOKE_ANALYSIS_REFERENCE_CODE)
    };

    public ChokeAnalysisModule(ModuleSetupContext context)
        : base(context) { }

    public override string ModuleId => "CHOKE_ANALYSIS";
    public override string ModuleName => "Choke Analysis";
    public override int Order => 77;
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
        var repo = GetRepo<R_CHOKE_ANALYSIS_REFERENCE_CODE>("R_CHOKE_ANALYSIS_REFERENCE_CODE", connectionName);
        foreach (var row in ChokeAnalysisReferenceCodeSeed.GetAllSeedRows())
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

            var entity = new R_CHOKE_ANALYSIS_REFERENCE_CODE
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
