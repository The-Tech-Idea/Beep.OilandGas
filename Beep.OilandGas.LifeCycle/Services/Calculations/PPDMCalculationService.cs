using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.DCA;
using Beep.OilandGas.DCA.Results;

using Beep.OilandGas.EconomicAnalysis;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.NodalAnalysis;
using Beep.OilandGas.NodalAnalysis.Calculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.WellTestAnalysis;

using Beep.OilandGas.FlashCalculations;
using Beep.OilandGas.FlashCalculations.Calculations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.ChokeAnalysis;
using Beep.OilandGas.ChokeAnalysis.Calculations;

using Beep.OilandGas.GasLift;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.PumpPerformance;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.SuckerRodPumping;
using Beep.OilandGas.SuckerRodPumping.Calculations;

using Beep.OilandGas.CompressorAnalysis;
using Beep.OilandGas.CompressorAnalysis.Calculations;

using Beep.OilandGas.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;

using Beep.OilandGas.PlungerLift;
using Beep.OilandGas.PlungerLift.Calculations;

using Beep.OilandGas.HydraulicPumps;
using Beep.OilandGas.HydraulicPumps.Calculations;

using Beep.OilandGas.LifeCycle.Services.DataMapping;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.Models.Data.Pumps;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.CompressorAnalysis;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.Models.Data.Calculations;
using EconomicAnalysisResult = Beep.OilandGas.Models.Data.Calculations.EconomicAnalysisResult;
using Beep.OilandGas.ProductionForecasting.Calculations;
using Beep.OilandGas.PPDM.Models;


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
        private readonly IFieldMappingService _fieldMappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMCalculationService>? _logger;

        // Cache for repositories
        private PPDMGenericRepository? _dcaResultRepository;
        private PPDMGenericRepository? _economicResultRepository;
        private PPDMGenericRepository? _nodalResultRepository;
        private PPDMGenericRepository? _wellTestResultRepository;
        private PPDMGenericRepository? _flashResultRepository;

        public PPDMCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IFieldMappingService fieldMappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMCalculationService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _fieldMappingService = fieldMappingService ?? throw new ArgumentNullException(nameof(fieldMappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        /// <summary>
        /// Gets or creates repository for DCA results
        /// Note: Uses metadata to resolve entity type and table name
        /// </summary>
        private async Task<PPDMGenericRepository> GetDCAResultRepositoryAsync()
        {
            if (_dcaResultRepository == null)
            {
                // Try to get entity type from metadata
                var metadata = await _metadata.GetTableMetadataAsync("DCA_CALCULATION");
                var tableName = metadata?.TableName ?? "DCA_CALCULATION";
                var entityType = ResolvePPDMEntityType(metadata?.EntityTypeName, tableName);

                _dcaResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    tableName,
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
                var tableName = metadata?.TableName ?? "ECONOMIC_ANALYSIS";
                var entityType = ResolvePPDMEntityType(metadata?.EntityTypeName, tableName);

                _economicResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    tableName,
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
                var tableName = metadata?.TableName ?? "NODAL_ANALYSIS";
                var entityType = ResolvePPDMEntityType(metadata?.EntityTypeName, tableName);

                _nodalResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    tableName,
                    null);
            }
            return _nodalResultRepository;
        }

        /// <summary>
        /// Gets or creates repository for Well Test Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetWellTestResultRepositoryAsync()
        {
            if (_wellTestResultRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_TEST");
                var tableName = metadata?.TableName ?? "WELL_TEST";
                var entityType = ResolvePPDMEntityType(metadata?.EntityTypeName, tableName);

                _wellTestResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    tableName,
                    null);
            }
            return _wellTestResultRepository;
        }

        /// <summary>
        /// Gets or creates repository for Flash Calculation results
        /// </summary>
        private async Task<PPDMGenericRepository> GetFlashResultRepositoryAsync()
        {
            if (_flashResultRepository == null)
            {
                var metadata = await _metadata.GetTableMetadataAsync("FLASH_CALCULATION");
                var tableName = metadata?.TableName ?? "FLASH_CALCULATION";
                var entityType = ResolvePPDMEntityType(metadata?.EntityTypeName, tableName);

                _flashResultRepository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    _connectionName,
                    tableName,
                    null);
            }
            return _flashResultRepository;
        }
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> EntityPropertyCache =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();
        private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> ResultPropertyCache =
            new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();

        private static Type? FindEntityTypeByName(string? entityTypeName)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
            {
                return null;
            }

            var ppdmAssembly = typeof(ANL_ANALYSIS_REPORT).Assembly;
            var modelAssembly = typeof(ModelEntityBase).Assembly;

            return FindEntityTypeInAssembly(ppdmAssembly, entityTypeName)
                ?? FindEntityTypeInAssembly(modelAssembly, entityTypeName);
        }

        private static Type? FindEntityTypeInAssembly(Assembly assembly, string entityTypeName)
        {
            var ppdmQualified = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{entityTypeName}", false, true);
            if (ppdmQualified != null && typeof(IPPDMEntity).IsAssignableFrom(ppdmQualified))
            {
                return ppdmQualified;
            }

            var modelQualified = assembly.GetType($"Beep.OilandGas.Models.Data.{entityTypeName}", false, true);
            if (modelQualified != null && typeof(IPPDMEntity).IsAssignableFrom(modelQualified))
            {
                return modelQualified;
            }

            var normalizedTarget = NormalizePropertyName(entityTypeName);
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(type => type != null).ToArray()!;
            }

            return types.FirstOrDefault(type =>
                type != null &&
                typeof(IPPDMEntity).IsAssignableFrom(type) &&
                NormalizePropertyName(type.Name).Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));
        }

        private static Type ResolvePPDMEntityType(string? entityTypeName, string tableName)
        {
            var resolvedType = FindEntityTypeByName(entityTypeName)
                ?? FindEntityTypeByName(tableName);

            if (resolvedType == null)
            {
                throw new InvalidOperationException(
                    $"Entity type not found for table {tableName} (entity type {entityTypeName ?? "unknown"}).");
            }

            return resolvedType;
        }

        private static string NormalizePropertyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var sanitized = name.Where(char.IsLetterOrDigit).ToArray();
            return new string(sanitized).ToUpperInvariant();
        }

        private static IReadOnlyDictionary<string, PropertyInfo> GetEntityPropertyMap(Type entityType)
        {
            return EntityPropertyCache.GetOrAdd(entityType, type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanWrite)
                    .ToDictionary(prop => NormalizePropertyName(prop.Name), prop => prop));
        }

        private static IReadOnlyDictionary<string, PropertyInfo> GetReadablePropertyMap(Type resultType)
        {
            return ResultPropertyCache.GetOrAdd(resultType, type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanRead)
                    .ToDictionary(prop => NormalizePropertyName(prop.Name), prop => prop));
        }

        private static bool TryResolveResultPropertyAlias(
            string normalizedEntityProperty,
            IReadOnlyDictionary<string, PropertyInfo> resultPropertyMap,
            out PropertyInfo resultProperty)
        {
            resultProperty = null!;

            if (normalizedEntityProperty.EndsWith("ANALYSISID", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("CALCULATIONID", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("RESULTID", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("RESULTID", out resultProperty))
                {
                    return true;
                }
            }

            if (normalizedEntityProperty.Equals("WELLTESTID", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONID", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISID", out resultProperty))
                {
                    return true;
                }
            }

            if (normalizedEntityProperty.EndsWith("ANALYSISDATE", StringComparison.OrdinalIgnoreCase) ||
                normalizedEntityProperty.EndsWith("CALCULATIONDATE", StringComparison.OrdinalIgnoreCase))
            {
                if (resultPropertyMap.TryGetValue("CALCULATIONDATE", out resultProperty) ||
                    resultPropertyMap.TryGetValue("ANALYSISDATE", out resultProperty))
                {
                    return true;
                }
            }

            return false;
        }

        private object CreateEntityFromResult(PPDMGenericRepository repository, object result)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var entityType = repository.EntityType;
            if (entityType.IsAssignableFrom(result.GetType()))
            {
                return result;
            }

            var entity = Activator.CreateInstance(entityType)
                ?? throw new InvalidOperationException($"Unable to create entity of type {entityType.Name}.");

            var propertyMap = GetEntityPropertyMap(entityType);
            var resultPropertyMap = GetReadablePropertyMap(result.GetType());

            foreach (var entityProperty in propertyMap)
            {
                if (!resultPropertyMap.TryGetValue(entityProperty.Key, out var resultProperty))
                {
                    if (!TryResolveResultPropertyAlias(entityProperty.Key, resultPropertyMap, out resultProperty))
                    {
                        continue;
                    }
                }

                var value = resultProperty.GetValue(result);
                TrySetPropertyValue(entity, entityProperty.Value, value);
            }

            return entity;
        }

        private bool TrySetPropertyValue(object entity, PropertyInfo property, object? value)
        {
            try
            {
                if (value == null || value == DBNull.Value)
                {
                    property.SetValue(entity, null);
                    return true;
                }

                if (value is JsonElement jsonElement)
                {
                    value = jsonElement.ValueKind == JsonValueKind.String
                        ? jsonElement.GetString()
                        : jsonElement.ToString();
                }

                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (value is Enum enumValue && targetType == typeof(string))
                {
                    value = enumValue.ToString();
                }
                else if (targetType.IsEnum && value is string enumText)
                {
                    value = Enum.Parse(targetType, enumText, true);
                }
                else if (targetType.IsEnum && value != null && !targetType.IsInstanceOfType(value))
                {
                    value = Enum.ToObject(targetType, value);
                }

                if (!targetType.IsInstanceOfType(value))
                {
                    value = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                }

                property.SetValue(entity, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(
                    ex,
                    "Failed to set {PropertyName} on {EntityType}",
                    property.Name,
                    entity.GetType().Name);
                return false;
            }
        }

        private async Task InsertAnalysisResultAsync(
            PPDMGenericRepository repository,
            object result,
            string? userId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            var entity = CreateEntityFromResult(repository, result);
            var effectiveUserId = string.IsNullOrWhiteSpace(userId) ? "SYSTEM" : userId;
            await repository.InsertAsync(entity, effectiveUserId);
        }

        private async Task<PPDMGenericRepository> CreateAnalysisResultRepositoryAsync(
            string primaryTableName,
            string fallbackTableName = "ANL_ANALYSIS_REPORT")
        {
            if (string.IsNullOrWhiteSpace(primaryTableName))
            {
                throw new ArgumentException("Primary table name must be provided", nameof(primaryTableName));
            }

            var metadata = await _metadata.GetTableMetadataAsync(primaryTableName);
            if (metadata == null && !string.IsNullOrWhiteSpace(fallbackTableName))
            {
                metadata = await _metadata.GetTableMetadataAsync(fallbackTableName);
            }

            if (metadata == null)
            {
                throw new InvalidOperationException($"{primaryTableName} or {fallbackTableName} table not found");
            }

            var tableName = string.IsNullOrWhiteSpace(metadata.TableName) ? primaryTableName : metadata.TableName;
            var entityType = ResolvePPDMEntityType(metadata.EntityTypeName, tableName);

            return new PPDMGenericRepository(
                _editor,
                _commonColumnHandler,
                _defaults,
                _metadata,
                entityType,
                _connectionName,
                tableName,
                null);
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
                AdditionalResults = new DcaAdditionalResults()
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
                    DeclineRate = (decimal)(di * 100.0) // As percentage
                });
            }

            return forecastPoints;
        }

        /// <summary>
        /// Performs economic analysis (NPV, IRR, Payback Period, ROI, etc.) for a well, pool, field, or project.
        /// Supports building cash flows from production forecast in request or from PPDM production data.
        /// </summary>
        /// <param name="request">Economic analysis request containing entity IDs, economic parameters, and optional production forecast</param>
        /// <returns>Economic analysis result with NPV, IRR, payback period, cash flows, and additional metrics</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when cash flow data is unavailable or calculation fails</exception>
        public async Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId) && 
                    string.IsNullOrEmpty(request.FieldId) && string.IsNullOrEmpty(request.ProjectId))
                {
                    throw new ArgumentException("At least one of WellId, PoolId, FieldId, or ProjectId must be provided");
                }

                _logger?.LogInformation("Starting Economic Analysis for WellId: {WellId}, PoolId: {PoolId}, FieldId: {FieldId}, ProjectId: {ProjectId}",
                    request.WellId, request.PoolId, request.FieldId, request.ProjectId);

                // Step 1: Build cash flows from request or PPDM data
                CashFlow[] cashFlows;
                if (request.ProductionForecast != null && request.ProductionForecast.Count > 0)
                {
                    // Build cash flows from production forecast in request
                    cashFlows = BuildCashFlowsFromProductionForecast(request);
                }
                else
                {
                    // Build cash flows from PPDM data
                    cashFlows = await BuildCashFlowsFromPPDMDataAsync(request);
                }

                if (cashFlows == null || cashFlows.Length == 0)
                {
                    throw new InvalidOperationException("No cash flow data available for economic analysis. Provide ProductionForecast or ensure PPDM data is available.");
                }

                // Step 2: Validate discount rate
                double discountRate = request.DiscountRate.HasValue 
                    ? (double)request.DiscountRate.Value / 100.0  // Convert percentage to decimal
                    : 0.10; // Default 10%

                if (discountRate < 0 || discountRate > 1)
                {
                    throw new ArgumentException("Discount rate must be between 0 and 100 percent");
                }

                // Step 3: Perform economic analysis
                double financeRate = request.AdditionalParameters?.FinanceRate != null
                    ? request.AdditionalParameters.FinanceRate.Value / 100.0
                    : 0.08;

                double reinvestRate = request.AdditionalParameters?.ReinvestRate != null
                    ? request.AdditionalParameters.ReinvestRate.Value / 100.0
                    : 0.12;

                EconomicResult economicResult;
                try
                {
                    economicResult = EconomicAnalyzer.Analyze(cashFlows, discountRate, financeRate, reinvestRate);
                }
                catch (Exception calcEx)
                {
                    _logger?.LogError(calcEx, "Error in economic calculation");
                    throw new InvalidOperationException($"Economic calculation failed: {calcEx.Message}", calcEx);
                }

                // Step 4: Generate NPV profile if requested
                List<NPVProfilePoint>? npvProfile = null;
                if (request.AdditionalParameters?.GenerateNpvProfile == true)
                {
                    double minRate = request.AdditionalParameters?.NpvProfileMinRate != null
                        ? request.AdditionalParameters.NpvProfileMinRate.Value / 100.0
                        : 0.0;
                    double maxRate = request.AdditionalParameters?.NpvProfileMaxRate != null
                        ? request.AdditionalParameters.NpvProfileMaxRate.Value / 100.0
                        : 0.5;
                    int points = request.AdditionalParameters?.NpvProfilePoints ?? 50;

                    npvProfile = EconomicAnalyzer.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
                }

                // Step 5: Map EconomicResult to EconomicAnalysisResult DTO
                var result = MapEconomicResultToDTO(economicResult, request, cashFlows, npvProfile);

                // Step 6: Store result in database
                var repository = await GetEconomicResultRepositoryAsync();

                await repository.InsertAsync(result, request.UserId ?? "system");

                _logger?.LogInformation("Economic Analysis calculation completed: {CalculationId}, NPV: {NPV}, IRR: {IRR}%",
                    result.CalculationId, result.NetPresentValue, result.InternalRateOfReturn);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Economic Analysis");

                // Return error result
                var errorResult = new EconomicAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    PoolId = request.PoolId,
                    FieldId = request.FieldId,
                    ProjectId = request.ProjectId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    CashFlowPoints = new List<EconomicCashFlowPoint>(),
                    AdditionalResults = new EconomicAnalysisAdditionalResults()
                };

                // Try to store error result
                try
                {
                    var repository = await GetEconomicResultRepositoryAsync();

                    await repository.InsertAsync(errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Economic Analysis error result");
                }

                throw;
            }
        }

        /// <summary>
        /// Performs nodal analysis (IPR/VLP) for a well to determine operating point and production optimization.
        /// Supports building reservoir and wellbore properties from request parameters or from PPDM data.
        /// </summary>
        /// <param name="request">Nodal analysis request containing well ID, reservoir/wellbore properties, and analysis parameters</param>
        /// <returns>Nodal analysis result with IPR/VLP curves, operating point, performance metrics, and recommendations</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when reservoir/wellbore properties are unavailable or calculation fails</exception>
        public async Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request)
        {
            string? resolvedWellId = null;

            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellUWI))
                {
                    throw new ArgumentException("WellUWI must be provided");
                }

                resolvedWellId = await GetWellIdByUwiAsync(request.WellUWI);
                if (string.IsNullOrEmpty(resolvedWellId))
                {
                    throw new InvalidOperationException($"Well not found for UWI: {request.WellUWI}");
                }

                _logger?.LogInformation("Starting Nodal Analysis for WellUWI: {WellUWI}, WellId: {WellId}",
                    request.WellUWI, resolvedWellId);

                // Step 1: Build reservoir properties from request or PPDM data
                ReservoirProperties reservoirProperties;
                if (request.ReservoirPressure.HasValue && request.ProductivityIndex.HasValue)
                {
                    // Use values from request
                    reservoirProperties = new ReservoirProperties
                    {
                        ReservoirPressure = (double)request.ReservoirPressure.Value,
                        BubblePointPressure = request.AdditionalParameters?.BubblePointPressure != null
                            ? (double)request.AdditionalParameters.BubblePointPressure.Value
                            : (double)(request.ReservoirPressure.Value * 0.8m),
                        ProductivityIndex = (double)request.ProductivityIndex.Value,
                        WaterCut = request.WaterCut.HasValue ? (double)request.WaterCut.Value / 100.0 : 0.0,
                        GasOilRatio = request.GasOilRatio.HasValue ? (double)request.GasOilRatio.Value : 0.0,
                        OilGravity = request.OilGravity.HasValue ? (double)request.OilGravity.Value : 35.0
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    reservoirProperties = await GetReservoirPropertiesForWellAsync(resolvedWellId);
                    
                    if (reservoirProperties == null || reservoirProperties.ReservoirPressure <= 0)
                    {
                        throw new InvalidOperationException("Reservoir properties not found or invalid. Provide ReservoirPressure and ProductivityIndex in request or ensure PPDM data is available.");
                    }
                }

                // Step 2: Build wellbore properties from request or PPDM data
                WellboreProperties wellboreProperties;
                if (request.TubingDiameter.HasValue && request.WellheadPressure.HasValue)
                {
                    // Use values from request
                    wellboreProperties = new WellboreProperties
                    {
                        TubingDiameter = (double)request.TubingDiameter.Value,
                        TubingLength = request.WellDepth.HasValue ? (double)request.WellDepth.Value : 8000.0,
                        WellheadPressure = (double)request.WellheadPressure.Value,
                        WaterCut = request.WaterCut.HasValue ? (double)request.WaterCut.Value / 100.0 : 0.0,
                        GasOilRatio = request.GasOilRatio.HasValue ? (double)request.GasOilRatio.Value : 0.0,
                        OilGravity = request.OilGravity.HasValue ? (double)request.OilGravity.Value : 35.0,
                        GasSpecificGravity = request.GasGravity.HasValue ? (double)request.GasGravity.Value : 0.65,
                        WellheadTemperature = request.Temperature.HasValue ? (double)request.Temperature.Value : 100.0,
                        BottomholeTemperature = request.Temperature.HasValue ? (double)request.Temperature.Value + 100.0 : 200.0
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    wellboreProperties = await GetWellborePropertiesForWellAsync(resolvedWellId);
                    
                    if (wellboreProperties == null || wellboreProperties.TubingDiameter <= 0)
                    {
                        throw new InvalidOperationException("Wellbore properties not found or invalid. Provide TubingDiameter and WellheadPressure in request or ensure PPDM data is available.");
                    }
                }

                // Step 3: Determine IPR method
                string iprMethod = request.IPRModel ?? "VOGEL";
                int numberOfPoints = request.NumberOfPoints ?? 50;
                double maxFlowRate = request.FlowRateRangeMax.HasValue 
                    ? (double)request.FlowRateRangeMax.Value 
                    : 5000.0; // Default 5000 BPD

                // Step 4: Generate IPR curve
                List<IPRPoint> iprCurve;
                try
                {
                    switch (iprMethod.ToUpperInvariant())
                    {
                        case "VOGEL":
                            iprCurve = IPRCalculator.GenerateVogelIPR(reservoirProperties, maxFlowRate, numberOfPoints);
                            break;
                        case "FETKOVICH":
                            // Fetkovich requires test points - use simplified approach
                            var testPoints = new List<(double flowRate, double pressure)>
                            {
                                (0, reservoirProperties.ReservoirPressure),
                                (maxFlowRate * 0.5, reservoirProperties.ReservoirPressure * 0.7),
                                (maxFlowRate, reservoirProperties.ReservoirPressure * 0.3)
                            };
                            iprCurve = IPRCalculator.GenerateFetkovichIPR(reservoirProperties, testPoints, maxFlowRate, numberOfPoints);
                            break;
                        default:
                            iprCurve = IPRCalculator.GenerateVogelIPR(reservoirProperties, maxFlowRate, numberOfPoints);
                            break;
                    }
                }
                catch (Exception iprEx)
                {
                    _logger?.LogError(iprEx, "Error generating IPR curve");
                    throw new InvalidOperationException($"IPR curve generation failed: {iprEx.Message}", iprEx);
                }

                // Step 5: Generate VLP curve
                double[] flowRates = iprCurve.Select(p => p.FlowRate).ToArray();
                List<VLPPoint> vlpCurve;
                try
                {
                    vlpCurve = VLPCalculator.GenerateVLP(wellboreProperties, flowRates);
                }
                catch (Exception vlpEx)
                {
                    _logger?.LogError(vlpEx, "Error generating VLP curve");
                    throw new InvalidOperationException($"VLP curve generation failed: {vlpEx.Message}", vlpEx);
                }

                // Step 6: Find operating point
                OperatingPoint operatingPoint;
                try
                {
                    operatingPoint = Beep.OilandGas.NodalAnalysis.Calculations.NodalAnalyzer.FindOperatingPoint(iprCurve, vlpCurve);
                }
                catch (Exception opEx)
                {
                    _logger?.LogError(opEx, "Error finding operating point");
                    throw new InvalidOperationException($"Operating point calculation failed: {opEx.Message}", opEx);
                }

                // Step 7: Map results to NodalAnalysisResult DTO
                var result = MapNodalAnalysisResultToDTO(
                    resolvedWellId, request, iprCurve, vlpCurve, operatingPoint, reservoirProperties, wellboreProperties);

                // Step 8: Store result in database
                var repository = await GetNodalResultRepositoryAsync();

                await repository.InsertAsync(result, request.UserId ?? "system");

                _logger?.LogInformation("Nodal Analysis calculation completed: {CalculationId}, Operating Flow Rate: {FlowRate} BPD, Operating Pressure: {Pressure} psi",
                    result.CalculationId, result.OperatingFlowRate, result.OperatingPressure);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Nodal Analysis");

                // Return error result
                var errorResult = new NodalAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = resolvedWellId ?? string.Empty,
                    WellboreId = resolvedWellId ?? string.Empty,
                    FieldId = request.FieldId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    IPRCurve = new List<NodalCurvePoint>(),
                    VLPCurve = new List<NodalCurvePoint>(),
                    Recommendations = new List<string>(),
                    AdditionalResults = new NodalAnalysisAdditionalResults
                    {
                        WellUwi = request.WellUWI
                    }
                };

                // Try to store error result
                try
                {
                    var repository = await GetNodalResultRepositoryAsync();

                    await repository.InsertAsync(errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Nodal Analysis error result");
                }

                throw;
            }
        }

        #region Economic Analysis Helper Methods

        /// <summary>
        /// Builds cash flows from production forecast in request
        /// </summary>
        private CashFlow[] BuildCashFlowsFromProductionForecast(EconomicAnalysisRequest request)
        {
            if (request.ProductionForecast == null || request.ProductionForecast.Count == 0)
            {
                return Array.Empty<CashFlow>();
            }

            var cashFlows = new List<CashFlow>();
            var startDate = request.AnalysisStartDate ?? request.ProductionForecast.First().Date;
            var oilPrice = request.OilPrice ?? 50.0m; // Default $50/bbl
            var gasPrice = request.GasPrice ?? 3.0m; // Default $3/Mscf
            var operatingCostPerUnit = request.OperatingCostPerUnit ?? 10.0m; // Default $10/bbl equivalent
            var royaltyRate = request.RoyaltyRate ?? 0.125m; // Default 12.5%
            var taxRate = request.TaxRate ?? 0.35m; // Default 35%
            var workingInterest = request.WorkingInterest ?? 1.0m; // Default 100%

            // Add initial investment (period 0)
            if (request.CapitalInvestment.HasValue && request.CapitalInvestment.Value != 0)
            {
                cashFlows.Add(new CashFlow
                {
                    Period = 0,
                    Amount = -(double)request.CapitalInvestment.Value,
                    Description = "Initial Capital Investment"
                });
            }

            // Build cash flows from production forecast
            int period = 1;
            foreach (var point in request.ProductionForecast.OrderBy(p => p.Date))
            {
                // Calculate revenue
                decimal revenue = 0;
                if (point.OilVolume.HasValue)
                    revenue += point.OilVolume.Value * oilPrice;
                if (point.GasVolume.HasValue)
                    revenue += point.GasVolume.Value * gasPrice / 1000.0m; // Convert Mscf to Mscf (already in Mscf)

                // Apply working interest
                revenue *= workingInterest;

                // Calculate costs
                decimal operatingCost = 0;
                if (point.OperatingCost.HasValue)
                {
                    operatingCost = point.OperatingCost.Value;
                }
                else
                {
                    // Estimate from volumes
                    decimal totalVolume = (point.OilVolume ?? 0) + (point.GasVolume ?? 0) / 6.0m; // Convert gas to oil equivalent
                    operatingCost = totalVolume * operatingCostPerUnit;
                }

                // Calculate royalties
                decimal royalties = revenue * royaltyRate;

                // Calculate taxes (on net revenue after royalties and costs)
                decimal netRevenue = revenue - royalties - operatingCost;
                decimal taxes = netRevenue > 0 ? netRevenue * taxRate : 0;

                // Net cash flow
                decimal netCashFlow = revenue - royalties - operatingCost - taxes;

                cashFlows.Add(new CashFlow
                {
                    Period = period++,
                    Amount = (double)netCashFlow,
                    Description = $"Period {period - 1} - {point.Date:yyyy-MM-dd}"
                });
            }

            return cashFlows.ToArray();
        }

        /// <summary>
        /// Builds cash flows from PPDM data (well, pool, or field)
        /// </summary>
        private async Task<CashFlow[]> BuildCashFlowsFromPPDMDataAsync(EconomicAnalysisRequest request)
        {
            try
            {
                var cashFlows = new List<CashFlow>();
                var oilPrice = request.OilPrice ?? 50.0m; // Default $50/bbl
                var gasPrice = request.GasPrice ?? 3.0m; // Default $3/Mscf
                var operatingCostPerUnit = request.OperatingCostPerUnit ?? 10.0m; // Default $10/bbl equivalent
                var royaltyRate = request.RoyaltyRate ?? 0.125m; // Default 12.5%
                var taxRate = request.TaxRate ?? 0.35m; // Default 35%
                var workingInterest = request.WorkingInterest ?? 1.0m; // Default 100%

                // Add initial investment (period 0)
                if (request.CapitalInvestment.HasValue && request.CapitalInvestment.Value != 0)
                {
                    cashFlows.Add(new CashFlow
                    {
                        Period = 0,
                        Amount = -(double)request.CapitalInvestment.Value,
                        Description = "Initial Capital Investment"
                    });
                }

                // Retrieve production data from PPDM
                var productionData = await GetProductionDataForEconomicAnalysisAsync(request);
                
                if (productionData.Count == 0)
                {
                    _logger?.LogWarning("No production data found in PPDM for economic analysis. " +
                        "Consider providing ProductionForecast in request.");
                    return cashFlows.ToArray();
                }

                // Group production data by period (monthly or yearly based on request)
                var startDate = request.AnalysisStartDate ?? productionData.Min(p => p.Date);
                var periodMonths = request.AnalysisPeriodYears.HasValue 
                    ? 12 / request.AnalysisPeriodYears.Value 
                    : 1; // Default monthly

                var groupedData = productionData
                    .GroupBy(p => GetPeriodNumber(p.Date, startDate, periodMonths))
                    .OrderBy(g => g.Key)
                    .ToList();

                int period = 1;
                foreach (var group in groupedData)
                {
                    var periodData = group.ToList();
                    var periodDate = periodData.First().Date;

                    // Aggregate production for the period
                    decimal totalOil = periodData.Sum(p => p.OilVolume ?? 0);
                    decimal totalGas = periodData.Sum(p => p.GasVolume ?? 0);

                    // Calculate revenue
                    decimal revenue = (totalOil * oilPrice) + (totalGas * gasPrice / 1000.0m);
                    revenue *= workingInterest;

                    // Calculate costs
                    decimal operatingCost = periodData.Sum(p => p.OperatingCost ?? 0);
                    if (operatingCost == 0)
                    {
                        decimal totalVolume = totalOil + (totalGas / 6.0m); // Convert gas to oil equivalent
                        operatingCost = totalVolume * operatingCostPerUnit;
                    }

                    // Calculate royalties and taxes
                    decimal royalties = revenue * royaltyRate;
                    decimal netRevenue = revenue - royalties - operatingCost;
                    decimal taxes = netRevenue > 0 ? netRevenue * taxRate : 0;

                    // Net cash flow
                    decimal netCashFlow = revenue - royalties - operatingCost - taxes;

                    cashFlows.Add(new CashFlow
                    {
                        Period = period++,
                        Amount = (double)netCashFlow,
                        Description = $"Period {period - 1} - {periodDate:yyyy-MM}"
                    });
                }

                return cashFlows.ToArray();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error building cash flows from PPDM data");
                // Return empty array on error - caller can handle
                return Array.Empty<CashFlow>();
            }
        }

        /// <summary>
        /// Gets production data for economic analysis from PPDM
        /// </summary>
        private async Task<List<EconomicProductionPoint>> GetProductionDataForEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            var productionPoints = new List<EconomicProductionPoint>();

            try
            {
                // Use similar approach to GetProductionDataForDCAAsync
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>();

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
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "FIELD_ID", 
                        Operator = "=", 
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.FieldId) 
                    });
                }

                // Add date filters if provided
                if (request.AnalysisStartDate.HasValue)
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "PRODUCTION_DATE", 
                        Operator = ">=", 
                        FilterValue = request.AnalysisStartDate.Value.ToString("yyyy-MM-dd") 
                    });
                }

                if (request.AnalysisEndDate.HasValue)
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "PRODUCTION_DATE", 
                        Operator = "<=", 
                        FilterValue = request.AnalysisEndDate.Value.ToString("yyyy-MM-dd") 
                    });
                }

                var entities = await repo.GetAsync(filters);
                
                foreach (var entity in entities.OrderBy(e => GetDateValue(e, "PRODUCTION_DATE")))
                {
                    var date = GetDateValue(entity, "PRODUCTION_DATE") ?? DateTime.UtcNow;
                    var oilVol = GetPropertyValueMultiple(entity, "OIL_VOLUME", "OIL_PROD", "OIL_VOL");
                    var gasVol = GetPropertyValueMultiple(entity, "GAS_VOLUME", "GAS_PROD", "GAS_VOL");
                    var waterVol = GetPropertyValueMultiple(entity, "WATER_VOLUME", "WATER_PROD", "WATER_VOL");

                    productionPoints.Add(new EconomicProductionPoint
                    {
                        Date = date,
                        OilVolume = oilVol,
                        GasVolume = gasVol,
                        WaterVolume = waterVol
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving production data for economic analysis");
            }

            return productionPoints;
        }

        /// <summary>
        /// Calculates period number based on date and start date
        /// </summary>
        private int GetPeriodNumber(DateTime date, DateTime startDate, int periodMonths)
        {
            var monthsDiff = ((date.Year - startDate.Year) * 12) + (date.Month - startDate.Month);
            return monthsDiff / periodMonths;
        }

        /// <summary>
        /// Maps EconomicResult from library to EconomicAnalysisResult DTO
        /// </summary>
        private EconomicAnalysisResult MapEconomicResultToDTO(
            EconomicResult economicResult,
            EconomicAnalysisRequest request,
            CashFlow[] cashFlows,
            List<NPVProfilePoint>? npvProfile)
        {
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
                NetPresentValue = (decimal)economicResult.NPV,
                InternalRateOfReturn = (decimal)(economicResult.IRR * 100.0), // Convert to percentage
                PaybackPeriod = (decimal)economicResult.PaybackPeriod,
                ReturnOnInvestment = (decimal)economicResult.ROI,
                ProfitabilityIndex = (decimal)economicResult.ProfitabilityIndex,
                CashFlowPoints = new List<EconomicCashFlowPoint>(),
                AdditionalResults = new EconomicAnalysisAdditionalResults()
            };

            // Map cash flows to cash flow points
            decimal cumulativeCashFlow = 0;
            decimal cumulativeDiscountedCashFlow = 0;
            double discountRate = request.DiscountRate.HasValue 
                ? (double)request.DiscountRate.Value / 100.0 
                : 0.10;

            foreach (var cf in cashFlows.OrderBy(c => c.Period))
            {
                decimal discountedCF = (decimal)(cf.Amount / Math.Pow(1 + discountRate, cf.Period));
                cumulativeCashFlow += (decimal)cf.Amount;
                cumulativeDiscountedCashFlow += discountedCF;

                result.CashFlowPoints.Add(new EconomicCashFlowPoint
                {
                    Date = request.AnalysisStartDate?.AddMonths(cf.Period * 12) ?? DateTime.UtcNow.AddMonths(cf.Period * 12),
                    NetCashFlow = (decimal)cf.Amount,
                    CumulativeCashFlow = cumulativeCashFlow,
                    DiscountedCashFlow = discountedCF,
                    CumulativeDiscountedCashFlow = cumulativeDiscountedCashFlow
                });
            }

            // Calculate totals
            var totalRevenue = result.CashFlowPoints.Where(cf => cf.NetCashFlow > 0).Sum(cf => cf.NetCashFlow);
            result.TotalRevenue = totalRevenue ?? 0m;
            var totalOperatingCosts = result.CashFlowPoints.Where(cf => cf.NetCashFlow < 0).Sum(cf => cf.NetCashFlow);
            result.TotalOperatingCosts = totalOperatingCosts.HasValue ? Math.Abs(totalOperatingCosts.Value) : null;
            result.NetCashFlow = cumulativeCashFlow;

            // Add NPV profile to additional results if available
            if (npvProfile != null && npvProfile.Count > 0)
            {
                result.AdditionalResults.NpvProfile = npvProfile;
            }

            // Add MIRR and discounted payback period
            result.AdditionalResults.Mirr = economicResult.MIRR * 100.0;
            result.AdditionalResults.DiscountedPaybackPeriod = economicResult.DiscountedPaybackPeriod;
            result.AdditionalResults.TotalCashFlow = economicResult.TotalCashFlow;
            result.AdditionalResults.PresentValue = economicResult.PresentValue;

            return result;
        }

        #endregion

        #region Nodal Analysis Helper Methods

        /// <summary>
        /// Retrieves reservoir properties from PPDM for a well
        /// </summary>
        private async Task<ReservoirProperties?> GetReservoirPropertiesForWellAsync(string wellId)
        {
            try
            {
                if (string.IsNullOrEmpty(wellId))
                    return null;

                // Get productivity index from well test
                var pi = await GetWellTestProductivityIndexAsync(wellId);
                if (!pi.HasValue || pi.Value <= 0)
                {
                    _logger?.LogWarning("Productivity index not found for well {WellId}", wellId);
                    return null;
                }

                // Get static/reservoir pressure from well test
                var reservoirPressure = await GetWellTestStaticPressureAsync(wellId);
                if (!reservoirPressure.HasValue || reservoirPressure.Value <= 0)
                {
                    _logger?.LogWarning("Reservoir pressure not found for well {WellId}", wellId);
                    return null;
                }

                // Get bubble point pressure (try from well test or use default)
                var bubblePointPressure = await GetBubblePointPressureForWellAsync(wellId) 
                    ?? (double)(reservoirPressure.Value * 0.8m); // Default 80% of reservoir pressure

                // Get water cut from latest production data
                var waterCut = await GetWaterCutForWellAsync(wellId) ?? 0.0;

                // Get GOR from latest production data
                var gor = await GetGasOilRatioForWellAsync(wellId) ?? 0.0;

                // Get oil gravity (try from well or use default)
                var oilGravity = await GetOilGravityForWellAsync(wellId) ?? 35.0;

                return new ReservoirProperties
                {
                    ReservoirPressure = (double)reservoirPressure.Value,
                    BubblePointPressure = bubblePointPressure,
                    ProductivityIndex = (double)pi.Value,
                    WaterCut = waterCut,
                    GasOilRatio = gor,
                    OilGravity = oilGravity
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving reservoir properties for well {WellId}", wellId);
                return null;
            }
        }

        /// <summary>
        /// Gets bubble point pressure for a well
        /// </summary>
        private async Task<double?> GetBubblePointPressureForWellAsync(string wellId)
        {
            // Try to get from well test or reservoir data
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Reservoir.BubblePointPressure");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets water cut for a well from production data
        /// </summary>
        private async Task<double?> GetWaterCutForWellAsync(string wellId)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId) }
                };

                var entities = await GetEntitiesAsync("PDEN_VOL_SUMMARY", filters, "PRODUCTION_DATE", DataRetrievalMode.Latest);
                var latest = entities.FirstOrDefault();

                if (latest != null)
                {
                    var oilVol = GetPropertyValueMultiple(latest, "OIL_VOLUME", "OIL_PROD", "OIL_VOL") ?? 0;
                    var waterVol = GetPropertyValueMultiple(latest, "WATER_VOLUME", "WATER_PROD", "WATER_VOL") ?? 0;
                    var totalVol = oilVol + waterVol;

                    if (totalVol > 0)
                    {
                        return (double)(waterVol / totalVol);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting water cut for well {WellId}", wellId);
            }

            return null;
        }

        /// <summary>
        /// Gets gas-oil ratio for a well from production data
        /// </summary>
        private async Task<double?> GetGasOilRatioForWellAsync(string wellId)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId) }
                };

                var entities = await GetEntitiesAsync("PDEN_VOL_SUMMARY", filters, "PRODUCTION_DATE", DataRetrievalMode.Latest);
                var latest = entities.FirstOrDefault();

                if (latest != null)
                {
                    var oilVol = GetPropertyValueMultiple(latest, "OIL_VOLUME", "OIL_PROD", "OIL_VOL") ?? 0;
                    var gasVol = GetPropertyValueMultiple(latest, "GAS_VOLUME", "GAS_PROD", "GAS_VOL") ?? 0;

                    if (oilVol > 0)
                    {
                        return (double)(gasVol / oilVol * 1000m); // Convert to SCF/STB
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting GOR for well {WellId}", wellId);
            }

            return null;
        }

        /// <summary>
        /// Gets oil gravity (API) for a well
        /// </summary>
        private async Task<double?> GetOilGravityForWellAsync(string wellId)
        {
            // Try to get from well test or fluid properties
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Fluid.OilGravity");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieves wellbore properties from PPDM for a well
        /// </summary>
        private async Task<WellboreProperties?> GetWellborePropertiesForWellAsync(string wellId)
        {
            try
            {
                if (string.IsNullOrEmpty(wellId))
                    return null;

                // Get tubing diameter from WELL_TUBULAR
                var tubingDiameter = await GetTubularOuterDiameterAsync(wellId, "TUBING");
                if (!tubingDiameter.HasValue || tubingDiameter.Value <= 0)
                {
                    _logger?.LogWarning("Tubing diameter not found for well {WellId}", wellId);
                    return null;
                }

                // Get tubing length/depth
                var tubingDepth = await GetTubularDepthAsync(wellId, "TUBING");
                var wellDepth = await GetWellTotalDepthAsync(wellId);
                var tubingLength = tubingDepth ?? wellDepth ?? 8000m; // Default 8000 ft

                // Get wellhead pressure (try from well test or use default)
                var wellheadPressure = await GetWellheadPressureForWellAsync(wellId) ?? 500m; // Default 500 psi

                // Get water cut and GOR (same as reservoir properties)
                var waterCut = await GetWaterCutForWellAsync(wellId) ?? 0.0;
                var gor = await GetGasOilRatioForWellAsync(wellId) ?? 0.0;

                // Get oil gravity
                var oilGravity = await GetOilGravityForWellAsync(wellId) ?? 35.0;

                // Get gas specific gravity (try from well test or use default)
                var gasGravity = await GetGasSpecificGravityForWellAsync(wellId) ?? 0.65;

                // Get temperatures (try from well test or use defaults)
                var wellheadTemp = await GetWellheadTemperatureForWellAsync(wellId) ?? 100.0;
                var bottomholeTemp = await GetBottomholeTemperatureForWellAsync(wellId) ?? (wellheadTemp + 100.0);

                return new WellboreProperties
                {
                    TubingDiameter = (double)tubingDiameter.Value,
                    TubingLength = (double)tubingLength,
                    WellheadPressure = (double)wellheadPressure,
                    WaterCut = waterCut,
                    GasOilRatio = gor,
                    OilGravity = oilGravity,
                    GasSpecificGravity = gasGravity,
                    WellheadTemperature = wellheadTemp,
                    BottomholeTemperature = bottomholeTemp
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving wellbore properties for well {WellId}", wellId);
                return null;
            }
        }

        /// <summary>
        /// Gets wellhead pressure for a well
        /// </summary>
        private async Task<decimal?> GetWellheadPressureForWellAsync(string wellId)
        {
            // Try to get from well test
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.WellheadPressure");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return value.Value * (mapping.ConversionFactor ?? 1m);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets gas specific gravity for a well
        /// </summary>
        private async Task<double?> GetGasSpecificGravityForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Fluid.GasSpecificGravity");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetEntityAsync(mapping.TableName, wellId, "WELL_ID");
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets wellhead temperature for a well
        /// </summary>
        private async Task<double?> GetWellheadTemperatureForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.WellheadTemperature");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Gets bottomhole temperature for a well
        /// </summary>
        private async Task<double?> GetBottomholeTemperatureForWellAsync(string wellId)
        {
            var mapping = await _fieldMappingService.GetFieldMappingAsync("WellTest.BottomholeTemperature");
            if (mapping != null && mapping.IsActive && !string.IsNullOrEmpty(mapping.TableName))
            {
                var entity = await GetLatestEntityForWellAsync(mapping.TableName, wellId, "EFFECTIVE_DATE", null);
                var value = GetPropertyValue(entity, mapping.FieldName);
                if (value.HasValue)
                {
                    return (double)(value.Value * (mapping.ConversionFactor ?? 1m));
                }
            }
            return null;
        }

        /// <summary>
        /// Maps NodalAnalysis results to NodalAnalysisResult DTO
        /// </summary>
        private NodalAnalysisResult MapNodalAnalysisResultToDTO(
            string wellId,
            NodalAnalysisRequest request,
            List<IPRPoint> iprCurve,
            List<VLPPoint> vlpCurve,
            OperatingPoint operatingPoint,
            ReservoirProperties reservoirProperties,
            WellboreProperties wellboreProperties)
        {
            var result = new NodalAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = wellId,
                WellboreId = wellId,
                FieldId = request.FieldId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId,
                OperatingFlowRate = (decimal)operatingPoint.FlowRate,
                OperatingPressure = (decimal)operatingPoint.BottomholePressure,
                OperatingTemperature = request.Temperature.HasValue ? request.Temperature : null,
                IPRCurve = new List<NodalCurvePoint>(),
                VLPCurve = new List<NodalCurvePoint>(),
                Recommendations = new List<string>(),
                AdditionalResults = new NodalAnalysisAdditionalResults
                {
                    WellUwi = request.WellUWI
                }
            };

            // Map IPR curve points
            foreach (var point in iprCurve)
            {
                result.IPRCurve.Add(new NodalCurvePoint
                {
                    FlowRate = (decimal)point.FlowRate,
                    Pressure = (decimal)point.FlowingBottomholePressure
                });
            }

            // Map VLP curve points
            foreach (var point in vlpCurve)
            {
                result.VLPCurve.Add(new NodalCurvePoint
                {
                    FlowRate = (decimal)point.FlowRate,
                    Pressure = (decimal)point.RequiredBottomholePressure
                });
            }

            // Calculate performance metrics
            if (iprCurve.Count > 0)
            {
                result.MaximumFlowRate = (decimal)iprCurve.Max(p => p.FlowRate);
                result.MinimumFlowRate = (decimal)iprCurve.Min(p => p.FlowRate);
                result.OptimalFlowRate = (decimal)operatingPoint.FlowRate;
            }

            if (vlpCurve.Count > 0 && iprCurve.Count > 0)
            {
                var maxVLP = vlpCurve.Max(p => p.RequiredBottomholePressure);
                var minIPR = iprCurve.Min(p => p.FlowingBottomholePressure);
                result.PressureDrop = (decimal)(maxVLP - minIPR);
            }

            // Calculate system efficiency (simplified)
            if (reservoirProperties.ReservoirPressure > 0)
            {
                double theoreticalMaxFlow = reservoirProperties.ProductivityIndex * reservoirProperties.ReservoirPressure;
                if (theoreticalMaxFlow > 0)
                {
                    result.SystemEfficiency = (decimal)((operatingPoint.FlowRate / theoreticalMaxFlow) * 100.0);
                }
            }

            // Generate recommendations
            if (result.MaximumFlowRate.HasValue && operatingPoint.FlowRate < (double)(result.MaximumFlowRate.Value * 0.8m))
            {
                result.Recommendations.Add("Consider optimizing well completion to increase flow rate");
            }

            if (result.PressureDrop > 1000)
            {
                result.Recommendations.Add("High pressure drop detected - consider larger tubing diameter");
            }

            if (result.SystemEfficiency < 50)
            {
                result.Recommendations.Add("Low system efficiency - review well configuration");
            }

            // Add additional results
            result.AdditionalResults.ReservoirPressure = (decimal)reservoirProperties.ReservoirPressure;
            result.AdditionalResults.ProductivityIndex = (decimal)reservoirProperties.ProductivityIndex;
            result.AdditionalResults.TubingDiameter = (decimal)wellboreProperties.TubingDiameter;
            result.AdditionalResults.TubingLength = (decimal)wellboreProperties.TubingLength;
            result.AdditionalResults.WellheadPressure = (decimal)wellboreProperties.WellheadPressure;
            result.AdditionalResults.IprMethod = request.IPRModel ?? "VOGEL";
            result.AdditionalResults.VlpModel = request.VLPModel ?? "HAGEDORN_BROWN";

            return result;
        }

        #endregion

        #region Well Test Analysis

        /// <summary>
        /// Performs well test analysis (pressure transient analysis) for a well.
        /// Supports build-up and drawdown analysis using Horner or MDH methods.
        /// </summary>
        /// <param name="request">Well test analysis request containing well ID, test ID, pressure-time data, and analysis parameters</param>
        /// <returns>Well test analysis result with permeability, skin factor, reservoir pressure, productivity index, and diagnostic data</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well test data is unavailable or calculation fails</exception>
        public async Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.TestId))
                {
                    throw new ArgumentException("At least one of WellId or TestId must be provided");
                }

                _logger?.LogInformation("Starting Well Test Analysis for WellId: {WellId}, TestId: {TestId}",
                    request.WellId, request.TestId);

                // Step 1: Build well test data from request or PPDM data
                WellTestData wellTestData;
                if (request.PressureTimeData != null && request.PressureTimeData.Count > 0)
                {
                    // Use data from request
                    wellTestData = new WellTestData
                    {
                        Time = request.PressureTimeData.Select(p => p.Time).ToList(),
                        Pressure = request.PressureTimeData.Select(p => p.Pressure).ToList(),
                        FlowRate = request.FlowRate.HasValue ? (double)request.FlowRate.Value : 0.0,
                        WellboreRadius = request.WellboreRadius.HasValue ? (double)request.WellboreRadius.Value : 0.25,
                        FormationThickness = request.FormationThickness.HasValue ? (double)request.FormationThickness.Value : 50.0,
                        Porosity = request.Porosity.HasValue ? (double)request.Porosity.Value : 0.2,
                        TotalCompressibility = request.TotalCompressibility.HasValue ? (double)request.TotalCompressibility.Value : 1e-5,
                        OilViscosity = request.OilViscosity.HasValue ? (double)request.OilViscosity.Value : 1.5,
                        OilFormationVolumeFactor = request.OilFormationVolumeFactor.HasValue ? (double)request.OilFormationVolumeFactor.Value : 1.2,
                        ProductionTime = request.ProductionTime.HasValue ? (double)request.ProductionTime.Value : 0.0,
                        IsGasWell = request.IsGasWell ?? false,
                        GasSpecificGravity = request.GasSpecificGravity.HasValue ? (double)request.GasSpecificGravity.Value : 0.65,
                        ReservoirTemperature = request.ReservoirTemperature.HasValue ? (double)request.ReservoirTemperature.Value : 150.0,
                        InitialReservoirPressure = request.InitialReservoirPressure.HasValue ? (double)request.InitialReservoirPressure.Value : 0.0,
                        TestType = request.AnalysisType.ToUpper() == "DRAWDOWN" ? WellTestType.Drawdown : WellTestType.BuildUp
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    wellTestData = await GetWellTestDataFromPPDMAsync(request.WellId ?? string.Empty, request.TestId ?? string.Empty);
                }

                // Step 2: Perform well test analysis
                WellTestAnalysisResult analysisResult;
                string analysisMethod = request.AnalysisMethod?.ToUpper() ?? "HORNER";

                if (request.AnalysisType.ToUpper() == "BUILDUP")
                {
                    if (analysisMethod == "MDH")
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUpMDH(wellTestData);
                    }
                    else
                    {
                        analysisResult = WellTestAnalyzer.AnalyzeBuildUp(wellTestData);
                    }
                }
                else
                {
                    // Drawdown analysis - would need DrawdownAnalysis class if available
                    // For now, use build-up method
                    analysisResult = WellTestAnalyzer.AnalyzeBuildUp(wellTestData);
                }

                // Step 3: Calculate derivative for diagnostic plots
                var pressureTimePoints = wellTestData.Time.Zip(wellTestData.Pressure, (t, p) => new PressureTimePoint(t, p)).ToList();
                var derivativePoints = WellTestAnalyzer.CalculateDerivative(pressureTimePoints);

                // Step 4: Identify reservoir model
                var identifiedModel = WellTestAnalyzer.IdentifyReservoirModel(derivativePoints);

                // Step 5: Map to DTO
                var result = MapWellTestResultToDTO(analysisResult, request, identifiedModel, derivativePoints);

                // Step 6: Store result in PPDM database
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    result.DiagnosticDataJson = JsonSerializer.Serialize(result.DiagnosticPoints ?? new List<WellTestDataPoint>());
                    result.DerivativeDataJson = JsonSerializer.Serialize(result.DerivativePoints ?? new List<WellTestDataPoint>());

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Well Test Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Well Test Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Well Test Analysis for WellId: {WellId}, TestId: {TestId}",
                    request.WellId, request.TestId);

                // Try to store error result
                try
                {
                    var repository = await GetWellTestResultRepositoryAsync();
                    var errorResult = new WellTestAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        TestId = request.TestId,
                        AnalysisType = request.AnalysisType,
                        AnalysisMethod = request.AnalysisMethod ?? "HORNER",
                        CalculationDate = DateTime.UtcNow,
                        Status = "FAILED",
                        ErrorMessage = ex.Message
                    };
                    await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Well Test Analysis error result");
                }

                throw;
            }
        }

        #endregion

        #region Flash Calculation

        /// <summary>
        /// Performs flash calculation (phase equilibrium) for a well or facility.
        /// Supports isothermal flash calculations with vapor-liquid equilibrium.
        /// </summary>
        /// <param name="request">Flash calculation request containing well/facility ID, pressure, temperature, and feed composition</param>
        /// <returns>Flash calculation result with vapor/liquid fractions, phase compositions, K-values, and phase properties</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when feed composition is unavailable or calculation fails</exception>
        public async Task<FlashCalculationResult> PerformFlashCalculationAsync(FlashCalculationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.FacilityId))
                {
                    throw new ArgumentException("At least one of WellId or FacilityId must be provided");
                }

                if (request.FeedComposition == null || request.FeedComposition.Count == 0)
                {
                    throw new ArgumentException("Feed composition must be provided");
                }

                _logger?.LogInformation("Starting Flash Calculation for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);

                // Step 1: Build flash conditions from request or PPDM data
                FlashConditions flashConditions;
                if (request.Pressure.HasValue && request.Temperature.HasValue && request.FeedComposition != null)
                {
                    // Use values from request
                    flashConditions = new FlashConditions
                    {
                        Pressure = request.Pressure.Value,
                        Temperature = request.Temperature.Value,
                        FeedComposition = request.FeedComposition.Select(c => new Component
                        {
                            Name = c.Name,
                            MoleFraction = c.MoleFraction,
                            CriticalTemperature = c.CriticalTemperature ?? GetDefaultCriticalTemperature(c.Name),
                            CriticalPressure = c.CriticalPressure ?? GetDefaultCriticalPressure(c.Name),
                            AcentricFactor = c.AcentricFactor ?? GetDefaultAcentricFactor(c.Name),
                            MolecularWeight = c.MolecularWeight ?? GetDefaultMolecularWeight(c.Name)
                        }).ToList()
                    };
                }
                else
                {
                    // Retrieve from PPDM data
                    flashConditions = await GetFlashConditionsFromPPDMAsync(request.WellId ?? string.Empty, request.FacilityId ?? string.Empty);
                }

                // Step 2: Perform flash calculation
                var flashResult = FlashCalculator.PerformIsothermalFlash(flashConditions);

                // Step 3: Calculate phase properties
                var vaporProperties = FlashCalculator.CalculateVaporProperties(flashResult, flashConditions);
                var liquidProperties = FlashCalculator.CalculateLiquidProperties(flashResult, flashConditions);

                // Step 4: Map to DTO
                var result = MapFlashResultToDTO(flashResult, request, vaporProperties, liquidProperties);

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    result.Pressure = flashConditions.Pressure;
                    result.Temperature = flashConditions.Temperature;
                    result.FeedCompositionJson = JsonSerializer.Serialize(flashConditions.FeedComposition);
                    result.VaporCompositionJson = JsonSerializer.Serialize(result.VaporComposition);
                    result.LiquidCompositionJson = JsonSerializer.Serialize(result.LiquidComposition);
                    result.KValuesJson = JsonSerializer.Serialize(result.KValues);

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Flash Calculation result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Flash Calculation result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Flash Calculation for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);

                // Try to store error result
                try
                {
                    var repository = await GetFlashResultRepositoryAsync();
                    var errorResult = new FlashCalculationResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        FacilityId = request.FacilityId,
                        CalculationDate = DateTime.UtcNow,
                        Status = "FAILED",
                        ErrorMessage = ex.Message
                    };
                    await InsertAnalysisResultAsync(repository, errorResult, request.UserId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Flash Calculation error result");
                }

                throw;
            }
        }

        #endregion

        #region Choke Analysis

        /// <summary>
        /// Performs choke analysis for a well.
        /// Calculates gas flow rate through chokes, choke sizing, and pressure calculations.
        /// </summary>
        /// <param name="request">Choke analysis request containing well ID, choke properties, and flow conditions</param>
        /// <returns>Choke analysis result with flow rate, pressure drop, and flow regime</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well or equipment data is unavailable</exception>
        public async Task<ChokeAnalysisResult> PerformChokeAnalysisAsync(ChokeAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId))
                {
                    throw new ArgumentException("WellId must be provided");
                }

                _logger?.LogInformation("Starting Choke Analysis for WellId: {WellId}, AnalysisType: {AnalysisType}",
                    request.WellId, request.AnalysisType);

                // Step 1: Retrieve well and equipment data from PPDM
                var well = await GetWellFromPPDMAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well with ID {request.WellId} not found");
                }

                WELL_EQUIPMENT? equipment = null;
                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    equipment = await GetWellEquipmentFromPPDMAsync(request.EquipmentId);
                }

                WELL_PRESSURE? wellPressure = await GetWellPressureFromPPDMAsync(request.WellId);

                // Step 2: Map to domain models using ChokeAnalysisMapper
                var mapper = new ChokeAnalysisMapper();
                var chokeProperties = mapper.MapToChokeProperties(well, wellPressure);
                var gasChokeProperties = mapper.MapToGasChokeProperties(well, wellPressure);

                // Override with request values if provided
                if (request.ChokeDiameter.HasValue)
                    chokeProperties.ChokeDiameter = request.ChokeDiameter.Value;
                if (!string.IsNullOrEmpty(request.ChokeType))
                    chokeProperties.ChokeType = Enum.Parse<ChokeType>(request.ChokeType, ignoreCase: true);
                if (request.DischargeCoefficient.HasValue)
                    chokeProperties.DischargeCoefficient = request.DischargeCoefficient.Value;
                if (request.UpstreamPressure.HasValue)
                    gasChokeProperties.UpstreamPressure = request.UpstreamPressure.Value;
                if (request.DownstreamPressure.HasValue)
                    gasChokeProperties.DownstreamPressure = request.DownstreamPressure.Value;
                if (request.Temperature.HasValue)
                    gasChokeProperties.Temperature = request.Temperature.Value;
                if (request.GasSpecificGravity.HasValue)
                    gasChokeProperties.GasSpecificGravity = request.GasSpecificGravity.Value;
                if (request.ZFactor.HasValue)
                    gasChokeProperties.ZFactor = request.ZFactor.Value;
                if (request.FlowRate.HasValue)
                    gasChokeProperties.FlowRate = request.FlowRate.Value;

                // Step 3: Perform calculation using ChokeAnalysis library
                ChokeFlowResult calculationResult;
                if (request.AnalysisType.ToUpper() == "DOWNSTREAM_PRESSURE" && request.FlowRate.HasValue)
                {
                    var downstreamPressure = GasChokeCalculator.CalculateDownstreamPressure(
                        chokeProperties, gasChokeProperties, request.FlowRate.Value);
                    calculationResult = new CHOKE_FLOW_RESULT
                    {
                        FLOW_RATE = request.FlowRate.Value,
                        DOWNSTREAM_PRESSURE = downstreamPressure,
                        UPSTREAM_PRESSURE = gasChokeProperties.UpstreamPressure,
                        PRESSURE_RATIO = downstreamPressure / gasChokeProperties.UpstreamPressure,
                        FLOW_REGIME = FlowRegime.Subsonic, // Will be determined by pressure ratio
                        CRITICAL_PRESSURE_RATIO = 0.546m // Approximate for natural gas
                    };
                }
                else if (request.AnalysisType.ToUpper() == "SIZING" && request.FlowRate.HasValue)
                {
                    var requiredSize = GasChokeCalculator.CalculateRequiredChokeSize(gasChokeProperties, request.FlowRate.Value);
                    calculationResult = new ChokeFlowResult
                    {
                        FlowRate = request.FlowRate.Value,
                        DownstreamPressure = gasChokeProperties.DownstreamPressure,
                        UpstreamPressure = gasChokeProperties.UpstreamPressure,
                        PressureRatio = gasChokeProperties.DownstreamPressure / gasChokeProperties.UpstreamPressure,
                        FlowRegime = FlowRegime.Subsonic,
                        CriticalPressureRatio = 0.546m
                    };
                    // Store required size in additional results
                }
                else if (request.AnalysisType.ToUpper() == "UPHOLE")
                {
                    calculationResult = GasChokeCalculator.CalculateUpholeChokeFlow(chokeProperties, gasChokeProperties);
                }
                else
                {
                    // Default to downhole
                    calculationResult = GasChokeCalculator.CalculateDownholeChokeFlow(chokeProperties, gasChokeProperties);
                }

                // Step 4: Map to DTO
                var result = new ChokeAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    EquipmentId = request.EquipmentId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    ChokeDiameter = chokeProperties.ChokeDiameter,
                    ChokeType = chokeProperties.ChokeType.ToString(),
                    DischargeCoefficient = chokeProperties.DischargeCoefficient,
                    FlowRate = calculationResult.FlowRate,
                    UpstreamPressure = calculationResult.UpstreamPressure,
                    DownstreamPressure = calculationResult.DownstreamPressure,
                    PressureRatio = calculationResult.PressureRatio,
                    FlowRegime = calculationResult.FlowRegime.ToString(),
                    CriticalPressureRatio = calculationResult.CriticalPressureRatio,
                    Status = "SUCCESS",
                    UserId = request.UserId
                };

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetChokeAnalysisResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Choke Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Choke Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Choke Analysis for WellId: {WellId}", request.WellId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Choke Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetChokeAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("CHOKE_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Choke Analysis result repository");
                throw;
            }
        }

        /// <summary>
        /// Gets WELL entity from PPDM database
        /// </summary>
        private async Task<WELL?> GetWellFromPPDMAsync(string wellId)
        {
            var entity = await GetEntityAsync("WELL", wellId, "WELL_ID");
            return entity as WELL;
        }

        private async Task<string?> GetWellIdByUwiAsync(string wellUwi)
        {
            if (string.IsNullOrWhiteSpace(wellUwi))
            {
                return null;
            }

            var filters = new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName = "WELL_UWI",
                    Operator = "=",
                    FilterValue = wellUwi
                }
            };

            var entities = await GetEntitiesAsync("WELL", filters, "ROW_CHANGED_DATE", DataRetrievalMode.Latest);
            var well = entities.FirstOrDefault() as WELL;

            if (well != null)
            {
                return well.WELL_ID;
            }

            var entity = entities.FirstOrDefault();
            var wellIdValue = GetPropertyValue(entity, "WELL_ID");
            return wellIdValue?.ToString();
        }

        /// <summary>
        /// Gets WELL_EQUIPMENT entity from PPDM database
        /// </summary>
        private async Task<WELL_EQUIPMENT?> GetWellEquipmentFromPPDMAsync(string equipmentId)
        {
            var entity = await GetEntityAsync("WELL_EQUIPMENT", equipmentId, "ROW_ID");
            return entity as WELL_EQUIPMENT;
        }

        /// <summary>
        /// Gets WELL_PRESSURE entity from PPDM database (latest)
        /// </summary>
        private async Task<WELL_PRESSURE?> GetWellPressureFromPPDMAsync(string wellId)
        {
            var entity = await GetLatestEntityForWellAsync("WELL_PRESSURE", wellId, "EFFECTIVE_DATE");
            return entity as WELL_PRESSURE;
        }

        /// <summary>
        /// Gets WELL_TUBULAR entity from PPDM database (latest)
        /// </summary>
        private async Task<WELL_TUBULAR?> GetWellTubularFromPPDMAsync(string wellId, string? tubularType = null)
        {
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
            };

            if (!string.IsNullOrEmpty(tubularType))
            {
                filters.Add(new AppFilter { FieldName = "TUBULAR_TYPE", Operator = "=", FilterValue = tubularType });
            }

            var entities = await GetEntitiesAsync("WELL_TUBULAR", filters, "EFFECTIVE_DATE", DataRetrievalMode.Latest);
            return entities.FirstOrDefault() as WELL_TUBULAR;
        }

        #endregion

        #region Gas Lift Analysis

        /// <summary>
        /// Performs gas lift analysis for a well.
        /// Calculates gas lift potential, valve design, and valve spacing.
        /// </summary>
        /// <param name="request">Gas lift analysis request containing well ID and analysis parameters</param>
        /// <returns>Gas lift analysis result with optimal injection rate, performance curve, and valve design</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well data is unavailable</exception>
        public async Task<GasLiftAnalysisResult> PerformGasLiftAnalysisAsync(GasLiftAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId))
                {
                    throw new ArgumentException("WellId must be provided");
                }

                _logger?.LogInformation("Starting Gas Lift Analysis for WellId: {WellId}, AnalysisType: {AnalysisType}",
                    request.WellId, request.AnalysisType);

                // Step 1: Retrieve well data from PPDM
                var well = await GetWellFromPPDMAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well with ID {request.WellId} not found");
                }

                var tubular = await GetWellTubularFromPPDMAsync(request.WellId, "TUBING");
                var wellPressure = await GetWellPressureFromPPDMAsync(request.WellId);

                // Step 2: Map to domain models using GasLiftMapper
                var mapper = new GasLiftMapper();
                var wellProperties = mapper.MapToGasLiftWellProperties(well, tubular, wellPressure);

                // Override with request values if provided
                if (request.WellDepth.HasValue) wellProperties.WellDepth = request.WellDepth.Value;
                if (request.TubingDiameter.HasValue) wellProperties.TubingDiameter = request.TubingDiameter.Value;
                if (request.CasingDiameter.HasValue) wellProperties.CasingDiameter = request.CasingDiameter.Value;
                if (request.WellheadPressure.HasValue) wellProperties.WellheadPressure = request.WellheadPressure.Value;
                if (request.BottomHolePressure.HasValue) wellProperties.BottomHolePressure = request.BottomHolePressure.Value;
                if (request.WellheadTemperature.HasValue) wellProperties.WellheadTemperature = request.WellheadTemperature.Value;
                if (request.BottomHoleTemperature.HasValue) wellProperties.BottomHoleTemperature = request.BottomHoleTemperature.Value;
                if (request.OilGravity.HasValue) wellProperties.OilGravity = request.OilGravity.Value;
                if (request.WaterCut.HasValue) wellProperties.WaterCut = request.WaterCut.Value;
                if (request.GasOilRatio.HasValue) wellProperties.GasOilRatio = request.GasOilRatio.Value;
                if (request.GasSpecificGravity.HasValue) wellProperties.GasSpecificGravity = request.GasSpecificGravity.Value;
                if (request.DesiredProductionRate.HasValue) wellProperties.DesiredProductionRate = request.DesiredProductionRate.Value;

                // Step 3: Perform calculation using GasLift library
                GasLiftAnalysisResult result;
                if (request.AnalysisType.ToUpper() == "VALVE_DESIGN")
                {
                    // Valve design analysis
                    var gasInjectionPressure = request.AdditionalParameters?.GasInjectionPressure
                        ?? wellProperties.WellheadPressure * 1.5m;
                    var numberOfValves = request.AdditionalParameters?.NumberOfValves ?? 5;

                    var designResult = GasLiftValveDesignCalculator.DesignValvesUS(wellProperties, gasInjectionPressure, numberOfValves);
                    
                    result = new GasLiftAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        Valves = designResult.Valves.Select(v => new GasLiftValveData
                        {
                            Depth = v.Depth,
                            PortSize = v.PortSize,
                            OpeningPressure = v.OpeningPressure,
                            ClosingPressure = v.ClosingPressure,
                            ValveType = v.ValveType.ToString(),
                            Temperature = v.Temperature,
                            GasInjectionRate = v.GasInjectionRate
                        }).ToList(),
                        TotalGasInjectionRate = designResult.TotalGasInjectionRate,
                        ExpectedProductionRate = designResult.ExpectedProductionRate,
                        SystemEfficiency = designResult.SystemEfficiency,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }
                else if (request.AnalysisType.ToUpper() == "VALVE_SPACING")
                {
                    // Valve spacing analysis
                    var spacingResult = GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(
                        wellProperties,
                        request.AdditionalParameters?.GasInjectionPressure
                            ?? wellProperties.WellheadPressure * 1.5m,
                        request.AdditionalParameters?.NumberOfValves ?? 5);

                    result = new GasLiftAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        ValveDepths = spacingResult.ValveDepths,
                        OpeningPressures = spacingResult.OpeningPressures,
                        NumberOfValves = spacingResult.NumberOfValves,
                        TotalDepthCoverage = spacingResult.TotalDepthCoverage,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }
                else
                {
                    // Default to potential analysis
                    var minRate = request.MinGasInjectionRate ?? 100m;
                    var maxRate = request.MaxGasInjectionRate ?? 5000m;
                    var numPoints = request.NumberOfPoints ?? 50;

                    var potentialResult = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
                        wellProperties, minRate, maxRate, numPoints);

                    result = new GasLiftAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        WellId = request.WellId,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        OptimalGasInjectionRate = potentialResult.OptimalGasInjectionRate,
                        MaximumProductionRate = potentialResult.MaximumProductionRate,
                        OptimalGasLiquidRatio = potentialResult.OptimalGasLiquidRatio,
                        PerformancePoints = potentialResult.PerformancePoints.Select(p => new GasLiftPerformancePoint
                        {
                            GasInjectionRate = p.GasInjectionRate,
                            ProductionRate = p.ProductionRate,
                            GasLiquidRatio = p.GasLiquidRatio,
                            BottomHolePressure = p.BottomHolePressure
                        }).ToList(),
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }

                // Step 4: Store result in PPDM database
                try
                {
                    var repository = await GetGasLiftAnalysisResultRepositoryAsync();
                    result.PerformancePointsJson = JsonSerializer.Serialize(result.PerformancePoints);
                    result.ValvesJson = result.Valves == null ? null : JsonSerializer.Serialize(result.Valves);
                    result.ValveDepthsJson = result.ValveDepths == null ? null : JsonSerializer.Serialize(result.ValveDepths);

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Gas Lift Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Gas Lift Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Gas Lift Analysis for WellId: {WellId}", request.WellId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Gas Lift Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetGasLiftAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("GAS_LIFT_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Gas Lift Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Pump Performance Analysis

        /// <summary>
        /// Performs pump performance analysis for a well or facility.
        /// Calculates pump performance curves, efficiency, power requirements, and NPSH.
        /// </summary>
        /// <param name="request">Pump analysis request containing well/facility ID, pump properties, and operating conditions</param>
        /// <returns>Pump analysis result with performance curves, efficiency, and power requirements</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well/facility or equipment data is unavailable</exception>
        public async Task<PumpAnalysisResult> PerformPumpAnalysisAsync(PumpAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.FacilityId))
                {
                    throw new ArgumentException("At least one of WellId or FacilityId must be provided");
                }

                _logger?.LogInformation("Starting Pump Analysis - WellId: {WellId}, FacilityId: {FacilityId}, PumpType: {PumpType}",
                    request.WellId, request.FacilityId, request.PumpType);

                // Step 1: Retrieve equipment data from PPDM
                WELL_EQUIPMENT? wellEquipment = null;
                FACILITY_EQUIPMENT? facilityEquipment = null;

                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    if (!string.IsNullOrEmpty(request.WellId))
                    {
                        wellEquipment = await GetWellEquipmentFromPPDMAsync(request.EquipmentId);
                    }
                    else if (!string.IsNullOrEmpty(request.FacilityId))
                    {
                        var equipment = await GetEntityAsync("FACILITY_EQUIPMENT", request.EquipmentId, "ROW_ID");
                        facilityEquipment = equipment as FACILITY_EQUIPMENT;
                    }
                }

                // Step 2: Build pump properties from request or PPDM data
                // For simplicity, we'll use request values if provided, otherwise use defaults
                var flowRates = request.AdditionalParameters?.FlowRates?.ToArray()
                    ?? new double[] { 100, 200, 300, 400, 500 };

                var heads = request.AdditionalParameters?.Heads?.ToArray()
                    ?? new double[] { 100, 90, 75, 60, 45 };

                var powers = request.AdditionalParameters?.Powers?.ToArray()
                    ?? new double[] { 80, 150, 230, 310, 380 };

                var specificGravity = request.FluidDensity.HasValue
                    ? (double)(request.FluidDensity.Value / 62.4m) // Convert lb/ft³ to specific gravity
                    : 1.0;

                // Step 3: Perform calculation using PumpPerformance library
                var efficiencies = PumpPerformanceCalc.HQCalc(flowRates, heads, powers, specificGravity);
                var hqCurve = HeadQuantityCalculations.GenerateHQCurve(flowRates, heads, powers, specificGravity);
                var bep = HeadQuantityCalculations.FindBestEfficiencyPoint(hqCurve);

                // Calculate NPSH if suction pressure provided
                double? npshAvailable = null;
                double? npshRequired = null;
                bool? cavitationRisk = null;

                if (request.SuctionPressure.HasValue && request.FluidDensity.HasValue)
                {
                    var vaporPressure = request.AdditionalParameters?.VaporPressure ?? 0.5;

                    npshAvailable = NPSHCalculations.CalculateNPSHAvailable(
                        (double)request.SuctionPressure.Value,
                        vaporPressure,
                        (double)request.FluidDensity.Value);

                    // Estimate NPSH required (simplified - would need pump-specific data)
                    npshRequired = bep?.FlowRate * 0.1; // Simplified estimate
                    cavitationRisk = npshAvailable < npshRequired;
                }

                // Step 4: Map to DTO
                var result = new PumpAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    FacilityId = request.FacilityId,
                    EquipmentId = request.EquipmentId,
                    PumpType = request.PumpType,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    FlowRate = request.FlowRate ?? (decimal)(bep?.FlowRate ?? 0),
                    Head = request.Head ?? (decimal)(bep?.Head ?? 0),
                    Power = request.Power ?? (decimal)(bep?.Power ?? 0),
                    Efficiency = request.Efficiency ?? (decimal)(bep?.Efficiency ?? 0),
                    BestEfficiencyPoint = (decimal)(bep?.FlowRate ?? 0),
                    PerformanceCurve = hqCurve.Select(p => new PumpPerformancePoint
                    {
                        FlowRate = (decimal)p.FlowRate,
                        Head = (decimal)p.Head,
                        Power = (decimal)p.Power,
                        Efficiency = (decimal)p.Efficiency
                    }).ToList(),
                    NPSHAvailable = npshAvailable.HasValue ? (decimal)npshAvailable.Value : null,
                    NPSHRequired = npshRequired.HasValue ? (decimal)npshRequired.Value : null,
                    CavitationRisk = cavitationRisk,
                    Status = "SUCCESS",
                    UserId = request.UserId
                };

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetPumpAnalysisResultRepositoryAsync();
                    result.PerformanceCurveJson = JsonSerializer.Serialize(result.PerformanceCurve);

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Pump Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Pump Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Pump Analysis for WellId: {WellId}, FacilityId: {FacilityId}",
                    request.WellId, request.FacilityId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Pump Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetPumpAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("PUMP_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Pump Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Sucker Rod Pumping Analysis

        /// <summary>
        /// Performs sucker rod pumping analysis for a well.
        /// Calculates rod loads, power requirements, production rate, and pump card.
        /// </summary>
        /// <param name="request">Sucker rod analysis request containing well ID, rod system properties, and operating parameters</param>
        /// <returns>Sucker rod analysis result with load analysis, power requirements, and pump card</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well or equipment data is unavailable</exception>
        public async Task<SuckerRodAnalysisResult> PerformSuckerRodAnalysisAsync(SuckerRodAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId))
                {
                    throw new ArgumentException("WellId must be provided");
                }

                _logger?.LogInformation("Starting Sucker Rod Analysis for WellId: {WellId}, AnalysisType: {AnalysisType}",
                    request.WellId, request.AnalysisType);

                // Step 1: Retrieve well data from PPDM
                var well = await GetWellFromPPDMAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well with ID {request.WellId} not found");
                }

                var tubular = await GetWellTubularFromPPDMAsync(request.WellId, "TUBING");
                var wellPressure = await GetWellPressureFromPPDMAsync(request.WellId);
                WELL_EQUIPMENT? equipment = null;
                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    equipment = await GetWellEquipmentFromPPDMAsync(request.EquipmentId);
                }

                // Step 2: Map to domain models using SuckerRodPumpingMapper (if exists) or build from request
                var systemProperties = new SuckerRodSystemProperties
                {
                    WellDepth = request.WellDepth ?? (decimal)(GetPropertyValueMultiple(well, "TOTAL_DEPTH", "DEPTH") ?? 5000m),
                    TubingDiameter = request.TubingDiameter ?? (decimal)(GetTubularInnerDiameterAsync(request.WellId, "TUBING").Result ?? 2.875m),
                    RodDiameter = request.AdditionalParameters?.RodDiameter ?? 0.875m,
                    PumpDiameter = request.PumpDiameter ?? 2.5m,
                    StrokeLength = request.StrokeLength ?? 84m, // inches
                    StrokesPerMinute = request.AdditionalParameters?.StrokesPerMinute ?? 12m,
                    WellheadPressure = request.AdditionalParameters?.WellheadPressure
                        ?? (decimal)(GetPropertyValueMultiple(wellPressure, "PRESSURE", "WELLHEAD_PRESSURE") ?? 100m),
                    BottomHolePressure = request.AdditionalParameters?.BottomHolePressure
                        ?? (decimal)(GetPropertyValueMultiple(wellPressure, "RESERVOIR_PRESSURE", "BOTTOM_HOLE_PRESSURE") ?? 2000m),
                    FluidLevel = request.FluidLevel ?? 4000m,
                    FluidDensity = request.FluidDensity ?? 50m, // lb/ft³
                    OilGravity = request.OilGravity ?? 35m, // API
                    WaterCut = request.WaterCut ?? 0.2m,
                    GasOilRatio = request.AdditionalParameters?.GasOilRatio ?? 500m
                };

                // Build rod string (simplified - single section)
                var rodString = new SuckerRodString
                {
                    Sections = new List<RodSection>
                    {
                        new RodSection
                        {
                            Diameter = systemProperties.RodDiameter,
                            Length = systemProperties.WellDepth,
                            Density = 490m // lb/ft³ (steel)
                        }
                    }
                };

                // Step 3: Perform calculation using SuckerRodPumping library
                var loadResult = SuckerRodLoadCalculator.CalculateLoads(systemProperties, rodString);
                var flowPowerResult = SuckerRodFlowRatePowerCalculator.CalculateFlowRateAndPower(systemProperties, loadResult);

                // Generate pump card (simplified - linear approximation)
                var pumpCard = new List<PumpCardPoint>();
                var strokeLength = (double)systemProperties.StrokeLength;
                for (int i = 0; i <= 20; i++)
                {
                    var position = (decimal)(i * strokeLength / 20.0);
                    var load = loadResult.PeakLoad - (loadResult.LoadRange * (decimal)(i / 20.0));
                    pumpCard.Add(new PumpCardPoint { Position = position, Load = load });
                }

                // Step 4: Map to DTO
                var result = new SuckerRodAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    EquipmentId = request.EquipmentId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    PeakLoad = loadResult.PeakLoad,
                    MinimumLoad = loadResult.MinimumLoad,
                    RodStringWeight = loadResult.RodStringWeight,
                    FluidLoad = loadResult.FluidLoad,
                    DynamicLoad = loadResult.DynamicLoad,
                    MaximumStress = loadResult.MaximumStress,
                    SafetyFactor = loadResult.LoadFactor,
                    PolishedRodHorsepower = flowPowerResult.PolishedRodHorsepower,
                    HydraulicHorsepower = flowPowerResult.HydraulicHorsepower,
                    FrictionHorsepower = flowPowerResult.FrictionHorsepower,
                    TotalPowerRequired = flowPowerResult.TotalHorsepower,
                    ProductionRate = flowPowerResult.ProductionRate,
                    PumpDisplacement = flowPowerResult.PumpDisplacement,
                    VolumetricEfficiency = flowPowerResult.VolumetricEfficiency,
                    PumpCard = pumpCard,
                    Status = "SUCCESS",
                    UserId = request.UserId
                };

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetSuckerRodAnalysisResultRepositoryAsync();
                    result.PumpCardJson = JsonSerializer.Serialize(result.PumpCard);

                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Sucker Rod Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Sucker Rod Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Sucker Rod Analysis for WellId: {WellId}", request.WellId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Sucker Rod Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetSuckerRodAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("SUCKER_ROD_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Sucker Rod Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Compressor Analysis

        /// <summary>
        /// Performs compressor analysis for a facility.
        /// Calculates compressor power requirements, efficiency, and discharge conditions.
        /// </summary>
        /// <param name="request">Compressor analysis request containing facility ID, compressor properties, and operating conditions</param>
        /// <returns>Compressor analysis result with power requirements, efficiency, and discharge conditions</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when facility or equipment data is unavailable</exception>
        public async Task<CompressorAnalysisResult> PerformCompressorAnalysisAsync(CompressorAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.FacilityId))
                {
                    throw new ArgumentException("FacilityId must be provided");
                }

                _logger?.LogInformation("Starting Compressor Analysis for FacilityId: {FacilityId}, CompressorType: {CompressorType}",
                    request.FacilityId, request.CompressorType);

                // Step 1: Retrieve facility and equipment data from PPDM
                var facility = await GetEntityAsync("FACILITY", request.FacilityId, "FACILITY_ID") as FACILITY;
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility with ID {request.FacilityId} not found");
                }

                FACILITY_EQUIPMENT? equipment = null;
                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    equipment = await GetEntityAsync("FACILITY_EQUIPMENT", request.EquipmentId, "ROW_ID") as FACILITY_EQUIPMENT;
                }

                // Step 2: Build compressor properties from request or PPDM data
                var operatingConditions = new CompressorOperatingConditions
                {
                    SuctionPressure = request.SuctionPressure ?? 100m,
                    DischargePressure = request.DischargePressure ?? 500m,
                    SuctionTemperature = request.SuctionTemperature ?? 520m,
                    GasFlowRate = request.FlowRate ?? 1000m,
                    GasSpecificGravity = request.GasSpecificGravity ?? 0.65m,
                    CompressorEfficiency = request.PolytropicEfficiency ?? 0.75m,
                    MechanicalEfficiency = 0.95m
                };

                // Step 3: Perform calculation using CompressorAnalysis library
                CompressorAnalysisResult result;
                if (request.CompressorType.ToUpper() == "RECIPROCATING")
                {
                    var reciprocatingProperties = new ReciprocatingCompressorProperties
                    {
                        OperatingConditions = operatingConditions,
                        CylinderDiameter = request.AdditionalParameters?.CylinderDiameter ?? 8.0m,
                        StrokeLength = request.AdditionalParameters?.StrokeLength ?? 12.0m,
                        RotationalSpeed = request.AdditionalParameters?.RotationalSpeed ?? 300m,
                        NumberOfCylinders = request.NumberOfCylinders ?? 2,
                        VolumetricEfficiency = request.VolumetricEfficiency ?? 0.85m,
                        ClearanceFactor = 0.05m
                    };

                    var calcResult = ReciprocatingCompressorCalculator.CalculatePower(reciprocatingProperties, useSIUnits: false);

                    result = new CompressorAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        FacilityId = request.FacilityId,
                        EquipmentId = request.EquipmentId,
                        CompressorType = request.CompressorType,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        PowerRequired = calcResult.BrakeHorsepower,
                        PolytropicHead = calcResult.PolytropicHead,
                        AdiabaticHead = calcResult.AdiabaticHead,
                        DischargeTemperature = calcResult.DischargeTemperature,
                        PolytropicEfficiency = calcResult.PolytropicEfficiency,
                        AdiabaticEfficiency = calcResult.AdiabaticEfficiency,
                        OverallEfficiency = calcResult.OverallEfficiency,
                        SuctionPressure = operatingConditions.SuctionPressure,
                        DischargePressure = operatingConditions.DischargePressure,
                        CompressionRatio = calcResult.CompressionRatio,
                        FlowRate = operatingConditions.GasFlowRate,
                        CylinderDisplacement = calcResult.CylinderDisplacement,
                        VolumetricEfficiency = calcResult.VolumetricEfficiency,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }
                else
                {
                    // Default to centrifugal
                    var centrifugalProperties = new CentrifugalCompressorProperties
                    {
                        OperatingConditions = operatingConditions,
                        PolytropicEfficiency = request.PolytropicEfficiency ?? 0.75m,
                        SpecificHeatRatio = 1.3m,
                        NumberOfStages = request.NumberOfStages ?? 1,
                        Speed = request.AdditionalParameters?.Speed ?? 3600m
                    };

                    var calcResult = CentrifugalCompressorCalculator.CalculatePower(centrifugalProperties, useSIUnits: false);

                    result = new CompressorAnalysisResult
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        FacilityId = request.FacilityId,
                        EquipmentId = request.EquipmentId,
                        CompressorType = request.CompressorType,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        PowerRequired = calcResult.BrakeHorsepower,
                        PolytropicHead = calcResult.PolytropicHead,
                        AdiabaticHead = calcResult.AdiabaticHead,
                        DischargeTemperature = calcResult.DischargeTemperature,
                        PolytropicEfficiency = calcResult.PolytropicEfficiency,
                        AdiabaticEfficiency = calcResult.AdiabaticEfficiency,
                        OverallEfficiency = calcResult.OverallEfficiency,
                        SuctionPressure = operatingConditions.SuctionPressure,
                        DischargePressure = operatingConditions.DischargePressure,
                        CompressionRatio = calcResult.CompressionRatio,
                        FlowRate = operatingConditions.GasFlowRate,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }

                // Step 4: Store result in PPDM database
                try
                {
                    var repository = await GetCompressorAnalysisResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Compressor Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Compressor Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Compressor Analysis for FacilityId: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Compressor Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetCompressorAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("COMPRESSOR_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Compressor Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Pipeline Analysis

        /// <summary>
        /// Performs pipeline analysis for a pipeline.
        /// Calculates pipeline capacity, flow rate, pressure drop, and flow regime.
        /// </summary>
        /// <param name="request">Pipeline analysis request containing pipeline ID, flow conditions, and product properties</param>
        /// <returns>Pipeline analysis result with flow rate, pressure drop, and capacity</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when pipeline data is unavailable</exception>
        public async Task<PIPELINE_ANALYSIS_RESULT> PerformPipelineAnalysisAsync(PipelineAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.PipelineId))
                {
                    throw new ArgumentException("PipelineId must be provided");
                }

                _logger?.LogInformation("Starting Pipeline Analysis for PipelineId: {PipelineId}, PipelineType: {PipelineType}",
                    request.PipelineId, request.PipelineType);

                // Step 1: Retrieve pipeline data from PPDM
                var pipeline = await GetEntityAsync("PIPELINE", request.PipelineId, "PIPELINE_ID") as PIPELINE;
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline with ID {request.PipelineId} not found");
                }

                // Step 2: Build pipeline properties from request or PPDM data
                var pipelineProperties = new PIPELINE_PROPERTIES
                {
                    LENGTH = request.Length ?? (decimal)(GetPropertyValueMultiple(pipeline, "LENGTH", "TOTAL_LENGTH") ?? 10m), // miles
                    DIAMETER = request.Diameter ?? (decimal)(GetPropertyValueMultiple(pipeline, "DIAMETER", "OUTER_DIAMETER") ?? 12m), // inches
                    ROUGHNESS = request.Roughness ?? 0.00018m, // inches (absolute roughness)
                    INLET_PRESSURE = request.InletPressure ?? 1000m, // psia
                    OUTLET_PRESSURE = request.OutletPressure ?? 500m, // psia
                    AVERAGE_TEMPERATURE = request.Temperature ?? 520m // Rankine
                };

                // Step 3: Perform calculation using PipelineAnalysis library
                PIPELINE_ANALYSIS_RESULT result;
                if (request.PipelineType.ToUpper() == "LIQUID")
                {
                    var liquidProperties = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = pipelineProperties,
                        LIQUID_DENSITY = request.LiquidDensity ?? 50m, // lb/ft³
                        LIQUID_VISCOSITY = request.LiquidViscosity ?? 1.0m, // cP
                        LIQUID_FLOW_RATE = request.FlowRate ?? 1000m // bbl/day
                    };

                    var capacityResult = PipelineCapacityCalculator.CalculateLiquidPipelineCapacity(liquidProperties);

                    result = new PIPELINE_ANALYSIS_RESULT
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        PipelineId = request.PipelineId,
                        PipelineType = request.PipelineType,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        FlowRate = capacityResult.MaximumFlowRate,
                        InletPressure = pipelineProperties.INLET_PRESSURE,
                        OutletPressure = capacityResult.OutletPressure,
                        PressureDrop = capacityResult.PressureDrop,
                        AveragePressure = (pipelineProperties.INLET_PRESSURE + capacityResult.OutletPressure) / 2m,
                        MaximumCapacity = capacityResult.MaximumFlowRate,
                        Utilization = request.FlowRate.HasValue ? (request.FlowRate.Value / capacityResult.MaximumFlowRate) : 0m,
                        ReynoldsNumber = capacityResult.ReynoldsNumber,
                        FrictionFactor = capacityResult.FrictionFactor,
                        FlowRegime = capacityResult.ReynoldsNumber < 2100 ? "LAMINAR" : "TURBULENT",
                        Length = pipelineProperties.LENGTH,
                        Diameter = pipelineProperties.DIAMETER,
                        Roughness = pipelineProperties.ROUGHNESS,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }
                else
                {
                    // Default to gas
                    var gasProperties = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = pipelineProperties,
                        GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                        GAS_FLOW_RATE = request.FlowRate ?? 1000m, // Mscf/day
                        BASE_TEMPERATURE = 520m, // Rankine
                        BASE_PRESSURE = 14.7m // psia
                    };

                    if (request.ZFactor.HasValue)
                    {
                        gasProperties.Z_FACTOR = request.ZFactor.Value;
                    }

                    var capacityResult = PipelineCapacityCalculator.CalculateGasPipelineCapacity(gasProperties);

                    result = new PIPELINE_ANALYSIS_RESULT
                    {
                        CalculationId = Guid.NewGuid().ToString(),
                        PipelineId = request.PipelineId,
                        PipelineType = request.PipelineType,
                        AnalysisType = request.AnalysisType,
                        CalculationDate = DateTime.UtcNow,
                        FlowRate = capacityResult.MAXIMUM_FLOW_RATE,
                        InletPressure = pipelineProperties.INLET_PRESSURE,
                        OutletPressure = capacityResult.OUTLET_PRESSURE,
                        PressureDrop = capacityResult.PRESSURE_DROP,
                        AveragePressure = (pipelineProperties.INLET_PRESSURE + capacityResult.OUTLET_PRESSURE) / 2m,
                        MaximumCapacity = capacityResult.MAXIMUM_FLOW_RATE,
                        Utilization = request.FlowRate.HasValue ? (request.FlowRate.Value / capacityResult.MAXIMUM_FLOW_RATE) : 0m,
                        ReynoldsNumber = capacityResult.REYNOLDS_NUMBER,
                        FrictionFactor = capacityResult.FRICTION_FACTOR,
                        FlowRegime = capacityResult.REYNOLDS_NUMBER < 2100 ? "LAMINAR" : "TURBULENT",
                        Length = pipelineProperties.LENGTH,
                        Diameter = pipelineProperties.DIAMETER,
                        Roughness = pipelineProperties.ROUGHNESS,
                        Status = "SUCCESS",
                        UserId = request.UserId
                    };
                }

                // Step 4: Store result in PPDM database
                try
                {
                    var repository = await GetPipelineAnalysisResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Pipeline Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Pipeline Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Pipeline Analysis for PipelineId: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Pipeline Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetPipelineAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("PIPELINE_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Pipeline Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Plunger Lift Analysis

        /// <summary>
        /// Performs plunger lift analysis for a well.
        /// Calculates plunger lift cycle performance, production rate, and optimization.
        /// </summary>
        /// <param name="request">Plunger lift analysis request containing well ID, plunger properties, and operating conditions</param>
        /// <returns>Plunger lift analysis result with production rate, cycle time, and optimization results</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well or equipment data is unavailable</exception>
        public async Task<PlungerLiftAnalysisResult> PerformPlungerLiftAnalysisAsync(PlungerLiftAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId))
                {
                    throw new ArgumentException("WellId must be provided");
                }

                _logger?.LogInformation("Starting Plunger Lift Analysis for WellId: {WellId}, AnalysisType: {AnalysisType}",
                    request.WellId, request.AnalysisType);

                // Step 1: Retrieve well data from PPDM
                var well = await GetWellFromPPDMAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well with ID {request.WellId} not found");
                }

                var tubular = await GetWellTubularFromPPDMAsync(request.WellId, "TUBING");
                var wellPressure = await GetWellPressureFromPPDMAsync(request.WellId);
                WELL_EQUIPMENT? equipment = null;
                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    equipment = await GetWellEquipmentFromPPDMAsync(request.EquipmentId);
                }

                // Step 2: Map to domain models
                var wellProperties = new PLUNGER_LIFT_WELL_PROPERTIES
                {
                    WellDepth = request.WellDepth ?? (decimal)(GetPropertyValueMultiple(well, "TOTAL_DEPTH", "DEPTH") ?? 5000m),
                    TUBING_DIAMETER = request.TubingDiameter ?? (decimal)(GetTubularInnerDiameterAsync(request.WellId, "TUBING").Result ?? 2.875m),
                    PLUNGER_DIAMETER = request.PlungerDiameter ?? 2.25m,
                    WELLHEAD_PRESSURE = request.WellheadPressure ?? (decimal)(GetPropertyValueMultiple(wellPressure, "PRESSURE", "WELLHEAD_PRESSURE") ?? 100m),
                    CASING_PRESSURE = request.AdditionalParameters?.CasingPressure ?? 200m,
                    BOTTOM_HOLE_PRESSURE = request.BottomHolePressure ?? (decimal)(GetPropertyValueMultiple(wellPressure, "RESERVOIR_PRESSURE", "BOTTOM_HOLE_PRESSURE") ?? 2000m),
                    WELLHEAD_TEMPERATURE = request.WellheadTemperature ?? (decimal)(GetPropertyValueMultiple(wellPressure, "TEMPERATURE", "WELLHEAD_TEMPERATURE") ?? 520m),
                    BOTTOM_HOLE_TEMPERATURE = request.BottomHoleTemperature ?? (decimal)(GetPropertyValueMultiple(wellPressure, "BOTTOM_HOLE_TEMPERATURE") ?? 580m),
                    OIL_GRAVITY = request.OilGravity ?? 35m,
                    WATER_CUT = request.WaterCut ?? 0.2m,
                    GasOilRatio = request.AdditionalParameters?.GasOilRatio ?? 500m,
                    GAS_SPECIFIC_GRAVITY = request.GasSpecificGravity ?? 0.65m,
                    LIQUID_PRODUCTION_RATE = request.LiquidProductionRate ?? 50m
                };

                // Step 3: Perform calculation using PlungerLift library
                var cycleResult = PlungerLiftCalculator.AnalyzeCycle(wellProperties);

                // Step 4: Map to DTO
                var result = new PlungerLiftAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    EquipmentId = request.EquipmentId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    ProductionRate = cycleResult.DailyProductionRate,
                    CycleTime = cycleResult.CycleTime,
                    GasFlowRate = request.GasFlowRate ?? (wellProperties.GasOilRatio * cycleResult.DailyProductionRate / 1000m),
                    PlungerVelocity = cycleResult.RiseVelocity,
                    OptimalCycleTime = cycleResult.CycleTime,
                    OptimalGasFlowRate = wellProperties.GasOilRatio * cycleResult.DailyProductionRate / 1000m,
                    MaximumProductionRate = cycleResult.DailyProductionRate,
                    FallTime = cycleResult.FallTime,
                    RiseTime = cycleResult.RiseTime,
                    ShutInTime = cycleResult.ShutInTime,
                    Status = "SUCCESS",
                    UserId = request.UserId
                };

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetPlungerLiftAnalysisResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Plunger Lift Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Plunger Lift Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Plunger Lift Analysis for WellId: {WellId}", request.WellId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Plunger Lift Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetPlungerLiftAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("PLUNGER_LIFT_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Plunger Lift Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Hydraulic Pump Analysis

        /// <summary>
        /// Performs hydraulic pump analysis for a well.
        /// Calculates hydraulic pump performance, efficiency, and power requirements.
        /// </summary>
        /// <param name="request">Hydraulic pump analysis request containing well ID, pump properties, and power fluid conditions</param>
        /// <returns>Hydraulic pump analysis result with production rate, power requirements, and efficiency</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when well or equipment data is unavailable</exception>
        public async Task<HydraulicPumpAnalysisResult> PerformHydraulicPumpAnalysisAsync(HydraulicPumpAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId))
                {
                    throw new ArgumentException("WellId must be provided");
                }

                _logger?.LogInformation("Starting Hydraulic Pump Analysis for WellId: {WellId}, AnalysisType: {AnalysisType}",
                    request.WellId, request.AnalysisType);

                // Step 1: Retrieve well data from PPDM
                var well = await GetWellFromPPDMAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well with ID {request.WellId} not found");
                }

                var tubular = await GetWellTubularFromPPDMAsync(request.WellId, "TUBING");
                var wellPressure = await GetWellPressureFromPPDMAsync(request.WellId);
                WELL_EQUIPMENT? equipment = null;
                if (!string.IsNullOrEmpty(request.EquipmentId))
                {
                    equipment = await GetWellEquipmentFromPPDMAsync(request.EquipmentId);
                }

                // Step 2: Map to domain models
                var wellProperties = new HydraulicPumpWellProperties
                {
                    WellDepth = request.WellDepth ?? (decimal)(GetPropertyValueMultiple(well, "TOTAL_DEPTH", "DEPTH") ?? 5000m),
                    PumpDepth = request.PumpDepth ?? (decimal)(GetPropertyValueMultiple(well, "TOTAL_DEPTH", "DEPTH") ?? 5000m),
                    TubingDiameter = request.TubingDiameter ?? (decimal)(GetTubularInnerDiameterAsync(request.WellId, "TUBING").Result ?? 2.875m),
                    OilGravity = request.OilGravity ?? 35m,
                    WaterCut = request.WaterCut ?? 0.2m,
                    GasOilRatio = request.GasOilRatio ?? 500m
                };

                var pumpProperties = new HydraulicJetPumpProperties
                {
                    NozzleDiameter = request.NozzleSize ?? 0.25m,
                    ThroatDiameter = request.ThroatSize ?? 0.5m,
                    PowerFluidPressure = request.PowerFluidPressure ?? 2000m,
                    PowerFluidRate = request.PowerFluidRate ?? 100m,
                    PowerFluidSpecificGravity = request.PowerFluidDensity.HasValue
                        ? (request.PowerFluidDensity.Value / 62.4m)
                        : 1.0m
                };

                // Step 3: Perform calculation using HydraulicPumps library
                var performanceResult = HydraulicJetPumpCalculator.CalculatePerformance(wellProperties, pumpProperties);

                // Step 4: Map to DTO
                var result = new HydraulicPumpAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    EquipmentId = request.EquipmentId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    ProductionRate = performanceResult.ProductionRate,
                    PowerFluidRate = pumpProperties.PowerFluidRate,
                    PowerFluidPressure = pumpProperties.PowerFluidPressure,
                    DischargePressure = performanceResult.PumpDischargePressure,
                    SuctionPressure = performanceResult.PumpIntakePressure,
                    HydraulicEfficiency = performanceResult.PumpEfficiency,
                    OverallEfficiency = performanceResult.SystemEfficiency,
                    PowerRequired = performanceResult.PowerFluidHorsepower,
                    RecommendedNozzleSize = pumpProperties.NozzleDiameter,
                    RecommendedThroatSize = pumpProperties.ThroatDiameter,
                    RecommendedPowerFluidRate = pumpProperties.PowerFluidRate,
                    Status = "SUCCESS",
                    UserId = request.UserId
                };

                // Step 5: Store result in PPDM database
                try
                {
                    var repository = await GetHydraulicPumpAnalysisResultRepositoryAsync();
                    await InsertAnalysisResultAsync(repository, result, request.UserId);
                    _logger?.LogInformation("Stored Hydraulic Pump Analysis result with ID: {CalculationId}", result.CalculationId);
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Hydraulic Pump Analysis result");
                    // Continue - don't fail the operation if storage fails
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Hydraulic Pump Analysis for WellId: {WellId}", request.WellId);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates repository for Hydraulic Pump Analysis results
        /// </summary>
        private async Task<PPDMGenericRepository> GetHydraulicPumpAnalysisResultRepositoryAsync()
        {
            try
            {
                return await CreateAnalysisResultRepositoryAsync("HYDRAULIC_PUMP_ANALYSIS_RESULT");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting Hydraulic Pump Analysis result repository");
                throw;
            }
        }

        #endregion

        #region Well Test Analysis Helper Methods

        /// <summary>
        /// Retrieves well test data from PPDM for a well
        /// </summary>
        private Task<WellTestData> GetWellTestDataFromPPDMAsync(string wellId, string testId)
        {
            _logger?.LogWarning("Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request.");

            return Task.FromException<WellTestData>(new InvalidOperationException(
                "Well test data retrieval from PPDM is not implemented. " +
                "Provide PressureTimeData and well properties in the request."));
        }

        /// <summary>
        /// Maps WellTestAnalysisResult from library to WellTestAnalysisResult DTO
        /// </summary>
        private WellTestAnalysisResult MapWellTestResultToDTO(
            WellTestAnalysisResult analysisResult,
            WellTestAnalysisCalculationRequest request,
            ReservoirModel identifiedModel,
            List<PRESSURE_TIME_POINT> derivativePoints)
        {
            var result = new WellTestAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                TestId = request.TestId,
                AnalysisType = request.AnalysisType,
                AnalysisMethod = request.AnalysisMethod ?? "HORNER",
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId,
                Permeability = analysisResult.Permeability,
                SkinFactor = analysisResult.SkinFactor,
                ReservoirPressure =analysisResult.ReservoirPressure,
                ProductivityIndex = analysisResult.ProductivityIndex,
                FlowEfficiency = analysisResult.FlowEfficiency,
                DamageRatio = analysisResult.DamageRatio,
                RadiusOfInvestigation = analysisResult.RadiusOfInvestigation,
                IdentifiedModel = identifiedModel,
                RSquared = analysisResult.RSquared,
                DiagnosticPoints = request.PressureTimeData,
                DerivativePoints = derivativePoints.Select(p => new WellTestDataPoint
                {
                    Time = p.TIME,
                    Pressure = p.PRESSURE
                }).ToList(),
                AdditionalResults = new WellTestAnalysisAdditionalResults()
            };

            result.AdditionalResults.AnalysisMethod = analysisResult.AnalysisMethod;
            result.AdditionalResults.FlowRate = request.FlowRate ?? 0.0m;
            result.AdditionalResults.WellboreRadius = request.WellboreRadius ?? 0.25m;
            result.AdditionalResults.FormationThickness = request.FormationThickness ?? 50.0m;

            return result;
        }

        #endregion

        #region Flash Calculation Helper Methods

        /// <summary>
        /// Retrieves flash conditions from PPDM for a well or facility
        /// </summary>
        private Task<FLASH_CONDITIONS> GetFlashConditionsFromPPDMAsync(string wellId, string facilityId)
        {
            _logger?.LogWarning("Flash conditions retrieval from PPDM is not implemented. " +
                "Provide Pressure, Temperature, and FeedComposition in the request.");

            return Task.FromException<FLASH_CONDITIONS>(new InvalidOperationException(
                "Flash conditions retrieval from PPDM is not implemented. " +
                "Provide Pressure, Temperature, and FeedComposition in the request."));
        }

        /// <summary>
        /// Maps FlashResult from library to FlashCalculationResult DTO
        /// </summary>
        private FlashCalculationResult MapFlashResultToDTO(
            FlashResult flashResult,
            FlashCalculationRequest request,
            PhasePropertiesData vaporProperties,
            PhasePropertiesData liquidProperties)
        {
            var result = new FlashCalculationResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                FacilityId = request.FacilityId,
                CalculationType = request.CalculationType,
                CalculationDate = DateTime.UtcNow,
                Status = flashResult.Converged ? "SUCCESS" : "PARTIAL",
                UserId = request.UserId,
                VaporFraction = flashResult.VaporFraction,
                LiquidFraction = flashResult.LiquidFraction,
                VaporComposition = flashResult.VaporComposition,
                LiquidComposition = flashResult.LiquidComposition,
                KValues = flashResult.KValues,
                Iterations = flashResult.Iterations,
                Converged = flashResult.Converged,
                ConvergenceError = flashResult.ConvergenceError,
                VaporProperties = new PhasePropertiesData
                {
                    Density = vaporProperties.Density,
                    MolecularWeight = vaporProperties.MolecularWeight,
                    SpecificGravity = vaporProperties.SpecificGravity,
                    Volume = vaporProperties.Volume
                },
                LiquidProperties = new PhasePropertiesData
                {
                    Density = liquidProperties.Density,
                    MolecularWeight = liquidProperties.MolecularWeight,
                    SpecificGravity = liquidProperties.SpecificGravity,
                    Volume = liquidProperties.Volume
                },
                AdditionalResults = new FlashCalculationAdditionalResults()
            };

            result.AdditionalResults.Pressure = request.Pressure ?? 0.0m;
            result.AdditionalResults.Temperature = request.Temperature ?? 0.0m;
            result.AdditionalResults.ComponentCount = request.FeedComposition?.Count ?? 0;

            return result;
        }

        /// <summary>
        /// Gets default critical temperature for a component name
        /// </summary>
        private decimal GetDefaultCriticalTemperature(string componentName)
        {
            // Common component critical temperatures (Rankine)
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 343.0m,
                "ETHANE" or "C2H6" => 549.7m,
                "PROPANE" or "C3H8" => 665.7m,
                "BUTANE" or "C4H10" => 765.3m,
                "PENTANE" or "C5H12" => 845.4m,
                "HEXANE" or "C6H14" => 913.4m,
                _ => 500.0m // Default
            };
        }

        /// <summary>
        /// Gets default critical pressure for a component name
        /// </summary>
        private decimal GetDefaultCriticalPressure(string componentName)
        {
            // Common component critical pressures (psia)
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 667.8m,
                "ETHANE" or "C2H6" => 707.8m,
                "PROPANE" or "C3H8" => 616.3m,
                "BUTANE" or "C4H10" => 550.7m,
                "PENTANE" or "C5H12" => 488.6m,
                "HEXANE" or "C6H14" => 436.9m,
                _ => 500.0m // Default
            };
        }

        /// <summary>
        /// Gets default acentric factor for a component name
        /// </summary>
        private decimal GetDefaultAcentricFactor(string componentName)
        {
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 0.0115m,
                "ETHANE" or "C2H6" => 0.0995m,
                "PROPANE" or "C3H8" => 0.1521m,
                "BUTANE" or "C4H10" => 0.2002m,
                "PENTANE" or "C5H12" => 0.2515m,
                "HEXANE" or "C6H14" => 0.3007m,
                _ => 0.2m // Default
            };
        }

        /// <summary>
        /// Gets default molecular weight for a component name
        /// </summary>
        private decimal GetDefaultMolecularWeight(string componentName)
        {
            return componentName.ToUpper() switch
            {
                "METHANE" or "CH4" => 16.04m,
                "ETHANE" or "C2H6" => 30.07m,
                "PROPANE" or "C3H8" => 44.10m,
                "BUTANE" or "C4H10" => 58.12m,
                "PENTANE" or "C5H12" => 72.15m,
                "HEXANE" or "C6H14" => 86.18m,
                _ => 50.0m // Default
            };
        }

        #endregion

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

                // Convert Entity or Dictionary to DTO
                // Use reflection to call the generic method with the correct type
                var method = typeof(PPDMCalculationService).GetMethod("ConvertEntityOrDictionaryToDto", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var genericMethod = method?.MakeGenericMethod(resultType);
                return genericMethod?.Invoke(this, new[] { result });
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
                var forecastType = request.AdditionalParameters?.ForecastType
                    ?? "PSEUDO_STEADY_STATE_SINGLE_PHASE";

                var bottomHolePressure = request.AdditionalParameters?.BottomHolePressure ?? 1000m;
                var forecastDuration = request.AdditionalParameters?.ForecastDuration ?? 1825m;
                var timeSteps = request.AdditionalParameters?.TimeSteps ?? 100;
                var bubblePointPressure = request.AdditionalParameters?.BubblePointPressure ?? 0m;

                // Step 3: Perform physics-based forecast
                PRODUCTION_FORECAST forecast;
                
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

                await repository.InsertAsync(result, request.UserId ?? "system");

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
            var mapping = await _fieldMappingService.GetFieldMappingAsync("Custom.WellboreRadius");
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
            var mapping = await _fieldMappingService.GetFieldMappingAsync(mappingKey);

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
        private async Task<RESERVOIR_FORECAST_PROPERTIES?> GetReservoirPropertiesForForecastAsync(DCARequest request)
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
            var testId = request.AdditionalParameters?.TestId;
            
            DateTime? asOfDate = null;
            if (request.AdditionalParameters?.AsOfDate != null)
            {
                asOfDate = request.AdditionalParameters.AsOfDate.Value;
            }

            // Use KNOWN PPDM fields from standard tables
            // POOL data - static reservoir properties (don't change over time)
            var reservoirProps = new RESERVOIR_FORECAST_PROPERTIES
            {
                // From POOL table - known PPDM 3.9 fields
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
                
                // Wellbore radius - not standard PPDM field, use custom mapping
                WELLBORE_RADIUS = await GetWellboreRadiusAsync(wellId, 0.25m),
                
                // Skin factor from WELL_TEST_ANALYSIS - get latest or by specific test/date
                // null testId = get latest test automatically
                SKIN_FACTOR = await GetWellTestSkinAsync(wellId, testId, asOfDate) ?? 0m
            };

            // Override pool permeability with test-derived permeability if available
            // This is more accurate as it comes from actual well test analysis
            var testPerm = await GetWellTestPermeabilityAsync(wellId, testId, asOfDate);
            if (testPerm.HasValue)
                reservoirProps.PERMEABILITY = testPerm.Value;

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
        private DCAResult MapProductionForecastToDCAResult(
            PRODUCTION_FORECAST forecast,
            DCARequest request)
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

            // Map forecast parameters
            result.InitialRate = forecast.INITIAL_PRODUCTION_RATE;
            result.EstimatedEUR = forecast.TOTAL_CUMULATIVE_PRODUCTION;
            var forecastPointsList = forecast.FORECAST_POINTS?.ToList() ?? new List<FORECAST_POINT>();
            result.DeclineRate = forecastPointsList.Count > 1
                ? (forecast.INITIAL_PRODUCTION_RATE - forecast.FINAL_PRODUCTION_RATE) / 
                  (forecast.INITIAL_PRODUCTION_RATE * (forecast.FORECAST_DURATION / 365.25m))
                : null;

            // Map forecast points (convert time in days to actual dates)
            var startDate = request.StartDate ?? DateTime.UtcNow;
            foreach (var point in forecast.FORECAST_POINTS)
            {
                result.ForecastPoints.Add(new DCAForecastPoint
                {
                    Date = startDate.AddDays((double)point.TIME),
                    ProductionRate = point.PRODUCTION_RATE,
                    CumulativeProduction = point.CUMULATIVE_PRODUCTION,
                    DeclineRate = null // Not calculated for physics-based forecasts
                });
            }

            // Store additional forecast metadata
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
