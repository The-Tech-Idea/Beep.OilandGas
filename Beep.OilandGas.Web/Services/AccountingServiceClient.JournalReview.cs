using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private static string JournalReviewRoute(string book,string currency,string id)
    {
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original journal identity is required.");
        return CompanySalesReceivableRoute+"/journal-reviews/"+Uri.EscapeDataString(id)+"/{0}?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency);
    }
    public async Task<CompanySalesJournalReviewSource> GetCompanySalesJournalReviewAsync(string book,string currency,string id)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original company/account.");_=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);
        var result=await _apiClient.GetAsync<CompanySalesJournalReviewSource>(JournalReviewRoute(book,currency,id).Replace("{0}","source"))??throw new InvalidOperationException("The complete independent journal review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed while loading journal review.");result.Require();
        if(result.ActorUserId!=selected.UserId || result.Current.Company!=selected.Scope || result.Current.BookId!=book || result.Current.CurrencyCode!=currency || result.Original.Header.JOURNAL_ENTRY_ID!=id)throw new InvalidOperationException("Journal review belongs to another original context.");return CompanyInventorySourceReview.Copy(result);
    }
    public static void RequireJournalDecision(CompanySalesJournalPreparationOutcome result,CompanySalesJournalReviewCommand command)
    {
        command.Require();result.Require(CompanySalesJournalPreparationProof.Require(command.Review.Original.Header).Command);
        if(JsonSerializer.Serialize(CompanySalesJournalReviewProof.Require(result.Header).Intent.Restore())!=JsonSerializer.Serialize(command))throw new InvalidOperationException("The native journal disposition does not confirm the exact original review request.");
    }
    public async Task<CompanySalesJournalPreparationOutcome> RecordCompanySalesJournalDecisionAsync(CompanySalesJournalReviewCommand command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null || snapshot.Selection.Scope!=command.Review.Current.Company || snapshot.Selection.UserId!=command.Review.ActorUserId)throw new InvalidOperationException("Return to the original reviewer company/account.");
        var result=await _apiClient.PostAsync<CompanySalesJournalReviewCommand,CompanySalesJournalPreparationOutcome>(CompanySalesReceivableRoute+"/journal-reviews",command)??throw new InvalidOperationException("Journal review is unconfirmed. Retain the exact original request.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during journal review. Retain the original request.");RequireJournalDecision(result,command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanySalesJournalReviewRecovery> GetCompanySalesJournalReviewRecoveryAsync(string book,string currency,string id)
    {
        var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection??throw new InvalidOperationException("Select the original reviewer company/account.");_=CompanyGLAccountReview.Create(selected.Scope,selected.UserId,book,currency,[]);
        var result=await _apiClient.GetAsync<CompanySalesJournalReviewRecovery>(JournalReviewRoute(book,currency,id).Replace("{0}","recovery"))??throw new InvalidOperationException("Original journal reviewer recovery is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during journal review recovery.");result.Require();
        if(result.Company!=selected.Scope || result.ActorUserId!=selected.UserId || result.JournalId!=id || result.Intent is {} intent && (intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency))throw new InvalidOperationException("Original journal review recovery belongs to another context.");return CompanyInventorySourceReview.Copy(result);
    }
}
