using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One survey station as the table shows it.</summary>
public sealed record OgSurveyStation(
    double Md,
    double Inclination,
    double Azimuth,
    double Tvd,
    double North,
    double East,
    double Dogleg);

/// <summary>A point on a plan or section view.</summary>
public sealed record OgPathPoint(double Md, double X, double Y);

/// <summary>One station's horizontal position uncertainty, for the plan view.</summary>
public sealed record OgStationEllipse(
    double North,
    double East,
    double SemiMajor,
    double SemiMinor,
    double AzimuthDegrees);

/// <summary>Whether the uncertainty ellipses can be drawn, and why not when they cannot.</summary>
public sealed record OgPathUncertainty(
    bool Available,
    string Statement,
    IReadOnlyList<OgStationEllipse> Ellipses);

/// <summary>
/// VSB-07: the well path — stations, plan and section paths, the anti-collision ladder and travelling
/// cylinder.
/// </summary>
/// <remarks>
/// <para>
/// The maths is the tools': <see cref="Trajectory.Compute"/> for MD/TVD/N/E, <see cref="AntiCollision.Scan"/>
/// for separation (ENG-01's computation, called and never re-implemented), and
/// <see cref="SurveyUncertainty.ForWell"/> for the ellipses. This class arranges the demo survey and states
/// what is and is not available.
/// </para>
/// <para>
/// A vertical well has no meaningful section azimuth. Rather than inventing 37°, the page says the section
/// uses 0° because the well is vertical — the assumption is stated, not hidden.
/// </para>
/// </remarks>
/// <summary>An offset well in the anti-collision scan: its survey and its surface position.</summary>
public sealed record OgOffsetWell(
    string Name,
    IReadOnlyList<(double Md, double Inc, double Azi)> Survey,
    double Latitude,
    double Longitude);
public static class OgWellPath
{
    public const string DefaultSectionAzimuthNote =
        "The well is vertical, so the section bearing is undefined; 0° (north) is used.";

    /// <summary>The demo reference survey: vertical, then a build to 35° and a hold.</summary>
    public static IReadOnlyList<(double Md, double Inc, double Azi)> ReferenceSurvey { get; } =
    [
        (0, 0, 0),
        (500, 0, 0),
        (800, 12, 45),
        (1100, 24, 45),
        (1400, 35, 45),
        (2000, 35, 45),
        (2600, 35, 45),
    ];

    /// <summary>An offset well, close enough to matter to the anti-collision scan.</summary>
    public static IReadOnlyList<(double Md, double Inc, double Azi)> OffsetSurvey { get; } =
    [
        (0, 0, 0),
        (500, 0, 0),
        (900, 8, 90),
        (1300, 20, 90),
        (1800, 26, 90),
        (2400, 26, 90),
    ];

    /// <summary>The reference trajectory, computed by the tools.</summary>
    public static Trajectory Reference { get; } = Trajectory.Compute(ReferenceSurvey, "m");

    public static IReadOnlyList<OgSurveyStation> Stations(Trajectory? trajectory = null)
    {
        var path = trajectory ?? Reference;

        return path.Stations
            .Select(station => new OgSurveyStation(
                station.Md,
                station.Inclination,
                station.Azimuth,
                station.Tvd,
                station.North,
                station.East,
                station.Dls))
            .ToList();
    }

    /// <summary>The plan view path: north against east, one point per station (AC-2).</summary>
    public static IReadOnlyList<OgPathPoint> PlanPath(IReadOnlyList<OgSurveyStation> stations)
    {
        ArgumentNullException.ThrowIfNull(stations);

        return stations.Select(station => new OgPathPoint(station.Md, station.East, station.North)).ToList();
    }

