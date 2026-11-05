using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanySalesJournalPreparationOutcome> PrepareCompanySalesJournalAsync(CompanySalesJournalPreparation command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=command.Source.Company || original.Selection.UserId!=command.Source.ActorUserId)throw new InvalidOperationException("Return to the original company/account to reconcile this journal request.");
        var result=await _apiClient.PostAsync<CompanySalesJournalPreparation,CompanySalesJournalPreparationOutcome>(CompanySalesReceivableRoute+"/journal-preparations",command) ??throw new InvalidOperationException("Journal preparation is unconfirmed. Retain and reconcile the original request.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during journal preparation. Retain the original request.");result.Require(command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanySalesJournalPreparationOutcome?> GetCompanySalesJournalPreparationAsync(string book,string currency,string id)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original journal company/account.");_=CompanyGLAccountReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,[]);
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original journal identity is required.");
        var result=await _apiClient.GetAsync<CompanySalesJournalPreparationOutcome>(CompanySalesReceivableRoute+"/journal-preparations/"+Uri.EscapeDataString(id)+"/recovery?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during original journal recovery.");if(result is null)return null;
        var command=CompanySalesJournalPreparationProof.Require(result.Header).Command;result.Require(command);
        if(command.Source.Company!=original.Selection.Scope || command.Source.ActorUserId!=original.Selection.UserId || command.Source.BookId!=book || command.Source.CurrencyCode!=currency || command.JournalId!=id)throw new InvalidOperationException("The original journal preparation recovery is unconfirmed.");return CompanyInventorySourceReview.Copy(result);
    }
}
