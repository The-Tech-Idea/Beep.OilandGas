using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Constants;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Client.App.Services.Field;

// The existing Field facade's HSE HTTP operations, never a local datasource path.
internal sealed class HseIncidentClient(ClientApiTransport api, CompanyClientContext company)
{
    private static string Endpoint(string incident, string action)
    {
        if (!LeaseSummaryValidator.Identifier(incident)) throw new ArgumentException("Select an exact incident.");
        return "/api/field/current/hse/incident/" + action + "?incidentId=" + Uri.EscapeDataString(incident);
    }
    public async Task<InvestigatorAssignmentReview> ReviewAsync(string incident, string expectedField, CancellationToken cancellationToken = default)
    {
        var captured = company.Snapshot;
        var selection = captured.Selection ?? throw new InvalidOperationException("Select an available company.");
        if (!LeaseSummaryValidator.Identifier(expectedField)) throw new ArgumentException("Select an exact FIELD.");
        var review = await api.GetAsync<InvestigatorAssignmentReview>(Endpoint(incident, "investigator-review"), cancellationToken);
        EnsureCurrent(captured);
        Validate(review, selection, incident, expectedField);
        return review with { Participants = Array.AsReadOnly(review.Participants.ToArray()) };
    }
    public async Task AssignAsync(InvestigatorAssignmentReview review, RecordedIncidentParticipant participant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(review);
        var captured = company.Snapshot;
        var selection = captured.Selection ?? throw new InvalidOperationException("Select an available company.");
        Validate(review, selection, review.IncidentId, review.Context?.FieldId!);
        // Freeze the original reviewed body before authentication/transport awaits. Never fetch a fresh revision automatically.
        var request = InvestigatorAssignmentReviewValidator.Select(review, participant);
        await api.PutAsync<AssignIncidentInvestigatorRequest, object>(Endpoint(review.IncidentId, "investigator"), request, cancellationToken);
        EnsureCurrent(captured);
    }
    public async Task<CAPlanCreationReview> ReviewPlanCreationAsync(string incident, string expectedField, CancellationToken cancellationToken = default)
    {
        var captured = company.Snapshot;
        var selection = captured.Selection ?? throw new InvalidOperationException("Select an available company.");
        if (!LeaseSummaryValidator.Identifier(expectedField)) throw new ArgumentException("Select an exact FIELD.");
        var review = await api.GetAsync<CAPlanCreationReview>(Endpoint(incident, "ca-plan-review"), cancellationToken);
        EnsureCurrent(captured);
        if (review is null) throw new ApiClientException("The plan creation review is absent.");
        InvestigatorAssignmentReviewValidator.Require(new(review.Context, review.IncidentId, review.Revision, review.Participants, null), selection.Scope, selection.UserId, incident, expectedField);
        if (review.Participants.Count == 0 || review.ResponseTypes is null || review.ResponseTypes.Count is < 1 or > 256 ||
            review.ResponseTypes.Any(value => !LeaseSummaryValidator.Identifier(value) || value == "STATUS") || review.ResponseTypes.Distinct(StringComparer.Ordinal).Count() != review.ResponseTypes.Count)
            throw new ApiClientException("The plan creation review has invalid recorded choices.");
        return review with { Participants = Array.AsReadOnly(review.Participants.ToArray()), ResponseTypes = Array.AsReadOnly(review.ResponseTypes.ToArray()) };
    }

    public async Task<CAPlanResolution> CreatePlanAsync(string incident, CreateCAPlanSubmission submission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        ArgumentNullException.ThrowIfNull(submission.Intent);
        if (submission.Intent.ExpectedRevision is not { Length: 64 } || submission.Intent.ExpectedRevision.Any(value => !char.IsAsciiHexDigit(value) || char.IsAsciiLetterLower(value)))
            throw new ArgumentException("The exact recorded plan creation review revision is required.");
        var captured = company.Snapshot;
        var selection = captured.Selection ?? throw new InvalidOperationException("Select an available company.");
        ValidateContext(submission.Context, selection);
        var frozen = new CreateCAPlanSubmission(submission.Context with { }, submission.Intent with { });
        var result = await api.PostAsync<CreateCAPlanSubmission, CAPlanResolution>(Endpoint(incident, "ca-plan"), frozen, cancellationToken);
        EnsureCurrent(captured);
        ValidatePlan(result, frozen.Context, incident);
        if (result.PlanId != frozen.Intent.PlanId.ToString("N")) throw new ApiClientException("The API did not confirm the submitted plan identity.");
        return result;
    }
    public async Task<CAPlanResolution> GetPlanAsync(string incident, FieldSelectionReview context, CancellationToken cancellationToken = default)
    {
        var captured = company.Snapshot;
        var selection = captured.Selection ?? throw new InvalidOperationException("Select an available company.");
        ValidateContext(context, selection);
        var result = await api.GetAsync<CAPlanResolution>(Endpoint(incident, "ca-plan"), cancellationToken);
        EnsureCurrent(captured);
        ValidatePlan(result, context, incident);
        return result;
    }
    private static void ValidateContext(FieldSelectionReview? context, CompanyClientSelection selection)
    {
        FieldSelectionReviewValidator.RequireReview(context, selection.Scope, selection.UserId);
        if (!LeaseSummaryValidator.Identifier(context!.FieldId)) throw new ArgumentException("An available reviewed FIELD is required.");
    }
    private static void ValidatePlan(CAPlanResolution? result, FieldSelectionReview context, string incident)
    {
        if (result is null || result.Context != context || result.IncidentId != incident || !LeaseSummaryValidator.Identifier(result.PlanId))
            throw new ApiClientException("The API returned a plan outside the reviewed incident/account/company/database/FIELD.");
    }
    private static void Validate(InvestigatorAssignmentReview? review, CompanyClientSelection selection, string incident, string field)
    {
        InvestigatorAssignmentReviewValidator.Require(review, selection.Scope, selection.UserId, incident, field);
        if (review!.CurrentInvestigator is not null && review.CurrentInvestigator.Role != HseReferenceCodes.Investigator.Role)
            throw new ApiClientException("The API returned an inconsistent current investigator role.");
    }
    private void EnsureCurrent((long Version, CompanyClientSelection? Selection) captured)
    {
        if (company.Snapshot != captured) throw new OperationCanceledException("The HSE operation's account/company selection changed.");
    }
}
