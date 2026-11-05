using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;
/// <summary>Retained independent decision on the displayed reported completion claim and native stage.</summary>
public sealed record DrillingCompletionReviewDecision([property:JsonRequired]DrillingCompletionAssignedReview Review,[property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Decision,[property:JsonRequired]string Rationale)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public DrillingCompletionAcceptance? Acceptance{get;init;}
    public DevelopmentAssignedReviewScope Scope()=>Review.Source.History.Execution.Basis.Scope();
    public bool Terminal()=>Decision=="REJECT" || Review.StageNumber==Review.Original.Workflow.Stages.Count;
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || Decision is not ("APPROVE" or "REJECT") || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>1600 || Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')))
            throw new InvalidOperationException("Choose approve or reject for the exact assigned completion review stage and enter a bounded review rationale.");
        var scope=Scope();Review.Require(scope.Company,scope.ActorUserId,Review.InstanceId);
        if(Decision=="APPROVE" && !Review.IsCurrentClaim)throw new InvalidOperationException("A historical or withdrawn completion claim cannot receive a new approval.");
        if(Acceptance is not null){if(Decision!="APPROVE")throw new InvalidOperationException("Physical completion acceptance requires an explicit final approval.");Acceptance.Require(Review);}
        if(JsonSerializer.SerializeToUtf8Bytes(this).Length>DrillingCompletionReviewStart.MaximumBodyLength)throw new InvalidOperationException("The retained completion review decision exceeds its supported bound.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public string AuditId(string kind)=>"completion-stage-"+kind+"-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=Review.InstanceId,IntentId}));
}
public sealed record DrillingCompletionReviewDecisionReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string InstanceId,[property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? ApprovalId,[property:JsonRequired]string? HistoryId,[property:JsonRequired]string? NextStepInstanceId,[property:JsonRequired]string WorkflowState,
    [property:JsonRequired]string RecordedRevision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonRequired] public bool CompletionAccepted{get;init;}
    public void Require(DrillingCompletionReviewDecision original)
    {
        original.Require();var next=original.Terminal()?null:DrillingCompletionReviewStartReceipt.StepId(InstanceId,original.Review.Original.Workflow.Stages[original.Review.StageNumber].Definition.StepId);
        if(IntentRevision!=original.Revision() || InstanceId!=original.Review.InstanceId || (Recorded?ApprovalId!=original.AuditId("approval") || HistoryId!=original.AuditId("decision") || NextStepInstanceId!=next:ApprovalId is not null || HistoryId is not null || NextStepInstanceId is not null) ||
            CompletionAccepted!=(Recorded && original.Acceptance is not null) || !LeaseSummaryValidator.Identifier(WorkflowState) || !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The original completion review decision, ordered destination and native outcome were not confirmed.");
    }
}
