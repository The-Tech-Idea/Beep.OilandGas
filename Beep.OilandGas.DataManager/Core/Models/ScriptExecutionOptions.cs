using Beep.OilandGas.DataManager.Core.State;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Options for script execution
    /// </summary>
    public class ScriptExecutionOptions
    {
        public bool ContinueOnError { get; set; } = false;
        public bool EnableRollback { get; set; } = false;
        public bool ExecuteOptionalScripts { get; set; } = false;
        public bool ValidateDependencies { get; set; } = true;
        public bool EnableParallelExecution { get; set; } = false;
        public int? MaxParallelTasks { get; set; }
        public ILogger? Logger { get; set; }
        public string? LogFilePath { get; set; }
        public List<ScriptType>? IncludedScriptTypes { get; set; }
        public List<ScriptType>? ExcludedScriptTypes { get; set; }
        
        // Checkpoint/Resume options
        public bool EnableCheckpointing { get; set; } = true;
        public string? ExecutionId { get; set; } // For resuming existing execution
        public TimeSpan CheckpointInterval { get; set; } = TimeSpan.FromMinutes(5);
        public IExecutionStateStore? StateStore { get; set; } // For persisting execution state
        
        // Error checking options
        public bool ValidateBeforeExecution { get; set; } = true;
        public bool CheckErrorsAfterExecution { get; set; } = true;
        public bool VerifyObjectsCreated { get; set; } = true; // Verify tables/indexes exist after creation
    }
}
