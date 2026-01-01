using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Defines a process template/workflow definition
    /// </summary>
    public class ProcessDefinition
    {
        public string ProcessId { get; set; } = string.Empty;
        public string ProcessName { get; set; } = string.Empty;
        public string ProcessType { get; set; } = string.Empty; // EXPLORATION, DEVELOPMENT, PRODUCTION, DECOMMISSIONING
        public string EntityType { get; set; } = string.Empty; // WELL, FIELD, RESERVOIR, PROSPECT, POOL, FACILITY
        public string Description { get; set; } = string.Empty;
        public List<ProcessStepDefinition> Steps { get; set; } = new List<ProcessStepDefinition>();
        public Dictionary<string, ProcessTransition> Transitions { get; set; } = new Dictionary<string, ProcessTransition>();
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
    }
}

