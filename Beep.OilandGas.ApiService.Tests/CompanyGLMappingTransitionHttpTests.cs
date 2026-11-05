#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingTransitionHttpTests
{
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("grant")][InlineData("role")][InlineData("binding")][InlineData("company")][InlineData("source")]
    public async Task ApiClientUsesCurrentAuthorityAndExactNativeTransitionRecovery(string fault)
    {
        var f=new CompanyGLMappingTransitionTests.Fixture();await f.Prepare(true);var native=f.Creation.Reader.Accounts.Native;
        await using var host=await AccountingSourceHttpTests.Start(role:fault!="role",permissions:["Accounting.View","Accounting.ConfigureLedger"],configureAccounting:(services,context)=>{services.AddSingleton(f.Creation.Reader.Service);services.AddSingleton(f.Creation.Reader.Accounts.Service);context.Actor="administrator";context.PolicyBinding=new string('B',64);});host.Client.DefaultRequestHeaders.Add("X-Test-Actor","administrator");
        using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var submission=f.Submission;
        if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="binding")submission=submission with{Command=submission.Command with{BindingRevision=new string('C',64),Replacement=submission.Command.Replacement! with{BindingRevision=new string('C',64)}}};if(fault=="company")submission=submission with{Command=submission.Command with{Company=new("tenant","foreign"),Replacement=submission.Command.Replacement! with{Company=new("tenant","foreign")}}};if(fault=="source")f.Creation.Reader.Accounts.Row.ACCOUNT_NAME="Changed native account";
        if(fault is "grant" or "role" or "binding" or "company" or "source"){using var refused=await host.Client.PostAsJsonAsync("api/accounting/traditional/gl/company-accounts/mappings/transitions",submission);Assert.Equal(fault=="source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,refused.StatusCode);Assert.Equal(0,native.Writes);return;}
        if(fault=="uncertain"){native.FailCommit=true;var unknown=await Assert.ThrowsAsync<OilGasApiException>(()=>client.TransitionCompanyGLMappingAsync(submission));Assert.Equal(HttpStatusCode.Conflict,unknown.StatusCode);Assert.Equal(2,native.Writes);native.FailCommit=false;}
        var outcome=await client.TransitionCompanyGLMappingAsync(submission);outcome.Require(submission.Command);Assert.Equal(2,native.Writes);var recovery=await client.GetCompanyGLMappingTransitionAsync("statutory","KWD",submission.Command.MappingId);Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(recovery));
        host.Context.Permissions=["Accounting.View"];var readonlyRecovery=await client.GetCompanyGLMappingTransitionAsync("statutory","KWD",submission.Command.MappingId);Assert.NotNull(readonlyRecovery);Assert.Equal(2,native.Writes);
    }
    [Fact] public async Task CircuitKeepsOriginalTransitionAfterLostRecoveryAndReconcilesAcrossPresentationRecreation()
    {
        var f=new CompanyGLMappingTransitionTests.Fixture();await f.Prepare(true);using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View","Accounting.ConfigureLedger"]);var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).ReturnsAsync(new CompanyGLMappingConfigurationReview(f.Submission.Review!,f.Submission.Command.BindingRevision));
        client.Setup(value=>value.TransitionCompanyGLMappingAsync(It.IsAny<CompanyGLMappingTransitionSubmission>())).Returns((CompanyGLMappingTransitionSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyGLMappingTransitionAsync("statutory","KWD",It.IsAny<string>())).Returns((string book,string currency,string id)=>lost?Task.FromException<CompanyGLMappingTransitionOutcome?>(new InvalidOperationException("Lost recovery")):f.Creation.Reader.Service.GetCompanyMappingTransitionAsync(f.Submission.Command.Company,"administrator",book,currency,id,f.Creation.Reader.Accounts.Authority));
        client.Setup(value=>value.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).Returns(()=>f.Creation.Reader.Service.GetCompanyMappingsAsync(f.Submission.Command.Company,"administrator","statutory","KWD",CompanyGLMappingReviewTests.Date,f.Creation.Reader.Accounts.Authority));
        using var state=new CompanyLedgerAccountState(company);var owner=new object();state.AttachOwner(owner);await state.LoadMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date,client.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.TransitionMappingAsync(f.Submission.Command.MappingId,f.Submission.Command.EffectiveThrough,"actual-account-id",null,"Original explicit replacement",client.Object));Assert.True(state.HasPendingMappingTransition);Assert.Equal(2,f.Creation.Reader.Accounts.Native.Writes);var original=JsonSerializer.Serialize(state.PendingMappingTransition!.Command);
        state.DetachOwner(owner);Assert.Null(state.PendingMappingTransition);state.AttachOwner(new object());Assert.Equal(original,JsonSerializer.Serialize(state.PendingMappingTransition!.Command));lost=false;await state.SubmitMappingTransitionAsync(client.Object);Assert.False(state.HasPendingLedgerRequest);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(2,f.Creation.Reader.Accounts.Native.Writes);
    }
}
#endif
