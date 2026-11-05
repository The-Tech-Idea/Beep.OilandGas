using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetHierarchyTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public async Task DirectEquipmentAppearsWithoutAnInferredFieldOrFacilityParent(bool root)
    {
        using var f=await Fixture.Create();await f.Grant("EQUIPMENT","pump");f.Rows.Add(new EQUIPMENT{EQUIPMENT_ID="pump",EQUIPMENT_NAME="Recorded pump",CURRENT_OWNER_BA_ID="company",ACTIVE_IND="Y"});
        var result=await Service(f).GetAssetHierarchyForUserAsync("actor",rootAssetId:root?"pump":null,rootAssetType:root?"EQUIPMENT":null);var equipment=root?result!:Assert.Single(result!.Children);
        Assert.Equal("EQUIPMENT",equipment.AssetType);Assert.Equal("pump",equipment.AssetId);Assert.Null(equipment.ParentAssetId);Assert.Null(equipment.ParentAssetType);Assert.Empty(equipment.Children);Assert.Empty(equipment.WorkOrderComponents);Assert.Empty(f.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task A_direct_order_is_presented_once_with_all_recorded_fields_and_no_inferred_parent(bool root)
    {
        using var f=await Fixture.Create();await f.Grant("WORK_ORDER","order");f.AddFieldEntitlement("east");f.AddFieldEntitlement("west");
        f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID="order",WORK_ORDER_NUMBER="Recorded order",ACTIVE_IND="Y"});
        f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID="order",COMPONENT_ID="a",FIELD_ID="east",ACTIVE_IND="Y"});
        f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID="order",COMPONENT_ID="b",FIELD_ID="west",ACTIVE_IND="Y"});
        var hierarchy=await Service(f).GetAssetHierarchyForUserAsync("actor",rootAssetId:root?"order":null,rootAssetType:root?"WORK_ORDER":null);
        var order=root?hierarchy!:Assert.Single(hierarchy!.Children);
        Assert.Equal("WORK_ORDER",order.AssetType);Assert.Equal("order",order.AssetId);Assert.Null(order.ParentAssetId);Assert.Null(order.ParentAssetType);
        Assert.Equal(new[]{"east","west"},WorkOrderParentAccess.Fields(order.AssetId,order.WorkOrderComponents));Assert.Empty(order.Children);
    }

    [Fact]
    public async Task Inherited_field_grants_do_not_add_an_order_to_the_hierarchy()
    {
        using var f=await Fixture.Create();await f.Grant("FIELD","east",inherit:true);
        f.Rows.Add(new FIELD{FIELD_ID="east",ACTIVE_IND="Y"});f.Rows.Add(new WORK_ORDER{WORK_ORDER_ID="order",ACTIVE_IND="Y"});
        f.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID="order",COMPONENT_ID="a",FIELD_ID="east",ACTIVE_IND="Y"});
        var hierarchy=await Service(f).GetAssetHierarchyForUserAsync("actor");
        Assert.Equal("FIELD",Assert.Single(hierarchy!.Children).AssetType);Assert.Empty(hierarchy.Children[0].Children);
    }
    private static AssetHierarchyService Service(Fixture f, Func<Task<AssetDatabaseScope>>? resolve = null)
    {
        _ = CompanyAssetFacadeTests.Service(f); // Configure the recording Beep SDK's cached target to match the owned module binding.
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        return new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
            resolve ?? (() => f.Gate.ResolveAsync()), subject => new CompanyPPDMRowAccessPolicy(f.Gate, store, f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), subject),
            subject => f.Gate.ResolveHierarchyCompanyAsync(subject), new RepositoryHierarchyConfigurationStore(f.Db, f.Gate), order => CompanyWorkOrderFieldReview.ComponentsAsync(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility, order));
    }

    private static object Row(string table, string id = "asset", string company = "company", string field = "A", string active = "Y") => table switch
    {
        "FIELD" => new FIELD { FIELD_ID = id, FIELD_NAME = "Stored field", ACTIVE_IND = active },
        "WELL" => new WELL { UWI = id, WELL_NAME = "Stored well", OPERATOR = company, ASSIGNED_FIELD = field, ACTIVE_IND = active },
        "POOL" => new POOL { POOL_ID = id, POOL_NAME = "Stored pool", BUSINESS_ASSOCIATE_ID = company, FIELD_ID = field, ACTIVE_IND = active },
        _ => new FACILITY { FACILITY_ID = id, FACILITY_TYPE = "gas", FACILITY_LONG_NAME = "Stored facility", CURRENT_OPERATOR = company, PRIMARY_FIELD_ID = field, ACTIVE_IND = active }
    };

    [Fact]
    public async Task Actual_policy_builds_only_active_company_children_from_three_stored_field_relationships()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(Row("FIELD", "A"));
        foreach (var table in new[] { "WELL", "POOL", "FACILITY" })
            f.Rows.AddRange([Row(table), Row(table, "foreign", "other"), Row(table, "inactive", active: "N"), Row(table, "other-field", field: "B")]);
        var service = Service(f); var hierarchy = await service.GetAssetHierarchyAsync("company");
        Assert.Equal("ORGANIZATION", hierarchy!.AssetType); Assert.False(hierarchy.UserHasAccess); Assert.Null(hierarchy.AssetName);
        var field = Assert.Single(hierarchy.Children); Assert.Equal("A", field.AssetId); Assert.Equal("Stored field", field.AssetName);
        Assert.Equal(new[] { "FACILITY", "POOL", "WELL" }, field.Children.Select(node => node.AssetType).Order());
        Assert.All(field.Children, node => { Assert.Equal("asset", node.AssetId); Assert.Equal("A", node.ParentAssetId); Assert.True(node.UserHasAccess); Assert.Empty(node.Children); });
        Assert.Equal("gas", field.Children.Single(node => node.AssetType == "FACILITY").FacilityType);
        Assert.Equal(4, f.Queries.Count); Assert.All(f.Connections, name => Assert.Equal("selected-tenant", name)); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Field_children_use_relationship_filters_and_never_read_unscoped_rows(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        await f.Grant("FIELD", "B", inherit: true); f.Rows.AddRange([Row("FIELD", "A"), Row(table), Row(table, "other", field: "B")]);
        var children = await Service(f).GetAssetChildrenAsync("A", "FIELD", "company");
        var child = Assert.Single(children); Assert.Equal(table, child.AssetType); Assert.Equal("asset", child.AssetId);
        var column = table switch { "WELL" => "ASSIGNED_FIELD", "POOL" => "FIELD_ID", _ => "PRIMARY_FIELD_ID" };
        Assert.Contains(f.Queries, query => query.Count(filter => filter.FieldName == column && filter.FilterValue == "A") == 2);
        Assert.All(f.Queries, query => Assert.Contains(query, filter => filter.FieldName == "ACTIVE_IND" && filter.FilterValue == "Y"));
    }

    [Theory]
    [InlineData("FIELD", "missing")] [InlineData("FIELD", "inactive")]
    [InlineData("WELL", "missing")] [InlineData("WELL", "inactive")] [InlineData("WELL", "foreign")]
    [InlineData("POOL", "missing")] [InlineData("POOL", "inactive")] [InlineData("POOL", "foreign")]
    [InlineData("FACILITY", "missing")] [InlineData("FACILITY", "inactive")] [InlineData("FACILITY", "foreign")]
    public async Task Roots_and_paths_never_fabricate_an_asset_from_a_registry_record_or_caller_id(string table, string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        if (table != "FACILITY") await f.Grant(table, "asset");
        if (reason != "missing") f.Rows.Add(Row(table, company: reason == "foreign" ? "other" : "company", active: reason == "inactive" ? "N" : "Y"));
        var service = Service(f); var facility = table == "FACILITY" ? "gas" : null;
        Assert.Null(await service.GetAssetHierarchyAsync("company", "asset", table, facility));
        Assert.Empty(await service.GetAssetPathAsync("asset", table, facilityType: facility));
        Assert.Empty(await service.GetAssetChildrenAsync("asset", table, facilityType: facility)); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Paths_use_actual_field_foreign_keys_and_preserve_facility_identity(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.AddRange([Row("FIELD", "A"), Row(table)]);
        var path = await Service(f).GetAssetPathAsync("asset", table, facilityType: table == "FACILITY" ? "gas" : null);
        Assert.Equal(new[] { "FIELD", table }, path.Select(node => node.AssetType)); Assert.Equal("A", path[0].AssetId);
        Assert.Equal(table == "FACILITY" ? "gas" : null, path[1].FacilityType); Assert.Equal("A", path[1].ParentAssetId);
        Assert.True(await Service(f).ValidateAccessAsync("actor", path));
    }

    [Fact]
    public async Task Facility_roots_and_paths_distinguish_equal_ids_with_different_types()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.Add(Row("FACILITY"));
        f.Rows.Add(new FACILITY { FACILITY_ID = "asset", FACILITY_TYPE = "oil", CURRENT_OPERATOR = "company", PRIMARY_FIELD_ID = "A", FACILITY_SHORT_NAME = "Oil facility", ACTIVE_IND = "Y" });
        var service = Service(f); var gas = await service.GetAssetHierarchyForUserAsync("actor", rootAssetId: "asset", rootAssetType: "FACILITY", rootFacilityType: "gas");
        var oil = Assert.Single(await service.GetAssetPathAsync("asset", "FACILITY", facilityType: "oil"));
        Assert.Equal("gas", gas!.FacilityType); Assert.Equal("Stored facility", gas.AssetName);
        Assert.Equal("oil", oil.FacilityType); Assert.Equal("Oil facility", oil.AssetName);
        Assert.Null(await service.GetAssetHierarchyAsync("company", "asset", "FACILITY", "water"));
        Assert.Contains(f.Queries, query => query.Any(filter => filter.FieldName == "FACILITY_TYPE" && filter.FilterValue == "gas"));
        Assert.True(await service.ValidateAccessAsync("actor", [oil]));
    }

    [Theory]
    [InlineData("root-id")] [InlineData("root-type")] [InlineData("unknown")] [InlineData("facility-missing")]
    [InlineData("facility-blank")] [InlineData("well-qualified")]
    public async Task Incomplete_or_unreviewed_root_keys_are_refused_before_provider_reads(string reason)
    {
        using var f = await Fixture.Create(); var service = Service(f);
        await Assert.ThrowsAsync<RefusalException>(() => service.GetAssetHierarchyAsync("company",
            reason == "root-type" ? null : "asset", reason == "root-id" ? null : reason == "unknown" ? "BLOCK" : reason == "well-qualified" ? "WELL" : "FACILITY",
            reason == "well-qualified" ? "gas" : reason == "facility-blank" ? " " : null));
        Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task A_direct_child_grant_never_promotes_an_ungranted_parent()
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.AddRange([Row("FIELD", "A"), Row("WELL")]);
        var service = Service(f); var root = await service.GetAssetHierarchyForUserAsync("actor");
        var leaf = Assert.Single(root!.Children); Assert.Equal("WELL", leaf.AssetType); Assert.Equal("FIELD", leaf.ParentAssetType);
        Assert.Equal("WELL", Assert.Single(await service.GetAssetPathAsync("asset", "WELL")).AssetType);
        Assert.False(await service.ValidateAccessAsync("actor", [new() { AssetId = "A", AssetType = "FIELD" }, leaf]));
        Assert.True(await service.ValidateAccessAsync("actor", [leaf]));
    }

    [Theory]
    [InlineData("unrelated")] [InlineData("reverse")] [InlineData("duplicate")] [InlineData("parent-assertion")]
    [InlineData("organization")] [InlineData("invented-chain")]
    public async Task Individually_visible_nodes_cannot_validate_an_invented_path(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); await f.Grant("FIELD", "B", inherit: true);
        f.Rows.AddRange([Row("FIELD", "A"), Row("FIELD", "B"), Row("WELL"), Row("POOL")]);
        var field = new AssetHierarchyNode { AssetId = "A", AssetType = "FIELD" }; var well = new AssetHierarchyNode { AssetId = "asset", AssetType = "WELL" };
        List<AssetHierarchyNode> path = reason switch
        {
            "unrelated" => [new() { AssetId = "B", AssetType = "FIELD" }, well], "reverse" => [well, field], "duplicate" => [field, field],
            "parent-assertion" => [field, new() { AssetId = "asset", AssetType = "WELL", ParentAssetId = "B" }],
            "organization" => [new() { AssetId = "other", AssetType = "ORGANIZATION" }, field, well],
            _ => [field, new() { AssetId = "asset", AssetType = "POOL" }, well]
        };
        Assert.False(await Service(f).ValidateAccessAsync("actor", path)); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("empty")] [InlineData("null-path")] [InlineData("null-node")] [InlineData("blank-user")]
    [InlineData("blank-id")] [InlineData("blank-type")] [InlineData("too-long")]
    public async Task Malformed_path_checks_never_read_the_provider(string reason)
    {
        using var f = await Fixture.Create(); var service = Service(f); var node = new AssetHierarchyNode { AssetId = "A", AssetType = "FIELD" };
        var path = reason == "null-path" ? null! : reason == "empty" ? new List<AssetHierarchyNode>() : new List<AssetHierarchyNode> { node };
        if (reason == "null-node") path.Add(null!); if (reason == "blank-id") node.AssetId = " "; if (reason == "blank-type") node.AssetType = " ";
        if (reason == "too-long") path.AddRange([node, node, node]);
        Assert.False(await service.ValidateAccessAsync(reason == "blank-user" ? " " : "actor", path)); Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("hierarchy")] [InlineData("user")] [InlineData("children")] [InlineData("path")] [InlineData("config")] [InlineData("update")]
    public async Task Organization_assertions_cannot_select_a_different_company(string operation)
    {
        using var f = await Fixture.Create(); var service = Service(f);
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(service, operation, "other")); Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    private static Task Invoke(IAssetHierarchyService service, string operation, string? organization = "company") => operation switch
    {
        "hierarchy" => service.GetAssetHierarchyAsync(organization!), "user" => service.GetAssetHierarchyForUserAsync("actor", organization),
        "children" => service.GetAssetChildrenAsync("A", "FIELD", organization), "path" => service.GetAssetPathAsync("asset", "WELL", organization),
        "config" => service.GetHierarchyConfigAsync(organization!), "update" => service.UpdateHierarchyConfigAsync(organization!, []),
        _ => service.ValidateAccessAsync("actor", [new() { AssetId = "A", AssetType = "FIELD" }])
    };

    [Fact]
    public async Task Company_admin_inspects_subject_policy_without_substituting_actor_grants()
    {
        using var f = await Fixture.Create(); f.Db.Users.Add(new() { Id = "member", UserName = "member", IsActive = true }); await f.Db.SaveChangesAsync(); await f.Select("company");
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>());
        Assert.True(await store.GrantAsync("member", (await f.Gate.ResolveAsync()).Fingerprint, "WELL", "asset", "READ", false)); f.Rows.Add(Row("WELL"));
        var service = Service(f); Assert.Empty((await service.GetAssetHierarchyForUserAsync("actor"))!.Children);
        Assert.Equal("asset", Assert.Single((await service.GetAssetHierarchyForUserAsync("member"))!.Children).AssetId);
        Assert.Single(f.Queries);
    }

    [Theory]
    [InlineData("hierarchy")] [InlineData("children")] [InlineData("path")] [InlineData("config")] [InlineData("update")]
    public async Task Ordinary_company_members_cannot_use_admin_hierarchy_operations(string operation)
    {
        using var f = await Fixture.Create(); f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync()); await f.Db.SaveChangesAsync(); await f.Select("company");
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Service(f), operation)); Assert.Empty(f.Queries);
        Assert.Empty((await Service(f).GetAssetHierarchyForUserAsync("actor"))!.Children);
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAssetHierarchyForUserAsync("other"));
    }

    [Theory]
    [InlineData("hierarchy")] [InlineData("user")] [InlineData("children")] [InlineData("path")] [InlineData("validate")]
    public async Task Current_grant_revocation_during_provider_read_cannot_return_a_hierarchy_or_positive_path(string operation)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.AddRange([Row("FIELD", "A"), Row("WELL")]);
        f.AfterRead = async () => { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync(); };
        await Assert.ThrowsAsync<RefusalException>(() => Invoke(Service(f), operation)); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("binding")] [InlineData("membership")] [InlineData("administrator")] [InlineData("cached-target")]
    public async Task Authority_or_target_changes_during_reads_refuse_stale_hierarchies(string change)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.Add(Row("FIELD", "A"));
        var service = Service(f);
        f.AfterRead = async () =>
        {
            if (change == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            if (change == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            if (change == "administrator") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
            if (change == "cached-target") f.Editor.Object.ConfigEditor.DataConnections.Clear();
            await f.Db.SaveChangesAsync();
        };
        if (change == "cached-target") await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAssetHierarchyAsync("company", "A", "FIELD"));
        else await Assert.ThrowsAsync<RefusalException>(() => service.GetAssetHierarchyAsync("company", "A", "FIELD"));
    }

    [Theory]
    [InlineData("foreign")] [InlineData("inactive")] [InlineData("wrong-id")]
    public async Task Provider_filter_bypasses_are_refused_by_the_actual_row_policy_or_required_equalities(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(Row("WELL", reason == "wrong-id" ? "other" : "asset", company: reason == "foreign" ? "other" : "company", active: reason == "inactive" ? "N" : "Y"));
        f.IgnoreFilters = true;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAssetPathAsync("asset", "WELL")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("failed")] [InlineData("cancelled")] [InlineData("exception")]
    public async Task Provider_failures_propagate_instead_of_becoming_missing_roots_or_denied_paths(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); f.Rows.Add(Row("FIELD", "A"));
        if (reason == "failed") f.Source.SetupGet(value => value.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
            else f.AfterRead = () => throw (reason == "cancelled" ? new OperationCanceledException() : new InvalidOperationException("provider unavailable"));
        if (reason == "cancelled") await Assert.ThrowsAsync<OperationCanceledException>(() => Service(f).ValidateAccessAsync("actor", [new() { AssetId = "A", AssetType = "FIELD" }]));
        else await Assert.ThrowsAsync<InvalidOperationException>(() => Service(f).GetAssetHierarchyAsync("company", "A", "FIELD"));
    }

    [Theory]
    [InlineData("organization")] [InlineData("user")] [InlineData("children")] [InlineData("path")]
    public async Task Controller_forwards_complete_facility_keys(string operation)
    {
        using var f = await Fixture.Create();
        var inner = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        var leaf = new AssetHierarchyNode { AssetId = "facility", AssetType = "FACILITY", FacilityType = "gas" };
        inner.Setup(value => value.GetAssetHierarchyAsync("company", "facility", "FACILITY", "gas")).ReturnsAsync(leaf);
        inner.Setup(value => value.GetAssetHierarchyForUserAsync("actor", "company", "facility", "FACILITY", "gas")).ReturnsAsync(leaf);
        inner.Setup(value => value.GetAssetChildrenAsync("facility", "FACILITY", "company", "gas")).ReturnsAsync([]);
        inner.Setup(value => value.GetAssetPathAsync("facility", "FACILITY", "company", "gas")).ReturnsAsync([leaf]);
        var controller = new AssetHierarchyController(inner.Object, f.Gate) { ControllerContext = new() { HttpContext = new DefaultHttpContext
        { User = new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        switch (operation)
        {
            case "organization": Assert.IsType<OkObjectResult>((await controller.GetAssetHierarchy("company", "facility", "FACILITY", "gas")).Result); break;
            case "user": Assert.IsType<OkObjectResult>((await controller.GetAssetHierarchyForUser("actor", "company", "facility", "FACILITY", "gas")).Result); break;
            case "children": Assert.IsType<OkObjectResult>((await controller.GetAssetChildren("facility", "FACILITY", "company", "gas")).Result); break;
            default: Assert.IsType<OkObjectResult>((await controller.GetAssetPath("facility", "FACILITY", "company", "gas")).Result); break;
        }
    }

    [Theory]
    [InlineData("hierarchy")] [InlineData("children")] [InlineData("path")] [InlineData("config")] [InlineData("update")]
    [InlineData("user")] [InlineData("validate")]
    public async Task A_forged_administrator_claim_cannot_cross_the_controllers_current_company_gate(string operation)
    {
        using var f = await Fixture.Create(); f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
        await f.Db.SaveChangesAsync(); await f.Select("company");
        var service = new Mock<IAssetHierarchyService>(MockBehavior.Strict);
        var controller = new AssetHierarchyController(service.Object, f.Gate) { ControllerContext = new() { HttpContext = new DefaultHttpContext
        { User = new(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test")) } } };
        Task InvokeController() => operation switch
        {
            "hierarchy" => controller.GetAssetHierarchy("company"), "children" => controller.GetAssetChildren("A", "FIELD"),
            "path" => controller.GetAssetPath("asset", "WELL"), "config" => controller.GetHierarchyConfig("company"),
            "update" => controller.UpdateHierarchyConfig("company", []), "user" => controller.GetAssetHierarchyForUser("other"),
            _ => controller.ValidateAccess(new() { UserId = "other", AssetPath = [] })
        };
        var refusal = await Assert.ThrowsAsync<RefusalException>(InvokeController); Assert.Equal(RefusalKind.Forbidden, refusal.Kind);
        service.VerifyNoOtherCalls(); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Final_validation_rechecks_an_earlier_table_after_a_later_provider_read_revokes_its_grant()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("WELL", "asset");
        f.Rows.AddRange([Row("FIELD", "A"), Row("WELL")]); var calls = 0;
        f.AfterRead = async () =>
        {
            if (++calls != 2) return;
            (await f.Db.Set<AppUserAssetAccess>().SingleAsync(grant => grant.AssetType == "FIELD")).IsActive = false;
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).GetAssetHierarchyForUserAsync("actor"));
        Assert.Equal(2, f.Queries.Count);
    }

    [Fact]
    public async Task Final_validation_cannot_return_rows_after_the_cached_target_changes_after_the_last_read()
    {
        using var f = await Fixture.Create(); await f.Grant("WELL", "asset"); f.Rows.Add(Row("WELL")); var calls = 0;
        var service = Service(f, async () =>
        {
            if (++calls == 3) f.Editor.Object.ConfigEditor.DataConnections.Clear();
            return await f.Gate.ResolveAsync();
        });
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetAssetHierarchyForUserAsync("actor", rootAssetId: "asset", rootAssetType: "WELL"));
        Assert.Single(f.Queries);
    }
}
