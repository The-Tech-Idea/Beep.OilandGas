using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private static string PeriodQuery(string book,string currency,string calendar)=>"?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)+"&calendar="+Uri.EscapeDataString(calendar);
    public async Task<CompanyLedgerPeriodSource> GetCompanyLedgerPeriodsAsync(string book,string currency,string calendar)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original ledger company/account.");_=CompanyLedgerPeriodReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,calendar,[]);
        var source=await _apiClient.GetAsync<CompanyLedgerPeriodSource>("api/accounting/traditional/gl/company-accounts/periods"+PeriodQuery(book,currency,calendar)) ??throw new InvalidOperationException("The complete recorded company periods are unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during period review.");source.Require(original.Selection.Scope,original.Selection.UserId,book,currency,calendar);return CompanyInventorySourceReview.Copy(source);
    }
    public async Task<CompanyLedgerPeriodConfigurationReview> GetCompanyLedgerPeriodConfigurationReviewAsync(string book,string currency,string calendar)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original period company/account.");_=CompanyLedgerPeriodReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,calendar,[]);
        var review=await _apiClient.GetAsync<CompanyLedgerPeriodConfigurationReview>("api/accounting/traditional/gl/company-accounts/periods/configuration-review"+PeriodQuery(book,currency,calendar)) ??throw new InvalidOperationException("The complete private period review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during period configuration review.");review.Require(original.Selection.Scope,original.Selection.UserId,book,currency,calendar);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<COMPANY_LEDGER_PERIOD> CreateCompanyLedgerPeriodAsync(CompanyLedgerPeriodCreationSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original period company/account to reconcile.");
        var result=await _apiClient.PostAsync<CompanyLedgerPeriodCreationSubmission,COMPANY_LEDGER_PERIOD>("api/accounting/traditional/gl/company-accounts/periods/configuration",submission) ??throw new InvalidOperationException("The period outcome is unconfirmed. Retain the original request.");
        if(original!=_apiClient.CompanySnapshot || CompanyLedgerPeriodAuthority.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original period outcome is unconfirmed. Retain and reconcile its original request.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyLedgerPeriodCreation?> GetCompanyLedgerPeriodCreationRequestAsync(string book,string currency,string calendar,string id)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original period company/account.");
        var command=await _apiClient.GetAsync<CompanyLedgerPeriodCreation>("api/accounting/traditional/gl/company-accounts/periods/configuration/"+Uri.EscapeDataString(id)+"/request"+PeriodQuery(book,currency,calendar));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during period request recovery.");
        if(command is not null){command.Require();if(command.Company!=original.Selection.Scope || command.ActorUserId!=original.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.CalendarId!=calendar || command.PeriodId!=id)throw new InvalidOperationException("The original period request is unconfirmed.");}return command;
    }
}
