using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-02 against a real SQLite database: the board, open diligence blocking an advance, a recorded transition
/// with actor/date/rationale, the rights matrix, a declined opportunity keeping its history, and a lost
/// acknowledgement retained for retry. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class LeasePipelineGatewayTests
{
    private const string ConnectionName = "lse02-pipeline";

    [IntegrationFact]
    public async Task The_pipeline_blocks_open_diligence_records_transitions_and_keeps_a_declined_history()
    {        var root = Path.Combine(OgPaths.DataRoot, "lse02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLse02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "pipeline.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "pipeline.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var pipeline = new OgLeasePipelineService(() => source, reporter);
        var asOf = new DateTime(2026, 6, 1);

        // The board: three opportunities for F-1 (the fourth is another field), one of them unclassified.
        var loaded = await pipeline.LoadAsync("F-1", "Alpha field", asOf);

        Assert.True(loaded.Success, loaded.Message);
        var board = loaded.Value!;

        Assert.Equal(3, board.CardCount);
        Assert.DoesNotContain(board.Cards, card => card.Id == "OP-4");
        Assert.Equal(1, board.OpenDiligenceCount);
        Assert.Contains(board.Columns, column => column.IsUnclassified && column.Stage == OgLeaseOpportunity.UnclassifiedStage);

        // The detail: money with its currency, the three right kinds, the negotiation round and the history.
        var detail = await pipeline.LoadDetailAsync("OP-1", asOf);

        Assert.True(detail.Success, detail.Message);
        var first = detail.Value!;

        Assert.Equal("1,000 USD", first.Opportunity.AskingPriceText);
        Assert.Equal("mineral 1 · surface 1 · working 1", first.RightsSummary);
        Assert.Single(first.Negotiations);
        Assert.Equal("900 USD", first.Negotiations[0].OfferText);
        Assert.Single(first.History);

        // AC-2: an open diligence item blocks the move out of due diligence.
        var blocked = await pipeline.AdvanceAsync(
            new OgPipelineDecision("OP-2", LeasePipelineStage.Negotiation, "diligence is complete", "sara"));

        Assert.False(blocked.Success);
        Assert.Equal(OgLeasePipelineService.CodeDiligenceOpen, blocked.Reference);
        Assert.Contains("Is the title clear?", blocked.Message!, StringComparison.Ordinal);

        // Closing the item unblocks the move, and the transition records actor, date and rationale (AC-1).
        var closed = await pipeline.RecordDiligenceAsync(
            "OP-2", "Title", "Is the title clear?", OgDiligenceItem.Pass, "DOC-1", "sara");

        Assert.True(closed.Success, closed.Message);
        Assert.Equal(0, closed.Value!.OpenDiligenceCount);

        var advanced = await pipeline.AdvanceAsync(
            new OgPipelineDecision("OP-2", LeasePipelineStage.Negotiation, "diligence is complete", "sara"));

        Assert.True(advanced.Success, advanced.Message);
        var moved = advanced.Value!;

        Assert.Equal(LeasePipelineStage.Negotiation, moved.Opportunity.Stage);
        Assert.Single(moved.History);
        Assert.Equal("sara", moved.History[0].Actor);
        Assert.Equal("diligence is complete", moved.History[0].Rationale);
        Assert.NotNull(moved.History[0].DecidedAt);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT ACQUISITION_STATUS FROM LEASE_ACQUISITION WHERE LEASE_ACQUISITION_ID = 'OP-2';";
            Assert.Equal(LeasePipelineStage.Negotiation, (string)command.ExecuteScalar()!);
        }

        // A negotiation round needs a counterparty and an explicit currency (the state table and AC-5).
        var noParty = await pipeline.RecordNegotiationAsync("OP-1", " ", 2, 950m, "USD", null, "sara");

        Assert.False(noParty.Success);
        Assert.Equal(OgLeasePipelineService.CodeCounterpartyRequired, noParty.Reference);

        var noCurrency = await pipeline.RecordNegotiationAsync("OP-1", "LandCo", 2, 950m, " ", null, "sara");

        Assert.False(noCurrency.Success);
        Assert.Equal(OgLeasePipelineService.CodeCurrencyRequired, noCurrency.Reference);

        // AC-4: a decline is a recorded transition, so the earlier history stays.
        var declined = await pipeline.DeclineAsync("OP-1", "no access at this price", "sara");

        Assert.True(declined.Success, declined.Message);
        Assert.Equal(LeasePipelineStage.Declined, declined.Value!.Opportunity.Stage);
        Assert.Equal(2, declined.Value.History.Count);
        Assert.Contains(declined.Value.History, entry => entry.Stage == LeasePipelineStage.Opportunity);
        Assert.Equal("no access at this price", declined.Value.History[0].Rationale);

        // AC-6-like state: a lost acknowledgement is retained and retried unchanged.
        ForceStageInsertFailure(database, "OP-3");
        var lost = await pipeline.AdvanceAsync(
            new OgPipelineDecision("OP-3", LeasePipelineStage.Opportunity, "enter the pipeline", "sara"));

        Assert.False(lost.Success);
        Assert.NotNull(pipeline.Pending);
        Assert.Equal("enter the pipeline", pipeline.Pending!.Rationale);
        Assert.Empty(History(database, "OP-3"));

        DropFailureTrigger(database);
        var retried = await pipeline.RetryAsync(asOf);

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(LeasePipelineStage.Opportunity, retried.Value!.Opportunity.Stage);
        Assert.Single(retried.Value.History);
        Assert.Equal("enter the pipeline", retried.Value.History[0].Rationale);
        Assert.Equal("sara", retried.Value.History[0].Actor);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE LEASE_ACQUISITION (
                LEASE_ACQUISITION_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                LEASE_NAME TEXT NULL,
                STATE_ID TEXT NULL,
                GROSS_ACREAGE REAL NOT NULL,
                ACREAGE_OUOM TEXT NULL,
                BONUS_AMOUNT REAL NOT NULL,
                BONUS_AMOUNT_OUOM TEXT NULL,
                OPERATOR TEXT NULL,
                ACQUISITION_DATE TEXT NULL,
                ACQUISITION_STATUS TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE LEASE_PIPELINE_STAGE (
                LEASE_ID TEXT NOT NULL,
                STAGE_SEQ_NO INTEGER NOT NULL,
                STAGE TEXT NULL,
                OUTCOME TEXT NULL,
                RATIONALE TEXT NULL,
                ACTOR TEXT NULL,
                DECIDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (LEASE_ID, STAGE_SEQ_NO));

            CREATE TABLE LEASE_DILIGENCE_ITEM (
                LEASE_ID TEXT NOT NULL,
                ITEM_ID TEXT NOT NULL,
                CATEGORY TEXT NULL,
                QUESTION TEXT NULL,
                OUTCOME TEXT NULL,
                EVIDENCE_REF TEXT NULL,
                REVIEWER TEXT NULL,
                REVIEWED_AT TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (LEASE_ID, ITEM_ID));

            CREATE TABLE LEASE_RIGHT (
                LEASE_ID TEXT NOT NULL,
                RIGHT_ID TEXT NOT NULL,
                RIGHT_TYPE TEXT NULL,
                OWNER TEXT NULL,
                INTEREST_PERCENT REAL NOT NULL,
                EFFECTIVE_DATE TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (LEASE_ID, RIGHT_ID));

            CREATE TABLE LEASE_NEGOTIATION (
                LEASE_ID TEXT NOT NULL,
                NEGOTIATION_ID TEXT NOT NULL,
                ROUND_NO INTEGER NOT NULL,
                COUNTERPARTY TEXT NULL,
                OFFER_AMOUNT REAL NOT NULL,
                CURRENCY TEXT NULL,
                NOTES TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                PRIMARY KEY (LEASE_ID, NEGOTIATION_ID));

            INSERT INTO LEASE_ACQUISITION
                (LEASE_ACQUISITION_ID, FIELD_ID, LEASE_NAME, STATE_ID, GROSS_ACREAGE, ACREAGE_OUOM,
                 BONUS_AMOUNT, BONUS_AMOUNT_OUOM, OPERATOR, ACQUISITION_DATE, ACQUISITION_STATUS)
            VALUES
                ('OP-1', 'F-1', 'Alpha', 'NORTH', 100, 'acres', 1000, 'USD', 'OP-A', '2026-01-01', 'OPPORTUNITY'),
                ('OP-2', 'F-1', 'Bravo', 'NORTH', 200, 'acres', 2000, 'USD', 'OP-B', '2026-01-02', 'DUE_DILIGENCE'),
                ('OP-3', 'F-1', 'Charlie', 'SOUTH', 50, 'acres', 500, 'USD', 'OP-C', '2026-01-03', NULL),
                ('OP-4', 'F-2', 'Delta', 'SOUTH', 300, 'acres', 3000, 'USD', 'OP-D', '2026-01-04', 'SCREENING');

            INSERT INTO LEASE_PIPELINE_STAGE
                (LEASE_ID, STAGE_SEQ_NO, STAGE, OUTCOME, RATIONALE, ACTOR, DECIDED_AT)
            VALUES ('OP-1', 1, 'OPPORTUNITY', 'ADVANCED', 'lead', 'system', '2026-01-01');

            INSERT INTO LEASE_DILIGENCE_ITEM
                (LEASE_ID, ITEM_ID, CATEGORY, QUESTION, OUTCOME)
            VALUES ('OP-2', 'D-1', 'Title', 'Is the title clear?', 'OPEN');

            INSERT INTO LEASE_RIGHT
                (LEASE_ID, RIGHT_ID, RIGHT_TYPE, OWNER, INTEREST_PERCENT, ACTIVE_IND)
            VALUES
                ('OP-1', 'R-1', 'MINERAL', 'Landowner', 1.0, 'Y'),
                ('OP-1', 'R-2', 'SURFACE', 'Rancher', 0.5, 'Y'),
                ('OP-1', 'R-3', 'WORKING', 'Operator', 0.25, 'Y');

            INSERT INTO LEASE_NEGOTIATION
                (LEASE_ID, NEGOTIATION_ID, ROUND_NO, COUNTERPARTY, OFFER_AMOUNT, CURRENCY, NOTES, ACTOR, RECORDED_AT)
            VALUES ('OP-1', 'N-1', 1, 'LandCo', 900, 'USD', 'first offer', 'sara', '2026-02-01');
            """;
        command.ExecuteNonQuery();
    }

    private static void ForceStageInsertFailure(string database, string leaseId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_STAGE BEFORE INSERT ON LEASE_PIPELINE_STAGE
             WHEN NEW.LEASE_ID = '{leaseId}'
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropFailureTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_STAGE;";
        command.ExecuteNonQuery();
    }

    private static List<string> History(string database, string leaseId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT STAGE FROM LEASE_PIPELINE_STAGE WHERE LEASE_ID = '{leaseId}';";

        using var reader = command.ExecuteReader();
        var rows = new List<string>();

        while (reader.Read())
        {
            rows.Add(reader.GetString(0));
        }

        return rows;
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
