using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Base interface for all data loaders in the drawing framework.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    public interface IDataLoader<T> : IDisposable
    {
        /// <summary>
        /// Gets the data source identifier (connection string, file path, etc.).
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Gets whether the loader is connected to the data source.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Connects to the data source.
        /// </summary>
        /// <returns>True if connection successful, false otherwise.</returns>
        bool Connect();

        /// <summary>
        /// Connects to the data source asynchronously.
        /// </summary>
        /// <returns>True if connection successful, false otherwise.</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// Disconnects from the data source.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Loads data from the data source.
        /// </summary>
        /// <param name="criteria">Optional criteria for filtering data.</param>
        /// <returns>The loaded data.</returns>
        T Load(object criteria = null);

        /// <summary>
        /// Loads data from the data source asynchronously.
        /// </summary>
        /// <param name="criteria">Optional criteria for filtering data.</param>
        /// <returns>The loaded data.</returns>
        Task<T> LoadAsync(object criteria = null);

        /// <summary>
        /// Validates the data source connection.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        bool ValidateConnection();

        /// <summary>
        /// Gets available data identifiers (well IDs, log names, etc.).
        /// </summary>
        /// <returns>List of available identifiers.</returns>
        List<string> GetAvailableIdentifiers();
    }
}

