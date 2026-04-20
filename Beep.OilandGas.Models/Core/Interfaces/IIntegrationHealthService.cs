using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IIntegrationHealthService
{
    Task<List<AdapterHealthStatus>> GetAllStatusAsync();
    Task<AdapterHealthStatus> GetStatusAsync(string adapterName);
    void RecordSuccess(string adapterName);
    void RecordFailure(string adapterName, Exception ex);
    void ResetCircuitBreaker(string adapterName);

    /// <summary>Returns the last N sync history entries across all adapters.</summary>
    List<IntegrationSyncHistoryEntry> GetRecentHistory(int count = 50);

    /// <summary>Appends a sync history entry (called by adapters after each sync).</summary>
    void AppendHistory(IntegrationSyncHistoryEntry entry);
}
