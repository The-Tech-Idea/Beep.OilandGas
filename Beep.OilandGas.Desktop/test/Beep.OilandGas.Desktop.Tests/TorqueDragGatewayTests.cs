using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-07 against a real SQLite database: the run follows the saved plan's trajectory and a vertical string's hook load is the
/// buoyed weight, which is the reference case (AC-1); the limiting component is named when the load is above its limit (AC-2); the
/// cased and open-hole factors are recorded and shown per station (AC-3); the stations are ordered by measured depth and cover the
/// string (AC-4); and a saved run records the scenario and the friction factors (AC-6). Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class TorqueDragGatewayTests
{
    private const string ConnectionName = "eng07-torque-drag";

    private const double PipeWeightLbmFt = 19.5;
    private const double Span = 2000;

    [IntegrationFact]
    public async Task A_vertical_run_reproduces_the_buoyed_weight_and_names_its_limiting_component()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "torque-drag.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "torque-drag.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var plans = new OgWellPlanService(() => source, reporter);
        var service = new OgTorqueDragService(() => source, reporter, plans);

        // A vertical 2 000 m hole planned explicitly, which is what the run follows.
        var plan = await plans.SaveAsync(new OgPlanRequest(
            "W-1", "North", "Vertical", WellPlanRules.Explicit, "m",
            0, 0, 0, 0, 0, 0, null, null, null, 0, 0,
            [new OgPlanSection(Span, 0, 0)]), "sara");

        Assert.True(plan.Success, plan.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(Span, plan.Value!.FinalMd!.Value, 3);

        // The shipped grade table is casing grades; the operator's drill-pipe grade is added for this run.
        var settings = WellboreEnvironment.Current.CasingDesign;
        var added = settings.GradeYield.TryAdd("S135", 135);

        try
        {
            var request = new OgTorqueDragRequest(
                "W-1", TorqueDragRules.PickUp, 0.2, 0.3, 1000, 10, 10, null, 0, 0,
                [new OgDrillStringComponent("drill pipe", 5, 4.276, PipeWeightLbmFt, Span, "S135")]);

            var run = await service.RunAsync(request);

            Assert.True(run.Success, run.Message + " :: " + string.Join(" | ", reporter.Failures));

            var result = run.Value!;

            // ---- AC-1: on a vertical string the hook load is the buoyed weight, within tolerance ----
            var steel = settings.SteelDensity.In("kg/m3");
            var mud = new Measure(10, "ppg").In("kg/m3");
            var buoyancy = 1 - mud / steel;
            var kgPerM = PipeWeightLbmFt * 0.45359237 / 0.3048;
            var expectedKn = kgPerM * 9.80665 * buoyancy * Span / 1000;

            Assert.InRange(result.HookLoadKn, expectedKn * 0.99, expectedKn * 1.01);

            // A vertical hole in pick-up has no side force, so nothing adds to the hook load and no torque is generated.
            Assert.InRange(result.SurfaceTorqueKnm, -0.01, 0.01);
            Assert.All(result.Rows, row => Assert.InRange(row.SideForceKn, -0.01, 0.01));

            // ---- AC-4: ordered by MD and covering the string ----
            Assert.Equal(0d, result.Rows[0].Md, 6);
            Assert.Equal(Span, result.Rows[^1].Md, 6);
            Assert.Equal(result.Rows.OrderBy(row => row.Md).Select(row => row.Md), result.Rows.Select(row => row.Md));
            Assert.Equal((int)(Span / 10) + 1, result.Rows.Count);

            // ---- AC-3: each station says which interval it is in, and both appear ----
            Assert.Contains(result.Rows, row => row.Interval == "cased");
            Assert.Contains(result.Rows, row => row.Interval == "open hole");
            Assert.All(result.Rows, row => Assert.True(row.Interval is "cased" or "open hole", row.Interval));

            // The surface load is the hook load, and the bit carries no load in this operation.
            Assert.Equal(result.HookLoadKn, result.Rows[0].LoadKn, 3);
            Assert.Equal(0d, result.Rows[^1].LoadKn, 3);

            Assert.True(result.Passed);
            Assert.True(result.MaxUtilisation < 1, result.LimitText);
            Assert.All(result.Rows, row => Assert.True(row.LimitKn > 0, row.Describe(result.Unit)));

            // ---- AC-6: the saved run records the scenario and the factors ----
            var saved = await service.SaveAsync(request, "nadia");

            Assert.True(saved.Success, saved.Message + " :: " + string.Join(" | ", reporter.Failures));
            Assert.NotNull(saved.Value!.RunId);

            Assert.Equal(1, Count(database,
                "TORQUE_DRAG_RUN WHERE SCENARIO = 'PICK_UP' AND CASED_FRICTION = 0.2 AND OPEN_HOLE_FRICTION = 0.3 AND " +
                "SHOE_DEPTH = 1000 AND RAN_BY = 'nadia' AND PASSED = 'Y' AND STATION_COUNT = 201 AND LIMIT_COMPONENT = 'drill pipe'"));
            Assert.Equal(1, Count(database,
                "TORQUE_DRAG_RUN WHERE ABS(HOOK_LOAD_KN - " + expectedKn.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + ") < 5"));

            // ---- AC-2: an overpull above the pipe's yield fails the run and names the component ----
            var overpull = await service.RunAsync(request with { BitTensionKn = 4000 });

            Assert.True(overpull.Success, overpull.Message + " :: " + string.Join(" | ", reporter.Failures));
            Assert.False(overpull.Value!.Passed);
            Assert.Equal("drill pipe", overpull.Value.LimitComponent);
            Assert.Contains("pipe-body yield", overpull.Value.LimitText, StringComparison.Ordinal);
            Assert.True(overpull.Value.MaxUtilisation > 1, overpull.Value.LimitText);
            Assert.Contains(overpull.Value.Rows, row => row.OverLimit);

            // ---- the saved run is the one read back ----
            var last = await service.LoadLastAsync("W-1");

            Assert.True(last.Success, last.Message + " :: " + string.Join(" | ", reporter.Failures));
            Assert.Equal(saved.Value.RunId, last.Value!.RunId);
            Assert.Equal("nadia", last.Value.RanBy);
            Assert.Equal(TorqueDragRules.PickUp, last.Value.Scenario);
            Assert.Equal(result.HookLoadKn, last.Value.HookLoadKn, 2);

            // ---- the refusals ----
            var noPlan = await service.RunAsync(request with { WellId = "W-404" });

            Assert.False(noPlan.Success);
            Assert.Equal(TorqueDragRules.CodeNoTrajectory, noPlan.Reference);
            Assert.Contains("No trajectory is available for W-404", noPlan.Message!, StringComparison.Ordinal);

            var noRuns = await service.LoadLastAsync("W-404");

            Assert.False(noRuns.Success);
            Assert.Equal(TorqueDragRules.CodeUnknownRun, noRuns.Reference);

            var noFriction = await service.RunAsync(request with { CasedFriction = 0 });

            Assert.False(noFriction.Success);
            Assert.Equal(TorqueDragRules.CodeNoFriction, noFriction.Reference);
            Assert.Contains("cased-hole interval", noFriction.Message!, StringComparison.Ordinal);

            var shortString = await service.RunAsync(request with
            {
                String = [new OgDrillStringComponent("drill pipe", 5, 4.276, PipeWeightLbmFt, 500, "S135")],
            });

            Assert.False(shortString.Success);
            Assert.Equal(TorqueDragRules.CodeStringMissing, shortString.Reference);
            Assert.Contains("does not reach TD", shortString.Message!, StringComparison.Ordinal);

            var unknownGrade = await service.RunAsync(request with
            {
                String = [new OgDrillStringComponent("drill pipe", 5, 4.276, PipeWeightLbmFt, Span, "G105")],
            });

            Assert.False(unknownGrade.Success);
            Assert.Equal(TorqueDragRules.CodeComponentUnknown, unknownGrade.Reference);
            Assert.Contains("G105", unknownGrade.Message!, StringComparison.Ordinal);

            // ---- a failed read is a failure, not an empty run ----
            Execute(database, "DROP TABLE TORQUE_DRAG_RUN;");

            var failed = await service.LoadLastAsync("W-1");

            Assert.False(failed.Success);
            Assert.Null(failed.Value);
        }
        finally
        {
            if (added)
            {
                settings.GradeYield.Remove("S135");
            }
        }
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) => Execute(database, """
            CREATE TABLE WELL_PLAN (
                PLAN_ID TEXT,
                UWI TEXT,
                FIELD_NAME TEXT,
                PLAN_NAME TEXT,
                DESIGN_METHOD TEXT,
                DEPTH_OUOM TEXT,
                TIE_ON_MD REAL,
                TIE_ON_NORTH REAL,
                TIE_ON_EAST REAL,
                TIE_ON_TVD REAL,
                TIE_ON_INCLINATION REAL,
                TIE_ON_AZIMUTH REAL,
                TARGET_NORTH REAL,
                TARGET_EAST REAL,
                TARGET_TVD REAL,
                TARGET_TOLERANCE REAL,
                DESIGN_DOGLEG REAL,
                MAX_DOGLEG REAL,
                SECTIONS_TEXT TEXT,
                STATION_COUNT INTEGER,
                FINAL_MD REAL,
                FINAL_NORTH REAL,
                FINAL_EAST REAL,
                FINAL_TVD REAL,
                FINAL_INCLINATION REAL,
                FINAL_AZIMUTH REAL,
                MISS_DISTANCE REAL,
                TARGET_HIT TEXT,
                MAX_STATION_DOGLEG REAL,
                TORTUOSITY_BASIS TEXT,
                TORTUOSITY REAL,
                HANDED_OFF_AT_UTC TEXT,
                HANDED_OFF_BY TEXT,
                SAVED_BY TEXT,
                SAVED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PLAN_ID));

            CREATE TABLE TORQUE_DRAG_RUN (
                RUN_ID TEXT,
                UWI TEXT,
                PLAN_ID TEXT,
                SCENARIO TEXT,
                CASED_FRICTION REAL,
                OPEN_HOLE_FRICTION REAL,
                SHOE_DEPTH REAL,
                SHOE_DEPTH_OUOM TEXT,
                MUD_DENSITY REAL,
                MUD_DENSITY_OUOM TEXT,
                ELEMENT_LENGTH_M REAL,
                WOB REAL,
                WOB_OUOM TEXT,
                BIT_TENSION_N REAL,
                BIT_TORQUE_NM REAL,
                STRING_TEXT TEXT,
                HOOK_LOAD_KN REAL,
                SURFACE_TORQUE_KNM REAL,
                LIMIT_TEXT TEXT,
                LIMIT_COMPONENT TEXT,
                MAX_UTILISATION REAL,
                MARGIN_KN REAL,
                STATION_COUNT INTEGER,
                PASSED TEXT,
                RAN_BY TEXT,
                RAN_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (RUN_ID));
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
