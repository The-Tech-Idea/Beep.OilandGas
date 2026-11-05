#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptStockCommandTests
{
    internal sealed class Fixture
    {
        public readonly PurchaseReceiptDecisionTests.Fixture Receiving=new();
        public PersonaTaskRoutingTests.Fixture Native=>Receiving.Existing.Native;
        public InventoryService Inventory;
        public INVENTORY_ITEM Item=>Assert.Single(Native.Rows.OfType<INVENTORY_ITEM>());
        public PurchaseReceiptStockSubmission Submission=null!;
        public string OrderId="order",ReceiptId="receipt";
        public bool CorruptMovement,CorruptItem,FailItem,Revoked,CorruptRecovery,FailRecovery;
        public Action? OnRecovery;
        public Action? OnInsert,OnUpdate;
        public Fixture()
        {
            Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
            Native.Source.Setup(value=>value.GetEntity("INVENTORY_STOCK_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<TheTechIdea.Beep.Report.AppFilter> filters,int page,int size)=>{
                var rows=Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID,StringComparer.Ordinal).ToArray();
                return new PagedResult(rows.Skip((page-1)*size).Take(size).Select(CompanyInventorySourceReview.Copy).ToArray(),page,size,rows.Length);
            });
            Native.Rows.Add(new INVENTORY_ITEM{INVENTORY_ITEM_ID="item",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="bbl",QUANTITY_ON_HAND=10m,UNIT_COST=22m,TOTAL_VALUE=99m});
            Receiving.Existing.OrderLine.INVENTORY_ITEM_ID="item";
            var creator=new ClassCreator(Native.Editor.Object);
            foreach(var pair in new[]{("INVENTORY_ITEM",typeof(INVENTORY_ITEM)),("INVENTORY_TRANSACTION",typeof(INVENTORY_TRANSACTION)),("INVENTORY_STOCK_INTENT",typeof(INVENTORY_STOCK_INTENT))})
                Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName==pair.Item1),true)).Returns(()=>creator.ConvertToEntityStructure(pair.Item2));
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("INVENTORY_ITEM")).ReturnsAsync(new List<string>{"INVENTORY_ITEM_ID"});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("INVENTORY_TRANSACTION")).ReturnsAsync(new List<string>{"INVENTORY_TRANSACTION_ID"});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("INVENTORY_STOCK_INTENT")).ReturnsAsync(new List<string>{"INVENTORY_STOCK_INTENT_ID"});
            Inventory=CompanyInventorySourceTests.CreateService(Native.Editor,()=>Task.FromResult(Native.Connection),new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler(),metadata.Object);
            Native.Source.Setup(value=>value.InsertEntity("INVENTORY_TRANSACTION",It.IsAny<object>())).Returns((string table,object value)=>{
                Native.Writes++;var row=CompanyInventorySourceReview.Copy((INVENTORY_TRANSACTION)value);if(CorruptMovement)row.QUANTITY=999;Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Source.Setup(value=>value.InsertEntity("INVENTORY_STOCK_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{
                Native.Writes++;if(FailRecovery)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((INVENTORY_STOCK_INTENT)value);
                if(CorruptRecovery)row.COMMAND_JSON="{}";Native.Rows.Add(row);OnRecovery?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Source.Setup(value=>value.UpdateEntity("INVENTORY_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
                Native.Writes++;if(FailItem)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((INVENTORY_ITEM)value);if(CorruptItem)row.QUANTITY_ON_HAND=999;
                Native.Rows.RemoveAll(item=>item is INVENTORY_ITEM);Native.Rows.Add(row);OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Initialize(string receiverActor="actor")
        {
            Receiving.Existing.Order.PURCHASE_ORDER_ID=OrderId;Receiving.Existing.OrderLine.PURCHASE_ORDER_ID=OrderId;
            Receiving.Existing.Receipt.PURCHASE_ORDER_ID=OrderId;Receiving.Existing.Receipt.PO_RECEIPT_ID=ReceiptId;
            await Receiving.Prepare(orderId:OrderId,receiptId:ReceiptId,actor:receiverActor);await Receiving.Record();Native.Writes=0;Native.Transactions.Clear();await Prepare();
        }
        public async Task Prepare(PurchaseReceiptStockMovementKind kind=PurchaseReceiptStockMovementKind.Receive,decimal quantity=4m,string? original=null,string actor="actor")
        {
            var purchase=(await Receiving.Existing.PO.GetSourceReviewAsync(new("tenant","company"),OrderId,actor))!;
            var inventory=await Inventory.GetReceiptStockInventoryAsync(purchase,ReceiptId);
            var review=PurchaseReceiptStockReview.Create(PurchaseReceiptInventoryReview.Create(purchase,inventory,ReceiptId,"binding"));
            Submission=new(new(new("tenant","company"),actor,OrderId,ReceiptId,kind,quantity,"bbl",original,review.Revision,"binding",Guid.NewGuid().ToString("N"),"Reviewed physical stock movement."),review);
        }
        public Task Authority(){if(Revoked)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        public async Task ApproveAlternateDestination()
        {
            var po=Receiving.Existing.PO;var scope=new CompanyScope("tenant","company");
            var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(po)!;
            Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_LINE_ITEM")).ReturnsAsync(new List<string>{"PO_LINE_ITEM_ID"});
            Native.Source.Setup(value=>value.UpdateEntity("PO_LINE_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
                ++Native.Writes;Native.Rows.RemoveAll(row=>row is PO_LINE_ITEM);Native.Rows.Add(CompanyInventorySourceReview.Copy((PO_LINE_ITEM)value));return new ErrorsInfo{Flag=Errors.Ok};});
            var target=new INVENTORY_ITEM{INVENTORY_ITEM_ID="alternate",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",UNIT_OF_MEASURE="bbl",WAREHOUSE_ID="other-warehouse",LOCATION_ID="other-bin"};
            CompanyInventorySourceReview Target(string actor)=>CompanyInventorySourceReview.Create(scope,actor,target,[]);
            var purchase=(await po.GetSourceReviewAsync(scope,OrderId,"link-preparer"))!;var line=purchase.Receipts.Single(row=>row.PO_RECEIPT_ID==ReceiptId).PO_LINE_ITEM_ID;
            var preparation=PurchaseOrderInventoryLinkProposalReview.Create(purchase,Target("link-preparer"),line,"binding");
            await po.PrepareInventoryLinkAsync(new(scope,"link-preparer",OrderId,line,"alternate",preparation.Revision,"binding",Guid.NewGuid().ToString("N"),"Review alternate destination."),preparation,()=>Task.FromResult(Target("link-preparer")),()=>Task.CompletedTask);
            purchase=(await po.GetSourceReviewAsync(scope,OrderId,"link-reviewer"))!;var review=PurchaseOrderInventoryLinkDecisionReview.Create(purchase,Target("link-reviewer"),line,"binding");
            await po.RecordInventoryLinkDecisionAsync(new(scope,"link-reviewer",OrderId,line,"alternate",PurchaseOrderInventoryLinkDecision.Approve,review.Revision,"binding",Guid.NewGuid().ToString("N"),"Independent destination approval."),review,()=>Task.FromResult(Target("link-reviewer")),()=>Task.CompletedTask);
        }
        public Task<PurchaseReceiptInventoryReview> Record()=>Inventory.RecordPurchaseReceiptStockAsync(Submission,Receiving.Existing.PO,Authority);
    }
    [Theory][InlineData("missing-movement")][InlineData("missing-entry")][InlineData("duplicate")][InlineData("foreign")][InlineData("reference")][InlineData("entry-digest")]
    public async Task RecordedReceiptMembershipCannotBeHiddenOrResetBeforeFreshStockReview(string mode)
    {
        var f=new Fixture();await f.Initialize();await f.Record();
        if(mode=="missing-movement")f.Native.Rows.RemoveAll(row=>row is INVENTORY_TRANSACTION);
        if(mode=="missing-entry")f.Native.Rows.RemoveAll(row=>row is INVENTORY_STOCK_INTENT);
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single()));
        if(mode=="foreign")f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single().OWNER_COMPANY_BA_ID="other";
        if(mode=="reference")f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single().REFERENCE_NUMBER="hidden-receipt";
        if(mode=="entry-digest")f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single().ORIGINAL_PURCHASE_JSON="{}";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Prepare());Assert.Equal(3,f.Native.Writes);Assert.Equal(14m,f.Item.QUANTITY_ON_HAND);
    }
    [Theory][InlineData(4)][InlineData(8)]
    public async Task AtomicReceiptPreservesFinancialAndReceivingDataAndRetriesWithoutWrites(int quantity)
    {
        var f=new Fixture();await f.Initialize();await f.Prepare(quantity:quantity);
        var receiptJson=JsonSerializer.Serialize(f.Receiving.Stored);var orderRevision=f.Submission.Review.Source.Purchase.Revision;
        var result=await f.Record();f.Submission.RequireResult(result);var retry=await f.Record();f.Submission.RequireResult(retry);
        Assert.Equal(10m+quantity,f.Item.QUANTITY_ON_HAND);Assert.Equal(22m,f.Item.UNIT_COST);Assert.Equal(99m,f.Item.TOTAL_VALUE);Assert.Equal(orderRevision,result.Purchase.Revision);
        Assert.Equal(receiptJson,JsonSerializer.Serialize(f.Receiving.Stored));Assert.Equal("DRAFT",f.Receiving.Existing.Invoice.STATUS);
        Assert.Equal(3,f.Native.Writes);Assert.Single(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());Assert.Single(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>());Assert.Equal(new[]{"begin","commit","begin","commit"},f.Native.Transactions);
        result.Inventory.Item.QUANTITY_ON_HAND=123;Assert.Equal(10m+quantity,f.Item.QUANTITY_ON_HAND);
    }
    [Theory][InlineData(1)][InlineData(4)]
    public async Task OriginalReceiptBoundReturnsPreserveGrossCapacityAndReconcileExactly(int quantity)
    {
        var f=new Fixture();await f.Initialize();await f.Record();var original=f.Submission.Command.IntentId;
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,quantity,original);var result=await f.Record();await f.Record();
        f.Submission.RequireResult(result);Assert.Equal(14m-quantity,f.Item.QUANTITY_ON_HAND);Assert.Equal(6,f.Native.Writes);
        var stock=PurchaseReceiptStockReview.Create(result);Assert.Equal(4m,stock.ReceivedQuantity);Assert.Equal(quantity,stock.ReturnedQuantity);Assert.Equal(4m,stock.RemainingReceiptQuantity);
        Assert.Equal(2,f.Native.Rows.OfType<INVENTORY_TRANSACTION>().Count());
    }
    [Theory][InlineData("quantity")][InlineData("negative")][InlineData("capacity")][InlineData("unit")][InlineData("reason")][InlineData("intent")][InlineData("kind")][InlineData("revision")][InlineData("binding")][InlineData("actor")]
    public async Task InvalidOriginalCommandNeverOpensTransaction(string mode)
    {
        var f=new Fixture();await f.Initialize();var c=f.Submission.Command;
        c=mode switch{"quantity"=>c with{Quantity=0},"negative"=>c with{Quantity=-1},"capacity"=>c with{Quantity=9},"unit"=>c with{QuantityUnit="m³"},"reason"=>c with{Reason="bad\nreason"},"intent"=>c with{IntentId=Guid.Empty.ToString("N")},"kind"=>c with{Kind=(PurchaseReceiptStockMovementKind)0},"revision"=>c with{ExpectedReviewRevision=new string('b',64)},"binding"=>c with{BindingRevision="other"},_=>c with{ActorUserId="SYSTEM"}};
        f.Submission=f.Submission with{Command=c};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Empty(f.Native.Transactions);Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("item")][InlineData("history")][InlineData("purchase")][InlineData("duplicate-item")][InlineData("intent-collision")]
    public async Task StaleOrAmbiguousNativeSourceNeverWrites(string mode)
    {
        var f=new Fixture();await f.Initialize();if(mode=="item")f.Item.LOCATION_ID="other";if(mode=="purchase")f.Receiving.Existing.Order.DESCRIPTION="changed";
        if(mode=="history")f.Native.Rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="usage",INVENTORY_ITEM_ID="item",TRANSACTION_TYPE="USAGE",QUANTITY=-1m,ACTIVE_IND="Y"});
        if(mode=="duplicate-item")f.Native.Rows.Add(f.Item);if(mode=="intent-collision")f.Native.Rows.Add(new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID=f.Submission.Command.IntentId,INVENTORY_ITEM_ID="other"});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("movement")][InlineData("item")][InlineData("write")][InlineData("purchase")][InlineData("binding")][InlineData("authority")]
    public async Task FailedOrChangedAtomicWritesRollBackBothOwners(string mode)
    {
        var f=new Fixture();await f.Initialize();f.CorruptMovement=mode=="movement";f.CorruptItem=mode=="item";f.FailItem=mode=="write";
        if(mode=="purchase")f.OnUpdate=()=>f.Receiving.Existing.Order.DESCRIPTION="changed";if(mode=="binding")f.OnInsert=()=>f.Native.Connection="changed";if(mode=="authority")f.OnInsert=()=>f.Revoked=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());Assert.Equal(new[]{"begin","rollback"},f.Native.Transactions);
        Assert.Empty(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>());
        if(mode is "movement" or "binding" or "authority")Assert.Equal(1,f.Native.Writes);
    }
    [Fact] public async Task UnknownCommitUsesOriginalIntentWithoutAnotherStockMovement()
    {
        var f=new Fixture();await f.Initialize();f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(14m,f.Item.QUANTITY_ON_HAND);
        var fresh=CompanyInventorySourceTests.CreateService(f.Native.Editor,()=>Task.FromResult(f.Native.Connection));
        var recovery=await fresh.GetStockRecoveryAsync(f.Submission.Command.Company,f.Submission.Command.IntentId,"actor",f.Receiving.Existing.PO,f.Authority);
        Assert.Equal(f.Submission.Command,recovery.OriginalSubmission().Command);Assert.Equal(f.Submission.Review.Revision,recovery.OriginalSubmission().Review.Revision);Assert.True(recovery.CurrentMatchesOriginalEffect());Assert.Equal(3,f.Native.Writes);
        f.Native.FailCommit=false;var result=await f.Record();f.Submission.RequireResult(result);Assert.Equal(3,f.Native.Writes);Assert.Single(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
    }
    [Theory][InlineData(true)][InlineData(false)] public async Task BorrowedOrInsufficientTransactionCannotMoveStock(bool borrowed)
    {
        var f=new Fixture();await f.Initialize();if(borrowed)f.Native.Borrowed=true;else f.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Native.Writes);
    }
    [Fact] public async Task LaterReceiptOrReturnCannotBeReplacedByRetryOfOlderIntent()
    {
        var f=new Fixture();await f.Initialize();await f.Record();var old=f.Submission;
        await f.Prepare(quantity:4m);await f.Record();f.Submission=old;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());
        Assert.Equal(18m,f.Item.QUANTITY_ON_HAND);Assert.Equal(6,f.Native.Writes);
    }
    [Theory][InlineData(true)][InlineData(false)] public async Task ReturnCannotExceedOriginalReceiptOrCurrentStock(bool original)
    {
        var f=new Fixture();await f.Initialize();await f.Record();var id=f.Submission.Command.IntentId;if(!original)f.Item.QUANTITY_ON_HAND=1m;
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,original?5m:2m,id);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(3,f.Native.Writes);
    }
    [Theory][InlineData("quantity")][InlineData("reason")][InlineData("proof")][InlineData("audit")][InlineData("item")]
    public async Task ReplacedIntentOrRecordedEvidenceCannotBeRepairedByRetry(string mode)
    {
        var f=new Fixture();await f.Initialize();await f.Record();
        if(mode=="quantity")f.Submission=f.Submission with{Command=f.Submission.Command with{Quantity=3}};
        if(mode=="reason")f.Submission=f.Submission with{Command=f.Submission.Command with{Reason="Replacement reason"}};
        var row=Assert.Single(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
        if(mode=="proof")row.PURCHASE_RECEIPT_STOCK_JSON="{}";if(mode=="audit")row.ROW_CHANGED_BY="other";if(mode=="item")f.Item.TOTAL_VALUE=100;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(3,f.Native.Writes);Assert.Equal(14m,f.Item.QUANTITY_ON_HAND);
    }
    [Theory][InlineData("movement")][InlineData("item")][InlineData("purchase")]
    public async Task PostCommitDriftReturnsUnconfirmedAndRetryNeverRepairsIt(string mode)
    {
        var f=new Fixture();await f.Initialize();f.Native.AfterCommit=()=>{
            if(mode=="movement")f.Native.Rows.RemoveAll(value=>value is INVENTORY_TRANSACTION);
            if(mode=="item")f.Item.QUANTITY_ON_HAND=15m;if(mode=="purchase")f.Native.Rows.OfType<PURCHASE_ORDER>().Single().DESCRIPTION="changed";
        };
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());f.Native.AfterCommit=null;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(3,f.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","rollback"},f.Native.Transactions);
    }
    [Fact] public async Task PartialReturnsAccumulateAgainstOneOriginalAndDoNotFreeReceiptCapacity()
    {
        var f=new Fixture();await f.Initialize();await f.Record();var id=f.Submission.Command.IntentId;
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,3m,id);await f.Record();
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,2m,id);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(6,f.Native.Writes);
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,1m,id);var result=await f.Record();var stock=PurchaseReceiptStockReview.Create(result);
        Assert.Equal(4m,stock.ReturnedQuantity);Assert.Equal(4m,stock.RemainingReceiptQuantity);Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Equal(9,f.Native.Writes);
    }
    [Theory][InlineData("write")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("purchase")][InlineData("duplicate")]
    public async Task RecoveryInsertionFailureRollsBackMovementAndPreventsQuantityWrite(string mode)
    {
        var f=new Fixture();await f.Initialize();f.FailRecovery=mode=="write";f.CorruptRecovery=mode=="corrupt";
        f.OnRecovery=()=>{
            if(mode=="authority")f.Revoked=true;if(mode=="binding")f.Native.Connection="changed";
            if(mode=="purchase")f.Receiving.Existing.Order.DESCRIPTION="changed";
            if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single()));
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);
        Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());Assert.Empty(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>());
        Assert.Equal(new[]{"begin","rollback"},f.Native.Transactions);Assert.Equal(mode=="purchase"?3:2,f.Native.Writes);
    }
    [Theory][InlineData("collision")][InlineData("metadata")]
    public async Task RecoveryIdentityAndMetadataMustBeAvailableBeforeAnyStockWrite(string mode)
    {
        var f=new Fixture();await f.Initialize();
        if(mode=="collision")f.Native.Rows.Add(new INVENTORY_STOCK_INTENT{INVENTORY_STOCK_INTENT_ID=f.Submission.Command.IntentId,OWNER_TENANT_ID="other"});
        else f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="INVENTORY_STOCK_INTENT"),true)).Returns(new EntityStructure());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Native.Writes);Assert.Equal(10m,f.Item.QUANTITY_ON_HAND);Assert.Empty(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());
    }
    [Theory][InlineData("missing")][InlineData("altered")][InlineData("duplicate")]
    public async Task PostCommitRecoveryDamageCannotBeRepairedByCommandRetry(string mode)
    {
        var f=new Fixture();await f.Initialize();f.Native.AfterCommit=()=>{
            var entry=f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().Single();
            if(mode=="missing")f.Native.Rows.Remove(entry);if(mode=="altered")entry.ORIGINAL_ITEM_JSON="{}";
            if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(entry));
        };
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());f.Native.AfterCommit=null;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(3,f.Native.Writes);Assert.Equal(14m,f.Item.QUANTITY_ON_HAND);
        Assert.Single(f.Native.Rows.OfType<INVENTORY_TRANSACTION>());Assert.Equal(new[]{"begin","commit","begin","rollback"},f.Native.Transactions);
    }
}
#endif
