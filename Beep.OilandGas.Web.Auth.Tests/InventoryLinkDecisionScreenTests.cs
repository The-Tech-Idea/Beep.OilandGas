using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Tests;
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
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InventoryLinkDecisionScreenTests
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
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("grant")][InlineData("account")][InlineData("failed")][InlineData("unknown")][InlineData("auth-change")][InlineData("choice")][InlineData("resolution")]
    public async Task ExistingProcurementHandlersReviewExplicitlyDecideAndRecoverIndependentIntent(string mode)
    {
        var f=await InventoryLinkDecisionTests.Fixture.Create();using var company=InventoryLinkDecisionWorkspaceTests.Company(mode=="resolution"?"actor":"reviewer");
        if(mode=="grant")Assert.True(company.TrySelect(company.Clear(),"reviewer",new(f.Command.Company,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View","Procurement.PrepareInventoryLink"])));
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);var reads=0;var writes=0;PurchaseOrderInventoryLinkDecisionSubmission? original=null;
        client.Setup(value=>value.GetPurchaseOrderAsync("order")).Returns(()=>f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order",company.Selection!.UserId));
        client.Setup(value=>value.GetInventoryLinkResolutionAsync(It.IsAny<PurchaseOrderInventoryLinkProposalSubmission>())).Returns(async(PurchaseOrderInventoryLinkProposalSubmission value)=>{
            var source=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","actor"))!;var result=new PurchaseOrderInventoryLinkResolution(source,"binding");result.Require(value);return result;});
        client.Setup(value=>value.GetInventoryLinkDecisionReviewAsync("order","po-line")).Returns(()=>{++reads;return mode=="failed"?Task.FromException<PurchaseOrderInventoryLinkDecisionReview>(new HttpRequestException()):Task.FromResult(f.Review);});
        client.Setup(value=>value.RecordInventoryLinkDecisionAsync(It.IsAny<PurchaseOrderInventoryLinkDecisionSubmission>())).Returns(async(PurchaseOrderInventoryLinkDecisionSubmission value)=>{
            ++writes;original??=CompanyInventorySourceReview.Copy(value);Assert.Equal(original.Command,value.Command);f.Command=value.Command;f.Review=value.Review;var row=await f.Record();
            if(mode is "unknown" or "auth-change" && writes==1)throw new HttpRequestException("Unknown decision result.");return row;});
        var authentication=new Authentication(mode=="account"?"other":mode=="resolution"?"actor":"reviewer");var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            Task Invoke(string name)=>(Task)typeof(ProcurementWorkbench).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
            void Render()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            var state=provider.GetRequiredService<PurchaseOrderWorkspaceState>();
            if(mode=="resolution")
            {
                await state.LoadDetailAsync("order",_=>Task.FromResult<PurchaseOrderSourceReview?>(f.PreparationReview.Purchase));
                await state.LoadLinkTargetsAsync(()=>Task.FromResult(new List<INVENTORY_ITEM>{f.Item}));await state.LoadLinkPreparationReviewAsync("po-line","target",(_,_,_)=>Task.FromResult(f.PreparationReview));
                await Assert.ThrowsAsync<HttpRequestException>(()=>state.RecordLinkPreparationAsync(f.Preparation.Reason,value=>{f.Preparation=value.Command;f.PreparationReview=value.Review;return Task.FromException<PO_LINE_ITEM>(new HttpRequestException());},_=>throw new Exception()));
                f.Stored.INVENTORY_LINK_REVIEW_JSON=PurchaseOrderInventoryLinkProposalProof.Create(f.Preparation,f.PreparationReview,f.Stored);
                var decisionSource=(await f.Existing.Existing.PO.GetSourceReviewAsync(f.Command.Company,"order","reviewer"))!;f.Review=PurchaseOrderInventoryLinkDecisionReview.Create(decisionSource,f.Target("reviewer"),"po-line","binding");f.Command=f.Command with{ReviewRevision=f.Review.Revision};await f.Record();
                await Invoke("ReviewPreparationResolution");Render();Assert.False(state.HasUnconfirmedLinkPreparationIntent);Assert.NotNull(state.LinkPreparationResolution);Assert.Contains("Original preparation independently resolved",root.ToHtmlString());Assert.Equal(0,writes);return;
            }
            typeof(ProcurementWorkbench).GetMethod("DecisionLineChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["po-line"]);await Invoke("ReviewLinkDecision");Render();
            if(mode is "grant" or "account" or "failed"){Assert.Null(state.LinkDecisionReview);Assert.DoesNotContain("Reviewed independent inventory-link decision",root.ToHtmlString());Assert.Equal(0,writes);if(mode!="failed")Assert.Equal(0,reads);return;}
            Assert.Contains("Reviewed independent inventory-link decision",root.ToHtmlString());Assert.Contains("Preparer: actor",root.ToHtmlString());Assert.Contains("Recorded on-hand quantity: Unavailable",root.ToHtmlString());
            typeof(ProcurementWorkbench).GetField("_decisionReason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Independent source/target review.");
            if(mode!="choice")typeof(ProcurementWorkbench).GetField("_linkChoice",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,mode=="reject"?PurchaseOrderInventoryLinkDecision.Reject:PurchaseOrderInventoryLinkDecision.Approve);
            await Invoke("RecordLinkDecision");Render();
            if(mode=="choice"){Assert.Equal(0,writes);Assert.False(state.HasUnconfirmedLinkDecisionIntent);return;}
            if(mode is "unknown" or "auth-change")
            {
                Assert.True(state.HasUnconfirmedLinkDecisionIntent);Assert.Contains("Reconcile original independent decision",root.ToHtmlString());
                if(mode=="auth-change"){authentication.Change();await Task.Yield();Assert.Null(state.Detail);Assert.True(state.HasUnconfirmedLinkDecisionIntent);Assert.Null(state.LinkDecisionResult);return;}
                typeof(ProcurementWorkbench).GetField("_linkChoice",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,PurchaseOrderInventoryLinkDecision.Reject);
                await Invoke("RecordLinkDecision");Render();Assert.Equal(2,writes);
            }
            Assert.False(state.HasUnconfirmedLinkDecisionIntent);Assert.NotNull(state.LinkDecisionResult);Assert.Contains("Independent inventory-link decision confirmed",root.ToHtmlString());Assert.Contains("Verified inventory-link decision",root.ToHtmlString());Assert.Equal(mode=="reject"?"current-item":"target",state.LinkDecisionResult!.INVENTORY_ITEM_ID);
        });
    }
}
#endif
