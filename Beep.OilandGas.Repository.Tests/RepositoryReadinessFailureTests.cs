using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

/// <summary>
/// OILGAS-CATCH-01: a readiness check that cannot read the repository answers Unavailable — the setup gate and the health
/// check act on that — and reports the failure, which it used to write only to a log line.
/// </summary>
public class RepositoryReadinessFailureTests
{
    [Fact]
    public async Task AFailedReadAnswersUnavailableAndIsReported()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options;
        using (var setup = new TestContext(options))
            setup.Database.EnsureCreated();
        using var context = new TestContext(new DbContextOptionsBuilder<TestContext>()
            .UseSqlite(connection).AddInterceptors(new FailingCommands()).Options);
        var failures = new RecordingFailureReporter();

        var readiness = await new RepositoryReadinessService(context, failures).CheckAsync();

        Assert.Equal(RepositoryReadiness.Unavailable, readiness);
        var report = Assert.Single(failures.Reports);
        Assert.IsType<SqliteException>(report.Exception);
        Assert.Equal(FailureSeverity.Error, report.Severity);
    }

    [Fact]
    public async Task AReadableRepositoryReportsNothing()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var context = new TestContext(new DbContextOptionsBuilder<TestContext>().UseSqlite(connection).Options);
        context.Database.EnsureCreated();
        var failures = new RecordingFailureReporter();

        Assert.Equal(RepositoryReadiness.BootstrapRequired,
            await new RepositoryReadinessService(context, failures).CheckAsync());
        Assert.Empty(failures.Reports);
    }

    private sealed class TestContext(DbContextOptions<TestContext> options) : RepositoryDbContext(options);

    /// <summary>Refuses every command, as a database that has gone away would.</summary>
    private sealed class FailingCommands : DbCommandInterceptor
    {
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default) =>
            throw new SqliteException("test failure", 1);

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default) =>
            throw new SqliteException("test failure", 1);
    }
}
