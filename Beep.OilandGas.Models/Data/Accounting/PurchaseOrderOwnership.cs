using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Explicit company ownership of the existing purchase-order header; vendor identity is a separate relationship.</summary>
public static class PurchaseOrderOwnership
{
    public static void RequireScope(CompanyScope company)
    {
        if(company is null || string.IsNullOrWhiteSpace(company.TenantId) || company.TenantId.Length>64 ||
            string.IsNullOrWhiteSpace(company.BusinessAssociateId) || company.BusinessAssociateId.Length>128)
            throw new InvalidOperationException("An explicit owning tenant and company are required.");
    }
    public static void Require(CompanyScope company,string id,PURCHASE_ORDER? order)
    {
        RequireScope(company);
        if(string.IsNullOrWhiteSpace(id) || order is null || order.PURCHASE_ORDER_ID!=id ||
            order.OWNER_TENANT_ID!=company.TenantId || order.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId)
            throw new InvalidOperationException("The recorded purchase order does not establish the selected owning company.");
    }
    public static void RequireReceipts(string id,IReadOnlyList<PO_RECEIPT>? receipts)
    {
        if(string.IsNullOrWhiteSpace(id) || receipts is null || receipts.Any(row=>row is null ||
            string.IsNullOrWhiteSpace(row.PO_RECEIPT_ID) || row.PURCHASE_ORDER_ID!=id || string.IsNullOrWhiteSpace(row.PO_LINE_ITEM_ID)) ||
            receipts.Select(row=>row.PO_RECEIPT_ID).Distinct(StringComparer.Ordinal).Count()!=receipts.Count)
            throw new InvalidOperationException("The recorded receipts cannot be confirmed for this purchase order.");
    }
    public static void RequireRows(CompanyScope company,IReadOnlyList<PURCHASE_ORDER>? rows)
    {
        RequireScope(company);
        if(rows is null)throw new InvalidOperationException("The recorded company purchase-order list is unavailable.");
        foreach(var row in rows)
        {
            Require(company,row?.PURCHASE_ORDER_ID ?? "",row);
            if(row!.ACTIVE_IND!="Y")throw new InvalidOperationException("The returned order does not match the active company query.");
        }
        if(rows.Select(row=>row.PURCHASE_ORDER_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Recorded purchase-order identities are ambiguous.");
    }
    public static void RequireVendorRows(CompanyScope company,string vendor,IReadOnlyList<PURCHASE_ORDER>? rows)
    {
        if(string.IsNullOrWhiteSpace(vendor))throw new InvalidOperationException("The recorded supplier purchase-order list is unavailable.");
        RequireRows(company,rows);
        if(rows!.Any(row=>row.VENDOR_BA_ID!=vendor))throw new InvalidOperationException("The returned order does not match the active supplier query.");
    }
    public static void RequireLines(string id,IReadOnlyList<PO_LINE_ITEM>? lines)
    {
        if(string.IsNullOrWhiteSpace(id) || lines is null || lines.Any(row=>row is null ||
            string.IsNullOrWhiteSpace(row.PO_LINE_ITEM_ID) || row.PURCHASE_ORDER_ID!=id) ||
            lines.Select(row=>row.PO_LINE_ITEM_ID).Distinct(StringComparer.Ordinal).Count()!=lines.Count)
            throw new InvalidOperationException("Recorded purchase-order lines cannot be confirmed for this order.");
    }
}
