using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Accounting.Services;

public partial class APInvoiceService
{
    /// <summary>
    /// Records one original reviewed allocation in the request-bound PRODUCTION Beep transaction.
    /// Authority must independently require a write grant and the original company, actor and binding.
    /// Purchase reads must use the canonical owned PO service on that same physical datasource.
    /// The AP API independently enforces original app roles, permissions and binding for submission and retry.
    /// </summary>
    public async Task<AP_RECEIPT_ALLOCATION> RecordReceiptAllocationAsync(APReceiptAllocationCommand command,
        APReceiptAllocationProposalReview displayed,Func<Task> requireAuthority,Func<Task<PurchaseOrderSourceReview>> readOrder)
    {
        ArgumentNullException.ThrowIfNull(command);ArgumentNullException.ThrowIfNull(displayed);
        ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(readOrder);
        // Freeze the original input before any upstream await. All fresh reviews must reproduce it.
        command=JsonSerializer.Deserialize<APReceiptAllocationCommand>(JsonSerializer.Serialize(command))!;
        displayed=JsonSerializer.Deserialize<APReceiptAllocationProposalReview>(JsonSerializer.Serialize(displayed))!;
        command.Require(displayed);
        await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION connection is required.");
        var connection=await _resolveConnection();
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("A request-bound PRODUCTION connection is required.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The allocation datasource is unavailable.");
        async Task Guard(bool owned)
        {
            await requireAuthority();
            if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))
                throw new InvalidOperationException("The original allocation binding changed.");
            if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);
        }
        AP_RECEIPT_ALLOCATION? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);
            var repo=await GetRepoAsync<AP_RECEIPT_ALLOCATION>("AP_RECEIPT_ALLOCATION");
            var schema=source.GetEntityStructure(new EntityStructure{EntityName="AP_RECEIPT_ALLOCATION",DatasourceEntityName="AP_RECEIPT_ALLOCATION"},true);
            var proof=schema?.Fields?.Where(field=>field.FieldName=="REVIEW_COMMAND_JSON").ToArray();
            if(proof is not {Length:1} || proof[0].Fieldtype!=typeof(string).FullName || proof[0].Size<4000)
                throw new InvalidOperationException("Review the PRODUCTION Beep migration for bounded allocation command proof before recording.");
            async Task<AP_RECEIPT_ALLOCATION?> Recorded()
            {
                await Guard(true);
                // Key is globally stable; do not hide a corrupt or foreign row with owner predicates.
                var raw=await repo.GetAsync([new(){FieldName="AP_RECEIPT_ALLOCATION_ID",Operator="=",FilterValue=command.AllocationId}])
                    ?? throw new InvalidOperationException("The allocation outcome read is unavailable.");
                var rows=raw.ToArray();
                if(rows.Length==0)return null;
                if(rows.Length!=1 || rows[0] is not AP_RECEIPT_ALLOCATION row)throw new InvalidOperationException("The allocation outcome is ambiguous.");
                RequireRecorded(row,command,displayed);
                return Copy(row);
            }
            async Task Review(AP_RECEIPT_ALLOCATION? own)
            {
                await Guard(true);
                var invoice=await GetSourceReviewAsync(command.Company,command.InvoiceId,command.ActorUserId)
                    ?? throw new InvalidOperationException("The original invoice is unavailable.");
                var order=await readOrder();await Guard(true);
                var purchase=new APLinePurchaseSourceReview(invoice,command.LineId,order);
                var rows=await GetReceiptAllocationsForLineAsync(command.Company,command.InvoiceId,command.LineId);
                if(own is not null)RequireOwnPresence(rows,own);
                var allocations=APReceiptAllocationReview.Create(command.Company,command.ActorUserId,command.InvoiceId,command.LineId,
                    command.Proposal.ExpectedInvoiceRevision,purchase,rows.Where(row=>own is null || row.AP_RECEIPT_ALLOCATION_ID!=own.AP_RECEIPT_ALLOCATION_ID).ToArray());
                var consumption=await GetReceiptConsumptionAsync(command.Company,command.ActorUserId,purchase,command.Proposal.ReceiptId,()=>Guard(true));
                if(own is not null)
                {
                    RequireOwnPresence(consumption.Contributors.SelectMany(item=>item.Allocations).ToArray(),own);
                    var contributors=consumption.Contributors.Select(item=>APReceiptAllocationReview.Create(command.Company,command.ActorUserId,
                        item.Source.Invoice.Invoice.AP_INVOICE_ID,item.Source.InvoiceLineId,item.Source.Invoice.Revision,item.Source,
                        item.Allocations.Where(row=>row.AP_RECEIPT_ALLOCATION_ID!=own.AP_RECEIPT_ALLOCATION_ID).ToArray()))
                        .Where(item=>item.Allocations.Count>0).ToArray();
                    consumption=APReceiptConsumptionReview.Create(command.Company,command.ActorUserId,purchase,command.Proposal.ReceiptId,contributors);
                }
                var current=APReceiptAllocationProposalReview.Create(command.Company,command.ActorUserId,command.InvoiceId,command.LineId,command.Proposal,allocations,consumption,command.BindingRevision);
                command.Require(current);await Guard(true);
            }
            confirmed=await Recorded();
            await Review(confirmed);
            if(confirmed is null)
            {
                var line=displayed.Source.Source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==command.LineId);
                var row=new AP_RECEIPT_ALLOCATION{
                    AP_RECEIPT_ALLOCATION_ID=command.AllocationId,OWNER_TENANT_ID=command.Company.TenantId,OWNER_COMPANY_BA_ID=command.Company.BusinessAssociateId,
                    AP_INVOICE_ID=command.InvoiceId,AP_INVOICE_LINE_ID=command.LineId,PURCHASE_ORDER_ID=line.PURCHASE_ORDER_ID!,PO_LINE_ITEM_ID=line.PO_LINE_ITEM_ID!,
                    PO_RECEIPT_ID=command.Proposal.ReceiptId,ALLOCATED_QUANTITY=command.Proposal.Quantity,QUANTITY_UOM=command.Proposal.Unit,ACTIVE_IND="Y",
                    PPDM_GUID=Guid.ParseExact(command.IntentId,"N").ToString(),ROW_CREATED_BY=command.ActorUserId,ROW_CREATED_DATE=DateTime.UtcNow,
                    REMARK=command.Reason,SOURCE="AP_RECEIPT_ALLOCATION"};
                var insert=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(AP_RECEIPT_ALLOCATION),connection,"AP_RECEIPT_ALLOCATION",
                    rowAccess:new AllocationInsertPolicy(command,()=>Guard(true)));
                await Guard(true);await insert.InsertAsync(row,command.ActorUserId);
                confirmed=await Recorded() ?? throw new InvalidOperationException("The recorded allocation cannot be confirmed.");
                if(JsonSerializer.Serialize(confirmed)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The persisted allocation differs from the inserted evidence.");
            }
            await Review(confirmed);
            await Guard(true);
        });
        await Guard(false);
        // A provider success flag is insufficient: confirm exact durable content after transaction release.
        var finalRepo=await GetRepoAsync<AP_RECEIPT_ALLOCATION>("AP_RECEIPT_ALLOCATION");
        var durable=await finalRepo.GetAsync([new(){FieldName="AP_RECEIPT_ALLOCATION_ID",Operator="=",FilterValue=command.AllocationId}]);
        var final=durable?.ToArray();
        if(final is not {Length:1} || final[0] is not AP_RECEIPT_ALLOCATION stored || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(confirmed))
            throw new InvalidOperationException("The committed allocation outcome is unconfirmed. Reconcile the original intent before recording another.");
        RequireRecorded(stored,command,displayed);await Guard(false);
        return Copy(stored);
    }
    private static AP_RECEIPT_ALLOCATION Copy(AP_RECEIPT_ALLOCATION row)=>JsonSerializer.Deserialize<AP_RECEIPT_ALLOCATION>(JsonSerializer.Serialize(row))!;
    private static void RequireOwnPresence(IReadOnlyList<AP_RECEIPT_ALLOCATION> rows,AP_RECEIPT_ALLOCATION own)
    {
        var matching=rows.Where(row=>row.AP_RECEIPT_ALLOCATION_ID==own.AP_RECEIPT_ALLOCATION_ID).ToArray();
        if(matching.Length!=1 || JsonSerializer.Serialize(matching[0])!=JsonSerializer.Serialize(own))
            throw new InvalidOperationException("The original allocation changed or is missing from its owned contributors.");
    }
    private static void RequireRecorded(AP_RECEIPT_ALLOCATION row,APReceiptAllocationCommand command,APReceiptAllocationProposalReview displayed)
    {
        _=APReceiptAllocationReceipt.Create(command,displayed,row);
    }
    // The existing repository calls this after its canonical common-column handler, immediately before SDK insert.
    private sealed class AllocationInsertPolicy(APReceiptAllocationCommand command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();
            if(type!=typeof(AP_RECEIPT_ALLOCATION) || table!="AP_RECEIPT_ALLOCATION" || operation!=PPDMRowMutation.Insert || previous is not null ||
                actor!=command.ActorUserId || entity is not AP_RECEIPT_ALLOCATION row || row.AP_RECEIPT_ALLOCATION_ID!=command.AllocationId)
                throw new InvalidOperationException("Only the original reviewed allocation insertion is authorized.");
            row.REVIEW_COMMAND_JSON=APReceiptAllocationRecordedProof.Create(command,row);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Insertion policy cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Insertion policy cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Insertion policy cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Insertion policy cannot grant reads.");
    }
}
