using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;

namespace Beep.OilandGas.Client.App.Services.Lease;

// Actual API contracts only, through the existing company-captured transport.
internal sealed class LeaseService(ClientApiTransport api, CompanyClientContext company) : ILeaseService
{
    private static string Root(string field) { Identifier(field); return "/api/fields/" + Uri.EscapeDataString(field) + "/lease-acquisition"; }
    public async Task<LeaseSummary> EvaluateLeaseAsync(string leaseId, string fieldId, CancellationToken cancellationToken = default)
    {
        Identifier(leaseId);
        var row = Required(await api.GetAsync<LeaseSummary>(Root(fieldId) + "/evaluate/" + Uri.EscapeDataString(leaseId), cancellationToken));
        Validate(row);
        if (row.LeaseId != leaseId || row.FieldId != fieldId) throw Invalid();
        return row;
    }
    public async Task<List<LeaseSummary>> GetAvailableLeasesAsync(Dictionary<string, string>? filters = null, CancellationToken cancellationToken = default)
    {
        var selected = filters is null ? new Dictionary<string, string>() : new Dictionary<string, string>(filters, StringComparer.Ordinal);
        var allowed = new[] { "FieldId", "PropertyId", "LeaseId", "LeaseNumber", "LeaseName", "Status" };
        if (selected.Any(pair => !allowed.Contains(pair.Key, StringComparer.Ordinal) || !Exact(pair.Value))) throw new ArgumentException("Use exact supported lease filters.");
        selected.TryGetValue("FieldId", out var field); selected.TryGetValue("PropertyId", out var property); selected.TryGetValue("LeaseId", out var id);
        if (field is not null && property is not null && field != property || field is null && property is null)
            throw new ArgumentException("Select an exact FIELD.");
        field ??= property;
        var endpoint = Root(field!); selected.Remove("FieldId"); selected.Remove("PropertyId");
        var query = string.Join("&", selected.Select(pair => Uri.EscapeDataString("filters[" + pair.Key + "]") + "=" + Uri.EscapeDataString(pair.Value)));
        var rows = Required(await api.GetAsync<List<LeaseSummary>>(endpoint + "/available" + (query.Length == 0 ? "" : "?" + query), cancellationToken));
        foreach (var row in rows)
        {
            Validate(row);
            if (field is not null && row.FieldId != field || id is not null && row.LeaseId != id ||
                selected.TryGetValue("LeaseNumber", out var number) && row.LeaseNumber != number ||
                selected.TryGetValue("LeaseName", out var name) && row.LeaseName != name ||
                selected.TryGetValue("Status", out var status) && row.Status != status) throw Invalid();
        }
        if (rows.Select(row => (row.LandRightSubtype, row.LeaseId)).Distinct().Count() != rows.Count) throw Invalid();
        return rows;
    }
    public async Task<LeaseCreationContext> GetCreationContextAsync(string fieldId, CancellationToken cancellationToken = default)
    {
        Identifier(fieldId); var captured = company.Snapshot; var selection = captured.Selection ?? throw new InvalidOperationException("Select a company before reviewing a creation context.");
        var context = Required(await api.GetAsync<LeaseCreationContext>(Root(fieldId) + "/creation-context", cancellationToken));
        EnsureCurrent(captured);
        Validate(context, selection, fieldId);
        return context;
    }
    public Task<LeaseCreationReceipt> CreateLeaseAcquisitionAsync(CreateLeaseAcquisition request, CancellationToken cancellationToken = default)
        => CreationAsync(request, false, cancellationToken);
    public Task<LeaseCreationReceipt> ReconcileLeaseCreationAsync(CreateLeaseAcquisition request, CancellationToken cancellationToken = default)
        => CreationAsync(request, true, cancellationToken);
    private async Task<LeaseCreationReceipt> CreationAsync(CreateLeaseAcquisition request, bool reconcile, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request); var intent = request.Snapshot();
        if (intent.CommandId == Guid.Empty || intent.Context is null || intent.PropertyId is null) throw new ArgumentException("Review a stable creation intent before sending it.");
        var captured = company.Snapshot; var selection = captured.Selection ?? throw new InvalidOperationException("Select the original company before sending a creation intent.");
        Validate(intent.Context, selection, intent.PropertyId);
        var receipt = Required(await api.PostAsync<CreateLeaseAcquisition, LeaseCreationReceipt>(Root(intent.PropertyId) + (reconcile ? "/creation-reconciliation" : ""), intent, token));
        EnsureCurrent(captured);
        if (receipt.CommandId != intent.CommandId || receipt.Context != intent.Context ||
            receipt.State is not ("Recorded" or "NotRecorded") || !reconcile && receipt.State != "Recorded" ||
            (receipt.State == "Recorded" ? receipt.LeaseId != intent.CommandId.ToString("N") : receipt.LeaseId is not null)) throw Invalid();
        return receipt;
    }
    public async Task UpdateLeaseStatusAsync(string leaseId, string status, string fieldId, CancellationToken cancellationToken = default)
    {
        Identifier(leaseId);
        if (status is not ("ACTIVE" or "INACTIVE" or "PENDING" or "TERMINATED")) throw new ArgumentException("Use a canonical lease status.");
        await api.PutAsync<UpdateLeaseStatusRequest, JsonElement>(Root(fieldId) + "/" + Uri.EscapeDataString(leaseId) + "/status", new() { Status = status }, cancellationToken);
    }
    private void EnsureCurrent((long Version, CompanyClientSelection? Selection) captured)
    { if (company.Snapshot != captured) throw new OperationCanceledException("The creation intent's account or company changed."); }
    private static void Validate(LeaseCreationContext context, CompanyClientSelection selection, string field)
    {
        if (context.Company != selection.Scope || context.ActorUserId != selection.UserId || context.FieldId != field ||
            !Exact(context.DatabaseScopeFingerprint) || !Exact(context.ActorUserId) || !Exact(context.FieldId) ||
            !Exact(context.LandRightSubtype) || !Exact(context.CompanyComponentType) || !Exact(context.StatusType) || !Zone(context.DateZoneId)) throw Invalid();
    }
    private static void Validate(LeaseSummary? row)
    {
        try { LeaseSummaryValidator.RequireValid(row); }
        catch (InvalidOperationException error) { throw new ApiClientException("The lease API returned inconsistent recorded facts.", error); }
    }
    private static bool Zone(string? id)
    { try { return Exact(id) && TimeZoneInfo.FindSystemTimeZoneById(id!) is not null; } catch (TimeZoneNotFoundException) { return false; } catch (InvalidTimeZoneException) { return false; } }
    private static bool Exact(string? value) => !string.IsNullOrWhiteSpace(value) && value == value.Trim() && !value.Any(char.IsControl);
    private static void Identifier(string value) { if (!Exact(value) || value.Length > 128) throw new ArgumentException("An exact identifier is required."); }
    private static T Required<T>(T? value) where T : class => value ?? throw Invalid();
    private static ApiClientException Invalid() => new("The lease API returned missing or inconsistent registration evidence.");
}