    /// <summary>The section view path: vertical section against TVD, one point per station (AC-2).</summary>
    public static IReadOnlyList<OgPathPoint> SectionPath(
        IReadOnlyList<OgSurveyStation> stations,
        double azimuthDegrees)
    {
        ArgumentNullException.ThrowIfNull(stations);

        var radians = azimuthDegrees * Math.PI / 180.0;

        return stations
            .Select(station => new OgPathPoint(
                station.Md,
                (station.North * Math.Cos(radians)) + (station.East * Math.Sin(radians)),
                station.Tvd))
            .ToList();
    }

    /// <summary>
    /// The section bearing, and the assumption made when the well is vertical (AC-5).
    /// </summary>
    public static (double Azimuth, bool Assumed, string? Note) SectionAzimuth(Trajectory? trajectory = null)
    {
        var path = trajectory ?? Reference;

        var last = path.Stations[^1];
        var horizontal = Math.Sqrt((last.North * last.North) + (last.East * last.East));

        if (horizontal < 1e-6 || last.Inclination < 0.05)
        {
            return (0, true, DefaultSectionAzimuthNote);
        }

        var azimuth = Math.Atan2(last.East, last.North) * 180.0 / Math.PI;
        return (azimuth < 0 ? azimuth + 360 : azimuth, false, null);
    }

    /// <summary>Ellipses only when an error model produces them (AC-4).</summary>
    public static OgPathUncertainty Uncertainty(Trajectory? trajectory = null)
    {
        var warnings = new List<string>();
        var stations = SurveyUncertainty.ForWell(trajectory ?? Reference, context: null, warnings);

        // A covariance of zero is not a model: it would draw an ellipse of radius zero, which says nothing.
        var ellipses = stations
            .Where(station => double.IsFinite(station.SigmaNorth) || double.IsFinite(station.SigmaEast))
            .Select(station =>
            {
                var (semiMajor, semiMinor, azimuth) = station.HorizontalEllipse;
                return new OgStationEllipse(station.North, station.East, semiMajor, semiMinor, azimuth);
            })
            .Where(ellipse => ellipse.SemiMajor > 0 && ellipse.SemiMinor >= 0)
            .ToList();

        if (ellipses.Count > 0)
        {
            return new OgPathUncertainty(true, $"{ellipses.Count} station ellipse(s) from the survey error model.", ellipses);
        }

        return new OgPathUncertainty(
            false,
            warnings.Count > 0
                ? "No uncertainty model: " + string.Join(" ", warnings)
                : "No uncertainty model is configured, so no ellipses are drawn.",
            []);
    }


    /// <summary>The single demo offset well (the behaviour shipped since VSB-07).</summary>
    public static IReadOnlyList<OgOffsetWell> DemoOffsetWells { get; } =
    [
        new("DEMO-OFF", OffsetSurvey, 29.0015, 48.0015),
    ];

    /// <summary>A second, closer offset well: a different set, for AC-6's "offset set changes".</summary>
    public static IReadOnlyList<OgOffsetWell> TwoOffsetWells { get; } =
    [
        new("DEMO-OFF", OffsetSurvey, 29.0015, 48.0015),
        new("DEMO-OFF-2", OffsetSurvey, 29.0008, 48.0009),
    ];

    /// <summary>The offset sets the page offers, in the order it shows them.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<OgOffsetWell>> OffsetSets { get; } =
        new Dictionary<string, IReadOnlyList<OgOffsetWell>>(StringComparer.OrdinalIgnoreCase)
        {
            ["One offset (demo)"] = DemoOffsetWells,
            ["Two offsets"] = TwoOffsetWells,
            ["No offsets"] = [],
        };

    /// <summary>The anti-collision result for the demo reference and offset wells.</summary>
    public static (AntiCollisionResult Result, IReadOnlyList<string> Warnings) Collision() =>
        ComputeOffsets(DemoOffsetWells, CancellationToken.None);

