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
public sealed class CompanyLedgerBookScreenTests
{
    private sealed class Authentication:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"administrator"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));}
    private sealed class Components:IComponentActivator{internal Beep.OilandGas.Web.Components.Workflow.CompanyBookWorkbench? Workbench;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is Beep.OilandGas.Web.Components.Workflow.CompanyBookWorkbench workbench)Workbench=workbench;return component;}}
    [Fact] public async Task FormRequiresExplicitFactsAndRetainsExactBodyUntilNativeAndPublicRecovery()
    {
        var f=new CompanyLedgerBookTests.Fixture();using var company=CompanyGLAccountHttpTests.Selected(actor:"administrator",grants:["Accounting.View","Accounting.ConfigureLedger"]);using var state=new CompanyLedgerAccountState(company);state.AttachOwner(new object());var client=new Mock<IAccountingServiceClient>();var lost=true;var bodies=new List<string>();
        client.Setup(value=>value.GetCompanyLedgerBookConfigurationReviewAsync()).Returns(async()=>new CompanyLedgerBookConfigurationReview(await f.Read(),new string('B',64)));
        client.Setup(value=>value.ConfigureCompanyLedgerBookAsync(It.IsAny<CompanyLedgerBookConfigurationSubmission>())).Returns((CompanyLedgerBookConfigurationSubmission submission)=>{bodies.Add(JsonSerializer.Serialize(submission.Command));f.Submission=submission;return f.Record();});
        client.Setup(value=>value.GetCompanyLedgerBooksAsync()).Returns(()=>lost?Task.FromException<CompanyLedgerBookReview>(new InvalidOperationException("Lost followup")):f.Read());
        client.Setup(value=>value.GetCompanyLedgerBookConfigurationAsync(It.IsAny<string>())).Returns((string id)=>f.Service.GetCompanyBookConfigurationAsync(CompanyLedgerBookTests.Company,"administrator",id,f.Reader.Authority));
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton(company);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state}});var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.CompanyBookWorkbench>(parameters);var components=(Components)provider.GetRequiredService<IComponentActivator>();
            void Field(string name,object? value)=>components.Workbench!.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(components.Workbench,value);
            void Render()=>components.Workbench!.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null);
            async Task Action(string name){await(Task)components.Workbench!.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Workbench,null)!;Render();}
            await Action("Review");Assert.Contains("No company accounting books are recorded",root.ToHtmlString());await Action("Record");Assert.Equal(0,f.Reader.Native.Writes);Assert.False(state.HasPendingBook);
            Field("_code","statutory");Field("_name","Recorded book");Field("_currency","KWD");Field("_precision",3);Field("_rounding","TO_EVEN");Field("_calendar","calendar-2026");Field("_timezone","Asia/Kuwait");Field("_reason","Reviewed company book");Render();await Action("Record");Assert.True(state.HasPendingBook);Assert.Equal(1,f.Reader.Native.Writes);var original=JsonSerializer.Serialize(state.PendingBook!.Command);Assert.Contains("exact original book request is retained",root.ToHtmlString());
            components.Workbench!.Dispose();root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Components.Workflow.CompanyBookWorkbench>(parameters);Assert.Equal(original,JsonSerializer.Serialize(state.PendingBook!.Command));lost=false;await Action("Retry");Assert.False(state.HasPendingBook);Assert.Equal(bodies[0],bodies[1]);Assert.Equal(1,f.Reader.Native.Writes);Assert.Contains("statutory company book configuration is confirmed",root.ToHtmlString());
            Assert.True(company.TrySelect(company.Clear(),"administrator",new(CompanyLedgerBookTests.Company,["ConfiguredCompanyRole"],["Accounting.View"])));Field("_recoveryId",f.Submission.Command.Id);Render();await Action("RecoverSelected");Assert.NotNull(state.BookResult);Assert.Equal(1,f.Reader.Native.Writes);await Action("ReadPublic");Assert.Single(state.Books!.Books);Assert.Contains("Asia/Kuwait",root.ToHtmlString());Assert.DoesNotContain("Review company book configuration",root.ToHtmlString());Assert.Equal(1,f.Reader.Native.Writes);
        });
    }
}
#endif
