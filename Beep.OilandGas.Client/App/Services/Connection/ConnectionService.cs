using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Client.App.Services.Connection
{
    /// <summary>
    /// Unified service for Connection management
    /// </summary>
    internal class ConnectionService : ServiceBase, IConnectionService
    {
        public ConnectionService(BeepOilandGasApp app, ILogger<ConnectionService>? logger = null)
            : base(app, logger)
        {
        }

        public async Task<List<object>> GetAllConnectionsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<object>>("/api/connection/connections", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}/test", null, cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<object> GetCurrentConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<object>("/api/connection/current", null, cancellationToken);
            return new { ConnectionName = CurrentConnectionName };
        }

        public async Task<object> SetCurrentConnectionAsync(string connectionName, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            _app.SetCurrentConnectionInternal(connectionName);
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams($"/api/connection/current/{Uri.EscapeDataString(connectionName)}", queryParams);
                return await PostAsync<object, object>(endpoint, null!, null, cancellationToken);
            }
            return new { Success = true, Message = $"Current connection set to '{connectionName}'" };
        }

        public async Task<object> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, object>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}/create", null!, null, cancellationToken);
            throw new InvalidOperationException("Local mode not supported for creating connections");
        }
    }
}