    /// <summary>
    /// Scans the offset wells <b>one at a time</b>, observing <paramref name="token"/> before each and after the
    /// last (VSB-07 AC-6). A cancelled scan throws <see cref="OperationCanceledException"/> and returns no
    /// partial result, so a superseded computation cannot be applied.
    /// </summary>
    /// <remarks>
    /// Each offset is its own <see cref="AntiCollision.Scan"/>; the results are concatenated, which is what a
    /// single multi-offset scan produces. That makes the per-offset token check real work rather than a
    /// decoration: the scan of a following well never starts once the set has changed.
    /// </remarks>
    public static (AntiCollisionResult Result, IReadOnlyList<string> Warnings) ComputeOffsets(
        IReadOnlyList<OgOffsetWell> offsets,
        CancellationToken token,
        Action<int, int>? progressed = null)
    {
        ArgumentNullException.ThrowIfNull(offsets);

        token.ThrowIfCancellationRequested();

        var warnings = new List<string>();
        var reference = Model("DEMO-REF", ReferenceSurvey, latitude: 29.0, longitude: 48.0);
        var combined = new AntiCollisionResult { Reference = reference.Header.Name };

        for (var index = 0; index < offsets.Count; index++)
        {
            token.ThrowIfCancellationRequested();

            var well = offsets[index];
            var model = Model(well.Name, well.Survey, well.Latitude, well.Longitude);
            var scan = AntiCollision.Scan(reference, [model], warnings);

            combined.Offsets.AddRange(scan.Offsets);
            progressed?.Invoke(index + 1, offsets.Count);
        }

        // A cancel after the last well but before the caller uses the result is still honoured.
        token.ThrowIfCancellationRequested();

        return (combined, warnings);
    }

    /// <summary>The ladder plot as a frame, drawn by the tools' <see cref="LadderPlotView"/>.</summary>
    public static byte[]? LadderFrame(AntiCollisionResult result, int width = 520, int height = 380)
    {
        ArgumentNullException.ThrowIfNull(result);

        var view = new LadderPlotView { Result = result };
        return Render(view, width, height);
    }

    /// <summary>The travelling cylinder as a frame, drawn by the tools' <see cref="TravellingCylinderView"/>.</summary>
    public static byte[]? CylinderFrame(AntiCollisionResult result, int width = 420, int height = 420)
    {
        ArgumentNullException.ThrowIfNull(result);

        var view = new TravellingCylinderView { Result = result };
        return Render(view, width, height);
    }

    /// <summary>The smallest separation factor and the offset it belongs to, for the panel.</summary>
    public static (string Offset, double Factor, double Distance)? Minimum(AntiCollisionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var best = result.Offsets
            .Select(offset => (offset.Name, Point: offset.Minimum))
            .Where(pair => pair.Point is not null)
            .OrderBy(pair => pair.Point!.SeparationFactor)
            .FirstOrDefault();

        return best.Point is null ? null : (best.Name, best.Point.SeparationFactor, best.Point.Distance);
    }

    private static WellModel Model(
        string name,
        IReadOnlyList<(double Md, double Inc, double Azi)> survey,
        double latitude,
        double longitude)
    {
        var model = new WellModel();
        model.Header.Name = name;
        model.Header.Latitude = latitude;
        model.Header.Longitude = longitude;
        model.Header.DepthDatum = "MSL";
        model.Header.Datums.Add(new DatumElevation { Code = "MSL", Elevation = new Quantity(0, "m"), Reference = "MSL" });

        foreach (var (md, inc, azi) in survey)
        {
            model.Survey.Add(new SurveyStation
            {
                Md = new Quantity(md, "m"),
                Inclination = inc,
                Azimuth = azi,
            });
        }

        return model;
    }

    private static byte[]? Render(ViewSurface view, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return null;
        }

        using var renderer = new ViewSurfaceRenderer { Surface = view };
        return renderer.Render(width, height) ? renderer.Frame : null;
    }
}

/// <summary>
/// A minimal path surface for the plan and section views: a polyline over labelled axes.
/// </summary>
/// <remarks>
/// Drawn here rather than reusing <c>MapView</c> because a well path has no map coordinates or CRS — it is
/// north/east from the survey. The stations it draws are the ones the table lists, so plan and section can
/// never disagree.
/// </remarks>
public sealed class OgPathSurface : ViewSurface
{
    private IReadOnlyList<OgPathPoint> _points = [];
    private IReadOnlyList<OgStationEllipse> _ellipses = [];
    private string _title = "Path";

