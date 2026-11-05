using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyAssetAccessTests
{
    private static readonly CompanyScope A = new("tenant", "company-a");
    private static readonly CompanyScope B = new("tenant", "company-b");
    private static readonly CompanyScope OtherTenant = new("other-tenant", "company-a");

    [Fact]
    public async Task The_updated_API_fixture_constructs_real_company_authority_and_binding_for_the_active_store()
    {
        using var f = new Fixture();
        var gate = await Beep.OilandGas.ApiService.Tests.Infrastructure.TestCompanyModules.CreateAsync(f.Db, "actor", "fixture-company", "fixture-tenant");
        Assert.Equal(new CompanyScope("fixture-tenant", "fixture-company"), gate.Company.Scope);
        var database = await gate.ResolveAsync(); var store = new RepositoryAssetAccessStore(f.Db, gate, Mock.Of<IAssetGrantTargetVerifier>());
        Assert.True(await store.GrantAsync("member", database.Fingerprint, "FIELD", "id", "READ", false));
        Assert.Equal("fixture-company", Assert.Single(await store.ReadAsync("member", database.Fingerprint)).OrganizationId);
    }

    [Fact]
    public async Task Identical_asset_ids_in_two_companies_have_independent_grants_revoke_and_audit()
    {
        using var f = new Fixture(); await f.Select(A); var fingerprint = f.Scope.Fingerprint;
        Assert.True(await f.Store.GrantAsync("member", fingerprint, "field", "same-id", "write", true));
        var original = Assert.Single(await f.Store.ReadAsync("member", fingerprint));
        Assert.Equal("company-a", original.OrganizationId); Assert.Equal("actor", original.CreatedBy);
        await f.Select(B); Assert.Equal(fingerprint, f.Scope.Fingerprint); Assert.Empty(await f.Store.ReadAsync("member", fingerprint));
        Assert.True(await f.Store.GrantAsync("member", fingerprint, "FIELD", "same-id", "READ", false));
        var other = Assert.Single(await f.Store.ReadAsync("member", fingerprint)); Assert.NotEqual(original.Id, other.Id);
        await f.Select(A); Assert.True(await f.Store.RevokeAsync("member", fingerprint, "FIELD", "same-id"));
        Assert.Empty(await f.Store.ReadAsync("member", fingerprint));
        await f.Select(B); Assert.Equal(other.Id, Assert.Single(await f.Store.ReadAsync("member", fingerprint)).Id);
        Assert.Equal(2, await f.Db.Set<AppUserAssetAccess>().CountAsync());
        await f.Select(A); Assert.True(await f.Store.GrantAsync("member", fingerprint, "FIELD", "same-id", "DELETE", true));
        var renewed = Assert.Single(await f.Store.ReadAsync("member", fingerprint)); Assert.Equal(original.Id, renewed.Id);
        Assert.Equal(original.CreatedUtc, renewed.CreatedUtc); Assert.NotEqual(original.ConcurrencyStamp, renewed.ConcurrencyStamp);
        Assert.Equal("actor", renewed.ChangedBy); Assert.Equal("DELETE", renewed.AccessLevel);
    }

    [Fact]
    public async Task Identical_company_and_asset_ids_in_different_tenants_cannot_share_a_database_scope()
    {
        using var f = new Fixture(); await f.Select(A); var first = f.Scope.Fingerprint;
        await f.Store.GrantAsync("member", first, "FIELD", "same-id", "READ", false);
        await f.Select(OtherTenant); var second = f.Scope.Fingerprint; Assert.NotEqual(first, second);
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("member", first));
        Assert.Empty(await f.Store.ReadAsync("member", second));
        await f.Store.GrantAsync("member", second, "FIELD", "same-id", "WRITE", false);
        await f.Select(A); Assert.Equal("READ", Assert.Single(await f.Store.ReadAsync("member", first)).AccessLevel);
    }

    [Theory]
    [InlineData("company-b")]
    [InlineData("COMPANY-A")]
    [InlineData("")]
    public async Task An_explicit_organization_is_an_assertion_and_cannot_redirect_grants_or_reads(string organization)
    {
        using var f = new Fixture(); await f.Select(A);
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("member", f.Scope.Fingerprint, organization));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false, organization));
        Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Fact]
    public async Task Historical_unowned_grants_remain_hidden_until_explicit_company_assignment()
    {
        using var f = new Fixture(); await f.Select(A);
        f.Db.Add(new AppUserAssetAccess { Id = "historical", UserId = "member", DatabaseScope = f.Scope.Fingerprint, AssetType = "FIELD", AssetId = "id" }); await f.Db.SaveChangesAsync();
        Assert.Empty(await f.Store.ReadAsync("member", f.Scope.Fingerprint));
        Assert.False(await f.Store.RevokeAsync("member", f.Scope.Fingerprint, "FIELD", "id"));
        Assert.True(await f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false));
        Assert.Equal("company-a", Assert.Single(await f.Store.ReadAsync("member", f.Scope.Fingerprint)).OrganizationId);
        Assert.Null((await f.Db.Set<AppUserAssetAccess>().FindAsync("historical"))!.OrganizationId);
    }

    [Fact]
    public async Task A_stored_key_with_inconsistent_ownership_is_refused_without_modifying_another_companys_record()
    {
        using var f = new Fixture(); await f.Select(A);
        await f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false);
        var stored = await f.Db.Set<AppUserAssetAccess>().SingleAsync(); stored.OrganizationId = "company-b";
        await f.Db.SaveChangesAsync(); var stamp = stored.ConcurrencyStamp;
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "WRITE", false));
        Assert.Equal("company-b", stored.OrganizationId); Assert.Equal("READ", stored.AccessLevel); Assert.Equal(stamp, stored.ConcurrencyStamp);
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("outside-company")]
    [InlineData("expired-membership")]
    [InlineData("missing")]
    public async Task Only_current_company_members_receive_grants_or_visible_assets(string reason)
    {
        using var f = new Fixture(); await f.Select(A); var user = reason == "missing" ? "missing" : reason == "outside-company" ? "outsider" : "member";
        if (reason == "inactive") { (await f.Db.Users.FindAsync(user))!.IsActive = false; await f.Db.SaveChangesAsync(); }
        if (reason == "expired-membership") { (await f.Db.Set<AppCompanyMembership>().FindAsync("tenant", "company-a", user))!.EffectiveToUtc = DateTime.UtcNow.AddDays(-1); await f.Db.SaveChangesAsync(); }
        Assert.False(await f.Store.GrantAsync(user, f.Scope.Fingerprint, "FIELD", "id", "READ", false));
        Assert.Empty(await f.Store.ReadAsync(user, f.Scope.Fingerprint)); Assert.Empty(await f.Db.Set<AppUserAssetAccess>().ToArrayAsync());
    }

    [Fact]
    public async Task A_member_reads_only_their_own_company_access_and_cannot_grant_revoke_or_inspect_others()
    {
        using var f = new Fixture(); await f.Select(A); await f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false);
        await f.Select(A, "member"); Assert.Single(await f.Store.ReadAsync("member", f.Scope.Fingerprint));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("actor", f.Scope.Fingerprint));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.RevokeAsync("member", f.Scope.Fingerprint, "FIELD", "id"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Reader.GetRolesAsync("actor"));
    }

    [Theory]
    [InlineData("membership")]
    [InlineData("role-expiry")]
    [InlineData("role-approval")]
    [InlineData("identity-role")]
    public async Task Revoked_administrator_authority_refuses_all_access_after_initial_selection(string reason)
    {
        using var f = new Fixture(); await f.Select(A);
        if (reason == "membership") f.Db.Remove((await f.Db.Set<AppCompanyMembership>().FindAsync("tenant", "company-a", "actor"))!);
        if (reason == "role-expiry") (await f.Db.Set<AppUserRoleExtension>().SingleAsync(role => role.UserId == "actor" && role.TenantId == "tenant" && role.BusinessAssociateId == "company-a")).EffectiveToUtc = DateTime.UtcNow.AddDays(-1);
        if (reason == "role-approval") (await f.Db.Set<AppUserRoleExtension>().SingleAsync(role => role.UserId == "actor" && role.TenantId == "tenant" && role.BusinessAssociateId == "company-a")).ApprovalStatus = "Revoked";
        if (reason == "identity-role") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync(role => role.UserId == "actor"));
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("member", f.Scope.Fingerprint));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.RevokeAsync("member", f.Scope.Fingerprint, "FIELD", "id"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Reader.HasPermissionAsync("actor", "Admin.ManageUsers"));
    }

    [Fact]
    public async Task Repository_administrator_membership_cannot_replace_a_company_administrator_assignment()
    {
        using var f = new Fixture(); f.Db.RemoveRange(await f.Db.Set<AppUserRoleExtension>().Where(role => role.UserId == "actor").ToArrayAsync());
        await f.Db.SaveChangesAsync(); await f.Select(A);
        Assert.Contains("Administrator", (await f.Access.GetAccessAsync("actor"))!.Roles); Assert.Empty(await f.Reader.GetRolesAsync("actor"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false));
    }

    [Fact]
    public async Task Permission_reader_uses_only_live_selected_company_role_grants()
    {
        using var f = new Fixture(); await f.Select(A, "member");
        Assert.Equal(new[] { "Engineer" }, await f.Reader.GetRolesAsync("member"));
        Assert.True(await f.Reader.HasPermissionAsync("member", "Production.View")); Assert.False(await f.Reader.HasPermissionAsync("member", "repository-only"));
        Assert.False(await f.Reader.HasPermissionAsync("member", "production.view"));
        await f.Select(B, "member"); Assert.Empty(await f.Reader.GetRolesAsync("member")); Assert.False(await f.Reader.HasPermissionAsync("member", "Production.View"));
        await f.Select(A); Assert.Equal(new[] { "Engineer" }, await f.Reader.GetRolesAsync("member")); Assert.Empty(await f.Reader.GetRolesAsync("outsider"));
    }

    [Fact]
    public async Task Company_authorization_queries_do_not_require_an_already_provisioned_business_binding()
    {
        using var f = new Fixture(); await f.Select(A); f.Db.RemoveRange(await f.Db.ModuleDatabases.ToArrayAsync()); await f.Db.SaveChangesAsync();
        Assert.Contains("Administrator", await f.Reader.GetRolesAsync("actor")); Assert.True(await f.Reader.HasPermissionAsync("member", "Production.View"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("member", f.Scope.Fingerprint));
    }

    [Fact]
    public async Task A_changed_binding_never_relabels_existing_grants_or_accepts_a_supplied_fingerprint()
    {
        using var f = new Fixture(); await f.Select(A); var original = f.Scope.Fingerprint;
        await f.Store.GrantAsync("member", original, "FIELD", "id", "READ", false);
        (await f.Db.ModuleDatabases.SingleAsync(binding => binding.TenantId == "tenant")).ConcurrencyStamp = "rebound"; await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.ReadAsync("member", original));
        await f.Select(A); Assert.NotEqual(original, f.Scope.Fingerprint); Assert.Empty(await f.Store.ReadAsync("member", f.Scope.Fingerprint));
        await Assert.ThrowsAsync<RefusalException>(() => f.Store.RevokeAsync("member", original, "FIELD", "id"));
    }

    [Fact]
    public async Task Revocation_is_exact_and_can_remove_the_grants_of_a_former_company_member()
    {
        using var f = new Fixture(); await f.Select(A); await f.Store.GrantAsync("member", f.Scope.Fingerprint, "FIELD", "id", "READ", false);
        f.Db.Remove((await f.Db.Set<AppCompanyMembership>().FindAsync("tenant", "company-a", "member"))!); await f.Db.SaveChangesAsync();
        Assert.False(await f.Store.RevokeAsync("member", f.Scope.Fingerprint, "FIELD", "ID"));
        Assert.True(await f.Store.RevokeAsync("member", f.Scope.Fingerprint, "FIELD", "id"));
        Assert.False((await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive);
    }

    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database = new("Data Source=:memory:");
        public Context Db { get; }
        public RepositoryAccessService Access { get; }
        private readonly RepositoryCompanyContextResolver companies;
        public Mock<IDMEEditor> Editor { get; } = new();
        public RepositoryAssetAccessStore Store { get; private set; } = null!;
        public RepositoryApplicationAuthorizationReader Reader { get; private set; } = null!;
        public AssetDatabaseScope Scope { get; private set; } = null!;
        public Fixture()
        {
            database.Open(); Db = new(new DbContextOptionsBuilder<Context>().UseSqlite(database).Options); Db.Database.EnsureCreated();
            Db.AddRange(new AppTenant { TenantId = "tenant" }, new AppTenant { TenantId = "other-tenant" });
            Db.Users.AddRange(new OilGasUser { Id = "actor", UserName = "actor", IsActive = true }, new OilGasUser { Id = "member", UserName = "member", IsActive = true }, new OilGasUser { Id = "outsider", UserName = "outsider", IsActive = true });
            Db.Roles.AddRange(new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" }, new IdentityRole { Id = "engineer", Name = "Engineer", NormalizedName = "ENGINEER" }, new IdentityRole { Id = "global", Name = "GlobalManager", NormalizedName = "GLOBALMANAGER" });
            Db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "actor", RoleId = "admin" }, new IdentityUserRole<string> { UserId = "member", RoleId = "engineer" }, new IdentityUserRole<string> { UserId = "member", RoleId = "global" });
            Db.RoleClaims.AddRange(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "Admin.ManageUsers" }, new IdentityRoleClaim<string> { RoleId = "engineer", ClaimType = "permission", ClaimValue = "Production.View" }, new IdentityRoleClaim<string> { RoleId = "global", ClaimType = "permission", ClaimValue = "repository-only" });
            foreach (var scope in new[] { A, B, OtherTenant })
            {
                Db.Add(new AppCompany { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId });
                foreach (var user in new[] { "actor", "member" }) Db.Add(new AppCompanyMembership { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, UserId = user, GrantedByUserId = "actor" });
                Db.Add(new AppUserRoleExtension { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, UserId = "actor", RoleId = "admin" });
            }
            Db.Add(new AppUserRoleExtension { TenantId = A.TenantId, BusinessAssociateId = A.BusinessAssociateId, UserId = "member", RoleId = "engineer" });
            var connections = new List<ConnectionProperties> { new() { ConnectionName = "a", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "database-a" }, new() { ConnectionName = "b", DatabaseType = DataSourceType.SqlServer, Host = "server", Database = "database-b" } };
            var config = new Mock<IConfigEditor>(); config.SetupGet(editor => editor.DataConnections).Returns(connections); Editor.SetupGet(editor => editor.ConfigEditor).Returns(config.Object);
            for (var index = 0; index < 2; index++)
            {
                var tenant = index == 0 ? "tenant" : "other-tenant"; var connection = connections[index];
                Db.Add(new AppModuleConnection { TenantId = tenant, ConnectionKey = connection.ConnectionName.ToUpperInvariant(), AssignedByUserId = "actor", PhysicalTargetKey = ModuleDatabaseTarget.Key(Editor.Object, connection) });
                Db.Add(new ModuleDatabaseBinding { TenantId = tenant, ModuleId = "PPDM_CORE", ConnectionName = connection.ConnectionName, ConnectionKey = connection.ConnectionName.ToUpperInvariant(), ConcurrencyStamp = "v1" });
            }
            Db.SaveChanges(); Access = new(Db); companies = new(Access);
        }
        public async Task Select(CompanyScope scope, string user = "actor")
        {
            var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await companies.ResolveAsync(user, scope))!);
            var gate = new CompanyModuleConnection(new(Db, Editor.Object, tenant), tenant, companies);
            Scope = await gate.ResolveAsync(); Store = new(Db, gate, Mock.Of<IAssetGrantTargetVerifier>()); Reader = new(gate);
        }
        public void Dispose() { Editor.Verify(editor => editor.GetDataSource(It.IsAny<string>()), Times.Never); Db.Dispose(); database.Dispose(); }
    }
}
