using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using System.Diagnostics;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for log data from CSV files.
    /// </summary>
    public class CsvLogLoader : ILogLoader
    {
        private readonly string filePath;
        private readonly CsvLogConfiguration configuration;
        private bool isConnected = false;

        /// <summary>
        /// Gets the data source (file path).
        /// </summary>
        public string DataSource => filePath;

        /// <summary>
        /// Gets whether the loader is connected.
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvLogLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the CSV file.</param>
        /// <param name="configuration">Optional CSV configuration.</param>
        public CsvLogLoader(string filePath, CsvLogConfiguration configuration = null)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            this.configuration = configuration ?? new CsvLogConfiguration();
        }

        /// <summary>
        /// Connects to the data source.
        /// </summary>
        public bool Connect()
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return false;

            isConnected = true;
            return true;
        }

        /// <summary>
        /// Connects asynchronously.
        /// </summary>
        public Task<bool> ConnectAsync()
        {
            return Task.FromResult(Connect());
        }

        /// <summary>
        /// Disconnects from the data source.
        /// </summary>
        public void Disconnect()
        {
            isConnected = false;
        }

        /// <summary>
        /// Loads log data.
        /// </summary>
        public LogData Load(object criteria = null)
        {
            if (!isConnected)
                Connect();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"CSV file not found: {filePath}");

            return ParseCsvFile(filePath);
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

                if (!File.Exists(filePath))
                    return DataLoadResult<LogData>.CreateFailure($"CSV file not found: {filePath}");

                var logData = ParseCsvFile(filePath);
                logData.WellIdentifier = wellIdentifier ?? logData.WellIdentifier;
                logData.LogName = logName ?? logData.LogName;

                // Apply depth filtering if configured
                if (configuration.MinDepth > 0 || configuration.MaxDepth > 0)
                {
                    FilterByDepth(logData, configuration.MinDepth, configuration.MaxDepth);
                }

                // Apply curve filtering if configured
                if (configuration.CurvesToLoad != null && configuration.CurvesToLoad.Count > 0)
                {
                    FilterCurves(logData, configuration.CurvesToLoad);
                }

                stats.RecordsLoaded = logData.DataPointCount;
                stats.Complete();

                var result = DataLoadResult<LogData>.CreateSuccess(logData, logData.DataPointCount);
                result.LoadDuration = stats.Duration;
                result.Metadata = new Dictionary<string, object>
                {
                    ["FileSize"] = new FileInfo(filePath).Length,
                    ["CurveCount"] = logData.Curves.Count
                };
                return result;
            }
            catch (Exception ex)
            {
                stats.Complete();
                return DataLoadResult<LogData>.CreateFailure($"Failed to load CSV file: {ex.Message}", ex.ToString());
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
                    Unit = metadata?.Unit ?? configuration?.DefaultUnit ?? "",
                    Description = metadata?.Description ?? curveName,
                    NullValue = metadata?.NullValue ?? configuration?.NullValue ?? -999.25,
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
        /// Filters log data by depth range.
        /// </summary>
        private void FilterByDepth(LogData logData, double minDepth, double maxDepth)
        {
            if (logData.Depths == null || logData.Depths.Count == 0)
                return;

            var filteredDepths = new List<double>();
            var filteredIndices = new List<int>();

            for (int i = 0; i < logData.Depths.Count; i++)
            {
                double depth = logData.Depths[i];
                if ((minDepth == 0 || depth >= minDepth) && (maxDepth == 0 || depth <= maxDepth))
                {
                    filteredDepths.Add(depth);
                    filteredIndices.Add(i);
                }
            }

            logData.Depths = filteredDepths;

            // Filter curve values
            foreach (var curveName in logData.Curves.Keys.ToList())
            {
                var originalValues = logData.Curves[curveName];
                var filteredValues = filteredIndices.Select(idx => originalValues[idx]).ToList();
                logData.Curves[curveName] = filteredValues;
            }

            // Update depth range
            if (filteredDepths.Count > 0)
            {
                logData.StartDepth = filteredDepths.Min();
                logData.EndDepth = filteredDepths.Max();
            }
        }

        /// <summary>
        /// Filters curves to only include specified curves.
        /// </summary>
        private void FilterCurves(LogData logData, List<string> curvesToLoad)
        {
            var curvesToRemove = logData.Curves.Keys.Where(c => !curvesToLoad.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
            foreach (var curveName in curvesToRemove)
            {
                logData.Curves.Remove(curveName);
                logData.CurveMetadata.Remove(curveName);
            }
        }

        /// <summary>
        /// Loads multiple logs (not applicable for single-file CSV).
        /// </summary>
        public Dictionary<string, LogData> LoadLogs(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            var logData = LoadLog(wellIdentifier, logNames?.FirstOrDefault(), configuration);
            return new Dictionary<string, LogData> { [logData.LogName ?? "CSV_LOG"] = logData };
        }

        /// <summary>
        /// Loads multiple logs asynchronously.
        /// </summary>
        public Task<Dictionary<string, LogData>> LoadLogsAsync(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadLogs(wellIdentifier, logNames, configuration));
        }

        /// <summary>
        /// Gets available log names.
        /// </summary>
        public List<string> GetAvailableLogs(string wellIdentifier)
        {
            var logData = Load();
            return new List<string> { logData.LogName ?? "CSV_LOG" };
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
            return File.Exists(filePath);
        }

        /// <summary>
        /// Gets available identifiers.
        /// </summary>
        public List<string> GetAvailableIdentifiers()
        {
            return new List<string> { Path.GetFileNameWithoutExtension(filePath) };
        }

        /// <summary>
        /// Parses a CSV file.
        /// </summary>
        private LogData ParseCsvFile(string filePath)
        {
            var logData = new LogData
            {
                LogName = Path.GetFileNameWithoutExtension(filePath),
                DepthUnit = configuration.DepthUnit ?? "feet"
            };

            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return logData;

            // Parse header
            var headerLine = lines[0];
            var headers = ParseCsvLine(headerLine);
            
            // Find depth column
            int depthColumnIndex = -1;
            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i].Trim().ToLower();
                if (header.Contains("depth") || header == "md" || header == "tvd")
                {
                    depthColumnIndex = i;
                    break;
                }
            }

            if (depthColumnIndex == -1 && headers.Count > 0)
            {
                // Assume first column is depth
                depthColumnIndex = 0;
            }

            // Initialize curve dictionaries
            var curveNames = new List<string>();
            for (int i = 0; i < headers.Count; i++)
            {
                if (i != depthColumnIndex)
                {
                    var curveName = headers[i].Trim();
                    if (!string.IsNullOrEmpty(curveName))
                    {
                        curveNames.Add(curveName);
                        logData.Curves[curveName] = new List<double>();
                        logData.CurveMetadata[curveName] = new LogCurveMetadata
                        {
                            Unit = configuration.DefaultUnit ?? "",
                            Description = curveName
                        };
                    }
                }
            }

            // Parse data rows
            int startRow = configuration.HasHeader ? 1 : 0;
            double? minDepth = null;
            double? maxDepth = null;

            for (int row = startRow; row < lines.Length; row++)
            {
                var values = ParseCsvLine(lines[row]);
                if (values.Count <= depthColumnIndex)
                    continue;

                // Parse depth
                if (double.TryParse(values[depthColumnIndex].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double depth))
                {
                    logData.Depths.Add(depth);

                    if (minDepth == null || depth < minDepth)
                        minDepth = depth;
                    if (maxDepth == null || depth > maxDepth)
                        maxDepth = depth;

                    // Parse curve values
                    for (int i = 0; i < curveNames.Count; i++)
                    {
                        int valueIndex = i < depthColumnIndex ? i : i + 1;
                        if (valueIndex < values.Count)
                        {
                            var valueStr = values[valueIndex].Trim();
                            if (double.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                            {
                                logData.Curves[curveNames[i]].Add(value);
                            }
                            else
                            {
                                // Use null value
                                logData.Curves[curveNames[i]].Add(configuration.NullValue);
                            }
                        }
                        else
                        {
                            logData.Curves[curveNames[i]].Add(configuration.NullValue);
                        }
                    }
                }
            }

            // Set depth range
            if (minDepth.HasValue && maxDepth.HasValue)
            {
                logData.StartDepth = minDepth.Value;
                logData.EndDepth = maxDepth.Value;
                if (logData.Depths.Count > 1)
                {
                    logData.DepthStep = (maxDepth.Value - minDepth.Value) / (logData.Depths.Count - 1);
                }
            }

            return logData;
        }

        /// <summary>
        /// Parses a CSV line, handling quoted values.
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            bool inQuotes = false;
            string currentValue = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if ((c == configuration.Separator || c == ',') && !inQuotes)
                {
                    values.Add(currentValue);
                    currentValue = "";
                }
                else
                {
                    currentValue += c;
                }
            }

            values.Add(currentValue);
            return values;
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }

    /// <summary>
    /// Configuration for CSV log loading.
    /// </summary>
    public class CsvLogConfiguration
    {
        /// <summary>
        /// Gets or sets whether the CSV has a header row.
        /// </summary>
        public bool HasHeader { get; set; } = true;

        /// <summary>
        /// Gets or sets the column separator.
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// Gets or sets the depth unit.
        /// </summary>
        public string DepthUnit { get; set; } = "feet";

        /// <summary>
        /// Gets or sets the default unit for curves.
        /// </summary>
        public string DefaultUnit { get; set; } = "";

        /// <summary>
        /// Gets or sets the null value.
        /// </summary>
        public double NullValue { get; set; } = -999.25;
    }
}

