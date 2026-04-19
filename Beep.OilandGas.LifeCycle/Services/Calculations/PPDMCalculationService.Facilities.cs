using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.PipelineAnalysis.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        public async Task<CompressorAnalysisResult> PerformCompressorAnalysisAsync(CompressorAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Compressor Analysis for FacilityId: {FacilityId}", request.FacilityId);
            var result = new CompressorAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                FacilityId = request.FacilityId,
                AnalysisType = request.AnalysisType,
                CompressorType = request.CompressorType ?? "CENTRIFUGAL",
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId
            };

            try
            {
                var operatingConditions = new COMPRESSOR_OPERATING_CONDITIONS
                {
                    SUCTION_PRESSURE = request.SuctionPressure ?? 100m,
                    DISCHARGE_PRESSURE = request.DischargePressure ?? 500m,
                    SUCTION_TEMPERATURE = request.SuctionTemperature ?? 60m,
                    GAS_FLOW_RATE = request.FlowRate ?? 5000m,
                    GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                    COMPRESSOR_EFFICIENCY = request.PolytropicEfficiency ?? 0.75m
                };

                var pressureResult = CompressorPressureCalculator.CalculateRequiredPressure(
                    operatingConditions,
                    requiredFlowRate: operatingConditions.GAS_FLOW_RATE,
                    COMPRESSOR_EFFICIENCY: operatingConditions.COMPRESSOR_EFFICIENCY);

                result.SuctionPressure = operatingConditions.SUCTION_PRESSURE;
                result.DischargePressure = pressureResult.REQUIRED_DISCHARGE_PRESSURE ?? operatingConditions.DISCHARGE_PRESSURE;
                result.CompressionRatio = pressureResult.COMPRESSION_RATIO ?? (operatingConditions.DISCHARGE_PRESSURE / operatingConditions.SUCTION_PRESSURE);
                result.PowerRequired = pressureResult.REQUIRED_POWER ?? 0m;
                result.DischargeTemperature = pressureResult.DISCHARGE_TEMPERATURE ?? 200m;
                result.PolytropicEfficiency = operatingConditions.COMPRESSOR_EFFICIENCY;
                result.FlowRate = operatingConditions.GAS_FLOW_RATE;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Compressor Analysis failed for FacilityId: {FacilityId}", request.FacilityId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }

        public async Task<PIPELINE_ANALYSIS_RESULT> PerformPipelineAnalysisAsync(PipelineAnalysisRequest request)
        {
            _logger?.LogInformation("Performing Pipeline Analysis for PipelineId: {PipelineId}", request.PipelineId);
            var result = new PIPELINE_ANALYSIS_RESULT
            {
                CalculationId = Guid.NewGuid().ToString(),
                PipelineId = request.PipelineId,
                PipelineType = request.PipelineType ?? "GAS",
                AnalysisType = request.AnalysisType ?? "FLOW",
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
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
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Pipeline Analysis failed for PipelineId: {PipelineId}", request.PipelineId);
                result.Status = "FAILED";
                result.ErrorMessage = ex.Message;
            }

            return await Task.FromResult(result);
        }
    }
}

