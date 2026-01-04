using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Repositories
{
    /// <summary>
    /// Service for managing field mapping configurations
    /// Used to configure which table and field to use for retrieving values (e.g., reservoir properties for forecasting)
    /// </summary>
    public interface IFieldMappingService
    {
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
    }
}
