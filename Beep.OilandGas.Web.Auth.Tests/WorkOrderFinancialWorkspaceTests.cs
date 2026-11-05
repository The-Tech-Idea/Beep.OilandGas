using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Components.Workflow;
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
public sealed class WorkOrderFinancialWorkspaceTests
{
    internal static CompanyWorkOrderFinancialSources Source(string mode="approved")
    {
        var status=WorkOrderBudgetDecisionStatusTests.Status(mode);var context=status.Current.Context;
        var books=new CompanyLedgerBookReview(context.Company,context.ActorUserId,[status.Current.Book],"");books=books with{Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(books))};
        var costs=new CompanyWorkOrderActualCostReview(status,[new("actual-cost",status.Query.AfeId,"WORK_ORDER_MATERIAL_USAGE","actual-consumption",new string('A',64),12.345m,"KWD",DateTime.UtcNow,new string('B',64))],DateTime.UtcNow);
        var result=new CompanyWorkOrderFinancialSources(context,status.Current.BudgetDatabase,new(status.Query.Budget.WorkOrderId,status.Query.Budget.FieldId),status.Current.Order,books,mode=="empty"?[]:[costs],DateTime.UtcNow);result.Require();return result;
    }
    private static WorkOrderSummary Header(CompanyWorkOrderFinancialSources source)=>JsonSerializer.Deserialize<WorkOrderSummary>(JsonSerializer.Serialize(source.Order))!;
    private static CompanyClientContext Company(CompanyWorkOrderFinancialSources source,string[]? permissions=null)
    {
        var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),source.Context.ActorUserId,new(source.Context.Company,["FinanceRole"],permissions??["Accounting.View"])));return company;
    }
    [Theory][InlineData("current")][InlineData("late-company")][InlineData("late-field")][InlineData("wrong-account")]
    public async Task FinanceDiscoveryReusesConfirmedOrdersAndFencesLateSources(string mode)
    {
        var source=Source();using var company=Company(source);using var state=new WorkOrderFinancialWorkspaceState(company);var client=new Mock<IWorkOrderServiceClient>();
        client.Setup(value=>value.GetWorkOrdersAsync(null,null)).ReturnsAsync([Header(source)]);
        client.Setup(value=>value.ReadFinancialSourcesAsync(source.Query,It.IsAny<CancellationToken>())).Returns(()=>
        {
            if(mode=="late-company")company.Clear();if(mode=="late-field")state.Clear();return Task.FromResult(mode=="wrong-account"?source with{Context=source.Context with{ActorUserId="foreign"}}:source);
        });
        await state.LoadOrdersAsync(client.Object,()=>Task.FromResult<string?>(source.Query.FieldId));Assert.Single(state.Orders);
        var error=await Record.ExceptionAsync(()=>state.ReadAsync(client.Object,source.Query.WorkOrderId));
        if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Current);Assert.Equal(12.345m,state.Current.Budgets[0].RecordedTotal());company.Clear();Assert.Null(state.Current);Assert.Empty(state.Orders);}
        else{Assert.Null(state.Current);if(mode=="wrong-account")Assert.NotNull(error);else Assert.Null(error);}
    }
    [Fact] public async Task UnconfirmedOrderCannotStartFinancialReadAndPermissionLossClearsSources()
    {
        var source=Source();using var company=Company(source);using var state=new WorkOrderFinancialWorkspaceState(company);var client=new Mock<IWorkOrderServiceClient>();
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReadAsync(client.Object,source.Query.WorkOrderId));
        client.Setup(value=>value.GetWorkOrdersAsync(null,null)).ReturnsAsync([Header(source)]);await state.LoadOrdersAsync(client.Object,()=>Task.FromResult<string?>(source.Query.FieldId));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReadAsync(client.Object,"foreign-order"));client.Verify(value=>value.ReadFinancialSourcesAsync(It.IsAny<CompanyWorkOrderFinancialQuery>(),It.IsAny<CancellationToken>()),Times.Never);
        Assert.True(company.TrySelect(company.Clear(),source.Context.ActorUserId,new(source.Context.Company,["FinanceRole"],[])));Assert.Empty(state.Orders);await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>state.LoadOrdersAsync(client.Object,()=>Task.FromResult<string?>(source.Query.FieldId)));
    }
    private sealed class Response(CompanyWorkOrderFinancialSources source,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/financial-sources",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(source)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")]
    public async Task TypedDiscoveryCannotPublishAnotherCompanyOrReadWithoutVisibility(string mode)
    {
        var source=Source();using var company=Company(source,mode=="permission"?[]:["Accounting.View"]);var handler=new Response(source,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(handler){BaseAddress=new("https://fixture.invalid/")};
        var client=new WorkOrderServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));var error=await Record.ExceptionAsync(()=>client.ReadFinancialSourcesAsync(source.Query));
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,handler.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"FinanceRole")],"fixture"))));}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private sealed class Components:IComponentActivator{internal WorkOrderFinancialWorkbench Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderFinancialWorkbench panel)Panel=panel;return result;}}
    [Theory][InlineData("approved")][InlineData("empty")][InlineData("unrecorded")][InlineData("changed")][InlineData("permission")]
    public async Task ActualFinanceWorkbenchFindsRecordedSourcesWithoutHeaderDefaultTotals(string mode)
    {
        var source=Source(mode);using var company=Company(source,mode=="permission"?[]:["Accounting.View","Accounting.PrepareWorkOrderBudget","Accounting.ReviewWorkOrderBudget","Accounting.RecordWorkOrderActualCost"]);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(source.Query.FieldId);
        var client=new Mock<IWorkOrderServiceClient>();var reads=0;var order=Header(source);order.BudgetAmt=99999;order.ActualAmt=88888;
        client.Setup(value=>value.GetWorkOrdersAsync(null,null)).ReturnsAsync([order]);client.Setup(value=>value.ReadFinancialSourcesAsync(source.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(source);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderFinancialWorkspaceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);
        services.AddSingleton<AuthenticationStateProvider>(new Auth(source.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderFinancialWorkbench>();var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            async Task Invoke(string method,params object[] arguments)=>await (Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,arguments)!;
            await Invoke("LoadOrdersAsync");if(mode=="permission"){Assert.Equal(0,reads);return;}await Invoke("SelectOrder",source.Query.WorkOrderId);await Invoke("ReadAsync");await Task.Yield();
            var html=root.ToHtmlString();Assert.Equal(1,reads);Assert.DoesNotContain("99999",html);Assert.DoesNotContain("88888",html);Assert.DoesNotContain("Private reviewer rationale",html);
            if(mode=="empty"){Assert.Contains("contains no recorded AFE",html);Assert.Contains("Prepare budget in",html);return;}
            Assert.Contains("recorded actual costs 12.345",html);Assert.Contains("financialAfe=",html);Assert.Contains("financialBook=",html);
            if(mode=="approved")Assert.Contains("Open recorded labor and material valuation",html);else Assert.DoesNotContain("Open recorded labor and material valuation",html);
            if(mode=="unrecorded")Assert.Contains("Open independent budget review",html);if(mode=="changed")Assert.Contains("Original budget work scope changed.",html);
            company.Clear();await Task.Yield();Assert.DoesNotContain(source.Budgets[0].Budget.Query.AfeId,root.ToHtmlString());
        });
        client.Verify(value=>value.CreateBudgetAsync(It.IsAny<CompanyWorkOrderBudgetCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
