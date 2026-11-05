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
    private static void RequireCompanyJournalSchema(IDataSource source)
    {
        foreach(var table in new[]{"JOURNAL_ENTRY","JOURNAL_ENTRY_LINE"})
        {
            var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
            if(schema?.EntityName!=table || keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<128)throw new InvalidOperationException("Review the canonical journal identity through the PRODUCTION Beep migration.");
            var strings=new List<(string Name,int Size)>{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("BOOK_ID",128),("FIELD_ID",128),("SOURCE_AR_INVOICE_ID",32),("COMPANY_LEDGER_PERIOD_ID",32),("CURRENCY_CODE",16)};
            strings.AddRange(table=="JOURNAL_ENTRY"?[("PREPARATION_WORKFLOW_JSON",CompanySalesJournalPreparationProof.MaximumLength),("ENTRY_NUMBER",128),("DESCRIPTION",1000),("REFERENCE_NUMBER",128),("SOURCE_MODULE",128),("STATUS",32),("ENTRY_TYPE",128)]:[("JOURNAL_ENTRY_ID",128),("GL_ACCOUNT_ID",128),("DESCRIPTION",128),("MAPPING_KEY",128)]);
            foreach(var (name,size) in strings){var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Complete bounded company/source/currency journal metadata is unconfirmed.");}
            foreach(var (name,type) in table=="JOURNAL_ENTRY"?new[]{("ENTRY_DATE",typeof(DateTime)),("TOTAL_DEBIT",typeof(decimal)),("TOTAL_CREDIT",typeof(decimal))}:new[]{("LINE_NUMBER",typeof(int)),("DEBIT_AMOUNT",typeof(decimal)),("CREDIT_AMOUNT",typeof(decimal))})
            {var fields=schema.Fields.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName)throw new InvalidOperationException("Exact native journal date/line/currency amount types are unconfirmed.");}
        }
    }
    public async Task<CompanySalesJournalPreparationOutcome> PrepareCompanySalesJournalAsync(CompanySalesJournalPreparation command,Func<Task> requireAuthority,Func<Task<CompanySalesJournalSourceReview>> readCurrent)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound PRODUCTION journal preparation owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original journal preparation binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original journal datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original journal preparation binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequireCompanyJournalSchema(source);}
        PPDMGenericRepository Repo<T>(string table,IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connection,table,rowAccess:policy);
        List<AppFilter> Scope()=>[new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Source.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Source.Company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=command.Source.BookId},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=command.Source.CurrencyCode}];
        async Task<JOURNAL_ENTRY?> Header(bool owned)
        {
            await Guard(owned);var rows=(await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY").GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=command.JournalId}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not JOURNAL_ENTRY))throw new InvalidOperationException("The original journal identity is unavailable or ambiguous.");var row=rows.SingleOrDefault() as JOURNAL_ENTRY;if(row is null)return null;
            var proof=CompanySalesJournalPreparationProof.Require(row);if(JsonSerializer.Serialize(proof.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Another original request owns this native journal identity.");return CompanyInventorySourceReview.Copy(row);
        }
        async Task<JOURNAL_ENTRY_LINE[]> Lines(bool owned)
        {
            await Guard(owned);var raw=(await Repo<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE").GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=command.JournalId}]))?.ToArray();await Guard(owned);
            if(raw is null || raw.Length>4 || raw.Any(row=>row is not JOURNAL_ENTRY_LINE))throw new InvalidOperationException("Complete original journal lines are unavailable or ambiguous.");return raw.Cast<JOURNAL_ENTRY_LINE>().OrderBy(row=>row.LINE_NUMBER).Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        async Task Reservations(bool owned,bool existing)
        {
            foreach(var (name,value) in new[]{("PPDM_GUID",Guid.ParseExact(command.IntentId,"N").ToString()),("SOURCE_AR_INVOICE_ID",command.Source.ReceivableId),("ENTRY_NUMBER",command.Number)})
            {
                await Guard(owned);var filters=name=="PPDM_GUID"?new List<AppFilter>():Scope();filters.Add(new(){FieldName=name,Operator="=",FilterValue=value});var rows=(await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY").GetAsync(filters))?.ToArray();await Guard(owned);
                if(rows is null || rows.Length>(existing?1:0) || rows.Any(row=>row is not JOURNAL_ENTRY header || header.JOURNAL_ENTRY_ID!=command.JournalId) || existing && rows.Length!=1)throw new InvalidOperationException("The original global intent, company/source or document reservation is unavailable, duplicated or owned by another journal.");
            }
        }
        // The native header retains the exact original request before the separate draft transaction.
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var existing=await Header(true);await Reservations(true,existing is not null);
            if(existing is not null){await RequireActualJournalPostingAsync(existing,connection,source,()=>Guard(true),_=>Guard(true));CompanySalesJournalPreparationProof.Require(existing).RequireLines(existing,await Lines(true));await Guard(true);return;}
            if((await Lines(true)).Length!=0)throw new InvalidOperationException("Orphaned original journal lines cannot be repaired by creating another header.");
            command.RequireCurrent(await readCurrent());await Guard(true);var row=command.CreateHeader();var policy=new JournalPreparationMutation(command,null,[],()=>Guard(true));await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY",policy).InsertAsync(row,command.Source.ActorUserId);
            var stored=await Header(true);if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The exact retained journal request is unconfirmed.");await Reservations(true,true);command.RequireCurrent(await readCurrent());await Guard(true);
        });
        var retained=await Header(false) ??throw new InvalidOperationException("The committed original preparation request is unconfirmed. Reconcile before another request.");
        CompanySalesJournalPreparationOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var header=await Header(true) ??throw new InvalidOperationException("The original retained journal is unavailable.");await Reservations(true,true);var lines=await Lines(true);var proof=CompanySalesJournalPreparationProof.Require(header);
            if(header.STATUS is "DRAFT" or "APPROVED" or "REJECTED" or "POSTED"){await RequireActualJournalPostingAsync(header,connection,source,()=>Guard(true),_=>Guard(true));proof.RequireLines(header,lines);confirmed=new(header,lines);await Guard(true);return;}
            if(header.STATUS!="REQUESTED" || lines.Length!=0 || JsonSerializer.Serialize(header)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The exact retained request or empty draft line reservation changed.");
            command.RequireCurrent(await readCurrent());await Guard(true);
            for(var index=0;index<command.LineIds.Count;index++)
            {
                await Guard(true);var id=command.LineIds[index];var reserved=(await Repo<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE").GetAsync([new(){FieldName="JOURNAL_ENTRY_LINE_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard(true);
                var guid=(await Repo<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE").GetAsync([new(){FieldName="PPDM_GUID",Operator="=",FilterValue=Guid.ParseExact(id,"N").ToString()}]))?.ToArray();await Guard(true);
                if(reserved is null || guid is null || reserved.Length!=0 || guid.Length!=0)throw new InvalidOperationException("The original global line identity or intent is unavailable or already reserved.");
                var line=command.CreateLine(index);await Repo<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE",new JournalPreparationMutation(command,retained,[],()=>Guard(true),index)).InsertAsync(line,command.Source.ActorUserId);
            }
            var storedLines=await Lines(true);if(storedLines.Length!=command.LineIds.Count)throw new InvalidOperationException("The exact original journal line cardinality is unconfirmed.");
            var updated=CompanyInventorySourceReview.Copy(header);updated.STATUS="DRAFT";await Repo<JOURNAL_ENTRY>("JOURNAL_ENTRY",new JournalPreparationMutation(command,retained,storedLines,()=>Guard(true))).UpdateAsync(updated,command.Source.ActorUserId);
            var stored=await Header(true) ??throw new InvalidOperationException("The exact draft header is unconfirmed.");var closing=await Lines(true);var outcome=new CompanySalesJournalPreparationOutcome(stored,closing);outcome.Require(command);if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(updated))throw new InvalidOperationException("The native draft header differs from its original preparation.");
            command.RequireCurrent(await readCurrent());await Guard(true);await Reservations(true,true);confirmed=outcome;
        });
        var durableHeader=await Header(false) ??throw new InvalidOperationException("The committed draft header is unavailable. Reconcile the original request.");var durable=new CompanySalesJournalPreparationOutcome(durableHeader,await Lines(false));durable.Require(command);
        if(JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed))throw new InvalidOperationException("The committed draft differs from its exact original transaction outcome. Retain and reconcile.");await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }
    private sealed class JournalPreparationMutation(CompanySalesJournalPreparation command,JOURNAL_ENTRY? retained,IReadOnlyList<JOURNAL_ENTRY_LINE> lines,Func<Task> guard,int? lineIndex=null):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(actor!=command.Source.ActorUserId)throw new UnauthorizedAccessException();
            if(type==typeof(JOURNAL_ENTRY) && table=="JOURNAL_ENTRY" && entity is JOURNAL_ENTRY header)
            {
                if(retained is null && previous is null && operation==PPDMRowMutation.Insert){header.PREPARATION_WORKFLOW_JSON=CompanySalesJournalPreparationProof.Retain(command,header);_=CompanySalesJournalPreparationProof.Require(header);return;}
                if(retained is not null && previous is JOURNAL_ENTRY old && operation==PPDMRowMutation.Update && JsonSerializer.Serialize(old)==JsonSerializer.Serialize(retained)){header.PREPARATION_WORKFLOW_JSON=CompanySalesJournalPreparationProof.Draft(CompanySalesJournalPreparationProof.Require(retained),header,lines);_=CompanySalesJournalPreparationProof.Require(header);return;}
            }
            if(type==typeof(JOURNAL_ENTRY_LINE) && table=="JOURNAL_ENTRY_LINE" && entity is JOURNAL_ENTRY_LINE row && lineIndex is {} index && retained is not null && previous is null && operation==PPDMRowMutation.Insert)
            {
                CompanySalesJournalPreparationProof.RequireLineInsertion(command,retained,row,index);return;
            }
            throw new InvalidOperationException("Only original request retention, exact line insertion and REQUESTED-to-DRAFT transition are authorized.");
        }
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(retained is null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY")throw new InvalidOperationException("This journal mutation grants no read.");return [new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=command.JournalId},new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Source.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Source.Company.BusinessAssociateId}];}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>[await GetReadConstraintsAsync(type,table)];
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(retained is null || type!=typeof(JOURNAL_ENTRY) || table!="JOURNAL_ENTRY" || rows.Count!=1 || rows[0] is not JOURNAL_ENTRY row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The exact retained journal predecessor is required.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Journal preparation grants no general read access.");
    }
}
