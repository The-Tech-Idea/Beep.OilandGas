using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.DataLoaders.PWLS;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for DLIS (Digital Log Interchange Standard) / RP66 binary log files.
    /// DLIS is an API Recommended Practice 66 standard for binary well log data.
    /// </summary>
    public class DlisLogLoader : ILogLoader
    {
        private readonly string filePath;
        private bool isConnected = false;
        private FileStream fileStream;
        private BinaryReader reader;
        private DlisFileStructure fileStructure;

        public string DataSource => filePath;
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DlisLogLoader"/> class.
        /// </summary>
        /// <param name="filePath">The path to the DLIS file.</param>
        public DlisLogLoader(string filePath)
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
                    throw new FileNotFoundException($"DLIS file not found: {filePath}");
                }

                fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                reader = new BinaryReader(fileStream, Encoding.UTF8);
                isConnected = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to DLIS file: {ex.Message}");
                isConnected = false;
                return false;
            }
        }

        public async Task<bool> ConnectAsync()
        {
            return await Task.Run(() => Connect());
        }

        public void Disconnect()
        {
            reader?.Dispose();
            fileStream?.Dispose();
            reader = null;
            fileStream = null;
            isConnected = false;
        }

        public void Dispose()
        {
            Disconnect();
        }

        public bool ValidateConnection()
        {
            if (!isConnected || reader == null) return false;

            try
            {
                // Check DLIS file signature (starts with Storage Unit Label)
                long originalPosition = fileStream.Position;
                fileStream.Position = 0;

                // DLIS files start with Storage Unit Label (80 bytes)
                byte[] label = reader.ReadBytes(80);
                fileStream.Position = originalPosition;

                // Check for DLIS signature (first byte should be 0x01 for Storage Unit Label)
                return label.Length == 80 && label[0] == 0x01;
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

            try
            {
                ParseFileStructure();
                return fileStructure?.LogicalFiles?.Select(lf => lf.Name).ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
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
                    result.Errors.Add("Failed to connect to DLIS file.");
                    return result;
                }

                // Parse DLIS file structure
                ParseFileStructure();

                if (fileStructure == null || fileStructure.LogicalFiles.Count == 0)
                {
                    result.Success = false;
                    result.Errors.Add("No logical files found in DLIS file.");
                    return result;
                }

                // Find logical file (use first one if logName not specified)
                var logicalFile = string.IsNullOrEmpty(logName)
                    ? fileStructure.LogicalFiles[0]
                    : fileStructure.LogicalFiles.FirstOrDefault(lf => lf.Name == logName);

                if (logicalFile == null)
                {
                    result.Success = false;
                    result.Errors.Add($"Logical file not found: {logName ?? "default"}");
                    return result;
                }

                // Extract log data from logical file
                ExtractLogData(logicalFile, result.Data, configuration, stats);

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
                result.Errors.Add($"Error loading DLIS log: {ex.Message}");
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

            ParseFileStructure();

            var filesToLoad = logNames != null && logNames.Any()
                ? fileStructure.LogicalFiles.Where(lf => logNames.Contains(lf.Name)).ToList()
                : fileStructure.LogicalFiles;

            foreach (var logicalFile in filesToLoad)
            {
                var logData = LoadLog(wellIdentifier, logicalFile.Name, configuration);
                result[logicalFile.Name] = logData;
            }

            return result;
        }

        public async Task<Dictionary<string, LogData>> LoadLogsAsync(string wellIdentifier, List<string> logNames = null, LogLoadConfiguration configuration = null)
        {
            return await Task.Run(() => LoadLogs(wellIdentifier, logNames, configuration));
        }

        public List<string> GetAvailableLogs(string wellIdentifier)
        {
            return GetAvailableIdentifiers();
        }

        public async Task<List<string>> GetAvailableLogsAsync(string wellIdentifier)
        {
            return await GetAvailableIdentifiersAsync();
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

        #region DLIS Parsing

        private void ParseFileStructure()
        {
            if (fileStructure != null) return;

            fileStructure = new DlisFileStructure();
            fileStream.Position = 0;

            try
            {
                // Read Storage Unit Label (80 bytes)
                var storageUnitLabel = ReadStorageUnitLabel();

                // Read Logical Records
                while (fileStream.Position < fileStream.Length)
                {
                    var record = ReadLogicalRecord();
                    if (record == null) break;

                    ProcessLogicalRecord(record);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing DLIS file structure: {ex.Message}");
            }
        }

        private DlisStorageUnitLabel ReadStorageUnitLabel()
        {
            var label = new DlisStorageUnitLabel
            {
                SequenceNumber = reader.ReadByte(),
                Structure = ReadBytes(4),
                Version = ReadBytes(2),
                StorageUnitSequence = ReadBytes(60),
                StorageSetIdentifier = ReadBytes(12)
            };

            return label;
        }

        private DlisLogicalRecord ReadLogicalRecord()
        {
            if (fileStream.Position >= fileStream.Length - 4)
                return null;

            // Read Logical Record Header
            var header = new DlisLogicalRecordHeader
            {
                Length = ReadUInt16(),
                Attributes = reader.ReadByte(),
                Type = reader.ReadByte()
            };

            if (header.Length < 4 || fileStream.Position + header.Length - 4 > fileStream.Length)
                return null;

            // Read record data
            var data = reader.ReadBytes(header.Length - 4);

            return new DlisLogicalRecord
            {
                Header = header,
                Data = data
            };
        }

        private void ProcessLogicalRecord(DlisLogicalRecord record)
        {
            // Process different record types
            switch (record.Header.Type)
            {
                case 1: // File Header Logical Record
                    ProcessFileHeaderRecord(record);
                    break;
                case 2: // Origin Logical Record
                    ProcessOriginRecord(record);
                    break;
                case 3: // Channel Logical Record
                    ProcessChannelRecord(record);
                    break;
                case 4: // Frame Logical Record
                    ProcessFrameRecord(record);
                    break;
                case 5: // Static Logical Record
                    ProcessStaticRecord(record);
                    break;
            }
        }

        private void ProcessFileHeaderRecord(DlisLogicalRecord record)
        {
            // Extract file header information
            // DLIS file headers contain metadata about the file
        }

        private void ProcessOriginRecord(DlisLogicalRecord record)
        {
            // Extract origin information (well, company, etc.)
            // This contains well identification
        }

        private void ProcessChannelRecord(DlisLogicalRecord record)
        {
            // Extract channel definitions
            // Channels define the log curves
        }

        private void ProcessFrameRecord(DlisLogicalRecord record)
        {
            // Extract frame data (actual log measurements)
            // Frames contain the data values
        }

        private void ProcessStaticRecord(DlisLogicalRecord record)
        {
            // Extract static data (constant values)
        }

        private void ExtractLogData(DlisLogicalFile logicalFile, LogData logData, LogLoadConfiguration configuration, DataLoadStatistics stats)
        {
            // Extract well identifier from origin records
            logData.WellIdentifier = logicalFile.WellIdentifier ?? "UNKNOWN";
            logData.LogName = logicalFile.Name;
            logData.LogType = "DLIS";

            // Extract channels and frames
            foreach (var channel in logicalFile.Channels)
            {
                var curveName = channel.Name;
                var curveValues = new List<double>();

                // Extract values from frames
                foreach (var frame in logicalFile.Frames)
                {
                    if (frame.ChannelValues.ContainsKey(channel.Index))
                    {
                        var value = frame.ChannelValues[channel.Index];
                        curveValues.Add(value);
                    }
                }

                // Apply depth filter
                if (configuration.MinDepth > 0 || configuration.MaxDepth > 0)
                {
                    FilterByDepth(logData, curveValues, configuration);
                }

                // Apply curve filter
                if (configuration.CurvesToLoad != null && !configuration.CurvesToLoad.Contains(curveName))
                    continue;

                logData.Curves[curveName] = curveValues;
                logData.CurveMetadata[curveName] = new LogCurveMetadata
                {
                    Mnemonic = curveName,
                    DisplayName = curveName,
                    Unit = channel.Unit ?? "",
                    Description = channel.Description ?? curveName,
                    MinValue = curveValues.Count > 0 ? curveValues.Min() : null,
                    MaxValue = curveValues.Count > 0 ? curveValues.Max() : null
                };

                stats.RecordsLoaded += curveValues.Count;
            }

            // Extract depths from frames
            if (logicalFile.DepthChannel != null)
            {
                foreach (var frame in logicalFile.Frames)
                {
                    if (frame.ChannelValues.ContainsKey(logicalFile.DepthChannel.Index))
                    {
                        logData.Depths.Add(frame.ChannelValues[logicalFile.DepthChannel.Index]);
                    }
                }

                if (logData.Depths.Count > 0)
                {
                    logData.StartDepth = logData.Depths.Min();
                    logData.EndDepth = logData.Depths.Max();
                    logData.DepthStep = (logData.EndDepth - logData.StartDepth) / Math.Max(1, logData.Depths.Count - 1);
                }
            }
        }

        private void FilterByDepth(LogData logData, List<double> curveValues, LogLoadConfiguration configuration)
        {
            if (logData.Depths == null || logData.Depths.Count == 0)
                return;

            var filteredDepths = new List<double>();
            var filteredIndices = new List<int>();

            for (int i = 0; i < logData.Depths.Count && i < curveValues.Count; i++)
            {
                double depth = logData.Depths[i];
                if ((configuration.MinDepth == 0 || depth >= configuration.MinDepth) &&
                    (configuration.MaxDepth == 0 || depth <= configuration.MaxDepth))
                {
                    filteredDepths.Add(depth);
                    filteredIndices.Add(i);
                }
            }

            logData.Depths = filteredDepths;

            // Filter curve values
            var filteredValues = filteredIndices.Select(idx => curveValues[idx]).ToList();
            curveValues.Clear();
            curveValues.AddRange(filteredValues);
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

        private ushort ReadUInt16()
        {
            var bytes = reader.ReadBytes(2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private uint ReadUInt32()
        {
            var bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private byte[] ReadBytes(int count)
        {
            return reader.ReadBytes(count);
        }

        #endregion
    }

    #region DLIS Data Structures

    internal class DlisFileStructure
    {
        public DlisStorageUnitLabel StorageUnitLabel { get; set; }
        public List<DlisLogicalFile> LogicalFiles { get; set; } = new List<DlisLogicalFile>();
    }

    internal class DlisStorageUnitLabel
    {
        public byte SequenceNumber { get; set; }
        public byte[] Structure { get; set; }
        public byte[] Version { get; set; }
        public byte[] StorageUnitSequence { get; set; }
        public byte[] StorageSetIdentifier { get; set; }
    }

    internal class DlisLogicalRecordHeader
    {
        public ushort Length { get; set; }
        public byte Attributes { get; set; }
        public byte Type { get; set; }
    }

    internal class DlisLogicalRecord
    {
        public DlisLogicalRecordHeader Header { get; set; }
        public byte[] Data { get; set; }
    }

    internal class DlisLogicalFile
    {
        public string Name { get; set; }
        public string WellIdentifier { get; set; }
        public List<DlisChannel> Channels { get; set; } = new List<DlisChannel>();
        public List<DlisFrame> Frames { get; set; } = new List<DlisFrame>();
        public DlisChannel DepthChannel { get; set; }
    }

    internal class DlisChannel
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
    }

    internal class DlisFrame
    {
        public Dictionary<int, double> ChannelValues { get; set; } = new Dictionary<int, double>();
        public double? Depth { get; set; }
    }

    #endregion
}

