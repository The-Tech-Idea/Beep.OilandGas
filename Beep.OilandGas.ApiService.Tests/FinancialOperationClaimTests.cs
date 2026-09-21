using Beep.OilandGas.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class FinancialOperationClaimTests
{
    private sealed class Context(DbContextOptions<Context> options) : RepositoryDbContext(options);
    private static Context Open(string connectionString) => new(new DbContextOptionsBuilder<Context>().UseSqlite(connectionString).Options);

    [Fact]
    public async Task IndependentConnectionsArbitrateOneOwnerAndRejectStaleRelease()
    {
        var file = Path.Combine(Path.GetTempPath(), "royalty-claim-" + Guid.NewGuid() + ".db");
        var connection = new SqliteConnectionStringBuilder { DataSource = file, Pooling = false }.ToString();
        try
        {
            await using (var setup = Open(connection))
            {
                await setup.Database.EnsureCreatedAsync();
                Assert.True(await new FinancialOperationClaimStore(setup).InitializeAsync("royalty:one", "setup"));
            }
            async Task<TheTechIdea.Data.OilGas.FinancialClaimHandle?> Acquire(string owner)
            {
                await using var db = Open(connection);
                return await new FinancialOperationClaimStore(db).TryAcquireAsync("royalty:one", 0, owner);
            }
            var attempts = await Task.WhenAll(Task.Run(() => Acquire("first")), Task.Run(() => Acquire("second")));
            var winner = Assert.Single(attempts, x => x is not null)!;
            Assert.Single(attempts, x => x is null);
            await using var inspect = Open(connection);
            var store = new FinancialOperationClaimStore(inspect);
            Assert.Equal(winner.OwnerId, (await store.ReadAsync("royalty:one"))!.OwnerId);
            Assert.False(await store.InitializeAsync("royalty:one", "another"));
            Assert.Null(await store.TryAcquireAsync("royalty:one", winner.Version, "third"));
            Assert.False(await store.ReleaseAsync(winner with { OwnerId = "wrong" }));
            Assert.False(await store.ReleaseAsync(winner with { Token = Guid.NewGuid().ToString() }));
            Assert.False(await store.ReleaseAsync(winner with { Version = 0 }));
            Assert.True(await store.ReleaseAsync(winner));
            Assert.False(await store.ReleaseAsync(winner));
            Assert.Null(await store.TryAcquireAsync("royalty:one", 0, "stale"));
            var next = await store.TryAcquireAsync("royalty:one", 2, "next");
            Assert.NotNull(next);
            Assert.Equal(3, next.Version);
            Assert.False(await store.ReleaseAsync(winner));
            Assert.Equal(next.Token, (await store.ReadAsync("royalty:one"))!.Token);
        }
        finally { if (File.Exists(file)) File.Delete(file); }
    }

    [Fact]
    public async Task DisposingOwnerDoesNotExpireClaimOrPermitTakeover()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<Context>().UseSqlite(connection).Options;
        await using (var owner = new Context(options))
        {
            await owner.Database.EnsureCreatedAsync();
            var store = new FinancialOperationClaimStore(owner);
            await store.InitializeAsync("pending", "actor");
            Assert.NotNull(await store.TryAcquireAsync("pending", 0, "actor"));
        }
        await using var recovery = new Context(options);
        var reader = new FinancialOperationClaimStore(recovery);
        Assert.NotNull((await reader.ReadAsync("pending"))!.Token);
        Assert.Null(await reader.TryAcquireAsync("pending", 1, "recovery"));
        Assert.Null(await reader.TryAcquireAsync("missing", 0, "actor"));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MaxValue)]
    public async Task InvalidVersionsFailBeforeDatabaseAccess(long version)
    {
        await using var db = Open("Data Source=:memory:");
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new FinancialOperationClaimStore(db).TryAcquireAsync("key", version, "actor"));
    }
}
