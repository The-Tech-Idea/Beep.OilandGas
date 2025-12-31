using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Connection information model
    /// </summary>
    public class ConnectionInfo
    {
        public string ConnectionName { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string? Database { get; set; }
        public int? Port { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Connection test result
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Set current connection result
    /// </summary>
    public class SetCurrentConnectionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// Current connection response
    /// </summary>
    public class CurrentConnectionResponse
    {
        public string? ConnectionName { get; set; }
    }

    /// <summary>
    /// Service interface for connection operations
    /// </summary>
    public interface IConnectionService
    {
        Task<List<ConnectionInfo>> GetAllConnectionsAsync();
        Task<ConnectionInfo?> GetConnectionAsync(string connectionName);
        Task<ConnectionTestResult> TestConnectionAsync(string connectionName);
        Task<CurrentConnectionResponse> GetCurrentConnectionAsync();
        Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName, string? userId = null);
    }

    /// <summary>
    /// Client service for connection management operations
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(
            ApiClient apiClient,
            ILogger<ConnectionService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all IDMEEditor configured connections
        /// </summary>
        public async Task<List<ConnectionInfo>> GetAllConnectionsAsync()
        {
            try
            {
                var connections = await _apiClient.GetAsync<List<ConnectionInfo>>("/api/connections");
                return connections ?? new List<ConnectionInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all connections");
                return new List<ConnectionInfo>();
            }
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        public async Task<ConnectionInfo?> GetConnectionAsync(string connectionName)
        {
            try
            {
                return await _apiClient.GetAsync<ConnectionInfo>($"/api/connections/{Uri.EscapeDataString(connectionName)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection {ConnectionName}", connectionName);
                return null;
            }
        }

        /// <summary>
        /// Test a database connection
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync(string connectionName)
        {
            try
            {
                var request = new { ConnectionName = connectionName };
                var result = await _apiClient.PostAsync<object, ConnectionTestResult>("/api/connections/test", request);
                return result ?? new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection {ConnectionName}", connectionName);
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Get current connection for user
        /// </summary>
        public async Task<CurrentConnectionResponse> GetCurrentConnectionAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<CurrentConnectionResponse>("/api/connections/current");
                return response ?? new CurrentConnectionResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current connection");
                return new CurrentConnectionResponse();
            }
        }

        /// <summary>
        /// Set current connection for user
        /// </summary>
        public async Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName, string? userId = null)
        {
            try
            {
                var request = new { ConnectionName = connectionName, UserId = userId };
                var result = await _apiClient.PostAsync<object, SetCurrentConnectionResult>("/api/connections/set-current", request);
                return result ?? new SetCurrentConnectionResult
                {
                    Success = false,
                    Message = "Failed to set current connection"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current connection {ConnectionName}", connectionName);
                return new SetCurrentConnectionResult
                {
                    Success = false,
                    Message = "Failed to set current connection",
                    ErrorDetails = ex.Message
                };
            }
        }
    }
}

