using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
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
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMatchingPolicyScreenTests
{
    private sealed class PolicyHandler(Task<HttpResponseMessage> response):HttpMessageHandler
    {
        public string? Route;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken){Route=request.RequestUri!.AbsolutePath;return response;}
    }
    private static CompanyClientContext Selected()
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));return company;}
    private static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    [Theory][InlineData("valid")][InlineData("empty")][InlineData("role")][InlineData("permission")][InlineData("binding")][InlineData("revoked")][InlineData("changed")][InlineData("schema")][InlineData("foreign")]
    public async Task ActualPolicyApiAndClientRequireUnchangedCompanyAuthorityAndCompleteEvidence(string mode)
    {
        var native=new CompanyMatchingPolicyTests.Fixture();if(mode=="empty")native.Native.Native.Rows.Remove(native.Row);
        if(mode=="schema")native.Native.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="ACCOUNTING_METHOD"),true)).Returns(new EntityStructure());
        if(mode=="foreign")native.Native.Native.Source.Setup(source=>source.GetEntityAsync("ACCOUNTING_METHOD",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{new ACCOUNTING_METHOD{ACCOUNTING_METHOD_ID="foreign",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="other",METHOD_TYPE="AP_MATCHING"}});
        await using var host=await AccountingSourceHttpTests.Start(role:mode!="role",permissions:mode=="permission"?[]:["Accounting.View"],configureAccounting:(services,context)=>
        {
            services.AddSingleton(native.Owner);
            context.OnCapture=count=>{if(count==3){if(mode=="binding")context.ChangeBinding=true;if(mode=="revoked")context.Permissions=[];if(mode=="changed")native.Row.MATCHING_POLICY_STATUS="RETIRED";}};
        });
        using var company=Selected();var client=Client(host.Client,company);
        if(mode is "valid" or "empty")
        {var review=await client.GetCompanyMatchingPoliciesAsync();review.Require(new("tenant","company"),"actor");Assert.Equal(mode=="empty"?0:1,review.Records.Count);}
        else
        {var failure=await Assert.ThrowsAsync<OilGasApiException>(client.GetCompanyMatchingPoliciesAsync);Assert.Equal(mode is "role" or "permission" or "binding" or "revoked"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);}
        Assert.Equal(0,native.Native.Native.Writes);Assert.Empty(native.Native.Native.Transactions);
        if(mode is "role" or "permission")Assert.DoesNotContain(native.Native.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="ACCOUNTING_METHOD");
    }
    [Theory][InlineData("valid")][InlineData("queue")][InlineData("empty")][InlineData("unavailable")][InlineData("permission")]
    public async Task CompletePayablesPageHandlerReviewsPoliciesThroughActualNativeApiAndTypedClient(string mode)
    {
        var native=new CompanyMatchingPolicyTests.Fixture();if(mode=="empty")native.Native.Native.Rows.Remove(native.Row);
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>services.AddSingleton(native.Owner));
        using var company=Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddSingleton(Mock.Of<IPersonaContextService>());services.AddSingleton<IUnifiedTaskInboxService>(new UnifiedTaskInboxService(api));
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        string? displayedError=null;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",mode=="queue"?null:"invoice"}}));
            if(mode=="unavailable")native.Row.MATCHING_POLICY_JSON="{}";if(mode=="permission")host.Context.Permissions=[];
            var page=provider.GetRequiredService<IComponentActivator>() as APInvoiceFinancialHttpTests.Components;
            await (Task)page!.Page!.GetType().GetMethod("ReviewPolicies",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page.Page,null)!;
            displayedError=(string?)page.Page.GetType().GetField("_policyError",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(page.Page);
            page.Page.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page.Page,null);
            return root.ToHtmlString();
        });
        if(mode is "valid" or "queue"){Assert.Contains("Quantity tolerance (%)",html);Assert.Contains("EXACT_RECORDED_UNITS",html);Assert.Contains("DRAFT",html);Assert.Contains("do not prove independent approval",html);}
        if(mode=="empty")Assert.Contains("No company matching-policy records were returned",html);
        if(mode=="unavailable"){Assert.False(string.IsNullOrWhiteSpace(displayedError));Assert.Contains(displayedError!,html);Assert.DoesNotContain("EXACT_RECORDED_UNITS",html);Assert.Null(provider.GetRequiredService<APInvoiceWorkspaceState>().MatchingPolicies);}
        if(mode=="permission")Assert.DoesNotContain("EXACT_RECORDED_UNITS",html);
        Assert.Equal(0,native.Native.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("clear")][InlineData("later")][InlineData("failed")][InlineData("foreign")][InlineData("actor")]
    public async Task WorkspaceCannotRetainOrAdoptUnconfirmedPolicyEvidence(string mode)
    {
        var native=new CompanyMatchingPolicyTests.Fixture();var review=await native.Owner.GetCompanyMatchingPoliciesAsync(new("tenant","company"),"actor");
        using var company=Selected();using var state=new APInvoiceWorkspaceState(company);await state.LoadMatchingPoliciesAsync(()=>Task.FromResult(review));
        state.MatchingPolicies!.Records[0].MATCHING_POLICY_STATUS="RETIRED";Assert.Equal("DRAFT",state.MatchingPolicies!.Records[0].MATCHING_POLICY_STATUS);
        if(mode=="failed"){await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadMatchingPoliciesAsync(()=>throw new InvalidOperationException("failed")));Assert.Null(state.MatchingPolicies);return;}
        if(mode is "foreign" or "actor")
        {
            var invalid=review with{Company=mode=="foreign"?new("tenant","other"):review.Company,ActorUserId=mode=="actor"?"other":review.ActorUserId};
            await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadMatchingPoliciesAsync(()=>Task.FromResult(invalid)));Assert.Null(state.MatchingPolicies);return;
        }
        var pending=new TaskCompletionSource<CompanyMatchingPolicyReview>();var load=state.LoadMatchingPoliciesAsync(()=>pending.Task);
        if(mode=="company")company.Clear();if(mode=="clear")state.Clear();
        if(mode=="later")await state.LoadMatchingPoliciesAsync(()=>Task.FromResult(CompanyMatchingPolicyReview.Create(review.Company,"actor",[])));
        pending.SetResult(review);await load;
        if(mode=="later")Assert.Empty(state.MatchingPolicies!.Records);else Assert.Null(state.MatchingPolicies);
    }
    [Theory][InlineData("changed")][InlineData("actor")][InlineData("foreign")][InlineData("content")][InlineData("null")]
    public async Task TypedClientRejectsLateOrUnconfirmedPolicyResponse(string mode)
    {
        var native=new CompanyMatchingPolicyTests.Fixture();var review=await native.Owner.GetCompanyMatchingPoliciesAsync(new("tenant","company"),"actor");
        var pending=new TaskCompletionSource<HttpResponseMessage>();using var handler=new PolicyHandler(pending.Task);using var http=new HttpClient(handler){BaseAddress=new("https://policy.invalid/")};
        using var company=Selected();var read=Client(http,company).GetCompanyMatchingPoliciesAsync();
        Assert.Equal("/api/accounting/traditional/ap/matching-policies",handler.Route);
        if(mode=="changed")company.Clear();if(mode=="actor")review=review with{ActorUserId="other"};if(mode=="foreign")review=review with{Company=new("tenant","other")};
        if(mode=="content")review.Records[0].MATCHING_POLICY_STATUS="RETIRED";
        pending.SetResult(new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:review)});
        if(mode=="changed")await Assert.ThrowsAsync<OperationCanceledException>(()=>read);
        else await Assert.ThrowsAsync<InvalidOperationException>(()=>read);
    }
}
#endif
