using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.DataLoaders.Models;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for well schematic data from PPDM38 database.
    /// </summary>
    public class Ppdm38SchematicLoader : ISchematicLoader
    {
        private readonly string connectionString;
        private DbConnection connection;
        private bool isConnected = false;

        /// <summary>
        /// Gets the data source (connection string).
        /// </summary>
        public string DataSource => connectionString;

        /// <summary>
        /// Gets whether the loader is connected.
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ppdm38SchematicLoader"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="connectionFactory">Factory for creating database connections.</param>
        public Ppdm38SchematicLoader(string connectionString, Func<DbConnection> connectionFactory = null)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.connectionFactory = connectionFactory;
        }

        private readonly Func<DbConnection> connectionFactory;

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
                    // Default to SQL Server (can be extended for other providers)
                    // connection = new SqlConnection(connectionString);
                    throw new NotImplementedException("Connection factory must be provided or implement default connection creation.");
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
        /// Loads well schematic data.
        /// </summary>
        public WellData Load(object criteria = null)
        {
            if (criteria is string wellIdentifier)
            {
                return LoadSchematic(wellIdentifier);
            }

            throw new ArgumentException("Criteria must be a well identifier string.");
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
            var result = LoadSchematicWithResult(wellIdentifier, configuration);
            return result.Data;
        }

        /// <summary>
        /// Loads well schematic asynchronously.
        /// </summary>
        public Task<WellData> LoadSchematicAsync(string wellIdentifier, WellSchematicLoadConfiguration configuration = null)
        {
            return Task.Run(async () =>
            {
                var result = await LoadSchematicWithResultAsync(wellIdentifier, configuration);
                return result.Data;
            });
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
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return DataLoadResult<WellData>.CreateFailure("Database connection is not open.");

                var wellData = new WellData
                {
                    UWI = wellIdentifier
                };

                // Load boreholes
                wellData.BoreHoles = LoadBoreholes(wellIdentifier, configuration);

                // Load casing, tubing, equipment, perforations for each borehole based on configuration
                foreach (var borehole in wellData.BoreHoles)
                {
                    if (configuration.LoadCasing)
                        borehole.Casing = LoadCasing(borehole.BoreHoleIndex, wellIdentifier, configuration);
                    else
                        borehole.Casing = new List<WellData_Casing>();

                    if (configuration.LoadTubing)
                        borehole.Tubing = LoadTubing(borehole.BoreHoleIndex, wellIdentifier, configuration);
                    else
                        borehole.Tubing = new List<WellData_Tubing>();

                    if (configuration.LoadEquipment)
                        borehole.Equip = LoadEquipment(borehole.BoreHoleIndex, wellIdentifier, configuration);
                    else
                        borehole.Equip = new List<WellData_Equip>();

                    if (configuration.LoadPerforations)
                        borehole.Perforation = LoadPerforations(borehole.BoreHoleIndex, wellIdentifier, configuration);
                    else
                        borehole.Perforation = new List<WellData_Perf>();
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
        /// Loads deviation survey (placeholder - implement based on PPDM38 schema).
        /// </summary>
        public DeviationSurvey LoadDeviationSurvey(string wellIdentifier, string boreholeIdentifier = null)
        {
            // TODO: Implement PPDM38 query for deviation survey
            return null;
        }

        /// <summary>
        /// Loads deviation survey asynchronously.
        /// </summary>
        public Task<DeviationSurvey> LoadDeviationSurveyAsync(string wellIdentifier, string boreholeIdentifier = null)
        {
            return Task.Run(() => LoadDeviationSurvey(wellIdentifier, boreholeIdentifier));
        }

        /// <summary>
        /// Loads multiple well schematics.
        /// </summary>
        public Dictionary<string, WellData> LoadSchematics(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null)
        {
            var result = new Dictionary<string, WellData>();
            foreach (var identifier in wellIdentifiers)
            {
                result[identifier] = LoadSchematic(identifier, configuration);
            }
            return result;
        }

        /// <summary>
        /// Loads multiple well schematics asynchronously.
        /// </summary>
        public async Task<Dictionary<string, WellData>> LoadSchematicsAsync(List<string> wellIdentifiers, WellSchematicLoadConfiguration configuration = null)
        {
            var tasks = wellIdentifiers.Select(async id => new { Id = id, Data = await LoadSchematicAsync(id, configuration) });
            var results = await Task.WhenAll(tasks);
            return results.ToDictionary(r => r.Id, r => r.Data);
        }

        /// <summary>
        /// Gets available well identifiers.
        /// </summary>
        public List<string> GetAvailableWells()
        {
            if (!isConnected)
                Connect();

            if (connection == null || connection.State != ConnectionState.Open)
                throw new InvalidOperationException("Database connection is not open.");

            var wells = new List<string>();
            // TODO: Implement query to get well list from PPDM38
            // Example: SELECT DISTINCT UWI FROM WELL
            return wells;
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
            try
            {
                if (connection == null || connection.State != ConnectionState.Open)
                    return false;

                // Simple validation query
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
            return GetAvailableWells();
        }

        /// <summary>
        /// Loads boreholes for a well.
        /// </summary>
        private List<WellData_Borehole> LoadBoreholes(string wellIdentifier, WellSchematicLoadConfiguration configuration)
        {
            var boreholes = new List<WellData_Borehole>();
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    // PPDM38 query structure - adjust table/column names as needed
                    command.CommandText = @"
                        SELECT 
                            BOREHOLE_ID,
                            UWI,
                            UBHI,
                            TOP_DEPTH,
                            BOTTOM_DEPTH,
                            DIAMETER,
                            BOREHOLE_INDEX,
                            IS_VERTICAL
                        FROM BOREHOLE 
                        WHERE UWI = @wellIdentifier
                        ORDER BY BOREHOLE_INDEX";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var borehole = new WellData_Borehole
                            {
                                ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                UWI = reader.IsDBNull(1) ? wellIdentifier : reader.GetString(1),
                                UBHI = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                TopDepth = reader.IsDBNull(3) ? 0 : (float)reader.GetDouble(3),
                                BottomDepth = reader.IsDBNull(4) ? 0 : (float)reader.GetDouble(4),
                                Diameter = reader.IsDBNull(5) ? 0 : (float)reader.GetDouble(5),
                                BoreHoleIndex = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                IsVertical = reader.IsDBNull(7) ? true : reader.GetBoolean(7)
                            };

                            // Apply depth filtering
                            if (configuration.MinDepth > 0 && borehole.BottomDepth < configuration.MinDepth)
                                continue;
                            if (configuration.MaxDepth > 0 && borehole.TopDepth > configuration.MaxDepth)
                                continue;

                            boreholes.Add(borehole);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle gracefully
                // For now, return empty list if query fails
            }

            return boreholes;
        }

        /// <summary>
        /// Loads casing for a borehole.
        /// </summary>
        private List<WellData_Casing> LoadCasing(int boreholeIndex, string wellIdentifier, WellSchematicLoadConfiguration configuration)
        {
            var casing = new List<WellData_Casing>();
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            CASING_ID,
                            UWI,
                            UBHI,
                            BOREHOLE_INDEX,
                            TOP_DEPTH,
                            BOTTOM_DEPTH,
                            OUTER_DIAMETER,
                            INNER_DIAMETER,
                            CASING_TYPE
                        FROM CASING 
                        WHERE UWI = @wellIdentifier AND BOREHOLE_INDEX = @boreholeIndex
                        ORDER BY TOP_DEPTH";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@boreholeIndex";
                    param.Value = boreholeIndex;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var casingItem = new WellData_Casing
                            {
                                ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                UWI = reader.IsDBNull(1) ? wellIdentifier : reader.GetString(1),
                                UBHI = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                BoreHoleIndex = boreholeIndex,
                                TopDepth = reader.IsDBNull(4) ? 0 : (float)reader.GetDouble(4),
                                BottomDepth = reader.IsDBNull(5) ? 0 : (float)reader.GetDouble(5),
                                OUTER_DIAMETER = reader.IsDBNull(6) ? 0 : (float)reader.GetDouble(6),
                                INNER_DIAMETER = reader.IsDBNull(7) ? 0 : (float)reader.GetDouble(7),
                                CasingType = reader.IsDBNull(8) ? "" : reader.GetString(8)
                            };

                            casingItem.Diameter = casingItem.OUTER_DIAMETER;
                            casingItem.Depth = (casingItem.TopDepth + casingItem.BottomDepth) / 2;

                            // Apply depth filtering
                            if (configuration.MinDepth > 0 && casingItem.BottomDepth < configuration.MinDepth)
                                continue;
                            if (configuration.MaxDepth > 0 && casingItem.TopDepth > configuration.MaxDepth)
                                continue;

                            casing.Add(casingItem);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle gracefully
            }

            return casing;
        }

        /// <summary>
        /// Loads tubing for a borehole.
        /// </summary>
        private List<WellData_Tubing> LoadTubing(int boreholeIndex, string wellIdentifier, WellSchematicLoadConfiguration configuration)
        {
            var tubing = new List<WellData_Tubing>();
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            TUBING_ID,
                            UWI,
                            UBHI,
                            BOREHOLE_INDEX,
                            TOP_DEPTH,
                            BOTTOM_DEPTH,
                            DIAMETER,
                            TUBE_INDEX,
                            TUBE_TYPE
                        FROM TUBING 
                        WHERE UWI = @wellIdentifier AND BOREHOLE_INDEX = @boreholeIndex
                        ORDER BY TUBE_INDEX, TOP_DEPTH";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@boreholeIndex";
                    param.Value = boreholeIndex;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tube = new WellData_Tubing
                            {
                                ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                UWI = reader.IsDBNull(1) ? wellIdentifier : reader.GetString(1),
                                UBHI = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                BoreHoleIndex = boreholeIndex,
                                TopDepth = reader.IsDBNull(4) ? 0 : (float)reader.GetDouble(4),
                                BottomDepth = reader.IsDBNull(5) ? 0 : (float)reader.GetDouble(5),
                                Diameter = reader.IsDBNull(6) ? 0 : (float)reader.GetDouble(6),
                                TubeIndex = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                TubeType = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                            };

                            // Apply depth filtering
                            if (configuration.MinDepth > 0 && tube.BottomDepth < configuration.MinDepth)
                                continue;
                            if (configuration.MaxDepth > 0 && tube.TopDepth > configuration.MaxDepth)
                                continue;

                            tubing.Add(tube);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle gracefully
            }

            return tubing;
        }

        /// <summary>
        /// Loads equipment for a borehole.
        /// </summary>
        private List<WellData_Equip> LoadEquipment(int boreholeIndex, string wellIdentifier, WellSchematicLoadConfiguration configuration)
        {
            var equipment = new List<WellData_Equip>();
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            EQUIPMENT_ID,
                            UWI,
                            UBHI,
                            BOREHOLE_INDEX,
                            TOP_DEPTH,
                            BOTTOM_DEPTH,
                            DIAMETER,
                            TUBE_INDEX,
                            EQUIPMENT_TYPE,
                            EQUIPMENT_NAME,
                            EQUIPMENT_SVG,
                            TOOLTIP_TEXT,
                            EQUIPMENT_DESCRIPTION,
                            EQUIPMENT_STATUS
                        FROM EQUIPMENT 
                        WHERE UWI = @wellIdentifier AND BOREHOLE_INDEX = @boreholeIndex
                        ORDER BY TOP_DEPTH";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@boreholeIndex";
                    param.Value = boreholeIndex;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var equip = new WellData_Equip
                            {
                                ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                UWI = reader.IsDBNull(1) ? wellIdentifier : reader.GetString(1),
                                UBHI = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                BoreHoleIndex = boreholeIndex,
                                TopDepth = reader.IsDBNull(4) ? 0 : (float)reader.GetDouble(4),
                                BottomDepth = reader.IsDBNull(5) ? 0 : (float)reader.GetDouble(5),
                                Diameter = reader.IsDBNull(6) ? 0 : (float)reader.GetDouble(6),
                                TubeIndex = reader.IsDBNull(7) ? -1 : reader.GetInt32(7),
                                EquipmentType = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                EquipmentName = reader.IsDBNull(9) ? "" : reader.GetString(9),
                                EquipmentSvg = reader.IsDBNull(10) ? "" : reader.GetString(10),
                                ToolTipText = reader.IsDBNull(11) ? "" : reader.GetString(11),
                                EquipmentDescription = reader.IsDBNull(12) ? "" : reader.GetString(12),
                                EquipmentStatus = reader.IsDBNull(13) ? "" : reader.GetString(13)
                            };

                            // Apply depth filtering
                            if (configuration.MinDepth > 0 && equip.BottomDepth < configuration.MinDepth)
                                continue;
                            if (configuration.MaxDepth > 0 && equip.TopDepth > configuration.MaxDepth)
                                continue;

                            equipment.Add(equip);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle gracefully
            }

            return equipment;
        }

        /// <summary>
        /// Loads perforations for a borehole.
        /// </summary>
        private List<WellData_Perf> LoadPerforations(int boreholeIndex, string wellIdentifier, WellSchematicLoadConfiguration configuration)
        {
            var perforations = new List<WellData_Perf>();
            
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            PERFORATION_ID,
                            UWI,
                            UBHI,
                            BOREHOLE_INDEX,
                            TOP_DEPTH,
                            BOTTOM_DEPTH,
                            COMPLETION_CODE,
                            SHOTS_PER_UOM,
                            PERF_TYPE,
                            SHOT_DENSITY,
                            SHOT_SIZE,
                            SHOT_DEPTH,
                            SHOT_THICKNESS,
                            SHOT_LENGTH,
                            SHOT_WIDTH
                        FROM PERFORATION 
                        WHERE UWI = @wellIdentifier AND BOREHOLE_INDEX = @boreholeIndex
                        ORDER BY TOP_DEPTH";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
                    command.Parameters.Add(param);

                    param = command.CreateParameter();
                    param.ParameterName = "@boreholeIndex";
                    param.Value = boreholeIndex;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var perf = new WellData_Perf
                            {
                                ID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                UWI = reader.IsDBNull(1) ? wellIdentifier : reader.GetString(1),
                                UBHI = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                BoreHoleIndex = boreholeIndex,
                                TopDepth = reader.IsDBNull(4) ? 0 : (float)reader.GetDouble(4),
                                BottomDepth = reader.IsDBNull(5) ? 0 : (float)reader.GetDouble(5),
                                CompletionCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                ShotsPerUOM = reader.IsDBNull(7) ? 0 : (float)reader.GetDouble(7),
                                PerfType = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                ShotDensity = reader.IsDBNull(9) ? 0 : (float)reader.GetDouble(9),
                                ShotSize = reader.IsDBNull(10) ? 0 : (float)reader.GetDouble(10),
                                ShotDepth = reader.IsDBNull(11) ? 0 : (float)reader.GetDouble(11),
                                ShotThickness = reader.IsDBNull(12) ? 0 : (float)reader.GetDouble(12),
                                ShotLength = reader.IsDBNull(13) ? 0 : (float)reader.GetDouble(13),
                                ShotWidth = reader.IsDBNull(14) ? 0 : (float)reader.GetDouble(14)
                            };

                            // Apply depth filtering
                            if (configuration.MinDepth > 0 && perf.BottomDepth < configuration.MinDepth)
                                continue;
                            if (configuration.MaxDepth > 0 && perf.TopDepth > configuration.MaxDepth)
                                continue;

                            perforations.Add(perf);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Log error or handle gracefully
            }

            return perforations;
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

