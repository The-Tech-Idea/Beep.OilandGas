using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;

namespace Beep.OilandGas.Web.Services;

/// <summary>Confirmed direct grants for one selected company and subject; API authority remains mandatory.</summary>
public sealed partial class AssetGrantAdministrationState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly Func<string, CancellationToken, Task<List<AssetAccess>?>> load;
    private readonly Func<string, GrantAccessRequest, CancellationToken, Task<bool>> grant;
    private readonly Func<string, RevokeAccessRequest, CancellationToken, Task<bool>> revoke;
    private readonly Func<Exception, string, string, string> explain;
    private CancellationTokenSource? pending;
    private long generation;
    private bool disposed;
    private (long Version, CompanyClientSelection? Selection)? loadedScope;
    private readonly Dictionary<AssetAccess, (string User, string Company, string Type, string Id, string? Facility, string Level, bool Inherit)> confirmed = new(ReferenceEqualityComparer.Instance);

    public string UserId { get; private set; } = "";
    public string AssetType { get; set; } = "FIELD";
    public string AssetId { get; set; } = "";
    public string? FacilityType { get; set; }
    public string AccessLevel { get; set; } = "READ";
    public bool Inherit { get; set; } = true;
    public IReadOnlyList<AssetAccess> Grants { get; private set; } = [];
    public bool Busy { get; private set; }
    public bool Loaded { get; private set; }
    public string? Error { get; private set; }
    public string? Confirmation { get; private set; }
    public bool CanLoad => !disposed && !Busy && IsAdministrator && Identifier(UserId);
    public bool CanEdit => !disposed && !Busy && Loaded && IsAdministrator && loadedScope == company.Snapshot;
    private bool IsAdministrator => company.Selection?.Roles.Contains("Administrator", StringComparer.Ordinal) == true;
    public event Action? Changed;

    public AssetGrantAdministrationState(CompanyClientContext company,
        Func<string, CancellationToken, Task<List<AssetAccess>?>> load,
        Func<string, GrantAccessRequest, CancellationToken, Task<bool>> grant,
        Func<string, RevokeAccessRequest, CancellationToken, Task<bool>> revoke,
        Func<Exception, string, string, string> explain,
        Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService review)
    {
        this.company = company; this.load = load; this.grant = grant; this.revoke = revoke; this.explain = explain;
        this.review = review ?? throw new ArgumentNullException(nameof(review));
        company.Changed += CompanyChanged;
    }

    public void SelectUser(string? userId)
    {
        if (disposed || UserId == userId) return;
        Clear(); UserId = userId ?? ""; ResetDraft(); Changed?.Invoke();
    }

    public void SelectAssetType(string? type)
    {
        if (!CanEdit) return;
        AssetType = type ?? ""; AssetId = ""; FacilityType = null; Inherit = AssetType == "FIELD"; Confirmation = null; Changed?.Invoke();
    }

    public async Task LoadAsync()
    {
        if (disposed) return;
        Clear(); var scope = company.Snapshot; var user = UserId; var version = generation;
        if (scope.Selection is null) { Error = "Select a company before managing asset grants."; Changed?.Invoke(); return; }
        if (!IsAdministrator) { Error = "A company Administrator is required to manage asset grants."; Changed?.Invoke(); return; }
        if (!Identifier(user)) { Error = "Enter an application user ID of at most 128 characters without surrounding whitespace."; Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && UserId == user;
        try
        {
            var rows = await load(ReadPath(user, scope.Selection.Scope.BusinessAssociateId), cancellation.Token);
            if (!Current()) return;
            ValidateRows(rows, user, scope.Selection.Scope.BusinessAssociateId);
            Publish(rows!, scope); Loaded = true;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, "loading direct asset grants", "Asset grants could not be loaded");
            if (Current()) Error = sentence;
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Busy = false; Changed?.Invoke(); }
        }
    }

    public Task GrantAsync()
    {
        if (!CanEdit) return Task.CompletedTask;
        if (!Supported(AssetType) || !Identifier(AssetId) || !Level(AccessLevel) || AssetType != "FIELD" && Inherit ||
            (AssetType == "FACILITY" ? !Identifier(FacilityType) : FacilityType is not null))
        {
            Error = "Choose FIELD, WELL, POOL, FACILITY, EQUIPMENT or WORK_ORDER, exact asset identifiers, and READ, WRITE or DELETE. Facilities require their type; only FIELD grants inherit to children.";
            Changed?.Invoke(); return Task.CompletedTask;
        }
        var request = new GrantAccessRequest { UserId = UserId, OrganizationId = company.Selection!.Scope.BusinessAssociateId,
            AssetType = AssetType, AssetId = AssetId, FacilityType = FacilityType, AccessLevel = AccessLevel, Inherit = Inherit };
        return MutateAsync(token => grant("/api/AccessControl/grant-access", request, token),
            rows => rows.Any(row => row.AssetType == request.AssetType && row.AssetId == request.AssetId && row.FacilityType == request.FacilityType && row.AccessLevel == request.AccessLevel && row.Inherit == request.Inherit),
            "granting asset access", "Asset grant saved and refreshed.");
    }

    public bool CanRevoke(AssetAccess row) => CanEdit && confirmed.TryGetValue(row, out var key) &&
        row.Active && row.UserId == key.User && row.OrganizationId == key.Company && row.AssetType == key.Type && row.AssetId == key.Id &&
        row.AccessLevel == key.Level && row.Inherit == key.Inherit && row.FacilityType == key.Facility;

    public Task RevokeAsync(AssetAccess row)
    {
        if (!CanRevoke(row)) return Task.CompletedTask;
        var key = confirmed[row];
        var request = new RevokeAccessRequest { UserId = key.User, AssetType = key.Type, AssetId = key.Id, FacilityType = key.Facility };
        return MutateAsync(token => revoke("/api/AccessControl/revoke-access", request, token),
            rows => !rows.Any(candidate => candidate.AssetType == key.Type && candidate.AssetId == key.Id && candidate.FacilityType == key.Facility),
            "revoking asset access", "Asset grant revoked and refreshed.");
    }

    private async Task MutateAsync(Func<CancellationToken, Task<bool>> send, Func<List<AssetAccess>, bool> confirmedChange,
        string operation, string success)
    {
        var scope = loadedScope!.Value; var user = UserId; var version = generation;
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Error = null; Confirmation = null; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && UserId == user;
        var applied = false;
        try
        {
            var result = await send(cancellation.Token);
            if (!Current()) return;
            if (!result) throw new InvalidOperationException("The asset grant API did not confirm the change.");
            applied = true;
            var rows = await load(ReadPath(user, scope.Selection!.Scope.BusinessAssociateId), cancellation.Token);
            if (!Current()) return;
            ValidateRows(rows, user, scope.Selection.Scope.BusinessAssociateId);
            if (!confirmedChange(rows!)) throw new InvalidOperationException("The requested asset grant change could not be confirmed by readback.");
            Publish(rows!, scope); Confirmation = success;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, applied ? "refreshing changed asset grants" : operation,
                applied ? "Changed asset grants could not be refreshed. Reload before making further changes" : "Asset grant change could not be confirmed. Reload before making further changes");
            if (Current()) { Grants = []; confirmed.Clear(); Loaded = false; loadedScope = null; Error = sentence; }
        }
        finally
        {
            if (ReferenceEquals(pending, cancellation)) pending = null;
            if (Current()) { Busy = false; Changed?.Invoke(); }
        }
    }

    private static string ReadPath(string user, string organization) => "/api/AccessControl/user/" + Uri.EscapeDataString(user) +
        "/assets?includeInherited=false&organizationId=" + Uri.EscapeDataString(organization);
    private void Publish(List<AssetAccess> rows, (long Version, CompanyClientSelection? Selection) scope)
    {
        Grants = rows.OrderBy(row => row.AssetType, StringComparer.Ordinal).ThenBy(row => row.AssetId, StringComparer.Ordinal).ThenBy(row => row.FacilityType, StringComparer.Ordinal).ToList();
        loadedScope = scope; confirmed.Clear();
        foreach (var row in Grants) confirmed.Add(row, (row.UserId, row.OrganizationId!, row.AssetType, row.AssetId, row.FacilityType, row.AccessLevel, row.Inherit));
    }
    private static void ValidateRows(List<AssetAccess>? rows, string user, string organization)
    {
        if (rows is null || rows.Count > 4096 || rows.Any(row => row is null || !row.Active || row.UserId != user || row.OrganizationId != organization ||
            !Supported(row.AssetType) || !Identifier(row.AssetId) || (row.AssetType is "WORK_ORDER" or "EQUIPMENT") && row.Inherit || (row.AssetType == "FACILITY" ? !Identifier(row.FacilityType) : row.FacilityType is not null) || !Level(row.AccessLevel)) ||
            rows.Select(row => (row.AssetType, row.AssetId, row.FacilityType)).Distinct().Count() != rows.Count)
            throw new InvalidOperationException("The direct asset-grant response has invalid or inconsistent company, subject or asset identities.");
    }
    private static bool Supported(string type) => type is "FIELD" or "WELL" or "POOL" or "FACILITY" or "WORK_ORDER" or "EQUIPMENT";
    private static bool Level(string level) => level is "READ" or "WRITE" or "DELETE";
    private static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
    private void ResetDraft() { AssetType = "FIELD"; AssetId = ""; FacilityType = null; AccessLevel = "READ"; Inherit = true; InterestFieldId = ""; }
    private void Clear()
    {
        ++generation; var old = pending; pending = null; old?.Cancel();
        Grants = []; confirmed.Clear(); Loaded = false; Busy = false; loadedScope = null; Error = null; Confirmation = null;
        ClearReview();
        InterestReview = null;
    }
    private void CompanyChanged() { Clear(); UserId = ""; ResetDraft(); if (!disposed) Changed?.Invoke(); }
    public void Dispose() { if (disposed) return; disposed = true; company.Changed -= CompanyChanged; Clear(); }
}
