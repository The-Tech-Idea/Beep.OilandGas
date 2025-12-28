using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Represents a history entry for a process instance
    /// </summary>
    public class ProcessHistoryEntry
    {
        public string HistoryId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public string StepInstanceId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // STATE_CHANGE, STEP_STARTED, STEP_COMPLETED, APPROVAL_REQUESTED, etc.
        public string PreviousState { get; set; } = string.Empty;
        public string NewState { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string PerformedBy { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Dictionary<string, object> ActionData { get; set; } = new Dictionary<string, object>();
    }
}

