using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class ProductionServiceClient : IProductionServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<ProductionServiceClient> _logger;

    public ProductionServiceClient(ApiClient apiClient, ILogger<ProductionServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProductionDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<ProductionDashboardSummary>(
                "/api/field/current/production/dashboard/summary",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production dashboard summary.");
            throw;
        }
    }

    public async Task<List<ProductionWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProductionWellStatusDto>>(
                "/api/field/current/production/dashboard/wells",
                cancellationToken);

            return result ?? new List<ProductionWellStatusDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production dashboard wells.");
            throw;
        }
    }

    public async Task<ProductionWellPerformanceDto?> GetWellPerformanceAsync(string wellId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        try
        {
            return await _apiClient.GetAsync<ProductionWellPerformanceDto>(
                $"/api/field/current/production/wells/{Uri.EscapeDataString(wellId)}/performance",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<WellPerformanceAnalysisResponse?> GetWellPerformanceAnalysisAsync(string wellId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        try
        {
            return await _apiClient.GetAsync<WellPerformanceAnalysisResponse>(
                $"/api/field/current/production/wells/{Uri.EscapeDataString(wellId)}/analysis",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance analysis for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<PerformanceDeviationResult> LogPerformanceDeviationAsync(string wellId, PerformanceDeviationRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<PerformanceDeviationRequest, PerformanceDeviationResult>(
                $"/api/field/current/production/wells/{Uri.EscapeDataString(wellId)}/analysis/deviation",
                request,
                cancellationToken) ?? new PerformanceDeviationResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging performance deviation for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<List<ProductionInterventionCandidateDto>> GetInterventionCandidatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProductionInterventionCandidateDto>>(
                "/api/field/current/production/intervention-candidates",
                cancellationToken);

            return result ?? new List<ProductionInterventionCandidateDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production intervention candidates.");
            throw;
        }
    }

    public async Task<ProductionInterventionDecisionResult> RecordInterventionDecisionAsync(string wellId, ProductionInterventionDecisionRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ProductionInterventionDecisionRequest, ProductionInterventionDecisionResult>(
                $"/api/field/current/production/intervention-candidates/{Uri.EscapeDataString(wellId)}/decision",
                request,
                cancellationToken) ?? new ProductionInterventionDecisionResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording intervention decision for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<ProductionDecommissioningTriggerResult> TransitionToDecommissioningAsync(string wellId, ProductionDecommissioningTriggerRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ProductionDecommissioningTriggerRequest, ProductionDecommissioningTriggerResult>(
                $"/api/field/current/production/intervention-candidates/{Uri.EscapeDataString(wellId)}/transition-to-decommissioning",
                request,
                cancellationToken) ?? new ProductionDecommissioningTriggerResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning well {WellId} to decommissioning.", wellId);
            throw;
        }
    }

    public async Task<List<WellTestResponse>> GetWellTestsAsync(string wellId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        try
        {
            var result = await _apiClient.GetAsync<List<WellTestResponse>>(
                $"/api/field/current/production/wells/{Uri.EscapeDataString(wellId)}/tests",
                cancellationToken);

            return result ?? new List<WellTestResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting well tests for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<bool> PatchAllocationAsync(string period, string wellId, ProductionAllocationPatchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(period))
            throw new ArgumentException("Allocation period is required.", nameof(period));

        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PatchAsync(
                $"/api/field/current/production/allocation/{Uri.EscapeDataString(period)}/wells/{Uri.EscapeDataString(wellId)}",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching production allocation for well {WellId} period {Period}.", wellId, period);
            throw;
        }
    }

    public async Task<List<FIELD>> GetFieldsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<FIELD>>("/api/production/fields", cancellationToken);
            return result ?? new List<FIELD>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production field register.");
            throw;
        }
    }

    public async Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<POOL>>("/api/production/pools", cancellationToken);
            return result ?? new List<POOL>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production pool register.");
            throw;
        }
    }

    public async Task<List<RESERVE_ENTITY>> GetReservesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<RESERVE_ENTITY>>("/api/production/reserves", cancellationToken);
            return result ?? new List<RESERVE_ENTITY>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reserves register.");
            throw;
        }
    }

    public async Task<List<PDEN_VOL_SUMMARY>> GetReportingAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<PDEN_VOL_SUMMARY>>("/api/production/reporting", cancellationToken);
            return result ?? new List<PDEN_VOL_SUMMARY>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production reporting rows.");
            throw;
        }
    }

    public async Task<List<ProductionForecastResponse>> GetForecastsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProductionForecastResponse>>("/api/production/field/forecasts", cancellationToken);
            return result ?? new List<ProductionForecastResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production forecasts for the active field.");
            throw;
        }
    }
}