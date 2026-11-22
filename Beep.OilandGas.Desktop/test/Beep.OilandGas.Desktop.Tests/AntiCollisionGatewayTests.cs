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
/// ENG-01 against a real SQLite database: the surveys are read from the recorded stations, the scan produces separation rows
/// (AC-2, AC-4), an offset with no survey is excluded and named (AC-3), the export carries the same rows (AC-5), and the saved run
/// records the error model, the sigma and the environment (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class AntiCollisionGatewayTests
{
    private const string ConnectionName = "eng01-anticollision";

    [IntegrationFact]
    public async Task A_scan_reads_the_surveys_and_saves_its_run()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "anticollision.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "anticollision.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var scan = new OgAntiCollision(() => source, reporter);

        var scanned = await scan.ScanAsync("REF-1", ["OFF-1", "OFF-2"]);

        Assert.True(scanned.Success, scanned.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = scanned.Value!;

        // ---- AC-3: the offset with no survey is excluded and named ----
        Assert.Equal(["OFF-1"], result.Offsets);
        Assert.Equal(["OFF-2"], result.Excluded);
        Assert.Contains("OFF-2", result.ExcludedText, StringComparison.Ordinal);

        // ---- AC-2 and AC-4: rows, the rule and its displayed levels ----
        Assert.NotEmpty(result.Rows);
        // The library names each offset by its well header, while the included list carries the identity that was scanned.
        Assert.All(result.Rows, row => Assert.Equal("Offset 1", row.Offset));
        Assert.All(result.Rows, row => Assert.True(row.SeparationFactor > 0));

        Assert.Contains("SF = (D - Rref - Roff - Sm)", result.Rule.Describe(), StringComparison.Ordinal);
        Assert.Contains("Stop below 1", string.Join(" · ", result.Rule.Levels), StringComparison.Ordinal);

        // The count is the number of points at the configured level, computed from the same rows.
        Assert.Equal(result.Rows.Count(row => row.IsViolation), result.Violations);
        Assert.Equal(result.Rows.Select(row => row.Md).Distinct().Count(), result.Stations);

        // A close well at this depth is a violation, which is the reason the scan exists.
        Assert.True(result.Violations > 0, result.RowsText);

        // ---- AC-5: the export is the rows ----
        var csv = result.Csv();
        var lines = csv.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(1 + result.Rows.Count, lines.Length);
        Assert.Contains(OgAntiCollisionScan.Figure(result.Rows[0].SeparationFactor), lines[1], StringComparison.Ordinal);

        // ---- AC-6: the run is recorded with its model, sigma and environment ----
        var saved = await scan.SaveRunAsync(result, "sara");

        Assert.True(saved.Success, saved.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.False(string.IsNullOrWhiteSpace(saved.Value));


        Assert.Equal(1, Count(database,
            "ANTICOLLISION_RUN WHERE REFERENCE_WELL = 'REF-1' AND EXCLUDED_WELLS = 'OFF-2' AND RUN_BY = 'sara'"));
        Assert.Equal(1, Count(database,
            "ANTICOLLISION_RUN WHERE ERROR_MODEL <> '' AND SIGMA = 1 AND ENVIRONMENT <> '' AND SETTINGS_JSON LIKE '%ScalingFactor%'"));

        var stored = Scalar(database,
            "SELECT VIOLATION_COUNT || '|' || STATION_COUNT FROM ANTICOLLISION_RUN WHERE REFERENCE_WELL = 'REF-1';");

        Assert.Equal(result.Violations + "|" + result.Stations, stored);

        // A run without an actor is refused.
        var anonymous = await scan.SaveRunAsync(result, " ");

        Assert.False(anonymous.Success);
        Assert.Equal(AntiCollisionRules.CodeActorRequired, anonymous.Reference);

        // ---- a reference with no survey is refused, not drawn empty ----
        var bare = await scan.ScanAsync("REF-NONE", ["OFF-1"]);

        Assert.False(bare.Success);
        Assert.Equal(AntiCollisionRules.CodeNoSurvey, bare.Reference);

        // Every offset without a survey is refused with the names.
        var noneOffsets = await scan.ScanAsync("REF-1", ["OFF-2"]);

        Assert.False(noneOffsets.Success);
        Assert.Contains("OFF-2", noneOffsets.Message!, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
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

            CREATE TABLE WELL_DIR_SRVY (
                UWI TEXT,
                SURVEY_ID TEXT,
                SOURCE TEXT,
                ACTIVE_IND TEXT,
                AZIMUTH_COORD_SYS_ID TEXT,
                AZIMUTH_COORD_SYS_QUALIFIER TEXT,
                AZIMUTH_COORD_SYS_REMARK TEXT,
                AZIMUTH_CORR_ANGLE REAL,
                AZIMUTH_CORR_ANGLE_OUOM TEXT,
                AZIMUTH_CORR_ANGLE_TYPE TEXT,
                AZIMUTH_NORTH_TYPE TEXT,
                BASE_DEPTH REAL,
                BASE_DEPTH_OUOM TEXT,
                COMPUTE_METHOD TEXT,
                CONFIDENTIAL_TYPE TEXT,
                COORD_ACQUISITION_ID TEXT,
                DIR_SURVEY_CLASS TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                EXTRAPOLATE_DEPTH REAL,
                EXTRAPOLATE_DEPTH_OUOM TEXT,
                EXTRAPOLATE_IND TEXT,
                GEOG_COORD_SYS_ID TEXT,
                LOCAL_COORD_SYS_ID TEXT,
                MAGNETIC_DECLINATION REAL,
                MAGNETIC_DECLINATION_OUOM TEXT,
                MAG_DECL_EW_DIRECTION TEXT,
                OFFSET_COORD_SYS_ID TEXT,
                OFFSET_COORD_SYS_QUALIFIER TEXT,
                OFFSET_COORD_SYS_REMARK TEXT,
                OFFSET_NORTH_TYPE TEXT,
                PLANE_OF_PROPOSAL REAL,
                PLANE_OF_PROPOSAL_OUOM TEXT,
                PPDM_GUID TEXT,
                PREFERRED_IND TEXT,
                RAD_UNCERT_DIST REAL,
                RAD_UNCERT_DIST_OUOM TEXT,
                RAD_UNCERT_DIST_REASON TEXT,
                RECORD_MODE TEXT,
                REMARK TEXT,
                REPORT_APD REAL,
                REPORT_LOG_DATUM TEXT,
                REPORT_LOG_DATUM_ELEV REAL,
                REPORT_LOG_DATUM_ELEV_OUOM TEXT,
                REPORT_PERM_DATUM TEXT,
                REPORT_PERM_DATUM_ELEV REAL,
                REPORT_PERM_DATUM_ELEV_OUOM TEXT,
                RESAMPLE_INTERVAL REAL,
                RESAMPLE_INTERVAL_OUOM TEXT,
                RPT_COORD_SYS_ID TEXT,
                RPT_COORD_SYS_QUALIFIER TEXT,
                RPT_COORD_SYS_REMARK TEXT,
                RPT_CORR_ANGLE REAL,
                RPT_CORR_ANGLE_OUOM TEXT,
                RPT_CORR_ANGLE_TYPE TEXT,
                RPT_NORTH_TYPE TEXT,
                RPT_SURVEY_TYPE TEXT,
                RPT_TIE_AZIMUTH REAL,
                RPT_TIE_AZIMUTH_OUOM TEXT,
                RPT_TIE_INCLINATION REAL,
                RPT_TIE_INCLINATION_OUOM TEXT,
                RPT_TIE_MD REAL,
                RPT_TIE_MD_OUOM TEXT,
                RPT_TIE_POINT_IND TEXT,
                RPT_TIE_TVD REAL,
                RPT_TIE_TVD_OUOM TEXT,
                RPT_TIE_X_OFFSET REAL,
                RPT_TIE_X_OFFSET_EW_DIR TEXT,
                RPT_TIE_X_OFFSET_OUOM TEXT,
                RPT_TIE_Y_OFFSET REAL,
                RPT_TIE_Y_OFFSET_NS_DIR TEXT,
                RPT_TIE_Y_OFFSET_OUOM TEXT,
                SOURCE_DOCUMENT_ID TEXT,
                SURVEY_COMPANY_BA_ID TEXT,
                SURVEY_COMPILER_BA_ID TEXT,
                SURVEY_DIGITAL_PRVDR_BA_ID TEXT,
                SURVEY_END_DATE TEXT,
                SURVEY_END_DATE_QUALIFIER TEXT,
                SURVEY_PROCESS_METHOD TEXT,
                SURVEY_QUALITY TEXT,
                SURVEY_REF_NUM TEXT,
                SURVEY_REPORT_TYPE TEXT,
                SURVEY_RUN_NUM TEXT,
                SURVEY_START_DATE TEXT,
                SURVEY_START_DATE_QUALIFIER TEXT,
                SURVEY_TYPE TEXT,
                TOP_DEPTH REAL,
                TOP_DEPTH_OUOM TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (UWI, SURVEY_ID, SOURCE));

            CREATE TABLE WELL (
                WELL_ID TEXT,
                FIELD_ID TEXT,
                WELL_TYPE TEXT,
                WELL_PURPOSE TEXT,
                UWI TEXT,
                ABANDONMENT_DATE TEXT,
                ACTIVE_IND TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                ASSIGNED_FIELD TEXT,
                BASE_DEPTH REAL,
                BASE_DEPTH_OUOM TEXT,
                BASE_NODE_ID TEXT,
                BASE_STRAT_NAME_SET_ID TEXT,
                BASE_STRAT_UNIT_ID TEXT,
                BOTTOM_HOLE_LATITUDE REAL,
                BOTTOM_HOLE_LONGITUDE REAL,
                CASING_FLANGE_ELEV REAL,
                CASING_FLANGE_ELEV_OUOM TEXT,
                COMPLETION_DATE TEXT,
                CONFIDENTIAL_DATE TEXT,
                CONFIDENTIAL_DEPTH REAL,
                CONFIDENTIAL_DEPTH_OUOM TEXT,
                CONFIDENTIAL_TYPE TEXT,
                CONFID_STRAT_NAME_SET_ID TEXT,
                CONFID_STRAT_UNIT_ID TEXT,
                CURRENT_CLASS TEXT,
                CURRENT_STATUS TEXT,
                CURRENT_STATUS_DATE TEXT,
                DEEPEST_DEPTH REAL,
                DEEPEST_DEPTH_OUOM TEXT,
                DEPTH_DATUM TEXT,
                DEPTH_DATUM_ELEV REAL,
                DEPTH_DATUM_ELEV_OUOM TEXT,
                DERRICK_FLOOR_ELEV REAL,
                DERRICK_FLOOR_ELEV_OUOM TEXT,
                DIFFERENCE_LAT_MSL REAL,
                DISCOVERY_IND TEXT,
                DRILL_TD REAL,
                DRILL_TD_OUOM TEXT,
                EFFECTIVE_DATE TEXT,
                ELEV_REF_DATUM TEXT,
                ENVIRONMENT_TYPE TEXT,
                EXPIRY_DATE TEXT,
                FAULTED_IND TEXT,
                FINAL_DRILL_DATE TEXT,
                FINAL_TD REAL,
                FINAL_TD_OUOM TEXT,
                GROUND_ELEV REAL,
                GROUND_ELEV_OUOM TEXT,
                GROUND_ELEV_TYPE TEXT,
                INITIAL_CLASS TEXT,
                KB_ELEV REAL,
                KB_ELEV_OUOM TEXT,
                LEASE_NAME TEXT,
                LEASE_NUM TEXT,
                LEGAL_SURVEY_TYPE TEXT,
                LOCATION_TYPE TEXT,
                LOG_TD REAL,
                LOG_TD_OUOM TEXT,
                MAX_TVD REAL,
                MAX_TVD_OUOM TEXT,
                NET_PAY REAL,
                NET_PAY_OUOM TEXT,
                OLDEST_STRAT_AGE TEXT,
                OLDEST_STRAT_NAME_SET_AGE TEXT,
                OLDEST_STRAT_NAME_SET_ID TEXT,
                OLDEST_STRAT_UNIT_ID TEXT,
                OPERATOR TEXT,
                PLATFORM_ID TEXT,
                PLATFORM_SF_SUBTYPE TEXT,
                PLOT_NAME TEXT,
                PLOT_SYMBOL TEXT,
                PLUGBACK_DEPTH REAL,
                PLUGBACK_DEPTH_OUOM TEXT,
                PPDM_GUID TEXT,
                PRIMARY_SOURCE TEXT,
                PROFILE_TYPE TEXT,
                REGULATORY_AGENCY TEXT,
                REMARK TEXT,
                RIG_ON_SITE_DATE TEXT,
                RIG_RELEASE_DATE TEXT,
                ROTARY_TABLE_ELEV REAL,
                ROTARY_TABLE_ELEV_OUOM TEXT,
                SOURCE_DOCUMENT_ID TEXT,
                SPUD_DATE TEXT,
                STATUS_TYPE TEXT,
                SUBSEA_ELEV_REF_TYPE TEXT,
                SURFACE_LATITUDE REAL,
                SURFACE_LONGITUDE REAL,
                SURFACE_NODE_ID TEXT,
                TAX_CREDIT_CODE TEXT,
                TD_STRAT_AGE TEXT,
                TD_STRAT_NAME_SET_AGE TEXT,
                TD_STRAT_NAME_SET_ID TEXT,
                TD_STRAT_UNIT_ID TEXT,
                TOP_DEPTH REAL,
                TOP_DEPTH_OUOM TEXT,
                TOP_STRAT_NAME_SET_ID TEXT,
                TOP_STRAT_UNIT_ID TEXT,
                WATER_ACOUSTIC_VEL REAL,
                WATER_ACOUSTIC_VEL_OUOM TEXT,
                WATER_DEPTH REAL,
                WATER_DEPTH_DATUM TEXT,
                WATER_DEPTH_OUOM TEXT,
                WELL_EVENT_NUM TEXT,
                WELL_GOVERNMENT_ID TEXT,
                WELL_INTERSECT_MD REAL,
                WELL_LEVEL_TYPE TEXT,
                WELL_NAME TEXT,
                WELL_NUM TEXT,
                WHIPSTOCK_DEPTH REAL,
                WHIPSTOCK_DEPTH_OUOM TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (UWI));

            CREATE TABLE WELLBORE (
                UWI TEXT,
                WELLBORE_ID TEXT,
                WELLBORE_NAME TEXT,
                PARENT_WELLBORE_ID TEXT,
                KIND TEXT,
                STATUS TEXT,
                SPUD_DATE TEXT,
                TOTAL_DEPTH REAL,
                TOTAL_DEPTH_OUOM TEXT,
                SURFACE_LATITUDE REAL,
                SURFACE_LONGITUDE REAL,
                DEPTH_DATUM TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (UWI, WELLBORE_ID));

            CREATE TABLE ANTICOLLISION_RUN (
                ANTICOLLISION_RUN_ID TEXT,
                REFERENCE_WELL TEXT,
                OFFSET_WELLS TEXT,
                EXCLUDED_WELLS TEXT,
                ERROR_MODEL TEXT,
                SIGMA REAL,
                SCALING_FACTOR REAL,
                SURFACE_MARGIN REAL,
                PROJECT_AHEAD REAL,
                SCAN_STEP REAL,
                ENVIRONMENT TEXT,
                STATION_COUNT INTEGER,
                VIOLATION_COUNT INTEGER,
                SETTINGS_JSON TEXT,
                RUN_BY TEXT,
                RUN_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ANTICOLLISION_RUN_ID));

            INSERT INTO WELL (UWI, WELL_NAME, ACTIVE_IND) VALUES
                ('REF-1', 'Reference 1', 'Y'),
                ('OFF-1', 'Offset 1', 'Y'),
                ('OFF-2', 'Offset 2 - no survey', 'Y'),
                ('REF-NONE', 'Reference without survey', 'Y');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, TOTAL_DEPTH, TOTAL_DEPTH_OUOM,
                SURFACE_LATITUDE, SURFACE_LONGITUDE, DEPTH_DATUM) VALUES
                ('REF-1', 'WB-R', 'Reference main bore', 1000, 'm', 26.0, 50.0, 'KB'),
                ('OFF-1', 'WB-O', 'Offset main bore', 1000, 'm', 26.00002, 50.0, 'KB');

            INSERT INTO WELL_DIR_SRVY (UWI, SURVEY_ID, SOURCE, AZIMUTH_NORTH_TYPE, ACTIVE_IND) VALUES
                ('REF-1', 'S1', 'PPDM', 'GRID', 'Y'),
                ('OFF-1', 'S1', 'PPDM', 'GRID', 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, AZIMUTH_OUOM, ACTIVE_IND) VALUES
                ('REF-1', 'S1', 'PPDM', 1, 0, 'm', 0, 0, 'deg', 'Y'),
                ('REF-1', 'S1', 'PPDM', 2, 500, 'm', 0, 0, 'deg', 'Y'),
                ('REF-1', 'S1', 'PPDM', 3, 1000, 'm', 0, 0, 'deg', 'Y'),
                ('OFF-1', 'S1', 'PPDM', 1, 0, 'm', 0, 0, 'deg', 'Y'),
                ('OFF-1', 'S1', 'PPDM', 2, 500, 'm', 0, 0, 'deg', 'Y'),
                ('OFF-1', 'S1', 'PPDM', 3, 1000, 'm', 0, 0, 'deg', 'Y');
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

    private static string Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
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
