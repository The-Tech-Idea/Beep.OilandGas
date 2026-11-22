using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-06 against a real SQLite database: the diagnosis is assembled from the records PRD-01, PRD-03, PRD-05 and WEL-04
/// already wrote (AC-1), every option states its basis (AC-2) and a gain carries its unit (AC-3), a decision records its
/// rationale and owner (AC-4), and the work order carries the decision and is raised once (AC-5) - including the case
/// where the creation is not acknowledged and is retried. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class InterventionGatewayTests
{
    private const string ConnectionName = "prd06-interventions";
    private static readonly DateTime Day = new(2026, 10, 1);

    [IntegrationFact]
    public async Task The_diagnosis_comes_from_the_records_and_a_decision_raises_one_work_order()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "interventions.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "interventions.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgInterventionService(() => source, reporter);

        // ---- the workspace and its diagnoses --------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field", null, Day);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal(3, workspace.Wells.Count);                    // the other field's well is absent
        Assert.DoesNotContain(workspace.Wells, well => well.WellId == "WELL-9");
        Assert.Equal(["ACIDIZE", "ESP", "GAS_LIFT"], workspace.Methods);   // the inactive code is not offered
        Assert.False(workspace.MethodReferenceMissing);

        // WELL-1: a declining rate, an open deferment and a material variance - four evidenced checks, no conflict.
        var well1 = workspace.Wells.Single(well => well.WellId == "WELL-1");

        Assert.Equal("producing", well1.StatusText);
        Assert.Equal(60m, well1.Rate);
        Assert.Equal(100m, well1.Average);
        Assert.Equal(7, well1.HistoryDays);
        Assert.Equal(-40m, well1.Change);
        Assert.Equal("-40 bbl", well1.ChangeText);

        Assert.Equal(4, well1.Evidence.Count);
        Assert.Contains(well1.Evidence, item => item.Kind == "RATE" && item.IsNegative);
        Assert.Contains(well1.Evidence, item => item.Kind == "DEFERMENT" && item.IsNegative);
        Assert.Contains(well1.Evidence, item => item.Kind == "VARIANCE");
        Assert.Contains(well1.Evidence, item => item.Kind == "STATUS" && item.IsPositive);

        Assert.True(well1.IsUnderperforming);
        Assert.False(well1.IsInsufficient);
        Assert.False(well1.HasConflicts);
        Assert.Equal(1m, well1.Confidence);
        Assert.Contains("the rate fell 40 bbl", well1.FlagsText, StringComparison.Ordinal);
        Assert.Contains("EQ-FAIL", well1.EvidenceText, StringComparison.Ordinal);
        Assert.Contains("2026-10", well1.EvidenceText, StringComparison.Ordinal);

        // WELL-2: no measurement and no status, so the diagnosis is "insufficient data" with the missing items (AC-1).
        var well2 = workspace.Wells.Single(well => well.WellId == "WELL-2");

        Assert.True(well2.IsInsufficient);
        Assert.Empty(well2.Evidence);
        Assert.Equal(0m, well2.Confidence);
        Assert.Contains("insufficient data", well2.DiagnosisText, StringComparison.Ordinal);
        Assert.Contains("no measured volume for the well", well2.DiagnosisText, StringComparison.Ordinal);
        Assert.Contains("no status recorded", well2.DiagnosisText, StringComparison.Ordinal);

        // WELL-3: recorded as suspended but measured above its own average - the evidence conflicts and is not resolved.
        var well3 = workspace.Wells.Single(well => well.WellId == "WELL-3");

        Assert.True(well3.HasConflicts);
        Assert.Contains("recorded as down", well3.DiagnosisText, StringComparison.Ordinal);
        Assert.Equal(0.5m, well3.Confidence);
        Assert.Contains("capped", well3.ConfidenceText, StringComparison.Ordinal);

        Assert.Contains("3 well(s)", workspace.Summary, StringComparison.Ordinal);
        Assert.Equal(2, workspace.UnderperformingCount);

        // ---- the option and decision rules (AC-2, AC-3, AC-4) ---------------
        var noBasis = await service.RecordDecisionAsync(
            Values(options: [Option(basis: null)]), "sara");

        Assert.False(noBasis.Success);
        Assert.Equal(InterventionRules.CodeBasisRequired, noBasis.Reference);

        var noUnit = await service.RecordDecisionAsync(
            Values(options: [Option(gain: 120m, gainUnit: null)]), "sara");

        Assert.False(noUnit.Success);
        Assert.Equal(InterventionRules.CodeGainUnitRequired, noUnit.Reference);

        var unknownMethod = await service.RecordDecisionAsync(
            Values(options: [Option(method: "TELEPORT")]), "sara");

        Assert.False(unknownMethod.Success);
        Assert.Equal(InterventionRules.CodeMethodUnknown, unknownMethod.Reference);

        var noRationale = await service.RecordDecisionAsync(Values(rationale: " "), "sara");

        Assert.False(noRationale.Success);
        Assert.Equal(InterventionRules.CodeRationaleRequired, noRationale.Reference);

        var unknownWell = await service.RecordDecisionAsync(Values(well: "WELL-404"), "sara");

        Assert.False(unknownWell.Success);
        Assert.Equal(InterventionRules.CodeWellUnknown, unknownWell.Reference);

        Assert.Equal(0, CountRows(database, "INTERVENTION_DECISION"));

        // ---- AC-4: a decision with its options -------------------------------
        var recorded = await service.RecordDecisionAsync(Values(), "sara");

        Assert.True(recorded.Success, recorded.Message);

        var decision = Assert.Single(recorded.Value!.Decisions);

        Assert.Equal(OgInterventionDecision.DecidedState, decision.State);
        Assert.Equal("sara", decision.Owner);
        Assert.Contains("the rate fell 40%", decision.Rationale!, StringComparison.Ordinal);
        Assert.Equal(Day.AddDays(30), decision.TargetDate);
        Assert.Equal(2, decision.Options.Count);
        Assert.Equal("GAS_LIFT", decision.Chosen!.Method);
        Assert.True(decision.Chosen.LiftRelated);
        Assert.True(decision.Chosen.IsChosen);
        Assert.Equal(InterventionRules.BasisTest, decision.Chosen.Basis);
        Assert.Equal("120 bbl", decision.Chosen.GainText);
        Assert.Contains("valve test", decision.Chosen.BasisDescription, StringComparison.Ordinal);
        Assert.False(decision.HasWorkOrder);
        Assert.Equal("no work order", decision.WorkOrderLink);

        Assert.Equal(1, CountRows(database, "INTERVENTION_DECISION"));
        Assert.Equal(2, CountRows(database, "INTERVENTION_OPTION"));

        // Exactly one option of the decision carries CHOSEN_IND.
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM INTERVENTION_OPTION WHERE CHOSEN_IND = 'Y';"));

        // ---- AC-5: the work order carries the decision and is raised once ---
        var raised = await service.CreateWorkOrderAsync(decision.Id, "WO-0001", "sara");

        Assert.True(raised.Success, raised.Message);

        var acted = raised.Value!.Decisions.Single(item => item.Id == decision.Id);

        Assert.True(acted.IsActed);
        Assert.Equal("WO-0001", acted.WorkOrderRef);
        Assert.Equal("sara", acted.WorkOrderBy);
        Assert.NotNull(acted.WorkOrderAt);
        Assert.Contains(decision.Id, acted.WorkOrderLink, StringComparison.Ordinal);
        Assert.Contains("WO-0001", acted.WorkOrderLink, StringComparison.Ordinal);

        var twice = await service.CreateWorkOrderAsync(decision.Id, "WO-0002", "sara");

        Assert.False(twice.Success);
        Assert.Equal(InterventionRules.CodeWorkOrderExists, twice.Reference);
        Assert.Contains("WO-0001", twice.Message!, StringComparison.Ordinal);
        Assert.Equal(1, CountRows(database, "INTERVENTION_DECISION"));

        // ---- the state table's "creation lost": retained and retried --------
        var second = await service.RecordDecisionAsync(
            Values(
                well: "WELL-3",
                options: [Option(method: "ESP", basis: InterventionRules.BasisModel)],
                chosen: "ESP"),
            "sara");

        Assert.True(second.Success, second.Message);

        var pendingDecision = second.Value!.Decisions.Single(item => item.WellId == "WELL-3");

        ForceWorkOrderFailure(database, pendingDecision.Id);

        var lost = await service.CreateWorkOrderAsync(pendingDecision.Id, "WO-0002", "sara");

        Assert.False(lost.Success);

        Assert.NotNull(service.Pending);
        Assert.Equal(pendingDecision.Id, service.Pending!.DecisionId);
        Assert.Equal("WO-0002", service.Pending.Reference);

        // The decision is unchanged: still decided, still without a work order.
        var stillOpen = await service.LoadAsync("F-1", "Alpha field", "WELL-3", Day);

        Assert.True(stillOpen.Success, stillOpen.Message);

        var unchanged = stillOpen.Value!.Decisions.Single(item => item.Id == pendingDecision.Id);

        Assert.False(unchanged.HasWorkOrder);
        Assert.False(unchanged.IsActed);

        DropWorkOrderFailure(database);

        var retried = await service.RetryWorkOrderAsync(null, "sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);

        var actedSecond = retried.Value!.Decisions.Single(item => item.Id == pendingDecision.Id);

        Assert.True(actedSecond.IsActed);
        Assert.Equal("WO-0002", actedSecond.WorkOrderRef);
        Assert.Equal(2, CountRows(database, "INTERVENTION_DECISION"));

        // ---- an empty method reference set refuses every option -------------
        ClearMethods(database);

        var withoutMethods = await service.LoadAsync("F-1", "Alpha field", null, Day);

        Assert.True(withoutMethods.Value!.MethodReferenceMissing);
        Assert.Contains("R_INTERVENTION_METHOD", withoutMethods.Value.MethodsText, StringComparison.Ordinal);

        var refused = await service.RecordDecisionAsync(Values(), "sara");

        Assert.False(refused.Success);
        Assert.Equal(InterventionRules.CodeMethodReferenceMissing, refused.Reference);

        // ---- a failed read is a failure -------------------------------------
        Drop(database, "INTERVENTION_OPTION");

        var failed = await service.LoadAsync("F-1", "Alpha field", null, Day);

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Null(failed.Value);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        var volumes = new System.Text.StringBuilder();

        // WELL-1 falls from 100 to 60 bbl/d on the day; WELL-3 rises from 50 to 100 while recorded suspended.
        for (var index = 0; index < 8; index++)
        {
            var date = Day.AddDays(-index);
            var wellOne = index == 0 ? 60 : 100;
            var wellThree = index == 0 ? 100 : 50;

            volumes.AppendLine(
                "INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, " +
                "ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME, " +
                "WATER_VOLUME_OUOM) VALUES " +
                $"('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '{date:yyyy-MM-dd}', {wellOne}, 'bbl', 0, 'MMscf', 0, 'bbl');");

            volumes.AppendLine(
                "INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, " +
                "ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME, " +
                "WATER_VOLUME_OUOM) VALUES " +
                $"('PD-3', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '{date:yyyy-MM-dd}', {wellThree}, 'bbl', 0, 'MMscf', 0, 'bbl');");
        }

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                ACTIVE_IND TEXT NULL,
                CURRENT_STATUS TEXT NULL);

            CREATE TABLE WELL_STATUS (
                UWI TEXT NOT NULL,
                STATUS_ID TEXT NOT NULL,
                STATUS_TYPE TEXT NULL,
                STATUS TEXT NULL,
                STATUS_DATE TEXT NULL,
                START_TIME TEXT NULL,
                END_TIME TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (UWI, STATUS_ID));

            CREATE TABLE PDEN_WELL (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                PRIMARY_UWI TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (PDEN_ID, PRIMARY_UWI));

            CREATE TABLE PDEN_VOL_SUMMARY (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                ACTIVITY_TYPE TEXT NULL,
                AMENDMENT_SEQ_NO REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                VOLUME_DATE TEXT NULL,
                OIL_VOLUME REAL NOT NULL,
                OIL_VOLUME_OUOM TEXT NULL,
                GAS_VOLUME REAL NOT NULL,
                GAS_VOLUME_OUOM TEXT NULL,
                WATER_VOLUME REAL NOT NULL,
                WATER_VOLUME_OUOM TEXT NULL,
                PRIMARY KEY (PDEN_ID, VOLUME_DATE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO));

            CREATE TABLE PRODUCTION_DEFERMENT (
                DEFERMENT_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                START_AT TEXT NOT NULL,
                END_AT TEXT NULL,
                REASON_CODE TEXT NOT NULL,
                DEFERRED_VOLUME REAL NULL,
                VOLUME_OUOM TEXT NULL,
                COMMENT TEXT NULL,
                REPORTER TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE RECONCILIATION_VARIANCE (
                VARIANCE_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                PERIOD_MONTH TEXT NOT NULL,
                DIMENSION TEXT NOT NULL,
                STAGE TEXT NOT NULL,
                AMOUNT REAL NULL,
                VOLUME_OUOM TEXT NULL,
                MATERIAL_IND TEXT NOT NULL,
                REASON_CODE TEXT NOT NULL,
                NOTE TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE R_INTERVENTION_METHOD (
                METHOD_CODE TEXT NOT NULL PRIMARY KEY,
                METHOD_NAME TEXT NULL,
                CATEGORY TEXT NULL,
                LIFT_RELATED_IND TEXT NOT NULL,
                ACTIVE_IND TEXT NULL);

            CREATE TABLE INTERVENTION_DECISION (
                DECISION_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                WELL_ID TEXT NOT NULL,
                CHOSEN_OPTION_ID TEXT NULL,
                RATIONALE TEXT NULL,
                OWNER TEXT NULL,
                TARGET_DATE TEXT NULL,
                STATE TEXT NOT NULL,
                WORK_ORDER_REF TEXT NULL,
                WORK_ORDER_AT TEXT NULL,
                WORK_ORDER_BY TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE INTERVENTION_OPTION (
                OPTION_ID TEXT NOT NULL PRIMARY KEY,
                DECISION_ID TEXT NOT NULL,
                WELL_ID TEXT NOT NULL,
                METHOD TEXT NOT NULL,
                ESTIMATED_GAIN REAL NULL,
                GAIN_OUOM TEXT NULL,
                ESTIMATED_COST REAL NULL,
                CURRENCY TEXT NULL,
                RISK TEXT NULL,
                BASIS TEXT NOT NULL,
                BASIS_TEXT TEXT NULL,
                CHOSEN_IND TEXT NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-2', 'Well two', 'F-1', 'Y', NULL),
                ('WELL-3', 'Well three', 'F-1', 'Y', 'SUSPENDED'),
                ('WELL-9', 'Other field well', 'F-2', 'Y', 'PRODUCING');

            INSERT INTO WELL_STATUS (UWI, STATUS_ID, STATUS_TYPE, STATUS, STATUS_DATE, START_TIME, ACTIVE_IND) VALUES
                ('WELL-1', 'WS-1', 'Well Status', 'PRODUCING', '2026-08-01', '2026-08-01', 'Y'),
                ('WELL-3', 'WS-3', 'Well Status', 'SUSPENDED', '2026-09-15', '2026-09-15', 'Y');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-3', 'WELL', 'PPDM', 'WELL-3', 'Y'),
                ('PD-9', 'WELL', 'PPDM', 'WELL-9', 'Y');

            INSERT INTO PRODUCTION_DEFERMENT (DEFERMENT_ID, WELL_ID, FIELD_ID, START_AT, END_AT, REASON_CODE,
                DEFERRED_VOLUME, VOLUME_OUOM, COMMENT, REPORTER, ACTOR, RECORDED_AT) VALUES
                ('D-1', 'WELL-1', 'F-1', '2026-10-01 06:00', NULL, 'EQ-FAIL', 40, 'bbl', 'seal leak', 'sara', 'sara',
                 '2026-10-01 07:00');

            INSERT INTO RECONCILIATION_VARIANCE (VARIANCE_ID, FIELD_ID, PERIOD_MONTH, DIMENSION, STAGE, AMOUNT,
                VOLUME_OUOM, MATERIAL_IND, REASON_CODE, NOTE, ACTOR, RECORDED_AT) VALUES
                ('V-1', 'F-1', '2026-10-01', 'WELL-1', 'MEASURED', -40, 'bbl', 'Y', 'MEASUREMENT',
                 'the meter was re-calibrated', 'sara', '2026-10-02 09:00');

            INSERT INTO R_INTERVENTION_METHOD (METHOD_CODE, METHOD_NAME, CATEGORY, LIFT_RELATED_IND, ACTIVE_IND) VALUES
                ('GAS_LIFT', 'Gas lift', 'LIFT', 'Y', 'Y'),
                ('ESP', 'Electric submersible pump', 'LIFT', 'Y', 'Y'),
                ('ACIDIZE', 'Matrix acid stimulation', 'STIMULATION', 'N', 'Y'),
                ('RETIRED', 'Retired method', 'OTHER', 'N', 'N');
            """;
        command.ExecuteNonQuery();

        using var volumeCommand = connection.CreateCommand();
        volumeCommand.CommandText = volumes.ToString();
        volumeCommand.ExecuteNonQuery();
    }

    /// <summary>Blocks the update that records the work order, so the creation is not acknowledged.</summary>
    private static void ForceWorkOrderFailure(string database, string decisionId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_WORK_ORDER BEFORE UPDATE ON INTERVENTION_DECISION
             WHEN NEW.DECISION_ID = '{decisionId}' AND NEW.WORK_ORDER_REF IS NOT NULL
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropWorkOrderFailure(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_WORK_ORDER;";
        command.ExecuteNonQuery();
    }

    private static void ClearMethods(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM R_INTERVENTION_METHOD;";
        command.ExecuteNonQuery();
    }

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
        command.ExecuteNonQuery();
    }

    private static OgInterventionDecisionValues Values(
        string? well = "WELL-1",
        IReadOnlyList<OgInterventionOptionValues>? options = null,
        string? chosen = "GAS_LIFT",
        string? rationale = "the rate fell 40% and the valve test is positive") =>
        new(
            "F-1",
            well,
            options ??
            [
                Option(method: "GAS_LIFT", gain: 120m, gainUnit: "bbl", cost: 250_000m),
                Option(method: "ESP", gain: 80m, gainUnit: "bbl", cost: 400_000m, basis: InterventionRules.BasisModel),
            ],
            chosen,
            rationale,
            "sara",
            Day.AddDays(30));

    private static OgInterventionOptionValues Option(
        string? method = "GAS_LIFT",
        decimal? gain = null,
        string? gainUnit = null,
        decimal? cost = null,
        string? basis = InterventionRules.BasisTest,
        string? risk = "LOW") =>
        new(method, gain, gainUnit, cost, "USD", risk, basis, "the gas lift valve test on WELL-7");

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
