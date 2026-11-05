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
public sealed class WorkOrderMaterialSourcePanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ConfiguredRole")],"fixture"))));}
    private sealed class Components:IComponentActivator
    {internal WorkOrderMaterialSourcePanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderMaterialSourcePanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("original")][InlineData("actor")][InlineData("permission")][InlineData("quantity")][InlineData("late-field")][InlineData("company")]
    public async Task ActualMaterialPanelShowsOriginalQuantityUnitLocationAndClearsForeignOrLateEvidence(string fault)
    {
        var source=WorkOrderMaterialSourceTests.Source();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(source.Context.Company,["ConfiguredRole"],fault=="permission"?[]:["Inventory.View"])));var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(c=>c.ReviewWorkOrderMaterialSourceAsync(It.IsAny<CompanyWorkOrderMaterialSourceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialSourceQuery query,CancellationToken _)=>{reads++;Assert.Equal(source.Query,query);if(fault=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("foreign");fields.Raise(f=>f.CurrentFieldChanged+=null!,"foreign");}return Task.FromResult(source);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(fault=="actor"?"foreign":"issuer"));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderMaterialSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",source.Query.Task},{"SourceRevision","source"}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);Set("_item",source.Query.ItemId);Set("_unit",source.Query.QuantityUnit);Set("_quantity",fault=="quantity"?null:source.Query.RequiredQuantity);
            await EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod("ReviewAsync",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();var html=root.ToHtmlString();Assert.Equal(fault is "actor" or "permission" or "quantity"?0:1,reads);
            if(fault is "original" or "company"){Assert.Contains("Recorded pump spare",html);Assert.Contains("Required: 2 EA",html);Assert.Contains("Recorded on hand: 5 EA",html);Assert.Contains("warehouse",html);Assert.Contains("allocation",html);}else Assert.DoesNotContain("Recorded pump spare",html);
            if(fault=="company"){company.Clear();await Task.Yield();Assert.DoesNotContain("Recorded pump spare",root.ToHtmlString());}Assert.Contains("does not reserve, issue or consume stock",root.ToHtmlString());
        });
    }
}
#endif
