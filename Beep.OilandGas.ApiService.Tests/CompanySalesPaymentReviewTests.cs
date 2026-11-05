#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using TheTechIdea.Beep.ConfigUtil;
using Moq;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed partial class CompanySalesPaymentReviewTests(CompanySalesPaymentReviewTests.ReviewFixture shared):IClassFixture<CompanySalesPaymentReviewTests.ReviewFixture>
{
    public sealed class ReviewFixture:IAsyncLifetime
    {
        internal CompanySalesPaymentDraftTests.PostedFixture Base=new();
        internal AR_PAYMENT Draft=null!;
        internal CompanySalesPaymentReviewSource Review=null!;
        internal object[] Rows=[];
        public async Task InitializeAsync(){using var validation=CompanySalesInvoiceValidation.Enter();await Base.InitializeAsync();var p=Base.Posting;var source=Base.Source;var native=p.Reviewed.Draft.Native;native.Source.Setup(value=>value.InsertEntity("AR_PAYMENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((AR_PAYMENT)value));return new ErrorsInfo{Flag=Errors.Ok};});var command=new CompanySalesPaymentCommand(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),source,"review-payment",source.Ledger.Journals.Single().PostingDate,1,"BANK_TRANSFER",source.CustomerId,"review-bank-trace","Original statement evidence",new DateTime(2026,10,12,0,0,0,DateTimeKind.Utc),"Explicit original payment proposal");Draft=await p.AR.PrepareCompanySalesPaymentAsync(command,p.Reviewed.Draft.Service,p.Guard,p.Field);Review=await p.AR.GetCompanySalesPaymentReviewAsync(source.Ledger.Company,"controller",source.Ledger.BindingRevision,Draft.AR_PAYMENT_ID,p.Reviewed.Draft.Service,p.Guard,p.Field);Rows=native.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone).ToArray();}
        public Task DisposeAsync()=>Task.CompletedTask;
    }
    private bool failed,corrupt,denied,fieldDenied,commitLost,bindingChanged,ledgerChanged;
    private void Reset()
    {
        failed=corrupt=denied=fieldDenied=commitLost=bindingChanged=ledgerChanged=false;var native=shared.Base.Posting.Reviewed.Draft.Native;native.Rows.Clear();native.Rows.AddRange(shared.Rows.Select(CompanyGLAccountReviewTests.Fixture.Clone));native.Writes=0;native.Transactions.Clear();native.Source.Invocations.Clear();native.Editor.Invocations.Clear();native.FailCommit=false;native.Borrowed=false;native.Editor.Setup(value=>value.GetDataSource(native.Connection)).Returns(native.Source.Object);shared.Base.Posting.Denied=false;shared.Base.Posting.FieldDenied=false;
        native.Source.Setup(value=>value.InsertEntity("SALES_PAYMENT_REVIEW_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_PAYMENT_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
        native.Source.Setup(value=>value.UpdateEntity("AR_PAYMENT",It.IsAny<object>())).Returns((string table,object value)=>{native.Writes++;if(failed)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((AR_PAYMENT)value);if(corrupt)row.PAYMENT_AMOUNT+=1;native.Rows.RemoveAll(item=>item is AR_PAYMENT before && before.AR_PAYMENT_ID==row.AR_PAYMENT_ID);native.Rows.Add(row);if(denied)shared.Base.Posting.Denied=true;if(fieldDenied)shared.Base.Posting.FieldDenied=true;if(commitLost)native.FailCommit=true;if(bindingChanged)native.Editor.Setup(value=>value.GetDataSource(native.Connection)).Returns(Mock.Of<TheTechIdea.Beep.IDataSource>());if(ledgerChanged)native.Rows.OfType<GL_ENTRY>().First().DEBIT_AMOUNT+=1;return new ErrorsInfo{Flag=Errors.Ok};});
    }
    private CompanySalesPaymentReviewCommand Command(string decision="APPROVE")=>new(Guid.NewGuid().ToString("N"),CompanyInventorySourceReview.Copy(shared.Review),decision,decision=="APPROVE"?"CONFIRMED":"ISSUE","Explicit financial review evidence","CONFIRMED","Explicit original receipt review evidence","Independent controller disposition");
    private Task<AR_PAYMENT> Record(CompanySalesPaymentReviewCommand command)=>shared.Base.Posting.AR.RecordCompanySalesPaymentReviewAsync(command,shared.Base.Posting.Reviewed.Draft.Service,shared.Base.Posting.Guard,shared.Base.Posting.Field);
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task NativeIndependentDispositionPreservesMoneyOriginalDraftAndSeparatePrivateJournal(string decision)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var p=shared.Base.Posting;var native=p.Reviewed.Draft.Native;var original=JsonSerializer.Serialize(native.Rows.Where(row=>row is AR_INVOICE or GL_ENTRY));var command=Command(decision);var row=await Record(command);Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",row.STATUS);Assert.Equal(shared.Draft.PREPARATION_WORKFLOW_JSON,row.PREPARATION_WORKFLOW_JSON);Assert.Equal(JsonSerializer.Serialize(shared.Draft),JsonSerializer.Serialize(CompanySalesPaymentReviewProof.OriginalDraft(row)));Assert.Equal(original,JsonSerializer.Serialize(native.Rows.Where(value=>value is AR_INVOICE or GL_ENTRY)));Assert.Equal(2,native.Writes);
        Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await Record(command)));Assert.Equal(2,native.Writes);var recovery=await p.AR.GetCompanySalesPaymentReviewRecoveryAsync(shared.Review.Current.Ledger.Company,"controller",row.AR_PAYMENT_ID,p.Guard,p.Field);recovery.Require();Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(recovery.Intent!.Restore()));Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovery.Outcome));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>p.AR.GetCompanySalesPaymentReviewRecoveryAsync(shared.Review.Current.Ledger.Company,"collector",row.AR_PAYMENT_ID,p.Guard,p.Field));var producer=await p.AR.GetCompanySalesPaymentDraftAsync(shared.Review.Current.Ledger.Company,"collector",row.AR_PAYMENT_ID,p.Guard,p.Field);Assert.Equal(JsonSerializer.Serialize(shared.Draft),JsonSerializer.Serialize(producer));Assert.DoesNotContain("REVIEW_WORKFLOW_JSON",JsonSerializer.Serialize(producer));
        var source=await p.AR.GetCompanySalesPaymentSourceAsync(shared.Review.Current.Ledger.Company,"collector",shared.Review.Current.Ledger.BindingRevision,shared.Review.Current.Ledger.BookId,shared.Review.Current.Ledger.CurrencyCode,shared.Review.Current.Ledger.ReceivableId,p.Reviewed.Draft.Service,p.Guard,p.Field);var fact=Assert.Single(source.Payments);Assert.Equal(row.STATUS,fact.Status);Assert.Equal("controller",fact.Review!.Reviewer);Assert.Equal(decision=="APPROVE"?1m:0m,source.UnreleasedProposedAmount);Assert.Equal(ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(shared.Draft)),fact.PreparationRevision);Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(source));
    }
    [Theory][InlineData("maker")][InlineData("missing-evidence")][InlineData("approve-issue")][InlineData("changed-original")]
    public async Task InvalidOrNonIndependentDecisionCannotRetainOrMutate(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var command=Command();if(fault=="maker")command=command with{Review=command.Review with{Current=command.Review.Current with{Ledger=command.Review.Current.Ledger with{ActorUserId="collector"}}}};if(fault=="missing-evidence")command=command with{ReceiptEvidence=""};if(fault=="approve-issue")command=command with{FinancialDisposition="ISSUE"};if(fault=="changed-original"){var copy=CompanyInventorySourceReview.Copy(command.Review.Original);copy.PAYMENT_AMOUNT+=1;command=command with{Review=command.Review with{Original=copy}};}await Assert.ThrowsAnyAsync<Exception>(()=>Record(command));Assert.Equal(0,shared.Base.Posting.Reviewed.Draft.Native.Writes);
    }
    [Theory][InlineData("failed")][InlineData("corrupt")][InlineData("authority")][InlineData("field")][InlineData("binding")][InlineData("ledger")]
    public async Task FailedDispositionRetainsOriginalDecisionAndRollsBackPayment(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();failed=fault=="failed";corrupt=fault=="corrupt";denied=fault=="authority";fieldDenied=fault=="field";bindingChanged=fault=="binding";ledgerChanged=fault=="ledger";var command=Command();await Assert.ThrowsAnyAsync<Exception>(()=>Record(command));var native=shared.Base.Posting.Reviewed.Draft.Native;Assert.Equal(JsonSerializer.Serialize(shared.Draft),JsonSerializer.Serialize(Assert.Single(native.Rows.OfType<AR_PAYMENT>())));Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(Assert.Single(native.Rows.OfType<SALES_PAYMENT_REVIEW_INTENT>()).Restore()));Assert.Equal("rollback",native.Transactions.Last());Assert.Equal(JsonSerializer.Serialize(shared.Rows.Where(row=>row is AR_INVOICE or GL_ENTRY)),JsonSerializer.Serialize(native.Rows.Where(row=>row is AR_INVOICE or GL_ENTRY)));
    }
    [Theory][InlineData("retention")][InlineData("disposition")]
    public async Task LostCommitRecoversOriginalReviewerAndExactRetryDoesNotInsertAgain(string point)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();Reset();var native=shared.Base.Posting.Reviewed.Draft.Native;native.FailCommit=point=="retention";commitLost=point=="disposition";var command=Command();await Assert.ThrowsAsync<InvalidOperationException>(()=>Record(command));native.FailCommit=false;commitLost=false;var p=shared.Base.Posting;var recovery=await p.AR.GetCompanySalesPaymentReviewRecoveryAsync(shared.Review.Current.Ledger.Company,"controller",shared.Draft.AR_PAYMENT_ID,p.Guard,p.Field);Assert.NotNull(recovery.Intent);Assert.Equal(point=="disposition",recovery.Outcome is not null);var writes=native.Writes;var row=await Record(command);Assert.Equal("APPROVED",row.STATUS);Assert.Equal(writes+(point=="retention"?1:0),native.Writes);Assert.Single(native.Rows.OfType<SALES_PAYMENT_REVIEW_INTENT>());await Assert.ThrowsAsync<InvalidOperationException>(()=>Record(command with{Reason="Another decision"}));
    }
}
#endif
