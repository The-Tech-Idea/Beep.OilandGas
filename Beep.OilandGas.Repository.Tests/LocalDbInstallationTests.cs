using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Beep.OilandGas.Repository.Tests;

public sealed class LocalDbInstallationTests(ITestOutputHelper output)
{
    [LocalDbFact]
    public async Task CompetingFirstRegistrationsLeaveExactlyOneAdministrator()
    {
        var database = $"BeepOilGas_Integration_{Guid.NewGuid():N}";
        output.WriteLine($"Retained race-test database: {database}");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = "SqlServer",
            ["Repository:ConnectionString"] = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true"
        }).Build();
        var barrier = new EmptyUsersBarrier();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOilGasRepository(configuration);
        services.AddDbContext<SqlServerRepositoryDbContext>(options => options.AddInterceptors(barrier));
        await using var provider = services.BuildServiceProvider();
        await using (var setup = provider.CreateAsyncScope())
            await setup.ServiceProvider.GetRequiredService<RepositoryDbContext>().Database.MigrateAsync();
        barrier.Enabled = true;

        async Task<BootstrapOutcome?> Register(string subject)
        {
            await using var scope = provider.CreateAsyncScope();
            try
            {
                return await scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>()
                    .BootstrapAsync("https://integration.invalid", subject);
            }
            catch (Exception ex) when (ex is DbUpdateException or InvalidOperationException)
            {
                output.WriteLine($"Competing registration outcome: {ex.GetType().Name}");
                return null;
            }
        }

        var results = await Task.WhenAll(Register("contender-a"), Register("contender-b"));
        Assert.Equal(2, barrier.Arrivals);
        Assert.Single(results.Where(x => x == BootstrapOutcome.Created));
        barrier.Enabled = false;
        await using (var verify = provider.CreateAsyncScope())
        {
            var db = verify.ServiceProvider.GetRequiredService<RepositoryDbContext>();
            Assert.Equal(1, await db.Bootstrap.CountAsync());
            Assert.Equal(1, await db.UserRoles.CountAsync());
            Assert.Equal(1, await db.Users.CountAsync());
            Assert.Equal(1, await db.UserLogins.CountAsync());
            Assert.Equal(1, await db.Set<AppUserExtension>().CountAsync());
        }
        var retries = new[] { await Register("contender-a"), await Register("contender-b") };
        Assert.Contains(BootstrapOutcome.Registered, retries);
        Assert.Contains(BootstrapOutcome.AlreadyCompleted, retries);
        await using var final = provider.CreateAsyncScope();
        var finalDb = final.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        Assert.Equal(2, await finalDb.Users.CountAsync());
        Assert.Equal(1, await finalDb.UserRoles.CountAsync());
        Assert.Equal(1, await finalDb.Set<AppUserRoleExtension>().CountAsync());
    }

    private sealed class EmptyUsersBarrier : DbCommandInterceptor
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int arrivals;
        public bool Enabled { get; set; }
        public int Arrivals => arrivals;
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command,
            CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            if (Enabled && command.CommandText.Contains("EXISTS", StringComparison.Ordinal) &&
                command.CommandText.Contains("FROM [AspNetUsers]", StringComparison.Ordinal))
            {
                if (Interlocked.Increment(ref arrivals) == 2) release.TrySetResult();
                await release.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
            }
            return result;
        }
    }

    [LocalDbFact]
    public async Task FreshInstallationAndFirstRegistrationUseRealSqlServerStores()
    {
        var database = $"BeepOilGas_Integration_{Guid.NewGuid():N}";
        output.WriteLine($"Retained test database: {database}");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Repository:Provider"] = "SqlServer",
            ["Repository:ConnectionString"] = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true"
        }).Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOilGasRepository(configuration);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
        await db.Database.MigrateAsync();
        Assert.Equal(4, (await db.Database.GetAppliedMigrationsAsync()).Count());
        await db.Database.MigrateAsync();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());

        var bootstrap = scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>();
        Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync("https://integration.invalid", "first"));
        Assert.Equal(BootstrapOutcome.Registered, await bootstrap.BootstrapAsync("https://integration.invalid", "second"));
        Assert.Equal(BootstrapOutcome.AlreadyCompleted, await bootstrap.BootstrapAsync("https://integration.invalid", "first"));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
        var first = await users.FindByLoginAsync(RepositoryBootstrapService.ExternalLoginProvider("https://integration.invalid"), "first");
        var second = await users.FindByLoginAsync(RepositoryBootstrapService.ExternalLoginProvider("https://integration.invalid"), "second");
        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.True(await users.IsInRoleAsync(first, "Administrator"));
        Assert.Empty(await users.GetRolesAsync(second));
        Assert.Null(first.PasswordHash);
        Assert.Null(second.PasswordHash);
        Assert.Equal(2, await db.Users.CountAsync());
        Assert.Equal(1, await db.UserRoles.CountAsync());
        Assert.Equal(first.Id, (await db.Bootstrap.SingleAsync()).AdministratorUserId);
        Assert.Equal(2, await db.Set<AppUserExtension>().CountAsync());
        Assert.Equal(1, await db.Set<AppUserRoleExtension>().CountAsync());
        Assert.Equal(RepositoryReadiness.Ready,
            await scope.ServiceProvider.GetRequiredService<IRepositoryReadinessService>().CheckAsync());
    }
}

public sealed class LocalDbFactAttribute : FactAttribute
{
    public LocalDbFactAttribute()
    {
        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("OILGAS_TEST_LOCALDB") != "1")
            Skip = "Set OILGAS_TEST_LOCALDB=1 on Windows to create a retained, isolated LocalDB integration database.";
    }
}
