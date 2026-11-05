using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>Explicit plan-baseline approval, separate from review-stage decisions and financial release.</summary>
public sealed record DevelopmentPlanRelease(
    [property:JsonRequired]DevelopmentReviewProgress Review,[property:JsonRequired]Guid IntentId,[property:JsonRequired]string Rationale)
{
    public const int MaximumBodyLength=1024*1024;
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>1600 ||
            Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')))throw new InvalidOperationException("Retain the approved review and an explicit bounded FDP baseline rationale.");
        Review.Require(Review.Scope.Company,Review.Scope.ActorUserId,Review.InstanceId);
        if(Review.WorkflowState!="REVIEW_APPROVED" || Review.Stages.Any(stage=>stage.Decision!="APPROVE") || Review.Scope.ActorUserId==Review.RequesterUserId || Review.Scope.ActorUserId==Review.Plan.ROW_CREATED_BY)
            throw new InvalidOperationException("An independent current final-stage recipient and all approved configured review stages are required for FDP baseline approval.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("The original FDP baseline input exceeds its bounded proof.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public static string BaselineId(CompanyScope company,string sourceId)=>"fdp-baseline-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,SourceDraftId=sourceId}));
    public static string ReviewEvidenceRevision(DevelopmentReviewProgress review)
    {
        review.Require(review.Scope.Company,review.Scope.ActorUserId,review.InstanceId);
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{review.Scope.Company,review.Scope.FieldId,review.Scope.CoreDatabase,review.Scope.PlanningDatabase,review.Scope.LifecycleDatabase,
            review.SourceRevision,review.InstanceId,review.WorkflowName,review.WorkflowVersion,review.RequesterUserId,review.WorkflowState,review.Stages,review.History}));
    }
    public FIELD_DEVELOPMENT_PLAN ToRecord(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Actual UTC baseline approval time is required.");
        var row=JsonSerializer.Deserialize<FIELD_DEVELOPMENT_PLAN>(JsonSerializer.Serialize(Review.Plan))!;
        row.FDP_ID=BaselineId(Review.Scope.Company,Review.Plan.FDP_ID);row.FDP_VERSION=checked(Review.Plan.FDP_VERSION+1);row.FDP_STATUS="APPROVED";
        row.SOURCE_DRAFT_FDP_ID=Review.Plan.FDP_ID;row.SOURCE_REVIEW_PROCESS_ID=Review.InstanceId;row.RELEASE_INTENT_ID=IntentId.ToString("D");row.RELEASE_REQUEST_REVISION=Revision();row.RELEASE_COMMAND_JSON=JsonSerializer.Serialize(this);
        row.SUBMISSION_DATE=Review.History.Single(entry=>entry.Action=="PROCESS_STARTED").AtUtc.UtcDateTime;row.APPROVAL_DATE=utc;row.APPROVED_BY=Review.Scope.ActorUserId;
        row.ROW_CREATED_BY=Review.Scope.ActorUserId;row.ROW_CREATED_DATE=utc;row.ROW_CHANGED_BY="";row.ROW_CHANGED_DATE=null;row.PPDM_GUID=IntentId.ToString("D");return row;
    }
    public static DevelopmentPlanRelease FromRecord(FIELD_DEVELOPMENT_PLAN row)
    {
        if(row?.RELEASE_COMMAND_JSON is not {Length:>0 and <=MaximumBodyLength} json)throw new InvalidOperationException("The bounded original FDP baseline command is unavailable.");
        var original=JsonSerializer.Deserialize<DevelopmentPlanRelease>(json)??throw new InvalidOperationException("The original FDP baseline input is invalid.");original.Require();
        if(row.ROW_CREATED_DATE is null || row.ROW_CREATED_DATE.Value.Kind==DateTimeKind.Local || row.APPROVAL_DATE?.Kind==DateTimeKind.Local || row.SUBMISSION_DATE?.Kind==DateTimeKind.Local)throw new InvalidOperationException("The original baseline UTC audit is unavailable.");
        var expected=original.ToRecord(DateTime.SpecifyKind(row.ROW_CREATED_DATE.Value,DateTimeKind.Utc));
        // Provider-neutral native timestamps can deserialize without Kind; normalize only the known UTC audit fields.
        var actual=JsonSerializer.Deserialize<FIELD_DEVELOPMENT_PLAN>(JsonSerializer.Serialize(row))!;
        actual.ROW_CREATED_DATE=DateTime.SpecifyKind(actual.ROW_CREATED_DATE!.Value,DateTimeKind.Utc);
        if(actual.APPROVAL_DATE is {} approval)actual.APPROVAL_DATE=DateTime.SpecifyKind(approval,DateTimeKind.Utc);
        if(actual.SUBMISSION_DATE is {} submitted)actual.SUBMISSION_DATE=DateTime.SpecifyKind(submitted,DateTimeKind.Utc);
        if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The canonical FDP baseline differs from its exact original source, approval or audit.");
        return original;
    }
}

public sealed record DevelopmentPlanBaselineReview(
    [property:JsonRequired]DevelopmentReviewProgress Review,[property:JsonRequired]FIELD_DEVELOPMENT_PLAN? Baseline,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentReviewProgress displayed)
    {
        Review.Require(displayed.Scope.Company,displayed.Scope.ActorUserId,displayed.InstanceId);
        if(Review.Revision!=displayed.Revision || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The baseline lookup differs from the displayed original review.");
        if(Baseline is not null && DevelopmentPlanRelease.ReviewEvidenceRevision(DevelopmentPlanRelease.FromRecord(Baseline).Review)!=DevelopmentPlanRelease.ReviewEvidenceRevision(Review))
            throw new InvalidOperationException("The FDP baseline belongs to another recorded source or approved review.");
    }
}
public sealed record DevelopmentPlanReleaseReceipt(
    [property:JsonRequired]string IntentRevision,[property:JsonRequired]DevelopmentPlanBaselineReview Result)
{
    public void Require(DevelopmentPlanRelease original)
    {
        original.Require();if(IntentRevision!=original.Revision() || Result is null)throw new InvalidOperationException("The FDP baseline outcome does not confirm its original intent.");
        Result.Require(original.Review);
        if(Result.Baseline is not null && DevelopmentPlanRelease.FromRecord(Result.Baseline).Revision()!=original.Revision())throw new InvalidOperationException("Another original command owns this FDP baseline.");
    }
}
