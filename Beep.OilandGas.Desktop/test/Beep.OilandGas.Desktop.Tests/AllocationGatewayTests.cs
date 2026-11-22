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
/// PRD-04 against a real SQLite database: a preview writes nothing (AC-5); every row carries its method and residual
/// (AC-1); a residual beyond tolerance blocks the commit until it is acknowledged (AC-2); an override needs a reason and
/// is recorded (AC-3); the same run committed twice is posted once (AC-4); and the committed run stores the hash of its
/// inputs (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class AllocationGatewayTests
{
    private const string ConnectionName = "prd04-allocation";
    private static readonly DateTime Day = new(2026, 10, 1);

    [IntegrationFact]
    public async Task A_preview_writes_nothing_and_the_commit_is_flagged_acknowledged_and_posted_once()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "allocation.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "allocation.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgAllocationWorkbenchService(() => source, reporter);

        // ---- the preview (AC-5) ----------------------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field", null, Day, AllocationRules.MeasuredShare);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var preview = loaded.Value!;

        Assert.NotNull(preview.Run);
        Assert.False(preview.Run!.IsCommitted);
        Assert.Equal(3, preview.Run.RowCount);                       // the F-2 well is not in the run
        Assert.DoesNotContain(preview.Run.Rows, row => row.WellId == "WELL-9");

        // The field's own wells measured 900 bbl, so the run allocates exactly that.
        Assert.Equal(900m, preview.Run.TotalMeasured);
        Assert.Equal(900m, preview.Run.TotalAllocated);
        Assert.Equal(0m, preview.Run.Residual);
        Assert.False(preview.Run.IsBeyondTolerance);
        Assert.Equal("bbl", preview.Run.Unit);

        // AC-1: every row states its method and its residual; the third well has no measurement.
        Assert.All(preview.Run.Rows, row => Assert.False(string.IsNullOrWhiteSpace(row.Method)));

        var well1 = preview.Run.Rows.Single(row => row.WellId == "WELL-1");

        Assert.Equal("600 bbl", well1.MeasuredText);
        Assert.Equal("600 bbl", well1.AllocatedText);
        Assert.Equal("0 bbl", well1.ResidualText);
        Assert.Equal("MEASURED_SHARE", well1.Method);
        Assert.Equal("allocated", well1.StatusText);

        var noMeasurement = preview.Run.Rows.Single(row => row.WellId == "WELL-3");

        Assert.True(noMeasurement.IsUnallocated);
        Assert.Equal("unallocated", noMeasurement.AllocatedText);
        Assert.Equal("no factor", noMeasurement.FactorText);
        Assert.Contains("1 row(s) are unallocated", preview.Run.FlagsText, StringComparison.Ordinal);

        // AC-5: nothing was written by the load or by a second preview.
        Assert.Equal(0, CountRows(database, "ALLOCATION_RUN"));
        Assert.Equal(0, CountRows(database, "ALLOCATION_RUN_ROW"));

        var secondPreview = await service.PreviewAsync(
            new OgAllocationInputs("F-1", null, Day, AllocationRules.MeasuredShare, 0.5m, []),
            "Alpha field");

        Assert.True(secondPreview.Success, secondPreview.Message);
        Assert.Equal(0, CountRows(database, "ALLOCATION_RUN"));

        // AC-6: the same inputs produce the same hash, and a different period does not.
        Assert.Equal(preview.Run.Hash, secondPreview.Value!.Run!.Hash);
        Assert.Equal(
            preview.Run.Hash,
            AllocationRules.Hash(
                new OgAllocationInputs("F-1", null, Day, AllocationRules.MeasuredShare, 0.5m, []),
                preview.Run.Rows));

        // ---- AC-3: an override needs its reason -------------------------------
        var noReason = await service.PreviewAsync(
            new OgAllocationInputs("F-1", null, Day, AllocationRules.MeasuredShare, 0.5m,
                [new OgAllocationOverride("WELL-1", 500m, null)]),
            "Alpha field");

        Assert.False(noReason.Success);
        Assert.Equal(AllocationRules.CodeOverrideReasonRequired, noReason.Reference);

        var overridden = await service.PreviewAsync(
            new OgAllocationInputs("F-1", null, Day, AllocationRules.MeasuredShare, 0.5m,
                [new OgAllocationOverride("WELL-1", 500m, "the separator was re-calibrated")]),
            "Alpha field");

        Assert.True(overridden.Success, overridden.Message);

        var overrideRun = overridden.Value!.Run!;
        var overriddenRow = overrideRun.Rows.Single(row => row.WellId == "WELL-1");

        Assert.True(overriddenRow.IsOverridden);
        Assert.Equal("500 bbl", overriddenRow.AllocatedText);
        Assert.Equal("the separator was re-calibrated", overriddenRow.OverrideReason);
        Assert.Contains("the separator was re-calibrated", overrideRun.BasisText, StringComparison.Ordinal);
        Assert.Equal(800m, overrideRun.TotalAllocated);
        Assert.Equal(-100m, overrideRun.Residual);
        Assert.True(overrideRun.IsBeyondTolerance);

        // ---- the facility's own measurement can be the run's total ----
        var facility = await service.LoadAsync("F-1", "Alpha field", "FAC-1", Day, AllocationRules.MeasuredShare);

        Assert.True(facility.Success, facility.Message);

        var facilityRun = facility.Value!.Run!;

        Assert.Equal(950m, facilityRun.TotalMeasured);                 // the facility measured 950 bbl
        Assert.Equal(950m, facilityRun.TotalAllocated);                // the factors share out exactly that
        Assert.Equal(0m, facilityRun.Residual);
        Assert.Contains("FAC-1", facilityRun.MeasuredSource, StringComparison.Ordinal);
        Assert.False(facilityRun.IsBeyondTolerance);

        // ---- AC-2: an override moves the allocated total, so the run must be acknowledged ----
        var overrideInputs = new OgAllocationInputs(
            "F-1", "FAC-1", Day, AllocationRules.MeasuredShare, 0.5m,
            [new OgAllocationOverride("WELL-1", 500m, "the separator was re-calibrated")]);

        var refused = await service.CommitAsync(
            overrideInputs,
            acknowledgeResidual: false,
            actor: "sara",
            fieldName: "Alpha field");

        Assert.False(refused.Success);
        Assert.Equal(AllocationRules.CodeResidualBeyondTolerance, refused.Reference);
        Assert.Contains("beyond the tolerance", refused.Message!, StringComparison.Ordinal);
        Assert.Equal(0, CountRows(database, "ALLOCATION_RUN"));

        var committed = await service.CommitAsync(
            overrideInputs,
            acknowledgeResidual: true,
            actor: "sara",
            fieldName: "Alpha field");

        Assert.True(committed.Success, committed.Message);

        var run = committed.Value!.Run!;

        Assert.True(run.IsCommitted);
        Assert.Equal("sara", run.CommittedBy);
        Assert.NotNull(run.CommittedAt);
        Assert.True(run.Acknowledged);
        Assert.Equal(64, run.Hash.Length);
        Assert.Single(committed.Value.History);
        Assert.Contains(run.Hash[..12], committed.Value.HistoryText, StringComparison.Ordinal);

        // The stored rows carry the method, the residual and the state (AC-1, AC-6).
        Assert.Equal(1, CountRows(database, "ALLOCATION_RUN"));
        Assert.Equal(3, CountRows(database, "ALLOCATION_RUN_ROW"));

        var stored = ReadStrings(database,
            "SELECT METHOD, INPUT_HASH, STATE, ACKNOWLEDGED_IND FROM ALLOCATION_RUN;");

        Assert.Equal("MEASURED_SHARE", stored[0]);
        Assert.Equal(run.Hash, stored[1]);
        Assert.Equal("COMMITTED", stored[2]);
        Assert.Equal("Y", stored[3]);

        var storedRows = ReadStrings(database,
            "SELECT METHOD, STATUS, IFNULL(ALLOCATED_VOLUME, -1) FROM ALLOCATION_RUN_ROW ORDER BY WELL_ID;");

        Assert.Contains("OVERRIDDEN", storedRows);
        Assert.Contains("UNALLOCATED", storedRows);
        Assert.Contains("the separator was re-calibrated",
            ReadStrings(database, "SELECT OVERRIDE_REASON FROM ALLOCATION_RUN_ROW WHERE WELL_ID = 'WELL-1';"));

        // ---- AC-4: committing the same run again posts nothing new ------------
        var again = await service.CommitAsync(
            overrideInputs,
            acknowledgeResidual: true,
            actor: "sara",
            fieldName: "Alpha field");

        Assert.True(again.Success, again.Message);
        Assert.Equal(run.Id, again.Value!.Run!.Id);
        Assert.Equal(1, CountRows(database, "ALLOCATION_RUN"));
        Assert.Equal(3, CountRows(database, "ALLOCATION_RUN_ROW"));

        // ---- the history reads back the committed run -------------------------
        var history = await service.LoadAsync("F-1", "Alpha field", null, Day, AllocationRules.MeasuredShare);

        Assert.Single(history.Value!.History);
        Assert.Equal(run.Hash, history.Value.History[0].Hash);
        Assert.Equal(950m, history.Value.History[0].TotalMeasured);

        // ---- a failed read is a failure --------------------------------------
        Drop(database, "ALLOCATION_RUN_ROW");

        var failed = await service.LoadAsync("F-1", "Alpha field", null, Day, AllocationRules.MeasuredShare);

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

            CREATE TABLE FACILITY_MEASUREMENT (
                MEASUREMENT_ID TEXT NOT NULL PRIMARY KEY,
                FACILITY_ID TEXT NOT NULL,
                FACILITY_TYPE TEXT NULL,
                EQUIPMENT_ID TEXT NULL,
                MEASUREMENT_TYPE TEXT NULL,
                MEASURED_VALUE REAL NOT NULL,
                MEASURED_UOM TEXT NULL,
                MEASURED_DATE TEXT NULL,
                QUALITY_CODE TEXT NULL,
                SOURCE_SYSTEM TEXT NULL,
                DETAILS_JSON TEXT NULL,
                ACTIVE_IND TEXT NULL);

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
                OVERRIDE_BY TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-2', 'Well two', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-3', 'Well three', 'F-1', 'Y', 'SUSPENDED'),
                ('WELL-9', 'Other field well', 'F-2', 'Y', 'PRODUCING');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'WELL-2', 'Y'),
                ('PD-9', 'WELL', 'PPDM', 'WELL-9', 'Y');

            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME,
                WATER_VOLUME_OUOM) VALUES
                ('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 600, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 300, 'bbl', 0, 'MMscf', 10, 'bbl'),
                ('PD-9', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 9000, 'bbl', 90, 'MMscf', 0, 'bbl');

            INSERT INTO FACILITY_MEASUREMENT (MEASUREMENT_ID, FACILITY_ID, MEASUREMENT_TYPE, MEASURED_VALUE,
                MEASURED_UOM, MEASURED_DATE, ACTIVE_IND) VALUES
                ('M-1', 'FAC-1', 'SALES', 950, 'bbl', '2026-10-01', 'Y');
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

    private static List<string> ReadStrings(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var values = new List<string>();

        while (reader.Read())
        {
            for (var index = 0; index < reader.FieldCount; index++)
            {
                values.Add(reader.IsDBNull(index) ? string.Empty : reader.GetValue(index).ToString() ?? string.Empty);
            }
        }

        return values;
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
