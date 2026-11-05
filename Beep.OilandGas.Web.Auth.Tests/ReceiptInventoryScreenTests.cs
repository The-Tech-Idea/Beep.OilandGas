using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
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
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ReceiptInventoryScreenTests
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
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("account")][InlineData("failed")][InlineData("auth-change")]
    public async Task CompleteProcurementHandlerDisplaysOnlyCurrentAuthorizedLinkedEvidence(string mode)
    {
        var review=await ReceiptInventoryWorkspaceTests.Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(review.Purchase.Company,["ConfiguredCompanyRole"],mode=="grant"?["Procurement.View"]:["Procurement.View","Inventory.View"])));
        var client=new Mock<IAccountingServiceClient>(MockBehavior.Strict);var reads=0;
        client.Setup(value=>value.GetPurchaseOrderAsync("order")).ReturnsAsync(review.Purchase);
        client.Setup(value=>value.GetPurchaseReceiptInventoryReviewAsync("order","receipt")).Returns(()=>{++reads;return mode=="failed"?Task.FromException<PurchaseReceiptInventoryReview>(new HttpRequestException()):Task.FromResult(review);});
        var authentication=new Authentication(mode=="account"?"other":"actor");
        var services=new ServiceCollection();services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(client.Object);services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,Beep.OilandGas.ApiService.Tests.APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            await (Task)typeof(ProcurementWorkbench).GetMethod("ReviewInventoryRow",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["receipt"])!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            var state=provider.GetRequiredService<PurchaseOrderWorkspaceState>();var html=root.ToHtmlString();
            if(mode is "valid" or "auth-change")
            {Assert.NotNull(state.InventoryLink);Assert.Contains("Verified receipt inventory source",html);Assert.Contains("/warehouse/items/item%2Fa%3Fb",html);Assert.Contains("Recorded on-hand quantity: Unavailable",html);Assert.Contains("supplier return execution and financial posting remain unconfirmed",html);}
            else{Assert.Null(state.InventoryLink);Assert.DoesNotContain("Verified receipt inventory source",html);if(mode!="failed")Assert.Equal(0,reads);}
            if(mode=="auth-change"){authentication.Change();await Task.Yield();Assert.Null(state.InventoryLink);Assert.Null(state.Detail);Assert.DoesNotContain("Verified receipt inventory source",root.ToHtmlString());}
        });
    }
}
#endif
