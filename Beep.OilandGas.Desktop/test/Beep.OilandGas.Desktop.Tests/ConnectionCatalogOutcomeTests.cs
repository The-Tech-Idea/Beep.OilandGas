using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.Utilities;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-01 AC-2 and AC-3: a connection test reports a distinct code for each outcome a person can act on,
/// and a local database is created by a driver that advertises local creation — not by whichever driver
/// happens to be first.
/// </summary>
public sealed class ConnectionCatalogOutcomeTests
{
    [Theory]
    // AC-2: credentials vs reachability vs a missing file vs a missing driver.
    [InlineData("Login failed for user 'sa'.", OgConnectionState.CredentialsRejected, OgConnectionFailureClassifier.CodeCredentialsRejected)]
    [InlineData("authentication failed for user 'probe'", OgConnectionState.CredentialsRejected, OgConnectionFailureClassifier.CodeCredentialsRejected)]
    [InlineData("Access denied (401)", OgConnectionState.CredentialsRejected, OgConnectionFailureClassifier.CodeCredentialsRejected)]
    [InlineData("password authentication failed", OgConnectionState.CredentialsRejected, OgConnectionFailureClassifier.CodeCredentialsRejected)]
    [InlineData("A network-related error occurred while establishing a connection", OgConnectionState.HostUnreachable, OgConnectionFailureClassifier.CodeHostUnreachable)]
    [InlineData("No such host is known", OgConnectionState.HostUnreachable, OgConnectionFailureClassifier.CodeHostUnreachable)]
    [InlineData("connection refused (10061)", OgConnectionState.HostUnreachable, OgConnectionFailureClassifier.CodeHostUnreachable)]
    [InlineData("The operation timed out", OgConnectionState.HostUnreachable, OgConnectionFailureClassifier.CodeHostUnreachable)]
    [InlineData("something the provider invented", OgConnectionState.Failed, OgConnectionFailureClassifier.CodeFailed)]
    [InlineData(null, OgConnectionState.Failed, OgConnectionFailureClassifier.CodeFailed)]
    public void A_providers_failure_is_placed_in_one_actionable_outcome(
        string? message,
        OgConnectionState expected,
        string expectedCode)
    {
        var (state, code) = OgConnectionFailureClassifier.Classify(message);

        Assert.Equal(expected, state);
        Assert.Equal(expectedCode, code);
    }

    [Fact]
    public void A_missing_driver_or_file_beats_whatever_the_message_says()
    {
        var missingDriver = OgConnectionFailureClassifier.Classify("login failed", driverMissing: true);
        Assert.Equal(OgConnectionState.DriverMissing, missingDriver.State);

        var missingFile = OgConnectionFailureClassifier.Classify("login failed", databaseMissing: true);
        Assert.Equal(OgConnectionState.MissingDatabase, missingFile.State);
    }

    [Fact]
    public void Every_state_has_a_sentence()
    {
        foreach (var state in Enum.GetValues<OgConnectionState>())
        {
            Assert.False(string.IsNullOrWhiteSpace(OgConnectionFailureClassifier.Describe(state)));
        }
    }

    [Fact]
    public void A_driver_that_advertises_local_creation_and_is_not_in_memory_is_chosen()
    {
        // BeepDM's list really does contain local in-memory drivers before SQLite; picking the first
        // "CreateLocal" one would choose a cache that creates no file at all.
        var cache = Driver(DataSourceType.CachedMemory, "TheTechIdea.Beep.CachedMemory", local: true, inMemory: true);
        var sqlite = Driver(DataSourceType.SqlLite, "System.Data.SQLite", local: true, inMemory: false);

        var chosen = OgDriverDiscovery.SelectLocalDriver([cache, sqlite]);

        Assert.NotNull(chosen);
        Assert.Equal(DataSourceType.SqlLite, chosen!.DatasourceType);
    }

    [Fact]
    public void A_driver_that_does_not_advertise_local_creation_is_not_used_as_a_fallback()
    {
        var remote = Driver(DataSourceType.SqlServer, "System.Data.SqlClient", local: false, inMemory: false);

        Assert.Null(OgDriverDiscovery.SelectLocalDriver([remote]));
    }

