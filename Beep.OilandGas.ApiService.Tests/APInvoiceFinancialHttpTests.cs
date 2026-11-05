#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Moq;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class APInvoiceFinancialHttpTests
{
    [Theory][InlineData("valid")][InlineData("revision")][InlineData("source")][InlineData("provider")]
    [InlineData("role")][InlineData("procurement")][InlineData("binding")][InlineData("role-set")]
    public async Task WholeInvoiceTypedClientRequiresCurrentCompleteOwnedEvidence(string fault)
    {
        await using var host=await AccountingSourceHttpTests.Start();host.Context.InvoiceId="invoice/α%2F";
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var invoice=(await client.GetAPInvoiceAsync(host.Context.InvoiceId))!;var revision=invoice.Revision;
        host.Context.Calls=0;
        if(fault=="revision")revision=new string('C',64);if(fault=="source")host.Context.SourceEdited=true;if(fault=="provider")host.Context.FailProvider=true;
        if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");if(fault=="procurement")host.Context.Permissions=["Accounting.View"];
        if(fault=="binding")host.Context.ChangeBinding=true;
        if(fault=="role-set")host.Context.OnCapture=count=>{if(count>=4)host.Context.Roles=["ConfiguredCompanyRole","OtherRole"];};
        if(fault=="valid")
        {
            var review=await client.GetAPInvoiceFinancialReviewAsync(host.Context.InvoiceId,revision);review.Require(new("tenant","company"),"actor",host.Context.InvoiceId,revision);
            Assert.Single(review.PurchaseLines);Assert.Equal("order",review.PurchaseLines[0].Source.Order.Header.PURCHASE_ORDER_ID);
            Assert.All(review.Checks.Where(row=>row.Code is "acceptance" or "policy" or "ledger" or "decision"),row=>Assert.Equal(APSourceComparisonOutcome.Unavailable,row.Outcome));
            Assert.True(host.Context.OrderReads>0);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPInvoiceFinancialReviewAsync(host.Context.InvoiceId,revision));
            Assert.Equal(fault is "role" or "procurement" or "binding" or "role-set"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,error.StatusCode);
        }
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
    internal sealed class Authentication : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"))));
    }
    internal sealed class Navigation : NavigationManager
    {
        public Navigation()=>Initialize("https://render.invalid/","https://render.invalid/accounting/payables/invoices/invoice");
        protected override void NavigateToCore(string uri,bool forceLoad)=>throw new InvalidOperationException("Static render has no navigation.");
    }
    internal sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>throw new InvalidOperationException("Static render has no browser.");
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>InvokeAsync<T>(id,args);
    }
    internal sealed class Components(IServiceProvider services) : IComponentActivator
    {
        public IComponent? Page;
        public IComponent CreateInstance(Type type){var component=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(type==typeof(Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench))Page=component;return component;}
    }
    [Theory][InlineData("valid")][InlineData("changed")][InlineData("permission")]
    public async Task CompleteInvoicePageHandlerUsesActualFinancialClientAndClearsUnconfirmedEvidence(string mode)
    {
        await using var host=await AccountingSourceHttpTests.Start();using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"]));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(company);services.AddSingleton<NavigationManager,Navigation>();
        services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client);services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();
        services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
            if(mode=="changed")host.Context.SourceEdited=true;if(mode=="permission")host.Context.Permissions=["Accounting.View"];
            var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            await (Task)page.GetType().GetMethod("ReviewFinancial",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);
            return WebUtility.HtmlDecode(root.ToHtmlString());});
        if(mode=="valid"){Assert.Contains("Whole invoice financial evidence",html);Assert.Contains("Verified effective company matching policy",html);Assert.Contains("Accounting book, period and ledger outcome",html);Assert.Contains("/procurement/orders/order",html);}
        else{Assert.DoesNotContain("Whole invoice financial evidence",html);Assert.DoesNotContain("Open owned PO for invoice line",html);}
        Assert.DoesNotContain(host.Context.Source!.Invocations,call=>new[]{"Insert","Update","Delete","BeginTransaction","Commit"}.Any(prefix=>call.Method.Name.StartsWith(prefix,StringComparison.Ordinal)));
    }
}
#endif
