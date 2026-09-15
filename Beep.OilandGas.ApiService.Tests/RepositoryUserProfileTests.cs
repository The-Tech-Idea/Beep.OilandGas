using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

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
            User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "actor") }, "test"))
        } };
        RepositoryUserProfileService Service(RepositoryDbContext context) => new(context,
            new RepositoryApplicationAuthorizationReader(context), new UpperInvariantLookupNormalizer(), accessor);
        var service = Service(db);
        Assert.NotNull(await service.GetUserProfileAsync("user"));
        Assert.Empty(await db.Set<AppUserExtension>().ToListAsync()); // Reads never create profiles.
        Assert.True(await service.UpdateUserPreferencesAsync("user", "{\"density\":\"compact\"}"));
        Assert.True(await service.UpdateUserPreferredLayoutAsync("user", "DefaultLayout"));
        Assert.True(await service.UpdateUserPrimaryRoleAsync("user", "reader"));
        await service.RecordUserLoginAsync("user");
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateUserPrimaryRoleAsync("user", "Administrator"));
        await Assert.ThrowsAnyAsync<System.Text.Json.JsonException>(() => service.UpdateUserPreferencesAsync("user", "invalid json"));
        Assert.Single(await db.UserRoles.ToListAsync());
        ((ClaimsIdentity)accessor.HttpContext.User.Identity!).AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
        var controller = new Beep.OilandGas.ApiService.Controllers.AccessControl.UserProfileController(service)
        {
            ControllerContext = new() { HttpContext = accessor.HttpContext }
        };
        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>((await controller.UpdateUserPreferences("user",
            new() { PreferencesJson = "invalid json" })).Result);
        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>((await controller.UpdateUserPrimaryRole("user",
            new() { PrimaryRole = "Administrator" })).Result);

        await using (var reopened = new SqlServerRepositoryDbContext(options))
        {
            var profile = await Service(reopened).GetUserProfileAsync("user");
            Assert.NotNull(profile);
            Assert.Equal("Reader", profile.PrimaryRole);
            Assert.Equal(new[] { "Reader" }, profile.Roles);
            Assert.Equal("DefaultLayout", profile.PreferredLayout);
            Assert.Equal("{\"density\":\"compact\"}", profile.UserPreferences);
            Assert.NotNull(profile.LastLoginDate);
            var extension = await reopened.Set<AppUserExtension>().SingleAsync();
            Assert.Equal("actor", extension.ChangedBy);
            Assert.Equal("reader", extension.PrimaryRoleId);
        }
        db.UserRoles.Remove(membership);
        await db.SaveChangesAsync();
        Assert.Null((await service.GetUserProfileAsync("user"))!.PrimaryRole);
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.Null(await service.GetUserProfileAsync("user"));
        Assert.False(await service.UpdateUserPreferredLayoutAsync("user", "OtherLayout"));
        Assert.Null(await service.GetUserProfileAsync("missing"));
        Assert.Single(await db.Set<AppUserExtension>().ToListAsync());
    }
}