    [Fact]
    public void A_local_in_memory_driver_alone_is_not_a_local_database()
    {
        var cache = Driver(DataSourceType.InMemoryCache, "TheTechIdea.Beep.InMemoryCache", local: true, inMemory: true);

        Assert.Null(OgDriverDiscovery.SelectLocalDriver([cache]));
    }

    [Fact]
    public async Task The_local_database_is_provisioned_by_a_driver_that_advertises_local_creation()
    {
        var root = Path.Combine(OgPaths.DataRoot, "ac3-" + Guid.NewGuid().ToString("n")[..8]);
        var connectionName = "ac3-local";

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAc3"));
            OgBundledDrivers.Register(host.Beep);

            var advertising = OgDriverDiscovery.SelectLocalDriver(host.Beep.Config_editor?.DataDriversClasses);
            Assert.NotNull(advertising);
            Assert.True(advertising!.CreateLocal, "AC-3: the selected driver must advertise local creation.");
            Assert.False(advertising.InMemory, "AC-3: the selected driver must not be an in-memory cache.");

            var catalog = new BeepDmConnectionCatalog(host, new QuietFailureReporter());
            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(connectionName, Path.Combine(root, "Data"), "ac3.db"));

            Assert.True(created.Success, $"{created.Message} (reference {created.Reference})");
            Assert.Equal(OgConnectionState.Ok, created.Value!.State);
            Assert.Contains("advertises local creation", created.Value.Detail!, StringComparison.Ordinal);
            Assert.Null(created.Reference);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task A_connection_whose_database_file_is_gone_reports_its_own_code()
    {
        var root = Path.Combine(OgPaths.DataRoot, "ac2-missing-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAc2"));
            OgBundledDrivers.Register(host.Beep);

            var catalog = new BeepDmConnectionCatalog(host, new QuietFailureReporter());
            Add(host, new ConnectionProperties
            {
                ConnectionName = "gone",
                DatabaseType = DataSourceType.SqlLite,
                Category = DatasourceCategory.RDBMS,
                FilePath = Path.Combine(root, "Data"),
                FileName = "gone.db",
                IsFile = true,
                IsLocal = true,
            });

            var tested = await catalog.TestAsync("gone");

            Assert.False(tested.Success);
            Assert.Equal(OgConnectionFailureClassifier.CodeDatabaseMissing, tested.Reference);
            Assert.Contains("missing", tested.Message!, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task A_connection_whose_driver_is_not_installed_reports_its_own_code()
    {
        var root = Path.Combine(OgPaths.DataRoot, "ac2-driver-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAc2"));
            OgBundledDrivers.Register(host.Beep);

            var catalog = new BeepDmConnectionCatalog(host, new QuietFailureReporter());
            Add(host, new ConnectionProperties
            {
                ConnectionName = "orphan",
                DatabaseType = DataSourceType.NONE,
                Category = DatasourceCategory.NONE,
            });

            var tested = await catalog.TestAsync("orphan");

            Assert.False(tested.Success);
            Assert.Equal(OgConnectionFailureClassifier.CodeDriverMissing, tested.Reference);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public async Task An_unknown_connection_is_still_its_own_code()
    {
        var root = Path.Combine(OgPaths.DataRoot, "ac2-none-" + Guid.NewGuid().ToString("n")[..8]);

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAc2"));
            var catalog = new BeepDmConnectionCatalog(host, new QuietFailureReporter());

            var tested = await catalog.TestAsync("never-configured");

            Assert.False(tested.Success);
            Assert.Equal("connection-not-found", tested.Reference);
        }
        finally
        {
            TryDelete(root);
        }
    }

    private static ConnectionDriversConfig Driver(
        DataSourceType type,
        string package,
        bool local,
        bool inMemory) =>
        new()
        {
            DatasourceType = type,
            DatasourceCategory = DatasourceCategory.RDBMS,
            PackageName = package,
            classHandler = type + "DataSource",
            CreateLocal = local,
            InMemory = inMemory,
        };

    private static void Add(OgBeepHost host, ConnectionProperties properties)
    {
        host.Beep.Config_editor!.DataConnections ??= [];
        host.Beep.Config_editor.DataConnections.Add(properties);
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
            // A provider may still hold a file; the test root is disposable either way.
        }
    }

    private sealed class QuietFailureReporter : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }
}
