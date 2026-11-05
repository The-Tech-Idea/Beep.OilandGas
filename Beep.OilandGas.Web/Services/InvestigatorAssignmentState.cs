using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

// One presentation session's reviewed selection and retained uncertain outcome; never a permission/intent store.
public sealed class InvestigatorAssignmentState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IFieldSelectionContext fields;
    private readonly IHSEServiceClient api;
    private readonly IInvestigatorAssignmentIntentStore store;
    private readonly Func<Exception, string, string, string> explain;
    private readonly Dictionary<Target, InvestigatorAssignmentIntent> pending = [];
    private readonly HashSet<RecordedIncidentParticipant> choices = new(ReferenceEqualityComparer.Instance);
    private sealed record Target(CompanyScope Company, string Actor, string Database, string Field, string Incident);
    private InvestigatorAssignmentReview? review;
    private FieldSelectionReview? fieldReview;
    private Target? target;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private CancellationTokenSource? reading;
    private long generation;
    private bool disposed;
    public IReadOnlyList<RecordedIncidentParticipant> Participants { get; private set; } = [];
    public RecordedIncidentParticipant? CurrentInvestigator { get; private set; }
    public RecordedIncidentParticipant? Selected { get; private set; }
    public string IncidentId { get; private set; } = "";
    public string FieldId { get; private set; } = "";
    public bool Loading { get; private set; }
    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool NeedsCompany => company.Selection is null;
    public bool NeedsField { get; private set; }
    public bool OutcomeReviewRequired => target is not null && pending.ContainsKey(target);
    public string? Error { get; private set; }
    public string? Message { get; private set; }
    public bool CanAssign => !disposed && !Loading && !Saving && Loaded && !OutcomeReviewRequired && loadedScope == company.Snapshot &&
        Selected is not null && choices.Contains(Selected) && company.Selection?.Permissions.Contains(PermissionConstants.HSE.ManageIncidents, StringComparer.Ordinal) == true;
    public event Action? Changed;
    public InvestigatorAssignmentState(CompanyClientContext company, IFieldSelectionContext fields, IHSEServiceClient api, Func<Exception, string, string, string> explain,IInvestigatorAssignmentIntentStore store)
    {
        this.company = company; this.fields = fields; this.api = api; this.explain = explain; this.store=store;
        company.Changed += CompanyChanged; fields.CurrentFieldChanged += FieldChanged;
    }
    public bool Select(RecordedIncidentParticipant? participant)
    {
        if (!Loaded || Loading || Saving || OutcomeReviewRequired || loadedScope != company.Snapshot || participant is not null && !choices.Contains(participant)) return false;
        Selected = participant; Error = null; Changed?.Invoke(); return true;
    }
    public async Task LoadAsync(string incidentId)
    {
        if (disposed) return;
        if (Saving)
        {
            if (IncidentId != incidentId) { Invalidate(); IncidentId = incidentId; Changed?.Invoke(); }
            return;
        }
        Invalidate(); IncidentId = incidentId;
        var version = generation; var captured = company.Snapshot;
        if (captured.Selection is null) { Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); reading = cancellation; Loading = true; Changed?.Invoke();
        try
        {
            if (!LeaseSummaryValidator.Identifier(incidentId)) throw new InvalidOperationException("Select an exact incident.");
            var selected = await fields.GetCurrentFieldSelectionAsync(cancellation.Token);
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCurrent(selected, captured.Selection.Scope, captured.Selection.UserId);
            if (!selected.SelectionAvailable) { NeedsField = true; return; }
            FieldId = selected.FieldId; fieldReview = selected.SelectionContext!;
            target = new(captured.Selection.Scope, captured.Selection.UserId, fieldReview.Database.Fingerprint, FieldId, incidentId);
            var retained=await store.ReadAsync(fieldReview,incidentId);
            if(!Current(version,captured))return;
            if(retained is not null)pending[target]=retained;
            var returned = await api.ReviewInvestigatorAssignmentAsync(incidentId, FieldId, cancellation.Token);
            if (!Current(version, captured)) return;
            var currentField = await fields.GetCurrentFieldSelectionAsync(cancellation.Token);
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCurrent(currentField, captured.Selection.Scope, captured.Selection.UserId);
            if (!currentField.SelectionAvailable || currentField.SelectionContext != fieldReview)
                throw new InvalidOperationException("The FIELD selection changed during investigator review.");
            InvestigatorAssignmentReviewValidator.Require(returned, captured.Selection.Scope, captured.Selection.UserId, incidentId, FieldId);
            if (returned.Context != fieldReview || returned.CurrentInvestigator is not null && returned.CurrentInvestigator.Role != HseReferenceCodes.Investigator.Role)
                throw new InvalidOperationException("The investigator review differs from the reviewed FIELD selection or current role.");
            review = returned with { Participants = Array.AsReadOnly(returned.Participants.Select(row => row with { }).ToArray()), CurrentInvestigator = returned.CurrentInvestigator is null ? null : returned.CurrentInvestigator with { } };
            Participants = review.Participants; foreach (var participant in Participants) choices.Add(participant);
            CurrentInvestigator = review.CurrentInvestigator; loadedScope = captured; Loaded = true;
            if (pending.TryGetValue(target, out var intent))
            {
                if (CurrentInvestigator?.BusinessAssociateId == intent.Participant.BusinessAssociateId && CurrentInvestigator.PartyObservation == intent.Participant.PartyObservation)
                {
                    await store.ClearAsync(intent);if(!Current(version,captured))return;
                    pending.Remove(target); Message = "The currently recorded investigator matches the submitted participant.";
                }
                else if (review.Revision != intent.Review.Revision)
                {
                    Message = "The recorded roster or authority changed. The earlier submission remains unconfirmed; reconcile it before another change.";
                }
                else Message = "No roster change has been observed. The submission remains unconfirmed; review the recorded outcome again before another change.";
            }
        }
        catch (OperationCanceledException) when (!Current(version, captured)) { }
        catch (Exception error)
        {
            if (Current(version, captured)) { ClearEvidence(); Error = explain(error, "reviewing the incident investigator", "Investigator records could not be confirmed"); }
        }
        finally { if (Current(version, captured)) { reading = null; Loading = false; Changed?.Invoke(); } }
    }
    public async Task AssignAsync()
    {
        if (!CanAssign || review is null || fieldReview is null || target is null) return;
        var original = new InvestigatorAssignmentIntent(review with { Participants = Array.AsReadOnly(review.Participants.ToArray()) }, Selected! with { });
        var key = target; var version = generation; var captured = company.Snapshot;
        pending.Add(key, original); Saving = true; Error = null; Message = null; Changed?.Invoke(); var attempted = false;
        try
        {
            var selected = await fields.GetCurrentFieldSelectionAsync();
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCurrent(selected, captured.Selection!.Scope, captured.Selection.UserId);
            if (!selected.SelectionAvailable || selected.SelectionContext != original.Review.Context)
                throw new InvalidOperationException("The FIELD selection changed before submission. Review the current records.");
            await store.RetainAsync(original);
            if(!Current(version,captured))return;
            selected = await fields.GetCurrentFieldSelectionAsync();
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCurrent(selected, captured.Selection!.Scope, captured.Selection.UserId);
            if (!selected.SelectionAvailable || selected.SelectionContext != original.Review.Context)
                throw new InvalidOperationException("The FIELD selection changed before submission. Review the current records.");
            attempted = true;
            await api.AssignInvestigatorAsync(original.Review, original.Participant);
            if (Current(version, captured)) { ClearEvidence(); Message = "Submission acknowledged. Review the recorded outcome before further changes."; }
        }
        catch (Exception error)
        {
            if (Current(version, captured)) { ClearEvidence(); Error = explain(error, "submitting the investigator selection", attempted ? "The assignment is unconfirmed; review its recorded outcome before another change" : "The investigator selection changed before submission. Review the current records"); }
        }
        finally
        {
            if (!attempted) pending.Remove(key);
            Saving = false; if (!disposed) Changed?.Invoke();
        }
    }
    private bool Current(long version, (long Version, CompanyClientSelection? Selection) captured) => !disposed && generation == version && company.Snapshot == captured;
    private void ClearEvidence() { Participants = []; choices.Clear(); Selected = null; CurrentInvestigator = null; review = null; loadedScope = null; Loaded = false; }
    private void Invalidate()
    {
        ++generation; reading?.Cancel(); reading = null; Loading = false; ClearEvidence(); fieldReview = null; target = null;
        FieldId = ""; NeedsField = false; Error = null; Message = null; // Saving and retained pending outcomes survive context changes.
    }
    private void CompanyChanged() { if (disposed) return; Invalidate(); Changed?.Invoke(); }
    private void FieldChanged(string _) { if (disposed) return; Invalidate(); Changed?.Invoke(); }
    public void Dispose() { if (disposed) return; disposed = true; company.Changed -= CompanyChanged; fields.CurrentFieldChanged -= FieldChanged; Invalidate(); }
}
