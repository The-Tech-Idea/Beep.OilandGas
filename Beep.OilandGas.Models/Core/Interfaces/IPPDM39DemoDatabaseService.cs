using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Manages demo and dummy data lifecycle: generation, status check, and removal.
    /// Isolated from production seeding to prevent accidental demo contamination.
    /// </summary>
    public interface IPPDM39DemoDatabaseService
    {
        // ── Demo data generation ───────────────────────────────────────────
        Task<GenerateDummyDataResponse> GenerateFullDemoAsync(string connectionName, string seedOption = "standard", string userId = "SYSTEM", string? operationId = null);
        Task<GenerateDummyDataResponse> GenerateDummyDataAsync(GenerateDummyDataRequest request, string connectionName, string? operationId = null);

        // ── Demo data status and removal ───────────────────────────────────
        DummyDataStatusResponse GetDummyDataStatus(string connectionName);
        Task<DummyDataDeleteResponse> DeleteDummyDataAsync(string connectionName, string userId = "SYSTEM");
    }
}
