#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingTransitionScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"administrator"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator{public Beep.OilandGas.Web.Components.Workflow.MappingTransitionWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.MappingTransitionWorkbench workbench)Workbench=workbench;return component;}}
    [Fact] public async Task RequiredAdministratorInputsAndOriginalRecoverySurviveLostFollowupAndRecreation()
    {
        var f=new CompanyGLMappingTransitionTests.Fixture();await f.Prepare(true);using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View","Accounting.ConfigureLedger"]);using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyGLMappingConfigurationReviewAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).ReturnsAsync(new CompanyGLMappingConfigurationReview(f.Submission.Review!,f.Submission.Command.BindingRevision));
        client.Setup(value=>value.TransitionCompanyGLMappingAsync(It.IsAny<CompanyGLMappingTransitionSubmission>())).Returns((CompanyGLMappingTransitionSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyGLMappingTransitionAsync("statutory","KWD",It.IsAny<string>())).Returns((string book,string currency,string id)=>lost?Task.FromException<CompanyGLMappingTransitionOutcome?>(new InvalidOperationException("Lost followup")):f.Creation.Reader.Service.GetCompanyMappingTransitionAsync(f.Submission.Command.Company,"administrator",book,currency,id,f.Creation.Reader.Accounts.Authority));
        client.Setup(value=>value.GetCompanyGLMappingsAsync("statutory","KWD",CompanyGLMappingReviewTests.Date)).Returns(()=>f.Creation.Reader.Service.GetCompanyMappingsAsync(f.Submission.Command.Company,"administrator","statutory","KWD",CompanyGLMappingReviewTests.Date,f.Creation.Reader.Accounts.Authority));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"Book","statutory"},{"Currency","KWD"},{"ReviewDate",CompanyGLMappingReviewTests.Date}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.MappingTransitionWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            void Render()=>components.Workbench!.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);
            async Task Action(string name){await(Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;components.Workbench.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);}
            await Action("Review");await Action("Record");Assert.Equal(0,f.Creation.Reader.Accounts.Native.Writes);Assert.False(state.HasPendingMappingTransition);
            Field("_mapping",f.Submission.Command.MappingId);Field("_through",f.Submission.Command.EffectiveThrough);Field("_action","REPLACE");Field("_account","actual-account-id");Field("_reason","Original explicit administrator replacement");Render();await Action("Record");Assert.True(state.HasPendingMappingTransition);Assert.Equal(2,f.Creation.Reader.Accounts.Native.Writes);var original=JsonSerializer.Serialize(state.PendingMappingTransition!.Command);Assert.Contains("original mapping transition remains retained",root.ToHtmlString());
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.MappingTransitionWorkbench>(parameters);Assert.Equal(original,JsonSerializer.Serialize(state.PendingMappingTransition!.Command));lost=false;await Action("Retry");Assert.False(state.HasPendingMappingTransition);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(2,f.Creation.Reader.Accounts.Native.Writes);Assert.Contains("durable receipt are confirmed",root.ToHtmlString());
            company.TrySelect(company.Version,"administrator",new(f.Submission.Command.Company,["ConfiguredCompanyRole"],["Accounting.View"]));Field("_recovery",f.Submission.Command.MappingId);Render();await Action("RecoverSelected");Assert.NotNull(state.MappingTransitionResult);Assert.Equal(2,f.Creation.Reader.Accounts.Native.Writes);
        });
    }
}
#endif
