using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public sealed class AddWorkOrderInspectionRequirementRequest
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? ApplicableStage {get;set;}
    public string FieldId {get;set;}=string.Empty;
    public int CondSeq {get;set;}
    public string CondType {get;set;}=string.Empty;
    public string RequiredText {get;set;}=string.Empty;
    public string Jurisdiction {get;set;}=string.Empty;
    public string RegulatoryRef {get;set;}=string.Empty;
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? PermitPrerequisiteKind {get;set;}
}
public static class WorkOrderInspectionApplicability
{
    public static IReadOnlyList<string> Stages {get;}=Array.AsReadOnly(new[]{"PRE_START","WORK_SUBMISSION","COMPLETION_VERIFICATION","RETURN_TO_SERVICE"});
    public static bool Valid(string? stage,string type,string? prerequisite)
    {
        if(stage is null)return true;
        if(!Stages.Contains(stage,StringComparer.Ordinal))return false;
        if(prerequisite is not null)return stage=="PRE_START" && type is ConditionType.PreStart or ConditionType.SceVerify;
        return type switch
        {
            ConditionType.PreStart=>stage=="PRE_START",
            ConditionType.Operational=>stage=="WORK_SUBMISSION",
            ConditionType.CloseOut=>stage=="COMPLETION_VERIFICATION",
            ConditionType.Regulatory or ConditionType.SceVerify=>true,
            _=>false
        };
    }
    public static void RequireComplete(IReadOnlyList<InspectionCondition> rows)
    {
        if(rows.Count==0 || rows.Any(row=>row is null || row.ApplicableStage is null || !Valid(row.ApplicableStage,row.CondType,row.PermitPrerequisiteKind)))throw new InvalidOperationException("The complete original inspection population requires explicit consistent applicability before phase-aware issue.");
    }
}
public static class WorkOrderPermitPrerequisiteKinds
{
    public static IReadOnlyList<string> All {get;}=Array.AsReadOnly(new[]{"ISOLATION","RISK","PERSONNEL","MATERIAL"});
    public static bool Valid(string? kind,string? reference,string type)=>kind is null || All.Contains(kind,StringComparer.Ordinal) && !string.IsNullOrWhiteSpace(reference) && type is ConditionType.PreStart or ConditionType.SceVerify;
}
public static class WorkOrderInspectionRules
{
    public static void RequireProjected(string field,string order,IReadOnlyList<InspectionCondition>? rows)
    {
        if(string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(order) || rows is null || rows.Any(r=>r is null || r.FieldId!=field || r.WorkOrderId!=order || string.IsNullOrWhiteSpace(r.InspectionId) || r.CondSeq<=0 ||
           string.IsNullOrWhiteSpace(r.RequiredText) || string.IsNullOrWhiteSpace(r.Jurisdiction) || string.IsNullOrWhiteSpace(r.RequirementActor) || !WorkOrderInspectionApplicability.Valid(r.ApplicableStage,r.CondType,r.PermitPrerequisiteKind) || !WorkOrderPermitPrerequisiteKinds.Valid(r.PermitPrerequisiteKind,r.RegulatoryRef,r.CondType) || r.CondType is not (ConditionType.PreStart or ConditionType.Operational or ConditionType.CloseOut or ConditionType.Regulatory or ConditionType.SceVerify) || r.SourceRevision is not {Length:64} || !r.SourceRevision.All(char.IsAsciiHexDigit) || r.RequirementRevision is not null && (r.RequirementRevision.Length!=64 || !r.RequirementRevision.All(char.IsAsciiHexDigit)) ||
           r.Status is not ("UNRECORDED" or "PASS" or "FAIL" or "NA") || r.Status!="UNRECORDED" && (string.IsNullOrWhiteSpace(r.Inspector) || string.IsNullOrWhiteSpace(r.ResultText) || r.InspectDate is null || r.InspectDate==default(DateTime)) || r.Status=="UNRECORDED" && (!string.IsNullOrWhiteSpace(r.Inspector) || !string.IsNullOrWhiteSpace(r.ResultText) || r.InspectDate is not null)) ||
           rows.Select(r=>r.InspectionId).Distinct(StringComparer.Ordinal).Count()!=rows.Count || rows.Select(r=>r.CondSeq).Distinct().Count()!=rows.Count)
            throw new InvalidOperationException("Inspection evidence cannot be confirmed for the original FIELD and order.");
    }
    public static WORK_ORDER_INSPECTION PrepareRequirement(AddWorkOrderInspectionRequirementRequest request,string order,string actor,string identity,DateTime utc)
    {
        if(request is null || string.IsNullOrWhiteSpace(order) || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(identity) || utc==default || utc.Kind!=DateTimeKind.Utc ||
           string.IsNullOrWhiteSpace(request.FieldId) || request.CondSeq<=0 || string.IsNullOrWhiteSpace(request.RequiredText) || string.IsNullOrWhiteSpace(request.Jurisdiction) || !WorkOrderInspectionApplicability.Valid(request.ApplicableStage,request.CondType,request.PermitPrerequisiteKind) || !WorkOrderPermitPrerequisiteKinds.Valid(request.PermitPrerequisiteKind,request.RegulatoryRef,request.CondType) ||
           request.CondType is not (ConditionType.PreStart or ConditionType.Operational or ConditionType.CloseOut or ConditionType.Regulatory or ConditionType.SceVerify))
            throw new InvalidOperationException("Confirm original FIELD/order, requirement sequence/type/text, reported jurisdiction and authenticated requirement owner.");
        return new(){INSPECTION_ID=identity,WORK_ORDER_ID=order,FIELD_ID=request.FieldId,COND_SEQ=request.CondSeq,COND_TYPE=request.CondType,REQUIRED_TEXT=request.RequiredText,JURISDICTION=request.Jurisdiction,
            REGULATORY_REF=request.RegulatoryRef,APPLICABLE_STAGE=request.ApplicableStage,PERMIT_PREREQUISITE_KIND=request.PermitPrerequisiteKind,REQUIREMENT_ACTOR=actor,ROW_CREATED_BY=actor,ROW_CREATED_DATE=utc,PPDM_GUID=Guid.NewGuid().ToString(),ACTIVE_IND="Y",SOURCE="OILGAS_WORK_ORDER_INSPECTION"};
    }
    public static void Require(string field,string order,IReadOnlyList<WORK_ORDER_INSPECTION>? rows)
    {
        if(string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(order) || rows is null || rows.Any(r=>r is null || r.FIELD_ID!=field || r.WORK_ORDER_ID!=order || r.ACTIVE_IND!="Y" ||
           string.IsNullOrWhiteSpace(r.INSPECTION_ID) || r.COND_SEQ<=0 || string.IsNullOrWhiteSpace(r.REQUIRED_TEXT) || string.IsNullOrWhiteSpace(r.JURISDICTION) || string.IsNullOrWhiteSpace(r.REQUIREMENT_ACTOR) || !WorkOrderInspectionApplicability.Valid(r.APPLICABLE_STAGE,r.COND_TYPE,r.PERMIT_PREREQUISITE_KIND) || !WorkOrderPermitPrerequisiteKinds.Valid(r.PERMIT_PREREQUISITE_KIND,r.REGULATORY_REF,r.COND_TYPE) ||
           r.COND_TYPE is not (ConditionType.PreStart or ConditionType.Operational or ConditionType.CloseOut or ConditionType.Regulatory or ConditionType.SceVerify) ||
           r.RESULT_STATUS is not ("" or "PASS" or "FAIL" or "NA") || r.RESULT_STATUS!="" && (string.IsNullOrWhiteSpace(r.INSPECTOR) || string.IsNullOrWhiteSpace(r.RESULT_NOTES) || r.INSPECTED_AT_UTC is null || r.INSPECTED_AT_UTC==default(DateTime)) ||
           r.RESULT_STATUS=="" && (!string.IsNullOrWhiteSpace(r.INSPECTOR) || !string.IsNullOrWhiteSpace(r.RESULT_NOTES) || r.INSPECTED_AT_UTC is not null)) ||
           rows.Select(r=>r.INSPECTION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count || rows.Select(r=>r.COND_SEQ).Distinct().Count()!=rows.Count)
            throw new InvalidOperationException("Recorded inspection requirements/results do not match their original FIELD/order and evidence identities.");
    }
    /// <summary>Immutable native requirement and creation audit, independent of subsequent result evidence.</summary>
    public static string RequirementRevision(WORK_ORDER_INSPECTION row)
    {
        Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);
        if(string.IsNullOrWhiteSpace(row.ROW_CREATED_BY) || row.ROW_CREATED_DATE is not {} created || created==default || created.Kind!=DateTimeKind.Utc || string.IsNullOrWhiteSpace(row.PPDM_GUID) || string.IsNullOrWhiteSpace(row.SOURCE))throw new InvalidOperationException("Complete original native requirement creation audit is unavailable.");
        if(new[]{row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.ROW_EFFECTIVE_DATE,row.ROW_EXPIRY_DATE}.Any(value=>value is {} date && date.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Native requirement effective audit time zone is unconfirmed.");
        var legacy=JsonSerializer.Serialize(new{row.INSPECTION_ID,row.FIELD_ID,row.WORK_ORDER_ID,row.COND_SEQ,row.COND_TYPE,row.REQUIRED_TEXT,row.JURISDICTION,row.REGULATORY_REF,row.PERMIT_PREREQUISITE_KIND,row.REQUIREMENT_ACTOR,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.SOURCE,row.PPDM_GUID,row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.ROW_EFFECTIVE_DATE,row.ROW_EXPIRY_DATE,row.ROW_QUALITY,row.REMARK});var body=row.APPLICABLE_STAGE is null?legacy:JsonSerializer.Serialize(new{Original=legacy,row.APPLICABLE_STAGE});return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
    }
    public static string Revision(WORK_ORDER_INSPECTION row)
    {
        Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);
        var original=JsonSerializer.Serialize(new{row.INSPECTION_ID,row.FIELD_ID,row.WORK_ORDER_ID,row.COND_SEQ,row.COND_TYPE,row.REQUIRED_TEXT,row.JURISDICTION,row.REGULATORY_REF,row.REQUIREMENT_ACTOR,row.RESULT_STATUS,row.RESULT_NOTES,row.INSPECTOR,row.INSPECTED_AT_UTC,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE,row.SOURCE,row.PPDM_GUID});
        var body=row.PERMIT_PREREQUISITE_KIND is null?original:JsonSerializer.Serialize(new{Original=original,row.PERMIT_PREREQUISITE_KIND});if(row.APPLICABLE_STAGE is not null)body=JsonSerializer.Serialize(new{Original=body,row.APPLICABLE_STAGE});if(row.MATERIAL_ACCEPTANCE_DATA_JSON is not null || row.MATERIAL_ACCEPTANCE_REVISION is not null)body=JsonSerializer.Serialize(new{Original=body,row.MATERIAL_ACCEPTANCE_DATA_JSON,row.MATERIAL_ACCEPTANCE_REVISION});if(row.PERSONNEL_ACCEPTANCE_DATA_JSON is not null || row.PERSONNEL_ACCEPTANCE_REVISION is not null)body=JsonSerializer.Serialize(new{Original=body,row.PERSONNEL_ACCEPTANCE_DATA_JSON,row.PERSONNEL_ACCEPTANCE_REVISION});if(row.DOCUMENT_ACCEPTANCE_DATA_JSON is not null || row.DOCUMENT_ACCEPTANCE_REVISION is not null)body=JsonSerializer.Serialize(new{Original=body,row.DOCUMENT_ACCEPTANCE_DATA_JSON,row.DOCUMENT_ACCEPTANCE_REVISION});return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
    }
    /// <summary>Explicit requirement-proof projection for versioned callers; legacy Project stays unchanged.</summary>
    public static InspectionCondition ProjectForRequirement(WORK_ORDER_INSPECTION row,InspectionCondition original)
    {
        var projected=Project(row);if(original.RequirementRevision is null)return projected;var immutable=RequirementRevision(row);if(immutable!=original.RequirementRevision)throw new InvalidOperationException("Original immutable native requirement/creation audit changed.");return projected with{RequirementRevision=immutable};
    }
    public static InspectionCondition ProjectWithRequirementProof(WORK_ORDER_INSPECTION row)=>Project(row) with{RequirementRevision=RequirementRevision(row)};
    public static InspectionCondition Project(WORK_ORDER_INSPECTION row)
    {
        Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);
        return new(row.COND_SEQ,row.COND_TYPE,row.REQUIRED_TEXT,row.RESULT_STATUS==""?"UNRECORDED":row.RESULT_STATUS,row.DOCUMENT_ACCEPTANCE_DATA_JSON is not null?"Independent document acceptance recorded":row.PERSONNEL_ACCEPTANCE_DATA_JSON is null?row.RESULT_NOTES:"Independent personnel acceptance recorded",row.INSPECTED_AT_UTC,row.REGULATORY_REF)
        {InspectionId=row.INSPECTION_ID,FieldId=row.FIELD_ID,WorkOrderId=row.WORK_ORDER_ID,Jurisdiction=row.JURISDICTION,RequirementActor=row.REQUIREMENT_ACTOR,Inspector=row.INSPECTOR,ApplicableStage=row.APPLICABLE_STAGE,PermitPrerequisiteKind=row.PERMIT_PREREQUISITE_KIND,SourceRevision=Revision(row),MaterialAcceptance=row.MATERIAL_ACCEPTANCE_DATA_JSON is null?null:TheTechIdea.Data.OilGas.CompanyWorkOrderMaterialAcceptanceCommand.Read(row).Facts(row),PersonnelAcceptance=row.PERSONNEL_ACCEPTANCE_DATA_JSON is null?null:TheTechIdea.Data.OilGas.CompanyWorkOrderPersonnelAcceptanceCommand.Read(row).Facts(row),DocumentAcceptance=row.DOCUMENT_ACCEPTANCE_DATA_JSON is null?null:TheTechIdea.Data.OilGas.CompanyWorkOrderDocumentAcceptanceCommand.Read(row).Facts(row)};
    }
    public static WORK_ORDER_INSPECTION PrepareResult(WORK_ORDER_INSPECTION row,RecordInspectionResultRequest request,string actor,DateTime utc)
    {
        Require(row.FIELD_ID,row.WORK_ORDER_ID,[row]);
        if(request is null || request.FieldId!=row.FIELD_ID || request.InspectionId!=row.INSPECTION_ID || request.ExpectedSourceRevision!=Revision(row) ||
           request.Result is not ("PASS" or "FAIL" or "NA") || string.IsNullOrWhiteSpace(request.Notes) || string.IsNullOrWhiteSpace(actor) || utc==default || utc.Kind!=DateTimeKind.Utc)
            throw new InvalidOperationException("Confirm original inspection revision, explicit result, rationale and authenticated inspector.");
        if(row.RESULT_STATUS!="")throw new InvalidOperationException("A result is already recorded. A correction requires its reviewed audit workflow; it cannot be overwritten.");
        return new(){INSPECTION_ID=row.INSPECTION_ID,WORK_ORDER_ID=row.WORK_ORDER_ID,FIELD_ID=row.FIELD_ID,COND_SEQ=row.COND_SEQ,COND_TYPE=row.COND_TYPE,REQUIRED_TEXT=row.REQUIRED_TEXT,
            JURISDICTION=row.JURISDICTION,REGULATORY_REF=row.REGULATORY_REF,APPLICABLE_STAGE=row.APPLICABLE_STAGE,PERMIT_PREREQUISITE_KIND=row.PERMIT_PREREQUISITE_KIND,REQUIREMENT_ACTOR=row.REQUIREMENT_ACTOR,RESULT_STATUS=request.Result,RESULT_NOTES=request.Notes,INSPECTOR=actor,INSPECTED_AT_UTC=utc,
            ACTIVE_IND=row.ACTIVE_IND,PPDM_GUID=row.PPDM_GUID,SOURCE=row.SOURCE,ROW_CREATED_BY=row.ROW_CREATED_BY,ROW_CREATED_DATE=row.ROW_CREATED_DATE,ROW_CHANGED_BY=actor,ROW_CHANGED_DATE=utc,
            EFFECTIVE_DATE=row.EFFECTIVE_DATE,EXPIRY_DATE=row.EXPIRY_DATE,ROW_EFFECTIVE_DATE=row.ROW_EFFECTIVE_DATE,ROW_EXPIRY_DATE=row.ROW_EXPIRY_DATE,ROW_QUALITY=row.ROW_QUALITY,REMARK=row.REMARK};
    }

