using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.FieldManagement;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.FieldManagement.Processes
{
    /// <summary>
    /// Service for Field Management process orchestration
    /// Handles Field Creation, Field Planning, and Field Operations workflows
    /// </summary>
    public class FieldManagementProcessService
    {
        private readonly IProcessService _processService;
        private readonly FieldManagementService _fieldManagementService;
        private readonly ILogger<FieldManagementProcessService>? _logger;

        public FieldManagementProcessService(
            IProcessService processService,
            FieldManagementService fieldManagementService,
            ILogger<FieldManagementProcessService>? logger = null)
        {
            _processService = processService ?? throw new ArgumentNullException(nameof(processService));
            _fieldManagementService = fieldManagementService ?? throw new ArgumentNullException(nameof(fieldManagementService));
            _logger = logger;
        }

        #region Field Creation Process

        public async Task<ProcessInstance> StartFieldCreationProcessAsync(string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
                var fieldCreationProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldCreation");
                
                if (fieldCreationProcess == null)
                {
                    throw new InvalidOperationException("FieldCreation process definition not found");
                }

                return await _processService.StartProcessAsync(
                    fieldCreationProcess.ProcessId,
                    fieldId,
                    "FIELD",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Field Creation process for field: {fieldId}");
                throw;
            }
        }

        public async Task<bool> ApproveFieldCreationAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "FIELD_APPROVAL", "APPROVED", userId);
        }

        public async Task<bool> SetupFieldAsync(string instanceId, Dictionary<string, object> setupData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_SETUP", setupData, userId);
        }

        public async Task<bool> ActivateFieldAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "FIELD_ACTIVATION", "ACTIVE", userId);
        }

        #endregion

        #region Field Planning Process

        public async Task<ProcessInstance> StartFieldPlanningProcessAsync(string fieldId, string planningType, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
                var planningProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldPlanning");
                
                if (planningProcess == null)
                {
                    throw new InvalidOperationException("FieldPlanning process definition not found");
                }

                return await _processService.StartProcessAsync(
                    planningProcess.ProcessId,
                    fieldId,
                    "FIELD",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Field Planning process for field: {fieldId}");
                throw;
            }
        }

        public async Task<bool> DesignFieldPlanAsync(string instanceId, Dictionary<string, object> designData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_DESIGN", designData, userId);
        }

        public async Task<bool> ReviewFieldPlanAsync(string instanceId, Dictionary<string, object> reviewData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_REVIEW", reviewData, userId);
        }

        public async Task<bool> ApproveFieldPlanAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PLAN_APPROVAL", "APPROVED", userId);
        }

        #endregion

        #region Field Operations Process

        public async Task<ProcessInstance> StartFieldOperationsProcessAsync(string fieldId, string userId)
        {
            try
            {
                var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
                var operationsProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldOperations");
                
                if (operationsProcess == null)
                {
                    throw new InvalidOperationException("FieldOperations process definition not found");
                }

                return await _processService.StartProcessAsync(
                    operationsProcess.ProcessId,
                    fieldId,
                    "FIELD",
                    fieldId,
                    userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting Field Operations process for field: {fieldId}");
                throw;
            }
        }

        public async Task<bool> RecordDailyOperationsAsync(string instanceId, Dictionary<string, object> operationsData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "DAILY_OPERATIONS", operationsData, userId);
        }

        public async Task<bool> MonitorFieldAsync(string instanceId, Dictionary<string, object> monitoringData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_MONITORING", monitoringData, userId);
        }

        public async Task<bool> GenerateOperationsReportAsync(string instanceId, Dictionary<string, object> reportData, string userId)
        {
            return await _processService.ExecuteStepAsync(instanceId, "OPERATIONS_REPORTING", reportData, userId);
        }

        #endregion
    }
}

