#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using Xunit;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderBudgetTests
{
    [Fact] public async Task PublicBudgetDiscoveryUsesOriginalBookAfeAndCompleteKnownEmptyMembershipWithoutPrivateIntent()
    {
        var f=new Fixture();var query=new CompanyWorkOrderFinancialQuery(f.Basis.Query.WorkOrderId,f.Basis.Query.FieldId);
        Assert.Empty(await f.Owner.ReadWorkOrderBudgetFactsAsync(f.Basis.Context,f.Basis.BudgetDatabase,query,f.Guard));
        var command=f.Command(await f.Review());var result=await f.Create(command);
        var facts=await f.Owner.ReadWorkOrderBudgetFactsAsync(f.Basis.Context,f.Basis.BudgetDatabase,query,f.Guard);Assert.Equal(CompanyWorkOrderBudgetCommand.Facts(result.Recorded),Assert.Single(facts));
        Assert.Equal(command.Review.Query.BookId,facts[0].BookId);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(facts));Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(facts));Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("schema")][InlineData("authority")][InlineData("binding")][InlineData("changed")][InlineData("foreign")]
    public async Task PublicBudgetDiscoveryCannotPublishPartialOrUnprovenOriginalPopulation(string fault)
    {
        var f=new Fixture();var result=await f.Create(f.Command(await f.Review()));var query=new CompanyWorkOrderFinancialQuery(f.Basis.Query.WorkOrderId,f.Basis.Query.FieldId);
        if(fault=="schema")f.Native.Source.Setup(value=>value.CheckEntityExist("AFE")).Returns(false);
        if(fault=="foreign")
        {
            var foreign=CompanyInventorySourceReview.Copy(result.Recorded);foreign.OWNER_COMPANY_BA_ID="foreign";foreign.WORK_ORDER_BUDGET_JSON="malformed private proof";
            f.Native.Source.Setup(value=>value.GetEntityAsync("AFE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new[]{foreign});
        }
        var calls=0;Task Guard(){if(fault=="authority")throw new UnauthorizedAccessException("Revoked viewer");if(fault=="changed" && ++calls==3)f.Native.Rows.OfType<AFE>().Single().ESTIMATED_COST=999;return Task.CompletedTask;}
        var database=fault=="binding"?f.Basis.BudgetDatabase with{Fingerprint="different"}:f.Basis.BudgetDatabase;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadWorkOrderBudgetFactsAsync(f.Basis.Context,database,query,Guard));Assert.Equal(1,f.Native.Writes);
    }
    private sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly AfeService Owner;
        internal readonly CompanyWorkOrderBudgetReview Basis;
        internal Fixture()
        {
            var start=WorkOrderExecutionStartFixture.Original(true);var requirements=start.Command.Review.Prerequisites.Requirements;
            var book=new CompanyLedgerBookFacts(Guid.NewGuid().ToString("N"),"configured-book","Recorded book","KWD",3,"TO_EVEN","calendar","UTC","book-maker",DateTime.UtcNow,new string('A',64));
            Basis=new(requirements.Context,new("production","native-production-binding"),new(requirements.Order.InstanceId,requirements.Order.FieldId,book.BookId),requirements.Order,book,[],DateTime.UtcNow);Basis.Require();
            Native.Rows.Clear();var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
            Native.Source.Setup(value=>value.CheckEntityExist("AFE")).Returns(true);Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(AFE)));
            Native.Source.Setup(value=>value.InsertEntity("AFE",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;Native.Rows.Add(CompanyInventorySourceReview.Copy((AFE)row));return new ErrorsInfo{Flag=Errors.Ok};});
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("AFE")).ReturnsAsync(["AFE_ID"]);
            metadata.Setup(value=>value.GetTableMetadataAsync("AFE")).ReturnsAsync(new PPDMTableMetadata{TableName="AFE",EntityTypeName=typeof(AFE).FullName!,PrimaryKeyColumn="AFE_ID",Module="PRODUCTION"});
            Owner=new(Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
        internal Task Guard()=>Task.CompletedTask;
        internal Task<CompanyWorkOrderBudgetReview> Review()=>Owner.ReviewWorkOrderBudgetAsync(Basis,Guard);
        internal CompanyWorkOrderBudgetCommand Command(CompanyWorkOrderBudgetReview review)=>new(review,Guid.NewGuid(),"AFE-work-1","Explicit work-order budget",12.875m,DateTime.UtcNow,"Recorded budget basis");
        internal Task<CompanyWorkOrderBudgetRecovery> Create(CompanyWorkOrderBudgetCommand command)=>Owner.CreateWorkOrderBudgetAsync(command,Guard,Review);
    }
    [Fact] public async Task NativeBudgetUsesExistingAfeOnceAndKeepsOriginalRecoveryAndRetryWithoutApprovalOrActualCost()
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());var result=await fixture.Create(command);
        Assert.Equal(1,fixture.Native.Writes);Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);Assert.Equal("DRAFT",result.Recorded.STATUS);
        Assert.Equal("KWD",result.Recorded.CURRENCY_CODE);Assert.Equal(command.BudgetAmount,result.Recorded.ESTIMATED_COST);Assert.Null(result.Recorded.ACTUAL_COST);Assert.Null(result.Recorded.APPROVAL_DATE);
        Assert.Equal(command.Review.Query.WorkOrderId,result.Recorded.WORK_ORDER_ID);Assert.Equal(command.Review.Book.BookId,result.Recorded.BOOK_ID);
        var closing=await fixture.Review();Assert.Single(closing.Budgets);Assert.Throws<ArgumentException>(()=>fixture.Command(closing).Require());
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await fixture.Create(command)));Assert.Equal(1,fixture.Native.Writes);
        var recovered=await fixture.Owner.RecoverWorkOrderBudgetAsync(command.Review.Context,command.Review.BudgetDatabase,new(command.Review.Query,command.IntentId),fixture.Guard);
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("precision")][InlineData("negative")][InlineData("zone")][InlineData("book")][InlineData("field")][InlineData("source")]
    public async Task BudgetCannotInventCurrencyRoundingEffectiveDateOrOriginalNativeOrder(string fault)
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());command=fault switch{
            "precision"=>command with{BudgetAmount=12.8751m},"negative"=>command with{BudgetAmount=-1},"zone"=>command with{EffectiveAtUtc=DateTime.SpecifyKind(DateTime.UtcNow,DateTimeKind.Unspecified)},
            "book"=>command with{Review=command.Review with{Book=command.Review.Book with{BookId="other"}}},
            "field"=>command with{Review=command.Review with{Query=command.Review.Query with{FieldId="other"}}},
            _=>command with{Review=command.Review with{Order=CompanyInventorySourceReview.Copy(command.Review.Order)}}};
        if(fault=="source")command.Review.Order.Source="PROJECT";
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Create(command));Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Fact] public async Task LostAcknowledgementRetainsExactlyOneAfeAndHistoricalRecoveryMakesNoReplacement()
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());fixture.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Create(command));Assert.Single(fixture.Native.Rows.OfType<AFE>());Assert.Equal(1,fixture.Native.Writes);
        var result=await fixture.Owner.RecoverWorkOrderBudgetAsync(command.Review.Context,command.Review.BudgetDatabase,new(command.Review.Query,command.IntentId),fixture.Guard);
        command.RequireRecorded(result.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("direct-create")][InlineData("approval")][InlineData("cost")][InlineData("line")]
    public async Task LegacyAutomaticOperationsCannotOverwriteOwnedBudgetOrInventItsActuals(string operation)
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());var result=await fixture.Create(command);var before=JsonSerializer.Serialize(result.Recorded);
        Task Attempt()=>operation switch{
            "direct-create"=>fixture.Owner.CreateAfeAsync(CompanyInventorySourceReview.Copy(result.Recorded),command.Review.Context.ActorUserId,command.Review.BudgetDatabase.ConnectionName),
            "approval"=>fixture.Owner.ApproveAfeAsync(result.Recorded.AFE_ID,DateTime.UtcNow,command.Review.Context.ActorUserId,command.Review.BudgetDatabase.ConnectionName),
            "cost"=>fixture.Owner.RecordCostAsync(result.Recorded.AFE_ID,new(){AMOUNT=1},command.Review.Context.ActorUserId,command.Review.BudgetDatabase.ConnectionName),
            _=>fixture.Owner.AddLineItemAsync(new(){AFE_ID=result.Recorded.AFE_ID,BUDGET_AMOUNT=1},command.Review.Context.ActorUserId,command.Review.BudgetDatabase.ConnectionName)};
        await Assert.ThrowsAsync<InvalidOperationException>(Attempt);Assert.Equal(1,fixture.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(fixture.Native.Rows.OfType<AFE>().Single()));
    }
    [Fact] public void FinancialMetadataKeepsRecognizedRelationshipsAndRefusesForeignOwnerBeforePartialMutation()
    {
        var relation=new PPDMForeignKey{ForeignKeyColumn="FINANCE_ID",ReferencedTable="FINANCE",ReferencedPrimaryKey="FINANCE_ID"};
        var metadata=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"ACCOUNTING_COST",new(){TableName="accounting_cost",EntityTypeName="ACCOUNTING_COST",PrimaryKeyColumn="ACCOUNTING_COST_ID",Module="Accounting",ForeignKeys=[relation]}}};
        Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.WorkOrderFinancialMetadata.AddTo(metadata);
        Assert.Equal("PRODUCTION",metadata["ACCOUNTING_COST"].Module);Assert.Same(relation,Assert.Single(metadata["ACCOUNTING_COST"].ForeignKeys));Assert.Equal(typeof(AFE).FullName,metadata["AFE"].EntityTypeName);
        var foreign=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"ACCOUNTING_COST",new(){TableName="ACCOUNTING_COST",EntityTypeName="foreign.Entity",PrimaryKeyColumn="ACCOUNTING_COST_ID",Module="Accounting"}}};
        Assert.Throws<InvalidOperationException>(()=>Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.WorkOrderFinancialMetadata.AddTo(foreign));Assert.Single(foreign);Assert.False(foreign.ContainsKey("AFE"));
    }
    [Theory][InlineData("borrowed")][InlineData("schema")][InlineData("book-drift")]
    public async Task UnownedSchemaOrChangedBookRefusesBeforeAnyAfeInsert(string fault)
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());
        if(fault=="borrowed")fixture.Native.Borrowed=true;
        if(fault=="schema")fixture.Native.Source.Setup(value=>value.CheckEntityExist("AFE")).Returns(false);
        var error=await Record.ExceptionAsync(()=>fixture.Owner.CreateWorkOrderBudgetAsync(command,fixture.Guard,
            async()=>{var current=await fixture.Review();return fault=="book-drift"?current with{Book=current.Book with{NativeRevision=new string('B',64)}}:current;}));
        Assert.NotNull(error);Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Fact] public async Task SeparateCoreDriftAfterCommitLeavesOriginalBudgetRecoverableAndDoesNotPretendCrossProviderAtomicity()
    {
        var fixture=new Fixture();var command=fixture.Command(await fixture.Review());fixture.Native.AfterCommit=()=>fixture.Basis.Order.Instructions="Changed native work scope";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Create(command));Assert.Equal(1,fixture.Native.Writes);Assert.Single(fixture.Native.Rows.OfType<AFE>());
        var result=await fixture.Owner.RecoverWorkOrderBudgetAsync(command.Review.Context,command.Review.BudgetDatabase,new(command.Review.Query,command.IntentId),fixture.Guard);
        command.RequireRecorded(result.Recorded);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task BudgetBookReaderContractUsesActualNativeConfiguredBookProofWithoutInventingCurrency()
    {
        var fixture=new Fixture();var command=new CompanyLedgerBookConfigurationCommand(Guid.NewGuid().ToString("N"),fixture.Basis.Context.Company,"book-maker",Guid.NewGuid().ToString("N"),"native-book","Recorded company book","KWD",3,"TO_EVEN","calendar","UTC","Reviewed book",new string('A',64),new string('B',64));
        var row=command.CreateRow();row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CHANGED_BY=command.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.ROW_CHANGED_DATE=row.ROW_CREATED_DATE;row.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(command,row);
        fixture.Native.Rows.Add(row);var creator=new ClassCreator(fixture.Native.Editor.Object);
        fixture.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="COMPANY_LEDGER_BOOK"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(COMPANY_LEDGER_BOOK)));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("COMPANY_LEDGER_BOOK")).ReturnsAsync(["COMPANY_LEDGER_BOOK_ID"]);
        ICompanyLedgerBookReader owner=new Beep.OilandGas.Accounting.Services.GLAccountService(fixture.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Accounting.Services.GLAccountService>.Instance,()=>Task.FromResult("production"));
        var result=await owner.GetCompanyBooksAsync(command.Company,fixture.Basis.Context.ActorUserId,fixture.Guard);result.Require(command.Company,fixture.Basis.Context.ActorUserId);
        var book=Assert.Single(result.Books);Assert.Equal(command.FunctionalCurrency,book.FunctionalCurrency);Assert.Equal(3,book.CurrencyDecimals);Assert.Equal(command.Id,book.Id);Assert.Equal(0,fixture.Native.Writes);
        row.FUNCTIONAL_CURRENCY="USD";await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.GetCompanyBooksAsync(command.Company,fixture.Basis.Context.ActorUserId,fixture.Guard));Assert.Equal(0,fixture.Native.Writes);
    }
}
#endif
