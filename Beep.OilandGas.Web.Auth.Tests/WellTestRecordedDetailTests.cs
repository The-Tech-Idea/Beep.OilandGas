using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Components.Shared;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WellTestRecordedDetailTests
{
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in this static renderer.");public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);}
    private static async Task<string> Render<T>(Dictionary<string,object?> values) where T:IComponent
    {var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(values));return root.ToHtmlString();});}
    private static Task<string> Detail(WellTestResponse? row)=>Render<WellTestRecordedDetail>(new(){{"Source",row}});
    [Fact] public async Task ExactEvidenceAndUnitsAreShown(){var html=await Detail(new(){TestId="record/1",TestNumber="test-A",RunNumber="run-A",Uwi="well",Source="provider",CreateUser="operator",UpdateUser="reviewer",OilFlowAmount=12,OilFlowAmountUom="m3/d",OilFlowAmountOuom="bbl/d"});Assert.Contains("record/1",html);Assert.Contains("test-A",html);Assert.Contains("run-A",html);Assert.Contains("operator",html);Assert.Contains("reviewer",html);Assert.Contains("m3/d",html);Assert.Contains("bbl/d",html);Assert.Contains("data-label=\"Current UOM\"",html);Assert.Contains("Independent approval",html);}
    [Fact] public async Task MissingEvidenceIsUnavailableRatherThanAccepted(){var html=await Detail(new(){TestId="record"});Assert.Contains("Recorded status: Unavailable",html);Assert.Contains("Preparer: Unavailable",html);Assert.Contains("Source timezone unavailable",html);Assert.DoesNotContain("APPROVED",html);Assert.DoesNotContain("0.00",html);}
    [Fact] public async Task NullDetailRequestsSelection(){var html=await Detail(null);Assert.Contains("Select a recorded test",html);Assert.DoesNotContain("<table",html);}
    [Fact] public async Task RecordedNoteIsEncoded(){var html=await Detail(new(){Description="<script>alert(1)</script>"});Assert.Contains("&lt;script&gt;",html);Assert.DoesNotContain("<script>",html);}
    [Theory][InlineData(false)][InlineData(true)] public async Task SharedKpiHasKeyboardActionOnlyWhenProvided(bool action){var values=new Dictionary<string,object?>{{"Label","Oil values recorded"},{"Value","2"}};if(action)values["OnClick"]=EventCallback.Factory.Create(new object(),()=>{});var html=await Render<KpiCard>(values);Assert.Equal(action,html.Contains("View contributing records",StringComparison.Ordinal));}
}
