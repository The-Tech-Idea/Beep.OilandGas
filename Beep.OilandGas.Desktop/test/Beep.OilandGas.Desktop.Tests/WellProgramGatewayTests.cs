using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-05 against a real SQLite database: the queue carries the prospect risk, the gate refuses an incomplete risk
/// and the author's own decision, a decision records actor/date/rationale/conditions, a second decision is refused,
/// conditions travel on the approved program, and a decision that fails is retained and retried unchanged.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class WellProgramGatewayTests
{
    private const string ConnectionName = "exp05-wellprogram";

    [IntegrationFact]
    public async Task The_gate_records_decisions_and_refuses_the_ones_it_must()
    {        var root = Path.Combine(OgPaths.DataRoot, "exp05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasExp05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "wellprogram.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "wellprogram.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var gate = new OgWellProgramApprovals(() => source, reporter);
        var today = new DateTime(2026, 3, 11);

        // ---- the queue ---------------------------------------------------------
        var queue = await gate.LoadQueueAsync("FIELD-1", "North", today);
        Assert.True(queue.Success, queue.Message);
        var programs = queue.Value!;

        Assert.Equal(4, programs.Count);
        Assert.All(programs, program => Assert.True(program.IsPending));

        var alpha = Assert.Single(programs, program => program.Id == "WP-1");
        Assert.Equal("Alpha exploration well", alpha.Name);
        Assert.Equal("Alpha", alpha.ProspectName);
        Assert.Equal("author", alpha.Author);
        Assert.Equal("12,000,000 USD", alpha.CostText);
        Assert.Equal("3200 m", alpha.DepthText);
        Assert.Equal(10, alpha.AgeDays);
        Assert.True(alpha.ProspectRiskComplete);

        var bravo = Assert.Single(programs, program => program.Id == "WP-2");
        Assert.False(bravo.ProspectRiskComplete);
        Assert.Contains("incomplete", bravo.RiskText, StringComparison.OrdinalIgnoreCase);

        // ---- AC-2: an incomplete prospect risk blocks the gate -----------------
        var blocked = await gate.DecideAsync(
            bravo,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "approver"));

        Assert.False(blocked.Success);
        Assert.Equal(OgWellProgramApprovals.CodeRiskIncomplete, blocked.Reference);
        Assert.Null(gate.Pending);
        Assert.Equal("PENDING", StateOf(database, "WP-2"));

        // ---- AC-3: the author cannot decide -----------------------------------
        var ownProgram = await gate.DecideAsync(
            alpha,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, null, [], "author"));

        Assert.False(ownProgram.Success);
        Assert.Equal(OgWellProgramApprovals.CodeAuthorCannotDecide, ownProgram.Reference);
        Assert.Equal("PENDING", StateOf(database, "WP-1"));

        // ---- AC-1: a decision records actor, date and rationale ---------------
        var approved = await gate.DecideAsync(
            alpha,
            new OgWellProgramDecision(OgWellProgramDecision.Approve, "the closure is well defined", [], "approver"));

        Assert.True(approved.Success, approved.Message);
        var decided = approved.Value!;

        Assert.Equal("APPROVED", decided.State);
        Assert.Equal("approver", decided.DecidedBy);
        Assert.NotNull(decided.DecidedAt);
        Assert.Equal("the closure is well defined", decided.Rationale);
        Assert.Single(decided.History);

        var entry = decided.History[0];
        Assert.Equal("APPROVE", entry.Decision);
        Assert.Equal("APPROVED", entry.ResultingState);
        Assert.Equal("approver", entry.Actor);
        Assert.Equal("the closure is well defined", entry.Rationale);
        Assert.NotNull(entry.DecidedAt);

        // ---- AC-4: a second decision is refused -------------------------------
        var second = await gate.DecideAsync(
            alpha,
            new OgWellProgramDecision(OgWellProgramDecision.Reject, "changed my mind", [], "other"));

        Assert.False(second.Success);
        Assert.Equal(OgWellProgramApprovals.CodeConflict, second.Reference);
        Assert.Contains("already decided", second.Message!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("APPROVED", StateOf(database, "WP-1"));
        Assert.Equal(1, DecisionCount(database, "WP-1"));

        // ---- AC-5: conditions travel on the approved program ------------------
        var charlie = Assert.Single(programs, program => program.Id == "WP-3");

        var conditional = await gate.DecideAsync(
            charlie,
            new OgWellProgramDecision(
                OgWellProgramDecision.ApproveWithConditions,
                "approved with the drilling constraints",
                ["Drill with a 9-5/8 liner", "No night operations over the village"],
                "approver"));

        Assert.True(conditional.Success, conditional.Message);
        Assert.Equal("APPROVED_WITH_CONDITIONS", conditional.Value!.State);
        Assert.True(conditional.Value.HasConditions);
        Assert.Equal(
            ["Drill with a 9-5/8 liner", "No night operations over the village"],
            conditional.Value.Conditions);
        Assert.Equal(conditional.Value.Conditions, conditional.Value.History[0].Conditions);

        // The conditions are on the program row, where DEV-05 reads them as constraints.
        Assert.Contains("9-5/8 liner", ConditionsOf(database, "WP-3"), StringComparison.Ordinal);

        // ---- AC-6: a lost acknowledgement retains the decision ----------------
        // A trigger makes the update fail for WP-4 only, which is a lost acknowledgement.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var trigger = connection.CreateCommand();
            trigger.CommandText =
                """
                CREATE TRIGGER BLOCK_WP_4 BEFORE UPDATE ON WELL_PROGRAM
                WHEN OLD.PROGRAM_ID = 'WP-4'
                BEGIN
                    SELECT RAISE(ABORT, 'blocked for the test');
                END;
                """;
            trigger.ExecuteNonQuery();
        }

        var delta = (await gate.LoadAsync("WP-4", today)).Value!;
        var retainedDecision = new OgWellProgramDecision(
            OgWellProgramDecision.Return, "needs a peer review", [], "approver");

        var lost = await gate.DecideAsync(delta, retainedDecision);

        Assert.False(lost.Success);
        Assert.NotNull(gate.Pending);
        Assert.Equal("WP-4", gate.RetainedProgramId);
        Assert.Equal("PENDING", StateOf(database, "WP-4"));
        Assert.Equal(0, DecisionCount(database, "WP-4"));

        // The trigger is removed and the retry sends the same decision.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TRIGGER BLOCK_WP_4;";
            drop.ExecuteNonQuery();
        }

        var retried = await gate.RetryAsync(today);

        Assert.True(retried.Success, retried.Message);
        Assert.Equal("RETURNED", retried.Value!.State);
        Assert.Equal("needs a peer review", retried.Value.Rationale);
        Assert.Equal("approver", retried.Value.DecidedBy);
        Assert.Null(gate.Pending);

        // ---- reads -------------------------------------------------------------
        var unknown = await gate.LoadAsync("WP-NOPE", today);
        Assert.False(unknown.Success);
        Assert.Equal(OgWellProgramApprovals.CodeUnknown, unknown.Reference);

        var empty = await gate.LoadQueueAsync("FIELD-NONE", "Nowhere", today);
        Assert.True(empty.Success);
        Assert.Empty(empty.Value!);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TABLE WELL_PROGRAM;";
            drop.ExecuteNonQuery();
        }

        var failed = await gate.LoadQueueAsync("FIELD-1", "North", today);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
        Assert.NotNull(failed.Reference);
        Assert.NotEmpty(reporter.Failures);
    }

    private static string? StateOf(string database, string programId) =>
        Scalar(database, $"SELECT STATE FROM WELL_PROGRAM WHERE PROGRAM_ID = '{programId}';");

    private static string ConditionsOf(string database, string programId) =>
        Scalar(database, $"SELECT CONDITIONS_TEXT FROM WELL_PROGRAM WHERE PROGRAM_ID = '{programId}';") ?? string.Empty;

    private static long DecisionCount(string database, string programId) =>
        long.Parse(Scalar(database, $"SELECT COUNT(*) FROM WELL_PROGRAM_DECISION WHERE PROGRAM_ID = '{programId}';")!);

    private static string? Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar()?.ToString();
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PROSPECT (
                PROSPECT_ID TEXT NOT NULL PRIMARY KEY,
                PROSPECT_NAME TEXT NULL);

            CREATE TABLE PROSPECT_RISK_ASSESSMENT (
                PROSPECT_ID TEXT NOT NULL,
                ASSESSMENT_ID TEXT NOT NULL,
                TRAP_RISK REAL NULL,
                RESERVOIR_RISK REAL NULL,
                SEAL_RISK REAL NULL,
                SOURCE_RISK REAL NULL,
                TIMING_RISK REAL NULL,
                PRIMARY KEY (PROSPECT_ID, ASSESSMENT_ID));

            CREATE TABLE WELL_PROGRAM (
                PROGRAM_ID TEXT NOT NULL PRIMARY KEY,
                PROSPECT_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PROGRAM_NAME TEXT NULL,
                OBJECTIVE TEXT NULL,
                TARGET_DEPTH REAL NULL,
                DEPTH_OUOM TEXT NULL,
                ESTIMATED_COST REAL NULL,
                CURRENCY TEXT NULL,
                STATE TEXT NULL,
                AUTHOR TEXT NULL,
                SUBMITTED_AT TEXT NULL,
                DECIDED_BY TEXT NULL,
                DECIDED_AT TEXT NULL,
                RATIONALE TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                ROW_VERSION INTEGER NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE WELL_PROGRAM_DECISION (
                PROGRAM_ID TEXT NOT NULL,
                DECISION_SEQ_NO INTEGER NOT NULL,
                DECISION TEXT NULL,
                RESULTING_STATE TEXT NULL,
                RATIONALE TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                DECIDED_BY TEXT NULL,
                DECIDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (PROGRAM_ID, DECISION_SEQ_NO));

            INSERT INTO PROSPECT (PROSPECT_ID, PROSPECT_NAME) VALUES
                ('P-1', 'Alpha'),
                ('P-2', 'Bravo'),
                ('P-3', 'Charlie'),
                ('P-4', 'Delta');

            INSERT INTO PROSPECT_RISK_ASSESSMENT
                (PROSPECT_ID, ASSESSMENT_ID, TRAP_RISK, RESERVOIR_RISK, SEAL_RISK, SOURCE_RISK, TIMING_RISK)
            VALUES
                ('P-1', 'A-1', 0.1, 0.2, 0.1, 0.1, 0.1),
                ('P-2', 'A-2', 0.1, 0.2, NULL, 0.1, 0.1),
                ('P-3', 'A-3', 0.1, 0.2, 0.1, 0.1, 0.1),
                ('P-4', 'A-4', 0.1, 0.2, 0.1, 0.1, 0.1);

            INSERT INTO WELL_PROGRAM
                (PROGRAM_ID, PROSPECT_ID, FIELD_ID, PROGRAM_NAME, OBJECTIVE, TARGET_DEPTH, DEPTH_OUOM,
                 ESTIMATED_COST, CURRENCY, STATE, AUTHOR, SUBMITTED_AT, ROW_VERSION)
            VALUES
                ('WP-1', 'P-1', 'FIELD-1', 'Alpha exploration well', 'Test the Alpha closure', 3200, 'm',
                 12000000, 'USD', 'PENDING', 'author', '2026-03-01 08:00:00', 1),
                ('WP-2', 'P-2', 'FIELD-1', 'Bravo exploration well', 'Test the Bravo closure', 2800, 'm',
                 9000000, 'USD', 'PENDING', 'author', '2026-03-02 08:00:00', 1),
                ('WP-3', 'P-3', 'FIELD-1', 'Charlie exploration well', 'Test the Charlie closure', 3100, 'm',
                 11000000, 'USD', 'PENDING', 'author', '2026-03-03 08:00:00', 1),
                ('WP-4', 'P-4', 'FIELD-1', 'Delta exploration well', 'Test the Delta closure', 2500, 'm',
                 7000000, 'USD', 'PENDING', 'author', '2026-03-04 08:00:00', 1),
                ('WP-9', 'P-1', 'FIELD-9', 'Other field well', 'Another field', 1000, 'm',
                 1000000, 'USD', 'PENDING', 'author', '2026-03-05 08:00:00', 1);
            """;
        command.ExecuteNonQuery();
    }

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