    public static void RequireReadback(WORK_ORDER_INSPECTION intended,WORK_ORDER_INSPECTION? saved)
    {
        if(saved is null)throw new InvalidOperationException("Saved inspection evidence is unavailable; review the original outcome before retrying.");
        Require(intended.FIELD_ID,intended.WORK_ORDER_ID,[saved]);
        if(saved.INSPECTION_ID!=intended.INSPECTION_ID || saved.COND_SEQ!=intended.COND_SEQ || saved.COND_TYPE!=intended.COND_TYPE || saved.REQUIRED_TEXT!=intended.REQUIRED_TEXT ||
           saved.APPLICABLE_STAGE!=intended.APPLICABLE_STAGE || saved.JURISDICTION!=intended.JURISDICTION || saved.REGULATORY_REF!=intended.REGULATORY_REF || saved.PERMIT_PREREQUISITE_KIND!=intended.PERMIT_PREREQUISITE_KIND || saved.REQUIREMENT_ACTOR!=intended.REQUIREMENT_ACTOR || saved.PPDM_GUID!=intended.PPDM_GUID || saved.SOURCE!=intended.SOURCE ||
           saved.RESULT_STATUS!=intended.RESULT_STATUS || saved.RESULT_NOTES!=intended.RESULT_NOTES || saved.INSPECTOR!=intended.INSPECTOR || saved.INSPECTED_AT_UTC!=intended.INSPECTED_AT_UTC || saved.MATERIAL_ACCEPTANCE_DATA_JSON!=intended.MATERIAL_ACCEPTANCE_DATA_JSON || saved.MATERIAL_ACCEPTANCE_REVISION!=intended.MATERIAL_ACCEPTANCE_REVISION || saved.PERSONNEL_ACCEPTANCE_DATA_JSON!=intended.PERSONNEL_ACCEPTANCE_DATA_JSON || saved.PERSONNEL_ACCEPTANCE_REVISION!=intended.PERSONNEL_ACCEPTANCE_REVISION || saved.DOCUMENT_ACCEPTANCE_DATA_JSON!=intended.DOCUMENT_ACCEPTANCE_DATA_JSON || saved.DOCUMENT_ACCEPTANCE_REVISION!=intended.DOCUMENT_ACCEPTANCE_REVISION)
            throw new InvalidOperationException("Saved inspection evidence differs from the submitted intent; review its original outcome before retrying.");
    }

}
