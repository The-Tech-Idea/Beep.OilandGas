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
/// EXP-03 against a real SQLite database: the editor reads every tab, produces the chance from the recorded
/// probabilities, refuses to edit a submitted prospect and revises it into a new draft instead, records the
/// assessor and date on every risk save, and keeps the economic summary read-only.
/// </summary>
/// <remarks>Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>): it provisions a local database through BeepDM.</remarks>
public sealed class ProspectEditorGatewayTests
{
    private const string ConnectionName = "exp03-editor";

    [IntegrationFact]
    public async Task The_editor_reads_edits_saves_risk_and_revises_a_submitted_prospect()
    {        var root = Path.Combine(OgPaths.DataRoot, "exp03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasExp03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "editor.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "editor.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var editor = new OgProspectEditor(() => source, reporter);

        // ---- reading every tab -------------------------------------------------
        var loaded = await editor.LoadAsync("P-1");
        Assert.True(loaded.Success, loaded.Message);
        var draft = loaded.Value!;

        Assert.Equal("Alpha", draft.Identity.Name);
        Assert.Equal("Upper sand", draft.Identity.PlayName);
        Assert.Equal("BA-1", draft.Identity.OperatorId);
        Assert.False(draft.IsLocked);

        Assert.Equal("THREE_WAY", draft.Trap.TrapType);
        Assert.Equal(12.5m, draft.Trap.ClosureArea);
        Assert.Equal("SANDSTONE", draft.Reservoir.Lithology);
        Assert.Equal(0.22m, draft.Reservoir.Porosity);
        Assert.Equal(1.5m, draft.Source.Toc);

        // AC-3: the percentiles read with their unit and in order.
        Assert.True(draft.Volume.IsOrdered);
        Assert.True(draft.Volume.HasUnit);
        Assert.Equal("MMBO", draft.Volume.Unit);
        Assert.Equal([10m, 20m, 30m], new[] { draft.Volume.Low, draft.Volume.Mid, draft.Volume.High });

        // AC-1: 0.9 * 0.8 * 0.9 * 0.9 * 0.9 = 52%.
        Assert.True(draft.RiskComplete);
        Assert.Equal(52m, draft.ChancePercent);
        Assert.Equal("52%", draft.ChanceText);

        // AC-5: every element carries the assessment's assessor and date.
        Assert.All(draft.Risk, element =>
        {
            Assert.Equal("geologist", element.Assessor);
            Assert.Equal(new DateTime(2026, 2, 1), element.AssessedAt);
        });

        // AC-6: the economic summary is read.
        Assert.Equal("BASE", draft.Economics.Scenario);
        Assert.Equal(1_000_000m, draft.Economics.Npv);
        Assert.Equal("USD", draft.Economics.Currency);

        Assert.Equal("APPRAISAL", draft.History[0].Stage);

        // AC-2: a prospect with one element missing reads "incomplete".
        var partial = (await editor.LoadAsync("P-3")).Value!;
        Assert.False(partial.RiskComplete);
        Assert.Null(partial.ChancePercent);
        Assert.Equal("incomplete", partial.ChanceText);
        Assert.Equal(["SEAL"], partial.MissingElements);

        // ---- editing a draft ---------------------------------------------------
        var historyBefore = HistoryCount(database, "P-1");

        var saved = await editor.SaveIdentityAsync(
            draft.Identity, "Alpha renamed", "PLAY-2", "BA-2", "renamed in the editor", "fahad");
        Assert.True(saved.Success, saved.Message);

        var reread = (await editor.LoadAsync("P-1")).Value!;
        Assert.Equal("Alpha renamed", reread.Identity.Name);
        Assert.Equal("PLAY-2", reread.Identity.PlayId);
        Assert.Equal("Lower sand", reread.Identity.PlayName);
        Assert.Equal("BA-2", reread.Identity.OperatorId);
        Assert.Equal(historyBefore + 1, HistoryCount(database, "P-1"));

        // AC-1 again, from a saved change: 0.9 * 0.8 * 0.5 * 0.9 * 0.9 = 29%.
        var riskSaved = await editor.SaveRiskAsync(
            "P-1",
            [new OgProspectRiskElement("SEAL", 0.5m, null, "sara", new DateTime(2026, 3, 2))],
            "sara",
            new DateTime(2026, 3, 2));

        Assert.True(riskSaved.Success, riskSaved.Message);

        var afterRisk = (await editor.LoadAsync("P-1")).Value!;
        Assert.Equal(29m, afterRisk.ChancePercent);
        Assert.All(afterRisk.Risk, element =>
        {
            Assert.Equal("sara", element.Assessor);
            Assert.Equal(new DateTime(2026, 3, 2), element.AssessedAt);
        });

        // ---- AC-4: a submitted prospect is not edited --------------------------
        var submitted = (await editor.LoadAsync("P-2")).Value!;
        Assert.True(submitted.IsLocked);

        var refusedEdit = await editor.SaveIdentityAsync(
            submitted.Identity, "Renamed", null, null, null, "fahad");
        Assert.False(refusedEdit.Success);
        Assert.Equal(OgProspectEditor.CodeLocked, refusedEdit.Reference);

        var refusedRisk = await editor.SaveRiskAsync(
            "P-2",
            [new OgProspectRiskElement("TRAP", 0.5m, null, "fahad", DateTime.Today)],
            "fahad",
            DateTime.Today);
        Assert.Equal(OgProspectEditor.CodeLocked, refusedRisk.Reference);

        var stillSubmitted = (await editor.LoadAsync("P-2")).Value!;
        Assert.Equal("Bravo", stillSubmitted.Identity.Name);
        Assert.Equal("SUBMITTED", stillSubmitted.Identity.Status);

        // A draft is edited, not revised.
        var wrongRevise = await editor.ReviseAsync("P-1", "P-1R", "fahad");
        Assert.False(wrongRevise.Success);
        Assert.Equal(OgProspectEditor.CodeRevisionNeedsLocked, wrongRevise.Reference);

        // A revision needs a new identifier.
        var duplicate = await editor.ReviseAsync("P-2", "P-1", "fahad");
        Assert.Equal(OgProspectEditor.CodeRevisionExists, duplicate.Reference);

        // ---- AC-4: a revision is a new version --------------------------------
        var revised = await editor.ReviseAsync("P-2", "P-2R", "fahad");
        Assert.True(revised.Success, revised.Message);

        var newVersion = revised.Value!;
        Assert.Equal("P-2R", newVersion.Identity.Id);
        Assert.Equal("DRAFT", newVersion.Identity.Status);
        Assert.False(newVersion.IsLocked);
        Assert.Equal(52m, newVersion.ChancePercent);
        Assert.All(newVersion.Risk, element => Assert.Equal("geologist", element.Assessor));

        // A new prospect starts with no stage history of its own.
        Assert.Empty(newVersion.History);

        Assert.Equal(1, HistoryCount(database, "P-2R"));

        // The submitted original is untouched.
        var original = (await editor.LoadAsync("P-2")).Value!;
        Assert.Equal("SUBMITTED", original.Identity.Status);

        // A missing prospect is a refusal, not an empty editor.
        var unknown = await editor.LoadAsync("P-NOPE");
        Assert.False(unknown.Success);
        Assert.Null(unknown.Value);
        Assert.Equal(OgProspectEditor.CodeUnknown, unknown.Reference);
    }

    private static long HistoryCount(string database, string prospectId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM PROSPECT_HISTORY WHERE PROSPECT_ID = '{prospectId}';";
        return (long)command.ExecuteScalar()!;
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE PLAY (
                PLAY_ID TEXT NOT NULL PRIMARY KEY,
                PLAY_NAME TEXT NULL,
                BASIN_ID TEXT NULL);

            CREATE TABLE PROSPECT (
                PROSPECT_ID TEXT NOT NULL PRIMARY KEY,
                PROSPECT_NAME TEXT NULL,
                PROSPECT_SHORT_NAME TEXT NULL,
                PRIMARY_FIELD_ID TEXT NULL,
                PLAY_ID TEXT NULL,
                PROSPECT_TYPE TEXT NULL,
                PROSPECT_STATUS TEXT NULL,
                RISK_LEVEL TEXT NULL,
                CURRENT_OPERATOR TEXT NULL,
                DESCRIPTION TEXT NULL,
                FIELD_ID TEXT NULL,
                ESTIMATED_VOLUME_OUOM TEXT NULL,
                ESTIMATED_VALUE_CURRENCY TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE PROSPECT_TRAP (
                PROSPECT_ID TEXT NOT NULL,
                TRAP_ID TEXT NOT NULL,
                TRAP_TYPE TEXT NULL,
                CLOSURE_AREA REAL NULL,
                CLOSURE_AREA_OUOM TEXT NULL,
                SEAL_QUALITY TEXT NULL,
                TRAP_CONFIDENCE TEXT NULL,
                DESCRIPTION TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, TRAP_ID));

            CREATE TABLE PROSPECT_RESERVOIR (
                PROSPECT_ID TEXT NOT NULL,
                RESERVOIR_ID TEXT NOT NULL,
                RESERVOIR_NAME TEXT NULL,
                STRAT_UNIT_ID TEXT NULL,
                LITHOLOGY TEXT NULL,
                POROSITY REAL NULL,
                PERMEABILITY REAL NULL,
                NET_PAY REAL NULL,
                NET_PAY_OUOM TEXT NULL,
                FLUID_TYPE TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, RESERVOIR_ID));

            CREATE TABLE PROSPECT_SOURCE_ROCK (
                PROSPECT_ID TEXT NOT NULL,
                SOURCE_ID TEXT NOT NULL,
                STRAT_UNIT_ID TEXT NULL,
                TOC REAL NULL,
                MATURITY_STAGE TEXT NULL,
                MIGRATION_DISTANCE REAL NULL,
                MIGRATION_DISTANCE_OUOM TEXT NULL,
                DESCRIPTION TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, SOURCE_ID));

