using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public async Task<DrillingExecutionAssignmentConflictReview> GetExecutionAssignmentConflictAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingExecutionAssignment,DrillingExecutionAssignmentConflictReview>("/api/drilling/programs/execution/assignment/conflict",original,cancellationToken)
            ??throw new InvalidOperationException("The original absence and different reporting responsibility are unconfirmed. Preserve the original.");
        result.Require(original);return result;
    }
    public async Task<DrillingExecutionAssignmentConflictReview> ConfirmExecutionAssignmentConflictAsync(DrillingExecutionAssignmentConflictAcknowledgement original,CancellationToken cancellationToken=default)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingExecutionAssignmentConflictAcknowledgement,DrillingExecutionAssignmentConflictReview>("/api/drilling/programs/execution/assignment/conflict/confirm",original,cancellationToken)
            ??throw new InvalidOperationException("The reviewed reporting responsibility is unconfirmed. Preserve the original.");
        result.Require(original.Original);if(result.Revision!=original.ReviewRevision)throw new InvalidOperationException("The reviewed reporting responsibility changed. Inspect again before clearing the original.");
        return result;
    }
}
