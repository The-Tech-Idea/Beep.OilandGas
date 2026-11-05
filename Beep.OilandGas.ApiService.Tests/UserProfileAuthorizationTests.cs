using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class UserProfileAuthorizationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task UnauthenticatedUnresolvedAndOtherUsersCannotAccessProfiles(bool authenticated, bool localId)
    {
        using var fixture = new Fixture(); fixture.SignIn(localId ? "other" : null, authenticated);
        foreach (var operation in new[] { "profile", "roles", "layout", "preferences", "primary", "preferred" })
            await Forbidden(() => Invoke(fixture.Controller, operation));
        fixture.Profile.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("profile")]
    [InlineData("roles")]
    [InlineData("layout")]
    [InlineData("preferences")]
    [InlineData("primary")]
    [InlineData("preferred")]
    public async Task AnIncomingAdministratorClaimCannotAuthorizeAnotherAccountsProfile(string operation)
    {
        using var fixture = new Fixture(); fixture.SignIn("other", claimedAdministrator: true);
        await Forbidden(() => Invoke(fixture.Controller, operation)); fixture.Profile.VerifyNoOtherCalls();
    }

    private static Task Invoke(UserProfileController controller, string operation) => operation switch
    {
        "profile" => controller.GetUserProfile("target"), "roles" => controller.GetUserRoles("target"),
        "layout" => controller.GetUserDefaultLayout("target"), "preferences" => controller.UpdateUserPreferences("target", new()),
        "primary" => controller.UpdateUserPrimaryRole("target", new()), _ => controller.UpdateUserPreferredLayout("target", new())
    };

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LoginIsRecordedOnlyForTheSignedInAccount(bool administrator)
    {
        using var fixture = new Fixture(); fixture.SignIn("owner", claimedAdministrator: administrator);
        fixture.Profile.Setup(x => x.RecordUserLoginAsync("owner")).Returns(Task.CompletedTask);
        Assert.IsType<OkObjectResult>(await fixture.Controller.RecordUserLogin());
        fixture.Profile.VerifyAll(); fixture.Profile.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LoginWithoutAResolvedAccountIsRefusedBeforeStorage()
    {
        using var fixture = new Fixture(); fixture.SignIn(null);
        fixture.Http.HttpContext!.User.AddIdentity(new ClaimsIdentity([new(ClaimTypes.NameIdentifier, "owner")], "test"));
        await Forbidden(() => fixture.Controller.RecordUserLogin()); fixture.Profile.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task AnOwnerCanSelectAPrimaryDisplayRoleThroughTheServiceWithoutAnAdministratorClaim()
    {
        using var fixture = new Fixture(); fixture.SignIn("owner");
        fixture.Profile.Setup(x => x.UpdateUserPrimaryRoleAsync("owner", "Reader")).ReturnsAsync(true);
        Assert.Equal(true, Assert.IsType<OkObjectResult>((await fixture.Controller.UpdateUserPrimaryRole("owner", new() { PrimaryRole = "Reader" })).Result).Value);
        fixture.Profile.VerifyAll(); fixture.Profile.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OwnerAndCurrentRepositoryAdministratorCanUpdatePreferencesWithoutIncomingRoles(bool administrator)
    {
        using var fixture = new Fixture(); fixture.SignIn(administrator ? "admin" : "target");
        fixture.Profile.Setup(x => x.UpdateUserPreferencesAsync("target", "{}")).ReturnsAsync(true);
        Assert.Equal(true, Assert.IsType<OkObjectResult>((await fixture.Controller.UpdateUserPreferences("target", new() { PreferencesJson = "{}" })).Result).Value);
        fixture.Profile.VerifyAll(); fixture.Profile.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RevokingRepositoryAuthorityRefusesTheNextControllerActionBeforeProfileStorage()
    {
        using var fixture = new Fixture(); fixture.SignIn("admin", claimedAdministrator: true);
        fixture.Db.UserRoles.Remove((await fixture.Db.UserRoles.FindAsync("admin", "administrator"))!); await fixture.Db.SaveChangesAsync();
        await Forbidden(() => fixture.Controller.UpdateUserPreferences("target", new() { PreferencesJson = "{}" }));
        fixture.Profile.VerifyNoOtherCalls();
    }

    private static async Task Forbidden(Func<Task> action) => Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(action)).Kind);
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection database = new("Data Source=:memory:;Foreign Keys=True");
        private readonly ServiceProvider services;
        public RepositoryDbContext Db { get; }
        public Mock<IUserProfileService> Profile { get; } = new(MockBehavior.Strict);
        public HttpContextAccessor Http { get; } = new() { HttpContext = new DefaultHttpContext() };
        public UserProfileController Controller { get; }
        public Fixture()
        {
            database.Open(); Db = new Context(new DbContextOptionsBuilder<Context>().UseSqlite(database).Options); Db.Database.EnsureCreated();
            foreach (var id in new[] { "owner", "target", "admin", "other" }) Db.Users.Add(new OilGasUser { Id = id, UserName = id, NormalizedUserName = id.ToUpperInvariant(), IsActive = true });
            Db.Roles.Add(new IdentityRole { Id = "administrator", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
            Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "admin", RoleId = "administrator" }); Db.SaveChanges();
            var registration = new ServiceCollection(); registration.AddLogging(); registration.AddSingleton(Db);
            registration.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>(); services = registration.BuildServiceProvider();
            var assignments = new RepositoryRoleAssignmentService(Db, services.GetRequiredService<UserManager<OilGasUser>>(), services.GetRequiredService<RoleManager<IdentityRole>>(), NullLogger<RepositoryRoleAssignmentService>.Instance);
            // Explicit resolved-account boundary in this fixture; production injects the auth owner's existing resolver.
            var account = new RepositoryRequestAccount(Http, principal => principal.FindFirst("party_id")?.Value, new RepositoryAccessService(Db), assignments);
            Controller = new(Profile.Object, account) { ControllerContext = new() { HttpContext = Http.HttpContext } };
        }
        public void SignIn(string? actor, bool authenticated = true, bool claimedAdministrator = false)
        {
            var claims = new List<Claim>(); if (actor is not null) claims.Add(new("party_id", actor));
            if (claimedAdministrator) claims.Add(new(ClaimTypes.Role, "Administrator"));
            Http.HttpContext!.User = new(new ClaimsIdentity(claims, authenticated ? "test" : null));
        }
        public void Dispose() { services.Dispose(); Db.Dispose(); database.Dispose(); }
    }
}
