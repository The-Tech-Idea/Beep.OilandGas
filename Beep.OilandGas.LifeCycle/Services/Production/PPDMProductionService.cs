using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ChokeAnalysis;
using Beep.OilandGas.ChokeAnalysis.Calculations;
using Beep.OilandGas.ChokeAnalysis.Models;
using Beep.OilandGas.SuckerRodPumping;
using Beep.OilandGas.SuckerRodPumping.Calculations;
using Beep.OilandGas.SuckerRodPumping.Models;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Beep.OilandGas.LifeCycle.Services.Production
{
    /// <summary>
    /// Service for Production & Reserves data management
    /// Implements both IPPDMProductionService and IFieldProductionService
    /// </summary>
    public class PPDMProductionService : IPPDMProductionService, IFieldProductionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMProductionService>? _logger;

        public PPDMProductionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMProductionService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName;
            _logger = logger;
        }

        public async Task<List<FIELD>> GetFieldsAsync(List<AppFilter> filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FIELD), _connectionName, "FIELD");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<FIELD>().ToList();
        }

        public async Task<List<POOL>> GetPoolsAsync(List<AppFilter> filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(POOL), _connectionName, "POOL");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<POOL>().ToList();
        }

        public async Task<List<PDEN_VOL_SUMMARY>> GetProductionAsync(List<AppFilter> filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<PDEN_VOL_SUMMARY>().ToList();
        }

        public async Task<List<RESERVE_ENTITY>> GetReservesAsync(List<AppFilter> filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(RESERVE_ENTITY), _connectionName, "RESERVE_ENTITY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<RESERVE_ENTITY>().ToList();
        }

        public async Task<List<PDEN_VOL_SUMMARY>> GetProductionReportingAsync(List<AppFilter> filters = null)
        {
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY");

            var results = await repo.GetAsync(filters ?? new List<AppFilter>());
            return results.Cast<PDEN_VOL_SUMMARY>().ToList();
        }

        // ============================================
        // IFieldProductionService Implementation
        // ============================================

        public async Task<List<ProductionResponse>> GetProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                // Production is linked to wells, which are linked to fields
                // We need to get wells for the field first, then get production for those wells
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                    return new List<ProductionResponse>();

                var wellEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{wellMetadata.EntityTypeName}");
                if (wellEntityType == null)
                    return new List<ProductionResponse>();

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    wellEntityType, _connectionName, "WELL");

                // Get all wells for the field
                var wells = await wellRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                        Operator = "="
                    }
                });

                if (!wells.Any())
                    return new List<ProductionResponse>();

                // Extract well IDs using reflection
                var wellIds = new List<string>();
                foreach (var well in wells)
                {
                    var wellIdProp = wellEntityType.GetProperty("WELL_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (wellIdProp != null)
                    {
                        var wellIdValue = wellIdProp.GetValue(well)?.ToString();
                        if (!string.IsNullOrEmpty(wellIdValue))
                            wellIds.Add(wellIdValue);
                    }
                }

                if (!wellIds.Any())
                    return new List<ProductionResponse>();

                // Get production for these wells
                var productionMetadata = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
                if (productionMetadata == null)
                    return new List<ProductionResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{productionMetadata.EntityTypeName}");
                if (entityType == null)
                    return new List<ProductionResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PDEN_VOL_SUMMARY");

                // Filter by well IDs (OR condition)
                var filters = new List<AppFilter>();
                foreach (var wellId in wellIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "WELL_ID",
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", wellId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionResponse), entityType);
                return dtoList.Cast<ProductionResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production for field: {fieldId}", ex);
            }
        }

        public async Task<ProductionResponse> CreateProductionForFieldAsync(string fieldId, ProductionRequest productionData, string userId)
        {
            var metadata = await _metadata.GetTableMetadataAsync("PDEN_VOL_SUMMARY");
            if (metadata == null)
                throw new InvalidOperationException("PDEN_VOL_SUMMARY table metadata not found");

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
            if (entityType == null)
                throw new InvalidOperationException($"Entity type not found for PDEN_VOL_SUMMARY: {metadata.EntityTypeName}");

            // Convert DTO to PPDM model
            var productionEntity = _mappingService.ConvertDTOToPPDMModelRuntime(productionData, typeof(ProductionRequest), entityType);
            
            // Set FIELD_ID automatically using reflection
            var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            if (fieldIdProp != null && fieldIdProp.CanWrite)
            {
                fieldIdProp.SetValue(productionEntity, _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", fieldId));
            }

            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, _connectionName, "PDEN_VOL_SUMMARY");

            var result = await repo.InsertAsync(productionEntity, userId);
            
            // Convert PPDM model back to DTO
            return (ProductionResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(ProductionResponse), entityType);
        }

        public async Task<List<ProductionResponse>> GetProductionByPoolForFieldAsync(string fieldId, string? poolId = null)
        {
            try
            {
                // Get production for field, then filter by pool if specified
                var production = await GetProductionForFieldAsync(fieldId);

                if (string.IsNullOrEmpty(poolId))
                    return production;

                // Filter by pool ID (assuming PDEN_VOL_SUMMARY has POOL_ID field)
                var formattedPoolId = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", poolId);
                return production.Where(p => p.PoolId == formattedPoolId).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production by pool for field: {fieldId}", ex);
            }
        }

        public async Task<List<ReservesResponse>> GetReservesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("RESERVE_ENTITY");
                if (metadata == null)
                    return new List<ReservesResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    return new List<ReservesResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "RESERVE_ENTITY");

                // Filter by field ID (assuming RESERVE_ENTITY has FIELD_ID or is linked through pool/well)
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("RESERVE_ENTITY", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ReservesResponse), entityType);
                return dtoList.Cast<ReservesResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting reserves for field: {fieldId}", ex);
            }
        }

        public async Task<List<WellTestResponse>> GetWellTestsForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                // Validate well belongs to field first by checking WELL table
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                    throw new InvalidOperationException("WELL table metadata not found");

                var wellEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{wellMetadata.EntityTypeName}");
                if (wellEntityType == null)
                    throw new InvalidOperationException($"Entity type not found for WELL: {wellMetadata.EntityTypeName}");

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    wellEntityType, _connectionName, "WELL");

                var formattedWellId = _defaults.FormatIdForTable("WELL", wellId);
                var well = await wellRepo.GetByIdAsync(formattedWellId);
                
                if (well == null)
                    throw new InvalidOperationException($"Well {wellId} not found");

                // Validate well belongs to field using reflection
                var fieldIdProp = wellEntityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null)
                {
                    var wellFieldId = fieldIdProp.GetValue(well)?.ToString();
                    var formattedFieldId = _defaults.FormatIdForTable("WELL", fieldId);
                    if (wellFieldId != formattedFieldId)
                        throw new InvalidOperationException($"Well {wellId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("WELL_TEST");
                if (metadata == null)
                    return new List<WellTestResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    return new List<WellTestResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL_TEST");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "WELL_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_TEST", wellId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(WellTestResponse), entityType);
                return dtoList.Cast<WellTestResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting well tests for well {wellId} in field: {fieldId}", ex);
            }
        }

        public async Task<List<ProductionForecastResponse>> GetProductionForecastsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_FORECAST");
                if (metadata == null)
                    return new List<ProductionForecastResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    return new List<ProductionForecastResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PRODUCTION_FORECAST");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("PRODUCTION_FORECAST", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionForecastResponse), entityType);
                return dtoList.Cast<ProductionForecastResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting production forecasts for field: {fieldId}", ex);
            }
        }

        public async Task<ProductionForecastResponse> CreateProductionForecastForFieldAsync(string fieldId, ProductionForecastRequest forecastData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_FORECAST");
                if (metadata == null)
                    throw new InvalidOperationException("PRODUCTION_FORECAST table metadata not found");

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    throw new InvalidOperationException($"Entity type not found for PRODUCTION_FORECAST: {metadata.EntityTypeName}");

                // Convert DTO to PPDM model
                var forecastEntity = _mappingService.ConvertDTOToPPDMModelRuntime(forecastData, typeof(ProductionForecastRequest), entityType);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(forecastEntity, _defaults.FormatIdForTable("PRODUCTION_FORECAST", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PRODUCTION_FORECAST");

                var result = await repo.InsertAsync(forecastEntity, userId);
                
                // Convert PPDM model back to DTO
                return (ProductionForecastResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(ProductionForecastResponse), entityType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating production forecast for field: {fieldId}", ex);
            }
        }

        public async Task<List<ProductionResponse>> GetFacilityProductionForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PRODUCTION_FACILITY");
                if (metadata == null)
                    return new List<ProductionResponse>();

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    return new List<ProductionResponse>();

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PRODUCTION_FACILITY");

                // Get facilities for field first, then get production for those facilities
                var facilityMetadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (facilityMetadata == null)
                    return new List<ProductionResponse>();

                var facilityEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{facilityMetadata.EntityTypeName}");
                if (facilityEntityType == null)
                    return new List<ProductionResponse>();

                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    facilityEntityType, _connectionName, "FACILITY");

                var facilities = await facilityRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    }
                });

                // Extract facility IDs using reflection
                var facilityIds = new List<string>();
                foreach (var facility in facilities)
                {
                    var facilityIdProp = facilityEntityType.GetProperty("FACILITY_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (facilityIdProp != null)
                    {
                        var facilityIdValue = facilityIdProp.GetValue(facility)?.ToString();
                        if (!string.IsNullOrEmpty(facilityIdValue))
                            facilityIds.Add(facilityIdValue);
                    }
                }

                if (!facilityIds.Any())
                    return new List<ProductionResponse>();

                var filters = new List<AppFilter>();
                foreach (var facilityId in facilityIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "FACILITY_ID",
                        FilterValue = _defaults.FormatIdForTable("PRODUCTION_FACILITY", facilityId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs (using ProductionResponse since facility production is still production data)
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(ProductionResponse), entityType);
                return dtoList.Cast<ProductionResponse>().ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error getting facility production for field: {fieldId}", ex);
            }
        }

        #region Choke Analysis Integration

        /// <summary>
        /// Analyzes choke flow for a well (downhole or uphole)
        /// </summary>
        public async Task<ChokeFlowResult> AnalyzeChokeFlowAsync(
            string fieldId,
            string wellId,
            string chokeLocation = "DOWNHOLE", // DOWNHOLE or UPHOLE
            decimal? chokeDiameter = null,
            decimal? upstreamPressure = null,
            decimal? downstreamPressure = null,
            decimal? gasFlowRate = null,
            decimal? gasSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing choke flow for well {WellId} in field {FieldId}, location: {Location}",
                    wellId, fieldId, chokeLocation);

                // Get choke and gas properties from PPDM or request
                var chokeProperties = await GetChokePropertiesAsync(wellId, chokeDiameter);
                var gasProperties = await GetGasChokePropertiesAsync(
                    fieldId, wellId, upstreamPressure, downstreamPressure, gasFlowRate, gasSpecificGravity);

                // Calculate choke flow
                ChokeFlowResult result;
                if (chokeLocation.ToUpper() == "UPHOLE")
                {
                    result = GasChokeCalculator.CalculateUpholeChokeFlow(chokeProperties, gasProperties);
                }
                else
                {
                    result = GasChokeCalculator.CalculateDownholeChokeFlow(chokeProperties, gasProperties);
                }

                // Store results in WELL_EQUIPMENT table
                await StoreChokeFlowResultsAsync(wellId, result, chokeLocation, userId ?? "SYSTEM");

                _logger?.LogInformation("Choke flow analysis completed for well {WellId}. Flow rate: {FlowRate} Mscf/day, Flow regime: {Regime}",
                    wellId, result.FlowRate, result.FlowRegime);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing choke flow for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Calculates required choke size for a target flow rate
        /// </summary>
        public async Task<ChokeProperties> CalculateChokeSizingAsync(
            string fieldId,
            string wellId,
            decimal targetFlowRate,
            string chokeLocation = "DOWNHOLE",
            decimal? upstreamPressure = null,
            decimal? downstreamPressure = null,
            decimal? gasSpecificGravity = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Calculating choke sizing for well {WellId} in field {FieldId}, target flow rate: {FlowRate} Mscf/day",
                    wellId, fieldId, targetFlowRate);

                // Get gas properties
                var gasProperties = await GetGasChokePropertiesAsync(
                    fieldId, wellId, upstreamPressure, downstreamPressure, targetFlowRate, gasSpecificGravity);

                // Iterate to find optimal choke diameter
                decimal minDiameter = 0.125m; // 1/8 inch
                decimal maxDiameter = 2.0m; // 2 inches
                decimal optimalDiameter = 0.5m; // Start with 1/2 inch

                for (int i = 0; i < 20; i++) // Max 20 iterations
                {
                    var chokeProperties = new ChokeProperties
                    {
                        ChokeDiameter = optimalDiameter,
                        ChokeType = ChokeType.Bean,
                        DischargeCoefficient = 0.85m
                    };

                    ChokeFlowResult result;
                    if (chokeLocation.ToUpper() == "UPHOLE")
                    {
                        result = GasChokeCalculator.CalculateUpholeChokeFlow(chokeProperties, gasProperties);
                    }
                    else
                    {
                        result = GasChokeCalculator.CalculateDownholeChokeFlow(chokeProperties, gasProperties);
                    }

                    decimal error = Math.Abs(result.FlowRate - targetFlowRate);
                    if (error < 1.0m) // Within 1 Mscf/day
                    {
                        break;
                    }

                    // Adjust diameter based on flow rate difference
                    if (result.FlowRate < targetFlowRate)
                    {
                        minDiameter = optimalDiameter;
                        optimalDiameter = (optimalDiameter + maxDiameter) / 2m;
                    }
                    else
                    {
                        maxDiameter = optimalDiameter;
                        optimalDiameter = (minDiameter + optimalDiameter) / 2m;
                    }
                }

                var finalChoke = new ChokeProperties
                {
                    ChokeDiameter = optimalDiameter,
                    ChokeType = ChokeType.Bean,
                    DischargeCoefficient = 0.85m
                };

                // Store sizing results
                await StoreChokeSizingResultsAsync(wellId, finalChoke, targetFlowRate, chokeLocation, userId ?? "SYSTEM");

                _logger?.LogInformation("Choke sizing completed for well {WellId}. Optimal diameter: {Diameter} inches",
                    wellId, optimalDiameter);

                return finalChoke;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating choke sizing for well {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Choke Analysis Helper Methods

        /// <summary>
        /// Retrieves choke properties from PPDM
        /// </summary>
        private async Task<ChokeProperties> GetChokePropertiesAsync(string wellId, decimal? chokeDiameter)
        {
            try
            {
                // Get well equipment data from PPDM
                // This is a simplified implementation - in production, you would retrieve actual values
                var chokeProperties = new ChokeProperties
                {
                    ChokeDiameter = chokeDiameter ?? 0.5m, // Default 1/2 inch
                    ChokeType = ChokeType.Bean,
                    DischargeCoefficient = 0.85m // Typical for bean choke
                };

                _logger?.LogWarning("Using default values for choke properties. For accurate analysis, provide choke diameter in request or ensure PPDM data is complete.");

                return chokeProperties;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving choke properties");
                throw;
            }
        }

        /// <summary>
        /// Retrieves gas properties for choke analysis from PPDM
        /// </summary>
        private async Task<GasChokeProperties> GetGasChokePropertiesAsync(
            string fieldId,
            string wellId,
            decimal? upstreamPressure,
            decimal? downstreamPressure,
            decimal? gasFlowRate,
            decimal? gasSpecificGravity)
        {
            try
            {
                // Get well/production data from PPDM
                // This is a simplified implementation - in production, you would retrieve actual values
                var gasProperties = new GasChokeProperties
                {
                    UpstreamPressure = upstreamPressure ?? 1000m, // Default 1000 psia
                    DownstreamPressure = downstreamPressure ?? 500m, // Default 500 psia
                    Temperature = 540m, // Default 80°F = 540°R
                    GasSpecificGravity = gasSpecificGravity ?? 0.65m,
                    FlowRate = gasFlowRate ?? 1000m, // Default 1000 Mscf/day
                    ZFactor = 0.9m // Default - would calculate from pressure/temperature
                };

                _logger?.LogWarning("Using default values for some gas properties. For accurate analysis, provide values in request or ensure PPDM data is complete.");

                return gasProperties;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving gas properties for choke analysis");
                throw;
            }
        }

        /// <summary>
        /// Stores choke flow analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreChokeFlowResultsAsync(
            string wellId,
            ChokeFlowResult result,
            string chokeLocation,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "CHOKE",
                    ["EQUIPMENT_NAME"] = $"{chokeLocation} Choke Flow Analysis",
                    ["DESCRIPTION"] = $"Flow rate: {result.FlowRate:F2} Mscf/day, Flow regime: {result.FlowRegime}",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ChokeLocation = chokeLocation,
                        FlowRate = result.FlowRate,
                        UpstreamPressure = result.UpstreamPressure,
                        DownstreamPressure = result.DownstreamPressure,
                        PressureRatio = result.PressureRatio,
                        FlowRegime = result.FlowRegime.ToString(),
                        CriticalPressureRatio = result.CriticalPressureRatio,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored choke flow results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing choke flow results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores choke sizing results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreChokeSizingResultsAsync(
            string wellId,
            ChokeProperties chokeProperties,
            decimal targetFlowRate,
            string chokeLocation,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "CHOKE",
                    ["EQUIPMENT_NAME"] = $"{chokeLocation} Choke Sizing",
                    ["DESCRIPTION"] = $"Optimal diameter: {chokeProperties.ChokeDiameter:F3} inches for {targetFlowRate:F2} Mscf/day",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ChokeLocation = chokeLocation,
                        ChokeDiameter = chokeProperties.ChokeDiameter,
                        ChokeType = chokeProperties.ChokeType.ToString(),
                        DischargeCoefficient = chokeProperties.DischargeCoefficient,
                        ChokeArea = chokeProperties.ChokeArea,
                        TargetFlowRate = targetFlowRate,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored choke sizing results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing choke sizing results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Sucker Rod Pumping Integration

        /// <summary>
        /// Analyzes sucker rod load for a well
        /// </summary>
        public async Task<SuckerRodLoadResult> AnalyzeSuckerRodLoadAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength = null,
            decimal? strokesPerMinute = null,
            decimal? rodDiameter = null,
            decimal? pumpDiameter = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing sucker rod load for well {WellId} in field {FieldId}",
                    wellId, fieldId);

                // Get sucker rod system properties from PPDM
                var systemProperties = await GetSuckerRodSystemPropertiesAsync(
                    fieldId, wellId, strokeLength, strokesPerMinute, rodDiameter, pumpDiameter);
                var rodString = await GetSuckerRodStringAsync(wellId, systemProperties);

                // Calculate loads
                var result = SuckerRodLoadCalculator.CalculateLoads(systemProperties, rodString);

                // Store results in WELL_EQUIPMENT table
                await StoreSuckerRodLoadResultsAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Sucker rod load analysis completed for well {WellId}. Peak load: {PeakLoad} lbs, Stress range: {StressRange} psi",
                    wellId, result.PeakLoad, result.StressRange);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing sucker rod load for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes sucker rod power requirements and production rate
        /// </summary>
        public async Task<SuckerRodFlowRatePowerResult> AnalyzeSuckerRodPowerAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength = null,
            decimal? strokesPerMinute = null,
            decimal? rodDiameter = null,
            decimal? pumpDiameter = null,
            string? userId = null)
        {
            try
            {
                _logger?.LogInformation("Analyzing sucker rod power for well {WellId} in field {FieldId}",
                    wellId, fieldId);

                // Get system properties
                var systemProperties = await GetSuckerRodSystemPropertiesAsync(
                    fieldId, wellId, strokeLength, strokesPerMinute, rodDiameter, pumpDiameter);
                var rodString = await GetSuckerRodStringAsync(wellId, systemProperties);

                // Calculate loads first
                var loadResult = SuckerRodLoadCalculator.CalculateLoads(systemProperties, rodString);

                // Calculate flow rate and power
                var result = SuckerRodFlowRatePowerCalculator.CalculateFlowRateAndPower(
                    systemProperties, loadResult);

                // Store results in WELL_EQUIPMENT table
                await StoreSuckerRodPowerResultsAsync(wellId, result, userId ?? "SYSTEM");

                _logger?.LogInformation("Sucker rod power analysis completed for well {WellId}. Production rate: {Rate} bbl/day, Motor HP: {MotorHP}",
                    wellId, result.ProductionRate, result.MotorHorsepower);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing sucker rod power for well {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Sucker Rod Pumping Helper Methods

        /// <summary>
        /// Retrieves sucker rod system properties from PPDM
        /// </summary>
        private async Task<SuckerRodSystemProperties> GetSuckerRodSystemPropertiesAsync(
            string fieldId,
            string wellId,
            decimal? strokeLength,
            decimal? strokesPerMinute,
            decimal? rodDiameter,
            decimal? pumpDiameter)
        {
            try
            {
                // Get well data from PPDM
                // This is a simplified implementation - in production, you would retrieve actual values
                var systemProperties = new SuckerRodSystemProperties
                {
                    WellDepth = 5000m, // Default - would retrieve from WELL table
                    TubingDiameter = 2.875m, // Default - would retrieve from WELL_EQUIPMENT
                    RodDiameter = rodDiameter ?? 0.875m, // Default 7/8 inch
                    PumpDiameter = pumpDiameter ?? 1.5m, // Default 1.5 inches
                    StrokeLength = strokeLength ?? 60m, // Default 60 inches
                    StrokesPerMinute = strokesPerMinute ?? 10m, // Default 10 SPM
                    WellheadPressure = 100m, // Default - would retrieve from production data
                    BottomHolePressure = 2000m, // Default - would retrieve from reservoir data
                    OilGravity = 35m, // Default 35 API
                    WaterCut = 0.3m, // Default 30%
                    GasOilRatio = 500m, // Default 500 scf/bbl
                    GasSpecificGravity = 0.65m, // Default
                    RodDensity = 490m, // Steel
                    PumpEfficiency = 0.85m
                };

                _logger?.LogWarning("Using default values for some sucker rod system properties. For accurate analysis, provide values in request or ensure PPDM data is complete.");

                return systemProperties;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving sucker rod system properties");
                throw;
            }
        }

        /// <summary>
        /// Gets sucker rod string configuration from PPDM
        /// </summary>
        private async Task<SuckerRodString> GetSuckerRodStringAsync(
            string wellId,
            SuckerRodSystemProperties systemProperties)
        {
            // This is a simplified implementation - in production, you would retrieve actual rod string from PPDM
            var rodString = new SuckerRodString
            {
                TotalLength = systemProperties.WellDepth
            };

            // Create a single rod section (simplified - actual systems may have multiple sections)
            var section = new RodSection
            {
                Diameter = systemProperties.RodDiameter,
                Length = systemProperties.WellDepth,
                Density = systemProperties.RodDensity
            };

            rodString.Sections.Add(section);

            return rodString;
        }

        /// <summary>
        /// Stores sucker rod load analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreSuckerRodLoadResultsAsync(
            string wellId,
            SuckerRodLoadResult result,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "SUCKER_ROD_PUMP",
                    ["EQUIPMENT_NAME"] = "Sucker Rod Load Analysis",
                    ["DESCRIPTION"] = $"Peak load: {result.PeakLoad:F2} lbs, Stress range: {result.StressRange:F2} psi",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        PeakLoad = result.PeakLoad,
                        MinimumLoad = result.MinimumLoad,
                        PolishedRodLoad = result.PolishedRodLoad,
                        RodStringWeight = result.RodStringWeight,
                        FluidLoad = result.FluidLoad,
                        DynamicLoad = result.DynamicLoad,
                        LoadRange = result.LoadRange,
                        StressRange = result.StressRange,
                        MaximumStress = result.MaximumStress,
                        LoadFactor = result.LoadFactor,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored sucker rod load results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing sucker rod load results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        /// <summary>
        /// Stores sucker rod power analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreSuckerRodPowerResultsAsync(
            string wellId,
            SuckerRodFlowRatePowerResult result,
            string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    _logger?.LogWarning("WELL_EQUIPMENT table metadata not found, skipping storage");
                    return;
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Dictionary<string, object>), _connectionName, "WELL_EQUIPMENT", null);

                var equipmentRecord = new Dictionary<string, object>
                {
                    ["WELL_EQUIPMENT_ID"] = Guid.NewGuid().ToString(),
                    ["WELL_ID"] = wellId,
                    ["EQUIPMENT_TYPE"] = "SUCKER_ROD_PUMP",
                    ["EQUIPMENT_NAME"] = "Sucker Rod Power Analysis",
                    ["DESCRIPTION"] = $"Production rate: {result.ProductionRate:F2} bbl/day, Motor HP: {result.MotorHorsepower:F2}",
                    ["EQUIPMENT_DATA_JSON"] = JsonSerializer.Serialize(new
                    {
                        ProductionRate = result.ProductionRate,
                        PumpDisplacement = result.PumpDisplacement,
                        VolumetricEfficiency = result.VolumetricEfficiency,
                        PolishedRodHorsepower = result.PolishedRodHorsepower,
                        HydraulicHorsepower = result.HydraulicHorsepower,
                        FrictionHorsepower = result.FrictionHorsepower,
                        TotalHorsepower = result.TotalHorsepower,
                        MotorHorsepower = result.MotorHorsepower,
                        SystemEfficiency = result.SystemEfficiency,
                        EnergyConsumption = result.EnergyConsumption,
                        AnalysisDate = DateTime.UtcNow
                    })
                };

                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored sucker rod power results for well {WellId}", wellId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error storing sucker rod power results for well {WellId}", wellId);
                // Don't throw - storage failure shouldn't fail the operation
            }
        }

        #endregion

        #region Production Operations Integration

        /// <summary>
        /// Manages production operations using ProductionOperations service
        /// </summary>
        public async Task<ProductionResponse> ManageProductionOperationsAsync(string fieldId, string wellId, ProductionRequest productionData, string userId)
        {
            try
            {
                _logger?.LogInformation("Managing production operations for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Use existing CreateProductionForFieldAsync method
                return await CreateProductionForFieldAsync(fieldId, productionData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error managing production operations for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Optimizes production using ProductionOperations service
        /// </summary>
        public async Task<ProductionResponse> OptimizeProductionAsync(string fieldId, string wellId, Dictionary<string, object> optimizationParameters, string userId)
        {
            try
            {
                _logger?.LogInformation("Optimizing production for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Get current production
                var production = await GetProductionForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId }
                });

                var currentProd = production.FirstOrDefault();
                if (currentProd == null)
                {
                    throw new InvalidOperationException($"No production data found for well {wellId}");
                }

                // Apply optimization (simplified - in full implementation would use ProductionManagementService)
                // For now, return current production
                return currentProd;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing production for well: {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Enhanced Recovery Integration

        /// <summary>
        /// Plans enhanced recovery operation using EnhancedRecovery service
        /// </summary>
        public async Task<EnhancedRecoveryOperationDto> PlanEnhancedRecoveryAsync(string fieldId, CreateEnhancedRecoveryOperationDto eorData, string userId)
        {
            try
            {
                _logger?.LogInformation("Planning enhanced recovery for field: {FieldId}", fieldId);

                // Create EOR plan in PPDM
                // In full implementation, would use EnhancedRecoveryService

                return new EnhancedRecoveryOperationDto
                {
                    OperationId = Guid.NewGuid().ToString(),
                    FieldId = fieldId,
                    EorMethod = eorData.EorMethod ?? "WATER_FLOODING",
                    Status = "PLANNED",
                    StartDate = eorData.StartDate ?? DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning enhanced recovery for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Executes enhanced recovery operation using EnhancedRecovery service
        /// </summary>
        public async Task<EnhancedRecoveryOperationDto> ExecuteEnhancedRecoveryAsync(string fieldId, string operationId, Dictionary<string, object> executionParameters, string userId)
        {
            try
            {
                _logger?.LogInformation("Executing enhanced recovery operation: {OperationId} for field: {FieldId}", operationId, fieldId);
using Beep.OilandGas.Models.Data.ProductionForecasting;

                // Update EOR operation status to EXECUTING
                // In full implementation, would use EnhancedRecoveryService

                return new EnhancedRecoveryOperationDto
                {
                    OperationId = operationId,
                    FieldId = fieldId,
                    Status = "EXECUTING",
                    StartDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing enhanced recovery operation: {OperationId}", operationId);
                throw;
            }
        }

        #endregion
    }
}



