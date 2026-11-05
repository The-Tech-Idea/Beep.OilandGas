using System.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetRelationshipTests
{
    private static string Column(string table) => table switch { "WELL" => "OPERATOR", "POOL" => "BUSINESS_ASSOCIATE_ID", _ => "CURRENT_OPERATOR" };
    private static object Row(string table, string? company) => table switch
    {
        "WELL" => new WELL { UWI = "asset", ASSIGNED_FIELD = "A", OPERATOR = company!, ACTIVE_IND = "Y" },
        "POOL" => new POOL { POOL_ID = "asset", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = company!, ACTIVE_IND = "Y" },
        _ => new FACILITY { FACILITY_ID = "asset", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = company!, ACTIVE_IND = "Y" }
    };
    private static RepositoryAssetAccessStore Store(Fixture f) => new(f.Db, f.Gate,
        new PPDMAssetGrantTargetVerifier(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Eligibility));
    private static void Admission(Fixture f)
    {
        _ = CompanyAssetFacadeTests.Service(f);
        f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(ConnectionState.Open);
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
    }

    [Theory]
    [InlineData("WELL", null)] [InlineData("WELL", "other")] [InlineData("WELL", "COMPANY")]
    [InlineData("POOL", null)] [InlineData("POOL", "other")] [InlineData("POOL", "COMPANY")]
    public async Task Active_assets_without_the_exact_selected_company_relationship_cannot_admit_grants(string table, string? company)
    {
        using var f = await Fixture.Create(); Admission(f); f.Rows.Add(Row(table, company));
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, table, "asset", "READ", false))).Kind);
        Assert.Contains(f.Queries.Last(), filter => filter.FieldName == Column(table) && filter.FilterValue == "company");
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync()); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")]
    public async Task Admission_postchecks_refuse_a_provider_ignoring_company_filters(string table)
    {
        using var f = await Fixture.Create(); Admission(f); f.Rows.Add(Row(table, "other")); f.IgnoreFilters = true;
        var scope = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, table, "asset", "READ", false))).Kind);
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToListAsync());
    }

    [Theory]
    [InlineData("WELL", "missing")] [InlineData("WELL", "table")] [InlineData("WELL", "key")] [InlineData("WELL", "duplicate")]
    [InlineData("POOL", "missing")] [InlineData("POOL", "table")] [InlineData("POOL", "key")] [InlineData("POOL", "duplicate")]
    [InlineData("FACILITY", "missing")] [InlineData("FACILITY", "table")] [InlineData("FACILITY", "key")] [InlineData("FACILITY", "duplicate")]
    public async Task Relationship_metadata_drift_refuses_reads_and_admission_before_provider_execution(string table, string reason)
    {
        using var f = await Fixture.Create(); Admission(f); await f.Grant("FIELD", "A", inherit: true);
        var definition = await f.Metadata.Object.GetTableMetadataAsync(table);
        var relation = definition.ForeignKeys.Single(key => key.ForeignKeyColumn == Column(table));
        if (reason == "missing") definition.ForeignKeys.Remove(relation);
        if (reason == "table") relation.ReferencedTable = "FIELD";
        if (reason == "key") relation.ReferencedPrimaryKey = "OTHER";
        if (reason == "duplicate") definition.ForeignKeys.Add(relation);
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync(table, []))).Kind);
        if (table != "FACILITY")
        {
            var scope = await f.Gate.ResolveAsync();
            Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Store(f).GrantAsync("actor", scope.Fingerprint, table, "asset", "READ", false))).Kind);
        }
        Assert.Empty(f.Queries); Assert.Empty(f.Connections); Assert.Empty(f.Writes);
        f.Editor.Verify(editor => editor.OpenDataSource(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Company_constraints_are_ANDed_with_every_direct_and_inherited_grant_query(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        if (table != "FACILITY") await f.Grant(table, "asset");
        f.Rows.Add(Row(table, "company")); f.Rows.Add(Row(table, "other"));
        var filters = new List<AppFilter> { new() { FieldName = Column(table), Operator = "=", FilterValue = "company" } };
        Assert.Single((await f.Service.GetEntitiesAsync(table, filters)).Entities);
        Assert.Equal(table == "FACILITY" ? 1 : 2, f.Queries.Count);
        Assert.All(f.Queries, query => Assert.Equal(2, query.Count(filter => filter.FieldName == Column(table) && filter.FilterValue == "company")));
        Assert.Single(filters); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task A_company_switch_with_identical_asset_and_field_ids_reads_only_its_company_relationship(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(Row(table, "company")); f.Rows.Add(Row(table, "other-company"));
        Assert.Equal("company", Assert.Single((await f.Service.GetEntitiesAsync(table, [])).Entities)[Column(table)]);
        await f.Select("other-company"); await f.Grant("FIELD", "A", inherit: true);
        Assert.Equal("other-company", Assert.Single((await f.Service.GetEntitiesAsync(table, [])).Entities)[Column(table)]);
        Assert.Equal(new[] { "company", "other-company" }, f.Queries.Select(query => query.Single(filter => filter.FieldName == Column(table)).FilterValue));
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Caller_company_filters_cannot_replace_the_selected_company_constraint(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.Add(Row(table, "other"));
        var requested = new List<AppFilter> { new() { FieldName = Column(table), Operator = "=", FilterValue = "other" } };
        Assert.Empty((await f.Service.GetEntitiesAsync(table, requested)).Entities);
        var query = Assert.Single(f.Queries);
        Assert.Contains(query, filter => filter.FieldName == Column(table) && filter.FilterValue == "other");
        Assert.Contains(query, filter => filter.FieldName == Column(table) && filter.FilterValue == "company");
        Assert.Equal("other", Assert.Single(requested).FilterValue);
    }

    [Theory]
    [InlineData("WELL", null)] [InlineData("WELL", "other")] [InlineData("WELL", "COMPANY")]
    [InlineData("POOL", null)] [InlineData("POOL", "other")] [InlineData("POOL", "COMPANY")]
    [InlineData("FACILITY", null)] [InlineData("FACILITY", "other")] [InlineData("FACILITY", "COMPANY")]
    public async Task Historical_and_inherited_grants_cannot_read_another_company_relationship(string table, string? company)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        if (table != "FACILITY") await f.Grant(table, "asset");
        f.Rows.Add(Row(table, company));
        Assert.Empty((await f.Service.GetEntitiesAsync(table, [])).Entities); Assert.Empty(f.Writes);
        Assert.All(f.Queries, query => Assert.Contains(query, filter => filter.FieldName == Column(table) && filter.FilterValue == "company"));
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Provider_results_cannot_bypass_company_constraints_even_with_valid_asset_grants(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(Row(table, "other")); f.IgnoreFilters = true;
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync(table, []))).Kind);
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task An_operator_or_company_change_observed_during_a_read_refuses_the_result(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        var row = Row(table, "company"); f.Rows.Add(row);
        f.AfterRead = () => { row.GetType().GetProperty(Column(table))!.SetValue(row, "other"); return Task.CompletedTask; };
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync(table, []))).Kind);
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL", false)] [InlineData("WELL", true)] [InlineData("POOL", false)] [InlineData("POOL", true)]
    public async Task Actual_generic_insert_and_update_cannot_assign_another_company_relationship(string table, bool update)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE", true);
        f.Rows.Add(Row(table, "company"));
        var data = table == "WELL" ? new Dictionary<string, object> { ["UWI"] = "asset", ["ASSIGNED_FIELD"] = "A", ["OPERATOR"] = "other" } :
            new Dictionary<string, object> { ["POOL_ID"] = "asset", ["FIELD_ID"] = "A", ["BUSINESS_ASSOCIATE_ID"] = "other" };
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => update ?
            f.Service.UpdateEntityAsync(table, "asset", data, "actor") : f.Service.InsertEntityAsync(table, data, "actor"))).Kind);
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Mutation_policy_requires_the_company_on_both_existing_and_proposed_rows(string table)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "DELETE", true);
        var own = Row(table, "company"); var other = Row(table, "other"); var type = own.GetType();
        await f.Policy.ValidateMutationAsync(type, table, own, own, PPDMRowMutation.Update, "actor");
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(type, table, own, other, PPDMRowMutation.Update, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(type, table, other, own, PPDMRowMutation.Update, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(type, table, other, other, PPDMRowMutation.Delete, "actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(type, table, Row(table, null), null, PPDMRowMutation.Insert, "actor"));
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")]
    public async Task Changed_company_relationship_blocks_existing_reads_and_renewal_without_rewriting_registry_history(string table)
    {
        using var f = await Fixture.Create(); Admission(f); var row = Row(table, "company"); f.Rows.Add(row);
        var scope = await f.Gate.ResolveAsync(); var store = Store(f);
        Assert.True(await store.GrantAsync("actor", scope.Fingerprint, table, "asset", "READ", false));
        var before = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync();
        row.GetType().GetProperty(Column(table))!.SetValue(row, "other");
        Assert.Empty((await f.Service.GetEntitiesAsync(table, [])).Entities);
        await Assert.ThrowsAsync<RefusalException>(() => store.GrantAsync("actor", scope.Fingerprint, table, "asset", "WRITE", false));
        var after = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync();
        Assert.True(after.IsActive); Assert.Equal(before.ConcurrencyStamp, after.ConcurrencyStamp); Assert.Equal("READ", after.AccessLevel);
        Assert.True(await store.RevokeAsync("actor", scope.Fingerprint, table, "asset"));
    }

    [Fact]
    public async Task Actual_CSV_execution_refuses_a_foreign_operator_after_writing_only_the_authorized_row()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", "WRITE", true);
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
        try
        {
            await File.WriteAllTextAsync(file, "UWI,ASSIGNED_FIELD,OPERATOR\nown,A,company\nforeign,A,other");
            var job = new CsvImportJob("op", "WELL", f.Gate.Company.Scope, await f.Gate.ResolveAsync(), "actor", false, []);
            var executor = new CsvImportExecutor(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
                NullLoggerFactory.Instance, Mock.Of<IProgressTrackingService>(), f.Gate, f.Policy);
            await Assert.ThrowsAsync<RefusalException>(() => executor.ExecuteAsync(job, file));
            var written = (WELL)Assert.Single(f.Writes); Assert.Equal("own", written.UWI); Assert.Equal("company", written.OPERATOR);
        }
        finally { File.Delete(file); }
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Diagnostics_preserve_company_conjunctions_and_do_not_grant_access_without_asset_predicates(string table)
    {
        using var f = await Fixture.Create(); f.Http.HttpContext = f.Request;
        var service = new CompanyRowLevelSecurityService(f.Account, f.Gate, Store(f), f.Policy, f.Metadata.Object, f.Service);
        var filters = new List<AppFilter> { new() { FieldName = Column(table), Operator = "=", FilterValue = "other" } };
        var plan = await service.ApplyRowFiltersAsync("actor", new ApplyRowFiltersRequest(table, filters));
        Assert.True(plan.DenyAll); Assert.Empty(plan.ReadConjunctions);
        Assert.Equal("company", Assert.Single(plan.CompanyConstraints).FilterValue); Assert.Equal(Column(table), plan.CompanyConstraints[0].FieldName);
        Assert.Equal("other", Assert.Single(plan.Filters).FilterValue); Assert.Single(filters);
        await f.Grant("FIELD", "A", inherit: true);
        plan = await service.ApplyRowFiltersAsync("actor", new(table)); Assert.False(plan.DenyAll); Assert.Single(plan.CompanyConstraints);
        Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }
}
