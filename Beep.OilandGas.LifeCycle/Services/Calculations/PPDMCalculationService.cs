using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.DCA;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.ProductionForecasting.Models;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    /// <summary>
    /// Service for performing calculations (DCA, Economic Analysis, Nodal Analysis)
    /// Stores calculation results in PPDM database using PPDMGenericRepository
    /// </summary>
    public class PPDMCalculationService : ICalculationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<PPDMCalculationService>? _logger;

        // Cache for repositories
        private PPDMGenericRepository? _dcaResultRepository;
        private PPDMGenericRepository? _economicResultRepository;
        private PPDMGenericRepository? _nodalResultRepository;

        public PPDMCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<PPDMCalculationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Gets or creates repository for DCA results
        /// Note: Uses dynamic entity type from metadata, falling back to dictionary if needed
        /// </summary>
        private async Task<PPDMGenericRepository> GetDCAResultRepositoryAsync()
        {
            if (_dcaResultRepository == null)
            {
                // Try to get entity type from metadata
                var metadata = await _metadata.GetTableMetadataAsync("DCA_CALCULATION");
                Type entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _dcaResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    "DCA_CALCULATION",
                    null);
            }
            return _dcaResultRepository;
        }

        /// <summary>
        /// Gets or creates repository for Economic Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetEconomicResultRepositoryAsync()
        {
            if (_economicResultRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("ECONOMIC_ANALYSIS");
                Type entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _economicResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    "ECONOMIC_ANALYSIS",
                    null);
            }
            return _economicResultRepository;
        }

        /// <summary>
        /// Gets or creates repository for Nodal Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetNodalResultRepositoryAsync()
        {
            if (_nodalResultRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("NODAL_ANALYSIS");
                Type entityType = typeof(Dictionary<string, object>);
                
                if (metadata != null && !string.IsNullOrEmpty(metadata.EntityTypeName))
                {
                    var resolvedType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                    if (resolvedType != null)
                    {
                        entityType = resolvedType;
                    }
                }

                _nodalResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    "NODAL_ANALYSIS",
                    null);
            }
            return _nodalResultRepository;
        }

        /// <summary>
        /// Converts a DTO to a dictionary for storage
        /// </summary>
        private Dictionary<string, object> ConvertToDictionary<T>(T dto) where T : class
        {
            var json = JsonSerializer.Serialize(dto);
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            // Ensure CalculationId is set if it's the ID property
            if (dto is DCAResult dcaResult && !string.IsNullOrEmpty(dcaResult.CalculationId))
            {
                dict["CALCULATION_ID"] = dcaResult.CalculationId;
            }
            else if (dto is EconomicAnalysisResult economicResult && !string.IsNullOrEmpty(economicResult.CalculationId))
            {
                dict["CALCULATION_ID"] = economicResult.CalculationId;
            }
            else if (dto is NodalAnalysisResult nodalResult && !string.IsNullOrEmpty(nodalResult.CalculationId))
            {
                dict["CALCULATION_ID"] = nodalResult.CalculationId;
            }

            return dict ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Converts a dictionary from storage to a DTO
        /// </summary>
        private T ConvertFromDictionary<T>(Dictionary<string, object> dict) where T : class
        {
            var json = JsonSerializer.Serialize(dict);
            return JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>();
        }

        public async Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request)
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
                var isPhysicsBased = false;
                if (request.AdditionalParameters?.ContainsKey("ForecastType") == true)
                {
                    var forecastTypeStr = request.AdditionalParameters["ForecastType"]?.ToString() ?? string.Empty;
                    isPhysicsBased = forecastTypeStr.StartsWith("PHYSICS", StringComparison.OrdinalIgnoreCase) ||
                                     forecastTypeStr.Equals("PSEUDO_STEADY_STATE", StringComparison.OrdinalIgnoreCase) ||
                                     forecastTypeStr.Equals("TRANSIENT", StringComparison.OrdinalIgnoreCase) ||
                                     forecastTypeStr.Equals("GAS_WELL", StringComparison.OrdinalIgnoreCase);
                }

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
                var initialQi = request.AdditionalParameters?.ContainsKey("InitialQi") == true 
                    ? Convert.ToDouble(request.AdditionalParameters["InitialQi"]) 
                    : productionRates.Max(); // Use max production rate as initial estimate
                var initialDi = request.AdditionalParameters?.ContainsKey("InitialDi") == true 
                    ? Convert.ToDouble(request.AdditionalParameters["InitialDi"]) 
                    : 0.1; // Default 10% decline rate

                DCAFitResult fitResult;
                
                // Use async analysis if available, otherwise use synchronous with statistics
                if (request.AdditionalParameters?.ContainsKey("UseAsync") == true && 
                    Convert.ToBoolean(request.AdditionalParameters["UseAsync"]))
                {
                    fitResult = await dcaManager.AnalyzeAsync(productionRates, timeData, initialQi, initialDi);
                }
                else
                {
                    var confidenceLevel = request.AdditionalParameters?.ContainsKey("ConfidenceLevel") == true 
                        ? Convert.ToDouble(request.AdditionalParameters["ConfidenceLevel"]) 
                        : 0.95;
                    fitResult = dcaManager.AnalyzeWithStatistics(productionRates, timeData, initialQi, initialDi, confidenceLevel);
                }

                // Step 4: Map DCAFitResult to DCAResult DTO
                var result = MapDCAFitResultToDCAResult(fitResult, request, productionRates, timeData);

                // Step 5: Generate forecast points if requested
                if (request.AdditionalParameters?.ContainsKey("GenerateForecast") == true && 
                    Convert.ToBoolean(request.AdditionalParameters["GenerateForecast"]))
                {
                    var forecastMonths = request.AdditionalParameters?.ContainsKey("ForecastMonths") == true 
                        ? Convert.ToInt32(request.AdditionalParameters["ForecastMonths"]) 
                        : 60; // Default 5 years
                    result.ForecastPoints = GenerateForecastPoints(fitResult, timeData, forecastMonths);
                }

                // Step 6: Store result in database
                var repository = await GetDCAResultRepositoryAsync();
                var dict = ConvertToDictionary(result);
                await repository.InsertAsync(dict, request.UserId ?? "system");

                _logger?.LogInformation("DCA calculation completed successfully: {CalculationId}, R²: {RSquared}, RMSE: {RMSE}", 
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
                    AdditionalResults = new Dictionary<string, object>()
                };

                // Try to store error result
                try
                {
                    var repository = await GetDCAResultRepositoryAsync();
                    var dict = ConvertToDictionary(errorResult);
                    await repository.InsertAsync(dict, request.UserId ?? "system");
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
                .OrderBy(item => 
                {
                    // Access PRODUCTION_DATE property directly from strongly-typed entity
                    var dateProp = item.GetType().GetProperty("PRODUCTION_DATE");
                    var dateValue = dateProp?.GetValue(item);
                    if (dateValue is DateTime dt)
                        return dt;
                    if (dateValue != null && dateValue is DateTime nullableDt)
                    {
                        return nullableDt;
                    }
                    // Check if it's a nullable DateTime using reflection
                    if (dateValue != null && dateValue.GetType() == typeof(DateTime?))
                    {
                        var nullableValue = (DateTime?)dateValue;
                        if (nullableValue.HasValue)
                            return nullableValue.Value;
                    }
                    return DateTime.MinValue;
                })
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
                // Get property info once (cached at type level, not instance)
                var type = point.GetType();
                var dateProp = type.GetProperty("PRODUCTION_DATE");
                var dateValue = dateProp?.GetValue(point);
                
                DateTime? date = null;
                if (dateValue is DateTime dt)
                    date = dt;
                else if (dateValue != null && dateValue is DateTime nullableDt)
                {
                    date = nullableDt;
                }
                // Check if it's a nullable DateTime using reflection
                else if (dateValue != null && dateValue.GetType() == typeof(DateTime?))
                {
                    var nullableValue = (DateTime?)dateValue;
                    if (nullableValue.HasValue)
                        date = nullableValue.Value;
                }

                if (!date.HasValue)
                    continue; // Skip points without valid dates

                // Extract production volume/rate based on fluid type - access properties directly
                double? volume = null;
                
                switch (productionFluidType?.ToUpperInvariant())
                {
                    case "OIL":
                        var oilVolumeProp = type.GetProperty("OIL_VOLUME");
                        var dailyOilProp = type.GetProperty("DAILY_OIL");
                        var oilVol = oilVolumeProp?.GetValue(point) ?? dailyOilProp?.GetValue(point);
                        volume = ConvertToDouble(oilVol);
                        break;
                    case "GAS":
                        var gasVolumeProp = type.GetProperty("GAS_VOLUME");
                        var dailyGasProp = type.GetProperty("DAILY_GAS");
                        var gasVol = gasVolumeProp?.GetValue(point) ?? dailyGasProp?.GetValue(point);
                        volume = ConvertToDouble(gasVol);
                        break;
                    case "WATER":
                        var waterVolumeProp = type.GetProperty("WATER_VOLUME");
                        var dailyWaterProp = type.GetProperty("DAILY_WATER");
                        var waterVol = waterVolumeProp?.GetValue(point) ?? dailyWaterProp?.GetValue(point);
                        volume = ConvertToDouble(waterVol);
                        break;
                    default:
                        // Default to oil
                        var defaultOilVolumeProp = type.GetProperty("OIL_VOLUME");
                        var defaultDailyOilProp = type.GetProperty("DAILY_OIL");
                        var defaultOilVol = defaultOilVolumeProp?.GetValue(point) ?? defaultDailyOilProp?.GetValue(point);
                        volume = ConvertToDouble(defaultOilVol);
                        break;
                }

                // If volume still not found, try production rate
                if (!volume.HasValue)
                {
                    var rateProp = type.GetProperty("PRODUCTION_RATE");
                    var rateValue = rateProp?.GetValue(point);
                    volume = ConvertToDouble(rateValue);
                }

                if (volume.HasValue && volume.Value > 0)
                {
                    productionRates.Add(volume.Value);
                    timeData.Add(date.Value);
                }
            }

            return (productionRates, timeData);
        }
        
        /// <summary>
        /// Converts various numeric types to double
        /// </summary>
        private double? ConvertToDouble(object? value)
        {
            if (value == null)
                return null;
                
            if (value is double d)
                return d;
            if (value is decimal dec)
                return (double)dec;
            if (value is float f)
                return f;
            if (value is int i)
                return i;
            if (value is long l)
                return l;
            if (double.TryParse(value.ToString(), out var parsed))
                return parsed;
                
            return null;
        }

        /// <summary>
        /// Maps DCAFitResult from DCAManager to DCAResult DTO
        /// </summary>
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
                AdditionalResults = new Dictionary<string, object>()
            };

            // Map decline curve parameters
            if (fitResult.Parameters != null && fitResult.Parameters.Length > 0)
            {
                result.InitialRate = (decimal)fitResult.Parameters[0]; // qi

                if (fitResult.Parameters.Length > 1)
                {
                    result.HyperbolicExponent = (decimal)fitResult.Parameters[1]; // b
                }

                // Calculate decline rate from data if available
                if (productionRates.Count > 1)
                {
                    var initialRate = productionRates.First();
                    var finalRate = productionRates.Last();
                    var timeSpan = (timeData.Last() - timeData.First()).TotalDays;
                    if (timeSpan > 0)
                    {
                        var declineRate = (initialRate - finalRate) / (initialRate * timeSpan / 365.25); // Annual decline rate
                        result.DeclineRate = (decimal)declineRate;
                    }
                }
            }

            // Map statistical metrics
            result.R2 = (decimal)fitResult.RSquared;
            result.RMSE = (decimal)fitResult.RMSE;
            result.CorrelationCoefficient = (decimal)Math.Sqrt(fitResult.RSquared); // Approximate correlation coefficient

            // Store additional metrics in AdditionalResults
            result.AdditionalResults = new Dictionary<string, object>
            {
                ["AdjustedRSquared"] = fitResult.AdjustedRSquared,
                ["MAE"] = fitResult.MAE,
                ["AIC"] = fitResult.AIC,
                ["BIC"] = fitResult.BIC,
                ["Iterations"] = fitResult.Iterations,
                ["Converged"] = fitResult.Converged,
                ["DataPointCount"] = productionRates.Count
            };

            // Calculate estimated EUR (Estimated Ultimate Recovery) - simplified calculation
            if (fitResult.Parameters != null && fitResult.Parameters.Length > 0 && result.DeclineRate.HasValue)
            {
                // Use Arps hyperbolic decline equation to estimate EUR
                // This is a simplified calculation - in practice, EUR would be calculated until economic limit
                var qi = fitResult.Parameters[0];
                var di = result.DeclineRate.Value;
                var economicLimit = 0.1; // Assume 10% of initial rate as economic limit
                var b = fitResult.Parameters.Length > 1 ? fitResult.Parameters[1] : 1.0;

                // Simplified EUR calculation (cumulative production to economic limit)
                // In practice, this would integrate the decline curve
                var estimatedEUR = qi / ((double)di * (1 - b)) * (Math.Pow(qi / economicLimit, 1 - b) - 1);
                result.EstimatedEUR = (decimal)estimatedEUR;
            }

            return result;
        }

        /// <summary>
        /// Generates forecast points based on DCA fit result
        /// </summary>
        private List<DCAForecastPoint> GenerateForecastPoints(
            DCAFitResult fitResult, 
            List<DateTime> timeData, 
            int forecastMonths)
        {
            var forecastPoints = new List<DCAForecastPoint>();
            if (fitResult.Parameters == null || fitResult.Parameters.Length == 0 || timeData.Count == 0)
                return forecastPoints;

            var startDate = timeData.Last(); // Start forecast from last data point
            var qi = fitResult.Parameters[0];
            var b = fitResult.Parameters.Length > 1 ? fitResult.Parameters[1] : 1.0;
            var di = 0.1; // Default decline rate - in practice, this would come from fit (as double)
            var cumulativeProduction = 0.0;

            for (int i = 1; i <= forecastMonths; i++)
            {
                var forecastDate = startDate.AddMonths(i);
                var daysSinceStart = (forecastDate - timeData.First()).TotalDays;
                
                // Calculate production rate using hyperbolic decline equation
                var productionRate = qi / Math.Pow(1 + b * di * daysSinceStart, 1.0 / b);
                
                // Simple cumulative calculation (in practice, this would integrate the curve)
                cumulativeProduction += productionRate * 30; // Approximate monthly production

                forecastPoints.Add(new DCAForecastPoint
                {
                    Date = forecastDate,
                    ProductionRate = (decimal)productionRate,
                    CumulativeProduction = (decimal)cumulativeProduction,
                    DeclineRate = (decimal)di * 100m // As percentage
                });
            }

            return forecastPoints;
        }

        public async Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId) && string.IsNullOrEmpty(request.FieldId) && string.IsNullOrEmpty(request.ProjectId))
                {
                    throw new ArgumentException("At least one of WellId, PoolId, FieldId, or ProjectId must be provided");
                }

                // TODO: Perform actual Economic Analysis calculation here
                // For now, create a placeholder result
                var result = new EconomicAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    PoolId = request.PoolId,
                    FieldId = request.FieldId,
                    ProjectId = request.ProjectId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "SUCCESS",
                    UserId = request.UserId,
                    CashFlowPoints = new List<EconomicCashFlowPoint>(),
                    AdditionalResults = new Dictionary<string, object>()
                };

                // Store result in database
                var repository = await GetEconomicResultRepositoryAsync();
                var dict = ConvertToDictionary(result);
                await repository.InsertAsync(dict, request.UserId ?? "system");

                _logger?.LogInformation("Economic Analysis calculation completed: {CalculationId}", result.CalculationId);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Economic Analysis");
                throw;
            }
        }

        public async Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.WellboreId))
                {
                    throw new ArgumentException("At least one of WellId or WellboreId must be provided");
                }

                // TODO: Perform actual Nodal Analysis calculation here
                // For now, create a placeholder result
                var result = new NodalAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    WellboreId = request.WellboreId,
                    FieldId = request.FieldId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "SUCCESS",
                    UserId = request.UserId,
                    IPRCurve = new List<NodalCurvePoint>(),
                    VLPCurve = new List<NodalCurvePoint>(),
                    Recommendations = new List<string>(),
                    AdditionalResults = new Dictionary<string, object>()
                };

                // Store result in database
                var repository = await GetNodalResultRepositoryAsync();
                var dict = ConvertToDictionary(result);
                await repository.InsertAsync(dict, request.UserId ?? "system");

                _logger?.LogInformation("Nodal Analysis calculation completed: {CalculationId}", result.CalculationId);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Nodal Analysis");
                throw;
            }
        }

        public async Task<object?> GetCalculationResultAsync(string calculationId, string calculationType)
        {
            try
            {
                PPDMGenericRepository repository;
                Type resultType;

                switch (calculationType.ToUpper())
                {
                    case "DCA":
                        repository = await GetDCAResultRepositoryAsync();
                        resultType = typeof(DCAResult);
                        break;
                    case "ECONOMIC":
                        repository = await GetEconomicResultRepositoryAsync();
                        resultType = typeof(EconomicAnalysisResult);
                        break;
                    case "NODAL":
                        repository = await GetNodalResultRepositoryAsync();
                        resultType = typeof(NodalAnalysisResult);
                        break;
                    default:
                        throw new ArgumentException($"Unknown calculation type: {calculationType}");
                }

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "CALCULATION_ID", Operator = "=", FilterValue = calculationId }
                };

                var results = await repository.GetAsync(filters);
                var result = results.FirstOrDefault();

                if (result == null)
                    return null;

                if (result is Dictionary<string, object> dict)
                {
                    var json = JsonSerializer.Serialize(dict);
                    return JsonSerializer.Deserialize(json, resultType);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting calculation result");
                throw;
            }
        }

        public async Task<List<object>> GetCalculationResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null, string? calculationType = null)
        {
            try
            {
                var allResults = new List<object>();

                // If calculation type is specified, only query that type
                if (!string.IsNullOrEmpty(calculationType))
                {
                    var results = await GetCalculationResultsByTypeAsync(calculationType, wellId, poolId, fieldId);
                    allResults.AddRange(results);
                }
                else
                {
                    // Query all calculation types
                    var dcaResults = await GetCalculationResultsByTypeAsync("DCA", wellId, poolId, fieldId);
                    var economicResults = await GetCalculationResultsByTypeAsync("ECONOMIC", wellId, poolId, fieldId);
                    var nodalResults = await GetCalculationResultsByTypeAsync("NODAL", wellId, poolId, fieldId);

                    allResults.AddRange(dcaResults);
                    allResults.AddRange(economicResults);
                    allResults.AddRange(nodalResults);
                }

                return allResults;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting calculation results");
                throw;
            }
        }

        private async Task<List<object>> GetCalculationResultsByTypeAsync(string calculationType, string? wellId, string? poolId, string? fieldId)
        {
            PPDMGenericRepository repository;

            switch (calculationType.ToUpper())
            {
                case "DCA":
                    repository = await GetDCAResultRepositoryAsync();
                    break;
                case "ECONOMIC":
                    repository = await GetEconomicResultRepositoryAsync();
                    break;
                case "NODAL":
                    repository = await GetNodalResultRepositoryAsync();
                    break;
                default:
                    return new List<object>();
            }

            var filters = new List<AppFilter>();

            if (!string.IsNullOrEmpty(wellId))
                filters.Add(new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId });

            if (!string.IsNullOrEmpty(poolId))
                filters.Add(new AppFilter { FieldName = "POOL_ID", Operator = "=", FilterValue = poolId });

            if (!string.IsNullOrEmpty(fieldId))
                filters.Add(new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId });

            var results = await repository.GetAsync(filters);
            return results.ToList();
        }

        /// <summary>
        /// Performs physics-based production forecasting using reservoir properties
        /// </summary>
        private async Task<DCAResult> PerformPhysicsBasedForecastAsync(
            DCARequest request, 
            string? operationId, 
            object? progressTracking)
        {
            try
            {
                _logger?.LogInformation("Starting physics-based forecast for WellId: {WellId}, PoolId: {PoolId}", 
                    request.WellId, request.PoolId);

                // Step 1: Retrieve reservoir properties from PPDM database
                var reservoirProperties = await GetReservoirPropertiesForForecastAsync(request);
                
                if (reservoirProperties == null)
                {
                    throw new InvalidOperationException("Reservoir properties not found. Cannot perform physics-based forecast.");
                }

                // Step 2: Determine forecast type and parameters
                var forecastType = request.AdditionalParameters?.ContainsKey("ForecastType") == true
                    ? request.AdditionalParameters["ForecastType"]?.ToString()
                    : "PSEUDO_STEADY_STATE_SINGLE_PHASE";

                var bottomHolePressure = request.AdditionalParameters?.ContainsKey("BottomHolePressure") == true
                    ? Convert.ToDecimal(request.AdditionalParameters["BottomHolePressure"])
                    : 1000m; // Default 1000 psia

                var forecastDuration = request.AdditionalParameters?.ContainsKey("ForecastDuration") == true
                    ? Convert.ToDecimal(request.AdditionalParameters["ForecastDuration"])
                    : 1825m; // Default 5 years in days

                var timeSteps = request.AdditionalParameters?.ContainsKey("TimeSteps") == true
                    ? Convert.ToInt32(request.AdditionalParameters["TimeSteps"])
                    : 100;

                var bubblePointPressure = request.AdditionalParameters?.ContainsKey("BubblePointPressure") == true
                    ? Convert.ToDecimal(request.AdditionalParameters["BubblePointPressure"])
                    : 0m;

                // Step 3: Perform physics-based forecast
                ProductionForecast forecast;
                
                switch (forecastType?.ToUpperInvariant())
                {
                    case "PSEUDO_STEADY_STATE_SINGLE_PHASE":
                    case "PSEUDO_STEADY_STATE":
                        forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
                            reservoirProperties, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    case "PSEUDO_STEADY_STATE_TWO_PHASE":
                        if (bubblePointPressure <= 0)
                            throw new ArgumentException("BubblePointPressure is required for two-phase forecast");
                        forecast = PseudoSteadyStateForecast.GenerateTwoPhaseForecast(
                            reservoirProperties, bottomHolePressure, bubblePointPressure, forecastDuration, timeSteps);
                        break;
                        
                    case "TRANSIENT":
                        forecast = TransientForecast.GenerateTransientForecast(
                            reservoirProperties, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    case "GAS_WELL":
                        forecast = GasWellForecast.GenerateGasWellForecast(
                            reservoirProperties, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                        
                    default:
                        // Default to single-phase pseudo-steady state
                        forecast = PseudoSteadyStateForecast.GenerateSinglePhaseForecast(
                            reservoirProperties, bottomHolePressure, forecastDuration, timeSteps);
                        break;
                }

                // Step 4: Map ProductionForecast to DCAResult DTO
                var result = MapProductionForecastToDCAResult(forecast, request);

                // Step 5: Store result in database
                var repository = await GetDCAResultRepositoryAsync();
                var dict = ConvertToDictionary(result);
                await repository.InsertAsync(dict, request.UserId ?? "system");

                _logger?.LogInformation("Physics-based forecast completed successfully: {CalculationId}, ForecastType: {ForecastType}", 
                    result.CalculationId, forecastType);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing physics-based forecast");
                
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
                    AdditionalResults = new Dictionary<string, object>()
                };

                // Try to store error result
                try
                {
                    var repository = await GetDCAResultRepositoryAsync();
                    var dict = ConvertToDictionary(errorResult);
                    await repository.InsertAsync(dict, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing physics-based forecast error result");
                }

                throw;
            }
        }

        #region Well Calculation Property Helpers - Known PPDM Fields

        // Entity cache to avoid multiple database calls for the same entity
        private readonly Dictionary<string, object?> _entityCache = new();
        private readonly Dictionary<string, List<object>> _entityListCache = new();

        /// <summary>
        /// Retrieval mode for time-series data
        /// </summary>
        public enum DataRetrievalMode
        {
            /// <summary>Get the most recent record (default)</summary>
            Latest,
            /// <summary>Get record at or nearest to a specific date</summary>
            ByDate,
            /// <summary>Get all historical records</summary>
            History
        }

        /// <summary>
        /// Gets a single PPDM entity from database with caching
        /// </summary>
        private async Task<object?> GetEntityAsync(string tableName, string entityId, string idFieldName)
        {
            var cacheKey = $"{tableName}:{entityId}";
            if (_entityCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                return null;

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName, null);

            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = idFieldName,
                    Operator = "=",
                    FilterValue = _defaults.FormatIdForTable(tableName, entityId)
                }
            };

            var entities = await repo.GetAsync(filters);
            var entity = entities.FirstOrDefault();
            _entityCache[cacheKey] = entity;
            return entity;
        }

        /// <summary>
        /// Gets multiple PPDM entities from database with caching and optional date ordering
        /// Used for time-series tables like WELL_TEST, PRODUCTION, etc.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="filters">Filters to apply</param>
        /// <param name="dateFieldName">Field to order by for time-series data (e.g., TEST_DATE, EFFECTIVE_DATE)</param>
        /// <param name="mode">Retrieval mode: Latest, ByDate, or History</param>
        /// <param name="asOfDate">Date for ByDate mode</param>
        private async Task<List<object>> GetEntitiesAsync(
            string tableName, 
            List<AppFilter> filters,
            string dateFieldName = "EFFECTIVE_DATE",
            DataRetrievalMode mode = DataRetrievalMode.Latest,
            DateTime? asOfDate = null)
        {
            var cacheKey = $"{tableName}:{string.Join(",", filters.Select(f => $"{f.FieldName}={f.FilterValue}"))}:{mode}:{asOfDate}";
            if (_entityListCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
                return new List<object>();

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(object);
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, tableName, null);

            // Add date filter for ByDate mode
            if (mode == DataRetrievalMode.ByDate && asOfDate.HasValue)
            {
                filters.Add(new AppFilter
                {
                    FieldName = dateFieldName,
                    Operator = "<=",
                    FilterValue = asOfDate.Value.ToString("yyyy-MM-dd")
                });
            }

            var entities = await repo.GetAsync(filters);
            var entityList = entities.ToList();

            // Sort by date descending to get latest first
            if (!string.IsNullOrEmpty(dateFieldName))
            {
                entityList = entityList
                    .OrderByDescending(e => GetDateValue(e, dateFieldName))
                    .ToList();
            }

            // For Latest or ByDate mode, take just the first record
            if (mode == DataRetrievalMode.Latest || mode == DataRetrievalMode.ByDate)
            {
                entityList = entityList.Take(1).ToList();
            }

            _entityListCache[cacheKey] = entityList;
            return entityList;
        }

        /// <summary>
        /// Gets the latest entity for a well from a time-series table
        /// </summary>
        private async Task<object?> GetLatestEntityForWellAsync(
            string tableName, 
            string wellId,
            string dateFieldName = "EFFECTIVE_DATE",
            DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = "WELL_ID",
                    Operator = "=",
                    FilterValue = wellId
                }
            };

            var mode = asOfDate.HasValue ? DataRetrievalMode.ByDate : DataRetrievalMode.Latest;
            var entities = await GetEntitiesAsync(tableName, filters, dateFieldName, mode, asOfDate);
            return entities.FirstOrDefault();
        }

        /// <summary>
        /// Gets all historical entities for a well from a time-series table
        /// </summary>
        private async Task<List<object>> GetHistoryForWellAsync(
            string tableName,
            string wellId,
            string dateFieldName = "EFFECTIVE_DATE",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<object>();

            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = "WELL_ID",
                    Operator = "=",
                    FilterValue = wellId
                }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter
                {
                    FieldName = dateFieldName,
                    Operator = ">=",
                    FilterValue = startDate.Value.ToString("yyyy-MM-dd")
                });
            }

            if (endDate.HasValue)
            {
                filters.Add(new AppFilter
                {
                    FieldName = dateFieldName,
                    Operator = "<=",
                    FilterValue = endDate.Value.ToString("yyyy-MM-dd")
                });
            }

            return await GetEntitiesAsync(tableName, filters, dateFieldName, DataRetrievalMode.History);
        }

        /// <summary>
        /// Gets a DateTime value from an entity property
        /// </summary>
        private DateTime? GetDateValue(object? entity, string propertyName)
        {
            if (entity == null || string.IsNullOrEmpty(propertyName))
                return null;

            var prop = entity.GetType().GetProperty(propertyName);
            if (prop == null)
                return null;

            var value = prop.GetValue(entity);
            if (value is DateTime dt)
                return dt;
            if (value != null && value is DateTime nullableDt)
                return nullableDt;
            if (DateTime.TryParse(value?.ToString(), out var parsed))
                return parsed;

            return null;
        }

        /// <summary>
        /// Gets a property value from entity by property name
        /// </summary>
        private decimal? GetPropertyValue(object? entity, string propertyName)
        {
            if (entity == null || string.IsNullOrEmpty(propertyName))
                return null;

            var prop = entity.GetType().GetProperty(propertyName);
            if (prop == null)
                return null;

            return ConvertToDecimalHelper(prop.GetValue(entity));
        }

        /// <summary>
        /// Gets a property value, trying multiple property names (for PPDM field variations)
        /// </summary>
        private decimal? GetPropertyValueMultiple(object? entity, params string[] propertyNames)
        {
            if (entity == null)
                return null;

            foreach (var propName in propertyNames)
            {
                var value = GetPropertyValue(entity, propName);
                if (value.HasValue)
                    return value;
            }
            return null;
        }

        #region POOL Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets initial reservoir pressure from POOL table
        /// PPDM field: INITIAL_RESERVOIR_PRESSURE or ORIG_RESERVOIR_PRES
        /// </summary>
        public async Task<decimal> GetPoolInitialPressureAsync(string poolId, decimal defaultValue = 3000m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "INITIAL_RESERVOIR_PRESSURE", 
                "ORIG_RESERVOIR_PRES", 
                "INITIAL_PRESSURE") ?? defaultValue;
        }

        /// <summary>
        /// Gets average porosity from POOL table
        /// PPDM field: AVG_POROSITY
        /// </summary>
        public async Task<decimal> GetPoolPorosityAsync(string poolId, decimal defaultValue = 0.2m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "AVG_POROSITY", "POROSITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets average permeability from POOL table
        /// PPDM field: AVG_PERMEABILITY
        /// </summary>
        public async Task<decimal> GetPoolPermeabilityAsync(string poolId, decimal defaultValue = 100m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "AVG_PERMEABILITY", "PERMEABILITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets average thickness/net pay from POOL table
        /// PPDM field: AVG_THICKNESS or NET_PAY_THICKNESS
        /// </summary>
        public async Task<decimal> GetPoolThicknessAsync(string poolId, decimal defaultValue = 50m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "AVG_THICKNESS", 
                "NET_PAY_THICKNESS", 
                "THICKNESS",
                "NET_PAY") ?? defaultValue;
        }

        /// <summary>
        /// Gets reservoir temperature from POOL table
        /// PPDM field: RESERVOIR_TEMPERATURE or AVG_RESERVOIR_TEMP
        /// </summary>
        public async Task<decimal> GetPoolTemperatureAsync(string poolId, decimal defaultValue = 560m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "RESERVOIR_TEMPERATURE", 
                "AVG_RESERVOIR_TEMP",
                "TEMPERATURE") ?? defaultValue;
        }

        /// <summary>
        /// Gets total compressibility from POOL table
        /// PPDM field: TOTAL_COMPRESSIBILITY or COMPRESSIBILITY
        /// </summary>
        public async Task<decimal> GetPoolCompressibilityAsync(string poolId, decimal defaultValue = 0.00001m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "TOTAL_COMPRESSIBILITY", 
                "COMPRESSIBILITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets bubble point pressure from POOL table
        /// PPDM field: BUBBLE_POINT_PRESSURE
        /// </summary>
        public async Task<decimal?> GetPoolBubblePointPressureAsync(string poolId)
        {
            if (string.IsNullOrEmpty(poolId))
                return null;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "BUBBLE_POINT_PRESSURE", "BUBBLE_POINT");
        }

        /// <summary>
        /// Gets oil viscosity from POOL table
        /// PPDM field: OIL_VISCOSITY
        /// </summary>
        public async Task<decimal> GetPoolOilViscosityAsync(string poolId, decimal defaultValue = 1.0m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "OIL_VISCOSITY", "VISCOSITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets gas viscosity from POOL table
        /// PPDM field: GAS_VISCOSITY
        /// </summary>
        public async Task<decimal> GetPoolGasViscosityAsync(string poolId, decimal defaultValue = 0.02m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "GAS_VISCOSITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets formation volume factor from POOL table
        /// PPDM field: FORMATION_VOLUME_FACTOR or FVF
        /// </summary>
        public async Task<decimal> GetPoolFormationVolumeFactorAsync(string poolId, decimal defaultValue = 1.2m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "FORMATION_VOLUME_FACTOR", 
                "OIL_FVF",
                "FVF") ?? defaultValue;
        }

        /// <summary>
        /// Gets gas gravity from POOL table
        /// PPDM field: GAS_GRAVITY or GAS_SPECIFIC_GRAVITY
        /// </summary>
        public async Task<decimal> GetPoolGasGravityAsync(string poolId, decimal defaultValue = 0.65m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, 
                "GAS_GRAVITY", 
                "GAS_SPECIFIC_GRAVITY") ?? defaultValue;
        }

        /// <summary>
        /// Gets drainage area from POOL table
        /// PPDM field: DRAINAGE_AREA
        /// </summary>
        public async Task<decimal> GetPoolDrainageAreaAsync(string poolId, decimal defaultValue = 640m)
        {
            if (string.IsNullOrEmpty(poolId))
                return defaultValue;

            var pool = await GetEntityAsync("POOL", poolId, "POOL_ID");
            return GetPropertyValueMultiple(pool, "DRAINAGE_AREA", "AREA") ?? defaultValue;
        }

        /// <summary>
        /// Gets drainage radius from POOL drainage area
        /// Calculated from DRAINAGE_AREA: radius = sqrt(area/pi)
        /// </summary>
        public async Task<decimal> GetPoolDrainageRadiusAsync(string poolId, decimal defaultValue = 1000m)
        {
            var area = await GetPoolDrainageAreaAsync(poolId, 0m);
            if (area <= 0)
                return defaultValue;

            // Convert acres to ft² (1 acre = 43560 ft²), then calculate radius
            var areaFt2 = area * 43560m;
            return (decimal)Math.Sqrt((double)(areaFt2 / (decimal)Math.PI));
        }

        #endregion

        #region WELL_TEST_ANALYSIS Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets permeability from WELL_TEST_ANALYSIS table
        /// PPDM field: PERMEABILITY
        /// </summary>
        /// <param name="wellId">Well ID</param>
        /// <param name="testId">Optional: specific test ID. If null, gets latest test.</param>
        /// <param name="asOfDate">Optional: get test at or before this date</param>
        public async Task<decimal?> GetWellTestPermeabilityAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                // Get specific test by ID
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                // Get latest or by date
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(analysis, "PERMEABILITY", "PERM");
        }

        /// <summary>
        /// Gets permeability history from WELL_TEST_ANALYSIS table
        /// Returns all test results for trend analysis
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestPermeabilityHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "PERMEABILITY", "PERM");
        }

        /// <summary>
        /// Gets skin factor from WELL_TEST_ANALYSIS table
        /// PPDM field: SKIN
        /// </summary>
        public async Task<decimal?> GetWellTestSkinAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(analysis, "SKIN", "SKIN_FACTOR");
        }

        /// <summary>
        /// Gets skin factor history from WELL_TEST_ANALYSIS table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestSkinHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "SKIN", "SKIN_FACTOR");
        }

        /// <summary>
        /// Gets productivity index from WELL_TEST_ANALYSIS table
        /// PPDM field: PRODUCTIVITY_INDEX or PI
        /// </summary>
        public async Task<decimal?> GetWellTestProductivityIndexAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(analysis, "PRODUCTIVITY_INDEX", "PI");
        }

        /// <summary>
        /// Gets productivity index history from WELL_TEST_ANALYSIS table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestProductivityIndexHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "PRODUCTIVITY_INDEX", "PI");
        }

        /// <summary>
        /// Gets AOF (Absolute Open Flow) from WELL_TEST_ANALYSIS table
        /// PPDM field: AOF_POTENTIAL or AOF
        /// </summary>
        public async Task<decimal?> GetWellTestAOFAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(analysis, "AOF_POTENTIAL", "AOF");
        }

        /// <summary>
        /// Gets wellbore storage coefficient from WELL_TEST_ANALYSIS table
        /// PPDM field: WELLBORE_STORAGE_COEFF
        /// </summary>
        public async Task<decimal?> GetWellTestWellboreStorageAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(analysis, "WELLBORE_STORAGE_COEFF", "WELLBORE_STORAGE");
        }

        /// <summary>
        /// Gets flow efficiency from WELL_TEST_ANALYSIS table
        /// PPDM field: FLOW_EFFICIENCY
        /// </summary>
        public async Task<decimal?> GetWellTestFlowEfficiencyAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? analysis;
            if (!string.IsNullOrEmpty(testId))
            {
                analysis = await GetEntityAsync("WELL_TEST_ANALYSIS", testId, "TEST_NUM");
            }
            else
            {
                analysis = await GetLatestEntityForWellAsync("WELL_TEST_ANALYSIS", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValue(analysis, "FLOW_EFFICIENCY");
        }

        #endregion

        #region WELL_TEST_FLOW Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets oil flow rate from WELL_TEST_FLOW table
        /// PPDM field: FLOW_RATE_OIL or OIL_RATE
        /// </summary>
        public async Task<decimal?> GetWellTestOilRateAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? flow;
            if (!string.IsNullOrEmpty(testId))
            {
                flow = await GetEntityAsync("WELL_TEST_FLOW", testId, "TEST_NUM");
            }
            else
            {
                flow = await GetLatestEntityForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(flow, "FLOW_RATE_OIL", "OIL_RATE", "OIL_FLOW_RATE");
        }

        /// <summary>
        /// Gets oil flow rate history from WELL_TEST_FLOW table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestOilRateHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "FLOW_RATE_OIL", "OIL_RATE", "OIL_FLOW_RATE");
        }

        /// <summary>
        /// Gets gas flow rate from WELL_TEST_FLOW table
        /// PPDM field: FLOW_RATE_GAS or GAS_RATE
        /// </summary>
        public async Task<decimal?> GetWellTestGasRateAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? flow;
            if (!string.IsNullOrEmpty(testId))
            {
                flow = await GetEntityAsync("WELL_TEST_FLOW", testId, "TEST_NUM");
            }
            else
            {
                flow = await GetLatestEntityForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(flow, "FLOW_RATE_GAS", "GAS_RATE", "GAS_FLOW_RATE");
        }

        /// <summary>
        /// Gets gas flow rate history from WELL_TEST_FLOW table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestGasRateHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "FLOW_RATE_GAS", "GAS_RATE", "GAS_FLOW_RATE");
        }

        /// <summary>
        /// Gets water flow rate from WELL_TEST_FLOW table
        /// PPDM field: FLOW_RATE_WATER or WATER_RATE
        /// </summary>
        public async Task<decimal?> GetWellTestWaterRateAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? flow;
            if (!string.IsNullOrEmpty(testId))
            {
                flow = await GetEntityAsync("WELL_TEST_FLOW", testId, "TEST_NUM");
            }
            else
            {
                flow = await GetLatestEntityForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(flow, "FLOW_RATE_WATER", "WATER_RATE", "WATER_FLOW_RATE");
        }

        /// <summary>
        /// Gets water flow rate history from WELL_TEST_FLOW table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestWaterRateHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "FLOW_RATE_WATER", "WATER_RATE", "WATER_FLOW_RATE");
        }

        /// <summary>
        /// Gets choke size from WELL_TEST_FLOW table
        /// PPDM field: CHOKE_SIZE
        /// </summary>
        public async Task<decimal?> GetWellTestChokeSizeAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? flow;
            if (!string.IsNullOrEmpty(testId))
            {
                flow = await GetEntityAsync("WELL_TEST_FLOW", testId, "TEST_NUM");
            }
            else
            {
                flow = await GetLatestEntityForWellAsync("WELL_TEST_FLOW", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValue(flow, "CHOKE_SIZE");
        }

        #endregion

        #region WELL_TEST_PRESSURE Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets static pressure from WELL_TEST_PRESSURE table
        /// PPDM field: STATIC_PRESSURE or SHUT_IN_PRESSURE
        /// </summary>
        public async Task<decimal?> GetWellTestStaticPressureAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? pressure;
            if (!string.IsNullOrEmpty(testId))
            {
                pressure = await GetEntityAsync("WELL_TEST_PRESSURE", testId, "TEST_NUM");
            }
            else
            {
                pressure = await GetLatestEntityForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(pressure, "STATIC_PRESSURE", "SHUT_IN_PRESSURE", "SITP");
        }

        /// <summary>
        /// Gets static pressure history from WELL_TEST_PRESSURE table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestStaticPressureHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "STATIC_PRESSURE", "SHUT_IN_PRESSURE", "SITP");
        }

        /// <summary>
        /// Gets flowing pressure from WELL_TEST_PRESSURE table
        /// PPDM field: FLOWING_PRESSURE
        /// </summary>
        public async Task<decimal?> GetWellTestFlowingPressureAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? pressure;
            if (!string.IsNullOrEmpty(testId))
            {
                pressure = await GetEntityAsync("WELL_TEST_PRESSURE", testId, "TEST_NUM");
            }
            else
            {
                pressure = await GetLatestEntityForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(pressure, "FLOWING_PRESSURE", "FLOW_PRESSURE", "FTP");
        }

        /// <summary>
        /// Gets flowing pressure history from WELL_TEST_PRESSURE table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestFlowingPressureHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "FLOWING_PRESSURE", "FLOW_PRESSURE", "FTP");
        }

        /// <summary>
        /// Gets bottom hole pressure from WELL_TEST_PRESSURE table
        /// PPDM field: BOTTOM_HOLE_PRESSURE or BHP
        /// </summary>
        public async Task<decimal?> GetWellTestBottomHolePressureAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? pressure;
            if (!string.IsNullOrEmpty(testId))
            {
                pressure = await GetEntityAsync("WELL_TEST_PRESSURE", testId, "TEST_NUM");
            }
            else
            {
                pressure = await GetLatestEntityForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(pressure, "BOTTOM_HOLE_PRESSURE", "BHP", "BHFP");
        }

        /// <summary>
        /// Gets bottom hole pressure history from WELL_TEST_PRESSURE table
        /// </summary>
        public async Task<List<(DateTime Date, decimal Value)>> GetWellTestBottomHolePressureHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_TEST_PRESSURE", wellId, "EFFECTIVE_DATE", startDate, endDate);
            return ExtractTimeSeriesData(entities, "EFFECTIVE_DATE", "BOTTOM_HOLE_PRESSURE", "BHP", "BHFP");
        }

        #endregion

        #region Time Series Data Extraction Helpers

        /// <summary>
        /// Extracts time series data from a list of entities
        /// Returns list of (Date, Value) tuples for charting or analysis
        /// </summary>
        private List<(DateTime Date, decimal Value)> ExtractTimeSeriesData(
            List<object> entities,
            string dateFieldName,
            params string[] valueFieldNames)
        {
            var result = new List<(DateTime Date, decimal Value)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, dateFieldName);
                if (!date.HasValue)
                    continue;

                var value = GetPropertyValueMultiple(entity, valueFieldNames);
                if (!value.HasValue)
                    continue;

                result.Add((date.Value, value.Value));
            }

            // Sort by date ascending for time series
            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region WELL Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets total depth from WELL table
        /// PPDM field: TOTAL_DEPTH
        /// </summary>
        public async Task<decimal?> GetWellTotalDepthAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetPropertyValueMultiple(well, "TOTAL_DEPTH", "TD", "FINAL_DEPTH");
        }

        /// <summary>
        /// Gets kelly bushing elevation from WELL table
        /// PPDM field: KB_ELEV
        /// </summary>
        public async Task<decimal?> GetWellKBElevationAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetPropertyValueMultiple(well, "KB_ELEV", "KELLY_BUSHING_ELEV", "KB_ELEVATION");
        }

        /// <summary>
        /// Gets ground elevation from WELL table
        /// PPDM field: GROUND_ELEV
        /// </summary>
        public async Task<decimal?> GetWellGroundElevationAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetPropertyValueMultiple(well, "GROUND_ELEV", "GROUND_ELEVATION", "GL_ELEV");
        }

        /// <summary>
        /// Gets water depth (offshore) from WELL table
        /// PPDM field: WATER_DEPTH
        /// </summary>
        public async Task<decimal?> GetWellWaterDepthAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetPropertyValue(well, "WATER_DEPTH");
        }

        /// <summary>
        /// Gets spud date from WELL table
        /// PPDM field: SPUD_DATE
        /// </summary>
        public async Task<DateTime?> GetWellSpudDateAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetDateValue(well, "SPUD_DATE");
        }

        /// <summary>
        /// Gets completion date from WELL table
        /// PPDM field: COMPLETION_DATE
        /// </summary>
        public async Task<DateTime?> GetWellCompletionDateAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var well = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return GetDateValue(well, "COMPLETION_DATE");
        }

        /// <summary>
        /// Gets wellbore radius/diameter - not standard PPDM field, use custom mapping
        /// Default: 0.25 ft (6 inch diameter)
        /// </summary>
        public async Task<decimal> GetWellboreRadiusAsync(string wellId, decimal defaultValue = 0.25m)
        {
            var mapping = await _defaults.GetFieldMappingAsync("Custom.WellboreRadius");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    if (mapping.ConversionFactor.HasValue)
                        value = value.Value * mapping.ConversionFactor.Value;
                    if (value.Value > 1)
                        return value.Value / 2m;
                    return value.Value;
                }
            }
            return defaultValue;
        }

        #endregion

        #region WELLBORE Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets measured depth from WELLBORE table
        /// PPDM field: MD
        /// </summary>
        public async Task<decimal?> GetWellboreMeasuredDepthAsync(string wellboreId)
        {
            if (string.IsNullOrEmpty(wellboreId))
                return null;

            var wellbore = await GetEntityAsync("WELLBORE", wellboreId, "WELLBORE_ID");
            return GetPropertyValueMultiple(wellbore, "MD", "MEASURED_DEPTH", "TOTAL_MD");
        }

        /// <summary>
        /// Gets true vertical depth from WELLBORE table
        /// PPDM field: TVD
        /// </summary>
        public async Task<decimal?> GetWellboreTrueVerticalDepthAsync(string wellboreId)
        {
            if (string.IsNullOrEmpty(wellboreId))
                return null;

            var wellbore = await GetEntityAsync("WELLBORE", wellboreId, "WELLBORE_ID");
            return GetPropertyValueMultiple(wellbore, "TVD", "TRUE_VERTICAL_DEPTH", "TOTAL_TVD");
        }

        /// <summary>
        /// Gets hole diameter from WELLBORE table
        /// PPDM field: HOLE_DIAMETER
        /// </summary>
        public async Task<decimal?> GetWellboreHoleDiameterAsync(string wellboreId)
        {
            if (string.IsNullOrEmpty(wellboreId))
                return null;

            var wellbore = await GetEntityAsync("WELLBORE", wellboreId, "WELLBORE_ID");
            return GetPropertyValueMultiple(wellbore, "HOLE_DIAMETER", "HOLE_SIZE", "BIT_SIZE");
        }

        /// <summary>
        /// Gets kick-off depth from WELLBORE table (for directional wells)
        /// PPDM field: KICKOFF_DEPTH
        /// </summary>
        public async Task<decimal?> GetWellboreKickoffDepthAsync(string wellboreId)
        {
            if (string.IsNullOrEmpty(wellboreId))
                return null;

            var wellbore = await GetEntityAsync("WELLBORE", wellboreId, "WELLBORE_ID");
            return GetPropertyValueMultiple(wellbore, "KICKOFF_DEPTH", "KOP_DEPTH", "KICK_OFF_DEPTH");
        }

        #endregion

        #region WELL_COMPLETION Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets completion top depth from WELL_COMPLETION table
        /// PPDM field: TOP_DEPTH
        /// </summary>
        public async Task<decimal?> GetCompletionTopDepthAsync(string completionId)
        {
            if (string.IsNullOrEmpty(completionId))
                return null;

            var completion = await GetEntityAsync("WELL_COMPLETION", completionId, "COMPLETION_OBS_NO");
            return GetPropertyValueMultiple(completion, "TOP_DEPTH", "COMPLETION_TOP", "TOP_MD");
        }

        /// <summary>
        /// Gets completion base depth from WELL_COMPLETION table
        /// PPDM field: BASE_DEPTH
        /// </summary>
        public async Task<decimal?> GetCompletionBaseDepthAsync(string completionId)
        {
            if (string.IsNullOrEmpty(completionId))
                return null;

            var completion = await GetEntityAsync("WELL_COMPLETION", completionId, "COMPLETION_OBS_NO");
            return GetPropertyValueMultiple(completion, "BASE_DEPTH", "COMPLETION_BASE", "BASE_MD");
        }

        /// <summary>
        /// Gets completion net pay thickness from WELL_COMPLETION table
        /// PPDM field: NET_PAY
        /// </summary>
        public async Task<decimal?> GetCompletionNetPayAsync(string completionId)
        {
            if (string.IsNullOrEmpty(completionId))
                return null;

            var completion = await GetEntityAsync("WELL_COMPLETION", completionId, "COMPLETION_OBS_NO");
            return GetPropertyValueMultiple(completion, "NET_PAY", "NET_PAY_THICKNESS", "COMPLETION_THICKNESS");
        }

        #endregion

        #region WELL_PERFORATION Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets perforation top depth from WELL_PERFORATION table
        /// </summary>
        public async Task<decimal?> GetPerfTopDepthAsync(string wellId, string? perfId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? perf;
            if (!string.IsNullOrEmpty(perfId))
            {
                perf = await GetEntityAsync("WELL_PERFORATION", perfId, "PERFORATION_OBS_NO");
            }
            else
            {
                perf = await GetLatestEntityForWellAsync("WELL_PERFORATION", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(perf, "TOP_DEPTH", "PERF_TOP", "TOP_MD");
        }

        /// <summary>
        /// Gets perforation base depth from WELL_PERFORATION table
        /// </summary>
        public async Task<decimal?> GetPerfBaseDepthAsync(string wellId, string? perfId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? perf;
            if (!string.IsNullOrEmpty(perfId))
            {
                perf = await GetEntityAsync("WELL_PERFORATION", perfId, "PERFORATION_OBS_NO");
            }
            else
            {
                perf = await GetLatestEntityForWellAsync("WELL_PERFORATION", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(perf, "BASE_DEPTH", "PERF_BASE", "BASE_MD");
        }

        /// <summary>
        /// Gets shots per foot from WELL_PERFORATION table
        /// </summary>
        public async Task<decimal?> GetPerfShotsPerFootAsync(string wellId, string? perfId = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? perf;
            if (!string.IsNullOrEmpty(perfId))
            {
                perf = await GetEntityAsync("WELL_PERFORATION", perfId, "PERFORATION_OBS_NO");
            }
            else
            {
                perf = await GetLatestEntityForWellAsync("WELL_PERFORATION", wellId, "EFFECTIVE_DATE", null);
            }

            return GetPropertyValueMultiple(perf, "SHOTS_PER_FOOT", "SPF", "SHOT_DENSITY");
        }

        /// <summary>
        /// Gets perforation diameter from WELL_PERFORATION table
        /// </summary>
        public async Task<decimal?> GetPerfDiameterAsync(string wellId, string? perfId = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? perf;
            if (!string.IsNullOrEmpty(perfId))
            {
                perf = await GetEntityAsync("WELL_PERFORATION", perfId, "PERFORATION_OBS_NO");
            }
            else
            {
                perf = await GetLatestEntityForWellAsync("WELL_PERFORATION", wellId, "EFFECTIVE_DATE", null);
            }

            return GetPropertyValueMultiple(perf, "HOLE_DIAMETER", "PERF_DIAMETER", "SHOT_SIZE");
        }

        /// <summary>
        /// Gets perforation history
        /// </summary>
        public async Task<List<(DateTime Date, decimal TopDepth, decimal BaseDepth)>> GetPerfHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal, decimal)>();

            var entities = await GetHistoryForWellAsync("WELL_PERFORATION", wellId, "EFFECTIVE_DATE", startDate, endDate);
            var result = new List<(DateTime Date, decimal TopDepth, decimal BaseDepth)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "EFFECTIVE_DATE");
                var top = GetPropertyValueMultiple(entity, "TOP_DEPTH", "PERF_TOP");
                var baseDepth = GetPropertyValueMultiple(entity, "BASE_DEPTH", "PERF_BASE");

                if (date.HasValue && top.HasValue && baseDepth.HasValue)
                    result.Add((date.Value, top.Value, baseDepth.Value));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region WELL_TEST_RECOVERY Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets oil recovery volume from WELL_TEST_RECOVERY table
        /// </summary>
        public async Task<decimal?> GetWellTestOilRecoveryAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? recovery;
            if (!string.IsNullOrEmpty(testId))
            {
                recovery = await GetEntityAsync("WELL_TEST_RECOVERY", testId, "TEST_NUM");
            }
            else
            {
                recovery = await GetLatestEntityForWellAsync("WELL_TEST_RECOVERY", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(recovery, "OIL_RECOVERY", "OIL_VOLUME", "RECOVERED_OIL");
        }

        /// <summary>
        /// Gets gas recovery volume from WELL_TEST_RECOVERY table
        /// </summary>
        public async Task<decimal?> GetWellTestGasRecoveryAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? recovery;
            if (!string.IsNullOrEmpty(testId))
            {
                recovery = await GetEntityAsync("WELL_TEST_RECOVERY", testId, "TEST_NUM");
            }
            else
            {
                recovery = await GetLatestEntityForWellAsync("WELL_TEST_RECOVERY", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(recovery, "GAS_RECOVERY", "GAS_VOLUME", "RECOVERED_GAS");
        }

        /// <summary>
        /// Gets water recovery volume from WELL_TEST_RECOVERY table
        /// </summary>
        public async Task<decimal?> GetWellTestWaterRecoveryAsync(string wellId, string? testId = null, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? recovery;
            if (!string.IsNullOrEmpty(testId))
            {
                recovery = await GetEntityAsync("WELL_TEST_RECOVERY", testId, "TEST_NUM");
            }
            else
            {
                recovery = await GetLatestEntityForWellAsync("WELL_TEST_RECOVERY", wellId, "EFFECTIVE_DATE", asOfDate);
            }

            return GetPropertyValueMultiple(recovery, "WATER_RECOVERY", "WATER_VOLUME", "RECOVERED_WATER");
        }

        #endregion

        #region WELL_TUBULAR Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets casing outer diameter from WELL_TUBULAR table
        /// </summary>
        public async Task<decimal?> GetTubularOuterDiameterAsync(string wellId, string? tubularType = "CASING")
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(tubularType))
            {
                filters.Add(new AppFilter { FieldName = "TUBULAR_TYPE", Operator = "=", FilterValue = tubularType });
            }

            var entities = await GetEntitiesAsync("WELL_TUBULAR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var tubular = entities.FirstOrDefault();

            return GetPropertyValueMultiple(tubular, "OUTER_DIAMETER", "OD", "OUTSIDE_DIAMETER");
        }

        /// <summary>
        /// Gets casing inner diameter from WELL_TUBULAR table
        /// </summary>
        public async Task<decimal?> GetTubularInnerDiameterAsync(string wellId, string? tubularType = "CASING")
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(tubularType))
            {
                filters.Add(new AppFilter { FieldName = "TUBULAR_TYPE", Operator = "=", FilterValue = tubularType });
            }

            var entities = await GetEntitiesAsync("WELL_TUBULAR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var tubular = entities.FirstOrDefault();

            return GetPropertyValueMultiple(tubular, "INNER_DIAMETER", "ID", "INSIDE_DIAMETER");
        }

        /// <summary>
        /// Gets tubing depth from WELL_TUBULAR table
        /// </summary>
        public async Task<decimal?> GetTubularDepthAsync(string wellId, string? tubularType = "TUBING")
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(tubularType))
            {
                filters.Add(new AppFilter { FieldName = "TUBULAR_TYPE", Operator = "=", FilterValue = tubularType });
            }

            var entities = await GetEntitiesAsync("WELL_TUBULAR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var tubular = entities.FirstOrDefault();

            return GetPropertyValueMultiple(tubular, "BASE_DEPTH", "SETTING_DEPTH", "DEPTH");
        }

        #endregion

        #region WELL_CORE Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets core porosity from WELL_CORE table
        /// </summary>
        public async Task<decimal?> GetCorePorosityAsync(string wellId, string? coreId = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? core;
            if (!string.IsNullOrEmpty(coreId))
            {
                core = await GetEntityAsync("WELL_CORE", coreId, "CORE_ID");
            }
            else
            {
                core = await GetLatestEntityForWellAsync("WELL_CORE", wellId, "EFFECTIVE_DATE", null);
            }

            return GetPropertyValueMultiple(core, "POROSITY", "AVG_POROSITY", "CORE_POROSITY");
        }

        /// <summary>
        /// Gets core permeability from WELL_CORE table
        /// </summary>
        public async Task<decimal?> GetCorePermeabilityAsync(string wellId, string? coreId = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? core;
            if (!string.IsNullOrEmpty(coreId))
            {
                core = await GetEntityAsync("WELL_CORE", coreId, "CORE_ID");
            }
            else
            {
                core = await GetLatestEntityForWellAsync("WELL_CORE", wellId, "EFFECTIVE_DATE", null);
            }

            return GetPropertyValueMultiple(core, "PERMEABILITY", "AVG_PERMEABILITY", "CORE_PERM");
        }

        /// <summary>
        /// Gets core saturation from WELL_CORE table
        /// </summary>
        public async Task<decimal?> GetCoreSaturationAsync(string wellId, string? coreId = null, string satType = "OIL")
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            object? core;
            if (!string.IsNullOrEmpty(coreId))
            {
                core = await GetEntityAsync("WELL_CORE", coreId, "CORE_ID");
            }
            else
            {
                core = await GetLatestEntityForWellAsync("WELL_CORE", wellId, "EFFECTIVE_DATE", null);
            }

            return satType.ToUpperInvariant() switch
            {
                "OIL" => GetPropertyValueMultiple(core, "OIL_SATURATION", "SO", "OIL_SAT"),
                "WATER" => GetPropertyValueMultiple(core, "WATER_SATURATION", "SW", "WATER_SAT"),
                "GAS" => GetPropertyValueMultiple(core, "GAS_SATURATION", "SG", "GAS_SAT"),
                _ => null
            };
        }

        #endregion

        #region PRODUCTION Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets oil production volume from PRODUCTION table
        /// </summary>
        public async Task<decimal?> GetProductionOilVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var prod = await GetLatestEntityForWellAsync("PRODUCTION", wellId, "PRODUCTION_DATE", asOfDate);
            return GetPropertyValueMultiple(prod, "OIL_VOLUME", "OIL_PROD", "OIL_PRODUCTION");
        }

        /// <summary>
        /// Gets gas production volume from PRODUCTION table
        /// </summary>
        public async Task<decimal?> GetProductionGasVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var prod = await GetLatestEntityForWellAsync("PRODUCTION", wellId, "PRODUCTION_DATE", asOfDate);
            return GetPropertyValueMultiple(prod, "GAS_VOLUME", "GAS_PROD", "GAS_PRODUCTION");
        }

        /// <summary>
        /// Gets water production volume from PRODUCTION table
        /// </summary>
        public async Task<decimal?> GetProductionWaterVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var prod = await GetLatestEntityForWellAsync("PRODUCTION", wellId, "PRODUCTION_DATE", asOfDate);
            return GetPropertyValueMultiple(prod, "WATER_VOLUME", "WATER_PROD", "WATER_PRODUCTION");
        }

        /// <summary>
        /// Gets production history for DCA and trend analysis
        /// </summary>
        public async Task<List<(DateTime Date, decimal OilRate, decimal GasRate, decimal WaterRate)>> GetProductionHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal, decimal, decimal)>();

            var entities = await GetHistoryForWellAsync("PDEN_VOL_SUMMARY", wellId, "PRODUCTION_DATE", startDate, endDate);
            var result = new List<(DateTime Date, decimal OilRate, decimal GasRate, decimal WaterRate)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "PRODUCTION_DATE");
                if (!date.HasValue) continue;

                var oil = GetPropertyValueMultiple(entity, "OIL_VOLUME", "OIL_PROD") ?? 0m;
                var gas = GetPropertyValueMultiple(entity, "GAS_VOLUME", "GAS_PROD") ?? 0m;
                var water = GetPropertyValueMultiple(entity, "WATER_VOLUME", "WATER_PROD") ?? 0m;

                result.Add((date.Value, oil, gas, water));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        /// <summary>
        /// Gets cumulative oil production from PRODUCTION table
        /// </summary>
        public async Task<decimal> GetCumulativeOilProductionAsync(string wellId, DateTime? upToDate = null)
        {
            var history = await GetProductionHistoryAsync(wellId, null, upToDate);
            return history.Sum(x => x.OilRate);
        }

        /// <summary>
        /// Gets cumulative gas production from PRODUCTION table
        /// </summary>
        public async Task<decimal> GetCumulativeGasProductionAsync(string wellId, DateTime? upToDate = null)
        {
            var history = await GetProductionHistoryAsync(wellId, null, upToDate);
            return history.Sum(x => x.GasRate);
        }

        #endregion

        #region WELL_PRESSURE Table - Known PPDM 3.9 Fields

        /// <summary>
        /// Gets static bottom hole pressure from WELL_PRESSURE table
        /// </summary>
        public async Task<decimal?> GetWellStaticBHPAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var pressure = await GetLatestEntityForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pressure, "STATIC_BHP", "SBHP", "STATIC_BOTTOM_HOLE_PRESSURE");
        }

        /// <summary>
        /// Gets flowing bottom hole pressure from WELL_PRESSURE table
        /// </summary>
        public async Task<decimal?> GetWellFlowingBHPAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var pressure = await GetLatestEntityForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pressure, "FLOWING_BHP", "FBHP", "FLOWING_BOTTOM_HOLE_PRESSURE");
        }

        /// <summary>
        /// Gets tubing head pressure from WELL_PRESSURE table
        /// </summary>
        public async Task<decimal?> GetWellTubingHeadPressureAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var pressure = await GetLatestEntityForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pressure, "TUBING_HEAD_PRESSURE", "THP", "FTP");
        }

        /// <summary>
        /// Gets casing head pressure from WELL_PRESSURE table
        /// </summary>
        public async Task<decimal?> GetWellCasingHeadPressureAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var pressure = await GetLatestEntityForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pressure, "CASING_HEAD_PRESSURE", "CHP", "CASING_PRESSURE");
        }

        /// <summary>
        /// Gets pressure history
        /// </summary>
        public async Task<List<(DateTime Date, decimal? SBHP, decimal? FBHP, decimal? THP)>> GetWellPressureHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, decimal?, decimal?, decimal?)>();

            var entities = await GetHistoryForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE", startDate, endDate);
            var result = new List<(DateTime Date, decimal? SBHP, decimal? FBHP, decimal? THP)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "EFFECTIVE_DATE");
                if (!date.HasValue) continue;

                var sbhp = GetPropertyValueMultiple(entity, "STATIC_BHP", "SBHP");
                var fbhp = GetPropertyValueMultiple(entity, "FLOWING_BHP", "FBHP");
                var thp = GetPropertyValueMultiple(entity, "TUBING_HEAD_PRESSURE", "THP");

                result.Add((date.Value, sbhp, fbhp, thp));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region WELL_LOG / WELL_LOG_CURVE Table - Log Data

        /// <summary>
        /// Gets log top depth from WELL_LOG table
        /// </summary>
        public async Task<decimal?> GetLogTopDepthAsync(string wellId, string? logType = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(logType))
            {
                filters.Add(new AppFilter { FieldName = "LOG_TYPE", Operator = "=", FilterValue = logType });
            }

            var entities = await GetEntitiesAsync("WELL_LOG", filters, "LOG_DATE", DataRetrievalMode.Latest);
            var log = entities.FirstOrDefault();

            return GetPropertyValueMultiple(log, "TOP_DEPTH", "LOG_TOP", "TOP_MD");
        }

        /// <summary>
        /// Gets log base depth from WELL_LOG table
        /// </summary>
        public async Task<decimal?> GetLogBaseDepthAsync(string wellId, string? logType = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(logType))
            {
                filters.Add(new AppFilter { FieldName = "LOG_TYPE", Operator = "=", FilterValue = logType });
            }

            var entities = await GetEntitiesAsync("WELL_LOG", filters, "LOG_DATE", DataRetrievalMode.Latest);
            var log = entities.FirstOrDefault();

            return GetPropertyValueMultiple(log, "BASE_DEPTH", "LOG_BASE", "BASE_MD");
        }

        /// <summary>
        /// Gets average porosity from log curves
        /// </summary>
        public async Task<decimal?> GetLogPorosityAsync(string wellId, string? curveType = "NPHI")
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(curveType))
            {
                filters.Add(new AppFilter { FieldName = "CURVE_ID", Operator = "=", FilterValue = curveType });
            }

            var entities = await GetEntitiesAsync("WELL_LOG_CURVE_VALUE", filters, "DEPTH", DataRetrievalMode.History);
            
            if (!entities.Any())
                return null;

            var values = entities
                .Select(e => GetPropertyValue(e, "CURVE_VALUE"))
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToList();

            return values.Any() ? values.Average() : null;
        }

        #endregion

        #region WELL_DIR_SRVY Table - Directional Survey Data

        /// <summary>
        /// Gets inclination at depth from WELL_DIR_SRVY_STATION table
        /// </summary>
        public async Task<decimal?> GetSurveyInclinationAsync(string wellId, decimal? atDepth = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var entities = await GetEntitiesAsync("WELL_DIR_SRVY_STATION", filters, "DEPTH", DataRetrievalMode.History);

            if (!entities.Any())
                return null;

            if (atDepth.HasValue)
            {
                // Find closest station to specified depth
                var closest = entities
                    .Select(e => new { Entity = e, Depth = GetPropertyValue(e, "DEPTH") ?? 0m })
                    .OrderBy(x => Math.Abs(x.Depth - atDepth.Value))
                    .FirstOrDefault();

                return closest != null ? GetPropertyValueMultiple(closest.Entity, "INCLINATION", "INCL", "ANGLE") : null;
            }

            // Return max inclination (for horizontal wells)
            return entities
                .Select(e => GetPropertyValueMultiple(e, "INCLINATION", "INCL"))
                .Where(v => v.HasValue)
                .Max();
        }

        /// <summary>
        /// Gets azimuth at depth from WELL_DIR_SRVY_STATION table
        /// </summary>
        public async Task<decimal?> GetSurveyAzimuthAsync(string wellId, decimal? atDepth = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var entities = await GetEntitiesAsync("WELL_DIR_SRVY_STATION", filters, "DEPTH", DataRetrievalMode.History);

            if (!entities.Any())
                return null;

            if (atDepth.HasValue)
            {
                var closest = entities
                    .Select(e => new { Entity = e, Depth = GetPropertyValue(e, "DEPTH") ?? 0m })
                    .OrderBy(x => Math.Abs(x.Depth - atDepth.Value))
                    .FirstOrDefault();

                return closest != null ? GetPropertyValueMultiple(closest.Entity, "AZIMUTH", "AZI", "DIRECTION") : null;
            }

            // Return azimuth at deepest point
            var deepest = entities
                .OrderByDescending(e => GetPropertyValue(e, "DEPTH") ?? 0m)
                .FirstOrDefault();

            return GetPropertyValueMultiple(deepest, "AZIMUTH", "AZI");
        }

        /// <summary>
        /// Gets TVD at depth from WELL_DIR_SRVY_STATION table
        /// </summary>
        public async Task<decimal?> GetSurveyTVDAsync(string wellId, decimal? atMD = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var entities = await GetEntitiesAsync("WELL_DIR_SRVY_STATION", filters, "DEPTH", DataRetrievalMode.History);

            if (!entities.Any())
                return null;

            if (atMD.HasValue)
            {
                var closest = entities
                    .Select(e => new { Entity = e, Depth = GetPropertyValue(e, "DEPTH") ?? 0m })
                    .OrderBy(x => Math.Abs(x.Depth - atMD.Value))
                    .FirstOrDefault();

                return closest != null ? GetPropertyValueMultiple(closest.Entity, "TVD", "TRUE_VERTICAL_DEPTH") : null;
            }

            // Return max TVD
            return entities
                .Select(e => GetPropertyValueMultiple(e, "TVD", "TRUE_VERTICAL_DEPTH"))
                .Where(v => v.HasValue)
                .Max();
        }

        /// <summary>
        /// Gets horizontal displacement from survey
        /// </summary>
        public async Task<decimal?> GetSurveyHorizontalDisplacementAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var entities = await GetEntitiesAsync("WELL_DIR_SRVY_STATION", filters, "DEPTH", DataRetrievalMode.History);

            if (!entities.Any())
                return null;

            // Get deepest station
            var deepest = entities
                .OrderByDescending(e => GetPropertyValue(e, "DEPTH") ?? 0m)
                .FirstOrDefault();

            return GetPropertyValueMultiple(deepest, "DEPARTURE", "HORIZONTAL_DISPLACEMENT", "CLOSURE_DISTANCE");
        }

        #endregion

        #region FIELD Table - Field Level Data

        /// <summary>
        /// Gets field area from FIELD table
        /// </summary>
        public async Task<decimal?> GetFieldAreaAsync(string fieldId)
        {
            if (string.IsNullOrEmpty(fieldId))
                return null;

            var field = await GetEntityAsync("FIELD", fieldId, "FIELD_ID");
            return GetPropertyValueMultiple(field, "FIELD_AREA", "AREA", "GROSS_AREA");
        }

        /// <summary>
        /// Gets field discovery date from FIELD table
        /// </summary>
        public async Task<DateTime?> GetFieldDiscoveryDateAsync(string fieldId)
        {
            if (string.IsNullOrEmpty(fieldId))
                return null;

            var field = await GetEntityAsync("FIELD", fieldId, "FIELD_ID");
            return GetDateValue(field, "DISCOVERY_DATE");
        }

        /// <summary>
        /// Gets field OOIP (Original Oil In Place) from FIELD table
        /// </summary>
        public async Task<decimal?> GetFieldOOIPAsync(string fieldId)
        {
            if (string.IsNullOrEmpty(fieldId))
                return null;

            var field = await GetEntityAsync("FIELD", fieldId, "FIELD_ID");
            return GetPropertyValueMultiple(field, "OOIP", "ORIGINAL_OIL_IN_PLACE", "OIL_IN_PLACE");
        }

        /// <summary>
        /// Gets field OGIP (Original Gas In Place) from FIELD table
        /// </summary>
        public async Task<decimal?> GetFieldOGIPAsync(string fieldId)
        {
            if (string.IsNullOrEmpty(fieldId))
                return null;

            var field = await GetEntityAsync("FIELD", fieldId, "FIELD_ID");
            return GetPropertyValueMultiple(field, "OGIP", "ORIGINAL_GAS_IN_PLACE", "GAS_IN_PLACE");
        }

        #endregion

        #region RESERVOIR / RESENT Table - Reserves Data

        /// <summary>
        /// Gets proved oil reserves from RESENT table
        /// </summary>
        public async Task<decimal?> GetProvedOilReservesAsync(string entityId, string entityType = "FIELD")
        {
            if (string.IsNullOrEmpty(entityId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = $"{entityType}_ID", Operator = "=", FilterValue = entityId },
                new AppFilter { FieldName = "RESERVE_CLASS", Operator = "=", FilterValue = "PROVED" }
            };

            var entities = await GetEntitiesAsync("RESENT", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var reserve = entities.FirstOrDefault();

            return GetPropertyValueMultiple(reserve, "OIL_VOLUME", "REMAINING_OIL", "OIL_RESERVES");
        }

        /// <summary>
        /// Gets proved gas reserves from RESENT table
        /// </summary>
        public async Task<decimal?> GetProvedGasReservesAsync(string entityId, string entityType = "FIELD")
        {
            if (string.IsNullOrEmpty(entityId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = $"{entityType}_ID", Operator = "=", FilterValue = entityId },
                new AppFilter { FieldName = "RESERVE_CLASS", Operator = "=", FilterValue = "PROVED" }
            };

            var entities = await GetEntitiesAsync("RESENT", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var reserve = entities.FirstOrDefault();

            return GetPropertyValueMultiple(reserve, "GAS_VOLUME", "REMAINING_GAS", "GAS_RESERVES");
        }

        /// <summary>
        /// Gets reserves history
        /// </summary>
        public async Task<List<(DateTime Date, decimal? Oil, decimal? Gas, string Class)>> GetReservesHistoryAsync(
            string entityId, string entityType = "FIELD", DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(entityId))
                return new List<(DateTime, decimal?, decimal?, string)>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = $"{entityType}_ID", Operator = "=", FilterValue = entityId }
            };

            var entities = await GetEntitiesAsync("RESERVE_ENTITY", filters, "EFFECTIVE_DATE", DataRetrievalMode.History);
            var result = new List<(DateTime Date, decimal? Oil, decimal? Gas, string Class)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "EFFECTIVE_DATE");
                if (!date.HasValue) continue;

                if (startDate.HasValue && date.Value < startDate.Value) continue;
                if (endDate.HasValue && date.Value > endDate.Value) continue;

                var oil = GetPropertyValueMultiple(entity, "OIL_VOLUME", "REMAINING_OIL");
                var gas = GetPropertyValueMultiple(entity, "GAS_VOLUME", "REMAINING_GAS");
                var reserveClass = GetStringValue(entity, "RESERVE_CLASS") ?? "UNKNOWN";

                result.Add((date.Value, oil, gas, reserveClass));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region WELL_FLUID_SAMPLE / ANL_REPORT Table - PVT Data

        /// <summary>
        /// Gets oil API gravity from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidAPIGravityAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "API_GRAVITY", "OIL_GRAVITY", "API");
        }

        /// <summary>
        /// Gets gas-oil ratio from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidGORAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "GOR", "GAS_OIL_RATIO", "SOLUTION_GOR");
        }

        /// <summary>
        /// Gets water cut from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidWaterCutAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "WATER_CUT", "BSW", "BS_AND_W");
        }

        /// <summary>
        /// Gets oil viscosity from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidOilViscosityAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "OIL_VISCOSITY", "VISCOSITY", "DEAD_OIL_VISCOSITY");
        }

        /// <summary>
        /// Gets bubble point pressure from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidBubblePointAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "BUBBLE_POINT", "BUBBLE_POINT_PRESSURE", "SATURATION_PRESSURE");
        }

        /// <summary>
        /// Gets formation volume factor from fluid analysis
        /// </summary>
        public async Task<decimal?> GetFluidFVFAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var sample = await GetLatestEntityForWellAsync("WELL_FLUID_SAMPLE", wellId, "SAMPLE_DATE", asOfDate);
            return GetPropertyValueMultiple(sample, "FVF", "FORMATION_VOLUME_FACTOR", "OIL_FVF", "BO");
        }

        #endregion

        #region WELL_TREATMENT Table - Stimulation Data

        /// <summary>
        /// Gets treatment type from WELL_TREATMENT table
        /// </summary>
        public async Task<string?> GetTreatmentTypeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetStringValue(treatment, "TREATMENT_TYPE");
        }

        /// <summary>
        /// Gets proppant volume from WELL_TREATMENT table (for frac jobs)
        /// </summary>
        public async Task<decimal?> GetTreatmentProppantVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "PROPPANT_VOLUME", "SAND_VOLUME", "PROPPANT_MASS");
        }

        /// <summary>
        /// Gets fluid volume from WELL_TREATMENT table
        /// </summary>
        public async Task<decimal?> GetTreatmentFluidVolumeAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "FLUID_VOLUME", "TREATMENT_VOLUME", "TOTAL_FLUID");
        }

        /// <summary>
        /// Gets maximum treatment pressure from WELL_TREATMENT table
        /// </summary>
        public async Task<decimal?> GetTreatmentMaxPressureAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var treatment = await GetLatestEntityForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", asOfDate);
            return GetPropertyValueMultiple(treatment, "MAX_PRESSURE", "TREATING_PRESSURE", "MAX_TREATING_PRESSURE");
        }

        /// <summary>
        /// Gets treatment history
        /// </summary>
        public async Task<List<(DateTime Date, string Type, decimal? Volume)>> GetTreatmentHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, string, decimal?)>();

            var entities = await GetHistoryForWellAsync("WELL_TREATMENT", wellId, "TREATMENT_DATE", startDate, endDate);
            var result = new List<(DateTime Date, string Type, decimal? Volume)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "TREATMENT_DATE");
                if (!date.HasValue) continue;

                var type = GetStringValue(entity, "TREATMENT_TYPE") ?? "UNKNOWN";
                var volume = GetPropertyValueMultiple(entity, "FLUID_VOLUME", "TREATMENT_VOLUME");

                result.Add((date.Value, type, volume));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region STRAT_UNIT / WELL_STRAT_UNIT_INTPR Table - Stratigraphy Data

        /// <summary>
        /// Gets formation top depth from WELL_STRAT_UNIT_INTPR table
        /// </summary>
        public async Task<decimal?> GetFormationTopDepthAsync(string wellId, string stratUnitId)
        {
            if (string.IsNullOrEmpty(wellId) || string.IsNullOrEmpty(stratUnitId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "STRAT_UNIT_ID", Operator = "=", FilterValue = stratUnitId }
            };

            var entities = await GetEntitiesAsync("WELL_STRAT_UNIT_INTPR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var interp = entities.FirstOrDefault();

            return GetPropertyValueMultiple(interp, "TOP_DEPTH", "FORMATION_TOP", "TOP_MD");
        }

        /// <summary>
        /// Gets formation base depth from WELL_STRAT_UNIT_INTPR table
        /// </summary>
        public async Task<decimal?> GetFormationBaseDepthAsync(string wellId, string stratUnitId)
        {
            if (string.IsNullOrEmpty(wellId) || string.IsNullOrEmpty(stratUnitId))
                return null;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "STRAT_UNIT_ID", Operator = "=", FilterValue = stratUnitId }
            };

            var entities = await GetEntitiesAsync("WELL_STRAT_UNIT_INTPR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            var interp = entities.FirstOrDefault();

            return GetPropertyValueMultiple(interp, "BASE_DEPTH", "FORMATION_BASE", "BASE_MD");
        }

        /// <summary>
        /// Gets formation thickness from WELL_STRAT_UNIT_INTPR table
        /// </summary>
        public async Task<decimal?> GetFormationThicknessAsync(string wellId, string stratUnitId)
        {
            if (string.IsNullOrEmpty(wellId) || string.IsNullOrEmpty(stratUnitId))
                return null;

            var top = await GetFormationTopDepthAsync(wellId, stratUnitId);
            var baseDepth = await GetFormationBaseDepthAsync(wellId, stratUnitId);

            if (top.HasValue && baseDepth.HasValue)
                return baseDepth.Value - top.Value;

            return null;
        }

        /// <summary>
        /// Gets all formations penetrated by well
        /// </summary>
        public async Task<List<(string StratUnitId, decimal TopDepth, decimal BaseDepth)>> GetWellFormationsAsync(string wellId)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(string, decimal, decimal)>();

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            var entities = await GetEntitiesAsync("WELL_STRAT_UNIT_INTPR", filters, "TOP_DEPTH", DataRetrievalMode.History);
            var result = new List<(string StratUnitId, decimal TopDepth, decimal BaseDepth)>();

            foreach (var entity in entities)
            {
                var stratId = GetStringValue(entity, "STRAT_UNIT_ID");
                var top = GetPropertyValueMultiple(entity, "TOP_DEPTH", "FORMATION_TOP");
                var baseDepth = GetPropertyValueMultiple(entity, "BASE_DEPTH", "FORMATION_BASE");

                if (!string.IsNullOrEmpty(stratId) && top.HasValue && baseDepth.HasValue)
                    result.Add((stratId, top.Value, baseDepth.Value));
            }

            return result.OrderBy(x => x.TopDepth).ToList();
        }

        #endregion

        #region PDEN_VOL_SUMMARY Table - Production Volumes Summary

        /// <summary>
        /// Gets cumulative oil from PDEN_VOL_SUMMARY table
        /// </summary>
        public async Task<decimal?> GetPDENCumulativeOilAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId))
                return null;

            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_OIL", "CUMULATIVE_OIL", "CUM_OIL_PROD");
        }

        /// <summary>
        /// Gets cumulative gas from PDEN_VOL_SUMMARY table
        /// </summary>
        public async Task<decimal?> GetPDENCumulativeGasAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId))
                return null;

            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_GAS", "CUMULATIVE_GAS", "CUM_GAS_PROD");
        }

        /// <summary>
        /// Gets cumulative water from PDEN_VOL_SUMMARY table
        /// </summary>
        public async Task<decimal?> GetPDENCumulativeWaterAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId))
                return null;

            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "CUM_WATER", "CUMULATIVE_WATER", "CUM_WATER_PROD");
        }

        /// <summary>
        /// Gets on-production days from PDEN_VOL_SUMMARY table
        /// </summary>
        public async Task<decimal?> GetPDENOnProdDaysAsync(string pdenId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(pdenId))
                return null;

            var pden = await GetLatestEntityForWellAsync("PDEN_VOL_SUMMARY", pdenId, "EFFECTIVE_DATE", asOfDate);
            return GetPropertyValueMultiple(pden, "ON_PROD_DAYS", "PRODUCING_DAYS", "DAYS_ON");
        }

        #endregion

        #region WELL_STATUS Table - Well Status Data

        /// <summary>
        /// Gets current well status from WELL_STATUS table
        /// </summary>
        public async Task<string?> GetWellStatusAsync(string wellId, DateTime? asOfDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return null;

            var status = await GetLatestEntityForWellAsync("WELL_STATUS", wellId, "EFFECTIVE_DATE", asOfDate);
            return GetStringValue(status, "STATUS_TYPE");
        }

        /// <summary>
        /// Gets well status history
        /// </summary>
        public async Task<List<(DateTime Date, string Status)>> GetWellStatusHistoryAsync(
            string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<(DateTime, string)>();

            var entities = await GetHistoryForWellAsync("WELL_STATUS", wellId, "EFFECTIVE_DATE", startDate, endDate);
            var result = new List<(DateTime Date, string Status)>();

            foreach (var entity in entities)
            {
                var date = GetDateValue(entity, "EFFECTIVE_DATE");
                var status = GetStringValue(entity, "STATUS_TYPE");

                if (date.HasValue && !string.IsNullOrEmpty(status))
                    result.Add((date.Value, status));
            }

            return result.OrderBy(x => x.Date).ToList();
        }

        #endregion

        #region Helper - Get String Value

        /// <summary>
        /// Gets string property value from entity
        /// </summary>
        private string? GetStringValue(object? entity, string propertyName)
        {
            if (entity == null || string.IsNullOrEmpty(propertyName))
                return null;

            try
            {
                var prop = entity.GetType().GetProperty(propertyName);
                return prop?.GetValue(entity)?.ToString();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Custom Field Mapping Support

        /// <summary>
        /// Gets a value using FieldMappingConfig (for custom/extension fields not in standard PPDM)
        /// </summary>
        public async Task<decimal?> GetCustomFieldValueAsync(string mappingKey, string entityId)
        {
            var mapping = await _defaults.GetFieldMappingAsync(mappingKey);
            if (mapping == null || !mapping.IsActive || string.IsNullOrEmpty(mapping.TableName))
                return null;

            // Determine the ID field name based on table
            var idFieldName = mapping.TableName.ToUpperInvariant() switch
            {
                "WELL" => "WELL_ID",
                "POOL" => "POOL_ID",
                "FIELD" => "FIELD_ID",
                "WELLBORE" => "WELLBORE_ID",
                "WELL_COMPLETION" => "COMPLETION_ID",
                "WELL_TEST" => "TEST_NUM",
                "WELL_TEST_ANALYSIS" => "TEST_NUM",
                _ => "ID"
            };

            var entity = await GetEntityAsync(mapping.TableName, entityId, idFieldName);
            if (entity == null)
                return ConvertToDecimalHelper(mapping.DefaultValue);

            // Apply conditions if specified
            if (mapping.Conditions != null && mapping.Conditions.Count > 0)
            {
                var entityType = entity.GetType();
                foreach (var condition in mapping.Conditions)
                {
                    var conditionProp = entityType.GetProperty(condition.Key);
                    if (conditionProp == null)
                        return ConvertToDecimalHelper(mapping.DefaultValue);

                    var conditionValue = conditionProp.GetValue(entity);
                    if (conditionValue?.ToString() != condition.Value?.ToString())
                        return ConvertToDecimalHelper(mapping.DefaultValue);
                }
            }

            var value = GetPropertyValue(entity, mapping.FieldName);
            
            // Apply conversion factor if specified
            if (value.HasValue && mapping.ConversionFactor.HasValue)
                value = value.Value * mapping.ConversionFactor.Value;

            return value ?? ConvertToDecimalHelper(mapping.DefaultValue);
        }

        /// <summary>
        /// Clears the entity cache (call between operations if needed)
        /// </summary>
        public void ClearEntityCache()
        {
            _entityCache.Clear();
        }

        #endregion

        #endregion

        /// <summary>
        /// Retrieves reservoir properties from PPDM database for physics-based forecasting.
        /// Uses KNOWN PPDM 3.9 fields directly from standard tables:
        /// - POOL: reservoir properties (pressure, porosity, permeability, thickness, etc.)
        /// - WELL_TEST_ANALYSIS: calculated test results (skin, permeability from test)
        /// - WELL_TEST_FLOW: flow rates
        /// - WELL_TEST_PRESSURE: pressure measurements
        /// For custom/extension fields, use FieldMappingConfig via GetCustomFieldValueAsync()
        /// </summary>
        private async Task<ReservoirForecastProperties?> GetReservoirPropertiesForForecastAsync(DCARequest request)
        {
            if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId))
            {
                throw new ArgumentException("WellId or PoolId is required for physics-based forecasting");
            }

            // Clear entity cache for fresh data
            ClearEntityCache();
            _entityListCache.Clear();

            var poolId = request.PoolId ?? string.Empty;
            var wellId = request.WellId ?? string.Empty;
            
            // Optional: Get specific test by ID, or get data as of a specific date
            var testId = request.AdditionalParameters?.ContainsKey("TestId") == true
                ? request.AdditionalParameters["TestId"]?.ToString()
                : null;
            
            DateTime? asOfDate = null;
            if (request.AdditionalParameters?.ContainsKey("AsOfDate") == true)
            {
                var dateStr = request.AdditionalParameters["AsOfDate"]?.ToString();
                if (DateTime.TryParse(dateStr, out var parsedDate))
                    asOfDate = parsedDate;
            }

            // Use KNOWN PPDM fields from standard tables
            // POOL data - static reservoir properties (don't change over time)
            var reservoirProps = new ReservoirForecastProperties
            {
                // From POOL table - known PPDM 3.9 fields
                InitialPressure = await GetPoolInitialPressureAsync(poolId, 3000m),
                Permeability = await GetPoolPermeabilityAsync(poolId, 100m),
                Thickness = await GetPoolThicknessAsync(poolId, 50m),
                Porosity = await GetPoolPorosityAsync(poolId, 0.2m),
                Temperature = await GetPoolTemperatureAsync(poolId, 560m),
                TotalCompressibility = await GetPoolCompressibilityAsync(poolId, 0.00001m),
                DrainageRadius = await GetPoolDrainageRadiusAsync(poolId, 1000m),
                FormationVolumeFactor = await GetPoolFormationVolumeFactorAsync(poolId, 1.2m),
                OilViscosity = await GetPoolOilViscosityAsync(poolId, 1.0m),
                GasSpecificGravity = await GetPoolGasGravityAsync(poolId, 0.65m),
                
                // Wellbore radius - not standard PPDM field, use custom mapping
                WellboreRadius = await GetWellboreRadiusAsync(wellId, 0.25m),
                
                // Skin factor from WELL_TEST_ANALYSIS - get latest or by specific test/date
                // null testId = get latest test automatically
                SkinFactor = await GetWellTestSkinAsync(wellId, testId, asOfDate) ?? 0m
            };

            // Override pool permeability with test-derived permeability if available
            // This is more accurate as it comes from actual well test analysis
            var testPerm = await GetWellTestPermeabilityAsync(wellId, testId, asOfDate);
            if (testPerm.HasValue)
                reservoirProps.Permeability = testPerm.Value;

            return reservoirProps;
        }

        /// <summary>
        /// Converts object to decimal, handling various numeric types
        /// </summary>
        private decimal? ConvertToDecimalHelper(object? value)
        {
            if (value == null)
                return null;
                
            if (value is decimal dec)
                return dec;
            if (value is double d)
                return (decimal)d;
            if (value is float f)
                return (decimal)f;
            if (value is int i)
                return i;
            if (value is long l)
                return l;
            if (decimal.TryParse(value.ToString(), out var parsed))
                return parsed;
                
            return null;
        }

        /// <summary>
        /// Maps ProductionForecast from ProductionForecasting library to DCAResult DTO
        /// </summary>
        private DCAResult MapProductionForecastToDCAResult(ProductionForecast forecast, DCARequest request)
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
                AdditionalResults = new Dictionary<string, object>()
            };

            // Map forecast parameters
            result.InitialRate = forecast.InitialProductionRate;
            result.EstimatedEUR = forecast.TotalCumulativeProduction;
            result.DeclineRate = forecast.ForecastPoints.Count > 1
                ? (forecast.InitialProductionRate - forecast.FinalProductionRate) / 
                  (forecast.InitialProductionRate * (forecast.ForecastDuration / 365.25m))
                : null;

            // Map forecast points (convert time in days to actual dates)
            var startDate = request.StartDate ?? DateTime.UtcNow;
            foreach (var point in forecast.ForecastPoints)
            {
                result.ForecastPoints.Add(new DCAForecastPoint
                {
                    Date = startDate.AddDays((double)point.Time),
                    ProductionRate = point.ProductionRate,
                    CumulativeProduction = point.CumulativeProduction,
                    DeclineRate = null // Not calculated for physics-based forecasts
                });
            }

            // Store additional forecast metadata
            result.AdditionalResults = new Dictionary<string, object>
            {
                ["ForecastType"] = forecast.ForecastType.ToString(),
                ["ForecastDuration"] = forecast.ForecastDuration,
                ["InitialProductionRate"] = forecast.InitialProductionRate,
                ["FinalProductionRate"] = forecast.FinalProductionRate,
                ["TotalCumulativeProduction"] = forecast.TotalCumulativeProduction,
                ["ForecastPointCount"] = forecast.ForecastPoints.Count
            };

            return result;
        }
    }
}
