#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanySalesInvoiceReviewTests
{
    private const string Grant="Accounting.ReviewSalesInvoice";
    private static ClaimsPrincipal User(string actor,bool role=true)=>new(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,actor)}.Concat(role?[new Claim(ClaimTypes.Role,"ConfiguredCompanyRole")]:[]),"test"));
    private sealed class Authentication:AuthenticationStateProvider
    {
        public string Actor="reviewer";public bool Role=true;
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User(Actor,Role)));
        public void Change(string actor){Actor=actor;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private static async Task<(CompanySalesInvoiceHttpTests.Fixture Fixture,CompanySalesInvoiceDraftOutcome Outcome)> Prepared(string mode="partial")
    {
        var f=new CompanySalesInvoiceHttpTests.Fixture();await f.Prepare(mode);
        async Task<CompanySalesInvoiceDraftReview> Current()=>CompanySalesInvoiceDraftReview.Create(f.Data.Current.Delivery,f.Data.Current.Policies,
            await f.Data.Owner.GetCompanySalesInvoicesForDeliveryAsync(f.Data.Current.Company,f.Data.Current.FieldId,f.Ticket,f.Data.Authority),f.Data.Current.BindingRevision);
        var outcome=await f.Data.Owner.ExecuteCompanyInvoiceDraftAsync(f.Data.Command,f.Data.Authority,Current);return(f,outcome);
    }
    private static CompanyClientContext Select(CompanyScope company,string actor,params string[] grants)
    {var selected=new CompanyClientContext();selected.TrySelect(selected.Clear(),actor,new(company,["ConfiguredCompanyRole"],new[]{"Accounting.View"}.Concat(grants).ToArray()));return selected;}
    private static void Reviewer(AccountingSourceHttpTests.Host host,string actor)
    {host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);}
    [Theory][InlineData("independent")][InlineData("self")][InlineData("binding")][InlineData("transport")]
    public async Task CanonicalReviewSeparatesCurrentReaderFromMakerAndRefusesChangedOrTransportedEvidence(string mode)
    {
        var(f,outcome)=await Prepared();var original=f.Data.Current;var actor=mode=="self"?f.Actor:"reviewer";var field=original.Delivery.Source.Source.Source;
        var readerField=FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows);
        var contract=RevenueSalesContractReview.Create(readerField,original.Delivery.Source.Source.TransactionId,original.Delivery.Source.Source.Contract);
        var delivery=new RevenueDeliveryAcceptanceReadReview(RevenueDeliverySourceReview.Create(contract,original.Delivery.Source.Allocation,original.Delivery.Source.Ticket),original.Delivery.Recorded);
        var current=CompanySalesInvoiceDraftReview.Create(delivery,CompanyBillingPolicyReview.Create(original.Company,actor,original.Policies.Records),[outcome.Invoice],mode=="binding"?new string('C',64):original.BindingRevision);
        var review=CompanySalesInvoiceReviewSource.Create(outcome,current);review.Require(original.Company,actor,outcome.Invoice.INVOICE_ID);
        Assert.Equal(mode!="self",review.IsIndependent);Assert.Equal(mode!="binding",review.HasCurrentOriginalSource);Assert.Equal(mode is "independent" or "transport",review.CanReview);
        if(mode is "self" or "binding")Assert.Throws<InvalidOperationException>(review.RequireIndependentCurrent);else review.RequireIndependentCurrent();
        if(mode=="transport"){var altered=review with{Revision=new string('C',64)};Assert.Throws<InvalidOperationException>(()=>altered.Require(original.Company,actor,review.InvoiceId));}
        Assert.Equal(f.Actor,review.Original.Invoice.ROW_CREATED_BY);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Theory][InlineData("view-only")][InlineData("prepare")][InlineData("policy")][InlineData("role")][InlineData("field")][InlineData("receipt")]
    public async Task ReviewApiRequiresDistinctCurrentReviewRoleAndFieldAndCompleteReceipt(string fault)
    {
        var(f,outcome)=await Prepared();await using var host=await f.Start(role:fault!="role",permissions:fault is "role" or "field" or "receipt"?["Accounting.View",Grant]:["Accounting.View",fault=="prepare"?"Accounting.PrepareSalesInvoice":fault=="policy"?"Accounting.ApproveBillingPolicy":"Accounting.View"]);
        Reviewer(host,"reviewer");f.AssetDenied=fault=="field";if(fault=="receipt")f.Data.Native.Rows.RemoveAll(row=>row is SALES_INVOICE_OUTCOME);
        var response=await host.Client.GetAsync("api/accounting/traditional/company-invoices/review/"+outcome.Invoice.INVOICE_ID);
        Assert.Equal(fault=="receipt"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(3,f.Data.Native.Writes);
    }
    [Theory][InlineData("independent")][InlineData("self")][InlineData("policy-advanced")][InlineData("auth")][InlineData("locale")]
    public async Task ActualApiClientAndReviewerPageShowOriginalFactsCurrentSourceAndNoApprovalWrite(string mode)
    {
        var originalCulture=CultureInfo.CurrentCulture;
        try{if(mode=="locale")CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("ar-SA");await ReviewerPage(mode);}
        finally{CultureInfo.CurrentCulture=originalCulture;}
    }
    private static async Task ReviewerPage(string mode)
    {
        var(f,outcome)=await Prepared();if(mode=="policy-advanced")
        {
            var history=await f.Data.Policy.Owner.GetCompanyBillingPoliciesAsync(f.Data.Current.Company,"next-maker",f.Data.Authority);
            var rules=CompanyBillingPolicyTests.Rules with{Version="sales-2027",EffectiveFrom=new DateTime(2027,1,1),EffectiveThrough=new DateTime(2027,12,31)};
            await f.Data.Policy.Owner.PrepareCompanyBillingPolicyAsync(new(Guid.NewGuid().ToString("N"),history.Company,"next-maker","next-policy",rules,"Later calendar policy",history.Revision,new string('B',64)),history,f.Data.Authority);
            history=await f.Data.Policy.Owner.GetCompanyBillingPoliciesAsync(history.Company,"next-reviewer",f.Data.Authority);var row=history.Records.Single(value=>value.ACCOUNTING_METHOD_ID=="next-policy");
            await f.Data.Policy.Owner.ApproveCompanyBillingPolicyAsync(new(Guid.NewGuid().ToString("N"),history.Company,"next-reviewer",row.ACCOUNTING_METHOD_ID,CompanyBillingPolicyProof.Require(history.Company,row).RowRevision,"Independent later policy",history.Revision,new string('B',64)),history,f.Data.Authority);
        }
        var actor=mode=="self"?f.Actor:"reviewer";await using var host=await f.Start(permissions:["Accounting.View",Grant]);Reviewer(host,actor);using var selected=Select(f.Data.Current.Company,actor,Grant);
        var client=CompanySalesInvoiceHttpTests.Fixture.Client(host.Client,selected);var auth=new Authentication{Actor=actor};var writes=f.Data.Native.Writes;
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(selected);services.AddScoped<CompanySalesInvoiceReviewWorkspaceState>();
        services.AddSingleton<IAccountingServiceClient>(client);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<CompanySalesInvoiceReviewWorkspaceState>();
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.SalesInvoiceReview>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Invoice",outcome.Invoice.INVOICE_ID}}));var html=WebUtility.HtmlDecode(root.ToHtmlString());
            Assert.NotNull(state.Source);Assert.Contains("Sales invoice reviewer workspace",html);Assert.Contains(outcome.Invoice.INVOICE_NUMBER,html);Assert.Contains(outcome.Invoice.TOTAL_AMOUNT.ToString(CultureInfo.CurrentCulture),html);Assert.Contains("Tax preparation evidence",html);
            Assert.Contains("2026-06-01",html);Assert.Contains("2026-07-01",html);
            Assert.Contains(mode=="self"?"maker cannot independently review":"reviewer is independent and current evidence matches",html);Assert.Contains("records no approval",html);
            Assert.Equal(mode!="self",state.Source!.CanReview);if(mode=="policy-advanced")Assert.Equal(2,state.Source.Current.Policies.Records.Count);
            if(mode=="auth"){auth.Change("other-account");Assert.Null(state.Source);Assert.DoesNotContain(outcome.Invoice.INVOICE_NUMBER,root.ToHtmlString());}
        });Assert.Equal(writes,f.Data.Native.Writes);Assert.Equal("DRAFT",Assert.Single(f.Data.Native.Rows.OfType<INVOICE>()).STATUS);
    }
    [Theory][InlineData("company")][InlineData("auth")][InlineData("detached")]
    public async Task LateReviewerReadCannotExposeInvoiceOrPrivateMakerEvidence(string mode)
    {
        var(f,outcome)=await Prepared();var original=f.Data.Current;using var company=Select(original.Company,f.Actor,Grant);using var state=new CompanySalesInvoiceReviewWorkspaceState(company);var owner=new object();state.AttachOwner(owner);state.Admit(User(f.Actor));
        var source=CompanySalesInvoiceReviewSource.Create(outcome,CompanySalesInvoiceDraftReview.Create(original.Delivery,original.Policies,[outcome.Invoice],original.BindingRevision));
        var delayed=new TaskCompletionSource<CompanySalesInvoiceReviewSource>(TaskCreationOptions.RunContinuationsAsynchronously);var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetCompanySalesInvoiceReviewAsync(outcome.Invoice.INVOICE_ID)).Returns(delayed.Task);
        var pending=state.LoadAsync(outcome.Invoice.INVOICE_ID,client.Object);if(mode=="company")company.Clear();else if(mode=="auth")state.Invalidate();else state.DetachOwner(owner);
        delayed.SetResult(source);await pending;Assert.Null(state.Source);Assert.Equal(3,f.Data.Native.Writes);
    }
}
#endif
