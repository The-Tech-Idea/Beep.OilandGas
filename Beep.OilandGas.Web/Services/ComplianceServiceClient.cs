using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Compliance;
using Microsoft.Extensions.Logging;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.Web.Services;

public sealed class ComplianceServiceClient : IComplianceServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<ComplianceServiceClient> _logger;
    private readonly IFailureReporter _failures;

    public ComplianceServiceClient(ApiClient apiClient, ILogger<ComplianceServiceClient> logger, IFailureReporter failures)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<List<ObligationSummary>> GetAllObligationsAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/compliance/obligations";
            if (year.HasValue)
                endpoint += $"?year={year.Value}";

            var result = await _apiClient.GetAsync<List<ObligationSummary>>(endpoint, cancellationToken);
            return result ?? new List<ObligationSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance obligations for year {Year}.", year);
            throw;
        }
    }

    public async Task<List<ObligationSummary>> GetUpcomingObligationsAsync(int daysAhead = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ObligationSummary>>(
                $"/api/field/current/compliance/obligations/upcoming?daysAhead={daysAhead}",
                cancellationToken);

            return result ?? new List<ObligationSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upcoming compliance obligations for {DaysAhead} days.", daysAhead);
            throw;
        }
    }

    public async Task<List<ObligationSummary>> GetOverdueObligationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ObligationSummary>>(
                "/api/field/current/compliance/obligations/overdue",
                cancellationToken);

            return result ?? new List<ObligationSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue compliance obligations.");
            throw;
        }
    }

    public async Task<ObligationDetailModel?> GetObligationAsync(string obligationId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(obligationId))
            throw new ArgumentException("Obligation ID is required.", nameof(obligationId));

        try
        {
            return await _apiClient.GetAsync<ObligationDetailModel>(
                $"/api/field/current/compliance/obligations/{Uri.EscapeDataString(obligationId)}",
                cancellationToken);
        }
        catch (OilGasApiException notFound) when (notFound.StatusCode == HttpStatusCode.NotFound)
        {
            // The API answers an obligation it does not hold with 404; this method's contract answers it as null, which
            // the caller shows as "not found". Recorded so the store holds every answer that was not a success.
            _failures.ReportHandled(
                notFound,
                "reading a compliance obligation",
                consequence: "the API holds no such obligation; the caller was answered null and says it was not found",
                FailureSeverity.Degraded);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance obligation {ObligationId}.", obligationId);
            throw;
        }
    }

    public async Task<string?> CreateObligationAsync(CreateObligationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<CreateObligationRequest, string>(
                "/api/field/current/compliance/obligations",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating compliance obligation for field {FieldId} and type {ObligationType}.", request.FieldId, request.ObligType);
            throw;
        }
    }

    public async Task<ComplianceScoreCard?> GetScoreAsync(int? year = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/compliance/score";
            if (year.HasValue)
                endpoint += $"?year={year.Value}";

            return await _apiClient.GetAsync<ComplianceScoreCard>(endpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting compliance score for year {Year}.", year);
            throw;
        }
    }

    public async Task SubmitObligationAsync(string obligationId, DateTime? submitDate = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(obligationId))
            throw new ArgumentException("Obligation ID is required.", nameof(obligationId));

        var effectiveSubmitDate = submitDate ?? DateTime.UtcNow;
        var endpoint = $"/api/field/current/compliance/obligations/{Uri.EscapeDataString(obligationId)}/submit?submitDate={Uri.EscapeDataString(effectiveSubmitDate.ToString("O"))}";

        try
        {
            await _apiClient.PostAsync(endpoint, new { }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting compliance obligation {ObligationId}.", obligationId);
            throw;
        }
    }

    public async Task WaiveObligationAsync(string obligationId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(obligationId))
            throw new ArgumentException("Obligation ID is required.", nameof(obligationId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Waive reason is required.", nameof(reason));

        var endpoint = $"/api/field/current/compliance/obligations/{Uri.EscapeDataString(obligationId)}/waive?reason={Uri.EscapeDataString(reason)}";

        try
        {
            await _apiClient.PostAsync(endpoint, new { }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error waiving compliance obligation {ObligationId}.", obligationId);
            throw;
        }
    }

    public async Task RecordPaymentAsync(string obligationId, RecordPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(obligationId))
            throw new ArgumentException("Obligation ID is required.", nameof(obligationId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            await _apiClient.PostAsync(
                $"/api/field/current/compliance/obligations/{Uri.EscapeDataString(obligationId)}/payment",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording payment for compliance obligation {ObligationId}.", obligationId);
            throw;
        }
    }

    public async Task<GHGEmissionReport?> GenerateGhgReportAsync(int year, string jurisdiction, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jurisdiction))
            throw new ArgumentException("Jurisdiction is required.", nameof(jurisdiction));

        try
        {
            return await _apiClient.PostAsync<object, GHGEmissionReport>(
                $"/api/field/current/compliance/ghg/report?year={year}&jurisdiction={Uri.EscapeDataString(jurisdiction)}",
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating GHG report for year {Year} and jurisdiction {Jurisdiction}.", year, jurisdiction);
            throw;
        }
    }
}