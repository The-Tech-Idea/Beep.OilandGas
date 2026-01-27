using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Validation;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Extension methods for process services to simplify common operations
    /// </summary>
    public static class ProcessServiceExtensions
    {
        /// <summary>
        /// Get process status as DTO
        /// </summary>
        public static async Task<ProcessStatusResponse> GetProcessStatusDtoAsync(
            this IProcessService processService,
            string instanceId)
        {
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
            {
                return null;
            }

            var processDef = await processService.GetProcessDefinitionAsync(instance.ProcessId);
            var completedSteps = await processService.GetCompletedStepsAsync(instanceId);
            var pendingSteps = await processService.GetPendingStepsAsync(instanceId);
            var availableTransitions = await processService.GetAvailableTransitionsAsync(instanceId);

            var totalSteps = instance.StepInstances.Count;
            var completedCount = completedSteps.Count;
            var progressPercentage = totalSteps > 0 ? (decimal)(completedCount * 100.0 / totalSteps) : 0;

            var currentStep = instance.StepInstances.FirstOrDefault(s => s.StepId == instance.CurrentStepId);
            var currentStepDef = processDef?.Steps.FirstOrDefault(s => s.StepId == instance.CurrentStepId);

            return new ProcessStatusResponse
            {
                InstanceId = instance.InstanceId,
                ProcessName = processDef?.ProcessName ?? string.Empty,
                Status = instance.Status.ToString(),
                CurrentState = instance.CurrentState,
                CurrentStepName = currentStepDef?.StepName ?? string.Empty,
                CompletedStepsCount = completedCount,
                TotalStepsCount = totalSteps,
                ProgressPercentage = progressPercentage,
                StartDate = instance.StartDate,
                CompletionDate = instance.CompletionDate,
                AvailableTransitions = availableTransitions
            };
        }

        /// <summary>
        /// Get process instance as DTO
        /// </summary>
        public static async Task<ProcessInstanceResponse> GetProcessInstanceDtoAsync(
            this IProcessService processService,
            string instanceId)
        {
            var instance = await processService.GetProcessInstanceAsync(instanceId);
            if (instance == null)
            {
                return null;
            }

            var processDef = await processService.GetProcessDefinitionAsync(instance.ProcessId);
            var completedSteps = await processService.GetCompletedStepsAsync(instanceId);
            var totalSteps = instance.StepInstances.Count;
            var completedCount = completedSteps.Count;
            var progressPercentage = totalSteps > 0 ? (decimal)(completedCount * 100.0 / totalSteps) : 0;

            var currentStep = instance.StepInstances.FirstOrDefault(s => s.StepId == instance.CurrentStepId);
            var currentStepDef = processDef?.Steps.FirstOrDefault(s => s.StepId == instance.CurrentStepId);

            var stepResponses = instance.StepInstances.Select(s =>
            {
                var stepDef = processDef?.Steps.FirstOrDefault(sd => sd.StepId == s.StepId);
                return new ProcessStepInstanceResponse
                {
                    StepInstanceId = s.StepInstanceId,
                    StepId = s.StepId,
                    StepName = stepDef?.StepName ?? string.Empty,
                    SequenceNumber = s.SequenceNumber,
                    Status = s.Status.ToString(),
                    IsCurrentStep = s.StepId == instance.CurrentStepId,
                    CanExecute = s.Status == StepStatus.PENDING && CanExecuteStep(instance, s),
                    RequiresApproval = stepDef?.RequiresApproval ?? false,
                    StartDate = s.StartDate,
                    CompletionDate = s.CompletionDate,
                    Outcome = s.Outcome ?? string.Empty,
                    Approvals = s.Approvals?.Select(a => new ApprovalRecordResponse
                    {
                        ApprovalId = a.ApprovalId,
                        ApprovalType = a.ApprovalType,
                        Status = a.Status.ToString(),
                        RequestedDate = a.RequestedDate,
                        RequestedBy = a.RequestedBy,
                        ApprovedDate = a.ApprovedDate,
                        ApprovedBy = a.ApprovedBy ?? string.Empty,
                        Notes = a.Notes ?? string.Empty
                    }).ToList() ?? new List<ApprovalRecordResponse>()
                };
            }).ToList();

            return new ProcessInstanceResponse
            {
                InstanceId = instance.InstanceId,
                ProcessId = instance.ProcessId,
                ProcessName = processDef?.ProcessName ?? string.Empty,
                EntityId = instance.EntityId,
                EntityType = instance.EntityType,
                FieldId = instance.FieldId,
                CurrentState = instance.CurrentState,
                CurrentStepId = instance.CurrentStepId,
                CurrentStepName = currentStepDef?.StepName ?? string.Empty,
                Status = instance.Status.ToString(),
                StartDate = instance.StartDate,
                CompletionDate = instance.CompletionDate,
                StartedBy = instance.StartedBy,
                CompletedStepsCount = completedCount,
                TotalStepsCount = totalSteps,
                ProgressPercentage = progressPercentage,
                Steps = stepResponses
            };
        }

        /// <summary>
        /// Get process history as DTOs
        /// </summary>
        public static async Task<List<ProcessHistoryResponse>> GetProcessHistoryDtosAsync(
            this IProcessService processService,
            string instanceId)
        {
            var history = await processService.GetProcessHistoryAsync(instanceId);
            return history.Select(h => new ProcessHistoryResponse
            {
                HistoryId = h.HistoryId,
                InstanceId = h.InstanceId,
                StepInstanceId = h.StepInstanceId ?? string.Empty,
                Action = h.Action,
                PreviousState = h.PreviousState ?? string.Empty,
                NewState = h.NewState ?? string.Empty,
                Timestamp = h.Timestamp,
                PerformedBy = h.PerformedBy,
                Notes = h.Notes ?? string.Empty
            }).ToList();
        }

        /// <summary>
        /// Start process from DTO request
        /// </summary>
        public static async Task<ProcessInstanceResponse> StartProcessFromDtoAsync(
            this IProcessService processService,
            ProcessInstanceRequest request,
            string userId)
        {
            var instance = await processService.StartProcessAsync(
                request.ProcessId,
                request.EntityId,
                request.EntityType,
                request.FieldId,
                userId);

            // Merge initial data if provided
            if (request.InitialData != null)
            {
                instance.ProcessData = request.InitialData;
                // Save would be handled by the service
            }

            return await processService.GetProcessInstanceDtoAsync(instance.InstanceId);
        }

        /// <summary>
        /// Execute step from DTO request
        /// </summary>
        public static async Task<ProcessStepExecutionResponse> ExecuteStepFromDtoAsync(
            this IProcessService processService,
            ProcessStepExecutionRequest request,
            string userId)
        {
            // Validate step first
            var validationResult = await processService.ValidateStepAsync(
                request.InstanceId,
                request.StepId,
                request.StepData);

            if (!validationResult.IsValid)
            {
                return new ProcessStepExecutionResponse
                {
                    StepId = request.StepId,
                    Status = "FAILED",
                    Success = false,
                    Message = validationResult.ErrorMessage,
                    ValidationResults = new List<ValidationResultResponse>
                    {
                        new ValidationResultResponse
                        {
                            ValidationId = validationResult.ValidationId,
                            IsValid = false,
                            ErrorMessage = validationResult.ErrorMessage,
                            ValidationDate = validationResult.ValidatedDate
                        }
                    }
                };
            }

            // Execute step
            var executed = await processService.ExecuteStepAsync(
                request.InstanceId,
                request.StepId,
                request.StepData,
                userId);

            if (!executed)
            {
                return new ProcessStepExecutionResponse
                {
                    StepId = request.StepId,
                    Status = "FAILED",
                    Success = false,
                    Message = "Failed to execute step"
                };
            }

            // Get step instance
            var instance = await processService.GetProcessInstanceAsync(request.InstanceId);
            var stepInstance = instance?.StepInstances.FirstOrDefault(s => s.StepId == request.StepId);

            return new ProcessStepExecutionResponse
            {
                StepInstanceId = stepInstance?.StepInstanceId ?? string.Empty,
                StepId = request.StepId,
                Status = stepInstance?.Status.ToString() ?? "UNKNOWN",
                Success = true,
                Message = "Step executed successfully",
                ValidationResults = stepInstance?.ValidationResults?.Select(v => new ValidationResultResponse
                {
                    ValidationId = v.ValidationId,
                    IsValid = v.IsValid,
                    ErrorMessage = v.ErrorMessage,
                    ValidationDate = v.ValidatedDate
                }).ToList() ?? new List<ValidationResultResponse>()
            };
        }

        /// <summary>
        /// Complete step from DTO request
        /// </summary>
        public static async Task<bool> CompleteStepFromDtoAsync(
            this IProcessService processService,
            ProcessStepCompletionRequest request,
            string userId)
        {
            return await processService.CompleteStepAsync(
                request.InstanceId,
                request.StepId,
                request.Outcome,
                userId);
        }

        /// <summary>
        /// Transition state from DTO request
        /// </summary>
        public static async Task<ProcessStateTransitionResponse> TransitionStateFromDtoAsync(
            this IProcessService processService,
            ProcessStateTransitionRequest request,
            string userId)
        {
            var instance = await processService.GetProcessInstanceAsync(request.InstanceId);
            if (instance == null)
            {
                return new ProcessStateTransitionResponse
                {
                    InstanceId = request.InstanceId,
                    Success = false,
                    Message = "Process instance not found"
                };
            }

            var previousState = instance.CurrentState;
            var canTransition = await processService.CanTransitionAsync(request.InstanceId, request.TargetState);

            if (!canTransition)
            {
                return new ProcessStateTransitionResponse
                {
                    InstanceId = request.InstanceId,
                    PreviousState = previousState,
                    NewState = request.TargetState,
                    Success = false,
                    Message = $"Transition from {previousState} to {request.TargetState} is not allowed"
                };
            }

            var transitioned = await processService.TransitionStateAsync(
                request.InstanceId,
                request.TargetState,
                userId);

            return new ProcessStateTransitionResponse
            {
                InstanceId = request.InstanceId,
                PreviousState = previousState,
                NewState = request.TargetState,
                Success = transitioned,
                Message = transitioned ? "State transition successful" : "State transition failed"
            };
        }

        /// <summary>
        /// Get all processes for an entity as DTOs
        /// </summary>
        public static async Task<List<ProcessInstanceResponse>> GetProcessInstancesForEntityDtosAsync(
            this IProcessService processService,
            string entityId,
            string entityType)
        {
            var instances = await processService.GetProcessInstancesForEntityAsync(entityId, entityType);
            var dtos = new List<ProcessInstanceResponse>();

            foreach (var instance in instances)
            {
                var dto = await processService.GetProcessInstanceDtoAsync(instance.InstanceId);
                if (dto != null)
                {
                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        #region Private Helper Methods

        private static bool CanExecuteStep(ProcessInstance instance, ProcessStepInstance stepInstance)
        {
            // Step can be executed if:
            // 1. It's in PENDING status
            // 2. All previous required steps are completed
            if (stepInstance.Status != StepStatus.PENDING)
            {
                return false;
            }

            // Check if all previous required steps are completed
            var previousSteps = instance.StepInstances
                .Where(s => s.SequenceNumber < stepInstance.SequenceNumber)
                .ToList();

            // For now, allow execution if step is pending
            // More complex logic can be added here based on step dependencies
            return true;
        }

        #endregion
    }
}

