using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Client.App.Services.Connection
{
    /// <summary>
    /// Service interface for Connection management
    /// </summary>
    public interface IConnectionService
    {
        Task<List<object>> GetAllConnectionsAsync(CancellationToken cancellationToken = default);
        Task<object> GetConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
        Task<object> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
        Task<object> GetCurrentConnectionAsync(CancellationToken cancellationToken = default);
        Task<object> SetCurrentConnectionAsync(string connectionName, string? userId = null, CancellationToken cancellationToken = default);
        Task<object> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default);
    }
}
