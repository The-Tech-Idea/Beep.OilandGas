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
using Beep.OilandGas.LifeCycle.Services.WorkOrder;
using Beep.OilandGas.LifeCycle.Services.Integration;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.PipelineManagement
{
    /// <summary>
    /// Comprehensive service for Pipeline management including creation, operations, maintenance, inspection, and integrity
    /// </summary>
    public class PipelineManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly WorkOrderManagementService? _workOrderService;
        private readonly DataFlowService? _dataFlowService;
        private readonly string _connectionName;
        private readonly ILogger<PipelineManagementService>? _logger;

        public PipelineManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            WorkOrderManagementService? workOrderService = null,
            DataFlowService? dataFlowService = null,
            string connectionName = "PPDM39",
            ILogger<PipelineManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _workOrderService = workOrderService;
            _dataFlowService = dataFlowService;
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        #region Pipeline Creation

        public async Task<PipelineResponse> CreatePipelineAsync(PipelineCreationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null)
                {
                    throw new InvalidOperationException("PIPELINE table metadata not found");
                }

                var entityType = typeof(PIPELINE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PIPELINE", null);

                var pipeline = new PIPELINE();
                pipeline.PIPELINE_NAME = request.PipelineName ?? string.Empty;
                pipeline.FIELD_ID = _defaults.FormatIdForTable("PIPELINE", request.FieldId ?? string.Empty);
                pipeline.PIPELINE_TYPE = request.PipelineType ?? string.Empty;
                if (request.Diameter.HasValue)
                    pipeline.DIAMETER = request.Diameter.Value;
                if (request.Length.HasValue)
                    pipeline.LENGTH = request.Length.Value;
                pipeline.MATERIAL = request.Material;
                pipeline.ACTIVE_IND = "Y";

                if (pipeline is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);
                var result = await repo.InsertAsync(pipeline, userId);
                var createdPipeline = result as PIPELINE ?? throw new InvalidOperationException("Failed to create pipeline");

                _logger?.LogInformation("Pipeline created: {PipelineId}, Name: {PipelineName}", createdPipeline.PIPELINE_ID, createdPipeline.PIPELINE_NAME);

                return new PipelineResponse
                {
                    PipelineId = createdPipeline.PIPELINE_ID,
                    PipelineName = createdPipeline.PIPELINE_NAME ?? string.Empty,
                    FieldId = createdPipeline.FIELD_ID ?? string.Empty,
                    Status = "ACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "PipelineType", createdPipeline.PIPELINE_TYPE ?? string.Empty },
                        { "Diameter", createdPipeline.DIAMETER },
                        { "Length", createdPipeline.LENGTH }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating pipeline: {PipelineName}", request.PipelineName);
                throw;
            }
        }

        public async Task<PipelineResponse?> GetPipelineAsync(string pipelineId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("PIPELINE");
                if (metadata == null) return null;

                var entityType = typeof(PIPELINE);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "PIPELINE", null);

                var formattedPipelineId = _defaults.FormatIdForTable("PIPELINE", pipelineId);
                var pipeline = await repo.GetByIdAsync(formattedPipelineId);
                if (pipeline is not PIPELINE pipelineEntity) return null;

                return new PipelineResponse
                {
                    PipelineId = pipelineEntity.PIPELINE_ID,
                    PipelineName = pipelineEntity.PIPELINE_NAME ?? string.Empty,
                    FieldId = pipelineEntity.FIELD_ID ?? string.Empty,
                    Status = pipelineEntity.ACTIVE_IND == "Y" ? "ACTIVE" : "INACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "PipelineType", pipelineEntity.PIPELINE_TYPE ?? string.Empty }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting pipeline: {PipelineId}", pipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Operations

        public async Task<bool> RecordPipelineOperationsAsync(PipelineOperationsRequest request, string userId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(request.PipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {request.PipelineId}");
                }

                var statusMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (statusMeta != null)
                {
                    var statusRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var statusRecord = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", request.PipelineId),
                        FACILITY_TYPE = "PIPELINE",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS = request.OperationType,
                        STATUS_TYPE = "OPERATION",
                        START_TIME = request.OperationDate,
                        REMARK = request.Description ?? string.Empty,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (statusRecord is IPPDMEntity se) _commonColumnHandler.PrepareForInsert(se, userId);
                    await statusRepo.InsertAsync(statusRecord, userId);
                }
                _logger?.LogInformation("Pipeline operations recorded for pipeline: {PipelineId}, Type: {OperationType}",
                    request.PipelineId, request.OperationType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording pipeline operations: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Maintenance

        public async Task<bool> CreatePipelineMaintenanceAsync(PipelineMaintenanceRequest request, string userId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(request.PipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {request.PipelineId}");
                }

                var maintMeta = await _metadata.GetTableMetadataAsync("FACILITY_MAINTAIN");
                if (maintMeta != null)
                {
                    var maintRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_MAINTAIN), _connectionName, "FACILITY_MAINTAIN", null);
                    var maintRecord = new FACILITY_MAINTAIN
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_MAINTAIN", request.PipelineId),
                        FACILITY_TYPE = "PIPELINE",
                        MAINTAIN_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        MAINTAIN_TYPE = request.MaintenanceType,
                        SCHEDULE_START_DATE = request.ScheduledDate,
                        ACTUAL_END_DATE = request.CompletedDate,
                        REMARK = request.Description ?? string.Empty,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (maintRecord is IPPDMEntity me) _commonColumnHandler.PrepareForInsert(me, userId);
                    await maintRepo.InsertAsync(maintRecord, userId);
                }
                _logger?.LogInformation("Pipeline maintenance created for pipeline: {PipelineId}, Type: {MaintenanceType}",
                    request.PipelineId, request.MaintenanceType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating pipeline maintenance: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Inspection

        public async Task<bool> CreatePipelineInspectionAsync(PipelineInspectionRequest request, string userId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(request.PipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {request.PipelineId}");
                }

                var inspMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (inspMeta != null)
                {
                    var inspRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var inspRecord = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", request.PipelineId),
                        FACILITY_TYPE = "PIPELINE",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS = request.Status ?? "COMPLETED",
                        STATUS_TYPE = "INSPECTION_" + request.InspectionType,
                        START_TIME = request.InspectionDate,
                        REMARK = string.IsNullOrEmpty(request.Findings) ? request.Inspector ?? string.Empty : $"{request.Inspector ?? string.Empty}: {request.Findings}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (inspRecord is IPPDMEntity ie) _commonColumnHandler.PrepareForInsert(ie, userId);
                    await inspRepo.InsertAsync(inspRecord, userId);
                }
                _logger?.LogInformation("Pipeline inspection created for pipeline: {PipelineId}, Type: {InspectionType}",
                    request.PipelineId, request.InspectionType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating pipeline inspection: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Integrity

        public async Task<bool> AssessPipelineIntegrityAsync(PipelineIntegrityRequest request, string userId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(request.PipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {request.PipelineId}");
                }

                var integrityMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (integrityMeta != null)
                {
                    var integrityRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var integrityRecord = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", request.PipelineId),
                        FACILITY_TYPE = "PIPELINE",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS = request.AssessmentResult ?? request.AssessmentType,
                        STATUS_TYPE = "INTEGRITY",
                        START_TIME = request.AssessmentDate,
                        REMARK = request.AssessmentType,
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (integrityRecord is IPPDMEntity ire) _commonColumnHandler.PrepareForInsert(ire, userId);
                    await integrityRepo.InsertAsync(integrityRecord, userId);
                }
                _logger?.LogInformation("Pipeline integrity assessed for pipeline: {PipelineId}, Type: {AssessmentType}",
                    request.PipelineId, request.AssessmentType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing pipeline integrity: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Flow Management

        public async Task<bool> RecordPipelineFlowAsync(PipelineFlowRequest request, string userId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(request.PipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {request.PipelineId}");
                }

                var flowMeta = await _metadata.GetTableMetadataAsync("FACILITY_STATUS");
                if (flowMeta != null)
                {
                    var flowRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        typeof(FACILITY_STATUS), _connectionName, "FACILITY_STATUS", null);
                    var flowRecord = new FACILITY_STATUS
                    {
                        FACILITY_ID = _defaults.FormatIdForTable("FACILITY_STATUS", request.PipelineId),
                        FACILITY_TYPE = "PIPELINE",
                        STATUS_ID = Guid.NewGuid().ToString("N").Substring(0, 16),
                        STATUS = request.ProductType ?? "FLOWING",
                        STATUS_TYPE = "FLOW",
                        START_TIME = request.FlowDate,
                        PERCENT_CAPABILITY = request.FlowRate,
                        REMARK = $"FlowRate={request.FlowRate}, Pressure={request.Pressure}, Product={request.ProductType}",
                        ACTIVE_IND = "Y",
                        PPDM_GUID = Guid.NewGuid().ToString()
                    };
                    if (flowRecord is IPPDMEntity fre) _commonColumnHandler.PrepareForInsert(fre, userId);
                    await flowRepo.InsertAsync(flowRecord, userId);
                }
                _logger?.LogInformation("Pipeline flow recorded for pipeline: {PipelineId}, FlowRate: {FlowRate}",
                    request.PipelineId, request.FlowRate);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording pipeline flow: {PipelineId}", request.PipelineId);
                throw;
            }
        }

        public async Task<PipelineCapacityResponse> GetPipelineCapacityAsync(string pipelineId)
        {
            try
            {
                var pipeline = await GetPipelineAsync(pipelineId);
                if (pipeline == null)
                {
                    throw new InvalidOperationException($"Pipeline not found: {pipelineId}");
                }

                return new PipelineCapacityResponse
                {
                    PipelineId = pipelineId,
                    MaximumCapacity = 0, // Would calculate from pipeline properties
                    CurrentUtilization = 0, // Would query current flow data
                    AvailableCapacity = 0, // Would calculate difference
                    ReportDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting pipeline capacity: {PipelineId}", pipelineId);
                throw;
            }
        }

        #endregion

        #region Pipeline Work Order Operations

        /// <summary>
        /// Creates a maintenance work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineMaintenanceWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "MAINTENANCE",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a repair work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineRepairWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "REPAIR",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates an inspection work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineInspectionWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "INSPECTION",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates an integrity test work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineIntegrityTestWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "INTEGRITY_TEST",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a cleaning work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineCleaningWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "CLEANING",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a modification work order for a pipeline
        /// </summary>
        public async Task<WorkOrderResponse> CreatePipelineModificationWorkOrderAsync(
            string pipelineId,
            string workOrderNumber,
            string? fieldId = null,
            string? propertyId = null,
            string? description = null,
            DateTime? requestDate = null,
            DateTime? dueDate = null,
            decimal? estimatedCost = null,
            string? instructions = null,
            string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "MODIFICATION",
                EntityType = "PIPELINE",
                EntityId = pipelineId,
                FieldId = fieldId,
                PropertyId = propertyId,
                Instructions = instructions,
                RequestDate = requestDate,
                DueDate = dueDate,
                EstimatedCost = estimatedCost,
                Description = description
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        #endregion

        #region Pipeline Analysis (using DataFlowService)

        /// <summary>
        /// Runs pipeline analysis for a pipeline using DataFlowService
        /// </summary>
        public async Task<PIPELINE_ANALYSIS_RESULT> AnalyzePipelineAsync(
            string pipelineId,
            string userId = "system",
            string pipelineType = "GAS",
            string analysisType = "CAPACITY",
            PipelineAnalysisOptions? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");

            }

            try
            {
                _logger?.LogInformation("Running pipeline analysis for pipeline: {PipelineId}, PipelineType: {PipelineType}", 
                    pipelineId, pipelineType);
                return await _dataFlowService.RunPipelineAnalysisAsync(pipelineId, userId, pipelineType, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running pipeline analysis for pipeline: {PipelineId}", pipelineId);
                throw;
            }
        }

        #endregion
    }
}

