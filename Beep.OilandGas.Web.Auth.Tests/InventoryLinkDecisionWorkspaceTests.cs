using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkDecisionWorkspaceTests
{
    [Fact] public async Task LaterIndependentReviewInvalidatesAnEarlierReceiverReview()
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));
        var pending=new TaskCompletionSource<PurchaseReceiptDecisionReview>();var receiving=state.LoadReceiverReviewAsync("receipt",(_,_)=>pending.Task);
        await state.LoadLinkDecisionReviewAsync("po-line",(_,_)=>Task.FromResult(f.Review));
        pending.SetResult(new(f.Review.Purchase,"receipt","binding"));await receiving;Assert.Null(state.ReceiverReview);Assert.NotNull(state.LinkDecisionReview);
    }
    internal static CompanyClientContext Company(string actor="reviewer")
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.RecordInventoryLinkDecision"])));return company;}
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("unknown")][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")][InlineData("result")][InlineData("header")][InlineData("missing-read")][InlineData("reason")][InlineData("choice")]
    public async Task ReviewerSaveRequiresExactOwnedReadBackAndRetainsOriginalUncertainIntent(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create(mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve);
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));
        await state.LoadLinkDecisionReviewAsync("po-line",(_,_)=>Task.FromResult(f.Review));
        var calls=0;PurchaseOrderInventoryLinkDecisionSubmission? original=null;
        async Task<PO_LINE_ITEM> Record(PurchaseOrderInventoryLinkDecisionSubmission value)
        {
            ++calls;original??=CompanyInventorySourceReview.Copy(value);Assert.Equal(original.Command,value.Command);f.Command=value.Command;f.Review=value.Review;
            var result=await f.Record();if(mode=="unknown" && calls==1)throw new HttpRequestException();
            if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="account")InventoryWorkspaceStateTests.Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
            if(mode=="result")result.INVENTORY_ITEM_ID="wrong";return result;
        }
        async Task<PurchaseOrderSourceReview?> Read(string id)
        {
            if(mode=="missing-read")return null;var source=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,id,"reviewer"))!;
            if(mode=="header"){source.Header.DESCRIPTION="changed";source=PurchaseOrderSourceReview.Create(source.Company,"reviewer",source.Header,source.Lines,source.Receipts);}return source;
        }
        var save=state.RecordLinkDecisionAsync(mode=="choice"?0:f.Command.Decision,mode=="reason"?"":"Independent source review.",Record,Read);
        if(mode is "unknown" or "result" or "header" or "missing-read" or "choice" or "reason")await Assert.ThrowsAnyAsync<Exception>(()=>save);else await save;
        Assert.False(state.LinkDecisionBusy);
        if(mode is "reason" or "choice"){Assert.Equal(0,calls);Assert.False(state.HasUnconfirmedLinkDecisionIntent);return;}
        if(mode is "approve" or "reject"){Assert.NotNull(state.LinkDecisionResult);Assert.False(state.HasUnconfirmedLinkDecisionIntent);Assert.Equal("reviewer",Assert.Single(state.Lines!).InventoryDecision!.Actor);}
        else
        {
            Assert.True(state.HasUnconfirmedLinkDecisionIntent);Assert.Null(state.LinkDecisionResult);Assert.Equal(original!.Command,state.LinkDecisionIntent!.Command);
            state.LinkDecisionIntent!.Review.Target.Item.LOCATION_ID="tampered";Assert.Equal("bin",state.LinkDecisionIntent!.Review.Target.Item.LOCATION_ID);
            if(mode=="unknown")
            {
                state.Clear();await state.LoadDetailAsync("order",Read);Assert.True(state.CanReconcileLinkDecision);
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>())));
                await state.RecordLinkDecisionAsync(PurchaseOrderInventoryLinkDecision.Reject,"Replacement choice and reason must be ignored.",Record,Read);
                Assert.Equal(2,calls);Assert.False(state.HasUnconfirmedLinkDecisionIntent);Assert.Equal(2,f.Existing.Existing.Native.Writes);
            }
        }
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("clear")][InlineData("actor")][InlineData("source")][InlineData("null")][InlineData("preparer")]
    public async Task IndependentReviewIsFrozenAndCannotPublishLateOrInvalidEvidence(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();using var company=Company(mode=="preparer"?"actor":"reviewer");using var state=new PurchaseOrderWorkspaceState(company);
        var source=PurchaseOrderSourceReview.Create(f.Command.Company,company.Selection!.UserId,f.Review.Purchase.Header,f.Review.Purchase.Lines,f.Review.Purchase.Receipts);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(source));
        var pending=new TaskCompletionSource<PurchaseOrderInventoryLinkDecisionReview>();var load=state.LoadLinkDecisionReviewAsync("po-line",(_,_)=>pending.Task);
        if(mode=="preparer"){await Assert.ThrowsAsync<InvalidOperationException>(()=>load);return;}
        if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="clear")state.Clear();if(mode=="actor")f.Review=f.Review with{Purchase=f.Review.Purchase with{ActorUserId="other"}};if(mode=="source")f.Review.Target.Item.LOCATION_ID="changed";
        pending.SetResult(mode=="null"?null!:f.Review);
        if(mode is "actor" or "source" or "null")await Assert.ThrowsAsync<InvalidOperationException>(()=>load);else await load;
        if(mode=="valid"){Assert.NotNull(state.LinkDecisionReview);state.LinkDecisionReview!.Target.Item.LOCATION_ID="tampered";Assert.Equal("bin",state.LinkDecisionReview!.Target.Item.LOCATION_ID);}
        else Assert.Null(state.LinkDecisionReview);
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("intent")][InlineData("binding")][InlineData("company")]
    public async Task PendingPreparerIntentCanResolveReadOnlyFromExactIndependentDecision(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create(mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve);
        using var company=Company("actor");using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.PreparationReview.Purchase));
        await state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{f.Item}));
        await state.LoadLinkPreparationReviewAsync("po-line","target",(_,_,_)=>Task.FromResult(f.PreparationReview));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordLinkPreparationAsync(f.Preparation.Reason,value=>{f.Preparation=value.Command;f.PreparationReview=value.Review;return Task.FromException<PO_LINE_ITEM>(new HttpRequestException());},_=>throw new Exception()));
        // This pending intent needs its own actual recorded preparation, not the fixture's different GUID.
        var prepared=f.Stored;prepared.INVENTORY_LINK_REVIEW_JSON=PurchaseOrderInventoryLinkProposalProof.Create(f.Preparation,f.PreparationReview,prepared);
        var decisionSource=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","reviewer"))!;f.Review=PurchaseOrderInventoryLinkDecisionReview.Create(decisionSource,f.Target("reviewer"),"po-line","binding");f.Command=f.Command with{ReviewRevision=f.Review.Revision};await f.Record();
        var source=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","actor"))!;var resolution=new PurchaseOrderInventoryLinkResolution(source,mode=="binding"?"changed":"binding");
        var read=state.ReconcileLinkPreparationResolutionAsync(value=>{
            if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");
            if(mode=="intent")throw new InvalidOperationException();return Task.FromResult(resolution);});
        if(mode is "binding" or "intent")await Assert.ThrowsAsync<InvalidOperationException>(()=>read);else await read;
        if(mode is "approve" or "reject"){Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResolution);Assert.Null(state.LinkPreparationResult);}
        else{Assert.True(state.HasUnconfirmedLinkPreparationIntent);Assert.Null(state.LinkPreparationResolution);}
        Assert.Equal(2,f.Existing.Existing.Native.Writes);
    }
}
#endif
