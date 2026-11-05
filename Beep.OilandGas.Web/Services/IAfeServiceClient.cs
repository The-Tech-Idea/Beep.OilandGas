using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public interface IAfeServiceClient
{
    Task<DevelopmentInvestmentStageRecoveryInput> ReadOwnInvestmentStageOriginalAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentStageReceipt> DecideAssignedInvestmentStageAsync(DevelopmentInvestmentStageDecision original,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentStageReceipt> RecoverAssignedInvestmentStageAsync(DevelopmentInvestmentStageDecision original,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentAssignedReview> ReadAssignedInvestmentAsync(string instanceId,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentHandoffStatus> ReadInvestmentHandoffAsync(string processId,string afeId,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentHandoff> PrepareInvestmentHandoffAsync(string processId,string afeId,DevelopmentInvestmentHandoffSelection selection,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentHandoffReceipt> StartInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentHandoffReceipt> RecoverInvestmentHandoffAsync(DevelopmentInvestmentHandoff original,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentRoutingReview> ReviewInvestmentRoutingAsync(string processId,string afeId,CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentQueue> QueueAsync(CancellationToken cancellationToken=default);
    Task<DevelopmentInvestmentReview> ReviewInvestmentAsync(string processId,string afeId,CancellationToken cancellationToken=default);
    Task<DevelopmentAfeReview> ReviewDevelopmentAsync(string processId,CancellationToken cancellationToken=default);
    Task<DevelopmentAfeReceipt> RecordDevelopmentAsync(DevelopmentAfeCommand original,CancellationToken cancellationToken=default);
    Task<DevelopmentAfeReceipt> RecoverDevelopmentAsync(DevelopmentAfeCommand original,CancellationToken cancellationToken=default);
}
