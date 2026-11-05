#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ReceiptInventoryWorkspaceTests
{
    internal static async Task<PurchaseReceiptInventoryReview> Review()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();f.Existing.OrderLine.INVENTORY_ITEM_ID="item/a?b";await f.Prepare();await f.Record();
        var purchase=(await f.Existing.PO.GetSourceReviewAsync(new("tenant","company"),"order","actor"))!;
        var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="item/a?b",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",QUANTITY_ON_HAND=null};
        return PurchaseReceiptInventoryReview.Create(purchase,CompanyInventorySourceReview.Create(purchase.Company,"actor",item,[]),"receipt","binding");
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")][InlineData("actor")][InlineData("source")][InlineData("failed")][InlineData("null")]
    public async Task LinkedDisplayRequiresOriginalCurrentSourceAndDropsLateOrFailedEvidence(string mode)
    {
        var review=await Review();using var company=InventoryWorkspaceStateTests.Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(review.Purchase));
        var pending=new TaskCompletionSource<PurchaseReceiptInventoryReview>();var load=state.LoadInventoryLinkAsync("receipt",(_,_)=>pending.Task);
        Assert.True(state.InventoryLinkBusy);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadInventoryLinkAsync("receipt",(_,_)=>Task.FromResult(review)));
        if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="account")InventoryWorkspaceStateTests.Select(company,actor:"other");if(mode=="clear")state.ClearInventoryLink();if(mode=="dispose")state.Dispose();
        if(mode=="actor")review=review with{Purchase=review.Purchase with{ActorUserId="other"}};
        if(mode=="source")review.Inventory.Item.LOCATION_ID="changed";
        if(mode=="failed")pending.SetException(new HttpRequestException());else pending.SetResult(mode=="null"?null!:review);
        if(mode is "actor" or "source" or "null")await Assert.ThrowsAsync<InvalidOperationException>(()=>load);else if(mode=="failed")await Assert.ThrowsAsync<HttpRequestException>(()=>load);else await load;
        Assert.False(state.InventoryLinkBusy);
        if(mode=="valid")
        {
            Assert.NotNull(state.InventoryLink);var copy=state.InventoryLink!;copy.Inventory.Item.LOCATION_ID="tampered";Assert.Equal("bin",state.InventoryLink!.Inventory.Item.LOCATION_ID);
            state.ClearReceiverReview();Assert.Null(state.InventoryLink);
        }
        else Assert.Null(state.InventoryLink);
    }
}
#endif
