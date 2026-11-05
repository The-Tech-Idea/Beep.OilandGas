using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<CompanyDeliverySavedWorkPage> GetDeliverySavedPageAsync(int page,int size)
    {
        DELIVERY_ACCEPTANCE_INTENT.RequirePage(page,size);var original=_apiClient.CompanySnapshot;
        if(original.Selection is not {} selection)throw new InvalidOperationException("Select the original delivery recovery company/account.");
        var result=await _apiClient.GetAsync<CompanyDeliverySavedWorkPage>("api/accounting/delivery-intents?page="+page.ToString(System.Globalization.CultureInfo.InvariantCulture)+"&size="+size.ToString(System.Globalization.CultureInfo.InvariantCulture)) ??throw new InvalidOperationException("The complete delivery recovery page is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Delivery recovery page context changed.");
        result.Require(selection.Scope,selection.UserId,page,size);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyDeliverySavedWorkReview> GetDeliverySavedIntentAsync(string intent)
    {
        CompanyDeliverySavedWorkReview.RequireIntentId(intent);var original=_apiClient.CompanySnapshot;
        if(original.Selection is not {} selection)throw new InvalidOperationException("Select the original saved delivery company/account.");
        var result=await _apiClient.GetAsync<CompanyDeliverySavedWorkReview>("api/accounting/delivery-intents/"+Uri.EscapeDataString(intent)) ??throw new InvalidOperationException("The original saved delivery review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Saved delivery recovery context changed.");
        result.Require(selection.Scope,selection.UserId,intent);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueDeliveryAcceptanceSavedReview> GetDeliverySavedReviewAsync(string field,DateTime from,DateTime through,string transaction)
    {
        FieldRevenueSourceReview.RequireWindow(field,from,through);var original=_apiClient.CompanySnapshot;
        if(original.Selection is not {} selection)throw new InvalidOperationException("Select the original delivery company/account.");
        var route="api/fields/"+Uri.EscapeDataString(field)+"/accounting/revenue-source/delivery/acceptance/saved?from="+from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&transactionId="+Uri.EscapeDataString(transaction);
        var result=await _apiClient.GetAsync<RevenueDeliveryAcceptanceSavedReview>(route) ??throw new InvalidOperationException("Saved delivery evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Saved delivery company/account changed.");
        result.Require(selection.Scope,selection.UserId,field,from,through,transaction);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<DELIVERY_ACCEPTANCE_INTENT> SaveDeliveryIntentAsync(RevenueDeliveryAcceptanceIntent command)
    {
        command=CompanyInventorySourceReview.Copy(command);command.RequireExecution();var original=_apiClient.CompanySnapshot;
        if(original.Selection?.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)throw new InvalidOperationException("The original delivery journal company/account are required.");
        var result=command.Correction is {} correction?
            await _apiClient.PostAsync<CompanyDeliveryCorrectionSubmission,DELIVERY_ACCEPTANCE_INTENT>("api/accounting/processes/"+Uri.EscapeDataString(correction.Review.InstanceId)+"/delivery-correction-intent",correction):
            await _apiClient.PostAsync<RevenueDeliveryAcceptanceIntent,DELIVERY_ACCEPTANCE_INTENT>("api/fields/"+Uri.EscapeDataString(command.Original.Source.Source.Source.FieldId)+"/accounting/revenue-source/delivery/acceptance/intent",command);
        if(result is null)throw new InvalidOperationException("Saving delivery intent is uncertain. Retain the original command.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Delivery journal context changed. Retain the original command.");
        if(System.Text.Json.JsonSerializer.Serialize(result.Restore(command.Company,command.ActorUserId,command.ScopeKey))!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The saved delivery acknowledgement differs from the original command.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueDeliveryAcceptanceReadReview> GetDeliveryAcceptanceAsync(string field,DateTime from,DateTime through,string transaction)
    {
        FieldRevenueSourceReview.RequireWindow(field,from,through);var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original delivery company and account.");
        var review=await _apiClient.GetAsync<RevenueDeliveryAcceptanceReadReview>("api/fields/"+Uri.EscapeDataString(field)+"/accounting/revenue-source/delivery/acceptance?from="+from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&transactionId="+Uri.EscapeDataString(transaction)) ??throw new InvalidOperationException("Delivery acceptance evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Original delivery account/company changed.");
        var source=review.Source.Source.Source;review.Source.Source.Require(original.Selection.Scope,original.Selection.UserId,field,from,through,transaction,source.Revision);review.Require();return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<RevenueDeliveryAcceptanceRecord> PrepareDeliveryAcceptanceAsync(RevenueDeliveryAcceptancePreparation preparation)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();preparation.Facts.RequireQuantity(preparation.Source);var original=_apiClient.CompanySnapshot;
        if(original.Selection?.Scope!=preparation.Company || original.Selection.UserId!=preparation.ActorUserId)throw new InvalidOperationException("Original delivery company/account are required.");
        var result=await _apiClient.PostAsync<RevenueDeliveryAcceptancePreparation,RevenueDeliveryAcceptanceRecord>("api/fields/"+Uri.EscapeDataString(preparation.Source.Source.Source.FieldId)+"/accounting/revenue-source/delivery/acceptance/prepare",preparation) ??throw new InvalidOperationException("Recorded delivery preparation is uncertain.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Delivery context changed. Retain the original intent.");
        result.Require();if(System.Text.Json.JsonSerializer.Serialize(result.Preparation)!=System.Text.Json.JsonSerializer.Serialize(preparation))throw new InvalidOperationException("The recorded delivery preparation differs from the original intent.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueDeliveryAcceptanceRecord> DecideDeliveryAcceptanceAsync(RevenueDeliveryAcceptanceSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Command.RequireExecution(submission.Preparation);var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection?.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)throw new InvalidOperationException("Original delivery reviewer company/account are required.");
        var result=await _apiClient.PostAsync<RevenueDeliveryAcceptanceSubmission,RevenueDeliveryAcceptanceRecord>("api/fields/"+Uri.EscapeDataString(submission.Preparation.Preparation.Source.Source.Source.FieldId)+"/accounting/revenue-source/delivery/acceptance/decide",submission) ??throw new InvalidOperationException("Recorded delivery decision is uncertain.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Delivery context changed. Retain the original decision intent.");
        result.Require();if(result.Decision!=command || result.PreparedAtUtc!=submission.Preparation.PreparedAtUtc || System.Text.Json.JsonSerializer.Serialize(result.Preparation)!=System.Text.Json.JsonSerializer.Serialize(submission.Preparation.Preparation))throw new InvalidOperationException("Recorded delivery decision differs from its original intent.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueDeliverySourceReview> GetRevenueDeliveryAsync(FieldRevenueSourceReview source,string transactionId)
    {
        source=CompanyInventorySourceReview.Copy(source);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original delivery company and account.");
        source.Require(original.Selection.Scope,original.Selection.UserId,source.FieldId,source.From,source.Through);
        if(!source.Rows.Any(row=>row.REVENUE_TRANSACTION_ID==transactionId && !string.IsNullOrWhiteSpace(row.CONTRACT_ID) && !string.IsNullOrWhiteSpace(row.ALLOCATION_RESULT_ID)))throw new InvalidOperationException("Select recorded contract and allocation references.");
        var route="api/fields/"+Uri.EscapeDataString(source.FieldId)+"/accounting/revenue-source/delivery?from="+source.From.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+source.Through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&transactionId="+Uri.EscapeDataString(transactionId)+"&sourceRevision="+Uri.EscapeDataString(source.Revision);
        var result=await _apiClient.GetAsync<RevenueDeliverySourceReview>(route) ??throw new InvalidOperationException("Recorded delivery evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Delivery company or account changed.");
        result.Source.Require(original.Selection.Scope,original.Selection.UserId,source.FieldId,source.From,source.Through,transactionId,source.Revision);result.Require(result.Source);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<RevenueSalesContractReview> GetRevenueSalesContractAsync(FieldRevenueSourceReview source,string transactionId)
    {
        source=CompanyInventorySourceReview.Copy(source);
        var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original revenue company and account.");
        source.Require(original.Selection.Scope,original.Selection.UserId,source.FieldId,source.From,source.Through);
        if(!source.Rows.Any(row=>row.REVENUE_TRANSACTION_ID==transactionId && !string.IsNullOrWhiteSpace(row.CONTRACT_ID)))throw new InvalidOperationException("Select a recorded revenue transaction with a contract reference.");
        var route="api/fields/"+Uri.EscapeDataString(source.FieldId)+"/accounting/revenue-source/sales-contract?from="+source.From.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+source.Through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&transactionId="+Uri.EscapeDataString(transactionId)+"&sourceRevision="+Uri.EscapeDataString(source.Revision);
        var result=await _apiClient.GetAsync<RevenueSalesContractReview>(route) ??throw new InvalidOperationException("Recorded sales contract is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Revenue company or account changed during contract review.");
        result.Require(original.Selection.Scope,original.Selection.UserId,source.FieldId,source.From,source.Through,transactionId,source.Revision);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<FieldRevenueSourceReview> GetFieldRevenueSourceAsync(string fieldId,DateTime from,DateTime through)
    {
        FieldRevenueSourceReview.RequireWindow(fieldId,from,through);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original revenue company and account.");
        var result=await _apiClient.GetAsync<FieldRevenueSourceReview>("api/fields/"+Uri.EscapeDataString(fieldId)+"/accounting/revenue-source?from="+from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture))
            ??throw new InvalidOperationException("Recorded revenue source is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Revenue company or account changed during the read.");
        result.Require(original.Selection.Scope,original.Selection.UserId,fieldId,from,through);return CompanyInventorySourceReview.Copy(result);
    }
}
