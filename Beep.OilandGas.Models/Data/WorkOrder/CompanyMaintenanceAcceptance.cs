using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Independent receiving review; assignment acceptance does not authorize execution or return to service.</summary>
public sealed record CompanyMaintenanceAcceptanceReview(FieldSelectionReview Context,CompanyMaintenanceRecoveryQuery Assignment,
    string InstanceId,string ProcessId,string DefinitionRevision,string ProcessRevision,string StepInstanceId,string AssignedRole,
    string WorkflowStarter,ProductionInterventionMaintenanceLink Source)
{
    public void Require()
    {
        if(Context is null || Assignment is null || Source is null)throw new InvalidOperationException("The original maintenance receiving review is unavailable.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Assignment.Require();
        Source.Require(Context.Company,Context.ActorUserId,Assignment.FieldId,Assignment.WorkOrderId,Assignment.WellId,Assignment.DecisionId);
        if(Context.FieldId!=Assignment.FieldId || InstanceId!=CompanyMaintenanceProcessStart.InstanceIdFor(Context.Company,Assignment.IntentId) ||
            new[]{InstanceId,ProcessId,StepInstanceId,AssignedRole,WorkflowStarter}.Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
            !ProcessDefinitionRevision.IsValid(DefinitionRevision) || !ProcessDefinitionRevision.IsValid(ProcessRevision) || Context.ActorUserId==WorkflowStarter || Context.ActorUserId==Source.Reviewer)
            throw new InvalidOperationException("The configured receiving role requires an actor independent of the original starter and production reviewer.");
    }
}
public sealed record CompanyMaintenanceAcceptanceCommand(CompanyMaintenanceAcceptanceReview Review,Guid IntentId,string Notes)
{
    public void Require()
    {
        if(Review is null)throw new ArgumentException("The receiving review is required.");Review.Require();
        if(IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Notes) || Notes.Length>2000 || Notes.Any(value=>char.IsControl(value) && value is not ('\r' or '\n' or '\t')))
            throw new ArgumentException("Retain the original acceptance identity and a bounded receiving rationale.");
    }
}
public sealed record CompanyMaintenanceAcceptanceReceipt(CompanyScope Company,string Actor,Guid IntentId,string InstanceId,
    string StepInstanceId,string HistoryId,string ApprovalId,string? NextStepInstanceId,IReadOnlyList<string> CompletedTaskIds,
    IReadOnlyList<string> NextTaskIds,string RecordedRevision)
{
    public void Require(CompanyMaintenanceAcceptanceCommand command)
    {
        command.Require();var review=command.Review;
        static bool Invalid(IReadOnlyList<string>? ids)=>ids is null || ids.Count>128 || ids.Any(value=>!LeaseSummaryValidator.Identifier(value)) || ids.Distinct(StringComparer.Ordinal).Count()!=ids.Count;
        if(Company!=review.Context.Company || Actor!=review.Context.ActorUserId || IntentId!=command.IntentId || InstanceId!=review.InstanceId || StepInstanceId!=review.StepInstanceId ||
            !LeaseSummaryValidator.Identifier(HistoryId) || !LeaseSummaryValidator.Identifier(ApprovalId) || NextStepInstanceId is not null && !LeaseSummaryValidator.Identifier(NextStepInstanceId) ||
            Invalid(CompletedTaskIds) || CompletedTaskIds.Count==0 || Invalid(NextTaskIds) || (NextStepInstanceId is null)!=(NextTaskIds.Count==0) || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("The original independent acceptance graph/task/audit receipt is unconfirmed.");
    }
}
public interface ICompanyMaintenanceAcceptanceCommands
{
    Task<CompanyMaintenanceRecoveryQuery> DiscoverMaintenanceReceivingAssignmentAsync(FieldSelectionReview current,CompanyMaintenanceReceivingQuery task,Func<string?,Task> requireCurrentReceivingAuthority);
    Task<CompanyMaintenanceAcceptanceCommand> ReadOriginalMaintenanceAcceptanceAsync(FieldSelectionReview current,CompanyMaintenanceAcceptanceReferenceQuery query,Func<string?,Task> requireCurrentReceivingAuthority);
    Task<CompanyMaintenanceAcceptanceReview> ReviewMaintenanceAcceptanceAsync(FieldSelectionReview current,CompanyMaintenanceRecoveryQuery query,
        ProductionInterventionMaintenanceLink source,Func<string?,Task> requireCurrentReceivingAuthority);
    Task<CompanyMaintenanceAcceptanceReceipt> AcceptMaintenanceAsync(CompanyMaintenanceAcceptanceCommand command,
        Func<string?,Task> requireCurrentReceivingAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource);
    Task<CompanyMaintenanceAcceptanceReceipt> RecoverMaintenanceAcceptanceAsync(CompanyMaintenanceAcceptanceCommand command,
        Func<string?,Task> requireCurrentReceivingAuthority,Func<Task<ProductionInterventionMaintenanceLink>> readOriginalSource);
}

public sealed record CompanyMaintenanceReceivingQuery(string InstanceId,string FieldId,string WorkOrderId)
{
    public void Require()
    {
        if(new[]{InstanceId,FieldId,WorkOrderId}.Any(value=>!LeaseSummaryValidator.Identifier(value)) || !InstanceId.StartsWith(CompanyMaintenanceProcessStart.InstancePrefix,StringComparison.Ordinal))
            throw new ArgumentException("The original recorded maintenance process/FIELD/order task identity is required.");
    }
}
public sealed record CompanyMaintenanceAcceptanceReferenceQuery(CompanyMaintenanceReceivingQuery Task,Guid IntentId)
{
    public void Require(){if(Task is null)throw new ArgumentException("The original receiving task is required.");Task.Require();if(IntentId==Guid.Empty)throw new ArgumentException("The original receiving acceptance reference is required.");}
}
public sealed record CompanyMaintenanceAcceptanceRecovery(CompanyMaintenanceAcceptanceCommand Command,CompanyMaintenanceAcceptanceReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceAcceptanceReferenceQuery query)
    {
        query.Require();if(Command is null || Receipt is null)throw new InvalidOperationException("The original receiving command and receipt are unavailable.");Command.Require();Receipt.Require(Command);
        var review=Command.Review;
        if(review.Context.Company!=company || review.Context.ActorUserId!=actor || Command.IntentId!=query.IntentId || review.InstanceId!=query.Task.InstanceId || review.Assignment.FieldId!=query.Task.FieldId || review.Assignment.WorkOrderId!=query.Task.WorkOrderId)
            throw new InvalidOperationException("The recovered acceptance differs from the original company/account/FIELD/order/task reference.");
    }
}
