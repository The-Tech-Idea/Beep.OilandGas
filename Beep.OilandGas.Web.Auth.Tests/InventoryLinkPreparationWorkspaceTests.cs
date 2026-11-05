#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkPreparationWorkspaceTests
{
    [Fact] public async Task AReceiverReviewStartedEarlierCannotReplaceANewerPreparationReview()
    {
        var f=await Fixture.Create();using var company=InventoryWorkspaceStateTests.Company();using var state=new PurchaseOrderWorkspaceState(company);await f.Load(state);
        var pending=new TaskCompletionSource<PurchaseReceiptDecisionReview>();var receiving=state.LoadReceiverReviewAsync("receipt",(_,_)=>pending.Task);
        await state.LoadLinkPreparationReviewAsync("po-line",f.Review.Target.Item.INVENTORY_ITEM_ID,(_,_,_)=>Task.FromResult(f.Review));
        pending.SetResult(new(f.Review.Purchase,"receipt","binding"));await receiving;Assert.Null(state.ReceiverReview);Assert.NotNull(state.LinkPreparationReview);
    }
    internal sealed class Fixture
    {
        public PurchaseOrderInventoryLinkProposalReview Review=null!;
        public PO_LINE_ITEM? Stored;
        public static async Task<Fixture> Create()
        {
            var native=new PurchaseReceiptDecisionTests.Fixture();var purchase=(await native.Existing.PO.GetSourceReviewAsync(new("tenant","company"),"order","actor"))!;
            var item=new INVENTORY_ITEM{INVENTORY_ITEM_ID="target/a?b",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",QUANTITY_ON_HAND=null};
            return new(){Review=PurchaseOrderInventoryLinkProposalReview.Create(purchase,CompanyInventorySourceReview.Create(purchase.Company,"actor",item,[]),"po-line","binding")};
        }
        public PO_LINE_ITEM Record(PurchaseOrderInventoryLinkProposalSubmission submission)
        {
            var row=CompanyInventorySourceReview.Copy(submission.Review.Purchase.Lines.Single(line=>line.PO_LINE_ITEM_ID==submission.Command.LineId));row.ROW_CHANGED_BY=submission.Command.ActorUserId;row.ROW_CHANGED_DATE=DateTime.UtcNow;
            row.INVENTORY_LINK_REVIEW_JSON=PurchaseOrderInventoryLinkProposalProof.Create(submission.Command,submission.Review,row);Stored=row;return CompanyInventorySourceReview.Copy(row);
        }
        public PurchaseOrderSourceReview Source()=>Stored is null?CompanyInventorySourceReview.Copy(Review.Purchase):PurchaseOrderSourceReview.Create(Review.Purchase.Company,"actor",Review.Purchase.Header,[Stored],Review.Purchase.Receipts);
        public async Task Load(PurchaseOrderWorkspaceState state)
        {
            await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(Source()));
            await state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{CompanyInventorySourceReview.Copy(Review.Target.Item)}));
            await state.LoadLinkPreparationReviewAsync("po-line",Review.Target.Item.INVENTORY_ITEM_ID,(_,_,_)=>Task.FromResult(Review));
        }
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")][InlineData("source")][InlineData("actor")][InlineData("null")][InlineData("failed")]
    public async Task ReviewIsFrozenAndLateOrChangedEvidenceCannotPublish(string mode)
    {
        var f=await Fixture.Create();using var company=InventoryWorkspaceStateTests.Company();using var state=new PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Review.Purchase));
        await state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{f.Review.Target.Item}));
        var pending=new TaskCompletionSource<PurchaseOrderInventoryLinkProposalReview>();var load=state.LoadLinkPreparationReviewAsync("po-line",f.Review.Target.Item.INVENTORY_ITEM_ID,(_,_,_)=>pending.Task);
        Assert.True(state.LinkPreparationBusy);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReceiverReviewAsync("receipt",(_,_)=>throw new Exception()));
        if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="account")InventoryWorkspaceStateTests.Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
        if(mode=="source")f.Review.Target.Item.LOCATION_ID="changed";if(mode=="actor")f.Review=f.Review with{Purchase=f.Review.Purchase with{ActorUserId="other"}};
        if(mode=="failed")pending.SetException(new HttpRequestException());else pending.SetResult(mode=="null"?null!:f.Review);
        if(mode is "source" or "actor" or "null")await Assert.ThrowsAsync<InvalidOperationException>(()=>load);else if(mode=="failed")await Assert.ThrowsAsync<HttpRequestException>(()=>load);else await load;
        Assert.False(state.LinkPreparationBusy);
        if(mode=="valid"){Assert.NotNull(state.LinkPreparationReview);state.LinkPreparationReview!.Target.Item.LOCATION_ID="tampered";Assert.Equal("bin",state.LinkPreparationReview!.Target.Item.LOCATION_ID);state.ClearReceiverReview();Assert.Null(state.LinkPreparationReview);}
        else Assert.Null(state.LinkPreparationReview);
    }
    [Theory][InlineData("valid")][InlineData("unknown")][InlineData("company")][InlineData("account")][InlineData("clear")][InlineData("dispose")][InlineData("result")][InlineData("read-back")][InlineData("header")][InlineData("missing-read")][InlineData("reason")]
    public async Task SaveConfirmsExactReadBackOrRetainsOriginalIntentForRecovery(string mode)
    {
        var f=await Fixture.Create();using var company=InventoryWorkspaceStateTests.Company();using var state=new PurchaseOrderWorkspaceState(company);await f.Load(state);
        var calls=0;PurchaseOrderInventoryLinkProposalSubmission? original=null;
        Task<PO_LINE_ITEM> Record(PurchaseOrderInventoryLinkProposalSubmission submission)
        {
            ++calls;original??=CompanyInventorySourceReview.Copy(submission);Assert.Equal(original.Command,submission.Command);
            if(f.Stored is null)f.Record(submission);var result=CompanyInventorySourceReview.Copy(f.Stored!);
            if(mode=="unknown" && calls==1)throw new HttpRequestException("Unknown command result.");
            if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="account")InventoryWorkspaceStateTests.Select(company,actor:"other");if(mode=="clear")state.Clear();if(mode=="dispose")state.Dispose();
            if(mode=="result")result.INVENTORY_ITEM_ID="applied-without-review";
            return Task.FromResult(result);
        }
        Task<PurchaseOrderSourceReview?> Read(string id)
        {
            if(mode=="missing-read")return Task.FromResult<PurchaseOrderSourceReview?>(null);
            var source=f.Source();if(mode=="header"){source.Header.DESCRIPTION="changed";source=PurchaseOrderSourceReview.Create(source.Company,"actor",source.Header,source.Lines,source.Receipts);}
            if(mode=="read-back")source.Lines[0].INVENTORY_LINK_REVIEW_JSON="{}";
            return Task.FromResult<PurchaseOrderSourceReview?>(source);
        }
        var save=state.RecordLinkPreparationAsync(mode=="reason"?"":"Reviewed original warehouse and unit.",Record,Read);
        if(mode is "unknown" or "result" or "read-back" or "header" or "missing-read" or "reason")await Assert.ThrowsAnyAsync<Exception>(()=>save);else await save;
        Assert.False(state.LinkPreparationBusy);
        if(mode=="reason"){Assert.Equal(0,calls);Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationReview);return;}
        if(mode=="valid")
        {
            Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResult);Assert.Null(state.LinkPreparationResult!.INVENTORY_ITEM_ID);
            Assert.Equal(original!.Command.IntentId,Assert.Single(state.Lines!).Preparation!.Intent);Assert.True(Assert.Single(state.Lines!).HasPreparationEvidence);
        }
        else
        {
            Assert.True(state.HasUnconfirmedLinkPreparationIntent);Assert.Null(state.LinkPreparationResult);Assert.Null(state.LinkPreparationReview);
            Assert.Equal(original!.Command,state.LinkPreparationIntent!.Command);state.LinkPreparationIntent!.Review.Target.Item.LOCATION_ID="tampered";Assert.Equal("bin",state.LinkPreparationIntent!.Review.Target.Item.LOCATION_ID);
            if(mode=="unknown")
            {
                state.Clear();await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Source()));Assert.True(state.CanReconcileLinkPreparation);
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>())));
                await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReceiverReviewAsync("receipt",(_,_)=>throw new Exception()));
                await state.RecordLinkPreparationAsync("Replacement reason must be ignored.",Record,Read);Assert.Equal(2,calls);Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResult);
            }
            if(mode is "company" or "account")
            {
                Assert.Null(state.Detail);Assert.False(state.CanReconcileLinkPreparation);Assert.Null(state.LinkPreparationRecoveryRoute);
                InventoryWorkspaceStateTests.Select(company);await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Source()));Assert.NotNull(state.LinkPreparationRecoveryRoute);
                await state.RecordLinkPreparationAsync("Changed draft must be ignored.",submission=>{Assert.Equal(original.Command,submission.Command);return Task.FromResult(CompanyInventorySourceReview.Copy(f.Stored!));},_=>Task.FromResult<PurchaseOrderSourceReview?>(f.Source()));
                Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResult);
            }
        }
    }
}
#endif
