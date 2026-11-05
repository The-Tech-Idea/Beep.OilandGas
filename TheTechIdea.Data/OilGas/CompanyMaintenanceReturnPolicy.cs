using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyMaintenanceReturnPolicyContext(CompanyScope Company,AssetDatabaseScope Database,string ActorUserId,string AuthorityRevision)
{
    public void Require(){if(Company is null || Database is null || !LeaseSummaryValidator.Identifier(Company.TenantId) || !LeaseSummaryValidator.Identifier(Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) || !LeaseSummaryValidator.Identifier(ActorUserId) || !ProcessDefinitionRevision.IsValid(AuthorityRevision))throw new InvalidOperationException("Exact current maintenance policy company/account/native binding/authority is required.");}
}
public sealed record CompanyMaintenanceReturnTransition(string AssetType,string StatusType,IReadOnlyList<string> AcceptedSourceValues,string TargetValue,string NativeSource,string SourceReference)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? Disposition{get;init;}
    public void Require()
    {
        if(AssetType is not ("FACILITY" or "EQUIPMENT"))throw new ArgumentException("Choose the exact native facility or equipment status family.");
        CompanyMaintenanceReturnPolicyDraft.Identifiers(StatusType);CompanyMaintenanceReturnPolicyDraft.Choices(AcceptedSourceValues,false);CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);
        if(Disposition=="PRESERVE"){if(TargetValue!="" || NativeSource!="")throw new ArgumentException("An explicitly preserved facet has no replacement target or source.");}
        else{if(Disposition is not (null or "TRANSITION"))throw new ArgumentException("Choose an exact transition or explicit preservation of the current native facet.");CompanyMaintenanceReturnPolicyDraft.Identifiers(TargetValue,NativeSource);if(AcceptedSourceValues.Contains(TargetValue,StringComparer.Ordinal))throw new ArgumentException("A return transition must name a distinct target; an existing target state is not a new activation.");}
    }
}
public sealed record CompanyMaintenanceReturnStatusRule(string StatusType,IReadOnlyList<string> AcceptedValues,string SourceReference)
{
    public void Require(){CompanyMaintenanceReturnPolicyDraft.Identifiers(StatusType);CompanyMaintenanceReturnPolicyDraft.Choices(AcceptedValues,false);CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);}
}
public sealed record CompanyMaintenanceReturnTermRule(string Decision,decimal? Value,string? UnitId,string SourceReference)
{
    public void Require()
    {
        CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);
        if(Decision=="EXACT"){if(Value is null)throw new ArgumentException("Enter the explicit recorded term; zero is a value and null is unconfirmed.");CompanyMaintenanceReturnPolicyDraft.Identifiers(UnitId);}
        else if(Decision!="NOT_APPLICABLE" || Value is not null || UnitId is not null)throw new ArgumentException("Choose exact recorded value/unit or explicit sourced non-applicability without substitute values.");
    }
}
public sealed record CompanyMaintenanceReturnConditionRule(string ConditionType,string ConditionCode,string Decision,string DueCondition,string? DueFrequency,string? ProductId,CompanyMaintenanceReturnTermRule Value,CompanyMaintenanceReturnTermRule DueTerm,string RestrictionDecision,string? RestrictionId,decimal? RestrictionVersion,string? RestrictionSourceDocumentId,string? RestrictionInterpretation,string SourceReference)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? AcceptedFulfilledByBaIds{get;init;}
    public void Require()
    {
        CompanyMaintenanceReturnPolicyDraft.Identifiers(ConditionType,ConditionCode,DueCondition);CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);
        if(Decision is not ("FULFILLED_ONLY" or "FULFILLED_OR_EXEMPT" or "EXEMPT_ONLY" or "NOT_APPLICABLE") || Value is null || DueTerm is null)throw new ArgumentException("Explicit condition applicability, term decisions and source are required.");
        Value.Require();DueTerm.Require();if(DueFrequency is not null)CompanyMaintenanceReturnPolicyDraft.Text(DueFrequency);if(ProductId is not null)CompanyMaintenanceReturnPolicyDraft.Identifiers(ProductId);if(AcceptedFulfilledByBaIds is not null)CompanyMaintenanceReturnPolicyDraft.Choices(AcceptedFulfilledByBaIds,Decision is "EXEMPT_ONLY" or "NOT_APPLICABLE");
        if(RestrictionDecision=="EXACT_VERSION")
        {
            CompanyMaintenanceReturnPolicyDraft.Identifiers(RestrictionId,RestrictionSourceDocumentId);if(RestrictionVersion is null)throw new ArgumentException("Enter the exact recorded restriction version; no latest version is selected.");CompanyMaintenanceReturnPolicyDraft.Text(RestrictionInterpretation);
        }
        else if(RestrictionDecision!="NO_RECORDED_RESTRICTION" || RestrictionId is not null || RestrictionVersion is not null || RestrictionSourceDocumentId is not null || RestrictionInterpretation is not null)throw new ArgumentException("Choose an exact sourced restriction version or explicitly require no recorded restriction key.");
    }
}
public sealed record CompanyMaintenanceReturnViolationRule(string ViolationType,IReadOnlyList<string> AcceptedResolutionTypes,string SourceReference)
{
    public void Require(){CompanyMaintenanceReturnPolicyDraft.Identifiers(ViolationType);CompanyMaintenanceReturnPolicyDraft.Choices(AcceptedResolutionTypes,false);CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);}
}
public sealed record CompanyMaintenanceReturnLicenseRule(string IssuerBaId,string LicenseType,string Applicability,string FeesDecision,IReadOnlyList<CompanyMaintenanceReturnStatusRule> Statuses,IReadOnlyList<CompanyMaintenanceReturnConditionRule> Conditions,IReadOnlyList<CompanyMaintenanceReturnViolationRule> Violations,string SourceReference)
{
    public void Require()
    {
        CompanyMaintenanceReturnPolicyDraft.Identifiers(IssuerBaId,LicenseType);CompanyMaintenanceReturnPolicyDraft.Text(SourceReference);
        if(Applicability is not ("REQUIRED" or "NOT_APPLICABLE") || FeesDecision is not ("REQUIRED_PAID" or "NOT_APPLICABLE") || Statuses is null || Conditions is null || Violations is null || Statuses.Count>64 || Conditions.Count>64 || Violations.Count>64 || Statuses.Any(row=>row is null) || Conditions.Any(row=>row is null) || Violations.Any(row=>row is null) || Statuses.Select(row=>row.StatusType).Distinct(StringComparer.Ordinal).Count()!=Statuses.Count || Conditions.Select(row=>(row.ConditionType,row.ConditionCode)).Distinct().Count()!=Conditions.Count || Violations.Select(row=>row.ViolationType).Distinct(StringComparer.Ordinal).Count()!=Violations.Count)throw new ArgumentException("Explicit unique bounded licence applicability/status/condition/violation decisions are required.");
        if(Applicability=="REQUIRED" && Statuses.Count==0 || Applicability=="NOT_APPLICABLE" && (Statuses.Count+Conditions.Count+Violations.Count>0 || FeesDecision!="NOT_APPLICABLE"))throw new ArgumentException("Required licences need explicit accepted status types; non-applicable licences retain only the explicit sourced applicability decision.");
        foreach(var row in Statuses)row.Require();foreach(var row in Conditions)row.Require();foreach(var row in Violations)row.Require();
    }
}
public sealed record CompanyMaintenanceReturnPolicyDraft(string WorkOrderType,string FacilityType,string Jurisdiction,string AreaId,string AreaType,IReadOnlyList<string> AuthorizedReturnRoles,IReadOnlyList<CompanyMaintenanceReturnTransition> Transitions,IReadOnlyList<CompanyMaintenanceReturnLicenseRule> Licenses,DateTime EffectiveFromUtc,DateTime EffectiveToUtc,string SourceReference)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public string? AreaApplicability{get;init;}
    internal static void Identifiers(params string?[] values){if(values.Any(value=>!LeaseSummaryValidator.Identifier(value) || value!.Length>128))throw new ArgumentException("Exact bounded native identifiers are required; wildcards and missing codes are not policy choices.");}
    internal static void Text(params string?[] values){if(values.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>4096 || value.Any(char.IsControl)))throw new ArgumentException("Explicit bounded source or applicability text without control characters is required.");}
    internal static void Choices(IReadOnlyList<string> values,bool allowEmpty){if(values is null || values.Count>64 || !allowEmpty && values.Count==0 || values.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128) || values.Distinct(StringComparer.Ordinal).Count()!=values.Count)throw new ArgumentException("Explicit bounded unique recorded choices are required.");}
    public void Require()
    {
        Identifiers(WorkOrderType,FacilityType,AreaId,AreaType);Text(Jurisdiction,SourceReference);Choices(AuthorizedReturnRoles,false);
        if(AreaApplicability is not (null or "APPLIES" or "NOT_APPLICABLE") || EffectiveFromUtc==default || EffectiveFromUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc<=EffectiveFromUtc || Transitions is null || Transitions.Count>64 || Transitions.Any(row=>row is null) || Transitions.Select(row=>(row.AssetType,row.StatusType)).Distinct().Count()!=Transitions.Count || Licenses is null || Licenses.Count>64 || Licenses.Any(row=>row is null) || Licenses.Select(row=>(row.IssuerBaId,row.LicenseType)).Distinct().Count()!=Licenses.Count || AreaApplicability=="NOT_APPLICABLE" && Transitions.Count+Licenses.Count!=0 || AreaApplicability!="NOT_APPLICABLE" && (Transitions.Count==0 || Licenses.Count==0))throw new ArgumentException("Explicit company work-order/facility/jurisdiction/area applicability, return roles, unique native transitions/licence decisions and increasing effective UTC window are required.");
        foreach(var transition in Transitions)transition.Require();foreach(var license in Licenses)license.Require();if(JsonSerializer.Serialize(this).Length>1024*1024)throw new InvalidOperationException("Bounded maintenance return policy is too large.");
    }
    public string Key(CompanyScope company){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,WorkOrderType,FacilityType,Jurisdiction,AreaId,AreaType}));}
}

