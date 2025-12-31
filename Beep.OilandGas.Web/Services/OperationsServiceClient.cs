using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.DTOs.Lease;
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

        public async Task<ProspectEvaluationDto> EvaluateProspectAsync(string prospectId)
        {
            try
            {
                var result = await _apiClient.GetAsync<ProspectEvaluationDto>(
                    $"/api/prospectidentification/evaluate/{Uri.EscapeDataString(prospectId)}");
                return result ?? throw new InvalidOperationException($"Failed to evaluate prospect {prospectId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating prospect {ProspectId}", prospectId);
                throw;
            }
        }

        public async Task<List<ProspectDto>> GetProspectsAsync(Dictionary<string, string>? filters = null)
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
                var result = await _apiClient.GetAsync<List<ProspectDto>>(endpoint);
                return result ?? new List<ProspectDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prospects");
                return new List<ProspectDto>();
            }
        }

        public async Task<string> CreateProspectAsync(ProspectDto prospect, string? userId = null)
        {
            try
            {
                var endpoint = "/api/prospectidentification";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var response = await _apiClient.PostAsync<ProspectDto, dynamic>(endpoint, prospect);
                // Extract prospectId from response
                return response?.prospectId?.ToString() ?? throw new InvalidOperationException("Failed to create prospect");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prospect");
                throw;
            }
        }

        public async Task<List<ProspectRankingDto>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria)
        {
            try
            {
                var request = new
                {
                    ProspectIds = prospectIds,
                    RankingCriteria = rankingCriteria
                };
                var result = await _apiClient.PostAsync<object, List<ProspectRankingDto>>(
                    "/api/prospectidentification/rank", request);
                return result ?? new List<ProspectRankingDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ranking prospects");
                return new List<ProspectRankingDto>();
            }
        }

        #endregion

        #region Enhanced Recovery Operations

        public async Task<EnhancedRecoveryOperationDto> AnalyzeEORPotentialAsync(string fieldId, string eorMethod)
        {
            try
            {
                var request = new
                {
                    FieldId = fieldId,
                    EorMethod = eorMethod
                };
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperationDto>(
                    "/api/enhancedrecovery/analyze-eor", request);
                return result ?? throw new InvalidOperationException("Failed to analyze EOR potential");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR potential for field {FieldId}", fieldId);
                throw;
            }
        }

        public async Task<EnhancedRecoveryOperationDto> CalculateRecoveryFactorAsync(string projectId)
        {
            try
            {
                var request = new
                {
                    ProjectId = projectId
                };
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperationDto>(
                    "/api/enhancedrecovery/recovery-factor", request);
                return result ?? throw new InvalidOperationException("Failed to calculate recovery factor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recovery factor for project {ProjectId}", projectId);
                throw;
            }
        }

        public async Task<InjectionOperationDto> ManageInjectionAsync(string injectionWellId, decimal injectionRate)
        {
            try
            {
                var request = new
                {
                    InjectionWellId = injectionWellId,
                    InjectionRate = injectionRate
                };
                var result = await _apiClient.PostAsync<object, InjectionOperationDto>(
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

        public async Task<string> CreateLeaseAcquisitionAsync(CreateLeaseAcquisitionDto leaseRequest, string? userId = null)
        {
            try
            {
                var endpoint = "/api/leaseacquisition";
                if (!string.IsNullOrEmpty(userId))
                {
                    endpoint += $"?userId={Uri.EscapeDataString(userId)}";
                }
                var response = await _apiClient.PostAsync<CreateLeaseAcquisitionDto, dynamic>(endpoint, leaseRequest);
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

        #region Drilling Operation Operations

        public async Task<List<DrillingOperationDto>> GetDrillingOperationsAsync(string? wellUWI = null)
        {
            try
            {
                var endpoint = "/api/drillingoperation/operations";
                if (!string.IsNullOrEmpty(wellUWI))
                {
                    endpoint += $"?wellUWI={Uri.EscapeDataString(wellUWI)}";
                }
                var result = await _apiClient.GetAsync<List<DrillingOperationDto>>(endpoint);
                return result ?? new List<DrillingOperationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations");
                return new List<DrillingOperationDto>();
            }
        }

        public async Task<DrillingOperationDto?> GetDrillingOperationAsync(string operationId)
        {
            try
            {
                var result = await _apiClient.GetAsync<DrillingOperationDto>(
                    $"/api/drillingoperation/operations/{Uri.EscapeDataString(operationId)}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId}", operationId);
                return null;
            }
        }

        public async Task<DrillingOperationDto> CreateDrillingOperationAsync(CreateDrillingOperationDto createDto)
        {
            try
            {
                var result = await _apiClient.PostAsync<CreateDrillingOperationDto, DrillingOperationDto>(
                    "/api/drillingoperation/operations", createDto);
                return result ?? throw new InvalidOperationException("Failed to create drilling operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation");
                throw;
            }
        }

        public async Task<DrillingOperationDto> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperationDto updateDto)
        {
            try
            {
                var result = await _apiClient.PutAsync<UpdateDrillingOperationDto, DrillingOperationDto>(
                    $"/api/drillingoperation/operations/{Uri.EscapeDataString(operationId)}", updateDto);
                return result ?? throw new InvalidOperationException("Failed to update drilling operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId}", operationId);
                throw;
            }
        }

        public async Task<List<DrillingReportDto>> GetDrillingReportsAsync(string operationId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<DrillingReportDto>>(
                    $"/api/drillingoperation/operations/{Uri.EscapeDataString(operationId)}/reports");
                return result ?? new List<DrillingReportDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId}", operationId);
                return new List<DrillingReportDto>();
            }
        }

        public async Task<DrillingReportDto> CreateDrillingReportAsync(string operationId, CreateDrillingReportDto createDto)
        {
            try
            {
                var result = await _apiClient.PostAsync<CreateDrillingReportDto, DrillingReportDto>(
                    $"/api/drillingoperation/operations/{Uri.EscapeDataString(operationId)}/reports", createDto);
                return result ?? throw new InvalidOperationException("Failed to create drilling report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId}", operationId);
                throw;
            }
        }

        #endregion
    }
}

