using System.Text;
using System.Text.Json;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Mapping;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-08: the rules the page states - the decimation it reports (AC-5), the nearest-node pick and its miss (AC-2), the exaggeration
/// warning (AC-3) and the refusals - plus the glTF/GLB the owner's exporter writes, checked against its own container and accessors
/// (AC-4). The scene built from a database is the opt-in test; these need no well.
/// </summary>
public sealed class SceneTests
{
    private const string PageFileName = "ScenePageView.cs";

    /// <summary>A flat surface of n x n nodes, 100 m apart, at one depth below sea level.</summary>
    private static Grid Surface(int n = 3, double depth = 2000, double dx = 100)
    {
        var grid = new Grid(new GridSpec { X0 = 640000, Y0 = 5760000, Dx = dx, Dy = dx, Nx = n, Ny = n });

        for (var j = 0; j < n; j++)
        {
            for (var i = 0; i < n; i++)
            {
                grid[i, j] = depth + i * 10;
            }
        }

        return grid;
    }

    /// <summary>A camera framed on the grid, the way the view frames it on its first draw.</summary>
    private static (Camera3D Camera, SceneSurface Surface, Scene3D Scene) Framed(int n = 3, double ve = 1)
    {
        var scene = new Scene3D();
        var surface = scene.AddSurface(Surface(n), "m", "Top");
        var camera = new Camera3D();

        camera.Fit(scene.Bounds()!.Value, 800, 600, ve);

        return (camera, surface, scene);
    }

    [Fact]
    public void A_surface_over_the_views_limit_is_thinned_by_the_same_stride_the_view_uses()
    {
        // AC-5: the page's statement and the drawing must come from one rule, so the stride is the view's own formula.
        var small = new GridSpec { X0 = 0, Y0 = 0, Dx = 10, Dy = 10, Nx = 10, Ny = 10 };

        Assert.Equal(1, OgSceneRules.StrideFor(small));
        Assert.Equal(81, OgSceneRules.DrawnCells(small, 1));

        // A grid of 401 x 401 nodes is 160 000 cells, four times the engine's 40 000: the stride is two.
        var big = new GridSpec { X0 = 0, Y0 = 0, Dx = 1, Dy = 1, Nx = 401, Ny = 401 };

        var stride = OgSceneRules.StrideFor(big);

        Assert.Equal(2, stride);
        Assert.True(OgSceneRules.DrawnCells(big, stride) <= OgSceneRules.MaxSurfaceCells,
            OgSceneRules.DrawnCells(big, stride).ToString(System.Globalization.CultureInfo.InvariantCulture));

        // Whatever the grid, the drawn count stays inside the limit and the stride never drops below one.
        foreach (var n in new[] { 2, 51, 250, 999 })
        {
            var spec = new GridSpec { X0 = 0, Y0 = 0, Dx = 1, Dy = 1, Nx = n, Ny = n };
            var step = OgSceneRules.StrideFor(spec);

            Assert.True(step >= 1);
            Assert.True(OgSceneRules.DrawnCells(spec, step) <= OgSceneRules.MaxSurfaceCells,
                n + " nodes → stride " + step + " → " + OgSceneRules.DrawnCells(spec, step) + " cells");
        }

        // The limit is the engine's own setting, so the page cannot disagree with what is drawn.
        Assert.Equal(MappingToolbox.Settings.MaxSurfaceCells, OgSceneRules.MaxSurfaceCells);
    }

