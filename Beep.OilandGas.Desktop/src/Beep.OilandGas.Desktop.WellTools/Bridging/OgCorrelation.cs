using System.IO;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One finding from the correlation set.</summary>
public sealed record OgCorrelationIssue(string Severity, string Code, string Message, string? Well);

/// <summary>One well as the correlation lays it out.</summary>
public sealed record OgCorrelationWellRow(string Title, int Tops, float Offset, bool HasDatum);

public sealed record OgCorrelationSummary(
    int Wells,
    int SharedTops,
    int MissingDatum,
    int WithoutTops,
    string Datum);

/// <summary>
/// VSB-06: several wells side by side, flattened on a chosen marker, with the checks a correlation needs.
/// </summary>
/// <remarks>
/// <para>
/// The panel, the flattening and the drawing are the tools': <see cref="CorrelationPanel"/>,
/// <see cref="CorrelationLayout.Build"/> (which offsets each well by its datum's axis value) and
/// <see cref="CorrelationRenderer.Draw"/>. This class builds the document set, states the checks and
/// renders or exports through those same services.
/// </para>
/// <para>
/// A well that does not carry the datum is <b>not</b> silently aligned: the tools give it an offset of zero,
/// which leaves it visibly out of line, and <see cref="Checks"/> names it. That is the whole point of a
/// correlation view — an aligned-looking well without the marker would be a lie.
/// </para>
/// <para>
/// <b>Pinch-outs are surfaced</b> through <see cref="CrossSectionLayout.Horizons"/>: a surface missing in a
/// neighbouring well ends on the next deeper surface present in both, and <see cref="SectionChecks"/> reports
/// where and between which wells. That requires a cross-section, which requires positions for every well.
/// </para>
/// </remarks>
public static class OgCorrelation
{
    public const string CodeDatumMissing = "correlation-datum-missing";
    public const string CodeNoTops = "correlation-no-tops";
    public const string CodeSingleWell = "correlation-needs-two-wells";

    /// <summary>A marker name every demo well but the third carries.</summary>
    public const string DemoDatum = "Top B";

    /// <summary>
    /// Three deterministic wells: two carry the datum at different depths, the third does not carry it at
    /// all, so the flattening and the "missing datum" check are exercised by the same document set.
    /// </summary>
    public static IReadOnlyList<(string Name, WellboreDocument Document)> DemoWells()
    {
        return
        [
            ("DEMO-1", Well("DEMO-1", 8100, 8400, datum: 8200, second: 8250, third: 8300)),
            ("DEMO-2", Well("DEMO-2", 8150, 8450, datum: 8230, second: null, third: 8350)),
            // DEMO-3 carries tops but not the datum, which is the case the check exists for.
            ("DEMO-3", Well("DEMO-3", 8200, 8500, datum: null, second: 8320, third: 8420)),
        ];
    }

    /// <summary>Builds the panel for the wells, flattened on <paramref name="datum"/>.</summary>
    public static CorrelationPanel Build(
        IReadOnlyList<(string Name, WellboreDocument Document)> wells,
        string? datum)
    {
        ArgumentNullException.ThrowIfNull(wells);

        var panel = new CorrelationPanel { Name = "Correlation" };

        foreach (var (name, document) in wells)
        {
            panel.Wells.Add(new CorrelationWell
            {
                Label = string.IsNullOrWhiteSpace(name) ? document.WellName : name,
                Document = document,
            });
        }

        panel.FlattenOn = string.IsNullOrWhiteSpace(datum) ? null : datum;
        return panel;
    }

