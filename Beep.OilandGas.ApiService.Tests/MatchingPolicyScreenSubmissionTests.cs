#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
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
public sealed class MatchingPolicyScreenSubmissionTests
{
    [Theory][InlineData("draft")][InlineData("landing")][InlineData("approve")][InlineData("unknown")][InlineData("recreated")][InlineData("approve-unknown")][InlineData("empty")][InlineData("utc")][InlineData("partial-utc")][InlineData("offset")][InlineData("reason")][InlineData("permission")][InlineData("self")]
    public async Task CompletePayablesScreenUsesActualPolicyReviewSubmissionAndReconciliation(string mode)
    {
        var native=new CompanyMatchingPolicyCommandTests.Fixture();var action=mode is "approve" or "approve-unknown" or "self"?"APPROVE":"DRAFT";
        var actor=mode is "approve" or "approve-unknown"?"controller":"actor";
        if(mode is "approve" or "approve-unknown" or "self"){await native.PrepareDraft(mode=="self"?actor:"maker");await native.Record();}
        var grant=action=="DRAFT"?"Accounting.PrepareMatchingPolicy":"Accounting.ApproveMatchingPolicy";
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View",grant],configureAccounting:(services,context)=>{services.AddSingleton(native.Owner);context.PolicyBinding=new string('B',64);context.Actor=actor;});
        if(actor!="actor")host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View",grant]));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddSingleton(Mock.Of<IPersonaContextService>());services.AddSingleton<IUnifiedTaskInboxService>(new UnifiedTaskInboxService(api));
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var state=provider.GetRequiredService<APInvoiceWorkspaceState>();string? unknownHtml=null;string? originalIntent=null;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",mode=="landing"?null:"invoice"}}));
            var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method){await (Task)page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;page.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);}
            void Field(string name,object value)=>page.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,value);
            await Invoke(action=="DRAFT"?"ReviewPolicyDraft":"ReviewPolicyApproval");Assert.NotNull(state.PolicyCommandReview);
            if(mode!="empty")
            {
                Field("_policyId","new-policy");Field("_policyVersion","v1");Field("_policyCurrency","KWD");Field("_policyQuantityTolerance",(decimal?)0);Field("_policyPriceTolerance",(decimal?)1);
                Field("_policyEffectiveUtc",mode switch{"utc"=>"2026-01-01","partial-utc"=>"00:00:00Z","offset"=>"2026-01-01T00:00:00+03:00",_=>"2026-01-01T00:00:00Z"});Field("_policyUnitRule","EXACT_RECORDED_UNITS");Field("_policyCurrencyRule","EXACT_RECORDED_CURRENCY");
                Field("_policyTaxRule","SEPARATE_REVIEW_REQUIRED");Field("_policyRoundingRule","EXACT_SOURCE_AMOUNTS");Field("_policyNonPoRule","SEPARATE_REVIEW_REQUIRED");
                Field("_policyReason",mode=="reason"?"":"Explicit policy preparation or independent review reason.");
            }
            if(mode is "approve" or "approve-unknown")state.SelectPolicyDraft("new-policy");if(mode=="permission")host.Context.Permissions=["Accounting.View"];
            if(mode is "unknown" or "recreated" or "approve-unknown")native.ReadOwner.Native.Native.FailCommit=true;
            await Invoke("RecordPolicy");
            if(mode is "unknown" or "recreated" or "approve-unknown")
            {
                Assert.True(state.HasUnconfirmedPolicyIntent);Assert.Null(state.PolicyResult);unknownHtml=root.ToHtmlString();originalIntent=state.PolicyIntent!.Command.IntentId;
                if(mode=="recreated")
                {
                    ((IDisposable)page).Dispose();Assert.True(state.HasUnconfirmedPolicyIntent);
                    root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
                    page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
                }
                native.ReadOwner.Native.Native.FailCommit=false;Field("_policyReason","Changed input must not replace original reason.");await Invoke("RecordPolicy");
                Assert.Equal(originalIntent,state.PolicyResult!.Command.IntentId);Assert.Equal("Explicit policy preparation or independent review reason.",state.PolicyResult.Command.Reason);
            }
            return root.ToHtmlString();
        });
        if(mode is "draft" or "landing" or "approve" or "unknown" or "recreated" or "approve-unknown")
        {
            Assert.NotNull(state.PolicyResult);Assert.False(state.HasUnconfirmedPolicyIntent);Assert.Contains("outcome confirmed in complete company history",html);
            Assert.Equal(action=="APPROVE"?"APPROVED":"DRAFT",state.PolicyResult!.Policy.MATCHING_POLICY_STATUS);Assert.Equal(action=="APPROVE"?2:1,native.ReadOwner.Native.Native.Writes);
            Assert.Contains(action=="APPROVE"?"Verified independent policy reviewer":"Verified draft preparer",html);
        }
        else
        {Assert.Null(state.PolicyResult);Assert.Equal(mode=="self"?1:0,native.ReadOwner.Native.Native.Writes);Assert.DoesNotContain("outcome confirmed in complete company history",html);}
        if(mode is "unknown" or "recreated" or "approve-unknown")Assert.Contains("Policy outcome is unconfirmed",unknownHtml);
        if(mode is "empty" or "utc" or "partial-utc" or "offset" or "reason")Assert.False(state.HasUnconfirmedPolicyIntent);
        if(mode=="permission")Assert.True(state.HasUnconfirmedPolicyIntent);
        if(mode=="self")Assert.Contains("No verified active draft prepared by another account",html);
        Assert.Equal("DRAFT",native.ReadOwner.Native.Invoice.STATUS);Assert.Empty(native.ReadOwner.Native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
}
#endif
