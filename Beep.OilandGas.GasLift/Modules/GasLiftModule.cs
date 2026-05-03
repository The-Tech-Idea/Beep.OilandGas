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
/// Registers extension gas lift reference data (<see cref="R_GAS_LIFT_REFERENCE_CODE"/>).
/// Physical DDL follows entity-driven tooling — see root <c>CLAUDE.md</c>.
/// </summary>
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
