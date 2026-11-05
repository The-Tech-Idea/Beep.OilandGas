using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalPostingOutcome> PostCompanySalesJournalAsync(CompanySalesJournalPostingCommand command,ARService ar,Func<Task<CompanySalesJournalPostingSource>> readCurrent,Func<Task> requireAuthority,Func<string,Task> requireFieldRead,Func<string,Task> requireFieldWrite)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(ar);ArgumentNullException.ThrowIfNull(readCurrent);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);ArgumentNullException.ThrowIfNull(requireFieldWrite);command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound native posting owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original posting connection is unavailable.");var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original posting datasource is unavailable.");var current=command.Source.Current;var journalId=command.Source.Reviewed.Header.JOURNAL_ENTRY_ID;
        async Task Guard(bool owned){await requireAuthority();await requireFieldRead(current.FieldId);await requireFieldWrite(current.FieldId);if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original posting binding changed.");await ar.RequireJournalSourceBindingAsync(connection,source);if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequireCompanyJournalPostingSchema(source);}
        PPDMGenericRepository Repo<T>(string table,IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connection,table,rowAccess:policy);
        async Task<T[]> Rows<T>(string table,List<AppFilter> filters,int maximum,bool owned)
        {
            await Guard(owned);var raw=(await Repo<T>(table).GetAsync(filters))?.ToArray();await Guard(owned);if(raw is null || raw.Length>maximum || raw.Any(value=>value is not T))throw new InvalidOperationException("Complete native posting reservations/artifacts are unavailable or ambiguous.");return raw.Cast<T>().ToArray();
        }
        List<AppFilter> By(string key,string value)=>[new(){FieldName=key,Operator="=",FilterValue=value}];
        List<AppFilter> Scope()=>[new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=current.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=current.Company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=current.BookId},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=current.CurrencyCode},new(){FieldName="SOURCE_AR_INVOICE_ID",Operator="=",FilterValue=current.ReceivableId}];
        async Task<SALES_JOURNAL_POSTING_INTENT?> Intent(bool owned)
        {
            var rows=await Rows<SALES_JOURNAL_POSTING_INTENT>("SALES_JOURNAL_POSTING_INTENT",By("JOURNAL_ENTRY_ID",journalId),1,owned);if(rows.SingleOrDefault() is not {} intent)return null;if(JsonSerializer.Serialize(intent.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Another original posting request owns this journal.");return CompanyInventorySourceReview.Copy(intent);
        }
        async Task<JOURNAL_ENTRY> Header(bool owned)
        {
            var rows=await Rows<JOURNAL_ENTRY>("JOURNAL_ENTRY",By("JOURNAL_ENTRY_ID",journalId),1,owned);if(rows.Length!=1)throw new InvalidOperationException("The exact original journal is unavailable.");var row=rows[0];if(row.OWNER_TENANT_ID!=current.Company.TenantId || row.OWNER_COMPANY_BA_ID!=current.Company.BusinessAssociateId || row.FIELD_ID!=current.FieldId || row.BOOK_ID!=current.BookId || row.CURRENCY_CODE!=current.CurrencyCode || row.SOURCE_AR_INVOICE_ID!=current.ReceivableId)throw new InvalidOperationException("The original posting journal context changed.");await RequireActualJournalReviewerAsync(row,connection,source,()=>Guard(owned));return row;
        }
        async Task<JOURNAL_ENTRY_LINE[]> Lines(bool owned)=>(await Rows<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE",By("JOURNAL_ENTRY_ID",journalId),4,owned)).OrderBy(row=>row.LINE_NUMBER).ToArray();
        async Task<GL_ENTRY[]> Entries(bool owned)=>await Rows<GL_ENTRY>("GL_ENTRY",By("JOURNAL_ENTRY_ID",journalId),4,owned);
        async Task Original(bool owned)
        {
            if(JsonSerializer.Serialize(new CompanySalesJournalPreparationOutcome(await Header(owned),await Lines(owned)))!=JsonSerializer.Serialize(command.Source.Reviewed) || (await Entries(owned)).Length!=0)throw new InvalidOperationException("The exact approved journal/empty GL predecessor changed; partial postings cannot be repaired.");
            var actual=await ar.GetCompanySalesReceivableForReviewAsync(current.Company,current.ReceivableId,()=>Guard(owned),requireFieldRead);if(JsonSerializer.Serialize(actual.Receivable)!=JsonSerializer.Serialize(command.Source.Receivable))throw new InvalidOperationException("The exact original native AR posting row changed.");
            if(await ar.ReadCompanyJournalPostingCorrelationAsync(current.Company,current.BookId,current.CurrencyCode,current.ReceivableId,connection,source,()=>Guard(owned)) is not null)throw new InvalidOperationException("This native AR/book/currency already has a posting correlation.");
        }
        async Task Reservations(bool owned,bool existing)
        {
            foreach(var filters in new[]{By("SALES_JOURNAL_POSTING_INTENT_ID",command.IntentId),By("PPDM_GUID",Guid.ParseExact(command.IntentId,"N").ToString()),Scope()}){var rows=await Rows<SALES_JOURNAL_POSTING_INTENT>("SALES_JOURNAL_POSTING_INTENT",filters,1,owned);if(rows.Length!=(existing?1:0) || rows.Any(row=>JsonSerializer.Serialize(row.Restore())!=JsonSerializer.Serialize(command)))throw new InvalidOperationException("The original global posting intent or AR/book/currency reservation is unavailable or occupied.");}
        }
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var existing=await Intent(true);await Reservations(true,existing is not null);if(existing is not null){var header=await Header(true);if(header.STATUS=="POSTED")_=(await ConfirmNativeJournalPostingAsync(header,connection,source,()=>Guard(true),requireFieldRead));else await Original(true);return;}
            await Original(true);command.Source.RequireCurrent(await readCurrent());await Guard(true);var intent=SALES_JOURNAL_POSTING_INTENT.Create(command);await Repo<SALES_JOURNAL_POSTING_INTENT>("SALES_JOURNAL_POSTING_INTENT",new JournalPostingMutation(command,null,()=>Guard(true))).InsertAsync(intent,command.Source.ActorUserId);
            var actual=await Intent(true);if(actual is null || JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The exact original native posting request retention is unconfirmed.");await Reservations(true,true);await Original(true);command.Source.RequireCurrent(await readCurrent());await Guard(true);
        });
        var retained=await Intent(false)??throw new InvalidOperationException("The committed original posting request is unconfirmed. Retain and reconcile it.");CompanySalesJournalPostingOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var intent=await Intent(true)??throw new InvalidOperationException("The retained original posting request disappeared.");if(JsonSerializer.Serialize(intent)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The retained original posting request changed.");await Reservations(true,true);var header=await Header(true);
            if(header.STATUS=="POSTED"){confirmed=await ConfirmNativeJournalPostingAsync(header,connection,source,()=>Guard(true),requireFieldRead);confirmed.Require(command);await Guard(true);return;}
            await Original(true);command.Source.RequireCurrent(await readCurrent());await Guard(true);
            for(var index=0;index<command.EntryIds.Count;index++)
            {
                var id=command.EntryIds[index];foreach(var filters in new[]{By("GL_ENTRY_ID",id),By("PPDM_GUID",Guid.ParseExact(id,"N").ToString())})if((await Rows<GL_ENTRY>("GL_ENTRY",filters,1,true)).Length!=0)throw new InvalidOperationException("An original global GL identity or intent is unavailable or reserved.");
                var entry=command.CreateEntry(index);await Repo<GL_ENTRY>("GL_ENTRY",new JournalPostingMutation(command,retained,()=>Guard(true),index)).InsertAsync(entry,command.Source.ActorUserId);await Guard(true);
            }
            var posted=CompanyInventorySourceReview.Copy(header);posted.STATUS="POSTED";posted.SALES_JOURNAL_POSTING_INTENT_ID=command.IntentId;await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY",new JournalPostingMutation(command,retained,()=>Guard(true))).UpdateAsync(posted,command.Source.ActorUserId);
            var nativeHeader=await Header(true);if(JsonSerializer.Serialize(nativeHeader)!=JsonSerializer.Serialize(posted))throw new InvalidOperationException("The exact native posted header acknowledgement changed.");var entries=await Entries(true);if(entries.Length!=command.EntryIds.Count || entries.Select(value=>value.GL_ENTRY_ID).Distinct().Count()!=entries.Length)throw new InvalidOperationException("The original complete native GL cardinality is unconfirmed.");
            var ordered=command.EntryIds.Select(id=>entries.SingleOrDefault(value=>value.GL_ENTRY_ID==id)??throw new InvalidOperationException("An original native GL row is missing.")).ToArray();var outcome=new CompanySalesJournalPostingOutcome(retained,new(nativeHeader,await Lines(true)),ordered,CompanyInventorySourceReview.Copy(command.Source.Receivable));outcome.Require(command);
            await ar.RecordCompanyJournalPostingCorrelationAsync(outcome,connection,source,()=>Guard(true),requireFieldRead);var native=await ConfirmNativeJournalPostingAsync(nativeHeader,connection,source,()=>Guard(true),requireFieldRead);native.Require(command);if(JsonSerializer.Serialize(native)!=JsonSerializer.Serialize(outcome))throw new InvalidOperationException("The complete atomic journal/GL/AR posting differs from its original native outcome.");
            command.Source.RequireCurrent(await readCurrent());await Reservations(true,true);await Guard(true);confirmed=native;
        });
        var first=await ConfirmNativeJournalPostingAsync(await Header(false),connection,source,()=>Guard(false),requireFieldRead);var closing=await ConfirmNativeJournalPostingAsync(await Header(false),connection,source,()=>Guard(false),requireFieldRead);closing.Require(command);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(confirmed))throw new InvalidOperationException("The durable complete native posting is unconfirmed. Retain and reconcile the original request.");await Guard(false);return closing;
    }
    private sealed class JournalPostingMutation(CompanySalesJournalPostingCommand command,SALES_JOURNAL_POSTING_INTENT? retained,Func<Task> guard,int? index=null):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(actor!=command.Source.ActorUserId)throw new UnauthorizedAccessException();
            if(retained is null && type==typeof(SALES_JOURNAL_POSTING_INTENT) && table=="SALES_JOURNAL_POSTING_INTENT" && entity is SALES_JOURNAL_POSTING_INTENT intent && previous is null && operation==PPDMRowMutation.Insert && JsonSerializer.Serialize(intent.Restore())==JsonSerializer.Serialize(command))return;
            if(retained is not null && type==typeof(GL_ENTRY) && table=="GL_ENTRY" && entity is GL_ENTRY entry && index is {} position && previous is null && operation==PPDMRowMutation.Insert)
            {
                CompanySalesJournalPostingOutcome.RequireEntryInsertion(command,retained,entry,position);return;
            }
            if(retained is not null && type==typeof(JOURNAL_ENTRY) && table=="JOURNAL_ENTRY" && entity is JOURNAL_ENTRY header && previous is JOURNAL_ENTRY old && operation==PPDMRowMutation.Update && JsonSerializer.Serialize(old)==JsonSerializer.Serialize(command.Source.Reviewed.Header)){header.POSTING_WORKFLOW_JSON=CompanySalesJournalPostingHeaderProof.Create(retained,header);_=CompanySalesJournalPostingHeaderProof.Require(header);return;}
            throw new InvalidOperationException("Only original posting retention, exact GL insertions and the approved-to-posted header transition are authorized.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(retained is null || index is not null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY")throw new InvalidOperationException("Posting insertion grants no reads.");return [new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=command.Source.Reviewed.Header.JOURNAL_ENTRY_ID}];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>[await GetReadConstraintsAsync(type,table)];
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(retained is null || index is not null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY" || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(command.Source.Reviewed.Header))throw new InvalidOperationException("The exact independently approved posting predecessor is required.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Posting mutations grant no general reads.");
    }
}
