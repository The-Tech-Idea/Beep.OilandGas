using System;
using System.Threading.Tasks;
using Beep.OilandGas.CompressorAnalysis.Constants;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PipelineAnalysis.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        public async Task<CompressorAnalysisResult> PerformCompressorAnalysisAsync(CompressorAnalysisRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger?.LogInformation("Performing Compressor Analysis for FacilityId: {FacilityId}", request.FacilityId);
            var effectiveCompressorType = EffectiveCompressorType(request);
            var effectiveAnalysisType = EffectiveAnalysisType(request);
            var result = new CompressorAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                FacilityId = request.FacilityId,
                AnalysisType = effectiveAnalysisType ?? CompressorAnalysisWellKnown.AnalysisType.Power,
                CompressorType = effectiveCompressorType ?? CompressorAnalysisWellKnown.CompressorType.Centrifugal,
                CalculationDate = DateTime.UtcNow,
                Status = CalculationRunStatus.Success,
                UserId = request.UserId
            };

            try
            {
                var operatingConditions = BuildOperatingConditionsForFacilityAnalysis(request);
                var analysisKey = (effectiveAnalysisType ?? CompressorAnalysisWellKnown.AnalysisType.Power).Trim().ToUpperInvariant();

                // PRESSURE = iterative discharge capability within HP cap (legacy packaged behaviour).
                // POWER / EFFICIENCY / default = full centrifugal or reciprocating power path (aligned with CompressorController).
                if (analysisKey == CompressorAnalysisWellKnown.AnalysisType.Pressure)
                {
                    var maxHp = request.AdditionalParameters?.MaxDriverHorsepower ?? 1000m;
                    var pressureResult = await _compressorAnalysisService.CalculateRequiredPressureAsync(
                        operatingConditions,
                        operatingConditions.GAS_FLOW_RATE,
                        maxPower: maxHp,
                        compressorEfficiency: operatingConditions.COMPRESSOR_EFFICIENCY);

                    ApplyCompressorPressureResult(result, pressureResult, operatingConditions);
                }
                else
                {
                    var powerResult = await CalculateFacilityCompressorPowerAsync(request, operatingConditions);
                    ApplyCompressorPowerResult(result, powerResult, operatingConditions);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Compressor Analysis failed for FacilityId: {FacilityId}", request.FacilityId);
                result.Status = CalculationRunStatus.Failed;
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        private static COMPRESSOR_OPERATING_CONDITIONS BuildOperatingConditionsForFacilityAnalysis(CompressorAnalysisRequest request)
        {
            var gs = request.GasSpecificGravity ?? 0.65m;
            var suctionR = request.SuctionTemperature ?? 520m;
            return new COMPRESSOR_OPERATING_CONDITIONS
            {
                COMPRESSOR_OPERATING_CONDITIONS_ID = Guid.NewGuid().ToString("N"),
                SUCTION_PRESSURE = request.SuctionPressure ?? 100m,
                DISCHARGE_PRESSURE = request.DischargePressure ?? 500m,
                SUCTION_TEMPERATURE = suctionR,
                DISCHARGE_TEMPERATURE = suctionR,
                GAS_FLOW_RATE = request.FlowRate ?? 5000m,
                GAS_SPECIFIC_GRAVITY = gs,
                GAS_MOLECULAR_WEIGHT = gs * CompressorConstants.AirMolecularWeight,
                COMPRESSOR_EFFICIENCY = request.PolytropicEfficiency ?? CompressorConstants.StandardPolytropicEfficiency,
                MECHANICAL_EFFICIENCY = CompressorConstants.StandardMECHANICAL_EFFICIENCY
            };
        }

        private async Task<COMPRESSOR_POWER_RESULT> CalculateFacilityCompressorPowerAsync(
            CompressorAnalysisRequest request,
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions)
        {
            if (IsReciprocatingCompressorType(EffectiveCompressorType(request)))
            {
                var recip = BuildReciprocatingPropertiesForFacilityAnalysis(operatingConditions, request);
                return await _compressorAnalysisService.CalculateReciprocatingPowerAsync(recip);
            }

            var centrifugal = BuildCentrifugalPropertiesForFacilityAnalysis(operatingConditions, request);
            return await _compressorAnalysisService.CalculateCentrifugalPowerAsync(centrifugal);
        }

        private static string? EffectiveCompressorType(CompressorAnalysisRequest request) =>
            string.IsNullOrWhiteSpace(request.CompressorType)
                ? request.AdditionalParameters?.CompressorType
                : request.CompressorType;

        private static string? EffectiveAnalysisType(CompressorAnalysisRequest request) =>
            string.IsNullOrWhiteSpace(request.AnalysisType)
                ? request.AdditionalParameters?.AnalysisType
                : request.AnalysisType;

        private static bool IsReciprocatingCompressorType(string? compressorType)
        {
            if (string.IsNullOrWhiteSpace(compressorType))
                return false;
            var t = compressorType.Trim();
            return t.Contains("RECIP", StringComparison.OrdinalIgnoreCase)
                   || t.Equals(CompressorAnalysisWellKnown.CompressorType.Reciprocating, StringComparison.OrdinalIgnoreCase);
        }

        private static CENTRIFUGAL_COMPRESSOR_PROPERTIES BuildCentrifugalPropertiesForFacilityAnalysis(
            COMPRESSOR_OPERATING_CONDITIONS oc,
            CompressorAnalysisRequest request)
        {
            var poly = request.PolytropicEfficiency ?? oc.COMPRESSOR_EFFICIENCY;
            var opts = request.AdditionalParameters;
            var speed = opts?.Speed ?? 10000m;
            return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                CENTRIFUGAL_COMPRESSOR_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                COMPRESSOR_OPERATING_CONDITIONS_ID = oc.COMPRESSOR_OPERATING_CONDITIONS_ID,
                OPERATING_CONDITIONS = oc,
                POLYTROPIC_EFFICIENCY = poly,
                SPECIFIC_HEAT_RATIO = CompressorConstants.StandardSpecificHeatRatio,
                NUMBER_OF_STAGES = request.NumberOfStages ?? 1,
                SPEED = speed
            };
        }

        private static RECIPROCATING_COMPRESSOR_PROPERTIES BuildReciprocatingPropertiesForFacilityAnalysis(
            COMPRESSOR_OPERATING_CONDITIONS oc,
            CompressorAnalysisRequest request)
        {
            var opts = request.AdditionalParameters;
            return new RECIPROCATING_COMPRESSOR_PROPERTIES
            {
                RECIPROCATING_COMPRESSOR_PROPERTIES_ID = Guid.NewGuid().ToString("N"),
                COMPRESSOR_OPERATING_CONDITIONS_ID = oc.COMPRESSOR_OPERATING_CONDITIONS_ID,
                OPERATING_CONDITIONS = oc,
                CYLINDER_DIAMETER = opts?.CylinderDiameter ?? 10m,
                STROKE_LENGTH = opts?.StrokeLength ?? 12m,
                ROTATIONAL_SPEED = opts?.RotationalSpeed ?? 300m,
                NUMBER_OF_CYLINDERS = request.NumberOfCylinders ?? 2,
                VOLUMETRIC_EFFICIENCY = request.VolumetricEfficiency ?? CompressorConstants.StandardVolumetricEfficiency,
                CLEARANCE_FACTOR = CompressorConstants.StandardClearanceFactor
            };
        }

        private static void ApplyCompressorPressureResult(
            CompressorAnalysisResult result,
            COMPRESSOR_PRESSURE_RESULT pressure,
            COMPRESSOR_OPERATING_CONDITIONS oc)
        {
            result.SuctionPressure = oc.SUCTION_PRESSURE;
            result.DischargePressure = pressure.REQUIRED_DISCHARGE_PRESSURE ?? oc.DISCHARGE_PRESSURE;
            result.CompressionRatio = pressure.COMPRESSION_RATIO ?? (oc.DISCHARGE_PRESSURE / oc.SUCTION_PRESSURE);
            result.PowerRequired = pressure.REQUIRED_POWER ?? 0m;
            result.DischargeTemperature = pressure.DISCHARGE_TEMPERATURE ?? 200m;
            result.PolytropicEfficiency = oc.COMPRESSOR_EFFICIENCY;
            result.FlowRate = oc.GAS_FLOW_RATE;
        }

        private static void ApplyCompressorPowerResult(
            CompressorAnalysisResult result,
            COMPRESSOR_POWER_RESULT power,
            COMPRESSOR_OPERATING_CONDITIONS oc)
        {
            result.SuctionPressure = oc.SUCTION_PRESSURE;
            result.DischargePressure = oc.DISCHARGE_PRESSURE;
            result.FlowRate = oc.GAS_FLOW_RATE;
            result.PolytropicEfficiency = oc.COMPRESSOR_EFFICIENCY;
            result.PowerRequired = power.BRAKE_HORSEPOWER;
            result.PolytropicHead = power.POLYTROPIC_HEAD;
            result.AdiabaticHead = power.ADIABATIC_HEAD;
            result.DischargeTemperature = power.DISCHARGE_TEMPERATURE;
            result.CompressionRatio = power.COMPRESSION_RATIO;
            result.OverallEfficiency = power.OVERALL_EFFICIENCY;
        }

        public async Task<PIPELINE_ANALYSIS_RESULT> PerformPipelineAnalysisAsync(PipelineAnalysisRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger?.LogInformation("Performing Pipeline Analysis for PipelineId: {PipelineId}", request.PipelineId);
            var result = new PIPELINE_ANALYSIS_RESULT
            {
                CalculationId = Guid.NewGuid().ToString(),
                PipelineId = request.PipelineId,
                PipelineType = request.PipelineType ?? "GAS",
                AnalysisType = request.AnalysisType ?? "FLOW",
                CalculationDate = DateTime.UtcNow,
                Status = CalculationRunStatus.Success,
                UserId = request.UserId,
                Length = request.Length,
                Diameter = request.Diameter,
                InletPressure = request.InletPressure,
                OutletPressure = request.OutletPressure
            };

            try
            {
                // Build shared pipeline geometry
                var pipelineProperties = new PIPELINE_PROPERTIES
                {
                    DIAMETER = request.Diameter ?? 12m,
                    LENGTH = request.Length ?? 10000m,
                    ROUGHNESS = request.Roughness ?? 0.0018m,
                    ELEVATION_CHANGE = request.ElevationChange ?? 0m,
                    INLET_PRESSURE = request.InletPressure ?? 1000m,
                    OUTLET_PRESSURE = request.OutletPressure ?? 800m,
                    AVERAGE_TEMPERATURE = request.Temperature ?? 60m
                };

                bool isGas = string.IsNullOrWhiteSpace(request.PipelineType)
                             || !request.PipelineType.Equals("LIQUID", StringComparison.OrdinalIgnoreCase);

                if (isGas)
                {
                    var gasProps = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        GAS_FLOW_RATE = request.FlowRate ?? 10000m,
                        GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                        Z_FACTOR = request.ZFactor ?? 0.9m,
                        PIPELINE_PROPERTIES = pipelineProperties,
                        PIPELINE = pipelineProperties
                    };
                    var flowResult = PipelineFlowCalculator.CalculateGasFlow(gasProps);
                    result.FlowRate = flowResult.FLOW_RATE;
                    result.FLOW_RATE = flowResult.FLOW_RATE;
                    result.PressureDrop = flowResult.PRESSURE_DROP;
                    result.PRESSURE_DROP = flowResult.PRESSURE_DROP;
                    result.Velocity = flowResult.FLOW_VELOCITY;
                    result.VELOCITY = flowResult.FLOW_VELOCITY;
                    result.ReynoldsNumber = flowResult.REYNOLDS_NUMBER;
                    result.FrictionFactor = flowResult.FRICTION_FACTOR;
                    result.FlowRegime = flowResult.FLOW_REGIME ?? "TURBULENT";
                }
                else
                {
                    var liquidProps = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        LIQUID_FLOW_RATE = request.FlowRate ?? 10000m,
                        LIQUID_SPECIFIC_GRAVITY = (request.LiquidDensity ?? 53.0m) / 62.4m,
                        LIQUID_VISCOSITY = request.LiquidViscosity ?? 1.0m,
                        LIQUID_DENSITY = request.LiquidDensity ?? 53.0m,
                        PIPELINE_PROPERTIES = pipelineProperties,
                        PIPELINE = pipelineProperties
                    };
                    var flowResult = PipelineFlowCalculator.CalculateLiquidFlow(liquidProps);
                    result.FlowRate = flowResult.FLOW_RATE;
                    result.FLOW_RATE = flowResult.FLOW_RATE;
                    result.PressureDrop = flowResult.PRESSURE_DROP;
                    result.PRESSURE_DROP = flowResult.PRESSURE_DROP;
                    result.Velocity = flowResult.FLOW_VELOCITY;
                    result.VELOCITY = flowResult.FLOW_VELOCITY;
                    result.ReynoldsNumber = flowResult.REYNOLDS_NUMBER;
                    result.FrictionFactor = flowResult.FRICTION_FACTOR;
                    result.FlowRegime = flowResult.FLOW_REGIME ?? "TURBULENT";
                }

                result.OutletPressure = (result.InletPressure ?? 0m) - (result.PressureDrop ?? 0m);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Pipeline Analysis failed for PipelineId: {PipelineId}", request.PipelineId);
                result.Status = CalculationRunStatus.Failed;
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }
    }
}

