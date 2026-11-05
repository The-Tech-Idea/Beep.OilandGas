using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.Lease;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

// Evidence for one explicitly selected FIELD, through the canonical lease API. Never a selection or authority store.
public sealed class LeasePortfolioState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly ILeaseServiceClient api;
    private readonly Func<Exception, string, string, string> explain;
    private CancellationTokenSource? pending;
    private long generation;
    private bool disposed;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private readonly Dictionary<LeaseSummary, (string Id, string Subtype, string Field, string Zone)> identities = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<(string User, CompanyScope Company, string Field, string Id)> uncertainStatus = [];
    public IReadOnlyList<LeaseSummary> Leases { get; private set; } = [];
    public string FieldId { get; private set; } = "";
    public bool Loading { get; private set; }
    public bool Busy { get; private set; }
    public bool Loaded { get; private set; }
    public DateTime? RetrievedAtUtc { get; private set; }
    public bool NeedsField { get; private set; }
    public bool NeedsCompany => company.Selection is null;
    public string? Error { get; private set; }
    public string? CommandError { get; private set; }
    public LeaseSummary? Evaluation { get; private set; }
    public string? Confirmation { get; private set; }
    public bool Current => !disposed && Loaded && !Loading && loadedScope == company.Snapshot;
    public bool HasUnconfirmedStatus => company.Selection is { } actor && uncertainStatus.Any(target => target.User == actor.UserId && target.Company == actor.Scope && target.Field == FieldId);
    public event Action? Changed;
    public LeasePortfolioState(CompanyClientContext company, ILeaseServiceClient api, Func<Exception, string, string, string> explain)
    { this.company = company; this.api = api; this.explain = explain; company.Changed += CompanyChanged; }

    public async Task LoadAsync(Func<Task<string?>> selectedField)
    {
        if (disposed) return;
        Invalidate(); var version = generation; var captured = company.Snapshot;
        if (captured.Selection is null) return;
        using var cancellation = new CancellationTokenSource(); pending = cancellation;
        Loading = true;
        try
        {
            var field = await selectedField();
            if (!IsCurrent(version, captured)) return;
            if (string.IsNullOrEmpty(field)) { NeedsField = true; return; }
            if (!LeaseSummaryValidator.Identifier(field)) throw new InvalidOperationException("The selected FIELD identity is invalid.");
            FieldId = field;
            var rows = await api.GetAvailableLeasesAsync(new() { ["FieldId"] = field }, cancellation.Token);
            if (!IsCurrent(version, captured)) return;
            LeaseSummaryValidator.RequireValid(rows, field);
            var snapshots = rows.Select(LeaseSummaryValidator.Snapshot).ToArray();
            foreach (var row in snapshots) identities.Add(row, (row.LeaseId, row.LandRightSubtype, row.FieldId, row.DateZoneId));
            Leases = snapshots; Loaded = true; loadedScope = captured; RetrievedAtUtc=DateTime.UtcNow;
        }
        catch (OperationCanceledException) when (!IsCurrent(version, captured)) { }
        catch (Exception error) { if (IsCurrent(version, captured)) Error = explain(error, "loading the lease portfolio", "Lease records could not be confirmed for this FIELD"); }
        finally { if (IsCurrent(version, captured)) { pending = null; Loading = false; Changed?.Invoke(); } }
    }
    public bool Owns(LeaseSummary row) => Current && identities.TryGetValue(row, out var key) && key == (row.LeaseId, row.LandRightSubtype, row.FieldId, row.DateZoneId) && row.FieldId == FieldId;
    public bool CanUpdate(LeaseSummary row) => !Busy && Owns(row) && company.Selection is { } actor &&
        actor.Roles.Contains("Administrator", StringComparer.Ordinal) && !uncertainStatus.Contains((actor.UserId, actor.Scope, FieldId, row.LeaseId));
    public async Task EvaluateAsync(LeaseSummary row)
    {
        if (Busy || !Owns(row)) return;
        var key = identities[row]; var version = generation; var captured = company.Snapshot;
        Busy = true; Evaluation = null; CommandError = null;
        try
        {
            var result = await api.EvaluateLeaseAsync(key.Id, key.Field);
            if (!IsCurrent(version, captured) || !Owns(row)) return;
            LeaseSummaryValidator.RequireValid(result, key.Field, key.Id);
            if (result.LandRightSubtype != key.Subtype || result.DateZoneId != key.Zone) throw new InvalidOperationException("The lease interpretation changed.");
            Evaluation = LeaseSummaryValidator.Snapshot(result);
        }
        catch (Exception error) { if (IsCurrent(version, captured)) CommandError = explain(error, "reading the lease record", "The lease record could not be confirmed"); }
        finally { if (IsCurrent(version, captured)) { Busy = false; Changed?.Invoke(); } }
    }
    public async Task UpdateStatusAsync(LeaseSummary row, string status)
    {
        if (!CanUpdate(row)) return;
        var key = identities[row]; var version = generation; var captured = company.Snapshot; var actor = captured.Selection!;
        var target = (actor.UserId, actor.Scope, key.Field, key.Id);
        uncertainStatus.Add(target); // Reserve before the await; context changes must not permit a second in-flight write.
        Busy = true; CommandError = null; Confirmation = null;
        try
        {
            await api.UpdateLeaseStatusAsync(key.Id, status, key.Field);
            uncertainStatus.Remove(target);
            if (!IsCurrent(version, captured) || !Owns(row)) return;
            Confirmation = "The requested status was recorded. Reload the FIELD portfolio for fresh facts.";
            Leases = []; identities.Clear(); Loaded = false; RetrievedAtUtc=null; loadedScope = null; Evaluation = null;
        }
        catch (Exception error)
        {
            uncertainStatus.Add(target);
            if (IsCurrent(version, captured)) CommandError = explain(error, "changing the lease status", "The status change is unconfirmed; review the original FIELD record before another status write");
        }
        finally { if (IsCurrent(version, captured)) { Busy = false; Changed?.Invoke(); } }
    }
    private bool IsCurrent(long version, (long Version, CompanyClientSelection? Selection) captured) => !disposed && generation == version && company.Snapshot == captured;
    private void CompanyChanged() { Invalidate(); NeedsField = company.Selection is not null; Changed?.Invoke(); }
    public void ClearSelection(){if(disposed)return;Invalidate();NeedsField=company.Selection is not null;Changed?.Invoke();}
    private void Invalidate()
    {
        generation++; pending?.Cancel(); pending = null; Loading = false; Busy = false;
        Leases = []; identities.Clear(); loadedScope = null; Loaded = false; RetrievedAtUtc=null; FieldId = ""; NeedsField = false;
        Error = null; CommandError = null; Evaluation = null; Confirmation = null;
    }
    public void Dispose() { if (disposed) return; disposed = true; Invalidate(); company.Changed -= CompanyChanged; }
}
