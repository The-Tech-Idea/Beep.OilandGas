using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
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
public sealed class WorkOrderPermitMaterialReadinessPanelTests
{
    private sealed class MaterialNavigation:NavigationManager{public MaterialNavigation()=>Initialize("http://fixture/","http://fixture/ppdm39/process/instances/original");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderPermitMaterialReadinessPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderPermitMaterialReadinessPanel panel)Panel=panel;return result;}}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("original")][InlineData("released")][InlineData("actor")][InlineData("permission")][InlineData("reference")][InlineData("late-field")][InlineData("company")]
    public async Task ActualReviewerPanelShowsExactCurrentMaterialBlockersAndClearsForeignEvidence(string mode)
    {
        var review=WorkOrderMaterialReadinessTests.Review(mode=="released"?"released":"original");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.PermitReview.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(c=>c.ReviewWorkOrderPermitMaterialReadinessAsync(It.IsAny<CompanyWorkOrderMaterialReadinessQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialReadinessQuery query,CancellationToken _)=>{reads++;Assert.Equal(review.Query,query);if(mode=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}return Task.FromResult(review);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<NavigationManager>(new MaterialNavigation());services.AddSingleton(new WorkOrderExecutionPermitDecisionState(company));services.AddSingleton(company);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"foreign":"independent"));services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderPermitMaterialReadinessPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",review.Query.Permit},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;panel.GetType().GetField("_allocation",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,mode=="reference"?"":review.Query.AllocationIntentId.ToString("D"));
            await EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod("ReviewAsync",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();var html=root.ToHtmlString();Assert.Equal(mode is "actor" or "permission" or "reference"?0:1,reads);
            if(mode is "original" or "released" or "company"){Assert.Contains("Recorded pump spare",html);Assert.Contains("Independent reviewer independent",html);Assert.Contains("Warehouse warehouse",html);Assert.Contains(mode=="released"?"has been released":"no listed blockers",html);}else Assert.DoesNotContain("Recorded pump spare",html);
            Assert.DoesNotContain("Private original reservation rationale",html);Assert.Contains("does not record permit approval",html);if(mode=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Recorded pump spare",root.ToHtmlString());}
        });
    }
}
#endif