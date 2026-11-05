using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>One final physical receiving decision; it neither changes received quantity nor approves an invoice.</summary>
public sealed record PurchaseReceiptDecisionCommand(CompanyScope Company,string ActorUserId,string OrderId,string ReceiptId,
    decimal AcceptedQuantity,decimal RejectedQuantity,string QuantityUnit,string ExpectedOrderRevision,string BindingRevision,string IntentId,string Reason)
{
    public void Require(PurchaseOrderSourceReview review)
    {
        RequireShape();review.Require(Company,ActorUserId,OrderId);
        if(review.Revision!=ExpectedOrderRevision)throw new InvalidOperationException("The reviewed purchase order changed.");
        var receipt=review.Receipts.SingleOrDefault(row=>row.PO_RECEIPT_ID==ReceiptId)
            ?? throw new InvalidOperationException("The receipt is not in this owned order.");
        RequireOriginal(receipt);
        var line=review.Lines.Single(row=>row.PO_LINE_ITEM_ID==receipt.PO_LINE_ITEM_ID);
        if(review.Header.ACTIVE_IND!="Y" || line.ACTIVE_IND!="Y" || line.QUANTITY_UOM!=QuantityUnit)
            throw new InvalidOperationException("An active owned order and line with exact recorded units are required.");
    }
    public void RequireShape()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        static bool Text(string? text,int max)=>!string.IsNullOrWhiteSpace(text) && text.Length<=max && !text.Any(char.IsControl);
        if(!Text(ActorUserId,128) || ActorUserId=="SYSTEM" || !Text(OrderId,128) || !Text(ReceiptId,128) ||
            !Text(QuantityUnit,64) || !Text(BindingRevision,512) || !Text(Reason,512) ||
            !ProcessDefinitionRevision.IsValid(ExpectedOrderRevision) || !Guid.TryParseExact(IntentId,"N",out var id) || id==Guid.Empty || id.ToString("N")!=IntentId ||
            AcceptedQuantity<0 || RejectedQuantity<0)
            throw new InvalidOperationException("An original company, receiver, receipt, unit, reviewed revision, binding, intent and reason are required.");
        try{if(checked(AcceptedQuantity+RejectedQuantity)<=0)throw new InvalidOperationException("A positive received quantity is required.");}
        catch(OverflowException){throw new InvalidOperationException("The receiver quantities exceed their supported range.");}
    }
    public void RequireOriginal(PO_RECEIPT receipt)
    {
        RequireShape();
        if(receipt.PO_RECEIPT_ID!=ReceiptId || receipt.PURCHASE_ORDER_ID!=OrderId || receipt.ACTIVE_IND!="Y" ||
            receipt.RECEIVED_QUANTITY is not >0 || receipt.QUANTITY_UOM!=QuantityUnit ||
            receipt.ACCEPTED_QUANTITY is not null || receipt.REJECTED_QUANTITY is not null || receipt.RECEIVER_DECISION_JSON is not null ||
            AcceptedQuantity+RejectedQuantity!=receipt.RECEIVED_QUANTITY)
            throw new InvalidOperationException("The first final decision must account for exactly the original received quantity and unit. Corrections require a separate reviewed workflow.");
    }
}

/// <summary>Original intent, original physical receipt and exact canonical result after common-column preparation.</summary>
public sealed record PurchaseReceiptDecisionProof(PurchaseReceiptDecisionCommand Command,PO_RECEIPT OriginalReceipt,string RowRevision)
{
    public const int MaximumLength=16000;
    public static T Copy<T>(T item)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item))!;
    public static string Digest(PO_RECEIPT row){var copy=Copy(row);copy.RECEIVER_DECISION_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(PurchaseReceiptDecisionCommand command,PO_RECEIPT original,PO_RECEIPT result)
    {
        command.RequireOriginal(original);
        var text=JsonSerializer.Serialize(new PurchaseReceiptDecisionProof(command,Copy(original),Digest(result)));
        if(text.Length>MaximumLength)throw new InvalidOperationException("The receipt decision proof exceeds its migrated bound.");
        return text;
    }
    public static PurchaseReceiptDecisionProof Require(CompanyScope company,PO_RECEIPT row)
    {
        if(string.IsNullOrEmpty(row.RECEIVER_DECISION_JSON) || row.RECEIVER_DECISION_JSON.Length>MaximumLength)
            throw new InvalidOperationException("The receipt has no bounded receiver decision proof.");
        PurchaseReceiptDecisionProof? proof;
        try{proof=JsonSerializer.Deserialize<PurchaseReceiptDecisionProof>(row.RECEIVER_DECISION_JSON);}
        catch(JsonException){throw new InvalidOperationException("The receiver decision proof is malformed.");}
        if(proof?.Command is null || proof.OriginalReceipt is null || proof.Command.Company!=company || proof.RowRevision!=Digest(row))
            throw new InvalidOperationException("The receiver decision content or company changed.");
        var command=proof.Command;command.RequireOriginal(proof.OriginalReceipt);
        var expected=Copy(proof.OriginalReceipt);expected.ACCEPTED_QUANTITY=command.AcceptedQuantity;expected.REJECTED_QUANTITY=command.RejectedQuantity;
        expected.ROW_CHANGED_BY=command.ActorUserId;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        if(row.ROW_CHANGED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not {} changed || changed==default || changed.Kind!=DateTimeKind.Utc ||
            (row.ROW_CREATED_DATE is {} created && changed<created) || Digest(expected)!=Digest(row) ||
            Create(command,proof.OriginalReceipt,row)!=row.RECEIVER_DECISION_JSON)
            throw new InvalidOperationException("The decision changed original physical evidence or lacks exact receiver audit.");
        return proof;
    }
}
