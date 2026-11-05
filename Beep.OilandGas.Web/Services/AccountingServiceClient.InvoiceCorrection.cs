using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<APInvoiceCorrectionDecisionReview> GetAPInvoiceCorrectionDecisionReviewAsync(string invoiceId,string expectedInvoiceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);if(!ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))throw new InvalidOperationException("Reload the latest recorded correction source.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original independent reviewer company and account.");
        var result=await _apiClient.GetAsync<APInvoiceCorrectionDecisionReview>("api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/correction-decision-review?expectedInvoiceRevision="+Uri.EscapeDataString(expectedInvoiceRevision))
            ??throw new InvalidOperationException("Independent corrected-invoice review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during independent correction review.");
        result.Require(original.Selection.Scope,original.Selection.UserId,invoiceId,expectedInvoiceRevision);return result;
    }
    public async Task<APInvoiceCorrectionResult> RecordAPInvoiceCorrectionAsync(APInvoiceCorrectionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=PurchaseReceiptDecisionProof.Copy(submission);submission.Command.Require(submission.Review);var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection is null || original.Selection.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Return to the original accountant company and account to record or reconcile correction.");
        var result=await _apiClient.PostAsync<APInvoiceCorrectionSubmission,APInvoiceCorrectionResult>("api/accounting/traditional/ap/"+Uri.EscapeDataString(command.InvoiceId)+"/correction-commands",submission)
            ??throw new InvalidOperationException("Correction is unconfirmed. Retain and reconcile the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during correction recording. Retain the original intent.");
        result.Require(submission);return result;
    }
    public async Task<APInvoiceCorrectionReview> ReviewAPInvoiceCorrectionAsync(string invoiceId,APInvoiceCorrectionDraft draft)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentNullException.ThrowIfNull(draft);
        draft=PurchaseReceiptDecisionProof.Copy(draft);draft.RequireShape();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original accountant company before reviewing correction.");
        var review=await _apiClient.PostAsync<APInvoiceCorrectionDraft,APInvoiceCorrectionReview>("api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/correction-review",draft)
            ??throw new InvalidOperationException("Correction review is unavailable. Reload current source evidence.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during correction review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,invoiceId,draft);return review;
    }
}
