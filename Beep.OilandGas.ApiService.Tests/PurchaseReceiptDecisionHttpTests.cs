#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
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
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptDecisionHttpTests
{
    private static readonly string[] Permissions=["Procurement.View","Procurement.RecordReceiptDecision"];
    private static Task<AccountingSourceHttpTests.Host> Host(PurchaseReceiptDecisionTests.Fixture f)=>AccountingSourceHttpTests.Start(permissions:Permissions,
        configureAccounting:(services,context)=>{services.AddSingleton(f.Existing.AP);services.AddSingleton(f.Existing.PO);context.Source=f.Existing.Native.Source;});
    private static void Select(CompanyClientContext company,string owner="company",string actor="actor")=>company.TrySelect(company.Clear(),actor,new(new("tenant",owner),["ConfiguredCompanyRole"],Permissions));
    private static AccountingServiceClient Client(HttpClient http,CompanyClientContext company)=>new(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    private static PurchaseReceiptDecisionSubmission Submission(PurchaseReceiptDecisionReview review)=>new(new(review.Source.Company,review.Source.ActorUserId,review.Source.Header.PURCHASE_ORDER_ID,review.ReceiptId,8,2,"bbl",review.Source.Revision,review.BindingRevision,Guid.NewGuid().ToString("N"),"Inspected and rejected damage."),review);
    [Fact] public async Task ActualBoundReadAndTypedWriteSupportExactRetryWithEscapedIdentifiers()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();f.Existing.Order.PURCHASE_ORDER_ID="order/α%2F";f.Existing.OrderLine.PURCHASE_ORDER_ID=f.Existing.Order.PURCHASE_ORDER_ID;
        f.Existing.Receipt.PURCHASE_ORDER_ID=f.Existing.Order.PURCHASE_ORDER_ID;f.Existing.Receipt.PO_RECEIPT_ID="receipt/α%2F";
        await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var review=await client.GetPurchaseReceiptDecisionReviewAsync(f.Existing.Order.PURCHASE_ORDER_ID,f.Existing.Receipt.PO_RECEIPT_ID);Assert.Equal("binding",review.BindingRevision);
        Assert.Empty(f.Existing.Native.Transactions);var submission=Submission(review);var result=await client.RecordPurchaseReceiptDecisionAsync(submission);result.Require(submission);
        await client.RecordPurchaseReceiptDecisionAsync(submission);Assert.Equal(1,f.Existing.Native.Writes);Assert.Equal(8m,result.Receipt.ACCEPTED_QUANTITY);
    }
    [Theory][InlineData("role")][InlineData("read")][InlineData("write")][InlineData("binding")][InlineData("roles-changed")][InlineData("permissions-changed")]
    public async Task WriteRequiresIndependentCurrentCompanyAuthority(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var submission=Submission(await client.GetPurchaseReceiptDecisionReviewAsync("order","receipt"));host.Context.Calls=0;
        if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");if(fault=="read")host.Context.Permissions=["Procurement.RecordReceiptDecision"];
        if(fault=="write")host.Context.Permissions=["Procurement.View"];if(fault=="binding")host.Context.ChangeBinding=true;
        if(fault=="roles-changed")host.Context.OnCapture=n=>{if(n>=4)host.Context.Roles=["ConfiguredCompanyRole","Another"];} ;
        if(fault=="permissions-changed")host.Context.OnCapture=n=>{if(n>=4)host.Context.Permissions=[..Permissions,"Another"];} ;
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordPurchaseReceiptDecisionAsync(submission));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Fact] public async Task PreviewNeedsReadAuthorityAndDoesNotGrantWrite()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);host.Context.Permissions=["Procurement.View"];
        using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);var submission=Submission(await client.GetPurchaseReceiptDecisionReviewAsync("order","receipt"));
        var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.RecordPurchaseReceiptDecisionAsync(submission));Assert.Equal(HttpStatusCode.Forbidden,error.StatusCode);Assert.Empty(f.Existing.Native.Transactions);
    }
    [Theory][InlineData("source")][InlineData("review-binding")][InlineData("reason")][InlineData("route")]
    public async Task ChangedOrInvalidBodyIsRefusedWithoutMutation(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var submission=Submission(await client.GetPurchaseReceiptDecisionReviewAsync("order","receipt"));
        if(fault=="source")f.Existing.Receipt.RECEIVED_QUANTITY=11;if(fault=="review-binding")submission=submission with{Review=submission.Review with{BindingRevision="different"}};
        if(fault=="reason")submission=submission with{Command=submission.Command with{Reason="bad\nreason"}};
        var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/purchase-order/order/receipts/"+(fault=="route"?"other":"receipt")+"/decision",submission);
        Assert.Equal(fault=="route"?HttpStatusCode.BadRequest:HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Theory][InlineData("account")][InlineData("company")]
    public async Task TypedClientCannotSubmitFromAnotherSelection(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        var submission=Submission(await client.GetPurchaseReceiptDecisionReviewAsync("order","receipt"));Select(company,fault=="company"?"other":"company",fault=="account"?"other":"actor");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.RecordPurchaseReceiptDecisionAsync(submission));Assert.Empty(f.Existing.Native.Transactions);
    }
    [Fact] public async Task WorkspaceUnknownCommitRetainsOriginalReasonAndReconcilesFreshOwnedEvidence()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        using var state=new PurchaseOrderWorkspaceState(company);await state.LoadDetailAsync("order",client.GetPurchaseOrderAsync);await state.LoadReceiverReviewAsync("receipt",client.GetPurchaseReceiptDecisionReviewAsync);
        f.Existing.Native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>state.RecordReceiverAsync(8,2,"Original reason",client.RecordPurchaseReceiptDecisionAsync,client.GetPurchaseOrderAsync));
        Assert.True(state.HasUnconfirmedReceiverIntent);var intent=state.ReceiverIntent!;state.ReceiverIntent!.Review.Source.Receipts[0].RECEIVED_QUANTITY=100;
        state.Clear();await state.LoadDetailAsync("order",client.GetPurchaseOrderAsync);f.Existing.Native.FailCommit=false;
        await state.RecordReceiverAsync(0,10,"Replacement reason",client.RecordPurchaseReceiptDecisionAsync,client.GetPurchaseOrderAsync);
        Assert.Equal("Original reason",state.ReceiverResult!.Command.Reason);Assert.Equal(8m,state.Receipts![0].Decision!.Accepted);Assert.Equal(1,f.Existing.Native.Writes);
        Assert.False(state.HasUnconfirmedReceiverIntent);Assert.NotEqual(intent.Command.ExpectedOrderRevision,state.SourceRevision);
    }
    [Theory][InlineData("company")][InlineData("source")]
    public async Task WorkspaceRejectsLateSuccessAndRetainsOriginalIntent(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        using var state=new PurchaseOrderWorkspaceState(company);await state.LoadDetailAsync("order",client.GetPurchaseOrderAsync);await state.LoadReceiverReviewAsync("receipt",client.GetPurchaseReceiptDecisionReviewAsync);
        var started=new TaskCompletionSource<PurchaseReceiptDecisionSubmission>();var pending=new TaskCompletionSource<PurchaseReceiptDecisionResult>();
        var task=state.RecordReceiverAsync(8,2,"Original",submission=>{started.SetResult(submission);return pending.Task;},client.GetPurchaseOrderAsync);var original=await started.Task;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordReceiverAsync(8,2,"Duplicate",client.RecordPurchaseReceiptDecisionAsync,client.GetPurchaseOrderAsync));
        if(fault=="company")Select(company,"other");else state.Clear();
        pending.SetResult(await clientAfterReturn(original));await task;Assert.Null(state.ReceiverResult);Assert.True(state.HasUnconfirmedReceiverIntent);
        async Task<PurchaseReceiptDecisionResult> clientAfterReturn(PurchaseReceiptDecisionSubmission submission)
        {var row=await f.Existing.PO.RecordReceiptDecisionAsync(submission.Command,submission.Review.Source,()=>Task.CompletedTask);return PurchaseReceiptDecisionResult.Create(submission,row);}
    }
    [Theory][InlineData("wrong-row")][InlineData("changed-order")]
    public async Task OwnedRefreshFailureCannotPublishReceivingSuccess(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);var client=Client(host.Client,company);
        using var state=new PurchaseOrderWorkspaceState(company);await state.LoadDetailAsync("order",client.GetPurchaseOrderAsync);await state.LoadReceiverReviewAsync("receipt",client.GetPurchaseReceiptDecisionReviewAsync);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecordReceiverAsync(8,2,"Original",client.RecordPurchaseReceiptDecisionAsync,async id=>{
            if(fault=="changed-order")f.Existing.OrderLine.ITEM_DESCRIPTION="Changed";
            else f.Stored.ACCEPTED_QUANTITY=9;
            return await client.GetPurchaseOrderAsync(id);
        }));
        Assert.Null(state.ReceiverResult);Assert.True(state.HasUnconfirmedReceiverIntent);Assert.Equal(1,f.Existing.Native.Writes);
    }
    [Fact] public async Task RecreatedProcurementPageKeepsOriginalCircuitIntentAndReconciles()
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
        services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();
        await using(var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>()))
        {
            await renderer.Dispatcher.InvokeAsync(async()=>{
                await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));
                var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;var type=page.GetType();
                type.GetField("_receipt",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"receipt");await Invoke("ReviewReceiver");
                type.GetField("_accepted",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,8m);type.GetField("_rejected",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,2m);
                type.GetField("_reason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Original circuit reason");f.Existing.Native.FailCommit=true;await Invoke("RecordReceiver");
                Assert.True(provider.GetRequiredService<PurchaseOrderWorkspaceState>().HasUnconfirmedReceiverIntent);
                Task Invoke(string method)=>(Task)type.GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            });
        }
        f.Existing.Native.FailCommit=false;
        await using var recreated=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await recreated.Dispatcher.InvokeAsync(async()=>{
            var root=await recreated.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));
            Assert.Contains("Reconcile original receiver intent",root.ToHtmlString());
            var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            await (Task)page.GetType().GetMethod("RecordReceiver",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);return root.ToHtmlString();
        });
        Assert.Contains("Receiver decision confirmed",html);Assert.Equal(1,f.Existing.Native.Writes);Assert.Equal("Original circuit reason",provider.GetRequiredService<PurchaseOrderWorkspaceState>().ReceiverResult!.Command.Reason);
    }
    private sealed class Navigation:NavigationManager
    {
        public Navigation()=>Initialize("https://render.invalid/","https://render.invalid/procurement/orders/order");
        protected override void NavigateToCore(string uri,bool forceLoad){Uri=ToAbsoluteUri(uri).ToString();}
    }
    private sealed class Components(IServiceProvider services):IComponentActivator
    {
        public IComponent? Page;
        public IComponent CreateInstance(Type type){var instance=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.ProcurementWorkbench))Page=instance;return instance;}
    }
    [Theory][InlineData("record")][InlineData("permission")][InlineData("reconcile")][InlineData("reason")]
    public async Task CompleteProcurementPageExecutesReviewedReceiverWorkflow(string mode)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await using var host=await Host(f);using var company=new CompanyClientContext();Select(company);
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton(company);services.AddScoped<PurchaseOrderWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
        services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.ProcurementWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","order"}}));
            var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;var type=page.GetType();
            type.GetField("_receipt",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"receipt");await Invoke("ReviewReceiver");
            type.GetField("_accepted",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,8m);type.GetField("_rejected",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,2m);
            type.GetField("_reason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,mode=="reason"?"":"Reviewed by receiver");
            if(mode=="permission")host.Context.Permissions=["Procurement.View"];if(mode=="reconcile")f.Existing.Native.FailCommit=true;await Invoke("RecordReceiver");
            if(mode=="reconcile"){f.Existing.Native.FailCommit=false;type.GetField("_reason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Replacement");await Invoke("RecordReceiver");}
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);return WebUtility.HtmlDecode(root.ToHtmlString());
            Task Invoke(string method)=>(Task)type.GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
        });
        if(mode is "record" or "reconcile"){Assert.Contains("Receiver decision confirmed",html);Assert.Contains("Verified accepted: 8",html);Assert.Equal(1,f.Existing.Native.Writes);Assert.Equal("Reviewed by receiver",PurchaseReceiptDecisionProof.Require(new("tenant","company"),f.Stored).Command.Reason);}
        else{Assert.DoesNotContain("Receiver decision confirmed",html);Assert.Equal(0,f.Existing.Native.Writes);}
    }
}
#endif
