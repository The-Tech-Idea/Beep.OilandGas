using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public class RepositoryBootstrapTests
{
    [Fact]
    public async Task FirstRegistrationCreatesPasswordlessAdminAndReplayIsNoOp()
    {
        using var fixture = new Fixture();
        var result = await fixture.Service.BootstrapAsync("https://issuer.example", "first-admin");
        Assert.Equal(BootstrapOutcome.Created, result);
        var user = Assert.Single(await fixture.Context.Users.ToListAsync());
        Assert.Null(user.PasswordHash);
        Assert.True(await fixture.Users.IsInRoleAsync(user, "Administrator"));
        var login = Assert.Single(await fixture.Context.UserLogins.ToListAsync());
        Assert.Equal("first-admin", login.ProviderKey);
        Assert.Equal(RepositoryBootstrapService.ExternalLoginProvider("https://issuer.example"), login.LoginProvider);
        Assert.Equal(user.Id, (await fixture.Context.Bootstrap.SingleAsync()).AdministratorUserId);
        Assert.Equal(BootstrapOutcome.AlreadyCompleted,
            await fixture.Service.BootstrapAsync("https://issuer.example", "first-admin"));
        Assert.Equal(1, await fixture.Context.Users.CountAsync());
        Assert.Equal(1, await fixture.Context.UserRoles.CountAsync());
    }

    [Theory]
    [InlineData("", "first-admin")]
    [InlineData("https://issuer.example", "")]
    public async Task MissingIdentityCannotWrite(string issuer, string subject)
    {
        using var fixture = new Fixture();
        Assert.Equal(BootstrapOutcome.NotAllowed, await fixture.Service.BootstrapAsync(issuer, subject));
        Assert.Empty(await fixture.Context.Users.ToListAsync());
        Assert.Empty(await fixture.Context.Bootstrap.ToListAsync());
    }

    [Fact]
    public async Task LaterRegistrationCreatesOrdinaryUserWithoutAdminRole()
    {
        using var fixture = new Fixture();
        Assert.Equal(BootstrapOutcome.Created,
            await fixture.Service.BootstrapAsync("https://issuer.example", "first-admin"));
        Assert.Equal(BootstrapOutcome.Registered,
            await fixture.Service.BootstrapAsync("https://issuer.example", "second-user"));
        var user = await fixture.Users.FindByLoginAsync(
            RepositoryBootstrapService.ExternalLoginProvider("https://issuer.example"), "second-user");
        Assert.NotNull(user);
        Assert.False(await fixture.Users.IsInRoleAsync(user, "Administrator"));
        Assert.Equal(2, await fixture.Context.Users.CountAsync());
        Assert.Equal(1, await fixture.Context.UserRoles.CountAsync());
    }

    [Fact]
    public async Task CompletionFailureRollsBackAccountRoleAndMembership()
    {
        // The bootstrap row's insert fails (an EF interceptor, not SQL — rule 5a); everything written before it rolls back.
        using var fixture = new Fixture(failBootstrapInsert: true);
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            fixture.Service.BootstrapAsync("https://issuer.example", "first-admin"));
        fixture.Context.ChangeTracker.Clear();
        Assert.Empty(await fixture.Context.Users.ToListAsync());
        Assert.Empty(await fixture.Context.Roles.ToListAsync());
        Assert.Empty(await fixture.Context.UserRoles.ToListAsync());
        Assert.Empty(await fixture.Context.UserLogins.ToListAsync());
        Assert.Empty(await fixture.Context.Bootstrap.ToListAsync());
    }

    [Fact]
    public async Task DisabledExistingUserCannotBootstrap()
    {
        using var fixture = new Fixture();
        var user = new OilGasUser { UserName = "disabled", IsActive = false };
        Assert.True((await fixture.Users.CreateAsync(user)).Succeeded);
        Assert.True((await fixture.Users.AddLoginAsync(user, new UserLoginInfo(
            RepositoryBootstrapService.ExternalLoginProvider("https://issuer.example"), "first-admin", "OIDC"))).Succeeded);
        Assert.Equal(BootstrapOutcome.NotAllowed,
            await fixture.Service.BootstrapAsync("https://issuer.example", "first-admin"));
        Assert.Empty(await fixture.Context.UserRoles.ToListAsync());
        Assert.Empty(await fixture.Context.Bootstrap.ToListAsync());
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);

    /// <summary>Refuses any save that adds the bootstrap row, as a database failure would.</summary>
    private sealed class FailBootstrapInsert : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
            eventData.Context!.ChangeTracker.Entries<RepositoryBootstrap>().Any(entry => entry.State == EntityState.Added)
                ? throw new DbUpdateException("test failure")
                : base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    [Fact]
    public async Task AccessLookupUsesTheAccountIdAndReflectsRevocation()
    {
        using var fixture = new Fixture();
        await fixture.Service.BootstrapAsync("https://issuer.example", "first-admin");
        var user = await fixture.Users.FindByLoginAsync(
            RepositoryBootstrapService.ExternalLoginProvider("https://issuer.example"), "first-admin");
        Assert.NotNull(user);
        var access = new RepositoryAccessService(fixture.Context);
        var result = await access.GetAccessAsync(user.Id);
        Assert.NotNull(result);
        Assert.Contains("Administrator", result.Roles);
        Assert.Null(await access.GetAccessAsync("no-such-account"));
        Assert.True((await fixture.Users.RemoveFromRoleAsync(user, "Administrator")).Succeeded);
        Assert.Empty((await access.GetAccessAsync(user.Id))!.Roles);
        user.IsActive = false;
        Assert.True((await fixture.Users.UpdateAsync(user)).Succeeded);
        Assert.False((await access.GetAccessAsync(user.Id))!.IsActive);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        private readonly ServiceProvider _provider;
        private readonly IServiceScope _scope;
        public RepositoryDbContext Context { get; }
        public UserManager<OilGasUser> Users { get; }
        public RepositoryBootstrapService Service { get; }

        public Fixture(bool failBootstrapInsert = false)
        {
            _connection.Open();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<TestContext>(options =>
            {
                options.UseSqlite(_connection);
                if (failBootstrapInsert) options.AddInterceptors(new FailBootstrapInsert());
            });
            services.AddScoped<RepositoryDbContext>(sp => sp.GetRequiredService<TestContext>());
            services.AddIdentityCore<OilGasUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<RepositoryDbContext>();
            services.AddScoped<RepositoryBootstrapService>();
            _provider = services.BuildServiceProvider();
            _scope = _provider.CreateScope();
            Context = _scope.ServiceProvider.GetRequiredService<RepositoryDbContext>();
            Context.Database.EnsureCreated();
            Users = _scope.ServiceProvider.GetRequiredService<UserManager<OilGasUser>>();
            Service = _scope.ServiceProvider.GetRequiredService<RepositoryBootstrapService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _provider.Dispose();
            _connection.Dispose();
        }
    }
}
