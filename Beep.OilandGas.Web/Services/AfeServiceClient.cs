using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class AfeServiceClient(ApiClient api):IAfeServiceClient
{
    private const string Endpoint="/api/accounting/cost/afe/development";
    public async Task<DevelopmentInvestmentStageRecoveryInput> ReadOwnInvestmentStageOriginalAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(instanceId) || !instanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose the exact original investment assignment.");
        var original=await api.GetAsync<DevelopmentInvestmentStageRecoveryInput>(Endpoint+"/assigned/"+Uri.EscapeDataString(instanceId)+"/original-decision",cancellationToken)??throw new InvalidOperationException("The original investment stage recovery input is unconfirmed.");
        original.Require(original.Company,original.ActorUserId,instanceId);return original;
    }
    public Task<DevelopmentInvestmentStageReceipt> DecideAssignedInvestmentStageAsync(DevelopmentInvestmentStageDecision original,CancellationToken cancellationToken=default)=>StageDecision(original,false,cancellationToken);
    public Task<DevelopmentInvestmentStageReceipt> RecoverAssignedInvestmentStageAsync(DevelopmentInvestmentStageDecision original,CancellationToken cancellationToken=default)=>StageDecision(original,true,cancellationToken);
    private async Task<DevelopmentInvestmentStageReceipt> StageDecision(DevelopmentInvestmentStageDecision original,bool recover,CancellationToken cancellationToken)
    {
        original.Require();var receipt=await api.PostAsync<DevelopmentInvestmentStageDecision,DevelopmentInvestmentStageReceipt>(Endpoint+"/assigned/decisions"+(recover?"/recover":""),original,cancellationToken)
            ??throw new InvalidOperationException("The original investment stage outcome is unconfirmed. Recover its retained input.");
        receipt.Require(original);return receipt;
    }
    public async Task<DevelopmentInvestmentAssignedReview> ReadAssignedInvestmentAsync(string instanceId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(instanceId) || !instanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Choose the exact assigned investment review.");
        var result=await api.GetAsync<DevelopmentInvestmentAssignedReview>(Endpoint+"/assigned/"+Uri.EscapeDataString(instanceId),cancellationToken)??throw new InvalidOperationException("The assigned investment source is unavailable.");
        if(result.Source?.Current?.Source?.Review?.Scope is not {} scope)throw new InvalidOperationException("The assigned investment scope is unavailable.");
        result.Require(scope.Company,scope.ActorUserId,instanceId);return result;
    }
    public async Task<DevelopmentInvestmentHandoffStatus> ReadInvestmentHandoffAsync(string processId,string afeId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId))throw new InvalidOperationException("Choose the exact original investment source.");
        var result=await api.GetAsync<DevelopmentInvestmentHandoffStatus>(Endpoint+"/"+Uri.EscapeDataString(afeId)+"/handoff?processId="+Uri.EscapeDataString(processId),cancellationToken)??throw new InvalidOperationException("The recorded investment handoff or its absence is unconfirmed.");
        if(result.Source?.Current?.Source?.Review?.Scope is not {} scope)throw new InvalidOperationException("The handoff status source is unavailable.");result.Require(scope.Company,scope.ActorUserId,processId,afeId);return result;
    }
    public async Task<DevelopmentInvestmentHandoff> PrepareInvestmentHandoffAsync(string processId,string afeId,DevelopmentInvestmentHandoffSelection selection,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId) || selection is null || !LeaseSummaryValidator.Identifier(selection.ProcessId) || !ProcessDefinitionRevision.IsValid(selection.RoutingRevision))throw new InvalidOperationException("Choose a workflow from the exact source-bound investment routing review.");
        var original=await api.PostAsync<DevelopmentInvestmentHandoffSelection,DevelopmentInvestmentHandoff>(Endpoint+"/"+Uri.EscapeDataString(afeId)+"/handoff/prepare?processId="+Uri.EscapeDataString(processId),selection,cancellationToken)??throw new InvalidOperationException("The original investment handoff could not be prepared.");
        original.Require();if(original.Review.Source.AfeId!=afeId || original.Review.Source.Current.Source.Review.InstanceId!=processId || original.Review.Revision!=selection.RoutingRevision || original.Workflow.ProcessId!=selection.ProcessId)throw new InvalidOperationException("The prepared investment handoff belongs to another source or routing review.");return original;
    }
    public Task<DevelopmentInvestmentHandoffReceipt> StartInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,CancellationToken cancellationToken=default)=>Handoff(original,false,cancellationToken);
    public Task<DevelopmentInvestmentHandoffReceipt> RecoverInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,CancellationToken cancellationToken=default)=>Handoff(original,true,cancellationToken);
    private async Task<DevelopmentInvestmentHandoffReceipt> Handoff(DevelopmentInvestmentHandoff original,bool recover,CancellationToken token)
    {
        original.Require();var receipt=await api.PostAsync<DevelopmentInvestmentHandoff,DevelopmentInvestmentHandoffReceipt>(Endpoint+"/handoffs"+(recover?"/recover":""),original,token)??throw new InvalidOperationException("The original investment handoff is unconfirmed. Recover its retained input.");receipt.Require(original);
        if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original investment handoff was not confirmed recorded.");return receipt;
    }
    public async Task<DevelopmentInvestmentRoutingReview> ReviewInvestmentRoutingAsync(string processId,string afeId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId))throw new InvalidOperationException("Choose the exact approved source and AFE before reviewing assignment readiness.");
        var result=await api.GetAsync<DevelopmentInvestmentRoutingReview>(Endpoint+"/"+Uri.EscapeDataString(afeId)+"/routing?processId="+Uri.EscapeDataString(processId),cancellationToken)
            ??throw new InvalidOperationException("The original investment routing evidence is unconfirmed.");
        result.Require(result.Source.Current.Source.Review.Scope.Company,result.Source.Current.Source.Review.Scope.ActorUserId,processId,afeId);return result;
    }
    public async Task<DevelopmentInvestmentQueue> QueueAsync(CancellationToken cancellationToken=default)
    {
        var result=await api.GetAsync<DevelopmentInvestmentQueue>(Endpoint+"/queue",cancellationToken)??throw new InvalidOperationException("The current company investment queue is unconfirmed.");result.Require(result.Company,result.ActorUserId);return result;
    }
    public async Task<DevelopmentInvestmentReview> ReviewInvestmentAsync(string processId,string afeId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId) || !LeaseSummaryValidator.Identifier(afeId))throw new InvalidOperationException("Choose an exact approved source and AFE request.");
        var result=await api.GetAsync<DevelopmentInvestmentReview>(Endpoint+"/"+Uri.EscapeDataString(afeId)+"/investment?processId="+Uri.EscapeDataString(processId),cancellationToken)??throw new InvalidOperationException("The current investment outcome is unconfirmed.");result.Require();
        if(result.AfeId!=afeId || result.Current.Source.Review.InstanceId!=processId)throw new InvalidOperationException("The investment evidence belongs to another original source or AFE.");return result;
    }
    public async Task<DevelopmentAfeReview> ReviewDevelopmentAsync(string processId,CancellationToken cancellationToken=default)
    {
        if(!LeaseSummaryValidator.Identifier(processId))throw new InvalidOperationException("Choose a recorded approved FDP source.");
        var review=await api.GetAsync<DevelopmentAfeReview>(Endpoint+"?processId="+Uri.EscapeDataString(processId),cancellationToken)??throw new InvalidOperationException("The approved source and company AFE review are unconfirmed.");review.Require();
        if(review.Source.Review.InstanceId!=processId)throw new InvalidOperationException("The AFE review belongs to another original source.");return review;
    }
    public Task<DevelopmentAfeReceipt> RecordDevelopmentAsync(DevelopmentAfeCommand original,CancellationToken cancellationToken=default)=>Send(original,false,cancellationToken);
    public Task<DevelopmentAfeReceipt> RecoverDevelopmentAsync(DevelopmentAfeCommand original,CancellationToken cancellationToken=default)=>Send(original,true,cancellationToken);
    private async Task<DevelopmentAfeReceipt> Send(DevelopmentAfeCommand original,bool recover,CancellationToken cancellationToken)
    {
        original.Require();var receipt=await api.PostAsync<DevelopmentAfeCommand,DevelopmentAfeReceipt>(Endpoint+(recover?"/recover":""),original,cancellationToken)??throw new InvalidOperationException("The original AFE outcome is unconfirmed. Recover its retained request.");receipt.Require(original);
        if(!recover && receipt.Recorded is null)throw new InvalidOperationException("The original AFE request was not confirmed recorded.");return receipt;
    }
}
