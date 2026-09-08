using Beep.OilandGas.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryPersonaTests
{
    [Fact]
    public async Task ProfileAndPreferenceWritesAreAuditedWithoutGrantingRoles()
    {
        using var fixture = new Fixture();
        var catalog = await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer", DefaultRoute: "/dashboard"), "user");
        Assert.Equal("ENGINEER", Assert.Single(await fixture.Service.CatalogAsync()).Code);
        var profile = await fixture.Service.SaveAsync("user", new(catalog.Code, "en"), "user");
        var stamp = profile.ConcurrencyStamp;
        await fixture.Service.SaveAsync("user", new(catalog.Code, "fr", ConcurrencyStamp: stamp), "user");
        Assert.Equal("fr", (await fixture.Service.GetAsync("user"))!.Locale);
        Assert.NotEqual(stamp, profile.ConcurrencyStamp);
        var preference = await fixture.Service.SavePreferenceAsync("user", catalog.Code, "density", new("compact"), "user");
        Assert.Equal("compact", Assert.Single(await fixture.Service.PreferencesAsync("user", catalog.Code)).Value);
        Assert.Equal(4, await fixture.Db.Set<AppPersonaAudit>().CountAsync());
        Assert.Empty(await fixture.Db.UserRoles.ToListAsync());
        Assert.Empty(await fixture.Db.RoleClaims.ToListAsync());
        Assert.Empty(await fixture.Db.UserClaims.ToListAsync());
    }

    [Fact]
    public async Task StaleUpdatesDoNotOverwriteSettingsOrCreateAuditRows()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("ENGINEER", new("Engineer"), "user");
        await fixture.Service.SaveAsync("user", new("ENGINEER", "en"), "user");
        await fixture.Service.SavePreferenceAsync("user", "ENGINEER", "density", new("compact"), "user");
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => fixture.Service.SaveAsync("user", new("ENGINEER", "fr"), "user"));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => fixture.Service.SavePreferenceAsync("user", "ENGINEER", "density", new("wide"), "user"));
        Assert.Equal("en", (await fixture.Service.GetAsync("user"))!.Locale);
        Assert.Equal(3, await fixture.Db.Set<AppPersonaAudit>().CountAsync());
    }

    [Fact]
    public async Task UnknownUsersAndInactivePersonasAreRejected()
    {
        using var fixture = new Fixture();
        await fixture.Service.SaveCatalogAsync("INACTIVE", new("Inactive", IsActive: false), "user");
        Assert.Empty(await fixture.Service.CatalogAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SaveAsync("missing", new(null), "user"));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SaveAsync("user", new("INACTIVE"), "user"));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.SaveAsync("user", new(null), "missing"));
        Assert.Empty(await fixture.Db.Set<AppUserPersona>().ToListAsync());
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
        public Fixture()
        {
            connection.Open();
            Db = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
            Db.Database.EnsureCreated();
            Db.Users.Add(new OilGasUser { Id = "user", UserName = "user", IsActive = true });
            Db.SaveChanges();
            Service = new RepositoryPersonaService(Db);
        }
        public void Dispose() { Db.Dispose(); connection.Dispose(); }
    }
}
