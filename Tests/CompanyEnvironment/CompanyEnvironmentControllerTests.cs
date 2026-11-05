using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.HSE.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Beep.OilandGas.ApiService.Services;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyEnvironmentControllerTests
{
    private sealed class Store : ICompanyEnvironmentStore
    {
        public CompanyEnvironmentConfiguration? Value { get; set; }
        public async Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return Value; }
        public async Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority)
        { await authority(); return CompanyEnvironmentIdentity.Prepare(company, request.Change, "build", Value, new string('A', 64), null); }
    }
    private sealed class Bindings : ICompanyEnvironmentBindingReader
    { public Task<string?> ResolveAsync(string module, CancellationToken token) => Task.FromResult<string?>(new string('A', 64)); }
    private sealed class Selections : ICompanyGhgSelectionStore
    {
        public async Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority) { await authority(); return null; }
        public async Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority)
        { await authority(); return CompanyGhgSelectionIdentity.Prepare(company, actor, request, null); }
    }
    private sealed class WellReader : ICompanyEnvironmentWellReader
    {
        public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(CompanyScope company, string wellId, IReadOnlyDictionary<string, string> filters, Func<Task> authority, CancellationToken token)
        { await authority(); return [new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["UWI"] = wellId, ["WELL_NAME"] = "North well" }]; }
    }
    private sealed class Defaults : ISystemSetupRecordReader
    {
        private readonly SystemSetupConfiguration baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", "8b8e47b307d04ab2b220b7af684ce620"), null);
        public string Revision => baseline.Revision;
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { await authority(); return category == "TOOLS" && key == "DEFAULT" ? baseline : null; }
    }
    private sealed class FactorRecords : ISystemSetupRecordReader
    {
        private readonly Defaults defaults = new();
        private readonly SystemSetupConfiguration catalog = SystemSetupIdentity.Prepare(new("GHG_FACTORS", "KW", "1",
            System.Text.Json.JsonSerializer.Serialize(new GhgFactorCatalog("KW", [new("test", "flare", "volume", "mass", 1m, new(2026, 1, 1), new(2026, 12, 31), "source")])), "", Guid.NewGuid().ToString("N")), null);
        public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
        { if (category == "GHG_FACTORS") { await authority(); return catalog; } return await defaults.ReadAsync(category, key, authority); }
    }
    private sealed class Access : IRepositoryAccessService
    {
        public RepositoryCompanyAccess? Value { get; set; }
        public int Calls { get; private set; }
        public bool RevokeAfterFirstCheck { get; set; }
        public Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string userId, CompanyScope scope, CancellationToken token = default)
        {
            Calls++;
            return Task.FromResult(RevokeAfterFirstCheck && Calls >= 3 ? null : Value);
        }
    }

    [Theory]
    [InlineData("allowed")]
    [InlineData("revoked")]
    [InlineData("different-company")]
    [InlineData("different-actor")]
    [InlineData("role-revoked")]
    public async Task PreviewRechecksCompanyMembershipAndAdministratorRole(string mode)
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access);
        var selected = await resolver.ResolveAsync("actor", company);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(selected!);
        if (mode == "revoked") access.Value = null;
        if (mode == "different-company") access.Value = new(new("tenant", "other"), ["Administrator"], []);
        if (mode == "role-revoked") access.Value = new(company, ["Operator"], []);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", mode == "different-actor" ? "other" : "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var controller = new CompanyEnvironmentController(tenant, resolver, new Store(), NullLogger<CompanyEnvironmentController>.Instance) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var result = await controller.Preview(new("KW", "{}"), new CompanyEnvironmentEffectiveResolver(new Store(), new Defaults()), default);
        if (mode == "allowed") Assert.Equal(company, Assert.IsType<CompanyEnvironmentDraftReview>(Assert.IsType<OkObjectResult>(result).Value).Settings.Company);
        else Assert.IsType<ForbidResult>(result);
        Assert.Equal("Administrator", Assert.Single(typeof(CompanyEnvironmentController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>()).Roles);
    }

    [Fact]
    public async Task RevocationDuringPreviewRefusesTheResult()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []), RevokeAfterFirstCheck = true };
        var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var controller = new CompanyEnvironmentController(tenant, resolver, new Store(), NullLogger<CompanyEnvironmentController>.Instance) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        Assert.IsType<ForbidResult>(await controller.Preview(new("KW", "{}"), new CompanyEnvironmentEffectiveResolver(new Store(), new Defaults()), default));
    }

    [Fact]
    public async Task SavedEnvironmentEndpointsUseOnlyTheAuthorizedCompany()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var store = new Store();
        var controller = new CompanyEnvironmentController(tenant, resolver, store, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var request = new CompanyEnvironmentSaveRequest("KW", "{}", "", Guid.NewGuid().ToString("N"));
        var draft = new CompanyEnvironmentPreviewRequest(request.CountryCode, request.OverridesJson);
        var review = await new CompanyEnvironmentEffectiveResolver(store, new Defaults()).PreviewDraftAsync(company, draft, () => Task.CompletedTask);
        var command = new CompanyEnvironmentReviewedSaveRequest(request, review);
        var result = Assert.IsType<CompanyEnvironmentConfiguration>(Assert.IsType<OkObjectResult>(await controller.Save(command, default)).Value);
        Assert.Equal(company, result.Company); store.Value = result;
        var read = Assert.IsType<CompanyEnvironmentReadResult>(Assert.IsType<OkObjectResult>(await controller.Read(default)).Value);
        Assert.Equal(result, read.Configuration);
        access.Value = null;
        Assert.IsType<ForbidResult>(await controller.Read(default));
        Assert.IsType<ForbidResult>(await controller.Save(command, default));
    }

    [Fact]
    public async Task VerificationEndpointChecksCurrentAdministratorAndCompany()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, new Defaults().Revision, null);
        var store = new Store { Value = saved }; var runtime = new CompanyEnvironmentRuntime(new CompanyEnvironmentEffectiveResolver(store, new Defaults()));
        var controller = new CompanyEnvironmentController(tenant, resolver, store, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var result = Assert.IsType<CompanyEnvironmentVerification>(Assert.IsType<OkObjectResult>(await controller.Verify(runtime, default)).Value);
        Assert.Equal(company, result.Company); Assert.Equal(saved.Revision, result.Revision); Assert.False(result.SourcesVerified);
        access.Value = null;
        Assert.IsType<ForbidResult>(await controller.Verify(runtime, default));
    }

    [Fact]
    public async Task SourceReviewRequiresCurrentCompanyAdministration()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}";
        var defaults = new Defaults();
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, defaults.Revision, null);
        var store = new Store { Value = saved }; var sources = new CompanyEnvironmentSources(new CompanyEnvironmentEffectiveResolver(store, defaults), new Bindings());
        var controller = new CompanyEnvironmentController(tenant, resolver, store, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var result = Assert.IsType<CompanyEnvironmentSourceReview>(Assert.IsType<OkObjectResult>(await controller.Sources(sources, default)).Value);
        Assert.Equal("Bound", Assert.Single(result.Sources).BindingStatus); Assert.False(result.DataTested);
        access.Value = null;
        Assert.IsType<ForbidResult>(await controller.Sources(sources, default));
    }

    [Fact]
    public async Task FactorCatalogReviewRequiresCurrentCompanyAdministration()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) }; var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", "{}", "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, new Defaults().Revision, null);
        var store = new Store { Value = saved }; var records = new FactorRecords();
        var reader = new CompanyGhgCatalogReader(new(store, records), records);
        var controller = new CompanyEnvironmentController(tenant, resolver, store, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var result = Assert.IsType<CompanyGhgCatalogReview>(Assert.IsType<OkObjectResult>(await controller.GhgCatalog("KW", reader, default)).Value);
        Assert.Equal(company, result.Company); Assert.Equal(saved.Revision, result.CompanyRevision);
        access.Value = null;
        Assert.IsType<ForbidResult>(await controller.GhgCatalog("KW", reader, default));
    }

    [Fact]
    public async Task FactorSelectionRequiresCurrentCompanyAdministration()
    {
        var company = new CompanyScope("tenant", "company"); var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access); var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var controller = new CompanyEnvironmentController(tenant, resolver, new Store(), NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var request = new CompanyGhgSelectionRequest("KW", new string('A', 64), new string('B', 64), "reviewed", "", Guid.NewGuid().ToString("N"));
        var saved = Assert.IsType<CompanyGhgSelection>(Assert.IsType<OkObjectResult>(await controller.SaveGhgSelection(request, new Selections(), default)).Value);
        Assert.Equal(company, saved.Company); Assert.Equal("actor", saved.SelectedBy);
        access.Value = null;
        var records = new FactorRecords();
        Assert.IsType<ForbidResult>(await controller.GhgSelection(new CompanyGhgSelectionStatusReader(new Selections(), new(new(new Store(), records), records)), default));
        Assert.IsType<ForbidResult>(await controller.SaveGhgSelection(request, new Selections(), default));
    }

    [Fact]
    public async Task WellTestEndpointRechecksCompanyAdministrator()
    {
        var company = new CompanyScope("tenant", "company");
        var access = new Access { Value = new(company, ["Administrator"], []) };
        var resolver = new RepositoryCompanyContextResolver(access);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await resolver.ResolveAsync("actor", company))!);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new("party_id", "actor"), new(ClaimTypes.Role, "Administrator")], "test"));
        var json = "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}";
        var defaults = new Defaults();
        var saved = CompanyEnvironmentIdentity.Prepare(company, new("KW", json, "", Guid.NewGuid().ToString("N")), CompanyEnvironmentPreviewer.ToolsVersion, null, defaults.Revision, null);
        var store = new Store { Value = saved }; var effective = new CompanyEnvironmentEffectiveResolver(store, defaults); var tests = new CompanyEnvironmentWellTestService(effective, new Bindings(), new WellReader());
        var controller = new CompanyEnvironmentController(tenant, resolver, store, NullLogger<CompanyEnvironmentController>.Instance)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } } };
        var review = await effective.ResolveAsync(company, () => Task.CompletedTask);
        var request = new CompanyEnvironmentWellTestRequest("wells", "well-1", saved.Revision, review.EffectiveRevision);
        var result = Assert.IsType<CompanyEnvironmentWellTest>(Assert.IsType<OkObjectResult>(await controller.TestWell(request, tests, default)).Value);
        Assert.Equal("Found", result.Status); Assert.False(result.RelatedTablesTested);
        access.Value = null;
        Assert.IsType<ForbidResult>(await controller.TestWell(request, tests, default));
    }
}
