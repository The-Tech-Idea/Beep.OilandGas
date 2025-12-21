using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

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

        public PPDMProductionService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName;
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
    }
}



