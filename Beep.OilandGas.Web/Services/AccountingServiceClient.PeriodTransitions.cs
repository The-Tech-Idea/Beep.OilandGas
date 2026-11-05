using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private const string PeriodStateRoute="api/accounting/traditional/gl/company-accounts/periods";
    public async Task<CompanyLedgerPeriodConfigurationReview> GetCompanyPeriodTransitionReviewAsync(string book,string currency,string calendar)
    {
        var snapshot=_apiClient.CompanySnapshot;if(snapshot.Selection is null)throw new InvalidOperationException("Select the original period company/account.");var result=await _apiClient.GetAsync<CompanyLedgerPeriodConfigurationReview>(PeriodStateRoute+"/transition-review?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)+"&calendar="+Uri.EscapeDataString(calendar)) ??throw new InvalidOperationException("The complete native period transition source is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during period review.");result.Require(snapshot.Selection.Scope,snapshot.Selection.UserId,book,currency,calendar);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyLedgerPeriodTransitionOutcome> TransitionCompanyPeriodAsync(CompanyLedgerPeriodTransitionSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var snapshot=_apiClient.CompanySnapshot;if(snapshot.Selection is null || snapshot.Selection.Scope!=submission.Command.Company || snapshot.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original period transition company/account.");
        var result=await _apiClient.PostAsync<CompanyLedgerPeriodTransitionSubmission,CompanyLedgerPeriodTransitionOutcome>(PeriodStateRoute+"/transitions",submission) ??throw new InvalidOperationException("The period transition is unconfirmed. Retain its original command.");if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during period confirmation.");result.Require(submission.Command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyLedgerPeriodTransitionOutcome?> GetCompanyPeriodTransitionAsync(string book,string currency,string calendar,string periodId,string intentId)
    {
        foreach(var id in new[]{periodId,intentId})if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original period and transition identities are required.");var snapshot=_apiClient.CompanySnapshot;if(snapshot.Selection is null)throw new InvalidOperationException("Select the original period company/account.");
        var result=await _apiClient.GetAsync<CompanyLedgerPeriodTransitionOutcome>(PeriodStateRoute+"/transitions/"+Uri.EscapeDataString(periodId)+"/"+Uri.EscapeDataString(intentId)+"?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)+"&calendar="+Uri.EscapeDataString(calendar));if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during period recovery.");if(result is null)return null;result.Require(result.Command);
        if(result.Command.Company!=snapshot.Selection.Scope || result.Command.ActorUserId!=snapshot.Selection.UserId || result.Command.BookId!=book || result.Command.CurrencyCode!=currency || result.Command.CalendarId!=calendar || result.Command.PeriodId!=periodId || result.Command.IntentId!=intentId)throw new InvalidOperationException("The original private period transition belongs to another context.");return CompanyInventorySourceReview.Copy(result);
    }
}
