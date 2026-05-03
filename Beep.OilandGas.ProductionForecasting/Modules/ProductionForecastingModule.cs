using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Constants;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionForecasting.Modules;

/// <summary>
/// Module order 75 — registers production forecasting entity types for schema setup and seeds
/// <c>R_PRODUCTION_FORECASTING_REFERENCE_CODE</c> from <see cref="ProductionForecastingReferenceCodeSeed"/>.
/// Physical DDL is produced by the PPDM39 setup / migration pipeline from these table classes, not from hand-maintained SQL files.
/// </summary>
public sealed class ProductionForecastingModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(PRODUCTION_FORECAST),
        typeof(PRODUCTION_FORECAST_POINT),
        typeof(R_PRODUCTION_FORECASTING_REFERENCE_CODE),
    };

    public ProductionForecastingModule(ModuleSetupContext context)
        : base(context) { }

    public override string ModuleId => "PRODUCTION_FORECASTING";
    public override string ModuleName => "Production Forecasting";
    public override int Order => 75;
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
        var repo = GetRepo<R_PRODUCTION_FORECASTING_REFERENCE_CODE>("R_PRODUCTION_FORECASTING_REFERENCE_CODE", connectionName);
        foreach (var row in ProductionForecastingReferenceCodeSeed.GetAllSeedRows())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entity = new R_PRODUCTION_FORECASTING_REFERENCE_CODE
            {
                REFERENCE_SET = row.ReferenceSet,
                REFERENCE_CODE = row.ReferenceCode,
                LONG_NAME = row.LongName,
                ACTIVE_IND = row.ActiveInd,
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow,
            };

            var existing = await repo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "REFERENCE_SET", Operator = "=", FilterValue = row.ReferenceSet },
                new AppFilter { FieldName = "REFERENCE_CODE", Operator = "=", FilterValue = row.ReferenceCode },
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
