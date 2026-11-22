using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-02 AC-4: a blocking conflict is reported before any write.
/// </summary>
/// <remarks>
/// The fixture pre-creates one of the module's tables with a shape the module cannot accept (its key column is
/// nullable and carries no primary key). The apply must refuse that module, insert no rows, and leave the
/// conflicting table exactly as it was - an upgrade that silently rewrites a table somebody else owns is the
/// failure this criterion exists to prevent.
/// </remarks>
public sealed class ModuleSetupConflictTests
{
    private const string ConnectionName = "dat02-conflict";
    private static readonly string[] Scope = ["WELL_STATUS_FACETS"];

    [IntegrationFact]
    public async Task A_mismatched_existing_table_is_refused_before_any_row_is_written()
    {        var root = Path.Combine(OgPaths.DataRoot, "dat02c-" + Guid.NewGuid().ToString("n")[..8]);
        var dataDirectory = Path.Combine(root, "Data");
        var database = Path.Combine(dataDirectory, "conflict.db");
        var reporter = new RecordingFailureReporter();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDat02Conflict"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var catalog = new BeepDmConnectionCatalog(host, reporter);
        var created = await catalog.EnsureLocalAsync(new OgLocalDatabaseRequest(ConnectionName, dataDirectory, "conflict.db"));
        Assert.True(created.Success, created.Message);

        // The conflict: the module's table exists, its key column is nullable, and no primary key is declared.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE R_WELL_STATUS_TYPE (
                    STATUS_TYPE  TEXT NULL,
                    ABBREVIATION TEXT NULL,
                    REMARK       TEXT NULL);
                INSERT INTO R_WELL_STATUS_TYPE (STATUS_TYPE, ABBREVIATION) VALUES ('PRE-EXISTING', 'PRE');
                """;
            command.ExecuteNonQuery();
        }

        using var composition = new OgSetupComposition(host, reporter, ConnectionName);
        var runner = new OgModuleSetupRunner(
            composition,
            new OgSetupStateStore(Path.Combine(root, "state.json")),
            reporter,
            "conflict-test");

        var applied = await runner.ApplyAsync(OgSetupConfirmation.None, Scope);
        var outcome = applied.Value;

        // The run reports the conflict rather than claiming success.
        Assert.NotNull(outcome);
        Assert.False(outcome!.Success, "a module with a mismatched table must not report success");
        Assert.Contains(
            outcome.Modules,
            module => !module.Success
                      && (module.Error?.Contains("schema", StringComparison.OrdinalIgnoreCase) == true
                          || module.Error?.Contains("conflict", StringComparison.OrdinalIgnoreCase) == true
                          || module.Error?.Contains("mismatch", StringComparison.OrdinalIgnoreCase) == true));

        // Before any write: nothing was seeded, and the pre-existing row and shape are untouched.
        Assert.Equal(0, outcome.RecordsInserted);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM R_WELL_STATUS_TYPE;";
                Assert.Equal(1L, (long)command.ExecuteScalar()!);
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('R_WELL_STATUS_TYPE') WHERE name = 'STATUS_TYPE' AND \"notnull\" = 0;";
                Assert.Equal(1L, (long)command.ExecuteScalar()!);
            }
        }

        // The other tables of the module were not created either: the module is the unit that failed.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'RA_WELL_STATUS_TYPE';";
            Assert.Equal(0L, (long)command.ExecuteScalar()!);
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
