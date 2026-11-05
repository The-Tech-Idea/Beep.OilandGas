using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client service for operations.
    /// </summary>
    public class OperationsServiceClient : IOperationsServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILeaseServiceClient _leases;
        private readonly ILogger<OperationsServiceClient> _logger;

        public OperationsServiceClient(
            ApiClient apiClient,
            ILogger<OperationsServiceClient> logger, ILeaseServiceClient leases)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _leases = leases ?? throw new ArgumentNullException(nameof(leases));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Prospect Identification Operations

        public async Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId)
        {
            try
            {
                var result = await _apiClient.GetAsync<ProspectEvaluation>(
                    $"/api/prospectidentification/evaluate/{Uri.EscapeDataString(prospectId)}");
                return result ?? throw new InvalidOperationException($"Failed to evaluate prospect {prospectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect {ProspectId}", prospectId);
                throw;
            }
        }

        public async Task<List<Prospect>> GetProspectsAsync(Dictionary<string, string>? filters = null)
        {
            var endpoint = "/api/prospectidentification";
            if (filters != null && filters.Count > 0)
            {
                var queryParams = new List<string>();
                foreach (var filter in filters)
                {
                    queryParams.Add($"{Uri.EscapeDataString(filter.Key)}={Uri.EscapeDataString(filter.Value)}");
                }
                endpoint += "?" + string.Join("&", queryParams);
            }
            var result = await _apiClient.GetAsync<List<Prospect>>(endpoint);
            return result ?? new List<Prospect>();
        }

        public async Task<string> CreateProspectAsync(Prospect prospect)
        {
            try
            {
                var endpoint = "/api/prospectidentification";
                var response = await _apiClient.PostAsync<Prospect, dynamic>(endpoint, prospect);
                // Extract prospectId from response
                return response?.prospectId?.ToString() ?? throw new InvalidOperationException("Failed to create prospect");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect");
                throw;
            }
        }

        public async Task<List<ProspectRanking>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria)
        {
            var request = new
            {
                ProspectIds = prospectIds,
                RankingCriteria = rankingCriteria
            };
            var result = await _apiClient.PostAsync<object, List<ProspectRanking>>(
                "/api/prospectidentification/rank", request);
            return result ?? new List<ProspectRanking>();
        }

        #endregion

        #region Enhanced Recovery Operations

        public async Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod)
        {
            try
            {
                var request = new
                {
                    FieldId = fieldId,
                    EorMethod = eorMethod
                };
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperation>(
                    "/api/enhancedrecovery/analyze-eor", request);
                return result ?? throw new InvalidOperationException("Failed to analyze EOR potential");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR potential for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string operationId)
        {
            try
            {
                var request = new
                {
                    OperationId = operationId
                };
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperation>(
                    "/api/enhancedrecovery/recovery-factor", request);
                return result ?? throw new InvalidOperationException("Failed to calculate recovery factor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recovery factor for operation {OperationId}", operationId);
                throw;
            }
        }

        public async Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate)
        {
            try
            {
                var request = new
                {
                    InjectionWellId = injectionWellId,
                    InjectionRate = injectionRate
                };
                var result = await _apiClient.PostAsync<object, InjectionOperation>(
                    "/api/enhancedrecovery/injection", request);
                return result ?? throw new InvalidOperationException("Failed to manage injection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing injection for well {InjectionWellId}", injectionWellId);
                throw;
            }
        }

        #endregion

        #region Lease Acquisition Operations

        public Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId) => _leases.EvaluateLeaseAsync(leaseId, fieldId);
        public Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null) => _leases.GetAvailableLeasesAsync(filters);
        public Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition request) => _leases.CreateLeaseAcquisitionAsync(request);
        public Task UpdateLeaseStatusAsync(string leaseId, string status, string fieldId) => _leases.UpdateLeaseStatusAsync(leaseId, status, fieldId);
        #endregion
    }
}

