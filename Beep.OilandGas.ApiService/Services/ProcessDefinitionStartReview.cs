using System.Text.Json;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Checks a current definition against the original start review; access is enforced separately.</summary>
public static class ProcessDefinitionStartReview
{
    public static void Require(ProcessInstanceRequest request, string fieldId, ProcessDefinition? definition)
    {
        if (string.IsNullOrWhiteSpace(fieldId) || request.ExpectedFieldId != fieldId ||
            string.IsNullOrWhiteSpace(request.ProcessId) || string.IsNullOrWhiteSpace(request.EntityId) ||
            string.IsNullOrWhiteSpace(request.EntityType) || !ProcessDefinitionRevision.IsValid(request.ExpectedDefinitionRevision) ||
            definition is null || definition.ProcessId != request.ProcessId || !definition.IsActive ||
            string.IsNullOrWhiteSpace(definition.ProcessName) || string.IsNullOrWhiteSpace(definition.ProcessType) ||
            definition.EntityType != request.EntityType || definition.Steps is not { Count: > 0 } ||
            definition.Steps.Any(step => step is null || string.IsNullOrWhiteSpace(step.StepId) ||
                string.IsNullOrWhiteSpace(step.StepName) || step.SequenceNumber <= 0 || step.RequiredRoles is null ||
                step.RequiredRoles.Any(string.IsNullOrWhiteSpace)) ||
            definition.Steps.Select(step => step.StepId).Distinct(StringComparer.Ordinal).Count() != definition.Steps.Count ||
            definition.Steps.Select(step => step.SequenceNumber).Distinct().Count() != definition.Steps.Count ||
            ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)) != request.ExpectedDefinitionRevision)
            throw new InvalidOperationException("The original FIELD, configured definition and source entity could not be confirmed.");
    }
}
