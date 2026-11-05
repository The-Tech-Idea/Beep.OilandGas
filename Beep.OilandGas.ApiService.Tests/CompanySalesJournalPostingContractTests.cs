#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPostingContractTests(CompanySalesJournalPreparationTests.ApprovedSourceFixture source):IClassFixture<CompanySalesJournalPreparationTests.ApprovedSourceFixture>
{
    private async Task<(CompanySalesJournalReviewTests.Fixture Native,CompanySalesJournalPostingCommand Command)> Approved()
    {
        var native=new CompanySalesJournalReviewTests.Fixture(source.Source);await native.Prepare();var reviewed=await native.Review();var posting=CompanySalesJournalPostingSource.Create("authorized-poster",reviewed,source.Source,source.Receivable);return(native,new(Guid.NewGuid().ToString("N"),posting,reviewed.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray(),"Explicit original journal posting reason"));
    }
    [Fact] public async Task CanonicalPostingRequestPreservesExactIndependentReviewAndCreatesOnlyFrozenNativeGLFacts()
    {
        var (native,command)=await Approved();command.Require();var before=JsonSerializer.Serialize(command.Source);var entries=command.EntryIds.Select((_,index)=>command.CreateEntry(index)).ToArray();Assert.Equal(command.Source.Reviewed.Header.TOTAL_DEBIT,entries.Sum(value=>value.DEBIT_AMOUNT));Assert.Equal(command.Source.Reviewed.Header.TOTAL_CREDIT,entries.Sum(value=>value.CREDIT_AMOUNT));
        for(var index=0;index<entries.Length;index++){var row=entries[index];var line=command.Source.Reviewed.Lines[index];Assert.Equal(line.JOURNAL_ENTRY_LINE_ID,row.JOURNAL_ENTRY_LINE_ID);Assert.Equal(line.GL_ACCOUNT_ID,row.GL_ACCOUNT_ID);Assert.Equal(line.DEBIT_AMOUNT,row.DEBIT_AMOUNT);Assert.Equal(line.CREDIT_AMOUNT,row.CREDIT_AMOUNT);Assert.Equal(line.MAPPING_KEY,row.MAPPING_KEY);Assert.Equal(command.Source.Current.PostingDate,row.ENTRY_DATE);Assert.Equal(command.Source.Current.CurrencyCode,row.CURRENCY_CODE);Assert.Equal(command.IntentId,row.SALES_JOURNAL_POSTING_INTENT_ID);Assert.Equal(command.Source.Current.ReceivableId,row.SOURCE_AR_INVOICE_ID);Assert.Equal(command.Source.Current.Company.TenantId,row.OWNER_TENANT_ID);Assert.Equal(command.Source.Current.BookId,row.BOOK_ID);}
        Assert.Equal(before,JsonSerializer.Serialize(command.Source));Assert.Equal("APPROVED",native.Draft.Native.Rows.OfType<JOURNAL_ENTRY>().Single().STATUS);Assert.Empty(native.Draft.Native.Rows.OfType<GL_ENTRY>());Assert.Equal(2,native.Draft.Native.Writes);
        var intent=SALES_JOURNAL_POSTING_INTENT.Create(command);intent.ROW_CREATED_BY=command.Source.ActorUserId;intent.ROW_CHANGED_BY=command.Source.ActorUserId;intent.ROW_CREATED_DATE=DateTime.UtcNow;intent.ROW_CHANGED_DATE=intent.ROW_CREATED_DATE;Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(intent.Restore()));intent.FIELD_ID="other-field";Assert.Throws<InvalidOperationException>(()=>intent.Restore());
        var schema=new ClassCreator(native.Draft.Native.Editor.Object).ConvertToEntityStructure(typeof(SALES_JOURNAL_POSTING_INTENT));Assert.Equal("SALES_JOURNAL_POSTING_INTENT_ID",Assert.Single(schema.Fields,value=>value.IsKey).FieldName);ModuleSchemaBoundary.Validate([typeof(SALES_JOURNAL_POSTING_INTENT)]);Assert.Throws<ArgumentException>(()=>ModuleSchemaBoundary.Validate([typeof(CompanySalesJournalPostingSource)]));
    }
    [Theory][InlineData("draft")][InlineData("rejected")][InlineData("receivable-money")][InlineData("receivable-audit")][InlineData("line")][InlineData("source")][InlineData("identity")][InlineData("count")][InlineData("duplicate")][InlineData("reason")]
    public async Task ChangedOrUnapprovedNativeFinancialFactsCannotAuthorizePosting(string fault)
    {
        var (_,original)=await Approved();var command=CompanyInventorySourceReview.Copy(original);
        if(fault=="draft")command.Source.Reviewed.Header.STATUS="DRAFT";if(fault=="rejected")command.Source.Reviewed.Header.STATUS="REJECTED";if(fault=="receivable-money")command.Source.Receivable.TOTAL_AMOUNT+=.001m;if(fault=="receivable-audit")command.Source.Receivable.ROW_CHANGED_BY="other-reviewer";if(fault=="line")command.Source.Reviewed.Lines[0].DEBIT_AMOUNT+=.001m;if(fault=="source")command=command with{Source=command.Source with{Current=command.Source.Current with{Revision=new string('A',64)}}};if(fault=="identity")command=command with{IntentId=command.Source.Reviewed.Header.JOURNAL_ENTRY_ID};if(fault=="count")command=command with{EntryIds=command.EntryIds.Take(1).ToArray()};if(fault=="duplicate")command=command with{EntryIds=command.EntryIds.Select(_=>command.EntryIds[0]).ToArray()};if(fault=="reason")command=command with{Reason=""};
        Assert.ThrowsAny<Exception>(()=>command.Require());Assert.ThrowsAny<Exception>(()=>command.CreateEntry(0));
    }
    [Fact] public async Task ScopedImmutableValidationNeverLendsSuccessToChangedNativeMoneyOrPostingIdentities()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var (_,command)=await Approved();command.Require();command.Require();var altered=CompanyInventorySourceReview.Copy(command);altered.Source.Receivable.TOTAL_AMOUNT+=.001m;Assert.Throws<InvalidOperationException>(()=>altered.Require());var ids=command with{EntryIds=command.EntryIds.Select(_=>command.EntryIds[0]).ToArray()};Assert.Throws<InvalidOperationException>(()=>ids.Require());command.Require();
    }
}
#endif
