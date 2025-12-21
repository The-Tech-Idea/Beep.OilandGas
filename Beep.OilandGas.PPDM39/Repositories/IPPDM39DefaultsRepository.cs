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

        // Well Status Facets (default values for well status decomposition)

        /// <summary>
        /// Gets well status facets for a given STATUS_ID
        /// Uses R_WELL_STATUS_XREF to decompose complex status values into atomic facets
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> GetWellStatusFacetsAsync(string statusId);

        /// <summary>
        /// Gets well status facets for a given STATUS_TYPE and STATUS
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, object>> GetWellStatusFacetsByTypeAndStatusAsync(string statusType, string status);

        /// <summary>
        /// Gets all well status facets grouped by STATUS_ID
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>> GetAllWellStatusFacetsAsync();

        /// <summary>
        /// Gets the well status XREF table name
        /// </summary>
        string GetWellStatusXrefTableName();

        // Well Status TYPE defaults (from R_WELL_STATUS table)

        /// <summary>
        /// Gets all STATUS_TYPE values from R_WELL_STATUS table
        /// These are the default status types available for wells (e.g., Role, Business Life Cycle Phase, etc.)
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.List<string>> GetAllWellStatusTypesAsync();

        /// <summary>
        /// Gets all STATUS_TYPE values grouped by their purpose/category
        /// </summary>
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>> GetWellStatusTypesGroupedAsync();

        /// <summary>
        /// Gets the list of default STATUS_TYPEs that should be initialized for every new well or wellbore
        /// These are the standard well facets from PPDM 3.9
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypes();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wells (not wellbores)
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWell();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellbores
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWellbore();

        /// <summary>
        /// Gets the default STATUS_TYPEs that should be initialized for wellhead streams
        /// </summary>
        System.Collections.Generic.List<string> GetDefaultWellStatusTypesForWellheadStream();

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

        #region Jurisdiction-Specific Support

        /// <summary>
        /// Gets the default currency code for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier (e.g., country code, state/province code)</param>
        /// <returns>Currency code (e.g., "USD", "CAD", "GBP")</returns>
        string GetCurrencyForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the default currency symbol for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Currency symbol (e.g., "$", "£", "€")</returns>
        string GetCurrencySymbolForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the default volume units for a jurisdiction (for production volumes)
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Dictionary with unit types and their default values (e.g., {"OIL": "BBL", "GAS": "MSCF", "WATER": "BBL"})</returns>
        Dictionary<string, string> GetVolumeUnitsForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the default depth/distance units for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Default depth/distance unit (e.g., "FT", "M")</returns>
        string GetDepthUnitForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the default temperature unit for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Default temperature unit (e.g., "F", "C")</returns>
        string GetTemperatureUnitForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the default pressure unit for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Default pressure unit (e.g., "PSI", "KPA", "BAR")</returns>
        string GetPressureUnitForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets all default units for a jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <returns>Dictionary mapping unit types to their default values</returns>
        Dictionary<string, string> GetAllUnitsForJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets validation rules for a specific jurisdiction
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <param name="entityType">Entity type or table name to get validation rules for (optional)</param>
        /// <returns>List of validation rules specific to the jurisdiction</returns>
        Task<List<JurisdictionValidationRule>> GetValidationRulesForJurisdictionAsync(string jurisdictionId, string? entityType = null);

        /// <summary>
        /// Validates a value against jurisdiction-specific validation rules
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <param name="fieldName">Field name to validate</param>
        /// <param name="value">Value to validate</param>
        /// <param name="entityType">Entity type or table name (optional)</param>
        /// <returns>Validation result with success status and any error messages</returns>
        Task<JurisdictionValidationResult> ValidateValueForJurisdictionAsync(string jurisdictionId, string fieldName, object value, string? entityType = null);

        /// <summary>
        /// Gets jurisdiction-specific default values for a table/entity type
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        /// <param name="tableName">Table name</param>
        /// <returns>Dictionary of field names to default values</returns>
        Task<Dictionary<string, object>> GetJurisdictionDefaultsForTableAsync(string jurisdictionId, string tableName);

        /// <summary>
        /// Sets the current jurisdiction context (affects all subsequent default value calls)
        /// </summary>
        /// <param name="jurisdictionId">Jurisdiction identifier</param>
        void SetCurrentJurisdiction(string jurisdictionId);

        /// <summary>
        /// Gets the current jurisdiction context
        /// </summary>
        /// <returns>Current jurisdiction identifier, or null if not set</returns>
        string? GetCurrentJurisdiction();

        /// <summary>
        /// Gets all available jurisdictions
        /// </summary>
        /// <returns>List of jurisdiction identifiers</returns>
        Task<List<string>> GetAvailableJurisdictionsAsync();

        /// <summary>
        /// Loads jurisdictions from BUSINESS_ASSOCIATE table based on JURISDICTION, COUNTRY_CODE, or COUNTRY fields
        /// Merges with existing default jurisdictions, with database values taking precedence
        /// This allows jurisdictions to be dynamically loaded from your PPDM database
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        Task LoadJurisdictionsFromBusinessAssociateAsync();

        #endregion

        #region Field Mapping Configuration

        /// <summary>
        /// Gets field mapping configuration for a specific property mapping key
        /// Used to configure which table and field to use for retrieving values (e.g., reservoir properties for forecasting)
        /// </summary>
        /// <param name="mappingKey">Mapping key (e.g., "ReservoirForecast.InitialPressure", "ReservoirForecast.Permeability")</param>
        /// <returns>Field mapping configuration, or null if not configured</returns>
        Task<FieldMappingConfig?> GetFieldMappingAsync(string mappingKey);

        /// <summary>
        /// Gets all field mappings for a specific mapping category
        /// </summary>
        /// <param name="category">Mapping category (e.g., "ReservoirForecast", "WellProperties")</param>
        /// <returns>Dictionary of mapping keys to field mapping configurations</returns>
        Task<Dictionary<string, FieldMappingConfig>> GetFieldMappingsByCategoryAsync(string category);

        /// <summary>
        /// Sets or updates a field mapping configuration
        /// </summary>
        /// <param name="mappingKey">Mapping key</param>
        /// <param name="config">Field mapping configuration</param>
        Task SetFieldMappingAsync(string mappingKey, FieldMappingConfig config);

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
