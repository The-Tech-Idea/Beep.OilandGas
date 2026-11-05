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
public sealed class ReceiptStockScreenTests
{
    private sealed class Authentication(string actor):AuthenticationStateProvider
    {
        private string actor=actor;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"))));
        public void Change(){actor="other";NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Navigation(string order,string receipt,string? intent=null):NavigationManager
    {public void Start()=>Initialize("https://render.invalid/","https://render.invalid"+(intent is null?InventoryWorkspaceState.StockRoute(order,receipt):InventoryWorkspaceState.SavedStockRoute(intent)));protected override void NavigateToCore(string uri,bool forceLoad){}}
    private sealed class Components(IServiceProvider services):IComponentActivator
    {public ReceiptStockWorkbench? Page;public WarehouseWorkbench? Warehouse;public IComponent CreateInstance(Type type){var instance=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(instance is ReceiptStockWorkbench page)Page=page;if(instance is WarehouseWorkbench warehouse)Warehouse=warehouse;return instance;}}
    [Theory][InlineData("recorded")][InlineData("empty")][InlineData("later")][InlineData("account")][InlineData("paging")][InlineData("historical-item")]
    public async Task WarehouseLandingFindsActualOriginalAccountIntentsAndTheirRecoveryRoutes(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var intent=f.Submission.Command.IntentId;if(mode!="empty")await f.Record();if(mode=="later")f.Item.LOCATION_ID="later bin";
        if(mode=="paging")for(var i=0;i<25;i++){await f.Prepare(quantity:0.01m);await f.Record();}
        if(mode=="historical-item")f.Native.Rows.RemoveAll(row=>row is INVENTORY_ITEM);
        var baseline=f.Native.Writes;using var company=ReceiptStockWorkspaceTests.Company();
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"])));
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Procurement.View","Inventory.View"],configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var navigation=new Navigation("order","receipt");navigation.Start();var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<InventoryWorkspaceState>();services.AddSingleton<AuthenticationStateProvider>(new Authentication(mode=="account"?"other":"actor"));services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<NavigationManager>(navigation);services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<WarehouseWorkbench>();var page=((Components)provider.GetRequiredService<IComponentActivator>()).Warehouse!;var state=provider.GetRequiredService<InventoryWorkspaceState>();
            await (Task)typeof(WarehouseWorkbench).GetMethod("Discover",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            if(mode=="paging")
            {
                Assert.Equal(26,state.StockPage!.TotalRecords);Assert.Equal(25,state.StockIndex!.Entries.Count);Assert.True(state.StockPage.HasNextPage);
                var first=state.StockIndex.Entries.Select(row=>row.Command.IntentId).ToArray();
                await (Task)typeof(WarehouseWorkbench).GetMethod("NextPage",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                Assert.Equal(2,state.StockPage!.PageNumber);Assert.Single(state.StockIndex!.Entries);Assert.False(state.StockPage.HasNextPage);Assert.True(state.StockPage.HasPreviousPage);
                Assert.DoesNotContain(state.StockIndex.Entries[0].Command.IntentId,first);
                await (Task)typeof(WarehouseWorkbench).GetMethod("PreviousPage",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                Assert.Equal(1,state.StockPage!.PageNumber);Assert.Equal(first,state.StockIndex!.Entries.Select(row=>row.Command.IntentId));Assert.Equal(baseline,f.Native.Writes);return;
            }
            if(mode=="account")Assert.Null(state.StockIndex);
            else if(mode=="empty"){Assert.Empty(state.StockIndex!.Entries);Assert.Contains("No recorded stock intents",root.ToHtmlString());}
            else{Assert.Single(state.StockIndex!.Entries);Assert.Contains(InventoryWorkspaceState.SavedStockRoute(intent),root.ToHtmlString());Assert.Contains(mode is "later" or "historical-item"?"Current source has changed":"Current source matches this movement",root.ToHtmlString());if(mode=="historical-item")Assert.Contains("Current owned source is unavailable",root.ToHtmlString());}
            Assert.Equal(baseline,f.Native.Writes);
        });
    }
    [Theory][InlineData("exact")][InlineData("unknown")][InlineData("item-changed")][InlineData("purchase-changed")][InlineData("later-receipt")]
    [InlineData("missing")][InlineData("grant")][InlineData("account")][InlineData("auth-change")][InlineData("bad-route")][InlineData("route-change")]
    [InlineData("historical-item")][InlineData("historical-purchase")][InlineData("historical-both")]
    public async Task FreshReceiptRecoveryPageUsesActualSavedEvidenceWithoutReplayingStock(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();await f.Initialize();var original=f.Submission;
        if(mode=="unknown"){f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());f.Native.FailCommit=false;}else await f.Record();
        if(mode=="item-changed")f.Item.LOCATION_ID="later bin";if(mode=="purchase-changed")f.Receiving.Existing.Order.DESCRIPTION="later order";
        if(mode=="later-receipt"){await f.Prepare(quantity:4);await f.Record();}if(mode=="missing")f.Native.Rows.RemoveAll(row=>row is INVENTORY_STOCK_INTENT);
        if(mode is "historical-purchase" or "historical-both")f.Native.Rows.RemoveAll(row=>row is PURCHASE_ORDER);
        if(mode is "historical-item" or "historical-both")f.Native.Rows.RemoveAll(row=>row is INVENTORY_ITEM);
        string? nextIntent=null;if(mode=="route-change"){await f.Prepare(quantity:4);await f.Record();nextIntent=f.Submission.Command.IntentId;}
        var baseline=f.Native.Writes;using var company=ReceiptStockWorkspaceTests.Company();
        string[] grants=mode=="grant"?["Inventory.View"]:["Procurement.View","Inventory.View"];
        Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],grants)));
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Procurement.View","Inventory.View"],configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var delayed=new TaskCompletionSource<PurchaseReceiptStockRecovery>();var delayedClient=new Mock<IAccountingServiceClient>();var recoveryReads=0;
        delayedClient.Setup(value=>value.GetPurchaseReceiptStockRecoveryAsync(It.IsAny<string>())).Returns((string id)=>++recoveryReads==1?delayed.Task:client.GetPurchaseReceiptStockRecoveryAsync(id));
        var authentication=new Authentication(mode=="account"?"other":"actor");var intent=mode=="bad-route"?"invalid":original.Command.IntentId;
        var navigation=new Navigation(f.OrderId,f.ReceiptId,intent);navigation.Start();var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<InventoryWorkspaceState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(mode=="route-change"?delayedClient.Object:client);services.AddSingleton<NavigationManager>(navigation);services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        services.AddScoped<PurchaseOrderWorkspaceState>(scope=>new(scope.GetRequiredService<CompanyClientContext>(),scope.GetRequiredService<InventoryWorkspaceState>()));
        await using var provider=services.BuildServiceProvider();provider.GetRequiredService<PurchaseOrderWorkspaceState>();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var state=provider.GetRequiredService<InventoryWorkspaceState>();Assert.Null(state.StockIntent);
            var rendering=renderer.RenderComponentAsync<ReceiptStockWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"IntentId",intent}}));
            if(mode=="route-change")
            {
                await Task.Yield();var changing=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;Assert.Equal(1,recoveryReads);
                changing.IntentId=nextIntent;await (Task)typeof(ReceiptStockWorkbench).GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(changing,null)!;
                delayed.SetResult(await client.GetPurchaseReceiptStockRecoveryAsync(intent));var changed=await rendering;
                Assert.Equal(nextIntent,state.StockRecovery!.Entry.INVENTORY_STOCK_INTENT_ID);Assert.Equal(18m,state.Detail!.Quantity);Assert.NotNull(state.StockResult);
                Assert.Equal(3,recoveryReads);Assert.Contains(nextIntent!,changed.ToHtmlString());Assert.Equal(baseline,f.Native.Writes);return;
            }
            var root=await rendering;
            var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            if(mode is "missing" or "grant" or "account" or "bad-route"){Assert.Null(state.StockRecovery);Assert.DoesNotContain("Saved physical stock intent verified",root.ToHtmlString());}
            else
            {
                Assert.Contains("Saved physical stock intent verified",root.ToHtmlString());Assert.Equal(original.Command,state.StockRecovery!.OriginalSubmission().Command);
                Assert.False(state.HasUnconfirmedStockIntent);if(mode is "historical-item" or "historical-both")Assert.Null(state.Detail);else Assert.Equal(mode=="later-receipt"?18m:14m,state.Detail!.Quantity);
                Assert.Equal(mode is "exact" or "unknown" or "auth-change",state.StockResult is not null);
                Assert.Contains(mode is "item-changed" or "purchase-changed" or "later-receipt" or "historical-item" or "historical-purchase" or "historical-both"?"Current source has changed":"Current source matches the original recorded stock effect",root.ToHtmlString());
                if(mode is "historical-item" or "historical-both")Assert.Contains("Current owned inventory item is unavailable",root.ToHtmlString());if(mode is "historical-purchase" or "historical-both")Assert.Contains("Current owned purchase order is unavailable",root.ToHtmlString());
                Assert.DoesNotContain("Record reviewed physical movement",root.ToHtmlString());
                await (Task)typeof(ReceiptStockWorkbench).GetMethod("Save",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                if(mode=="auth-change"){authentication.Change();await Task.Yield();Assert.Null(state.StockRecovery);Assert.Null(state.Detail);Assert.False(state.HasUnconfirmedStockIntent);}
            }
            Assert.Equal(baseline,f.Native.Writes);
        });
    }
    [Theory][InlineData("receive")][InlineData("return")][InlineData("relinked-return")][InlineData("escaped")][InlineData("unknown")][InlineData("auth-change")][InlineData("auth-during")]
    [InlineData("read-only")][InlineData("grant")][InlineData("account")][InlineData("choice")][InlineData("bad-reason")][InlineData("procurement-busy")]
    public async Task ReceiptPageAndActualTypedClientApiNativeJourneyConfirmOrRetainOriginalIntent(string mode)
    {
        var f=new PurchaseReceiptStockCommandTests.Fixture();if(mode=="escaped"){f.OrderId="order/a?b";f.ReceiptId="receipt/α%2F";}await f.Initialize();string? originalReceipt=null;
        if(mode is "return" or "relinked-return"){await f.Record();originalReceipt=f.Submission.Command.IntentId;if(mode=="relinked-return")await f.ApproveAlternateDestination();}
        var baseline=f.Native.Writes;using var company=ReceiptStockWorkspaceTests.Company();
        if(mode is "read-only" or "grant")Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Procurement.View","Inventory.View"])));
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Procurement.View","Inventory.View","Inventory.RecordReceiptStock","Inventory.RecordSupplierReturn"],configureAccounting:(services,context)=>{services.AddSingleton(f.Inventory);services.AddSingleton(f.Receiving.Existing.PO);services.AddSingleton(f.Receiving.Existing.AP);});
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var authentication=new Authentication(mode=="account"?"other":"actor");var navigation=new Navigation(f.OrderId,f.ReceiptId);navigation.Start();var services=new ServiceCollection();
        services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<InventoryWorkspaceState>();services.AddSingleton<AuthenticationStateProvider>(authentication);services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<NavigationManager>(navigation);services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        services.AddScoped<PurchaseOrderWorkspaceState>(scope=>new(scope.GetRequiredService<CompanyClientContext>(),scope.GetRequiredService<InventoryWorkspaceState>()));
        await using var provider=services.BuildServiceProvider();var procurement=provider.GetRequiredService<PurchaseOrderWorkspaceState>();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<ReceiptStockWorkbench>();var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;var state=provider.GetRequiredService<InventoryWorkspaceState>();
            Task Invoke(string name)=>(Task)typeof(ReceiptStockWorkbench).GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
            void Render()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
            void Field(string name,object value)=>typeof(ReceiptStockWorkbench).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,value);
            if(mode=="account"){Assert.Null(state.StockReview);Assert.Equal(baseline,f.Native.Writes);return;}
            Assert.Contains("Verified receipt stock capacity",root.ToHtmlString());Assert.Equal(f.OrderId,page.OrderId);Assert.Equal(f.ReceiptId,page.ReceiptId);
            if(mode=="relinked-return")
            {
                Assert.Contains("This receipt retains its recorded stock destination item",root.ToHtmlString());Assert.Contains("purchase order line now links to alternate",root.ToHtmlString());
                Assert.True(state.StockReview!.Source.UsesRecordedStockDestination);Assert.Equal(4m,state.StockReview.ReceivedQuantity);Assert.Equal(4m,state.StockReview.RemainingReceiptQuantity);
            }
            if(mode=="read-only"){Assert.Contains("Read-only receipt stock review",root.ToHtmlString());Assert.DoesNotContain("Record reviewed physical movement",root.ToHtmlString());Assert.Equal(baseline,f.Native.Writes);return;}
            if(mode!="choice")Field("_kind",mode is "return" or "relinked-return"?PurchaseReceiptStockMovementKind.Return:PurchaseReceiptStockMovementKind.Receive);
            Field("_quantity",mode is "return" or "relinked-return"?1m:4m);if(originalReceipt is not null)Field("_originalReceipt",originalReceipt);
            Field("_reason",mode=="bad-reason"?"two\nlines":"Physical screen reason.");
            Task? procurementRead=null;TaskCompletionSource<PurchaseReceiptInventoryReview>? procurementReply=null;
            if(mode=="procurement-busy")
            {
                await procurement.LoadDetailAsync(f.OrderId,_=>Task.FromResult<PurchaseOrderSourceReview?>(state.StockReview!.Source.Purchase));
                procurementReply=new();procurementRead=procurement.LoadInventoryLinkAsync(f.ReceiptId,(_,_)=>procurementReply.Task);
                Render();Assert.Contains("Procurement work is busy or unconfirmed",root.ToHtmlString());
                await Invoke("Save");Assert.Null(state.StockIntent);Assert.Equal(baseline,f.Native.Writes);
                procurementReply.SetResult(state.StockReview!.Source);await procurementRead;Assert.False(state.ProcurementWorkBlocked);
            }
            if(mode is "unknown" or "auth-change")f.Native.FailCommit=true;if(mode=="auth-during")f.OnInsert=authentication.Change;
            await Invoke("Save");Render();
            if(mode is "grant" or "choice" or "bad-reason"){Assert.False(state.HasUnconfirmedStockIntent);Assert.Equal(baseline,f.Native.Writes);return;}
            if(mode is "unknown" or "auth-change" or "auth-during")
            {
                Assert.True(state.HasUnconfirmedStockIntent);Assert.Null(state.StockResult);Assert.Contains("Reconcile original stock intent",root.ToHtmlString());
                if(mode is "auth-change" or "auth-during"){if(mode=="auth-change")authentication.Change();await Task.Yield();Assert.Null(state.Detail);Assert.Null(state.StockReview);Assert.True(state.HasUnconfirmedStockIntent);return;}
                var original=state.StockIntent!.Command;await Invoke("Reload");Render();Assert.True(state.HasUnconfirmedStockIntent);
                var pendingLanding=await renderer.RenderComponentAsync<WarehouseWorkbench>();
                Assert.Contains("Return to original stock intent",pendingLanding.ToHtmlString());Assert.True(state.HasUnconfirmedStockIntent);Assert.Null(state.StockResult);
                Field("_kind",PurchaseReceiptStockMovementKind.Return);Field("_quantity",999m);Field("_reason","Replacement ignored");f.Native.FailCommit=false;
                await Invoke("Save");Render();Assert.Equal(original,state.StockIntent!.Command);
            }
            Assert.NotNull(state.StockResult);Assert.False(state.HasUnconfirmedStockIntent);Assert.Equal(baseline+3,f.Native.Writes);
            Assert.Contains("Physical stock movement confirmed by complete current read-back",root.ToHtmlString());Assert.Equal(mode is "return" or "relinked-return"?13m:14m,state.Detail!.Quantity);
            Assert.Equal(mode is "return" or "relinked-return"?1m:0m,state.StockReview!.ReturnedQuantity);Assert.Equal(4m,state.StockReview.RemainingReceiptQuantity);
            var landing=await renderer.RenderComponentAsync<WarehouseWorkbench>();
            Assert.Equal(mode is "return" or "relinked-return"?13m:14m,Assert.Single(state.Rows!).Quantity);Assert.Equal(1,state.Count(InventoryWorkspaceState.EvidenceQueue.All));
            Assert.Contains("/warehouse/items/item",landing.ToHtmlString());Assert.Null(state.StockReview);
        });
    }
}
#endif
