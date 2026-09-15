using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;
using Xunit.Abstractions;

namespace Beep.OilandGas.Repository.Tests;

public class RegistrationProfileTests(ITestOutputHelper output)
{
    [LocalDbFact]
    public async Task RegistrationMetadataDoesNotMergeAccountsOrOverwriteExistingProfiles()
    {
        var database = $"BeepOilGas_RegistrationProfile_{Guid.NewGuid():N}";
        output.WriteLine($"Retained registration profile database: {database}");
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
        var bootstrap = scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>();
        const string issuer = "https://profile-test.invalid";
        Assert.Equal(BootstrapOutcome.Created, await bootstrap.BootstrapAsync(issuer, "first", profile:
            new("  First Person  ", " shared@example.invalid ", true)));
        Assert.Equal(BootstrapOutcome.Registered, await bootstrap.BootstrapAsync(issuer, "second", profile:
            new("Second Person", "shared@example.invalid", false)));
        Assert.Equal(BootstrapOutcome.AlreadyCompleted, await bootstrap.BootstrapAsync(issuer, "first", profile:
            new("Overwrite Attempt", "different@example.invalid", false)));

        db.ChangeTracker.Clear();
        var accounts = await db.Users.ToListAsync();
        Assert.Equal(2, accounts.Count);
        Assert.All(accounts, user => Assert.Equal("shared@example.invalid", user.Email));
        Assert.All(accounts, user => Assert.Null(user.PasswordHash));
        var adminId = (await db.Bootstrap.SingleAsync()).AdministratorUserId;
        Assert.True(accounts.Single(x => x.Id == adminId).EmailConfirmed);
        Assert.False(accounts.Single(x => x.Id != adminId).EmailConfirmed);
        var adminProfile = await db.Set<AppUserExtension>().SingleAsync(x => x.UserId == adminId);
        Assert.Equal("First Person", adminProfile.FullName);
        Assert.Equal(adminId, adminProfile.ChangedBy);
        Assert.Single(await db.UserRoles.ToListAsync());

        var invalidProfiles = new[]
        {
            new ExternalRegistrationProfile(new string('x', 1001), "not-an-email", true),
            new ExternalRegistrationProfile("name\nwith-control", "Display <user@example.invalid>", true),
            new ExternalRegistrationProfile(" ", new string('x', 257), true)
        };
        for (var i = 0; i < invalidProfiles.Length; i++)
        {
            Assert.Equal(BootstrapOutcome.Registered, await bootstrap.BootstrapAsync(issuer, $"invalid-{i}", profile: invalidProfiles[i]));
        }
        db.ChangeTracker.Clear();
        var incomplete = await db.Users.Where(x => x.Email == null).ToListAsync();
        Assert.Equal(3, incomplete.Count);
        Assert.All(incomplete, user => Assert.False(user.EmailConfirmed));
        foreach (var user in incomplete)
            Assert.Null((await db.Set<AppUserExtension>().SingleAsync(x => x.UserId == user.Id)).FullName);
        Assert.Single(await db.UserRoles.ToListAsync());
    }
}
