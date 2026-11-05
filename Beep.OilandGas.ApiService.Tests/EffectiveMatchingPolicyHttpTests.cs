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
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class EffectiveMatchingPolicyHttpTests
{
    private static async Task<CompanyMatchingPolicyCommandTests.Fixture> Prepared()
    {
        var f=new CompanyMatchingPolicyCommandTests.Fixture();await f.PrepareDraft();await f.Record();await f.PrepareApproval();await f.Record();
        var source=f.ReadOwner.Native;source.Invoice.INVOICE_DATE=new DateTime(2026,1,2,0,0,0,DateTimeKind.Utc);source.Invoice.ACTIVE_IND=source.Line.ACTIVE_IND=source.Order.ACTIVE_IND=source.OrderLine.ACTIVE_IND="Y";
        source.Line.UNIT_PRICE=1.005m;return f;
    }
    private static CompanyClientContext Selected()
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View"]));return company;}
    [Theory][InlineData("valid")][InlineData("changed")][InlineData("unverified")][InlineData("permission")][InlineData("binding")][InlineData("schema")]
    public async Task FinancialApiCapturesCompletePolicyHistoryWithInvoiceAndPurchaseEvidence(string fault)
    {
        var f=await Prepared();await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Owner);services.AddSingleton(f.ReadOwner.Native.AP);services.AddSingleton(f.ReadOwner.Native.PO);});
        using var company=Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var invoice=(await client.GetAPInvoiceAsync("invoice"))!;
        if(fault=="unverified")f.Stored.MATCHING_POLICY_COMMAND_JSON=null;if(fault=="permission")host.Context.Permissions=[];if(fault=="binding"){host.Context.Calls=0;host.Context.ChangeBinding=true;}
        if(fault=="schema")f.ReadOwner.Native.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="ACCOUNTING_METHOD"),true)).Returns(new EntityStructure());
        if(fault=="changed")
        {
            var calls=0;f.ReadOwner.Native.Native.Source.Setup(source=>source.GetEntityAsync("ACCOUNTING_METHOD",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>
            {var row=PurchaseReceiptDecisionProof.Copy(f.Stored);if(++calls==1)f.Stored.DESCRIPTION="changed after first policy capture";return new object[]{row};});
        }
        if(fault is "valid" or "unverified")
        {
            var review=await client.GetAPInvoiceFinancialReviewAsync("invoice",invoice.Revision);Assert.NotNull(review.MatchingPolicies);
            Assert.Equal(fault=="valid"?"new-policy":null,review.PolicySelection.PolicyId);
            Assert.Equal(fault=="valid"?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="policy-price/line").Outcome);
            Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Checks.Single(row=>row.Code=="decision").Outcome);
        }
        else{var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPInvoiceFinancialReviewAsync("invoice",invoice.Revision));Assert.Equal(fault is "permission" or "binding"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);}
        Assert.Equal(2,f.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.ReadOwner.Native.Invoice.STATUS);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompleteInvoiceScreenShowsPolicyVarianceAndRefusesUnavailableReload(bool schemaFailure)
    {
        var f=await Prepared();FinancialReceiptConsumptionTests.Seed(f.ReadOwner.Native,"exceeds");await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Owner);services.AddSingleton(f.ReadOwner.Native.AP);services.AddSingleton(f.ReadOwner.Native.PO);});
        using var company=Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
            var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Load(){await (Task)page.GetType().GetMethod("ReviewFinancial",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null)!;page.GetType().GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);}
            await Load();Assert.Equal("new-policy",state.FinancialReview!.PolicySelection.PolicyId);
            if(schemaFailure){f.ReadOwner.Native.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="ACCOUNTING_METHOD"),true)).Returns(new EntityStructure());await Load();}
            return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(schemaFailure){Assert.Null(state.FinancialReview);Assert.DoesNotContain("Verified applicable policy: new-policy",html);}
        else{Assert.Contains("Verified applicable policy: new-policy",html);Assert.Contains("Policy price variance for line",html);Assert.Contains("Within recorded limit",html);Assert.Contains("no tax decision or amount is inferred",html);Assert.Contains("Complete receipt consumption across invoices",html);Assert.Contains("Open contributor invoice second",html);Assert.Contains("/accounting/payables/invoices/second",html);Assert.Contains("Allocation: competitor",html);Assert.Contains("all active allocations: 11",html);Assert.Contains("verified accepted quantity: 10",html);Assert.Contains("Exceeds recorded limit",html);}
        Assert.Equal(2,f.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.ReadOwner.Native.Invoice.STATUS);
    }
}
#endif
