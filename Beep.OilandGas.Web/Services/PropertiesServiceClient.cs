using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.HeatMap;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client service for properties operations.
    /// </summary>
    public class PropertiesServiceClient : IPropertiesServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<PropertiesServiceClient> _logger;

        public PropertiesServiceClient(
            ApiClient apiClient,
            ILogger<PropertiesServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Heat Map Operations

        public async Task<HeatMapResult> GenerateHeatMapAsync(List<HeatMapDataPoint> dataPoints, HeatMapConfiguration configuration)
        {
            try
            {
                var request = new
                {
                    DataPoints = dataPoints,
                    Configuration = configuration
                };
                var result = await _apiClient.PostAsync<object, HeatMapResult>(
                    "/api/heatmap/generate", request);
                return result ?? throw new InvalidOperationException("Failed to generate heat map");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating heat map");
                throw;
            }
        }

        public async Task<string> SaveHeatMapConfigurationAsync(HeatMapConfigurationRecord configuration, string? userId = null)
        {
            try
            {
                var endpoint = "/api/heatmap/configuration";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var response = await _apiClient.PostAsync<HeatMapConfigurationRecord, dynamic>(endpoint, configuration);
                return response?.heatMapId?.ToString() ?? throw new InvalidOperationException("Failed to save heat map configuration");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving heat map configuration");
                throw;
            }
        }

        public async Task<HeatMapConfigurationRecord?> GetHeatMapConfigurationAsync(string heatMapId)
        {
            try
            {
                var result = await _apiClient.GetAsync<HeatMapConfigurationRecord>(
                    $"/api/heatmap/configuration/{Uri.EscapeDataString(heatMapId)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting heat map configuration {HeatMapId}", heatMapId);
                return null;
            }
        }

        public async Task<HeatMapResult> GenerateProductionHeatMapAsync(string fieldId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var request = new
                {
                    FieldId = fieldId,
                    StartDate = startDate,
                    EndDate = endDate
                };
                var result = await _apiClient.PostAsync<object, HeatMapResult>(
                    "/api/heatmap/production", request);
                return result ?? throw new InvalidOperationException("Failed to generate production heat map");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating production heat map for field {FieldId}", fieldId);
                throw;
            }
        }

        #endregion
    }
}

