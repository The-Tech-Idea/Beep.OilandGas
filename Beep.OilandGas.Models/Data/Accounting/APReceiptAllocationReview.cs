using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded allocations with exact source membership; not matching approval or quantity reconciliation.</summary>
public sealed record APReceiptAllocationReview(APLinePurchaseSourceReview Source,IReadOnlyList<AP_RECEIPT_ALLOCATION> Allocations,
    string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static void RequireOwnership(CompanyScope company,IReadOnlyList<AP_RECEIPT_ALLOCATION> rows)
    {
        APInvoiceOwnership.RequireScope(company);
        if(rows is null || rows.Any(row=>row is null || row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId))
            throw new InvalidOperationException("Recorded receipt allocation ownership is unavailable or belongs to another company.");
    }
    public static void RequireRows(string invoiceId,string lineId,IReadOnlyList<AP_RECEIPT_ALLOCATION>? rows)
    {
        if(string.IsNullOrWhiteSpace(invoiceId) || string.IsNullOrWhiteSpace(lineId) || rows is null)
            throw new InvalidOperationException("Recorded receipt allocations are unavailable.");
        foreach(var row in rows)
        {
            if(row is null || row.AP_INVOICE_ID!=invoiceId || row.AP_INVOICE_LINE_ID!=lineId ||
                string.IsNullOrWhiteSpace(row.AP_RECEIPT_ALLOCATION_ID) || string.IsNullOrWhiteSpace(row.PURCHASE_ORDER_ID) ||
                string.IsNullOrWhiteSpace(row.PO_LINE_ITEM_ID) || string.IsNullOrWhiteSpace(row.PO_RECEIPT_ID))
                throw new InvalidOperationException("An allocation has no confirmed invoice-line or receipt identity.");
            foreach(var value in new[]{row.AP_RECEIPT_ALLOCATION_ID,row.AP_INVOICE_ID,row.AP_INVOICE_LINE_ID,row.PURCHASE_ORDER_ID,row.PO_LINE_ITEM_ID,row.PO_RECEIPT_ID})
                if(value.Length>128 || value.Any(char.IsControl))throw new InvalidOperationException("An allocation identity exceeds its source contract.");
            if(row.ALLOCATED_QUANTITY is not >0 || string.IsNullOrWhiteSpace(row.QUANTITY_UOM) ||
                row.QUANTITY_UOM.Length>64 || row.QUANTITY_UOM.Any(char.IsControl))
                throw new InvalidOperationException("Recorded allocation quantity and unit must be explicit and positive.");
        }
        if(rows.Select(row=>row.AP_RECEIPT_ALLOCATION_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Recorded allocation identities are ambiguous.");
    }
    public static APReceiptAllocationReview Create(CompanyScope company,string actor,string invoiceId,string lineId,string invoiceRevision,
        APLinePurchaseSourceReview source,IReadOnlyList<AP_RECEIPT_ALLOCATION> rows)
    {
        RequireRows(invoiceId,lineId,rows);
        var frozenSource=JsonSerializer.Deserialize<APLinePurchaseSourceReview>(JsonSerializer.Serialize(source))!;
        var frozenRows=Array.AsReadOnly(rows.OrderBy(row=>row.AP_RECEIPT_ALLOCATION_ID,StringComparer.Ordinal)
            .Select(row=>JsonSerializer.Deserialize<AP_RECEIPT_ALLOCATION>(JsonSerializer.Serialize(row))!).ToArray());
        var review=new APReceiptAllocationReview(frozenSource,frozenRows,Digest(frozenSource,frozenRows),DateTimeOffset.UtcNow);
        review.Require(company,actor,invoiceId,lineId,invoiceRevision);return review;
    }
    public void Require(CompanyScope company,string actor,string invoiceId,string lineId,string invoiceRevision)
    {
        if(Source is null)throw new InvalidOperationException("The allocation source is unavailable.");
        var orderId=Source.RequireOrderId(company,actor,invoiceId,lineId,invoiceRevision);
        RequireOwnership(company,Allocations);
        RequireRows(invoiceId,lineId,Allocations);
        var line=Source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==lineId);
        foreach(var row in Allocations)
            if(row.PURCHASE_ORDER_ID!=orderId || row.PO_LINE_ITEM_ID!=line.PO_LINE_ITEM_ID ||
                !Source.Order.Receipts.Any(receipt=>receipt.PO_RECEIPT_ID==row.PO_RECEIPT_ID && receipt.PO_LINE_ITEM_ID==row.PO_LINE_ITEM_ID))
                throw new InvalidOperationException("The allocation does not resolve to this owned order line and receipt.");
        if(!ProcessDefinitionRevision.IsValid(Revision) || Revision!=Digest(Source,Allocations) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The recorded allocation content changed or its provenance is unavailable.");
    }
    private static string Digest(APLinePurchaseSourceReview source,IReadOnlyList<AP_RECEIPT_ALLOCATION> rows)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new {source.Invoice.Revision,OrderRevision=source.Order.Revision,source.InvoiceLineId,
            Rows=rows.OrderBy(row=>row.AP_RECEIPT_ALLOCATION_ID,StringComparer.Ordinal).Select(row=>new{
                row.OWNER_TENANT_ID,row.OWNER_COMPANY_BA_ID,row.AP_RECEIPT_ALLOCATION_ID,row.AP_INVOICE_ID,row.AP_INVOICE_LINE_ID,row.PURCHASE_ORDER_ID,row.PO_LINE_ITEM_ID,row.PO_RECEIPT_ID,
                row.ALLOCATED_QUANTITY,row.QUANTITY_UOM,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE,
                row.PPDM_GUID,row.SOURCE,row.REMARK,row.REVIEW_COMMAND_JSON,row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.ROW_EFFECTIVE_DATE,row.ROW_EXPIRY_DATE,row.ROW_QUALITY})}));
}
