using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class AccountingServiceClient
{
    public async Task<CompanyDeliveryCorrectionReview> GetDeliveryCorrectionReviewAsync(string instance)
    {
        if(string.IsNullOrWhiteSpace(instance) || instance.Length>128 || instance.Any(char.IsControl))throw new ArgumentException("The bounded original process identity is required.");var original=_apiClient.CompanySnapshot;if(original.Selection is not {} selection)throw new InvalidOperationException("Select the original company/account.");
        var result=await _apiClient.GetAsync<CompanyDeliveryCorrectionReview>("api/accounting/processes/"+Uri.EscapeDataString(instance)+"/delivery-correction-review") ??throw new InvalidOperationException("The original rejected receiving review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The correction company/account changed.");result.Require(selection.Scope,selection.UserId,instance);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueDeliveryAcceptanceRecord> CorrectDeliveryAcceptanceAsync(CompanyDeliveryCorrectionSubmission command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection?.Scope!=command.Review.Company || original.Selection.UserId!=command.Review.ActorUserId)throw new InvalidOperationException("The original correction company/account are required.");
        var result=await _apiClient.PostAsync<CompanyDeliveryCorrectionSubmission,RevenueDeliveryAcceptanceRecord>("api/accounting/processes/"+Uri.EscapeDataString(command.Review.InstanceId)+"/delivery-correction",command) ??throw new InvalidOperationException("Correction is uncertain. Retain and reconcile the original command.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("The correction context changed. Retain the original command.");result.Require();
        if(JsonSerializer.Serialize(result.Preparation)!=JsonSerializer.Serialize(command.Correction.Preparation))throw new InvalidOperationException("The correction acknowledgement differs from its original intent.");return CompanyInventorySourceReview.Copy(result);
    }
}
