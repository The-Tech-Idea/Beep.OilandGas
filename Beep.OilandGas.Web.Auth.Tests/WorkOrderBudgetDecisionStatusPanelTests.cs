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
public sealed class WorkOrderBudgetDecisionStatusPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"FinanceViewer")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderBudgetDecisionStatusPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderBudgetDecisionStatusPanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("approved")][InlineData("rejected")][InlineData("changed")][InlineData("unrecorded")][InlineData("late-company")][InlineData("field")][InlineData("permission")][InlineData("costs")][InlineData("empty-costs")]
    public async Task PublicStatusFormShowsRecordedOutcomeAndActualBlockersWithoutMutationOrPrivateRationale(string mode)
    {
        var status=WorkOrderBudgetDecisionStatusTests.Status(mode);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Current.Context.ActorUserId,new(status.Current.Context.Company,["FinanceViewer"],mode=="permission"?[]:["Accounting.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":status.Query.Budget.FieldId);
        var client=new Mock<IWorkOrderServiceClient>();var reads=0;client.Setup(value=>value.ReadBudgetDecisionStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="late-company")company.Clear();return Task.FromResult(status);});
        var costs=new CompanyWorkOrderActualCostReview(status,mode=="costs"?[new("cost-source",status.Query.AfeId,"WORK_ORDER_LABOR_USAGE","labor-source",new string('A',64),12.345m,status.Original().CurrencyCode,DateTime.UtcNow,new string('B',64))]:[],DateTime.UtcNow);
        client.Setup(value=>value.ReadActualCostsAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(costs);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderBudgetDecisionStatusState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);
        services.AddSingleton<AuthenticationStateProvider>(new Auth(status.Current.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderBudgetDecisionStatusPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",status.Query.Budget.WorkOrderId},{"FieldId",status.Query.Budget.FieldId},{"OriginalBookId",status.Query.Budget.BookId},{"OriginalAfeId",status.Query.AfeId}}));
            var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            Assert.Equal(status.Query.Budget.BookId,panel.GetType().GetField("_book",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));Assert.Equal(status.Query.AfeId,panel.GetType().GetField("_afe",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));
            await (Task)panel.GetType().GetMethod(mode is "costs" or "empty-costs"?"LoadCostsAsync":"LoadAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();
            var html=root.ToHtmlString();Assert.DoesNotContain("Private reviewer rationale",html);
            if(mode is "field" or "permission"){Assert.Equal(0,reads);return;}Assert.Equal(1,reads);
            if(mode=="late-company"){Assert.DoesNotContain(status.Query.AfeId,html);Assert.DoesNotContain("Recorded approval matches",html);return;}
            if(mode is "costs" or "empty-costs"){Assert.Contains("Recorded actual costs",html);Assert.Contains("Original budget remaining",html);if(mode=="costs")Assert.Contains("cost-source",html);else Assert.Contains("contains no recorded costs",html);return;}
            if(mode=="approved")Assert.Contains("Recorded approval matches",html);else Assert.DoesNotContain("Recorded approval matches",html);
            if(mode=="unrecorded")Assert.Contains("contains no independent budget decision",html);
            if(mode=="changed"){Assert.Contains("Recorded independent Approve",html);Assert.Contains("Original budget work scope changed.",html);}
            if(mode=="rejected")Assert.Contains("independently rejected",html);
        });
        client.Verify(value=>value.RecordBudgetDecisionAsync(It.IsAny<CompanyWorkOrderBudgetDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
