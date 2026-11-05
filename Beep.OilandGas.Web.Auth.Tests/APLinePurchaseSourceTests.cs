using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class APLinePurchaseSourceTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static APInvoiceSourceReview Invoice()=>APInvoiceSourceReview.Create(Scope,"accountant",
        new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",CURRENCY_CODE="USD"},
        [new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",PURCHASE_ORDER_ID="order/a?b",PO_LINE_ITEM_ID="order-line",QUANTITY=2m,QUANTITY_UOM="m³"}]);
    private static PurchaseOrderSourceReview Order(string company="company",string actor="accountant",string supplier="supplier",string id="order/a?b",bool empty=false,decimal amount=12m)=>
        PurchaseOrderSourceReview.Create(new("tenant",company),actor,new(){PURCHASE_ORDER_ID=id,OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID=company,VENDOR_BA_ID=supplier,TOTAL_AMOUNT=amount,CURRENCY_CODE="KWD"},
            empty?[]:[new(){PO_LINE_ITEM_ID="order-line",PURCHASE_ORDER_ID=id,QUANTITY=3m,QUANTITY_UOM="bbl"}],[]);
    private static APLinePurchaseSourceReview Review()=>new(Invoice(),"line",Order());
    private static CompanyClientContext Selected(){var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(Scope,["ConfiguredAccountant"],["Accounting.View","Procurement.View"])));return company;}
    [Fact] public void OwnedReferenceAndInternationalUnitsSurviveWebJsonWithoutAMatchingDecision()
    {
        var review=Review();var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var returned=JsonSerializer.Deserialize<APLinePurchaseSourceReview>(JsonSerializer.Serialize(review,options),options)!;
        Assert.Equal("order/a?b",returned.RequireOrderId(Scope,"accountant","invoice","line",review.Invoice.Revision));
        Assert.Equal("USD",returned.Invoice.Invoice.CURRENCY_CODE);Assert.Equal("KWD",returned.Order.Header.CURRENCY_CODE);
        Assert.Equal("m³",returned.Invoice.Lines[0].QUANTITY_UOM);Assert.Equal("bbl",returned.Order.Lines[0].QUANTITY_UOM);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("supplier")][InlineData("order")][InlineData("line")][InlineData("revision")][InlineData("missing-order-line")][InlineData("changed-invoice")][InlineData("changed-order")][InlineData("no-reference")]
    public void UnconfirmedCrossRoleReferenceCannotPublishAnOrder(string fault)
    {
        var review=Review();var expected=review.Invoice.Revision;
        if(fault=="company")review=review with{Order=Order(company:"other")};if(fault=="actor")review=review with{Order=Order(actor:"other")};
        if(fault=="supplier")review=review with{Order=Order(supplier:"other")};if(fault=="order")review=review with{Order=Order(id:"other")};
        if(fault=="line")review=review with{InvoiceLineId="other"};if(fault=="revision")expected=new string('A',64);
        if(fault=="missing-order-line")review=review with{Order=Order(empty:true)};
        if(fault=="changed-invoice")review.Invoice.Lines[0].QUANTITY=90m;if(fault=="changed-order")review.Order.Header.TOTAL_AMOUNT=90m;
        if(fault=="no-reference"){
            review.Invoice.Lines[0].PURCHASE_ORDER_ID=null;review.Invoice.Lines[0].PO_LINE_ITEM_ID=null;
            var invoice=APInvoiceSourceReview.Create(Scope,"accountant",review.Invoice.Invoice,review.Invoice.Lines);review=review with{Invoice=invoice};expected=invoice.Revision;
        }
        Assert.Throws<InvalidOperationException>(()=>review.RequireOrderId(Scope,"accountant","invoice","line",expected));
    }
    [Fact] public async Task StateUsesOriginalInvoiceRevisionAndExistingFrozenPurchaseProjection()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));
        await state.LoadPurchaseSourceAsync("line",(invoice,line,revision)=>{Assert.Equal("invoice",invoice);Assert.Equal("line",line);Assert.Equal(review.Invoice.Revision,revision);return Task.FromResult(review);});
        review.Order.Header.TOTAL_AMOUNT=90m;Assert.Equal(12m,state.LinkedOrder.Detail!.RecordedAmount);Assert.Equal("line",state.SelectedPurchaseLineId);
        Assert.Equal("/procurement/orders/order%2Fa%3Fb",state.LinkedOrder.Detail.Route);
    }
    [Theory][InlineData("failed")][InlineData("wrong")]
    public async Task FailedOrForeignReloadClearsOwnedOrderButPreservesTheAPSource(string fault)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));await state.LoadPurchaseSourceAsync("line",(_,_,_)=>Task.FromResult(review));
        await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadPurchaseSourceAsync("line",(_,_,_)=>fault=="failed"?throw new HttpRequestException():Task.FromResult(review with{Order=Order(company:"other")})));
        Assert.Null(state.LinkedOrder.Detail);Assert.Null(state.SelectedPurchaseLineId);Assert.NotNull(state.Detail);
    }
    [Theory][InlineData("company")][InlineData("invoice")]
    public async Task ContextChangeCannotAdoptLatePurchaseEvidence(string change)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));
        var pending=new TaskCompletionSource<APLinePurchaseSourceReview>();var load=state.LoadPurchaseSourceAsync("line",(_,_,_)=>pending.Task);
        if(change=="company"){company.Clear();Assert.True(company.TrySelect(company.Version,"accountant",new(Scope,[],[])));}else state.Clear();
        pending.SetResult(review);await load;Assert.Null(state.LinkedOrder.Detail);Assert.Null(state.SelectedPurchaseLineId);
    }
    [Fact] public async Task LaterPurchaseReviewCannotBeOverwrittenByAnEarlierResponse()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));
        var pending=new TaskCompletionSource<APLinePurchaseSourceReview>();var earlier=state.LoadPurchaseSourceAsync("line",(_,_,_)=>pending.Task);
        await state.LoadPurchaseSourceAsync("line",(_,_,_)=>Task.FromResult(review with{Order=Order(amount:20m)}));pending.SetResult(review);await earlier;
        Assert.Equal(20m,state.LinkedOrder.Detail!.RecordedAmount);
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static source rendering has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ActualPanelShowsOwnedSourceOnlyWithinTheProcurementUiGate(bool allowed)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Invoice));await state.LoadPurchaseSourceAsync("line",(_,_,_)=>Task.FromResult(review));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var rendered=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state},{"AllowPurchaseSource",allowed},{"OnReview",EventCallback.Factory.Create<string>(new object(),_=>Task.CompletedTask)}}));return rendered.ToHtmlString();});
        if(allowed){Assert.Contains("Review owned PO source",html);Assert.Contains("Owned purchase source for invoice line: line",html);Assert.Contains("/procurement/orders/order%2Fa%3Fb",html);Assert.Contains("no allocation to this invoice line is established",html);Assert.Contains("Currency: KWD",html);}
        else {Assert.DoesNotContain("Review owned PO source",html);Assert.DoesNotContain("Owned purchase source for invoice line",html);Assert.DoesNotContain("/procurement/orders/",html);}
    }
    [Fact] public async Task CompanyLandingFiltersOnlyRecordedStatusesAndFreezesRows()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        var first=Invoice().Invoice;first.ACTIVE_IND="Y";first.STATUS="RECORDED";
        var second=Invoice().Invoice;second.AP_INVOICE_ID="other/a?b";second.VENDOR_BA_ID="other-supplier";second.ACTIVE_IND="Y";second.STATUS=null;second.TOTAL_AMOUNT=null;
        await state.LoadCompanyAsync(vendor=>{Assert.Null(vendor);return Task.FromResult(new List<AP_INVOICE>{second,first});});
        Assert.Equal(2,state.VisibleRows!.Count);Assert.Equal("RECORDED",Assert.Single(state.RecordedStatuses));
        first.STATUS="APPROVED";Assert.Equal("RECORDED",state.Rows![0].Status);
        state.SetStatusFilter("RECORDED");Assert.Equal("invoice",Assert.Single(state.VisibleRows!).Id);
        Assert.Throws<InvalidOperationException>(()=>state.SetStatusFilter("APPROVED"));
        state.SetStatusFilter(null);Assert.Equal(2,state.VisibleRows!.Count);Assert.Null(state.Rows[1].Amount);
        Assert.Equal("/accounting/payables/invoices/other%2Fa%3Fb",state.Rows[1].Route);
        company.Clear();Assert.Null(state.Rows);Assert.Null(state.VisibleRows);Assert.Empty(state.RecordedStatuses);Assert.Null(state.StatusFilter);
    }
    [Fact] public async Task CompanyQueueAwayAndBackCannotPublishTheOldRead()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var pending=new TaskCompletionSource<List<AP_INVOICE>>();
        var read=state.LoadCompanyAsync(_=>pending.Task);company.Clear();Assert.True(company.TrySelect(company.Version,"accountant",new(Scope,[],[])));
        var invoice=Invoice().Invoice;invoice.ACTIVE_IND="Y";pending.SetResult([invoice]);await read;Assert.Null(state.Rows);
    }
    [Fact] public async Task FailedCompanyReloadClearsQueueAndStatus()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var invoice=Invoice().Invoice;invoice.ACTIVE_IND="Y";invoice.STATUS="RECORDED";
        await state.LoadCompanyAsync(_=>Task.FromResult(new List<AP_INVOICE>{invoice}));state.SetStatusFilter("RECORDED");
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadCompanyAsync(_=>throw new HttpRequestException()));
        Assert.Null(state.Rows);Assert.Null(state.StatusFilter);Assert.Null(state.RetrievedAtUtc);
    }
    [Fact] public async Task ActualCompanyQueueRendersRecordedCountsStatusesAndInvoiceDrillDown()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var invoice=Invoice().Invoice;
        invoice.AP_INVOICE_ID="invoice/a?b";invoice.ACTIVE_IND="Y";invoice.STATUS="RECORDED";
        await state.LoadCompanyAsync(_=>Task.FromResult(new List<AP_INVOICE>{invoice}));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var component=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return component.ToHtmlString();});
        Assert.Contains("Supplier restriction: All suppliers",html);Assert.Contains("Recorded active invoices: 1",html);Assert.Contains("Showing: 1",html);Assert.Contains("RECORDED",html);
        Assert.Contains("/accounting/payables/invoices/invoice%2Fa%3Fb",html);Assert.Contains("No cross-currency liability total",html);
    }
    private static APReceiptAllocationReview Allocated(decimal quantity=2m,bool changedOrder=false)
    {
        var order=Order(amount:changedOrder?99m:12m);order=PurchaseOrderSourceReview.Create(Scope,"accountant",order.Header,order.Lines,
            [new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order/a?b",PO_LINE_ITEM_ID="order-line",RECEIVED_QUANTITY=5m,QUANTITY_UOM="bbl",ACTIVE_IND="Y"}]);
        var source=new APLinePurchaseSourceReview(Invoice(),"line",order);
        return APReceiptAllocationReview.Create(Scope,"accountant","invoice","line",source.Invoice.Revision,source,
            [new(){OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="allocation",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",PURCHASE_ORDER_ID="order/a?b",PO_LINE_ITEM_ID="order-line",
                PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=quantity,QUANTITY_UOM="m³",ACTIVE_IND="N",ROW_CREATED_BY="recorder"}]);
    }
    private static async Task PrepareAllocation(APInvoiceWorkspaceState state,APReceiptAllocationReview review)
    {
        await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(review.Source.Invoice));
        await state.LoadPurchaseSourceAsync("line",(_,_,_)=>Task.FromResult(review.Source));
    }
    [Fact] public async Task AllocationStateUsesBothDisplayedRevisionsAndFreezesHistory()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Allocated();await PrepareAllocation(state,review);
        await state.LoadReceiptAllocationsAsync((invoice,line,invoiceRevision,orderRevision)=>{
            Assert.Equal("invoice",invoice);Assert.Equal("line",line);Assert.Equal(state.SourceRevision,invoiceRevision);Assert.Equal(state.LinkedOrder.SourceRevision,orderRevision);return Task.FromResult(review);});
        review.Allocations[0].ALLOCATED_QUANTITY=99;Assert.Equal(2m,Assert.Single(state.Allocations!).Quantity);Assert.Equal("N",state.Allocations![0].ActiveIndicator);
        Assert.Equal(review.Revision,state.AllocationRevision);Assert.Equal(review.RetrievedAtUtc,state.AllocationsRetrievedAtUtc);
    }
    [Theory][InlineData("company")][InlineData("invoice")][InlineData("purchase")]
    public async Task ChangedDisplayedSourceCannotAdoptLateAllocationHistory(string change)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Allocated();await PrepareAllocation(state,review);
        var pending=new TaskCompletionSource<APReceiptAllocationReview>();var load=state.LoadReceiptAllocationsAsync((_,_,_,_)=>pending.Task);
        if(change=="company"){company.Clear();Assert.True(company.TrySelect(company.Version,"accountant",new(Scope,[],[])));}
        if(change=="invoice")state.Clear();if(change=="purchase")await state.LoadPurchaseSourceAsync("line",(_,_,_)=>Task.FromResult(review.Source));
        pending.SetResult(review);await load;Assert.Null(state.Allocations);Assert.Null(state.AllocationRevision);
    }
    [Theory][InlineData("failed")][InlineData("order")]
    public async Task FailedOrChangedAllocationReloadClearsHistoryAndPreservesSources(string fault)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Allocated();await PrepareAllocation(state,review);
        await state.LoadReceiptAllocationsAsync((_,_,_,_)=>Task.FromResult(review));
        if(fault=="failed")await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadReceiptAllocationsAsync((_,_,_,_)=>throw new HttpRequestException()));
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReceiptAllocationsAsync((_,_,_,_)=>Task.FromResult(Allocated(changedOrder:true))));
        Assert.Null(state.Allocations);Assert.Null(state.AllocationRevision);Assert.NotNull(state.Detail);Assert.NotNull(state.LinkedOrder.Detail);
    }
    [Fact] public async Task OlderAllocationResponseCannotOverwriteNewerReviewedRows()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Allocated();await PrepareAllocation(state,review);
        var pending=new TaskCompletionSource<APReceiptAllocationReview>();var older=state.LoadReceiptAllocationsAsync((_,_,_,_)=>pending.Task);
        await state.LoadReceiptAllocationsAsync((_,_,_,_)=>Task.FromResult(Allocated(3m)));pending.SetResult(review);await older;
        Assert.Equal(3m,Assert.Single(state.Allocations!).Quantity);
    }
    [Theory][InlineData(true,false)][InlineData(false,false)][InlineData(true,true)][InlineData(false,true)]
    public async Task ActualAllocationPanelRequiresTheProcurementGateAndShowsReceiptHistory(bool allowed,bool empty)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Allocated();if(empty)review=APReceiptAllocationReview.Create(Scope,"accountant","invoice","line",review.Source.Invoice.Revision,review.Source,[]);await PrepareAllocation(state,review);
        await state.LoadReceiptAllocationsAsync((_,_,_,_)=>Task.FromResult(review));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var component=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{
            {"Review",state},{"AllowPurchaseSource",allowed},{"OnAllocations",EventCallback.Factory.Create(new object(),()=>Task.CompletedTask)}}));return System.Net.WebUtility.HtmlDecode(component.ToHtmlString());});
        if(allowed){Assert.Contains("Review recorded receipt allocations",html);Assert.Contains("Recorded receipt allocations for invoice line: line",html);if(empty)Assert.Contains("No recorded receipt allocations were returned",html);else{Assert.Contains("Receipt: receipt",html);Assert.Contains("Unit: m³",html);Assert.Contains("recorder",html);}Assert.Contains("global receipt consumption",html);}
        else{Assert.DoesNotContain("Review recorded receipt allocations",html);Assert.DoesNotContain("Recorded receipt allocations for invoice line",html);Assert.DoesNotContain("Allocation content revision",html);}
    }
    private static List<AP_INVOICE> StatusQueue()
    {
        var statuses=new string?[]{"DRAFT","DRAFT","POSTED",null," ","Unavailable"};
        return statuses.Select((status,index)=>{var row=Invoice().Invoice;row.AP_INVOICE_ID="status-invoice-"+index;row.ACTIVE_IND="Y";row.STATUS=status;row.CURRENCY_CODE=index%2==0?"KWD":"USD";return row;}).ToList();
    }
    [Theory][InlineData("DRAFT",false,2)][InlineData("POSTED",false,1)][InlineData("Unavailable",false,1)][InlineData(null,true,2)]
    public async Task RecordedStatusCountSelectsExactlyItsContributingCurrentInvoiceRows(string? status,bool unavailable,int count)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var rows=StatusQueue();
        await state.LoadCompanyAsync(_=>Task.FromResult(rows));Assert.Equal(6,state.StatusCounts.Sum(bucket=>bucket.Count));
        var bucket=Assert.Single(state.StatusCounts,value=>value.Status==status && value.Unavailable==unavailable);Assert.Equal(count,bucket.Count);
        state.SetStatusFilter(status,unavailable);Assert.Equal(count,state.VisibleRows!.Count);
        var expected=rows.Where(row=>unavailable?string.IsNullOrWhiteSpace(row.STATUS):row.STATUS==status).Select(row=>row.AP_INVOICE_ID).OrderBy(id=>id,StringComparer.Ordinal);
        Assert.Equal(expected,state.VisibleRows.Select(row=>row.Id));state.SetStatusFilter(null);Assert.Equal(6,state.VisibleRows.Count);
        company.Clear();Assert.Empty(state.StatusCounts);Assert.Null(state.VisibleRows);Assert.False(state.UnavailableStatusOnly);
    }
    [Fact] public async Task StatusSummaryRemainsFrozenAndFailedReloadCannotShowPreviousCounts()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var rows=StatusQueue();await state.LoadCompanyAsync(_=>Task.FromResult(rows));
        rows[0].STATUS="OTHER";Assert.Equal(2,Assert.Single(state.StatusCounts,bucket=>bucket.Status=="DRAFT").Count);
        state.SetStatusFilter(null,true);await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadCompanyAsync(_=>throw new HttpRequestException()));
        Assert.Empty(state.StatusCounts);Assert.Null(state.VisibleRows);Assert.False(state.UnavailableStatusOnly);
        Assert.Throws<InvalidOperationException>(()=>state.SetStatusFilter(null,true));
    }
    [Theory][InlineData("DRAFT",false,"status-invoice-0",2)][InlineData(null,true,"status-invoice-3",2)][InlineData("Unavailable",false,"status-invoice-5",1)]
    public async Task ActualStatusSummaryRendersItsCountAndExactSelectedDrillDown(string? status,bool unavailable,string expectedId,int count)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await state.LoadCompanyAsync(_=>Task.FromResult(StatusQueue()));state.SetStatusFilter(status,unavailable);
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var component=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return System.Net.WebUtility.HtmlDecode(component.ToHtmlString());});
        Assert.Contains("All returned statuses: 6",html);Assert.Contains("DRAFT: 2",html);Assert.Contains("Unavailable recorded status: 2",html);Assert.Contains("Unavailable: 1",html);
        Assert.Contains("Showing: "+count,html);Assert.Contains("/accounting/payables/invoices/"+expectedId,html);
        Assert.DoesNotContain("/accounting/payables/invoices/status-invoice-2",html);Assert.Contains("No cross-currency liability total",html);
    }
    private static async Task PrepareProposal(APInvoiceWorkspaceState state)
    {
        var review=Allocated();await PrepareAllocation(state,review);await state.LoadReceiptAllocationsAsync((_,_,_,_)=>Task.FromResult(review));
        state.SetProposalReceipt("receipt");state.SetProposalQuantity(1m);
    }
    [Fact] public async Task ProposalStateRetainsExactInputAndFrozenReviewWithoutAllocatingAnything()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await PrepareProposal(state);
        await state.ReviewAllocationProposalAsync((invoice,line,proposal)=>{
            Assert.Equal("invoice",invoice);Assert.Equal("line",line);Assert.Equal(1m,proposal.Quantity);Assert.Equal("bbl",proposal.Unit);
            return Task.FromResult(APReceiptAllocationProposalReview.Create(Scope,"accountant",invoice,line,proposal,Allocated()));});
        Assert.Equal(1m,state.ReviewedProposal!.Quantity);Assert.Equal("receipt",state.ReviewedProposal.ReceiptId);
        Assert.Equal(2m,Assert.Single(state.Allocations!).Quantity);state.SetProposalQuantity(2m);Assert.Null(state.ReviewedProposal);
    }
    [Theory][InlineData("input")][InlineData("company")][InlineData("allocations")]
    public async Task ProposalChangesDiscardLateReviewedInput(string change)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await PrepareProposal(state);var pending=new TaskCompletionSource<APReceiptAllocationProposalReview>();APReceiptAllocationProposal? captured=null;
        var load=state.ReviewAllocationProposalAsync((_,_,proposal)=>{captured=proposal;return pending.Task;});
        if(change=="input")state.SetProposalQuantity(2m);if(change=="company")company.Clear();if(change=="allocations")state.ClearReceiptAllocations();
        pending.SetResult(APReceiptAllocationProposalReview.Create(Scope,"accountant","invoice","line",captured!,Allocated()));await load;Assert.Null(state.ReviewedProposal);
    }
    [Fact] public async Task FailedProposalReloadClearsReviewAndRetainsExplicitPreparerInput()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await PrepareProposal(state);
        await state.ReviewAllocationProposalAsync((invoice,line,proposal)=>Task.FromResult(APReceiptAllocationProposalReview.Create(Scope,"accountant",invoice,line,proposal,Allocated())));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.ReviewAllocationProposalAsync((_,_,_)=>throw new HttpRequestException()));
        Assert.Null(state.ReviewedProposal);Assert.Equal(1m,state.ProposalQuantity);Assert.Equal("receipt",state.ProposalReceiptId);Assert.NotNull(state.Allocations);
    }
    [Theory][InlineData("bbl")][InlineData("m³")]
    public async Task ActualProposalReviewRendersFrozenReceiptContributorsAndExactInvoiceLink(string unit)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await PrepareProposal(state);
        var source=Allocated();var header=System.Text.Json.JsonSerializer.Deserialize<AP_INVOICE>(System.Text.Json.JsonSerializer.Serialize(source.Source.Invoice.Invoice))!;
        header.AP_INVOICE_ID="other/invoice%2F";
        var invoice=APInvoiceSourceReview.Create(Scope,"accountant",header,[new(){AP_INVOICE_ID=header.AP_INVOICE_ID,AP_INVOICE_LINE_ID="other-line",PURCHASE_ORDER_ID=source.Source.Order.Header.PURCHASE_ORDER_ID,PO_LINE_ITEM_ID=source.Source.Invoice.Lines[0].PO_LINE_ITEM_ID}]);
        var row=new AP_RECEIPT_ALLOCATION{OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_RECEIPT_ALLOCATION_ID="other-allocation",AP_INVOICE_ID=header.AP_INVOICE_ID,AP_INVOICE_LINE_ID="other-line",PURCHASE_ORDER_ID=source.Source.Order.Header.PURCHASE_ORDER_ID,
            PO_LINE_ITEM_ID=invoice.Lines[0].PO_LINE_ITEM_ID!,PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM=unit,ACTIVE_IND="Y"};
        var contribution=APReceiptAllocationReview.Create(Scope,"accountant",header.AP_INVOICE_ID,"other-line",invoice.Revision,new(invoice,"other-line",source.Source.Order),[row]);
        var consumption=APReceiptConsumptionReview.Create(Scope,"accountant",source.Source,"receipt",[contribution]);
        await state.ReviewAllocationProposalAsync((id,line,proposal)=>Task.FromResult(APReceiptAllocationProposalReview.Create(Scope,"accountant",id,line,proposal,source,consumption)));
        consumption.Contributors[0].Allocations[0].ALLOCATED_QUANTITY=99;
        Assert.Equal(2m,Assert.Single(state.ReviewedProposal!.Contributors!).Quantity);
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var component=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{
            {"Review",state},{"AllowPurchaseSource",true},{"AllowPreparation",true},{"OnProposal",EventCallback.Factory.Create(new object(),()=>Task.CompletedTask)}}));return System.Net.WebUtility.HtmlDecode(component.ToHtmlString());});
        Assert.Contains("Receipt allocations across invoice lines: 1",html);Assert.Contains("other-allocation",html);
        Assert.Contains("/accounting/payables/invoices/other%2Finvoice%252F",html);Assert.Contains("not an available balance",html);
        Assert.Contains("Invoice / PO / receipt source comparisons",html);Assert.Contains("Recorded unit codes",html);Assert.Contains("Recorded currency codes",html);
        Assert.Contains("Receipt acceptance",html);Assert.Contains("Independent reviewer handoff",html);Assert.Contains("No default quantity or price tolerance",html);
        Assert.Equal(8,state.ReviewedProposal.Comparisons!.Count);
        if(unit=="m³")Assert.Contains("Recorded quantity total is unavailable",html);else Assert.DoesNotContain("Recorded quantity total is unavailable",html);
        state.SetProposalQuantity(3);Assert.Null(state.ReviewedProposal);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task ActualProposalFormIsGatedAndExplainsTheReadOnlyReview(bool allowed)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await PrepareProposal(state);
        await state.ReviewAllocationProposalAsync((invoice,line,proposal)=>Task.FromResult(APReceiptAllocationProposalReview.Create(Scope,"accountant",invoice,line,proposal,Allocated())));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var component=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{
            {"Review",state},{"AllowPurchaseSource",true},{"AllowPreparation",allowed},{"OnProposal",EventCallback.Factory.Create(new object(),()=>Task.CompletedTask)}}));return System.Net.WebUtility.HtmlDecode(component.ToHtmlString());});
        if(allowed){Assert.Contains("Prepare receipt allocation proposal",html);Assert.Contains("Reviewed proposal: receipt",html);Assert.Contains("Recorded receipt unit: bbl",html);Assert.Contains("This proposal review records no allocation",html);}
        else{Assert.DoesNotContain("Prepare receipt allocation proposal",html);Assert.DoesNotContain("Reviewed proposal: receipt",html);}
    }
}
