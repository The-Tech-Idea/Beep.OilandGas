using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services
{
    public class DecommissioningServiceClient : IDecommissioningServiceClient
    {
        private readonly ApiClient _apiClient;

        public DecommissioningServiceClient(ApiClient apiClient)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<List<WellAbandonmentResponse>> GetAbandonedWellsAsync(Dictionary<string, string>? filters = null)
        {
            var endpoint = BuildEndpoint("/api/field/current/decommissioning/wells-abandoned", filters);
            var result = await _apiClient.GetAsync<List<WellAbandonmentResponse>>(endpoint);
            return result ?? new List<WellAbandonmentResponse>();
        }

        public async Task<WellAbandonmentResponse?> GetWellAbandonmentAsync(string abandonmentId)
        {
            return await _apiClient.GetAsync<WellAbandonmentResponse>(
                $"/api/field/current/decommissioning/wells-abandoned/{Uri.EscapeDataString(abandonmentId)}");
        }

        public async Task<WellAbandonmentResponse?> AbandonWellAsync(string wellId, WellAbandonmentRequest request)
        {
            return await _apiClient.PostAsync<WellAbandonmentRequest, WellAbandonmentResponse>(
                $"/api/field/current/decommissioning/abandon-well?wellId={Uri.EscapeDataString(wellId)}",
                request);
        }

        public async Task ApprovePAAsync(string abandonmentId)
        {
            await _apiClient.PatchAsync(
                $"/api/field/current/decommissioning/wells-abandoned/{Uri.EscapeDataString(abandonmentId)}/approve",
                new { });
        }

        public async Task<List<FacilityDecommissioningResponse>> GetDecommissionedFacilitiesAsync(Dictionary<string, string>? filters = null)
        {
            var endpoint = BuildEndpoint("/api/field/current/decommissioning/facilities", filters);
            var result = await _apiClient.GetAsync<List<FacilityDecommissioningResponse>>(endpoint);
            return result ?? new List<FacilityDecommissioningResponse>();
        }

        public async Task<FacilityDecommissioningResponse?> GetFacilityDecommissioningAsync(string decommissioningId)
        {
            return await _apiClient.GetAsync<FacilityDecommissioningResponse>(
                $"/api/field/current/decommissioning/facilities/{Uri.EscapeDataString(decommissioningId)}");
        }

        public async Task<DecommissioningCostEstimateResponse?> EstimateCostsAsync()
        {
            return await _apiClient.PostAsync<object, DecommissioningCostEstimateResponse>(
                "/api/field/current/decommissioning/cost-estimation", new { });
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