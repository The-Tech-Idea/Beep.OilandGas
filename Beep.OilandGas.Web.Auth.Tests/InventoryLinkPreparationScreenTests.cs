#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Pages.PPDM39.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkPreparationScreenTests
{
    private sealed class Authentication(string actor):AuthenticationStateProvider
    {
        private string actor=actor;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"))));
        public void Change(){actor="other";NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Navigation:NavigationManager
    {public Navigation()=>Initialize("https://render.invalid/","https://render.invalid/procurement/orders/order");protected override void NavigateToCore(string uri,bool forceLoad){}}
    private sealed class Components(IServiceProvider services):IComponentActivator
    {public ProcurementWorkbench? Page;public IComponent CreateInstance(Type type){var instance=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(instance is ProcurementWorkbench page)Page=page;return instance;}}
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("account")][InlineData("failed")][InlineData("unknown")][InlineData("auth-change")][InlineData("empty")]
    public async Task WholeProcurementHandlersReviewSaveAndReconcileWithCurrentAccountAndGrant(string mode)
    {
        var f=await InventoryLinkPreparationWorkspaceTests.Fixture.Create();using var company=new CompanyClientContext();
        string[] grants=mode=="grant"?["Procurement.View","Inventory.View"]:["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"];
        Assert.True(company.TrySelect(company.Clear(),"actor",new(f.Review.Purchase.Company,["ConfiguredCompanyRole"],grants)));
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);var targetReads=0;var writes=0;PurchaseOrderInventoryLinkProposalSubmission? original=null;
        client.Setup(value=>value.GetPurchaseOrderAsync("order")).ReturnsAsync(()=>f.Source());
        client.Setup(value=>value.GetCompanyInventoryItemsAsync()).Returns(()=>{++targetReads;return Task.FromResult(mode=="empty"?new List<INVENTORY_ITEM>():new List<INVENTORY_ITEM>{f.Review.Target.Item});});
        client.Setup(value=>value.GetInventoryLinkPreparationAsync("order","po-line",f.Review.Target.Item.INVENTORY_ITEM_ID)).Returns(()=>mode=="failed"?Task.FromException<PurchaseOrderInventoryLinkProposalReview>(new HttpRequestException()):Task.FromResult(f.Review));
        client.Setup(value=>value.PrepareInventoryLinkAsync(It.IsAny<PurchaseOrderInventoryLinkProposalSubmission>())).Returns((PurchaseOrderInventoryLinkProposalSubmission value)=>{
            ++writes;original??=CompanyInventorySourceReview.Copy(value);Assert.Equal(original.Command,value.Command);
            if(f.Stored is null)f.Record(value);if(mode is "unknown" or "auth-change" && writes==1)return Task.FromException<PO_LINE_ITEM>(new HttpRequestException("Unknown preparation outcome."));
            return Task.FromResult(CompanyInventorySourceReview.Copy(f.Stored!));});
        var authentication=new Authentication(mode=="account"?"other":"actor");var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,Beep.OilandGas.ApiService.Tests.APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            Task Invoke(string name,params object?[] args)=>(Task)typeof(ProcurementWorkbench).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,args)!;
            void Change(string name,string value)=>typeof(ProcurementWorkbench).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,[value]);
            void Render()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            await Invoke("LoadLinkTargets");var state=provider.GetRequiredService<PurchaseOrderWorkspaceState>();
            if(mode=="empty"){Render();Assert.Contains("No active owned inventory targets were returned",root.ToHtmlString());Assert.Equal(0,writes);return;}
            if(mode is "grant" or "account"){Render();Assert.Equal(0,targetReads);Assert.Equal(0,writes);Assert.Null(state.LinkPreparationReview);return;}
            Change("LinkLineChanged","po-line");Change("LinkTargetChanged",f.Review.Target.Item.INVENTORY_ITEM_ID);await Invoke("ReviewLinkPreparation");Render();
            if(mode=="failed"){Assert.Null(state.LinkPreparationReview);Assert.DoesNotContain("Reviewed inventory-link preparation source",root.ToHtmlString());Assert.Equal(0,writes);return;}
            Assert.Contains("Reviewed inventory-link preparation source",root.ToHtmlString());Assert.Contains("/warehouse/items/target%2Fa%3Fb",root.ToHtmlString());Assert.Contains("Recorded on-hand quantity: Unavailable",root.ToHtmlString());
            typeof(ProcurementWorkbench).GetField("_linkReason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Reviewed target warehouse and unit.");await Invoke("RecordLinkPreparation");Render();
            if(mode is "unknown" or "auth-change")
            {
                Assert.True(state.HasUnconfirmedLinkPreparationIntent);Assert.Contains("Reconcile original preparation intent",root.ToHtmlString());Assert.DoesNotContain("Receiver outcome is unconfirmed",root.ToHtmlString());
                if(mode=="auth-change"){authentication.Change();await Task.Yield();Assert.Null(state.Detail);Assert.True(state.HasUnconfirmedLinkPreparationIntent);Assert.Null(state.LinkPreparationResult);return;}
                await Invoke("RecordLinkPreparation");Render();Assert.Equal(2,writes);
            }
            Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResult);Assert.Contains("Inventory-link preparation confirmed for",root.ToHtmlString());Assert.Contains("Verified inventory-link preparation",root.ToHtmlString());Assert.Null(state.LinkPreparationResult!.INVENTORY_ITEM_ID);
        });
    }
}
#endif
