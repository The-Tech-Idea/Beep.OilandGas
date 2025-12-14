using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using System.Diagnostics;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for log data from database sources.
    /// </summary>
    public class DatabaseLogLoader : ILogLoader
    {
        private readonly string connectionString;
        private DbConnection connection;
        private bool isConnected = false;
        private readonly Func<DbConnection> connectionFactory;

        /// <summary>
        /// Gets the data source (connection string).
        /// </summary>
        public string DataSource => connectionString;

        /// <summary>
        /// Gets whether the loader is connected.
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseLogLoader"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public DatabaseLogLoader(string connectionString, Func<DbConnection> connectionFactory = null)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Connects to the database.
        /// </summary>
        public bool Connect()
        {
            try
            {
                if (connectionFactory != null)
                {
                    connection = connectionFactory();
                }
                else
                {
                    throw new NotImplementedException("Connection factory must be provided.");
                }

                if (connection.State != ConnectionState.Open)
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }

                isConnected = true;
                return true;
            }
            catch
            {
                isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (connectionFactory != null)
                {
                    connection = connectionFactory();
                }
                else
                {
                    throw new NotImplementedException("Connection factory must be provided.");
                }

                if (connection.State != ConnectionState.Open)
                {
                    connection.ConnectionString = connectionString;
                    await connection.OpenAsync();
                }

                isConnected = true;
                return true;
            }
            catch
            {
                isConnected = false;
                return false;
            }
        }

        /// <summary>
        /// Disconnects from the database.
        /// </summary>
        public void Disconnect()
        {
            if (connection != null && connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            isConnected = false;
        }

        /// <summary>
        /// Loads log data.
        /// </summary>
        public LogData Load(object criteria = null)
        {
            if (criteria is Dictionary<string, object> dict)
            {
                var wellId = dict.ContainsKey("WellIdentifier") ? dict["WellIdentifier"]?.ToString() : null;
                var logName = dict.ContainsKey("LogName") ? dict["LogName"]?.ToString() : null;
                return LoadLog(wellId, logName);
            }
            throw new ArgumentException("Criteria must contain WellIdentifier and LogName.");
        }

        /// <summary>
        /// Loads log data asynchronously.
        /// </summary>
        public Task<LogData> LoadAsync(object criteria = null)
        {
            return Task.Run(() => Load(criteria));
        }

        /// <summary>
        /// Loads log data for a specific well and log name.
        /// </summary>
        public LogData LoadLog(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            var result = LoadLogWithResult(wellIdentifier, logName, configuration);
            return result.Data;
        }

        /// <summary>
        /// Loads log data asynchronously.
        /// </summary>
        public Task<LogData> LoadLogAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            return Task.Run(async () =>
            {
                var result = await LoadLogWithResultAsync(wellIdentifier, logName, configuration);
                return result.Data;
            });
        }

        /// <summary>
        /// Loads log data with result object.
        /// </summary>
        public DataLoadResult<LogData> LoadLogWithResult(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            var stats = new DataLoadStatistics();
            configuration = configuration ?? new LogLoadConfiguration();

            try
            {
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return DataLoadResult<LogData>.CreateFailure("Database connection is not open.");

                var logData = new LogData
                {
                    WellIdentifier = wellIdentifier,
                    LogName = logName
                };

                // Build query based on configuration
                var query = BuildLogQuery(wellIdentifier, logName, configuration);
                
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    AddQueryParameters(command, wellIdentifier, logName, configuration);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            LoadLogDataFromReader(reader, logData, configuration);
                        }
                    }
                }

                stats.RecordsLoaded = logData.DataPointCount;
                stats.Complete();

                var result = DataLoadResult<LogData>.CreateSuccess(logData, logData.DataPointCount);
                result.LoadDuration = stats.Duration;
                return result;
            }
            catch (Exception ex)
            {
                stats.Complete();
                return DataLoadResult<LogData>.CreateFailure($"Failed to load log data: {ex.Message}", ex.ToString());
            }
        }

        /// <summary>
        /// Loads log data with result object asynchronously.
        /// </summary>
        public Task<DataLoadResult<LogData>> LoadLogWithResultAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadLogWithResult(wellIdentifier, logName, configuration));
        }

        /// <summary>
        /// Gets log curve information.
        /// </summary>
        public LogCurveInfoCollection GetLogCurveInfo(string wellIdentifier, string logName)
        {
            var logData = LoadLog(wellIdentifier, logName);
            var curves = new LogCurveInfoCollection();

            foreach (var curveName in logData.Curves.Keys)
            {
                var metadata = logData.CurveMetadata.ContainsKey(curveName) ? logData.CurveMetadata[curveName] : null;
                var values = logData.Curves[curveName];

                curves.Add(new LogCurveInfo
                {
                    Name = curveName,
                    Unit = metadata?.Unit ?? "",
                    Description = metadata?.Description ?? curveName,
                    NullValue = metadata?.NullValue ?? -999.25,
                    DataPointCount = values.Count,
                    MinValue = values.Count > 0 ? values.Min() : null,
                    MaxValue = values.Count > 0 ? values.Max() : null,
                    DataType = "float"
                });
            }

            return curves;
        }

        /// <summary>
        /// Gets log curve information asynchronously.
        /// </summary>
        public Task<LogCurveInfoCollection> GetLogCurveInfoAsync(string wellIdentifier, string logName)
        {
            return Task.Run(() => GetLogCurveInfo(wellIdentifier, logName));
        }

        /// <summary>
        /// Builds SQL query for loading log data.
        /// </summary>
        private string BuildLogQuery(string wellIdentifier, string logName, LogLoadConfiguration configuration)
        {
            // Generic query structure - should be customized per database schema
            var query = "SELECT Depth";
            
            if (configuration.CurvesToLoad != null && configuration.CurvesToLoad.Count > 0)
            {
                query += ", " + string.Join(", ", configuration.CurvesToLoad);
            }
            else
            {
                query += ", *"; // Load all curves
            }

            query += " FROM LOG_DATA WHERE WellID = @wellIdentifier AND LogName = @logName";

            if (configuration.MinDepth > 0)
                query += " AND Depth >= @minDepth";
            if (configuration.MaxDepth > 0)
                query += " AND Depth <= @maxDepth";

            query += " ORDER BY Depth";

            return query;
        }

        /// <summary>
        /// Adds parameters to the query command.
        /// </summary>
        private void AddQueryParameters(DbCommand command, string wellIdentifier, string logName, LogLoadConfiguration configuration)
        {
            var param = command.CreateParameter();
            param.ParameterName = "@wellIdentifier";
            param.Value = wellIdentifier;
            command.Parameters.Add(param);

            param = command.CreateParameter();
            param.ParameterName = "@logName";
            param.Value = logName;
            command.Parameters.Add(param);

            if (configuration.MinDepth > 0)
            {
                param = command.CreateParameter();
                param.ParameterName = "@minDepth";
                param.Value = configuration.MinDepth;
                command.Parameters.Add(param);
            }

            if (configuration.MaxDepth > 0)
            {
                param = command.CreateParameter();
                param.ParameterName = "@maxDepth";
                param.Value = configuration.MaxDepth;
                command.Parameters.Add(param);
            }
        }

        /// <summary>
        /// Loads log data from a data reader.
        /// </summary>
        private void LoadLogDataFromReader(DbDataReader reader, LogData logData, LogLoadConfiguration configuration)
        {
            var depths = new List<double>();
            var curveNames = new List<string>();
            var curveData = new Dictionary<string, List<double>>();

            // Get curve names from columns (skip Depth column)
            for (int i = 1; i < reader.FieldCount; i++)
            {
                var curveName = reader.GetName(i);
                curveNames.Add(curveName);
                curveData[curveName] = new List<double>();
                logData.CurveMetadata[curveName] = new LogCurveMetadata
                {
                    NullValue = configuration.NullValue
                };
            }

            while (reader.Read())
            {
                double depth = reader.GetDouble(0);
                depths.Add(depth);

                for (int i = 0; i < curveNames.Count; i++)
                {
                    int colIndex = i + 1;
                    if (reader.IsDBNull(colIndex))
                    {
                        curveData[curveNames[i]].Add(configuration.NullValue);
                    }
                    else
                    {
                        curveData[curveNames[i]].Add(reader.GetDouble(colIndex));
                    }
                }
            }

            logData.Depths = depths;
            logData.Curves = curveData;

            if (depths.Count > 0)
            {
                logData.StartDepth = depths.Min();
                logData.EndDepth = depths.Max();
                if (depths.Count > 1)
                {
                    logData.DepthStep = (logData.EndDepth - logData.StartDepth) / (depths.Count - 1);
                }
            }
        }

        /// <summary>
        /// Loads multiple logs for a well.
        /// </summary>
        public Dictionary<string, LogData> LoadLogs(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            var result = new Dictionary<string, LogData>();

            if (logNames == null || logNames.Count == 0)
            {
                logNames = GetAvailableLogs(wellIdentifier);
            }

            foreach (var logName in logNames)
            {
                result[logName] = LoadLog(wellIdentifier, logName, configuration);
            }

            return result;
        }

        /// <summary>
        /// Loads multiple logs asynchronously.
        /// </summary>
        public async Task<Dictionary<string, LogData>> LoadLogsAsync(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            if (logNames == null || logNames.Count == 0)
            {
                logNames = await GetAvailableLogsAsync(wellIdentifier);
            }

            var tasks = logNames.Select(async name => new { Name = name, Data = await LoadLogAsync(wellIdentifier, name, configuration) });
            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(r => r.Name, r => r.Data);
        }

        /// <summary>
        /// Gets available log names for a well.
        /// </summary>
        public List<string> GetAvailableLogs(string wellIdentifier)
        {
            if (!isConnected)
                Connect();

            if (connection == null || connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            var logNames = new List<string>();
            // TODO: Implement query to get log names
            // Example: SELECT DISTINCT LogName FROM LOGS WHERE WellID = @wellIdentifier
            return logNames;
        }

        /// <summary>
        /// Gets available log names asynchronously.
        /// </summary>
        public Task<List<string>> GetAvailableLogsAsync(string wellIdentifier)
        {
            return Task.Run(() => GetAvailableLogs(wellIdentifier));
        }

        /// <summary>
        /// Validates the connection.
        /// </summary>
        public bool ValidateConnection()
        {
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                    return false;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT 1";
                    command.ExecuteScalar();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets available identifiers.
        /// </summary>
        public List<string> GetAvailableIdentifiers()
        {
            // TODO: Implement query to get well identifiers
            return new List<string>();
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
            connection?.Dispose();
        }
    }
}

