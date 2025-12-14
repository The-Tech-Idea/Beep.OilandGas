using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders
{
    /// <summary>
    /// Interface for loading log data from various sources (LAS, WITSML, database, etc.).
    /// </summary>
    public interface ILogLoader : IDataLoader<LogData>
    {
        /// <summary>
        /// Loads log data for a specific well and log name.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier (UWI, API, etc.).</param>
        /// <param name="logName">The log name.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The log data.</returns>
        LogData LoadLog(string wellIdentifier, string logName, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Loads log data asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logName">The log name.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The log data.</returns>
        Task<LogData> LoadLogAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Loads log data with result object.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logName">The log name.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        DataLoadResult<LogData> LoadLogWithResult(string wellIdentifier, string logName, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Loads log data with result object asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logName">The log name.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>The load result.</returns>
        Task<DataLoadResult<LogData>> LoadLogWithResultAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Loads multiple logs for a well.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logNames">Optional list of log names. If null, loads all available logs.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>Dictionary of log name to log data.</returns>
        Dictionary<string, LogData> LoadLogs(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Loads multiple logs asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logNames">Optional list of log names.</param>
        /// <param name="configuration">Optional load configuration.</param>
        /// <returns>Dictionary of log name to log data.</returns>
        Task<Dictionary<string, LogData>> LoadLogsAsync(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null);

        /// <summary>
        /// Gets available log names for a well.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <returns>List of available log names.</returns>
        List<string> GetAvailableLogs(string wellIdentifier);

        /// <summary>
        /// Gets available log names asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <returns>List of available log names.</returns>
        Task<List<string>> GetAvailableLogsAsync(string wellIdentifier);

        /// <summary>
        /// Gets log curve information for a log.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logName">The log name.</param>
        /// <returns>Collection of log curve information.</returns>
        LogCurveInfoCollection GetLogCurveInfo(string wellIdentifier, string logName);

        /// <summary>
        /// Gets log curve information asynchronously.
        /// </summary>
        /// <param name="wellIdentifier">The well identifier.</param>
        /// <param name="logName">The log name.</param>
        /// <returns>Collection of log curve information.</returns>
        Task<LogCurveInfoCollection> GetLogCurveInfoAsync(string wellIdentifier, string logName);
    }
}
