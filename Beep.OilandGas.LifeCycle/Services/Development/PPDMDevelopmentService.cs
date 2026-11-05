using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.GasLift;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.PipelineAnalysis;
using Beep.OilandGas.PipelineAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.CompressorAnalysis.Data;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Drilling;

namespace Beep.OilandGas.LifeCycle.Services.Development
{
    /// <summary>
    /// Service for Development phase data management, field-scoped
    /// </summary>
    public class PPDMDevelopmentService : IFieldDevelopmentService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly ICompanyFieldSelectionService _selections;
        private readonly IPPDMRowAccessPolicy _rowAccess;
        private FieldSelectionReview? _review;
        private readonly ILogger<PPDMDevelopmentService>? _logger;
        public PPDMDevelopmentService(IDMEEditor editor, ICommonColumnHandler commonColumnHandler, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata, PPDMMappingService mappingService, ICompanyFieldSelectionService selections, IPPDMRowAccessPolicy rowAccess, ILogger<PPDMDevelopmentService>? logger = null)
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

        // The existing profile owns authority; this is only a fence for one phase request.
        private async Task<string> RequireCurrentBindingAsync(string? fieldId = null, string? actor = null, bool requireField = false)
        {
            var current = await _selections.GetCurrentFieldSelectionAsync();
            if (current.SelectionContext is not { } review)
                throw RefusalException.Conflict("The company FIELD review is unavailable.");
            FieldSelectionReviewValidator.RequireCurrent(current, review.Company, review.ActorUserId);
            if (requireField && !current.SelectionAvailable || fieldId is not null && fieldId != current.FieldId || actor is not null && actor != review.ActorUserId || _review is not null && _review != review)
                throw RefusalException.Conflict("The development account, company, database or FIELD differs from its reviewed request.");
            _review = review;
            return review.Database.ConnectionName;
        }

        private async Task<T> ConfirmAsync<T>(T result)
        {
            await RequireCurrentBindingAsync();
            return result;
        }

