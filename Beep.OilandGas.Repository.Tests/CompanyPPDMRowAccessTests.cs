using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Tests.Infrastructure;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Utilities;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyPPDMRowAccessTests
{
    [Fact]
    public async Task InheritedFieldAccessCanReadAnExternallyOperatedFacilityWithoutOpeningOtherFieldsOrCompanies()
    {
        using var f=await Fixture.Create();f.Rows.AddRange([new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"},new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"}]);
        await f.Grant("FIELD","A",inherit:true);var result=await f.Service.GetEntitiesAsync("FACILITY",[]);
        Assert.Single(result.Entities);Assert.Equal("gas",result.Entities[0]["FACILITY_TYPE"]);Assert.Equal("partner",result.Entities[0]["CURRENT_OPERATOR"]);
        var foreign=new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="B",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"};Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",foreign));
        var entitlement=f.Rows.OfType<ENT_COMPONENT>().Single(value=>value.FIELD_ID=="A" && value.BUSINESS_ASSOCIATE_ID=="company");entitlement.ACTIVE_IND="N";Assert.Empty((await f.Service.GetEntitiesAsync("FACILITY",[])).Entities);entitlement.ACTIVE_IND="Y";
        var grant=await f.Db.Set<AppUserAssetAccess>().SingleAsync(value=>value.AssetType=="FIELD" && value.AssetId=="A");grant.IsActive=false;await f.Db.SaveChangesAsync();Assert.Empty((await f.Service.GetEntitiesAsync("FACILITY",[])).Entities);
        await f.Select("other-company");Assert.Empty((await f.Service.GetEntitiesAsync("FACILITY",[])).Entities);
    }
    [Fact]
    public async Task FieldGrantWithoutInheritanceDoesNotOpenAnExternallyOperatedFacility()
    {
        using var f=await Fixture.Create();f.Rows.Add(new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"});
        await f.Grant("FIELD","A",inherit:false);Assert.Empty((await f.Service.GetEntitiesAsync("FACILITY",[])).Entities);
    }
    [Fact]
    public async Task DirectFacilityGrantAloneDoesNotOpenAnExternallyOperatedFacility()
    {
        using var f=await Fixture.Create();f.Rows.Add(new FACILITY{FACILITY_ID="shared",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR="partner",ACTIVE_IND="Y"});
        await f.Grant("FACILITY","shared",facilityType:"gas");Assert.Empty((await f.Service.GetEntitiesAsync("FACILITY",[])).Entities);
        Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(typeof(FACILITY),"FACILITY",f.Rows.OfType<FACILITY>().Single(),false));
    }
    [Theory][InlineData("status",PPDMRowMutation.Insert)][InlineData("status",PPDMRowMutation.Update)][InlineData("status",PPDMRowMutation.Delete)][InlineData("intent",PPDMRowMutation.Insert)][InlineData("intent",PPDMRowMutation.Update)][InlineData("intent",PPDMRowMutation.Delete)][InlineData("facility-edit",PPDMRowMutation.Insert)][InlineData("facility-edit",PPDMRowMutation.Update)][InlineData("facility-edit",PPDMRowMutation.Delete)][InlineData("facility",PPDMRowMutation.Insert)][InlineData("facility",PPDMRowMutation.Update)][InlineData("facility",PPDMRowMutation.Delete)][InlineData("facility-link",PPDMRowMutation.Insert)][InlineData("facility-link",PPDMRowMutation.Update)][InlineData("facility-link",PPDMRowMutation.Delete)][InlineData("create-intent",PPDMRowMutation.Insert)][InlineData("create-intent",PPDMRowMutation.Update)][InlineData("create-intent",PPDMRowMutation.Delete)]
    public async Task StandardCompanyPolicyCannotAuthorizeRawWellStatusOrPrivateReturnIntentWrites(string kind,PPDMRowMutation operation)
    {
        using var f=await Fixture.Create();object row=kind switch{"status"=>new WELL_STATUS{UWI="well",SOURCE="source",STATUS_ID="original-condition",STATUS_TYPE="Condition",STATUS="Active"},"facility-edit"=>new FACILITY_EDIT_INTENT{FACILITY_EDIT_INTENT_ID="original"},"facility"=>new FACILITY{FACILITY_ID="original",FACILITY_TYPE="gas"},"facility-link"=>new FACILITY_FIELD{FACILITY_ID="original",FACILITY_TYPE="gas",FIELD_ID="A"},"create-intent"=>new FACILITY_CREATE_INTENT{FACILITY_CREATE_INTENT_ID="original"},_=>new WORK_ORDER_ASSET_RETURN{RETURN_ID="original-return"}};
        var reads=f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");
        await Assert.ThrowsAsync<RefusalException>(()=>f.Policy.ValidateMutationAsync(row.GetType(),row.GetType().Name,row,operation==PPDMRowMutation.Insert?null:row,operation,"actor"));
        Assert.Empty(f.Writes);Assert.Equal(reads,f.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));
    }
    [Fact]
    public async Task Selected_company_grants_filter_the_real_repository_before_provider_reads()
    {
        using var f = await Fixture.Create();
        f.Rows.AddRange([new FIELD { FIELD_ID = "A" }, new FIELD { FIELD_ID = "B" }]);
        await f.Grant("FIELD", "A");
        var result = await f.Service.GetEntitiesAsync("FIELD", []);
        Assert.Equal("A", Assert.Single(result.Entities)["FIELD_ID"]);
        Assert.Equal("A", Assert.Single(Assert.Single(f.Queries)).FilterValue);
        Assert.NotEmpty(f.Connections); Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection));
        await f.Select("other-company");
        Assert.Empty((await f.Service.GetEntitiesAsync("FIELD", [])).Entities);
        Assert.Single(f.Queries); // Same tenant database, no cross-company grants.
    }

    [Fact]
    public async Task No_grants_and_an_administrator_role_do_not_open_an_unfiltered_query()
    {
        using var f = await Fixture.Create();
        Assert.Empty((await f.Service.GetEntitiesAsync("FIELD", [])).Entities);
        Assert.Empty(f.Queries); Assert.Empty(f.Connections);
    }

    [Fact]
    public async Task Direct_and_inherited_well_grants_use_separate_SDK_conjunctions_and_deduplicate_keys()
    {
        using var f = await Fixture.Create();
        f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company" });
        await f.Grant("WELL", "well"); await f.Grant("FIELD", "A", inherit: true);
        Assert.Single((await f.Service.GetEntitiesAsync("WELL", [])).Entities);
        Assert.Equal(new[] { "UWI", "ASSIGNED_FIELD" }, f.Queries.Select(query => query.Single(filter => filter.FieldName != "OPERATOR").FieldName));
    }

    [Theory]
    [InlineData("READ", false)]
    [InlineData("READ", true)]
    [InlineData("WRITE", false)]
    [InlineData("WRITE", true)]
    public async Task Field_inheritance_is_explicit_and_preserves_access_level(string level, bool inherit)
    {
        using var f = await Fixture.Create(); f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company" });
        await f.Grant("FIELD", "A", level, inherit);
        Assert.Equal(inherit ? 1 : 0, (await f.Service.GetEntitiesAsync("WELL", [])).Count);
        if (inherit && level == "WRITE")
            Assert.True((await f.Service.UpdateEntityAsync("WELL", "well", new() { ["UWI"] = "well", ["ASSIGNED_FIELD"] = "A", ["OPERATOR"] = "company" }, "actor")).Success);
        else
            await Assert.ThrowsAsync<RefusalException>(() => f.Service.UpdateEntityAsync("WELL", "well", new() { ["UWI"] = "well", ["ASSIGNED_FIELD"] = "A", ["OPERATOR"] = "company" }, "actor"));
        Assert.Equal(inherit && level == "WRITE" ? 1 : 0, f.Writes.Count);
    }

    [Theory]
    [InlineData("FACILITY")]
    public async Task Historical_unqualified_composite_grants_cannot_authorize_rows_without_field_inheritance(string table)
    {
        using var f = await Fixture.Create();
        object row = table == "POOL" ? new POOL { POOL_ID = "asset", POOL_TYPE = "type", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company" } :
            new FACILITY { FACILITY_ID = "asset", FACILITY_TYPE = "type", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company" };
        f.Rows.Add(row);
        f.Db.Add(new AppUserAssetAccess { Id = "historical-composite", UserId = "actor", AssetType = table, AssetId = "asset",
            DatabaseScope = (await f.Gate.ResolveAsync()).Fingerprint, OrganizationId = "company", AccessLevel = "READ", IsActive = true, CreatedBy = "actor", ChangedBy = "actor" });
        await f.Db.SaveChangesAsync();
        Assert.Empty((await f.Service.GetEntitiesAsync(table, [])).Entities);
        await f.Grant("FIELD", "A", inherit: true);
        Assert.Single((await f.Service.GetEntitiesAsync(table, [])).Entities);
    }

    [Fact]
    public async Task A_direct_pool_grant_uses_the_metadata_proven_single_pool_key()
    {
        using var f = await Fixture.Create(); await f.Grant("POOL", "pool");
        f.Rows.Add(new POOL { POOL_ID = "pool", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company" });
        Assert.Single((await f.Service.GetEntitiesAsync("POOL", [])).Entities);
        Assert.Contains(Assert.Single(f.Queries), filter => filter.FieldName == "POOL_ID" && filter.FilterValue == "pool");
    }

    [Theory]
    [InlineData("key")]
    [InlineData("owner-reference")]
    public async Task Changed_primary_key_or_ownership_metadata_refuses_access_before_a_provider_read(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        var table = await f.Metadata.Object.GetTableMetadataAsync("WELL");
        if (reason == "key") table.PrimaryKeyColumn = "ASSIGNED_FIELD";
        else table.ForeignKeys.Single(key => key.ForeignKeyColumn == "ASSIGNED_FIELD").ReferencedTable = "BUSINESS_ASSOCIATE";
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("WELL", []));
        Assert.Empty(f.Connections); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Provider_write_failure_is_not_reported_as_a_successful_mutation()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE");
        f.Source.Setup(value => value.InsertEntity("FIELD", It.IsAny<object>())).Returns(new ErrorsInfo { Flag = Errors.Failed });
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Service.InsertEntityAsync("FIELD", new() { ["FIELD_ID"] = "A" }, "actor"));
    }

    [Theory]
    [InlineData("field")]
    [InlineData("operator")]
    public async Task Caller_filters_cannot_supply_column_or_operator_fragments(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A");
        var filters = new List<AppFilter> { new() { FieldName = reason == "field" ? "FIELD_ID OR 1=1" : "FIELD_ID",
            Operator = reason == "operator" ? "= A OR 1=1" : "=", FilterValue = "A" } };
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("FIELD", filters));
        Assert.Empty(f.Queries); Assert.Empty(f.Connections);
    }

    [Fact]
    public async Task Caller_filters_are_copied_and_ownership_filters_cannot_be_overridden()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A" }, new FIELD { FIELD_ID = "B" }]);
        var requested = new List<AppFilter> { new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = "B" } };
        Assert.Empty((await f.Service.GetEntitiesAsync("FIELD", requested)).Entities);
        Assert.Single(requested); Assert.Equal("B", requested[0].FilterValue);
        Assert.Equal(new[] { "B", "A" }, Assert.Single(f.Queries).Select(filter => filter.FilterValue));
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("wrong-authorized-key")]
    [InlineData("missing-key")]
    public async Task A_provider_ignoring_required_filters_or_keys_never_returns_data(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A");
        if (reason == "wrong-authorized-key") await f.Grant("FIELD", "B");
        f.Rows.Add(new FIELD { FIELD_ID = reason == "missing-key" ? null! : "B" }); f.IgnoreFilters = true;
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntityByIdAsync("FIELD", "A"));
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("membership")]
    [InlineData("asset")]
    [InlineData("binding")]
    public async Task Access_changes_during_a_provider_read_refuse_the_result(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); f.Rows.Add(new FIELD { FIELD_ID = "A" });
        f.AfterRead = async () =>
        {
            if (reason == "membership") f.Db.Remove(await f.Db.Set<AppCompanyMembership>().SingleAsync(value => value.BusinessAssociateId == "company"));
            if (reason == "asset") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
            if (reason == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("FIELD", []));
        Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task A_revoked_later_predicate_does_not_reach_the_provider()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "B");
        f.Rows.AddRange([new FIELD { FIELD_ID = "A" }, new FIELD { FIELD_ID = "B" }]);
        f.AfterRead = async () =>
        {
            var first = f.Queries[0].Single().FilterValue;
            (await f.Db.Set<AppUserAssetAccess>().SingleAsync(value => value.AssetId != first)).IsActive = false;
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("FIELD", []));
        Assert.Single(f.Queries);
    }

    [Theory]
    [InlineData("READ", false, false)]
    [InlineData("WRITE", true, false)]
    [InlineData("DELETE", true, true)]
    public async Task Write_and_delete_operations_require_their_asset_access_levels(string level, bool canWrite, bool canDelete)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", level); f.Rows.Add(new FIELD { FIELD_ID = "A" });
        if (canWrite) Assert.True((await f.Service.InsertEntityAsync("FIELD", new() { ["FIELD_ID"] = "A" }, "actor")).Success);
        else await Assert.ThrowsAsync<RefusalException>(() => f.Service.InsertEntityAsync("FIELD", new() { ["FIELD_ID"] = "A" }, "actor"));
        if (canDelete) Assert.True((await f.Service.DeleteEntityAsync("FIELD", "A", "actor")).Success);
        else await Assert.ThrowsAsync<RefusalException>(() => f.Service.DeleteEntityAsync("FIELD", "A", "actor"));
        Assert.Equal((canWrite ? 1 : 0) + (canDelete ? 1 : 0), f.Writes.Count);
    }

    [Theory]
    [InlineData("route")]
    [InlineData("owner")]
    [InlineData("foreign-actor")]
    [InlineData("missing-row")]
    public async Task Generic_updates_refuse_identity_ownership_and_actor_changes(string reason)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE", true); await f.Grant("FIELD", "B", "WRITE", true);
        if (reason != "missing-row") f.Rows.Add(new WELL { UWI = "well", ASSIGNED_FIELD = "A", OPERATOR = "company" });
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.UpdateEntityAsync("WELL", reason == "route" ? "another" : "well",
            new() { ["UWI"] = "well", ["ASSIGNED_FIELD"] = reason == "owner" ? "B" : "A", ["OPERATOR"] = "company" }, reason == "foreign-actor" ? "other" : "actor"));
        Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Unknown_table_ownership_and_direct_UnitOfWork_or_batch_paths_fail_closed()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE");
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("BUSINESS_ASSOCIATE", []));
        var repo = f.Repository(typeof(FIELD));
        Assert.Throws<RefusalException>(() => repo.UnitOfWork);
        await Assert.ThrowsAsync<RefusalException>(() => repo.InsertBatchAsync([new FIELD { FIELD_ID = "A" }], "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => repo.UpdateBatchAsync([new FIELD { FIELD_ID = "A" }], "actor"));
        Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Actual_CSV_executor_enforces_import_permission_and_each_rows_asset_grant()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE");
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        try
        {
            await File.WriteAllTextAsync(file, "FIELD_ID\nA\nB");
            var job = new CsvImportJob("op", "FIELD", f.Gate.Company.Scope, await f.Gate.ResolveAsync(), "actor", false, []);
            var executor = new CsvImportExecutor(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
                NullLoggerFactory.Instance, Mock.Of<IProgressTrackingService>(), f.Gate, f.Policy);
            await Assert.ThrowsAsync<RefusalException>(() => executor.ExecuteAsync(job, file));
            Assert.Equal("A", ((FIELD)Assert.Single(f.Writes)).FIELD_ID); // Partial import is explicit, never reported atomic.
            f.Db.RoleClaims.Remove(await f.Db.RoleClaims.SingleAsync(value => value.ClaimValue == PermissionConstants.DataManagement.ImportData));
            await f.Db.SaveChangesAsync(); await f.Select("company");
            var denied = new CsvImportExecutor(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
                NullLoggerFactory.Instance, Mock.Of<IProgressTrackingService>(), f.Gate, f.Policy);
            await Assert.ThrowsAsync<RefusalException>(() => denied.ExecuteAsync(job, file));
            Assert.Single(f.Writes);
        }
        finally { File.Delete(file); }
    }

}
