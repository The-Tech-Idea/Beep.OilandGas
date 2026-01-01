using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;

namespace Beep.OilandGas.DataManager.Core.Models
{
    /// <summary>
    /// Execution state for checkpoint and resume functionality
    /// </summary>
    public class ExecutionState
    {
        public string ExecutionId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string ConnectionName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? LastCheckpoint { get; set; }
        public List<string> CompletedScripts { get; set; } = new List<string>();
        public List<string> FailedScripts { get; set; } = new List<string>();
        public List<string> PendingScripts { get; set; } = new List<string>();
        public Dictionary<string, ScriptExecutionResult> ScriptResults { get; set; } = new Dictionary<string, ScriptExecutionResult>();
        public bool IsCompleted { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
