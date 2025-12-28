using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.WorkOrder;
using Beep.OilandGas.LifeCycle.Services.Accounting;
using Beep.OilandGas.Models.DTOs.LifeCycle;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.WorkOrder.Processes
{
    /// <summary>
    /// Service for Work Order process orchestration
    /// Handles Work Order Creation, Approval, Execution, and Completion workflows
    /// </summary>
    public class WorkOrderProcessService
    {
        private readonly IProcessService _processService;
        private readonly WorkOrderManagementService _workOrderService;
        private readonly WorkOrderAccountingService? _accountingService;
        private readonly ILogger<WorkOrderProcessService>? _logger;

        public WorkOrderProcessService(
            IProcessService processService,
            WorkOrderManagementService workOrderService,
            WorkOrderAccountingService? accountingService = null,
            ILogger<WorkOrderProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _workOrderService = workOrderService ?? throw new ArgumentNullException(nameof(workOrderService));
            _accountingService = accountingService;
            _logger = logger;
        }

        #region Work Order Creation Process

        /// <summary>
        /// Starts the work order creation process: Request → Approval → AFE Creation → Execution → Completion
        /// </summary>
        public async Task<ProcessInstance> StartWorkOrderProcessAsync(
            string workOrderId,
            string entityId,
            string entityType,
            string? fieldId,
            string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("WORK_ORDER");
                var workOrderProcess = processDef.FirstOrDefault(p => p.ProcessName == "WorkOrderCreation");
                
                if (workOrderProcess == null)
                {
                    throw new InvalidOperationException("WorkOrderCreation process definition not found");
                }

                return await _processService.StartProcessAsync(
                    workOrderProcess.ProcessId,
                    workOrderId,
                    entityType,
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting work order process for work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        #endregion

        #region Work Order Approval Process

        /// <summary>
        /// Starts the work order approval process: Review → Approval → Authorization
        /// </summary>
        public async Task<ProcessInstance> StartWorkOrderApprovalProcessAsync(
            string workOrderId,
            string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("WORK_ORDER");
                var approvalProcess = processDef.FirstOrDefault(p => p.ProcessName == "WorkOrderApproval");
                
                if (approvalProcess == null)
                {
                    throw new InvalidOperationException("WorkOrderApproval process definition not found");
                }

                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                return await _processService.StartProcessAsync(
                    approvalProcess.ProcessId,
                    workOrderId,
                    workOrder.EntityType,
                    workOrder.FieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting work order approval process for work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        #endregion

        #region Work Order Execution Process

        /// <summary>
        /// Starts the work order execution process: Planning → Execution → Cost Recording → Completion
        /// </summary>
        public async Task<ProcessInstance> StartWorkOrderExecutionProcessAsync(
            string workOrderId,
            string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("WORK_ORDER");
                var executionProcess = processDef.FirstOrDefault(p => p.ProcessName == "WorkOrderExecution");
                
                if (executionProcess == null)
                {
                    throw new InvalidOperationException("WorkOrderExecution process definition not found");
                }

                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                // Create or link AFE if not already done
                if (_accountingService != null && string.IsNullOrEmpty(workOrder.AfeId))
                {
                    try
                    {
                        var afe = await _accountingService.CreateOrLinkAFEAsync(workOrder, userId);
                        workOrder.AfeId = afe.AFE_ID;
                        _logger?.LogInformation("AFE {AfeId} created/linked for work order {WorkOrderId}", afe.AFE_ID, workOrderId);
                    }
                    catch (Exception afeEx)
                    {
                        _logger?.LogWarning(afeEx, "Failed to create/link AFE for work order {WorkOrderId}, continuing with execution", workOrderId);
                    }
                }

                return await _processService.StartProcessAsync(
                    executionProcess.ProcessId,
                    workOrderId,
                    workOrder.EntityType,
                    workOrder.FieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting work order execution process for work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        #endregion

        #region Work Order Completion Process

        /// <summary>
        /// Starts the work order completion process: Verification → Cost Finalization → Reporting
        /// </summary>
        public async Task<ProcessInstance> StartWorkOrderCompletionProcessAsync(
            string workOrderId,
            string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("WORK_ORDER");
                var completionProcess = processDef.FirstOrDefault(p => p.ProcessName == "WorkOrderCompletion");
                
                if (completionProcess == null)
                {
                    throw new InvalidOperationException("WorkOrderCompletion process definition not found");
                }

                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                return await _processService.StartProcessAsync(
                    completionProcess.ProcessId,
                    workOrderId,
                    workOrder.EntityType,
                    workOrder.FieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error starting work order completion process for work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        /// <summary>
        /// Completes a work order and finalizes costs
        /// </summary>
        public async Task<bool> CompleteWorkOrderAsync(
            string workOrderId,
            DateTime completeDate,
            string userId)
        {
            try
            {
                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                var updateRequest = new WorkOrderUpdateRequest
                {
                    WorkOrderId = workOrderId,
                    Status = "COMPLETED",
                    CompleteDate = completeDate
                };

                await _workOrderService.UpdateWorkOrderAsync(updateRequest, userId);

                _logger?.LogInformation("Work order {WorkOrderId} completed", workOrderId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error completing work order {WorkOrderId}", workOrderId);
                throw;
            }
        }

        #endregion

        #region Workflow Step Implementations

        /// <summary>
        /// Plans work order: Work planning and resource allocation step
        /// </summary>
        public async Task<bool> PlanWorkOrderAsync(string instanceId, Dictionary<string, object> planningData, string userId)
        {
            try
            {
                _logger?.LogInformation("Planning work order for process instance {InstanceId}", instanceId);
                return await _processService.ExecuteStepAsync(instanceId, "WORK_ORDER_PLANNING", planningData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning work order for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Approves work order: Work order approval step
        /// </summary>
        public async Task<bool> ApproveWorkOrderAsync(string instanceId, string userId)
        {
            try
            {
                _logger?.LogInformation("Approving work order for process instance {InstanceId}", instanceId);
                return await _processService.CompleteStepAsync(instanceId, "WORK_ORDER_APPROVAL", "APPROVED", userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error approving work order for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Authorizes AFE: AFE authorization step
        /// </summary>
        public async Task<bool> AuthorizeAFEAsync(string instanceId, string userId)
        {
            try
            {
                _logger?.LogInformation("Authorizing AFE for work order process instance {InstanceId}", instanceId);
                
                // Get work order ID from process instance
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance {instanceId} not found");
                }

                var workOrderId = instance.EntityId;
                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                // Create or link AFE if accounting service is available
                if (_accountingService != null && string.IsNullOrEmpty(workOrder.AfeId))
                {
                    var afe = await _accountingService.CreateOrLinkAFEAsync(workOrder, userId);
                    _logger?.LogInformation("AFE {AfeId} created/linked for work order {WorkOrderId}", afe.AFE_ID, workOrderId);
                }

                return await _processService.CompleteStepAsync(instanceId, "AFE_AUTHORIZATION", "AUTHORIZED", userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error authorizing AFE for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Executes work order: Work execution step
        /// </summary>
        public async Task<bool> ExecuteWorkOrderAsync(string instanceId, Dictionary<string, object> executionData, string userId)
        {
            try
            {
                _logger?.LogInformation("Executing work order for process instance {InstanceId}", instanceId);
                return await _processService.ExecuteStepAsync(instanceId, "WORK_ORDER_EXECUTION", executionData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing work order for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Records work order costs: Cost recording step
        /// </summary>
        public async Task<bool> RecordWorkOrderCostsAsync(string instanceId, List<WorkOrderCostRequest> costs, string userId)
        {
            try
            {
                if (_accountingService == null)
                {
                    throw new InvalidOperationException("WorkOrderAccountingService is not available");
                }

                _logger?.LogInformation("Recording {Count} costs for work order process instance {InstanceId}", costs.Count, instanceId);

                // Get work order ID from process instance
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance {instanceId} not found");
                }

                var workOrderId = instance.EntityId;
                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                // Record each cost
                foreach (var cost in costs)
                {
                    cost.WorkOrderId = workOrderId;
                    await _accountingService.RecordWorkOrderCostAsync(
                        cost,
                        workOrder.EntityType == "WELL" ? workOrder.EntityId : null,
                        workOrder.EntityType == "FACILITY" ? workOrder.EntityId : null,
                        workOrder.FieldId,
                        workOrder.PropertyId,
                        cost.TransactionDate,
                        cost.Description,
                        userId);
                }

                return await _processService.ExecuteStepAsync(instanceId, "COST_RECORDING", new Dictionary<string, object> { { "CostCount", costs.Count } }, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording work order costs for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Verifies work order completion: Completion verification step
        /// </summary>
        public async Task<bool> VerifyWorkOrderCompletionAsync(string instanceId, Dictionary<string, object> verificationData, string userId)
        {
            try
            {
                _logger?.LogInformation("Verifying work order completion for process instance {InstanceId}", instanceId);
                return await _processService.ExecuteStepAsync(instanceId, "COMPLETION_VERIFICATION", verificationData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying work order completion for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Finalizes work order costs: Cost finalization step
        /// </summary>
        public async Task<bool> FinalizeWorkOrderCostsAsync(string instanceId, string userId)
        {
            try
            {
                _logger?.LogInformation("Finalizing work order costs for process instance {InstanceId}", instanceId);

                // Get work order ID from process instance
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance {instanceId} not found");
                }

                var workOrderId = instance.EntityId;
                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                // Update work order status to indicate costs are finalized
                var updateRequest = new WorkOrderUpdateRequest
                {
                    WorkOrderId = workOrderId,
                    Status = "COSTS_FINALIZED"
                };
                await _workOrderService.UpdateWorkOrderAsync(updateRequest, userId);

                return await _processService.CompleteStepAsync(instanceId, "COST_FINALIZATION", "FINALIZED", userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error finalizing work order costs for instance {InstanceId}", instanceId);
                throw;
            }
        }

        /// <summary>
        /// Generates work order report: Reporting step
        /// </summary>
        public async Task<bool> GenerateWorkOrderReportAsync(string instanceId, string userId)
        {
            try
            {
                _logger?.LogInformation("Generating work order report for process instance {InstanceId}", instanceId);
                
                // Get work order ID from process instance
                var instance = await _processService.GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance {instanceId} not found");
                }

                var workOrderId = instance.EntityId;
                var workOrder = await _workOrderService.GetWorkOrderAsync(workOrderId);

                // Generate report data
                var reportData = new Dictionary<string, object>
                {
                    { "WorkOrderId", workOrder.WorkOrderId },
                    { "WorkOrderNumber", workOrder.WorkOrderNumber },
                    { "WorkOrderType", workOrder.WorkOrderType },
                    { "Status", workOrder.Status },
                    { "EstimatedCost", workOrder.EstimatedCost },
                    { "ActualCost", workOrder.ActualCost },
                    { "ReportDate", DateTime.UtcNow }
                };

                return await _processService.ExecuteStepAsync(instanceId, "WORK_ORDER_REPORTING", reportData, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating work order report for instance {InstanceId}", instanceId);
                throw;
            }
        }

        #endregion
    }
}

