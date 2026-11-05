using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public enum PurchaseReceiptStockMovementKind { Receive=1, Return=2 }

/// <summary>Compact retained physical-movement provenance; does not embed earlier histories or imply ledger posting.</summary>
public sealed record PurchaseReceiptStockMovement(CompanyScope Company,string OrderId,string ReceiptId,string ItemId,
    string QuantityUnit,string WarehouseId,string LocationId,string ReceiptDecisionRevision,string SourceReviewRevision,
    string BindingRevision,string IntentId,string ActorUserId,PurchaseReceiptStockMovementKind Kind,decimal Quantity,
    decimal PriorQuantityOnHand,decimal ResultQuantityOnHand,string? OriginalReceiptTransactionId,string Reason,string RowRevision)
{
    public void Require(CompanyScope company,INVENTORY_TRANSACTION row)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(Company!=company || !Text(OrderId,128) || !Text(ReceiptId,128) || !Text(ItemId,128) ||
            !Text(QuantityUnit,64) || !Text(WarehouseId,128) || !Text(LocationId,128) || !Text(BindingRevision,512) ||
            !Text(ActorUserId,128) || ActorUserId=="SYSTEM" || !Text(Reason,512) ||
            !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId ||
            !ProcessDefinitionRevision.IsValid(ReceiptDecisionRevision) || !ProcessDefinitionRevision.IsValid(SourceReviewRevision) ||
            !Enum.IsDefined(Kind) || Quantity<=0 || PriorQuantityOnHand<0 || ResultQuantityOnHand<0 ||
            ResultQuantityOnHand!=checked(PriorQuantityOnHand+(Kind==PurchaseReceiptStockMovementKind.Receive?Quantity:-Quantity)) ||
            (Kind==PurchaseReceiptStockMovementKind.Receive?OriginalReceiptTransactionId is not null:!Text(OriginalReceiptTransactionId,128)) ||
            row.INVENTORY_TRANSACTION_ID!=IntentId || row.INVENTORY_ITEM_ID!=ItemId || row.ACTIVE_IND!="Y" ||
            row.TRANSACTION_TYPE!=(Kind==PurchaseReceiptStockMovementKind.Receive?"PURCHASE_RECEIPT":"SUPPLIER_RETURN") ||
            row.QUANTITY!=(Kind==PurchaseReceiptStockMovementKind.Receive?Quantity:-Quantity) ||
            row.ROW_CREATED_BY!=ActorUserId || row.ROW_CHANGED_BY!=ActorUserId || row.ROW_CREATED_DATE is null ||
            row.ROW_CREATED_DATE.Value.Kind!=DateTimeKind.Utc || row.ROW_CHANGED_DATE!=row.ROW_CREATED_DATE || row.TRANSACTION_DATE!=row.ROW_CREATED_DATE ||
            RowRevision!=DigestRow(row))
            throw new InvalidOperationException("Receipt stock movement ownership, intent, quantity, original receipt or audit is unconfirmed.");
    }
    public static PurchaseReceiptStockMovement Read(CompanyScope company,INVENTORY_TRANSACTION row)
    {
        var json=row.PURCHASE_RECEIPT_STOCK_JSON;
        if(string.IsNullOrWhiteSpace(json) || json.Length>16384)throw new InvalidOperationException("Receipt stock movement provenance is unavailable.");
        PurchaseReceiptStockMovement? proof;
        try{proof=JsonSerializer.Deserialize<PurchaseReceiptStockMovement>(json);}catch(JsonException error){throw new InvalidOperationException("Receipt stock movement provenance is malformed.",error);}
        if(proof is null || JsonSerializer.Serialize(proof)!=json)throw new InvalidOperationException("Receipt stock movement provenance is not canonical.");
        proof.Require(company,row);return proof;
    }
    public static string DigestRow(INVENTORY_TRANSACTION row)
    {
        var copy=CompanyInventorySourceReview.Copy(row);copy.PURCHASE_RECEIPT_STOCK_JSON=null;
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    private static bool Text(string? value,int max)=>!string.IsNullOrWhiteSpace(value) && value.Length<=max && !value.Any(char.IsControl);
}

