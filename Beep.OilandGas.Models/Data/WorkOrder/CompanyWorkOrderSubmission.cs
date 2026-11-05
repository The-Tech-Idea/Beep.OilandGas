using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyWorkOrderSubmissionReview(CompanyWorkOrderSubmissionInspectionReview Inspections,CompanyMaintenanceAssignedStageReview Stage)
{
    public void Require()
    {
        var context=Inspections.Current.Context;var reference=new CompanyWorkOrderExecutionStartReference(Inspections.Start.Permit,Inspections.Start.IntentId);
        Inspections.Require(context.Company,context.ActorUserId,reference);Stage.Require(context.Company,context.ActorUserId,reference.Permit.Task);
        if(Stage.AssignedRole!=Inspections.Start.AssignedRole || Stage.StepId!=Inspections.Start.StepId)throw new InvalidOperationException("The current configured execution role/stage must match the original start.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Inspections=Inspections.SourceRevision(),Stage}));}
}
public sealed class WORK_ORDER_EXECUTION_SUBMISSION:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string SUBMISSION_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    [Required,MaxLength(128)] public string PERMIT_ID {get;set;}="";
    [Required,MaxLength(128)] public string START_ID {get;set;}="";
    [Required,MaxLength(128)] public string EXECUTOR_USER_ID {get;set;}="";
    [Required,MaxLength(128)] public string EMPLOYEE_BA_ID {get;set;}="";
    [Required] public DateTime COMPLETED_AT_UTC {get;set;}
    [Required,MaxLength(64)] public string SOURCE_REVISION {get;set;}="";
    [Required,MaxLength(8*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
/// <summary>An executor's recorded work report; independent verification, cost and return remain separate.</summary>
public sealed record CompanyWorkOrderSubmissionCommand(CompanyWorkOrderSubmissionReview Review,Guid IntentId,DateTime CompletedAtUtc,
    string WorkPerformed,IReadOnlyList<string> OutstandingDefects,string Reason)
{
    public const int MaximumBodyLength=8*1024*1024;
    private static bool Text(string? value,int limit)=>!string.IsNullOrWhiteSpace(value) && value.Length<=limit && !value.Any(ch=>char.IsControl(ch) && ch is not ('\r' or '\n' or '\t'));
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || CompletedAtUtc.Kind!=DateTimeKind.Utc || !Text(WorkPerformed,16384) || !Text(Reason,4096) || OutstandingDefects is null || OutstandingDefects.Count>64 || OutstandingDefects.Any(value=>!Text(value,2048)) || OutstandingDefects.Distinct(StringComparer.Ordinal).Count()!=OutstandingDefects.Count)
            throw new ArgumentException("Retain the original start/current inspection review, actual completed UTC, explicit work/defects and submission rationale.");
        Review.Require();var comparison=Review.Inspections;
        if(comparison.Blockers.Count!=0 || CompletedAtUtc<comparison.Start.StartedAtUtc || CompletedAtUtc>comparison.ComparedAtUtc || CompletedAtUtc>=comparison.Current.Approval.ValidToUtc || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("Actual work submission requires accepted original applicable inspections and completed UTC after start within the approved window and current comparison.");
    }
    public string SubmissionId(){Require();return "wo-submission-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Inspections.Current.Context.Company,IntentId}));}
    public void RequireCurrent(CompanyWorkOrderSubmissionReview current)
    {
        Require();current.Require();if(current.Inspections.ComparedAtUtc<Review.Inspections.ComparedAtUtc || current.Inspections.Blockers.Count!=0 || current.SourceRevision()!=Review.SourceRevision())throw new InvalidOperationException("Original start/current order/inspections/assignment changed before work submission.");
    }
    public void RequireCurrentStage(CompanyMaintenanceAssignedStageReview stage)
    {
        Require();var context=Review.Inspections.Current.Context;stage.Require(context.Company,context.ActorUserId,Review.Inspections.Start.Permit.Task);
        if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(Review.Stage))throw new InvalidOperationException("Original configured execution stage/role changed before work submission.");
    }
    public WORK_ORDER_EXECUTION_SUBMISSION Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.Inspections.ComparedAtUtc || utc<CompletedAtUtc || utc>=Review.Inspections.Current.Approval.ValidToUtc)throw new InvalidOperationException("Current explicit UTC within the original permit window is required for submission audit.");
        var context=Review.Inspections.Current.Context;var start=Review.Inspections.Start;var task=start.Permit.Task;
        var row=new WORK_ORDER_EXECUTION_SUBMISSION{SUBMISSION_ID=SubmissionId(),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=task.FieldId,WORK_ORDER_ID=task.WorkOrderId,PROCESS_INSTANCE_ID=task.InstanceId,PERMIT_ID=start.PermitId,START_ID=start.StartId,EXECUTOR_USER_ID=context.ActorUserId,EMPLOYEE_BA_ID=start.EmployeeBaId,COMPLETED_AT_UTC=CompletedAtUtc,SOURCE_REVISION=Review.SourceRevision(),COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",PPDM_GUID=IntentId.ToString("D"),SOURCE="OILGAS_WORK_SUBMISSION",ROW_CREATED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_BY=context.ActorUserId,ROW_CHANGED_DATE=utc};
        row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_EXECUTION_SUBMISSION row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(WORK_ORDER_EXECUTION_SUBMISSION row)
    {Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original work report, executor, source or native audit changed.");}
    public static CompanyWorkOrderSubmissionCommand Read(WORK_ORDER_EXECUTION_SUBMISSION row)
    {
        if(string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>MaximumBodyLength)throw new InvalidOperationException("Original bounded work-submission command is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderSubmissionCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original work-submission command is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException){throw new InvalidOperationException("Original work-submission command is malformed.");}
    }
}
public sealed record CompanyWorkOrderSubmissionReceipt(CompanyScope Company,string ActorUserId,Guid IntentId,string SubmissionId,string StartId,DateTime CompletedAtUtc,string RecordedRevision)
{
    public void Require(CompanyWorkOrderSubmissionCommand command)
    {command.Require();var context=command.Review.Inspections.Current.Context;if(Company!=context.Company || ActorUserId!=context.ActorUserId || IntentId!=command.IntentId || SubmissionId!=command.SubmissionId() || StartId!=command.Review.Inspections.Start.StartId || CompletedAtUtc!=command.CompletedAtUtc || !ProcessDefinitionRevision.IsValid(RecordedRevision))throw new InvalidOperationException("Original recorded submission acknowledgement differs.");}
}
public sealed record CompanyWorkOrderSubmissionReference(CompanyWorkOrderExecutionStartReference Start,Guid IntentId)
{
    public void Require(){Start.Require();if(IntentId==Guid.Empty)throw new ArgumentException("Retain the exact original work-submission intent.");}
    public string SubmissionId(CompanyScope company){Require();return "wo-submission-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId}));}
}
public sealed record CompanyWorkOrderSubmissionRecovery(CompanyWorkOrderSubmissionCommand Command,WORK_ORDER_EXECUTION_SUBMISSION Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderSubmissionReference reference)
    {
        reference.Require();Command.Require();var comparison=Command.Review.Inspections;
        if(comparison.Current.Context.Company!=company || comparison.Current.Context.ActorUserId!=actor || Command.IntentId!=reference.IntentId || comparison.Start.IntentId!=reference.Start.IntentId || comparison.Start.Permit!=reference.Start.Permit || Recorded.SUBMISSION_ID!=reference.SubmissionId(company))throw new UnauthorizedAccessException("Original private work report belongs to another company/account/start/reference.");Command.RequireRecorded(Recorded);
    }
}
