using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Auth.Tests.Fixtures;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class PurchaseOrderSourceRenderingTests
{
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in source rendering.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static async Task<string> Render(string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"buyer",new(new("tenant","company"),[],[])));using var state=new PurchaseOrderWorkspaceState(company);
        var order=new PURCHASE_ORDER{PURCHASE_ORDER_ID="order/a?b",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",ACTIVE_IND="Y",PO_NUMBER="PO-1",STATUS="DRAFT",DESCRIPTION="<script>recorded</script>",TOTAL_AMOUNT=12m};
        if(mode=="measurements")order.CURRENCY_CODE="KWD";
        if(mode is "history" or "measurements")await state.LoadDetailAsync(order.PURCHASE_ORDER_ID,_=>Task.FromResult<PurchaseOrderSourceReview?>(PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",order,
            [new(){PO_LINE_ITEM_ID="line",PURCHASE_ORDER_ID=order.PURCHASE_ORDER_ID,LINE_NUMBER=3,ITEM_DESCRIPTION="<script>item</script>",QUANTITY=12m,QUANTITY_UOM=mode=="measurements"?"m³":null,UNIT_PRICE=null}],
            [new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID=order.PURCHASE_ORDER_ID,PO_LINE_ITEM_ID="line",RECEIVED_QUANTITY=null,QUANTITY_UOM=mode=="measurements"?"bbl":null,RECEIVED_BY="receiver",ACTIVE_IND="N",DESCRIPTION="<script>receipt</script>"}])));
        if(mode=="detail")await state.LoadDetailAsync(order.PURCHASE_ORDER_ID,_=>Task.FromResult<PurchaseOrderSourceReview?>(PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",order,[],[])));
        if(mode is "rows" or "empty")await state.LoadVendorAsync("supplier",_=>Task.FromResult(mode=="empty"?new List<PURCHASE_ORDER>():[order]));
        if(mode=="company")await state.LoadCompanyAsync(_=>Task.FromResult(new List<PURCHASE_ORDER>{order}));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<PurchaseOrderRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return root.ToHtmlString();});
    }
    [Fact] public async Task DetailShowsRecordedAmountWithoutInventedCurrencyOrDownstreamCompletion()
    {
        var html=await Render("detail");Assert.Contains("Recorded amount: 12",html);Assert.Contains("Currency: unavailable",html);Assert.Contains("received goods, stock, invoice matching",html);Assert.Contains("&lt;script&gt;recorded",html);Assert.DoesNotContain("<script>",html);
    }
    [Fact] public async Task SupplierQueueOpensEncodedCurrentOwnedHeader()
    {
        var html=await Render("rows");Assert.Contains("PO-1",html);Assert.Contains("/procurement/orders/order%2Fa%3Fb",html);Assert.Contains("DRAFT",html);Assert.Contains("Expected delivery",html);
    }
    [Theory][InlineData("empty")][InlineData("unconfirmed")]
    public async Task EmptyAndUnconfirmedEvidenceAreDistinct(string mode)
    {
        var html=await Render(mode);Assert.Contains(mode=="empty"?"No company-owned purchase orders were returned":"No company-owned purchase-order source is confirmed",html);Assert.DoesNotContain("PO-1",html);
    }
    [Fact] public async Task RecordedHistoryShowsActualLineLinkReceiverAndInactiveRecordWithoutInventingUnits()
    {
        var html=await Render("history");Assert.Contains("Recorded purchase-order lines",html);Assert.Contains("Recorded receipt history (active and inactive)",html);Assert.Contains("Line: line",html);Assert.Contains("receiver",html);Assert.Contains("Active indicator: N",html);Assert.Contains("Unit: unavailable",html);Assert.Contains("&lt;script&gt;receipt",html);Assert.Contains("&lt;script&gt;item",html);Assert.DoesNotContain("<script>",html);
    }
    [Fact] public async Task RecordedCurrencyAndDistinctUnitsAreVisibleWithoutConversion()
    {
        var html=System.Net.WebUtility.HtmlDecode(await Render("measurements"));
        Assert.Contains("Currency: KWD",html);Assert.Contains("m³",html);Assert.Contains("Unit: bbl",html);
        Assert.Contains("catalog validity, conversions",html);Assert.Contains("without comparison or aggregation",html);
        Assert.DoesNotContain("Currency: USD",html);Assert.DoesNotContain("Remaining quantity",html);
    }
    [Fact] public async Task EmptyConfirmedChildSetsDoNotClaimStockOrReceivingCompletion()
    {
        var html=await Render("detail");Assert.Contains("No recorded lines were returned",html);Assert.Contains("Stock and receiving completion are unconfirmed",html);
    }
    [Fact] public async Task ActualCompanyQueueShowsReturnedCountsAndTheSameOrderDetail()
    {
        var html=await Render("company");Assert.Contains("Supplier restriction: All suppliers",html);
        Assert.Contains("Recorded active orders: 1",html);Assert.Contains("Showing: 1",html);
        Assert.Contains("/procurement/orders/order%2Fa%3Fb",html);Assert.Contains("Recorded order status",html);
        Assert.Contains("Recorded status does not verify approval",html);Assert.Contains("Recorded amount / currency",html);
    }
    [Fact] public async Task ReceivingEvidenceQueuesExplainScopeAndWarehouseGaps()
    {
        var html=await Render("history");
        Assert.Contains("Receiver and warehouse evidence queues for this order",html);
        Assert.Contains("Receipt counts use this reviewed order only",html);
        Assert.Contains("Quantities are not summed across units",html);
        Assert.Contains("stock movement and supplier return evidence remain unconfirmed",html);
        Assert.Contains("Receipt / line",html);Assert.Contains("receipt",html);
    }
}
