using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesPostedLedgerTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    [Fact] public async Task ActualNativeLedgerProjectionDistinguishesUnpostedAndConfirmedFactsForAnotherAccountingReader()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesJournalPostingTests.Fixture(source.Source);await f.Prepare(source);var original=f.Command.Source.Current;
        Task<CompanySalesPostedLedgerReview> Read(string book="statutory",string currency="KWD")=>f.Reviewed.Draft.Service.GetCompanySalesPostedLedgerAsync(original.Company,"readonly-accountant",original.BindingRevision,book,currency,original.ReceivableId,f.AR,f.Guard,f.Field);
        var empty=await Read();empty.Require(original.Company,"readonly-accountant",original.BookId,original.CurrencyCode,original.ReceivableId);Assert.Empty(empty.Journals);Assert.Equal(0,f.Reviewed.Draft.Native.Writes);
        var outcome=await f.Post();var ledger=await Read();ledger.Require(original.Company,"readonly-accountant",original.BookId,original.CurrencyCode,original.ReceivableId);var row=Assert.Single(ledger.Journals);Assert.Equal(outcome.Journal.Header.JOURNAL_ENTRY_ID,row.JournalId);Assert.Equal("authorized-poster",row.PostedBy);Assert.Equal("independent-reviewer",row.Reviewer);Assert.Equal(source.Source.ActorUserId,row.Maker);Assert.Equal(outcome.Journal.Header.TOTAL_DEBIT,row.Debit);Assert.Equal(outcome.Journal.Header.TOTAL_CREDIT,row.Credit);Assert.Equal(4,row.Lines.Count);Assert.Equal(f.Command.Source.Current.Lines.Select(value=>value.AccountNumber),row.Lines.Select(value=>value.AccountNumber));Assert.Equal(7,f.Reviewed.Draft.Native.Writes);
        var json=JsonSerializer.Serialize(ledger);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("WORKFLOW_JSON",json);Assert.DoesNotContain("OUTCOME_JSON",json);Assert.DoesNotContain("AUTHORITY_JSON",json);Assert.DoesNotContain(f.Command.Reason,json);
        var otherBook=await Read("other-book");Assert.Empty(otherBook.Journals);Assert.Equal("other-book",otherBook.BookId);var otherCurrency=await Read(currency:"USD");Assert.Empty(otherCurrency.Journals);Assert.Equal("USD",otherCurrency.CurrencyCode);
        f.FieldDenied=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Read());f.FieldDenied=false;var native=f.Reviewed.Draft.Native;var entry=native.Rows.OfType<GL_ENTRY>().First();var debit=entry.DEBIT_AMOUNT;entry.DEBIT_AMOUNT+=.001m;await Assert.ThrowsAnyAsync<Exception>(()=>Read());entry.DEBIT_AMOUNT=debit;native.Rows.Remove(native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>().Single());await Assert.ThrowsAsync<InvalidOperationException>(()=>Read());Assert.Equal(7,native.Writes);
    }
}
#endif
