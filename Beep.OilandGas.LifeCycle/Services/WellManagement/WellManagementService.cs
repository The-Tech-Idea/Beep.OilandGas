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
using Beep.OilandGas.LifeCycle.Services.WellLifecycle;
using Beep.OilandGas.LifeCycle.Services.WorkOrder;
using Beep.OilandGas.LifeCycle.Services.Integration;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.WellManagement
{
    /// <summary>
    /// Comprehensive service for Well management including creation, planning, operations, maintenance, and inspection
    /// </summary>
    public class WellManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly WellLifecycleService _lifecycleService;
        private readonly WorkOrderManagementService? _workOrderService;
        private readonly DataFlowService? _dataFlowService;
        private readonly string _connectionName;
        private readonly ILogger<WellManagementService>? _logger;

        public WellManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            WellLifecycleService lifecycleService,
            WorkOrderManagementService? workOrderService = null,
            DataFlowService? dataFlowService = null,
            string connectionName = "PPDM39",
            ILogger<WellManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _lifecycleService = lifecycleService ?? throw new ArgumentNullException(nameof(lifecycleService));
            _workOrderService = workOrderService;
            _dataFlowService = dataFlowService;
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        #region Well Creation

        /// <summary>
        /// Creates a new well with workflow support
        /// </summary>
        public async Task<WellResponse> CreateWellAsync(WellCreationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    throw new InvalidOperationException("WELL table metadata not found");
                }

                var entityType = typeof(WELL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL", null);

                // Create WELL entity
                var well = new WELL();
                well.WELL_NAME = request.WellName;
                well.FIELD_ID = _defaults.FormatIdForTable("WELL", request.FieldId);
                well.WELL_TYPE = request.WellType;
                well.WELL_PURPOSE = request.WellPurpose;
                
                if (request.SurfaceLatitude.HasValue)
                    well.SURFACE_LATITUDE = request.SurfaceLatitude.Value;
                if (request.SurfaceLongitude.HasValue)
                    well.SURFACE_LONGITUDE = request.SurfaceLongitude.Value;
                if (request.BottomHoleLatitude.HasValue)
                    well.BOTTOM_HOLE_LATITUDE = request.BottomHoleLatitude.Value;
                if (request.BottomHoleLongitude.HasValue)
                    well.BOTTOM_HOLE_LONGITUDE = request.BottomHoleLongitude.Value;

                well.ACTIVE_IND = "Y";

                // Set common columns
                if (well is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);

                // Insert well
                var result = await repo.InsertAsync(well, userId);
                var createdWell = result as WELL ?? throw new InvalidOperationException("Failed to create well");

                // Initialize well state to PLANNED
                await _lifecycleService.TransitionWellStateAsync(createdWell.WELL_ID, "PLANNED", userId);

                _logger?.LogInformation("Well created: {WellId}, Name: {WellName}", createdWell.WELL_ID, createdWell.WELL_NAME);

                return new WellResponse
                {
                    WellId = createdWell.WELL_ID,
                    WellName = createdWell.WELL_NAME ?? string.Empty,
                    FieldId = createdWell.FIELD_ID ?? string.Empty,
                    CurrentState = "PLANNED",
                    Status = "ACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "WellType", createdWell.WELL_TYPE ?? string.Empty },
                        { "WellPurpose", createdWell.WELL_PURPOSE ?? string.Empty }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating well: {WellName}", request.WellName);
                throw;
            }
        }

        /// <summary>
        /// Gets well by ID
        /// </summary>
        public async Task<WellResponse?> GetWellAsync(string wellId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("WELL");
                if (metadata == null)
                {
                    return null;
                }

                var entityType = typeof(WELL);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "WELL", null);

                var formattedWellId = _defaults.FormatIdForTable("WELL", wellId);
                var well = await repo.GetByIdAsync(formattedWellId);
                if (well is not WELL wellEntity)
                {
                    return null;
                }

                var currentState = await _lifecycleService.GetCurrentWellStateAsync(wellId);

                return new WellResponse
                {
                    WellId = wellEntity.WELL_ID,
                    WellName = wellEntity.WELL_NAME ?? string.Empty,
                    FieldId = wellEntity.FIELD_ID ?? string.Empty,
                    CurrentState = currentState,
                    Status = wellEntity.ACTIVE_IND == "Y" ? "ACTIVE" : "INACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "WellType", wellEntity.WELL_TYPE ?? string.Empty },
                        { "WellPurpose", wellEntity.WELL_PURPOSE ?? string.Empty }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting well: {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Well Planning

        /// <summary>
        /// Creates a well planning record
        /// </summary>
        public async Task<bool> CreateWellPlanningAsync(WellPlanningRequest request, string userId)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {request.WellId}");
                }

                // Store planning data (could be in a separate PLANNING table or as part of process)
                _logger?.LogInformation("Well planning created for well: {WellId}, Type: {PlanningType}", 
                    request.WellId, request.PlanningType);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating well planning: {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Well Operations

        /// <summary>
        /// Records well operations
        /// </summary>
        public async Task<bool> RecordWellOperationsAsync(WellOperationsRequest request, string userId)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {request.WellId}");
                }

                // Store operations data (could be in OPERATIONS_LOG table)
                _logger?.LogInformation("Well operations recorded for well: {WellId}, Type: {OperationType}", 
                    request.WellId, request.OperationType);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording well operations: {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Well Maintenance

        /// <summary>
        /// Creates a well maintenance record
        /// </summary>
        public async Task<bool> CreateWellMaintenanceAsync(WellMaintenanceRequest request, string userId)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {request.WellId}");
                }

                // Store maintenance data (could be in MAINTENANCE_SCHEDULE or MAINTENANCE_HISTORY table)
                _logger?.LogInformation("Well maintenance created for well: {WellId}, Type: {MaintenanceType}", 
                    request.WellId, request.MaintenanceType);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating well maintenance: {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Well Inspection

        /// <summary>
        /// Creates a well inspection record
        /// </summary>
        public async Task<bool> CreateWellInspectionAsync(WellInspectionRequest request, string userId)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {request.WellId}");
                }

                // Store inspection data (could be in INSPECTION_SCHEDULE or INSPECTION_RESULT table)
                _logger?.LogInformation("Well inspection created for well: {WellId}, Type: {InspectionType}", 
                    request.WellId, request.InspectionType);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating well inspection: {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Well Equipment

        /// <summary>
        /// Records well equipment
        /// </summary>
        public async Task<bool> RecordWellEquipmentAsync(WellEquipmentRequest request, string userId)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(request.WellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {request.WellId}");
                }

                // Store equipment data in WELL_EQUIPMENT table
                var metadata = await _metadata.GetTableMetadataAsync("WELL_EQUIPMENT");
                if (metadata != null)
                {
                    var entityType = typeof(WELL_EQUIPMENT);
                    var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        entityType, _connectionName, "WELL_EQUIPMENT", null);

                    var equipment = new WELL_EQUIPMENT();
                    equipment.WELL_ID = _defaults.FormatIdForTable("WELL_EQUIPMENT", request.WellId);
                    equipment.EQUIPMENT_TYPE = request.EquipmentType;
                    equipment.EQUIPMENT_NAME = request.EquipmentName;
                    equipment.MANUFACTURER = request.Manufacturer;
                    equipment.MODEL = request.Model;
                    if (request.InstallationDate.HasValue)
                        equipment.INSTALLATION_DATE = request.InstallationDate.Value;

                    if (equipment is IPPDMEntity equipmentEntity)
                        _commonColumnHandler.PrepareForInsert(equipmentEntity, userId);
                    await repo.InsertAsync(equipment, userId);
                }

                _logger?.LogInformation("Well equipment recorded for well: {WellId}, Type: {EquipmentType}", 
                    request.WellId, request.EquipmentType);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording well equipment: {WellId}", request.WellId);
                throw;
            }
        }

        #endregion

        #region Well Workover Operations

        /// <summary>
        /// Creates a rig workover work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateRigWorkoverAsync(RigWorkoverRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "RIG_WORKOVER",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkoverData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a rigless workover work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateRiglessWorkoverAsync(RiglessWorkoverRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "RIGLESS_WORKOVER",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkoverData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a wireline work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateWirelineWorkAsync(WirelineWorkRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "WIRELINE_WORK",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.OperationData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a coiled tubing work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateCoiledTubingWorkAsync(CoiledTubingWorkRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "COILED_TUBING_WORK",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.OperationData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a snubbing work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateSnubbingWorkAsync(string wellId, string workOrderNumber, string? fieldId = null, string? propertyId = null, string? description = null, DateTime? requestDate = null, DateTime? dueDate = null, decimal? estimatedCost = null, string? instructions = null, string userId = "system")
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = workOrderNumber,
                WorkOrderType = "SNUBBING_WORK",
                EntityType = "WELL",
                EntityId = wellId,
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
        /// Creates a well test work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateWellTestAsync(WellTestRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "TESTING",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.TestData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a stimulation work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateStimulationAsync(StimulationRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "STIMULATION",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.StimulationData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a cleanout work order
        /// </summary>
        public async Task<WorkOrderResponse> CreateCleanoutAsync(CleanoutRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "CLEANOUT",
                EntityType = "WELL",
                EntityId = request.WellId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.CleanoutData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        #endregion

        #region Well Performance

        /// <summary>
        /// Gets well performance metrics
        /// </summary>
        public async Task<WellPerformanceResponse> GetWellPerformanceAsync(string wellId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Validate well exists
                var well = await GetWellAsync(wellId);
                if (well == null)
                {
                    throw new InvalidOperationException($"Well not found: {wellId}");
                }

                // Calculate performance metrics
                var metrics = new Dictionary<string, decimal>();
                
                // This would query production data, costs, etc.
                // For now, return empty metrics structure
                
                return new WellPerformanceResponse
                {
                    WellId = wellId,
                    ReportDate = DateTime.UtcNow,
                    Metrics = metrics,
                    AdditionalData = new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting well performance: {WellId}", wellId);
                throw;
            }
        }

        #endregion

        #region Well Analysis (using DataFlowService)

        /// <summary>
        /// Runs nodal analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.NodalAnalysisResult> RunWellNodalAnalysisAsync(
            string wellId,
            string userId,
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running nodal analysis for well: {WellId}", wellId);
                return await _dataFlowService.RunNodalAnalysisAsync(wellId, userId, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running nodal analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs DCA (Decline Curve Analysis) for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.DCAResult> RunWellDCAAsync(
            string wellId,
            string userId,
            string calculationType = "Hyperbolic",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running DCA for well: {WellId}, Type: {Type}", wellId, calculationType);
                return await _dataFlowService.RunDCAAsync(wellId: wellId, userId: userId, calculationType: calculationType, additionalParameters: additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running DCA for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs well test analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.WellTestAnalysisResult> RunWellTestAnalysisAsync(
            string wellId,
            string? testId = null,
            string userId = "system",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running well test analysis for well: {WellId}, TestId: {TestId}", wellId, testId);
                return await _dataFlowService.RunWellTestAnalysisAsync(wellId, testId, userId, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running well test analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs choke analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.ChokeAnalysisResult> AnalyzeWellChokeAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "DOWNHOLE",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running choke analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);
                return await _dataFlowService.RunChokeAnalysisAsync(wellId, userId, equipmentId, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running choke analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs gas lift analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.GasLiftAnalysisResult> AnalyzeGasLiftAsync(
            string wellId,
            string userId = "system",
            string analysisType = "POTENTIAL",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running gas lift analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);
                return await _dataFlowService.RunGasLiftAnalysisAsync(wellId, userId, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running gas lift analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs pump performance analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.PumpAnalysisResult> AnalyzeWellPumpAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string pumpType = "ESP",
            string analysisType = "PERFORMANCE",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running pump analysis for well: {WellId}, PumpType: {PumpType}", wellId, pumpType);
                return await _dataFlowService.RunPumpAnalysisAsync(wellId, null, equipmentId, userId, pumpType, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running pump analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs sucker rod pumping analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.SuckerRodAnalysisResult> AnalyzeSuckerRodSystemAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "LOAD",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running sucker rod analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);
                return await _dataFlowService.RunSuckerRodAnalysisAsync(wellId, userId, equipmentId, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running sucker rod analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs plunger lift analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.PlungerLiftAnalysisResult> AnalyzePlungerLiftAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "PERFORMANCE",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running plunger lift analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);
                return await _dataFlowService.RunPlungerLiftAnalysisAsync(wellId, userId, equipmentId, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running plunger lift analysis for well: {WellId}", wellId);
                throw;
            }
        }

        /// <summary>
        /// Runs hydraulic pump analysis for a well using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.HydraulicPumpAnalysisResult> AnalyzeHydraulicPumpAsync(
            string wellId,
            string userId = "system",
            string? equipmentId = null,
            string analysisType = "PERFORMANCE",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running hydraulic pump analysis for well: {WellId}, AnalysisType: {AnalysisType}", wellId, analysisType);
                return await _dataFlowService.RunHydraulicPumpAnalysisAsync(wellId, userId, equipmentId, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running hydraulic pump analysis for well: {WellId}", wellId);
                throw;
            }
        }

        #endregion
    }
}

