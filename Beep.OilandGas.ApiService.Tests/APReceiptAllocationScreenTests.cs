#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Text.Json;
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

public sealed class APReceiptAllocationScreenTests
{
    private static readonly string[] Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation","Accounting.RecordReceiptAllocation"];
    private static void Select(CompanyClientContext company,string actor="actor",string owner="company")=>company.TrySelect(company.Clear(),actor,new(new("tenant",owner),["ConfiguredCompanyRole"],Permissions));
    private static AccountingServiceClient Client(AccountingSourceHttpTests.Host host,CompanyClientContext company)=>new(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
    private static async Task Prepare(APInvoiceWorkspaceState state,IAccountingServiceClient client)
    {
        await state.LoadDetailAsync("invoice",client.GetAPInvoiceAsync);await state.LoadPurchaseSourceAsync("line",client.GetAPLinePurchaseSourceAsync);
        await state.LoadReceiptAllocationsAsync(client.GetAPReceiptAllocationsAsync);state.SetProposalReceipt("receipt");state.SetProposalQuantity(2m);
        await state.ReviewAllocationProposalAsync(client.ReviewAPReceiptAllocationAsync);
    }
    private static Task Submit(APInvoiceWorkspaceState state,IAccountingServiceClient client,string reason="Reviewed receipt quantity.")=>state.RecordAllocationAsync(
        client.RecordAPReceiptAllocationAsync,client.GetAPInvoiceAsync,client.GetAPLinePurchaseSourceAsync,client.GetAPReceiptAllocationsAsync,
        client.GetAPReceiptConsumptionAsync,client.GetAPInvoiceFinancialReviewAsync,reason);
    [Fact] public async Task ActualApiStateConfirmsOwnedLineReceiptContributorsAndWholeInvoiceBeforeSuccess()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();fixture.AddCompeting(1m);await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);using var state=new APInvoiceWorkspaceState(company);var client=Client(host,company);
        await Prepare(state,client);Assert.True(state.CanRecordAllocation);await Submit(state,client);
        Assert.NotNull(state.AllocationReceipt);Assert.False(state.HasUnconfirmedAllocationIntent);Assert.Equal(1,fixture.Native.Writes);
        Assert.Equal(3m,state.RefreshedReceiptConsumption!.RecordedActiveQuantity);Assert.Equal(2,state.RefreshedReceiptConsumption.Contributors.Count);
        Assert.Equal(2m,Assert.Single(state.Allocations!).Quantity);Assert.Equal(state.AllocationRevision,Assert.Single(state.FinancialReview!.PurchaseLines).Revision);
        Assert.Null(state.ReviewedProposal);Assert.False(state.CanRecordAllocation);Assert.Equal("DRAFT",state.Detail!.Status);
        state.AllocationReceipt!.Allocation.ALLOCATED_QUANTITY=999m;
        state.RefreshedReceiptConsumption!.Contributors[0].Allocations[0].ALLOCATED_QUANTITY=999m;
        Assert.Equal(2m,state.AllocationReceipt!.Allocation.ALLOCATED_QUANTITY);Assert.Equal(3m,state.RefreshedReceiptConsumption!.RecordedActiveQuantity);
    }
    [Fact] public async Task UncertainCommitRetainsOriginalIntentAndReasonForExplicitReconciliation()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);using var state=new APInvoiceWorkspaceState(company);var client=Client(host,company);await Prepare(state,client);
        fixture.Native.FailCommit=true;await Assert.ThrowsAsync<OilGasApiException>(()=>Submit(state,client,"Original reason."));
        var original=state.AllocationIntent!;Assert.True(state.HasUnconfirmedAllocationIntent);Assert.True(state.CanReconcileAllocation);Assert.Null(state.AllocationReceipt);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAllocationProposalAsync(client.ReviewAPReceiptAllocationAsync));
        fixture.Native.FailCommit=false;await Submit(state,client,"Replacement reason.");
        Assert.Equal(original.Command,state.AllocationIntent!.Command);Assert.Equal("Original reason.",state.AllocationReceipt!.Command.Reason);Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("account")][InlineData("source")]
    public async Task LateChangesPreserveOriginalIntentAndRejectDuplicateWhileBusy(string change)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);using var state=new APInvoiceWorkspaceState(company);var client=Client(host,company);await Prepare(state,client);
        var ready=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var submit=state.RecordAllocationAsync(async input=>{var receipt=await client.RecordAPReceiptAllocationAsync(input);ready.SetResult();await release.Task;return receipt;},
            client.GetAPInvoiceAsync,client.GetAPLinePurchaseSourceAsync,client.GetAPReceiptAllocationsAsync,client.GetAPReceiptConsumptionAsync,client.GetAPInvoiceFinancialReviewAsync,"Original reason.");
        await ready.Task;var original=state.AllocationIntent!.Command;Assert.True(state.AllocationCommandBusy);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(state,client,"Duplicate."));
        if(change=="source")state.Clear();else Select(company,change=="account"?"other":"actor",change=="company"?"other":"company");
        release.SetResult();await submit;Assert.Null(state.AllocationReceipt);Assert.True(state.HasUnconfirmedAllocationIntent);Assert.False(state.CanReconcileAllocation);
        Assert.Equal(original,state.AllocationIntent!.Command);Select(company);await state.LoadDetailAsync("invoice",client.GetAPInvoiceAsync);
        await Submit(state,client,"Replacement.");Assert.NotNull(state.AllocationReceipt);Assert.Equal(1,fixture.Native.Writes);Assert.Equal(original,state.AllocationIntent!.Command);
    }
    [Theory][InlineData("missing")][InlineData("consumption")][InlineData("financial")]
    public async Task ReadBackFailureCannotPublishSuccessOrReplaceOriginalIntent(string fault)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);using var state=new APInvoiceWorkspaceState(company);var client=Client(host,company);await Prepare(state,client);
        var boundary=new Mock<IAccountingServiceClient>(MockBehavior.Strict);
        boundary.Setup(value=>value.RecordAPReceiptAllocationAsync(It.IsAny<APReceiptAllocationSubmission>())).Returns((APReceiptAllocationSubmission input)=>client.RecordAPReceiptAllocationAsync(input));
        boundary.Setup(value=>value.GetAPInvoiceAsync("invoice")).Returns(()=>client.GetAPInvoiceAsync("invoice"));
        boundary.Setup(value=>value.GetAPLinePurchaseSourceAsync("invoice","line",It.IsAny<string>())).Returns((string id,string line,string revision)=>client.GetAPLinePurchaseSourceAsync(id,line,revision));
        boundary.Setup(value=>value.GetAPReceiptAllocationsAsync("invoice","line",It.IsAny<string>(),It.IsAny<string>())).Returns(async(string id,string line,string revision,string order)=>{
            var review=await client.GetAPReceiptAllocationsAsync(id,line,revision,order);
            return fault=="missing"?APReceiptAllocationReview.Create(new("tenant","company"),"actor",id,line,revision,review.Source,[]):review;});
        boundary.Setup(value=>value.GetAPReceiptConsumptionAsync("invoice","line","receipt",It.IsAny<string>(),It.IsAny<string>())).Returns(async(string id,string line,string receipt,string revision,string order)=>{
            var review=await client.GetAPReceiptConsumptionAsync(id,line,receipt,revision,order);
            return fault=="consumption"?APReceiptConsumptionReview.Create(new("tenant","company"),"actor",review.Source,"receipt",[]):review;});
        boundary.Setup(value=>value.GetAPInvoiceFinancialReviewAsync("invoice",It.IsAny<string>())).Returns((string id,string revision)=>fault=="financial"?
            throw new InvalidOperationException("Explicit financial read failure."):client.GetAPInvoiceFinancialReviewAsync(id,revision));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(state,boundary.Object));var original=state.AllocationIntent!.Command;
        Assert.Null(state.AllocationReceipt);Assert.Null(state.FinancialReview);Assert.True(state.HasUnconfirmedAllocationIntent);
        await Submit(state,client,"Replacement.");Assert.Equal(original,state.AllocationReceipt!.Command);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task InvalidReasonCannotCreatePendingIntentOrOpenTransaction()
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);using var state=new APInvoiceWorkspaceState(company);var client=Client(host,company);await Prepare(state,client);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Submit(state,client,""));Assert.Null(state.AllocationIntent);Assert.Empty(fixture.Native.Transactions);
    }
    [Theory][InlineData("record")][InlineData("reconcile")][InlineData("permission")][InlineData("navigation")]
    public async Task CompleteInvoicePageHandlerRecordsOrReconcilesOriginalAllocation(string mode)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var client=Client(host,company);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<APInvoiceWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
        services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager,APInvoiceFinancialHttpTests.Navigation>();
        services.AddSingleton<AuthenticationStateProvider,APInvoiceFinancialHttpTests.Authentication>();services.AddSingleton<IAccountingServiceClient>(client);
        services.AddSingleton<IBusinessProcessServiceClient>(new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance));
        services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,APInvoiceFinancialHttpTests.Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
            var page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            async Task Invoke(string method,params object?[] args)=>await (Task)page.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,args)!;
            await Invoke("ReviewPurchase","line");await Invoke("ReviewAllocations");var state=provider.GetRequiredService<APInvoiceWorkspaceState>();
            state.SetProposalReceipt("receipt");state.SetProposalQuantity(2m);await Invoke("ReviewProposal");
            page.GetType().GetField("_allocationReason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Original screen reason.");
            if(mode=="permission")host.Context.Permissions=["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation"];
            if(mode is "reconcile" or "navigation")fixture.Native.FailCommit=true;
            await Invoke("RecordAllocation");
            if(mode is "reconcile" or "navigation")
            {
                Assert.True(state.HasUnconfirmedAllocationIntent);fixture.Native.FailCommit=false;
                if(mode=="navigation")
                {
                    var original=state.AllocationIntent!.Command;((IDisposable)page).Dispose();Assert.True(state.HasUnconfirmedAllocationIntent);
                    root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Pages.PPDM39.Accounting.PayablesWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id","invoice"}}));
                    page=((APInvoiceFinancialHttpTests.Components)provider.GetRequiredService<IComponentActivator>()).Page!;
                    Assert.Same(state,provider.GetRequiredService<APInvoiceWorkspaceState>());Assert.Equal(original,state.AllocationIntent!.Command);
                    Assert.Contains("Reconcile original allocation",WebUtility.HtmlDecode(root.ToHtmlString()));
                }
                page.GetType().GetField("_allocationReason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(page,"Replacement.");await Invoke("RecordAllocation");
            }
            typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(page,null);return WebUtility.HtmlDecode(root.ToHtmlString());
        });
        if(mode=="permission"){Assert.DoesNotContain("refreshed source evidence confirmed",html);Assert.Equal(0,fixture.Native.Writes);}
        else{Assert.Contains("Receipt allocation and refreshed source evidence confirmed",html);Assert.Contains("Refreshed receipt contributors: 1",html);Assert.Contains("Whole invoice financial evidence",html);Assert.Contains("Original screen reason.",provider.GetRequiredService<APInvoiceWorkspaceState>().AllocationReceipt!.Command.Reason);Assert.Equal(1,fixture.Native.Writes);}
    }
    [Theory][InlineData("read-only")][InlineData("permission")][InlineData("role")][InlineData("revision")][InlineData("receipt")][InlineData("binding")][InlineData("role-set")]
    public async Task ReceiptConsumptionReadRequiresOriginalOwnedEvidenceWithoutPreparationGrant(string fault)
    {
        var fixture=new APReceiptAllocationCommandTests.Fixture();fixture.AddCompeting(1m);await fixture.Prepare();await using var host=await APReceiptAllocationCommandHttpTests.Host(fixture);
        using var company=new CompanyClientContext();Select(company);var client=Client(host,company);
        var invoiceRevision=fixture.Command.Proposal.ExpectedInvoiceRevision;var receipt="receipt";
        host.Context.Permissions=fault=="permission"?["Accounting.View"]:["Accounting.View","Procurement.View"];
        if(fault=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
        if(fault=="revision")invoiceRevision=new string('a',64);if(fault=="receipt")receipt="foreign";
        if(fault=="binding")host.Context.ChangeBinding=true;
        if(fault=="role-set")host.Context.OnCapture=count=>{if(count>=4)host.Context.Roles=["ConfiguredCompanyRole","AnotherRole"];};
        if(fault=="read-only")
        {
            var review=await client.GetAPReceiptConsumptionAsync("invoice","line",receipt,invoiceRevision,fixture.Command.Proposal.ExpectedOrderRevision);
            Assert.Equal(1m,review.RecordedActiveQuantity);Assert.Equal("second",Assert.Single(review.Contributors).Source.Invoice.Invoice.AP_INVOICE_ID);
        }
        else
        {
            var error=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetAPReceiptConsumptionAsync("invoice","line",receipt,invoiceRevision,fixture.Command.Proposal.ExpectedOrderRevision));
            Assert.Equal(fault is "revision" or "receipt"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,error.StatusCode);
        }
        Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
}
#endif
