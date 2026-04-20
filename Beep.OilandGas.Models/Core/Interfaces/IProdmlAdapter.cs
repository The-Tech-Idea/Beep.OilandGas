using Beep.OilandGas.Models.Data.Integrations;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IProdmlAdapter
{
    Task<ProdmlSyncResult> SyncMonthlyVolumesAsync(
        string prodmlEndpoint, string fieldId, int year, int month, string userId);

    Task<ProdmlSyncResult> SyncDailyAllocationsAsync(
        string prodmlEndpoint, string fieldId, DateTime date, string userId);

    Task<List<ProdmlWellSummary>> GetAvailableWellsAsync(string prodmlEndpoint);
}
