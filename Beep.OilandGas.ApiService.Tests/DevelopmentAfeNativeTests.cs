#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentAfeNativeTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentPlanBaselineNativeTests.Fixture Planning=new();
        internal PersonaTaskRoutingTests.Fixture Native=new();
        internal AfeService Owner;
        internal GLAccountService Books;
        internal IPPDMMetadataRepository Metadata=null!;
        internal DevelopmentAfeReview Basis=null!;
        internal bool FailInsert,CorruptInsert,Revoke;
        internal Action? OnInsert;
        internal Fixture()
        {
            Native.Connection="production";Native.Rows.Clear();Native.Source.SetupGet(source=>source.DatasourceName).Returns("production");
            var configured=new ConnectionProperties{ConnectionName="production",DatabaseType=DataSourceType.SqlLite,ConnectionString="Data Source="+Path.Combine(Path.GetTempPath(),"development-afe-native-fixture.db")};
            var config=new Mock<IConfigEditor>();config.SetupGet(value=>value.DataConnections).Returns(new List<ConnectionProperties>{configured});Native.Editor.SetupGet(editor=>editor.ConfigEditor).Returns(config.Object);
            Mock.Get(Native.Source.Object.Dataconnection).SetupGet(value=>value.ConnectionProp).Returns(configured);
            var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(editor=>editor.classCreator).Returns(creator);
            Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns(true);
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>creator.ConvertToEntityStructure(request.EntityName=="AFE"?typeof(AFE):request.EntityName=="APPROVAL_WORKFLOW"?typeof(APPROVAL_WORKFLOW):typeof(COMPANY_LEDGER_BOOK)));
            Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
            Native.Source.Setup(source=>source.InsertEntity("AFE",It.IsAny<object>())).Returns((string table,object value)=>{
                Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((AFE)value);if(CorruptInsert)row.ESTIMATED_COST=999m;Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var columns=Mock.Of<ICommonColumnHandler>();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadataMock=new Mock<IPPDMMetadataRepository>();metadataMock.Setup(value=>value.GetPrimaryKeyColumnsAsync("AFE")).ReturnsAsync(["AFE_ID"]);metadataMock.Setup(value=>value.GetTableMetadataAsync("AFE")).ReturnsAsync(new PPDMTableMetadata{TableName="AFE",EntityTypeName=typeof(AFE).FullName!,PrimaryKeyColumn="AFE_ID",Module="PRODUCTION"});var metadata=metadataMock.Object;Metadata=metadata;
            Owner=new(Native.Editor.Object,columns,defaults,metadata);Books=new(Native.Editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance,()=>Task.FromResult("production"));
        }
        internal async Task Prepare()
        {
            await Planning.Prepare();var baseline=(await Planning.Approve()).Result;
            var book=new CompanyLedgerBookTests.Fixture();await book.Prepare();Native.Rows.Add(await book.Record());
            var books=await Books.GetCompanyBooksAsync(baseline.Review.Scope.Company,baseline.Review.Scope.ActorUserId,Guard);
            Basis=new(baseline,new("production","production-binding"),books,[],"",DateTimeOffset.UtcNow);Basis=Basis with{Revision=Basis.ContentRevision()};Basis.Require();
        }
        internal Task Guard(){if(Revoke)throw new UnauthorizedAccessException("Funding request authority revoked");return Task.CompletedTask;}
        internal async Task<DevelopmentAfeReview> Read()
        {
            Assert.DoesNotContain("begin",Native.Transactions.TakeLast(1));var books=await Books.GetCompanyBooksAsync(Basis.Source.Review.Scope.Company,Basis.Source.Review.Scope.ActorUserId,Guard);
            var current=Basis with{Books=books,Afes=[],CheckedAtUtc=DateTimeOffset.UtcNow};current=current with{Revision=current.ContentRevision()};return await Owner.ReviewDevelopmentAfesAsync(current,Guard);
        }
        internal async Task<DevelopmentAfeCommand> Command()=>new(await Read(),Guid.NewGuid(),"statutory","AFE-2026-1","Recorded FDP funding request",1000.125m,new(2027,2,3),"Explicit company-book budget basis; no exchange conversion inferred");
        internal Task<DevelopmentAfeReceipt> Record(DevelopmentAfeCommand command,bool recover=false)=>Owner.RecordDevelopmentAfeAsync(command,Guard,Read,recover);
        public void Dispose()=>Planning.Dispose();
    }
    [Fact] public async Task ApprovedBaselineAndConfiguredBookProduceOneDraftAfeWithExactCurrencyCalendarAndAudit()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var receipt=await f.Record(command);receipt.Require(command);var row=receipt.Recorded!;
        Assert.Equal("DRAFT",row.STATUS);Assert.Equal(1000.125m,row.ESTIMATED_COST);Assert.Equal("KWD",row.CURRENCY_CODE);Assert.Null(row.ACTUAL_COST);Assert.Null(row.APPROVAL_DATE);Assert.Null(row.WORK_ORDER_ID);
        Assert.Equal(command.Review.Source.Baseline!.FDP_ID,row.FDP_BASELINE_ID);Assert.Equal(new DateTime(2027,2,3),row.ROW_EFFECTIVE_DATE);Assert.Equal(1,f.Native.Writes);
        var facts=Assert.Single((await f.Read()).Afes);Assert.Equal(3,facts.CurrencyDecimals);Assert.Equal("DRAFT",facts.Status);Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(facts));
        await f.Record(command);await f.Record(command,true);Assert.Equal(1,f.Native.Writes);
    }
    [Fact] public async Task RecoveryChecksAbsenceAndCommitLossWithoutAnotherDraftOrChangedOriginal()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();Assert.Null((await f.Record(command,true)).Recorded);Assert.Equal(0,f.Native.Writes);
        f.OnInsert=()=>f.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));f.Native.FailCommit=false;f.OnInsert=null;
        Assert.NotNull((await f.Record(command,true)).Recorded);Assert.Equal(1,f.Native.Writes);await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command with{IntentId=Guid.NewGuid()}));Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("book")]
    public async Task NativeWriteBoundaryRollsBackCompleteRequestAndBookPopulation(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var before=JsonSerializer.Serialize(f.Native.Rows);f.FailInsert=fault=="insert";f.CorruptInsert=fault=="readback";
        f.OnInsert=()=>{if(fault=="authority")f.Revoke=true;if(fault=="book")f.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single().FUNCTIONAL_CURRENCY="USD";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal("rollback",f.Native.Transactions.Last());
    }
    [Theory][InlineData("schema")][InlineData("book")][InlineData("precision")][InlineData("date")][InlineData("baseline")]
    public async Task UnqualifiedSourceOrInputsCannotProduceAfe(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();
        if(fault=="schema")f.Native.Source.Setup(source=>source.CheckEntityExist("AFE")).Returns(false);
        if(fault=="book")command=command with{BookId="unknown"};if(fault=="precision")command=command with{Amount=1.1234m};if(fault=="date")command=command with{EffectiveDate=default};
        if(fault=="baseline")command.Review.Source.Baseline!.APPROVED_BY="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("foreign")][InlineData("inactive")][InlineData("amount")][InlineData("duplicate")]
    public async Task ChangedOriginalCannotBecomeCheckedAbsenceOrBeRepaired(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();await f.Record(command);var row=f.Native.Rows.OfType<AFE>().Single();
        if(fault=="foreign")row.OWNER_COMPANY_BA_ID="foreign";if(fault=="inactive")row.ACTIVE_IND="N";if(fault=="amount")row.ESTIMATED_COST=999;if(fault=="duplicate")f.Native.Rows.Add(row);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command,true));await Assert.ThrowsAnyAsync<Exception>(async()=>await f.Read());Assert.Equal(1,f.Native.Writes);
    }
    [Fact] public async Task FullNativeNameCapacitySurvivesRequestReadbackAndPublicFactsWithoutTruncation()
    {
        using var f=new Fixture();await f.Prepare();var command=(await f.Command()) with{Name=new string('ض',256),Number=new string('N',128)};
        Assert.Throws<InvalidOperationException>(()=>(command with{Name=command.Name+"x"}).Require());
        var result=await f.Record(command);Assert.Equal(command.Name,result.Recorded!.AFE_NAME);var facts=Assert.Single((await f.Read()).Afes);Assert.Equal(command.Name,facts.Name);Assert.Equal(command.Number,facts.Number);Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("create")][InlineData("approve")][InlineData("authorization")][InlineData("cost")][InlineData("line")]
    public async Task UnqualifiedMutatorsCannotAlterDevelopmentOwnershipApprovalLinesOrActuals(string operation)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var receipt=await f.Record(command);var before=JsonSerializer.Serialize(f.Native.Rows);var row=receipt.Recorded!;var actor=command.Review.Source.Review.Scope.ActorUserId;
        Task Attempt()=>operation switch{
            "create"=>f.Owner.CreateAfeAsync(CompanyInventorySourceReview.Copy(row),actor,"production"),
            "approve"=>f.Owner.ApproveAfeAsync(row.AFE_ID,DateTime.UtcNow,actor,"production"),
            "authorization"=>new AuthorizationWorkflowService(f.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),f.Metadata).ApproveAfeAsync(row.AFE_ID,DateTime.UtcNow,actor,"production"),
            "cost"=>f.Owner.RecordCostAsync(row.AFE_ID,new(){AMOUNT=1},actor,"production"),
            _=>f.Owner.AddLineItemAsync(new(){AFE_ID=row.AFE_ID,BUDGET_AMOUNT=1},actor,"production")};
        await Assert.ThrowsAsync<InvalidOperationException>(Attempt);Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(1,f.Native.Writes);
    }

}
#endif
