using System.Windows.Automation;
using System.Windows.Controls;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Rendering;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Reporting;

/// <summary>A chart point chosen by the person, for the page to describe (RPT-03 AC-5).</summary>
public sealed record OgChartPointSelected(string Series, string Label, double Value, double X, double Y);

/// <summary>A render failure the contract reported without an exception.</summary>
public sealed class OgChartRenderException(string message) : Exception(message);

/// <summary>
/// RPT-03: the chart surface. It paints through the same <see cref="ViewSurface"/> path as every other well
/// visual, hands the canvas to the domain's <see cref="IRenderingService"/>, and reports what went wrong.
/// </summary>
/// <remarks>
/// The palette is supplied by the host from the theme tokens, so a skin change is a repaint (AC-2). An empty
/// series draws "No data" inside the plot (AC-3). A render that fails is drawn as a failure panel <b>and</b>
/// recorded through the failure reporter (AC-4) — never a silently blank chart.
/// </remarks>
public sealed class ChartSurface : ViewSurface
{
    private const float HitRadius = 14f;

    public ChartSurface()
    {
        Name = "Chart";
        Width = 640;
        Height = 380;
    }

    public IRenderingService? Renderer { get; set; }

    public RenderChartRequest? Request { get; set; }

    /// <summary>The theme palette; null uses the renderer's default tokens.</summary>
    public IReadOnlyList<SKColor>? Palette { get; set; }

    public SKColor ChartBackground { get; set; } = SKColors.White;

    public IFailureReporter? FailureReporter { get; set; }

    /// <summary>Raised when a click lands on a drawn point (AC-5).</summary>
    public event EventHandler<OgChartPointSelected>? PointSelected;

    /// <summary>What the last draw said: "rendered", "no data", a failure message, or a placeholder.</summary>
    public string? LastMessage { get; private set; }

    public IReadOnlyList<OgChartPoint> LastPoints { get; private set; } = [];

    /// <summary>REL-03 AC-5: a text alternative for the chart (type, series, value ranges).</summary>
    public string? AccessibleSummary { get; private set; }

    public override string AccessibleName => "Chart";

    public override string AccessibleDescription => AccessibleSummary ?? "A chart has not been drawn yet.";

    protected override void DrawContent(SKCanvas canvas)
    {
        canvas.Clear(ChartBackground);
        LastPoints = [];

        using var text = new SKPaint { IsAntialias = true, Color = new SKColor(0x33, 0x33, 0x33) };
        using var font = new SKFont { Size = 12f };

        if (Renderer is null)
        {
            LastMessage = "No chart renderer is registered.";
            Centre(canvas, LastMessage, font, text);
            return;
        }

        var request = Request;

        if (request is null)
        {
            LastMessage = "No chart has been requested.";
            Centre(canvas, LastMessage, font, text);
            return;
        }

        // The chart is drawn at the surface's size, so an export of the same request matches what is on screen.
        request.Width = Width;
        request.Height = Height;

        if (Renderer is IOgChartPaletteAware aware)
        {
            if (Palette is { Count: > 0 } palette)
            {
                aware.Palette = palette;
            }

            aware.Background = ChartBackground;
        }

        try
        {
            var result = Renderer
                .RenderChartAsync(request, canvas, "desktop")
                .GetAwaiter()
                .GetResult();

            LastPoints = (Renderer as OgSkiaChartRenderer)?.LastPoints ?? [];
        AccessibleSummary = Summarise(request);
            LastMessage = (Renderer as OgSkiaChartRenderer)?.LastOutcome;

            if (!result.IsSuccess)
            {
                AccessibleSummary = result.ErrorMessage ?? "The chart did not render.";
                Report(result.ErrorMessage ?? "the chart did not render");
                DrawFailure(canvas, result.ErrorMessage ?? "The chart did not render.", font);
            }
        }
        catch (Exception ex)
        {
            var reference = FailureReporter?.ReportHandled(
                ex,
                "rendering a chart",
                "the chart is not shown",
                FailureSeverity.Error);

            if (reference is null)
            {
                throw;
            }

            LastMessage = $"The chart could not be rendered. Reference: {reference}";
            DrawFailure(canvas, LastMessage ?? "The chart could not be rendered.", font);
        }
    }

