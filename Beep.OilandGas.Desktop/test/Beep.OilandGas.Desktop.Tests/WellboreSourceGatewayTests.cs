using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-03 against a real SQLite database: a well read from its own records renders with a depth track and at least one data
/// track (AC-1), the axis is the recorded total depth, the completion intervals become the interval track, the readout answers
/// at a depth, and the missing curve store is stated rather than hidden (AC-5).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellboreSourceGatewayTests
{
    private const string ConnectionName = "vsb03-wellbore";

    [IntegrationFact]
    public async Task A_well_from_the_database_draws_a_depth_track_and_a_data_track()
    {        var root = Path.Combine(OgPaths.DataRoot, "vsb03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasVsb03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "wellbore.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "wellbore.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellboreSource(() => source, reporter);

        // ---- AC-1: the well renders from its own records ----
        var loaded = await service.LoadAsync("W-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var view = loaded.Value!;

        Assert.Equal("W-1", view.WellId);
        Assert.Equal("North 1", view.WellName);
        Assert.True(view.HasDepthAndDataTrack);
        Assert.Equal(0f, view.Top);
        Assert.Equal(2500f, view.Bottom);
        Assert.Equal("m", view.Unit);

        var depthTrack = Assert.Single(view.Document.Tracks.OfType<DepthTrack>());
        var intervals = Assert.Single(view.Document.Tracks.OfType<IntervalTrack>());

        Assert.Equal(2, intervals.Intervals.Count);
        Assert.Equal(1000f, intervals.Intervals[0].Top);
        Assert.Equal(1010f, intervals.Intervals[0].Bottom);
        Assert.Equal("PERFORATED", intervals.Intervals[0].Text);
        Assert.Equal("OPEN_HOLE", intervals.Intervals[1].Text);

        // AC-3's data: the readout answers with the interval at the depth.
        Assert.Contains("PERFORATED", view.ReadoutAt(1005f), StringComparison.Ordinal);
        Assert.Contains("nothing is recorded at this depth", view.ReadoutAt(200f), StringComparison.Ordinal);

        // AC-5: the missing curve store is stated, not hidden behind a demo curve.
        Assert.Contains(view.Warnings, warning => warning.Contains("no measured curve values", StringComparison.Ordinal));
        Assert.Contains("no measured curve values", view.WarningsText, StringComparison.Ordinal);

        // An inactive completion is not drawn.
        Assert.DoesNotContain(intervals.Intervals, interval => interval.Top == 2000f);

        // ---- a well whose axis comes from its deepest interval ----
        var shallow = await service.LoadAsync("W-2");

        Assert.True(shallow.Success, shallow.Message);
        Assert.Equal(2100f, shallow.Value!.Bottom);
        Assert.Equal("ft", shallow.Value.Unit);
        Assert.Contains(shallow.Value.Warnings, warning => warning.Contains("no total depth", StringComparison.Ordinal));

        // ---- a well with no depth-indexed data is refused with its reason, not drawn empty ----
        var bare = await service.LoadAsync("W-3");

        Assert.False(bare.Success);
        Assert.Equal(WellboreSourceRules.CodeNoDepthData, bare.Reference);
        Assert.Contains("nothing to draw beside the depth axis", bare.Message!, StringComparison.Ordinal);

        // ---- an unknown well and a missing identity ----
        var unknown = await service.LoadAsync("W-404");

        Assert.False(unknown.Success);
        Assert.Equal(WellboreSourceRules.CodeWellUnknown, unknown.Reference);

        var noIdentity = await service.LoadAsync(" ");

        Assert.False(noIdentity.Success);
        Assert.Equal(WellboreSourceRules.CodeWellRequired, noIdentity.Reference);

        // ---- a failed read is a failure, not an empty well ----
        Execute(database, "DROP TABLE WELL_COMPLETION;");

        var failed = await service.LoadAsync("W-1");

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

            CREATE TABLE WELL_COMPLETION (
                UWI TEXT,
                SOURCE TEXT,
                COMPLETION_OBS_NO REAL,
                ACTIVE_IND TEXT,
                BASE_DEPTH REAL,
                BASE_DEPTH_OUOM TEXT,
                BASE_STRAT_UNIT_ID TEXT,
                COMPLETION_DATE TEXT,
                COMPLETION_METHOD TEXT,
                COMPLETION_STRAT_UNIT_ID TEXT,
                COMPLETION_TYPE TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                PPDM_GUID TEXT,
                REMARK TEXT,
                STRAT_NAME_SET_ID TEXT,
                TOP_DEPTH REAL,
                TOP_DEPTH_OUOM TEXT,
                TOP_STRAT_UNIT_ID TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (UWI, SOURCE, COMPLETION_OBS_NO));

            INSERT INTO WELL (UWI, WELL_NAME, BASE_DEPTH, BASE_DEPTH_OUOM, ACTIVE_IND) VALUES
                ('W-1', 'North 1', 2500, 'm', 'Y'),
                ('W-2', 'North 2', NULL, NULL, 'Y'),
                ('W-3', 'North 3', 1200, 'm', 'Y');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, TOTAL_DEPTH, TOTAL_DEPTH_OUOM) VALUES
                ('W-1', 'WB-1', 'North 1 main bore', 2500, 'm'),
                ('W-2', 'WB-2', 'North 2 main bore', NULL, NULL),
                ('W-3', 'WB-3', 'North 3 main bore', 1200, 'm');

            INSERT INTO WELL_COMPLETION (UWI, SOURCE, COMPLETION_OBS_NO, TOP_DEPTH, BASE_DEPTH, TOP_DEPTH_OUOM,
                COMPLETION_TYPE, ACTIVE_IND) VALUES
                ('W-1', 'PPDM', 1, 1000, 1010, 'm', 'PERFORATED', 'Y'),
                ('W-1', 'PPDM', 2, 1500, 1520, 'm', 'OPEN_HOLE', 'Y'),
                ('W-1', 'PPDM', 3, 2000, 2010, 'm', 'ABANDONED', 'N'),
                ('W-2', 'PPDM', 1, 2000, 2100, 'ft', 'PERFORATED', 'Y');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
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
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
