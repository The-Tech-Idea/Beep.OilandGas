using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class APReceiptAllocationCommandTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        public readonly PersonaTaskRoutingTests.Fixture Native=new();
        public readonly APInvoiceService AP;
        public readonly PurchaseOrderService PO;
        public AP_INVOICE Invoice=new(){AP_INVOICE_ID="invoice",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",STATUS="DRAFT",CURRENCY_CODE="KWD",TOTAL_AMOUNT=10m};
        public AP_INVOICE_LINE Line=new(){AP_INVOICE_LINE_ID="line",AP_INVOICE_ID="invoice",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",QUANTITY=10m,QUANTITY_UOM="bbl",UNIT_PRICE=1m,LINE_TOTAL=10m,CURRENCY_CODE="KWD"};
        public PURCHASE_ORDER Order=new(){PURCHASE_ORDER_ID="order",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",VENDOR_BA_ID="supplier",CURRENCY_CODE="KWD"};
        public PO_LINE_ITEM OrderLine=new(){PO_LINE_ITEM_ID="po-line",PURCHASE_ORDER_ID="order",QUANTITY=10m,QUANTITY_UOM="bbl",UNIT_PRICE=1m};
        public PO_RECEIPT Receipt=new(){PO_RECEIPT_ID="receipt",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",RECEIVED_QUANTITY=10m,QUANTITY_UOM="bbl"};
        public APReceiptAllocationProposalReview Displayed=null!;
        public APReceiptAllocationCommand Command=null!;
        public int AuthorityReads;
        public int RevokeAt;
        public bool CorruptInsert;
        public Action? OnInsert;
        public Fixture(bool accepted=true)
        {
            Native.Connection="production";Native.Rows.Clear();Native.Rows.AddRange([Invoice,Line,Order,OrderLine,Receipt]);
            var types=new Dictionary<string,Type>{{"AP_INVOICE",typeof(AP_INVOICE)},{"AP_INVOICE_LINE",typeof(AP_INVOICE_LINE)},
                {"AP_RECEIPT_ALLOCATION",typeof(AP_RECEIPT_ALLOCATION)},{"PURCHASE_ORDER",typeof(PURCHASE_ORDER)},{"PO_LINE_ITEM",typeof(PO_LINE_ITEM)},{"PO_RECEIPT",typeof(PO_RECEIPT)}};
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure entity,bool refresh)=>
                new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(types[entity.EntityName]));
            Native.Source.Setup(source=>source.InsertEntity("AP_RECEIPT_ALLOCATION",It.IsAny<object>())).Returns((string table,object item)=>{
                Native.Writes++;var row=Clone((AP_RECEIPT_ALLOCATION)item);
                if(CorruptInsert)row.ALLOCATED_QUANTITY=99m;Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var columns=new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            var defaults=Mock.Of<IPPDM39DefaultsRepository>();var metadataReader=new Mock<IPPDMMetadataRepository>();
            metadataReader.Setup(reader=>reader.GetPrimaryKeyColumnsAsync("AP_RECEIPT_ALLOCATION")).ReturnsAsync(new List<string>{"AP_RECEIPT_ALLOCATION_ID"});
            metadataReader.Setup(reader=>reader.GetPrimaryKeyColumnsAsync("PO_RECEIPT")).ReturnsAsync(new List<string>{"PO_RECEIPT_ID"});
            metadataReader.Setup(reader=>reader.GetPrimaryKeyColumnsAsync("AP_INVOICE")).ReturnsAsync(new List<string>{"AP_INVOICE_ID"});
            metadataReader.Setup(reader=>reader.GetPrimaryKeyColumnsAsync("AP_INVOICE_LINE")).ReturnsAsync(new List<string>{"AP_INVOICE_LINE_ID"});
            var metadata=metadataReader.Object;
            Func<Task<string>> binding=()=>Task.FromResult(Native.Connection);
            var accounts=new GLAccountService(Native.Editor.Object,columns,defaults,metadata,NullLogger<GLAccountService>.Instance,binding);
            var journal=new JournalEntryService(Native.Editor.Object,columns,defaults,metadata,accounts,NullLogger<JournalEntryService>.Instance,binding);
            AP=new(Native.Editor.Object,columns,defaults,metadata,new AccountingBasisPostingService(journal),NullLogger<APInvoiceService>.Instance,resolveConnection:binding);
            PO=new(Native.Editor.Object,columns,defaults,metadata,NullLogger<PurchaseOrderService>.Instance,binding);
            if(accepted)SeedReceiverDecision();
        }
        // Explicit prior receiving evidence for allocation fixtures; receiver command tests use accepted:false.
        public void SeedReceiverDecision(decimal accepted=10m,decimal rejected=0m)
        {
            Receipt.ACCEPTED_QUANTITY=null;Receipt.REJECTED_QUANTITY=null;Receipt.RECEIVER_DECISION_JSON=null;Receipt.ROW_CHANGED_BY=null!;Receipt.ROW_CHANGED_DATE=null;
            var original=Clone(Receipt);var review=PurchaseOrderSourceReview.Create(Company,"receiver",Order,[OrderLine],[original]);
            var command=new PurchaseReceiptDecisionCommand(Company,"receiver",Order.PURCHASE_ORDER_ID,Receipt.PO_RECEIPT_ID,accepted,rejected,Receipt.QUANTITY_UOM!,review.Revision,"binding",Guid.NewGuid().ToString("N"),"Prior verified physical receiving.");
            command.Require(review);Receipt.ACCEPTED_QUANTITY=accepted;Receipt.REJECTED_QUANTITY=rejected;Receipt.ROW_CHANGED_BY="receiver";Receipt.ROW_CHANGED_DATE=DateTime.UtcNow;
            Receipt.RECEIVER_DECISION_JSON=PurchaseReceiptDecisionProof.Create(command,original,Receipt);PurchaseReceiptDecisionProof.Require(Company,Receipt);
        }
        public Task Authority()
        {
            AuthorityReads++;if(RevokeAt!=0 && AuthorityReads>=RevokeAt)throw new UnauthorizedAccessException("The original write grant was revoked.");
            if(Native.Connection!="production")throw new InvalidOperationException("Original production binding changed.");
            return Task.CompletedTask;
        }
        public async Task Prepare(decimal quantity=2m)
        {
            var invoice=(await AP.GetSourceReviewAsync(Company,"invoice","actor"))!;
            var purchase=new APLinePurchaseSourceReview(invoice,"line",(await PO.GetSourceReviewAsync(Company,"order","actor"))!);
            var allocations=APReceiptAllocationReview.Create(Company,"actor","invoice","line",invoice.Revision,purchase,await AP.GetReceiptAllocationsForLineAsync(Company,"invoice","line"));
            var consumption=await AP.GetReceiptConsumptionAsync(Company,"actor",purchase,"receipt",Authority);
            var proposal=new APReceiptAllocationProposal("receipt",quantity,"bbl",invoice.Revision,purchase.Order.Revision,allocations.Revision);
            Displayed=APReceiptAllocationProposalReview.Create(Company,"actor","invoice","line",proposal,allocations,consumption,"binding");
            Command=new(Company,"actor","invoice","line",proposal,consumption.Revision,Displayed.Revision,"binding",Guid.NewGuid().ToString("N"),"Reviewed recorded receipt quantity.");
            AuthorityReads=0;
        }
        public Task<AP_RECEIPT_ALLOCATION> Record()=>AP.RecordReceiptAllocationAsync(Command,Displayed,Authority,async()=>
            (await PO.GetSourceReviewAsync(Company,"order","actor"))!);
        public void AddCompeting(decimal quantity)
        {
            var invoice=Clone(Invoice);invoice.AP_INVOICE_ID="second";
            var line=Clone(Line);line.AP_INVOICE_ID="second";line.AP_INVOICE_LINE_ID="second-line";
            Native.Rows.AddRange([invoice,line,new AP_RECEIPT_ALLOCATION{AP_RECEIPT_ALLOCATION_ID="competitor",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",
                AP_INVOICE_ID="second",AP_INVOICE_LINE_ID="second-line",PURCHASE_ORDER_ID="order",PO_LINE_ITEM_ID="po-line",PO_RECEIPT_ID="receipt",ALLOCATED_QUANTITY=quantity,QUANTITY_UOM="bbl"}]);
        }
    }
    private static T Clone<T>(T item)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(item))!;
    [Fact] public async Task OriginalReviewedIntentCommitsAndExactRetryDoesNotAllocateTwice()
    {
        var fixture=new Fixture();await fixture.Prepare();var first=await fixture.Record();var retry=await fixture.Record();
        Assert.Equal(first.AP_RECEIPT_ALLOCATION_ID,retry.AP_RECEIPT_ALLOCATION_ID);Assert.Equal(1,fixture.Native.Writes);
        APReceiptAllocationRecordedProof.Require(fixture.Command,retry);Assert.Equal(2m,retry.ALLOCATED_QUANTITY);
        Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Native.Transactions);
        Assert.Equal("DRAFT",fixture.Invoice.STATUS);Assert.Equal(0,fixture.Native.Rows.Count(row=>row.GetType().Name=="JOURNAL_ENTRY"));
        first.ALLOCATED_QUANTITY=999m;Assert.Equal(2m,Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>()).ALLOCATED_QUANTITY);
    }
    [Theory][InlineData("invoice")][InlineData("receipt")][InlineData("competitor")][InlineData("status")][InlineData("inactive")][InlineData("unit")]
    public async Task ChangedEvidenceBeforeRecordingCannotWrite(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();
        if(fault=="invoice")fixture.Invoice.TOTAL_AMOUNT=11m;if(fault=="receipt")fixture.Receipt.RECEIVED_QUANTITY=9m;
        if(fault=="competitor")fixture.AddCompeting(1m);if(fault=="status")fixture.Invoice.STATUS="RECEIVED";
        if(fault=="inactive")fixture.OrderLine.ACTIVE_IND="N";if(fault=="unit")fixture.OrderLine.QUANTITY_UOM="m³";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Native.Writes);
    }
    [Theory][InlineData("quantity")][InlineData("reason")][InlineData("intent")][InlineData("binding")][InlineData("consumption")][InlineData("review")]
    public async Task InvalidOrReplacedOriginalCommandNeverOpensTransaction(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Command=fault switch{
            "quantity"=>fixture.Command with{Proposal=fixture.Command.Proposal with{Quantity=3m}},"reason"=>fixture.Command with{Reason=""},
            "intent"=>fixture.Command with{IntentId=Guid.Empty.ToString("N")},"binding"=>fixture.Command with{BindingRevision=""},
            "consumption"=>fixture.Command with{ExpectedConsumptionRevision=new string('a',64)},_=>fixture.Command with{ExpectedProposalRevision=new string('b',64)}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Empty(fixture.Native.Transactions);Assert.Equal(0,fixture.Native.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task QuantityLimitAndUnitMismatchRefuseEvenFreshReviewedProposal(bool unit)
    {
        var fixture=new Fixture();if(unit)fixture.OrderLine.QUANTITY_UOM="m³";else fixture.AddCompeting(9m);
        await fixture.Prepare();await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Native.Writes);
    }
    [Theory][InlineData("proof")][InlineData("quantity")][InlineData("inactive")][InlineData("actor")][InlineData("changed")][InlineData("owner")][InlineData("reason")]
    public async Task AlteredRecordedOutcomeCannotBeRetriedOrRepaired(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();var row=Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        if(fault=="proof")row.REVIEW_COMMAND_JSON="{}";if(fault=="quantity")row.ALLOCATED_QUANTITY=3;
        if(fault=="inactive")row.ACTIVE_IND="N";if(fault=="actor")row.ROW_CREATED_BY="other";
        if(fault=="changed")row.ROW_CHANGED_DATE=DateTime.UtcNow;if(fault=="owner")row.OWNER_COMPANY_BA_ID="foreign";if(fault=="reason")row.REMARK="other";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task ClosingCompetingConsumptionOrCorruptInsertRollsBack(bool corrupt)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.CorruptInsert=corrupt;if(!corrupt)fixture.OnInsert=()=>fixture.AddCompeting(1m);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Empty(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        Assert.Equal(new[]{"begin","rollback"},fixture.Native.Transactions);
    }
    [Fact] public async Task UnconfirmedCommitRetainsOriginalOutcomeAndExactRetryReadsIt()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Native.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        fixture.Native.FailCommit=false;await fixture.Record();Assert.Equal(1,fixture.Native.Writes);Assert.DoesNotContain("rollback",fixture.Native.Transactions);
    }
    [Fact] public async Task ReadCommittedCannotAuthorizeRecording()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Native.Writes);
    }
    [Theory][InlineData(1)][InlineData(3)][InlineData(10)][InlineData(11)][InlineData(17)] public async Task RevocationNeverConfirmsAnAllocation(int at)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.RevokeAt=at;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Record());Assert.Empty(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
    }
    [Theory][InlineData("missing")][InlineData("size")][InlineData("duplicate")]
    public async Task UnmigratedProofSchemaRefusesBeforeWrites(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();var schema=new ClassCreator(fixture.Native.Editor.Object).ConvertToEntityStructure(typeof(AP_RECEIPT_ALLOCATION));
        var proof=schema.Fields.Single(field=>field.FieldName=="REVIEW_COMMAND_JSON");
        if(fault=="missing")schema.Fields.Remove(proof);if(fault=="size")proof.Size=100;if(fault=="duplicate")schema.Fields.Add(proof);
        fixture.Native.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(entity=>entity.EntityName=="AP_RECEIPT_ALLOCATION"),true)).Returns(schema);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Native.Writes);
    }
    [Fact] public async Task ProofColumnIsCanonicalBoundedBeepMetadataAndContentRevisionIncludesIt()
    {
        var fixture=new Fixture();await fixture.Prepare();var schema=new ClassCreator(fixture.Native.Editor.Object).ConvertToEntityStructure(typeof(AP_RECEIPT_ALLOCATION));
        Assert.Equal(4000,schema.Fields.Single(field=>field.FieldName=="REVIEW_COMMAND_JSON").Size);
        var row=await fixture.Record();var review=APReceiptAllocationReview.Create(Company,"actor","invoice","line",fixture.Command.Proposal.ExpectedInvoiceRevision,fixture.Displayed.Source.Source,[row]);
        review.Allocations[0].REVIEW_COMMAND_JSON="{}";
        Assert.Throws<InvalidOperationException>(()=>review.Require(Company,"actor","invoice","line",fixture.Command.Proposal.ExpectedInvoiceRevision));
    }
    [Fact] public async Task OriginalConsumptionIncludesOtherInvoicesAndRecordingPreservesFinancialDifferences()
    {
        var fixture=new Fixture();fixture.AddCompeting(7m);fixture.Line.UNIT_PRICE=2m;fixture.Invoice.CURRENCY_CODE="USD";
        await fixture.Prepare();Assert.Equal(7m,fixture.Displayed.ReceiptConsumption!.RecordedActiveQuantity);
        Assert.Equal(APSourceComparisonOutcome.RecordedDifference,fixture.Displayed.Comparisons.Single(row=>row.Code=="currencies").Outcome);
        await fixture.Record();Assert.Equal(9m,fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Sum(row=>row.ALLOCATED_QUANTITY));
        Assert.Equal("DRAFT",fixture.Invoice.STATUS);Assert.Equal("USD",fixture.Invoice.CURRENCY_CODE);
    }
    [Fact] public async Task OriginalRetryAfterAnotherContributorChangedCannotReturnConfirmation()
    {
        var fixture=new Fixture();fixture.AddCompeting(1m);await fixture.Prepare();await fixture.Record();
        fixture.Native.Rows.OfType<AP_INVOICE>().Single(row=>row.AP_INVOICE_ID=="second").TOTAL_AMOUNT=12m;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task SameIntentWithChangedReasonCannotBecomeAnotherWrite()
    {
        var fixture=new Fixture();await fixture.Prepare();await fixture.Record();fixture.Command=fixture.Command with{Reason="Different request."};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task BindingChangeImmediatelyAfterInsertRollsBack()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.OnInsert=()=>fixture.Native.Connection="changed";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Empty(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());
        Assert.Equal(new[]{"begin","rollback"},fixture.Native.Transactions);
    }
    [Fact] public async Task BorrowedTransactionCannotRecordOrCommit()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Native.Borrowed=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Fact] public async Task SuccessfulCommitFlagWithoutDurableRowCannotReturnSuccess()
    {
        var fixture=new Fixture();await fixture.Prepare();
        fixture.Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
            fixture.Native.Transactions.LastOrDefault()=="commit" && table=="AP_RECEIPT_ALLOCATION"?Array.Empty<object>():
            fixture.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue))
                .Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);Assert.Equal(1,fixture.Native.Writes);
    }
    [Fact] public async Task AuthorityLossAfterCommitReturnsNoSuccessAndOriginalRetryAddsNoAllocation()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.RevokeAt=19;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Record());Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);
        Assert.Single(fixture.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>());fixture.RevokeAt=0;
        await fixture.Record();Assert.Equal(1,fixture.Native.Writes);
    }
}
#endif
