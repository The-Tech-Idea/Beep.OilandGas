using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
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
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerPeriodTransitionScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"controller"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator{public Beep.OilandGas.Web.Components.Workflow.PeriodTransitionWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.PeriodTransitionWorkbench workbench)Workbench=workbench;return component;}}
    [Fact] public async Task IndependentControllerFormRequiresEvidenceAndRetainsOriginalAfterLostRecoveryAndRecreation()
    {
        var f=new CompanyLedgerPeriodTransitionTests.Fixture();await f.Prepare();using var company=CompanyGLAccountHttpTests.Selected(actor:"controller",grants:["Accounting.View","Accounting.ManagePeriods","Accounting.ReopenPeriods"]);using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyPeriodTransitionReviewAsync("statutory","KWD","company-gregorian")).ReturnsAsync(new CompanyLedgerPeriodConfigurationReview(f.Submission.Review!,f.Submission.Command.BindingRevision));
        client.Setup(value=>value.TransitionCompanyPeriodAsync(It.IsAny<CompanyLedgerPeriodTransitionSubmission>())).Returns((CompanyLedgerPeriodTransitionSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyPeriodTransitionAsync("statutory","KWD","company-gregorian",It.IsAny<string>(),It.IsAny<string>())).Returns((string book,string currency,string calendar,string id,string intent)=>lost?Task.FromException<CompanyLedgerPeriodTransitionOutcome?>(new InvalidOperationException("Lost followup")):f.Creation.Reader.Service.GetCompanyPeriodTransitionAsync(f.Submission.Command.Company,"controller",book,currency,calendar,id,intent,f.Creation.Reader.Native.Authority));
        client.Setup(value=>value.GetCompanyLedgerPeriodsAsync("statutory","KWD","company-gregorian")).Returns(async()=>CompanyLedgerPeriodSource.FromNative(await f.Creation.Reader.Service.GetCompanyPeriodsAsync(f.Submission.Command.Company,"controller","statutory","KWD","company-gregorian",f.Creation.Reader.Native.Authority)));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state},{"Book","statutory"},{"Currency","KWD"},{"Calendar","company-gregorian"}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PeriodTransitionWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            void Render()=>components.Workbench!.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);
            async Task Action(string name){await(Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Render();}
            await Action("Review");await Action("Record");Assert.Equal(0,f.Creation.Reader.Native.Native.Writes);Assert.False(state.HasPendingPeriodTransition);
            Field("_period",f.Original.COMPANY_LEDGER_PERIOD_ID);Field("_target","CLOSED");Field("_disposition","CONFIRMED");Field("_evidence","Explicit independent controller checks");Field("_reason","Original reviewed period close");Render();await Action("Record");Assert.True(state.HasPendingPeriodTransition);Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);var original=JsonSerializer.Serialize(state.PendingPeriodTransition!.Command);Assert.Contains("exact original period transition is retained",root.ToHtmlString());
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.PeriodTransitionWorkbench>(parameters);Assert.Equal(original,JsonSerializer.Serialize(state.PendingPeriodTransition!.Command));lost=false;await Action("Retry");Assert.False(state.HasPendingPeriodTransition);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);Assert.Contains("CLOSED period state and durable history are confirmed",root.ToHtmlString());
            company.TrySelect(company.Version,"controller",new(f.Submission.Command.Company,["ConfiguredCompanyRole"],["Accounting.View"]));Field("_recoveryPeriod",f.Submission.Command.PeriodId);Field("_recoveryIntent",f.Submission.Command.IntentId);Render();await Action("RecoverSelected");Assert.NotNull(state.PeriodTransitionResult);Assert.Equal(1,f.Creation.Reader.Native.Native.Writes);
        });
    }
}
#endif
