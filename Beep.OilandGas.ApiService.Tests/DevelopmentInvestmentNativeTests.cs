#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentAfeNativeTests.Fixture Afe=new();internal AuthorizationWorkflowService Owner;internal DevelopmentAfeReview Basis=null!;
        internal bool FailInsert,CorruptInsert;internal Action? OnInsert;
        internal Fixture()
        {
            Owner=new(Afe.Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<Beep.OilandGas.PPDM39.Core.IPPDM39DefaultsRepository>(),Afe.Metadata);
            Afe.Native.Source.Setup(source=>source.InsertEntity("APPROVAL_WORKFLOW",It.IsAny<object>())).Returns((string table,object value)=>{
                Afe.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((APPROVAL_WORKFLOW)value);if(CorruptInsert)row.AMOUNT+=1;Afe.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        internal async Task Prepare()
        {
            await Afe.Prepare();var request=await Afe.Command();await Afe.Record(request);var current=await Afe.Read();var progress=current.Source.Review with{Scope=current.Source.Review.Scope with{ActorUserId="investment-reviewer"}};progress=progress with{Revision=progress.ContentRevision()};
            Basis=current with{Source=current.Source with{Review=progress},Books=current.Books.AsReader("investment-reviewer")};Basis=Basis with{Revision=Basis.ContentRevision()};Basis.Require();
        }
        internal Task<DevelopmentInvestmentReview> Read()=>Owner.ReviewDevelopmentInvestmentAsync(Basis,Basis.Afes.Single().AfeId,Afe.Owner,Afe.Guard);
        internal async Task<DevelopmentInvestmentCommand> Command(string decision="APPROVE")=>new(await Read(),Guid.NewGuid(),decision,"Explicit independent company investment rationale");
        internal Task<DevelopmentInvestmentReceipt> Record(DevelopmentInvestmentCommand original,bool recover=false)=>Owner.RecordDevelopmentInvestmentAsync(original,Afe.Owner,Afe.Guard,Read,recover);
        public void Dispose()=>Afe.Dispose();
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task IndependentNativeDecisionPreservesDraftRequestAndExactMoneyAndRecordsActualReviewerOnce(string decision)
    {
        using var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Afe.Native.Rows.OfType<AFE>().Single());var command=await f.Command(decision);var result=await f.Record(command);result.Require(command);
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",result.Recorded!.STATUS);Assert.Equal("investment-reviewer",result.Recorded.APPROVED_BY);Assert.Equal(1000.125m,result.Recorded.AMOUNT);Assert.Equal("KWD",result.Recorded.CURRENCY_CODE);
        Assert.Equal(before,JsonSerializer.Serialize(f.Afe.Native.Rows.OfType<AFE>().Single()));Assert.Equal(2,f.Afe.Native.Writes);await f.Record(command);await f.Record(command,true);Assert.Equal(2,f.Afe.Native.Writes);
        var current=await f.Read();Assert.Equal(decision,current.Recorded!.Decision);Assert.DoesNotContain(command.IntentId.ToString(),JsonSerializer.Serialize(current.Recorded));
    }
    [Fact] public async Task OriginalRecoveryChecksAbsenceAndLostCommitWithoutSecondOrChangedDecision()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();Assert.Null((await f.Record(command,true)).Recorded);Assert.Equal(1,f.Afe.Native.Writes);
        f.OnInsert=()=>f.Afe.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));f.Afe.Native.FailCommit=false;f.OnInsert=null;
        Assert.NotNull((await f.Record(command,true)).Recorded);Assert.Equal(2,f.Afe.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command with{IntentId=Guid.NewGuid()},true));Assert.Equal(2,f.Afe.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("book")][InlineData("request")]
    public async Task NativeDecisionBoundaryRollsBackWithoutMutatingOriginalRequestOrLeavingPartialDecision(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var before=JsonSerializer.Serialize(f.Afe.Native.Rows);f.FailInsert=fault=="insert";f.CorruptInsert=fault=="readback";
        f.OnInsert=()=>{if(fault=="authority")f.Afe.Revoke=true;if(fault=="book")f.Afe.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single().FUNCTIONAL_CURRENCY="USD";if(fault=="request")f.Afe.Native.Rows.OfType<AFE>().Single().ESTIMATED_COST=3;};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command));Assert.Equal(before,JsonSerializer.Serialize(f.Afe.Native.Rows));Assert.Equal("rollback",f.Afe.Native.Transactions.Last());
    }
    [Theory][InlineData("inactive")][InlineData("foreign")][InlineData("retagged")][InlineData("duplicate")][InlineData("amount")]
    public async Task AlteredDecisionNeverBecomesCheckedAbsenceOrSilentRepair(string fault)
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();await f.Record(command);var row=f.Afe.Native.Rows.OfType<APPROVAL_WORKFLOW>().Single();
        if(fault=="inactive")row.ACTIVE_IND="N";if(fault=="foreign")row.OWNER_COMPANY_BA_ID="foreign";if(fault=="retagged")row.ENTITY_ID="another";if(fault=="duplicate")f.Afe.Native.Rows.Add(row);if(fault=="amount")row.AMOUNT=0;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(command,true));Assert.Equal(2,f.Afe.Native.Writes);
    }
    [Fact] public async Task MakerCannotDecideAndChangedBookBlocksNewDecisionButNotRecordedReadOrOriginalRecovery()
    {
        using var f=new Fixture();await f.Prepare();var command=await f.Command();var progress=f.Basis.Source.Review with{Scope=f.Basis.Source.Review.Scope with{ActorUserId="reviewer"}};progress=progress with{Revision=progress.ContentRevision()};
        var maker=f.Basis with{Source=f.Basis.Source with{Review=progress},Books=f.Basis.Books.AsReader("reviewer")};maker=maker with{Revision=maker.ContentRevision()};
        var current=await f.Owner.ReviewDevelopmentInvestmentAsync(maker,command.Review.AfeId,f.Afe.Owner,f.Afe.Guard);Assert.Contains(current.Blockers,text=>text.Contains("maker"));Assert.Throws<InvalidOperationException>(()=>new DevelopmentInvestmentCommand(current,Guid.NewGuid(),"APPROVE","Attempted self approval").Require());
        await f.Record(command);f.Basis=f.Basis with{Books=CompanyLedgerBookReview.Create(f.Basis.Books.Company,f.Basis.Books.ActorUserId,[])};f.Basis=f.Basis with{Revision=f.Basis.ContentRevision()};
        Assert.NotNull((await f.Read()).Recorded);Assert.NotNull((await f.Record(command,true)).Recorded);Assert.Equal(2,f.Afe.Native.Writes);
    }
    [Fact] public async Task UnexpectedCompanionAfeDuringDecisionInsertRollsBackTheWholeFrozenRequestPopulation()
    {
        using var f=new Fixture();await f.Prepare();var native=f.Afe.Native;var company=f.Basis.Source.Review.Scope.Company;var actor=f.Basis.Source.Review.Scope.ActorUserId;
        var configured=CompanyLedgerBookConfigurationProof.Require(native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single()).Command with{IntentId=Guid.NewGuid().ToString("N"),Id=Guid.NewGuid().ToString("N"),BookId="management",Name="Management book"};
        var book=configured.CreateRow();book.ROW_CREATED_BY=configured.ActorUserId;book.ROW_CHANGED_BY=configured.ActorUserId;book.ROW_CREATED_DATE=DateTime.UtcNow;book.ROW_CHANGED_DATE=book.ROW_CREATED_DATE;book.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(configured,book);native.Rows.Add(book);
        f.Basis=f.Basis with{Books=await f.Afe.Books.GetCompanyBooksAsync(company,actor,f.Afe.Guard)};f.Basis=f.Basis with{Revision=f.Basis.ContentRevision()};var original=await f.Command();
        var progress=f.Basis.Source.Review with{Scope=f.Basis.Source.Review.Scope with{ActorUserId="reviewer"}};progress=progress with{Revision=progress.ContentRevision()};var maker=f.Basis with{Source=f.Basis.Source with{Review=progress},Books=f.Basis.Books.AsReader("reviewer")};maker=maker with{Revision=maker.ContentRevision()};
        var companion=new DevelopmentAfeCommand(maker,Guid.NewGuid(),"management","AFE-companion","Unexpected companion request",2.125m,new(2027,2,3),"Controlled same-source companion mutation");companion.Require();var before=JsonSerializer.Serialize(native.Rows);
        f.OnInsert=()=>native.Rows.Add(companion.ToRecord(DateTime.UtcNow));await Assert.ThrowsAnyAsync<Exception>(()=>f.Record(original));Assert.Equal(before,JsonSerializer.Serialize(native.Rows));Assert.Empty(native.Rows.OfType<APPROVAL_WORKFLOW>());Assert.Equal("rollback",native.Transactions.Last());
    }
    [Theory][InlineData("current")][InlineData("source")][InlineData("inactive")][InlineData("duplicate")][InlineData("provider-null")][InlineData("field-private")][InlineData("foreign-before-private")]
    public async Task NativeRequestIndexKeepsExactFieldScopeAndCannotTurnCorruptOrFailedEvidenceIntoEmptyWork(string mode)
    {
        using var f=new DevelopmentAfeNativeTests.Fixture();await f.Prepare();var command=await f.Command();await f.Record(command);var row=f.Native.Rows.OfType<AFE>().Single();
        if(mode=="source")row.SOURCE="retagged";if(mode=="inactive")row.ACTIVE_IND="N";if(mode=="duplicate")f.Native.Rows.Add(row);
        if(mode=="field-private")row.DEVELOPMENT_BUDGET_JSON="malformed private input";
        if(mode=="provider-null")f.Native.Source.Setup(source=>source.GetEntityAsync("AFE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((IEnumerable<object>)null!);
        if(mode=="foreign-before-private")
        {
            var foreign=CompanyInventorySourceReview.Copy(row);foreign.OWNER_COMPANY_BA_ID="foreign";foreign.DEVELOPMENT_BUDGET_JSON="malformed private foreign input";
            f.Native.Source.Setup(source=>source.GetEntityAsync("AFE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new[]{foreign});
        }
        Task<IReadOnlyList<DevelopmentAfeIndexItem>> Read()=>f.Owner.ReadDevelopmentIndexAsync(f.Basis.Source.Review.Scope.Company,f.Basis.BudgetDatabase,[mode=="field-private"?"ungranted-field":f.Basis.Source.Review.Scope.FieldId],f.Guard);
        if(mode=="current"){var found=Assert.Single(await Read());Assert.Equal(command.AfeId(),found.Afe.AfeId);Assert.Equal(command.Review.Source.Review.InstanceId,found.ProcessId);}
        else if(mode=="field-private")Assert.Empty(await Read());else if(mode=="foreign-before-private")await Assert.ThrowsAsync<UnauthorizedAccessException>(Read);else await Assert.ThrowsAnyAsync<Exception>(Read);
        Assert.Equal(1,f.Native.Writes);
    }

}
#endif
