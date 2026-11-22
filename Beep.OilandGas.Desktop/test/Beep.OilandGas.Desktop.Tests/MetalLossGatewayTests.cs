using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.OilGas.Integrity;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-03 against a real SQLite database and a real multi-finger caliper LAS file: the joints are the tools' own analysis of the
/// measured radii, so the wall loss is a computed figure rather than a stored one (AC-1); the damage class is the configured one
/// and its absence is stated (AC-2); the re-rating uses the measured wall and names its load case (AC-3); remaining life needs a
/// dated earlier inspection at the same depth and a configured limit (AC-4); and the recorded result keeps the log, the fingers,
/// the wall basis and the classes (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class MetalLossGatewayTests
{
    private const string ConnectionName = "eng03-metal-loss";

    [IntegrationFact]
    public async Task A_caliper_log_is_measured_joint_by_joint_and_the_result_is_recorded()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "metal-loss.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "metal-loss.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var caliper = Path.Combine(root, "caliper.las");
        File.WriteAllText(caliper, Caliper(radius: 3.1, fingers: 8));

        var service = new OgMetalLossService(() => source, reporter);

        // ---- AC-1: the joints are measured from the recorded radii ----
        var analysed = await service.AnalyseAsync("W-1", caliper, 10m, 5000m, "sara");

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = analysed.Value!;

        Assert.Single(workspace.Strings);
        Assert.Equal("PRODUCTION", workspace.Strings[0].Name);
        Assert.Equal(7d, workspace.Strings[0].Od);
        Assert.Equal(6d, workspace.Strings[0].InnerDiameter);
        Assert.Equal(0.5d, workspace.Strings[0].NominalWall);

        var joint = Assert.Single(workspace.Joints);

        Assert.Equal(1000d, joint.Top);
        Assert.Equal(1010d, joint.Bottom);
        Assert.Equal("ft", joint.Unit);

        // A 7 x 6 pipe with every finger reading a 3.1 in radius has 0.2 of the 0.5 in wall gone.
        Assert.Equal(0.2d, joint.MaxPenetration!.Value, 4);
        Assert.Equal(0.4d, joint.MinRemainingWall!.Value, 4);
        Assert.Equal(0.5d, joint.NominalWall!.Value, 4);
        Assert.Equal(20d, joint.LossPercent!.Value, 3);
        Assert.Contains("loss 20", joint.Describe(), StringComparison.Ordinal);

        // The areal metal loss is the tool's own figure for the same radii.
        Assert.Equal((4 * 3.1 * 3.1 - 36) / (49 - 36), joint.MaxMetalLoss!.Value, 4);

        // AC-2: the classes ship empty, so the joint is unclassified and the page says why.
        Assert.Null(joint.DamageClass);
        Assert.Contains("No damage class is configured", workspace.DamageClassNote, StringComparison.Ordinal);

        // AC-3: the re-rating is at the measured wall, and it names its load case.
        Assert.Equal("L80", joint.Grade);
        Assert.Equal(0.4d, joint.WallUsed!.Value, 4);
        Assert.Equal(8000d, joint.Burst!.Value);
        Assert.Equal(664000d, joint.Tension!.Value);
        Assert.True(joint.Collapse is > 0d, "the collapse rating is computed at the measured wall");
        Assert.Contains(workspace.Rerate, row => row.Case == MetalLossRules.Burst && row.Unit == "psi");
        Assert.Contains(workspace.Rerate, row => row.Case == MetalLossRules.Collapse);
        Assert.Contains(workspace.Rerate, row => row.Case == MetalLossRules.Tension && row.Unit == "lbf");

        // The burst case at a 5 000 psi test: 8 000 / 1.25 = 6 400 allowed, so 5 000 / 6 400 = 0.78.
        var burst = workspace.Rerate.Single(row => row.Case == MetalLossRules.Burst);

        Assert.Equal(6400d, burst.Rating!.Value, 4);
        Assert.Equal(0.78125d, burst.Utilisation!.Value, 4);
        Assert.True(burst.Passes);

        Assert.NotNull(joint.Pass);
        Assert.Equal(workspace.Joints.Count(item => item.Pass == false), workspace.Failed);

        // AC-4: one inspection is not a basis.
        Assert.False(workspace.Life.HasBasis);
        Assert.Contains("no earlier caliper result", workspace.Life.Basis, StringComparison.Ordinal);
        Assert.Contains("Basis not recorded", workspace.Life.Describe(), StringComparison.Ordinal);

        // ---- AC-6: the result is recorded with the log, the fingers, the basis and the classes ----
        Assert.NotNull(workspace.ResultId);
        Assert.Equal(1, Count(database, "MFC_RESULT WHERE UWI = 'W-1'"));
        Assert.Equal(1, Count(database,
            "MFC_RESULT WHERE JOINT_SOURCE = 'intervals' AND WALL_BASIS = 'minimumWall' AND JOINT_COUNT = 1 AND " +
            "DAMAGE_CLASSES_JSON = '[]' AND FINGERS LIKE 'RAD1,%' AND LOG_REFERENCE LIKE '%caliper.las'"));
        // Every sample reads the same penetration, so the worst is recorded at the first one - 1 000 ft.
        Assert.Equal(1, Count(database,
            "MFC_RESULT WHERE ABS(WORST_PENETRATION - 0.2) < 0.0001 AND ABS(WORST_DEPTH - 1000) < 0.5"));
        Assert.Equal(1, Count(database, "MFC_JOINT WHERE JOINT_NUMBER = 1 AND GRADE = 'L80'"));
        Assert.Equal(1, Count(database,
            "MFC_JOINT WHERE ABS(MAX_PENETRATION - 0.2) < 0.0001 AND ABS(MIN_REMAINING_WALL - 0.4) < 0.0001 AND " +
            "ABS(NOMINAL_WALL - 0.5) < 0.0001 AND BURST = 8000 AND TENSION = 664000"));

        // ---- AC-4: a dated earlier inspection at the same depth is a basis ----
        Execute(database,
            "UPDATE MFC_RESULT SET WORST_PENETRATION = 0.15, WORST_DEPTH = 1000, CHECKED_AT_UTC = datetime('now', '-365 days') " +
            "WHERE UWI = 'W-1';");

        var classes = IntegrityToolbox.Settings.DamageClasses;
        var restored = classes.ToList();

        try
        {
            classes.Clear();
            classes.Add(new DamageClass { Name = "Severe", PenetrationFrom = 0.5 });

            var second = await service.AnalyseAsync("W-1", caliper, 10m, 5000m, "nadia");

            Assert.True(second.Success, second.Message + " :: " + string.Join(" | ", reporter.Failures));

            // 0.15 to 0.2 over a year is 0.05 of the wall a year; to the 50 % limit there are about six years left.
            Assert.True(second.Value!.Life.HasBasis, second.Value.Life.Basis);
            Assert.Equal(0.05d, second.Value.Life.RatePerYear!.Value, 4);
            Assert.Equal(6d, second.Value.Life.YearsRemaining!.Value, 1);
            Assert.Contains("projected to the 50 % damage limit", second.Value.Life.Basis, StringComparison.Ordinal);

            // A joint below the only class is still unclassified - the band was not lowered to fit.
            Assert.Null(second.Value.Joints.Single().DamageClass);

            // The second run becomes the earlier inspection of the third, so the rate has something to move from.
            Execute(database,
                "UPDATE MFC_RESULT SET WORST_PENETRATION = 0.18, CHECKED_AT_UTC = datetime('now', '-300 days') WHERE " +
                "MFC_RESULT_ID = (SELECT MFC_RESULT_ID FROM MFC_RESULT ORDER BY CHECKED_AT_UTC DESC LIMIT 1);");

            // ---- AC-2: a class that covers the penetration is the one the tool settings supply ----
            classes.Clear();
            classes.Add(new DamageClass { Name = "Moderate", PenetrationFrom = 0.1 });
            classes.Add(new DamageClass { Name = "Severe", PenetrationFrom = 0.5 });

            var third = await service.AnalyseAsync("W-1", caliper, 10m, 5000m, "omar");

            Assert.True(third.Success, third.Message + " :: " + string.Join(" | ", reporter.Failures));

            Assert.Equal("Moderate", third.Value!.Joints.Single().DamageClass);
            Assert.Contains("2 damage class(es) configured", third.Value.DamageClassNote, StringComparison.Ordinal);

            // The classes are recorded with the run, so a later change does not rewrite what this inspection concluded.
            Assert.Equal(1, Count(database,
                "MFC_RESULT WHERE DAMAGE_CLASSES_JSON LIKE '%Moderate%' AND WORST_DAMAGE_CLASS = 'Moderate'"));

            // The limit is now the lowest class (10 %), which the joint is already past: no life is claimed.
            Assert.True(third.Value.Life.RatePerYear is not null);
            Assert.Equal(0d, third.Value.Life.YearsRemaining);
            Assert.Contains("at or beyond", third.Value.Life.Basis, StringComparison.Ordinal);
        }
        finally
        {
            classes.Clear();
            classes.AddRange(restored);
        }

        // ---- the feature's states ----
        var noFingers = Path.Combine(root, "two-fingers.las");
        File.WriteAllText(noFingers, Caliper(radius: 3.1, fingers: 2));

        var uncovered = await service.AnalyseAsync("W-1", noFingers, 10m, 5000m, "sara");

        Assert.False(uncovered.Success);
        Assert.Equal(MetalLossRules.CodeUncovered, uncovered.Reference);

        var noWell = await service.AnalyseAsync(" ", caliper, 10m, 5000m, "sara");

        Assert.False(noWell.Success);
        Assert.Equal(MetalLossRules.CodeWellRequired, noWell.Reference);

        var noActor = await service.AnalyseAsync("W-1", caliper, 10m, 5000m, " ");

        Assert.False(noActor.Success);
        Assert.Equal(MetalLossRules.CodeActorRequired, noActor.Reference);

        var missing = await service.AnalyseAsync("W-1", Path.Combine(root, "nothing.las"), 10m, 5000m, "sara");

        Assert.False(missing.Success);
        Assert.Equal(MetalLossRules.CodeNoLog, missing.Reference);

        // A well with no tubular record cannot be measured against nominal pipe, which the record says.
        var otherWell = await service.LoadAsync("W-2");

        Assert.True(otherWell.Success, otherWell.Message);
        Assert.Empty(otherWell.Value!.Strings);
        Assert.Empty(otherWell.Value.Joints);
        Assert.Contains(otherWell.Value.Warnings, warning => warning.Contains("No casing or tubing", StringComparison.Ordinal));

        // A well with no result is a readable empty record, not a failure.
        var empty = await service.LoadAsync("W-1");
        Assert.True(empty.Success, empty.Message);

        var noWellId = await service.LoadAsync(" ");

        Assert.False(noWellId.Success);
        Assert.Equal(MetalLossRules.CodeWellRequired, noWellId.Reference);

        // ---- a failed read is a failure, not an empty record ----
        Execute(database, "DROP TABLE MFC_JOINT;");

        var failedRead = await service.LoadAsync("W-1");

        Assert.False(failedRead.Success);
        Assert.Null(failedRead.Value);
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>
    /// A LAS 2.0 multi-finger caliper log: <paramref name="fingers"/> radius curves, every one reading the same radius, so the
    /// penetration is exactly <c>(2 r - id) / (od - id)</c> for the seeded 7 in x 6 in pipe.
    /// </summary>
    private static string Caliper(double radius, int fingers)
    {
        var text = new System.Text.StringBuilder();

        text.AppendLine("~Version");
        text.AppendLine("VERS.   2.0 : CWLS LOG ASCII STANDARD - VERSION 2.0");
        text.AppendLine("WRAP.   NO");
        text.AppendLine("~Well");
        text.AppendLine("STRT.F  1000.0");
        text.AppendLine("STOP.F  1010.0");
        text.AppendLine("STEP.F  0.5");
        text.AppendLine("NULL.   -999.25");
        text.AppendLine("COMP.   Smoke Energy");
        text.AppendLine("WELL.   W-1");
        text.AppendLine("~Curve");
        text.AppendLine("DEPT.F  : Measured depth");
        text.AppendLine("CCL.A   : Casing collar locator");

        for (var finger = 1; finger <= fingers; finger++)
        {
            text.AppendLine("RAD" + finger.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".IN : Finger " +
                            finger.ToString(System.Globalization.CultureInfo.InvariantCulture) + " radius");
        }

        text.AppendLine("~Ascii");

        for (var depth = 1000d; depth <= 1010d + 1e-9; depth += 0.5d)
        {
            var row = new System.Text.StringBuilder();
            row.Append(depth.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            row.Append(" 0.0");

            for (var finger = 0; finger < fingers; finger++)
            {
                row.Append(' ');
                row.Append(radius.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture));
            }

            text.AppendLine(row.ToString());
        }

        return text.ToString();
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

            CREATE TABLE MFC_RESULT (
                MFC_RESULT_ID TEXT,
                UWI TEXT,
                LOG_NAME TEXT,
                LOG_REFERENCE TEXT,
                JOINT_SOURCE TEXT,
                FINGERS TEXT,
                JOINT_COUNT INTEGER,
                WALL_BASIS TEXT,
                DAMAGE_CLASSES_JSON TEXT,
                FLUID_GRADIENT_PPG REAL,
                TEST_PRESSURE_PSI REAL,
                WORST_JOINT TEXT,
                WORST_DEPTH REAL,
                WORST_PENETRATION REAL,
                WORST_DAMAGE_CLASS TEXT,
                FAILED_JOINTS INTEGER,
                DEPTH_OUOM TEXT,
                CHECKED_BY TEXT,
                CHECKED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (MFC_RESULT_ID));

            CREATE TABLE MFC_JOINT (
                MFC_JOINT_ID TEXT,
                MFC_RESULT_ID TEXT,
                UWI TEXT,
                JOINT_NUMBER INTEGER,
                TOP_DEPTH REAL,
                BOTTOM_DEPTH REAL,
                DEPTH_OUOM TEXT,
                MAX_PENETRATION REAL,
                PENETRATION_DEPTH REAL,
                MAX_METAL_LOSS REAL,
                MIN_REMAINING_WALL REAL,
                NOMINAL_WALL REAL,
                MAX_OVALITY REAL,
                MIN_ID REAL,
                DAMAGE_CLASS TEXT,
                STRING_NAME TEXT,
                GRADE TEXT,
                WALL_USED REAL,
                NOMINAL_BURST REAL,
                NOMINAL_COLLAPSE REAL,
                NOMINAL_TENSION REAL,
                BURST REAL,
                COLLAPSE REAL,
                COLLAPSE_REGIME TEXT,
                TENSION REAL,
                BURST_LOAD REAL,
                COLLAPSE_LOAD REAL,
                TENSION_LOAD REAL,
                PASSES TEXT,
                NOTES TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (MFC_JOINT_ID));

            INSERT INTO WELL_TUBULAR (UWI, SOURCE, TUBING_TYPE, TUBING_OBS_NO, OUTSIDE_DIAMETER,
                OUTSIDE_DIAMETER_OUOM, INSIDE_DIAMETER, INSIDE_DIAMETER_OUOM, TUBING_GRADE, TUBING_WEIGHT,
                TUBING_WEIGHT_OUOM, BASE_DEPTH, BASE_DEPTH_OUOM, SHOE_DEPTH, SHOE_DEPTH_OUOM, HUNG_TOP_DEPTH,
                HUNG_TOP_DEPTH_OUOM, ACTIVE_IND)
                VALUES ('W-1', 'PPDM', 'PRODUCTION', 1, 7, 'in', 6, 'in', 'L80', 47, 'lbm/ft', 1010, 'ft', 1010, 'ft',
                    1000, 'ft', 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, AZIMUTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'S1', 'PPDM', 1, 1000, 'ft', 0, 0, 'deg', 'Y'),
                ('W-1', 'S1', 'PPDM', 2, 1010, 'ft', 0, 0, 'deg', 'Y');
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
