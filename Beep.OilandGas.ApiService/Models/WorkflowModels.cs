using System;
using System.Collections.Generic;

namespace Beep.OilandGas.ApiService.Models
{
    /// <summary>
    /// Workflow step definition
    /// </summary>
    public class WorkflowStep
    {
        public string StepId { get; set; } = Guid.NewGuid().ToString();
        public string StepName { get; set; } = string.Empty;
        public string? DependsOn { get; set; } // StepId this step depends on
        public bool CanRunInParallel { get; set; } = false;
        public int EstimatedWeight { get; set; } = 1; // Weight for progress calculation
        public string OperationType { get; set; } = string.Empty; // "ImportCsv", "Validate", "QualityCheck", etc.
        public Dictionary<string, object>? Parameters { get; set; }
    }

    /// <summary>
    /// Workflow definition
    /// </summary>
    public class WorkflowDefinition
    {
        public string WorkflowId { get; set; } = Guid.NewGuid().ToString();
        public string WorkflowName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public bool StopOnError { get; set; } = true;
        public string? ConnectionName { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Workflow execution request
    /// </summary>
    public class WorkflowExecutionRequest
    {
        public WorkflowDefinition Workflow { get; set; } = new WorkflowDefinition();
        public string? OperationId { get; set; }
    }

    /// <summary>
    /// Workflow execution result
    /// </summary>
    public class WorkflowExecutionResult
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, object>? StepResults { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
    }
}
