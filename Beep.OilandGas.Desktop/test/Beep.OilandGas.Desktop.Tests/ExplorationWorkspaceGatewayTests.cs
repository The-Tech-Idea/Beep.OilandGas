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
/// EXP-01 against a real SQLite database: the funnel is counted per stage and scoped to the field, the KPI counts
/// come from PLAY/LEAD/WELL, the incomplete-risk warning counts only the prospects missing elements, and a table
/// that disappears is a failure rather than a page of zeros.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class ExplorationWorkspaceGatewayTests
{
    private const string ConnectionName = "exp01-workspace";

    [IntegrationFact]
    public async Task The_funnel_and_the_warning_are_counted_from_records_for_the_field_only()
    {        var root = Path.Combine(OgPaths.DataRoot, "exp01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasExp01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "exploration.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "exploration.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var workspace = new OgExplorationWorkspace(() => source, reporter);

        // This account may open the well index and nothing else in the source list.
        static bool CanOpen(string route) => string.Equals(route, "wells", StringComparison.OrdinalIgnoreCase);

        // AC-1: counts from records, for this field only.
        var loaded = await workspace.LoadAsync("FIELD-1", "North", "2026", "Exploration geologist", CanOpen);
        Assert.True(loaded.Success, loaded.Message);
        var snapshot = loaded.Value!;

        Assert.Equal(3, snapshot.ProspectsInFunnel);
        Assert.Equal(2, snapshot.Funnel.Count);
        Assert.Equal("SCREENING", snapshot.Funnel[0].Stage);
        Assert.Equal(2, snapshot.Funnel[0].Count);
        Assert.Equal("DRILLING", snapshot.Funnel[1].Stage);
        Assert.Equal(1, snapshot.Funnel[1].Count);
        Assert.Equal(1, snapshot.Funnel[0].Sequence);

        // The other field's prospect is not in this field's funnel.
        Assert.DoesNotContain(snapshot.Funnel, stage => stage.Count > 2);

        Assert.Equal(2, snapshot.Kpis.Plays);
        Assert.Equal(3, snapshot.Kpis.Leads);
        Assert.Equal(1, snapshot.Kpis.WellsPlanned);
        Assert.Equal(1, snapshot.Kpis.WellsDrilling);

        // AC-3: only the two prospects with a missing element (or no assessment at all).
        Assert.Equal(2, snapshot.IncompleteRiskCount);
        Assert.Equal(["PROS-2", "PROS-4"], snapshot.IncompleteRiskProspects);
        Assert.DoesNotContain("PROS-1", snapshot.IncompleteRiskProspects);
        Assert.DoesNotContain("PROS-3", snapshot.IncompleteRiskProspects);

        Assert.True(snapshot.HasActivity);

        // AC-2: the link list is the permission answer, not a fixed panel.
        var wells = Assert.Single(snapshot.Links, link => link.Route == "wells");
        Assert.True(wells.Available);
        var board = Assert.Single(snapshot.Links, link => link.Route == "prospects");
        Assert.False(board.Available);
        Assert.Contains("EXP-02", board.WhyNot!, StringComparison.Ordinal);
        Assert.Single(snapshot.AvailableLinks, link => link.Route == "wells");

        // A session that carries only the field's name still finds records stored with that name.
        var byName = await workspace.LoadAsync(null, "South", "2026", "Exploration geologist", CanOpen);
        Assert.Equal(1, byName.Value!.ProspectsInFunnel);
        Assert.Equal("SCREENING", byName.Value.Funnel[0].Stage);

        // AC-1: an empty field is empty, not failed.
        var other = await workspace.LoadAsync("FIELD-NONE", "Nowhere", "2026", "Exploration geologist", CanOpen);
        Assert.True(other.Success);
        Assert.False(other.Value!.HasActivity);
        Assert.Equal(0, other.Value.ProspectsInFunnel);
        Assert.Equal(0, other.Value.Kpis.Plays);

        // AC-1: a table that disappears is a failure, with no counts.
        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var drop = connection.CreateCommand();
            drop.CommandText = "DROP TABLE PROSPECT_WORKFLOW_STAGE;";
            drop.ExecuteNonQuery();
        }

        var failed = await workspace.LoadAsync("FIELD-1", "North", "2026", "Exploration geologist", CanOpen);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
        Assert.NotNull(failed.Reference);
        Assert.NotEmpty(reporter.Failures);
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
                FIELD_ID TEXT NULL,
                PRIMARY_FIELD_ID TEXT NULL,
                PROSPECT_STATUS TEXT NULL);

            CREATE TABLE PROSPECT_WORKFLOW_STAGE (
                PROSPECT_ID TEXT NOT NULL,
                STAGE_SEQ_NO REAL NOT NULL,
                WORKFLOW_STAGE TEXT NOT NULL,
                PRIMARY KEY (PROSPECT_ID, STAGE_SEQ_NO));

            CREATE TABLE PROSPECT_RISK_ASSESSMENT (
                PROSPECT_ID TEXT NOT NULL,
                ASSESSMENT_ID TEXT NOT NULL,
                TRAP_RISK REAL NULL,
                RESERVOIR_RISK REAL NULL,
                SEAL_RISK REAL NULL,
                SOURCE_RISK REAL NULL,
                TIMING_RISK REAL NULL,
                PRIMARY KEY (PROSPECT_ID, ASSESSMENT_ID));

            CREATE TABLE PLAY (
                PLAY_ID TEXT NOT NULL PRIMARY KEY,
                PLAY_NAME TEXT NULL,
                FIELD_ID TEXT NULL);

            CREATE TABLE LEAD (
                LEAD_ID TEXT NOT NULL PRIMARY KEY,
                LEAD_NAME TEXT NULL,
                FIELD_ID TEXT NULL);

            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                CURRENT_STATUS TEXT NULL);

            INSERT INTO PROSPECT (PROSPECT_ID, PROSPECT_NAME, FIELD_ID, PROSPECT_STATUS) VALUES
                ('PROS-1', 'Alpha', 'FIELD-1', 'ACTIVE'),
                ('PROS-2', 'Bravo', 'FIELD-1', 'ACTIVE'),
                ('PROS-3', 'Charlie', 'FIELD-1', 'ACTIVE'),
                ('PROS-4', 'Delta', 'FIELD-1', 'ACTIVE'),
                ('PROS-9', 'Other field', 'FIELD-2', 'ACTIVE'),
                ('PROS-5', 'Name only', 'South', 'ACTIVE');

            INSERT INTO PROSPECT_WORKFLOW_STAGE (PROSPECT_ID, STAGE_SEQ_NO, WORKFLOW_STAGE) VALUES
                ('PROS-1', 1, 'SCREENING'),
                ('PROS-2', 1, 'SCREENING'),
                ('PROS-3', 2, 'DRILLING'),
                ('PROS-9', 1, 'SCREENING'),
                ('PROS-5', 1, 'SCREENING');

            INSERT INTO PROSPECT_RISK_ASSESSMENT
                (PROSPECT_ID, ASSESSMENT_ID, TRAP_RISK, RESERVOIR_RISK, SEAL_RISK, SOURCE_RISK, TIMING_RISK)
            VALUES
                ('PROS-1', 'A1', 0.2, 0.3, 0.1, 0.4, 0.2),
                ('PROS-3', 'A3', 0.2, 0.3, 0.1, 0.4, 0.2),
                ('PROS-4', 'A4', 0.2, NULL, 0.1, 0.4, 0.2);

            INSERT INTO PLAY (PLAY_ID, PLAY_NAME, FIELD_ID) VALUES
                ('PLAY-1', 'Upper sand', 'FIELD-1'),
                ('PLAY-2', 'Lower sand', 'FIELD-1'),
                ('PLAY-9', 'Other field play', 'FIELD-2');

            INSERT INTO LEAD (LEAD_ID, LEAD_NAME, FIELD_ID) VALUES
                ('LEAD-1', 'Lead one', 'FIELD-1'),
                ('LEAD-2', 'Lead two', 'FIELD-1'),
                ('LEAD-3', 'Lead three', 'FIELD-1'),
                ('LEAD-9', 'Other lead', 'FIELD-2');

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, CURRENT_STATUS) VALUES
                ('UWI-1', 'Exploration one', 'FIELD-1', 'PLANNED'),
                ('UWI-2', 'Exploration two', 'FIELD-1', 'DRILLING'),
                ('UWI-3', 'Abandoned one', 'FIELD-1', 'ABANDONED'),
                ('UWI-9', 'Other field well', 'FIELD-2', 'PLANNED');
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
