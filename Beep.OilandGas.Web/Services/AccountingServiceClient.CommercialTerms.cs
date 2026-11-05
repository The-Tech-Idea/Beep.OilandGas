using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<SalesContractTermsReadReview> GetSalesContractTermsAsync(string fieldId,DateTime from,DateTime through,string transactionId)
    {
        FieldRevenueSourceReview.RequireWindow(fieldId,from,through);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original commercial company and account.");
        var route="api/fields/"+Uri.EscapeDataString(fieldId)+"/accounting/commercial-terms?from="+from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&through="+through.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)+"&transactionId="+Uri.EscapeDataString(transactionId);
        var review=await _apiClient.GetAsync<SalesContractTermsReadReview>(route) ??throw new InvalidOperationException("Commercial terms review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Commercial company or account changed during review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,fieldId,from,through,transactionId);return CompanyInventorySourceReview.Copy(review);
    }
    public async Task<SalesContractTermsRecord> PrepareSalesContractTermsAsync(SalesContractTermsPreparation preparation)
    {
        preparation=CompanyInventorySourceReview.Copy(preparation);preparation.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection?.Scope!=preparation.Company || original.Selection.UserId!=preparation.ActorUserId)throw new InvalidOperationException("Original commercial company and account are required.");
        var result=await _apiClient.PostAsync<SalesContractTermsPreparation,SalesContractTermsRecord>("api/fields/"+Uri.EscapeDataString(preparation.Source.Source.FieldId)+"/accounting/commercial-terms/prepare",preparation) ??throw new InvalidOperationException("Recorded commercial preparation is uncertain.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Commercial context changed. Retain the original preparation intent.");
        result.Require();if(JsonSerializer.Serialize(result.Preparation)!=JsonSerializer.Serialize(preparation))throw new InvalidOperationException("The recorded commercial preparation differs from its original intent.");
        return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<SalesContractTermsRecord> DecideSalesContractTermsAsync(SalesContractTermsDecisionSubmission submission)
    {
        submission=CompanyInventorySourceReview.Copy(submission);submission.Require();var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection?.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)throw new InvalidOperationException("Original commercial reviewer company and account are required.");
        var result=await _apiClient.PostAsync<SalesContractTermsDecisionSubmission,SalesContractTermsRecord>("api/fields/"+Uri.EscapeDataString(submission.Preparation.Preparation.Source.Source.FieldId)+"/accounting/commercial-terms/decide",submission) ??throw new InvalidOperationException("Recorded commercial decision is uncertain.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Commercial context changed. Retain the original decision intent.");
        result.Require();if(result.Decision!=command || result.PreparedAtUtc!=submission.Preparation.PreparedAtUtc || JsonSerializer.Serialize(result.Preparation)!=JsonSerializer.Serialize(submission.Preparation.Preparation))throw new InvalidOperationException("The recorded commercial decision differs from its original review and intent.");
        return CompanyInventorySourceReview.Copy(result);
    }
}
