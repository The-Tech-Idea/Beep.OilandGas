using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    private static string ExecutionPath(string review,string suffix)
    {if(!LeaseSummaryValidator.Identifier(review) || !review.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact independently approved program review.");return "/api/drilling/programs/execution"+suffix+"?reviewId="+Uri.EscapeDataString(review);}
    public async Task<DrillingExecutionBasis> GetExecutionBasisAsync(string reviewId,CancellationToken cancellationToken=default)
    {var result=await _apiClient.GetAsync<DrillingExecutionBasis>(ExecutionPath(reviewId,"/basis"),cancellationToken)??throw new InvalidOperationException("The approved execution basis is unavailable.");result.Require();if(result.ReviewInstanceId!=reviewId)throw new InvalidOperationException("The execution basis belongs to another original program review.");return result;}
    public async Task<DrillingExecutionStatus> GetExecutionStatusAsync(string reviewId,CancellationToken cancellationToken=default)
    {var result=await _apiClient.GetAsync<DrillingExecutionStatus>(ExecutionPath(reviewId,""),cancellationToken)??throw new InvalidOperationException("The execution outcome or its absence is unconfirmed.");result.Require();if(result.Basis.ReviewInstanceId!=reviewId)throw new InvalidOperationException("The execution outcome belongs to another original program review.");return result;}
    public Task<DrillingExecutionStartReceipt> StartExecutionAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default)=>ExecutionStart(original,false,cancellationToken);
    public Task<DrillingExecutionStartReceipt> RecoverExecutionStartAsync(DrillingExecutionStart original,CancellationToken cancellationToken=default)=>ExecutionStart(original,true,cancellationToken);
    private async Task<DrillingExecutionStartReceipt> ExecutionStart(DrillingExecutionStart original,bool recover,CancellationToken token)
    {original.Require();var result=await _apiClient.PostAsync<DrillingExecutionStart,DrillingExecutionStartReceipt>("/api/drilling/programs/execution/"+(recover?"recover":"start"),original,token)??throw new InvalidOperationException("The original execution start is unconfirmed. Preserve its retained input.");result.Require(original);return result;}
}
