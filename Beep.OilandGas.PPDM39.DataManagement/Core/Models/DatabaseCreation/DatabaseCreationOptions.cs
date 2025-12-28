using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Options for database creation
    /// </summary>
    public class DatabaseCreationOptions
    {
        public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ScriptsPath { get; set; } = string.Empty; // Path to scripts directory (e.g., "Scripts/Sqlserver")
        public List<string>? Categories { get; set; } // Optional: specific modules/categories to execute
        public List<ScriptType>? ScriptTypes { get; set; } // Optional: specific script types to execute
        public bool EnableLogging { get; set; } = true;
        public string? LogFilePath { get; set; }
        public bool ContinueOnError { get; set; } = false;
        public bool EnableRollback { get; set; } = false;
        public bool ExecuteConsolidatedScripts { get; set; } = true; // Execute TAB.sql, PK.sql, etc.
        public bool ExecuteIndividualScripts { get; set; } = true; // Execute individual table scripts
        public bool ExecuteOptionalScripts { get; set; } = false; // Execute TCM, CCM, SYN, GUID
        public bool ValidateDependencies { get; set; } = true;
        public bool EnableParallelExecution { get; set; } = false; // Execute independent scripts in parallel
        public int? MaxParallelTasks { get; set; } // Maximum parallel tasks (null = unlimited)
        public string? ExecutionId { get; set; } // Optional: resume from previous execution
    }
}








