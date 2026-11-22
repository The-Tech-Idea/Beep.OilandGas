using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-04 against a real SQLite database: history is appended and never edited, codes come from R_WELL_STATUS, an
/// illegal transition is refused with the legal set, effective and recorded dates differ, the change is audited, and
/// a change that fails is retained and retried unchanged.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellStatusGatewayTests
{
    private const string ConnectionName = "wel04-status";

    [IntegrationFact]
    public async Task A_change_appends_history_and_a_failed_change_is_retained_for_retry()
    {        var root = Path.Combine(OgPaths.DataRoot, "wel04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWel04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "status.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "status.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var audit = new List<string>();
        var status = new OgWellStatus(
            () => source,
            reporter,
            (verb, id, outcome, detail) => audit.Add($"{verb}|{id}|{outcome}|{detail}"));

        // The options are the reference table's active codes only (AC-2).
        var loaded = await status.LoadAsync("UWI-A");
        Assert.True(loaded.Success, loaded.Message);
        var view = loaded.Value!;

        Assert.Equal(5, view.Options.Count);
        Assert.DoesNotContain(view.Options, option => option.Status == "RETIRED");
        Assert.Equal("PLANNED", view.Current!.Status);
        Assert.Single(view.History);
        Assert.Contains("DRILLING", view.LegalNext);

        // AC-2: a literal is refused and the codes are listed.
        var literal = await status.ChangeAsync(
            new OgWellStatusChange("UWI-A", "PLANNED", "BOGUS", null, "TEST", null, "fahad"));

        Assert.False(literal.Success);
        Assert.Equal(OgWellStatus.CodeUnknownStatus, literal.Reference);
        Assert.Contains("DRILLING", literal.Message!, StringComparison.Ordinal);

        // AC-3: an illegal transition is refused with the legal set.
        var illegal = await status.ChangeAsync(
            new OgWellStatusChange("UWI-A", "PLANNED", "PRODUCING", null, "TEST", null, "fahad"));

        Assert.False(illegal.Success);
        Assert.Equal(OgWellStatus.CodeIllegalTransition, illegal.Reference);
        Assert.Contains("DRILLING", illegal.Message!, StringComparison.Ordinal);

        // AC-1, AC-5, AC-6: append; the effective date is independent of now; actor/from/to/reason are recorded.
        var effective = new DateTime(2026, 2, 1);
        var changed = await status.ChangeAsync(
            new OgWellStatusChange("UWI-A", "PLANNED", "DRILLING", effective, "SPUD", "spudded the well", "fahad"));

        Assert.True(changed.Success, changed.Message);

        var after = changed.Value!;
        Assert.Equal(2, after.History.Count);
        Assert.Equal("DRILLING", after.Current!.Status);

        var newest = after.History[0];
        var previous = after.History[1];

        Assert.Equal("PLANNED", previous.Status);
        Assert.Equal("DRILLING", newest.Status);
        Assert.Equal("PLANNED", newest.FromCode);
        Assert.False(previous.Active);
        Assert.True(newest.Active);
        Assert.Equal(effective, newest.EffectiveDate);
        Assert.NotNull(newest.RecordedAt);
        Assert.NotEqual(newest.EffectiveDate!.Value.Date, newest.RecordedAt!.Value.Date);
        Assert.Equal("fahad", newest.Actor);
        Assert.Equal("SPUD", newest.Reason);
        Assert.Equal("spudded the well", newest.Note);
        Assert.Contains(audit, entry => entry.Contains("from=PLANNED to=DRILLING reason=SPUD actor=fahad"));

        // AC-1: the previous row's status was never rewritten, and the well carries the current status.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT STATUS, ACTIVE_IND FROM WELL_STATUS WHERE UWI = 'UWI-A' AND STATUS = 'PLANNED';";
            using var reader = command.ExecuteReader();
            Assert.True(reader.Read());
            Assert.Equal("PLANNED", reader.GetString(0));
            Assert.Equal("N", reader.GetString(1));

            using var well = connection.CreateCommand();
            well.CommandText = "SELECT CURRENT_STATUS FROM WELL WHERE UWI = 'UWI-A';";
            Assert.Equal("DRILLING", (string)well.ExecuteScalar()!);
        }

        // AC-4: a failed acknowledgement retains the original; the retry sends the same effective date and note.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TABLE WELL_STATUS;";
            drop.ExecuteNonQuery();
        }

        var retained = new OgWellStatusChange(
            "UWI-B", "DRILLING", "COMPLETED", new DateTime(2026, 3, 1), "DRILL", "finished drilling", "sara");

        var failed = await status.ChangeAsync(retained);

        Assert.False(failed.Success);
        Assert.NotNull(status.Pending);
        Assert.Equal(retained, status.Pending);

        CreateStatusTable(database);

        var retried = await status.RetryAsync();

        Assert.True(retried.Success, retried.Message);
        Assert.Null(status.Pending);

        var retriedEntry = Assert.Single(retried.Value!.History);
        Assert.Equal("COMPLETED", retriedEntry.Status);
        Assert.Equal(new DateTime(2026, 3, 1), retriedEntry.EffectiveDate);
        Assert.Equal("finished drilling", retriedEntry.Note);
        Assert.Equal("sara", retriedEntry.Actor);
    }

    private static void CreateStatusTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = StatusTableSql;
        command.ExecuteNonQuery();
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                CURRENT_STATUS TEXT NULL,
                CURRENT_STATUS_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE R_WELL_STATUS (
                STATUS_TYPE TEXT NOT NULL,
                STATUS TEXT NOT NULL,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (STATUS_TYPE, STATUS));

            INSERT INTO WELL (UWI, CURRENT_STATUS, CURRENT_STATUS_DATE) VALUES
                ('UWI-A', 'PLANNED', '2026-01-01'),
                ('UWI-B', 'DRILLING', '2026-01-02');

            INSERT INTO R_WELL_STATUS (STATUS_TYPE, STATUS, LONG_NAME, SHORT_NAME, ACTIVE_IND) VALUES
                ('Well Status', 'PLANNED', 'Planned', 'PLN', 'Y'),
                ('Well Status', 'DRILLING', 'Drilling', 'DRL', 'Y'),
                ('Well Status', 'COMPLETED', 'Completed', 'CMP', 'Y'),
                ('Well Status', 'PRODUCING', 'Producing', 'PRD', 'Y'),
                ('Well Status', 'ABANDONED', 'Abandoned', 'ABD', 'Y'),
                ('Well Status', 'RETIRED', 'Retired', 'RET', 'N');
            """ + StatusTableSql + """

            INSERT INTO WELL_STATUS
                (UWI, SOURCE, STATUS_ID, STATUS_TYPE, STATUS, STATUS_DATE, EFFECTIVE_DATE, START_TIME,
                 ACTIVE_IND, ROW_CREATED_BY, ROW_CREATED_DATE, ROW_CHANGED_BY, ROW_CHANGED_DATE)
            VALUES
                ('UWI-A', 'DESKTOP', 'S1', 'Well Status', 'PLANNED', '2026-01-01 08:00:00',
                 '2026-01-01 08:00:00', '2026-01-01 08:00:00', 'Y', 'system', '2026-01-01 08:00:00',
                 'system', '2026-01-01 08:00:00');
            """;
        command.ExecuteNonQuery();
    }

    private const string StatusTableSql =
        """
        CREATE TABLE IF NOT EXISTS WELL_STATUS (
            UWI TEXT NOT NULL,
            SOURCE TEXT NOT NULL,
            STATUS_ID TEXT NOT NULL,
            STATUS_TYPE TEXT NULL,
            STATUS TEXT NULL,
            STATUS_DATE TEXT NULL,
            EFFECTIVE_DATE TEXT NULL,
            START_TIME TEXT NULL,
            END_TIME TEXT NULL,
            ACTIVE_IND TEXT NULL,
            STATUS_QUALIFIER TEXT NULL,
            REMARK TEXT NULL,
            ROW_CREATED_BY TEXT NULL,
            ROW_CREATED_DATE TEXT NULL,
            ROW_CHANGED_BY TEXT NULL,
            ROW_CHANGED_DATE TEXT NULL,
            PRIMARY KEY (UWI, SOURCE, STATUS_ID));
        """;

    private sealed class Recording : IFailureReporter
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
