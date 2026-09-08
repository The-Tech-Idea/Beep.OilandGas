using System;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Services
{
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
    /// SignalR-based implementation for web/API scenarios
    /// </summary>
    public class ProgressTrackingService : BackgroundService, IProgressTrackingService
        {
            private readonly IHubContext<ProgressHub> _hubContext;
            private readonly ILogger<ProgressTrackingService> _logger;
            private readonly IBackgroundOperationQueue? _queue;
            private readonly object _gate = new();
            private readonly TimeProvider _clock;
            private static readonly JsonSerializerOptions SnapshotOptions = new(JsonSerializerDefaults.Web);
            private sealed record Notification(string Group, string Method, JsonElement Payload);
            private readonly Channel<Notification> _notifications = Channel.CreateBounded<Notification>(new BoundedChannelOptions(256)
            { SingleReader = true, FullMode = BoundedChannelFullMode.DropOldest });
            private readonly ConcurrentDictionary<string, ProgressUpdate> _progressStore = new();
            private readonly ConcurrentDictionary<string, WorkflowProgress> _workflowStore = new();
            private readonly ConcurrentDictionary<string, MultiOperationProgress> _operationGroups = new();

        public ProgressTrackingService(
            IHubContext<ProgressHub> hubContext,
            ILogger<ProgressTrackingService> logger,
            IBackgroundOperationQueue? queue = null,
            TimeProvider? clock = null)
        {
            _hubContext = hubContext;
            _logger = logger;
            _queue = queue;
            _clock = clock ?? TimeProvider.System;
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

                _progressStore.TryAdd(operationId, progress);
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
                if (!_progressStore.TryGetValue(operationId, out var progress))
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
                if (!_progressStore.TryGetValue(progressUpdate.OperationId, out var progress))
                {
                    progress = Snapshot(progressUpdate)!;
                    progress.Timestamp = _clock.GetUtcNow().UtcDateTime;
                    _progressStore.TryAdd(progressUpdate.OperationId, progress);
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
                if (!_progressStore.TryGetValue(operationId, out var progress))
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
                PruneExpired();
                _progressStore.TryGetValue(operationId, out var progress);
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
                if (_progressStore.TryGetValue(operationId, out var progress))
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

                _workflowStore.TryAdd(workflowId, workflow);
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
                if (!_workflowStore.TryGetValue(workflowId, out var workflow))
                {
                    _logger.LogWarning("Workflow completion for unknown workflow {WorkflowId}", workflowId);
                    return;
                }

                workflow.Status = success ? WorkflowStatus.Completed : WorkflowStatus.Failed;
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
                PruneExpired();
                _workflowStore.TryGetValue(workflowId, out var workflow);
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
                            Timestamp = _clock.GetUtcNow().UtcDateTime
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
                Publish(groupId, "MultiOperationProgress", group);
            }
        }

        public void UpdateOperationGroup(string groupId)
        {
            lock (_gate)
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
                Publish(groupId, "MultiOperationProgress", group);
            }
        }

        public MultiOperationProgress? GetOperationGroupProgress(string groupId)
        {
            lock (_gate)
            {
                _operationGroups.TryGetValue(groupId, out var group);
                return Snapshot(group);
            }
        }

        private static T? Snapshot<T>(T? value) where T : class => value == null ? null :
            JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToElement(value, SnapshotOptions), SnapshotOptions);

        private void Publish<T>(string group, string method, T value)
        {
            _notifications.Writer.TryWrite(new(group, method, JsonSerializer.SerializeToElement(value, SnapshotOptions)));
        }

        private void PruneExpired()
        {
            lock (_gate)
            {
                var now = _clock.GetUtcNow().UtcDateTime;
                foreach (var item in _progressStore)
                    if (item.Value.IsComplete && now - item.Value.Timestamp >= TimeSpan.FromMinutes(5))
                        _progressStore.TryRemove(item.Key, out _);
                foreach (var item in _workflowStore)
                    if (item.Value.CompletedAt is DateTime completed && now - completed >= TimeSpan.FromMinutes(10))
                        _workflowStore.TryRemove(item.Key, out _);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            Task.WhenAll(BroadcastAsync(stoppingToken), CleanupAsync(stoppingToken));

        private async Task BroadcastAsync(CancellationToken token)
        {
            try
            {
                await foreach (var item in _notifications.Reader.ReadAllAsync(token))
                {
                    token.ThrowIfCancellationRequested();
                    try { await _hubContext.Clients.Group(item.Group).SendAsync(item.Method, item.Payload, token); }
                    catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                    catch (Exception ex) { _logger.LogWarning(ex, "Progress broadcast failed for {Group}", item.Group); }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            finally { while (_notifications.Reader.TryRead(out _)) { } }
        }

        private async Task CleanupAsync(CancellationToken token)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
            try { while (await timer.WaitForNextTickAsync(token)) PruneExpired(); }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        }

        public override Task StopAsync(CancellationToken token)
        {
            _notifications.Writer.TryComplete();
            return base.StopAsync(token);
        }
    }
}
