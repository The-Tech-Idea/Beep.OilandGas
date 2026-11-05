using TheTechIdea.Data.OilGas;
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

    public async Task<ProductionDashboardResponse> GetDashboardAsync(string fieldId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        return await _apiClient.GetAsync<ProductionDashboardResponse>(
            $"/api/fields/{Uri.EscapeDataString(fieldId)}/production/dashboard", cancellationToken)
            ?? throw new InvalidOperationException("Production dashboard response was empty.");
    }

    public async Task<List<ProductionWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProductionWellStatusDto>>(
                "/api/field/current/production/dashboard/wells",
                cancellationToken);

            return result ?? throw new InvalidOperationException("Production well response was empty.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production dashboard wells.");
            throw;
        }
    }

    public async Task<ReservoirDashboardSummary?> GetReservoirDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<ReservoirDashboardSummary>(
                "/api/field/current/reservoir/dashboard/summary",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservoir dashboard summary.");
            throw;
        }
    }

    public async Task<List<ReservoirPoolDto>> GetReservoirPoolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ReservoirPoolDto>>(
                "/api/field/current/reservoir/dashboard/pools",
                cancellationToken);

            return result ?? new List<ReservoirPoolDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservoir dashboard pools.");
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
        var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null)throw new InvalidOperationException("Select the original company before reading recorded interventions.");
        try
        {
            var result = await _apiClient.GetAsync<List<ProductionInterventionCandidateDto>>(
                "/api/field/current/production/intervention-candidates",
                cancellationToken);

            if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The intervention company context changed.");
            if(result is null)throw new InvalidOperationException("Recorded intervention candidates were not returned.");
            if(result.Count>0){var field=result[0]?.FieldId;if(string.IsNullOrWhiteSpace(field))throw new InvalidOperationException("The recorded intervention FIELD is unavailable.");ProductionInterventionCandidateChecks.Require(result,snapshot.Selection.Scope,snapshot.Selection.UserId,field);}
            return result;
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
        var snapshot = _apiClient.CompanySnapshot;
        if (request.IntentId != Guid.Empty && (snapshot.Selection is null || request.Context is null ||
            request.Context.Company != snapshot.Selection.Scope || request.Context.ActorUserId != snapshot.Selection.UserId || request.Context.FieldId != request.FieldId))
            throw new InvalidOperationException("The original native decision must match the selected company/account/FIELD review.");

        try
        {
            var result = await _apiClient.PostAsync<ProductionInterventionDecisionRequest, ProductionInterventionDecisionResult>(
                request.IntentId == Guid.Empty
                    ? $"/api/field/current/production/intervention-candidates/{Uri.EscapeDataString(wellId)}/decision"
                    : $"/api/field/current/production/intervention-decisions?wellId={Uri.EscapeDataString(wellId)}",
                request,
                cancellationToken) ?? throw new InvalidOperationException("The recorded intervention decision was not returned.");
            if (request.IntentId != Guid.Empty)
            {
                if (snapshot != _apiClient.CompanySnapshot) throw new OperationCanceledException("The original native decision company context changed.");
                ProductionInterventionDecisionChecks.RequireAcknowledgement(request, result, wellId);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording intervention decision for well {WellId}.", wellId);
            throw;
        }
    }

    public async Task<ProductionInterventionNativeRecovery> RecoverInterventionDecisionAsync(string wellId, Guid intentId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wellId);
        if (intentId == Guid.Empty) throw new ArgumentException("The original decision identity is required.", nameof(intentId));
        var snapshot = _apiClient.CompanySnapshot;
        if (snapshot.Selection is null) throw new InvalidOperationException("Select the original company before recovering a private decision.");
        var value = await _apiClient.GetAsync<ProductionInterventionNativeRecovery>($"/api/field/current/production/intervention-decisions/{intentId:D}?wellId={Uri.EscapeDataString(wellId)}", cancellationToken)
            ?? throw new InvalidOperationException("The original native recovery was not returned.");
        if (snapshot != _apiClient.CompanySnapshot) throw new OperationCanceledException("The original decision recovery company context changed.");
        value.Require();
        if (value.Company != snapshot.Selection.Scope || value.Actor != snapshot.Selection.UserId || value.IntentId != intentId || value.Intent?.Restore().Review.Well.UWI is { } originalWell && originalWell != wellId)
            throw new InvalidOperationException("The private native recovery belongs to another company/account/WELL/identity.");
        return value;
    }
    public async Task<ProductionInterventionMaintenanceLink> GetInterventionMaintenanceLinkAsync(string wellId,Guid decisionId,string workOrderId,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wellId);ArgumentException.ThrowIfNullOrWhiteSpace(workOrderId);
        if(decisionId==Guid.Empty)throw new ArgumentException("The original decision reference is required.",nameof(decisionId));
        var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null)throw new InvalidOperationException("Select the receiving company before reading recorded intervention source.");
        var value=await _apiClient.GetAsync<ProductionInterventionMaintenanceLink>($"/api/field/current/production/intervention-decisions/{decisionId:D}/maintenance-link?wellId={Uri.EscapeDataString(wellId)}&workOrderId={Uri.EscapeDataString(workOrderId)}",cancellationToken)
            ??throw new InvalidOperationException("The original receiving source was not returned.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("The receiving source company changed.");
        value.Require(snapshot.Selection.Scope,snapshot.Selection.UserId,value.FieldId,workOrderId,wellId,decisionId);
        return value;
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

    public async Task<WellTestReview> GetWellTestReviewAsync(string wellId,CancellationToken cancellationToken=default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wellId);
        return await _apiClient.GetAsync<WellTestReview>($"/api/field/current/production/wells/{Uri.EscapeDataString(wellId)}/tests/review",cancellationToken)
            ?? throw new InvalidOperationException("Recorded well-test review was not returned.");
    }
    public async Task<List<WellTestResponse>> GetWellTestsAsync(string wellId,CancellationToken cancellationToken=default)
        => (await GetWellTestReviewAsync(wellId,cancellationToken)).Rows.ToList();

    public async Task PatchAllocationAsync(string period, string wellId, ProductionAllocationPatchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(period))
            throw new ArgumentException("Allocation period is required.", nameof(period));

        if (string.IsNullOrWhiteSpace(wellId))
            throw new ArgumentException("Well ID is required.", nameof(wellId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _apiClient.PatchAsync(
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
