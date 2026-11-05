using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Pages.PPDM39.Accounting;
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
using Beep.OilandGas.ApiService.Tests;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Microsoft.Extensions.Logging.Abstractions;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WarehouseScreenTests
{
    private sealed class Authentication(string actor="actor",bool role=true):AuthenticationStateProvider
    {
        private ClaimsPrincipal user=new(new ClaimsIdentity(role?[new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"Warehouse")]:[new(ClaimTypes.NameIdentifier,actor)],"app"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(user));
        public void Change(string actor){user=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"Warehouse")],"app"));NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Navigation(string? id):NavigationManager
    {public Navigation():this(null){} protected override void NavigateToCore(string uri,bool forceLoad){} public void InitializePage()=>Initialize("https://warehouse.test/",id is null?"https://warehouse.test/warehouse":"https://warehouse.test/warehouse/items/"+System.Uri.EscapeDataString(id));}
    private sealed class NoBrowser:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>throw new InvalidOperationException("No browser");public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>InvokeAsync<T>(name,args);}
    [Theory][InlineData("queue")][InlineData("detail")][InlineData("empty")][InlineData("account")][InlineData("role")][InlineData("grant")][InlineData("auth-change")]
    public async Task ActualWarehousePageShowsOnlyCurrentAuthorizedEvidence(string mode)
    {
        using var company=InventoryWorkspaceStateTests.Company();using var state=new InventoryWorkspaceState(company);var item=InventoryWorkspaceStateTests.Item();var calls=0;
        if(mode=="grant")Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Warehouse"],[])));
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
        client.Setup(value=>value.GetCompanyInventoryItemsAsync()).ReturnsAsync(()=>{++calls;return mode=="empty"?[]:new List<INVENTORY_ITEM>{item};});
        client.Setup(value=>value.GetCompanyInventorySourceAsync(item.INVENTORY_ITEM_ID)).ReturnsAsync(()=>{++calls;return CompanyInventorySourceReview.Create(new("tenant","company"),"actor",item,[new(){INVENTORY_TRANSACTION_ID="movement",INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,ACTIVE_IND="N",NOTES="<script>reason</script>"}]);});
        var authentication=new Authentication(mode=="account"?"other":"actor",mode!="role");var navigation=new Navigation(mode=="detail"?item.INVENTORY_ITEM_ID:null);navigation.InitializePage();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>());services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<NavigationManager>(navigation);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<WarehouseWorkbench>(ParameterView.FromDictionary(mode=="detail"?new Dictionary<string,object?>{{"Id",item.INVENTORY_ITEM_ID}}:new()));var html=root.ToHtmlString();
            if(mode is "account" or "role" or "grant"){Assert.Equal(0,calls);Assert.Null(state.Rows);Assert.DoesNotContain("item%2Fa%3Fb",html);}
            else if(mode=="empty")Assert.Contains("No active company-owned inventory items",html);
            else if(mode=="detail"){Assert.Contains("movement",html);Assert.Contains("&lt;script&gt;reason",html);Assert.Contains("Recorded on-hand quantity: Unavailable",html);Assert.DoesNotContain("<script>",html);}
            else{Assert.Contains("/warehouse/items/item%2Fa%3Fb",html);Assert.Contains("Company warehouse evidence queues",html);Assert.Contains("not summed across units or currencies",html);}
            if(mode=="auth-change"){authentication.Change("other");await Task.Yield();Assert.Null(state.Rows);Assert.DoesNotContain("item%2Fa%3Fb",root.ToHtmlString());}
        });
    }
    [Fact] public async Task CompleteWarehouseDetailReadsThroughTypedClientHttpAndNativeSourceOwner()
    {
        var item=InventoryWorkspaceStateTests.Item();var native=new Mock<IDataSource>();var creator=new ClassCreator(Mock.Of<IDMEEditor>());
        native.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>creator.ConvertToEntityStructure(request.EntityName=="INVENTORY_ITEM"?typeof(INVENTORY_ITEM):typeof(INVENTORY_TRANSACTION)));
        native.Setup(value=>value.GetEntityAsync("INVENTORY_ITEM",It.IsAny<List<AppFilter>>())).ReturnsAsync(new[]{item});
        native.Setup(value=>value.GetEntityAsync("INVENTORY_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync(new[]{new INVENTORY_TRANSACTION{INVENTORY_TRANSACTION_ID="recorded-movement",INVENTORY_ITEM_ID=item.INVENTORY_ITEM_ID,ACTIVE_IND="N",NOTES="Physical movement reason"}});
        var editor=new Mock<IDMEEditor>();editor.Setup(value=>value.GetDataSource("bound")).Returns(native.Object);
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Inventory.View"],configureAccounting:(services,context)=>{services.AddSingleton(CompanyInventorySourceTests.CreateService(editor,()=>Task.FromResult("bound")));context.Roles=["Warehouse"];});
        host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","Warehouse");
        using var company=InventoryWorkspaceStateTests.Company();using var state=new InventoryWorkspaceState(company);
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var navigation=new Navigation(item.INVENTORY_ITEM_ID);navigation.InitializePage();
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>(Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IFieldSelectionContext>());services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(new Authentication());services.AddSingleton<NavigationManager>(navigation);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<WarehouseWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",item.INVENTORY_ITEM_ID}}));
            Assert.Equal(item.INVENTORY_ITEM_ID,state.Detail!.Id);Assert.NotNull(state.Revision);Assert.Null(state.Detail.Quantity);Assert.Equal("recorded-movement",Assert.Single(state.Movements!).Id);
            Assert.Contains("Physical movement reason",root.ToHtmlString());Assert.Contains("Source content review",root.ToHtmlString());
        });
    }
}
#endif
