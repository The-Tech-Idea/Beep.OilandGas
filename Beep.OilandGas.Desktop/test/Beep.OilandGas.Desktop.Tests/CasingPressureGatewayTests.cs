using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Integrity;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-04 against a real SQLite database: the MAWOP is the API RP 90 rule computed from the recorded pipe and the MAASP is the
/// owner's screening with its own deratings, each shown with the term behind it (AC-2); a recorded test is classified with the
/// class and the rule (AC-1); a pressure above the MAASP is CRITICAL rather than a warning (AC-3); a test with no valve-closed
/// time is accepted and marked incomplete (AC-4); the trend holds only recorded tests (AC-5); and the thresholds are the
/// configured ones (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class CasingPressureGatewayTests
{
    private const string ConnectionName = "eng04-casing-pressure";

    [IntegrationFact]
    public async Task The_annuli_are_screened_and_a_test_is_classified_and_recorded()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "casing-pressure.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "casing-pressure.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgCasingPressureService(() => source, reporter);

        // ---- AC-2: MAWOP and MAASP per annulus, with their basis ----
        var loaded = await service.LoadAsync("W-1", null);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal(2, workspace.Annuli.Count);

        var a = workspace.Selected!;

        Assert.Equal("A", a.Name);
        Assert.Equal("TUBING", a.InnerString);
        Assert.Equal("CASING", a.OuterString);
        Assert.Equal("GAS", a.Fluid);

        // 50 % of the casing MIYP (6 870 psi) is 3 435; the outermost term is 30 % of it, 2 061, which governs.
        Assert.Equal(2061d, a.Mawop!.Value, 1);
        Assert.Contains("outermost casing", a.MawopGoverning!, StringComparison.Ordinal);
        Assert.Equal(3, a.MawopTerms.Count);
        Assert.Contains(a.MawopTerms, term => term.Contains("50% of the MIYP of CASING", StringComparison.Ordinal));
        Assert.Contains(a.MawopTerms, term => term.Contains("3435", StringComparison.Ordinal));

        Assert.NotNull(a.Maasp);
        Assert.Equal(4, a.MaaspLimits.Count);
        Assert.All(a.MaaspLimits, limit => Assert.Contains("psi (", limit, StringComparison.Ordinal));
        Assert.Contains(a.MaaspLimits, limit => limit.StartsWith("Outer casing burst", StringComparison.Ordinal));
        Assert.Contains(a.MaaspLimits, limit => limit.StartsWith("Formation at outer shoe", StringComparison.Ordinal));
        Assert.Contains(a.MaaspLimits, limit => limit.StartsWith("Wellhead / hanger rating", StringComparison.Ordinal));

        Assert.True(a.CanClassify);
        Assert.Null(a.CurrentPressure);
        Assert.Contains("MAWOP 2061", a.LimitsText, StringComparison.Ordinal);
        Assert.Contains("governed by 30% of the MIYP of CASING", a.LimitsText, StringComparison.Ordinal);
        Assert.Contains("MAASP", a.LimitsText, StringComparison.Ordinal);

        // The annulus whose inner string is not in the model cannot be screened, and says which input is missing.
        var b = workspace.Annuli.Single(annulus => annulus.Name == "B");

        Assert.False(b.CanClassify);
        Assert.Null(b.Mawop);
        Assert.Contains("not in the model", string.Join(" ", b.MawopNotes), StringComparison.Ordinal);
        Assert.Contains("MAWOP not computed", b.LimitsText, StringComparison.Ordinal);

        Assert.Empty(workspace.History);
        Assert.Null(workspace.Classification);

        // ---- AC-1: a clean test is classified with the API class and the rule ----
        var clean = await service.RecordTestAsync(
            "W-1", "A",
            new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 1, 8, 10, 0, DateTimeKind.Utc),
            22m, "degC", "bled to zero in six minutes",
            [
                new OgPressureReading(1200m, "psi", new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 1, 8, 6, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 1, 8, 40, 0, DateTimeKind.Utc)),
            ],
            "sara");

        Assert.True(clean.Success, clean.Message + " :: " + string.Join(" | ", reporter.Failures));

        var classified = clean.Value!.Classification!;

        Assert.Equal(CasingPressureRules.NotSustained, classified.Class);
        Assert.Equal(CasingPressureRules.Info, classified.Severity);
        Assert.True(classified.Complete);
        Assert.Contains("not sustained", classified.Rule, StringComparison.Ordinal);
        Assert.Contains("meets the API RP 90 criteria", classified.Rule, StringComparison.Ordinal);
        Assert.Equal(2061d, classified.Mawop!.Value, 1);
        Assert.Equal(a.Maasp, classified.Maasp);
        Assert.Null(classified.Exceedance);
        Assert.Contains("did not rebuild", classified.Action, StringComparison.Ordinal);

        Assert.Single(clean.Value.History);
        Assert.Equal(1200d, clean.Value.Selected!.CurrentPressure!.Value, 4);

        Assert.Equal(1, Count(database,
            "CASING_PRESSURE_TEST WHERE ANNULUS_NAME = 'A' AND COMPLETE = 'Y' AND CLASS_TEXT = 'NOT SUSTAINED' AND " +
            "SEVERITY = 'INFO' AND RECORDED_BY = 'sara' AND TEMPERATURE = 22 AND BLEED_OFF_TEXT = 'bled to zero in six minutes'"));

        // ---- AC-3: above the MAASP is critical, never a warning ----
        var maasp = a.Maasp!.Value;
        var over = (decimal)maasp + 100m;

        var critical = await service.RecordTestAsync(
            "W-1", "A",
            new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 2, 8, 10, 0, DateTimeKind.Utc),
            22m, "degC", "bled to zero",
            [
                new OgPressureReading(over, "psi", new DateTime(2026, 11, 2, 8, 0, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 2, 8, 6, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 2, 8, 40, 0, DateTimeKind.Utc)),
            ],
            "nadia");

        Assert.True(critical.Success, critical.Message + " :: " + string.Join(" | ", reporter.Failures));

        var overLimit = critical.Value!.Classification!;

        Assert.Equal(CasingPressureRules.Critical, overLimit.Severity);
        Assert.True(overLimit.IsCritical);
        Assert.Equal(100d, overLimit.Exceedance!.Value, 1);
        Assert.Contains("Reduce the annular pressure below the MAASP", overLimit.Action, StringComparison.Ordinal);
        Assert.Contains("above the MAASP", overLimit.Describe(), StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "CASING_PRESSURE_TEST WHERE ANNULUS_NAME = 'A' AND SEVERITY = 'CRITICAL' AND ABS(EXCEEDANCE - 100) < 0.5"));

        // The annulus state reads from the recorded test, and the higher pressure is now the current one.
        var reread = await service.LoadAsync("W-1", "A");

        Assert.True(reread.Success, reread.Message);
        Assert.Equal((double)over, reread.Value!.Selected!.CurrentPressure!.Value, 3);
        Assert.Contains("above the MAASP", reread.Value.Selected.StateText, StringComparison.Ordinal);

        // ---- AC-4: a test with no valve-closed time is accepted and marked incomplete ----
        var incomplete = await service.RecordTestAsync(
            "W-1", "A",
            new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc),
            null,
            21m, "degC", "no bleed-down recorded",
            [new OgPressureReading(800m, "psi", new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc))],
            "omar");

        Assert.True(incomplete.Success, incomplete.Message + " :: " + string.Join(" | ", reporter.Failures));

        var incompleteClass = incomplete.Value!.Classification!;

        Assert.Equal(CasingPressureRules.Unclassified, incompleteClass.Class);
        Assert.False(incompleteClass.Complete);
        Assert.Contains("no valve-closed time", incompleteClass.Rule, StringComparison.Ordinal);
        Assert.Contains("incomplete", incompleteClass.Describe(), StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "CASING_PRESSURE_TEST WHERE COMPLETE = 'N' AND CLASS_TEXT = 'UNCLASSIFIED' AND DURATION_HOURS IS NULL"));

        // ---- AC-5: the trend is exactly the recorded tests, in test order ----
        Assert.Equal(3, incomplete.Value.History.Count);
        Assert.Equal(
            incomplete.Value.History.OrderBy(test => test.At).Select(test => test.Id),
            incomplete.Value.History.Select(test => test.Id));

        // ---- AC-6: the thresholds are the configured ones, not literals ----
        var settings = IntegrityToolbox.Settings.CasingPressure;
        var threshold = settings.DiagnosticThreshold.In("psi");
        var restore = threshold;

        try
        {
            settings.DiagnosticThreshold = new Measure(50, "psi");

            var above = await service.RecordTestAsync(
                "W-1", "A",
                new DateTime(2026, 11, 4, 8, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 11, 4, 8, 10, 0, DateTimeKind.Utc),
                20m, "degC", "bled to zero",
                [
                    new OgPressureReading(60m, "psi", new DateTime(2026, 11, 4, 8, 0, 0, DateTimeKind.Utc)),
                    new OgPressureReading(0m, "psi", new DateTime(2026, 11, 4, 8, 5, 0, DateTimeKind.Utc)),
                    new OgPressureReading(0m, "psi", new DateTime(2026, 11, 4, 8, 30, 0, DateTimeKind.Utc)),
                ],
                "sara");

            Assert.True(above.Success, above.Message);
            Assert.Equal(CasingPressureRules.NotSustained, above.Value!.Classification!.Class);
        }
        finally
        {
            settings.DiagnosticThreshold = new Measure(restore, "psi");
        }

        var below = await service.RecordTestAsync(
            "W-1", "A",
            new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 5, 8, 10, 0, DateTimeKind.Utc),
            20m, "degC", "bled to zero",
            [
                new OgPressureReading(60m, "psi", new DateTime(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 5, 8, 5, 0, DateTimeKind.Utc)),
                new OgPressureReading(0m, "psi", new DateTime(2026, 11, 5, 8, 30, 0, DateTimeKind.Utc)),
            ],
            "sara");

        Assert.True(below.Success, below.Message);
        Assert.Equal(CasingPressureRules.BelowThreshold, below.Value!.Classification!.Class);
        Assert.Contains("monitor", below.Value.Classification.Action, StringComparison.Ordinal);

        // ---- the feature's states ----
        var unknownAnnulus = await service.LoadAsync("W-1", "Z");

        Assert.False(unknownAnnulus.Success);
        Assert.Equal(CasingPressureRules.CodeAnnulusUnknown, unknownAnnulus.Reference);

        var noMawop = await service.RecordTestAsync(
            "W-1", "B", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "",
            [new OgPressureReading(100m, "psi", DateTime.UtcNow)], "sara");

        Assert.False(noMawop.Success);
        Assert.Equal(CasingPressureRules.CodeNoMawop, noMawop.Reference);
        Assert.Contains("not in the model", noMawop.Message!, StringComparison.Ordinal);

        var noReadings = await service.RecordTestAsync(
            "W-1", "A", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, 20m, "degC", "", [], "sara");

        Assert.False(noReadings.Success);
        Assert.Equal(CasingPressureRules.CodeNoReadings, noReadings.Reference);

        // A well with no tubular record has no annulus to monitor, and says so.
        var bare = await service.LoadAsync("W-2", null);

        Assert.True(bare.Success, bare.Message);
        Assert.Empty(bare.Value!.Annuli);
        Assert.Contains(bare.Value.Warnings, warning => warning.Contains("no annulus to monitor", StringComparison.Ordinal));

        // ---- a failed read is a failure, not an empty list ----
        Execute(database, "DROP TABLE CASING_PRESSURE_TEST;");

        var failedRead = await service.LoadAsync("W-1", null);

        Assert.False(failedRead.Success);
        Assert.Null(failedRead.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
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

            CREATE TABLE WELL_DIR_SRVY_STATION (
                UWI TEXT,
                SURVEY_ID TEXT,
                SOURCE TEXT,
                DEPTH_OBS_NO REAL,
                ACCURACY_PROBLEM_IND TEXT,
                ACCURACY_PROBLEM_REASON TEXT,
                ACTIVE_IND TEXT,
                AZIMUTH REAL,
                AZIMUTH_OUOM TEXT,
                CLOSURE_DIST REAL,
                CLOSURE_DIST_DIR REAL,
                CLOSURE_DIST_DIR_OUOM TEXT,
                CLOSURE_DIST_OUOM TEXT,
                DELTA_LAT REAL,
                DELTA_LONG REAL,
                DOG_LEG_SEVERITY REAL,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                INCLINATION REAL,
                INCLINATION_OUOM TEXT,
                LATITUDE REAL,
                LONGITUDE REAL,
                PERIOD_OBS_NO REAL,
                POINT_TYPE TEXT,
                PPDM_GUID TEXT,
                RAD_UNCERT_DIST REAL,
                RAD_UNCERT_DIST_OUOM TEXT,
                RAD_UNCERT_DIST_REASON TEXT,
                REMARK TEXT,
                RPT_AZIMUTH REAL,
                RPT_AZIMUTH_OUOM TEXT,
                STATION_ID TEXT,
                STATION_MD REAL,
                STATION_MD_OUOM TEXT,
                STATION_TVD REAL,
                STATION_TVDSS REAL,
                STATION_TVDSS_OUOM TEXT,
                STATION_TVD_OUOM TEXT,
                SURVEY_RUN_NUM TEXT,
                SURVEY_TYPE TEXT,
                VERTICAL_SECTION REAL,
                VERTICAL_SECTION_OUOM TEXT,
                X_OFFSET REAL,
                X_OFFSET_EW_DIRECTION TEXT,
                X_OFFSET_OUOM TEXT,
                Y_OFFSET REAL,
                Y_OFFSET_NS_DIRECTION TEXT,
                Y_OFFSET_OUOM TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO));

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

            CREATE TABLE CASING_PRESSURE_TEST (
                TEST_ID TEXT,
                UWI TEXT,
                ANNULUS_NAME TEXT,
                BLEED_START_UTC TEXT,
                VALVE_CLOSED_UTC TEXT,
                DURATION_HOURS REAL,
                READINGS_JSON TEXT,
                INITIAL_PRESSURE REAL,
                MAX_PRESSURE REAL,
                PRESSURE_OUOM TEXT,
                TEMPERATURE REAL,
                TEMPERATURE_OUOM TEXT,
                BLEED_OFF_TEXT TEXT,
                CLASS_TEXT TEXT,
                RULE_TEXT TEXT,
                ACTION_TEXT TEXT,
                SEVERITY TEXT,
                MAWOP REAL,
                MAASP REAL,
                EXCEEDANCE REAL,
                COMPLETE TEXT,
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
                PRIMARY KEY (TEST_ID));

            INSERT INTO WELL_TUBULAR (UWI, SOURCE, TUBING_TYPE, TUBING_OBS_NO, OUTSIDE_DIAMETER,
                OUTSIDE_DIAMETER_OUOM, INSIDE_DIAMETER, INSIDE_DIAMETER_OUOM, TUBING_GRADE, TUBING_WEIGHT,
                TUBING_WEIGHT_OUOM, BASE_DEPTH, BASE_DEPTH_OUOM, SHOE_DEPTH, SHOE_DEPTH_OUOM, HUNG_TOP_DEPTH,
                HUNG_TOP_DEPTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft', 10000, 'ft', 10000, 'ft', 0, 'ft', 'Y'),
                ('W-1', 'PPDM', 'TUBING', 1, 4.5, 'in', 3.958, 'in', 'L80', 12.6, 'lbm/ft', 9500, 'ft', 9500, 'ft', 0, 'ft', 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, AZIMUTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'S1', 'PPDM', 1, 0, 'ft', 0, 0, 'deg', 'Y'),
                ('W-1', 'S1', 'PPDM', 2, 10000, 'ft', 0, 0, 'deg', 'Y');

            INSERT INTO WELL_ANNULUS (ANNULUS_ID, UWI, ANNULUS_NAME, INNER_STRING_ID, OUTER_STRING_ID, FLUID,
                FLUID_DENSITY, FLUID_DENSITY_OUOM, SHOE_FRACTURE_GRADIENT, SHOE_FRACTURE_GRADIENT_OUOM,
                WELLHEAD_RATING, WELLHEAD_RATING_OUOM, RECORDED_BY, RECORDED_AT_UTC) VALUES
                ('AN-A', 'W-1', 'A', 'W-1|PPDM|TUBING|1', 'W-1|PPDM|CASING|1', 'GAS', 9, 'ppg', 1.0, 'psi/ft', 5000, 'psi', 'sara', '2026-10-01'),
                ('AN-B', 'W-1', 'B', 'W-1|PPDM|MISSING|1', 'W-1|PPDM|CASING|1', 'BRINE', 8.6, 'ppg', NULL, NULL, NULL, NULL, 'sara', '2026-10-01');
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
