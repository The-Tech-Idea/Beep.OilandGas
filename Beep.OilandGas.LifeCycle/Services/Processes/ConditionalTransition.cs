using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.LifeCycle.Services.Processes;

/// <summary>
/// A process transition that evaluates a condition expression against context data
/// to determine which target state to transition to.
/// Part of Phase 2 workflow engine enhancement.
/// </summary>
public class ConditionalTransition : ProcessTransition
{
    /// <summary>Expression to evaluate, e.g. "ESTIMATED_COST > 500000".</summary>
    public string ConditionExpression { get; set; } = string.Empty;

    /// <summary>Target state when condition evaluates to TRUE.</summary>
    public string TrueTargetStateId { get; set; } = string.Empty;

    /// <summary>Target state when condition evaluates to FALSE.</summary>
    public string FalseTargetStateId { get; set; } = string.Empty;

    /// <summary>Actions to execute on the TRUE branch.</summary>
    public List<TransitionAction>? TrueActions { get; set; }

    /// <summary>Actions to execute on the FALSE branch.</summary>
    public List<TransitionAction>? FalseActions { get; set; }
}
