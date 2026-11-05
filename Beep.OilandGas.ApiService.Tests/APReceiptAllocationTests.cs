using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class APReceiptAllocationTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static APLinePurchaseSourceReview Source()=>new(
        APInvoiceSourceReview.Create(Scope,"actor",new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier"},
            [new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line"}]),"line",
        PurchaseOrderSourceReview.Create(Scope,"actor",new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier"},
            [new(){PO_LINE_ITEM_ID="po-line",PURCHASE_ORDER_ID="order"},new(){PO_LINE_ITEM_ID="other-line",PURCHASE_ORDER_ID="order"}],
            [new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",RECEIVED_QUANTITY=9m,QUANTITY_UOM="bbl"},
             new(){PO_RECEIPT_ID="other-receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="other-line"}]));
    private static AP_RECEIPT_ALLOCATION Allocation()=>new(){OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="allocation",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",
        PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM="m³",ACTIVE_IND="Y",ROW_CREATED_BY="recorder"};
    [Fact] public void ReceiptMembershipFreezesRecordedValuesAndPreservesDifferentUnitsWithoutConversion()
    {
        var source=Source();var allocation=Allocation();var review=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[allocation]);
        allocation.ALLOCATED_QUANTITY=99m;source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID=="receipt").RECEIVED_QUANTITY=99m;
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);var json=JsonSerializer.Serialize(review,options);
        var returned=JsonSerializer.Deserialize<APReceiptAllocationReview>(json,options)!;returned.Require(Scope,"actor","invoice","line",review.Source.Invoice.Revision);
        Assert.Equal(2m,returned.Allocations[0].ALLOCATED_QUANTITY);Assert.Equal("m³",returned.Allocations[0].QUANTITY_UOM);
        Assert.Equal(9m,returned.Source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID=="receipt").RECEIVED_QUANTITY);Assert.Equal("bbl",returned.Source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID=="receipt").QUANTITY_UOM);
    }
    [Theory][InlineData("invoice")][InlineData("line")][InlineData("order")][InlineData("po-line")][InlineData("receipt")][InlineData("other-receipt")]
    [InlineData("quantity-null")][InlineData("quantity-zero")][InlineData("quantity-negative")][InlineData("unit")][InlineData("duplicate")][InlineData("null")]
    public void IncompleteOrForeignAllocationEvidenceCannotBeReviewed(string fault)
    {
        var source=Source();var row=Allocation();
        if(fault=="invoice")row.AP_INVOICE_ID="other";if(fault=="line")row.AP_INVOICE_LINE_ID="other";
        if(fault=="order")row.PURCHASE_ORDER_ID="other";if(fault=="po-line")row.PO_LINE_ITEM_ID="other-line";
        if(fault=="receipt")row.PO_RECEIPT_ID="missing";if(fault=="other-receipt")row.PO_RECEIPT_ID="other-receipt";
        if(fault=="quantity-null")row.ALLOCATED_QUANTITY=null;if(fault=="quantity-zero")row.ALLOCATED_QUANTITY=0;
        if(fault=="quantity-negative")row.ALLOCATED_QUANTITY=-1;if(fault=="unit")row.QUANTITY_UOM=null;
        Assert.Throws<InvalidOperationException>(()=>APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,
            fault=="duplicate"?[row,row]:fault=="null"?[null!]:[row]));
    }
    [Theory][InlineData("quantity")][InlineData("unit")][InlineData("activity")][InlineData("actor")][InlineData("receipt")]
    public void ChangedAllocationOrReceiptInvalidatesTheReviewedContent(string fault)
    {
        var source=Source();var review=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[Allocation()]);
        if(fault=="quantity")review.Allocations[0].ALLOCATED_QUANTITY=3;if(fault=="unit")review.Allocations[0].QUANTITY_UOM="bbl";
        if(fault=="activity")review.Allocations[0].ACTIVE_IND="N";if(fault=="actor")review.Allocations[0].ROW_CREATED_BY="other";
        if(fault=="receipt")review.Source.Order.Receipts[0].RECEIVED_QUANTITY=8;
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"actor","invoice","line",source.Invoice.Revision));
    }
    [Fact] public void ConfirmedEmptyAllocationHistoryIsValidWithoutEstablishingAMatch()
    {
        var source=Source();var review=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[]);
        review.Require(Scope,"actor","invoice","line",source.Invoice.Revision);Assert.Empty(review.Allocations);
    }
    [Fact] public void OneInvoiceLineCanRecordPartialAllocationsFromMultipleExactReceipts()
    {
        var source=Source();var receipts=source.Order.Receipts.ToList();receipts.Add(new(){PO_RECEIPT_ID="receipt-two",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",RECEIVED_QUANTITY=4m,QUANTITY_UOM="m³"});
        source=source with{Order=PurchaseOrderSourceReview.Create(Scope,"actor",source.Order.Header,source.Order.Lines,receipts)};
        var second=Allocation();second.AP_RECEIPT_ALLOCATION_ID="allocation-two";second.PO_RECEIPT_ID="receipt-two";second.ALLOCATED_QUANTITY=1m;second.ACTIVE_IND="N";
        var review=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[second,Allocation()]);
        review.Require(Scope,"actor","invoice","line",source.Invoice.Revision);Assert.Equal(2,review.Allocations.Count);
        Assert.Equal("allocation",review.Allocations[0].AP_RECEIPT_ALLOCATION_ID);Assert.Equal("N",review.Allocations[1].ACTIVE_IND);
    }
    private static System.Security.Claims.ClaimsPrincipal User()=>new(new System.Security.Claims.ClaimsIdentity([
        new(System.Security.Claims.ClaimTypes.Role,"ConfiguredAccountant")],"test"));
    private static RepositoryCompanyAccess Access()=>new(Scope,["ConfiguredAccountant"],["Accounting.View","Procurement.View"]);
    [Fact] public async Task AllocationOrchestrationRechecksSourcesAndReturnsClosingRecordedRows()
    {
        var authority=0;var allocations=0;var source=Source();
        var review=await Beep.OilandGas.ApiService.Services.APInvoiceReadChecks.ReadReceiptAllocationsAsync(User(),"actor","invoice","line",source.Invoice.Revision,source.Order.Revision,
            ()=>{authority++;return Task.FromResult((Access(),"binding"));},
            (_,_,_)=>Task.FromResult<APInvoiceSourceReview?>(Source().Invoice),(_,_,_)=>Task.FromResult<PurchaseOrderSourceReview?>(Source().Order),
            (scope,invoice,line)=>{Assert.Equal(Scope,scope);Assert.Equal("invoice",invoice);Assert.Equal("line",line);allocations++;return Task.FromResult(new List<AP_RECEIPT_ALLOCATION>{Allocation()});});
        Assert.Equal(17,authority);Assert.Equal(2,allocations);Assert.Single(review.Allocations);
    }
    public static IEnumerable<object[]> AllocationRevocations()=>from stage in new[]{1,8,9,10,16,17}
        from fault in new[]{"role","accounting","procurement","company","binding"}
        where stage!=1 || fault!="company" select new object[]{stage,fault};
    [Theory][MemberData(nameof(AllocationRevocations))]
    public async Task AllocationAuthorityRevocationStopsTheNextReadOrPublication(int stage,string fault)
    {
        var authority=0;var allocations=0;var source=Source();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Beep.OilandGas.ApiService.Services.APInvoiceReadChecks.ReadReceiptAllocationsAsync(User(),"actor","invoice","line",source.Invoice.Revision,source.Order.Revision,
            ()=>{authority++;var access=Access();var binding="binding";
                if(authority==stage){if(fault=="role")access=access with{Roles=[]};if(fault=="accounting")access=access with{Permissions=["Procurement.View"]};
                    if(fault=="procurement")access=access with{Permissions=["Accounting.View"]};if(fault=="company")access=access with{Scope=new("tenant","other")};if(fault=="binding")binding=stage==1?"":"changed";}
                return Task.FromResult((access,binding));},
            (_,_,_)=>Task.FromResult<APInvoiceSourceReview?>(Source().Invoice),(_,_,_)=>Task.FromResult<PurchaseOrderSourceReview?>(Source().Order),
            (_,_,_)=>{allocations++;return Task.FromResult(new List<AP_RECEIPT_ALLOCATION>{Allocation()});}));
        Assert.Equal(stage,authority);Assert.Equal(stage<=8?0:stage<=16?1:2,allocations);
    }
    [Theory][InlineData("allocation")][InlineData("order")]
    public async Task ChangedReviewedOrderOrClosingAllocationCannotPublish(string fault)
    {
        var source=Source();var reads=0;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Beep.OilandGas.ApiService.Services.APInvoiceReadChecks.ReadReceiptAllocationsAsync(User(),"actor","invoice","line",source.Invoice.Revision,
            fault=="order"?new string('A',64):source.Order.Revision,()=>Task.FromResult((Access(),"binding")),
            (_,_,_)=>Task.FromResult<APInvoiceSourceReview?>(Source().Invoice),(_,_,_)=>Task.FromResult<PurchaseOrderSourceReview?>(Source().Order),
            (_,_,_)=>{reads++;var row=Allocation();if(reads==2)row.ALLOCATED_QUANTITY=3;return Task.FromResult(new List<AP_RECEIPT_ALLOCATION>{row});}));
        Assert.Equal(fault=="order"?0:2,reads);
    }
    [Theory][InlineData("same-unit",2)][InlineData("different-unit",null)][InlineData("unknown-activity",null)][InlineData("inactive",0)]
    public void ReceiptContributorsPreserveHistoryWithoutInventingUnitConversions(string condition,int? quantity)
    {
        var source=Source();var row=Allocation();if(condition!="different-unit")row.QUANTITY_UOM="bbl";
        if(condition=="unknown-activity")row.ACTIVE_IND=null!;if(condition=="inactive")row.ACTIVE_IND="N";
        var contribution=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[row]);
        var review=APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[contribution]);
        row.ALLOCATED_QUANTITY=99m;contribution.Allocations[0].ALLOCATED_QUANTITY=88m;
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var returned=JsonSerializer.Deserialize<APReceiptConsumptionReview>(JsonSerializer.Serialize(review,options),options)!;
        returned.Require(Scope,"actor",source,"receipt");Assert.Equal(quantity.HasValue?(decimal?)quantity:null,returned.RecordedActiveQuantity);
        Assert.Equal(2m,returned.Contributors[0].Allocations[0].ALLOCATED_QUANTITY);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate-line")][InlineData("duplicate-id")][InlineData("receipt")][InlineData("changed")]
    public void ReceiptContributorsRefuseForeignAmbiguousOrChangedEvidence(string fault)
    {
        var source=Source();var other=source.Invoice.Invoice;other=JsonSerializer.Deserialize<AP_INVOICE>(JsonSerializer.Serialize(other))!;
        other.AP_INVOICE_ID="second";if(fault=="foreign")other.OWNER_COMPANY_BA_ID="foreign";
        var invoice=APInvoiceSourceReview.Create(Scope,"actor",Source().Invoice.Invoice,Source().Invoice.Lines);
        var first=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[Allocation()]);
        if(fault=="foreign"){
            Assert.Throws<InvalidOperationException>(()=>APInvoiceSourceReview.Create(Scope,"actor",other,[new(){AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="line"}]));return;
        }
        if(fault=="duplicate-line"){
            Assert.Throws<InvalidOperationException>(()=>APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[first,first]));return;
        }
        if(fault=="duplicate-id"){
            invoice=APInvoiceSourceReview.Create(Scope,"actor",other,[new(){AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="second-line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line"}]);
            var row=Allocation();row.AP_INVOICE_ID="second";row.AP_INVOICE_LINE_ID="second-line";
            var second=APReceiptAllocationReview.Create(Scope,"actor","second","second-line",invoice.Revision,new(invoice,"second-line",source.Order),[row]);
            Assert.Throws<InvalidOperationException>(()=>APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[first,second]));return;
        }
        var review=APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[first]);
        if(fault=="changed")review.Contributors[0].Allocations[0].ALLOCATED_QUANTITY=3m;
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"actor",source,fault=="receipt"?"other-receipt":"receipt"));
    }
    [Fact] public void EmptyReceiptContributorsRemainExplicitRecordedEvidence()
    {
        var source=Source();var review=APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[]);
        review.Require(Scope,"actor",source,"receipt");Assert.Empty(review.Contributors);Assert.Equal(0m,review.RecordedActiveQuantity);
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("missing-tenant")][InlineData("missing-company")]
    public void AllocationOwnershipCannotBeInferredFromCollidingLocalSourceIds(string fault)
    {
        var source=Source();var row=Allocation();
        if(fault=="tenant")row.OWNER_TENANT_ID="foreign";if(fault=="company")row.OWNER_COMPANY_BA_ID="foreign";
        if(fault=="missing-tenant")row.OWNER_TENANT_ID=null;if(fault=="missing-company")row.OWNER_COMPANY_BA_ID=null;
        Assert.Throws<InvalidOperationException>(()=>APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[row]));
    }
    [Theory][InlineData(true)][InlineData(false)] public void ChangedAllocationOwnershipInvalidatesFrozenReview(bool tenant)
    {
        var source=Source();var review=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[Allocation()]);
        if(tenant)review.Allocations[0].OWNER_TENANT_ID="foreign";else review.Allocations[0].OWNER_COMPANY_BA_ID="foreign";
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"actor","invoice","line",source.Invoice.Revision));
    }
    private static APReceiptAllocationProposalReview ComparisonReview(string fault="none")
    {
        var source=Source();var header=source.Invoice.Invoice;header.CURRENCY_CODE="USD";
        var line=source.Invoice.Lines[0];line.QUANTITY_UOM=fault=="unit"?"m³":"bbl";line.CURRENCY_CODE=fault=="currency"?"KWD":fault=="missing-currency"?null:"USD";
        line.QUANTITY=fault=="missing-quantity"?null:8m;line.UNIT_PRICE=fault=="zero-price"?0m:fault=="price"?11m:10m;
        var order=source.Order.Header;order.CURRENCY_CODE="USD";
        var orderLine=source.Order.Lines.Single(row=>row.PO_LINE_ITEM_ID=="po-line");orderLine.QUANTITY_UOM="bbl";orderLine.UNIT_PRICE=fault=="zero-price"?0m:10m;
        var receipt=source.Order.Receipts.Single(row=>row.PO_RECEIPT_ID=="receipt");receipt.ACTIVE_IND="Y";receipt.RECEIPT_STATUS="RECEIVED";
        source=new(APInvoiceSourceReview.Create(Scope,"actor",header,source.Invoice.Lines),"line",PurchaseOrderSourceReview.Create(Scope,"actor",order,source.Order.Lines,source.Order.Receipts));
        var row=Allocation();row.QUANTITY_UOM=fault=="allocation-unit"?"m³":"bbl";if(fault=="activity")row.ACTIVE_IND=null!;
        var rows=new List<AP_RECEIPT_ALLOCATION>{row};
        if(fault=="overflow"){
            row.ALLOCATED_QUANTITY=decimal.MaxValue;var second=Allocation();second.AP_RECEIPT_ALLOCATION_ID="allocation-two";second.QUANTITY_UOM="bbl";rows.Add(second);
        }
        var allocations=APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,rows);
        var consumption=fault=="missing-consumption"?null:APReceiptConsumptionReview.Create(Scope,"actor",source,"receipt",[allocations]);
        var proposal=new APReceiptAllocationProposal("receipt",fault=="quantity"?8m:1m,"bbl",source.Invoice.Revision,source.Order.Revision,allocations.Revision);
        return APReceiptAllocationProposalReview.Create(Scope,"actor","invoice","line",proposal,allocations,consumption);
    }
    [Theory]
    [InlineData("none","units",APSourceComparisonOutcome.RecordedAgreement)]
    [InlineData("unit","units",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("currency","currencies",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("missing-currency","currencies",APSourceComparisonOutcome.Unavailable)]
    [InlineData("price","prices",APSourceComparisonOutcome.RecordedDifference)]
    [InlineData("zero-price","prices",APSourceComparisonOutcome.RecordedAgreement)]
    [InlineData("currency","prices",APSourceComparisonOutcome.Unavailable)]
    [InlineData("unit","prices",APSourceComparisonOutcome.Unavailable)]
    [InlineData("none","invoice-quantity",APSourceComparisonOutcome.WithinRecordedLimit)]
    [InlineData("quantity","invoice-quantity",APSourceComparisonOutcome.ExceedsRecordedLimit)]
    [InlineData("quantity","receipt-quantity",APSourceComparisonOutcome.ExceedsRecordedLimit)]
    [InlineData("missing-quantity","invoice-quantity",APSourceComparisonOutcome.Unavailable)]
    [InlineData("activity","invoice-quantity",APSourceComparisonOutcome.Unavailable)]
    [InlineData("allocation-unit","receipt-quantity",APSourceComparisonOutcome.Unavailable)]
    [InlineData("missing-consumption","receipt-quantity",APSourceComparisonOutcome.Unavailable)]
    [InlineData("overflow","receipt-quantity",APSourceComparisonOutcome.Unavailable)]
    [InlineData("overflow","invoice-quantity",APSourceComparisonOutcome.Unavailable)]
    public void SourceComparisonsUseExactRecordedEvidenceWithoutDefaultsOrConversions(string fault,string code,APSourceComparisonOutcome expected)
    {Assert.Equal(expected,Assert.Single(ComparisonReview(fault).Comparisons,row=>row.Code==code).Outcome);}
    [Fact] public void RecordedAgreementDoesNotEstablishAcceptanceToleranceOrIndependentApproval()
    {
        var review=ComparisonReview();var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var returned=JsonSerializer.Deserialize<APReceiptAllocationProposalReview>(JsonSerializer.Serialize(review,options),options)!;
        Assert.Equal(8,returned.Comparisons.Count);
        foreach(var code in new[]{"acceptance","tolerances","reviewer"})Assert.Equal(APSourceComparisonOutcome.Unavailable,Assert.Single(returned.Comparisons,row=>row.Code==code).Outcome);
        Assert.Contains("RECEIVED",Assert.Single(returned.Comparisons,row=>row.Code=="acceptance").Evidence);
        returned.Source.Source.Invoice.Lines[0].UNIT_PRICE=1m;
        Assert.Throws<InvalidOperationException>(()=>returned.Comparisons);
    }
    private static APReceiptAllocationReview ProposalSource(bool inactive=false)
    {
        var source=Source();var receipts=source.Order.Receipts.ToList();receipts.Single(row=>row.PO_RECEIPT_ID=="receipt").ACTIVE_IND=inactive?"N":"Y";
        source=source with{Order=PurchaseOrderSourceReview.Create(Scope,"actor",source.Order.Header,source.Order.Lines,receipts)};
        return APReceiptAllocationReview.Create(Scope,"actor","invoice","line",source.Invoice.Revision,source,[Allocation()]);
    }
    [Theory][InlineData("zero")][InlineData("exceeds")][InlineData("unit")][InlineData("other-line")][InlineData("inactive")][InlineData("revision")]
    public void ProposalRequiresExactActiveReceiptQuantityUnitAndDisplayedHistory(string fault)
    {
        var source=ProposalSource(fault=="inactive");var proposal=new APReceiptAllocationProposal(fault=="other-line"?"other-receipt":"receipt",
            fault=="zero"?0:fault=="exceeds"?10:1,fault=="unit"?"m³":"bbl",source.Source.Invoice.Revision,source.Source.Order.Revision,
            fault=="revision"?new string('A',64):source.Revision);
        Assert.Throws<InvalidOperationException>(()=>proposal.Require(Scope,"actor","invoice","line",source));
    }
    [Fact] public void ProposalReviewFreezesSourceAndRequiresTheExactPreparerInput()
    {
        var source=ProposalSource();var proposal=new APReceiptAllocationProposal("receipt",1m,"bbl",source.Source.Invoice.Revision,source.Source.Order.Revision,source.Revision);
        var review=APReceiptAllocationProposalReview.Create(Scope,"actor","invoice","line",proposal,source);source.Allocations[0].ALLOCATED_QUANTITY=99;
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);var returned=JsonSerializer.Deserialize<APReceiptAllocationProposalReview>(JsonSerializer.Serialize(review,options),options)!;
        returned.Require(Scope,"actor","invoice","line",proposal);Assert.Equal(2m,returned.Source.Allocations[0].ALLOCATED_QUANTITY);
        Assert.Throws<InvalidOperationException>(()=>returned.Require(Scope,"actor","invoice","line",proposal with{Quantity=2m}));
    }
}
