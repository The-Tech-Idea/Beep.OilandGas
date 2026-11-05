using TheTechIdea.Data.OilGas;
using SdkFieldService = Beep.OilandGas.Client.App.Services.Field.IFieldService;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.Web.Services;

public sealed partial class HSEServiceClient : IHSEServiceClient
{
    private readonly SdkFieldService _fields;
    private readonly ApiClient _apiClient;
    private readonly ILogger<HSEServiceClient> _logger;

    public HSEServiceClient(ApiClient apiClient, ILogger<HSEServiceClient> logger, SdkFieldService fields)
    {
        _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<InvestigatorAssignmentReview> ReviewInvestigatorAssignmentAsync(string incidentId, string expectedFieldId, CancellationToken cancellationToken = default) =>
        _fields.ReviewInvestigatorAssignmentAsync(incidentId, expectedFieldId, cancellationToken);
    public Task AssignInvestigatorAsync(InvestigatorAssignmentReview review, RecordedIncidentParticipant participant, CancellationToken cancellationToken = default) =>
        _fields.AssignInvestigatorAsync(review, participant, cancellationToken);

    public async Task<List<HSEIncidentRecord>> GetIncidentsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/field/current/hse/incidents";
            if (from.HasValue && to.HasValue)
                endpoint += $"?from={Uri.EscapeDataString(from.Value.ToString("O"))}&to={Uri.EscapeDataString(to.Value.ToString("O"))}";

            var result = await _apiClient.GetAsync<List<HSEIncidentRecord>>(endpoint, cancellationToken);
            return result ?? throw new InvalidOperationException("The incident population was not returned.");
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
                BuildIncidentReadEndpoint(incidentId),
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
                BuildIncidentReadEndpoint(incidentId, "causes"),
                cancellationToken);
            return result ?? throw new InvalidOperationException("The incident causes were not returned.");
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
                BuildIncidentReadEndpoint(incidentId, "cas"),
                cancellationToken);
            return result ?? throw new InvalidOperationException("The incident corrective actions were not returned.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting corrective actions for HSE incident {IncidentId}.", incidentId);
            throw;
        }
    }

    public async Task<CorrectiveActionOptions> ReviewCorrectiveActionOptionsAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context); FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new ArgumentException("An exact incident is required.", nameof(incidentId));
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException("The incident belongs to another company or account.");
        var result = await _apiClient.GetAsync<CorrectiveActionOptions>(BuildIncidentEndpoint(incidentId, "cas/options"), cancellationToken)
            ?? throw new InvalidOperationException("The corrective-action options are unavailable.");
        result.Require();
        if (snapshot != _apiClient.CompanySnapshot || result.Context != context || result.IncidentId != incidentId)
            throw new InvalidOperationException("The company, FIELD or incident changed during option review.");
        return result;
    }

    public async Task<string> AddCorrectiveActionAsync(FieldSelectionReview context, string incidentId, AddCARequest request, string planId, string optionsRevision, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context); ArgumentNullException.ThrowIfNull(request);
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId) || !LeaseSummaryValidator.Identifier(planId) || !ProcessDefinitionRevision.IsValid(optionsRevision) || request.ActionId == Guid.Empty) throw new InvalidOperationException("An exact incident, plan, choices and action are required.");
        var snapshot = _apiClient.CompanySnapshot; var selection = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selection.Scope != context.Company || selection.UserId != context.ActorUserId) throw new UnauthorizedAccessException("The action belongs to another company or account.");
        var result = await _apiClient.PostAsync<AddCASubmission, AddCAReceipt>(BuildIncidentEndpoint(incidentId, "cas"), new(context, request, planId, optionsRevision), cancellationToken);
        if (snapshot != _apiClient.CompanySnapshot || result is null || result.Context != context || result.IncidentId != incidentId ||
            result.ActionId != request.ActionId || result.StepId != request.ActionId.ToString("N"))
            throw new InvalidOperationException("The recorded action or current company could not be confirmed.");
        return result.StepId;
    }

    public async Task<CAVerificationReview> ReviewVerificationAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException();
        var result = await _apiClient.GetAsync<CAVerificationReview>(BuildIncidentEndpoint(incidentId, "cas/verification-review"), cancellationToken)
            ?? throw new InvalidOperationException("Verification evidence is unavailable.");
        result.Require();
        if (snapshot != _apiClient.CompanySnapshot || result.Context != context || result.IncidentId != incidentId)
            throw new InvalidOperationException("The company, FIELD or incident changed during verification review.");
        return result;
    }

    public async Task<CAVerificationDecision> RecordVerificationAsync(CAVerificationDecision original, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); var context = original.Review.Context;
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException();
        var result = await _apiClient.PostAsync<CAVerificationDecision, CAVerificationDecision>(BuildIncidentEndpoint(original.Review.IncidentId, "cas/verification-decision"), original, cancellationToken);
        if (snapshot != _apiClient.CompanySnapshot || result is null || System.Text.Json.JsonSerializer.Serialize(result) != System.Text.Json.JsonSerializer.Serialize(original))
            throw new InvalidOperationException("The recorded verification could not be confirmed.");
        return result;
    }

    public async Task<IncidentClosureReview> ReviewClosureAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required.");
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException();
        var result = await _apiClient.GetAsync<IncidentClosureReview>(BuildIncidentEndpoint(incidentId, "closure-review"), cancellationToken)
            ?? throw new InvalidOperationException("Incident closure evidence is unavailable.");
        result.Require();
        if (snapshot != _apiClient.CompanySnapshot || result.Actions.Context != context || result.Actions.IncidentId != incidentId)
            throw new InvalidOperationException("The incident closure evidence belongs to another company, account or FIELD.");
        return result;
    }

    public async Task<CACompletionReview> ReviewCompletionAsync(FieldSelectionReview context, string incidentId, int stepSequence, CancellationToken cancellationToken = default)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(incidentId) || stepSequence <= 0) throw new InvalidOperationException("An exact action is required.");
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException();
        var result = await _apiClient.GetAsync<CACompletionReview>(BuildIncidentEndpoint(incidentId, $"cas/{stepSequence}/completion-review"), cancellationToken);
        if (snapshot != _apiClient.CompanySnapshot || result is null || result.Context != context || result.IncidentId != incidentId || result.StepSequence != stepSequence ||
            !LeaseSummaryValidator.Identifier(result.PlanId) || !LeaseSummaryValidator.Identifier(result.StepId) || !LeaseSummaryValidator.Identifier(result.ResponsiblePerson) ||
            string.IsNullOrWhiteSpace(result.Description) || result.DueDate.Kind != DateTimeKind.Utc || !ProcessDefinitionRevision.IsValid(result.Revision))
            throw new InvalidOperationException("The current action review could not be confirmed.");
        return result;
    }

    public async Task CompleteCorrectiveActionAsync(CACompletionReview review, string notes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(review); var context = review.Context;
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        if (!LeaseSummaryValidator.Identifier(review.IncidentId) || !LeaseSummaryValidator.Identifier(review.PlanId) || !LeaseSummaryValidator.Identifier(review.StepId) ||
            !ProcessDefinitionRevision.IsValid(review.Revision) || review.StepSequence <= 0 || string.IsNullOrWhiteSpace(notes) || notes.Length > 2000)
            throw new InvalidOperationException("Exact reviewed action and completion evidence are required.");
        var snapshot = _apiClient.CompanySnapshot; var selected = snapshot.Selection ?? throw new InvalidOperationException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException();
        var result = await _apiClient.PostAsync<CompleteCASubmission, CompleteCAReceipt>(BuildIncidentEndpoint(review.IncidentId, $"cas/{review.StepSequence}/complete"), new(review, notes), cancellationToken);
        if (snapshot != _apiClient.CompanySnapshot || result != new CompleteCAReceipt(context, review.IncidentId, review.PlanId, review.StepSequence, review.StepId, notes))
            throw new InvalidOperationException("The recorded completion could not be confirmed.");
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
            return result ?? throw new InvalidOperationException("The HAZOP node population was unavailable.");
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

    public async Task UpdateHazopDeviationStatusAsync(string studyId, int nodeSeq, int condSeq, string status, CancellationToken cancellationToken = default)
    {
        if (nodeSeq <= 0)
            throw new ArgumentOutOfRangeException(nameof(nodeSeq), "Node sequence must be positive.");
        if (condSeq <= 0)
            throw new ArgumentOutOfRangeException(nameof(condSeq), "Deviation sequence must be positive.");
        if (string.IsNullOrWhiteSpace(status))
            throw new ArgumentException("Deviation status is required.", nameof(status));

        try
        {
            await _apiClient.PutAsync(
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

    private static string BuildIncidentReadEndpoint(string incidentId, string? suffix = null)
    {
        if (string.IsNullOrWhiteSpace(incidentId)) throw new ArgumentException("Incident ID is required.", nameof(incidentId));
        return $"/api/field/current/hse/incident{(suffix is null ? "" : "/" + suffix)}?incidentId={Uri.EscapeDataString(incidentId)}";
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
