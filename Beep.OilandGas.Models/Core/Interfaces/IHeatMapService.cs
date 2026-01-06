using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.HeatMap;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for heat map generation and management.
    /// Provides heat map creation, persistence, and integration with field data.
    /// </summary>
    public interface IHeatMapService
    {
        /// <summary>
        /// Generates a heat map from data points.
        /// </summary>
        /// <param name="dataPoints">Data points for heat map</param>
        /// <param name="configuration">Heat map configuration</param>
        /// <returns>Generated heat map</returns>
        Task<HeatMapResultDto> GenerateHeatMapAsync(List<HeatMapDataPoint> dataPoints, HeatMapConfigurationDto configuration);

        /// <summary>
        /// Saves heat map configuration to database.
        /// </summary>
        /// <param name="configuration">Heat map configuration</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Heat map identifier</returns>
        Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationDto configuration, string userId);

        /// <summary>
        /// Gets heat map configuration from database.
        /// </summary>
        /// <param name="heatMapId">Heat map identifier</param>
        /// <returns>Heat map configuration</returns>
        Task<HeatMapConfigurationDto?> GetHeatMapConfigurationAsync(string heatMapId);

        /// <summary>
        /// Generates heat map from field production data.
        /// </summary>
        /// <param name="fieldId">Field identifier</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Generated heat map</returns>
        Task<HeatMapResultDto> GenerateProductionHeatMapAsync(string fieldId, System.DateTime startDate, System.DateTime endDate);
    }
}




