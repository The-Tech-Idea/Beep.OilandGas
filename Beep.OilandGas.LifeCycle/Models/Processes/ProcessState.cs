using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Represents a state in a process state machine
    /// </summary>
    public class ProcessState
    {
        public string StateId { get; set; } = string.Empty;
        public string StateName { get; set; } = string.Empty;
        public string StateType { get; set; } = string.Empty; // INITIAL, INTERMEDIATE, FINAL, ERROR
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> StateProperties { get; set; } = new Dictionary<string, object>();
        public List<string> AllowedTransitions { get; set; } = new List<string>();
        public bool IsTerminal { get; set; } = false;
    }
}

