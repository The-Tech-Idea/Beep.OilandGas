using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetFacadeTests
{
    internal static UserAssetAccessService Service(Fixture f, Func<Task<AssetDatabaseScope>>? resolve = null,
        IAssetGrantTargetVerifier? admission = null, IApplicationAuthorizationReader? authorization = null)
    {
        var configured = new ConnectionProperties { ConnectionName = "selected-tenant", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "assets-tenant" };
        var config = new Mock<IConfigEditor>(); config.SetupGet(value => value.DataConnections).Returns([configured]);
        f.Editor.SetupGet(value => value.ConfigEditor).Returns(config.Object);
        var connection = new Mock<IDataConnection>(); connection.SetupGet(value => value.ConnectionProp).Returns(configured);
        f.Source.SetupGet(value => value.DatasourceName).Returns(configured.ConnectionName);
        f.Source.SetupGet(value => value.Dataconnection).Returns(connection.Object);
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, admission ?? Mock.Of<IAssetGrantTargetVerifier>());
        return new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
            authorization ?? new RepositoryApplicationAuthorizationReader(f.Gate), Mock.Of<IApplicationRolePermissionStore>(), store,
            resolve ?? (() => f.Gate.ResolveAsync()), user => new CompanyPPDMRowAccessPolicy(f.Gate, store, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), user),
            new PPDMCompanyFieldInterestReview(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy,
                Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration)));
    }
    private static object Row(string table, string id = "asset", string company = "company", string active = "Y") => table switch
    {
        "FIELD" => new FIELD { FIELD_ID = id, ACTIVE_IND = active },
        "WELL" => new WELL { UWI = id, OPERATOR = company, ASSIGNED_FIELD = "A", ACTIVE_IND = active },
        "POOL" => new POOL { POOL_ID = id, BUSINESS_ASSOCIATE_ID = company, FIELD_ID = "A", ACTIVE_IND = active },
        _ => new FACILITY { FACILITY_ID = id, FACILITY_TYPE = "gas", CURRENT_OPERATOR = company, PRIMARY_FIELD_ID = "A", ACTIVE_IND = active }
    };

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Actual_access_facade_inheritance_uses_current_company_policy_and_active_stored_rows(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE", true);
        var foreign=Row(table,"foreign","other");if(foreign is FACILITY facility)facility.PRIMARY_FIELD_ID="B";
        f.Rows.AddRange([Row(table),foreign,Row(table,"inactive",active:"N")]);
        var result = await Service(f).GetUserAccessibleAssetsAsync("actor", table);
        var asset = Assert.Single(result); Assert.Equal("asset", asset.AssetId); Assert.Equal("WRITE", asset.AccessLevel);
        Assert.Equal("company", asset.OrganizationId); Assert.Equal("actor", asset.UserId); Assert.False(asset.Inherit);
        Assert.Equal(table == "FACILITY" ? "gas" : null, asset.FacilityType);
        Assert.Contains(Assert.Single(f.Queries), filter => filter.FieldName == "ACTIVE_IND" && filter.FilterValue == "Y");
        Assert.Equal(table=="FACILITY"?2:3, f.Queries[0].Count); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("FIELD", "missing")] [InlineData("FIELD", "inactive")]
    [InlineData("WELL", "missing")] [InlineData("WELL", "inactive")] [InlineData("WELL", "other-company")]
    [InlineData("POOL", "missing")] [InlineData("POOL", "inactive")] [InlineData("POOL", "other-company")]
    public async Task A_direct_registry_record_cannot_report_access_to_a_missing_inactive_or_foreign_asset(string table, string reason)
    {
        using var f = await Fixture.Create(); await f.Grant(table, "asset");
        if (reason != "missing") f.Rows.Add(Row(table, company: reason == "other-company" ? "other" : "company", active: reason == "inactive" ? "N" : "Y"));
        var service = Service(f);
        Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", table, includeInherited: false));
        Assert.False((await service.CheckAssetAccessAsync("actor", "asset", table)).HasAccess);
        Assert.True((await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")]
    public async Task Direct_only_results_do_not_promote_field_inheritance(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE", true); await f.Grant(table, "asset", "READ");
        f.Rows.AddRange([Row(table), Row(table, "inherited")]);
        var service = Service(f); var direct = Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", table, includeInherited: false));
        Assert.Equal("asset", direct.AssetId); Assert.Equal("READ", direct.AccessLevel);
        var all = await service.GetUserAccessibleAssetsAsync("actor", table); Assert.Equal(2, all.Count); Assert.All(all, asset => Assert.Equal("WRITE", asset.AccessLevel));
    }

    [Fact]
    public async Task Facility_results_and_checks_preserve_the_complete_composite_key()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "DELETE", true);
        f.Rows.AddRange([Row("FACILITY"), new FACILITY { FACILITY_ID = "asset", FACILITY_TYPE = "oil", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }]);
        var service = Service(f); var result = await service.GetUserAccessibleAssetsAsync("actor", "FACILITY");
        Assert.Equal(new[] { "gas", "oil" }, result.Select(asset => asset.FacilityType).Order());
        Assert.All(result, asset => { Assert.Equal("asset", asset.AssetId); Assert.Equal("DELETE", asset.AccessLevel); });
        Assert.True((await service.CheckAssetAccessAsync("actor", "asset", "FACILITY", facilityType: "gas")).HasAccess);
        Assert.True((await service.CheckAssetAccessAsync("actor", "asset", "FACILITY", facilityType: "oil")).HasAccess);
        Assert.False((await service.CheckAssetAccessAsync("actor", "asset", "FACILITY", facilityType: "water")).HasAccess);
        await Assert.ThrowsAsync<RefusalException>(() => service.CheckAssetAccessAsync("actor", "asset", "FACILITY"));
        Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY", includeInherited: false));
    }

    [Fact]
    public async Task A_historical_ambiguous_direct_facility_grant_does_not_authorize_any_facility_type()
    {
        using var f = await Fixture.Create();
        f.Db.Add(new AppUserAssetAccess { Id = "historical-unqualified-facility", UserId = "actor", AssetType = "FACILITY", AssetId = "asset",
            DatabaseScope = (await f.Gate.ResolveAsync()).Fingerprint, OrganizationId = "company", AccessLevel = "READ", IsActive = true, CreatedBy = "actor", ChangedBy = "actor" });
        await f.Db.SaveChangesAsync(); f.Rows.Add(Row("FACILITY"));
        var service = Service(f); Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY"));
        Assert.False((await service.CheckAssetAccessAsync("actor", "asset", "FACILITY", facilityType: "gas")).HasAccess);
        Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Provider_filter_bypasses_propagate_a_refusal_through_the_access_facade(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        var foreign=Row(table,company:"other");if(foreign is FACILITY facility)facility.PRIMARY_FIELD_ID="B";
        f.Rows.Add(foreign);f.IgnoreFilters=true;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetUserAccessibleAssetsAsync("actor", table)); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("target")] [InlineData("name")] [InlineData("duplicate")] [InlineData("missing")]
    public async Task A_stale_or_ambiguous_cached_datasource_cannot_supply_access_results(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.Add(Row("WELL")); var service = Service(f);
        var configured = (ConnectionProperties)Assert.Single(f.Editor.Object.ConfigEditor.DataConnections);
        if (reason == "target") Mock.Get(f.Source.Object.Dataconnection).SetupGet(value => value.ConnectionProp).Returns(new ConnectionProperties { ConnectionName = configured.ConnectionName, Database = "other" });
        if (reason == "name") f.Source.SetupGet(value => value.DatasourceName).Returns("other");
        if (reason == "duplicate") f.Editor.Object.ConfigEditor.DataConnections.Add(configured);
        if (reason == "missing") f.Editor.Object.ConfigEditor.DataConnections.Clear();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckAssetAccessAsync("actor", "asset", "WELL"));
        Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task A_cached_target_changed_during_read_cannot_be_reported_as_access()
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.Add(Row("WELL")); var service = Service(f);
        f.AfterRead = () => { Mock.Get(f.Source.Object.Dataconnection).SetupGet(value => value.ConnectionProp).Returns(new ConnectionProperties { ConnectionName = "selected-tenant", Database = "other" }); return Task.CompletedTask; };
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckAssetAccessAsync("actor", "asset", "WELL"));
    }

    [Theory]
    [InlineData("binding")] [InlineData("membership")] [InlineData("asset")]
    public async Task Fresh_policy_checks_refuse_authority_changes_during_access_reads(string change)
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.Add(Row("WELL"));
        f.AfterRead = async () =>
        {
            if (change == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            if (change == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            if (change == "asset") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).CheckAssetAccessAsync("actor", "asset", "WELL"));
    }

    [Fact]
    public async Task A_company_administrator_inspects_the_subjects_grants_without_substituting_the_actors_grants()
    {
        using var f = await Fixture.Create(); f.Db.Users.Add(new() { Id = "member", UserName = "member", IsActive = true }); await f.Db.SaveChangesAsync(); await f.Select("company");
        var scope = await f.Gate.ResolveAsync(); var store = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        Assert.True(await store.GrantAsync("member", scope.Fingerprint, "WELL", "asset", "READ", false));
        f.Rows.Add(Row("WELL")); var service = Service(f);
        Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", "WELL"));
        Assert.Equal("member", Assert.Single(await service.GetUserAccessibleAssetsAsync("member", "WELL")).UserId);
        Assert.Single(f.Queries);
    }

    [Fact]
    public async Task An_ordinary_member_cannot_inspect_another_members_asset_access()
    {
        using var f = await Fixture.Create(); f.Db.Users.Add(new() { Id = "member", UserName = "member", IsActive = true });
        f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync(); await f.Select("company");
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetUserAccessibleAssetsAsync("member", "WELL")); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Access_levels_are_recomputed_from_fresh_grants_after_the_data_read()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "DELETE", true); f.Rows.Add(Row("WELL"));
        var calls = 0; var service = Service(f, async () =>
        {
            if (++calls == 2) { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).AccessLevel = "READ"; await f.Db.SaveChangesAsync(); }
            return await f.Gate.ResolveAsync();
        });
        Assert.Equal("READ", Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", "WELL")).AccessLevel);
    }

    [Theory]
    [InlineData("READ")] [InlineData("WRITE")] [InlineData("DELETE")]
    public async Task Access_level_never_substitutes_for_required_current_application_permission(string level)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", level); f.Rows.Add(Row("FIELD", "A")); var service = Service(f);
        Assert.True((await service.CheckAssetAccessAsync("actor", "A", "FIELD", PermissionConstants.DataManagement.ImportData)).HasAccess);
        Assert.False((await service.CheckAssetAccessAsync("actor", "A", "FIELD", "unknown.permission")).HasAccess);
    }

    [Fact]
    public async Task Unknown_asset_paths_and_misapplied_facility_keys_refuse_without_querying_the_provider()
    {
        using var f = await Fixture.Create(); var service = Service(f);
        await Assert.ThrowsAsync<RefusalException>(() => service.GetUserAccessibleAssetsAsync("actor", "UNKNOWN"));
        await Assert.ThrowsAsync<RefusalException>(() => service.CheckAssetAccessAsync("actor", "asset", "WELL", facilityType: "gas"));
        Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task An_unresolved_scope_is_an_explicit_failure_for_reads_checks_and_mutations()
    {
        using var f = await Fixture.Create(); var service = Service(f, () => throw new InvalidOperationException("No current binding"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserAccessibleAssetsAsync("actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckAssetAccessAsync("actor", "asset", "WELL"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GrantAssetAccessAsync("actor", "asset", "WELL"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RevokeAssetAccessAsync("actor", "asset", "WELL")); Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("FIELD")] [InlineData("WELL")] [InlineData("POOL")]
    public async Task The_facade_routes_grant_and_revoke_through_actual_current_scope_admission_and_registry(string table)
    {
        using var f = await Fixture.Create(); f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" }); f.Rows.Add(Row(table));
        if (table == "FIELD") f.AddFieldEntitlement("asset");
        f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(System.Data.ConnectionState.Open);
        var verifier = new PPDMAssetGrantTargetVerifier(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility);
        var service = Service(f, admission: verifier);
        Assert.True(await service.GrantAssetAccessAsync("actor", "asset", table, "WRITE", false));
        var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(); Assert.Equal("actor", grant.CreatedBy);
        Assert.Equal((await f.Gate.ResolveAsync()).Fingerprint, grant.DatabaseScope);
        Assert.Equal("asset", Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", table, includeInherited: false)).AssetId);
        Assert.True(await service.RevokeAssetAccessAsync("actor", "asset", table));
        Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", table)); Assert.False(grant.IsActive); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task A_reported_provider_failure_is_not_an_access_denial_or_success(bool returnsRow)
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); if (returnsRow) f.Rows.Add(Row("WELL"));
        f.Source.SetupGet(source => source.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).CheckAssetAccessAsync("actor", "asset", "WELL"));
    }

    [Fact]
    public async Task A_scope_changed_after_read_cannot_return_the_previous_asset_list()
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.Add(Row("WELL"));
        var scope = await f.Gate.ResolveAsync(); var calls = 0;
        var service = Service(f, () => Task.FromResult(++calls == 1 ? scope : scope with { Fingerprint = new string('F', 64) }));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.GetUserAccessibleAssetsAsync("actor", "WELL"))).Kind);
    }

    [Theory]
    [InlineData("revoked")] [InlineData("downgraded")] [InlineData("binding")]
    public async Task Permission_resolution_cannot_retain_an_earlier_asset_snapshot(string change)
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset", "DELETE"); f.Rows.Add(Row("WELL"));
        var authorization = new Mock<IApplicationAuthorizationReader>(MockBehavior.Strict);
        authorization.Setup(value => value.HasPermissionAsync("actor", "approve")).Returns(async () =>
        {
            var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync();
            if (change == "revoked") grant.IsActive = false;
            if (change == "downgraded") grant.AccessLevel = "READ";
            if (change == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            await f.Db.SaveChangesAsync(); return true;
        });
        var service = Service(f, authorization: authorization.Object);
        if (change == "binding") await Assert.ThrowsAsync<RefusalException>(() => service.CheckAssetAccessAsync("actor", "asset", "WELL", "approve"));
        else
        {
            var result = await service.CheckAssetAccessAsync("actor", "asset", "WELL", "approve");
            Assert.Equal(change == "downgraded", result.HasAccess); Assert.Equal(change == "downgraded" ? "READ" : null, result.AccessLevel);
        }
        authorization.Verify(value => value.HasPermissionAsync("actor", "approve"), Times.Once);
    }
}
