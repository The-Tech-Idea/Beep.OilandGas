using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryPersonaTests
{
    [Fact]
    public async Task RecordParameterValidationAlsoProtectsDirectServiceCalls()
    {
        var service = new RepositoryPersonaService(null!);
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            service.SaveCatalogAsync("ENGINEER", new(""), "user"));
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            service.SaveAsync("user", new(null, Locale: new string('x', 33)), "user", new("tenant", "company"), null!));
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            service.SavePreferenceAsync("user", new("tenant", "company"), "ENGINEER", "view", new(new string('x', 4001)), "user", null!));
    }

    [Fact]
    public async Task ProfileAndPreferenceWritesAreAuditedWithoutGrantingRoles()
    {
        using var fixture = new Fixture();
        var catalog = await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer", DefaultRoute: "/dashboard"), "user");
        Assert.Equal("ENGINEER", Assert.Single(await fixture.Service.CatalogAsync()).Code);
        var profile = await fixture.Save("user", new(catalog.Code, "en"), "user");
        var stamp = profile.ConcurrencyStamp;
        await fixture.Save("user", new(catalog.Code, "fr", ConcurrencyStamp: stamp), "user");
        Assert.Equal("fr", (await fixture.Service.GetAsync("user", fixture.Company))!.Locale);
        Assert.NotEqual(stamp, profile.ConcurrencyStamp);
        var preference = await fixture.SavePreference("user", catalog.Code, "density", new("compact"), "user");
        Assert.Equal("compact", Assert.Single(await fixture.Service.PreferencesAsync("user", fixture.Company, catalog.Code)).Value);
        Assert.Equal(4, await fixture.Db.Set<AppPersonaAudit>().CountAsync());
        Assert.Empty(await fixture.Db.UserRoles.ToListAsync());
        Assert.Single(await fixture.Db.RoleClaims.ToListAsync());
        Assert.Empty(await fixture.Db.UserClaims.ToListAsync());
    }

    [Fact]
    public async Task StaleUpdatesDoNotOverwriteSettingsOrCreateAuditRows()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer"), "user");
        await fixture.Save("user", new("ENGINEER", "en"), "user");
        await fixture.SavePreference("user", "ENGINEER", "density", new("compact"), "user");
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => fixture.Save("user", new("ENGINEER", "fr"), "user"));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => fixture.SavePreference("user", "ENGINEER", "density", new("wide"), "user"));
        Assert.Equal("en", (await fixture.Service.GetAsync("user", fixture.Company))!.Locale);
        Assert.Equal(3, await fixture.Db.Set<AppPersonaAudit>().CountAsync());
    }

    [Fact]
    public async Task UnknownUsersAndInactivePersonasAreRejected()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("INACTIVE", new("Inactive", IsActive: false), "user");
        Assert.Empty(await fixture.Service.CatalogAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save("missing", new(null), "user"));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save("user", new("INACTIVE"), "user"));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Save("user", new(null), "missing"));
        Assert.Empty(await fixture.Db.Set<AppUserPersona>().ToListAsync());
    }

    [Fact]
    public async Task ProfileCannotSelectWorkspaceNotAssignedToCompanyRoles()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("OTHER", new("Other"), "user");
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Save("user", new("OTHER"), "user"));
        Assert.Empty(await fixture.Db.Set<AppUserPersona>().AsNoTracking().ToListAsync());
        Assert.DoesNotContain(await fixture.Db.Set<AppPersonaAudit>().AsNoTracking().ToListAsync(), row => row.Action == "ProfileSaved");
    }

    [Fact]
    public async Task OneAccountKeepsIndependentCompanyProfilesAndPreferences()
    {
        using var fixture = new Fixture(); var other = new CompanyScope("tenant", "other");
        fixture.Db.Add(new AppCompany { TenantId = other.TenantId, BusinessAssociateId = other.BusinessAssociateId });
        await fixture.Db.SaveChangesAsync();
        fixture.Access.Setup(x => x.GetCompanyAccessAsync("user", other, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(other, ["Worker"], []));
        await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer"), "user");
        await fixture.Save("user", new("ENGINEER", "en"), "user");
        await fixture.Service.SaveAsync("user", new("ENGINEER", "ar-KW"), "user", other, fixture.Access.Object);
        await fixture.SavePreference("user", "ENGINEER", "density", new("compact"), "user");
        await fixture.Service.SavePreferenceAsync("user", other, "ENGINEER", "density", new("wide"), "user", fixture.Access.Object);
        Assert.Equal("en", (await fixture.Service.GetAsync("user", fixture.Company))!.Locale);
        Assert.Equal("ar-KW", (await fixture.Service.GetAsync("user", other))!.Locale);
        Assert.Equal("compact", Assert.Single(await fixture.Service.PreferencesAsync("user", fixture.Company, "ENGINEER")).Value);
        Assert.Equal("wide", Assert.Single(await fixture.Service.PreferencesAsync("user", other, "ENGINEER")).Value);
    }

    [Fact]
    public async Task RevokedCompanyMembershipRollsBackProfileAndAudit()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer"), "user");
        var reads = 0;
        fixture.Access.Setup(x => x.GetCompanyAccessAsync("user", fixture.Company, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++reads <= 3 ? new RepositoryCompanyAccess(fixture.Company, ["Worker"], []) : null);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Save("user", new("ENGINEER"), "user"));
        Assert.Empty(await fixture.Db.Set<AppUserPersona>().AsNoTracking().ToListAsync());
        Assert.DoesNotContain(await fixture.Db.Set<AppPersonaAudit>().AsNoTracking().ToListAsync(), row => row.Action == "ProfileSaved");
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("//example.com")]
    [InlineData("/\\example.com")]
    public async Task PersonaRoutesCannotRedirectOutsideApplication(string route)
    {
        using var fixture = new Fixture();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer", DefaultRoute: route), "user"));
        Assert.Empty(await fixture.Db.Set<AppPersona>().ToListAsync());
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);
    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        public RepositoryDbContext Db { get; }
        public RepositoryPersonaService Service { get; }
        public CompanyScope Company { get; } = new("tenant", "company");
        public Mock<IRepositoryAccessService> Access { get; } = new();
        public Task<AppUserPersona> Save(string user, PersonaProfileUpdate request, string actor) => Service.SaveAsync(user, request, actor, Company, Access.Object);
        public Task<AppPersonaPreference> SavePreference(string user, string code, string view, PersonaPreferenceUpdate request, string actor) => Service.SavePreferenceAsync(user, Company, code, view, request, actor, Access.Object);
        public Fixture()
        {
            connection.Open();
            Db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
            Db.Database.EnsureCreated();
            Db.Users.Add(new OilGasUser { Id = "user", UserName = "user", IsActive = true });
            Db.Add(new AppTenant { TenantId = Company.TenantId, Name = "Tenant" });
            Db.Add(new AppCompany { TenantId = Company.TenantId, BusinessAssociateId = Company.BusinessAssociateId });
            Db.Roles.Add(new IdentityRole { Id = "worker", Name = "Worker", NormalizedName = "WORKER" });
            Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "worker", ClaimType = RepositoryPersonaService.WorkspaceClaimType, ClaimValue = "ENGINEER" });
            Db.SaveChanges();
            Service = new RepositoryPersonaService(Db);
            Access.Setup(x => x.GetCompanyAccessAsync("user", Company, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(Company, ["Worker"], []));
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); }
    }
}
