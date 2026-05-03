using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Pumps;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.SuckerRodPumping.Calculations;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Constants;
using Beep.OilandGas.PlungerLift.Calculations;
using Beep.OilandGas.HydraulicPumps.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        public async Task<GasLiftAnalysisResult> PerformGasLiftAnalysisAsync(GasLiftAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Gas Lift Analysis for WellId: {WellId}", request.WellId);
            var result = new GasLiftAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                UserId = request.UserId,
                Status = "SUCCESS"
            };

            try
            {
                // Retrieve actual well depth from DB; fall back to request value or 8000 ft
                var wellDepth = (await GetWellTotalDepthAsync(request.WellId ?? string.Empty))
                                ?? request.WellDepth ?? 8000m;

                var wellProps = new GAS_LIFT_WELL_PROPERTIES
                {
                    WELL_UWI = request.WellId ?? string.Empty,
                    WELL_DEPTH = wellDepth,
                    WELLHEAD_PRESSURE = request.WellheadPressure ?? 200m,
                    BOTTOM_HOLE_PRESSURE = request.BottomHolePressure ?? 2500m,
                    GAS_OIL_RATIO = request.GasOilRatio ?? 500m,
                    DESIRED_PRODUCTION_RATE = request.DesiredProductionRate ?? 500m,
                    OIL_GRAVITY = request.OilGravity ?? 35m,
                    WATER_CUT = request.WaterCut ?? 0.3m,
                    WELLHEAD_TEMPERATURE = request.WellheadTemperature ?? 80m,
                    BOTTOM_HOLE_TEMPERATURE = request.BottomHoleTemperature ?? 200m,
                    GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                    TUBING_DIAMETER = (int)(request.TubingDiameter ?? 2.875m)
                };

                var analysisResult = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                    wellProps,
                    minGasInjectionRate: request.MinGasInjectionRate ?? 100m,
                    maxGasInjectionRate: request.MaxGasInjectionRate ?? 5000m,
                    numberOfPoints: request.NumberOfPoints ?? 50);

                result.OptimalGasInjectionRate = analysisResult.OPTIMAL_GAS_INJECTION_RATE;
                result.MaximumProductionRate = analysisResult.MAXIMUM_PRODUCTION_RATE;
                result.OptimalGasLiquidRatio = analysisResult.OPTIMAL_GAS_LIQUID_RATIO;
                result.TotalGasInjectionRate = analysisResult.OPTIMAL_GAS_INJECTION_RATE;
                result.ExpectedProductionRate = analysisResult.MAXIMUM_PRODUCTION_RATE;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Gas Lift Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<ChokeAnalysisResult> PerformChokeAnalysisAsync(ChokeAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Choke Analysis for WellId: {WellId}", request.WellId);
            var result = new ChokeAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                UserId = request.UserId,
                Status = "SUCCESS",
                ChokeDiameter = request.ChokeDiameter ?? 0.5m,
                ChokeType = request.ChokeType ?? "FIXED",
                DischargeCoefficient = request.DischargeCoefficient ?? 0.85m
            };

            try
            {
                var correlation = request.AdditionalParameters?.CorrelationMethod;
                var useMultiphaseOrchestration = ChokeAnalysisReferenceCodes.UseMultiphaseOrchestration(correlation);

                var diameterInches = request.ChokeDiameter ?? 0.5m;

                // Single-phase gas path (aligned with Beep.OilandGas.ChokeAnalysis / IChokeAnalysisService)
                if (!useMultiphaseOrchestration &&
                    request.UpstreamPressure is decimal pUp &&
                    request.DownstreamPressure is decimal pDn &&
                    pUp > 0m && pDn >= 0m && pDn < pUp)
                {
                    var chokeTypeStr = string.IsNullOrWhiteSpace(request.ChokeType) ? "BEAN" : request.ChokeType;
                    if (string.Equals(chokeTypeStr, "FIXED", StringComparison.OrdinalIgnoreCase))
                        chokeTypeStr = "BEAN";

                    var choke = new CHOKE_PROPERTIES
                    {
                        CHOKE_DIAMETER = diameterInches,
                        CHOKE_TYPE = chokeTypeStr,
                        DISCHARGE_COEFFICIENT = request.DischargeCoefficient ?? 0.85m,
                        CHOKE_AREA = 0m
                    };

                    var gas = new GAS_CHOKE_PROPERTIES
                    {
                        UPSTREAM_PRESSURE = pUp,
                        DOWNSTREAM_PRESSURE = pDn,
                        TEMPERATURE = request.Temperature ?? 520m,
                        GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                        Z_FACTOR = request.ZFactor ?? 0.9m
                    };

                    var chokeFlow = string.Equals(request.AnalysisType, "UPHOLE", StringComparison.OrdinalIgnoreCase)
                        ? await _chokeAnalysisService.CalculateUpholeChokeFlowAsync(choke, gas).ConfigureAwait(false)
                        : await _chokeAnalysisService.CalculateDownholeChokeFlowAsync(choke, gas).ConfigureAwait(false);

                    result.ChokeDiameter = choke.CHOKE_DIAMETER;
                    result.ChokeType = choke.CHOKE_TYPE;
                    result.DischargeCoefficient = choke.DISCHARGE_COEFFICIENT;
                    result.FlowRate = chokeFlow.FLOW_RATE;
                    result.UpstreamPressure = chokeFlow.UPSTREAM_PRESSURE;
                    result.DownstreamPressure = chokeFlow.DOWNSTREAM_PRESSURE;
                    result.PressureRatio = chokeFlow.PRESSURE_RATIO;
                    result.CriticalPressureRatio = chokeFlow.CRITICAL_PRESSURE_RATIO;
                    result.FlowRegime = chokeFlow.FLOW_REGIME ?? string.Empty;
                    return await Task.FromResult(result);
                }

                // Multiphase Gilbert-style fallback when correlation requests empirical path or full gas inputs unavailable
                var liquidRate = request.FlowRate ?? 500m;
                // Fall back to latest WELL_TEST GOR from PPDM; then use 500 scf/bbl industry default
                var ppdmGorRaw = !string.IsNullOrEmpty(request.WellId)
                    ? await GetGasOilRatioForWellAsync(request.WellId)
                    : null;
                var glr = ppdmGorRaw.HasValue ? (decimal)ppdmGorRaw.Value : 500m;

                var pressures = MultiphaseChokeCalculator.CalculatePressures(liquidRate, glr, diameterInches);

                var estimatedUpstream =
                    MultiphaseChokeCalculator.SelectCorrelationUpstreamPressure(pressures, correlation);

                result.FlowRate = liquidRate;
                result.UpstreamPressure = request.UpstreamPressure ?? estimatedUpstream;
                result.DownstreamPressure = request.DownstreamPressure ?? 0m;
                result.PressureRatio = result.UpstreamPressure > 0
                    ? result.DownstreamPressure / result.UpstreamPressure
                    : 0m;
                var criticalRatio = ResolveMultiphaseCriticalPressureRatio(request.AdditionalParameters?.CriticalPressureRatioOverride);
                result.CriticalPressureRatio = criticalRatio;
                // Same reference codes as single-phase gas / R_CHOKE_ANALYSIS_REFERENCE_CODE (simplified multiphase heuristic).
                result.FlowRegime = result.PressureRatio < criticalRatio
                    ? ChokeAnalysisReferenceCodes.RegimeSonic
                    : ChokeAnalysisReferenceCodes.RegimeSubsonic;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Choke Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<PumpAnalysisResult> PerformPumpAnalysisAsync(PumpAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Pump Analysis for WellId: {WellId}, FacilityId: {FacilityId}", request.WellId, request.FacilityId);
            var result = new PumpAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                FacilityId = request.FacilityId,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId
            };

            try
            {
                // Use HydraulicPumpCalculator to estimate efficiency
                var flowRate = request.FlowRate ?? 500m;
                var dischargeP = request.DischargePressure ?? 2000m;
                var hydraulicHP = HydraulicPumpCalculator.CalculateHydraulicHorsepower(flowRate, dischargeP);
                // Efficiency heuristic: hydraulic HP / (hydraulic HP + 20% friction)
                var estimatedEfficiency = hydraulicHP > 0 ? hydraulicHP / (hydraulicHP * 1.2m) : 0.85m;

                result.Efficiency = Math.Min(1.0m, estimatedEfficiency);
                result.FlowRate = flowRate;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Pump Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<SuckerRodAnalysisResult> PerformSuckerRodAnalysisAsync(SuckerRodAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Sucker Rod Analysis for WellId: {WellId}", request.WellId);
            var result = new SuckerRodAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                EquipmentId = request.EquipmentId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId
            };

            try
            {
                var wellDepth = (await GetWellTotalDepthAsync(request.WellId ?? string.Empty))
                                ?? request.WellDepth ?? 8000m;

                var systemProps = new SUCKER_ROD_SYSTEM_PROPERTIES
                {
                    WELL_DEPTH = wellDepth,
                    TUBING_DIAMETER = request.TubingDiameter ?? 2.875m,
                    PUMP_DIAMETER = request.PumpDiameter ?? 1.75m,
                    STROKE_LENGTH = request.StrokeLength ?? 48m,
                    STROKES_PER_MINUTE = request.StrokeRate ?? 8m,
                    OIL_GRAVITY = request.OilGravity ?? 35m,
                    WATER_CUT = request.WaterCut ?? 0.3m,
                    PUMP_EFFICIENCY = request.VolumetricEfficiency ?? 0.85m,
                    FLUID_LEVEL = request.FluidLevel ?? (wellDepth * 0.7m),
                    FLUID_DENSITY = request.FluidDensity ?? 8.33m,
                    PUMP_SETTING_DEPTH = request.PumpDepth ?? wellDepth
                };

                var productionRate = SuckerRodFlowRatePowerCalculator.CalculateProductionRate(systemProps);

                result.ProductionRate = productionRate;
                result.VolumetricEfficiency = systemProps.PUMP_EFFICIENCY;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Sucker Rod Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<PlungerLiftAnalysisResult> PerformPlungerLiftAnalysisAsync(PlungerLiftAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Plunger Lift Analysis for WellId: {WellId}", request.WellId);
            var result = new PlungerLiftAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                EquipmentId = request.EquipmentId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId
            };

            try
            {
                var wellDepth = (await GetWellTotalDepthAsync(request.WellId ?? string.Empty))
                                ?? request.WellDepth ?? 8000m;
                var gasDensity = request.GasSpecificGravity.HasValue ? request.GasSpecificGravity.Value * 0.0764m : 0.0497m; // lbm/ft³ approx
                const decimal liquidDensity = 53.0m; // water ~53 lbm/ft³ approx
                const decimal surfaceTension = 25.0m; // dynes/cm typical
                var liquidLoadBbl = request.LiquidProductionRate ?? 50m;
                var avgPressure = ((request.BottomHolePressure ?? 2500m) + (request.WellheadPressure ?? 200m)) / 2m;

                var criticalVelocity = PlungerLiftCalculator.CalculateCriticalVelocity_Turner(
                    gasDensity, liquidDensity, surfaceTension);
                var gasRequired = PlungerLiftCalculator.EstimateGasRequired(
                    wellDepth, liquidLoadBbl, avgPressure);
                var riseVelocity = PlungerLiftCalculator.EstimateRiseVelocity(avgPressure);
                var fallVelocity = PlungerLiftCalculator.EstimateFallVelocity("SOLID_BAR", false);

                result.CriticalVelocity = criticalVelocity;
                result.OptimalGasFlowRate = gasRequired;
                result.GasProduction = request.GasFlowRate ?? gasRequired;
                result.LiquidProduction = liquidLoadBbl;
                result.PlungerVelocity = riseVelocity;
                result.FallTime = wellDepth / (fallVelocity > 0 ? fallVelocity : 200m) / 60m; // minutes
                result.RiseTime = wellDepth / (riseVelocity > 0 ? riseVelocity : 800m) / 60m; // minutes
                result.ProductionRate = liquidLoadBbl;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Plunger Lift Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<HydraulicPumpAnalysisResult> PerformHydraulicPumpAnalysisAsync(HydraulicPumpAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Hydraulic Pump Analysis for WellId: {WellId}", request.WellId);
            var result = new HydraulicPumpAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                EquipmentId = request.EquipmentId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId
            };

            try
            {
                var powerFluidRate = request.PowerFluidRate ?? 500m;
                var powerFluidPressure = request.PowerFluidPressure ?? 3000m;
                var productionTarget = request.ProductionRate ?? 200m;
                var nozzle = request.NozzleSize ?? 0.25m;
                var throat = request.ThroatSize ?? 0.375m;
                var suctionPressure = request.SuctionPressure ?? 1000m;
                var dischargePressure = request.DischargePressure ?? 2500m;

                var hydraulicHP = HydraulicPumpCalculator.CalculateHydraulicHorsepower(powerFluidRate, powerFluidPressure);
                var (efficiency, areaRatio) = HydraulicPumpCalculator.CalculateJetPumpPerformance(
                    nozzle, throat, powerFluidPressure, suctionPressure, dischargePressure);
                var recommendedRate = HydraulicPumpCalculator.RecommendPowerFluidRate(productionTarget, efficiency);

                result.ProductionRate = productionTarget;
                result.PowerFluidRate = powerFluidRate;
                result.PowerFluidPressure = powerFluidPressure;
                result.DischargePressure = dischargePressure;
                result.SuctionPressure = suctionPressure;
                result.HydraulicEfficiency = efficiency;
                result.OverallEfficiency = efficiency;
                result.PowerRequired = hydraulicHP;
                result.RecommendedPowerFluidRate = recommendedRate;
                result.RecommendedNozzleSize = nozzle;
                result.RecommendedThroatSize = throat;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Hydraulic Pump Analysis failed for WellId: {WellId}", request.WellId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Critical pressure ratio for simplified multiphase regime labeling; optional override from <see cref="ChokeAnalysisOptions.CriticalPressureRatioOverride"/> when in (0,1).
        /// </summary>
        private static decimal ResolveMultiphaseCriticalPressureRatio(decimal? overrideValue)
        {
            if (overrideValue is decimal o && o > 0m && o < 1m)
                return o;
            return MultiphaseChokeCalculator.DefaultMultiphaseCriticalPressureRatio;
        }
    }
}

