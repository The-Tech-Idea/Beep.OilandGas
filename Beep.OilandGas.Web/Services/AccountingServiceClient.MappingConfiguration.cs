using Beep.OilandGas.Accounting.Models;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private const string MappingConfigurationRoute="api/accounting/traditional/gl/company-accounts/mappings";
    public async Task<CompanyGLMappingConfigurationReview> GetCompanyGLMappingConfigurationReviewAsync(string book,string currency,DateTime date)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original mapping company/account.");CompanyGLMappingReview.RequireDate(date);
        var review=await _apiClient.GetAsync<CompanyGLMappingConfigurationReview>(MappingConfigurationRoute+"/configuration-review?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency)+"&date="+date.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)) ??throw new InvalidOperationException("The complete mapping configuration review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during mapping review.");review.Require(original.Selection.Scope,original.Selection.UserId,book,currency,date);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<GLAccountMapping> ConfigureCompanyGLMappingAsync(CompanyGLMappingConfigurationSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original company/account to reconcile mapping configuration.");
        var result=await _apiClient.PostAsync<CompanyGLMappingConfigurationSubmission,GLAccountMapping>(MappingConfigurationRoute+"/configuration",submission) ??throw new InvalidOperationException("The original mapping outcome is unconfirmed.");
        if(original!=_apiClient.CompanySnapshot || CompanyGLMappingConfigurationProof.Require(result).Command!=submission.Command)throw new InvalidOperationException("The original mapping outcome is unconfirmed. Retain the original request.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyGLMappingConfigurationCommand?> GetCompanyGLMappingConfigurationRequestAsync(string book,string currency,string id)
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original mapping company/account.");
        var command=await _apiClient.GetAsync<CompanyGLMappingConfigurationCommand>(MappingConfigurationRoute+"/configuration/"+Uri.EscapeDataString(id)+"/request?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company/account changed during original mapping recovery.");
        if(command is not null){command.Require();if(command.Company!=original.Selection.Scope || command.ActorUserId!=original.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.MappingId!=id)throw new InvalidOperationException("The original mapping request is unconfirmed.");}return command;
    }
}
