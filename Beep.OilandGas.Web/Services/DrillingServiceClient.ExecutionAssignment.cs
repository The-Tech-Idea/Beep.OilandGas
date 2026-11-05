using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;

public partial class DrillingServiceClient
{
    public async Task<DrillingExecutionAssignmentReview> GetExecutionAssignmentAsync(string reviewId,CancellationToken cancellationToken=default)
    {
        var result=await _apiClient.GetAsync<DrillingExecutionAssignmentReview>(ExecutionPath(reviewId,"/assignment"),cancellationToken)
            ??throw new InvalidOperationException("The original execution assignment review is unavailable.");
        var scope=result.Source.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,reviewId);return result;
    }
    public Task<DrillingExecutionAssignmentReceipt> AssignExecutionAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default)
        =>ExecutionAssignment(original,false,cancellationToken);
    public Task<DrillingExecutionAssignmentReceipt> RecoverExecutionAssignmentAsync(DrillingExecutionAssignment original,CancellationToken cancellationToken=default)
        =>ExecutionAssignment(original,true,cancellationToken);
    private async Task<DrillingExecutionAssignmentReceipt> ExecutionAssignment(DrillingExecutionAssignment original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingExecutionAssignment,DrillingExecutionAssignmentReceipt>(
            "/api/drilling/programs/execution/assignment"+(recover?"/recover":""),original,token)
            ??throw new InvalidOperationException("The original reporting assignment is unconfirmed. Preserve its retained input.");
        result.Require(original);return result;
    }
    public async Task<DrillingExecutionAssignedWork> GetAssignedExecutionAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Choose an exact reporting responsibility from the company inbox.");
        var result=await _apiClient.GetAsync<DrillingExecutionAssignedWork>("/api/drilling/programs/execution/assigned?processId="+Uri.EscapeDataString(processId),cancellationToken)
            ??throw new InvalidOperationException("The original assigned execution and individual reporting task are unavailable.");
        var scope=result.Source.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,processId);return result;
    }
}
