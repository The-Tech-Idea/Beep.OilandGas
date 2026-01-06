using System;
using System.Collections.Generic;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Models.DTOs.DataManagement
{
    /// <summary>
    /// Request to create a PPDM database
    /// </summary>
    public class CreateDatabaseRequest
    {
        /// <summary>
        /// Connection configuration
        /// </summary>
        public ConnectionProperties Connection { get; set; } = null!;

        /// <summary>
        /// Database creation options
        /// </summary>
        public DatabaseCreationOptionsDto Options { get; set; } = null!;
    }

    /// <summary>
    /// Database creation options DTO for API
    /// </summary>
    public class DatabaseCreationOptionsDto
    {
        public string DatabaseType { get; set; } = string.Empty; // SqlServer, Oracle, etc.
        public string ScriptsPath { get; set; } = string.Empty;
        public List<string>? Categories { get; set; }
        public List<string>? ScriptTypes { get; set; } // TAB, PK, FK, etc.
        public bool EnableLogging { get; set; } = true;
        public string? LogFilePath { get; set; }
        public bool ContinueOnError { get; set; } = false;
        public bool EnableRollback { get; set; } = false;
        public bool ExecuteConsolidatedScripts { get; set; } = true;
        public bool ExecuteIndividualScripts { get; set; } = true;
        public bool ExecuteOptionalScripts { get; set; } = false;
        public bool ValidateDependencies { get; set; } = true;
        public bool EnableParallelExecution { get; set; } = false;
        public int? MaxParallelTasks { get; set; }
        public string? ExecutionId { get; set; }
    }

    /// <summary>
    /// Database creation result DTO for API
    /// </summary>
    public class DatabaseCreationResultDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public int SkippedScripts { get; set; }
        public List<ScriptExecutionResultDto> ScriptResults { get; set; } = new List<ScriptExecutionResultDto>();
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
        public string? LogFilePath { get; set; }
    }

    /// <summary>
    /// Script execution result DTO for API
    /// </summary>
    public class ScriptExecutionResultDto
    {
        public string ScriptFileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int? RowsAffected { get; set; }
        public string? ExecutionLog { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Script execution progress DTO for API
    /// </summary>
    public class ScriptExecutionProgressDto
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int TotalScripts { get; set; }
        public int CompletedScripts { get; set; }
        public int FailedScripts { get; set; }
        public int SkippedScripts { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string CurrentScript { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public string Status { get; set; } = "Not Started";
    }

    /// <summary>
    /// Script info DTO for API
    /// </summary>
    public class ScriptInfoDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public string ScriptType { get; set; } = string.Empty;
        public string? TableName { get; set; }
        public string? Module { get; set; }
        public string? SubjectArea { get; set; }
        public bool IsConsolidated { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsOptional { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public int ExecutionOrder { get; set; }
        public List<string> Dependencies { get; set; } = new List<string>();
        public string? Category { get; set; }
        public object Name { get; set; }
    }
}




