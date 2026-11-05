using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using System.Security.Claims;
using System.Reflection;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyStepReviewHttpTests
{
    private static readonly string[] Permissions=["Accounting.View","Procurement.View","Workflow.Start","Workflow.Approve"];
    private static async Task<CompanyAccountingProcessStartReceipt> Start(CompanyProcessStartTests.Fixture native,AccountingSourceHttpTests.Host host,string type="AP_INVOICE",string id="invoice",string starter="starter")
    {
        var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/start-review",new CompanyAccountingProcessStartQuery("definition",type,id));response.EnsureSuccessStatusCode();
        var review=(await response.Content.ReadFromJsonAsync<CompanyAccountingProcessStartReview>())!;
        // Native writer under explicit test authority; current HTTP reviewer is a different recorded actor.
        return await native.Start(review.CreateIntent(Guid.NewGuid().ToString("N")) with {ActorUserId=starter});
    }
    private static string Route(string id)=>"/api/accounting/processes/"+Uri.EscapeDataString(id)+"/step-review";
    [Theory][InlineData("AP_INVOICE","invoice")][InlineData("PURCHASE_ORDER","order")]
    public async Task ActualNativeHandoffStepReviewAndTypedClientConfirmCurrentIndependentRole(string type,string id)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;
        await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);var receipt=await Start(native,host,type,id);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var review=await client.ReviewCompanyAccountingStepAsync(receipt.InstanceId);Assert.NotNull(review);review.Require(new("tenant","company"),"actor",receipt.InstanceId);
        Assert.True(review.Eligible);Assert.True(review.RequiresIndependentActor);Assert.Equal("Workflow.Approve",review.RequiredPermission);Assert.Equal("invoice-preparer",review.SourceMakerUserId);
        var process=await client.GetCompanyAccountingInstanceAsync(receipt.InstanceId);Assert.Equal(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(process)),review.ProcessRevision);
        Assert.Equal(6,native.Native.Writes);
    }
    [Theory][InlineData("starter")][InlineData("maker")][InlineData("permission")]
    public async Task ReadableProcessDoesNotImplyIndependentStepAuthority(string fault)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        if(fault=="maker")host.Context.Preparer="actor";
        var receipt=await Start(native,host,starter:fault=="starter"?"actor":"starter");
        if(fault=="permission")host.Context.Permissions=["Accounting.View","Workflow.Start"];
        var response=await host.Client.GetAsync(Route(receipt.InstanceId));Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var review=(await response.Content.ReadFromJsonAsync<CompanyAccountingStepReview>())!;Assert.False(review.Eligible);
        Assert.Equal(fault=="permission"?"WORKFLOW_PERMISSION_REQUIRED":"ROLE_OR_INDEPENDENCE_REQUIRED",review.Eligibility);Assert.Equal(6,native.Native.Writes);
    }
    [Theory][InlineData("definition")][InlineData("source")][InlineData("edited")][InlineData("provider")][InlineData("step")][InlineData("company")][InlineData("role")][InlineData("revoke")][InlineData("intent")]
    public async Task MissingChangedOrDeniedEvidenceCannotPublishStepEligibility(string fault)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);var receipt=await Start(native,host);
        var expected=HttpStatusCode.Conflict;
        switch(fault)
        {
            case "definition":native.Definition.VERSION="2.0.0";break;
            case "source":host.Context.Preparer="different";break;
            case "edited":host.Context.SourceEdited=true;break;
            case "provider":host.Context.FailProvider=true;break;
            case "step":native.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().First().REQUIRED_ROLE="Other";break;
            case "intent":
                var process=native.Native.Rows.OfType<PROCESS_INSTANCE>().Single();
                var data=JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.Process.PROCESS_DATA>(process.INSTANCE_DATA_JSON!)!;
                data.Data[CompanyAccountingProcessStart.PayloadKey]="malformed recorded intent";process.INSTANCE_DATA_JSON=JsonSerializer.Serialize(data);break;
            case "company":host.Client.DefaultRequestHeaders.Add("X-Test-Company","second");break;
            case "role":host.Client.DefaultRequestHeaders.Remove("X-Test-Role");expected=HttpStatusCode.Forbidden;break;
            case "revoke":host.Context.Calls=0;host.Context.OnCapture=count=>{if(count>=4)host.Context.Permissions=["Accounting.View"];};expected=HttpStatusCode.Forbidden;break;
        }
        var response=await host.Client.GetAsync(Route(receipt.InstanceId));Assert.Equal(expected,response.StatusCode);
        var body=await response.Content.ReadAsStringAsync();Assert.DoesNotContain("ELIGIBLE",body);Assert.DoesNotContain("private provider",body);Assert.Equal(6,native.Native.Writes);
    }
    [Theory][InlineData("AP_INVOICE","complete")][InlineData("PURCHASE_ORDER","complete")]
    [InlineData("AP_INVOICE","terminal")][InlineData("AP_INVOICE","reason")]
    [InlineData("AP_INVOICE","permission")][InlineData("AP_INVOICE","reconcile")][InlineData("PURCHASE_ORDER","reconcile")]
    public async Task CompletePageHandlerRecordsOrReconcilesTheOriginalDecision(string type,string mode)
    {
        var (native,command)=await CompanyStepCommandTests.Prepare(terminal:mode=="terminal",type:type);
        await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();
        services.AddSingleton<IBusinessProcessServiceClient>(client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();
        var navigation=new Navigation(command.Review.InstanceId);navigation.InitializeForTest();services.AddSingleton<NavigationManager>(navigation);
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var completion=provider.GetRequiredService<CompanyAccountingStepState>();var before=native.Native.Writes;
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",command.Review.InstanceId}}));
            var detail=((Components)provider.GetRequiredService<IComponentActivator>()).Detail!;
            async Task Invoke(string method){await (Task)detail.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(detail,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(detail,null);}
            void Notes(string value)=>detail.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(detail,value);
            await Invoke("ReviewStep");Assert.Contains("Completion reason",root.ToHtmlString());
            Notes(mode=="reason"?" ":command.Notes);
            if(mode=="permission")host.Context.Permissions=["Accounting.View","Procurement.View"];
            if(mode=="reconcile")host.Context.OnCapture=_=>{if(native.Native.Writes>before)host.Context.SourceEdited=true;};
            await Invoke("CompleteStep");
            if(mode is "reason" or "permission")
            {
                Assert.Null(completion.Receipt);Assert.Equal(before,native.Native.Writes);
                if(mode=="reason")Assert.Null(completion.Intent);else Assert.True(completion.HasUnconfirmedIntent);
                Assert.DoesNotContain("process reload confirmed",root.ToHtmlString());return;
            }
            if(mode=="reconcile")
            {
                Assert.Null(completion.Receipt);Assert.True(completion.HasUnconfirmedIntent);var retained=completion.Intent;
                Assert.Contains("Reconcile original completion",root.ToHtmlString());var committed=native.Native.Writes;
                host.Context.OnCapture=null;host.Context.SourceEdited=false;Notes("A different reason must not replace the retained decision.");
                await Invoke("CompleteStep");Assert.Equal(retained,completion.Intent);Assert.Equal(committed,native.Native.Writes);
            }
            var outcome=Assert.IsType<CompanyAccountingStepReceipt>(completion.Receipt);Assert.False(completion.HasUnconfirmedIntent);
            Assert.Contains("process reload confirmed",root.ToHtmlString());Assert.Contains(outcome.HistoryId,root.ToHtmlString());
            Assert.Equal(mode=="terminal"?0:2,outcome.NextTaskIds.Count);
            Assert.Equal(command.Notes,native.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES);
            var writes=native.Native.Writes;await Invoke("CompleteStep");Assert.Equal(writes,native.Native.Writes);
            company.Clear();typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(detail,null);
            Assert.Null(completion.Receipt);Assert.DoesNotContain("process reload confirmed",root.ToHtmlString());
        });
    }
    private sealed class Authentication : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));
    }
    private sealed class Navigation(string id) : NavigationManager
    {
        public void InitializeForTest()=>Initialize("https://render.invalid/","https://render.invalid/accounting/processes/"+System.Uri.EscapeDataString(id));
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static rendering has no navigation.");
    }
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static rendering has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private sealed class Components(IServiceProvider services) : IComponentActivator
    {
        public IComponent? Detail;
        public IComponent CreateInstance(Type type)
        {
            var component=(IComponent)ActivatorUtilities.CreateInstance(services,type);
            if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail))Detail=component;
            return component;
        }
    }
    [Theory][InlineData("eligible")][InlineData("independent")][InlineData("edited")][InlineData("company")]
    public async Task CompleteProcessPageHandlerShowsReviewedEligibilityOrClearsChangedEvidence(string mode)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");await using var host=await CompanyProcessStartHttpTests.Host(native,permissions:Permissions);
        var receipt=await Start(native,host,starter:mode=="independent"?"actor":"starter");
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],Permissions));
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();
        services.AddSingleton<IBusinessProcessServiceClient>(client);services.AddSingleton<IAccountingServiceClient>(new Mock<IAccountingServiceClient>(MockBehavior.Strict).Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();
        var navigation=new Navigation(receipt.InstanceId);navigation.InitializeForTest();services.AddSingleton<NavigationManager>(navigation);
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",receipt.InstanceId}}));
            if(mode=="edited")host.Context.SourceEdited=true;
            if(mode=="company")company.Clear();
            var detail=((Components)provider.GetRequiredService<IComponentActivator>()).Detail!;
            // Invoke the actual bound handler and rerender; this is not browser interaction evidence.
            await (Task)detail.GetType().GetMethod("ReviewStep",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(detail,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(detail,null);
            return System.Net.WebUtility.HtmlDecode(root.ToHtmlString());});
        if(mode=="company"){Assert.DoesNotContain("Current assigned step eligibility",html);Assert.DoesNotContain("Source maker:",html);}
        else if(mode=="edited"){Assert.Contains("source-maker evidence is unavailable or changed",html);Assert.DoesNotContain("Current role and maker checks permit",html);}
        else
        {
            Assert.Contains("Source maker: invoice-preparer",html);Assert.Contains("Required permission: Workflow.Approve",html);
            Assert.Contains(mode=="eligible"?"Current role and maker checks permit":"independent actor requirement refuses",html);
            Assert.Contains("Financial matching, posting and payment remain separate",html);
        }
        Assert.Equal(6,native.Native.Writes);
    }
}
#endif
