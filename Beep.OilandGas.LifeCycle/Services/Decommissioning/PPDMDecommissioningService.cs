using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
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
        private readonly ICompanyFieldSelectionService _selections;
        private readonly IPPDMRowAccessPolicy _rowAccess;
        private FieldSelectionReview? _review;
        private readonly ILogger<PPDMDecommissioningService>? _logger;

        public PPDMDecommissioningService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            ICompanyFieldSelectionService selections,
            IPPDMRowAccessPolicy rowAccess,
            ILogger<PPDMDecommissioningService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _selections = selections ?? throw new ArgumentNullException(nameof(selections));
            _rowAccess = rowAccess ?? throw new ArgumentNullException(nameof(rowAccess));
            _logger = logger;
        }

        // Captures the existing profile review for this phase request; never chooses or persists another FIELD.
        private async Task<string> RequireCurrentBindingAsync(string? fieldId = null, string? actor = null)
        {
            var current = await _selections.GetCurrentFieldSelectionAsync();
            if (current.SelectionContext is not { } review)
                throw RefusalException.Conflict("The current FIELD review is unavailable.");
            FieldSelectionReviewValidator.RequireCurrent(current, review.Company, review.ActorUserId);
            if (!current.SelectionAvailable || fieldId is not null && fieldId != current.FieldId ||
                actor is not null && actor != review.ActorUserId || _review is not null && _review != review)
                throw RefusalException.Conflict("The retirement account, company, database or FIELD differs from its reviewed request.");
            _review = review;
            return review.Database.ConnectionName;
        }
        public async Task<List<WellAbandonmentResponse>> GetAbandonedWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT") ?? throw RefusalException.Conflict("WELL_ABANDONMENT metadata is unavailable; review the canonical module setup.");
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ABANDONMENT), await RequireCurrentBindingAsync(), "WELL_ABANDONMENT", rowAccess: _rowAccess);

                // WELL_ABANDONMENT is linked to WELL, which is linked to FIELD
                // Get wells for field first, then get abandonments for those wells
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), await RequireCurrentBindingAsync(), "WELL", null, rowAccess: _rowAccess);

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
                {
                    await RequireCurrentBindingAsync(fieldId);
                    return new List<WellAbandonmentResponse>();
                }

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
                await RequireCurrentBindingAsync(fieldId);
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
            try
            {
                // Validate well belongs to field
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL), await RequireCurrentBindingAsync(), "WELL", null, rowAccess: _rowAccess);

                var formattedWellId = _defaults.FormatIdForTable("WELL", wellId);
                var well = await wellRepo.GetByIdAsync(formattedWellId);

                if (well == null)
                    throw new InvalidOperationException($"Well {wellId} not found");

                // Validate well belongs to field using strongly-typed access
                var wellObj = well as WELL;
                if (wellObj != null)
                {
                    var formattedFieldId = _defaults.FormatIdForTable("WELL", fieldId);
                    if (!string.Equals(wellObj.ASSIGNED_FIELD, formattedFieldId, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Well {wellId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT") ?? throw RefusalException.Conflict("WELL_ABANDONMENT metadata is unavailable; review the canonical module setup.");
                if (metadata == null)
                    throw RefusalException.Conflict("WELL_ABANDONMENT metadata is unavailable; review the canonical module setup.");

                // Convert DTO to PPDM model
                var abandonmentEntity = _mappingService.ConvertDTOToPPDMModelRuntime(abandonmentData, typeof(WellAbandonmentRequest), typeof(WELL_ABANDONMENT));
                
                // Set UWI and FIELD_ID using typed assignment
                if (abandonmentEntity is WELL_ABANDONMENT typedAbandonment)
                {
                    typedAbandonment.UWI = formattedWellId;
                    typedAbandonment.FIELD_ID = _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(WELL_ABANDONMENT), await RequireCurrentBindingAsync(), "WELL_ABANDONMENT", rowAccess: _rowAccess);

                var result = await repo.InsertAsync(abandonmentEntity, userId);

                await UpsertWellStatusAsync(
                    wellId: wellId,
                    statusType: "DECOMMISSIONING",
                    statusCode: "P_AND_A_PLANNED",
                    statusId: "P_AND_A_PLANNED",
                    userId: userId,
                    remark: "Well abandonment planned");
                
                // Convert PPDM model back to DTO
                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
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

                await RequireCurrentBindingAsync(fieldId);

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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_DECOMMISSIONING");
                var entityType = metadata == null ? null : typeof(FACILITY_DECOMMISSIONING);
                if (metadata == null) throw RefusalException.Conflict("FACILITY_DECOMMISSIONING metadata is unavailable; review the canonical module setup.");

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY_DECOMMISSIONING), await RequireCurrentBindingAsync(), "FACILITY_DECOMMISSIONING", rowAccess: _rowAccess);

                // FACILITY_DECOMMISSIONING is linked to FACILITY, which is linked to FIELD
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), await RequireCurrentBindingAsync(), "FACILITY", null, rowAccess: _rowAccess);

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
                {
                    await RequireCurrentBindingAsync(fieldId);
                    return new List<FacilityDecommissioningResponse>();
                }

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
                await RequireCurrentBindingAsync(fieldId);
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
            try
            {
                // Validate facility belongs to field
                var facilityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY), await RequireCurrentBindingAsync(), "FACILITY", null, rowAccess: _rowAccess);

                var formattedFacilityId = _defaults.FormatIdForTable("FACILITY", facilityId);
                var facility = await facilityRepo.GetByIdAsync(formattedFacilityId);

                if (facility == null)
                    throw new InvalidOperationException($"Facility {facilityId} not found");

                // Validate facility belongs to field using strongly-typed access
                var facilityObj = facility as FACILITY;
                if (facilityObj != null)
                {
                    var formattedFieldId = _defaults.FormatIdForTable("FACILITY", fieldId);
                    if (!string.Equals(facilityObj.PRIMARY_FIELD_ID, formattedFieldId, StringComparison.Ordinal))
                        throw new InvalidOperationException($"Facility {facilityId} does not belong to field {fieldId}");
                }

                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_DECOMMISSIONING");
                if (metadata == null)
                    throw RefusalException.Conflict("FACILITY_DECOMMISSIONING metadata is unavailable; review the canonical module setup.");

                // Convert DTO to PPDM model
                var decommissionEntity = _mappingService.ConvertDTOToPPDMModelRuntime(decommissionData, typeof(FacilityDecommissioningRequest), typeof(FACILITY_DECOMMISSIONING));
                
                // Set FACILITY_ID and FIELD_ID using typed assignment
                if (decommissionEntity is FACILITY_DECOMMISSIONING typedDecommission)
                {
                    typedDecommission.FACILITY_ID = formattedFacilityId;
                    typedDecommission.FIELD_ID = _defaults.FormatIdForTable("FACILITY_DECOMMISSIONING", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(FACILITY_DECOMMISSIONING), await RequireCurrentBindingAsync(), "FACILITY_DECOMMISSIONING", rowAccess: _rowAccess);

                var result = await repo.InsertAsync(decommissionEntity, userId);
                
                // Convert PPDM model back to DTO
                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
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

                await RequireCurrentBindingAsync(fieldId);

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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ENVIRONMENTAL_RESTORATION");
                if (metadata == null) throw RefusalException.Conflict("ENVIRONMENTAL_RESTORATION metadata is unavailable; review the canonical module setup.");

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(ENVIRONMENTAL_RESTORATION), await RequireCurrentBindingAsync(), "ENVIRONMENTAL_RESTORATION", rowAccess: _rowAccess);

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
                await RequireCurrentBindingAsync(fieldId);
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("ENVIRONMENTAL_RESTORATION");
                if (metadata == null)
                    throw RefusalException.Conflict("ENVIRONMENTAL_RESTORATION metadata is unavailable; review the canonical module setup.");

                // Convert DTO to PPDM model
                var restorationEntity = _mappingService.ConvertDTOToPPDMModelRuntime(restorationData, typeof(EnvironmentalRestorationRequest), typeof(ENVIRONMENTAL_RESTORATION));
                
                // Set FIELD_ID using typed assignment
                if (restorationEntity is ENVIRONMENTAL_RESTORATION typedRestoration)
                {
                    typedRestoration.FIELD_ID = _defaults.FormatIdForTable("ENVIRONMENTAL_RESTORATION", fieldId);
                }

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(ENVIRONMENTAL_RESTORATION), await RequireCurrentBindingAsync(), "ENVIRONMENTAL_RESTORATION", rowAccess: _rowAccess);

                var result = await repo.InsertAsync(restorationEntity, userId);
                
                // Convert PPDM model back to DTO
                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("DECOMMISSIONING_COST");
                if (metadata == null) throw RefusalException.Conflict("DECOMMISSIONING_COST metadata is unavailable; review the canonical module setup.");

                var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}");
                if (entityType == null) throw RefusalException.Conflict("The canonical retirement cost entity is unavailable.");

                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, await RequireCurrentBindingAsync(), "DECOMMISSIONING_COST", rowAccess: _rowAccess);

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
                await RequireCurrentBindingAsync(fieldId);
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId);
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
                    typeof(Beep.OilandGas.PPDM39.Models.WELL), await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);

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
                    typeof(Beep.OilandGas.PPDM39.Models.FACILITY), await RequireCurrentBindingAsync(), "FACILITY", rowAccess: _rowAccess);

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

                await RequireCurrentBindingAsync(fieldId);

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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
            try
            {
                _logger?.LogInformation("Planning decommissioning for well: {WellId} in field: {FieldId}", wellId, fieldId);

                // Use existing AbandonWellForFieldAsync method
                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
                        typeof(WELL_ACTIVITY), await RequireCurrentBindingAsync(), "WELL_ACTIVITY", null, rowAccess: _rowAccess);
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

                await UpsertWellStatusAsync(
                    wellId: wellId,
                    statusType: "DECOMMISSIONING",
                    statusCode: "P_AND_A_EXECUTING",
                    statusId: "P_AND_A_EXECUTING",
                    userId: userId,
                    remark: $"Abandonment {abandonmentId} execution started");

                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));

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
            if (!LeaseSummaryValidator.Identifier(fieldId)) throw RefusalException.Invalid("An exact FIELD identity is required.");
            await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));
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
                        typeof(WELL_ACTIVITY), await RequireCurrentBindingAsync(), "WELL_ACTIVITY", null, rowAccess: _rowAccess);
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

                await UpsertWellStatusAsync(
                    wellId: wellId,
                    statusType: "DECOMMISSIONING",
                    statusCode: passed ? "P_AND_A_VERIFIED" : "P_AND_A_FAILED",
                    statusId: passed ? "P_AND_A_VERIFIED" : "P_AND_A_FAILED",
                    userId: userId,
                    remark: $"Abandonment {abandonmentId} verification by {verifiedBy}");

                await RequireCurrentBindingAsync(fieldId, userId ?? throw RefusalException.Invalid("An exact application actor is required."));

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

        #region Private Status Helpers

        private async Task UpsertWellStatusAsync(
            string wellId,
            string statusType,
            string statusCode,
            string statusId,
            string userId,
            string remark)
        {
            var formattedWellId = _defaults.FormatIdForTable("WELL_STATUS", wellId);
            await ValidateWellStatusReferencesAsync(statusType, statusCode);

            var wsRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(WELL_STATUS), await RequireCurrentBindingAsync(), "WELL_STATUS", null, rowAccess: _rowAccess);

            var now = DateTime.UtcNow;
            var statusRow = new WELL_STATUS
            {
                UWI = formattedWellId,
                STATUS_TYPE = statusType,
                STATUS = statusCode,
                STATUS_ID = statusId,
                EFFECTIVE_DATE = now,
                STATUS_DATE = now,
                START_TIME = now,
                ACTIVE_IND = "Y",
                SOURCE = "LIFECYCLE",
                REMARK = remark,
                PPDM_GUID = Guid.NewGuid().ToString()
            };
            if (statusRow is IPPDMEntity e)
                _commonColumnHandler.PrepareForInsert(e, userId);

            await wsRepo.InsertAsync(statusRow, userId);
        }

        private async Task ValidateWellStatusReferencesAsync(string statusType, string statusCode)
        {
            var typeRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(R_WELL_STATUS_TYPE), await RequireCurrentBindingAsync(), "R_WELL_STATUS_TYPE", null, rowAccess: _rowAccess);
            var statusTypeRows = await typeRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = statusType },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });
            if (!statusTypeRows.OfType<R_WELL_STATUS_TYPE>().Any())
                throw new InvalidOperationException($"Invalid WELL_STATUS.STATUS_TYPE '{statusType}'.");

            var statusRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(R_WELL_STATUS), await RequireCurrentBindingAsync(), "R_WELL_STATUS", null, rowAccess: _rowAccess);
            var statusRows = await statusRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_TYPE", Operator = "=", FilterValue = statusType },
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = statusCode },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            });
            if (!statusRows.OfType<R_WELL_STATUS>().Any())
                throw new InvalidOperationException($"Invalid WELL_STATUS code '{statusCode}' for type '{statusType}'.");
        }

        #endregion
    }
}
