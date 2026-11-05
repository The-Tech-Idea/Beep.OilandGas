using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Middleware;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class RepositoryTenantBindingTests
{
    [Fact]
    public async Task Identical_modules_in_different_tenants_resolve_only_their_owned_connection()
    {
        using var f = new Fixture();
        await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        await f.Bindings.BindAsync("B", "PRODUCTION", new("db-b", null), "user");
        Assert.Equal("db-a", await f.Resolver("A").ResolveAsync("production"));
        Assert.Equal("db-b", await f.Resolver("B").ResolveAsync("PRODUCTION"));
        Assert.Single(await f.Bindings.ListAsync("A"));
        Assert.Single(await f.Bindings.ListAsync("B"));
        f.Editor.Verify(x => x.GetDataSource(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Unowned_historical_binding_is_preserved_but_never_used_as_default()
    {
        using var f = new Fixture();
        f.Db.ModuleDatabases.Add(new() { ModuleId = "PRODUCTION", ConnectionName = "db-a" });
        await f.Db.SaveChangesAsync();
        Assert.Single(await f.Bindings.UnownedAsync());
        Assert.Null(await f.Resolver("A").FindAsync("PRODUCTION"));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() =>
            f.Resolver("A").ResolveAsync("PRODUCTION"))).Kind);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() =>
            f.Resolver().ResolveAsync("PRODUCTION"))).Kind);
    }

    [Fact]
    public async Task Explicit_ownership_assignment_records_evidence_and_cannot_transfer_tenant()
    {
        using var f = new Fixture();
        var unowned = new ModuleDatabaseBinding { ModuleId = "PRODUCTION", ConnectionName = "db-a" };
        f.Db.Add(unowned);
        await f.Db.SaveChangesAsync();
        var oldStamp = unowned.ConcurrencyStamp;
        var owned = await f.Bindings.AssignOwnershipAsync(unowned.BindingId, new("A", oldStamp, "Reviewed deployment manifest"), "user");
        Assert.Equal("A", owned.TenantId);
        Assert.Equal("user", owned.OwnershipAssignedByUserId);
        Assert.Equal("Reviewed deployment manifest", owned.OwnershipEvidence);
        Assert.NotEqual(oldStamp, owned.ConcurrencyStamp);
        Assert.Empty(await f.Bindings.UnownedAsync());
        Assert.Equal("db-a", await f.Resolver("A").ResolveAsync("PRODUCTION"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.AssignOwnershipAsync(owned.BindingId,
            new("B", owned.ConcurrencyStamp, "Other evidence"), "user"));
        Assert.Null(await f.Resolver("B").FindAsync("PRODUCTION"));
    }

    [Fact]
    public async Task Binding_requires_current_version_and_ownership_requires_evidence()
    {
        using var f = new Fixture();
        var binding = await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.BindAsync("A", "PRODUCTION", new("db-b", "stale"), "user"));
        f.Db.ChangeTracker.Clear();
        Assert.Equal("db-a", await f.Resolver("A").ResolveAsync("PRODUCTION"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.AssignOwnershipAsync(binding.BindingId,
            new("A", binding.ConcurrencyStamp, ""), "user"));
    }

    [Fact]
    public async Task Connection_name_cannot_be_shared_by_independent_tenants_but_can_serve_several_owned_modules()
    {
        using var f = new Fixture();
        await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        await f.Bindings.BindAsync("A", "PPDM_CORE", new("DB-A", null), "user");
        Assert.Single(await f.Db.Set<AppModuleConnection>().ToArrayAsync());
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.BindAsync("B", "PRODUCTION", new("Db-A", null), "user"));
        Assert.Empty(await f.Bindings.ListAsync("B"));
        Assert.Equal(2, (await f.Bindings.ListAsync("A")).Length);
    }

    [Fact]
    public async Task Database_foreign_key_rejects_cross_tenant_connection_owner()
    {
        using var f = new Fixture();
        await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        f.Db.ModuleDatabases.Add(new() { TenantId = "B", ModuleId = "PPDM_CORE", ConnectionName = "db-a", ConnectionKey = "DB-A" });
        await Assert.ThrowsAsync<DbUpdateException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Database_connection_key_and_tenant_module_uniqueness_arbitrate_duplicate_claims()
    {
        using var f = new Fixture();
        await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        f.Db.ChangeTracker.Clear();
        f.Db.Add(new AppModuleConnection { TenantId = "B", ConnectionKey = "DB-A", AssignedByUserId = "user" });
        await Assert.ThrowsAsync<DbUpdateException>(() => f.Db.SaveChangesAsync());
        f.Db.ChangeTracker.Clear();
        f.Db.ModuleDatabases.Add(new() { TenantId = "A", ModuleId = "PRODUCTION", ConnectionName = "db-a", ConnectionKey = "DB-A" });
        await Assert.ThrowsAsync<DbUpdateException>(() => f.Db.SaveChangesAsync());
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("disabled")]
    public async Task Missing_or_disabled_tenant_cannot_bind_or_resolve(string state)
    {
        using var f = new Fixture();
        if (state == "disabled")
        { (await f.Db.Set<AppTenant>().FindAsync("A"))!.IsActive = false; await f.Db.SaveChangesAsync(); }
        var tenantId = state == "missing" ? "missing" : "A";
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.BindAsync(tenantId, "PRODUCTION", new("db-a", null), "user"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Resolver(tenantId).ResolveAsync("PRODUCTION"));
    }

    [Fact]
    public async Task Null_owner_key_and_missing_configured_connection_cannot_route()
    {
        using var f = new Fixture();
        f.Db.ModuleDatabases.Add(new() { TenantId = "A", ModuleId = "PRODUCTION", ConnectionName = "db-a" });
        await f.Db.SaveChangesAsync();
        Assert.Null(await f.Resolver("A").FindAsync("PRODUCTION"));
        f.Db.ChangeTracker.Clear();
        var binding = await f.Db.ModuleDatabases.SingleAsync();
        f.Db.ModuleDatabases.Remove(binding);
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Bindings.BindAsync("A", "PRODUCTION", new("not-configured", null), "user"));
        Assert.Null(await f.Resolver("A").FindAsync("PRODUCTION"));
    }

    [Fact]
    public async Task Migration_fingerprint_is_tenant_scoped_and_changes_when_binding_changes()
    {
        using var f = new Fixture();
        var binding = await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", null), "user");
        await f.Bindings.BindAsync("B", "PRODUCTION", new("db-b", null), "user");
        var a = f.Resolver("A");
        var before = await a.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "db-a");
        Assert.NotEqual(before, await f.Resolver("B").GetMigrationBindingFingerprintAsync(["PRODUCTION"], "db-b"));
        await Assert.ThrowsAsync<RefusalException>(() => a.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "db-b"));
        await f.Bindings.BindAsync("A", "PRODUCTION", new("db-a", binding.ConcurrencyStamp), "user");
        Assert.NotEqual(before, await a.GetMigrationBindingFingerprintAsync(["PRODUCTION"], "db-a"));
    }

    [Theory]
    [InlineData("/api/field/current/accounting", null, null, 400)]
    [InlineData("/api/field/current/accounting", "B", "operator", 403)]
    [InlineData("/api/field/current/accounting", "A", "operator", 200)]
    [InlineData("/api/auth/repository/me", null, null, 200)]
    public async Task Request_scope_requires_authoritative_membership(string path, string? tenantId, string? companyId, int status)
    {
        using var f = new Fixture();
        var http = f.Http(path, tenantId, companyId);
        var selection = new RepositoryTenantContext();
        var reached = false;
        var middleware = new CompanyRequestMiddleware(context => { reached = true; return Task.CompletedTask; },
            principal => principal.FindFirst("test-resolved-account")?.Value);
        await CompanyRequestTestInvoker.InvokeAsync(middleware, http, selection, new(f.Access), f.Access, f.Bindings);
        Assert.Equal(status, http.Response.StatusCode);
        Assert.Equal(status == 200, reached);
        if (companyId is not null && status == 200)
        {
            Assert.Equal("A", selection.TenantId);
            Assert.True(http.User.IsInRole("Engineer"));
            Assert.False(http.User.IsInRole("Administrator"));
            Assert.False(http.User.IsInRole("Forged"));
            Assert.DoesNotContain(http.User.Claims, x => x.Type == "permission" && x.Value == "setup.manage");
        }
    }

    [Fact]
    public async Task Multiple_company_headers_are_refused_and_request_scope_cannot_change_tenant()
    {
        using var f = new Fixture();
        var http = f.Http("/api/fields/1", "A", "operator");
        http.Request.Headers[CompanyRequestHeaders.Tenant] = new[] { "A", "B" };
        var middleware = new CompanyRequestMiddleware(_ => throw new InvalidOperationException("Must not reach domain"), _ => "user");
        var selection = new RepositoryTenantContext();
        await CompanyRequestTestInvoker.InvokeAsync(middleware, http, selection, new(f.Access), f.Access, f.Bindings);
        Assert.Equal(400, http.Response.StatusCode);
        Assert.Null(selection.TenantId);
        selection.SelectTenant("A");
        Assert.Throws<InvalidOperationException>(() => selection.SelectTenant("B"));
    }

    [Fact]
    public async Task Repository_setup_tenant_selection_preserves_global_roles_and_checks_current_authority()
    {
        using var f = new Fixture();
        var http = f.Http("/api/setup/modules", "B", null);
        var selection = new RepositoryTenantContext();
        var reached = false;
        var middleware = new CompanyRequestMiddleware(_ => { reached = true; return Task.CompletedTask; }, _ => "user");
        await CompanyRequestTestInvoker.InvokeAsync(middleware, http, selection, new(f.Access), f.Access, f.Bindings);
        Assert.True(reached);
        Assert.Equal("B", selection.TenantId);
        Assert.True(http.User.IsInRole("Administrator"));
        f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync(x => x.RoleId == "admin"));
        await f.Db.SaveChangesAsync();
        reached = false;
        http = f.Http("/api/setup/modules", "B", null);
        await CompanyRequestTestInvoker.InvokeAsync(middleware, http, new(), new(f.Access), f.Access, f.Bindings);
        Assert.False(reached);
        Assert.Equal(403, http.Response.StatusCode);
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:;Foreign Keys=True");
        private readonly ServiceProvider services = new ServiceCollection().AddLogging().BuildServiceProvider();
        public RepositoryDbContext Db { get; }
        public RepositoryAccessService Access { get; }
        public ModuleBindingService Bindings { get; }
        public Mock<IDMEEditor> Editor { get; } = new(MockBehavior.Strict);
        public Fixture()
        {
            connection.Open();
            Db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
            Db.Database.EnsureCreated();
            Db.AddRange(new AppTenant { TenantId = "A", Name = "A" }, new AppTenant { TenantId = "B", Name = "B" });
            Db.Users.Add(new OilGasUser { Id = "user", UserName = "user" });
            Db.Roles.AddRange(new IdentityRole { Id = "admin", Name = "Administrator" }, new IdentityRole { Id = "engineer", Name = "Engineer" });
            Db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "user", RoleId = "admin" },
                new IdentityUserRole<string> { UserId = "user", RoleId = "engineer" });
            Db.Add(new AppCompany { TenantId = "A", BusinessAssociateId = "operator" });
            Db.Add(new AppCompanyMembership { TenantId = "A", BusinessAssociateId = "operator", UserId = "user",
                GrantedByUserId = "user", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
            Db.Add(new AppUserRoleExtension { TenantId = "A", BusinessAssociateId = "operator", UserId = "user",
                RoleId = "engineer", EffectiveFromUtc = DateTime.UtcNow.AddDays(-1) });
            Db.SaveChanges();
            Access = new(Db);
            var config = new Mock<IConfigEditor>();
            config.SetupGet(x => x.DataConnections).Returns([new ConnectionProperties { ConnectionName = "db-a", Database = "db-a", Host = "server", DatabaseType = TheTechIdea.Beep.Utilities.DataSourceType.SqlServer },
                new ConnectionProperties { ConnectionName = "db-b", Database = "db-b", Host = "server", DatabaseType = TheTechIdea.Beep.Utilities.DataSourceType.SqlServer }]);
            Editor.SetupGet(x => x.ConfigEditor).Returns(config.Object);
            Bindings = new(Db, Editor.Object);
        }
        public ModuleConnectionResolver Resolver(string? tenantId = null)
        {
            var context = new RepositoryTenantContext();
            if (tenantId is not null) context.SelectTenant(tenantId);
            return new(Db, Editor.Object, context);
        }
        public DefaultHttpContext Http(string path, string? tenantId, string? company)
        {
            var http = new DefaultHttpContext { RequestServices = services,
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("test-resolved-account", "user"),
                    new Claim(ClaimTypes.Role, "Administrator"), new Claim("permission", "setup.manage")], "test")) };
            http.User.AddIdentity(new ClaimsIdentity([new Claim("roles", "Forged"), new Claim(ClaimTypes.Role, "Forged")], "external"));
            http.Request.Path = path;
            http.Response.Body = new MemoryStream();
            if (tenantId is not null) http.Request.Headers[CompanyRequestHeaders.Tenant] = tenantId;
            if (company is not null) http.Request.Headers[CompanyRequestHeaders.Company] = company;
            return http;
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); services.Dispose(); }
    }
}
