using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.HeatMap;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Service interface for properties operations (HeatMap).
    /// </summary>
    public interface IPropertiesServiceClient
    {
        // Heat Map Operations
        Task<HeatMapResultDto> GenerateHeatMapAsync(List<HeatMapDataPoint> dataPoints, HeatMapConfiguration configuration);
        Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationDto configuration, string? userId = null);
        Task<HeatMapConfigurationDto?> GetHeatMapConfigurationAsync(string heatMapId);
        Task<HeatMapResultDto> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate);
    }
}

