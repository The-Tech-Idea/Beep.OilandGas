using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class ReceiverAccountantHandoffTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualReceiverEvidenceHandsOffToAnIndependentConfiguredAccountant(bool changedEvidence)
    {
        var production=new PurchaseReceiptDecisionTests.Fixture();var date=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);
        production.Existing.Order.ROW_CREATED_BY=production.Existing.OrderLine.ROW_CREATED_BY="po-preparer";
        production.Existing.Order.ROW_CREATED_DATE=production.Existing.OrderLine.ROW_CREATED_DATE=date;
        var workflow=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");workflow.Definition.ENTITY_TYPE="PURCHASE_ORDER";workflow.Native.Destinations=["ACCOUNTANT"];CompanyStepCommandTests.ConfigureUpdates(workflow);
        workflow.Native.Personas.Setup(value=>value.GetActivePersonasAsync("Controller")).ReturnsAsync(new List<string>{"CONTROLLER"});
        using var rbac=new WorkflowInboxHttpTests.Fixture("ACCOUNTANT");
        string[] receiverPermissions=["Procurement.View","Procurement.RecordReceiptDecision","Workflow.Start"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:receiverPermissions,configure:(services,context)=>{
            rbac.Configure(services,context);context.FixedProcess=workflow.Service;context.StartBinding=new string('B',64);
            services.AddSingleton<IProcessService>(workflow.Service);services.AddSingleton<ICrossPersonaTaskRouter>(workflow.Native.Router);
        },configureAccounting:(services,context)=>{services.AddSingleton(production.Existing.PO);services.AddSingleton(production.Existing.AP);context.Source=production.Existing.Native.Source;context.Actor="receiver";context.Roles=["ReceiverRole"];context.Permissions=receiverPermissions;});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","receiver");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ReceiverRole");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"receiver",new(new("tenant","company"),["ReceiverRole"],receiverPermissions));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var accounting=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var processes=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        var original=await accounting.GetPurchaseReceiptDecisionReviewAsync("order","receipt");
        var intent=new PurchaseReceiptDecisionSubmission(new(original.Source.Company,"receiver","order","receipt",8,2,"bbl",original.Source.Revision,original.BindingRevision,Guid.NewGuid().ToString("N"),"Receiver inspected damaged goods."),original);
        var received=await accounting.RecordPurchaseReceiptDecisionAsync(intent);Assert.Equal("receiver",received.Receipt.ROW_CHANGED_BY);
        var order=(await accounting.GetPurchaseOrderAsync("order"))!;Assert.NotEqual(original.Source.Revision,order.Revision);
        using var handoff=new CompanyAccountingHandoffState(company);handoff.AttachSource("PURCHASE_ORDER","order",order.Revision);
        await handoff.LoadCatalogAsync(processes);handoff.Select("definition");await handoff.SubmitAsync(processes);
        var started=handoff.Receipt!;Assert.Single(started.TaskIds);Assert.Equal("receiver",handoff.Intent!.ActorUserId);
        Assert.Equal(order.Revision,handoff.Intent.SourceRevision);Assert.Equal("po-preparer",handoff.Intent.SourcePreparerUserId);
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");
        host.Context.Actor="actor";host.Context.Roles=["ConfiguredCompanyRole"];host.Context.Permissions=["Accounting.View","Procurement.View","Workflow.Approve"];
        company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],host.Context.Permissions));
        var inbox=await new UnifiedTaskInboxService(api).GetInboxAsync("ACCOUNTANT");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
        Assert.Equal(started.TaskIds[0],task.TaskId);Assert.Equal(started.InstanceId,task.WorkflowName);Assert.Equal("PURCHASE_ORDER",task.EntityType);
        var review=(await processes.ReviewCompanyAccountingStepAsync(started.InstanceId))!;Assert.True(review.Eligible);Assert.Equal("actor",review.ActorUserId);
        Assert.Equal("receiver",review.WorkflowStarterUserId);Assert.Equal("po-preparer",review.SourceMakerUserId);Assert.Equal(order.Revision,review.SourceRevision);
        if(changedEvidence)
        {
            production.Stored.RECEIVER_DECISION_JSON="{}";
            await Assert.ThrowsAsync<OilGasApiException>(()=>processes.CompleteCompanyAccountingStepAsync(new(review,Guid.NewGuid().ToString("N"),"Reviewed receiver evidence.")));
            Assert.Equal(5,workflow.Native.Writes);Assert.Equal(1,production.Existing.Native.Writes);
        }
        else
        {
            var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Independently reviewed receiver evidence; financial approval remains separate.");
            var completion=await processes.CompleteCompanyAccountingStepAsync(command);completion.Require(command);
            Assert.Contains(started.TaskIds[0],completion.CompletedTaskIds);Assert.Single(completion.NextTaskIds);
            Assert.Equal(8m,production.Stored.ACCEPTED_QUANTITY);Assert.Equal(2m,production.Stored.REJECTED_QUANTITY);Assert.Equal("DRAFT",production.Existing.Invoice.STATUS);
            Assert.Equal(1,production.Existing.Native.Writes);Assert.Equal("commit",workflow.Native.Transactions.Last());
        }
    }
}
public sealed class ProcurementWarehouseInboxJourneyTests
{
    [Theory][InlineData("complete")][InlineData("approved")][InlineData("source-drift")][InlineData("history-drift")][InlineData("revoked")]
    public async Task ReceiptPartialReturnsAndLaterLinkCyclesRetainTheirOwnersThroughIndependentInboxReview(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();var po=f.Receiving.Existing.PO;var scope=new CompanyScope("tenant","company");
        f.Receiving.Existing.Order.ROW_CREATED_BY=f.Receiving.Existing.OrderLine.ROW_CREATED_BY="po-preparer";
        f.Receiving.Existing.Order.ROW_CREATED_DATE=f.Receiving.Existing.OrderLine.ROW_CREATED_DATE=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc);
        await f.Initialize("receiver");var receiver=JsonSerializer.Serialize(f.Receiving.Stored);
        await f.Prepare(actor:"warehouse");await f.Record();var originalReceipt=f.Submission.Command.IntentId;
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,originalReceipt,"warehouse");await f.Record();
        await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,originalReceipt,"warehouse");var returned=await f.Record();
        var stock=PurchaseReceiptStockReview.Create(returned);Assert.Equal(4m,stock.ReceivedQuantity);Assert.Equal(2m,stock.ReturnedQuantity);Assert.Equal(4m,stock.RemainingReceiptQuantity);
        Assert.Equal(12m,f.Item.QUANTITY_ON_HAND);var item=JsonSerializer.Serialize(f.Item);
        var movements=JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_TRANSACTION>().OrderBy(row=>row.INVENTORY_TRANSACTION_ID));
        var recoveryEntries=JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID));
        var creator=new ClassCreator(f.Native.Editor.Object);
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="PO_INVENTORY_LINK_HISTORY"),true)).Returns(creator.ConvertToEntityStructure(typeof(PO_INVENTORY_LINK_HISTORY)));
        var metadata=(IPPDMMetadataRepository)typeof(PurchaseOrderService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(po)!;
        Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_LINE_ITEM")).ReturnsAsync(new List<string>{"PO_LINE_ITEM_ID"});
        Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync("PO_INVENTORY_LINK_HISTORY")).ReturnsAsync(new List<string>{"PO_INVENTORY_LINK_HISTORY_ID"});
        f.Native.Source.Setup(value=>value.UpdateEntity("PO_LINE_ITEM",It.IsAny<object>())).Returns((string table,object value)=>{
            ++f.Native.Writes;var row=CompanyInventorySourceReview.Copy((PO_LINE_ITEM)value);f.Native.Rows.RemoveAll(value=>value is PO_LINE_ITEM);f.Native.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};});
        f.Native.Source.Setup(value=>value.InsertEntity("PO_INVENTORY_LINK_HISTORY",It.IsAny<object>())).Returns((string table,object value)=>{
            ++f.Native.Writes;f.Native.Rows.Add(CompanyInventorySourceReview.Copy((PO_INVENTORY_LINK_HISTORY)value));return new ErrorsInfo{Flag=Errors.Ok};});
        for(var cycle=0;cycle<2;cycle++)
        {
            // The approved case changes the current line while preserving its earlier physical receipt destination.
            var target=new INVENTORY_ITEM{INVENTORY_ITEM_ID=$"alternate-{cycle}",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",UNIT_OF_MEASURE="bbl",WAREHOUSE_ID="warehouse",LOCATION_ID="other-bin"};
            CompanyInventorySourceReview Target(string actor)=>CompanyInventorySourceReview.Create(scope,actor,target,[]);
            var purchase=(await po.GetSourceReviewAsync(scope,"order","link-preparer"))!;
            var preparation=PurchaseOrderInventoryLinkProposalReview.Create(purchase,Target("link-preparer"),"po-line","binding");
            await po.PrepareInventoryLinkAsync(new(scope,"link-preparer","order","po-line",target.INVENTORY_ITEM_ID,preparation.Revision,"binding",Guid.NewGuid().ToString("N"),"Review alternate destination."),preparation,()=>Task.FromResult(Target("link-preparer")),()=>Task.CompletedTask);
            purchase=(await po.GetSourceReviewAsync(scope,"order","link-reviewer"))!;
            var decision=PurchaseOrderInventoryLinkDecisionReview.Create(purchase,Target("link-reviewer"),"po-line","binding");
            await po.RecordInventoryLinkDecisionAsync(new(scope,"link-reviewer","order","po-line",target.INVENTORY_ITEM_ID,mode=="approved" && cycle==1?PurchaseOrderInventoryLinkDecision.Approve:PurchaseOrderInventoryLinkDecision.Reject,decision.Revision,"binding",Guid.NewGuid().ToString("N"),"Independently review alternate destination."),decision,()=>Task.FromResult(Target("link-reviewer")),()=>Task.CompletedTask);
            purchase=(await po.GetSourceReviewAsync(scope,"order","cycle-owner"))!;
            var history=await po.GetInventoryLinkHistoryAsync(scope,"order","po-line","cycle-owner",()=>Task.CompletedTask);
            var reopen=PurchaseOrderInventoryLinkCycleReview.Create(purchase,history,"po-line",PurchaseOrderInventoryLinkCycleAction.ReopenResolved,"binding");
            await po.ReopenInventoryLinkAsync(new(scope,"cycle-owner","order","po-line",reopen.Action,reopen.Revision,"binding",Guid.NewGuid().ToString("N"),"Retain completed review before next cycle."),reopen,()=>Task.CompletedTask);
        }
        Assert.Equal(2,f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>().Count());Assert.Equal(17,f.Native.Writes);
        Assert.All(f.Native.Rows.OfType<INVENTORY_TRANSACTION>(),row=>Assert.Equal("warehouse",row.ROW_CREATED_BY));
        Assert.All(f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>(),entry=>{
            Assert.Equal("cycle-owner",entry.ROW_CREATED_BY);
            var archived=JsonSerializer.Deserialize<PO_LINE_ITEM>(entry.ARCHIVED_LINE_JSON)!;
            Assert.Equal("link-preparer",PurchaseOrderInventoryLinkProposalProof.Require(scope,archived).Command.ActorUserId);
            Assert.Equal("link-reviewer",PurchaseOrderInventoryLinkDecisionProof.Require(scope,archived).Command.ActorUserId);
        });
        var original=await f.Inventory.GetStockRecoveryAsync(scope,originalReceipt,"warehouse",po,f.Authority);
        Assert.Equal("warehouse",original.OriginalSubmission().Command.ActorUserId);Assert.False(original.CurrentMatchesOriginalEffect());
        Assert.Equal("receiver",PurchaseReceiptDecisionProof.Require(scope,f.Receiving.Stored).Command.ActorUserId);
        var workflow=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");workflow.Definition.ENTITY_TYPE="PURCHASE_ORDER";workflow.Native.Destinations=["ACCOUNTANT"];CompanyStepCommandTests.ConfigureUpdates(workflow);
        workflow.Native.Personas.Setup(value=>value.GetActivePersonasAsync("Controller")).ReturnsAsync(new List<string>{"CONTROLLER"});
        using var rbac=new WorkflowInboxHttpTests.Fixture("ACCOUNTANT");string[] grants=["Procurement.View","Inventory.View","Workflow.Start"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:grants,configure:(services,context)=>{
            rbac.Configure(services,context);context.FixedProcess=workflow.Service;context.StartBinding=new string('B',64);services.AddSingleton<IProcessService>(workflow.Service);services.AddSingleton<ICrossPersonaTaskRouter>(workflow.Native.Router);
        },configureAccounting:(services,context)=>{services.AddSingleton(po);services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.AP);context.Actor="starter";context.Permissions=grants;});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","starter");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"starter",new(scope,["ConfiguredCompanyRole"],grants)));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var processes=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        var expectedWrites=17;
        if(mode=="approved")
        {
            var receiptReview=await new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance).GetPurchaseReceiptStockReviewAsync("order","receipt");
            Assert.True(receiptReview.Source.UsesRecordedStockDestination);Assert.Equal("alternate-1",receiptReview.Source.CurrentLineInventoryItemId);
            Assert.Equal("item",receiptReview.Source.Inventory.Item.INVENTORY_ITEM_ID);Assert.Equal(4m,receiptReview.ReceivedQuantity);Assert.Equal(2m,receiptReview.ReturnedQuantity);Assert.Equal(4m,receiptReview.RemainingReceiptQuantity);
            await f.Prepare(quantity:5,actor:"warehouse");await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(17,f.Native.Writes);
            await f.Prepare(PurchaseReceiptStockMovementKind.Return,1,originalReceipt,"warehouse");await f.Record();expectedWrites=20;
            receiptReview=await new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance).GetPurchaseReceiptStockReviewAsync("order","receipt");
            Assert.Equal(3m,receiptReview.ReturnedQuantity);Assert.Equal(4m,receiptReview.RemainingReceiptQuantity);Assert.Equal(11m,receiptReview.Source.Inventory.Item.QUANTITY_ON_HAND);
            item=JsonSerializer.Serialize(f.Item);movements=JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_TRANSACTION>().OrderBy(row=>row.INVENTORY_TRANSACTION_ID));
            recoveryEntries=JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID));
        }
        var source=(await po.GetSourceReviewAsync(scope,"order","starter"))!;
        using var handoff=new CompanyAccountingHandoffState(company);handoff.AttachSource("PURCHASE_ORDER","order",source.Revision);await handoff.LoadCatalogAsync(processes);handoff.Select("definition");await handoff.SubmitAsync(processes);
        Assert.Equal("po-preparer",handoff.Intent!.SourcePreparerUserId);var started=handoff.Receipt!;
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Context.Actor="actor";host.Context.Permissions=["Procurement.View","Inventory.View","Workflow.Approve"];Assert.True(company.TrySelect(company.Clear(),"actor",new(scope,["ConfiguredCompanyRole"],host.Context.Permissions)));
        var inbox=await new UnifiedTaskInboxService(api).GetInboxAsync("ACCOUNTANT");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));Assert.Equal(started.TaskIds[0],task.TaskId);
        var review=(await processes.ReviewCompanyAccountingStepAsync(started.InstanceId))!;Assert.True(review.Eligible);Assert.Equal("po-preparer",review.SourceMakerUserId);Assert.Equal("starter",review.WorkflowStarterUserId);Assert.Equal(source.Revision,review.SourceRevision);
        var command=new CompanyAccountingStepCommand(review,Guid.NewGuid().ToString("N"),"Independent review of procurement source; physical stock and financial posting remain separate.");
        if(mode=="source-drift")f.Receiving.Existing.Order.DESCRIPTION="Changed after accountant review";
        if(mode=="history-drift")f.Native.Rows.OfType<PO_INVENTORY_LINK_HISTORY>().First().REASON="Changed after accountant review";
        if(mode=="revoked")host.Context.Permissions=["Procurement.View","Workflow.Approve"];
        if(mode is "complete" or "approved")
        {
            var completion=await processes.CompleteCompanyAccountingStepAsync(command);completion.Require(command);Assert.Contains(task.TaskId,completion.CompletedTaskIds);Assert.Single(completion.NextTaskIds);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>processes.CompleteCompanyAccountingStepAsync(command));Assert.Equal(mode=="revoked"?System.Net.HttpStatusCode.Forbidden:System.Net.HttpStatusCode.Conflict,error.StatusCode);Assert.Equal(5,workflow.Native.Writes);
        }
        Assert.Equal(expectedWrites,f.Native.Writes);Assert.Equal(receiver,JsonSerializer.Serialize(f.Receiving.Stored));Assert.Equal(item,JsonSerializer.Serialize(f.Item));
        Assert.Equal(movements,JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_TRANSACTION>().OrderBy(row=>row.INVENTORY_TRANSACTION_ID)));
        Assert.Equal(recoveryEntries,JsonSerializer.Serialize(f.Native.Rows.OfType<INVENTORY_STOCK_INTENT>().OrderBy(row=>row.INVENTORY_STOCK_INTENT_ID)));
        Assert.Equal("DRAFT",f.Receiving.Existing.Invoice.STATUS);Assert.Empty(f.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
}
#endif
