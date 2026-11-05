using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;

namespace Beep.OilandGas.Web.Services;

public sealed partial class AssetGrantAdministrationState
{
    private readonly Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService review;
    private readonly Dictionary<AppUserAssetAccess, string> reviewed = new(ReferenceEqualityComparer.Instance);
    public AssetGrantReviewPage? ReviewPage { get; private set; }
    public AssetGrantRetirementPage? RetirementHistory { get; private set; }
    public string RetirementReason { get; set; } = "";
    public bool CanReview => CanLoad;
    public bool CanReviewRows => !disposed && !Busy && ReviewPage is not null && IsAdministrator && loadedScope == company.Snapshot;
    public bool CanRetire(AppUserAssetAccess row) => CanReviewRow(row) && row.IsActive;
    public bool CanReviewRow(AppUserAssetAccess row) => CanReviewRows && reviewed.TryGetValue(row, out var snapshot) && JsonSerializer.Serialize(row) == snapshot;

    public Task LoadReviewAsync() => LoadReviewPageAsync(0);
    public async Task LoadReviewPageAsync(int offset)
    {
        if (disposed) return;
        Clear(); var scope = company.Snapshot; var user = UserId; var version = generation;
        if (scope.Selection is null || !IsAdministrator || !Identifier(user) || offset < 0)
        { Error = "Select a company Administrator account and enter the application user ID to review stored grants."; Changed?.Invoke(); return; }
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && UserId == user;
        try
        {
            var page = await review.ReviewStoredGrantsAsync(user, offset, 50, cancellation.Token);
            if (!Current()) return;
            ValidateReview(page, scope.Selection.Scope, user, offset);
            PublishReview(page, scope);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, "reviewing stored asset grants", "Stored grants could not be reviewed");
            if (Current()) Error = sentence;
        }
        finally { if (ReferenceEquals(pending, cancellation)) pending = null; if (Current()) { Busy = false; Changed?.Invoke(); } }
    }

    public async Task LoadRetirementHistoryAsync(AppUserAssetAccess row, int offset = 0)
    {
        if (!CanReviewRow(row) || offset < 0) return;
        var scope = loadedScope!.Value; var user = UserId; var version = generation; var id = row.Id; var database = row.DatabaseScope;
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Error = null; RetirementHistory = null; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && UserId == user;
        try
        {
            var history = await review.GetGrantRetirementsAsync(id, offset, 50, cancellation.Token);
            if (!Current()) return;
            ValidateHistory(history, scope.Selection!.Scope, database, id, offset, 50); RetirementHistory = history;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, "reading grant retirement history", "Retirement history could not be loaded");
            if (Current()) { ClearReview(); loadedScope = null; Error = sentence; }
        }
        finally { if (ReferenceEquals(pending, cancellation)) pending = null; if (Current()) { Busy = false; Changed?.Invoke(); } }
    }

    public async Task RetireAsync(AppUserAssetAccess row)
    {
        if (!CanRetire(row)) return;
        if (string.IsNullOrWhiteSpace(RetirementReason) || RetirementReason.Length > 1000 || RetirementReason != RetirementReason.Trim() || RetirementReason.Any(char.IsControl))
        { Error = "Provide a retirement reason of at most 1000 characters without surrounding whitespace or control characters."; Changed?.Invoke(); return; }
        var scope = loadedScope!.Value; var user = UserId; var version = generation; var original = reviewed[row];
        var id = row.Id; var database = row.DatabaseScope; var offset = ReviewPage!.Offset;
        var request = new RetireAssetGrantRequest(row.ConcurrencyStamp, RetirementReason);
        using var cancellation = new CancellationTokenSource(); pending = cancellation; Busy = true; Error = null; Confirmation = null; Changed?.Invoke();
        bool Current() => !disposed && generation == version && company.Snapshot == scope && UserId == user;
        try
        {
            var receipt = await review.RetireStoredGrantAsync(id, request, cancellation.Token);
            if (!Current()) return;
            ValidateReceipt(receipt, scope.Selection!.Scope, database, id);
            if (receipt.PreviousRevision != request.ExpectedRevision || receipt.SnapshotJson != original || receipt.Reason != request.Reason || receipt.ActorUserId != scope.Selection.UserId)
                throw new InvalidOperationException("Retirement evidence does not match the reviewed grant and command.");
            var history = await review.GetGrantRetirementsAsync(id, 0, 50, cancellation.Token);
            if (!Current()) return;
            ValidateHistory(history, scope.Selection.Scope, database, id, 0, 50);
            if (!history.Items.Any(audit => JsonSerializer.Serialize(audit) == JsonSerializer.Serialize(receipt)))
                throw new InvalidOperationException("The retirement receipt could not be confirmed from persistent history.");
            var page = await review.ReviewStoredGrantsAsync(user, offset, 50, cancellation.Token);
            if (!Current()) return;
            ValidateReview(page, scope.Selection.Scope, user, offset);
            if (page.DatabaseScope != database || !page.Items.Any(fresh => fresh.Id == id && !fresh.IsActive && fresh.ConcurrencyStamp == receipt.Revision))
                throw new InvalidOperationException("The stored grant retirement could not be confirmed by fresh inventory.");
            PublishReview(page, scope); RetirementHistory = history; RetirementReason = "";
            Confirmation = "Grant retired; inactive status and retirement evidence confirmed.";
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested || !Current()) { }
        catch (Exception failure)
        {
            if (!Current()) return;
            var sentence = explain(failure, "retiring stored asset access", "Grant retirement could not be confirmed. Reload and review before making further changes");
            if (Current()) { ClearReview(); loadedScope = null; Error = sentence; }
        }
        finally { if (ReferenceEquals(pending, cancellation)) pending = null; if (Current()) { Busy = false; Changed?.Invoke(); } }
    }

    private void PublishReview(AssetGrantReviewPage page, (long Version, CompanyClientSelection? Selection) scope)
    {
        ReviewPage = page; reviewed.Clear(); loadedScope = scope;
        foreach (var row in page.Items) reviewed.Add(row, JsonSerializer.Serialize(row));
    }
    private static void ValidateReview(AssetGrantReviewPage page, CompanyScope scope, string user, int offset)
    {
        if (page is null || page.Company != scope || page.UserId != user || page.Offset != offset || page.Limit != 50 || page.Total < 0 ||
            !Fingerprint(page.DatabaseScope) || page.Items is null || page.Items.Count > page.Limit || page.Items.Count > page.Total ||
            page.Items.Any(row => row is null || row.UserId != user || row.DatabaseScope != page.DatabaseScope || row.OrganizationId != scope.BusinessAssociateId ||
                !Identifier(row.Id) || row.Id.Length > 64 || string.IsNullOrWhiteSpace(row.ConcurrencyStamp) || row.ConcurrencyStamp.Length > 36) ||
            page.Items.Select(row => row.Id).Distinct(StringComparer.Ordinal).Count() != page.Items.Count)
            throw new InvalidOperationException("Stored grant inventory has inconsistent ownership, revisions or paging.");
        // Historical asset/qualifier/level fields are displayed as stored evidence, without inferring new authority.
    }
    private static void ValidateHistory(AssetGrantRetirementPage page, CompanyScope scope, string database, string id, int offset, int limit)
    {
        if (page is null || page.Company != scope || page.DatabaseScope != database || page.GrantId != id || page.Offset != offset || page.Limit != limit ||
            page.Total < 0 || page.Items is null || page.Items.Count > limit || page.Items.Count > page.Total || page.Items.Any(row => row is null) || page.Items.Select(row => row.Revision).Distinct(StringComparer.Ordinal).Count() != page.Items.Count)
            throw new InvalidOperationException("Grant retirement history has inconsistent ownership or paging.");
        foreach (var receipt in page.Items) ValidateReceipt(receipt, scope, database, id);
    }
    private static void ValidateReceipt(AssetGrantRetirement receipt, CompanyScope scope, string database, string id)
    {
        if (receipt is null || receipt.GrantId != id || receipt.TenantId != scope.TenantId || receipt.OrganizationId != scope.BusinessAssociateId || receipt.DatabaseScope != database ||
            !Guid.TryParseExact(receipt.Revision, "D", out _) || !Identifier(receipt.ActorUserId) || string.IsNullOrWhiteSpace(receipt.PreviousRevision) ||
            string.IsNullOrWhiteSpace(receipt.Reason) || receipt.Reason.Length > 1000 || string.IsNullOrWhiteSpace(receipt.SnapshotJson))
            throw new InvalidOperationException("Stored grant retirement evidence is invalid.");
        var before = JsonSerializer.Deserialize<AppUserAssetAccess>(receipt.SnapshotJson);
        if (before is null || before.Id != id || before.DatabaseScope != database || before.OrganizationId != scope.BusinessAssociateId ||
            before.ConcurrencyStamp != receipt.PreviousRevision || !before.IsActive)
            throw new InvalidOperationException("Retirement evidence has an inconsistent reviewed grant snapshot.");
    }
    private static bool Fingerprint(string value) => value is { Length: 64 } && value.All(char.IsAsciiHexDigit) && value == value.ToUpperInvariant();
    private void ClearReview() { ReviewPage = null; RetirementHistory = null; RetirementReason = ""; reviewed.Clear(); }
}
