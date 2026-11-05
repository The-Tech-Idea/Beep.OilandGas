#if WORKFLOW_INBOX_FOCUSED
using System.Net.Http.Json;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyAccountingHandoffTests
{
    [Theory][InlineData("late-company","RUN_TICKET")][InlineData("late-source","RUN_TICKET")][InlineData("changing-set","RUN_TICKET")][InlineData("foreign-envelope","RUN_TICKET")][InlineData("failed-discovery","RUN_TICKET")][InlineData("late-company","JOURNAL_ENTRY")][InlineData("late-source","JOURNAL_ENTRY")][InlineData("changing-set","JOURNAL_ENTRY")][InlineData("foreign-envelope","JOURNAL_ENTRY")][InlineData("failed-discovery","JOURNAL_ENTRY")]
    public async Task NativeDiscoveryRefusesChangedAuthoritySourceCandidatesAndUnavailableEvidence(string mode,string kind)
    {
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);var client=new Mock<IBusinessProcessServiceClient>(MockBehavior.Strict);
        var query=new CompanyAccountingProcessStartCatalogQuery(kind,"ticket",new string('A',64));state.AttachSource(query.EntityType,query.EntityId,query.ExpectedSourceRevision);
        var review=Review() with{EntityType=kind,EntityId="ticket"};
        var catalog=new CompanyAccountingProcessStartCatalog(review.Company,review.ActorUserId,review.EntityType,review.EntityId,review.SourceRevision,review.BindingRevision,[review]);
        var discovery=new CompanyNativeDecisionHandoffReview(review.Company,review.ActorUserId,review.EntityType,review.EntityId,review.SourceRevision,review.BindingRevision,new string('D',64),null,null,null);
        var response=new TaskCompletionSource<CompanyNativeDecisionHandoffReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.DiscoverNativeDecisionHandoffAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).Returns(response.Task);
        if(mode=="changing-set")
        {
            client.SetupSequence(value=>value.DiscoverNativeDecisionHandoffAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(discovery).ReturnsAsync(discovery with{CandidateSetRevision=new string('E',64)});
            client.Setup(value=>value.GetCompanyAccountingStartCatalogAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(catalog);
        }
        var loading=state.LoadCatalogAsync(client.Object);
        if(mode=="late-company")company.Clear();
        if(mode=="late-source")state.AttachSource(kind,"another-ticket",query.ExpectedSourceRevision);
        if(mode=="failed-discovery")response.SetException(new HttpRequestException("Discovery is unavailable."));
        else response.SetResult(mode=="foreign-envelope"?discovery with{Company=new("foreign-tenant","company")}:discovery);
        if(mode.StartsWith("late",StringComparison.Ordinal))await loading;else await Assert.ThrowsAnyAsync<Exception>(()=>loading);
        Assert.Null(state.Catalog);Assert.Null(state.Review);Assert.Null(state.Discovery);Assert.Null(state.Intent);Assert.False(state.Busy);
        client.Verify(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>()),Times.Never);
        client.Verify(value=>value.GetCompanyAccountingStartCatalogAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>()),mode=="changing-set"?Times.Once():Times.Never());
    }
    private static CompanyClientContext Company()
    {
        var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Workflow.Start"]));return company;
    }
    private static CompanyAccountingProcessStartReview Review()=>new(new("tenant","company"),"actor","definition","AP_INVOICE","invoice",new string('A',64),new string('B',64),new string('C',64),"maker","Recorded workflow","1.1.0","ConfiguredCompanyRole");
    private static CompanyAccountingProcessStartCatalog Catalog()=>new(new("tenant","company"),"actor","AP_INVOICE","invoice",new string('A',64),new string('C',64),[Review()]);
    private static Mock<IBusinessProcessServiceClient> Client()
    {
        var client=new Mock<IBusinessProcessServiceClient>(MockBehavior.Strict);
        client.Setup(value=>value.GetCompanyAccountingStartCatalogAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync(Catalog());
        client.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).ReturnsAsync((CompanyAccountingProcessStart intent,CancellationToken token)=>
            new CompanyAccountingProcessStartReceipt(intent.Company,intent.ActorUserId,intent.IntentId,intent.InstanceId(),intent.EntityType,intent.EntityId,"step",["task"],new string('D',64)));
        return client;
    }
    [Fact] public async Task ExplicitChoiceCreatesOneIntentAndConfirmedSubmissionCannotBeRepeatedByState()
    {
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);var client=Client();state.AttachSource("AP_INVOICE","invoice",new string('A',64));
        await state.LoadCatalogAsync(client.Object);Assert.Null(state.Review);Assert.Null(state.Intent);
        state.Select("definition");await state.SubmitAsync(client.Object);Assert.NotNull(state.Receipt);Assert.False(state.HasUnconfirmedIntent);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));
        client.Verify(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Fact] public async Task UnconfirmedOutcomeRetainsIntentAcrossSourceAndCompanyChangesAndRequiresFreshMatchingReview()
    {
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);var client=Client();state.AttachSource("AP_INVOICE","invoice",new string('A',64));
        await state.LoadCatalogAsync(client.Object);state.Select("definition");
        client.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("unconfirmed"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));var intent=state.Intent;Assert.NotNull(intent);Assert.True(state.HasUnconfirmedIntent);Assert.Null(state.Review);
        state.AttachSource("AP_INVOICE","other",new string('A',64));Assert.Same(intent,state.Intent);Assert.Null(state.Catalog);
        company.Clear();Assert.Same(intent,state.Intent);
        company.TrySelect(company.Version,"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Workflow.Start"]));
        state.AttachSource("AP_INVOICE","invoice",new string('A',64));await state.LoadCatalogAsync(client.Object);state.Select("definition");
        client.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyAccountingProcessStartReceipt(intent!.Company,intent.ActorUserId,intent.IntentId,intent.InstanceId(),intent.EntityType,intent.EntityId,"step",["task"],new string('D',64)));
        await state.SubmitAsync(client.Object);Assert.Same(intent,state.Intent);Assert.NotNull(state.Receipt);
    }
    [Fact] public async Task SourceChangeDuringSubmissionRetainsOriginalIntentAndDiscardsLateReceipt()
    {
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);var client=Client();state.AttachSource("AP_INVOICE","invoice",new string('A',64));
        await state.LoadCatalogAsync(client.Object);state.Select("definition");
        var response=new TaskCompletionSource<CompanyAccountingProcessStartReceipt>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).Returns(response.Task);
        var pending=state.SubmitAsync(client.Object);var intent=state.Intent!;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object));
        state.AttachSource("AP_INVOICE","other",new string('A',64));response.SetResult(new(intent.Company,intent.ActorUserId,intent.IntentId,intent.InstanceId(),intent.EntityType,intent.EntityId,"step",["task"],new string('D',64)));
        await pending;Assert.Null(state.Receipt);Assert.True(state.HasUnconfirmedIntent);Assert.Same(intent,state.Intent);
    }
    [Fact] public async Task ChangedCompanyCatalogCannotBecomeVisible()
    {
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);var client=Client();state.AttachSource("AP_INVOICE","invoice",new string('A',64));
        var response=new TaskCompletionSource<CompanyAccountingProcessStartCatalog>(TaskCreationOptions.RunContinuationsAsynchronously);
        client.Setup(value=>value.GetCompanyAccountingStartCatalogAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).Returns(response.Task);
        var pending=state.LoadCatalogAsync(client.Object);company.Clear();response.SetResult(Catalog());await pending;Assert.Null(state.Catalog);Assert.Null(state.Review);
    }
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredCompanyRole"),new(ClaimTypes.NameIdentifier,"actor"),new("party_id","actor")],"test"))));
    }
    private sealed class Navigation(string path) : NavigationManager
    {
        public void InitializeForTest()=>Initialize("https://render.invalid/","https://render.invalid"+path);
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static render does not navigate.");
    }
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static render has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    [Theory]
    [InlineData("AP_INVOICE","invoice","/accounting/payables/invoices/invoice","confirmed")]
    [InlineData("PURCHASE_ORDER","order","/procurement/orders/order","confirmed")]
    [InlineData("AP_INVOICE","invoice","/accounting/payables/invoices/invoice","unconfirmed")]
    [InlineData("PURCHASE_ORDER","order","/procurement/orders/order","unconfirmed")]
    [InlineData("AP_INVOICE","invoice","/accounting/payables/invoices/invoice","empty")]
    [InlineData("PURCHASE_ORDER","order","/procurement/orders/order","empty")]
    public async Task CompleteWorkbenchesRenderConfirmedUnconfirmedAndEmptyHandoffEvidence(string type,string id,string path,string outcome)
    {
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");native.Definition.ENTITY_TYPE=type;
        await using var host=await CompanyProcessStartHttpTests.Host(native);
        using var company=Company();using var state=new CompanyAccountingHandoffState(company);
        var client=new BusinessProcessServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var review=await client.ReviewCompanyAccountingStartAsync(new("definition",type,id));state.AttachSource(type,id,review.SourceRevision);
        if(outcome=="empty")native.Definition.ACTIVE_IND="N";
        await state.LoadCatalogAsync(client);
        if(outcome!="empty")
        {
            state.Select("definition");
            if(outcome=="confirmed"){await state.SubmitAsync(client);Assert.NotNull(state.Receipt);}
            else
            {
                // Explicit transport-failure boundary; real HTTP catalog/source evidence remains in use.
                var transport=new Mock<IBusinessProcessServiceClient>(MockBehavior.Strict);
                transport.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("unconfirmed transport"));
                await Assert.ThrowsAsync<HttpRequestException>(()=>state.SubmitAsync(transport.Object));
            }
        }
        var accounting=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
        accounting.Setup(value=>value.GetAPInvoiceAsync(id)).Returns(()=>host.Client.GetFromJsonAsync<APInvoiceSourceReview>("/api/accounting/traditional/ap/"+Uri.EscapeDataString(id)));
        accounting.Setup(value=>value.GetPurchaseOrderAsync(id)).Returns(()=>host.Client.GetFromJsonAsync<PurchaseOrderSourceReview>("/api/accounting/traditional/purchase-order/"+Uri.EscapeDataString(id)));
        var services=new ServiceCollection();services.AddLogging();services.AddAuthorizationCore();services.AddCascadingAuthenticationState();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();
        services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton<IBusinessProcessServiceClient>(client);services.AddSingleton(accounting.Object);services.AddScoped<APInvoiceWorkspaceState>();
        services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<AuthenticationStateProvider,Auth>();var navigation=new Navigation(path);navigation.InitializeForTest();services.AddSingleton<NavigationManager>(navigation);
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var parameters=ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",id}});
            var rendered=type=="AP_INVOICE"?await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(parameters):await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.ProcurementWorkbench>(parameters);
            return System.Net.WebUtility.HtmlDecode(rendered.ToHtmlString());});
        if(outcome=="confirmed")
        {
            Assert.Contains("Recorded handoff confirmed",html);Assert.Contains("Open recorded process",html);Assert.Contains("Initial assigned role",html);
            Assert.Contains(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",state.Receipt!.InstanceId,type),html);Assert.Equal(6,native.Native.Writes);
        }
        else
        {
            Assert.DoesNotContain("Recorded handoff confirmed",html);Assert.DoesNotContain("Open recorded process",html);Assert.Equal(0,native.Native.Writes);
            Assert.Contains(outcome=="empty"?"No active recorded workflow matches this source":"previous handoff outcome is unconfirmed",html);
        }
    }
}
#endif
