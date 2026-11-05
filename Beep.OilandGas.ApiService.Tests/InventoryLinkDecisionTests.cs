#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InventoryLinkDecisionTests
{
    internal sealed class Fixture
    {
        public PurchaseReceiptDecisionTests.Fixture Existing=new();
        public INVENTORY_ITEM Item=new(){INVENTORY_ITEM_ID="target",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl"};
        public PurchaseOrderInventoryLinkProposalCommand Preparation=null!;
        public PurchaseOrderInventoryLinkProposalReview PreparationReview=null!;
        public PurchaseOrderInventoryLinkDecisionReview Review=null!;
        public PurchaseOrderInventoryLinkDecisionCommand Command=null!;
        public string? Fault;
        public int AuthorityCalls;
        public PO_LINE_ITEM Stored=>Assert.Single(Existing.Existing.Native.Rows.OfType<PO_LINE_ITEM>());
        public CompanyInventorySourceReview Target(string actor)=>CompanyInventorySourceReview.Create(new("tenant","company"),actor,Item,[]);
        public static async Task<Fixture> Create(PurchaseOrderInventoryLinkDecision decision=PurchaseOrderInventoryLinkDecision.Approve,bool receiving=false,bool large=false,string orderId="order",string lineId="po-line",string itemId="target",bool withMaker=false)
        {
            var f=new Fixture();var owner=f.Existing.Existing;var n=owner.Native;owner.OrderLine.INVENTORY_ITEM_ID="current-item";
            if(withMaker){owner.Order.ROW_CREATED_BY=owner.OrderLine.ROW_CREATED_BY="po-preparer";owner.Order.ROW_CREATED_DATE=owner.OrderLine.ROW_CREATED_DATE=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);}
            owner.Order.PURCHASE_ORDER_ID=orderId;owner.OrderLine.PURCHASE_ORDER_ID=orderId;owner.OrderLine.PO_LINE_ITEM_ID=lineId;owner.Receipt.PURCHASE_ORDER_ID=orderId;owner.Receipt.PO_LINE_ITEM_ID=lineId;f.Item.INVENTORY_ITEM_ID=itemId;
            if(large)f.Item.DESCRIPTION=new string('X',140000);
            var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(owner.PO)!;
            Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_LINE_ITEM")).ReturnsAsync(new List<string>{"PO_LINE_ITEM_ID"});
            n.Source.Setup(value=>value.UpdateEntity("PO_LINE_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
                ++n.Writes;if(f.Fault=="write")return new ErrorsInfo{Flag=Errors.Failed};
                var row=CompanyInventorySourceReview.Copy((PO_LINE_ITEM)value);if(f.Fault=="corrupt")row.INVENTORY_ITEM_ID="tampered";
                n.Rows.RemoveAll(value=>value is PO_LINE_ITEM line && line.PO_LINE_ITEM_ID==row.PO_LINE_ITEM_ID);n.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};});
            if(receiving){await f.Existing.Prepare(orderId:orderId);await f.Existing.Record();}
            var purchase=(await owner.PO.GetSourceReviewAsync(new("tenant","company"),orderId,"actor"))!;
            f.PreparationReview=PurchaseOrderInventoryLinkProposalReview.Create(purchase,f.Target("actor"),lineId,"binding");
            f.Preparation=new(purchase.Company,"actor",orderId,lineId,itemId,f.PreparationReview.Revision,"binding",Guid.NewGuid().ToString("N"),"Reviewed warehouse target.");
            await owner.PO.PrepareInventoryLinkAsync(f.Preparation,f.PreparationReview,()=>Task.FromResult(f.Target("actor")),()=>Task.CompletedTask);
            var prepared=(await owner.PO.GetSourceReviewAsync(purchase.Company,orderId,"reviewer"))!;
            f.Review=PurchaseOrderInventoryLinkDecisionReview.Create(prepared,f.Target("reviewer"),lineId,"binding");
            f.Command=new(purchase.Company,"reviewer",orderId,lineId,itemId,decision,f.Review.Revision,"binding",Guid.NewGuid().ToString("N"),"Independently reviewed warehouse and original line.");return f;
        }
        public Task<PO_LINE_ITEM> Record()=>Existing.Existing.PO.RecordInventoryLinkDecisionAsync(Command,Review,()=>Task.FromResult(Target("reviewer")),()=>{
            ++AuthorityCalls;if(Fault=="grant" || Fault=="authority-during" && AuthorityCalls>1)throw new UnauthorizedAccessException();return Task.CompletedTask;});
    }
    [Fact] public async Task DecisionRetainsLargeValidPreparationAndTargetEvidenceWithoutTruncation()
    {
        var f=await Fixture.Create(large:true);var row=await f.Record();
        Assert.True(row.INVENTORY_LINK_DECISION_JSON!.Length>PurchaseOrderInventoryLinkProposalProof.MaximumLength);
        Assert.True(row.INVENTORY_LINK_DECISION_JSON.Length<PurchaseOrderInventoryLinkDecisionProof.MaximumLength);
        var proof=PurchaseOrderInventoryLinkDecisionProof.Require(f.Command.Company,row);Assert.Equal(140000,proof.Review.Target.Item.DESCRIPTION.Length);
        Assert.Equal(f.Preparation,PurchaseOrderInventoryLinkProposalProof.Require(f.Command.Company,row).Command);Assert.Equal(2,f.Existing.Existing.Native.Writes);
    }
    [Fact] public async Task ApprovedReferenceSupportsExistingReceiverInventoryReviewWithoutStockOrReceiptMutation()
    {
        var f=await Fixture.Create(receiving:true);var originalReceipt=CompanyInventorySourceReview.Copy(f.Existing.Stored);await f.Record();
        var purchase=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","reviewer"))!;
        var linked=PurchaseReceiptInventoryReview.Create(purchase,f.Target("reviewer"),"receipt","binding");
        linked.Require(f.Command.Company,"reviewer","order","receipt");Assert.Equal("target",linked.Inventory.Item.INVENTORY_ITEM_ID);Assert.Null(linked.Inventory.Item.QUANTITY_ON_HAND);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(originalReceipt),System.Text.Json.JsonSerializer.Serialize(f.Existing.Stored));Assert.Equal(3,f.Existing.Existing.Native.Writes);
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("retry")][InlineData("grant")][InlineData("authority-during")][InlineData("target")][InlineData("header")][InlineData("schema")][InlineData("write")][InlineData("corrupt")][InlineData("unknown")][InlineData("post-commit")][InlineData("intent")][InlineData("reason")]
    public async Task IndependentDecisionAtomicallyPreservesProposalAndOnlyAppliesAnApprovedReference(string mode)
    {
        var f=await Fixture.Create(mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve);var n=f.Existing.Existing.Native;
        var preparationJson=f.Stored.INVENTORY_LINK_REVIEW_JSON;f.Fault=mode;
        if(mode=="target")f.Item.LOCATION_ID="changed";if(mode=="header")f.Existing.Existing.Order.DESCRIPTION="changed";
        if(mode=="schema")
        {
            var schema=new TheTechIdea.Beep.Tools.ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(PO_LINE_ITEM));schema.Fields.RemoveAll(field=>field.FieldName=="INVENTORY_LINK_DECISION_JSON");
            n.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_LINE_ITEM"),true)).Returns(schema);
        }
        if(mode=="unknown")n.FailCommit=true;if(mode=="post-commit")n.AfterCommit=()=>f.Item.LOCATION_ID="changed";
        if(mode is "grant" or "authority-during" or "target" or "header" or "schema" or "write" or "corrupt")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal("current-item",f.Stored.INVENTORY_ITEM_ID);Assert.Null(f.Stored.INVENTORY_LINK_DECISION_JSON);Assert.Equal(preparationJson,f.Stored.INVENTORY_LINK_REVIEW_JSON);
            Assert.Equal(mode is "write" or "corrupt"?2:1,n.Writes);if(mode!="grant")Assert.Contains("rollback",n.Transactions);
        }
        else
        {
            if(mode is "unknown" or "post-commit"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());n.FailCommit=false;n.AfterCommit=null;f.Item.LOCATION_ID="bin";}
            var result=await f.Record();var proof=PurchaseOrderInventoryLinkDecisionProof.Require(f.Command.Company,result);Assert.Equal(f.Command,proof.Command);
            Assert.Equal(mode=="reject"?"current-item":"target",result.INVENTORY_ITEM_ID);Assert.Equal(preparationJson,result.INVENTORY_LINK_REVIEW_JSON);Assert.Equal("reviewer",result.ROW_CHANGED_BY);
            Assert.Equal(f.Preparation,PurchaseOrderInventoryLinkProposalProof.Require(f.Command.Company,result).Command);await f.Record();Assert.Equal(2,n.Writes);
            if(mode is "intent" or "reason"){f.Command=mode=="intent"?f.Command with{IntentId=Guid.NewGuid().ToString("N")}:f.Command with{Reason="Changed decision reason"};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Writes);}
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Existing.Existing.PO.PrepareInventoryLinkAsync(f.Preparation,f.PreparationReview,()=>Task.FromResult(f.Target("actor")),()=>Task.CompletedTask));Assert.Equal(2,n.Writes);
        }
        Assert.Null(Assert.Single(n.Rows.OfType<PO_RECEIPT>()).RECEIVER_DECISION_JSON);
    }
    [Theory][InlineData("same-actor")][InlineData("binding")][InlineData("target")][InlineData("unknown-decision")][InlineData("preparation-proof")][InlineData("preparation-audit")]
    public async Task ChangedOrNonIndependentReviewCannotAuthorizeADecision(string mode)
    {
        var f=await Fixture.Create();
        if(mode=="unknown-decision"){f.Command=f.Command with{Decision=0};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());}
        else
        {
            var actor=mode=="same-actor"?"actor":"reviewer";var line=f.Stored;
            if(mode=="preparation-proof")line.INVENTORY_LINK_REVIEW_JSON="{}";if(mode=="preparation-audit")line.ROW_CHANGED_BY="forged";if(mode=="target")f.Item.UNIT_OF_MEASURE="m³";
            var source=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order",actor))!;
            Assert.Throws<InvalidOperationException>(()=>PurchaseOrderInventoryLinkDecisionReview.Create(source,f.Target(actor),"po-line",mode=="binding"?"changed":"binding"));
        }
        Assert.Equal(1,f.Existing.Existing.Native.Writes);Assert.Null(f.Stored.INVENTORY_LINK_DECISION_JSON);
    }
    [Theory][InlineData("reference")][InlineData("proposal")][InlineData("audit")][InlineData("time")][InlineData("proof")]
    public async Task RecordedDecisionCannotConfirmTamperedApplicationOrOriginalProposal(string mode)
    {
        var f=await Fixture.Create();await f.Record();var row=CompanyInventorySourceReview.Copy(f.Stored);
        if(mode=="reference")row.INVENTORY_ITEM_ID="other";if(mode=="proposal")row.INVENTORY_LINK_REVIEW_JSON="{}";if(mode=="audit")row.ROW_CHANGED_BY="actor";
        if(mode=="time")row.ROW_CHANGED_DATE=DateTime.SpecifyKind(DateTime.MinValue,DateTimeKind.Utc);if(mode=="proof")row.INVENTORY_LINK_DECISION_JSON="{}";
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderInventoryLinkDecisionProof.Require(f.Command.Company,row));
        Assert.Throws<InvalidOperationException>(()=>PurchaseOrderInventoryLinkProposalProof.Require(f.Command.Company,row));
    }
}
#endif
