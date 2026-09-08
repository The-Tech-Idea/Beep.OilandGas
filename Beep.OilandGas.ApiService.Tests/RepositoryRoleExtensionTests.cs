using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryRoleExtensionTests
{
    [Fact]
    public async Task CatalogIncludesCustomIdentityPermissionsWithoutCreatingMetadataOnRead()
    {
        using var fixture = new Fixture();
        var source = new IdentityRole("Custom Source");
        var target = new IdentityRole("Custom Target");
        Assert.True((await fixture.Roles.CreateAsync(source)).Succeeded);
        Assert.True((await fixture.Roles.CreateAsync(target)).Succeeded);
        Assert.True((await fixture.Roles.AddClaimAsync(source, new System.Security.Claims.Claim("permission", "Custom.Inspect"))).Succeeded);
        Assert.True((await fixture.Roles.AddClaimAsync(source, new System.Security.Claims.Claim("department", "Not.A.Permission"))).Succeeded);

        var catalog = await fixture.Service.GetPermissionCatalogAsync();

        var permission = Assert.Single(catalog, x => x.PermissionKey == "Custom.Inspect");
        Assert.DoesNotContain(catalog, x => x.PermissionKey == "Not.A.Permission");
        Assert.Empty(await fixture.Db.Set<AppPermissionExtension>().ToListAsync());
        await fixture.Service.GrantPermissionToRoleAsync(target.Id, permission.PermissionId, "actor");
        Assert.True(await fixture.Db.RoleClaims.AnyAsync(x => x.RoleId == target.Id && x.ClaimType == "permission" && x.ClaimValue == "Custom.Inspect"));
        Assert.Single(await fixture.Service.GetPermissionCatalogAsync(), x => x.PermissionKey == "Custom.Inspect");
    }

    [Fact]
    public async Task GrantByPermissionKeyReusesExistingExtensionId()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        fixture.Db.Add(new AppPermissionExtension { PermissionId = "metadata-id", PermissionKey = "Custom.Inspect" });
        await fixture.Db.SaveChangesAsync();

        var byKey = await fixture.Service.GrantPermissionToRoleAsync(role.Id, "Custom.Inspect", "actor");
        var byId = await fixture.Service.GrantPermissionToRoleAsync(role.Id, "metadata-id", "actor");

        Assert.Equal("metadata-id", byKey.PermissionId);
        Assert.Equal(byKey.RolePermissionId, byId.RolePermissionId);
        Assert.Equal(1, await fixture.Db.Set<AppPermissionExtension>().CountAsync());
        Assert.Equal(1, await fixture.Db.RoleClaims.CountAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.GrantPermissionToRoleAsync(role.Id, "Unknown.Permission", "actor"));
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    public async Task LastAdministratorProtectionUsesIdentityNormalizedNames(string roleName)
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "only-admin" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        var role = new IdentityRole(roleName);
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var assignment = await fixture.Service.AssignRoleAsync(user.Id, role.Id, user.Id);
        var removal = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.RevokeRoleAsync(assignment.UserRoleId, user.Id));
        Assert.Contains("last active administrator", removal.Message);
        Assert.True(await fixture.Users.IsInRoleAsync(user, "Administrator"));
        fixture.Db.ChangeTracker.Clear();
        var deactivation = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.UserService.DeleteAsync(user.Id));
        Assert.Contains("last active administrator", deactivation.Message);
        Assert.True((await fixture.Users.FindByIdAsync(user.Id))!.IsActive);
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("ADMINISTRATOR")]
    public async Task FirstRegistrationCanonicalizesExistingReservedRoleWithoutDuplicatingIt(string roleName)
    {
        using var fixture = new Fixture();
        var role = new IdentityRole(roleName);
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var bootstrap = new RepositoryBootstrapService(fixture.Db, fixture.Users, fixture.Roles);

        Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync("https://issuer", "first-user"));

        var savedRole = await fixture.Db.Roles.SingleAsync();
        Assert.Equal(role.Id, savedRole.Id);
        Assert.Equal("Administrator", savedRole.Name);
        var access = await new RepositoryAccessService(fixture.Db).GetAccessAsync("https://issuer", "first-user");
        Assert.Equal("Administrator", Assert.Single(access!.Roles));
        Assert.Equal(role.Id, (await fixture.Db.Set<AppRoleExtension>().SingleAsync()).RoleId);
    }

    [Fact]
    public async Task GrantReusesDuplicateIdentityPermissionWithoutCreatingAnotherClaim()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        for (var i = 0; i < 2; i++)
            Assert.True((await fixture.Roles.AddClaimAsync(role, new System.Security.Claims.Claim("permission", code))).Succeeded);

        var grant = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor");

        Assert.Equal(2, await fixture.Db.RoleClaims.CountAsync());
        Assert.Equal(grant.RolePermissionId, (await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor")).RolePermissionId);
        Assert.Single(await fixture.Db.Set<AppRolePermissionExtension>().ToListAsync());
    }

    [Fact]
    public async Task RegrantAfterExternalClaimChangeCreatesNewHistoryAndPreservesChangedClaim()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        var old = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor");
        var claim = await fixture.Db.RoleClaims.SingleAsync();
        claim.ClaimValue = "Changed.Permission";
        await fixture.Db.SaveChangesAsync();

        var current = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor");

        Assert.NotEqual(old.RolePermissionId, current.RolePermissionId);
        Assert.Equal(2, await fixture.Db.RoleClaims.CountAsync());
        var history = await fixture.Db.Set<AppRolePermissionExtension>().SingleAsync(x => x.RolePermissionId == old.RolePermissionId);
        Assert.NotNull(history.EffectiveToUtc);
        Assert.Null(history.RoleClaimId);
        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(current.RolePermissionId, "actor"));
        Assert.Equal("Changed.Permission", (await fixture.Db.RoleClaims.SingleAsync()).ClaimValue);
    }

    [Fact]
    public async Task ExtensionRevocationTargetsExactClaimAndPreservesDuplicateGrant()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        var grant = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor");
        var original = await fixture.Db.RoleClaims.SingleAsync();
        Assert.True((await fixture.Roles.AddClaimAsync(role, new System.Security.Claims.Claim("permission", code))).Succeeded);
        var duplicate = await fixture.Db.RoleClaims.SingleAsync(x => x.Id != original.Id);

        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(grant.RolePermissionId, "actor"));

        Assert.Equal(duplicate.Id, (await fixture.Db.RoleClaims.SingleAsync()).Id);
        var history = await fixture.Db.Set<AppRolePermissionExtension>().SingleAsync();
        Assert.Null(history.RoleClaimId);
        Assert.NotNull(history.EffectiveToUtc);
        var remaining = Assert.Single(await fixture.Service.GetRolePermissionsAsync(role.Id));
        Assert.Equal("claim:" + duplicate.Id, remaining.RolePermissionId);
        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(remaining.RolePermissionId, "actor"));
        Assert.Empty(await fixture.Db.RoleClaims.ToListAsync());
    }

    [Fact]
    public async Task StaleExtensionCannotRevokeChangedIdentityPermission()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        var grant = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, "actor");
        var claim = await fixture.Db.RoleClaims.SingleAsync();
        claim.ClaimValue = "Changed.Permission";
        await fixture.Db.SaveChangesAsync();

        Assert.False(await fixture.Service.RevokePermissionFromRoleAsync(grant.RolePermissionId, "actor"));

        Assert.Equal("Changed.Permission", (await fixture.Db.RoleClaims.SingleAsync()).ClaimValue);
        Assert.Null((await fixture.Db.Set<AppRolePermissionExtension>().SingleAsync()).EffectiveToUtc);
        var current = Assert.Single(await fixture.Service.GetRolePermissionsAsync(role.Id));
        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(current.RolePermissionId, "actor"));
        Assert.Null((await fixture.Db.Set<AppRolePermissionExtension>().SingleAsync()).RoleClaimId);
    }

    [Fact]
    public async Task IdentityPermissionWithoutMetadataIsVisibleAndRevocable()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        Assert.True((await fixture.Roles.AddClaimAsync(role, new System.Security.Claims.Claim("permission", "Field.Read"))).Succeeded);
        var grant = Assert.Single(await fixture.Service.GetRolePermissionsAsync(role.Id));
        Assert.Equal("Field.Read", grant.PermissionId);
        Assert.Empty(await fixture.Db.Set<AppRolePermissionExtension>().ToListAsync());
        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(grant.RolePermissionId, "actor"));
        Assert.Empty(await fixture.Db.RoleClaims.ToListAsync());
        Assert.False(await fixture.Service.RevokePermissionFromRoleAsync(grant.RolePermissionId, "actor"));
    }

    [Fact]
    public async Task PermissionRevocationCannotRemoveNonPermissionClaims()
    {
        using var fixture = new Fixture();
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        Assert.True((await fixture.Roles.AddClaimAsync(role, new System.Security.Claims.Claim("department", "Engineering"))).Succeeded);
        var claim = await fixture.Db.RoleClaims.SingleAsync();
        Assert.Empty(await fixture.Service.GetRolePermissionsAsync(role.Id));
        Assert.False(await fixture.Service.RevokePermissionFromRoleAsync("claim:" + claim.Id, "actor"));
        Assert.Equal(1, await fixture.Db.RoleClaims.CountAsync());
    }

    [Fact]
    public async Task IdentityMembershipWithoutMetadataIsVisibleAndRemovable()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "identity-user" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        Assert.True((await fixture.Roles.CreateAsync(new IdentityRole("Viewer"))).Succeeded);
        Assert.True((await fixture.Users.AddToRoleAsync(user, "Viewer")).Succeeded);
        var assignment = Assert.Single(await fixture.Service.GetUserRoleAssignmentsAsync(user.Id));
        Assert.StartsWith("identity:", assignment.UserRoleId);
        Assert.Empty(await fixture.Db.Set<AppUserRoleExtension>().ToListAsync());
        Assert.True(await fixture.UserService.RemoveFromRoleAsync(user.Id, "Viewer"));
        Assert.False(await fixture.Users.IsInRoleAsync(user, "Viewer"));
        Assert.NotNull((await fixture.Db.Set<AppUserRoleExtension>().SingleAsync()).EffectiveToUtc);
        Assert.False(await fixture.Service.RevokeRoleAsync(assignment.UserRoleId, "actor"));
    }

    [Fact]
    public async Task LastAdministratorWithoutMetadataIsStillProtected()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "identity-admin" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        Assert.True((await fixture.Roles.CreateAsync(new IdentityRole("Administrator"))).Succeeded);
        Assert.True((await fixture.Users.AddToRoleAsync(user, "Administrator")).Succeeded);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.UserService.RemoveFromRoleAsync(user.Id, "Administrator"));
        Assert.True(await fixture.Users.IsInRoleAsync(user, "Administrator"));
        Assert.Empty(await fixture.Db.Set<AppUserRoleExtension>().ToListAsync());
    }

    [Fact]
    public async Task RoleCreationUsesIdentityAndExtensionMetadataWithoutDuplicateNames()
    {
        using var fixture = new Fixture();
        var catalog = new RepositoryRoleCatalogService(fixture.Db, fixture.Roles);
        var created = await catalog.CreateAsync(new(" Field Engineer ", "Operational access"));
        Assert.Equal("Field Engineer", created.RoleName);
        Assert.Equal(created.RoleId, (await fixture.Roles.FindByNameAsync("field engineer"))!.Id);
        Assert.Equal("Operational access", (await fixture.Db.Set<AppRoleExtension>().SingleAsync()).Description);
        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.CreateAsync(new("FIELD ENGINEER", null)));
        Assert.Equal(1, await fixture.Db.Roles.CountAsync());
        Assert.Equal(1, await fixture.Db.Set<AppRoleExtension>().CountAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Admin,Viewer")]
    public async Task InvalidRoleNamesCannotCreateIdentityRows(string name)
    {
        using var fixture = new Fixture();
        var catalog = new RepositoryRoleCatalogService(fixture.Db, fixture.Roles);
        await Assert.ThrowsAsync<ArgumentException>(() => catalog.CreateAsync(new(name, null)));
        Assert.Empty(await fixture.Db.Roles.ToListAsync());
    }

    [Fact]
    public async Task GrantsAffectIdentityAndRevocationPreservesExtensionHistory()
    {
        using var fixture = new Fixture();
        var admin = new OilGasUser { UserName = "admin" };
        Assert.True((await fixture.Users.CreateAsync(admin)).Succeeded);
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        fixture.Db.Add(new AppRoleExtension { RoleId = role.Id, Description = "Field observer", ValidFieldScope = "FIELD-A" });
        await fixture.Db.SaveChangesAsync();
        var grant = await fixture.Service.AssignRoleAsync(admin.Id, role.Id, admin.Id, "Approved review access");
        Assert.True(await fixture.Users.IsInRoleAsync(admin, "Viewer"));
        Assert.Equal("Approved review access", grant.AssignmentReason);
        Assert.Equal(grant.UserRoleId, (await fixture.Service.AssignRoleAsync(admin.Id, role.Id, admin.Id)).UserRoleId);
        Assert.Equal("FIELD-A", (await fixture.Service.GetRoleCatalogAsync()).Single().ValidFieldScope);
        Assert.True(await fixture.Service.RevokeRoleAsync(grant.UserRoleId, admin.Id));
        Assert.False(await fixture.Users.IsInRoleAsync(admin, "Viewer"));
        var history = await fixture.Db.Set<AppUserRoleExtension>().SingleAsync();
        Assert.Equal("Revoked", history.ApprovalStatus);
        Assert.NotNull(history.EffectiveToUtc);
        Assert.Empty(await fixture.Service.GetUserRoleAssignmentsAsync(admin.Id));

        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        var permission = await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, admin.Id);
        Assert.True(await fixture.Db.RoleClaims.AnyAsync(x => x.RoleId == role.Id && x.ClaimValue == code));
        Assert.True(await fixture.Service.RevokePermissionFromRoleAsync(permission.RolePermissionId, admin.Id));
        Assert.False(await fixture.Db.RoleClaims.AnyAsync(x => x.RoleId == role.Id));
        var permissionHistory = await fixture.Db.Set<AppRolePermissionExtension>().SingleAsync();
        Assert.Null(permissionHistory.RoleClaimId);
        Assert.NotNull(permissionHistory.EffectiveToUtc);
    }

    [Fact]
    public async Task LastAdministratorCannotBeRevoked()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "only-admin" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        var role = new IdentityRole("Administrator");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        var assignment = await fixture.Service.AssignRoleAsync(user.Id, role.Id, user.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.RevokeRoleAsync(assignment.UserRoleId, user.Id));
        Assert.True(await fixture.Users.IsInRoleAsync(user, "Administrator"));
    }

    [Fact]
    public async Task ExtensionMetadataCannotSubstituteForIdentityGrants()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "viewer" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        var role = new IdentityRole("Viewer");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        await fixture.Service.AssignRoleAsync(user.Id, role.Id, user.Id);
        Assert.True((await fixture.Users.RemoveFromRoleAsync(user, role.Name!)).Succeeded);
        Assert.Empty(await fixture.Service.GetUserRoleAssignmentsAsync(user.Id));
        Assert.Single(await fixture.Db.Set<AppUserRoleExtension>().ToListAsync());

        var code = (await fixture.Service.GetPermissionCatalogAsync()).First().PermissionId;
        await fixture.Service.GrantPermissionToRoleAsync(role.Id, code, user.Id);
        Assert.Single(await fixture.Service.GetRolePermissionsAsync(role.Id));
        var claim = await fixture.Db.RoleClaims.SingleAsync();
        claim.ClaimValue = "different-permission";
        await fixture.Db.SaveChangesAsync();
        var changedClaim = Assert.Single(await fixture.Service.GetRolePermissionsAsync(role.Id));
        Assert.Equal("different-permission", changedClaim.PermissionId);
        Assert.StartsWith("claim:", changedClaim.RolePermissionId);
        Assert.Single(await fixture.Db.Set<AppRolePermissionExtension>().ToListAsync());
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);

    [Fact]
    public async Task UserMetadataAndSoftDeactivationUseRepository()
    {
        using var fixture = new Fixture();
        var account = new OilGasUser { UserName = "ordinary" };
        Assert.True((await fixture.Users.CreateAsync(account)).Succeeded);
        var input = (await fixture.UserService.GetByIdAsync(account.Id))!;
        Assert.NotNull(await fixture.UserService.UpdateAsync(account.Id, new("Field Engineer", null, input.ConcurrencyStamp)));
        Assert.Equal("Field Engineer", (await fixture.UserService.GetByIdAsync(account.Id))!.FullName);
        Assert.Equal("Field Engineer", (await fixture.Db.Set<AppUserExtension>().SingleAsync()).FullName);
        Assert.True(await fixture.UserService.DeleteAsync(account.Id));
        Assert.False((await fixture.Users.FindByIdAsync(account.Id))!.IsActive);
        Assert.Equal(1, await fixture.Db.Users.CountAsync());
    }

    [Fact]
    public async Task LastAdministratorCannotBeDisabled()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "administrator" };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        var role = new IdentityRole("Administrator");
        Assert.True((await fixture.Roles.CreateAsync(role)).Succeeded);
        await fixture.Service.AssignRoleAsync(user.Id, role.Id, user.Id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.UserService.DeleteAsync(user.Id));
        Assert.True((await fixture.Users.FindByIdAsync(user.Id))!.IsActive);
    }

    [Fact]
    public async Task UserUpdateRejectsStaleVersionWithoutOverwritingMetadata()
    {
        using var fixture = new Fixture();
        var account = new OilGasUser { UserName = "versioned", Email = "verified@example.test" };
        Assert.True((await fixture.Users.CreateAsync(account)).Succeeded);
        var original = (await fixture.UserService.GetByIdAsync(account.Id))!;
        var saved = (await fixture.UserService.UpdateAsync(account.Id, new("First", null, original.ConcurrencyStamp)))!;
        Assert.NotEqual(original.ConcurrencyStamp, saved.ConcurrencyStamp);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            fixture.UserService.UpdateAsync(account.Id, new("Stale", false, original.ConcurrencyStamp)));
        var current = (await fixture.UserService.GetByIdAsync(account.Id))!;
        Assert.Equal("First", current.FullName);
        Assert.True(current.IsActive);
        Assert.Equal("verified@example.test", current.Email);
        Assert.Equal("actor", (await fixture.Db.Set<AppUserExtension>().SingleAsync()).ChangedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UserUpdateRequiresVersion(string version)
    {
        using var fixture = new Fixture();
        var account = new OilGasUser { UserName = "required-version" };
        Assert.True((await fixture.Users.CreateAsync(account)).Succeeded);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            fixture.UserService.UpdateAsync(account.Id, new("Not saved", null, version)));
        Assert.Empty(await fixture.Db.Set<AppUserExtension>().ToListAsync());
    }

    [Fact]
    public async Task CanonicalRoleCatalogIncludesIdentityRolesWithoutExtensions()
    {
        using var fixture = new Fixture();
        Assert.True((await fixture.Roles.CreateAsync(new IdentityRole("Direct"))).Succeeded);
        var catalog = new RepositoryRoleCatalogService(fixture.Db, fixture.Roles);
        var created = await catalog.CreateAsync(new("Extended", "Description"));
        var all = await catalog.GetAllAsync();
        Assert.Equal(2, all.Count);
        Assert.Null(all.Single(x => x.RoleName == "Direct").Description);
        Assert.Equal(created, all.Single(x => x.RoleName == "Extended"));
        Assert.Equal(1, await fixture.Db.Set<AppRoleExtension>().CountAsync());
    }

    [Fact]
    public async Task ReadinessRequiresOperatorRecoveryForUsersWithoutBootstrapMarker()
    {
        using var fixture = new Fixture();
        var readiness = new RepositoryReadinessService(fixture.Db,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RepositoryReadinessService>.Instance);
        Assert.Equal(RepositoryReadiness.BootstrapRequired, await readiness.CheckAsync());
        Assert.True((await fixture.Users.CreateAsync(new OilGasUser { UserName = "existing" })).Succeeded);
        Assert.Equal(RepositoryReadiness.RecoveryRequired, await readiness.CheckAsync());
        Assert.Empty(await fixture.Db.UserRoles.ToListAsync());
        Assert.Empty(await fixture.Db.Bootstrap.ToListAsync());
    }

    [Fact]
    public async Task ReadinessDetectsLostAdministratorWithoutGrantingAnother()
    {
        using var fixture = new Fixture();
        var bootstrap = new RepositoryBootstrapService(fixture.Db, fixture.Users, fixture.Roles);
        Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync("https://issuer", "first"));
        var readiness = new RepositoryReadinessService(fixture.Db,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<RepositoryReadinessService>.Instance);
        Assert.Equal(RepositoryReadiness.Ready, await readiness.CheckAsync());
        fixture.Db.UserRoles.RemoveRange(await fixture.Db.UserRoles.ToListAsync());
        await fixture.Db.SaveChangesAsync();
        Assert.Equal(RepositoryReadiness.RecoveryRequired, await readiness.CheckAsync());
        Assert.Empty(await fixture.Db.UserRoles.ToListAsync());
    }

    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly ServiceProvider _provider;
        private readonly IServiceScope _scope;
        public RepositoryDbContext Db { get; }
        public UserManager<OilGasUser> Users { get; }
        public RoleManager<IdentityRole> Roles { get; }
        public RepositoryRoleAssignmentService Service { get; }
        public RepositoryUserService UserService { get; }
        public Fixture()
        {
            _connection.Open();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestContext>(options => options.UseSqlite(_connection));
            services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<TestContext>());
            services.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            services.AddScoped<RepositoryRoleAssignmentService>();
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor>(new Microsoft.AspNetCore.Http.HttpContextAccessor
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
                {
                    User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
                    { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "actor") }, "test"))
                }
            });
            services.AddScoped<RepositoryUserService>();
            _provider = services.BuildServiceProvider();
            _scope = _provider.CreateScope();
            Db = _scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
            Db.Database.EnsureCreated();
            Users = _scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            Roles = _scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            Service = _scope.ServiceProvider.GetRequiredService<RepositoryRoleAssignmentService>();
            UserService = _scope.ServiceProvider.GetRequiredService<RepositoryUserService>();
        }
        public void Dispose() { _scope.Dispose(); _provider.Dispose(); _connection.Dispose(); }
    }
}
