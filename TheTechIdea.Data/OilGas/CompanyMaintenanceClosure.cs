using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
public sealed record CompanyMaintenanceClosureReview(CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderReturnFacts Acceptance,CompanyWorkOrderReturnReview Current)
{
    public void Require()
    {
        if(Stage is null || Acceptance is null || Current is null)throw new InvalidOperationException("Original terminal return assignment, recorded acceptance and current physical evidence are required.");Acceptance.Require();Current.RequireAcceptance();Stage.Require(Current.Completion.Context.Company,Current.Completion.Context.ActorUserId,Acceptance.Query.Task);
        if(Stage!=Current.Assignment.Stage || Acceptance.Query!=Current.Completion.Report.Query || Acceptance.Company!=Stage.Context.Company || Acceptance.CoreDatabase!=Current.Completion.Context.Database || Acceptance.Role!=Stage.AssignedRole || Acceptance.ReportRevision!=Current.Completion.Report.Revision || Acceptance.PhysicalSubjectRevision is null || Acceptance.PhysicalSubjectRevision!=Current.PhysicalSubjectRevision() || Acceptance.RecordedAtUtc>Current.Completion.ComparedAtUtc)throw new InvalidOperationException("Current terminal role/work/return evidence differs from original recorded physical acceptance.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Stage,Acceptance,Current=Current.SourceRevision()}));}
}
public sealed record CompanyMaintenanceClosureCommand(CompanyMaintenanceClosureReview Review,Guid IntentId,string Reason,bool Confirmed)
{
    public void Require(){if(Review is null)throw new ArgumentException("Original reviewed maintenance closure is required.");Review.Require();if(IntentId==Guid.Empty || !Confirmed || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>1024*1024)throw new ArgumentException("Confirm the original terminal return duty/current evidence, explicit reason and retained closure intent.");}
    public CompanyMaintenanceClosureReference Reference()=>new(Review.Acceptance.Query,IntentId);
    public string HistoryId(){Require();return "maintenance-close-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Stage.Context.Company,Review.Stage.Task,IntentId}));}
}
public sealed record CompanyMaintenanceClosureReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original report and closure intent are required.");Query.Require();}}
public sealed record CompanyMaintenanceClosureReceipt(CompanyScope Company,string ActorUserId,Guid IntentId,string InstanceId,string SubmissionId,string StepInstanceId,string HistoryId,IReadOnlyList<string> ClosedTaskIds,DateTime RecordedAtUtc,string Revision)
{
    public void Require(CompanyMaintenanceClosureCommand command){command.Require();var stage=command.Review.Stage;if(Company!=stage.Context.Company || ActorUserId!=stage.Context.ActorUserId || IntentId!=command.IntentId || InstanceId!=stage.Task.InstanceId || SubmissionId!=command.Reference().Query.SubmissionId || StepInstanceId!=stage.StepInstanceId || HistoryId!=command.HistoryId() || ClosedTaskIds is not {Count:>0 and <=128} || ClosedTaskIds.Any(id=>!LeaseSummaryValidator.Identifier(id) || id.Length>128) || ClosedTaskIds.Distinct(StringComparer.Ordinal).Count()!=ClosedTaskIds.Count || RecordedAtUtc.Kind!=DateTimeKind.Utc || RecordedAtUtc<command.Review.Current.Completion.ComparedAtUtc || !ProcessDefinitionRevision.IsValid(Revision))throw new InvalidOperationException("Recorded closure differs from its original company/account/source/tasks/UTC/intent.");}
}
public sealed record CompanyMaintenanceClosureRecovery(CompanyMaintenanceClosureCommand Command,CompanyMaintenanceClosureReceipt Receipt)
{public void Require(CompanyScope company,string actor,CompanyMaintenanceClosureReference reference){reference.Require();if(Command?.Review?.Stage?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original private closure belongs to another company/account/report/reference.");Receipt.Require(Command);}}
public sealed record CompanyMaintenanceClosureFacts(CompanyScope Company,AssetDatabaseScope CoreDatabase,CompanyWorkOrderCompletionQuery Query,string ActorUserId,string Role,string HistoryId,IReadOnlyList<string> ClosedTaskIds,DateTime RecordedAtUtc,string AcceptanceRevision,string PhysicalSubjectRevision,string ReportRevision,string Revision)
{
    public void Require(){PurchaseOrderOwnership.RequireScope(Company);Query.Require();if(CoreDatabase is null || !LeaseSummaryValidator.Identifier(CoreDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(CoreDatabase.Fingerprint) || new[]{ActorUserId,Role,HistoryId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ClosedTaskIds is not {Count:>0 and <=128} || ClosedTaskIds.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || ClosedTaskIds.Distinct(StringComparer.Ordinal).Count()!=ClosedTaskIds.Count || RecordedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || new[]{AcceptanceRevision,PhysicalSubjectRevision,ReportRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)))throw new InvalidOperationException("Original public closure task/audit/source facts are unavailable.");}
    public static CompanyMaintenanceClosureFacts From(CompanyMaintenanceClosureRecovery saved){var command=saved.Command;var stage=command.Review.Stage;saved.Require(stage.Context.Company,stage.Context.ActorUserId,command.Reference());var result=new CompanyMaintenanceClosureFacts(stage.Context.Company,command.Review.Current.Completion.Context.Database,command.Reference().Query,stage.Context.ActorUserId,stage.AssignedRole,saved.Receipt.HistoryId,saved.Receipt.ClosedTaskIds,saved.Receipt.RecordedAtUtc,command.Review.Acceptance.Revision,command.Review.Current.PhysicalSubjectRevision(),command.Review.Acceptance.ReportRevision,saved.Receipt.Revision);result.Require();return result;}
}
public sealed record CompanyMaintenanceClosureStatus(FieldSelectionReview Context,CompanyWorkOrderCompletionQuery Query,CompanyMaintenanceClosureFacts? Recorded)
{public void Require(){FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Query.Require();Recorded?.Require();if(Context.FieldId!=Query.Task.FieldId || Recorded is {} facts && (facts.Company!=Context.Company || facts.CoreDatabase!=Context.Database || facts.Query!=Query))throw new InvalidOperationException("Original public closure company/account/FIELD/report/CORE differs.");}}
public interface ICompanyMaintenanceClosureOwner
{
    Task<(CompanyMaintenanceReturnAssignment Assignment,CompanyWorkOrderReturnFacts Acceptance)> ReviewMaintenanceClosureStageAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<string?,Task> authority);
    Task<CompanyMaintenanceClosureRecovery> CloseMaintenanceAsync(CompanyMaintenanceClosureCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderReturnReview?>> readEvidence);
    Task<CompanyMaintenanceClosureRecovery> RecoverMaintenanceClosureAsync(FieldSelectionReview context,CompanyMaintenanceClosureReference reference,Func<string?,Task> authority);
    Task<CompanyMaintenanceClosureFacts?> ReadMaintenanceClosureFactsAsync(FieldSelectionReview context,CompanyWorkOrderCompletionQuery query,Func<Task> authority);
}
