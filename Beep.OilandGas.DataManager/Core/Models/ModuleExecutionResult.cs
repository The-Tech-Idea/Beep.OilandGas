using Beep.OilandGas.DataManager.Core.State;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Result of executing all scripts for a module
    /// </summary>
    public class ModuleExecutionResult
    {
        public string ModuleName { get; set; } = string.Empty;
        public string? ExecutionId { get; set; } // Unique ID for resuming
        public bool Success { get; set; }
        public bool IsCompleted { get; set; } // False if stopped/cancelled
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public int SkippedScripts { get; set; }
        public List<ScriptExecutionResult> ScriptResults { get; set; } = new List<ScriptExecutionResult>();
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public ExecutionState? Checkpoint { get; set; } // State for resuming
    }
}
