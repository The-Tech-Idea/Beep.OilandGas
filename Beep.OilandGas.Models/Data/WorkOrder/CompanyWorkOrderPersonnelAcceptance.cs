using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed record CompanyWorkOrderPersonnelAcceptanceFacts(Guid IntentId,Guid PermitIntentId,string AssignmentId,string AssignmentRevision,string PolicyRecordId,string PolicyRevision,string PolicyCatalogRevision,string CredentialRevision,string Reviewer,DateTime AcceptedAtUtc,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int RequirementReferenceVersion {get;init;}
}
public sealed record CompanyWorkOrderPersonnelAcceptanceCommand(CompanyPersonnelPolicyEvidenceReview Review,string InspectionId,Guid IntentId,string Reason)
{
    public const int MaximumBodyLength=8*1024*1024;
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int RequirementReferenceVersion {get;init;}
    public static string RequirementReference(CompanyPersonnelPolicyEvidenceReview evidence,int version)=>version switch{0=>evidence.Assignment.Assignment.AssignmentId,1 when evidence.Assignment.CurrentPersonnel.PermitReview.Proposal.RequirementEvidenceVersion is 1 or 2=>evidence.Assignment.CurrentPersonnel.PermitReview.Proposal.PersonnelReference,_=>throw new InvalidOperationException("Retained personnel requirement reference policy is unsupported.")};
    private sealed record StoredProof(CompanyWorkOrderPersonnelAcceptanceCommand Command,string PendingRowJson);
    [System.Text.Json.Serialization.JsonIgnore] public InspectionCondition OriginalInspection=>Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Inspections.Single(row=>row.InspectionId==InspectionId);
    public void Require()
    {
        if(Review is null || !LeaseSummaryValidator.Identifier(InspectionId) || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl))throw new ArgumentException("Retain exact current personnel policy/source, inspection, new intent and rationale.");
        var assignment=Review.Assignment;var permit=assignment.CurrentPersonnel.PermitReview;var actor=permit.Context.ActorUserId;Review.Require(permit.Context.Company,actor,assignment.Query);
        if(Review.Blockers.Count!=1 || Review.Blockers[0]!=CompanyPersonnelPolicyEvidenceReview.PendingAcceptanceBlocker || Review.Credentials.ComparedAtUtc<permit.Proposal.ValidFromUtc || Review.Credentials.ComparedAtUtc>=permit.Proposal.ValidToUtc || Review.ApplicablePolicy is not {} policy)throw new InvalidOperationException("Complete current uncancelled duty, explicit effective policy and confirmed credential evidence are required before personnel acceptance.");
        if(actor==permit.Proposal.Context.ActorUserId || actor==assignment.Assignment.AssignedBy || actor==policy.RecordedBy)throw new UnauthorizedAccessException("Personnel acceptance reviewer must differ from original permit issuer, assignment maker and policy maker.");
        var selected=permit.CurrentSource.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();
        if(selected.Length!=1 || selected[0].Status!="UNRECORDED" || selected[0].PermitPrerequisiteKind!="PERSONNEL" || selected[0].RegulatoryRef!=RequirementReference(Review,RequirementReferenceVersion) || selected[0].Jurisdiction!=permit.Proposal.Jurisdiction || selected[0].RequirementActor==actor || selected[0].PersonnelAcceptance is not null)throw new InvalidOperationException("Select the exact original unrecorded PERSONNEL assignment requirement with matching jurisdiction and independent requirement author.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Retained personnel acceptance proof exceeds its bounded native capacity.");
    }
    private string Proof(CompanyPersonnelPolicyEvidenceReview current,WORK_ORDER_INSPECTION? own)
    {
        var permit=current.Assignment.CurrentPersonnel.PermitReview;var source=permit.CurrentSource;var selected=source.Inspections.Where(row=>row.InspectionId==InspectionId).ToArray();if(selected.Length!=1)throw new InvalidOperationException("Exact current personnel inspection is unavailable.");
        if(own is null){if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original pending personnel requirement changed.");}
        else{RequireRecorded(own);if(JsonSerializer.Serialize(selected[0])!=JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(own,OriginalInspection)))throw new InvalidOperationException("Only the exact original accepted personnel inspection may close proof.");}
        var normalized=CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,source.Order,source.Inspections.Select(row=>row.InspectionId==InspectionId?OriginalInspection:row).ToArray());
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{current.EligibilityEvidenceVersion,current.Assignment.Query,current.Assignment.Roster.Context,current.Assignment.Roster.Assignments,current.Assignment.Roster.Releases,PermitContext=permit.Context,permit.Proposal,OriginalPermit=permit.OriginalSource.SnapshotRevision,permit.RecordedRevision,permit.PrerequisiteEvidenceVersion,Inspection=normalized,PolicyContext=current.Policies.Context,current.Policies.Versions,PersonnelContext=current.Credentials.Personnel.Context,PersonnelQuery=current.Credentials.Personnel.Query,current.Credentials.Personnel.Employment,current.Credentials.Personnel.License,current.Credentials.Statuses,current.Credentials.Conditions}));
    }
    public void RequireCurrent(CompanyPersonnelPolicyEvidenceReview current,WORK_ORDER_INSPECTION? own=null)
    {
        Require();var context=Review.Assignment.CurrentPersonnel.PermitReview.Context;current.Require(context.Company,context.ActorUserId,Review.Assignment.Query);
        if(current.Blockers.Count!=1 || current.Blockers[0]!=CompanyPersonnelPolicyEvidenceReview.PendingAcceptanceBlocker || current.Credentials.ComparedAtUtc<Review.Credentials.ComparedAtUtc || current.Credentials.ComparedAtUtc<current.Assignment.CurrentPersonnel.PermitReview.Proposal.ValidFromUtc || current.Credentials.ComparedAtUtc>=current.Assignment.CurrentPersonnel.PermitReview.Proposal.ValidToUtc || Proof(current,own)!=Proof(Review,null))throw new InvalidOperationException("Complete original assignment/release/policy/member/licence/status/condition/permit/order/authority proof changed during personnel acceptance.");
    }
    public void RequireCurrentCore(CompanyMaintenanceInspectionReview current,WORK_ORDER_INSPECTION? own)
    {
        Require();var original=Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource;var context=Review.Assignment.CurrentPersonnel.PermitReview.Context;current.Require(context.Company,context.ActorUserId,Review.Assignment.Query.Permit.Task);
        var selected=current.Inspections.Single(row=>row.InspectionId==InspectionId);
        if(own is null){if(JsonSerializer.Serialize(selected)!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Original local personnel requirement changed.");}else{RequireRecorded(own);if(JsonSerializer.Serialize(selected)!=JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(own,OriginalInspection)))throw new InvalidOperationException("Original local accepted personnel row changed.");}
        if(CompanyMaintenanceInspectionReview.Revision(current.Stage,current.InspectionDatabase,current.Order,current.Inspections.Select(row=>row.InspectionId==InspectionId?OriginalInspection:row).ToArray())!=original.SnapshotRevision)throw new InvalidOperationException("Complete original local order/inspection proof changed.");
    }
    public WORK_ORDER_INSPECTION Prepare(WORK_ORDER_INSPECTION pending,DateTime utc)
    {
        Require();var permit=Review.Assignment.CurrentPersonnel.PermitReview;
        if(utc.Kind!=DateTimeKind.Utc || utc<Review.Credentials.ComparedAtUtc || utc>=permit.Proposal.ValidToUtc || JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(pending,OriginalInspection))!=JsonSerializer.Serialize(OriginalInspection))throw new InvalidOperationException("Exact original pending personnel row and current explicit acceptance UTC are required.");
        var row=WorkOrderInspectionRules.PrepareResult(pending,new(){FieldId=pending.FIELD_ID,InspectionId=InspectionId,ExpectedSourceRevision=OriginalInspection.SourceRevision,Result="PASS",Notes=Reason},permit.Context.ActorUserId,utc);
        row.PERSONNEL_ACCEPTANCE_DATA_JSON=JsonSerializer.Serialize(new StoredProof(this,JsonSerializer.Serialize(pending)));if(row.PERSONNEL_ACCEPTANCE_DATA_JSON.Length>MaximumBodyLength)throw new InvalidOperationException("Complete original personnel proof exceeds its native capacity.");row.PERSONNEL_ACCEPTANCE_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_INSPECTION row){var copy=CompanyInventorySourceReview.Copy(row);copy.PERSONNEL_ACCEPTANCE_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyWorkOrderPersonnelAcceptanceCommand Read(WORK_ORDER_INSPECTION row)
    {
        if(row.PERSONNEL_ACCEPTANCE_DATA_JSON is not {Length:>0} body || body.Length>MaximumBodyLength)throw new InvalidOperationException("Original retained personnel acceptance is unavailable.");
        CompanyWorkOrderPersonnelAcceptanceCommand command;try{command=JsonSerializer.Deserialize<StoredProof>(body)?.Command??throw new InvalidOperationException("Original personnel acceptance proof is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original personnel acceptance proof is malformed.");}command.RequireRecorded(row);return command;
    }
    public void RequireRecorded(WORK_ORDER_INSPECTION row)
    {
        Require();WorkOrderInspectionRules.Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);var original=OriginalInspection;var permit=Review.Assignment.CurrentPersonnel.PermitReview;var actor=permit.Context.ActorUserId;
        if(row.PERSONNEL_ACCEPTANCE_DATA_JSON is not {Length:>0} body || body.Length>MaximumBodyLength)throw new InvalidOperationException("Complete original personnel proof is unavailable.");
        StoredProof stored;WORK_ORDER_INSPECTION pending;try{stored=JsonSerializer.Deserialize<StoredProof>(body)??throw new InvalidOperationException("Original proof is unavailable.");pending=JsonSerializer.Deserialize<WORK_ORDER_INSPECTION>(stored.PendingRowJson)??throw new InvalidOperationException("Original pending row is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original proof is malformed.");}
        if(JsonSerializer.Serialize(stored.Command)!=JsonSerializer.Serialize(this) || JsonSerializer.Serialize(WorkOrderInspectionRules.ProjectForRequirement(pending,OriginalInspection))!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Complete original pending personnel requirement/audit changed.");
        if(row.INSPECTION_ID!=InspectionId || row.FIELD_ID!=original.FieldId || row.WORK_ORDER_ID!=original.WorkOrderId || row.COND_SEQ!=original.CondSeq || row.APPLICABLE_STAGE!=original.ApplicableStage || row.COND_TYPE!=original.CondType || row.REQUIRED_TEXT!=original.RequiredText || row.JURISDICTION!=original.Jurisdiction || row.REGULATORY_REF!=original.RegulatoryRef || row.REGULATORY_REF!=RequirementReference(Review,RequirementReferenceVersion) || row.PERMIT_PREREQUISITE_KIND!="PERSONNEL" || row.REQUIREMENT_ACTOR!=original.RequirementActor || row.RESULT_STATUS!="PASS" || row.RESULT_NOTES!=Reason || row.INSPECTOR!=actor ||
            row.INSPECTED_AT_UTC is not {} utc || utc.Kind!=DateTimeKind.Utc || utc<Review.Credentials.ComparedAtUtc || utc>=permit.Proposal.ValidToUtc || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE!=utc || row.PERSONNEL_ACCEPTANCE_REVISION!=Digest(row))throw new InvalidOperationException("Original personnel acceptance proof/result/complete audit changed.");
        var expected=Prepare(pending,row.INSPECTED_AT_UTC.Value);if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Complete original personnel acceptance/native audit changed.");
    }
    public static string PolicyProof(CompanyPersonnelPolicyCatalog current)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(current.Versions));
    public static string CredentialProof(CompanyPersonnelCredentialFacts current)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{current.Personnel.Query,current.Personnel.Employment,current.Personnel.License,current.Statuses,current.Conditions}));
    public CompanyWorkOrderPersonnelAcceptanceFacts Facts(WORK_ORDER_INSPECTION row){RequireRecorded(row);var p=Review.ApplicablePolicy!;var a=Review.Assignment.Assignment;return new(IntentId,Review.Assignment.Query.Permit.IntentId,a.AssignmentId,a.RecordedRevision,p.RecordId,p.Revision,PolicyProof(Review.Policies),CredentialProof(Review.Credentials),Review.Assignment.CurrentPersonnel.PermitReview.Context.ActorUserId,row.INSPECTED_AT_UTC!.Value,row.PERSONNEL_ACCEPTANCE_REVISION!){RequirementReferenceVersion=RequirementReferenceVersion};}
}
public sealed record CompanyWorkOrderPersonnelAcceptanceReference(CompanyWorkOrderExecutionPermitReference Permit,string InspectionId,Guid IntentId)
{
    public void Require(){if(Permit is null || !LeaseSummaryValidator.Identifier(InspectionId) || IntentId==Guid.Empty)throw new ArgumentException("Retain the exact original permit/inspection/personnel acceptance references.");Permit.Require();}
}
public sealed record CompanyWorkOrderPersonnelAcceptanceRecovery(CompanyWorkOrderPersonnelAcceptanceCommand Command,WORK_ORDER_INSPECTION Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderPersonnelAcceptanceReference reference){reference.Require();if(Command is null || Recorded is null || Command.Review.Assignment.CurrentPersonnel.PermitReview.Context.Company!=company || Command.Review.Assignment.CurrentPersonnel.PermitReview.Context.ActorUserId!=actor || Command.Review.Assignment.Query.Permit!=reference.Permit || Command.InspectionId!=reference.InspectionId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original personnel acceptance company/reviewer/reference differs.");Command.RequireRecorded(Recorded);}
}
public interface ICompanyWorkOrderPersonnelAcceptanceOwner
{
    Task<WORK_ORDER_INSPECTION> RecordPersonnelAcceptanceAsync(CompanyWorkOrderPersonnelAcceptanceCommand command,Func<Task> requireAuthority,Func<WORK_ORDER_INSPECTION?,Task> requireCurrentProof,Func<WORK_ORDER_INSPECTION?,Task> requireCoreProof,bool readOnly=false);
    Task<CompanyWorkOrderPersonnelAcceptanceRecovery> RecoverPersonnelAcceptanceReferenceAsync(FieldSelectionReview context,CompanyWorkOrderPersonnelAcceptanceReference reference,Func<Task> requireAuthority);
}
public interface ICompanyPersonnelAcceptanceCoreProof
{
    Task RequirePersonnelAcceptanceCoreAsync(CompanyPersonnelPolicyEvidenceReview original,Func<Task> requireAuthority);
    Task RequireExecutionPersonnelCoreAsync(FieldSelectionReview executor,CompanyPersonnelPolicyEvidenceReview original,Func<Task> requireAuthority);
}
