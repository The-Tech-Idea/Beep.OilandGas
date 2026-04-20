using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IWitsmlAdapter
{
    Task<WitsmlSyncResult> SyncWellAsync(string witsmlWellUid, string fieldId, string userId);
    Task<WitsmlSyncResult> SyncCasingAsync(string witsmlWellUid, string witsmlWellboreUid, string userId);
    Task<WitsmlSyncResult> SyncLogAsync(string witsmlWellUid, string witsmlWellboreUid, string logUid, string userId);
    Task<List<WitsmlWellSummary>> GetAvailableWellsAsync(string serverUrl);
}
