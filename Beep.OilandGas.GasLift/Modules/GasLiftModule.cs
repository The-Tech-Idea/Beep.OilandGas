using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Constants;
using Beep.OilandGas.GasLift.Data;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.GasLift.Modules;

/// <summary>
/// Feature module setup for gas lift: registers the extension LOV table and seeds reference rows.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why only one <see cref="EntityTypes"/> entry?</b>
/// The only gas-lift <b>extension</b> table class owned in this assembly is
/// <see cref="R_GAS_LIFT_REFERENCE_CODE"/> (<c>Beep.OilandGas.GasLift.Data</c> / <c>Data/Tables</c>).
/// It is <b>not</b> part of standard PPDM 3.9 core, so it belongs on <see cref="ModuleSetupBase.EntityTypes"/> for
/// metadata/schema tooling. Transactional wire types such as <c>GAS_LIFT_DESIGN</c>, <c>GAS_LIFT_WELL_PROPERTIES</c>,
/// <c>GAS_LIFT_POTENTIAL_RESULT</c>, etc. live in <c>Beep.OilandGas.Models</c> with the shared PPDM39 schema path;
/// they are intentionally <b>not</b> duplicated here (same pattern as other domains: module lists feature-specific
/// <c>R_*</c> / extension tables only — see root <c>CLAUDE.md</c>).
/// </para>
/// <para>
/// <b><see cref="SeedAsync"/></b> idempotently inserts rows from <see cref="GasLiftReferenceCodeSeed"/> into
/// <c>R_GAS_LIFT_REFERENCE_CODE</c>: standard port sizes (from <see cref="GasLiftConstants"/>), operating mode,
/// design method, valve service, injection gas source, and design-limit keys (aligned with
/// <c>GasLiftDesignLimitMessages</c> / validators). Skip when <c>REFERENCE_SET</c> + <c>REFERENCE_CODE</c> already exist.
/// </para>
/// <para>Physical DDL for <c>R_GAS_LIFT_REFERENCE_CODE</c> follows entity-driven tooling, not hand-written feature SQL.</para>
/// </remarks>
public sealed class GasLiftModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(R_GAS_LIFT_REFERENCE_CODE)
    };

    public GasLiftModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "GAS_LIFT";

    public override string ModuleName => "Gas lift (design, spacing, performance)";

    /// <summary>After flash/PVT (73), before production forecasting (75).</summary>
    public override int Order => 74;

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
        var repo = GetRepo<R_GAS_LIFT_REFERENCE_CODE>("R_GAS_LIFT_REFERENCE_CODE", connectionName);

        foreach (var row in GasLiftReferenceCodeSeed.GetAllSeedRows())
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

            var entity = new R_GAS_LIFT_REFERENCE_CODE
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
