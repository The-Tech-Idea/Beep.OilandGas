using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Payload to execute a single script or a set of scripts
    /// </summary>
    public class ScriptExecPayload : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to use for script execution. If null, the current configured connection will be used.
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Single script name to execute (if not executing multiple)
        /// </summary>
        private string? ScriptNameValue;

        public string? ScriptName

        {

            get { return this.ScriptNameValue; }

            set { SetProperty(ref ScriptNameValue, value); }

        }

        /// <summary>
        /// When true, execute all discovered scripts for the database type
        /// </summary>
        private bool ExecuteAllValue = false;

        public bool ExecuteAll

        {

            get { return this.ExecuteAllValue; }

            set { SetProperty(ref ExecuteAllValue, value); }

        }

        /// <summary>
        /// Optional explicit list of script file names to execute
        /// </summary>
        private List<string>? ScriptNamesValue;

        public List<string>? ScriptNames

        {

            get { return this.ScriptNamesValue; }

            set { SetProperty(ref ScriptNamesValue, value); }

        }

        /// <summary>
        /// If the operation originated from an existing connection name, set it here so logs can reference the original
        /// </summary>
        private string? OriginalConnectionNameValue;

        public string? OriginalConnectionName

        {

            get { return this.OriginalConnectionNameValue; }

            set { SetProperty(ref OriginalConnectionNameValue, value); }

        }

        /// <summary>
        /// Continue execution when one script fails
        /// </summary>
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }

        /// <summary>
        /// Enable rollback on failure if supported by the provider
        /// </summary>
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

        }

        /// <summary>
        /// When true, drop objects if they exist before creating (where applicable)
        /// </summary>
        private bool DropIfExistsValue = false;

        public bool DropIfExists

        {

            get { return this.DropIfExistsValue; }

            set { SetProperty(ref DropIfExistsValue, value); }

        }

        /// <summary>
        /// Optional operation id for progress tracking
        /// </summary>
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }

        /// <summary>
        /// Optional user id performing the operation
        /// </summary>
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Payload to execute multiple or all scripts with execution options
    /// </summary>
    public class AllScriptsExecPayload : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to use for the operation
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// Execute all discovered scripts
        /// </summary>
        private bool ExecuteAllValue = true;

        public bool ExecuteAll

        {

            get { return this.ExecuteAllValue; }

            set { SetProperty(ref ExecuteAllValue, value); }

        }

        /// <summary>
        /// Optional subset of script names to execute instead of all
        /// </summary>
        private List<string>? ScriptNamesValue;

        public List<string>? ScriptNames

        {

            get { return this.ScriptNamesValue; }

            set { SetProperty(ref ScriptNamesValue, value); }

        }

        /// <summary>
        /// Execute consolidated scripts (TAB, PK, etc.)
        /// </summary>
        private bool ExecuteConsolidatedScriptsValue = true;

        public bool ExecuteConsolidatedScripts

        {

            get { return this.ExecuteConsolidatedScriptsValue; }

            set { SetProperty(ref ExecuteConsolidatedScriptsValue, value); }

        }

        /// <summary>
        /// Execute individual table scripts
        /// </summary>
        private bool ExecuteIndividualScriptsValue = true;

        public bool ExecuteIndividualScripts

        {

            get { return this.ExecuteIndividualScriptsValue; }

            set { SetProperty(ref ExecuteIndividualScriptsValue, value); }

        }

        /// <summary>
        /// Execute optional/supporting scripts
        /// </summary>
        private bool ExecuteOptionalScriptsValue = false;

        public bool ExecuteOptionalScripts

        {

            get { return this.ExecuteOptionalScriptsValue; }

            set { SetProperty(ref ExecuteOptionalScriptsValue, value); }

        }

        /// <summary>
        /// Continue execution when a script fails
        /// </summary>
        private bool ContinueOnErrorValue = false;

        public bool ContinueOnError

        {

            get { return this.ContinueOnErrorValue; }

            set { SetProperty(ref ContinueOnErrorValue, value); }

        }

        /// <summary>
        /// Enable rollback on failure if supported
        /// </summary>
        private bool EnableRollbackValue = false;

        public bool EnableRollback

        {

            get { return this.EnableRollbackValue; }

            set { SetProperty(ref EnableRollbackValue, value); }

        }

        /// <summary>
        /// Optional operation id for progress tracking
        /// </summary>
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }

        /// <summary>
        /// Optional user id performing the operation
        /// </summary>
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result returned when executing multiple scripts
    /// </summary>
    public class AllScriptsExecutionResult : ModelEntityBase
    {
        /// <summary>
        /// Unique execution identifier
        /// </summary>
        private string ExecutionIdValue = string.Empty;

        public string ExecutionId

        {

            get { return this.ExecutionIdValue; }

            set { SetProperty(ref ExecutionIdValue, value); }

        }

        /// <summary>
        /// Overall success flag
        /// </summary>
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }

        /// <summary>
        /// Start time of the operation
        /// </summary>
        private DateTime StartTimeValue;

        public DateTime StartTime

        {

            get { return this.StartTimeValue; }

            set { SetProperty(ref StartTimeValue, value); }

        }

        /// <summary>
        /// End time of the operation
        /// </summary>
        private DateTime EndTimeValue;

        public DateTime EndTime

        {

            get { return this.EndTimeValue; }

            set { SetProperty(ref EndTimeValue, value); }

        }

        /// <summary>
        /// Total duration of the operation
        /// </summary>
        public TimeSpan TotalDuration => EndTime - StartTime;

        /// <summary>
        /// Individual script execution results (DTO)
        /// </summary>
        private List<ScriptExecutionResult> ResultsValue = new List<ScriptExecutionResult>();

        public List<ScriptExecutionResult> Results

        {

            get { return this.ResultsValue; }

            set { SetProperty(ref ResultsValue, value); }

        }

        /// <summary>
        /// Total number of scripts attempted
        /// </summary>
        private int TotalScriptsValue;

        public int TotalScripts

        {

            get { return this.TotalScriptsValue; }

            set { SetProperty(ref TotalScriptsValue, value); }

        }

        /// <summary>
        /// Number of successful scripts
        /// </summary>
        private int SuccessfulScriptsValue;

        public int SuccessfulScripts

        {

            get { return this.SuccessfulScriptsValue; }

            set { SetProperty(ref SuccessfulScriptsValue, value); }

        }

        /// <summary>
        /// Number of failed scripts
        /// </summary>
        private int FailedScriptsValue;

        public int FailedScripts

        {

            get { return this.FailedScriptsValue; }

            set { SetProperty(ref FailedScriptsValue, value); }

        }

        /// <summary>
        /// Flag indicating whether all scripts succeeded
        /// </summary>
        private bool AllSucceededValue;

        public bool AllSucceeded

        {

            get { return this.AllSucceededValue; }

            set { SetProperty(ref AllSucceededValue, value); }

        }

        /// <summary>
        /// Optional summary dictionary
        /// </summary>
        public Dictionary<string, object>? Summary { get; set; }

        /// <summary>
        /// Optional log file path where execution log was written
        /// </summary>
        private string? LogFilePathValue;

        public string? LogFilePath

        {

            get { return this.LogFilePathValue; }

            set { SetProperty(ref LogFilePathValue, value); }

        }
    }

    /// <summary>
    /// Request to save or update a connection
    /// </summary>
    public class SaveConnectionRequest : ModelEntityBase
    {
        /// <summary>
        /// Connection properties to save
        /// </summary>
        private ConnectionProperties? ConnectionValue;

        [Required]
        public ConnectionProperties? Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        /// <summary>
        /// If true, set this connection as the current active connection
        /// </summary>
        private bool SetAsCurrentValue = false;

        public bool SetAsCurrent

        {

            get { return this.SetAsCurrentValue; }

            set { SetProperty(ref SetAsCurrentValue, value); }

        }

        /// <summary>
        /// After saving, optionally test the connection
        /// </summary>
        private bool TestAfterSaveValue = true;

        public bool TestAfterSave

        {

            get { return this.TestAfterSaveValue; }

            set { SetProperty(ref TestAfterSaveValue, value); }

        }

        /// <summary>
        /// After saving, optionally open the connection in the editor
        /// </summary>
        private bool OpenAfterSaveValue = false;

        public bool OpenAfterSave

        {

            get { return this.OpenAfterSaveValue; }

            set { SetProperty(ref OpenAfterSaveValue, value); }

        }

        /// <summary>
        /// Original connection name when updating an existing connection
        /// </summary>
        private string? OriginalConnectionNameValue;

        public string? OriginalConnectionName

        {

            get { return this.OriginalConnectionNameValue; }

            set { SetProperty(ref OriginalConnectionNameValue, value); }

        }

        /// <summary>
        /// User performing the operation
        /// </summary>
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result returned after saving a connection
    /// </summary>
    public class SaveConnectionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Request to set the current active connection
    /// </summary>
    public class SetCurrentDatabaseRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result for setting the current active connection
    /// </summary>
    public class SetCurrentDatabaseResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }

        /// <summary>
        /// If true, client must log out to apply the change
        /// </summary>
        private bool RequiresLogoutValue = false;

        public bool RequiresLogout

        {

            get { return this.RequiresLogoutValue; }

            set { SetProperty(ref RequiresLogoutValue, value); }

        }
    }

    /// <summary>
    /// Drop database or schema request
    /// </summary>
    public class DropDatabaseRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
        private bool DropIfExistsValue = true;

        public bool DropIfExists

        {

            get { return this.DropIfExistsValue; }

            set { SetProperty(ref DropIfExistsValue, value); }

        }
        private bool ForceValue = false;

        public bool Force

        {

            get { return this.ForceValue; }

            set { SetProperty(ref ForceValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of a drop database request
    /// </summary>
    public class DropDatabaseResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
    }

    /// <summary>
    /// Recreate database request
    /// </summary>
    public class RecreateDatabaseRequest : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        [Required]
        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
        private bool BackupFirstValue = false;

        public bool BackupFirst

        {

            get { return this.BackupFirstValue; }

            set { SetProperty(ref BackupFirstValue, value); }

        }
        private string? BackupPathValue;

        public string? BackupPath

        {

            get { return this.BackupPathValue; }

            set { SetProperty(ref BackupPathValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of recreate database request
    /// </summary>
    public class RecreateDatabaseResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? BackupFileValue;

        public string? BackupFile

        {

            get { return this.BackupFileValue; }

            set { SetProperty(ref BackupFileValue, value); }

        }
    }

    /// <summary>
    /// Copy database (ETL) request
    /// </summary>
    public class CopyDatabaseRequest : ModelEntityBase
    {
        private string SourceConnectionNameValue = string.Empty;

        [Required]
        public string SourceConnectionName

        {

            get { return this.SourceConnectionNameValue; }

            set { SetProperty(ref SourceConnectionNameValue, value); }

        }
        private string TargetConnectionNameValue = string.Empty;

        [Required]
        public string TargetConnectionName

        {

            get { return this.TargetConnectionNameValue; }

            set { SetProperty(ref TargetConnectionNameValue, value); }

        }
        private string? SourceSchemaValue;

        public string? SourceSchema

        {

            get { return this.SourceSchemaValue; }

            set { SetProperty(ref SourceSchemaValue, value); }

        }
        private string? TargetSchemaValue;

        public string? TargetSchema

        {

            get { return this.TargetSchemaValue; }

            set { SetProperty(ref TargetSchemaValue, value); }

        }
        private bool CopyStructureOnlyValue = false;

        public bool CopyStructureOnly

        {

            get { return this.CopyStructureOnlyValue; }

            set { SetProperty(ref CopyStructureOnlyValue, value); }

        }
        private bool CopyDataValue = true;

        public bool CopyData

        {

            get { return this.CopyDataValue; }

            set { SetProperty(ref CopyDataValue, value); }

        }
        private bool TruncateTargetTablesValue = false;

        public bool TruncateTargetTables

        {

            get { return this.TruncateTargetTablesValue; }

            set { SetProperty(ref TruncateTargetTablesValue, value); }

        }
        private List<string>? TableNamesValue;

        public List<string>? TableNames

        {

            get { return this.TableNamesValue; }

            set { SetProperty(ref TableNamesValue, value); }

        }
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of a copy database operation
    /// </summary>
    public class CopyDatabaseResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string? OperationIdValue;

        public string? OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }

    /// <summary>
    /// Request to import LOV (List of Values) data
    /// </summary>
    public class LOVImportRequest : ModelEntityBase
    {
        private string FilePathValue = string.Empty;

        [Required(ErrorMessage = "FilePath is required")]
        public string FilePath

        {

            get { return this.FilePathValue; }

            set { SetProperty(ref FilePathValue, value); }

        }
        private string? TargetTableValue;

        public string? TargetTable

        {

            get { return this.TargetTableValue; }

            set { SetProperty(ref TargetTableValue, value); }

        }
        public Dictionary<string, string>? ColumnMapping { get; set; }
        private bool? SkipExistingValue;

        public bool? SkipExisting

        {

            get { return this.SkipExistingValue; }

            set { SetProperty(ref SkipExistingValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private string? ConnectionNameValue;

        public string? ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
    }

    /// <summary>
    /// Request to check if a database driver is available
    /// </summary>
    public class CheckDriverRequest : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        [Required(ErrorMessage = "DatabaseType is required")]
        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
    }

    /// <summary>
    /// Request to execute a database script
    /// </summary>
    public class ExecuteScriptRequest : ModelEntityBase
    {
        private ConnectionProperties ConnectionValue = null!;

        [Required(ErrorMessage = "Connection is required")]
        public ConnectionProperties Connection

        {

            get { return this.ConnectionValue; }

            set { SetProperty(ref ConnectionValue, value); }

        }

        private string ScriptNameValue = string.Empty;


        [Required(ErrorMessage = "ScriptName is required")]
        public string ScriptName


        {


            get { return this.ScriptNameValue; }


            set { SetProperty(ref ScriptNameValue, value); }


        }
    }
}






