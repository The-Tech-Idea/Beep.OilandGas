using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.FieldManagement;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Reporting;
using System.Text.Json;

namespace Beep.OilandGas.LifeCycle.Services.FieldManagement.Processes
{
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

        public async Task<ProcessInstance> StartFieldCreationProcessAsync(string fieldId, string userId)
        {
            var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
            var fieldCreationProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldCreation");
            if (fieldCreationProcess == null)
                throw new InvalidOperationException("FieldCreation process definition not found");

            return await _processService.StartProcessAsync(fieldCreationProcess.ProcessId, fieldId, "FIELD", fieldId, userId);
        }

        public async Task<bool> ApproveFieldCreationAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "FIELD_APPROVAL", "APPROVED", userId);
        }

        public async Task<bool> SetupFieldAsync(string instanceId, FieldCreationRequest setupData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "FIELD_SETUP",
                Data = { ["FieldCreation"] = JsonSerializer.SerializeToElement(setupData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_SETUP", stepData, userId);
        }

        public async Task<bool> ActivateFieldAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "FIELD_ACTIVATION", "ACTIVE", userId);
        }

        public async Task<ProcessInstance> StartFieldPlanningProcessAsync(string fieldId, string planningType, string userId)
        {
            var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
            var planningProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldPlanning");
            if (planningProcess == null)
                throw new InvalidOperationException("FieldPlanning process definition not found");

            return await _processService.StartProcessAsync(planningProcess.ProcessId, fieldId, "FIELD", fieldId, userId);
        }

        public async Task<bool> DesignFieldPlanAsync(string instanceId, FieldPlanningRequest designData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "FIELD_DESIGN",
                Data = { ["FieldPlanning"] = JsonSerializer.SerializeToElement(designData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_DESIGN", stepData, userId);
        }

        public async Task<bool> ReviewFieldPlanAsync(string instanceId, FieldPlanningRequest reviewData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "FIELD_REVIEW",
                Data = { ["FieldPlanning"] = JsonSerializer.SerializeToElement(reviewData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_REVIEW", stepData, userId);
        }

        public async Task<bool> ApproveFieldPlanAsync(string instanceId, string userId)
        {
            return await _processService.CompleteStepAsync(instanceId, "PLAN_APPROVAL", "APPROVED", userId);
        }

        public async Task<ProcessInstance> StartFieldOperationsProcessAsync(string fieldId, string userId)
        {
            var processDef = await _processService.GetProcessDefinitionsByTypeAsync("FIELD_MANAGEMENT");
            var operationsProcess = processDef.FirstOrDefault(p => p.ProcessName == "FieldOperations");
            if (operationsProcess == null)
                throw new InvalidOperationException("FieldOperations process definition not found");

            return await _processService.StartProcessAsync(operationsProcess.ProcessId, fieldId, "FIELD", fieldId, userId);
        }

        public async Task<bool> RecordDailyOperationsAsync(string instanceId, FieldOperationsRequest operationsData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "DAILY_OPERATIONS",
                Data = { ["FieldOperations"] = JsonSerializer.SerializeToElement(operationsData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "DAILY_OPERATIONS", stepData, userId);
        }

        public async Task<bool> MonitorFieldAsync(string instanceId, FieldPerformanceRequest monitoringData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "FIELD_MONITORING",
                Data = { ["FieldPerformance"] = JsonSerializer.SerializeToElement(monitoringData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "FIELD_MONITORING", stepData, userId);
        }

        public async Task<bool> GenerateOperationsReportAsync(string instanceId, GenerateOperationalReportRequest reportData, string userId)
        {
            var stepData = new PROCESS_STEP_DATA
            {
                StepInstanceId = instanceId,
                StepType = "OPERATIONS_REPORTING",
                Data = { ["GenerateOperationalReport"] = JsonSerializer.SerializeToElement(reportData) },
                LastUpdated = DateTime.UtcNow
            };
            return await _processService.ExecuteStepAsync(instanceId, "OPERATIONS_REPORTING", stepData, userId);
        }
    }
}
