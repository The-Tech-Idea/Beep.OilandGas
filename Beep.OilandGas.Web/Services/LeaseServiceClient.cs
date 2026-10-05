using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Lease;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    public class LeaseServiceClient : ILeaseServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LeaseServiceClient> _logger;

        public LeaseServiceClient(ApiClient apiClient, ILogger<LeaseServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LeaseSummary> EvaluateLeaseAsync(string leaseId)
        {
            try
            {
                var result = await _apiClient.GetAsync<LeaseSummary>(
                    $"/api/leaseacquisition/evaluate/{Uri.EscapeDataString(leaseId)}");
                return result ?? throw new InvalidOperationException($"Failed to evaluate lease {leaseId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating lease {LeaseId}", leaseId);
                throw;
            }
        }

        public async Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null)
        {
            var endpoint = BuildEndpoint("/api/leaseacquisition/available", filters);
            var result = await _apiClient.GetAsync<List<LeaseSummary>>(endpoint);
            return result ?? new List<LeaseSummary>();
        }

        public async Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest)
        {
            try
            {
                var endpoint = "/api/leaseacquisition";
                var response = await _apiClient.PostAsync<CreateLeaseAcquisition, dynamic>(endpoint, leaseRequest);
                return response?.leaseId?.ToString() ?? throw new InvalidOperationException("Failed to create lease acquisition");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease acquisition");
                throw;
            }
        }

        public async Task UpdateLeaseStatusAsync(string leaseId, string status)
        {
            var endpoint = $"/api/leaseacquisition/{Uri.EscapeDataString(leaseId)}/status";
            await _apiClient.PutAsync(endpoint, new { Status = status });
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