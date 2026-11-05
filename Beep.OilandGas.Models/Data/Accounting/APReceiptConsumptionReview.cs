using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded receipt contributors across owned invoice lines; not an available balance or matching approval.</summary>
public sealed record APReceiptConsumptionReview(APLinePurchaseSourceReview Source,string ReceiptId,
    IReadOnlyList<APReceiptAllocationReview> Contributors,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public PurchaseReceiptDecisionCommand? VerifiedReceiverDecision
    {
        get
        {
            if(Source?.Invoice is null || Source.Order is null || Contributors is null)return null;
            Require(Source.Invoice.Company,Source.Invoice.ActorUserId,Source,ReceiptId);
            var line=Source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==Source.InvoiceLineId);
            var orderLine=Source.Order.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
            var receipt=Source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID==ReceiptId);
            if(Source.Order.Header.ACTIVE_IND!="Y" || orderLine.ACTIVE_IND!="Y" || receipt.ACTIVE_IND!="Y" ||
                string.IsNullOrWhiteSpace(line.QUANTITY_UOM) || line.QUANTITY_UOM!=orderLine.QUANTITY_UOM || line.QUANTITY_UOM!=receipt.QUANTITY_UOM ||
                string.IsNullOrEmpty(receipt.RECEIVER_DECISION_JSON))return null;
            // A missing decision is unavailable. Present but malformed or inconsistent authoritative proof is refused.
            return PurchaseReceiptDecisionProof.Require(Source.Invoice.Company,receipt).Command;
        }
    }
    public decimal? VerifiedAcceptedQuantity=>VerifiedReceiverDecision?.AcceptedQuantity;
    public decimal? VerifiedRejectedQuantity=>VerifiedReceiverDecision?.RejectedQuantity;
    // Unknown activity or an active allocation in another unit prevents quantity arithmetic.
    public decimal? RecordedActiveQuantity
    {
        get
        {
            var receipt=Source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID==ReceiptId);
            var rows=Contributors.SelectMany(review=>review.Allocations).ToArray();
            if(string.IsNullOrWhiteSpace(receipt.QUANTITY_UOM) || rows.Any(row=>row.ACTIVE_IND is not ("Y" or "N") ||
                row.ACTIVE_IND=="Y" && row.QUANTITY_UOM!=receipt.QUANTITY_UOM))return null;
            return ExactDecimalArithmetic.SumOrUnavailable(rows.Where(row=>row.ACTIVE_IND=="Y").Select(row=>row.ALLOCATED_QUANTITY!.Value));
        }
    }
    public static APReceiptConsumptionReview Create(CompanyScope company,string actor,APLinePurchaseSourceReview source,
        string receiptId,IReadOnlyList<APReceiptAllocationReview> contributors)
    {
        var frozenSource=JsonSerializer.Deserialize<APLinePurchaseSourceReview>(JsonSerializer.Serialize(source))!;
        var frozen=Array.AsReadOnly(contributors.OrderBy(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID,StringComparer.Ordinal)
            .ThenBy(row=>row.Source.InvoiceLineId,StringComparer.Ordinal)
            .Select(row=>JsonSerializer.Deserialize<APReceiptAllocationReview>(JsonSerializer.Serialize(row))!).ToArray());
        var result=new APReceiptConsumptionReview(frozenSource,receiptId,frozen,Digest(frozenSource,receiptId,frozen),DateTimeOffset.UtcNow);
        result.Require(company,actor,source,receiptId);_=result.VerifiedReceiverDecision;return result;
    }
    public void Require(CompanyScope company,string actor,APLinePurchaseSourceReview expected,string receiptId)
    {
        if(Source is null || expected is null || Contributors is null || ReceiptId!=receiptId ||
            string.IsNullOrWhiteSpace(ReceiptId) || ReceiptId.Length>128 || ReceiptId.Any(char.IsControl))
            throw new InvalidOperationException("The receipt consumption source is unavailable.");
        var invoiceId=expected.Invoice.Invoice.AP_INVOICE_ID;
        var orderId=Source.RequireOrderId(company,actor,invoiceId,expected.InvoiceLineId,expected.Invoice.Revision);
        if(Source.Order.Revision!=expected.Order.Revision)throw new InvalidOperationException("The reviewed purchase source changed.");
        var line=Source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==Source.InvoiceLineId);
        if(!Source.Order.Receipts.Any(row=>row.PO_RECEIPT_ID==ReceiptId && row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID))
            throw new InvalidOperationException("The selected receipt does not belong to this invoice's order line.");
        var lineKeys=new HashSet<(string,string)>();var allocationIds=new HashSet<string>(StringComparer.Ordinal);
        foreach(var contribution in Contributors)
        {
            if(contribution?.Source?.Invoice is null)throw new InvalidOperationException("An allocation contributor is unavailable.");
            var contributorId=contribution.Source.Invoice.Invoice.AP_INVOICE_ID;
            contribution.Require(company,actor,contributorId,contribution.Source.InvoiceLineId,contribution.Source.Invoice.Revision);
            if(contribution.Source.Order.Revision!=Source.Order.Revision || contribution.Allocations.Count==0 ||
                !lineKeys.Add((contributorId,contribution.Source.InvoiceLineId)) ||
                contributorId==invoiceId && contribution.Source.Invoice.Revision!=Source.Invoice.Revision)
                throw new InvalidOperationException("The receipt contributors are ambiguous or their sources changed.");
            foreach(var row in contribution.Allocations)
                if(row.PO_RECEIPT_ID!=ReceiptId || row.PURCHASE_ORDER_ID!=orderId || row.PO_LINE_ITEM_ID!=line.PO_LINE_ITEM_ID ||
                    !allocationIds.Add(row.AP_RECEIPT_ALLOCATION_ID))
                    throw new InvalidOperationException("An allocation does not belong to this exact receipt or has an ambiguous identity.");
        }
        if(!ProcessDefinitionRevision.IsValid(Revision) || Revision!=Digest(Source,ReceiptId,Contributors) ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The receipt contributor content changed or its provenance is unavailable.");
    }
    private static string Digest(APLinePurchaseSourceReview source,string receiptId,IReadOnlyList<APReceiptAllocationReview> contributors)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{source.Invoice.Revision,OrderRevision=source.Order.Revision,
            source.InvoiceLineId,receiptId,Contributors=contributors.OrderBy(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID,StringComparer.Ordinal)
                .ThenBy(row=>row.Source.InvoiceLineId,StringComparer.Ordinal).Select(row=>row.Revision)}));
}