        private async Task EnsureWellBelongsToFieldAsync(string fieldId, string wellId)
        {
            if (string.IsNullOrWhiteSpace(wellId)) throw RefusalException.Invalid("wellId is required.");
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL), await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
            var rows = (await repo.GetAsync(new List<AppFilter> { new() { FieldName = "UWI", Operator = "=", FilterValue = wellId }, new() { FieldName = "ASSIGNED_FIELD", Operator = "=", FilterValue = fieldId }, new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } })).Cast<WELL>().ToList();
            if (rows.Count != 1 || rows[0].UWI != wellId || rows[0].ASSIGNED_FIELD != fieldId || rows[0].ACTIVE_IND != "Y")
                throw RefusalException.NotFound("The active WELL is not uniquely recorded in the reviewed FIELD.");
        }

        private async Task EnsureFacilityBelongsToFieldAsync(string fieldId, string facilityId)
        {
            if (string.IsNullOrWhiteSpace(facilityId)) throw RefusalException.Invalid("facilityId is required.");
            var rows = await GetFacilitiesForFieldAsync(fieldId, new List<AppFilter> { new() { FieldName = "FACILITY_ID", Operator = "=", FilterValue = facilityId } });
            if (rows.Count != 1 || rows[0].FACILITY_ID != facilityId || rows[0].PRIMARY_FIELD_ID != fieldId || rows[0].ACTIVE_IND != "Y")
                throw RefusalException.Conflict("A unique active FACILITY identity in the reviewed FIELD is required.");
        }

        private async Task EnsurePipelineBelongsToFieldAsync(string fieldId, string pipelineId)
        {
            if (string.IsNullOrWhiteSpace(pipelineId)) throw RefusalException.Invalid("pipelineId is required.");
            var rows = await GetPipelinesForFieldAsync(fieldId, new List<AppFilter> { new() { FieldName = "PIPELINE_ID", Operator = "=", FilterValue = pipelineId } });
            if (rows.Count != 1 || rows[0].PIPELINE_ID != pipelineId || rows[0].FIELD_ID != fieldId || rows[0].ACTIVE_IND != "Y")
                throw RefusalException.NotFound("The active PIPELINE is not uniquely recorded in the reviewed FIELD.");
        }

        public async Task<List<POOL>> GetPoolsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    throw RefusalException.Conflict("POOL metadata is unavailable for the reviewed development request.");
                }

                var entityType = typeof(POOL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "POOL", rowAccess: _rowAccess);
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("POOL", fieldId),
                        Operator = "="
                    }
                };
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);
                var results = await repo.GetAsync(filters);
                // Return PPDM models directly
                if (results.Cast<POOL>().Any(row => row.FIELD_ID != fieldId))
                    throw RefusalException.Conflict("POOL rows differ from the reviewed FIELD.");
                return await ConfirmAsync(results.Cast<POOL>().ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pools for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL> CreatePoolForFieldAsync(string fieldId, PoolRequest poolData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("POOL table metadata not found");
                }

                var entityType = typeof(POOL);
                // Convert DTO to PPDM model
                var poolEntity = _mappingService.ConvertDTOToPPDMModel<POOL, PoolRequest>(poolData);
                // Set FIELD_ID directly
                poolEntity.FIELD_ID = _defaults.FormatIdForTable("POOL", fieldId);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "POOL", rowAccess: _rowAccess);
                var result = await repo.InsertAsync(poolEntity, userId);
                // Return PPDM model directly
                return await ConfirmAsync((POOL)result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating pool for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL> UpdatePoolForFieldAsync(string fieldId, string poolId, PoolRequest poolData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                var existingPool = await GetPoolForFieldAsync(fieldId, poolId);
                if (existingPool == null)
                {
                    throw RefusalException.NotFound($"Pool {poolId} was not found in field {fieldId}.");
                }

                var metadata = await _metadata.GetTableMetadataAsync("POOL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("POOL table metadata not found");
                }

                var entityType = typeof(POOL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "POOL", rowAccess: _rowAccess);
                var formattedId = _defaults.FormatIdForTable("POOL", poolId);
                ApplyPoolRequest(existingPool, poolData);
                existingPool.POOL_ID = formattedId;
                existingPool.FIELD_ID = _defaults.FormatIdForTable("POOL", fieldId);
                var result = await repo.UpdateAsync(existingPool, userId);
                // Return PPDM model directly
                return await ConfirmAsync((POOL)result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating pool {poolId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<POOL?> GetPoolForFieldAsync(string fieldId, string poolId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var pools = await GetPoolsForFieldAsync(fieldId, new List<AppFilter> { new AppFilter { FieldName = "POOL_ID", FilterValue = _defaults.FormatIdForTable("POOL", poolId), Operator = "=" } });
                if (pools.Count > 1 || pools.Any(row => row.POOL_ID != poolId || row.FIELD_ID != fieldId))
                    throw RefusalException.Conflict("POOL evidence differs from the requested identity.");
                return await ConfirmAsync(pools.SingleOrDefault());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pool {poolId} for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetDevelopmentWellsForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    throw RefusalException.Conflict("WELL metadata is unavailable for the reviewed development request.");
                }

                var entityType = typeof(WELL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "ASSIGNED_FIELD",
                        FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "CURRENT_CLASS",
                        FilterValue = "DEVELOPMENT",
                        Operator = "="
                    }
                };
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);
                var results = await repo.GetAsync(filters);
                // Return PPDM models directly
                if (results.Cast<WELL>().Any(row => row.ASSIGNED_FIELD != fieldId))
                    throw RefusalException.Conflict("WELL rows differ from the reviewed FIELD.");
                return await ConfirmAsync(results.Cast<WELL>().ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting development wells for field: {fieldId}");
                throw;
            }
        }

        public async Task<bool> AssignRigToWellAsync(string fieldId, string uwi, string rigName, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureWellBelongsToFieldAsync(fieldId, uwi);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    throw RefusalException.Conflict("WELL metadata is unavailable for the reviewed development request.");
                }

                var entityType = typeof(WELL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
                var well = await repo.GetByIdAsync(_defaults.FormatIdForTable("WELL", uwi));
                if (well == null)
                {
                    return await ConfirmAsync(false);
                }

                var wellEntity = (WELL)well;
                if (wellEntity.UWI != uwi || wellEntity.ASSIGNED_FIELD != fieldId || wellEntity.ACTIVE_IND != "Y")
                    throw RefusalException.Conflict("The WELL changed before rig assignment.");
                wellEntity.STATUS_TYPE = $"RIG:{rigName}";
                wellEntity.ROW_CHANGED_BY = userId;
                wellEntity.ROW_CHANGED_DATE = DateTime.UtcNow;
                await repo.UpdateAsync(wellEntity, userId);
                return await ConfirmAsync(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error assigning rig to well {uwi}");
                throw;
            }
        }

        public async Task<WELL> CreateDevelopmentWellForFieldAsync(string fieldId, DevelopmentWellRequest wellData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("WELL table metadata not found");
                }

                // Convert DTO to PPDM model
                var wellEntity = _mappingService.ConvertDTOToPPDMModel<WELL, DevelopmentWellRequest>(wellData);
                // WELL field link is ASSIGNED_FIELD (not FIELD_ID); WELL has no WELL_TYPE — use CURRENT_CLASS
                wellEntity.ASSIGNED_FIELD = _defaults.FormatIdForTable("WELL", fieldId);
                wellEntity.CURRENT_CLASS = "DEVELOPMENT";
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL), await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
                var result = await repo.InsertAsync(wellEntity, userId);
                // Return PPDM model directly
                return await ConfirmAsync((WELL)result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating development well for field: {fieldId}");
                throw;
            }
        }

        public async Task<List<WELL>> GetWellboresForWellAsync(string fieldId, string wellId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            await EnsureWellBelongsToFieldAsync(fieldId, wellId);
            try
            {
                // First validate that the well belongs to the field
                var wells = await GetDevelopmentWellsForFieldAsync(fieldId, new List<AppFilter> { new AppFilter { FieldName = "UWI", FilterValue = _defaults.FormatIdForTable("WELL", wellId), Operator = "=" } });
                if (!wells.Any())
                {
                    throw RefusalException.NotFound($"Well {wellId} was not found in field {fieldId}.");
                }

                // Wellbores are WELL table records with specific well_level_type, linked via WELL_XREF
                // Get the parent well's UWI to query WELL_XREF
                var parentWell = wells.OfType<WELL>().FirstOrDefault();
                if (parentWell == null)
                {
                    _logger?.LogWarning("Parent well {WellId} could not be cast to WELL", wellId);
                    return await ConfirmAsync(new List<WELL>());
                }

                var parentUwi = parentWell.UWI;
                if (string.IsNullOrWhiteSpace(parentUwi))
                {
                    _logger?.LogWarning("Parent well {WellId} does not have a UWI", wellId);
                    return await ConfirmAsync(new List<WELL>());
                }

                // Query WELL_XREF to find wellbores linked to this well by UWI
                var wellXrefMetadata = await _metadata.GetTableMetadataAsync("WELL_XREF");
                if (wellXrefMetadata == null)
                {
                    throw RefusalException.Conflict("WELL_XREF metadata is unavailable for the reviewed development request.");
                }

                var wellXrefType = typeof(WELL_XREF);
                var wellXrefRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, wellXrefType, await RequireCurrentBindingAsync(), "WELL_XREF", rowAccess: _rowAccess);
                var wellboreXrefType = _defaults.GetWellboreXrefType();
                // Query WELL_XREF by parent well's UWI and XREF_TYPE = wellbore
                var xrefFilters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        FilterValue = parentUwi,
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "XREF_TYPE",
                        FilterValue = wellboreXrefType,
                        Operator = "="
                    }
                };
                var xrefResults = await wellXrefRepo.GetAsync(xrefFilters);
                var wellXrefs = xrefResults.Cast<WELL_XREF>().ToList();
                if (!wellXrefs.Any())
                {
                    return await ConfirmAsync(new List<WELL>());
                }

                // Get the wellbore UWIs from the xref records
                // The UWI in WELL_XREF identifies the wellbore well (same UWI structure)
                var wellboreUwis = new HashSet<string>();
                foreach (var xref in wellXrefs)
                {
                    var uwi = xref.UWI;
                    if (!string.IsNullOrWhiteSpace(uwi) && uwi != parentUwi)
                        wellboreUwis.Add(uwi);
                }

                if (!wellboreUwis.Any())
                    return await ConfirmAsync(new List<WELL>());
                // Now query WELL table for these wellbore UWIs
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                {
                    throw RefusalException.Conflict("WELL metadata is unavailable for the reviewed development request.");
                }

                var wellEntityType = typeof(WELL);
                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, wellEntityType, await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
                // Filter WELL table by wellbore UWIs
                var wellFilters = new List<AppFilter>();
                // Query each UWI (repository should handle this efficiently)
                foreach (var uwi in wellboreUwis)
                {
                    wellFilters.Add(new AppFilter { FieldName = "UWI", FilterValue = uwi, Operator = "=" });
                }

                // Note: Wellbores are WELL records with specific well_level_type
                // The well_level_type field on WELL table identifies them as wellbores
                // This is validated via WELL_XREF filtering above
                if (additionalFilters != null)
                    wellFilters.AddRange(additionalFilters);
                var wellResults = await wellRepo.GetAsync(wellFilters);
                // Return PPDM models directly
                return await ConfirmAsync(wellResults.Cast<WELL>().ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting wellbores for well {wellId} in field: {fieldId}");
                throw;
            }
        }

        public async Task<List<FACILITY>> GetFacilitiesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    throw RefusalException.Conflict("FACILITY metadata is unavailable for the reviewed development request.");
                }

                var entityType = typeof(FACILITY);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "FACILITY", rowAccess: _rowAccess);
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PRIMARY_FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    }
                };
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);
                var results = await repo.GetAsync(filters);
                // Return PPDM models directly
                if (results.Cast<FACILITY>().Any(row => row.PRIMARY_FIELD_ID != fieldId))
                    throw RefusalException.Conflict("FACILITY rows differ from the reviewed FIELD.");
                return await ConfirmAsync(results.Cast<FACILITY>().ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting facilities for field: {fieldId}");
                throw;
            }
        }

        public async Task<FacilityResponse> CreateFacilityForFieldAsync(string fieldId, FacilityRequest facilityData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FACILITY table metadata not found");
                }

                // FACILITY is a known compile-time type; use it directly
                var facilityEntity = _mappingService.ConvertDTOToPPDMModel<FACILITY, FacilityRequest>(facilityData);
                // FACILITY field link is PRIMARY_FIELD_ID (not FIELD_ID)
                facilityEntity.PRIMARY_FIELD_ID = _defaults.FormatIdForTable("FACILITY", fieldId);
                ApplyFacilityRequest(facilityEntity, facilityData);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY), await RequireCurrentBindingAsync(), "FACILITY", rowAccess: _rowAccess);
                var result = await repo.InsertAsync(facilityEntity, userId);
                return await ConfirmAsync(_mappingService.ConvertPPDMModelToDTO<FacilityResponse, FACILITY>((FACILITY)result));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating facility for field: {fieldId}");
                throw;
            }
        }

        public async Task<FacilityResponse> UpdateFacilityForFieldAsync(string fieldId, string facilityId, FacilityRequest facilityData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureFacilityBelongsToFieldAsync(fieldId, facilityId);
            try
            {
                var existingFacility = await GetFacilityForFieldAsync(fieldId, facilityId);
                if (existingFacility == null)
                {
                    throw RefusalException.NotFound($"Facility {facilityId} was not found in field {fieldId}.");
                }

                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FACILITY table metadata not found");
                }

                ApplyFacilityRequest(existingFacility, facilityData);
                existingFacility.FACILITY_ID = _defaults.FormatIdForTable("FACILITY", facilityId);
                existingFacility.PRIMARY_FIELD_ID = _defaults.FormatIdForTable("FACILITY", fieldId);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY), await RequireCurrentBindingAsync(), "FACILITY", rowAccess: _rowAccess);
                var result = await repo.UpdateAsync(existingFacility, userId);
                return await ConfirmAsync(_mappingService.ConvertPPDMModelToDTO<FacilityResponse, FACILITY>((FACILITY)result));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error updating facility {facilityId} for field: {fieldId}");
                throw;
            }
        }

        private async Task<FACILITY?> GetFacilityForFieldAsync(string fieldId, string facilityId)
        {
            var facilities = await GetFacilitiesForFieldAsync(fieldId, new List<AppFilter> { new AppFilter { FieldName = "FACILITY_ID", FilterValue = _defaults.FormatIdForTable("FACILITY", facilityId), Operator = "=" } });
            if (facilities.Count > 1 || facilities.Any(row => row.FACILITY_ID != facilityId || row.PRIMARY_FIELD_ID != fieldId))
                throw RefusalException.Conflict("A unique requested FACILITY identity is required.");
            return facilities.SingleOrDefault();
        }

        private static void ApplyPoolRequest(POOL poolEntity, PoolRequest poolData)
        {
            if (!string.IsNullOrWhiteSpace(poolData.PoolName))
            {
                poolEntity.POOL_NAME = poolData.PoolName.Trim();
            }

            poolEntity.POOL_TYPE = NormalizeString(poolData.PoolType);
            poolEntity.POOL_STATUS = NormalizeString(poolData.Status);
            poolEntity.REMARK = NormalizeString(poolData.Description);
            if (!string.IsNullOrWhiteSpace(poolData.StratUnitId))
            {
                poolEntity.STRAT_UNIT_ID = poolData.StratUnitId.Trim();
            }
        }

        private static void ApplyFacilityRequest(FACILITY facilityEntity, FacilityRequest facilityData)
        {
            if (!string.IsNullOrWhiteSpace(facilityData.FacilityName))
            {
                var facilityName = facilityData.FacilityName.Trim();
                facilityEntity.FACILITY_LONG_NAME = facilityName;
                facilityEntity.FACILITY_SHORT_NAME = facilityName;
            }

            facilityEntity.FACILITY_TYPE = NormalizeString(facilityData.FacilityType);
            facilityEntity.DESCRIPTION = NormalizeString(facilityData.Description);
            facilityEntity.REMARK = NormalizeString(facilityData.Status);
            facilityEntity.ACTIVE_IND = MapFacilityStatusToActiveIndicator(facilityData.Status);
        }

        private static string NormalizeString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string MapFacilityStatusToActiveIndicator(string? status)
        {
            return string.IsNullOrWhiteSpace(status) || status.Equals("PLANNED", StringComparison.OrdinalIgnoreCase) || status.Equals("DESIGN", StringComparison.OrdinalIgnoreCase) || status.Equals("CONSTRUCTION", StringComparison.OrdinalIgnoreCase) || status.Equals("INACTIVE", StringComparison.OrdinalIgnoreCase) || status.Equals("DECOMMISSIONED", StringComparison.OrdinalIgnoreCase) || status.Equals("ABANDONED", StringComparison.OrdinalIgnoreCase) ? "N" : "Y";
        }

        public async Task<List<PIPELINE>> GetPipelinesForFieldAsync(string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null)
                {
                    throw RefusalException.Conflict("PIPELINE metadata is unavailable for the reviewed development request.");
                }

                var entityType = typeof(PIPELINE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, entityType, await RequireCurrentBindingAsync(), "PIPELINE", rowAccess: _rowAccess);
                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("PIPELINE", fieldId),
                        Operator = "="
                    }
                };
                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);
                var results = await repo.GetAsync(filters);
                // Return PPDM models directly
                if (results.Cast<PIPELINE>().Any(row => row.FIELD_ID != fieldId))
                    throw RefusalException.Conflict("PIPELINE rows differ from the reviewed FIELD.");
                return await ConfirmAsync(results.Cast<PIPELINE>().ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error getting pipelines for field: {fieldId}");
                throw;
            }
        }

        public async Task<PIPELINE> CreatePipelineForFieldAsync(string fieldId, PipelineRequest pipelineData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PIPELINE table metadata not found");
                }

                // Convert DTO to PPDM model
                var pipelineEntity = _mappingService.ConvertDTOToPPDMModel<PIPELINE, PipelineRequest>(pipelineData);
                // PIPELINE field link is PRIMARY_FIELD_ID (not FIELD_ID)
                pipelineEntity.PRIMARY_FIELD_ID = _defaults.FormatIdForTable("PIPELINE", fieldId);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PIPELINE), await RequireCurrentBindingAsync(), "PIPELINE", rowAccess: _rowAccess);
                var result = await repo.InsertAsync(pipelineEntity, userId);
                // Return PPDM model directly
                return await ConfirmAsync((PIPELINE)result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error creating pipeline for field: {fieldId}");
                throw;
            }
        }

