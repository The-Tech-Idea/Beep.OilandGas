using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>Original work-permit proposal, never independent approval or permission to execute.</summary>
public sealed record CompanyWorkOrderExecutionPermitIntent(FieldSelectionReview Context,CompanyMaintenanceReceivingQuery Task,
    Guid IntentId,string InspectionRevision,string PermitType,string Jurisdiction,string ScopeDescription,
    string IsolationReference,string RiskReference,string PersonnelReference,string MaterialReference,DateTime ValidFromUtc,DateTime ValidToUtc)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int RequirementEvidenceVersion {get;init;}
    public void Require()
    {
        if(Context is null || Task is null)throw new ArgumentException("The original work-permit account/source review is required.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Task.Require();
        static bool Text(string? value,int limit)=>!string.IsNullOrWhiteSpace(value) && value.Length<=limit && !value.Any(char.IsControl);
        if(RequirementEvidenceVersion is not (0 or 1 or 2) || Context.FieldId!=Task.FieldId || IntentId==Guid.Empty || !ProcessDefinitionRevision.IsValid(InspectionRevision) || !Enum.TryParse<global::Beep.OilandGas.Models.Enums.Shared.PermitToWorkType>(PermitType,out var type) || !Enum.IsDefined(type) || type==global::Beep.OilandGas.Models.Enums.Shared.PermitToWorkType.Unknown || type.ToString()!=PermitType || !Text(Jurisdiction,256) ||
            new[]{ScopeDescription,IsolationReference,RiskReference,PersonnelReference,MaterialReference}.Any(value=>!Text(value,2048)) ||
            ValidFromUtc==default || ValidToUtc==default || ValidFromUtc.Kind!=DateTimeKind.Utc || ValidToUtc.Kind!=DateTimeKind.Utc || ValidToUtc<=ValidFromUtc)
            throw new ArgumentException("Retain the original task/inspection revision, explicit permit type/jurisdiction/evidence references and increasing UTC validity window.");
    }
    public static string PermitIdFor(CompanyScope company,Guid intentId)
    {if(company is null || intentId==Guid.Empty)throw new ArgumentException("The original company/permit reference is required.");return "work-permit-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,IntentId=intentId}));}
    public string PermitId(){Require();return PermitIdFor(Context.Company,IntentId);}
    public void RequireSource(CompanyMaintenanceInspectionReview source)
    {
        Require();source.Require(Context.Company,Context.ActorUserId,Task);
        if(RequirementEvidenceVersion!=2 && source.Inspections.Any(row=>row.ApplicableStage is not null))throw new InvalidOperationException("Explicit inspection applicability requires its additive phase-aware issue policy; an existing retained policy cannot adopt it.");
        if(RequirementEvidenceVersion is 1 or 2 && (source.Inspections.Count==0 || source.Inspections.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision))))throw new InvalidOperationException("Versioned issue requires complete original immutable native requirement proof.");
        if(RequirementEvidenceVersion==2)WorkOrderInspectionApplicability.RequireComplete(source.Inspections);
        if(source.InspectionDatabase!=Context.Database || source.SnapshotRevision!=InspectionRevision)throw new InvalidOperationException("The original inspection/core database evidence changed before permit issue.");
    }
    public WORK_ORDER_EXECUTION_PERMIT Prepare(DateTime utc,CompanyMaintenanceInspectionReview source)
    {
        RequireSource(source);if(utc==default || utc.Kind!=DateTimeKind.Utc)throw new ArgumentException("A recorded UTC issue time is required.");
        var sourceJson=JsonSerializer.Serialize(source);if(sourceJson.Length>131072)throw new InvalidOperationException("The complete original inspection source exceeds the retained evidence capacity.");
        var row=PrepareBody(utc);row.INSPECTION_DATA_JSON=sourceJson;return row;
    }
    private WORK_ORDER_EXECUTION_PERMIT PrepareBody(DateTime utc)
    {
        Require();
        return new(){PERMIT_ID=PermitId(),TENANT_ID=Context.Company.TenantId,BUSINESS_ASSOCIATE_ID=Context.Company.BusinessAssociateId,FIELD_ID=Task.FieldId,WORK_ORDER_ID=Task.WorkOrderId,PROCESS_INSTANCE_ID=Task.InstanceId,
            ISSUE_INTENT_ID=IntentId.ToString("D"),ISSUED_BY=Context.ActorUserId,STATUS="ISSUED_PENDING_APPROVAL",INSPECTION_REVISION=InspectionRevision,PERMIT_TYPE=PermitType,JURISDICTION=Jurisdiction,SCOPE_DESCRIPTION=ScopeDescription,
            ISOLATION_REFERENCE=IsolationReference,RISK_REFERENCE=RiskReference,PERSONNEL_REFERENCE=PersonnelReference,MATERIAL_REFERENCE=MaterialReference,VALID_FROM_UTC=ValidFromUtc,VALID_TO_UTC=ValidToUtc,
            ISSUE_DATA_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=Context.ActorUserId,ROW_CREATED_DATE=utc,PPDM_GUID=Guid.NewGuid().ToString("D"),SOURCE="OILGAS_WORK_ORDER_EXECUTION_PERMIT"};
    }
    public CompanyMaintenanceInspectionReview RequireRecordedSource(WORK_ORDER_EXECUTION_PERMIT row)
    {
        if(row is null || string.IsNullOrWhiteSpace(row.INSPECTION_DATA_JSON) || row.INSPECTION_DATA_JSON.Length>131072)throw new InvalidOperationException("The complete original inspection source is unavailable; independent approval cannot assume it.");
        try
        {
            var source=JsonSerializer.Deserialize<CompanyMaintenanceInspectionReview>(row.INSPECTION_DATA_JSON)??throw new InvalidOperationException("The original inspection source is unavailable.");RequireSource(source);return source;
        }
        catch(JsonException error){throw new InvalidOperationException("The original inspection source is malformed.",error);}
    }
    public void RequireRecorded(WORK_ORDER_EXECUTION_PERMIT row)
    {
        if(row?.INSPECTION_DATA_JSON is not null)RequireRecordedSource(row);
        var original=PrepareBody(DateTime.SpecifyKind(DateTime.UnixEpoch,DateTimeKind.Utc));
        if(row is null || row.PERMIT_ID!=original.PERMIT_ID || row.TENANT_ID!=original.TENANT_ID || row.BUSINESS_ASSOCIATE_ID!=original.BUSINESS_ASSOCIATE_ID || row.FIELD_ID!=original.FIELD_ID || row.WORK_ORDER_ID!=original.WORK_ORDER_ID || row.PROCESS_INSTANCE_ID!=original.PROCESS_INSTANCE_ID ||
            row.ISSUE_INTENT_ID!=original.ISSUE_INTENT_ID || row.ISSUED_BY!=original.ISSUED_BY || row.STATUS!=original.STATUS || row.INSPECTION_REVISION!=original.INSPECTION_REVISION || row.PERMIT_TYPE!=original.PERMIT_TYPE || row.JURISDICTION!=original.JURISDICTION || row.SCOPE_DESCRIPTION!=original.SCOPE_DESCRIPTION ||
            row.ISOLATION_REFERENCE!=original.ISOLATION_REFERENCE || row.RISK_REFERENCE!=original.RISK_REFERENCE || row.PERSONNEL_REFERENCE!=original.PERSONNEL_REFERENCE || row.MATERIAL_REFERENCE!=original.MATERIAL_REFERENCE || row.VALID_FROM_UTC!=original.VALID_FROM_UTC || row.VALID_TO_UTC!=original.VALID_TO_UTC ||
            row.ISSUE_DATA_JSON!=original.ISSUE_DATA_JSON || row.ACTIVE_IND!="Y" || row.ROW_CREATED_BY!=Context.ActorUserId || row.ROW_CREATED_DATE is null || row.ROW_CREATED_DATE==default(DateTime) || !Guid.TryParse(row.PPDM_GUID,out var guid) || guid==Guid.Empty || row.SOURCE!=original.SOURCE ||
            !string.IsNullOrEmpty(row.ROW_CHANGED_BY) || row.ROW_CHANGED_DATE is not null || row.DECISION_INTENT_ID is not null || row.DECIDED_BY is not null || row.DECIDED_AT_UTC is not null || row.DECISION_DATA_JSON is not null)
            throw new InvalidOperationException("The original pending permit body/company/source/audit evidence is unconfirmed; it cannot be repaired or adopted.");
    }
}
public sealed record CompanyWorkOrderExecutionPermitReceipt(CompanyScope Company,string Actor,Guid IntentId,string PermitId,string RecordedRevision)
{
    public void Require(CompanyWorkOrderExecutionPermitIntent intent)
    {intent.Require();if(Company!=intent.Context.Company || Actor!=intent.Context.ActorUserId || IntentId!=intent.IntentId || PermitId!=intent.PermitId() || !ProcessDefinitionRevision.IsValid(RecordedRevision))throw new InvalidOperationException("The original pending work-permit receipt is unconfirmed.");}
}
public sealed record CompanyWorkOrderExecutionPermitIssueReview(FieldSelectionReview Context,CompanyMaintenanceInspectionReview Source)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)] public int RequirementEvidenceVersion {get;init;}
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReceivingQuery task)
    {
        if(Context is null || Source is null)throw new InvalidOperationException("The current permit issue review is unavailable.");
        FieldSelectionReviewValidator.RequireReview(Context,company,actor);Source.Require(company,actor,task);
        if(RequirementEvidenceVersion is not (0 or 1 or 2) || RequirementEvidenceVersion is 1 or 2 && (Source.Inspections.Count==0 || Source.Inspections.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision))))throw new InvalidOperationException("Complete versioned native requirement evidence is unavailable.");
        if(RequirementEvidenceVersion==2)WorkOrderInspectionApplicability.RequireComplete(Source.Inspections);
        if(Context.FieldId!=task.FieldId || Context.Database!=Source.InspectionDatabase)throw new InvalidOperationException("The issue review differs from the original work-order/core source.");
    }
}
public sealed record CompanyWorkOrderExecutionPermitReference(CompanyMaintenanceReceivingQuery Task,Guid IntentId)
{
    public void Require(){if(Task is null || IntentId==Guid.Empty)throw new ArgumentException("The original task/permit issue reference is required.");Task.Require();}
}
public sealed record CompanyWorkOrderExecutionPermitRecovery(CompanyWorkOrderExecutionPermitIntent Intent,CompanyWorkOrderExecutionPermitReceipt Receipt)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(Intent is null || Receipt is null)throw new InvalidOperationException("The original pending permit intent/receipt is unavailable.");Intent.Require();Receipt.Require(Intent);
        if(Intent.Context.Company!=company || Intent.Context.ActorUserId!=actor || Intent.Task!=query.Task || Intent.IntentId!=query.IntentId)throw new InvalidOperationException("The recovered pending permit differs from its original company/account/task/reference.");
    }
}
