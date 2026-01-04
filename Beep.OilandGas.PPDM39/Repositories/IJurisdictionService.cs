using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Repositories
{
    /// <summary>
    /// Service for managing jurisdiction-specific configurations
    /// Handles currency, units, validation rules, and defaults per jurisdiction
    /// </summary>
    public interface IJurisdictionService
    {
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

        /// <summary>
        /// Serializes a JurisdictionConfig to key-value pairs in the database
        /// </summary>
        Task SaveJurisdictionConfigAsync(JurisdictionConfig config, string databaseId, string? userId = null);

        /// <summary>
        /// Deserializes a JurisdictionConfig from the database
        /// </summary>
        Task<JurisdictionConfig?> LoadJurisdictionConfigAsync(string jurisdictionId, string databaseId, string? userId = null);
    }

    /// <summary>
    /// Configuration class for jurisdiction settings
    /// Moved here to avoid circular dependencies
    /// </summary>
    public class JurisdictionConfig
    {
        public string JurisdictionId { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public string CurrencySymbol { get; set; } = "$";
        public string DepthUnit { get; set; } = "FT";
        public string TemperatureUnit { get; set; } = "F";
        public string PressureUnit { get; set; } = "PSI";
        public Dictionary<string, string> VolumeUnits { get; set; } = new Dictionary<string, string>();
    }
}
