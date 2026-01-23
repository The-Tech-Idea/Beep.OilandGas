using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Progress update for long-running operations
    /// </summary>
    public class ProgressUpdate : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        } // "ScriptExecution", "CopyDatabase", "CreateSchema", etc.
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentStepValue = string.Empty;

        public string CurrentStep

        {

            get { return this.CurrentStepValue; }

            set { SetProperty(ref CurrentStepValue, value); }

        }
        private string StatusMessageValue = string.Empty;

        public string StatusMessage

        {

            get { return this.StatusMessageValue; }

            set { SetProperty(ref StatusMessageValue, value); }

        }
        private long? ItemsProcessedValue;

        public long? ItemsProcessed

        {

            get { return this.ItemsProcessedValue; }

            set { SetProperty(ref ItemsProcessedValue, value); }

        }
        private long? TotalItemsValue;

        public long? TotalItems

        {

            get { return this.TotalItemsValue; }

            set { SetProperty(ref TotalItemsValue, value); }

        }
        private bool IsCompleteValue;

        public bool IsComplete

        {

            get { return this.IsCompleteValue; }

            set { SetProperty(ref IsCompleteValue, value); }

        }
        private bool HasErrorValue;

        public bool HasError

        {

            get { return this.HasErrorValue; }

            set { SetProperty(ref HasErrorValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private DateTime TimestampValue = DateTime.UtcNow;

        public DateTime Timestamp

        {

            get { return this.TimestampValue; }

            set { SetProperty(ref TimestampValue, value); }

        }
    }

    /// <summary>
    /// Script execution progress
    /// </summary>
    public class ScriptExecutionProgress : ProgressUpdate
    {
        private string ScriptNameValue = string.Empty;

        public string ScriptName

        {

            get { return this.ScriptNameValue; }

            set { SetProperty(ref ScriptNameValue, value); }

        }
        private int CurrentScriptIndexValue;

        public int CurrentScriptIndex

        {

            get { return this.CurrentScriptIndexValue; }

            set { SetProperty(ref CurrentScriptIndexValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int StatementsExecutedValue;

        public int StatementsExecuted

        {

            get { return this.StatementsExecutedValue; }

            set { SetProperty(ref StatementsExecutedValue, value); }

        }
        private int TotalStatementsValue;

        public int TotalStatements

        {

            get { return this.TotalStatementsValue; }

            set { SetProperty(ref TotalStatementsValue, value); }

        }
        private TimeSpan? ElapsedTimeValue;

        public TimeSpan? ElapsedTime

        {

            get { return this.ElapsedTimeValue; }

            set { SetProperty(ref ElapsedTimeValue, value); }

        }
        private TimeSpan? EstimatedTimeRemainingValue;

        public TimeSpan? EstimatedTimeRemaining

        {

            get { return this.EstimatedTimeRemainingValue; }

            set { SetProperty(ref EstimatedTimeRemainingValue, value); }

        }
    }

    /// <summary>
    /// Database copy progress
    /// </summary>
    public class DatabaseCopyProgress : ProgressUpdate
    {
        private string SourceConnectionValue = string.Empty;

        public string SourceConnection

        {

            get { return this.SourceConnectionValue; }

            set { SetProperty(ref SourceConnectionValue, value); }

        }
        private string TargetConnectionValue = string.Empty;

        public string TargetConnection

        {

            get { return this.TargetConnectionValue; }

            set { SetProperty(ref TargetConnectionValue, value); }

        }
        private string CurrentTableValue = string.Empty;

        public string CurrentTable

        {

            get { return this.CurrentTableValue; }

            set { SetProperty(ref CurrentTableValue, value); }

        }
        private int TablesCopiedValue;

        public int TablesCopied

        {

            get { return this.TablesCopiedValue; }

            set { SetProperty(ref TablesCopiedValue, value); }

        }
        private int TotalTablesValue;

        public int TotalTables

        {

            get { return this.TotalTablesValue; }

            set { SetProperty(ref TotalTablesValue, value); }

        }
        private long RowsCopiedValue;

        public long RowsCopied

        {

            get { return this.RowsCopiedValue; }

            set { SetProperty(ref RowsCopiedValue, value); }

        }
        private long TotalRowsValue;

        public long TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private TimeSpan? ElapsedTimeValue;

        public TimeSpan? ElapsedTime

        {

            get { return this.ElapsedTimeValue; }

            set { SetProperty(ref ElapsedTimeValue, value); }

        }
        private TimeSpan? EstimatedTimeRemainingValue;

        public TimeSpan? EstimatedTimeRemaining

        {

            get { return this.EstimatedTimeRemainingValue; }

            set { SetProperty(ref EstimatedTimeRemainingValue, value); }

        }
    }

    /// <summary>
    /// Schema creation progress
    /// </summary>
    public class SchemaCreationProgress : ProgressUpdate
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string SchemaNameValue = string.Empty;

        public string SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
        public new string CurrentStep { get; set; } = string.Empty; // "Creating", "Verifying", "Complete"
    }

    /// <summary>
    /// Database drop/recreate progress
    /// </summary>
    public class DatabaseOperationProgress : ProgressUpdate
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string OperationValue = string.Empty;

        public string Operation

        {

            get { return this.OperationValue; }

            set { SetProperty(ref OperationValue, value); }

        } // "Drop", "Recreate"
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
    public class OperationProgress : ModelEntityBase
    {
        private string StepNameValue = string.Empty;

        public string StepName

        {

            get { return this.StepNameValue; }

            set { SetProperty(ref StepNameValue, value); }

        }
        private string StepIdValue = string.Empty;

        public string StepId

        {

            get { return this.StepIdValue; }

            set { SetProperty(ref StepIdValue, value); }

        }
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private int ProgressPercentageValue;

        public int ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string StatusMessageValue = string.Empty;

        public string StatusMessage

        {

            get { return this.StatusMessageValue; }

            set { SetProperty(ref StatusMessageValue, value); }

        }
        private OperationStatus StatusValue = OperationStatus.Pending;

        public OperationStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private long? ItemsProcessedValue;

        public long? ItemsProcessed

        {

            get { return this.ItemsProcessedValue; }

            set { SetProperty(ref ItemsProcessedValue, value); }

        }
        private long? TotalItemsValue;

        public long? TotalItems

        {

            get { return this.TotalItemsValue; }

            set { SetProperty(ref TotalItemsValue, value); }

        }
        private DateTime? StartedAtValue;

        public DateTime? StartedAt

        {

            get { return this.StartedAtValue; }

            set { SetProperty(ref StartedAtValue, value); }

        }
        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt

        {

            get { return this.CompletedAtValue; }

            set { SetProperty(ref CompletedAtValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private int WeightValue = 1;

        public int Weight

        {

            get { return this.WeightValue; }

            set { SetProperty(ref WeightValue, value); }

        } // Weight for progress aggregation (default 1)
    }

    /// <summary>
    /// Progress for workflow pipelines (chained operations)
    /// </summary>
    public class WorkflowProgress : ProgressUpdate
    {
        private string WorkflowNameValue = string.Empty;

        public string WorkflowName

        {

            get { return this.WorkflowNameValue; }

            set { SetProperty(ref WorkflowNameValue, value); }

        }
        private List<OperationProgress> StepsValue = new List<OperationProgress>();

        public List<OperationProgress> Steps

        {

            get { return this.StepsValue; }

            set { SetProperty(ref StepsValue, value); }

        }
        private int CurrentStepIndexValue = -1;

        public int CurrentStepIndex

        {

            get { return this.CurrentStepIndexValue; }

            set { SetProperty(ref CurrentStepIndexValue, value); }

        }
        private int OverallProgressValue;

        public int OverallProgress

        {

            get { return this.OverallProgressValue; }

            set { SetProperty(ref OverallProgressValue, value); }

        }
        private WorkflowStatus StatusValue = WorkflowStatus.NotStarted;

        public WorkflowStatus Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private int TotalStepsValue;

        public int TotalSteps

        {

            get { return this.TotalStepsValue; }

            set { SetProperty(ref TotalStepsValue, value); }

        }
        private int CompletedStepsValue;

        public int CompletedSteps

        {

            get { return this.CompletedStepsValue; }

            set { SetProperty(ref CompletedStepsValue, value); }

        }
        private int FailedStepsValue;

        public int FailedSteps

        {

            get { return this.FailedStepsValue; }

            set { SetProperty(ref FailedStepsValue, value); }

        }
        private DateTime? StartedAtValue;

        public DateTime? StartedAt

        {

            get { return this.StartedAtValue; }

            set { SetProperty(ref StartedAtValue, value); }

        }
        private DateTime? CompletedAtValue;

        public DateTime? CompletedAt

        {

            get { return this.CompletedAtValue; }

            set { SetProperty(ref CompletedAtValue, value); }

        }
        private string? CurrentStepNameValue;

        public string? CurrentStepName

        {

            get { return this.CurrentStepNameValue; }

            set { SetProperty(ref CurrentStepNameValue, value); }

        }
    }

    /// <summary>
    /// Progress for multiple concurrent operations
    /// </summary>
    public class MultiOperationProgress : ProgressUpdate
    {
        public Dictionary<string, ProgressUpdate> Operations { get; set; } = new Dictionary<string, ProgressUpdate>();
        private int TotalOperationsValue;

        public int TotalOperations

        {

            get { return this.TotalOperationsValue; }

            set { SetProperty(ref TotalOperationsValue, value); }

        }
        private int CompletedOperationsValue;

        public int CompletedOperations

        {

            get { return this.CompletedOperationsValue; }

            set { SetProperty(ref CompletedOperationsValue, value); }

        }
        private int FailedOperationsValue;

        public int FailedOperations

        {

            get { return this.FailedOperationsValue; }

            set { SetProperty(ref FailedOperationsValue, value); }

        }
        private int RunningOperationsValue;

        public int RunningOperations

        {

            get { return this.RunningOperationsValue; }

            set { SetProperty(ref RunningOperationsValue, value); }

        }
        private string GroupNameValue = string.Empty;

        public string GroupName

        {

            get { return this.GroupNameValue; }

            set { SetProperty(ref GroupNameValue, value); }

        }
        private int OverallProgressValue;

        public int OverallProgress

        {

            get { return this.OverallProgressValue; }

            set { SetProperty(ref OverallProgressValue, value); }

        }
    }
}








