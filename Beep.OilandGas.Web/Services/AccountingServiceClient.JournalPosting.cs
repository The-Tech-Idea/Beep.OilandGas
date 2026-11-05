using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private static string JournalPostingRoute(string book,string currency,string id,string operation)
    {
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original journal posting identity is required.");return CompanySalesReceivableRoute+"/journal-postings/"+Uri.EscapeDataString(id)+"/"+operation+"?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency);
    }
    public async Task<CompanySalesJournalPostingSource> GetCompanySalesJournalPostingSourceAsync(string book,string currency,string id)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original posting company/account.");_=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);
        var result=await _apiClient.GetAsync<CompanySalesJournalPostingSource>(JournalPostingRoute(book,currency,id,"source"))??throw new InvalidOperationException("The exact independently reviewed native posting source is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed while loading posting source.");result.Require();
        if(result.ActorUserId!=selected.UserId || result.Current.Company!=selected.Scope || result.Current.BookId!=book || result.Current.CurrencyCode!=currency || result.Reviewed.Header.JOURNAL_ENTRY_ID!=id)throw new InvalidOperationException("The posting source belongs to another original context.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanySalesJournalPostingOutcome> PostCompanySalesJournalAsync(CompanySalesJournalPostingCommand command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.UserId!=command.Source.ActorUserId || selected.Scope!=command.Source.Current.Company)throw new InvalidOperationException("Return to the original posting producer/company/account.");var result=await _apiClient.PostAsync<CompanySalesJournalPostingCommand,CompanySalesJournalPostingOutcome>(CompanySalesReceivableRoute+"/journal-postings",command)??throw new InvalidOperationException("Posting is unconfirmed. Retain and reconcile the exact original request.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during posting. Retain the original request.");result.Require(command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanySalesJournalPostingRecovery> GetCompanySalesJournalPostingRecoveryAsync(string book,string currency,string id)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original posting producer/company/account.");_=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);
        var result=await _apiClient.GetAsync<CompanySalesJournalPostingRecovery>(JournalPostingRoute(book,currency,id,"recovery"))??throw new InvalidOperationException("The original native posting recovery is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during posting recovery.");result.Require();
        if(result.Company!=selected.Scope || result.ActorUserId!=selected.UserId || result.JournalId!=id || result.Intent is {} intent && (intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency))throw new InvalidOperationException("The original posting recovery belongs to another company/account/book/currency/journal.");return CompanyInventorySourceReview.Copy(result);
    }
}
