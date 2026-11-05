#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodTransitionTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyLedgerPeriodCreationTests.Fixture Creation=new();
        internal COMPANY_LEDGER_PERIOD Original=null!;
        internal CompanyLedgerPeriodTransitionSubmission Submission=null!;
        internal bool FailUpdate,CorruptUpdate;
        internal Action? AfterUpdate;
        internal async Task Prepare()
        {
            await Creation.Prepare();Original=await Creation.Record();var native=Creation.Reader.Native.Native;
            native.Source.Setup(value=>value.UpdateEntity("COMPANY_LEDGER_PERIOD",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(FailUpdate)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((COMPANY_LEDGER_PERIOD)value);if(CorruptUpdate)row.PERIOD_CODE="corrupted";native.Rows.RemoveAll(value=>value is COMPANY_LEDGER_PERIOD before && before.COMPANY_LEDGER_PERIOD_ID==row.COMPANY_LEDGER_PERIOD_ID);native.Rows.Add(row);AfterUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});native.Writes=0;native.Transactions.Clear();await Review("controller","CLOSED");
        }
        internal async Task Review(string actor,string target)
        {
            var original=Creation.Submission.Command;var review=await Creation.Reader.Service.GetCompanyPeriodsAsync(original.Company,actor,original.BookId,original.CurrencyCode,original.CalendarId,Creation.Reader.Native.Authority);var period=review.Periods.Single(row=>row.COMPANY_LEDGER_PERIOD_ID==Original.COMPANY_LEDGER_PERIOD_ID);
            Submission=new(new(Guid.NewGuid().ToString("N"),review.Company,actor,review.BookId,review.CurrencyCode,review.CalendarId,period.COMPANY_LEDGER_PERIOD_ID,period.STATUS,target,"CONFIRMED","Explicit independently reviewed closing/control evidence","Controlled company period state",review.Revision,original.BindingRevision),review);
        }
        internal Task<CompanyLedgerPeriodTransitionOutcome> Record()=>Creation.Reader.Service.TransitionCompanyPeriodAsync(Submission,Creation.Reader.Native.Authority);
    }
    [Fact] public async Task CloseLockReopenKeepOrderedIndependentHistoryAndOriginalNoWriteRecovery()
    {
        var f=new Fixture();await f.Prepare();var close=await f.Record();close.Require(f.Submission.Command);Assert.Equal("CLOSED",close.Period.STATUS);Assert.Equal(f.Creation.Submission.Command,CompanyLedgerPeriodAuthority.Require(close.Period).Command);
        var privateClose=f.Submission.Command;var review=await f.Creation.Reader.Read();Assert.Throws<InvalidOperationException>(()=>review.RequireOpen(new DateTime(2026,11,15)));
        await f.Review("locker","LOCKED");await f.Record();await f.Review("reopener","OPEN");var reopened=await f.Record();Assert.Equal(3,CompanyLedgerPeriodStateHistory.Require(reopened.Period).Entries.Count);Assert.Equal(3,f.Creation.Reader.Native.Native.Writes);Assert.Equal(JsonSerializer.Serialize(reopened),JsonSerializer.Serialize(await f.Record()));Assert.Equal(3,f.Creation.Reader.Native.Native.Writes);
        Assert.Equal("OPEN",(await f.Creation.Reader.Read()).RequireOpen(new DateTime(2026,11,15)).STATUS);
        var old=await f.Creation.Reader.Service.GetCompanyPeriodTransitionAsync(privateClose.Company,privateClose.ActorUserId,privateClose.BookId,privateClose.CurrencyCode,privateClose.CalendarId,privateClose.PeriodId,privateClose.IntentId,f.Creation.Reader.Native.Authority);Assert.Equal(JsonSerializer.Serialize(close),JsonSerializer.Serialize(old));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Creation.Reader.Service.GetCompanyPeriodTransitionAsync(privateClose.Company,"other",privateClose.BookId,privateClose.CurrencyCode,privateClose.CalendarId,privateClose.PeriodId,privateClose.IntentId,f.Creation.Reader.Native.Authority));
    }
    [Theory][InlineData("maker")][InlineData("state")][InlineData("source")][InlineData("reason")][InlineData("evidence")][InlineData("update")][InlineData("corrupt")][InlineData("authority")][InlineData("binding")][InlineData("other-period")]
    public async Task InvalidOrChangedPeriodTransitionNeverLeavesPartialState(string fault)
    {
        var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Creation.Reader.Native.Native.Rows);var command=f.Submission.Command;
        if(fault=="maker")command=command with{ActorUserId="administrator"};if(fault=="state")command=command with{ToState="LOCKED"};if(fault=="source")command=command with{SourceRevision=new string('C',64)};if(fault=="reason")command=command with{Reason=""};if(fault=="evidence")command=command with{ClosingEvidence=""};f.Submission=f.Submission with{Command=command};f.FailUpdate=fault=="update";f.CorruptUpdate=fault=="corrupt";
        f.AfterUpdate=()=>{if(fault=="authority")f.Creation.Reader.Native.RevokeAt=f.Creation.Reader.Native.AuthorityCalls+1;if(fault=="binding")f.Creation.Reader.Native.Native.Connection="changed";if(fault=="other-period")f.Creation.Reader.Native.Native.Rows.OfType<COMPANY_LEDGER_PERIOD>().Single(row=>row.COMPANY_LEDGER_PERIOD_ID!=command.PeriodId).PERIOD_CODE="changed";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(before,JsonSerializer.Serialize(f.Creation.Reader.Native.Native.Rows));
    }
    [Fact] public async Task UncertainCommitKeepsOriginalCommandAndExactRetryDoesNotAppendAnotherHistoryEntry()
    {
        var f=new Fixture();await f.Prepare();var native=f.Creation.Reader.Native.Native;native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(1,native.Writes);native.FailCommit=false;
        var result=await f.Record();result.Require(f.Submission.Command);Assert.Single(CompanyLedgerPeriodStateHistory.Require(result.Period).Entries);Assert.Equal(1,native.Writes);f.Submission=f.Submission with{Command=f.Submission.Command with{Reason="Changed body"}};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,native.Writes);
    }
    [Theory][InlineData("empty")][InlineData("null-command")][InlineData("state")][InlineData("actor")]
    public async Task MalformedOrTamperedStateHistoryCannotSupplyPostingOrAuthority(string fault)
    {
        var f=new Fixture();await f.Prepare();var result=await f.Record();var row=f.Creation.Reader.Native.Native.Rows.OfType<COMPANY_LEDGER_PERIOD>().Single(row=>row.COMPANY_LEDGER_PERIOD_ID==result.Command.PeriodId);
        if(fault=="empty")row.STATE_HISTORY_JSON="{}";if(fault=="null-command"){var history=CompanyLedgerPeriodStateHistory.Require(row);row.STATE_HISTORY_JSON=JsonSerializer.Serialize(history with{Entries=[history.Entries[0] with{Command=null!}]});}if(fault=="state")row.STATUS="OPEN";if(fault=="actor")row.ROW_CHANGED_BY="other";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Creation.Reader.Read());Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);
    }
}
#endif