#region Gas Lift Integration
        /// <summary>
        /// Analyzes gas lift potential for a well
        /// </summary>
        public async Task<GAS_LIFT_WELL_PROPERTIES> AnalyzeGasLiftPotentialAsync(string fieldId, string wellId, decimal minGasInjectionRate = 100m, decimal maxGasInjectionRate = 5000m, int numberOfPoints = 50, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureWellBelongsToFieldAsync(fieldId, wellId);
            try
            {
                _logger?.LogInformation("Analyzing gas lift potential for well {WellId} in field {FieldId}", wellId, fieldId);
                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);
                // Perform gas lift potential analysis
                var result = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(wellProperties, minGasInjectionRate, maxGasInjectionRate, numberOfPoints);
                // Store results in WELL_EQUIPMENT table
                await StoreGasLiftPotentialResultsAsync(wellId, result, _review!.ActorUserId);
                _logger?.LogInformation("Gas lift potential analysis completed for well {WellId}. Optimal injection rate: {Rate} Mscf/day, Max production: {Production} bbl/day", wellId, result.OPTIMAL_GAS_INJECTION_RATE, result.MAXIMUM_PRODUCTION_RATE);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing gas lift potential for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Designs gas lift valves for a well (US field units)
        /// </summary>
        public async Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignGasLiftValvesAsync(string fieldId, string wellId, decimal gasInjectionPressure, int numberOfValves = 5, bool useSIUnits = false, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureWellBelongsToFieldAsync(fieldId, wellId);
            try
            {
                _logger?.LogInformation("Designing gas lift valves for well {WellId} in field {FieldId}", wellId, fieldId);
                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);
                // Design valves
                GAS_LIFT_VALVE_DESIGN_RESULT result;
                if (useSIUnits)
                {
                    result = GasLiftValveDesignCalculator.DesignValvesSI(wellProperties, gasInjectionPressure, numberOfValves);
                }
                else
                {
                    result = GasLiftValveDesignCalculator.DesignValvesUS(wellProperties, gasInjectionPressure, numberOfValves);
                }

                // Store valve design in WELL_EQUIPMENT table
                await StoreGasLiftValveDesignAsync(wellId, result, _review!.ActorUserId);
                _logger?.LogInformation("Gas lift valve design completed for well {WellId}. Designed {Count} valves", wellId, result.Valves.Count);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error designing gas lift valves for well {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Calculates gas lift valve spacing
        /// </summary>
        public async Task<GasLiftValveSpacingResult> CalculateGasLiftValveSpacingAsync(string fieldId, string wellId, decimal gasInjectionPressure, int numberOfValves = 5, string spacingMethod = "EQUAL_PRESSURE_DROP", // EQUAL_PRESSURE_DROP or EQUAL_DEPTH
 string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureWellBelongsToFieldAsync(fieldId, wellId);
            try
            {
                _logger?.LogInformation("Calculating gas lift valve spacing for well {WellId} in field {FieldId}", wellId, fieldId);
                // Get well properties from PPDM
                var wellProperties = await GetGasLiftWellPropertiesAsync(fieldId, wellId);
                // Calculate valve spacing
                GasLiftValveSpacingResult result;
                if (spacingMethod == "EQUAL_DEPTH")
                {
                    result = GasLiftValveSpacingCalculator.CalculateEqualDepthSpacing(wellProperties, gasInjectionPressure, numberOfValves);
                }
                else
                {
                    result = GasLiftValveSpacingCalculator.CalculateEqualPressureDropSpacing(wellProperties, gasInjectionPressure, numberOfValves);
                }

                // Store spacing results
                await StoreGasLiftValveSpacingAsync(wellId, result, spacingMethod, _review!.ActorUserId);
                _logger?.LogInformation("Gas lift valve spacing calculated for well {WellId}. Method: {Method}", wellId, spacingMethod);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating gas lift valve spacing for well {WellId}", wellId);
                throw;
            }
        }

#endregion
#region Gas Lift Helper Methods
        /// <summary>
        /// Retrieves well properties from PPDM for gas lift analysis
        /// </summary>
        private async Task<GAS_LIFT_WELL_PROPERTIES> GetGasLiftWellPropertiesAsync(string fieldId, string wellId)
        {
            try
            {
                // Get well data from PPDM
                var wellMetadata = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMetadata == null)
                {
                    throw new InvalidOperationException("WELL table metadata not found");
                }

                var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL), await RequireCurrentBindingAsync(), "WELL", rowAccess: _rowAccess);
                var wellFilters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        Operator = "=",
                        FilterValue = wellId
                    },
                    new AppFilter
                    {
                        FieldName = "ASSIGNED_FIELD",
                        Operator = "=",
                        FilterValue = fieldId
                    }
                };
                var wells = await wellRepo.GetAsync(wellFilters);
                var wellsList = wells?.ToList() ?? new List<object>();
                if (wellsList.Count == 0)
                {
                    throw RefusalException.NotFound($"Well {wellId} was not found in field {fieldId}.");
                }

                var well = wellsList[0] as WELL;
                if (well == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve well {wellId}");
                }

                if (well.UWI != wellId || well.ASSIGNED_FIELD != fieldId || well.ACTIVE_IND != "Y")
                    throw RefusalException.Conflict("The WELL differs from the reviewed analysis resource.");

                // Compute well depth with UOM conversion (FINAL_TD preferred over DRILL_TD)
                decimal toFt(decimal depth, string? uom) => string.Equals(uom, "M", StringComparison.OrdinalIgnoreCase) ? depth * 3.28084m : depth;
                var wellDepthFt = well.FINAL_TD > 0 ? toFt(well.FINAL_TD, well.FINAL_TD_OUOM) : well.DRILL_TD > 0 ? toFt(well.DRILL_TD, well.FINAL_TD_OUOM) : 10000m;
                // Query WELL_TUBULAR for tubing and casing outer diameters
                var tubularRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_TUBULAR), await RequireCurrentBindingAsync(), "WELL_TUBULAR", rowAccess: _rowAccess);
                var tubingRecords = await tubularRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId }, new AppFilter { FieldName = "TUBING_TYPE", Operator = "=", FilterValue = "TUBING" }, new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } });
                var casingRecords = await tubularRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId }, new AppFilter { FieldName = "TUBING_TYPE", Operator = "=", FilterValue = "CASING" }, new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } });
                var tubingOd = tubingRecords?.OfType<WELL_TUBULAR>().Where(t => t.OUTSIDE_DIAMETER > 0).OrderByDescending(t => t.OUTSIDE_DIAMETER).FirstOrDefault()?.OUTSIDE_DIAMETER;
                var casingOd = casingRecords?.OfType<WELL_TUBULAR>().Where(t => t.OUTSIDE_DIAMETER > 0).OrderByDescending(t => t.OUTSIDE_DIAMETER).FirstOrDefault()?.OUTSIDE_DIAMETER;
                // Query latest WELL_TEST for fluid and pressure properties
                var testRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_TEST), await RequireCurrentBindingAsync(), "WELL_TEST", rowAccess: _rowAccess);
                var testRecords = await testRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = wellId }, new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" } });
                var latestTest = testRecords?.OfType<WELL_TEST>().OrderByDescending(t => t.TEST_DATE ?? t.EFFECTIVE_DATE).FirstOrDefault();
                // Wellhead temp in Rankine; bottomhole via geothermal gradient (~1.5°F/100 ft)
                var wellheadTempF = latestTest?.FLOW_TEMPERATURE > 0 ? latestTest.FLOW_TEMPERATURE : 60m;
                var wellheadTempR = wellheadTempF + 459.67m;
                var bottomholeTempR = wellheadTempR + (wellDepthFt * 0.015m);
                var wellProperties = new GAS_LIFT_WELL_PROPERTIES
                {
                    WELL_DEPTH = wellDepthFt,
                    TUBING_DIAMETER = (int)Math.Round(tubingOd ?? 2.875m),
                    CASING_DIAMETER = casingOd ?? 7.0m,
                    WELLHEAD_PRESSURE = latestTest?.FLOW_PRESSURE > 0 ? latestTest.FLOW_PRESSURE : 100m,
                    BOTTOM_HOLE_PRESSURE = latestTest?.STATIC_PRESSURE > 0 ? latestTest.STATIC_PRESSURE : 2000m,
                    WELLHEAD_TEMPERATURE = wellheadTempR,
                    BOTTOM_HOLE_TEMPERATURE = bottomholeTempR,
                    OIL_GRAVITY = latestTest?.OIL_GRAVITY > 0 ? latestTest.OIL_GRAVITY : 35m,
                    WATER_CUT = latestTest?.WATER_CUT_PERCENT > 0 ? latestTest.WATER_CUT_PERCENT / 100m : 0.3m,
                    GAS_OIL_RATIO = latestTest?.GOR > 0 ? latestTest.GOR : 500m,
                    GAS_SPECIFIC_GRAVITY = latestTest?.GAS_GRAVITY > 0 ? latestTest.GAS_GRAVITY : 0.65m,
                    DESIRED_PRODUCTION_RATE = 2000m
                };
                _logger?.LogInformation("Gas lift well properties loaded for {WellId}: depth={Depth}ft, tubingOd={TubingOd}in, testDate={TestDate}", wellId, wellDepthFt, wellProperties.TUBING_DIAMETER, latestTest?.TEST_DATE?.ToString("yyyy-MM-dd") ?? "none");
                return wellProperties;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving well properties for gas lift analysis");
                throw;
            }
        }

        /// <summary>
        /// Stores gas lift potential analysis results in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreGasLiftPotentialResultsAsync(string wellId, GAS_LIFT_WELL_PROPERTIES result, string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    throw RefusalException.Conflict("WELL_EQUIPMENT metadata is unavailable; the development analysis was not saved.");
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_EQUIPMENT), await RequireCurrentBindingAsync(), "WELL_EQUIPMENT", rowAccess: _rowAccess);
                var equipmentRecord = new WELL_EQUIPMENT
                {
                    UWI = wellId,
                    EQUIPMENT_ID = $"GAS_LIFT_ANALYSIS_{Guid.NewGuid():N}",
                    SOURCE = "LIFECYCLE",
                    INSTALL_DATE = DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    REMARK = $"GAS_LIFT_SYSTEM | Optimal injection: {result.OPTIMAL_GAS_INJECTION_RATE:F2} Mscf/day, Max production: {result.MAXIMUM_PRODUCTION_RATE:F2} bbl/day, Optimal GLR: {result.OPTIMAL_GAS_LIQUID_RATIO:F2}"};
                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored gas lift potential results for well {WellId}", wellId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

        /// <summary>
        /// Stores gas lift valve design in WELL_EQUIPMENT table
        /// </summary>
        private async Task StoreGasLiftValveDesignAsync(string wellId, GAS_LIFT_VALVE_DESIGN_RESULT result, string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    throw RefusalException.Conflict("WELL_EQUIPMENT metadata is unavailable; the development analysis was not saved.");
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_EQUIPMENT), await RequireCurrentBindingAsync(), "WELL_EQUIPMENT", rowAccess: _rowAccess);
                // Store each valve as a separate equipment record
                foreach (var valve in result.Valves)
                {
                    var equipmentRecord = new WELL_EQUIPMENT
                    {
                        UWI = wellId,
                        EQUIPMENT_ID = $"GLV_{valve.DEPTH:F0}_{Guid.NewGuid():N}",
                        SOURCE = "LIFECYCLE",
                        INSTALL_DATE = DateTime.UtcNow,
                        INSTALL_TOP_DEPTH = valve.DEPTH,
                        ACTIVE_IND = "Y",
                        REMARK = $"GAS_LIFT_VALVE | Depth: {valve.DEPTH:F0} ft, Port: {valve.PORT_SIZE:F3} in, Open P: {valve.OPENING_PRESSURE:F2} psia, Close P: {valve.CLOSING_PRESSURE:F2} psia, Type: {valve.VALVE_TYPE}"};
                    await equipmentRepo.InsertAsync(equipmentRecord, userId);
                }

                _logger?.LogInformation("Stored gas lift valve design for well {WellId} ({Count} valves)", wellId, result.Valves.Count);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

        /// <summary>
        /// Stores gas lift valve spacing results
        /// </summary>
        private async Task StoreGasLiftValveSpacingAsync(string wellId, GasLiftValveSpacingResult result, string spacingMethod, string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    throw RefusalException.Conflict("WELL_EQUIPMENT metadata is unavailable; the development analysis was not saved.");
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_EQUIPMENT), await RequireCurrentBindingAsync(), "WELL_EQUIPMENT", rowAccess: _rowAccess);
                var equipmentRecord = new WELL_EQUIPMENT
                {
                    UWI = wellId,
                    EQUIPMENT_ID = $"GLV_SPACING_{Guid.NewGuid():N}",
                    SOURCE = "LIFECYCLE",
                    INSTALL_DATE = DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    REMARK = $"GAS_LIFT_VALVE_SPACING | Method: {spacingMethod}, Valves: {result.NumberOfValves}, Depth coverage: {result.TotalDepthCoverage:F0} ft"};
                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored gas lift valve spacing for well {WellId}", wellId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