            CREATE TABLE PROSPECT_VOLUME_ESTIMATE (
                PROSPECT_ID TEXT NOT NULL,
                ESTIMATE_ID TEXT NOT NULL,
                VOLUME_OUOM TEXT NULL,
                ESTIMATE_DATE TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, ESTIMATE_ID));

            CREATE TABLE PROSPECT_RISK_ASSESSMENT (
                PROSPECT_ID TEXT NOT NULL,
                ASSESSMENT_ID TEXT NOT NULL,
                RISK_MODEL_TYPE TEXT NULL,
                TRAP_RISK REAL NULL,
                RESERVOIR_RISK REAL NULL,
                SEAL_RISK REAL NULL,
                SOURCE_RISK REAL NULL,
                TIMING_RISK REAL NULL,
                OVERALL_GEOLOGICAL_RISK REAL NULL,
                POS_GEOLOGICAL REAL NULL,
                ASSESSOR TEXT NULL,
                ASSESSMENT_DATE TEXT NULL,
                RISK_FACTORS TEXT NULL,
                EMV REAL NULL,
                NPV REAL NULL,
                LOW_ESTIMATE_OIL REAL NULL,
                BEST_ESTIMATE_OIL REAL NULL,
                HIGH_ESTIMATE_OIL REAL NULL,
                LOW_ESTIMATE_GAS REAL NULL,
                BEST_ESTIMATE_GAS REAL NULL,
                HIGH_ESTIMATE_GAS REAL NULL,
                FLUID_TYPE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, ASSESSMENT_ID));

            CREATE TABLE PROSPECT_ECONOMIC (
                PROSPECT_ID TEXT NOT NULL,
                ECONOMIC_ID TEXT NOT NULL,
                ECONOMIC_SCENARIO TEXT NULL,
                EVALUATION_DATE TEXT NULL,
                EVALUATOR TEXT NULL,
                DESCRIPTION TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, ECONOMIC_ID));

            CREATE TABLE PROSPECT_HISTORY (
                PROSPECT_ID TEXT NOT NULL,
                HISTORY_SEQ_NO REAL NOT NULL,
                FIELD_NAME TEXT NULL,
                OLD_VALUE TEXT NULL,
                NEW_VALUE TEXT NULL,
                CHANGE_REASON TEXT NULL,
                CHANGED_BY TEXT NULL,
                CHANGE_DATE TEXT NULL,
                DESCRIPTION TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, HISTORY_SEQ_NO));

            CREATE TABLE PROSPECT_WORKFLOW_STAGE (
                PROSPECT_ID TEXT NOT NULL,
                STAGE_SEQ_NO REAL NOT NULL,
                WORKFLOW_STAGE TEXT NOT NULL,
                STAGE_START_DATE TEXT NULL,
                STAGE_END_DATE TEXT NULL,
                STAGE_OUTCOME TEXT NULL,
                PRIMARY KEY (PROSPECT_ID, STAGE_SEQ_NO));

            INSERT INTO PLAY (PLAY_ID, PLAY_NAME, BASIN_ID) VALUES
                ('PLAY-1', 'Upper sand', 'BASIN-1'),
                ('PLAY-2', 'Lower sand', 'BASIN-2');

            INSERT INTO PROSPECT
                (PROSPECT_ID, PROSPECT_NAME, PLAY_ID, PROSPECT_TYPE, PROSPECT_STATUS, CURRENT_OPERATOR,
                 DESCRIPTION, FIELD_ID, ESTIMATED_VALUE_CURRENCY)
            VALUES
                ('P-1', 'Alpha', 'PLAY-1', 'LEAD', 'DRAFT', 'BA-1', 'first', 'FIELD-1', 'USD'),
                ('P-2', 'Bravo', 'PLAY-1', 'LEAD', 'SUBMITTED', 'BA-1', 'second', 'FIELD-1', 'USD'),
                ('P-3', 'Charlie', 'PLAY-1', 'LEAD', 'DRAFT', 'BA-1', 'third', 'FIELD-1', 'USD');

            INSERT INTO PROSPECT_TRAP
                (PROSPECT_ID, TRAP_ID, TRAP_TYPE, CLOSURE_AREA, CLOSURE_AREA_OUOM, SEAL_QUALITY, TRAP_CONFIDENCE,
                 DESCRIPTION)
            VALUES ('P-1', 'T-1', 'THREE_WAY', 12.5, 'km2', 'GOOD', 'HIGH', 'a four-way dip closure');

            INSERT INTO PROSPECT_RESERVOIR
                (PROSPECT_ID, RESERVOIR_ID, RESERVOIR_NAME, STRAT_UNIT_ID, LITHOLOGY, POROSITY, PERMEABILITY,
                 NET_PAY, NET_PAY_OUOM, FLUID_TYPE)
            VALUES ('P-1', 'R-1', 'Upper reservoir', 'SU-1', 'SANDSTONE', 0.22, 250, 15, 'm', 'OIL');

            INSERT INTO PROSPECT_SOURCE_ROCK
                (PROSPECT_ID, SOURCE_ID, STRAT_UNIT_ID, TOC, MATURITY_STAGE, MIGRATION_DISTANCE,
                 MIGRATION_DISTANCE_OUOM, DESCRIPTION)
            VALUES ('P-1', 'S-1', 'SU-2', 1.5, 'MATURE', 25, 'km', 'good source');

            INSERT INTO PROSPECT_VOLUME_ESTIMATE (PROSPECT_ID, ESTIMATE_ID, VOLUME_OUOM, ESTIMATE_DATE) VALUES
                ('P-1', 'V-1', 'MMBO', '2026-01-15');

            INSERT INTO PROSPECT_RISK_ASSESSMENT
                (PROSPECT_ID, ASSESSMENT_ID, RISK_MODEL_TYPE, TRAP_RISK, RESERVOIR_RISK, SEAL_RISK, SOURCE_RISK,
                 TIMING_RISK, OVERALL_GEOLOGICAL_RISK, POS_GEOLOGICAL, ASSESSOR, ASSESSMENT_DATE, RISK_FACTORS,
                 EMV, NPV, LOW_ESTIMATE_OIL, BEST_ESTIMATE_OIL, HIGH_ESTIMATE_OIL, FLUID_TYPE)
            VALUES
                ('P-1', 'A-1', 'GEOLOGICAL', 0.1, 0.2, 0.1, 0.1, 0.1, 0.48, 0.52, 'geologist', '2026-02-01',
                 'trap and seal are well understood', 500000, 1000000, 10, 20, 30, 'OIL'),
                ('P-2', 'A-2', 'GEOLOGICAL', 0.1, 0.2, 0.1, 0.1, 0.1, 0.48, 0.52, 'geologist', '2026-02-01',
                 'complete', 400000, 900000, 8, 16, 24, 'OIL'),
                ('P-3', 'A-3', 'GEOLOGICAL', 0.1, 0.2, NULL, 0.1, 0.1, NULL, NULL, 'geologist', '2026-02-01',
                 'seal not assessed', NULL, NULL, 5, 10, 15, 'OIL');

            INSERT INTO PROSPECT_ECONOMIC
                (PROSPECT_ID, ECONOMIC_ID, ECONOMIC_SCENARIO, EVALUATION_DATE, EVALUATOR, DESCRIPTION)
            VALUES ('P-1', 'E-1', 'BASE', '2026-01-20', 'economist', 'base case');

            INSERT INTO PROSPECT_HISTORY
                (PROSPECT_ID, HISTORY_SEQ_NO, FIELD_NAME, OLD_VALUE, NEW_VALUE, CHANGE_REASON, CHANGED_BY,
                 CHANGE_DATE)
            VALUES ('P-1', 1, 'IDENTITY', 'Alpha', 'Alpha', 'created', 'system', '2026-01-01');

            INSERT INTO PROSPECT_WORKFLOW_STAGE
                (PROSPECT_ID, STAGE_SEQ_NO, WORKFLOW_STAGE, STAGE_START_DATE, STAGE_END_DATE, STAGE_OUTCOME)
            VALUES
                ('P-1', 1.0, 'SCREENING', '2025-12-01', '2026-02-01', 'APPROVED'),
                ('P-1', 2.0, 'APPRAISAL', '2026-02-01', NULL, NULL);
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
