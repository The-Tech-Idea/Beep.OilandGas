using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    private static void ReportingIdentity(string assignmentId)
    {
        if(!LeaseSummaryValidator.Identifier(assignmentId) || !assignmentId.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Open an exact original reporting responsibility.");
    }
    public async Task<DrillingReportingClosureStatus> GetReportingResponsibilityAsync(string assignmentId,CancellationToken cancellationToken=default)
    {
        ReportingIdentity(assignmentId);
        var result=await _apiClient.GetAsync<DrillingReportingClosureStatus>("/api/drilling/programs/execution/reporting/status?assignmentId="+Uri.EscapeDataString(assignmentId),cancellationToken)
            ??throw new InvalidOperationException("The original reporting responsibility is unavailable.");
        var scope=result.Scope();result.Require(scope.Company,scope.ActorUserId,assignmentId);return result;
    }
    public async Task<DrillingReportingClosureReview> ReviewReportingClosureAsync(string assignmentId,string completionId,CancellationToken cancellationToken=default)
    {
        ReportingIdentity(assignmentId);
        if(!LeaseSummaryValidator.Identifier(completionId) || !completionId.StartsWith(DrillingCompletionReviewStart.InstancePrefix,StringComparison.Ordinal))
            throw new InvalidOperationException("Choose an exact independently accepted completion review.");
        var result=await _apiClient.GetAsync<DrillingReportingClosureReview>("/api/drilling/programs/execution/reporting/closure/review?assignmentId="+Uri.EscapeDataString(assignmentId)+"&completionId="+Uri.EscapeDataString(completionId),cancellationToken)
            ??throw new InvalidOperationException("The complete current reporting closure review is unavailable.");
        result.Require();
        if(result.Responsibility.Root.InstanceId()!=assignmentId || result.Evidence.Acceptance.InstanceId!=completionId)
            throw new InvalidOperationException("The closure review belongs to another responsibility or accepted completion.");
        return result;
    }
    public Task<DrillingReportingClosureReceipt> CloseReportingAsync(DrillingReportingClosure original,CancellationToken cancellationToken=default)=>ReportingClosure(original,false,cancellationToken);
    public Task<DrillingReportingClosureReceipt> RecoverReportingClosureAsync(DrillingReportingClosure original,CancellationToken cancellationToken=default)=>ReportingClosure(original,true,cancellationToken);
    private async Task<DrillingReportingClosureReceipt> ReportingClosure(DrillingReportingClosure original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingReportingClosure,DrillingReportingClosureReceipt>("/api/drilling/programs/execution/reporting/closure"+(recover?"/recover":""),original,token)
            ??throw new InvalidOperationException("The original reporting closure is unconfirmed. Preserve and recover its exact retained input.");
        result.Require(original);return result;
    }
}
