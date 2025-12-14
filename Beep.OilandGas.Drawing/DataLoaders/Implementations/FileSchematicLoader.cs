using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for well schematic data from JSON files.
    /// </summary>
    public class FileSchematicLoader : ISchematicLoader
    {
        private readonly string filePath;
        private readonly string directoryPath;
        private bool isConnected = false;

        /// <summary>
        /// Gets the data source (file path or directory).
        /// </summary>
        public string DataSource => filePath ?? directoryPath;

        /// <summary>
        /// Gets whether the loader is connected.
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance for loading from a single file.
        /// </summary>
        /// <param name="filePath">The path to the JSON file.</param>
        public FileSchematicLoader(string filePath)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        /// <summary>
        /// Initializes a new instance for loading from a directory.
        /// </summary>
        /// <param name="directoryPath">The directory path containing JSON files.</param>
        /// <param name="isDirectory">True if loading from directory.</param>
        public FileSchematicLoader(string directoryPath, bool isDirectory)
        {
            if (!isDirectory)
                throw new ArgumentException("Use single-parameter constructor for file loading.");
            
            this.directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        }

        /// <summary>
        /// Connects to the data source.
        /// </summary>
        public bool Connect()
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                isConnected = File.Exists(filePath);
            }
            else if (!string.IsNullOrEmpty(directoryPath))
            {
                isConnected = Directory.Exists(directoryPath);
            }
            else
            {
                isConnected = false;
            }

            return isConnected;
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
        /// Loads well schematic data.
        /// </summary>
        public WellData Load(object criteria = null)
        {
            if (criteria is string wellIdentifier)
            {
                return LoadSchematic(wellIdentifier);
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                return LoadFromFile(filePath);
            }

            throw new ArgumentException("Criteria must be a well identifier string when loading from directory.");
        }

        /// <summary>
        /// Loads well schematic data asynchronously.
        /// </summary>
        public Task<WellData> LoadAsync(object criteria = null)
        {
            return Task.Run(() => Load(criteria));
        }

        /// <summary>
        /// Loads well schematic for a specific well.
        /// </summary>
        public WellData LoadSchematic(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            if (!isConnected)
                Connect();

            if (!string.IsNullOrEmpty(filePath))
            {
                return LoadFromFile(filePath, configuration);
            }
            else if (!string.IsNullOrEmpty(directoryPath))
            {
                // Find file matching well identifier
                var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
                var matchingFile = files.FirstOrDefault(f => 
                    Path.GetFileNameWithoutExtension(f).Equals(wellIdentifier, StringComparison.OrdinalIgnoreCase) ||
                    f.Contains(wellIdentifier));

                if (matchingFile != null)
                {
                    return LoadFromFile(matchingFile, configuration);
                }

                throw new FileNotFoundException($"No file found for well identifier: {wellIdentifier}");
            }

            throw new InvalidOperationException("No file path or directory path specified.");
        }

        /// <summary>
        /// Loads well schematic asynchronously.
        /// </summary>
        public Task<WellData> LoadSchematicAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadSchematic(wellIdentifier, configuration));
        }

        /// <summary>
        /// Loads well schematic data with result object.
        /// </summary>
        public DataLoadResult<WellData> LoadSchematicWithResult(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            var stats = new DataLoadStatistics();
            configuration = configuration ?? new WellSchematicLoadConfiguration();

            try
            {
                var wellData = LoadSchematic(wellIdentifier, configuration);
                
                // Apply configuration filters
                if (wellData.BoreHoles != null)
                {
                    foreach (var borehole in wellData.BoreHoles)
                    {
                        if (!configuration.LoadCasing)
                            borehole.Casing = new List<WellData_Casing>();
                        if (!configuration.LoadTubing)
                            borehole.Tubing = new List<WellData_Tubing>();
                        if (!configuration.LoadEquipment)
                            borehole.Equip = new List<WellData_Equip>();
                        if (!configuration.LoadPerforations)
                            borehole.Perforation = new List<WellData_Perf>();

                        // Apply depth filtering
                        if (configuration.MinDepth > 0 || configuration.MaxDepth > 0)
                        {
                            FilterByDepth(borehole, configuration.MinDepth, configuration.MaxDepth);
                        }
                    }
                }

                stats.RecordsLoaded = wellData.BoreHoles?.Count ?? 0;
                stats.Complete();

                var result = DataLoadResult<WellData>.CreateSuccess(wellData, stats.RecordsLoaded);
                result.LoadDuration = stats.Duration;
                return result;
            }
            catch (Exception ex)
            {
                stats.Complete();
                return DataLoadResult<WellData>.CreateFailure($"Failed to load schematic: {ex.Message}", ex.ToString());
            }
        }

        /// <summary>
        /// Loads well schematic data with result object asynchronously.
        /// </summary>
        public Task<DataLoadResult<WellData>> LoadSchematicWithResultAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadSchematicWithResult(wellIdentifier, configuration));
        }

        /// <summary>
        /// Loads well schematic data with extended information.
        /// </summary>
        public WellSchematicData LoadSchematicData(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            var result = LoadSchematicWithResult(wellIdentifier, configuration);
            
            return new WellSchematicData
            {
                WellData = result.Data,
                DataSource = DataSource,
                LoadedDate = DateTime.Now,
                IsValidated = result.Success && (configuration?.ValidateAfterLoad ?? false),
                ValidationErrors = result.Errors
            };
        }

        /// <summary>
        /// Loads well schematic data with extended information asynchronously.
        /// </summary>
        public Task<WellSchematicData> LoadSchematicDataAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadSchematicData(wellIdentifier, configuration));
        }

        /// <summary>
        /// Loads deviation survey (not supported for file-based loader).
        /// </summary>
        public DeviationSurvey LoadDeviationSurvey(string wellIdentifier, string boreholeIdentifier = null)
        {
            // File-based loader doesn't support deviation surveys
            return null;
        }

        /// <summary>
        /// Loads deviation survey asynchronously.
        /// </summary>
        public Task<DeviationSurvey> LoadDeviationSurveyAsync(string wellIdentifier, string boreholeIdentifier = null)
        {
            return Task.FromResult<DeviationSurvey>(null);
        }

        /// <summary>
        /// Filters borehole data by depth range.
        /// </summary>
        private void FilterByDepth(WellData_Borehole borehole, float minDepth, float maxDepth)
        {
            // Filter casing
            if (borehole.Casing != null)
            {
                borehole.Casing = borehole.Casing.Where(c => 
                    (minDepth == 0 || c.BottomDepth >= minDepth) && 
                    (maxDepth == 0 || c.TopDepth <= maxDepth)).ToList();
            }

            // Filter tubing
            if (borehole.Tubing != null)
            {
                borehole.Tubing = borehole.Tubing.Where(t => 
                    (minDepth == 0 || t.BottomDepth >= minDepth) && 
                    (maxDepth == 0 || t.TopDepth <= maxDepth)).ToList();
            }

            // Filter equipment
            if (borehole.Equip != null)
            {
                borehole.Equip = borehole.Equip.Where(e => 
                    (minDepth == 0 || e.BottomDepth >= minDepth) && 
                    (maxDepth == 0 || e.TopDepth <= maxDepth)).ToList();
            }

            // Filter perforations
            if (borehole.Perforation != null)
            {
                borehole.Perforation = borehole.Perforation.Where(p => 
                    (minDepth == 0 || p.BottomDepth >= minDepth) && 
                    (maxDepth == 0 || p.TopDepth <= maxDepth)).ToList();
            }
        }

        /// <summary>
        /// Loads multiple well schematics.
        /// </summary>
        public Dictionary<string, WellData> LoadSchematics(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null)
        {
            var result = new Dictionary<string, WellData>();
            foreach (var identifier in wellIdentifiers)
            {
                try
                {
                    result[identifier] = LoadSchematic(identifier, configuration);
                }
                catch
                {
                    // Skip failed loads
                }
            }
            return result;
        }

        /// <summary>
        /// Loads multiple well schematics asynchronously.
        /// </summary>
        public async Task<Dictionary<string, WellData>> LoadSchematicsAsync(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null)
        {
            var tasks = wellIdentifiers.Select(async id =>
            {
                try
                {
                    var data = await LoadSchematicAsync(id, configuration);
                    return new { Id = id, Data = data };
                }
                catch
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null).ToDictionary(r => r.Id, r => r.Data);
        }

        /// <summary>
        /// Gets available well identifiers.
        /// </summary>
        public List<string> GetAvailableWells()
        {
            if (!string.IsNullOrEmpty(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);
                return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                return new List<string> { Path.GetFileNameWithoutExtension(filePath) };
            }

            return new List<string>();
        }

        /// <summary>
        /// Gets available well identifiers asynchronously.
        /// </summary>
        public Task<List<string>> GetAvailableWellsAsync()
        {
            return Task.Run(() => GetAvailableWells());
        }

        /// <summary>
        /// Validates the connection.
        /// </summary>
        public bool ValidateConnection()
        {
            if (!string.IsNullOrEmpty(filePath))
                return File.Exists(filePath);
            else if (!string.IsNullOrEmpty(directoryPath))
                return Directory.Exists(directoryPath);
            return false;
        }

        /// <summary>
        /// Gets available identifiers.
        /// </summary>
        public List<string> GetAvailableIdentifiers()
        {
            return GetAvailableWells();
        }

        /// <summary>
        /// Loads well data from a JSON file.
        /// </summary>
        private WellData LoadFromFile(string filePath, WellSchematicLoadConfiguration configuration = null)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            configuration = configuration ?? new WellSchematicLoadConfiguration();

            try
            {
                var json = File.ReadAllText(filePath);
                var wellData = JsonSerializer.Deserialize<WellData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                });

                if (wellData == null)
                    throw new InvalidDataException($"Failed to deserialize well data from: {filePath}");

                // Ensure UWI is set
                if (string.IsNullOrEmpty(wellData.UWI))
                {
                    wellData.UWI = Path.GetFileNameWithoutExtension(filePath);
                }

                // Apply configuration filters
                if (wellData.BoreHoles != null && configuration != null)
                {
                    foreach (var borehole in wellData.BoreHoles)
                    {
                        if (!configuration.LoadCasing && borehole.Casing != null)
                            borehole.Casing.Clear();
                        if (!configuration.LoadTubing && borehole.Tubing != null)
                            borehole.Tubing.Clear();
                        if (!configuration.LoadEquipment && borehole.Equip != null)
                            borehole.Equip.Clear();
                        if (!configuration.LoadPerforations && borehole.Perforation != null)
                            borehole.Perforation.Clear();
                    }
                }

                return wellData;
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException($"Invalid JSON format in file: {filePath}", ex);
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

