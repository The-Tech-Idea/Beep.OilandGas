using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class AcceptedReceiptCapacityTests
{
    [Theory][InlineData(8,6,2,true)][InlineData(8,7,2,false)][InlineData(8,8,1,false)][InlineData(0,0,1,false)]
    public async Task ActualReceiverDecisionGatesNativeAllocationAgainstEveryInvoiceContributor(int accepted,int consumed,int proposed,bool allowed)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await f.Prepare(accepted,10-accepted);await f.Record();
        if(consumed>0)f.Existing.AddCompeting(consumed);await f.Existing.Prepare(proposed);
        var review=f.Existing.Displayed;Assert.Equal(10m,review.Source.Source.Order.Receipts[0].RECEIVED_QUANTITY);
        Assert.Equal(accepted,review.ReceiptConsumption!.VerifiedAcceptedQuantity);Assert.Equal(10-accepted,review.ReceiptConsumption.VerifiedRejectedQuantity);
        Assert.Equal(consumed,review.ReceiptConsumption.RecordedActiveQuantity);
        Assert.Equal(APSourceComparisonOutcome.WithinRecordedLimit,review.Comparisons.Single(row=>row.Code=="receipt-quantity").Outcome);
        Assert.Equal(allowed?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit,review.Comparisons.Single(row=>row.Code=="acceptance").Outcome);
        if(allowed){var row=await f.Existing.Record();Assert.Equal(proposed,row.ALLOCATED_QUANTITY);await f.Existing.Record();Assert.Equal(2,f.Existing.Native.Writes);}
        else{var writes=f.Existing.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Existing.Record());Assert.Equal(writes,f.Existing.Native.Writes);}
        Assert.Equal("DRAFT",f.Existing.Invoice.STATUS);
    }
    [Theory][InlineData("missing")][InlineData("proof")][InlineData("quantity")][InlineData("actor")][InlineData("order-unit")][InlineData("inactive")][InlineData("consumption-unit")][InlineData("activity")]
    public async Task UnverifiedAcceptanceOrConsumptionCannotAuthorizeNativeRecording(string fault)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();if(fault!="missing"){await f.Prepare();await f.Record();}
        if(fault=="proof")f.Stored.RECEIVER_DECISION_JSON="{}";if(fault=="quantity")f.Stored.ACCEPTED_QUANTITY=9;
        if(fault=="actor")f.Stored.ROW_CHANGED_BY="other";if(fault=="order-unit")f.Existing.OrderLine.QUANTITY_UOM="m³";
        if(fault=="inactive")f.Existing.OrderLine.ACTIVE_IND="N";
        if(fault is "consumption-unit" or "activity"){
            f.Existing.AddCompeting(1);var row=f.Existing.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single();if(fault=="consumption-unit")row.QUANTITY_UOM="m³";else row.ACTIVE_IND="?";}
        if(fault is "proof" or "quantity" or "actor")
        {
            // Corrupt authoritative proof is refused before a diagnostic snapshot can be returned.
            var before=f.Existing.Native.Writes;
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Existing.Prepare());
            Assert.Equal(before,f.Existing.Native.Writes);
            return;
        }
        await f.Existing.Prepare();Assert.Equal(APSourceComparisonOutcome.Unavailable,f.Existing.Displayed.Comparisons.Single(row=>row.Code=="acceptance").Outcome);
        var writes=f.Existing.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Existing.Record());Assert.Equal(writes,f.Existing.Native.Writes);
    }
    [Fact] public async Task ReceiptDecisionChangeInvalidatesAlreadyPreparedAllocationAndCannotBeRetriedAsAnotherIntent()
    {
        var f=new APReceiptAllocationCommandTests.Fixture();await f.Prepare();f.Receipt.RECEIVER_DECISION_JSON="{}";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualHttpPreparationAndWorkspaceExposeAcceptedAndConsumedQuantities(bool exceeded)
    {
        var f=new PurchaseReceiptDecisionTests.Fixture();await f.Prepare();await f.Record();f.Existing.AddCompeting(exceeded?7:6);
        await using var host=await APReceiptAllocationCommandHttpTests.Host(f.Existing);using var company=new CompanyClientContext();
        company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","Procurement.View","Accounting.PrepareReceiptAllocation","Accounting.RecordReceiptAllocation"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        using var state=new APInvoiceWorkspaceState(company);await state.LoadDetailAsync("invoice",client.GetAPInvoiceAsync);await state.LoadPurchaseSourceAsync("line",client.GetAPLinePurchaseSourceAsync);
        await state.LoadReceiptAllocationsAsync(client.GetAPReceiptAllocationsAsync);state.SetProposalReceipt("receipt");state.SetProposalQuantity(2m);await state.ReviewAllocationProposalAsync(client.ReviewAPReceiptAllocationAsync);
        Assert.Equal(8m,state.ReviewedProposal!.AcceptedQuantity);Assert.Equal(2m,state.ReviewedProposal.RejectedQuantity);Assert.Equal("actor",state.ReviewedProposal.DecisionActor);
        Assert.Equal(exceeded?7m:6m,state.ReviewedProposal.RecordedActiveQuantity);Assert.Equal(!exceeded,state.CanRecordAllocation);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.APInvoiceRenderHost>(
            ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state},{"AllowPurchaseSource",true},{"AllowPreparation",true},{"OnProposal",EventCallback.Factory.Create(new object(),()=>Task.CompletedTask)}}));return System.Net.WebUtility.HtmlDecode(root.ToHtmlString());});
        Assert.Contains("Verified accepted quantity: 8",html);Assert.Contains("Rejected quantity: 2",html);Assert.Contains("Receiver decision by: actor",html);
        var complete=await client.GetAPReceiptConsumptionAsync("invoice","line","receipt",state.SourceRevision!,state.LinkedOrder.SourceRevision!);
        Assert.Equal(8m,complete.VerifiedAcceptedQuantity);Assert.Equal(exceeded?7m:6m,complete.RecordedActiveQuantity);
        if(exceeded){await f.Existing.Prepare();await Assert.ThrowsAsync<InvalidOperationException>(()=>client.RecordAPReceiptAllocationAsync(new(f.Existing.Command,f.Existing.Displayed)));
            var response=await host.Client.PostAsJsonAsync("api/accounting/traditional/ap/invoice/lines/line/receipt-allocations",new APReceiptAllocationSubmission(f.Existing.Command,f.Existing.Displayed));Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);}
        else{await state.RecordAllocationAsync(client.RecordAPReceiptAllocationAsync,client.GetAPInvoiceAsync,client.GetAPLinePurchaseSourceAsync,client.GetAPReceiptAllocationsAsync,client.GetAPReceiptConsumptionAsync,client.GetAPInvoiceFinancialReviewAsync,"Reviewed accepted capacity.");Assert.NotNull(state.AllocationReceipt);Assert.Equal(8m,state.RefreshedReceiptConsumption!.RecordedActiveQuantity);}
    }
}
#endif
