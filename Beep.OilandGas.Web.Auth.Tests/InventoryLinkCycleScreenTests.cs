#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Pages.PPDM39.Accounting;
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
public sealed class InventoryLinkCycleScreenTests
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
    [Theory][InlineData("resolved")][InlineData("pending")][InlineData("grant")][InlineData("account")][InlineData("unknown")][InlineData("auth-change")][InlineData("choice")][InlineData("read-only")][InlineData("bad-reason")]
    public async Task ProcurementPageAndActualClientApiNativeJourneyRetainAuditAndOriginalCycleIntent(string mode)
    {
        var f=await InventoryLinkCycleTests.Fixture.Create(pending:mode=="pending");var baseline=f.Native.Writes;
        using var company=InventoryLinkCycleWorkspaceTests.Company();
        if(mode is "grant" or "read-only")company.TrySelect(company.Clear(),"actor",new(f.Command.Company,["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"]));
        var inventory=CompanyInventorySourceTests.CreateService(f.Native.Editor,()=>Task.FromResult(f.Native.Connection));
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Procurement.View","Inventory.View","Procurement.ReopenInventoryLink"],configureAccounting:(services,context)=>{services.AddSingleton(inventory);services.AddSingleton(f.Service);services.AddSingleton(f.Existing.Existing.Existing.AP);});
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var authentication=new Authentication(mode=="account"?"other":"actor");var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;var state=provider.GetRequiredService<PurchaseOrderWorkspaceState>();
            Task Invoke(string name)=>(Task)typeof(ProcurementWorkbench).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
            void Render()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            void Field(string name,object value)=>typeof(ProcurementWorkbench).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,value);
            typeof(ProcurementWorkbench).GetMethod("CycleLineChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["po-line"]);
            if(mode=="read-only"){await Invoke("LoadLinkCycleHistory");Render();Assert.Contains("No archived cycles are recorded",root.ToHtmlString());Assert.DoesNotContain("Explicit cycle action",root.ToHtmlString());Assert.Equal(baseline,f.Native.Writes);return;}
            if(mode!="choice")typeof(ProcurementWorkbench).GetMethod("CycleActionChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,[f.Command.Action]);
            await Invoke("ReviewLinkCycle");Render();
            if(mode is "grant" or "account" or "choice"){Assert.Null(state.LinkCycleReview);Assert.DoesNotContain("Reviewed inventory-link cycle",root.ToHtmlString());Assert.Equal(baseline,f.Native.Writes);return;}
            Assert.Contains("Reviewed inventory-link cycle",root.ToHtmlString());Field("_cycleReason",mode=="bad-reason"?"two\nlines":"Explicit screen cycle reason.");
            if(mode is "unknown" or "auth-change")f.Native.FailCommit=true;
            await Invoke("RecordLinkCycle");Render();
            if(mode=="bad-reason"){Assert.False(state.HasUnconfirmedLinkCycleIntent);Assert.Equal(baseline,f.Native.Writes);return;}
            if(mode is "unknown" or "auth-change")
            {
                Assert.True(state.HasUnconfirmedLinkCycleIntent);Assert.Null(state.LinkCycleResult);Assert.Contains("Reconcile original cycle intent",root.ToHtmlString());
                Assert.DoesNotContain("Hand off this source to a workflow",root.ToHtmlString());
                if(mode=="auth-change"){authentication.Change();await Task.Yield();Assert.Null(state.Detail);Assert.Null(state.LinkCycleHistory);Assert.True(state.HasUnconfirmedLinkCycleIntent);return;}
                await Invoke("Reload");Render();Assert.NotNull(state.Detail);Assert.True(state.HasUnconfirmedLinkCycleIntent);Assert.DoesNotContain("Hand off this source to a workflow",root.ToHtmlString());
                var original=state.LinkCycleIntent!.Command;Field("_cycleReason","Replacement reason ignored.");Field("_cycleAction",PurchaseOrderInventoryLinkCycleAction.SupersedePending);f.Native.FailCommit=false;
                await Invoke("RecordLinkCycle");Render();Assert.Equal(original,state.LinkCycleIntent!.Command);
            }
            Assert.NotNull(state.LinkCycleResult);Assert.False(state.HasUnconfirmedLinkCycleIntent);Assert.Equal(baseline+2,f.Native.Writes);
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());Assert.Contains("Cycle reopening confirmed",html);Assert.Contains("Original preparer: actor",html);
            Assert.Contains("Hand off this source to a workflow",html);
            if(mode=="pending")Assert.Contains("no independent decision is recorded",html);else Assert.Contains("Reviewer: reviewer",html);
            Assert.False(state.Lines![0].HasPreparationEvidence);Assert.Null(f.Stored.INVENTORY_LINK_REVIEW_JSON);
        });
    }
}
#endif
