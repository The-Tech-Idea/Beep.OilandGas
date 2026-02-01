using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.DCA;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.DCA.Results;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        public async Task<Beep.OilandGas.Models.Data.Calculations.DCAResult> PerformDCAAnalysisAsync(Beep.OilandGas.Models.Data.Calculations.DCARequest request)
        {
            return await PerformDCAAnalysisAsync(request, null, null);
        }

        /// <summary>
        /// Perform DCA Analysis with progress tracking (internal overload)
        /// </summary>
        internal async Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request, string? operationId, object? progressTracking)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId) && string.IsNullOrEmpty(request.FieldId))
                {
                    throw new ArgumentException("At least one of WellId, PoolId, or FieldId must be provided");
                }

                _logger?.LogInformation("Starting DCA analysis for WellId: {WellId}, PoolId: {PoolId}, FieldId: {FieldId}", 
                    request.WellId, request.PoolId, request.FieldId);

                // Check if this is a physics-based forecast (uses reservoir properties instead of historical data)
                var forecastTypeStr = request.AdditionalParameters?.ForecastType ?? string.Empty;
                var isPhysicsBased = !string.IsNullOrEmpty(forecastTypeStr) &&
                    (forecastTypeStr.StartsWith("PHYSICS", StringComparison.OrdinalIgnoreCase) ||
                     forecastTypeStr.Equals("PSEUDO_STEADY_STATE", StringComparison.OrdinalIgnoreCase) ||
                     forecastTypeStr.Equals("TRANSIENT", StringComparison.OrdinalIgnoreCase) ||
                     forecastTypeStr.Equals("GAS_WELL", StringComparison.OrdinalIgnoreCase));

                if (isPhysicsBased)
                {
                    return await PerformPhysicsBasedForecastAsync(request, operationId, progressTracking);
                }

                // Step 1: Retrieve production data from PPDM database (for DCA)
                var productionDataPoints = await GetProductionDataForDCAAsync(request);
                
                if (productionDataPoints.Count == 0)
                {
                    throw new InvalidOperationException("No production data found for the specified criteria. Cannot perform DCA analysis.");
                }

                // Step 2: Extract production rates and dates for DCAManager
                var (productionRates, timeData) = ExtractProductionDataPoints(productionDataPoints, request.ProductionFluidType);

                if (productionRates.Count < 3)
                {
                    throw new InvalidOperationException($"Insufficient production data points ({productionRates.Count}). At least 3 data points are required for DCA analysis.");
                }

                // Step 3: Perform DCA analysis using DCAManager
                var dcaManager = new DCAManager();
                
                // Get initial estimates from request or use defaults
                var initialQi = request.AdditionalParameters?.InitialQi ?? productionRates.Max();
                var initialDi = request.AdditionalParameters?.InitialDi ?? 0.1;

                DCAFitResult fitResult;
                
                // Use async analysis if available, otherwise use synchronous with statistics
                if (request.AdditionalParameters?.UseAsync == true)
                {
                    fitResult = await dcaManager.AnalyzeAsync(productionRates, timeData, initialQi, initialDi);
                }
                else
                {
                    var confidenceLevel = request.AdditionalParameters?.ConfidenceLevel ?? 0.95;
                    fitResult = dcaManager.AnalyzeWithStatistics(productionRates, timeData, initialQi, initialDi, confidenceLevel);
                }

                // Step 4: Map DCAFitResult to DCAResult DTO
                var result = MapDCAFitResultToDCAResult(fitResult, request, productionRates, timeData);

                // Step 5: Generate forecast points if requested
                if (request.AdditionalParameters?.GenerateForecast == true)
                {
                    var forecastMonths = request.AdditionalParameters?.ForecastMonths ?? 60;
                    result.ForecastPoints = GenerateForecastPoints(fitResult, timeData, forecastMonths);
                }

                // Step 6: Store result in database
                var repository = await GetDCAResultRepositoryAsync();
              
                await repository.InsertAsync(result, request.UserId ?? "system");

                _logger?.LogInformation("DCA calculation completed successfully: {CalculationId}, RÂ²: {RSquared}, RMSE: {RMSE}", 
                    result.CalculationId, result.R2, result.RMSE);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing DCA analysis");
                
                // Return error result
                var errorResult = new DCAResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    PoolId = request.PoolId,
                    FieldId = request.FieldId,
                    CalculationType = request.CalculationType,
                    CalculationDate = DateTime.UtcNow,
                    ProductionFluidType = request.ProductionFluidType,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    ForecastPoints = new List<DCAForecastPoint>(),
                    AdditionalResults = new DcaAdditionalResults()
                };

                // Try to store error result
                try
                {
                    var repository = await GetDCAResultRepositoryAsync();

                    await repository.InsertAsync(errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing DCA error result");
                }

                throw;
            }
        }

        /// <summary>
        /// Retrieves production data from PPDM database for DCA analysis
        /// </summary>
        private async Task<List<PDEN_VOL_SUMMARY>> GetProductionDataForDCAAsync(DCARequest request)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

            var filters = new List<AppFilter>();

            // Apply filters based on request
            if (!string.IsNullOrEmpty(request.WellId))
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = "WELL_ID", 
                    Operator = "=", 
                    FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.WellId) 
                });
            }
            else if (!string.IsNullOrEmpty(request.PoolId))
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = "POOL_ID", 
                    Operator = "=", 
                    FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.PoolId) 
                });
            }
            else if (!string.IsNullOrEmpty(request.FieldId))
            {
                // For field, we might need to join through wells or pools
                // For now, try direct FIELD_ID if it exists in PDEN_VOL_SUMMARY table
                filters.Add(new AppFilter 
                { 
                    FieldName = "FIELD_ID", 
                    Operator = "=", 
                    FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.FieldId) 
                });
            }

            // Apply date range filters
            if (request.StartDate.HasValue)
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = "PRODUCTION_DATE", 
                    Operator = ">=", 
                    FilterValue = request.StartDate.Value.ToString("yyyy-MM-dd") 
                });
            }
            if (request.EndDate.HasValue)
            {
                filters.Add(new AppFilter 
                { 
                    FieldName = "PRODUCTION_DATE", 
                    Operator = "<=", 
                    FilterValue = request.EndDate.Value.ToString("yyyy-MM-dd") 
                });
            }

            // Only active records
            filters.Add(new AppFilter 
            { 
                FieldName = "ACTIVE_IND", 
                Operator = "=", 
                FilterValue = _defaults.GetActiveIndicatorYes() 
            });

            var productionData = await repo.GetAsync(filters);
            
            // Sort by production date - cast to PDEN_VOL_SUMMARY and access property directly
            var sortedData = productionData
                .Cast<PDEN_VOL_SUMMARY>()
                .OrderBy(item => GetDateValue(item, "PRODUCTION_DATE") ?? DateTime.MinValue)
                .ToList();

            return sortedData;
        }

        /// <summary>
        /// Extracts production rates and dates from production data points
        /// Uses strongly-typed property access (no reflection for property names)
        /// </summary>
        private (List<double> productionRates, List<DateTime> timeData) ExtractProductionDataPoints(
            List<PDEN_VOL_SUMMARY> productionDataPoints, 
            string? productionFluidType)
        {
            var productionRates = new List<double>();
            var timeData = new List<DateTime>();

            foreach (var point in productionDataPoints)
            {
                var date = GetDateValue(point, "PRODUCTION_DATE");
                if (!date.HasValue)
                    continue; 

                // Extract production volume/rate based on fluid type
                double? volume = null;
                
                switch (productionFluidType?.ToUpperInvariant())
                {
                    case "OIL":
                        volume = (double?)GetPropertyValueMultiple(point, "OIL_VOLUME", "DAILY_OIL");
                        break;
                    case "GAS":
                        volume = (double?)GetPropertyValueMultiple(point, "GAS_VOLUME", "DAILY_GAS");
                        break;
                    case "WATER":
                        volume = (double?)GetPropertyValueMultiple(point, "WATER_VOLUME", "DAILY_WATER");
                        break;
                    default:
                        volume = (double?)GetPropertyValueMultiple(point, "OIL_VOLUME", "DAILY_OIL");
                        break;
                }

                // If volume still not found, try production rate
                if (!volume.HasValue)
                {
                    volume = (double?)GetPropertyValue(point, "PRODUCTION_RATE");
                }

                if (volume.HasValue && volume.Value > 0)
                {
                    productionRates.Add(volume.Value);
                    timeData.Add(date.Value);
                }
            }

            return (productionRates, timeData);
        }

        private DCAResult MapDCAFitResultToDCAResult(
            DCAFitResult fitResult, 
            DCARequest request, 
            List<double> productionRates, 
            List<DateTime> timeData)
        {
            var result = new DCAResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                PoolId = request.PoolId,
                FieldId = request.FieldId,
                CalculationType = request.CalculationType,
                CalculationDate = DateTime.UtcNow,
                ProductionFluidType = request.ProductionFluidType,
                Status = fitResult.Converged ? "SUCCESS" : "PARTIAL",
                UserId = request.UserId,
                ForecastPoints = new List<DCAForecastPoint>(),
                AdditionalResults = new DcaAdditionalResults()
            };

            if (fitResult.Parameters != null && fitResult.Parameters.Length > 0)
            {
                result.InitialRate = (decimal)fitResult.Parameters[0]; 

                if (fitResult.Parameters.Length > 1)
                {
                    result.HyperbolicExponent = (decimal)fitResult.Parameters[1]; 
                }

                if (productionRates.Count > 1)
                {
                    var initialRate = productionRates.First();
                    var finalRate = productionRates.Last();
                    var timeSpan = (timeData.Last() - timeData.First()).TotalDays;
                    if (timeSpan > 0)
                    {
                        var declineRate = (initialRate - finalRate) / (initialRate * timeSpan / 365.25); 
                        result.DeclineRate = (decimal)declineRate;
                    }
                }
            }

            result.R2 = (decimal)fitResult.RSquared;
            result.RMSE = (decimal)fitResult.RMSE;
            result.CorrelationCoefficient = (decimal)Math.Sqrt(fitResult.RSquared); 

            result.AdditionalResults = new DcaAdditionalResults
            {
                AdjustedRSquared = fitResult.AdjustedRSquared,
                Mae = fitResult.MAE,
                Aic = fitResult.AIC,
                Bic = fitResult.BIC,
                Iterations = fitResult.Iterations,
                Converged = fitResult.Converged,
                DataPointCount = productionRates.Count
            };

            if (fitResult.Parameters != null && fitResult.Parameters.Length > 0 && result.DeclineRate.HasValue)
            {
                var qi = fitResult.Parameters[0];
                var di = (double)result.DeclineRate.Value;
                var economicLimit = 0.1; 
                var b = fitResult.Parameters.Length > 1 ? fitResult.Parameters[1] : 1.0;

                if (di != 0 && (1 - b) != 0)
                {
                    var estimatedEUR = qi / (di * (1 - b)) * (Math.Pow(qi / economicLimit, 1 - b) - 1);
                    result.EstimatedEUR = (decimal)estimatedEUR;
                }
            }

            return result;
        }

        private List<DCAForecastPoint> GenerateForecastPoints(
            DCAFitResult fitResult, 
            List<DateTime> timeData, 
            int forecastMonths)
        {
            var forecastPoints = new List<DCAForecastPoint>();
            if (fitResult.Parameters == null || fitResult.Parameters.Length == 0 || timeData.Count == 0)
                return forecastPoints;

            var startDate = timeData.Last(); 
            var qi = fitResult.Parameters[0];
            var b = fitResult.Parameters.Length > 1 ? fitResult.Parameters[1] : 1.0;
            var di = 0.1; 
            var cumulativeProduction = 0.0;

            for (int i = 1; i <= forecastMonths; i++)
            {
                var forecastDate = startDate.AddMonths(i);
                var daysSinceStart = (forecastDate - timeData.First()).TotalDays;
                
                var productionRate = qi / Math.Pow(1 + b * di * daysSinceStart, 1.0 / b);
                cumulativeProduction += productionRate * 30; 

                forecastPoints.Add(new DCAForecastPoint
                {
                    Date = forecastDate,
                    ProductionRate = (decimal)productionRate,
                    CumulativeProduction = (decimal)cumulativeProduction,
                    DeclineRate = (decimal)(di * 100.0) 
                });
            }

            return forecastPoints;
        }

        private async Task<Beep.OilandGas.Models.Data.Calculations.DCAResult> PerformPhysicsBasedForecastAsync(
            Beep.OilandGas.Models.Data.Calculations.DCARequest request, 
            string? operationId, 
            object? progressTracking)
        {
            try
            {
                _logger?.LogInformation("Starting physics-based forecast for WellId: {WellId}, PoolId: {PoolId}", 
                    request.WellId, request.PoolId);

                var reservoirProperties = await GetReservoirPropertiesForForecastAsync(request);
                
                if (reservoirProperties == null)
                {
                    throw new InvalidOperationException("Reservoir properties not found. Cannot perform physics-based forecast.");
                }

                var forecastTypeStr = request.AdditionalParameters?.ForecastType ?? "PSEUDO_STEADY_STATE_SINGLE_PHASE";

                var bottomHolePressure = (decimal)(request.AdditionalParameters?.BottomHolePressure ?? 1000m);
                var forecastDuration = (decimal)(request.AdditionalParameters?.ForecastDuration ?? 1825m);
                var timeSteps = request.AdditionalParameters?.TimeSteps ?? 100;
                var bubblePointPressure = (decimal)(request.AdditionalParameters?.BubblePointPressure ?? 0m);

                PRODUCTION_FORECAST forecast;
                
                switch (forecastTypeStr.ToUpperInvariant())
                {
                    case "PSEUDO_STEADY_STATE_SINGLE_PHASE":
                    case "PSEUDO_STEADY_STATE":
                        forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
                            reservoirProperties!, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    case "PSEUDO_STEADY_STATE_TWO_PHASE":
                        if (bubblePointPressure <= 0)
                            throw new ArgumentException("BubblePointPressure is required for two-phase forecast");
                        
                        var res2 = PseudoSteadyStateForecast.GenerateTwoPhaseForecast(
                            reservoirProperties!, bottomHolePressure, bubblePointPressure, forecastDuration, timeSteps);
                        
                        // Map PRODUCTION_FORECAST to PRODUCTION_FORECAST if needed
                        forecast = new PRODUCTION_FORECAST
                        {
                            FORECAST_TYPE = ForecastType.PseudoSteadyStateTwoPhase,
                            FORECAST_DURATION = res2.FORECAST_DURATION,
                            INITIAL_PRODUCTION_RATE = res2.INITIAL_PRODUCTION_RATE,
                            FINAL_PRODUCTION_RATE = res2.FINAL_PRODUCTION_RATE,
                            TOTAL_CUMULATIVE_PRODUCTION = res2.TOTAL_CUMULATIVE_PRODUCTION,
                            FORECAST_POINTS = res2.FORECAST_POINTS.Select(p => new FORECAST_POINT
                            {
                                TIME = p.TIME,
                                PRODUCTION_RATE = p.PRODUCTION_RATE,
                                CUMULATIVE_PRODUCTION = p.CUMULATIVE_PRODUCTION,
                                RESERVOIR_PRESSURE = p.RESERVOIR_PRESSURE,
                                BOTTOM_HOLE_PRESSURE = p.BOTTOM_HOLE_PRESSURE
                            }).ToList()
                        };
                        break;
                        
                    case "TRANSIENT":
                        forecast = TransientForecast.GenerateTransientForecast(
                            reservoirProperties!, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    case "GAS_WELL":
                        forecast = GasWellForecast.GenerateGasWellForecast(
                            reservoirProperties!, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    default:
                        forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
                            reservoirProperties!, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                }

                var result = MapProductionForecastToDCAResult(forecast, request);

                var repository = await GetDCAResultRepositoryAsync();
                await repository.InsertAsync(result, request.UserId ?? "system");

                _logger?.LogInformation("Physics-based forecast completed successfully: {CalculationId}, ForecastType: {ForecastType}", 
                    result.CalculationId, forecastTypeStr);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing physics-based forecast");
                
                var errorResult = new DCAResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    PoolId = request.PoolId,
                    FieldId = request.FieldId,
                    CalculationType = request.CalculationType,
                    CalculationDate = DateTime.UtcNow,
                    ProductionFluidType = request.ProductionFluidType,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    ForecastPoints = new List<DCAForecastPoint>(),
                    AdditionalResults = new DcaAdditionalResults()
                };

                try
                {
                    var repository = await GetDCAResultRepositoryAsync();
                    await repository.InsertAsync(errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing physics-based forecast error result");
                }

                throw;
            }
        }

    }
}
