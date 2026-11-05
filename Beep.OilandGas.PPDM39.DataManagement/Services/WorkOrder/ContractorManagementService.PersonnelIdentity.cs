using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
public partial class ContractorManagementService:ICompanyPersonnelIdentityOwner,ICompanyDocumentReviewerIdentityCore,ICompanyRecordedReviewerIdentityCore,ICompanyCompletionIdentityCore
{
    public Task RequireRecordedReviewerIdentityCoreAsync(CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts reviewer,string reviewerUserId,DateTime fromUtc,DateTime toUtc,Func<Task> authority)
        =>RequireReviewerIdentityCoreAsync(identities.Context,identities,reviewer,reviewerUserId,fromUtc,toUtc,authority,false);

    public Task RequireExecutionReviewerIdentityCoreAsync(FieldSelectionReview executor,CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts reviewer,string reviewerUserId,DateTime fromUtc,DateTime toUtc,Func<Task> authority)
    {
        if(executor.Company!=identities.Context.Company || executor.Database!=identities.Context.Database)throw new UnauthorizedAccessException("Execution reviewer identity requires the original company/native database.");
        FieldSelectionReviewValidator.RequireReview(executor,identities.Context.Company,executor.ActorUserId);
        var context=new CompanyPersonnelPolicyContext(executor.Company,executor.Database,executor.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(executor)));
        return RequireReviewerIdentityCoreAsync(context,identities,reviewer,reviewerUserId,fromUtc,toUtc,authority,true);
    }
    private async Task RequireReviewerIdentityCoreAsync(CompanyPersonnelPolicyContext context,CompanyPersonnelIdentityCatalog identities,CompanyNativePersonFacts reviewer,string reviewerUserId,DateTime fromUtc,DateTime toUtc,Func<Task> authority,bool execution)
    {
        ArgumentNullException.ThrowIfNull(identities);ArgumentNullException.ThrowIfNull(reviewer);ArgumentNullException.ThrowIfNull(authority);identities.Require();reviewer.Require(identities.Context.Company,reviewer.PersonBaId);
        var utc=DateTime.UtcNow;
        if(!LeaseSummaryValidator.Identifier(reviewerUserId) || fromUtc.Kind!=DateTimeKind.Utc || toUtc.Kind!=DateTimeKind.Utc || toUtc<=fromUtc || utc<fromUtc || utc>=toUtc)throw new InvalidOperationException("An explicit current permit/reviewer UTC window is required.");
        var rows=await ReadIdentityRowsAsync(context,authority,true);var current=CompanyPersonnelIdentityCatalog.Create(context,rows.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);
        if(execution?JsonSerializer.Serialize(current.Versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal))!=JsonSerializer.Serialize(identities.Versions.OrderBy(row=>row.RecordId,StringComparer.Ordinal)):current.Revision!=identities.Revision)throw new InvalidOperationException("Complete recorded-reviewer identity population changed inside the owned boundary.");
        var links=current.Versions.Where(row=>row.UserId==reviewerUserId && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc).ToArray();
        if(links.Length!=1 || links[0].PersonBaId!=reviewer.PersonBaId || links[0].PersonType!=reviewer.Type || links[0].RecordedBy==reviewerUserId || links[0].EffectiveFromUtc>fromUtc || links[0].EffectiveToUtc<toUtc)throw new InvalidOperationException("The exact recorded reviewer/native person does not cover this permit window.");
        var person=await ReadIdentityPersonCoreAsync(context,reviewer.PersonBaId,authority,true);
        if(person!=reviewer || !Covers(person.EffectiveFromUtc,person.EffectiveToUtc) || !Covers(person.Employment.EffectiveDate,person.Employment.ExpiryDate))throw new InvalidOperationException("Current recorded reviewer person/employment proof changed inside approval.");
        var closingRows=await ReadIdentityRowsAsync(context,authority,true);var closing=CompanyPersonnelIdentityCatalog.Create(context,closingRows.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);
        if(closing.Revision!=current.Revision || await ReadIdentityPersonCoreAsync(context,reviewer.PersonBaId,authority,true)!=person)throw new InvalidOperationException("Complete recorded reviewer identity/person proof changed during owned closure.");
        await authority();
        bool Covers(DateTime? start,DateTime? end)=>start is {} from && end is {} to && from.Kind==DateTimeKind.Utc && to.Kind==DateTimeKind.Utc && from<=fromUtc && to>=toUtc;
    }
    public async Task RequireDocumentReviewerIdentityCoreAsync(CompanyWorkOrderDocumentContributorReview original,Func<Task> authority)
    {
        original.Require();var context=original.Identities.Context;var rows=await ReadIdentityRowsAsync(context,authority,true);var catalog=CompanyPersonnelIdentityCatalog.Create(context,rows.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);if(catalog.Revision!=original.Identities.Revision || original.ReviewerPerson is null)throw new InvalidOperationException("Complete original local reviewer identity-link population changed.");var person=await ReadIdentityPersonCoreAsync(context,original.ReviewerPerson.PersonBaId,authority,true);if(person!=original.ReviewerPerson)throw new InvalidOperationException("Original local reviewer person/employment proof changed.");await authority();
    }
    private async Task<PPDMGenericRepository> IdentityRepository(Type type,string[] keys,IDataSource source,CompanyPersonnelPolicyContext context)
    {
        var table=type.Name;if(!source.CheckEntityExist(table))throw new InvalidOperationException("Review the explicit PPDM_CORE Beep identity/source manifest; required schema is unavailable.");var shape=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var actual=shape?.Fields?.Where(field=>field.IsKey).ToArray();
        if(actual is null || actual.Any(field=>field.AllowDBNull) || !actual.Select(field=>field.FieldName).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Complete native identity/person key metadata is unavailable.");
        var definition=await _metadata.GetTableMetadataAsync(table);if(definition is null || definition.EntityTypeName!=type.FullName && definition.EntityTypeName!=type.Name || !definition.PrimaryKeyColumn.Split(',').Select(value=>value.Trim()).Order(StringComparer.Ordinal).SequenceEqual(keys.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Canonical identity/person metadata is unavailable.");
        var foreign=table switch{"BA_EMPLOYEE"=>new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID"},"COMPANY_PERSONNEL_IDENTITY"=>new[]{"PERSON_BA_ID","OWNER_COMPANY_BA_ID"},_=>Array.Empty<string>()};foreach(var column in foreign){var rows=definition.ForeignKeys.Where(row=>row.ForeignKeyColumn==column).ToArray();if(rows.Length!=1 || rows[0].ReferencedTable!="BUSINESS_ASSOCIATE" || rows[0].ReferencedPrimaryKey!="BUSINESS_ASSOCIATE_ID")throw new InvalidOperationException("Complete unambiguous person/company relationship metadata is unavailable.");}
        if(table=="COMPANY_PERSONNEL_IDENTITY")foreach(var (name,size) in new[]{("COMMAND_JSON",CompanyPersonnelIdentityCommand.MaximumBodyLength),("RECORDED_REVISION",64)}){var fields=shape!.Fields.Where(field=>field.FieldName==name).ToArray();if(fields.Length!=1 || fields[0].Fieldtype!=typeof(string).FullName || fields[0].AllowDBNull || fields[0].Size1<size)throw new InvalidOperationException("Bounded identity proof columns require the explicit Beep manifest.");}
        return new(_editor,_commonColumnHandler,_defaults,_metadata,type,context.Database.ConnectionName,table);
    }
    private async Task RequireIdentityTarget(CompanyPersonnelPolicyContext context,IDataSource source,Func<Task> authority,bool owned)
    {
        context.Require();await authority();await _database.RequireContextAsync(context.Company,context.ActorUserId,context.Database);if(!ReferenceEquals(source,_editor.GetDataSource(context.Database.ConnectionName)))throw new InvalidOperationException("Original personnel identity target changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(source);
    }
    public Task<CompanyNativePersonFacts> ReadIdentityPersonAsync(CompanyPersonnelPolicyContext context,string personBaId,Func<Task> authority)=>ReadIdentityPersonCoreAsync(context,personBaId,authority,false);
    public Task<CompanyNativePersonFacts> ReadCompletionPersonCoreAsync(CompanyPersonnelPolicyContext context,string personBaId,Func<Task> authority)=>ReadIdentityPersonCoreAsync(context,personBaId,authority,true);
    public async Task<CompanyPersonnelIdentityCatalog> ReadCompletionIdentitiesCoreAsync(CompanyPersonnelPolicyContext context,Func<Task> authority)
    {var first=await ReadIdentityRowsAsync(context,authority,true);var closing=await ReadIdentityRowsAsync(context,authority,true);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete owned completion identity population changed.");return CompanyPersonnelIdentityCatalog.Create(context,closing.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);}
    private async Task<CompanyNativePersonFacts> ReadIdentityPersonCoreAsync(CompanyPersonnelPolicyContext context,string personBaId,Func<Task> authority,bool owned)
    {
        context.Require();if(!LeaseSummaryValidator.Identifier(personBaId) || personBaId==context.Company.BusinessAssociateId)throw new ArgumentException("Exact native person distinct from the company is required.");var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original person source is unavailable.");
        async Task Guard()=>await RequireIdentityTarget(context,source,authority,owned);
        async Task<(BA_EMPLOYEE Employee,BUSINESS_ASSOCIATE Person)> Read()
        {
            await Guard();var employees=(await (await IdentityRepository(typeof(BA_EMPLOYEE),["EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"],source,context)).GetAsync([new(){FieldName="EMPLOYER_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId},new(){FieldName="EMPLOYEE_BA_ID",Operator="=",FilterValue=personBaId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(employees.Length!=1 || employees[0] is not BA_EMPLOYEE employee || employee.EMPLOYER_BA_ID!=context.Company.BusinessAssociateId || employee.EMPLOYEE_BA_ID!=personBaId || employee.ACTIVE_IND!="Y" || employee.EMPLOYEE_OBS_NO<0)throw new UnauthorizedAccessException("One exact current company employee relationship is required before personal metadata.");DateKinds(employee.EFFECTIVE_DATE,employee.EXPIRY_DATE);if(employee.EFFECTIVE_DATE is not {} start || employee.EXPIRY_DATE is not {} end || start>DateTime.UtcNow || end<=DateTime.UtcNow)throw new UnauthorizedAccessException("Current explicit employment effective UTC is required before private person metadata.");
            var persons=(await (await IdentityRepository(typeof(BUSINESS_ASSOCIATE),["BUSINESS_ASSOCIATE_ID"],source,context)).GetAsync([new(){FieldName="BUSINESS_ASSOCIATE_ID",Operator="=",FilterValue=personBaId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}])).ToArray();await Guard();
            if(persons.Length!=1 || persons[0] is not BUSINESS_ASSOCIATE person || person.BUSINESS_ASSOCIATE_ID!=personBaId || person.ACTIVE_IND!="Y")throw new UnauthorizedAccessException("Exact active native person is unavailable or foreign.");DateKinds(person.EFFECTIVE_DATE,person.EXPIRY_DATE);return(CompanyInventorySourceReview.Copy(employee),CompanyInventorySourceReview.Copy(person));
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(new{first.Employee,first.Person})!=JsonSerializer.Serialize(new{closing.Employee,closing.Person}))throw new InvalidOperationException("Complete original company employee/person metadata changed.");await Guard();var e=closing.Employee;var p=closing.Person;var employment=new CompanyEmployeeSourceFacts(e.EMPLOYER_BA_ID,e.EMPLOYEE_BA_ID,e.EMPLOYEE_OBS_NO,e.EMPLOYEE_POSITION,e.STATUS,e.EFFECTIVE_DATE,e.EXPIRY_DATE,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(e)));var result=new CompanyNativePersonFacts(context.Company,personBaId,p.BA_LONG_NAME,p.BA_TYPE,p.EFFECTIVE_DATE,p.EXPIRY_DATE,employment,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{closing.Employee,closing.Person})));result.Require(context.Company,personBaId);return result;
        static void DateKinds(params DateTime?[] values){if(values.Any(value=>value is {} date && date.Kind!=DateTimeKind.Utc))throw new InvalidOperationException("Native person/employment time zone is unconfirmed; cloning cannot establish UTC provenance.");}
    }
    private async Task<COMPANY_PERSONNEL_IDENTITY[]> ReadIdentityRowsAsync(CompanyPersonnelPolicyContext context,Func<Task> authority,bool owned=false)
    {
        var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original personnel identity source is unavailable.");await RequireIdentityTarget(context,source,authority,owned);var repo=await IdentityRepository(typeof(COMPANY_PERSONNEL_IDENTITY),["IDENTITY_RECORD_ID"],source,context);var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=context.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=context.Company.BusinessAssociateId}])).ToArray();await RequireIdentityTarget(context,source,authority,owned);
        if(raw.Length>4096 || raw.Any(value=>value is not COMPANY_PERSONNEL_IDENTITY row || row.OWNER_TENANT_ID!=context.Company.TenantId || row.OWNER_COMPANY_BA_ID!=context.Company.BusinessAssociateId || row.ACTIVE_IND!="Y"))throw new UnauthorizedAccessException("Complete identity-link population is unavailable or foreign before private proof parsing.");var rows=raw.Cast<COMPANY_PERSONNEL_IDENTITY>().ToArray();if(rows.Select(row=>row.IDENTITY_RECORD_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Identity-link record population is ambiguous.");return rows.Select(CompanyInventorySourceReview.Copy).OrderBy(row=>row.IDENTITY_RECORD_ID,StringComparer.Ordinal).ToArray();
    }
    public async Task<CompanyPersonnelIdentityCatalog> ReadPersonnelIdentitiesAsync(CompanyPersonnelPolicyContext context,Func<Task> authority)
    {var first=await ReadIdentityRowsAsync(context,authority);var closing=await ReadIdentityRowsAsync(context,authority);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Complete original identity-link population changed.");return CompanyPersonnelIdentityCatalog.Create(context,closing.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow);}
    public async Task<COMPANY_PERSONNEL_IDENTITY> RecordPersonnelIdentityAsync(CompanyPersonnelIdentityCommand original,Func<Task> authority,Func<Task> currentApplicationProof,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var context=command.Review.Context;var source=_editor.GetDataSource(context.Database.ConnectionName)??throw new InvalidOperationException("Original identity-link target is unavailable.");async Task Guard(bool owned=false)=>await RequireIdentityTarget(context,source,authority,owned);
        COMPANY_PERSONNEL_IDENTITY? Existing(IReadOnlyList<COMPANY_PERSONNEL_IDENTITY> rows){var row=rows.SingleOrDefault(row=>row.IDENTITY_RECORD_ID==command.RecordId());if(row is null)return null;if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Only the original identity-link recorder may recover private proof.");command.RequireRecorded(row);return row;}
        async Task<COMPANY_PERSONNEL_IDENTITY[]> Read(bool owned=false)=>await ReadIdentityRowsAsync(context,()=>Guard(owned),owned);
        void Baseline(IReadOnlyList<COMPANY_PERSONNEL_IDENTITY> rows)=>command.RequireCatalog(CompanyPersonnelIdentityCatalog.Create(context,rows.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow));
        var first=await Read();var saved=Existing(first);if(readOnly || saved is not null){if(saved is null)throw new InvalidOperationException("Original identity-link append is unconfirmed; retain its intent.");var closing=Existing(await Read());if(JsonSerializer.Serialize(saved)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original identity-link history changed during recovery.");return saved;}
        await currentApplicationProof();Baseline(first);COMPANY_PERSONNEL_IDENTITY? confirmed=null;
        async Task Person(bool owned){var facts=await ReadIdentityPersonCoreAsync(context,command.Review.Query.PersonBaId,()=>Guard(owned),owned);if(facts!=command.Review.Person)throw new InvalidOperationException("Original complete native person/employment proof changed.");}
        await Person(false);await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);await currentApplicationProof();var before=await Read(true);confirmed=Existing(before);if(confirmed is not null)return;Baseline(before);await Person(true);var row=command.Prepare(DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_PERSONNEL_IDENTITY),context.Database.ConnectionName,"COMPANY_PERSONNEL_IDENTITY",rowAccess:new PersonnelIdentityInsertPolicy(row,command,()=>Guard(true)));
            await writer.InsertAsync(row,context.ActorUserId);var after=await Read(true);confirmed=Existing(after);if(confirmed is null)throw new InvalidOperationException("Original identity-link append is unconfirmed.");Baseline(after.Where(value=>value.IDENTITY_RECORD_ID!=row.IDENTITY_RECORD_ID).ToArray());CompanyPersonnelIdentityCatalog.Create(context,after.Select(CompanyPersonnelIdentityVersion.From).ToArray(),DateTime.UtcNow).Require();await Person(true);await currentApplicationProof();await Guard(true);
        });var durable=Existing(await Read());if(confirmed is null || JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Committed original identity link is unconfirmed. Recover its original intent.");await currentApplicationProof();await Guard();return durable!;
    }
    public async Task<CompanyPersonnelIdentityRecovery> RecoverPersonnelIdentityReferenceAsync(CompanyPersonnelPolicyContext context,CompanyPersonnelIdentityReference reference,Func<Task> authority)
    {
        reference.Require();var rows=await ReadIdentityRowsAsync(context,authority);var row=rows.SingleOrDefault(row=>row.IDENTITY_RECORD_ID==reference.RecordId)??throw new InvalidOperationException("Original identity-link record is unavailable.");if(row.ROW_CREATED_BY!=context.ActorUserId)throw new UnauthorizedAccessException("Original recorded maker closes before private identity proof parsing.");var command=CompanyPersonnelIdentityCommand.Read(row);var result=new CompanyPersonnelIdentityRecovery(command,row);result.Require(context.Company,context.ActorUserId,reference);var closing=await RecordPersonnelIdentityAsync(command,authority,()=>throw new InvalidOperationException("Private recovery cannot create a new identity link."),true);if(JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original identity-link record changed during private recovery.");return new(command,closing);
    }
    private sealed class PersonnelIdentityInsertPolicy(COMPANY_PERSONNEL_IDENTITY original,CompanyPersonnelIdentityCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private readonly COMPANY_PERSONNEL_IDENTITY expected=CompanyInventorySourceReview.Copy(original);
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {await guard();if(type!=typeof(COMPANY_PERSONNEL_IDENTITY) || table!="COMPANY_PERSONNEL_IDENTITY" || operation!=PPDMRowMutation.Insert || previous is not null || !ReferenceEquals(entity,original) || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only one exact original verified identity append is authorized.");original.PPDM_GUID=expected.PPDM_GUID;original.ACTIVE_IND=expected.ACTIVE_IND;original.ROW_CREATED_BY=expected.ROW_CREATED_BY;original.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;original.ROW_CHANGED_BY=expected.ROW_CHANGED_BY;original.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;command.RequireRecorded(original);}
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Identity append grants no general row access.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Identity append grants no general row access.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Identity append grants no general row access.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Identity append grants no general row access.");
    }
}
