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
/// EXP-02 against a real SQLite database: the board's columns follow the recorded stage sequence, an unclassified
/// prospect keeps a column, an incomplete risk reads "incomplete", the filters work, a refused advance writes
/// nothing, and a failed read returns no board rather than zeros.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class ProspectBoardGatewayTests
{
    private const string ConnectionName = "exp02-board";

    private static readonly DateTime AsOf = new(2026, 3, 1);

    [IntegrationFact]
    public async Task The_board_orders_columns_by_sequence_and_refuses_a_stage_change_without_writing()
    {        var root = Path.Combine(OgPaths.DataRoot, "exp02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasExp02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "board.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "board.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var board = new OgProspectBoard(() => source, reporter);

        var loaded = await board.LoadAsync("FIELD-1", "North", null, AsOf);
        Assert.True(loaded.Success, loaded.Message);
        var snapshot = loaded.Value!;

        // AC-1: SCREENING (seq 1) before APPRAISAL (2) before DRILLING (3); alphabetical would start with APPRAISAL.
        Assert.Equal(
            ["SCREENING", "APPRAISAL", "DRILLING", OgProspectCard.UnclassifiedStage],
            snapshot.Columns.Select(column => column.Stage));

        Assert.Equal([1, 2, 1, 1], snapshot.Columns.Select(column => column.Count));
        Assert.Equal(5, snapshot.CardCount);

        // The other field's prospect never appears.
        Assert.DoesNotContain(snapshot.Cards, card => card.Id == "P-9");

        // AC-2: the unclassified prospect is on the board, in its own column.
        var unclassified = Assert.Single(snapshot.Cards, card => card.Id == "P-3");
        Assert.Equal(OgProspectCard.UnclassifiedStage, unclassified.StageLabel);
        Assert.Equal(int.MaxValue, unclassified.StageOrder);

        // AC-3: P-4 has one element missing, so it reads "incomplete" and contributes no chance.
        var incomplete = Assert.Single(snapshot.Cards, card => card.Id == "P-4");
        Assert.False(incomplete.RiskComplete);
        Assert.Null(incomplete.ChancePercent);
        Assert.Equal("incomplete", incomplete.ChanceText);
        Assert.Equal(1, snapshot.IncompleteRiskCount);

        // The complete assessment gives a real chance: the element product 0.9*0.8*0.9*0.9*0.9 => 52%.
        var alpha = Assert.Single(snapshot.Cards, card => card.Id == "P-1");
        Assert.True(alpha.RiskComplete);
        Assert.Equal(52m, alpha.ChancePercent);
        Assert.Equal("52%", alpha.ChanceText);

        // The current stage is the highest recorded sequence, with its start date and age.
        Assert.Equal("APPRAISAL", alpha.Stage);
        Assert.Equal(new DateTime(2026, 2, 1), alpha.StageEnteredAt);
        Assert.Equal(28, alpha.StageAgeDays);
        Assert.Equal(2, alpha.History.Count);
        Assert.Equal("APPRAISAL", alpha.History[0].Stage);

        // Ownership and play come from the join, and the options are offered for filtering.
        Assert.Equal("Upper sand", alpha.Play);
        Assert.Equal("BASIN-1", alpha.Basin);
        Assert.Equal("BA-1", alpha.Owner);
        Assert.Contains("Upper sand", snapshot.Plays);
        Assert.Contains("BASIN-2", snapshot.Basins);
        Assert.Contains("BA-2", snapshot.Owners);

        // Filters: by play, by owner, by basin, by minimum chance, by stage age.
        var byPlay = (await board.LoadAsync("FIELD-1", "North", new OgProspectFilter(Play: "Lower sand"), AsOf)).Value!;
        Assert.Equal(3, byPlay.CardCount);
        Assert.All(byPlay.Cards, card => Assert.Equal("Lower sand", card.Play));
        Assert.Equal(4, byPlay.ColumnCount);

        var byOwner = (await board.LoadAsync("FIELD-1", "North", new OgProspectFilter(Owner: "BA-1"), AsOf)).Value!;
        Assert.All(byOwner.Cards, card => Assert.Equal("BA-1", card.Owner));

        var byBasin = (await board.LoadAsync("FIELD-1", "North", new OgProspectFilter(Basin: "BASIN-2"), AsOf)).Value!;
        Assert.All(byBasin.Cards, card => Assert.Equal("BASIN-2", card.Basin));

        var byChance = (await board.LoadAsync(
            "FIELD-1", "North", new OgProspectFilter(MinimumChancePercent: 50m), AsOf)).Value!;
        Assert.All(byChance.Cards, card => Assert.True(card.RiskComplete && card.ChancePercent is { } chance && chance >= 50m));
        Assert.DoesNotContain(byChance.Cards, card => card.Id == "P-4");

        var byAge = (await board.LoadAsync(
            "FIELD-1", "North", new OgProspectFilter(MaximumStageAgeDays: 10), AsOf)).Value!;
        Assert.All(byAge.Cards, card => Assert.True(card.StageAgeDays is { } age && age <= 10));

        // AC-4: the advance is refused, and the stage rows are exactly as they were.
        var before = StageRowCount(database);

        var refused = await board.AdvanceAsync(alpha, "DRILLING", "fahad");

        Assert.False(refused.Success);
        Assert.Equal(OgProspectBoard.CodeAdvanceNotWired, refused.Reference);
        Assert.Contains("STG-14", refused.Message!, StringComparison.Ordinal);
        Assert.Equal(before, StageRowCount(database));

        var stillAppraisal = Assert.Single(
            (await board.LoadAsync("FIELD-1", "North", null, AsOf)).Value!.Cards,
            card => card.Id == "P-1");
        Assert.Equal("APPRAISAL", stillAppraisal.Stage);

        // AC-5: the drop path refuses with the same words.
        var dropped = await board.MoveAsync(alpha, "DRILLING", "fahad");
        Assert.Equal(refused.Reference, dropped.Reference);
        Assert.Equal(refused.Message, dropped.Message);

        // A field with no records is an empty board, not a failure.
        var empty = await board.LoadAsync("FIELD-NONE", "Nowhere", null, AsOf);
        Assert.True(empty.Success);
        Assert.True(empty.Value!.IsEmpty);
        Assert.Empty(empty.Value.Columns);

        // AC-6: a table that disappears is a failure with no board at all.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TABLE PROSPECT_WORKFLOW_STAGE;";
            drop.ExecuteNonQuery();
        }

        var failed = await board.LoadAsync("FIELD-1", "North", null, AsOf);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
        Assert.NotNull(failed.Reference);
        Assert.NotEmpty(reporter.Failures);
    }

    private static long StageRowCount(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM PROSPECT_WORKFLOW_STAGE;";
        return (long)command.ExecuteScalar()!;
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
                PROSPECT_NAME TEXT NULL,
                PLAY_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PRIMARY_FIELD_ID TEXT NULL,
                CURRENT_OPERATOR TEXT NULL);

            CREATE TABLE PLAY (
                PLAY_ID TEXT NOT NULL PRIMARY KEY,
                PLAY_NAME TEXT NULL,
                BASIN_ID TEXT NULL,
                FIELD_ID TEXT NULL);

            CREATE TABLE PROSPECT_RISK_ASSESSMENT (
                PROSPECT_ID TEXT NOT NULL,
                ASSESSMENT_ID TEXT NOT NULL,
                TRAP_RISK REAL NULL,
                RESERVOIR_RISK REAL NULL,
                SEAL_RISK REAL NULL,
                SOURCE_RISK REAL NULL,
                TIMING_RISK REAL NULL,
                OVERALL_GEOLOGICAL_RISK REAL NULL,
                PRIMARY KEY (PROSPECT_ID, ASSESSMENT_ID));

            CREATE TABLE PROSPECT_WORKFLOW_STAGE (
                PROSPECT_ID TEXT NOT NULL,
                STAGE_SEQ_NO REAL NOT NULL,
                WORKFLOW_STAGE TEXT NOT NULL,
                STAGE_START_DATE TEXT NULL,
                STAGE_END_DATE TEXT NULL,
                STAGE_OUTCOME TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, STAGE_SEQ_NO));

            INSERT INTO PLAY (PLAY_ID, PLAY_NAME, BASIN_ID, FIELD_ID) VALUES
                ('PLAY-1', 'Upper sand', 'BASIN-1', 'FIELD-1'),
                ('PLAY-2', 'Lower sand', 'BASIN-2', 'FIELD-1'),
                ('PLAY-9', 'Other field play', 'BASIN-2', 'FIELD-2');

            INSERT INTO PROSPECT (PROSPECT_ID, PROSPECT_NAME, PLAY_ID, FIELD_ID, CURRENT_OPERATOR) VALUES
                ('P-1', 'Alpha', 'PLAY-1', 'FIELD-1', 'BA-1'),
                ('P-2', 'Bravo', 'PLAY-2', 'FIELD-1', 'BA-2'),
                ('P-3', 'Charlie', 'PLAY-2', 'FIELD-1', 'BA-2'),
                ('P-4', 'Delta', 'PLAY-2', 'FIELD-1', 'BA-2'),
                ('P-5', 'Echo', 'PLAY-1', 'FIELD-1', 'BA-1'),
                ('P-9', 'Other field', 'PLAY-9', 'FIELD-2', 'BA-9');

            INSERT INTO PROSPECT_RISK_ASSESSMENT
                (PROSPECT_ID, ASSESSMENT_ID, TRAP_RISK, RESERVOIR_RISK, SEAL_RISK, SOURCE_RISK, TIMING_RISK,
                 OVERALL_GEOLOGICAL_RISK)
            VALUES
                ('P-1', 'A1', 0.1, 0.2, 0.1, 0.1, 0.1, 0.40),
                ('P-2', 'A2', 0.1, 0.1, 0.1, 0.1, 0.1, 0.20),
                ('P-3', 'A3', 0.2, 0.2, 0.2, 0.2, 0.2, 0.60),
                ('P-4', 'A4', 0.1, NULL, 0.1, 0.1, 0.1, 0.40),
                ('P-5', 'A5', 0.1, 0.1, 0.1, 0.1, 0.1, 0.40),
                ('P-9', 'A9', 0.1, 0.1, 0.1, 0.1, 0.1, 0.40);

            INSERT INTO PROSPECT_WORKFLOW_STAGE
                (PROSPECT_ID, STAGE_SEQ_NO, WORKFLOW_STAGE, STAGE_START_DATE, STAGE_END_DATE, STAGE_OUTCOME)
            VALUES
                ('P-1', 1.0, 'SCREENING', '2025-12-01', '2026-02-01', 'APPROVED'),
                ('P-1', 2.0, 'APPRAISAL', '2026-02-01', NULL, NULL),
                ('P-2', 3.0, 'DRILLING', '2026-02-20', NULL, NULL),
                ('P-4', 2.0, 'APPRAISAL', '2026-01-15', NULL, NULL),
                ('P-5', 1.0, 'SCREENING', '2026-02-25', NULL, NULL),
                ('P-9', 1.0, 'SCREENING', '2026-01-01', NULL, NULL);
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
