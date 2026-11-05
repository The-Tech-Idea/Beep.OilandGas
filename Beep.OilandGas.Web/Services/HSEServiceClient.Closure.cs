using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;

public sealed partial class HSEServiceClient
{
    public async Task<IncidentClosureResolutionReview> ReviewClosureResolutionAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        RequireClosureCompany(context); if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required."); var snapshot = _apiClient.CompanySnapshot;
        var review = await _apiClient.GetAsync<IncidentClosureResolutionReview>("/api/field/current/hse/workflow/closure/resolution-review?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken) ?? throw new InvalidOperationException("The native history resolution review is unavailable."); review.Require();
        if (snapshot != _apiClient.CompanySnapshot || review.Context != context || review.IncidentId != incidentId) throw new InvalidOperationException("The native resolution review belongs to another company, account or FIELD."); return review;
    }
    public async Task<IncidentClosedReceipt> ResolveClosureAsync(IncidentClosureResolution original, CancellationToken cancellationToken = default)
    {
        original.Require(); RequireClosureCompany(original.Review.Context); var snapshot = _apiClient.CompanySnapshot;
        var receipt = await _apiClient.PostAsync<IncidentClosureResolution, IncidentClosedReceipt>("/api/field/current/hse/workflow/closure/resolve-native-history", original, cancellationToken) ?? throw new InvalidOperationException("The native resolution outcome is unconfirmed."); receipt.Require();
        if (snapshot != _apiClient.CompanySnapshot || JsonSerializer.Serialize(receipt.Resolution) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The resolved receipt differs from the original native history review."); return receipt;
    }
    public async Task<CompanyHseClosureReview> ReviewCorrectionReturnAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        RequireClosureCompany(context); if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required."); var snapshot = _apiClient.CompanySnapshot;
        var result = await _apiClient.GetAsync<CompanyHseClosureReview>("/api/field/current/hse/workflow/closure/correction-review?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken) ?? throw new InvalidOperationException("The correction return review is unavailable."); result.RequireCorrectionReview();
        if (snapshot != _apiClient.CompanySnapshot || result.Context != context || result.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The rejected verification review belongs to another scope."); return result;
    }
    public async Task<CompanyHseCorrectionReceipt> ReturnForCorrectionAsync(CompanyHseCorrectionCommand original, CancellationToken cancellationToken = default)
    {
        original.Require(); RequireClosureCompany(original.Review.Context); var snapshot = _apiClient.CompanySnapshot;
        var receipt = await _apiClient.PostAsync<CompanyHseCorrectionCommand, CompanyHseCorrectionReceipt>("/api/field/current/hse/workflow/closure/return-for-correction", original, cancellationToken) ?? throw new InvalidOperationException("The correction return outcome is unconfirmed."); receipt.Require();
        if (snapshot != _apiClient.CompanySnapshot || JsonSerializer.Serialize(receipt.Original) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The correction return differs from its original."); return receipt;
    }
    private void RequireClosureCompany(FieldSelectionReview context)
    {
        FieldSelectionReviewValidator.RequireReview(context, context.Company, context.ActorUserId);
        var selected = _apiClient.CompanySnapshot.Selection ?? throw new UnauthorizedAccessException("Select a company.");
        if (selected.Scope != context.Company || selected.UserId != context.ActorUserId) throw new UnauthorizedAccessException("The closure belongs to another company or account.");
    }
    public async Task<CompanyHseClosureReview> ReviewClosureStageAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        RequireClosureCompany(context); if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required."); var snapshot = _apiClient.CompanySnapshot;
        var result = await _apiClient.GetAsync<CompanyHseClosureReview>("/api/field/current/hse/workflow/closure/review?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken) ?? throw new InvalidOperationException("The lifecycle closure review is unavailable.");
        result.Require();
        if (snapshot != _apiClient.CompanySnapshot || result.Context != context || result.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The lifecycle closure review scope changed.");
        return result;
    }
    public async Task<CompanyHseClosureOutcome> CompleteClosureStageAsync(CompanyHseClosureCommand original, CancellationToken cancellationToken = default)
    {
        original.Require(); RequireClosureCompany(original.Review.Context); var snapshot = _apiClient.CompanySnapshot;
        var receipt = await _apiClient.PostAsync<CompanyHseClosureCommand, CompanyHseClosureReceipt>("/api/field/current/hse/workflow/closure/complete-stage", original, cancellationToken) ?? throw new InvalidOperationException("The lifecycle approval is unconfirmed.");
        receipt.Require(original);
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The company changed after lifecycle approval.");
        RequireClosureCompany(original.Review.Context);
        var result = original.Review.Stage == "INC_CLOSURE"
            ? await _apiClient.PostAsync<CompanyHseClosureRecovery, CompanyHseClosureOutcome>("/api/field/current/hse/workflow/closure/record-native", new(original, receipt), cancellationToken) ?? throw new InvalidOperationException("The native incident CLOSED status is unconfirmed.")
            : new CompanyHseClosureOutcome(original, receipt, null);
        result.Require();
        if (JsonSerializer.Serialize(result.Lifecycle) != JsonSerializer.Serialize(receipt)) throw new InvalidOperationException("The native closure differs from the recorded lifecycle approval.");
        if (snapshot != _apiClient.CompanySnapshot || JsonSerializer.Serialize(result.Original) != JsonSerializer.Serialize(original)) throw new InvalidOperationException("The confirmed closure differs from the original.");
        return result;
    }
    public async Task<CompanyHseClosureRecovery?> RecoverClosureStageAsync(FieldSelectionReview context, string incidentId, CancellationToken cancellationToken = default)
    {
        RequireClosureCompany(context); if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("An exact incident is required."); var snapshot = _apiClient.CompanySnapshot;
        CompanyHseClosureRecovery? result;
        try { result = await _apiClient.GetAsync<CompanyHseClosureRecovery>("/api/field/current/hse/workflow/closure/recovery?incidentId=" + Uri.EscapeDataString(incidentId), cancellationToken); }
        catch (OilGasApiException error) when (error.StatusCode == System.Net.HttpStatusCode.NotFound)
        { if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The selected closure company changed during recovery."); return null; }
        if (snapshot != _apiClient.CompanySnapshot) throw new InvalidOperationException("The selected closure company changed.");
        if (result is null) throw new InvalidOperationException("The closure recovery response is unavailable."); result.Require(); var prior = result.Original.Review.Context;
        if (prior.Company != context.Company || prior.ActorUserId != context.ActorUserId || prior.Database != context.Database || prior.FieldId != context.FieldId || result.Original.Review.Evidence.Actions.IncidentId != incidentId) throw new InvalidOperationException("The recovered closure belongs to another account, FIELD or incident.");
        return result;
    }
}
