#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalReviewTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    internal sealed class Fixture
    {
        public readonly CompanySalesJournalPreparationTests.Fixture Draft;
        public CompanySalesJournalPreparationOutcome Original=null!;
        public CompanySalesJournalReviewCommand Command=null!;
        public string Actor="independent-reviewer";
        public bool Denied,SourceChanged,FailIntent,FailReview,CorruptReview,UnknownReview;
        public Action? OnUpdate;
        public int Reads;
        public Fixture(CompanySalesJournalSourceReview source)
        {
            Draft=new(source);Draft.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(Draft.Native.Editor.Object).ConvertToEntityStructure(request.EntityName switch{"JOURNAL_ENTRY"=>typeof(JOURNAL_ENTRY),"JOURNAL_ENTRY_LINE"=>typeof(JOURNAL_ENTRY_LINE),_=>typeof(SALES_JOURNAL_REVIEW_INTENT)}));
            Draft.Native.Source.Setup(value=>value.InsertEntity("SALES_JOURNAL_REVIEW_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{Draft.Native.Writes++;if(FailIntent)return new ErrorsInfo{Flag=Errors.Failed};Draft.Native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_JOURNAL_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Prepare(string decision="APPROVE")
        {
            Original=await Draft.Prepare();Draft.Native.Writes=0;Draft.Native.Transactions.Clear();
            Draft.Native.Source.Setup(value=>value.UpdateEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{Draft.Native.Writes++;if(FailReview)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value);if(CorruptReview)row.TOTAL_DEBIT+=.001m;Draft.Native.Rows.RemoveAll(value=>value is JOURNAL_ENTRY header && header.JOURNAL_ENTRY_ID==row.JOURNAL_ENTRY_ID);Draft.Native.Rows.Add(row);if(UnknownReview)Draft.Native.FailCommit=true;OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Command=new(Guid.NewGuid().ToString("N"),await Current(),decision,"CONFIRMED","Exact native balanced currency evidence","CONFIRMED","Original approved source/configuration/period evidence","Explicit independent journal review");
        }
        public Task Guard(){if(Denied)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        public async Task<CompanySalesJournalReviewSource> Current()
        {
            Reads++;var value=Draft.Command.Source;var actual=await Draft.Service.GetCompanySalesJournalForReviewAsync(value.Company,Actor,value.BookId,value.CurrencyCode,Draft.Command.JournalId,Guard,_=>Guard()) ??throw new InvalidOperationException();
            var header=actual.Header.REVIEW_WORKFLOW_JSON is null?actual.Header:CompanySalesJournalReviewProof.OriginalDraftHeader(actual.Header);
            return CompanySalesJournalReviewSource.Create(Actor,new(header,actual.Lines),SourceChanged?value with{Revision="changed"}:CompanyInventorySourceReview.Copy(value));
        }
        public Task<CompanySalesJournalPreparationOutcome> Review()=>Draft.Service.RecordCompanySalesJournalReviewAsync(Command,Guard,Current);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task IndependentNativeReviewPreservesOriginalMakerAmountsLinesAndPreparationAndKnownRetry(string decision)
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare(decision);var lines=JsonSerializer.Serialize(fixture.Original.Lines);var preparation=fixture.Original.Header.PREPARATION_WORKFLOW_JSON;var result=await fixture.Review();result.Require(fixture.Draft.Command);
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",result.Header.STATUS);Assert.Equal(source.Source.ActorUserId,result.Header.ROW_CREATED_BY);Assert.Equal(fixture.Actor,result.Header.ROW_CHANGED_BY);Assert.Equal(lines,JsonSerializer.Serialize(result.Lines));Assert.Equal(preparation,result.Header.PREPARATION_WORKFLOW_JSON);Assert.Equal(2,fixture.Draft.Native.Writes);
        fixture.SourceChanged=true;var reads=fixture.Reads;var retry=await fixture.Review();Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(retry));Assert.Equal(reads,fixture.Reads);Assert.Equal(2,fixture.Draft.Native.Writes);
        var originalPreparation=await fixture.Draft.Prepare();Assert.Equal(result.Header.STATUS,originalPreparation.Header.STATUS);Assert.Equal(2,fixture.Draft.Native.Writes);
    }
    [Theory][InlineData("self")][InlineData("evidence")][InlineData("financial")][InlineData("source")][InlineData("reason")][InlineData("decision")]
    public async Task SelfReviewMissingEvidenceOrUnconfirmedApprovalCannotRetainReviewerCommand(string fault)
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();if(fault=="self")fixture.Actor=source.Source.ActorUserId;
        if(fault=="self")await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Current());else{fixture.Command=fault switch{"evidence"=>fixture.Command with{SourceEvidence=""},"financial"=>fixture.Command with{FinancialDisposition="ISSUE"},"source"=>fixture.Command with{SourceDisposition="ISSUE"},"reason"=>fixture.Command with{Reason=""},_=>fixture.Command with{Decision="POST"}};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Review());}
        Assert.Equal(0,fixture.Draft.Native.Writes);Assert.Empty(fixture.Draft.Native.Rows.OfType<SALES_JOURNAL_REVIEW_INTENT>());
    }
    [Theory][InlineData("intent")][InlineData("update")][InlineData("corrupt")][InlineData("source")][InlineData("authority")]
    public async Task FailedDispositionKeepsOriginalDraftAndRetainedIndependentRequest(string fault)
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();fixture.FailIntent=fault=="intent";fixture.FailReview=fault=="update";fixture.CorruptReview=fault=="corrupt";fixture.OnUpdate=()=>{if(fault=="source")fixture.SourceChanged=true;if(fault=="authority")fixture.Denied=true;};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Review());Assert.Equal("DRAFT",fixture.Draft.Native.Rows.OfType<JOURNAL_ENTRY>().Single().STATUS);Assert.Equal(4,fixture.Draft.Native.Rows.OfType<JOURNAL_ENTRY_LINE>().Count());Assert.Equal(fault=="intent"?0:1,fixture.Draft.Native.Rows.OfType<SALES_JOURNAL_REVIEW_INTENT>().Count());Assert.Equal("rollback",fixture.Draft.Native.Transactions.Last());
    }
    [Fact] public async Task UnknownDispositionCommitReconcilesExactOriginalReviewWithoutAnotherMutation()
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();fixture.UnknownReview=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Review());Assert.Equal(2,fixture.Draft.Native.Writes);fixture.UnknownReview=false;fixture.Draft.Native.FailCommit=false;fixture.SourceChanged=true;
        var result=await fixture.Review();Assert.Equal("APPROVED",result.Header.STATUS);Assert.Equal(2,fixture.Draft.Native.Writes);
    }
    [Fact] public async Task ACompetingReviewerCannotReplaceOriginalRetainedRequest()
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();fixture.FailReview=true;await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Review());fixture.FailReview=false;fixture.Command=fixture.Command with{IntentId=Guid.NewGuid().ToString("N"),Reason="replacement"};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Review());Assert.Single(fixture.Draft.Native.Rows.OfType<SALES_JOURNAL_REVIEW_INTENT>());Assert.Equal("DRAFT",fixture.Draft.Native.Rows.OfType<JOURNAL_ENTRY>().Single().STATUS);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task OriginalReviewerRecoversRetainedOrReviewedOutcomeWithoutAnotherMutation(bool retainedOnly)
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();fixture.FailReview=retainedOnly;if(retainedOnly)await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Review());else await fixture.Review();var value=fixture.Draft.Command.Source;
        var recovered=await fixture.Draft.Service.GetCompanySalesJournalReviewRecoveryAsync(value.Company,fixture.Actor,value.BookId,value.CurrencyCode,fixture.Draft.Command.JournalId,fixture.Guard,_=>fixture.Guard());recovered.Require();Assert.NotNull(recovered.Intent);Assert.Equal(JsonSerializer.Serialize(fixture.Command),JsonSerializer.Serialize(recovered.Intent!.Restore()));Assert.Equal(retainedOnly,recovered.Outcome is null);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Draft.Service.GetCompanySalesJournalReviewRecoveryAsync(value.Company,"other-reviewer",value.BookId,value.CurrencyCode,fixture.Draft.Command.JournalId,fixture.Guard,_=>fixture.Guard()));
    }
    [Fact] public async Task ReviewedHeaderCannotBorrowProofWhenActualReviewerArtifactIsMissing()
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();await fixture.Review();fixture.Draft.Native.Rows.RemoveAll(row=>row is SALES_JOURNAL_REVIEW_INTENT);var value=fixture.Draft.Command.Source;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Draft.Service.GetCompanySalesJournalForReviewAsync(value.Company,fixture.Actor,value.BookId,value.CurrencyCode,fixture.Draft.Command.JournalId,fixture.Guard,_=>fixture.Guard()));Assert.Equal(2,fixture.Draft.Native.Writes);
    }
}
#endif
