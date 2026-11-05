using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Security.Claims;
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
public sealed class InvoiceVarianceDecisionReviewTests
{
    internal static async Task<InvoiceVarianceCommandTests.Fixture> Recorded(bool within=true)
    {
        var f=new InvoiceVarianceCommandTests.Fixture();await f.Prepare();
        if(within)
        {
            f.Existing.ReadOwner.Native.OrderLine.QUANTITY=2m;
            var financial=await InvoiceVariancePreparationTests.Financial(f.Existing);
            f.Review=APInvoiceVarianceProposalReview.Create(financial,InvoiceVariancePreparationTests.Proposal(financial),new string('B',64));
            f.Command=new(new("tenant","company"),"actor","invoice",f.Review.Proposal,f.Review.Revision,f.Review.BindingRevision,Guid.NewGuid().ToString("N"));
        }
        await f.Record();return f;
    }
    internal static async Task<APInvoiceFinancialReview> Financial(InvoiceVarianceCommandTests.Fixture f,string actor="reviewer")
    {
        var n=f.Existing.ReadOwner.Native;var company=new CompanyScope("tenant","company");
        var invoice=await n.AP.GetSourceReviewAsync(company,"invoice",actor)??throw new InvalidOperationException("The owned controller invoice is unavailable.");var order=(await n.PO.GetSourceReviewAsync(company,"order",actor))!;
        var source=new APLinePurchaseSourceReview(invoice,"line",order);
        var allocation=APReceiptAllocationReview.Create(company,actor,"invoice","line",invoice.Revision,source,await n.AP.GetReceiptAllocationsForLineAsync(company,"invoice","line"));
        return APInvoiceFinancialReview.Create(invoice,[allocation],await f.Existing.Owner.GetCompanyMatchingPoliciesAsync(company,actor),[await n.AP.GetReceiptConsumptionAsync(company,actor,source,"receipt",n.Authority)]);
    }
    internal static CompanyClientContext Selected(string actor="reviewer",bool grant=true)
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredControllerRole"],grant?["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"]:["Accounting.View","Procurement.View"]));return company;}
    [Theory][InlineData(true)][InlineData(false)]
    public async Task IndependentReviewUsesCurrentCompleteEvidenceAndNeverWritesDisposition(bool within)
    {
        var f=await Recorded(within);var current=await Financial(f);var review=APInvoiceVarianceDecisionReview.Create(current,f.Command.BindingRevision);
        review.Require(f.Command.Company,"reviewer","invoice",current.Invoice.Revision);Assert.True(review.EvidenceMatchesProposal);
        Assert.Equal(f.Review.Financial.Revision,review.Evidence.Revision);Assert.Equal("reviewer",review.Evidence.Invoice.ActorUserId);
        Assert.Equal("actor",review.RecordedProposal.Command.ActorUserId);Assert.Equal(within,review.EligibleDispositions.Contains("ACCEPT_WITHIN_POLICY_VARIANCE"));
        Assert.Contains("REQUEST_CORRECTION",review.EligibleDispositions);Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.Stored.STATUS);
    }
    [Theory][InlineData("actor")][InlineData("source-maker")][InlineData("SYSTEM")][InlineData("bad\nactor")]
    public async Task PreparerOrSourceMakerCannotReviewOwnProposal(string actor)
    {var f=await Recorded();await Assert.ThrowsAsync<InvalidOperationException>(async()=>APInvoiceVarianceDecisionReview.Create(await Financial(f,actor),f.Command.BindingRevision));}
    [Theory][InlineData("contributor")][InlineData("purchase")][InlineData("line")][InlineData("policy")][InlineData("receiver")]
    public async Task LaterEvidenceIsVisibleAndCannotOfferWithinPolicyAcceptance(string change)
    {
        var f=await Recorded();var n=f.Existing.ReadOwner.Native;
        if(change=="contributor")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(change=="purchase")n.OrderLine.UNIT_PRICE=2m;if(change=="line")n.Line.ITEM_DESCRIPTION="changed";
        if(change=="policy")f.Existing.Stored.DESCRIPTION="changed";if(change=="receiver")n.Receipt.RECEIVER_DECISION_JSON=null;
        var review=APInvoiceVarianceDecisionReview.Create(await Financial(f),f.Command.BindingRevision);
        Assert.False(review.EvidenceMatchesProposal);Assert.Equal(new[]{"REQUEST_CORRECTION"},review.EligibleDispositions);Assert.Equal(1,n.Native.Writes);
    }
    [Theory][InlineData("binding")][InlineData("proof")][InlineData("header")][InlineData("revision")]
    public async Task UnverifiedOrTamperedReviewCannotBeConsumed(string fault)
    {
        var f=await Recorded();var current=await Financial(f);var review=APInvoiceVarianceDecisionReview.Create(current,f.Command.BindingRevision);
        if(fault=="binding")review=review with{BindingRevision=new string('C',64)};
        if(fault=="proof")review=review with{RecordedProposal=review.RecordedProposal with{Command=f.Command with{IntentId=Guid.NewGuid().ToString("N")}}};
        if(fault=="header")review.Current.Invoice.Invoice.TOTAL_AMOUNT=99m;if(fault=="revision")review=review with{Revision=new string('C',64)};
        Assert.Throws<InvalidOperationException>(()=>review.Require(f.Command.Company,"reviewer","invoice",current.Invoice.Revision));
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("preparer")][InlineData("creator")][InlineData("binding")][InlineData("late-grant")][InlineData("late-role")][InlineData("source")]
    public async Task ActualApiAndClientIndependentlyEnforceControllerReview(string fault)
    {
        var f=await Recorded();var n=f.Existing.ReadOwner.Native;var actor=fault=="preparer"?"actor":fault=="creator"?"source-maker":"reviewer";
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Actor=actor;host.Context.Roles=["ConfiguredControllerRole"];host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredControllerRole");
        using var company=Selected(actor);var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var current=await Financial(f,actor);
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];
        if(fault=="role")host.Context.Roles=["OtherRole"];if(fault=="binding")host.Context.PolicyBinding=new string('C',64);
        if(fault=="late-grant")host.Context.OnCapture=_=>{if(host.Context.Calls>2)host.Context.Permissions=["Accounting.View","Procurement.View"];};
        if(fault=="late-role")host.Context.OnCapture=_=>{if(host.Context.Calls>2)host.Context.Roles=["OtherRole"];};
        if(fault=="source")f.Stored.TOTAL_AMOUNT=99m;
        if(fault=="valid"){var review=await client.GetAPInvoiceVarianceDecisionReviewAsync("invoice",current.Invoice.Revision);Assert.Contains("ACCEPT_WITHIN_POLICY_VARIANCE",review.EligibleDispositions);}
        else{var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPInvoiceVarianceDecisionReviewAsync("invoice",current.Invoice.Revision));Assert.Equal(fault is "binding" or "source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);}
        Assert.Equal(1,n.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("clear")][InlineData("company")][InlineData("dispose")][InlineData("grant")][InlineData("failure")]
    public async Task ScopedReviewCopiesEvidenceAndRejectsLateOrFailedPublication(string scenario)
    {
        var f=await Recorded();using var company=Selected(grant:scenario!="grant");using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",id=>f.Existing.ReadOwner.Native.AP.GetSourceReviewAsync(f.Command.Company,id,"reviewer"));
        var review=APInvoiceVarianceDecisionReview.Create(await Financial(f),f.Command.BindingRevision);
        if(scenario=="grant"){await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVarianceDecisionReviewAsync((_,_)=>Task.FromResult(review)));return;}
        await state.LoadVarianceDecisionReviewAsync((_,_)=>Task.FromResult(review));Assert.NotNull(state.VarianceDecisionReview);
        state.VarianceDecisionReview!.Current.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceDecisionReview!.Current.Invoice.Invoice.TOTAL_AMOUNT);
        if(scenario=="failure"){await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVarianceDecisionReviewAsync((_,_)=>throw new InvalidOperationException("unavailable")));Assert.Null(state.VarianceDecisionReview);return;}
        var pending=new TaskCompletionSource<APInvoiceVarianceDecisionReview>();var loading=state.LoadVarianceDecisionReviewAsync((_,_)=>pending.Task);
        if(scenario=="clear")state.Clear();if(scenario=="company")company.Clear();if(scenario=="dispose")state.Dispose();pending.SetResult(review);await loading;
        Assert.Equal(scenario=="valid",state.VarianceDecisionReview is not null);
    }
    private sealed class Authentication:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"ConfiguredControllerRole")],"test"))));}
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompleteControllerScreenLoadsIndependentReviewAndClearsItWhenServerGrantIsLost(bool lost)
    {
        var f=await Recorded();var n=f.Existing.ReadOwner.Native;
        await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Existing.Owner);services.AddSingleton(n.AP);services.AddSingleton(n.PO);});
        host.Context.Actor="reviewer";host.Context.Roles=["ConfiguredControllerRole"];host.Context.PolicyBinding=f.Command.BindingRevision;host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.ReviewInvoiceVariance"];
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredControllerRole");
        using var company=Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
            var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method){var result=page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,[]);if(result is Task task)await task;}
            await Invoke("ReviewVarianceDecision");Assert.NotNull(state.VarianceDecisionReview);
            if(lost){host.Context.Permissions=["Accounting.View","Procurement.View"];await Invoke("ReviewVarianceDecision");Assert.Null(state.VarianceDecisionReview);}
            await Invoke("StateHasChanged");return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(lost)Assert.DoesNotContain("Eligible variance dispositions:",html);
        else{Assert.Contains("Reviewer account: reviewer",html);Assert.Contains("Proposal preparer: actor",html);Assert.Contains("ACCEPT_WITHIN_POLICY_VARIANCE",html);Assert.Contains("No durable controller disposition",html);}
        Assert.Equal(1,n.Native.Writes);Assert.Equal("DRAFT",f.Stored.STATUS);
    }
}
#endif
