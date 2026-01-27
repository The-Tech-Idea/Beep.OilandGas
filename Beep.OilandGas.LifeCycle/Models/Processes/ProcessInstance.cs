using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Represents an active process instance
    /// </summary>
    public class ProcessInstance
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ProcessId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty; // Well, Field, Reservoir, etc.
        public string EntityType { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public string CurrentStepId { get; set; } = string.Empty;
        public ProcessStatus Status { get; set; } = ProcessStatus.NOT_STARTED;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }
        public string StartedBy { get; set; } = string.Empty;
        public PROCESS_DATA ProcessData { get; set; } = new PROCESS_DATA();
        public List<ProcessStepInstance> StepInstances { get; set; } = new List<ProcessStepInstance>();
        public List<ProcessHistoryEntry> History { get; set; } = new List<ProcessHistoryEntry>();
    }

    /// <summary>
    /// Process status enumeration
    /// </summary>
    public enum ProcessStatus
    {
        NOT_STARTED,
        IN_PROGRESS,
        COMPLETED,
        FAILED,
        CANCELLED,
        SUSPENDED
    }
}

