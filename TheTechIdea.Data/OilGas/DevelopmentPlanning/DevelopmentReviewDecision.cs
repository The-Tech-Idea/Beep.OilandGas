using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>A retained decision about one exact assigned review stage; never an implicit investment or operations release.</summary>
public sealed record DevelopmentReviewDecision(
    [property:JsonRequired]DevelopmentAssignedReview Review,
    [property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Decision,
    [property:JsonRequired]string Notes)
{
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || Decision is not ("APPROVE" or "REJECT") || string.IsNullOrWhiteSpace(Notes) || Notes.Length>1600 ||
            Notes!=Notes.Trim() || Notes.Any(character=>char.IsControl(character) && character is not ('\r' or '\n' or '\t')))
            throw new InvalidOperationException("Review the exact assigned stage and record an explicit decision with a bounded rationale.");
        Review.Require(Review.Scope.Company,Review.Scope.ActorUserId,Review.InstanceId);
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
}

public sealed record DevelopmentReviewDecisionReceipt(
    [property:JsonRequired]string IntentRevision,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]bool Recorded,
    [property:JsonRequired]string? ApprovalId,
    [property:JsonRequired]string? HistoryId,
    [property:JsonRequired]string? NextStepInstanceId,
    [property:JsonRequired]string WorkflowState,
    [property:JsonRequired]string RecordedRevision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentReviewDecision original)
    {
        original.Require();
        if(IntentRevision!=original.Revision() || InstanceId!=original.Review.InstanceId ||
            (Recorded? !LeaseSummaryValidator.Identifier(ApprovalId) || !LeaseSummaryValidator.Identifier(HistoryId) : ApprovalId is not null || HistoryId is not null || NextStepInstanceId is not null) ||
            NextStepInstanceId is not null && !LeaseSummaryValidator.Identifier(NextStepInstanceId) || !LeaseSummaryValidator.Identifier(WorkflowState) ||
            !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded decision outcome does not match the retained original review.");
    }
}
