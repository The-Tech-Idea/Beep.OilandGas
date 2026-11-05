using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for generic PPDM39 data operations
    /// Platform-agnostic interface for CRUD operations on PPDM39 entities
    /// </summary>
    public interface IPPDM39DataService
    {
        Task<GetEntitiesResponse> GetEntitiesAsync(string tableName, List<AppFilter> filters, string? connectionName = null);
        Task<GenericEntityResponse> GetEntityByIdAsync(string tableName, object id, string? connectionName = null);
        Task<GenericEntityResponse> InsertEntityAsync(string tableName, Dictionary<string, object> entityData, string userId, string? connectionName = null);
        Task<GenericEntityResponse> UpdateEntityAsync(string tableName, string entityId, Dictionary<string, object> entityData, string userId, string? connectionName = null);
        Task<GenericEntityResponse> DeleteEntityAsync(string tableName, object id, string userId, string? connectionName = null);
    }
}



