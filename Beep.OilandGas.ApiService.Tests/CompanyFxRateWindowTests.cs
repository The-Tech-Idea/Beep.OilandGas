using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using TheTechIdea.Beep;
using System.Net;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyFxRateWindowTests
{
    internal static void Seed(CompanyFxRateTests.Fixture f,int count)
    {
        for(var index=0;index<count;index++){var date=f.Date.AddDays(index-count);var command=f.Submission.Command with{Id=Guid.NewGuid().ToString("N"),IntentId=Guid.NewGuid().ToString("N"),Version="historical-"+index,QuoteDate=date,EffectiveFrom=date,EffectiveThrough=date,ObservedAtUtc=DateTime.SpecifyKind(date,DateTimeKind.Utc)};var row=command.CreateRow();row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.CONFIGURATION_JSON=CompanyFxRateProof.Create(command,row);f.Books.Reader.Native.Rows.Add(row);}
    }
    [Fact] public async Task MoreThan128DailyQuotesRemainReadableAndNewQuotePreservesAllHistoricalRows()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();Seed(f,150);var history=JsonSerializer.Serialize(f.Books.Reader.Native.Rows.OfType<FX_RATE>());var date=f.Date.AddDays(-100);var dated=await f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",date,f.Books.Reader.Authority);dated.RequireWindow(date,date);Assert.Single(dated.Rates);Assert.Equal(date,dated.RequireApplicable().EffectiveFrom);
        var range=await f.Service.GetCompanyFxRateWindowAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",date,date.AddDays(2),f.Books.Reader.Authority);Assert.Equal(3,range.Rates.Count);Assert.Empty((await f.Read()).Rates);var recorded=await f.Record();Assert.Equal(f.Submission.Command,CompanyFxRateProof.Require(recorded).Command);Assert.Equal(151,f.Books.Reader.Native.Rows.OfType<FX_RATE>().Count());Assert.Equal(history,JsonSerializer.Serialize(f.Books.Reader.Native.Rows.OfType<FX_RATE>().Where(row=>row.FX_RATE_ID!=recorded.FX_RATE_ID)));Assert.Equal(1,f.Books.Reader.Native.Writes);
        var calls=f.Books.Reader.Native.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="FX_RATE").Select(call=>(List<AppFilter>)call.Arguments[1]).ToArray();Assert.Contains(calls,filters=>filters.Any(value=>value.FieldName=="EFFECTIVE_FROM" && value.Operator=="<=") && filters.Any(value=>value.FieldName=="EFFECTIVE_THROUGH" && value.Operator==">="));
    }
    [Fact] public async Task VersionOutsideWindowAndInactiveVersionStillReserveIdentityWithoutNewInsertion()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();Seed(f,1);var previous=f.Books.Reader.Native.Rows.OfType<FX_RATE>().Single();var original=CompanyFxRateProof.Require(previous).Command;var candidate=f.Submission.Command with{Version=original.Version};f.Submission=f.Submission with{Command=candidate};Assert.Empty((await f.Read()).Rates);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Books.Reader.Native.Writes);
        previous=f.Books.Reader.Native.Rows.OfType<FX_RATE>().Single();previous.ACTIVE_IND="N";await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Books.Reader.Native.Writes);
    }
    [Theory][InlineData("start")][InlineData("end")][InlineData("outside")][InlineData("overlap")][InlineData("inverted")]
    public async Task InclusiveBoundariesAndProviderWindowViolationsAreVerified(string fault)
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();await f.Record();if(fault=="start" || fault=="end"){var quote=f.Books.Reader.Native.Rows.OfType<FX_RATE>().Single();var c=CompanyFxRateProof.Require(quote).Command with{EffectiveThrough=f.Date.AddDays(2)};quote.EFFECTIVE_THROUGH=c.EffectiveThrough;quote.CONFIGURATION_JSON=CompanyFxRateProof.Create(c,quote);var day=fault=="start"?f.Date:f.Date.AddDays(2);Assert.Single((await f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",day,f.Books.Reader.Authority)).Rates);return;}
        if(fault=="outside"){f.Books.Reader.Native.Source.Setup(source=>source.GetEntityAsync("FX_RATE",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>f.Books.Reader.Native.Rows.OfType<FX_RATE>().Select(row=>(object)CompanyInventorySourceReview.Copy(row)).ToArray());var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",f.Date.AddDays(1),f.Books.Reader.Authority));Assert.Contains("outside",error.Message);return;}
        if(fault=="overlap"){var command=f.Submission.Command with{Id=Guid.NewGuid().ToString("N"),IntentId=Guid.NewGuid().ToString("N"),Version="overlap"};var row=command.CreateRow();row.ROW_CREATED_BY=command.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.CONFIGURATION_JSON=CompanyFxRateProof.Create(command,row);f.Books.Reader.Native.Rows.Add(row);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());return;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.GetCompanyFxRateWindowAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",f.Date.AddDays(1),f.Date,f.Books.Reader.Authority));
    }
    [Fact] public async Task OriginalWholeRegistryReceiptAndCommandRecoverWithoutReinterpretingScope()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();Seed(f,2);var books=await f.Books.Read();var legacy=CompanyFxRateReview.Create(books,"statutory","USD",f.Date,f.Books.Reader.Native.Rows.OfType<FX_RATE>());Assert.Null(legacy.Window);Assert.DoesNotContain("Window",JsonSerializer.Serialize(legacy));f.Submission=new(f.Submission.Command with{SourceRevision=legacy.Revision},legacy);await f.Record();var original=f.Submission.Command;f.Submission=new(original,null);await f.Record();Assert.Equal(1,f.Books.Reader.Native.Writes);var recovered=await f.Service.GetCompanyFxRateConfigurationAsync(original.Company,original.ActorUserId,original.Id,f.Books.Reader.Authority);Assert.Equal(original,CompanyFxRateProof.Require(recovered!).Command);
        var wide=CompanyFxRateReview.Create(books,"statutory","USD",f.Date,f.Books.Reader.Native.Rows.OfType<FX_RATE>());var oldConversion=CompanyFxConversion.Create(wide,2,10,8,1,1);var json=JsonSerializer.Serialize(oldConversion);Assert.DoesNotContain("Window",json);var restored=JsonSerializer.Deserialize<CompanyFxConversion>(json)!;restored.Require(wide,2,10,8,1,1);Assert.Equal(json,JsonSerializer.Serialize(restored));var scoped=await f.Read();scoped.RequireWindow(f.Date,f.Date);Assert.NotEqual(wide.Revision,scoped.Revision);Assert.Throws<InvalidOperationException>(()=>restored.Require(scoped,2,10,8,1,1));
    }
    [Fact] public async Task VersionReservationAddedOutsideWindowAfterInsertionRollsBackExactRate()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();f.OnInsert=()=>{var c=f.Submission.Command with{Id=Guid.NewGuid().ToString("N"),IntentId=Guid.NewGuid().ToString("N"),QuoteDate=f.Date.AddDays(-1),EffectiveFrom=f.Date.AddDays(-1),EffectiveThrough=f.Date.AddDays(-1)};var row=c.CreateRow();row.ROW_CREATED_BY=c.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.CONFIGURATION_JSON=CompanyFxRateProof.Create(c,row);f.Books.Reader.Native.Rows.Add(row);};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Empty(f.Books.Reader.Native.Rows.OfType<FX_RATE>());Assert.Equal("rollback",f.Books.Reader.Native.Transactions.Last());
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task NewIntervalEvidenceBeforeOrAfterInsertionCannotLendOriginalReviewedSource(bool after)
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();void Add(){var c=f.Submission.Command with{Id=Guid.NewGuid().ToString("N"),IntentId=Guid.NewGuid().ToString("N"),Version="other-overlap"};var row=c.CreateRow();row.ROW_CREATED_BY=c.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.CONFIGURATION_JSON=CompanyFxRateProof.Create(c,row);f.Books.Reader.Native.Rows.Add(row);}if(after)f.OnInsert=Add;else Add();await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(after?1:0,f.Books.Reader.Native.Writes);Assert.Equal(after?0:1,f.Books.Reader.Native.Rows.OfType<FX_RATE>().Count());Assert.Equal("rollback",f.Books.Reader.Native.Transactions.Last());
    }
    [Fact] public async Task ActualApiIntervalSourceHistoryAndUncertainRecoveryKeepOriginalWindowAndBody()
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();Seed(f,150);await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Service);context.PolicyBinding=new string('B',64);});using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var end=f.Date.AddDays(2);var source=await client.GetCompanyFxRateConfigurationReviewAsync("statutory","USD",f.Date,end);source.Source.RequireWindow(f.Date,end);Assert.Empty(source.Source.Rates);var command=CompanyFxRateTests.Command(source.Source) with{EffectiveThrough=end};var submission=new CompanyFxRateConfigurationSubmission(command,source.Source);f.Books.Reader.Native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>client.ConfigureCompanyFxRateAsync(submission));f.Books.Reader.Native.FailCommit=false;var row=await client.ConfigureCompanyFxRateAsync(submission with{Review=null});Assert.Equal(command,CompanyFxRateProof.Require(row).Command);Assert.Equal(1,f.Books.Reader.Native.Writes);Assert.Equal(151,f.Books.Reader.Native.Rows.OfType<FX_RATE>().Count());Assert.Single((await client.GetCompanyFxRatesAsync("statutory","USD",end)).Rates);Assert.Empty((await client.GetCompanyFxRatesAsync("statutory","USD",end.AddDays(1))).Rates);Assert.Single((await client.GetCompanyFxRatesAsync("statutory","USD",f.Date.AddDays(-140))).Rates);
        host.Context.Permissions=["Accounting.View"];Assert.Equal(command,CompanyFxRateProof.Require((await client.GetCompanyFxRateConfigurationAsync(command.Id))!).Command);var denied=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyFxRateConfigurationReviewAsync("statutory","USD",f.Date,end));Assert.Equal(HttpStatusCode.Forbidden,denied.StatusCode);
    }
    [Theory][InlineData("company")][InlineData("currency")][InlineData("book")]
    public async Task ProviderCannotLendForeignCompanyBookOrCurrencyRowsToTheWindow(string fault)
    {
        var f=new CompanyFxRateTests.Fixture();await f.Prepare();await f.Record();if(fault=="book"){await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","other-book","USD",f.Date,f.Books.Reader.Authority));return;}
        var row=f.Books.Reader.Native.Rows.OfType<FX_RATE>().Single();if(fault=="company"){var c=CompanyFxRateProof.Require(row).Command with{Company=new("other-tenant","other-company")};row.OWNER_TENANT_ID=c.Company.TenantId;row.OWNER_COMPANY_BA_ID=c.Company.BusinessAssociateId;row.CONFIGURATION_JSON=CompanyFxRateProof.Create(c,row);}
        f.Books.Reader.Native.Source.Setup(source=>source.GetEntityAsync("FX_RATE",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>new object[]{CompanyInventorySourceReview.Copy(row)});await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","statutory",fault=="currency"?"EUR":"USD",f.Date,f.Books.Reader.Authority));Assert.Equal(1,f.Books.Reader.Native.Writes);
    }
}
#endif
