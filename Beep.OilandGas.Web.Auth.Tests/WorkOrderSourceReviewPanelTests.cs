using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderSourceReviewPanelTests
{
    // Static component-render fixture only; any browser call fails instead of being simulated as successful.
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier,object?[]? args)=>throw new InvalidOperationException("No browser in this focused renderer.");
        public ValueTask<T> InvokeAsync<T>(string identifier,CancellationToken token,object?[]? args)=>InvokeAsync<T>(identifier,args);
    }
    private static ProcessTransitionReview Review(bool recorded=false)=>new(new("tenant","company"),"reviewer","field","process","review","accept",new string('A',64),[],WorkOrder:new("order/a?b","field","WO-42","WO-CORRECTIVE",null,null,"<script>source</script>",[new("order/a?b","component","field","well","equipment")],recorded?[new(1,"CLOSE_OUT","Recorded closeout","FAIL","Failed pressure test",DateTime.UtcNow,"Reference"){RequirementActor="planner",Inspector="technician",Jurisdiction="Authority"}]:[],DateTimeOffset.UtcNow));
    private static async Task<string> Render(ProcessTransitionReview? review)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<WorkOrderSourceReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",review}}));return root.ToHtmlString();});
    }
    [Fact] public async Task ActualOrderAndComponentsRenderWithoutInventingAcceptance()
    {
        var html=await Render(Review());Assert.Contains("WO-42",html);Assert.Contains("well",html);Assert.Contains("equipment",html);Assert.Contains("Requested: Unavailable",html);
        Assert.Contains("No recorded inspection requirements",html);Assert.Contains("return-to-service authorization",html);Assert.Contains("/ppdm39/workorder/order%2Fa%3Fb",html);
        Assert.Contains("&lt;script&gt;",html);Assert.DoesNotContain("<script>",html);Assert.DoesNotContain("Recorded UTC:",html);
    }
    [Fact] public async Task RecordedFailurePreservesMakerInspectorAndRationale()
    {
        var html=await Render(Review(true));Assert.Contains("FAIL",html);Assert.Contains("Failed pressure test",html);Assert.Contains("planner",html);Assert.Contains("technician",html);Assert.Contains("Authority",html);Assert.Contains("Recorded UTC:",html);
    }
    [Fact] public async Task MissingReviewShowsExplicitUnavailableSource(){var html=await Render(null);Assert.Contains("Recorded maintenance details are unavailable",html);Assert.DoesNotContain("WO-42",html);}
}
