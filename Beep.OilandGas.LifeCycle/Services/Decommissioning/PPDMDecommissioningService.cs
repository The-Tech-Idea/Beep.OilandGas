using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace Beep.OilandGas.LifeCycle.Services.Decommissioning
{
    /// <summary>
    /// Service for Decommissioning phase data management, field-scoped
    /// </summary>
    public class PPDMDecommissioningService : IFieldDecommissioningService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<PPDMDecommissioningService>? _logger;

        public PPDMDecommissioningService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<PPDMDecommissioningService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public async Task<List<WellAbandonmentResponse>> GetAbandonedWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT");
                if (metadata == null)
                {
                    _logger?.LogWarning("WELL_ABANDONMENT table metadata not found");
                    return new List<WellAbandonmentResponse>();
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    _logger?.LogWarning($"Entity type not found for WELL_ABANDONMENT: {metadata.EntityTypeName}");
                    return new List<WellAbandonmentResponse>();
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL_ABANDONMENT");

                // WELL_ABANDONMENT is linked to WELL, which is linked to FIELD
                // Get wells for field first, then get abandonments for those wells
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                    return new List<WellAbandonmentResponse>();

                var wellEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{wellMetadata.EntityTypeName}");
                if (wellEntityType == null)
                    return new List<WellAbandonmentResponse>();

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    wellEntityType, _connectionName, "WELL");

                var wells = await wellRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                        Operator = "="
                    }
                });

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
                    return new List<WellAbandonmentResponse>();

                var filters = new List<AppFilter>();
                foreach (var wellId in wellIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "WELL_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_ABANDONMENT", wellId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(WellAbandonmentResponse), entityType);
                return dtoList.Cast<WellAbandonmentResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting abandoned wells for field: {fieldId}");
                throw;
            }
        }

        public async Task<WellAbandonmentResponse> AbandonWellForFieldAsync(string fieldId, string wellId, WellAbandonmentRequest abandonmentData, string userId)
        {
            try
            {
                // Validate well belongs to field
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

                var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT");
                if (metadata == null)
                    throw new InvalidOperationException("WELL_ABANDONMENT table metadata not found");

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    throw new InvalidOperationException($"Entity type not found for WELL_ABANDONMENT: {metadata.EntityTypeName}");

                // Convert DTO to PPDM model
                var abandonmentEntity = _mappingService.ConvertDTOToPPDMModelRuntime(abandonmentData, typeof(WellAbandonmentRequest), entityType);
                
                // Set WELL_ID and FIELD_ID automatically using reflection
                var wellIdProp = entityType.GetProperty("WELL_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (wellIdProp != null && wellIdProp.CanWrite)
                {
                    wellIdProp.SetValue(abandonmentEntity, formattedWellId);
                }
                var fieldIdProp2 = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp2 != null && fieldIdProp2.CanWrite)
                {
                    fieldIdProp2.SetValue(abandonmentEntity, _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL_ABANDONMENT");

                var result = await repo.InsertAsync(abandonmentEntity, userId);
                
                // Convert PPDM model back to DTO
                return (WellAbandonmentResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(WellAbandonmentResponse), entityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error abandoning well {wellId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<WellAbandonmentResponse?> GetWellAbandonmentForFieldAsync(string fieldId, string abandonmentId)
        {
            try
            {
                var abandonedWells = await GetAbandonedWellsForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "WELL_ABANDONMENT_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_ABANDONMENT", abandonmentId),
                        Operator = "="
                    }
                });

                return abandonedWells.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting well abandonment {abandonmentId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_DECOMMISSIONING");
                if (metadata == null)
                {
                    _logger?.LogWarning("FACILITY_DECOMMISSIONING table metadata not found");
                    return new List<FacilityDecommissioningResponse>();
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    _logger?.LogWarning($"Entity type not found for FACILITY_DECOMMISSIONING: {metadata.EntityTypeName}");
                    return new List<FacilityDecommissioningResponse>();
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY_DECOMMISSIONING");

                // FACILITY_DECOMMISSIONING is linked to FACILITY, which is linked to FIELD
                var facilityMetadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (facilityMetadata == null)
                    return new List<FacilityDecommissioningResponse>();

                var facilityEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{facilityMetadata.EntityTypeName}");
                if (facilityEntityType == null)
                    return new List<FacilityDecommissioningResponse>();

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
                    return new List<FacilityDecommissioningResponse>();

                var filters = new List<AppFilter>();
                foreach (var facilityId in facilityIds)
                {
                    filters.Add(new AppFilter
                    {
                        FieldName = "FACILITY_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY_DECOMMISSIONING", facilityId),
                        Operator = "="
                    });
                }

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(FacilityDecommissioningResponse), entityType);
                return dtoList.Cast<FacilityDecommissioningResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting decommissioned facilities for field: {fieldId}");
                throw;
            }
        }

        public async Task<FacilityDecommissioningResponse> DecommissionFacilityForFieldAsync(string fieldId, string facilityId, FacilityDecommissioningRequest decommissionData, string userId)
        {
            try
            {
                // Validate facility belongs to field
                var facilityMetadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (facilityMetadata == null)
                    throw new InvalidOperationException("FACILITY table metadata not found");

                var facilityEntityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{facilityMetadata.EntityTypeName}");
                if (facilityEntityType == null)
                    throw new InvalidOperationException($"Entity type not found for FACILITY: {facilityMetadata.EntityTypeName}");

                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    facilityEntityType, _connectionName, "FACILITY");

                var formattedFacilityId = _defaults.FormatIdForTable("FACILITY", facilityId);
                var facility = await facilityRepo.GetByIdAsync(formattedFacilityId);

                if (facility == null)
                    throw new InvalidOperationException($"Facility {facilityId} not found");

                // Validate facility belongs to field using reflection
                var fieldIdProp = facilityEntityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null)
                {
                    var facilityFieldId = fieldIdProp.GetValue(facility)?.ToString();
                    var formattedFieldId = _defaults.FormatIdForTable("FACILITY", fieldId);
                    if (facilityFieldId != formattedFieldId)
                        throw new InvalidOperationException($"Facility {facilityId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_DECOMMISSIONING");
                if (metadata == null)
                    throw new InvalidOperationException("FACILITY_DECOMMISSIONING table metadata not found");

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    throw new InvalidOperationException($"Entity type not found for FACILITY_DECOMMISSIONING: {metadata.EntityTypeName}");

                // Convert DTO to PPDM model
                var decommissionEntity = _mappingService.ConvertDTOToPPDMModelRuntime(decommissionData, typeof(FacilityDecommissioningRequest), entityType);
                
                // Set FACILITY_ID and FIELD_ID automatically using reflection
                var facilityIdProp = entityType.GetProperty("FACILITY_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (facilityIdProp != null && facilityIdProp.CanWrite)
                {
                    facilityIdProp.SetValue(decommissionEntity, formattedFacilityId);
                }
                var fieldIdProp2 = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp2 != null && fieldIdProp2.CanWrite)
                {
                    fieldIdProp2.SetValue(decommissionEntity, _defaults.FormatIdForTable("FACILITY_DECOMMISSIONING", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY_DECOMMISSIONING");

                var result = await repo.InsertAsync(decommissionEntity, userId);
                
                // Convert PPDM model back to DTO
                return (FacilityDecommissioningResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(FacilityDecommissioningResponse), entityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error decommissioning facility {facilityId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningForFieldAsync(string fieldId, string decommissioningId)
        {
            try
            {
                var decommissionedFacilities = await GetDecommissionedFacilitiesForFieldAsync(fieldId, new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FACILITY_DECOMMISSIONING_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY_DECOMMISSIONING", decommissioningId),
                        Operator = "="
                    }
                });

                return decommissionedFacilities.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting facility decommissioning {decommissioningId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<EnvironmentalRestorationResponse>> GetEnvironmentalRestorationsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ENVIRONMENTAL_RESTORATION");
                if (metadata == null)
                {
                    _logger?.LogWarning("ENVIRONMENTAL_RESTORATION table metadata not found");
                    return new List<EnvironmentalRestorationResponse>();
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    _logger?.LogWarning($"Entity type not found for ENVIRONMENTAL_RESTORATION: {metadata.EntityTypeName}");
                    return new List<EnvironmentalRestorationResponse>();
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "ENVIRONMENTAL_RESTORATION");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("ENVIRONMENTAL_RESTORATION", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(EnvironmentalRestorationResponse), entityType);
                return dtoList.Cast<EnvironmentalRestorationResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting environmental restorations for field: {fieldId}");
                throw;
            }
        }

        public async Task<EnvironmentalRestorationResponse> CreateEnvironmentalRestorationForFieldAsync(string fieldId, EnvironmentalRestorationRequest restorationData, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ENVIRONMENTAL_RESTORATION");
                if (metadata == null)
                    throw new InvalidOperationException("ENVIRONMENTAL_RESTORATION table metadata not found");

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                    throw new InvalidOperationException($"Entity type not found for ENVIRONMENTAL_RESTORATION: {metadata.EntityTypeName}");

                // Convert DTO to PPDM model
                var restorationEntity = _mappingService.ConvertDTOToPPDMModelRuntime(restorationData, typeof(EnvironmentalRestorationRequest), entityType);
                
                // Set FIELD_ID automatically using reflection
                var fieldIdProp = entityType.GetProperty("FIELD_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (fieldIdProp != null && fieldIdProp.CanWrite)
                {
                    fieldIdProp.SetValue(restorationEntity, _defaults.FormatIdForTable("ENVIRONMENTAL_RESTORATION", fieldId));
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "ENVIRONMENTAL_RESTORATION");

                var result = await repo.InsertAsync(restorationEntity, userId);
                
                // Convert PPDM model back to DTO
                return (EnvironmentalRestorationResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(EnvironmentalRestorationResponse), entityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating environmental restoration for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<DecommissioningCostResponse>> GetDecommissioningCostsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("DECOMMISSIONING_COST");
                if (metadata == null)
                {
                    _logger?.LogWarning("DECOMMISSIONING_COST table metadata not found");
                    return new List<DecommissioningCostResponse>();
                }

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null)
                {
                    _logger?.LogWarning($"Entity type not found for DECOMMISSIONING_COST: {metadata.EntityTypeName}");
                    return new List<DecommissioningCostResponse>();
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "DECOMMISSIONING_COST");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("DECOMMISSIONING_COST", fieldId),
                        Operator = "="
                    }
                };

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);
                
                // Convert PPDM models to DTOs
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(DecommissioningCostResponse), entityType);
                return dtoList.Cast<DecommissioningCostResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting decommissioning costs for field: {fieldId}");
                throw;
            }
        }

        public async Task<DecommissioningCostEstimateResponse> EstimateCostsForFieldAsync(string fieldId)
        {
            try
            {
                // Cost calculation constants (configurable)
                const decimal baseWellCost = 50000m;
                const decimal depthFactorPerFoot = 100m;
                const decimal baseFacilityCost = 100000m;
                const decimal restorationCostPerWell = 25000m;
                const decimal restorationCostPerFacility = 50000m;
                const decimal permitFeePerWell = 5000m;
                const decimal permitFeePerFacility = 10000m;
                const decimal inspectionFeePerWell = 2000m;

                var abandonedWells = await GetAbandonedWellsForFieldAsync(fieldId);
                var decommissionedFacilities = await GetDecommissionedFacilitiesForFieldAsync(fieldId);

                // Calculate well abandonment costs
                decimal totalWellCost = 0;
                var wellBreakdown = new List<WellAbandonmentCostBreakdown>();

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Beep.OilandGas.PPDM39.Models.WELL), _connectionName, "WELL");

                foreach (var abandonedWell in abandonedWells)
                {
                    // Get well details to calculate depth-based costs
                    var wellFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = abandonedWell.WellId }
                    };
                    var wellData = await wellRepo.GetAsync(wellFilters);
                    var well = wellData?.FirstOrDefault();

                    decimal wellDepth = 0;
                    decimal locationFactor = 1.0m; // Default, could be based on location complexity

                    if (well != null)
                    {
                        if (well is Beep.OilandGas.PPDM39.Models.WELL w)
                        {
                            var depthProp = w.GetType().GetProperty("TOTAL_DEPTH") ?? w.GetType().GetProperty("WELL_DEPTH");
                            if (depthProp?.GetValue(w) is decimal depth) wellDepth = depth;
                        }
                        else if (well is IDictionary<string, object> wDict)
                        {
                            if (wDict.TryGetValue("TOTAL_DEPTH", out var depthVal))
                            {
                                if (depthVal is decimal d) wellDepth = d;
                                else if (decimal.TryParse(depthVal?.ToString(), out var parsed)) wellDepth = parsed;
                            }
                            else if (wDict.TryGetValue("WELL_DEPTH", out var depthVal2))
                            {
                                if (depthVal2 is decimal d2) wellDepth = d2;
                                else if (decimal.TryParse(depthVal2?.ToString(), out var parsed2)) wellDepth = parsed2;
                            }
                        }
                    }

                    // Calculate well cost: (baseCost + (depth * depthFactor)) * locationFactor
                    var wellCost = (baseWellCost + (wellDepth * depthFactorPerFoot)) * locationFactor;
                    totalWellCost += wellCost;

                    wellBreakdown.Add(new WellAbandonmentCostBreakdown
                    {
                        WellId = abandonedWell.WellId,
                        EstimatedCost = wellCost,
                        CostCurrency = "USD"
                    });
                }

                // Calculate facility decommissioning costs
                decimal totalFacilityCost = 0;
                var facilityBreakdown = new List<FacilityDecommissioningCostBreakdown>();

                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(Beep.OilandGas.PPDM39.Models.FACILITY), _connectionName, "FACILITY");

                foreach (var decommissionedFacility in decommissionedFacilities)
                {
                    // Get facility details to calculate size-based costs
                    var facilityFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = decommissionedFacility.FacilityId }
                    };
                    var facilityData = await facilityRepo.GetAsync(facilityFilters);
                    var facility = facilityData?.FirstOrDefault();

                    decimal sizeFactor = 1.0m; // Default, could be based on facility type and capacity

                    if (facility != null)
                    {
                        if (facility is Beep.OilandGas.PPDM39.Models.FACILITY f)
                        {
                            var typeProp = f.GetType().GetProperty("FACILITY_TYPE");
                            var type = typeProp?.GetValue(f)?.ToString()?.ToUpper();
                            // Adjust size factor based on facility type
                            if (type?.Contains("PLATFORM") == true) sizeFactor = 2.0m;
                            else if (type?.Contains("PROCESSING") == true) sizeFactor = 1.5m;
                            else if (type?.Contains("STORAGE") == true) sizeFactor = 1.2m;
                        }
                        else if (facility is IDictionary<string, object> fDict)
                        {
                            var type = fDict.TryGetValue("FACILITY_TYPE", out var typeVal) ? typeVal?.ToString()?.ToUpper() : null;
                            if (type?.Contains("PLATFORM") == true) sizeFactor = 2.0m;
                            else if (type?.Contains("PROCESSING") == true) sizeFactor = 1.5m;
                            else if (type?.Contains("STORAGE") == true) sizeFactor = 1.2m;
                        }
                    }

                    // Calculate facility cost: baseCost * sizeFactor
                    var facilityCost = baseFacilityCost * sizeFactor;
                    totalFacilityCost += facilityCost;

                    facilityBreakdown.Add(new FacilityDecommissioningCostBreakdown
                    {
                        FacilityId = decommissionedFacility.FacilityId,
                        EstimatedCost = facilityCost,
                        CostCurrency = "USD"
                    });
                }

                // Calculate site restoration costs
                var restorationCost = (restorationCostPerWell * abandonedWells.Count) + 
                                     (restorationCostPerFacility * decommissionedFacilities.Count);

                // Calculate regulatory costs
                var regulatoryCost = (permitFeePerWell * abandonedWells.Count) + 
                                    (permitFeePerFacility * decommissionedFacilities.Count) + 
                                    (inspectionFeePerWell * abandonedWells.Count);

                // Calculate total cost
                var totalCost = totalWellCost + totalFacilityCost + restorationCost + regulatoryCost;

                return new DecommissioningCostEstimateResponse
                {
                    FieldId = fieldId,
                    EstimateDate = DateTime.UtcNow,
                    EstimatedWellAbandonmentCost = totalWellCost,
                    EstimatedFacilityDecommissioningCost = totalFacilityCost,
                    EstimatedSiteRestorationCost = restorationCost,
                    EstimatedRegulatoryCost = regulatoryCost,
                    EstimatedTotalCost = totalCost,
                    CostCurrency = "USD",
                    EstimatedWellsToAbandon = abandonedWells.Count,
                    EstimatedFacilitiesToDecommission = decommissionedFacilities.Count,
                    WellBreakdown = wellBreakdown,
                    FacilityBreakdown = facilityBreakdown,
                    EstimationMethod = "CALCULATED",
                    Notes = $"Cost estimation based on {abandonedWells.Count} wells and {decommissionedFacilities.Count} facilities. Well costs include depth factor. Facility costs include type-based size factor."
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error estimating costs for field: {fieldId}");
                throw;
            }
        }

        #region Decommissioning Integration

        /// <summary>
        /// Plans decommissioning operation using Decommissioning service
        /// </summary>
        public async Task<WellAbandonmentResponse> PlanDecommissioningAsync(string fieldId, string wellId, WellAbandonmentRequest abandonmentData, string userId)
        {
            try
            {
                _logger?.LogInformation("Planning decommissioning for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Use existing AbandonWellForFieldAsync method
                return await AbandonWellForFieldAsync(fieldId, wellId, abandonmentData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning decommissioning for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Executes decommissioning operation using Decommissioning service
        /// </summary>
        public async Task<WellAbandonmentResponse> ExecuteDecommissioningAsync(string fieldId, string wellId, string abandonmentId, Dictionary<string, object> executionParameters, string userId)
        {
            try
            {
                _logger?.LogInformation("Executing decommissioning for well: {WellId}, AbandonmentId: {AbandonmentId}", wellId, abandonmentId);

                // Get abandonment record
                var abandonment = await GetWellAbandonmentForFieldAsync(fieldId, abandonmentId);
                if (abandonment == null)
                {
                    throw new InvalidOperationException($"Abandonment {abandonmentId} not found for well {wellId}");
                }

                // Update status to EXECUTING
                // In full implementation, would use WellPluggingService

                return new WellAbandonmentResponse
                {
                    AbandonmentId = abandonmentId,
                    WellId = wellId,
                    Status = "EXECUTING",
                    AbandonmentDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing decommissioning for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Verifies decommissioning operation using Decommissioning service
        /// </summary>
        public async Task<WellAbandonmentResponse> VerifyDecommissioningAsync(string fieldId, string wellId, string abandonmentId, string verifiedBy, bool passed, string userId)
        {
            try
            {
                _logger?.LogInformation("Verifying decommissioning for well: {WellId}, AbandonmentId: {AbandonmentId}, Passed: {Passed}", 
                    wellId, abandonmentId, passed);

                // Get abandonment record
                var abandonment = await GetWellAbandonmentForFieldAsync(fieldId, abandonmentId);
                if (abandonment == null)
                {
                    throw new InvalidOperationException($"Abandonment {abandonmentId} not found for well {wellId}");
                }

                // Update verification status
                // In full implementation, would use WellPluggingService.VerifyWellPluggingAsync

                return new WellAbandonmentResponse
                {
                    AbandonmentId = abandonmentId,
                    WellId = wellId,
                    Status = passed ? "VERIFIED" : "FAILED",
                    VerificationDate = DateTime.UtcNow,
                    VerifiedBy = verifiedBy
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying decommissioning for well: {WellId}", wellId);
                throw;
            }
        }

        #endregion
    }
}
