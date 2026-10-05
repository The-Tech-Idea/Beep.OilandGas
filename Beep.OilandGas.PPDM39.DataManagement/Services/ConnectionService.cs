using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for managing IDMEEditor configured connections
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01: a failure reaches the caller. Each method caught it and answered "no connections", "no such
    /// connection", an unnamed current connection, or a result carrying the exception's text. The setup service it calls
    /// reports its own failures and answers them in its results.
    /// </remarks>
    public class ConnectionService
    {
        private readonly IDMEEditor _editor;
        private readonly ILogger<ConnectionService> _logger;
        private readonly IPPDM39SetupService _setupService;

        public ConnectionService(
            IDMEEditor editor,
            IPPDM39SetupService setupService,
            ILogger<ConnectionService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all configured connections from IDMEEditor
        /// </summary>
        public List<ConnectionInfo> GetAllConnections()
        {
            var connections = _editor.ConfigEditor?.DataConnections ?? new List<ConnectionProperties>();
            
            return connections.Select(c => new ConnectionInfo
            {
                ConnectionName = c.ConnectionName ?? string.Empty,
                DatabaseType = c.DatabaseType.ToString() ?? "Unknown",
                Server = c.Host ?? string.Empty,
                Database = c.Database,
                Port = c.Port,
                IsActive = c.ConnectionName == _setupService.GetCurrentConnectionName()
                
            }).ToList();
        }

        /// <summary>
        /// Get connection details by name
        /// </summary>
        public ConnectionInfo? GetConnection(string connectionName)
        {
            var connectionConfig = _setupService.GetConnectionByName(connectionName);
            if (connectionConfig == null)
                return null;

            return new ConnectionInfo
            {
                ConnectionName = connectionConfig.ConnectionName ?? string.Empty,
                DatabaseType = connectionConfig.DatabaseType ?? "Unknown",
                Server = connectionConfig.Host ?? string.Empty,
                Database = connectionConfig.Database,
                Port = connectionConfig.Port,
                IsActive = connectionConfig.ConnectionName == _setupService.GetCurrentConnectionName()
            };
        }

        /// <summary>
        /// Test a connection
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync(string connectionName)
        {
            var connectionConfig = _setupService.GetConnectionByName(connectionName);
            if (connectionConfig == null)
            {
                return new ConnectionTestResult
                {
                    Success = false,
                    Message = $"Connection '{connectionName}' not found"
                };
            }

            var testResult = await _setupService.TestConnectionAsync(connectionConfig);
            return new ConnectionTestResult
            {
                Success = testResult.Success,
                Message = testResult.Message ?? "Connection test completed",
                ErrorDetails = testResult.ErrorDetails
            };
        }

        /// <summary>
        /// Get current connection
        /// </summary>
        public CurrentConnectionResponse GetCurrentConnection()
        {
            var connectionName = _setupService.GetCurrentConnectionName();
            return new CurrentConnectionResponse
            {
                ConnectionName = connectionName
            };
        }

        /// <summary>
        /// Set current connection
        /// </summary>
        public SetCurrentConnectionResult SetCurrentConnection(string connectionName)
        {
            var result = _setupService.SetCurrentConnection(connectionName);
            return new SetCurrentConnectionResult
            {
                Success = result.Success,
                Message = result.Message ?? "Connection set successfully",
                ErrorDetails = result.ErrorDetails
            };
        }
    }
}
