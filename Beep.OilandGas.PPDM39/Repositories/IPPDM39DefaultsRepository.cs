using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Repositories
{
    /// <summary>
    /// Repository for PPDM39 system-wide default values and reference data
    /// Provides consistent default values across all PPDM39 repositories
    /// </summary>
    public interface IPPDM39DefaultsRepository
    {
        /// <summary>
        /// Gets the default value for ACTIVE_IND when creating new records
        /// </summary>
        string GetActiveIndicatorYes();

        /// <summary>
        /// Gets the value for ACTIVE_IND when soft deleting records
        /// </summary>
        string GetActiveIndicatorNo();

        /// <summary>
        /// Gets the default value for ROW_QUALITY
        /// </summary>
        string GetDefaultRowQuality();

        /// <summary>
        /// Gets the default value for PREFERRED_IND
        /// </summary>
        string GetDefaultPreferredIndicator();

        /// <summary>
        /// Gets the default value for CERTIFIED_IND
        /// </summary>
        string GetDefaultCertifiedIndicator();

        /// <summary>
        /// Gets the default value for HIERARCHY_TYPE
        /// </summary>
        string GetDefaultHierarchyType();

        /// <summary>
        /// Gets the default value for STRAT_COLUMN_TYPE
        /// </summary>
        string GetDefaultStratColumnType();

        /// <summary>
        /// Gets the default value for STRAT_TYPE
        /// </summary>
        string GetDefaultStratType();

        /// <summary>
        /// Gets the default value for STRAT_UNIT_TYPE
        /// </summary>
        string GetDefaultStratUnitType();

        /// <summary>
        /// Gets the default value for STRAT_STATUS
        /// </summary>
        string GetDefaultStratStatus();

        /// <summary>
        /// Gets the default value for AREA_TYPE
        /// </summary>
        string GetDefaultAreaType();

        /// <summary>
        /// Gets the default value for DEPTH_OUOM
        /// </summary>
        string GetDefaultDepthOuom();

        /// <summary>
        /// Gets the default value for PICK_DEPTH_OUOM
        /// </summary>
        string GetDefaultPickDepthOuom();

        /// <summary>
        /// Gets the default value for AZIMUTH_NORTH_TYPE
        /// </summary>
        string GetDefaultAzimuthNorthType();

        /// <summary>
        /// Gets the default value for CONFORMITY_RELATIONSHIP
        /// </summary>
        string GetDefaultConformityRelationship();

        /// <summary>
        /// Gets the default value for STRAT_INTERPRET_METHOD
        /// </summary>
        string GetDefaultStratInterpretMethod();

        /// <summary>
        /// Gets the default value for PICK_QUALITY
        /// </summary>
        string GetDefaultPickQuality();

        /// <summary>
        /// Gets the default value for PICK_VERSION_TYPE
        /// </summary>
        string GetDefaultPickVersionType();

        /// <summary>
        /// Gets the default value for TVD_METHOD
        /// </summary>
        string GetDefaultTvdMethod();

        /// <summary>
        /// Gets the default value for SOURCE
        /// </summary>
        string GetDefaultSource();

        /// <summary>
        /// Gets the default value for REMARK
        /// </summary>
        string GetDefaultRemark();

        // Well Structure XREF_TYPE values (default values for WELL_XREF table)

        /// <summary>
        /// Gets the XREF_TYPE value for Well Origin
        /// </summary>
        string GetWellOriginXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore
        /// </summary>
        string GetWellboreXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Segment
        /// </summary>
        string GetWellboreSegmentXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Contact Interval
        /// </summary>
        string GetWellboreContactIntervalXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellbore Completion
        /// </summary>
        string GetWellboreCompletionXrefType();

        /// <summary>
        /// Gets the XREF_TYPE value for Wellhead Stream
        /// </summary>
        string GetWellheadStreamXrefType();

        /// <summary>
        /// Gets all well structure XREF_TYPE values
        /// </summary>
        System.Collections.Generic.Dictionary<string, string> GetAllWellStructureXrefTypes();

        // ID Type Configuration for PPDM Tables

        /// <summary>
        /// Gets whether PPDM tables use string IDs (all PPDM tables use string IDs by default)
        /// </summary>
        bool UseStringIds();

        /// <summary>
        /// Gets the ID type for a specific table (defaults to string for all PPDM tables)
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>ID type name (e.g., "String", "Int32", "Guid")</returns>
        string GetIdTypeForTable(string tableName);

        /// <summary>
        /// Formats an ID value according to the table's ID type configuration
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="id">ID value</param>
        /// <returns>Formatted ID value (as string for PPDM tables)</returns>
        string FormatIdForTable(string tableName, object id);


        #region Database-Backed Default Values

        /// <summary>
        /// Gets a default value from database (user-specific if userId provided, otherwise system default)
        /// Falls back to hardcoded constant if not found in database
        /// </summary>
        /// <param name="key">Default value key (e.g., "ACTIVE_IND_YES", "JURISDICTION.US.CURRENCY")</param>
        /// <param name="databaseId">Database identifier (GUID)</param>
        /// <param name="userId">Optional user identifier for user-specific overrides</param>
        /// <returns>Default value from database, or null if not found (will use hardcoded fallback)</returns>
        Task<string?> GetDefaultValueAsync(string key, string databaseId, string? userId = null);

        /// <summary>
        /// Sets or updates a default value in the database
        /// </summary>
        /// <param name="key">Default value key</param>
        /// <param name="value">Default value</param>
        /// <param name="databaseId">Database identifier (GUID)</param>
        /// <param name="userId">Optional user identifier for user-specific overrides (null = system default)</param>
        /// <param name="category">Category grouping (e.g., "System", "Jurisdiction", "FieldMapping")</param>
        /// <param name="valueType">Data type hint (e.g., "String", "Decimal", "Boolean", "JSON")</param>
        /// <param name="description">Optional description</param>
        Task SetDefaultValueAsync(string key, string value, string databaseId, string? userId = null, string category = "System", string valueType = "String", string description = null);

        /// <summary>
        /// Gets all default values for a category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="databaseId">Database identifier (GUID)</param>
        /// <param name="userId">Optional user identifier</param>
        /// <returns>Dictionary of default keys to values</returns>
        Task<Dictionary<string, string>> GetDefaultsByCategoryAsync(string category, string databaseId, string? userId = null);

        /// <summary>
        /// Initializes system defaults for a database
        /// Seeds all hardcoded constants into the database
        /// </summary>
        /// <param name="databaseId">Database identifier (GUID)</param>
        /// <param name="userId">User identifier for audit trail (defaults to "SYSTEM")</param>
        Task InitializeSystemDefaultsAsync(string databaseId, string userId = "SYSTEM");

        #endregion
    }

    #region Field Mapping DTOs

    /// <summary>
    /// Configuration for mapping a logical property to a database table and field
    /// </summary>
    public class FieldMappingConfig
    {
        /// <summary>
        /// Mapping key (e.g., "ReservoirForecast.InitialPressure")
        /// </summary>
        public string MappingKey { get; set; } = string.Empty;

        /// <summary>
        /// Mapping category (e.g., "ReservoirForecast", "WellProperties")
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Table name to retrieve the value from (e.g., "POOL", "WELL")
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Field name in the table (e.g., "INITIAL_PRESSURE", "PRESSURE")
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// Additional conditions to apply when retrieving the value (optional)
        /// Dictionary of field names to values for filtering
        /// </summary>
        public Dictionary<string, object>? Conditions { get; set; }

        /// <summary>
        /// Default value if the field is not found or is null
        /// </summary>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// Unit conversion factor (multiply retrieved value by this factor)
        /// </summary>
        public decimal? ConversionFactor { get; set; }

        /// <summary>
        /// Whether this mapping is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Description of the mapping
        /// </summary>
        public string? Description { get; set; }
    }

    #endregion

    #region Jurisdiction Support DTOs

    /// <summary>
    /// Validation rule specific to a jurisdiction
    /// </summary>
    public class JurisdictionValidationRule
    {
        public string RuleId { get; set; } = string.Empty;
        public string JurisdictionId { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty; // Required, Range, Pattern, Custom, etc.
        public string? RuleValue { get; set; } // Value for the rule (e.g., min/max, pattern, etc.)
        public string ErrorMessage { get; set; } = string.Empty;
        public string Severity { get; set; } = "Error"; // Error, Warning, Info
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Result of jurisdiction-specific validation
    /// </summary>
    public class JurisdictionValidationResult
    {
        public bool IsValid { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public List<string> WarningMessages { get; set; } = new List<string>();
        public string? RuleId { get; set; }
    }

    #endregion
}
