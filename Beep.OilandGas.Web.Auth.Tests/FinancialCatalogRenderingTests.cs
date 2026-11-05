using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class FinancialCatalogRenderingTests
{
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in catalog rendering.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static ProcessDefinition Row()=>new(){ProcessId="financial/a?b",ProcessName="Configured local accounting",ProcessType="FINANCIAL",EntityType="JOURNAL_ENTRY",Version="",Configuration=[]};
    private static async Task<string> Render(bool confirmed=true,List<ProcessDefinition>? rows=null,bool clear=false)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(new("tenant","company"),[],[])));using var state=new FinancialProcessCatalogState(company);
        if(confirmed)await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(rows ?? new List<ProcessDefinition>{Row()}));if(clear)company.Clear();
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<FinancialCatalogRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return root.ToHtmlString();});
    }
    [Fact] public async Task CatalogUsesActualDefinitionAndMissingReferenceWithoutDefaultRegime()
    {
        var html=await Render();Assert.Contains("Configured local accounting",html);Assert.Contains("JOURNAL_ENTRY",html);Assert.Contains("Not recorded",html);Assert.Contains("Unavailable",html);
        Assert.Contains("/ppdm39/process/definitions/financial%2Fa%3Fb",html);Assert.Contains("not assigned transactions",html);Assert.Contains("Definitions: 1",html);Assert.DoesNotContain("FASB",html);Assert.DoesNotContain("SOX",html);
    }
    [Fact] public async Task EmptyCatalogDoesNotInventSeededStandards(){var html=await Render(rows:[]);Assert.Contains("No active financial definitions",html);Assert.DoesNotContain("<table",html);Assert.DoesNotContain("CRW_",html);}
    [Theory][InlineData(false)][InlineData(true)] public async Task UnconfirmedAndChangedCompanyCannotExposeOldDefinitions(bool clear)
    {var html=await Render(confirmed:clear,clear:clear);Assert.Contains("No definitions are confirmed",html);Assert.DoesNotContain("Configured local accounting",html);Assert.DoesNotContain("<table",html);}
    [Fact] public async Task ActualRegulationAndDefinitionTextAreEncoded()
    {
        var row=Row();row.ProcessName="<script>process</script>";row.Configuration["regulation"]="<script>reference</script>";var html=await Render(rows:[row]);
        Assert.Contains("&lt;script&gt;process",html);Assert.Contains("&lt;script&gt;reference",html);Assert.DoesNotContain("<script>",html);
    }
}
