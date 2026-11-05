using System.Text.Json;
using System.Net;
using System.Text;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class StoredAssetGrantReviewTests
{
    private static RepositoryAssetAccessStore Store(Fixture f) => new(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
    private static async Task<AppUserAssetAccess> Historical(Fixture f, bool active = true, string id = "historical")
    {
        var row = new AppUserAssetAccess { Id = id, UserId = "actor", DatabaseScope = (await f.Gate.ResolveAsync()).Fingerprint,
            OrganizationId = "company", AssetType = "FACILITY", AssetId = "old-plant", IsActive = active,
            AccessLevel = "DELETE", CreatedBy = "actor", ChangedBy = "actor", CreatedUtc = DateTime.UtcNow.AddYears(-1), ChangedUtc = DateTime.UtcNow.AddDays(-1) };
        f.Db.Add(row); await f.Db.SaveChangesAsync(); return row;
    }

    [Fact]
    public async Task Stored_inventory_includes_unqualified_inactive_and_no_longer_visible_grants_without_reading_PPDM()
    {
        using var f = await Fixture.Create(); await Historical(f); await Historical(f, false, "inactive"); await f.Grant("WELL", "gone");
        var page = await Store(f).ReviewAsync("actor", (await f.Gate.ResolveAsync()).Fingerprint);
        Assert.Equal(3, page.Total); Assert.Equal(3, page.Items.Count); Assert.Contains(page.Items, row => !row.IsActive);
        Assert.Null(page.Items.Single(row => row.Id == "historical").FacilityType);
        Assert.Empty(f.Queries); Assert.Empty(f.Connections); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("inactive-user")] [InlineData("former-member")]
    public async Task Administrators_review_and_retire_access_for_accounts_that_cannot_receive_new_grants(string reason)
    {
        using var f = await Fixture.Create(); f.Db.Users.Add(new() { Id = "former", IsActive = reason != "inactive-user" });
        var row = await Historical(f); row.UserId = "former"; await f.Db.SaveChangesAsync(); var scope = row.DatabaseScope;
        Assert.Null(await f.Gate.ResolveSubjectAsync("former"));
        var store = Store(f); Assert.Equal(row.Id, Assert.Single((await store.ReviewAsync("former", scope)).Items).Id);
        var audit = await store.RetireAsync(row.Id, scope, new(row.ConcurrencyStamp, "Former account access reviewed"));
        Assert.Equal("former", JsonSerializer.Deserialize<AppUserAssetAccess>(audit.SnapshotJson)!.UserId);
        Assert.False((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Retirement_preserves_unknown_keys_and_creation_evidence_and_persists_server_actor_and_revision()
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var before = JsonSerializer.Serialize(row); var previous = row.ConcurrencyStamp;
        var store = Store(f); var audit = await store.RetireAsync(row.Id, row.DatabaseScope, new(previous, "Unqualified historical facility reviewed"));
        Assert.Equal(before, audit.SnapshotJson); Assert.Equal(previous, audit.PreviousRevision);
        Assert.Equal("actor", audit.ActorUserId); Assert.Equal("tenant", audit.TenantId); Assert.Equal("company", audit.OrganizationId);
        Assert.True(Guid.TryParseExact(audit.Revision, "D", out _)); Assert.Equal(DateTimeKind.Utc, audit.ChangedUtc.Kind);
        var fresh = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); Assert.False(fresh.IsActive); Assert.Null(fresh.FacilityType);
        Assert.Equal(row.CreatedUtc, fresh.CreatedUtc); Assert.Equal(row.CreatedBy, fresh.CreatedBy); Assert.Equal(audit.Revision, fresh.ConcurrencyStamp);
        var history = await store.ReadRetirementsAsync(row.Id, row.DatabaseScope); Assert.Equal(1, history.Total); Assert.Equal(audit.Revision, Assert.Single(history.Items).Revision);
        Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("foreign-company")] [InlineData("unowned")] [InlineData("other-database")] [InlineData("wrong-id")]
    public async Task Inventory_and_retirement_never_claim_unproven_or_foreign_record_ownership(string reason)
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var scope = row.DatabaseScope;
        if (reason == "foreign-company") row.OrganizationId = "other";
        if (reason == "unowned") row.OrganizationId = null;
        if (reason == "other-database") row.DatabaseScope = new string('F', 64);
        await f.Db.SaveChangesAsync(); var store = Store(f);
        if (reason != "wrong-id") Assert.Empty((await store.ReviewAsync("actor", scope)).Items);
        await Assert.ThrowsAsync<RefusalException>(() => store.RetireAsync(reason == "wrong-id" ? "HISTORICAL" : row.Id, scope, new(row.ConcurrencyStamp, "Reviewed")));
        await Assert.ThrowsAsync<RefusalException>(() => store.ReadRetirementsAsync(reason == "wrong-id" ? "HISTORICAL" : row.Id, scope));
        Assert.True((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync()).IsActive); Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
    }

    [Theory]
    [InlineData("role")] [InlineData("membership")] [InlineData("binding")]
    public async Task Current_authority_and_binding_changes_refuse_inventory_history_and_retirement(string reason)
    {
        using var f = await Fixture.Create(); var row = await Historical(f);
        if (reason == "role") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
        if (reason == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
        if (reason == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
        await f.Db.SaveChangesAsync(); var store = Store(f);
        await Assert.ThrowsAsync<RefusalException>(() => store.ReviewAsync("actor", row.DatabaseScope));
        await Assert.ThrowsAsync<RefusalException>(() => store.ReadRetirementsAsync(row.Id, row.DatabaseScope));
        await Assert.ThrowsAsync<RefusalException>(() => store.RetireAsync(row.Id, row.DatabaseScope, new(row.ConcurrencyStamp, "Reviewed")));
        Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
    }

    [Theory]
    [InlineData("revision")] [InlineData("inactive")]
    public async Task Stale_or_already_inactive_grants_require_review_again_and_never_add_false_audit(string reason)
    {
        using var f = await Fixture.Create(); var row = await Historical(f, reason != "inactive"); var store = Store(f);
        var failure = await Assert.ThrowsAsync<RefusalException>(() => store.RetireAsync(row.Id, row.DatabaseScope,
            new(reason == "revision" ? Guid.NewGuid().ToString() : row.ConcurrencyStamp, "Reviewed")));
        Assert.Equal(RefusalKind.Conflict, failure.Kind); Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
        Assert.False(f.Db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData("empty-reason")] [InlineData("long-reason")] [InlineData("padded-reason")] [InlineData("control-reason")]
    [InlineData("empty-revision")] [InlineData("long-revision")] [InlineData("padded-revision")] [InlineData("control-revision")]
    public async Task Invalid_review_commands_leave_grants_and_evidence_unchanged(string invalid)
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var revision = row.ConcurrencyStamp; var reason = "Reviewed";
        switch (invalid)
        {
            case "empty-reason": reason = ""; break; case "long-reason": reason = new string('r', 1001); break;
            case "padded-reason": reason = " Reviewed "; break; case "control-reason": reason = "bad\nreason"; break;
            case "empty-revision": revision = ""; break; case "long-revision": revision = new string('r', 37); break;
            case "padded-revision": revision = " old "; break; default: revision = "bad\nrevision"; break;
        }
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).RetireAsync(row.Id, row.DatabaseScope, new(revision, reason)))).Kind);
        Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync()); Assert.True(row.IsActive);
    }

    [Fact]
    public async Task A_failure_after_SaveChanges_rolls_back_both_grant_and_audit_and_leaves_no_queued_writes()
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var original = row.ConcurrencyStamp;
        EventHandler<SavedChangesEventArgs> fail = (_, _) => throw new InvalidOperationException("Persist failure"); f.Db.SavedChanges += fail;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store(f).RetireAsync(row.Id, row.DatabaseScope, new(original, "Reviewed")));
        f.Db.SavedChanges -= fail; Assert.False(f.Db.ChangeTracker.HasChanges()); await f.Db.SaveChangesAsync();
        var fresh = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); Assert.True(fresh.IsActive); Assert.Equal(original, fresh.ConcurrencyStamp);
        Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
    }

    [Fact]
    public async Task EF_compare_and_swap_failure_rolls_back_the_retirement_audit()
    {
        using var f = await Fixture.Create(); var row = await Historical(f);
        EventHandler<SavingChangesEventArgs> conflict = (_, _) => f.Db.ChangeTracker.Entries<AppUserAssetAccess>().Single()
            .Property(value => value.ConcurrencyStamp).OriginalValue = "changed-concurrently";
        f.Db.SavingChanges += conflict;
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).RetireAsync(row.Id, row.DatabaseScope, new(row.ConcurrencyStamp, "Reviewed")))).Kind);
        f.Db.SavingChanges -= conflict; Assert.False(f.Db.ChangeTracker.HasChanges());
        Assert.True((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync()).IsActive); Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
    }

    [Fact]
    public async Task A_retirement_cannot_save_unrelated_pending_changes()
    {
        using var f = await Fixture.Create(); var row = await Historical(f); (await f.Db.Users.SingleAsync()).UserName = "unsaved";
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store(f).RetireAsync(row.Id, row.DatabaseScope, new(row.ConcurrencyStamp, "Reviewed")));
        Assert.True(row.IsActive); Assert.Empty(await f.Db.Set<AssetGrantRetirement>().ToListAsync());
    }

    [Theory]
    [InlineData(-1, 50)] [InlineData(0, 0)] [InlineData(0, 101)]
    public async Task Invalid_inventory_and_history_paging_is_refused(int offset, int limit)
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var store = Store(f);
        await Assert.ThrowsAsync<RefusalException>(() => store.ReviewAsync("actor", row.DatabaseScope, offset, limit));
        await Assert.ThrowsAsync<RefusalException>(() => store.ReadRetirementsAsync(row.Id, row.DatabaseScope, offset, limit));
    }

    [Fact]
    public async Task Review_pages_are_bounded_and_exact_and_unknown_subjects_are_explicit_empty()
    {
        using var f = await Fixture.Create(); for (var i = 0; i < 7; i++) await Historical(f, id: "historical-" + i);
        var scope = (await f.Gate.ResolveAsync()).Fingerprint; var store = Store(f);
        var first = await store.ReviewAsync("actor", scope, 0, 3); var second = await store.ReviewAsync("actor", scope, 3, 3);
        Assert.Equal(7, first.Total); Assert.Equal(7, second.Total); Assert.Equal(6, first.Items.Concat(second.Items).Select(row => row.Id).Distinct().Count());
        Assert.Single((await store.ReviewAsync("actor", scope, 6, 3)).Items); Assert.Empty((await store.ReviewAsync("missing", scope)).Items);
    }

    [Fact]
    public async Task Regrant_after_retirement_does_not_rewrite_the_previous_retirement_snapshot()
    {
        using var f = await Fixture.Create(); await f.Grant("FACILITY", "plant", "WRITE", facilityType: "gas");
        var row = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); var store = Store(f);
        var audit = await store.RetireAsync(row.Id, row.DatabaseScope, new(row.ConcurrencyStamp, "Reviewed"));
        Assert.True(await store.GrantAsync("actor", row.DatabaseScope, "FACILITY", "plant", "READ", false, facilityType: "gas"));
        var fresh = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); Assert.True(fresh.IsActive); Assert.Equal("READ", fresh.AccessLevel);
        var history = await store.ReadRetirementsAsync(row.Id, row.DatabaseScope); Assert.Equal("WRITE", JsonSerializer.Deserialize<AppUserAssetAccess>(Assert.Single(history.Items).SnapshotJson)!.AccessLevel);
        Assert.Equal(audit.Revision, history.Items[0].Revision);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task Existing_retirement_evidence_cannot_be_modified_or_deleted_through_the_repository(bool delete)
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var receipt = await Store(f).RetireAsync(row.Id, row.DatabaseScope, new(row.ConcurrencyStamp, "Reviewed"));
        var audit = await f.Db.Set<AssetGrantRetirement>().SingleAsync();
        if (delete) f.Db.Remove(audit); else f.Db.Entry(audit).Property(value => value.Reason).CurrentValue = "rewritten";
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Db.SaveChangesAsync());
        Assert.Throws<InvalidOperationException>(() => f.Db.SaveChanges()); f.Db.Entry(audit).State = EntityState.Detached;
        Assert.Equal(receipt.Reason, (await f.Db.Set<AssetGrantRetirement>().AsNoTracking().SingleAsync()).Reason);
    }

    [Fact]
    public async Task Actual_controller_facade_store_SDK_JSON_and_Web_review_retire_historical_access_without_PPDM_reads()
    {
        using var f = await Fixture.Create(); var row = await Historical(f); var backend = CompanyAssetFacadeTests.Service(f);
        var controller = new AccessControlController(backend, principal => principal.FindFirst("party_id")?.Value)
        {
            ControllerContext = new() { HttpContext = new DefaultHttpContext { User = new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test")) } }
        };
        using var company = new CompanyClientContext(); SelectReview(company); var routes = new List<string>();
        using var http = new HttpClient(new ReviewHandler(async (request, token) =>
        {
            Assert.Equal("tenant", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Tenant)));
            Assert.Equal("company", Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
            var path = request.RequestUri!.PathAndQuery; routes.Add(path); object? result;
            if (request.Method == HttpMethod.Post)
            {
                Assert.Equal("/api/AccessControl/stored-grants/historical/retire", path);
                var command = JsonSerializer.Deserialize<RetireAssetGrantRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                result = Assert.IsType<OkObjectResult>((await controller.RetireStoredGrant(row.Id, command, token)).Result).Value;
            }
            else if (path.Contains("/retirements?"))
                result = Assert.IsType<OkObjectResult>((await controller.GetGrantRetirements(row.Id, token: token)).Result).Value;
            else
            {
                Assert.Equal("/api/AccessControl/user/actor/stored-grants?offset=0&limit=50", path);
                result = Assert.IsType<OkObjectResult>((await controller.ReviewStoredGrants("actor", token: token)).Result).Value;
            }
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }), Encoding.UTF8, "application/json") };
        }));
        var sdk = new Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService(new(http, new() { ApiBaseUrl = "https://api.test" }, company));
        using var state = ReviewState(company, sdk); state.SelectUser("actor"); await state.LoadReviewAsync();
        var reviewed = Assert.Single(state.ReviewPage!.Items); Assert.Null(reviewed.FacilityType); Assert.True(state.CanRetire(reviewed));
        state.RetirementReason = "Reviewed historical access"; await state.RetireAsync(reviewed);
        Assert.NotNull(state.Confirmation); Assert.Null(state.Error); Assert.False(Assert.Single(state.ReviewPage!.Items).IsActive);
        var audit = Assert.Single(state.RetirementHistory!.Items); Assert.Equal(reviewed.ConcurrencyStamp, audit.PreviousRevision);
        Assert.Equal("actor", audit.ActorUserId); Assert.Equal(4, routes.Count);
        Assert.Empty(f.Queries); Assert.Empty(f.Connections); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("user")] [InlineData("organization")] [InlineData("database")]
    public async Task Case_insensitive_provider_aliases_are_refused_before_inventory_counts_and_out_of_range_pages(string alias)
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); connection.Open();
        using var db = new ReviewCollationContext(new DbContextOptionsBuilder<ReviewCollationContext>().UseSqlite(connection).Options); db.Database.EnsureCreated();
        db.Users.Add(new() { Id = "actor", IsActive = true }); db.Roles.Add(new() { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
        db.UserRoles.Add(new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" }); await db.SaveChangesAsync();
        var gate = await TestCompanyModules.CreateAsync(db, "actor"); var database = (await gate.ResolveAsync()).Fingerprint;
        db.Add(new AppUserAssetAccess { Id = "historical", UserId = "actor", OrganizationId = alias == "organization" ? "COMPANY" : "company",
            DatabaseScope = alias == "database" ? database.ToLowerInvariant() : database, AssetType = "FACILITY", AssetId = "plant", IsActive = true }); await db.SaveChangesAsync();
        var store = new RepositoryAssetAccessStore(db, gate, Mock.Of<IAssetGrantTargetVerifier>()); var user = alias == "user" ? "ACTOR" : "actor";
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => store.ReviewAsync(user, database, 100, 50))).Kind);
        Assert.Empty(await db.Set<AssetGrantRetirement>().ToListAsync());
    }

    private sealed class ReviewCollationContext(DbContextOptions<ReviewCollationContext> options) : RepositoryDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppUserAssetAccess>().Property(row => row.UserId).UseCollation("NOCASE");
            builder.Entity<AppUserAssetAccess>().Property(row => row.OrganizationId).UseCollation("NOCASE");
            builder.Entity<AppUserAssetAccess>().Property(row => row.DatabaseScope).UseCollation("NOCASE");
        }
    }

    private sealed class ReviewHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => handler(request, cancellationToken);
    }
}
