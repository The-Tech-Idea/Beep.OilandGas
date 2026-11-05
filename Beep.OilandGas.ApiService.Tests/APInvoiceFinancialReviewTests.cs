using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.App;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class APInvoiceFinancialReviewTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    internal static APInvoiceFinancialReview Review(string fault="valid")
    {
        var date=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);
        AP_INVOICE header=new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",STATUS="DRAFT",ACTIVE_IND="Y",CURRENCY_CODE="KWD",TOTAL_AMOUNT=10m,ROW_CREATED_BY="preparer",ROW_CREATED_DATE=date};
        AP_INVOICE_LINE line=new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",ACTIVE_IND="Y",QUANTITY=2m,QUANTITY_UOM="bbl",UNIT_PRICE=5m,LINE_TOTAL=10m,CURRENCY_CODE="KWD",ROW_CREATED_BY="preparer",ROW_CREATED_DATE=date};
        switch(fault){case "total":header.TOTAL_AMOUNT=11m;break;case "currency":line.CURRENCY_CODE="USD";break;case "activity":line.ACTIVE_IND="?";break;
            case "maker":header.ROW_CREATED_BY=line.ROW_CREATED_BY="actor";break;case "edited":header.ROW_CHANGED_BY="editor";header.ROW_CHANGED_DATE=date.AddDays(1);break;
            case "amount":line.LINE_TOTAL=9m;break;case "overflow":line.QUANTITY=decimal.MaxValue;break;case "no-po":line.PURCHASE_ORDER_ID=line.PO_LINE_ITEM_ID=null;break;}
        var invoice=APInvoiceSourceReview.Create(Scope,"actor",header,[line]);
        if(fault=="no-po")return APInvoiceFinancialReview.Create(invoice,[]);
        var order=PurchaseOrderSourceReview.Create(Scope,"actor",new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",CURRENCY_CODE="KWD"},
            [new(){PO_LINE_ITEM_ID="po-line",PURCHASE_ORDER_ID="order",UNIT_PRICE=fault=="price"?6m:5m,QUANTITY_UOM=fault=="unit"?"m3":"bbl"}],
            [new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",RECEIVED_QUANTITY=2m,QUANTITY_UOM="bbl"}]);
        var allocation=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",invoice.Revision,new(invoice,"line",order),
            [new(){AP_RECEIPT_ALLOCATION_ID="allocation",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=fault=="coverage"?1m:2m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"}]);
        return APInvoiceFinancialReview.Create(invoice,[allocation]);
    }
    [Theory][InlineData("valid","invoice-total",APSourceComparisonOutcome.RecordedAgreement)]
    [InlineData("total","invoice-total",APSourceComparisonOutcome.RecordedDifference)][InlineData("currency","invoice-total",APSourceComparisonOutcome.Unavailable)]
    [InlineData("activity","activity",APSourceComparisonOutcome.Unavailable)][InlineData("maker","maker",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("edited","maker",APSourceComparisonOutcome.Unavailable)][InlineData("price","price/line",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("unit","price/line",APSourceComparisonOutcome.Unavailable)][InlineData("coverage","allocation/line",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("amount","amount/line",APSourceComparisonOutcome.RecordedDifference)][InlineData("overflow","amount/line",APSourceComparisonOutcome.Unavailable)]
    [InlineData("no-po","purchase/line",APSourceComparisonOutcome.Unavailable)]
    public void ExactRecordedDiagnosticsDoNotCreateFinancialApproval(string fault,string code,APSourceComparisonOutcome expected)
    {
        var review=Review(fault);review.Require(Scope,"actor","invoice",review.Invoice.Revision);Assert.Equal(expected,review.Checks.Single(row=>row.Code==code).Outcome);
        foreach(var missing in new[]{"acceptance","policy","ledger","decision"})Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code==missing).Outcome);
    }
    [Theory][InlineData("missing-line")][InlineData("duplicate-line")][InlineData("revision")][InlineData("actor")]
    public void PartialChangedOrForeignReviewCannotBeAdopted(string fault)
    {
        var review=Review();if(fault=="missing-line")review=review with{PurchaseLines=[]};if(fault=="duplicate-line")review=review with{PurchaseLines=[review.PurchaseLines[0],review.PurchaseLines[0]]};if(fault=="revision")review=review with{Revision=new string('C',64)};
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,fault=="actor"?"other":"actor","invoice",review.Invoice.Revision));
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task WorkspaceRejectsLateReviewAfterCompanyOrSourceInvalidation(bool companyChange)
    {
        var review=Review();using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(Scope,["Accountant"],["Accounting.View"]));
        using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));
        var pending=new TaskCompletionSource<APInvoiceFinancialReview>();var loading=state.LoadFinancialReviewAsync((_,_)=>pending.Task);
        if(companyChange)company.Clear();else state.ClearPurchaseSource();pending.SetResult(review);await loading;Assert.Null(state.FinancialReview);
    }
    [Fact] public async Task WorkspaceProtectsFinancialEvidenceAndClearsFailedReload()
    {
        var review=Review();using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(Scope,["Accountant"],["Accounting.View"]));
        using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));await state.LoadFinancialReviewAsync((_,_)=>Task.FromResult(review));
        state.FinancialReview!.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(10m,state.FinancialReview!.Invoice.Invoice.TOTAL_AMOUNT);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadFinancialReviewAsync((_,_)=>throw new InvalidOperationException("Provider unavailable")));Assert.Null(state.FinancialReview);
    }
}