#endregion
#region Pipeline Analysis Integration
        /// <summary>
        /// Analyzes pipeline capacity for gas or liquid pipelines
        /// </summary>
        public async Task<PIPELINE_CAPACITY_RESULT> AnalyzePipelineCapacityAsync(string fieldId, string pipelineId, string pipelineType = "GAS", // GAS or LIQUID
 decimal? gasFlowRate = null, decimal? liquidFlowRate = null, decimal? gasSpecificGravity = null, decimal? liquidSpecificGravity = null, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsurePipelineBelongsToFieldAsync(fieldId, pipelineId);
            try
            {
                _logger?.LogInformation("Analyzing pipeline capacity for pipeline {PipelineId} in field {FieldId}, type: {Type}", pipelineId, fieldId, pipelineType);
                // Get pipeline properties from PPDM
                var PIPELINE_PROPERTIES = await GetPipelinePropertiesFromPPDMAsync(fieldId, pipelineId);
                PIPELINE_CAPACITY_RESULT result;
                if (pipelineType.ToUpper() == "GAS")
                {
                    // Gas pipeline capacity
                    var gasFlowProperties = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = PIPELINE_PROPERTIES,
                        GAS_FLOW_RATE = gasFlowRate ?? 1000m, // Default
                        GAS_SPECIFIC_GRAVITY = gasSpecificGravity ?? 0.65m,
                        GAS_MOLECULAR_WEIGHT = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                        BASE_PRESSURE = 14.7m,
                        BASE_TEMPERATURE = 520m
                    };
                    result = PipelineCapacityCalculator.CalculateGasPipelineCapacity(gasFlowProperties);
                }
                else
                {
                    // Liquid pipeline capacity
                    var liquidFlowProperties = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = PIPELINE_PROPERTIES,
                        LIQUID_FLOW_RATE = liquidFlowRate ?? 1000m, // Default
                        LIQUID_SPECIFIC_GRAVITY = liquidSpecificGravity ?? 0.85m,
                        LIQUID_VISCOSITY = 1.0m
                    };
                    result = PipelineCapacityCalculator.CalculateLiquidPipelineCapacity(liquidFlowProperties);
                }

                // Store results in PIPELINE table
                await StorePipelineCapacityResultsAsync(pipelineId, result, pipelineType, _review!.ActorUserId);
                _logger?.LogInformation("Pipeline capacity analysis completed for pipeline {PipelineId}. Max flow rate: {FlowRate}, Pressure drop: {PressureDrop}", pipelineId, result.MAXIMUM_FLOW_RATE, result.PRESSURE_DROP);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing pipeline capacity for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes pipeline flow (flow rate or pressure drop) for gas or liquid pipelines
        /// </summary>
        public async Task<PIPELINE_FLOW_ANALYSIS_RESULT> AnalyzePipelineFlowAsync(string fieldId, string pipelineId, string pipelineType = "GAS", // GAS or LIQUID
 decimal? gasFlowRate = null, decimal? liquidFlowRate = null, decimal? gasSpecificGravity = null, decimal? liquidSpecificGravity = null, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsurePipelineBelongsToFieldAsync(fieldId, pipelineId);
            try
            {
                _logger?.LogInformation("Analyzing pipeline flow for pipeline {PipelineId} in field {FieldId}, type: {Type}", pipelineId, fieldId, pipelineType);
                // Get pipeline properties from PPDM
                var PIPELINE_PROPERTIES = await GetPipelinePropertiesFromPPDMAsync(fieldId, pipelineId);
                PIPELINE_FLOW_ANALYSIS_RESULT result;
                if (pipelineType.ToUpper() == "GAS")
                {
                    // Gas pipeline flow
                    var gasFlowProperties = new GAS_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = PIPELINE_PROPERTIES,
                        GAS_FLOW_RATE = gasFlowRate ?? 1000m,
                        GAS_SPECIFIC_GRAVITY = gasSpecificGravity ?? 0.65m,
                        GAS_MOLECULAR_WEIGHT = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                        BASE_PRESSURE = 14.7m,
                        BASE_TEMPERATURE = 520m
                    };
                    result = PipelineFlowCalculator.CalculateGasFlow(gasFlowProperties);
                }
                else
                {
                    // Liquid pipeline flow
                    var liquidFlowProperties = new LIQUID_PIPELINE_FLOW_PROPERTIES
                    {
                        PIPELINE_PROPERTIES = PIPELINE_PROPERTIES,
                        LIQUID_FLOW_RATE = liquidFlowRate ?? 1000m,
                        LIQUID_SPECIFIC_GRAVITY = liquidSpecificGravity ?? 0.85m,
                        LIQUID_VISCOSITY = 1.0m
                    };
                    result = PipelineFlowCalculator.CalculateLiquidFlow(liquidFlowProperties);
                }

                // Store results in PIPELINE table
                await StorePipelineFlowResultsAsync(pipelineId, result, pipelineType, _review!.ActorUserId);
                _logger?.LogInformation("Pipeline flow analysis completed for pipeline {PipelineId}. Flow rate: {FlowRate}, Pressure drop: {PressureDrop}", pipelineId, result.FLOW_RATE, result.PRESSURE_DROP);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing pipeline flow for pipeline {PipelineId}", pipelineId);
                throw;
            }
        }

