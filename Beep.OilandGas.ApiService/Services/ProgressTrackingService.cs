using Beep.OilandGas.Models.Data.Company;
using System;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Repository;
using Beep.OilandGas.ApiService.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace Beep.OilandGas.ApiService.Services
{
    /// <summary>
    /// SignalR hub for progress updates
    /// </summary>
    [Authorize]
    public class ProgressHub(RepositoryTenantContext tenant, CompanyProgressState progress) : Hub
    {
        private string Group(string resource, CompanyChannelKind kind) => CompanyHubGroups.Name(new(
            (tenant.Company ?? throw new HubException("A validated company is required.")).Scope, kind, resource));
        public async Task JoinOperationGroup(string operationId)
        {
            RequireOwned(operationId, CompanyChannelKind.Operation);
            await Groups.AddToGroupAsync(Context.ConnectionId, Group(operationId, CompanyChannelKind.Operation), Context.ConnectionAborted);
        }

        public async Task LeaveOperationGroup(string operationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(operationId, CompanyChannelKind.Operation), Context.ConnectionAborted);
        }

        public async Task JoinWorkflowGroup(string workflowId)
        {
            RequireOwned(workflowId, CompanyChannelKind.Workflow);
            await Groups.AddToGroupAsync(Context.ConnectionId, Group(workflowId, CompanyChannelKind.Workflow), Context.ConnectionAborted);
        }

        public async Task LeaveWorkflowGroup(string workflowId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Group(workflowId, CompanyChannelKind.Workflow), Context.ConnectionAborted);
        }

        private void RequireOwned(string resource, CompanyChannelKind kind)
        {
            var company = tenant.Company ?? throw new HubException("A validated company is required.");
            if (!progress.CanRead(company, new(company.Scope, kind, resource)))
                throw new HubException("The progress resource is unavailable to this account and company.");
        }
    }

    /// <summary>
    /// Implementation of progress tracking service
    /// SignalR-based implementation for web/API scenarios
    /// </summary>
    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly CompanyProgressState _state;
        private readonly RepositoryTenantContext _tenant;
        private readonly ILogger<ProgressTrackingService> _logger;
        private readonly IBackgroundOperationQueue? _queue;
        private object _gate => _state.Gate;
        private TimeProvider _clock => _state.Clock;
        private ConcurrentDictionary<CompanyProgressKey, ProgressUpdate> _progressStore => _state.Operations;
        private ConcurrentDictionary<CompanyProgressKey, WorkflowProgress> _workflowStore => _state.Workflows;
        private ConcurrentDictionary<CompanyProgressKey, MultiOperationProgress> _operationGroups => _state.Groups;
        private static readonly JsonSerializerOptions SnapshotOptions = new(JsonSerializerDefaults.Web);

        public ProgressTrackingService(CompanyProgressState state, RepositoryTenantContext tenant,
            ILogger<ProgressTrackingService> logger, IBackgroundOperationQueue? queue = null)
        { _state = state; _tenant = tenant; _logger = logger; _queue = queue; }

        private RepositoryCompanyContext Company => _tenant.Company ??
            throw new InvalidOperationException("Progress requires a validated company and account.");
        private CompanyProgressKey Key(string resource)
        {
            _ = CompanyHubGroups.Name(new(Company.Scope, CompanyChannelKind.Operation, resource));
            return new(Company.Scope, Company.UserId, resource);
        }

        public string StartOperation(string operationType, string description)
        {
            lock (_gate)
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
                    Timestamp = _clock.GetUtcNow().UtcDateTime
                };

                _progressStore.TryAdd(Key(operationId), progress);
                _state.Register(Company, CompanyChannelKind.Operation, operationId);
                _logger.LogInformation("Started operation {OperationId} of type {OperationType}: {Description}",
                    operationId, operationType, description);

                // Broadcast initial progress
                Publish(operationId, "ProgressUpdate", progress);

                return operationId;
            }
        }

        public void UpdateProgress(string operationId, int percentage, string statusMessage, long? itemsProcessed = null, long? totalItems = null)
        {
            lock (_gate)
            {
                if (!CanRead(operationId, CompanyChannelKind.Operation)) return;
                if (!_progressStore.TryGetValue(Key(operationId), out var progress))
                {
                    _logger.LogWarning("Progress update for unknown operation {OperationId}", operationId);
                    return;
                }

                if (progress.IsComplete) return;
                progress.ProgressPercentage = Math.Min(100, Math.Max(0, percentage));
                progress.StatusMessage = statusMessage;
                progress.ItemsProcessed = itemsProcessed;
                progress.TotalItems = totalItems;
                progress.Timestamp = _clock.GetUtcNow().UtcDateTime;

                _logger.LogDebug("Progress update for {OperationId} ({OperationType}): {Percentage}% - {Message}",
                    operationId, progress.OperationType, percentage, statusMessage);

                // Broadcast progress update
                Publish(operationId, "ProgressUpdate", progress);
            }
        }

        public void UpdateProgress(ProgressUpdate progressUpdate)
        {
            lock (_gate)
            {
                if (!CanRead(progressUpdate.OperationId, CompanyChannelKind.Operation)) return;
                if (!_progressStore.TryGetValue(Key(progressUpdate.OperationId), out var progress))
                {
                    _logger.LogWarning("Progress update for unknown operation {OperationId}", progressUpdate.OperationId);
                    return;
                }
                else
                {
                    if (progress.IsComplete) return;
                    // Update existing progress
                    progress.ProgressPercentage = progressUpdate.ProgressPercentage;
                    progress.StatusMessage = progressUpdate.StatusMessage;
                    progress.CurrentStep = progressUpdate.CurrentStep;
                    progress.ItemsProcessed = progressUpdate.ItemsProcessed;
                    progress.TotalItems = progressUpdate.TotalItems;
                    progress.Timestamp = _clock.GetUtcNow().UtcDateTime;
                    progress.IsComplete = progressUpdate.IsComplete;
                    progress.HasError = progressUpdate.HasError;
                    progress.ErrorMessage = progressUpdate.ErrorMessage;
                }

                _logger.LogDebug("Progress update for {OperationId}: {Percentage}%", progressUpdate.OperationId, progressUpdate.ProgressPercentage);

                // Broadcast progress update
                Publish(progressUpdate.OperationId, "ProgressUpdate", progress);
            }
        }

        public void CompleteOperation(string operationId, bool success, string? message = null, string? errorMessage = null)
        {
            lock (_gate)
            {
                if (!CanRead(operationId, CompanyChannelKind.Operation)) return;
                if (!_progressStore.TryGetValue(Key(operationId), out var progress))
                {
                    _logger.LogWarning("Completion for unknown operation {OperationId}", operationId);
                    return;
                }

                if (progress.IsComplete) return;
                progress.IsComplete = true;
                progress.ProgressPercentage = 100;
                progress.HasError = !success;
                progress.ErrorMessage = errorMessage;
                progress.StatusMessage = message ?? (success ? "Operation completed successfully" : "Operation failed");
                progress.Timestamp = _clock.GetUtcNow().UtcDateTime;

                _logger.LogInformation("Completed operation {OperationId}: {Success} - {Message}", operationId, success, progress.StatusMessage);

                // Broadcast completion
                Publish(operationId, "ProgressUpdate", progress);
            }
        }

        public ProgressUpdate? GetProgress(string operationId)
        {
            lock (_gate)
            {
                if (!CanRead(operationId, CompanyChannelKind.Operation)) return null;
                PruneExpired();
                _progressStore.TryGetValue(Key(operationId), out var progress);
                var job = _queue?.GetStatus(CsvImportJob.QueueKey(operationId));
                if (progress != null && job?.State is BackgroundOperationState.Failed or BackgroundOperationState.Cancelled)
                {
                    if (!progress.IsComplete) progress.Timestamp = _clock.GetUtcNow().UtcDateTime;
                    progress.IsComplete = true;
                    progress.HasError = true;
                    progress.ErrorMessage = job.Error ?? "Import stopped before completion. Review imported rows before retrying.";
                    progress.StatusMessage = "Import " + job.State;
                }
                return Snapshot(progress);
            }
        }

        public void CancelOperation(string operationId)
        {
            lock (_gate)
            {
                if (CanRead(operationId, CompanyChannelKind.Workflow) && _workflowStore.TryGetValue(Key(operationId), out var workflow))
                {
                    if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Failed or WorkflowStatus.Cancelled) return;
                    workflow.Status = WorkflowStatus.Cancelled;
                    workflow.CompletedAt = _clock.GetUtcNow().UtcDateTime;
                    workflow.IsComplete = true;
                    workflow.HasError = true;
                    workflow.ErrorMessage = "Workflow tracking cancelled by user";
                    foreach (var step in workflow.Steps.Where(step => step.Status is OperationStatus.Pending or OperationStatus.Running))
                    { step.Status = OperationStatus.Cancelled; step.CompletedAt = workflow.CompletedAt; }
                    Publish(operationId, "WorkflowProgress", workflow);
                    return;
                }
                if (!CanRead(operationId, CompanyChannelKind.Operation)) return;
                if (_progressStore.TryGetValue(Key(operationId), out var progress))
                {
                    if (progress.IsComplete) return;
                    progress.IsComplete = true;
                    progress.HasError = true;
                    progress.ErrorMessage = "Operation cancelled by user";
                    progress.StatusMessage = "Operation cancelled";
                    progress.Timestamp = _clock.GetUtcNow().UtcDateTime;

                    Publish(operationId, "ProgressUpdate", progress);

                    _logger.LogInformation("Cancelled operation {OperationId}", operationId);
                }
            }
        }

        // Workflow support methods
        public string StartWorkflow(string workflowName, List<string> stepNames, List<int>? stepWeights = null)
        {
            lock (_gate)
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
                    StartedAt = _clock.GetUtcNow().UtcDateTime,
                    ProgressPercentage = 0
                };

                _workflowStore.TryAdd(Key(workflowId), workflow);
                _state.Register(Company, CompanyChannelKind.Workflow, workflowId);
                _logger.LogInformation("Started workflow {WorkflowId}: {WorkflowName} with {StepCount} steps",
                    workflowId, workflowName, steps.Count);

                // Broadcast workflow start
                Publish(workflowId, "WorkflowProgress", workflow);

                return workflowId;
            }
        }

        public void UpdateWorkflowStep(string workflowId, string stepId, string operationId, int progress, string message, long? itemsProcessed = null, long? totalItems = null)
        {
            lock (_gate)
            {
                if (!CanRead(workflowId, CompanyChannelKind.Workflow)) return;
                if (!_workflowStore.TryGetValue(Key(workflowId), out var workflow))
                {
                    _logger.LogWarning("Workflow step update for unknown workflow {WorkflowId}", workflowId);
                    return;
                }

                if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Failed or WorkflowStatus.Cancelled) return;

                var step = workflow.Steps.FirstOrDefault(s => s.StepId == stepId);
                if (step == null)
                {
                    _logger.LogWarning("Step {StepId} not found in workflow {WorkflowId}", stepId, workflowId);
                    return;
                }

                if (step.Status is OperationStatus.Completed or OperationStatus.Failed or OperationStatus.Cancelled) return;

                step.Status = OperationStatus.Running;
                step.ProgressPercentage = Math.Min(100, Math.Max(0, progress));
                step.StatusMessage = message;
                step.ItemsProcessed = itemsProcessed;
                step.TotalItems = totalItems;
                step.StartedAt ??= _clock.GetUtcNow().UtcDateTime;

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
                Publish(workflowId, "WorkflowProgress", workflow);
            }
        }

        public void CompleteWorkflowStep(string workflowId, string stepId, bool success, string? errorMessage = null)
        {
            lock (_gate)
            {
                if (!CanRead(workflowId, CompanyChannelKind.Workflow)) return;
                if (!_workflowStore.TryGetValue(Key(workflowId), out var workflow))
                {
                    _logger.LogWarning("Workflow step completion for unknown workflow {WorkflowId}", workflowId);
                    return;
                }

                if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Failed or WorkflowStatus.Cancelled) return;

                var step = workflow.Steps.FirstOrDefault(s => s.StepId == stepId);
                if (step == null)
                {
                    _logger.LogWarning("Step {StepId} not found in workflow {WorkflowId}", stepId, workflowId);
                    return;
                }

                if (step.Status is OperationStatus.Completed or OperationStatus.Failed or OperationStatus.Cancelled) return;

                step.Status = success ? OperationStatus.Completed : OperationStatus.Failed;
                step.ProgressPercentage = 100;
                step.CompletedAt = _clock.GetUtcNow().UtcDateTime;
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
                Publish(workflowId, "WorkflowProgress", workflow);
            }
        }

        public void CompleteWorkflow(string workflowId, bool success, string? errorMessage = null)
        {
            lock (_gate)
            {
                if (!CanRead(workflowId, CompanyChannelKind.Workflow)) return;
                if (!_workflowStore.TryGetValue(Key(workflowId), out var workflow))
                {
                    _logger.LogWarning("Workflow completion for unknown workflow {WorkflowId}", workflowId);
                    return;
                }

                if (workflow.Status is WorkflowStatus.Completed or WorkflowStatus.Failed or WorkflowStatus.Cancelled) return;

                workflow.Status = success ? WorkflowStatus.Completed : WorkflowStatus.Failed;
                workflow.IsComplete = true;
                workflow.HasError = !success;
                workflow.CompletedAt = _clock.GetUtcNow().UtcDateTime;
                workflow.ProgressPercentage = 100;
                workflow.OverallProgress = 100;
                if (!success)
                {
                    workflow.ErrorMessage = errorMessage;
                }

                _logger.LogInformation("Completed workflow {WorkflowId}: {Success} - {Message}",
                    workflowId, success, errorMessage ?? "Completed");

                // Broadcast workflow completion
                Publish(workflowId, "WorkflowProgress", workflow);
            }
        }

        public WorkflowProgress? GetWorkflowProgress(string workflowId)
        {
            lock (_gate)
            {
                if (!CanRead(workflowId, CompanyChannelKind.Workflow)) return null;
                PruneExpired();
                _workflowStore.TryGetValue(Key(workflowId), out var workflow);
                return Snapshot(workflow);
            }
        }

        // Multi-operation support methods
        public void RegisterOperationGroup(string groupId, List<string> operationIds, string groupName = "")
        {
            lock (_gate)
            {
                var operations = new Dictionary<string, ProgressUpdate>();
                foreach (var id in operationIds)
                {
                    if (CanRead(id, CompanyChannelKind.Operation) && _progressStore.TryGetValue(Key(id), out var progress))
                    {
                        operations[id] = progress;
                    }
                    else
                    {
                        throw new InvalidOperationException("All grouped operations must belong to the current account and company.");
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

                if (_progressStore.ContainsKey(Key(groupId)) || !_operationGroups.TryAdd(Key(groupId), group))
                    throw new InvalidOperationException("The operation group identifier is already in use.");
                _state.Register(Company, CompanyChannelKind.Operation, groupId);
                _logger.LogInformation("Registered operation group {GroupId}: {GroupName} with {Count} operations",
                    groupId, groupName, operationIds.Count);

                // Broadcast group registration
                group.Timestamp = _clock.GetUtcNow().UtcDateTime;
                group.IsComplete = group.Operations.Values.All(operation => operation.IsComplete);
                Publish(groupId, "MultiOperationProgress", group);
            }
        }

        public void UpdateOperationGroup(string groupId)
        {
            lock (_gate)
            {
                if (!CanRead(groupId, CompanyChannelKind.Operation)) return;
                if (!_operationGroups.TryGetValue(Key(groupId), out var group))
                {
                    _logger.LogWarning("Update for unknown operation group {GroupId}", groupId);
                    return;
                }

                // Refresh all operations from progress store
                foreach (var kvp in group.Operations.ToList())
                {
                    var operationId = kvp.Key;
                    if (_progressStore.TryGetValue(Key(operationId), out var progress))
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
                group.Timestamp = _clock.GetUtcNow().UtcDateTime;
                group.IsComplete = group.Operations.Values.All(operation => operation.IsComplete);
                Publish(groupId, "MultiOperationProgress", group);
            }
        }

        public MultiOperationProgress? GetOperationGroupProgress(string groupId)
        {
            lock (_gate)
            {
                if (!CanRead(groupId, CompanyChannelKind.Operation)) return null;
                _operationGroups.TryGetValue(Key(groupId), out var group);
                return Snapshot(group);
            }
        }

        private static T? Snapshot<T>(T? value) where T : class => value == null ? null :
            JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToElement(value, SnapshotOptions), SnapshotOptions);

        private void Publish<T>(string resource, string method, T value)
        {
            var kind = method == "WorkflowProgress" ? CompanyChannelKind.Workflow : CompanyChannelKind.Operation;
            _state.Notifications.Writer.TryWrite(new(Company, kind, resource,
                JsonSerializer.SerializeToElement(value, SnapshotOptions)));
        }

        private bool CanRead(string resource, CompanyChannelKind kind) => _state.CanRead(Company, new(Company.Scope, kind, resource));

        private void PruneExpired() => _state.PruneExpired();
    }
}
