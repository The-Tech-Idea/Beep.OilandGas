using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class PurchaseOrderWorkspaceState
{
    private PurchaseReceiptInventoryReview? inventoryLink;
    private int inventoryGeneration;
    public bool InventoryLinkBusy {get;private set;}
    public event Action? InventoryLinkChanged;
    public PurchaseReceiptInventoryReview? InventoryLink=>inventoryLink is null?null:CompanyInventorySourceReview.Copy(inventoryLink);
    public void ClearInventoryLink(){inventoryLink=null;++inventoryGeneration;}
    public async Task LoadInventoryLinkAsync(string receiptId,Func<string,string,Task<PurchaseReceiptInventoryReview>> read)
    {
        if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || InventoryLinkBusy || ReceiverBusy || HasUnconfirmedReceiverIntent || LinkPreparationBlocked || LinkDecisionBlocked)throw new InvalidOperationException("Wait for receiving/preparation or reconcile its original intent before reviewing inventory linkage.");
        ClearInventoryLink();var generation=inventoryGeneration;var version=Version;var original=company.Snapshot;var order=Detail?.Id;var revision=SourceRevision;
        if(original.Selection is null || order is null || revision is null || Receipts?.Any(row=>row.Id==receiptId)!=true)
            throw new InvalidOperationException("Reload the original owned order and receipt first.");
        InventoryLinkBusy=true;InventoryLinkChanged?.Invoke();
        try
        {
            var result=CompanyInventorySourceReview.Copy(await read(order,receiptId) ?? throw new InvalidOperationException("Receipt inventory source is unavailable."));
            if(disposed || WarehouseWorkBlocked || LinkCycleBlocked || generation!=inventoryGeneration || version!=Version || original!=company.Snapshot)return;
            result.Require(original.Selection.Scope,original.Selection.UserId,order,receiptId);
            if(result.Purchase.Revision!=revision || SourceRevision!=revision)throw new InvalidOperationException("The displayed receiving source changed. Reload the original order.");
            inventoryLink=result;
        }
        finally{InventoryLinkBusy=false;InventoryLinkChanged?.Invoke();}
    }
}
