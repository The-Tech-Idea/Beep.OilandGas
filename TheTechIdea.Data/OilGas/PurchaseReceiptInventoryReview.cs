using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Exact recorded receipt-to-item source graph, without stock or supplier-return effects.</summary>
public sealed record PurchaseReceiptInventoryReview(PurchaseOrderSourceReview Purchase,CompanyInventorySourceReview Inventory,
    string ReceiptId,string BindingRevision,string Revision)
{
    public string? CurrentLineInventoryItemId=>Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==Purchase.Receipts.Single(value=>value.PO_RECEIPT_ID==ReceiptId).PO_LINE_ITEM_ID).INVENTORY_ITEM_ID;
    public bool UsesRecordedStockDestination=>CurrentLineInventoryItemId!=Inventory.Item.INVENTORY_ITEM_ID;
    public static string ResolveStockItemId(PurchaseOrderSourceReview purchase,string receiptId,IReadOnlyList<INVENTORY_TRANSACTION> rows)
    {
        purchase.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID);
        var receipt=purchase.Receipts.Single(row=>row.PO_RECEIPT_ID==receiptId);
        if(rows.Select(row=>row.INVENTORY_TRANSACTION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Physical receipt movement identities are ambiguous.");
        var proofs=rows.Select(row=>PurchaseReceiptStockMovement.Read(purchase.Company,row)).ToArray();
        if(proofs.Any(proof=>proof.OrderId!=purchase.Header.PURCHASE_ORDER_ID || proof.ReceiptId!=receiptId) ||
            rows.Any(row=>row.REFERENCE_NUMBER!=receiptId))throw new InvalidOperationException("Physical receipt movement membership is unconfirmed.");
        if(proofs.Length==0)return purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==receipt.PO_LINE_ITEM_ID).INVENTORY_ITEM_ID
            ??throw new InvalidOperationException("The recorded PO line has no inventory destination.");
        var items=proofs.Select(proof=>proof.ItemId).Distinct(StringComparer.Ordinal).ToArray();
        if(items.Length!=1 || !proofs.Any(proof=>proof.Kind==PurchaseReceiptStockMovementKind.Receive))
            throw new InvalidOperationException("The original physical receipt destination is ambiguous or unavailable.");
        return items[0];
    }
    public void Require(CompanyScope company,string actor,string orderId,string receiptId)
    {
        if(Purchase is null || Inventory?.Item is null || ReceiptId!=receiptId || string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl))
            throw new InvalidOperationException("The original receipt inventory source or binding is unavailable.");
        Purchase.Require(company,actor,orderId);Inventory.Require(company,actor,Inventory.Item.INVENTORY_ITEM_ID);
        var receipt=Purchase.Receipts.SingleOrDefault(row=>row.PO_RECEIPT_ID==receiptId) ?? throw new InvalidOperationException("The receipt is outside this owned order.");
        var line=Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==receipt.PO_LINE_ITEM_ID);var item=Inventory.Item;
        var decision=PurchaseReceiptDecisionProof.Require(company,receipt);
        if(Purchase.Header.ACTIVE_IND!="Y" || line.ACTIVE_IND!="Y" || receipt.ACTIVE_IND!="Y" || item.ACTIVE_IND!="Y" ||
            string.IsNullOrWhiteSpace(line.INVENTORY_ITEM_ID) || line.INVENTORY_ITEM_ID!=item.INVENTORY_ITEM_ID && !HasRecordedDestination(company,orderId,receiptId,item,receipt) ||
            string.IsNullOrWhiteSpace(item.WAREHOUSE_ID) || string.IsNullOrWhiteSpace(item.LOCATION_ID) ||
            string.IsNullOrWhiteSpace(item.UNIT_OF_MEASURE) || item.UNIT_OF_MEASURE!=line.QUANTITY_UOM || item.UNIT_OF_MEASURE!=receipt.QUANTITY_UOM || item.UNIT_OF_MEASURE!=decision.Command.QuantityUnit ||
            Revision!=Digest(Purchase,Inventory,ReceiptId,BindingRevision))
            throw new InvalidOperationException("Recorded inventory linkage, warehouse/location, unit, decision or source revisions do not agree.");
    }
    private bool HasRecordedDestination(CompanyScope company,string orderId,string receiptId,INVENTORY_ITEM item,PO_RECEIPT receipt)
    {
        var revision=ProcessDefinitionRevision.FromJson(receipt.RECEIVER_DECISION_JSON!);
        return Inventory.Transactions.Where(row=>row.PURCHASE_RECEIPT_STOCK_JSON is not null).Select(row=>PurchaseReceiptStockMovement.Read(company,row))
            .Any(proof=>proof.Kind==PurchaseReceiptStockMovementKind.Receive && proof.OrderId==orderId && proof.ReceiptId==receiptId &&
                proof.ItemId==item.INVENTORY_ITEM_ID && proof.ReceiptDecisionRevision==revision && proof.QuantityUnit==item.UNIT_OF_MEASURE &&
                proof.WarehouseId==item.WAREHOUSE_ID && proof.LocationId==item.LOCATION_ID);
    }
    public static PurchaseReceiptInventoryReview Create(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview inventory,string receiptId,string binding)
    {
        ArgumentNullException.ThrowIfNull(purchase);ArgumentNullException.ThrowIfNull(inventory);
        purchase=CompanyInventorySourceReview.Copy(purchase);inventory=CompanyInventorySourceReview.Copy(inventory);
        var review=new PurchaseReceiptInventoryReview(purchase,inventory,receiptId,binding,Digest(purchase,inventory,receiptId,binding));
        review.Require(purchase.Company,purchase.ActorUserId,purchase.Header.PURCHASE_ORDER_ID,receiptId);return review;
    }
    private static string Digest(PurchaseOrderSourceReview purchase,CompanyInventorySourceReview inventory,string receipt,string binding)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{purchase.Company,purchase.ActorUserId,PurchaseRevision=purchase.Revision,InventoryRevision=inventory.Revision,ReceiptId=receipt,BindingRevision=binding}));
}
