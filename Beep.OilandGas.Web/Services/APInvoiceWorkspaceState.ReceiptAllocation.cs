using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public sealed partial class APInvoiceWorkspaceState
{
    private APReceiptAllocationProposalReview? reviewedAllocationEvidence;
    private APReceiptAllocationSubmission? allocationIntent;
    private APReceiptAllocationReceipt? allocationReceipt;
    private APReceiptConsumptionReview? refreshedConsumption;
    private bool allocationConfirmed;
    public event Action? AllocationChanged;
    private static T CopyAllocation<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public bool AllocationCommandBusy {get;private set;}
    public bool HasUnconfirmedAllocationIntent=>allocationIntent is not null && !allocationConfirmed;
    public string? AllocationRecoveryRoute=>HasUnconfirmedAllocationIntent && company.Selection is {} selected && selected.Scope==allocationIntent!.Command.Company &&
        selected.UserId==allocationIntent.Command.ActorUserId?"/accounting/payables/invoices/"+Uri.EscapeDataString(allocationIntent.Command.InvoiceId):null;
    public APReceiptAllocationSubmission? AllocationIntent=>allocationIntent is null?null:CopyAllocation(allocationIntent);
    public APReceiptAllocationReceipt? AllocationReceipt=>allocationReceipt is null?null:CopyAllocation(allocationReceipt);
    public APReceiptConsumptionReview? RefreshedReceiptConsumption=>refreshedConsumption is null?null:CopyAllocation(refreshedConsumption);
    public bool CanReconcileAllocation=>!disposed && !AllocationCommandBusy && HasUnconfirmedAllocationIntent && company.Selection is {} selected &&
        selected.Scope==allocationIntent!.Command.Company && selected.UserId==allocationIntent.Command.ActorUserId && Detail?.Id==allocationIntent.Command.InvoiceId;
    public bool CanRecordAllocation
    {
        get
        {
            if(CorrectionCommandUnsettled || disposed || AllocationCommandBusy || HasUnconfirmedAllocationIntent || reviewedAllocationEvidence is not {} review || company.Selection is not {} selected ||
                Detail?.Id!=review.Source.Source.Invoice.Invoice.AP_INVOICE_ID || SelectedPurchaseLineId!=review.Source.Source.InvoiceLineId ||
                SourceRevision!=review.Proposal.ExpectedInvoiceRevision || LinkedOrder.SourceRevision!=review.Proposal.ExpectedOrderRevision || AllocationRevision!=review.Proposal.ExpectedAllocationRevision)return false;
            try
            {
                new APReceiptAllocationCommand(selected.Scope,selected.UserId,Detail.Id,SelectedPurchaseLineId,review.Proposal,review.ReceiptConsumption?.Revision ?? "",
                    review.Revision,review.BindingRevision ?? "",new Guid("11111111-1111-1111-1111-111111111111").ToString("N"),"Readiness check").Require(review);
                return true;
            }
            catch(InvalidOperationException){return false;}
        }
    }
    private void InvalidateAllocationDisplay()
    {
        allocationReceipt=null;refreshedConsumption=null;
        if(allocationConfirmed && !AllocationCommandBusy)allocationIntent=null;
        allocationConfirmed=false;
    }
    public async Task RecordAllocationAsync(Func<APReceiptAllocationSubmission,Task<APReceiptAllocationReceipt>> record,
        Func<string,Task<APInvoiceSourceReview?>> readInvoice,Func<string,string,string,Task<APLinePurchaseSourceReview>> readPurchase,
        Func<string,string,string,string,Task<APReceiptAllocationReview>> readAllocations,
        Func<string,string,string,string,string,Task<APReceiptConsumptionReview>> readConsumption,
        Func<string,string,Task<APInvoiceFinancialReview>> readFinancial,string reason)
    {
        if(CorrectionCommandUnsettled || disposed || AllocationCommandBusy || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || VarianceCommandBusy || HasUnconfirmedVarianceIntent)throw new InvalidOperationException("Wait for or reconcile the current source command outcome.");
        if(allocationIntent is null || allocationConfirmed)
        {
            if(!CanRecordAllocation || reviewedAllocationEvidence is not {} review || company.Selection is not {} selected)
                throw new InvalidOperationException("Review the current invoice-line proposal before recording.");
            var command=new APReceiptAllocationCommand(selected.Scope,selected.UserId,Detail!.Id,SelectedPurchaseLineId!,review.Proposal,
                review.ReceiptConsumption!.Revision,review.Revision,review.BindingRevision!,Guid.NewGuid().ToString("N"),reason);
            command.Require(review);allocationIntent=new(command,CopyAllocation(review));allocationConfirmed=false;
        }
        if(!CanReconcileAllocation)throw new InvalidOperationException("Return to the original company, account and invoice to reconcile this allocation.");
        var intent=CopyAllocation(allocationIntent);var commandIntent=intent.Command;
        var original=company.Snapshot;var generation=Version;var purchase=purchaseVersion;var allocations=allocationVersion;var proposal=proposalVersion;
        AllocationCommandBusy=true;allocationReceipt=null;refreshedConsumption=null;ClearFinancialReview();
        bool Current()=>!disposed && original==company.Snapshot && generation==Version && purchase==purchaseVersion && allocations==allocationVersion && proposal==proposalVersion;
        try
        {
            var receipt=CopyAllocation(await record(CopyAllocation(intent)));receipt.Require(commandIntent,intent.Review);
            if(!Current())return;
            // Stage all source owners before publishing any refreshed data or successful outcome.
            using var staged=new APInvoiceWorkspaceState(company);
            await staged.LoadDetailAsync(commandIntent.InvoiceId,readInvoice);if(!Current())return;
            if(staged.SourceRevision!=commandIntent.Proposal.ExpectedInvoiceRevision)throw new InvalidOperationException("The invoice changed after allocation recording.");
            await staged.LoadPurchaseSourceAsync(commandIntent.LineId,readPurchase);if(!Current())return;
            if(staged.LinkedOrder.SourceRevision!=commandIntent.Proposal.ExpectedOrderRevision)throw new InvalidOperationException("The purchase source changed after allocation recording.");
            APReceiptAllocationReview? recorded=null;
            await staged.LoadReceiptAllocationsAsync(async(id,line,invoiceRevision,orderRevision)=>{
                recorded=CopyAllocation(await readAllocations(id,line,invoiceRevision,orderRevision));return recorded;});if(!Current())return;
            var own=recorded!.Allocations.Where(row=>row.AP_RECEIPT_ALLOCATION_ID==receipt.Allocation.AP_RECEIPT_ALLOCATION_ID).ToArray();
            if(own.Length!=1 || JsonSerializer.Serialize(own[0])!=JsonSerializer.Serialize(receipt.Allocation))
                throw new InvalidOperationException("The allocation is absent or changed in its owned line read-back.");
            var consumption=await readConsumption(commandIntent.InvoiceId,commandIntent.LineId,commandIntent.Proposal.ReceiptId,
                staged.SourceRevision!,staged.LinkedOrder.SourceRevision!);if(!Current())return;
            consumption=CopyAllocation(consumption);consumption.Require(commandIntent.Company,commandIntent.ActorUserId,recorded.Source,commandIntent.Proposal.ReceiptId);
            var consumed=consumption.Contributors.SelectMany(item=>item.Allocations).Where(row=>row.AP_RECEIPT_ALLOCATION_ID==receipt.Allocation.AP_RECEIPT_ALLOCATION_ID).ToArray();
            if(consumed.Length!=1 || JsonSerializer.Serialize(consumed[0])!=JsonSerializer.Serialize(receipt.Allocation) || consumption.RecordedActiveQuantity is null)
                throw new InvalidOperationException("The allocation's complete receipt consumption is unconfirmed.");
            await staged.LoadFinancialReviewAsync(readFinancial);if(!Current())return;
            var invoiceFinancial=staged.FinancialReview!;
            if(invoiceFinancial.PurchaseLines.SingleOrDefault(item=>item.Source.InvoiceLineId==commandIntent.LineId)?.Revision!=recorded.Revision)
                throw new InvalidOperationException("The financial evidence and allocation read-back changed.");
            await LinkedOrder.LoadDetailAsync(recorded.Source.Order.Header.PURCHASE_ORDER_ID,_=>Task.FromResult<PurchaseOrderSourceReview?>(recorded.Source.Order));
            if(!Current())return;
            Detail=staged.Detail;Lines=staged.Lines;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;
            SelectedPurchaseLineId=commandIntent.LineId;Allocations=staged.Allocations;AllocationRevision=staged.AllocationRevision;AllocationsRetrievedAtUtc=staged.AllocationsRetrievedAtUtc;
            ClearProposal();financial=CopyAllocation(invoiceFinancial);refreshedConsumption=consumption;allocationReceipt=CopyAllocation(receipt);allocationConfirmed=true;
        }
        catch
        {
            if(Current()){ClearReceiptAllocations();ClearFinancialReview();}
            throw;
        }
        finally{AllocationCommandBusy=false;AllocationChanged?.Invoke();}
    }
}
