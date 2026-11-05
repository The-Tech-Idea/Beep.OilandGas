#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodHttpTests
{
    [Fact] public async Task SuccessfulNativeAcknowledgementWithFailedPublicPeriodSourceRetainsExactOriginalCommand()
    {
        var native=new CompanyLedgerPeriodCreationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var actual=CompanyGLAccountHttpTests.Client(host.Client,company);var wrapper=new Mock<IAccountingServiceClient>();
        wrapper.Setup(client=>client.GetCompanyLedgerPeriodConfigurationReviewAsync("statutory","KWD","company-gregorian")).Returns(()=>actual.GetCompanyLedgerPeriodConfigurationReviewAsync("statutory","KWD","company-gregorian"));
        wrapper.Setup(client=>client.CreateCompanyLedgerPeriodAsync(It.IsAny<CompanyLedgerPeriodCreationSubmission>())).Returns((CompanyLedgerPeriodCreationSubmission submission)=>actual.CreateCompanyLedgerPeriodAsync(submission));
        wrapper.Setup(client=>client.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian")).ThrowsAsync(new InvalidOperationException("Public period source unavailable"));
        using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);await state.LoadPeriodConfigurationReviewAsync("statutory","KWD","company-gregorian",wrapper.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.CreatePeriodAsync("FY26-M11",new DateTime(2026,11,1),new DateTime(2026,11,30),"Reviewed original period",wrapper.Object));Assert.True(state.HasPendingPeriod);Assert.Null(state.PeriodResult);Assert.Equal(1,native.Reader.Native.Native.Writes);var saved=JsonSerializer.Serialize(state.PendingPeriod);
        state.DetachOwner(owner);company.Clear();Assert.Null(state.PendingPeriod);Assert.True(state.HasPendingPeriod);company.TrySelect(company.Version,"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.ConfigureLedger"]));state.AttachOwner(new object());Assert.Equal(saved,JsonSerializer.Serialize(state.PendingPeriod));
        wrapper.Setup(client=>client.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian")).Returns(()=>actual.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian"));await state.SubmitPeriodAsync(wrapper.Object);Assert.False(state.HasPendingPeriod);Assert.NotNull(state.PeriodResult);Assert.Equal(1,native.Reader.Native.Native.Writes);
    }
    private static CompanyLedgerPeriodCreationSubmission Submission(CompanyLedgerPeriodConfigurationReview review)=>new(new(review.History.Company,review.History.ActorUserId,review.History.BookId,review.History.CurrencyCode,review.History.CalendarId,Guid.NewGuid().ToString("N"),"FY26-M11",new DateTime(2026,11,1),new DateTime(2026,11,30),Guid.NewGuid().ToString("N"),"Reviewed original company calendar period",review.History.Revision,review.BindingRevision),review.History);
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("auth")][InlineData("role")][InlineData("permission")][InlineData("schema")][InlineData("revoked")][InlineData("binding")][InlineData("changed")][InlineData("calendar")]
    public async Task ActualApiAndTypedClientPublishOnlyRecordedPublicPeriodFacts(string fault)
    {
        var native=new CompanyLedgerPeriodReadTests.Fixture();if(fault=="empty")native.Native.Native.Rows.Clear();if(fault=="schema")native.Native.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="AUTHORITY_JSON"));
        await using var host=await AccountingSourceHttpTests.Start(authenticate:fault!="auth",role:fault!="role",permissions:fault=="permission"?[]:["Accounting.View"],configureAccounting:(services,context)=>{
            services.AddSingleton(native.Service);services.AddSingleton(native.Native.Service);context.PolicyBinding=new string('B',64);native.Native.OnRead=()=>{if(fault=="revoked" && native.Native.Reads==1)context.Permissions=[];if(fault=="binding" && native.Native.Reads==1)context.PolicyBinding=new string('C',64);if(fault=="changed" && native.Native.Reads==2)native.Period.PERIOD_CODE="changed";};});
        using var company=CompanyGLAccountHttpTests.Selected();var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        if(fault is "valid" or "empty")
        {
            var source=await client.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian");source.Require(new("tenant","company"),"actor","statutory","KWD","company-gregorian");Assert.Equal(fault=="empty"?0:1,source.Periods.Count);
            var json=JsonSerializer.Serialize(source);Assert.DoesNotContain("AUTHORITY_JSON",json);Assert.DoesNotContain("BindingRevision",json);Assert.DoesNotContain("IntentId",json);
        }
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyLedgerPeriodsAsync("statutory","KWD",fault=="calendar"?"another-calendar":"company-gregorian"));Assert.Equal(fault=="auth"?HttpStatusCode.Unauthorized:fault is "role" or "permission" or "revoked" or "binding"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);}
        if(fault is "auth" or "role" or "permission" or "schema")Assert.Equal(0,native.Native.Reads);native.Native.NoWrites();
    }
    [Fact] public async Task AuthorizedOriginalPeriodRequestCommitsAndRecoversThroughActualApiWithoutAnotherWrite()
    {
        var native=new CompanyLedgerPeriodCreationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyLedgerPeriodConfigurationReviewAsync("statutory","KWD","company-gregorian"));
        native.Reader.Native.Native.FailCommit=true;var unavailable=await Assert.ThrowsAsync<OilGasApiException>(()=>client.CreateCompanyLedgerPeriodAsync(submission));Assert.Equal(HttpStatusCode.Conflict,unavailable.StatusCode);
        native.Reader.Native.Native.FailCommit=false;Assert.Equal(submission.Command,await client.GetCompanyLedgerPeriodCreationRequestAsync("statutory","KWD","company-gregorian",submission.Command.PeriodId));
        var result=await client.CreateCompanyLedgerPeriodAsync(submission with{Review=null});Assert.Equal(submission.Command,CompanyLedgerPeriodAuthority.Require(result).Command);Assert.Equal(1,native.Reader.Native.Native.Writes);Assert.Equal(2,(await client.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian")).Periods.Count);
        host.Context.Actor="other";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","other");company.TrySelect(company.Clear(),"other",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.ConfigureLedger"]));
        var forbidden=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetCompanyLedgerPeriodCreationRequestAsync("statutory","KWD","company-gregorian",submission.Command.PeriodId));Assert.Equal(HttpStatusCode.Forbidden,forbidden.StatusCode);
    }
    [Theory][InlineData("grant")][InlineData("actor")][InlineData("company")][InlineData("binding")]
    public async Task UnconfirmedConfigurationAuthorityNeverCreatesAPeriod(string fault)
    {
        var native=new CompanyLedgerPeriodCreationTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(native.Reader.Service);services.AddSingleton(native.Reader.Native.Service);context.PolicyBinding=new string('B',64);});
        using var company=CompanyGLAccountHttpTests.Selected(grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=Submission(await client.GetCompanyLedgerPeriodConfigurationReviewAsync("statutory","KWD","company-gregorian"));
        if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="actor")submission=submission with{Command=submission.Command with{ActorUserId="other"}};if(fault=="company")submission=submission with{Command=submission.Command with{Company=new("tenant","other")}};if(fault=="binding")submission=submission with{Command=submission.Command with{BindingRevision=new string('C',64)}};
        await Assert.ThrowsAnyAsync<Exception>(()=>client.CreateCompanyLedgerPeriodAsync(submission));Assert.Equal(0,native.Reader.Native.Native.Writes);
    }
    [Theory][InlineData("account")][InlineData("company")][InlineData("grant")]
    public async Task ClientDiscardsLatePeriodSourceAfterCompanyAccountOrGrantChange(string fault)
    {
        var native=new CompanyLedgerPeriodReadTests.Fixture();using var company=CompanyGLAccountHttpTests.Selected();native.Native.OnRead=()=>{if(native.Native.Reads==1)company.TrySelect(company.Clear(),fault=="account"?"other":"actor",new(new("tenant",fault=="company"?"other":"company"),["ConfiguredCompanyRole"],fault=="grant"?[]:["Accounting.View"]));};
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,context)=>{services.AddSingleton(native.Service);services.AddSingleton(native.Native.Service);context.PolicyBinding=new string('B',64);});
        await Assert.ThrowsAnyAsync<Exception>(()=>CompanyGLAccountHttpTests.Client(host.Client,company).GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian"));native.Native.NoWrites();
    }
}
#endif
