using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class WellStatusServiceClient : IWellStatusServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<WellStatusServiceClient> _logger;

    public WellStatusServiceClient(ApiClient apiClient, ILogger<WellStatusServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Dictionary<string, WELL_STATUS>> GetCurrentStatusAsync(string uwi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            var result = await _apiClient.GetAsync<Dictionary<string, WELL_STATUS>>(
                $"/api/wellstatus/{Uri.EscapeDataString(uwi)}/current",
                cancellationToken);
            return result ?? new Dictionary<string, WELL_STATUS>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current well status for {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<List<WellServices.FacetTypeDto>> GetFacetPageDataAsync(string uwi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            var result = await _apiClient.GetAsync<List<WellServices.FacetTypeDto>>(
                $"/api/wellstatus/{Uri.EscapeDataString(uwi)}/page-data",
                cancellationToken);
            return result ?? new List<WellServices.FacetTypeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting well status page data for {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<List<WELL_STATUS>> GetStatusHistoryAsync(string uwi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            var result = await _apiClient.GetAsync<List<WELL_STATUS>>(
                $"/api/wellstatus/{Uri.EscapeDataString(uwi)}/history",
                cancellationToken);
            return result ?? new List<WELL_STATUS>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting well status history for {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<List<WellServices.FacetQualifierDto>> GetQualifiersAsync(string statusType, string status, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(statusType))
            throw new ArgumentException("Status type is required.", nameof(statusType));
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Status is required.", nameof(status));

        try
        {
            var result = await _apiClient.GetAsync<List<WellServices.FacetQualifierDto>>(
                $"/api/wellstatus/reference/{Uri.EscapeDataString(statusType)}/qualifiers/{Uri.EscapeDataString(status)}",
                cancellationToken);
            return result ?? new List<WellServices.FacetQualifierDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting well status qualifiers for {StatusType}/{Status}.", statusType, status);
            throw;
        }
    }

    public async Task<WELL_STATUS?> SetFacetAsync(string uwi, WellServices.SetFacetRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync<WellServices.SetFacetRequest, WELL_STATUS>(
                $"/api/wellstatus/{Uri.EscapeDataString(uwi)}/facet",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting well status facet for {Uwi}.", uwi);
            throw;
        }
    }
}