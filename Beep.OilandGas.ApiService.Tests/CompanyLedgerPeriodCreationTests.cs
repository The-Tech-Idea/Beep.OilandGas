using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodCreationTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public readonly CompanyLedgerPeriodReadTests.Fixture Reader=new();
        public CompanyLedgerPeriodCreationSubmission Submission=null!;
        public bool FailInsert,CorruptInsert;
        public Action? OnInsert;
        public Fixture()
        {
            Reader.Native.Native.Source.Setup(source=>source.InsertEntity("COMPANY_LEDGER_PERIOD",It.IsAny<object>())).Returns((string table,object value)=>{
                Reader.Native.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=JsonSerializer.Deserialize<COMPANY_LEDGER_PERIOD>(JsonSerializer.Serialize(value))!;
                if(CorruptInsert)row.STATUS="CLOSED";Reader.Native.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Prepare()
        {
            var review=await Reader.Service.GetCompanyPeriodsAsync(Company,"administrator","statutory","KWD","company-gregorian",Reader.Native.Authority);
            Submission=new(new(Company,"administrator","statutory","KWD","company-gregorian",Guid.NewGuid().ToString("N"),"FY26-M11",new DateTime(2026,11,1),new DateTime(2026,11,30),Guid.NewGuid().ToString("N"),"Reviewed company calendar period",review.Revision,new string('B',64)),review);
        }
        public Task<COMPANY_LEDGER_PERIOD> Record()=>Reader.Service.CreateCompanyPeriodAsync(Submission,Reader.Native.Authority);
    }
    [Fact] public async Task NativeCreationCommitsOnceAndExactRetryReturnsOriginalDurableAuthority()
    {
        var fixture=new Fixture();await fixture.Prepare();var row=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyLedgerPeriodAuthority.Require(row).Command);
        Assert.Equal("OPEN",row.STATUS);Assert.Equal("administrator",row.ROW_CREATED_BY);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await fixture.Record()));
        Assert.Equal(1,fixture.Reader.Native.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Reader.Native.Native.Transactions);
        var review=await fixture.Reader.Read();Assert.Equal(row.COMPANY_LEDGER_PERIOD_ID,review.RequireOpen(new DateTime(2026,11,15)).COMPANY_LEDGER_PERIOD_ID);
    }
    [Theory][InlineData("source")][InlineData("code")][InlineData("overlap")][InlineData("calendar")][InlineData("date")][InlineData("reason")][InlineData("review")]
    public async Task ChangedUnreviewedOrConflictingPeriodNeverWrites(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();var command=fixture.Submission.Command;
        if(fault=="source")fixture.Reader.Period.PERIOD_CODE="changed";if(fault=="code")command=command with{PeriodCode=fixture.Reader.Period.PERIOD_CODE};if(fault=="overlap")command=command with{From=new DateTime(2026,10,31)};
        if(fault=="calendar")command=command with{CalendarId="another-calendar"};if(fault=="date")command=command with{From=DateTime.SpecifyKind(command.From,DateTimeKind.Utc)};if(fault=="reason")command=command with{Reason=""};
        fixture.Submission=fixture.Submission with{Command=command,Review=fault=="review"?null:fixture.Submission.Review};await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("binding")][InlineData("other-period")]
    public async Task NativeFailureOrClosingSourceChangeRollsBackNewPeriod(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.FailInsert=fault=="insert";fixture.CorruptInsert=fault=="readback";
        fixture.OnInsert=()=>{if(fault=="authority")fixture.Reader.Native.RevokeAt=fixture.Reader.Native.AuthorityCalls+1;if(fault=="binding")fixture.Reader.Native.Native.Connection="changed";if(fault=="other-period")fixture.Reader.Period.PERIOD_CODE="changed";};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Single(fixture.Reader.Native.Native.Rows.OfType<COMPANY_LEDGER_PERIOD>());Assert.Equal("rollback",fixture.Reader.Native.Native.Transactions.Last());
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("schema")]
    public async Task MissingOwnedSerializableTransactionOrReceiptMetadataNeverWrites(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();if(fault=="borrowed")fixture.Reader.Native.Native.Borrowed=true;if(fault=="isolation")fixture.Reader.Native.Native.Isolation=IsolationLevel.ReadCommitted;
        if(fault=="schema")fixture.Reader.Native.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="AUTHORITY_JSON"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Native.Writes);if(fault=="borrowed")Assert.Empty(fixture.Reader.Native.Native.Transactions);
    }
    [Fact] public async Task LostAcknowledgementReconcilesOriginalCommandWithoutReusingChangedSourceForANewWrite()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Reader.Native.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Native.Native.Writes);
        fixture.Reader.Native.Native.FailCommit=false;fixture.Reader.Period.PERIOD_CODE="later-history";fixture.Submission=fixture.Submission with{Review=null};var row=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyLedgerPeriodAuthority.Require(row).Command);Assert.Equal(1,fixture.Reader.Native.Native.Writes);
    }
    [Fact] public async Task OriginalIntentOrIdentityCannotBeReusedForAnAlteredPeriod()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();var first=fixture.Submission;fixture.Submission=first with{Command=first.Command with{PeriodCode="altered"}};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());
        fixture.Submission=first with{Command=first.Command with{PeriodId=Guid.NewGuid().ToString("N")}};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Native.Native.Writes);
    }
    [Fact] public async Task OriginalActorRecoversPrivateRequestAndOtherCompanyAccountCannotBorrowIt()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();var command=fixture.Submission.Command;
        Assert.Equal(command,await fixture.Reader.Service.GetCompanyPeriodCreationAsync(Company,"administrator","statutory","KWD","company-gregorian",command.PeriodId,fixture.Reader.Native.Authority));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Reader.Service.GetCompanyPeriodCreationAsync(Company,"accountant","statutory","KWD","company-gregorian",command.PeriodId,fixture.Reader.Native.Authority));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Reader.Service.GetCompanyPeriodCreationAsync(new("tenant","other-company"),"administrator","statutory","KWD","company-gregorian",command.PeriodId,fixture.Reader.Native.Authority));
        Assert.Null(await fixture.Reader.Service.GetCompanyPeriodCreationAsync(Company,"administrator","statutory","KWD","company-gregorian",Guid.NewGuid().ToString("N"),fixture.Reader.Native.Authority));Assert.Equal(1,fixture.Reader.Native.Native.Writes);
    }
    [Theory][InlineData("source")][InlineData("binding")][InlineData("authority")]
    public async Task ChangedPrivateRecoveryCannotPublishTheOriginalRequest(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();fixture.Reader.Native.Reads=0;fixture.Reader.Native.OnRead=()=>{if(fixture.Reader.Native.Reads!=2)return;if(fault=="source")fixture.Reader.Native.Native.Rows.OfType<COMPANY_LEDGER_PERIOD>().Single(row=>row.COMPANY_LEDGER_PERIOD_ID==fixture.Submission.Command.PeriodId).PERIOD_CODE="changed";if(fault=="binding")fixture.Reader.Native.Native.Connection="changed";if(fault=="authority")fixture.Reader.Native.RevokeAt=fixture.Reader.Native.AuthorityCalls+1;};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Reader.Service.GetCompanyPeriodCreationAsync(Company,"administrator","statutory","KWD","company-gregorian",fixture.Submission.Command.PeriodId,fixture.Reader.Native.Authority));Assert.Equal(1,fixture.Reader.Native.Native.Writes);
    }
}
#endif
