#nullable enable
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Validation;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Core;

namespace Beep.OilandGas.ChokeAnalysis.Services
{
    /// <summary>
    /// Service implementation for choke flow analysis using industry-standard petroleum engineering methods.
    /// Provides comprehensive choke performance evaluation with engineering accuracy.
    /// </summary>
    public partial class ChokeAnalysisService : IChokeAnalysisService
    {
        private readonly ILogger<ChokeAnalysisService>? _logger;

        public ChokeAnalysisService(ILogger<ChokeAnalysisService>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculates gas flow rate through a downhole choke using PPDM entities.
        /// </summary>
        public async Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(
            WELL well,
            WELL_TEST_FLOW_MEAS? flowMeas = null,
            WELL_TUBULAR? tubing = null,
            WELL_PRESSURE? pressure = null)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    _logger?.LogInformation("Calculating downhole choke flow from PPDM entities for Well: {UWI}", well.UWI);

                    // 1. Construct CHOKE_PROPERTIES
                    var chokeDiameter = flowMeas != null && flowMeas.SURFACE_CHOKE_DIAMETER > 0 
                        ? (decimal)flowMeas.SURFACE_CHOKE_DIAMETER 
                        : 0.5m; // Default or throw? Using 0.5 as explicit default for now if missing.

                    var choke = new CHOKE_PROPERTIES
                    {
                        CHOKE_DIAMETER = chokeDiameter,
                        CHOKE_TYPE = ChokeType.Bean.ToString(), // Default converted to string
                        DISCHARGE_COEFFICIENT = ValueRetrievers.GetDefaultDischargeCoefficient(well)
                    };

                    // 2. Construct GAS_CHOKE_PROPERTIES
                    // We need to handle potential nulls or missing data gracefully or throw meaningful errors
                    // Using ValueRetrievers helpers which might throw InvalidOperationException if data is missing, which is good.
                    
                    var gasProperties = new GAS_CHOKE_PROPERTIES
                    {
                        // Using specific gravity from Well if available
                        GAS_SPECIFIC_GRAVITY = ValueRetrievers.GetGasSpecificGravityDecimal(well),
                         // Temperature at wellhead
                        TEMPERATURE = ValueRetrievers.GetWellheadTemperatureInRankine(well, pressure),
                         // Upstream Pressure (Wellhead Pressure)
                        UPSTREAM_PRESSURE = ValueRetrievers.GetWellheadPressureDecimal(well, pressure),
                         // Downstream Pressure (Estimated as 80% if not provided otherwise)
                        DOWNSTREAM_PRESSURE = ValueRetrievers.GetDownstreamPressure80Percent(well, pressure),
                        
                        Z_FACTOR = 0.9m, // Default or calculate?
                        FLOW_RATE = 0m // To be calculated
                    };

                    // 3. Call core calculation
                    return await CalculateDownholeChokeFlowAsync(choke, gasProperties);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error calculating downhole choke flow from PPDM entities");
                    throw;
                }
            });
        }

        /// <summary>
        /// Calculates gas flow rate through a downhole choke with enhanced accuracy.
        /// </summary>
        public async Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating downhole choke flow for choke size: {ChokeDiameter} in", choke.CHOKE_DIAMETER);
                    
                    var result = GasChokeCalculator.CalculateDownholeChokeFlow(choke, gasProperties);
                    
                    _logger?.LogInformation("Choke flow calculation completed. Flow rate: {FlowRate} Mscf/day, Regime: {FlowRegime}", 
                        result.FLOW_RATE, result.FLOW_REGIME);
                    
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
        public async Task<CHOKE_FLOW_RESULT> CalculateUpholeChokeFlowAsync(
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger?.LogInformation("Calculating uphole choke flow for choke size: {ChokeDiameter} in", choke.CHOKE_DIAMETER);
                    
                    // Uphole calculations are similar to downhole but may have different operating conditions
                    var result = GasChokeCalculator.CalculateUpholeChokeFlow(choke, gasProperties);
                    
                    _logger?.LogInformation("Uphole choke flow calculation completed. Flow rate: {FlowRate} Mscf/day", result.FLOW_RATE);
                    
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
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties,
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
            GAS_CHOKE_PROPERTIES gasProperties,
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
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties)
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
                    if (choke.CHOKE_DIAMETER > 1.5m)
                    {
                        warnings.Add("Large choke size detected. Consider flow control implications.");
                    }

                    if (gasProperties.UPSTREAM_PRESSURE > 10000m)
                    {
                        warnings.Add("High upstream pressure. Ensure choke rating is adequate.");
                    }

                    var pressureRatio = gasProperties.DOWNSTREAM_PRESSURE / gasProperties.UPSTREAM_PRESSURE;
                    if (pressureRatio < 0.3m)
                    {
                        warnings.Add("Very low downstream pressure. Verify critical flow conditions.");
                    }

                    if (Math.Abs(gasProperties.GAS_SPECIFIC_GRAVITY - 0.65m) > 0.2m)
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
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties,
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
                        var testProperties = new GAS_CHOKE_PROPERTIES
                        {
                            UPSTREAM_PRESSURE = gasProperties.UPSTREAM_PRESSURE,
                            DOWNSTREAM_PRESSURE = pressureRange.Min + (i * pressureStep),
                            TEMPERATURE = gasProperties.TEMPERATURE,
                            GAS_SPECIFIC_GRAVITY = gasProperties.GAS_SPECIFIC_GRAVITY,
                            Z_FACTOR = gasProperties.Z_FACTOR,
                            FLOW_RATE = gasProperties.FLOW_RATE
                        };

                        var flowResult = GasChokeCalculator.CalculateDownholeChokeFlow(choke, testProperties);

                        results[i] = new ChokePerformanceCurve
                        {
                            DownstreamPressure = testProperties.DOWNSTREAM_PRESSURE,
                            FlowRate = flowResult.FLOW_RATE,
                            FlowRegime = flowResult.FLOW_REGIME,
                            PressureRatio = flowResult.PRESSURE_RATIO,
                            IsCriticalFlow = flowResult.PRESSURE_RATIO < flowResult.CRITICAL_PRESSURE_RATIO
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
            CHOKE_PROPERTIES choke,
            GAS_CHOKE_PROPERTIES gasProperties)
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
                    
                    if (flowResult.FLOW_REGIME == FlowRegime.Sonic)
                    {
                        recommendations.Add("Choke is in critical flow. Consider monitoring for erosion and wear.");
                    }
                    else
                    {
                        recommendations.Add("Subsonic flow detected. Downstream pressure changes will affect flow rate.");
                    }

                    if (flowResult.PRESSURE_RATIO > 0.9m)
                    {
                        recommendations.Add("High pressure ratio. Flow is pressure-sensitive - consider flow control improvements.");
                    }

                    if (choke.DISCHARGE_COEFFICIENT < 0.8m)
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