using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class HSEServiceClient : IHSEServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<HSEServiceClient> _logger;

    public HSEServiceClient(ApiClient apiClient, ILogger<HSEServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/hse/incidents";
            if (from.HasValue && to.HasValue)
                endpoint += $"?from={Uri.EscapeDataString(from.Value.ToString("O"))}&to={Uri.EscapeDataString(to.Value.ToString("O"))}";

            var result = await _apiClient.GetAsync<List<HSEIncidentRecord>>(endpoint, cancellationToken);
            return result ?? new List<HSEIncidentRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSE incidents from {From} to {To}.", from, to);
            throw;
        }
    }

    public async Task<HSEIncidentRecord?> GetIncidentAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<HSEIncidentRecord>(
                BuildIncidentEndpoint(incidentId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<List<CauseFinding>> GetIncidentCausesAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<CauseFinding>>(
                BuildIncidentEndpoint(incidentId, "causes"),
                cancellationToken);
            return result ?? new List<CauseFinding>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting RCA causes for HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<List<CAStatus>> GetIncidentCorrectiveActionsAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<CAStatus>>(
                BuildIncidentEndpoint(incidentId, "cas"),
                cancellationToken);
            return result ?? new List<CAStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting corrective actions for HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<BarrierSummary?> GetIncidentBarrierSummaryAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<BarrierSummary>(
                BuildIncidentEndpoint(incidentId, "barriers/summary"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting barrier summary for HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<bool> IsIncidentRcaCompleteAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<bool>(
                BuildIncidentEndpoint(incidentId, "rca-complete"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting RCA completion status for HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<HAZOPSummary?> GetHazopStudyAsync(string studyId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<HAZOPSummary>(
                BuildHazopStudyEndpoint(studyId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HAZOP study {StudyId}.", studyId);
            throw;
        }
    }

    public async Task<List<HAZOPNode>> GetHazopNodesAsync(string studyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<HAZOPNode>>(
                BuildHazopStudyEndpoint(studyId, "nodes"),
                cancellationToken);
            return result ?? new List<HAZOPNode>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HAZOP nodes for study {StudyId}.", studyId);
            throw;
        }
    }

    public async Task<string?> AddHazopNodeAsync(string studyId, AddNodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<AddNodeRequest, string>(
                BuildHazopStudyEndpoint(studyId, "nodes"),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding HAZOP node to study {StudyId}.", studyId);
            throw;
        }
    }

    public async Task<string?> AddHazopDeviationAsync(string studyId, int nodeSeq, AddDeviationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (nodeSeq <= 0)
            throw new ArgumentOutOfRangeException(nameof(nodeSeq), "Node sequence must be positive.");

        try
        {
            return await _apiClient.PostAsync<AddDeviationRequest, string>(
                BuildHazopStudyEndpoint(studyId, $"nodes/{nodeSeq}/deviations"),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding deviation to HAZOP study {StudyId}, node {NodeSeq}.", studyId, nodeSeq);
            throw;
        }
    }

    public async Task<bool> UpdateHazopDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, CancellationToken cancellationToken = default)
    {
        if (nodeSeq <= 0)
            throw new ArgumentOutOfRangeException(nameof(nodeSeq), "Node sequence must be positive.");
        if (condSeq <= 0)
            throw new ArgumentOutOfRangeException(nameof(condSeq), "Deviation sequence must be positive.");
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Deviation status is required.", nameof(status));

        try
        {
            return await _apiClient.PutAsync(
                BuildHazopStudyEndpoint(studyId, $"nodes/{nodeSeq}/deviations/{condSeq}/status?status={Uri.EscapeDataString(status)}"),
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating deviation status for HAZOP study {StudyId}, node {NodeSeq}, deviation {CondSeq}.", studyId, nodeSeq, condSeq);
            throw;
        }
    }

    public async Task<HSEKPISet?> GetKpisAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/hse/kpi";
            if (from.HasValue && to.HasValue)
                endpoint += $"?from={Uri.EscapeDataString(from.Value.ToString("O"))}&to={Uri.EscapeDataString(to.Value.ToString("O"))}";

            return await _apiClient.GetAsync<HSEKPISet>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting HSE KPIs from {From} to {To}.", from, to);
            throw;
        }
    }

    private static string BuildIncidentEndpoint(string incidentId, string? suffix = null)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        var endpoint = $"/api/field/current/hse/incidents/{Uri.EscapeDataString(incidentId)}";
        return string.IsNullOrWhiteSpace(suffix) ? endpoint : $"{endpoint}/{suffix}";
    }

    private static string BuildHazopStudyEndpoint(string studyId, string? suffix = null)
    {
        if (string.IsNullOrWhiteSpace(studyId))
            throw new ArgumentException("Study ID is required.", nameof(studyId));

        var endpoint = $"/api/field/current/hse/hazop/{Uri.EscapeDataString(studyId)}";
        return string.IsNullOrWhiteSpace(suffix) ? endpoint : $"{endpoint}/{suffix}";
    }
}