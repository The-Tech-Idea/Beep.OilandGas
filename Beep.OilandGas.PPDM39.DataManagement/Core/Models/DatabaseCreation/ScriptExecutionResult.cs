using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation
{
    /// <summary>
    /// Result of executing a single script
    /// </summary>
    public class ScriptExecutionResult
    {
        public string ScriptFileName { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Exception? Exception { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration => EndTime - StartTime;
        public int? RowsAffected { get; set; }
        public string? ExecutionLog { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public string ScriptName { get; set; }
        public string Message { get; set; }
    }
}








