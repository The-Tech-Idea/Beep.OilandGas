using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyPersonnelIdentityQuery(string UserId,string PersonBaId)
{public void Require(){if(!LeaseSummaryValidator.Identifier(UserId) || !LeaseSummaryValidator.Identifier(PersonBaId))throw new ArgumentException("Select the exact current company application account and recorded native person.");}}
public sealed record CompanyApplicationPersonFacts(CompanyScope Company,string UserId,string? UserName,bool Active,string Revision)
{public void Require(CompanyScope company,string user){if(Company!=company || UserId!=user || !Active || !ProcessDefinitionRevision.IsValid(Revision))throw new UnauthorizedAccessException("Current active application account/company membership proof is required.");}}
public sealed record CompanyNativePersonFacts(CompanyScope Company,string PersonBaId,string? Name,string? Type,DateTime? EffectiveFromUtc,DateTime? EffectiveToUtc,CompanyEmployeeSourceFacts Employment,string Revision)
{
    public void Require(CompanyScope company,string person)
    {if(Company!=company || PersonBaId!=person || person==company.BusinessAssociateId || !LeaseSummaryValidator.Identifier(person) || Employment is null || Employment.EmployerBaId!=company.BusinessAssociateId || Employment.EmployeeBaId!=person || !ProcessDefinitionRevision.IsValid(Employment.Revision) || !ProcessDefinitionRevision.IsValid(Revision))throw new UnauthorizedAccessException("Exact current company employee/native person relationship is unavailable.");}
}
public sealed record CompanyPersonnelIdentityVersion(CompanyScope Company,AssetDatabaseScope Database,string RecordId,string UserId,string PersonBaId,string PersonType,int Version,DateTime EffectiveFromUtc,DateTime EffectiveToUtc,string SourceReference,string RecordedBy,DateTime RecordedAtUtc,string Revision)
{
    public void Require()
    {
        if(Company is null || Database is null || !LeaseSummaryValidator.Identifier(UserId) || !LeaseSummaryValidator.Identifier(PersonBaId) || !LeaseSummaryValidator.Identifier(RecordedBy) || UserId==RecordedBy || Version<1 || string.IsNullOrWhiteSpace(PersonType) || PersonType.Length>128 || string.IsNullOrWhiteSpace(SourceReference) || SourceReference.Length>2048 || EffectiveFromUtc==default || EffectiveFromUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc<=EffectiveFromUtc || RecordedAtUtc==default || RecordedAtUtc.Kind!=DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(Revision) || RecordId!=CompanyPersonnelIdentityCommand.RecordIdFor(Company,UserId,Version))throw new InvalidOperationException("Complete independently recorded identity link/version/audit is unavailable.");
    }
    public static CompanyPersonnelIdentityVersion From(COMPANY_PERSONNEL_IDENTITY row)
    {var command=CompanyPersonnelIdentityCommand.Read(row);command.RequireRecorded(row);var result=new CompanyPersonnelIdentityVersion(command.Review.Context.Company,command.Review.Context.Database,row.IDENTITY_RECORD_ID,row.APP_USER_ID,row.PERSON_BA_ID,row.PERSON_TYPE,row.VERSION_NO,row.EFFECTIVE_FROM_UTC,row.EFFECTIVE_TO_UTC,row.SOURCE_REFERENCE,row.ROW_CREATED_BY!,row.ROW_CREATED_DATE!.Value,row.RECORDED_REVISION);result.Require();return result;}
}
public sealed record CompanyPersonnelIdentityCatalog(CompanyPersonnelPolicyContext Context,IReadOnlyList<CompanyPersonnelIdentityVersion> Versions,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Context.Require();if(Versions is null || Versions.Count>4096 || Versions.Any(row=>row is null) || Versions.Select(row=>row.RecordId).Distinct(StringComparer.Ordinal).Count()!=Versions.Count || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || Revision!=Digest(Context,Versions))throw new InvalidOperationException("Complete current identity-link population is unavailable.");
        foreach(var row in Versions){row.Require();if(row.Company!=Context.Company || row.Database!=Context.Database || row.RecordedAtUtc>ComparedAtUtc)throw new UnauthorizedAccessException("Identity-link population contains another company/binding or future audit.");}
        foreach(var group in Versions.GroupBy(row=>row.UserId)){var rows=group.OrderBy(row=>row.Version).ToArray();if(!rows.Select(row=>row.Version).SequenceEqual(Enumerable.Range(1,rows.Length)))throw new InvalidOperationException("Identity-link version history is incomplete.");}
        foreach(var group in Versions.GroupBy(row=>row.UserId).Concat(Versions.GroupBy(row=>row.PersonBaId))){var rows=group.OrderBy(row=>row.EffectiveFromUtc).ToArray();for(var i=1;i<rows.Length;i++)if(rows[i].EffectiveFromUtc<rows[i-1].EffectiveToUtc)throw new InvalidOperationException("Overlapping account/person identities are ambiguous; no mapping is silently preferred.");}
    }
    public static string Digest(CompanyPersonnelPolicyContext context,IReadOnlyList<CompanyPersonnelIdentityVersion> versions)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context=context,Versions=versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal).ToArray()}));
    public static CompanyPersonnelIdentityCatalog Create(CompanyPersonnelPolicyContext context,IReadOnlyList<CompanyPersonnelIdentityVersion> versions,DateTime utc){var result=new CompanyPersonnelIdentityCatalog(context,versions,utc,Digest(context,versions));result.Require();return result;}
}
public sealed record CompanyPersonnelIdentityReview(CompanyPersonnelPolicyContext Context,CompanyPersonnelIdentityQuery Query,CompanyApplicationPersonFacts Account,CompanyNativePersonFacts Person,CompanyPersonnelIdentityCatalog Catalog,DateTime ComparedAtUtc,string Revision)
{
    public void Require()
    {
        Context.Require();Query.Require();Account.Require(Context.Company,Query.UserId);Person.Require(Context.Company,Query.PersonBaId);Catalog.Require();
        if(Catalog.Context!=Context || ComparedAtUtc==default || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Catalog.ComparedAtUtc || Revision!=Digest(this))throw new InvalidOperationException("Complete current original identity-link review changed.");
    }
    public static string Digest(CompanyPersonnelIdentityReview review)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(review with{Revision=""}));
    public static CompanyPersonnelIdentityReview Create(CompanyPersonnelPolicyContext context,CompanyPersonnelIdentityQuery query,CompanyApplicationPersonFacts account,CompanyNativePersonFacts person,CompanyPersonnelIdentityCatalog catalog,DateTime utc){var result=new CompanyPersonnelIdentityReview(context,query,account,person,catalog,utc,"");result=result with{Revision=Digest(result)};result.Require();return result;}
}
public sealed class COMPANY_PERSONNEL_IDENTITY:ModelEntityBase
{
    [Key,Required,MaxLength(128)] public string IDENTITY_RECORD_ID {get;set;}="";
    [Required,MaxLength(64)] public string OWNER_TENANT_ID {get;set;}="";
    [Required,MaxLength(128)] public string OWNER_COMPANY_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string APP_USER_ID {get;set;}="";
    [Required,MaxLength(128)] public string PERSON_BA_ID {get;set;}="";
    [Required,MaxLength(128)] public string PERSON_TYPE {get;set;}="";
    public int VERSION_NO {get;set;}
    public DateTime EFFECTIVE_FROM_UTC {get;set;}
    public DateTime EFFECTIVE_TO_UTC {get;set;}
    [Required,MaxLength(2048)] public string SOURCE_REFERENCE {get;set;}="";
    [Required,MaxLength(2*1024*1024)] public string COMMAND_JSON {get;set;}="";
    [Required,MaxLength(64)] public string RECORDED_REVISION {get;set;}="";
}
public sealed record CompanyPersonnelIdentityCommand(CompanyPersonnelIdentityReview Review,int Version,Guid IntentId,DateTime EffectiveFromUtc,DateTime EffectiveToUtc,string SourceReference,string Reason,bool IdentityVerified)
{
    public const int MaximumBodyLength=2*1024*1024;
    public void Require()
    {
        Review.Require();if(Version<1 || IntentId==Guid.Empty || !IdentityVerified || Review.Context.ActorUserId==Review.Query.UserId || EffectiveFromUtc==default || EffectiveFromUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc.Kind!=DateTimeKind.Utc || EffectiveToUtc<=EffectiveFromUtc || new[]{SourceReference,Reason}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>2048 || value.Any(char.IsControl)) || string.IsNullOrWhiteSpace(Review.Person.Type) || Review.Person.Type.Length>128)throw new ArgumentException("Retain exact verified account/person, independent administrator, increasing effective UTC, source evidence, rationale and original intent.");
        if(!Covers(Review.Person.EffectiveFromUtc,Review.Person.EffectiveToUtc) || !Covers(Review.Person.Employment.EffectiveDate,Review.Person.Employment.ExpiryDate) || JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Actual native person/employment effective UTC must cover the complete identity-link window; no date/type/default identity is inferred.");
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=EffectiveFromUtc && to>=EffectiveToUtc && Review.ComparedAtUtc>=from && Review.ComparedAtUtc<to;
    }
    public static string RecordIdFor(CompanyScope company,string user,int version)=>"person-identity-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,User=user,Version=version}));
    public string RecordId()=>RecordIdFor(Review.Context.Company,Review.Query.UserId,Version);
    public void RequireCatalog(CompanyPersonnelIdentityCatalog catalog)
    {
        Require();catalog.Require();if(catalog.Context!=Review.Context || catalog.Revision!=Review.Catalog.Revision || Version!=catalog.Versions.Count(row=>row.UserId==Review.Query.UserId)+1 || catalog.Versions.Any(row=>(row.UserId==Review.Query.UserId || row.PersonBaId==Review.Query.PersonBaId) && EffectiveFromUtc<row.EffectiveToUtc && row.EffectiveFromUtc<EffectiveToUtc))throw new InvalidOperationException("Original complete identity-link population/version changed or account/person windows overlap.");
    }
    public void RequireCurrent(CompanyPersonnelIdentityReview current)
    {Require();current.Require();if(current.Context!=Review.Context || current.Query!=Review.Query || current.Account!=Review.Account || current.Person!=Review.Person || current.Catalog.Revision!=Review.Catalog.Revision || current.ComparedAtUtc<Review.ComparedAtUtc)throw new InvalidOperationException("Complete original account/member/person/identity-link proof changed.");}
    public COMPANY_PERSONNEL_IDENTITY Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc || utc>=EffectiveToUtc)throw new InvalidOperationException("Actual current UTC is required for identity-link audit.");var row=new COMPANY_PERSONNEL_IDENTITY{IDENTITY_RECORD_ID=RecordId(),OWNER_TENANT_ID=Review.Context.Company.TenantId,OWNER_COMPANY_BA_ID=Review.Context.Company.BusinessAssociateId,APP_USER_ID=Review.Query.UserId,PERSON_BA_ID=Review.Query.PersonBaId,PERSON_TYPE=Review.Person.Type!,VERSION_NO=Version,EFFECTIVE_FROM_UTC=EffectiveFromUtc,EFFECTIVE_TO_UTC=EffectiveToUtc,SOURCE_REFERENCE=SourceReference,COMMAND_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",ROW_CREATED_BY=Review.Context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_BY=Review.Context.ActorUserId,ROW_CHANGED_DATE=utc,SOURCE="OILGAS_PERSONNEL_IDENTITY",PPDM_GUID=IntentId.ToString("D")};row.RECORDED_REVISION=Digest(row);return row;
    }
    public static string Digest(COMPANY_PERSONNEL_IDENTITY row){var copy=CompanyInventorySourceReview.Copy(row);copy.RECORDED_REVISION="";return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(COMPANY_PERSONNEL_IDENTITY row){Require();if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original identity-link body/record/account/person/native audit changed.");}
    public static CompanyPersonnelIdentityCommand Read(COMPANY_PERSONNEL_IDENTITY row){if(row.COMMAND_JSON is not {Length:>0} body || body.Length>MaximumBodyLength)throw new InvalidOperationException("Bounded original identity-link proof is unavailable.");CompanyPersonnelIdentityCommand command;try{command=JsonSerializer.Deserialize<CompanyPersonnelIdentityCommand>(body)??throw new InvalidOperationException("Original identity-link command is unavailable.");}catch(JsonException){throw new InvalidOperationException("Original identity-link command is malformed.");}command.RequireRecorded(row);return command;}
}
public sealed record CompanyPersonnelIdentityReference(string RecordId,Guid IntentId)
{public void Require(){if(!LeaseSummaryValidator.Identifier(RecordId) || !RecordId.StartsWith("person-identity-",StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(RecordId["person-identity-".Length..]) || IntentId==Guid.Empty)throw new ArgumentException("Retain original identity-link record and intent references.");}}
public sealed record CompanyPersonnelIdentityRecovery(CompanyPersonnelIdentityCommand Command,COMPANY_PERSONNEL_IDENTITY Recorded)
{public void Require(CompanyScope company,string actor,CompanyPersonnelIdentityReference reference){reference.Require();Command.Require();if(Command.Review.Context.Company!=company || Command.Review.Context.ActorUserId!=actor || Command.RecordId()!=reference.RecordId || Command.IntentId!=reference.IntentId)throw new UnauthorizedAccessException("Original private identity-link maker/company/reference differs.");Command.RequireRecorded(Recorded);}}
public interface ICompanyPersonnelIdentityOwner
{
    Task<CompanyNativePersonFacts> ReadIdentityPersonAsync(CompanyPersonnelPolicyContext context,string personBaId,Func<Task> authority);
    Task<CompanyPersonnelIdentityCatalog> ReadPersonnelIdentitiesAsync(CompanyPersonnelPolicyContext context,Func<Task> authority);
    Task<COMPANY_PERSONNEL_IDENTITY> RecordPersonnelIdentityAsync(CompanyPersonnelIdentityCommand command,Func<Task> authority,Func<Task> currentApplicationProof,bool readOnly=false);
    Task<CompanyPersonnelIdentityRecovery> RecoverPersonnelIdentityReferenceAsync(CompanyPersonnelPolicyContext context,CompanyPersonnelIdentityReference reference,Func<Task> authority);
}

public interface ICompanyDocumentReviewerIdentityCore
{
    Task RequireDocumentReviewerIdentityCoreAsync(CompanyWorkOrderDocumentContributorReview original,Func<Task> authority);
}

/// <summary>Recorded acceptance reviewer identity is distinct from the account viewing approval.</summary>
public interface ICompanyRecordedReviewerIdentityCore
{
    Task RequireRecordedReviewerIdentityCoreAsync(CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts reviewer,string reviewerUserId,DateTime fromUtc,DateTime toUtc,Func<Task> authority);
    Task RequireExecutionReviewerIdentityCoreAsync(FieldSelectionReview executor,CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts reviewer,string reviewerUserId,DateTime fromUtc,DateTime toUtc,Func<Task> authority);
}
public interface ICompanyWorkOrderExecutorIdentityOwner
{
    Task<CompanyWorkOrderExecutorIdentity> ReadExecutorIdentityAsync(CompanyWorkOrderExecutionPrerequisites prerequisites,CompanyMaintenanceAssignedStageReview stage,Func<Task> authority);
    Task RequireExecutorIdentityCoreAsync(CompanyWorkOrderExecutorIdentity original,Func<Task> authority);
}
