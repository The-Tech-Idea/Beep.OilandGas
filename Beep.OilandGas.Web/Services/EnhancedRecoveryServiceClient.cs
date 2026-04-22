using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services
{
    public class EnhancedRecoveryServiceClient : IEnhancedRecoveryServiceClient
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<EnhancedRecoveryServiceClient> _logger;

        public EnhancedRecoveryServiceClient(ApiClient apiClient, ILogger<EnhancedRecoveryServiceClient> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod)
        {
            try
            {
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperation>(
                    "/api/enhancedrecovery/analyze-eor",
                    new { FieldId = fieldId, EorMethod = eorMethod });
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
                var result = await _apiClient.PostAsync<object, EnhancedRecoveryOperation>(
                    "/api/enhancedrecovery/recovery-factor",
                    new { OperationId = operationId });
                return result ?? throw new InvalidOperationException("Failed to calculate recovery factor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating recovery factor for operation {OperationId}", operationId);
                throw;
            }
        }

        public async Task<EOREconomicAnalysis> AnalyzeEconomicsAsync(AnalyzeEOREconomicsRequest request)
        {
            try
            {
                var result = await _apiClient.PostAsync<AnalyzeEOREconomicsRequest, EOREconomicAnalysis>(
                    "/api/enhancedrecovery/economics",
                    request);
                return result ?? throw new InvalidOperationException("Failed to analyze EOR economics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing EOR economics for field {FieldId}", request.FieldId);
                throw;
            }
        }

        public async Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null)
        {
            try
            {
                var endpoint = string.IsNullOrWhiteSpace(wellUWI)
                    ? "/api/enhancedrecovery/injection"
                    : $"/api/enhancedrecovery/injection?wellUWI={Uri.EscapeDataString(wellUWI)}";

                var result = await _apiClient.GetAsync<List<InjectionOperation>>(endpoint);
                return result ?? new List<InjectionOperation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading injection operations for well {InjectionWellId}", wellUWI);
                throw;
            }
        }

        public async Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate)
        {
            try
            {
                var result = await _apiClient.PostAsync<object, InjectionOperation>(
                    "/api/enhancedrecovery/injection",
                    new { InjectionWellId = injectionWellId, InjectionRate = injectionRate });
                return result ?? throw new InvalidOperationException("Failed to manage injection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing injection for well {InjectionWellId}", injectionWellId);
                throw;
            }
        }
    }
}