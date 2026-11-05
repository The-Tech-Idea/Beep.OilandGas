using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private const string BillingPolicyRoute="api/accounting/traditional/billing-policies";
    public async Task<CompanyBillingPolicyReview> GetCompanyBillingPoliciesAsync()
    {
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original policy company and account.");
        var review=await _apiClient.GetAsync<CompanyBillingPolicyReview>(BillingPolicyRoute) ??throw new InvalidOperationException("Billing policy history is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during billing policy review.");review.Require(original.Selection.Scope,original.Selection.UserId);return review;
    }
    public async Task<CompanyBillingPolicyCommandReview> GetCompanyBillingPolicyCommandReviewAsync(string action)
    {
        if(action is not ("DRAFT" or "APPROVE"))throw new InvalidOperationException("Choose an explicit billing policy action.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original policy company and account.");
        var review=await _apiClient.GetAsync<CompanyBillingPolicyCommandReview>(BillingPolicyRoute+"/command-review?action="+Uri.EscapeDataString(action)) ??throw new InvalidOperationException("Billing policy command review is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during billing policy review.");review.Require(original.Selection.Scope,original.Selection.UserId,action);return review;
    }
    public async Task<CompanyBillingPolicyResult> PrepareCompanyBillingPolicyAsync(CompanyBillingPolicyDraftSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=PurchaseReceiptDecisionProof.Copy(submission);submission.Review.Require(submission.Command);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original policy company and account to submit or reconcile.");
        var result=await _apiClient.PostAsync<CompanyBillingPolicyDraftSubmission,CompanyBillingPolicyResult>(BillingPolicyRoute+"/drafts",submission) ??throw new InvalidOperationException("Preparation outcome is unconfirmed. Retain the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during preparation. Retain the original intent.");result.Require(submission);return result;
    }
    public async Task<CompanyBillingPolicyResult> ApproveCompanyBillingPolicyAsync(CompanyBillingPolicyApprovalSubmission submission)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(submission.Review);
        submission=PurchaseReceiptDecisionProof.Copy(submission);submission.Review.Require(submission.Command);var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=submission.Command.Company || original.Selection.UserId!=submission.Command.ActorUserId)throw new InvalidOperationException("Return to the original reviewer company and account to submit or reconcile.");
        var result=await _apiClient.PostAsync<CompanyBillingPolicyApprovalSubmission,CompanyBillingPolicyResult>(BillingPolicyRoute+"/approvals",submission) ??throw new InvalidOperationException("Approval outcome is unconfirmed. Retain the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during approval. Retain the original intent.");result.Require(submission);return result;
    }
}
