using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class RecordedProcessRenderingTests
{
    [Fact] public async Task SharedProcessPanelPreservesFieldScopeWithoutCompanyAccountingLink()
    {
        using var company=CompanyAccountingProcessStateTests.Company();var source=CompanyAccountingProcessStateTests.Source();source.FieldId="field";source.EntityType="WELL";
        using var state=new Beep.OilandGas.Web.Services.ProcessInstanceDetailState(company,_=>Task.FromResult<ProcessInstance?>(source),_=>Task.FromResult(source.History));
        await state.LoadAsync("process",()=>Task.FromResult<string?>("field"));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Fixtures.RecordedProcessRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state}}));return System.Net.WebUtility.HtmlDecode(root.ToHtmlString());});
        Assert.Contains("FIELD: field",html);Assert.Contains("Recorded source history",html);Assert.DoesNotContain("Scope: Company accounting",html);Assert.DoesNotContain("Open owned accounting source",html);
    }
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("AP_INVOICE","/accounting/payables/invoices/")][InlineData("PURCHASE_ORDER","/procurement/orders/")]
    public async Task ActualCompanyProcessPanelRendersRecordedAssignmentHistoryAndExactSource(string type,string prefix)
    {
        using var company=CompanyAccountingProcessStateTests.Company();using var state=CompanyAccountingProcessStateTests.State(company);
        await state.LoadAccountingAsync("process",_=>Task.FromResult<ProcessInstance?>(CompanyAccountingProcessStateTests.Source(type)));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        async Task<string> Render()=>await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Fixtures.RecordedProcessRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"State",state}}));return System.Net.WebUtility.HtmlDecode(root.ToHtmlString());});
        var html=await Render();Assert.Contains("Scope: Company accounting",html);Assert.Contains("Recorded review",html);Assert.Contains("reviewer",html);Assert.Contains("Accountant",html);
        Assert.Contains("Recorded source history",html);Assert.Contains(prefix+"source%2F%CE%B1%252F",html);Assert.DoesNotContain("FIELD:",html);Assert.Contains("independent accounting decisions",html);
        company.Clear();var cleared=await Render();Assert.DoesNotContain("Recorded source history",cleared);Assert.DoesNotContain("Open owned accounting source",cleared);
    }
}