    /// <summary>The checks a person must see before trusting the alignment.</summary>
    public static IReadOnlyList<OgCorrelationIssue> Checks(CorrelationPanel panel, string? datum)
    {
        ArgumentNullException.ThrowIfNull(panel);

        var issues = new List<OgCorrelationIssue>();

        if (panel.Wells.Count < 2)
        {
            issues.Add(new OgCorrelationIssue(
                "Error",
                CodeSingleWell,
                "Correlation needs at least two wells.",
                panel.Wells.FirstOrDefault()?.Title));
        }

        foreach (var well in panel.Wells)
        {
            if (well.Document.Markers.Count == 0)
            {
                issues.Add(new OgCorrelationIssue(
                    "Warning",
                    CodeNoTops,
                    $"{well.Title} has no tops; it is shown without markers.",
                    well.Title));
                continue;
            }

            if (!string.IsNullOrWhiteSpace(datum) && MarkerMatching.Find(well.Document.Markers, datum) is null)
            {
                issues.Add(new OgCorrelationIssue(
                    "Warning",
                    CodeDatumMissing,
                    $"{well.Title} does not carry '{datum}'; it is offset by zero and is not aligned on the datum.",
                    well.Title));
            }
        }

        return issues;
    }

    /// <summary>The laid-out columns: their offsets are what put the wells on a common datum.</summary>
    public static IReadOnlyList<OgCorrelationWellRow> Columns(
        CorrelationPanel panel,
        SKRect? bounds = null)
    {
        ArgumentNullException.ThrowIfNull(panel);

        var area = bounds ?? new SKRect(0, 0, 900, 600);
        var columns = CorrelationLayout.Build(panel, area);
        var rows = new List<OgCorrelationWellRow>();

        foreach (var column in columns)
        {
            var datum = string.IsNullOrWhiteSpace(panel.FlattenOn)
                ? null
                : MarkerMatching.Find(column.Well.Document.Markers, panel.FlattenOn);

            rows.Add(new OgCorrelationWellRow(
                column.Well.Title,
                column.Well.Document.Markers.Count,
                column.Offset,
                datum is not null));
        }

        return rows;
    }

    /// <summary>The axis value a well's datum lands on after flattening — zero means aligned.</summary>
    public static float DatumPositionAfterFlatten(CorrelationPanel panel, string wellTitle)
    {
        ArgumentNullException.ThrowIfNull(panel);

        var area = new SKRect(0, 0, 900, 600);
        var column = CorrelationLayout.Build(panel, area)
            .FirstOrDefault(item => string.Equals(item.Well.Title, wellTitle, StringComparison.Ordinal));

        if (column is null
            || string.IsNullOrWhiteSpace(panel.FlattenOn)
            || MarkerMatching.Find(column.Well.Document.Markers, panel.FlattenOn) is not { } datum)
        {
            return float.NaN;
        }

        return CorrelationLayout.AxisValue(column.Well.Document, panel.Reference, datum.Depth) - column.Offset;
    }

    public static OgCorrelationSummary Summarise(CorrelationPanel panel, IReadOnlyList<OgCorrelationIssue> checks)
    {
        ArgumentNullException.ThrowIfNull(panel);

        return new OgCorrelationSummary(
            panel.Wells.Count,
            panel.SharedTops().Count,
            checks.Count(issue => issue.Code == CodeDatumMissing),
            checks.Count(issue => issue.Code == CodeNoTops),
            panel.FlattenOn ?? "(none)");
    }

    /// <summary>Renders the panel to a BGRA frame, the same shape the host consumes.</summary>
    public static byte[]? Render(CorrelationPanel panel, int width, int height, WellboreTheme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(panel);

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        surface.Canvas.Clear(SKColors.White);

        CorrelationRenderer.Draw(surface.Canvas, new SKRect(0, 0, width, height), panel, theme);
        surface.Canvas.Flush();

        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        var frame = new byte[width * height * 4];
        bitmap.GetPixelSpan().CopyTo(frame);

        return frame;
    }

    /// <summary>The pages the panel export will write (AC-5), before writing them.</summary>
    public static IReadOnlyList<(float top, float bottom)?> Pages(
        CorrelationPanel panel,
        WellborePrintOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(panel);

        return PanelExport.CorrelationPages(panel, options ?? new WellborePrintOptions());
    }

    /// <summary>Writes the correlation panel as a paginated PDF; returns the page count.</summary>
    public static int ExportPdf(
        CorrelationPanel panel,
        Stream stream,
        WellborePrintOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(stream);

        return PanelExport.ToPdf(panel, stream, options);
    }

