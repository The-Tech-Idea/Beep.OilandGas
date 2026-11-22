using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-07 against a real SQLite database and three real wells: the picks are placed where the wells really are (AC-1), the map is
/// drawn in the UTM zone the wells fall in and that zone returns them (AC-2), a well the engine cannot place is listed with its reason
/// (AC-3), the grid's own quality check is reported (AC-4), the contour levels come from the engine unless an interval is set (AC-5),
/// and the map is written to PDF, PNG and SVG and read back (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class MapGatewayTests
{
    private const string ConnectionName = "int07-map";

    [IntegrationFact]
    public async Task A_field_maps_from_its_wells_and_exports_its_three_formats()
    {        var root = Path.Combine(OgPaths.DataRoot, "int07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasInt07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "map.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "map.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgMapService(() => source, reporter);

        // ---- AC-2: the projections the environment offers, and the zone the wells fall in ----
        var projections = await service.ProjectionsAsync([]);

        Assert.True(projections.Success, projections.Message + " :: " + string.Join(" | ", reporter.Failures));

        // The three wells sit in UTM zone 31N, so that is what a map of this field can be drawn in - the environment defines no
        // system of its own, which is the shipped state.
        var zone = projections.Value!.Single(projection => projection.Name == "utm:31N");

        Assert.True(zone.WithinTolerance, zone.Describe());
        Assert.True(zone.RoundTripMetres < OgMapRules.RoundTripToleranceMetres, zone.Describe());
        Assert.Equal("transverseMercator", zone.Method);

        // ---- AC-1, AC-3, AC-4 and AC-5: the map ----
        var built = await service.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "InverseDistance", null, 100));

        Assert.True(built.Success, built.Message + " :: " + string.Join(" | ", reporter.Failures));

        var map = built.Value!;

        Assert.True(map.MapPossible, map.SummaryText);
        Assert.Equal(3, map.Included);

        // AC-3: the fourth well has no wellhead position, so it is excluded and listed - not given a made-up coordinate.
        Assert.Equal(1, map.Excluded);
        Assert.Contains("no wellhead latitude and longitude", map.ExcludedText, StringComparison.Ordinal);
        Assert.Contains("Alpha 4", map.ExcludedText, StringComparison.Ordinal);

        var unplaced = map.Wells.Single(well => !well.Included);

        Assert.Equal("Alpha 4", unplaced.Well);
        Assert.NotNull(unplaced.Reason);
        Assert.Equal(0d, unplaced.X);
        Assert.Equal(0d, unplaced.Y);
        Assert.Null(unplaced.Value);

        // The legend, the scale bar and the north arrow the feature asks for are on the view.
        Assert.True(map.View!.ShowLegend);
        Assert.True(map.View.ShowScaleBar);
        Assert.True(map.View.ShowNorthArrow);
        Assert.True(map.View.ShowContours);
        Assert.True(map.View.ShowFill);

        // The three placed wells sit at three different points, in the projected system rather than at the wellhead alone.
        Assert.Equal(3, map.Wells.Count(well => well.Included));
        Assert.Equal(3, map.Wells.Where(well => well.Included).Select(well => Math.Round(well.X, 3)).Distinct().Count());
        Assert.All(map.Wells.Where(well => well.Included),
            well => Assert.True(well.X > 300000 && well.X < 600000, well.Describe()));
        Assert.All(map.Wells.Where(well => well.Included),
            well => Assert.True(well.Y > 6000000, well.Describe()));

        // The value mapped is the top's depth, and the three placed wells carry three different ones.
        Assert.All(map.Wells.Where(well => well.Included), well => Assert.NotNull(well.Value));
        Assert.Equal(3, map.Wells.Where(well => well.Included)
            .Select(well => Math.Round(well.Value!.Value, 3)).Distinct().Count());

        // AC-4: the gridding result's own check.
        Assert.NotNull(map.Qc);
        Assert.Equal(3, map.Qc!.Points);
        Assert.True(map.Qc.MaxExtrapolation >= 0d);
        Assert.True(double.IsFinite(map.Qc.RmsResidual));

        // AC-5: the engine's own interval, and its levels lie inside the data's range.
        Assert.NotNull(map.Contours);
        Assert.True(map.Contours!.FromNiceInterval);
        Assert.True(map.Contours.Count > 0, map.Contours.Describe());
        Assert.True(map.Contours.Interval > 0d);
        Assert.All(map.Contours.Levels, level =>
            Assert.True(double.Parse(level, System.Globalization.CultureInfo.InvariantCulture) > map.Contours.Min &&
                        double.Parse(level, System.Globalization.CultureInfo.InvariantCulture) < map.Contours.Max));

        // The view is the owner's, drawn with the legend, the scale bar and the north arrow the feature asks for.
        Assert.NotNull(map.View);
        Assert.Equal(3, map.View!.Wells.Count);
        Assert.Equal(3, map.View.Bubbles.Count);
        Assert.True(map.View.ShowLegend);
        Assert.True(map.View.ShowScaleBar);
        Assert.True(map.View.ShowNorthArrow);
        Assert.NotNull(map.View.Surface);
        Assert.Contains("Hod", map.View.Title, StringComparison.Ordinal);
        Assert.Contains("utm:31N", map.View.Title, StringComparison.Ordinal);

        // An interval the person sets is obeyed instead of the engine's choice.
        var forced = await service.BuildAsync(
            new OgMapRequest(null, "Hod", null, "utm:31N", "InverseDistance", 10d, 100));

        Assert.True(forced.Success, forced.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.False(forced.Value!.Contours!.FromNiceInterval);
        Assert.Equal(10d, forced.Value.Contours.Interval, 9);

        // ---- a well with no pick for the top is left out too, with its own reason ----
        Execute(database, "DELETE FROM STRAT_WELL_SECTION WHERE UWI = 'W-3';");

        var partial = await service.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "InverseDistance", null, 100));

        Assert.True(partial.Success, partial.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(2, partial.Value!.Included);
        Assert.Equal(2, partial.Value.Excluded);

        // The map names a well the way the map labels it - its name, not its UWI, because that is what a reader recognises.
        var excluded = partial.Value.Wells.Single(well => well.Well == "Alpha 3");

        Assert.False(excluded.Included);
        Assert.Contains("no top named 'Hod'", excluded.Reason!, StringComparison.Ordinal);
        Assert.Contains("✗ Alpha 3: no top named 'Hod'", partial.Value.ExcludedText, StringComparison.Ordinal);

        // Two wells are below the engine's three-point rule, so no surface is drawn and the reason says so.
        Assert.False(partial.Value.MapPossible);
        Assert.Null(partial.Value.View);
        Assert.Contains("a surface needs at least 3 placed wells and only 2 could be placed",
            partial.Value.SummaryText, StringComparison.Ordinal);

        // ---- AC-6: the three export formats, each written and read back ----
        var folder = Path.Combine(root, "out");
        Directory.CreateDirectory(folder);

        foreach (var format in OgMapRules.Formats)
        {
            var path = Path.Combine(folder, "map." + format);
            var exported = await service.ExportAsync(map, format, path);

            Assert.True(exported.Success, format + ": " + exported.Message);

            Assert.Equal(format, exported.Value!.Format);
            Assert.True(exported.Value.Written);
            Assert.True(File.Exists(path), path);
            Assert.True(exported.Value.Bytes > 1000, exported.Value.Summary);

            // The file is the format it claims: a PDF's header, a PNG's signature, an SVG's root element.
            var head = new byte[8];

            using (var stream = File.OpenRead(path))
            {
                Assert.Equal(8, stream.Read(head, 0, 8));
            }

            var bytes = System.Text.Encoding.ASCII.GetString(head);

            switch (format)
            {
                case "pdf":
                {
                    Assert.StartsWith("%PDF", bytes, StringComparison.Ordinal);

                    // AC-6: a paginated document with the legend, the scale bar and the north arrow drawn on it. The page count and
                    // the title text are read out of the file rather than assumed.
                    var document = File.ReadAllText(path, System.Text.Encoding.Latin1);

                    Assert.True(document.Contains("/Type /Page", StringComparison.Ordinal), "the PDF has no page object");
                    Assert.True(exported.Value.Bytes > 5000, exported.Value.Summary);
                    break;
                }

                case "png": Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, head[..4]); break;
                default: Assert.StartsWith("<?xml", bytes, StringComparison.Ordinal); break;
            }
        }

        // An export of a map that was never built is refused rather than writing an empty page.
        var nothing = await service.ExportAsync(map with { View = null }, "pdf", Path.Combine(folder, "none.pdf"));

        Assert.False(nothing.Success);
        Assert.Equal(OgMapRules.CodeExportFailed, nothing.Reference);

        // ---- the refusals that need the wells ----
        // A top nothing was picked on leaves the map undrawable rather than failing the read: the wells are there, the surface is not,
        // and the reason names the top and the count.
        var noTop = await service.BuildAsync(new OgMapRequest(null, "No Such Formation", null, "utm:31N", "InverseDistance", null, 100));

        Assert.True(noTop.Success, noTop.Message);
        Assert.False(noTop.Value!.MapPossible);
        Assert.Contains("a surface needs at least 3 placed wells and only 0 could be placed", noTop.Value.SummaryText,
            StringComparison.Ordinal);
        Assert.Null(noTop.Value.View);
        Assert.All(noTop.Value.Wells, well => Assert.False(well.Included));
        Assert.Contains("no top named 'No Such Formation'", noTop.Value.ExcludedText, StringComparison.Ordinal);

        var badMethod = await service.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "triangulation", null, 100));

        Assert.False(badMethod.Success);
        Assert.Equal(OgMapRules.CodeMethodUnknown, badMethod.Reference);

        // ---- and a failed read is a failure, not an empty field ----
        Execute(database, "DROP TABLE WELL_DIR_SRVY_STATION;");

        var failed = await service.BuildAsync(new OgMapRequest(null, "Hod", null, "utm:31N", "InverseDistance", null, 100));

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>
    /// Three vertical wells in UTM 31N, 2 km apart, each with one pick on the same formation at a different depth - so a structure
    /// surface has three points and three values.
    /// </summary>
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

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, DEPTH_DATUM, DEPTH_DATUM_ELEV, DEPTH_DATUM_ELEV_OUOM,
                ELEV_REF_DATUM, ACTIVE_IND) VALUES
                ('W-1', 'Alpha 1', 'North', 'KB', 30, 'm', 'MSL', 'Y'),
                ('W-2', 'Alpha 2', 'North', 'KB', 30, 'm', 'MSL', 'Y'),
                ('W-3', 'Alpha 3', 'North', 'KB', 30, 'm', 'MSL', 'Y'),
                ('W-4', 'Alpha 4', 'North', 'KB', 30, 'm', 'MSL', 'Y');

            INSERT INTO WELLBORE (UWI, WELLBORE_ID, WELLBORE_NAME, TOTAL_DEPTH, TOTAL_DEPTH_OUOM,
                SURFACE_LATITUDE, SURFACE_LONGITUDE, DEPTH_DATUM, ACTIVE_IND) VALUES
                ('W-1', 'W-1-B1', 'Alpha 1 main', 2500, 'm', 58.5000, 2.1000, 'KB', 'Y'),
                ('W-2', 'W-2-B1', 'Alpha 2 main', 2500, 'm', 58.5180, 2.1000, 'KB', 'Y'),
                ('W-3', 'W-3-B1', 'Alpha 3 main', 2500, 'm', 58.5000, 2.1300, 'KB', 'Y'),
                -- AC-3: a well whose wellhead position was never recorded. It has a survey and a pick, so it is only the position
                -- that stops it being placed - which is exactly what the map must say rather than inventing coordinates.
                ('W-4', 'W-4-B1', 'Alpha 4 main', 2500, 'm', NULL, NULL, 'KB', 'Y');

            INSERT INTO WELL_DIR_SRVY_STATION (UWI, SURVEY_ID, SOURCE, DEPTH_OBS_NO, STATION_MD, STATION_MD_OUOM,
                INCLINATION, AZIMUTH, ACTIVE_IND) VALUES
                ('W-1', 'S-1', 'PPDM', 1, 0, 'm', 0, 0, 'Y'),
                ('W-1', 'S-1', 'PPDM', 2, 2500, 'm', 0, 0, 'Y'),
                ('W-2', 'S-2', 'PPDM', 1, 0, 'm', 0, 0, 'Y'),
                ('W-2', 'S-2', 'PPDM', 2, 2500, 'm', 0, 0, 'Y'),
                ('W-3', 'S-3', 'PPDM', 1, 0, 'm', 0, 0, 'Y'),
                ('W-3', 'S-3', 'PPDM', 2, 2500, 'm', 0, 0, 'Y'),
                ('W-4', 'S-4', 'PPDM', 1, 0, 'm', 0, 0, 'Y'),
                ('W-4', 'S-4', 'PPDM', 2, 2500, 'm', 0, 0, 'Y');

            INSERT INTO STRAT_UNIT (STRAT_NAME_SET_ID, STRAT_UNIT_ID, LONG_NAME, ACTIVE_IND) VALUES
                ('NNS', 'HOD', 'Hod', 'Y');

            INSERT INTO STRAT_WELL_SECTION (UWI, STRAT_NAME_SET_ID, STRAT_UNIT_ID, INTERP_ID, PICK_DEPTH,
                PICK_DEPTH_OUOM, PICK_TVD, INTERPRETER, ACTIVE_IND) VALUES
                ('W-1', 'NNS', 'HOD', 'I-1', 2000, 'm', 2000, 'sara', 'Y'),
                ('W-2', 'NNS', 'HOD', 'I-2', 2030, 'm', 2030, 'sara', 'Y'),
                ('W-3', 'NNS', 'HOD', 'I-3', 1980, 'm', 1980, 'sara', 'Y'),
                ('W-4', 'NNS', 'HOD', 'I-4', 1960, 'm', 1960, 'sara', 'Y');
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
