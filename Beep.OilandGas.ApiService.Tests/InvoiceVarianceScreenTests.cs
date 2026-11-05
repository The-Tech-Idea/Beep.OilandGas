using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
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
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceScreenTests
{
    [Theory][InlineData("valid")][InlineData("unknown")][InlineData("retry")][InlineData("recreate")][InlineData("grant")][InlineData("changed")]
    public async Task CompleteInvoicePageRecordsOrRetainsAndReconcilesOneOriginalProposal(string scenario)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.PolicyBinding=new string('B',64);host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance","Accounting.RecordInvoiceVariance"];
        using var company=InvoiceVarianceWorkspaceTests.Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<APInvoiceWorkspaceState>();string? originalIntent=null;
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            async Task<Microsoft.AspNetCore.Components.Web.HtmlRendering.HtmlRootComponent> Render()=>await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
            var root=await Render();var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method,params object[] args){var result=page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,args);if(result is Task task)await task;}
            await Invoke("ReviewFinancial");await Invoke("VarianceDispositionChanged","REQUEST_VARIANCE_REVIEW");await Invoke("VarianceReasonChanged","Explicit accountant variance request.");
            foreach(var row in state.RecordedVariances)await Invoke("VarianceExplanationChanged",row.Code,"Explicit explanation for "+row.Code);await Invoke("ReviewVariance");Assert.NotNull(state.VarianceProposalReview);
            if(scenario=="grant")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"];
            if(scenario is "unknown" or "retry" or "recreate" or "changed")n.Native.FailCommit=true;
            await Invoke("RecordVariance");
            if(scenario!="valid")
            {
                Assert.True(state.HasUnconfirmedVarianceIntent);Assert.Null(state.VarianceResult);originalIntent=state.VarianceIntent!.Command.IntentId;
                if(scenario is "retry" or "recreate" or "changed")
                {
                    n.Native.FailCommit=false;
                    if(scenario=="recreate"){((IDisposable)page).Dispose();root=await Render();page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;Assert.Equal(originalIntent,state.VarianceIntent!.Command.IntentId);}
                    if(scenario=="changed")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
                    await Invoke("RecordVariance");
                }
            }
            await Invoke("StateHasChanged");return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(scenario is "valid" or "retry" or "recreate")
        {
            Assert.False(state.HasUnconfirmedVarianceIntent);Assert.NotNull(state.VarianceResult);Assert.Contains("Recorded variance proposal confirmed against current invoice source",html);Assert.Contains("Verified recorded invoice variance proposal",html);Assert.Contains("Explicit accountant variance request.",html);Assert.Contains("Explicit explanation for price/line",html);
            if(originalIntent is not null)Assert.Equal(originalIntent,state.RecordedVariance!.Command.IntentId);
        }
        else{Assert.True(state.HasUnconfirmedVarianceIntent);Assert.DoesNotContain("Recorded variance proposal confirmed against current invoice source",html);Assert.Contains("Original variance proposal outcome is unconfirmed",html);Assert.Contains(originalIntent!,html);}
        Assert.Equal(scenario=="grant"?0:1,n.Native.Writes);Assert.Equal("DRAFT",f.Stored.STATUS);Assert.Contains("controller",html,StringComparison.OrdinalIgnoreCase);
    }
}
#endif
