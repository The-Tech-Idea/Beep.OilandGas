using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    public async Task<APInvoiceVarianceDecisionResult> RecordAPInvoiceVarianceDecisionAsync(APInvoiceVarianceDecisionSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=PurchaseReceiptDecisionProof.Copy(submission);submission.Command.Require(submission.Review);var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection is null || original.Selection.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Return to the original controller company and account to record or reconcile.");
        var result=await _apiClient.PostAsync<APInvoiceVarianceDecisionSubmission,APInvoiceVarianceDecisionResult>("api/accounting/traditional/ap/"+Uri.EscapeDataString(command.InvoiceId)+"/variance-decision-commands",submission)
            ??throw new InvalidOperationException("Controller disposition is unconfirmed. Retain and reconcile the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during controller submission. Retain the original intent.");
        result.Require(submission);return result;
    }
    public async Task<APInvoiceVarianceDecisionReview> GetAPInvoiceVarianceDecisionReviewAsync(string invoiceId,string expectedInvoiceRevision)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);if(!ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))throw new InvalidOperationException("Reload the current variance invoice source.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original controller company and account.");
        var review=await _apiClient.GetAsync<APInvoiceVarianceDecisionReview>("api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/variance-decision-review?expectedInvoiceRevision="+Uri.EscapeDataString(expectedInvoiceRevision))??throw new InvalidOperationException("Independent variance review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during independent variance review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,invoiceId,expectedInvoiceRevision);return review;
    }
    public async Task<APInvoiceVarianceResult> RecordAPInvoiceVarianceAsync(APInvoiceVarianceSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=PurchaseReceiptDecisionProof.Copy(submission);submission.Command.Require(submission.Review);var original=_apiClient.CompanySnapshot;var command=submission.Command;
        if(original.Selection is null || original.Selection.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)
            throw new InvalidOperationException("Return to the original variance company and account to submit or reconcile.");
        var result=await _apiClient.PostAsync<APInvoiceVarianceSubmission,APInvoiceVarianceResult>("api/accounting/traditional/ap/"+Uri.EscapeDataString(command.InvoiceId)+"/variance-proposal-commands",submission)
            ?? throw new InvalidOperationException("Variance proposal outcome is unconfirmed. Retain and reconcile the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during variance submission. Retain the original intent.");
        result.Require(submission);return result;
    }
    public async Task<APInvoiceVarianceProposalReview> ReviewAPInvoiceVarianceAsync(string invoiceId,APInvoiceVarianceProposal proposal)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);ArgumentNullException.ThrowIfNull(proposal);
        proposal=PurchaseReceiptDecisionProof.Copy(proposal);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null)throw new InvalidOperationException("Select the original variance company and account.");
        var review=await _apiClient.PostAsync<APInvoiceVarianceProposal,APInvoiceVarianceProposalReview>("api/accounting/traditional/ap/"+Uri.EscapeDataString(invoiceId)+"/variance-proposal-review",proposal)
            ?? throw new InvalidOperationException("Variance preparation review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during variance preparation.");
        review.Require(original.Selection.Scope,original.Selection.UserId,invoiceId,proposal);return review;
    }
}
