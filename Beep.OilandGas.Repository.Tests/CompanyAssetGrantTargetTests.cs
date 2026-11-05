using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetGrantTargetTests
{
    private static PPDMAssetGrantTargetVerifier Verifier(Fixture f) => new(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility);
    private static RepositoryAssetAccessStore Store(Fixture f) => new(f.Db, f.Gate, Verifier(f));
    private static async Task<Fixture> OrderReady(string secondLevel = "WRITE")
    {
        var f = await Fixture.Create();
        _ = CompanyAssetFacadeTests.Service(f);
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
        f.Rows.Add(new WORK_ORDER { WORK_ORDER_ID = "order", ACTIVE_IND = "Y" });
        f.Rows.Add(new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = "east", FIELD_ID = "east", ACTIVE_IND = "Y" });
        f.Rows.Add(new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = "west", FIELD_ID = "west", ACTIVE_IND = "Y" });
        f.AddFieldEntitlement("east", "WRITE"); f.AddFieldEntitlement("west", secondLevel);
        return f;
    }

    [Fact]
    public async Task Direct_order_grant_requires_every_recorded_component_field_and_saves_in_existing_store()
    {
        using var f = await OrderReady(); var scope = await f.Gate.ResolveAsync();
        Assert.True(await Store(f).GrantAsync("actor", scope.Fingerprint, "WORK_ORDER", "order", "WRITE", false));
        var saved = await f.Db.Set<AppUserAssetAccess>().SingleAsync();
        Assert.Equal("WORK_ORDER", saved.AssetType); Assert.Equal("order", saved.AssetId); Assert.False(saved.Inherit);
        Assert.Equal("company", saved.OrganizationId); Assert.Equal(scope.Fingerprint, saved.DatabaseScope);
        Assert.Contains(f.EligibilityQueries, query => query.Filters.Any(filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "east"));
        Assert.Contains(f.EligibilityQueries, query => query.Filters.Any(filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "west"));
        Assert.Empty(f.Writes);
    }

    [Theory][InlineData("READ")][InlineData("revoked")]
    public async Task A_second_field_that_cannot_permit_write_prevents_order_grant(string secondLevel)
    {
        using var f = await OrderReady(secondLevel == "revoked" ? "WRITE" : secondLevel);
        if (secondLevel == "revoked") f.Rows.OfType<ENTITLEMENT>().Single(row => row.ENTITLEMENT_ID.EndsWith(":west")).ACTIVE_IND = "N";
        var scope = await f.Gate.ResolveAsync();
        await Forbidden(() => Store(f).GrantAsync("actor", scope.Fingerprint, "WORK_ORDER", "order", "WRITE", false));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Theory][InlineData("missing")][InlineData("duplicate")][InlineData("unscoped")]
    public async Task Incomplete_order_component_scope_refuses_admission_without_saving(string fault)
    {
        using var f = await OrderReady();
        if (fault == "missing") f.Rows.RemoveAll(row => row is WORK_ORDER_COMPONENT);
        if (fault == "duplicate") f.Rows.Add(new WORK_ORDER_COMPONENT { WORK_ORDER_ID = "order", COMPONENT_ID = "east", FIELD_ID = "west", ACTIVE_IND = "Y" });
        if (fault == "unscoped") f.Rows.OfType<WORK_ORDER_COMPONENT>().First().FIELD_ID = "";
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "WORK_ORDER", "order", "WRITE", false))).Kind);
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Fact]
    public async Task Component_reparenting_during_entitlement_review_refuses_order_grant()
    {
        using var f = await OrderReady(); var changed = false;
        f.AfterEligibilityRead = () => { if (!changed) { changed = true; f.Rows.OfType<WORK_ORDER_COMPONENT>().First().FIELD_ID = "west"; } return Task.CompletedTask; };
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "WORK_ORDER", "order", "WRITE", false))).Kind);
        Assert.True(changed); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Theory][InlineData("WRITE", "WRITE")][InlineData("READ", "READ")]
    public async Task Effective_order_access_is_capped_by_every_current_field_entitlement(string secondLevel, string expected)
    {
        using var f = await OrderReady(secondLevel); await f.Grant("WORK_ORDER", "order", "WRITE");
        Assert.Equal(expected, await f.Policy.GetEffectiveAccessLevelAsync(typeof(WORK_ORDER), "WORK_ORDER", f.Rows.OfType<WORK_ORDER>().Single()));
    }

    [Fact]
    public async Task Direct_order_grant_downgrade_during_parent_review_cannot_retain_write()
    {
        using var f = await OrderReady(); await f.Grant("WORK_ORDER", "order", "WRITE"); var changed = false;
        f.AfterEligibilityRead = async () =>
        {
            if (changed) return; changed = true; var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync();
            grant.AccessLevel = "READ"; grant.ConcurrencyStamp = Guid.NewGuid().ToString(); await f.Db.SaveChangesAsync();
        };
        Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(WORK_ORDER), "WORK_ORDER", f.Rows.OfType<WORK_ORDER>().Single()));
        Assert.True(changed);
    }

    [Fact]
    public async Task Inherited_order_grant_requests_are_refused_by_the_existing_store()
    {
        using var f = await OrderReady(); var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "WORK_ORDER", "order", "WRITE", true))).Kind);
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync()); Assert.Empty(f.Queries);
    }
    private static async Task<Fixture> Ready(string assetType = "FIELD")
    {
        var f = await Fixture.Create(); f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(ConnectionState.Open);
        _ = CompanyAssetFacadeTests.Service(f);
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
        f.Rows.Add(assetType switch
        {
            "WELL" => new WELL { UWI = "asset", ACTIVE_IND = "Y", OPERATOR = "company" }, "POOL" => new POOL { POOL_ID = "asset", ACTIVE_IND = "Y", BUSINESS_ASSOCIATE_ID = "company" },
            _ => new FIELD { FIELD_ID = "asset", ACTIVE_IND = "Y" }
        });
        if (assetType == "FIELD") f.AddFieldEntitlement("asset");
        return f;
    }

    [Theory]
    [InlineData("FIELD", "FIELD_ID")]
    [InlineData("WELL", "UWI")]
    [InlineData("POOL", "POOL_ID")]
    public async Task The_actual_store_verifies_active_company_and_exact_asset_before_saving_a_grant(string type, string key)
    {
        using var f = await Ready(type); var scope = await f.Gate.ResolveAsync();
        Assert.True(await Store(f).GrantAsync("actor", scope.Fingerprint, type, "asset", "WRITE", true));
        var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(); Assert.Equal("company", grant.OrganizationId); Assert.Equal("asset", grant.AssetId);
        Assert.Equal("actor", grant.CreatedBy); Assert.Equal(scope.Fingerprint, grant.DatabaseScope);
        Assert.Equal(2, f.Queries.Count); Assert.Equal(2, f.Queries[0].Count); Assert.Equal(type == "FIELD" ? 2 : 3, f.Queries[1].Count);
        Assert.Contains(f.Queries[0], filter => filter.FieldName == "BUSINESS_ASSOCIATE_ID" && filter.Operator == "=" && filter.FilterValue == "company");
        Assert.Contains(f.Queries[1], filter => filter.FieldName == key && filter.Operator == "=" && filter.FilterValue == "asset");
        Assert.All(f.Queries, query => Assert.Contains(query, filter => filter.FieldName == "ACTIVE_IND" && filter.FilterValue == "Y"));
        Assert.All(f.Connections, value => Assert.Equal(scope.ConnectionName, value)); Assert.Empty(f.Writes);
        f.Editor.Verify(editor => editor.OpenDataSource(scope.ConnectionName), Times.Exactly(type == "FIELD" ? 2 : 1));
    }

    [Theory]
    [InlineData("missing-asset")]
    [InlineData("inactive-asset")]
    [InlineData("missing-company")]
    [InlineData("inactive-company")]
    public async Task Missing_or_inactive_company_and_asset_records_cannot_admit_grants(string reason)
    {
        using var f = await Ready();
        if (reason == "missing-asset") f.Rows.RemoveAll(row => row is FIELD);
        if (reason == "inactive-asset") ((FIELD)f.Rows.Single(row => row is FIELD)).ACTIVE_IND = "N";
        if (reason == "missing-company") f.Rows.RemoveAll(row => row is BUSINESS_ASSOCIATE);
        if (reason == "inactive-company") ((BUSINESS_ASSOCIATE)f.Rows.Single(row => row is BUSINESS_ASSOCIATE)).ACTIVE_IND = "N";
        var scope = await f.Gate.ResolveAsync();
        await Forbidden(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Theory]
    [InlineData("wrong-asset")]
    [InlineData("wrong-company")]
    [InlineData("wrong-active")]
    [InlineData("duplicate")]
    public async Task Provider_rows_must_honor_exact_identity_active_state_and_cardinality(string reason)
    {
        using var f = await Ready(); f.IgnoreFilters = true;
        if (reason == "wrong-asset") ((FIELD)f.Rows.Single(row => row is FIELD)).FIELD_ID = "ASSET";
        if (reason == "wrong-company") ((BUSINESS_ASSOCIATE)f.Rows.Single(row => row is BUSINESS_ASSOCIATE)).BUSINESS_ASSOCIATE_ID = "other";
        if (reason == "wrong-active") ((FIELD)f.Rows.Single(row => row is FIELD)).ACTIVE_IND = "N";
        if (reason == "duplicate") f.Rows.Add(new FIELD { FIELD_ID = "asset", ACTIVE_IND = "Y" });
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false))).Kind);
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Theory]
    [InlineData("failed-open")]
    [InlineData("provider-throw")]
    [InlineData("provider-error")]
    public async Task SDK_failures_cannot_be_reported_as_verified_targets(string reason)
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        if (reason == "failed-open") f.Editor.Setup(editor => editor.OpenDataSource(scope.ConnectionName)).Returns(ConnectionState.Broken);
        if (reason == "provider-throw") f.AfterRead = () => throw new InvalidOperationException("Provider refused the read");
        if (reason == "provider-error") f.Source.SetupGet(source => source.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync()); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("company-meta")]
    [InlineData("asset-meta")]
    public async Task Metadata_identity_drift_refuses_admission_before_opening_a_datasource(string table)
    {
        using var f = await Ready(); var name = table == "company-meta" ? "BUSINESS_ASSOCIATE" : "FIELD";
        var definition = await f.Metadata.Object.GetTableMetadataAsync(name); definition.PrimaryKeyColumn = "WRONG";
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false))).Kind);
        Assert.Empty(f.Connections); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
        f.Editor.Verify(editor => editor.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task An_ambiguous_direct_facility_identity_is_refused_without_a_provider_read()
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FACILITY", "plant", "READ", false))).Kind);
        Assert.Empty(f.Connections); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Theory]
    [InlineData("binding")]
    [InlineData("administrator")]
    [InlineData("membership")]
    public async Task Authority_and_target_changes_during_SDK_verification_are_rechecked_before_persistence(string change)
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        f.AfterRead = async () =>
        {
            if (f.Queries.Count != 2) return;
            if (change == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            if (change == "administrator") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
            if (change == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Fact]
    public async Task A_target_membership_lost_during_verification_is_not_assigned_access()
    {
        using var f = await Ready(); f.Db.Users.Add(new() { Id = "member", UserName = "member", IsActive = true }); await f.Db.SaveChangesAsync(); await f.Select("company");
        var scope = await f.Gate.ResolveAsync(); f.AfterRead = async () =>
        {
            if (f.Queries.Count != 2) return;
            (await f.Db.Set<AppCompanyMembership>().SingleAsync(member => member.UserId == "member")).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            await f.Db.SaveChangesAsync();
        };
        Assert.False(await Store(f).GrantAsync("member", scope.Fingerprint, "FIELD", "asset", "READ", false));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Fact]
    public async Task Failed_renewal_preserves_existing_grant_level_audit_and_version_and_cleanup_still_works()
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync(); var store = Store(f);
        Assert.True(await store.GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false));
        var before = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); f.Rows.RemoveAll(row => row is FIELD);
        await Forbidden(() => store.GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "DELETE", true));
        var after = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync();
        Assert.Equal(before.AccessLevel, after.AccessLevel); Assert.Equal(before.ChangedUtc, after.ChangedUtc); Assert.Equal(before.ConcurrencyStamp, after.ConcurrencyStamp);
        Assert.True(await store.RevokeAsync("actor", scope.Fingerprint, "FIELD", "asset"));
        Assert.False((await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive);
    }

    [Fact]
    public async Task Independent_verification_cannot_redirect_to_a_requested_fingerprint()
    {
        using var f = await Ready();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Verifier(f).ValidateAsync(new string('A', 64), "FIELD", "asset", "READ"))).Kind);
        Assert.Empty(f.Connections);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SDK_dictionary_and_DataRow_results_use_the_existing_repository_conversion(bool dataRow)
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        f.Source.Setup(source => source.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()))
            .ReturnsAsync((string table, List<TheTechIdea.Beep.Report.AppFilter> filters) =>
            {
                var key = table == "BUSINESS_ASSOCIATE" ? "BUSINESS_ASSOCIATE_ID" : "FIELD_ID";
                var id = table == "BUSINESS_ASSOCIATE" ? "company" : "asset";
                if (table != "BUSINESS_ASSOCIATE" && table != "FIELD")
                    return (IEnumerable<object>)f.Rows.Where(row => row.GetType().Name == table && filters.All(filter =>
                        Convert.ToString(row.GetType().GetProperty(filter.FieldName)!.GetValue(row)) == filter.FilterValue)).ToArray();
                if (!dataRow) return (IEnumerable<object>)[new Dictionary<string, object> { [key] = id, ["ACTIVE_IND"] = "Y" }];
                var result = new DataTable(); result.Columns.Add(key, typeof(string)); result.Columns.Add("ACTIVE_IND", typeof(string));
                result.Rows.Add(id, "Y"); return result.Rows.Cast<object>().ToArray();
            });
        Assert.True(await Store(f).GrantAsync("actor", scope.Fingerprint, "FIELD", "asset", "READ", false));
        Assert.Single(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    private static async Task Forbidden(Func<Task> action) => Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(action)).Kind);
}

