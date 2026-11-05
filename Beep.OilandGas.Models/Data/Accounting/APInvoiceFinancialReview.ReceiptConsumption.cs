using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Globalization;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed partial record APInvoiceFinancialReview
{
    private void RequireReceiptConsumption(CompanyScope company,string actor)
    {
        if(ReceiptConsumption is null)return; // Source-only diagnostics explicitly report unavailable evidence.
        var expected=PurchaseLines.SelectMany(line=>line.Allocations.Select(row=>(line.Source.InvoiceLineId,ReceiptId:row.PO_RECEIPT_ID))).Distinct().OrderBy(row=>row.InvoiceLineId,StringComparer.Ordinal).ThenBy(row=>row.ReceiptId,StringComparer.Ordinal).ToArray();
        if(ReceiptConsumption.Any(row=>row?.Source is null) || !expected.SequenceEqual(ReceiptConsumption.Select(row=>(row.Source.InvoiceLineId,row.ReceiptId)).OrderBy(row=>row.InvoiceLineId,StringComparer.Ordinal).ThenBy(row=>row.ReceiptId,StringComparer.Ordinal)))
            throw new InvalidOperationException("Complete receipt consumption for every recorded allocation is required.");
        foreach(var receipt in ReceiptConsumption)
        {
            var purchase=PurchaseLines.Single(row=>row.Source.InvoiceLineId==receipt.Source.InvoiceLineId);
            receipt.Require(company,actor,purchase.Source,receipt.ReceiptId);
            var own=receipt.Contributors.Where(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID==Invoice.Invoice.AP_INVOICE_ID && row.Source.InvoiceLineId==purchase.Source.InvoiceLineId).SelectMany(row=>row.Allocations);
            var expectedRows=purchase.Allocations.Where(row=>row.PO_RECEIPT_ID==receipt.ReceiptId);
            string Rows(IEnumerable<global::Beep.OilandGas.Models.Data.ProductionAccounting.AP_RECEIPT_ALLOCATION> rows)=>JsonSerializer.Serialize(rows.OrderBy(row=>row.AP_RECEIPT_ALLOCATION_ID,StringComparer.Ordinal));
            if(Rows(own)!=Rows(expectedRows))throw new InvalidOperationException("The receipt contributor history omits or changes this invoice's recorded allocations.");
        }
        foreach(var group in ReceiptConsumption.GroupBy(row=>row.ReceiptId,StringComparer.Ordinal))
        {
            string History(APReceiptConsumptionReview row)=>JsonSerializer.Serialize(new{row.Source.Order.Revision,Contributors=row.Contributors.Select(item=>item.Revision).Order(StringComparer.Ordinal)});
            if(group.Select(History).Distinct(StringComparer.Ordinal).Count()!=1)throw new InvalidOperationException("Shared receipt contributor evidence changed across invoice lines.");
        }
    }
    public IReadOnlyList<APSourceComparison> ReceiptConsumptionChecks
    {
        get
        {
            Require(Invoice.Company,Invoice.ActorUserId,Invoice.Invoice.AP_INVOICE_ID,Invoice.Revision);
            if(ReceiptConsumption is not {Count:>0})return [new("global-consumption","Complete receipt consumption",APSourceComparisonOutcome.Unavailable,"No allocated receipt contributor evidence is available. This does not establish an available receipt balance.")];
            return ReceiptConsumption.Select(receipt=>
            {
                var recorded=receipt.RecordedActiveQuantity;var accepted=receipt.VerifiedAcceptedQuantity;
                var unit=receipt.Source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID==receipt.ReceiptId).QUANTITY_UOM;
                var outcome=recorded is null || accepted is null?APSourceComparisonOutcome.Unavailable:recorded<=accepted?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit;
                string Number(decimal? value)=>value?.ToString(CultureInfo.InvariantCulture)??"Unavailable";
                return new APSourceComparison("global-consumption/"+receipt.Source.InvoiceLineId+"/"+receipt.ReceiptId,"All invoice allocations for receipt "+receipt.ReceiptId,outcome,
                    $"Owning invoice line: {receipt.Source.InvoiceLineId}; all active allocations: {Number(recorded)}; verified accepted quantity: {Number(accepted)}; unit: {unit ?? "Unavailable"}; contributor invoice/line groups: {receipt.Contributors.Count}. Includes inactive history. Unknown activity, incompatible units or missing receiver proof is unavailable. This diagnostic is not a reservation, stock balance or financial approval.");
            }).ToArray();
        }
    }
}
