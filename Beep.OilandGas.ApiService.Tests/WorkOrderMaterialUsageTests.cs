using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
#if WORK_ORDER_BUDGET_FOCUSED
extern alias cachedAccounting;
#endif
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

/// <summary>Actual inventory/repository/transaction owner; execution proof callback and provider are controlled seams.</summary>
public sealed class WorkOrderMaterialUsageTests
{
#if WORK_ORDER_BUDGET_FOCUSED
    private async Task<(Fixture Physical,Beep.OilandGas.ProductionAccounting.Services.AfeService Costs,CompanyWorkOrderMaterialCostReview Basis)> Financial()
    {
        var f=new Fixture();await f.Record();var context=f.Context with{ActorUserId="budget-maker",Revision="financial-authority"};
        var book=new CompanyLedgerBookFacts(Guid.NewGuid().ToString("N"),"book","Recorded company book","KWD",3,"TO_EVEN","calendar","UTC","book-maker",DateTime.UtcNow,new string('A',64));
        var order=f.Command.Review.Material.Source.WorkOrder.Order;var query=new CompanyWorkOrderBudgetQuery(order.InstanceId,order.FieldId,book.BookId);
        var current=new CompanyWorkOrderBudgetReview(context,context.Database,query,order,book,[],DateTime.UtcNow);
        var draft=new CompanyWorkOrderBudgetCommand(current,Guid.NewGuid(),"MaterialAFE","Recorded material budget",100m,f.Command.UsedAtUtc.AddDays(-1),"Original budget basis").Prepare(DateTime.UtcNow);
        current=current with{Context=context with{ActorUserId="budget-reviewer"},Budgets=[CompanyWorkOrderBudgetCommand.Facts(draft)],ComparedAtUtc=DateTime.UtcNow};
        var decisionQuery=new CompanyWorkOrderBudgetDecisionQuery(query,draft.AFE_ID);var approval=new CompanyWorkOrderBudgetDecisionCommand(new(current,decisionQuery),Guid.NewGuid(),WorkOrderBudgetDecision.Approve,"Independent material budget approval").Prepare(DateTime.UtcNow);
        current=current with{Context=context with{ActorUserId="financial-recorder"},ComparedAtUtc=DateTime.UtcNow};var decision=CompanyWorkOrderBudgetDecisionFacts.From(approval);
        var status=new CompanyWorkOrderBudgetDecisionStatus(current,decisionQuery,decision,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,decisionQuery,decision),DateTime.UtcNow);
        var native=f.Stock.Native;var metadata=(IPPDMMetadataRepository)typeof(InventoryService).GetField("_metadata",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(f.Stock.Inventory)!;
        var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        foreach(var (table,type,key) in new[]{("AFE",typeof(AFE),"AFE_ID"),("APPROVAL_WORKFLOW",typeof(APPROVAL_WORKFLOW),"APPROVAL_WORKFLOW_ID"),("ACCOUNTING_COST",typeof(ACCOUNTING_COST),"ACCOUNTING_COST_ID")})
        {
            native.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
            Mock.Get(metadata).Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
        }
        native.Rows.AddRange([draft,approval]);native.Source.Setup(value=>value.InsertEntity("ACCOUNTING_COST",It.IsAny<object>())).Returns((string _,object row)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((ACCOUNTING_COST)row));return new ErrorsInfo{Flag=Errors.Ok};});
        var owner=new Beep.OilandGas.ProductionAccounting.Services.AfeService(native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata);
        var source=await f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(current.Context,current.BudgetDatabase,query.WorkOrderId,f.Command.UsageId(),f.Authority);
        var basis=await owner.ReviewWorkOrderMaterialCostAsync(status,new(decisionQuery,source.UsageId),source,f.Authority);return(f,owner,basis);
    }
    [Fact] public async Task MaterialActualCostUsesRecordedConsumptionOnceWithoutAnotherStockMovement()
    {
        var (f,owner,basis)=await Financial();var stock=JsonSerializer.Serialize(f.Stock.Item);var movement=JsonSerializer.Serialize(f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single());
        var command=new CompanyWorkOrderMaterialCostCommand(basis,Guid.NewGuid(),12.345m,"EA","KWD",basis.ComparedAtUtc,"Recorded-material","Explicit rate evidence");
        async Task<CompanyWorkOrderMaterialCostReview> Current(){var source=await f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,basis.Material.WorkOrderId,basis.Material.UsageId,f.Authority);return await owner.ReviewWorkOrderMaterialCostAsync(basis.Budget,basis.Query,source,f.Authority);}
        Task<CompanyWorkOrderMaterialCostSource> Owned()=>f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,basis.Material.WorkOrderId,basis.Material.UsageId,f.Authority,true);
        var result=await owner.RecordWorkOrderMaterialCostAsync(command,f.Authority,Current,Owned);command.RequireRecorded(result.Recorded);Assert.Equal(4,f.Stock.Native.Writes);Assert.Single(f.Stock.Native.Rows.OfType<ACCOUNTING_COST>());
        Assert.Equal(12.345m,result.Recorded.AMOUNT);Assert.Equal(1m,result.Recorded.SOURCE_QUANTITY);Assert.Equal("EA",result.Recorded.SOURCE_UNIT);Assert.Null(result.Recorded.FINANCE_ID);
        Assert.Equal(stock,JsonSerializer.Serialize(f.Stock.Item));Assert.Equal(movement,JsonSerializer.Serialize(f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single()));
        var totals=await owner.ReadWorkOrderActualCostsAsync(basis.Budget,f.Authority);Assert.Equal(12.345m,totals.RecordedTotal());Assert.Equal(87.655m,totals.RemainingOriginalBudget());
        Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(totals));Assert.DoesNotContain(f.Command.Reason,JsonSerializer.Serialize(basis.Material));Assert.DoesNotContain(f.Command.IntentId.ToString(),JsonSerializer.Serialize(basis.Material));
        var closing=await Current();await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordWorkOrderMaterialCostAsync(command with{Review=closing,IntentId=Guid.NewGuid()},f.Authority,Current,Owned));
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await owner.RecordWorkOrderMaterialCostAsync(command,f.Authority,Current,Owned)));Assert.Equal(4,f.Stock.Native.Writes);
        f.Stock.Native.Rows.RemoveAll(row=>row is WORK_ORDER_MATERIAL_USAGE or INVENTORY_TRANSACTION or AFE or APPROVAL_WORKFLOW);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await owner.RecoverWorkOrderMaterialCostAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,command.Reference(),f.Authority)));Assert.Equal(4,f.Stock.Native.Writes);
    }
    [Theory][InlineData("quantity")][InlineData("movement")][InlineData("owner")][InlineData("schema")][InlineData("binding")]
    public async Task FinancialMaterialSourceRefusesUnprovenPhysicalConsumption(string fault)
    {
        var (f,_,basis)=await Financial();var row=f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>().Single();
        if(fault=="quantity")row.QUANTITY=999;if(fault=="movement")f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>().Single().QUANTITY=-999;if(fault=="owner"){row.OWNER_COMPANY_BA_ID="foreign";row.COMMAND_JSON="{}";}
        if(fault=="schema")f.Stock.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_MATERIAL_USAGE"),true)).Returns(new EntityStructure());
        var production=fault=="binding"?basis.Budget.Current.BudgetDatabase with{Fingerprint="different"}:basis.Budget.Current.BudgetDatabase;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(basis.Budget.Current.Context,production,basis.Material.WorkOrderId,basis.Material.UsageId,f.Authority));Assert.Equal(3,f.Stock.Native.Writes);
    }
    [Theory][InlineData("Rate")][InlineData("RateUnit")][InlineData("CurrencyCode")][InlineData("ValuedAtUtc")]
    public async Task MaterialValuationRequiresExplicitFinancialJsonFacts(string property)
    {
        var (_,_,basis)=await Financial();var command=new CompanyWorkOrderMaterialCostCommand(basis,Guid.NewGuid(),1m,"EA","KWD",basis.ComparedAtUtc,"category","Explicit evidence");
        var json=System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(command))!.AsObject();Assert.True(json.Remove(property));Assert.Throws<JsonException>(()=>JsonSerializer.Deserialize<CompanyWorkOrderMaterialCostCommand>(json.ToJsonString()));
    }
    [Theory][InlineData("unit")][InlineData("currency")][InlineData("rate")][InlineData("budget")][InlineData("owned-source")][InlineData("unknown")]
    public async Task MaterialCostCannotInferFinancialFactsOrReplaceUncertainRecordedConsumption(string fault)
    {
        var (f,owner,basis)=await Financial();var command=new CompanyWorkOrderMaterialCostCommand(basis,Guid.NewGuid(),1m,"EA","KWD",basis.ComparedAtUtc,"category","Explicit evidence");
        if(fault=="unit")command=command with{RateUnit="KG"};if(fault=="currency")command=command with{CurrencyCode="USD"};if(fault=="rate")command=command with{Rate=-1};if(fault=="budget")command=command with{Rate=101m};
        if(fault=="unknown")f.Stock.Native.FailCommit=true;
        async Task<CompanyWorkOrderMaterialCostReview> Current(){var source=await f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,basis.Material.WorkOrderId,basis.Material.UsageId,f.Authority);return await owner.ReviewWorkOrderMaterialCostAsync(basis.Budget,basis.Query,source,f.Authority);}
        Task<CompanyWorkOrderMaterialCostSource> Owned()=>fault=="owned-source"?Task.FromResult(basis.Material with{Quantity=2m}):f.Stock.Inventory.ReadWorkOrderMaterialCostSourceAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,basis.Material.WorkOrderId,basis.Material.UsageId,f.Authority,true);
        await Assert.ThrowsAnyAsync<Exception>(()=>owner.RecordWorkOrderMaterialCostAsync(command,f.Authority,Current,Owned));
        if(fault=="unknown")
        {
            Assert.Equal(4,f.Stock.Native.Writes);Assert.Single(f.Stock.Native.Rows.OfType<ACCOUNTING_COST>());
            var recovered=await owner.RecoverWorkOrderMaterialCostAsync(basis.Budget.Current.Context,basis.Budget.Current.BudgetDatabase,command.Reference(),f.Authority);command.RequireRecorded(recovered.Recorded);Assert.Equal(4,f.Stock.Native.Writes);
        }
        else{Assert.Equal(3,f.Stock.Native.Writes);Assert.Empty(f.Stock.Native.Rows.OfType<ACCOUNTING_COST>());}
    }
