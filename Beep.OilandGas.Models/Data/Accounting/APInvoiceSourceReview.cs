using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

public static class APInvoiceOwnership
{
    public static void RequireScope(CompanyScope company)=>PurchaseOrderOwnership.RequireScope(company);
    public static void Require(CompanyScope company,string id,AP_INVOICE? invoice)
    {
        RequireScope(company);
        if(string.IsNullOrWhiteSpace(id) || invoice is null || invoice.AP_INVOICE_ID!=id ||
            invoice.OWNER_TENANT_ID!=company.TenantId || invoice.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId)
            throw new InvalidOperationException("The recorded AP invoice does not belong to the original company.");
        if(invoice.CURRENCY_CODE is {} currency && (currency.Length>32 || currency.Any(char.IsControl)))
            throw new InvalidOperationException("The recorded AP currency exceeds its source contract.");
        if(invoice.VARIANCE_PROPOSAL_JSON?.Length>APInvoiceVarianceProof.MaximumLength)
            throw new InvalidOperationException("The recorded invoice variance proof exceeds its bounded source contract.");
        if(invoice.VARIANCE_DECISION_JSON?.Length>APInvoiceVarianceDecisionProof.MaximumLength)
            throw new InvalidOperationException("The recorded controller disposition exceeds its bounded source contract.");
        if(invoice.CORRECTION_HISTORY_JSON?.Length>APInvoiceCorrectionHistory.MaximumLength)
            throw new InvalidOperationException("The recorded correction history exceeds its bounded source contract.");
    }
    public static void RequireLines(string invoice,IReadOnlyList<AP_INVOICE_LINE>? lines)
    {
        if(string.IsNullOrWhiteSpace(invoice) || lines is null)throw new InvalidOperationException("The recorded AP line set is unavailable.");
        foreach(var line in lines)
        {
            if(line is null || string.IsNullOrWhiteSpace(line.AP_INVOICE_LINE_ID) || line.AP_INVOICE_ID!=invoice)
                throw new InvalidOperationException("An AP line has no confirmed invoice membership.");
            if(string.IsNullOrWhiteSpace(line.PURCHASE_ORDER_ID)!=string.IsNullOrWhiteSpace(line.PO_LINE_ITEM_ID))
                throw new InvalidOperationException("The recorded PO reference is incomplete.");
            if(line.ITEM_DESCRIPTION?.Length>1024)throw new InvalidOperationException("The recorded AP line description exceeds its source contract.");
            foreach(var (value,bound) in new[]{(line.AP_INVOICE_LINE_ID,128),(line.AP_INVOICE_ID,128),
                (line.QUANTITY_UOM,64),(line.CURRENCY_CODE,32),(line.PURCHASE_ORDER_ID,128),(line.PO_LINE_ITEM_ID,128)})
                if(value is not null && (value.Length>bound || value.Any(char.IsControl)))throw new InvalidOperationException("A recorded AP line value exceeds its source contract.");
        }
        if(lines.Select(line=>line.AP_INVOICE_LINE_ID).Distinct(StringComparer.Ordinal).Count()!=lines.Count)
            throw new InvalidOperationException("The AP line set contains ambiguous identities.");
    }
    public static void RequireRows(CompanyScope company,IReadOnlyList<AP_INVOICE>? rows)
    {
        RequireScope(company);
        if(rows is null)throw new InvalidOperationException("The AP company source list is unavailable.");
        foreach(var row in rows)
        {
            Require(company,row?.AP_INVOICE_ID ?? "",row);
            if(row!.ACTIVE_IND!="Y")throw new InvalidOperationException("The recorded AP invoice does not match this active company query.");
        }
        if(rows.Select(row=>row.AP_INVOICE_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("The AP source list contains ambiguous identities.");
    }
    public static void RequireVendorRows(CompanyScope company,string vendor,IReadOnlyList<AP_INVOICE>? rows)
    {
        if(string.IsNullOrWhiteSpace(vendor))throw new InvalidOperationException("The AP supplier source list is unavailable.");
        RequireRows(company,rows);
        if(rows!.Any(row=>row.VENDOR_BA_ID!=vendor))throw new InvalidOperationException("The recorded AP invoice does not match this active supplier query.");
    }
}

/// <summary>Recorded header evidence only; not matching, payment verification or posting confirmation.</summary>
public sealed partial record APInvoiceSourceReview(CompanyScope Company,string ActorUserId,AP_INVOICE Invoice,IReadOnlyList<AP_INVOICE_LINE> Lines,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static APInvoiceSourceReview Create(CompanyScope company,string actor,AP_INVOICE invoice,IReadOnlyList<AP_INVOICE_LINE> lines)
    {
        APInvoiceOwnership.Require(company,invoice?.AP_INVOICE_ID ?? "",invoice);
        var frozen=JsonSerializer.Deserialize<AP_INVOICE>(JsonSerializer.Serialize(invoice))!;
        APInvoiceOwnership.RequireLines(invoice!.AP_INVOICE_ID,lines);
        var frozenLines=Array.AsReadOnly(lines.OrderBy(row=>row.AP_INVOICE_LINE_ID,StringComparer.Ordinal).Select(row=>JsonSerializer.Deserialize<AP_INVOICE_LINE>(JsonSerializer.Serialize(row))!).ToArray());
        var review=new APInvoiceSourceReview(company,actor,frozen,frozenLines,Digest(company,frozen,frozenLines),DateTimeOffset.UtcNow);
        review.Require(company,actor,invoice!.AP_INVOICE_ID);return review;
    }
    public void Require(CompanyScope company,string actor,string id)
    {
        APInvoiceOwnership.Require(company,id,Invoice);
        APInvoiceOwnership.RequireLines(id,Lines);
        if(Invoice.CORRECTION_HISTORY_JSON is not null)_=APInvoiceCorrectionHistory.Require(company,Invoice,Lines);
        if(Company!=company || string.IsNullOrWhiteSpace(actor) || ActorUserId!=actor || !ProcessDefinitionRevision.IsValid(Revision) ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero || Revision!=Digest(company,Invoice,Lines))
            throw new InvalidOperationException("The AP source review does not match the original company, actor and recorded content.");
    }
    internal static string CorrectionSourceRevision(CompanyScope company,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines)=>Digest(company,row,lines);
    private static string Digest(CompanyScope company,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines)
    {
        var source=LegacyDigest(company,row,lines);
        return row.CORRECTION_HISTORY_JSON is null?source:ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{RecordedSource=source,CorrectionHistory=row.CORRECTION_HISTORY_JSON}));
    }
    private static string LegacyDigest(CompanyScope company,AP_INVOICE row,IReadOnlyList<AP_INVOICE_LINE> lines)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        company,row.AP_INVOICE_ID,row.OWNER_TENANT_ID,row.OWNER_COMPANY_BA_ID,row.INVOICE_NUMBER,row.VENDOR_BA_ID,
        row.INVOICE_DATE,row.DUE_DATE,row.TOTAL_AMOUNT,row.PAID_AMOUNT,row.BALANCE_DUE,row.CURRENCY_CODE,row.STATUS,row.VARIANCE_PROPOSAL_JSON,row.ROW_ID,
        row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE,row.ROW_EFFECTIVE_DATE,
        row.ROW_EXPIRY_DATE,row.ROW_QUALITY,row.PPDM_GUID,row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.SOURCE,row.REMARK,Lines=lines.OrderBy(line=>line.AP_INVOICE_LINE_ID,StringComparer.Ordinal).Select(line=>new{
            line.AP_INVOICE_LINE_ID,line.AP_INVOICE_ID,line.LINE_NUMBER,line.ITEM_DESCRIPTION,line.QUANTITY,line.QUANTITY_UOM,line.UNIT_PRICE,
            line.LINE_TOTAL,line.CURRENCY_CODE,line.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,line.ACTIVE_IND,line.ROW_CREATED_BY,line.ROW_CREATED_DATE,
            line.ROW_CHANGED_BY,line.ROW_CHANGED_DATE,line.ROW_EFFECTIVE_DATE,line.ROW_EXPIRY_DATE,line.ROW_QUALITY,line.PPDM_GUID,
            line.EFFECTIVE_DATE,line.EXPIRY_DATE,line.SOURCE,line.REMARK})}));
}
