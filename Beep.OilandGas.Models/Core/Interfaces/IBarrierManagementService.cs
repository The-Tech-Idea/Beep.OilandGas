using Beep.OilandGas.Models.Data.HSE;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IBarrierManagementService
{
    Task<List<BarrierRecord>> GetBarriersForIncidentAsync(string incidentId);
    Task                      AddBarrierAsync(string incidentId, AddBarrierRequest request, string userId);
    Task                      SetBarrierStatusAsync(string incidentId, string equipId, string status, string userId);
    Task<BarrierSummary>      GetBarrierSummaryAsync(string incidentId);
}
