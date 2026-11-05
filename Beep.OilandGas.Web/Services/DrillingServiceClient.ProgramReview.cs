using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class DrillingServiceClient
{
    public Task<DrillingProgramReviewDecisionReceipt> DecideProgramReviewAsync(DrillingProgramReviewDecision original,CancellationToken cancellationToken=default)=>ProgramDecision(original,false,cancellationToken);
    public Task<DrillingProgramReviewDecisionReceipt> RecoverProgramReviewDecisionAsync(DrillingProgramReviewDecision original,CancellationToken cancellationToken=default)=>ProgramDecision(original,true,cancellationToken);
    private async Task<DrillingProgramReviewDecisionReceipt> ProgramDecision(DrillingProgramReviewDecision original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingProgramReviewDecision,DrillingProgramReviewDecisionReceipt>("/api/drilling/programs/review/decision"+(recover?"/recover":""),original,token)??throw new InvalidOperationException("The original program decision outcome is unconfirmed. Preserve its recovery input.");result.Require(original);return result;
    }
    public async Task<DrillingProgramReviewProgress> GetProgramReviewProgressAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact recorded program review.");var result=await _apiClient.GetAsync<DrillingProgramReviewProgress>("/api/drilling/programs/review/progress?processId="+Uri.EscapeDataString(processId),cancellationToken)??throw new InvalidOperationException("The complete original program outcome is unavailable.");var scope=result.Source.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,processId);return result;
    }
    public async Task<DrillingProgramAssignedReview> GetAssignedProgramReviewAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !processId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose an exact assigned program review.");var result=await _apiClient.GetAsync<DrillingProgramAssignedReview>("/api/drilling/programs/review/assigned?processId="+Uri.EscapeDataString(processId),cancellationToken)??throw new InvalidOperationException("The assigned program source is unavailable.");var scope=result.Source.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,processId);return result;
    }
    public Task<DrillingProgramReviewStartReceipt> SubmitProgramReviewAsync(DrillingProgramReviewStart original,CancellationToken cancellationToken=default)=>ProgramReviewStart(original,false,cancellationToken);
    public Task<DrillingProgramReviewStartReceipt> RecoverProgramReviewAsync(DrillingProgramReviewStart original,CancellationToken cancellationToken=default)=>ProgramReviewStart(original,true,cancellationToken);
    private async Task<DrillingProgramReviewStartReceipt> ProgramReviewStart(DrillingProgramReviewStart original,bool recover,CancellationToken token)
    {
        original.Require();var result=await _apiClient.PostAsync<DrillingProgramReviewStart,DrillingProgramReviewStartReceipt>("/api/drilling/programs/review/"+(recover?"recover":"start"),original,token)??throw new InvalidOperationException("The original program assignment outcome is unconfirmed. Preserve its retained input.");result.Require(original);return result;
    }
    public async Task<DrillingProgramReviewReadiness> ReviewProgramRoutingAsync(string processId,string afeId,string wellUwi,string programId,string programRevision,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId) || string.IsNullOrWhiteSpace(wellUwi) || wellUwi.Length>128 || wellUwi.Any(char.IsControl) || !LeaseSummaryValidator.Identifier(programId) || !ProcessDefinitionRevision.IsValid(programRevision))throw new InvalidOperationException("Reload an exact saved program source before inspecting review routing.");
        var path="/api/drilling/programs/review?processId="+Uri.EscapeDataString(processId)+"&afeId="+Uri.EscapeDataString(afeId)+"&wellUwi="+Uri.EscapeDataString(wellUwi)+"&programId="+Uri.EscapeDataString(programId)+"&programRevision="+Uri.EscapeDataString(programRevision);
        var result=await _apiClient.GetAsync<DrillingProgramReviewReadiness>(path,cancellationToken)??throw new InvalidOperationException("The original program review source and routing are unavailable.");var basis=result.Source.Basis;result.Require(basis.Scope().Company,basis.Scope().ActorUserId,programId,programRevision);
        if(basis.Well.Uwi!=wellUwi || basis.Funding.Source.AfeId!=afeId || basis.Funding.Source.Current.Source.Review.InstanceId!=processId)throw new InvalidOperationException("The review belongs to another original program/well/funding source.");return result;
    }
}
