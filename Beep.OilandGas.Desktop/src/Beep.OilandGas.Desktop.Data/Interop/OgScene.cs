using System.Globalization;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.OilGas.Mapping;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.LifeCycle.Data.Tables;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;

/// <summary>A picked cell of the scene's surface (INT-08, AC-2).</summary>
/// <param name="Value">The surface's value at the node - the depth below sea level, in the map's unit.</param>
/// <param name="Elevation">The same depth as an elevation in metres, which is the coordinate the scene is drawn in.</param>
/// <param name="ScreenDistance">How far the node is from the click, in points - so a near miss can be judged rather than hidden.</param>
public sealed record OgScenePick(
    double Easting,
    double Northing,
    double Value,
    double Elevation,
    string Unit,
    double ScreenDistance)
{
    public string Describe() => Unit.Length == 0
        ? OgSceneRules.Figure(Value) + " at " + OgSceneRules.Figure(Easting) + ", " + OgSceneRules.Figure(Northing)
        : OgSceneRules.Figure(Value) + " " + Unit + " below sea level at " +
          OgSceneRules.Figure(Easting) + ", " + OgSceneRules.Figure(Northing) + " · elevation " +
          OgSceneRules.Figure(Elevation) + " m · " + OgSceneRules.Figure(ScreenDistance) + " pt from the click";
}

