namespace TheTechIdea.Data.OilGas;

/// <summary>Authorized public decision audit; private rationale, retained command and recovery intent are omitted.</summary>
public sealed record CompanyWorkOrderCompletionDecisionFacts(CompanyScope Company,AssetDatabaseScope CoreDatabase,CompanyWorkOrderCompletionQuery Query,
    string ReviewerUserId,string ReviewerPersonBaId,string ReviewerRole,string Decision,DateTime RecordedAtUtc,string ApprovalId,string HistoryId,string ReportRevision,string EvidenceRevision,string Revision)
{
    public string? WorkSubjectRevision{get;init;}
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);if(Query is null)throw new InvalidOperationException("Original report query is required.");Query.Require();
        if(CoreDatabase is null || !LeaseSummaryValidator.Identifier(CoreDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(CoreDatabase.Fingerprint) ||
            new[]{ReviewerUserId,ReviewerPersonBaId,ReviewerRole,ApprovalId,HistoryId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || Decision is not ("VERIFIED" or "REJECTED") ||
            RecordedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || new[]{ReportRevision,EvidenceRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) || WorkSubjectRevision is not null && !ProcessDefinitionRevision.IsValid(WorkSubjectRevision))throw new InvalidOperationException("Original recorded completion decision and public reviewer/source/UTC audit are unavailable.");
    }
    public static CompanyWorkOrderCompletionDecisionFacts From(CompanyWorkOrderCompletionDecisionRecovery recorded)
    {
        var command=recorded.Command;var stage=command.Review.Stage;recorded.Require(stage.Context.Company,stage.Context.ActorUserId,command.Reference());var report=command.Review.Completion.Report;
        var result=new CompanyWorkOrderCompletionDecisionFacts(stage.Context.Company,command.Review.Completion.Context.Database,report.Query,stage.Context.ActorUserId,command.Review.Completion.ReviewerPerson!.PersonBaId,stage.AssignedRole,command.Decision,recorded.Receipt.RecordedAtUtc,recorded.Receipt.ApprovalId,recorded.Receipt.HistoryId,report.Revision,command.Review.Completion.SourceRevision(),recorded.Receipt.Revision){WorkSubjectRevision=command.Review.Completion.WorkSubjectRevision()};result.Require();return result;
    }
}
public sealed record CompanyWorkOrderCompletionDecisionStatus(FieldSelectionReview Context,CompanyWorkOrderCompletionQuery Query,CompanyWorkOrderCompletionDecisionFacts? Recorded)
{
    public void Require()
    {
        if(Context is null || Query is null)throw new InvalidOperationException("Original company/account/FIELD/report context is required.");FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Query.Require();Recorded?.Require();
        if(Context.FieldId!=Query.Task.FieldId || Recorded is {} facts && (facts.Company!=Context.Company || facts.CoreDatabase!=Context.Database || facts.Query!=Query))throw new InvalidOperationException("Recorded decision status differs from the original company/account/FIELD/order/report/CORE query.");
    }
}
public interface ICompanyWorkOrderCompletionDecisionReads
{
    Task<CompanyWorkOrderCompletionDecisionFacts?> ReadCompletionDecisionFactsAsync(FieldSelectionReview lifecycle,CompanyWorkOrderCompletionQuery query,Func<Task> authority);
}
