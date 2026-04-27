using System;

namespace Beep.OilandGas.LifeCycle.Services.Exploration.Processes;

/// <summary>
/// Thrown when an exploration workflow step is invoked before its prerequisite step has left
/// <c>PENDING</c> on the process instance step list.
/// </summary>
public sealed class ExplorationWorkflowPrerequisiteException : InvalidOperationException
{
    public string InstanceId { get; }
    public string AttemptedStepId { get; }
    public string PrerequisiteStepId { get; }

    public ExplorationWorkflowPrerequisiteException(string instanceId, string attemptedStepId, string prerequisiteStepId)
        : base(
            $"Process instance '{instanceId}' cannot run step '{attemptedStepId}' until prerequisite step '{prerequisiteStepId}' has advanced past PENDING.")
    {
        InstanceId = instanceId;
        AttemptedStepId = attemptedStepId;
        PrerequisiteStepId = prerequisiteStepId;
    }
}
