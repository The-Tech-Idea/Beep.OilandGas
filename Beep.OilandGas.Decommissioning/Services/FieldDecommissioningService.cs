using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Decommissioning.Services
{
    /// <summary>
    /// Field-scoped decommissioning service.
    /// Implements IFieldDecommissioningService using PPDMGenericRepository + metadata-based
    /// entity resolution. All PPDM CRUD follows the canonical AppFilter pattern; no raw SQL.
    ///
    /// PPDM 3.9 table coverage:
    ///   WELL_ABANDONMENT  — abandonment records per well
    ///   WELL              — parent well (FIELD_ID join)
    ///   FACILITY          — facility parent (PRIMARY_FIELD_ID)
    ///   PROJECT           — env. restoration + cost tracking (PROJECT_TYPE discriminator)
    ///   PROJECT_STEP      — activity steps within a decommissioning project
    /// </summary>
    public partial class FieldDecommissioningService : IFieldDecommissioningService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly string _connectionName;
        private readonly ILogger<FieldDecommissioningService>? _logger;

        public FieldDecommissioningService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            string connectionName = "PPDM39",
            ILogger<FieldDecommissioningService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        // ---------------------------------------------------------------------------
        // WELL ABANDONMENT
        // ---------------------------------------------------------------------------

        public async Task<List<WellAbandonmentResponse>> GetAbandonedWellsForFieldAsync(
            string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId), "Field ID is required");

            _logger?.LogInformation("Getting abandoned wells for field {FieldId}", fieldId);
            try
            {
                // Step 1: resolve WELL_ABANDONMENT metadata
                var abanMeta = await _metadata.GetTableMetadataAsync("WELL_ABANDONMENT");
                if (abanMeta == null)
                {
                    _logger?.LogWarning("WELL_ABANDONMENT table metadata not found");
                    return new List<WellAbandonmentResponse>();
                }

                var abanType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{abanMeta.EntityTypeName}");
                if (abanType == null)
                {
                    _logger?.LogWarning("Entity type not found for WELL_ABANDONMENT: {Name}", abanMeta.EntityTypeName);
                    return new List<WellAbandonmentResponse>();
                }

                // Step 2: get WELL IDs for the field
                var wellIds = await GetWellIdsForFieldAsync(fieldId);
                if (!wellIds.Any())
                    return new List<WellAbandonmentResponse>();

                // Step 3: query WELL_ABANDONMENT for those wells
                var abanRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    abanType, _connectionName, "WELL_ABANDONMENT");

                var filters = wellIds
                    .Select(id => new AppFilter
                    {
                        FieldName = "WELL_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_ABANDONMENT", id),
                        Operator = "="
                    })
                    .ToList();

                if (additionalFilters != null)
                    filters.AddRange(additionalFilters);

                var results = await abanRepo.GetAsync(filters);

                var dtos = _mappingService.ConvertPPDMModelListToDTOListRuntime(
                    results, typeof(WellAbandonmentResponse), abanType);
                return dtos.Cast<WellAbandonmentResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting abandoned wells for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<WellAbandonmentResponse> AbandonWellForFieldAsync(
            string fieldId, string wellId,
            WellAbandonmentRequest abandonmentData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));

            _logger?.LogInformation("Recording abandonment for well {WellId} in field {FieldId}", wellId, fieldId);
            try
            {
                // Validate well belongs to the field
                await ValidateWellBelongsToFieldAsync(fieldId, wellId);

                var meta = await GetTableMetaAndTypeAsync("WELL_ABANDONMENT");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "WELL_ABANDONMENT");

                var entity = _mappingService.ConvertDTOToPPDMModelRuntime(
                    abandonmentData, typeof(WellAbandonmentRequest), meta.EntityType);

                SetPropertyViaReflection(entity, meta.EntityType, "WELL_ID",
                    _defaults.FormatIdForTable("WELL_ABANDONMENT", wellId));
                SetPropertyViaReflection(entity, meta.EntityType, "FIELD_ID",
                    _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId));

                var inserted = await repo.InsertAsync(entity, userId);

                _logger?.LogInformation("Abandonment recorded for well {WellId} in field {FieldId}", wellId, fieldId);

                return (WellAbandonmentResponse)_mappingService.ConvertPPDMModelToDTORuntime(
                    inserted, typeof(WellAbandonmentResponse), meta.EntityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error abandoning well {WellId} in field {FieldId}", wellId, fieldId);
                throw;
            }
        }

        public async Task<WellAbandonmentResponse?> GetWellAbandonmentForFieldAsync(
            string fieldId, string abandonmentId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            try
            {
                var meta = await GetTableMetaAndTypeAsync("WELL_ABANDONMENT");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "WELL_ABANDONMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "ABANDONMENT_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_ABANDONMENT", abandonmentId),
                        Operator = "="
                    },
                    new AppFilter
                    {
                        FieldName = "FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("WELL_ABANDONMENT", fieldId),
                        Operator = "="
                    }
                };

                var results = await repo.GetAsync(filters);
                var first = results.FirstOrDefault();
                if (first == null)
                    return null;

                return (WellAbandonmentResponse?)_mappingService.ConvertPPDMModelToDTORuntime(
                    first, typeof(WellAbandonmentResponse), meta.EntityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting abandonment {Id} for field {FieldId}", abandonmentId, fieldId);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // FACILITY DECOMMISSIONING
        // ---------------------------------------------------------------------------

        public async Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesForFieldAsync(
            string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            _logger?.LogInformation("Getting decommissioned facilities for field {FieldId}", fieldId);
            try
            {
                var meta = await GetTableMetaAndTypeAsync("FACILITY");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "FACILITY");

                var filters = new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "PRIMARY_FIELD_ID",
                        FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId),
                        Operator = "="
                    },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                if (additionalFilters != null) filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                // Filter to decommissioned (ABANDONED_DATE populated)
                var decommissioned = results
                    .Where(f => GetPropertyValue(f, "ABANDONED_DATE") != null)
                    .ToList();

                var dtos = _mappingService.ConvertPPDMModelListToDTOListRuntime(
                    decommissioned, typeof(FacilityDecommissioningResponse), meta.EntityType);
                return dtos.Cast<FacilityDecommissioningResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting decommissioned facilities for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<FacilityDecommissioningResponse> DecommissionFacilityForFieldAsync(
            string fieldId, string facilityId,
            FacilityDecommissioningRequest decommissionData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (string.IsNullOrWhiteSpace(facilityId))
                throw new ArgumentNullException(nameof(facilityId));

            _logger?.LogInformation("Decommissioning facility {FacilityId} in field {FieldId}", facilityId, fieldId);
            try
            {
                var meta = await GetTableMetaAndTypeAsync("FACILITY");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "FACILITY");

                // Validate facility belongs to field
                var existing = await repo.GetByIdAsync(
                    _defaults.FormatIdForTable("FACILITY", facilityId));
                if (existing == null)
                    throw new OperationCanceledException($"Facility {facilityId} not found");

                var facilityFieldId = GetPropertyValue(existing, "PRIMARY_FIELD_ID")?.ToString();
                if (facilityFieldId != _defaults.FormatIdForTable("FACILITY", fieldId))
                    throw new OperationCanceledException($"Facility {facilityId} does not belong to field {fieldId}");

                // Set ABANDONED_DATE
                SetPropertyViaReflection(existing, meta.EntityType, "ABANDONED_DATE",
                    decommissionData.DecommissioningStartDate ?? DateTime.UtcNow);

                var updated = await repo.UpdateAsync(existing, userId);

                _logger?.LogInformation("Facility {FacilityId} decommissioned in field {FieldId}", facilityId, fieldId);

                return (FacilityDecommissioningResponse)_mappingService.ConvertPPDMModelToDTORuntime(
                    updated, typeof(FacilityDecommissioningResponse), meta.EntityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error decommissioning facility {FacilityId} in field {FieldId}", facilityId, fieldId);
                throw;
            }
        }

        public async Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningForFieldAsync(
            string fieldId, string decommissioningId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            try
            {
                var meta = await GetTableMetaAndTypeAsync("FACILITY");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "FACILITY");

                var existing = await repo.GetByIdAsync(
                    _defaults.FormatIdForTable("FACILITY", decommissioningId));
                if (existing == null)
                    return null;

                var facilityFieldId = GetPropertyValue(existing, "PRIMARY_FIELD_ID")?.ToString();
                if (facilityFieldId != _defaults.FormatIdForTable("FACILITY", fieldId))
                    return null;

                return (FacilityDecommissioningResponse?)_mappingService.ConvertPPDMModelToDTORuntime(
                    existing, typeof(FacilityDecommissioningResponse), meta.EntityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting facility decommissioning {Id} for field {FieldId}", decommissioningId, fieldId);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // ENVIRONMENTAL RESTORATION  (PROJECT with PROJECT_TYPE = 'ENV_RESTORATION')
        // ---------------------------------------------------------------------------

        public async Task<List<EnvironmentalRestorationResponse>> GetEnvironmentalRestorationsForFieldAsync(
            string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            try
            {
                var meta = await GetTableMetaAndTypeAsync("PROJECT");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "PROJECT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("PROJECT", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "PROJECT_TYPE", FilterValue = "ENV_RESTORATION", Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                if (additionalFilters != null) filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                var dtos = _mappingService.ConvertPPDMModelListToDTOListRuntime(
                    results, typeof(EnvironmentalRestorationResponse), meta.EntityType);
                return dtos.Cast<EnvironmentalRestorationResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting env. restorations for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<EnvironmentalRestorationResponse> CreateEnvironmentalRestorationForFieldAsync(
            string fieldId, EnvironmentalRestorationRequest restorationData, string userId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            _logger?.LogInformation("Creating environmental restoration project for field {FieldId}", fieldId);
            try
            {
                var meta = await GetTableMetaAndTypeAsync("PROJECT");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "PROJECT");

                var entity = _mappingService.ConvertDTOToPPDMModelRuntime(
                    restorationData, typeof(EnvironmentalRestorationRequest), meta.EntityType);

                SetPropertyViaReflection(entity, meta.EntityType, "FIELD_ID",
                    _defaults.FormatIdForTable("PROJECT", fieldId));
                SetPropertyViaReflection(entity, meta.EntityType, "PROJECT_TYPE", "ENV_RESTORATION");
                SetPropertyViaReflection(entity, meta.EntityType, "PROJECT_STATUS", "PLANNED");
                SetPropertyViaReflection(entity, meta.EntityType, "ACTIVE_IND", "Y");

                var inserted = await repo.InsertAsync(entity, userId);

                _logger?.LogInformation("Environmental restoration project created for field {FieldId}", fieldId);

                return (EnvironmentalRestorationResponse)_mappingService.ConvertPPDMModelToDTORuntime(
                    inserted, typeof(EnvironmentalRestorationResponse), meta.EntityType);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating env. restoration for field {FieldId}", fieldId);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // DECOMMISSIONING COSTS  (PROJECT with PROJECT_TYPE = 'DECOM_COST')
        // ---------------------------------------------------------------------------

        public async Task<List<DecommissioningCostResponse>> GetDecommissioningCostsForFieldAsync(
            string fieldId, List<AppFilter>? additionalFilters = null)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            try
            {
                var meta = await GetTableMetaAndTypeAsync("PROJECT");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    meta.EntityType, _connectionName, "PROJECT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("PROJECT", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "PROJECT_TYPE", FilterValue = "DECOM_COST", Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                };
                if (additionalFilters != null) filters.AddRange(additionalFilters);

                var results = await repo.GetAsync(filters);

                var dtos = _mappingService.ConvertPPDMModelListToDTOListRuntime(
                    results, typeof(DecommissioningCostResponse), meta.EntityType);
                return dtos.Cast<DecommissioningCostResponse>().ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting decommissioning costs for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<DecommissioningCostEstimateResponse> EstimateCostsForFieldAsync(string fieldId)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));

            _logger?.LogInformation("Estimating decommissioning costs for field {FieldId}", fieldId);
            try
            {
                var wellMeta = await GetTableMetaAndTypeAsync("WELL");
                var wellRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    wellMeta.EntityType, _connectionName, "WELL");

                var facilityMeta = await GetTableMetaAndTypeAsync("FACILITY");
                var facilityRepo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    facilityMeta.EntityType, _connectionName, "FACILITY");

                var wells = await wellRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "FIELD_ID", FilterValue = _defaults.FormatIdForTable("WELL", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                });

                var facilities = await facilityRepo.GetAsync(new List<AppFilter>
                {
                    new AppFilter { FieldName = "PRIMARY_FIELD_ID", FilterValue = _defaults.FormatIdForTable("FACILITY", fieldId), Operator = "=" },
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = "Y", Operator = "=" }
                });

                const decimal WellAbandonmentRate    = 250_000m;
                const decimal FacilityDecomRate      = 500_000m;
                const decimal SiteRestorationRate    = 50_000m;
                const decimal EnvironmentalBaseRate  = 100_000m;
                const decimal ContingencyPercent     = 0.20m;

                int wellCount     = wells.Count();
                int facilityCount = facilities.Count();

                decimal wellAbandonmentCost     = wellCount     * WellAbandonmentRate;
                decimal facilityCost            = facilityCount * FacilityDecomRate;
                decimal restorationCost         = wellCount     * SiteRestorationRate;
                decimal remediationCost         = EnvironmentalBaseRate * Math.Max(1, (wellCount + facilityCount) / 5);
                decimal subtotal                = wellAbandonmentCost + facilityCost + restorationCost + remediationCost;
                decimal contingency             = subtotal * ContingencyPercent;

                _logger?.LogInformation(
                    "Cost estimate for field {FieldId}: {WellCount} wells, {FacilityCount} facilities, total={Total:C}",
                    fieldId, wellCount, facilityCount, subtotal + contingency);

                return new DecommissioningCostEstimateResponse
                {
                    FieldId                              = fieldId,
                    EstimateDate                         = DateTime.UtcNow,
                    EstimatedWellAbandonmentCost         = wellAbandonmentCost,
                    EstimatedFacilityDecommissioningCost = facilityCost,
                    EstimatedSiteRestorationCost         = restorationCost,
                    EstimatedRegulatoryCost              = remediationCost,
                    EstimatedTotalCost                   = subtotal + contingency,
                    EstimatedWellsToAbandon              = wellCount,
                    EstimatedFacilitiesToDecommission    = facilityCount,
                    EstimationMethod                     = "PARAMETRIC",
                    Notes                                = $"Parametric estimate: {wellCount} wells @ {WellAbandonmentRate:C}/well, {facilityCount} facilities @ {FacilityDecomRate:C}/facility. 20% contingency applied."
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error estimating costs for field {FieldId}", fieldId);
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns all UWI / WELL_ID values for wells that belong to a field.
        /// Uses metadata-based entity type resolution and reflection to read WELL_ID.
        /// </summary>
        private async Task<List<string>> GetWellIdsForFieldAsync(string fieldId)
        {
            var wellMeta = await GetTableMetaAndTypeAsync("WELL");
            var wellRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                wellMeta.EntityType, _connectionName, "WELL");

            var wells = await wellRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter
                {
                    FieldName  = "FIELD_ID",
                    FilterValue = _defaults.FormatIdForTable("WELL", fieldId),
                    Operator   = "="
                }
            });

            var ids = new List<string>();
            foreach (var well in wells)
            {
                // WELL_ID (alias UWI) column
                var val = GetPropertyValue(well, "UWI") ?? GetPropertyValue(well, "WELL_ID");
                if (val != null)
                    ids.Add(val.ToString()!);
            }
            return ids;
        }

        /// <summary>
        /// Validates that wellId belongs to the given fieldId. Throws OperationCanceledException on mismatch.
        /// </summary>
        private async Task ValidateWellBelongsToFieldAsync(string fieldId, string wellId)
        {
            var wellMeta = await GetTableMetaAndTypeAsync("WELL");
            var wellRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                wellMeta.EntityType, _connectionName, "WELL");

            var well = await wellRepo.GetByIdAsync(_defaults.FormatIdForTable("WELL", wellId));
            if (well == null)
                throw new OperationCanceledException($"Well {wellId} not found");

            var wellFieldId = GetPropertyValue(well, "FIELD_ID")?.ToString();
            if (wellFieldId != _defaults.FormatIdForTable("WELL", fieldId))
                throw new OperationCanceledException($"Well {wellId} does not belong to field {fieldId}");
        }

        /// <summary>Resolves table metadata and entity Type in one call.</summary>
        private async Task<(PPDMTableMetadata Meta, Type EntityType)> GetTableMetaAndTypeAsync(string tableName)
        {
            var meta = await _metadata.GetTableMetadataAsync(tableName);
            if (meta == null)
                throw new InvalidOperationException($"{tableName} table metadata not found");

            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{meta.EntityTypeName}");
            if (entityType == null)
                throw new InvalidOperationException($"Entity type not found for {tableName}: {meta.EntityTypeName}");

            return (meta, entityType);
        }

        /// <summary>Sets a property on an entity via reflection; silently skips non-existent / read-only properties.</summary>
        private static void SetPropertyViaReflection(object entity, Type entityType, string propertyName, object? value)
        {
            var prop = entityType.GetProperty(propertyName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop?.CanWrite == true)
                prop.SetValue(entity, value);
        }

        /// <summary>Gets a property value from an entity via reflection.</summary>
        private static object? GetPropertyValue(object entity, string propertyName)
        {
            return entity.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                ?.GetValue(entity);
        }
    }
}
