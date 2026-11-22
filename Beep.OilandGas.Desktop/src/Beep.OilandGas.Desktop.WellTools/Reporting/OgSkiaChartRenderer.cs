using System.IO;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Rendering;
using SkiaSharp;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Reporting;

/// <summary>One named series the renderer understands.</summary>
public sealed record OgChartSeries(string Name, IReadOnlyList<double> Values);

/// <summary>A drawn point, in surface coordinates, for hit testing (RPT-03 AC-5).</summary>
public sealed record OgChartPoint(string Series, string Label, double Value, float X, float Y);

/// <summary>The palette is the theme's, not the renderer's: the host supplies it at draw time (AC-2).</summary>
public interface IOgChartPaletteAware
{
    IReadOnlyList<SKColor> Palette { get; set; }

    SKColor Background { get; set; }
}

/// <summary>
/// RPT-03: the desktop's implementation of the domain's <see cref="IRenderingService"/>.
/// </summary>
/// <remarks>
/// <para>
/// The interface has no implementation in the owning repository, so this supplies one: the chart types it
/// supports are <c>line</c>, <c>area</c>, <c>bar</c>, <c>scatter</c>, <c>pie</c> and <c>donut</c>. An unknown
/// type is a failed result naming the type, never a blank canvas.
/// </para>
/// <para>
/// <b>Data shape.</b> <c>RenderChartRequest.ChartData</c> is a loose dictionary; this renderer reads
/// <c>series</c> (a sequence of <see cref="OgChartSeries"/>, a dictionary of name → values, or a sequence of
/// dictionaries with <c>name</c>/<c>values</c>) and optional <c>labels</c>. A single <c>values</c> sequence is
/// accepted as one unnamed series. Anything else is reported rather than guessed.
/// </para>
/// <para>
/// <b>Colours come from the palette</b> the host sets from <c>BeepChartBrush1..8</c>; the default is those
/// eight tokens. <see cref="LastPoints"/> is what the surface hit-tests, so a click reports the series and
/// value under the pointer.
/// </para>
/// <para>
/// Chart configurations are kept <b>in session only</b>: persistence is <c>CHART_CONFIGURATION</c> in PPDM,
/// which needs DAT-02. Saving works so a caller can round-trip a configuration, but it does not survive a
/// restart, and the class says so rather than pretending.
/// </para>
/// </remarks>
public sealed class OgSkiaChartRenderer : IRenderingService, IOgChartPaletteAware
{
    /// <summary>The eight chart tokens from `ControlDefaults.xaml`, used when no host palette is set.</summary>
    public static IReadOnlyList<SKColor> DefaultPalette { get; } =
    [
        new SKColor(0x0B, 0x6E, 0x4F),
        new SKColor(0x1A, 0x5F, 0xB4),
        new SKColor(0xB2, 0x6A, 0x00),
        new SKColor(0xB3, 0x26, 0x1E),
        new SKColor(0x6A, 0x3D, 0xA1),
        new SKColor(0x0F, 0x7B, 0x8A),
        new SKColor(0x8A, 0x6D, 0x3B),
        new SKColor(0x4C, 0x62, 0x72),
    ];

    private readonly Dictionary<string, CHART_CONFIGURATION> _configurations = new(StringComparer.Ordinal);

    public IReadOnlyList<SKColor> Palette { get; set; } = DefaultPalette;

    public SKColor Background { get; set; } = SKColors.White;

    /// <summary>The points of the last render, in canvas coordinates.</summary>
    public IReadOnlyList<OgChartPoint> LastPoints { get; private set; } = [];

    /// <summary>"rendered", "no data" or "unsupported" — what the last draw produced.</summary>
    public string LastOutcome { get; private set; } = "not rendered";

    public Task<RenderChartResult> RenderChartAsync(
        RenderChartRequest request,
        SKCanvas canvas,
        string userId,
        string connectionName = "PPDM39")
    {
        if (request is null)
        {
            return Task.FromResult(Failed(null, "no request"));
        }

        if (canvas is null)
        {
            return Task.FromResult(Failed(request, "no canvas"));
        }

        var width = request.Width > 0 ? request.Width : 800f;
        var height = request.Height > 0 ? request.Height : 600f;

        return Task.FromResult(Draw(request, canvas, width, height));
    }

