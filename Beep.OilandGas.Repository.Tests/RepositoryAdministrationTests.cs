using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class RepositoryAdministrationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private const string Permission = PermissionConstants.WellManagement.View;

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    public async Task The_authority_check_uses_standard_Identity_role_normalization(string name)
    {
        using var f = new Fixture(); var role = (await f.Db.Roles.FindAsync("admin"))!;
        role.Name = name; await f.Db.SaveChangesAsync();
        await f.Assignments.RequireRepositoryAdministratorAsync("repository-admin");
        await Forbidden(() => f.Assignments.RequireRepositoryAdministratorAsync("company-admin"));
    }

    [Theory]
    [InlineData("company-admin")]
    [InlineData("ordinary")]
    [InlineData("missing")]
    public async Task Shared_role_creation_refuses_accounts_without_repository_authority(string actor)
    {
        using var f = new Fixture();
        await Forbidden(() => f.Assignments.CreateRoleAsync(new("New role", "Company must not create a global definition"), actor));
        Assert.Equal(2, await f.Db.Roles.CountAsync()); Assert.Empty(await f.Db.Set<AppRoleExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Approved_role_creation_uses_the_existing_catalog_and_standard_Identity_store()
    {
        using var f = new Fixture();
        var created = await f.Assignments.CreateRoleAsync(new(" Inspector ", "Inspection access"), "repository-admin");
        Assert.Equal("Inspector", created.RoleName); Assert.Equal("Inspection access", created.Description);
        var role = await f.Db.Roles.FindAsync(created.RoleId); Assert.Equal("INSPECTOR", role!.NormalizedName);
        Assert.Equal("Inspection access", (await f.Db.Set<AppRoleExtension>().FindAsync(created.RoleId))!.Description);
        Assert.Empty(await f.Db.RoleClaims.ToArrayAsync()); Assert.False(await f.Db.UserRoles.AnyAsync(value => value.RoleId == created.RoleId));
    }

    [Theory]
    [InlineData("company-admin")]
    [InlineData("ordinary")]
    [InlineData("missing")]
    public async Task Principal_administrator_claims_do_not_grant_shared_permission_authority(string actor)
    {
        using var f = new Fixture(); f.SignIn(actor, administratorClaim: true);
        await Forbidden(() => f.Store.GetPermissionsAsync("reader"));
        await Forbidden(() => f.Store.GrantAsync("reader", Permission));
        await Forbidden(() => f.Store.RevokeAsync("reader", Permission));
        Assert.Empty(await f.Db.RoleClaims.ToArrayAsync()); Assert.Empty(await f.Db.Set<AppRolePermissionExtension>().ToArrayAsync());
    }

    [Theory]
    [InlineData("assign-role")]
    [InlineData("revoke-role")]
    [InlineData("grant-permission")]
    [InlineData("revoke-permission")]
    public async Task Every_global_assignment_mutation_checks_live_repository_authority_before_resource_lookup(string operation)
    {
        using var f = new Fixture();
        await Forbidden(() => operation switch
        {
            "assign-role" => f.Assignments.AssignRoleAsync("ordinary", "admin", "company-admin"),
            "revoke-role" => f.Assignments.RevokeRoleAsync("repository", "company-admin"),
            "grant-permission" => f.Assignments.GrantPermissionToRoleAsync("reader", Permission, "company-admin"),
            _ => f.Assignments.RevokePermissionFromRoleAsync("missing-record", "company-admin")
        });
        Assert.Empty(await f.Db.RoleClaims.ToArrayAsync());
        Assert.Equal("Approved", (await f.Db.Set<AppUserRoleExtension>().FindAsync("repository"))!.ApprovalStatus);
        Assert.False(await f.Db.UserRoles.AnyAsync(value => value.UserId == "ordinary" && value.RoleId == "admin"));
    }

    [Theory]
    [InlineData("no-http-context")]
    [InlineData("anonymous")]
    [InlineData("unresolved")]
    [InlineData("empty-resolved-id")]
    [InlineData("subject-only")]
    public async Task The_store_requires_an_authenticated_resolved_app_account(string reason)
    {
        using var f = new Fixture(); f.SignIn("repository-admin");
        if (reason == "no-http-context") f.Http.HttpContext = null;
        if (reason == "anonymous") f.Http.HttpContext!.User = new(new ClaimsIdentity([new("party_id", "repository-admin")]));
        if (reason == "unresolved") f.ResolutionUnavailable = true;
        if (reason == "empty-resolved-id") f.ResolutionEmpty = true;
        if (reason == "subject-only") f.Http.HttpContext!.User = new(new ClaimsIdentity([new("sub", "repository-admin"), new(ClaimTypes.NameIdentifier, "repository-admin")], "test"));
        await Forbidden(() => f.Store.GetPermissionsAsync("reader"));
        await Forbidden(() => f.Store.GrantAsync("reader", Permission));
        await Forbidden(() => f.Store.RevokeAsync("reader", Permission));
        Assert.Empty(await f.Db.RoleClaims.ToArrayAsync());
    }

    [Fact]
    public async Task Current_repository_authority_works_without_an_incoming_role_claim_and_keeps_audit_history()
    {
        using var f = new Fixture(); f.SignIn("repository-admin", administratorClaim: false);
        Assert.True(await f.Store.GrantAsync("reader", Permission));
        Assert.Equal(new[] { Permission }, await f.Store.GetPermissionsAsync("reader"));
        var grant = await f.Db.Set<AppRolePermissionExtension>().SingleAsync();
        Assert.Equal("repository-admin", grant.ApprovedByUserId); Assert.Equal(Now.UtcDateTime, grant.ApprovedAtUtc);
        Assert.Equal(Permission, (await f.Db.RoleClaims.SingleAsync()).ClaimValue);
        Assert.True(await f.Store.RevokeAsync("reader", Permission));
        Assert.Empty(await f.Db.RoleClaims.ToArrayAsync()); Assert.Null(grant.RoleClaimId); Assert.Equal(Now.UtcDateTime, grant.EffectiveToUtc);
        Assert.False(await f.Store.RevokeAsync("reader", Permission));
        Assert.True(await f.Store.GrantAsync("reader", Permission));
        Assert.Equal(2, await f.Db.Set<AppRolePermissionExtension>().CountAsync());
    }

    [Theory]
    [InlineData("inactive")]
    [InlineData("identity-membership")]
    [InlineData("expired")]
    [InlineData("future")]
    [InlineData("revoked")]
    [InlineData("pending")]
    [InlineData("company-scoped")]
    [InlineData("partial-scope")]
    public async Task Repository_grant_lifecycle_is_rechecked_after_a_successful_mutation(string reason)
    {
        using var f = new Fixture(); f.SignIn("repository-admin", administratorClaim: true);
        await f.Store.GrantAsync("reader", Permission);
        var assignment = (await f.Db.Set<AppUserRoleExtension>().FindAsync("repository"))!;
        if (reason == "inactive") (await f.Db.Users.FindAsync("repository-admin"))!.IsActive = false;
        if (reason == "identity-membership") f.Db.UserRoles.Remove((await f.Db.UserRoles.FindAsync("repository-admin", "admin"))!);
        if (reason == "expired") assignment.EffectiveToUtc = Now.UtcDateTime;
        if (reason == "future") assignment.EffectiveFromUtc = Now.UtcDateTime.AddDays(1);
        if (reason is "revoked" or "pending") assignment.ApprovalStatus = reason == "revoked" ? "Revoked" : "Pending";
        if (reason is "company-scoped" or "partial-scope") assignment.TenantId = "A";
        if (reason == "company-scoped") assignment.BusinessAssociateId = "operator";
        await f.Db.SaveChangesAsync();
        await Forbidden(() => f.Store.GetPermissionsAsync("reader"));
        await Forbidden(() => f.Store.GrantAsync("reader", PermissionConstants.WellManagement.Edit));
        await Forbidden(() => f.Store.RevokeAsync("reader", Permission));
        Assert.Equal(Permission, (await f.Db.RoleClaims.SingleAsync()).ClaimValue);
        Assert.Null((await f.Db.Set<AppRolePermissionExtension>().SingleAsync()).EffectiveToUtc);
    }

    [Fact]
    public async Task Shared_definition_changes_affect_explicit_company_assignments_only_after_repository_admin_approval()
    {
        using var f = new Fixture(); var a = new CompanyScope("A", "operator"); var b = new CompanyScope("B", "operator");
        Assert.Empty((await f.Access.GetCompanyAccessAsync("ordinary", a))!.Permissions);
        f.SignIn("company-admin", true); await Forbidden(() => f.Store.GrantAsync("reader", Permission));
        f.SignIn("repository-admin"); await f.Store.GrantAsync("reader", Permission);
        Assert.Equal(new[] { Permission }, (await f.Access.GetCompanyAccessAsync("ordinary", a))!.Permissions);
        Assert.Equal(new[] { Permission }, (await f.Access.GetCompanyAccessAsync("ordinary", b))!.Permissions);
        Assert.Empty((await f.Access.GetAccessAsync("ordinary"))!.Permissions);
        await f.Store.RevokeAsync("reader", Permission);
        Assert.Empty((await f.Access.GetCompanyAccessAsync("ordinary", a))!.Permissions);
        Assert.Empty((await f.Access.GetCompanyAccessAsync("ordinary", b))!.Permissions);
    }

    [Fact]
    public async Task Permission_identifier_resolution_reuses_the_existing_definition_and_refuses_ambiguous_grants()
    {
        using var f = new Fixture(); f.SignIn("repository-admin");
        f.Db.Add(new AppPermissionExtension { PermissionId = "id", PermissionKey = Permission }); await f.Db.SaveChangesAsync();
        await f.Store.GrantAsync("reader", "id");
        Assert.Equal(new[] { "id" }, await f.Store.GetPermissionsAsync("reader"));
        Assert.True(await f.Store.RevokeAsync("reader", Permission));
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "permission", ClaimValue = Permission },
            new IdentityRoleClaim<string> { RoleId = "reader", ClaimType = "permission", ClaimValue = Permission });
        await f.Db.SaveChangesAsync();
        var refused = await Assert.ThrowsAsync<RefusalException>(() => f.Store.RevokeAsync("reader", "id"));
        Assert.Equal(RefusalKind.Conflict, refused.Kind); Assert.Equal(2, await f.Db.RoleClaims.CountAsync());
    }

    [Fact]
    public async Task Repository_role_mutations_preserve_selected_company_assignments_and_audit_actor()
    {
        using var f = new Fixture();
        var assignment = await f.Assignments.AssignRoleAsync("ordinary", "reader", "repository-admin", "Approved global read");
        Assert.Equal("repository-admin", assignment.GrantedByUserId);
        Assert.Contains("Reader", (await f.Access.GetAccessAsync("ordinary"))!.Roles);
        Assert.True(await f.Assignments.RevokeRoleAsync(assignment.UserRoleId, "repository-admin"));
        Assert.Equal("repository-admin", (await f.Db.Set<AppUserRoleExtension>().FindAsync(assignment.UserRoleId))!.RevokedByUserId);
        Assert.Empty((await f.Access.GetAccessAsync("ordinary"))!.Roles);
        Assert.Equal(new[] { "Reader" }, (await f.Access.GetCompanyAccessAsync("ordinary", new("A", "operator")))!.Roles);
        Assert.Equal(new[] { "Reader" }, (await f.Access.GetCompanyAccessAsync("ordinary", new("B", "operator")))!.Roles);
    }

    private static async Task Forbidden(Func<Task> action) =>
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(action)).Kind);
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:;Foreign Keys=True");
        private readonly ServiceProvider services;
        public RepositoryDbContext Db { get; }
        public RepositoryAccessService Access { get; }
        public RepositoryRoleAssignmentService Assignments { get; }
        public RepositoryApplicationRolePermissionStore Store { get; }
        public HttpContextAccessor Http { get; } = new() { HttpContext = new DefaultHttpContext() };
        public bool ResolutionUnavailable { get; set; }
        public bool ResolutionEmpty { get; set; }
        public Fixture()
        {
            connection.Open(); Db = new Context(new DbContextOptionsBuilder<Context>().UseSqlite(connection).Options); Db.Database.EnsureCreated();
            foreach (var user in new[] { "repository-admin", "company-admin", "ordinary" }) Db.Users.Add(new OilGasUser { Id = user, UserName = user, NormalizedUserName = user.ToUpperInvariant(), IsActive = true });
            Db.Roles.AddRange(new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
                new IdentityRole { Id = "reader", Name = "Reader", NormalizedName = "READER" });
            Db.UserRoles.AddRange(new IdentityUserRole<string> { UserId = "repository-admin", RoleId = "admin" },
                new IdentityUserRole<string> { UserId = "company-admin", RoleId = "admin" }, new IdentityUserRole<string> { UserId = "ordinary", RoleId = "reader" });
            foreach (var tenant in new[] { "A", "B" })
            {
                Db.Add(new AppTenant { TenantId = tenant }); Db.Add(new AppCompany { TenantId = tenant, BusinessAssociateId = "operator" });
                foreach (var user in new[] { "company-admin", "ordinary" })
                {
                    Db.Add(new AppCompanyMembership { TenantId = tenant, BusinessAssociateId = "operator", UserId = user, GrantedByUserId = "repository-admin", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
                    Db.Add(new AppUserRoleExtension { TenantId = tenant, BusinessAssociateId = "operator", UserId = user, RoleId = user == "company-admin" ? "admin" : "reader", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
                }
                Db.Add(new AppCompanyMembership { TenantId = tenant, BusinessAssociateId = "operator", UserId = "repository-admin", GrantedByUserId = "repository-admin", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            }
            Db.Add(new AppUserRoleExtension { UserRoleId = "repository", UserId = "repository-admin", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            Db.SaveChanges();
            var registration = new ServiceCollection(); registration.AddLogging(); registration.AddSingleton(Db);
            registration.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            services = registration.BuildServiceProvider(); Access = new(Db, new Clock());
            Assignments = new(Db, services.GetRequiredService<UserManager<OilGasUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>(), NullLogger<RepositoryRoleAssignmentService>.Instance, new Clock());
            Store = new(Assignments, Db, new RepositoryRequestAccount(Http, principal => ResolutionUnavailable ? null : ResolutionEmpty ? "" : principal.FindFirst("party_id")?.Value, Access, Assignments));
            SignIn("repository-admin");
        }
        public void SignIn(string user, bool administratorClaim = false) => Http.HttpContext!.User = new(new ClaimsIdentity(
            administratorClaim ? new Claim[] { new("party_id", user), new(ClaimTypes.Role, "Administrator") } : [new("party_id", user)], "test"));
        public void Dispose() { services.Dispose(); Db.Dispose(); connection.Dispose(); }
    }
}