    /// <summary>The text alternative: type, series names and value ranges (AC-5).</summary>
    private static string Summarise(RenderChartRequest request)
    {
        var (_, series) = ChartDataReader.Read(request.ChartData);
        var populated = series.Where(item => item.Values.Count > 0).ToList();

        if (populated.Count == 0)
        {
            return $"{request.ChartType} chart with no data.";
        }

        return $"{request.ChartType} chart with {populated.Count} series: " +
               string.Join(
                   "; ",
                   populated.Select(item =>
                       $"{item.Name} {item.Values.Min():0.##} to {item.Values.Max():0.##}")) +
               ".";
    }

    protected override bool OnMouseUp(SKPoint point)
    {
        if (LastPoints.Count == 0)
        {
            return false;
        }

        var nearest = LastPoints
            .Select(candidate => (Point: candidate, Distance: Distance(candidate, point)))
            .Where(pair => pair.Distance <= HitRadius)
            .OrderBy(pair => pair.Distance)
            .Select(pair => pair.Point)
            .FirstOrDefault();

        if (nearest is null)
        {
            return false;
        }

        PointSelected?.Invoke(this, new OgChartPointSelected(
            nearest.Series,
            nearest.Label,
            nearest.Value,
            nearest.X,
            nearest.Y));

        return true;
    }

    private void Report(string message)
    {
        var reference = FailureReporter?.ReportHandled(
            new OgChartRenderException(message),
            "rendering a chart",
            "the chart is not shown",
            FailureSeverity.Error);

        LastMessage = reference is null
            ? message
            : $"{message} Reference: {reference}";
    }

    private void DrawFailure(SKCanvas canvas, string message, SKFont font)
    {
        canvas.Clear(ChartBackground);

        using var paint = new SKPaint { IsAntialias = true, Color = new SKColor(0xB3, 0x26, 0x1E) };
        var lines = message.Length <= 70
            ? new[] { message }
            : new[] { message[..70], message[70..] };

        for (var i = 0; i < lines.Length; i++)
        {
            canvas.DrawText(
                lines[i],
                Width / 2f,
                (Height / 2f) - 8 + (i * 18),
                SKTextAlign.Center,
                font,
                paint);
        }
    }

    private void Centre(SKCanvas canvas, string message, SKFont font, SKPaint paint) =>
        canvas.DrawText(message, Width / 2f, Height / 2f, SKTextAlign.Center, font, paint);

    private static double Distance(OgChartPoint point, SKPoint target) =>
        Math.Sqrt(Math.Pow(point.X - target.X, 2) + Math.Pow(point.Y - target.Y, 2));
}

/// <summary>
/// RPT-03: the reusable chart control. A thin WPF wrapper over <see cref="SkiaViewSurfaceHost"/> so a page
/// hosts a chart with the same paint path, failure containment and device scaling as the well visuals.
/// </summary>
public sealed class ChartHost : ContentControl
{
    private readonly ChartSurface _surface = new();
    private readonly SkiaViewSurfaceHost _host;

    public ChartHost()
    {
        _host = new SkiaViewSurfaceHost { Surface = _surface };
        Content = _host;
        AutomationProperties.SetName(this, "Chart");
        AutomationProperties.SetHelpText(this, "A chart.");
    }

    public IRenderingService? Renderer
    {
        get => _surface.Renderer;
        set
        {
            _surface.Renderer = value;
            _host.Redraw();
        }
    }

    public RenderChartRequest? Request
    {
        get => _surface.Request;
        set
        {
            _surface.Request = value;
            _host.Redraw();
            AutomationProperties.SetHelpText(this, _surface.AccessibleDescription);
        }
    }

    public IFailureReporter? FailureReporter
    {
        get => _surface.FailureReporter;
        set
        {
            _surface.FailureReporter = value;
            _host.FailureReporter = value;
        }
    }

    /// <summary>The theme palette, applied on the next draw (RPT-03 AC-2).</summary>
    public IReadOnlyList<SKColor>? Palette
    {
        get => _surface.Palette;
        set
        {
            _surface.Palette = value;
            _host.Redraw();
        }
    }

    public SKColor ChartBackground
    {
        get => _surface.ChartBackground;
        set
        {
            _surface.ChartBackground = value;
            _host.Redraw();
        }
    }

    public string? LastMessage => _surface.LastMessage;

    public event EventHandler<OgChartPointSelected>? PointSelected
    {
        add => _surface.PointSelected += value;
        remove => _surface.PointSelected -= value;
    }

    public bool Redraw() => _host.Redraw();
}
