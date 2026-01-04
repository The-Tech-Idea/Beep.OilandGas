using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using static Beep.OilandGas.PPDM39.Repositories.IPPDM39DefaultsRepository;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for managing field mapping configurations
    /// Used to configure which table and field to use for retrieving values (e.g., reservoir properties for forecasting)
    /// </summary>
    public class FieldMappingService : IFieldMappingService
    {
        private readonly IDMEEditor _editor;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly string _connectionName;
        
        // In-memory storage for field mappings (can be persisted to database or config file)
        private readonly Dictionary<string, FieldMappingConfig> _fieldMappings = new Dictionary<string, FieldMappingConfig>();
        private readonly object _mappingsLock = new object();

        public FieldMappingService(
            IDMEEditor editor = null,
            IPPDM39DefaultsRepository defaults = null,
            string connectionName = "PPDM39")
        {
            _editor = editor;
            _defaults = defaults;
            _connectionName = connectionName;
        }

        public async Task<FieldMappingConfig?> GetFieldMappingAsync(string mappingKey)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            lock (_mappingsLock)
            {
                // Initialize default mappings if not already loaded
                if (_fieldMappings.Count == 0)
                {
                    InitializeDefaultFieldMappings();
                }

                return _fieldMappings.TryGetValue(mappingKey, out var config) ? config : null;
            }
        }

        public async Task<Dictionary<string, FieldMappingConfig>> GetFieldMappingsByCategoryAsync(string category)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            lock (_mappingsLock)
            {
                // Initialize default mappings if not already loaded
                if (_fieldMappings.Count == 0)
                {
                    InitializeDefaultFieldMappings();
                }

                return _fieldMappings
                    .Where(kvp => kvp.Value.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public async Task SetFieldMappingAsync(string mappingKey, FieldMappingConfig config)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            if (string.IsNullOrEmpty(mappingKey))
                throw new ArgumentException("Mapping key cannot be null or empty", nameof(mappingKey));
            
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            lock (_mappingsLock)
            {
                config.MappingKey = mappingKey;
                _fieldMappings[mappingKey] = config;

                // Optionally persist to database if defaults repository is available
                if (_defaults != null && !string.IsNullOrEmpty(_connectionName))
                {
                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(config);
                        _defaults.SetDefaultValueAsync($"FIELD_MAPPING.{mappingKey}", json, _connectionName, null, "FieldMapping", "JSON", config.Description).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Silently fail - in-memory storage will still work
                    }
                }
            }
        }

        /// <summary>
        /// Initializes default field mappings for reservoir forecasting properties
        /// These can be overridden by calling SetFieldMappingAsync
        /// </summary>
        private void InitializeDefaultFieldMappings()
        {
            // ==========================================================================
            // CUSTOM/EXTENSION FIELD MAPPINGS ONLY
            // ==========================================================================
            // Known PPDM 3.9 fields are accessed directly via PPDMCalculationService
            // helper functions (e.g., GetPoolInitialPressureAsync, GetWellTestSkinAsync).
            // 
            // This configuration is ONLY for:
            // - Custom tables/columns not in standard PPDM 3.9
            // - Extension tables added by your organization
            // - Overriding default field locations
            //
            // Standard PPDM 3.9 fields accessed by direct functions:
            // - POOL: AVG_POROSITY, AVG_PERMEABILITY, AVG_THICKNESS, INITIAL_RESERVOIR_PRESSURE,
            //         RESERVOIR_TEMPERATURE, TOTAL_COMPRESSIBILITY, BUBBLE_POINT_PRESSURE,
            //         OIL_VISCOSITY, GAS_VISCOSITY, FORMATION_VOLUME_FACTOR, GAS_GRAVITY,
            //         DRAINAGE_AREA
            // - WELL_TEST_ANALYSIS: PERMEABILITY, SKIN, PRODUCTIVITY_INDEX, AOF_POTENTIAL,
            //                       WELLBORE_STORAGE_COEFF, FLOW_EFFICIENCY
            // - WELL_TEST_FLOW: FLOW_RATE_OIL, FLOW_RATE_GAS, FLOW_RATE_WATER, CHOKE_SIZE
            // - WELL_TEST_PRESSURE: STATIC_PRESSURE, FLOWING_PRESSURE, BOTTOM_HOLE_PRESSURE
            // ==========================================================================

            // EXAMPLE: Custom wellbore radius field (not standard in PPDM 3.9)
            // Configure this if your database has a custom column for wellbore radius
            _fieldMappings["Custom.WellboreRadius"] = new FieldMappingConfig
            {
                MappingKey = "Custom.WellboreRadius",
                Category = "Custom",
                TableName = "WELLBORE", // Or your custom table name
                FieldName = "WELLBORE_RADIUS", // Or "HOLE_DIAMETER" with ConversionFactor = 0.5
                DefaultValue = 0.25m,
                ConversionFactor = null, // Set to 0.5 if using HOLE_DIAMETER
                IsActive = false, // Disabled by default - enable if you have this custom field
                Description = "Custom wellbore radius field - enable if your database has this column"
            };

            // EXAMPLE: Custom extension table for reservoir properties
            // Use this pattern if you have extension tables
            _fieldMappings["Custom.ReservoirExtension.SpecialProperty"] = new FieldMappingConfig
            {
                MappingKey = "Custom.ReservoirExtension.SpecialProperty",
                Category = "Custom",
                TableName = "X_RESERVOIR_EXTENSION", // Your custom extension table
                FieldName = "SPECIAL_PROPERTY",
                DefaultValue = null,
                IsActive = false, // Disabled by default
                Description = "Example custom extension table field"
            };

            // EXAMPLE: Conditional field mapping (with conditions)
            // Use this if you need to filter by another column value
            _fieldMappings["Custom.WellProperty.ActiveOnly"] = new FieldMappingConfig
            {
                MappingKey = "Custom.WellProperty.ActiveOnly",
                Category = "Custom",
                TableName = "WELL",
                FieldName = "SOME_CUSTOM_PROPERTY",
                Conditions = new Dictionary<string, object>
                {
                    { "ACTIVE_IND", "Y" } // Only get value where ACTIVE_IND = 'Y'
                },
                DefaultValue = null,
                IsActive = false,
                Description = "Example conditional field mapping"
            };

            // EXAMPLE: Unit conversion field
            // Use this when you need to apply a conversion factor
            _fieldMappings["Custom.PressureInKPa"] = new FieldMappingConfig
            {
                MappingKey = "Custom.PressureInKPa",
                Category = "Custom",
                TableName = "POOL",
                FieldName = "INITIAL_PRESSURE_KPA", // Custom field in kPa
                ConversionFactor = 0.145038m, // Convert kPa to psia
                DefaultValue = 3000m, // Default in psia
                IsActive = false,
                Description = "Example: custom pressure field in kPa, converted to psia"
            };
        }
    }
}
