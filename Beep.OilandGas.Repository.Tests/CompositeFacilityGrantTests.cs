using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompositeFacilityGrantTests
{
    private static FACILITY Plant(string type = "gas", string id = "plant", string company = "company", string active = "Y") =>
        new() { FACILITY_ID = id, FACILITY_TYPE = type, CURRENT_OPERATOR = company, PRIMARY_FIELD_ID = "A", ACTIVE_IND = active };
    private static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); _ = CompanyAssetFacadeTests.Service(f);
        f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(ConnectionState.Open);
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
        f.Rows.AddRange([Plant(), Plant("oil"), Plant(id: "other")]); return f;
    }
    private static PPDMAssetGrantTargetVerifier Verifier(Fixture f) => new(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility);
    private static RepositoryAssetAccessStore Store(Fixture f) => new(f.Db, f.Gate, Verifier(f));
    private static async Task Grant(Fixture f, string type = "gas", string level = "READ") =>
        Assert.True(await Store(f).GrantAsync("actor", (await f.Gate.ResolveAsync()).Fingerprint, "FACILITY", "plant", level, false, facilityType: type));

    [Fact]
    public async Task Complete_keys_create_distinct_grants_and_revocation_cannot_touch_another_facility_type()
    {
        using var f = await Ready(); await Grant(f); await Grant(f, "oil", "DELETE");
        var before = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().ToListAsync(); Assert.Equal(2, before.Count); Assert.Equal(2, before.Select(row => row.Id).Distinct().Count());
        Assert.All(before, row => { Assert.Equal("plant", row.AssetId); Assert.Equal("actor", row.CreatedBy); Assert.Equal("company", row.OrganizationId); });
        var scope = await f.Gate.ResolveAsync(); Assert.True(await Store(f).RevokeAsync("actor", scope.Fingerprint, "FACILITY", "plant", "gas"));
        Assert.False((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(row => row.FacilityType == "gas")).IsActive);
        Assert.True((await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(row => row.FacilityType == "oil")).IsActive);
        Assert.False(await Store(f).RevokeAsync("actor", scope.Fingerprint, "FACILITY", "plant", "GAS"));
        await Grant(f, level: "WRITE"); var after = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().ToListAsync(); Assert.Equal(2, after.Count);
        Assert.Equal(before.Single(row => row.FacilityType == "gas").CreatedUtc, after.Single(row => row.FacilityType == "gas").CreatedUtc); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData(null, false)] [InlineData(null, true)] [InlineData("", false)] [InlineData("", true)] [InlineData(" gas ", false)] [InlineData(" gas ", true)]
    [InlineData("bad\ntype", false)] [InlineData("bad\ntype", true)] [InlineData("long", false)] [InlineData("long", true)]
    public async Task Incomplete_or_noncanonical_facility_keys_are_refused_before_provider_access(string? qualifier, bool revoke)
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync(); if (qualifier == "long") qualifier = new string('a', 129);
        var failure = await Assert.ThrowsAsync<RefusalException>(() => revoke ? Store(f).RevokeAsync("actor", scope.Fingerprint, "FACILITY", "plant", qualifier) :
            Store(f).GrantAsync("actor", scope.Fingerprint, "FACILITY", "plant", "READ", false, facilityType: qualifier));
        Assert.Equal(RefusalKind.Invalid, failure.Kind); Assert.Empty(f.Queries); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Theory]
    [InlineData("FIELD", false)] [InlineData("FIELD", true)] [InlineData("WELL", false)] [InlineData("WELL", true)] [InlineData("POOL", false)] [InlineData("POOL", true)]
    public async Task Facility_qualifiers_cannot_be_attached_to_other_asset_grants(string type, bool revoke)
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        var failure = await Assert.ThrowsAsync<RefusalException>(() => revoke ? Store(f).RevokeAsync("actor", scope.Fingerprint, type, "plant", "gas") :
            Store(f).GrantAsync("actor", scope.Fingerprint, type, "plant", "READ", false, facilityType: "gas"));
        Assert.Equal(RefusalKind.Invalid, failure.Kind); Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("missing")] [InlineData("inactive")] [InlineData("foreign")] [InlineData("wrong-key")] [InlineData("ambiguous")]
    public async Task Admission_checks_both_facility_keys_active_state_and_selected_company(string reason)
    {
        using var f = await Ready(); f.Rows.RemoveAll(row => row is FACILITY);
        if (reason != "missing") f.Rows.Add(Plant(reason == "wrong-key" ? "oil" : "gas", company: reason == "foreign" ? "other" : "company", active: reason == "inactive" ? "N" : "Y"));
        if (reason == "ambiguous") f.Rows.Add(Plant()); if (reason == "wrong-key") f.IgnoreFilters = true;
        await Assert.ThrowsAsync<RefusalException>(() => Grant(f)); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
        Assert.Contains(f.Queries, query=>query.Any(value=>value.FieldName=="FACILITY_ID" && value.FilterValue=="plant") && query.Any(value=>value.FieldName=="FACILITY_TYPE" && value.FilterValue=="gas") && query.Any(value=>value.FieldName=="ACTIVE_IND" && value.FilterValue=="Y"));
    }

    [Theory][InlineData("allowed")][InlineData("no-entitlement")][InlineData("low-entitlement")][InlineData("missing-field")][InlineData("ambiguous-field")]
    public async Task PartnerFacilityGrantRequiresCurrentCompanyFieldEntitlement(string mode)
    {
        using var f=await Ready();var facility=f.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_ID=="plant" && row.FACILITY_TYPE=="gas");facility.CURRENT_OPERATOR="partner";
        if(mode!="missing-field")f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});if(mode=="ambiguous-field")f.Rows.Add(new FIELD{FIELD_ID="A",ACTIVE_IND="Y"});
        if(mode!="no-entitlement")f.AddFieldEntitlement("A",mode=="low-entitlement"?"READ":"WRITE");
        if(mode=="allowed")
        {
            await f.Grant("FIELD","A","WRITE",inherit:true);await Grant(f,level:"WRITE");Assert.Equal("WRITE",await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facility,false));
            var facade=CompanyAssetFacadeTests.Service(f,admission:Verifier(f));Assert.True((await facade.CheckAssetAccessAsync("actor","plant","FACILITY","WRITE","gas")).HasAccess);
            var direct=Assert.Single(await facade.GetUserAccessibleAssetsAsync("actor","FACILITY",includeInherited:false));Assert.Equal("WRITE",direct.AccessLevel);
            var field=await f.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FIELD");field.AccessLevel="READ";await f.Db.SaveChangesAsync();Assert.Equal("READ",await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facility,false));
            field.AccessLevel="WRITE";field.IsActive=false;await f.Db.SaveChangesAsync();Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facility,false));
            field.IsActive=true;await f.Db.SaveChangesAsync();var entitlement=f.Rows.OfType<ENT_COMPONENT>().Single(row=>row.FIELD_ID=="A" && row.BUSINESS_ASSOCIATE_ID=="company");entitlement.ACTIVE_IND="N";Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",facility,false));
        }
        else{await Assert.ThrowsAnyAsync<Exception>(()=>Grant(f,level:"WRITE"));Assert.Empty(await f.Db.Set<AppUserAssetAccess>().Where(row=>row.AssetType=="FACILITY").ToArrayAsync());}
    }

    [Theory]
    [InlineData("missing-type")] [InlineData("wrong-type")]
    public async Task Facility_metadata_must_prove_the_exact_composite_key(string reason)
    {
        using var f = await Ready(); (await f.Metadata.Object.GetTableMetadataAsync("FACILITY")).PrimaryKeyColumn = reason == "missing-type" ? "FACILITY_ID" : "FACILITY_ID,CURRENT_OPERATOR";
        var failure = await Assert.ThrowsAsync<RefusalException>(() => Grant(f)); Assert.Equal(RefusalKind.Conflict, failure.Kind); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task A_stale_cached_target_cannot_admit_a_facility_grant()
    {
        using var f = await Ready(); var connection = new Mock<IDataConnection>();
        connection.SetupGet(value => value.ConnectionProp).Returns(new ConnectionProperties { ConnectionName = "selected-tenant", DatabaseType = DataSourceType.SqlServer, Host = "other-server", Database = "wrong-database" });
        f.Source.SetupGet(value => value.Dataconnection).Returns(connection.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => Grant(f)); Assert.Empty(f.Queries); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Fact]
    public async Task SDK_conjunctions_and_facade_checks_keep_facility_types_separate()
    {
        using var f = await Ready(); await Grant(f); f.Queries.Clear(); var service = CompanyAssetFacadeTests.Service(f);
        var result = Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY", includeInherited: false)); Assert.Equal("gas", result.FacilityType);
        var query = Assert.Single(f.Queries); Assert.Equal(4, query.Count); Assert.Contains(query, value => value.FieldName == "FACILITY_ID" && value.FilterValue == "plant");
        Assert.Contains(query, value => value.FieldName == "FACILITY_TYPE" && value.FilterValue == "gas"); Assert.Contains(query, value => value.FieldName == "CURRENT_OPERATOR" && value.FilterValue == "company");
        Assert.True((await service.CheckAssetAccessAsync("actor", "plant", "FACILITY", facilityType: "gas")).HasAccess);
        Assert.False((await service.CheckAssetAccessAsync("actor", "plant", "FACILITY", facilityType: "oil")).HasAccess);
        Assert.False((await service.CheckAssetAccessAsync("actor", "other", "FACILITY", facilityType: "gas")).HasAccess);
        var groups = await f.Policy.GetReadConjunctionsAsync(typeof(FACILITY), "FACILITY"); Assert.Equal(3, Assert.Single(groups).Count);
    }

    [Fact]
    public async Task Direct_and_field_inherited_conjunctions_remain_distinct_and_direct_only_reads_keep_direct_levels()
    {
        using var f = await Ready(); await Grant(f); await f.Grant("FIELD", "A", "WRITE", true); var service = CompanyAssetFacadeTests.Service(f);
        var direct = Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY", includeInherited: false)); Assert.Equal("gas", direct.FacilityType); Assert.Equal("READ", direct.AccessLevel);
        var inherited = await service.GetUserAccessibleAssetsAsync("actor", "FACILITY"); Assert.Equal(3, inherited.Count); Assert.All(inherited, row => Assert.Equal("WRITE", row.AccessLevel));
        var groups = await f.Policy.GetReadConjunctionsAsync(typeof(FACILITY), "FACILITY"); Assert.Equal(new[] { 1, 3 }, groups.Select(group => group.Count).Order());
    }

    [Theory]
    [InlineData("qualifier")] [InlineData("id")]
    public async Task Provider_filter_bypasses_cannot_widen_either_facility_key_part(string part)
    {
        using var f = await Ready(); await Grant(f); f.Rows.RemoveAll(row => row is FACILITY); f.Rows.Add(part == "qualifier" ? Plant("oil") : Plant(id: "other")); f.IgnoreFilters = true;
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(FACILITY)).GetAsync([]));
    }

    [Theory]
    [InlineData("READ")] [InlineData("WRITE")] [InlineData("DELETE")]
    public async Task Mutation_level_and_complete_key_are_enforced_for_facility_rows(string level)
    {
        using var f = await Ready(); await Grant(f, level: level); var entity = Plant();
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(FACILITY), "FACILITY", entity, entity, PPDMRowMutation.Update, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(FACILITY), "FACILITY", entity, entity, PPDMRowMutation.Delete, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(FACILITY), "FACILITY", Plant("oil"), Plant("oil"), PPDMRowMutation.Update, "actor"));
    }

    [Theory]
    [InlineData(PPDMRowMutation.Insert)] [InlineData(PPDMRowMutation.Update)] [InlineData(PPDMRowMutation.Delete)]
    public async Task Facility_equipment_mutation_requires_a_reviewed_owner_even_with_facility_write(PPDMRowMutation operation)
    {
        using var f = await Ready(); await Grant(f, level: "DELETE");
        var row = new FACILITY_EQUIPMENT { FACILITY_ID = "plant", FACILITY_TYPE = "gas", EQUIPMENT_ID = "pump", INSTALL_OBS_NO = 1, ACTIVE_IND = "Y" };
        var error = await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(FACILITY_EQUIPMENT), "FACILITY_EQUIPMENT", row, operation == PPDMRowMutation.Insert ? null : row, operation, "actor"));
        Assert.Equal(RefusalKind.Forbidden, error.Kind); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Generic_mutations_cannot_rekey_a_facility_even_when_both_types_are_authorized()
    {
        using var f = await Ready(); await Grant(f, level: "WRITE"); await Grant(f, "oil", "WRITE");
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(FACILITY), "FACILITY", Plant("oil"), Plant(), PPDMRowMutation.Update, "actor"));
    }

    [Fact]
    public async Task Revocation_during_a_provider_read_refuses_previously_read_facility_rows()
    {
        using var f = await Ready(); await Grant(f); var scope = await f.Gate.ResolveAsync();
        f.AfterRead = async () => { f.AfterRead = null; await Store(f).RevokeAsync("actor", scope.Fingerprint, "FACILITY", "plant", "gas"); };
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(FACILITY)).GetAsync([]));
    }

    [Fact]
    public async Task Historical_unqualified_facility_grants_remain_recorded_but_cannot_grant_or_widen_access()
    {
        using var f = await Ready(); var scope = await f.Gate.ResolveAsync();
        f.Db.Add(new AppUserAssetAccess { Id = "historical-facility", UserId = "actor", DatabaseScope = scope.Fingerprint, OrganizationId = "company", AssetType = "FACILITY",
            AssetId = "plant", FacilityType = null, AccessLevel = "DELETE", Inherit = true, IsActive = true, CreatedBy = "actor", ChangedBy = "actor" }); await f.Db.SaveChangesAsync();
        Assert.Empty(await f.Policy.GetReadConjunctionsAsync(typeof(FACILITY), "FACILITY")); await Grant(f);
        var service = CompanyAssetFacadeTests.Service(f); var only = Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY")); Assert.Equal("gas", only.FacilityType); Assert.Equal("READ", only.AccessLevel);
        await Store(f).RevokeAsync("actor", scope.Fingerprint, "FACILITY", "plant", "gas"); Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", "FACILITY"));
        var historical = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(row => row.Id == "historical-facility"); Assert.True(historical.IsActive); Assert.Null(historical.FacilityType);
    }

    [Fact]
    public async Task Actual_editor_qualifies_grants_and_revokes_only_one_of_same_id_facilities()
    {
        using var f = await Ready(); var service = CompanyAssetFacadeTests.Service(f, admission: Verifier(f)); using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(f.Gate.Company.Scope, ["Administrator"], [])));
        using var state = new AssetGrantAdministrationState(company, async (_, _) => await service.GetUserAccessibleAssetsAsync("actor", organizationId: "company", includeInherited: false),
            (_, request, _) => service.GrantAssetAccessAsync(request.UserId, request.AssetId, request.AssetType, request.AccessLevel, request.Inherit, request.OrganizationId, request.FacilityType),
            (_, request, _) => service.RevokeAssetAccessAsync(request.UserId, request.AssetId, request.AssetType, request.FacilityType), (_, _, message) => message,
            Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());
        state.SelectUser("actor"); await state.LoadAsync(); state.SelectAssetType("FACILITY"); state.AssetId = "plant"; state.FacilityType = "gas"; await state.GrantAsync(); Assert.NotNull(state.Confirmation);
        state.FacilityType = "oil"; state.AccessLevel = "DELETE"; await state.GrantAsync(); Assert.NotNull(state.Confirmation); Assert.Equal(2, state.Grants.Count);
        var gas = state.Grants.Single(row => row.FacilityType == "gas"); gas.FacilityType = "oil"; Assert.False(state.CanRevoke(gas)); gas.FacilityType = "gas";
        await state.RevokeAsync(gas); Assert.NotNull(state.Confirmation); Assert.Equal("oil", Assert.Single(state.Grants).FacilityType); Assert.Null(state.Error);
    }

    [Fact]
    public async Task A_grant_readback_of_another_facility_type_is_not_a_save_confirmation()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], []))); var reads = 0;
        using var state = new AssetGrantAdministrationState(company, (_, _) => Task.FromResult<List<AssetAccess>?>(++reads == 1 ? [] :
            [new() { UserId = "actor", OrganizationId = "company", AssetType = "FACILITY", AssetId = "plant", FacilityType = "oil", AccessLevel = "READ", Inherit = false, Active = true }]),
            (_, request, _) => { Assert.Equal("gas", request.FacilityType); return Task.FromResult(true); }, (_, _, _) => throw new InvalidOperationException(), (_, _, message) => message,
            Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());
        state.SelectUser("actor"); await state.LoadAsync(); state.SelectAssetType("FACILITY"); state.AssetId = "plant"; state.FacilityType = "gas"; await state.GrantAsync();
        Assert.Null(state.Confirmation); Assert.NotNull(state.Error); Assert.False(state.CanEdit); Assert.Empty(state.Grants);
    }

    [Fact]
    public async Task Direct_facility_grants_reach_the_actual_hierarchy_path_and_complete_Web_selector()
    {
        using var f = await Ready(); await Grant(f); var assets = Store(f);
        var hierarchy = new AssetHierarchyService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, () => f.Gate.ResolveAsync(),
            subject => new CompanyPPDMRowAccessPolicy(f.Gate, assets, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject), subject => f.Gate.ResolveHierarchyCompanyAsync(subject), new RepositoryHierarchyConfigurationStore(f.Db, f.Gate), order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
        var path = await hierarchy.GetAssetPathAsync("plant", "FACILITY", "company", "gas"); Assert.Equal("gas", Assert.Single(path).FacilityType);
        Assert.True(await hierarchy.ValidateAccessAsync("actor", path)); Assert.Empty(await hierarchy.GetAssetPathAsync("plant", "FACILITY", "company", "oil"));
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(f.Gate.Company.Scope, ["Administrator"], [])));
        using var state = new AssetHierarchyState(company, (_, _) => hierarchy.GetAssetHierarchyForUserAsync("actor", "company", "plant", "FACILITY", "gas"), (_, _, message) => message);
        await state.LoadAsync(rootAssetId: "plant", rootAssetType: "FACILITY", rootFacilityType: "gas");
        Assert.Equal("gas", state.Hierarchy!.FacilityType); Assert.True(state.CanSelect(state.Hierarchy)); Assert.Null(state.Error);
    }

    [Fact]
    public async Task Canonical_diagnostics_serialize_both_facility_keys_inside_one_conjunction()
    {
        using var f = await Ready(); await Grant(f); f.Http.HttpContext = f.Request;
        var diagnostic = new CompanyRowLevelSecurityService(f.Account, f.Gate, Store(f), f.Policy, f.Metadata.Object, f.Service);
        var result = await diagnostic.ApplyRowFiltersAsync("actor", new("FACILITY"));
        Assert.Equal(3, Assert.Single(result.ReadConjunctions).Count); Assert.Equal(3, result.FilterCount); Assert.False(result.DenyAll);
        Assert.Empty(result.CompanyConstraints);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var group = json.RootElement.GetProperty("readConjunctions")[0]; Assert.Equal(3, group.GetArrayLength());
        Assert.Contains(group.EnumerateArray(), value => value.GetProperty("fieldName").GetString() == "FACILITY_TYPE" && value.GetProperty("filterValue").GetString() == "gas");
        Assert.Contains(group.EnumerateArray(), value => value.GetProperty("fieldName").GetString() == "CURRENT_OPERATOR" && value.GetProperty("filterValue").GetString() == "company");
    }
}
