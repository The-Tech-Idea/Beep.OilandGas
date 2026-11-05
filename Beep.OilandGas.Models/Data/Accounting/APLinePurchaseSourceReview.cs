using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Owned AP/PO source relationship evidence, not receipt allocation or a matching decision.</summary>
public sealed record APLinePurchaseSourceReview(APInvoiceSourceReview Invoice,string InvoiceLineId,PurchaseOrderSourceReview Order)
{
    public string RequireOrderId(CompanyScope company,string actor,string invoiceId,string lineId,string expectedInvoiceRevision)
    {
        if(Invoice is null || Order is null || InvoiceLineId!=lineId || !ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))
            throw new InvalidOperationException("The AP purchase source review is incomplete.");
        Invoice.Require(company,actor,invoiceId);
        if(Invoice.Revision!=expectedInvoiceRevision)throw new InvalidOperationException("The recorded AP source changed. Reload the invoice before reviewing its order.");
        var line=Invoice.Lines.SingleOrDefault(row=>row.AP_INVOICE_LINE_ID==lineId)
            ?? throw new InvalidOperationException("The recorded invoice line is unavailable.");
        if(string.IsNullOrWhiteSpace(line.PURCHASE_ORDER_ID) || string.IsNullOrWhiteSpace(line.PO_LINE_ITEM_ID))
            throw new InvalidOperationException("The invoice line has no complete recorded PO reference.");
        Order.Require(company,actor,line.PURCHASE_ORDER_ID);
        if(!Order.Lines.Any(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID) || string.IsNullOrWhiteSpace(Invoice.Invoice.VENDOR_BA_ID) ||
            Invoice.Invoice.VENDOR_BA_ID!=Order.Header.VENDOR_BA_ID)
            throw new InvalidOperationException("The recorded AP reference does not resolve to this supplier's owned order line.");
        return line.PURCHASE_ORDER_ID;
    }
}
