using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Client.Connection
{
    /// <summary>
    /// Manager for multiple connections
    /// </summary>
    public class ConnectionManager
    {
        private List<ConnectionInfo>? _cachedConnections;

        public ConnectionManager(List<ConnectionInfo> connections)
        {
            _cachedConnections = connections ?? new List<ConnectionInfo>();
        }

        public string? CurrentConnectionName { get; set; }

        public ConnectionInfo? CurrentConnection
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentConnectionName))
                    return null;
                return GetAllConnections().FirstOrDefault(c =>
                    c.ConnectionName.Equals(CurrentConnectionName, StringComparison.OrdinalIgnoreCase));
            }
        }

        public Task<List<ConnectionInfo>> LoadAllConnectionsAsync()
        {
            return Task.FromResult(_cachedConnections ?? new List<ConnectionInfo>());
        }

        public List<ConnectionInfo> GetAllConnections()
        {
            return _cachedConnections ?? new List<ConnectionInfo>();
        }

        public ConnectionInfo? GetConnection(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
                return null;
            return GetAllConnections().FirstOrDefault(c =>
                c.ConnectionName.Equals(connectionName, StringComparison.OrdinalIgnoreCase));
        }

        public List<ConnectionInfo> FilterConnections(Func<ConnectionInfo, bool> predicate)
        {
            return GetAllConnections().Where(predicate).ToList();
        }

        public async Task<bool> TestConnectionAsync(string connectionName, IDMEEditor? dmeEditor = null)
        {
            if (dmeEditor == null)
                return false;

            try
            {
                var connection = GetConnection(connectionName);
                if (connection == null)
                    return false;

                var dataSource = dmeEditor.GetDataSource(connectionName);
                if (dataSource != null)
                {
                    var state = dataSource.Openconnection();
                    var isOpen = state.ToString().Equals("Open", StringComparison.OrdinalIgnoreCase);
                    if (isOpen)
                    {
                        dataSource.Closeconnection();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
