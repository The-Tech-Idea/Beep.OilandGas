using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    public class DecommissioningServiceClient : IDecommissioningServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DecommissioningServiceClient> _logger;

        public DecommissioningServiceClient(
            ApiClient apiClient,
            ILogger<DecommissioningServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<WellAbandonmentResponse>> GetAbandonedWellsAsync(Dictionary<string, string>? filters = null)
        {
            try
            {
                var endpoint = BuildEndpoint("/api/field/current/decommissioning/wells-abandoned", filters);
                var result = await _apiClient.GetAsync<List<WellAbandonmentResponse>>(endpoint);
                return result ?? new List<WellAbandonmentResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting abandoned wells for current field");
                return new List<WellAbandonmentResponse>();
            }
        }

        public async Task<WellAbandonmentResponse?> GetWellAbandonmentAsync(string abandonmentId)
        {
            try
            {
                return await _apiClient.GetAsync<WellAbandonmentResponse>(
                    $"/api/field/current/decommissioning/wells-abandoned/{Uri.EscapeDataString(abandonmentId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well abandonment {AbandonmentId}", abandonmentId);
                return null;
            }
        }

        public async Task<WellAbandonmentResponse?> AbandonWellAsync(string wellId, WellAbandonmentRequest request, string userId)
        {
            try
            {
                return await _apiClient.PostAsync<WellAbandonmentRequest, WellAbandonmentResponse>(
                    $"/api/field/current/decommissioning/abandon-well?wellId={Uri.EscapeDataString(wellId)}&userId={Uri.EscapeDataString(userId)}",
                    request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording abandonment for well {WellId}", wellId);
                return null;
            }
        }

        public async Task<bool> ApprovePAAsync(string abandonmentId)
        {
            try
            {
                return await _apiClient.PatchAsync(
                    $"/api/field/current/decommissioning/wells-abandoned/{Uri.EscapeDataString(abandonmentId)}/approve",
                    new { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving P&A programme {AbandonmentId}", abandonmentId);
                return false;
            }
        }

        public async Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesAsync(Dictionary<string, string>? filters = null)
        {
            try
            {
                var endpoint = BuildEndpoint("/api/field/current/decommissioning/facilities", filters);
                var result = await _apiClient.GetAsync<List<FacilityDecommissioningResponse>>(endpoint);
                return result ?? new List<FacilityDecommissioningResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting decommissioned facilities for current field");
                return new List<FacilityDecommissioningResponse>();
            }
        }

        public async Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningAsync(string decommissioningId)
        {
            try
            {
                return await _apiClient.GetAsync<FacilityDecommissioningResponse>(
                    $"/api/field/current/decommissioning/facilities/{Uri.EscapeDataString(decommissioningId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting facility decommissioning {DecommissioningId}", decommissioningId);
                return null;
            }
        }

        public async Task<DecommissioningCostEstimateResponse?> EstimateCostsAsync()
        {
            try
            {
                return await _apiClient.PostAsync<object, DecommissioningCostEstimateResponse>(
                    "/api/field/current/decommissioning/cost-estimation", new { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating decommissioning costs for current field");
                return null;
            }
        }

        private static string BuildEndpoint(string endpoint, Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0)
            {
                return endpoint;
            }

            var queryParams = new List<string>();
            foreach (var filter in filters)
            {
                queryParams.Add($"{Uri.EscapeDataString(filter.Key)}={Uri.EscapeDataString(filter.Value)}");
            }

            return endpoint + "?" + string.Join("&", queryParams);
        }
    }
}