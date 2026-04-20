using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface ISapErpAdapter
{
    Task<SapSyncResult> SyncWorkOrderAsync(string sapWoNumber, string fieldId, string userId);
    Task<SapSyncResult> SyncCostsAsync(string sapWoNumber, string userId);
    Task<List<SapWorkOrderSummary>> GetOpenWorkOrdersAsync(string plantCode, string fieldId);
}
