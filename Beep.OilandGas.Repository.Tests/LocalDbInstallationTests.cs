using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Security.Claims;

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
        Assert.Equal(6, (await db.Database.GetAppliedMigrationsAsync()).Count());
        await db.Database.MigrateAsync();
        Assert.Empty(await db.Database.GetPendingMigrationsAsync());

        var bootstrap = scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>();
        Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync("https://integration.invalid", "first"));
        Assert.Equal(BootstrapOutcome.Registered, await bootstrap.BootstrapAsync("https://integration.invalid", "second"));
        Assert.Equal(BootstrapOutcome.AlreadyCompleted, await bootstrap.BootstrapAsync("https://integration.invalid", "first"));
        var exactSubjectAccess = new RepositoryAccessService(db);
        Assert.Null(await exactSubjectAccess.GetAccessAsync("https://integration.invalid", "FIRST"));
        Assert.Null(await exactSubjectAccess.GetAccessAsync("https://integration.invalid", "first "));
        Assert.Equal(BootstrapOutcome.NotAllowed, await bootstrap.BootstrapAsync("https://integration.invalid", "FIRST"));
        Assert.Equal(BootstrapOutcome.NotAllowed, await bootstrap.BootstrapAsync("https://integration.invalid", "first "));
        Assert.Contains("Administrator", (await exactSubjectAccess.GetAccessAsync("https://integration.invalid", "first"))!.Roles);

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

        // A separately built provider cannot satisfy these reads from the bootstrap context's tracking cache.
        var reopenedServices = new ServiceCollection();
        reopenedServices.AddLogging();
        reopenedServices.AddOilGasRepository(configuration);
        await using var reopened = reopenedServices.BuildServiceProvider();
        await using (var readScope = reopened.CreateAsyncScope())
        {
            var repository = readScope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
            await repository.Database.OpenConnectionAsync();
            using var tablesQuery = repository.Database.GetDbConnection().CreateCommand();
            tablesQuery.CommandText = "SELECT name FROM sys.tables ORDER BY name";
            using var tableRows = await tablesQuery.ExecuteReaderAsync();
            var tables = new List<string>();
            while (await tableRows.ReadAsync()) tables.Add(tableRows.GetString(0));
            Assert.Equal(new[] { "AspNetRoleClaims", "AspNetRoles", "AspNetUserClaims", "AspNetUserLogins",
                "AspNetUserRoles", "AspNetUsers", "AspNetUserTokens" }.OrderBy(x => x, StringComparer.Ordinal),
                tables.Where(x => x.StartsWith("AspNet", StringComparison.Ordinal)).OrderBy(x => x, StringComparer.Ordinal));
            Assert.DoesNotContain("OIL_COMPOSITION", tables);
            Assert.DoesNotContain("WELL", tables);
            await tableRows.CloseAsync();

            var access = readScope.ServiceProvider.GetRequiredService<IRepositoryAccessService>();
            var persistedAdmin = await access.GetAccessAsync("https://integration.invalid", "first");
            Assert.NotNull(persistedAdmin);
            Assert.Equal(first.Id, persistedAdmin.UserId);
            Assert.Equal(new[] { "Administrator" }, persistedAdmin.Roles);
            var persistedUser = await access.GetAccessAsync("https://integration.invalid", "second");
            Assert.NotNull(persistedUser);
            Assert.Empty(persistedUser.Roles);

            var roleManager = readScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var readerRole = new IdentityRole("Reader");
            Assert.True((await roleManager.CreateAsync(readerRole)).Succeeded);
            Assert.True((await roleManager.AddClaimAsync(readerRole, new Claim("permission", "module.read"))).Succeeded);
            var userManager = readScope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            var member = (await userManager.FindByIdAsync(second.Id))!;
            Assert.True((await userManager.AddToRoleAsync(member, "Reader")).Succeeded);
        }

        await using (var revokeScope = reopened.CreateAsyncScope())
        {
            var access = revokeScope.ServiceProvider.GetRequiredService<IRepositoryAccessService>();
            var assigned = await access.GetAccessAsync("https://integration.invalid", "second");
            Assert.NotNull(assigned);
            Assert.Equal(new[] { "Reader" }, assigned.Roles);
            Assert.Equal(new[] { "module.read" }, assigned.Permissions);
            var userManager = revokeScope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            var member = (await userManager.FindByIdAsync(second.Id))!;
            Assert.True((await userManager.RemoveFromRoleAsync(member, "Reader")).Succeeded);
        }

        await using (var deactivateScope = reopened.CreateAsyncScope())
        {
            var access = deactivateScope.ServiceProvider.GetRequiredService<IRepositoryAccessService>();
            var revoked = await access.GetAccessAsync("https://integration.invalid", "second");
            Assert.NotNull(revoked);
            Assert.Empty(revoked.Roles);
            Assert.Empty(revoked.Permissions);
            var userManager = deactivateScope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            var member = (await userManager.FindByIdAsync(second.Id))!;
            Assert.True((await userManager.AddToRoleAsync(member, "Reader")).Succeeded);
            member.IsActive = false;
            Assert.True((await userManager.UpdateAsync(member)).Succeeded);
        }

        await using (var finalScope = reopened.CreateAsyncScope())
        {
            var access = finalScope.ServiceProvider.GetRequiredService<IRepositoryAccessService>();
            var disabled = await access.GetAccessAsync("https://integration.invalid", "second");
            Assert.NotNull(disabled);
            Assert.False(disabled.IsActive);
            Assert.Empty(disabled.Roles);
            Assert.Empty(disabled.Permissions);
            Assert.Equal(BootstrapOutcome.NotAllowed,
                await finalScope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>()
                    .BootstrapAsync("https://integration.invalid", "second"));
        }
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
