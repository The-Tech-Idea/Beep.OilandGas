using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LifeCycle.Models.Processes
{
    /// <summary>
    /// Represents a state transition in a process
    /// </summary>
    public class ProcessTransition
    {
        public string TransitionId { get; set; } = string.Empty;
        public string FromStateId { get; set; } = string.Empty;
        public string ToStateId { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty; // Event that triggers transition
        public TransitionCondition Condition { get; set; } = new TransitionCondition();
        public List<TransitionAction> Actions { get; set; } = new List<TransitionAction>();
        public bool RequiresApproval { get; set; } = false;
        public List<string> RequiredRoles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Condition for a state transition
    /// </summary>
    public class TransitionCondition
    {
        public string ConditionType { get; set; } = string.Empty; // ALWAYS, CONDITIONAL, APPROVAL_REQUIRED
        public Dictionary<string, object> ConditionParameters { get; set; } = new Dictionary<string, object>();
        public string ConditionExpression { get; set; } = string.Empty; // For complex conditions
    }

    /// <summary>
    /// Action to execute during a state transition
    /// </summary>
    public class TransitionAction
    {
        public string ActionId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // UPDATE_ENTITY, SEND_NOTIFICATION, EXECUTE_SERVICE
        public Dictionary<string, object> ActionParameters { get; set; } = new Dictionary<string, object>();
        public int ExecutionOrder { get; set; }
    }
}

