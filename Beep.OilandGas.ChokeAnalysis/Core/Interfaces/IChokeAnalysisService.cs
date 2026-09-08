using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.ChokeAnalysis.Core.Interfaces;

public interface IChokeAnalysisService
{
    Task<CHOKE_FLOW_RESULT> CalculateDownholeChokeFlowAsync(CHOKE_PROPERTIES choke, GAS_CHOKE_PROPERTIES gasProperties, CancellationToken cancellationToken = default);
    Task<CHOKE_FLOW_RESULT> CalculateUpholeChokeFlowAsync(CHOKE_PROPERTIES choke, GAS_CHOKE_PROPERTIES gasProperties, CancellationToken cancellationToken = default);
}
