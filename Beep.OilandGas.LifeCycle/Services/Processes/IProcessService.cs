using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.LifeCycle.Services.Processes
{
    /// <summary>
    /// Interface for process/workflow management service
    /// </summary>
    public interface IProcessService
    {
        // Process Definition Management
        Task<ProcessDefinition> GetProcessDefinitionAsync(string processId);
        Task<List<ProcessDefinition>> GetProcessDefinitionsByTypeAsync(string processType);
        Task<ProcessDefinition> CreateProcessDefinitionAsync(ProcessDefinition definition, string userId);
        Task<ProcessDefinition> UpdateProcessDefinitionAsync(string processId, ProcessDefinition definition, string userId);
        Task<bool> DeleteProcessDefinitionAsync(string processId, string userId);

        // Process Instance Management
        Task<ProcessInstance> StartProcessAsync(string processId, string entityId, string entityType, string fieldId, string userId);
        Task<ProcessInstance> GetProcessInstanceAsync(string instanceId);
        Task<List<ProcessInstance>> GetProcessInstancesForEntityAsync(string entityId, string entityType);
        Task<ProcessInstance> GetCurrentProcessForEntityAsync(string entityId, string entityType);
        Task<bool> CancelProcessAsync(string instanceId, string reason, string userId);

        // Process Execution
        Task<bool> ExecuteStepAsync(string instanceId, string stepId, Dictionary<string, object> stepData, string userId);
        Task<bool> CompleteStepAsync(string instanceId, string stepId, string outcome, string userId);
        Task<bool> SkipStepAsync(string instanceId, string stepId, string reason, string userId);
        Task<bool> RollbackStepAsync(string instanceId, string stepId, string reason, string userId);

        // State Management
        Task<bool> TransitionStateAsync(string instanceId, string targetState, string userId);
        Task<List<string>> GetAvailableTransitionsAsync(string instanceId);
        Task<bool> CanTransitionAsync(string instanceId, string targetState);

        // Process Status
        Task<ProcessStatus> GetProcessStatusAsync(string instanceId);
        Task<List<ProcessStepInstance>> GetCompletedStepsAsync(string instanceId);
        Task<List<ProcessStepInstance>> GetPendingStepsAsync(string instanceId);

        // Process History
        Task<List<ProcessHistoryEntry>> GetProcessHistoryAsync(string instanceId);
        Task<ProcessHistoryEntry> AddHistoryEntryAsync(string instanceId, ProcessHistoryEntry entry);

        // Validation
        Task<ValidationResult> ValidateStepAsync(string instanceId, string stepId, Dictionary<string, object> stepData);
        Task<bool> ValidateProcessCompletionAsync(string instanceId);

        // Approvals
        Task<bool> RequestApprovalAsync(string stepInstanceId, string approvalType, string requestedBy, string userId);
        Task<bool> ApproveStepAsync(string approvalId, string approvedBy, string notes, string userId);
        Task<bool> RejectStepAsync(string approvalId, string rejectedBy, string reason, string userId);
    }
}

