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
        private readonly ILogger<OperationsServiceClient> _logger;

        public OperationsServiceClient(
            ApiClient apiClient,
            ILogger<OperationsServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects");
                return new List<Prospect>();
            }
        }

        public async Task<string> CreateProspectAsync(Prospect prospect, string? userId = null)
        {
            try
            {
                var endpoint = "/api/prospectidentification";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ranking prospects");
                return new List<ProspectRanking>();
            }
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
            try
            {
                var endpoint = "/api/leaseacquisition/available";
                if (filters != null && filters.Count > 0)
                {
                    var queryParams = new List<string>();
                    foreach (var filter in filters)
                    {
                        queryParams.Add($"{Uri.EscapeDataString(filter.Key)}={Uri.EscapeDataString(filter.Value)}");
                    }
                    endpoint += "?" + string.Join("&", queryParams);
                }
                var result = await _apiClient.GetAsync<List<LeaseSummary>>(endpoint);
                return result ?? new List<LeaseSummary>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available leases");
                return new List<LeaseSummary>();
            }
        }

        public async Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition leaseRequest, string? userId = null)
        {
            try
            {
                var endpoint = "/api/leaseacquisition";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var response = await _apiClient.PostAsync<CreateLeaseAcquisition, dynamic>(endpoint, leaseRequest);
                return response?.leaseId?.ToString() ?? throw new InvalidOperationException("Failed to create lease acquisition");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease acquisition");
                throw;
            }
        }

        public async Task<bool> UpdateLeaseStatusAsync(string leaseId, string status, string? userId = null)
        {
            try
            {
                var request = new { Status = status };
                var endpoint = $"/api/leaseacquisition/{Uri.EscapeDataString(leaseId)}/status";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                return await _apiClient.PutAsync(endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lease status for {LeaseId}", leaseId);
                return false;
            }
        }

        #endregion
    }
}

