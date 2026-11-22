using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-01 against a real SQLite database: the counts, the date-derived and program attention, the dated spend, the
/// source gates, the field scope, the hidden attention strip when the program table is absent, and a failed read that
/// is a failure rather than zeroes. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DevelopmentWorkspaceGatewayTests
{
    private const string ConnectionName = "dev01-workspace";

    [IntegrationFact]
    public async Task The_workspace_counts_records_derives_attention_and_hides_an_unavailable_strip()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "development.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "development.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var workspace = new OgDevelopmentWorkspace(() => source, reporter);
        var asOf = new DateTime(2026, 6, 1);

        var loaded = await workspace.LoadAsync("F-1", "Alpha field", asOf, route => route == "wells");

        Assert.True(loaded.Success, loaded.Message);
        var snapshot = loaded.Value!;

        Assert.Equal(3, snapshot.PlanCount);
        Assert.Equal(1, snapshot.WellsPlanned);
        Assert.Equal(1, snapshot.WellsDrilling);
        Assert.Equal(1, snapshot.FacilitiesInConstruction);
        Assert.Equal(115m, snapshot.Spend.Single(spend => spend.Currency == "USD").Amount);
        Assert.Equal(20m, snapshot.Spend.Single(spend => spend.Currency == "EUR").Amount);

        // Attention is derived: one plan past the review window and one well without an approved program.
        Assert.Equal(2, snapshot.Attention.Count);
        Assert.Contains(snapshot.Attention, item => item.Kind == OgDevelopmentAttention.PlanReview && item.Subject == "Draft plan");
        Assert.Contains(snapshot.Attention, item => item.Kind == OgDevelopmentAttention.ProgramMissing && item.Subject == "WELL-1");

        // The source links are gated one by one.
        Assert.True(snapshot.Links.Single(link => link.Route == "wells").Available);
        Assert.False(snapshot.Links.Single(link => link.Route == "fdpwizard").Available);

        // No field selection shows both fields.
        var all = await workspace.LoadAsync(null, string.Empty, asOf, _ => false);

        Assert.True(all.Success, all.Message);
        Assert.Equal(4, all.Value!.PlanCount);

        // Without the program table the strip is hidden with a note; the workspace still reads.
        Drop(database, "DRILLING_PROGRAM");
        var hidden = await workspace.LoadAsync("F-1", "Alpha field", asOf, _ => true);

        Assert.True(hidden.Success, hidden.Message);
        Assert.False(hidden.Value!.AttentionAvailable);
        Assert.NotNull(hidden.Value.AttentionNote);
        Assert.Contains(hidden.Value.Attention, item => item.Kind == OgDevelopmentAttention.PlanReview);
        Assert.DoesNotContain(hidden.Value.Attention, item => item.Kind == OgDevelopmentAttention.ProgramMissing);

        // AC-1: a failed plan read is a failure, not zero counts.
        Drop(database, "FIELD_DEVELOPMENT_PLAN");
        var failed = await workspace.LoadAsync("F-1", "Alpha field", asOf, _ => true);

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Null(failed.Value);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD_DEVELOPMENT_PLAN (
                FDP_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FDP_NAME TEXT NULL,
                FDP_STATUS TEXT NULL,
                SUBMISSION_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_WELL_SCHEDULE (
                SCHEDULE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PLANNED_WELL_ID TEXT NULL,
                WELL_NAME TEXT NULL,
                SCHEDULE_STATUS TEXT NULL);

            CREATE TABLE FACILITY_INVESTMENT (
                FACILITY_INV_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                FACILITY_NAME TEXT NULL,
                INVESTMENT_PHASE TEXT NULL,
                PLANNED_START_DATE TEXT NULL,
                PLANNED_COMPLETION_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_COSTS (
                DEVELOPMENT_COST_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FDP_ID TEXT NULL,
                COST_DATE TEXT NULL,
                COST_CURRENCY TEXT NULL,
                TOTAL_DEVELOPMENT_COSTS REAL NULL,
                DEVELOPMENT_WELL_DRILLING_COSTS REAL NULL,
                DEVELOPMENT_WELL_EQUIPMENT REAL NULL);

            CREATE TABLE DRILLING_PROGRAM (
                PROGRAM_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PROGRAM_NAME TEXT NULL,
                PROGRAM_STATUS TEXT NULL);

            INSERT INTO FIELD_DEVELOPMENT_PLAN (FDP_ID, FIELD_ID, FDP_NAME, FDP_STATUS, SUBMISSION_DATE) VALUES
                ('FDP-1', 'F-1', 'Approved plan', 'APPROVED', '2026-01-01'),
                ('FDP-2', 'F-1', 'Draft plan', 'DRAFT', '2026-04-01'),
                ('FDP-3', 'F-1', 'New draft', 'DRAFT', '2026-05-30'),
                ('FDP-4', 'F-2', 'Other field', 'DRAFT', '2026-04-01');

            INSERT INTO DEVELOPMENT_WELL_SCHEDULE
                (SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID, WELL_NAME, SCHEDULE_STATUS) VALUES
                ('S-1', 'FDP-1', 'F-1', 'WELL-1', 'WELL-1', 'PLANNED'),
                ('S-2', 'FDP-1', 'F-1', 'WELL-2', 'WELL-2', 'DRILLING'),
                ('S-3', 'FDP-1', 'F-1', 'WELL-3', 'WELL-3', 'COMPLETED');

            INSERT INTO FACILITY_INVESTMENT
                (FACILITY_INV_ID, FDP_ID, FIELD_ID, FACILITY_NAME, INVESTMENT_PHASE, PLANNED_START_DATE,
                 PLANNED_COMPLETION_DATE) VALUES
                ('FI-1', 'FDP-1', 'F-1', 'Central facility', 'CONSTRUCTION', '2026-01-01', '2026-12-01'),
                ('FI-2', 'FDP-1', 'F-1', 'Future facility', 'PLANNED', '2026-07-01', '2027-01-01'),
                ('FI-3', 'FDP-1', 'F-1', 'Completed facility', 'COMPLETED', '2025-01-01', '2026-05-01');

            INSERT INTO DEVELOPMENT_COSTS
                (DEVELOPMENT_COST_ID, FIELD_ID, FDP_ID, COST_DATE, COST_CURRENCY, TOTAL_DEVELOPMENT_COSTS,
                 DEVELOPMENT_WELL_DRILLING_COSTS, DEVELOPMENT_WELL_EQUIPMENT) VALUES
                ('C-1', 'F-1', 'FDP-1', '2026-05-01', 'USD', 100, NULL, NULL),
                ('C-2', 'F-1', 'FDP-1', '2026-07-01', 'USD', 900, NULL, NULL),
                ('C-3', 'F-1', 'FDP-1', NULL, 'EUR', 20, NULL, NULL),
                ('C-4', 'F-1', 'FDP-1', '2026-05-20', 'USD', NULL, 10, 5);

            INSERT INTO DRILLING_PROGRAM (PROGRAM_ID, WELL_ID, FIELD_ID, PROGRAM_NAME, PROGRAM_STATUS) VALUES
                ('PR-1', 'WELL-2', 'F-1', 'WELL-2', 'APPROVED');
            """;
        command.ExecuteNonQuery();
    }

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
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
