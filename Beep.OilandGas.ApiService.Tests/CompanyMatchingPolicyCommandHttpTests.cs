#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMatchingPolicyCommandHttpTests
{
    private static CompanyClientContext Selected(string actor="actor",string role="ConfiguredCompanyRole",params string[] grants)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),[role],new[]{"Accounting.View"}.Concat(grants).ToArray()));return company;}
    private static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    private static CompanyMatchingPolicySubmission Draft(CompanyMatchingPolicyCommandReview review,CompanyMatchingPolicyCommandTests.Fixture f)=>new(
        new(review.History.Company,review.History.ActorUserId,"new-policy","DRAFT",CompanyMatchingPolicyReview.Rules(f.ReadOwner.Row),f.ReadOwner.Row.EFFECTIVE_DATE!.Value,null,
            review.History.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Explicit matching policy prepared for company controller review."),review);
    [Fact] public async Task SeparateAccountantAndControllerUseActualPolicyReviewApiClientNativeWriterAndExactRetries()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareMatchingPolicy"],configureAccounting:(services,context)=>
        {services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);context.Actor="maker";context.Roles=["AccountantRole"];});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","maker");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","AccountantRole");
        using var maker=Selected("maker","AccountantRole","Accounting.PrepareMatchingPolicy");var preparer=Client(host.Client,maker);
        var draft=Draft(await preparer.GetCompanyMatchingPolicyCommandReviewAsync("DRAFT"),native);var recorded=await preparer.RecordCompanyMatchingPolicyAsync(draft);
        recorded.Require(draft);(await preparer.RecordCompanyMatchingPolicyAsync(draft)).Require(draft);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
        host.Context.Actor="controller";host.Context.Roles=["ControllerRole"];host.Context.Permissions=["Accounting.View","Accounting.ApproveMatchingPolicy"];
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","controller");
        host.Client.DefaultRequestHeaders.Remove("X-Test-Assigned-Role");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ControllerRole");
        using var controller=Selected("controller","ControllerRole","Accounting.ApproveMatchingPolicy");var reviewer=Client(host.Client,controller);
        var review=await reviewer.GetCompanyMatchingPolicyCommandReviewAsync("APPROVE");var row=Assert.Single(review.History.Records);
        var approval=new CompanyMatchingPolicySubmission(new(review.History.Company,"controller",row.ACCOUNTING_METHOD_ID,"APPROVE",CompanyMatchingPolicyReview.Rules(row),row.EFFECTIVE_DATE!.Value,row.EXPIRY_DATE,
            review.History.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Independently reviewed the original explicit policy and dates."),review);
        var approved=await reviewer.RecordCompanyMatchingPolicyAsync(approval);approved.Require(approval);(await reviewer.RecordCompanyMatchingPolicyAsync(approval)).Require(approval);
        Assert.Equal("maker",approved.Policy.ROW_CREATED_BY);Assert.Equal("controller",approved.Policy.ROW_CHANGED_BY);Assert.Equal("APPROVED",approved.Policy.MATCHING_POLICY_STATUS);
        Assert.Equal(2,native.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",CompanyMatchingPolicyProof.Require(approval.Command.Company,approved.Policy).Original!.MATCHING_POLICY_STATUS);
        Assert.Equal("DRAFT",native.ReadOwner.Native.Invoice.STATUS);Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("action")][InlineData("binding")]
    public async Task PolicyCommandPreviewRequiresExplicitCurrentActionAuthority(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(role:mode!="role",permissions:mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.PrepareMatchingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=mode=="binding"?"invalid":new string('B',64);});
        using var company=Selected();var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>Client(host.Client,company).GetCompanyMatchingPolicyCommandReviewAsync(mode=="action"?"APPROVE":"DRAFT"));
        Assert.Equal(mode=="binding"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,failure.StatusCode);Assert.Equal(0,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("company")][InlineData("grant")][InlineData("binding")][InlineData("history")][InlineData("action")]
    public async Task PolicySubmissionCannotRebindOriginalEvidenceOrAuthority(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareMatchingPolicy"],configureAccounting:(services,context)=>
        {services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected();var client=Client(host.Client,company);var submission=Draft(await client.GetCompanyMatchingPolicyCommandReviewAsync("DRAFT"),native);
        var command=submission.Command;command=mode switch{"actor"=>command with{ActorUserId="other"},"company"=>command with{Company=new("tenant","other")},
            "binding"=>command with{BindingRevision=new string('C',64)},"history"=>command with{ExpectedHistoryRevision=new string('C',64)},"action"=>command with{Action="APPROVE"},_=>command};
        if(mode=="grant")host.Context.Permissions=["Accounting.View"];
        var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/ap/matching-policies/commands",submission with{Command=command});
        Assert.Equal(mode=="history"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("revoked")][InlineData("binding")][InlineData("corrupt")][InlineData("commit")]
    public async Task NativeOutcomeAndPostWriteAuthorityAreCheckedByActualHttpCommand(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareMatchingPolicy"],configureAccounting:(services,context)=>
        {services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected();var client=Client(host.Client,company);var submission=Draft(await client.GetCompanyMatchingPolicyCommandReviewAsync("DRAFT"),native);
        if(mode=="revoked")native.OnWrite=()=>host.Context.Permissions=["Accounting.View"];if(mode=="binding")native.OnWrite=()=>host.Context.ChangeBinding=true;
        if(mode=="corrupt")native.Corrupt=true;if(mode=="commit")native.ReadOwner.Native.Native.FailCommit=true;
        var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordCompanyMatchingPolicyAsync(submission));
        Assert.Equal(mode is "revoked" or "binding"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);
        if(mode=="commit")
        {native.ReadOwner.Native.Native.FailCommit=false;var writes=native.ReadOwner.Native.Native.Writes;(await client.RecordCompanyMatchingPolicyAsync(submission)).Require(submission);Assert.Equal(writes,native.ReadOwner.Native.Native.Writes);}
        else Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>());
    }
    [Fact] public async Task AdditionalApprovalGrantCannotAuthorizeTheDraftPreparersOwnApproval()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareMatchingPolicy","Accounting.ApproveMatchingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected();var client=Client(host.Client,company);var draft=Draft(await client.GetCompanyMatchingPolicyCommandReviewAsync("DRAFT"),native);
        await client.RecordCompanyMatchingPolicyAsync(draft);var review=await client.GetCompanyMatchingPolicyCommandReviewAsync("APPROVE");
        var approval=new CompanyMatchingPolicySubmission(draft.Command with{Action="APPROVE",ExpectedHistoryRevision=review.History.Revision,IntentId=Guid.NewGuid().ToString("N"),Reason="Attempted independent approval."},review);
        var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/ap/matching-policies/commands",approval);
        Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal("DRAFT",native.Stored.MATCHING_POLICY_STATUS);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
    }
}
#endif
