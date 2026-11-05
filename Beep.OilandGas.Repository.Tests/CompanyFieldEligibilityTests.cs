using System.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldEligibilityTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
    private sealed class Clock(DateTime instant) : TimeProvider
    {
        public DateTime Instant { get; set; } = instant;
        public override DateTimeOffset GetUtcNow() => new(Instant);
    }
    private static PPDMCompanyFieldEligibility Eligibility(Fixture f, TimeProvider? clock = null) =>
        new(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object,
            Options.Create(f.EligibilityConfiguration), clock ?? new Clock(Now));
    private static ENT_COMPONENT Component(Fixture f) => f.Rows.OfType<ENT_COMPONENT>().Single();
    private static ENTITLEMENT Header(Fixture f) => f.Rows.OfType<ENTITLEMENT>().Single();
    private static CompanyFieldEligibilityRules Rules(Fixture f) => f.EligibilityConfiguration.Companies.Single();
    private static async Task<Fixture> Ready(string level = "DELETE")
    {
        var f = await Fixture.Create(); f.AddFieldEntitlement("A", level);
        f.Rows.Add(new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y", FIELD_NAME = "Recorded field" });
        f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" });
        return f;
    }

    [Theory]
    [InlineData("READ")] [InlineData("WRITE")] [InlineData("DELETE")]
    public async Task Exact_company_entitlement_chain_resolves_the_reviewed_level_and_uses_bounded_Beep_predicates(string level)
    {
        using var f = await Ready(level);
        Assert.Equal(level, await Eligibility(f).GetMaximumLevelAsync("A")); Assert.Empty(f.Queries); Assert.Empty(f.Writes);
        var component = Assert.Single(f.EligibilityQueries, query => query.Table == "ENT_COMPONENT");
        Assert.Equal(5, component.Filters.Count);
        Assert.Contains(component.Filters, filter => filter.FieldName == "FIELD_ID" && filter.FilterValue == "A");
        Assert.Contains(component.Filters, filter => filter.FieldName == "BUSINESS_ASSOCIATE_ID" && filter.FilterValue == "company");
        Assert.All(f.EligibilityQueries, query => {
            Assert.All(query.Filters, filter => Assert.Equal("=", filter.Operator));
            Assert.Contains(query.Filters, filter => filter.FieldName == "ACTIVE_IND" && filter.FilterValue == "Y");
        });
        Assert.All(f.Connections, connection => Assert.Equal("selected-tenant", connection));
    }

    [Theory]
    [InlineData("missing-component")] [InlineData("foreign-company")] [InlineData("foreign-field")]
    [InlineData("inactive-component")] [InlineData("wrong-component-type")] [InlineData("wrong-component-condition")]
    [InlineData("missing-header")] [InlineData("inactive-header")] [InlineData("wrong-header-type")]
    [InlineData("unknown-access-code")] [InlineData("missing-component-date")] [InlineData("missing-header-date")]
    [InlineData("expired-component")] [InlineData("expired-header")] [InlineData("future-component")]
    [InlineData("future-header")] [InlineData("missing-type-reference")] [InlineData("inactive-access-reference")]
    [InlineData("expired-condition-reference")]
    public async Task Missing_foreign_inactive_or_noncurrent_PPDM_evidence_does_not_become_company_FIELD_authority(string fault)
    {
        using var f = await Ready(); var component = Component(f); var header = Header(f);
        switch (fault)
        {
            case "missing-component": f.Rows.Remove(component); break;
            case "foreign-company": component.BUSINESS_ASSOCIATE_ID = "other"; break;
            case "foreign-field": component.FIELD_ID = "B"; break;
            case "inactive-component": component.ACTIVE_IND = "N"; break;
            case "wrong-component-type": component.ENT_COMP_TYPE = "other"; break;
            case "wrong-component-condition": component.ACCESS_COND_CODE = "other"; break;
            case "missing-header": f.Rows.Remove(header); break;
            case "inactive-header": header.ACTIVE_IND = "N"; break;
            case "wrong-header-type": header.ENTITLEMENT_TYPE = "other"; break;
            case "unknown-access-code": header.ACCESS_COND_CODE = "display-name-delete"; break;
            case "missing-component-date": component.EFFECTIVE_DATE = null; break;
            case "missing-header-date": header.EFFECTIVE_DATE = null; break;
            case "expired-component": component.EXPIRY_DATE = Now; break;
            case "expired-header": header.EXPIRY_DATE = Now; break;
            case "future-component": component.EFFECTIVE_DATE = Now.AddSeconds(1); break;
            case "future-header": header.EFFECTIVE_DATE = Now.AddSeconds(1); break;
            case "missing-type-reference": f.Rows.RemoveAll(row => row is R_ENT_TYPE); break;
            case "inactive-access-reference": f.Rows.OfType<R_ENT_ACCESS_TYPE>().Single(row => row.ACCESS_TYPE == "test-delete").ACTIVE_IND = "N"; break;
            case "expired-condition-reference": f.Rows.OfType<R_ACCESS_CONDITION>().Single().EXPIRY_DATE = Now; break;
        }
        Assert.Null(await Eligibility(f).GetMaximumLevelAsync("A")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("component-key")] [InlineData("negative-observation")] [InlineData("fractional-observation")]
    [InlineData("duplicate-component")] [InlineData("duplicate-header")] [InlineData("duplicate-reference")]
    [InlineData("component-rule")] [InlineData("component-expiry-action")]
    [InlineData("header-access-condition")] [InlineData("header-use-condition")] [InlineData("header-expiry-action")]
    [InlineData("inverted-window")]
    public async Task Ambiguous_keys_and_uninterpreted_terms_refuse_authority_explicitly(string fault)
    {
        using var f = await Ready(); var component = Component(f); var header = Header(f);
        switch (fault)
        {
            case "component-key": component.COMPONENT_ID = " "; break;
            case "negative-observation": component.COMPONENT_OBS_NO = -1; break;
            case "fractional-observation": component.COMPONENT_OBS_NO = 0.5m; break;
            case "duplicate-component": f.Rows.Add(component); break;
            case "duplicate-header": f.Rows.Add(header); break;
            case "duplicate-reference": f.Rows.Add(f.Rows.OfType<R_ENT_TYPE>().Single()); break;
            case "component-rule": component.ENT_RULE = "approve later"; break;
            case "component-expiry-action": component.EXPIRY_ACTION = "renew"; break;
            case "header-access-condition": header.ACCESS_CONDITION = "country restriction"; break;
            case "header-use-condition": header.USE_CONDITION = "commercial approval"; break;
            case "header-expiry-action": header.EXPIRY_ACTION = "renew"; break;
            case "inverted-window": header.EXPIRY_DATE = header.EFFECTIVE_DATE; break;
        }
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"))).Kind);
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("missing-company")] [InlineData("duplicate-company")] [InlineData("wrong-tenant")]
    [InlineData("missing-zone")] [InlineData("unknown-zone")] [InlineData("missing-level")]
    [InlineData("aliased-code")] [InlineData("unknown-level")] [InlineData("padded-code")]
    public async Task Explicit_company_vocabulary_and_date_zone_are_required_without_inferred_defaults(string fault)
    {
        using var f = await Ready(); var rule = Rules(f);
        switch (fault)
        {
            case "missing-company": f.EligibilityConfiguration.Companies.Clear(); break;
            case "duplicate-company": f.EligibilityConfiguration.Companies.Add(rule); break;
            case "wrong-tenant": rule.TenantId = "other"; break;
            case "missing-zone": rule.TimeZoneId = ""; break;
            case "unknown-zone": rule.TimeZoneId = "unsupported-reviewed-zone"; break;
            case "missing-level": rule.AccessTypes.Remove("WRITE"); break;
            case "aliased-code": rule.AccessTypes["READ"] = rule.AccessTypes["DELETE"]; break;
            case "unknown-level": rule.AccessTypes.Remove("WRITE"); rule.AccessTypes.Add("ADMIN", "test-write"); break;
            case "padded-code": rule.ComponentType = " test-field"; break;
        }
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"))).Kind);
        Assert.Empty(f.EligibilityQueries); Assert.Empty(f.Connections);
    }

    [Theory]
    [InlineData("ENT_COMPONENT", "key")] [InlineData("ENT_COMPONENT", "field")]
    [InlineData("ENT_COMPONENT", "company")] [InlineData("ENT_COMPONENT", "condition")]
    [InlineData("ENTITLEMENT", "key")] [InlineData("ENTITLEMENT", "access-type")]
    public async Task Canonical_relationship_metadata_drift_refuses_evidence_before_opening_the_provider(string table, string fault)
    {
        using var f = await Ready(); var definition = await f.Metadata.Object.GetTableMetadataAsync(table);
        if (fault == "key") definition.PrimaryKeyColumn = "WRONG";
        else {
            var column = fault switch { "field" => "FIELD_ID", "company" => "BUSINESS_ASSOCIATE_ID", _ => "ACCESS_COND_CODE" };
            var key = definition.ForeignKeys.Single(key => key.ForeignKeyColumn == column && (fault != "company" || key.ReferencedTable == "BUSINESS_ASSOCIATE"));
            key.ReferencedPrimaryKey = "WRONG";
        }
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"))).Kind);
        Assert.Empty(f.Connections); Assert.Empty(f.EligibilityQueries);
    }

    [Theory]
    [InlineData("filter-company")] [InlineData("filter-active")] [InlineData("provider-error")]
    [InlineData("failed-open")] [InlineData("membership")] [InlineData("binding")]
    public async Task Provider_or_current_scope_failure_never_looks_like_verified_eligibility(string fault)
    {
        using var f = await Ready();
        if (fault.StartsWith("filter")) {
            f.IgnoreEligibilityFilters = true;
            if (fault == "filter-company") Component(f).BUSINESS_ASSOCIATE_ID = "other"; else Component(f).ACTIVE_IND = "N";
        }
        if (fault == "provider-error") f.Source.SetupGet(source => source.ErrorObject).Returns(new ErrorsInfo { Flag = Errors.Failed });
        if (fault == "failed-open") f.Editor.Setup(editor => editor.OpenDataSource("selected-tenant")).Returns(ConnectionState.Broken);
        if (fault is "membership" or "binding") f.AfterEligibilityRead = async () => {
            f.AfterEligibilityRead = null;
            if (fault == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = Now.AddYears(-1);
            else (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            await f.Db.SaveChangesAsync();
        };
        if (fault is "provider-error" or "failed-open") await Assert.ThrowsAsync<InvalidOperationException>(() => Eligibility(f).GetMaximumLevelAsync("A"));
        else await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"));
        Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Configured_unspecified_boundaries_use_company_time_and_expiry_is_exclusive()
    {
        using var f = await Ready(); Rules(f).TimeZoneId = "Asia/Kuwait";
        Component(f).EFFECTIVE_DATE = Header(f).EFFECTIVE_DATE = new DateTime(2026, 10, 5, 15, 0, 0, DateTimeKind.Unspecified);
        Header(f).EXPIRY_DATE = new DateTime(2026, 10, 5, 15, 1, 0, DateTimeKind.Unspecified);
        var clock = new Clock(Now);
        Assert.Equal("DELETE", await Eligibility(f, clock).GetMaximumLevelAsync("A"));
        clock.Instant = Now.AddMinutes(1); Assert.Null(await Eligibility(f, clock).GetMaximumLevelAsync("A"));
    }

    [Theory]
    [InlineData("invalid")] [InlineData("ambiguous")] [InlineData("machine-local")]
    public async Task Ambiguous_invalid_and_machine_local_timestamps_are_refused(string fault)
    {
        using var f = await Ready(); Rules(f).TimeZoneId = "America/New_York";
        Header(f).EFFECTIVE_DATE = fault switch {
            "invalid" => new DateTime(2026, 3, 8, 2, 30, 0, DateTimeKind.Unspecified),
            "ambiguous" => new DateTime(2025, 11, 2, 1, 30, 0, DateTimeKind.Unspecified),
            _ => DateTime.SpecifyKind(Now.AddMinutes(-1), DateTimeKind.Local)
        };
        await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"));
    }

    [Theory]
    [InlineData("component")] [InlineData("header")] [InlineData("reference")]
    public async Task Evidence_that_expires_during_the_last_provider_read_does_not_escape_the_return_check(string target)
    {
        using var f = await Ready(); var clock = new Clock(Now);
        if (target == "component") Component(f).EXPIRY_DATE = Now.AddSeconds(1);
        if (target == "header") Header(f).EXPIRY_DATE = Now.AddSeconds(1);
        if (target == "reference") f.Rows.OfType<R_ENT_TYPE>().Single().EXPIRY_DATE = Now.AddSeconds(1);
        f.AfterEligibilityRead = () => { if (f.EligibilityQueries.Last().Table == "R_ENT_ACCESS_TYPE") clock.Instant = Now.AddSeconds(1); return Task.CompletedTask; };
        Assert.Null(await Eligibility(f, clock).GetMaximumLevelAsync("A"));
    }

    [Theory]
    [InlineData("READ", "DELETE", "READ")] [InlineData("WRITE", "DELETE", "WRITE")]
    [InlineData("DELETE", "READ", "READ")]
    public async Task Stored_app_grants_are_capped_in_the_actual_facade_and_generic_mutations_without_rewriting_evidence(string entitlement, string grant, string effective)
    {
        using var f = await Ready(entitlement); await f.Grant("FIELD", "A", grant, inherit: true);
        var before = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync();
        Assert.Equal(effective, Assert.Single(await CompanyAssetFacadeTests.Service(f).GetUserAccessibleAssetsAsync("actor", "FIELD")).AccessLevel);
        if (effective == "READ") await Assert.ThrowsAsync<RefusalException>(() => f.Service.InsertEntityAsync("FIELD", new() { ["FIELD_ID"] = "A" }, "actor"));
        else Assert.True((await f.Service.InsertEntityAsync("FIELD", new() { ["FIELD_ID"] = "A" }, "actor")).Success);
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.DeleteEntityAsync("FIELD", "A", "actor"));
        var after = await f.Db.Set<AppUserAssetAccess>().AsNoTracking().SingleAsync(); Assert.Equal(grant, after.AccessLevel);
        Assert.Equal(before.ConcurrencyStamp, after.ConcurrencyStamp); Assert.Equal(before.ChangedUtc, after.ChangedUtc);
    }

    [Fact]
    public async Task Neither_a_generic_field_component_link_nor_a_financial_interest_substitutes_for_data_entitlement()
    {
        using var f = await Ready(); await f.Grant("FIELD", "A", "DELETE", true); f.Rows.RemoveAll(row => row is ENT_COMPONENT);
        f.Rows.Add(new FIELD_COMPONENT { FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", INTEREST_SET_ID = "partnership" });
        Assert.Empty((await f.Service.GetEntitiesAsync("FIELD", [])).Entities);
        Assert.Empty(await CompanyAssetFacadeTests.Service(f).GetUserAccessibleAssetsAsync("actor", "FIELD")); Assert.Empty(f.Queries);
        Assert.True((await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive);
    }

    [Theory]
    [InlineData("WELL")] [InlineData("POOL")] [InlineData("FACILITY")]
    public async Task Inherited_children_observe_the_FIELD_cap_and_a_direct_grant_keeps_its_own_reviewed_authority(string table)
    {
        using var f = await Ready("READ"); await f.Grant("FIELD", "A", "DELETE", true);
        object row = table switch {
            "WELL" => new WELL { UWI = "child", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" },
            "POOL" => new POOL { POOL_ID = "child", FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ACTIVE_IND = "Y" },
            _ => new FACILITY { FACILITY_ID = "child", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company", ACTIVE_IND = "Y" }
        };
        f.Rows.Add(row); var service = CompanyAssetFacadeTests.Service(f);
        Assert.Equal("READ", Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", table)).AccessLevel);
        Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor", table, includeInherited: false));
        await f.Policy.ValidateRowsAsync(row.GetType(), table, [row]);
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(row.GetType(), table, row, row, PPDMRowMutation.Update, "actor"));
        await f.Grant(table, "child", "WRITE", facilityType: table == "FACILITY" ? "gas" : null);
        Assert.Equal("WRITE", Assert.Single(await service.GetUserAccessibleAssetsAsync("actor", table, includeInherited: false)).AccessLevel);
        await f.Policy.ValidateMutationAsync(row.GetType(), table, row, row, PPDMRowMutation.Update, "actor");
    }

    [Theory]
    [InlineData("missing", "READ")] [InlineData("READ", "WRITE")] [InlineData("WRITE", "DELETE")]
    public async Task Actual_grant_admission_rejects_unsupported_FIELD_levels_before_any_registry_write(string entitlement, string requested)
    {
        using var f = await Ready(entitlement == "missing" ? "READ" : entitlement);
        if (entitlement == "missing") f.Rows.RemoveAll(row => row is ENT_COMPONENT);
        var verifier = new PPDMAssetGrantTargetVerifier(f.Gate, f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, Eligibility(f));
        var store = new RepositoryAssetAccessStore(f.Db, f.Gate, verifier);
        var database = await f.Gate.ResolveAsync();
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => store.GrantAsync("actor", database.Fingerprint, "FIELD", "A", requested, true))).Kind);
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync()); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task A_grant_revoked_during_entitlement_lookup_does_not_authorize_a_business_query()
    {
        using var f = await Ready(); await f.Grant("FIELD", "A", "DELETE");
        f.AfterEligibilityRead = async () => {
            f.AfterEligibilityRead = null; (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync();
        };
        Assert.Empty((await f.Service.GetEntitiesAsync("FIELD", [])).Entities); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Entitlement_revocation_during_a_business_read_refuses_the_accumulated_result()
    {
        using var f = await Ready(); await f.Grant("FIELD", "A"); f.AfterRead = () => { Component(f).ACTIVE_IND = "N"; return Task.CompletedTask; };
        await Assert.ThrowsAsync<RefusalException>(() => f.Service.GetEntitiesAsync("FIELD", [])); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("component")] [InlineData("header")] [InlineData("access-reference")]
    public async Task Multiple_entitlement_paths_use_only_the_highest_level_still_current_at_return(string expiring)
    {
        using var f = await Ready(); var clock = new Clock(Now); var component = Component(f); var header = Header(f);
        if (expiring == "component") component.EXPIRY_DATE = Now.AddSeconds(1);
        if (expiring == "header") header.EXPIRY_DATE = Now.AddSeconds(1);
        if (expiring == "access-reference") f.Rows.OfType<R_ENT_ACCESS_TYPE>().Single(row => row.ACCESS_TYPE == "test-delete").EXPIRY_DATE = Now.AddSeconds(1);
        f.Rows.Add(new ENT_COMPONENT { ENTITLEMENT_ID = "read-entitlement", COMPONENT_ID = "second", COMPONENT_OBS_NO = 2,
            FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ENT_COMP_TYPE = "test-field", ACCESS_COND_CODE = "test-unconditional",
            ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) });
        f.Rows.Add(new ENTITLEMENT { ENTITLEMENT_ID = "read-entitlement", ENTITLEMENT_TYPE = "test-app-data", ACCESS_COND_CODE = "test-read",
            ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) });
        f.AfterEligibilityRead = () => { if (f.EligibilityQueries.Last().Table == "R_ENT_ACCESS_TYPE" &&
            f.EligibilityQueries.Last().Filters.Any(filter => filter.FilterValue == "test-read")) clock.Instant = Now.AddSeconds(1); return Task.CompletedTask; };
        Assert.Equal("READ", await Eligibility(f, clock).GetMaximumLevelAsync("A"));
    }

    [Fact]
    public async Task Excessive_component_evidence_refuses_the_lookup_instead_of_truncating_authority()
    {
        using var f = await Ready(); var component = Component(f);
        for (var index = 2; index <= 257; index++) f.Rows.Add(new ENT_COMPONENT {
            ENTITLEMENT_ID = component.ENTITLEMENT_ID, COMPONENT_ID = "evidence-" + index, COMPONENT_OBS_NO = index,
            FIELD_ID = "A", BUSINESS_ASSOCIATE_ID = "company", ENT_COMP_TYPE = "test-field", ACCESS_COND_CODE = "test-unconditional",
            ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) });
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Eligibility(f).GetMaximumLevelAsync("A"))).Kind);
        Assert.DoesNotContain(f.EligibilityQueries, query => query.Table == "ENTITLEMENT");
    }

    [Fact]
    public async Task Scope_diagnostics_exclude_an_ineligible_FIELD_without_hiding_an_independent_direct_asset_grant()
    {
        using var f = await Ready(); await f.Grant("FIELD", "A", "DELETE", true); await f.Grant("WELL", "direct");
        Header(f).ACTIVE_IND = "N"; f.Http.HttpContext = f.Request;
        var diagnostics = new CompanyRowLevelSecurityService(f.Account, f.Gate,
            new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Policy, f.Metadata.Object, f.Service);
        Assert.Empty(await diagnostics.GetUserAccessibleFieldsAsync("actor"));
        Assert.Equal(new[] { "direct" }, await diagnostics.GetUserAccessibleAssetsAsync("actor"));
        Assert.True((await diagnostics.ApplyRowFiltersAsync("actor", new("FIELD"))).DenyAll); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task A_noninheriting_FIELD_grant_does_not_add_a_prerequisite_to_an_independent_direct_well_query()
    {
        using var f = await Ready(); await f.Grant("FIELD", "A", inherit: false); await f.Grant("WELL", "direct", "WRITE");
        f.EligibilityConfiguration.Companies.Clear(); f.Rows.Add(new WELL { UWI = "direct", OPERATOR = "company", ACTIVE_IND = "Y" });
        Assert.Equal("WRITE", Assert.Single(await CompanyAssetFacadeTests.Service(f).GetUserAccessibleAssetsAsync("actor", "WELL", includeInherited: false)).AccessLevel);
        Assert.Empty(f.EligibilityQueries);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task A_cached_datasource_target_changed_before_or_during_evidence_reads_is_refused(bool during)
    {
        using var f = await Ready();
        Task Change() {
            var connection = new Mock<IDataConnection>();
            connection.SetupGet(value => value.ConnectionProp).Returns(new ConnectionProperties { ConnectionName = "selected-tenant", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "foreign" });
            f.Source.SetupGet(source => source.Dataconnection).Returns(connection.Object); return Task.CompletedTask;
        }
        if (during) f.AfterEligibilityRead = Change; else await Change();
        await Assert.ThrowsAsync<InvalidOperationException>(() => Eligibility(f).GetMaximumLevelAsync("A"));
        Assert.Empty(f.Queries); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData(false, "missing")] [InlineData(false, "null")] [InlineData(true, "missing")] [InlineData(true, "null")]
    [InlineData(false, "zero")] [InlineData(true, "zero")]
    public async Task Dictionary_and_DataRow_evidence_preserves_explicit_zero_and_refuses_absent_or_null_observation_keys(bool dataRow, string observation)
    {
        using var f = await Ready(); var component = Component(f);
        f.Source.Setup(source => source.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()))
            .ReturnsAsync((string table, List<TheTechIdea.Beep.Report.AppFilter> filters) => {
                if (table != "ENT_COMPONENT") return (IEnumerable<object>)f.Rows.Where(row => row.GetType().Name == table && filters.All(filter =>
                    Convert.ToString(row.GetType().GetProperty(filter.FieldName)!.GetValue(row)) == filter.FilterValue)).ToArray();
                var values = typeof(ENT_COMPONENT).GetProperties().Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                    .ToDictionary(property => property.Name, property => property.GetValue(component)!);
                if (observation == "missing") values.Remove("COMPONENT_OBS_NO");
                else values["COMPONENT_OBS_NO"] = observation == "null" ? DBNull.Value : 0m;
                if (!dataRow) return (IEnumerable<object>)[values];
                var rows = new DataTable(); foreach (var pair in values) rows.Columns.Add(pair.Key, typeof(object));
                rows.Rows.Add(values.Values.Select(value => value ?? DBNull.Value).ToArray()); return rows.Rows.Cast<object>().ToArray();
            });
        if (observation == "zero") Assert.Equal("DELETE", await Eligibility(f).GetMaximumLevelAsync("A"));
        else await Assert.ThrowsAsync<InvalidOperationException>(() => Eligibility(f).GetMaximumLevelAsync("A"));
        Assert.Empty(f.Writes);
    }
}
