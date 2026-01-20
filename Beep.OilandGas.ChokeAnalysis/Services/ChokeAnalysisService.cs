using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Validation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ChokeAnalysis.Services
{
    /// <summary>
    /// Service implementation for choke flow analysis using industry-standard petroleum engineering methods.
    /// Provides comprehensive choke performance evaluation with engineering accuracy.
    /// </summary>
    public partial class ChokeAnalysisService : IChokeAnalysisService
    {
        private readonly ILogger<ChokeAnalysisService> _logger;

        public ChokeAnalysisService(ILogger<ChokeAnalysisService> logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculates gas flow rate through a downhole choke with enhanced accuracy.
        /// </summary>
        public async Task<ChokeFlowResult> CalculateDownholeChokeFlowAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating downhole choke flow for choke size: {ChokeDiameter} in", choke.ChokeDiameter);
                    
                    var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);
                    
                    _logger?.LogInformation("Choke flow calculation completed. Flow rate: {FlowRate} Mscf/day, Regime: {FlowRegime}", 
                        result.FlowRate, result.FlowRegime);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating downhole choke flow");
                    throw;
                }
            });
        }

        /// <summary>
        /// Calculates gas flow rate through an uphole choke.
        /// </summary>
        public async Task<ChokeFlowResult> CalculateUpholeChokeFlowAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating uphole choke flow for choke size: {ChokeDiameter} in", choke.ChokeDiameter);
                    
                    // Uphole calculations are similar to downhole but may have different operating conditions
                    var result = GasChokeCalculator.CalculateUpholeChokeFlow(choke, gasProperties);
                    
                    _logger?.LogInformation("Uphole choke flow calculation completed. Flow rate: {FlowRate} Mscf/day", result.FlowRate);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating uphole choke flow");
                    throw;
                }
            });
        }

        /// <summary>
        /// Calculates downstream pressure for a given flow rate and choke configuration.
        /// </summary>
        public async Task<decimal> CalculateDownstreamPressureAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            decimal flowRate)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating downstream pressure for flow rate: {FlowRate} Mscf/day", flowRate);
                    
                    var result = GasChokeCalculator.CalculateDownstreamPressure(choke, gasProperties, flowRate);
                    
                    _logger?.LogInformation("Downstream pressure calculation completed: {Pressure} psia", result);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating downstream pressure");
                    throw;
                }
            });
        }

        /// <summary>
        /// Calculates required choke size for specified flow conditions.
        /// </summary>
        public async Task<decimal> CalculateRequiredChokeSizeAsync(
            GasChokeProperties gasProperties,
            decimal flowRate)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating required choke size for flow rate: {FlowRate} Mscf/day", flowRate);
                    
                    var result = GasChokeCalculator.CalculateRequiredChokeSize(gasProperties, flowRate);
                    
                    _logger?.LogInformation("Required choke size calculation completed: {ChokeSize} in", result);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating required choke size");
                    throw;
                }
            });
        }

        /// <summary>
        /// Validates choke configuration parameters.
        /// </summary>
        public async Task<ChokeValidationResult> ValidateChokeConfigurationAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            return await Task.Run(() =>
            {
                var result = new ChokeValidationResult();
                var errors = new System.Collections.Generic.List<string>();
                var warnings = new System.Collections.Generic.List<string>();

                try
                {
                    _logger?.LogInformation("Validating choke configuration");

                    // Use existing validator for basic validation
                    ChokeAnalysis.Validation.ChokeValidator.ValidateChokeProperties(choke);
                    ChokeAnalysis.Validation.ChokeValidator.ValidateGasChokeProperties(gasProperties);

                    // Additional engineering validations
                    if (choke.ChokeDiameter > 1.5m)
                    {
                        warnings.Add("Large choke size detected. Consider flow control implications.");
                    }

                    if (gasProperties.UpstreamPressure > 10000m)
                    {
                        warnings.Add("High upstream pressure. Ensure choke rating is adequate.");
                    }

                    var pressureRatio = gasProperties.DownstreamPressure / gasProperties.UpstreamPressure;
                    if (pressureRatio < 0.3m)
                    {
                        warnings.Add("Very low downstream pressure. Verify critical flow conditions.");
                    }

                    if (Math.Abs(gasProperties.GasSpecificGravity - 0.65m) > 0.2m)
                    {
                        warnings.Add("Gas specific gravity outside typical range. Verify Z-factor correlations.");
                    }

                    result.IsValid = errors.Count == 0;
                    result.Errors = errors.ToArray();
                    result.Warnings = warnings.ToArray();

                    _logger?.LogInformation("Choke validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}", 
                        result.IsValid, result.Errors.Length, result.Warnings.Length);

                    return result;
                }
                catch (Exception ex)
                {
                    errors.Add($"Validation error: {ex.Message}");
                    result.IsValid = false;
                    result.Errors = errors.ToArray();
                    result.Warnings = warnings.ToArray();

                    _logger?.LogError(ex, "Error during choke validation");
                    return result;
                }
            });
        }

        /// <summary>
        /// Calculates choke performance characteristics across a range of conditions.
        /// </summary>
        public async Task<ChokePerformanceCurve[]> CalculatePerformanceCurveAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties,
            (decimal Min, decimal Max) pressureRange,
            int numberOfPoints = 20)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (numberOfPoints < 2)
                        numberOfPoints = 2;

                    _logger?.LogInformation("Calculating performance curve with {PointCount} points from {MinPressure} to {MaxPressure} psia", 
                        numberOfPoints, pressureRange.Min, pressureRange.Max);

                    var results = new ChokePerformanceCurve[numberOfPoints];
                    decimal pressureStep = (pressureRange.Max - pressureRange.Min) / (numberOfPoints - 1);

                    for (int i = 0; i < numberOfPoints; i++)
                    {
                        var testProperties = new GasChokeProperties
                        {
                            UpstreamPressure = gasProperties.UpstreamPressure,
                            DownstreamPressure = pressureRange.Min + (i * pressureStep),
                            Temperature = gasProperties.Temperature,
                            GasSpecificGravity = gasProperties.GasSpecificGravity,
                            ZFactor = gasProperties.ZFactor,
                            FlowRate = gasProperties.FlowRate
                        };

                        var flowResult = GasChokeCalculator.CalculateDownholeChokeFlow(choke, testProperties);

                        results[i] = new ChokePerformanceCurve
                        {
                            DownstreamPressure = testProperties.DownstreamPressure,
                            FlowRate = flowResult.FlowRate,
                            FlowRegime = flowResult.FlowRegime,
                            PressureRatio = flowResult.PressureRatio,
                            IsCriticalFlow = flowResult.PressureRatio < flowResult.CriticalPressureRatio
                        };
                    }

                    _logger?.LogInformation("Performance curve calculation completed for {PointCount} points", numberOfPoints);
                    
                    return results;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating performance curve");
                    throw;
                }
            });
        }

        /// <summary>
        /// Provides engineering recommendations for choke optimization.
        /// </summary>
        public async Task<string[]> GetOptimizationRecommendationsAsync(
            ChokeProperties choke,
            GasChokeProperties gasProperties)
        {
            return await Task.Run(() =>
            {
                var recommendations = new System.Collections.Generic.List<string>();

                try
                {
                    _logger?.LogInformation("Generating choke optimization recommendations");

                    var validation = ValidateChokeConfigurationAsync(choke, gasProperties).Result;
                    
                    // Add warnings as recommendations
                    foreach (var warning in validation.Warnings)
                    {
                        recommendations.Add($"RECOMMENDATION: {warning}");
                    }

                    // Flow regime specific recommendations
                    var flowResult = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);
                    
                    if (flowResult.FlowRegime == FlowRegime.Sonic)
                    {
                        recommendations.Add("Choke is in critical flow. Consider monitoring for erosion and wear.");
                    }
                    else
                    {
                        recommendations.Add("Subsonic flow detected. Downstream pressure changes will affect flow rate.");
                    }

                    if (flowResult.PressureRatio > 0.9m)
                    {
                        recommendations.Add("High pressure ratio. Flow is pressure-sensitive - consider flow control improvements.");
                    }

                    if (choke.DischargeCoefficient < 0.8m)
                    {
                        recommendations.Add("Low discharge coefficient. Consider choke cleaning or replacement.");
                    }

                    _logger?.LogInformation("Generated {Count} optimization recommendations", recommendations.Count);
                    
                    return recommendations.ToArray();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error generating optimization recommendations");
                    return new[] { $"Error generating recommendations: {ex.Message}" };
                }
            });
        }
    }
}