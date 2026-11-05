using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryWorkspaceStateTests
{
    internal static CompanyClientContext Company(){var company=new CompanyClientContext();Select(company);return company;}
    internal static void Select(CompanyClientContext company,string owner="company",string actor="actor")=>Assert.True(company.TrySelect(company.Clear(),actor,new(new("tenant",owner),["Warehouse"],["Inventory.View"])));
    internal static INVENTORY_ITEM Item(string id="item/a?b")=>new(){INVENTORY_ITEM_ID=id,OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",ITEM_NAME="<script>item</script>",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="ea",CURRENCY_CODE="KWD",QUANTITY_ON_HAND=null};
    [Fact] public async Task ExactWarehouseAndExceptionCountsOpenOriginalItemWithNoQuantityAggregation()
    {
        using var company=Company();using var state=new InventoryWorkspaceState(company);var unknown=Item();var nonpositive=Item("negative");nonpositive.QUANTITY_ON_HAND=-2;nonpositive.LOCATION_ID=null;nonpositive.UNIT_OF_MEASURE="bbl";
        var other=Item("other");other.WAREHOUSE_ID="other";other.QUANTITY_ON_HAND=3;
        Assert.Null(state.Count(InventoryWorkspaceState.EvidenceQueue.All));await state.LoadQueueAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{unknown,nonpositive,other}));unknown.WAREHOUSE_ID="changed";
        state.SetWarehouse("warehouse");Assert.Equal(2,state.Count(InventoryWorkspaceState.EvidenceQueue.All));Assert.Equal(1,state.Count(InventoryWorkspaceState.EvidenceQueue.UnknownQuantity));Assert.Equal(1,state.Count(InventoryWorkspaceState.EvidenceQueue.MissingLocation));
        state.SetQueue(InventoryWorkspaceState.EvidenceQueue.UnknownQuantity);Assert.Equal("/warehouse/items/item%2Fa%3Fb",Assert.Single(state.VisibleRows!).Route);
        state.SetQueue(InventoryWorkspaceState.EvidenceQueue.NonpositiveQuantity);Assert.Equal("negative",Assert.Single(state.VisibleRows!).Id);
        Assert.Throws<InvalidOperationException>(()=>state.SetWarehouse("changed"));Assert.Throws<ArgumentOutOfRangeException>(()=>state.SetQueue((InventoryWorkspaceState.EvidenceQueue)123));
        company.Clear();Assert.Null(state.Rows);Assert.Null(state.Count(InventoryWorkspaceState.EvidenceQueue.All));Assert.Null(state.WarehouseFilter);Assert.Equal(InventoryWorkspaceState.EvidenceQueue.All,state.Queue);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")]
    public async Task LateQueueCannotPublishAfterOriginalContextIsLost(string mode)
    {
        using var company=Company();using var state=new InventoryWorkspaceState(company);var pending=new TaskCompletionSource<List<INVENTORY_ITEM>>();var load=state.LoadQueueAsync(()=>pending.Task);
        if(mode=="company")Select(company,"other");if(mode=="account")Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();pending.SetResult([Item()]);await load;Assert.Null(state.Rows);Assert.Null(state.RetrievedAtUtc);
    }
    [Fact] public async Task DetailRetainsInactiveHistoryAndUnknownAmountsAndFreezesNativeEvidence()
    {
        using var company=Company();using var state=new InventoryWorkspaceState(company);var item=Item();var movement=new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="movement",INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,ACTIVE_IND="N",QUANTITY=null,NOTES="Recorded reason"};
        var review=CompanyInventorySourceReview.Create(new("tenant","company"),"actor",item,[movement]);await state.LoadDetailAsync(item.INVENTORY_ITEM_ID,_=>Task.FromResult<CompanyInventorySourceReview?>(review));review.Item.LOCATION_ID="changed";review.Transactions[0].NOTES="changed";
        Assert.Equal("bin",state.Detail!.Location);Assert.Null(state.Detail.Quantity);Assert.Equal("N",Assert.Single(state.Movements!).Active);Assert.Equal("Recorded reason",state.Movements![0].Notes);Assert.Equal(review.RetrievedAtUtc,state.RetrievedAtUtc);
    }
    [Theory][InlineData("empty")][InlineData("failed")][InlineData("foreign")]
    public async Task EmptySourceAndFailedEvidenceRemainDistinct(string mode)
    {
        using var company=Company();using var state=new InventoryWorkspaceState(company);await state.LoadQueueAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{Item()}));state.SetWarehouse("warehouse");
        if(mode=="empty"){await state.LoadQueueAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>()));Assert.Equal(0,state.Count(InventoryWorkspaceState.EvidenceQueue.All));}
        else
        {
            var row=Item();row.OWNER_COMPANY_BA_ID="other";
            if(mode=="failed")await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadQueueAsync(()=>throw new HttpRequestException()));
            else await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadQueueAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{row})));
            Assert.Null(state.Rows);Assert.Null(state.Count(InventoryWorkspaceState.EvidenceQueue.All));
        }
        Assert.Null(state.WarehouseFilter);
    }
}
