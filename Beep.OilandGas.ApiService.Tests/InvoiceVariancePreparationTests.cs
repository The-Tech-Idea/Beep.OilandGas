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
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVariancePreparationTests
{
    internal static async Task<CompanyMatchingPolicyCommandTests.Fixture> Prepared()
    {
        var f=new CompanyMatchingPolicyCommandTests.Fixture();await f.PrepareDraft();await f.Record();await f.PrepareApproval();await f.Record();
        var native=f.ReadOwner.Native;FinancialReceiptConsumptionTests.Seed(native);
        var date=new DateTime(2026,1,2,0,0,0,DateTimeKind.Utc);native.Invoice.INVOICE_DATE=date;
        native.Invoice.ROW_CREATED_BY=native.Line.ROW_CREATED_BY="source-maker";native.Invoice.ROW_CREATED_DATE=native.Line.ROW_CREATED_DATE=date;
        native.Line.QUANTITY=2m;native.Line.UNIT_PRICE=1.005m;native.Line.LINE_TOTAL=native.Invoice.TOTAL_AMOUNT=2.01m;return f;
    }
    internal static CompanyClientContext Selected()
    {var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"]));return company;}
    internal static APInvoiceVarianceProposal Proposal(APInvoiceFinancialReview financial)=>new("REQUEST_VARIANCE_REVIEW",financial.Invoice.Revision,financial.Revision,
        financial.Checks.Where(row=>row.Outcome is APSourceComparisonOutcome.RecordedDifference or APSourceComparisonOutcome.ExceedsRecordedLimit).Select(row=>new APInvoiceVarianceExplanation(row.Code,"Reviewed partial billing against the recorded source; independent disposition required.")).ToArray(),"Request independent review of the explicit recorded differences.");
    internal static async Task<APInvoiceFinancialReview> Financial(CompanyMatchingPolicyCommandTests.Fixture f)
    {
        var native=f.ReadOwner.Native;var company=new CompanyScope("tenant","company");var invoice=await native.AP.GetSourceReviewAsync(company,"invoice","actor")??throw new InvalidOperationException("The owned invoice is unavailable.");
        var order=(await native.PO.GetSourceReviewAsync(company,"order","actor"))!;var source=new APLinePurchaseSourceReview(invoice,"line",order);
        var allocation=APReceiptAllocationReview.Create(company,"actor","invoice","line",invoice.Revision,source,await native.AP.GetReceiptAllocationsForLineAsync(company,"invoice","line"));
        var consumption=await native.AP.GetReceiptConsumptionAsync(company,"actor",source,"receipt",native.Authority);
        return APInvoiceFinancialReview.Create(invoice,[allocation],await f.Owner.GetCompanyMatchingPoliciesAsync(company,"actor"),[consumption]);
    }
    [Theory][InlineData("valid")][InlineData("correction")][InlineData("missing")][InlineData("duplicate")][InlineData("unknown")][InlineData("blank")][InlineData("control")][InlineData("bound")][InlineData("revision")][InlineData("disposition")]
    public async Task PreparationRequiresExactKnownDifferencesAndExplicitBoundedExplanations(string fault)
    {
        var f=await Prepared();var financial=await Financial(f);var input=Proposal(financial);
        input=fault switch{"correction"=>input with{Disposition="REQUEST_CORRECTION"},"missing"=>input with{Explanations=[]},"duplicate"=>input with{Explanations=input.Explanations.Concat([input.Explanations[0]]).ToArray()},"unknown"=>input with{Explanations=[new("policy-tax","Assumed zero tax.")]},"blank"=>input with{Reason=" "},"control"=>input with{Reason="reason\n"},"bound"=>input with{Reason=new string('x',1001)},"revision"=>input with{ExpectedFinancialRevision=new string('C',64)},"disposition"=>input with{Disposition="APPROVED"},_=>input};
        if(fault is "valid" or "correction")
        {
            var review=APInvoiceVarianceProposalReview.Create(financial,input,new string('B',64));review.Require(financial.Invoice.Company,"actor","invoice",input);
            Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Financial.Checks.Single(row=>row.Code=="decision").Outcome);Assert.Equal("DRAFT",review.Financial.Invoice.Invoice.STATUS);
        }
        else Assert.Throws<InvalidOperationException>(()=>APInvoiceVarianceProposalReview.Create(financial,input,new string('B',64)));
        Assert.Equal(2,f.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("maker")][InlineData("audit")][InlineData("inactive")][InlineData("status")][InlineData("policy")][InlineData("consumption")][InlineData("recorded")]
    public async Task SourceIndependenceAndCompleteEvidenceCannotBeAssumed(string fault)
    {
        var f=await Prepared();var native=f.ReadOwner.Native;
        if(fault=="maker")native.Invoice.ROW_CREATED_BY=native.Line.ROW_CREATED_BY="actor";if(fault=="audit")native.Line.ROW_CREATED_DATE=null;
        if(fault=="inactive")native.Invoice.ACTIVE_IND="N";if(fault=="status")native.Invoice.STATUS="POSTED";if(fault=="policy")f.Stored.MATCHING_POLICY_COMMAND_JSON=null;
        if(fault=="recorded")native.Invoice.VARIANCE_PROPOSAL_JSON="unverified prior proposal";
        var financial=await Financial(f);if(fault=="consumption")financial=APInvoiceFinancialReview.Create(financial.Invoice,financial.PurchaseLines,financial.MatchingPolicies);
        Assert.Throws<InvalidOperationException>(()=>APInvoiceVarianceProposalReview.Create(financial,Proposal(financial),new string('B',64)));
    }
    [Fact] public async Task PreparationFreezesInputsAndRejectsDifferentActorOrChangedReason()
    {
        var f=await Prepared();var financial=await Financial(f);var input=Proposal(financial);var review=APInvoiceVarianceProposalReview.Create(financial,input,new string('B',64));
        financial.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,review.Financial.Invoice.Invoice.TOTAL_AMOUNT);
        Assert.Throws<InvalidOperationException>(()=>review.Require(review.Financial.Invoice.Company,"controller","invoice",input));
        Assert.Throws<InvalidOperationException>(()=>review.Require(review.Financial.Invoice.Company,"actor","invoice",input with{Reason="different"}));
        review.Financial.ReceiptConsumption![0].Contributors[0].Allocations[0].ALLOCATED_QUANTITY=99m;Assert.Throws<InvalidOperationException>(()=>review.Require(review.Financial.Invoice.Company,"actor","invoice",input));
    }
    [Fact] public async Task AgreementCannotBeManufacturedIntoAVarianceRequest()
    {
        var f=await Prepared();f.ReadOwner.Native.OrderLine.QUANTITY=2m;f.ReadOwner.Native.OrderLine.UNIT_PRICE=1.005m;var financial=await Financial(f);
        Assert.Empty(Proposal(financial).Explanations);Assert.Throws<InvalidOperationException>(()=>APInvoiceVarianceProposalReview.Create(financial,Proposal(financial),new string('B',64)));
    }
    [Fact] public async Task CorrectionRetainsUnavailablePolicyEvidenceWithoutInventingAnApprovedPolicy()
    {
        var f=await Prepared();f.Stored.MATCHING_POLICY_COMMAND_JSON=null;var financial=await Financial(f);var input=Proposal(financial) with{Disposition="REQUEST_CORRECTION"};
        var review=APInvoiceVarianceProposalReview.Create(financial,input,new string('B',64));Assert.Null(review.Financial.PolicySelection.Rules);
        Assert.DoesNotContain(review.Proposal.Explanations,row=>row.CheckCode=="policy");Assert.Equal(APSourceComparisonOutcome.Unavailable,review.Financial.Checks.Single(row=>row.Code=="policy").Outcome);
    }
    [Fact] public async Task WorkspaceProtectsPreparationAndClearsItBeforeAFailedReload()
    {
        var f=await Prepared();var financial=await Financial(f);var input=Proposal(financial);using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(financial.Invoice));await state.LoadFinancialReviewAsync((_,_)=>Task.FromResult(financial));
        await state.ReviewVarianceProposalAsync(input.Disposition,input.Explanations,input.Reason,(_,proposal)=>Task.FromResult(APInvoiceVarianceProposalReview.Create(financial,proposal,new string('B',64))));
        state.VarianceProposalReview!.Financial.Invoice.Invoice.TOTAL_AMOUNT=99m;Assert.Equal(2.01m,state.VarianceProposalReview!.Financial.Invoice.Invoice.TOTAL_AMOUNT);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewVarianceProposalAsync(input.Disposition,input.Explanations,input.Reason,(_,_)=>throw new InvalidOperationException("unavailable")));Assert.Null(state.VarianceProposalReview);
    }
    [Theory][InlineData("valid")][InlineData("grant")][InlineData("role")][InlineData("changed")][InlineData("binding")][InlineData("maker")]
    public async Task ActualApiAndTypedClientRequireCurrentAuthorityAndExactDisplayedEvidence(string fault)
    {
        var f=await Prepared();await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Owner);services.AddSingleton(f.ReadOwner.Native.AP);services.AddSingleton(f.ReadOwner.Native.PO);});
        host.Context.PolicyBinding=new string('B',64);host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"];
        using var company=Selected();var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var invoice=(await client.GetAPInvoiceAsync("invoice"))!;var financial=await client.GetAPInvoiceFinancialReviewAsync("invoice",invoice.Revision);var input=Proposal(financial);
        if(fault=="grant")host.Context.Permissions=["Accounting.View","Procurement.View"];if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
        if(fault=="changed")f.ReadOwner.Native.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="binding"){host.Context.PolicyBinding=null;host.Context.Calls=0;host.Context.ChangeBinding=true;}
        if(fault=="maker")f.ReadOwner.Native.Invoice.ROW_CREATED_BY=f.ReadOwner.Native.Line.ROW_CREATED_BY="actor";
        if(fault=="valid"){var review=await client.ReviewAPInvoiceVarianceAsync("invoice",input);review.Require(new("tenant","company"),"actor","invoice",input);Assert.Equal(financial.Revision,review.Financial.Revision);}
        else{var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.ReviewAPInvoiceVarianceAsync("invoice",input));Assert.Equal(fault is "grant" or "role" or "binding"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,failure.StatusCode);}
        Assert.Equal(2,f.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.ReadOwner.Native.Invoice.STATUS);
    }
    [Theory][InlineData("company")][InlineData("financial")][InlineData("input")][InlineData("dispose")]
    public async Task WorkspaceRefusesLatePreparationAfterEvidenceOrInputInvalidation(string fault)
    {
        var f=await Prepared();var financial=await Financial(f);var input=Proposal(financial);using var company=Selected();using var state=new APInvoiceWorkspaceState(company);
        await state.LoadDetailAsync("invoice",_=>Task.FromResult<APInvoiceSourceReview?>(financial.Invoice));await state.LoadFinancialReviewAsync((_,_)=>Task.FromResult(financial));
        var pending=new TaskCompletionSource<APInvoiceVarianceProposalReview>();var loading=state.ReviewVarianceProposalAsync(input.Disposition,input.Explanations,input.Reason,(_,_)=>pending.Task);
        if(fault=="company")company.Clear();if(fault=="financial")state.ClearFinancialReview();if(fault=="input")state.ClearVarianceProposal();if(fault=="dispose")state.Dispose();
        pending.SetResult(APInvoiceVarianceProposalReview.Create(financial,input,new string('B',64)));await loading;Assert.Null(state.VarianceProposalReview);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task CompleteInvoicePageReviewsExplicitInputsAndClearsFailedReload(bool revoke)
    {
        var f=await Prepared();await using var host=await AccountingSourceHttpTests.Start(configureAccounting:(services,_)=>{services.AddSingleton(f.Owner);services.AddSingleton(f.ReadOwner.Native.AP);services.AddSingleton(f.ReadOwner.Native.PO);});
        host.Context.PolicyBinding=new string('B',64);host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareInvoiceVariance"];
        using var company=Selected();var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(company);
        services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddScoped<CompanyAccountingHandoffState>();services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<PurchaseOrderWorkspaceState>();
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
        var html=await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method,params object[] args){var result=page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,args);if(result is Task task)await task;}
            await Invoke("ReviewFinancial");await Invoke("VarianceDispositionChanged","REQUEST_VARIANCE_REVIEW");await Invoke("VarianceReasonChanged","Independent review requested.");
            foreach(var row in state.RecordedVariances)await Invoke("VarianceExplanationChanged",row.Code,"Explicit source explanation for "+row.Code);
            await Invoke("ReviewVariance");Assert.NotNull(state.VarianceProposalReview);
            await Invoke("VarianceReasonChanged","Independent review reason revised.");Assert.Null(state.VarianceProposalReview);await Invoke("ReviewVariance");Assert.Equal("Independent review reason revised.",state.VarianceProposalReview!.Proposal.Reason);
            if(revoke){host.Context.Permissions=["Accounting.View","Procurement.View"];await Invoke("ReviewVariance");Assert.Null(state.VarianceProposalReview);Assert.Null(state.FinancialReview);}
            await Invoke("StateHasChanged");return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(revoke){Assert.DoesNotContain("Fresh variance preparation reviewed",html);Assert.Contains("Variance preparation could not be confirmed",html);}
        else{Assert.Contains("Accountant invoice variance preparation",html);Assert.Contains("Fresh variance preparation reviewed",html);Assert.Contains("Explicit source explanation for price/line",html);Assert.Contains("No durable proposal or controller decision has been recorded",html);Assert.Contains("Unavailable diagnostics requiring separate evidence",html);}
        Assert.Equal(2,f.ReadOwner.Native.Native.Writes);Assert.Equal("DRAFT",f.ReadOwner.Native.Invoice.STATUS);
    }
}
#endif
