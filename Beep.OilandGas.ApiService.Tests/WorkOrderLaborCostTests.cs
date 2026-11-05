#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using Xunit;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderLaborCostTests
{
    [Theory][InlineData("duplicate-source")][InlineData("currency")][InlineData("future")][InlineData("overflow")]
    public void PublicTotalsCannotHideInvalidCorrelationsOrInventConvertedAmounts(string fault)
    {
        var fixture=new Fixture();var utc=DateTime.UtcNow;var row=new CompanyWorkOrderActualCostFacts("cost-one",fixture.Draft.AFE_ID,"WORK_ORDER_LABOR_USAGE","labor-one",new string('A',64),1m,"KWD",utc,new string('B',64));
        CompanyWorkOrderActualCostFacts[] rows=[row];
        if(fault=="duplicate-source")rows=[row,row with{Id="cost-two"}];if(fault=="currency")rows=[row with{CurrencyCode="USD"}];if(fault=="future")rows=[row with{ValuedAtUtc=utc.AddDays(1)}];
        if(fault=="overflow")rows=[row with{Amount=decimal.MaxValue},row with{Id="cost-two",SourceId="labor-two",Amount=decimal.MaxValue}];
        var review=new CompanyWorkOrderActualCostReview(fixture.Basis.Budget,rows,DateTime.UtcNow);Assert.Throws<InvalidOperationException>(()=>review.RecordedTotal());
    }
    [Fact] public async Task PublicActualTotalsReadCompleteNativePopulationWithoutPrivateRateRationaleOrIntent()
    {
        var fixture=new Fixture();var empty=await fixture.Owner.ReadWorkOrderActualCostsAsync(fixture.Basis.Budget,fixture.Guard);
        Assert.Empty(empty.Costs);Assert.Equal(0m,empty.RecordedTotal());Assert.Equal(100m,empty.RemainingOriginalBudget());
        var command=fixture.Command(await fixture.Review());await fixture.Record(command);
        var result=await fixture.Owner.ReadWorkOrderActualCostsAsync(fixture.Basis.Budget,fixture.Guard);Assert.Single(result.Costs);
        Assert.Equal(command.Amount(),result.RecordedTotal());Assert.Equal(100m-command.Amount(),result.RemainingOriginalBudget());
        var json=JsonSerializer.Serialize(result);Assert.DoesNotContain(command.Reason,json);Assert.DoesNotContain(command.IntentId.ToString(),json);Assert.DoesNotContain("WORK_ORDER_COST_JSON",json);
        Assert.Equal(command.Review.Labor.LaborId,result.Costs[0].SourceId);Assert.Equal(command.Review.Labor.Revision,result.Costs[0].SourceRevision);
        Assert.Equal(1,fixture.Native.Writes);Assert.Null(fixture.Draft.ACTUAL_COST);
    }
    [Theory][InlineData("schema")][InlineData("legacy")][InlineData("changed")]
    public async Task PublicActualTotalsRefuseMissingOrChangingPopulationRatherThanPublishingZero(string fault)
    {
        var fixture=new Fixture();
        if(fault=="schema")fixture.Native.Source.Setup(value=>value.CheckEntityExist("ACCOUNTING_COST")).Returns(false);
        if(fault=="legacy")fixture.Native.Rows.Add(new ACCOUNTING_COST{ACCOUNTING_COST_ID="unowned",AFE_ID=fixture.Draft.AFE_ID,AMOUNT=90});
        var calls=0;async Task Guard(){await fixture.Guard();if(fault=="changed" && ++calls==4)fixture.Native.Rows.Add(new ACCOUNTING_COST{ACCOUNTING_COST_ID="late",AFE_ID=fixture.Draft.AFE_ID,AMOUNT=90});}
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Owner.ReadWorkOrderActualCostsAsync(fixture.Basis.Budget,Guard));Assert.Equal(0,fixture.Native.Writes);
    }
    private sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly AfeService Owner;
        internal readonly AFE Draft;
        internal readonly APPROVAL_WORKFLOW Approved;
        internal readonly WORK_ORDER_LABOR_USAGE Labor;
        internal CompanyWorkOrderLaborCostReview Basis;
        internal Fixture()
        {
            var start=WorkOrderExecutionStartFixture.Original(true).Command;var started=start.Prepare(DateTime.UtcNow);var requirements=start.Review.Prerequisites.Requirements;
            var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(new(start,started),requirements,DateTime.UtcNow);
            var laborReview=new CompanyWorkOrderLaborUsageReview(comparison,start.Review.Stage,start.Review.Prerequisites.Personnel,[],null,DateTime.UtcNow);
            var from=started.STARTED_AT_UTC;var to=laborReview.ComparedAtUtc;var laborCommand=new CompanyWorkOrderLaborUsageCommand(laborReview,Guid.NewGuid(),from,to,(decimal)(to.Ticks-from.Ticks)/TimeSpan.TicksPerSecond,WorkOrderLaborTimeUnit.Second,"Actual reported labor","Original executor report");Labor=laborCommand.Prepare(DateTime.UtcNow);
            var context=requirements.Context with{ActorUserId="budget-maker",Revision="budget-maker-authority"};var database=new AssetDatabaseScope("production","production-binding");
            var book=new CompanyLedgerBookFacts(Guid.NewGuid().ToString("N"),"book","Recorded company book","KWD",3,"TO_EVEN","calendar","UTC","book-maker",DateTime.UtcNow,new string('A',64));
            var query=new CompanyWorkOrderBudgetQuery(requirements.Order.InstanceId,requirements.Order.FieldId,book.BookId);var budgetReview=new CompanyWorkOrderBudgetReview(context,database,query,requirements.Order,book,[],DateTime.UtcNow);
            var budget=new CompanyWorkOrderBudgetCommand(budgetReview,Guid.NewGuid(),"ExplicitAFE","Recorded labor budget",100m,from.AddDays(-1),"Original budget basis");Draft=budget.Prepare(DateTime.UtcNow);
            var current=budgetReview with{Context=context with{ActorUserId="budget-reviewer",Revision="independent-authority"},Budgets=[CompanyWorkOrderBudgetCommand.Facts(Draft)],ComparedAtUtc=DateTime.UtcNow};
            var decisionReview=new CompanyWorkOrderBudgetDecisionReview(current,new(query,Draft.AFE_ID));var decision=new CompanyWorkOrderBudgetDecisionCommand(decisionReview,Guid.NewGuid(),WorkOrderBudgetDecision.Approve,"Independent recorded approval");Approved=decision.Prepare(DateTime.UtcNow);
            current=current with{Context=context with{ActorUserId="cost-recorder",Revision="cost-authority"},ComparedAtUtc=DateTime.UtcNow};
            var fact=CompanyWorkOrderBudgetDecisionFacts.From(Approved);var status=new CompanyWorkOrderBudgetDecisionStatus(current,decisionReview.Query,fact,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,decisionReview.Query,fact),DateTime.UtcNow);
            Basis=new(status,new(status.Query,Labor.LABOR_ID),CompanyWorkOrderLaborUsageFacts.From(Labor),[],DateTime.UtcNow);Basis.Require();
            Native.Rows.Clear();Native.Rows.AddRange([Draft,Approved,Labor]);var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
            var types=new Dictionary<string,Type>{{"AFE",typeof(AFE)},{"APPROVAL_WORKFLOW",typeof(APPROVAL_WORKFLOW)},{"ACCOUNTING_COST",typeof(ACCOUNTING_COST)},{"WORK_ORDER_LABOR_USAGE",typeof(WORK_ORDER_LABOR_USAGE)}};
            Native.Source.Setup(value=>value.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>types.ContainsKey(table));Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure shape,bool _)=>creator.ConvertToEntityStructure(types[shape.EntityName]));
            Native.Source.Setup(value=>value.InsertEntity("ACCOUNTING_COST",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;Native.Rows.Add(CompanyInventorySourceReview.Copy((ACCOUNTING_COST)row));return new ErrorsInfo{Flag=Errors.Ok};});
            var metadata=new Mock<IPPDMMetadataRepository>();foreach(var (table,type) in types){var key=type.GetProperties().Single(property=>property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true)).Name;metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);}
            Owner=new(Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
        internal Task Guard()=>Task.CompletedTask;
        internal Task<CompanyWorkOrderLaborCostReview> Review()=>Owner.ReviewWorkOrderLaborCostAsync(Basis.Budget,Basis.Query,Basis.Labor,Guard);
        internal CompanyWorkOrderLaborCostCommand Command(CompanyWorkOrderLaborCostReview review)=>new(review,Guid.NewGuid(),1m,review.Labor.Unit,review.Budget.Current.Book.FunctionalCurrency,review.ComparedAtUtc,"Recorded-labor-category","Explicit labor rate and valuation source");
        internal Task<CompanyWorkOrderLaborCostRecovery> Record(CompanyWorkOrderLaborCostCommand command)=>Owner.RecordWorkOrderLaborCostAsync(command,Guard,Review);
    }
    [Fact] public async Task NativeLaborValuationRecordsExistingCostOnceWithActualQuantityRateCurrencyAndNoApprovalOrStockMutation()
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());var draft=JsonSerializer.Serialize(fixture.Draft);var approval=JsonSerializer.Serialize(fixture.Approved);var labor=JsonSerializer.Serialize(fixture.Labor);
        var result=await fixture.Record(command);result.Require(command.Review.Budget.Current.Context.Company,command.Review.Budget.Current.Context.ActorUserId,command.Reference());Assert.Equal(1,fixture.Native.Writes);Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);
        Assert.Equal(command.Amount(),result.Recorded.AMOUNT);Assert.Equal(command.Review.Labor.Quantity,result.Recorded.SOURCE_QUANTITY);Assert.Equal(command.Rate,result.Recorded.UNIT_RATE);Assert.Equal("KWD",result.Recorded.CURRENCY_CODE);Assert.Null(result.Recorded.FINANCE_ID);
        Assert.Equal(draft,JsonSerializer.Serialize(fixture.Draft));Assert.Equal(approval,JsonSerializer.Serialize(fixture.Approved));Assert.Equal(labor,JsonSerializer.Serialize(fixture.Labor));Assert.Null(fixture.Draft.ACTUAL_COST);
        var next=await fixture.Review();Assert.Single(next.Costs);await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record(fixture.Command(next)));Assert.Equal(1,fixture.Native.Writes);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await fixture.Record(command)));Assert.Equal(1,fixture.Native.Writes);
        fixture.Native.Rows.Remove(fixture.Labor);fixture.Native.Rows.Remove(fixture.Approved);fixture.Native.Rows.Remove(fixture.Draft);
        var recovered=await fixture.Owner.RecoverWorkOrderLaborCostAsync(command.Review.Budget.Current.Context,command.Review.Budget.Current.BudgetDatabase,command.Reference(),fixture.Guard);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("rate")][InlineData("unit")][InlineData("currency")][InlineData("future")][InlineData("approval")][InlineData("source")][InlineData("schema")][InlineData("budget")]
    public async Task ValuationCannotInferRateUnitCurrencyDateOrUnprovenApprovedSource(string fault)
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());
        if(fault=="rate")command=command with{Rate=-1};if(fault=="unit")command=command with{RateUnit=WorkOrderLaborTimeUnit.Hour};if(fault=="currency")command=command with{CurrencyCode="USD"};
        if(fault=="future")command=command with{ValuedAtUtc=command.Review.ComparedAtUtc.AddSeconds(1)};if(fault=="approval")fixture.Approved.STATUS="REJECTED";if(fault=="source")fixture.Labor.QUANTITY+=1;
        if(fault=="schema")fixture.Native.Source.Setup(value=>value.CheckEntityExist("ACCOUNTING_COST")).Returns(false);
        if(fault=="budget")command=command with{Rate=1000m/command.Review.Labor.Quantity};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record(command));Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Rows.OfType<ACCOUNTING_COST>());
    }
    [Fact] public async Task LostAcknowledgementKeepsOneActualCostAndReadOnlyRecoveryDoesNotRevalueLabor()
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());fixture.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record(command));Assert.Single(fixture.Native.Rows.OfType<ACCOUNTING_COST>());Assert.Equal(1,fixture.Native.Writes);
        var result=await fixture.Owner.RecoverWorkOrderLaborCostAsync(command.Review.Budget.Current.Context,command.Review.Budget.Current.BudgetDatabase,command.Reference(),fixture.Guard);
        command.RequireRecorded(result.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task UnownedLegacyCostForOriginalAfeCannotBecomeKnownEmptyActualTotal()
    {
        var fixture=new Fixture();fixture.Native.Rows.Add(new ACCOUNTING_COST{ACCOUNTING_COST_ID="legacy-cost",AFE_ID=fixture.Draft.AFE_ID,AMOUNT=90});
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Review());Assert.Equal(0,fixture.Native.Writes);
    }
    [Fact] public async Task FinancialLaborReaderReturnsOriginalQuantityWithoutExecutorPrivateWorkReportOrIntent()
    {
        var fixture=new Fixture();var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_LABOR_USAGE")).ReturnsAsync(["LABOR_ID"]);
        var owner=new Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderExecutionPermitService(fixture.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        var fact=await owner.ReadLaborFactsAsync(fixture.Basis.Budget.Current.Context,fixture.Basis.Labor.WorkOrderId,fixture.Basis.Labor.LaborId,fixture.Guard);
        Assert.Equal(fixture.Basis.Labor,fact);Assert.DoesNotContain("Actual reported labor",JsonSerializer.Serialize(fact));Assert.DoesNotContain("Original executor report",JsonSerializer.Serialize(fact));Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(fact));Assert.Equal(0,fixture.Native.Writes);
    }
    [Theory][InlineData("Rate")][InlineData("RateUnit")][InlineData("CurrencyCode")][InlineData("ValuedAtUtc")]
    public async Task MissingFinancialJsonFactCannotBecomeAnImplicitZeroRateOrUnitDate(string property)
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());var json=System.Text.Json.Nodes.JsonNode.Parse(JsonSerializer.Serialize(command))!.AsObject();Assert.True(json.Remove(property));
        Assert.Throws<JsonException>(()=>JsonSerializer.Deserialize<CompanyWorkOrderLaborCostCommand>(json.ToJsonString()));Assert.Equal(0,fixture.Native.Writes);
    }
    [Fact] public async Task ExplicitBookRoundingIsAppliedToActualQuantityTimesRateWithoutConvertingUnits()
    {
        var fixture=new Fixture();var review=await fixture.Review();var rate=1m/review.Labor.Quantity;var command=fixture.Command(review) with{Rate=rate};
        Assert.Equal(decimal.Round(review.Labor.Quantity*rate,3,MidpointRounding.ToEven),command.Amount());Assert.Equal(review.Labor.Unit,command.RateUnit);
        var invalid=command with{RateUnit=WorkOrderLaborTimeUnit.Minute};Assert.Throws<ArgumentException>(()=>invalid.Require());Assert.Equal(0,fixture.Native.Writes);
    }
}
#endif
