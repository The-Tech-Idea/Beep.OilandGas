using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Microsoft.Extensions.Logging;
using PROSPECT = Beep.OilandGas.Models.Data.ProspectIdentification.PROSPECT;
using SEIS_ACQTN_SURVEY = Beep.OilandGas.PPDM39.Models.SEIS_ACQTN_SURVEY;
using SEIS_LINE = Beep.OilandGas.PPDM39.Models.SEIS_LINE;

namespace Beep.OilandGas.Web.Services;

public sealed class ExplorationServiceClient : IExplorationServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<ExplorationServiceClient> _logger;

    public ExplorationServiceClient(ApiClient apiClient, ILogger<ExplorationServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ExplorationDashboardSummaryDto?> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<ExplorationDashboardSummaryDto>(
                "/api/field/current/exploration/dashboard-summary",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exploration dashboard summary.");
            throw;
        }
    }

    public async Task<List<PROSPECT>> GetProspectsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<PROSPECT>>(
                "/api/field/current/exploration/prospects",
                cancellationToken);

            return result ?? new List<PROSPECT>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field prospects.");
            throw;
        }
    }

    public async Task<PROSPECT?> GetProspectAsync(string prospectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

        try
        {
            return await _apiClient.GetAsync<PROSPECT>(
                $"/api/field/current/exploration/prospects/{Uri.EscapeDataString(prospectId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prospect {ProspectId}.", prospectId);
            throw;
        }
    }

    public async Task<List<ExplorationAfeLineDto>> GetProspectAfeLinesAsync(string prospectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

        try
        {
            var result = await _apiClient.GetAsync<List<ExplorationAfeLineDto>>(
                $"/api/field/current/exploration/prospects/{Uri.EscapeDataString(prospectId)}/afe-lines",
                cancellationToken);

            return result ?? new List<ExplorationAfeLineDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AFE lines for prospect {ProspectId}.", prospectId);
            throw;
        }
    }

    public async Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

        try
        {
            var result = await _apiClient.GetAsync<ProspectEvaluation>(
                $"/api/prospectidentification/evaluate/{Uri.EscapeDataString(prospectId)}",
                cancellationToken);

            return result ?? throw new InvalidOperationException($"Failed to evaluate prospect {prospectId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating prospect {ProspectId}.", prospectId);
            throw;
        }
    }

    public async Task<PROSPECT?> CreateProspectAsync(ProspectRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ProspectRequest, PROSPECT>(
                BuildUserScopedEndpoint("/api/field/current/exploration/prospects", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating field prospect {ProspectName}.", request.ProspectName);
            throw;
        }
    }

    public async Task<PROSPECT?> UpdateProspectAsync(string prospectId, ProspectRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PutAsync<ProspectRequest, PROSPECT>(
                BuildUserScopedEndpoint($"/api/field/current/exploration/prospects/{Uri.EscapeDataString(prospectId)}", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prospect {ProspectId}.", prospectId);
            throw;
        }
    }

    public async Task<bool> DeleteProspectAsync(string prospectId, string? userId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));

        try
        {
            return await _apiClient.DeleteAsync(
                BuildUserScopedEndpoint($"/api/field/current/exploration/prospects/{Uri.EscapeDataString(prospectId)}", userId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prospect {ProspectId}.", prospectId);
            throw;
        }
    }

    public async Task<List<SEIS_ACQTN_SURVEY>> GetSeismicSurveysAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<SEIS_ACQTN_SURVEY>>(
                "/api/field/current/exploration/seismic-surveys",
                cancellationToken);

            return result ?? new List<SEIS_ACQTN_SURVEY>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field seismic surveys.");
            throw;
        }
    }

    public async Task<SEIS_ACQTN_SURVEY?> CreateSeismicSurveyAsync(SeismicSurveyRequest request, string? userId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<SeismicSurveyRequest, SEIS_ACQTN_SURVEY>(
                BuildUserScopedEndpoint("/api/field/current/exploration/seismic-surveys", userId),
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating seismic survey {SurveyName}.", request.SurveyName);
            throw;
        }
    }

    public async Task<List<SEIS_LINE>> GetSeismicLinesAsync(string surveyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(surveyId))
            throw new ArgumentException("Survey ID is required.", nameof(surveyId));

        try
        {
            var result = await _apiClient.GetAsync<List<SEIS_LINE>>(
                $"/api/field/current/exploration/seismic-surveys/{Uri.EscapeDataString(surveyId)}/lines",
                cancellationToken);

            return result ?? new List<SEIS_LINE>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seismic lines for survey {SurveyId}.", surveyId);
            throw;
        }
    }

    public async Task<bool> RecordProspectDecisionAsync(string prospectId, string decision, string? comments = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prospectId))
            throw new ArgumentException("Prospect ID is required.", nameof(prospectId));
        if (string.IsNullOrWhiteSpace(decision))
            throw new ArgumentException("Decision is required.", nameof(decision));

        try
        {
            return await _apiClient.PostAsync(
                $"/api/field/current/exploration/prospects/{Uri.EscapeDataString(prospectId)}/decision",
                new ProspectDecisionRequest { Decision = decision, Comments = comments },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording decision {Decision} for prospect {ProspectId}.", decision, prospectId);
            throw;
        }
    }

    private static string BuildUserScopedEndpoint(string endpoint, string? userId)
    {
        return string.IsNullOrWhiteSpace(userId)
            ? endpoint
            : $"{endpoint}?userId={Uri.EscapeDataString(userId)}";
    }

    private sealed class ProspectDecisionRequest
    {
        public string? Decision { get; set; }
        public string? Comments { get; set; }
    }
}