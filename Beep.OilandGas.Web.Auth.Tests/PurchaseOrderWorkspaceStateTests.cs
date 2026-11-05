using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Web.Services;
using Xunit;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class PurchaseOrderWorkspaceStateTests
{
    private static CompanyClientContext Company(){var company=new CompanyClientContext();Select(company,"company");return company;}
    private static void Select(CompanyClientContext company,string id)=>Assert.True(company.TrySelect(company.Clear(),"buyer",new(new("tenant",id),["ConfiguredBuyer"],["Procurement.View"])));
    private static PURCHASE_ORDER Order()=>new(){PURCHASE_ORDER_ID="order/a?b",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",ACTIVE_IND="Y",PO_NUMBER="PO-1",STATUS="DRAFT",TOTAL_AMOUNT=12m};
    [Fact] public async Task VendorRowsAreFrozenAndOpenEncodedOriginalHeader()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var order=Order();await state.LoadVendorAsync("supplier",_=>Task.FromResult(new List<PURCHASE_ORDER>{order}));
        order.PO_NUMBER="changed";var row=Assert.Single(state.Rows!);Assert.Equal("PO-1",row.Number);Assert.Equal("/procurement/orders/order%2Fa%3Fb",row.Route);Assert.Equal(12m,row.RecordedAmount);Assert.NotNull(state.RetrievedAtUtc);
    }
    [Theory][InlineData("owner")][InlineData("supplier")][InlineData("duplicate")][InlineData("missing")][InlineData("inactive")]
    public async Task MissingOrWrongScopeRowsCannotRemainVisible(string fault)
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var order=Order();
        if(fault=="inactive")order.ACTIVE_IND="N";if(fault=="owner")order.OWNER_COMPANY_BA_ID="other";if(fault=="supplier")order.VENDOR_BA_ID="another";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadVendorAsync("supplier",_=>Task.FromResult(fault=="missing"?null!:fault=="duplicate"?new List<PURCHASE_ORDER>{order,order}:new List<PURCHASE_ORDER>{order})));
        Assert.Null(state.Rows);Assert.Null(state.Detail);
    }
    [Fact] public async Task FailedReloadClearsPreviousHeader()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);await state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",Order(),[],[])));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadDetailAsync("order/a?b",_=>Task.FromException<PurchaseOrderSourceReview?>(new HttpRequestException())));Assert.Null(state.Detail);
    }
    [Fact] public async Task LateOldCompanyDetailCannotPopulateNewCompany()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var pending=new TaskCompletionSource<PurchaseOrderSourceReview?>();var load=state.LoadDetailAsync("order/a?b",_=>pending.Task);
        Select(company,"other");pending.SetResult(PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",Order(),[],[]));await load;Assert.Null(state.Detail);
    }
    [Fact] public async Task ChangedSupplierInvalidationRefusesLateRows()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var pending=new TaskCompletionSource<List<PURCHASE_ORDER>>();var load=state.LoadVendorAsync("supplier",_=>pending.Task);
        state.Clear();pending.SetResult([Order()]);await load;Assert.Null(state.Rows);
    }
    [Fact] public async Task EmptyConfirmedResultDoesNotInventUnassignedOrders()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);await state.LoadVendorAsync("supplier",_=>Task.FromResult(new List<PURCHASE_ORDER>()));Assert.Empty(state.Rows!);
    }
    private static PurchaseOrderSourceReview SourceReview()=>PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",Order(),
        [new(){PO_LINE_ITEM_ID="line",PURCHASE_ORDER_ID="order/a?b",LINE_NUMBER=3,QUANTITY=12m,UNIT_PRICE=null}],
        [new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order/a?b",PO_LINE_ITEM_ID="line",RECEIVED_QUANTITY=null,RECEIVED_BY="receiver",ACTIVE_IND="N"}]);
    [Fact] public async Task DetailFreezesLineAndReceiptProvenanceAndUsesReturnedRetrievalTime()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var source=SourceReview();
        await state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(source));source.Lines[0].QUANTITY=99m;source.Receipts[0].RECEIVED_BY="another";
        Assert.Equal(12m,Assert.Single(state.Lines!).Quantity);Assert.Null(state.Lines![0].UnitPrice);
        var receipt=Assert.Single(state.Receipts!);Assert.Equal(3,receipt.LineNumber);Assert.Null(receipt.Quantity);Assert.Equal("receiver",receipt.ReceivedBy);Assert.Equal("N",receipt.ActiveIndicator);Assert.Equal(source.RetrievedAtUtc,state.RetrievedAtUtc);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("changed")][InlineData("missing")]
    public async Task UnconfirmedCompoundSourceCannotPublishAnyPreviousChildEvidence(string fault)
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var review=SourceReview();await state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(review));
        if(fault=="company")review=review with{Company=new("tenant","another")};if(fault=="actor")review=review with{ActorUserId="another"};if(fault=="changed")review.Lines[0].QUANTITY=99m;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(fault=="missing"?null:review)));
        Assert.Null(state.Detail);Assert.Null(state.Lines);Assert.Null(state.Receipts);Assert.Null(state.SourceRevision);
    }
    [Fact] public async Task DetailPreservesDistinctRecordedCurrencyAndLineReceiptUnits()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var initial=SourceReview();
        initial.Header.CURRENCY_CODE="KWD";initial.Lines[0].QUANTITY_UOM="m³";initial.Receipts[0].QUANTITY_UOM="bbl";
        var review=PurchaseOrderSourceReview.Create(initial.Company,initial.ActorUserId,initial.Header,initial.Lines,initial.Receipts);
        await state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(review));
        review.Header.CURRENCY_CODE="USD";review.Lines[0].QUANTITY_UOM="L";review.Receipts[0].QUANTITY_UOM="L";
        Assert.Equal("KWD",state.Detail!.CurrencyCode);Assert.Equal("m³",Assert.Single(state.Lines!).QuantityUnit);Assert.Equal("bbl",Assert.Single(state.Receipts!).QuantityUnit);
    }
    [Fact] public async Task CompanyQueueRetainsActualStatusesAndFreezesExactFilteredLinks()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var order=Order();var second=Order();second.PURCHASE_ORDER_ID="other";second.VENDOR_BA_ID="another";second.STATUS=null;
        await state.LoadCompanyAsync(vendor=>{Assert.Null(vendor);return Task.FromResult(new List<PURCHASE_ORDER>{second,order});});
        Assert.Equal(2,state.VisibleRows!.Count);Assert.Equal("DRAFT",Assert.Single(state.RecordedStatuses));order.STATUS="APPROVED";
        state.SetStatusFilter("DRAFT");Assert.Equal("/procurement/orders/order%2Fa%3Fb",Assert.Single(state.VisibleRows!).Route);
        Assert.Throws<InvalidOperationException>(()=>state.SetStatusFilter("APPROVED"));state.SetStatusFilter(null);Assert.Equal(2,state.VisibleRows!.Count);
        company.Clear();Assert.Null(state.Rows);Assert.Empty(state.RecordedStatuses);Assert.Null(state.StatusFilter);
    }
    [Fact] public async Task CompanyQueueSwitchAwayAndBackDiscardsLateRead()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);var pending=new TaskCompletionSource<List<PURCHASE_ORDER>>();
        var load=state.LoadCompanyAsync(_=>pending.Task);Select(company,"other");Select(company,"company");pending.SetResult([Order()]);await load;Assert.Null(state.Rows);
    }
    [Fact] public async Task FailedCompanyQueueReloadClearsRecordsAndFilter()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);await state.LoadCompanyAsync(_=>Task.FromResult(new List<PURCHASE_ORDER>{Order()}));state.SetStatusFilter("DRAFT");
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadCompanyAsync(_=>throw new HttpRequestException()));Assert.Null(state.Rows);Assert.Null(state.StatusFilter);Assert.Null(state.RetrievedAtUtc);
    }
    [Fact] public async Task ReceivingQueuesRequireVerifiedProofAndRetainMixedDecisionInBothQueues()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        var original=new PO_RECEIPT{PO_RECEIPT_ID="mixed",PURCHASE_ORDER_ID="order/a?b",PO_LINE_ITEM_ID="line",RECEIVED_QUANTITY=5m,QUANTITY_UOM="ea",ACTIVE_IND="Y"};
        var command=new PurchaseReceiptDecisionCommand(new("tenant","company"),"receiver",original.PURCHASE_ORDER_ID,original.PO_RECEIPT_ID,3m,2m,"ea",new string('a',64),"binding",Guid.NewGuid().ToString("N"),"Damaged goods separated");
        var recorded=PurchaseReceiptDecisionProof.Copy(original);recorded.ACCEPTED_QUANTITY=3m;recorded.REJECTED_QUANTITY=2m;recorded.ROW_CHANGED_BY="receiver";recorded.ROW_CHANGED_DATE=DateTime.UtcNow;
        recorded.RECEIVER_DECISION_JSON=PurchaseReceiptDecisionProof.Create(command,original,recorded);
        var pending=PurchaseReceiptDecisionProof.Copy(original);pending.PO_RECEIPT_ID="pending";
        var unproven=PurchaseReceiptDecisionProof.Copy(original);unproven.PO_RECEIPT_ID="unproven";unproven.ACCEPTED_QUANTITY=5m;unproven.RECEIVER_DECISION_JSON="{}";
        var inactive=PurchaseReceiptDecisionProof.Copy(original);inactive.PO_RECEIPT_ID="inactive";inactive.ACTIVE_IND="N";
        var source=PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",Order(),[new(){PO_LINE_ITEM_ID="line",PURCHASE_ORDER_ID=original.PURCHASE_ORDER_ID}], [recorded,pending,unproven,inactive]);
        await state.LoadDetailAsync(original.PURCHASE_ORDER_ID,_=>Task.FromResult<PurchaseOrderSourceReview?>(source));
        Assert.Equal(4,state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.All));
        Assert.Equal(2,state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.NeedsDecision));
        Assert.Equal(1,state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.InactiveOrUnconfirmed));
        foreach(var queue in new[]{PurchaseOrderWorkspaceState.ReceivingQueue.Accepted,PurchaseOrderWorkspaceState.ReceivingQueue.Rejected})
        {state.SetReceiptFilter(queue);Assert.Equal(1,state.ReceiptCount(queue));Assert.Equal("mixed",Assert.Single(state.VisibleReceipts!).Id);}
        recorded.ACCEPTED_QUANTITY=99m;Assert.Equal(3m,Assert.Single(state.VisibleReceipts!).Decision!.Accepted);
        state.SetReceiptFilter(PurchaseOrderWorkspaceState.ReceivingQueue.NeedsDecision);Assert.Equal(new[]{"pending","unproven"},state.VisibleReceipts!.Select(row=>row.Id));
        company.Clear();Assert.Null(state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.All));Assert.Null(state.VisibleReceipts);Assert.Equal(PurchaseOrderWorkspaceState.ReceivingQueue.All,state.ReceiptFilter);
    }
    [Fact] public async Task EmptyReceivingEvidenceAndUnavailableEvidenceRemainDistinct()
    {
        using var company=Company();using var state=new PurchaseOrderWorkspaceState(company);
        Assert.Null(state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.All));
        await state.LoadDetailAsync("order/a?b",_=>Task.FromResult<PurchaseOrderSourceReview?>(PurchaseOrderSourceReview.Create(new("tenant","company"),"buyer",Order(),[],[])));
        Assert.Equal(0,state.ReceiptCount(PurchaseOrderWorkspaceState.ReceivingQueue.NeedsDecision));
        Assert.Throws<ArgumentOutOfRangeException>(()=>state.SetReceiptFilter((PurchaseOrderWorkspaceState.ReceivingQueue)999));
        state.SetReceiptFilter(PurchaseOrderWorkspaceState.ReceivingQueue.Rejected);
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadDetailAsync("order/a?b",_=>throw new HttpRequestException()));
        Assert.Null(state.VisibleReceipts);Assert.Equal(PurchaseOrderWorkspaceState.ReceivingQueue.All,state.ReceiptFilter);
    }
}
