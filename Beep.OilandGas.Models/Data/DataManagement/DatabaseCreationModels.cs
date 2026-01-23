using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.DataManagement
{
    /// <summary>
    /// Request to create a PPDM database
    /// </summary>
    public class CreateDatabaseRequest : ModelEntityBase
    {
        /// <summary>
        /// Connection configuration
        /// </summary>
        private ConnectionProperties ConnectionValue = null!;

        public ConnectionProperties Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Database creation options
        /// </summary>
        private DatabaseCreationOptions OptionsValue = null!;

        public DatabaseCreationOptions Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }
    }

    /// <summary>
    /// Database creation options DTO for API
    /// </summary>
    public class DatabaseCreationOptions : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        } // SqlServer, Oracle, etc.
        private string ScriptsPathValue = string.Empty;

        public string ScriptsPath

        {

            get { return this.ScriptsPathValue; }

            set { SetProperty(ref ScriptsPathValue, value); }

        }
        private List<string>? CategoriesValue;

        public List<string>? Categories

        {

            get { return this.CategoriesValue; }

            set { SetProperty(ref CategoriesValue, value); }

        }
        private List<string>? ScriptTypesValue;

        public List<string>? ScriptTypes

        {

            get { return this.ScriptTypesValue; }

            set { SetProperty(ref ScriptTypesValue, value); }

        } // TAB, PK, FK, etc.
        private bool EnableLoggingValue = true;

        public bool EnableLogging

        {

            get { return this.EnableLoggingValue; }

            set { SetProperty(ref EnableLoggingValue, value); }

        }
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

        }
        private bool ExecuteConsolidatedScriptsValue = true;

        public bool ExecuteConsolidatedScripts

        {

            get { return this.ExecuteConsolidatedScriptsValue; }

            set { SetProperty(ref ExecuteConsolidatedScriptsValue, value); }

        }
        private bool ExecuteIndividualScriptsValue = true;

        public bool ExecuteIndividualScripts

        {

            get { return this.ExecuteIndividualScriptsValue; }

            set { SetProperty(ref ExecuteIndividualScriptsValue, value); }

        }
        private bool ExecuteOptionalScriptsValue = false;

        public bool ExecuteOptionalScripts

        {

            get { return this.ExecuteOptionalScriptsValue; }

            set { SetProperty(ref ExecuteOptionalScriptsValue, value); }

        }
        private bool ValidateDependenciesValue = true;

        public bool ValidateDependencies

        {

            get { return this.ValidateDependenciesValue; }

            set { SetProperty(ref ValidateDependenciesValue, value); }

        }
        private bool EnableParallelExecutionValue = false;

        public bool EnableParallelExecution

        {

            get { return this.EnableParallelExecutionValue; }

            set { SetProperty(ref EnableParallelExecutionValue, value); }

        }
        private int? MaxParallelTasksValue;

        public int? MaxParallelTasks

        {

            get { return this.MaxParallelTasksValue; }

            set { SetProperty(ref MaxParallelTasksValue, value); }

        }
        private string? ExecutionIdValue;

        public string? ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }
    }

    /// <summary>
    /// Database creation result DTO for API
    /// </summary>
    public class DatabaseCreationResult : ModelEntityBase
    {
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime EndTimeValue;

        public DateTime EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }
        private TimeSpan TotalDurationValue;

        public TimeSpan TotalDuration

        {

            get { return this.TotalDurationValue; }

            set { SetProperty(ref TotalDurationValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int SuccessfulScriptsValue;

        public int SuccessfulScripts

        {

            get { return this.SuccessfulScriptsValue; }

            set { SetProperty(ref SuccessfulScriptsValue, value); }

        }
        private int FailedScriptsValue;

        public int FailedScripts

        {

            get { return this.FailedScriptsValue; }

            set { SetProperty(ref FailedScriptsValue, value); }

        }
        private int SkippedScriptsValue;

        public int SkippedScripts

        {

            get { return this.SkippedScriptsValue; }

            set { SetProperty(ref SkippedScriptsValue, value); }

        }
        private List<ScriptExecutionResult> ScriptResultsValue = new List<ScriptExecutionResult>();

        public List<ScriptExecutionResult> ScriptResults

        {

            get { return this.ScriptResultsValue; }

            set { SetProperty(ref ScriptResultsValue, value); }

        }
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
    }

    /// <summary>
    /// Script execution result DTO for API
    /// </summary>
    public class ScriptExecutionResult : ModelEntityBase
    {
        private string ScriptFileNameValue = string.Empty;

        public string ScriptFileName

        {

            get { return this.ScriptFileNameValue; }

            set { SetProperty(ref ScriptFileNameValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime EndTimeValue;

        public DateTime EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }
        private TimeSpan DurationValue;

        public TimeSpan Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private int? RowsAffectedValue;

        public int? RowsAffected

        {

            get { return this.RowsAffectedValue; }

            set { SetProperty(ref RowsAffectedValue, value); }

        }
        private string? ExecutionLogValue;

        public string? ExecutionLog

        {

            get { return this.ExecutionLogValue; }

            set { SetProperty(ref ExecutionLogValue, value); }

        }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Script execution progress DTO for API
    /// </summary>
    public class ScriptExecutionProgressInfo : ModelEntityBase
    {
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }
        private int CompletedScriptsValue;

        public int CompletedScripts

        {

            get { return this.CompletedScriptsValue; }

            set { SetProperty(ref CompletedScriptsValue, value); }

        }
        private int FailedScriptsValue;

        public int FailedScripts

        {

            get { return this.FailedScriptsValue; }

            set { SetProperty(ref FailedScriptsValue, value); }

        }
        private int SkippedScriptsValue;

        public int SkippedScripts

        {

            get { return this.SkippedScriptsValue; }

            set { SetProperty(ref SkippedScriptsValue, value); }

        }
        private decimal ProgressPercentageValue;

        public decimal ProgressPercentage

        {

            get { return this.ProgressPercentageValue; }

            set { SetProperty(ref ProgressPercentageValue, value); }

        }
        private string CurrentScriptValue = string.Empty;

        public string CurrentScript

        {

            get { return this.CurrentScriptValue; }

            set { SetProperty(ref CurrentScriptValue, value); }

        }
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }
        private DateTime? EstimatedCompletionTimeValue;

        public DateTime? EstimatedCompletionTime

        {

            get { return this.EstimatedCompletionTimeValue; }

            set { SetProperty(ref EstimatedCompletionTimeValue, value); }

        }
        private string StatusValue = "Not Started";

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    /// <summary>
    /// Script info DTO for API
    /// </summary>
    public class ScriptInfo : ModelEntityBase
    {
        private string FileNameValue = string.Empty;

        public string FileName

        {

            get { return this.FileNameValue; }

            set { SetProperty(ref FileNameValue, value); }

        }
        private string FullPathValue = string.Empty;

        public string FullPath

        {

            get { return this.FullPathValue; }

            set { SetProperty(ref FullPathValue, value); }

        }
        private string RelativePathValue = string.Empty;

        public string RelativePath

        {

            get { return this.RelativePathValue; }

            set { SetProperty(ref RelativePathValue, value); }

        }
        private string ScriptTypeValue = string.Empty;

        public string ScriptType

        {

            get { return this.ScriptTypeValue; }

            set { SetProperty(ref ScriptTypeValue, value); }

        }
        private string? TableNameValue;

        public string? TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private string? ModuleValue;

        public string? Module

        {

            get { return this.ModuleValue; }

            set { SetProperty(ref ModuleValue, value); }

        }
        private string? SubjectAreaValue;

        public string? SubjectArea

        {

            get { return this.SubjectAreaValue; }

            set { SetProperty(ref SubjectAreaValue, value); }

        }
        private bool IsConsolidatedValue;

        public bool IsConsolidated

        {

            get { return this.IsConsolidatedValue; }

            set { SetProperty(ref IsConsolidatedValue, value); }

        }
        private bool IsMandatoryValue;

        public bool IsMandatory

        {

            get { return this.IsMandatoryValue; }

            set { SetProperty(ref IsMandatoryValue, value); }

        }
        private bool IsOptionalValue;

        public bool IsOptional

        {

            get { return this.IsOptionalValue; }

            set { SetProperty(ref IsOptionalValue, value); }

        }
        private long FileSizeValue;

        public long FileSize

        {

            get { return this.FileSizeValue; }

            set { SetProperty(ref FileSizeValue, value); }

        }
        private DateTime LastModifiedValue;

        public DateTime LastModified

        {

            get { return this.LastModifiedValue; }

            set { SetProperty(ref LastModifiedValue, value); }

        }
        private int ExecutionOrderValue;

        public int ExecutionOrder

        {

            get { return this.ExecutionOrderValue; }

            set { SetProperty(ref ExecutionOrderValue, value); }

        }
        private List<string> DependenciesValue = new List<string>();

        public List<string> Dependencies

        {

            get { return this.DependenciesValue; }

            set { SetProperty(ref DependenciesValue, value); }

        }
        private string? CategoryValue;

        public string? Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
        private object NameValue;

        public object Name

        {

            get { return this.NameValue; }

            set { SetProperty(ref NameValue, value); }

        }
    }
}








