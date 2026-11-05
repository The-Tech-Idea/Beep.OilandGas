using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkCycleTests
{
    internal sealed class Fixture
    {
        public InventoryLinkDecisionTests.Fixture Existing=null!;
        public PurchaseOrderInventoryLinkCycleReview Review=null!;
        public PurchaseOrderInventoryLinkCycleCommand Command=null!;
        public string? Fault;
        public int AuthorityCalls;
        public PersonaTaskRoutingTests.Fixture Native=>Existing.Existing.Existing.Native;
        public PurchaseOrderService Service=>Existing.Existing.Existing.PO;
        public PO_LINE_ITEM Stored=>Existing.Stored;
        public static async Task<Fixture> Create(bool pending=false,bool rejected=false,string orderId="order",string lineId="po-line",bool withMaker=false)
        {
            var f=new Fixture{Existing=await InventoryLinkDecisionTests.Fixture.Create(rejected?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve,orderId:orderId,lineId:lineId,withMaker:withMaker)};
            if(!pending)await f.Existing.Record();var n=f.Native;
            var schema=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(PO_INVENTORY_LINK_HISTORY));
            n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_INVENTORY_LINK_HISTORY"),true)).Returns(schema);
            var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f.Service)!;
            Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_INVENTORY_LINK_HISTORY")).ReturnsAsync(new List<string>{"PO_INVENTORY_LINK_HISTORY_ID"});
            n.Source.Setup(value=>value.InsertEntity("PO_INVENTORY_LINK_HISTORY",It.IsAny<object>())).Returns((string table,object value)=>{
                ++n.Writes;if(f.Fault=="archive-write")return new ErrorsInfo{Flag=Errors.Failed};var entry=CompanyInventorySourceReview.Copy((PO_INVENTORY_LINK_HISTORY)value);
                if(f.Fault=="archive-corrupt")entry.REASON="corrupt";n.Rows.Add(entry);return new ErrorsInfo{Flag=Errors.Ok};});
            await f.Prepare(pending?PurchaseOrderInventoryLinkCycleAction.SupersedePending:PurchaseOrderInventoryLinkCycleAction.ReopenResolved);return f;
        }
        public async Task Prepare(PurchaseOrderInventoryLinkCycleAction action)
        {
            var company=new CompanyScope("tenant","company");var order=Existing.Preparation.OrderId;var line=Existing.Preparation.LineId;var source=(await Service.GetSourceReviewAsync(company,order,"actor"))!;
            var history=await Service.GetInventoryLinkHistoryAsync(company,order,line,"actor",()=>Task.CompletedTask);
            Review=PurchaseOrderInventoryLinkCycleReview.Create(source,history,line,action,"binding");
            Command=new(company,"actor",order,line,action,Review.Revision,"binding",Guid.NewGuid().ToString("N"),"Archive original audit before a new reviewed cycle.");
        }
        public Task<PurchaseOrderInventoryLinkCycleResult> Record()=>Service.ReopenInventoryLinkAsync(Command,Review,()=>{
            ++AuthorityCalls;if(Fault=="grant" || Fault=="authority-during" && AuthorityCalls>1 || Fault=="post-commit-grant" && Native.Transactions.LastOrDefault()=="commit")throw new UnauthorizedAccessException();return Task.CompletedTask;});
    }
    [Theory][InlineData("resolved")][InlineData("rejected")][InlineData("pending")][InlineData("retry")][InlineData("unknown")][InlineData("post-commit")][InlineData("post-commit-grant")]
    public async Task AtomicCycleRetainsCompleteOriginalAuditAndReferenceAndRecoversOnlyExactIntent(string mode)
    {
        var f=await Fixture.Create(pending:mode=="pending",rejected:mode=="rejected");var original=JsonSerializer.Serialize(f.Stored);var reference=f.Stored.INVENTORY_ITEM_ID;var baseline=f.Native.Writes;
        if(mode=="unknown")f.Native.FailCommit=true;
        if(mode=="post-commit")f.Native.AfterCommit=()=>Assert.Single(f.Native.Rows.OfType<PURCHASE_ORDER>()).DESCRIPTION="changed";
        if(mode=="post-commit-grant")f.Fault=mode;
        if(mode is "unknown" or "post-commit" or "post-commit-grant")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());f.Native.FailCommit=false;f.Native.AfterCommit=null;f.Fault=null;
            if(mode=="post-commit")Assert.Single(f.Native.Rows.OfType<PURCHASE_ORDER>()).DESCRIPTION=f.Review.Purchase.Header.DESCRIPTION;
        }
        var result=await f.Record();new PurchaseOrderInventoryLinkCycleSubmission(f.Command,f.Review).RequireResult(result);
        Assert.Equal(original,Assert.Single(result.History.Entries).ARCHIVED_LINE_JSON);Assert.Equal(reference,f.Stored.INVENTORY_ITEM_ID);
        Assert.Null(f.Stored.INVENTORY_LINK_REVIEW_JSON);Assert.Null(f.Stored.INVENTORY_LINK_DECISION_JSON);Assert.Equal("actor",f.Stored.ROW_CHANGED_BY);
        Assert.Equal(Assert.Single(result.History.Entries).RECORDED_REVISION,f.Stored.INVENTORY_LINK_HISTORY_REVISION);Assert.Equal(baseline+2,f.Native.Writes);
        await f.Record();Assert.Equal(baseline+2,f.Native.Writes);Assert.Null(Assert.Single(f.Native.Rows.OfType<PO_RECEIPT>()).RECEIVER_DECISION_JSON);
        var changed=f.Command with{Reason="Replacement reason"};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ReopenInventoryLinkAsync(changed,f.Review,()=>Task.CompletedTask));Assert.Equal(baseline+2,f.Native.Writes);
    }
    [Theory][InlineData("grant")][InlineData("authority-during")][InlineData("header")][InlineData("line")][InlineData("schema")][InlineData("archive-write")][InlineData("archive-corrupt")][InlineData("line-write")][InlineData("line-corrupt")][InlineData("duplicate-line")][InlineData("foreign-intent")][InlineData("isolation")]
    public async Task UnconfirmedOrUnauthorizedCycleCannotLeavePartialArchiveOrClearOriginalEvidence(string mode)
    {
        var f=await Fixture.Create();var baseline=f.Native.Writes;f.Fault=mode;
        if(mode=="header")f.Existing.Existing.Existing.Order.DESCRIPTION="changed";if(mode=="line")f.Stored.QUANTITY=99;
        if(mode=="schema"){var schema=new ClassCreator(f.Native.Editor.Object).ConvertToEntityStructure(typeof(PO_LINE_ITEM));schema.Fields.RemoveAll(value=>value.FieldName=="INVENTORY_LINK_DECISION_JSON");f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_LINE_ITEM"),true)).Returns(schema);}
        if(mode=="line-write")f.Existing.Fault="write";if(mode=="line-corrupt")f.Existing.Fault="corrupt";
        if(mode=="duplicate-line")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Stored));
        if(mode=="foreign-intent")f.Native.Rows.Add(new PO_INVENTORY_LINK_HISTORY{PO_INVENTORY_LINK_HISTORY_ID=f.Command.IntentId,CYCLE_INTENT_ID=f.Command.IntentId,OWNER_TENANT_ID="other"});
        if(mode=="isolation")f.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;
        var before=JsonSerializer.Serialize(f.Native.Rows.OfType<PO_LINE_ITEM>().First());
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());
        Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.OfType<PO_LINE_ITEM>().First()));
        Assert.Equal(mode=="foreign-intent"?1:0,f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>().Count());
        Assert.Equal(baseline+(mode is "archive-write" or "archive-corrupt"?1:mode is "line-write" or "line-corrupt"?2:0),f.Native.Writes);
    }
    [Fact]
    public async Task SubsequentNativePreparationDecisionAndReopenPreserveEarlierArchiveWithoutNestedHistoryCopies()
    {
        var f=await Fixture.Create();await f.Record();var firstCommand=f.Command;var firstReview=f.Review;var first=JsonSerializer.Serialize(Assert.Single(f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>()));
        f.Existing.Item.INVENTORY_ITEM_ID="next-target";
        var purchase=(await f.Service.GetSourceReviewAsync(f.Command.Company,"order","new-preparer"))!;
        var proposal=PurchaseOrderInventoryLinkProposalReview.Create(purchase,f.Existing.Target("new-preparer"),"po-line","binding");
        var prepare=new PurchaseOrderInventoryLinkProposalCommand(f.Command.Company,"new-preparer","order","po-line","next-target",proposal.Revision,"binding",Guid.NewGuid().ToString("N"),"Next independently reviewed target.");
        await f.Service.PrepareInventoryLinkAsync(prepare,proposal,()=>Task.FromResult(f.Existing.Target("new-preparer")),()=>Task.CompletedTask);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ReopenInventoryLinkAsync(firstCommand,firstReview,()=>Task.CompletedTask));Assert.Equal(5,f.Native.Writes);
        var prepared=(await f.Service.GetSourceReviewAsync(f.Command.Company,"order","new-reviewer"))!;
        var decision=PurchaseOrderInventoryLinkDecisionReview.Create(prepared,f.Existing.Target("new-reviewer"),"po-line","binding");
        await f.Service.RecordInventoryLinkDecisionAsync(new(f.Command.Company,"new-reviewer","order","po-line","next-target",PurchaseOrderInventoryLinkDecision.Approve,decision.Revision,"binding",Guid.NewGuid().ToString("N"),"Independent next target approval."),decision,()=>Task.FromResult(f.Existing.Target("new-reviewer")),()=>Task.CompletedTask);
        await f.Prepare(PurchaseOrderInventoryLinkCycleAction.ReopenResolved);var result=await f.Record();Assert.Equal(2,result.History.Entries.Count);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.ReopenInventoryLinkAsync(firstCommand,firstReview,()=>Task.CompletedTask));
        Assert.Equal(first,JsonSerializer.Serialize(result.History.Entries[0]));Assert.Equal("next-target",f.Stored.INVENTORY_ITEM_ID);
        Assert.Equal(result.History.Entries[0].RECORDED_REVISION,result.History.Entries[1].PREVIOUS_REVISION);Assert.DoesNotContain("ARCHIVED_LINE_JSON",result.History.Entries[1].ARCHIVED_LINE_JSON);
        Assert.Equal(8,f.Native.Writes);
    }
    [Theory][InlineData("binding")][InlineData("actor")][InlineData("intent")][InlineData("reason")][InlineData("action")][InlineData("revision")]
    public async Task ChangedSubmissionCannotReplaceOriginalExplicitCycleIntent(string mode)
    {
        var f=await Fixture.Create();var baseline=f.Native.Writes;
        f.Command=mode switch{"binding"=>f.Command with{BindingRevision="changed"},"actor"=>f.Command with{ActorUserId="other"},"intent"=>f.Command with{IntentId=Guid.Empty.ToString("N")},"reason"=>f.Command with{Reason="two\nlines"},"action"=>f.Command with{Action=0},_=>f.Command with{ReviewRevision=new string('A',64)}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(baseline,f.Native.Writes);Assert.Empty(f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>());
    }
    [Theory][InlineData("reference")][InlineData("cursor")][InlineData("archive")][InlineData("header")][InlineData("actor")]
    public async Task CompleteResultProofRefusesAlteredHistoryLineOrderOrAccount(string mode)
    {
        var f=await Fixture.Create();var result=CompanyInventorySourceReview.Copy(await f.Record());
        if(mode=="reference")result.Purchase.Lines[0].INVENTORY_ITEM_ID="changed";if(mode=="cursor")result.Purchase.Lines[0].INVENTORY_LINK_HISTORY_REVISION=null;
        if(mode=="archive")result.History.Entries[0].REASON="changed";if(mode=="header")result.Purchase.Header.DESCRIPTION="changed";if(mode=="actor")result=result with{Purchase=result.Purchase with{ActorUserId="other"}};
        Assert.Throws<InvalidOperationException>(()=>new PurchaseOrderInventoryLinkCycleSubmission(f.Command,f.Review).RequireResult(result));Assert.Equal(4,f.Native.Writes);
    }
    [Fact]
    public async Task SupersessionReviewsCurrentOrderAndRetainsOldProposalEvenWhenTargetOrHeaderChangedBeforeReview()
    {
        var f=await Fixture.Create(pending:true);var original=JsonSerializer.Serialize(f.Stored);
        f.Existing.Item.LOCATION_ID="changed-bin";f.Existing.Existing.Existing.Order.DESCRIPTION="Updated order before explicit supersession review.";
        await f.Prepare(PurchaseOrderInventoryLinkCycleAction.SupersedePending);var result=await f.Record();
        Assert.Equal(original,Assert.Single(result.History.Entries).ARCHIVED_LINE_JSON);Assert.Equal("current-item",f.Stored.INVENTORY_ITEM_ID);Assert.Equal(3,f.Native.Writes);
    }
}
#endif
