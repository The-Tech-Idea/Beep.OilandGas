using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectionInfo = Beep.OilandGas.Models.Data.DataManagement.ConnectionInfo;
using ConnectionTestResult = Beep.OilandGas.Models.Data.DataManagement.ConnectionTestResult;
using SetCurrentConnectionResult = Beep.OilandGas.Models.Data.DataManagement.SetCurrentConnectionResult;
using CurrentConnectionResponse = Beep.OilandGas.Models.Data.DataManagement.CurrentConnectionResponse;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for connection operations
    /// </summary>
    public interface IConnectionService
    {
        Task<List<ConnectionInfo>> GetAllConnectionsAsync();
        Task<ConnectionInfo?> GetConnectionAsync(string connectionName);
        Task<ConnectionTestResult> TestConnectionAsync(string connectionName);
        Task<CurrentConnectionResponse> GetCurrentConnectionAsync();
        Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName);
    }

    /// <summary>
    /// Client service for connection management operations
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private readonly ApiClient _apiClient;
        private readonly OilGasCallFailures _calls;

        public ConnectionService(ApiClient apiClient, OilGasCallFailures calls)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _calls = calls ?? throw new ArgumentNullException(nameof(calls));
        }

        /// <summary>
        /// Get all IDMEEditor configured connections
        /// </summary>
        public async Task<List<ConnectionInfo>> GetAllConnectionsAsync()
        {
            var connections = await _apiClient.GetAsync<List<ConnectionInfo>>("/api/connections");
            return connections ?? new List<ConnectionInfo>();
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        public async Task<ConnectionInfo?> GetConnectionAsync(string connectionName)
        {
            return await _apiClient.GetAsync<ConnectionInfo>($"/api/connections/{Uri.EscapeDataString(connectionName)}");
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
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "testing a database connection", "The connection could not be tested")
                };
            }
        }

        /// <summary>
        /// Get current connection for user
        /// </summary>
        public async Task<CurrentConnectionResponse> GetCurrentConnectionAsync()
        {
            var response = await _apiClient.GetAsync<CurrentConnectionResponse>("/api/connections/current");
            return response ?? new CurrentConnectionResponse();
        }

        /// <summary>
        /// Set current connection for user
        /// </summary>
        public async Task<SetCurrentConnectionResult> SetCurrentConnectionAsync(string connectionName)
        {
            try
            {
                var request = new { ConnectionName = connectionName };
                var result = await _apiClient.PostAsync<object, SetCurrentConnectionResult>("/api/connections/set-current", request);
                return result ?? new SetCurrentConnectionResult
                {
                    Success = false,
                    Message = "Failed to set current connection"
                };
            }
            // A failed call is answered as a failed result the page shows (its contract); the store keeps the failure.
            catch (Exception failure) when (OilGasCallFailures.IsCallFailure(failure))
            {
                return new SetCurrentConnectionResult
                {
                    Success = false,
                    Message = _calls.Explain(failure, "setting the current connection", "The current connection was not changed")
                };
            }
        }
    }
}