    [Fact]
    public void The_decimation_sentence_carries_the_numbers_it_was_computed_from()
    {
        var workspace = Workspace(SceneBuilt: true, surfaceCells: 160_801, drawnCells: 40_000, stride: 2);
        var text = workspace.DecimationText;

        Assert.Contains("Decimated", text, StringComparison.Ordinal);
        Assert.Contains("160801 cell(s)", text, StringComparison.Ordinal);
        Assert.Contains("40000", text, StringComparison.Ordinal);
        Assert.Contains("every 2th node", text, StringComparison.Ordinal);
        Assert.Contains("40000 cells", text, StringComparison.Ordinal);

        // A grid inside the limit says so plainly rather than claiming a decimation that did not happen.
        var plain = Workspace(SceneBuilt: true, surfaceCells: 81, drawnCells: 81, stride: 1);

        Assert.Contains("No decimation", plain.DecimationText, StringComparison.Ordinal);
        Assert.DoesNotContain("every", plain.DecimationText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_pick_returns_the_node_under_the_click_and_a_miss_returns_nothing()
    {
        // AC-2: the node is projected through the camera, so the click hits the cell a person pointed at.
        var (camera, surface, scene) = Framed();
        var grid = surface.Depths;
        var elevation = -grid[1, 1] * surface.MetresPerUnit;
        var point = camera.Project(grid.X(1), grid.Y(1), elevation, 800, 600, 1);

        Assert.NotNull(point);

        var picked = OgSceneRules.PickAt(surface, camera, 800, 600, 1, point!.Value.X, point.Value.Y);

        Assert.NotNull(picked);
        Assert.Equal(grid.X(1), picked!.Easting, 6);
        Assert.Equal(grid.Y(1), picked.Northing, 6);
        Assert.Equal(grid[1, 1], picked.Value, 9);
        Assert.Equal(-grid[1, 1], picked.Elevation, 9);
        Assert.Equal("m", picked.Unit);
        Assert.True(picked.ScreenDistance < 1e-6);

        // A click in a corner of the view is far from every node, so nothing is reported rather than the nearest node that happens to
        // be somewhere else.
        Assert.Null(OgSceneRules.PickAt(surface, camera, 800, 600, 1, 1, 1));

        // A masked node is not a value. Clicking where it was finds nothing inside the tolerance - the neighbouring nodes are further
        // away than 25 points at this zoom - and a click on a real neighbour still answers, so the mask neither invents a depth nor
        // blocks the surface around it.
        grid[1, 1] = double.NaN;

        Assert.Null(OgSceneRules.PickAt(surface, camera, 800, 600, 1, point.Value.X, point.Value.Y));

        var neighbour = camera.Project(grid.X(2), grid.Y(2), -grid[2, 2], 800, 600, 1)!.Value;
        var real = OgSceneRules.PickAt(surface, camera, 800, 600, 1, neighbour.X, neighbour.Y);

        Assert.NotNull(real);
        Assert.Equal(grid[2, 2], real!.Value, 9);
        Assert.False(double.IsNaN(real.Value));

        _ = scene;
    }

    [Fact]
    public void The_exaggeration_moves_the_geometry_and_the_export_warns_about_it()
    {
        // AC-3: an exaggerated elevation is higher on the screen than the same elevation at true scale.
        var (camera, surface, _) = Framed(ve: 1);
        var grid = surface.Depths;
        var atScale = camera.Project(grid.X(2), grid.Y(2), -grid[2, 2], 800, 600, 1)!.Value;
        var exaggerated = camera.Project(grid.X(2), grid.Y(2), -grid[2, 2], 800, 600, 3)!.Value;

        Assert.NotEqual(atScale.Y, exaggerated.Y, 3);

        // A scene stretched past five is warned about, and a large file is too; neither is silent.
        var mild = OgSceneRules.WarningsFor(1, 40_000);

        Assert.Empty(mild);

        var stretched = OgSceneRules.WarningsFor(8, 40_000);

        Assert.Single(stretched);
        Assert.Contains("not true scale", stretched[0], StringComparison.Ordinal);
        Assert.Contains("×8", stretched[0], StringComparison.Ordinal);
        Assert.Contains("extras", stretched[0], StringComparison.Ordinal);

        var large = OgSceneRules.WarningsFor(1, 30_000_000);

        Assert.Single(large);
        Assert.Contains("MB", large[0], StringComparison.Ordinal);

        Assert.Equal(2, OgSceneRules.WarningsFor(8, 30_000_000).Count);
    }

    [Fact]
    public void A_large_scene_is_drawn_through_the_decimation_and_in_reasonable_time()
    {
        // AC-6, measured rather than assumed: a 300 x 300 grid is 89 401 cells, more than twice the engine's limit, so the view thins
        // it - and the frame it draws is timed, because software rendering on the 2D canvas is where a large grid would hurt.
        var scene = new Scene3D();

        scene.AddSurface(Surface(300, 2000, 25), "m", "Top");

        var view = new Scene3DView { Scene = scene, Width = 900, Height = 600 };

        view.FitToScene();

        var spec = scene.Surfaces.Single().Depths.Spec;
        var stride = OgSceneRules.StrideFor(spec);
        var drawn = OgSceneRules.DrawnCells(spec, stride);

        Assert.Equal(299 * 299, (spec.Nx - 1) * (spec.Ny - 1));
        Assert.True(stride > 1, "a 300 x 300 grid must be thinned: stride " + stride);
        Assert.True(drawn <= OgSceneRules.MaxSurfaceCells, drawn.ToString(System.Globalization.CultureInfo.InvariantCulture));

        var watch = System.Diagnostics.Stopwatch.StartNew();
        string svg;

        using (var memory = new MemoryStream())
        {
            using (var canvas = SKSvgCanvas.Create(new SKRect(0, 0, view.Width, view.Height), memory))
            {
                view.Draw(canvas);
            }

            svg = System.Text.Encoding.UTF8.GetString(memory.ToArray());
        }

        watch.Stop();

        // The frame is a real drawing, and it took long enough to measure but not long enough to be a stall.
        Assert.Contains("<path", svg, StringComparison.Ordinal);
        Assert.True(watch.ElapsedMilliseconds < 5000,
            "drawing " + drawn + " cells of a " + spec.Nx + " x " + spec.Ny + " grid took " +
            watch.ElapsedMilliseconds + " ms"); 
    }

    [Fact]
    public async Task The_glb_is_the_specs_container_and_records_the_exaggeration()
    {
        // AC-4 and AC-3: the file is a glTF 2.0 binary - magic, version, a JSON chunk and a binary chunk that ends the file - and the
        // exaggeration is in the root node's extras and its scale, which is what a viewer reads.
        var scene = new Scene3D();

        scene.AddSurface(Surface(4, 2000), "m", "Top");
        scene.MapSystem = CoordinateSystems.Utm(31, north: true);

        var service = new OgSceneService(() => null, new Quiet(), new NoMap());
        var workspace = Workspace(SceneBuilt: true, surfaceCells: 16, drawnCells: 9, stride: 1) with
        {
            Scene = scene,
            VerticalExaggeration = 3,
        };

        var path = Path.Combine(Path.GetTempPath(), "int08-" + Guid.NewGuid().ToString("n")[..8] + ".glb");

        try
        {
            var exported = await service.ExportAsync(workspace, "glb", path);

            Assert.True(exported.Success, exported.Message);
            Assert.True(exported.Value!.Written);
            Assert.Equal(3d, exported.Value.VerticalExaggeration, 9);
            Assert.True(File.Exists(path));

            var file = File.ReadAllBytes(path);

            // the container
            Assert.Equal(0x46546C67u, BitConverter.ToUInt32(file, 0));                    // "glTF"
            Assert.Equal(2u, BitConverter.ToUInt32(file, 4));                             // version 2
            Assert.Equal((uint)file.Length, BitConverter.ToUInt32(file, 8));              // the declared length is the file's

            var jsonLength = (int)BitConverter.ToUInt32(file, 12);

            Assert.Equal(0x4E4F534Au, BitConverter.ToUInt32(file, 16));                   // "JSON"
            Assert.Equal(0, jsonLength % 4);

            var binAt = 20 + jsonLength;
            var binLength = (int)BitConverter.ToUInt32(file, binAt);

            Assert.Equal(0x004E4942u, BitConverter.ToUInt32(file, binAt + 4));            // "BIN\0"
            Assert.Equal(0, binLength % 4);
            Assert.Equal(file.Length, binAt + 8 + binLength);

            // the JSON the viewer reads
            using var document = JsonDocument.Parse(file.AsMemory(20, jsonLength));
            var root = document.RootElement;

            Assert.Equal("2.0", root.GetProperty("asset").GetProperty("version").GetString());

            var node = root.GetProperty("nodes")[0];

            Assert.Equal(3d, node.GetProperty("extras").GetProperty("verticalExaggeration").GetDouble(), 9);
            Assert.Equal("utm:31N", node.GetProperty("extras").GetProperty("coordinateSystem").GetString());
            Assert.Equal("metre", node.GetProperty("extras").GetProperty("units").GetString());

            var scale = node.GetProperty("scale");

            Assert.Equal(1d, scale[0].GetDouble(), 9);
            Assert.Equal(3d, scale[1].GetDouble(), 9);                                    // the exaggeration is the vertical scale
            Assert.Equal(1d, scale[2].GetDouble(), 9);

            // Every accessor resolves into the binary chunk, which is what an external loader checks first.
            var views = root.GetProperty("bufferViews");

            foreach (var accessor in root.GetProperty("accessors").EnumerateArray())
            {
                var view = views[accessor.GetProperty("bufferView").GetInt32()];
                var offset = view.GetProperty("byteOffset").GetInt32() +
                             (accessor.TryGetProperty("byteOffset", out var at) ? at.GetInt32() : 0);

                Assert.True(offset >= 0 && offset <= binLength, "accessor outside the buffer: " + offset);
            }

            Assert.True(root.GetProperty("meshes").GetArrayLength() > 0);

            // And the JSON form is the same scene, embedded as a data URI.
            var gltfPath = Path.ChangeExtension(path, ".gltf");
            var asJson = await service.ExportAsync(workspace, "gltf", gltfPath);

            Assert.True(asJson.Success, asJson.Message);

            using var json = JsonDocument.Parse(File.ReadAllText(gltfPath));

            Assert.Equal("2.0", json.RootElement.GetProperty("asset").GetProperty("version").GetString());
            Assert.StartsWith("data:application/octet-stream;base64,",
                json.RootElement.GetProperty("buffers")[0].GetProperty("uri").GetString(), StringComparison.Ordinal);

            File.Delete(gltfPath);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public async Task A_scene_is_refused_without_a_top_a_projection_a_known_method_or_a_sane_exaggeration()
    {
        var offline = new OgSceneService(() => null, new Quiet(), new NoMap());

        var noTop = await offline.BuildAsync(new OgSceneRequest(null, "  ", "utm:31N", "InverseDistance", 1, 100));

        Assert.False(noTop.Success);
        Assert.Equal(OgSceneRules.CodeTopRequired, noTop.Reference);
        Assert.Contains("name the formation top", noTop.Message!, StringComparison.Ordinal);

        var noProjection = await offline.BuildAsync(new OgSceneRequest(null, "Hod", " ", "InverseDistance", 1, 100));

        Assert.Equal(OgSceneRules.CodeProjectionRequired, noProjection.Reference);

        var badProjection = await offline.BuildAsync(new OgSceneRequest(null, "Hod", "EPSG:32631", "InverseDistance", 1, 100));

        Assert.Equal(OgSceneRules.CodeProjectionUnknown, badProjection.Reference);
        Assert.Contains("the map page lists", badProjection.Message!, StringComparison.Ordinal);

        var badMethod = await offline.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "triangulation", 1, 100));

        Assert.Equal(OgSceneRules.CodeMethodUnknown, badMethod.Reference);

        // The exaggeration is a picture's honesty, so a value outside the range is refused with the range rather than clamped silently.
        foreach (var wrong in new[] { 0d, -2d, 21d, double.NaN })
        {
            var refused = await offline.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", wrong, 100));

            Assert.Equal(OgSceneRules.CodeExaggeration, refused.Reference);
            Assert.Contains("must be between 1 and 20", refused.Message!, StringComparison.Ordinal);
        }

        var noConnection = await offline.BuildAsync(new OgSceneRequest(null, "Hod", "utm:31N", "InverseDistance", 1, 100));

        Assert.Equal(OgSceneRules.CodeNoConnection, noConnection.Reference);

        // An export needs a scene, a path and a format the exporter writes.
        var empty = Workspace(SceneBuilt: false, surfaceCells: 0, drawnCells: 0, stride: 1);

        Assert.Equal(OgSceneRules.CodeNoScene, (await offline.ExportAsync(empty, "glb", "scene.glb")).Reference);
        Assert.Equal(OgSceneRules.CodeExportPath,
            (await offline.ExportAsync(empty with { SceneBuilt = true, Scene = new Scene3D() }, "glb", " ")).Reference);
        Assert.Equal(OgSceneRules.CodeFormatUnknown,
            (await offline.ExportAsync(empty with { SceneBuilt = true, Scene = new Scene3D() }, "obj", "scene.obj")).Reference);

        // Picking before a scene exists is refused rather than answering with a coordinate of zero.
        Assert.Equal(OgSceneRules.CodeNoScene, (await offline.PickAsync(empty, 10, 10)).Reference);
    }

    [Fact]
    public void The_page_applies_the_slider_to_the_view_and_clears_it_when_a_build_fails()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-3: the slider's value goes to the view and is printed beside it.
        Assert.Contains("_vertical.ValueChanged += (_, _) => VerticalChanged();", page, StringComparison.Ordinal);
        Assert.Contains("view.VerticalExaggeration = _vertical.Value", page, StringComparison.Ordinal);
        Assert.Contains("view.FitToScene()", page, StringComparison.Ordinal);
        Assert.Contains("Minimum = OgSceneRules.MinExaggeration", page, StringComparison.Ordinal);
        Assert.Contains("Maximum = OgSceneRules.MaxExaggeration", page, StringComparison.Ordinal);

        // AC-2: the click is the pick, and a miss says so rather than showing the last value.
        Assert.Contains("_host.Clicked += (_, pointer) => _ = PickAsync(pointer.SurfacePoint);", page, StringComparison.Ordinal);
        Assert.Contains("await _scenes.PickAsync(_workspace, point.X, point.Y)", page, StringComparison.Ordinal);

        // AC-5 and the export warning are on the page rather than left in the service.
        Assert.Contains("workspace.DecimationText", page, StringComparison.Ordinal);
        Assert.Contains("exported.Value.WarningsText", page, StringComparison.Ordinal);

        // A failed build takes the last scene off the screen.
        Assert.Contains("_host.Surface = null", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private static OgSceneWorkspace Workspace(bool SceneBuilt, int surfaceCells, int drawnCells, int stride) =>
        new("utm:31N",
            "Hod (TVDSS)",
            "m",
            SceneBuilt,
            SceneBuilt ? null : "no surface",
            [],
            0,
            0,
            surfaceCells,
            drawnCells,
            stride,
            0,
            0,
            "(no extent)",
            [],
            1,
            315,
            45,
            0.85,
            OgSceneRules.Ambient,
            OgSceneRules.Diffuse,
            null,
            null,
            DateTime.UtcNow);

    private sealed class NoMap : IOgMap
    {
        public Task<OgResult<IReadOnlyList<OgMapProjection>>> ProjectionsAsync(
            IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<OgMapProjection>>.Ok([]));

        public Task<OgResult<OgMapWorkspace>> BuildAsync(OgMapRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgMapWorkspace>.Failed("not used", "not used"));

        public Task<OgResult<OgMapExport>> ExportAsync(OgMapWorkspace workspace, string format, string path,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgMapExport>.Failed("not used", "not used"));
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
