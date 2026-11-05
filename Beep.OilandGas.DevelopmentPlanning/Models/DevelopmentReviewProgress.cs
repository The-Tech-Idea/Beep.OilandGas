using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

public sealed record DevelopmentReviewStageProgress(
    [property:JsonRequired]string StepId,[property:JsonRequired]string StepInstanceId,[property:JsonRequired]int Sequence,
    [property:JsonRequired]string Name,[property:JsonRequired]string AssignedRole,[property:JsonRequired]string Status,
    [property:JsonRequired]string? Decision,[property:JsonRequired]string? ActorUserId,[property:JsonRequired]string? Rationale,
    [property:JsonRequired]string? ApprovalId,[property:JsonRequired]string? HistoryId,[property:JsonRequired]DateTimeOffset? DecidedAtUtc);

public sealed record DevelopmentReviewHistory(
    [property:JsonRequired]string HistoryId,[property:JsonRequired]string? StepInstanceId,[property:JsonRequired]string Action,
    [property:JsonRequired]string ActorUserId,[property:JsonRequired]string Notes,[property:JsonRequired]DateTimeOffset AtUtc);

/// <summary>Read-only projection of the canonical review graph. Contains no retained commands or recipient roster.</summary>
public sealed record DevelopmentReviewProgress(
    [property:JsonRequired]DevelopmentAssignedReviewScope Scope,[property:JsonRequired]FIELD_DEVELOPMENT_PLAN Plan,
    [property:JsonRequired]string SourceRevision,[property:JsonRequired]string InstanceId,[property:JsonRequired]string WorkflowName,
    [property:JsonRequired]string WorkflowVersion,[property:JsonRequired]string RequesterUserId,[property:JsonRequired]string WorkflowState,
    [property:JsonRequired]IReadOnlyList<DevelopmentReviewStageProgress> Stages,[property:JsonRequired]IReadOnlyList<DevelopmentReviewHistory> History,
    [property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this with{Revision="",CheckedAtUtc=default}));
    public void Require(CompanyScope company,string actor,string? processId=null)
    {
        if(Scope is null || Scope.Company!=company || Scope.ActorUserId!=actor || Plan is null ||
            new[]{Scope.FieldId,InstanceId,WorkflowVersion,RequesterUserId}.Any(id=>!LeaseSummaryValidator.Identifier(id)) ||
            processId is not null && processId!=InstanceId || string.IsNullOrWhiteSpace(WorkflowName) || WorkflowName.Length>256 || WorkflowName.Any(char.IsControl) ||
            new[]{Scope.CoreDatabase,Scope.PlanningDatabase,Scope.LifecycleDatabase}.Any(db=>db is null || !LeaseSummaryValidator.Identifier(db.ConnectionName) || !LeaseSummaryValidator.Identifier(db.Fingerprint)) ||
            !ProcessDefinitionRevision.IsValid(SourceRevision) || !ProcessDefinitionRevision.IsValid(Revision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero ||
            Stages is null || Stages.Count is <1 or >64 || Stages.Any(row=>row is null) || History is null || History.Any(row=>row is null) || History.Count is <1 or >65)
            throw new InvalidOperationException("The recorded development review scope or evidence is unavailable.");
        DevelopmentPlanDraftReview.RequirePopulation([Plan],company,Scope.FieldId,Plan.FDP_ID);
        if(SourceRevision!=DevelopmentPlanSourceReview.RevisionFor(company,Scope.CoreDatabase,Scope.FieldId,Scope.PlanningDatabase,Plan.FDP_ID,Plan) || Revision!=ContentRevision() ||
            Stages.Select(row=>row.StepId).Distinct(StringComparer.Ordinal).Count()!=Stages.Count || Stages.Select(row=>row.StepInstanceId).Distinct(StringComparer.Ordinal).Count()!=Stages.Count ||
            Stages.Select(row=>row.Sequence).Distinct().Count()!=Stages.Count || !Stages.Select(row=>row.Sequence).SequenceEqual(Stages.Select(row=>row.Sequence).Order()))
            throw new InvalidOperationException("The recorded development source or ordered stages changed.");
        var undecided=false;var rejected=false;var pending=0;var decisions=0;
        foreach(var stage in Stages)
        {
            if(new[]{stage.StepId,stage.StepInstanceId,stage.AssignedRole}.Any(id=>!LeaseSummaryValidator.Identifier(id)) || string.IsNullOrWhiteSpace(stage.Name) || stage.Name.Length>256 || stage.Name.Any(char.IsControl))
                throw new InvalidOperationException("A recorded review stage is invalid.");
            if(stage.Decision is null)
            {
                undecided=true;if(stage.Status=="PENDING")pending++;else if(stage.Status!="BLOCKED")throw new InvalidOperationException("An undecided review stage has an invalid state.");
                if(stage.ActorUserId is not null || stage.Rationale is not null || stage.ApprovalId is not null || stage.HistoryId is not null || stage.DecidedAtUtc is not null)
                    throw new InvalidOperationException("An undecided review stage cannot imply an audit decision.");
            }
            else
            {
                if(undecided || rejected || stage.Decision is not ("APPROVE" or "REJECT") || stage.Status!=(stage.Decision=="APPROVE"?"COMPLETED":"FAILED") ||
                    new[]{stage.ActorUserId,stage.ApprovalId,stage.HistoryId}.Any(id=>!LeaseSummaryValidator.Identifier(id)) || stage.ActorUserId==RequesterUserId || stage.ActorUserId==Plan.ROW_CREATED_BY ||
                    string.IsNullOrWhiteSpace(stage.Rationale) || stage.Rationale.Length>1600 || stage.DecidedAtUtc is null || stage.DecidedAtUtc==default(DateTimeOffset) || stage.DecidedAtUtc.Value.Offset!=TimeSpan.Zero)
                    throw new InvalidOperationException("A recorded review decision is incomplete or out of order.");
                rejected=stage.Decision=="REJECT";decisions++;
                if(History.Count(row=>row.HistoryId==stage.HistoryId && row.StepInstanceId==stage.StepInstanceId && row.ActorUserId==stage.ActorUserId && row.Notes==stage.Rationale &&
                    row.AtUtc==stage.DecidedAtUtc && row.Action==(rejected?"DEVELOPMENT_REVIEW_REJECTED":"DEVELOPMENT_REVIEW_APPROVED"))!=1)
                    throw new InvalidOperationException("A recorded decision differs from its canonical history.");
            }
        }
        if(History.Count!=decisions+1 || History.Select(row=>row.HistoryId).Distinct(StringComparer.Ordinal).Count()!=History.Count ||
            History.Any(row=>!LeaseSummaryValidator.Identifier(row.HistoryId) || !LeaseSummaryValidator.Identifier(row.ActorUserId) || row.AtUtc==default || row.AtUtc.Offset!=TimeSpan.Zero) ||
            History.Count(row=>row.Action=="PROCESS_STARTED" && row.ActorUserId==RequesterUserId)!=1 ||
            (rejected?WorkflowState!="REVIEW_REJECTED" || pending!=0:decisions==Stages.Count?WorkflowState!="REVIEW_APPROVED" || pending!=0:
                pending!=1 || Stages[decisions].Status!="PENDING" || WorkflowState!=(decisions==0?"INITIAL":Stages[decisions].StepId)))
            throw new InvalidOperationException("The recorded review outcome or history is inconsistent.");
    }
}

public sealed record DevelopmentPlanReviewStatus(
    [property:JsonRequired]DevelopmentPlanSourceReview Source,[property:JsonRequired]DevelopmentReviewProgress? Review,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentPlanSourceReview original)
    {
        Source.Require(original.Context,original.PlanId);
        if(Source.Plan is null || Source.Context!=original.Context || Source.Revision!=original.Revision || Source.PlanningDatabase!=original.PlanningDatabase || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded review lookup differs from the displayed draft.");
        if(Review is not null)
        {
            Review.Require(Source.Context.Company,Source.Context.ActorUserId);
            if(Review.Plan.FDP_ID!=Source.PlanId || Review.SourceRevision!=Source.Revision || Review.Scope.CoreDatabase!=Source.Context.Database || Review.Scope.PlanningDatabase!=Source.PlanningDatabase || Review.Scope.FieldId!=Source.Context.FieldId)
                throw new InvalidOperationException("The recorded review belongs to another draft or binding.");
        }
    }
}
