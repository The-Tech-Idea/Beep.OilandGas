using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Runtime data payload for a process instance.
    /// </summary>
    public class PROCESS_DATA
    {
        public string InstanceId { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public string? DataJson { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
