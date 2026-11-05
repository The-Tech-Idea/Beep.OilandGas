using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class APLinePurchaseReadTests
{
    private static readonly CompanyScope Scope=new("tenant","company");
    private static ClaimsPrincipal User()=>new(new ClaimsIdentity([
        new Claim(ClaimTypes.NameIdentifier,"accountant"),new Claim(ClaimTypes.Role,"ConfiguredAccountant")],"test"));
    private static RepositoryCompanyAccess Access()=>new(Scope,["ConfiguredAccountant"],["Accounting.View","Procurement.View"]);
    private static APInvoiceSourceReview Invoice(decimal quantity=2)=>APInvoiceSourceReview.Create(Scope,"accountant",
        new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier"},
        [new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",QUANTITY=quantity}]);
    private static PurchaseOrderSourceReview Order(decimal quantity=3,string supplier="supplier")=>PurchaseOrderSourceReview.Create(Scope,"accountant",
        new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID=supplier},
        [new(){PO_LINE_ITEM_ID="po-line",PURCHASE_ORDER_ID="order",QUANTITY=quantity}],[]);
    [Fact] public async Task SameAuthorityAndSourcesReturnClosingReviewUnderOriginalArguments()
    {
        var auth=0;var invoices=0;var orders=0;var invoice=Invoice();
        var result=await APInvoiceReadChecks.ReadPurchaseSourceAsync(User(),"accountant","invoice","line",invoice.Revision,
            ()=>{auth++;return Task.FromResult((Access(),"binding"));},
            (scope,id,actor)=>{Assert.Equal(Scope,scope);Assert.Equal("invoice",id);Assert.Equal("accountant",actor);invoices++;return Task.FromResult<APInvoiceSourceReview?>(Invoice());},
            (scope,id,actor)=>{Assert.Equal(Scope,scope);Assert.Equal("order",id);Assert.Equal("accountant",actor);orders++;return Task.FromResult<PurchaseOrderSourceReview?>(Order());});
        Assert.Equal(6,auth);Assert.Equal(2,invoices);Assert.Equal(2,orders);
        Assert.Equal("order",result.RequireOrderId(Scope,"accountant","invoice","line",invoice.Revision));
    }
    public static IEnumerable<object[]> Revocations()=>from stage in Enumerable.Range(1,6)
        from fault in new[]{"role","accounting","procurement","company","binding"} where stage!=1 || fault!="company" select new object[]{stage,fault};
    [Theory][MemberData(nameof(Revocations))]
    public async Task EveryReadBoundaryRefusesRevokedAuthorityBeforeTheNextProviderRead(int stage,string fault)
    {
        var auth=0;var invoices=0;var orders=0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>APInvoiceReadChecks.ReadPurchaseSourceAsync(User(),"accountant","invoice","line",Invoice().Revision,
            ()=>{
                auth++;var access=Access();var binding="binding";
                if(auth==stage){
                    if(fault=="role")access=access with{Roles=["OtherRole"]};
                    if(fault=="accounting")access=access with{Permissions=["Procurement.View"]};
                    if(fault=="procurement")access=access with{Permissions=["Accounting.View"]};
                    if(fault=="company")access=access with{Scope=new("tenant","other")};
                    if(fault=="binding")binding=stage==1?"":"changed";
                }
                return Task.FromResult((access,binding));
            },(_,_,_)=>{invoices++;return Task.FromResult<APInvoiceSourceReview?>(Invoice());},
            (_,_,_)=>{orders++;return Task.FromResult<PurchaseOrderSourceReview?>(Order());}));
        Assert.Equal(stage,auth);
        Assert.Equal(stage<=2?0:stage<=4?1:2,invoices);
        Assert.Equal(stage<=3?0:stage<=5?1:2,orders);
    }
    [Theory][InlineData("missing-invoice")][InlineData("missing-order")][InlineData("first-invoice-changed")]
    [InlineData("closing-invoice-changed")][InlineData("closing-order-changed")][InlineData("supplier")]
    public async Task UnavailableOrChangedEvidenceCannotPublishTheCrossRoleReview(string fault)
    {
        var invoices=0;var orders=0;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>APInvoiceReadChecks.ReadPurchaseSourceAsync(User(),"accountant","invoice","line",Invoice().Revision,
            ()=>Task.FromResult((Access(),"binding")),
            (_,_,_)=>{invoices++;return Task.FromResult<APInvoiceSourceReview?>(fault=="missing-invoice"?null:
                Invoice(fault=="first-invoice-changed" || fault=="closing-invoice-changed" && invoices==2?9:2));},
            (_,_,_)=>{orders++;return Task.FromResult<PurchaseOrderSourceReview?>(fault=="missing-order"?null:
                Order(fault=="closing-order-changed" && orders==2?9:3,fault=="supplier"?"other":"supplier"));}));
        if(fault is "missing-invoice" or "first-invoice-changed"){Assert.Equal(1,invoices);Assert.Equal(0,orders);}
        if(fault is "missing-order" or "supplier"){Assert.Equal(1,invoices);Assert.Equal(1,orders);}
        if(fault=="closing-invoice-changed"){Assert.Equal(2,invoices);Assert.Equal(1,orders);}
        if(fault=="closing-order-changed"){Assert.Equal(2,invoices);Assert.Equal(2,orders);}
    }
    [Fact] public async Task MissingReviewedRevisionRefusesBeforeAuthorityOrProviderWork()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(()=>APInvoiceReadChecks.ReadPurchaseSourceAsync(User(),"accountant","invoice","line","invalid",
            ()=>throw new Exception("Authority must not run."),(_,_,_)=>throw new Exception("Invoice must not run."),(_,_,_)=>throw new Exception("Order must not run.")));
    }
    [Theory][InlineData("allowed")][InlineData("missing")][InlineData("revoked")]
    public async Task RetainedInventoryEvidenceRequiresItsReadGrantBeforeCrossRolePublication(string mode)
    {
        var order=Order();order.Lines[0].INVENTORY_LINK_REVIEW_JSON="{\"retained-target\":\"warehouse-evidence\"}";
        order=PurchaseOrderSourceReview.Create(Scope,"accountant",order.Header,order.Lines,order.Receipts);var calls=0;
        Task<APLinePurchaseSourceReview> Read()=>APInvoiceReadChecks.ReadPurchaseSourceAsync(User(),"accountant","invoice","line",Invoice().Revision,
            ()=>{++calls;var access=Access();if(mode!="missing" && (mode!="revoked" || calls<6))access=access with{Permissions=["Accounting.View","Procurement.View","Inventory.View"]};return Task.FromResult((access,"binding"));},
            (_,_,_)=>Task.FromResult<APInvoiceSourceReview?>(Invoice()),(_,_,_)=>Task.FromResult<PurchaseOrderSourceReview?>(order));
        if(mode=="allowed")Assert.NotNull(await Read());else await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Read());
    }
}
