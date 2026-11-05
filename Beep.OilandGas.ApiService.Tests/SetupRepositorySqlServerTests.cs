using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
#if SETUP_SQLSERVER_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SetupRepositorySqlServerTests(ITestOutputHelper output)
{
    private sealed class MutableSource : IRepositoryConnectionSource
    {
        public RepositoryConnectionSettings? Current;
        public RepositoryConnectionSettings? Resolve() => Current;
    }
    private sealed class PriorPersona
    {
        public string UserId { get; set; } = "";
        public string ChangedBy { get; set; } = "";
        public DateTime ChangedUtc { get; set; }
        public string ConcurrencyStamp { get; set; } = "";
    }
    private sealed class PriorPreference
    {
        public string UserId { get; set; } = "";
        public string PersonaCode { get; set; } = "";
        public string ViewKey { get; set; } = "";
        public string ChangedBy { get; set; } = "";
        public DateTime ChangedUtc { get; set; }
        public string ConcurrencyStamp { get; set; } = "";
    }
    private sealed class PriorPersonaContext(DbContextOptions<PriorPersonaContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PriorPersona>(entity => { entity.ToTable("APP_USER_PERSONA"); entity.HasKey(x => x.UserId); });
            builder.Entity<PriorPreference>(entity => { entity.ToTable("APP_PERSONA_PREFERENCE"); entity.HasKey(x => new { x.UserId, x.PersonaCode, x.ViewKey }); });
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExistingAccountWidePersonaStopsCompanyScopeMigrationWithoutChangingData(bool preference)
    {
        var name = "BeepOilGas_PersonaPreflight_" + Guid.NewGuid().ToString("N");
        var connection = $"Server=(localdb)\\MSSQLLocalDB;Database={name};Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>().UseSqlServer(connection).Options;
        await using var db = new SqlServerRepositoryDbContext(options);
        try
        {
            await db.GetService<IMigrator>().MigrateAsync("20261028124456_CompanyMembershipRetirement");
            db.Users.Add(new OilGasUser { Id = "user", UserName = "user", IsActive = true });
            if (preference) db.Add(new AppPersona { Code = "ENGINEER", Name = "Engineer" });
            await db.SaveChangesAsync();
            var oldOptions = new DbContextOptionsBuilder<PriorPersonaContext>().UseSqlServer(connection).Options;
            await using var prior = new PriorPersonaContext(oldOptions);
            if (preference) prior.Add(new PriorPreference { UserId = "user", PersonaCode = "ENGINEER", ViewKey = "view", ChangedBy = "user", ChangedUtc = DateTime.UtcNow, ConcurrencyStamp = Guid.NewGuid().ToString() });
            else prior.Add(new PriorPersona { UserId = "user", ChangedBy = "user", ChangedUtc = DateTime.UtcNow, ConcurrencyStamp = Guid.NewGuid().ToString() });
            await prior.SaveChangesAsync();
            var service = new SetupRepositoryMigrationService(db, new RepositoryReadinessService(db, Mock.Of<IFailureReporter>()));
            var error = await Assert.ThrowsAsync<PersonaScopeTransitionRequiredException>(() => service.PlanAsync(() => Task.CompletedTask, default));
            Assert.Contains("Account-wide persona settings exist", error.Message);
            if (preference) Assert.Single(await prior.Set<PriorPreference>().AsNoTracking().ToListAsync());
            else Assert.Single(await prior.Set<PriorPersona>().AsNoTracking().ToListAsync());
            Assert.Contains(await db.Database.GetPendingMigrationsAsync(), name => name.EndsWith("_CompanyPersonaScope", StringComparison.Ordinal));
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }

    [Fact]
    public async Task MigratedSqlServerStoresDifferentPersonaSettingsForTwoCompanies()
    {
        var name = "BeepOilGas_PersonaCompany_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={name};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new SqlServerRepositoryDbContext(options);
        try
        {
            await db.Database.MigrateAsync();
            db.Users.Add(new OilGasUser { Id = "user", UserName = "user", IsActive = true });
            db.Add(new AppTenant { TenantId = "tenant", Name = "Tenant" });
            db.AddRange(new AppCompany { TenantId = "tenant", BusinessAssociateId = "kuwait" }, new AppCompany { TenantId = "tenant", BusinessAssociateId = "norway" });
            db.Add(new AppPersona { Code = "ENGINEER", Name = "Engineer" });
            foreach (var (company, locale, value) in new[] { ("kuwait", "ar-KW", "compact"), ("norway", "nb-NO", "wide") })
            {
                db.Add(new AppUserPersona { UserId = "user", TenantId = "tenant", BusinessAssociateId = company, PersonaCode = "ENGINEER", Locale = locale });
                db.Add(new AppPersonaPreference { UserId = "user", TenantId = "tenant", BusinessAssociateId = company, PersonaCode = "ENGINEER", ViewKey = "density", Value = value });
            }
            await db.SaveChangesAsync(); db.ChangeTracker.Clear();
            Assert.Equal("ar-KW", (await db.Set<AppUserPersona>().SingleAsync(x => x.BusinessAssociateId == "kuwait")).Locale);
            Assert.Equal("nb-NO", (await db.Set<AppUserPersona>().SingleAsync(x => x.BusinessAssociateId == "norway")).Locale);
            Assert.Equal("compact", (await db.Set<AppPersonaPreference>().SingleAsync(x => x.BusinessAssociateId == "kuwait")).Value);
            Assert.Equal("wide", (await db.Set<AppPersonaPreference>().SingleAsync(x => x.BusinessAssociateId == "norway")).Value);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }

    [Fact]
    public async Task ConnectionSelectionIsCapturedOncePerScope()
    {
        var source = new MutableSource();
        var services = new ServiceCollection(); services.AddLogging();
        services.AddSingleton<IRepositoryConnectionSource>(source);
        services.AddSingleton(Mock.Of<IFailureReporter>());
        services.AddOilGasRepository(new ConfigurationBuilder().Build());
        using var root = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var first = root.CreateScope();
        Assert.Equal(RepositoryReadiness.Unavailable, await first.ServiceProvider.GetRequiredService<IRepositoryReadinessService>().CheckAsync());
        source.Current = new("SQLSERVER", "Server=(localdb)\\MSSQLLocalDB;Database=FirstTarget;Integrated Security=true");
        Assert.Throws<InvalidOperationException>(() => first.ServiceProvider.GetRequiredService<RepositoryDbContext>());
        using var second = root.CreateScope();
        var db = second.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        source.Current = new("SQLSERVER", "Server=(localdb)\\MSSQLLocalDB;Database=SecondTarget;Integrated Security=true");
        Assert.Contains("FirstTarget", db.Database.GetConnectionString());
        using var third = root.CreateScope();
        Assert.Contains("SecondTarget", third.ServiceProvider.GetRequiredService<RepositoryDbContext>().Database.GetConnectionString());
        source.Current = new("unsupported", "Data Source=unknown");
        using var fourth = root.CreateScope();
        Assert.Equal(RepositoryReadiness.Unavailable, await fourth.ServiceProvider.GetRequiredService<IRepositoryReadinessService>().CheckAsync());
        Assert.Throws<InvalidOperationException>(() => fourth.ServiceProvider.GetRequiredService<RepositoryDbContext>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReviewedIdentityMigrationsInstallFreshLocalDbRepository(bool databaseAlreadyExists)
    {
        var name = "BeepOilGas_Setup_" + Guid.NewGuid().ToString("N");
        output.WriteLine("Retained isolated test database: " + name);
        var options = new DbContextOptionsBuilder<SqlServerRepositoryDbContext>()
            .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={name};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new SqlServerRepositoryDbContext(options);
        if (databaseAlreadyExists) await db.GetService<IRelationalDatabaseCreator>().CreateAsync();
        var readiness = new RepositoryReadinessService(db, Mock.Of<IFailureReporter>());
        Assert.Equal(databaseAlreadyExists ? RepositoryReadiness.MigrationRequired : RepositoryReadiness.Unavailable, await readiness.CheckAsync());
        var service = new SetupRepositoryMigrationService(db, readiness);
        var plan = await service.PlanAsync(() => Task.CompletedTask, default);
        Assert.Equal(!databaseAlreadyExists, plan.CreatesDatabase);
        Assert.NotEmpty(plan.PendingMigrations);
        var result = await service.ApplyAsync(new(plan.Provider, plan.TargetToken, plan.CreatesDatabase, plan.PendingMigrations), () => Task.CompletedTask, default);
        Assert.Equal(RepositoryReadiness.BootstrapRequired, result.Readiness);
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(plan.PendingMigrations, await db.Database.GetAppliedMigrationsAsync());
        await using var reopened = new SqlServerRepositoryDbContext(options);
        Assert.Equal(RepositoryReadiness.BootstrapRequired, await new RepositoryReadinessService(reopened, Mock.Of<IFailureReporter>()).CheckAsync());
    }
}
#endif