    public Task<ExportChartResult> ExportChartAsync(
        ExportChartRequest request,
        string userId,
        string connectionName = "PPDM39")
    {
        ArgumentNullException.ThrowIfNull(request);

        var format = (request.ExportFormat ?? "png").Trim().ToLowerInvariant();

        if (format is not ("png" or "image/png"))
        {
            return Task.FromResult(new ExportChartResult
            {
                ChartType = request.ChartType ?? string.Empty,
                ExportFormat = request.ExportFormat ?? string.Empty,
                FilePath = request.FilePath ?? string.Empty,
                IsSuccess = false,
                ErrorMessage = $"'{request.ExportFormat}' is not a format this build writes; use PNG.",
            });
        }

        var width = request.Width > 0 ? request.Width : 800f;
        var height = request.Height > 0 ? request.Height : 600f;

        // Draw through the same ChartSurface the window uses, so the export carries exactly what the person
        // sees — including the tools' trademark footnote, which ViewSurface.Draw adds to every surface.
        var surface = new ChartSurface
        {
            Renderer = this,
            Palette = Palette,
            ChartBackground = Background,
            Request = new RenderChartRequest
            {
                ChartType = request.ChartType ?? string.Empty,
                Width = width,
                Height = height,
                ChartData = request.ChartData,
                ConfigurationId = request.ConfigurationId,
            },
        };

        using var view = new Hosting.ViewSurfaceRenderer { Surface = surface };

        if (!view.Render((int)Math.Round(width), (int)Math.Round(height)) || view.Frame is null)
        {
            return Task.FromResult(new ExportChartResult
            {
                ChartType = request.ChartType ?? string.Empty,
                ExportFormat = request.ExportFormat ?? string.Empty,
                FilePath = request.FilePath ?? string.Empty,
                IsSuccess = false,
                ErrorMessage = "The chart could not be drawn for export.",
            });
        }

        using var image = SKImage.FromPixelCopy(
            new SKImageInfo(view.PixelWidth, view.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul),
            view.Frame);

        using var bitmap = SKBitmap.FromImage(image);
        using var encoded = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(request.FilePath))!);
        using (var stream = File.Create(request.FilePath))
        {
            encoded.SaveTo(stream);
        }

        var rendered = surface.LastMessage is "rendered" or "no data";

        return Task.FromResult(new ExportChartResult
        {
            ChartType = request.ChartType ?? string.Empty,
            ExportFormat = "png",
            FilePath = request.FilePath ?? string.Empty,
            IsSuccess = rendered,
            ErrorMessage = rendered ? null : surface.LastMessage,
        });
    }

    public Task<CHART_CONFIGURATION> SaveChartConfigurationAsync(
        SaveChartConfigurationRequest request,
        string userId,
        string connectionName = "PPDM39")
    {
        ArgumentNullException.ThrowIfNull(request);

        var id = Guid.NewGuid().ToString("n")[..12];

        var configuration = new CHART_CONFIGURATION
        {
            CHART_CONFIGURATION_ID = id,
            CHART_TYPE = request.ChartType,
            CONFIGURATION_NAME = request.ConfigurationName,
            CONFIGURATION_DATA = request.ConfigurationData,
            CREATED_BY = userId,
            IS_DEFAULT = request.IsDefault ? "Y" : "N",
        };

        _configurations[id] = configuration;
        return Task.FromResult(configuration);
    }

    public Task<CHART_CONFIGURATION?> GetChartConfigurationAsync(string configurationId, string connectionName = "PPDM39") =>
        Task.FromResult(_configurations.TryGetValue(configurationId, out var configuration) ? configuration : null);

    public Task<List<CHART_CONFIGURATION>> GetChartConfigurationsAsync(string? chartType, string connectionName = "PPDM39")
    {
        var matches = _configurations.Values
            .Where(configuration => string.IsNullOrWhiteSpace(chartType)
                                    || string.Equals(configuration.CHART_TYPE, chartType, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult(matches);
    }

    public Task<CHART_CONFIGURATION?> GetDefaultChartConfigurationAsync(string chartType, string connectionName = "PPDM39")
    {
        var match = _configurations.Values.FirstOrDefault(configuration =>
            string.Equals(configuration.CHART_TYPE, chartType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(configuration.IS_DEFAULT, "Y", StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(match);
    }

    public Task<bool> DeleteChartConfigurationAsync(string configurationId, string userId, string connectionName = "PPDM39") =>
        Task.FromResult(_configurations.Remove(configurationId));

    // ------------------------------------------------------------------ drawing

    private RenderChartResult Draw(RenderChartRequest request, SKCanvas canvas, float width, float height)
    {
        canvas.Clear(Background);
        LastPoints = [];

        var palette = Palette.Count > 0 ? Palette : DefaultPalette;
        var (labels, series) = ChartDataReader.Read(request.ChartData);
        var type = (request.ChartType ?? string.Empty).Trim().ToLowerInvariant();

        using var text = new SKPaint { IsAntialias = true, Color = new SKColor(0x33, 0x33, 0x33) };
        using var font = new SKFont { Size = 11f };

        if (series.Count == 0 || series.All(item => item.Values.Count == 0))
        {
            LastOutcome = "no data";
            DrawCentred(canvas, "No data", width, height, font, text);
            return Ok(request);
        }

        var supported = type is "line" or "area" or "bar" or "scatter" or "pie" or "donut";

        if (!supported)
        {
            LastOutcome = "unsupported";
            DrawCentred(canvas, $"Unsupported chart type '{request.ChartType}'", width, height, font, text);
            return Failed(request, $"Unsupported chart type '{request.ChartType}'.");
        }

        if (type is "pie" or "donut")
        {
            DrawPie(canvas, width, height, type == "donut", labels, series, palette, font, text);
        }
        else
        {
            DrawCartesian(canvas, width, height, type, labels, series, palette, font, text);
        }

        LastOutcome = "rendered";
        return Ok(request);
    }

    private void DrawCartesian(
        SKCanvas canvas,
        float width,
        float height,
        string type,
        IReadOnlyList<string> labels,
        IReadOnlyList<OgChartSeries> series,
        IReadOnlyList<SKColor> palette,
        SKFont font,
        SKPaint text)
    {
        var plot = new SKRect(56, 18, width - 18, height - 34);
        var count = series.Max(item => item.Values.Count);
        var minimum = series.SelectMany(item => item.Values).DefaultIfEmpty(0).Min();
        var maximum = series.SelectMany(item => item.Values).DefaultIfEmpty(1).Max();

        if (Math.Abs(maximum - minimum) < 1e-9)
        {
            maximum = minimum + 1;
        }
        else
        {
            var padding = (maximum - minimum) * 0.08;
            minimum -= padding;
            maximum += padding;
        }

        using var axis = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1f,
            Color = new SKColor(0xD5, 0xD5, 0xD5),
        };

        for (var line = 0; line <= 4; line++)
        {
            var y = plot.Top + (plot.Height * line / 4f);
            canvas.DrawLine(plot.Left, y, plot.Right, y, axis);

            var value = maximum - ((maximum - minimum) * line / 4d);
            canvas.DrawText(
                value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
                plot.Left - 6,
                y + 4,
                SKTextAlign.Right,
                font,
                text);
        }

        canvas.DrawLine(plot.Left, plot.Top, plot.Left, plot.Bottom, axis);
        canvas.DrawLine(plot.Left, plot.Bottom, plot.Right, plot.Bottom, axis);

        float XOf(int index) => count <= 1
            ? plot.MidX
            : plot.Left + (plot.Width * index / (count - 1f));

        float YOf(double value) =>
            (float)(plot.Bottom - (((value - minimum) / (maximum - minimum)) * plot.Height));

        var points = new List<OgChartPoint>();

        for (var s = 0; s < series.Count; s++)
        {
            var item = series[s];
            var colour = palette[s % palette.Count];

            using var stroke = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.8f,
                Color = colour,
            };

            using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = colour };

            if (type == "bar")
            {
                var slot = plot.Width / Math.Max(1, count);
                var barWidth = Math.Max(2f, (slot / (series.Count + 1f)) * 0.8f);

                for (var i = 0; i < item.Values.Count; i++)
                {
                    var x = plot.Left + (slot * i) + (slot * (s + 0.5f) / series.Count);
                    var y = YOf(item.Values[i]);
                    canvas.DrawRect(new SKRect(x - (barWidth / 2), y, x + (barWidth / 2), plot.Bottom), fill);
                    points.Add(new OgChartPoint(item.Name, LabelOf(labels, i), item.Values[i], x, y));
                }
            }
            else
            {
                if (type is "line" or "area")
                {
                    using var builder = new SKPathBuilder();

                    for (var i = 0; i < item.Values.Count; i++)
                    {
                        var point = new SKPoint(XOf(i), YOf(item.Values[i]));

                        if (i == 0)
                        {
                            builder.MoveTo(point);
                        }
                        else
                        {
                            builder.LineTo(point);
                        }
                    }

                    if (type == "area")
                    {
                        using var area = new SKPaint
                        {
                            IsAntialias = true,
                            Style = SKPaintStyle.Fill,
                            Color = colour.WithAlpha(0x40),
                        };

                        using var areaBuilder = new SKPathBuilder();

                        for (var i = 0; i < item.Values.Count; i++)
                        {
                            var point = new SKPoint(XOf(i), YOf(item.Values[i]));

                            if (i == 0)
                            {
                                areaBuilder.MoveTo(point);
                            }
                            else
                            {
                                areaBuilder.LineTo(point);
                            }
                        }

                        areaBuilder.LineTo(XOf(item.Values.Count - 1), plot.Bottom);
                        areaBuilder.LineTo(XOf(0), plot.Bottom);
                        areaBuilder.Close();

                        using var filled = areaBuilder.Detach();
                        canvas.DrawPath(filled, area);
                    }

                    using var path = builder.Detach();
                    canvas.DrawPath(path, stroke);
                }

                for (var i = 0; i < item.Values.Count; i++)
                {
                    var point = new SKPoint(XOf(i), YOf(item.Values[i]));
                    canvas.DrawCircle(point, type == "scatter" ? 3.2f : 2.4f, fill);
                    points.Add(new OgChartPoint(item.Name, LabelOf(labels, i), item.Values[i], point.X, point.Y));
                }
            }
        }

        LastPoints = points;

        // REL-03 AC-4: series identity is not colour alone — each one is named next to its swatch, so a
        // colour-blind reader can still tell oil from gas. Pie and donut already carry a legend.
        var legendX = plot.Left;

        for (var s = 0; s < series.Count; s++)
        {
            using var swatch = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = palette[s % palette.Count] };
            canvas.DrawRect(new SKRect(legendX, 4, legendX + 10, 14), swatch);

            var label = series[s].Name;

            canvas.DrawText(label, legendX + 14, 12, SKTextAlign.Left, font, text);
            legendX += 22 + (label.Length * 6f);
        }

        var step = Math.Max(1, (int)Math.Ceiling(count / 6d));

        for (var i = 0; i < count; i += step)
        {
            canvas.DrawText(LabelOf(labels, i), XOf(i), plot.Bottom + 16, SKTextAlign.Center, font, text);
        }
    }

    private void DrawPie(
        SKCanvas canvas,
        float width,
        float height,
        bool donut,
        IReadOnlyList<string> labels,
        IReadOnlyList<OgChartSeries> series,
        IReadOnlyList<SKColor> palette,
        SKFont font,
        SKPaint text)
    {
        var values = series[0].Values;
        var total = values.Sum(Math.Abs);

        if (total <= 0)
        {
            LastOutcome = "no data";
            DrawCentred(canvas, "No data", width, height, font, text);
            return;
        }

        var size = Math.Min(width - 190, height - 60);
        var radius = Math.Max(20f, size / 2f);
        var centre = new SKPoint(40 + radius, height / 2f);
        var rectangle = new SKRect(centre.X - radius, centre.Y - radius, centre.X + radius, centre.Y + radius);

        var points = new List<OgChartPoint>();
        var start = -90f;

        for (var i = 0; i < values.Count; i++)
        {
            var sweep = (float)(Math.Abs(values[i]) / total * 360d);
            var colour = palette[i % palette.Count];

            using var slice = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = colour };
            using var builder = new SKPathBuilder();
            builder.MoveTo(centre);
            builder.ArcTo(rectangle, start, sweep, false);
            builder.Close();

            using var path = builder.Detach();
            canvas.DrawPath(path, slice);

            var mid = (start + (sweep / 2f)) * Math.PI / 180d;
            var labelRadius = radius * 0.62f;
            points.Add(new OgChartPoint(
                series[0].Name,
                LabelOf(labels, i),
                values[i],
                centre.X + (float)(Math.Cos(mid) * labelRadius),
                centre.Y + (float)(Math.Sin(mid) * labelRadius)));

            // Legend, one row per slice, to the right of the circle.
            var legendX = centre.X + radius + 18;
            var legendY = (height / 2f) - ((values.Count - 1) * 8f) + (i * 16f);

            canvas.DrawRect(new SKRect(legendX, legendY - 8, legendX + 10, legendY + 2), slice);
            canvas.DrawText(
                $"{LabelOf(labels, i)}  {values[i].ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}",
                legendX + 16,
                legendY,
                SKTextAlign.Left,
                font,
                text);

            start += sweep;
        }

        if (donut)
        {
            using var hole = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = Background };
            canvas.DrawCircle(centre, radius * 0.55f, hole);
        }

        LastPoints = points;
    }

    private static string LabelOf(IReadOnlyList<string> labels, int index) =>
        index < labels.Count ? labels[index] : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static void DrawCentred(SKCanvas canvas, string message, float width, float height, SKFont font, SKPaint text) =>
        canvas.DrawText(message, width / 2f, height / 2f, SKTextAlign.Center, font, text);

    private static RenderChartResult Ok(RenderChartRequest request) => new()
    {
        ChartId = Guid.NewGuid().ToString("n")[..8],
        ChartType = request.ChartType ?? string.Empty,
        IsSuccess = true,
        RenderDate = DateTime.UtcNow,
    };

    private static RenderChartResult Failed(RenderChartRequest? request, string message) => new()
    {
        ChartId = Guid.NewGuid().ToString("n")[..8],
        ChartType = request?.ChartType ?? string.Empty,
        IsSuccess = false,
        ErrorMessage = message,
        RenderDate = DateTime.UtcNow,
    };
}

