using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Beep.OilandGas.DrillingAndConstruction.Models;
/// <summary>Retained independent decision on the displayed saved program and native stage.</summary>
public sealed record DrillingProgramReviewDecision([property:JsonRequired]DrillingProgramAssignedReview Review,[property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Decision,[property:JsonRequired]string Rationale)
{
    public DevelopmentAssignedReviewScope Scope()=>Review.Source.Basis.Scope();
    public bool Terminal()=>Decision=="REJECT" || Review.StageNumber==Review.Original.Workflow.Stages.Count;
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || Decision is not ("APPROVE" or "REJECT") || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>1600 || Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')))
            throw new InvalidOperationException("Choose approve or reject for the exact assigned program stage and enter a bounded review rationale.");
        var scope=Scope();Review.Require(scope.Company,scope.ActorUserId,Review.InstanceId);
        if(JsonSerializer.SerializeToUtf8Bytes(this).Length>DrillingProgramReviewStart.MaximumBodyLength)throw new InvalidOperationException("The retained program decision exceeds its supported bound.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public string AuditId(string kind)=>"program-stage-"+kind+"-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=Review.InstanceId,IntentId}));
}
public sealed record DrillingProgramReviewDecisionReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string InstanceId,[property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? ApprovalId,[property:JsonRequired]string? HistoryId,[property:JsonRequired]string? NextStepInstanceId,[property:JsonRequired]string WorkflowState,
    [property:JsonRequired]string RecordedRevision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingProgramReviewDecision original)
    {
        original.Require();var next=original.Terminal()?null:DrillingProgramReviewStartReceipt.StepId(InstanceId,original.Review.Original.Workflow.Stages[original.Review.StageNumber].Definition.StepId);
        if(IntentRevision!=original.Revision() || InstanceId!=original.Review.InstanceId || (Recorded?ApprovalId!=original.AuditId("approval") || HistoryId!=original.AuditId("decision") || NextStepInstanceId!=next:ApprovalId is not null || HistoryId is not null || NextStepInstanceId is not null) ||
            !LeaseSummaryValidator.Identifier(WorkflowState) || !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The original program decision, ordered destination and native outcome were not confirmed.");
    }
}
