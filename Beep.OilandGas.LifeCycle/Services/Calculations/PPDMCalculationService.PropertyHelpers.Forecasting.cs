using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.ProductionForecasting.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        private async Task<RESERVOIR_FORECAST_PROPERTIES?> GetReservoirPropertiesForForecastAsync(Beep.OilandGas.Models.Data.Calculations.DCARequest request)
        {
            if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId))
            {
                throw new ArgumentException("WellId or PoolId is required for physics-based forecasting");
            }

            ClearEntityCache();

            var poolId = request.PoolId ?? string.Empty;
            var wellId = request.WellId ?? string.Empty;
            
            var testId = request.AdditionalParameters?.TestId;
            DateTime? asOfDate = request.AdditionalParameters?.AsOfDate;

            var reservoirProps = new RESERVOIR_FORECAST_PROPERTIES
            {
                INITIAL_PRESSURE = await GetPoolInitialPressureAsync(poolId, 3000m),
                PERMEABILITY = await GetPoolPermeabilityAsync(poolId, 100m),
                THICKNESS = await GetPoolThicknessAsync(poolId, 50m),
                POROSITY = await GetPoolPorosityAsync(poolId, 0.2m),
                TEMPERATURE = await GetPoolTemperatureAsync(poolId, 560m),
                TOTAL_COMPRESSIBILITY = await GetPoolCompressibilityAsync(poolId, 0.00001m),
                DRAINAGE_RADIUS = await GetPoolDrainageRadiusAsync(poolId, 1000m),
                FORMATION_VOLUME_FACTOR = await GetPoolFormationVolumeFactorAsync(poolId, 1.2m),
                OIL_VISCOSITY = await GetPoolOilViscosityAsync(poolId, 1.0m),
                GAS_SPECIFIC_GRAVITY = await GetPoolGasGravityAsync(poolId, 0.65m),
                WELLBORE_RADIUS = 0.25m, // Default or mock
                SKIN_FACTOR = await GetWellTestSkinAsync(wellId, testId, asOfDate) ?? 0m
            };

            var testPerm = await GetWellTestPermeabilityAsync(wellId, testId, asOfDate);
            if (testPerm.HasValue)
                reservoirProps.PERMEABILITY = testPerm.Value;

            return reservoirProps;
        }

        private Beep.OilandGas.Models.Data.Calculations.DCAResult MapProductionForecastToDCAResult(
            Beep.OilandGas.Models.Data.ProductionForecasting.PRODUCTION_FORECAST forecast,
            Beep.OilandGas.Models.Data.Calculations.DCARequest request)
        {
            var result = new DCAResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                PoolId = request.PoolId,
                FieldId = request.FieldId,
                CalculationType = request.CalculationType,
                CalculationDate = DateTime.UtcNow,
                ProductionFluidType = request.ProductionFluidType ?? "OIL",
                Status = "SUCCESS",
                UserId = request.UserId,
                ForecastPoints = new List<DCAForecastPoint>(),
                AdditionalResults = new DcaAdditionalResults()
            };

            result.InitialRate = forecast.INITIAL_PRODUCTION_RATE;
            result.EstimatedEUR = forecast.TOTAL_CUMULATIVE_PRODUCTION;
            
            var startDate = request.StartDate ?? DateTime.UtcNow;
            if (forecast.FORECAST_POINTS != null)
            {
                foreach (var point in forecast.FORECAST_POINTS)
                {
                    result.ForecastPoints.Add(new DCAForecastPoint
                    {
                        Date = startDate.AddDays((double)point.TIME),
                        ProductionRate = point.PRODUCTION_RATE,
                        CumulativeProduction = point.CUMULATIVE_PRODUCTION
                    });
                }
            }

            result.AdditionalResults = new DcaAdditionalResults
            {
                ForecastType = forecast.FORECAST_TYPE.ToString(),
                ForecastDuration = forecast.FORECAST_DURATION,
                InitialProductionRate = forecast.INITIAL_PRODUCTION_RATE,
                FinalProductionRate = forecast.FINAL_PRODUCTION_RATE,
                TotalCumulativeProduction = forecast.TOTAL_CUMULATIVE_PRODUCTION,
                ForecastPointCount = forecast.FORECAST_POINTS?.Count ?? 0
            };

            return result;
        }
    }
}