#endif
    private sealed class StockFixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native = new();
        internal readonly InventoryService Inventory;
        internal INVENTORY_ITEM Item => Assert.Single(Native.Rows.OfType<INVENTORY_ITEM>());
        internal bool FailItem, CorruptMovement, CorruptItem;
        internal Action? OnInsert, OnUpdate;
        internal StockFixture()
        {
            var metadata = new Mock<IPPDMMetadataRepository>(); var creator = new ClassCreator(Native.Editor.Object);
            foreach(var (name,type) in new[]{("INVENTORY_ITEM",typeof(INVENTORY_ITEM)),("INVENTORY_TRANSACTION",typeof(INVENTORY_TRANSACTION))})
            {
                metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(name)).ReturnsAsync([name+"_ID"]);
                Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName==name),true))
                    .Returns(()=>creator.ConvertToEntityStructure(type));
            }
            var columns = new CommonColumnHandler(); var defaults = Mock.Of<IPPDM39DefaultsRepository>();
#if WORK_ORDER_BUDGET_FOCUSED
            // Posting is an unused cached dependency seam in this focused physical/financial source gate.
            var accounts = new cachedAccounting::Beep.OilandGas.Accounting.Services.GLAccountService(Native.Editor.Object,columns,defaults,metadata.Object,NullLogger<cachedAccounting::Beep.OilandGas.Accounting.Services.GLAccountService>.Instance);
            var journal = new cachedAccounting::Beep.OilandGas.Accounting.Services.JournalEntryService(Native.Editor.Object,columns,defaults,metadata.Object,accounts,NullLogger<cachedAccounting::Beep.OilandGas.Accounting.Services.JournalEntryService>.Instance);
