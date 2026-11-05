using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

public partial class DrillingServiceClient
{
    public async Task<DrillingExecutionConflictReview> GetExecutionConflictAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingExecutionStart,DrillingExecutionConflictReview>("/api/drilling/programs/execution/conflict",original,cancellationToken)
            ??throw new InvalidOperationException("The exact different recorded execution is unavailable. Preserve the original.");
        result.Require(original);return result;
    }
    public async Task<DrillingExecutionConflictReview> ConfirmExecutionConflictAsync(DrillingExecutionConflictAcknowledgement original,CancellationToken cancellationToken=default)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingExecutionConflictAcknowledgement,DrillingExecutionConflictReview>("/api/drilling/programs/execution/conflict/confirm",original,cancellationToken)
            ??throw new InvalidOperationException("The reviewed execution conflict is unconfirmed. Preserve the original.");
        result.Require(original.Original);if(result.Revision!=original.Review.Revision)throw new InvalidOperationException("The reviewed competing execution changed. Inspect again before clearing the original.");
        return result;
    }
}
