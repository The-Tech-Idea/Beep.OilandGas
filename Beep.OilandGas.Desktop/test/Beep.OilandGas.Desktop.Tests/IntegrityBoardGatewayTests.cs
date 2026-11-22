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
/// ENG-05 against a real SQLite database: the categories are the owner's guideline-117 assessment of the recorded barrier set and
/// the annulus and wall-loss evidence, with an unassessed well distinct from a green one (AC-1); the order is the owner's
/// worst-first rule (AC-2); the counts are the owner's (AC-3); every finding names its input (AC-4); the CSV matches the board
/// (AC-5); and a failed read returns no board at all (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class IntegrityBoardGatewayTests
{
    private const string ConnectionName = "eng05-integrity-board";

    [IntegrationFact]
    public async Task The_field_is_boarded_worst_first_and_assessments_are_recorded()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "integrity-board.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "integrity-board.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgIntegrityBoardService(() => source, reporter);

        // ---- AC-2, AC-3 and AC-4: the field, worst first, with the findings' inputs named ----
        var loaded = await service.LoadAsync("North", null, null);

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var board = loaded.Value!;

        Assert.Equal(3, board.Scoped);
        Assert.Equal(3, board.Wells.Count);
        Assert.Equal(["W-3", "W-2", "W-1"], board.Wells.Select(well => well.WellId));

        // AC-1: every well is unassessed until an assessment is recorded - and each still shows its own category.
        Assert.All(board.Wells, well => Assert.True(well.Unassessed));
        Assert.All(board.Wells, well => Assert.StartsWith("unassessed", well.StateText, StringComparison.Ordinal));

        var unbarrured = board.Wells.Single(well => well.WellId == "W-3");

        Assert.Equal("Red", unbarrured.Category);
        Assert.False(unbarrured.Compliant);
        Assert.Contains(unbarrured.Findings, finding => finding.Source == IntegrityBoardRules.BarrierSource);
        Assert.Contains("no barrier set", string.Join(" ", unbarrured.Reasons), StringComparison.Ordinal);

        var pressured = board.Wells.Single(well => well.WellId == "W-2");

        Assert.Equal("Orange", pressured.Category);
        Assert.Equal(1, pressured.AnnuliAssessed);
        Assert.Contains(pressured.Findings, finding =>
            finding.Source == IntegrityBoardRules.CasingPressureSource &&
            finding.Text.Contains("A", StringComparison.Ordinal));

        var healthy = board.Wells.Single(well => well.WellId == "W-1");

        Assert.Equal("Green", healthy.Category);
        Assert.True(healthy.Compliant);
        Assert.Contains("no casing-pressure test", healthy.MissingInputs);
        Assert.Contains("partial inputs", healthy.StateText, StringComparison.Ordinal);

        // AC-3: the counts are the owner's over the field, and they add up.
        Assert.Equal(3, board.Counts.Sum(count => count.Count));
        Assert.Equal(1, board.Counts.Single(count => count.Category == "Green").Count);
        Assert.Equal(0, board.Counts.Single(count => count.Category == "Yellow").Count);
        Assert.Equal(1, board.Counts.Single(count => count.Category == "Orange").Count);
        Assert.Equal(1, board.Counts.Single(count => count.Category == "Red").Count);

        // AC-5: the export is the owner's, over the same wells, in the same order.
        var lines = board.Csv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("well,category,compliant,reasons", lines[0]);
        Assert.Equal(4, lines.Length);
        Assert.StartsWith("W-3,Red,no,", lines[1], StringComparison.Ordinal);
        Assert.StartsWith("W-2,Orange,no,", lines[2], StringComparison.Ordinal);
        Assert.StartsWith("W-1,Green,yes,", lines[3], StringComparison.Ordinal);
        Assert.Equal(board.Wells.Count, board.Board.Count);

        // ---- the South field is boarded on its own, and its corroded joint names the string ----
        var south = await service.LoadAsync("South", null, null);

        Assert.True(south.Success, south.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Single(south.Value!.Wells);

        var corroded = south.Value.Wells.Single();

        Assert.Equal("W-4", corroded.WellId);
        Assert.Equal("Yellow", corroded.Category);
        Assert.Equal(1, corroded.FailingJoints);
        Assert.Contains(corroded.Findings, finding =>
            finding.Source == IntegrityBoardRules.MetalLossSource &&
            finding.Text.Contains("TUBING", StringComparison.Ordinal));

        // ---- the filters ----
        var redOnly = await service.LoadAsync("North", "Red", null);

        Assert.True(redOnly.Success, redOnly.Message);
        Assert.Single(redOnly.Value!.Wells);
        Assert.Equal("W-3", redOnly.Value.Wells[0].WellId);
        Assert.Equal(3, redOnly.Value.Scoped);

        var unknown = await service.LoadAsync("North", "Purple", null);

        Assert.False(unknown.Success);
        Assert.Equal(IntegrityBoardRules.CodeCategoryUnknown, unknown.Reference);

        // ---- AC-1 again: once recorded, the same wells are assessed and say so ----
        var recorded = await service.RecordAsync("North", null, null, "sara");

        Assert.True(recorded.Success, recorded.Message + " :: " + string.Join(" | ", reporter.Failures));

        Assert.All(recorded.Value!.Wells, well => Assert.False(well.Unassessed));
        Assert.All(recorded.Value.Wells, well => Assert.NotNull(well.AssessedAt));
        Assert.All(recorded.Value.Wells, well => Assert.Equal("sara", well.AssessedBy));
        Assert.Contains(recorded.Value.Wells, well =>
            well.WellId == "W-1" && well.StateText.Contains("green", StringComparison.Ordinal));

        Assert.Equal(3, Count(database, "INTEGRITY_ASSESSMENT WHERE ASSESSED_BY = 'sara'"));
        Assert.Equal(1, Count(database,
            "INTEGRITY_ASSESSMENT WHERE UWI = 'W-2' AND CATEGORY = 'Orange' AND COMPLIANT = 'N' AND ANNULI_COUNT = 1"));
        Assert.Equal(1, Count(database,
            "INTEGRITY_ASSESSMENT WHERE UWI = 'W-3' AND CATEGORY = 'Red' AND FINDINGS_JSON LIKE '%barrier set%'"));

        var noActor = await service.RecordAsync("North", null, null, " ");

        Assert.False(noActor.Success);
        Assert.Equal(IntegrityBoardRules.CodeActorRequired, noActor.Reference);

        // ---- the age filter, which is also the staleness badge ----
        Execute(database,
            "UPDATE INTEGRITY_ASSESSMENT SET ASSESSED_AT_UTC = datetime('now', '-40 days') WHERE UWI = 'W-1';");

        var stale = await service.LoadAsync("North", null, 30);

        Assert.True(stale.Success, stale.Message);
        Assert.Single(stale.Value!.Wells);

        var aged = stale.Value.Wells[0];

        Assert.Equal("W-1", aged.WellId);
        Assert.True(aged.Stale);
        Assert.Contains("stale", aged.StateText, StringComparison.Ordinal);
        Assert.True(aged.AgeDays > 39 && aged.AgeDays < 42, aged.AgeText);

        // ---- AC-6: a failed read is a failure, and no board is returned ----
        Execute(database, "DROP TABLE WELL_BARRIER;");

        var failed = await service.LoadAsync("North", null, null);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
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

            CREATE TABLE WELL_BARRIER (
                BARRIER_ID TEXT,
                UWI TEXT,
                FIELD_NAME TEXT,
                PHASE TEXT,
                SET_STATUS TEXT,
                REVISION TEXT,
                SET_DATE TEXT,
                PREPARED_BY TEXT,
                VERIFIED_BY TEXT,
                ENVELOPE_ROLE TEXT,
                ELEMENT_TYPE TEXT,
                ELEMENT_NAME TEXT,
                TARGET_KIND TEXT,
                TARGET_ID TEXT,
                TOP_DEPTH REAL,
                TOP_DEPTH_OUOM TEXT,
                BOTTOM_DEPTH REAL,
                BOTTOM_DEPTH_OUOM TEXT,
                ELEMENT_STATUS TEXT,
                VERIFIED_DATE TEXT,
                VERIFICATION_METHOD TEXT,
                ACCEPTANCE_REF TEXT,
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
                PRIMARY KEY (BARRIER_ID));

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

            CREATE TABLE INTEGRITY_ASSESSMENT (
                ASSESSMENT_ID TEXT,
                UWI TEXT,
                FIELD_NAME TEXT,
                CATEGORY TEXT,
                COMPLIANT TEXT,
                REASONS_TEXT TEXT,
                FINDINGS_JSON TEXT,
                ENVELOPES_JSON TEXT,
                ANNULI_COUNT INTEGER,
                JOINTS_COUNT INTEGER,
                LEAK_TO_SURFACE TEXT,
                ASSESSED_BY TEXT,
                ASSESSED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ASSESSMENT_ID));

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND) VALUES
                ('W-1', 'North 1', 'North', 'Y'),
                ('W-2', 'North 2', 'North', 'Y'),
                ('W-3', 'North 3', 'North', 'Y'),
                ('W-4', 'South 1', 'South', 'Y');

            INSERT INTO WELL_TUBULAR (UWI, SOURCE, TUBING_TYPE, TUBING_OBS_NO, OUTSIDE_DIAMETER,
                OUTSIDE_DIAMETER_OUOM, INSIDE_DIAMETER, INSIDE_DIAMETER_OUOM, TUBING_GRADE, TUBING_WEIGHT,
                TUBING_WEIGHT_OUOM, BASE_DEPTH, BASE_DEPTH_OUOM, SHOE_DEPTH, SHOE_DEPTH_OUOM, HUNG_TOP_DEPTH,
                HUNG_TOP_DEPTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft', 10000, 'ft', 10000, 'ft', 0, 'ft', 'Y'),
                ('W-1', 'PPDM', 'TUBING', 1, 4.5, 'in', 3.958, 'in', 'L80', 12.6, 'lbm/ft', 9500, 'ft', 9500, 'ft', 0, 'ft', 'Y'),
                ('W-2', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft', 10000, 'ft', 10000, 'ft', 0, 'ft', 'Y'),
                ('W-2', 'PPDM', 'TUBING', 1, 4.5, 'in', 3.958, 'in', 'L80', 12.6, 'lbm/ft', 9500, 'ft', 9500, 'ft', 0, 'ft', 'Y'),
                ('W-3', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft', 10000, 'ft', 10000, 'ft', 0, 'ft', 'Y'),
                ('W-4', 'PPDM', 'CASING', 1, 9.625, 'in', 8.681, 'in', 'L80', 47, 'lbm/ft', 10000, 'ft', 10000, 'ft', 0, 'ft', 'Y'),
                ('W-4', 'PPDM', 'TUBING', 1, 4.5, 'in', 3.958, 'in', 'L80', 12.6, 'lbm/ft', 9500, 'ft', 9500, 'ft', 0, 'ft', 'Y');

            INSERT INTO WELL_BARRIER (BARRIER_ID, UWI, FIELD_NAME, PHASE, SET_STATUS, REVISION, SET_DATE,
                ENVELOPE_ROLE, ELEMENT_TYPE, ELEMENT_NAME, TARGET_KIND, TARGET_ID, ELEMENT_STATUS, ACTIVE_IND) VALUES
                ('B-101', 'W-1', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'DHSV', 'DHSV-1', 'Component', 'DHSV-1', 'Intact', 'Y'),
                ('B-102', 'W-1', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'PACKER', 'Packer 1', 'Component', 'PACKER-1', 'Intact', 'Y'),
                ('B-103', 'W-1', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Secondary', 'CASING_CEMENT', 'Casing cement', 'String', 'W-1|PPDM|CASING|1', 'Intact', 'Y'),
                ('B-201', 'W-2', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'DHSV', 'DHSV-2', 'Component', 'DHSV-2', 'Intact', 'Y'),
                ('B-202', 'W-2', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'PACKER', 'Packer 2', 'Component', 'PACKER-2', 'Intact', 'Y'),
                ('B-203', 'W-2', 'North', 'Production', 'AsBuilt', '1', '2026-01-10', 'Secondary', 'CASING_CEMENT', 'Casing cement', 'String', 'W-2|PPDM|CASING|1', 'Intact', 'Y'),
                ('B-401', 'W-4', 'South', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'TUBING', 'Production tubing', 'String', 'W-4|PPDM|TUBING|1', 'Intact', 'Y'),
                ('B-402', 'W-4', 'South', 'Production', 'AsBuilt', '1', '2026-01-10', 'Primary', 'DHSV', 'DHSV-4', 'Component', 'DHSV-4', 'Intact', 'Y'),
                ('B-403', 'W-4', 'South', 'Production', 'AsBuilt', '1', '2026-01-10', 'Secondary', 'CASING_CEMENT', 'Casing cement', 'String', 'W-4|PPDM|CASING|1', 'Intact', 'Y');

            INSERT INTO WELL_ANNULUS (ANNULUS_ID, UWI, ANNULUS_NAME, INNER_STRING_ID, OUTER_STRING_ID, FLUID,
                FLUID_DENSITY, FLUID_DENSITY_OUOM, SHOE_FRACTURE_GRADIENT, SHOE_FRACTURE_GRADIENT_OUOM,
                WELLHEAD_RATING, WELLHEAD_RATING_OUOM) VALUES
                ('AN-2A', 'W-2', 'A', 'W-2|PPDM|TUBING|1', 'W-2|PPDM|CASING|1', 'GAS', 9, 'ppg', 1.0, 'psi/ft', 5000, 'psi');

            INSERT INTO CASING_PRESSURE_TEST (TEST_ID, UWI, ANNULUS_NAME, BLEED_START_UTC, VALVE_CLOSED_UTC,
                DURATION_HOURS, READINGS_JSON, INITIAL_PRESSURE, MAX_PRESSURE, PRESSURE_OUOM, CLASS_TEXT, SEVERITY,
                MAWOP, MAASP, COMPLETE, RECORDED_BY, RECORDED_AT_UTC) VALUES
                ('T-2A-1', 'W-2', 'A', '2026-11-01 08:00:00', '2026-11-01 08:10:00', 0.1667,
                 '[{"at":"2026-11-01T08:00:00","pressure":2500,"unit":"psi"},{"at":"2026-11-01T08:06:00","pressure":0,"unit":"psi"},{"at":"2026-11-01T08:40:00","pressure":0,"unit":"psi"}]',
                 2500, 2500, 'psi', 'NOT SUSTAINED', 'WARNING', 2061, 5000, 'Y', 'sara', '2026-11-01 09:00:00');

            INSERT INTO MFC_RESULT (MFC_RESULT_ID, UWI, LOG_NAME, LOG_REFERENCE, JOINT_SOURCE, FINGERS, JOINT_COUNT,
                WALL_BASIS, DAMAGE_CLASSES_JSON, WORST_JOINT, WORST_DEPTH, WORST_PENETRATION, FAILED_JOINTS,
                DEPTH_OUOM, CHECKED_BY, CHECKED_AT_UTC) VALUES
                ('MFC-4', 'W-4', 'W-4 caliper', 'caliper.las', 'intervals', 'RAD1,RAD2,RAD3,RAD4', 1, 'minimumWall', '[]',
                 'joint 1', 1000, 0.2, 1, 'ft', 'sara', '2026-11-01 09:00:00');

            INSERT INTO MFC_JOINT (MFC_JOINT_ID, MFC_RESULT_ID, UWI, JOINT_NUMBER, TOP_DEPTH, BOTTOM_DEPTH, DEPTH_OUOM,
                MAX_PENETRATION, PENETRATION_DEPTH, MIN_REMAINING_WALL, NOMINAL_WALL, GRADE, STRING_NAME, WALL_USED,
                BURST, COLLAPSE, TENSION, PASSES) VALUES
                ('J-4-1', 'MFC-4', 'W-4', 1, 1000, 1010, 'ft', 0.2, 1005, 0.4, 0.5, 'L80', 'TUBING', 0.4,
                 8000, 6750, 664000, 'N');
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
