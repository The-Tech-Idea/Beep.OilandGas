using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.Web.Services;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodTransitionHttpTests
{
    private static readonly string[] Grants=["Accounting.View","Accounting.ManagePeriods","Accounting.ReopenPeriods"];
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("role")][InlineData("grant")][InlineData("binding")][InlineData("company")][InlineData("source")][InlineData("reopen-grant")]
    public async Task ActualApiClientControlsIndependentPeriodStateAndReadOnlyOriginalRecovery(string fault)
    {
        var f=new CompanyLedgerPeriodTransitionTests.Fixture();await f.Prepare();var native=f.Creation.Reader.Native.Native;
        if(fault=="reopen-grant"){await f.Record();await f.Review("reopener","OPEN");}
        var actor=f.Submission.Command.ActorUserId;await using var host=await AccountingSourceHttpTests.Start(role:fault!="role",permissions:Grants,configureAccounting:(services,context)=>{services.AddSingleton(f.Creation.Reader.Service);services.AddSingleton(f.Creation.Reader.Native.Service);context.Actor=actor;context.PolicyBinding=new string('B',64);});host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        using var company=CompanyGLAccountHttpTests.Selected(actor:actor,grants:Grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=f.Submission;var writes=native.Writes;
        if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="reopen-grant")host.Context.Permissions=["Accounting.View","Accounting.ManagePeriods"];if(fault=="binding")submission=submission with{Command=submission.Command with{BindingRevision=new string('C',64)}};if(fault=="company")submission=submission with{Command=submission.Command with{Company=new("tenant","foreign")}};if(fault=="source")f.Creation.Reader.Period.PERIOD_CODE="changed";
        if(fault is "role" or "grant" or "binding" or "company" or "source" or "reopen-grant"){using var refused=await host.Client.PostAsJsonAsync("api/accounting/traditional/gl/company-accounts/periods/transitions",submission);Assert.Equal(fault=="source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,refused.StatusCode);Assert.Equal(writes,native.Writes);return;}
        var review=await client.GetCompanyPeriodTransitionReviewAsync("statutory","KWD","company-gregorian");Assert.Equal(submission.Review!.Revision,review.History.Revision);
        if(fault=="uncertain"){native.FailCommit=true;var unknown=await Assert.ThrowsAsync<OilGasApiException>(()=>client.TransitionCompanyPeriodAsync(submission));Assert.Equal(HttpStatusCode.Conflict,unknown.StatusCode);Assert.Equal(1,native.Writes);native.FailCommit=false;}
        var outcome=await client.TransitionCompanyPeriodAsync(submission);outcome.Require(submission.Command);Assert.Equal(1,native.Writes);Assert.Equal("CLOSED",outcome.Period.STATUS);host.Context.Permissions=["Accounting.View"];
        var recovery=await client.GetCompanyPeriodTransitionAsync("statutory","KWD","company-gregorian",submission.Command.PeriodId,submission.Command.IntentId);Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(recovery));Assert.Equal(1,native.Writes);
        var publicSource=await client.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian");var json=JsonSerializer.Serialize(publicSource);Assert.DoesNotContain("AUTHORITY_JSON",json);Assert.DoesNotContain("STATE_HISTORY_JSON",json);Assert.Equal("CLOSED",publicSource.Periods.Single(row=>row.PeriodId==submission.Command.PeriodId).Status);
    }
    [Fact] public async Task CircuitRetainsOriginalPeriodCommandAfterLostRecoveryAndBlocksCompetingLedgerRequests()
    {
        var f=new CompanyLedgerPeriodTransitionTests.Fixture();await f.Prepare();using var company=CompanyGLAccountHttpTests.Selected(actor:"controller",grants:Grants);using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyPeriodTransitionReviewAsync("statutory","KWD","company-gregorian")).ReturnsAsync(new CompanyLedgerPeriodConfigurationReview(f.Submission.Review!,f.Submission.Command.BindingRevision));
        client.Setup(value=>value.TransitionCompanyPeriodAsync(It.IsAny<CompanyLedgerPeriodTransitionSubmission>())).Returns((CompanyLedgerPeriodTransitionSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyPeriodTransitionAsync("statutory","KWD","company-gregorian",It.IsAny<string>(),It.IsAny<string>())).Returns((string book,string currency,string calendar,string id,string intent)=>lost?Task.FromException<CompanyLedgerPeriodTransitionOutcome?>(new InvalidOperationException("Lost followup")):f.Creation.Reader.Service.GetCompanyPeriodTransitionAsync(f.Submission.Command.Company,"controller",book,currency,calendar,id,intent,f.Creation.Reader.Native.Authority));
        client.Setup(value=>value.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian")).Returns(async()=>CompanyLedgerPeriodSource.FromNative(await f.Creation.Reader.Service.GetCompanyPeriodsAsync(f.Submission.Command.Company,"controller","statutory","KWD","company-gregorian",f.Creation.Reader.Native.Authority)));
        await state.LoadPeriodTransitionReviewAsync("statutory","KWD","company-gregorian",client.Object);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ChangePeriodStateAsync(f.Original.COMPANY_LEDGER_PERIOD_ID,"CLOSED","CONFIRMED","Explicit controller checks","Controlled original close",client.Object));Assert.True(state.HasPendingPeriodTransition);Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);var original=JsonSerializer.Serialize(state.PendingPeriodTransition!.Command);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadPeriodTransitionReviewAsync("statutory","KWD","company-gregorian",client.Object));state.Invalidate();Assert.Equal(original,JsonSerializer.Serialize(state.PendingPeriodTransition!.Command));lost=false;await state.SubmitPeriodTransitionAsync(client.Object);Assert.False(state.HasPendingLedgerRequest);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);
    }
}
#endif
