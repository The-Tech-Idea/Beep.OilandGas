using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed class WORK_ORDER_PERSONNEL_ASSIGNMENT:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string ASSIGNMENT_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    [Required,MaxLength(64)] public string PERMIT_INTENT_ID {get;set;}="";
    [Required,MaxLength(128)] public string EMPLOYEE_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string LICENSE_ID {get;set;}="";
    [Required,MaxLength(128)] public string DUTY_CODE {get;set;}="";
    public DateTime VALID_FROM_UTC {get;set;}
    public DateTime VALID_TO_UTC {get;set;}
    [Required,MaxLength(2*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
public sealed record CompanyWorkOrderPersonnelAssignmentCommand(CompanyWorkOrderPersonnelSourceReview Review,Guid IntentId,string DutyCode,DateTime ValidFromUtc,DateTime ValidToUtc,string Reason)
{
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || !LeaseSummaryValidator.Identifier(DutyCode) || DutyCode.Length>128 || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) || ValidFromUtc==default || ValidToUtc==default || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ValidToUtc<=ValidFromUtc)throw new ArgumentException("Retain original company personnel source, intent, explicit duty/UTC window and rationale.");
        var context=Review.PermitReview.Context;Review.Require(context.Company,context.ActorUserId,Review.Personnel.Query);var employee=Review.Personnel.Employment??throw new InvalidOperationException("Exact active company employment membership is required.");var license=Review.Personnel.License??throw new InvalidOperationException("Exact recorded employee licence is required.");
        if(Review.Personnel.Query.LicenseId!=Review.PermitReview.Proposal.PersonnelReference || employee.EffectiveDate is not {} start || employee.ExpiryDate is not {} end || start.Kind!=DateTimeKind.Utc || end.Kind!=DateTimeKind.Utc || start>ValidFromUtc || end<ValidToUtc || ValidFromUtc<Review.PermitReview.Proposal.ValidFromUtc || ValidToUtc>Review.PermitReview.Proposal.ValidToUtc || string.IsNullOrWhiteSpace(employee.Status) || string.IsNullOrWhiteSpace(employee.Position))throw new InvalidOperationException("Recorded company membership and the explicit assignment must cover the reported permit window; no missing date/status/position is inferred.");
        if(JsonSerializer.Serialize(this).Length>2*1024*1024)throw new InvalidOperationException("Retained personnel assignment exceeds its bounded native body.");
    }
    public string AssignmentId(){Require();return "wo-personnel-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=Review.Personnel.Context.Company,IntentId}));}
    public WORK_ORDER_PERSONNEL_ASSIGNMENT Prepare(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc || utc<Review.Personnel.ComparedAtUtc || utc>=ValidToUtc)throw new InvalidOperationException("Current explicit assignment audit UTC is required.");var context=Review.Personnel.Context;var query=Review.Personnel.Query;var row=new WORK_ORDER_PERSONNEL_ASSIGNMENT{ASSIGNMENT_ID=AssignmentId(),OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=query.Permit.Task.FieldId,WORK_ORDER_ID=query.Permit.Task.WorkOrderId,PROCESS_INSTANCE_ID=query.Permit.Task.InstanceId,PERMIT_INTENT_ID=query.Permit.IntentId.ToString("D"),EMPLOYEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,DUTY_CODE=DutyCode,VALID_FROM_UTC=ValidFromUtc,VALID_TO_UTC=ValidToUtc,COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=context.ActorUserId,ROW_CHANGED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,PPDM_GUID=IntentId.ToString("D"),SOURCE="OILGAS_WORK_ORDER_PERSONNEL_ASSIGNMENT"};row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_PERSONNEL_ASSIGNMENT row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(WORK_ORDER_PERSONNEL_ASSIGNMENT row)
    {
        Require();if(row.ROW_CREATED_DATE is not {} utc)throw new InvalidOperationException("Original personnel assignment audit is unavailable.");var expected=Prepare(utc);if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Original personnel assignment body/duty/window/owner or complete audit changed.");
    }
    public void RequireCurrent(CompanyWorkOrderPersonnelSourceReview current)
    {
        Require();current.Require(Review.Personnel.Context.Company,Review.Personnel.Context.ActorUserId,Review.Personnel.Query);if(current.Personnel.Context!=Review.Personnel.Context || current.Personnel.Employment!=Review.Personnel.Employment || current.Personnel.License!=Review.Personnel.License || current.PermitReview.Context!=Review.PermitReview.Context || current.PermitReview.RecordedRevision!=Review.PermitReview.RecordedRevision || current.PermitReview.CurrentSource.SnapshotRevision!=Review.PermitReview.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Original company/task/membership/licence/permit/source changed before assignment.");
    }
}
public interface ICompanyWorkOrderPersonnelAssignmentOwner
{
    Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> RecordPersonnelAssignmentAsync(CompanyWorkOrderPersonnelAssignmentCommand command,Func<Task> requireAuthority,Func<Task> requireCurrentSource,bool readOnly=false,Func<Task>? requireCoreSource=null);
    Task<CompanyWorkOrderPersonnelAssignmentRecovery> RecoverPersonnelAssignmentReferenceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelAssignmentReference reference,Func<Task> requireAuthority);
}
public sealed record CompanyWorkOrderPersonnelAssignmentReference(CompanyWorkOrderExecutionPermitReference Permit,Guid IntentId)
{
    public void Require(){if(Permit is null || IntentId==Guid.Empty)throw new ArgumentException("Retain the original personnel assignment and permit/task references.");Permit.Require();}
    public string AssignmentId(CompanyScope company){Require();return "wo-personnel-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId}));}
}
public sealed record CompanyWorkOrderPersonnelAssignmentRecovery(CompanyWorkOrderPersonnelAssignmentCommand Command,WORK_ORDER_PERSONNEL_ASSIGNMENT Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderPersonnelAssignmentReference reference)
    {
        reference.Require();Command.Require();if(Command.Review.Personnel.Context.Company!=company || Command.Review.Personnel.Context.ActorUserId!=actor || Command.IntentId!=reference.IntentId || Command.Review.Personnel.Query.Permit!=reference.Permit)throw new UnauthorizedAccessException("Original private personnel assignment company/actor/task reference changed.");
        Command.RequireRecorded(Recorded);
    }
}
