using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public partial class DrillingServiceClient
{
    public async Task<DrillingCompletionAssignedReview> GetAssignedCompletionReviewAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open an exact assigned completion review.");
        var result=await _apiClient.GetAsync<DrillingCompletionAssignedReview>("/api/drilling/programs/execution/completion/assigned?processId="+Uri.EscapeDataString(processId),cancellationToken)
            ??throw new InvalidOperationException("The independent completion task and original claim are unavailable.");
        var scope=result.Source.History.Execution.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,processId);return result;
    }
    public Task<DrillingCompletionReviewStartReceipt> SubmitCompletionReviewAsync(DrillingCompletionReviewStart original,CancellationToken cancellationToken=default)=>CompletionReviewStart(original,false,cancellationToken);
    public Task<DrillingCompletionReviewStartReceipt> RecoverCompletionReviewAsync(DrillingCompletionReviewStart original,CancellationToken cancellationToken=default)=>CompletionReviewStart(original,true,cancellationToken);
    private async Task<DrillingCompletionReviewStartReceipt> CompletionReviewStart(DrillingCompletionReviewStart original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingCompletionReviewStart,DrillingCompletionReviewStartReceipt>(
            "/api/drilling/programs/execution/completion/review"+(recover?"/recover":""),original,token)
            ??throw new InvalidOperationException("The completion-review submission outcome is unconfirmed. Preserve and recover its original input.");
        result.Require(original);return result;
    }
    public async Task<DrillingCompletionReviewReadiness> GetCompletionRoutingAsync(string reviewId, CancellationToken cancellationToken = default)
    {
        if (!LeaseSummaryValidator.Identifier(reviewId) || !reviewId.StartsWith(DrillingProgramReviewStart.InstancePrefix, StringComparison.Ordinal))
            throw new InvalidOperationException("Open the exact recorded drilling execution before inspecting completion routing.");
        var result = await _apiClient.GetAsync<DrillingCompletionReviewReadiness>(
            "/api/drilling/programs/execution/completion/review?reviewId=" + Uri.EscapeDataString(reviewId), cancellationToken)
            ?? throw new InvalidOperationException("The current completion source and independent routing are unavailable.");
        var scope = result.Source.Execution.Basis.Scope(); result.Require(scope.Company, scope.ActorUserId, reviewId); return result;
    }
}
