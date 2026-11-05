using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class RepositoryAccountProfileTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 5, 12, 0, 0, TimeSpan.Zero);
    private static readonly CompanyScope A = new("A", "operator");
    private static readonly CompanyScope B = new("B", "operator");

    [Theory]
    [InlineData("read")]
    [InlineData("username")]
    [InlineData("directory")]
    [InlineData("roles")]
    [InlineData("deletion-status")]
    [InlineData("update")]
    [InlineData("delete")]
    [InlineData("add-role")]
    [InlineData("remove-role")]
    public async Task Company_administrator_and_principal_claims_cannot_manage_other_global_accounts(string operation)
    {
        using var f = new Fixture(); await f.Select("company-admin", A); f.AddPrincipalAdministrator();
        var target = (await f.Db.Users.FindAsync("member"))!;
        await Forbidden(() => operation switch
        {
            "read" => f.Users.GetByIdAsync("member"), "username" => f.Users.GetByUsernameAsync("member"),
            "directory" => f.Users.GetAllAsync(), "roles" => f.Users.GetRolesAsync("member"),
            "deletion-status" => f.Users.IsLastActiveAdministratorAsync("member"),
            "update" => f.Users.UpdateAsync("member", new("Changed", null, target.ConcurrencyStamp!)),
            "delete" => f.Users.DeleteAsync("member"), "add-role" => f.Users.AddToRoleAsync("member", "Administrator"),
            _ => f.Users.RemoveFromRoleAsync("member", "Reader")
        });
        Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync()); Assert.True(target.IsActive);
        Assert.False(await f.Db.UserRoles.AnyAsync(value => value.UserId == "member" && value.RoleId == "admin"));
    }

    [Fact]
    public async Task Self_account_reads_and_name_updates_use_the_authenticated_actor_and_do_not_change_roles()
    {
        using var f = new Fixture(); await f.Select("member", A); var before = await f.Db.UserRoles.CountAsync();
        var user = (await f.Users.GetByIdAsync("member"))!;
        var updated = await f.Users.UpdateAsync("member", new("New name", null, user.ConcurrencyStamp));
        Assert.Equal("New name", updated!.FullName); Assert.NotEqual(user.ConcurrencyStamp, updated.ConcurrencyStamp);
        var metadata = await f.Db.Set<AppUserExtension>().SingleAsync();
        Assert.Equal("member", metadata.ChangedBy); Assert.Equal(Now.UtcDateTime, metadata.CreatedUtc); Assert.Equal(Now.UtcDateTime, metadata.ChangedUtc);
        Assert.Equal(before, await f.Db.UserRoles.CountAsync()); Assert.Equal(new[] { "Reader" }, await f.Users.GetRolesAsync("member"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task General_self_updates_cannot_select_account_activation_state(bool active)
    {
        using var f = new Fixture(); await f.Select("member", A); var user = (await f.Users.GetByIdAsync("member"))!;
        await Forbidden(() => f.Users.UpdateAsync("member", new("Changed", active, user.ConcurrencyStamp)));
        Assert.True((await f.Db.Users.FindAsync("member"))!.IsActive); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Controlled_self_deactivation_preserves_history_and_never_accepts_another_subject()
    {
        using var f = new Fixture(); await f.Select("member", A);
        Assert.True(await f.Users.DeactivateSelfAsync()); Assert.False((await f.Db.Users.FindAsync("member"))!.IsActive);
        Assert.True((await f.Db.Users.FindAsync("other"))!.IsActive);
        Assert.Equal("member", (await f.Db.Set<AppUserExtension>().SingleAsync()).ChangedBy);
        await Forbidden(() => f.Users.GetByIdAsync("member"));
    }

    [Fact]
    public async Task The_last_repository_administrator_cannot_deactivate_even_when_company_administrators_exist()
    {
        using var f = new Fixture(); await f.Select("root", A);
        Assert.True(await f.Users.IsLastActiveAdministratorAsync("root"));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => f.Users.DeactivateSelfAsync())).Kind);
        Assert.True((await f.Db.Users.FindAsync("root"))!.IsActive); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Current_repository_administration_can_manage_accounts_and_is_rechecked_after_revocation()
    {
        using var f = new Fixture(); await f.Select("root", A);
        Assert.Equal(4, (await f.Users.GetAllAsync()).Count()); Assert.NotNull(await f.Users.GetByUsernameAsync("member"));
        var target = (await f.Users.GetByIdAsync("member"))!;
        Assert.False((await f.Users.UpdateAsync("member", new("Reviewed", false, target.ConcurrencyStamp)))!.IsActive);
        Assert.Equal("root", (await f.Db.Set<AppUserExtension>().SingleAsync()).ChangedBy);
        (await f.Db.Set<AppUserRoleExtension>().FindAsync("root-global"))!.ApprovalStatus = "Revoked"; await f.Db.SaveChangesAsync();
        await Forbidden(() => f.Users.GetAllAsync()); await Forbidden(() => f.Users.GetByIdAsync("member"));
    }

    [Fact]
    public async Task Stale_versions_refuse_a_name_update_without_overwriting_existing_metadata()
    {
        using var f = new Fixture(); await f.Select("member", A);
        var before = (await f.Users.GetByIdAsync("member"))!;
        await f.Users.UpdateAsync("member", new("First", null, before.ConcurrencyStamp));
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => f.Users.UpdateAsync("member", new("Stale", null, before.ConcurrencyStamp)))).Kind);
        Assert.Equal("First", (await f.Users.GetByIdAsync("member"))!.FullName);
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("unresolved")]
    [InlineData("missing")]
    [InlineData("inactive")]
    public async Task Account_services_refuse_without_current_resolved_active_authority(string reason)
    {
        using var f = new Fixture(); await f.Select("member", A);
        if (reason == "anonymous") f.Http.HttpContext!.User = new(new ClaimsIdentity([new("party_id", "member")]));
        if (reason == "unresolved") f.ResolutionUnavailable = true;
        if (reason == "missing") f.SignIn("missing");
        if (reason == "inactive") { (await f.Db.Users.FindAsync("member"))!.IsActive = false; await f.Db.SaveChangesAsync(); }
        await Forbidden(() => f.Users.GetByIdAsync("member"));
        await Forbidden(() => f.Profiles.UpdateUserPreferredLayoutAsync("member", "Layout"));
        Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Theory]
    [InlineData("profile")]
    [InlineData("preferences")]
    [InlineData("layout")]
    [InlineData("primary-role")]
    [InlineData("login")]
    public async Task Company_administrators_cannot_read_or_write_another_accounts_shared_profile(string operation)
    {
        using var f = new Fixture(); await f.Select("company-admin", A);
        await Forbidden(() => operation switch
        {
            "profile" => f.Profiles.GetUserProfileAsync("member"),
            "preferences" => f.Profiles.UpdateUserPreferencesAsync("member", "{}"),
            "layout" => f.Profiles.UpdateUserPreferredLayoutAsync("member", "Layout"),
            "primary-role" => f.Profiles.UpdateUserPrimaryRoleAsync("member", "Engineer"),
            _ => f.Profiles.RecordUserLoginAsync("member")
        });
        Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Self_profile_preferences_and_current_company_role_selection_never_grant_Identity_memberships()
    {
        using var f = new Fixture(); await f.Select("member", A); var count = await f.Db.UserRoles.CountAsync();
        Assert.NotNull(await f.Profiles.GetUserProfileAsync("member")); Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
        await f.Profiles.UpdateUserPreferencesAsync("member", "{\"density\":\"compact\"}");
        await f.Profiles.UpdateUserPreferredLayoutAsync("member", "DefaultLayout");
        await f.Profiles.UpdateUserPrimaryRoleAsync("member", "engineer"); await f.Profiles.RecordUserLoginAsync("member");
        var profile = (await f.Profiles.GetUserProfileAsync("member"))!;
        Assert.Equal("Engineer", profile.PrimaryRole); Assert.Equal(new[] { "Engineer" }, profile.Roles);
        Assert.Equal("DefaultLayout", profile.PreferredLayout); Assert.Equal(Now.UtcDateTime, profile.LastLoginDate);
        var metadata = await f.Db.Set<AppUserExtension>().SingleAsync(); Assert.Equal("member", metadata.ChangedBy); Assert.Equal(Now.UtcDateTime, metadata.ChangedUtc);
        Assert.Equal(count, await f.Db.UserRoles.CountAsync()); Assert.Equal("engineer", metadata.PrimaryRoleId);
        Assert.Empty(await f.Db.ModuleDatabases.ToArrayAsync()); // Preferences/roles work before PPDM provisioning.
    }

    [Theory]
    [InlineData("Accountant")]
    [InlineData("Reader")]
    [InlineData("Administrator")]
    public async Task A_primary_role_cannot_select_another_company_repository_only_or_unassigned_role(string role)
    {
        using var f = new Fixture(); await f.Select("member", A);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => f.Profiles.UpdateUserPrimaryRoleAsync("member", role))).Kind);
        Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("pending")]
    [InlineData("identity-removed")]
    public async Task Lost_company_roles_refuse_new_selection_and_hide_existing_primary_role_after_refresh(string reason)
    {
        using var f = new Fixture(); await f.Select("member", A); await f.Profiles.UpdateUserPrimaryRoleAsync("member", "Engineer");
        var grant = (await f.Db.Set<AppUserRoleExtension>().FindAsync("member-A"))!;
        if (reason == "expired") grant.EffectiveToUtc = Now.UtcDateTime;
        if (reason == "pending") grant.ApprovalStatus = "Pending";
        if (reason == "identity-removed") f.Db.UserRoles.Remove((await f.Db.UserRoles.FindAsync("member", "engineer"))!);
        await f.Db.SaveChangesAsync();
        await Forbidden(() => f.Profiles.GetUserProfileAsync("member"));
        await f.Select("member", A); Assert.Null((await f.Profiles.GetUserProfileAsync("member"))!.PrimaryRole);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => f.Profiles.UpdateUserPrimaryRoleAsync("member", "Engineer"))).Kind);
        Assert.Equal("engineer", (await f.Db.Set<AppUserExtension>().SingleAsync()).PrimaryRoleId);
    }

    [Fact]
    public async Task Switching_company_filters_the_single_shared_display_preference_without_changing_grants()
    {
        using var f = new Fixture(); await f.Select("member", A); await f.Profiles.UpdateUserPrimaryRoleAsync("member", "Engineer");
        await f.Select("member", B); Assert.Null((await f.Profiles.GetUserProfileAsync("member"))!.PrimaryRole);
        await f.Profiles.UpdateUserPrimaryRoleAsync("member", "Accountant"); Assert.Equal("Accountant", (await f.Profiles.GetUserProfileAsync("member"))!.PrimaryRole);
        await f.Select("member", A); Assert.Null((await f.Profiles.GetUserProfileAsync("member"))!.PrimaryRole);
        Assert.Single(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task Profile_actor_must_match_the_validated_company_context()
    {
        using var f = new Fixture(); await f.Select("member", A); f.SignIn("other");
        await Forbidden(() => f.Profiles.UpdateUserPreferredLayoutAsync("other", "Layout"));
        Assert.Empty(await f.Db.Set<AppUserExtension>().ToArrayAsync());
    }

    [Fact]
    public async Task A_repository_administrator_can_update_shared_preferences_but_cannot_record_someone_elses_login()
    {
        using var f = new Fixture(); await f.Select("root", A);
        Assert.True(await f.Profiles.UpdateUserPreferredLayoutAsync("member", "ReviewedLayout"));
        Assert.True(await f.Profiles.UpdateUserPrimaryRoleAsync("member", "Engineer"));
        await Forbidden(() => f.Profiles.RecordUserLoginAsync("member"));
        Assert.Equal("root", (await f.Db.Set<AppUserExtension>().SingleAsync()).ChangedBy);
        Assert.Null((await f.Db.Set<AppUserExtension>().SingleAsync()).LastLoginUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("other-company")]
    public async Task Organization_arguments_cannot_override_selected_profile_role_scope(string organization)
    {
        using var f = new Fixture(); await f.Select("member", A);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => f.Profiles.GetUserRolesAsync("member", organization))).Kind);
    }

    private static async Task Forbidden(Func<Task> action) => Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(action)).Kind);
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Clock : TimeProvider { public override DateTimeOffset GetUtcNow() => Now; }
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database = new("Data Source=:memory:;Foreign Keys=True");
        private readonly ServiceProvider services;
        public RepositoryDbContext Db { get; }
        public RepositoryUserService Users { get; }
        public RepositoryUserProfileService Profiles { get; private set; } = null!;
        private readonly RepositoryCompanyContextResolver companies;
        private readonly RepositoryRequestAccount account;
        public HttpContextAccessor Http { get; } = new() { HttpContext = new DefaultHttpContext() };
        public bool ResolutionUnavailable { get; set; }
        private readonly Mock<IDMEEditor> editor = new(MockBehavior.Strict);
        public Fixture()
        {
            database.Open(); Db = new Context(new DbContextOptionsBuilder<Context>().UseSqlite(database).Options); Db.Database.EnsureCreated();
            foreach (var id in new[] { "root", "member", "company-admin", "other" }) Db.Users.Add(new OilGasUser { Id = id, UserName = id, NormalizedUserName = id.ToUpperInvariant(), IsActive = true });
            foreach (var pair in new[] { ("admin", "Administrator"), ("engineer", "Engineer"), ("accountant", "Accountant"), ("reader", "Reader") }) Db.Roles.Add(new IdentityRole { Id = pair.Item1, Name = pair.Item2, NormalizedName = pair.Item2.ToUpperInvariant() });
            foreach (var pair in new[] { ("root", "admin"), ("company-admin", "admin"), ("member", "engineer"), ("member", "accountant"), ("member", "reader") }) Db.UserRoles.Add(new IdentityUserRole<string> { UserId = pair.Item1, RoleId = pair.Item2 });
            foreach (var scope in new[] { A, B })
            {
                Db.Add(new AppTenant { TenantId = scope.TenantId }); Db.Add(new AppCompany { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId });
                foreach (var id in new[] { "root", "member", "company-admin", "other" }) Db.Add(new AppCompanyMembership { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, UserId = id, GrantedByUserId = "root", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
                Db.Add(new AppUserRoleExtension { UserId = "root", RoleId = "admin", TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
                Db.Add(new AppUserRoleExtension { UserRoleId = "member-" + scope.TenantId, UserId = "member", RoleId = scope == A ? "engineer" : "accountant", TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            }
            Db.Add(new AppUserRoleExtension { UserRoleId = "root-global", UserId = "root", RoleId = "admin", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            Db.Add(new AppUserRoleExtension { UserId = "company-admin", RoleId = "admin", TenantId = A.TenantId, BusinessAssociateId = A.BusinessAssociateId, EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            Db.Add(new AppUserRoleExtension { UserId = "member", RoleId = "reader", EffectiveFromUtc = Now.UtcDateTime.AddDays(-1) });
            Db.SaveChanges();
            var registration = new ServiceCollection(); registration.AddLogging(); registration.AddSingleton(Db);
            registration.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            services = registration.BuildServiceProvider();
            var access = new RepositoryAccessService(Db, new Clock()); companies = new(access);
            var assignments = new RepositoryRoleAssignmentService(Db, services.GetRequiredService<UserManager<OilGasUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>(), NullLogger<RepositoryRoleAssignmentService>.Instance, new Clock());
            account = new(Http, principal => ResolutionUnavailable ? null : principal.FindFirst("party_id")?.Value, access, assignments);
            Users = new(Db, services.GetRequiredService<UserManager<OilGasUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>(), assignments, account, new Clock());
        }
        public async Task Select(string actor, CompanyScope scope)
        {
            SignIn(actor); var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await companies.ResolveAsync(actor, scope))!);
            var company = new CompanyModuleConnection(new(Db, editor.Object, tenant), tenant, companies);
            Profiles = new(Db, new RepositoryApplicationAuthorizationReader(company), new UpperInvariantLookupNormalizer(), account, company, Mock.Of<IPPDM39DataService>(), new Clock());
        }
        public void SignIn(string actor) => Http.HttpContext!.User = new(new ClaimsIdentity([new("party_id", actor)], "test"));
        public void AddPrincipalAdministrator() => ((ClaimsIdentity)Http.HttpContext!.User.Identity!).AddClaim(new(ClaimTypes.Role, "Administrator"));
        public void Dispose() { editor.VerifyNoOtherCalls(); services.Dispose(); Db.Dispose(); database.Dispose(); }
    }
}