    // ------------------------------------------------------------------ VSB-06 AC-3/AC-4: section + pinch-outs

    public const string CodePinchOut = "correlation-pinch-out";
    public const string CodeNoCoordinates = "correlation-no-coordinates";

    /// <summary>A well's surface position in projected coordinates (the panel's unit).</summary>
    public sealed record OgWellPosition(string Well, double X, double Y, string? Crs = "EPSG:32631");

    /// <summary>
    /// Deterministic positions for the demo wells: a line that runs east and slightly north, so the section's
    /// horizontal axis is a real distance rather than an index.
    /// </summary>
    public static IReadOnlyList<OgWellPosition> DemoPositions() =>
    [
        new("DEMO-1", 0, 0),
        new("DEMO-2", 1_500, 220),
        new("DEMO-3", 3_200, 540),
    ];

    /// <summary>
    /// Builds the cross-section for wells with positions. A well without a position is a refusal, not a well
    /// drawn at zero: a section with an invented location is worse than no section (AC-4).
    /// </summary>
    public static OgResult<CrossSectionPanel> BuildSection(
        IReadOnlyList<(string Name, WellboreDocument Document)> wells,
        IReadOnlyList<OgWellPosition> positions,
        DepthUnit unit = DepthUnit.Feet)
    {
        ArgumentNullException.ThrowIfNull(wells);
        ArgumentNullException.ThrowIfNull(positions);

        var missing = wells
            .Where(well => !positions.Any(position =>
                string.Equals(position.Well, well.Name, StringComparison.OrdinalIgnoreCase)))
            .Select(well => well.Name)
            .ToList();

        if (missing.Count > 0)
        {
            return OgResult<CrossSectionPanel>.Failed(
                $"No coordinates for {string.Join(", ", missing)}; a cross-section needs a position for every well.",
                CodeNoCoordinates);
        }

        var placed = wells
            .Select(well => (
                Well: well,
                Position: positions.First(position =>
                    string.Equals(position.Well, well.Name, StringComparison.OrdinalIgnoreCase))))
            .OrderBy(item => item.Position.X)
            .ThenBy(item => item.Position.Y)
            .ToList();

        var panel = new CrossSectionPanel { Name = "Cross-section", Unit = unit };

        // The line runs through the outermost wells.
        panel.Line.Add((placed[0].Position.X, placed[0].Position.Y));
        panel.Line.Add((placed[^1].Position.X, placed[^1].Position.Y));

        foreach (var (well, position) in placed)
        {
            panel.Wells.Add(new CrossSectionWell
            {
                Document = well.Document,
                X = position.X,
                Y = position.Y,
                Crs = position.Crs,
                Label = well.Name,
            });
        }

        return OgResult<CrossSectionPanel>.Ok(panel);
    }

    /// <summary>
    /// The section's checks: the tools' coordinate-system findings plus every pinch-out the horizon builder
    /// reports (AC-3). A pinch-out is information, not an error: the unit tapers to nothing and the unit above
    /// continues onto the surface below.
    /// </summary>
    public static IReadOnlyList<OgCorrelationIssue> SectionChecks(CrossSectionPanel panel)
    {
        ArgumentNullException.ThrowIfNull(panel);

        var issues = new List<OgCorrelationIssue>();

        foreach (var issue in panel.Validate())
        {
            issues.Add(new OgCorrelationIssue(issue.Severity.ToString(), issue.Code, issue.Message, null));
        }

        var paths = CrossSectionLayout.Paths(panel);
        var horizons = CrossSectionLayout.Horizons(panel, paths);
        var ordered = paths
            .Where(path => path.Points.Count > 0)
            .OrderBy(path => path.Points[0].s)
            .ToList();

        var unit = DepthUnits.Symbol(panel.Unit);

        foreach (var horizon in horizons)
        {
            foreach (var (s, _, kind) in horizon.Terminations.Where(termination =>
                         termination.kind == HorizonTermination.PinchOut))
            {
                var left = ordered.LastOrDefault(path => path.Points[0].s <= s) ?? ordered.FirstOrDefault();
                var right = ordered.FirstOrDefault(path => path.Points[0].s >= s) ?? ordered.LastOrDefault();

                if (left is null || right is null)
                {
                    continue;
                }

                issues.Add(new OgCorrelationIssue(
                    "Info",
                    CodePinchOut,
                    $"'{horizon.Name}' pinches out between {left.Well.Title} and {right.Well.Title} at {s:0.#} {unit}.",
                    left.Well.Title));
            }
        }

        return issues;
    }

