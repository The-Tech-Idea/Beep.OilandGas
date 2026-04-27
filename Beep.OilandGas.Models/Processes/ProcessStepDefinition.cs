using Beep.OilandGas.Models.Data;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Processes
{
    /// <summary>
    /// Defines a step within a process
    /// </summary>
    public class ProcessStepDefinition
    {
        public string StepId { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public string StepType { get; set; } = string.Empty; // ACTION, APPROVAL, VALIDATION, NOTIFICATION
        public bool IsRequired { get; set; } = true;
        public bool RequiresApproval { get; set; } = false;
        public List<string> RequiredRoles { get; set; } = new List<string>();
        public Dictionary<string, ValidationRule> ValidationRules { get; set; } = new Dictionary<string, ValidationRule>();
        public Dictionary<string, object> StepConfiguration { get; set; } = new Dictionary<string, object>();
        public string NextStepId { get; set; } = string.Empty;
        public List<string> ConditionalNextSteps { get; set; } = new List<string>(); // Based on step outcome
    }
}