    public OgPathSurface() : base()
    {
        Width = 520;
        Height = 420;
    }

    public IReadOnlyList<OgPathPoint> Points
    {
        get => _points;
        set { _points = value ?? []; InvalidateVisual(); }
    }

    /// <summary>
    /// Position ellipses for the plan view. Empty means no error model: nothing is drawn and the page says
    /// why (AC-4).
    /// </summary>
    public IReadOnlyList<OgStationEllipse> Ellipses
    {
        get => _ellipses;
        set { _ellipses = value ?? []; InvalidateVisual(); }
    }

    public string Title
    {
        get => _title;
        set { _title = value ?? "Path"; InvalidateVisual(); }
    }

    protected override void DrawContent(SKCanvas canvas)
    {
        canvas.Clear(SKColors.White);

        var plot = new SKRect(48, 28, Width - 16, Height - 36);

        using var axis = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1f, Color = new SKColor(0x88, 0x88, 0x88) };
        using var line = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f, Color = new SKColor(0x0B, 0x6E, 0x4F) };
        using var text = new SKPaint { IsAntialias = true, Color = new SKColor(0x33, 0x33, 0x33) };
        using var font = new SKFont { Size = 11f };

        canvas.DrawRect(plot, axis);
        canvas.DrawText(_title, plot.Left, 18, SKTextAlign.Left, font, text);

        if (_points.Count < 2)
        {
            canvas.DrawText("no survey stations", plot.Left + 8, plot.MidY, SKTextAlign.Left, font, text);
            return;
        }

        var minX = _points.Min(point => point.X);
        var maxX = _points.Max(point => point.X);
        var minY = _points.Min(point => point.Y);
        var maxY = _points.Max(point => point.Y);

        var spanX = Math.Max(1e-6, maxX - minX);
        var spanY = Math.Max(1e-6, maxY - minY);

        SKPoint Map(OgPathPoint point) => new(
            (float)(plot.Left + 8 + ((point.X - minX) / spanX) * (plot.Width - 16)),
            (float)(plot.Bottom - 8 - ((point.Y - minY) / spanY) * (plot.Height - 16)));

        Span<SKPoint> path = stackalloc SKPoint[_points.Count];
        for (var i = 0; i < _points.Count; i++)
        {
            path[i] = Map(_points[i]);
        }

        canvas.DrawPoints(SKPointMode.Polygon, path.ToArray(), line);

        foreach (var point in _points)
        {
            canvas.DrawCircle(Map(point), 2.2f, line);
        }

        // AC-4: the ellipses are drawn only when the error model produced them.
        if (_ellipses.Count > 0)
        {
            using var ellipsePaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f,
                Color = new SKColor(0x1A, 0x5F, 0xB4),
            };

            var scale = Math.Min(plot.Width / spanX, plot.Height / spanY);

            foreach (var ellipse in _ellipses)
            {
                var centre = Map(new OgPathPoint(0, ellipse.East, ellipse.North));
                var semiMajor = (float)(ellipse.SemiMajor * scale);
                var semiMinor = (float)(ellipse.SemiMinor * scale);

                if (semiMajor < 0.5f)
                {
                    continue;
                }

                canvas.Save();
                canvas.Translate(centre.X, centre.Y);
                canvas.RotateDegrees((float)-ellipse.AzimuthDegrees);
                canvas.DrawOval(
                    new SKRect(-semiMajor, -Math.Max(0.5f, semiMinor), semiMajor, Math.Max(0.5f, semiMinor)),
                    ellipsePaint);
                canvas.Restore();
            }
        }

        canvas.DrawText($"MD {_points[0].Md:0} → {_points[^1].Md:0}", plot.Left, plot.Bottom + 22, SKTextAlign.Left, font, text);
    }
}
