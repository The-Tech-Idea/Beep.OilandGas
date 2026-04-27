using System;
using System.Linq;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ProspectIdentification;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.LifeCycle.Services.Exploration.Processes;

public partial class ExplorationProcessService
{
    private static bool IsProspectToDiscoveryProspectWorkflow(ProcessInstance? instance) =>
        instance is not null
        && string.Equals(instance.ProcessId, ExplorationReferenceCodes.ProcessIdProspectToDiscovery, StringComparison.OrdinalIgnoreCase)
        && string.Equals(instance.EntityType, ExplorationReferenceCodes.EntityTypeProspect, StringComparison.OrdinalIgnoreCase);

    private static bool IsDiscoveryToDevelopmentDiscoveryWorkflow(ProcessInstance? instance) =>
        instance is not null
        && string.Equals(instance.ProcessId, ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment, StringComparison.OrdinalIgnoreCase)
        && string.Equals(instance.EntityType, ExplorationReferenceCodes.EntityTypeDiscovery, StringComparison.OrdinalIgnoreCase);

    private static bool IsPriorStepAdvanced(ProcessInstance instance, string prerequisiteStepId)
    {
        if (string.IsNullOrWhiteSpace(prerequisiteStepId))
            return true;
        var steps = instance.StepInstances;
        if (steps == null || steps.Count == 0)
            return false;
        return steps.Any(s =>
            string.Equals(s.StepId, prerequisiteStepId, StringComparison.OrdinalIgnoreCase)
            && s.Status != StepStatus.PENDING);
    }

    private void RequireExplorationWorkflowStepPrerequisite(
        ProcessInstance? instance,
        Func<ProcessInstance?, bool> isWorkflowInstance,
        string prerequisiteStepId,
        string attemptedStepId,
        string instanceId)
    {
        if (!isWorkflowInstance(instance))
            return;
        if (instance == null)
            return;
        if (IsPriorStepAdvanced(instance, prerequisiteStepId))
            return;

        _logger?.LogDebug(
            "Exploration workflow: instance {InstanceId} blocked for {Step} — prerequisite {Prior} not advanced",
            instanceId,
            attemptedStepId,
            prerequisiteStepId);
        throw new ExplorationWorkflowPrerequisiteException(instanceId, attemptedStepId, prerequisiteStepId);
    }
}