/// <summary>Accepted receipt capacity and proven physical history. Returned stock never frees gross receipt capacity.</summary>
public sealed record PurchaseReceiptStockReview(PurchaseReceiptInventoryReview Source,decimal AcceptedQuantity,
    decimal ReceivedQuantity,decimal ReturnedQuantity,decimal RemainingReceiptQuantity,string Revision)
{
    public void Require(CompanyScope company,string actor,string orderId,string receiptId)
    {
        if(Source is null)throw new InvalidOperationException("Receipt stock source is unavailable.");
        Source.Require(company,actor,orderId,receiptId);
        var expected=Values(Source);
        if(AcceptedQuantity!=expected.Accepted || ReceivedQuantity!=expected.Received || ReturnedQuantity!=expected.Returned ||
            RemainingReceiptQuantity!=expected.Accepted-expected.Received || Revision!=Digest(Source))
            throw new InvalidOperationException("Receipt stock capacity or provenance changed.");
    }
    public static PurchaseReceiptStockReview Create(PurchaseReceiptInventoryReview source)
    {
        ArgumentNullException.ThrowIfNull(source);source=CompanyInventorySourceReview.Copy(source);
        source.Require(source.Purchase.Company,source.Purchase.ActorUserId,source.Purchase.Header.PURCHASE_ORDER_ID,source.ReceiptId);
        var values=Values(source);var result=new PurchaseReceiptStockReview(source,values.Accepted,values.Received,values.Returned,values.Accepted-values.Received,Digest(source));
        result.Require(source.Purchase.Company,source.Purchase.ActorUserId,source.Purchase.Header.PURCHASE_ORDER_ID,source.ReceiptId);return result;
    }
    private static (decimal Accepted,decimal Received,decimal Returned) Values(PurchaseReceiptInventoryReview source)
    {
        if(source.Inventory.Item.QUANTITY_ON_HAND is null or <0)throw new InvalidOperationException("Recorded nonnegative quantity on hand is required for a stock review.");
        var receipt=source.Purchase.Receipts.Single(row=>row.PO_RECEIPT_ID==source.ReceiptId);
        var accepted=PurchaseReceiptDecisionProof.Require(source.Purchase.Company,receipt).Command.AcceptedQuantity;
        var movements=new List<(INVENTORY_TRANSACTION Row,PurchaseReceiptStockMovement Proof)>();
        foreach(var row in source.Inventory.Transactions)
        {
            if(row.PURCHASE_RECEIPT_STOCK_JSON is null)
            {
                if(row.TRANSACTION_TYPE!="USAGE" || row.QUANTITY is null or >=0 || row.ACTIVE_IND!="Y")
                    throw new InvalidOperationException("Unclassified or unproven receipt/return history requires reconciliation before stock review.");
                continue;
            }
            movements.Add((row,PurchaseReceiptStockMovement.Read(source.Purchase.Company,row)));
        }
        foreach(var returned in movements.Where(value=>value.Proof.Kind==PurchaseReceiptStockMovementKind.Return))
        {
            var original=movements.SingleOrDefault(value=>value.Row.INVENTORY_TRANSACTION_ID==returned.Proof.OriginalReceiptTransactionId);
            if(original.Proof is null || original.Proof.Kind!=PurchaseReceiptStockMovementKind.Receive ||
                original.Proof.OrderId!=returned.Proof.OrderId || original.Proof.ReceiptId!=returned.Proof.ReceiptId ||
                original.Proof.QuantityUnit!=returned.Proof.QuantityUnit || original.Proof.WarehouseId!=returned.Proof.WarehouseId ||
                original.Proof.LocationId!=returned.Proof.LocationId || original.Proof.ReceiptDecisionRevision!=returned.Proof.ReceiptDecisionRevision ||
                returned.Row.TRANSACTION_DATE<original.Row.TRANSACTION_DATE ||
                movements.Where(value=>value.Proof.Kind==PurchaseReceiptStockMovementKind.Return && value.Proof.OriginalReceiptTransactionId==original.Row.INVENTORY_TRANSACTION_ID).Sum(value=>value.Proof.Quantity)>original.Proof.Quantity)
                throw new InvalidOperationException("Supplier returns exceed or do not identify their original physical receipt.");
        }
        var relevant=movements.Where(value=>value.Proof.OrderId==source.Purchase.Header.PURCHASE_ORDER_ID && value.Proof.ReceiptId==source.ReceiptId).ToArray();
        var receiverRevision=ProcessDefinitionRevision.FromJson(receipt.RECEIVER_DECISION_JSON!);
        if(relevant.Any(value=>value.Proof.QuantityUnit!=source.Inventory.Item.UNIT_OF_MEASURE ||
            value.Proof.WarehouseId!=source.Inventory.Item.WAREHOUSE_ID || value.Proof.LocationId!=source.Inventory.Item.LOCATION_ID ||
            value.Proof.ReceiptDecisionRevision!=receiverRevision))
            throw new InvalidOperationException("Physical receipt provenance disagrees with current acceptance, warehouse, location or unit.");
        var received=relevant.Where(value=>value.Proof.Kind==PurchaseReceiptStockMovementKind.Receive).Sum(value=>value.Proof.Quantity);
        var returnedQuantity=relevant.Where(value=>value.Proof.Kind==PurchaseReceiptStockMovementKind.Return).Sum(value=>value.Proof.Quantity);
        if(received>accepted)throw new InvalidOperationException("Gross stock receipts exceed physical receiver acceptance.");
        return (accepted,received,returnedQuantity);
    }
    private static string Digest(PurchaseReceiptInventoryReview source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{StockReceiptSourceRevision=source.Revision}));
}
