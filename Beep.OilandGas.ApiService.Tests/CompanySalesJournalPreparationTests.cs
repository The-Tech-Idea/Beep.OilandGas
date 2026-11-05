#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
using System.Security.Claims;
using Beep.OilandGas.Web.Services;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPreparationTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    [Fact] public async Task FailedFollowupAfterNativeAcknowledgementKeepsExactCircuitCommandUntilCompleteRecovery()
    {
        var fixture=new Fixture(source.Source);var grants=CompanySalesJournalPreparationHttpTests.Grants;using var company=CompanyGLAccountHttpTests.Selected(actor:source.Source.ActorUserId,grants:grants);var client=new Mock<IAccountingServiceClient>();
        client.Setup(value=>value.GetCompanySalesJournalSourceAsync(source.Source.ReceivableId,source.Source.BookId,source.Source.CurrencyCode,source.Source.CalendarId,source.Source.PostingDate,source.Source.Selection)).ReturnsAsync(CompanyInventorySourceReview.Copy(source.Source));
        client.Setup(value=>value.PrepareCompanySalesJournalAsync(It.IsAny<CompanySalesJournalPreparation>())).Returns((CompanySalesJournalPreparation command)=>fixture.Service.PrepareCompanySalesJournalAsync(command,fixture.Guard,fixture.Read));
        client.Setup(value=>value.GetCompanySalesJournalPreparationAsync(source.Source.BookId,source.Source.CurrencyCode,It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("Native follow-up unavailable"));
        using var state=new CompanySalesReceivableWorkspaceState(company);state.AttachOwner(new object());state.Admit(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,source.Source.ActorUserId),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test")));
        await state.LoadJournalSourceAsync(source.Source.ReceivableId,source.Source.BookId,source.Source.CurrencyCode,source.Source.CalendarId,source.Source.PostingDate,source.Source.Selection,client.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareJournalAsync("explicit-followup-journal","Reviewed original source",client.Object));Assert.True(state.HasPendingJournal);Assert.Null(state.JournalPreparation);Assert.Equal(6,fixture.Native.Writes);var saved=JsonSerializer.Serialize(state.PendingJournal);
        client.Setup(value=>value.GetCompanySalesJournalPreparationAsync(source.Source.BookId,source.Source.CurrencyCode,It.IsAny<string>())).Returns((string book,string currency,string id)=>fixture.Service.GetCompanySalesJournalPreparationAsync(source.Source.Company,source.Source.ActorUserId,book,currency,id,fixture.Guard,_=>fixture.Guard()));
        await state.SubmitJournalPreparationAsync(client.Object);Assert.False(state.HasPendingJournal);Assert.Equal("DRAFT",state.JournalPreparation!.Header.STATUS);Assert.Equal(6,fixture.Native.Writes);Assert.Contains("explicit-followup-journal",saved);
    }
    public sealed class ApprovedSourceFixture:IAsyncLifetime
    {
        public CompanySalesJournalSourceReview Source=null!;
        public AR_INVOICE Receivable=null!;
        internal CompanySalesJournalSourceTests.Fixture NativeFixture=null!;
        public async Task InitializeAsync(){using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare(taxAndCharges:true);Source=await fixture.Read();NativeFixture=fixture;Receivable=CompanyInventorySourceReview.Copy(fixture.Current.Original.Receivable);fixture.NoWrites();}
        public Task DisposeAsync()=>Task.CompletedTask;
    }
    internal sealed class Fixture
    {
        public readonly PersonaTaskRoutingTests.Fixture Native=new();
        public readonly JournalEntryService Service;
        public CompanySalesJournalPreparation Command;
        public CompanySalesJournalSourceReview Current;
        public int CurrentReads,HeaderWrites,LineWrites,FailLine;
        public bool Denied,CorruptLine,FailHeaderUpdate;
        public Action? OnLine,OnHeader;
        public Fixture(CompanySalesJournalSourceReview source)
        {
            Current=CompanyInventorySourceReview.Copy(source);Command=new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),CompanyInventorySourceReview.Copy(source),"explicit-journal-001","Record exact approved company source",source.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray());
            Native.Rows.Clear();Native.Connection="production";var columns=new CommonColumnHandler();var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});
            var defaults=Mock.Of<IPPDM39DefaultsRepository>();var accounts=new GLAccountService(Native.Editor.Object,columns,defaults,metadata.Object,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(Native.Connection));
            Service=new(Native.Editor.Object,columns,defaults,metadata.Object,accounts,NullLogger<JournalEntryService>.Instance,()=>Task.FromResult(Native.Connection));
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(request.EntityName=="JOURNAL_ENTRY"?typeof(JOURNAL_ENTRY):typeof(JOURNAL_ENTRY_LINE)));
            Native.Source.Setup(value=>value.InsertEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{Native.Writes++;HeaderWrites++;Native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value));OnHeader?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Source.Setup(value=>value.InsertEntity("JOURNAL_ENTRY_LINE",It.IsAny<object>())).Returns((string table,object value)=>{Native.Writes++;LineWrites++;if(LineWrites==FailLine)return new ErrorsInfo{Flag=Errors.Failed};var line=CompanyInventorySourceReview.Copy((JOURNAL_ENTRY_LINE)value);if(CorruptLine)line.CREDIT_AMOUNT+=.001m;Native.Rows.Add(line);OnLine?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Source.Setup(value=>value.UpdateEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{Native.Writes++;HeaderWrites++;if(FailHeaderUpdate)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows.RemoveAll(row=>row is JOURNAL_ENTRY header && header.JOURNAL_ENTRY_ID==((JOURNAL_ENTRY)value).JOURNAL_ENTRY_ID);Native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value));return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public Task Guard(){if(Denied)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        public Task<CompanySalesJournalSourceReview> Read(){CurrentReads++;return Task.FromResult(CompanyInventorySourceReview.Copy(Current));}
        public Task<CompanySalesJournalPreparationOutcome> Prepare()=>Service.PrepareCompanySalesJournalAsync(Command,Guard,Read);
    }
    [Fact] public async Task ExactRequestIsRetainedBeforeAtomicDraftAndKnownRetryCreatesNoMoreLinesOrReadsAnotherSource()
    {
        var fixture=new Fixture(source.Source);var outcome=await fixture.Prepare();outcome.Require(fixture.Command);Assert.Equal("DRAFT",outcome.Header.STATUS);Assert.Equal(4,outcome.Lines.Count);Assert.Equal(fixture.Command.Source.Lines.Sum(line=>line.Debit),outcome.Lines.Sum(line=>line.DEBIT_AMOUNT));
        Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Native.Transactions);Assert.Equal(6,fixture.Native.Writes);var reads=fixture.CurrentReads;
        fixture.Current=fixture.Current with{Revision="changed"};var retry=await fixture.Prepare();Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(retry));Assert.Equal(reads,fixture.CurrentReads);Assert.Equal(6,fixture.Native.Writes);
    }
    [Theory][InlineData("line")][InlineData("corrupt")][InlineData("header")][InlineData("authority")][InlineData("binding")][InlineData("source")]
    public async Task FailedDraftRollsBackEveryLineAndKeepsExactOriginalRequestedHeader(string fault)
    {
        var fixture=new Fixture(source.Source);fixture.FailLine=fault=="line"?2:0;fixture.CorruptLine=fault=="corrupt";fixture.FailHeaderUpdate=fault=="header";
        fixture.OnLine=()=>{if(fault=="authority")fixture.Denied=true;if(fault=="binding")fixture.Native.Connection="changed";if(fault=="source")fixture.Current=fixture.Current with{Revision="changed"};};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Prepare());Assert.Equal("REQUESTED",Assert.Single(fixture.Native.Rows.OfType<JOURNAL_ENTRY>()).STATUS);Assert.Empty(fixture.Native.Rows.OfType<JOURNAL_ENTRY_LINE>());Assert.Equal("rollback",fixture.Native.Transactions.Last());
    }
    [Theory][InlineData("request")][InlineData("draft")]
    public async Task UnknownCommitReconcilesOnlyOriginalRetainedOrDraftedOutcome(string step)
    {
        var fixture=new Fixture(source.Source);if(step=="request")fixture.Native.FailCommit=true;else fixture.OnLine=()=>fixture.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Prepare());var writes=fixture.Native.Writes;var original=CompanySalesJournalPreparationProof.Require(Assert.Single(fixture.Native.Rows.OfType<JOURNAL_ENTRY>())).Command;Assert.Equal(JsonSerializer.Serialize(fixture.Command),JsonSerializer.Serialize(original));
        fixture.Native.FailCommit=false;fixture.OnLine=null;var recovered=await fixture.Prepare();recovered.Require(fixture.Command);Assert.Equal("DRAFT",recovered.Header.STATUS);if(step=="draft")Assert.Equal(writes,fixture.Native.Writes);
    }
    [Theory][InlineData("source")][InlineData("number")][InlineData("line-ids")][InlineData("borrowed")][InlineData("isolation")]
    public async Task InvalidOriginalRequestOrUnownedTransactionNeverRetainsAnotherJournal(string fault)
    {
        var fixture=new Fixture(source.Source);if(fault=="source")fixture.Current=fixture.Current with{Revision="changed"};if(fault=="number")fixture.Command=fixture.Command with{Number=""};if(fault=="line-ids")fixture.Command=fixture.Command with{LineIds=fixture.Command.LineIds.Select(_=>fixture.Command.JournalId).ToArray()};if(fault=="borrowed")fixture.Native.Borrowed=true;if(fault=="isolation")fixture.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Prepare());Assert.Empty(fixture.Native.Rows);Assert.Equal(0,fixture.Native.Writes);
    }
    [Fact] public async Task AnOriginalCompanySourceOrDocumentNumberCannotBePreparedAgainUnderAnotherIdentity()
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();fixture.Command=fixture.Command with{JournalId=Guid.NewGuid().ToString("N"),IntentId=Guid.NewGuid().ToString("N"),LineIds=source.Source.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray()};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Prepare());Assert.Single(fixture.Native.Rows.OfType<JOURNAL_ENTRY>());Assert.Equal(6,fixture.Native.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task OriginalProducerRecoversRequestedOrDraftedNativeOutcomeWithoutAnotherWrite(bool requested)
    {
        var fixture=new Fixture(source.Source);fixture.FailLine=requested?2:0;if(requested)await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Prepare());else await fixture.Prepare();var writes=fixture.Native.Writes;
        var outcome=await fixture.Service.GetCompanySalesJournalPreparationAsync(fixture.Command.Source.Company,fixture.Command.Source.ActorUserId,fixture.Command.Source.BookId,fixture.Command.Source.CurrencyCode,fixture.Command.JournalId,fixture.Guard,_=>fixture.Guard());Assert.NotNull(outcome);outcome.Require(fixture.Command);Assert.Equal(requested?"REQUESTED":"DRAFT",outcome.Header.STATUS);Assert.Equal(writes,fixture.Native.Writes);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Service.GetCompanySalesJournalPreparationAsync(fixture.Command.Source.Company,"other",fixture.Command.Source.BookId,fixture.Command.Source.CurrencyCode,fixture.Command.JournalId,fixture.Guard,_=>fixture.Guard()));
        Assert.Null(await fixture.Service.GetCompanySalesJournalPreparationAsync(fixture.Command.Source.Company,fixture.Command.Source.ActorUserId,fixture.Command.Source.BookId,fixture.Command.Source.CurrencyCode,Guid.NewGuid().ToString("N"),fixture.Guard,_=>fixture.Guard()));
    }
    [Theory][InlineData("line")][InlineData("audit")][InlineData("scope")][InlineData("field")]
    public async Task UnconfirmedOrUnauthorizedNativeRecoveryCannotPublishOriginalCommand(string fault)
    {
        var fixture=new Fixture(source.Source);await fixture.Prepare();var header=fixture.Native.Rows.OfType<JOURNAL_ENTRY>().Single();if(fault=="line")fixture.Native.Rows.OfType<JOURNAL_ENTRY_LINE>().First().CREDIT_AMOUNT+=.001m;if(fault=="audit")header.ROW_CREATED_BY="other";if(fault=="scope")header.OWNER_COMPANY_BA_ID="other";
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Service.GetCompanySalesJournalPreparationAsync(fixture.Command.Source.Company,fixture.Command.Source.ActorUserId,fixture.Command.Source.BookId,fixture.Command.Source.CurrencyCode,fixture.Command.JournalId,fixture.Guard,_=>fault=="field"?throw new UnauthorizedAccessException():fixture.Guard()));Assert.Equal(6,fixture.Native.Writes);
    }
    [Theory][InlineData("header")][InlineData("lines")][InlineData("create")]
    public async Task StandaloneLegacyJournalCannotReadOrBorrowCompanyOwnershipMetadata(string operation)
    {
        var fixture=new Fixture(source.Source);var columns=new CommonColumnHandler();var metadata=Mock.Of<IPPDMMetadataRepository>();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var accounts=new GLAccountService(fixture.Native.Editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance);
        var legacy=new JournalEntryService(fixture.Native.Editor.Object,columns,defaults,metadata,accounts,NullLogger<JournalEntryService>.Instance);fixture.Native.Rows.Add(fixture.Command.CreateHeader());fixture.Native.Rows.Add(fixture.Command.CreateLine(0));
        await Assert.ThrowsAnyAsync<Exception>(async()=>{if(operation=="header")await legacy.GetEntryByIdAsync(fixture.Command.JournalId);else if(operation=="lines")await legacy.GetEntryLineItemsAsync(fixture.Command.JournalId);else await legacy.CreateEntryAsync(fixture.Command.Source.PostingDate,"legacy",[fixture.Command.CreateLine(0)],fixture.Command.Source.ActorUserId);});Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
}
#endif

