using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Overall result of database creation process
    /// </summary>
    public class DatabaseCreationResult
    {
        public string ExecutionId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration => EndTime - StartTime;
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public int SkippedScripts { get; set; }
        public List<ScriptExecutionResult> ScriptResults { get; set; } = new List<ScriptExecutionResult>();
        public Dictionary<string, object> Summary { get; set; } = new Dictionary<string, object>();
        public string? LogFilePath { get; set; }
    }
}









