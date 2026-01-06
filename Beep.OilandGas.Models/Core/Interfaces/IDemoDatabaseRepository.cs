using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Repository interface for demo database metadata storage
    /// Platform-agnostic interface for demo database metadata persistence
    /// </summary>
    public interface IDemoDatabaseRepository
    {
        Task AddAsync(DemoDatabaseMetadata metadata);
        Task<List<DemoDatabaseMetadata>> GetAllAsync();
        Task<DemoDatabaseMetadata?> GetByConnectionNameAsync(string connectionName);
        Task<List<DemoDatabaseMetadata>> GetByUserIdAsync(string userId);
        Task DeleteAsync(string connectionName);
        Task<List<DemoDatabaseMetadata>> GetExpiredAsync();
    }
}



