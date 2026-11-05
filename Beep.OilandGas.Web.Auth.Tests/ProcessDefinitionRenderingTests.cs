using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ProcessDefinitionRenderingTests
{
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in definition rendering.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static async Task<string> Render(bool loaded=true,bool empty=false)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(new("tenant","company"),[],[])));using var state=new ProcessDefinitionDetailState(company);
        var definition=new ProcessDefinition{ProcessId="definition",ProcessName="Configured finance",EntityType="JOURNAL_ENTRY",ProcessType="FINANCIAL",CreatedDate=default,Version="",Steps=empty?[]:[new(){StepId="approve",StepName="Recorded approval",SequenceNumber=1,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["Accountant","<script>role</script>"]}]};
        if(loaded)await state.LoadAsync("definition",()=>Task.FromResult<string?>("field"),_=>Task.FromResult<ProcessDefinition?>(definition));
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<ProcessDefinitionReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return root.ToHtmlString();});
    }
    [Fact] public async Task RecordedResponsibilitiesRenderWithoutGuessedTablesOrCreationDate()
    {
        var html=await Render();Assert.Contains("Recorded approval",html);Assert.Contains("Accountant",html);Assert.Contains("Approval required",html);Assert.Contains("Recorded creation date: Unavailable",html);Assert.Contains("Version: Unavailable",html);
        Assert.Contains("&lt;script&gt;role",html);Assert.DoesNotContain("<script>",html);Assert.DoesNotContain("PROJECT_STEP_BA",html);Assert.Contains("do not grant roles",html);
    }
    [Fact] public async Task EmptyStepListExplainsUnavailableStart(){var html=await Render(empty:true);Assert.Contains("No recorded steps",html);Assert.Contains("Starting is unavailable",html);}
    [Fact] public async Task MissingReviewDoesNotDisplayOldDefinition(){var html=await Render(loaded:false);Assert.Contains("No configured definition is confirmed",html);Assert.DoesNotContain("Configured finance",html);}
    private static async Task<string> RenderSource(ProcessStartSourceReview? source)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<ProcessStartSourcePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",source}}));return root.ToHtmlString();});
    }
    private static ProcessStartSourceReview StartSource(string type)=>new(new("tenant","company"),"actor","field","definition",new string('A',64),"source",type,"JE-1","<script>recorded</script>",null,null,new string('B',64),DateTimeOffset.UtcNow);
    [Fact] public async Task JournalStartShowsRecordedHeaderAndExplicitFieldAndPostingLimits()
    {
        var html=await RenderSource(StartSource("JOURNAL_ENTRY"));Assert.Contains("JE-1",html);Assert.Contains("Status: Unavailable",html);Assert.Contains("direct journal-to-FIELD relationship is unconfirmed",html);Assert.Contains("does not post, approve or close",html);Assert.Contains("&lt;script&gt;recorded",html);Assert.DoesNotContain("<script>",html);
    }
    [Fact] public async Task MaintenanceStartShowsFieldRelationshipWithoutClaimingAcceptance()
    {
        var html=await RenderSource(StartSource("WORK_ORDER"));Assert.Contains("recorded work-order components establish the FIELD relationship",html);Assert.Contains("does not accept inspections or complete maintenance",html);
    }
    [Fact] public async Task UnreviewedStartDoesNotRetainOldSourceMetadata()
    {
        var html=await RenderSource(null);Assert.Contains("review its recorded data before starting",html);Assert.DoesNotContain("JE-1",html);
    }
}
