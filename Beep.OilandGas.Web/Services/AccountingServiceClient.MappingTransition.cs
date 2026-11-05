using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanyGLMappingTransitionOutcome> TransitionCompanyGLMappingAsync(CompanyGLMappingTransitionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);submission=CompanyInventorySourceReview.Copy(submission);submission.Command.Require();var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null || snapshot.Selection.Scope!=submission.Command.Company || snapshot.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original mapping transition company/account.");
        var result=await _apiClient.PostAsync<CompanyGLMappingTransitionSubmission,CompanyGLMappingTransitionOutcome>(MappingConfigurationRoute+"/transitions",submission) ??throw new InvalidOperationException("The original transition is unconfirmed. Retain its command.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during transition confirmation.");result.Require(submission.Command);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyGLMappingTransitionOutcome?> GetCompanyGLMappingTransitionAsync(string book,string currency,string id)
    {
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original mapping identity is required.");var snapshot=_apiClient.CompanySnapshot;if(snapshot.Selection is null)throw new InvalidOperationException("Select the original transition company/account.");
        var result=await _apiClient.GetAsync<CompanyGLMappingTransitionOutcome>(MappingConfigurationRoute+"/transitions/"+Uri.EscapeDataString(id)+"?book="+Uri.EscapeDataString(book)+"&currency="+Uri.EscapeDataString(currency));
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The company/account changed during transition recovery.");if(result is null)return null;result.Require(result.Command);
        if(result.Command.Company!=snapshot.Selection.Scope || result.Command.ActorUserId!=snapshot.Selection.UserId || result.Command.BookId!=book || result.Command.CurrencyCode!=currency || result.Command.MappingId!=id)throw new InvalidOperationException("The original private transition belongs to another context.");return CompanyInventorySourceReview.Copy(result);
    }
}
