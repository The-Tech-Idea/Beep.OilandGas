using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Web.Services;
public sealed partial class PurchaseOrderWorkspaceState
{
    private PurchaseOrderInventoryLinkDecisionReview? linkDecisionReview;
    private PurchaseOrderInventoryLinkDecisionSubmission? linkDecisionIntent;
    private PO_LINE_ITEM? linkDecisionResult;
    private bool linkDecisionConfirmed;
    private int linkDecisionGeneration;
    public bool LinkDecisionBusy {get;private set;}
    public event Action? LinkDecisionChanged;
    public bool HasUnconfirmedLinkDecisionIntent=>linkDecisionIntent is not null && !linkDecisionConfirmed;
    public bool LinkDecisionBlocked=>LinkDecisionBusy || HasUnconfirmedLinkDecisionIntent;
    public PurchaseOrderInventoryLinkDecisionReview? LinkDecisionReview=>linkDecisionReview is null?null:CompanyInventorySourceReview.Copy(linkDecisionReview);
    public PurchaseOrderInventoryLinkDecisionSubmission? LinkDecisionIntent=>linkDecisionIntent is null?null:CompanyInventorySourceReview.Copy(linkDecisionIntent);
    public PO_LINE_ITEM? LinkDecisionResult=>linkDecisionResult is null?null:CompanyInventorySourceReview.Copy(linkDecisionResult);
    public string? LinkDecisionRecoveryRoute=>HasUnconfirmedLinkDecisionIntent && company.Selection is {} selected && selected.Scope==linkDecisionIntent!.Command.Company && selected.UserId==linkDecisionIntent.Command.ActorUserId?
        "/procurement/orders/"+Uri.EscapeDataString(linkDecisionIntent.Command.OrderId):null;
    public bool CanReconcileLinkDecision=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && !LinkDecisionBusy && !ReceiverBusy && !HasUnconfirmedReceiverIntent && !LinkPreparationBlocked && HasUnconfirmedLinkDecisionIntent &&
        company.Selection is {} selected && selected.Scope==linkDecisionIntent!.Command.Company && selected.UserId==linkDecisionIntent.Command.ActorUserId && Detail?.Id==linkDecisionIntent.Command.OrderId;
    private void ClearLinkDecisionDisplay()
    {
        ++linkDecisionGeneration;linkDecisionReview=null;linkDecisionResult=null;
        if(linkDecisionConfirmed && !LinkDecisionBusy)linkDecisionIntent=null;linkDecisionConfirmed=false;
    }
    public void ClearLinkDecisionReview()
    {
        if(LinkCycleBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Wait or reconcile the original independent decision first.");
        ++linkDecisionGeneration;linkDecisionReview=null;linkDecisionResult=null;
    }
    public async Task LoadLinkDecisionReviewAsync(string lineId,Func<string,string,Task<PurchaseOrderInventoryLinkDecisionReview>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || LinkDecisionBlocked || LinkPreparationBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)throw new InvalidOperationException("Wait or reconcile the original company intent before independent review.");
        ClearLinkDecisionReview();ClearLinkPreparationReview();ClearReceiverDisplay();var original=company.Snapshot;var version=Version;var generation=linkDecisionGeneration;var order=Detail?.Id;var revision=SourceRevision;
        var line=Lines?.SingleOrDefault(row=>row.Id==lineId);
        if(original.Selection is null || order is null || revision is null || line?.ActiveIndicator!="Y" || line.Preparation is null || line.HasDecisionEvidence || line.Preparation.Actor==original.Selection.UserId)
            throw new InvalidOperationException("Select an unresolved verified preparation made by another account.");
        LinkDecisionBusy=true;LinkDecisionChanged?.Invoke();
        try
        {
            var review=CompanyInventorySourceReview.Copy(await read(order,lineId) ?? throw new InvalidOperationException("Independent decision source is unavailable."));
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || version!=Version || generation!=linkDecisionGeneration || original!=company.Snapshot)return;
            review.Require(original.Selection.Scope,original.Selection.UserId,order,lineId,line.Preparation.Target);
            if(review.Purchase.Revision!=revision || SourceRevision!=revision)throw new InvalidOperationException("The displayed prepared source changed. Reload the original order.");
            linkDecisionReview=review;
        }
        finally{LinkDecisionBusy=false;LinkDecisionChanged?.Invoke();}
    }
    public async Task RecordLinkDecisionAsync(PurchaseOrderInventoryLinkDecision decision,string reason,Func<PurchaseOrderInventoryLinkDecisionSubmission,Task<PO_LINE_ITEM>> record,Func<string,Task<PurchaseOrderSourceReview?>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || LinkDecisionBusy || LinkPreparationBlocked || ReceiverBusy || HasUnconfirmedReceiverIntent || InventoryLinkBusy)throw new InvalidOperationException("Wait or reconcile current preparation/receiving work.");
        if(linkDecisionIntent is null || linkDecisionConfirmed)
        {
            var review=linkDecisionReview ?? throw new InvalidOperationException("Review the original prepared line first.");var selected=company.Selection ?? throw new InvalidOperationException("Select the original reviewer company and account.");
            var command=new PurchaseOrderInventoryLinkDecisionCommand(selected.Scope,selected.UserId,review.Purchase.Header.PURCHASE_ORDER_ID,review.LineId,review.Target.Item.INVENTORY_ITEM_ID,decision,review.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            var submission=new PurchaseOrderInventoryLinkDecisionSubmission(command,review);submission.Require(selected.Scope,selected.UserId,command.OrderId,command.LineId);
            if(Detail?.Id!=command.OrderId || SourceRevision!=review.Purchase.Revision)throw new InvalidOperationException("Reload the original prepared source.");
            linkDecisionIntent=CompanyInventorySourceReview.Copy(submission);linkDecisionConfirmed=false;
        }
        if(!CanReconcileLinkDecision)throw new InvalidOperationException("Return to the original reviewer company, account and order to reconcile.");
        var intent=CompanyInventorySourceReview.Copy(linkDecisionIntent!);var original=company.Snapshot;var version=Version;var generation=linkDecisionGeneration;
        LinkDecisionBusy=true;linkDecisionReview=null;linkDecisionResult=null;ClearInventoryLink();LinkDecisionChanged?.Invoke();
        bool Current()=>!disposed && !WarehouseWorkBlocked && !LinkCycleBlocked && version==Version && generation==linkDecisionGeneration && original==company.Snapshot;
        try
        {
            var result=CompanyInventorySourceReview.Copy(await record(CompanyInventorySourceReview.Copy(intent)) ?? throw new InvalidOperationException("Independent decision outcome is unavailable."));intent.RequireResult(result);if(!Current())return;
            PurchaseOrderSourceReview? fresh=null;using var staged=new PurchaseOrderWorkspaceState(company);
            await staged.LoadDetailAsync(intent.Command.OrderId,async id=>{fresh=CompanyInventorySourceReview.Copy(await read(id) ?? throw new InvalidOperationException("Decision read-back is unavailable."));return fresh;});
            if(!Current())return;
            var stored=fresh!.Lines.Single(row=>row.PO_LINE_ITEM_ID==intent.Command.LineId);intent.RequireResult(stored);
            if(JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("The recorded decision read-back changed.");
            var originalSource=PurchaseOrderSourceReview.Create(intent.Command.Company,intent.Command.ActorUserId,fresh.Header,
                fresh.Lines.Select(row=>row.PO_LINE_ITEM_ID==intent.Command.LineId?intent.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==intent.Command.LineId):row).ToArray(),fresh.Receipts);
            if(originalSource.Revision!=intent.Review.Purchase.Revision)throw new InvalidOperationException("Original prepared purchase source changed during decision confirmation.");
            Detail=staged.Detail;Lines=staged.Lines;Receipts=staged.Receipts;SourceRevision=staged.SourceRevision;RetrievedAtUtc=staged.RetrievedAtUtc;Rows=null;
            linkDecisionResult=result;linkDecisionConfirmed=true;
        }
        catch{if(Current()){linkDecisionResult=null;linkDecisionReview=null;}throw;}
        finally{LinkDecisionBusy=false;LinkDecisionChanged?.Invoke();}
    }
}
