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
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM.Models;
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
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ABANDONMENT), _connectionName, "WELL_ABANDONMENT");

                // WELL_ABANDONMENT is linked to WELL, which is linked to FIELD
                // Get wells for field first, then get abandonments for those wells
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL", null);

                var wells = await wellRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "ASSIGNED_FIELD",
                        FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                        Operator = "="
                    }
                });

                var wellIds = wells.OfType<WELL>()
                    .Select(w => w.UWI)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

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
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(WellAbandonmentResponse), typeof(WELL_ABANDONMENT));
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
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), _connectionName, "WELL", null);

                var formattedWellId = _defaults.FormatIdForTable("WELL", wellId);
                var well = await wellRepo.GetByIdAsync(formattedWellId);

                if (well == null)
                    throw new InvalidOperationException($"Well {wellId} not found");

                // Validate well belongs to field using strongly-typed access
                var wellObj = well as WELL;
                if (wellObj != null)
                {
                    var formattedFieldId = _defaults.FormatIdForTable("WELL", fieldId);
                    if (!string.Equals(wellObj.ASSIGNED_FIELD, formattedFieldId, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Well {wellId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT");
                if (metadata == null)
                    throw new InvalidOperationException("WELL_ABANDONMENT table metadata not found");

                // Convert DTO to PPDM model
                var abandonmentEntity = _mappingService.ConvertDTOToPPDMModelRuntime(abandonmentData, typeof(WellAbandonmentRequest), typeof(WELL_ABANDONMENT));
                
                // Set UWI and FIELD_ID using typed assignment
                if (abandonmentEntity is WELL_ABANDONMENT typedAbandonment)
                {
                    typedAbandonment.UWI = formattedWellId;
                    typedAbandonment.FIELD_ID = _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ABANDONMENT), _connectionName, "WELL_ABANDONMENT");

                var result = await repo.InsertAsync(abandonmentEntity, userId);
                
                // Convert PPDM model back to DTO
                return (WellAbandonmentResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(WellAbandonmentResponse), typeof(WELL_ABANDONMENT));
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
                var entityType = metadata == null ? null : typeof(FACILITY_DECOMMISSIONING);
                if (metadata == null)
                {
                    _logger?.LogWarning("FACILITY_DECOMMISSIONING table/entity not found — falling back to FACILITY_STATUS decommission events");
                    return await GetDecommissionedFacilitiesFromStatusAsync(fieldId, additionalFilters);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY_DECOMMISSIONING), _connectionName, "FACILITY_DECOMMISSIONING");

                // FACILITY_DECOMMISSIONING is linked to FACILITY, which is linked to FIELD
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY", null);

                var facilities = await facilityRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PRIMARY_FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    }
                });

                var facilityIds = facilities.OfType<FACILITY>()
                    .Select(f => f.FACILITY_ID)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToList();

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
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(FacilityDecommissioningResponse), typeof(FACILITY_DECOMMISSIONING));
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
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), _connectionName, "FACILITY", null);

                var formattedFacilityId = _defaults.FormatIdForTable("FACILITY", facilityId);
                var facility = await facilityRepo.GetByIdAsync(formattedFacilityId);

                if (facility == null)
                    throw new InvalidOperationException($"Facility {facilityId} not found");

                // Validate facility belongs to field using strongly-typed access
                var facilityObj = facility as FACILITY;
                if (facilityObj != null)
                {
                    var formattedFieldId = _defaults.FormatIdForTable("FACILITY", fieldId);
                    if (!string.Equals(facilityObj.PRIMARY_FIELD_ID, formattedFieldId, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException($"Facility {facilityId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_DECOMMISSIONING");
                if (metadata == null)
                    throw new InvalidOperationException("FACILITY_DECOMMISSIONING table metadata not found");

                // Convert DTO to PPDM model
                var decommissionEntity = _mappingService.ConvertDTOToPPDMModelRuntime(decommissionData, typeof(FacilityDecommissioningRequest), typeof(FACILITY_DECOMMISSIONING));
                
                // Set FACILITY_ID and FIELD_ID using typed assignment
                if (decommissionEntity is FACILITY_DECOMMISSIONING typedDecommission)
                {
                    typedDecommission.FACILITY_ID = formattedFacilityId;
                    typedDecommission.FIELD_ID = _defaults.FormatIdForTable("FACILITY_DECOMMISSIONING", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY_DECOMMISSIONING), _connectionName, "FACILITY_DECOMMISSIONING");

                var result = await repo.InsertAsync(decommissionEntity, userId);
                
                // Convert PPDM model back to DTO
                return (FacilityDecommissioningResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(FacilityDecommissioningResponse), typeof(FACILITY_DECOMMISSIONING));
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

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(ENVIRONMENTAL_RESTORATION), _connectionName, "ENVIRONMENTAL_RESTORATION");

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
                var dtoList = _mappingService.ConvertPPDMModelListToDTOListRuntime(results, typeof(EnvironmentalRestorationResponse), typeof(ENVIRONMENTAL_RESTORATION));
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

                // Convert DTO to PPDM model
                var restorationEntity = _mappingService.ConvertDTOToPPDMModelRuntime(restorationData, typeof(EnvironmentalRestorationRequest), typeof(ENVIRONMENTAL_RESTORATION));
                
                // Set FIELD_ID using typed assignment
                if (restorationEntity is ENVIRONMENTAL_RESTORATION typedRestoration)
                {
                    typedRestoration.FIELD_ID = _defaults.FormatIdForTable("ENVIRONMENTAL_RESTORATION", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(ENVIRONMENTAL_RESTORATION), _connectionName, "ENVIRONMENTAL_RESTORATION");

                var result = await repo.InsertAsync(restorationEntity, userId);
                
                // Convert PPDM model back to DTO
                return (EnvironmentalRestorationResponse)_mappingService.ConvertPPDMModelToDTORuntime(result, typeof(EnvironmentalRestorationResponse), typeof(ENVIRONMENTAL_RESTORATION));
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
                        if (well is WELL w)
                        {
                            var rawDepth = w.FINAL_TD > 0 ? w.FINAL_TD : w.DRILL_TD;
                            if (rawDepth > 0)
                                wellDepth = string.Equals(w.FINAL_TD_OUOM, "M", StringComparison.OrdinalIgnoreCase)
                                    ? rawDepth * 3.28084m
                                    : rawDepth;
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
                        if (facility is FACILITY f)
                        {
                            var type = f.FACILITY_TYPE?.ToUpper();
                            // Adjust size factor based on facility type
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

                // Record execution event in WELL_ACTIVITY
                var activityMeta = await _metadata.GetTableMetadataAsync("WELL_ACTIVITY");
                if (activityMeta != null)
                {
                    var activityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY", null);
                    var activity = new WELL_ACTIVITY
                    {
                        UWI = _defaults.FormatIdForTable("WELL_ACTIVITY", wellId),
                        SOURCE = "LIFECYCLE",
                        ACTIVITY_OBS_NO = Math.Abs((decimal)Guid.NewGuid().GetHashCode()),
                        ACTIVITY_TYPE_ID = "DECOMMISSION_EXEC",
                        EVENT_DATE = DateTime.UtcNow,
                        REMARK = $"AbandonmentId: {abandonmentId}, ExecutedBy: {userId}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (activity is IPPDMEntity activityEntity)
                        _commonColumnHandler.PrepareForInsert(activityEntity, userId);
                    await activityRepo.InsertAsync(activity, userId);
                }

                return new WellAbandonmentResponse
                {
                    AbandonmentId = abandonmentId,
                    WellId = wellId,
                    Status = "EXECUTING",
                    AbandonmentEndDate = DateTime.UtcNow
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

                // Record verification event in WELL_ACTIVITY
                var activityMeta = await _metadata.GetTableMetadataAsync("WELL_ACTIVITY");
                if (activityMeta != null)
                {
                    var activityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY", null);
                    var activity = new WELL_ACTIVITY
                    {
                        UWI = _defaults.FormatIdForTable("WELL_ACTIVITY", wellId),
                        SOURCE = "LIFECYCLE",
                        ACTIVITY_OBS_NO = Math.Abs((decimal)Guid.NewGuid().GetHashCode()),
                        ACTIVITY_TYPE_ID = passed ? "DECOMMISSION_VERIFIED" : "DECOMMISSION_FAILED",
                        EVENT_DATE = DateTime.UtcNow,
                        REMARK = $"AbandonmentId: {abandonmentId}, VerifiedBy: {verifiedBy}, Passed: {passed}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (activity is IPPDMEntity activityEntity)
                        _commonColumnHandler.PrepareForInsert(activityEntity, userId);
                    await activityRepo.InsertAsync(activity, userId);
                }

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

        #region Private Fallback Helpers

        private async Task<List<WellAbandonmentResponse>> GetAbandonedWellsFromActivityAsync(string fieldId, List<AppFilter>? additionalFilters)
        {
            // Get well IDs for this field
            var wellMeta = await _metadata.GetTableMetadataAsync("WELL");
            if (wellMeta == null) return new List<WellAbandonmentResponse>();

            var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL), _connectionName, "WELL", null);
            var wellFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL", fieldId) }
            };
            var wells = await wellRepo.GetAsync(wellFilters);
            var wellList = wells?.OfType<WELL>().Where(w => !string.IsNullOrEmpty(w.UWI)).ToList()
                          ?? new List<WELL>();
            if (!wellList.Any()) return new List<WellAbandonmentResponse>();

            // Query WELL_ACTIVITY for decommission events
            var actRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL_ACTIVITY), _connectionName, "WELL_ACTIVITY", null);
            var responses = new List<WellAbandonmentResponse>();
            foreach (var well in wellList)
            {
                var actFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL_ACTIVITY", well.UWI!) },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                var acts = await actRepo.GetAsync(actFilters);
                foreach (var act in acts?.OfType<WELL_ACTIVITY>()
                    .Where(a => (a.ACTIVITY_TYPE_ID ?? string.Empty).StartsWith("DECOMMISSION_", StringComparison.OrdinalIgnoreCase))
                    ?? Enumerable.Empty<WELL_ACTIVITY>())
                {
                    responses.Add(new WellAbandonmentResponse
                    {
                        AbandonmentId = act.PPDM_GUID ?? Guid.NewGuid().ToString(),
                        WellId = well.UWI,
                        FieldId = fieldId,
                        AbandonmentType = act.ACTIVITY_TYPE_ID?.Length > 13 ? act.ACTIVITY_TYPE_ID.Substring(13) : act.ACTIVITY_TYPE_ID,
                        Status = "COMPLETED",
                        AbandonmentStartDate = act.EVENT_DATE
                    });
                }
            }
            return responses;
        }

        private async Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesFromStatusAsync(string fieldId, List<AppFilter>? additionalFilters)
        {
            // Get facility IDs for this field
            var facMeta = await _metadata.GetTableMetadataAsync("FACILITY");
            if (facMeta == null) return new List<FacilityDecommissioningResponse>();

            var facRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FACILITY), _connectionName, "FACILITY", null);
            var facFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId) }
            };
            var facilities = await facRepo.GetAsync(facFilters);
            var facilityList = facilities?.OfType<FACILITY>()
                .Where(f => !string.IsNullOrEmpty(f.FACILITY_ID)).ToList()
                ?? new List<FACILITY>();
            if (!facilityList.Any()) return new List<FacilityDecommissioningResponse>();

            // Query FACILITY_STATUS for decommission events
            var fsRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
            var responses = new List<FacilityDecommissioningResponse>();
            foreach (var facility in facilityList)
            {
                var fsFilters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FACILITY_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("FACILITY_STATUS", facility.FACILITY_ID!) },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };
                var statuses = await fsRepo.GetAsync(fsFilters);
                foreach (var fs in statuses?.OfType<FACILITY_STATUS>()
                    .Where(s => (s.STATUS_TYPE ?? string.Empty).StartsWith("DECOMMISSION_", StringComparison.OrdinalIgnoreCase))
                    ?? Enumerable.Empty<FACILITY_STATUS>())
                {
                    responses.Add(new FacilityDecommissioningResponse
                    {
                        DecommissioningId = fs.PPDM_GUID ?? Guid.NewGuid().ToString(),
                        FacilityId = facility.FACILITY_ID,
                        FieldId = fieldId,
                        DecommissioningType = fs.STATUS_TYPE?.Length > 13 ? fs.STATUS_TYPE.Substring(13) : fs.STATUS_TYPE,
                        Status = fs.STATUS ?? "COMPLETED",
                        DecommissioningStartDate = fs.START_TIME
                    });
                }
            }
            return responses;
        }

        #endregion
    }
}
