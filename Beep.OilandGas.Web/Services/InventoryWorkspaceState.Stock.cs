using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class InventoryWorkspaceState
{
    private PurchaseReceiptStockReview? stockReview;
    private PurchaseReceiptStockSubmission? stockIntent;
    private PurchaseReceiptInventoryReview? stockResult;
    private PurchaseReceiptStockRecovery? stockRecovery;
    private PurchaseReceiptStockIntentIndex? stockIndex;
    private PurchaseReceiptStockIntentPage? stockPage;
    public PurchaseReceiptStockIntentPage? StockPage=>stockPage is null?null:CompanyInventorySourceReview.Copy(stockPage);
    public async Task LoadStockPageAsync(int page,int size,Func<int,int,Task<PurchaseReceiptStockIntentPage>> read)
    {
        PurchaseReceiptStockIntentPage.RequireRequest(page,size);
        if(disposed || StockBusy || ProcurementWorkBlocked)throw new InvalidOperationException("Wait for original stock/procurement work before discovery.");
        stockIndex=null;stockPage=null;var original=company.Snapshot;var version=Version;var generation=stockGeneration;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        StockBusy=true;StockChanged?.Invoke();
        try
        {
            var result=CompanyInventorySourceReview.Copy(await read(page,size) ??throw new InvalidOperationException("Original stock page is unavailable."));
            if(disposed || ProcurementWorkBlocked || original!=company.Snapshot || version!=Version || generation!=stockGeneration)return;
            result.Require(original.Selection.Scope,original.Selection.UserId,page,size);stockPage=result;stockIndex=CompanyInventorySourceReview.Copy(result.Index);
        }
        finally{StockBusy=false;StockChanged?.Invoke();}
    }
    public PurchaseReceiptStockIntentIndex? StockIndex=>stockIndex is null?null:CompanyInventorySourceReview.Copy(stockIndex);
    public async Task LoadStockIndexAsync(Func<Task<PurchaseReceiptStockIntentIndex>> read)
    {
        if(disposed || StockBusy || ProcurementWorkBlocked)throw new InvalidOperationException("Wait for original stock/procurement work before discovery.");
        stockIndex=null;stockPage=null;var original=company.Snapshot;var version=Version;var generation=stockGeneration;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        StockBusy=true;StockChanged?.Invoke();
        try
        {
            var result=CompanyInventorySourceReview.Copy(await read() ??throw new InvalidOperationException("Saved stock discovery is unavailable."));
            if(disposed || ProcurementWorkBlocked || original!=company.Snapshot || version!=Version || generation!=stockGeneration)return;
            result.Require(original.Selection.Scope,original.Selection.UserId);stockIndex=result;
        }
        finally{StockBusy=false;StockChanged?.Invoke();}
    }
    private bool stockConfirmed;
    private int stockGeneration;
    public bool StockBusy {get;private set;}
    public bool HasUnconfirmedStockIntent=>stockIntent is not null && !stockConfirmed;
    public bool StockBlocked=>StockBusy || HasUnconfirmedStockIntent;
    public bool CanReconcileStock=>!disposed && !StockBusy && !ProcurementWorkBlocked && HasUnconfirmedStockIntent && company.Selection is {} selected &&
        selected.Scope==stockIntent!.Command.Company && selected.UserId==stockIntent.Command.ActorUserId;
    public event Action? StockChanged;
    public PurchaseReceiptStockReview? StockReview=>stockReview is null?null:CompanyInventorySourceReview.Copy(stockReview);
    public PurchaseReceiptStockSubmission? StockIntent=>stockIntent is null?null:CompanyInventorySourceReview.Copy(stockIntent);
    public PurchaseReceiptInventoryReview? StockResult=>stockResult is null?null:CompanyInventorySourceReview.Copy(stockResult);
    public PurchaseReceiptStockRecovery? StockRecovery=>stockRecovery is null?null:CompanyInventorySourceReview.Copy(stockRecovery);
    public string? SavedStockRecoveryRoute=>stockIntent is null?null:SavedStockRoute(stockIntent.Command.IntentId);
    public static string SavedStockRoute(string intent)=>"/warehouse/stock-intents/"+Uri.EscapeDataString(intent);
    public string? StockRecoveryRoute=>CanReconcileStock?StockRoute(stockIntent!.Command.OrderId,stockIntent.Command.ReceiptId):null;
    public static string StockRoute(string order,string receipt)=>"/warehouse/receipt-stock/"+Uri.EscapeDataString(order)+"/"+Uri.EscapeDataString(receipt);
    private void ClearStockDisplay()
    {
        ++stockGeneration;stockReview=null;stockResult=null;stockRecovery=null;stockIndex=null;stockPage=null;
        if(stockConfirmed && !StockBusy)stockIntent=null;stockConfirmed=false;
    }
    public async Task LoadStockRecoveryAsync(string intentId,Func<string,Task<PurchaseReceiptStockRecovery>> read)
    {
        if(disposed || StockBusy || ProcurementWorkBlocked)throw new InvalidOperationException("Wait for original stock/procurement work.");
        if(HasUnconfirmedStockIntent && (!CanReconcileStock || stockIntent!.Command.IntentId!=intentId))
            throw new InvalidOperationException("Recover the original pending stock intent first.");
        ClearStockDisplay();var original=company.Snapshot;var version=Version;var generation=stockGeneration;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        bool Current()=>!disposed && !ProcurementWorkBlocked && original==company.Snapshot && version==Version && generation==stockGeneration;
        StockBusy=true;StockChanged?.Invoke();
        try
        {
            var first=CompanyInventorySourceReview.Copy(await read(intentId) ??throw new InvalidOperationException("Saved stock evidence is unavailable."));
            first.Require(original.Selection.Scope,original.Selection.UserId,intentId);if(!Current())return;
            var closing=CompanyInventorySourceReview.Copy(await read(intentId) ??throw new InvalidOperationException("Closing saved stock evidence is unavailable."));
            closing.Require(original.Selection.Scope,original.Selection.UserId,intentId);if(!Current())return;
            if(first.Revision!=closing.Revision)throw new InvalidOperationException("Saved stock evidence or current source changed during recovery.");
            var restored=closing.OriginalSubmission();
            if(HasUnconfirmedStockIntent && (restored.Command!=stockIntent!.Command || restored.Review.Revision!=stockIntent.Review.Revision))
                throw new InvalidOperationException("Saved evidence does not confirm the original pending stock intent.");
            var exact=closing.CurrentMatchesOriginalEffect();
            stockRecovery=closing;stockIntent=restored;stockConfirmed=true;
            if(exact){stockResult=PurchaseReceiptInventoryReview.Create(closing.CurrentPurchase!,closing.CurrentInventory!,restored.Command.ReceiptId,restored.Command.BindingRevision);stockReview=PurchaseReceiptStockReview.Create(stockResult);}
            if(closing.CurrentInventory is {} inventory)PublishDetail(inventory);
            else{Detail=null;Movements=null;Revision=null;RetrievedAtUtc=null;}Rows=null;
        }
        finally{StockBusy=false;StockChanged?.Invoke();}
    }
    public async Task LoadStockReviewAsync(string order,string receipt,Func<string,string,Task<PurchaseReceiptStockReview>> read)
    {
        if(disposed || StockBlocked || ProcurementWorkBlocked)throw new InvalidOperationException("Wait or reconcile the original stock/procurement intent first.");
        ClearStockDisplay();var original=company.Snapshot;var version=Version;var generation=stockGeneration;
        if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        StockBusy=true;StockChanged?.Invoke();
        try
        {
            var review=CompanyInventorySourceReview.Copy(await read(order,receipt) ??throw new InvalidOperationException("Complete receipt stock review is unavailable."));
            if(disposed || version!=Version || generation!=stockGeneration || original!=company.Snapshot)return;
            review.Require(original.Selection.Scope,original.Selection.UserId,order,receipt);stockReview=review;
            PublishDetail(review.Source.Inventory);Rows=null;
        }
        finally{StockBusy=false;StockChanged?.Invoke();}
    }
    public async Task RecordStockAsync(PurchaseReceiptStockMovementKind kind,decimal quantity,string? originalReceipt,string reason,
        Func<PurchaseReceiptStockSubmission,Task<PurchaseReceiptInventoryReview>> record,Func<string,string,Task<PurchaseReceiptInventoryReview>> read)
    {
        if(disposed || StockBusy || ProcurementWorkBlocked)throw new InvalidOperationException("Wait for original stock/procurement work.");
        if(stockIntent is null || stockConfirmed)
        {
            var review=stockReview ??throw new InvalidOperationException("Review the original receipt stock source first.");
            var selected=company.Selection ??throw new InvalidOperationException("Select the original company account.");
            var command=new PurchaseReceiptStockCommand(selected.Scope,selected.UserId,review.Source.Purchase.Header.PURCHASE_ORDER_ID,review.Source.ReceiptId,
                kind,quantity,review.Source.Inventory.Item.UNIT_OF_MEASURE!,originalReceipt,review.Revision,review.Source.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            var intent=new PurchaseReceiptStockSubmission(command,review);intent.Require();stockIntent=CompanyInventorySourceReview.Copy(intent);stockConfirmed=false;
        }
        if(!CanReconcileStock)throw new InvalidOperationException("Return to the original company and account for stock reconciliation.");
        var submission=CompanyInventorySourceReview.Copy(stockIntent!);var original=company.Snapshot;var version=Version;var generation=stockGeneration;
        bool Current()=>!disposed && !ProcurementWorkBlocked && original==company.Snapshot && version==Version && generation==stockGeneration;
        StockBusy=true;stockReview=null;stockResult=null;StockChanged?.Invoke();
        try
        {
            var result=CompanyInventorySourceReview.Copy(await record(CompanyInventorySourceReview.Copy(submission)) ??throw new InvalidOperationException("Stock result is unavailable."));
            submission.RequireResult(result);if(!Current())return;
            var fresh=CompanyInventorySourceReview.Copy(await read(submission.Command.OrderId,submission.Command.ReceiptId) ??throw new InvalidOperationException("Complete stock read-back is unavailable."));
            submission.RequireResult(fresh);if(!Current())return;
            if(result.Revision!=fresh.Revision)throw new InvalidOperationException("Committed stock read-back changed.");
            var reviewed=PurchaseReceiptStockReview.Create(fresh);if(!Current())return;
            PublishDetail(fresh.Inventory);Rows=null;stockReview=reviewed;stockResult=result;stockConfirmed=true;
        }
        catch{if(Current()){stockReview=null;stockResult=null;}throw;}
        finally{StockBusy=false;StockChanged?.Invoke();}
    }
}
