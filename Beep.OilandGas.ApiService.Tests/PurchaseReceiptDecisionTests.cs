using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class PurchaseReceiptDecisionTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public APReceiptAllocationCommandTests.Fixture Existing=new(accepted:false);
        public PurchaseOrderSourceReview Review=null!;
        public PurchaseReceiptDecisionCommand Command=null!;
        public bool Corrupt,FailWrite;
        public Action? OnUpdate;
        public Fixture()
        {
            Existing.Native.Source.Setup(source=>source.UpdateEntity("PO_RECEIPT",It.IsAny<object>())).Returns((string table,object value)=>{
                Existing.Native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};
                var row=PurchaseReceiptDecisionProof.Copy((PO_RECEIPT)value);if(Corrupt)row.RECEIVED_QUANTITY=11m;
                Existing.Native.Rows.RemoveAll(item=>item is PO_RECEIPT receipt && receipt.PO_RECEIPT_ID==row.PO_RECEIPT_ID);
                Existing.Native.Rows.Add(row);OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public async Task Prepare(decimal accepted=8m,decimal rejected=2m,string orderId="order",string receiptId="receipt",string actor="actor")
        {
            Review=(await Existing.PO.GetSourceReviewAsync(Company,orderId,actor))!;
            Command=new(Company,actor,orderId,receiptId,accepted,rejected,"bbl",Review.Revision,"binding",Guid.NewGuid().ToString("N"),"Inspected receipt: damaged portion rejected.");
        }
        public Task<PO_RECEIPT> Record()=>Existing.PO.RecordReceiptDecisionAsync(Command,Review,Existing.Authority);
        public PO_RECEIPT Stored=>Assert.Single(Existing.Native.Rows.OfType<PO_RECEIPT>());
    }
    [Theory][InlineData(10,0)][InlineData(8,2)][InlineData(0,10)]
    public async Task FinalReceivingDecisionPreservesPhysicalEvidenceAndRetriesExactly(int accepted,int rejected)
    {
        var f=new Fixture();await f.Prepare(accepted,rejected);var row=await f.Record();var retry=await f.Record();
        Assert.Equal(accepted,row.ACCEPTED_QUANTITY);Assert.Equal(rejected,row.REJECTED_QUANTITY);Assert.Equal(10m,row.RECEIVED_QUANTITY);
        Assert.Equal("actor",row.ROW_CHANGED_BY);Assert.Equal(f.Command,PurchaseReceiptDecisionProof.Require(Company,retry).Command);
        Assert.Equal(1,f.Existing.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","commit"},f.Existing.Native.Transactions);
        Assert.Equal("DRAFT",f.Existing.Invoice.STATUS);Assert.Empty(f.Existing.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        row.ACCEPTED_QUANTITY=999;Assert.Equal(accepted,f.Stored.ACCEPTED_QUANTITY);
        var refreshed=(await f.Existing.PO.GetSourceReviewAsync(Company,"order","actor"))!;Assert.NotEqual(f.Review.Revision,refreshed.Revision);
    }
    [Theory][InlineData(-1,11)][InlineData(1,-1)][InlineData(9,0)][InlineData(11,0)][InlineData(0,0)]
    public async Task InvalidFinalPartitionNeverOpensTransaction(int accepted,int rejected)
    {
        var f=new Fixture();await f.Prepare(accepted,rejected);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());
        Assert.Empty(f.Existing.Native.Transactions);Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Theory][InlineData("receipt")][InlineData("line")][InlineData("header")][InlineData("unit")][InlineData("inactive")][InlineData("owner")]
    public async Task ChangedReviewedOrderRefusesWithoutWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();switch(fault){case "receipt":f.Existing.Receipt.DESCRIPTION="changed";break;
            case "line":f.Existing.OrderLine.QUANTITY=11;break;case "header":f.Existing.Order.DESCRIPTION="changed";break;
            case "unit":f.Existing.Receipt.QUANTITY_UOM="m³";break;case "inactive":f.Existing.OrderLine.ACTIVE_IND="N";break;
            case "owner":f.Existing.Order.OWNER_COMPANY_BA_ID="foreign";break;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Theory][InlineData("accepted")][InlineData("reason")][InlineData("intent")][InlineData("actor")][InlineData("proof")][InlineData("received")]
    public async Task AlteredOriginalIntentOrStoredDecisionCannotBeRepaired(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.Record();switch(fault){case "accepted":f.Command=f.Command with{AcceptedQuantity=7,RejectedQuantity=3};break;
            case "reason":f.Command=f.Command with{Reason="Different"};break;case "intent":f.Command=f.Command with{IntentId=Guid.NewGuid().ToString("N")};break;
            case "actor":f.Stored.ROW_CHANGED_BY="other";break;case "proof":f.Stored.RECEIVER_DECISION_JSON="{}";break;case "received":f.Stored.RECEIVED_QUANTITY=11;break;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,f.Existing.Native.Writes);
    }
    [Theory][InlineData("corrupt")][InlineData("write")][InlineData("binding")][InlineData("closing")][InlineData("authority")]
    public async Task FailedOrChangedWriteRollsBackDecision(string fault)
    {
        var f=new Fixture();await f.Prepare();f.Corrupt=fault=="corrupt";f.FailWrite=fault=="write";
        if(fault=="binding")f.OnUpdate=()=>f.Existing.Native.Connection="changed";
        if(fault=="closing")f.OnUpdate=()=>f.Existing.Order.DESCRIPTION="changed";
        if(fault=="authority")f.OnUpdate=()=>f.Existing.RevokeAt=f.Existing.AuthorityReads+1;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Null(f.Stored.RECEIVER_DECISION_JSON);Assert.Null(f.Stored.ACCEPTED_QUANTITY);
        Assert.Equal(new[]{"begin","rollback"},f.Existing.Native.Transactions);
    }
    [Fact] public async Task UnknownCommitRequiresOriginalRetryAndDoesNotApplyDecisionTwice()
    {
        var f=new Fixture();await f.Prepare();f.Existing.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());
        Assert.NotNull(f.Stored.RECEIVER_DECISION_JSON);f.Existing.Native.FailCommit=false;await f.Record();Assert.Equal(1,f.Existing.Native.Writes);
    }
    [Theory][InlineData(true)][InlineData(false)] public async Task BorrowedOrInsufficientTransactionCannotWrite(bool borrowed)
    {
        var f=new Fixture();await f.Prepare();f.Existing.Native.Borrowed=borrowed;if(!borrowed)f.Existing.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("size")][InlineData("duplicate")][InlineData("nullable")][InlineData("type")]
    public async Task UnmigratedDecisionColumnsCannotWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();var schema=new ClassCreator(f.Existing.Native.Editor.Object).ConvertToEntityStructure(typeof(PO_RECEIPT));
        var field=schema.Fields.Single(item=>item.FieldName=="RECEIVER_DECISION_JSON");
        if(fault=="missing")schema.Fields.Remove(field);if(fault=="size")field.Size=100;if(fault=="duplicate")schema.Fields.Add(field);
        if(fault=="nullable")field.AllowDBNull=false;if(fault=="type")field.Fieldtype=typeof(int).FullName;
        f.Existing.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(item=>item.EntityName=="PO_RECEIPT"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Fact] public async Task DuplicateReceiptKeyCannotBeHiddenByOrderOwnership()
    {
        var f=new Fixture();await f.Prepare();var foreign=PurchaseReceiptDecisionProof.Copy(f.Existing.Receipt);foreign.PURCHASE_ORDER_ID="foreign";f.Existing.Native.Rows.Add(foreign);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Existing.Native.Writes);
    }
    [Fact] public async Task UnconfirmedPostCommitReadCannotReturnSuccess()
    {
        var f=new Fixture();await f.Prepare();f.Existing.Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
            f.Existing.Native.Transactions.LastOrDefault()=="commit" && table=="PO_RECEIPT"?Array.Empty<object>():
            f.Existing.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue))
                .Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(new[]{"begin","commit"},f.Existing.Native.Transactions);
    }
    [Theory][InlineData("within")][InlineData("exceeded")][InlineData("tampered")][InlineData("missing")][InlineData("unit")][InlineData("inactive-order")][InlineData("inactive-line")]
    public async Task ActualReceiverDecisionFeedsAccountantEvidenceWithoutFinancialApproval(string fault)
    {
        var f=new Fixture();
        f.Existing.Native.Rows.Add(new AP_RECEIPT_ALLOCATION{AP_RECEIPT_ALLOCATION_ID="legacy-allocation",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",AP_INVOICE_ID="invoice",AP_INVOICE_LINE_ID="line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=2m,QUANTITY_UOM="bbl"});
        await f.Prepare(fault=="exceeded"?1m:8m,fault=="exceeded"?9m:2m);
        if(fault!="missing")await f.Record();if(fault=="tampered")f.Stored.ACCEPTED_QUANTITY=9;
        if(fault=="unit")f.Existing.OrderLine.QUANTITY_UOM="m³";if(fault=="inactive-order")f.Existing.Order.ACTIVE_IND="N";if(fault=="inactive-line")f.Existing.OrderLine.ACTIVE_IND="N";
        var invoice=(await f.Existing.AP.GetSourceReviewAsync(Company,"invoice","actor"))!;
        var purchase=new APLinePurchaseSourceReview(invoice,"line",(await f.Existing.PO.GetSourceReviewAsync(Company,"order","actor"))!);
        var allocations=APReceiptAllocationReview.Create(Company,"actor","invoice","line",invoice.Revision,purchase,
            await f.Existing.AP.GetReceiptAllocationsForLineAsync(Company,"invoice","line"));
        var financial=APInvoiceFinancialReview.Create(invoice,[allocations]);
        Assert.Equal(fault is "tampered" or "missing" or "unit" or "inactive-order" or "inactive-line"?APSourceComparisonOutcome.Unavailable:fault=="exceeded"?APSourceComparisonOutcome.RecordedDifference:APSourceComparisonOutcome.WithinRecordedLimit,
            financial.Checks.Single(row=>row.Code=="acceptance").Outcome);
        Assert.Contains("other invoices",financial.Checks.Single(row=>row.Code=="acceptance").Evidence);
        foreach(var code in new[]{"policy","ledger","decision"})Assert.Equal(APSourceComparisonOutcome.Unavailable,financial.Checks.Single(row=>row.Code==code).Outcome);
        Assert.Equal("DRAFT",invoice.Invoice.STATUS);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ActualProcurementHistoryDisplaysOnlyVerifiedReceiverEvidence(bool tampered)
    {
        var f=new Fixture();await f.Prepare();await f.Record();if(tampered)f.Stored.RECEIVER_DECISION_JSON="{}";
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(Company,[],[]));
        using var state=new Beep.OilandGas.Web.Services.PurchaseOrderWorkspaceState(company);
        await state.LoadDetailAsync("order",id=>f.Existing.PO.GetSourceReviewAsync(Company,id,"actor"));
        var services=new Microsoft.Extensions.DependencyInjection.ServiceCollection();services.AddLogging();services.AddMudServices();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        await using var provider=services.BuildServiceProvider();
        await using var renderer=new Microsoft.AspNetCore.Components.Web.HtmlRenderer(provider,provider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>());
        var html=await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<Beep.OilandGas.Web.Auth.Tests.Fixtures.PurchaseOrderRenderHost>(
            Microsoft.AspNetCore.Components.ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",state}}));return root.ToHtmlString();});
        if(tampered){Assert.Contains("Receiver decision unavailable or unconfirmed",html);Assert.DoesNotContain("Verified accepted:",html);Assert.Null(state.Receipts![0].Decision);}
        else{Assert.Contains("Verified accepted: 8",html);Assert.Contains("Rejected: 2",html);Assert.Contains("Decision by: actor",html);Assert.Contains("damaged portion rejected",html);Assert.Equal(8m,state.Receipts![0].Decision!.Accepted);}
    }
    [Theory][InlineData("reason")][InlineData("actor")][InlineData("intent")][InlineData("overflow")]
    public async Task InvalidReceiverIntentCannotStart(string fault)
    {
        var f=new Fixture();await f.Prepare();f.Command=fault switch{"reason"=>f.Command with{Reason="bad\nreason"},"actor"=>f.Command with{ActorUserId="SYSTEM"},
            "intent"=>f.Command with{IntentId=Guid.Empty.ToString("N")},_=>f.Command with{AcceptedQuantity=decimal.MaxValue,RejectedQuantity=1}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Empty(f.Existing.Native.Transactions);
    }
    [Fact] public async Task ChangedOtherSourceAfterCommitCannotReturnOriginalRetryConfirmation()
    {
        var f=new Fixture();await f.Prepare();await f.Record();f.Existing.OrderLine.ITEM_DESCRIPTION="Changed after receiving";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,f.Existing.Native.Writes);
    }
    [Fact] public async Task ReceiverAuthorityLossBeforeStartCannotWrite()
    {
        var f=new Fixture();await f.Prepare();f.Existing.RevokeAt=1;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.Empty(f.Existing.Native.Transactions);
    }
}
#endif
