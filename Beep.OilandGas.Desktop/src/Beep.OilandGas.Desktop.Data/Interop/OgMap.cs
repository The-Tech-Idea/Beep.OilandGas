using System.Globalization;
using System.Text;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.OilGas.Mapping;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Beep.OilandGas.LifeCycle.Data.Tables;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;

/// <summary>One well on the map, with the table's own verdict on whether it was placed (INT-07).</summary>
public sealed record OgMapWell(
    string Well,
    bool Included,
    string? Reason,
    double X,
    double Y,
    double? Value,
    double? Md,
    double? Tvd,
    IReadOnlyList<string> Notes)
{
    public string Describe() => Well + " · " +
                                (Included
                                    ? OgMapRules.Figure(X) + ", " + OgMapRules.Figure(Y) + " m · value " +
                                      (Value is { } value ? OgMapRules.Figure(value) : "?")
                                    : "left out: " + (Reason ?? "no reason given"));
}

/// <summary>What the grid's own quality check found (INT-07, AC-4).</summary>
public sealed record OgMapQc(
    int Points,
    int Merged,
    int EmptyCells,
    double MaxExtrapolation,
    string ExtrapolationUnit,
    double MaxAbsResidual,
    double RmsResidual,
    bool OvershootsData,
    IReadOnlyList<string> ResidualWells)
{
    public string Describe() =>
        Points.ToString(CultureInfo.InvariantCulture) + " point(s) · " +
        EmptyCells.ToString(CultureInfo.InvariantCulture) + " empty cell(s) · farthest cell " +
        OgMapRules.Figure(MaxExtrapolation) + " " + ExtrapolationUnit + " from a datum · largest residual " +
        OgMapRules.Figure(MaxAbsResidual) + " (RMS " + OgMapRules.Figure(RmsResidual) + ")" +
        (OvershootsData ? " · the surface goes beyond the data's range" : string.Empty);
}

/// <summary>The contour levels the map was drawn at (INT-07, AC-5).</summary>
public sealed record OgMapContours(
    int Count,
    double Interval,
    double Min,
    double Max,
    bool FromNiceInterval,
    IReadOnlyList<string> Levels)
{
    public string Describe() =>
        Count.ToString(CultureInfo.InvariantCulture) + " contour(s) every " + OgMapRules.Figure(Interval) +
        " from " + OgMapRules.Figure(Min) + " to " + OgMapRules.Figure(Max) +
        (FromNiceInterval ? " (the engine's own interval)" : " (as set)");
}

/// <summary>A coordinate system the map can be drawn in, and how well it round-trips (INT-07, AC-2).</summary>
public sealed record OgMapProjection(string Name, string Method, string Axes, double RoundTripMetres, bool WithinTolerance)
{
    public string Describe() =>
        Name + " · " + Method + " · axes " + Axes + " · round trip " +
        OgMapRules.Figure(RoundTripMetres) + " m" + (WithinTolerance ? string.Empty : " - OUTSIDE the tolerance");
}

