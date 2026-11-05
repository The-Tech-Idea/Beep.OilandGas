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
public sealed class MaintenanceHandoffPanelTests
{
    private sealed class Auth:AuthenticationStateProvider
    {
        private ClaimsPrincipal user=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"fixture"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(user));
        internal void Revoke(){user=new(new ClaimsIdentity());NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Nav:NavigationManager
    {
        internal Nav(){Initialize("http://fixture/","http://fixture/ppdm39/workorder/detail?workOrderId=order%2F%D8%A7%D9%84%D8%B9%D8%A7%D9%84%D9%85%252F");}
        protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();
        protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier,object?[]? args)=>ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string identifier,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);
    }
    private sealed class Components:IComponentActivator
    {
        internal CompanyMaintenanceHandoff? Panel;
        public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is CompanyMaintenanceHandoff panel)Panel=panel;return component;}
    }
    private static Task Invoke(CompanyMaintenanceHandoff panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
    [Theory][InlineData("allowed")][InlineData("empty")][InlineData("unknown")][InlineData("fresh")][InlineData("missing")][InlineData("company")][InlineData("signout")]
    public async Task ActualMudPanelUsesRecordedChoicesOriginalReferenceAndReadOnlyRecovery(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],[])));
        var source=new ProductionInterventionMaintenanceLink(new("tenant","company"),"actor","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"original-engineer","Recorded approval",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        var review=new CompanyMaintenanceProcessStartReview(new(source.Company,new("lifecycle","fingerprint"),"actor","revision","field"),"recorded-workflow",new string('C',64),source,"Configured maintenance review","ConfiguredReceivingRole");
        CompanyMaintenanceProcessStart? intent=fault is "fresh" or "missing"?review.CreateIntent(Guid.NewGuid()):null;
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var nav=new Nav();var auth=new Auth();var writes=0;
        client.Setup(value=>value.GetMaintenanceHandoffChoicesAsync(It.IsAny<CompanyMaintenanceSourceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyMaintenanceStartCatalog(review.Context,source,fault=="empty"?[]:[review]));
        client.Setup(value=>value.ReviewMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStartQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        CompanyMaintenanceProcessStartReceipt Receipt(CompanyMaintenanceProcessStart command)=>new(command.Context.Company,command.Context.ActorUserId,command.Context.FieldId!,command.IntentId,command.InstanceId(),command.Source.WorkOrderId,command.Source.DecisionId,"initial-step",["recorded-task"],new string('D',64));
        client.Setup(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceProcessStart command,CancellationToken _) =>
        {
            Assert.Contains("maintenanceIntent="+command.IntentId.ToString("D"),nav.Uri);intent=command;writes++;
            return fault=="unknown"?Task.FromException<CompanyMaintenanceProcessStartReceipt>(new HttpRequestException("Unconfirmed response")):Task.FromResult(Receipt(command));
        });
        client.Setup(value=>value.RecoverMaintenanceHandoffReferenceAsync(It.IsAny<CompanyMaintenanceRecoveryQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceRecoveryQuery query,CancellationToken _) =>
        {
            Assert.NotNull(intent);Assert.Equal(intent.IntentId,query.IntentId);Assert.Equal(source.WorkOrderId,query.WorkOrderId);Assert.Equal(source.DecisionId,query.DecisionId);
            return fault=="missing"?Task.FromException<CompanyMaintenanceRecovery>(new InvalidOperationException("Original assignment unavailable")):Task.FromResult(new CompanyMaintenanceRecovery(intent,Receipt(intent)));
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton(client.Object);services.AddScoped<CompanyMaintenanceHandoffState>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyMaintenanceHandoff>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Source",source},{"OriginalReference",intent?.IntentId}}));
            var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel!;
            if(fault is "fresh" or "missing")
            {
                Assert.Equal(0,writes);Assert.Contains(intent!.IntentId.ToString("D"),root.ToHtmlString());
                Assert.Equal(fault=="fresh",root.ToHtmlString().Contains("Original receiving assignment confirmed.",StringComparison.Ordinal));
                return;
            }
            await Invoke(panel,"LoadChoicesAsync");
            if(fault=="empty"){Assert.Contains("No configured WORK_ORDER",root.ToHtmlString());Assert.Equal(0,writes);return;}
            Assert.Contains("Recorded receiving workflow",root.ToHtmlString());
            panel.GetType().GetMethod("SelectProcess",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,["recorded-workflow"]);
            await Invoke(panel,"ReviewAsync");Assert.Contains("ConfiguredReceivingRole",root.ToHtmlString());Assert.Contains("Create receiving assignment",root.ToHtmlString());
            await Invoke(panel,"SubmitAsync");Assert.Equal(1,writes);
            if(fault=="unknown")
            {
                Assert.Contains("Assignment outcome is unconfirmed",root.ToHtmlString());await Invoke(panel,"SubmitAsync");Assert.Equal(1,writes);
                await Invoke(panel,"RecoverAsync");Assert.Contains("Original receiving assignment confirmed.",root.ToHtmlString());Assert.Equal(1,writes);
            }
            else Assert.Contains("Original receiving assignment confirmed.",root.ToHtmlString());
            if(fault=="company")company.Clear();if(fault=="signout")auth.Revoke();
            if(fault is "company" or "signout"){Assert.DoesNotContain("Original receiving assignment confirmed.",root.ToHtmlString());Assert.DoesNotContain(intent!.IntentId.ToString("D"),root.ToHtmlString());}
            Assert.Contains("Independent acceptance",root.ToHtmlString());
        });
    }
}
#endif
