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
/// PRD-05 against a real SQLite database: the waterfall walks produced → measured → allocated → sold → booked with each
/// step's difference (AC-1); a material variance blocks the report until it is reasoned (AC-2); a stage in another unit
/// refuses the comparison (AC-3); a closed month refuses a reason (AC-4); and the month total equals the sum of the day
/// rows (AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ReconciliationGatewayTests
{
    private const string ConnectionName = "prd05-reconciliation";
    private static readonly DateTime Month = new(2026, 10, 1);

    [IntegrationFact]
    public async Task The_waterfall_adds_up_a_reason_unblocks_the_report_and_a_closed_month_is_read_only()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "reconciliation.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "reconciliation.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgReconciliationService(() => source, reporter);

        var loaded = await service.LoadAsync("F-1", "Alpha field", Month);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal("2026-10", workspace.PeriodLabel);
        Assert.False(workspace.IsClosed);
        Assert.Equal(2, workspace.Lines.Count);                    // the F-2 well is absent
        Assert.DoesNotContain(workspace.Lines, line => line.Dimension == "WELL-9");

        // ---- AC-1: the waterfall and its differences ------------------------
        var field = workspace.FieldLine!;

        Assert.Equal(1300m, field.Produced);                        // 600 + 400 + 300
        Assert.Equal(900m, field.Measured);                         // the committed run's measured total
        Assert.Equal(850m, field.Allocated);                        // … and its allocated total
        Assert.Equal(600m, field.Sold);                             // the run ticket
        Assert.Equal(850m, field.Booked);                           // the accounting allocation

        Assert.True(workspace.IsConsistent);
        Assert.Contains("produced 1,300 bbl", workspace.WaterfallText, StringComparison.Ordinal);
        Assert.Contains("booked 850 bbl", workspace.WaterfallText, StringComparison.Ordinal);

        var steps = field.Steps;

        Assert.Equal(-400m, steps[1].Difference);                   // measured − produced
        Assert.Equal(-50m, steps[2].Difference);
        Assert.Equal(-250m, steps[3].Difference);
        Assert.Equal(250m, steps[4].Difference);

        // ---- AC-2: the material variances block the report ----------------
        // WELL-1 moved 40% between produced and measured; WELL-2 moved 6.7% between measured and allocated.
        Assert.Equal(2, workspace.MaterialCount);
        Assert.Equal(2, workspace.NeedsReasonCount);
        Assert.False(workspace.ExportReady);
        Assert.Empty(workspace.UnitMismatch);

        var blocked = await service.BuildReportAsync("F-1", "Alpha field", Month);

        Assert.False(blocked.Success);
        Assert.Equal(ReconciliationRules.CodeMaterialVarianceWithoutReason, blocked.Reference);
        Assert.Contains("no reason", blocked.Message!, StringComparison.Ordinal);

        var noCode = await service.AssignReasonAsync(
            new OgReconciliationReasonValues("F-1", Month, "WELL-1", ReconciliationRules.Measured, null, null), "sara");

        Assert.False(noCode.Success);
        Assert.Equal(ReconciliationRules.CodeReasonRequired, noCode.Reference);

        var unknownCode = await service.AssignReasonAsync(
            new OgReconciliationReasonValues("F-1", Month, "WELL-1", ReconciliationRules.Measured, "MADE-UP", null),
            "sara");

        Assert.False(unknownCode.Success);
        Assert.Equal(ReconciliationRules.CodeReasonUnknown, unknownCode.Reference);

        var firstReason = await service.AssignReasonAsync(
            new OgReconciliationReasonValues(
                "F-1", Month, "WELL-1", ReconciliationRules.Measured, "MEASUREMENT", "the separator was re-calibrated"),
            "sara");

        Assert.True(firstReason.Success, firstReason.Message);
        Assert.Equal(1, firstReason.Value!.NeedsReasonCount);       // WELL-2 still has none

        var assigned = await service.AssignReasonAsync(
            new OgReconciliationReasonValues(
                "F-1", Month, "WELL-2", ReconciliationRules.Allocated, "TIMING", "the ticket was cut on the 2nd"),
            "sara");

        Assert.True(assigned.Success, assigned.Message);

        var reasoned = assigned.Value!;

        Assert.True(reasoned.ExportReady);
        Assert.Equal(0, reasoned.NeedsReasonCount);
        Assert.Contains("MEASUREMENT", reasoned.Lines.Single(line => line.Dimension == "WELL-1").ReasonText,
            StringComparison.Ordinal);
        Assert.Contains("TIMING", reasoned.Lines.Single(line => line.Dimension == "WELL-2").ReasonText,
            StringComparison.Ordinal);

        // The reason is stored once and is updated, not duplicated, when it is assigned again.
        var reassigned = await service.AssignReasonAsync(
            new OgReconciliationReasonValues(
                "F-1", Month, "WELL-1", ReconciliationRules.Measured, "TIMING", "the meter was read late"),
            "sara");

        Assert.True(reassigned.Success, reassigned.Message);
        Assert.Equal(2, CountRows(database, "RECONCILIATION_VARIANCE"));
        Assert.Contains("TIMING", reassigned.Value!.Lines.Single(line => line.Dimension == "WELL-1").ReasonText,
            StringComparison.Ordinal);

        // ---- AC-2: the report and its CSV ---------------------------------
        var report = await service.BuildReportAsync("F-1", "Alpha field", Month);

        Assert.True(report.Success, report.Message);
        Assert.Contains("Volume reconciliation", report.Value!.Text, StringComparison.Ordinal);
        Assert.Contains("monthly totals equal the sum of the daily rows", report.Value.Text,
            StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("dimension,name,produced", report.Value.Csv, StringComparison.Ordinal);
        Assert.Contains("\"WELL-1\"", report.Value.Csv, StringComparison.Ordinal);

        // ---- AC-5: the month total is the sum of the day rows -------------
        var days = reasoned.Days.Where(day => day.HasData).ToList();

        Assert.Equal(3, days.Count);                                // 1st, 2nd, 3rd
        Assert.Equal(reasoned.Monthly.Produced, days.Sum(day => day.Produced ?? 0m));
        Assert.Equal(reasoned.Monthly.Measured, days.Sum(day => day.Measured ?? 0m));
        Assert.Equal(reasoned.Monthly.Allocated, days.Sum(day => day.Allocated ?? 0m));
        Assert.Equal(reasoned.Monthly.Sold, days.Sum(day => day.Sold ?? 0m));
        Assert.Equal(reasoned.Monthly.Booked, days.Sum(day => day.Booked ?? 0m));
        Assert.Equal(900m, days.Single(day => day.Date == Month).Produced);   // 600 + 300 on the first
        Assert.Equal(600m, days.Single(day => day.Date == Month).Sold);

        // ---- AC-4: a closed month is read-only ---------------------------
        var closed = await service.ClosePeriodAsync(Month, "F-1", "sara", "Alpha field");

        Assert.True(closed.Success, closed.Message);
        Assert.True(closed.Value!.IsClosed);
        Assert.Equal("sara", closed.Value.ClosedBy);
        Assert.NotNull(closed.Value.ClosedAt);

        var afterClose = await service.AssignReasonAsync(
            new OgReconciliationReasonValues("F-1", Month, "WELL-1", ReconciliationRules.Measured, "MEASUREMENT", null),
            "sara");

        Assert.False(afterClose.Success);
        Assert.Equal(ReconciliationRules.CodePeriodClosed, afterClose.Reference);

        var closeTwice = await service.ClosePeriodAsync(Month, "F-1", "sara");

        Assert.False(closeTwice.Success);
        Assert.Equal(ReconciliationRules.CodePeriodClosed, closeTwice.Reference);

        // The report is still readable in a closed month; the month is read-only, not invisible.
        Assert.True((await service.BuildReportAsync("F-1", "Alpha field", Month)).Success);

        // ---- AC-3: a stage in another unit refuses the comparison ---------
        SetTicketUnit(database, "m3");

        var mixed = await service.LoadAsync("F-1", "Alpha field", Month);

        Assert.True(mixed.Success, mixed.Message);
        Assert.Contains("more than one unit", mixed.Value!.UnitMismatch, StringComparison.Ordinal);
        Assert.Contains("m3", mixed.Value.UnitMismatch, StringComparison.Ordinal);
        Assert.False(mixed.Value.ExportReady);

        var refused = await service.BuildReportAsync("F-1", "Alpha field", Month);

        Assert.False(refused.Success);
        Assert.Equal(ReconciliationRules.CodeUnitMismatch, refused.Reference);

        // ---- a failed read is a failure ----------------------------------
        Drop(database, "RECONCILIATION_PERIOD");

        var failed = await service.LoadAsync("F-1", "Alpha field", Month);

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
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                ACTIVE_IND TEXT NULL,
                CURRENT_STATUS TEXT NULL);

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

            CREATE TABLE PDEN_VOL_DISPOSITION (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                ACTIVITY_TYPE TEXT NULL,
                PRODUCT_TYPE TEXT NULL,
                DISPOSITION_OBS_NO REAL NOT NULL,
                AMENDMENT_SEQ_NO REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                VOLUME REAL NOT NULL,
                VOLUME_OUOM TEXT NULL,
                VOLUME_UOM TEXT NULL,
                VOLUME_START_DATE TEXT NULL,
                VOLUME_END_DATE TEXT NULL,
                PRIMARY KEY (PDEN_ID, VOLUME_START_DATE, ACTIVITY_TYPE, DISPOSITION_OBS_NO));

            CREATE TABLE PRODUCTION_ALLOCATION (
                PRODUCTION_ALLOCATION_ID TEXT NOT NULL PRIMARY KEY,
                PDEN_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                WELL_ID TEXT NULL,
                POOL_ID TEXT NULL,
                ALLOCATION_DATE TEXT NULL,
                TOTAL_PRODUCTION REAL NOT NULL,
                ALLOCATION_METHOD TEXT NULL,
                ALLOCATED_VOLUME REAL NULL,
                ALLOCATED_VOLUME_OUOM TEXT NULL,
                ALLOCATION_STATUS TEXT NULL);

            CREATE TABLE ALLOCATION_RUN (
                ALLOCATION_RUN_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FACILITY_ID TEXT NULL,
                PERIOD_DATE TEXT NOT NULL,
                METHOD TEXT NOT NULL,
                STATE TEXT NOT NULL,
                TOTAL_MEASURED REAL NULL,
                TOTAL_ALLOCATED REAL NULL,
                RESIDUAL REAL NULL,
                VOLUME_OUOM TEXT NULL,
                TOLERANCE_PERCENT REAL NOT NULL,
                ACKNOWLEDGED_IND TEXT NOT NULL,
                INPUT_HASH TEXT NOT NULL,
                MEASURED_SOURCE TEXT NULL,
                COMMITTED_BY TEXT NULL,
                COMMITTED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE ALLOCATION_RUN_ROW (
                ALLOCATION_RUN_ROW_ID TEXT NOT NULL PRIMARY KEY,
                ALLOCATION_RUN_ID TEXT NOT NULL,
                WELL_ID TEXT NOT NULL,
                WELL_NAME TEXT NULL,
                MEASURED_VOLUME REAL NULL,
                ALLOCATED_VOLUME REAL NULL,
                VOLUME_OUOM TEXT NULL,
                FACTOR REAL NULL,
                RESIDUAL REAL NULL,
                METHOD TEXT NOT NULL,
                STATUS TEXT NOT NULL,
                BASIS_TEXT TEXT NULL,
                OVERRIDE_REASON TEXT NULL,
                OVERRIDE_BY TEXT NULL);

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

            CREATE TABLE RECONCILIATION_PERIOD (
                PERIOD_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                PERIOD_MONTH TEXT NOT NULL,
                STATE TEXT NOT NULL,
                CLOSED_BY TEXT NULL,
                CLOSED_AT TEXT NULL,
                NOTE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE R_RECONCILIATION_REASON (
                REASON_CODE TEXT NOT NULL PRIMARY KEY,
                REASON_NAME TEXT NULL,
                CATEGORY TEXT NULL,
                ACTIVE_IND TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-2', 'Well two', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-9', 'Other field well', 'F-2', 'Y', 'PRODUCING');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'WELL-2', 'Y'),
                ('PD-9', 'WELL', 'PPDM', 'WELL-9', 'Y');

            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME,
                WATER_VOLUME_OUOM) VALUES
                ('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 600, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-02', 400, 'bbl', 4, 'MMscf', 15, 'bbl'),
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 300, 'bbl', 0, 'MMscf', 10, 'bbl'),
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-03', 0, 'bbl', 0, 'MMscf', 0, 'bbl'),
                ('PD-9', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 9000, 'bbl', 90, 'MMscf', 0, 'bbl');

            INSERT INTO ALLOCATION_RUN (ALLOCATION_RUN_ID, FIELD_ID, FACILITY_ID, PERIOD_DATE, METHOD, STATE,
                TOTAL_MEASURED, TOTAL_ALLOCATED, RESIDUAL, VOLUME_OUOM, TOLERANCE_PERCENT, ACKNOWLEDGED_IND, INPUT_HASH,
                MEASURED_SOURCE, COMMITTED_BY, COMMITTED_AT) VALUES
                ('AR-1', 'F-1', NULL, '2026-10-01', 'MEASURED_SHARE', 'COMMITTED', 900, 850, -50, 'bbl', 0.5, 'N',
                 'hash-1', 'the field''s own well measurements', 'sara', '2026-10-01 12:00');

            INSERT INTO ALLOCATION_RUN_ROW (ALLOCATION_RUN_ROW_ID, ALLOCATION_RUN_ID, WELL_ID, WELL_NAME,
                MEASURED_VOLUME, ALLOCATED_VOLUME, VOLUME_OUOM, FACTOR, RESIDUAL, METHOD, STATUS, BASIS_TEXT) VALUES
                ('ARR-1', 'AR-1', 'WELL-1', 'Well one', 600, 570, 'bbl', 0.6667, -30, 'MEASURED_SHARE', 'ALLOCATED', 'measured share'),
                ('ARR-2', 'AR-1', 'WELL-2', 'Well two', 300, 280, 'bbl', 0.3333, -20, 'MEASURED_SHARE', 'ALLOCATED', 'measured share');

            INSERT INTO PDEN_VOL_DISPOSITION (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, PRODUCT_TYPE,
                DISPOSITION_OBS_NO, AMENDMENT_SEQ_NO, ACTIVE_IND, VOLUME, VOLUME_OUOM, VOLUME_START_DATE) VALUES
                ('PD-1', 'WELL', 'PPDM', 'SALES', 'OIL', 1, 0, 'Y', 600, 'bbl', '2026-10-01'),
                ('PD-1', 'WELL', 'PPDM', 'SALES', 'OIL', 2, 0, 'Y', 0, 'bbl', '2026-10-02'),
                ('PD-9', 'WELL', 'PPDM', 'SALES', 'OIL', 3, 0, 'Y', 9000, 'bbl', '2026-10-01');

            INSERT INTO PRODUCTION_ALLOCATION (PRODUCTION_ALLOCATION_ID, PDEN_ID, FIELD_ID, WELL_ID, POOL_ID,
                ALLOCATION_DATE, TOTAL_PRODUCTION, ALLOCATION_METHOD, ALLOCATED_VOLUME, ALLOCATED_VOLUME_OUOM,
                ALLOCATION_STATUS) VALUES
                ('PA-1', 'PD-1', 'F-1', 'WELL-1', 'P-1', '2026-10-01', 600, 'MEASURED', 570, 'bbl', 'POSTED'),
                ('PA-2', 'PD-2', 'F-1', 'WELL-2', 'P-1', '2026-10-01', 300, 'MEASURED', 280, 'bbl', 'POSTED');

            INSERT INTO R_RECONCILIATION_REASON (REASON_CODE, REASON_NAME, CATEGORY, ACTIVE_IND) VALUES
                ('MEASUREMENT', 'Meter drift', 'MEASUREMENT', 'Y'),
                ('TIMING', 'Ticket cut next day', 'TIMING', 'Y'),
                ('RETIRED', 'Retired category', 'OTHER', 'N');
            """;
        command.ExecuteNonQuery();
    }

    private static long CountRows(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {table};";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void SetTicketUnit(string database, string unit)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE PDEN_VOL_DISPOSITION SET VOLUME_OUOM = $unit;";
        command.Parameters.AddWithValue("$unit", unit);
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
