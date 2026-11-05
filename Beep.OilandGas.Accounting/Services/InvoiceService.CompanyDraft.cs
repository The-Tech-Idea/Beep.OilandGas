using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class InvoiceService
{
    private static void RequireCompanyInvoiceSchema(IDataSource source)
    {
        var schema=source.GetEntityStructure(new EntityStructure{EntityName="INVOICE",DatasourceEntityName="INVOICE"},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!="INVOICE_ID" || keys[0].AllowDBNull || keys[0].Fieldtype!=typeof(string).FullName || Math.Max(keys[0].Size,keys[0].Size1)<128)
            throw new InvalidOperationException("Review the canonical invoice key through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{("OWNER_TENANT_ID",128),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("SOURCE_RUN_TICKET_ID",128),("BILLING_SOURCE_KEY",64),("BILLING_WORKFLOW_JSON",CompanySalesInvoiceDraftProof.MaximumLength)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || !fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("Review complete nullable bounded invoice source/proof metadata through the PRODUCTION Beep migration.");
        }
        foreach(var (name,type) in new[]{("INVOICE_NUMBER",typeof(string)),("CUSTOMER_BA_ID",typeof(string)),("INVOICE_DATE",typeof(DateTime)),("DUE_DATE",typeof(DateTime)),("SUBTOTAL",typeof(decimal)),
            ("TAX_AMOUNT",typeof(decimal)),("TOTAL_AMOUNT",typeof(decimal)),("PAID_AMOUNT",typeof(decimal)),("BALANCE_DUE",typeof(decimal)),("CURRENCY_CODE",typeof(string)),("STATUS",typeof(string)),("DESCRIPTION",typeof(string))})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=type.FullName)throw new InvalidOperationException("The invoice proposal's native business columns are unconfirmed.");
        }
        foreach(var (name,size) in new[]{("INVOICE_NUMBER",128),("CUSTOMER_BA_ID",128),("CURRENCY_CODE",16),("STATUS",32),("DESCRIPTION",1000)})
        {
            var field=schema!.Fields.Single(value=>value.FieldName==name);
            if(Math.Max(field.Size,field.Size1)<size)throw new InvalidOperationException("Review the bounded invoice identity, currency, status and reason columns through the PRODUCTION Beep migration.");
        }
    }
    private static void RequireInvoiceOutcomeSchema(IDataSource source)
    {
        const string table="SALES_INVOICE_OUTCOME";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the invoice outcome receipt key through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("INVOICE_ID",32),("INTENT_REVISION",64),("INVOICE_REVISION",64),("RECORDED_REVISION",64),("INVOICE_JSON",SALES_INVOICE_OUTCOME.MaximumInvoiceLength)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)
                throw new InvalidOperationException("The complete bounded invoice outcome receipt schema is unconfirmed.");
        }
    }
    private async Task<SALES_INVOICE_OUTCOME?> ReadInvoiceOutcomeReceipt(string intentId,string connection,IDataSource source,Func<Task> guard)
    {
        await guard();RequireInvoiceOutcomeSchema(source);var repo=await GetRepoAsync<SALES_INVOICE_OUTCOME>("SALES_INVOICE_OUTCOME",connection);
        async Task<SALES_INVOICE_OUTCOME?> Read()
        {
            var raw=(await repo.GetAsync([new(){FieldName="SALES_INVOICE_OUTCOME_ID",Operator="=",FilterValue=intentId}]))?.ToArray();await guard();
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_INVOICE_OUTCOME))throw new InvalidOperationException("The unique original invoice outcome receipt is unconfirmed.");
            if(raw.SingleOrDefault() is not SALES_INVOICE_OUTCOME row)return null;
            if(row.SALES_INVOICE_OUTCOME_ID!=intentId)throw new InvalidOperationException("The provider returned another invoice outcome identity.");return CompanyInventorySourceReview.Copy(row);
        }
        var initial=await Read();var closing=await Read();
        if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original invoice outcome receipt changed during read-back.");return closing;
    }

    public async Task<IReadOnlyList<INVOICE>> GetCompanySalesInvoicesForDeliveryAsync(CompanyScope company,string field,string ticket,Func<Task> requireAuthority)
    {
        PurchaseOrderOwnership.RequireScope(company);RequireInvoiceSourceId(field);RequireInvoiceSourceId(ticket);ArgumentNullException.ThrowIfNull(requireAuthority);
        await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard()
        {
            await requireAuthority();var current=await InvoiceIntentBinding();
            if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice source binding changed.");
        }
        async Task<INVOICE[]> Read()
        {
            await Guard();RequireCompanyInvoiceSchema(binding.Source);var repo=await GetRepoAsync<INVOICE>("INVOICE",binding.Connection);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="SOURCE_RUN_TICKET_ID",Operator="=",FilterValue=ticket}]))?.ToArray();
            await Guard();if(raw is null || raw.Length>CompanySalesInvoiceDraftReview.MaximumInvoices || raw.Any(row=>row is not INVOICE))throw new InvalidOperationException("Complete bounded native invoice contributors are unconfirmed.");
            var rows=raw.Cast<INVOICE>().OrderBy(row=>row.INVOICE_ID,StringComparer.Ordinal).ToArray();
            if(rows.Select(row=>row.INVOICE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Length)throw new InvalidOperationException("Native invoice contributors contain duplicate identities.");
            foreach(var row in rows)
            {
                var contributor=CompanySalesInvoiceContributor.Create(company,row);contributor.Require(company,field,ticket);
                if(row.FIELD_ID!=field || row.SOURCE_RUN_TICKET_ID!=ticket)throw new InvalidOperationException("The provider returned another invoice source.");
            }
            return rows.Select(CompanyInventorySourceReview.Copy).ToArray();
        }
        var first=await Read();var closing=await Read();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete native invoice history changed during read-back.");
        await Guard();return closing;
    }

    public async Task<CompanySalesInvoiceDraftOutcome?> GetCompanyInvoiceDraftOutcomeAsync(CompanyScope company,string actor,string intentId,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        ArgumentNullException.ThrowIfNull(requireAuthority);await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard()
        {
            await requireAuthority();var current=await InvoiceIntentBinding();
            if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice outcome binding changed.");
        }
        var intent=await GetCompanyInvoiceDraftIntentAsync(company,actor,intentId,Guard);
        var receipt=await ReadInvoiceOutcomeReceipt(intentId,binding.Connection,binding.Source,Guard);
        if(intent is null){if(receipt is not null)throw new InvalidOperationException("An invoice outcome exists without its original command journal.");await Guard();return null;}
        var command=intent.Restore(company,actor);var rows=await GetCompanySalesInvoicesForDeliveryAsync(company,command.Review.FieldId,command.Review.TicketId,Guard);
        if(rows.Count==0 && receipt is null){await Guard();return null;}
        if(rows.Count!=1 || receipt is null)throw new InvalidOperationException("The original invoice and durable outcome receipt are missing or ambiguous. Reconcile; do not recreate the draft.");
        var outcome=new CompanySalesInvoiceDraftOutcome(intent,rows[0],receipt);outcome.Require(command);
        SALES_INVOICE_REVIEW_INTENT? reviewerIntent=null;
        if(outcome.Invoice.REVIEW_WORKFLOW_JSON is not null)
        {
            reviewerIntent=await ReadInvoiceReviewIntent(company,outcome.Invoice.INVOICE_ID,binding.Connection,binding.Source,Guard);
            if(reviewerIntent is null || JsonSerializer.Serialize(reviewerIntent)!=JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(company,outcome.Invoice).Intent))throw new InvalidOperationException("The native disposition's original reviewer journal is missing or changed.");
        }
        var closing=await GetCompanyInvoiceDraftIntentAsync(company,actor,intentId,Guard);
        var closingReceipt=await ReadInvoiceOutcomeReceipt(intentId,binding.Connection,binding.Source,Guard);
        var closingRows=await GetCompanySalesInvoicesForDeliveryAsync(company,command.Review.FieldId,command.Review.TicketId,Guard);
        if(reviewerIntent is not null && JsonSerializer.Serialize(reviewerIntent)!=JsonSerializer.Serialize(await ReadInvoiceReviewIntent(company,outcome.Invoice.INVOICE_ID,binding.Connection,binding.Source,Guard)))throw new InvalidOperationException("The original reviewer journal changed during invoice outcome capture.");
        if(JsonSerializer.Serialize(intent)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(receipt)!=JsonSerializer.Serialize(closingReceipt) || JsonSerializer.Serialize(rows)!=JsonSerializer.Serialize(closingRows))
            throw new InvalidOperationException("The complete invoice journal, receipt or draft changed during outcome read-back.");
        await Guard();return CompanyInventorySourceReview.Copy(outcome);
    }

    public async Task<CompanySalesInvoiceDraftOutcome> ExecuteCompanyInvoiceDraftAsync(CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<CompanySalesInvoiceDraftReview>> readCurrent)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);
        command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(bool owned)
        {
            await requireAuthority();var current=await InvoiceIntentBinding();
            if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice execution binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(binding.Source,IsolationLevel.Serializable);
        }
        var known=await GetCompanyInvoiceDraftOutcomeAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(false));
        if(known is not null){known.Require(command);await Guard(false);return known;}
        // Original command retention commits separately and survives a later draft-write failure.
        var intent=await SaveCompanyInvoiceDraftIntentAsync(command,()=>Guard(false),readCurrent);await Guard(false);
        CompanySalesInvoiceDraftOutcome? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>
        {
            await Guard(true);RequireCompanyInvoiceSchema(binding.Source);
            var retained=await GetCompanyInvoiceDraftIntentAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(true));
            if(JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The original durable invoice command changed before execution.");
            var rows=await GetCompanySalesInvoicesForDeliveryAsync(command.Review.Company,command.Review.FieldId,command.Review.TicketId,()=>Guard(true));
            if(rows.Count>0)
            {
                recorded=await GetCompanyInvoiceDraftOutcomeAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(true));
                if(recorded is null)throw new InvalidOperationException("The original native invoice outcome receipt is unconfirmed.");recorded.Require(command);
            }
            else
            {
                var current=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(current);await Guard(true);
                var repo=await GetRepoAsync<INVOICE>("INVOICE",binding.Connection);
                foreach(var filters in new List<AppFilter>[]
                {
                    [new(){FieldName="INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}],
                    [new(){FieldName="PPDM_GUID",Operator="=",FilterValue=command.IntentId}],
                    [new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Review.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Review.Company.BusinessAssociateId},new(){FieldName="INVOICE_NUMBER",Operator="=",FilterValue=command.Facts.InvoiceNumber}]
                })
                {
                    var collision=(await repo.GetAsync(filters))?.ToArray();await Guard(true);
                    if(collision is null || collision.Length!=0)throw new InvalidOperationException("The original invoice identity, intent or company invoice number is already used or unconfirmed.");
                }
                var invoice=CompanySalesInvoiceDraftProof.Prepare(command,DateTime.UtcNow);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVOICE),binding.Connection,"INVOICE",rowAccess:new CompanyInvoiceInsertPolicy(invoice,intent,()=>Guard(true)));
                await writer.InsertAsync(invoice,command.Review.ActorUserId);await Guard(true);
                var inserted=await GetCompanySalesInvoicesForDeliveryAsync(command.Review.Company,command.Review.FieldId,command.Review.TicketId,()=>Guard(true));
                if(inserted.Count!=1 || JsonSerializer.Serialize(inserted[0])!=JsonSerializer.Serialize(invoice))throw new InvalidOperationException("The saved native invoice differs from its exact original insertion.");
                if(await ReadInvoiceOutcomeReceipt(command.IntentId,binding.Connection,binding.Source,()=>Guard(true)) is not null)throw new InvalidOperationException("Another invoice outcome receipt already uses the original identity.");
                var receipt=SALES_INVOICE_OUTCOME.Create(intent,invoice,DateTime.UtcNow);
                var receiptWriter=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_INVOICE_OUTCOME),binding.Connection,"SALES_INVOICE_OUTCOME",rowAccess:new InvoiceOutcomeInsertPolicy(receipt,intent,()=>Guard(true)));
                await receiptWriter.InsertAsync(receipt,command.Review.ActorUserId);await Guard(true);
                var saved=await GetCompanyInvoiceDraftOutcomeAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(true));
                if(saved is null || JsonSerializer.Serialize(saved.Invoice)!=JsonSerializer.Serialize(invoice) || JsonSerializer.Serialize(saved.Receipt)!=JsonSerializer.Serialize(receipt))throw new InvalidOperationException("The saved native invoice or receipt differs from its exact draft read-back.");
                var closing=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireAfterInsertion(closing,saved.Invoice);await Guard(true);
                recorded=saved;
            }
            await Guard(true);
        });
        await Guard(false);var durable=await GetCompanyInvoiceDraftOutcomeAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(false));
        if(recorded is null || durable is null || JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(durable))throw new InvalidOperationException("Invoice draft commit is uncertain. Reconcile the original intent before another proposal.");
        durable.Require(command);await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }

    private static void RequireInvoiceSourceId(string value)
    {if(string.IsNullOrWhiteSpace(value) || value.Length>128 || value.Any(char.IsControl))throw new InvalidOperationException("A bounded original invoice field and delivery identity are required.");}

    private sealed class CompanyInvoiceInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly INVOICE expected;
        private readonly SALES_INVOICE_INTENT intent;
        private readonly Func<Task> guard;
        public CompanyInvoiceInsertPolicy(INVOICE original,SALES_INVOICE_INTENT retained,Func<Task> requireAuthority)
        {expected=CompanyInventorySourceReview.Copy(original);intent=CompanyInventorySourceReview.Copy(retained);guard=requireAuthority;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(INVOICE) || table!="INVOICE" || operation!=PPDMRowMutation.Insert || previous is not null || entity is not INVOICE row || actor!=expected.ROW_CREATED_BY)
                throw new InvalidOperationException("Only the original owned invoice draft insertion is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;
            normalized.ROW_EFFECTIVE_DATE=expected.ROW_EFFECTIVE_DATE;normalized.BILLING_WORKFLOW_JSON=expected.BILLING_WORKFLOW_JSON;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(expected) || row.ROW_CREATED_DATE is not {} created || created.Kind!=DateTimeKind.Utc || created<expected.ROW_CREATED_DATE)
                throw new InvalidOperationException("The original invoice proposal or creation audit changed during repository preparation.");
            var company=new CompanyScope(expected.OWNER_TENANT_ID!,expected.OWNER_COMPANY_BA_ID!);var original=CompanySalesInvoiceDraftProof.Require(company,expected).Record.Command;
            row.BILLING_WORKFLOW_JSON=CompanySalesInvoiceDraftProof.Create(new(original,created),row);
            var retained=intent.Restore(company,actor!);var actual=CompanySalesInvoiceDraftProof.Require(company,row);
            if(JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(original) || JsonSerializer.Serialize(actual.Record.Command)!=JsonSerializer.Serialize(original) || row.ROW_CREATED_DATE<intent.ROW_CREATED_DATE)
                throw new InvalidOperationException("The invoice insertion differs from its original retained command.");
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Invoice insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Invoice insertion cannot grant reads.");
    }
    private sealed class InvoiceOutcomeInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly SALES_INVOICE_OUTCOME expected;
        private readonly SALES_INVOICE_INTENT intent;
        private readonly Func<Task> guard;
        public InvoiceOutcomeInsertPolicy(SALES_INVOICE_OUTCOME original,SALES_INVOICE_INTENT retained,Func<Task> requireAuthority)
        {expected=CompanyInventorySourceReview.Copy(original);intent=CompanyInventorySourceReview.Copy(retained);guard=requireAuthority;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(SALES_INVOICE_OUTCOME) || table!="SALES_INVOICE_OUTCOME" || operation!=PPDMRowMutation.Insert || previous is not null || entity is not SALES_INVOICE_OUTCOME row || actor!=expected.ROW_CREATED_BY)
                throw new InvalidOperationException("Only the original append-only invoice outcome receipt is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=expected.ROW_EFFECTIVE_DATE;normalized.RECORDED_REVISION=expected.RECORDED_REVISION;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(expected) || row.ROW_CREATED_DATE<expected.ROW_CREATED_DATE)throw new InvalidOperationException("The original invoice receipt changed during repository preparation.");
            row.RECORDED_REVISION=SALES_INVOICE_OUTCOME.Revision(row);_=row.Restore(intent);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice receipt insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice receipt insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Invoice receipt insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Invoice receipt insertion cannot grant reads.");
    }
}
