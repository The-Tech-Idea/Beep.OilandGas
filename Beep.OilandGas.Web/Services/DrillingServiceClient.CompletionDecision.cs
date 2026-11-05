using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingCompletionReviewProgress> GetCompletionReviewProgressAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open the exact original completion review history.");
        var result=await _apiClient.GetAsync<DrillingCompletionReviewProgress>("/api/drilling/programs/execution/completion/progress?processId="+Uri.EscapeDataString(processId),cancellationToken)
            ??throw new InvalidOperationException("The complete original completion decision history is unavailable.");
        var scope=result.Source.History.Execution.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,processId);return result;
    }
    public Task<DrillingCompletionReviewDecisionReceipt> DecideCompletionReviewAsync(DrillingCompletionReviewDecision original,CancellationToken cancellationToken=default)=>CompletionDecision(original,false,cancellationToken);
    public Task<DrillingCompletionReviewDecisionReceipt> RecoverCompletionReviewDecisionAsync(DrillingCompletionReviewDecision original,CancellationToken cancellationToken=default)=>CompletionDecision(original,true,cancellationToken);
    private async Task<DrillingCompletionReviewDecisionReceipt> CompletionDecision(DrillingCompletionReviewDecision original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingCompletionReviewDecision,DrillingCompletionReviewDecisionReceipt>("/api/drilling/programs/execution/completion/decision"+(recover?"/recover":""),original,token)
            ??throw new InvalidOperationException("The original completion decision outcome is unconfirmed. Preserve and recover its exact input.");result.Require(original);return result;
    }
}
