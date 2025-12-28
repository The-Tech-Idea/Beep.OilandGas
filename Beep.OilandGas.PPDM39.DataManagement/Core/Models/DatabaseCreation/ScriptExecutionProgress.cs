using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Progress tracking for script execution
    /// </summary>
    public class ScriptExecutionProgress
    {
        public string ExecutionId { get; set; } = string.Empty;
        public int TotalScripts { get; set; }
        public int CompletedScripts { get; set; }
        public int FailedScripts { get; set; }
        public int SkippedScripts { get; set; }
        public decimal ProgressPercentage => TotalScripts > 0 ? (decimal)CompletedScripts * 100 / TotalScripts : 0;
        public string CurrentScript { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public List<ScriptExecutionResult> Results { get; set; } = new List<ScriptExecutionResult>();
        public string Status { get; set; } = "Not Started"; // Not Started, In Progress, Completed, Failed, Cancelled
    }
}








