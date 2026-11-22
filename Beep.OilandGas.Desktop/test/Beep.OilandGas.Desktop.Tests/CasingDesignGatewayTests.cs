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
/// ENG-02 against a real SQLite database: the API 5C3 ratings are computed from the recorded strings, the loads from the recorded
/// mud and test pressure, and a verdict is written per section per case (AC-1, AC-2); the governing case is named (AC-3); the
/// factors are the configured ones (AC-4); an unknown grade and an unresolved wall are refused (AC-5); and the run records the
/// grades, walls and factors (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class CasingDesignGatewayTests
{
    private const string ConnectionName = "eng02-casing";

    [IntegrationFact]
    public async Task A_casing_design_is_rated_per_section_and_case_and_the_run_is_recorded()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "casing.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "casing.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgCasingDesignService(() => source, reporter);

        // ---- the read: the sections come from the design's own outline ----
        var loaded = await service.LoadAsync("PD-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = loaded.Value!;

        Assert.Equal("North 1", workspace.WellName);
        Assert.Equal("ft", workspace.DepthUnit);
        Assert.Equal(2, workspace.Sections.Count);
        Assert.Empty(workspace.Verdicts);

        Assert.Equal("SURFACE", workspace.Sections[0].Name);
        Assert.Equal(20m, workspace.Sections[0].Od);
        Assert.Equal(0.438m, workspace.Sections[0].Wall);
        Assert.Equal("K55", workspace.Sections[0].Grade);
        Assert.Equal(1500m, workspace.Sections[0].Bottom);

        // The section whose top the outline never recorded is measured from surface, and that is said out loud.
        Assert.Contains(workspace.Warnings, warning => warning.Contains("records no top", StringComparison.Ordinal));

        // ---- AC-4: the factors are the configured ones, printed rather than edited ----
        Assert.Equal(1.25m, workspace.Factors.Burst);
        Assert.Equal(1.1m, workspace.Factors.Collapse);
        Assert.Equal(1.4m, workspace.Factors.Tension);
        Assert.Contains("from configuration", workspace.Factors.Describe(), StringComparison.Ordinal);

        // ---- AC-1, AC-2 and AC-3: the check rates every section against every case ----
        var failed = await service.CheckAsync("PD-1", 10m, 5000m, 0m, "sara");

        Assert.True(failed.Success, failed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = failed.Value!;

        Assert.Equal(6, result.Verdicts.Count);
        Assert.Equal("FAIL", result.Outcome);
        Assert.False(result.Passed);

        // The 9 5/8 47 lb/ft L80 string: burst 6 870 psi, collapse 4 760 psi, tension 1 086 000 lbf.
        var production = result.Verdicts.Where(verdict => verdict.Section == "PRODUCTION").ToList();

        Assert.Equal(3, production.Count);

        var burst = production.Single(verdict => verdict.LoadCase == CasingDesignRules.Burst);
        var collapse = production.Single(verdict => verdict.LoadCase == CasingDesignRules.Collapse);
        var tension = production.Single(verdict => verdict.LoadCase == CasingDesignRules.Tension);

        Assert.Equal(6870m, burst.Resistance);
        Assert.Equal(4760m, collapse.Resistance);
        Assert.Equal(1086000m, tension.Resistance);

        // The loads are the load case: the test pressure, the full-evacuation gradient at the shoe, the buoyed weight.
        Assert.Equal(5000m, burst.Load);
        Assert.Equal(5194.8m, collapse.Load);
        Assert.Equal(1.25m, burst.RequiredFactor);
        Assert.Equal(1.1m, collapse.RequiredFactor);
        Assert.Equal(1.4m, tension.RequiredFactor);

        // AC-2: the utilisation is load times factor over resistance, and it decides the verdict.
        Assert.Equal(6250m / 6870m, burst.Utilisation, 6);
        Assert.Equal(5194.8m * 1.1m / 4760m, collapse.Utilisation, 4);
        Assert.True(burst.Passes);
        Assert.False(collapse.Passes);

        // AC-3: the governing case is named, and it is the 20 in conductor's burst at a 5 000 psi test.
        Assert.Equal("SURFACE · BURST", result.GoverningCase);
        Assert.True(result.GoverningUtilisation > 2.9m && result.GoverningUtilisation < 3.0m,
            result.GoverningText);

        Assert.Contains("Governing case: SURFACE · BURST", result.GoverningText, StringComparison.Ordinal);
        Assert.Equal(3, result.Failed);

        // ---- AC-6: the run records the inputs, the factors and the verdicts ----
        Assert.Equal(1, Count(database,
            "CASING_DESIGN_RUN WHERE DESIGN_ID = 'PD-1' AND OUTCOME = 'FAIL' AND SECTIONS_CHECKED = 2 AND " +
            "FLUID_GRADIENT_PPG = 10 AND TEST_PRESSURE_PSI = 5000 AND CHECKED_BY = 'sara' AND " +
            "DESIGN_FACTORS_JSON LIKE '%\"Burst\":1.25%' AND GOVERNING_CASE = 'SURFACE · BURST'"));

        Assert.Equal(6, Count(database, "CASING_DESIGN_VERDICT WHERE LOAD_CASE = 'BURST' OR LOAD_CASE = 'COLLAPSE' OR LOAD_CASE = 'TENSION'"));
        Assert.Equal(2, Count(database, "CASING_DESIGN_VERDICT WHERE LOAD_CASE = 'BURST' AND REQUIRED_FACTOR = 1.25"));
        Assert.Equal(2, Count(database, "CASING_DESIGN_VERDICT WHERE LOAD_CASE = 'COLLAPSE' AND REQUIRED_FACTOR = 1.1 AND PASSES = 'N'"));
        Assert.Equal(2, Count(database, "CASING_DESIGN_VERDICT WHERE LOAD_CASE = 'TENSION' AND REQUIRED_FACTOR = 1.4 AND PASSES = 'Y'"));

        // The walls and grades the verdicts were computed from are on the design's own rows (AC-6).
        Assert.Equal(1, Count(database,
            "DEVELOPMENT_CASING_STRING WHERE CASING_STRING_ID = 'CS-2' AND GRADE = 'L80' AND WALL_THICKNESS = 0.472"));

        // ---- a second run that passes, which proves the verdicts follow the load case ----
        var passed = await service.CheckAsync("PD-1", 3m, 1000m, 0m, "nadia");

        Assert.True(passed.Success, passed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal("PASS", passed.Value!.Outcome);
        Assert.Equal(0, passed.Value.Failed);
        Assert.Equal(2, passed.Value.Verdicts.Count(verdict => verdict.LoadCase == CasingDesignRules.Collapse));

        Assert.Equal(2, Count(database, "CASING_DESIGN_RUN"));

        // The design read back shows the most recent run, not the first.
        var latest = (await service.LoadAsync("PD-1")).Value!;

        Assert.Equal(passed.Value.RunId, latest.RunId);
        Assert.Equal("PASS", latest.Outcome);
        Assert.Equal(6, latest.Verdicts.Count);

        // ---- the feature's states ----
        var noLoad = await service.CheckAsync("PD-1", 0m, 0m, 0m, "sara");

        Assert.False(noLoad.Success);
        Assert.Equal(CasingDesignRules.CodeNoLoadCase, noLoad.Reference);

        var noActor = await service.CheckAsync("PD-1", 10m, 5000m, 0m, " ");

        Assert.False(noActor.Success);
        Assert.Equal(CasingDesignRules.CodeActorRequired, noActor.Reference);

        // AC-5: a grade the configuration does not carry is refused, naming it, and no run is written.
        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET GRADE = 'X999' WHERE CASING_STRING_ID = 'CS-2';");

        var unknown = await service.CheckAsync("PD-1", 10m, 5000m, 0m, "sara");

        Assert.False(unknown.Success);
        Assert.Equal(CasingDesignRules.CodeUnknownGrade, unknown.Reference);
        Assert.Contains("X999", unknown.Message!, StringComparison.Ordinal);
        Assert.Equal(2, Count(database, "CASING_DESIGN_RUN"));

        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET GRADE = 'L80' WHERE CASING_STRING_ID = 'CS-2';");

        // A missing wall, with a weight the catalogue does not know, is refused rather than guessed.
        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET WALL_THICKNESS = NULL, WEIGHT = 51.1 WHERE CASING_STRING_ID = 'CS-1';");

        var noWall = await service.CheckAsync("PD-1", 10m, 5000m, 0m, "sara");

        Assert.False(noWall.Success);
        Assert.Equal(CasingDesignRules.CodeNoWall, noWall.Reference);
        Assert.Contains("SURFACE", noWall.Message!, StringComparison.Ordinal);

        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET WALL_THICKNESS = 0.438, WEIGHT = 94 WHERE CASING_STRING_ID = 'CS-1';");

        // Mixed units are refused with both units named.
        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET DEPTH_OUOM = 'm' WHERE CASING_STRING_ID = 'CS-2';");

        var mixed = await service.CheckAsync("PD-1", 10m, 5000m, 0m, "sara");

        Assert.False(mixed.Success);
        Assert.Equal(CasingDesignRules.CodeMixedUnits, mixed.Reference);
        Assert.Contains("'ft'", mixed.Message!, StringComparison.Ordinal);
        Assert.Contains("'m'", mixed.Message!, StringComparison.Ordinal);

        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET DEPTH_OUOM = 'ft' WHERE CASING_STRING_ID = 'CS-2';");

        // A section with no length is refused.
        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET TOP_DEPTH = 2000 WHERE CASING_STRING_ID = 'CS-1';");

        var inverted = await service.CheckAsync("PD-1", 10m, 5000m, 0m, "sara");

        Assert.False(inverted.Success);
        Assert.Equal(CasingDesignRules.CodeSectionInvalid, inverted.Reference);

        Execute(database, "UPDATE DEVELOPMENT_CASING_STRING SET TOP_DEPTH = NULL WHERE CASING_STRING_ID = 'CS-1';");

        // ---- AC-6: recording a section records the wall and top the rating needs ----
        var added = await service.AddSectionAsync(
            "PD-1", "intermediate", 13.375m, 0.5m, 68m, "K55", 0m, 5000m, "ft", "nadia");

        Assert.True(added.Success, added.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(3, added.Value!.Sections.Count);
        Assert.Equal(3, added.Value.Sections[2].Order);
        Assert.Equal(0.5m, added.Value.Sections[2].Wall);

        Assert.Equal(1, Count(database,
            "DEVELOPMENT_CASING_STRING WHERE STRING_NAME = 'INTERMEDIATE' AND WALL_THICKNESS = 0.5 AND TOP_DEPTH = 0 AND GRADE = 'K55'"));

        // The path the design does not exist on is a refusal, not an empty design.
        var unknownDesign = await service.LoadAsync("PD-404");

        Assert.False(unknownDesign.Success);
        Assert.Equal(CasingDesignRules.CodeDesignUnknown, unknownDesign.Reference);

        // ---- a failed read is a failure, not an empty design ----
        Execute(database, "DROP TABLE CASING_DESIGN_VERDICT;");

        var failedRead = await service.LoadAsync("PD-1");

        Assert.False(failedRead.Success);
        Assert.Null(failedRead.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE DEVELOPMENT_WELL_DESIGN (
                DESIGN_ID TEXT,
                SCHEDULE_ID TEXT,
                FDP_ID TEXT,
                FIELD_ID TEXT,
                PLANNED_WELL_ID TEXT,
                WELL_NAME TEXT,
                VERSION INTEGER,
                CURRENT_IND TEXT,
                DESIGN_STATE TEXT,
                OBJECTIVE TEXT,
                TARGET_FORMATION TEXT,
                TARGET_DEPTH REAL,
                DEPTH_OUOM TEXT,
                TRAJECTORY_INTENT TEXT,
                COMPLETION_CONCEPT TEXT,
                RISK_TEXT TEXT,
                ESTIMATED_COST REAL,
                COST_CURRENCY TEXT,
                SPUD_WINDOW_START TEXT,
                SPUD_WINDOW_END TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                REVISION_REASON TEXT,
                ACTOR TEXT,
                RECORDED_AT TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DESIGN_ID));

            CREATE TABLE DEVELOPMENT_CASING_STRING (
                WALL_THICKNESS REAL,
                TOP_DEPTH REAL,
                CASING_STRING_ID TEXT,
                DESIGN_ID TEXT,
                STRING_ORDER INTEGER,
                STRING_NAME TEXT,
                OD REAL,
                WEIGHT REAL,
                GRADE TEXT,
                SET_DEPTH REAL,
                DEPTH_OUOM TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (CASING_STRING_ID));

            CREATE TABLE CASING_DESIGN_RUN (
                RUN_ID TEXT,
                DESIGN_ID TEXT,
                FLUID_GRADIENT_PPG REAL,
                TEST_PRESSURE_PSI REAL,
                OVERPULL_LBF REAL,
                DESIGN_FACTORS_JSON TEXT,
                GOVERNING_CASE TEXT,
                GOVERNING_UTILISATION REAL,
                OUTCOME TEXT,
                SECTIONS_CHECKED INTEGER,
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
                PRIMARY KEY (RUN_ID));

            CREATE TABLE CASING_DESIGN_VERDICT (
                VERDICT_ID TEXT,
                RUN_ID TEXT,
                DESIGN_ID TEXT,
                CASING_STRING_ID TEXT,
                SECTION_NAME TEXT,
                LOAD_CASE TEXT,
                LOAD REAL,
                RESISTANCE REAL,
                UTILISATION REAL,
                REQUIRED_FACTOR REAL,
                PASSES TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (VERDICT_ID));

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

            INSERT INTO DEVELOPMENT_WELL_DESIGN (DESIGN_ID, SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID,
                WELL_NAME, VERSION, CURRENT_IND, DESIGN_STATE, TARGET_DEPTH, DEPTH_OUOM, ESTIMATED_COST)
                VALUES ('PD-1', 'S-1', 'FDP-1', 'F-1', 'W-1', 'North 1', 1, 'Y', 'DRAFT', 10000, 'ft', 0);

            INSERT INTO DEVELOPMENT_CASING_STRING (CASING_STRING_ID, DESIGN_ID, STRING_ORDER, STRING_NAME, OD, WEIGHT,
                GRADE, SET_DEPTH, DEPTH_OUOM, WALL_THICKNESS, TOP_DEPTH) VALUES
                ('CS-1', 'PD-1', 1, 'SURFACE', 20, 94, 'K55', 1500, 'ft', 0.438, NULL),
                ('CS-2', 'PD-1', 2, 'PRODUCTION', 9.625, 47, 'L80', 10000, 'ft', 0.472, 0);

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, AZIMUTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'S1', 'PPDM', 1, 0, 'ft', 0, 0, 'deg', 'Y'),
                ('W-1', 'S1', 'PPDM', 2, 10000, 'ft', 0, 0, 'deg', 'Y');
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
