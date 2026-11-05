using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.ApiService.Tests;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ReceiptStockWorkspaceTests
{
    [Theory][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")]
    public async Task LateStockPageCannotPublishAcrossOriginalWorkspaceBoundary(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();
        var evidence=await f.Inventory.GetStockRecoveryPageAsync(new("tenant","company"),"actor",1,25,f.Receiving.Existing.PO,f.Authority);
        using var company=Company();using var state=new InventoryWorkspaceState(company);var pending=new TaskCompletionSource<PurchaseReceiptStockIntentPage>();
        var work=state.LoadStockPageAsync(1,25,(_,_)=>pending.Task);Assert.True(state.StockBusy);
        if(mode=="company")Select(company,"other");if(mode=="account")Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
        pending.SetResult(evidence);await work;Assert.Null(state.StockPage);Assert.Null(state.StockIndex);Assert.False(state.StockBusy);Assert.Equal(3,f.Native.Writes);
    }
    internal static CompanyClientContext Company(){var c=new CompanyClientContext();Select(c);return c;}
    internal static void Select(CompanyClientContext c,string owner="company",string actor="actor")=>Assert.True(c.TrySelect(c.Clear(),actor,new(new("tenant",owner),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Inventory.RecordReceiptStock","Inventory.RecordSupplierReturn"])));
    [Theory][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")]
    public async Task LateStockDiscoveryCannotPublishAcrossOriginalWorkspaceBoundary(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();
        var evidence=await f.Inventory.GetStockRecoveryIndexAsync(new("tenant","company"),"actor",f.Receiving.Existing.PO,f.Authority);
        using var company=Company();using var state=new InventoryWorkspaceState(company);var pending=new TaskCompletionSource<PurchaseReceiptStockIntentIndex>();
        var work=state.LoadStockIndexAsync(()=>pending.Task);Assert.True(state.StockBusy);
        if(mode=="company")Select(company,"other");if(mode=="account")Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
        pending.SetResult(evidence);await work;Assert.Null(state.StockIndex);Assert.False(state.StockBusy);Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("exact")][InlineData("later")][InlineData("missing")][InlineData("wrong-intent")][InlineData("changed-command")][InlineData("closing-drift")][InlineData("historical")]
    public async Task SavedEvidenceReconcilesOnlyOriginalPendingIntentAndNeverSendsAnotherCommand(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();using var company=Company();using var state=new InventoryWorkspaceState(company);
        using var procurement=new PurchaseOrderWorkspaceState(company,state);
        await state.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review));f.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordStockAsync(PurchaseReceiptStockMovementKind.Receive,4,null,"Original",submission=>{f.Submission=submission;return f.Record();},(_,_)=>throw new InvalidOperationException()));
        f.Native.FailCommit=false;var original=state.StockIntent!;Assert.True(procurement.WarehouseWorkBlocked);state.Clear();
        if(mode=="later"){await f.Prepare(quantity:4);await f.Record();}
        if(mode=="historical")f.Native.Rows.RemoveAll(row=>row is INVENTORY_ITEM);
        var baseline=f.Native.Writes;var calls=0;
        async Task<PurchaseReceiptStockRecovery> Read(string id)
        {
            calls++;if(mode=="missing")throw new InvalidOperationException("Saved evidence is missing.");
            var result=await f.Inventory.GetStockRecoveryAsync(new("tenant","company"),id,"actor",f.Receiving.Existing.PO,f.Authority);
            if(mode=="changed-command")result.Entry.COMMAND_JSON="{}";
            if(mode=="closing-drift" && calls==1)f.Item.LOCATION_ID="closing bin";
            return result;
        }
        if(mode is "exact" or "later" or "historical")
        {
            await state.LoadStockRecoveryAsync(original.Command.IntentId,Read);Assert.False(state.HasUnconfirmedStockIntent);Assert.False(procurement.WarehouseWorkBlocked);
            Assert.Equal(original.Command,state.StockRecovery!.OriginalSubmission().Command);Assert.Equal(mode=="exact",state.StockResult is not null);
            if(mode=="historical"){Assert.Null(state.Detail);Assert.Null(state.StockReview);Assert.Null(state.StockResult);}else Assert.Equal(mode=="later"?18m:14m,state.Detail!.Quantity);Assert.Equal(2,calls);
            state.StockRecovery!.Entry.COMMAND_JSON="caller";Assert.Equal(original.Command,state.StockRecovery!.OriginalSubmission().Command);
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadStockRecoveryAsync(mode=="wrong-intent"?Guid.NewGuid().ToString("N"):original.Command.IntentId,Read));
            Assert.True(state.HasUnconfirmedStockIntent);Assert.True(procurement.WarehouseWorkBlocked);Assert.Null(state.StockRecovery);Assert.Null(state.StockResult);Assert.Equal(original.Command,state.StockIntent!.Command);
            if(mode=="wrong-intent")Assert.Equal(0,calls);
        }
        Assert.Equal(baseline,f.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")]
    public async Task LateSavedRecoveryCannotPublishAcrossContextOrLifecycleChanges(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();await f.Record();using var company=Company();using var state=new InventoryWorkspaceState(company);
        var evidence=await f.Inventory.GetStockRecoveryAsync(new("tenant","company"),f.Submission.Command.IntentId,"actor",f.Receiving.Existing.PO,f.Authority);
        var pending=new TaskCompletionSource<PurchaseReceiptStockRecovery>();var reads=0;
        var work=state.LoadStockRecoveryAsync(f.Submission.Command.IntentId,_=>{reads++;return pending.Task;});
        if(mode=="company")Select(company,"other");if(mode=="account")Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
        pending.SetResult(evidence);await work;Assert.Null(state.StockRecovery);Assert.Null(state.StockIntent);Assert.Null(state.Detail);Assert.Null(state.StockResult);Assert.Equal(1,reads);Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("receive")][InlineData("return")][InlineData("unknown")][InlineData("readback")]
    public async Task ActualNativeRecordingRequiresFreshCompleteConfirmationAndRetainsOriginalRecovery(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();string? receipt=null;
        if(mode=="return"){await f.Record();receipt=f.Submission.Command.IntentId;await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,receipt);}
        var baseline=f.Native.Writes;using var company=Company();using var state=new InventoryWorkspaceState(company);
        await state.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review));
        var kind=mode=="return"?PurchaseReceiptStockMovementKind.Return:PurchaseReceiptStockMovementKind.Receive;var quantity=mode=="return"?1m:4m;
        var calls=0;
        Task<PurchaseReceiptInventoryReview> Record(PurchaseReceiptStockSubmission submission){calls++;f.Submission=submission;return f.Record();}
        Task<PurchaseReceiptInventoryReview> Read(string order,string id)=>Task.FromResult(PurchaseReceiptInventoryReview.Create(f.Submission.Review.Source.Purchase,
            CompanyInventorySourceReview.Create(new("tenant","company"),"actor",f.Item,f.Native.Rows.OfType<INVENTORY_TRANSACTION>().ToArray()),id,"binding"));
        if(mode=="unknown")f.Native.FailCommit=true;
        if(mode is "unknown" or "readback")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordStockAsync(kind,quantity,receipt,"Original reason",Record,mode=="readback"?(_,_)=>throw new InvalidOperationException():Read));
            var original=state.StockIntent!.Command;Assert.True(state.HasUnconfirmedStockIntent);Assert.Null(state.StockResult);Assert.Equal(baseline+3,f.Native.Writes);
            state.Clear();Assert.True(state.HasUnconfirmedStockIntent);Assert.Equal("/warehouse/receipt-stock/order/receipt",state.StockRecoveryRoute);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review)));
            f.Native.FailCommit=false;await state.RecordStockAsync((PurchaseReceiptStockMovementKind)0,999,"replacement","Replacement reason",Record,Read);
            Assert.Equal(original,state.StockIntent!.Command);Assert.Equal(2,calls);
        }
        else await state.RecordStockAsync(kind,quantity,receipt,"Original reason",Record,Read);
        Assert.False(state.HasUnconfirmedStockIntent);Assert.NotNull(state.StockResult);Assert.Equal(baseline+3,f.Native.Writes);Assert.Equal(mode=="return"?13m:14m,state.Detail!.Quantity);
        var display=state.StockReview!;display.Source.Inventory.Item.LOCATION_ID="changed";Assert.Equal("bin",state.StockReview!.Source.Inventory.Item.LOCATION_ID);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")]
    public async Task LateReviewIsDiscardedAfterOriginalContextIsLost(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();using var company=Company();using var state=new InventoryWorkspaceState(company);
        var pending=new TaskCompletionSource<PurchaseReceiptStockReview>();var read=state.LoadStockReviewAsync("order","receipt",(_,_)=>pending.Task);
        if(mode=="company")Select(company,"other");if(mode=="account")Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
        pending.SetResult(f.Submission.Review);await read;Assert.Null(state.StockReview);Assert.Null(state.Detail);
    }
    [Theory][InlineData("company")][InlineData("clear")][InlineData("dispose")]
    public async Task LateRecordedResultRetainsUnconfirmedIntentInsteadOfPublishing(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();using var company=Company();using var state=new InventoryWorkspaceState(company);
        await state.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review));var result=new TaskCompletionSource<PurchaseReceiptInventoryReview>();
        var work=state.RecordStockAsync(PurchaseReceiptStockMovementKind.Receive,4,null,"Original",submission=>{f.Submission=submission;return result.Task;},(_,_)=>throw new InvalidOperationException("Late result cannot start read-back."));
        if(mode=="company")Select(company,"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();result.SetResult(await f.Record());await work;
        Assert.True(state.HasUnconfirmedStockIntent);Assert.Null(state.StockResult);Assert.Null(state.Detail);Assert.NotNull(state.StockIntent);
    }
    [Theory][InlineData(0,4)][InlineData(1,0)][InlineData(1,9)][InlineData(2,1)]
    public async Task InvalidChoiceQuantityOrOriginalReceiptNeverCreatesIntent(int kind,int quantity)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();using var company=Company();using var state=new InventoryWorkspaceState(company);
        await state.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordStockAsync((PurchaseReceiptStockMovementKind)kind,quantity,null,"Reason",_=>throw new InvalidOperationException("No send"),(_,_)=>throw new InvalidOperationException("No read")));
        Assert.False(state.HasUnconfirmedStockIntent);Assert.Equal(0,f.Native.Writes);
    }
}
#endif
