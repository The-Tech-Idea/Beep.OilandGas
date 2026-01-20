using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client service for pump operations.
    /// </summary>
    public class PumpServiceClient : IPumpServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<PumpServiceClient> _logger;

        public PumpServiceClient(
            ApiClient apiClient,
            ILogger<PumpServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Hydraulic Pump Operations

        public async Task<HydraulicPumpDesign> DesignHydraulicPumpSystemAsync(string wellUWI, string pumpType, decimal wellDepth, decimal desiredFlowRate)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    PumpType = pumpType,
                    WellDepth = wellDepth,
                    DesiredFlowRate = desiredFlowRate
                };
                var result = await _apiClient.PostAsync<object, HydraulicPumpDesign>(
                    "/api/hydraulicpump/design", request);
                return result ?? throw new InvalidOperationException("Failed to design hydraulic pump system");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing hydraulic pump system for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<PumpPerformanceAnalysis> AnalyzeHydraulicPumpPerformanceAsync(string pumpId)
        {
            try
            {
                var request = new { PumpId = pumpId };
                var result = await _apiClient.PostAsync<object, PumpPerformanceAnalysis>(
                    "/api/hydraulicpump/analyze-performance", request);
                return result ?? throw new InvalidOperationException("Failed to analyze hydraulic pump performance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing hydraulic pump performance for pump {PumpId}", pumpId);
                throw;
            }
        }

        public async Task<bool> SaveHydraulicPumpDesignAsync(HydraulicPumpDesign design, string? userId = null)
        {
            try
            {
                var endpoint = "/api/hydraulicpump/design/save";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, design);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving hydraulic pump design");
                return false;
            }
        }

        public async Task<List<PumpPerformanceHistory>> GetHydraulicPumpPerformanceHistoryAsync(string pumpId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<PumpPerformanceHistory>>(
                    $"/api/hydraulicpump/performance-history/{Uri.EscapeDataString(pumpId)}");
                return result ?? new List<PumpPerformanceHistory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hydraulic pump performance history for pump {PumpId}", pumpId);
                return new List<PumpPerformanceHistory>();
            }
        }

        #endregion

        #region Plunger Lift Operations

        public async Task<PlungerLiftDesign> DesignPlungerLiftSystemAsync(string wellUWI, PlungerLiftWellProperties wellProperties)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    WellProperties = wellProperties
                };
                var result = await _apiClient.PostAsync<object, PlungerLiftDesign>(
                    "/api/plungerlift/design", request);
                return result ?? throw new InvalidOperationException("Failed to design plunger lift system");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing plunger lift system for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<PlungerLiftPerformance> AnalyzePlungerLiftPerformanceAsync(string wellUWI)
        {
            try
            {
                var request = new { WellUWI = wellUWI };
                var result = await _apiClient.PostAsync<object, PlungerLiftPerformance>(
                    "/api/plungerlift/analyze-performance", request);
                return result ?? throw new InvalidOperationException("Failed to analyze plunger lift performance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing plunger lift performance for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<bool> SavePlungerLiftDesignAsync(PlungerLiftDesign design, string? userId = null)
        {
            try
            {
                var endpoint = "/api/plungerlift/design/save";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, design);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving plunger lift design");
                return false;
            }
        }

        #endregion

        #region Sucker Rod Pumping Operations

        public async Task<SuckerRodPumpDesign> DesignSuckerRodPumpSystemAsync(string wellUWI, SuckerRodPumpWellProperties wellProperties)
        {
            try
            {
                var request = new
                {
                    WellUWI = wellUWI,
                    WellProperties = wellProperties
                };
                var result = await _apiClient.PostAsync<object, SuckerRodPumpDesign>(
                    "/api/suckerrodpumping/design", request);
                return result ?? throw new InvalidOperationException("Failed to design sucker rod pump system");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error designing sucker rod pump system for well {WellUWI}", wellUWI);
                throw;
            }
        }

        public async Task<SuckerRodPumpPerformance> AnalyzeSuckerRodPumpPerformanceAsync(string pumpId)
        {
            try
            {
                var request = new { PumpId = pumpId };
                var result = await _apiClient.PostAsync<object, SuckerRodPumpPerformance>(
                    "/api/suckerrodpumping/analyze-performance", request);
                return result ?? throw new InvalidOperationException("Failed to analyze sucker rod pump performance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sucker rod pump performance for pump {PumpId}", pumpId);
                throw;
            }
        }

        public async Task<bool> SaveSuckerRodPumpDesignAsync(SuckerRodPumpDesign design, string? userId = null)
        {
            try
            {
                var endpoint = "/api/suckerrodpumping/design/save";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PostAsync(endpoint, design);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving sucker rod pump design");
                return false;
            }
        }

        #endregion
    }
}

