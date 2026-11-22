using System.Text.Json;
using Microsoft.Data.Sqlite;
using SkiaSharp;
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
/// INT-08 against a real SQLite database: a surface with three wells is built and the camera frames it (AC-1), a click on a node
/// returns that node's value and a click on nothing returns nothing (AC-2), the exaggeration is applied to the view and written into
/// the file (AC-3), the GLB is a glTF 2.0 container an external viewer can open (AC-4), and the thinning the view applies is stated
/// with the numbers behind it (AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class SceneGatewayTests
{
    private const string ConnectionName = "int08-scene";

    [IntegrationFact]
    public async Task A_field_becomes_a_scene_and_its_glb_carries_the_exaggeration()
    {        var root = Path.Combine(OgPaths.DataRoot, "int08-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasInt08"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "scene.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "scene.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var maps = new OgMapService(() => source, reporter);
        var service = new OgSceneService(() => source, reporter, maps);

        // ---- AC-1 and AC-5: the scene, its paths, its picks and its decimation ----
        var built = await service.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", 1, 100));

        Assert.True(built.Success, built.Message + " :: " + string.Join(" | ", reporter.Failures));

        var scene = built.Value!;

        Assert.True(scene.SceneBuilt, scene.SummaryText);
        Assert.Equal("utm:31N", scene.Projection);
        Assert.Equal(3, scene.Included);
        Assert.Equal(1, scene.Excluded);
        Assert.Contains("Alpha 4", scene.ExcludedText, StringComparison.Ordinal);

        Assert.NotNull(scene.Scene);
        Assert.Equal(3, scene.Scene!.Wells.Count);
        Assert.Equal(3, scene.Scene.Picks.Count);
        Assert.Single(scene.Scene.Surfaces);

        Assert.Equal(3, scene.WellPaths);
        Assert.Equal(3, scene.PickCount);

        // The extent is the field's own, and the elevation range is the subsea depth of the top.
        Assert.Contains(" m, elevation ", scene.BoundsText, StringComparison.Ordinal);

        // AC-5: a small grid is drawn whole, and the sentence says so with the numbers.
        Assert.Equal(1, scene.Stride);
        Assert.Equal(scene.SurfaceCells, scene.DrawnCells);
        Assert.Contains("No decimation", scene.DecimationText, StringComparison.Ordinal);

        // The view is the owner's, framed on the scene, with the exaggeration the request asked for.
        Assert.NotNull(scene.View);
        Assert.Equal(1d, scene.View!.VerticalExaggeration, 9);
        Assert.True(scene.View.ShowLegend);
        Assert.True(scene.View.ShowPicks);
        Assert.True(scene.View.ShowWellLabels);
        Assert.Contains("Hod", scene.View.Title, StringComparison.Ordinal);
        Assert.Contains("utm:31N", scene.View.Title, StringComparison.Ordinal);

        // The lighting is stated, including the two constants the engine fixes rather than setting.
        Assert.Contains("azimuth", scene.LightingText, StringComparison.Ordinal);
        Assert.Contains("ambient 0.3", scene.LightingText, StringComparison.Ordinal);
        Assert.Contains("diffuse 0.7", scene.LightingText, StringComparison.Ordinal);

        // ---- AC-2: a click on a node reads that node; a click on nothing reads nothing ----
        var surface = scene.Scene.Surfaces.Single();
        var pick = FirstNodeScreenPoint(scene, surface);

        var picked = await service.PickAsync(scene, pick.X, pick.Y);

        Assert.True(picked.Success, picked.Message);

        var grid = surface.Depths;
        var node = NearestNode(grid, picked.Value!.Easting, picked.Value.Northing);

        Assert.Equal(grid[node.i, node.j], picked.Value.Value, 6);

        // The value is the depth in the map's unit; the elevation is the same depth in metres, which is the scene's own axis - so the
        // conversion is the surface's, not a second guess.
        Assert.Equal(-grid[node.i, node.j] * surface.MetresPerUnit, picked.Value.Elevation, 6);
        Assert.Contains("below sea level", picked.Value.Describe(), StringComparison.Ordinal);
        Assert.True(picked.Value.ScreenDistance < 1, picked.Value.Describe());

        var miss = await service.PickAsync(scene, 0.5, 0.5);

        Assert.False(miss.Success);
        Assert.Equal(OgSceneRules.CodePickMiss, miss.Reference);
        Assert.Contains("did not land on the surface", miss.Message!, StringComparison.Ordinal);

        // ---- AC-3: the exaggeration is on the view, and the geometry it draws moves with it ----
        var exaggerated = await service.BuildAsync(
            new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", 8, 100));

        Assert.True(exaggerated.Success, exaggerated.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(8d, exaggerated.Value!.View!.VerticalExaggeration, 9);
        Assert.Equal(8d, exaggerated.Value.VerticalExaggeration, 9);

        // The same node sits at a different height on the screen once the scene is stretched.
        var view = exaggerated.Value.View;
        var atScale = scene.View!.Camera.Project(grid.X(node.i), grid.Y(node.j),
            -grid[node.i, node.j] * surface.MetresPerUnit, scene.View.Width, scene.View.Height, 1);

        var stretched = view.Camera.Project(grid.X(node.i), grid.Y(node.j),
            -grid[node.i, node.j] * surface.MetresPerUnit, view.Width, view.Height, 8);

        Assert.NotNull(atScale);
        Assert.NotNull(stretched);
        Assert.NotEqual(atScale!.Value.Y, stretched!.Value.Y, 2);

        // ---- AC-3 and AC-4: the file is a glTF 2.0 binary, and it records the exaggeration it was drawn at ----
        var folder = Path.Combine(root, "out");
        Directory.CreateDirectory(folder);

        var path = Path.Combine(folder, "scene.glb");
        var exported = await service.ExportAsync(exaggerated.Value, "glb", path);

        Assert.True(exported.Success, exported.Message);
        Assert.True(exported.Value!.Written);
        Assert.Equal(8d, exported.Value.VerticalExaggeration, 9);
        Assert.Contains("×8", exported.Value.Summary, StringComparison.Ordinal);

        // The exaggeration past five warns, and the warning is on the export rather than left unsaid.
        Assert.Contains(exported.Value.Warnings, warning => warning.Contains("not true scale", StringComparison.Ordinal));
        Assert.Contains("vertical exaggeration", exported.Value.WarningsText, StringComparison.OrdinalIgnoreCase);

        var file = File.ReadAllBytes(path);

        Assert.Equal(0x46546C67u, BitConverter.ToUInt32(file, 0));
        Assert.Equal(2u, BitConverter.ToUInt32(file, 4));
        Assert.Equal((uint)file.Length, BitConverter.ToUInt32(file, 8));

        var jsonLength = (int)BitConverter.ToUInt32(file, 12);

        Assert.Equal(0x4E4F534Au, BitConverter.ToUInt32(file, 16));

        using var document = JsonDocument.Parse(file.AsMemory(20, jsonLength));
        var rootNode = document.RootElement.GetProperty("nodes")[0];

        Assert.Equal(8d, rootNode.GetProperty("extras").GetProperty("verticalExaggeration").GetDouble(), 9);
        Assert.Equal("utm:31N", rootNode.GetProperty("extras").GetProperty("coordinateSystem").GetString());
        Assert.Equal(8d, rootNode.GetProperty("scale")[1].GetDouble(), 9);

        // The surface, the wells and the picks are meshes in the file, so a viewer sees what the page showed.
        var meshes = document.RootElement.GetProperty("meshes").EnumerateArray()
            .Select(mesh => mesh.GetProperty("name").GetString())
            .ToList();

        Assert.Contains(meshes, name => name is not null && name.Contains("Hod", StringComparison.Ordinal));

        // Each well path is its own mesh, named for the well, and the picks are one node - so a viewer's scene tree reads like the
        // page's.
        Assert.Contains("Alpha 1", meshes);
        Assert.Contains("Alpha 2", meshes);
        Assert.Contains("Alpha 3", meshes);
        Assert.Contains("picks", meshes);

        // The JSON form carries the same scene with its buffer embedded.
        var gltf = Path.Combine(folder, "scene.gltf");
        var asJson = await service.ExportAsync(exaggerated.Value, "gltf", gltf);

        Assert.True(asJson.Success, asJson.Message);

        using var json = JsonDocument.Parse(File.ReadAllText(gltf));

        Assert.Equal("2.0", json.RootElement.GetProperty("asset").GetProperty("version").GetString());
        Assert.StartsWith("data:application/octet-stream;base64,",
            json.RootElement.GetProperty("buffers")[0].GetProperty("uri").GetString(), StringComparison.Ordinal);

        // ---- fewer than three wells is not a scene ----
        Execute(database, "DELETE FROM STRAT_WELL_SECTION WHERE UWI = 'W-3';");

        var tooFew = await service.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", 1, 100));

        Assert.True(tooFew.Success, tooFew.Message);
        Assert.False(tooFew.Value!.SceneBuilt);
        Assert.Contains("a surface needs at least 3 placed wells and only 2 could be placed",
            tooFew.Value.SummaryText, StringComparison.Ordinal);
        Assert.Null(tooFew.Value.Scene);

        Assert.Equal(OgSceneRules.CodeNoScene, (await service.PickAsync(tooFew.Value, 10, 10)).Reference);
        Assert.Equal(OgSceneRules.CodeNoScene,
            (await service.ExportAsync(tooFew.Value, "glb", Path.Combine(folder, "none.glb"))).Reference);

        // ---- and a failed read is a failure, not an empty scene ----
        Execute(database, "DROP TABLE WELL_DIR_SRVY_STATION;");

        var failed = await service.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", 1, 100));

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the pick helpers

    /// <summary>The screen position of the surface's middle node, through the view's own camera.</summary>
    private static SKPoint FirstNodeScreenPoint(OgSceneWorkspace workspace, TheTechIdea.OilGas.Mapping.SceneSurface surface)
    {
        var grid = surface.Depths;
        var i = grid.Spec.Nx / 2;
        var j = grid.Spec.Ny / 2;

        for (var attempt = 0; attempt < 4 && double.IsNaN(grid[i, j]); attempt++)
        {
            i = Math.Max(0, i - 1);
        }

        var point = workspace.View!.Camera.Project(grid.X(i), grid.Y(j), -grid[i, j] * surface.MetresPerUnit,
            workspace.View.Width, workspace.View.Height, workspace.VerticalExaggeration);

        Assert.NotNull(point);

        return new SKPoint((float)point!.Value.X, (float)point.Value.Y);
    }

    private static (int i, int j) NearestNode(TheTechIdea.OilGas.Mapping.Grid grid, double x, double y)
    {
        var spec = grid.Spec;
        var i = (int)Math.Round((x - spec.X0) / spec.Dx);
        var j = (int)Math.Round((y - spec.Y0) / spec.Dy);

        return (Math.Clamp(i, 0, spec.Nx - 1), Math.Clamp(j, 0, spec.Ny - 1));
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>As INT-07's: three vertical wells in UTM 31N with one pick each, plus a fourth with no wellhead position.</summary>
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
