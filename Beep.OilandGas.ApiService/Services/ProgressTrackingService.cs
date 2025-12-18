using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// Service for tracking and broadcasting progress of long-running operations
    /// </summary>
    public interface IProgressTrackingService
    {
        string StartOperation(string operationType, string description);
        void UpdateProgress(string operationId, int percentage, string statusMessage, long? itemsProcessed = null, long? totalItems = null);
        void UpdateProgress(ProgressUpdate progress);
        void CompleteOperation(string operationId, bool success, string? message = null, string? errorMessage = null);
        ProgressUpdate? GetProgress(string operationId);
        void CancelOperation(string operationId);

        // Workflow support
        string StartWorkflow(string workflowName, List<string> stepNames, List<int>? stepWeights = null);
        void UpdateWorkflowStep(string workflowId, string stepId, string operationId, int progress, string message, long? itemsProcessed = null, long? totalItems = null);
        void CompleteWorkflowStep(string workflowId, string stepId, bool success, string? errorMessage = null);
        void CompleteWorkflow(string workflowId, bool success, string? errorMessage = null);
        WorkflowProgress? GetWorkflowProgress(string workflowId);

        // Multi-operation support
        void RegisterOperationGroup(string groupId, List<string> operationIds, string groupName = "");
        void UpdateOperationGroup(string groupId);
        MultiOperationProgress? GetOperationGroupProgress(string groupId);
    }

    /// <summary>
    /// SignalR hub for progress updates
    /// </summary>
    public class ProgressHub : Hub
    {
        public async Task JoinOperationGroup(string operationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, operationId);
        }

        public async Task LeaveOperationGroup(string operationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, operationId);
        }

        public async Task JoinWorkflowGroup(string workflowId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workflowId);
        }

        public async Task LeaveWorkflowGroup(string workflowId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workflowId);
        }
    }

        /// <summary>
        /// Implementation of progress tracking service
        /// </summary>
        public class ProgressTrackingService : IProgressTrackingService
        {
            private readonly IHubContext<ProgressHub> _hubContext;
            private readonly ILogger<ProgressTrackingService> _logger;
            private readonly ConcurrentDictionary<string, ProgressUpdate> _progressStore = new();
            private readonly ConcurrentDictionary<string, WorkflowProgress> _workflowStore = new();
            private readonly ConcurrentDictionary<string, MultiOperationProgress> _operationGroups = new();

        public ProgressTrackingService(
            IHubContext<ProgressHub> hubContext,
            ILogger<ProgressTrackingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public string StartOperation(string operationType, string description)
        {
            var operationId = Guid.NewGuid().ToString();
            var progress = new ProgressUpdate
            {
                OperationId = operationId,
                OperationType = operationType,
                ProgressPercentage = 0,
                CurrentStep = "Starting",
                StatusMessage = description,
                IsComplete = false,
                HasError = false,
                Timestamp = DateTime.UtcNow
            };

            _progressStore.TryAdd(operationId, progress);
            _logger.LogInformation("Started operation {OperationId} of type {OperationType}: {Description}", 
                operationId, operationType, description);

            // Broadcast initial progress
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(operationId).SendAsync("ProgressUpdate", progress);
            });

            return operationId;
        }

        public void UpdateProgress(string operationId, int percentage, string statusMessage, long? itemsProcessed = null, long? totalItems = null)
        {
            if (!_progressStore.TryGetValue(operationId, out var progress))
            {
                _logger.LogWarning("Progress update for unknown operation {OperationId}", operationId);
                return;
            }

            progress.ProgressPercentage = Math.Min(100, Math.Max(0, percentage));
            progress.StatusMessage = statusMessage;
            progress.ItemsProcessed = itemsProcessed;
            progress.TotalItems = totalItems;
            progress.Timestamp = DateTime.UtcNow;

            _logger.LogDebug("Progress update for {OperationId} ({OperationType}): {Percentage}% - {Message}", 
                operationId, progress.OperationType, percentage, statusMessage);

            // Broadcast progress update
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(operationId).SendAsync("ProgressUpdate", progress);
            });
        }

        public void UpdateProgress(ProgressUpdate progressUpdate)
        {
            if (!_progressStore.TryGetValue(progressUpdate.OperationId, out var progress))
            {
                _progressStore.TryAdd(progressUpdate.OperationId, progressUpdate);
                progress = progressUpdate;
            }
            else
            {
                // Update existing progress
                progress.ProgressPercentage = progressUpdate.ProgressPercentage;
                progress.StatusMessage = progressUpdate.StatusMessage;
                progress.CurrentStep = progressUpdate.CurrentStep;
                progress.ItemsProcessed = progressUpdate.ItemsProcessed;
                progress.TotalItems = progressUpdate.TotalItems;
                progress.Timestamp = DateTime.UtcNow;
                progress.IsComplete = progressUpdate.IsComplete;
                progress.HasError = progressUpdate.HasError;
                progress.ErrorMessage = progressUpdate.ErrorMessage;
            }

            _logger.LogDebug("Progress update for {OperationId}: {Percentage}%", progressUpdate.OperationId, progressUpdate.ProgressPercentage);

            // Broadcast progress update
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(progressUpdate.OperationId).SendAsync("ProgressUpdate", progress);
            });
        }

        public void CompleteOperation(string operationId, bool success, string? message = null, string? errorMessage = null)
        {
            if (!_progressStore.TryGetValue(operationId, out var progress))
            {
                _logger.LogWarning("Completion for unknown operation {OperationId}", operationId);
                return;
            }

            progress.IsComplete = true;
            progress.ProgressPercentage = 100;
            progress.HasError = !success;
            progress.ErrorMessage = errorMessage;
            progress.StatusMessage = message ?? (success ? "Operation completed successfully" : "Operation failed");
            progress.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Completed operation {OperationId}: {Success} - {Message}", operationId, success, progress.StatusMessage);

            // Broadcast completion
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(operationId).SendAsync("ProgressUpdate", progress);
                
                // Clean up after a delay (keep progress for 5 minutes for retrieval)
                await Task.Delay(TimeSpan.FromMinutes(5));
                _progressStore.TryRemove(operationId, out _);
            });
        }

        public ProgressUpdate? GetProgress(string operationId)
        {
            _progressStore.TryGetValue(operationId, out var progress);
            return progress;
        }

        public void CancelOperation(string operationId)
        {
            if (_progressStore.TryGetValue(operationId, out var progress))
            {
                progress.IsComplete = true;
                progress.HasError = true;
                progress.ErrorMessage = "Operation cancelled by user";
                progress.StatusMessage = "Operation cancelled";
                progress.Timestamp = DateTime.UtcNow;

                _ = Task.Run(async () =>
                {
                    await _hubContext.Clients.Group(operationId).SendAsync("ProgressUpdate", progress);
                });

                _logger.LogInformation("Cancelled operation {OperationId}", operationId);
            }
        }

        // Workflow support methods
        public string StartWorkflow(string workflowName, List<string> stepNames, List<int>? stepWeights = null)
        {
            var workflowId = Guid.NewGuid().ToString();
            var steps = stepNames.Select((name, index) => new OperationProgress
            {
                StepId = Guid.NewGuid().ToString(),
                StepName = name,
                Status = OperationStatus.Pending,
                ProgressPercentage = 0,
                Weight = stepWeights?[index] ?? 1
            }).ToList();

            var workflow = new WorkflowProgress
            {
                OperationId = workflowId,
                WorkflowName = workflowName,
                Steps = steps,
                TotalSteps = steps.Count,
                CurrentStepIndex = -1,
                Status = WorkflowStatus.Running,
                StartedAt = DateTime.UtcNow,
                ProgressPercentage = 0
            };

            _workflowStore.TryAdd(workflowId, workflow);
            _logger.LogInformation("Started workflow {WorkflowId}: {WorkflowName} with {StepCount} steps", 
                workflowId, workflowName, steps.Count);

            // Broadcast workflow start
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(workflowId).SendAsync("WorkflowProgress", workflow);
            });

            return workflowId;
        }

        public void UpdateWorkflowStep(string workflowId, string stepId, string operationId, int progress, string message, long? itemsProcessed = null, long? totalItems = null)
        {
            if (!_workflowStore.TryGetValue(workflowId, out var workflow))
            {
                _logger.LogWarning("Workflow step update for unknown workflow {WorkflowId}", workflowId);
                return;
            }

            var step = workflow.Steps.FirstOrDefault(s => s.StepId == stepId);
            if (step == null)
            {
                _logger.LogWarning("Step {StepId} not found in workflow {WorkflowId}", stepId, workflowId);
                return;
            }

            step.Status = OperationStatus.Running;
            step.ProgressPercentage = Math.Min(100, Math.Max(0, progress));
            step.StatusMessage = message;
            step.ItemsProcessed = itemsProcessed;
            step.TotalItems = totalItems;
            step.StartedAt ??= DateTime.UtcNow;

            // Update current step index if this is the first running step
            if (workflow.CurrentStepIndex == -1 || workflow.Steps[workflow.CurrentStepIndex].Status != OperationStatus.Running)
            {
                workflow.CurrentStepIndex = workflow.Steps.IndexOf(step);
                workflow.CurrentStepName = step.StepName;
            }

            // Calculate overall progress
            int totalWeight = workflow.Steps.Sum(s => s.Weight);
            int completedWeight = workflow.Steps
                .Where(s => s.Status == OperationStatus.Completed)
                .Sum(s => s.Weight);
            int currentStepWeight = (int)(step.Weight * (progress / 100.0));
            workflow.OverallProgress = totalWeight > 0 
                ? (int)(((completedWeight + currentStepWeight) / (double)totalWeight) * 100)
                : 0;
            workflow.ProgressPercentage = workflow.OverallProgress;

            _logger.LogDebug("Workflow {WorkflowId} step {StepName}: {Progress}% - {Message}", 
                workflowId, step.StepName, progress, message);

            // Broadcast workflow progress
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(workflowId).SendAsync("WorkflowProgress", workflow);
            });
        }

        public void CompleteWorkflowStep(string workflowId, string stepId, bool success, string? errorMessage = null)
        {
            if (!_workflowStore.TryGetValue(workflowId, out var workflow))
            {
                _logger.LogWarning("Workflow step completion for unknown workflow {WorkflowId}", workflowId);
                return;
            }

            var step = workflow.Steps.FirstOrDefault(s => s.StepId == stepId);
            if (step == null)
            {
                _logger.LogWarning("Step {StepId} not found in workflow {WorkflowId}", stepId, workflowId);
                return;
            }

            step.Status = success ? OperationStatus.Completed : OperationStatus.Failed;
            step.ProgressPercentage = 100;
            step.CompletedAt = DateTime.UtcNow;
            if (!success)
            {
                step.ErrorMessage = errorMessage;
            }
            workflow.CompletedSteps = workflow.Steps.Count(s => s.Status == OperationStatus.Completed);
            workflow.FailedSteps = workflow.Steps.Count(s => s.Status == OperationStatus.Failed);

            // Move to next step
            var currentIndex = workflow.Steps.IndexOf(step);
            if (currentIndex < workflow.Steps.Count - 1)
            {
                workflow.CurrentStepIndex = currentIndex + 1;
                workflow.CurrentStepName = workflow.Steps[workflow.CurrentStepIndex].StepName;
            }

            // Recalculate overall progress
            int totalWeight = workflow.Steps.Sum(s => s.Weight);
            int completedWeight = workflow.Steps
                .Where(s => s.Status == OperationStatus.Completed)
                .Sum(s => s.Weight);
            workflow.OverallProgress = totalWeight > 0 
                ? (int)((completedWeight / (double)totalWeight) * 100)
                : 0;
            workflow.ProgressPercentage = workflow.OverallProgress;

            _logger.LogInformation("Completed workflow {WorkflowId} step {StepName}: {Success}", 
                workflowId, step.StepName, success);

            // Broadcast workflow progress
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(workflowId).SendAsync("WorkflowProgress", workflow);
            });
        }

        public void CompleteWorkflow(string workflowId, bool success, string? errorMessage = null)
        {
            if (!_workflowStore.TryGetValue(workflowId, out var workflow))
            {
                _logger.LogWarning("Workflow completion for unknown workflow {WorkflowId}", workflowId);
                return;
            }

            workflow.Status = success ? WorkflowStatus.Completed : WorkflowStatus.Failed;
            workflow.CompletedAt = DateTime.UtcNow;
            workflow.ProgressPercentage = 100;
            workflow.OverallProgress = 100;
            if (!success)
            {
                workflow.ErrorMessage = errorMessage;
            }

            _logger.LogInformation("Completed workflow {WorkflowId}: {Success} - {Message}", 
                workflowId, success, errorMessage ?? "Completed");

            // Broadcast workflow completion
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(workflowId).SendAsync("WorkflowProgress", workflow);
                
                // Clean up after a delay
                await Task.Delay(TimeSpan.FromMinutes(10));
                _workflowStore.TryRemove(workflowId, out _);
            });
        }

        public WorkflowProgress? GetWorkflowProgress(string workflowId)
        {
            _workflowStore.TryGetValue(workflowId, out var workflow);
            return workflow;
        }

        // Multi-operation support methods
        public void RegisterOperationGroup(string groupId, List<string> operationIds, string groupName = "")
        {
            var operations = new Dictionary<string, ProgressUpdate>();
            foreach (var id in operationIds)
            {
                if (_progressStore.TryGetValue(id, out var progress))
                {
                    operations[id] = progress;
                }
                else
                {
                    // Create a placeholder progress for operations that haven't started yet
                    operations[id] = new ProgressUpdate
                    {
                        OperationId = id,
                        OperationType = "Unknown",
                        ProgressPercentage = 0,
                        StatusMessage = "Not started",
                        IsComplete = false,
                        HasError = false,
                        Timestamp = DateTime.UtcNow
                    };
                }
            }

            var group = new MultiOperationProgress
            {
                OperationId = groupId,
                GroupName = groupName,
                Operations = operations,
                TotalOperations = operations.Count,
                CompletedOperations = operations.Count(o => o.Value.IsComplete && !o.Value.HasError),
                RunningOperations = operations.Count(o => !o.Value.IsComplete),
                FailedOperations = operations.Count(o => o.Value.IsComplete && o.Value.HasError),
                OverallProgress = operations.Any() 
                    ? (int)operations.Average(o => o.Value.ProgressPercentage)
                    : 0
            };

            _operationGroups.TryAdd(groupId, group);
            _logger.LogInformation("Registered operation group {GroupId}: {GroupName} with {Count} operations", 
                groupId, groupName, operationIds.Count);

            // Broadcast group registration
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(groupId).SendAsync("MultiOperationProgress", group);
            });
        }

        public void UpdateOperationGroup(string groupId)
        {
            if (!_operationGroups.TryGetValue(groupId, out var group))
            {
                _logger.LogWarning("Update for unknown operation group {GroupId}", groupId);
                return;
            }

            // Refresh all operations from progress store
            foreach (var kvp in group.Operations.ToList())
            {
                var operationId = kvp.Key;
                if (_progressStore.TryGetValue(operationId, out var progress))
                {
                    group.Operations[operationId] = progress;
                }
            }

            // Recalculate group statistics
            group.CompletedOperations = group.Operations.Count(o => o.Value.IsComplete && !o.Value.HasError);
            group.RunningOperations = group.Operations.Count(o => !o.Value.IsComplete);
            group.FailedOperations = group.Operations.Count(o => o.Value.IsComplete && o.Value.HasError);
            group.OverallProgress = group.Operations.Any()
                ? (int)group.Operations.Average(o => o.Value.ProgressPercentage)
                : 0;

            // Broadcast group update
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.Group(groupId).SendAsync("MultiOperationProgress", group);
            });
        }

        public MultiOperationProgress? GetOperationGroupProgress(string groupId)
        {
            _operationGroups.TryGetValue(groupId, out var group);
            return group;
        }
    }
}
