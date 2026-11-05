using System.Text.Json;
using System.Text.Json.Serialization;
namespace TheTechIdea.Data.OilGas;
/// <summary>Approved native program facts for execution, without proposed/frozen unselected recipient populations.</summary>
public sealed record DrillingExecutionBasis([property:JsonRequired]DrillingProgramReviewSource Source,[property:JsonRequired]string ReviewInstanceId,
    [property:JsonRequired]string ReviewOriginalRevision,[property:JsonRequired]string ReviewNativeRevision,[property:JsonRequired]string SubmittedBy,
    [property:JsonRequired]IReadOnlyList<DrillingProgramStageProgress> Approvals,[property:JsonRequired]IReadOnlyList<string> RecorderRoles,[property:JsonRequired]string? PersonaCode,
    [property:JsonRequired]string? RecorderAuthorityRevision,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore]public bool CanRecord=>RecorderRoles.Count>0;
    public DevelopmentAssignedReviewScope Scope()=>Source.Basis.Scope();
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Source=Source.Revision,ReviewInstanceId,ReviewOriginalRevision,ReviewNativeRevision,SubmittedBy,Approvals,RecorderRoles,PersonaCode,RecorderAuthorityRevision}));
    public void Require()
    {
        Source.Require();if(!LeaseSummaryValidator.Identifier(ReviewInstanceId) || !ReviewInstanceId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(ReviewOriginalRevision) || !ProcessDefinitionRevision.IsValid(ReviewNativeRevision) || !LeaseSummaryValidator.Identifier(SubmittedBy) ||
            Approvals is null || Approvals.Count is <1 or >64 || Approvals.Any(row=>row is null) || RecorderRoles is null || RecorderRoles.Count>256 || RecorderRoles.Any(role=>!LeaseSummaryValidator.Identifier(role)) || RecorderRoles.Distinct(StringComparer.Ordinal).Count()!=RecorderRoles.Count ||
            (CanRecord?!LeaseSummaryValidator.Identifier(PersonaCode) || PersonaCode!.Length>64 || !ProcessDefinitionRevision.IsValid(RecorderAuthorityRevision):PersonaCode is not null || RecorderAuthorityRevision is not null) || Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The exact approved original program and current execution recorder facts are required.");
        DateTimeOffset? previous=null;for(int index=0;index<Approvals.Count;index++)
        {
            var row=Approvals[index];if(row.Number!=index+1 || row.State!="APPROVED" || row.Decision!="APPROVE" || !LeaseSummaryValidator.Identifier(row.StepId) || string.IsNullOrWhiteSpace(row.Name) || !LeaseSummaryValidator.Identifier(row.AssignedRole) || row.PersonaCodes is null || row.PersonaCodes.Count==0 || !row.PersonaCodes.Contains(row.ReviewerPersona!,StringComparer.Ordinal) ||
                !LeaseSummaryValidator.Identifier(row.Reviewer) || row.Reviewer==SubmittedBy || row.Reviewer==Source.Program.ROW_CREATED_BY || string.IsNullOrWhiteSpace(row.Rationale) || row.Rationale.Length>1600 || row.AtUtc is not {} at || at.Offset!=TimeSpan.Zero || at>CheckedAtUtc || previous is not null && at<previous || row.IntentId is null || row.IntentId==Guid.Empty)throw new InvalidOperationException("Execution requires every recorded independent program stage approval, never a pending or rejected review.");
            var digest=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Instance=ReviewInstanceId,IntentId=row.IntentId.Value}));if(row.ApprovalId!="program-stage-approval-"+digest || row.HistoryId!="program-stage-decision-"+digest)throw new InvalidOperationException("The native program approval audit changed.");previous=at;
        }
        if(Approvals.Select(row=>row.StepId).Distinct(StringComparer.Ordinal).Count()!=Approvals.Count || Approvals.Select(row=>row.IntentId).Distinct().Count()!=Approvals.Count)throw new InvalidOperationException("The complete program approval sequence is ambiguous.");
    }
    public void RequireSource(DrillingExecutionBasis current,bool exact,bool originalActor=true)
    {
        Require();current.Require();if((originalActor?current.Scope():current.Scope() with{ActorUserId=Scope().ActorUserId})!=Scope() || current.Source.Basis.ExecutionDatabase!=Source.Basis.ExecutionDatabase || current.Source.Program.PROGRAM_ID!=Source.Program.PROGRAM_ID || current.Source.Program.PROGRAM_DRAFT_REVISION!=Source.Program.PROGRAM_DRAFT_REVISION || current.ReviewInstanceId!=ReviewInstanceId || current.ReviewOriginalRevision!=ReviewOriginalRevision || current.ReviewNativeRevision!=ReviewNativeRevision || JsonSerializer.Serialize(current.Approvals)!=JsonSerializer.Serialize(Approvals) || current.Source.Basis.Funding.Source.Afe()!=Source.Basis.Funding.Source.Afe() || exact && current.Revision!=Revision)throw new InvalidOperationException("The original approved program/version, native decisions, funding, well/FIELD or execution bindings changed.");
    }
}
public sealed record DrillingExecutionStart([property:JsonRequired]DrillingExecutionBasis Basis,[property:JsonRequired]Guid IntentId,[property:JsonRequired]string RecorderRole,
    [property:JsonRequired]DateTimeOffset SpudAtUtc,[property:JsonRequired]string Rationale)
{
    public const int MaximumBodyLength=8*1024*1024;
    public void Require()
    {
        Basis.Require();if(!Basis.CanRecord || !Basis.RecorderRoles.Contains(RecorderRole,StringComparer.Ordinal) || IntentId==Guid.Empty || SpudAtUtc==default || SpudAtUtc.Offset!=TimeSpan.Zero || SpudAtUtc<Basis.Approvals[^1].AtUtc || string.IsNullOrWhiteSpace(Rationale) || Rationale!=Rationale.Trim() || Rationale.Length>1600 || Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) || JsonSerializer.SerializeToUtf8Bytes(this).Length>MaximumBodyLength)throw new InvalidOperationException("Choose your current execution role and explicitly report actual spud UTC at or after final approval with a bounded rationale.");
    }
    public string OperationId(){Require();return "drilling-execution-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Basis.Scope().Company,IntentId}));}
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public DRILLING_OPERATION ToRecord(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc==default || utc<Basis.CheckedAtUtc.UtcDateTime || utc<SpudAtUtc.UtcDateTime)throw new InvalidOperationException("Actual producer UTC must confirm an observed, non-future spud report.");var scope=Basis.Scope();var row=new DRILLING_OPERATION{DRILLING_OPERATION_ID=OperationId(),OWNER_TENANT_ID=scope.Company.TenantId,OWNER_COMPANY_BA_ID=scope.Company.BusinessAssociateId,FIELD_ID=scope.FieldId,WELL_UWI=Basis.Source.Basis.Well.Uwi,PROGRAM_ID=Basis.Source.Program.PROGRAM_ID,PROGRAM_DRAFT_REVISION=Basis.Source.Program.PROGRAM_DRAFT_REVISION,PROGRAM_REVIEW_INSTANCE_ID=Basis.ReviewInstanceId,PROGRAM_REVIEW_NATIVE_REVISION=Basis.ReviewNativeRevision,RECORDER_ROLE=RecorderRole,RECORDER_PERSONA_CODE=Basis.PersonaCode!,SPUD_DATE=SpudAtUtc.UtcDateTime,STATUS="STARTED",EXECUTION_START_JSON=JsonSerializer.Serialize(this),PPDM_GUID=IntentId.ToString("D"),SOURCE="BEEP_OILGAS",ACTIVE_IND="Y",ROW_CREATED_BY=scope.ActorUserId,ROW_CHANGED_BY=scope.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};row.EXECUTION_START_REVISION=Digest(row);return row;
    }
    public static string Digest(DRILLING_OPERATION row){var copy=CompanyInventorySourceReview.Copy(row);copy.EXECUTION_START_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static DrillingExecutionStart FromRecord(DRILLING_OPERATION row)
    {
        if(row?.EXECUTION_START_JSON is not {Length:>0 and <=MaximumBodyLength} json)throw new InvalidOperationException("The bounded original execution start proof is unavailable.");var original=JsonSerializer.Deserialize<DrillingExecutionStart>(json)??throw new InvalidOperationException("The original execution start is invalid.");original.Require();var actual=CompanyInventorySourceReview.Copy(row);
        if(actual.ROW_CREATED_DATE is not {} created || actual.ROW_CHANGED_DATE is not {} changed || actual.SPUD_DATE is not {} spud || created.Kind==DateTimeKind.Local || changed.Kind==DateTimeKind.Local || spud.Kind==DateTimeKind.Local || changed<created)throw new InvalidOperationException("The exact execution actual UTC/audit proof is unavailable.");actual.ROW_CREATED_DATE=DateTime.SpecifyKind(created,DateTimeKind.Utc);actual.ROW_CHANGED_DATE=DateTime.SpecifyKind(changed,DateTimeKind.Utc);actual.SPUD_DATE=DateTime.SpecifyKind(spud,DateTimeKind.Utc);var expected=original.ToRecord(actual.ROW_CREATED_DATE.Value);expected.ROW_CHANGED_DATE=actual.ROW_CHANGED_DATE;expected.EXECUTION_START_REVISION=Digest(expected);actual.EXECUTION_START_REVISION=Digest(actual);
        if(row.EXECUTION_START_REVISION!=expected.EXECUTION_START_REVISION || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The native execution differs from its exact original company/program/approved-source/actual/audit record.");return original;
    }
}
public sealed record DrillingExecutionStartReceipt([property:JsonRequired]string IntentRevision,[property:JsonRequired]string OperationId,[property:JsonRequired]DRILLING_OPERATION? Recorded,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingExecutionStart original)
    {original.Require();if(IntentRevision!=original.Revision() || OperationId!=original.OperationId() || Recorded is not null && DrillingExecutionStart.FromRecord(Recorded).Revision()!=original.Revision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The exact original execution start outcome is unconfirmed.");}
}

public sealed record DrillingExecutionStatus([property:JsonRequired]DrillingExecutionBasis Basis,[property:JsonRequired]DRILLING_OPERATION? Recorded,[property:JsonRequired]string Revision,[property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Basis=Basis.Revision,Recorded}));
    public void Require()
    {Basis.Require();if(Recorded is not null)DrillingExecutionStart.FromRecord(Recorded).Basis.RequireSource(Basis,false,false);if(Revision!=ContentRevision() || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero)throw new InvalidOperationException("The exact original execution outcome or confirmed absence is unavailable.");}
}