    // ------------------------------------------------------------------ VSB-06 AC-6: cancellable render

    /// <summary>One render: the panel, its columns, the checks, and the frame (null when only the model was asked for).</summary>
    public sealed record OgCorrelationRender(
        CorrelationPanel Panel,
        IReadOnlyList<OgCorrelationWellRow> Columns,
        IReadOnlyList<OgCorrelationIssue> Issues,
        byte[]? Frame);

    /// <summary>
    /// Renders the panel while observing <paramref name="token"/> <b>between wells</b>, before the frame and
    /// after it. A cancelled render throws <see cref="OperationCanceledException"/> and produces no frame, so a
    /// caller cannot apply half a picture.
    /// </summary>
    /// <remarks>
    /// The frame itself is one <see cref="CorrelationRenderer.Draw"/> call, which cannot be interrupted mid-draw.
    /// The token is therefore checked at every well boundary and immediately before and after the draw: a cancel
    /// that arrives while drawing discards the frame instead of showing it. That is the strongest honest
    /// cancellation this renderer supports.
    /// </remarks>
    public static OgCorrelationRender RenderChunked(
        IReadOnlyList<(string Name, WellboreDocument Document)> wells,
        string? datum,
        int width,
        int height,
        CancellationToken token,
        Action<int, int>? progressed = null)
    {
        ArgumentNullException.ThrowIfNull(wells);

        token.ThrowIfCancellationRequested();

        var panel = Build(wells, datum);

        token.ThrowIfCancellationRequested();

        var columns = Columns(panel);

        // The per-well checks: a change of well set is observed at the next well boundary, not after the draw.
        for (var index = 0; index < panel.Wells.Count; index++)
        {
            token.ThrowIfCancellationRequested();
            progressed?.Invoke(index + 1, panel.Wells.Count);
        }

        var issues = Checks(panel, panel.FlattenOn);

        token.ThrowIfCancellationRequested();

        var frame = Render(panel, width, height);

        // A cancel during the draw is honoured by discarding the frame.
        token.ThrowIfCancellationRequested();

        return new OgCorrelationRender(panel, columns, issues, frame);
    }

    /// <summary>Renders the cross-section to a BGRA frame, the same shape the host consumes.</summary>
    public static byte[]? RenderSection(
        CrossSectionPanel panel,
        int width,
        int height,
        WellboreTheme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(panel);

        if (width <= 0 || height <= 0)
        {
            return null;
        }

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        surface.Canvas.Clear(SKColors.White);

        CrossSectionRenderer.Draw(surface.Canvas, new SKRect(0, 0, width, height), panel, theme);
        surface.Canvas.Flush();

        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        var frame = new byte[width * height * 4];
        bitmap.GetPixelSpan().CopyTo(frame);

        return frame;
    }

    private static WellboreDocument Well(
        string name,
        float top,
        float bottom,
        float? datum,
        float? second,
        float? third)
    {
        var document = TheTechIdea.OilGas.WellLogs.WellLogTemplates.CreateTripleComboDemo(
            top,
            bottom,
            DepthUnit.Feet);

        document.Name = name;
        document.WellName = name;
        document.Markers.Clear();

        if (datum is { } d)
        {
            document.Markers.Add(new DepthMarker { Name = DemoDatum, Depth = d, Kind = MarkerKind.Top });
        }

        if (second is { } s)
        {
            document.Markers.Add(new DepthMarker { Name = "Top A", Depth = s, Kind = MarkerKind.Top });
        }

        if (third is { } t)
        {
            document.Markers.Add(new DepthMarker { Name = "Top C", Depth = t, Kind = MarkerKind.Top });
        }

        return document;
    }
}
