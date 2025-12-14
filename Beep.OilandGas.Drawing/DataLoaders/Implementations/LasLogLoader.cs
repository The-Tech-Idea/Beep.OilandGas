using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.DataLoaders.PWLS;
using System.Diagnostics;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for LAS (Log ASCII Standard) format log files.
    /// </summary>
    public class LasLogLoader : ILogLoader
    {
        private string filePath;
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
        /// Initializes a new instance of the <see cref="LasLogLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the LAS file.</param>
        public LasLogLoader(string filePath)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Connects to the data source.
        /// </summary>
        /// <returns>True if connection successful.</returns>
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
        /// <param name="criteria">Optional criteria (not used for file-based loader).</param>
        /// <returns>The log data.</returns>
        public LogData Load(object criteria = null)
        {
            if (!isConnected)
                Connect();

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"LAS file not found: {filePath}");

            return ParseLasFile(filePath);
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
                    return DataLoadResult<LogData>.CreateFailure($"LAS file not found: {filePath}");

                var logData = ParseLasFile(filePath, configuration);
                
                // Apply well identifier and log name if provided
                if (!string.IsNullOrEmpty(wellIdentifier))
                    logData.WellIdentifier = wellIdentifier;
                if (!string.IsNullOrEmpty(logName))
                    logData.LogName = logName;

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

                // Interpolate missing values if configured
                if (configuration.InterpolateMissingValues)
                {
                    InterpolateMissingValues(logData, configuration.NullValue);
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
                return DataLoadResult<LogData>.CreateFailure($"Failed to load LAS file: {ex.Message}", ex.ToString());
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
        /// Loads multiple logs (not applicable for single-file LAS).
        /// </summary>
        public Dictionary<string, LogData> LoadLogs(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            var logData = LoadLog(wellIdentifier, null, configuration);
            return new Dictionary<string, LogData> { [logData.LogName ?? "LAS_LOG"] = logData };
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
            return new List<string> { logData.LogName };
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
        /// Gets available identifiers (file name).
        /// </summary>
        public List<string> GetAvailableIdentifiers()
        {
            return new List<string> { Path.GetFileNameWithoutExtension(filePath) };
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
        /// Interpolates missing values in log data.
        /// </summary>
        private void InterpolateMissingValues(LogData logData, double nullValue)
        {
            foreach (var curveName in logData.Curves.Keys)
            {
                var values = logData.Curves[curveName];
                for (int i = 0; i < values.Count; i++)
                {
                    if (Math.Abs(values[i] - nullValue) < 0.001)
                    {
                        // Find nearest non-null values
                        double? prevValue = null;
                        double? nextValue = null;

                        for (int j = i - 1; j >= 0 && prevValue == null; j--)
                        {
                            if (Math.Abs(values[j] - nullValue) >= 0.001)
                                prevValue = values[j];
                        }

                        for (int j = i + 1; j < values.Count && nextValue == null; j++)
                        {
                            if (Math.Abs(values[j] - nullValue) >= 0.001)
                                nextValue = values[j];
                        }

                        if (prevValue.HasValue && nextValue.HasValue)
                        {
                            values[i] = (prevValue.Value + nextValue.Value) / 2.0;
                        }
                        else if (prevValue.HasValue)
                        {
                            values[i] = prevValue.Value;
                        }
                        else if (nextValue.HasValue)
                        {
                            values[i] = nextValue.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parses a LAS file.
        /// </summary>
        private LogData ParseLasFile(string filePath, LogLoadConfiguration configuration = null)
        {
            var logData = new LogData();
            var lines = File.ReadAllLines(filePath);
            var currentSection = "";
            var curveNames = new List<string>();
            var curveMetadata = new Dictionary<string, LogCurveMetadata>();
            var mnemonicToDisplayName = new Dictionary<string, string>(); // For PWLS mapping

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Check for section headers
                if (trimmedLine.StartsWith("~"))
                {
                    currentSection = trimmedLine.ToUpper();
                    continue;
                }

                // Parse section content
                if (currentSection == "~VERSION INFORMATION" || currentSection == "~WELL INFORMATION")
                {
                    ParseMetadataLine(trimmedLine, logData);
                }
                else if (currentSection == "~CURVE INFORMATION")
                {
                    ParseCurveInformation(trimmedLine, curveNames, curveMetadata, logData, configuration);
                    
                    // Build mnemonic to display name mapping for PWLS
                    if (configuration?.UsePwlsMapping == true)
                    {
                        var mnemonic = curveNames.Last();
                        var displayName = PwlsMnemonicMapper.MapToPwlsProperty(mnemonic);
                        mnemonicToDisplayName[mnemonic] = displayName;
                    }
                }
                else if (currentSection == "~ASCII")
                {
                    ParseAsciiData(trimmedLine, logData, curveNames);
                }
            }

            // Apply PWLS mapping to curve dictionary keys if enabled
            if (configuration?.UsePwlsMapping == true && mnemonicToDisplayName.Any())
            {
                var remappedCurves = new Dictionary<string, List<double>>();
                var remappedMetadata = new Dictionary<string, LogCurveMetadata>();

                foreach (var kvp in logData.Curves)
                {
                    var originalMnemonic = kvp.Key;
                    var displayName = mnemonicToDisplayName.ContainsKey(originalMnemonic) 
                        ? mnemonicToDisplayName[originalMnemonic] 
                        : originalMnemonic;

                    // Add with PWLS name
                    remappedCurves[displayName] = kvp.Value;
                    if (curveMetadata.ContainsKey(originalMnemonic))
                    {
                        remappedMetadata[displayName] = curveMetadata[originalMnemonic];
                    }

                    // Keep original if configured
                    if (configuration.KeepOriginalMnemonics && displayName != originalMnemonic)
                    {
                        remappedCurves[originalMnemonic] = kvp.Value;
                        if (curveMetadata.ContainsKey(originalMnemonic))
                        {
                            remappedMetadata[originalMnemonic] = curveMetadata[originalMnemonic];
                        }
                    }
                }

                logData.Curves = remappedCurves;
                logData.CurveMetadata = remappedMetadata;
            }
            else
            {
                // Set curve metadata as-is
                logData.CurveMetadata = curveMetadata;
            }

            return logData;
        }

        /// <summary>
        /// Parses a metadata line.
        /// </summary>
        private void ParseMetadataLine(string line, LogData logData)
        {
            var parts = line.Split(new[] { '.' }, 2);
            if (parts.Length < 2)
                return;

            var mnemonic = parts[0].Trim();
            var value = parts[1].Split(':')[0].Trim();

            switch (mnemonic.ToUpper())
            {
                case "WELL":
                    logData.WellIdentifier = value;
                    break;
                case "STRT":
                    if (double.TryParse(value, out double startDepth))
                        logData.StartDepth = startDepth;
                    break;
                case "STOP":
                    if (double.TryParse(value, out double endDepth))
                        logData.EndDepth = endDepth;
                    break;
                case "STEP":
                    if (double.TryParse(value, out double step))
                        logData.DepthStep = step;
                    break;
                case "NULL":
                    // Store null value
                    break;
            }

            logData.Metadata[mnemonic] = value;
        }

        /// <summary>
        /// Parses curve information.
        /// </summary>
        private void ParseCurveInformation(string line, List<string> curveNames, Dictionary<string, LogCurveMetadata> curveMetadata, LogData logData, LogLoadConfiguration configuration = null)
        {
            var parts = line.Split(new[] { '.' }, 2);
            if (parts.Length < 2)
                return;

            var mnemonic = parts[0].Trim();
            var rest = parts[1].Split(':');
            if (rest.Length < 2)
                return;

            var unit = rest[0].Trim();
            var description = rest.Length > 1 ? rest[1].Trim() : "";

            // Apply PWLS mapping if enabled
            string displayName = mnemonic;
            if (configuration?.UsePwlsMapping == true)
            {
                displayName = PwlsMnemonicMapper.MapToPwlsProperty(mnemonic);
                
                // Store original mnemonic in metadata if keeping originals
                if (configuration.KeepOriginalMnemonics && displayName != mnemonic)
                {
                    description = $"{description} (Original: {mnemonic})";
                }
            }

            curveNames.Add(mnemonic); // Keep original for data parsing
            curveMetadata[mnemonic] = new LogCurveMetadata
            {
                Unit = unit,
                Description = description,
                Mnemonic = mnemonic,
                DisplayName = displayName // Store PWLS name if mapped
            };

            // First curve is typically depth
            if (curveNames.Count == 1)
            {
                logData.DepthUnit = unit;
            }
        }

        /// <summary>
        /// Parses ASCII data section.
        /// </summary>
        private void ParseAsciiData(string line, LogData logData, List<string> curveNames)
        {
            var values = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length < curveNames.Count)
                return;

            // First value is depth
            if (double.TryParse(values[0], out double depth))
            {
                logData.Depths.Add(depth);

                // Parse curve values
                for (int i = 1; i < curveNames.Count && i < values.Length; i++)
                {
                    var curveName = curveNames[i];
                    if (!logData.Curves.ContainsKey(curveName))
                    {
                        logData.Curves[curveName] = new List<double>();
                    }

                    if (double.TryParse(values[i], out double value))
                    {
                        logData.Curves[curveName].Add(value);
                    }
                    else
                    {
                        // Use null value
                        logData.Curves[curveName].Add(-999.25);
                    }
                }
            }
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public void Dispose()
        {
            Disconnect();
        }
    }
}