#else
            var accounts = new GLAccountService(Native.Editor.Object,columns,defaults,metadata.Object,NullLogger<GLAccountService>.Instance);
            var journal = new JournalEntryService(Native.Editor.Object,columns,defaults,metadata.Object,accounts,NullLogger<JournalEntryService>.Instance);
#endif
            Inventory = new(Native.Editor.Object,columns,defaults,metadata.Object,new AccountingBasisPostingService(journal),
                NullLogger<InventoryService>.Instance,resolveConnection:()=>Task.FromResult(Native.Connection));
            Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
            Native.Source.Setup(value=>value.InsertEntity("INVENTORY_TRANSACTION",It.IsAny<object>())).Returns((string _,object value)=>
            {
                Native.Writes++;var row=CompanyInventorySourceReview.Copy((INVENTORY_TRANSACTION)value);
                if(CorruptMovement)row.QUANTITY=999m;Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
            Native.Source.Setup(value=>value.UpdateEntity("INVENTORY_ITEM",It.IsAny<object>())).Returns((string _,object value)=>
            {
                Native.Writes++;if(FailItem)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((INVENTORY_ITEM)value);
                if(CorruptItem)row.QUANTITY_ON_HAND=999m;Native.Rows.RemoveAll(item=>item is INVENTORY_ITEM);Native.Rows.Add(row);
                OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
        }
    }
    private sealed class Fixture
    {
        internal readonly StockFixture Stock = new();
        internal readonly WorkOrderMaterialAcceptanceFixture Accepted;
        internal readonly CompanyWorkOrderMaterialUsageCommand Command;
        internal readonly FieldSelectionReview Context;
        internal int Proofs;
        internal bool Revoked, Drift, CorruptIntent, FailIntent;
        internal Fixture()
        {
            Accepted = new(requirementProof: true, inventoryDatabase: new(Stock.Native.Connection, "production-fingerprint"));
            var native = Stock.Native;
            native.Rows.Clear(); native.Rows.Add(CompanyInventorySourceReview.Copy(Accepted.Inventory));
            native.Rows.Add(CompanyInventorySourceReview.Copy(Accepted.Allocation));
            var metadata = (IPPDMMetadataRepository)typeof(InventoryService).GetField("_metadata", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(Stock.Inventory)!;
            var creator = new ClassCreator(native.Editor.Object);
            native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_MATERIAL_USAGE")).Returns(true);
            foreach (var (table, type, key) in new[] { ("WORK_ORDER_MATERIAL_ALLOCATION", typeof(WORK_ORDER_MATERIAL_ALLOCATION), "ALLOCATION_ID"),
                ("WORK_ORDER_MATERIAL_RELEASE", typeof(WORK_ORDER_MATERIAL_RELEASE), "RELEASE_ID"),
                ("WORK_ORDER_MATERIAL_USAGE", typeof(WORK_ORDER_MATERIAL_USAGE), "USAGE_ID") })
            {
                Mock.Get(metadata).Setup(value => value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
                native.Source.Setup(value => value.GetEntityStructure(It.Is<EntityStructure>(request => request.EntityName == table), true))
                    .Returns(() => creator.ConvertToEntityStructure(type));
            }
            native.Source.Setup(value => value.InsertEntity("WORK_ORDER_MATERIAL_USAGE", It.IsAny<object>())).Returns((string _, object value) =>
            {
                native.Writes++; if (FailIntent) return new ErrorsInfo { Flag = Errors.Failed };
                var row = CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_USAGE)value);
                if (CorruptIntent) row.COMMAND_JSON = "{}"; native.Rows.Add(row); return new ErrorsInfo { Flag = Errors.Ok };
            });
            var old = CompanyWorkOrderMaterialAllocationCommand.Read(Accepted.Allocation).Review.Source;
            Context = old.Context with { ActorUserId = "executor", Revision = "current-executor-authority" };
            var stage = old.WorkOrder.Stage with { Context = old.WorkOrder.Stage.Context with { ActorUserId = "executor" } };
            var work = old.WorkOrder with { Stage = stage, SnapshotRevision = CompanyMaintenanceInspectionReview.Revision(stage,
                old.WorkOrder.InspectionDatabase, old.WorkOrder.Order, old.WorkOrder.Inspections) };
            var inventory = CompanyInventorySourceReview.Create(Context.Company, Context.ActorUserId, Stock.Item, []);
            var source = old with { Context = Context, WorkOrder = work, Inventory = inventory,
                Blockers = CompanyWorkOrderMaterialSourceReview.GetBlockers(old.Query, inventory) };
            var material = CompanyWorkOrderMaterialAllocationReview.Create(source, [Accepted.Allocation], DateTime.UtcNow);
            var startIntent = Guid.NewGuid(); var permit = new CompanyWorkOrderExecutionPermitReference(Accepted.Permit.Task, Accepted.Permit.IntentId);
            var start = new CompanyWorkOrderRecordedStart(Context.Company, permit, startIntent,
                new CompanyWorkOrderExecutionStartReference(permit, startIntent).StartId(Context.Company), Accepted.Permit.PermitId(),
                "executor", "employee", "ConfiguredRole", "execute", material.ComparedAtUtc, material.ComparedAtUtc, new string('A', 64), new string('B', 64));
            var review = new CompanyWorkOrderMaterialUsageReview(start, CompanyWorkOrderMaterialReservationFacts.From(Accepted.Allocation),
                material, Accepted.Permit.ValidToUtc, DateTime.UtcNow);
            Command = new(review, Guid.NewGuid(), 1m, "EA", DateTime.UtcNow, "Actual issue to the original work order"); Command.Require();
        }
        internal Task Authority() => Revoked ? Task.FromException(new UnauthorizedAccessException("Current actor revoked")) : Task.CompletedTask;
        internal Task Execution(bool owned)
        {
            Proofs++; if (owned) PPDMDataSourceTransaction.RequireOwned(Stock.Native.Source.Object, IsolationLevel.Serializable);
            else PPDMDataSourceTransaction.RequireIdle(Stock.Native.Source.Object);
            return Drift ? Task.FromException(new InvalidOperationException("Original execution source changed")) : Task.CompletedTask;
        }
        internal Task<CompanyWorkOrderMaterialUsageRecovery> Record() => Stock.Inventory.RecordWorkOrderMaterialUsageAsync(Command, Authority, Execution);
        internal Task<CompanyWorkOrderMaterialUsageRecovery> Recover() => Stock.Inventory.RecoverWorkOrderMaterialUsageAsync(Context,
            new(new(Command.Review.Start.Permit, Command.Review.Start.IntentId), Command.IntentId), Authority);
    }

    [Fact]
    public async Task PhysicalUsageDecrementsStockOnceAndOnlyRemainingReservationConsumesCapacity()
    {
        var f = new Fixture(); var result = await f.Record();
        Assert.Equal(3, f.Stock.Native.Writes); Assert.Equal(4m, f.Stock.Item.QUANTITY_ON_HAND);
        result.Require(f.Context.Company, f.Context.ActorUserId, new(new(f.Command.Review.Start.Permit, f.Command.Review.Start.IntentId), f.Command.IntentId));
        Assert.Null(result.Movement.UNIT_COST); Assert.Null(result.Movement.TOTAL_COST); Assert.Null(result.Movement.NOTES);
        Assert.DoesNotContain("COMMAND_JSON", result.Movement.WORK_ORDER_MATERIAL_USAGE_JSON);
        Assert.DoesNotContain(f.Command.Reason, result.Movement.WORK_ORDER_MATERIAL_USAGE_JSON);
        var proofs = f.Proofs; Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(await f.Recover()));
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(await f.Record())); Assert.Equal(proofs, f.Proofs);
        Assert.Equal(3, f.Stock.Native.Writes);
        var current = await f.Stock.Inventory.GetCompanyInventorySourceAsync(f.Context.Company, f.Stock.Item.INVENTORY_ITEM_ID, f.Context.ActorUserId);
        var old = f.Command.Review.Material.Source;
        var source = old with { Inventory = current!, Blockers = CompanyWorkOrderMaterialSourceReview.GetBlockers(old.Query, current!) };
        var review = await f.Stock.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source, f.Authority);
        Assert.Equal(1m, review.ReservedQuantity); Assert.Equal(3m, review.AvailableQuantity); Assert.Single(review.Usages!);
        // Another original issue can consume the final remaining unit, without reusing the first intent.
        var next = f.Command with { IntentId = Guid.NewGuid(), Review = f.Command.Review with { Material = review, ComparedAtUtc = DateTime.UtcNow }, UsedAtUtc = DateTime.UtcNow };
        await f.Stock.Inventory.RecordWorkOrderMaterialUsageAsync(next, f.Authority, f.Execution);
        Assert.Equal(6, f.Stock.Native.Writes); Assert.Equal(3m, f.Stock.Item.QUANTITY_ON_HAND);
        Assert.Equal(2, f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>().Count());
    }

    [Theory]
    [InlineData("item-write")][InlineData("movement-readback")][InlineData("item-readback")]
    [InlineData("intent-write")][InlineData("intent-readback")][InlineData("authority")]
    [InlineData("execution-before")][InlineData("execution-after")][InlineData("unknown-commit")]
    public async Task PhysicalUsageRollsBackFailureOrRecoversTheOriginalUnknownCommit(string mode)
    {
        var f = new Fixture(); f.Stock.FailItem = mode == "item-write"; f.Stock.CorruptMovement = mode == "movement-readback";
        f.Stock.CorruptItem = mode == "item-readback"; f.FailIntent = mode == "intent-write"; f.CorruptIntent = mode == "intent-readback";
        if (mode == "authority") f.Stock.OnInsert = () => f.Revoked = true;
        if (mode == "execution-before") f.Drift = true;
        if (mode == "execution-after") f.Stock.OnUpdate = () => f.Drift = true;
        f.Stock.Native.FailCommit = mode == "unknown-commit";
        await Assert.ThrowsAnyAsync<Exception>(() => f.Record());
        if (mode != "unknown-commit")
        {
            Assert.Equal(5m, f.Stock.Item.QUANTITY_ON_HAND); Assert.Empty(f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>());
            Assert.Empty(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>()); return;
        }
        Assert.Equal(4m, f.Stock.Item.QUANTITY_ON_HAND); Assert.Single(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>());
        f.Stock.Native.FailCommit = false; f.Drift = true; var writes = f.Stock.Native.Writes;
        var saved = await f.Recover(); saved.Command.RequireRecorded(saved.Recorded);
        await f.Record(); Assert.Equal(writes, f.Stock.Native.Writes);
    }

    [Theory][InlineData("quantity")][InlineData("unit")][InlineData("utc")][InlineData("early")][InlineData("reference")]
    public void UsageRequiresExplicitReservedQuantityUnitUtcAndOriginalStart(string mode)
    {
        var f = new Fixture(); var command = f.Command;
        if (mode == "quantity") command = command with { Quantity = 3m };
        if (mode == "unit") command = command with { Unit = "bbl" };
        if (mode == "utc") command = command with { UsedAtUtc = DateTime.SpecifyKind(command.UsedAtUtc, DateTimeKind.Unspecified) };
        if (mode == "early") command = command with { UsedAtUtc = command.Review.ComparedAtUtc.AddMinutes(-1) };
        if (mode == "reference") command = command with { Review = command.Review with { Start = command.Review.Start with { StartId = "different" } } };
        Assert.ThrowsAny<Exception>(command.Require); Assert.Equal(0, f.Stock.Native.Writes);
    }

    [Fact]
    public async Task ForeignUsageHeaderRefusesBeforeMalformedPrivateBodyAndRecoveryRequiresCurrentAuthority()
    {
        var f = new Fixture(); await f.Record(); var row = Assert.Single(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>());
        row.OWNER_COMPANY_BA_ID = "foreign"; row.COMMAND_JSON = "{";
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Recover());
        f.Revoked = true; await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Recover()); Assert.Equal(3, f.Stock.Native.Writes);
    }

    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("schema")][InlineData("capacity")][InlineData("quantity-type")]
    public async Task UsageRequiresItsOwnedSerializableTargetAndPhysicalBeepSchema(string mode)
    {
        var f = new Fixture();
        if (mode == "borrowed") f.Stock.Native.Borrowed = true;
        if (mode == "isolation") f.Stock.Native.Isolation = IsolationLevel.ReadCommitted;
        if (mode is "schema" or "capacity" or "quantity-type")
        {
            var shape = new ClassCreator(f.Stock.Native.Editor.Object).ConvertToEntityStructure(typeof(WORK_ORDER_MATERIAL_USAGE));
            if (mode == "schema") shape.Fields.Single(row=>row.FieldName=="USAGE_ID").AllowDBNull = true;
            if (mode == "capacity") shape.Fields.Single(row=>row.FieldName=="COMMAND_JSON").Size1 = 32;
            if (mode == "quantity-type") shape.Fields.Single(row=>row.FieldName=="QUANTITY").Fieldtype = typeof(double).FullName!;
            f.Stock.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName=="WORK_ORDER_MATERIAL_USAGE"),true)).Returns(shape);
        }
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record()); Assert.Equal(0,f.Stock.Native.Writes);
        Assert.Equal(5m,f.Stock.Item.QUANTITY_ON_HAND);
    }

    [Theory][InlineData("missing-private")][InlineData("changed-private")][InlineData("changed-movement")][InlineData("missing-movement")]
    public async Task PublicUsageCannotReduceReservationsWithoutItsCompleteOriginalPrivateAudit(string mode)
    {
        var f = new Fixture(); await f.Record();
        var row = Assert.Single(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>());
        if(mode=="missing-private")f.Stock.Native.Rows.Remove(row);
        if(mode=="changed-private")row.COMMAND_JSON="{}";
        if(mode=="changed-movement")Assert.Single(f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>()).QUANTITY=-2m;
        if(mode=="missing-movement")f.Stock.Native.Rows.RemoveAll(value=>value is INVENTORY_TRANSACTION);
        // A migrated table must still be inspected when every public movement is missing.
        f.Stock.Native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_MATERIAL_USAGE")).Returns(true);
        var inventory=await f.Stock.Inventory.GetCompanyInventorySourceAsync(f.Context.Company,f.Stock.Item.INVENTORY_ITEM_ID,f.Context.ActorUserId);
        var source=f.Command.Review.Material.Source with{Inventory=inventory!,Blockers=CompanyWorkOrderMaterialSourceReview.GetBlockers(f.Command.Review.Material.Source.Query,inventory!)};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Stock.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,f.Authority));
        Assert.Equal(3,f.Stock.Native.Writes);
    }

    [Fact]
    public async Task LateExecutionDriftRetainsCommittedOriginalRecoveryWithoutAReplacementOrAnotherDecrement()
    {
        var f=new Fixture();f.Stock.Native.AfterCommit=()=>f.Drift=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(4m,f.Stock.Item.QUANTITY_ON_HAND);
        var first=await f.Recover();await f.Record();Assert.Equal(3,f.Stock.Native.Writes);
        var replacement=f.Command with{IntentId=Guid.NewGuid()};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Stock.Inventory.RecordWorkOrderMaterialUsageAsync(replacement,f.Authority,f.Execution));
        Assert.Equal(3,f.Stock.Native.Writes);Assert.Equal(JsonSerializer.Serialize(first),JsonSerializer.Serialize(await f.Recover()));
    }

    [Fact]
    public async Task ReplacementIntentCannotConsumeAReviewWhoseOriginalStockPreimageHasAlreadyChanged()
    {
        var f=new Fixture();await f.Record();var replacement=f.Command with{IntentId=Guid.NewGuid()};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Stock.Inventory.RecordWorkOrderMaterialUsageAsync(replacement,f.Authority,f.Execution));
        Assert.Equal(3,f.Stock.Native.Writes);Assert.Equal(4m,f.Stock.Item.QUANTITY_ON_HAND);
        Assert.Single(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_USAGE>());
    }

    [Fact]
    public async Task ReleasingAPartlyConsumedReservationFreesOnlyItsRemainingCapacityWithoutReturningPhysicalStock()
    {
        var f=new Fixture();await f.Record();
        f.Stock.Native.Source.Setup(value=>value.InsertEntity("WORK_ORDER_MATERIAL_RELEASE",It.IsAny<object>())).Returns((string _,object value)=>
        {f.Stock.Native.Writes++;f.Stock.Native.Rows.Add(CompanyInventorySourceReview.Copy((WORK_ORDER_MATERIAL_RELEASE)value));return new ErrorsInfo{Flag=Errors.Ok};});
        var allocation=f.Accepted.Allocation;var original=CompanyWorkOrderMaterialAllocationCommand.Read(allocation);
        var release=new CompanyWorkOrderMaterialReleaseCommand(new(original,allocation),Guid.NewGuid(),"Release unused reservation after actual partial use");
        await f.Stock.Inventory.ReleaseWorkOrderMaterialAsync(release,()=>Task.CompletedTask,()=>Task.CompletedTask);
        Assert.Equal(4m,f.Stock.Item.QUANTITY_ON_HAND);Assert.Equal(4,f.Stock.Native.Writes);
        Assert.Single(f.Stock.Native.Rows.OfType<INVENTORY_TRANSACTION>());
        var inventory=await f.Stock.Inventory.GetCompanyInventorySourceAsync(f.Context.Company,f.Stock.Item.INVENTORY_ITEM_ID,f.Context.ActorUserId);
        var old=f.Command.Review.Material.Source;var source=old with{Inventory=inventory!,Blockers=CompanyWorkOrderMaterialSourceReview.GetBlockers(old.Query,inventory!)};
        var current=await f.Stock.Inventory.ReviewWorkOrderMaterialAllocationsAsync(source,f.Authority);
        Assert.Equal(0m,current.ReservedQuantity);Assert.Equal(4m,current.AvailableQuantity);Assert.Single(current.Usages!);Assert.Single(current.Releases!);
        Assert.ThrowsAny<Exception>(()=>(f.Command.Review with{Material=current,ComparedAtUtc=DateTime.UtcNow}).Require());
    }

    [Theory][InlineData(false)][InlineData(true)]
    public async Task WarehouseQueueShowsActualConsumptionAndRemainingReservationOnlyForAuthorizedOrders(bool hidden)
    {
        var f=new Fixture();await f.Record();var task=f.Command.Review.Start.Permit.Task;
        if(hidden)Assert.Single(f.Stock.Native.Rows.OfType<WORK_ORDER_MATERIAL_ALLOCATION>()).COMMAND_JSON="{";
        var before=f.Stock.Native.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_MATERIAL_USAGE");
        var queue=await f.Stock.Inventory.ReadMaterialReservationQueueAsync(f.Context,order=>!hidden && order==task.WorkOrderId,f.Authority);
        queue.Require(f.Context.Company,f.Context.ActorUserId,task.FieldId);
        if(hidden)
        {
            Assert.Empty(queue.Rows);
            Assert.Equal(before,f.Stock.Native.Source.Invocations.Count(value=>value.Method.Name=="GetEntityAsync" && value.Arguments[0] as string=="WORK_ORDER_MATERIAL_USAGE"));
        }
        else
        {
            var row=Assert.Single(queue.Rows);Assert.Equal(1m,row.ConsumedQuantity);Assert.Equal(1m,row.RemainingReservedAt(queue.ComparedAtUtc));
            Assert.Single(row.Usages!);Assert.Equal(CompanyMaterialReservationStatus.Active,row.StatusAt(queue.ComparedAtUtc));
            Assert.DoesNotContain(f.Command.Reason,JsonSerializer.Serialize(queue));Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(queue));
        }
        Assert.Equal(3,f.Stock.Native.Writes);
    }

    [Theory][InlineData("unknown")][InlineData("empty")][InlineData("partial")][InlineData("consumed")]
    [InlineData("foreign")][InlineData("unit")][InlineData("overused")][InlineData("future")]
    public void WarehouseQueueDistinguishesUnknownUsageFromConfirmedZeroAndPreservesOriginalUnits(string mode)
    {
        var f=new Fixture();var reservation=f.Command.Review.Reservation;var utc=DateTime.UtcNow;
        var movement=f.Command.PrepareMovement(utc);var facts=CompanyWorkOrderMaterialUsageFacts.From(movement);
        if(mode=="consumed")facts=facts with{Quantity=reservation.Quantity};
        if(mode=="foreign")facts=facts with{Task=facts.Task with{WorkOrderId="foreign"}};
        if(mode=="unit")facts=facts with{Unit="bbl"};
        if(mode=="overused")facts=facts with{Quantity=reservation.Quantity+1m};
        if(mode=="future")facts=facts with{RecordedAtUtc=utc.AddMinutes(1)};
        var entry=new CompanyMaterialReservationQueueEntry(reservation,null){Usages=mode=="unknown"?null:mode=="empty"?[]:[facts]};
        if(mode is "foreign" or "unit" or "overused" or "future")
        {Assert.ThrowsAny<Exception>(()=>CompanyMaterialReservationQueue.Create(f.Context,[entry],utc));return;}
        var queue=CompanyMaterialReservationQueue.Create(f.Context,[entry],utc);queue.Require(f.Context.Company,f.Context.ActorUserId,f.Context.FieldId!);
        if(mode=="unknown")
        {Assert.Null(entry.ConsumedQuantity);Assert.Null(entry.RemainingReservedAt(utc));Assert.DoesNotContain("Usages",JsonSerializer.Serialize(entry));Assert.DoesNotContain("ConsumedQuantity",JsonSerializer.Serialize(entry));}
        else
        {
            Assert.Equal(mode=="empty"?0m:mode=="consumed"?reservation.Quantity:f.Command.Quantity,entry.ConsumedQuantity);
            Assert.Equal(reservation.Quantity-entry.ConsumedQuantity,entry.RemainingReservedAt(utc));
            Assert.Equal(mode=="consumed"?CompanyMaterialReservationStatus.Consumed:CompanyMaterialReservationStatus.Active,entry.StatusAt(utc));
        }
    }

    [Fact]
    public void CanonicalProductionMaterialMetadataKeepsCoreReferencesOutsideProductionForeignKeys()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(WORK_ORDER_MATERIAL_USAGE).Assembly);
        var shape=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(WORK_ORDER_MATERIAL_USAGE));
        Assert.Equal("USAGE_ID",Assert.Single(shape.Fields,field=>field.IsKey).FieldName);
        var entries=Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.WorkOrderMaterialMetadata.AddTo(new());
        Assert.Equal(3,entries.Count);Assert.Equal("USAGE_ID",entries["WORK_ORDER_MATERIAL_USAGE"].PrimaryKeyColumn);
        Assert.All(entries.Values,entry=>Assert.Equal("PRODUCTION",entry.Module));
        Assert.DoesNotContain(entries["WORK_ORDER_MATERIAL_USAGE"].ForeignKeys,key=>key.ForeignKeyColumn is "START_ID" or "WORK_ORDER_ID" or "FIELD_ID");
        var untouched=new Dictionary<string,Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata>{{"WORK_ORDER_MATERIAL_USAGE",new()}};
        Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.WorkOrderMaterialMetadata.AddTo(untouched));
        Assert.Single(untouched);
    }
}
#endif
