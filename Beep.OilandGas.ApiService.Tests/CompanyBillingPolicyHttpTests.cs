using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyBillingPolicyHttpTests
{
    private static CompanyClientContext Selected(string actor="actor",string role="ConfiguredCompanyRole",params string[] grants)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),[role],new[]{"Accounting.View"}.Concat(grants).ToArray()));return company;}
    private static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    private static CompanyBillingPolicyDraftSubmission Draft(CompanyBillingPolicyCommandReview review)=>new(
        new(Guid.NewGuid().ToString("N"),review.History.Company,review.History.ActorUserId,"sales-policy",CompanyBillingPolicyTests.Rules,
            "Explicit currency precision and invoice review rules prepared.",review.History.Revision,review.BindingRevision),review);
    [Fact] public async Task CompanyMakerAndIndependentControllerUseActualApiClientAndNativePolicyOwnerWithExactRetries()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareBillingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var selected=Selected(grants:["Accounting.PrepareBillingPolicy"]);var client=Client(host.Client,selected);
        Assert.Empty((await client.GetCompanyBillingPoliciesAsync()).Records);
        var draft=Draft(await client.GetCompanyBillingPolicyCommandReviewAsync("DRAFT"));(await client.PrepareCompanyBillingPolicyAsync(draft)).Require(draft);(await client.PrepareCompanyBillingPolicyAsync(draft)).Require(draft);
        Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
        host.Context.Actor="controller";host.Context.Roles=["ControllerRole"];host.Context.Permissions=["Accounting.View","Accounting.ApproveBillingPolicy"];
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","controller");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ControllerRole");
        using var controller=Selected("controller","ControllerRole","Accounting.ApproveBillingPolicy");var reviewer=Client(host.Client,controller);
        var review=await reviewer.GetCompanyBillingPolicyCommandReviewAsync("APPROVE");var row=Assert.Single(review.History.Records);
        var approval=new CompanyBillingPolicyApprovalSubmission(new(Guid.NewGuid().ToString("N"),review.History.Company,"controller",row.ACCOUNTING_METHOD_ID,
            CompanyBillingPolicyProof.Require(review.History.Company,row).RowRevision,"Independent policy and calendar review completed.",review.History.Revision,review.BindingRevision),review);
        var result=await reviewer.ApproveCompanyBillingPolicyAsync(approval);result.Require(approval);(await reviewer.ApproveCompanyBillingPolicyAsync(approval)).Require(approval);
        Assert.Equal(2,native.ReadOwner.Native.Native.Writes);Assert.Equal("actor",result.Policy.ROW_CREATED_BY);Assert.Equal("controller",result.Policy.ROW_CHANGED_BY);
        Assert.Equal("APPROVED",result.Policy.MATCHING_POLICY_STATUS);Assert.Equal("DRAFT",CompanyBillingPolicyProof.Require(review.History.Company,result.Policy).Original!.MATCHING_POLICY_STATUS);
        Assert.Equal("DRAFT",native.ReadOwner.Native.Invoice.STATUS);Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<INVOICE>());
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("AP-permission")][InlineData("action")][InlineData("binding")][InlineData("anonymous")]
    public async Task PolicyReviewRequiresCurrentCompanyRoleAndDistinctBillingGrant(string fault)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(authenticate:fault!="anonymous",role:fault!="role",
            permissions:fault=="permission"?["Accounting.View"]:fault=="AP-permission"?["Accounting.View","Accounting.PrepareMatchingPolicy"]:["Accounting.View","Accounting.PrepareBillingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=fault=="binding"?"invalid":new string('B',64);});
        using var selected=Selected();var error=await Assert.ThrowsAsync<OilGasApiException>(()=>Client(host.Client,selected).GetCompanyBillingPolicyCommandReviewAsync(fault=="action"?"APPROVE":"DRAFT"));
        Assert.Equal(fault=="binding"?HttpStatusCode.Conflict:fault=="anonymous"?HttpStatusCode.Unauthorized:HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("company")][InlineData("grant")][InlineData("binding")][InlineData("history")][InlineData("review-action")]
    public async Task TransportedCommandCannotRebindReviewedCompanyActorBindingHistoryOrAction(string fault)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareBillingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var selected=Selected();var submission=Draft(await Client(host.Client,selected).GetCompanyBillingPolicyCommandReviewAsync("DRAFT"));
        submission=submission with{Command=fault switch{"actor"=>submission.Command with{ActorUserId="other"},"company"=>submission.Command with{Company=new("tenant","other")},
            "binding"=>submission.Command with{BindingRevision=new string('C',64)},"history"=>submission.Command with{HistoryRevision=new string('C',64)},_=>submission.Command}};
        if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="review-action")submission=submission with{Review=submission.Review with{Action="APPROVE"}};
        var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/billing-policies/drafts",submission);
        Assert.Equal(fault is "history" or "review-action"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(0,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("roles")][InlineData("grant")][InlineData("binding")][InlineData("actor")]
    public async Task AuthorityChangeAtProviderWriteRollsBackThroughApiAndRetainsOriginalCommand(string fault)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareBillingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var selected=Selected();var submission=Draft(await Client(host.Client,selected).GetCompanyBillingPolicyCommandReviewAsync("DRAFT"));
        native.OnWrite=()=>{if(fault=="roles")host.Context.Roles=["changed"];if(fault=="grant")host.Context.Permissions=["Accounting.View"];if(fault=="binding")host.Context.PolicyBinding=new string('C',64);if(fault=="actor")host.Context.Actor="changed";};
        var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/billing-policies/drafts",submission);Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);
        Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<ACCOUNTING_METHOD>());Assert.Equal("rollback",native.ReadOwner.Native.Native.Transactions.Last());Assert.NotEmpty(submission.Command.IntentId);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task NativeUncertainCommitCanBeReconciledThroughSameApiClientSubmission(bool laterHistory)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.PrepareBillingPolicy"],
            configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var selected=Selected();var client=Client(host.Client,selected);var submission=Draft(await client.GetCompanyBillingPolicyCommandReviewAsync("DRAFT"));
        native.ReadOwner.Native.Native.FailCommit=true;var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanyBillingPolicyAsync(submission));Assert.Equal(HttpStatusCode.Conflict,error.StatusCode);
        native.ReadOwner.Native.Native.FailCommit=false;
        if(laterHistory)
        {
            var current=await native.Owner.GetCompanyBillingPoliciesAsync(submission.Command.Company,"actor",native.ReadOwner.Native.Authority);
            await native.Owner.PrepareCompanyBillingPolicyAsync(new(Guid.NewGuid().ToString("N"),current.Company,"actor","later-policy",CompanyBillingPolicyTests.Rules,"A later reviewed policy version",current.Revision,new string('B',64)),current,native.ReadOwner.Native.Authority);
        }
        (await client.PrepareCompanyBillingPolicyAsync(submission)).Require(submission);Assert.Equal(laterHistory?2:1,native.ReadOwner.Native.Native.Writes);
    }
}
#endif
