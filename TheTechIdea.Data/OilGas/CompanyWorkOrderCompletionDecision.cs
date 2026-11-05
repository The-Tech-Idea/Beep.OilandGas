using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWorkOrderCompletionDecisionReview(CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderCompletionReview Completion)
{
    public void Require()
    {
        if(Stage is null || Completion is null)throw new InvalidOperationException("Current configured reviewer assignment and completion evidence are required.");
        Completion.Require();var current=Completion.Context;Stage.Require(current.Company,current.ActorUserId,Completion.Report.Query.Task);
        if(Stage.Context.FieldId!=current.FieldId || Stage.AssignedRole==Completion.Report.ExecutionRole)throw new InvalidOperationException("A distinct configured independent review role is required.");
    }
    public void RequireIndependentReviewer()
    {
        Require();var current=Completion.Context;var report=Completion.Report;var person=Completion.ReviewerPerson;var at=Completion.ComparedAtUtc;
        var links=Completion.Identities.Versions.Where(row=>row.UserId==current.ActorUserId && row.EffectiveFromUtc<=at && row.EffectiveToUtc>at).ToArray();
        if(current.ActorUserId==report.ExecutorUserId || person is null || person.PersonBaId==report.EmployeeBaId || links.Length!=1 || links[0].RecordedBy==current.ActorUserId || links[0].PersonBaId!=person.PersonBaId || links[0].PersonType!=person.Type ||
            !Covers(person.EffectiveFromUtc,person.EffectiveToUtc) || !Covers(person.Employment.EffectiveDate,person.Employment.ExpiryDate))throw new UnauthorizedAccessException("Current independently recorded reviewer identity and effective native person/employment, distinct from the executor, are required.");
        bool Covers(DateTime? from,DateTime? to)=>from is {} start && to is {} end && start.Kind==DateTimeKind.Utc && end.Kind==DateTimeKind.Utc && start<=at && end>at;
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Stage,Completion=Completion.SourceRevision()}));}
}
public sealed record CompanyWorkOrderCompletionDecisionCommand(CompanyWorkOrderCompletionDecisionReview Review,Guid IntentId,string Decision,string Reason)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed completion evidence is required.");Review.RequireIndependentReviewer();
        if(IntentId==Guid.Empty || Decision is not ("VERIFIED" or "REJECTED") || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>1024*1024)throw new ArgumentException("Retain the original completion evidence, decision, explicit reason and intent reference.");
        if(Decision=="VERIFIED" && Review.Completion.Blockers.Any(value=>value!=CompanyWorkOrderCompletionReview.PendingVerification))throw new InvalidOperationException("Every current completion and independence blocker must be resolved before verification.");
    }
    public CompanyWorkOrderCompletionDecisionReference Reference()=>new(Review.Completion.Report.Query,IntentId);
    public string ApprovalId(){Require();return "work-verification-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Stage.Context.Company,Review.Stage.Task,IntentId}));}
    public string HistoryId()=>ApprovalId()+"-history";
}
public sealed record CompanyWorkOrderCompletionDecisionReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original report and completion decision reference are required.");Query.Require();}
}
public sealed record CompanyWorkOrderCompletionDecisionReceipt(CompanyScope Company,string ReviewerUserId,Guid IntentId,string SubmissionId,string StepInstanceId,string ApprovalId,string HistoryId,string Decision,DateTime RecordedAtUtc,string Revision)
{
    public void Require(CompanyWorkOrderCompletionDecisionCommand command)
    {
        command.Require();var stage=command.Review.Stage;
        if(Company!=stage.Context.Company || ReviewerUserId!=stage.Context.ActorUserId || IntentId!=command.IntentId || SubmissionId!=command.Review.Completion.Report.Query.SubmissionId || StepInstanceId!=stage.StepInstanceId || ApprovalId!=command.ApprovalId() || HistoryId!=command.HistoryId() || Decision!=command.Decision || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<command.Review.Completion.ComparedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Recorded completion decision differs from its original source, actor, intent and UTC audit.");
    }
}
public sealed record CompanyWorkOrderCompletionDecisionRecovery(CompanyWorkOrderCompletionDecisionCommand Command,CompanyWorkOrderCompletionDecisionReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderCompletionDecisionReference reference)
    {reference.Require();if(Command?.Review?.Stage?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original completion decision belongs to another company/account/report/reference.");Receipt.Require(Command);}
}
public interface ICompanyWorkOrderCompletionDecisionOwner
{
    Task<CompanyMaintenanceAssignedStageReview> ReviewCompletionDecisionStageAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority);
    Task<CompanyWorkOrderCompletionDecisionRecovery> RecordCompletionDecisionAsync(CompanyWorkOrderCompletionDecisionCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderCompletionReview?>> readCompletion);
    Task<CompanyWorkOrderCompletionDecisionRecovery> RecoverCompletionDecisionAsync(FieldSelectionReview context,CompanyWorkOrderCompletionDecisionReference reference,Func<string?,Task> authority);
}
public interface ICompanyCompletionIdentityCore
{
    Task<CompanyPersonnelIdentityCatalog> ReadCompletionIdentitiesCoreAsync(CompanyPersonnelPolicyContext context,Func<Task> authority);
    Task<CompanyNativePersonFacts> ReadCompletionPersonCoreAsync(CompanyPersonnelPolicyContext context,string personBaId,Func<Task> authority);
}