#endregion
#region Pipeline Analysis Helper Methods
        /// <summary>
        /// Retrieves pipeline properties from PPDM for analysis
        /// </summary>
        private async Task<PIPELINE_PROPERTIES> GetPipelinePropertiesFromPPDMAsync(string fieldId, string pipelineId)
        {
            try
            {
                // Get pipeline data from PPDM
                var pipelineMetadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (pipelineMetadata == null)
                {
                    throw new InvalidOperationException("PIPELINE table metadata not found");
                }

                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PIPELINE), await RequireCurrentBindingAsync(), "PIPELINE", rowAccess: _rowAccess);
                var pipelineFilters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PIPELINE_ID",
                        Operator = "=",
                        FilterValue = pipelineId
                    },
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        Operator = "=",
                        FilterValue = fieldId
                    }
                };
                var pipelines = await pipelineRepo.GetAsync(pipelineFilters);
                var pipelinesList = pipelines?.ToList() ?? new List<object>();
                if (pipelinesList.Count == 0)
                {
                    throw RefusalException.NotFound($"Pipeline {pipelineId} was not found in field {fieldId}.");
                }

                var pipeline = pipelinesList[0] as PIPELINE;
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Failed to retrieve pipeline {pipelineId}");
                }

                if (pipeline.PIPELINE_ID != pipelineId || pipeline.FIELD_ID != fieldId || pipeline.ACTIVE_IND != "Y")
                    throw RefusalException.Conflict("The PIPELINE differs from the reviewed analysis resource.");

                // Map PPDM pipeline record to analysis properties
                // Convert diameter and length to target UOM (inches / feet)
                decimal mapDiameter(decimal d, string? uom) => string.Equals(uom, "MM", StringComparison.OrdinalIgnoreCase) ? d / 25.4m : string.Equals(uom, "CM", StringComparison.OrdinalIgnoreCase) ? d / 2.54m : string.Equals(uom, "M", StringComparison.OrdinalIgnoreCase) ? d * 39.3701m : d; // assume IN
                decimal mapLength(decimal l, string? uom) => string.Equals(uom, "M", StringComparison.OrdinalIgnoreCase) ? l * 3.28084m : string.Equals(uom, "KM", StringComparison.OrdinalIgnoreCase) ? l * 3280.84m : string.Equals(uom, "MI", StringComparison.OrdinalIgnoreCase) ? l * 5280m : l; // assume FT
                var diameter = pipeline.DIAMETER > 0 ? mapDiameter(pipeline.DIAMETER, pipeline.DIAMETER_OUOM) : 12m;
                var length = pipeline.LENGTH > 0 ? mapLength(pipeline.LENGTH, pipeline.LENGTH_OUOM) : 50000m;
                // DESIGN_PRESSURE and DESIGN_TEMPERATURE come directly from the PPDM record
                // ELEVATION is the net elevation change along the route
                // Outlet pressure is estimated at 50% of design pressure if not separately stored
                var inletPressure = pipeline.DESIGN_PRESSURE > 0 ? pipeline.DESIGN_PRESSURE : 1000m;
                var avgTempF = pipeline.DESIGN_TEMPERATURE > 0 ? pipeline.DESIGN_TEMPERATURE : 80m;
                var avgTempR = avgTempF + 459.67m;
                var PIPELINE_PROPERTIES = new PIPELINE_PROPERTIES
                {
                    DIAMETER = diameter,
                    LENGTH = length,
                    ROUGHNESS = 0.00015m, // Standard commercial steel pipe roughness (ft)
                    ELEVATION_CHANGE = pipeline.ELEVATION,
                    INLET_PRESSURE = inletPressure,
                    OUTLET_PRESSURE = inletPressure * 0.5m, // Conservative: 50% pressure recovery
                    AVERAGE_TEMPERATURE = avgTempR
                };
                _logger?.LogInformation("Pipeline properties loaded for {PipelineId}: diameter={Diameter}in, length={Length}ft, elevation={Elevation}ft", pipelineId, PIPELINE_PROPERTIES.DIAMETER, PIPELINE_PROPERTIES.LENGTH, PIPELINE_PROPERTIES.ELEVATION_CHANGE);
                return PIPELINE_PROPERTIES;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving pipeline properties for analysis");
                throw;
            }
        }

        /// <summary>
        /// Stores pipeline capacity analysis results in PIPELINE table
        /// </summary>
        private async Task StorePipelineCapacityResultsAsync(string pipelineId, PIPELINE_CAPACITY_RESULT result, string pipelineType, string userId)
        {
            try
            {
                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PIPELINE), await RequireCurrentBindingAsync(), "PIPELINE", rowAccess: _rowAccess);
                // Update pipeline with capacity results
                // Note: In production, you might want to store in a separate PIPELINE_ANALYSIS table
                var updateData = new Dictionary<string, object>
                {
                    ["PIPELINE_ID"] = pipelineId,
                    ["CAPACITY_ANALYSIS_JSON"] = JsonSerializer.Serialize(new { PipelineType = pipelineType, MaximumFlowRate = result.MAXIMUM_FLOW_RATE, PressureDrop = result.PRESSURE_DROP, FlowVelocity = result.FLOW_VELOCITY, ReynoldsNumber = result.REYNOLDS_NUMBER, FrictionFactor = result.FRICTION_FACTOR, PressureGradient = result.PRESSURE_GRADIENT, OutletPressure = result.OUTLET_PRESSURE, AnalysisDate = DateTime.UtcNow })
                };
                await pipelineRepo.UpdateAsync(updateData, userId);
                _logger?.LogInformation("Stored pipeline capacity results for pipeline {PipelineId}", pipelineId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

        /// <summary>
        /// Stores pipeline flow analysis results in PIPELINE table
        /// </summary>
        private async Task StorePipelineFlowResultsAsync(string pipelineId, PIPELINE_FLOW_ANALYSIS_RESULT result, string pipelineType, string userId)
        {
            try
            {
                var pipelineRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(PIPELINE), await RequireCurrentBindingAsync(), "PIPELINE", rowAccess: _rowAccess);
                // Update pipeline with flow results
                var updateData = new Dictionary<string, object>
                {
                    ["PIPELINE_ID"] = pipelineId,
                    ["FLOW_ANALYSIS_JSON"] = JsonSerializer.Serialize(new { PipelineType = pipelineType, FlowRate = result.FLOW_RATE, PressureDrop = result.PRESSURE_DROP, FlowVelocity = result.FLOW_VELOCITY, ReynoldsNumber = result.REYNOLDS_NUMBER, FrictionFactor = result.FRICTION_FACTOR, FlowRegime = result.FLOW_REGIME, AnalysisDate = DateTime.UtcNow })
                };
                await pipelineRepo.UpdateAsync(updateData, userId);
                _logger?.LogInformation("Stored pipeline flow results for pipeline {PipelineId}", pipelineId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

#endregion
#region Compressor Analysis Integration
        /// <summary>
        /// Analyzes compressor power requirements for centrifugal or reciprocating compressors
        /// </summary>
        public async Task<COMPRESSOR_POWER_RESULT> AnalyzeCompressorPowerAsync(string fieldId, string facilityId, string compressorType = CompressorAnalysisWellKnown.CompressorType.Centrifugal, decimal? suctionPressure = null, decimal? dischargePressure = null, decimal? gasFlowRate = null, decimal? gasSpecificGravity = null, bool useSIUnits = false, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureFacilityBelongsToFieldAsync(fieldId, facilityId);
            try
            {
                _logger?.LogInformation("Analyzing compressor power for facility {FacilityId} in field {FieldId}, type: {Type}", facilityId, fieldId, compressorType);
                // Get compressor operating conditions from PPDM or request
                var operatingConditions = await GetCompressorOperatingConditionsAsync(fieldId, facilityId, suctionPressure, dischargePressure, gasFlowRate, gasSpecificGravity);
                COMPRESSOR_POWER_RESULT result;
                if (string.Equals(compressorType, CompressorAnalysisWellKnown.CompressorType.Reciprocating, StringComparison.OrdinalIgnoreCase))
                {
                    // Reciprocating compressor
                    var compressorProperties = await GetReciprocatingCompressorPropertiesAsync(facilityId, operatingConditions);
                    result = ReciprocatingCompressorCalculator.CalculatePower(compressorProperties, useSIUnits);
                }
                else
                {
                    // Centrifugal compressor
                    var compressorProperties = await GetCentrifugalCompressorPropertiesAsync(facilityId, operatingConditions);
                    result = CentrifugalCompressorCalculator.CalculatePower(compressorProperties, useSIUnits);
                }

                // Store results in FACILITY_EQUIPMENT table
                await StoreCompressorPowerResultsAsync(facilityId, result, compressorType, _review!.ActorUserId);
                _logger?.LogInformation("Compressor power analysis completed for facility {FacilityId}. Brake HP: {BHP}, Motor HP: {MotorHP}", facilityId, result.BRAKE_HORSEPOWER, result.MOTOR_HORSEPOWER);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing compressor power for facility {FacilityId}", facilityId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes compressor pressure requirements
        /// </summary>
        public async Task<COMPRESSOR_PRESSURE_RESULT> AnalyzeCompressorPressureAsync(string fieldId, string facilityId, decimal requiredFlowRate, decimal? maxPower = null, decimal? compressorEfficiency = null, decimal? suctionPressure = null, decimal? gasSpecificGravity = null, string? userId = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureFacilityBelongsToFieldAsync(fieldId, facilityId);
            try
            {
                _logger?.LogInformation("Analyzing compressor pressure for facility {FacilityId} in field {FieldId}", facilityId, fieldId);
                // Get compressor operating conditions
                var operatingConditions = await GetCompressorOperatingConditionsAsync(fieldId, facilityId, suctionPressure, null, requiredFlowRate, gasSpecificGravity);
                // Calculate required pressure
                var result = CompressorPressureCalculator.CalculateRequiredPressure(operatingConditions, requiredFlowRate, maxPower ?? 1000m, compressorEfficiency ?? 0.75m);
                // Store results in FACILITY_EQUIPMENT table
                await StoreCompressorPressureResultsAsync(facilityId, result, _review!.ActorUserId);
                _logger?.LogInformation("Compressor pressure analysis completed for facility {FacilityId}. Required discharge pressure: {Pressure} psia", facilityId, result.REQUIRED_DISCHARGE_PRESSURE);
                return await ConfirmAsync(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing compressor pressure for facility {FacilityId}", facilityId);
                throw;
            }
        }

#endregion
#region Compressor Analysis Helper Methods
        /// <summary>
        /// Retrieves compressor operating conditions from PPDM
        /// </summary>
        private async Task<COMPRESSOR_OPERATING_CONDITIONS> GetCompressorOperatingConditionsAsync(string fieldId, string facilityId, decimal? suctionPressure, decimal? dischargePressure, decimal? gasFlowRate, decimal? gasSpecificGravity)
        {
            try
            {
                // Caller-supplied pressures, flow rate, and gas gravity take precedence.
                // Temperature defaults: suction 60°F (520°R), discharge 140°F (600°R) — industry-standard initial estimates.
                // Efficiency defaults: overall 75%, mechanical 95% — typical centrifugal/reciprocating values.
                var operatingConditions = new COMPRESSOR_OPERATING_CONDITIONS
                {
                    SUCTION_PRESSURE = suctionPressure ?? 100m,
                    DISCHARGE_PRESSURE = dischargePressure ?? 500m,
                    SUCTION_TEMPERATURE = 520m, // 60°F in Rankine
                    DISCHARGE_TEMPERATURE = 600m, // 140°F in Rankine
                    GAS_FLOW_RATE = gasFlowRate ?? 1000m,
                    GAS_SPECIFIC_GRAVITY = gasSpecificGravity ?? 0.65m,
                    GAS_MOLECULAR_WEIGHT = (gasSpecificGravity ?? 0.65m) * 28.9645m,
                    COMPRESSOR_EFFICIENCY = 0.75m,
                    MECHANICAL_EFFICIENCY = 0.95m
                };
                if (!suctionPressure.HasValue || !dischargePressure.HasValue || !gasFlowRate.HasValue)
                    _logger?.LogWarning("Compressor analysis for facility {FacilityId}: using engineering defaults for unspecified parameters", facilityId);
                return operatingConditions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving compressor operating conditions");
                throw;
            }
        }

        /// <summary>
        /// Gets centrifugal compressor properties from PPDM
        /// </summary>
        private async Task<CENTRIFUGAL_COMPRESSOR_PROPERTIES> GetCentrifugalCompressorPropertiesAsync(string facilityId, COMPRESSOR_OPERATING_CONDITIONS operatingConditions)
        {
            // Standard engineering defaults: polytropic efficiency 75%, k=1.3 (natural gas), single-stage, 3600 RPM sync motor
            return new CENTRIFUGAL_COMPRESSOR_PROPERTIES
            {
                OPERATING_CONDITIONS = operatingConditions,
                POLYTROPIC_EFFICIENCY = 0.75m,
                SPECIFIC_HEAT_RATIO = 1.3m,
                NUMBER_OF_STAGES = 1,
                SPEED = 3600
            };
        }

        /// <summary>
        /// Gets reciprocating compressor properties from PPDM
        /// </summary>
        private async Task<RECIPROCATING_COMPRESSOR_PROPERTIES> GetReciprocatingCompressorPropertiesAsync(string facilityId, COMPRESSOR_OPERATING_CONDITIONS operatingConditions)
        {
            // Standard engineering defaults: 12" bore, 6" stroke, 300 RPM (slow-speed), 2 cylinders, 85% volumetric efficiency, 5% clearance
            return new RECIPROCATING_COMPRESSOR_PROPERTIES
            {
                OPERATING_CONDITIONS = operatingConditions,
                CYLINDER_DIAMETER = 12m,
                STROKE_LENGTH = 6m,
                ROTATIONAL_SPEED = 300m,
                NUMBER_OF_CYLINDERS = 2,
                VOLUMETRIC_EFFICIENCY = 0.85m,
                CLEARANCE_FACTOR = (int)0.05m
            };
        }

        /// <summary>
        /// Stores compressor power analysis results in FACILITY_EQUIPMENT table
        /// </summary>
        private async Task StoreCompressorPowerResultsAsync(string facilityId, COMPRESSOR_POWER_RESULT result, string compressorType, string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("FACILITY_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    throw RefusalException.Conflict("FACILITY_EQUIPMENT metadata is unavailable; the development analysis was not saved.");
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY_EQUIPMENT), await RequireCurrentBindingAsync(), "FACILITY_EQUIPMENT", rowAccess: _rowAccess);
                var equipmentRecord = new FACILITY_EQUIPMENT
                {
                    FACILITY_ID = facilityId,
                    FACILITY_TYPE = "COMPRESSOR",
                    EQUIPMENT_ID = $"COMPR_POWER_{Guid.NewGuid():N}",
                    SOURCE = "LIFECYCLE",
                    INSTALL_DATE = DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    USE_DESCRIPTION = $"{compressorType} Compressor Power Analysis",
                    REMARK = $"Brake HP: {result.BRAKE_HORSEPOWER:F2}, Motor HP: {result.MOTOR_HORSEPOWER:F2}, Power kW: {result.POWER_CONSUMPTION_KW:F2}, Ratio: {result.COMPRESSION_RATIO:F3}, Poly head: {result.POLYTROPIC_HEAD:F2}, Adiabatic head: {result.ADIABATIC_HEAD:F2}"};
                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored compressor power results for facility {FacilityId}", facilityId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

        /// <summary>
        /// Stores compressor pressure analysis results in FACILITY_EQUIPMENT table
        /// </summary>
        private async Task StoreCompressorPressureResultsAsync(string facilityId, COMPRESSOR_PRESSURE_RESULT result, string userId)
        {
            try
            {
                var equipmentMetadata = await _metadata.GetTableMetadataAsync("FACILITY_EQUIPMENT");
                if (equipmentMetadata == null)
                {
                    throw RefusalException.Conflict("FACILITY_EQUIPMENT metadata is unavailable; the development analysis was not saved.");
                }

                var equipmentRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(FACILITY_EQUIPMENT), await RequireCurrentBindingAsync(), "FACILITY_EQUIPMENT", rowAccess: _rowAccess);
                var equipmentRecord = new FACILITY_EQUIPMENT
                {
                    FACILITY_ID = facilityId,
                    FACILITY_TYPE = "COMPRESSOR",
                    EQUIPMENT_ID = $"COMPR_PRESSURE_{Guid.NewGuid():N}",
                    SOURCE = "LIFECYCLE",
                    INSTALL_DATE = DateTime.UtcNow,
                    ACTIVE_IND = "Y",
                    USE_DESCRIPTION = "Compressor Pressure Analysis",
                    REMARK = $"Required discharge P: {result.REQUIRED_DISCHARGE_PRESSURE:F2} psia, Ratio: {result.COMPRESSION_RATIO:F3}, Power: {result.REQUIRED_POWER:F2} HP, Discharge T: {result.DISCHARGE_TEMPERATURE:F2} °F"};
                await equipmentRepo.InsertAsync(equipmentRecord, userId);
                _logger?.LogInformation("Stored compressor pressure results for facility {FacilityId}", facilityId);
            }
            catch (Exception ex)when (ex is not OperationCanceledException)
            {
                _logger?.LogError(ex, "Development analysis storage failed.");
                throw;
            }
        }

#endregion
#region Development Planning Integration
        /// <summary>
        /// Designs a well using DevelopmentPlanning service
        /// </summary>
        public async Task<WellPlan> DesignWellAsync(string fieldId, string planId, CreateWellPlan wellPlanData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            try
            {
                _logger?.LogInformation("Designing well for plan: {PlanId} in field: {FieldId}", planId, fieldId);
                var wellPlanId = Guid.NewGuid().ToString();
                var wellId = wellPlanId;
                // Store well design event in WELL_ACTIVITY
                var activityMeta = await _metadata.GetTableMetadataAsync("WELL_ACTIVITY");
                if (activityMeta == null)
                    throw RefusalException.Conflict("WELL_ACTIVITY metadata is unavailable; the development command was not saved.");
                if (activityMeta != null)
                {
                    var activityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(WELL_ACTIVITY), await RequireCurrentBindingAsync(), "WELL_ACTIVITY", rowAccess: _rowAccess);
                    var activity = new WELL_ACTIVITY
                    {
                        UWI = _defaults.FormatIdForTable("WELL_ACTIVITY", wellId),
                        SOURCE = "LIFECYCLE",
                        ACTIVITY_OBS_NO = Math.Abs((decimal)Guid.NewGuid().GetHashCode()),
                        ACTIVITY_TYPE_ID = "WELL_DESIGN",
                        EVENT_DATE = DateTime.UtcNow,
                        REMARK = $"PlanId:{planId}; WellPlanId:{wellPlanId}; Name:{wellPlanData.WellName ?? "Well"}; Type:{wellPlanData.WellType ?? "PRODUCTION"}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (activity is IPPDMEntity activityEntity)
                        _commonColumnHandler.PrepareForInsert(activityEntity, userId);
                    await activityRepo.InsertAsync(activity, userId);
                }

                return await ConfirmAsync(new WellPlan { WellPlanId = wellPlanId, PlanId = planId, WellName = wellPlanData.WellName ?? "Well", WellType = wellPlanData.WellType ?? "PRODUCTION", Status = "DESIGNED", CreatedDate = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error designing well for plan: {PlanId}", planId);
                throw;
            }
        }

#endregion
#region Drilling and Construction Integration
        /// <summary>
        /// Constructs facility using DrillingAndConstruction service
        /// </summary>
        public async Task<FacilityResponse> ConstructFacilityAsync(string fieldId, string facilityId, FacilityRequest facilityData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            if (string.IsNullOrWhiteSpace(userId)) throw RefusalException.Invalid("userId is required.");
            await RequireCurrentBindingAsync(fieldId, userId, true);
            await EnsureFacilityBelongsToFieldAsync(fieldId, facilityId);
            try
            {
                _logger?.LogInformation("Constructing facility: {FacilityId} in field: {FieldId}", facilityId, fieldId);
                // Use existing CreateFacilityForFieldAsync method
                return await ConfirmAsync(await CreateFacilityForFieldAsync(fieldId, facilityData, userId));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error constructing facility: {FacilityId}", facilityId);
                throw;
            }
        }

#endregion
#region Dashboard
        /// <summary>Returns well count summary for the Development dashboard KPI row.</summary>
        public async Task<DevelopmentDashboardSummary> GetDevelopmentDashboardSummaryAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var wells = await GetDevelopmentWellsForFieldAsync(fieldId);
                return await ConfirmAsync(new DevelopmentDashboardSummary { FieldId = fieldId, TotalWells = wells.Count, DrillingWells = wells.Count(w => w.CURRENT_STATUS != null && w.CURRENT_STATUS.Equals("DRILLING", StringComparison.OrdinalIgnoreCase)), CompletedWells = wells.Count(w => w.CURRENT_STATUS != null && (w.CURRENT_STATUS.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase) || w.CURRENT_STATUS.Equals("PRODUCING", StringComparison.OrdinalIgnoreCase))), PlannedWells = wells.Count(w => string.IsNullOrEmpty(w.CURRENT_STATUS) || w.CURRENT_STATUS.Equals("PLANNED", StringComparison.OrdinalIgnoreCase)), });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting development dashboard summary for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>Returns per-well status rows for the Development dashboard wells grid.</summary>
        public async Task<List<DevelopmentWellStatusDto>> GetDevelopmentWellStatusAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var wells = await GetDevelopmentWellsForFieldAsync(fieldId);
                return await ConfirmAsync(wells.Select(w => new DevelopmentWellStatusDto { WellId = w.UWI ?? string.Empty, WellName = w.WELL_NAME ?? w.UWI ?? string.Empty, WellType = w.CURRENT_CLASS ?? string.Empty, DrillingStatus = w.CURRENT_STATUS ?? "PLANNED", CurrentDepthM = (double)(w.DRILL_TD), TargetDepthM = (double)(w.FINAL_TD), }).ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting development well status for field {FieldId}", fieldId);
                throw;
            }
        }

#endregion
#region Reservoir Dashboard
        /// <summary>Returns pool count summary for the Reservoir Overview dashboard.</summary>
        public async Task<ReservoirDashboardSummary> GetReservoirDashboardSummaryAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var pools = await GetPoolsForFieldAsync(fieldId);
                var activePools = pools.Where(p => string.Equals(p.POOL_STATUS, "ACTIVE", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.POOL_STATUS)).ToList();
                return await ConfirmAsync(new ReservoirDashboardSummary { FieldId = fieldId, ActivePoolCount = activePools.Count, OoipMmbbl = 0, // Requires PDEN_VOL_SUMMARY data
 RecoveryFactorPct = 0, // Requires production volume data
 Reserves1PMmbbl = 0, // Requires reserves classification data
 Reserves2PMmbbl = 0, // Requires reserves classification data
 Reserves3PMmbbl = 0, // Requires reserves classification data
 ContingentResourcesMmbbl = 0, // Requires contingent resources classification
 });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting reservoir dashboard summary for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>Returns pool rows for the Reservoir Overview pools grid.</summary>
        public async Task<List<ReservoirPoolDto>> GetReservoirPoolsAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId)) throw RefusalException.Invalid("fieldId is required.");
            await RequireCurrentBindingAsync(fieldId, null, true);
            try
            {
                var pools = await GetPoolsForFieldAsync(fieldId);
                return await ConfirmAsync(pools.Select(p => new ReservoirPoolDto { PoolId = p.POOL_ID ?? string.Empty, PoolName = p.POOL_NAME ?? p.POOL_ID ?? string.Empty, Status = p.POOL_STATUS ?? "UNKNOWN", AreaAcres = 0, Formation = p.STRAT_UNIT_ID ?? string.Empty, }).ToList());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting reservoir pools for field {FieldId}", fieldId);
                throw;
            }
        }
#endregion
    }
}