public sealed class WORK_ORDER_MAINTENANCE_RETURN_POLICY:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string POLICY_RECORD_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(64)] public string POLICY_KEY {get;set;}="";
    public int VERSION_NO {get;set;}
    public DateTime EFFECTIVE_FROM_UTC {get;set;}
    public DateTime EFFECTIVE_TO_UTC {get;set;}
    [Required,MaxLength(1024*1024)] public string POLICY_JSON {get;set;}="";
    [Required,MaxLength(2*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
public sealed record CompanyMaintenanceReturnPolicyVersion(CompanyScope Company,AssetDatabaseScope Database,string RecordId,string PolicyKey,int Version,CompanyMaintenanceReturnPolicyDraft Policy,string RecordedBy,DateTime RecordedAtUtc,string Revision)
{
    public void Require()
    {
        Policy.Require();if(Company is null || Database is null || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) ||
            PolicyKey!=Policy.Key(Company) || Version<1 || RecordId!=CompanyMaintenanceReturnPolicyCommand.RecordIdFor(Company,PolicyKey,Version) || !LeaseSummaryValidator.Identifier(RecordedBy) || RecordedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("Complete original policy version/company/native binding/audit is unavailable.");
    }
    public static CompanyMaintenanceReturnPolicyVersion From(WORK_ORDER_MAINTENANCE_RETURN_POLICY row)
    {
        var command=CompanyMaintenanceReturnPolicyCommand.Read(row);command.RequireRecorded(row);
        var result=new CompanyMaintenanceReturnPolicyVersion(command.Context.Company,command.Context.Database,row.POLICY_RECORD_ID,row.POLICY_KEY,row.VERSION_NO,command.Policy,row.ROW_CREATED_BY!,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION);result.Require();return result;
    }
}
public sealed record CompanyMaintenanceReturnPolicyCatalog(CompanyMaintenanceReturnPolicyContext Context,IReadOnlyList<CompanyMaintenanceReturnPolicyVersion> Versions,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Context.Require();if(Versions is null || Versions.Count>4096 || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Versions.Select(row=>row.RecordId).Distinct(StringComparer.Ordinal).Count()!=Versions.Count || Revision!=Digest(Context,Versions))throw new InvalidOperationException("Complete bounded current company policy catalog is unavailable.");
        foreach(var row in Versions){row.Require();if(row.Company!=Context.Company || row.Database!=Context.Database || row.RecordedAtUtc>ComparedAtUtc)throw new UnauthorizedAccessException("Policy catalog contains another company/native binding or future audit.");}
        foreach(var group in Versions.GroupBy(row=>row.PolicyKey))
        {
            var rows=group.OrderBy(row=>row.Version).ToArray();if(!rows.Select(row=>row.Version).SequenceEqual(Enumerable.Range(1,rows.Length)))throw new InvalidOperationException("Policy version history is incomplete or ambiguous.");
            for(var i=1;i<rows.Length;i++)if(rows[i].Policy.EffectiveFromUtc<rows[i-1].Policy.EffectiveToUtc)throw new InvalidOperationException("Policy effective versions overlap; no version is silently preferred.");
        }
    }
    public static string Digest(CompanyMaintenanceReturnPolicyContext context,IReadOnlyList<CompanyMaintenanceReturnPolicyVersion> versions)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Versions=versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal).ToArray()}));
    public static CompanyMaintenanceReturnPolicyCatalog Create(CompanyMaintenanceReturnPolicyContext context,IReadOnlyList<CompanyMaintenanceReturnPolicyVersion> versions,DateTime utc){var result=new CompanyMaintenanceReturnPolicyCatalog(context,versions,utc,Digest(context,versions));result.Require();return result;}
}
public sealed record CompanyMaintenanceReturnPolicyCommand(CompanyMaintenanceReturnPolicyContext Context,string ExpectedCatalogRevision,CompanyMaintenanceReturnPolicyDraft Policy,int Version,Guid IntentId,string Reason)
{
    public void Require(){Context.Require();Policy.Require();if(!ProcessDefinitionRevision.IsValid(ExpectedCatalogRevision) || Version<1 || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) || JsonSerializer.Serialize(this).Length>2*1024*1024)throw new ArgumentException("Retain the original policy catalog, explicit next version, new intent and rationale.");}
    public static string RecordIdFor(CompanyScope company,string key,int version)=>"wo-maintenance-policy-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Key=key,Version=version}));
    public string RecordId(){Require();return RecordIdFor(Context.Company,Policy.Key(Context.Company),Version);}
    public void RequireCatalog(CompanyMaintenanceReturnPolicyCatalog catalog)
    {
        Require();catalog.Require();if(catalog.Context!=Context || catalog.Revision!=ExpectedCatalogRevision)throw new InvalidOperationException("Complete original policy catalog or authority changed before append.");
        var previous=catalog.Versions.Where(row=>row.PolicyKey==Policy.Key(Context.Company)).OrderBy(row=>row.Version).ToArray();
        if(Version!=previous.Length+1 || previous.Length>0 && Policy.EffectiveFromUtc<previous[^1].Policy.EffectiveToUtc)throw new InvalidOperationException("Append the exact next non-overlapping effective policy version; prior policy/history is immutable.");
    }
    public WORK_ORDER_MAINTENANCE_RETURN_POLICY Prepare(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc || utc>=Policy.EffectiveToUtc)throw new InvalidOperationException("Explicit current policy audit UTC before effective end is required.");
        var row=new WORK_ORDER_MAINTENANCE_RETURN_POLICY{POLICY_RECORD_ID=RecordId(),OWNER_TENANT_ID=Context.Company.TenantId,OWNER_COMPANY_BA_ID=Context.Company.BusinessAssociateId,POLICY_KEY=Policy.Key(Context.Company),VERSION_NO=Version,EFFECTIVE_FROM_UTC=Policy.EffectiveFromUtc,EFFECTIVE_TO_UTC=Policy.EffectiveToUtc,POLICY_JSON=JsonSerializer.Serialize(Policy),COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=Context.ActorUserId,ROW_CHANGED_BY=Context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,PPDM_GUID=IntentId.ToString("D"),SOURCE="OILGAS_WORK_ORDER_MAINTENANCE_RETURN_POLICY"};row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_MAINTENANCE_RETURN_POLICY row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyMaintenanceReturnPolicyCommand Read(WORK_ORDER_MAINTENANCE_RETURN_POLICY row)
    {
        if(string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>2*1024*1024)throw new InvalidOperationException("Original bounded maintenance return policy proof is unavailable.");
        try{return JsonSerializer.Deserialize<CompanyMaintenanceReturnPolicyCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original policy proof is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original policy proof is malformed.");}
    }
    public void RequireRecorded(WORK_ORDER_MAINTENANCE_RETURN_POLICY row){Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original policy/version/body/company/audit changed.");}
}
public sealed record CompanyMaintenanceReturnPolicyReference(string RecordId,Guid IntentId)
{
    public void Require(){if(string.IsNullOrWhiteSpace(RecordId) || !RecordId.StartsWith("wo-maintenance-policy-",StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(RecordId["wo-maintenance-policy-".Length..]) || IntentId==Guid.Empty)throw new ArgumentException("Retain the original policy record and intent.");}
}
public sealed record CompanyMaintenanceReturnPolicyRecovery(CompanyMaintenanceReturnPolicyCommand Command,WORK_ORDER_MAINTENANCE_RETURN_POLICY Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReturnPolicyReference reference){reference.Require();Command.Require();if(Command.Context.Company!=company || Command.Context.ActorUserId!=actor || Command.RecordId()!=reference.RecordId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original private policy company/maker/reference differs.");Command.RequireRecorded(Recorded);}
}
public interface ICompanyMaintenanceReturnPolicyOwner
{
    Task<CompanyMaintenanceReturnPolicyCatalog> ReadMaintenanceReturnPoliciesAsync(CompanyMaintenanceReturnPolicyContext context,Func<Task> requireAuthority);
    Task<WORK_ORDER_MAINTENANCE_RETURN_POLICY> RecordMaintenanceReturnPolicyAsync(CompanyMaintenanceReturnPolicyCommand command,Func<Task> requireAuthority,bool readOnly=false);
    Task<CompanyMaintenanceReturnPolicyRecovery> RecoverMaintenanceReturnPolicyReferenceAsync(CompanyMaintenanceReturnPolicyContext context,CompanyMaintenanceReturnPolicyReference reference,Func<Task> requireAuthority);
}
