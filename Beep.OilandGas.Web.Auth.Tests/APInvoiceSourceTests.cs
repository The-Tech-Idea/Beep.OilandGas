using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class APInvoiceSourceTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static AP_INVOICE Invoice()=>new(){AP_INVOICE_ID="invoice/a?b",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",INVOICE_NUMBER="Bill-1",VENDOR_BA_ID="supplier",ACTIVE_IND="Y",STATUS="DRAFT",TOTAL_AMOUNT=12m,PAID_AMOUNT=null,BALANCE_DUE=0m,CURRENCY_CODE="KWD",REMARK="<script>recorded</script>"};
    private static APInvoiceSourceReview Review()=>APInvoiceSourceReview.Create(Scope,"accountant",Invoice(),[]);
    private static AP_INVOICE_LINE Line()=>new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice/a?b",LINE_NUMBER=3,ITEM_DESCRIPTION="<script>invoice item</script>",QUANTITY=0m,QUANTITY_UOM="m³",UNIT_PRICE=null,LINE_TOTAL=12m,CURRENCY_CODE="KWD",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="order-line",ACTIVE_IND="N",ROW_CREATED_BY="recorder"};
    private static APInvoiceSourceReview LineReview()=>APInvoiceSourceReview.Create(Scope,"accountant",Invoice(),[Line()]);
    private static CompanyClientContext Selected(){var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(Scope,["ConfiguredAccountant"],["Accounting.View"])));return company;}
    [Fact] public void CanonicalAPTypeAndBeepManifestShapeHaveOneDataOwner()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(AP_INVOICE).Assembly);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE));
        var key=Assert.Single(schema.Fields,row=>row.IsKey);Assert.Equal("AP_INVOICE_ID",key.FieldName);Assert.False(key.AllowDBNull);
        foreach(var (column,size) in new[]{("OWNER_TENANT_ID",64),("OWNER_COMPANY_BA_ID",128),("CURRENCY_CODE",32)})
        {var field=Assert.Single(schema.Fields,row=>row.FieldName==column);Assert.True(field.AllowDBNull);Assert.Equal(size,field.Size1);Assert.Equal("System.String",field.Fieldtype);}
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("identity")][InlineData("unassigned")][InlineData("supplier-owner")]
    public void UnconfirmedInvoiceOwnershipRefuses(string fault)
    {
        var invoice=Invoice();if(fault=="tenant")invoice.OWNER_TENANT_ID="other";if(fault=="company")invoice.OWNER_COMPANY_BA_ID="other";if(fault=="identity")invoice.AP_INVOICE_ID="other";
        if(fault=="unassigned")invoice.OWNER_COMPANY_BA_ID=null;if(fault=="supplier-owner"){invoice.VENDOR_BA_ID="company";invoice.OWNER_COMPANY_BA_ID="other";}
        Assert.Throws<InvalidOperationException>(()=>APInvoiceOwnership.Require(Scope,"invoice/a?b",invoice));
    }
    [Theory][InlineData("null")][InlineData("duplicate")][InlineData("supplier")][InlineData("inactive")]
    public void UnconfirmedSupplierQueryCannotPublish(string fault)
    {
        var invoice=Invoice();if(fault=="supplier")invoice.VENDOR_BA_ID="other";if(fault=="inactive")invoice.ACTIVE_IND="N";
        Assert.Throws<InvalidOperationException>(()=>APInvoiceOwnership.RequireVendorRows(Scope,"supplier",fault=="null"?null:fault=="duplicate"?[invoice,invoice]:[invoice]));
    }
    [Fact] public void ReviewFreezesHeaderAndSurvivesWebJson()
    {
        var invoice=Invoice();var review=APInvoiceSourceReview.Create(Scope,"accountant",invoice,[]);invoice.TOTAL_AMOUNT=99m;
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);var returned=JsonSerializer.Deserialize<APInvoiceSourceReview>(JsonSerializer.Serialize(review,options),options)!;
        returned.Require(Scope,"accountant","invoice/a?b");Assert.Equal(12m,returned.Invoice.TOTAL_AMOUNT);Assert.Null(returned.Invoice.PAID_AMOUNT);Assert.Equal(0m,returned.Invoice.BALANCE_DUE);Assert.Equal("KWD",returned.Invoice.CURRENCY_CODE);
    }
    [Theory][InlineData("amount")][InlineData("currency")][InlineData("paid")][InlineData("status")][InlineData("audit")]
    public void ChangedRecordedContentCannotReuseReview(string fault)
    {
        var review=Review();if(fault=="amount")review.Invoice.TOTAL_AMOUNT=99m;if(fault=="currency")review.Invoice.CURRENCY_CODE="USD";if(fault=="paid")review.Invoice.PAID_AMOUNT=12m;if(fault=="status")review.Invoice.STATUS="PAID";if(fault=="audit")review.Invoice.ROW_CREATED_BY="other";
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"accountant","invoice/a?b"));
    }
    [Theory][InlineData("actor")][InlineData("company")][InlineData("revision")][InlineData("time")]
    public void ReturnedReviewMustMatchOriginalScopeAndProvenance(string fault)
    {
        var review=Review();review=fault switch{"actor"=>review with{ActorUserId="other"},"company"=>review with{Company=new("tenant","other")},"revision"=>review with{Revision="bad"},_=>review with{RetrievedAtUtc=default}};
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"accountant","invoice/a?b"));
    }
    [Fact] public void ProcurementPermissionDoesNotGrantAccountingAccess()
    {
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredAccountant")],"app"));
        Assert.True(APInvoiceReadChecks.CanRead(user,Scope,new(Scope,["ConfiguredAccountant"],["Accounting.View"])));
        Assert.False(APInvoiceReadChecks.CanRead(user,Scope,new(Scope,["ConfiguredAccountant"],["Procurement.View"])));
        Assert.False(APInvoiceReadChecks.CanRead(user,Scope,new(Scope,["Other"],["Accounting.View"])));
        Assert.False(APInvoiceReadChecks.CanRead(user,Scope,new(new("tenant","other"),["ConfiguredAccountant"],["Accounting.View"])));
    }
    [Fact] public async Task DetailFreezesRecordedValuesWithoutSynthesizingPayment()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var review=Review();
        await state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(review));review.Invoice.CURRENCY_CODE="USD";
        Assert.Equal("KWD",state.Detail!.Currency);Assert.Null(state.Detail.PaidAmount);Assert.Equal(0m,state.Detail.BalanceDue);Assert.Equal(review.RetrievedAtUtc,state.RetrievedAtUtc);Assert.Equal("/accounting/payables/invoices/invoice%2Fa%3Fb",state.Detail.Route);
    }
    [Theory][InlineData("missing")][InlineData("wrong")][InlineData("changed")]
    public async Task InvalidReloadClearsPriorHeader(string fault)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(Review()));
        var review=Review();if(fault=="wrong")review=review with{ActorUserId="other"};if(fault=="changed")review.Invoice.STATUS="PAID";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(fault=="missing"?null:review)));
        Assert.Null(state.Detail);Assert.Null(state.SourceRevision);Assert.Null(state.RetrievedAtUtc);
    }
    [Fact] public async Task CompanyAwayAndBackCannotAdoptLateInvoiceIdentity()
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);var pending=new TaskCompletionSource<APInvoiceSourceReview?>();var load=state.LoadDetailAsync("invoice/a?b",_=>pending.Task);
        company.Clear();Assert.True(company.TrySelect(company.Version,"accountant",new(Scope,["ConfiguredAccountant"],["Accounting.View"])));pending.SetResult(Review());await load;Assert.Null(state.Detail);
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static rendering has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    [Theory][InlineData("detail")][InlineData("lines")][InlineData("list")][InlineData("empty")][InlineData("unconfirmed")]
    public async Task ActualPanelShowsRecordedValuesAndDistinctEvidenceStates(string mode)
    {
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        if(mode is "detail" or "lines")await state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(mode=="lines"?LineReview():Review()));
        if(mode is "list" or "empty")await state.LoadVendorAsync("supplier",_=>Task.FromResult(mode=="empty"?new List<AP_INVOICE>():[Invoice()]));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var result=await renderer.RenderComponentAsync<Fixtures.APInvoiceRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return result.ToHtmlString();});
        if(mode=="detail"){Assert.Contains("Recorded paid amount: Unavailable",html);Assert.Contains("Recorded balance: 0",html);Assert.Contains("Currency: KWD",html);Assert.Contains("exact PO/receipt links",html);Assert.Contains("&lt;script&gt;recorded",html);Assert.DoesNotContain("<script>",html);}
        if(mode=="list"){Assert.Contains("Bill-1",html);Assert.Contains("/accounting/payables/invoices/invoice%2Fa%3Fb",html);Assert.Contains("KWD",html);}
        if(mode=="empty")Assert.Contains("does not establish no historical bills or no liability",html);
        if(mode=="unconfirmed")Assert.Contains("No company-owned AP invoice source is confirmed",html);
        if(mode=="detail")Assert.Contains("No recorded invoice lines were returned",html);
        if(mode=="lines"){Assert.Contains("Recorded vendor invoice lines (active and inactive)",html);Assert.Contains("Unit: m",System.Net.WebUtility.HtmlDecode(html));Assert.Contains("Price: Unavailable",html);Assert.Contains("Amount: 12",html);Assert.Contains("Active indicator: N",html);Assert.Contains("recorder",html);Assert.Contains("&lt;script&gt;invoice item",html);Assert.DoesNotContain("<script>",html);Assert.Contains("Ownership and matching: unconfirmed",html);Assert.DoesNotContain("href=\"/procurement",html);}
    }
    [Fact] public void CanonicalAPLineDeclaresItsOwnKeyAndNullableBoundedMeasurementFields()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(AP_INVOICE_LINE).Assembly);
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(AP_INVOICE_LINE));var key=Assert.Single(schema.Fields,row=>row.IsKey);
        Assert.Equal("AP_INVOICE_LINE_ID",key.FieldName);Assert.False(key.AllowDBNull);
        foreach(var (column,size) in new[]{("QUANTITY_UOM",64),("CURRENCY_CODE",32),("PURCHASE_ORDER_ID",128),("PO_LINE_ITEM_ID",128)})
        {var field=Assert.Single(schema.Fields,row=>row.FieldName==column);Assert.True(field.AllowDBNull);Assert.Equal(size,field.Size1);}
    }
    [Theory][InlineData("null")][InlineData("duplicate")][InlineData("foreign")][InlineData("missing-id")][InlineData("partial-po")]
    public void UnconfirmedAPLineMembershipOrReferencesCannotPublish(string fault)
    {
        var line=Line();if(fault=="foreign")line.AP_INVOICE_ID="other";if(fault=="missing-id")line.AP_INVOICE_LINE_ID="";if(fault=="partial-po")line.PO_LINE_ITEM_ID=null;
        Assert.Throws<InvalidOperationException>(()=>APInvoiceSourceReview.Create(Scope,"accountant",Invoice(),fault=="null"?null!:fault=="duplicate"?[line,line]:[line]));
    }
    [Theory][InlineData("quantity")][InlineData("unit")][InlineData("currency")][InlineData("reference")][InlineData("audit")]
    public void ChangedAPLineContentCannotReuseItsReview(string fault)
    {
        var review=LineReview();var line=review.Lines[0];if(fault=="quantity")line.QUANTITY=9m;if(fault=="unit")line.QUANTITY_UOM="bbl";if(fault=="currency")line.CURRENCY_CODE="USD";if(fault=="reference")line.PO_LINE_ITEM_ID="other";if(fault=="audit")line.ROW_CREATED_BY="other";
        Assert.Throws<InvalidOperationException>(()=>review.Require(Scope,"accountant","invoice/a?b"));
    }
    [Fact] public async Task APLineJsonAndFrozenStateRetainRecordedZeroNullUnitsAndHistory()
    {
        var options=new JsonSerializerOptions(JsonSerializerDefaults.Web);var review=JsonSerializer.Deserialize<APInvoiceSourceReview>(JsonSerializer.Serialize(LineReview(),options),options)!;
        review.Require(Scope,"accountant","invoice/a?b");using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(review));review.Lines[0].QUANTITY_UOM="bbl";
        var line=Assert.Single(state.Lines!);Assert.Equal("m³",line.Unit);Assert.Equal(0m,line.Quantity);Assert.Null(line.UnitPrice);Assert.Equal("N",line.ActiveIndicator);Assert.Equal("order-line",line.OrderLineId);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadDetailAsync("invoice/a?b",_=>Task.FromResult<APInvoiceSourceReview?>(review)));Assert.Null(state.Lines);Assert.Null(state.Detail);
    }
}
