using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class ARService
{
    private static void ReceivableId(string value)
    {if(string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl))throw new InvalidOperationException("A bounded original receivable identity is required.");}
    private async Task<(string Connection,IDataSource Source)> ReceivableBinding()
    {if(_resolveConnection is null)throw new InvalidOperationException("A request-bound company PRODUCTION receivable resolver is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original receivable binding is unavailable.");return(connection,_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original receivable datasource is unavailable."));}
    private static void ReceivableSchema(IDataSource source,bool journal)
    {
        var table=journal?"SALES_RECEIVABLE_INTENT":"AR_INVOICE";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<(journal?32:128))throw new InvalidOperationException("Review the canonical receivable key through the PRODUCTION Beep migration.");
        (string Name,int Size,bool? Nullable)[] columns=journal?[("OWNER_TENANT_ID",64,false),("OWNER_COMPANY_BA_ID",128,false),("FIELD_ID",128,false),("SOURCE_INVOICE_ID",128,false),("AR_INVOICE_ID",32,false),("COMMAND_JSON",SALES_RECEIVABLE_INTENT.MaximumCommandLength,false),("RECORDED_REVISION",64,false)]:
            [("OWNER_TENANT_ID",128,true),("OWNER_COMPANY_BA_ID",128,true),("FIELD_ID",128,true),("SOURCE_INVOICE_ID",128,true),("CURRENCY_CODE",16,true),("PREPARATION_WORKFLOW_JSON",CompanySalesReceivableProof.MaximumLength,true),("REVIEW_WORKFLOW_JSON",CompanySalesReceivableDecisionProof.MaximumLength,true),("INVOICE_NUMBER",128,null),("CUSTOMER_BA_ID",128,null),("STATUS",32,null)];
        foreach(var (name,size,nullable) in columns)
        {var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || nullable is {} required && fields[0].AllowDBNull!=required || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("The complete bounded receivable company/source/proof metadata is unconfirmed.");}
        if(!journal)foreach(var (name,type) in new[]{("INVOICE_DATE",typeof(DateTime)),("DUE_DATE",typeof(DateTime)),("TOTAL_AMOUNT",typeof(decimal)),("PAID_AMOUNT",typeof(decimal)),("BALANCE_DUE",typeof(decimal))})
        {var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName)throw new InvalidOperationException("The exact receivable business column types are unconfirmed.");}
    }
    private PPDMGenericRepository OwnedReceivableRepo<T>(string connection,string table,IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connection,table,rowAccess:policy);
    private static void ReceivableReceiptSchema(IDataSource source)
    {
        const string table="SALES_RECEIVABLE_OUTCOME";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the canonical receivable receipt key through Beep migration.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("SOURCE_INVOICE_ID",128),("AR_INVOICE_ID",32),("INTENT_REVISION",64),("RECEIVABLE_JSON",SALES_RECEIVABLE_OUTCOME.MaximumSnapshotLength),("RECORDED_REVISION",64)})
        {var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("The complete bounded receivable outcome receipt metadata is unconfirmed.");}
    }
    private async Task<SALES_RECEIVABLE_OUTCOME?> ReceivableReceipt(CompanyScope company,string invoice,string connection,IDataSource source,Func<Task> guard)
    {
        async Task<SALES_RECEIVABLE_OUTCOME?> Read()
        {
            await guard();ReceivableReceiptSchema(source);var rows=(await OwnedReceivableRepo<SALES_RECEIVABLE_OUTCOME>(connection,"SALES_RECEIVABLE_OUTCOME").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="SOURCE_INVOICE_ID",Operator="=",FilterValue=invoice}]))?.ToArray();await guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not SALES_RECEIVABLE_OUTCOME))throw new InvalidOperationException("The unique original receivable receipt is unconfirmed.");
            if(rows.SingleOrDefault() is not SALES_RECEIVABLE_OUTCOME row)return null;
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.SOURCE_INVOICE_ID!=invoice)throw new InvalidOperationException("The provider returned another receivable receipt scope.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original receivable receipt changed.");return closing;
    }

    public async Task<IReadOnlyList<AR_INVOICE>> GetCompanySalesReceivablesAsync(CompanyScope company,string field,string invoiceId,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);ReceivableId(field);ReceivableId(invoiceId);ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();var binding=await ReceivableBinding();
        async Task Guard(){await requireAuthority();var current=await ReceivableBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original company receivable binding changed.");}
        async Task<AR_INVOICE[]> Read()
        {
            await Guard();ReceivableSchema(binding.Source,false);var raw=(await OwnedReceivableRepo<AR_INVOICE>(binding.Connection,"AR_INVOICE").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="SOURCE_INVOICE_ID",Operator="=",FilterValue=invoiceId}]))?.ToArray();await Guard();
            if(raw is null || raw.Length>CompanySalesReceivableSource.MaximumReceivables || raw.Any(row=>row is not AR_INVOICE))throw new InvalidOperationException("Complete bounded native receivable contributors are unconfirmed.");var rows=raw.Cast<AR_INVOICE>().OrderBy(row=>row.AR_INVOICE_ID,StringComparer.Ordinal).ToArray();
            if(rows.Select(row=>row.AR_INVOICE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Duplicate native receivable identities are unconfirmed.");
            foreach(var row in rows)
            {
                var proof=CompanySalesReceivableProof.Require(company,row);if(row.FIELD_ID!=field || row.SOURCE_INVOICE_ID!=invoiceId)throw new InvalidOperationException("The provider returned another receivable source.");
                var original=await GetCompanySalesReceivableIntentAsync(company,row.ROW_CREATED_BY,invoiceId,Guard);var receipt=await ReceivableReceipt(company,invoiceId,binding.Connection,binding.Source,Guard);
                if(original is null || receipt is null || JsonSerializer.Serialize(original.Restore(company,row.ROW_CREATED_BY))!=JsonSerializer.Serialize(proof.Record.Command) || JsonSerializer.Serialize(receipt.Restore(original))!=JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.OriginalDraft(company,row)))throw new InvalidOperationException("The native receivable's original journal or outcome receipt is missing or changed.");
                await RequireReceivableDecisionJournal(company,row,binding.Connection,binding.Source,Guard);
            }
            return rows.Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        var first=await Read();var closing=await Read();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete native receivable source changed during read-back.");return closing;
    }
    public async Task<SALES_RECEIVABLE_INTENT?> GetCompanySalesReceivableIntentAsync(CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);ReceivableId(actor);ReceivableId(invoiceId);ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();var binding=await ReceivableBinding();
        async Task Guard(){await requireAuthority();var current=await ReceivableBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original receivable journal binding changed.");}
        async Task<SALES_RECEIVABLE_INTENT?> Read()
        {
            await Guard();ReceivableSchema(binding.Source,true);
            var raw=(await OwnedReceivableRepo<SALES_RECEIVABLE_INTENT>(binding.Connection,"SALES_RECEIVABLE_INTENT").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="SOURCE_INVOICE_ID",Operator="=",FilterValue=invoiceId}]))?.ToArray();await Guard();
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_RECEIVABLE_INTENT))throw new InvalidOperationException("One exact retained source receivable intent is required.");if(raw.SingleOrDefault() is not SALES_RECEIVABLE_INTENT row)return null;
            // A competing source reservation is refused; its original private command is not returned to another account.
            var original=row.Restore(company,actor);if(original.Source.InvoiceId!=invoiceId)throw new InvalidOperationException("The provider returned another receivable journal source.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var closing=await Read();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original receivable intent changed.");return closing;
    }
    public async Task<CompanySalesReceivableOutcome?> GetCompanySalesReceivableOutcomeAsync(CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();var binding=await ReceivableBinding();
        async Task Guard(){await requireAuthority();var current=await ReceivableBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original receivable outcome binding changed.");}
        var intent=await GetCompanySalesReceivableIntentAsync(company,actor,invoiceId,Guard);
        var receipt=await ReceivableReceipt(company,invoiceId,binding.Connection,binding.Source,Guard);
        if(intent is null)
        {
            ReceivableSchema(binding.Source,false);var orphaned=(await OwnedReceivableRepo<AR_INVOICE>(binding.Connection,"AR_INVOICE").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="SOURCE_INVOICE_ID",Operator="=",FilterValue=invoiceId}]))?.ToArray();await Guard();
            if(receipt is not null || orphaned is null || orphaned.Length!=0)throw new InvalidOperationException("A native receivable or receipt exists without its original journal. Reconcile; do not recreate.");return null;
        }
        var command=intent.Restore(company,actor);var rows=await GetCompanySalesReceivablesAsync(company,command.Source.FieldId,invoiceId,Guard);
        if(rows.Count==0 && receipt is null){await Guard();return null;}
        if(rows.Count!=1 || receipt is null)throw new InvalidOperationException("The original receivable draft or outcome receipt is missing or ambiguous. Reconcile; do not recreate.");var result=new CompanySalesReceivableOutcome(intent,rows[0],receipt);result.Require(command);
        var closing=await GetCompanySalesReceivableIntentAsync(company,actor,invoiceId,Guard);var closingReceipt=await ReceivableReceipt(company,invoiceId,binding.Connection,binding.Source,Guard);await Guard();
        if(JsonSerializer.Serialize(intent)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(receipt)!=JsonSerializer.Serialize(closingReceipt))throw new InvalidOperationException("The original receivable journal/receipt changed during outcome read-back.");return result;
    }
    public async Task<CompanySalesReceivableOutcome> PrepareCompanySalesReceivableAsync(CompanySalesReceivableCommand command,Func<Task> requireAuthority,Func<Task<CompanySalesReceivableSource>> readCurrent)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();var binding=await ReceivableBinding();
        async Task Guard(bool owned)
        {await requireAuthority();var current=await ReceivableBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original receivable execution binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(binding.Source,IsolationLevel.Serializable);}
        var known=await GetCompanySalesReceivableOutcomeAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(false));if(known is not null){known.Require(command);await Guard(false);return known;}
        SALES_RECEIVABLE_INTENT? retained=null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>{
            await Guard(true);ReceivableSchema(binding.Source,true);ReceivableSchema(binding.Source,false);ReceivableReceiptSchema(binding.Source);
            retained=await GetCompanySalesReceivableIntentAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(true));
            if(retained is not null){if(JsonSerializer.Serialize(retained.Restore(command.Source.Company,command.Source.ActorUserId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Another original receivable command already reserves this invoice.");return;}
            command.RequireCurrent(await readCurrent());await Guard(true);
            await RefuseGlobalIdentity<SALES_RECEIVABLE_INTENT>("SALES_RECEIVABLE_INTENT","SALES_RECEIVABLE_INTENT_ID",command.IntentId,binding.Connection,()=>Guard(true));
            var entry=SALES_RECEIVABLE_INTENT.Create(command,DateTime.UtcNow);
            await OwnedReceivableRepo<SALES_RECEIVABLE_INTENT>(binding.Connection,"SALES_RECEIVABLE_INTENT",new ReceivableInsertPolicy(entry,null,()=>Guard(true))).InsertAsync(entry,command.Source.ActorUserId);
            retained=await GetCompanySalesReceivableIntentAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(true));
            if(retained is null || JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The retained receivable intent differs from its exact stored command.");command.RequireCurrent(await readCurrent());await Guard(true);
        });
        await Guard(false);var durableIntent=await GetCompanySalesReceivableIntentAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(false));
        if(retained is null || durableIntent is null || JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(durableIntent))throw new InvalidOperationException("Original receivable intent commit is uncertain; reconcile that command.");
        CompanySalesReceivableOutcome? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>{
            await Guard(true);ReceivableSchema(binding.Source,true);ReceivableSchema(binding.Source,false);ReceivableReceiptSchema(binding.Source);
            var existing=await GetCompanySalesReceivableOutcomeAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(true));if(existing is not null){existing.Require(command);recorded=existing;return;}
            command.RequireCurrent(await readCurrent());await Guard(true);
            var currentIntent=await GetCompanySalesReceivableIntentAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(true));
            if(JsonSerializer.Serialize(currentIntent)!=JsonSerializer.Serialize(durableIntent))throw new InvalidOperationException("The original receivable command journal changed before draft insertion.");
            await RefuseGlobalIdentity<AR_INVOICE>("AR_INVOICE","AR_INVOICE_ID",command.ReceivableId,binding.Connection,()=>Guard(true));
            var numbers=(await OwnedReceivableRepo<AR_INVOICE>(binding.Connection,"AR_INVOICE").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Source.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Source.Company.BusinessAssociateId},new(){FieldName="INVOICE_NUMBER",Operator="=",FilterValue=command.Number}]))?.ToArray();await Guard(true);
            if(numbers is null || numbers.Length!=0)throw new InvalidOperationException("The selected company's original receivable number is unavailable or already recorded.");
            var draft=CompanySalesReceivableProof.Prepare(command,DateTime.UtcNow);
            await OwnedReceivableRepo<AR_INVOICE>(binding.Connection,"AR_INVOICE",new ReceivableInsertPolicy(null,draft,()=>Guard(true))).InsertAsync(draft,command.Source.ActorUserId);
            var receipt=SALES_RECEIVABLE_OUTCOME.Create(currentIntent!,draft,DateTime.UtcNow);
            await RefuseGlobalIdentity<SALES_RECEIVABLE_OUTCOME>("SALES_RECEIVABLE_OUTCOME","SALES_RECEIVABLE_OUTCOME_ID",command.IntentId,binding.Connection,()=>Guard(true));
            await OwnedReceivableRepo<SALES_RECEIVABLE_OUTCOME>(binding.Connection,"SALES_RECEIVABLE_OUTCOME",new ReceivableReceiptInsertPolicy(receipt,currentIntent!,()=>Guard(true))).InsertAsync(receipt,command.Source.ActorUserId);
            recorded=await GetCompanySalesReceivableOutcomeAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(true));
            if(recorded is null || JsonSerializer.Serialize(recorded.Receivable)!=JsonSerializer.Serialize(draft) || JsonSerializer.Serialize(recorded.Receipt)!=JsonSerializer.Serialize(receipt))throw new InvalidOperationException("The complete stored receivable draft/receipt differs from its original insertion.");
            command.RequireAfterInsertion(await readCurrent(),draft);await Guard(true);
        });
        await Guard(false);var durable=await GetCompanySalesReceivableOutcomeAsync(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId,()=>Guard(false));
        if(recorded is null || durable is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Receivable draft commit is uncertain; reconcile the original intent before another proposal.");durable.Require(command);await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }
    private async Task RefuseGlobalIdentity<T>(string table,string key,string id,string connection,Func<Task> guard)
    {await guard();var rows=(await OwnedReceivableRepo<T>(connection,table).GetAsync([new(){FieldName=key,Operator="=",FilterValue=id}]))?.ToArray();await guard();if(rows is null || rows.Length!=0)throw new InvalidOperationException("The original global receivable identity is unavailable or occupied.");}

    private sealed class ReceivableInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly SALES_RECEIVABLE_INTENT? intent;
        private readonly AR_INVOICE? invoice;
        private readonly Func<Task> guard;
        public ReceivableInsertPolicy(SALES_RECEIVABLE_INTENT? intent,AR_INVOICE? invoice,Func<Task> guard)
        {this.intent=CompanyInventorySourceReview.Copy(intent);this.invoice=CompanyInventorySourceReview.Copy(invoice);this.guard=guard;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(operation!=PPDMRowMutation.Insert || previous is not null)throw new InvalidOperationException("Only the original receivable insertion is authorized.");
            if(intent is not null)
            {
                if(type!=typeof(SALES_RECEIVABLE_INTENT) || table!="SALES_RECEIVABLE_INTENT" || entity is not SALES_RECEIVABLE_INTENT row || actor!=intent.ROW_CREATED_BY)throw new InvalidOperationException("Only the original receivable journal is authorized.");
                var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=intent.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=intent.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=intent.ROW_EFFECTIVE_DATE;normalized.RECORDED_REVISION=intent.RECORDED_REVISION;
                if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(intent) || row.ROW_CREATED_DATE<intent.ROW_CREATED_DATE)throw new InvalidOperationException("The original receivable journal changed during repository preparation.");
                row.RECORDED_REVISION=SALES_RECEIVABLE_INTENT.Revision(row);_=row.Restore(new(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID),actor!);
            }
            else
            {
                if(invoice is null || type!=typeof(AR_INVOICE) || table!="AR_INVOICE" || entity is not AR_INVOICE row || actor!=invoice.ROW_CREATED_BY || row.PREPARATION_WORKFLOW_JSON!=invoice.PREPARATION_WORKFLOW_JSON)throw new InvalidOperationException("Only the original receivable draft is authorized.");
                var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=invoice.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=invoice.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=invoice.ROW_EFFECTIVE_DATE;
                if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(invoice) || row.ROW_CREATED_DATE is not {} created || created.Kind!=DateTimeKind.Utc || created<invoice.ROW_CREATED_DATE)throw new InvalidOperationException("The original receivable proposal changed during repository preparation.");
                var company=new CompanyScope(invoice.OWNER_TENANT_ID!,invoice.OWNER_COMPANY_BA_ID!);var command=CompanySalesReceivableProof.Require(company,invoice).Record.Command;
                row.PREPARATION_WORKFLOW_JSON=CompanySalesReceivableProof.Create(new(command,created),row);_=CompanySalesReceivableProof.Require(company,row);
            }
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Receivable insertion does not grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Receivable insertion does not grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Receivable insertion does not grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Receivable insertion does not grant reads.");
    }
    private sealed class ReceivableReceiptInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly SALES_RECEIVABLE_OUTCOME expected;
        private readonly SALES_RECEIVABLE_INTENT intent;
        private readonly Func<Task> guard;
        public ReceivableReceiptInsertPolicy(SALES_RECEIVABLE_OUTCOME expected,SALES_RECEIVABLE_INTENT intent,Func<Task> guard)
        {this.expected=CompanyInventorySourceReview.Copy(expected);this.intent=CompanyInventorySourceReview.Copy(intent);this.guard=guard;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(SALES_RECEIVABLE_OUTCOME) || table!="SALES_RECEIVABLE_OUTCOME" || entity is not SALES_RECEIVABLE_OUTCOME row || operation!=PPDMRowMutation.Insert || previous is not null || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only the original receivable outcome receipt insertion is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=expected.ROW_EFFECTIVE_DATE;normalized.RECORDED_REVISION=expected.RECORDED_REVISION;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(expected) || row.ROW_CREATED_DATE<expected.ROW_CREATED_DATE)throw new InvalidOperationException("The original receivable outcome changed during repository preparation.");row.RECORDED_REVISION=SALES_RECEIVABLE_OUTCOME.Revision(row);_=row.Restore(intent);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Receipt insertion does not grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Receipt insertion does not grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Receipt insertion does not grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Receipt insertion does not grant reads.");
    }
}
