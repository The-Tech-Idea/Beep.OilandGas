using System;
using System.Collections.Concurrent;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Client.Connection
{
    /// <summary>
    /// Manages data source lifecycle for local mode
    /// </summary>
    public class DataSourceManager : IDisposable
    {
        private readonly IDMEEditor _dmeEditor;
        private readonly ConcurrentDictionary<string, object?> _dataSources = new();
        private bool _disposed;

        public DataSourceManager(IDMEEditor dmeEditor)
        {
            _dmeEditor = dmeEditor ?? throw new ArgumentNullException(nameof(dmeEditor));
        }

        public object? GetOrCreateDataSource(string connectionName)
        {
            if (string.IsNullOrEmpty(connectionName))
                throw new ArgumentException("Connection name is required", nameof(connectionName));

            return _dataSources.GetOrAdd(connectionName, name =>
            {
                var dataSource = _dmeEditor.GetDataSource(name);
                if (dataSource != null)
                {
                    dataSource.Openconnection();
                }
                return dataSource;
            });
        }

        public void ReleaseDataSource(string connectionName)
        {
            if (_dataSources.TryRemove(connectionName, out var dataSource))
            {
                if (dataSource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var kvp in _dataSources)
                {
                    if (kvp.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _dataSources.Clear();
                _disposed = true;
            }
        }
    }
}
