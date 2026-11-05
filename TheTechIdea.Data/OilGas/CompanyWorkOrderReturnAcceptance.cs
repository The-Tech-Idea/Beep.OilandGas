using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyMaintenanceReturnAssignment(CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderCompletionDecisionFacts Verification,string HandoffHistoryId,DateTime HandoffAtUtc,string HandoffRevision)
{
    public void Require()
    {
        if(Stage is null || Verification is null)throw new InvalidOperationException("Original configured return assignment and recorded verification are required.");Verification.Require();Stage.Require(Stage.Context.Company,Stage.Context.ActorUserId,Verification.Query.Task);
        if(Verification.Company!=Stage.Context.Company || Verification.Decision!="VERIFIED" || Verification.WorkSubjectRevision is null || Stage.AssignedRole==Verification.ReviewerRole || Stage.Context.ActorUserId==Verification.ReviewerUserId || !LeaseSummaryValidator.Identifier(HandoffHistoryId) || HandoffAtUtc.Kind!=DateTimeKind.Utc || HandoffAtUtc<Verification.RecordedAtUtc || !ProcessDefinitionRevision.IsValid(HandoffRevision))throw new InvalidOperationException("Original independent configured return assignment/handoff/verification differs.");
    }
}
public sealed record CompanyWorkOrderReturnReview(CompanyMaintenanceReturnAssignment Assignment,CompanyWorkOrderCompletionReview Completion,IReadOnlyList<CompanyNativePersonFacts> Inspectors,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyMaintenanceReturnAssignment assignment,CompanyWorkOrderCompletionReview completion,IReadOnlyList<CompanyNativePersonFacts> inspectors)
    {
        assignment.Require();new CompanyWorkOrderCompletionDecisionReview(assignment.Stage,completion).Require();completion.Require();var result=completion.Blockers.Where(value=>value!=CompanyWorkOrderCompletionReview.PendingVerification).ToList();var verified=assignment.Verification;var context=completion.Context;var report=completion.Report;
        if(verified.Query!=report.Query || verified.Company!=context.Company || verified.CoreDatabase!=context.Database || inspectors is null || inspectors.Count>128 || inspectors.Any(row=>row is null) || inspectors.Select(row=>row.PersonBaId).Distinct(StringComparer.Ordinal).Count()!=inspectors.Count)throw new InvalidOperationException("Complete original company/report/CORE/return inspector population is required.");
        foreach(var person in inspectors)person.Require(context.Company,person.PersonBaId);
        if(verified.ReportRevision!=report.Revision || verified.WorkSubjectRevision!=completion.WorkSubjectRevision())result.Add("The original independently verified work or completion inspection subject changed.");
        if(context.ActorUserId==verified.ReviewerUserId || completion.ReviewerPerson?.PersonBaId==verified.ReviewerPersonBaId)result.Add("Return approver and original verifier must be different accounts and native people.");
        if(assignment.Stage.AssignedRole==report.ExecutionRole)result.Add("Return approval requires a role distinct from the original executor.");
        if(completion.ComparedAtUtc<assignment.HandoffAtUtc)result.Add("Current return comparison UTC precedes the recorded role handoff.");
        var checks=completion.Requirements.Where(row=>row.ApplicableStage=="RETURN_TO_SERVICE").ToArray();if(checks.Length==0)result.Add("No explicitly approved return-to-service inspections are configured.");
        if(checks.Any(row=>row.Status!="PASS"))result.Add("All current return-to-service inspection results must be accepted.");
        foreach(var check in checks.Where(row=>row.Status=="PASS"))
        {
            if(check.InspectDate is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<assignment.HandoffAtUtc || utc>completion.ComparedAtUtc){result.Add($"Return inspection {check.InspectionId} must follow the handoff and precede the current UTC comparison.");continue;}
            var links=completion.Identities.Versions.Where(row=>row.UserId==check.Inspector && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc && row.RecordedAtUtc<=utc).ToArray();
            if(links.Length!=1 || links[0].RecordedBy==check.Inspector){result.Add($"Return inspector identity for {check.InspectionId} is unconfirmed at inspection UTC.");continue;}
            var link=links[0];var person=inspectors.SingleOrDefault(row=>row.PersonBaId==link.PersonBaId);
            if(check.Inspector==report.ExecutorUserId || check.Inspector==verified.ReviewerUserId || link.PersonBaId==report.EmployeeBaId || link.PersonBaId==verified.ReviewerPersonBaId)result.Add($"Return inspector for {check.InspectionId} is the original executor or verifier.");
            if(person is null || person.Type!=link.PersonType || !Covers(person.EffectiveFromUtc,person.EffectiveToUtc,utc) || !Covers(person.Employment.EffectiveDate,person.Employment.ExpiryDate,utc))result.Add($"Native return inspector/person/employment evidence for {check.InspectionId} is unconfirmed at inspection UTC.");
        }
        return result.ToArray();
        static bool Covers(DateTime? from,DateTime? to,DateTime utc)=>from is {} start && to is {} end && start.Kind==DateTimeKind.Utc && end.Kind==DateTimeKind.Utc && start<=utc && end>utc;
    }
    public void Require(){if(Assignment is null || Completion is null || Inspectors is null || Blockers is null || !GetBlockers(Assignment,Completion,Inspectors).SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Complete current independent return evidence differs.");}
    public void RequireAcceptance(){Require();new CompanyWorkOrderCompletionDecisionReview(Assignment.Stage,Completion).RequireIndependentReviewer();if(Blockers.Count!=0)throw new InvalidOperationException("Resolve every current work/return inspection/identity blocker before accepting physical return.");}
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Assignment,Completion=Completion.SourceRevision(),Inspectors=Inspectors.OrderBy(row=>row.PersonBaId,StringComparer.Ordinal),Blockers}));}
    public string PhysicalSubjectRevision()
    {
        Require();var rows=Completion.Requirements.Where(row=>row.ApplicableStage=="RETURN_TO_SERVICE").OrderBy(row=>row.CondSeq).ToArray();
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Assignment.Verification,Assignment.HandoffHistoryId,Assignment.HandoffAtUtc,Assignment.HandoffRevision,Work=Completion.WorkSubjectRevision(),Results=rows,
            Identities=rows.Where(row=>row.Status=="PASS" && row.InspectDate is not null).Select(row=>new{row.InspectionId,Versions=Completion.Identities.Versions.Where(link=>link.UserId==row.Inspector && link.EffectiveFromUtc<=row.InspectDate!.Value && link.EffectiveToUtc>row.InspectDate!.Value).OrderBy(link=>link.RecordId,StringComparer.Ordinal)}),Inspectors=Inspectors.OrderBy(row=>row.PersonBaId,StringComparer.Ordinal)}));
    }
}
public sealed record CompanyWorkOrderReturnCommand(CompanyWorkOrderReturnReview Review,Guid IntentId,DateTime ReturnedAtUtc,string Reason,bool Confirmed)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original current return review is required.");Review.RequireAcceptance();
        if(IntentId==Guid.Empty || !Confirmed || ReturnedAtUtc.Kind!=DateTimeKind.Utc || ReturnedAtUtc<Review.Assignment.HandoffAtUtc || ReturnedAtUtc>DateTime.UtcNow || Review.Completion.Requirements.Any(row=>row.ApplicableStage=="RETURN_TO_SERVICE" && row.InspectDate>ReturnedAtUtc) || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>1024*1024)throw new ArgumentException("Confirm the original evidence, explicit actual returned UTC after inspections, rationale and retained intent.");
    }
    public CompanyWorkOrderReturnReference Reference()=>new(Review.Completion.Report.Query,IntentId);
    public string ApprovalId(){Require();return "return-acceptance-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Assignment.Stage.Context.Company,Review.Assignment.Stage.Task,IntentId}));}
    public string HistoryId()=>ApprovalId()+"-history";
}
public sealed record CompanyWorkOrderReturnReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original report and return acceptance intent are required.");Query.Require();}}
public sealed record CompanyWorkOrderReturnReceipt(CompanyScope Company,string ApproverUserId,Guid IntentId,string SubmissionId,string StepInstanceId,string ApprovalId,string HistoryId,DateTime ReturnedAtUtc,DateTime RecordedAtUtc,string Revision)
{
    public void Require(CompanyWorkOrderReturnCommand command){command.Require();var stage=command.Review.Assignment.Stage;if(Company!=stage.Context.Company || ApproverUserId!=stage.Context.ActorUserId || IntentId!=command.IntentId || SubmissionId!=command.Review.Completion.Report.Query.SubmissionId || StepInstanceId!=stage.StepInstanceId || ApprovalId!=command.ApprovalId() || HistoryId!=command.HistoryId() || ReturnedAtUtc!=command.ReturnedAtUtc || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<command.Review.Completion.ComparedAtUtc || RecordedAtUtc<ReturnedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Recorded return acceptance differs from the original actor/source/UTC/intent.");}
}
public sealed record CompanyWorkOrderReturnRecovery(CompanyWorkOrderReturnCommand Command,CompanyWorkOrderReturnReceipt Receipt)
{public void Require(CompanyScope company,string actor,CompanyWorkOrderReturnReference reference){reference.Require();if(Command?.Review?.Assignment?.Stage?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original return acceptance belongs to another company/account/report/reference.");Receipt.Require(Command);}}
public sealed record CompanyWorkOrderReturnFacts(CompanyScope Company,AssetDatabaseScope CoreDatabase,CompanyWorkOrderCompletionQuery Query,string ApproverUserId,string ApproverPersonBaId,string Role,DateTime ReturnedAtUtc,DateTime RecordedAtUtc,string ApprovalId,string HistoryId,string ReportRevision,string EvidenceRevision,string Revision)
{
    public string? PhysicalSubjectRevision{get;init;}
    public void Require(){PurchaseOrderOwnership.RequireScope(Company);Query.Require();if(CoreDatabase is null || !LeaseSummaryValidator.Identifier(CoreDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(CoreDatabase.Fingerprint) || new[]{ApproverUserId,ApproverPersonBaId,Role,ApprovalId,HistoryId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ReturnedAtUtc==default || ReturnedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<ReturnedAtUtc || new[]{ReportRevision,EvidenceRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) || PhysicalSubjectRevision is not null && !ProcessDefinitionRevision.IsValid(PhysicalSubjectRevision))throw new InvalidOperationException("Original public return acceptance audit is unavailable.");}
    public static CompanyWorkOrderReturnFacts From(CompanyWorkOrderReturnRecovery saved){var command=saved.Command;var stage=command.Review.Assignment.Stage;saved.Require(stage.Context.Company,stage.Context.ActorUserId,command.Reference());var result=new CompanyWorkOrderReturnFacts(stage.Context.Company,command.Review.Completion.Context.Database,command.Reference().Query,stage.Context.ActorUserId,command.Review.Completion.ReviewerPerson!.PersonBaId,stage.AssignedRole,command.ReturnedAtUtc,saved.Receipt.RecordedAtUtc,saved.Receipt.ApprovalId,saved.Receipt.HistoryId,command.Review.Completion.Report.Revision,command.Review.SourceRevision(),saved.Receipt.Revision){PhysicalSubjectRevision=command.Review.PhysicalSubjectRevision()};result.Require();return result;}
}
public sealed record CompanyWorkOrderReturnStatus(FieldSelectionReview Context,CompanyWorkOrderCompletionQuery Query,CompanyWorkOrderReturnFacts? Recorded)
{public void Require(){FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Query.Require();Recorded?.Require();if(Context.FieldId!=Query.Task.FieldId || Recorded is {} facts && (facts.Company!=Context.Company || facts.CoreDatabase!=Context.Database || facts.Query!=Query))throw new InvalidOperationException("Original return status company/account/FIELD/report/CORE differs.");}}
public interface ICompanyWorkOrderReturnOwner
{
    Task<CompanyMaintenanceReturnAssignment> ReadReturnAssignmentAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority);
    Task<CompanyWorkOrderReturnRecovery> RecordReturnAcceptanceAsync(CompanyWorkOrderReturnCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview?>> readEvidence);
    Task<CompanyWorkOrderReturnRecovery> RecoverReturnAcceptanceAsync(FieldSelectionReview context,CompanyWorkOrderReturnReference reference,Func<string?,Task> authority);
    Task<CompanyWorkOrderReturnFacts?> ReadReturnAcceptanceFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority);
}
