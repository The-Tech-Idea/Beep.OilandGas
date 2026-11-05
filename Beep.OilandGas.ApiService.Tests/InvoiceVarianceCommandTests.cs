using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class InvoiceVarianceCommandTests
{
    internal sealed class Fixture
    {
        public CompanyMatchingPolicyCommandTests.Fixture Existing=null!;
        public APInvoiceVarianceProposalReview Review=null!;
        public APInvoiceVarianceCommand Command=null!;
        public bool Corrupt,FailWrite;
        public Action? OnUpdate;
        public AP_INVOICE Stored=>Existing.ReadOwner.Native.Native.Rows.OfType<AP_INVOICE>().Single(row=>row.AP_INVOICE_ID=="invoice");
        public async Task Prepare()
        {
            Existing=await InvoiceVariancePreparationTests.Prepared();var native=Existing.ReadOwner.Native.Native;native.Writes=0;native.Transactions.Clear();
            var financial=await InvoiceVariancePreparationTests.Financial(Existing);Review=APInvoiceVarianceProposalReview.Create(financial,InvoiceVariancePreparationTests.Proposal(financial),new string('B',64));
            Command=new(new("tenant","company"),"actor","invoice",Review.Proposal,Review.Revision,Review.BindingRevision,Guid.NewGuid().ToString("N"));
            native.Source.Setup(source=>source.UpdateEntity("AP_INVOICE",It.IsAny<object>())).Returns((string table,object value)=>
            {
                native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var row=PurchaseReceiptDecisionProof.Copy((AP_INVOICE)value);if(Corrupt)row.TOTAL_AMOUNT=99m;
                native.Rows.RemoveAll(item=>item is AP_INVOICE invoice && invoice.AP_INVOICE_ID==row.AP_INVOICE_ID);native.Rows.Add(row);OnUpdate?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
        }
        public Task<AP_INVOICE> Record()=>Existing.ReadOwner.Native.AP.RecordVarianceProposalAsync(Command,Review,Existing.ReadOwner.Native.Authority,()=>InvoiceVariancePreparationTests.Financial(Existing));
    }
    [Fact] public async Task NativeProposalRecordsExactOriginalIntentOnceAndKeepsFinancialStatusDraft()
    {
        var f=new Fixture();await f.Prepare();var row=await f.Record();var proof=APInvoiceVarianceProof.Require(f.Command.Company,row);
        Assert.True(proof.Command.Same(f.Command));Assert.Equal("DRAFT",row.STATUS);Assert.Equal(2.01m,row.TOTAL_AMOUNT);Assert.Equal("actor",row.ROW_CHANGED_BY);Assert.Equal("source-maker",row.ROW_CREATED_BY);
        new APInvoiceVarianceResult(f.Command,row).Require(new(f.Command,f.Review));var retry=await f.Record();Assert.Equal(row.VARIANCE_PROPOSAL_JSON,retry.VARIANCE_PROPOSAL_JSON);
        Assert.Equal(1,f.Existing.ReadOwner.Native.Native.Writes);Assert.Equal(new[]{"begin","commit","begin","commit"},f.Existing.ReadOwner.Native.Native.Transactions);
        var current=await InvoiceVariancePreparationTests.Financial(f.Existing);Assert.NotEqual(f.Review.Financial.Revision,current.Revision);Assert.Equal(f.Review.Financial.Revision,current.OriginalVarianceEvidence(f.Command).Revision);
    }
    [Theory][InlineData("intent")][InlineData("actor")][InlineData("binding")][InlineData("proposal")][InlineData("preparation")]
    public async Task AlteredOriginalSubmissionCannotOpenTransaction(string fault)
    {
        var f=new Fixture();await f.Prepare();f.Command=fault switch{"intent"=>f.Command with{IntentId="not-guid"},"actor"=>f.Command with{ActorUserId="other"},"binding"=>f.Command with{BindingRevision=new string('C',64)},"proposal"=>f.Command with{Proposal=f.Command.Proposal with{Reason="different"}},_=>f.Command with{ExpectedPreparationRevision=new string('C',64)}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Empty(f.Existing.ReadOwner.Native.Native.Transactions);Assert.Equal(0,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Theory][InlineData("invoice")][InlineData("line")][InlineData("purchase")][InlineData("allocation")][InlineData("policy")][InlineData("receiver")][InlineData("foreign")][InlineData("duplicate")]
    public async Task ChangedCompleteEvidenceOrGlobalIdentityRefusesBeforeWrite(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native;
        if(fault=="invoice")f.Stored.TOTAL_AMOUNT=99m;if(fault=="line")n.Line.ITEM_DESCRIPTION="changed";if(fault=="purchase")n.OrderLine.UNIT_PRICE=99m;
        if(fault=="allocation")n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="policy")f.Existing.Stored.DESCRIPTION="changed";if(fault=="receiver")n.Receipt.RECEIVER_DECISION_JSON=null;if(fault=="foreign")f.Stored.OWNER_COMPANY_BA_ID="foreign";
        if(fault=="duplicate")n.Native.Rows.Add(PurchaseReceiptDecisionProof.Copy(f.Stored));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,n.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("type")][InlineData("size")][InlineData("nullable")]
    public async Task FreshBeepSchemaMustDeclareBoundedNullableProposalColumn(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native.Native;
        n.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(row=>row.EntityName=="AP_INVOICE"),true)).Returns(()=>
        {
            var schema=new ClassCreator(n.Editor.Object).ConvertToEntityStructure(typeof(AP_INVOICE));var field=schema.Fields.Single(row=>row.FieldName=="VARIANCE_PROPOSAL_JSON");
            if(fault=="missing")schema.Fields.Remove(field);if(fault=="type")field.Fieldtype=typeof(decimal).FullName;if(fault=="size")field.Size=100;if(fault=="nullable")field.AllowDBNull=false;return schema;
        });
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,n.Writes);
    }
    [Theory][InlineData("write")][InlineData("readback")][InlineData("late-source")][InlineData("late-authority")][InlineData("binding")]
    public async Task FailureOrDriftDuringRecordingCannotPublishAConfirmedProposal(string fault)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native;
        f.FailWrite=fault=="write";f.Corrupt=fault=="readback";
        if(fault=="late-source")f.OnUpdate=()=>n.Native.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="late-authority")f.OnUpdate=()=>n.RevokeAt=n.AuthorityReads+1;if(fault=="binding")f.OnUpdate=()=>n.Native.Connection="changed";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(new[]{"begin","rollback"},n.Native.Transactions);Assert.Null(f.Stored.VARIANCE_PROPOSAL_JSON);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task BorrowedOrInsufficientIsolationCannotRecord(bool borrowed)
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native.Native;n.Borrowed=borrowed;if(!borrowed)n.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,n.Writes);
    }
    [Fact] public async Task UnknownCommitReconcilesOriginalIntentWithoutAnotherProposalWrite()
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native.Native;n.FailCommit=true;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.NotNull(f.Stored.VARIANCE_PROPOSAL_JSON);n.FailCommit=false;
        var result=await f.Record();Assert.True(APInvoiceVarianceProof.Require(f.Command.Company,result).Command.Same(f.Command));Assert.Equal(1,n.Writes);
    }
    [Theory][InlineData("other-intent")][InlineData("changed-contributor")][InlineData("changed-proof")]
    public async Task RecordedProposalCannotBeReplacedAndRetryCannotIgnoreLaterEvidence(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.Record();var n=f.Existing.ReadOwner.Native.Native;
        if(fault=="other-intent")f.Command=f.Command with{IntentId=Guid.NewGuid().ToString("N")};
        if(fault=="changed-contributor")n.Rows.OfType<AP_RECEIPT_ALLOCATION>().Single(row=>row.AP_RECEIPT_ALLOCATION_ID=="competitor").ALLOCATED_QUANTITY=4m;
        if(fault=="changed-proof")f.Stored.VARIANCE_PROPOSAL_JSON=f.Stored.VARIANCE_PROPOSAL_JSON!.Replace(f.Command.Proposal.Reason,"Changed proposal reason.",StringComparison.Ordinal);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,n.Writes);
    }
    [Fact] public async Task ProviderCommitSuccessWithoutDurableInvoiceCannotReturnSuccess()
    {
        var f=new Fixture();await f.Prepare();var n=f.Existing.ReadOwner.Native.Native;
        n.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
            n.Transactions.LastOrDefault()=="commit"?Array.Empty<object>():n.Rows.Where(row=>row is AP_INVOICE && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,n.Writes);
    }
    [Fact] public async Task LostSubmissionAuthorityCannotOpenATransaction()
    {
        var f=new Fixture();await f.Prepare();f.Existing.ReadOwner.Native.RevokeAt=1;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.Empty(f.Existing.ReadOwner.Native.Native.Transactions);Assert.Equal(0,f.Existing.ReadOwner.Native.Native.Writes);
    }
    [Fact] public async Task LostAuthorityAfterCommitRetainsOriginalProposalForReconciliation()
    {
        var f=new Fixture();await f.Prepare();var native=f.Existing.ReadOwner.Native;var n=native.Native;var revoked=false;
        n.Source.Setup(source=>source.GetEntityAsync("AP_INVOICE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>
        {
            var rows=n.Rows.Where(row=>row is AP_INVOICE && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize(JsonSerializer.Serialize(row),row.GetType())!).ToArray();
            if(!revoked && n.Transactions.LastOrDefault()=="commit"){revoked=true;native.RevokeAt=native.AuthorityReads+1;}return rows;
        });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Record());Assert.NotNull(f.Stored.VARIANCE_PROPOSAL_JSON);native.RevokeAt=0;await f.Record();Assert.Equal(1,n.Writes);
    }
    [Fact] public async Task CanonicalBeepMetadataAndSourceRevisionIncludeNotifyingProposalColumn()
    {
        var f=new Fixture();await f.Prepare();var field=new ClassCreator(f.Existing.ReadOwner.Native.Native.Editor.Object).ConvertToEntityStructure(typeof(AP_INVOICE)).Fields.Single(row=>row.FieldName=="VARIANCE_PROPOSAL_JSON");
        Assert.Equal(typeof(string).FullName,field.Fieldtype);Assert.True(field.AllowDBNull);Assert.Equal(APInvoiceVarianceProof.MaximumLength,field.Size);
        var notifications=new List<string?>();f.Stored.PropertyChanged+=(_,args)=>notifications.Add(args.PropertyName);var old=f.Review.Financial.Invoice.Revision;
        await f.Record();Assert.NotEqual(old,(await InvoiceVariancePreparationTests.Financial(f.Existing)).Invoice.Revision);
        var copy=PurchaseReceiptDecisionProof.Copy(f.Stored);copy.PropertyChanged+=(_,args)=>notifications.Add(args.PropertyName);copy.VARIANCE_PROPOSAL_JSON=null;Assert.Contains("VARIANCE_PROPOSAL_JSON",notifications);
    }
    [Theory][InlineData("row")][InlineData("proof")][InlineData("result")]
    public async Task VerifiedOutcomeCannotBeAdoptedAfterChangingItsRowCommandOrExpectedSubmission(string fault)
    {
        var f=new Fixture();await f.Prepare();var row=await f.Record();
        if(fault=="row")row.TOTAL_AMOUNT=99m;if(fault=="proof")row.VARIANCE_PROPOSAL_JSON=row.VARIANCE_PROPOSAL_JSON!.Replace("\"IntentId\"", "\"UnrecognizedIntent\"",StringComparison.Ordinal);
        var result=new APInvoiceVarianceResult(fault=="result"?f.Command with{IntentId=Guid.NewGuid().ToString("N")}:f.Command,row);
        Assert.Throws<InvalidOperationException>(()=>result.Require(new(f.Command,f.Review)));
    }
}
#endif
