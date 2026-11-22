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
/// ENG-08 against a real SQLite database and a real saved well plan: the circulating system is the owner's (AC-1); velocity and loss
/// are per section (AC-2); an ECD above the recorded fracture gradient is critical with the depth (AC-3); nozzles are recorded in
/// 32nds (AC-4); the optimiser's solved rate moves the ECD monotonically (AC-5); and a saved run records the mud model and the rate
/// (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class HydraulicsGatewayTests
{
    private const string ConnectionName = "eng08-hydraulics";

    private const double BitMd = 2000;

    [IntegrationFact]
    public async Task The_circulating_system_is_computed_per_section_and_a_target_ecd_is_solved_monotonically()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng08-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng08"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "hydraulics.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "hydraulics.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var plans = new OgWellPlanService(() => source, reporter);
        var service = new OgHydraulicsService(() => source, reporter, plans);

        // A vertical 2 000 m hole, which is what the run follows.
        var plan = await plans.SaveAsync(new OgPlanRequest(
            "W-1", "North", "Vertical", WellPlanRules.Explicit, "m",
            0, 0, 0, 0, 0, 0, null, null, null, 0, 0,
            [new OgPlanSection(BitMd, 0, 0)]), "sara");

        Assert.True(plan.Success, plan.Message + " :: " + string.Join(" | ", reporter.Failures));

        // ---- AC-1 and AC-2: the reference figures and one row per flow path ----
        var run = await service.RunAsync(Request());

        Assert.True(run.Success, run.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = run.Value!;

        Assert.Equal("m", result.Unit);
        Assert.Equal(BitMd, result.BitMd, 6);
        Assert.Equal(BitMd, result.BitTvd, 6);
        Assert.Equal(BitMd * 3.280839895, result.BitTvdFt, 3);

        Assert.Equal(3, result.Results.Count);
        Assert.Equal("inside pipe", result.Results[0].Interval);
        Assert.Equal("annulus", result.Results[1].Interval);
        Assert.All(result.Results, row => Assert.True(row.Computed, row.Describe("m")));
        Assert.All(result.Results, row => Assert.True(row.VelocityFtS > 0, row.Describe("m")));
        Assert.All(result.Results, row => Assert.True(row.LossPsi > 0, row.Describe("m")));

        // The vertical annulus above the shoe flows slower than the narrower one below it, at the same rate.
        Assert.True(result.Results[2].VelocityFtS > result.Results[1].VelocityFtS,
            result.Results[2].Describe("m") + " vs " + result.Results[1].Describe("m"));

        // AC-1: the owner's own relation, recomputed here from its two published constants.
        Assert.NotNull(result.Ecd);
        Assert.NotNull(result.AnnularLoss);
        Assert.Equal(10 + result.AnnularLoss!.Value / (0.052 * result.BitTvdFt), result.Ecd!.Value, 6);
        Assert.True(result.AnnularLoss > 0, result.SummaryText);

        // The profile's deepest point is the same ECD the system result quotes, so the two cannot disagree on one page.
        var deepest = result.Profile.OrderByDescending(point => point.Depth).First();

        Assert.Equal(BitMd, deepest.Depth, 6);
        Assert.Equal(result.Ecd!.Value, deepest.Value, 4);
        Assert.Equal(result.MaxEcd!.Value, deepest.Value, 6);
        Assert.Equal(BitMd, result.MaxEcdDepth!.Value, 6);

        // AC-4: the nozzles are recorded and shown in 32nds.
        Assert.Equal(new[] { 12d, 12d, 12d }, result.Nozzles32nds);
        Assert.Contains("12/32", result.NozzlesText, StringComparison.Ordinal);
        Assert.Contains("32nds of an inch", result.NozzlesText, StringComparison.Ordinal);
        Assert.True(result.BitLoss > 0, result.BitLoss.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.True(result.Bhhp > 0);
        Assert.True(result.ImpactForce > 0);

        // ---- AC-3: the ECD above the recorded fracture gradient is critical, with the depth ----
        Assert.True(result.FractureRecorded);
        Assert.Equal(10.25, result.FractureGradientPpg!.Value, 6);
        Assert.Equal(1000, result.FractureDepth!.Value, 3);
        Assert.True(result.FractureExceeded);
        Assert.StartsWith("CRITICAL", result.CriticalText, StringComparison.Ordinal);
        Assert.Contains("at " + BitMd.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " m",
            result.CriticalText, StringComparison.Ordinal);
        Assert.Contains("10.25 ppg", result.CriticalText, StringComparison.Ordinal);

        // A lighter mud puts the same system below the same recorded gradient, so the comparison is real.
        var light = await service.RunAsync(Request() with { MudDensityPpg = 9.6 });

        Assert.True(light.Success, light.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.False(light.Value!.FractureExceeded);
        Assert.Contains("within the recorded fracture gradient", light.Value.CriticalText, StringComparison.Ordinal);

        // ---- the feature's "no nozzle" state: zero bit loss, flagged, and the ECD unchanged ----
        var noNozzle = await service.RunAsync(Request() with { Nozzles32nds = [] });

        Assert.True(noNozzle.Success, noNozzle.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.True(noNozzle.Value!.NoNozzles);
        Assert.Equal(0d, noNozzle.Value.BitLoss);
        Assert.Contains(noNozzle.Value.Warnings,
            warning => warning.Contains("No nozzle is recorded", StringComparison.Ordinal));

        // The nozzle sets the bit loss, not the ECD: the two runs agree on the ECD and differ on the standpipe pressure.
        Assert.Equal(result.Ecd!.Value, noNozzle.Value.Ecd!.Value, 6);
        Assert.NotNull(noNozzle.Value.StandpipePressure);
        Assert.True(double.IsFinite(noNozzle.Value.StandpipePressure!.Value), "the standpipe pressure is not infinity");
        Assert.Equal(result.StandpipePressure!.Value - result.BitLoss, noNozzle.Value.StandpipePressure!.Value, 6);

        // ---- AC-5: the solved rate rises monotonically with the target, and the ECD with the rate ----
        var targets = new[] { 10.4, 10.6, 10.8 };
        var solved = new List<OgHydraulicsRecommendation>();

        foreach (var target in targets)
        {
            var recommendation = await service.OptimiseAsync(Request(), target);

            Assert.True(recommendation.Success, recommendation.Message + " :: " +
                                               string.Join(" | ", reporter.Failures));
            Assert.True(recommendation.Value!.Feasible, recommendation.Value.Reason);
            Assert.NotNull(recommendation.Value.FlowRateGpm);

            // The model steps at the laminar-to-turbulent transition, so the solved rate reaches the target from below and may
            // overshoot it by the size of that step - never fall short of it.
            Assert.True(recommendation.Value.EcdAtSolution!.Value >= target - 1e-6,
                recommendation.Value.EcdAtSolution.Value.ToString("0.######"));
            Assert.True(recommendation.Value.EcdAtSolution.Value <= target + 0.1,
                recommendation.Value.Reason);

            solved.Add(recommendation.Value);
        }

        Assert.True(solved[0].FlowRateGpm < solved[1].FlowRateGpm, "rate must rise with the target");
        Assert.True(solved[1].FlowRateGpm < solved[2].FlowRateGpm, "rate must rise with the target");
        Assert.True(solved[0].EcdAtSolution < solved[1].EcdAtSolution, "ECD must rise with the rate");
        Assert.True(solved[1].EcdAtSolution < solved[2].EcdAtSolution, "ECD must rise with the rate");

        // The trace is the search's own record: the ECD rises across every probe it visited.
        var traced = solved[2].Trace
            .Select(line => line.Split("->", StringSplitOptions.TrimEntries))
            .Where(pair => pair.Length == 2)
            .Select(pair => double.Parse(pair[1].Split(' ')[0], System.Globalization.CultureInfo.InvariantCulture))
            .ToList();

        Assert.True(traced.Count >= 5, string.Join(" | ", solved[2].Trace));
        Assert.Equal(traced.OrderBy(value => value).ToList(), traced);

        // Where it overshoots, the reason names the transition that makes an exact hit impossible.
        Assert.All(solved.Where(item => item.EcdAtSolution > item.TargetEcdPpg + 1e-6),
            item => Assert.Contains("steps from laminar to turbulent flow", item.Reason, StringComparison.Ordinal));

        // The recommendation reports the bit hydraulics at the solved rate, with the nozzles as entered.
        Assert.Contains("12/32", solved[2].NozzleText, StringComparison.Ordinal);
        Assert.True(solved[2].BitLossPsi > 0);
        Assert.StartsWith("Feasible", solved[2].Describe(), StringComparison.Ordinal);

        // ---- AC-6: the saved run records the mud model, the rate and the answer ----
        var saved = await service.SaveAsync(Request(), "nadia");

        Assert.True(saved.Success, saved.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.NotNull(saved.Value!.RunId);

        Assert.Equal(1, Count(database,
            "HYDRAULICS_RUN WHERE RHEOLOGY_MODEL = 'binghamPlastic' AND ABS(MUD_DENSITY_PPG - 10) < 0.001 AND " +
            "FLOW_RATE = 300 AND FLOW_RATE_OUOM = 'gpm' AND NOZZLE_COUNT = 3 AND SECTION_COUNT = 3 AND RAN_BY = 'nadia' AND " +
            "ABS(ECD - " + result.Ecd!.Value.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture) + ") < 0.0001"));
        Assert.Equal(1, Count(database,
            "HYDRAULICS_RUN WHERE THETA600 = 30 AND THETA300 = 20 AND ABS(PLASTIC_VISCOSITY - 10) < 0.001 AND " +
            "ABS(YIELD_POINT - 10) < 0.001 AND FRACTURE_EXCEEDED = 'Y'"));
        Assert.Equal(1, Count(database, "HYDRAULICS_RUN WHERE NOZZLES_TEXT LIKE '%12%' AND BIT_LOSS > 0"));

        // The run is replayed from its recorded inputs, not copied.
        var last = await service.LoadLastAsync("W-1");

        Assert.True(last.Success, last.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(saved.Value.RunId, last.Value!.RunId);
        Assert.Equal("nadia", last.Value.RanBy);
        Assert.Equal(result.Ecd!.Value, last.Value.Ecd!.Value, 6);
        Assert.Equal(result.Results.Count, last.Value.Results.Count);

        // ---- refusals that need the plan ----
        var beyond = await service.RunAsync(Request(casedBottom: 3000));

        Assert.False(beyond.Success);
        Assert.Equal(HydraulicsRules.CodeSectionBeyondWell, beyond.Reference);
        Assert.Contains("below the hole the run follows", beyond.Message!, StringComparison.Ordinal);

        var noLength = await service.RunAsync(Request(casedTop: 500, casedBottom: 500));

        Assert.False(noLength.Success);
        Assert.Equal(HydraulicsRules.CodeSectionInvalid, noLength.Reference);
        Assert.Contains("no length to flow through", noLength.Message!, StringComparison.Ordinal);

        var badGap = await service.RunAsync(Request(casedInner: 9));

        Assert.False(badGap.Success);
        Assert.Equal(HydraulicsRules.CodeSectionInvalid, badGap.Reference);
        Assert.Contains("no annular gap", badGap.Message!, StringComparison.Ordinal);

        var overlap = await service.RunAsync(new OgHydraulicsRequest(
            "W-1", 10, 30, 20, 300, "gpm", 100, new[] { 12d },
            [
                new OgFlowSection("cased annulus", 0, 1500, 8.681, 5),
                new OgFlowSection("open-hole annulus", 1000, BitMd, 8.5, 5),
            ]));

        Assert.False(overlap.Success);
        Assert.Equal(HydraulicsRules.CodeSectionInvalid, overlap.Reference);
        Assert.Contains("counted twice", overlap.Message!, StringComparison.Ordinal);

        var noPlan = await service.RunAsync(Request() with { WellId = "W-404" });

        Assert.False(noPlan.Success);
        Assert.Equal(HydraulicsRules.CodeNoTrajectory, noPlan.Reference);
        Assert.Contains("No trajectory is available for W-404", noPlan.Message!, StringComparison.Ordinal);

        var noRuns = await service.LoadLastAsync("W-404");

        Assert.False(noRuns.Success);
        Assert.Equal(HydraulicsRules.CodeUnknownRun, noRuns.Reference);

        // ---- a failed read is a failure, not an empty run ----
        Execute(database, "DROP TABLE HYDRAULICS_RUN;");

        var failed = await service.LoadLastAsync("W-1");

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the request

    /// <summary>
    /// The reference system: 4.276 in drill pipe to the bit, 8.681 in casing to a 1 000 m shoe and 8.5 in hole below it, at 300 gpm
    /// with a 10 ppg Bingham mud.
    /// </summary>
    private static OgHydraulicsRequest Request(
        double casedTop = 0,
        double casedBottom = 1000,
        double casedInner = 5) =>
        new("W-1", 10, 30, 20, 300, "gpm", 100, new[] { 12d, 12d, 12d },
        new List<OgFlowSection>
        {
            new OgFlowSection("drill pipe", 0, BitMd, 4.276, null),
            new OgFlowSection("cased annulus", casedTop, casedBottom, 8.681, casedInner),
            new OgFlowSection("open-hole annulus", 1000, BitMd, 8.5, 5),
        });

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

            CREATE TABLE HYDRAULICS_RUN (
                RUN_ID TEXT,
                UWI TEXT,
                PLAN_ID TEXT,
                RHEOLOGY_MODEL TEXT,
                MUD_DENSITY_PPG REAL,
                THETA600 REAL,
                THETA300 REAL,
                PLASTIC_VISCOSITY REAL,
                YIELD_POINT REAL,
                FLOW_INDEX REAL,
                CONSISTENCY_INDEX REAL,
                FLOW_RATE REAL,
                FLOW_RATE_OUOM TEXT,
                SURFACE_LOSS_PSI REAL,
                BIT_TVD REAL,
                BIT_TVD_OUOM TEXT,
                NOZZLES_TEXT TEXT,
                NOZZLE_COUNT INTEGER,
                SECTIONS_TEXT TEXT,
                SECTION_COUNT INTEGER,
                STANDPIPE_PRESSURE REAL,
                ANNULAR_LOSS REAL,
                ECD REAL,
                BIT_LOSS REAL,
                BIT_HHP REAL,
                IMPACT_FORCE REAL,
                MAX_ECD REAL,
                MAX_ECD_DEPTH REAL,
                FRACTURE_GRADIENT REAL,
                FRACTURE_DEPTH REAL,
                FRACTURE_EXCEEDED TEXT,
                CRITICAL_TEXT TEXT,
                DEPTH_OUOM TEXT,
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

            CREATE TABLE WELL_ANNULUS (
                ANNULUS_ID TEXT,
                UWI TEXT,
                ANNULUS_NAME TEXT,
                INNER_STRING_ID TEXT,
                OUTER_STRING_ID TEXT,
                FLUID TEXT,
                FLUID_DENSITY REAL,
                FLUID_DENSITY_OUOM TEXT,
                SHOE_FRACTURE_GRADIENT REAL,
                SHOE_FRACTURE_GRADIENT_OUOM TEXT,
                WELLHEAD_RATING REAL,
                WELLHEAD_RATING_OUOM TEXT,
                RECORDED_MAASP REAL,
                RECORDED_MAASP_OUOM TEXT,
                RECORDED_MAWOP REAL,
                RECORDED_MAWOP_OUOM TEXT,
                NOTES TEXT,
                RECORDED_BY TEXT,
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
                PRIMARY KEY (ANNULUS_ID));

            CREATE TABLE WELL_TUBULAR (
                UWI TEXT,
                SOURCE TEXT,
                TUBING_TYPE TEXT,
                TUBING_OBS_NO REAL,
                ACTIVE_IND TEXT,
                BASE_DEPTH REAL,
                BASE_DEPTH_OUOM TEXT,
                BASE_STRAT_UNIT_ID TEXT,
                CAT_EQUIP_ID TEXT,
                COLLAR_TYPE TEXT,
                COUPLING_TYPE TEXT,
                EFFECTIVE_DATE TEXT,
                EQUIPMENT_ID TEXT,
                EXPIRY_DATE TEXT,
                HOLE_SIZE REAL,
                HOLE_SIZE_OUOM TEXT,
                HUNG_TOP_DEPTH REAL,
                HUNG_TOP_DEPTH_OUOM TEXT,
                INSIDE_DIAMETER REAL,
                INSIDE_DIAMETER_OUOM TEXT,
                JOINT_COUNT REAL,
                LEFT_IN_HOLE_LENGTH REAL,
                LEFT_IN_HOLE_LENGTH_OUOM TEXT,
                LINER_TYPE TEXT,
                MANUFACTURER_BA_ID TEXT,
                MIXED_STRING_IND TEXT,
                OBSERVATION_DATE TEXT,
                OUTSIDE_DIAMETER REAL,
                OUTSIDE_DIAMETER_DESC TEXT,
                OUTSIDE_DIAMETER_OUOM TEXT,
                PACKER_SET_DEPTH REAL,
                PACKER_SET_DEPTH_OUOM TEXT,
                PERIOD_OBS_NO REAL,
                PPDM_GUID TEXT,
                PULLED_LENGTH REAL,
                PULLED_LENGTH_OUOM TEXT,
                REMARK TEXT,
                REPORTED_BASE_TVD REAL,
                REPORTED_BASE_TVD_OUOM TEXT,
                REPORTED_TOP_TVD REAL,
                REPORTED_TOP_TVD_OUOM TEXT,
                SEA_FLOOR_PENETRATION REAL,
                SEA_FLOOR_PENETRATION_OUOM TEXT,
                SHOE_DEPTH REAL,
                SHOE_DEPTH_OUOM TEXT,
                STEEL_SPEC TEXT,
                STRAT_NAME_SET_ID TEXT,
                TOP_STRAT_UNIT_ID TEXT,
                TORQUE REAL,
                TORQUE_OUOM TEXT,
                TUBING_DENSITY REAL,
                TUBING_DENSITY_OUOM TEXT,
                TUBING_GRADE TEXT,
                TUBING_STRENGTH REAL,
                TUBING_STRENGTH_OUOM TEXT,
                TUBING_WEIGHT REAL,
                TUBING_WEIGHT_OUOM TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (UWI, SOURCE, TUBING_TYPE, TUBING_OBS_NO));

            INSERT INTO WELL_TUBULAR (UWI, SOURCE, TUBING_TYPE, TUBING_OBS_NO, OUTSIDE_DIAMETER,
                OUTSIDE_DIAMETER_OUOM, INSIDE_DIAMETER, INSIDE_DIAMETER_OUOM, TUBING_GRADE, TUBING_WEIGHT,
                TUBING_WEIGHT_OUOM, BASE_DEPTH, BASE_DEPTH_OUOM, SHOE_DEPTH, SHOE_DEPTH_OUOM, HUNG_TOP_DEPTH,
                HUNG_TOP_DEPTH_OUOM, ACTIVE_IND)
                VALUES ('W-1', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft',
                    1000, 'm', 1000, 'm', 0, 'm', 'Y');

            INSERT INTO WELL_ANNULUS (ANNULUS_ID, UWI, ANNULUS_NAME, INNER_STRING_ID, OUTER_STRING_ID, FLUID,
                SHOE_FRACTURE_GRADIENT, SHOE_FRACTURE_GRADIENT_OUOM, ACTIVE_IND)
                VALUES ('AN-8A', 'W-1', 'A', 'W-1|PPDM|CASING|1', 'W-1|PPDM|CASING|1', 'WATER', 10.25, 'ppg', 'Y');
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
