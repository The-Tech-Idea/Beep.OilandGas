using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.DataLoaders.PWLS;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for WITSML Log data (WITSML v1.4, v2.0, v2.1).
    /// WITSML is an Energistics standard for well data exchange.
    /// </summary>
    public class WitsmlLogLoader : ILogLoader
    {
        private readonly string filePath;
        private XDocument document;
        private bool isConnected = false;

        // WITSML namespaces (support multiple versions)
        private readonly XNamespace witsml14 = "http://www.witsml.org/schemas/1series";
        private readonly XNamespace witsml20 = "http://www.energistics.org/schemas/witsmlv2";
        private readonly XNamespace eml = "http://www.energistics.org/schemas/eml";

        public string DataSource => filePath;
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="WitsmlLogLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the WITSML XML file.</param>
        public WitsmlLogLoader(string filePath)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public bool Connect()
        {
            if (isConnected) return true;

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"WITSML file not found: {filePath}");
                }

                document = XDocument.Load(filePath);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to WITSML file: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            if (isConnected) return true;

            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"WITSML file not found: {filePath}");
                }

                using (var stream = File.OpenRead(filePath))
                {
                    document = await XDocument.LoadAsync(stream, LoadOptions.None, default);
                }
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to WITSML file: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public void Disconnect()
        {
            document = null;
            isConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public bool ValidateConnection()
        {
            if (!isConnected || document == null) return false;

            try
            {
                var root = document.Root;
                return root != null && (
                    root.Name.Namespace == witsml14 ||
                    root.Name.Namespace == witsml20 ||
                    root.Elements().Any(e => e.Name.Namespace == witsml14 || e.Name.Namespace == witsml20)
                );
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAvailableIdentifiers()
        {
            if (!isConnected) Connect();
            if (!isConnected) return new List<string>();

            var identifiers = new List<string>();

            try
            {
                var ns = DetectWitsmlNamespace();
                var logElements = document.Descendants(ns + "log");

                foreach (var log in logElements)
                {
                    var uid = log.Element(ns + "uid")?.Value;
                    var name = log.Element(ns + "name")?.Value;
                    
                    if (!string.IsNullOrEmpty(uid))
                        identifiers.Add(uid);
                    else if (!string.IsNullOrEmpty(name))
                        identifiers.Add(name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available identifiers: {ex.Message}");
            }

            return identifiers;
        }

        public Task<List<string>> GetAvailableIdentifiersAsync()
        {
            return Task.Run(() => GetAvailableIdentifiers());
        }

        public LogData Load(object criteria = null)
        {
            var identifiers = GetAvailableIdentifiers();
            if (identifiers.Count == 0)
                return new LogData();

            return LoadLog(identifiers[0], null);
        }

        public Task<LogData> LoadAsync(object criteria = null)
        {
            return Task.Run(() => Load(criteria));
        }

        public LogData LoadLog(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            var result = LoadLogWithResult(wellIdentifier, logName, configuration);
            return result.Data;
        }

        public async Task<LogData> LoadLogAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            var result = await LoadLogWithResultAsync(wellIdentifier, logName, configuration);
            return result.Data;
        }

        public DataLoadResult<LogData> LoadLogWithResult(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            var result = new DataLoadResult<LogData> { Data = new LogData() };
            var stats = new DataLoadStatistics();
            stats.StartTime = DateTime.UtcNow;

            configuration = configuration ?? new LogLoadConfiguration();

            try
            {
                if (!isConnected) Connect();
                if (!isConnected)
                {
                    result.Success = false;
                    result.Errors.Add("Failed to connect to WITSML file.");
                    return result;
                }

                var ns = DetectWitsmlNamespace();
                var logElement = FindLogElement(ns, wellIdentifier, logName);

                if (logElement == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Log not found: {logName ?? "default"}");
                    return result;
                }

                // Extract log data
                ExtractLogData(ns, logElement, result.Data, configuration, stats);

                // Apply PWLS mapping if enabled
                if (configuration.UsePwlsMapping)
                {
                    ApplyPwlsMapping(result.Data, configuration);
                }

                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error loading WITSML log: {ex.Message}");
                Console.WriteLine($"Exception in LoadLogWithResult: {ex}");
            }
            finally
            {
                stats.EndTime = DateTime.UtcNow;
                result.Statistics = stats;
            }

            return result;
        }

        public async Task<DataLoadResult<LogData>> LoadLogWithResultAsync(string wellIdentifier, string logName, LogLoadConfiguration configuration = null)
        {
            return await Task.Run(() => LoadLogWithResult(wellIdentifier, logName, configuration));
        }

        public Dictionary<string, LogData> LoadLogs(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            var result = new Dictionary<string, LogData>();

            if (!isConnected) Connect();
            if (!isConnected) return result;

            var ns = DetectWitsmlNamespace();
            var logElements = document.Descendants(ns + "log");

            var logsToLoad = logNames != null && logNames.Any()
                ? logElements.Where(log => 
                    logNames.Contains(log.Element(ns + "uid")?.Value ?? log.Element(ns + "name")?.Value))
                : logElements;

            foreach (var logElement in logsToLoad)
            {
                var logUid = logElement.Element(ns + "uid")?.Value;
                var logName = logElement.Element(ns + "name")?.Value ?? logUid;
                var logData = LoadLog(wellIdentifier, logName, configuration);
                result[logName] = logData;
            }

            return result;
        }

        public async Task<Dictionary<string, LogData>> LoadLogsAsync(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            return await Task.Run(() => LoadLogs(wellIdentifier, logNames, configuration));
        }

        public List<string> GetAvailableLogs(string wellIdentifier)
        {
            if (!isConnected) Connect();
            if (!isConnected) return new List<string>();

            var ns = DetectWitsmlNamespace();
            var logElements = document.Descendants(ns + "log");

            return logElements
                .Select(log => log.Element(ns + "name")?.Value ?? log.Element(ns + "uid")?.Value)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        public async Task<List<string>> GetAvailableLogsAsync(string wellIdentifier)
        {
            return await Task.Run(() => GetAvailableLogs(wellIdentifier));
        }

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
                    Name = metadata?.DisplayName ?? curveName,
                    Mnemonic = metadata?.Mnemonic ?? curveName,
                    Unit = metadata?.Unit ?? "",
                    Description = metadata?.Description ?? curveName,
                    DataPointCount = values.Count,
                    MinValue = values.Count > 0 ? values.Min() : null,
                    MaxValue = values.Count > 0 ? values.Max() : null,
                    DataType = "float"
                });
            }

            return curves;
        }

        public Task<LogCurveInfoCollection> GetLogCurveInfoAsync(string wellIdentifier, string logName)
        {
            return Task.Run(() => GetLogCurveInfo(wellIdentifier, logName));
        }

        #region Private Helper Methods

        private XNamespace DetectWitsmlNamespace()
        {
            var root = document.Root;
            if (root == null) return witsml20;

            // Check root namespace
            if (root.Name.Namespace == witsml14) return witsml14;
            if (root.Name.Namespace == witsml20) return witsml20;

            // Check child elements
            var firstElement = root.Elements().FirstOrDefault();
            if (firstElement != null)
            {
                if (firstElement.Name.Namespace == witsml14) return witsml14;
                if (firstElement.Name.Namespace == witsml20) return witsml20;
            }

            // Default to v2.0
            return witsml20;
        }

        private XElement FindLogElement(XNamespace ns, string wellIdentifier, string logName)
        {
            var logElements = document.Descendants(ns + "log");

            if (!string.IsNullOrEmpty(logName))
            {
                // Try to find by UID first
                var byUid = logElements.FirstOrDefault(log => log.Element(ns + "uid")?.Value == logName);
                if (byUid != null) return byUid;

                // Try to find by name
                var byName = logElements.FirstOrDefault(log => log.Element(ns + "name")?.Value == logName);
                if (byName != null) return byName;
            }

            // Return first log if well identifier matches or no specific log requested
            var matchingLog = logElements.FirstOrDefault(log =>
            {
                var wellUid = log.Element(ns + "wellUid")?.Value;
                var wellboreUid = log.Element(ns + "wellboreUid")?.Value;
                return wellUid == wellIdentifier || wellboreUid == wellIdentifier;
            });

            return matchingLog ?? logElements.FirstOrDefault();
        }

        private void ExtractLogData(XNamespace ns, XElement logElement, LogData logData, LogLoadConfiguration configuration, DataLoadStatistics stats)
        {
            // Extract basic log information
            logData.LogName = logElement.Element(ns + "name")?.Value ?? logElement.Element(ns + "uid")?.Value;
            logData.WellIdentifier = logElement.Element(ns + "wellUid")?.Value ?? logElement.Element(ns + "wellboreUid")?.Value;
            logData.LogType = logElement.Element(ns + "objectGrowing")?.Value == "true" ? "Real-time" : "Static";

            // Extract start/stop index
            var startIndex = logElement.Element(ns + "startIndex");
            var endIndex = logElement.Element(ns + "endIndex");

            if (startIndex != null)
            {
                var startValue = startIndex.Element(ns + "value")?.Value;
                if (double.TryParse(startValue, out var start))
                    logData.StartDepth = start;
            }

            if (endIndex != null)
            {
                var endValue = endIndex.Element(ns + "value")?.Value;
                if (double.TryParse(endValue, out var end))
                    logData.EndDepth = end;
            }

            // Extract index curve (depth)
            var indexCurve = logElement.Element(ns + "indexCurve")?.Value ?? "MD";
            logData.DepthUnit = logElement.Element(ns + "indexType")?.Value ?? "measured depth";

            // Extract log curves
            var logCurves = logElement.Elements(ns + "logCurveInfo").ToList();
            var curveNames = new List<string> { indexCurve };

            foreach (var curveInfo in logCurves)
            {
                var mnemonic = curveInfo.Element(ns + "mnemonic")?.Value;
                if (!string.IsNullOrEmpty(mnemonic) && mnemonic != indexCurve)
                {
                    curveNames.Add(mnemonic);
                }
            }

            // Extract log data
            var logDataElement = logElement.Element(ns + "data");
            if (logDataElement != null)
            {
                ExtractLogDataValues(ns, logDataElement, curveNames, logData, configuration, stats);
            }

            // Extract curve metadata
            foreach (var curveInfo in logCurves)
            {
                var mnemonic = curveInfo.Element(ns + "mnemonic")?.Value;
                if (string.IsNullOrEmpty(mnemonic)) continue;

                logData.CurveMetadata[mnemonic] = new LogCurveMetadata
                {
                    Mnemonic = mnemonic,
                    DisplayName = mnemonic,
                    Unit = curveInfo.Element(ns + "unit")?.Value ?? "",
                    Description = curveInfo.Element(ns + "curveDescription")?.Value ?? mnemonic
                };
            }
        }

        private void ExtractLogDataValues(XNamespace ns, XElement dataElement, List<string> curveNames, LogData logData, LogLoadConfiguration configuration, DataLoadStatistics stats)
        {
            var dataText = dataElement.Value;
            var lines = dataText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = line.Split(new[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < curveNames.Count) continue;

                // First value is typically depth/index
                if (double.TryParse(values[0], out var depth))
                {
                    // Apply depth filter
                    if (configuration.MinDepth > 0 && depth < configuration.MinDepth) continue;
                    if (configuration.MaxDepth > 0 && depth > configuration.MaxDepth) continue;

                    logData.Depths.Add(depth);

                    // Extract curve values
                    for (int i = 1; i < curveNames.Count && i < values.Length; i++)
                    {
                        var curveName = curveNames[i];

                        // Apply curve filter
                        if (configuration.CurvesToLoad != null && !configuration.CurvesToLoad.Contains(curveName))
                            continue;

                        if (!logData.Curves.ContainsKey(curveName))
                        {
                            logData.Curves[curveName] = new List<double>();
                        }

                        if (double.TryParse(values[i], out var value))
                        {
                            logData.Curves[curveName].Add(value);
                        }
                        else
                        {
                            logData.Curves[curveName].Add(configuration.NullValue);
                        }
                    }

                    stats.RecordsLoaded++;
                }
            }

            // Calculate depth step
            if (logData.Depths.Count > 1)
            {
                logData.DepthStep = (logData.EndDepth - logData.StartDepth) / (logData.Depths.Count - 1);
            }
        }

        private void ApplyPwlsMapping(LogData logData, LogLoadConfiguration configuration)
        {
            var remappedCurves = new Dictionary<string, List<double>>();
            var remappedMetadata = new Dictionary<string, LogCurveMetadata>();

            foreach (var kvp in logData.Curves)
            {
                var originalMnemonic = kvp.Key;
                var pwlsName = PwlsMnemonicMapper.MapToPwlsProperty(originalMnemonic);
                var metadata = logData.CurveMetadata.ContainsKey(originalMnemonic)
                    ? logData.CurveMetadata[originalMnemonic]
                    : new LogCurveMetadata { Mnemonic = originalMnemonic };

                metadata.DisplayName = pwlsName;

                // Add with PWLS name
                remappedCurves[pwlsName] = kvp.Value;
                remappedMetadata[pwlsName] = metadata;

                // Keep original if configured
                if (configuration.KeepOriginalMnemonics && pwlsName != originalMnemonic)
                {
                    remappedCurves[originalMnemonic] = kvp.Value;
                    remappedMetadata[originalMnemonic] = metadata;
                }
            }

            logData.Curves = remappedCurves;
            logData.CurveMetadata = remappedMetadata;
        }

        #endregion
    }
}

