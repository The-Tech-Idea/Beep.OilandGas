using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.DataManagement;

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

        public async Task<List<ConnectionInfo>> GetAllConnectionsAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<List<ConnectionInfo>>("/api/connection/connections", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ConnectionInfo> GetConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<ConnectionInfo>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<ConnectionTestResult> TestConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<ConnectionTestResult>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}/test", cancellationToken);
            throw new InvalidOperationException("Local mode not yet implemented");
        }

        public async Task<CurrentConnectionResponse> GetCurrentConnectionAsync(CancellationToken cancellationToken = default)
        {
            if (AccessMode == ServiceAccessMode.Remote)
                return await GetAsync<CurrentConnectionResponse>("/api/connection/current", cancellationToken);
            return new CurrentConnectionResponse { ConnectionName = CurrentConnectionName };
        }

        public async Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName, string? userId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            _app.SetCurrentConnectionInternal(connectionName);
            if (AccessMode == ServiceAccessMode.Remote)
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(userId)) queryParams["userId"] = userId;
                var endpoint = BuildRequestUriWithParams($"/api/connection/current/{Uri.EscapeDataString(connectionName)}", queryParams);
                return await PostAsync<object, SetCurrentConnectionResult>(endpoint, null!, cancellationToken);
            }
            return new SetCurrentConnectionResult { Success = true, Message = $"Current connection set to '{connectionName}'" };
        }

        public async Task<CreateConnectionResult> CreateConnectionAsync(string connectionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(connectionName)) throw new ArgumentException("Connection name is required", nameof(connectionName));
            if (AccessMode == ServiceAccessMode.Remote)
                return await PostAsync<object, CreateConnectionResult>($"/api/connection/connections/{Uri.EscapeDataString(connectionName)}/create", null!, cancellationToken);
            throw new InvalidOperationException("Local mode not supported for creating connections");
        }
    }
}
