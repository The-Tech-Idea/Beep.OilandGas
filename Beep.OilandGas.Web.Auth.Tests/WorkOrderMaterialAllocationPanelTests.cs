using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
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
public sealed class WorkOrderMaterialAllocationPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderMaterialAllocationPanel Panel=null!;public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is WorkOrderMaterialAllocationPanel panel)Panel=panel;return value;}}
    private sealed class Nav:NavigationManager{public Nav()=>Initialize("http://fixture/","http://fixture/ppdm39/process/instances/original");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("recreation")][InlineData("actor")][InlineData("permission")][InlineData("utc")][InlineData("late-field")][InlineData("release")][InlineData("release-unknown")][InlineData("release-recreation")]
    public async Task ActualReservationPanelRetainsReferenceAndShowsOnlyCurrentOriginalEvidence(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var utc=new DateTime(2026,10,13,10,0,0,DateTimeKind.Utc);var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var original=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(2),"Original requirement");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:["Inventory.View","Inventory.ReserveWorkOrderMaterial","Inventory.ReleaseWorkOrderMaterial"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var writes=0;var reads=0;var nav=new Nav();
        client.Setup(c=>c.ReviewWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialSourceQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(c=>c.RecordWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationCommand command,CancellationToken _)=>
        {writes++;Assert.Contains(command.IntentId.ToString("D"),nav.Uri);if(mode=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}return mode=="unknown"?Task.FromException<WORK_ORDER_MATERIAL_ALLOCATION>(new HttpRequestException("Unknown commit")):Task.FromResult(command.Prepare(utc));});
        client.Setup(c=>c.RecoverWorkOrderMaterialAllocationReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAllocationReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationReferenceQuery query,CancellationToken _)=>{reads++;Assert.Equal(original.IntentId,query.IntentId);return Task.FromResult(new CompanyWorkOrderMaterialAllocationRecovery(original,original.Prepare(utc)));});
        var releaseWrites=0;client.Setup(c=>c.ReleaseWorkOrderMaterialAsync(It.IsAny<CompanyWorkOrderMaterialReleaseCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialReleaseCommand command,CancellationToken _)=>{releaseWrites++;Assert.Contains(command.IntentId.ToString("D"),nav.Uri);return mode=="release-unknown"?Task.FromException<WORK_ORDER_MATERIAL_RELEASE>(new HttpRequestException("Unknown release acknowledgement")):Task.FromResult(command.Prepare(utc.AddSeconds(1)));});
        var releaseOriginal=new CompanyWorkOrderMaterialReleaseCommand(new(original,original.Prepare(utc)),Guid.NewGuid(),"Cancelled original requirement");var releaseReads=0;client.Setup(c=>c.RecoverWorkOrderMaterialReleaseReferenceAsync(It.IsAny<CompanyWorkOrderMaterialReleaseReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialReleaseReferenceQuery query,CancellationToken _)=>{releaseReads++;Assert.Equal(releaseOriginal.IntentId,query.IntentId);Assert.Equal(original.IntentId,query.Allocation.IntentId);return Task.FromResult(new CompanyWorkOrderMaterialReleaseRecovery(releaseOriginal,releaseOriginal.Prepare(utc.AddSeconds(1))));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"foreign":"issuer"));services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new WorkOrderMaterialAllocationState(company));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=new Dictionary<string,object?>{{"Task",source.Query.Task},{"SourceRevision","source"}};if(mode is "recreation" or "release-recreation")parameters["OriginalReference"]=original.IntentId;if(mode=="release-recreation")parameters["ReleaseReference"]=releaseOriginal.IntentId;
            var root=await renderer.RenderComponentAsync<WorkOrderMaterialAllocationPanel>(ParameterView.FromDictionary(parameters));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode is not ("recreation" or "release-recreation"))
            {
                Set("_item",source.Query.ItemId);Set("_quantity",2m);Set("_unit","EA");await Invoke("ReviewAsync");Set("_from",mode=="utc"?"2026-10-13 10:00":"2026-10-13T10:00:00Z");Set("_to","2026-10-13T12:00:00Z");Set("_reason","Original requirement");await Invoke("ReserveAsync");
            }
            if(mode is "release" or "release-unknown"){Set("_releaseReason","Cancelled original requirement");await Invoke("ReleaseAsync");Assert.Equal(1,releaseWrites);}
            var html=root.ToHtmlString();Assert.Equal(mode is "actor" or "permission" or "utc" or "recreation" or "release-recreation"?0:1,writes);Assert.Equal(mode is "recreation" or "release-recreation"?1:0,reads);
            if(mode is "record" or "recreation" or "release" or "release-unknown" or "release-recreation"){Assert.Contains("Recorded reservation",html);Assert.Contains("Retained original requirement",html);Assert.Contains("Original requirement",html);}else if(mode=="unknown")Assert.Contains("outcome is unconfirmed",html);else Assert.DoesNotContain("Recorded reservation wo-material",html);
            if(mode is "release" or "release-recreation")Assert.Contains("Recorded full release",html);if(mode=="release-unknown")Assert.Contains("original release outcome is unconfirmed",html);
            Assert.Equal(mode=="release-recreation"?1:0,releaseReads);Assert.Contains("does not issue or consume physical stock",html);
            if(mode=="release"){await Invoke("BeginNewAsync");Assert.DoesNotContain("materialAllocationId",nav.Uri);Assert.DoesNotContain("materialReleaseId",nav.Uri);Assert.DoesNotContain("Cancelled original requirement",root.ToHtmlString());Assert.Equal(1,writes);Assert.Equal(1,releaseWrites);}
            if(mode=="record"){company.Clear();await Task.Yield();Assert.DoesNotContain("Original requirement",root.ToHtmlString());}
        });
    }
    [Theory][InlineData("original")][InlineData("empty")][InlineData("actor")][InlineData("role")][InlineData("late-field")][InlineData("company")][InlineData("foreign-item")][InlineData("damaged")]
    public async Task ActualAssignedItemChooserKeepsExplicitInputsAndInvalidatesOldSourceReview(string mode)
    {
        var source=WorkOrderMaterialSourceTests.Source();var catalog=CompanyWorkOrderMaterialCatalog.Create(source.WorkOrder.Stage,[source.Inventory.Item],DateTime.UtcNow);if(mode=="empty")catalog=CompanyWorkOrderMaterialCatalog.Create(catalog.Stage,[],catalog.ComparedAtUtc);if(mode=="damaged")catalog=catalog with{Revision=new string('F',64)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var calls=0;
        client.Setup(c=>c.ReviewWorkOrderMaterialCatalogAsync(source.Query.Task,It.IsAny<CancellationToken>())).Returns(()=>{calls++;if(mode=="company")company.Clear();if(mode=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(f=>f.CurrentFieldChanged+=null!,"other");}return Task.FromResult(mode=="role"?catalog with{Stage=catalog.Stage with{AssignedRole="OtherRole"}}:catalog);});
        client.Setup(c=>c.ReviewWorkOrderMaterialAllocationAsync(source.Query,It.IsAny<CancellationToken>())).ReturnsAsync(CompanyWorkOrderMaterialAllocationReview.Create(source,[],DateTime.UtcNow));var nav=new Nav();var state=new WorkOrderMaterialAllocationState(company);var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"issuer"));services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(state);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderMaterialAllocationPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",source.Query.Task},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            await Invoke("LoadItemsAsync");Assert.Equal(mode=="actor"?0:1,calls);var html=root.ToHtmlString();
            if(mode=="empty"){Assert.Contains("No active recorded company inventory choices",html);return;}
            if(mode is "actor" or "role" or "late-field" or "company" or "damaged"){Assert.DoesNotContain("Recorded choices: 1",html);Assert.Null(state.Review);return;}
            Assert.Contains("Recorded choices: 1",html);void Select(string id)=>panel.GetType().GetMethod("SelectItem",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,[id]);
            if(mode=="foreign-item"){var error=Assert.Throws<TargetInvocationException>(()=>Select("outside"));Assert.IsType<InvalidOperationException>(error.InnerException);Assert.Null(state.Review);return;}
            await EventCallback.Factory.Create(panel,()=>Select(source.Query.ItemId)).InvokeAsync();html=root.ToHtmlString();Assert.Contains("Open selected recorded inventory evidence",html);Assert.Contains(Uri.EscapeDataString(source.Query.ItemId),html);
            Assert.Null(panel.GetType().GetField("_quantity",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.Equal("",panel.GetType().GetField("_unit",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));
            panel.GetType().GetField("_quantity",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,2m);panel.GetType().GetField("_unit",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"EA");await Invoke("ReviewAsync");Assert.True(state.CanReserve);
            panel.GetType().GetMethod("SetQuantity",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,[3m]);Assert.Null(state.Review);Assert.False(state.CanReserve);
            panel.GetType().GetField("_quantity",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,2m);await Invoke("ReviewAsync");Assert.True(state.CanReserve);await Invoke("LoadItemsAsync");Assert.Null(state.Review);Assert.False(state.CanReserve);Assert.Null(panel.GetType().GetField("_quantity",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));
            client.Verify(c=>c.RecordWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>()),Times.Never);company.Clear();await Task.Yield();Assert.DoesNotContain("Recorded choices: 1",root.ToHtmlString());
        });
    }
}
#endif