using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.DTOs.DataManagement;

namespace Beep.OilandGas.Models.DTOs
{
    /// <summary>
    /// Payload to execute a single script or a set of scripts
    /// </summary>
    public class ScriptExecPayload
    {
        /// <summary>
        /// Connection properties to use for script execution. If null, the current configured connection will be used.
        /// </summary>
        [Required]
        public ConnectionProperties? Connection { get; set; }

        /// <summary>
        /// Single script name to execute (if not executing multiple)
        /// </summary>
        public string? ScriptName { get; set; }

        /// <summary>
        /// When true, execute all discovered scripts for the database type
        /// </summary>
        public bool ExecuteAll { get; set; } = false;

        /// <summary>
        /// Optional explicit list of script file names to execute
        /// </summary>
        public List<string>? ScriptNames { get; set; }

        /// <summary>
        /// If the operation originated from an existing connection name, set it here so logs can reference the original
        /// </summary>
        public string? OriginalConnectionName { get; set; }

        /// <summary>
        /// Continue execution when one script fails
        /// </summary>
        public bool ContinueOnError { get; set; } = false;

        /// <summary>
        /// Enable rollback on failure if supported by the provider
        /// </summary>
        public bool EnableRollback { get; set; } = false;

        /// <summary>
        /// When true, drop objects if they exist before creating (where applicable)
        /// </summary>
        public bool DropIfExists { get; set; } = false;

        /// <summary>
        /// Optional operation id for progress tracking
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// Optional user id performing the operation
        /// </summary>
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Payload to execute multiple or all scripts with execution options
    /// </summary>
    public class AllScriptsExecPayload
    {
        /// <summary>
        /// Connection properties to use for the operation
        /// </summary>
        [Required]
        public ConnectionProperties? Connection { get; set; }

        /// <summary>
        /// Execute all discovered scripts
        /// </summary>
        public bool ExecuteAll { get; set; } = true;

        /// <summary>
        /// Optional subset of script names to execute instead of all
        /// </summary>
        public List<string>? ScriptNames { get; set; }

        /// <summary>
        /// Execute consolidated scripts (TAB, PK, etc.)
        /// </summary>
        public bool ExecuteConsolidatedScripts { get; set; } = true;

        /// <summary>
        /// Execute individual table scripts
        /// </summary>
        public bool ExecuteIndividualScripts { get; set; } = true;

        /// <summary>
        /// Execute optional/supporting scripts
        /// </summary>
        public bool ExecuteOptionalScripts { get; set; } = false;

        /// <summary>
        /// Continue execution when a script fails
        /// </summary>
        public bool ContinueOnError { get; set; } = false;

        /// <summary>
        /// Enable rollback on failure if supported
        /// </summary>
        public bool EnableRollback { get; set; } = false;

        /// <summary>
        /// Optional operation id for progress tracking
        /// </summary>
        public string? OperationId { get; set; }

        /// <summary>
        /// Optional user id performing the operation
        /// </summary>
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result returned when executing multiple scripts
    /// </summary>
    public class AllScriptsExecutionResult
    {
        /// <summary>
        /// Unique execution identifier
        /// </summary>
        public string ExecutionId { get; set; } = string.Empty;

        /// <summary>
        /// Overall success flag
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error message if the operation failed
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Start time of the operation
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the operation
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Total duration of the operation
        /// </summary>
        public TimeSpan TotalDuration => EndTime - StartTime;

        /// <summary>
        /// Individual script execution results (DTO)
        /// </summary>
        public List<ScriptExecutionResultDto> Results { get; set; } = new List<ScriptExecutionResultDto>();

        /// <summary>
        /// Total number of scripts attempted
        /// </summary>
        public int TotalScripts { get; set; }

        /// <summary>
        /// Number of successful scripts
        /// </summary>
        public int SuccessfulScripts { get; set; }

        /// <summary>
        /// Number of failed scripts
        /// </summary>
        public int FailedScripts { get; set; }

        /// <summary>
        /// Flag indicating whether all scripts succeeded
        /// </summary>
        public bool AllSucceeded { get; set; }

        /// <summary>
        /// Optional summary dictionary
        /// </summary>
        public Dictionary<string, object>? Summary { get; set; }

        /// <summary>
        /// Optional log file path where execution log was written
        /// </summary>
        public string? LogFilePath { get; set; }
    }

    /// <summary>
    /// Request to save or update a connection
    /// </summary>
    public class SaveConnectionRequest
    {
        /// <summary>
        /// Connection properties to save
        /// </summary>
        [Required]
        public ConnectionProperties? Connection { get; set; }

        /// <summary>
        /// If true, set this connection as the current active connection
        /// </summary>
        public bool SetAsCurrent { get; set; } = false;

        /// <summary>
        /// After saving, optionally test the connection
        /// </summary>
        public bool TestAfterSave { get; set; } = true;

        /// <summary>
        /// After saving, optionally open the connection in the editor
        /// </summary>
        public bool OpenAfterSave { get; set; } = false;

        /// <summary>
        /// Original connection name when updating an existing connection
        /// </summary>
        public string? OriginalConnectionName { get; set; }

        /// <summary>
        /// User performing the operation
        /// </summary>
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result returned after saving a connection
    /// </summary>
    public class SaveConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ConnectionName { get; set; }
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Request to set the current active connection
    /// </summary>
    public class SetCurrentDatabaseRequest
    {
        [Required]
        public string ConnectionName { get; set; } = string.Empty;
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result for setting the current active connection
    /// </summary>
    public class SetCurrentDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// If true, client must log out to apply the change
        /// </summary>
        public bool RequiresLogout { get; set; } = false;
    }

    /// <summary>
    /// Drop database or schema request
    /// </summary>
    public class DropDatabaseRequest
    {
        [Required]
        public string ConnectionName { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public bool DropIfExists { get; set; } = true;
        public bool Force { get; set; } = false;
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of a drop database request
    /// </summary>
    public class DropDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Recreate database request
    /// </summary>
    public class RecreateDatabaseRequest
    {
        [Required]
        public string ConnectionName { get; set; } = string.Empty;
        public string? SchemaName { get; set; }
        public bool BackupFirst { get; set; } = false;
        public string? BackupPath { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of recreate database request
    /// </summary>
    public class RecreateDatabaseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? BackupFile { get; set; }
    }

    /// <summary>
    /// Copy database (ETL) request
    /// </summary>
    public class CopyDatabaseRequest
    {
        [Required]
        public string SourceConnectionName { get; set; } = string.Empty;
        [Required]
        public string TargetConnectionName { get; set; } = string.Empty;
        public string? SourceSchema { get; set; }
        public string? TargetSchema { get; set; }
        public bool CopyStructureOnly { get; set; } = false;
        public bool CopyData { get; set; } = true;
        public bool TruncateTargetTables { get; set; } = false;
        public List<string>? TableNames { get; set; }
        public string? OperationId { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of a copy database operation
    /// </summary>
    public class CopyDatabaseResult
    {
        public bool Success { get; set; }
        public string? OperationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }
}
