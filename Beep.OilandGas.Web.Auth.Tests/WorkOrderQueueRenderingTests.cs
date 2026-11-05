using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderQueueRenderingTests
{
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("No browser in static queue rendering.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    private static WorkOrderSummary Row()=>new(){InstanceId="order/a?b",InstanceName="WO-42",FieldId="field",Source="WORK_ORDER",State="",EvidenceUnavailableReason="No recorded workflow state",WoSubType="WO-CORRECTIVE",Components=[new("order/a?b","component","field","well-UWI","equipment-42")]};
    private static async Task<string> Render(bool confirmed=true,List<WorkOrderSummary>? rows=null,bool clear=false)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"planner",new(new("tenant","company"),[],[])));using var review=new WorkOrderListState(company);
        if(confirmed)await review.LoadAsync(null,()=>Task.FromResult<string?>("field"),_=>Task.FromResult(rows ?? new List<WorkOrderSummary>{Row()}));
        if(clear)company.Clear();
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<WorkOrderQueueRenderHost>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",review}}));return root.ToHtmlString();});
    }
    [Theory][InlineData("en-US")][InlineData("fr-FR")]
    public async Task RecordedDatesUseDisplayCultureWithoutTimezoneConversion(string culture)
    {
        var previous=System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo(culture);
            var row=Row();row.RequestDate=new DateTime(2026,3,4,23,30,0,DateTimeKind.Unspecified);row.DueDate=new DateTime(2026,3,5,0,30,0,DateTimeKind.Unspecified);
            var html=await Render(rows:[row]);Assert.Contains(row.RequestDate.Value.ToString("d"),html);Assert.Contains(row.DueDate.Value.ToString("d"),html);
            Assert.Contains("No overdue or execution-readiness status is inferred",html);
        }
        finally{System.Globalization.CultureInfo.CurrentCulture=previous;}
    }
    [Fact] public async Task QueueShowsActualOrderRelationshipsAndMissingDatesWithoutStateBoard()
    {
        var html=await Render();Assert.Contains("WO-42",html);Assert.Contains("well-UWI",html);Assert.Contains("equipment-42",html);Assert.Contains("component",html);Assert.Contains("Unavailable",html);
        Assert.Contains("/ppdm39/workorder/detail?workOrderId=order%2Fa%3Fb",html);Assert.Contains("Search loaded order",html);Assert.Contains("data-label=\"Due date\"",html);Assert.Contains("Loaded headers: 1",html);
        Assert.DoesNotContain("Completed",html);Assert.DoesNotContain("Under Review",html);Assert.Contains("Follow assigned process work",html);
    }
    [Fact] public async Task EmptyConfirmedQueueDoesNotBecomeUnconfirmedOrCompleted()
    {
        var html=await Render(rows:[]);Assert.Contains("No recorded work orders were returned",html);Assert.DoesNotContain("not confirmed",html);Assert.DoesNotContain("<table",html);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task UnconfirmedOrChangedCompanyCannotRenderOldQueue(bool clear)
    {
        var html=await Render(confirmed:clear,clear:clear);Assert.Contains("queue is not confirmed",html);Assert.DoesNotContain("WO-42",html);Assert.DoesNotContain("<table",html);
    }
    [Fact] public async Task QueueEncodesRecordedNumbersAndReferences()
    {
        var row=Row();row.InstanceName="<script>order</script>";row.Components[0]=row.Components[0] with{EquipmentId="<script>equipment</script>"};var html=await Render(rows:[row]);
        Assert.Contains("&lt;script&gt;order",html);Assert.Contains("&lt;script&gt;equipment",html);Assert.DoesNotContain("<script>",html);
    }
}
