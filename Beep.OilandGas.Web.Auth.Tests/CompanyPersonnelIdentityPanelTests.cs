using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class CompanyPersonnelIdentityPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"Administrator")],"app"))));}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string name,object?[]? arguments)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? arguments)=>ValueTask.FromResult(default(T)!);}
    private sealed class Nav:NavigationManager{public Nav(){Initialize("https://identity.test/","https://identity.test/admin/dashboard");}protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();protected override void NavigateToCore(string uri,NavigationOptions options)=>NavigateToCore(uri,options.ForceLoad);}
    private sealed class Components:IComponentActivator{public CompanyPersonnelIdentityPanel Panel=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is CompanyPersonnelIdentityPanel panel)Panel=panel;return component;}}
    [Theory][InlineData("record")][InlineData("lost")][InlineData("actor")][InlineData("permission")][InlineData("unverified")][InlineData("company")]
    public async Task ActualAdministratorFormVerifiesBlankIdentityChoicesRetainsBeforeTransportAndRecoversWithoutAnotherWrite(string mode)
    {
        var review=CompanyPersonnelIdentityClientTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(review.Context.Company,["Administrator"],mode=="permission"?[]:["HSE.ConfigurePersonnelIdentity"])));using var state=new CompanyPersonnelIdentityState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewCompanyPersonnelIdentityAsync(review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);var nav=new Nav();var writes=0;CompanyPersonnelIdentityCommand? original=null;COMPANY_PERSONNEL_IDENTITY? saved=null;
        client.Setup(value=>value.RecordCompanyPersonnelIdentityAsync(It.IsAny<CompanyPersonnelIdentityCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelIdentityCommand command,CancellationToken _)=>{Assert.Contains(command.IntentId.ToString("D"),nav.Uri);Assert.Contains(command.RecordId(),nav.Uri);writes++;original=command;saved=command.Prepare(DateTime.UtcNow);if(mode=="lost")throw new HttpRequestException("Lost response");return Task.FromResult(saved);});client.Setup(value=>value.RecoverCompanyPersonnelIdentityReferenceAsync(It.IsAny<CompanyPersonnelIdentityReference>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelIdentityReference reference,CancellationToken _)=>{Assert.Equal(original!.IntentId,reference.IntentId);return Task.FromResult(new CompanyPersonnelIdentityRecovery(original,saved!));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"administrator"));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyPersonnelIdentityPanel>();var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!)).InvokeAsync();void Set(string name,object value)=>panel.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
            Assert.Equal("",panel.GetType().GetField("_user",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));panel.GetType().GetMethod("UserChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,[review.Query.UserId]);panel.GetType().GetMethod("PersonChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,[review.Query.PersonBaId]);Assert.Equal(review.Query.UserId,panel.GetType().GetField("_user",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));await Invoke("ReviewAsync");if(mode is "actor" or "permission"){Assert.Null(state.Review);Assert.Equal(0,writes);return;}
            Assert.Contains("Actual account",root.ToHtmlString());Assert.Contains("Reported person name",root.ToHtmlString());Assert.Equal(false,panel.GetType().GetField("_verified",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));Assert.Equal("",panel.GetType().GetField("_from",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));Set("_from",review.ComparedAtUtc.ToString("O"));Set("_to",review.ComparedAtUtc.AddDays(10).ToString("O"));Set("_source","Actual identity verification evidence");Set("_reason","Private original administrator rationale");Set("_verified",mode!="unverified");await Invoke("RecordAsync");if(mode=="unverified"){Assert.Equal(0,writes);Assert.Null(state.VisibleReference);return;}Assert.Equal(1,writes);
            if(mode=="lost"){Assert.Contains("Retained original link",root.ToHtmlString());await Invoke("RecordAsync");Assert.Equal(1,writes);await Invoke("RecoverAsync");Assert.Equal(1,writes);}Assert.Contains("Verified identity link recorded:",root.ToHtmlString());original!.RequireRecorded(state.Receipt!);if(mode=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Verified identity link recorded:",root.ToHtmlString());}else{await Invoke("BeginNextAsync");Assert.True(state.CanReview);Assert.DoesNotContain("personnelIdentityIntent",nav.Uri);}
        });
    }
}
#endif
