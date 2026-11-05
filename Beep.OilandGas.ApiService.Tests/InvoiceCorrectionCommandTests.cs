using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceCorrectionCommandTests
{
    internal sealed class Fixture
    {
        public readonly InvoiceVarianceDecisionCommandTests.Fixture Existing;
        public Fixture(InvoiceVarianceDecisionCommandTests.Fixture? existing=null){Existing=existing??new();}
        public APInvoiceCorrectionReview Review=null!;public APInvoiceCorrectionCommand Command=null!;
        public bool FailLine,CorruptLine;public Action? OnLine;
        public async Task Prepare(bool alreadyPrepared=false)
        {
            if(!alreadyPrepared){await Existing.Prepare(within:false);await Existing.Record();}var n=Existing.Existing.Existing.ReadOwner.Native.Native;n.Transactions.Clear();
            n.Source.Setup(source=>source.UpdateEntity("AP_INVOICE_LINE",It.IsAny<object>())).Returns((string table,object value)=>
            {
                n.Writes++;if(FailLine)return new ErrorsInfo{Flag=Errors.Failed};var line=PurchaseReceiptDecisionProof.Copy((AP_INVOICE_LINE)value);if(CorruptLine)line.UNIT_PRICE=99m;
                n.Rows.RemoveAll(row=>row is AP_INVOICE_LINE previous && previous.AP_INVOICE_LINE_ID==line.AP_INVOICE_LINE_ID);n.Rows.Add(line);OnLine?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
            await PrepareNext(3m);
        }
        public async Task PrepareNext(decimal total)
        {
            var financial=await InvoiceVarianceDecisionReviewTests.Financial(Existing.Existing,"actor");var row=financial.Invoice.Invoice;var prior=row.CORRECTION_HISTORY_JSON;
            var count=prior is null?0:APInvoiceCorrectionHistory.Require(Existing.Command.Company,row,financial.Invoice.Lines).Versions.Count;
            var draft=new APInvoiceCorrectionDraft(financial.Invoice.Revision,financial.Revision,Existing.Command.IntentId,APInvoiceCorrectionReview.DecisionRevision(row.VARIANCE_DECISION_JSON!),total,"Exact billed correction against controller request.",[new("line",total,1m,total,"Corrected billed line")]);
            Review=APInvoiceCorrectionReview.Create(financial,draft,Existing.Command.BindingRevision);
            Command=new(Existing.Command.Company,"actor","invoice",draft,Review.Revision,Review.BindingRevision,count+1,prior is null?null:ProcessDefinitionRevision.FromJson(prior),Guid.NewGuid().ToString("N"));
        }
        public Task<APInvoiceCorrectionResult> Record()=>Existing.Existing.Existing.ReadOwner.Native.AP.RecordCorrectionAsync(Command,Review,Existing.Existing.Existing.ReadOwner.Native.Authority,()=>InvoiceVarianceDecisionReviewTests.Financial(Existing.Existing,"actor"));
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task NativeCorrectionRecordsExactHeaderLinesHistoryAndRetryWithoutExtraWrites(bool next)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;var proposal=f.Review.Current.Invoice.Invoice.VARIANCE_PROPOSAL_JSON;var request=f.Review.Current.Invoice.Invoice.VARIANCE_DECISION_JSON;
        var result=await f.Record();result.Require(new(f.Command,f.Review));Assert.Equal(4,n.Writes);Assert.Equal(3m,result.Invoice.TOTAL_AMOUNT);Assert.Equal(3m,Assert.Single(result.Lines).LINE_TOTAL);Assert.Equal("DRAFT",result.Invoice.STATUS);
        Assert.Equal(proposal,result.Invoice.VARIANCE_PROPOSAL_JSON);Assert.Equal(request,result.Invoice.VARIANCE_DECISION_JSON);Assert.Equal("source-maker",result.Invoice.ROW_CREATED_BY);Assert.Equal("actor",result.Invoice.ROW_CHANGED_BY);
        Assert.Equal(result.Invoice.ROW_CHANGED_DATE,result.Lines[0].ROW_CHANGED_DATE);Assert.Single(APInvoiceCorrectionHistory.Require(f.Command.Company,result.Invoice,result.Lines).Versions);
        var retry=await f.Record();Assert.Equal(result.Invoice.CORRECTION_HISTORY_JSON,retry.Invoice.CORRECTION_HISTORY_JSON);Assert.Equal(4,n.Writes);
        var current=await InvoiceVarianceDecisionReviewTests.Financial(f.Existing.Existing,"actor");Assert.Equal(f.Review.Current.Revision,current.OriginalCorrectionEvidence(f.Command).Revision);Assert.NotEqual(current.Revision,f.Review.Current.Revision);
        Assert.Equal(f.Review.Current.Invoice.ForCompanyWorkflow().Revision,current.Invoice.ForCompanyWorkflow().Revision);
        if(next){await f.PrepareNext(4m);var second=await f.Record();second.Require(new(f.Command,f.Review));Assert.Equal(6,n.Writes);Assert.Equal(2,APInvoiceCorrectionHistory.Require(f.Command.Company,second.Invoice,second.Lines).Versions.Count);await f.Record();Assert.Equal(6,n.Writes);}
    }
    [Theory][InlineData("intent")][InlineData("actor")][InlineData("review")][InlineData("binding")][InlineData("version")][InlineData("predecessor")][InlineData("draft")]
    public async Task InvalidOrChangedSubmissionCannotOpenTransaction(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;
        f.Command=fault switch{"intent"=>f.Command with{IntentId="invalid"},"actor"=>f.Command with{ActorUserId="another"},"review"=>f.Command with{ExpectedReviewRevision=new string('C',64)},"binding"=>f.Command with{BindingRevision=new string('C',64)},"version"=>f.Command with{Version=2,PreviousHistoryRevision=new string('C',64)},"predecessor"=>f.Command with{PreviousHistoryRevision=new string('C',64)},_=>f.Command with{Draft=f.Command.Draft with{TotalAmount=99m}}};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Empty(n.Transactions);Assert.Equal(2,n.Writes);
    }
    [Theory][InlineData("invoice")][InlineData("line")][InlineData("purchase")][InlineData("allocation")][InlineData("policy")][InlineData("receiver")][InlineData("foreign")][InlineData("duplicate-invoice")][InlineData("duplicate-line")][InlineData("foreign-line-id")]
    public async Task FullFreshEvidenceAndGlobalRowIdentitiesMustMatchBeforeAnyCorrectionWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;
        if(fault=="invoice")f.Existing.Existing.Stored.TOTAL_AMOUNT=99m;if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="purchase")n.OrderLine.UNIT_PRICE=99m;
        if(fault=="allocation")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="policy")f.Existing.Existing.Existing.Stored.DESCRIPTION="changed";if(fault=="receiver")n.Receipt.RECEIVER_DECISION_JSON=null;if(fault=="foreign")f.Existing.Existing.Stored.OWNER_COMPANY_BA_ID="foreign";
        if(fault=="duplicate-invoice")n.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Existing.Existing.Stored));if(fault is "duplicate-line" or "foreign-line-id"){var line=PurchaseReceiptDecisionProof.Copy(n.Line);if(fault=="foreign-line-id")line.AP_INVOICE_ID="other-invoice";n.Native.Rows.Add(line);}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Native.Writes);Assert.Equal(new[]{"begin","rollback"},n.Native.Transactions);
    }
    [Theory][InlineData("missing")][InlineData("type")][InlineData("size")][InlineData("nullable")][InlineData("line-type")][InlineData("header-type")]
    public async Task BeepMetadataMustConfirmTypedAmountsAndBoundedNullableHistory(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;var table=fault=="line-type"?"AP_INVOICE_LINE":"AP_INVOICE";
        n.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName==table),true)).Returns(()=>
        {
            var schema=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(table=="AP_INVOICE"?typeof(AP_INVOICE):typeof(AP_INVOICE_LINE));var field=schema.Fields.Single(row=>row.FieldName==(fault=="line-type"?"QUANTITY":fault=="header-type"?"TOTAL_AMOUNT":"CORRECTION_HISTORY_JSON"));
            if(fault=="missing")schema.Fields.Remove(field);if(fault is "type" or "line-type" or "header-type")field.Fieldtype=typeof(int).FullName;if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;return schema;
        });
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Writes);Assert.Equal(new[]{"begin","rollback"},n.Transactions);
    }
    [Theory][InlineData("line-write")][InlineData("header-write")][InlineData("line-readback")][InlineData("header-readback")][InlineData("late-contributor")][InlineData("late-authority")][InlineData("binding")]
    public async Task PartialWritesFailedReadbackAndLateDriftRollBackWholeCorrection(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;var original=JsonSerializer.Serialize(f.Review.Current.Invoice.Invoice);var lines=JsonSerializer.Serialize(f.Review.Current.Invoice.Lines);
        f.FailLine=fault=="line-write";f.CorruptLine=fault=="line-readback";f.Existing.Existing.FailWrite=fault=="header-write";f.Existing.Existing.Corrupt=fault=="header-readback";
        if(fault=="late-contributor")f.OnLine=()=>n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-authority")f.OnLine=()=>n.RevokeAt=n.AuthorityReads+1;if(fault=="binding")f.OnLine=()=>n.Native.Connection="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(new[]{"begin","rollback"},n.Native.Transactions);Assert.Equal(original,JsonSerializer.Serialize(f.Existing.Existing.Stored));Assert.Equal(lines,JsonSerializer.Serialize(n.Native.Rows.OfType<AP_INVOICE_LINE>().Where(line=>line.AP_INVOICE_ID=="invoice")));
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task BorrowedOrInsufficientTransactionsCannotWriteCorrection(bool borrowed)
    {var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;n.Borrowed=borrowed;if(!borrowed)n.Isolation=IsolationLevel.ReadCommitted;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(2,n.Writes);}
    [Theory][InlineData("valid")][InlineData("intent")][InlineData("reason")][InlineData("contributor")][InlineData("proof")]
    public async Task UnknownCommitCanOnlyReconcileExactOriginalCorrectionWithCurrentEvidence(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;n.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.NotNull(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);Assert.Equal(4,n.Writes);n.FailCommit=false;
        if(fault=="intent")f.Command=f.Command with{IntentId=Guid.NewGuid().ToString("N")};if(fault=="reason")f.Command=f.Command with{Draft=f.Command.Draft with{Reason="A substituted reason."}};
        if(fault=="contributor")n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;if(fault=="proof")f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON=f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON!.Replace(f.Command.Draft.Reason,"Altered recorded reason.",StringComparison.Ordinal);
        if(fault=="valid")(await f.Record()).Require(new(f.Command,f.Review));else await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(4,n.Writes);
    }
    [Theory][InlineData("header")][InlineData("line")]
    public async Task CommitWithoutDurableCompleteInvoiceCannotPublishSuccess(string table)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native.Native;var entity=table=="header"?"AP_INVOICE":"AP_INVOICE_LINE";
        n.Source.Setup(source=>source.GetEntityAsync(entity,It.IsAny<List<AppFilter>>())).ReturnsAsync((string name,List<AppFilter> filters)=>n.Transactions.LastOrDefault()=="commit"?Array.Empty<object>():n.Rows.Where(row=>row.GetType().Name==name && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(4,n.Writes);Assert.NotNull(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);
    }
    [Fact] public async Task RevokedOriginalAuthorityCannotOpenCorrectionTransaction()
    {var f=new Fixture();await f.Prepare();var n=f.Existing.Existing.Existing.ReadOwner.Native;n.RevokeAt=n.AuthorityReads+1;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.Empty(n.Native.Transactions);Assert.Equal(2,n.Native.Writes);}
    [Theory][InlineData("authority")][InlineData("contributor")]
    public async Task PostCommitDriftCannotPublishAndOriginalIntentCanOnlyReconcileAfterExactEvidenceReturns(string fault)
    {
        var f=new Fixture();await f.Prepare();var owner=f.Existing.Existing.Existing.ReadOwner.Native;var n=owner.Native;var changed=false;
        n.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
        {
            var rows=n.Rows.Where(row=>row is AP_INVOICE && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            if(!changed && n.Transactions.LastOrDefault()=="commit")
            {changed=true;if(fault=="authority")owner.RevokeAt=owner.AuthorityReads+1;else n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;}
            return rows;
        });
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.NotNull(f.Existing.Existing.Stored.CORRECTION_HISTORY_JSON);Assert.Equal(4,n.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(4,n.Writes);owner.RevokeAt=0;n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=3m;
        (await f.Record()).Require(new(f.Command,f.Review));Assert.Equal(4,n.Writes);
    }
}
#endif
