using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

public sealed class BoundProcessService(Func<Task<string>> resolveConnection, Func<string, IProcessService> createService)
    : IProcessService
{
    // A fresh fixed-target service keeps nested calls and repository caches on one database.
    private async Task<T> RunAsync<T>(Func<IProcessService, Task<T>> operation)
        => await operation(await CreateBoundServiceAsync());

    public async Task<IProcessService> CreateBoundServiceAsync()
    {
        var connection = await resolveConnection();
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Process operations require a bound LIFECYCLE database.");
        return createService(connection);
    }

    public Task<ProcessDefinition?> GetProcessDefinitionAsync(string processId) => RunAsync(s => s.GetProcessDefinitionAsync(processId));
    public Task<List<ProcessDefinition>> GetProcessDefinitionsByTypeAsync(string processType) => RunAsync(s => s.GetProcessDefinitionsByTypeAsync(processType));
    public Task<ProcessDefinition> CreateProcessDefinitionAsync(ProcessDefinition definition, string userId) => RunAsync(s => s.CreateProcessDefinitionAsync(definition, userId));
    public Task<ProcessDefinition> UpdateProcessDefinitionAsync(string processId, ProcessDefinition definition, string userId) => RunAsync(s => s.UpdateProcessDefinitionAsync(processId, definition, userId));
    public Task<bool> DeleteProcessDefinitionAsync(string processId, string userId) => RunAsync(s => s.DeleteProcessDefinitionAsync(processId, userId));
    public Task<ProcessInstance> StartProcessAsync(string processId, string entityId, string entityType, string fieldId, string userId) => RunAsync(s => s.StartProcessAsync(processId, entityId, entityType, fieldId, userId));
    public Task<ProcessInstance?> GetProcessInstanceAsync(string instanceId) => RunAsync(s => s.GetProcessInstanceAsync(instanceId));
    public Task<List<ProcessInstance>> GetProcessInstancesForEntityAsync(string entityId, string entityType) => RunAsync(s => s.GetProcessInstancesForEntityAsync(entityId, entityType));
    public Task<List<ProcessInstance>> GetProcessInstancesForFieldAsync(string fieldId) => RunAsync(s => s.GetProcessInstancesForFieldAsync(fieldId));
    public Task<ProcessInstance?> GetCurrentProcessForEntityAsync(string entityId, string entityType) => RunAsync(s => s.GetCurrentProcessForEntityAsync(entityId, entityType));
    public Task<bool> CancelProcessAsync(string instanceId, string reason, string userId) => RunAsync(s => s.CancelProcessAsync(instanceId, reason, userId));
    public Task<bool> ExecuteStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData, string userId) => RunAsync(s => s.ExecuteStepAsync(instanceId, stepId, stepData, userId));
    public Task<bool> CompleteStepAsync(string instanceId, string stepId, string outcome, string userId) => RunAsync(s => s.CompleteStepAsync(instanceId, stepId, outcome, userId));
    public Task<bool> SkipStepAsync(string instanceId, string stepId, string reason, string userId) => RunAsync(s => s.SkipStepAsync(instanceId, stepId, reason, userId));
    public Task<bool> RollbackStepAsync(string instanceId, string stepId, string reason, string userId) => RunAsync(s => s.RollbackStepAsync(instanceId, stepId, reason, userId));
    public Task<bool> TransitionStateAsync(string instanceId, string targetState, string userId) => RunAsync(s => s.TransitionStateAsync(instanceId, targetState, userId));
    public Task<List<string>> GetAvailableTransitionsAsync(string instanceId) => RunAsync(s => s.GetAvailableTransitionsAsync(instanceId));
    public Task<bool> CanTransitionAsync(string instanceId, string targetState) => RunAsync(s => s.CanTransitionAsync(instanceId, targetState));
    public Task<ProcessStatus> GetProcessStatusAsync(string instanceId) => RunAsync(s => s.GetProcessStatusAsync(instanceId));
    public Task<List<ProcessStepInstance>> GetCompletedStepsAsync(string instanceId) => RunAsync(s => s.GetCompletedStepsAsync(instanceId));
    public Task<List<ProcessStepInstance>> GetPendingStepsAsync(string instanceId) => RunAsync(s => s.GetPendingStepsAsync(instanceId));
    public Task<List<ProcessHistoryEntry>> GetProcessHistoryAsync(string instanceId) => RunAsync(s => s.GetProcessHistoryAsync(instanceId));
    public Task<ProcessHistoryEntry> AddHistoryEntryAsync(string instanceId, ProcessHistoryEntry entry) => RunAsync(s => s.AddHistoryEntryAsync(instanceId, entry));
    public Task<ValidationResult> ValidateStepAsync(string instanceId, string stepId, PROCESS_STEP_DATA stepData) => RunAsync(s => s.ValidateStepAsync(instanceId, stepId, stepData));
    public Task<bool> ValidateProcessCompletionAsync(string instanceId) => RunAsync(s => s.ValidateProcessCompletionAsync(instanceId));
    public Task<bool> RequestApprovalAsync(string stepInstanceId, string approvalType, string requestedBy, string userId) => RunAsync(s => s.RequestApprovalAsync(stepInstanceId, approvalType, requestedBy, userId));
    public Task<bool> ApproveStepAsync(string approvalId, string approvedBy, string notes, string userId) => RunAsync(s => s.ApproveStepAsync(approvalId, approvedBy, notes, userId));
    public Task<bool> RejectStepAsync(string approvalId, string rejectedBy, string reason, string userId) => RunAsync(s => s.RejectStepAsync(approvalId, rejectedBy, reason, userId));
}
