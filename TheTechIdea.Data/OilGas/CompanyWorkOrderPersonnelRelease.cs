using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

/// <summary>Immutable cancellation of one original duty assignment; preserves its original window and audit.</summary>
public sealed class WORK_ORDER_PERSONNEL_RELEASE:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string RELEASE_ID {get;set;}="";
    [Required,MaxLength(128)] public string ASSIGNMENT_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string FIELD_ID {get;set;}="";
    [Required,MaxLength(128)] public string WORK_ORDER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PROCESS_INSTANCE_ID {get;set;}="";
    [Required,MaxLength(64)] public string PERMIT_INTENT_ID {get;set;}="";
    public DateTime RELEASED_AT_UTC {get;set;}
    [Required,MaxLength(4*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
public sealed record CompanyWorkOrderPersonnelReleaseCommand(CompanyWorkOrderPersonnelAssignmentRecovery Original,Guid IntentId,string Reason)
{
    public void Require()
    {
        if(Original?.Command is null || Original.Recorded is null || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl))throw new ArgumentException("Retain the complete original personnel assignment, new cancellation intent and explicit rationale.");
        var context=Original.Command.Review.Personnel.Context;
        Original.Require(context.Company,context.ActorUserId,new(Original.Command.Review.Personnel.Query.Permit,Original.Command.IntentId));
        if(JsonSerializer.Serialize(this).Length>4*1024*1024)throw new InvalidOperationException("Original personnel release exceeds its bounded retained body.");
    }
    public string ReleaseId(){Require();return ReleaseIdFor(Original.Recorded.ASSIGNMENT_ID);}
    public static string ReleaseIdFor(string assignmentId)=>"wo-personnel-release-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{AssignmentId=assignmentId}));
    public WORK_ORDER_PERSONNEL_RELEASE Prepare(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc || utc<Original.Recorded.ROW_CREATED_DATE)throw new InvalidOperationException("Explicit release audit UTC must follow the original assignment.");
        var original=Original.Recorded;var row=new WORK_ORDER_PERSONNEL_RELEASE{RELEASE_ID=ReleaseId(),ASSIGNMENT_ID=original.ASSIGNMENT_ID,OWNER_TENANT_ID=original.OWNER_TENANT_ID,OWNER_COMPANY_BA_ID=original.OWNER_COMPANY_BA_ID,FIELD_ID=original.FIELD_ID,WORK_ORDER_ID=original.WORK_ORDER_ID,PROCESS_INSTANCE_ID=original.PROCESS_INSTANCE_ID,PERMIT_INTENT_ID=original.PERMIT_INTENT_ID,RELEASED_AT_UTC=utc,COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=original.ROW_CREATED_BY,ROW_CHANGED_BY=original.ROW_CREATED_BY,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,PPDM_GUID=IntentId.ToString("D"),SOURCE="OILGAS_WORK_ORDER_PERSONNEL_RELEASE"};
        row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_PERSONNEL_RELEASE row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderPersonnelReleaseCommand Read(WORK_ORDER_PERSONNEL_RELEASE row)
    {
        if(row is null || string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>4*1024*1024)throw new InvalidOperationException("Original bounded personnel release proof is unavailable.");
        try{return JsonSerializer.Deserialize<CompanyWorkOrderPersonnelReleaseCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original release proof is unavailable.");}
        catch(JsonException){throw new InvalidOperationException("Original release proof is malformed.");}
    }
    public void RequireRecorded(WORK_ORDER_PERSONNEL_RELEASE row)
    {
        Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Complete original personnel release company/task/body/audit changed.");
    }
}
public sealed record CompanyWorkOrderPersonnelReleaseFacts(string AssignmentId,string AssignmentRevision,string ReleaseId,string ReleasedBy,DateTime ReleasedAtUtc,string RecordedRevision)
{
    public void Require(CompanyWorkOrderPersonnelAssignmentFacts original)
    {
        original.Require();if(AssignmentId!=original.AssignmentId || AssignmentRevision!=original.RecordedRevision || ReleaseId!=CompanyWorkOrderPersonnelReleaseCommand.ReleaseIdFor(AssignmentId) ||
            ReleasedBy!=original.AssignedBy || ReleasedAtUtc.Kind!=DateTimeKind.Utc || ReleasedAtUtc<original.AssignedAtUtc || !ProcessDefinitionRevision.IsValid(RecordedRevision))throw new InvalidOperationException("Public cancellation does not match the complete original assignment/maker/audit.");
    }
    public static CompanyWorkOrderPersonnelReleaseFacts From(WORK_ORDER_PERSONNEL_RELEASE row,WORK_ORDER_PERSONNEL_ASSIGNMENT original)
    {
        var command=CompanyWorkOrderPersonnelReleaseCommand.Read(row);command.RequireRecorded(row);
        if(JsonSerializer.Serialize(command.Original.Recorded)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Cancellation does not match the complete original native assignment.");
        var result=new CompanyWorkOrderPersonnelReleaseFacts(row.ASSIGNMENT_ID,original.RECORDED_REVISION,row.RELEASE_ID,row.ROW_CREATED_BY!,row.RELEASED_AT_UTC,row.RECORDED_REVISION);
        result.Require(CompanyWorkOrderPersonnelAssignmentFacts.From(original));return result;
    }
}
public sealed record CompanyWorkOrderPersonnelReleaseReference(CompanyWorkOrderPersonnelAssignmentReference Assignment,Guid IntentId)
{
    public void Require(){if(Assignment is null || IntentId==Guid.Empty)throw new ArgumentException("Retain the original assignment and personnel release intent references.");Assignment.Require();}
}
public sealed record CompanyWorkOrderPersonnelReleaseRecovery(CompanyWorkOrderPersonnelReleaseCommand Command,WORK_ORDER_PERSONNEL_RELEASE Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderPersonnelReleaseReference reference)
    {
        reference.Require();Command.Require();Command.Original.Require(company,actor,reference.Assignment);
        if(Command.IntentId!=reference.IntentId)throw new InvalidOperationException("Original cancellation intent changed.");Command.RequireRecorded(Recorded);
    }
}
public interface ICompanyWorkOrderPersonnelReleaseOwner
{
    Task<WORK_ORDER_PERSONNEL_RELEASE> RecordPersonnelReleaseAsync(CompanyWorkOrderPersonnelReleaseCommand command,Func<Task> requireAuthority,bool readOnly=false);
    Task<CompanyWorkOrderPersonnelReleaseRecovery> RecoverPersonnelReleaseReferenceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelReleaseReference reference,Func<Task> requireAuthority);
}
