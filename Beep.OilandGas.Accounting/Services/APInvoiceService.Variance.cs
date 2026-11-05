using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class APInvoiceService
{
    /// <summary>Requires current original submission authority and complete financial reads on the same physical PRODUCTION source.</summary>
    public async Task<AP_INVOICE> RecordVarianceProposalAsync(APInvoiceVarianceCommand command,APInvoiceVarianceProposalReview displayed,
        Func<Task> requireAuthority,Func<Task<APInvoiceFinancialReview>> readFinancial)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readFinancial);
        command=PurchaseReceiptDecisionProof.Copy(command);displayed=PurchaseReceiptDecisionProof.Copy(displayed);command.Require(displayed);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original production connection is unavailable.");
        var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original invoice datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original variance production binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        AP_INVOICE? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);var schema=source.GetEntityStructure(new EntityStructure{EntityName="AP_INVOICE",DatasourceEntityName="AP_INVOICE"},true);
            var fields=schema?.Fields?.Where(field=>field.FieldName=="VARIANCE_PROPOSAL_JSON").ToArray();
            if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || fields[0].Size<APInvoiceVarianceProof.MaximumLength)
                throw new InvalidOperationException("Review the nullable bounded invoice proposal column in the PRODUCTION Beep migration.");
            var repo=await GetRepoAsync<AP_INVOICE>("AP_INVOICE");
            async Task<AP_INVOICE> Review()
            {
                await Guard(true);var financial=await readFinancial();financial.Require(command.Company,command.ActorUserId,command.InvoiceId,financial.Invoice.Revision);await Guard(true);
                var raw=(await repo.GetAsync([new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}]))?.ToArray();
                if(raw is not {Length:1} || raw[0] is not AP_INVOICE row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(financial.Invoice.Invoice))
                    throw new InvalidOperationException("The globally identified invoice differs from its complete owned financial capture.");
                APInvoiceOwnership.Require(command.Company,command.InvoiceId,row);
                var original=row.VARIANCE_PROPOSAL_JSON is null?financial:financial.OriginalVarianceEvidence(command);
                command.Require(APInvoiceVarianceProposalReview.Create(original,command.Proposal,command.BindingRevision));await Guard(true);return PurchaseReceiptDecisionProof.Copy(row);
            }
            var row=await Review();
            if(row.VARIANCE_PROPOSAL_JSON is null)
            {
                var original=PurchaseReceiptDecisionProof.Copy(row);
                var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AP_INVOICE),connection,"AP_INVOICE",rowAccess:new VarianceMutation(command,original,()=>Guard(true)));
                await writer.UpdateAsync(row,command.ActorUserId);var stored=await Review();
                if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The stored variance proposal differs from the reviewed invoice write.");row=stored;
            }
            _=APInvoiceVarianceProof.Require(command.Company,row);confirmed=await Review();await Guard(true);
        });
        await Guard(false);var finalRepo=await GetRepoAsync<AP_INVOICE>("AP_INVOICE");
        var durable=(await finalRepo.GetAsync([new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}]))?.ToArray();
        if(durable is not {Length:1} || durable[0] is not AP_INVOICE final || JsonSerializer.Serialize(final)!=JsonSerializer.Serialize(confirmed) || !APInvoiceVarianceProof.Require(command.Company,final).Command.Same(command))
            throw new InvalidOperationException("Committed variance proposal is unconfirmed. Reconcile the original intent.");
        await Guard(false);return PurchaseReceiptDecisionProof.Copy(final);
    }
    private sealed class VarianceMutation(APInvoiceVarianceCommand command,AP_INVOICE original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        private async Task Table(Type type,string table){await guard();if(type!=typeof(AP_INVOICE) || table!="AP_INVOICE")throw new InvalidOperationException("Variance mutation is restricted to the original invoice.");}
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await Table(type,table);
            if(actor!=command.ActorUserId || operation!=PPDMRowMutation.Update || entity is not AP_INVOICE row || JsonSerializer.Serialize(previous)!=JsonSerializer.Serialize(original))
                throw new InvalidOperationException("Only the reviewed original invoice proposal update is authorized.");
            row.VARIANCE_PROPOSAL_JSON=APInvoiceVarianceProof.Create(command,original,row);_=APInvoiceVarianceProof.Require(command.Company,row);
        }
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)
        {await Table(type,table);return new List<IReadOnlyList<AppFilter>>{new List<AppFilter>{new(){FieldName="AP_INVOICE_ID",Operator="=",FilterValue=command.InvoiceId}}};}
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)
        {await Table(type,table);return new List<AppFilter>{new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Company.BusinessAssociateId}};}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)
        {await Table(type,table);if(rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The original invoice changed before proposal recording.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Variance mutation cannot grant reads.");
    }
}
