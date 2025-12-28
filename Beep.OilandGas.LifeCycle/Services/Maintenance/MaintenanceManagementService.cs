using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.DTOs.LifeCycle;
using Beep.OilandGas.LifeCycle.Services.WorkOrder;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Maintenance
{
    /// <summary>
    /// Service for Maintenance Management including preventive, corrective, and emergency maintenance
    /// </summary>
    public class MaintenanceManagementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly WorkOrderManagementService? _workOrderService;
        private readonly string _connectionName;
        private readonly ILogger<MaintenanceManagementService>? _logger;

        public MaintenanceManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            WorkOrderManagementService? workOrderService = null,
            string connectionName = "PPDM39",
            ILogger<MaintenanceManagementService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _workOrderService = workOrderService;
            _connectionName = connectionName ?? "PPDM39";
            _logger = logger;
        }

        public async Task<MaintenanceResponse> ScheduleMaintenanceAsync(MaintenanceScheduleRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Maintenance scheduled for {EntityType}: {EntityId}, Type: {MaintenanceType}, Date: {ScheduledDate}", 
                    request.EntityType, request.EntityId, request.MaintenanceType, request.ScheduledDate);

                // Create work order if WorkOrderService is available and maintenance requires it
                string? workOrderId = null;
                if (_workOrderService != null && request.CreateWorkOrder == true)
                {
                    try
                    {
                        var workOrderNumber = $"WO-{request.EntityType}-{request.EntityId}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                        var workOrderRequest = new WorkOrderCreationRequest
                        {
                            WorkOrderNumber = workOrderNumber,
                            WorkOrderType = "MAINTENANCE",
                            EntityType = request.EntityType,
                            EntityId = request.EntityId,
                            Instructions = $"Scheduled {request.MaintenanceType} maintenance",
                            RequestDate = DateTime.UtcNow,
                            DueDate = request.ScheduledDate,
                            Description = request.ScheduleData?.ContainsKey("Description") == true 
                                ? request.ScheduleData["Description"]?.ToString() 
                                : $"Scheduled maintenance for {request.EntityType} {request.EntityId}",
                            AdditionalProperties = request.ScheduleData
                        };

                        var workOrder = await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
                        workOrderId = workOrder.WorkOrderId;
                        _logger?.LogInformation("Work order {WorkOrderId} created for scheduled maintenance", workOrderId);
                    }
                    catch (Exception woEx)
                    {
                        _logger?.LogWarning(woEx, "Failed to create work order for scheduled maintenance, continuing without work order");
                    }
                }
                
                return new MaintenanceResponse
                {
                    MaintenanceId = Guid.NewGuid().ToString(),
                    EntityId = request.EntityId,
                    EntityType = request.EntityType,
                    MaintenanceType = request.MaintenanceType,
                    ScheduledDate = request.ScheduledDate,
                    Status = "SCHEDULED",
                    MaintenanceData = request.ScheduleData,
                    WorkOrderId = workOrderId
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error scheduling maintenance: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<bool> ExecuteMaintenanceAsync(MaintenanceExecutionRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Maintenance executed: {MaintenanceId}, Date: {ExecutionDate}, By: {ExecutedBy}", 
                    request.MaintenanceId, request.ExecutionDate, request.ExecutedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing maintenance: {MaintenanceId}", request.MaintenanceId);
                throw;
            }
        }

        public async Task<MaintenanceResponse> RequestMaintenanceAsync(MaintenanceRequest request, string userId)
        {
            try
            {
                _logger?.LogInformation("Maintenance requested for {EntityType}: {EntityId}, Type: {MaintenanceType}, Priority: {Priority}", 
                    request.EntityType, request.EntityId, request.MaintenanceType, request.Priority);

                // Create work order if WorkOrderService is available
                string? workOrderId = null;
                if (_workOrderService != null)
                {
                    try
                    {
                        var workOrderNumber = $"WO-{request.EntityType}-{request.EntityId}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                        var workOrderRequest = new WorkOrderCreationRequest
                        {
                            WorkOrderNumber = workOrderNumber,
                            WorkOrderType = "MAINTENANCE",
                            EntityType = request.EntityType,
                            EntityId = request.EntityId,
                            Instructions = $"{request.MaintenanceType} maintenance requested - Priority: {request.Priority}",
                            RequestDate = DateTime.UtcNow,
                            Description = request.Description,
                            AdditionalProperties = request.RequestData
                        };

                        var workOrder = await _workOrderService.CreateWorkOrderAsync(workOrderRequest, userId);
                        workOrderId = workOrder.WorkOrderId;
                        _logger?.LogInformation("Work order {WorkOrderId} created for maintenance request", workOrderId);
                    }
                    catch (Exception woEx)
                    {
                        _logger?.LogWarning(woEx, "Failed to create work order for maintenance request, continuing without work order");
                    }
                }
                
                return new MaintenanceResponse
                {
                    MaintenanceId = Guid.NewGuid().ToString(),
                    EntityId = request.EntityId,
                    EntityType = request.EntityType,
                    MaintenanceType = request.MaintenanceType,
                    Status = "REQUESTED",
                    WorkOrderId = workOrderId,
                    MaintenanceData = request.RequestData
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error requesting maintenance: {EntityId}", request.EntityId);
                throw;
            }
        }

        public async Task<List<MaintenanceResponse>> GetMaintenanceHistoryAsync(MaintenanceHistoryRequest request)
        {
            try
            {
                _logger?.LogInformation("Retrieving maintenance history for {EntityType}: {EntityId}", 
                    request.EntityType, request.EntityId);
                
                return new List<MaintenanceResponse>();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting maintenance history: {EntityId}", request.EntityId);
                throw;
            }
        }
    }
}

