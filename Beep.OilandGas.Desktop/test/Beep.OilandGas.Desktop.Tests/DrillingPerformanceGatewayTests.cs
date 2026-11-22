using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-09 against a real SQLite database: the time-depth curve ends where the daily report headers do (AC-1); the non-productive
/// time adds up by cause (AC-2); a line with no cause is its own visible bucket (AC-3); a CSV line with an unrecognised code is
/// rejected with its line number and the rest still imported (AC-4); the offset rows come from the same analysis as the well's own
/// (AC-5); and the export is the grid's rows and the cause table (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DrillingPerformanceGatewayTests
{
    private const string ConnectionName = "eng09-performance";

    [IntegrationFact]
    public async Task The_reports_are_analysed_by_cause_and_compared_with_offsets()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng09-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng09"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "performance.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "performance.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgDrillingPerformanceService(() => source, reporter);

        // ---- AC-1 and AC-2: the curve, the totals and the causes ----
        var loaded = await service.LoadAsync("W-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = loaded.Value!;

        Assert.Equal(4, result.ReportCount);
        Assert.Equal(6, result.Rows.Count);

        // The curve ends at the deepest depth the report headers record, and the page says so.
        Assert.True(result.CurveMatchesReports);
        Assert.Equal(1500, result.ReportsDepth, 6);
        Assert.Equal(1500, result.CurveDepth!.Value, 6);
        Assert.Contains("which is the deepest depth the reports record", result.CurveText, StringComparison.Ordinal);

        // 36 reported hours, 12 of them non-productive, and one report that records no period at all.
        Assert.Equal(36, result.TotalHours, 6);
        Assert.Equal(12, result.NptHours, 6);
        Assert.Equal(1d / 3d, result.NptFraction!.Value, 6);
        Assert.Equal(1, result.IncompleteReports);
        Assert.Equal(["R-4"], result.IncompleteReportIds);
        Assert.Contains("incomplete (R-4)", result.CountsText, StringComparison.Ordinal);
        Assert.Contains("so they are partial", result.SummaryText, StringComparison.Ordinal);

        // AC-2: the causes add up to the non-productive hours, exactly.
        Assert.Equal(2, result.NptByCause.Count);
        Assert.Equal(result.NptHours, result.NptByCause.Sum(cause => cause.Hours), 6);

        var pump = result.NptByCause.Single(cause => cause.Cause == "pump failure");

        Assert.Equal(8, pump.Hours, 6);
        Assert.Equal(2d / 3d, pump.Fraction, 6);
        Assert.Equal(1, pump.Lines);

        // AC-3: the line with no cause is visible under its own name and is a third of the non-productive time.
        var unclassified = result.NptByCause.Single(cause => cause.Cause == OgPerformanceRules.Unclassified);

        Assert.Equal(4, unclassified.Hours, 6);
        Assert.Equal(1d / 3d, unclassified.Fraction, 6);
        Assert.Contains("unclassified", result.NptText, StringComparison.Ordinal);
        Assert.Contains(result.Warnings,
            warning => warning.Contains("has no recorded cause", StringComparison.Ordinal));

        // The rate of penetration is footage over drilling-code hours, and one row per drilling run.
        Assert.Equal(500d / 18d, result.OnBottomRop!.Value, 6);
        Assert.Equal(3, result.Rop.Count);
        Assert.Equal(250, result.Rop[0].Footage, 6);
        Assert.Equal(250d / 6d, result.Rop[0].Rop!.Value, 6);
        Assert.Equal(100, result.Rop[2].Footage, 6);
        Assert.Equal(100d / 8d, result.Rop[2].Rop!.Value, 6);

        // The cost share is apportioned by hours and said to be, because no NPT cost is recorded anywhere.
        Assert.Equal(240000, result.TotalCost!.Value, 6);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(80000d / 12d * 8d, pump.CostImpact, 4);
        Assert.Equal(60000d / 12d * 4d, unclassified.CostImpact, 4);
        Assert.Contains("apportioned from each report's cost by its hours", result.CostText, StringComparison.Ordinal);

        // ---- AC-6: the export is the grid and the cause table ----
        var rows = result.RowsCsv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("line,report,start,end,code,name,hours,from,to,npt,cause,description", rows[0]);
        Assert.Equal(1 + result.Rows.Count, rows.Length);
        Assert.Contains("\"R-1\"", rows[1], StringComparison.Ordinal);
        Assert.Contains("\"2\"", rows[1], StringComparison.Ordinal);
        Assert.Contains(",1000,1250,N,", rows[1], StringComparison.Ordinal);

        var causes = result.CausesCsv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("cause,hours,fraction,lines,cost_impact,currency", causes[0]);
        Assert.Equal(1 + result.NptByCause.Count, causes.Length);
        Assert.Contains("\"pump failure\"", causes[1] + causes[2], StringComparison.Ordinal);
        Assert.Contains("\"unclassified\"", causes[1] + causes[2], StringComparison.Ordinal);

        // ---- AC-5: the offsets are the same analysis, so the numbers are the same numbers ----
        var offsets = await service.OffsetsAsync("W-1", ["W-2"]);

        Assert.True(offsets.Success, offsets.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, offsets.Value!.Count);

        var mine = offsets.Value[0];

        Assert.Equal("W-1", mine.WellId);
        Assert.Equal(result.Days, mine.Days, 6);
        Assert.Equal(result.Depth, mine.Depth, 6);
        Assert.Equal(result.NptHours, mine.NptHours, 6);
        Assert.Equal(result.NptFraction!.Value * 100d, mine.NptPercent, 6);
        Assert.Equal(result.OnBottomRop!.Value, mine.Rop!.Value, 6);
        Assert.True(mine.HasReports);

        // The offset well really is different, which is what makes the comparison meaningful rather than a copy.
        var offset = offsets.Value[1];

        Assert.Equal("W-2", offset.WellId);
        Assert.Equal(1, offset.Reports);
        Assert.Equal(0d, offset.NptPercent, 6);
        Assert.Equal(25, offset.Rop!.Value, 6);
        Assert.Equal(800, offset.Depth, 6);

        // A well with no line is listed and says so rather than being silently dropped.
        var withEmpty = await service.OffsetsAsync("W-1", ["W-3"]);

        Assert.True(withEmpty.Success, withEmpty.Message);
        Assert.False(withEmpty.Value![1].HasReports);
        Assert.Contains("no activity line is recorded", withEmpty.Value[1].Describe(), StringComparison.Ordinal);

        // ---- AC-4: an unrecognised code is rejected with its line number, and the rest is imported ----
        var csv =
            "start,end,code,depth_from,depth_to,description,npt\n" +
            "2026-10-01T06:00:00Z,2026-10-01T12:00:00Z,2,100,250,drilled,false\n" +
            "2026-10-01T12:00:00Z,2026-10-01T18:00:00Z,6,250,250,trip,false\n" +
            "2026-10-01T18:00:00Z,2026-10-02T02:00:00Z,99,250,250,unknown code,true\n";

        var imported = await service.ImportCsvAsync("W-9", csv, "sara");

        Assert.True(imported.Success, imported.Message + " :: " + string.Join(" | ", reporter.Failures));

        var rejection = Assert.Single(imported.Value!.Rejected);

        Assert.Equal(4, rejection.Line);
        Assert.Contains("'99' is not an operation code", rejection.Reason, StringComparison.Ordinal);
        Assert.StartsWith("line 4:", rejection.Describe(), StringComparison.Ordinal);

        Assert.Equal(2, imported.Value.Accepted);
        Assert.Equal(0, imported.Value.Attached);
        Assert.Equal(2, imported.Value.Unattached);
        Assert.Contains("2 line(s) imported", imported.Value.Describe(), StringComparison.Ordinal);

        Assert.Equal(2, Count(database, "DEVELOPMENT_REPORT_ACTIVITY WHERE WELL_ID = 'W-9'"));
        Assert.Equal(1, Count(database,
            "DEVELOPMENT_REPORT_ACTIVITY WHERE WELL_ID = 'W-9' AND REPORT_ID = 'UNATTACHED' AND CODE = '2' AND ACTOR = 'sara'"));

        // A CSV of nothing but bad codes is refused outright rather than written as an empty import.
        var allBad = await service.ImportCsvAsync(
            "W-9", "start,end,code\n2026-10-01T06:00:00Z,2026-10-01T07:00:00Z,99\n", "sara");

        Assert.False(allBad.Success);
        Assert.Equal(OgPerformanceRules.CodeNoLines, allBad.Reference);
        Assert.Equal(2, Count(database, "DEVELOPMENT_REPORT_ACTIVITY WHERE WELL_ID = 'W-9'"));

        // ---- the empty state says what to do rather than showing zeros as a result ----
        var empty = await service.LoadAsync("W-3");

        Assert.True(empty.Success, empty.Message);
        Assert.True(empty.Value!.Empty);
        Assert.Empty(empty.Value.Rows);
        Assert.Contains("Record the daily reports (DEV-06)", empty.Value.SummaryText, StringComparison.Ordinal);

        // ---- a failed read is a failure, not an empty analysis ----
        Execute(database, "DROP TABLE DEVELOPMENT_REPORT_ACTIVITY;");

        var failed = await service.LoadAsync("W-1");

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) => Execute(database, """
            CREATE TABLE DEVELOPMENT_DAILY_REPORT (
                REPORT_ID TEXT,
                EXECUTION_ID TEXT,
                PROGRAM_ID TEXT,
                WELL_ID TEXT,
                REPORT_DATE TEXT,
                SHIFT TEXT,
                REVISION INTEGER,
                CORRECTS_REPORT_ID TEXT,
                CORRECTION_REASON TEXT,
                PERIOD_START TEXT,
                PERIOD_END TEXT,
                DEPTH_START REAL,
                DEPTH_END REAL,
                DEPTH_OUOM TEXT,
                COST REAL,
                CURRENCY TEXT,
                HSE_EVENTS TEXT,
                PERSONNEL TEXT,
                EQUIPMENT TEXT,
                STATE TEXT,
                AUTHOR TEXT,
                SUBMITTED_BY TEXT,
                SUBMITTED_AT TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (REPORT_ID));

            CREATE TABLE DEVELOPMENT_REPORT_ACTIVITY (
                ACTIVITY_ID TEXT,
                REPORT_ID TEXT,
                WELL_ID TEXT,
                START_UTC TEXT,
                END_UTC TEXT,
                CODE TEXT,
                DEPTH_FROM REAL,
                DEPTH_TO REAL,
                DEPTH_OUOM TEXT,
                DESCRIPTION TEXT,
                NPT TEXT,
                NPT_CAUSE TEXT,
                ACTOR TEXT,
                RECORDED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ACTIVITY_ID));

            INSERT INTO DEVELOPMENT_DAILY_REPORT (REPORT_ID, EXECUTION_ID, PROGRAM_ID, WELL_ID, REPORT_DATE,
                PERIOD_START, PERIOD_END, DEPTH_START, DEPTH_END, DEPTH_OUOM, COST, CURRENCY, STATE, ACTIVE_IND) VALUES
                ('R-1', 'E-1', 'P-1', 'W-1', '2026-10-01', '2026-10-01 06:00:00', '2026-10-01 18:00:00', 1000, 1250, 'm', 100000, 'USD', 'SUBMITTED', 'Y'),
                ('R-2', 'E-1', 'P-1', 'W-1', '2026-10-02', '2026-10-01 18:00:00', '2026-10-02 06:00:00', 1250, 1400, 'm', 80000, 'USD', 'SUBMITTED', 'Y'),
                ('R-3', 'E-1', 'P-1', 'W-1', '2026-10-03', '2026-10-02 06:00:00', '2026-10-02 18:00:00', 1400, 1500, 'm', 60000, 'USD', 'SUBMITTED', 'Y'),
                ('R-4', 'E-1', 'P-1', 'W-1', '2026-10-04', NULL, NULL, 1500, 1500, 'm', 0, 'USD', 'DRAFT', 'Y'),
                ('R-9', 'E-2', 'P-2', 'W-2', '2026-10-05', '2026-10-05 00:00:00', '2026-10-06 00:00:00', 500, 800, 'm', 50000, 'USD', 'SUBMITTED', 'Y');

            INSERT INTO DEVELOPMENT_REPORT_ACTIVITY (ACTIVITY_ID, REPORT_ID, WELL_ID, START_UTC, END_UTC, CODE,
                DEPTH_FROM, DEPTH_TO, DEPTH_OUOM, DESCRIPTION, NPT, NPT_CAUSE, ACTOR, RECORDED_AT_UTC, ACTIVE_IND) VALUES
                ('A-1', 'R-1', 'W-1', '2026-10-01 06:00:00', '2026-10-01 12:00:00', '2', 1000, 1250, 'm', 'drilled 8.5in hole', 'N', NULL, 'sara', '2026-10-02 00:00:00', 'Y'),
                ('A-2', 'R-1', 'W-1', '2026-10-01 12:00:00', '2026-10-01 18:00:00', '6', 1250, 1250, 'm', 'trip for bit', 'N', NULL, 'sara', '2026-10-02 00:00:00', 'Y'),
                ('A-3', 'R-2', 'W-1', '2026-10-01 18:00:00', '2026-10-02 02:00:00', '7', 1250, 1250, 'm', 'pump failed', 'Y', 'pump failure', 'sara', '2026-10-03 00:00:00', 'Y'),
                ('A-4', 'R-2', 'W-1', '2026-10-02 02:00:00', '2026-10-02 06:00:00', '2', 1250, 1400, 'm', 'drilled', 'N', NULL, 'sara', '2026-10-03 00:00:00', 'Y'),
                ('A-5', 'R-3', 'W-1', '2026-10-02 06:00:00', '2026-10-02 10:00:00', '7', 1400, 1400, 'm', 'rig repair', 'Y', NULL, 'sara', '2026-10-04 00:00:00', 'Y'),
                ('A-6', 'R-3', 'W-1', '2026-10-02 10:00:00', '2026-10-02 18:00:00', '2', 1400, 1500, 'm', 'drilled', 'N', NULL, 'sara', '2026-10-04 00:00:00', 'Y'),
                ('A-9', 'R-9', 'W-2', '2026-10-05 00:00:00', '2026-10-05 12:00:00', '2', 500, 800, 'm', 'drilled', 'N', NULL, 'omar', '2026-10-06 00:00:00', 'Y'),
                ('A-10', 'R-9', 'W-2', '2026-10-05 12:00:00', '2026-10-06 00:00:00', '6', 800, 800, 'm', 'trip', 'N', NULL, 'omar', '2026-10-06 00:00:00', 'Y');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
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
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
