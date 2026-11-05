using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.ProductionOperations.Models;

public sealed record CompanyMaintenanceExecutionHandoffReview(CompanyMaintenanceAssignedStageReview Stage,CompanyWorkOrderSubmissionFacts Report,
    string NextStepId,string NextStepInstanceId,string NextRole)
{
    public void Require()
    {
        if(Stage is null || Report is null)throw new InvalidOperationException("Original configured executor assignment and recorded work report are required.");
        Stage.Require(Stage.Context.Company,Stage.Context.ActorUserId,Report.Query.Task);Report.Require();
        if(Report.Company!=Stage.Context.Company || Report.ExecutorUserId!=Stage.Context.ActorUserId || Report.ExecutionStepId!=Stage.StepId || Report.ExecutionRole!=Stage.AssignedRole || new[]{NextStepId,NextStepInstanceId,NextRole}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || NextStepId==Stage.StepId || NextRole==Stage.AssignedRole)
            throw new InvalidOperationException("Original executor and a distinct immediately configured independent review role are required.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
}
public sealed record CompanyMaintenanceExecutionHandoffCommand(CompanyMaintenanceExecutionHandoffReview Review,Guid IntentId,string Reason)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed work-report handoff is required.");Review.Require();
        if(IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.Serialize(this).Length>1024*1024)
            throw new ArgumentException("Retain the original report/role/source, handoff intent and explicit reason.");
    }
    public CompanyMaintenanceExecutionHandoffReference Reference()=>new(Review.Report.Query,IntentId);
}
public sealed record CompanyMaintenanceExecutionHandoffReference(CompanyWorkOrderCompletionQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original recorded report and handoff intent reference are required.");Query.Require();}
}
public sealed record CompanyMaintenanceExecutionHandoffReceipt(CompanyScope Company,string ExecutorUserId,Guid IntentId,string InstanceId,string SubmissionId,
    string ClosedStepInstanceId,string ReviewerStepInstanceId,string ReviewerRole,string HistoryId,IReadOnlyList<string> ClosedTaskIds,IReadOnlyList<string> ReviewerTaskIds,string Revision)
{
    public void Require(CompanyMaintenanceExecutionHandoffCommand command)
    {
        command.Require();var original=command.Review;var task=original.Report.Query.Task;
        if(Company!=original.Report.Company || ExecutorUserId!=original.Report.ExecutorUserId || IntentId!=command.IntentId || InstanceId!=task.InstanceId || SubmissionId!=original.Report.Query.SubmissionId ||
            ClosedStepInstanceId!=original.Stage.StepInstanceId || ReviewerStepInstanceId!=original.NextStepInstanceId || ReviewerRole!=original.NextRole || !LeaseSummaryValidator.Identifier(HistoryId) ||
            ClosedTaskIds is null || ReviewerTaskIds is null || ClosedTaskIds.Count is <1 or >128 || ReviewerTaskIds.Count is <1 or >128 ||
            ClosedTaskIds.Concat(ReviewerTaskIds).Any(id=>!LeaseSummaryValidator.Identifier(id)) || ClosedTaskIds.Concat(ReviewerTaskIds).Distinct(StringComparer.Ordinal).Count()!=ClosedTaskIds.Count+ReviewerTaskIds.Count || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("Recorded executor/reviewer task handoff does not match the retained original command.");
    }
}
public sealed record CompanyMaintenanceExecutionHandoffRecovery(CompanyMaintenanceExecutionHandoffCommand Command,CompanyMaintenanceExecutionHandoffReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceExecutionHandoffReference reference)
    {reference.Require();if(Command?.Review?.Stage?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original private execution handoff belongs to another company/account/report/reference.");Receipt.Require(Command);}
}
public interface ICompanyMaintenanceExecutionHandoffOwner
{
    Task<CompanyMaintenanceExecutionHandoffReview> ReviewExecutionHandoffAsync(CompanyMaintenanceAssignedStageReview stage,CompanyWorkOrderSubmissionFacts report,Func<string?,Task> authority);
    Task<CompanyMaintenanceExecutionHandoffRecovery> RecordExecutionHandoffAsync(CompanyMaintenanceExecutionHandoffCommand command,Func<string?,Task> authority,Func<bool,Task<CompanyWorkOrderSubmissionFacts?>> readReport);
    Task<CompanyMaintenanceExecutionHandoffRecovery> RecoverExecutionHandoffAsync(FieldSelectionReview context,CompanyMaintenanceExecutionHandoffReference reference,Func<string?,Task> authority);
}
