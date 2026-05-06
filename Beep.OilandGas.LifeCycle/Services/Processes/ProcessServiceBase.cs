using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using ValidationResult = Beep.OilandGas.Models.Data.ValidationResult;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Base implementation for process management service
    /// </summary>
    public abstract class ProcessServiceBase : IProcessService
    {
        protected readonly IDMEEditor _editor;
        protected readonly ICommonColumnHandler _commonColumnHandler;
        protected readonly IPPDM39DefaultsRepository _defaults;
        protected readonly IPPDMMetadataRepository _metadata;
        protected readonly string _connectionName;
        protected readonly ILogger<ProcessServiceBase>? _logger;

        protected ProcessServiceBase(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<ProcessServiceBase>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        // Process Definition Management - To be implemented by derived classes
        public abstract Task<ProcessDefinition?> GetProcessDefinitionAsync(string processId);
        public abstract Task<List<ProcessDefinition>> GetProcessDefinitionsByTypeAsync(string processType);
        public abstract Task<ProcessDefinition> CreateProcessDefinitionAsync(ProcessDefinition definition, string userId);
        public abstract Task<ProcessDefinition> UpdateProcessDefinitionAsync(string processId, ProcessDefinition definition, string userId);
        public abstract Task<bool> DeleteProcessDefinitionAsync(string processId, string userId);

        // Process Instance Management - Base implementation
        public virtual async Task<ProcessInstance> StartProcessAsync(string processId, string entityId, string entityType, string fieldId, string userId)
        {
            try
            {
                var processDef = await GetProcessDefinitionAsync(processId);
                if (processDef == null)
                {
                    throw new InvalidOperationException($"Process definition not found: {processId}");
                }

                if (!processDef.IsActive)
                {
                    throw new InvalidOperationException($"Process definition is not active: {processId}");
                }

                var orderedSteps = processDef.Steps?.OrderBy(s => s.SequenceNumber).ToList() ?? new List<ProcessStepDefinition>();
                if (orderedSteps.Count == 0)
                {
                    throw new InvalidOperationException($"Process definition '{processId}' has no steps defined");
                }

                var instanceId = GenerateInstanceId();
                var instance = new ProcessInstance
                {
                    InstanceId = instanceId,
                    ProcessId = processId,
                    EntityId = entityId,
                    EntityType = entityType,
                    FieldId = fieldId,
                    CurrentState = "INITIAL",
                    CurrentStepId = orderedSteps[0].StepId,
                    Status = ProcessStatus.IN_PROGRESS,
                    StartDate = DateTime.UtcNow,
                    StartedBy = userId,
                    ProcessData = new PROCESS_DATA
                    {
                        InstanceId = instanceId,
                        LastUpdated = DateTime.UtcNow
                    },
                    StepInstances = new List<ProcessStepInstance>(),
                    History = new List<ProcessHistoryEntry>()
                };

                // Create initial step instances
                for (int i = 0; i < orderedSteps.Count; i++)
                {
                    var step = orderedSteps[i];
                    instance.StepInstances.Add(new ProcessStepInstance
                    {
                        StepInstanceId = GenerateStepInstanceId(),
                        InstanceId = instance.InstanceId,
                        StepId = step.StepId,
                        StepName = step.StepName,
                        SequenceNumber = step.SequenceNumber,
                        Status = i == 0 ? StepStatus.PENDING : StepStatus.BLOCKED,
                        RequiredRole = step.RequiredRoles.Count > 0 ? step.RequiredRoles[0] : null,
                        ApprovalRequired = step.RequiresApproval,
                        StepData = new PROCESS_STEP_DATA(),
                        Approvals = new List<ApprovalRecord>(),
                        ValidationResults = new List<ValidationResult>()
                    });
                }

                // Save instance (to be implemented by derived classes)
                await SaveProcessInstanceAsync(instance);

                // Add history entry
                await AddHistoryEntryAsync(instance.InstanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instance.InstanceId,
                    Action = "PROCESS_STARTED",
                    NewState = "INITIAL",
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId,
                    Notes = $"Process {processDef.ProcessName} started for {entityType} {entityId}"
                });

                _logger?.LogInformation($"Process {processId} started for {entityType} {entityId}");

                return instance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error starting process {processId} for {entityType} {entityId}");
                throw;
            }
        }

        public abstract Task<ProcessInstance?> GetProcessInstanceAsync(string instanceId);
        public abstract Task<List<ProcessInstance>> GetProcessInstancesForEntityAsync(string entityId, string entityType);
        public abstract Task<ProcessInstance?> GetCurrentProcessForEntityAsync(string entityId, string entityType);
        public abstract Task<bool> CancelProcessAsync(string instanceId, string reason, string userId);

        // Process Execution - Base implementation
        public virtual async Task<bool> ExecuteStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData, string userId)
        {
            try
            {
                var instance = await GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance not found: {instanceId}");
                }

                var stepInstance = instance.StepInstances.FirstOrDefault(s => s.StepId == stepId);
                if (stepInstance == null)
                {
                    throw new InvalidOperationException($"Step instance not found: {stepId}");
                }

                if (stepInstance.Status != StepStatus.PENDING)
                {
                    throw new InvalidOperationException($"Step {stepId} is not in PENDING status");
                }

                // Validate step data
                var validationResult = await ValidateStepAsync(instanceId, stepId, stepData);
                if (!validationResult.IsValid)
                {
                    throw new InvalidOperationException($"Step validation failed: {validationResult.ErrorMessage}");
                }

                // Update step instance
                stepInstance.Status = StepStatus.IN_PROGRESS;
                stepInstance.StartDate = DateTime.UtcNow;
                stepInstance.StepData ??= new PROCESS_STEP_DATA();
                stepInstance.StepData.StepInstanceId = stepInstance.StepInstanceId;
                if (stepData?.Data != null)
                {
                    foreach (var kvp in stepData.Data)
                        stepInstance.StepData.Data[kvp.Key] = kvp.Value;
                    if (!string.IsNullOrEmpty(stepData.StepType))
                        stepInstance.StepData.StepType = stepData.StepType;
                }
                stepInstance.StepData.LastUpdated = DateTime.UtcNow;
                stepInstance.ValidationResults.Add(validationResult);

                // Update instance
                instance.CurrentStepId = stepId;
                await SaveProcessInstanceAsync(instance);

                // Add history entry
                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    StepInstanceId = stepInstance.StepInstanceId,
                    Action = "STEP_STARTED",
                    NewState = stepId,
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId
                });

                _logger?.LogInformation($"Step {stepId} executed for process instance {instanceId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error executing step {stepId} for process instance {instanceId}");
                throw;
            }
        }

        public virtual async Task<bool> CompleteStepAsync(string instanceId, string stepId, string outcome, string userId)
        {
            try
            {
                var instance = await GetProcessInstanceAsync(instanceId);
                if (instance == null)
                {
                    throw new InvalidOperationException($"Process instance not found: {instanceId}");
                }

                var stepInstance = instance.StepInstances.FirstOrDefault(s => s.StepId == stepId);
                if (stepInstance == null)
                {
                    throw new InvalidOperationException($"Step instance not found: {stepId}");
                }

                if (stepInstance.Status != StepStatus.IN_PROGRESS && stepInstance.Status != StepStatus.PENDING)
                {
                    throw new InvalidOperationException($"Step {stepId} is not in a completable status (PENDING or IN_PROGRESS)");
                }

                // If step was PENDING, mark start timestamps
                if (stepInstance.Status == StepStatus.PENDING)
                {
                    stepInstance.StartDate = DateTime.UtcNow;
                }

                // Update step instance
                stepInstance.Status = StepStatus.COMPLETED;
                stepInstance.CompletionDate = DateTime.UtcNow;
                stepInstance.CompletedBy = userId;
                stepInstance.Outcome = outcome;

                // Determine next step based on outcome and conditional branching
                var processDef = await GetProcessDefinitionAsync(instance.ProcessId);
                var currentStep = processDef?.Steps?.FirstOrDefault(s => s.StepId == stepId);
                string? nextStepId = null;

                if (currentStep != null)
                {
                    // Check conditional next steps (stored as "outcome:stepId" pairs in StepConfiguration)
                    if (currentStep.StepConfiguration?.TryGetValue("ConditionalNextSteps", out var condObj) == true)
                    {
                        var condStr = condObj?.ToString() ?? "";
                        var pairs = condStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var pair in pairs)
                        {
                            var parts = pair.Split(':');
                            if (parts.Length == 2 && string.Equals(parts[0].Trim(), outcome, StringComparison.OrdinalIgnoreCase))
                            {
                                nextStepId = parts[1].Trim();
                                break;
                            }
                        }
                    }

                    // Fall back to default next step
                    if (string.IsNullOrEmpty(nextStepId) && !string.IsNullOrEmpty(currentStep.NextStepId))
                    {
                        nextStepId = currentStep.NextStepId;
                    }
                }

                if (!string.IsNullOrEmpty(nextStepId))
                {
                    var nextStep = instance.StepInstances.FirstOrDefault(s => s.StepId == nextStepId);
                    if (nextStep != null)
                    {
                        nextStep.Status = StepStatus.PENDING;
                        instance.CurrentStepId = nextStepId;
                        instance.CurrentState = nextStepId;
                    }
                }
                else
                {
                    instance.CurrentState = stepId;
                }

                // Check if process is complete
                var allStepsCompleted = instance.StepInstances.All(s =>
                    s.Status == StepStatus.COMPLETED ||
                    s.Status == StepStatus.SKIPPED);
                if (allStepsCompleted)
                {
                    instance.Status = ProcessStatus.COMPLETED;
                    instance.CompletionDate = DateTime.UtcNow;
                    instance.CompletedBy = userId;
                    instance.CurrentState = "COMPLETED";
                }

                await SaveProcessInstanceAsync(instance);

                // Add history entry
                await AddHistoryEntryAsync(instanceId, new ProcessHistoryEntry
                {
                    HistoryId = GenerateHistoryId(),
                    InstanceId = instanceId,
                    StepInstanceId = stepInstance.StepInstanceId,
                    Action = "STEP_COMPLETED",
                    PreviousState = stepId,
                    NewState = instance.CurrentStepId,
                    Timestamp = DateTime.UtcNow,
                    PerformedBy = userId,
                    Notes = $"Step completed with outcome: {outcome}"
                });

                _logger?.LogInformation($"Step {stepId} completed for process instance {instanceId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error completing step {stepId} for process instance {instanceId}");
                throw;
            }
        }

        public abstract Task<bool> SkipStepAsync(string instanceId, string stepId, string reason, string userId);
        public abstract Task<bool> RollbackStepAsync(string instanceId, string stepId, string reason, string userId);

        // State Management - To be implemented by derived classes
        public abstract Task<bool> TransitionStateAsync(string instanceId, string targetState, string userId);
        public abstract Task<List<string>> GetAvailableTransitionsAsync(string instanceId);
        public abstract Task<bool> CanTransitionAsync(string instanceId, string targetState);

        // Process Status - Base implementation
        public virtual async Task<ProcessStatus> GetProcessStatusAsync(string instanceId)
        {
            var instance = await GetProcessInstanceAsync(instanceId);
            return instance?.Status ?? ProcessStatus.NOT_STARTED;
        }

        public virtual async Task<List<ProcessStepInstance>> GetCompletedStepsAsync(string instanceId)
        {
            var instance = await GetProcessInstanceAsync(instanceId);
            return instance?.StepInstances.Where(s => s.Status == StepStatus.COMPLETED).ToList() ?? new List<ProcessStepInstance>();
        }

        public virtual async Task<List<ProcessStepInstance>> GetPendingStepsAsync(string instanceId)
        {
            var instance = await GetProcessInstanceAsync(instanceId);
            return instance?.StepInstances.Where(s => s.Status == StepStatus.PENDING).ToList() ?? new List<ProcessStepInstance>();
        }

        // Process History - Base implementation
        public abstract Task<List<ProcessHistoryEntry>> GetProcessHistoryAsync(string instanceId);
        public abstract Task<ProcessHistoryEntry> AddHistoryEntryAsync(string instanceId, ProcessHistoryEntry entry);

        // Validation - To be implemented by derived classes
        public abstract Task<ValidationResult> ValidateStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData);
        public abstract Task<bool> ValidateProcessCompletionAsync(string instanceId);

        // Approvals - To be implemented by derived classes
        public abstract Task<bool> RequestApprovalAsync(string stepInstanceId, string approvalType, string requestedBy, string userId);
        public abstract Task<bool> ApproveStepAsync(string approvalId, string approvedBy, string notes, string userId);
        public abstract Task<bool> RejectStepAsync(string approvalId, string rejectedBy, string reason, string userId);

        // Helper methods
        protected abstract Task SaveProcessInstanceAsync(ProcessInstance instance);
        protected abstract Task<ProcessInstance?> LoadProcessInstanceAsync(string instanceId);

        protected virtual string GenerateInstanceId() => $"PI_{Guid.NewGuid():N}";
        protected virtual string GenerateStepInstanceId() => $"PSI_{Guid.NewGuid():N}";
        protected virtual string GenerateHistoryId() => $"PH_{Guid.NewGuid():N}";
        protected virtual string GenerateApprovalId() => $"PA_{Guid.NewGuid():N}";
    }
}

