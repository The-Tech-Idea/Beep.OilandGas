using System.Net;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Web.Services;

public sealed class FieldSelectionState : IDisposable
{
    private readonly ApiClient _api;
    private readonly IDataManagementService _data;
    private readonly OilGasCallFailures _failures;
    private int _version;
    private bool _disposed;
    public List<FieldListItem> Fields { get; private set; } = [];
    public string? SelectedId { get; private set; }
    public bool Loading { get; private set; }
    public bool Saving { get; private set; }
    public bool Loaded { get; private set; }
    public string? Error { get; private set; }
    public string? Confirmation { get; private set; }
    public event Action? Changed;
    public FieldSelectionState(ApiClient api, IDataManagementService data, OilGasCallFailures failures)
    {
        _api = api; _data = data; _failures = failures; _data.CurrentFieldChanged += FieldChanged;
    }
    public async Task LoadAsync()
    {
        if (_disposed || Saving) return;
        var version = ++_version;
        Loading = true; Loaded = false; Error = null; Confirmation = null; Fields = []; SelectedId = null;
        try
        {
            var fields = await _api.GetAsync<List<FieldListItem>>("/api/field/fields") ?? throw new InvalidOperationException();
            if (fields.Any(x => string.IsNullOrWhiteSpace(x.FieldId)) || fields.Select(x => x.FieldId).Distinct().Count() != fields.Count)
                throw new InvalidOperationException("Field identities are incomplete or ambiguous.");
            var selected = await _data.GetCurrentFieldIdAsync();
            if (!Current(version)) return;
            Fields = fields.OrderBy(x => x.FieldName, StringComparer.CurrentCultureIgnoreCase).ToList();
            SelectedId = selected; Loaded = true;
            if (!string.IsNullOrWhiteSpace(selected) && !fields.Any(x => x.FieldId == selected))
            { SelectedId = null; Error = "The previous field is no longer available. Choose an available field."; }
        }
        // The selector renders a sentence for every failure of this load; the store keeps the exception (a superseded
        // load's too, though nobody is shown it).
        catch (Exception ex)
        {
            var sentence = Refusal(ex) is { } own
                ? _failures.Told(ex, "loading the available fields", own)
                : _failures.Explain(ex, "loading the available fields", "Field settings could not be loaded");
            if (Current(version)) Error = sentence;
        }
        finally { if (Current(version)) Loading = false; }
    }
    public async Task SelectAsync(string id)
    {
        if (_disposed || Loading || Saving || !Loaded || !Fields.Any(x => x.FieldId == id) || id == SelectedId) return;
        Saving = true; Error = null; Confirmation = null;
        try
        {
            var success = await _data.SetCurrentFieldAsync(id);
            if (_disposed) return;
            if (success) { SelectedId = id; Confirmation = "Active field updated."; }
            else Error = "The field was not changed. Choose another field or refresh the available fields.";
        }
        // The selector renders a sentence for every failure of this change; the store keeps the exception.
        catch (Exception ex)
        {
            var sentence = Refusal(ex) is { } own
                ? _failures.Told(ex, "changing the active field", own)
                : _failures.Explain(ex, "changing the active field", "The field change could not be confirmed. Refresh before continuing");
            if (!_disposed) { SelectedId = null; Loaded = false; Error = sentence; }
        }
        finally { if (!_disposed) Saving = false; }
    }
    private void FieldChanged(string id)
    {
        if (_disposed) return;
        ++_version; Loading = false; SelectedId = string.IsNullOrWhiteSpace(id) ? null : id;
        if (SelectedId is null || !Loaded)
        {
            Loaded = false;
            Error = "Refresh the available fields to confirm the active field.";
        }
        Confirmation = null;
        Changed?.Invoke();
    }
    private bool Current(int version) => !_disposed && version == _version;

    /// <summary>The selector's own words for a refusal it recognises; null for anything else.</summary>
    private static string? Refusal(Exception ex) => ex switch
    {
        OilGasApiException { StatusCode: HttpStatusCode.Forbidden } => "You do not have access to these field settings. Contact your app administrator.",
        OilGasApiException { StatusCode: HttpStatusCode.Unauthorized } => "Your session has expired. Sign in again to choose a field.",
        _ => null,
    };
    public void Dispose() { _disposed = true; ++_version; _data.CurrentFieldChanged -= FieldChanged; Fields = []; SelectedId = null; }
}
