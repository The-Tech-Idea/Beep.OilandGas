using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;

namespace Beep.OilandGas.Client.App.Services.Connection
{
    /// <summary>
    /// Service interface for Connection management
    /// </summary>
    public interface IConnectionService
    {
        Task<List<ConnectionInfo>> GetAllConnectionsAsync(CancellationToken cancellationToken = default);
        Task<ConnectionInfo> GetConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
        Task<ConnectionTestResult> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
        Task<CurrentConnectionResponse> GetCurrentConnectionAsync(CancellationToken cancellationToken = default);
        Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName, string? userId = null, CancellationToken cancellationToken = default);
        Task<CreateConnectionResult> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
    }
}
