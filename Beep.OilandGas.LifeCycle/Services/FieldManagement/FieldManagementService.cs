using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.LifeCycle.Services.FieldLifecycle;
using Beep.OilandGas.LifeCycle.Services.Integration;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Services.FieldManagement
{
    /// <summary>
    /// Comprehensive service for Field management including creation, planning, operations, and performance tracking
    /// </summary>
    public class FieldManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly FieldLifecycleService _lifecycleService;
        private readonly DataFlowService? _dataFlowService;
        private readonly string _connectionName;
        private readonly ILogger<FieldManagementService>? _logger;

        public FieldManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            FieldLifecycleService lifecycleService,
            DataFlowService? dataFlowService = null,
            string connectionName = "PPDM39",
            ILogger<FieldManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
            _dataFlowService = dataFlowService;
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        #region Field Creation

        /// <summary>
        /// Creates a new field with workflow support
        /// </summary>
        public async Task<FieldResponse> CreateFieldAsync(FieldCreationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FIELD table metadata not found");
                }

                var entityType = typeof(FIELD);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD", null);

                // Create FIELD entity
                var field = new FIELD();
                field.FIELD_NAME = request.FieldName ?? string.Empty;
                field.FIELD_TYPE = request.FieldType ?? string.Empty;
                field.AREA_ID = request.AreaId ?? string.Empty;
                field.ACTIVE_IND = "Y";

                // Set common columns
                if (field is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);

                // Insert field
                var result = await repo.InsertAsync(field, userId);
                var createdField = result as FIELD ?? throw new InvalidOperationException("Failed to create field");

                // Initialize field phase to EXPLORATION
                await _lifecycleService.StartFieldPhaseAsync(createdField.FIELD_ID, "EXPLORATION", userId);

                _logger?.LogInformation("Field created: {FieldId}, Name: {FieldName}", createdField.FIELD_ID, createdField.FIELD_NAME);

                return new FieldResponse
                {
                    FieldId = createdField.FIELD_ID,
                    FieldName = createdField.FIELD_NAME ?? string.Empty,
                    CurrentPhase = "EXPLORATION",
                    Status = "ACTIVE",
                    FieldType = createdField.FIELD_TYPE ?? string.Empty,
                    AreaId = createdField.AREA_ID ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating field: {FieldName}", request.FieldName);
                throw;
            }
        }

        /// <summary>
        /// Gets field by ID
        /// </summary>
        public async Task<FieldResponse?> GetFieldAsync(string fieldId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD");
                if (metadata == null)
                {
                    return null;
                }

                var entityType = typeof(FIELD);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD", null);

                var formattedFieldId = _defaults.FormatIdForTable("FIELD", fieldId);
                var field = await repo.GetByIdAsync(formattedFieldId);
                if (field is not FIELD fieldEntity)
                {
                    return null;
                }

                var currentPhase = await _lifecycleService.GetCurrentFieldPhaseAsync(fieldId);

                return new FieldResponse
                {
                    FieldId = fieldEntity.FIELD_ID,
                    FieldName = fieldEntity.FIELD_NAME ?? string.Empty,
                    CurrentPhase = currentPhase,
                    Status = fieldEntity.ACTIVE_IND == "Y" ? "ACTIVE" : "INACTIVE",
                    FieldType = fieldEntity.FIELD_TYPE ?? string.Empty,
                    AreaId = fieldEntity.AREA_ID ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting field: {FieldId}", fieldId);
                throw;
            }
        }

        #endregion

        #region Field Planning

        /// <summary>
        /// Creates a field planning record
        /// </summary>
        public async Task<bool> CreateFieldPlanningAsync(FieldPlanningRequest request, string userId)
        {
            try
            {
                // Validate field exists
                var field = await GetFieldAsync(request.FieldId);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field not found: {request.FieldId}");
                }

                var versionMeta = await _metadata.GetTableMetadataAsync("FIELD_VERSION");
                if (versionMeta != null)
                {
                    var versionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FIELD_VERSION), _connectionName, "FIELD_VERSION", null);
                    var version = new FIELD_VERSION
                    {
                        FIELD_ID = _defaults.FormatIdForTable("FIELD_VERSION", request.FieldId),
                        SOURCE = "LIFECYCLE",
                        EFFECTIVE_DATE = request.TargetStartDate ?? DateTime.UtcNow,
                        FIELD_TYPE = "PLANNING_" + request.PlanningType,
                        REMARK = request.Description ?? string.Empty,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (version is IPPDMEntity ve) _commonColumnHandler.PrepareForInsert(ve, userId);
                    await versionRepo.InsertAsync(version, userId);
                }
                _logger?.LogInformation("Field planning created for field: {FieldId}, Type: {PlanningType}",
                    request.FieldId, request.PlanningType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating field planning: {FieldId}", request.FieldId);
                throw;
            }
        }

        #endregion

        #region Field Operations

        /// <summary>
        /// Records field operations
        /// </summary>
        public async Task<bool> RecordFieldOperationsAsync(FieldOperationsRequest request, string userId)
        {
            try
            {
                // Validate field exists
                var field = await GetFieldAsync(request.FieldId);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field not found: {request.FieldId}");
                }

                var verMeta = await _metadata.GetTableMetadataAsync("FIELD_VERSION");
                if (verMeta != null)
                {
                    var verRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FIELD_VERSION), _connectionName, "FIELD_VERSION", null);
                    var version = new FIELD_VERSION
                    {
                        FIELD_ID = _defaults.FormatIdForTable("FIELD_VERSION", request.FieldId),
                        SOURCE = "LIFECYCLE",
                        EFFECTIVE_DATE = request.OperationDate,
                        FIELD_TYPE = request.OperationType,
                        REMARK = request.Description ?? string.Empty,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (version is IPPDMEntity ve) _commonColumnHandler.PrepareForInsert(ve, userId);
                    await verRepo.InsertAsync(version, userId);
                }
                _logger?.LogInformation("Field operations recorded for field: {FieldId}, Type: {OperationType}",
                    request.FieldId, request.OperationType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording field operations: {FieldId}", request.FieldId);
                throw;
            }
        }

        #endregion

        #region Field Configuration

        /// <summary>
        /// Updates field configuration
        /// </summary>
        public async Task<bool> UpdateFieldConfigurationAsync(FieldConfigurationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FIELD");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FIELD table metadata not found");
                }

                var entityType = typeof(FIELD);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FIELD", null);

                var formattedFieldId = _defaults.FormatIdForTable("FIELD", request.FieldId);
                var field = await repo.GetByIdAsync(formattedFieldId);
                if (field is not FIELD fieldEntity)
                {
                    throw new InvalidOperationException($"Field not found: {request.FieldId}");
                }

                // Update configuration properties
                foreach (var config in request.Configuration)
                {
                    var prop = entityType.GetProperty(config.Key, 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(fieldEntity, config.Value);
                    }
                }

                // Set common columns
                if (fieldEntity is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForUpdate(entity, userId);

                await repo.UpdateAsync(fieldEntity, userId);

                _logger?.LogInformation("Field configuration updated for field: {FieldId}", request.FieldId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating field configuration: {FieldId}", request.FieldId);
                throw;
            }
        }

        #endregion

        #region Field Performance

        /// <summary>
        /// Gets field performance metrics
        /// </summary>
        public async Task<FieldPerformanceResponse> GetFieldPerformanceAsync(FieldPerformanceRequest request)
        {
            try
            {
                // Validate field exists
                var field = await GetFieldAsync(request.FieldId);
                if (field == null)
                {
                    throw new InvalidOperationException($"Field not found: {request.FieldId}");
                }

                var metrics = new Dictionary<string, decimal>();
                var additionalData = new Dictionary<string, object>();

                // Query FIELD_VERSION for operations count
                var versionMeta = await _metadata.GetTableMetadataAsync("FIELD_VERSION");
                if (versionMeta != null)
                {
                    var versionRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FIELD_VERSION), _connectionName, "FIELD_VERSION", null);
                    var versionFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("FIELD_VERSION", request.FieldId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    var versions = await versionRepo.GetAsync(versionFilters);
                    var vList = versions?.ToList() ?? new List<object>();
                    metrics["TotalOperationsCount"] = vList.Count;
                    metrics["PlanningOperationsCount"] = vList.OfType<FIELD_VERSION>()
                        .Count(v => v.FIELD_TYPE != null &&
                            v.FIELD_TYPE.StartsWith("PLANNING_", StringComparison.OrdinalIgnoreCase));
                }

                // Query WELL table for well count
                var wellMeta = await _metadata.GetTableMetadataAsync("WELL");
                if (wellMeta != null)
                {
                    var wellRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(WELL), _connectionName, "WELL", null);
                    var wellFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "FIELD_ID", Operator = "=", FilterValue = _defaults.FormatIdForTable("WELL", request.FieldId) },
                        new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                    };
                    var wells = await wellRepo.GetAsync(wellFilters);
                    metrics["ActiveWellCount"] = wells?.Count() ?? 0;
                }

                return new FieldPerformanceResponse
                {
                    FieldId = request.FieldId,
                    ReportDate = DateTime.UtcNow,
                    Metrics = metrics,
                    AdditionalData = additionalData
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting field performance: {FieldId}", request.FieldId);
                throw;
            }
        }

        #endregion

        #region Field Analysis (using DataFlowService)

        /// <summary>
        /// Runs DCA (Decline Curve Analysis) for a field using DataFlowService
        /// </summary>
        public async Task<DCAResult> RunFieldDCAAsync(
            string fieldId,
            string userId,
            DcaAnalysisOptions? options = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                 var calculationType = options?.DeclineModel ?? "Hyperbolic";
                 var additionalParameters = options; 

                _logger?.LogInformation("Running DCA for field: {FieldId}, Type: {Type}", fieldId, calculationType);
                return await _dataFlowService.RunDCAAsync(fieldId: fieldId, userId: userId, calculationType: calculationType, additionalParameters: additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running DCA for field: {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Runs DCA (Decline Curve Analysis) for a pool using DataFlowService
        /// </summary>
        public async Task<DCAResult> RunPoolDCAAsync(
            string poolId,
            string userId,
            DcaAnalysisOptions? options = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                 var calculationType = options?.DeclineModel ?? "Hyperbolic";
                var additionalParameters = options;

                _logger?.LogInformation("Running DCA for pool: {PoolId}, Type: {Type}", poolId, calculationType);
                return await _dataFlowService.RunDCAAsync(poolId: poolId, userId: userId, calculationType: calculationType, additionalParameters: additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running DCA for pool: {PoolId}", poolId);
                throw;
            }
        }

        #endregion
    }
}

