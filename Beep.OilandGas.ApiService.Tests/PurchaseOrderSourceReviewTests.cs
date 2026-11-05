using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseOrderSourceReviewTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static PURCHASE_ORDER Header()=>new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",PO_NUMBER="PO-1",TOTAL_AMOUNT=12m};
    private static PO_LINE_ITEM Line(string id="line")=>new(){PO_LINE_ITEM_ID=id,PURCHASE_ORDER_ID="order",LINE_NUMBER=1,QUANTITY=3m,UNIT_PRICE=4m,LINE_TOTAL=12m,ITEM_DESCRIPTION="Recorded item"};
    private static PO_RECEIPT Receipt()=>new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="line",RECEIVED_QUANTITY=2m,RECEIVED_BY="receiver",RECEIPT_STATUS="RECEIVED",ACTIVE_IND="N"};
    private static PurchaseOrderSourceReview Review()=>PurchaseOrderSourceReview.Create(Company,"buyer",Header(),[Line()],[Receipt()]);
    [Fact] public void ReviewFreezesSourcesAndPreservesInactiveReceiptAndNullableValues()
    {
        var header=Header();var line=Line();var receipt=Receipt();receipt.RECEIVED_QUANTITY=null;
        var review=PurchaseOrderSourceReview.Create(Company,"buyer",header,[line],[receipt]);
        header.PO_NUMBER="changed";line.QUANTITY=100m;receipt.RECEIVED_BY="another";
        Assert.Equal("PO-1",review.Header.PO_NUMBER);Assert.Equal(3m,Assert.Single(review.Lines).QUANTITY);
        Assert.Null(Assert.Single(review.Receipts).RECEIVED_QUANTITY);Assert.Equal("N",review.Receipts[0].ACTIVE_IND);Assert.Equal("receiver",review.Receipts[0].RECEIVED_BY);review.Require(Company,"buyer","order");
    }
    [Fact] public void ActualContractSurvivesWebJsonRoundtripWithoutChangingContentProof()
    {
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);var review=Review();
        var received=JsonSerializer.Deserialize<PurchaseOrderSourceReview>(JsonSerializer.Serialize(review,options),options)!;
        received.Require(Company,"buyer","order");Assert.Equal(review.Revision,received.Revision);
    }
    [Fact] public void ProviderOrderingDoesNotChangeDigest()
    {
        var header=Header();var first=Line();var second=Line("second");var receipt=Receipt();
        Assert.Equal(PurchaseOrderSourceReview.Create(Company,"buyer",header,[first,second],[receipt]).Revision,
            PurchaseOrderSourceReview.Create(Company,"buyer",header,[second,first],[receipt]).Revision);
    }
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("foreign")][InlineData("duplicate")]
    public void MissingOrAmbiguousLineSetCannotEstablishReceiptSource(string fault)
    {
        var line=Line();if(fault=="identity")line.PO_LINE_ITEM_ID="";if(fault=="foreign")line.PURCHASE_ORDER_ID="another";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderSourceReview.Create(Company,"buyer",Header(),fault=="missing"?null!:fault=="duplicate"?[line,line]:[line],[Receipt()]));
    }
    [Theory][InlineData("missing")][InlineData("identity")][InlineData("foreign")][InlineData("duplicate")][InlineData("orphan")]
    public void ReceiptNeedsExactHeaderAndActualLineMembership(string fault)
    {
        var receipt=Receipt();if(fault=="identity")receipt.PO_RECEIPT_ID="";if(fault=="foreign")receipt.PURCHASE_ORDER_ID="another";if(fault=="orphan")receipt.PO_LINE_ITEM_ID="another-line";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderSourceReview.Create(Company,"buyer",Header(),[Line()],fault=="missing"?null!:fault=="duplicate"?[receipt,receipt]:[receipt]));
    }
    [Theory][InlineData("header")][InlineData("quantity")][InlineData("price")][InlineData("receipt")][InlineData("receiver")][InlineData("active")][InlineData("currency")][InlineData("line-unit")][InlineData("receipt-unit")]
    public void MutatedContentCannotReuseOriginalReviewDigest(string fault)
    {
        var review=Review();if(fault=="header")review.Header.TOTAL_AMOUNT=100m;if(fault=="quantity")review.Lines[0].QUANTITY=100m;if(fault=="price")review.Lines[0].UNIT_PRICE=100m;
        if(fault=="receipt")review.Receipts[0].RECEIVED_QUANTITY=100m;if(fault=="receiver")review.Receipts[0].RECEIVED_BY="another";if(fault=="active")review.Receipts[0].ACTIVE_IND="Y";
        if(fault=="currency")review.Header.CURRENCY_CODE="KWD";if(fault=="line-unit")review.Lines[0].QUANTITY_UOM="m³";if(fault=="receipt-unit")review.Receipts[0].QUANTITY_UOM="bbl";
        Assert.Throws<InvalidOperationException>(()=>review.Require(Company,"buyer","order"));
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("revision")][InlineData("time")]
    public void ReturnedSourceMustMatchOriginalContextAndProvenance(string fault)
    {
        var review=Review();review=fault switch{"company"=>review with{Company=new("tenant","another")},"actor"=>review with{ActorUserId="another"},"revision"=>review with{Revision="bad"},_=>review with{RetrievedAtUtc=default}};
        Assert.Throws<InvalidOperationException>(()=>review.Require(Company,"buyer","order"));
    }
    [Fact] public void RecordedMeasurementCodesSurviveWebJsonWithoutNormalizationOrConversion()
    {
        var header=Header();header.CURRENCY_CODE="KWD";var line=Line();line.QUANTITY_UOM="m³";var receipt=Receipt();receipt.QUANTITY_UOM="bbl";
        var review=PurchaseOrderSourceReview.Create(Company,"buyer",header,[line],[receipt]);
        var options=new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
        var returned=System.Text.Json.JsonSerializer.Deserialize<PurchaseOrderSourceReview>(System.Text.Json.JsonSerializer.Serialize(review,options),options)!;
        returned.Require(Company,"buyer","order");Assert.Equal("KWD",returned.Header.CURRENCY_CODE);Assert.Equal("m³",returned.Lines[0].QUANTITY_UOM);Assert.Equal("bbl",returned.Receipts[0].QUANTITY_UOM);
    }
    [Theory][InlineData("currency-long")][InlineData("line-long")][InlineData("receipt-control")]
    public void InvalidRecordedCodeCannotPublishAReview(string fault)
    {
        var header=Header();var line=Line();var receipt=Receipt();
        if(fault=="currency-long")header.CURRENCY_CODE=new string('X',33);if(fault=="line-long")line.QUANTITY_UOM=new string('X',65);if(fault=="receipt-control")receipt.QUANTITY_UOM="bbl\n";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderSourceReview.Create(Company,"buyer",header,[line],[receipt]));
    }
}
