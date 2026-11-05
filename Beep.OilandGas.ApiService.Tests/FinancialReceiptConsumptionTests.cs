#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class FinancialReceiptConsumptionTests
{
    internal static void Seed(APReceiptAllocationCommandTests.Fixture f,string fault="valid")
    {
        f.Invoice.ACTIVE_IND=f.Line.ACTIVE_IND=f.Order.ACTIVE_IND=f.OrderLine.ACTIVE_IND="Y";
        f.Native.Rows.Add(new AP_RECEIPT_ALLOCATION{AP_RECEIPT_ALLOCATION_ID="own-allocation",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"});
        f.AddCompeting(fault=="exceeds"?9m:3m);var competing=f.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor");
        competing.ACTIVE_IND=fault=="activity"?"?":fault=="inactive"?"N":"Y";if(fault=="unit")competing.QUANTITY_UOM="m3";if(fault=="proof")f.Receipt.RECEIVER_DECISION_JSON=null;
    }
    private static async Task<APInvoiceFinancialReview> Review(APReceiptAllocationCommandTests.Fixture f,string fault="valid")
    {
        var scope=new CompanyScope("tenant","company");var invoice=(await f.AP.GetSourceReviewAsync(scope,"invoice","actor"))!;
        var order=(await f.PO.GetSourceReviewAsync(scope,"order","actor"))!;var source=new APLinePurchaseSourceReview(invoice,"line",order);
        var allocations=APReceiptAllocationReview.Create(scope,"actor","invoice","line",invoice.Revision,source,await f.AP.GetReceiptAllocationsForLineAsync(scope,"invoice","line"));
        var consumption=await f.AP.GetReceiptConsumptionAsync(scope,"actor",source,"receipt",f.Authority);
        if(fault=="omit-own")consumption=APReceiptConsumptionReview.Create(scope,"actor",source,"receipt",consumption.Contributors.Where(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID!="invoice").ToArray());
        if(fault=="duplicate")return APInvoiceFinancialReview.Create(invoice,[allocations],consumption:[consumption,consumption]);
        return APInvoiceFinancialReview.Create(invoice,[allocations],consumption:fault=="missing"?[]:[consumption]);
    }
    [Theory][InlineData("valid",APSourceComparisonOutcome.WithinRecordedLimit,5)][InlineData("exceeds",APSourceComparisonOutcome.ExceedsRecordedLimit,11)][InlineData("inactive",APSourceComparisonOutcome.WithinRecordedLimit,2)]
    [InlineData("activity",APSourceComparisonOutcome.Unavailable,-1)][InlineData("unit",APSourceComparisonOutcome.Unavailable,-1)][InlineData("proof",APSourceComparisonOutcome.Unavailable,5)]
    public async Task FullNativeContributorsAreComparedWithVerifiedAcceptance(string fault,APSourceComparisonOutcome outcome,int quantity)
    {
        var f=new APReceiptAllocationCommandTests.Fixture();Seed(f,fault);var review=await Review(f);
        Assert.Equal(2,review.ReceiptConsumption![0].Contributors.Count);Assert.Equal(quantity<0?null:(decimal?)quantity,review.ReceiptConsumption[0].RecordedActiveQuantity);
        Assert.Equal(outcome,review.ReceiptConsumptionChecks.Single().Outcome);Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="decision").Outcome);
        Assert.Equal(0,f.Native.Writes);Assert.Equal("DRAFT",f.Invoice.STATUS);
    }
    [Theory][InlineData("omit-own")][InlineData("missing")][InlineData("duplicate")]
    public async Task FinancialAssemblyRefusesIncompleteOrAmbiguousReceiptSets(string fault)
    {var f=new APReceiptAllocationCommandTests.Fixture();Seed(f);await Assert.ThrowsAsync<InvalidOperationException>(()=>Review(f,fault));}
    [Fact] public async Task OtherInvoiceAllocationChangesWholeFinancialRevisionAndTamperingIsRejected()
    {
        var f=new APReceiptAllocationCommandTests.Fixture();Seed(f);var initial=await Review(f);f.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        var closing=await Review(f);Assert.Equal(initial.Invoice.Revision,closing.Invoice.Revision);Assert.NotEqual(initial.Revision,closing.Revision);
        closing.ReceiptConsumption![0].Contributors[0].Allocations[0].ALLOCATED_QUANTITY=99m;
        Assert.Throws<InvalidOperationException>(()=>closing.Require(closing.Invoice.Company,"actor","invoice",closing.Invoice.Revision));
    }
    [Fact] public async Task SharedReceiptAcrossOwnInvoiceLinesRequiresOneConsistentGlobalHistory()
    {
        var f=new APReceiptAllocationCommandTests.Fixture();Seed(f);var secondLine=PurchaseReceiptDecisionProof.Copy(f.Line);secondLine.AP_INVOICE_LINE_ID="line-two";f.Native.Rows.Add(secondLine);
        var secondAllocation=PurchaseReceiptDecisionProof.Copy(f.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="own-allocation"));secondAllocation.AP_INVOICE_LINE_ID="line-two";secondAllocation.AP_RECEIPT_ALLOCATION_ID="own-two";f.Native.Rows.Add(secondAllocation);
        var scope=new CompanyScope("tenant","company");var invoice=(await f.AP.GetSourceReviewAsync(scope,"invoice","actor"))!;var order=(await f.PO.GetSourceReviewAsync(scope,"order","actor"))!;
        var purchases=new List<APReceiptAllocationReview>();var receipts=new List<APReceiptConsumptionReview>();
        foreach(var line in invoice.Lines)
        {
            var source=new APLinePurchaseSourceReview(invoice,line.AP_INVOICE_LINE_ID,order);
            purchases.Add(APReceiptAllocationReview.Create(scope,"actor","invoice",line.AP_INVOICE_LINE_ID,invoice.Revision,source,await f.AP.GetReceiptAllocationsForLineAsync(scope,"invoice",line.AP_INVOICE_LINE_ID)));
            receipts.Add(await f.AP.GetReceiptConsumptionAsync(scope,"actor",source,"receipt",f.Authority));
        }
        var review=APInvoiceFinancialReview.Create(invoice,purchases,consumption:receipts);Assert.Equal(2,review.ReceiptConsumptionChecks.Count);Assert.All(review.ReceiptConsumption!,row=>Assert.Equal(7m,row.RecordedActiveQuantity));
        f.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        receipts[1]=await f.AP.GetReceiptConsumptionAsync(scope,"actor",purchases[1].Source,"receipt",f.Authority);
        Assert.Throws<InvalidOperationException>(()=>APInvoiceFinancialReview.Create(invoice,purchases,consumption:receipts));
    }
    [Fact] public async Task ChangedOwnAllocationCannotHideInsideOtherwiseValidGlobalEvidence()
    {
        var f=new APReceiptAllocationCommandTests.Fixture();Seed(f);var review=await Review(f);var receipt=review.ReceiptConsumption![0];
        var own=receipt.Contributors.Single(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID=="invoice");var changed=PurchaseReceiptDecisionProof.Copy(own.Allocations[0]);changed.ALLOCATED_QUANTITY=1m;
        var wrong=APReceiptAllocationReview.Create(review.Invoice.Company,"actor","invoice","line",review.Invoice.Revision,own.Source,[changed]);
        var consumption=APReceiptConsumptionReview.Create(review.Invoice.Company,"actor",receipt.Source,"receipt",[wrong,receipt.Contributors.Single(row=>row.Source.Invoice.Invoice.AP_INVOICE_ID=="second")]);
        Assert.Throws<InvalidOperationException>(()=>APInvoiceFinancialReview.Create(review.Invoice,review.PurchaseLines,consumption:[consumption]));
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ExistingFinancialApiAndTypedClientRequireStableGlobalHistory(bool changed)
    {
        var f=new CompanyMatchingPolicyCommandTests.Fixture();var native=f.ReadOwner.Native;Seed(native,"exceeds");
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Owner);services.AddSingleton(native.AP);services.AddSingleton(native.PO);});
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var invoice=(await client.GetAPInvoiceAsync("invoice"))!;
        if(changed)
        {
            var reads=0;native.Native.Source.Setup(source=>source.GetEntityAsync("AP_RECEIPT_ALLOCATION",It.Is<List<AppFilter>>(filters=>filters.Any(row=>row.FieldName=="PO_RECEIPT_ID")))).ReturnsAsync(()=>
            {
                var rows=native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Select(row=>PurchaseReceiptDecisionProof.Copy(row)).ToArray();
                if(++reads==1)native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=8m;
                return rows.Cast<object>().ToArray();
            });
            var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPInvoiceFinancialReviewAsync("invoice",invoice.Revision));Assert.Equal(HttpStatusCode.Conflict,failure.StatusCode);
        }
        else
        {
            var review=await client.GetAPInvoiceFinancialReviewAsync("invoice",invoice.Revision);Assert.Equal(2,review.ReceiptConsumption![0].Contributors.Count);
            Assert.Equal(APSourceComparisonOutcome.ExceedsRecordedLimit,review.ReceiptConsumptionChecks.Single().Outcome);
        }
        Assert.Equal(0,native.Native.Writes);
    }
}
#endif
