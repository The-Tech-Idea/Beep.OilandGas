using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodReadTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private static readonly DateTime Date=new(2026,10,12);
    internal static COMPANY_LEDGER_PERIOD Row(string code="FY26-M10")
    {
        var empty=CompanyLedgerPeriodReview.Create(Company,"administrator","statutory","KWD","company-gregorian",[]);
        var command=new CompanyLedgerPeriodCreation(Company,"administrator","statutory","KWD","company-gregorian",Guid.NewGuid().ToString("N"),code,new DateTime(2026,10,1),new DateTime(2026,10,31),Guid.NewGuid().ToString("N"),"Explicit company calendar period",empty.Revision,new string('B',64));
        var row=command.CreateRow();row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CREATED_DATE=new DateTime(2026,10,1,0,0,0,DateTimeKind.Utc);row.AUTHORITY_JSON=CompanyLedgerPeriodAuthority.Create(command,row);return row;
    }
    internal sealed class Fixture
    {
        public readonly CompanyGLAccountReviewTests.Fixture Native=new();
        public readonly PeriodClosingService Service;
        public COMPANY_LEDGER_PERIOD Period=Row();
        public Fixture()
        {
            Native.Native.Rows.Clear();Native.Native.Rows.Add(Period);Native.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>{
                var schema=new ClassCreator(Native.Native.Editor.Object).ConvertToEntityStructure(typeof(COMPANY_LEDGER_PERIOD));Native.ChangeSchema?.Invoke(schema);return schema;});
            var columns=new CommonColumnHandler();var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadataMock=new Mock<IPPDMMetadataRepository>();metadataMock.Setup(value=>value.GetPrimaryKeyColumnsAsync("COMPANY_LEDGER_PERIOD")).ReturnsAsync(new List<string>{"COMPANY_LEDGER_PERIOD_ID"});var metadata=metadataMock.Object;var editor=Native.Native.Editor.Object;
            var journal=new JournalEntryService(editor,columns,defaults,metadata,Native.Service,NullLogger<JournalEntryService>.Instance,()=>Task.FromResult(Native.Native.Connection));
            var basis=new AccountingBasisPostingService(journal);var ap=new APInvoiceService(editor,columns,defaults,metadata,basis,NullLogger<APInvoiceService>.Instance,resolveConnection:()=>Task.FromResult(Native.Native.Connection));
            var ar=new ARService(editor,columns,defaults,metadata,basis,Mock.Of<IFailureReporter>(),NullLogger<ARService>.Instance,resolveConnection:()=>Task.FromResult(Native.Native.Connection));
            var trial=new TrialBalanceService(editor,columns,defaults,metadata,Native.Service,NullLogger<TrialBalanceService>.Instance);
            Service=new(editor,columns,defaults,metadata,trial,journal,basis,ap,ar,NullLogger<PeriodClosingService>.Instance,resolveConnection:()=>Task.FromResult(Native.Native.Connection));
        }
        public Task<CompanyLedgerPeriodReview> Read()=>Service.GetCompanyPeriodsAsync(Company,"accountant","statutory","KWD","company-gregorian",Native.Authority);
    }
    [Fact] public async Task CompleteRecordedPeriodSourceResolvesOneExplicitOpenCalendarDateWithoutInferringChecklistAuthority()
    {
        var fixture=new Fixture();var review=await fixture.Read();review.Require(Company,"accountant","statutory","KWD","company-gregorian");Assert.Equal(fixture.Period.COMPANY_LEDGER_PERIOD_ID,review.RequireOpen(Date).COMPANY_LEDGER_PERIOD_ID);
        Assert.Throws<InvalidOperationException>(()=>review.RequireOpen(Date.AddMonths(1)));Assert.Equal(2,fixture.Native.Reads);fixture.Native.NoWrites();
    }
    [Fact] public async Task EmptyIsUnavailableForPostingAndProviderFailureIsNotAnOpenPeriod()
    {
        var fixture=new Fixture();fixture.Native.Native.Rows.Clear();var empty=await fixture.Read();Assert.Empty(empty.Periods);Assert.Throws<InvalidOperationException>(()=>empty.RequireOpen(Date));fixture.Native.NullRows=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.Native.NoWrites();
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("book")][InlineData("currency")][InlineData("calendar")][InlineData("inactive")][InlineData("date")][InlineData("closed")][InlineData("locked")][InlineData("receipt")][InlineData("duplicate")]
    public async Task ForeignUnconfirmedClosedOrAmbiguousPeriodFactsNeverSupplyPostingAuthority(string fault)
    {
        var fixture=new Fixture();switch(fault){case "tenant":fixture.Period.OWNER_TENANT_ID="other";break;case "company":fixture.Period.OWNER_COMPANY_BA_ID="other";break;case "book":fixture.Period.BOOK_ID="other";break;case "currency":fixture.Period.CURRENCY_CODE="USD";break;
            case "calendar":fixture.Period.CALENDAR_ID="another-calendar";break;case "inactive":fixture.Period.ACTIVE_IND="N";break;case "date":fixture.Period.START_DATE=Date.AddHours(1);break;case "closed":fixture.Period.STATUS="CLOSED";break;case "locked":fixture.Period.STATUS="LOCKED";break;case "receipt":fixture.Period.AUTHORITY_JSON="{}";break;case "duplicate":fixture.Native.OverrideRows=[fixture.Period,fixture.Period];break;}
        fixture.Native.OverrideRows ??=[fixture.Period];await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());fixture.Native.NoWrites();
    }
    [Theory][InlineData("key")][InlineData("proof")][InlineData("date")]
    public async Task NativeBeepPeriodMetadataMustBeConfirmedBeforeAnyPrivateRead(string fault)
    {
        var fixture=new Fixture();fixture.Native.ChangeSchema=schema=>{if(fault=="key")schema.Fields.Single(field=>field.IsKey).IsKey=false;
            else if(fault=="proof"){var field=schema.Fields.Single(field=>field.FieldName=="AUTHORITY_JSON");field.Size=field.Size1=1;}else schema.Fields.Single(field=>field.FieldName=="END_DATE").AllowDBNull=true;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.Native.Reads);fixture.Native.NoWrites();
    }
    [Theory][InlineData("binding")][InlineData("period")][InlineData("authority")]
    public async Task ClosingBindingPeriodOrAuthorityChangeCannotPublish(string fault)
    {
        var fixture=new Fixture();fixture.Native.OnRead=()=>{if(fixture.Native.Reads!=2)return;if(fault=="binding")fixture.Native.Native.Connection="changed";if(fault=="period")fixture.Period.PERIOD_CODE="changed";if(fault=="authority")fixture.Native.RevokeAt=fixture.Native.AuthorityCalls+1;};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.Native.NoWrites();
    }
    [Fact] public void PeriodCodesAndInclusiveRangesCannotOverlapOrBeSilentlySelectedByOrder()
    {
        var first=Row();var second=Row("FY26-other");Assert.Throws<InvalidOperationException>(()=>CompanyLedgerPeriodReview.Create(Company,"accountant","statutory","KWD","company-gregorian",[first,second]));
        var row=Row();var review=CompanyLedgerPeriodReview.Create(Company,"accountant","statutory","KWD","company-gregorian",[row]);review.Periods[0].PERIOD_CODE="tampered";Assert.Throws<InvalidOperationException>(()=>review.RequireOpen(Date));
    }
    [Theory][InlineData("check")][InlineData("close")][InlineData("reopen")]
    public async Task CompanylessChecklistAndClosingJournalsCannotClaimDurablePeriodTransitions(string operation)
    {
        var fixture=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(async()=>{if(operation=="check")await fixture.Service.ValidatePeriodCloseAsync(Date);else if(operation=="close")await fixture.Service.ClosePeriodAsync(Date,"October");else await fixture.Service.ReopenPeriodAsync(Date);});
        Assert.Equal(0,fixture.Native.Reads);fixture.Native.NoWrites();Assert.Equal("OPEN",fixture.Period.STATUS);
    }
}
#endif
