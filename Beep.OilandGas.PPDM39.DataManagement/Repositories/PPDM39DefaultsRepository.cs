using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.PPDM39.DataManagement.Repositories
{
    /// <summary>
    /// Implementation of PPDM39 defaults repository
    /// Provides system-wide default values for PPDM39 entities
    /// Includes Well Structure definitions based on PPDM 3.9 Well Structure Model
    /// Includes Well Status Facet retrieval for decomposing complex status values
    /// </summary>
    public class PPDM39DefaultsRepository : IPPDM39DefaultsRepository
    {
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private IUnitOfWorkWrapper _wellStatusXrefUnitOfWork;
        private readonly object _unitOfWorkLock = new object();
        private const string WELL_STATUS_XREF_TABLE = "R_WELL_STATUS_XREF";

        public PPDM39DefaultsRepository(IDMEEditor editor = null, string connectionName = "PPDM39", PPDM39.Core.Metadata.IPPDMMetadataRepository metadata = null)
        {
            _editor = editor;
            _connectionName = connectionName;
            InitializeDefaultJurisdictions();
        }
        // Standard PPDM39 default values
        private const string ACTIVE_IND_YES = "Y";
        private const string ACTIVE_IND_NO = "N";
        private const string DEFAULT_ROW_QUALITY = "GOOD";
        private const string DEFAULT_PREFERRED_IND = "N";
        private const string DEFAULT_CERTIFIED_IND = "N";
        private const string DEFAULT_HIERARCHY_TYPE = "PARENT_CHILD";
        private const string DEFAULT_STRAT_COLUMN_TYPE = "STANDARD";
        private const string DEFAULT_STRAT_TYPE = "LITHOSTRATIGRAPHIC";
        private const string DEFAULT_STRAT_UNIT_TYPE = "FORMATION";
        private const string DEFAULT_STRAT_STATUS = "VALID";
        private const string DEFAULT_AREA_TYPE = "FIELD";
        private const string DEFAULT_DEPTH_OUOM = "M";
        private const string DEFAULT_PICK_DEPTH_OUOM = "M";
        private const string DEFAULT_AZIMUTH_NORTH_TYPE = "TRUE_NORTH";
        private const string DEFAULT_CONFORMITY_RELATIONSHIP = "CONFORMABLE";
        private const string DEFAULT_STRAT_INTERPRET_METHOD = "LOG_INTERPRETATION";
        private const string DEFAULT_PICK_QUALITY = "GOOD";
        private const string DEFAULT_PICK_VERSION_TYPE = "CURRENT";
        private const string DEFAULT_TVD_METHOD = "STANDARD";
        private const string DEFAULT_SOURCE = "SYSTEM";
        private const string DEFAULT_REMARK = "";

        // Well Structure XREF_TYPE values (used in WELL_XREF table)
        private const string WELL_ORIGIN_XREF_TYPE = "WELL_ORIGIN";
        private const string WELLBORE_XREF_TYPE = "WELLBORE";
        private const string WELLBORE_SEGMENT_XREF_TYPE = "WELLBORE_SEGMENT";
        private const string WELLBORE_CONTACT_INTERVAL_XREF_TYPE = "WELLBORE_CONTACT_INTERVAL";
        private const string WELLBORE_COMPLETION_XREF_TYPE = "WELLBORE_COMPLETION";
        private const string WELLHEAD_STREAM_XREF_TYPE = "WELLHEAD_STREAM";

        // Default Well Status Types (STATUS_TYPE values from PPDM 3.9 Well Facets)
        // These are the standard facets that should be initialized for every well/wellbore
        // Based on PPDM 3.9 Well Facets documentation
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES = new List<string>
        {
            "Business Interest",           // Well/Wellbore - Only 1 value (mutually exclusive, ranked)
            "Business Life Cycle Phase",   // Well - Changes predictably, may reoccur
            "Business Intention",          // Well - Set at Drilling start, doesn't change unless reverts to Planning
            "Operatorship",                // Well - May change if accountability transfers
            "Outcome",                     // Well - Doesn't change unless Business Life Cycle reverts to Planning
            "Lahee Class",                 // Well - Doesn't change over life cycle
            "Role",                        // Wellbore - May change over life cycle
            "Play Type",                   // Well - May change if Role changes or different formation completed
            "Well Structure",               // Well - May change as new wellbores added
            "Trajectory Type",             // Wellbore - Doesn't change over life cycle
            "Fluid Direction",             // Wellhead Stream - Can change
            "Well Reporting Class",        // Well - Can change over life cycle
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion - Can change
            "Wellbore Status",              // Wellbore - Slowly changing, measures milestones
            "Well Status"                   // Well - Summary state, may change infrequently
        };

        // STATUS_TYPEs specific to Well level (not Wellbore)
        // Based on PPDM 3.9 documentation: Well Component = Well
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELL = new List<string>
        {
            "Business Interest",           // Well or Wellbore (but typically Well)
            "Business Life Cycle Phase",   // Well
            "Business Intention",          // Well
            "Operatorship",                // Well
            "Outcome",                     // Well
            "Lahee Class",                 // Well
            "Play Type",                   // Well
            "Well Structure",               // Well
            "Well Reporting Class",        // Well
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Well Status"                   // Well
        };

        // STATUS_TYPEs specific to Wellbore level
        // Based on PPDM 3.9 documentation: Well Component = Wellbore
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE = new List<string>
        {
            "Business Interest",           // Well or Wellbore (can be at Wellbore level)
            "Role",                        // Wellbore
            "Trajectory Type",             // Wellbore
            "Fluid Type",                  // Well/Wellbore/Wellbore Completion
            "Wellbore Status"              // Wellbore
        };

        // STATUS_TYPEs specific to Wellhead Stream level
        private static readonly List<string> DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM = new List<string>
        {
            "Fluid Direction"              // Wellhead Stream
        };

        public string GetActiveIndicatorYes() => ACTIVE_IND_YES;
        public string GetActiveIndicatorNo() => ACTIVE_IND_NO;
        public string GetDefaultRowQuality() => DEFAULT_ROW_QUALITY;
        public string GetDefaultPreferredIndicator() => DEFAULT_PREFERRED_IND;
        public string GetDefaultCertifiedIndicator() => DEFAULT_CERTIFIED_IND;
        public string GetDefaultHierarchyType() => DEFAULT_HIERARCHY_TYPE;
        public string GetDefaultStratColumnType() => DEFAULT_STRAT_COLUMN_TYPE;
        public string GetDefaultStratType() => DEFAULT_STRAT_TYPE;
        public string GetDefaultStratUnitType() => DEFAULT_STRAT_UNIT_TYPE;
        public string GetDefaultStratStatus() => DEFAULT_STRAT_STATUS;
        public string GetDefaultAreaType() => DEFAULT_AREA_TYPE;
        public string GetDefaultDepthOuom() => DEFAULT_DEPTH_OUOM;
        public string GetDefaultPickDepthOuom() => DEFAULT_PICK_DEPTH_OUOM;
        public string GetDefaultAzimuthNorthType() => DEFAULT_AZIMUTH_NORTH_TYPE;
        public string GetDefaultConformityRelationship() => DEFAULT_CONFORMITY_RELATIONSHIP;
        public string GetDefaultStratInterpretMethod() => DEFAULT_STRAT_INTERPRET_METHOD;
        public string GetDefaultPickQuality() => DEFAULT_PICK_QUALITY;
        public string GetDefaultPickVersionType() => DEFAULT_PICK_VERSION_TYPE;
        public string GetDefaultTvdMethod() => DEFAULT_TVD_METHOD;
        public string GetDefaultSource() => DEFAULT_SOURCE;
        public string GetDefaultRemark() => DEFAULT_REMARK;

        // Well Structure XREF_TYPE Methods (default values only)

        public string GetWellOriginXrefType() => WELL_ORIGIN_XREF_TYPE;
        public string GetWellboreXrefType() => WELLBORE_XREF_TYPE;
        public string GetWellboreSegmentXrefType() => WELLBORE_SEGMENT_XREF_TYPE;
        public string GetWellboreContactIntervalXrefType() => WELLBORE_CONTACT_INTERVAL_XREF_TYPE;
        public string GetWellboreCompletionXrefType() => WELLBORE_COMPLETION_XREF_TYPE;
        public string GetWellheadStreamXrefType() => WELLHEAD_STREAM_XREF_TYPE;

        public Dictionary<string, string> GetAllWellStructureXrefTypes()
        {
            return new Dictionary<string, string>
            {
                { "Well Origin", WELL_ORIGIN_XREF_TYPE },
                { "Wellbore", WELLBORE_XREF_TYPE },
                { "Wellbore Segment", WELLBORE_SEGMENT_XREF_TYPE },
                { "Wellbore Contact Interval", WELLBORE_CONTACT_INTERVAL_XREF_TYPE },
                { "Wellbore Completion", WELLBORE_COMPLETION_XREF_TYPE },
                { "Wellhead Stream", WELLHEAD_STREAM_XREF_TYPE }
            };
        }

        // Well Status Facets Methods

        /// <summary>
        /// Gets the UnitOfWork for R_WELL_STATUS_XREF table
        /// </summary>
        private IUnitOfWorkWrapper GetWellStatusXrefUnitOfWork()
        {
            if (_editor == null)
                throw new InvalidOperationException("IDMEEditor is required for well status facet queries. Please inject IDMEEditor in constructor.");

            if (_wellStatusXrefUnitOfWork == null)
            {
                lock (_unitOfWorkLock)
                {
                    if (_wellStatusXrefUnitOfWork == null)
                    {
                        _wellStatusXrefUnitOfWork = UnitOfWorkFactory.CreateUnitOfWork(
                            typeof(object), // Dynamic type for cross-reference table
                            _editor,
                            _connectionName,
                            WELL_STATUS_XREF_TABLE,
                            "STATUS_XREF_ID");
                    }
                }
            }

            return _wellStatusXrefUnitOfWork;
        }

        public string GetWellStatusXrefTableName() => WELL_STATUS_XREF_TABLE;

        public async Task<Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
                throw new ArgumentException("Status ID cannot be null or empty", nameof(statusId));

            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS_XREF_ID", FilterValue = statusId, Operator = "=" },
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusXrefs = ConvertToDynamicList(queryResult);

            // Extract facet information from the cross-reference
            var facets = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            foreach (var xref in statusXrefs)
            {
                // Extract facet values from the cross-reference record
                if (xref is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    foreach (var kvp in dict)
                    {
                        if (!facets.ContainsKey(kvp.Key) && 
                            !string.Equals(kvp.Key, "STATUS_XREF_ID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kvp.Key, "ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                        {
                            facets[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return facets;
        }

        public async Task<Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status)
        {
            if (string.IsNullOrWhiteSpace(statusType))
                throw new ArgumentException("Status Type cannot be null or empty", nameof(statusType));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be null or empty", nameof(status));

            // First, we need to find the STATUS_XREF_ID for this STATUS_TYPE and STATUS
            // This would typically come from R_WELL_STATUS table or similar
            // For now, we'll use a pattern: STATUS_TYPE + "_" + STATUS
            var statusId = $"{statusType}_{status}";
            return await GetWellStatusFacetsAsync(statusId);
        }

        public async Task<Dictionary<string, Dictionary<string, object>>> GetAllWellStatusFacetsAsync()
        {
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = WELL_STATUS_XREF_TABLE;

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusXrefs = ConvertToDynamicList(queryResult);

            // Group facets by STATUS_XREF_ID
            var allFacets = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            foreach (var xref in statusXrefs)
            {
                if (xref is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    var statusId = dict.ContainsKey("STATUS_XREF_ID") ? dict["STATUS_XREF_ID"]?.ToString() : null;

                    if (string.IsNullOrWhiteSpace(statusId))
                        continue;

                    if (!allFacets.ContainsKey(statusId))
                    {
                        allFacets[statusId] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    }

                    foreach (var kvp in dict)
                    {
                        if (!string.Equals(kvp.Key, "STATUS_XREF_ID", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(kvp.Key, "ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!allFacets[statusId].ContainsKey(kvp.Key))
                            {
                                allFacets[statusId][kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }
            }

            return allFacets;
        }

        private List<dynamic> ConvertToDynamicList(dynamic result)
        {
            var list = new List<dynamic>();
            if (result == null) return list;

            if (result is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    list.Add(item);
                }
            }
            else
            {
                list.Add(result);
            }

            return list;
        }

        // Well Status TYPE Methods (default values from R_WELL_STATUS)

        public async Task<List<string>> GetAllWellStatusTypesAsync()
        {
            if (_editor == null)
                throw new InvalidOperationException("IDMEEditor is required for well status type queries. Please inject IDMEEditor in constructor.");

            // Get all distinct STATUS_TYPE values from R_WELL_STATUS
            var uow = GetWellStatusXrefUnitOfWork();
            uow.EntityName = "R_WELL_STATUS";

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
            };

            var queryResult = await uow.Get(filters);
            var statusRefs = ConvertToDynamicList(queryResult);

            var statusTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var statusRef in statusRefs)
            {
                if (statusRef is System.Dynamic.ExpandoObject expando)
                {
                    var dict = (IDictionary<string, object>)expando;
                    if (dict.ContainsKey("STATUS_TYPE") && dict["STATUS_TYPE"] != null)
                    {
                        var statusType = dict["STATUS_TYPE"].ToString();
                        if (!string.IsNullOrWhiteSpace(statusType))
                        {
                            statusTypes.Add(statusType);
                        }
                    }
                }
            }

            return statusTypes.OrderBy(t => t).ToList();
        }

        public async Task<Dictionary<string, List<string>>> GetWellStatusTypesGroupedAsync()
        {
            // Group status types by common categories
            // This is a logical grouping - can be customized based on business needs
            var allTypes = await GetAllWellStatusTypesAsync();
            var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Business/Operational Status Types
            var businessTypes = new List<string>();
            var operationalTypes = new List<string>();
            var technicalTypes = new List<string>();

            foreach (var statusType in allTypes)
            {
                var upperType = statusType.ToUpper();
                if (upperType.Contains("BUSINESS") || upperType.Contains("LIFE") || upperType.Contains("INTENTION") || 
                    upperType.Contains("OUTCOME") || upperType.Contains("INTEREST") || upperType.Contains("OPERATORSHIP"))
                {
                    businessTypes.Add(statusType);
                }
                else if (upperType.Contains("STATUS") || upperType.Contains("ROLE") || upperType.Contains("TRAJECTORY") ||
                         upperType.Contains("FLUID") || upperType.Contains("WELLBORE"))
                {
                    operationalTypes.Add(statusType);
                }
                else if (upperType.Contains("CLASS") || upperType.Contains("TYPE") || upperType.Contains("STRUCTURE"))
                {
                    technicalTypes.Add(statusType);
                }
                else
                {
                    // Default to operational
                    operationalTypes.Add(statusType);
                }
            }

            if (businessTypes.Any())
                grouped["Business"] = businessTypes.OrderBy(t => t).ToList();
            if (operationalTypes.Any())
                grouped["Operational"] = operationalTypes.OrderBy(t => t).ToList();
            if (technicalTypes.Any())
                grouped["Technical"] = technicalTypes.OrderBy(t => t).ToList();

            return grouped;
        }

        /// <summary>
        /// Gets the list of default STATUS_TYPEs that should be initialized for every new well or wellbore
        /// These are the standard well facets from PPDM 3.9
        /// </summary>
        public List<string> GetDefaultWellStatusTypes()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wells (not wellbores)
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWell()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELL);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellbores
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWellbore()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELLBORE);
        }

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellhead streams
        /// </summary>
        public List<string> GetDefaultWellStatusTypesForWellheadStream()
        {
            return new List<string>(DEFAULT_WELL_STATUS_TYPES_FOR_WELLHEAD_STREAM);
        }

        // ID Type Configuration for PPDM Tables

        /// <summary>
        /// Gets whether PPDM tables use string IDs
        /// All PPDM tables use string IDs by default
        /// </summary>
        public bool UseStringIds()
        {
            return true; // All PPDM tables use string IDs
        }

        /// <summary>
        /// Gets the ID type for a specific table
        /// Defaults to string for all PPDM tables
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>ID type name (always "String" for PPDM tables)</returns>
        public string GetIdTypeForTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // All PPDM tables use string IDs
            return "String";
        }

        /// <summary>
        /// Formats an ID value according to the table's ID type configuration
        /// For PPDM tables, this always converts to string
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="id">ID value</param>
        /// <returns>Formatted ID value as string</returns>
        public string FormatIdForTable(string tableName, object id)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));

            // All PPDM tables use string IDs, so convert to string
            return id?.ToString() ?? string.Empty;
        }

        #region Field Mapping Configuration

        // In-memory storage for field mappings (can be persisted to database or config file)
        private readonly Dictionary<string, FieldMappingConfig> _fieldMappings = new Dictionary<string, FieldMappingConfig>();

        /// <summary>
        /// Gets field mapping configuration for a specific property mapping key
        /// </summary>
        public async Task<FieldMappingConfig?> GetFieldMappingAsync(string mappingKey)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            // Initialize default mappings if not already loaded
            if (_fieldMappings.Count == 0)
            {
                InitializeDefaultFieldMappings();
            }

            return _fieldMappings.TryGetValue(mappingKey, out var config) ? config : null;
        }

        /// <summary>
        /// Gets all field mappings for a specific mapping category
        /// </summary>
        public async Task<Dictionary<string, FieldMappingConfig>> GetFieldMappingsByCategoryAsync(string category)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            // Initialize default mappings if not already loaded
            if (_fieldMappings.Count == 0)
            {
                InitializeDefaultFieldMappings();
            }

            return _fieldMappings
                .Where(kvp => kvp.Value.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        /// <summary>
        /// Sets or updates a field mapping configuration
        /// </summary>
        public async Task SetFieldMappingAsync(string mappingKey, FieldMappingConfig config)
        {
            await Task.CompletedTask; // Async signature for future database persistence
            
            if (string.IsNullOrEmpty(mappingKey))
                throw new ArgumentException("Mapping key cannot be null or empty", nameof(mappingKey));
            
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.MappingKey = mappingKey;
            _fieldMappings[mappingKey] = config;
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

        #endregion

        #region Jurisdiction-Specific Support

        private string? _currentJurisdiction;
        private bool _jurisdictionsLoadedFromDatabase = false;
        private readonly object _jurisdictionLock = new object();

        // Default jurisdiction configurations (instance variable to allow database updates)
        private readonly Dictionary<string, JurisdictionConfig> _jurisdictionConfigs = new Dictionary<string, JurisdictionConfig>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initialize default jurisdiction configurations
        /// Called during construction or when loading from database
        /// </summary>
        private void InitializeDefaultJurisdictions()
        {
            lock (_jurisdictionLock)
            {
                if (_jurisdictionConfigs.Count > 0)
                    return; // Already initialized

                // North America
                _jurisdictionConfigs["US"] = new JurisdictionConfig
                {
                    JurisdictionId = "US",
                    Currency = "USD",
                    CurrencySymbol = "$",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CA"] = new JurisdictionConfig
                {
                    JurisdictionId = "CA",
                    Currency = "CAD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };
                _jurisdictionConfigs["MX"] = new JurisdictionConfig
                {
                    JurisdictionId = "MX",
                    Currency = "MXN",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Europe
                _jurisdictionConfigs["GB"] = new JurisdictionConfig
                {
                    JurisdictionId = "GB",
                    Currency = "GBP",
                    CurrencySymbol = "£",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["NO"] = new JurisdictionConfig
                {
                    JurisdictionId = "NO",
                    Currency = "NOK",
                    CurrencySymbol = "kr",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["NL"] = new JurisdictionConfig
                {
                    JurisdictionId = "NL",
                    Currency = "EUR",
                    CurrencySymbol = "€",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["DK"] = new JurisdictionConfig
                {
                    JurisdictionId = "DK",
                    Currency = "DKK",
                    CurrencySymbol = "kr",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "SM3" }, { "GAS", "SM3" }, { "WATER", "SM3" } }
                };
                _jurisdictionConfigs["RU"] = new JurisdictionConfig
                {
                    JurisdictionId = "RU",
                    Currency = "RUB",
                    CurrencySymbol = "₽",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "ATM",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };

                // Middle East
                _jurisdictionConfigs["SA"] = new JurisdictionConfig
                {
                    JurisdictionId = "SA",
                    Currency = "SAR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AE"] = new JurisdictionConfig
                {
                    JurisdictionId = "AE",
                    Currency = "AED",
                    CurrencySymbol = "د.إ",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["KW"] = new JurisdictionConfig
                {
                    JurisdictionId = "KW",
                    Currency = "KWD",
                    CurrencySymbol = "د.ك",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["QA"] = new JurisdictionConfig
                {
                    JurisdictionId = "QA",
                    Currency = "QAR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["OM"] = new JurisdictionConfig
                {
                    JurisdictionId = "OM",
                    Currency = "OMR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["IQ"] = new JurisdictionConfig
                {
                    JurisdictionId = "IQ",
                    Currency = "IQD",
                    CurrencySymbol = "ع.د",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["IR"] = new JurisdictionConfig
                {
                    JurisdictionId = "IR",
                    Currency = "IRR",
                    CurrencySymbol = "﷼",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Asia Pacific
                _jurisdictionConfigs["AU"] = new JurisdictionConfig
                {
                    JurisdictionId = "AU",
                    Currency = "AUD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CN"] = new JurisdictionConfig
                {
                    JurisdictionId = "CN",
                    Currency = "CNY",
                    CurrencySymbol = "¥",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "MPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "M3" }, { "GAS", "E3M3" }, { "WATER", "M3" } }
                };
                _jurisdictionConfigs["IN"] = new JurisdictionConfig
                {
                    JurisdictionId = "IN",
                    Currency = "INR",
                    CurrencySymbol = "₹",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["MY"] = new JurisdictionConfig
                {
                    JurisdictionId = "MY",
                    Currency = "MYR",
                    CurrencySymbol = "RM",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["SG"] = new JurisdictionConfig
                {
                    JurisdictionId = "SG",
                    Currency = "SGD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["TH"] = new JurisdictionConfig
                {
                    JurisdictionId = "TH",
                    Currency = "THB",
                    CurrencySymbol = "฿",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["ID"] = new JurisdictionConfig
                {
                    JurisdictionId = "ID",
                    Currency = "IDR",
                    CurrencySymbol = "Rp",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["JP"] = new JurisdictionConfig
                {
                    JurisdictionId = "JP",
                    Currency = "JPY",
                    CurrencySymbol = "¥",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "MPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // South America
                _jurisdictionConfigs["BR"] = new JurisdictionConfig
                {
                    JurisdictionId = "BR",
                    Currency = "BRL",
                    CurrencySymbol = "R$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "KPA",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AR"] = new JurisdictionConfig
                {
                    JurisdictionId = "AR",
                    Currency = "ARS",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["CO"] = new JurisdictionConfig
                {
                    JurisdictionId = "CO",
                    Currency = "COP",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["VE"] = new JurisdictionConfig
                {
                    JurisdictionId = "VE",
                    Currency = "VES",
                    CurrencySymbol = "Bs",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["EC"] = new JurisdictionConfig
                {
                    JurisdictionId = "EC",
                    Currency = "USD",
                    CurrencySymbol = "$",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };

                // Africa
                _jurisdictionConfigs["NG"] = new JurisdictionConfig
                {
                    JurisdictionId = "NG",
                    Currency = "NGN",
                    CurrencySymbol = "₦",
                    DepthUnit = "FT",
                    TemperatureUnit = "F",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["ZA"] = new JurisdictionConfig
                {
                    JurisdictionId = "ZA",
                    Currency = "ZAR",
                    CurrencySymbol = "R",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["AO"] = new JurisdictionConfig
                {
                    JurisdictionId = "AO",
                    Currency = "AOA",
                    CurrencySymbol = "Kz",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["DZ"] = new JurisdictionConfig
                {
                    JurisdictionId = "DZ",
                    Currency = "DZD",
                    CurrencySymbol = "د.ج",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["LY"] = new JurisdictionConfig
                {
                    JurisdictionId = "LY",
                    Currency = "LYD",
                    CurrencySymbol = "ل.د",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "BAR",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
                _jurisdictionConfigs["EG"] = new JurisdictionConfig
                {
                    JurisdictionId = "EG",
                    Currency = "EGP",
                    CurrencySymbol = "£",
                    DepthUnit = "M",
                    TemperatureUnit = "C",
                    PressureUnit = "PSI",
                    VolumeUnits = new Dictionary<string, string> { { "OIL", "BBL" }, { "GAS", "MSCF" }, { "WATER", "BBL" } }
                };
            }
        }

        private JurisdictionConfig GetJurisdictionConfig(string jurisdictionId)
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            if (string.IsNullOrEmpty(jurisdictionId))
                jurisdictionId = _currentJurisdiction ?? "US";

            return _jurisdictionConfigs.TryGetValue(jurisdictionId, out var config) 
                ? config 
                : _jurisdictionConfigs["US"]; // Default to US
        }

        public string GetCurrencyForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).Currency;
        }

        public string GetCurrencySymbolForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).CurrencySymbol;
        }

        public Dictionary<string, string> GetVolumeUnitsForJurisdiction(string jurisdictionId)
        {
            return new Dictionary<string, string>(GetJurisdictionConfig(jurisdictionId).VolumeUnits);
        }

        public string GetDepthUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).DepthUnit;
        }

        public string GetTemperatureUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).TemperatureUnit;
        }

        public string GetPressureUnitForJurisdiction(string jurisdictionId)
        {
            return GetJurisdictionConfig(jurisdictionId).PressureUnit;
        }

        public Dictionary<string, string> GetAllUnitsForJurisdiction(string jurisdictionId)
        {
            var config = GetJurisdictionConfig(jurisdictionId);
            var units = new Dictionary<string, string>
            {
                { "DEPTH", config.DepthUnit },
                { "TEMPERATURE", config.TemperatureUnit },
                { "PRESSURE", config.PressureUnit }
            };

            foreach (var kvp in config.VolumeUnits)
            {
                units[$"VOLUME_{kvp.Key}"] = kvp.Value;
            }

            return units;
        }

        public async Task<List<JurisdictionValidationRule>> GetValidationRulesForJurisdictionAsync(string jurisdictionId, string? entityType = null)
        {
            await Task.CompletedTask;
            
            // Return empty list - can be extended to load from database or configuration
            var rules = new List<JurisdictionValidationRule>();

            // Example rules that could be jurisdiction-specific
            if (jurisdictionId.Equals("CA", StringComparison.OrdinalIgnoreCase))
            {
                rules.Add(new JurisdictionValidationRule
                {
                    RuleId = "CA_UWI_FORMAT",
                    JurisdictionId = "CA",
                    EntityType = "WELL",
                    FieldName = "UWI",
                    RuleType = "Pattern",
                    RuleValue = @"^\d{2}-\d{2}-\d{3}-\d{2}W\d{1}$",
                    ErrorMessage = "UWI must match Canadian format (XX-XX-XXX-XXW#)",
                    Severity = "Error",
                    IsActive = true
                });
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                rules = rules.Where(r => r.EntityType == null || r.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return rules;
        }

        public async Task<JurisdictionValidationResult> ValidateValueForJurisdictionAsync(string jurisdictionId, string fieldName, object value, string? entityType = null)
        {
            var result = new JurisdictionValidationResult
            {
                IsValid = true,
                FieldName = fieldName
            };

            var rules = await GetValidationRulesForJurisdictionAsync(jurisdictionId, entityType);
            var applicableRules = rules.Where(r => r.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase) && r.IsActive).ToList();

            foreach (var rule in applicableRules)
            {
                var isValid = ValidateRule(rule, value);
                if (!isValid)
                {
                    result.IsValid = false;
                    result.RuleId = rule.RuleId;

                    if (rule.Severity == "Error")
                        result.ErrorMessages.Add(rule.ErrorMessage);
                    else if (rule.Severity == "Warning")
                        result.WarningMessages.Add(rule.ErrorMessage);
                }
            }

            return result;
        }

        private bool ValidateRule(JurisdictionValidationRule rule, object value)
        {
            if (value == null)
                return rule.RuleType != "Required";

            var stringValue = value.ToString() ?? string.Empty;

            return rule.RuleType switch
            {
                "Required" => !string.IsNullOrWhiteSpace(stringValue),
                "Pattern" when !string.IsNullOrEmpty(rule.RuleValue) => 
                    System.Text.RegularExpressions.Regex.IsMatch(stringValue, rule.RuleValue),
                "Range" when !string.IsNullOrEmpty(rule.RuleValue) => ValidateRange(stringValue, rule.RuleValue),
                _ => true
            };
        }

        private bool ValidateRange(string value, string rangeSpec)
        {
            if (!decimal.TryParse(value, out var numValue))
                return false;

            var parts = rangeSpec.Split(',');
            if (parts.Length != 2)
                return false;

            var hasMin = decimal.TryParse(parts[0], out var min);
            var hasMax = decimal.TryParse(parts[1], out var max);

            if (hasMin && numValue < min) return false;
            if (hasMax && numValue > max) return false;

            return true;
        }

        public async Task<Dictionary<string, object>> GetJurisdictionDefaultsForTableAsync(string jurisdictionId, string tableName)
        {
            await Task.CompletedTask;

            var defaults = new Dictionary<string, object>();
            var config = GetJurisdictionConfig(jurisdictionId);

            // Add jurisdiction-specific defaults based on table
            switch (tableName.ToUpperInvariant())
            {
                case "WELL":
                case "WELLBORE":
                    defaults["DEPTH_OUOM"] = config.DepthUnit;
                    break;

                case "PRODUCTION":
                    defaults["OIL_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("OIL", "BBL");
                    defaults["GAS_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("GAS", "MSCF");
                    defaults["WATER_VOLUME_OUOM"] = config.VolumeUnits.GetValueOrDefault("WATER", "BBL");
                    break;

                case "POOL":
                    defaults["PRESSURE_OUOM"] = config.PressureUnit;
                    defaults["TEMPERATURE_OUOM"] = config.TemperatureUnit;
                    break;
            }

            return defaults;
        }

        public void SetCurrentJurisdiction(string jurisdictionId)
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            if (!string.IsNullOrEmpty(jurisdictionId) && _jurisdictionConfigs.ContainsKey(jurisdictionId))
            {
                _currentJurisdiction = jurisdictionId;
            }
            else
            {
                _currentJurisdiction = "US"; // Default to US
            }
        }

        public string? GetCurrentJurisdiction()
        {
            return _currentJurisdiction;
        }

        public async Task<List<string>> GetAvailableJurisdictionsAsync()
        {
            // Ensure defaults are initialized
            if (_jurisdictionConfigs.Count == 0)
            {
                InitializeDefaultJurisdictions();
            }

            // Optionally load from database if not already loaded
            if (!_jurisdictionsLoadedFromDatabase && _editor != null)
            {
                await LoadJurisdictionsFromBusinessAssociateAsync();
            }

            return _jurisdictionConfigs.Keys.OrderBy(k => k).ToList();
        }

        /// <summary>
        /// Loads jurisdictions from BUSINESS_ASSOCIATE table based on JURISDICTION field
        /// Merges with existing default jurisdictions, with database values taking precedence
        /// </summary>
        public async Task LoadJurisdictionsFromBusinessAssociateAsync()
        {
            if (_editor == null)
            {
                return; // Cannot load from database without editor
            }

            try
            {
                lock (_jurisdictionLock)
                {
                    if (_jurisdictionsLoadedFromDatabase)
                        return; // Already loaded
                }

                // Ensure defaults are initialized first
                if (_jurisdictionConfigs.Count == 0)
                {
                    InitializeDefaultJurisdictions();
                }

                var uow = UnitOfWorkFactory.CreateUnitOfWork(
                    typeof(object),
                    _editor,
                    _connectionName,
                    "BUSINESS_ASSOCIATE",
                    "BUSINESS_ASSOCIATE_ID");

                // Get distinct jurisdictions from BUSINESS_ASSOCIATE table
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ACTIVE_IND", FilterValue = GetActiveIndicatorYes(), Operator = "=" }
                };

                var queryResult = await uow.Get(filters);
                var businessAssociates = ConvertToDynamicList(queryResult);

                // Extract unique jurisdictions and build configurations
                var jurisdictionsFromDb = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var ba in businessAssociates)
                {
                    if (ba is System.Dynamic.ExpandoObject expando)
                    {
                        var dict = (IDictionary<string, object>)expando;
                        
                        // Check for JURISDICTION field
                        if (dict.ContainsKey("JURISDICTION") && dict["JURISDICTION"] != null)
                        {
                            var jurisdictionId = dict["JURISDICTION"].ToString();
                            if (!string.IsNullOrWhiteSpace(jurisdictionId) && !jurisdictionsFromDb.Contains(jurisdictionId))
                            {
                                jurisdictionsFromDb.Add(jurisdictionId);

                                // If this jurisdiction doesn't exist in defaults, create a config from BUSINESS_ASSOCIATE data
                                if (!_jurisdictionConfigs.ContainsKey(jurisdictionId))
                                {
                                    var config = CreateJurisdictionConfigFromBusinessAssociate(dict, jurisdictionId);
                                    if (config != null)
                                    {
                                        _jurisdictionConfigs[jurisdictionId] = config;
                                    }
                                }
                            }
                        }

                        // Also check for COUNTRY_CODE or COUNTRY fields
                        var countryField = dict.ContainsKey("COUNTRY_CODE") ? "COUNTRY_CODE" :
                                          dict.ContainsKey("COUNTRY") ? "COUNTRY" : null;

                        if (countryField != null && dict[countryField] != null)
                        {
                            var countryCode = dict[countryField].ToString();
                            if (!string.IsNullOrWhiteSpace(countryCode) && !jurisdictionsFromDb.Contains(countryCode))
                            {
                                jurisdictionsFromDb.Add(countryCode);

                                if (!_jurisdictionConfigs.ContainsKey(countryCode))
                                {
                                    var config = CreateJurisdictionConfigFromBusinessAssociate(dict, countryCode);
                                    if (config != null)
                                    {
                                        _jurisdictionConfigs[countryCode] = config;
                                    }
                                }
                            }
                        }
                    }
                }

                lock (_jurisdictionLock)
                {
                    _jurisdictionsLoadedFromDatabase = true;
                }
            }
            catch (Exception)
            {
                // Silently fail - defaults will be used instead
                // Could log this in production
            }
        }

        /// <summary>
        /// Creates a jurisdiction configuration from BUSINESS_ASSOCIATE record
        /// Uses defaults and infers from common patterns if fields are not available
        /// </summary>
        private JurisdictionConfig? CreateJurisdictionConfigFromBusinessAssociate(IDictionary<string, object> dict, string jurisdictionId)
        {
            // Try to extract currency and unit information from BUSINESS_ASSOCIATE
            // Many fields may not exist, so we'll use intelligent defaults based on jurisdiction ID
            
            var config = new JurisdictionConfig
            {
                JurisdictionId = jurisdictionId
            };

            // Try to get currency from BUSINESS_ASSOCIATE fields
            if (dict.ContainsKey("CURRENCY_CODE") && dict["CURRENCY_CODE"] != null)
            {
                config.Currency = dict["CURRENCY_CODE"].ToString() ?? "USD";
            }
            else if (dict.ContainsKey("CURRENCY") && dict["CURRENCY"] != null)
            {
                config.Currency = dict["CURRENCY"].ToString() ?? "USD";
            }
            else
            {
                // Infer currency from jurisdiction ID (common country codes)
                config.Currency = GetCurrencyByCountryCode(jurisdictionId);
            }

            // Set currency symbol based on currency
            config.CurrencySymbol = GetCurrencySymbol(config.Currency);

            // Try to get unit preferences from BUSINESS_ASSOCIATE
            // If not available, infer based on jurisdiction (metric vs imperial)
            var useMetric = IsMetricJurisdiction(jurisdictionId);
            
            config.DepthUnit = useMetric ? "M" : "FT";
            config.TemperatureUnit = useMetric ? "C" : "F";
            config.PressureUnit = useMetric ? (config.Currency == "EUR" || config.Currency == "GBP" ? "BAR" : "KPA") : "PSI";
            
            // Volume units - typically BBL for oil regardless, but gas can vary
            config.VolumeUnits = new Dictionary<string, string>
            {
                { "OIL", useMetric && (jurisdictionId == "NO" || jurisdictionId == "NL") ? "SM3" : "BBL" },
                { "GAS", useMetric ? (jurisdictionId == "NO" || jurisdictionId == "NL" ? "SM3" : "E3M3") : "MSCF" },
                { "WATER", useMetric && (jurisdictionId == "NO" || jurisdictionId == "NL") ? "SM3" : "BBL" }
            };

            return config;
        }

        /// <summary>
        /// Gets currency code based on country/jurisdiction code
        /// </summary>
        private string GetCurrencyByCountryCode(string countryCode)
        {
            // Map common country codes to currencies
            return countryCode.ToUpperInvariant() switch
            {
                "US" => "USD",
                "CA" => "CAD",
                "MX" => "MXN",
                "GB" => "GBP",
                "NO" => "NOK",
                "DK" => "DKK",
                "NL" => "EUR",
                "DE" => "EUR",
                "FR" => "EUR",
                "IT" => "EUR",
                "ES" => "EUR",
                "SA" => "SAR",
                "AE" => "AED",
                "KW" => "KWD",
                "QA" => "QAR",
                "AU" => "AUD",
                "CN" => "CNY",
                "IN" => "INR",
                "JP" => "JPY",
                "BR" => "BRL",
                "AR" => "ARS",
                "NG" => "NGN",
                "ZA" => "ZAR",
                _ => "USD" // Default to USD
            };
        }

        /// <summary>
        /// Gets currency symbol based on currency code
        /// </summary>
        private string GetCurrencySymbol(string currencyCode)
        {
            return currencyCode.ToUpperInvariant() switch
            {
                "USD" => "$",
                "CAD" => "$",
                "MXN" => "$",
                "AUD" => "$",
                "SGD" => "$",
                "GBP" => "£",
                "EUR" => "€",
                "NOK" => "kr",
                "DKK" => "kr",
                "SAR" => "﷼",
                "AED" => "د.إ",
                "CNY" => "¥",
                "JPY" => "¥",
                "INR" => "₹",
                "BRL" => "R$",
                "RUB" => "₽",
                _ => "$" // Default
            };
        }

        /// <summary>
        /// Determines if a jurisdiction typically uses metric units
        /// </summary>
        private bool IsMetricJurisdiction(string jurisdictionId)
        {
            // US and a few others use imperial
            var imperialJurisdictions = new[] { "US", "SA", "AE", "KW", "QA", "OM", "IQ", "NG" };
            return !imperialJurisdictions.Contains(jurisdictionId.ToUpperInvariant());
        }

        /// <summary>
        /// Internal configuration class for jurisdiction settings
        /// </summary>
        private class JurisdictionConfig
        {
            public string JurisdictionId { get; set; } = string.Empty;
            public string Currency { get; set; } = "USD";
            public string CurrencySymbol { get; set; } = "$";
            public string DepthUnit { get; set; } = "FT";
            public string TemperatureUnit { get; set; } = "F";
            public string PressureUnit { get; set; } = "PSI";
            public Dictionary<string, string> VolumeUnits { get; set; } = new Dictionary<string, string>();
        }

        #endregion
    }
}

