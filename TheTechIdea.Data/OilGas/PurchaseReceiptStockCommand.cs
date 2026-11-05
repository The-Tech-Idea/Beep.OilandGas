using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record PurchaseReceiptStockCommand(CompanyScope Company,string ActorUserId,string OrderId,string ReceiptId,
    PurchaseReceiptStockMovementKind Kind,decimal Quantity,string QuantityUnit,string? OriginalReceiptTransactionId,
    string ExpectedReviewRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(PurchaseReceiptStockReview review)
    {
        ArgumentNullException.ThrowIfNull(review);review.Require(Company,ActorUserId,OrderId,ReceiptId);
        if(!Enum.IsDefined(Kind) || string.IsNullOrWhiteSpace(ActorUserId) || ActorUserId.Length>128 || ActorUserId.Any(char.IsControl) || ActorUserId=="SYSTEM" || Quantity<=0 ||
            QuantityUnit!=review.Source.Inventory.Item.UNIT_OF_MEASURE || ExpectedReviewRevision!=review.Revision ||
            BindingRevision!=review.Source.BindingRevision || !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length>512 || Reason.Any(char.IsControl))
            throw new InvalidOperationException("Original stock command, source, intent, unit or reason is unavailable.");
        if(Kind==PurchaseReceiptStockMovementKind.Receive)
        {
            if(OriginalReceiptTransactionId is not null || Quantity>review.RemainingReceiptQuantity)
                throw new InvalidOperationException("Stock receipt exceeds remaining gross receiver acceptance.");
        }
        else
        {
            var row=review.Source.Inventory.Transactions.SingleOrDefault(value=>value.INVENTORY_TRANSACTION_ID==OriginalReceiptTransactionId)
                ??throw new InvalidOperationException("Select an original physical receipt transaction.");
            var original=PurchaseReceiptStockMovement.Read(Company,row);
            var returned=review.Source.Inventory.Transactions.Where(value=>value.PURCHASE_RECEIPT_STOCK_JSON is not null)
                .Select(value=>PurchaseReceiptStockMovement.Read(Company,value)).Where(value=>value.Kind==PurchaseReceiptStockMovementKind.Return && value.OriginalReceiptTransactionId==OriginalReceiptTransactionId).Sum(value=>value.Quantity);
            if(original.Kind!=PurchaseReceiptStockMovementKind.Receive || original.OrderId!=OrderId || original.ReceiptId!=ReceiptId ||
                Quantity>original.Quantity-returned || Quantity>review.Source.Inventory.Item.QUANTITY_ON_HAND)
                throw new InvalidOperationException("Supplier return exceeds its original physical receipt or recorded stock balance.");
        }
        _=checked(review.Source.Inventory.Item.QUANTITY_ON_HAND!.Value+(Kind==PurchaseReceiptStockMovementKind.Receive?Quantity:-Quantity));
    }
    public PurchaseReceiptStockMovement Movement(PurchaseReceiptStockReview review,INVENTORY_TRANSACTION row)
    {
        Require(review);
        var receipt=review.Source.Purchase.Receipts.Single(value=>value.PO_RECEIPT_ID==ReceiptId);var item=review.Source.Inventory.Item;
        return new(Company,OrderId,ReceiptId,item.INVENTORY_ITEM_ID,QuantityUnit,item.WAREHOUSE_ID!,item.LOCATION_ID!,
            ProcessDefinitionRevision.FromJson(receipt.RECEIVER_DECISION_JSON!),review.Revision,BindingRevision,IntentId,ActorUserId,Kind,Quantity,
            item.QUANTITY_ON_HAND!.Value,checked(item.QUANTITY_ON_HAND.Value+(Kind==PurchaseReceiptStockMovementKind.Receive?Quantity:-Quantity)),OriginalReceiptTransactionId,Reason,PurchaseReceiptStockMovement.DigestRow(row));
    }
    public INVENTORY_ITEM ResultItem(PurchaseReceiptStockReview review,INVENTORY_TRANSACTION row)
    {
        Require(review);var proof=PurchaseReceiptStockMovement.Read(Company,row);
        if(proof!=Movement(review,row))throw new InvalidOperationException("Recorded movement does not confirm the original stock intent.");
        var item=CompanyInventorySourceReview.Copy(review.Source.Inventory.Item);item.QUANTITY_ON_HAND=proof.ResultQuantityOnHand;
        item.ROW_CHANGED_BY=ActorUserId;item.ROW_CHANGED_DATE=row.ROW_CREATED_DATE;
        if(item.ROW_CHANGED_DATE<item.ROW_CREATED_DATE || item.ROW_CHANGED_DATE<review.Source.Inventory.Item.ROW_CHANGED_DATE)
            throw new InvalidOperationException("Stock recording would precede the original inventory audit.");
        return item;
    }
}

public sealed record PurchaseReceiptStockSubmission(PurchaseReceiptStockCommand Command,PurchaseReceiptStockReview Review)
{
    public void Require()
    {
        if(Command is null || Review is null)throw new InvalidOperationException("Original stock submission is unavailable.");
        Command.Require(Review);
    }
    public void RequireResult(PurchaseReceiptInventoryReview source)
    {
        Require();if(source is null)throw new InvalidOperationException("Complete stock read-back is unavailable.");
        source.Require(Command.Company,Command.ActorUserId,Command.OrderId,Command.ReceiptId);
        var row=source.Inventory.Transactions.SingleOrDefault(value=>value.INVENTORY_TRANSACTION_ID==Command.IntentId)
            ??throw new InvalidOperationException("Original movement was not recorded.");
        var proof=PurchaseReceiptStockMovement.Read(Command.Company,row);
        if(proof!=Command.Movement(Review,row) || source.BindingRevision!=Command.BindingRevision ||
            source.Purchase.Revision!=Review.Source.Purchase.Revision || row.UNIT_COST is not null || row.TOTAL_COST is not null ||
            row.REFERENCE_NUMBER!=Command.ReceiptId || row.NOTES!=Command.Reason ||
            JsonSerializer.Serialize(source.Inventory.Item)!=JsonSerializer.Serialize(Command.ResultItem(Review,row)))
            throw new InvalidOperationException("Stock movement, original purchase or item read-back changed.");
        var expectedHistory=Review.Source.Inventory.Transactions.Concat([row]).OrderBy(value=>value.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal);
        if(JsonSerializer.Serialize(expectedHistory)!=JsonSerializer.Serialize(source.Inventory.Transactions.OrderBy(value=>value.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal)))
            throw new InvalidOperationException("Complete stock history differs from the original reviewed history and exact movement.");
        _=PurchaseReceiptStockReview.Create(source);
    }
}
