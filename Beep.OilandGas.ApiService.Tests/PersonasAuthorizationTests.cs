using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class PersonasAuthorizationTests
{
    [Fact]
    public async Task NotificationPersonaReaderUsesActiveLocalProfileAndRechecksChanges()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        var user = new OilGasUser { Id = "local-user", UserName = "user", IsActive = true };
        var persona = new AppPersona { Code = "ENGINEER", Name = "Engineer", IsActive = true };
        db.Users.Add(user);
        db.Add(persona);
        db.Add(new AppUserPersona { UserId = user.Id, PersonaCode = persona.Code, ChangedBy = user.Id });
        await db.SaveChangesAsync();
        var reader = new Beep.OilandGas.ApiService.Hubs.RepositoryNotificationPersonaReader(db);
        Assert.True(await reader.IsCurrentPersonaAsync(user.Id, persona.Code));
        Assert.False(await reader.IsCurrentPersonaAsync("external-subject", persona.Code));
        Assert.False(await reader.IsCurrentPersonaAsync(user.Id, "OTHER"));
        persona.IsActive = false;
        await db.SaveChangesAsync();
        Assert.False(await reader.IsCurrentPersonaAsync(user.Id, persona.Code));
        persona.IsActive = true;
        user.IsActive = false;
        await db.SaveChangesAsync();
        Assert.False(await reader.IsCurrentPersonaAsync(user.Id, persona.Code));
    }

    [Theory]
    [InlineData("other", false)]
    [InlineData(null, true)]
    public async Task UnauthorizedAccountActionsCannotReachRepository(string? actor, bool admin)
    {
        var controller = Create(null!, actor, admin);
        Assert.IsType<ForbidResult>(await controller.Profile("owner", default));
        Assert.IsType<ForbidResult>(await controller.SaveProfile("owner", new(null), default));
        Assert.IsType<ForbidResult>(await controller.Preferences("owner", "ENGINEER", default));
        Assert.IsType<ForbidResult>(await controller.SavePreference("owner", "ENGINEER", "view", new("value"), default));
    }

    [Theory]
    [InlineData("owner", false)]
    [InlineData("admin", true)]
    public async Task OwnerOrAdministratorWritesWithLocalActorAndDetectsStaleVersion(string actor, bool admin)
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
        db.Database.EnsureCreated();
        db.Users.Add(new OilGasUser { Id = "owner", UserName = "owner" });
        db.Users.Add(new OilGasUser { Id = "admin", UserName = "admin" });
        await db.SaveChangesAsync();
        var controller = Create(new RepositoryPersonaService(db), actor, admin);
        var response = Assert.IsType<OkObjectResult>(await controller.SaveProfile("owner", new(null, Locale: "en"), default));
        var profile = Assert.IsType<AppUserPersona>(response.Value);
        Assert.Equal("owner", profile.UserId);
        Assert.Equal(actor, profile.ChangedBy);
        Assert.IsType<ConflictObjectResult>(await controller.SaveProfile("owner", new(null, Locale: "fr"), default));
        Assert.Equal("en", (await db.Set<AppUserPersona>().SingleAsync()).Locale);
        Assert.Single(await db.Set<AppPersonaAudit>().ToListAsync());
    }

    [Fact]
    public void CatalogChangesRequireStandardAdministratorRole()
    {
        var attribute = Assert.Single(typeof(PersonasController).GetMethod(nameof(PersonasController.SaveCatalog))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Administrator", attribute.Roles);
    }

    private static PersonasController Create(RepositoryPersonaService service, string? actor, bool admin)
    {
        var claims = new List<Claim>();
        if (actor is not null) claims.Add(new Claim("party_id", actor));
        if (admin) claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
        return new(service, new Beep.OilandGas.ApiService.Tests.Infrastructure.RecordingFailureReporter()) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext
        { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "repository")) } } };
    }
    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);
}
