using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using TheTechIdeaWeb.Diagnostics;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesPaymentDraftTests(CompanySalesPaymentDraftTests.PostedFixture shared):IClassFixture<CompanySalesPaymentDraftTests.PostedFixture>
{
    public sealed class PostedFixture:IAsyncLifetime
    {
        internal CompanySalesJournalPostingTests.Fixture Posting=null!;
        internal CompanySalesJournalPreparationTests.ApprovedSourceFixture Baseline=null!;
        internal object[] Rows=[];
        internal CompanySalesPaymentSource Source=null!;
        public async Task InitializeAsync(){using var validation=CompanySalesInvoiceValidation.Enter();var baseline=new CompanySalesJournalPreparationTests.ApprovedSourceFixture();await baseline.InitializeAsync();Baseline=baseline;Posting=new(baseline.Source);await Posting.Prepare(baseline);await Posting.Post();var native=Posting.Reviewed.Draft.Native;Rows=native.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone).ToArray();var types=Rows.Select(value=>value.GetType()).Concat([typeof(AR_PAYMENT),typeof(SALES_PAYMENT_REVIEW_INTENT)]).Distinct().ToDictionary(type=>type==typeof(GLAccountMapping)?"GL_ACCOUNT_MAPPING":type.Name,type=>type);native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types[requested.EntityName]));Source=await Posting.AR.GetCompanySalesPaymentSourceAsync(baseline.Source.Company,"collector",baseline.Source.BindingRevision,baseline.Source.BookId,baseline.Source.CurrencyCode,baseline.Source.ReceivableId,Posting.Reviewed.Draft.Service,Posting.Guard,Posting.Field);}
        public Task DisposeAsync()=>Task.CompletedTask;
    }
    private CompanySalesPaymentCommand Command(decimal? amount=null)=>new(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),CompanyInventorySourceReview.Copy(shared.Source),"payment-001",shared.Source.Ledger.Journals.Single().PostingDate,amount??1m,"BANK_TRANSFER",shared.Source.CustomerId,"bank-trace-001","Original bank statement reference",new DateTime(2026,10,12,0,0,0,DateTimeKind.Utc),"Original receipt proposal");
    private void Reset(){var native=shared.Posting.Reviewed.Draft.Native;native.Rows.Clear();native.Rows.AddRange(shared.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone));native.Writes=0;native.Transactions.Clear();native.FailCommit=false;native.Borrowed=false;shared.Posting.Denied=false;shared.Posting.FieldDenied=false;native.Source.Setup(value=>value.InsertEntity("AR_PAYMENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((AR_PAYMENT)value));return new ErrorsInfo{Flag=Errors.Ok};});}
    private Task<AR_PAYMENT> Prepare(CompanySalesPaymentCommand command)=>shared.Posting.AR.PrepareCompanySalesPaymentAsync(command,shared.Posting.Reviewed.Draft.Service,shared.Posting.Guard,shared.Posting.Field);
    [Theory][InlineData("complete")][InlineData("tenant")][InlineData("receipt")][InlineData("source")]
    public async Task CompanylessLegacyOwnerCannotReadOrAllocateCompanyPaymentEvidence(string marker)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var native=shared.Posting.Reviewed.Draft.Native;
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});
        var basis=(AccountingBasisPostingService)typeof(ARService).GetField("_basisPosting",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(shared.Posting.AR)!;
        var legacy=new ARService(native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,basis,Mock.Of<IFailureReporter>(),NullLogger<ARService>.Instance);
        var row=marker=="complete"?Command().CreateRow():new AR_PAYMENT{AR_PAYMENT_ID=Guid.NewGuid().ToString("N"),ACTIVE_IND="Y"};
        if(marker=="tenant")row.OWNER_TENANT_ID="protected";if(marker=="receipt")row.PREPARATION_WORKFLOW_JSON="protected receipt";if(marker=="source")row.SOURCE=CompanySalesPaymentProof.NativeSource;
        row.AR_INVOICE_ID="legacy-invoice";native.Rows.Add(row);native.Rows.Add(new AR_INVOICE{AR_INVOICE_ID="legacy-invoice",ACTIVE_IND="Y",TOTAL_AMOUNT=100,BALANCE_DUE=100});var before=JsonSerializer.Serialize(native.Rows);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>legacy.ApplyPaymentAsync(row.AR_PAYMENT_ID,"legacy-invoice",1,"legacy-actor",native.Connection));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>legacy.GetPaymentsByInvoiceAsync("legacy-invoice",native.Connection));
        Assert.Equal(before,JsonSerializer.Serialize(native.Rows));Assert.Equal(0,native.Writes);
    }
    [Fact] public async Task NativeProposalCommitsOncePreservesARAndLedgerMoneyAndRecoversOnlyOriginalProducer()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var original=JsonSerializer.Serialize(shared.Rows.Where(value=>value is AR_INVOICE or GL_ENTRY));var command=Command();var row=await Prepare(command);Assert.Equal("DRAFT",row.STATUS);Assert.Equal(command.Amount,row.PAYMENT_AMOUNT);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(CompanySalesPaymentProof.Require(row).Command));var writes=shared.Posting.Reviewed.Draft.Native.Writes;Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await Prepare(command)));Assert.Equal(writes,shared.Posting.Reviewed.Draft.Native.Writes);Assert.Equal(original,JsonSerializer.Serialize(shared.Posting.Reviewed.Draft.Native.Rows.Where(value=>value is AR_INVOICE or GL_ENTRY)));
        var source=await shared.Posting.AR.GetCompanySalesPaymentSourceAsync(command.Source.Ledger.Company,"collector",command.Source.Ledger.BindingRevision,command.Source.Ledger.BookId,command.Source.Ledger.CurrencyCode,command.Source.Ledger.ReceivableId,shared.Posting.Reviewed.Draft.Service,shared.Posting.Guard,shared.Posting.Field);Assert.Equal(command.Amount,source.UnreleasedProposedAmount);Assert.Equal(source.OriginalAmount-command.Amount,source.AvailableToPropose);Assert.DoesNotContain("PREPARATION_WORKFLOW_JSON",JsonSerializer.Serialize(source));var recovered=await shared.Posting.AR.GetCompanySalesPaymentDraftAsync(source.Ledger.Company,"collector",command.Id,shared.Posting.Guard,shared.Posting.Field);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovered));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>shared.Posting.AR.GetCompanySalesPaymentDraftAsync(source.Ledger.Company,"another",command.Id,shared.Posting.Guard,shared.Posting.Field));Assert.Equal(writes,shared.Posting.Reviewed.Draft.Native.Writes);
    }
    [Theory][InlineData("overpayment")][InlineData("precision")][InlineData("method")][InlineData("evidence")][InlineData("reason")]
    public async Task InvalidOrOversubscribedProposalCannotWriteOrClampMoney(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var c=Command();c=fault switch{"overpayment"=>c with{Amount=c.Source.OriginalAmount+1},"precision"=>c with{Amount=.0001m},"method"=>c with{Method="DEFAULT"},"evidence"=>c with{EvidenceReference=""},_=>c with{Reason=""}};await Assert.ThrowsAnyAsync<Exception>(()=>Prepare(c));Assert.Equal(0,shared.Posting.Reviewed.Draft.Native.Writes);
    }
    [Fact] public async Task UnknownCommitReconcilesExactOriginalProposalWithoutCurrentSourceOrAnotherInsertion()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var command=Command();var native=shared.Posting.Reviewed.Draft.Native;native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>Prepare(command));native.FailCommit=false;var row=await Prepare(command);Assert.Equal(command.IntentId,CompanySalesPaymentProof.Require(row).Command.IntentId);Assert.Equal(1,native.Writes);await Assert.ThrowsAsync<InvalidOperationException>(()=>Prepare(command with{EvidenceReference="Different bank evidence"}));Assert.Equal(1,native.Writes);
    }
    [Theory][InlineData("failed")][InlineData("corrupt")][InlineData("authority")][InlineData("receivable")][InlineData("ledger")]
    public async Task FailedOrUnconfirmedProposalRollsBackWithoutChangingReceivableOrGL(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var native=shared.Posting.Reviewed.Draft.Native;native.Source.Setup(value=>value.InsertEntity("AR_PAYMENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(fault=="failed")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((AR_PAYMENT)value);if(fault=="corrupt")row.PAYMENT_AMOUNT+=1;native.Rows.Add(row);if(fault=="authority")shared.Posting.Denied=true;if(fault=="receivable")native.Rows.OfType<AR_INVOICE>().Single().TOTAL_AMOUNT+=1;if(fault=="ledger")native.Rows.OfType<GL_ENTRY>().First().DEBIT_AMOUNT+=1;return new ErrorsInfo{Flag=Errors.Ok};});await Assert.ThrowsAnyAsync<Exception>(()=>Prepare(Command()));Assert.Empty(native.Rows.OfType<AR_PAYMENT>());Assert.Equal("rollback",native.Transactions.Last());Assert.Equal(JsonSerializer.Serialize(shared.Rows.Where(value=>value is AR_INVOICE or GL_ENTRY)),JsonSerializer.Serialize(native.Rows.Where(value=>value is AR_INVOICE or GL_ENTRY)));
    }
}
#endif
