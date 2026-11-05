using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPostingSourceTests
{
    [Fact] public async Task ActualPostingSourceConfirmsReviewedJournalNativeARAndLivePeerReadUnderOneBindingWithoutWrites()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();CompanySalesJournalPreparationHttpTests.Writes(fixture);var maker=await fixture.Read();var preparation=new CompanySalesJournalPreparation(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),maker,"explicit-posting-source-journal","Original balanced source",maker.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray());var draft=await fixture.Journal.PrepareCompanySalesJournalAsync(preparation,fixture.Guard,fixture.Read);
        var types=fixture.Native.Rows.Select(value=>value.GetType()).Concat([typeof(SALES_JOURNAL_REVIEW_INTENT)]).Distinct().ToDictionary(type=>type==typeof(GLAccountMapping)?"GL_ACCOUNT_MAPPING":type.Name,type=>type);fixture.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(fixture.Native.Editor.Object).ConvertToEntityStructure(types[request.EntityName]));fixture.Native.Source.Setup(value=>value.InsertEntity("SALES_JOURNAL_REVIEW_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{fixture.Native.Writes++;fixture.Native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_JOURNAL_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
        var reviewer="independent-reviewer";var reviewSource=CompanySalesJournalReviewSource.Create(reviewer,draft,maker);var review=new CompanySalesJournalReviewCommand(Guid.NewGuid().ToString("N"),reviewSource,"APPROVE","CONFIRMED","Exact balanced lines","CONFIRMED","Original approved source","Independent journal approval");
        await fixture.Journal.RecordCompanySalesJournalReviewAsync(review,fixture.Guard,()=>Task.FromResult(CompanyInventorySourceReview.Copy(reviewSource)));fixture.Native.Writes=0;fixture.Native.Transactions.Clear();fixture.Receivable.Actor="authorized-poster";var fields=0;bool denied=false,changed=false;
        Task Guard(){Assert.Equal("authorized-poster",fixture.Receivable.Actor);if(denied)throw new UnauthorizedAccessException();return fixture.Guard();}
        Task Field(string id){Assert.Equal(maker.FieldId,id);fields++;return Guard();}
        async Task<CompanySalesJournalSourceReview> Current(){var current=await fixture.Journal.GetCompanySalesJournalSourceAsync(maker.Company,fixture.Receivable.Actor,maker.ReceivableId,maker.BookId,maker.CurrencyCode,maker.CalendarId,maker.PostingDate,maker.Selection,fixture.Receivable.Data.Owner,fixture.Mappings,fixture.Periods,fixture.Receivable.Current,Guard,Field,maker.ActorUserId);return changed?current with{Revision=new string('C',64)}:current;}
        Task<CompanySalesJournalPostingSource> Read()=>fixture.Journal.GetCompanySalesJournalPostingSourceAsync(maker.Company,fixture.Receivable.Actor,maker.BookId,maker.CurrencyCode,preparation.JournalId,fixture.Receivable.Data.Owner,Current,Guard,Field);
        var source=await Read();source.Require();Assert.Equal("authorized-poster",source.ActorUserId);Assert.Equal(JsonSerializer.Serialize(maker),JsonSerializer.Serialize(source.Current));Assert.Equal(JsonSerializer.Serialize(fixture.Native.Rows.OfType<AR_INVOICE>().Single()),JsonSerializer.Serialize(source.Receivable));Assert.True(fields>0);fixture.NoWrites();
        denied=true;await Assert.ThrowsAsync<UnauthorizedAccessException>(Read);denied=false;
        var intent=fixture.Native.Rows.OfType<SALES_JOURNAL_REVIEW_INTENT>().Single();fixture.Native.Rows.Remove(intent);await Assert.ThrowsAsync<InvalidOperationException>(Read);fixture.Native.Rows.Add(intent);
        var receivable=fixture.Native.Rows.OfType<AR_INVOICE>().Single();var total=receivable.TOTAL_AMOUNT;receivable.TOTAL_AMOUNT+=.001m;await Assert.ThrowsAnyAsync<Exception>(Read);receivable.TOTAL_AMOUNT=total;
        changed=true;await Assert.ThrowsAsync<InvalidOperationException>(Read);fixture.NoWrites();
    }
}
#endif
