#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Tests;
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
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaterialAcceptancePanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"app"))));}
    private sealed class Nav:NavigationManager{public Nav()=>Initialize("https://fixture/","https://fixture/ppdm39/process/instances/original");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator{internal WorkOrderPermitMaterialReadinessPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderPermitMaterialReadinessPanel panel)Panel=panel;return result;}}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("fresh")][InlineData("actor")][InlineData("permission")][InlineData("late-field")][InlineData("company")][InlineData("versioned")]
    public async Task ActualReviewerFormRetainsOriginalReferencesPreservesParentReviewAndRecoversWithoutWrite(string mode)
    {
        var f=new WorkOrderMaterialAcceptanceFixture(requirementProof:mode=="versioned");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(f.Permit.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(f.Command.Review.Query.Permit,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(f.Command.Review.Query.Permit,It.IsAny<CancellationToken>())).ReturnsAsync(f.Command.Review.PermitReview);await state.ReviewAsync(client.Object);var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var nav=new Nav();var writes=0;var referenceReads=0;
        client.Setup(c=>c.ReviewWorkOrderPermitMaterialReadinessAsync(f.Command.Review.Query,It.IsAny<CancellationToken>())).ReturnsAsync(f.Command.Review);
        client.Setup(c=>c.RecordWorkOrderMaterialAcceptanceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAcceptanceCommand command,CancellationToken _)=>{writes++;Assert.Equal(f.Command.OriginalInspection.RequirementRevision,command.OriginalInspection.RequirementRevision);Assert.Contains(command.IntentId.ToString("D"),nav.Uri);Assert.Contains("materialInspectionId="+command.InspectionId,nav.Uri);Assert.Contains("permitReviewId="+command.Review.Query.Permit.IntentId.ToString("D"),nav.Uri);if(mode=="late-field"){fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(c=>c.CurrentFieldChanged+=null!,"other");}return mode=="unknown"?Task.FromException<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>(new HttpRequestException("Unknown result")):Task.FromResult(command.Prepare(f.Pending,DateTime.UtcNow));});
        client.Setup(c=>c.RecoverWorkOrderMaterialAcceptanceReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAcceptanceReference reference,CancellationToken _)=>{referenceReads++;Assert.Equal(f.Command.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderMaterialAcceptanceRecovery(f.Command,f.Command.Prepare(f.Pending,DateTime.UtcNow)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"independent"));services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=new Dictionary<string,object?>{{"Permit",f.Command.Review.Query.Permit},{"SourceRevision","source"}};if(mode=="fresh"){parameters["AcceptanceReference"]=f.Command.IntentId;parameters["AcceptanceInspection"]=f.Command.InspectionId;}
            var root=await renderer.RenderComponentAsync<WorkOrderPermitMaterialReadinessPanel>(ParameterView.FromDictionary(parameters));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;Assert.NotNull(state.Review);
            void Set(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode!="fresh"){Set("_allocation",f.Command.Review.Query.AllocationIntentId.ToString("D"));await Invoke("ReviewAsync");Set("_inspection",f.Command.InspectionId);Set("_rationale","Explicit reviewer acceptance");await Invoke("AcceptAsync");}
            var html=root.ToHtmlString();Assert.Equal(mode is "actor" or "permission" or "fresh"?0:1,writes);Assert.Equal(mode=="fresh"?1:0,referenceReads);
            if(mode is "record" or "fresh" or "company" or "versioned"){Assert.Contains("original material inspection acceptance is recorded",html);Assert.Contains("Recorded acceptance UTC",html);Assert.NotNull(state.MaterialReceipt);}else if(mode=="unknown"){Assert.Contains("outcome is unconfirmed",html);Assert.False(state.CanAcceptMaterial);await Invoke("AcceptAsync");Assert.Equal(1,writes);}else Assert.DoesNotContain("Recorded acceptance UTC",html);
            Assert.DoesNotContain("Private original logistics rationale",html);Assert.Contains("does not record permit approval",html);if(mode=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Explicit reviewer acceptance",root.ToHtmlString());}
        });
    }
}
#endif
