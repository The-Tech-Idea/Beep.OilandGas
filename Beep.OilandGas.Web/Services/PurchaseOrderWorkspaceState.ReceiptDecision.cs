using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class PurchaseOrderWorkspaceState
{
    private PurchaseReceiptDecisionReview? receiverReview;
    private PurchaseReceiptDecisionSubmission? receiverIntent;
    private PurchaseReceiptDecisionResult? receiverResult;
    private bool receiverConfirmed;
    private int receiverGeneration;
    public bool ReceiverBusy {get;private set;}
    public event Action? ReceiverChanged;
    public bool HasUnconfirmedReceiverIntent=>receiverIntent is not null && !receiverConfirmed;
    public PurchaseReceiptDecisionReview? ReceiverReview=>receiverReview is null?null:PurchaseReceiptDecisionProof.Copy(receiverReview);
    public PurchaseReceiptDecisionSubmission? ReceiverIntent=>receiverIntent is null?null:PurchaseReceiptDecisionProof.Copy(receiverIntent);
    public PurchaseReceiptDecisionResult? ReceiverResult=>receiverResult is null?null:PurchaseReceiptDecisionProof.Copy(receiverResult);
    public string? ReceiverRecoveryRoute=>HasUnconfirmedReceiverIntent && company.Selection is {} selected && selected.Scope==receiverIntent!.Command.Company && selected.UserId==receiverIntent.Command.ActorUserId?
        "/procurement/orders/"+Uri.EscapeDataString(receiverIntent.Command.OrderId):null;
    public bool CanReconcileReceiver=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && !ReceiverBusy && HasUnconfirmedReceiverIntent && company.Selection is {} selected &&
        selected.Scope==receiverIntent!.Command.Company && selected.UserId==receiverIntent.Command.ActorUserId && Detail?.Id==receiverIntent.Command.OrderId;
    private void ClearReceiverDisplay()
    {
        ClearInventoryLink();
        ++receiverGeneration;
        receiverReview=null;receiverResult=null;if(receiverConfirmed && !ReceiverBusy)receiverIntent=null;receiverConfirmed=false;
    }
    public void ClearReceiverReview(){if(LinkCycleBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || LinkPreparationBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Reconcile the original receiving/preparation intent first.");ClearLinkDecisionReview();ClearLinkPreparationReview();ClearReceiverDisplay();++Version;}
    public async Task LoadReceiverReviewAsync(string receipt,Func<string,string,Task<PurchaseReceiptDecisionReview>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || LinkPreparationBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Reconcile the original receiver/preparation intent before reviewing another decision.");
        ClearLinkDecisionReview();ClearLinkPreparationReview();++receiverGeneration;var receiverCapture=receiverGeneration;ClearInventoryLink();receiverReview=null;receiverResult=null;var scope=company.Snapshot;var generation=Version;var order=Detail?.Id;var revision=SourceRevision;
        if(scope.Selection is null || order is null || revision is null)throw new InvalidOperationException("Reload the original company order first.");
        var review=PurchaseReceiptDecisionProof.Copy(await read(order,receipt));
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || generation!=Version || receiverCapture!=receiverGeneration || scope!=company.Snapshot)return;
        review.Require(scope.Selection.Scope,scope.Selection.UserId,order,receipt);
        if(review.Source.Revision!=revision)throw new InvalidOperationException("The displayed receiving source changed. Reload the order.");
        var row=review.Source.Receipts.Single(item=>item.PO_RECEIPT_ID==receipt);
        if(row.ACCEPTED_QUANTITY is not null || row.REJECTED_QUANTITY is not null || row.RECEIVER_DECISION_JSON is not null)
            throw new InvalidOperationException("This receipt already has decision evidence. Use an independently reviewed correction workflow.");
        receiverReview=review;
    }
    public async Task RecordReceiverAsync(decimal accepted,decimal rejected,string reason,
        Func<PurchaseReceiptDecisionSubmission,Task<PurchaseReceiptDecisionResult>> record,Func<string,Task<PurchaseOrderSourceReview?>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || ReceiverBusy || LinkPreparationBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Wait or reconcile the current preparation before receiving.");
        if(receiverIntent is null || receiverConfirmed)
        {
            var review=receiverReview ?? throw new InvalidOperationException("Review the original receipt before recording.");
            var selected=company.Selection ?? throw new InvalidOperationException("Select the original company.");
            var row=review.Source.Receipts.Single(item=>item.PO_RECEIPT_ID==review.ReceiptId);
            var command=new PurchaseReceiptDecisionCommand(selected.Scope,selected.UserId,review.Source.Header.PURCHASE_ORDER_ID,review.ReceiptId,accepted,rejected,row.QUANTITY_UOM ?? "",
                review.Source.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            review.Require(command);if(Detail?.Id!=command.OrderId || SourceRevision!=command.ExpectedOrderRevision)throw new InvalidOperationException("Reload the original receiving source.");
            receiverIntent=new(command,PurchaseReceiptDecisionProof.Copy(review));receiverConfirmed=false;
        }
        if(!CanReconcileReceiver)throw new InvalidOperationException("Return to the original company, account and order to reconcile receiving.");
        var intent=PurchaseReceiptDecisionProof.Copy(receiverIntent);var scope=company.Snapshot;var generation=Version;
        ClearInventoryLink();ReceiverBusy=true;receiverResult=null;receiverReview=null;
        bool Current()=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && generation==Version && scope==company.Snapshot;
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await record(PurchaseReceiptDecisionProof.Copy(intent)));result.Require(intent);if(!Current())return;
            PurchaseOrderSourceReview? fresh=null;
            using var staged=new PurchaseOrderWorkspaceState(company);
            await staged.LoadDetailAsync(intent.Command.OrderId,async id=>{fresh=PurchaseReceiptDecisionProof.Copy(await read(id) ?? throw new InvalidOperationException("The receiving read-back is unavailable."));return fresh;});
            if(!Current())return;
            var stored=fresh!.Receipts.Single(row=>row.PO_RECEIPT_ID==intent.Command.ReceiptId);
            if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(result.Receipt))throw new InvalidOperationException("The owned receiver decision read-back changed.");
            var proof=PurchaseReceiptDecisionProof.Require(intent.Command.Company,stored);
            var original=PurchaseOrderSourceReview.Create(intent.Command.Company,intent.Command.ActorUserId,fresh.Header,fresh.Lines,
                fresh.Receipts.Select(row=>row.PO_RECEIPT_ID==intent.Command.ReceiptId?proof.OriginalReceipt:row).ToArray());
            intent.Review.Require(intent.Command);intent.Command.Require(original);
            Detail=staged.Detail;Lines=staged.Lines;Receipts=staged.Receipts;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;Rows=null;
            receiverResult=result;receiverConfirmed=true;
        }
        catch{if(Current()){receiverResult=null;receiverReview=null;}throw;}
        finally{ReceiverBusy=false;ReceiverChanged?.Invoke();}
    }
}
