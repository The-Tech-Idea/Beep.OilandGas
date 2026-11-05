#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Components.Workflow;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class MaterialReservationQueueTests
{
    internal static CompanyMaterialReservationQueue Queue(bool withConsumption=false)
    {
        var company=new CompanyScope("tenant","company");var utc=DateTime.UtcNow;var context=new FieldSelectionReview(company,new("production","fingerprint"),"actor","authority","field");
        CompanyMaterialReservationQueueEntry Entry(int n)
        {
            var id=Guid.NewGuid();var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order/a?b");var from=n==1?utc.AddHours(1):utc.AddHours(-2);var to=n==2?utc.AddHours(-1):utc.AddHours(2);
            var row=new CompanyWorkOrderMaterialReservationFacts(company,task,id,CompanyWorkOrderMaterialReservationFacts.IdFor(company,id),new string('A',64),n==3?"other":"actor","item/a?b",n+1,n==1?"bbl":"ea","warehouse","bin",from,to);
            var entry=new CompanyMaterialReservationQueueEntry(row,n==3?new(row.AllocationId,new string('B',64),utc.AddMinutes(-1),row.Quantity):null);
            if(withConsumption && n==0)
            {
                var intent=Guid.NewGuid();var usage=new CompanyWorkOrderMaterialUsageFacts(CompanyWorkOrderMaterialUsageFacts.IdFor(company,intent),intent,
                    company,task,row.ItemId,row.AllocationId,row.Revision,"recorded-start",new string('C',64),"executor",row.Quantity,row.Unit,row.Warehouse,row.Location,utc.AddMinutes(-2),utc.AddMinutes(-1));
                entry=entry with{Usages=[usage]};
            }
            return entry;
        }
        return CompanyMaterialReservationQueue.Create(context,Enumerable.Range(0,4).Select(Entry).ToArray(),utc);
    }
    [Fact] public async Task StateCountsRecordsAcrossUnitsAndFreezesExactMembership()
    {
        using var company=InventoryWorkspaceStateTests.Company();using var state=new InventoryWorkspaceState(company);var queue=Queue();Assert.Null(state.ReservationCount());await state.LoadMaterialReservationsAsync("field",_=>Task.FromResult(queue));Assert.Equal(4,state.ReservationCount());
        foreach(var status in Enum.GetValues<CompanyMaterialReservationStatus>()){Assert.Equal(status==CompanyMaterialReservationStatus.Consumed?0:1,state.ReservationCount(status));state.FilterReservations(status);if(status==CompanyMaterialReservationStatus.Consumed)Assert.Empty(state.VisibleReservations!);else Assert.Equal(status,Assert.Single(state.VisibleReservations!).StatusAt(queue.ComparedAtUtc));}
        ((CompanyMaterialReservationQueueEntry[])queue.Rows)[0]=queue.Rows[1];Assert.Equal(4,state.ReservationCount());Assert.Throws<ArgumentOutOfRangeException>(()=>state.FilterReservations((CompanyMaterialReservationStatus)99));company.Clear();Assert.Null(state.ReservationCount());
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("field")][InlineData("dispose")]
    public async Task LateStateCannotPublishWhenOriginalSelectionIsLost(string mode)
    {
        using var company=InventoryWorkspaceStateTests.Company();using var state=new InventoryWorkspaceState(company);var pending=new TaskCompletionSource<CompanyMaterialReservationQueue>();var load=state.LoadMaterialReservationsAsync("field",_=>pending.Task);
        if(mode=="company")InventoryWorkspaceStateTests.Select(company,"other");if(mode=="account")InventoryWorkspaceStateTests.Select(company,actor:"other");if(mode=="field")state.ClearReservations();if(mode=="dispose")state.Dispose();pending.SetResult(Queue());await load;Assert.Null(state.MaterialReservations);
    }
    private sealed class Transport(Func<HttpRequestMessage,Task<HttpResponseMessage>> send):HttpMessageHandler{protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)=>send(request);}
    [Theory][InlineData("original")][InlineData("foreign")][InlineData("late")][InlineData("denied")]
    public async Task TypedClientUsesExplicitFieldAndRejectsForeignOrLateEvidence(string mode)
    {
        using var company=InventoryWorkspaceStateTests.Company();var queue=Queue();if(mode=="foreign")queue=queue with{Revision=new string('F',64)};var calls=0;
        using var http=new HttpClient(new Transport(request=>{calls++;Assert.Equal("/api/accounting/traditional/inventory/company/work-order-reservations?fieldId=field",request.RequestUri!.PathAndQuery);if(mode=="late")company.Clear();return Task.FromResult(new HttpResponseMessage(mode=="denied"?HttpStatusCode.Forbidden:HttpStatusCode.OK){Content=JsonContent.Create(queue)});})){BaseAddress=new("https://queue.test/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{var result=await client.GetCompanyMaterialReservationsAsync("field");Assert.Equal(4,result.Count());});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(1,calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"Warehouse")],"app"))));}
    private sealed class Components:IComponentActivator{internal CompanyMaterialReservationQueuePanel Panel=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is CompanyMaterialReservationQueuePanel panel)Panel=panel;return component;}}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("original")][InlineData("consumed")][InlineData("actor")][InlineData("late-field")][InlineData("error")]
    public async Task ActualDashboardShowsOriginalLinksFiltersAndClearsLateField(string mode)
    {
        using var company=InventoryWorkspaceStateTests.Company();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Warehouse"],["Inventory.View","Inventory.ReserveWorkOrderMaterial"])));using var state=new InventoryWorkspaceState(company);var fields=new Mock<IFieldSelectionContext>();fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("field");var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);var calls=0;var queue=Queue(mode=="consumed");
        client.Setup(c=>c.GetCompanyMaterialReservationsAsync("field")).Returns(()=>{calls++;if(mode=="error")throw new InvalidOperationException("unavailable");if(mode=="late-field"){fields.Setup(f=>f.GetCurrentFieldIdAsync()).ReturnsAsync("other");fields.Raise(f=>f.CurrentFieldChanged+=null!,"other");}return Task.FromResult(queue);});
        var activator=new Components();var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator>(activator);services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(fields.Object);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"actor"));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyMaterialReservationQueuePanel>();Assert.DoesNotContain("All (4)",root.ToHtmlString());await (Task)typeof(CompanyMaterialReservationQueuePanel).GetMethod("ReviewAsync",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(activator.Panel,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(activator.Panel,null);
            var html=root.ToHtmlString();if(mode is "original" or "consumed")
            {
                Assert.Contains("All (4)",html);Assert.Contains(mode=="consumed"?"Active (0)":"Active (1)",html);Assert.Contains(mode=="consumed"?"Consumed (1)":"Consumed (0)",html);Assert.Contains("Upcoming (1)",html);Assert.Contains("Expired (1)",html);Assert.Contains("Released (1)",html);Assert.Contains("order%2Fa%3Fb",html);Assert.Contains("materialAllocationId=",html);Assert.Contains("/ppdm39/process/instances/",html);Assert.DoesNotContain("COMMAND_JSON",html);
                Assert.Contains("Original reservation:",html);Assert.Contains("Remaining reserved:",html);
                Assert.Contains(mode=="consumed"?"Recorded physical use: 1 ea":"Recorded physical use: unavailable",html);
                state.FilterReservations(CompanyMaterialReservationStatus.Released);Assert.Single(state.VisibleReservations!);company.Clear();await Task.Yield();Assert.Null(state.MaterialReservations);
            }
            else{Assert.Null(state.MaterialReservations);Assert.DoesNotContain("materialAllocationId=",html);}Assert.Equal(mode=="actor"?0:1,calls);
        });
    }
}
#endif
