using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Repository;
using Beep.OilandGas.UserManagement.Models.Requests.UserManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Beep.Report;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyRowSecurityTests
{
    private static CompanyRowLevelSecurityService Security(Fixture f)
    {
        // Install the request in the calling test's execution context after the async fixture factory returns.
        f.Http.HttpContext = f.Request;
        return new(f.Account, f.Gate, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Policy, f.Metadata.Object, f.Service);
    }
    private static RowLevelSecurityController Controller(Fixture f) => new(Security(f), f.Account)
        { ControllerContext = new() { HttpContext = f.Request } };

    [Fact]
    public async Task No_grants_deny_all_rows_even_with_incoming_wildcard_and_administrator_claims()
    {
        using var f = await Fixture.Create();
        ((ClaimsIdentity)f.Request.User.Identity!).AddClaims([new("field_scope", "*"), new(ClaimTypes.Role, "Administrator")]);
        var service = Security(f);
        Assert.Empty(await service.GetUserAccessibleFieldsAsync("actor")); Assert.Empty(await service.GetUserAccessibleAssetsAsync("actor"));
        var plan = await service.ApplyRowFiltersAsync("actor", new("FIELD")); Assert.True(plan.DenyAll); Assert.Empty(plan.ReadConjunctions);
        Assert.Empty(f.Queries); Assert.Empty(f.Connections);
    }

    [Fact]
    public async Task Scope_queries_use_exact_current_company_grants_and_clear_on_company_switch()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); await f.Grant("FIELD", "a"); await f.Grant("WELL", "well");
        Assert.Equal(new[] { "A", "a" }, await Security(f).GetUserAccessibleFieldsAsync("actor"));
        Assert.Equal(new[] { "A", "a", "well" }, await Security(f).GetUserAccessibleAssetsAsync("actor"));
        Assert.Equal(new[] { "company" }, await Security(f).GetUserAccessibleOrganizationsAsync("actor"));
        Assert.NotEmpty(f.EligibilityQueries); Assert.Empty(f.Queries); f.Connections.Clear();
        await f.Select("other-company"); Assert.Empty(await Security(f).GetUserAccessibleFieldsAsync("actor"));
        Assert.Equal(new[] { "other-company" }, await Security(f).GetUserAccessibleOrganizationsAsync("actor"));
        Assert.Empty(f.Connections);
    }

    [Theory]
    [InlineData("fields")]
    [InlineData("assets")]
    [InlineData("organizations")]
    [InlineData("filters")]
    [InlineData("row")]
    [InlineData("source")]
    [InlineData("database")]
    [InlineData("datasource")]
    public async Task Every_service_operation_independently_refuses_a_subject_other_than_the_resolved_actor(string operation)
    {
        using var f = await Fixture.Create(); var service = Security(f);
        await Forbidden(() => operation switch
        {
            "fields" => service.GetUserAccessibleFieldsAsync("other"), "assets" => service.GetUserAccessibleAssetsAsync("other"),
            "organizations" => service.GetUserAccessibleOrganizationsAsync("other"), "filters" => service.ApplyRowFiltersAsync("other", new("FIELD")),
            "row" => service.CheckRowAccessAsync("other", new("FIELD", "{}")), "source" => service.CheckSourceAccessAsync("other", new("selected-tenant")),
            "database" => service.CheckDatabaseAccessAsync("other", new(new string('A', 64))), _ => service.CheckDataSourceAccessAsync("other", new("selected-tenant"))
        });
        Assert.Empty(f.Connections); Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("membership")]
    [InlineData("role")]
    public async Task Stale_accounts_and_company_grants_refuse_scope_reads(string reason)
    {
        using var f = await Fixture.Create(); var service = Security(f); await f.Grant("FIELD", "A");
        if (reason == "inactive") (await f.Db.Users.FindAsync("actor"))!.IsActive = false;
        if (reason == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
        if (reason == "role") f.Db.UserRoles.Remove((await f.Db.UserRoles.SingleAsync()));
        await f.Db.SaveChangesAsync();
        await Forbidden(() => service.GetUserAccessibleFieldsAsync("actor")); Assert.Empty(f.Connections);
    }

    [Fact]
    public async Task Captured_binding_changes_are_refused_instead_of_returning_another_target()
    {
        using var f = await Fixture.Create(); var service = Security(f);
        await service.GetUserAccessibleFieldsAsync("actor");
        (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed"; await f.Db.SaveChangesAsync();
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => service.GetUserAccessibleFieldsAsync("actor"))).Kind);
    }

    [Fact]
    public async Task A_supplied_field_owner_cannot_make_an_actual_foreign_well_readable()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); f.Rows.Add(new WELL { UWI = "foreign", ASSIGNED_FIELD = "B", OPERATOR = "company" });
        var check = await Security(f).CheckRowAccessAsync("actor", new("WELL", "{\"UWI\":\"foreign\",\"ASSIGNED_FIELD\":\"A\"}"));
        Assert.False(check.HasAccess); var query = Assert.Single(f.Queries);
        Assert.Contains(query, filter => filter.FieldName == "UWI" && filter.FilterValue == "foreign");
        Assert.Contains(query, filter => filter.FieldName == "ASSIGNED_FIELD" && filter.FilterValue == "A"); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Row_checks_use_actual_stored_rows_and_complete_composite_keys()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(new FACILITY { FACILITY_ID = "plant", FACILITY_TYPE = "gas", PRIMARY_FIELD_ID = "A", CURRENT_OPERATOR = "company" });
        Assert.True((await Security(f).CheckRowAccessAsync("actor", new("FACILITY", "{\"FACILITY_ID\":\"plant\",\"FACILITY_TYPE\":\"gas\"}"))).HasAccess);
        Assert.False((await Security(f).CheckRowAccessAsync("actor", new("FACILITY", "{\"FACILITY_ID\":\"plant\",\"FACILITY_TYPE\":\"oil\"}"))).HasAccess);
        var query=f.Queries[0];Assert.Equal(3,query.Count);
        Assert.Contains(query,filter=>filter.FieldName=="FACILITY_ID" && filter.FilterValue=="plant");
        Assert.Contains(query,filter=>filter.FieldName=="FACILITY_TYPE" && filter.FilterValue=="gas");
        Assert.Contains(query,filter=>filter.FieldName=="PRIMARY_FIELD_ID" && filter.FilterValue=="A");
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("[]")]
    [InlineData("{}")]
    [InlineData("{\"FIELD_ID\":1}")]
    [InlineData("{\"FIELD_ID\":\"\"}")]
    [InlineData("{\"FIELD_ID\":\"A\",\"FIELD_ID\":\"B\"}")]
    public async Task Row_checks_require_an_unambiguous_complete_JSON_key(string? json)
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A");
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => Security(f).CheckRowAccessAsync("actor", new("FIELD", json)))).Kind);
        Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Diagnostic_row_checks_retain_provider_equality_verification()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true);
        f.Rows.Add(new WELL { UWI = "different", ASSIGNED_FIELD = "A", OPERATOR = "company" }); f.IgnoreFilters = true;
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Security(f).CheckRowAccessAsync("actor", new("WELL", "{\"UWI\":\"requested\"}")))).Kind);
    }

    [Fact]
    public async Task Filter_diagnostics_return_SDK_predicates_and_copy_narrowing_filters_without_SQL()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A", inherit: true); await f.Grant("WELL", "well");
        var filter = new AppFilter { FieldName = "UWI", Operator = "=", FilterValue = "well" };
        var result = await Security(f).ApplyRowFiltersAsync("actor", new("WELL", [filter]));
        Assert.False(result.DenyAll); Assert.Equal(2, result.FilterCount);
        Assert.Contains(result.ReadConjunctions.SelectMany(group => group), value => value.FieldName == "UWI" && value.FilterValue == "well");
        Assert.Contains(result.ReadConjunctions.SelectMany(group => group), value => value.FieldName == "ASSIGNED_FIELD" && value.FilterValue == "A");
        filter.FilterValue = "changed"; Assert.Equal("well", Assert.Single(result.Filters).FilterValue);
        Assert.DoesNotContain("FilterExpressions", JsonSerializer.Serialize(result)); Assert.Empty(f.Queries);
    }

    [Theory]
    [InlineData("FIELD_ID OR 1=1", "=")]
    [InlineData("FIELD_ID", "IN")]
    [InlineData("UNKNOWN", "=")]
    public async Task Filter_diagnostics_use_the_same_column_operator_validation_as_actual_repository_reads(string column, string operation)
    {
        using var f = await Fixture.Create();
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => Security(f).ApplyRowFiltersAsync("actor", new("FIELD", [new() { FieldName = column, Operator = operation, FilterValue = "A" }])))).Kind);
        Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Unreviewed_table_ownership_cannot_supply_filter_or_row_approval()
    {
        using var f = await Fixture.Create();
        await Forbidden(() => Security(f).ApplyRowFiltersAsync("actor", new("BUSINESS_ASSOCIATE")));
        await Forbidden(() => Security(f).CheckRowAccessAsync("actor", new("BUSINESS_ASSOCIATE", "{}"))); Assert.Empty(f.Queries);
    }

    [Fact]
    public async Task Target_checks_match_only_the_selected_reviewed_binding_without_granting_rows()
    {
        using var f = await Fixture.Create(); var service = Security(f); var scope = await f.Gate.ResolveAsync();
        Assert.True((await service.CheckSourceAccessAsync("actor", new("SELECTED-TENANT"))).HasAccess);
        Assert.True((await service.CheckDataSourceAccessAsync("actor", new(scope.ConnectionName))).HasAccess);
        Assert.True((await service.CheckDatabaseAccessAsync("actor", new(scope.Fingerprint))).HasAccess);
        Assert.False((await service.CheckSourceAccessAsync("actor", new("*"))).HasAccess);
        Assert.False((await service.CheckDataSourceAccessAsync("actor", new("other"))).HasAccess);
        Assert.False((await service.CheckDatabaseAccessAsync("actor", new(scope.Fingerprint.ToLowerInvariant()))).HasAccess);
        Assert.Empty(await service.GetUserAccessibleFieldsAsync("actor")); Assert.Empty(f.Connections);
    }

    [Fact]
    public async Task Controller_scope_queries_ignore_subject_query_strings_and_use_fresh_actor_authority()
    {
        using var f = await Fixture.Create(); await f.Grant("FIELD", "A"); f.Request.Request.QueryString = new("?userId=other");
        Assert.Equal(new[] { "A" }, Assert.IsType<string[]>(Assert.IsType<OkObjectResult>(await Controller(f).GetAccessibleFields()).Value));
        (await f.Db.Users.FindAsync("actor"))!.IsActive = false; await f.Db.SaveChangesAsync();
        await Forbidden(() => Controller(f).GetAccessibleFields());
    }

    [Theory]
    [InlineData("/api/identity/security")]
    [InlineData("/API/IDENTITY/SECURITY/check-row-access")]
    public void Row_security_routes_require_company_selection_in_the_shared_server_and_client_policy(string route)
    {
        Assert.False(CompanyRequestPolicy.IsControlPlane(route)); Assert.True(CompanyRequestPolicy.RequiresCompany(route));
        Assert.True(CompanyRequestPolicy.IsControlPlane("/api/identity/users")); Assert.True(CompanyRequestPolicy.IsControlPlane("/api/identity/security-catalog"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Actual_company_middleware_refuses_unselected_security_routes_and_installs_current_scope(bool selected)
    {
        using var f = await Fixture.Create(); var reached = false;
        f.Request.Request.Path = "/api/identity/security/accessible-fields";
        ((ClaimsIdentity)f.Request.User.Identity!).AddClaims([new("field_scope", "*"), new(ClaimTypes.Role, "Forged")]);
        if (selected)
        {
            f.Request.Request.Headers[CompanyRequestHeaders.Tenant] = f.Gate.Company.Scope.TenantId;
            f.Request.Request.Headers[CompanyRequestHeaders.Company] = f.Gate.Company.Scope.BusinessAssociateId;
        }
        var context = new RepositoryTenantContext(); var access = new RepositoryAccessService(f.Db);
        var middleware = new CompanyRequestMiddleware(_ => { reached = true; return Task.CompletedTask; }, principal => principal.FindFirst("party_id")?.Value);
        await CompanyRequestTestInvoker.InvokeAsync(middleware, f.Request, context, new(access), access, new(f.Db, f.Editor.Object));
        Assert.Equal(selected, reached);
        if (!selected) { Assert.Equal(400, f.Request.Response.StatusCode); Assert.Null(context.Company); }
        else
        {
            Assert.Equal(f.Gate.Company.Scope, context.Company!.Scope);
            Assert.False(f.Request.User.HasClaim(claim => claim.Type == "field_scope"));
            Assert.False(f.Request.User.IsInRole("Forged")); Assert.True(f.Request.User.IsInRole("Administrator"));
        }
        Assert.Empty(f.Connections);
    }

    [Fact]
    public void Actual_client_capture_requires_company_headers_for_security_diagnostics()
    {
        using var context = new CompanyClientContext();
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/identity/security/accessible-fields");
        Assert.Throws<InvalidOperationException>(() => context.Prepare(request));
        var scope = new CompanyScope("tenant", "operator");
        Assert.True(context.TrySelect(context.Clear(), "actor", new(scope, [], [])));
        context.Prepare(request);
        Assert.Equal(scope.TenantId, Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Tenant)));
        Assert.Equal(scope.BusinessAssociateId, Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));
    }

    private static async Task Forbidden(Func<Task> action) => Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(action)).Kind);
}
