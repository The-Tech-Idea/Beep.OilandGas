using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using ReviewClient = Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class StoredAssetGrantReviewTests
{
    private static readonly CompanyScope ReviewScope = new("tenant", "company");
    private static string ReviewDatabase => new('A', 64);
    private static AppUserAssetAccess ReviewRow() => new() { Id = "historical", UserId = "subject", OrganizationId = "company", DatabaseScope = ReviewDatabase,
        AssetType = "FACILITY", AssetId = "plant", IsActive = true, ConcurrencyStamp = Guid.NewGuid().ToString(), CreatedBy = "actor", ChangedBy = "actor" };
    private static AssetGrantReviewPage ReviewResult(AppUserAssetAccess row, int offset = 0) => new(ReviewScope, ReviewDatabase, "subject", offset, 50, 1, [row]);
    private static AssetGrantRetirement Receipt(AppUserAssetAccess row) => new() { GrantId = row.Id, Revision = Guid.NewGuid().ToString(), PreviousRevision = row.ConcurrencyStamp,
        TenantId = "tenant", OrganizationId = "company", DatabaseScope = ReviewDatabase, ActorUserId = "actor", Reason = "Reviewed historical access",
        ChangedUtc = DateTime.UtcNow, SnapshotJson = JsonSerializer.Serialize(row) };
    private static AssetGrantRetirementPage HistoryResult(AssetGrantRetirement receipt) => new(ReviewScope, ReviewDatabase, receipt.GrantId, 0, 50, 1, [receipt]);
    private static void SelectReview(CompanyClientContext company, string companyId = "company", bool admin = true, string actor = "actor") =>
        Assert.True(company.TrySelect(company.Clear(), actor, new(new("tenant", companyId), admin ? ["Administrator"] : [], [])));
    private static AssetGrantAdministrationState ReviewState(CompanyClientContext company, ReviewClient client, Func<Exception, string, string, string>? explain = null) =>
        new(company, (_, _) => throw new InvalidOperationException("Unexpected PPDM-visible read"), (_, _, _) => throw new InvalidOperationException("Unexpected grant"),
            (_, _, _) => throw new InvalidOperationException("Unexpected key-based revoke"), explain ?? ((_, _, sentence) => sentence), client);

    [Fact]
    public async Task Stored_review_accepts_historical_unknown_fields_but_never_enables_the_effective_grant_editor()
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); row.AssetType = "UNKNOWN"; row.AssetId = ""; row.AccessLevel = "historic";
        var client = new Mock<ReviewClient>(MockBehavior.Strict); client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(ReviewResult(row));
        using var state = ReviewState(company, client.Object); state.SelectUser("subject"); await state.LoadReviewAsync();
        Assert.True(state.CanRetire(row)); Assert.False(state.CanEdit); Assert.False(state.Loaded); Assert.Null(row.FacilityType); Assert.Empty(state.Grants);
        Assert.False(state.CanRetire(ReviewRow())); row.AssetId = "changed"; Assert.False(state.CanRetire(row));
    }

    [Theory]
    [InlineData("company")] [InlineData("member")] [InlineData("user")]
    public async Task Missing_company_authority_or_subject_makes_no_stored_review_request(string missing)
    {
        using var company = new CompanyClientContext(); if (missing != "company") SelectReview(company, admin: missing != "member");
        var client = new Mock<ReviewClient>(MockBehavior.Strict); using var state = ReviewState(company, client.Object);
        state.SelectUser(missing == "user" ? " subject " : "subject"); await state.LoadReviewAsync();
        Assert.NotNull(state.Error); Assert.Null(state.ReviewPage); Assert.False(state.Busy); client.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("company")] [InlineData("database")] [InlineData("user")] [InlineData("offset")] [InlineData("limit")]
    [InlineData("total")] [InlineData("duplicate")] [InlineData("row-company")] [InlineData("row-user")] [InlineData("revision")]
    public async Task Inconsistent_review_responses_do_not_expose_retirement_actions(string reason)
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); var page = ReviewResult(row);
        switch (reason)
        {
            case "company": page = page with { Company = new("tenant", "other") }; break;
            case "database": page = page with { DatabaseScope = "not-reviewed" }; break;
            case "user": page = page with { UserId = "other" }; break; case "offset": page = page with { Offset = 1 }; break;
            case "limit": page = page with { Limit = 100 }; break; case "total": page = page with { Total = 0 }; break;
            case "duplicate": page.Items.Add(row); break; case "row-company": row.OrganizationId = "other"; break;
            case "row-user": row.UserId = "other"; break; default: row.ConcurrencyStamp = ""; break;
        }
        var client = new Mock<ReviewClient>(); client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(page);
        using var state = ReviewState(company, client.Object); state.SelectUser("subject"); await state.LoadReviewAsync();
        Assert.NotNull(state.Error); Assert.Null(state.ReviewPage); Assert.False(state.CanRetire(row));
    }

    [Fact]
    public async Task Retirement_requires_the_reviewed_revision_persistent_receipt_and_fresh_inactive_inventory()
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); var receipt = Receipt(row);
        var fresh = JsonSerializer.Deserialize<AppUserAssetAccess>(JsonSerializer.Serialize(row))!; fresh.IsActive = false; fresh.ConcurrencyStamp = receipt.Revision;
        var client = new Mock<ReviewClient>(MockBehavior.Strict); var reads = 0;
        client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(() => ReviewResult(++reads == 1 ? row : fresh));
        client.Setup(value => value.RetireStoredGrantAsync("historical", new RetireAssetGrantRequest(row.ConcurrencyStamp, receipt.Reason), It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        client.Setup(value => value.GetGrantRetirementsAsync("historical", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(HistoryResult(receipt));
        using var state = ReviewState(company, client.Object); state.SelectUser("subject"); await state.LoadReviewAsync(); state.RetirementReason = receipt.Reason;
        await state.RetireAsync(row); Assert.NotNull(state.Confirmation); Assert.Null(state.Error); Assert.False(state.CanRetire(row));
        Assert.False(Assert.Single(state.ReviewPage!.Items).IsActive); Assert.Equal(receipt.Revision, Assert.Single(state.RetirementHistory!.Items).Revision);
        Assert.Empty(state.RetirementReason); Assert.False(state.CanEdit); client.VerifyAll();
    }

    [Theory]
    [InlineData("empty")] [InlineData("long")] [InlineData("padded")] [InlineData("control")]
    public async Task Retirement_requires_a_valid_reason_before_sending_the_command(string reason)
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); var client = new Mock<ReviewClient>(MockBehavior.Strict);
        client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(ReviewResult(row));
        using var state = ReviewState(company, client.Object); state.SelectUser("subject"); await state.LoadReviewAsync();
        state.RetirementReason = reason switch { "empty" => "", "long" => new string('r', 1001), "padded" => " reason ", _ => "bad\nreason" };
        await state.RetireAsync(row); Assert.NotNull(state.Error); Assert.True(state.CanRetire(row)); client.VerifyAll(); client.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("missing-receipt")] [InlineData("foreign-actor")] [InlineData("missing-history")] [InlineData("foreign-history")]
    [InlineData("active-readback")] [InlineData("stale-readback")] [InlineData("missing-readback")] [InlineData("null-history-row")]
    public async Task Unconfirmed_retirement_clears_actions_until_a_fresh_review(string reason)
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); var receipt = Receipt(row); var fresh = ReviewRow();
        fresh.IsActive = false; fresh.ConcurrencyStamp = receipt.Revision; var history = HistoryResult(receipt);
        if (reason == "foreign-actor") receipt = new() { GrantId = receipt.GrantId, Revision = receipt.Revision, PreviousRevision = receipt.PreviousRevision,
            TenantId = receipt.TenantId, OrganizationId = receipt.OrganizationId, DatabaseScope = receipt.DatabaseScope, Reason = receipt.Reason, SnapshotJson = receipt.SnapshotJson, ActorUserId = "other" };
        if (reason == "missing-history") history = history with { Items = [], Total = 0 };
        if (reason == "foreign-history") history = history with { Company = new("tenant", "other") };
        if (reason == "null-history-row") history = history with { Items = [null!] };
        if (reason == "active-readback") fresh.IsActive = true;
        if (reason == "stale-readback") fresh.ConcurrencyStamp = row.ConcurrencyStamp;
        var reads = 0; var client = new Mock<ReviewClient>();
        client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(() => ++reads == 1 ? ReviewResult(row) :
            reason == "missing-readback" ? new(ReviewScope, ReviewDatabase, "subject", 0, 50, 0, []) : ReviewResult(fresh));
        client.Setup(value => value.RetireStoredGrantAsync("historical", It.IsAny<RetireAssetGrantRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(reason == "missing-receipt" ? null! : receipt);
        client.Setup(value => value.GetGrantRetirementsAsync("historical", 0, 50, It.IsAny<CancellationToken>())).ReturnsAsync(history);
        using var state = ReviewState(company, client.Object); state.SelectUser("subject"); await state.LoadReviewAsync(); state.RetirementReason = "Reviewed historical access";
        await state.RetireAsync(row); Assert.NotNull(state.Error); Assert.Null(state.Confirmation); Assert.Null(state.ReviewPage); Assert.False(state.CanRetire(row)); Assert.False(state.Busy);
    }

    [Theory]
    [InlineData("inventory", "company")] [InlineData("inventory", "subject")] [InlineData("inventory", "dispose")]
    [InlineData("retire", "company")] [InlineData("retire", "subject")] [InlineData("retire", "dispose")]
    [InlineData("history", "company")] [InlineData("history", "subject")] [InlineData("history", "dispose")]
    [InlineData("readback", "company")] [InlineData("readback", "subject")] [InlineData("readback", "dispose")]
    [InlineData("inventory", "account")] [InlineData("retire", "account")] [InlineData("history", "account")] [InlineData("readback", "account")]
    public async Task Late_review_retirement_and_readback_failures_cannot_repopulate_cleared_selections_or_call_disposed_failure_services(string phase, string change)
    {
        using var company = new CompanyClientContext(); SelectReview(company); var row = ReviewRow(); var receipt = Receipt(row); var reads = 0;
        var waitingInventory = new TaskCompletionSource<AssetGrantReviewPage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitingRetirement = new TaskCompletionSource<AssetGrantRetirement>(TaskCreationOptions.RunContinuationsAsynchronously);
        var waitingHistory = new TaskCompletionSource<AssetGrantRetirementPage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var client = new Mock<ReviewClient>();
        client.Setup(value => value.ReviewStoredGrantsAsync("subject", 0, 50, It.IsAny<CancellationToken>())).Returns(() => phase == "inventory" || phase == "readback" && ++reads == 2 ? waitingInventory.Task : Task.FromResult(ReviewResult(row)));
        client.Setup(value => value.RetireStoredGrantAsync("historical", It.IsAny<RetireAssetGrantRequest>(), It.IsAny<CancellationToken>())).Returns(() => phase == "retire" ? waitingRetirement.Task : Task.FromResult(receipt));
        client.Setup(value => value.GetGrantRetirementsAsync("historical", 0, 50, It.IsAny<CancellationToken>())).Returns(() => phase == "history" ? waitingHistory.Task : Task.FromResult(HistoryResult(receipt)));
        using var state = ReviewState(company, client.Object, (_, _, _) => throw new ObjectDisposedException("Old failure services")); state.SelectUser("subject");
        Task pending;
        if (phase == "inventory") pending = state.LoadReviewAsync();
        else { await state.LoadReviewAsync(); state.RetirementReason = receipt.Reason; pending = state.RetireAsync(row); }
        Assert.False(pending.IsCompleted);
        if (change == "company") SelectReview(company, "other"); else if (change == "subject") state.SelectUser("other"); else if (change == "account") SelectReview(company, actor: "other-account"); else state.Dispose();
        if (phase is "inventory" or "readback") waitingInventory.SetException(new InvalidOperationException("late"));
        else if (phase == "retire") waitingRetirement.SetException(new InvalidOperationException("late")); else waitingHistory.SetException(new InvalidOperationException("late"));
        await pending; Assert.Null(state.ReviewPage); Assert.Null(state.RetirementHistory); Assert.Null(state.Error); Assert.Null(state.Confirmation); Assert.False(state.Busy);
    }
}
