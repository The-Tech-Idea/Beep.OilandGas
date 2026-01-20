using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for creating and managing demo SQLite databases
    /// Platform-agnostic interface for demo database operations
    /// </summary>
    public interface IDemoDatabaseService
    {
        Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(CreateDemoDatabaseRequest request);
        Task<DeleteDemoDatabaseResponse> DeleteDemoDatabaseAsync(string connectionName);
        Task<CleanupDemoDatabasesResponse> CleanupExpiredDatabasesAsync();
        List<DemoDatabaseMetadata> GetUserDemoDatabases(string userId);
        List<DemoDatabaseMetadata> GetAllDemoDatabases();
    }
}



