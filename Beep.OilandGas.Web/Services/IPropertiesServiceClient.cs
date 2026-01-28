using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.HeatMap;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for properties operations (HeatMap).
    /// </summary>
    public interface IPropertiesServiceClient
    {
        // Heat Map Operations
        Task<HeatMapResult> GenerateHeatMapAsync(List<HEAT_MAP_DATA_POINT> dataPoints, HEAT_MAP_CONFIGURATION configuration);
        Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationRecord configuration, string? userId = null);
        Task<HeatMapConfigurationRecord?> GetHeatMapConfigurationAsync(string heatMapId);
        Task<HeatMapResult> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate);
    }
}

