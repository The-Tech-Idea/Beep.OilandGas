using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RPT-02 against a real SQLite database and the owners' own PDF writer: a pack over three wells writes three documents and one manifest
/// (AC-1), a well with no survey is skipped with its reason (AC-2), a cancelled run keeps what completed and still writes the manifest
/// (AC-3), the file names are unique and deterministic (AC-4), the print options reach the document (AC-5), and the manifest's page
/// counts are read back from the files (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class PrintPackGatewayTests
{
    private const string ConnectionName = "rpt02-packs";

    [IntegrationFact]
    public async Task A_pack_writes_a_document_per_well_and_one_manifest()
    {        var root = Path.Combine(OgPaths.DataRoot, "rpt02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRpt02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "packs.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "packs.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var reports = new OgReportsService(() => source, reporter,
            () => Path.Combine(root, "Reporting", "Templates"), new OgReportPdf());

        var service = new OgPrintPacksService(() => source, reporter, reports);

        var allowed = new OgPermissions(["Manager"], ["reporting.pack.run", "data.export", "drilling.read", "stratigraphy.read"], true);

        var pack = new OgPrintPack("wellview", new OgPackScope(OgPackScope.All, null, []),
            new OgPackOptions("A4", 0f, IncludeHeaderBlock: true, IncludeLegend: true, "Field pack"));

        // ---- the preview writes nothing and measures the first document ----
        var folder = Path.Combine(root, "pack");

        var estimate = await service.EstimateAsync(pack, allowed);

        Assert.True(estimate.Success, estimate.Message + " :: " + string.Join(" | ", reporter.Failures));
        // Four wells are in scope and three can be drawn: the estimate reports the scope and what it would leave out.
        Assert.Equal(3, estimate.Value!.Files);
        Assert.Equal(4, estimate.Value.Wells.Count);
        Assert.Equal(1, estimate.Value.Skipped.Count);
        Assert.Contains("no survey", estimate.Value.Skipped[0], StringComparison.Ordinal);
        Assert.True(estimate.Value.SamplePages > 0, estimate.Value.Describe());
        Assert.NotEmpty(estimate.Value.NameSample);
        Assert.False(Directory.Exists(folder));

        // ---- AC-1, AC-4, AC-5 and AC-6: the pack ----
        var run = await service.RunAsync(pack, folder, allowed, stopOnError: false, progress: null);

        Assert.True(run.Success, run.Message + " :: " + string.Join(" | ", reporter.Failures));

        var manifest = run.Value!;

        Assert.Equal(3, manifest.Written);
        Assert.Equal(1, manifest.Skipped);
        Assert.Equal(0, manifest.Failed);
        Assert.False(manifest.Cancelled);
        Assert.NotNull(manifest.ManifestPath);
        Assert.True(File.Exists(manifest.ManifestPath));
        Assert.True(File.Exists(manifest.CsvPath));

        var files = Directory.GetFiles(folder, "*.pdf").OrderBy(path => path, StringComparer.Ordinal).ToList();

        Assert.Equal(3, files.Count);

        // AC-4: names are the deterministic pattern, and no two are the same.
        Assert.Equal(files.Count, files.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(files, path => Assert.Matches(@"^\d{3}-wellview-W-\d\.pdf$", Path.GetFileName(path)));
        Assert.Equal(OgPackRules.FileName(0, "wellview", "W-1"), Path.GetFileName(files[0]));
        Assert.Equal(OgPackRules.FileName(2, "wellview", "W-3"), Path.GetFileName(files[2]));

        // AC-6: every manifest item's page count is the file's own, and the files start with a PDF header.
        foreach (var item in manifest.Items.Where(item => item.Written))
        {
            var bytes = File.ReadAllBytes(Path.Combine(folder, item.File!));

            Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4), StringComparison.Ordinal);
            Assert.Equal(OgPackRules.PageCount(bytes), item.Pages);
            Assert.True(item.Pages > 0, item.Describe());
            Assert.Equal(bytes.Length, item.Bytes);
        }

        Assert.Equal(manifest.Items.Where(item => item.Written).Sum(item => item.Pages), manifest.Pages);

        // AC-2: the skipped well is named with its reason, and the manifest says so in both formats.
        var skipped = manifest.Items.Single(item => !item.Written);

        Assert.Equal("W-4", skipped.WellId);
        Assert.Contains("no survey", skipped.Reason!, StringComparison.Ordinal);

        var csv = File.ReadAllText(manifest.CsvPath!).Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        // One line per well in scope - the three written and the one skipped - plus the header.
        Assert.Equal(5, csv.Length);
        Assert.Contains("\"W-4\"", string.Join("\n", csv), StringComparison.Ordinal);
        Assert.Contains("\"skipped\"", string.Join("\n", csv), StringComparison.Ordinal);

        // AC-5: the options reached the documents - a header block and a legend make a taller document than neither.
        var withChrome = File.ReadAllBytes(Path.Combine(folder, manifest.Items.First(item => item.Written).File!));

        var bareFolder = Path.Combine(root, "bare");

        var bare = await service.RunAsync(
            pack with { Options = pack.Options with { IncludeHeaderBlock = false, IncludeLegend = false } },
            bareFolder, allowed, false, null);

        Assert.True(bare.Success, bare.Message);

        var bareFile = File.ReadAllBytes(Path.Combine(bareFolder, bare.Value!.Items.First(item => item.Written).File!));

        Assert.NotEqual(withChrome.Length, bareFile.Length);

        // ---- AC-3: a cancelled run keeps what completed and writes the manifest ----
        var cancelledFolder = Path.Combine(root, "cancelled");
        using var cancellation = new CancellationTokenSource();

        var progress = new Callback(report =>
        {
            if (report.Done >= 1)
            {
                cancellation.Cancel();
            }
        });

        var cancelled = await service.RunAsync(pack, cancelledFolder, allowed, false, progress, cancellation.Token);

        Assert.True(cancelled.Success, cancelled.Message);
        Assert.True(cancelled.Value!.Cancelled);
        Assert.True(cancelled.Value.Written >= 1);
        Assert.True(File.Exists(cancelled.Value.ManifestPath));
        Assert.Equal(cancelled.Value.Written, Directory.GetFiles(cancelledFolder, "*.pdf").Length);

        // ---- the report pack: one RPT-01 report per well ----
        var reportPack = new OgPrintPack("report", new OgPackScope(OgPackScope.Selected, null, ["W-1", "W-2"]),
            new OgPackOptions("A4", 0f, true, true, OgReportsService.SurveyStations));

        var reportFolder = Path.Combine(root, "reports");
        var reportRun = await service.RunAsync(reportPack, reportFolder, allowed, false, null);

        Assert.True(reportRun.Success, reportRun.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, reportRun.Value!.Written);
        Assert.All(reportRun.Value.Items.Where(item => item.Written),
            item => Assert.True(item.Pages > 0, item.Describe()));

        // ---- a report key the pack cannot print per well is refused ----
        var wrongReport = await service.EstimateAsync(
            reportPack with { Options = reportPack.Options with { Title = OgReportsService.WellsRegister } }, allowed);

        Assert.False(wrongReport.Success);
        Assert.Equal(OgPackRules.CodeReportKey, wrongReport.Reference);

        // ---- no well in scope is a refusal with the reason ----
        var noWells = await service.EstimateAsync(
            pack with { Scope = new OgPackScope(OgPackScope.Selected, null, ["NO-SUCH-WELL"]) }, allowed);

        Assert.False(noWells.Success);
        Assert.Equal(OgPackRules.CodeNoWells, noWells.Reference);
        Assert.Contains("No well is in scope", noWells.Message!, StringComparison.Ordinal);

        // ---- and a well that cannot be drawn is skipped, not written ----
        Execute(database, "DELETE FROM WELL_DIR_SRVY_STATION WHERE UWI = 'W-2';");

        var afterDelete = await service.RunAsync(
            pack with { Scope = new OgPackScope(OgPackScope.Selected, null, ["W-2"]) }, Path.Combine(root, "one"), allowed,
            false, null);

        Assert.True(afterDelete.Success, afterDelete.Message);
        Assert.Equal(0, afterDelete.Value!.Written);
        Assert.Equal(1, afterDelete.Value.Skipped);
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>Three drawable wells and one with no survey, so the pack has a skip without being empty.</summary>
    private static void Seed(string database) => Execute(database, """
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

            CREATE TABLE STRAT_WELL_SECTION (
                UWI TEXT,
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                INTERP_ID TEXT,
                ACTIVE_IND TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                AZIMUTH_NORTH_TYPE TEXT,
                CERTIFIED_IND TEXT,
                CONFORMITY_RELATIONSHIP TEXT,
                DIP_ANGLE REAL,
                DIP_DIRECTION TEXT,
                DOMINANT_LITHOLOGY TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FAULT_HEAVE REAL,
                FAULT_THROW REAL,
                INTERPRETER TEXT,
                MISSING_SECTION REAL,
                MISSING_STRAT_TYPE TEXT,
                ORDINAL_SEQ_NO REAL,
                OVERTURNED_IND TEXT,
                PICK_DATE TEXT,
                PICK_DEPTH REAL,
                PICK_DEPTH_OUOM TEXT,
                PICK_LOCATION TEXT,
                PICK_QUALIFIER TEXT,
                PICK_QUALIF_REASON TEXT,
                PICK_QUALITY TEXT,
                PICK_TVD REAL,
                PICK_VERSION_TYPE TEXT,
                PPDM_GUID TEXT,
                PREFERRED_PICK_IND TEXT,
                REMARK TEXT,
                REPEAT_SECTION REAL,
                REPEAT_STRAT_OCCUR_NO REAL,
                REPEAT_STRAT_TYPE TEXT,
                SOURCE TEXT,
                SOURCE_DOCUMENT_ID TEXT,
                STRAT_INTERPRET_METHOD TEXT,
                STRIKE REAL,
                SW_APPLICATION_ID TEXT,
                TVD_METHOD TEXT,
                VERSION_OBS_NO REAL,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                PRIMARY KEY (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID));

            CREATE TABLE STRAT_UNIT (
                STRAT_NAME_SET_ID TEXT,
                STRAT_UNIT_ID TEXT,
                ABBREVIATION TEXT,
                ACTIVE_IND TEXT,
                AREA_ID TEXT,
                AREA_TYPE TEXT,
                BUSINESS_ASSOCIATE_ID TEXT,
                CONFIDENCE_ID TEXT,
                CURRENT_STATUS_DATE TEXT,
                DESCRIPTION TEXT,
                EFFECTIVE_DATE TEXT,
                EXPIRY_DATE TEXT,
                FAULT_TYPE TEXT,
                FORM_CODE TEXT,
                GROUP_CODE TEXT,
                LONG_NAME TEXT,
                ORDINAL_AGE_CODE REAL,
                PPDM_GUID TEXT,
                PREFERRED_IND TEXT,
                REMARK TEXT,
                SHORT_NAME TEXT,
                SOURCE TEXT,
                STRAT_INTERPRET_METHOD TEXT,
                STRAT_STATUS TEXT,
                STRAT_TYPE TEXT,
                STRAT_UNIT_TYPE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PRIMARY KEY (STRAT_NAME_SET_ID, STRAT_UNIT_ID));

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, CURRENT_STATUS, DEEPEST_DEPTH, ACTIVE_IND) VALUES
                ('W-1', 'Alpha 1', 'North', 'ACTIVE', 2500, 'Y'),
                ('W-2', 'Alpha 2', 'North', 'ACTIVE', 2000, 'Y'),
                ('W-3', 'Beta 1', 'South', 'ACTIVE', 1800, 'Y'),
                ('W-4', 'Beta 2', 'South', 'ACTIVE', 1600, 'Y');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, KIND, STATUS, TOTAL_DEPTH, ACTIVE_IND) VALUES
                ('W-1', 'W-1-B1', 'Alpha 1 main', 'Original', 'Active', 2500, 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, STATION_TVD, ACTIVE_IND) VALUES
                ('W-1', 'S-1', 'PPDM', 1, 0, 'm', 0, 0, 0, 'Y'),
                ('W-1', 'S-1', 'PPDM', 2, 2500, 'm', 0, 0, 2500, 'Y'),
                ('W-2', 'S-2', 'PPDM', 1, 0, 'm', 0, 0, 0, 'Y'),
                ('W-2', 'S-2', 'PPDM', 2, 2000, 'm', 0, 0, 2000, 'Y'),
                ('W-3', 'S-3', 'PPDM', 1, 0, 'm', 0, 0, 0, 'Y'),
                ('W-3', 'S-3', 'PPDM', 2, 1800, 'm', 0, 0, 1800, 'Y');

            INSERT INTO STRAT_UNIT (STRAT_NAME_SET_ID, STRAT_UNIT_ID, LONG_NAME, ACTIVE_IND) VALUES
                ('NNS', 'HOD', 'Hod', 'Y');

            INSERT INTO STRAT_WELL_SECTION (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID, PICK_DEPTH,
                PICK_DEPTH_OUOM, PICK_TVD, INTERPRETER, ACTIVE_IND) VALUES
                ('W-1', 'NNS', 'HOD', 'I-1', 2000, 'm', 2000, 'sara', 'Y'),
                ('W-2', 'NNS', 'HOD', 'I-2', 1700, 'm', 1700, 'sara', 'Y');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    /// <summary>A progress callback that can act on the run as it goes - how the cancellation test stops it at a well boundary.</summary>
    private sealed class Callback : IProgress<OgPackProgress>
    {
        private readonly Action<OgPackProgress> _onProgress;

        public Callback(Action<OgPackProgress> onProgress) => _onProgress = onProgress;

        public void Report(OgPackProgress value) => _onProgress(value);
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
