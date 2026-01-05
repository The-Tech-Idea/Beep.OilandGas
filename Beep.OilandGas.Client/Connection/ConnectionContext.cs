using System;
using System.Threading;

namespace Beep.OilandGas.Client.Connection
{
    /// <summary>
    /// Context class for managing connection state
    /// Thread-safe for async operations
    /// </summary>
    public class ConnectionContext
    {
        private readonly AsyncLocal<string?> _connectionName = new AsyncLocal<string?>();

        /// <summary>
        /// Gets or sets the current connection name for this context
        /// </summary>
        public string? ConnectionName
        {
            get => _connectionName.Value;
            set => _connectionName.Value = value;
        }

        /// <summary>
        /// Clears the connection name
        /// </summary>
        public void Clear()
        {
            _connectionName.Value = null;
        }
    }
}

