using Beep.OilandGas.Models.DTOs.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for tracking and broadcasting progress of long-running operations
    /// Platform-agnostic interface for progress tracking
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
}
