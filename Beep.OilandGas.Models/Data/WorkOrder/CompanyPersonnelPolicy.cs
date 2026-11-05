using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyPersonnelPolicyContext(CompanyScope Company,AssetDatabaseScope Database,string ActorUserId,string AuthorityRevision)
{
    public void Require(){if(Company is null || Database is null || !LeaseSummaryValidator.Identifier(Company.TenantId) || !LeaseSummaryValidator.Identifier(Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) || !LeaseSummaryValidator.Identifier(ActorUserId) || !ProcessDefinitionRevision.IsValid(AuthorityRevision))throw new InvalidOperationException("Exact current policy company/account/native binding/authority is required.");}
}
public sealed record CompanyPersonnelConditionRequirement(string Code,string Type,string Comparison,decimal Value,string Unit,string SourceReference)
{
    public void Require()
    {
        if(new[]{Code,Type,Unit,SourceReference}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>256 || value.Any(char.IsControl)) || Comparison is not ("MINIMUM" or "EXACT") || Value<=0)throw new ArgumentException("Explicit recorded condition code/type, supported comparison, positive required value/unit and source reference are required; no missing numeric value is accepted.");
    }
}
public sealed record CompanyPersonnelPolicyDraft(string DutyCode,string WorkOrderType,string Jurisdiction,string LicenseType,string GrantingAuthority,
    string StatusType,IReadOnlyList<string> AcceptedStatuses,IReadOnlyList<CompanyPersonnelConditionRequirement> Conditions,DateTime EffectiveFromUtc,DateTime EffectiveToUtc,string SourceReference)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? AcceptedEmploymentStatuses {get;init;}
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? AcceptedEmploymentPositions {get;init;}
    public void RequireCurrentEligibility()
    {
        Require();if(AcceptedEmploymentStatuses is null || AcceptedEmploymentPositions is null)throw new InvalidOperationException("Configure explicit accepted employment statuses and position restrictions before using a current personnel policy.");
    }
    public void Require()
    {
        if(new[]{DutyCode,WorkOrderType,Jurisdiction,LicenseType,GrantingAuthority,StatusType,SourceReference}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>256 || value.Any(char.IsControl)) ||
            DutyCode.Length>128 || WorkOrderType.Length>128 || EffectiveFromUtc==default || EffectiveFromUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc<=EffectiveFromUtc ||
            AcceptedStatuses is null || AcceptedStatuses.Count is <1 or >64 || AcceptedStatuses.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) || AcceptedStatuses.Distinct(StringComparer.Ordinal).Count()!=AcceptedStatuses.Count ||
            Conditions is null || Conditions.Count is <1 or >64 || Conditions.Any(value=>value is null) || Conditions.Select(value=>(value.Code,value.Type)).Distinct().Count()!=Conditions.Count)
            throw new ArgumentException("Explicit company duty/work-order/jurisdiction, licence/authority/status choices, competency conditions, source and increasing effective UTC window are required.");
        foreach(var condition in Conditions)condition.Require();
        ValidateEmploymentChoices(AcceptedEmploymentStatuses,false);ValidateEmploymentChoices(AcceptedEmploymentPositions,true);
        if(JsonSerializer.Serialize(this).Length>1024*1024)throw new InvalidOperationException("Bounded personnel policy is too large.");
    }
    private static void ValidateEmploymentChoices(IReadOnlyList<string>? values,bool allowEmpty)
    {
        if(values is null)return; // Absent fields preserve historical policy JSON; current use requires explicit configuration.
        if(values.Count>64 || !allowEmpty && values.Count==0 || values.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl)) || values.Distinct(StringComparer.Ordinal).Count()!=values.Count)throw new ArgumentException("Employment policy choices must be explicit, bounded and unique; an empty position list explicitly permits any recorded position.");
    }
    public string Key(CompanyScope company){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,DutyCode,WorkOrderType,Jurisdiction}));}
}
public sealed class WORK_ORDER_PERSONNEL_POLICY:ModelEntityBase
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
public sealed record CompanyPersonnelPolicyVersion(CompanyScope Company,AssetDatabaseScope Database,string RecordId,string PolicyKey,int Version,CompanyPersonnelPolicyDraft Policy,string RecordedBy,DateTime RecordedAtUtc,string Revision)
{
    public void Require()
    {
        Policy.Require();if(Company is null || Database is null || !LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint) ||
            PolicyKey!=Policy.Key(Company) || Version<1 || RecordId!=CompanyPersonnelPolicyCommand.RecordIdFor(Company,PolicyKey,Version) || !LeaseSummaryValidator.Identifier(RecordedBy) || RecordedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("Complete original policy version/company/native binding/audit is unavailable.");
    }
    public static CompanyPersonnelPolicyVersion From(WORK_ORDER_PERSONNEL_POLICY row)
    {
        var command=CompanyPersonnelPolicyCommand.Read(row);command.RequireRecorded(row);
        var result=new CompanyPersonnelPolicyVersion(command.Context.Company,command.Context.Database,row.POLICY_RECORD_ID,row.POLICY_KEY,row.VERSION_NO,command.Policy,row.ROW_CREATED_BY!,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION);result.Require();return result;
    }
}
public sealed record CompanyPersonnelPolicyCatalog(CompanyPersonnelPolicyContext Context,IReadOnlyList<CompanyPersonnelPolicyVersion> Versions,DateTime ComparedAtUtc,string Revision)
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
    public static string Digest(CompanyPersonnelPolicyContext context,IReadOnlyList<CompanyPersonnelPolicyVersion> versions)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Versions=versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal).ToArray()}));
    public static CompanyPersonnelPolicyCatalog Create(CompanyPersonnelPolicyContext context,IReadOnlyList<CompanyPersonnelPolicyVersion> versions,DateTime utc){var result=new CompanyPersonnelPolicyCatalog(context,versions,utc,Digest(context,versions));result.Require();return result;}
}
public sealed record CompanyPersonnelPolicyCommand(CompanyPersonnelPolicyContext Context,string ExpectedCatalogRevision,CompanyPersonnelPolicyDraft Policy,int Version,Guid IntentId,string Reason)
{
    public void Require(){Context.Require();Policy.Require();if(!ProcessDefinitionRevision.IsValid(ExpectedCatalogRevision) || Version<1 || IntentId==Guid.Empty || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) || JsonSerializer.Serialize(this).Length>2*1024*1024)throw new ArgumentException("Retain the original policy catalog, explicit next version, new intent and rationale.");}
    public static string RecordIdFor(CompanyScope company,string key,int version)=>"wo-personnel-policy-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Key=key,Version=version}));
    public string RecordId(){Require();return RecordIdFor(Context.Company,Policy.Key(Context.Company),Version);}
    public void RequireCatalog(CompanyPersonnelPolicyCatalog catalog)
    {
        Require();catalog.Require();if(catalog.Context!=Context || catalog.Revision!=ExpectedCatalogRevision)throw new InvalidOperationException("Complete original policy catalog or authority changed before append.");
        var previous=catalog.Versions.Where(row=>row.PolicyKey==Policy.Key(Context.Company)).OrderBy(row=>row.Version).ToArray();
        if(Version!=previous.Length+1 || previous.Length>0 && Policy.EffectiveFromUtc<previous[^1].Policy.EffectiveToUtc)throw new InvalidOperationException("Append the exact next non-overlapping effective policy version; prior policy/history is immutable.");
    }
    public WORK_ORDER_PERSONNEL_POLICY Prepare(DateTime utc)
    {
        Require();if(utc==default || utc.Kind!=DateTimeKind.Utc || utc>=Policy.EffectiveToUtc)throw new InvalidOperationException("Explicit current policy audit UTC before effective end is required.");
        var row=new WORK_ORDER_PERSONNEL_POLICY{POLICY_RECORD_ID=RecordId(),OWNER_TENANT_ID=Context.Company.TenantId,OWNER_COMPANY_BA_ID=Context.Company.BusinessAssociateId,POLICY_KEY=Policy.Key(Context.Company),VERSION_NO=Version,EFFECTIVE_FROM_UTC=Policy.EffectiveFromUtc,EFFECTIVE_TO_UTC=Policy.EffectiveToUtc,POLICY_JSON=JsonSerializer.Serialize(Policy),COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=Context.ActorUserId,ROW_CHANGED_BY=Context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,PPDM_GUID=IntentId.ToString("D"),SOURCE="OILGAS_WORK_ORDER_PERSONNEL_POLICY"};row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(WORK_ORDER_PERSONNEL_POLICY row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static CompanyPersonnelPolicyCommand Read(WORK_ORDER_PERSONNEL_POLICY row)
    {
        if(string.IsNullOrWhiteSpace(row.COMMAND_JSON) || row.COMMAND_JSON.Length>2*1024*1024)throw new InvalidOperationException("Original bounded personnel policy proof is unavailable.");
        try{return JsonSerializer.Deserialize<CompanyPersonnelPolicyCommand>(row.COMMAND_JSON)??throw new InvalidOperationException("Original policy proof is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original policy proof is malformed.");}
    }
    public void RequireRecorded(WORK_ORDER_PERSONNEL_POLICY row){Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original policy/version/body/company/audit changed.");}
}
public sealed record CompanyPersonnelPolicyReference(string RecordId,Guid IntentId)
{
    public void Require(){if(string.IsNullOrWhiteSpace(RecordId) || !RecordId.StartsWith("wo-personnel-policy-",StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(RecordId["wo-personnel-policy-".Length..]) || IntentId==Guid.Empty)throw new ArgumentException("Retain the original policy record and intent.");}
}
public sealed record CompanyPersonnelPolicyRecovery(CompanyPersonnelPolicyCommand Command,WORK_ORDER_PERSONNEL_POLICY Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyPersonnelPolicyReference reference){reference.Require();Command.Require();if(Command.Context.Company!=company || Command.Context.ActorUserId!=actor || Command.RecordId()!=reference.RecordId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original private policy company/maker/reference differs.");Command.RequireRecorded(Recorded);}
}
public interface ICompanyPersonnelPolicyOwner
{
    Task<CompanyPersonnelPolicyCatalog> ReadPersonnelPoliciesAsync(CompanyPersonnelPolicyContext context,Func<Task> requireAuthority);
    Task<WORK_ORDER_PERSONNEL_POLICY> RecordPersonnelPolicyAsync(CompanyPersonnelPolicyCommand command,Func<Task> requireAuthority,bool readOnly=false);
    Task<CompanyPersonnelPolicyRecovery> RecoverPersonnelPolicyReferenceAsync(CompanyPersonnelPolicyContext context,CompanyPersonnelPolicyReference reference,Func<Task> requireAuthority);
}
