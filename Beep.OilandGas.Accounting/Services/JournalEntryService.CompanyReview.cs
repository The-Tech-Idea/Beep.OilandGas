using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalReviewRecovery> GetCompanySalesJournalReviewRecoveryAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound reviewer recovery owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original review binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original review datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original reviewer recovery binding changed.");RequireCompanyJournalReviewSchema(source);}
        async Task<CompanySalesJournalReviewRecovery> Capture()
        {
            await Guard();var outcome=await GetCompanySalesJournalForReviewAsync(company,actor,book,currency,id,Guard,requireFieldRead) ??throw new InvalidOperationException("The original reviewed journal is unavailable.");await Guard();
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_JOURNAL_REVIEW_INTENT),connection,"SALES_JOURNAL_REVIEW_INTENT");var rows=(await repo.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not SALES_JOURNAL_REVIEW_INTENT))throw new InvalidOperationException("The original reviewer recovery reservation is unavailable or ambiguous.");var intent=rows.SingleOrDefault() as SALES_JOURNAL_REVIEW_INTENT;
            if(intent is not null && (intent.OWNER_TENANT_ID!=company.TenantId || intent.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency || intent.ROW_CREATED_BY!=actor))throw new UnauthorizedAccessException("Only the original reviewer can recover this request in the original company book/currency.");
            var result=new CompanySalesJournalReviewRecovery(company,actor,id,intent,intent is not null && outcome.Header.REVIEW_WORKFLOW_JSON is not null?outcome:null);result.Require();await Guard();return CompanyInventorySourceReview.Copy(result);
        }
        var first=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original reviewer recovery changed.");return closing;
    }
    private async Task RequireActualJournalReviewerAsync(JOURNAL_ENTRY header,string connection,IDataSource source,Func<Task> guard)
    {
        if(header.REVIEW_WORKFLOW_JSON is null)return;await guard();RequireCompanyJournalReviewSchema(source);var proof=CompanySalesJournalReviewProof.Require(header);
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_JOURNAL_REVIEW_INTENT),connection,"SALES_JOURNAL_REVIEW_INTENT");
        var rows=(await repo.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=header.JOURNAL_ENTRY_ID}]))?.ToArray();await guard();
        if(rows is not {Length:1} || rows[0] is not SALES_JOURNAL_REVIEW_INTENT row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(proof.Intent))throw new InvalidOperationException("The reviewed native journal lacks its exact original retained reviewer artifact. Reconcile; do not recreate.");
        _=row.Restore();
    }
    private static void RequireCompanyJournalReviewSchema(IDataSource source)
    {
        RequireCompanyJournalSchema(source);var header=source.GetEntityStructure(new EntityStructure{EntityName="JOURNAL_ENTRY",DatasourceEntityName="JOURNAL_ENTRY"},true);var proof=header?.Fields?.Where(field=>field.FieldName=="REVIEW_WORKFLOW_JSON").ToArray();
        if(proof is not {Length:1} || !proof[0].AllowDBNull || proof[0].Fieldtype!=typeof(string).FullName || Math.Max(proof[0].Size,proof[0].Size1)<CompanySalesJournalReviewProof.MaximumLength)throw new InvalidOperationException("Review the bounded nullable native journal review proof through the PRODUCTION Beep migration.");
        const string table="SALES_JOURNAL_REVIEW_INTENT";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].Fieldtype!=typeof(string).FullName || keys[0].AllowDBNull)throw new InvalidOperationException("The original native journal reviewer key is unconfirmed.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("BOOK_ID",128),("CURRENCY_CODE",16),("FIELD_ID",128),("JOURNAL_ENTRY_ID",32),("SOURCE_REVISION",64),("COMMAND_JSON",SALES_JOURNAL_REVIEW_INTENT.MaximumCommandLength)})
        {var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Complete bounded original journal reviewer metadata is unconfirmed.");}
    }
    public async Task<CompanySalesJournalPreparationOutcome> RecordCompanySalesJournalReviewAsync(CompanySalesJournalReviewCommand command,Func<Task> requireAuthority,Func<Task<CompanySalesJournalReviewSource>> readCurrent)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound journal review owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original review binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original review datasource is unavailable.");var original=command.Review.Original;var maker=CompanySalesJournalPreparationProof.Require(original.Header).Command;var scope=maker.Source.Company;
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original journal review binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequireCompanyJournalReviewSchema(source);}
        PPDMGenericRepository Repo<T>(string table,IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connection,table,rowAccess:policy);
        async Task<SALES_JOURNAL_REVIEW_INTENT?> Intent(bool owned)
        {
            await Guard(owned);var raw=(await Repo<SALES_JOURNAL_REVIEW_INTENT>("SALES_JOURNAL_REVIEW_INTENT").GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=maker.JournalId}]))?.ToArray();await Guard(owned);
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_JOURNAL_REVIEW_INTENT))throw new InvalidOperationException("The unique original journal reviewer reservation is unconfirmed.");if(raw.SingleOrDefault() is not SALES_JOURNAL_REVIEW_INTENT value)return null;
            if(JsonSerializer.Serialize(value.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Another original reviewer command owns this journal.");return CompanyInventorySourceReview.Copy(value);
        }
        async Task<CompanySalesJournalPreparationOutcome> Outcome(bool owned)
        {
            await Guard(owned);var result=await GetCompanySalesJournalForReviewAsync(scope,command.Review.ActorUserId,maker.Source.BookId,maker.Source.CurrencyCode,maker.JournalId,()=>Guard(owned),_=>Guard(owned)) ??throw new InvalidOperationException("The original native journal outcome is unavailable.");await Guard(owned);return result;
        }
        async Task MatchOriginal(bool owned){if(JsonSerializer.Serialize(await Outcome(owned))!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original draft header or native lines changed.");}
        // Retain the immutable independent request before the separate disposition transaction.
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);if(await Intent(true) is not null)return;
            foreach(var (field,value) in new[]{("SALES_JOURNAL_REVIEW_INTENT_ID",command.IntentId),("PPDM_GUID",Guid.ParseExact(command.IntentId,"N").ToString())})
            {var reserved=(await Repo<SALES_JOURNAL_REVIEW_INTENT>("SALES_JOURNAL_REVIEW_INTENT").GetAsync([new(){FieldName=field,Operator="=",FilterValue=value}]))?.ToArray();await Guard(true);if(reserved is null || reserved.Length!=0)throw new InvalidOperationException("The original global reviewer identity or intent is unavailable or reserved.");}
            await MatchOriginal(true);command.Review.RequireCurrent(await readCurrent());await Guard(true);var row=SALES_JOURNAL_REVIEW_INTENT.Create(command);await Repo<SALES_JOURNAL_REVIEW_INTENT>("SALES_JOURNAL_REVIEW_INTENT",new JournalReviewMutation(command,null,()=>Guard(true))).InsertAsync(row,command.Review.ActorUserId);
            var stored=await Intent(true);if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The retained original reviewer request is unconfirmed.");await MatchOriginal(true);command.Review.RequireCurrent(await readCurrent());await Guard(true);
        });
        var retained=await Intent(false) ??throw new InvalidOperationException("The committed original reviewer request is unconfirmed. Reconcile it before another command.");CompanySalesJournalPreparationOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var intent=await Intent(true) ??throw new InvalidOperationException("The original retained review disappeared.");if(JsonSerializer.Serialize(intent)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The original retained review changed.");var current=await Outcome(true);
            if(current.Header.REVIEW_WORKFLOW_JSON is not null){if(JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(current.Header).Intent)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The reviewed native header belongs to another original reviewer request.");confirmed=current;await Guard(true);return;}
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original draft changed before review.");command.Review.RequireCurrent(await readCurrent());await Guard(true);
            var updated=CompanyInventorySourceReview.Copy(current.Header);updated.STATUS=command.Decision=="APPROVE"?"APPROVED":"REJECTED";await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY",new JournalReviewMutation(command,retained,()=>Guard(true))).UpdateAsync(updated,command.Review.ActorUserId);
            var result=await Outcome(true);if(JsonSerializer.Serialize(result.Header)!=JsonSerializer.Serialize(updated) || JsonSerializer.Serialize(result.Lines)!=JsonSerializer.Serialize(original.Lines) || JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(result.Header).Intent)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The native independent review differs from the exact original request or frozen lines.");
            // Compare fresh maker-view source while keeping the original draft in the disposition read contract.
            command.Review.RequireCurrent(await readCurrent());await Guard(true);confirmed=result;
        });
        var durable=await Outcome(false);if(JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed) || JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(durable.Header).Intent)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The committed independent review is unconfirmed. Retain and reconcile its original command.");await Guard(false);return durable;
    }
    private sealed class JournalReviewMutation(CompanySalesJournalReviewCommand command,SALES_JOURNAL_REVIEW_INTENT? intent,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(actor!=command.Review.ActorUserId)throw new UnauthorizedAccessException();
            if(intent is null && type==typeof(SALES_JOURNAL_REVIEW_INTENT) && table=="SALES_JOURNAL_REVIEW_INTENT" && entity is SALES_JOURNAL_REVIEW_INTENT row && previous is null && operation==PPDMRowMutation.Insert)
            {if(JsonSerializer.Serialize(row.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Only the exact original independent reviewer insertion is authorized.");return;}
            if(intent is not null && type==typeof(JOURNAL_ENTRY) && table=="JOURNAL_ENTRY" && entity is JOURNAL_ENTRY header && previous is JOURNAL_ENTRY old && operation==PPDMRowMutation.Update && JsonSerializer.Serialize(old)==JsonSerializer.Serialize(command.Review.Original.Header))
            {header.REVIEW_WORKFLOW_JSON=CompanySalesJournalReviewProof.Create(intent,header);_=CompanySalesJournalReviewProof.Require(header);return;}
            throw new InvalidOperationException("Only exact reviewer request retention and the original draft disposition are authorized.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(intent is null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY")throw new InvalidOperationException("Review retention grants no reads.");return [new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=command.Review.Original.Header.JOURNAL_ENTRY_ID}];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>[await GetReadConstraintsAsync(type,table)];
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(intent is null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY" || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(command.Review.Original.Header))throw new InvalidOperationException("The exact original draft predecessor is required.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Journal disposition grants no general read access.");
    }
}
