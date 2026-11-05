using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

/// <summary>Retained independent decision against the exact reviewed pending proposal and current source.</summary>
public sealed record CompanyWorkOrderExecutionPermitDecision(CompanyWorkOrderExecutionPermitApprovalReview Review,
    Guid IntentId,string Outcome,string Rationale)
{
    private sealed record StoredEvidence(CompanyWorkOrderExecutionPermitDecision Decision,string PendingRowJson);
    public CompanyWorkOrderExecutionPermitReference ProposalReference()=>new(Review.Proposal.Task,Review.Proposal.IntentId);
    public void Require()
    {
        if(Review?.Context is null || Review.Proposal is null || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Rationale) || Rationale.Length>2048 || Rationale.Any(char.IsControl))throw new ArgumentException("Retain the complete original independent review, decision identity and explicit rationale.");
        Review.Require(Review.Context.Company,Review.Context.ActorUserId,ProposalReference());
        if(Outcome=="Approve")Review.RequireCurrentPrerequisiteAcceptance(Review.ComparedAtUtc);
        else if(Outcome!="Reject")throw new InvalidOperationException("The independent permit decision outcome is unsupported.");
        if(JsonSerializer.Serialize(this).Length>524288)throw new InvalidOperationException("The retained independent decision exceeds the evidence capacity.");
    }
    public void RequirePending(WORK_ORDER_EXECUTION_PERMIT row)
    {
        Require();Review.Proposal.RequireRecorded(row);
        if(row.DECISION_INTENT_ID is not null || row.DECIDED_BY is not null || row.DECIDED_AT_UTC is not null || row.DECISION_DATA_JSON is not null || ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row))!=Review.RecordedRevision)
            throw new InvalidOperationException("The original reviewed pending proposal changed or already has a decision.");
    }
    public WORK_ORDER_EXECUTION_PERMIT Prepare(WORK_ORDER_EXECUTION_PERMIT pending,DateTime utc)
    {
        RequirePending(pending);if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc)throw new InvalidOperationException("A current recorded UTC decision time is required.");
        if(Outcome=="Approve")Review.RequireCurrentPrerequisiteAcceptance(utc);
        var evidence=JsonSerializer.Serialize(new StoredEvidence(this,JsonSerializer.Serialize(pending)));if(evidence.Length>524288)throw new InvalidOperationException("The complete decision and original pending-row evidence exceed retention capacity.");
        var row=CompanyInventorySourceReview.Copy(pending);row.STATUS=Outcome=="Approve"?"APPROVED":"REJECTED";row.DECISION_INTENT_ID=IntentId.ToString("D");row.DECIDED_BY=Review.Context.ActorUserId;row.DECIDED_AT_UTC=utc;row.DECISION_DATA_JSON=evidence;row.ROW_CHANGED_BY=row.DECIDED_BY;row.ROW_CHANGED_DATE=utc;return row;
    }
    public void RequireRecorded(WORK_ORDER_EXECUTION_PERMIT row)
    {
        Require();if(row is null || row.STATUS!=(Outcome=="Approve"?"APPROVED":"REJECTED") || row.DECISION_INTENT_ID!=IntentId.ToString("D") || row.DECIDED_BY!=Review.Context.ActorUserId || row.DECIDED_AT_UTC is not {} decided || decided.Kind!=DateTimeKind.Utc || decided<Review.ComparedAtUtc || row.ROW_CHANGED_BY!=row.DECIDED_BY || row.ROW_CHANGED_DATE is not {} changed || changed.Kind!=DateTimeKind.Utc || changed<decided)
            throw new InvalidOperationException("The original independent decision/body/actor/audit is unconfirmed.");
        var evidence=ReadEvidence(row);if(JsonSerializer.Serialize(evidence.Decision)!=JsonSerializer.Serialize(this))throw new InvalidOperationException("The recorded independent decision differs from the original body.");
        WORK_ORDER_EXECUTION_PERMIT pending;
        try{pending=JsonSerializer.Deserialize<WORK_ORDER_EXECUTION_PERMIT>(evidence.PendingRowJson)??throw new InvalidOperationException("The original pending row is unavailable.");}
        catch(JsonException error){throw new InvalidOperationException("The original pending-row evidence is malformed.",error);}RequirePending(pending);
        var expected=Prepare(pending,decided);expected.ROW_CHANGED_DATE=changed;
        if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original pending proposal or decision audit was changed.");
    }
    private static StoredEvidence ReadEvidence(WORK_ORDER_EXECUTION_PERMIT row)
    {
        if(string.IsNullOrWhiteSpace(row.DECISION_DATA_JSON) || row.DECISION_DATA_JSON.Length>524288)throw new InvalidOperationException("The complete original decision evidence is unavailable.");
        try{var evidence=JsonSerializer.Deserialize<StoredEvidence>(row.DECISION_DATA_JSON)??throw new InvalidOperationException("The original decision evidence is unavailable.");if(evidence.Decision is null || string.IsNullOrWhiteSpace(evidence.PendingRowJson))throw new InvalidOperationException("The complete original decision and pending-row evidence are unavailable.");return evidence;}
        catch(JsonException error){throw new InvalidOperationException("The original decision evidence is malformed.",error);}
    }
    public static CompanyWorkOrderExecutionPermitDecision FromRecorded(WORK_ORDER_EXECUTION_PERMIT row)
    {
        var decision=ReadEvidence(row).Decision??throw new InvalidOperationException("The original decision body is unavailable.");decision.RequireRecorded(row);return decision;
    }
    public WORK_ORDER_EXECUTION_PERMIT ReadOriginalPending(WORK_ORDER_EXECUTION_PERMIT row)
    {
        RequireRecorded(row);
        var evidence=ReadEvidence(row);
        try{return CompanyInventorySourceReview.Copy(JsonSerializer.Deserialize<WORK_ORDER_EXECUTION_PERMIT>(evidence.PendingRowJson)??throw new InvalidOperationException("The original pending permit evidence is unavailable."));}
        catch(JsonException error){throw new InvalidOperationException("The original pending permit evidence is malformed.",error);}
    }
}
public sealed record CompanyWorkOrderExecutionPermitDecisionReceipt(CompanyScope Company,string Actor,Guid IntentId,string PermitId,string Outcome,string RecordedRevision)
{
    public void Require(CompanyWorkOrderExecutionPermitDecision decision)
    {decision.Require();if(Company!=decision.Review.Context.Company || Actor!=decision.Review.Context.ActorUserId || IntentId!=decision.IntentId || PermitId!=decision.Review.Proposal.PermitId() || Outcome!=decision.Outcome || !ProcessDefinitionRevision.IsValid(RecordedRevision))throw new InvalidOperationException("The original independent permit decision receipt is unconfirmed.");}
}
public sealed record CompanyWorkOrderExecutionPermitDecisionReference(CompanyWorkOrderExecutionPermitReference Proposal,Guid IntentId)
{
    public void Require(){if(Proposal is null || IntentId==Guid.Empty)throw new ArgumentException("The original proposal/task and decision reference are required.");Proposal.Require();}
}
public sealed record CompanyWorkOrderExecutionPermitDecisionRecovery(CompanyWorkOrderExecutionPermitDecision Decision,CompanyWorkOrderExecutionPermitDecisionReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitDecisionReference query)
    {query.Require();if(Decision is null || Receipt is null)throw new InvalidOperationException("The complete original independent decision is unavailable.");Receipt.Require(Decision);if(Decision.Review.Context.Company!=company || Decision.Review.Context.ActorUserId!=actor || Decision.ProposalReference()!=query.Proposal || Decision.IntentId!=query.IntentId)throw new InvalidOperationException("The recovered decision differs from its original reviewer/company/task/reference.");}
}
