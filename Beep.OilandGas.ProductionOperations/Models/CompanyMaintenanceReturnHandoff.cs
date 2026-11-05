using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.ProductionOperations.Models;

public sealed record CompanyMaintenanceReturnHandoffReview(CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderCompletionDecisionFacts Decision,
    CompanyWorkOrderCompletionReview Completion,string NextStepId,string NextStepInstanceId,string NextRole)
{
    public void Require()
    {
        if(Stage is null || Decision is null || Completion is null)throw new InvalidOperationException("Original configured review assignment, recorded verification and current work subject are required.");
        Decision.Require();new CompanyWorkOrderCompletionDecisionReview(Stage,Completion).RequireIndependentReviewer();
        if(Decision.Company!=Completion.Context.Company || Decision.CoreDatabase!=Completion.Context.Database || Decision.Query!=Completion.Report.Query || Decision.ReportRevision!=Completion.Report.Revision || Decision.Decision!="VERIFIED" ||
            Decision.WorkSubjectRevision is null || Decision.WorkSubjectRevision!=Completion.WorkSubjectRevision() || Decision.RecordedAtUtc>Completion.ComparedAtUtc || Completion.Blockers.Any(value=>value!=CompanyWorkOrderCompletionReview.PendingVerification) ||
            new[]{NextStepId,NextStepInstanceId,NextRole}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || NextStepId==Stage.StepId || NextRole==Stage.AssignedRole || NextRole==Completion.Report.ExecutionRole)
            throw new InvalidOperationException("Unchanged independently verified work and a distinct explicitly configured return approval role are required.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Stage,Decision,Completion=Completion.SourceRevision(),NextStepId,NextStepInstanceId,NextRole}));}
}
public interface ICompanyMaintenanceReturnHandoffOwner
{
    Task<CompanyMaintenanceReturnHandoffReview> ReviewReturnHandoffAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,CompanyWorkOrderCompletionReview completion,Func<string?,Task> authority);
    Task<CompanyMaintenanceReturnHandoffRecovery> RecordReturnHandoffAsync(CompanyMaintenanceReturnHandoffCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderCompletionReview?>> readCompletion);
    Task<CompanyMaintenanceReturnHandoffRecovery> RecoverReturnHandoffAsync(FieldSelectionReview context,CompanyMaintenanceReturnHandoffReference reference,Func<string?,Task> authority);
}
public sealed record CompanyMaintenanceReturnHandoffCommand(CompanyMaintenanceReturnHandoffReview Review,Guid IntentId,string Reason)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed return handoff is required.");Review.Require();
        if(IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>1024*1024)throw new ArgumentException("Retain the original verified source, return assignment, explicit reason and intent.");
    }
    public CompanyMaintenanceReturnHandoffReference Reference()=>new(Review.Completion.Report.Query,IntentId);
    public string HistoryId(){Require();return "return-handoff-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Stage.Context.Company,Review.Stage.Task,IntentId}));}
}
public sealed record CompanyMaintenanceReturnHandoffReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original report and return handoff intent are required.");Query.Require();}
}
public sealed record CompanyMaintenanceReturnHandoffReceipt(CompanyScope Company,string ReviewerUserId,Guid IntentId,string InstanceId,string SubmissionId,
    string ClosedStepInstanceId,string ReturnStepInstanceId,string ReturnRole,string HistoryId,IReadOnlyList<string> ClosedTaskIds,IReadOnlyList<string> ReturnTaskIds,DateTime RecordedAtUtc,string Revision)
{
    public void Require(CompanyMaintenanceReturnHandoffCommand command)
    {
        command.Require();var review=command.Review;
        if(Company!=review.Stage.Context.Company || ReviewerUserId!=review.Stage.Context.ActorUserId || IntentId!=command.IntentId || InstanceId!=review.Stage.Task.InstanceId || SubmissionId!=review.Completion.Report.Query.SubmissionId ||
            ClosedStepInstanceId!=review.Stage.StepInstanceId || ReturnStepInstanceId!=review.NextStepInstanceId || ReturnRole!=review.NextRole || HistoryId!=command.HistoryId() || ClosedTaskIds is not {Count:>0 and <=128} || ReturnTaskIds is not {Count:>0 and <=128} ||
            ClosedTaskIds.Concat(ReturnTaskIds).Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ClosedTaskIds.Concat(ReturnTaskIds).Distinct(StringComparer.Ordinal).Count()!=ClosedTaskIds.Count+ReturnTaskIds.Count ||
            RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<review.Completion.ComparedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Recorded reviewer/return task handoff differs from its original source, actor and UTC audit.");
    }
}
public sealed record CompanyMaintenanceReturnHandoffRecovery(CompanyMaintenanceReturnHandoffCommand Command,CompanyMaintenanceReturnHandoffReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReturnHandoffReference reference)
    {reference.Require();if(Command?.Review?.Stage?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original return handoff belongs to another company/account/report/reference.");Receipt.Require(Command);}
}
