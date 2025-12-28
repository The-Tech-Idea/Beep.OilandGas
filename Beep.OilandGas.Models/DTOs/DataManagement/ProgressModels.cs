using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.DataManagement
{
    /// <summary>
    /// Progress update for long-running operations
    /// </summary>
    public class ProgressUpdate
    {
        public string OperationId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty; // "ScriptExecution", "CopyDatabase", "CreateSchema", etc.
        public int ProgressPercentage { get; set; }
        public string CurrentStep { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public long? ItemsProcessed { get; set; }
        public long? TotalItems { get; set; }
        public bool IsComplete { get; set; }
        public bool HasError { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Script execution progress
    /// </summary>
    public class ScriptExecutionProgress : ProgressUpdate
    {
        public string ScriptName { get; set; } = string.Empty;
        public int CurrentScriptIndex { get; set; }
        public int TotalScripts { get; set; }
        public int StatementsExecuted { get; set; }
        public int TotalStatements { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    /// <summary>
    /// Database copy progress
    /// </summary>
    public class DatabaseCopyProgress : ProgressUpdate
    {
        public string SourceConnection { get; set; } = string.Empty;
        public string TargetConnection { get; set; } = string.Empty;
        public string CurrentTable { get; set; } = string.Empty;
        public int TablesCopied { get; set; }
        public int TotalTables { get; set; }
        public long RowsCopied { get; set; }
        public long TotalRows { get; set; }
        public TimeSpan? ElapsedTime { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
    }

    /// <summary>
    /// Schema creation progress
    /// </summary>
    public class SchemaCreationProgress : ProgressUpdate
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public new string CurrentStep { get; set; } = string.Empty; // "Creating", "Verifying", "Complete"
    }

    /// <summary>
    /// Database drop/recreate progress
    /// </summary>
    public class DatabaseOperationProgress : ProgressUpdate
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty; // "Drop", "Recreate"
        public new string CurrentStep { get; set; } = string.Empty;
    }

    /// <summary>
    /// Operation status for workflow steps
    /// </summary>
    public enum OperationStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled,
        Skipped
    }

    /// <summary>
    /// Workflow status
    /// </summary>
    public enum WorkflowStatus
    {
        NotStarted,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Progress for individual operations in a workflow pipeline
    /// </summary>
    public class OperationProgress
    {
        public string StepName { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public OperationStatus Status { get; set; } = OperationStatus.Pending;
        public long? ItemsProcessed { get; set; }
        public long? TotalItems { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public int Weight { get; set; } = 1; // Weight for progress aggregation (default 1)
    }

    /// <summary>
    /// Progress for workflow pipelines (chained operations)
    /// </summary>
    public class WorkflowProgress : ProgressUpdate
    {
        public string WorkflowName { get; set; } = string.Empty;
        public List<OperationProgress> Steps { get; set; } = new List<OperationProgress>();
        public int CurrentStepIndex { get; set; } = -1;
        public int OverallProgress { get; set; }
        public WorkflowStatus Status { get; set; } = WorkflowStatus.NotStarted;
        public int TotalSteps { get; set; }
        public int CompletedSteps { get; set; }
        public int FailedSteps { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? CurrentStepName { get; set; }
    }

    /// <summary>
    /// Progress for multiple concurrent operations
    /// </summary>
    public class MultiOperationProgress : ProgressUpdate
    {
        public Dictionary<string, ProgressUpdate> Operations { get; set; } = new Dictionary<string, ProgressUpdate>();
        public int TotalOperations { get; set; }
        public int CompletedOperations { get; set; }
        public int FailedOperations { get; set; }
        public int RunningOperations { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public int OverallProgress { get; set; }
    }
}

