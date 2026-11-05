#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class EffectiveMatchingPolicyTests
{
    internal static async Task<APInvoiceFinancialReview> Review(decimal invoicePrice=5,decimal poPrice=5,decimal invoiceQuantity=2,decimal poQuantity=2,decimal tolerance=1,string fault="valid")
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();var rules=CompanyMatchingPolicyReview.Rules(native.ReadOwner.Row) with{PriceTolerancePercent=tolerance,QuantityTolerancePercent=tolerance};
        native.ReadOwner.Row.MATCHING_POLICY_JSON=JsonSerializer.Serialize(rules);await native.PrepareDraft();
        native.Command=native.Command with{ExpiresAtUtc=new DateTime(2026,1,31,0,0,0,DateTimeKind.Utc)};await native.Record();await native.PrepareApproval();await native.Record();
        var policies=await native.Owner.GetCompanyMatchingPoliciesAsync(new("tenant","company"),"actor");var rows=policies.Records.ToList();
        if(fault=="unverified")rows[0].MATCHING_POLICY_COMMAND_JSON=null;if(fault=="changed")rows[0].DESCRIPTION="changed";if(fault=="inactive")rows[0].ACTIVE_IND="N";
        if(fault=="draft")rows[0]=CompanyMatchingPolicyProof.Require(policies.Company,rows[0]).Original!;
        if(fault=="ambiguous"){var row=PurchaseReceiptDecisionProof.Copy(rows[0]);row.ACCOUNTING_METHOD_ID="other-policy";rows.Add(row);}
        policies=CompanyMatchingPolicyReview.Create(policies.Company,"actor",rows);
        var original=APInvoiceFinancialReviewTests.Review();var header=PurchaseReceiptDecisionProof.Copy(original.Invoice.Invoice);var line=PurchaseReceiptDecisionProof.Copy(original.Invoice.Lines[0]);
        header.INVOICE_DATE=fault switch{"date"=>null,"zone"=>new DateTime(2026,1,2),"expired"=>new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc),"future"=>new DateTime(2025,12,31,0,0,0,DateTimeKind.Utc),_=>new DateTime(2026,1,2,0,0,0,DateTimeKind.Utc)};
        if(fault=="currency")header.CURRENCY_CODE=line.CURRENCY_CODE="USD";
        line.UNIT_PRICE=invoicePrice;line.QUANTITY=invoiceQuantity;
        var invoice=APInvoiceSourceReview.Create(original.Invoice.Company,"actor",header,[line]);var purchase=original.PurchaseLines[0];
        var orderHeader=PurchaseReceiptDecisionProof.Copy(purchase.Source.Order.Header);orderHeader.ACTIVE_IND="Y";var orderLine=PurchaseReceiptDecisionProof.Copy(purchase.Source.Order.Lines[0]);
        orderLine.ACTIVE_IND=fault=="order-activity"?"N":"Y";orderLine.UNIT_PRICE=poPrice;orderLine.QUANTITY=poQuantity;
        if(fault=="unit")orderLine.QUANTITY_UOM="m3";if(fault=="order-currency")orderHeader.CURRENCY_CODE="USD";
        var order=PurchaseOrderSourceReview.Create(invoice.Company,"actor",orderHeader,[orderLine],purchase.Source.Order.Receipts);
        var allocations=APReceiptAllocationReview.Create(invoice.Company,"actor","invoice","line",invoice.Revision,new(invoice,"line",order),purchase.Allocations);
        return APInvoiceFinancialReview.Create(invoice,[allocations],policies);
    }
    [Fact] public async Task ExactlyOneVerifiedEffectivePolicySuppliesExplicitRulesWithoutFinancialAuthority()
    {
        var review=await Review();Assert.Equal("new-policy",review.PolicySelection.PolicyId);Assert.Equal(1m,review.PolicySelection.Rules!.PriceTolerancePercent);
        Assert.Contains("reviewer: controller",review.PolicySelection.Evidence);Assert.Equal(APSourceComparisonOutcome.RecordedAgreement,review.Checks.Single(row=>row.Code=="policy").Outcome);
        Assert.Equal(APSourceComparisonOutcome.WithinRecordedLimit,review.Checks.Single(row=>row.Code=="policy-price/line").Outcome);
        foreach(var code in new[]{"policy-tax","policy-non-po","acceptance","ledger","decision"})Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code==code).Outcome);
    }
    [Theory][InlineData("date")][InlineData("zone")][InlineData("expired")][InlineData("future")][InlineData("currency")][InlineData("unverified")][InlineData("changed")][InlineData("inactive")][InlineData("draft")][InlineData("ambiguous")]
    public async Task MissingOrAmbiguousPolicyNeverUsesLatestRowOrUnverifiedStatus(string fault)
    {
        var review=await Review(fault:fault);Assert.Null(review.PolicySelection.PolicyId);Assert.Null(review.PolicySelection.Rules);
        Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="policy").Outcome);
        Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="policy-price/line").Outcome);
    }
    [Theory][InlineData(5.05,5,1,true)][InlineData(5.051,5,1,false)][InlineData(4.95,5,1,true)][InlineData(4.949,5,1,false)][InlineData(5,5,0,true)][InlineData(5.001,5,0,false)]
    public async Task PriceAndQuantityToleranceAreExactTwoSidedPOBasisComparisons(decimal observed,decimal basis,decimal tolerance,bool within)
    {
        var review=await Review(observed,basis,observed,basis,tolerance);var expected=within?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit;
        foreach(var code in new[]{"policy-price/line","policy-quantity/line"})Assert.Equal(expected,review.Checks.Single(row=>row.Code==code).Outcome);
        Assert.Contains("full recorded PO line quantity basis",review.Checks.Single(row=>row.Code=="policy-quantity/line").Evidence);
    }
    [Theory][InlineData("unit")][InlineData("order-currency")][InlineData("order-activity")]
    public async Task PolicyDoesNotConvertMissingUnitsCurrencyOrInactiveSources(string fault)
    {
        var review=await Review(fault:fault);Assert.NotNull(review.PolicySelection.Rules);
        foreach(var code in new[]{"policy-price/line","policy-quantity/line"})Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code==code).Outcome);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task PercentageComparisonDoesNotRoundTinyThresholdsOrOverflowDecimalProducts(bool large)
    {
        var value=large?decimal.MaxValue:0.0000000000000000000000000002m;var basis=large?decimal.MaxValue:0.0000000000000000000000000001m;
        var review=await Review(value,basis,value,basis,1);
        Assert.Equal(large?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit,review.Checks.Single(row=>row.Code=="policy-price/line").Outcome);
    }
    [Fact] public async Task ZeroInvoiceQuantityCannotBeAcceptedByAFullPercentageTolerance()
    {
        var review=await Review(invoiceQuantity:0,tolerance:100);
        Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="policy-quantity/line").Outcome);
    }
    [Fact] public async Task ZeroBasisIsUnavailableAndPolicySnapshotCannotChangeUnderAReviewRevision()
    {
        var zero=await Review(poPrice:0,poQuantity:0);foreach(var code in new[]{"policy-price/line","policy-quantity/line"})Assert.Equal(APSourceComparisonOutcome.Unavailable,zero.Checks.Single(row=>row.Code==code).Outcome);
        var review=await Review();review.MatchingPolicies!.Records[0].DESCRIPTION="changed";
        Assert.Throws<InvalidOperationException>(()=>review.Require(review.Invoice.Company,"actor","invoice",review.Invoice.Revision));
    }
}
#endif