/// <summary>What was exported (INT-08, AC-3 and AC-4).</summary>
public sealed record OgSceneExport(
    string Format,
    string Path,
    long Bytes,
    double VerticalExaggeration,
    bool Written,
    IReadOnlyList<string> Warnings,
    string Message)
{
    public string Summary => Format.ToUpperInvariant() + " · " +
                             (Bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB · vertical exaggeration ×" +
                             VerticalExaggeration.ToString("0.##", CultureInfo.InvariantCulture) + " · " + Path +
                             (Written ? string.Empty : " (not written)");

    public string WarningsText => Warnings.Count == 0
        ? "The export raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));
}

/// <summary>What the scene is asked for (INT-08's <c>OgSceneRequest</c>).</summary>
/// <param name="VerticalExaggeration">Applied to the camera and recorded in the export; 1 is true scale.</param>
public sealed record OgSceneRequest(
    string? Field,
    string TopName,
    string Projection,
    string Method,
    double VerticalExaggeration,
    int MaxWells,
    IProgress<(int done, int total)>? Progress = null);

/// <summary>The 3D workspace (INT-08).</summary>
public sealed record OgSceneWorkspace(
    string Projection,
    string ValueDescription,
    string ValueUnit,
    bool SceneBuilt,
    string? Reason,
    IReadOnlyList<OgMapWell> Wells,
    int Included,
    int Excluded,
    int SurfaceCells,
    int DrawnCells,
    int Stride,
    int WellPaths,
    int PickCount,
    string BoundsText,
    IReadOnlyList<string> Notes,
    double VerticalExaggeration,
    double LightAzimuth,
    double LightElevation,
    double SurfaceOpacity,
    double Ambient,
    double Diffuse,
    Scene3D? Scene,
    Scene3DView? View,
    DateTime ReadAt)
{
    public string SummaryText => !SceneBuilt
        ? "No scene: " + (Reason ?? "the data does not support one") + "."
        : Included.ToString(CultureInfo.InvariantCulture) + " well(s) over " + ValueDescription.ToLowerInvariant() +
          " · " + WellPaths.ToString(CultureInfo.InvariantCulture) + " path(s) · " +
          PickCount.ToString(CultureInfo.InvariantCulture) + " pick(s) · vertical exaggeration ×" +
          VerticalExaggeration.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// AC-5: the thinning the view will apply, stated rather than left in the drawing code. The unit is the cell, because that is the
    /// unit the view draws and the unit its limit is expressed in - a node count would be a different number for the same picture.
    /// </summary>
    public string DecimationText => Stride <= 1
        ? "No decimation: the surface's " + SurfaceCells.ToString(CultureInfo.InvariantCulture) +
          " cell(s) are all drawn (the view's limit is " +
          OgSceneRules.MaxSurfaceCells.ToString(CultureInfo.InvariantCulture) + ")."
        : "Decimated: the surface's " + SurfaceCells.ToString(CultureInfo.InvariantCulture) +
          " cell(s) are drawn as at most " + DrawnCells.ToString(CultureInfo.InvariantCulture) +
          ", every " + Stride.ToString(CultureInfo.InvariantCulture) + "th node, because the view draws at most " +
          OgSceneRules.MaxSurfaceCells.ToString(CultureInfo.InvariantCulture) + " cells.";

    public string LightingText =>
        "Light from azimuth " + LightAzimuth.ToString("0.#", CultureInfo.InvariantCulture) + "°, " +
        LightElevation.ToString("0.#", CultureInfo.InvariantCulture) + "° above the horizon; ambient " +
        Ambient.ToString("0.##", CultureInfo.InvariantCulture) + " and diffuse " +
        Diffuse.ToString("0.##", CultureInfo.InvariantCulture) + " as the engine's shading fixes them; surface opacity " +
        SurfaceOpacity.ToString("0.##", CultureInfo.InvariantCulture) + ".";

    public string WellsText => Wells.Count == 0
        ? "No well is in scope."
        : string.Join(Environment.NewLine, Wells.Select(well => well.Describe()));

    public string ExcludedText => Excluded == 0
        ? "Every well in scope is in the scene."
        : string.Join(Environment.NewLine, Wells.Where(well => !well.Included)
            .Select(well => "✗ " + well.Well + ": " + (well.Reason ?? "no reason given")));

    public string NotesText => Notes.Count == 0
        ? "The scene raised no note."
        : string.Join(Environment.NewLine, Notes.Select(note => "! " + note));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The scene rules INT-08 states once.</summary>
public static class OgSceneRules
{
    public const string CodeNoConnection = "scene-no-connection";
    public const string CodeReadFailed = "scene-read-failed";
    public const string CodeTopRequired = "scene-top-required";
    public const string CodeProjectionRequired = "scene-projection-required";
    public const string CodeProjectionUnknown = "scene-projection-unknown";
    public const string CodeMethodUnknown = "scene-method-unknown";
    public const string CodeExaggeration = "scene-exaggeration";
    public const string CodeTooFewWells = "scene-too-few-wells";
    public const string CodeNoScene = "scene-not-built";
    public const string CodePickMiss = "scene-pick-miss";
    public const string CodeExportFailed = "scene-export-failed";
    public const string CodeExportPath = "scene-export-path";
    public const string CodeFormatUnknown = "scene-export-format";

    public static IReadOnlyList<string> Formats { get; } = ["glb", "gltf"];

    /// <summary>The most cells the view draws, read from the engine's own setting so the page cannot disagree with the drawing.</summary>
    public static int MaxSurfaceCells => MappingToolbox.Settings.MaxSurfaceCells;

    /// <summary>
    /// The engine's shading constants: 0.3 ambient plus 0.7 diffuse, written into the view. They are stated rather than offered as
    /// settings, because the view does not read them from anywhere - changing them would mean editing the drawing code.
    /// </summary>
    public const double Ambient = 0.3;

    public const double Diffuse = 0.7;

    /// <summary>The exaggeration range the page offers: 1 is true scale and beyond about 20 the relief is a caricature.</summary>
    public const double MinExaggeration = 1;

    public const double MaxExaggeration = 20;

    /// <summary>AC-2's tolerance: how far from the click a node may be and still be the picked one.</summary>
    public const double PickTolerancePoints = 25;

    /// <summary>
    /// The stride the view thins a surface by: the same formula its drawing uses, so the page's statement and the drawing agree.
    /// </summary>
    public static int StrideFor(GridSpec spec, int maxCells = 0)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var limit = maxCells > 0 ? maxCells : MaxSurfaceCells;
        var cells = (spec.Nx - 1.0) * (spec.Ny - 1.0);

        if (cells <= limit)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(Math.Sqrt(cells / limit)));
    }

    /// <summary>How many cells the view actually draws at that stride.</summary>
    public static int DrawnCells(GridSpec spec, int stride)
    {
        ArgumentNullException.ThrowIfNull(spec);

        var step = Math.Max(1, stride);
        var nx = (int)Math.Ceiling((spec.Nx - 1.0) / step);
        var ny = (int)Math.Ceiling((spec.Ny - 1.0) / step);

        return nx * ny;
    }

    /// <summary>
    /// AC-2: the nearest drawn node to a click, or null when the click is further from every node than the tolerance - the feature's
    /// "picking misses: no readout rather than a wrong one".
    /// </summary>
    public static OgScenePick? PickAt(
        SceneSurface surface,
        Camera3D camera,
        double width,
        double height,
        double verticalExaggeration,
        double x,
        double y,
        double tolerance = PickTolerancePoints)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(camera);

        var grid = surface.Depths;
        var spec = grid.Spec;
        var stride = StrideFor(spec);

        OgScenePick? best = null;
        var closest = double.MaxValue;

        for (var j = 0; j < spec.Ny; j += stride)
        {
            for (var i = 0; i < spec.Nx; i += stride)
            {
                var value = grid[i, j];

                if (double.IsNaN(value))
                {
                    continue;
                }

                var elevation = -value * surface.MetresPerUnit;

                if (camera.Project(grid.X(i), grid.Y(j), elevation, width, height, verticalExaggeration) is not { } point)
                {
                    continue;
                }

                var distance = Math.Sqrt((point.X - x) * (point.X - x) + (point.Y - y) * (point.Y - y));

                if (distance >= closest)
                {
                    continue;
                }

                closest = distance;
                best = new OgScenePick(grid.X(i), grid.Y(j), value, elevation, surface.ValueUnit, distance);
            }
        }

        return best is not null && closest <= tolerance ? best : null;
    }

    /// <summary>
    /// AC-4's warning: an exaggerated scene is a taller file and a picture that is no longer true scale. Both are stated, because an
    /// export that leaves without saying either would be a surprise.
    /// </summary>
    public static IReadOnlyList<string> WarningsFor(double verticalExaggeration, long bytes)
    {
        var warnings = new List<string>();

        if (verticalExaggeration > 5)
        {
            warnings.Add(
                "the vertical exaggeration is ×" + verticalExaggeration.ToString("0.##", CultureInfo.InvariantCulture) +
                ": the geometry is not true scale, and a reader of the file must be told so - the exaggeration is in the root node's " +
                "extras and its scale, so a viewer that honours scale shows the same relief");
        }

        if (bytes > 20_000_000)
        {
            warnings.Add(
                "the file is " + (bytes / 1024d / 1024d).ToString("0.#", CultureInfo.InvariantCulture) +
                " MB: large for a viewer on a laptop, and the surface's nodes are the cause");
        }

        return warnings;
    }

    public static string Figure(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>INT-08: the field in three dimensions, and its glTF export.</summary>
public interface IOgScene
{
    /// <summary>The coordinate systems the map offers, with their round-trip error - INT-07's list, not a second one.</summary>
    Task<OgResult<IReadOnlyList<OgMapProjection>>> ProjectionsAsync(
        IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken cancellationToken = default);

    /// <summary>AC-1, AC-3, AC-5: grids the value, adds the well paths and the picks, and builds the view.</summary>
    Task<OgResult<OgSceneWorkspace>> BuildAsync(OgSceneRequest request, CancellationToken cancellationToken = default);

    /// <summary>AC-2: the value at the cell nearest a click, or a refusal rather than a wrong number.</summary>
    Task<OgResult<OgScenePick>> PickAsync(
        OgSceneWorkspace workspace, double x, double y, CancellationToken cancellationToken = default);

    /// <summary>AC-3 and AC-4: writes the scene as GLB or glTF with the exaggeration it is shown at.</summary>
    Task<OgResult<OgSceneExport>> ExportAsync(
        OgSceneWorkspace workspace, string format, string path, CancellationToken cancellationToken = default);
}

/// <summary>
/// INT-08 over the tools' <see cref="Scene3D"/>, <see cref="Scene3DView"/>, <see cref="Camera3D"/> and <see cref="SceneExport"/>,
/// with the surface from <see cref="Gridding"/> and the wells placed as INT-07 places them.
/// </summary>
/// <remarks>
/// <para>
/// The scene is the engine's: <see cref="Gridding.Run"/> makes the surface, <see cref="Scene3D.AddSurface"/> converts it to elevation,
/// <see cref="Scene3D.AddWells"/> places the paths through <c>WellMaps.Paths</c>, <see cref="Scene3D.AddPicks"/> marks the top, and
/// <see cref="SceneExport"/> writes the glTF. The desktop adds the well models assembled from PPDM, the refusal rules, the picking
/// arithmetic, the decimation statement and the record.
/// </para>
/// <para>
/// Picking is a nearest-drawn-node test: every node the view draws is projected through the same camera and the nearest to the click
/// inside a tolerance is the answer. That is the cell a person pointed at, and a click that lands on nothing returns nothing rather
/// than the closest node that happens to be far away.
/// </para>
/// </remarks>
public sealed class OgSceneService : IOgScene
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly IOgMap _maps;

    /// <summary>
    /// The scene over the same connection the map reads, taking INT-07's service so the projection list is the map's own rather than a
    /// second implementation of the same question.
    /// </summary>
    public OgSceneService(Func<IDataSource?> source, IFailureReporter failures, IOgMap maps)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _maps = maps ?? throw new ArgumentNullException(nameof(maps));
    }

    public Task<OgResult<IReadOnlyList<OgMapProjection>>> ProjectionsAsync(
        IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken cancellationToken = default) =>
        _maps.ProjectionsAsync(points, cancellationToken);

    public async Task<OgResult<OgSceneWorkspace>> BuildAsync(
        OgSceneRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TopName))
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "A scene needs the value to draw: name the formation top.", OgSceneRules.CodeTopRequired);
        }

        if (string.IsNullOrWhiteSpace(request.Projection))
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "A scene is drawn in a coordinate system, and none was named.", OgSceneRules.CodeProjectionRequired);
        }

        if (!OgMapRules.IsKnownProjection(request.Projection))
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "'" + request.Projection.Trim() + "' is not a coordinate system this environment defines, so the scene cannot be " +
                "projected. Choose one of the systems the map page lists.", OgSceneRules.CodeProjectionUnknown);
        }

        if (OgMapRules.MethodOf(request.Method) is not { } method)
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "'" + request.Method + "' is not a gridding method; use " + string.Join(", ", OgMapRules.Methods) + ".",
                OgSceneRules.CodeMethodUnknown);
        }

        var exaggeration = request.VerticalExaggeration;

        if (double.IsNaN(exaggeration) || double.IsInfinity(exaggeration) ||
            exaggeration < OgSceneRules.MinExaggeration || exaggeration > OgSceneRules.MaxExaggeration)
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "The vertical exaggeration is " + OgSceneRules.Figure(exaggeration) + "; it must be between " +
                OgSceneRules.Figure(OgSceneRules.MinExaggeration) + " and " +
                OgSceneRules.Figure(OgSceneRules.MaxExaggeration) + ", because a scene stretched further no longer reads as relief.",
                OgSceneRules.CodeExaggeration);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgSceneWorkspace>.Failed(
                "No connection is open, so the wells cannot be read.", OgSceneRules.CodeNoConnection);
        }

        try
        {
            var system = CoordinateSystems.Get(request.Projection.Trim());
            var models = await ReadWellsAsync(source, request, cancellationToken).ConfigureAwait(false);

            if (models.Count == 0)
            {
                return OgResult<OgSceneWorkspace>.Failed(
                    "No well with a recorded survey and a top named '" + request.TopName.Trim() + "' is in scope, so there is " +
                    "nothing to draw.", OgSceneRules.CodeTooFewWells);
            }

            var map = WellMaps.Structure(models, request.TopName.Trim());
            var wells = map.Rows.Select(row => new OgMapWell(
                row.Well, row.Included, row.Reason, row.X, row.Y, row.Value, row.Md, row.Tvd, row.Notes.ToList())).ToList();

            var included = wells.Count(well => well.Included);

            if (included < OgMapRules.MinimumWells)
            {
                return OgResult<OgSceneWorkspace>.Ok(new OgSceneWorkspace(
                    system.Name,
                    map.Description,
                    map.ValueUnit,
                    SceneBuilt: false,
                    "a surface needs at least " + OgMapRules.MinimumWells + " placed wells and only " +
                    included.ToString(CultureInfo.InvariantCulture) + " could be placed",
                    wells,
                    included,
                    wells.Count - included,
                    0, 0, 1, 0, 0,
                    "(no scene)",
                    map.Notes.ToList(),
                    exaggeration,
                    MappingToolbox.Settings.LightAzimuth,
                    MappingToolbox.Settings.LightElevation,
                    MappingToolbox.Settings.SurfaceOpacity,
                    OgSceneRules.Ambient,
                    OgSceneRules.Diffuse,
                    null,
                    null,
                    DateTime.UtcNow));
            }

            var gridded = Gridding.Run(map.Points(), method);
            var spec = gridded.Grid.Spec;

            // The scene is the engine's: the surface as elevation, the paths through the map's own placing, the picks on the top.
            var scene = new Scene3D();
            scene.AddSurface(gridded.Grid, map.ValueUnit, map.Description);
            scene.AddWells(models);
            scene.AddPicks(models, request.TopName.Trim());

            var stride = OgSceneRules.StrideFor(spec);

            var view = new Scene3DView
            {
                Scene = scene,
                Title = map.Description + " in 3D · " + system.Name,
                VerticalExaggeration = exaggeration,
                ShowAxes = true,
                ShowWellLabels = true,
                ShowPicks = true,
                ShowLegend = true,
                Width = 920,
                Height = 660,
            };

            view.FitToScene();

            var notes = new List<string>(gridded.Notes);
            notes.AddRange(map.Notes);
            notes.AddRange(scene.Notes);

            var bounds = scene.Bounds();

            return OgResult<OgSceneWorkspace>.Ok(new OgSceneWorkspace(
                system.Name,
                map.Description,
                map.ValueUnit,
                SceneBuilt: true,
                null,
                wells,
                included,
                wells.Count - included,
                (spec.Nx - 1) * (spec.Ny - 1),
                OgSceneRules.DrawnCells(spec, stride),
                stride,
                scene.Wells.Count,
                scene.Picks.Count,
                bounds is { } box
                    ? OgSceneRules.Figure(box.MaxX - box.MinX) + " x " + OgSceneRules.Figure(box.MaxY - box.MinY) +
                      " m, elevation " + OgSceneRules.Figure(box.MinZ) + " to " + OgSceneRules.Figure(box.MaxZ) + " m"
                    : "(no extent)",
                notes.Distinct(StringComparer.Ordinal).ToList(),
                exaggeration,
                MappingToolbox.Settings.LightAzimuth,
                MappingToolbox.Settings.LightElevation,
                MappingToolbox.Settings.SurfaceOpacity,
                OgSceneRules.Ambient,
                OgSceneRules.Diffuse,
                scene,
                view,
                DateTime.UtcNow));
        }
        catch (ArgumentException ex)
        {
            // The engine refuses a set of points it cannot grid as an ordinary answer.
            var reference = _failures.ReportHandled(
                ex, "building the 3D scene for " + request.TopName, "the scene is not built", FailureSeverity.Degraded);

            return OgResult<OgSceneWorkspace>.Failed(
                "The scene could not be built from these wells: check that they are not all on one line and that the values are not " +
                "all identical. (reference " + reference + ")", OgSceneRules.CodeTooFewWells);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "building the 3D scene for " + request.TopName, "the scene is not shown", FailureSeverity.Error);

            return OgResult<OgSceneWorkspace>.Failed(
                "The scene could not be built" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgSceneRules.CodeReadFailed);
        }
    }

    public Task<OgResult<OgScenePick>> PickAsync(
        OgSceneWorkspace workspace, double x, double y, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (!workspace.SceneBuilt || workspace.Scene is null || workspace.View is null)
        {
            return Task.FromResult(OgResult<OgScenePick>.Failed(
                "There is no scene to pick in: build one first.", OgSceneRules.CodeNoScene));
        }

        var surface = workspace.Scene.Surfaces.FirstOrDefault();

        if (surface is null)
        {
            return Task.FromResult(OgResult<OgScenePick>.Failed(
                "The scene has no surface, so there is nothing to pick.", OgSceneRules.CodeNoScene));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var picked = OgSceneRules.PickAt(
            surface,
            workspace.View.Camera,
            workspace.View.Width,
            workspace.View.Height,
            workspace.VerticalExaggeration,
            x,
            y);

        // AC-2 and the feature's own state: a click that lands on nothing returns nothing, rather than the nearest node that happens
        // to be somewhere else.
        return Task.FromResult(picked is null
            ? OgResult<OgScenePick>.Failed(
                "The click did not land on the surface: no cell is within " +
                OgSceneRules.Figure(OgSceneRules.PickTolerancePoints) +
                " points of it, so no value is reported.", OgSceneRules.CodePickMiss)
            : OgResult<OgScenePick>.Ok(picked));
    }

    public Task<OgResult<OgSceneExport>> ExportAsync(
        OgSceneWorkspace workspace, string format, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        if (!workspace.SceneBuilt || workspace.Scene is null)
        {
            return Task.FromResult(OgResult<OgSceneExport>.Failed(
                "There is no scene to export: the last build could not place enough wells.", OgSceneRules.CodeNoScene));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(OgResult<OgSceneExport>.Failed(
                "An export writes a file; no path was given.", OgSceneRules.CodeExportPath));
        }

        var chosen = (format ?? string.Empty).Trim().ToLowerInvariant();

        if (!OgSceneRules.Formats.Contains(chosen, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(OgResult<OgSceneExport>.Failed(
                "'" + format + "' is not an export format; use " + string.Join(", ", OgSceneRules.Formats) + ".",
                OgSceneRules.CodeFormatUnknown));
        }

        try
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(path));

            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var file = File.Create(path))
            {
                // AC-3: the exaggeration the scene is shown at is the one written, so the export is the picture on the screen.
                if (chosen == "glb")
                {
                    SceneExport.ToGlb(workspace.Scene, file, workspace.VerticalExaggeration);
                }
                else
                {
                    SceneExport.ToGltf(workspace.Scene, file, workspace.VerticalExaggeration);
                }
            }

            var bytes = new FileInfo(path).Length;
            var warnings = OgSceneRules.WarningsFor(workspace.VerticalExaggeration, bytes);

            return Task.FromResult(OgResult<OgSceneExport>.Ok(new OgSceneExport(
                chosen,
                path,
                bytes,
                workspace.VerticalExaggeration,
                true,
                warnings,
                chosen.ToUpperInvariant() + " written: " + bytes.ToString(CultureInfo.InvariantCulture) + " bytes")));
        }
        catch (ArgumentException ex)
        {
            // The exporter refuses an empty scene or an exaggeration that is not positive, as ordinary answers.
            var reference = _failures.ReportHandled(
                ex, "exporting the scene to " + chosen, "the file is not written", FailureSeverity.Degraded);

            return Task.FromResult(OgResult<OgSceneExport>.Failed(
                "The scene could not be written: it has nothing the exporter can draw. (reference " + reference + ")",
                OgSceneRules.CodeExportFailed));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "exporting the scene to " + chosen, "the file is not written", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgSceneExport>.Failed(
                "The scene could not be written to '" + path + "'" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgSceneRules.CodeExportFailed));
        }
    }

    // ------------------------------------------------------------------ the wells

    /// <summary>
    /// Builds the well model the engine reads from the PPDM tables - the header, its wellhead position, the datum and its elevation, the
    /// survey and the picks - exactly as the map does, so a well appears in the same place in both.
    /// </summary>
    private Task<List<WellModel>> ReadWellsAsync(
        IDataSource source, OgSceneRequest request, CancellationToken cancellationToken) =>
        OgWellModelReader.ReadAsync(source, request.Field, request.MaxWells, request.Progress, cancellationToken);

    private static string UnitOf(string? uom) =>
        string.IsNullOrWhiteSpace(uom) ? WellboreEnvironment.Current.Units.DepthUnit : uom.Trim();
}
