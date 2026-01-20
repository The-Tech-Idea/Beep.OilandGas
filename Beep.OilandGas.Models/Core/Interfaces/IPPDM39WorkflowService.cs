using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for executing workflow pipelines with progress tracking
    /// Platform-agnostic interface for workflow execution
    /// </summary>
    public interface IPPDM39WorkflowService
    {
        Task<WorkflowExecutionResult> ExecuteWorkflowAsync(WorkflowDefinition workflow, string? operationId = null);
        Task<WorkflowDefinition> SaveWorkflowDefinitionAsync(WorkflowDefinition workflow);
        Task<List<WorkflowDefinition>> GetWorkflowsAsync(string? fieldId = null, string? phase = null);
        Task<List<WorkflowDefinition>> GetWorkflowsByPhaseAsync(string phase, string? fieldId = null);
        Task SaveWorkflowExecutionResultAsync(WorkflowExecutionResult result);
        Task<List<WorkflowExecutionResult>> GetWorkflowExecutionHistoryAsync(string workflowId, int limit = 50);
    }
}



