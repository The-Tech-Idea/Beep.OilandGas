using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public class EnhancedRecoveryClient : IEnhancedRecoveryClient
{
    private readonly ApiClient _apiClient;

    public EnhancedRecoveryClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<EnhancedRecoveryOperationDto>> GetOperationsAsync(string? fieldId = null, CancellationToken cancellationToken = default)
    {
        var url = "/api/field/current/enhanced-recovery/operations" + (!string.IsNullOrWhiteSpace(fieldId) ? $"?fieldId={fieldId}" : "");
        return await _apiClient.GetAsync<List<EnhancedRecoveryOperationDto>>(url, cancellationToken) ?? new();
    }

    public async Task<EnhancedRecoveryOperationDto?> GetOperationAsync(string operationId, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<EnhancedRecoveryOperationDto>($"/api/field/current/enhanced-recovery/operations/{operationId}", cancellationToken);
    }

    public async Task<List<InjectionOperationDto>> GetInjectionOperationsAsync(string? wellUwi = null, CancellationToken cancellationToken = default)
    {
        var url = "/api/field/current/enhanced-recovery/injection" + (!string.IsNullOrWhiteSpace(wellUwi) ? $"?wellUwi={wellUwi}" : "");
        return await _apiClient.GetAsync<List<InjectionOperationDto>>(url, cancellationToken) ?? new();
    }

    public async Task<List<WaterFloodingDto>> GetWaterFloodingAsync(string? fieldId = null, CancellationToken cancellationToken = default)
    {
        var url = "/api/field/current/enhanced-recovery/water-flooding" + (!string.IsNullOrWhiteSpace(fieldId) ? $"?fieldId={fieldId}" : "");
        return await _apiClient.GetAsync<List<WaterFloodingDto>>(url, cancellationToken) ?? new();
    }

    public async Task<List<GasInjectionDto>> GetGasInjectionAsync(string? fieldId = null, CancellationToken cancellationToken = default)
    {
        var url = "/api/field/current/enhanced-recovery/gas-injection" + (!string.IsNullOrWhiteSpace(fieldId) ? $"?fieldId={fieldId}" : "");
        return await _apiClient.GetAsync<List<GasInjectionDto>>(url, cancellationToken) ?? new();
    }

    public async Task<EorEconomicAnalysisDto> AnalyzeEconomicsAsync(EorEconomicRequest request, CancellationToken cancellationToken = default)
    {
        return await _apiClient.PostAsync<EorEconomicRequest, EorEconomicAnalysisDto>("/api/field/current/enhanced-recovery/economics", request, cancellationToken)
            ?? new EorEconomicAnalysisDto();
    }
}
