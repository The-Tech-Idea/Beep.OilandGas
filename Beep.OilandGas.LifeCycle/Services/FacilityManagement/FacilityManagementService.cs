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
using Beep.OilandGas.Models.DTOs.LifeCycle;
using Beep.OilandGas.LifeCycle.Services.WorkOrder;
using Beep.OilandGas.LifeCycle.Services.Integration;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.FacilityManagement
{
    /// <summary>
    /// Comprehensive service for Facility management including creation, operations, maintenance, inspection, and integrity
    /// </summary>
    public class FacilityManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly WorkOrderManagementService? _workOrderService;
        private readonly DataFlowService? _dataFlowService;
        private readonly string _connectionName;
        private readonly ILogger<FacilityManagementService>? _logger;

        public FacilityManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            WorkOrderManagementService? workOrderService = null,
            DataFlowService? dataFlowService = null,
            string connectionName = "PPDM39",
            ILogger<FacilityManagementService>? logger = null)
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

        #region Facility Creation

        public async Task<FacilityResponse> CreateFacilityAsync(FacilityCreationRequest request, string userId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null)
                {
                    throw new InvalidOperationException("FACILITY table metadata not found");
                }

                var entityType = typeof(FACILITY);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY", null);

                var facility = new FACILITY();
                facility.FACILITY_NAME = request.FacilityName;
                facility.FIELD_ID = _defaults.FormatIdForTable("FACILITY", request.FieldId);
                facility.FACILITY_TYPE = request.FacilityType;
                facility.FACILITY_PURPOSE = request.FacilityPurpose;
                if (request.Capacity.HasValue)
                    facility.CAPACITY = request.Capacity.Value;
                facility.ACTIVE_IND = "Y";

                if (facility is IPPDMEntity entity)
                    _commonColumnHandler.PrepareForInsert(entity, userId);
                var result = await repo.InsertAsync(facility, userId);
                var createdFacility = result as FACILITY ?? throw new InvalidOperationException("Failed to create facility");

                _logger?.LogInformation("Facility created: {FacilityId}, Name: {FacilityName}", createdFacility.FACILITY_ID, createdFacility.FACILITY_NAME);

                return new FacilityResponse
                {
                    FacilityId = createdFacility.FACILITY_ID,
                    FacilityName = createdFacility.FACILITY_NAME ?? string.Empty,
                    FieldId = createdFacility.FIELD_ID ?? string.Empty,
                    Status = "ACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "FacilityType", createdFacility.FACILITY_TYPE ?? string.Empty },
                        { "FacilityPurpose", createdFacility.FACILITY_PURPOSE ?? string.Empty }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating facility: {FacilityName}", request.FacilityName);
                throw;
            }
        }

        public async Task<FacilityResponse?> GetFacilityAsync(string facilityId)
        {
            try
            {
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY");
                if (metadata == null) return null;

                var entityType = typeof(FACILITY);
                var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, _connectionName, "FACILITY", null);

                var formattedFacilityId = _defaults.FormatIdForTable("FACILITY", facilityId);
                var facility = await repo.GetByIdAsync(formattedFacilityId);
                if (facility is not FACILITY facilityEntity) return null;

                return new FacilityResponse
                {
                    FacilityId = facilityEntity.FACILITY_ID,
                    FacilityName = facilityEntity.FACILITY_NAME ?? string.Empty,
                    FieldId = facilityEntity.FIELD_ID ?? string.Empty,
                    Status = facilityEntity.ACTIVE_IND == "Y" ? "ACTIVE" : "INACTIVE",
                    Properties = new Dictionary<string, object>
                    {
                        { "FacilityType", facilityEntity.FACILITY_TYPE ?? string.Empty }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting facility: {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion

        #region Facility Operations

        public async Task<bool> RecordFacilityOperationsAsync(FacilityOperationsRequest request, string userId)
        {
            try
            {
                var facility = await GetFacilityAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {request.FacilityId}");
                }

                _logger?.LogInformation("Facility operations recorded for facility: {FacilityId}, Type: {OperationType}", 
                    request.FacilityId, request.OperationType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording facility operations: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Facility Maintenance

        public async Task<bool> CreateFacilityMaintenanceAsync(FacilityMaintenanceRequest request, string userId)
        {
            try
            {
                var facility = await GetFacilityAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {request.FacilityId}");
                }

                _logger?.LogInformation("Facility maintenance created for facility: {FacilityId}, Type: {MaintenanceType}", 
                    request.FacilityId, request.MaintenanceType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating facility maintenance: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Facility Inspection

        public async Task<bool> CreateFacilityInspectionAsync(FacilityInspectionRequest request, string userId)
        {
            try
            {
                var facility = await GetFacilityAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {request.FacilityId}");
                }

                _logger?.LogInformation("Facility inspection created for facility: {FacilityId}, Type: {InspectionType}", 
                    request.FacilityId, request.InspectionType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating facility inspection: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Facility Integrity

        public async Task<bool> AssessFacilityIntegrityAsync(FacilityIntegrityRequest request, string userId)
        {
            try
            {
                var facility = await GetFacilityAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {request.FacilityId}");
                }

                _logger?.LogInformation("Facility integrity assessed for facility: {FacilityId}, Type: {AssessmentType}", 
                    request.FacilityId, request.AssessmentType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing facility integrity: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Facility Equipment

        public async Task<bool> RecordFacilityEquipmentAsync(FacilityEquipmentRequest request, string userId)
        {
            try
            {
                var facility = await GetFacilityAsync(request.FacilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {request.FacilityId}");
                }

                // Store equipment data in FACILITY_EQUIPMENT table
                var metadata = await _metadata.GetTableMetadataAsync("FACILITY_EQUIPMENT");
                if (metadata != null)
                {
                    var entityType = typeof(FACILITY_EQUIPMENT);
                    var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                        entityType, _connectionName, "FACILITY_EQUIPMENT", null);

                    var equipment = new FACILITY_EQUIPMENT();
                    equipment.FACILITY_ID = _defaults.FormatIdForTable("FACILITY_EQUIPMENT", request.FacilityId);
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

                _logger?.LogInformation("Facility equipment recorded for facility: {FacilityId}, Type: {EquipmentType}", 
                    request.FacilityId, request.EquipmentType);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording facility equipment: {FacilityId}", request.FacilityId);
                throw;
            }
        }

        #endregion

        #region Facility Performance

        public async Task<FacilityPerformanceResponse> GetFacilityPerformanceAsync(string facilityId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var facility = await GetFacilityAsync(facilityId);
                if (facility == null)
                {
                    throw new InvalidOperationException($"Facility not found: {facilityId}");
                }

                var metrics = new Dictionary<string, decimal>();
                
                return new FacilityPerformanceResponse
                {
                    FacilityId = facilityId,
                    ReportDate = DateTime.UtcNow,
                    Metrics = metrics,
                    AdditionalData = new Dictionary<string, object>()
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting facility performance: {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion

        #region Facility Work Order Operations

        /// <summary>
        /// Creates a maintenance work order for a facility
        /// </summary>
        public async Task<WorkOrderResponse> CreateMaintenanceWorkOrderAsync(FacilityWorkOrderRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "MAINTENANCE",
                EntityType = "FACILITY",
                EntityId = request.FacilityId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkOrderData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates a repair work order for a facility
        /// </summary>
        public async Task<WorkOrderResponse> CreateRepairWorkOrderAsync(FacilityWorkOrderRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "REPAIR",
                EntityType = "FACILITY",
                EntityId = request.FacilityId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkOrderData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates an upgrade work order for a facility
        /// </summary>
        public async Task<WorkOrderResponse> CreateUpgradeWorkOrderAsync(FacilityWorkOrderRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "UPGRADE",
                EntityType = "FACILITY",
                EntityId = request.FacilityId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkOrderData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        /// <summary>
        /// Creates an inspection work order for a facility
        /// </summary>
        public async Task<WorkOrderResponse> CreateInspectionWorkOrderAsync(FacilityWorkOrderRequest request, string userId)
        {
            if (_workOrderService == null)
            {
                throw new InvalidOperationException("WorkOrderManagementService is not available");
            }

            var workOrderRequest = new WorkOrderCreationRequest
            {
                WorkOrderNumber = request.WorkOrderNumber,
                WorkOrderType = "INSPECTION",
                EntityType = "FACILITY",
                EntityId = request.FacilityId,
                FieldId = request.FieldId,
                PropertyId = request.PropertyId,
                Instructions = request.Instructions,
                RequestDate = request.RequestDate,
                DueDate = request.DueDate,
                EstimatedCost = request.EstimatedCost,
                Description = request.Description,
                AdditionalProperties = request.WorkOrderData
            };

            return await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
        }

        #endregion

        #region Facility Analysis (using DataFlowService)

        /// <summary>
        /// Runs compressor analysis for a facility using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.DTOs.CompressorAnalysisResult> AnalyzeCompressorAsync(
            string facilityId,
            string userId = "system",
            string? equipmentId = null,
            string compressorType = "CENTRIFUGAL",
            string analysisType = "POWER",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running compressor analysis for facility: {FacilityId}, CompressorType: {CompressorType}", 
                    facilityId, compressorType);
                return await _dataFlowService.RunCompressorAnalysisAsync(facilityId, userId, equipmentId, compressorType, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running compressor analysis for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        /// <summary>
        /// Runs pump performance analysis for a facility using DataFlowService
        /// </summary>
        public async Task<Beep.OilandGas.Models.DTOs.PumpAnalysisResult> AnalyzeFacilityPumpAsync(
            string facilityId,
            string userId = "system",
            string? equipmentId = null,
            string pumpType = "CENTRIFUGAL",
            string analysisType = "PERFORMANCE",
            Dictionary<string, object>? additionalParameters = null)
        {
            if (_dataFlowService == null)
            {
                throw new InvalidOperationException("DataFlowService is not available. Inject DataFlowService in constructor to use analysis features.");
            }

            try
            {
                _logger?.LogInformation("Running pump analysis for facility: {FacilityId}, PumpType: {PumpType}", facilityId, pumpType);
                return await _dataFlowService.RunPumpAnalysisAsync(null, facilityId, equipmentId, userId, pumpType, analysisType, additionalParameters);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error running pump analysis for facility: {FacilityId}", facilityId);
                throw;
            }
        }

        #endregion
    }
}

