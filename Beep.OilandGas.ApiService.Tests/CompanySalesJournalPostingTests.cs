#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPostingTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    internal sealed class Fixture
    {
        public readonly CompanySalesJournalReviewTests.Fixture Reviewed;
        public ARService AR=null!;
        public CompanySalesJournalPostingCommand Command=null!;
        public bool Denied,FieldDenied,Changed,FailHeader,FailOutcome,CorruptEntry,UnknownPosting;
        public int FailEntry,EntryWrites,Reads;
        public Action? OnEntry;
        public Fixture(CompanySalesJournalSourceReview current){Reviewed=new(current);}
        public async Task Prepare(CompanySalesJournalPreparationTests.ApprovedSourceFixture baseline,Func<Task>? beforeReview=null)
        {
            await Reviewed.Prepare();if(beforeReview is not null)await beforeReview();var approved=await Reviewed.Review();var native=Reviewed.Draft.Native;
            foreach(var row in baseline.NativeFixture.Native.Rows)native.Rows.Add(JsonSerializer.Deserialize(JsonSerializer.Serialize(row,row.GetType()),row.GetType())!);
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});var original=baseline.NativeFixture.Receivable.Data.Owner;
            T Dependency<T>(string name)=>(T)typeof(ARService).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(original)!;
            AR=new(native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,Dependency<AccountingBasisPostingService>("_basisPosting"),Dependency<IFailureReporter>("_failures"),NullLogger<ARService>.Instance,resolveConnection:()=>Task.FromResult(native.Connection));
            var types=native.Rows.Select(value=>value.GetType()).Concat([typeof(GL_ENTRY),typeof(SALES_JOURNAL_POSTING_INTENT),typeof(SALES_JOURNAL_POSTING_OUTCOME)]).Distinct().ToDictionary(type=>type==typeof(GLAccountMapping)?"GL_ACCOUNT_MAPPING":type.Name,type=>type);native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types[request.EntityName]));
            native.Source.Setup(value=>value.InsertEntity("SALES_JOURNAL_POSTING_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_JOURNAL_POSTING_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("GL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;EntryWrites++;if(EntryWrites==FailEntry)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((GL_ENTRY)value);if(CorruptEntry)row.DEBIT_AMOUNT+=.001m;native.Rows.Add(row);OnEntry?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.InsertEntity("SALES_JOURNAL_POSTING_OUTCOME",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(FailOutcome)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_JOURNAL_POSTING_OUTCOME)value));if(UnknownPosting)native.FailCommit=true;return new ErrorsInfo{Flag=Errors.Ok};});
            native.Source.Setup(value=>value.UpdateEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(FailHeader)return new ErrorsInfo{Flag=Errors.Failed};native.Rows.RemoveAll(row=>row is JOURNAL_ENTRY header && header.JOURNAL_ENTRY_ID==((JOURNAL_ENTRY)value).JOURNAL_ENTRY_ID);native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value));return new ErrorsInfo{Flag=Errors.Ok};});
            var posting=CompanySalesJournalPostingSource.Create("authorized-poster",approved,baseline.Source,baseline.Receivable);Command=new(Guid.NewGuid().ToString("N"),posting,approved.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray(),"Explicit original balanced journal posting");native.Writes=0;native.Transactions.Clear();
        }
        public Task Guard(){if(Denied)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        public Task Field(string id){Assert.Equal(Command.Source.Current.FieldId,id);if(FieldDenied)throw new UnauthorizedAccessException();return Guard();}
        public Task<CompanySalesJournalPostingSource> Current(){Reads++;return Task.FromResult(Changed?Command.Source with{Revision=new string('F',64)}:CompanyInventorySourceReview.Copy(Command.Source));}
        public Task<CompanySalesJournalPostingOutcome> Post()=>Reviewed.Draft.Service.PostCompanySalesJournalAsync(Command,AR,Current,Guard,Field,Field);
        public Task<CompanySalesJournalPostingRecovery> Recover(string? actor=null)=>Reviewed.Draft.Service.GetCompanySalesJournalPostingRecoveryAsync(Command.Source.Current.Company,actor??Command.Source.ActorUserId,Command.Source.Current.BookId,Command.Source.Current.CurrencyCode,Command.Source.Reviewed.Header.JOURNAL_ENTRY_ID,Guard,Field);
    }
    [Fact] public async Task OriginalRetentionThenAtomicBalancedGLPostedJournalAndARCorrelationHaveExactDurableRecovery()
    {
        var f=new Fixture(source.Source);await f.Prepare(source);var ar=JsonSerializer.Serialize(source.Receivable);var result=await f.Post();result.Require(f.Command);var native=f.Reviewed.Draft.Native;
        Assert.Equal(new[]{"begin","commit","begin","commit"},native.Transactions);Assert.Single(native.Rows.OfType<SALES_JOURNAL_POSTING_INTENT>());Assert.Single(native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>());Assert.Equal(4,native.Rows.OfType<GL_ENTRY>().Count());Assert.Equal("POSTED",result.Journal.Header.STATUS);Assert.Equal(ar,JsonSerializer.Serialize(native.Rows.OfType<AR_INVOICE>().Single()));Assert.Equal(result.Journal.Header.TOTAL_DEBIT,result.Entries.Sum(value=>value.DEBIT_AMOUNT));Assert.Equal(result.Journal.Header.TOTAL_CREDIT,result.Entries.Sum(value=>value.CREDIT_AMOUNT));Assert.Equal(7,native.Writes);
        var recovered=await f.Recover();recovered.Require();Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered.Outcome));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Recover("another-poster"));
        var reads=f.Reads;f.Changed=true;var replay=await f.Post();Assert.Equal(reads,f.Reads);Assert.Equal(7,native.Writes);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(replay));var maker=await f.Reviewed.Draft.Prepare();Assert.Equal("POSTED",maker.Header.STATUS);Assert.Equal(7,native.Writes);
    }
    [Theory][InlineData("entry")][InlineData("corrupt")][InlineData("header")][InlineData("outcome")][InlineData("authority")][InlineData("field")][InlineData("binding")][InlineData("closing-source")]
    public async Task FailedAtomicPostingRollsBackEveryGLHeaderAndARCorrelationButKeepsOriginalIntent(string fault)
    {
        var f=new Fixture(source.Source);await f.Prepare(source);f.FailEntry=fault=="entry"?2:0;f.CorruptEntry=fault=="corrupt";f.FailHeader=fault=="header";f.FailOutcome=fault=="outcome";var native=f.Reviewed.Draft.Native;
        f.OnEntry=()=>{if(fault=="authority")f.Denied=true;if(fault=="field")f.FieldDenied=true;if(fault=="binding")native.Connection="other";if(fault=="closing-source")f.Changed=true;};
        await Assert.ThrowsAnyAsync<Exception>(f.Post);Assert.Single(native.Rows.OfType<SALES_JOURNAL_POSTING_INTENT>());Assert.Empty(native.Rows.OfType<GL_ENTRY>());Assert.Empty(native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>());Assert.Equal("APPROVED",native.Rows.OfType<JOURNAL_ENTRY>().Single().STATUS);Assert.Equal(JsonSerializer.Serialize(source.Receivable),JsonSerializer.Serialize(native.Rows.OfType<AR_INVOICE>().Single()));Assert.Equal("rollback",native.Transactions.Last());
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task UnknownRetentionOrPostingCommitReconcilesOnlyExactOriginalArtifacts(bool posting)
    {
        var f=new Fixture(source.Source);await f.Prepare(source);f.UnknownPosting=posting;f.Reviewed.Draft.Native.FailCommit=!posting;await Assert.ThrowsAnyAsync<Exception>(f.Post);var recovery=await f.Recover();recovery.Require();Assert.Equal(posting,recovery.Outcome is not null);var body=JsonSerializer.Serialize(f.Command);var writes=f.Reviewed.Draft.Native.Writes;f.UnknownPosting=false;f.Reviewed.Draft.Native.FailCommit=false;
        var outcome=await f.Post();outcome.Require(f.Command);Assert.Equal(body,JsonSerializer.Serialize(f.Command));if(posting)Assert.Equal(writes,f.Reviewed.Draft.Native.Writes);Assert.Equal(4,f.Reviewed.Draft.Native.Rows.OfType<GL_ENTRY>().Count());Assert.Single(f.Reviewed.Draft.Native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>());
    }
    [Theory][InlineData("gl")][InlineData("correlation")][InlineData("intent")][InlineData("ar-review")]
    public async Task PostedHeaderCannotLendSuccessWhenActualOriginalArtifactsDisappear(string missing)
    {
        var f=new Fixture(source.Source);await f.Prepare(source);_=await f.Post();var native=f.Reviewed.Draft.Native;var writes=native.Writes;native.Rows.Remove(missing switch{"gl"=>native.Rows.OfType<GL_ENTRY>().First(),"correlation"=>native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>().Single(),"intent"=>native.Rows.OfType<SALES_JOURNAL_POSTING_INTENT>().Single(),_=>native.Rows.OfType<SALES_RECEIVABLE_REVIEW_INTENT>().Single()});await Assert.ThrowsAnyAsync<Exception>(()=>f.Recover());await Assert.ThrowsAnyAsync<Exception>(f.Post);Assert.Equal(writes,native.Writes);
    }
    [Fact] public async Task CompanylessLegacyGLEvidenceAndBalancesRefuseEvenPartialCompanyMarkers()
    {
        var f=new CompanySalesJournalPreparationTests.Fixture(source.Source);var columns=new CommonColumnHandler();var metadata=Mock.Of<IPPDMMetadataRepository>();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var accounts=new GLAccountService(f.Native.Editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance);var legacy=new JournalEntryService(f.Native.Editor.Object,columns,defaults,metadata,accounts,NullLogger<JournalEntryService>.Instance);
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(f.Native.Editor.Object).ConvertToEntityStructure(typeof(GL_ENTRY)));
        f.Native.Source.Setup(value=>value.GetEntityAsync("GL_ENTRY",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{new GL_ENTRY{GL_ENTRY_ID=Guid.NewGuid().ToString("N"),GL_ACCOUNT_ID="actual-account",BOOK_ID="company-book",DEBIT_AMOUNT=10m,ACTIVE_IND="Y"}});
        var date=new DateTime(2026,1,1);var read=await Assert.ThrowsAsync<InvalidOperationException>(()=>legacy.GetEntriesByAccountAsync("actual-account",date,date));Assert.Contains("Company-owned GL evidence",read.Message);var balance=await Assert.ThrowsAsync<InvalidOperationException>(()=>legacy.GetAccountBalanceAsync("actual-account",date));Assert.Contains("Company-owned GL evidence",balance.Message);Assert.Equal(0,f.Native.Writes);
    }
}
#endif
