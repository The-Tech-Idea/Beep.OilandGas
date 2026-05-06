using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Runtime data payload for a process step execution.
    /// </summary>
    public class PROCESS_STEP_DATA
    {
        public string StepInstanceId { get; set; } = string.Empty;
        public string StepType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? DataJson { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime? LastUpdated { get; set; }
    }
}