/// <summary>Reads the loose <c>ChartData</c> dictionary into labels and series.</summary>
internal static class ChartDataReader
{
    public static (IReadOnlyList<string> Labels, IReadOnlyList<OgChartSeries> Series) Read(
        Dictionary<string, object>? data)
    {
        if (data is null)
        {
            return ([], []);
        }

        var labels = Values(data, "labels")
            .Select(value => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty)
            .ToList();

        var series = new List<OgChartSeries>();

        if (data.TryGetValue("series", out var raw))
        {
            switch (raw)
            {
                case IEnumerable<OgChartSeries> typed:
                    series.AddRange(typed);
                    break;

                case IDictionary<string, object> named:
                    foreach (var pair in named)
                    {
                        series.Add(new OgChartSeries(pair.Key, ToDoubles(pair.Value)));
                    }

                    break;

                case System.Collections.IEnumerable list:
                    foreach (var item in list)
                    {
                        if (item is IDictionary<string, object> map)
                        {
                            var name = map.TryGetValue("name", out var n) ? Convert.ToString(n) ?? "Series" : "Series";
                            map.TryGetValue("values", out var v);
                            series.Add(new OgChartSeries(name, ToDoubles(v)));
                        }
                        else if (item is OgChartSeries entry)
                        {
                            series.Add(entry);
                        }
                    }

                    break;
            }
        }
        else if (data.TryGetValue("values", out var values))
        {
            series.Add(new OgChartSeries("Series 1", ToDoubles(values)));
        }

        return (labels, series);
    }

    private static IEnumerable<object?> Values(Dictionary<string, object> data, string key) =>
        data.TryGetValue(key, out var raw) && raw is System.Collections.IEnumerable items and not string
            ? items.Cast<object?>()
            : [];

    private static IReadOnlyList<double> ToDoubles(object? raw)
    {
        if (raw is System.Collections.IEnumerable items and not string)
        {
            var values = new List<double>();

            foreach (var item in items)
            {
                if (item is null)
                {
                    continue;
                }

                if (item is double value)
                {
                    values.Add(value);
                }
                else if (item is IConvertible convertible)
                {
                    values.Add(convertible.ToDouble(System.Globalization.CultureInfo.InvariantCulture));
                }
            }

            return values;
        }

        return raw is IConvertible single
            ? [single.ToDouble(System.Globalization.CultureInfo.InvariantCulture)]
            : [];
    }
}
