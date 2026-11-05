using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Globalization;
namespace Beep.OilandGas.Models.Data.Accounting;

public enum APSourceComparisonOutcome { RecordedAgreement,RecordedDifference,WithinRecordedLimit,ExceedsRecordedLimit,Unavailable }
public sealed record APSourceComparison(string Code,string Label,APSourceComparisonOutcome Outcome,string Evidence)
{
    public string OutcomeText=>Outcome switch{
        APSourceComparisonOutcome.RecordedAgreement=>"Recorded agreement",APSourceComparisonOutcome.RecordedDifference=>"Recorded difference",
        APSourceComparisonOutcome.WithinRecordedLimit=>"Within recorded limit",APSourceComparisonOutcome.ExceedsRecordedLimit=>"Exceeds recorded limit",APSourceComparisonOutcome.Unavailable=>"Unavailable"};
}

/// <summary>Exact recorded comparisons for preparer review. These do not establish matching approval or availability.</summary>
public static class APReceiptAllocationComparisons
{
    public static IReadOnlyList<APSourceComparison> Evaluate(APReceiptAllocationProposalReview review)
    {
        var source=review.Source.Source;var invoice=source.Invoice;
        review.Require(invoice.Company,invoice.ActorUserId,invoice.Invoice.AP_INVOICE_ID,source.InvoiceLineId,review.Proposal);
        var line=invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==source.InvoiceLineId);
        var orderLine=source.Order.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
        var receipt=source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID==review.Proposal.ReceiptId);
        var rows=new List<APSourceComparison>();
        APSourceComparisonOutcome Codes(params string?[] values)=>values.Any(string.IsNullOrWhiteSpace)?APSourceComparisonOutcome.Unavailable:
            values.Distinct(StringComparer.Ordinal).Count()==1?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference;
        string Text(string? value)=>string.IsNullOrWhiteSpace(value)?"Unavailable":value;
        string Quantity(decimal? value)=>value?.ToString(CultureInfo.InvariantCulture) ?? "Unavailable";
        var units=Codes(line.QUANTITY_UOM,orderLine.QUANTITY_UOM,receipt.QUANTITY_UOM,review.Proposal.Unit);
        rows.Add(new("units","Recorded unit codes",units,$"Invoice: {Text(line.QUANTITY_UOM)}; PO line: {Text(orderLine.QUANTITY_UOM)}; receipt: {Text(receipt.QUANTITY_UOM)}; proposal: {review.Proposal.Unit}. No conversion is applied."));
        var currencies=Codes(invoice.Invoice.CURRENCY_CODE,line.CURRENCY_CODE,source.Order.Header.CURRENCY_CODE);
        rows.Add(new("currencies","Recorded currency codes",currencies,$"Invoice header: {Text(invoice.Invoice.CURRENCY_CODE)}; invoice line: {Text(line.CURRENCY_CODE)}; PO header: {Text(source.Order.Header.CURRENCY_CODE)}. No exchange rate is applied."));
        var prices=units==APSourceComparisonOutcome.RecordedAgreement && currencies==APSourceComparisonOutcome.RecordedAgreement &&
            line.UNIT_PRICE is >=0 && orderLine.UNIT_PRICE is >=0?line.UNIT_PRICE==orderLine.UNIT_PRICE?
                APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference:APSourceComparisonOutcome.Unavailable;
        rows.Add(new("prices","Recorded unit prices",prices,$"Invoice: {Quantity(line.UNIT_PRICE)}; PO line: {Quantity(orderLine.UNIT_PRICE)}. Comparison requires exact unit and currency agreement; no tolerance is assumed."));
        var allocations=review.Source.Allocations;
        decimal? invoiceAllocated=null;
        if(units==APSourceComparisonOutcome.RecordedAgreement && allocations.All(row=>row.ACTIVE_IND is "Y" or "N" &&
            (row.ACTIVE_IND!="Y" || row.QUANTITY_UOM==line.QUANTITY_UOM))){
            invoiceAllocated=ExactDecimalArithmetic.SumOrUnavailable(allocations.Where(row=>row.ACTIVE_IND=="Y").Select(row=>row.ALLOCATED_QUANTITY!.Value));
        }
        bool Fits(decimal allocated,decimal quantity,decimal limit)=>allocated<=limit && quantity<=limit-allocated;
        var invoiceLimit=invoiceAllocated.HasValue && line.QUANTITY is >0?Fits(invoiceAllocated.Value,review.Proposal.Quantity,line.QUANTITY.Value)?
            APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit:APSourceComparisonOutcome.Unavailable;
        rows.Add(new("invoice-quantity","Invoice line recorded quantity",invoiceLimit,$"Recorded invoice quantity: {Quantity(line.QUANTITY)}; recorded active line allocations: {Quantity(invoiceAllocated)}; proposal: {Quantity(review.Proposal.Quantity)}. Unconfirmed activity/units or exceeded numeric range prevent arithmetic."));
        var receiptAllocated=review.ReceiptConsumption?.RecordedActiveQuantity;
        var receiptLimit=receiptAllocated.HasValue && receipt.RECEIVED_QUANTITY is >0?Fits(receiptAllocated.Value,review.Proposal.Quantity,receipt.RECEIVED_QUANTITY.Value)?
            APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit:APSourceComparisonOutcome.Unavailable;
        rows.Add(new("receipt-quantity","Receipt recorded quantity",receiptLimit,$"Recorded received quantity: {Quantity(receipt.RECEIVED_QUANTITY)}; recorded active receipt allocations: {Quantity(receiptAllocated)}; proposal: {Quantity(review.Proposal.Quantity)}. This read is not an atomic reservation or available balance."));
        var decision=review.ReceiptConsumption?.VerifiedReceiverDecision;
        var acceptedLimit=units==APSourceComparisonOutcome.RecordedAgreement && receiptAllocated.HasValue && decision is not null?
            Fits(receiptAllocated.Value,review.Proposal.Quantity,decision.AcceptedQuantity)?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit:APSourceComparisonOutcome.Unavailable;
        rows.Add(new("acceptance","Receipt acceptance and verified accepted capacity",acceptedLimit,
            $"Recorded receipt status: {Text(receipt.RECEIPT_STATUS)}; recorded received quantity: {Quantity(receipt.RECEIVED_QUANTITY)}; verified accepted quantity: {Quantity(decision?.AcceptedQuantity)}; verified rejected quantity: {Quantity(decision?.RejectedQuantity)}; recorded active consumption across invoice lines: {Quantity(receiptAllocated)}; proposal: {Quantity(review.Proposal.Quantity)}; decision actor: {Text(decision?.ActorUserId)}. Missing or altered receiver proof, activity or exact units prevent this comparison. Receipt status alone does not establish acceptance. This read is not an atomic reservation or available balance."));
        rows.Add(new("tolerances","Company matching tolerances",APSourceComparisonOutcome.Unavailable,"A reviewed company matching policy has not been established. No default quantity or price tolerance is applied."));
        rows.Add(new("reviewer","Independent reviewer handoff",APSourceComparisonOutcome.Unavailable,"No durable submission or independent reviewer decision has been recorded by this source review."));
        return rows.AsReadOnly();
    }
}
