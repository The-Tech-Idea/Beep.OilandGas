#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyBillingPolicyScreenTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    private sealed class Authentication:AuthenticationStateProvider
    {
        public string Actor="actor";public bool Role=true;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            new[]{new Claim(ClaimTypes.NameIdentifier,Actor)}.Concat(Role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"))));
        public void Change(string actor){Actor=actor;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Components:IComponentActivator
    {
        public Beep.OilandGas.Web.Components.Workflow.BillingPolicyWorkbench? Workbench;
        public IComponent CreateInstance(Type type)
        {var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.BillingPolicyWorkbench workbench)Workbench=workbench;return component;}
    }
    private static CompanyClientContext Selected(string actor="actor",params string[] grants)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(Company,["ConfiguredCompanyRole"],new[]{"Accounting.View"}.Concat(grants).ToArray()));return company;}
    private static async Task Seed(CompanyMatchingPolicyCommandTests.Fixture native,string maker)
    {
        var history=await native.Owner.GetCompanyBillingPoliciesAsync(Company,maker,native.ReadOwner.Native.Authority);
        await native.Owner.PrepareCompanyBillingPolicyAsync(new(Guid.NewGuid().ToString("N"),Company,maker,"sales-policy",CompanyBillingPolicyTests.Rules,
            "Original explicit rules prepared for independent review.",history.Revision,new string('B',64)),history,native.ReadOwner.Native.Authority);
    }
    [Theory][InlineData("draft")][InlineData("approve")][InlineData("self")][InlineData("unknown")][InlineData("recreated")][InlineData("approve-unknown")]
    [InlineData("empty")][InlineData("precision")][InlineData("rounding")][InlineData("date-time")][InlineData("expiry")][InlineData("reason")][InlineData("multiline-reason")][InlineData("permission")]
    public async Task CompleteBillingPolicyPageUsesActualApiClientNativeWriteAndExactRetainedRequest(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();var action=mode is "approve" or "self" or "approve-unknown"?"APPROVE":"DRAFT";
        if(action=="APPROVE")await Seed(native,mode=="self"?"actor":"maker");var grant=action=="DRAFT"?"Accounting.PrepareBillingPolicy":"Accounting.ApproveBillingPolicy";
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View",grant],configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected(grants:[grant]);var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        services.AddSingleton(company);services.AddScoped<CompanyBillingPolicyWorkspaceState>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var state=provider.GetRequiredService<CompanyBillingPolicyWorkspaceState>();string? unknownHtml=null;string? originalIntent=null;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.BillingPolicies>();var workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;
            void Refresh()=>workbench.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null);
            async Task Invoke(string method){await (Task)workbench.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(workbench,null)!;Refresh();}
            void Field(string name,object? value)=>workbench.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(workbench,value);
            Assert.Contains("Sales billing policy preparation and review",root.ToHtmlString());await Invoke(action=="DRAFT"?"ReviewDraft":"ReviewApproval");Assert.NotNull(state.Review);
            if(mode!="empty")
            {
                Field("_policyId","sales-policy");Field("_version","sales-v1");Field("_currency","KWD");Field("_decimals",mode=="precision"?null:(int?)3);
                Field("_rounding",mode=="rounding"?"DEFAULT":"TO_EVEN");Field("_from",mode=="date-time"?new DateTime(2026,1,1,12,0,0):new DateTime(2026,1,1));
                Field("_through",mode=="expiry"?new DateTime(2025,12,31):new DateTime(2026,12,31));Field("_tax","EXPLICIT_INVOICE_TAX_REVIEW");Field("_charges","EXPLICIT_INVOICE_CHARGES_REVIEW");Field("_rights","CONTRACT_ENTITLEMENT_REVIEW");
                Field("_reason",mode=="reason"?"":mode=="multiline-reason"?"Invalid\nmultiline reason":"Exact explicit currency and independent review reason.");Refresh();
            }
            if(action=="APPROVE" && mode!="self")state.SelectDraft("sales-policy");if(mode=="permission")host.Context.Permissions=["Accounting.View"];
            if(mode is "unknown" or "recreated" or "approve-unknown")native.ReadOwner.Native.Native.FailCommit=true;await Invoke("Record");
            if(mode is "unknown" or "recreated" or "approve-unknown")
            {
                Assert.True(state.HasPendingIntent);Assert.Null(state.Result);originalIntent=state.PendingIntentId;unknownHtml=root.ToHtmlString();
                if(mode=="recreated")
                {workbench.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.BillingPolicies>();workbench=((Components)provider.GetRequiredService<IComponentActivator>()).Workbench!;}
                native.ReadOwner.Native.Native.FailCommit=false;Field("_reason","Changed input cannot replace retained reason.");await Invoke("Retry");
                var proof=CompanyBillingPolicyProof.Require(Company,state.Result!.Policy);Assert.Equal(originalIntent,action=="DRAFT"?proof.Draft.IntentId:proof.Approval!.IntentId);
                Assert.Equal("Exact explicit currency and independent review reason.",action=="DRAFT"?proof.Draft.Reason:proof.Approval!.Reason);
            }
            return root.ToHtmlString();
        });
        if(mode is "draft" or "approve" or "unknown" or "recreated" or "approve-unknown")
        {Assert.NotNull(state.Result);Assert.False(state.HasPendingIntent);Assert.Contains("outcome confirmed in complete company history",html);Assert.Equal(action=="DRAFT"?1:2,native.ReadOwner.Native.Native.Writes);Assert.Contains(action=="DRAFT"?"Independent approval has not been recorded":"Independent reviewer: actor",html);Assert.Contains("Currency: KWD",html);Assert.Contains("2026-01-01",html);}
        else{Assert.Null(state.Result);Assert.Equal(mode=="self"?1:0,native.ReadOwner.Native.Native.Writes);Assert.DoesNotContain("outcome confirmed in complete company history",html);}
        if(mode is "unknown" or "recreated" or "approve-unknown")Assert.Contains("original request is retained",unknownHtml);
        if(mode is "empty" or "precision" or "rounding" or "date-time" or "expiry" or "reason" or "multiline-reason")Assert.False(state.HasPendingIntent);
        if(mode=="permission")Assert.True(state.HasPendingIntent);if(mode=="self")Assert.Contains("No verified draft prepared by another account",html);
        Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<INVOICE>());Assert.Equal("DRAFT",native.ReadOwner.Native.Invoice.STATUS);
    }
    [Theory][InlineData("account")][InlineData("role")][InlineData("permission")][InlineData("clear")]
    public async Task ScreenRequiresMatchingAuthenticatedAccountStandardRoleAndCurrentReadGrant(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected();if(mode=="clear")company.Clear();if(mode=="permission")company.TrySelect(company.Clear(),"actor",new(Company,["ConfiguredCompanyRole"],[]));
        var authentication=new Authentication{Actor=mode=="account"?"other":"actor",Role=mode!="role"};var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyBillingPolicyWorkspaceState>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(authentication);
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>(await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.BillingPolicyWorkbench>()).ToHtmlString());
        Assert.Contains("Select the company and account with current accounting access",html);Assert.DoesNotContain("History revision:",html);Assert.Equal(0,host.Context.Calls);Assert.Equal(0,native.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task AccountNotificationClearsAllRenderedEvidenceAndControlsImmediately()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();await Seed(native,"maker");await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","Accounting.ApproveBillingPolicy"],configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);});
        using var company=Selected(grants:["Accounting.ApproveBillingPolicy"]);var authentication=new Authentication();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyBillingPolicyWorkspaceState>();services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(authentication);
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.BillingPolicyWorkbench>();Assert.Contains("sales-policy",root.ToHtmlString());
            authentication.Change("other");Assert.Null(provider.GetRequiredService<CompanyBillingPolicyWorkspaceState>().History);Assert.DoesNotContain("sales-policy",root.ToHtmlString());Assert.DoesNotContain("Review independent approval authority",root.ToHtmlString());
        });
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("owner")][InlineData("clear")]
    public async Task WorkspaceDropsLateReadEvidenceAfterContextOrPresentationInvalidation(string mode)
    {
        using var company=Selected();using var state=new CompanyBillingPolicyWorkspaceState(company);var owner=new object();state.AttachOwner(owner);
        var pending=new TaskCompletionSource<CompanyBillingPolicyReview>();var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanyBillingPoliciesAsync()).Returns(pending.Task);
        var load=state.LoadHistoryAsync(client.Object);
        if(mode=="company")company.TrySelect(company.Clear(),"actor",new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View"]));
        if(mode=="actor")company.TrySelect(company.Clear(),"other",new(Company,["ConfiguredCompanyRole"],["Accounting.View"]));if(mode=="owner")state.DetachOwner(owner);if(mode=="clear")state.Invalidate();
        pending.SetResult(CompanyBillingPolicyReview.Create(Company,"actor",[]));await load;Assert.Null(state.History);Assert.False(state.Busy);
    }
    [Fact] public async Task ForeignContextCannotExposeOrReplaceRetainedUncertainPolicyCommand()
    {
        using var company=Selected(grants:["Accounting.PrepareBillingPolicy"]);using var state=new CompanyBillingPolicyWorkspaceState(company);state.AttachOwner(new object());
        var history=CompanyBillingPolicyReview.Create(Company,"actor",[]);var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
        client.Setup(value=>value.GetCompanyBillingPolicyCommandReviewAsync("DRAFT")).ReturnsAsync(new CompanyBillingPolicyCommandReview(history,"DRAFT",new string('B',64)));
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).ThrowsAsync(new InvalidOperationException("Unknown outcome"));
        await state.LoadReviewAsync("DRAFT",client.Object);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync("sales-policy",CompanyBillingPolicyTests.Rules,"Original reason",client.Object));
        var intent=state.PendingDraft!.Command;Assert.True(state.HasPendingIntent);state.PendingDraft!.Command.Rules.Require();
        company.TrySelect(company.Clear(),"actor",new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareBillingPolicy"]));
        Assert.True(state.HasPendingIntent);Assert.False(state.CanRetryPending);Assert.Null(state.PendingDraft);Assert.Null(state.PendingIntentId);Assert.Null(state.PendingPolicyId);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReviewAsync("DRAFT",client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitPendingAsync(client.Object));
        company.TrySelect(company.Clear(),"actor",new(Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareBillingPolicy"]));Assert.Equal(intent,state.PendingDraft!.Command);
        Assert.True(state.CanRetryPending);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadReviewAsync("DRAFT",client.Object));client.Verify(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>()),Times.Once);
    }
    [Theory][InlineData("null")][InlineData("actor")][InlineData("company")][InlineData("content")]
    public async Task WorkspaceCannotAdoptUnconfirmedTransportedHistory(string mode)
    {
        using var company=Selected();using var state=new CompanyBillingPolicyWorkspaceState(company);state.AttachOwner(new object());
        var history=CompanyBillingPolicyReview.Create(Company,"actor",[]);history=mode switch{"actor"=>history with{ActorUserId="other"},"company"=>history with{Company=new("tenant","other")},"content"=>history with{Revision=new string('C',64)},_=>history};
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanyBillingPoliciesAsync()).ReturnsAsync(mode=="null"?null!:history);
        await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadHistoryAsync(client.Object));Assert.Null(state.History);Assert.False(state.Busy);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("owner")]
    public async Task LateSuccessfulWriteDoesNotPublishAfterInvalidationAndOriginalIntentCanBeReconciled(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected(grants:["Accounting.PrepareBillingPolicy"]);using var state=new CompanyBillingPolicyWorkspaceState(company);var owner=new object();state.AttachOwner(owner);
        var history=await native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority);var ack=new TaskCompletionSource<CompanyBillingPolicyResult>();CompanyBillingPolicyDraftSubmission? sent=null;
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanyBillingPolicyCommandReviewAsync("DRAFT")).ReturnsAsync(new CompanyBillingPolicyCommandReview(history,"DRAFT",new string('B',64)));
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).Returns((CompanyBillingPolicyDraftSubmission value)=>{sent=PurchaseReceiptDecisionProof.Copy(value);return ack.Task;});
        await state.LoadReviewAsync("DRAFT",client.Object);var submit=state.PrepareAsync("sales-policy",CompanyBillingPolicyTests.Rules,"Original reviewed reason",client.Object);Assert.NotNull(sent);
        var durable=await native.Owner.PrepareCompanyBillingPolicyAsync(sent!.Command,sent.Review.History,native.ReadOwner.Native.Authority);
        if(mode=="owner")state.DetachOwner(owner);else company.TrySelect(company.Clear(),mode=="actor"?"other":"actor",new(mode=="company"?new("tenant","other"):Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareBillingPolicy"]));
        ack.SetResult(new(durable));await submit;Assert.Null(state.Result);Assert.Null(state.History);Assert.True(state.HasPendingIntent);Assert.False(state.CanRetryPending);
        if(mode=="owner")state.AttachOwner(new object());else company.TrySelect(company.Clear(),"actor",new(Company,["ConfiguredCompanyRole"],["Accounting.View","Accounting.PrepareBillingPolicy"]));
        Assert.Equal(sent.Command,state.PendingDraft!.Command);
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).Returns(async(CompanyBillingPolicyDraftSubmission value)=>new CompanyBillingPolicyResult(await native.Owner.PrepareCompanyBillingPolicyAsync(value.Command,value.Review.History,native.ReadOwner.Native.Authority)));
        client.Setup(value=>value.GetCompanyBillingPoliciesAsync()).Returns(()=>native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority));
        await state.SubmitPendingAsync(client.Object);Assert.NotNull(state.Result);Assert.False(state.HasPendingIntent);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("history")][InlineData("outcome")]
    public async Task CorruptedConfirmationCannotClearExactRetainedPolicyIntent(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected(grants:["Accounting.PrepareBillingPolicy"]);using var state=new CompanyBillingPolicyWorkspaceState(company);state.AttachOwner(new object());
        var history=await native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority);CompanyBillingPolicyDraftSubmission? sent=null;var corrupt=true;
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanyBillingPolicyCommandReviewAsync("DRAFT")).ReturnsAsync(new CompanyBillingPolicyCommandReview(history,"DRAFT",new string('B',64)));
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).Returns(async(CompanyBillingPolicyDraftSubmission value)=>
        {sent=PurchaseReceiptDecisionProof.Copy(value);var row=await native.Owner.PrepareCompanyBillingPolicyAsync(value.Command,value.Review.History,native.ReadOwner.Native.Authority);if(corrupt && mode=="outcome")row.DESCRIPTION="altered returned outcome";return new CompanyBillingPolicyResult(row);});
        client.Setup(value=>value.GetCompanyBillingPoliciesAsync()).Returns(async()=>
        {var current=await native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority);if(corrupt && mode=="history")current.Records[0].DESCRIPTION="altered transported history";return current;});
        await state.LoadReviewAsync("DRAFT",client.Object);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync("sales-policy",CompanyBillingPolicyTests.Rules,"Original reason",client.Object));
        Assert.Null(state.Result);Assert.Null(state.History);Assert.True(state.HasPendingIntent);Assert.Equal(sent!.Command,state.PendingDraft!.Command);
        corrupt=false;await state.SubmitPendingAsync(client.Object);Assert.NotNull(state.Result);Assert.False(state.HasPendingIntent);Assert.Equal(1,native.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("grant")][InlineData("roles")][InlineData("disposed")]
    public async Task RetainedCommandDetailsAreHiddenWhenReadAuthorityOrCircuitIsGone(string mode)
    {
        using var company=Selected(grants:["Accounting.PrepareBillingPolicy"]);using var state=new CompanyBillingPolicyWorkspaceState(company);state.AttachOwner(new object());
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);client.Setup(value=>value.GetCompanyBillingPolicyCommandReviewAsync("DRAFT")).ReturnsAsync(new CompanyBillingPolicyCommandReview(CompanyBillingPolicyReview.Create(Company,"actor",[]),"DRAFT",new string('B',64)));
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).ThrowsAsync(new InvalidOperationException("Unknown outcome"));
        await state.LoadReviewAsync("DRAFT",client.Object);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync("sales-policy",CompanyBillingPolicyTests.Rules,"Original reason",client.Object));
        if(mode=="disposed")state.Dispose();else company.TrySelect(company.Clear(),"actor",new(Company,mode=="roles"?[]:["ConfiguredCompanyRole"],mode=="grant"?[]:["Accounting.View","Accounting.PrepareBillingPolicy"]));
        Assert.True(state.HasPendingIntent);Assert.Null(state.PendingDraft);Assert.Null(state.PendingIntentId);Assert.False(state.CanRetryPending);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitPendingAsync(client.Object));
    }
    [Fact] public async Task ScreenStateConfirmsRetainedOriginalOutcomeAfterAnotherAccountAddsPolicyHistory()
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();using var company=Selected(grants:["Accounting.PrepareBillingPolicy"]);using var state=new CompanyBillingPolicyWorkspaceState(company);state.AttachOwner(new object());
        var history=await native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority);var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
        client.Setup(value=>value.GetCompanyBillingPolicyCommandReviewAsync("DRAFT")).ReturnsAsync(new CompanyBillingPolicyCommandReview(history,"DRAFT",new string('B',64)));
        client.Setup(value=>value.PrepareCompanyBillingPolicyAsync(It.IsAny<CompanyBillingPolicyDraftSubmission>())).Returns(async(CompanyBillingPolicyDraftSubmission value)=>new CompanyBillingPolicyResult(await native.Owner.PrepareCompanyBillingPolicyAsync(value.Command,value.Review.History,native.ReadOwner.Native.Authority)));
        client.Setup(value=>value.GetCompanyBillingPoliciesAsync()).Returns(()=>native.Owner.GetCompanyBillingPoliciesAsync(Company,"actor",native.ReadOwner.Native.Authority));
        await state.LoadReviewAsync("DRAFT",client.Object);native.ReadOwner.Native.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.PrepareAsync("sales-policy",CompanyBillingPolicyTests.Rules,"Original reason",client.Object));var original=state.PendingDraft!.Command;
        native.ReadOwner.Native.Native.FailCommit=false;var later=await native.Owner.GetCompanyBillingPoliciesAsync(Company,"another-maker",native.ReadOwner.Native.Authority);
        await native.Owner.PrepareCompanyBillingPolicyAsync(new(Guid.NewGuid().ToString("N"),Company,"another-maker","later-policy",CompanyBillingPolicyTests.Rules,"Another independently prepared version",later.Revision,new string('B',64)),later,native.ReadOwner.Native.Authority);
        await state.SubmitPendingAsync(client.Object);Assert.False(state.HasPendingIntent);Assert.Equal(original,CompanyBillingPolicyProof.Require(Company,state.Result!.Policy).Draft);
        Assert.Equal(2,state.History!.Records.Count);Assert.Equal(2,native.ReadOwner.Native.Native.Writes);
    }
}
#endif
