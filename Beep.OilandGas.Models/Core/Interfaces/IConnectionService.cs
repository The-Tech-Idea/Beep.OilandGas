using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.DataManagement;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for managing IDMEEditor configured connections
    /// Platform-agnostic interface for connection management
    /// </summary>
    public interface IConnectionService
    {
        List<ConnectionInfo> GetAllConnections();
        ConnectionInfo? GetConnection(string connectionName);
        Task<ConnectionTestResult> TestConnectionAsync(string connectionName);
        CurrentConnectionResponse GetCurrentConnection();
        SetCurrentConnectionResult SetCurrentConnection(string connectionName, string? userId = null);
    }
}
