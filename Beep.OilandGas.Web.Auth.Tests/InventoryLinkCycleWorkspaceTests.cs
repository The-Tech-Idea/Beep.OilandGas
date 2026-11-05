#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkCycleWorkspaceTests
{
    internal static CompanyClientContext Company()
    {
        var context=new CompanyClientContext();context.TrySelect(context.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.ReopenInventoryLink"]));return context;
    }
    internal static async Task<PurchaseOrderInventoryLinkCycleResult> Source(InventoryLinkCycleTests.Fixture f)
    {
        var purchase=(await f.Service.GetSourceReviewAsync(f.Command.Company,f.Command.OrderId,"actor"))!;
        var history=await f.Service.GetInventoryLinkHistoryAsync(f.Command.Company,f.Command.OrderId,f.Command.LineId,"actor",()=>Task.CompletedTask);return new(purchase,history);
    }
    [Theory][InlineData("resolved")][InlineData("pending")][InlineData("unknown")][InlineData("result")][InlineData("read-back")][InlineData("late-company")][InlineData("clear")][InlineData("dispose")][InlineData("reason")]
    public async Task CycleConfirmationRequiresExactFreshCompleteEvidenceAndRetainsUncertainOriginalIntent(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create(pending:mode=="pending");using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));await state.LoadLinkCycleReviewAsync("po-line",f.Command.Action,(_,_,_)=>Task.FromResult(f.Review));
        var calls=0;PurchaseOrderInventoryLinkCycleSubmission? original=null;
        async Task<PurchaseOrderInventoryLinkCycleResult> Record(PurchaseOrderInventoryLinkCycleSubmission value)
        {
            ++calls;original??=CompanyInventorySourceReview.Copy(value);Assert.Equal(original.Command,value.Command);f.Command=value.Command;f.Review=value.Review;
            var result=await f.Record();if(mode=="unknown" && calls==1)throw new HttpRequestException();
            if(mode=="result")result.Purchase.Lines[0].INVENTORY_ITEM_ID="forged";
            if(mode=="late-company")company.TrySelect(company.Clear(),"other",new(new("tenant","other"),["ConfiguredCompanyRole"],[]));
            if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();return result;
        }
        async Task<PurchaseOrderInventoryLinkCycleResult> Read(string order,string line){var result=await Source(f);if(mode=="read-back")result.Purchase.Header.DESCRIPTION="forged";return result;}
        var save=state.RecordLinkCycleAsync(mode=="reason"?"two\nlines":"Explicit cycle reason.",Record,Read);
        if(mode is "unknown" or "result" or "read-back" or "reason")await Assert.ThrowsAnyAsync<Exception>(()=>save);else await save;
        if(mode=="reason"){Assert.Equal(0,calls);Assert.False(state.HasUnconfirmedLinkCycleIntent);return;}
        if(mode is "resolved" or "pending")
        {
            Assert.NotNull(state.LinkCycleResult);Assert.False(state.HasUnconfirmedLinkCycleIntent);Assert.False(state.Lines![0].HasPreparationEvidence);
            state.LinkCycleHistory!.History.Entries[0].REASON="tampered";Assert.NotEqual("tampered",state.LinkCycleHistory!.History.Entries[0].REASON);
        }
        else
        {
            Assert.True(state.HasUnconfirmedLinkCycleIntent);Assert.Null(state.LinkCycleResult);
            if(mode=="unknown")
            {
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadLinkTargetsAsync(()=>throw new Exception()));
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReceiverReviewAsync("receipt",(_,_)=>throw new Exception()));
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadLinkDecisionReviewAsync("po-line",(_,_)=>throw new Exception()));
                await state.RecordLinkCycleAsync("Replacement reason ignored.",Record,Read);Assert.Equal(2,calls);Assert.False(state.HasUnconfirmedLinkCycleIntent);Assert.NotNull(state.LinkCycleResult);
            }
        }
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("clear")][InlineData("null")][InlineData("source")][InlineData("action")]
    public async Task CycleReviewFreezesEvidenceAndDropsLateCompanyOrSource(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create();using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));
        var pending=new TaskCompletionSource<PurchaseOrderInventoryLinkCycleReview>();var load=state.LoadLinkCycleReviewAsync("po-line",f.Command.Action,(_,_,_)=>pending.Task);
        if(mode=="company")company.Clear();if(mode=="clear")state.Clear();if(mode=="source")f.Review.Purchase.Header.DESCRIPTION="changed";if(mode=="action")f.Review=f.Review with{Action=0};
        pending.SetResult(mode=="null"?null!:f.Review);if(mode is "null" or "source" or "action")await Assert.ThrowsAsync<InvalidOperationException>(()=>load);else await load;
        if(mode=="valid"){Assert.NotNull(state.LinkCycleReview);state.LinkCycleReview!.Purchase.Header.DESCRIPTION="tampered";Assert.NotEqual("tampered",state.LinkCycleReview!.Purchase.Header.DESCRIPTION);}else Assert.Null(state.LinkCycleReview);
    }
}
#endif
