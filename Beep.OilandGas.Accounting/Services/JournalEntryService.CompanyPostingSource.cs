using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalPostingSource> GetCompanySalesJournalPostingSourceAsync(CompanyScope company,string actor,string book,string currency,string id,ARService ar,Func<Task<CompanySalesJournalSourceReview>> readCurrent,Func<Task> requireAuthority,Func<string,Task> requireFieldRead,bool afterPosting=false,bool close=true)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(ar);ArgumentNullException.ThrowIfNull(readCurrent);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound PRODUCTION posting source owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original posting source binding is unavailable.");var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original posting source datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original journal posting source binding changed.");await ar.RequireJournalSourceBindingAsync(connection,source);RequireCompanyJournalReviewSchema(source);}
        async Task<CompanySalesJournalPreparationOutcome> Journal(){await Guard();var value=await GetCompanySalesJournalForReviewAsync(company,actor,book,currency,id,Guard,requireFieldRead)??throw new InvalidOperationException("The original independently approved journal is unavailable.");await Guard();if(afterPosting && value.Header.STATUS=="POSTED")value=new(CompanySalesJournalPostingHeaderProof.OriginalReviewedHeader(value.Header),value.Lines);if(value.Header.STATUS!="APPROVED")throw new InvalidOperationException("Only an independently approved journal can supply a posting source.");return value;}
        async Task<CompanySalesReceivableOutcome> Receivable(string key){await Guard();var value=await ar.GetCompanySalesReceivableForReviewAsync(company,key,Guard,requireFieldRead)??throw new InvalidOperationException("The exact native posting receivable is unavailable.");await Guard();return value;}
        async Task<CompanySalesJournalSourceReview> Current(){await Guard();var value=await readCurrent();await Guard();await requireFieldRead(value.FieldId);await Guard();return value;}
        async Task<CompanySalesJournalPostingSource> Capture(bool closing,string? receivable=null)
        {
            CompanySalesJournalPreparationOutcome journal;CompanySalesReceivableOutcome actual;CompanySalesJournalSourceReview current;
            if(closing){current=await Current();actual=await Receivable(receivable!);journal=await Journal();}
            else{journal=await Journal();actual=await Receivable(journal.Header.SOURCE_AR_INVOICE_ID!);current=await Current();}
            if(actual.Receivable.AR_INVOICE_ID!=journal.Header.SOURCE_AR_INVOICE_ID)throw new InvalidOperationException("The original journal and native receivable correlation changed.");var result=CompanySalesJournalPostingSource.Create(actor,journal,current,actual.Receivable);
            if(result.Current.Company!=company || result.Current.BookId!=book || result.Current.CurrencyCode!=currency || result.Reviewed.Header.JOURNAL_ENTRY_ID!=id)throw new InvalidOperationException("The original posting company/book/currency/journal context changed.");await requireFieldRead(result.Current.FieldId);await Guard();return result;
        }
        var initial=await Capture(false);if(!close){await Guard();return CompanyInventorySourceReview.Copy(initial);}var closing=await Capture(true,initial.Current.ReceivableId);initial.RequireCurrent(closing);await Guard();return CompanyInventorySourceReview.Copy(closing);
    }
}
