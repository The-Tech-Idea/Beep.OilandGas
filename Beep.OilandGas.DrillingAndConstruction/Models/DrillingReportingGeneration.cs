using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

/// <summary>The current qualified responsibility, retaining its immutable initial assignment.</summary>
public sealed record DrillingReportingGeneration(
    [property:JsonRequired]DrillingExecutionAssignment Root,
    [property:JsonRequired]int Number,
    [property:JsonRequired]DrillingExecutionAssignmentOption Workflow,
    [property:JsonRequired]string StepInstanceId,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]string ManagerUserId,
    [property:JsonRequired]string ManagerRole,
    [property:JsonRequired]string ManagerPersona,
    [property:JsonRequired]string ManagerAuthorityRevision)
{
    public static DrillingReportingGeneration Initial(DrillingExecutionAssignment root)
    {
        root.Require();
        return new(root,1,root.Workflow,DrillingExecutionAssignmentReceipt.StepId(root.InstanceId(),root.Workflow.Step.StepId),
            root.Revision(),root.Scope().ActorUserId,root.SenderRole,root.SenderPersonaCode,root.SenderAuthorityRevision);
    }
    public IReadOnlyList<string> TaskIds()
    {
        var id=Root.InstanceId();
        return Workflow.Step.PersonaCodes.Select(persona=>DrillingExecutionAssignmentReceipt.TaskId(id,StepInstanceId,persona)).Order(StringComparer.Ordinal).ToArray();
    }
    public static DrillingReportingGeneration After(DrillingReportingReassignment command)
    {
        command.Require();
        return new(command.Review.Responsibility.Root,command.Review.Responsibility.Number+1,command.Review.Destination,command.StepInstanceId(),command.Revision(),
            command.Scope().ActorUserId,command.Review.ManagerRole,command.Review.ManagerPersona,command.Review.AuthorityRevision);
    }
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));
    public void Require()
    {
        if(Root is null || Workflow is null)throw new InvalidOperationException("The original assignment and current reporting workflow are required.");
        Root.Require();Workflow.Require();
        if(Number is <1 or >256 || !Workflow.Ready || !LeaseSummaryValidator.Identifier(StepInstanceId) || !ProcessDefinitionRevision.IsValid(Revision) ||
            !LeaseSummaryValidator.Identifier(ManagerUserId) || !LeaseSummaryValidator.Identifier(ManagerRole) || !LeaseSummaryValidator.Identifier(ManagerPersona) ||
            ManagerPersona.Length>64 || !ProcessDefinitionRevision.IsValid(ManagerAuthorityRevision) ||
            (Number==1 ? JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(Initial(Root)) : !StepInstanceId.StartsWith("reporting-transfer-step-",StringComparison.Ordinal)))
            throw new InvalidOperationException("The current reporting generation, accountable manager and complete receiving workflow are unconfirmed.");
    }
}
