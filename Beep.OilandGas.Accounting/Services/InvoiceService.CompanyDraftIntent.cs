using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;

public partial class InvoiceService
{
    private static void RequireInvoiceIntentSchema(IDataSource source)
    {
        const string table="SALES_INVOICE_INTENT";var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].FieldName!=table+"_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the invoice preparation journal key through the PRODUCTION Beep migration.");
        foreach(var (name,size) in new[]{(table+"_ID",32),("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("FIELD_ID",128),("RUN_TICKET_ID",128),("INVOICE_ID",32),("SOURCE_REVISION",64),("SCOPE_KEY",64),("COMMAND_JSON",SALES_INVOICE_INTENT.MaximumCommandLength),("RECORDED_REVISION",64)})
        {
            var fields=schema?.Fields?.Where(field=>field.FieldName==name).ToArray();if(fields is not {Length:1} || fields[0].AllowDBNull || fields[0].Fieldtype!=typeof(string).FullName || Math.Max(fields[0].Size,fields[0].Size1)<size)throw new InvalidOperationException("Review complete bounded invoice journal metadata through the PRODUCTION Beep migration.");
        }
    }
    private async Task<(string Connection,IDataSource Source)> InvoiceIntentBinding()
    {
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION invoice resolver is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The selected production binding is unavailable.");
        return(connection,_editor.GetDataSource(connection) ??throw new InvalidOperationException("The invoice preparation datasource is unavailable."));
    }
    public async Task<SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentAsync(CompanyScope company,string actor,string intentId,Func<Task> requireAuthority)
    {
        PurchaseOrderOwnership.RequireScope(company);if(string.IsNullOrWhiteSpace(actor) || actor.Length>128 || actor.Any(char.IsControl) || !Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty)throw new InvalidOperationException("The original bounded invoice company/account and intent are required.");ArgumentNullException.ThrowIfNull(requireAuthority);
        await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(){await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice preparation binding changed.");}
        async Task<SALES_INVOICE_INTENT?> Read()
        {
            await Guard();RequireInvoiceIntentSchema(binding.Source);var repo=await GetRepoAsync<SALES_INVOICE_INTENT>("SALES_INVOICE_INTENT",binding.Connection);
            var rows=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="ROW_CREATED_BY",Operator="=",FilterValue=actor},new(){FieldName="SALES_INVOICE_INTENT_ID",Operator="=",FilterValue=intentId}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not SALES_INVOICE_INTENT))throw new InvalidOperationException("The original unique invoice journal identity is unavailable or ambiguous.");
            if(rows.SingleOrDefault() is not SALES_INVOICE_INTENT row)return null;_=row.Restore(company,actor);if(row.SALES_INVOICE_INTENT_ID!=intentId)throw new InvalidOperationException("The provider returned another invoice intent identity.");return CompanyInventorySourceReview.Copy(row);
        }
        var first=await Read();var closing=await Read();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original invoice journal changed during read-back.");return closing;
    }
    public async Task<SALES_INVOICE_INTENT?> GetCompanyInvoiceDraftIntentForDeliveryAsync(CompanyScope company,string actor,string field,string ticket,Func<Task> requireAuthority)
    {
        PurchaseOrderOwnership.RequireScope(company);RequireInvoiceSourceId(actor);RequireInvoiceSourceId(field);RequireInvoiceSourceId(ticket);ArgumentNullException.ThrowIfNull(requireAuthority);
        await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard()
        {
            await requireAuthority();var current=await InvoiceIntentBinding();
            if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The retained invoice command binding changed.");
        }
        async Task<SALES_INVOICE_INTENT?> Read()
        {
            await Guard();RequireInvoiceIntentSchema(binding.Source);var repo=await GetRepoAsync<SALES_INVOICE_INTENT>("SALES_INVOICE_INTENT",binding.Connection);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
                new(){FieldName="FIELD_ID",Operator="=",FilterValue=field},new(){FieldName="RUN_TICKET_ID",Operator="=",FilterValue=ticket}]))?.ToArray();
            await Guard();if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_INVOICE_INTENT))throw new InvalidOperationException("The unique original company/delivery command is unconfirmed.");
            if(raw.SingleOrDefault() is not SALES_INVOICE_INTENT row)return null;RequireInvoiceSourceId(row.ROW_CREATED_BY);var command=row.Restore(company,row.ROW_CREATED_BY);
            if(command.Review.FieldId!=field || command.Review.TicketId!=ticket)throw new InvalidOperationException("The provider returned another original invoice delivery.");
            if(row.ROW_CREATED_BY!=actor)
            {
                // Shared invoice evidence remains readable; another maker's unfinished request cannot appear as an empty preparation source.
                var outcome=await GetCompanyInvoiceDraftOutcomeAsync(company,row.ROW_CREATED_BY,row.SALES_INVOICE_INTENT_ID,Guard);await Guard();
                if(outcome is null || JsonSerializer.Serialize(outcome.Intent)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("Another account's original invoice preparation remains unconfirmed. Reconcile it through the original account before another proposal.");
                outcome.Require(command);return null;
            }
            return CompanyInventorySourceReview.Copy(row);
        }
        var initial=await Read();var closing=await Read();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original invoice delivery command changed during read-back.");return closing;
    }
    public async Task<SALES_INVOICE_INTENT> SaveCompanyInvoiceDraftIntentAsync(CompanySalesInvoiceDraftCommand command,Func<Task> requireAuthority,Func<Task<CompanySalesInvoiceDraftReview>> readCurrent)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readCurrent);command=CompanyInventorySourceReview.Copy(command);command.Require();await requireAuthority();var binding=await InvoiceIntentBinding();
        async Task Guard(bool owned){await requireAuthority();var current=await InvoiceIntentBinding();if(current.Connection!=binding.Connection || !ReferenceEquals(current.Source,binding.Source))throw new InvalidOperationException("The original invoice journal binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(binding.Source,IsolationLevel.Serializable);}
        async Task<CompanySalesInvoiceDraftReview> Current(){await Guard(true);var source=CompanyInventorySourceReview.Copy(await readCurrent());command.RequireCurrent(source);await Guard(true);return source;}
        SALES_INVOICE_INTENT? recorded=null;
        await PPDMDataSourceTransaction.ExecuteAsync(binding.Source,async()=>
        {
            await Guard(true);RequireInvoiceIntentSchema(binding.Source);var repo=await GetRepoAsync<SALES_INVOICE_INTENT>("SALES_INVOICE_INTENT",binding.Connection);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Review.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Review.Company.BusinessAssociateId},new(){FieldName="SCOPE_KEY",Operator="=",FilterValue=command.Review.SourceKey}]))?.ToArray();await Guard(true);
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not SALES_INVOICE_INTENT))throw new InvalidOperationException("The unique original delivery-to-invoice preparation is unconfirmed.");
            if(raw.SingleOrDefault() is SALES_INVOICE_INTENT existing)
            {
                if(JsonSerializer.Serialize(existing.Restore(command.Review.Company,command.Review.ActorUserId))!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Another original invoice intent is retained for this delivery. Reconcile it before another proposal.");
                recorded=CompanyInventorySourceReview.Copy(existing);
            }
            else
            {
                var current=await Current();var collision=(await repo.GetAsync([new(){FieldName="SALES_INVOICE_INTENT_ID",Operator="=",FilterValue=command.IntentId}]))?.ToArray();
                if(collision is null || collision.Length!=0)throw new InvalidOperationException("The original invoice intent identity is unavailable or already used.");
                var entry=SALES_INVOICE_INTENT.Create(command,DateTime.UtcNow);var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_INVOICE_INTENT),binding.Connection,"SALES_INVOICE_INTENT",rowAccess:new InvoiceIntentInsertPolicy(entry,()=>Guard(true)));
                await writer.InsertAsync(entry,command.Review.ActorUserId);await Guard(true);
                recorded=await GetCompanyInvoiceDraftIntentAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(true));
                if(JsonSerializer.Serialize(recorded)!=JsonSerializer.Serialize(entry))throw new InvalidOperationException("The saved invoice command differs from its exact read-back.");
                var closing=await Current();if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("The complete invoice source changed while retaining the command.");
            }
            await Guard(true);
        });
        await Guard(false);var durable=await GetCompanyInvoiceDraftIntentAsync(command.Review.Company,command.Review.ActorUserId,command.IntentId,()=>Guard(false));
        if(recorded is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(recorded))throw new InvalidOperationException("Invoice preparation journal commit is uncertain. Reconcile the original intent.");await Guard(false);return CompanyInventorySourceReview.Copy(durable!);
    }
    private sealed class InvoiceIntentInsertPolicy:IPPDMRowAccessPolicy
    {
        private readonly SALES_INVOICE_INTENT expected;
        private readonly Func<Task> guard;
        public InvoiceIntentInsertPolicy(SALES_INVOICE_INTENT original,Func<Task> requireAuthority)
        {expected=CompanyInventorySourceReview.Copy(original);guard=requireAuthority;}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(SALES_INVOICE_INTENT) || table!="SALES_INVOICE_INTENT" || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.ROW_CREATED_BY || entity is not SALES_INVOICE_INTENT row)throw new InvalidOperationException("Only the original invoice intent insertion is authorized.");
            var normalized=CompanyInventorySourceReview.Copy(row);normalized.ROW_CREATED_DATE=expected.ROW_CREATED_DATE;normalized.ROW_CHANGED_DATE=expected.ROW_CHANGED_DATE;normalized.ROW_EFFECTIVE_DATE=expected.ROW_EFFECTIVE_DATE;normalized.RECORDED_REVISION=expected.RECORDED_REVISION;
            if(JsonSerializer.Serialize(normalized)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original invoice intent changed before insertion.");
            var company=new CompanyScope(expected.OWNER_TENANT_ID,expected.OWNER_COMPANY_BA_ID);_=expected.Restore(company,actor!);
            row.RECORDED_REVISION=SALES_INVOICE_INTENT.Revision(row);_=row.Restore(company,actor!);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice journal insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Invoice journal insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Invoice journal insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Invoice journal insertion cannot grant reads.");
    }
}
