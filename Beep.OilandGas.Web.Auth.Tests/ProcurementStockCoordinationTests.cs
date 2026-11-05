using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProcurementStockCoordinationTests
{
    [Theory][InlineData("busy")][InlineData("unknown")]
    public async Task StockWorkFencesAllProcurementActionsUntilOriginalNativeIntentIsConfirmed(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();using var company=ReceiptStockWorkspaceTests.Company();
        using var stock=new InventoryWorkspaceState(company);using var procurement=new PurchaseOrderWorkspaceState(company,stock);
        await procurement.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Submission.Review.Source.Purchase));
        await stock.LoadStockReviewAsync("order","receipt",(_,_)=>Task.FromResult(f.Submission.Review));
        Assert.Null(procurement.Detail); // Stock capture invalidates previously displayed target/source evidence.
        var pending=new TaskCompletionSource<PurchaseReceiptInventoryReview>();
        Task<PurchaseReceiptInventoryReview> Read(string order,string receipt)=>Task.FromResult(PurchaseReceiptInventoryReview.Create(f.Submission.Review.Source.Purchase,
            CompanyInventorySourceReview.Create(new("tenant","company"),"actor",f.Item,f.Native.Rows.OfType<INVENTORY_TRANSACTION>().ToArray()),receipt,"binding"));
        var work=stock.RecordStockAsync(PurchaseReceiptStockMovementKind.Receive,4,null,"Original stock reason",submission=>{f.Submission=submission;return pending.Task;},Read);
        if(mode=="unknown")
        {
            f.Native.FailCommit=true;try{await f.Record();}catch(InvalidOperationException){}pending.SetException(new InvalidOperationException("Unknown commit"));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>work);Assert.True(stock.HasUnconfirmedStockIntent);
        }
        Assert.True(procurement.WarehouseWorkBlocked);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.LoadLinkTargetsAsync(()=>throw new Exception("No target call")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.LoadReceiverReviewAsync("receipt",(_,_)=>throw new Exception("No receiver call")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.LoadInventoryLinkAsync("receipt",(_,_)=>throw new Exception("No link call")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.LoadLinkDecisionReviewAsync("po-line",(_,_)=>throw new Exception("No decision call")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.LoadLinkCycleHistoryAsync("po-line",(_,_)=>throw new Exception("No history call")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>procurement.RecordReceiverAsync(8,2,"Reason",_=>throw new Exception("No receiver write"),_=>throw new Exception()));
        if(mode=="busy"){pending.SetResult(await f.Record());await work;}
        else
        {
            Assert.Equal("/warehouse/receipt-stock/order/receipt",procurement.WarehouseRecoveryRoute);procurement.Clear();Assert.True(procurement.WarehouseWorkBlocked);
            f.Native.FailCommit=false;await stock.RecordStockAsync(0,999,null,"Replacement",submission=>{f.Submission=submission;return f.Record();},Read);
        }
        Assert.False(procurement.WarehouseWorkBlocked);Assert.Equal(3,f.Native.Writes);Assert.Equal(14m,stock.Detail!.Quantity);
        await procurement.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Submission.Review.Source.Purchase));Assert.NotNull(procurement.Detail);
    }
    [Theory][InlineData("busy")][InlineData("unknown")]
    public async Task ProcurementCycleBlocksStockWithoutBlockingItsOwnOriginalIntentRecovery(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create();using var company=ReceiptStockWorkspaceTests.Company();using var stock=new InventoryWorkspaceState(company);using var procurement=new PurchaseOrderWorkspaceState(company,stock);
        await procurement.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));
        await procurement.LoadLinkCycleReviewAsync("po-line",f.Command.Action,(_,_,_)=>Task.FromResult(f.Review));
        var pending=new TaskCompletionSource<PurchaseOrderInventoryLinkCycleResult>();
        var work=procurement.RecordLinkCycleAsync("Original cycle",submission=>{f.Command=submission.Command;f.Review=submission.Review;return pending.Task;},(_,_)=>InventoryLinkCycleWorkspaceTests.Source(f));
        if(mode=="unknown"){await f.Record();pending.SetException(new InvalidOperationException("Unconfirmed result"));await Assert.ThrowsAsync<InvalidOperationException>(()=>work);}
        Assert.True(stock.ProcurementWorkBlocked);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>stock.LoadStockReviewAsync("order","receipt",(_,_)=>throw new Exception("No stock read")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>stock.RecordStockAsync(PurchaseReceiptStockMovementKind.Receive,1,null,"Reason",_=>throw new Exception("No stock write"),(_,_)=>throw new Exception()));
        Assert.Null(stock.StockIntent);
        if(mode=="busy"){pending.SetResult(await f.Record());await work;}
        else
        {
            Assert.Equal("/procurement/orders/order",stock.ProcurementRecoveryRoute);Assert.True(procurement.CanReconcileLinkCycle);
            await procurement.RecordLinkCycleAsync("Replacement ignored",submission=>{f.Command=submission.Command;f.Review=submission.Review;return f.Record();},(_,_)=>InventoryLinkCycleWorkspaceTests.Source(f));
        }
        Assert.False(stock.ProcurementWorkBlocked);Assert.NotNull(procurement.LinkCycleResult);
    }
    [Fact] public void CoordinationRequiresOneCompanyContextAndDoesNotRetainDisposedProcurement()
    {
        using var company=ReceiptStockWorkspaceTests.Company();using var other=ReceiptStockWorkspaceTests.Company();using var stock=new InventoryWorkspaceState(company);
        Assert.Throws<InvalidOperationException>(()=>new PurchaseOrderWorkspaceState(other,stock));
        using(var procurement=new PurchaseOrderWorkspaceState(company,stock))Assert.Throws<InvalidOperationException>(()=>new PurchaseOrderWorkspaceState(company,stock));
        using var replacement=new PurchaseOrderWorkspaceState(company,stock);Assert.False(stock.ProcurementWorkBlocked);
    }
}
#endif
