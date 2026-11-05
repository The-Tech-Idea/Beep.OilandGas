using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanyGLAccountConfigurationReview> GetCompanyGLAccountConfigurationReviewAsync(string book,string currency)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original ledger company/account.");
        _=CompanyGLAccountReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,[]);
        var review=await _apiClient.GetAsync<CompanyGLAccountConfigurationReview>("api/accounting/traditional/gl/company-accounts/configuration-review?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)) ??throw new InvalidOperationException("The complete configuration review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during configuration review.");review.Require(original.Selection.Scope,original.Selection.UserId,book,currency);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<GL_ACCOUNT> ConfigureCompanyGLAccountAsync(CompanyGLAccountConfigurationSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();
        var original=_apiClient.CompanySnapshot;if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original company/account to reconcile configuration.");
        var result=await _apiClient.PostAsync<CompanyGLAccountConfigurationSubmission,GL_ACCOUNT>("api/accounting/traditional/gl/company-accounts/configuration",submission) ??throw new InvalidOperationException("The configuration outcome is unconfirmed. Retain the original request.");
        if(original!=_apiClient.CompanySnapshot || CompanyGLAccountConfigurationProof.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original configuration outcome is unconfirmed. Retain and reconcile the original request.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyGLAccountConfigurationCommand?> GetCompanyGLAccountConfigurationRequestAsync(string book,string currency,string id)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original configuration company/account.");
        var command=await _apiClient.GetAsync<CompanyGLAccountConfigurationCommand>("api/accounting/traditional/gl/company-accounts/configuration/"+Uri.EscapeDataString(id)+"/request?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during original-request recovery.");
        if(command is not null){command.Require();if(command.Company!=original.Selection.Scope || command.ActorUserId!=original.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.AccountId!=id)throw new InvalidOperationException("The original configuration request is unconfirmed.");}return command;
    }
    public async Task<CompanyGLMappingReview> GetCompanyGLMappingsAsync(string book,string currency,DateTime date)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original ledger company and account.");
        _=CompanyGLAccountReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,[]);CompanyGLMappingReview.RequireDate(date);
        var review=await _apiClient.GetAsync<CompanyGLMappingReview>("api/accounting/traditional/gl/company-accounts/mappings?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)+
            "&date="+Uri.EscapeDataString(date.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture))) ??throw new InvalidOperationException("The complete company mapping review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during mapping review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,book,currency,date);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<CompanyGLAccountReview> GetCompanyGLAccountsAsync(string book,string currency)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original ledger company and account.");
        _=CompanyGLAccountReview.Create(original.Selection.Scope,original.Selection.UserId,book,currency,[]);
        var review=await _apiClient.GetAsync<CompanyGLAccountReview>("api/accounting/traditional/gl/company-accounts?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency))
            ??throw new InvalidOperationException("Company ledger account configuration is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during ledger configuration review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,book,currency);return CompanyInventorySourceReview.Copy(review);
    }
}
