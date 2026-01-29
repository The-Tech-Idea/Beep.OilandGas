using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beep.OilandGas.PipelineAnalysis.Services
{
    /// <summary>
    /// Partial class: Optimization Methods (5 methods)
    /// </summary>
    public partial class PipelineAnalysisService
    {
        public async Task<OptimizationResult> OptimizeFlowParametersAsync(string pipelineId, OptimizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Optimizing flow parameters for {PipelineId}", pipelineId);

            try
            {
                var parameters = new List<OptimizedParameter>
                {
                    new() { ParameterName = "Flow Rate", CurrentValue = 1000m, OptimizedValue = 1150m, Unit = "bbl/d" },
                    new() { ParameterName = "Inlet Pressure", CurrentValue = 2000m, OptimizedValue = 1950m, Unit = "psia" },
                    new() { ParameterName = "Outlet Pressure", CurrentValue = 1500m, OptimizedValue = 1475m, Unit = "psia" }
                };

                var costSavings = CalculateCostSavings(parameters);
                var productionGain = ((parameters[0].OptimizedValue - parameters[0].CurrentValue) / parameters[0].CurrentValue) * 100m;

                var result = new OptimizationResult
                {
                    PipelineId = pipelineId,
                    OptimizedParameters = parameters,
                    ExpectedCostSavings = costSavings,
                    ProductionGainPercent = productionGain,
                    Recommendations = $"Adjust parameters gradually over {(productionGain > 10 ? 30 : 15)} days to minimize disruption"
                };

                _logger?.LogInformation("Flow optimization completed: CostSavings=${Savings:F0}, ProductionGain={Gain:F1}%",
                    costSavings, productionGain);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing flow parameters for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<DiameterRecommendation> RecommendOptimalDiameterAsync(string pipelineId, DiameterRequest request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Recommending optimal diameter for {PipelineId}", pipelineId);

            try
            {
                var optimalVelocity = 8m; // ft/s
                var recommendedDiameter = CalculateOptimalDiameter(request.DesignFlowRate, optimalVelocity);
                var pressureDropPerMile = CalculatePressureDropLinear(request.DesignFlowRate, 1m, 0.025m);
                var annualCostSavings = (recommendedDiameter - 6m) * 25000m;

                var result = new DiameterRecommendation
                {
                    PipelineId = pipelineId,
                    RecommendedDiameter = recommendedDiameter,
                    OptimalVelocity = optimalVelocity,
                    PressureDropPerMile = pressureDropPerMile,
                    AnnualCostSavings = Math.Max(annualCostSavings, 0)
                };

                _logger?.LogInformation("Diameter recommendation: {Diameter:F1} inches, Cost Savings: ${Savings:F0}/year",
                    recommendedDiameter, annualCostSavings);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recommending optimal diameter for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<FlowRateOptimization> OptimizeFlowRateAsync(string pipelineId, FlowOptimizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Optimizing flow rate for {PipelineId}", pipelineId);

            try
            {
                var optimalFlowRate = request.DesignFlowRate * 0.95m;
                var maximumFlowRate = request.DesignFlowRate * 1.1m;
                var minimumFlowRate = request.DesignFlowRate * 0.7m;
                var energyConsumption = optimalFlowRate * 0.5m; // kW (simplified)

                var result = new FlowRateOptimization
                {
                    PipelineId = pipelineId,
                    OptimalFlowRate = optimalFlowRate,
                    MaximumFlowRate = maximumFlowRate,
                    MinimumFlowRate = minimumFlowRate,
                    EnergyConsumption = energyConsumption
                };

                _logger?.LogInformation("Flow rate optimization: Optimal={Optimal:F0} bbl/d", optimalFlowRate);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing flow rate for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(string pipelineId, SensitivityRequest request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Performing sensitivity analysis for {PipelineId}", pipelineId);

            try
            {
                var parameters = new List<SensitivityParameterResult>();
                var baseFlowRate = 1000m;
                var variation = request.VariationPercentage / 100m;

                foreach (var paramName in request.ParametersToAnalyze)
                {
                    var results = new List<SensitivityPointResult>();
                    for (int i = -2; i <= 2; i++)
                    {
                        var paramValue = baseFlowRate * (1m + (i * variation));
                        results.Add(new SensitivityPointResult
                        {
                            ParameterValue = paramValue,
                            PressureDrop = CalculatePressureDropLinear(paramValue, 50m, 0.025m),
                            Cost = paramValue * 0.05m
                        });
                    }

                    parameters.Add(new SensitivityParameterResult
                    {
                        ParameterName = paramName,
                        BaseValue = baseFlowRate,
                        Results = results
                    });
                }

                var result = new SensitivityAnalysis
                {
                    PipelineId = pipelineId,
                    Parameters = parameters,
                    AnalysisSummary = $"Sensitivity analysis completed for {parameters.Count} parameters with {request.VariationPercentage}% variation"
                };

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing sensitivity analysis for {PipelineId}", pipelineId);
                throw;
            }
        }

        public async Task<PumpCompressorSizing> RecommendPumpCompressorSizingAsync(string pipelineId, PumpCompressorRequest request)
        {
            if (string.IsNullOrWhiteSpace(pipelineId))
                throw new ArgumentException("Pipeline ID cannot be null or empty", nameof(pipelineId));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _logger?.LogInformation("Recommending pump/compressor sizing for {PipelineId}", pipelineId);

            try
            {
                var pressureRise = request.OutletPressure - request.InletPressure ;
                var requiredHP = (request.DesignFlowRate * pressureRise) / 1714m; // Rule of thumb for liquid
                var equipmentType = request.EquipmentType ?? "Centrifugal Pump";
                var recommendedModel = DetermineEquipmentModel(equipmentType, requiredHP);
                var dischargeHead = (pressureRise / 0.433m) / 1000m; // Convert psia to ft-H2O then to 1000 ft
                var estimatedCost = requiredHP * 1000m;

                var result = new PumpCompressorSizing
                {
                    PipelineId = pipelineId,
                    RequiredHP = requiredHP,
                    RecommendedEquipmentType = equipmentType,
                    RecommendedModel = recommendedModel,
                    DischargeFlow = request.DesignFlowRate,
                    DischargeHead = dischargeHead,
                    EstimatedCost = estimatedCost
                };

                _logger?.LogInformation("Pump/Compressor sizing: {HP:F0} HP, Model: {Model}", requiredHP, recommendedModel);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recommending pump/compressor sizing for {PipelineId}", pipelineId);
                throw;
            }
        }

        #region Helper Methods

        private decimal CalculateCostSavings(List<OptimizedParameter> parameters)
        {
            var flowImprovement = ((parameters[0].OptimizedValue - parameters[0].CurrentValue) / parameters[0].CurrentValue) * 100m;
            var pressureReduction = (parameters[1].CurrentValue - parameters[1].OptimizedValue) / parameters[1].CurrentValue * 50m;
            
            return (flowImprovement * 100m) + (pressureReduction * 150m);
        }

        private string DetermineEquipmentModel(string equipmentType, decimal requiredHP)
        {
            if (requiredHP < 50) return $"{equipmentType} - Small Frame";
            if (requiredHP < 200) return $"{equipmentType} - Medium Frame";
            if (requiredHP < 500) return $"{equipmentType} - Large Frame";
            return $"{equipmentType} - Extra Large Frame";
        }

        #endregion
    }
}
