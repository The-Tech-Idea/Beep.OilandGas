using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Development;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class DevelopmentServiceClient : IDevelopmentServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<DevelopmentServiceClient> _logger;

    public DevelopmentServiceClient(ApiClient apiClient, ILogger<DevelopmentServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DevelopmentDashboardSummary?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<DevelopmentDashboardSummary>(
                "/api/field/current/development/dashboard/summary",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development dashboard summary.");
            throw;
        }
    }

    public async Task<List<DevelopmentWellStatusDto>> GetDashboardWellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<DevelopmentWellStatusDto>>(
                "/api/field/current/development/dashboard/wells",
                cancellationToken);

            return result ?? new List<DevelopmentWellStatusDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development dashboard wells.");
            throw;
        }
    }

    public async Task<DevelopmentConstructionProgressDto?> GetConstructionProgressAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<DevelopmentConstructionProgressDto>(
                "/api/field/current/development/construction-progress",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development construction progress.");
            throw;
        }
    }

    public async Task<List<WELL>> GetWellsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<WELL>>(
                "/api/field/current/development/wells",
                cancellationToken);

            return result ?? new List<WELL>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development wells.");
            throw;
        }
    }

    public async Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        try
        {
            return await _apiClient.GetAsync<WELL>(
                $"/api/field/current/development/wells/{Uri.EscapeDataString(uwi)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development well {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<bool> AssignRigAsync(string uwi, DevelopmentAssignRigRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(uwi))
            throw new ArgumentException("UWI is required.", nameof(uwi));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync(
                $"/api/field/current/development/wells/{Uri.EscapeDataString(uwi)}/rig",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning rig to well {Uwi}.", uwi);
            throw;
        }
    }

    public async Task<FdpStatusResponse?> GetFdpStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<FdpStatusResponse>(
                "/api/field/current/development/fdp",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FDP status.");
            throw;
        }
    }

    public async Task<SubmitFdpDraftResponse?> SubmitFdpAsync(SubmitFdpDraftRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<SubmitFdpDraftRequest, SubmitFdpDraftResponse>(
                "/api/field/current/development/fdp",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting FDP draft.");
            throw;
        }
    }

    public async Task<List<POOL>> GetPoolsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<POOL>>(
                "/api/field/current/development/pools",
                cancellationToken);

            return result ?? new List<POOL>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development pools.");
            throw;
        }
    }

    public async Task<POOL?> CreatePoolAsync(PoolRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<PoolRequest, POOL>(
                BuildUserScopedEndpoint("/api/field/current/development/pools", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating development pool {PoolName}.", request.PoolName);
            throw;
        }
    }

    public async Task<POOL?> UpdatePoolAsync(string poolId, PoolRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(poolId))
            throw new ArgumentException("Pool ID is required.", nameof(poolId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync<PoolRequest, POOL>(
                BuildUserScopedEndpoint($"/api/field/current/development/pools/{Uri.EscapeDataString(poolId)}", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating development pool {PoolId}.", poolId);
            throw;
        }
    }

    public async Task<List<FACILITY>> GetFacilitiesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<FACILITY>>(
                "/api/field/current/development/facilities",
                cancellationToken);

            return result ?? new List<FACILITY>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting development facilities.");
            throw;
        }
    }

    public async Task<FacilityResponse?> CreateFacilityAsync(FacilityRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<FacilityRequest, FacilityResponse>(
                BuildUserScopedEndpoint("/api/field/current/development/facilities", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating development facility {FacilityName}.", request.FacilityName);
            throw;
        }
    }

    public async Task<FacilityResponse?> UpdateFacilityAsync(string facilityId, FacilityRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
            throw new ArgumentException("Facility ID is required.", nameof(facilityId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync<FacilityRequest, FacilityResponse>(
                BuildUserScopedEndpoint($"/api/field/current/development/facilities/{Uri.EscapeDataString(facilityId)}", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating development facility {FacilityId}.", facilityId);
            throw;
        }
    }

    private static string BuildUserScopedEndpoint(string endpoint, string? userId)
    {
        return string.IsNullOrWhiteSpace(userId)
            ? endpoint
            : $"{endpoint}?userId={Uri.EscapeDataString(userId)}";
    }
}