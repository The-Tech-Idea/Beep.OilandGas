using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HeatMap;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    public interface IHeatMapService
    {
        Task<HeatMapResult> GenerateHeatMapAsync(List<HEAT_MAP_DATA_POINT> dataPoints, HeatMapConfigurationRecord configuration);
        Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationRecord configuration, string userId);
        Task<HeatMapConfigurationRecord?> GetHeatMapConfigurationAsync(string heatMapId);
        Task<HeatMapResult> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate);
        Task<ThermalAnalysisResult> AnalyzeThermalPatternAsync(string locationId, List<HEAT_MAP_DATA_POINT> dataPoints);
        Task<List<ThermalAnomaly>> DetectThermalAnomaliesAsync(string locationId, List<HEAT_MAP_DATA_POINT> dataPoints, decimal stdDevThreshold = 2.0m);
        Task<ThermalTrendAnalysis> AnalyzeTemperatureTrendAsync(string locationId, List<decimal> historicalTemperatures, int forecastMonths = 6);
        Task<TemperatureGradientAnalysis> AnalyzeTemperatureGradientAsync(string locationId, List<HEAT_MAP_DATA_POINT> dataPoints);
        Task<List<TemperatureZone>> IdentifyTemperatureZonesAsync(string locationId, List<HEAT_MAP_DATA_POINT> dataPoints);
        Task<ThermalImageQuality> AssessThermalImageQualityAsync(string imageId, List<HEAT_MAP_DATA_POINT> dataPoints);
        Task<ThermalComparisonResult> CompareThermalDataAsync(string locationId, List<HEAT_MAP_DATA_POINT> baselineData, List<HEAT_MAP_DATA_POINT> currentData);
    }
}
