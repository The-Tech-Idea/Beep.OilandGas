using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.DataLoaders.Implementations
{
    /// <summary>
    /// Loader for layer/lithology data from PPDM38 database.
    /// </summary>
    public class Ppdm38LayerLoader : ILayerLoader
    {
        private readonly string connectionString;
        private readonly Func<DbConnection> connectionFactory;
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
        /// Initializes a new instance of the <see cref="Ppdm38LayerLoader"/> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="connectionFactory">Factory function to create database connections.</param>
        public Ppdm38LayerLoader(string connectionString, Func<DbConnection> connectionFactory)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// Connects to the database.
        /// </summary>
        public bool Connect()
        {
            try
            {
                if (connection == null)
                {
                    connection = connectionFactory();
                    connection.ConnectionString = connectionString;
                }

                if (connection.State != ConnectionState.Open)
                {
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
        public Task<bool> ConnectAsync()
        {
            return Task.Run(() => Connect());
        }

        /// <summary>
        /// Disconnects from the database.
        /// </summary>
        public void Disconnect()
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
            isConnected = false;
        }

        /// <summary>
        /// Loads layer data.
        /// </summary>
        public List<LayerData> Load(object criteria = null)
        {
            if (criteria is string wellIdentifier)
            {
                return LoadLayers(wellIdentifier);
            }
            throw new ArgumentException("Criteria must be a well identifier string.");
        }

        /// <summary>
        /// Loads layer data asynchronously.
        /// </summary>
        public Task<List<LayerData>> LoadAsync(object criteria = null)
        {
            return Task.Run(() => Load(criteria));
        }

        /// <summary>
        /// Loads layers for a specific well.
        /// </summary>
        public List<LayerData> LoadLayers(string wellIdentifier, LayerLoadConfiguration configuration = null)
        {
            var result = LoadLayersWithResult(wellIdentifier, configuration);
            return result.Data;
        }

        /// <summary>
        /// Loads layers asynchronously.
        /// </summary>
        public Task<List<LayerData>> LoadLayersAsync(string wellIdentifier, LayerLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadLayers(wellIdentifier, configuration));
        }

        /// <summary>
        /// Loads layers with result object.
        /// </summary>
        public DataLoadResult<List<LayerData>> LoadLayersWithResult(string wellIdentifier, LayerLoadConfiguration configuration = null)
        {
            var stats = new DataLoadStatistics();
            configuration = configuration ?? new LayerLoadConfiguration();

            try
            {
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return DataLoadResult<List<LayerData>>.CreateFailure("Database connection is not open.");

                var layers = new List<LayerData>();

                // Load lithology data from PPDM38
                using (var command = connection.CreateCommand())
                {
                    // PPDM38 query structure - adjust table/column names as needed
                    command.CommandText = @"
                        SELECT 
                            L.LITHOLOGY_LOG_ID,
                            L.UWI,
                            L.DEPTH_OBS_NO,
                            L.ROCK_TYPE,
                            L.ROCK_TYPE_OBS_NO,
                            L.TOP_DEPTH,
                            L.BOTTOM_DEPTH,
                            L.LITHOLOGY,
                            L.FACIES,
                            L.POROSITY,
                            L.PERMEABILITY,
                            L.WATER_SATURATION,
                            L.OIL_SATURATION,
                            L.GAS_SATURATION,
                            L.NET_TO_GROSS,
                            L.IS_PAY_ZONE,
                            LC.COLOR,
                            LC.PATTERN_TYPE
                        FROM LITHOLOGY_LOG L
                        LEFT JOIN LITH_ROCK_COLOR LC ON L.ROCK_TYPE = LC.ROCK_TYPE
                        WHERE L.UWI = @wellIdentifier";

                    if (configuration.MinDepth > 0)
                        command.CommandText += " AND L.BOTTOM_DEPTH >= @minDepth";
                    if (configuration.MaxDepth > 0)
                        command.CommandText += " AND L.TOP_DEPTH <= @maxDepth";
                    if (configuration.PayZonesOnly)
                        command.CommandText += " AND L.IS_PAY_ZONE = 1";
                    if (configuration.LithologyTypes != null && configuration.LithologyTypes.Count > 0)
                    {
                        var lithParams = string.Join(",", configuration.LithologyTypes.Select((_, i) => $"@lith{i}"));
                        command.CommandText += $" AND L.LITHOLOGY IN ({lithParams})";
                    }
                    if (configuration.FaciesTypes != null && configuration.FaciesTypes.Count > 0)
                    {
                        var faciesParams = string.Join(",", configuration.FaciesTypes.Select((_, i) => $"@facies{i}"));
                        command.CommandText += $" AND L.FACIES IN ({faciesParams})";
                    }

                    command.CommandText += " ORDER BY L.TOP_DEPTH";

                    var param = command.CreateParameter();
                    param.ParameterName = "@wellIdentifier";
                    param.Value = wellIdentifier;
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

                    // Add lithology type parameters
                    if (configuration.LithologyTypes != null && configuration.LithologyTypes.Count > 0)
                    {
                        for (int i = 0; i < configuration.LithologyTypes.Count; i++)
                        {
                            param = command.CreateParameter();
                            param.ParameterName = $"@lith{i}";
                            param.Value = configuration.LithologyTypes[i];
                            command.Parameters.Add(param);
                        }
                    }

                    // Add facies type parameters
                    if (configuration.FaciesTypes != null && configuration.FaciesTypes.Count > 0)
                    {
                        for (int i = 0; i < configuration.FaciesTypes.Count; i++)
                        {
                            param = command.CreateParameter();
                            param.ParameterName = $"@facies{i}";
                            param.Value = configuration.FaciesTypes[i];
                            command.Parameters.Add(param);
                        }
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var layer = new LayerData
                            {
                                LayerId = reader.IsDBNull(0) ? Guid.NewGuid().ToString() : reader.GetString(0),
                                TopDepth = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                                BottomDepth = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                                Lithology = reader.IsDBNull(7) ? "" : reader.GetString(7),
                                Facies = reader.IsDBNull(8) ? "" : reader.GetString(8),
                                Porosity = reader.IsDBNull(9) ? null : (double?)reader.GetDouble(9),
                                Permeability = reader.IsDBNull(10) ? null : (double?)reader.GetDouble(10),
                                WaterSaturation = reader.IsDBNull(11) ? null : (double?)reader.GetDouble(11),
                                OilSaturation = reader.IsDBNull(12) ? null : (double?)reader.GetDouble(12),
                                GasSaturation = reader.IsDBNull(13) ? null : (double?)reader.GetDouble(13),
                                NetToGross = reader.IsDBNull(14) ? null : (double?)reader.GetDouble(14),
                                IsPayZone = reader.IsDBNull(15) ? false : reader.GetBoolean(15)
                            };

                            // Apply color and pattern from database or use defaults
                            if (configuration.IncludeColorCodes)
                            {
                                layer.ColorCode = reader.IsDBNull(16) ? null : reader.GetString(16);
                                if (string.IsNullOrEmpty(layer.ColorCode))
                                {
                                    // Use lithology color palette
                                    var color = LithologyColorPalette.GetLithologyColor(layer.Lithology);
                                    layer.ColorCode = $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
                                }
                            }

                            if (configuration.IncludePatternTypes)
                            {
                                layer.PatternType = reader.IsDBNull(17) ? null : reader.GetString(17);
                                if (string.IsNullOrEmpty(layer.PatternType))
                                {
                                    // Use lithology pattern
                                    var pattern = LithologyColorPalette.GetLithologyPattern(layer.Lithology);
                                    layer.PatternType = pattern.ToString();
                                }
                            }

                            layers.Add(layer);
                        }
                    }
                }

                stats.RecordsLoaded = layers.Count;
                stats.Complete();

                var result = DataLoadResult<List<LayerData>>.CreateSuccess(layers, layers.Count);
                result.LoadDuration = stats.Duration;
                return result;
            }
            catch (Exception ex)
            {
                stats.Complete();
                return DataLoadResult<List<LayerData>>.CreateFailure($"Failed to load layers: {ex.Message}", ex.ToString());
            }
        }

        /// <summary>
        /// Loads layers with result object asynchronously.
        /// </summary>
        public Task<DataLoadResult<List<LayerData>>> LoadLayersWithResultAsync(string wellIdentifier, LayerLoadConfiguration configuration = null)
        {
            return Task.Run(() => LoadLayersWithResult(wellIdentifier, configuration));
        }

        /// <summary>
        /// Gets available lithology types.
        /// </summary>
        public List<string> GetAvailableLithologies()
        {
            var lithologies = new List<string>();

            try
            {
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return lithologies;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT DISTINCT LITHOLOGY FROM LITHOLOGY_LOG WHERE LITHOLOGY IS NOT NULL ORDER BY LITHOLOGY";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                lithologies.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return lithologies;
        }

        /// <summary>
        /// Gets available facies types.
        /// </summary>
        public List<string> GetAvailableFacies()
        {
            var facies = new List<string>();

            try
            {
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return facies;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT DISTINCT FACIES FROM LITHOLOGY_LOG WHERE FACIES IS NOT NULL ORDER BY FACIES";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                facies.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return facies;
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
        /// Gets available identifiers (well UWIs).
        /// </summary>
        public List<string> GetAvailableIdentifiers()
        {
            var identifiers = new List<string>();

            try
            {
                if (!isConnected)
                    Connect();

                if (connection == null || connection.State != ConnectionState.Open)
                    return identifiers;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT DISTINCT UWI FROM LITHOLOGY_LOG WHERE UWI IS NOT NULL ORDER BY UWI";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                                identifiers.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return identifiers;
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

