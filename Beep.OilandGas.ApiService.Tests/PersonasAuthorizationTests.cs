using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Identity;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
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
        var scope = new CompanyScope("tenant", "company");
        db.Add(new AppTenant { TenantId = scope.TenantId, Name = "Tenant" });
        db.Add(new AppCompany { TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId });
        db.Users.Add(user);
        db.Add(persona);
        db.Add(new AppUserPersona { UserId = user.Id, TenantId = scope.TenantId, BusinessAssociateId = scope.BusinessAssociateId, PersonaCode = persona.Code, ChangedBy = user.Id });
        await db.SaveChangesAsync();
        var access = new Mock<IRepositoryAccessService>(); access.Setup(x => x.GetCompanyAccessAsync(user.Id, scope, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(scope, ["Worker"], []));
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await new RepositoryCompanyContextResolver(access.Object).ResolveAsync(user.Id, scope))!);
        var reader = new Beep.OilandGas.ApiService.Hubs.RepositoryNotificationPersonaReader(db, tenant);
        Assert.True(await reader.IsCurrentPersonaAsync(user.Id, persona.Code));
        Assert.False(await reader.IsCurrentPersonaAsync("external-subject", persona.Code));
        Assert.False(await reader.IsCurrentPersonaAsync(user.Id, "OTHER"));
        var other = new CompanyScope("tenant", "other");
        db.Add(new AppCompany { TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId });
        db.Add(new AppPersona { Code = "OTHER", Name = "Other", IsActive = true });
        db.Add(new AppUserPersona { UserId = user.Id, TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId, PersonaCode = "OTHER", ChangedBy = user.Id });
        await db.SaveChangesAsync();
        access.Setup(x => x.GetCompanyAccessAsync(user.Id, other, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(other, ["Worker"], []));
        var otherTenant = new RepositoryTenantContext(); otherTenant.SelectCompany((await new RepositoryCompanyContextResolver(access.Object).ResolveAsync(user.Id, other))!);
        var otherReader = new Beep.OilandGas.ApiService.Hubs.RepositoryNotificationPersonaReader(db, otherTenant);
        Assert.True(await otherReader.IsCurrentPersonaAsync(user.Id, "OTHER"));
        Assert.False(await otherReader.IsCurrentPersonaAsync(user.Id, "ENGINEER"));
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
        Assert.IsType<ForbidResult>(await controller.Profile("owner", null!, null!, default));
        Assert.IsType<ForbidResult>(await controller.SaveProfile("owner", new(null), null!, null!, default));
        Assert.IsType<ForbidResult>(await controller.Preferences("owner", "ENGINEER", null!, null!, default));
        Assert.IsType<ForbidResult>(await controller.SavePreference("owner", "ENGINEER", "view", new("value"), null!, null!, default));
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
        db.Add(new AppTenant { TenantId = "tenant", Name = "Tenant" });
        db.Add(new AppCompany { TenantId = "tenant", BusinessAssociateId = "company" });
        await db.SaveChangesAsync();
        var controller = Create(new RepositoryPersonaService(db), actor, admin);
        var scope = new CompanyScope("tenant", "company"); var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetCompanyAccessAsync(It.IsAny<string>(), scope, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string user, CompanyScope company, CancellationToken _) => new RepositoryCompanyAccess(company, ["Worker"], []));
        access.Setup(x => x.GetAccessAsync("admin", It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryUserAccess("admin", true, ["Administrator"], []));
        var selected = await new RepositoryCompanyContextResolver(access.Object).ResolveAsync(actor, scope);
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany(selected!);
        var response = Assert.IsType<OkObjectResult>(await controller.SaveProfile("owner", new(null, Locale: "en"), tenant, access.Object, default));
        var profile = Assert.IsType<AppUserPersona>(response.Value);
        Assert.Equal("owner", profile.UserId);
        Assert.Equal(actor, profile.ChangedBy);
        Assert.IsType<ConflictObjectResult>(await controller.SaveProfile("owner", new(null, Locale: "fr"), tenant, access.Object, default));
        Assert.Equal("en", (await db.Set<AppUserPersona>().SingleAsync()).Locale);
        Assert.Single(await db.Set<AppPersonaAudit>().ToListAsync());
    }

    [Fact]
    public async Task ProfileEndpointRejectsActiveWorkspaceNotAssignedToCompanyRoles()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); connection.Open();
        using var db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options); db.Database.EnsureCreated();
        db.Users.Add(new OilGasUser { Id = "owner", UserName = "owner", IsActive = true });
        db.Add(new AppTenant { TenantId = "tenant", Name = "Tenant" });
        db.Add(new AppCompany { TenantId = "tenant", BusinessAssociateId = "company" });
        db.Add(new AppPersona { Code = "OTHER", Name = "Other", IsActive = true }); await db.SaveChangesAsync();
        var scope = new CompanyScope("tenant", "company"); var access = new Mock<IRepositoryAccessService>();
        access.Setup(x => x.GetCompanyAccessAsync("owner", scope, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(scope, ["Worker"], []));
        var tenant = new RepositoryTenantContext(); tenant.SelectCompany((await new RepositoryCompanyContextResolver(access.Object).ResolveAsync("owner", scope))!);
        var controller = Create(new RepositoryPersonaService(db), "owner", false);
        Assert.IsType<ForbidResult>(await controller.SaveProfile("owner", new("OTHER"), tenant, access.Object, default));
        Assert.Empty(await db.Set<AppUserPersona>().AsNoTracking().ToListAsync());
        Assert.Empty(await db.Set<AppPersonaAudit>().AsNoTracking().ToListAsync());
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
