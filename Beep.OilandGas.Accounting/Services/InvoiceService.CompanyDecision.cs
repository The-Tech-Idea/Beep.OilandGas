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
    private static void RequireInvoiceReviewSchema(IDataSource source)
    {
        var invoice=source.GetEntityStructure(new EntityStructure{EntityName="INVOICE",DatasourceEntityName="INVOICE"},true);
        var proof=invoice?.Fields?.Where(field=>field.FieldName=="REVIEW_WORKFLOW_JSON").ToArray();
        if(proof is not {Length:1} || !proof[0].AllowDBNull || proof[0].Fieldtype!=typeof(string).FullName || Math.Max(proof[0].Size,proof[0].Size1)<CompanySalesInvoiceDecisionProof.MaximumLength)
            throw new InvalidOperationException("Review the nullable bounded native invoice disposition column through the PRODUCTION Beep migration.");
        const string table="SALES_INVOICE_REVIEW_INTENT";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("The original reviewer journal key is unconfirmed.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("INVOICE_ID",32),("SOURCE_REVISION",64),("COMMAND_JSON",SALES_INVOICE_REVIEW_INTENT.MaximumCommandLength),("RECORDED_REVISION",64)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();
            if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Review complete bounded reviewer journal metadata through the PRODUCTION Beep migration.");
        }
    }
    private async Task<SALES_INVOICE_REVIEW_INTENT?> ReadInvoiceReviewIntent(CompanyScope company,string invoiceId,string connection,IDataSource source,Func<Task> guard)
    {
        await guard();RequireInvoiceReviewSchema(source);var repo=await GetRepoAsync<SALES_INVOICE_REVIEW_INTENT>("SALES_INVOICE_REVIEW_INTENT",connection);
        var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="INVOICE_ID",Operator="=",FilterValue=invoiceId}]))?.ToArray();await guard();
        if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_INVOICE_REVIEW_INTENT))throw new InvalidOperationException("The unique original reviewer reservation is unconfirmed.");
        if(raw.SingleOrDefault() is not SALES_INVOICE_REVIEW_INTENT intent)return null;
        _=intent.Restore(company,intent.ROW_CREATED_BY);if(intent.INVOICE_ID!=invoiceId)throw new InvalidOperationException("The provider returned another invoice's reviewer command.");return CompanyInventorySourceReview.Copy(intent);
    }
    public async Task<SALES_INVOICE_REVIEW_INTENT?> GetCompanyInvoiceReviewIntentAsync(CompanyScope company,string actor,string invoiceId,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess)
    {
        PurchaseOrderOwnership.RequireScope(company);RequireInvoiceSourceId(actor);RequireInvoiceSourceId(invoiceId);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldAccess);
        await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(){await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original reviewer binding changed.");}
        async Task<SALES_INVOICE_REVIEW_INTENT?> Capture()
        {
            // Discover and authorize FIELD before returning any private retained decision body.
            var invoice=await GetCompanyInvoiceDraftForReviewAsync(company,invoiceId,Guard,requireFieldAccess);await Guard();
            var intent=await ReadInvoiceReviewIntent(company,invoiceId,binding.Connection,binding.Source,Guard);
            if(intent is null){if(invoice.Invoice.REVIEW_WORKFLOW_JSON is not null)throw new InvalidOperationException("A native disposition exists without its original reviewer journal.");return null;}
            if(invoice.Invoice.REVIEW_WORKFLOW_JSON is not null && JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(company,invoice.Invoice).Intent)!=JsonSerializer.Serialize(intent))throw new InvalidOperationException("The native disposition differs from its retained journal.");
            if(intent.ROW_CREATED_BY!=actor){if(invoice.Invoice.REVIEW_WORKFLOW_JSON is null)throw new InvalidOperationException("Another reviewer retains an unfinished original decision. Reconcile it before a different decision.");return null;}
            return intent;
        }
        var first=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original reviewer journal changed during recovery.");return closing;
    }
    public async Task<CompanySalesInvoiceDraftOutcome> RecordCompanyInvoiceDecisionAsync(CompanySalesInvoiceDecisionCommand command,Func<Task> requireAuthority,Func<Task<CompanySalesInvoiceReviewSource>> readCurrent)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);
        command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(bool owned)
        {await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice decision binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(binding.Source,IsolationLevel.Serializable);}
        async Task Field(string field){if(field!=command.Review.Current.FieldId)throw new InvalidOperationException("The original invoice decision FIELD changed.");await Guard(false);}
        async Task<CompanySalesInvoiceDraftOutcome> Native()=>await GetCompanyInvoiceDraftForReviewAsync(command.Company,command.InvoiceId,()=>Guard(false),Field);
        async Task RequireKnown(CompanySalesInvoiceDraftOutcome outcome)
        {
            var proof=CompanySalesInvoiceDecisionProof.Require(command.Company,outcome.Invoice);
            var journal=await ReadInvoiceReviewIntent(command.Company,command.InvoiceId,binding.Connection,binding.Source,()=>Guard(false));
            if(journal is null || JsonSerializer.Serialize(journal)!=JsonSerializer.Serialize(proof.Intent) || JsonSerializer.Serialize(journal.Restore(command.Company,command.ActorUserId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The original committed reviewer command differs. Reconcile without replacing it.");
            outcome.Require(command.Review.Original.Intent.Restore(command.Company,command.Review.Original.Intent.ROW_CREATED_BY));
        }
        var existing=await Native();
        if(existing.Invoice.REVIEW_WORKFLOW_JSON is not null){await RequireKnown(existing);await Guard(false);return existing;}
        SALES_INVOICE_REVIEW_INTENT? retained=null;
        // Commit the original private decision body separately so a later failed invoice update can be recovered by invoice identity.
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>
        {
            await Guard(true);RequireInvoiceReviewSchema(binding.Source);
            retained=await ReadInvoiceReviewIntent(command.Company,command.InvoiceId,binding.Connection,binding.Source,()=>Guard(true));
            if(retained is not null){if(JsonSerializer.Serialize(retained.Restore(command.Company,command.ActorUserId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("A different original reviewer command is retained.");}
            else
            {
                command.RequireCurrent(await readCurrent());await Guard(true);var repo=await GetRepoAsync<SALES_INVOICE_REVIEW_INTENT>("SALES_INVOICE_REVIEW_INTENT",binding.Connection);
                var collisions=(await repo.GetAsync([new(){FieldName="SALES_INVOICE_REVIEW_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();if(collisions is null || collisions.Length!=0)throw new InvalidOperationException("The original reviewer intent identity is already used or unconfirmed.");
                var entry=SALES_INVOICE_REVIEW_INTENT.Create(command,DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_INVOICE_REVIEW_INTENT),binding.Connection,"SALES_INVOICE_REVIEW_INTENT",rowAccess:new InvoiceReviewInsertPolicy(entry,()=>Guard(true)));
                await writer.InsertAsync(entry,command.ActorUserId);retained=await ReadInvoiceReviewIntent(command.Company,command.InvoiceId,binding.Connection,binding.Source,()=>Guard(true));
                if(JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The retained reviewer command differs from its native read-back.");command.RequireCurrent(await readCurrent());
            }
            await Guard(true);
        });
        await Guard(false);var durableIntent=await ReadInvoiceReviewIntent(command.Company,command.InvoiceId,binding.Connection,binding.Source,()=>Guard(false));
        if(retained is null || JsonSerializer.Serialize(retained)!=JsonSerializer.Serialize(durableIntent))throw new InvalidOperationException("The original reviewer journal commit is uncertain. Reconcile it before disposition.");
        CompanySalesInvoiceDraftOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>
        {
            await Guard(true);RequireInvoiceReviewSchema(binding.Source);var source=await readCurrent();await Guard(true);
            if(source.Original.Invoice.REVIEW_WORKFLOW_JSON is not null){await RequireKnown(source.Original);confirmed=source.Original;return;}
            command.RequireCurrent(source);var original=CompanyInventorySourceReview.Copy(source.Original.Invoice);var row=CompanyInventorySourceReview.Copy(original);
            var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(INVOICE),binding.Connection,"INVOICE",rowAccess:new InvoiceDecisionMutation(durableIntent!,original,()=>Guard(true)));
            await writer.UpdateAsync(row,command.ActorUserId);await Guard(true);
            var closing=await readCurrent();await Guard(true);
            var expected=CompanySalesInvoiceReviewSource.Create(command.Review.Original with{Invoice=row},CompanySalesInvoiceDraftReview.Create(command.Review.Current.Delivery,command.Review.Current.Policies,[row],command.Review.Current.BindingRevision));
            if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The complete source or stored invoice disposition changed during native closure.");await RequireKnown(closing.Original);confirmed=closing.Original;await Guard(true);
        });
        await Guard(false);var durable=await Native();await RequireKnown(durable);await Guard(false);
        if(confirmed is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed))throw new InvalidOperationException("Committed invoice disposition is unconfirmed. Reconcile the original reviewer command.");return CompanyInventorySourceReview.Copy(durable);
    }
    private sealed class InvoiceReviewInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly SALES_INVOICE_REVIEW_INTENT expected;private readonly Func<Task> guard;
        public InvoiceReviewInsertPolicy(SALES_INVOICE_REVIEW_INTENT entry,Func<Task> guard){expected=CompanyInventorySourceReview.Copy(entry);this.guard=guard;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(SALES_INVOICE_REVIEW_INTENT) || table!="SALES_INVOICE_REVIEW_INTENT" || previous is not null || operation!=PPDMRowMutation.Insert || entity is not SALES_INVOICE_REVIEW_INTENT row || actor!=expected.ROW_CREATED_BY)throw new InvalidOperationException("Only the original reviewer journal insertion is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=expected.ROW_EFFECTIVE_DATE;normalized.RECORDED_REVISION=expected.RECORDED_REVISION;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The reviewer command changed before insertion.");row.RECORDED_REVISION=SALES_INVOICE_REVIEW_INTENT.Revision(row);_=row.Restore(new(expected.OWNER_TENANT_ID,expected.OWNER_COMPANY_BA_ID),actor!);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Reviewer journal insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Reviewer journal insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Reviewer journal insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Reviewer journal insertion cannot grant reads.");
    }
    private sealed class InvoiceDecisionMutation:IPPDMRowAccessPolicy
    {
        private readonly SALES_INVOICE_REVIEW_INTENT intent;private readonly INVOICE original;private readonly Func<Task> guard;
        public InvoiceDecisionMutation(SALES_INVOICE_REVIEW_INTENT intent,INVOICE original,Func<Task> guard){this.intent=CompanyInventorySourceReview.Copy(intent);this.original=CompanyInventorySourceReview.Copy(original);this.guard=guard;}
        private async Task Table(Type type,string table){await guard();if(type!=typeof(INVOICE) || table!="INVOICE")throw new InvalidOperationException("Disposition is restricted to the original native invoice.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,table);var company=new CompanyScope(intent.OWNER_TENANT_ID,intent.OWNER_COMPANY_BA_ID);var command=intent.Restore(company,intent.ROW_CREATED_BY);
            if(operation!=PPDMRowMutation.Update || actor!=command.ActorUserId || entity is not INVOICE row || JsonSerializer.Serialize(previous)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Only the exact original independent invoice disposition update is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CHANGED_BY=original.ROW_CHANGED_BY;normalized.ROW_CHANGED_DATE=original.ROW_CHANGED_DATE;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The invoice was altered before disposition recording.");
            row.STATUS=command.Decision=="APPROVE"?"APPROVED":"REJECTED";row.REVIEW_WORKFLOW_JSON=CompanySalesInvoiceDecisionProof.Create(intent,row);_=CompanySalesInvoiceDecisionProof.Require(company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="INVOICE_ID",Operator="=",FilterValue=original.INVOICE_ID}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);return new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=intent.OWNER_TENANT_ID},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=intent.OWNER_COMPANY_BA_ID}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original native invoice changed before disposition.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Invoice disposition cannot grant reads.");
    }
}