/// <summary>A built map (INT-07).</summary>
public sealed record OgMapWorkspace(
    string Projection,
    string ProjectionAxes,
    string ValueDescription,
    string ValueUnit,
    bool MapPossible,
    string? Reason,
    IReadOnlyList<OgMapWell> Wells,
    int Included,
    int Excluded,
    OgMapQc? Qc,
    OgMapContours? Contours,
    IReadOnlyList<string> GridNotes,
    IReadOnlyList<string> MapNotes,
    ContourMapView? View,
    double North,
    double East,
    DateTime ReadAt)
{
    public string SummaryText => !MapPossible
        ? "No map: " + (Reason ?? "the data does not support one") + "."
        : Included.ToString(CultureInfo.InvariantCulture) + " well(s) on the map of " +
          ValueDescription.ToLowerInvariant() + " · " + Excluded.ToString(CultureInfo.InvariantCulture) +
          " left out · " + (Contours?.Describe() ?? "no contours");

    public string WellsText => Wells.Count == 0
        ? "No well is in scope."
        : string.Join(Environment.NewLine, Wells.Select(well => well.Describe()));

    public string ExcludedText => Excluded == 0
        ? "Every well in scope was placed."
        : string.Join(Environment.NewLine, Wells.Where(well => !well.Included)
            .Select(well => "✗ " + well.Well + ": " + (well.Reason ?? "no reason given")));

    public string QcText => Qc?.Describe() ?? "No grid was built, so there is nothing to check.";

    public string NotesText => GridNotes.Concat(MapNotes).Distinct(StringComparer.Ordinal).DefaultIfEmpty("The map raised no note.")
        .Aggregate((left, right) => left + Environment.NewLine + "! " + right);

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>What was exported, and whether it was written (INT-07, AC-6).</summary>
public sealed record OgMapExport(string Format, string Path, long Bytes, bool Written, string Message)
{
    public string Summary => Format.ToUpperInvariant() + " · " + (Bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) +
                             " KB · " + Path + (Written ? string.Empty : " (not written)");
}

/// <summary>What the map is asked for (INT-07's <c>OgMapRequest</c>).</summary>
/// <param name="TopName">The formation top for a structure map, and the upper top for an isochore.</param>
/// <param name="LowerTop">The lower top when the value is the thickness between two tops.</param>
public sealed record OgMapRequest(
    string? Field,
    string TopName,
    string? LowerTop,
    string Projection,
    string Method,
    double? ContourInterval,
    int MaxWells,
    IProgress<(int done, int total)>? Progress = null);

/// <summary>The mapping rules INT-07 states once.</summary>
public static class OgMapRules
{
    public const string CodeNoConnection = "map-no-connection";
    public const string CodeReadFailed = "map-read-failed";
    public const string CodeProjectionRequired = "map-projection-required";
    public const string CodeProjectionUnknown = "map-projection-unknown";
    public const string CodeTooFewWells = "map-too-few-wells";
    public const string CodeTopRequired = "map-top-required";
    public const string CodeMethodUnknown = "map-method-unknown";
    public const string CodeExportFailed = "map-export-failed";
    public const string CodeExportPath = "map-export-path";
    public const string CodeFormatUnknown = "map-export-format";

    /// <summary>The methods the engine offers, in its own names.</summary>
    public static IReadOnlyList<string> Methods { get; } =
        ["InverseDistance", "MinimumCurvature", "OrdinaryKriging"];

    public static IReadOnlyList<string> Formats { get; } = ["pdf", "png", "svg"];

    /// <summary>The engine's own rule: a surface needs three points before it means anything.</summary>
    public const int MinimumWells = 3;

    /// <summary>AC-2's tolerance: a projection that cannot return the point it was given is not usable.</summary>
    public const double RoundTripToleranceMetres = 0.05;

    public static GriddingMethod? MethodOf(string? method) =>
        (method ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "inversedistance" or "idw" or "inverse distance" => GriddingMethod.InverseDistance,
            "minimumcurvature" or "mincurv" or "minimum curvature" => GriddingMethod.MinimumCurvature,
            "ordinarykriging" or "kriging" => GriddingMethod.OrdinaryKriging,
            _ => null,
        };

    /// <summary>AC-5: the levels are the engine's own, unless an interval was given.</summary>
    public static IReadOnlyList<double> LevelsOf(
        double min,
        double max,
        double? interval,
        string unit,
        out double used,
        out bool fromNiceInterval,
        int indexEvery = 5)
    {
        if (max <= min)
        {
            used = 0d;
            fromNiceInterval = false;
            return [];
        }

        var nice = Contouring.Interval(max - min, unit);

        if (interval is { } wanted && wanted > 0d)
        {
            used = wanted;
            fromNiceInterval = false;
            return Contouring.Levels(min, max, wanted, indexEvery).Select(level => level.Level).ToList();
        }

        // The engine's own interval: it decides how many lines a range deserves, and its result carries the index flags the view
        // draws heavier. Only the levels are returned here; the flags are the view's to read from the interval it re-derives.
        used = nice;
        fromNiceInterval = true;
        return Contouring.Levels(min, max, unit).Select(level => level.Level).ToList();
    }

    /// <summary>AC-2: the worst distance a projection moves a point when it converts it and back.</summary>
    public static double RoundTripMetres(CoordinateSystem system, IEnumerable<(double Latitude, double Longitude)> points)
    {
        ArgumentNullException.ThrowIfNull(system);
        ArgumentNullException.ThrowIfNull(points);

        var worst = 0d;

        foreach (var (latitude, longitude) in points)
        {
            // The system's own forward and reverse: the pair a map would actually use, rather than a transform built for one
            // direction and then asked to run backwards.
            var (x, y) = system.FromGeographic(latitude, longitude);
            var (backLatitude, backLongitude) = system.ToGeographic(x, y);

            // Degrees to metres at the point's own latitude: the round trip's error, not the projection's scale.
            const double metresPerDegreeLatitude = 111320d;
            var metresPerDegreeLongitude = metresPerDegreeLatitude * Math.Cos(latitude * Math.PI / 180d);

            var northError = Math.Abs(backLatitude - latitude) * metresPerDegreeLatitude;
            var eastError = Math.Abs(backLongitude - longitude) * metresPerDegreeLongitude;

            worst = Math.Max(worst, Math.Sqrt(northError * northError + eastError * eastError));
        }

        return worst;
    }

    /// <summary>
    /// Whether a name is a coordinate system the engine can resolve: one the environment defines, or a generated UTM zone or
    /// geographic system. Checked before the engine is asked, so a typo is a refusal rather than an exception used as one.
    /// </summary>
    public static bool IsKnownProjection(string? name)
    {
        var clean = (name ?? string.Empty).Trim();

        if (clean.Length == 0)
        {
            return false;
        }

        if (CoordinateSystems.DefinedNames().Contains(clean, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        var utm = System.Text.RegularExpressions.Regex.Match(
            clean,
            @"^utm:(\d{1,2})([NS])(:.+)?$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);

        if (utm.Success)
        {
            // There are sixty zones. A number outside them is not a system, and the engine would refuse it - so it is refused here,
            // where the answer is a sentence rather than an exception.
            return int.TryParse(utm.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var zone) &&
                   zone is >= 1 and <= 60;
        }

        return System.Text.RegularExpressions.Regex.IsMatch(
            clean,
            @"^geographic(:.+)?$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

    /// <summary>
    /// The UTM zones covering a set of positions: one per zone the points fall in, six degrees wide, named as the engine names them
    /// (<c>utm:38N</c>). A field straddling a zone boundary is offered both, so neither half is mapped in the other's zone.
    /// </summary>
    public static IReadOnlyList<string> ZonesOf(IReadOnlyList<(double Latitude, double Longitude)> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        var zones = new List<string>();

        foreach (var (latitude, longitude) in points)
        {
            var zone = (int)Math.Floor((longitude + 180d) / 6d) + 1;

            zone = Math.Clamp(zone, 1, 60);

            var name = "utm:" + zone.ToString(CultureInfo.InvariantCulture) + (latitude >= 0d ? "N" : "S");

            if (!zones.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                zones.Add(name);
            }
        }

        return zones
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string Figure(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>INT-07: the map of a value across the field.</summary>
public interface IOgMap
{
    /// <summary>The coordinate systems the engine defines, each with its round-trip error (AC-2).</summary>
    Task<OgResult<IReadOnlyList<OgMapProjection>>> ProjectionsAsync(IReadOnlyList<(double Latitude, double Longitude)> points,
        CancellationToken cancellationToken = default);

    /// <summary>AC-1..AC-5: reads the wells, places them, grids the value, contours it and builds the view.</summary>
    Task<OgResult<OgMapWorkspace>> BuildAsync(OgMapRequest request, CancellationToken cancellationToken = default);

    /// <summary>AC-6: writes the map to a PDF, PNG or SVG, and reports what was written.</summary>
    Task<OgResult<OgMapExport>> ExportAsync(OgMapWorkspace workspace, string format, string path,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// INT-07 over the tools' <see cref="WellMaps"/>, <see cref="Gridding"/>, <see cref="Contouring"/>, <see cref="ContourBands"/>,
/// <see cref="ContourMapView"/> and <see cref="MapExport"/>, reading wells from the PPDM tables the application already opens.
/// </summary>
/// <remarks>
/// <para>
/// Every geometric step is the engine's: a pick is placed where the well really is (wellhead plus the trajectory at the pick's
/// measured depth), the grid comes from <see cref="GridSpec.FromPoints"/>, the quality check from the gridding result, the levels from
/// <see cref="Contouring"/>, and the drawing and the exports from the view and its exporter. What this service adds is the well model
/// assembled from the database, the refusal rules, the workspace and the record.
/// </para>
/// <para>
/// The wellhead coordinates in PPDM (SURFACE_LATITUDE / SURFACE_LONGITUDE) are geographic WGS 84, which the model is told explicitly:
/// the engine skips a well whose CRS it cannot resolve rather than assuming one, and a wrong datum can move a well hundreds of metres.
/// </para>
/// </remarks>
public sealed class OgMapService : IOgMap
{
    /// <summary>The CRS the PPDM wellhead coordinates are recorded in.</summary>
    public const string WellheadCrs = OgWellModelReader.WellheadCrs;

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgMapService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<IReadOnlyList<OgMapProjection>>> ProjectionsAsync(
        IReadOnlyList<(double Latitude, double Longitude)> points, CancellationToken cancellationToken = default)
    {
        try
        {
            // The systems the environment defines, and - because mapping.coordinateSystems ships empty - the UTM zone each well
            // actually falls in, which the engine generates from its own definition. A map is always projectable then, and the
            // projection offered is the one that suits the wells rather than one this build picked.
            var probes = points.Count > 0 ? points : await WellheadPointsAsync(cancellationToken).ConfigureAwait(false);

            var names = new List<string>(CoordinateSystems.DefinedNames());

            foreach (var zone in names.Count == 0 ? OgMapRules.ZonesOf(probes) : OgMapRules.ZonesOf(probes))
            {
                if (!names.Contains(zone, StringComparer.OrdinalIgnoreCase))
                {
                    names.Add(zone);
                }
            }

            var list = new List<OgMapProjection>();

            foreach (var name in names)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (CoordinateSystems.Get(name) is not { } system)
                {
                    continue;
                }

                // The round trip is measured at the wells' own positions, so the error is the one this map would carry.
                var error = OgMapRules.RoundTripMetres(system, probes.Count > 0 ? probes : [(0d, 0d)]);

                list.Add(new OgMapProjection(
                    system.Name,
                    system.Definition.Method,
                    system.Definition.Axes ?? "en",
                    error,
                    error <= OgMapRules.RoundTripToleranceMetres));
            }

            return OgResult<IReadOnlyList<OgMapProjection>>.Ok(list);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "listing the map projections", "no projection is offered", FailureSeverity.Degraded);

            return OgResult<IReadOnlyList<OgMapProjection>>.Failed(
                "The projections could not be read (reference " + reference + ").", reference ?? OgMapRules.CodeReadFailed);
        }
    }

    /// <summary>The wellhead positions of the wells in the database, so a UTM zone can be derived from where the field actually is.</summary>
    private async Task<IReadOnlyList<(double Latitude, double Longitude)>> WellheadPointsAsync(
        CancellationToken cancellationToken)
    {
        var source = _source();

        if (source is null)
        {
            return [];
        }

        try
        {
            var bores = await OgUow.ReadAllAsync<WELLBORE>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            return bores
                .Where(bore => bore.SURFACE_LATITUDE is { } latitude && bore.SURFACE_LONGITUDE is { } longitude &&
                               (double)latitude != 0d && (double)longitude != 0d)
                .Take(2000)
                .Select(bore => ((double)bore.SURFACE_LATITUDE!.Value, (double)bore.SURFACE_LONGITUDE!.Value))
                .ToList();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // The projections can still be listed from the configured systems alone; the reason is reported rather than shown as an
            // empty list that would read as "the environment defines nothing".
            _failures.ReportHandled(
                ex, "reading the wellhead positions for the projection list",
                "the UTM zones cannot be derived from the wells", FailureSeverity.Degraded);

            return [];
        }
    }

    public async Task<OgResult<OgMapWorkspace>> BuildAsync(
        OgMapRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Projection))
        {
            return OgResult<OgMapWorkspace>.Failed(
                "A map is drawn in a coordinate system, and none was named - a projection is required.",
                OgMapRules.CodeProjectionRequired);
        }

        if (string.IsNullOrWhiteSpace(request.TopName))
        {
            return OgResult<OgMapWorkspace>.Failed(
                "A map needs the value to map: name the formation top.", OgMapRules.CodeTopRequired);
        }

        if (OgMapRules.MethodOf(request.Method) is not { } method)
        {
            return OgResult<OgMapWorkspace>.Failed(
                "'" + request.Method + "' is not a gridding method; use " + string.Join(", ", OgMapRules.Methods) + ".",
                OgMapRules.CodeMethodUnknown);
        }

        if (!OgMapRules.IsKnownProjection(request.Projection))
        {
            return OgResult<OgMapWorkspace>.Failed(
                "'" + request.Projection.Trim() + "' is not a coordinate system this environment defines, so the map cannot be " +
                "projected. Choose one of the systems the page lists (utm:<zone><N|S> needs no definition).",
                OgMapRules.CodeProjectionUnknown);
        }

        var system = CoordinateSystems.Get(request.Projection.Trim());

        var source = _source();

        if (source is null)
        {
            return OgResult<OgMapWorkspace>.Failed(
                "No connection is open, so the wells cannot be read.", OgMapRules.CodeNoConnection);
        }

        try
        {
            var models = await ReadWellsAsync(source, request, cancellationToken).ConfigureAwait(false);

            if (models.Count == 0)
            {
                return OgResult<OgMapWorkspace>.Failed(
                    "No well with a recorded survey and a top named '" + request.TopName.Trim() + "' is in scope, so there is " +
                    "nothing to map.", OgMapRules.CodeTooFewWells);
            }

            // The engine does the work: the pick is placed at the wellhead plus the trajectory at the pick's measured depth, and a
            // well it cannot place is left out of the map with its reason in the table.
            var map = string.IsNullOrWhiteSpace(request.LowerTop)
                ? WellMaps.Structure(models, request.TopName.Trim())
                : WellMaps.Thickness(models, request.TopName.Trim(), request.LowerTop!.Trim());

            var wells = map.Rows.Select(row => new OgMapWell(
                row.Well, row.Included, row.Reason, row.X, row.Y, row.Value, row.Md, row.Tvd, row.Notes.ToList())).ToList();

            var included = wells.Count(well => well.Included);
            var excluded = wells.Count - included;

            if (included < OgMapRules.MinimumWells)
            {
                return OgResult<OgMapWorkspace>.Ok(new OgMapWorkspace(
                    system.Name,
                    system.Definition.Axes ?? "en",
                    map.Description,
                    map.ValueUnit,
                    MapPossible: false,
                    "a surface needs at least " + OgMapRules.MinimumWells + " placed wells and only " +
                    included.ToString(CultureInfo.InvariantCulture) + " could be placed",
                    wells,
                    included,
                    excluded,
                    null,
                    null,
                    [],
                    map.Notes.ToList(),
                    null,
                    0d,
                    0d,
                    DateTime.UtcNow));
            }

            var points = map.Points();
            var gridded = Gridding.Run(points, method);
            var gridNotes = gridded.Notes.ToList();

            // AC-5: the engine's own interval unless the request sets one.
            var levels = OgMapRules.LevelsOf(
                gridded.Qc.GridMin, gridded.Qc.GridMax, request.ContourInterval, map.ValueUnit, out var used, out var nice);

            var contours = new OgMapContours(
                levels.Count,
                used,
                gridded.Qc.GridMin,
                gridded.Qc.GridMax,
                nice,
                levels.Select(level => OgMapRules.Figure(level)).ToList());

            var qc = new OgMapQc(
                gridded.Qc.PointCount,
                gridded.Qc.MergedDuplicates,
                gridded.Qc.MaskedNodes,
                gridded.Qc.MaxDistanceToData,
                "m",
                gridded.Qc.MaxAbsResidual,
                gridded.Qc.RmsResidual,
                gridded.Qc.OvershootsData,
                gridded.Qc.Residuals
                    .OrderByDescending(residual => Math.Abs(residual.residual))
                    .Take(3)
                    .Select(residual => (residual.point.Label ?? "(unnamed)") + " " +
                                        OgMapRules.Figure(residual.residual))
                    .ToList());

            var view = new ContourMapView
            {
                Surface = gridded.Grid,
                ValueUnit = map.ValueUnit,
                Title = map.Description + " · " + system.Name,
                ContourInterval = request.ContourInterval is { } wanted && wanted > 0d ? wanted : null,
                Extent = gridded.Grid.Spec,
                ShowFill = true,
                ShowContours = true,
                ShowLabels = true,
                ShowWells = true,
                ShowLegend = true,
                ShowScaleBar = true,
                ShowNorthArrow = true,
            };

            foreach (var well in wells.Where(well => well.Included))
            {
                view.Wells.Add(new MapWell { Label = well.Well, X = well.X, Y = well.Y });

                // The value at the well is what the bubbles are for: their area stands for it, so the map reads as numbers as well as
                // contours - which is the bubble map the feature asks for.
                if (well.Value is { } value)
                {
                    view.Bubbles.Add(new MapBubble { Label = well.Well, X = well.X, Y = well.Y, Value = value });
                }
            }

            return OgResult<OgMapWorkspace>.Ok(new OgMapWorkspace(
                system.Name,
                system.Definition.Axes ?? "en",
                map.Description,
                map.ValueUnit,
                MapPossible: true,
                null,
                wells,
                included,
                excluded,
                qc,
                contours,
                gridNotes,
                map.Notes.ToList(),
                view,
                gridded.Grid.Spec.Y1 - gridded.Grid.Spec.Y0,
                gridded.Grid.Spec.X1 - gridded.Grid.Spec.X0,
                DateTime.UtcNow));
        }
        catch (ArgumentException ex)
        {
            // The engine refuses a set of points it cannot grid (all on one line, coincident, ...) as an ordinary answer.
            var reference = _failures.ReportHandled(
                ex, "building the map for " + request.TopName, "the map is not built", FailureSeverity.Degraded);

            return OgResult<OgMapWorkspace>.Failed(
                "The map could not be built from these wells: check that they are not all on one line and that the values are not " +
                "all identical. (reference " + reference + ")", OgMapRules.CodeTooFewWells);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "building the map for " + request.TopName, "the map is not shown", FailureSeverity.Error);

            return OgResult<OgMapWorkspace>.Failed(
                "The map could not be built" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMapRules.CodeReadFailed);
        }
    }

    public Task<OgResult<OgMapExport>> ExportAsync(
        OgMapWorkspace workspace, string format, string path, CancellationToken cancellationToken = default)
    {
        if (workspace.View is null)
        {
            return Task.FromResult(OgResult<OgMapExport>.Failed(
                "There is no map to export: the last build could not place enough wells.", OgMapRules.CodeExportFailed));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(OgResult<OgMapExport>.Failed(
                "An export writes a file; no path was given.", OgMapRules.CodeExportPath));
        }

        var chosen = (format ?? string.Empty).Trim().ToLowerInvariant();

        if (!OgMapRules.Formats.Contains(chosen, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(OgResult<OgMapExport>.Failed(
                "'" + format + "' is not an export format; use " + string.Join(", ", OgMapRules.Formats) + ".",
                OgMapRules.CodeFormatUnknown));
        }

        try
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(path));

            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using var file = File.Create(path);

            switch (chosen)
            {
                case "pdf": MapExport.ToPdf(workspace.View, file); break;
                case "png": MapExport.ToPng(workspace.View, file); break;
                default: MapExport.ToSvg(workspace.View, file); break;
            }

            file.Flush();

            var bytes = new FileInfo(path).Length;

            return Task.FromResult(OgResult<OgMapExport>.Ok(new OgMapExport(
                chosen, path, bytes, true, chosen.ToUpperInvariant() + " written: " + bytes.ToString(CultureInfo.InvariantCulture) + " bytes")));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            // The feature's own state: the failure is reported and the request is kept - the caller still holds the workspace.
            var reference = _failures.ReportHandled(
                ex, "exporting the map to " + chosen, "the file is not written", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgMapExport>.Failed(
                "The map could not be written to '" + path + "'" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgMapRules.CodeExportFailed));
        }
    }

    // ------------------------------------------------------------------ the wells

    /// <summary>
    /// Builds the well model the engine reads from the PPDM tables: the header and its wellhead position, the datum and its elevation,
    /// the survey and the picks.
    /// </summary>
    private Task<List<WellModel>> ReadWellsAsync(
        IDataSource source, OgMapRequest request, CancellationToken cancellationToken) =>
        OgWellModelReader.ReadAsync(source, request.Field, request.MaxWells, request.Progress, cancellationToken);

    private static string UnitOf(string? uom) =>
        string.IsNullOrWhiteSpace(uom) ? WellboreEnvironment.Current.Units.DepthUnit : uom.Trim();
}
