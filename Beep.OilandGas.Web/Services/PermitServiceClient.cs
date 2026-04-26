using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Beep.OilandGas.Web.Services;

public class PermitServiceClient : IPermitServiceClient
{
    private readonly ApiClient _apiClient;

    public PermitServiceClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<PermitApplicationSummary>> GetAllAsync(string? status = null, string? authority = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={status}");
        if (!string.IsNullOrWhiteSpace(authority)) queryParams.Add($"authority={authority}");
        var url = "/api/field/current/permits" + (queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "");
        return await _apiClient.GetAsync<List<PermitApplicationSummary>>(url, cancellationToken) ?? new();
    }

    public async Task<PermitApplicationDetail?> GetByIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<PermitApplicationDetail>($"/api/field/current/permits/{applicationId}", cancellationToken);
    }

    public async Task<string> CreateAsync(CreatePermitApplicationRequest request, CancellationToken cancellationToken = default)
    {
        return await _apiClient.PostAsync<CreatePermitApplicationRequest, string>("/api/field/current/permits", request, cancellationToken) ?? string.Empty;
    }

    public async Task UpdateAsync(string applicationId, UpdatePermitApplicationRequest request, CancellationToken cancellationToken = default)
    {
        await _apiClient.PutAsync($"/api/field/current/permits/{applicationId}", request, cancellationToken);
    }

    public async Task SubmitAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        await _apiClient.PostAsync<object, object>($"/api/field/current/permits/{applicationId}/submit", new { }, cancellationToken);
    }

    public async Task<PermitDecisionResult> ProcessDecisionAsync(string applicationId, PermitDecisionRequest request, CancellationToken cancellationToken = default)
    {
        return await _apiClient.PostAsync<PermitDecisionRequest, PermitDecisionResult>($"/api/field/current/permits/{applicationId}/decision", request, cancellationToken)
            ?? new PermitDecisionResult();
    }

    public async Task<PermitComplianceResult?> CheckComplianceAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        return await _apiClient.GetAsync<PermitComplianceResult>($"/api/field/current/permits/{applicationId}/compliance", cancellationToken);
    }
}
