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
/// RPT-01 against a real SQLite database: each report runs over its table and previews with pagination (AC-1), the document is rendered
/// from a template whose unknown token fails it by name (AC-2), an empty result prints the no-data sentence (AC-3), the area's read
/// permission is the gate (AC-4), the PDF is the preview's pages (AC-5), and a template saved twice is two versions with one default
/// (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ReportsGatewayTests
{
    private const string ConnectionName = "rpt01-reports";

    [IntegrationFact]
    public async Task Every_report_runs_over_its_table_and_its_exports_are_the_preview()
    {        var root = Path.Combine(OgPaths.DataRoot, "rpt01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRpt01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "reports.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "reports.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgReportsService(() => source, reporter,
            () => Path.Combine(root, "Reporting", "Templates"), new OgReportPdf());

        var allowed = new OgPermissions(["Accountant"], ["wells.read", "drilling.read", "stratigraphy.read"], true);

        // ---- AC-1 and AC-4: the wells register, over the WELL table ----
        var wells = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), allowed);

        Assert.True(wells.Success, wells.Message + " :: " + string.Join(" | ", reporter.Failures));

        var register = wells.Value!;

        Assert.False(register.Empty);
        Assert.Equal(3, register.Table.Count);
        Assert.Contains("W-1", register.PageText, StringComparison.Ordinal);
        Assert.Contains("W-3", register.PageText, StringComparison.Ordinal);
        Assert.True(register.UsedFallbackLayout);
        Assert.Contains("(generated layout)", register.TitleText, StringComparison.Ordinal);
        Assert.Contains("3", register.SummaryText, StringComparison.Ordinal);

        // A parameter filters, and the parameter appears in the document.
        var filtered = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string> { ["field"] = "North" }, allowed);

        Assert.True(filtered.Success, filtered.Message);
        Assert.Equal(2, filtered.Value!.Table.Count);
        Assert.Contains("field=North", filtered.Value.PageText, StringComparison.Ordinal);

        // AC-3: a field nothing is in prints the sentence rather than an empty table.
        var empty = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string> { ["field"] = "Nowhere" }, allowed);

        Assert.True(empty.Success, empty.Message);
        Assert.True(empty.Value!.Empty);
        Assert.Contains("No data for these parameters", empty.Value.PageText, StringComparison.Ordinal);

        // ---- the other reports over their own tables ----
        var bores = await service.RunAsync(OgReportsService.Wellbores,
            new Dictionary<string, string> { ["well"] = "W-1" }, allowed);

        Assert.True(bores.Success, bores.Message);
        Assert.Equal(2, bores.Value!.Table.Count);

        var picks = await service.RunAsync(OgReportsService.FormationPicks,
            new Dictionary<string, string> { ["well"] = "W-1" }, allowed);

        Assert.True(picks.Success, picks.Message);
        Assert.Equal(2, picks.Value!.Table.Count);
        Assert.Contains("Hod", picks.Value.PageText, StringComparison.Ordinal);

        var stations = await service.RunAsync(OgReportsService.SurveyStations,
            new Dictionary<string, string> { ["well"] = "W-1" }, allowed);

        Assert.True(stations.Success, stations.Message);
        Assert.Equal(2, stations.Value!.Table.Count);

        var reports = await service.RunAsync(OgReportsService.DailyReports,
            new Dictionary<string, string> { ["well"] = "W-1", ["from"] = "2026-10-02" }, allowed);

        Assert.True(reports.Success, reports.Message);
        Assert.Equal(2, reports.Value!.Table.Count);

        // ---- AC-2: a template with an unknown token fails, naming it ----
        var saved = await service.SaveTemplateAsync("house",
            "{{Name}}\n{{Table}}\n{{Count}} row(s) - {{Generated}}", makeDefault: true, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, saved.Value!.Version);
        Assert.True(saved.Value.IsDefault);

        var templated = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), allowed, "house");

        Assert.True(templated.Success, templated.Message);
        Assert.False(templated.Value!.UsedFallbackLayout);
        Assert.Equal("house", templated.Value.TemplateName);
        Assert.Equal(1, templated.Value.TemplateVersion);
        Assert.Contains("3 row(s)", templated.Value.PageText, StringComparison.Ordinal);

        var broken = await service.SaveTemplateAsync("broken", "{{Name}} {{Nonsense}}", makeDefault: false, "sara");

        Assert.True(broken.Success, broken.Message);

        var refused = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), allowed, "broken");

        Assert.False(refused.Success);
        Assert.Equal(OgReportRules.CodeUnknownToken, refused.Reference);
        Assert.Contains("{{Nonsense}}", refused.Message!, StringComparison.Ordinal);

        // ---- AC-6: a second save is a second version, and the default moved off the first ----
        var version2 = await service.SaveTemplateAsync("house", "{{Name}} v2\n{{Table}}", makeDefault: false, "sara");

        Assert.True(version2.Success, version2.Message);
        Assert.Equal(2, version2.Value!.Version);
        Assert.False(version2.Value.IsDefault);

        var other = await service.SaveTemplateAsync("client", "{{Name}}", makeDefault: true, "sara");

        Assert.True(other.Success, other.Message);

        var templates = await service.TemplatesAsync();

        Assert.True(templates.Success);
        Assert.Equal(3, templates.Value!.Count);
        Assert.Single(templates.Value.Where(template => template.IsDefault));
        Assert.Equal("client", templates.Value.Single(template => template.IsDefault).Name);

        // With the explicit default being another template, an unnamed run uses it.
        var defaulted = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), allowed);

        Assert.True(defaulted.Success, defaulted.Message);
        Assert.Equal("client", defaulted.Value!.TemplateName);
        Assert.False(defaulted.Value.UsedFallbackLayout);

        // ---- AC-5: the PDF is the preview's pages, one PDF page each ----
        var folder = Path.Combine(root, "out");

        Directory.CreateDirectory(folder);

        var pdfPath = Path.Combine(folder, "register.pdf");
        var exported = await service.ExportAsync(templated.Value, "pdf", pdfPath);

        Assert.True(exported.Success, exported.Message);

        var pdf = File.ReadAllBytes(pdfPath);

        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4), StringComparison.Ordinal);
        Assert.True(templated.Value.Pages.Count > 0);

        var document = System.Text.Encoding.Latin1.GetString(pdf);

        // The page tree node is '/Type /Pages', which contains the page's own marker - so the pages are the difference.
        Assert.Equal(templated.Value.Pages.Count,
            CountOf(document, "/Type /Page") - CountOf(document, "/Type /Pages"));

        // The other three formats write the same table and the same document.
        var csv = await service.ExportAsync(templated.Value, "csv", Path.Combine(folder, "register.csv"));

        Assert.True(csv.Success, csv.Message);
        Assert.Equal(4, File.ReadAllLines(Path.Combine(folder, "register.csv")).Length);

        var html = await service.ExportAsync(templated.Value, "html", Path.Combine(folder, "register.html"));

        Assert.True(html.Success, html.Message);
        Assert.Contains("W-1", File.ReadAllText(Path.Combine(folder, "register.html")), StringComparison.Ordinal);

        var xlsx = await service.ExportAsync(templated.Value, "xlsx", Path.Combine(folder, "register.xlsx"));

        Assert.True(xlsx.Success, xlsx.Message);
        Assert.Equal(new byte[] { 0x50, 0x4B }, File.ReadAllBytes(Path.Combine(folder, "register.xlsx"))[..2]);

        // ---- AC-1: a long row set paginates, and the pages add up to the rows ----
        for (var index = 0; index < 60; index++)
        {
            Execute(database, "INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, CURRENT_STATUS, DEEPEST_DEPTH, ACTIVE_IND) VALUES ('X-" +
                               index.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                               "', 'Extra', 'North', 'ACTIVE', 1000, 'Y');");
        }

        var paged = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), allowed, "house");

        Assert.True(paged.Success, paged.Message);
        Assert.Equal(63, paged.Value!.Table.Count);
        Assert.True(paged.Value.PageCount > 1, paged.Value.PageCount.ToString(System.Globalization.CultureInfo.InvariantCulture));

        var everyLine = paged.Value.Pages.SelectMany(page => page.Lines).ToList();

        Assert.Contains(everyLine, line => line.Contains("X-59", StringComparison.Ordinal));
        Assert.Contains(everyLine, line => line.Contains("W-1", StringComparison.Ordinal));

        // ---- AC-4: a report the person may not read is refused by area ----
        var nobody = new OgPermissions(["Viewer"], ["stratigraphy.read"], true);

        var refusedByArea = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), nobody);

        Assert.False(refusedByArea.Success);
        Assert.Equal(OgReportRules.CodePermission, refusedByArea.Reference);

        var unknownArea = new OgPermissions([], [], permissionSourceAvailable: false);

        Assert.False((await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), unknownArea)).Success);

        // ---- and a failed read is a failure, not an empty report ----
        Execute(database, "DROP TABLE WELLBORE;");

        var failed = await service.RunAsync(OgReportsService.Wellbores,
            new Dictionary<string, string> { ["well"] = "W-1" }, allowed);

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    private static int CountOf(string text, string needle)
    {
        var count = 0;
        var at = 0;

        while ((at = text.IndexOf(needle, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += needle.Length;
        }

        return count;
    }

    // ------------------------------------------------------------------ the fixture

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

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, CURRENT_STATUS, DEEPEST_DEPTH, ACTIVE_IND) VALUES
                ('W-1', 'Alpha 1', 'North', 'ACTIVE', 2500, 'Y'),
                ('W-2', 'Alpha 2', 'North', 'SUSPENDED', 2000, 'Y'),
                ('W-3', 'Beta 1', 'South', 'ACTIVE', 1800, 'Y');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, KIND, STATUS, TOTAL_DEPTH, ACTIVE_IND) VALUES
                ('W-1', 'W-1-B1', 'Alpha 1 main', 'Original', 'Active', 2500, 'Y'),
                ('W-1', 'W-1-B2', 'Alpha 1 sidetrack', 'Sidetrack', 'Active', 2400, 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, STATION_TVD, ACTIVE_IND) VALUES
                ('W-1', 'S-1', 'PPDM', 1, 0, 'm', 0, 0, 0, 'Y'),
                ('W-1', 'S-1', 'PPDM', 2, 2500, 'm', 0, 0, 2500, 'Y');

            INSERT INTO STRAT_UNIT (STRAT_NAME_SET_ID, STRAT_UNIT_ID, LONG_NAME, ACTIVE_IND) VALUES
                ('NNS', 'HOD', 'Hod', 'Y'),
                ('NNS', 'TOR', 'Tor', 'Y');

            INSERT INTO STRAT_WELL_SECTION (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID, PICK_DEPTH,
                PICK_DEPTH_OUOM, PICK_TVD, INTERPRETER, ACTIVE_IND) VALUES
                ('W-1', 'NNS', 'HOD', 'I-1', 2000, 'm', 2000, 'sara', 'Y'),
                ('W-1', 'NNS', 'TOR', 'I-2', 2300, 'm', 2300, 'sara', 'Y');

            INSERT INTO DEVELOPMENT_DAILY_REPORT (REPORT_ID, EXECUTION_ID, PROGRAM_ID, WELL_ID, REPORT_DATE,
                PERIOD_START, PERIOD_END, DEPTH_START, DEPTH_END, DEPTH_OUOM, COST, CURRENCY, STATE, ACTIVE_IND) VALUES
                ('R-1', 'E-1', 'P-1', 'W-1', '2026-10-01', '2026-10-01 06:00:00', '2026-10-01 18:00:00', 1000, 1250, 'm', 100000, 'USD', 'SUBMITTED', 'Y'),
                ('R-2', 'E-1', 'P-1', 'W-1', '2026-10-02', '2026-10-01 18:00:00', '2026-10-02 06:00:00', 1250, 1400, 'm', 80000, 'USD', 'SUBMITTED', 'Y'),
                ('R-3', 'E-1', 'P-1', 'W-1', '2026-10-03', '2026-10-02 06:00:00', '2026-10-02 18:00:00', 1400, 1500, 'm', 60000, 'USD', 'SUBMITTED', 'Y');
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
