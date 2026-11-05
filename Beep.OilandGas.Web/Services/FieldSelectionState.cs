using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

// UI evidence for the existing profile-owned preference, never an authorization or selection store.
public sealed class FieldSelectionState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IFieldSelectionContext data;
    private readonly Func<Exception, string, string, string> explain;
    private CancellationTokenSource? pending;
    private long generation;
    private bool disposed;
    private FieldSelectionReview? review;
    private string? pendingField;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private readonly Dictionary<FieldListItem, string> identities = new(ReferenceEqualityComparer.Instance);
    public IReadOnlyList<FieldListItem> Fields { get; private set; } = [];
    public string? SelectedId { get; private set; }
    public bool Loading { get; private set; }
    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public bool NeedsCompany => company.Selection is null;
    public string? Error { get; private set; }
    public string? Confirmation { get; private set; }
    public event Action? Changed;
    public FieldSelectionState(CompanyClientContext company, IFieldSelectionContext data, Func<Exception, string, string, string> explain)
    { this.company = company; this.data = data; this.explain = explain; company.Changed += CompanyChanged; data.CurrentFieldChanged += FieldChanged; }

    public async Task LoadAsync()
    {
        if (disposed || Saving) return;
        Invalidate(); var version = generation; var captured = company.Snapshot;
        if (captured.Selection is null) return;
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Loading = true;
        try
        {
            var fields = await data.GetAvailableFieldsAsync(cancellation.Token);
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCatalog(fields);
            var snapshots = fields.Select(FieldSelectionReviewValidator.Snapshot).ToArray();
            var selected = await data.GetCurrentFieldSelectionAsync(cancellation.Token);
            if (!Current(version, captured)) return;
            FieldSelectionReviewValidator.RequireCurrent(selected, captured.Selection.Scope, captured.Selection.UserId);
            if (selected.SelectionAvailable && !snapshots.Any(row => row.FieldId == selected.FieldId))
                throw new InvalidOperationException("The selected FIELD is absent from the reviewed catalog. Refresh the selector.");
            review = selected.SelectionContext; loadedScope = captured;
            Fields = snapshots.OrderBy(row => row.FieldName, StringComparer.CurrentCultureIgnoreCase).ToArray();
            foreach (var row in Fields) identities.Add(row, row.FieldId);
            SelectedId = selected.SelectionAvailable ? selected.FieldId : null; Loaded = true;
            if (!selected.SelectionAvailable && review!.FieldId is not null)
                Error = "The saved FIELD is no longer available. Choose an available FIELD.";
        }
        catch (OperationCanceledException) when (!Current(version, captured)) { }
        catch (Exception error) { if (Current(version, captured)) Error = explain(error, "loading the FIELD selector", "FIELD settings could not be confirmed"); }
        finally { if (Current(version, captured)) { pending = null; Loading = false; Changed?.Invoke(); } }
    }

    public async Task SelectAsync(string id)
    {
        if (disposed || Loading || Saving || !Loaded || loadedScope != company.Snapshot || review is null || id == SelectedId ||
            !Fields.Any(row => identities.TryGetValue(row, out var trusted) && trusted == row.FieldId && trusted == id)) return;
        var captured = company.Snapshot; var version = generation;
        var intent = new SetActiveFieldRequest { FieldId = id, Context = review with { } };
        Saving = true; pendingField = id; Error = null; Confirmation = null;
        try
        {
            var success = await data.SetCurrentFieldAsync(intent);
            if (!Current(version, captured)) return;
            // The owner validates the acknowledgement. Its event alone can never confirm this command.
            if (!success) throw new InvalidOperationException("The FIELD change was not confirmed.");
            ClearEvidence(); SelectedId = id; Confirmation = "FIELD preference saved. Refresh to review another change.";
        }
        catch (Exception error)
        {
            if (Current(version, captured)) { ClearEvidence(); Error = explain(error, "saving the FIELD preference", "The FIELD change could not be confirmed. Refresh before continuing"); }
        }
        finally { Saving = false; pendingField = null; if (!disposed) Changed?.Invoke(); }
    }
    private void CompanyChanged() { if (disposed) return; Invalidate(); Changed?.Invoke(); }
    private void FieldChanged(string id)
    {
        if (disposed) return;
        if (Saving && id == pendingField) return; // A matching owner event alone cannot confirm this command.
        if (Saving && string.IsNullOrEmpty(id)) { ClearEvidence(); return; } // Its eventual failure supplies the explanation.
        Invalidate(); Error = "Refresh to review the current FIELD preference."; Changed?.Invoke();
    }
    private bool Current(long version, (long Version, CompanyClientSelection? Selection) captured) => !disposed && generation == version && company.Snapshot == captured;
    private void ClearEvidence() { Fields = []; identities.Clear(); review = null; loadedScope = null; SelectedId = null; Loaded = false; }
    private void Invalidate()
    {
        ++generation; pending?.Cancel(); pending = null; ClearEvidence(); Loading = false; Error = null; Confirmation = null;
    }
    public void Dispose() { if (disposed) return; disposed = true; company.Changed -= CompanyChanged; data.CurrentFieldChanged -= FieldChanged; Invalidate(); }
}
