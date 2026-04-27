using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Analytics;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed class BusinessProcessServiceClient : IBusinessProcessServiceClient
{
    private readonly ApiClient _apiClient;
    private readonly ILogger<BusinessProcessServiceClient> _logger;

    public BusinessProcessServiceClient(ApiClient apiClient, ILogger<BusinessProcessServiceClient> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<ProcessDefinition>> GetDefinitionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessDefinition>>(
                "/api/field/current/process/definitions",
                cancellationToken);

            return result ?? new List<ProcessDefinition>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definitions.");
            throw;
        }
    }

    public async Task<ProcessDefinition?> GetDefinitionAsync(string processId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processId))
            throw new ArgumentException("Process ID is required.", nameof(processId));

        try
        {
            return await _apiClient.GetAsync<ProcessDefinition>(
                $"/api/field/current/process/definitions/{Uri.EscapeDataString(processId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process definition {ProcessId}.", processId);
            throw;
        }
    }

    public async Task<ProcessInstance?> StartInstanceAsync(ProcessInstanceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ProcessInstanceRequest, ProcessInstance>(
                "/api/field/current/process/instances",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting process instance for process {ProcessId} and entity {EntityId}.", request.ProcessId, request.EntityId);
            throw;
        }
    }

    public async Task<ProcessInstance?> SubmitComplianceReportAsync(ComplianceReportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<ComplianceReportRequest, ProcessInstance>(
                "/api/field/current/process/compliance/reports",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error submitting compliance report for jurisdiction {Jurisdiction} and obligation {ObligationType}.",
                request.JurisdictionTag,
                request.ObligationType);
            throw;
        }
    }

    public async Task<ProcessInstance?> ReportHseIncidentAsync(HSEIncidentReportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            return await _apiClient.PostAsync<HSEIncidentReportRequest, ProcessInstance>(
                "/api/field/current/process/hse/incidents",
                request,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error reporting workflow HSE incident for type {IncidentType} at {IncidentDateTime}.",
                request.IncidentType,
                request.IncidentDateTime);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetHseIncidentsAsync(string? status = null, string? severity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/process/hse/incidents";
            var queryParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
                queryParts.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(severity))
                queryParts.Add($"severity={Uri.EscapeDataString(severity)}");
            if (queryParts.Count > 0)
                endpoint += $"?{string.Join("&", queryParts)}";

            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(endpoint, cancellationToken);
            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow HSE incidents with status {Status} and severity {Severity}.", status, severity);
            throw;
        }
    }

    public async Task<ProcessInstance?> GetHseIncidentAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstance>(
                BuildHseIncidentEndpoint(incidentId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<ProcessInstance?> EnsureHseIncidentWorkflowAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        try
        {
            return await _apiClient.PostAsync<object, ProcessInstance>(
                BuildHseIncidentEndpoint(incidentId, "ensure-workflow"),
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task SubmitHseIncidentRcaAsync(string incidentId, HSEIncidentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var succeeded = await _apiClient.PostAsync(
                BuildHseIncidentEndpoint(incidentId, "rca"),
                request,
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to submit RCA for incident {incidentId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting RCA for workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task RaiseHseCorrectiveActionsAsync(string incidentId, HSEIncidentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var succeeded = await _apiClient.PostAsync(
                BuildHseIncidentEndpoint(incidentId, "actions"),
                request,
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to raise corrective actions for incident {incidentId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error raising corrective actions for workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task CloseHseCorrectiveActionAsync(string incidentId, string actionId, CorrectiveActionCloseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));
        if (string.IsNullOrWhiteSpace(actionId))
            throw new ArgumentException("Action ID is required.", nameof(actionId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var succeeded = await _apiClient.PatchAsync(
                BuildHseIncidentEndpoint(incidentId, $"actions/{Uri.EscapeDataString(actionId)}"),
                request,
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to close corrective action {actionId} for incident {incidentId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing corrective action {ActionId} for workflow HSE incident {IncidentId}.", actionId, incidentId);
            throw;
        }
    }

    public async Task CloseHseIncidentAsync(string incidentId, IncidentCloseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(incidentId))
            throw new ArgumentException("Incident ID is required.", nameof(incidentId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var succeeded = await _apiClient.PostAsync(
                BuildHseIncidentEndpoint(incidentId, "close"),
                request,
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to close incident {incidentId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing workflow HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetInstancesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(
                "/api/field/current/process/instances",
                cancellationToken);

            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instances.");
            throw;
        }
    }

    public async Task<ProcessInstance?> GetInstanceAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance ID is required.", nameof(instanceId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstance>(
                $"/api/field/current/process/instances/{Uri.EscapeDataString(instanceId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting process instance {InstanceId}.", instanceId);
            throw;
        }
    }

    public async Task<List<ProcessHistoryEntry>> GetHistoryAsync(string instanceId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
            throw new ArgumentException("Instance ID is required.", nameof(instanceId));

        try
        {
            var result = await _apiClient.GetAsync<List<ProcessHistoryEntry>>(
                $"/api/field/current/process/instances/{Uri.EscapeDataString(instanceId)}/history",
                cancellationToken);

            return result ?? new List<ProcessHistoryEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting history for process instance {InstanceId}.", instanceId);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetGatesAsync(string? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/gates";
            if (!string.IsNullOrWhiteSpace(status))
                endpoint += $"?status={Uri.EscapeDataString(status)}";

            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(endpoint, cancellationToken);
            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate reviews with status {Status}.", status);
            throw;
        }
    }

    public async Task<List<ProcessInstanceSummary>> GetPendingGatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<ProcessInstanceSummary>>(
                "/api/field/current/gates/pending",
                cancellationToken);

            return result ?? new List<ProcessInstanceSummary>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending gate reviews.");
            throw;
        }
    }

    public async Task<ProcessInstanceSummary?> GetGateAsync(string gateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        try
        {
            return await _apiClient.GetAsync<ProcessInstanceSummary>(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate review {GateId}.", gateId);
            throw;
        }
    }

    public async Task<GateChecklistResponse?> GetGateChecklistAsync(string gateId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        try
        {
            return await _apiClient.GetAsync<GateChecklistResponse>(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}/checklist",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist for gate review {GateId}.", gateId);
            throw;
        }
    }

    public async Task ApproveGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "approve", cancellationToken);
    }

    public async Task RejectGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "reject", cancellationToken);
    }

    public async Task DeferGateAsync(string gateId, GateReviewDecisionRequest request, CancellationToken cancellationToken = default)
    {
        await PostGateDecisionAsync(gateId, request, "defer", cancellationToken);
    }

    public async Task<AnalyticsDashboardSummary?> GetAnalyticsDashboardAsync(int days = 30, double exposureHours = 1_000_000, CancellationToken cancellationToken = default)
    {
        try
        {
            var exposureValue = exposureHours.ToString(CultureInfo.InvariantCulture);
            return await _apiClient.GetAsync<AnalyticsDashboardSummary>(
                $"/api/field/current/process/analytics/dashboard?days={days}&exposureHours={Uri.EscapeDataString(exposureValue)}",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics dashboard for {Days} days.", days);
            throw;
        }
    }

    public async Task<List<KPITrendPoint>> GetProductionTrendAsync(int days = 365, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _apiClient.GetAsync<List<KPITrendPoint>>(
                $"/api/field/current/process/analytics/production/trend?days={days}",
                cancellationToken);

            return result ?? new List<KPITrendPoint>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting production trend for {Days} days.", days);
            throw;
        }
    }

    public async Task<ReservesMaturationSummary?> GetReservesMaturationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _apiClient.GetAsync<ReservesMaturationSummary>(
                "/api/field/current/process/analytics/reserves/maturation",
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reserves maturation analytics.");
            throw;
        }
    }

    private async Task PostGateDecisionAsync(string gateId, GateReviewDecisionRequest request, string action, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gateId))
            throw new ArgumentException("Gate ID is required.", nameof(gateId));

        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var succeeded = await _apiClient.PostAsync(
                $"/api/field/current/gates/{Uri.EscapeDataString(gateId)}/{action}",
                request,
                cancellationToken);

            if (!succeeded)
                throw new InvalidOperationException($"Failed to {action} gate review {gateId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing gate action {Action} for gate {GateId}.", action, gateId);
            throw;
        }
    }

    private static string BuildHseIncidentEndpoint(string incidentId, string? suffix = null)
    {
        var endpoint = $"/api/field/current/process/hse/incidents/{Uri.EscapeDataString(incidentId)}";
        return string.IsNullOrWhiteSpace(suffix) ? endpoint : $"{endpoint}/{suffix}";
    }
}