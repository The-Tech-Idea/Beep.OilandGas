using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Drilling;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Client service for drilling and construction operations.
    /// </summary>
    public class DrillingServiceClient : IDrillingServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<DrillingServiceClient> _logger;

        public DrillingServiceClient(
            ApiClient apiClient,
            ILogger<DrillingServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DRILLING_OPERATION>> GetDrillingOperationsAsync(string? wellUWI = null)
        {
            try
            {
                var endpoint = "/api/field/current/drilling/operations";
                if (!string.IsNullOrEmpty(wellUWI))
                {
                    endpoint += $"?wellUWI={Uri.EscapeDataString(wellUWI)}";
                }

                var result = await _apiClient.GetAsync<List<DRILLING_OPERATION>>(endpoint);
                return result ?? new List<DRILLING_OPERATION>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operations");
                return new List<DRILLING_OPERATION>();
            }
        }

        public async Task<DRILLING_OPERATION?> GetDrillingOperationAsync(string operationId)
        {
            try
            {
                return await _apiClient.GetAsync<DRILLING_OPERATION>(
                    $"/api/field/current/drilling/operations/{Uri.EscapeDataString(operationId)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling operation {OperationId}", operationId);
                return null;
            }
        }

        public async Task<DRILLING_OPERATION> CreateDrillingOperationAsync(CREATE_DRILLING_OPERATION createDto)
        {
            try
            {
                var result = await _apiClient.PostAsync<CREATE_DRILLING_OPERATION, DRILLING_OPERATION>(
                    "/api/field/current/drilling/operations", createDto);
                return result ?? throw new InvalidOperationException("Failed to create drilling operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling operation");
                throw;
            }
        }

        public async Task<DRILLING_OPERATION> UpdateDrillingOperationAsync(string operationId, UpdateDrillingOperation updateDto)
        {
            try
            {
                var result = await _apiClient.PutAsync<UpdateDrillingOperation, DRILLING_OPERATION>(
                    $"/api/field/current/drilling/operations/{Uri.EscapeDataString(operationId)}", updateDto);
                return result ?? throw new InvalidOperationException("Failed to update drilling operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating drilling operation {OperationId}", operationId);
                throw;
            }
        }

        public async Task<List<DRILLING_REPORT>> GetDrillingReportsAsync(string operationId)
        {
            try
            {
                var result = await _apiClient.GetAsync<List<DRILLING_REPORT>>(
                    $"/api/field/current/drilling/operations/{Uri.EscapeDataString(operationId)}/reports");
                return result ?? new List<DRILLING_REPORT>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drilling reports for operation {OperationId}", operationId);
                return new List<DRILLING_REPORT>();
            }
        }

        public async Task<DRILLING_REPORT> CreateDrillingReportAsync(string operationId, CreateDrillingReport createDto)
        {
            try
            {
                var result = await _apiClient.PostAsync<CreateDrillingReport, DRILLING_REPORT>(
                    $"/api/field/current/drilling/operations/{Uri.EscapeDataString(operationId)}/reports", createDto);
                return result ?? throw new InvalidOperationException("Failed to create drilling report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drilling report for operation {OperationId}", operationId);
                throw;
            }
        }
    }
}