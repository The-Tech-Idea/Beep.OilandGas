using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalWorkflowSourceTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    [Fact] public async Task ActualJournalWorkflowSourceUsesRecordedContextAndStablePreparationThroughReviewAndPosting()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var f=new CompanySalesJournalPostingTests.Fixture(source.Source);await f.Prepare(source);var current=f.Command.Source.Current;var draft=f.Reviewed.Original;
        var preparation=CompanyAccountingSourceTypes.JournalPreparationRevision(draft);Assert.Equal(preparation,CompanyAccountingSourceTypes.JournalPreparationRevision(f.Command.Source.Reviewed));
        Task<CompanySalesJournalWorkflowSource> Read(string actor="workflow-reader",string? binding=null)=>f.Reviewed.Draft.Service.GetCompanySalesJournalWorkflowSourceAsync(current.Company,actor,binding??current.BindingRevision,f.Command.Source.Reviewed.Header.JOURNAL_ENTRY_ID,f.Guard,f.Field);
        var approved=await Read();approved.Require(current.Company,"workflow-reader",approved.JournalId);Assert.Equal("APPROVED",approved.Journal.Header.STATUS);Assert.Equal(current.BookId,approved.Journal.Header.BOOK_ID);Assert.Equal(current.CurrencyCode,approved.Journal.Header.CURRENCY_CODE);Assert.Equal(preparation,approved.SourceRevision);
        var outcome=await f.Post();var posted=await Read();posted.Require(current.Company,"workflow-reader",posted.JournalId);Assert.Equal("POSTED",posted.Journal.Header.STATUS);Assert.Equal(preparation,posted.SourceRevision);Assert.NotEqual(approved.Revision,posted.Revision);Assert.Equal(JsonSerializer.Serialize(outcome.Journal),JsonSerializer.Serialize(posted.Journal));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>Read(binding:new string('C',64)));f.FieldDenied=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>Read());f.FieldDenied=false;f.Reviewed.Draft.Native.Rows.Remove(f.Reviewed.Draft.Native.Rows.OfType<SALES_JOURNAL_POSTING_OUTCOME>().Single());await Assert.ThrowsAsync<InvalidOperationException>(()=>Read());Assert.Equal(7,f.Reviewed.Draft.Native.Writes);
    }
}
#endif
