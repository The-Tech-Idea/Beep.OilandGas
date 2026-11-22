using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-01: the connection catalog over BeepDM. Proves a local file database is created through the
/// catalog, that the connection is persisted, and that a fresh runtime reopens the same database.
/// </summary>
public sealed class ConnectionCatalogIntegrationTests
{
    private const string ConnectionName = "dat01-local";

    [Fact]
    public async Task Creates_reopens_and_reads_a_local_database()
    {
        var root = Path.Combine(OgPaths.DataRoot, "dat01-" + Guid.NewGuid().ToString("n")[..8]);
        var configDirectory = Path.Combine(root, "Beep");
        var dataDirectory = Path.Combine(root, "Data");
        var databaseFile = Path.Combine(dataDirectory, "dat01.db");
        var reporter = new RecordingFailureReporter();

        try
        {
            // ---- first run: create the database through the catalog -------------------------------
            using (var host = OgBeepHost.Create(new OgBeepHostOptions(configDirectory, "BeepOilGasDat01")))
            {
                // Discovery is idempotent: a driver already registered by LoadAssemblies() is skipped.
                OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

                var registered = host.Beep.Config_editor!.DataDriversClasses!;
                Assert.Contains(registered, driver =>
                    driver.DatasourceType == TheTechIdea.Beep.Utilities.DataSourceType.SqlLite);

                var localDriver = OgDriverDiscovery.FirstLocalDriver(host.Beep);
                Assert.NotNull(localDriver);

                var catalog = new BeepDmConnectionCatalog(host, reporter);

                var created = await catalog.EnsureLocalAsync(
                    new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "dat01.db"));

                Assert.True(created.Success, $"{created.Message} (reference {created.Reference})");
                Assert.Equal(OgConnectionState.Ok, created.Value!.State);
                Assert.True(File.Exists(databaseFile), $"'{databaseFile}' was not created.");

                // Seed one table so the reopened file has something real to read.
                using (var seed = new SqliteConnection($"Data Source={databaseFile}"))
                {
                    seed.Open();
                    using var command = seed.CreateCommand();
                    command.CommandText =
                        """
                        CREATE TABLE IF NOT EXISTS PPDM_PROBE (ID TEXT PRIMARY KEY, NAME TEXT);
                        INSERT OR REPLACE INTO PPDM_PROBE (ID, NAME) VALUES ('1', 'probe');
                        """;
                    command.ExecuteNonQuery();
                }

                var tested = await catalog.TestAsync(ConnectionName);
                Assert.True(tested.Success, tested.Message);
                Assert.Equal(OgConnectionState.Ok, tested.Value!.State);
            }

            // ---- second run: a brand new runtime must find and open the same database --------------
            using (var host = OgBeepHost.Create(new OgBeepHostOptions(configDirectory, "BeepOilGasDat01")))
            {
                OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);
                var catalog = new BeepDmConnectionCatalog(host, reporter);

                var listed = await catalog.ListAsync();
                Assert.True(listed.Success, listed.Message);
                Assert.Contains(listed.Value!, connection =>
                    string.Equals(connection.Name, ConnectionName, StringComparison.OrdinalIgnoreCase));

                var reopened = await catalog.TestAsync(ConnectionName);
                Assert.True(reopened.Success, reopened.Message);
                Assert.Equal(OgConnectionState.Ok, reopened.Value!.State);
            }

            // ---- the file is a real SQLite database and kept its data ------------------------------
            using (var verify = new SqliteConnection($"Data Source={databaseFile}"))
            {
                verify.Open();
                using var read = verify.CreateCommand();
                read.CommandText = "SELECT NAME FROM PPDM_PROBE WHERE ID = '1';";
                Assert.Equal("probe", (string?)read.ExecuteScalar());
            }

            Assert.Empty(reporter.Failures);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task Testing_an_unknown_connection_is_refused_with_a_reason()
    {
        var root = Path.Combine(OgPaths.DataRoot, "dat01-neg-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat01"));
            var catalog = new BeepDmConnectionCatalog(host, reporter);

            var result = await catalog.TestAsync("no-such-connection");

            Assert.False(result.Success);
            Assert.Equal("connection-not-found", result.Reference);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task Deleting_a_connection_that_is_open_is_refused()
    {
        var root = Path.Combine(OgPaths.DataRoot, "dat01-inuse-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat01"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "inuse.db"));

            Assert.True(created.Success, created.Message);

            // The source is open after creation, so removal must be refused rather than orphaning it.
            var removed = await catalog.RemoveAsync(ConnectionName);

            Assert.False(removed.Success);
            Assert.Equal("connection-in-use", removed.Reference);

            var listed = await catalog.ListAsync();
            Assert.Contains(listed.Value!, connection => connection.Name == ConnectionName);
        }
        finally
        {
            TryDelete(root);
        }
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A provider may still hold the file; the test root is disposable either way.
        }
    }

    /// <summary>Records what the catalog reported, so the test can assert nothing was swallowed.</summary>
    private sealed class RecordingFailureReporter : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{severity}: {operation} — {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
