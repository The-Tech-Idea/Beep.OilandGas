using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Beep.OilandGas.ApiService.Tests.Infrastructure;

namespace Beep.OilandGas.ApiService.Tests;

public class RepositoryUserProfileTests(ITestOutputHelper output)
{
    [LocalDbModuleFact]
    public async Task ProfilesPersistAsIdentityExtensionsAndNeverGrantRoles()
    {
        var database = $"BeepOilGas_Profiles_{Guid.NewGuid():N}";
        output.WriteLine($"Retained profile test database: {database}");
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new SqlServerRepositoryDbContext(options);
        await db.Database.MigrateAsync();
        var user = new OilGasUser { Id = "user", UserName = "user", NormalizedUserName = "USER", IsActive = true };
        db.Users.Add(user);
        db.Roles.AddRange(new IdentityRole { Id = "reader", Name = "Reader", NormalizedName = "READER" },
            new IdentityRole { Id = "admin", Name = "Administrator", NormalizedName = "ADMINISTRATOR" });
        var membership = new IdentityUserRole<string> { UserId = user.Id, RoleId = "reader" };
        db.UserRoles.Add(membership);
        await db.SaveChangesAsync();
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("party_id", "user") }, "test"))
        } };
        using var fixture = await ProfileFixture.CreateAsync(db, accessor);
        var service = fixture.Service;
        Assert.NotNull(await service.GetUserProfileAsync("user"));
        Assert.Empty(await db.Set<AppUserExtension>().ToListAsync()); // Reads never create profiles.
        Assert.True(await service.UpdateUserPreferencesAsync("user", "{\"density\":\"compact\"}"));
        Assert.True(await service.UpdateUserPreferredLayoutAsync("user", "DefaultLayout"));
        Assert.True(await service.UpdateUserPrimaryRoleAsync("user", "reader"));
        await service.RecordUserLoginAsync("user");
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => service.UpdateUserPrimaryRoleAsync("user", "Administrator"))).Kind);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => service.UpdateUserPreferencesAsync("user", "invalid json"))).Kind);
        Assert.Single(await db.UserRoles.ToListAsync());
        ((ClaimsIdentity)accessor.HttpContext.User.Identity!).AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
        var controller = new Beep.OilandGas.ApiService.Controllers.AccessControl.UserProfileController(service, fixture.Account)
        {
            ControllerContext = new() { HttpContext = accessor.HttpContext }
        };
        // Direct action invocation has no HTTP exception middleware; assert the service's typed refusal.
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => controller.UpdateUserPreferences("user",
            new() { PreferencesJson = "invalid json" }))).Kind);
        Assert.Equal(RefusalKind.Invalid, (await Assert.ThrowsAsync<RefusalException>(() => controller.UpdateUserPrimaryRole("user",
            new() { PrimaryRole = "Administrator" }))).Kind);

        await using (var reopened = new SqlServerRepositoryDbContext(options))
        {
            using var reopenedFixture = await ProfileFixture.CreateAsync(reopened, accessor);
            var profile = await reopenedFixture.Service.GetUserProfileAsync("user");
            Assert.NotNull(profile);
            Assert.Equal("Reader", profile.PrimaryRole);
            Assert.Equal(new[] { "Reader" }, profile.Roles);
            Assert.Equal("DefaultLayout", profile.PreferredLayout);
            Assert.Equal("{\"density\":\"compact\"}", profile.UserPreferences);
            Assert.NotNull(profile.LastLoginDate);
            var extension = await reopened.Set<AppUserExtension>().SingleAsync();
            Assert.Equal("user", extension.ChangedBy);
            Assert.Equal("reader", extension.PrimaryRoleId);
        }
        db.UserRoles.Remove(membership);
        await db.SaveChangesAsync();
        using var refreshedFixture = await ProfileFixture.CreateAsync(db, accessor);
        service = refreshedFixture.Service;
        Assert.Null((await service.GetUserProfileAsync("user"))!.PrimaryRole);
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => service.GetUserProfileAsync("user"))).Kind);
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => service.UpdateUserPreferredLayoutAsync("user", "OtherLayout"))).Kind);
        Assert.Equal(RefusalKind.Forbidden, (await Assert.ThrowsAsync<RefusalException>(() => service.GetUserProfileAsync("missing"))).Kind);
        Assert.Single(await db.Set<AppUserExtension>().ToListAsync());
    }

    private sealed class ProfileFixture(ServiceProvider provider, RepositoryUserProfileService service, RepositoryRequestAccount account) : IDisposable
    {
        public RepositoryUserProfileService Service => service;
        public RepositoryRequestAccount Account => account;
        public static async Task<ProfileFixture> CreateAsync(RepositoryDbContext context, IHttpContextAccessor accessor)
        {
            var registration = new ServiceCollection();
            registration.AddLogging(); registration.AddSingleton(context);
            registration.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            registration.AddScoped<RepositoryRoleAssignmentService>();
            var provider = registration.BuildServiceProvider();
            var account = new RepositoryRequestAccount(accessor, principal => principal.FindActingUserId(),
                new RepositoryAccessService(context), provider.GetRequiredService<RepositoryRoleAssignmentService>());
            var company = await TestCompanyModules.CreateAsync(context, "user");
            return new(provider, new(context, new RepositoryApplicationAuthorizationReader(company),
                new UpperInvariantLookupNormalizer(), account, company, Moq.Mock.Of<Beep.OilandGas.Models.Core.Interfaces.IPPDM39DataService>()), account);
        }
        public void Dispose() => provider.Dispose();
    }
}
