using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IRCAService
{
    Task<List<CauseFinding>> GetCauseChainAsync(string incidentId);
    Task                     AddCauseAsync(string incidentId, AddCauseRequest request, string userId);
    Task                     UpdateCauseAsync(string incidentId, int causeSeq, UpdateCauseRequest request, string userId);
    Task<bool>               IsRCACompleteAsync(string incidentId);
}
