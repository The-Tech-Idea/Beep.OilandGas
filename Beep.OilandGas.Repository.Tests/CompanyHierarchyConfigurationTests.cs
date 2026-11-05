using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyHierarchyConfigurationTests
{
    [Fact]
    public async Task Adding_independent_order_category_updates_the_existing_revision_aggregate()
    {
        using var f=await Fixture.Create();var store=new RepositoryHierarchyConfigurationStore(f.Db,f.Gate);
        await store.UpdateAsync("company",Levels());var rows=await store.ReadAsync("company",true);var previous=rows[0].Revision;
        foreach(var row in rows)row.Active=false;
        rows.Add(new(){OrganizationId="company",AssetType="WORK_ORDER",HierarchyLevel=0,ParentLevel=null,LevelName="أوامر العمل",Active=true,Revision=previous});
        Assert.True(await store.UpdateAsync("company",rows));var saved=await store.ReadAsync("company",true);
        Assert.Equal(5,saved.Count);Assert.Equal("أوامر العمل",saved.Single(row=>row.AssetType=="WORK_ORDER").LevelName);
        Assert.True(saved.Single(row=>row.AssetType=="WORK_ORDER").Active);Assert.False(saved.Single(row=>row.AssetType=="FIELD").Active);
        Assert.NotEqual(previous,saved[0].Revision);Assert.Single(await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().ToListAsync());
        Assert.Equal(2,await f.Db.Set<OrganizationHierarchyConfigRevision>().CountAsync());
    }

    [Theory][InlineData("parent")][InlineData("duplicate")]
    public async Task Order_presentation_cannot_claim_a_field_parent_or_duplicate_category(string fault)
    {
        using var f=await Fixture.Create();var rows=Levels();
        rows.Add(new(){OrganizationId="company",AssetType="WORK_ORDER",HierarchyLevel=fault=="parent"?1:0,ParentLevel=fault=="parent"?0:null,LevelName="Orders",Active=true});
        if(fault=="duplicate")rows.Add(rows[^1]);
        var store=new RepositoryHierarchyConfigurationStore(f.Db,f.Gate);
        Assert.Equal(RefusalKind.Invalid,(await Assert.ThrowsAsync<RefusalException>(()=>store.UpdateAsync("company",rows))).Kind);
        Assert.Empty(await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().ToListAsync());
    }
    private static List<HierarchyConfig> Levels(string company = "company", string? revision = null) =>
        new[] { "FIELD", "WELL", "POOL", "FACILITY" }.Select(type => new HierarchyConfig
        { OrganizationId = company, AssetType = type, HierarchyLevel = type == "FIELD" ? 0 : 1, ParentLevel = type == "FIELD" ? null : 0,
            LevelName = type == "FIELD" ? "حقل" : type, Active = true, Revision = revision }).ToList();

    [Fact]
    public async Task An_atomic_create_and_update_record_server_actor_time_revision_and_immutable_history()
    {
        using var f = await Fixture.Create(); var clock = new Clock(); var store = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate, clock);
        Assert.Empty(await store.ReadAsync("company")); var draft = Levels(); draft[0].ROW_CREATED_BY = "spoofed";
        Assert.True(await store.UpdateAsync("company", draft));
        var first = await store.ReadAsync("company", true); var revision = Assert.Single(first.Select(row => row.Revision).Distinct());
        Assert.True(Guid.TryParseExact(revision, "D", out _)); Assert.Equal("حقل", first.Single(row => row.AssetType == "FIELD").LevelName);
        var row = await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().AsNoTracking().SingleAsync();
        Assert.Equal("tenant", row.TENANT_ID); Assert.Equal("company", row.ORGANIZATION_ID); Assert.Equal("actor", row.CREATED_BY); Assert.Equal("actor", row.CHANGED_BY);
        Assert.Equal(clock.GetUtcNow().UtcDateTime, row.CREATED_UTC); Assert.DoesNotContain("spoofed", row.LEVELS_JSON); Assert.DoesNotContain("ROW_CREATED", row.LEVELS_JSON);
        var before = await f.Db.Set<OrganizationHierarchyConfigRevision>().AsNoTracking().SingleAsync(); Assert.Null(before.PreviousRevision);
        first.Single(level => level.AssetType == "WELL").LevelName = "Wells in service"; clock.Now = clock.Now.AddMinutes(1);
        Assert.True(await store.UpdateAsync("company", first)); var second = await store.ReadAsync("company");
        Assert.All(second, level => Assert.NotEqual(revision, level.Revision));
        var history = await f.Db.Set<OrganizationHierarchyConfigRevision>().AsNoTracking().OrderBy(value => value.ChangedUtc).ToListAsync();
        Assert.Equal(2, history.Count); Assert.Equal(before.LevelsJson, history[0].LevelsJson); Assert.Equal(revision, history[1].PreviousRevision);
        Assert.Equal("actor", history[1].ActorUserId); Assert.Equal(clock.GetUtcNow().UtcDateTime, history[1].ChangedUtc);
        Assert.False(f.Db.ChangeTracker.HasChanges()); Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("null")] [InlineData("empty")] [InlineData("missing-type")] [InlineData("duplicate-type")]
    [InlineData("unknown-type")] [InlineData("null-row")] [InlineData("wrong-company")] [InlineData("blank-name")]
    [InlineData("long-name")] [InlineData("control-name")] [InlineData("field-parent")] [InlineData("child-parent")]
    [InlineData("field-depth")] [InlineData("child-depth")] [InlineData("inactive-parent")] [InlineData("mixed-revisions")]
    [InlineData("invalid-revision")]
    public async Task Invalid_configuration_is_refused_without_persistent_or_audit_changes(string reason)
    {
        using var f = await Fixture.Create(); var draft = Levels();
        switch (reason)
        {
            case "null": draft = null!; break; case "empty": draft.Clear(); break; case "missing-type": draft.RemoveAt(0); break;
            case "duplicate-type": draft[1].AssetType = "FIELD"; break; case "unknown-type": draft[1].AssetType = "AREA"; break;
            case "null-row": draft[1] = null!; break; case "wrong-company": draft[1].OrganizationId = "other"; break;
            case "blank-name": draft[1].LevelName = " "; break; case "long-name": draft[1].LevelName = new string('a', 129); break;
            case "control-name": draft[1].LevelName = "a\nb"; break; case "field-parent": draft[0].ParentLevel = 1; break;
            case "child-parent": draft[1].ParentLevel = 1; break; case "field-depth": draft[0].HierarchyLevel = 1; break;
            case "child-depth": draft[1].HierarchyLevel = 2; break; case "inactive-parent": draft[0].Active = false; break;
            case "mixed-revisions": draft[1].Revision = Guid.NewGuid().ToString(); break;
            case "invalid-revision": foreach (var row in draft) row.Revision = "bad"; break;
        }
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => new RepositoryHierarchyConfigurationStore(f.Db, f.Gate).UpdateAsync("company", draft));
        Assert.Equal(RefusalKind.Invalid, refusal.Kind); Assert.Empty(await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().ToListAsync()); Assert.Empty(await f.Db.Set<OrganizationHierarchyConfigRevision>().ToListAsync());
        Assert.False(f.Db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData("stale")] [InlineData("missing")] [InlineData("unexpected")]
    public async Task Expected_revision_is_a_required_compare_and_swap_without_lost_updates(string reason)
    {
        using var f = await Fixture.Create(); var store = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate);
        if (reason != "unexpected") Assert.True(await store.UpdateAsync("company", Levels()));
        var expected = reason == "unexpected" ? Guid.NewGuid().ToString() : (await store.ReadAsync("company"))[0].Revision;
        if (reason == "stale") Assert.True(await store.UpdateAsync("company", Levels(revision: expected)));
        var count = await f.Db.Set<OrganizationHierarchyConfigRevision>().CountAsync();
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => store.UpdateAsync("company", Levels(revision: reason == "missing" ? null : expected)));
        Assert.Equal(RefusalKind.Conflict, refusal.Kind); Assert.Equal(count, await f.Db.Set<OrganizationHierarchyConfigRevision>().CountAsync()); Assert.False(f.Db.ChangeTracker.HasChanges());
    }

    [Theory]
    [InlineData("organization")] [InlineData("membership")] [InlineData("role")] [InlineData("account")]
    public async Task Current_company_authority_is_required_before_a_configuration_command(string reason)
    {
        using var f = await Fixture.Create();
        if (reason == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
        if (reason == "role") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
        if (reason == "account") (await f.Db.Users.SingleAsync()).IsActive = false;
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => new RepositoryHierarchyConfigurationStore(f.Db, f.Gate).UpdateAsync(reason == "organization" ? "other" : "company", Levels()));
        Assert.Empty(await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().ToListAsync()); Assert.Empty(await f.Db.Set<OrganizationHierarchyConfigRevision>().ToListAsync());
    }

    [Fact]
    public async Task Members_can_read_display_configuration_but_cannot_administer_it()
    {
        using var f = await Fixture.Create(); var store = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate); Assert.True(await store.UpdateAsync("company", Levels()));
        f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync(); await f.Select("company"); store = new(f.Db, f.Gate);
        Assert.Equal(4, (await store.ReadAsync("company")).Count);
        await Assert.ThrowsAsync<RefusalException>(() => store.ReadAsync("company", true));
        await Assert.ThrowsAsync<RefusalException>(() => store.UpdateAsync("company", Levels()));
        Assert.Single(await f.Db.Set<OrganizationHierarchyConfigRevision>().ToListAsync());
    }

    [Fact]
    public async Task Configurations_are_separate_for_equal_company_ids_in_different_tenants()
    {
        using var f = await Fixture.Create(); var first = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate);
        Assert.True(await first.UpdateAsync("company", Levels()));
        var otherGate = await TestCompanyModules.CreateAsync(f.Db, "actor", "company", "other-tenant"); var second = new RepositoryHierarchyConfigurationStore(f.Db, otherGate);
        Assert.Empty(await second.ReadAsync("company")); var other = Levels(); other[0].LevelName = "Other tenant field"; Assert.True(await second.UpdateAsync("company", other));
        Assert.Equal("حقل", (await first.ReadAsync("company")).Single(row => row.AssetType == "FIELD").LevelName);
        Assert.Equal("Other tenant field", (await second.ReadAsync("company")).Single(row => row.AssetType == "FIELD").LevelName);
        Assert.Equal(2, await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().CountAsync()); Assert.Equal(2, await f.Db.Set<OrganizationHierarchyConfigRevision>().CountAsync());
    }

    [Theory]
    [InlineData("before-save")] [InlineData("after-save")]
    public async Task A_provider_or_post_save_failure_rolls_back_the_aggregate_and_audit_and_clears_pending_entities(string stage)
    {
        using var f = await Fixture.Create();
        await using var isolated = new CompanyPPDMRowAccessTests.Context(new DbContextOptionsBuilder<CompanyPPDMRowAccessTests.Context>()
            .UseSqlite(f.Db.Database.GetDbConnection()).AddInterceptors(new FailingSave(stage)).Options);
        var gate = await TestCompanyModules.CreateAsync(isolated, "actor"); var store = new RepositoryHierarchyConfigurationStore(isolated, gate);
        await Assert.ThrowsAsync<InvalidOperationException>(() => store.UpdateAsync("company", Levels()));
        Assert.Empty(await f.Db.Set<ORGANIZATION_HIERARCHY_CONFIG>().AsNoTracking().ToListAsync()); Assert.Empty(await f.Db.Set<OrganizationHierarchyConfigRevision>().AsNoTracking().ToListAsync());
        Assert.False(isolated.ChangeTracker.HasChanges()); Assert.Null(isolated.Database.CurrentTransaction);
    }

    [Fact]
    public async Task Configuration_does_not_require_or_change_a_PPDM_module_binding()
    {
        using var f = await Fixture.Create(); f.Db.ModuleDatabases.Remove(await f.Db.ModuleDatabases.SingleAsync()); await f.Db.SaveChangesAsync();
        var store = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate); Assert.True(await store.UpdateAsync("company", Levels()));
        Assert.Equal(4, (await store.ReadAsync("company", true)).Count); Assert.Empty(f.Connections); Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Display_labels_and_visibility_use_the_persisted_configuration_without_mutating_asset_authority()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); _ = CompanyAssetFacadeTests.Service(f);
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", FIELD_NAME = "Actual field", ACTIVE_IND = "Y" },
            new POOL { POOL_ID = "pool", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" },
            new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" }]);
        var configuration = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate); var levels = Levels(); levels.Single(row => row.AssetType == "POOL").Active = false;
        Assert.True(await configuration.UpdateAsync("company", levels)); var assets = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        var service = new AssetHierarchyService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, () => f.Gate.ResolveAsync(),
            subject => new CompanyPPDMRowAccessPolicy(f.Gate, assets, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject), subject => f.Gate.ResolveHierarchyCompanyAsync(subject), configuration, order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
        var hierarchy = await service.GetAssetHierarchyForUserAsync("actor"); var field = Assert.Single(hierarchy!.Children);
        Assert.Equal("حقل", field.LevelName); Assert.Equal("Actual field", field.AssetName); Assert.Equal("WELL", Assert.Single(field.Children).AssetType);
        Assert.True(await service.ValidateAccessAsync("actor", [new() { AssetId = "pool", AssetType = "POOL" }]));
        Assert.True((await CompanyAssetFacadeTests.Service(f).CheckAssetAccessAsync("actor", "pool", "POOL")).HasAccess); Assert.Single(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Fact]
    public async Task Configuration_changes_during_PPDM_reads_refuse_the_old_display_snapshot()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); _ = CompanyAssetFacadeTests.Service(f); f.Rows.Add(new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" });
        var configuration = new RepositoryHierarchyConfigurationStore(f.Db, f.Gate); Assert.True(await configuration.UpdateAsync("company", Levels()));
        var assets = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        var service = new AssetHierarchyService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, () => f.Gate.ResolveAsync(),
            subject => new CompanyPPDMRowAccessPolicy(f.Gate, assets, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject), subject => f.Gate.ResolveHierarchyCompanyAsync(subject), configuration, order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
        f.AfterRead = async () => { var rows = await configuration.ReadAsync("company"); rows[0].LevelName = "Updated"; await configuration.UpdateAsync("company", rows); };
        var refusal = await Assert.ThrowsAsync<RefusalException>(() => service.GetAssetHierarchyForUserAsync("actor", rootAssetId: "A", rootAssetType: "FIELD"));
        Assert.Equal(RefusalKind.Conflict, refusal.Kind);
    }

    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    private sealed class FailingSave(string stage) : SaveChangesInterceptor
    {
        private bool relevant;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData data, InterceptionResult<int> result, CancellationToken token = default)
        {
            relevant = data.Context!.ChangeTracker.Entries<OrganizationHierarchyConfigRevision>().Any(entry => entry.State == EntityState.Added);
            if (relevant && stage == "before-save") throw new InvalidOperationException("provider failure");
            return ValueTask.FromResult(result);
        }
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData data, int result, CancellationToken token = default)
        {
            if (relevant && stage == "after-save") throw new InvalidOperationException("post-save failure");
            return ValueTask.FromResult(result);
        }
    }
}
