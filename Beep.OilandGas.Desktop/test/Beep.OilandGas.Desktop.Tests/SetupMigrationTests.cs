using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Settings;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-11 (gap G-8): an installation created by an earlier build is brought forward by re-saving its documents and
/// stamping the version last; a database written by a newer build is refused and nothing is rewritten.
/// </summary>
public sealed class SetupMigrationTests
{
    private const string ConnectionName = "migration-probe";

    [Fact]
    public async Task An_older_installation_is_migrated_and_stays_readable()
    {
        await WithDatabase(async (databaseFile, settings, audit) =>
        {
            await SeedEarlierInstallationAsync(settings);

            var before = await settings.ReadSchemaVersionAsync();
            Assert.Equal(OgSetupCompatibility.Unversioned, before.Value);

            var result = await NewMigration(settings, audit).MigrateAsync("sysadmin");

            Assert.True(result.Success, $"{result.Message} ({result.Reference})");
            Assert.Equal(OgSetupCompatibility.Unversioned, result.Value!.FromVersion);
            Assert.Equal(OgSetupCompatibility.CurrentSchemaVersion, result.Value.ToVersion);
            Assert.Contains(OgSharedSetup.CompanyId, result.Value.Rewritten);
            Assert.Contains(OgSharedSetup.ModulesId, result.Value.Rewritten);

            var after = await settings.ReadAsync();
            Assert.True(after.Success, after.Message);
            Assert.Equal("Probe Energy", after.Value!.Installation!.CompanyName);
            Assert.Equal(["WELL", "HSE"], after.Value.Modules);
            Assert.Equal(OgSetupCompatibility.CurrentSchemaVersion, (await settings.ReadSchemaVersionAsync()).Value);

            // The run and the writes it made are in the trail.
            Assert.Contains(audit.Rows, row => row.Verb == "setup.migrate" && row.Outcome == "ok");
            Assert.Contains(audit.Rows, row => row.Verb == "settings.save" && row.Outcome == "ok");

            // A second run has nothing to do.
            audit.Rows.Clear();
            var again = await NewMigration(settings, audit).MigrateAsync("sysadmin");

            Assert.True(again.Success, again.Message);
            Assert.Empty(again.Value!.Rewritten);
        });
    }

    [Fact]
    public async Task A_database_written_by_a_newer_build_is_refused_and_nothing_is_rewritten()
    {
        await WithDatabase(async (databaseFile, settings, audit) =>
        {
            await SeedEarlierInstallationAsync(settings);

            // A newer build stamped the database: create the version row through the ordinary service, then make it
            // claim a version this build does not understand.
            Assert.True((await settings.WriteSchemaVersionAsync(string.Empty, "newer-build")).Success);

            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText =
                    "UPDATE BEEPSETTINGS SET SETTINGS_JSON = '{\"schemaVersion\":99}' WHERE SETTINGS_ID = 'SYSTEM';";
                command.ExecuteNonQuery();
            }

            var before = (await settings.ReadAsync()).Value!;
            var companyRevision = before.InstallationRevision;

            var refused = await NewMigration(settings, audit).MigrateAsync("sysadmin");

            Assert.False(refused.Success);
            Assert.Equal(OgSetupCompatibility.CodeDatabaseNewer, refused.Reference);

            // Nothing was rewritten: the company revision is the one the earlier installation wrote.
            var after = (await settings.ReadAsync()).Value!;
            Assert.Equal(companyRevision, after.InstallationRevision);
            Assert.Equal("Probe Energy", after.Installation!.CompanyName);

            Assert.Contains(audit.Rows, row => row.Verb == "setup.migrate" && row.Outcome == "refused");
        });
    }

    // ------------------------------------------------------------------ fixtures

    /// <summary>Three documents and no version row: an installation as an earlier build left it.</summary>
    private static async Task SeedEarlierInstallationAsync(OgSharedSetupService settings)
    {
        var company = new OgInstallationSettings(
            "Probe Energy", "PROBE", "AE", "en-US", "UTC", "USD", 1, "OPERATOR", "oilfield", "North Field",
            "admin", new DateTimeOffset(2026, 11, 1, 8, 0, 0, TimeSpan.Zero));

        Assert.True((await settings.WriteInstallationAsync(company, string.Empty, "admin")).Success);
        Assert.True((await settings.WriteToolsEnvironmentAsync(
            "{\"extends\":\"country:AE\",\"units\":{\"system\":\"oilfield\"}}", string.Empty, "admin")).Success);
        Assert.True((await settings.WriteModulesAsync(["WELL", "HSE"], string.Empty, "admin")).Success);
    }

    private static OgSetupMigration NewMigration(OgSharedSetupService settings, RecordingAudit audit) =>
        new(settings, new RecordingFailureReporter(), audit);

    private static async Task WithDatabase(
        Func<string, OgSharedSetupService, RecordingAudit, Task> test)
    {
        var root = Path.Combine(OgPaths.DataRoot, "migration-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new RecordingFailureReporter();
        var audit = new RecordingAudit();

        try
        {
            using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasMigration"));
            OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

            var catalog = new BeepDmConnectionCatalog(host, reporter);
            var dataDirectory = Path.Combine(root, "Data");
            var databaseFile = Path.Combine(dataDirectory, "migration.db");

            var created = await catalog.EnsureLocalAsync(
                new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "migration.db"));

            Assert.True(created.Success, created.Message);

            var store = new BeepDmSharedSetupStore(host, reporter, ConnectionName);
            var settings = new OgSharedSetupService(store, reporter, audit);

            await test(databaseFile, settings, audit);
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
