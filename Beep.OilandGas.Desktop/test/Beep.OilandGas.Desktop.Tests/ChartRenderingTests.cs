using System.Security.Cryptography;
using System.Threading;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Rendering;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Reporting;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RPT-03: every supported chart type renders, colours come from the palette, an empty series says so, a failure
/// is reported, a click selects a point, and an export matches the frame on screen.
/// </summary>
public sealed class ChartRenderingTests
{
    private static readonly SKColor Red = new(255, 0, 0);
    private static readonly SKColor Green = new(0, 160, 0);

    [Theory]
    [InlineData("line")]
    [InlineData("area")]
    [InlineData("bar")]
    [InlineData("scatter")]
    [InlineData("pie")]
    [InlineData("donut")]
    public void Every_supported_chart_type_renders(string type)
    {
        var renderer = new OgSkiaChartRenderer();
        var (result, frame) = Draw(renderer, type, Series());

        // AC-1
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("rendered", renderer.LastOutcome);
        Assert.Contains(frame, value => value != 0);
    }

    [Fact]
    public void Colours_come_from_the_palette_and_follow_a_change()
    {
        var renderer = new OgSkiaChartRenderer
        {
            Palette = [Red, Red],
        };

        var (_, first) = Draw(renderer, "line", Series());
        Assert.Contains(Pixels(first), pixel => pixel == Red);

        // AC-2: a different palette is a different picture, with no trace of the old colour.
        renderer.Palette = [Green, Green];
        var (_, second) = Draw(renderer, "line", Series());

        Assert.Contains(Pixels(second), pixel => pixel == Green);
        Assert.DoesNotContain(Pixels(second), pixel => pixel == Red);
    }

    [Fact]
    public void An_empty_series_says_no_data_instead_of_drawing_empty_axes()
    {
        var renderer = new OgSkiaChartRenderer();
        var (result, frame) = Draw(renderer, "line", new List<OgChartSeries>());

        // AC-3
        Assert.True(result.IsSuccess);
        Assert.Equal("no data", renderer.LastOutcome);
        Assert.Empty(renderer.LastPoints);

        // Something was drawn (the sentence), so the chart area is not blank.
        Assert.Contains(frame, value => value != 0);
    }

    [Fact]
    public void A_chart_describes_itself_in_words_for_a_screen_reader()
    {
        var renderer = new OgSkiaChartRenderer { Palette = [Red, Green], Background = SKColors.White };
        var surface = new ChartSurface
        {
            Renderer = renderer,
            Request = Request("line", Series()),
            Palette = [Red, Green],
            Width = 400,
            Height = 300,
        };

        using var view = new ViewSurfaceRenderer { Surface = surface };
        Assert.True(view.Render(400, 300));

        // REL-03 AC-5: the canvas is not a mystery to UI Automation - it names its type and each series' range.
        Assert.Equal("Chart", surface.AccessibleName);
        Assert.Contains("line", surface.AccessibleDescription, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2 series", surface.AccessibleDescription, StringComparison.Ordinal);
        Assert.Contains("Oil 10 to 25", surface.AccessibleDescription, StringComparison.Ordinal);
        Assert.Contains("Gas 8 to 18", surface.AccessibleDescription, StringComparison.Ordinal);

        // An empty chart says so rather than describing nothing.
        surface.Request = Request("pie", []);
        Assert.True(view.Render(400, 300));
        Assert.Contains("no data", surface.AccessibleDescription, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_render_failure_is_reported_and_drawn_rather_than_swallowed()
    {
        var reporter = new RecordingFailures();
        var surface = new ChartSurface
        {
            Renderer = new ThrowingRenderer(),
            Request = Request("line", Series()),
            Palette = [Red],
            ChartBackground = SKColors.White,
            FailureReporter = reporter,
        };

        using var renderer = new ViewSurfaceRenderer { Surface = surface };
        Assert.True(renderer.Render(320, 200));

        // AC-4: the failure is on screen and in the store, with a reference.
        Assert.Single(reporter.Reported);
        Assert.Contains("Reference", surface.LastMessage, StringComparison.Ordinal);
        Assert.Contains(renderer.Frame!, value => value != 0);
    }

    [Fact]
    public void A_click_selects_the_nearest_drawn_point()
    {
        var renderer = new OgSkiaChartRenderer();
        var surface = new ChartSurface
        {
            Renderer = renderer,
            Request = Request("line", Series()),
            Palette = [Red],
            Width = 400,
            Height = 300,
        };

        OgChartPointSelected? selected = null;
        surface.PointSelected += (_, point) => selected = point;

        using var view = new ViewSurfaceRenderer { Surface = surface };
        Assert.True(view.Render(400, 300));

        var target = surface.LastPoints[2];

        // AC-5: a click near a marker reports the series, label and value under it.
        Assert.True(surface.HandleMouseUp(new SKPoint(target.X + 2, target.Y + 2)));

        Assert.NotNull(selected);
        Assert.Equal(target.Series, selected!.Series);
        Assert.Equal(target.Label, selected.Label);
        Assert.Equal(target.Value, selected.Value);

        // A click far from every point selects nothing.
        Assert.False(surface.HandleMouseUp(new SKPoint(2, 2)));
    }

    [Fact]
    public async Task An_export_matches_the_frame_on_screen()
    {
        var renderer = new OgSkiaChartRenderer
        {
            Palette = [Red, Green],
            Background = SKColors.White,
        };

        var surface = new ChartSurface
        {
            Renderer = renderer,
            Request = Request("bar", Series()),
            Palette = [Red, Green],
            ChartBackground = SKColors.White,
            Width = 320,
            Height = 200,
        };

        using var view = new ViewSurfaceRenderer { Surface = surface };
        Assert.True(view.Render(320, 200));
        var onScreen = view.Frame!.ToArray();

        var path = Path.Combine(
            TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping.OgPaths.DataRoot,
            "charts-" + Guid.NewGuid().ToString("n")[..8],
            "chart.png");

        var exported = await renderer
            .ExportChartAsync(
                new ExportChartRequest
                {
                    ChartType = "bar",
                    ExportFormat = "png",
                    FilePath = path,
                    Width = 320,
                    Height = 200,
                    ChartData = SeriesData(),
                },
                "test");

        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        Assert.True(File.Exists(path));

        // AC-6: the same draw, so the pixels match. Compared through SKColor so the storage order does not
        // matter; a handful of antialiased edge pixels are allowed to differ by at most 2 per channel.
        using var decoded = SKBitmap.Decode(path);
        Assert.NotNull(decoded);
        Assert.Equal(320, decoded!.Width);
        Assert.Equal(200, decoded.Height);

        using var image = SKImage.FromPixelCopy(
            new SKImageInfo(320, 200, SKColorType.Bgra8888, SKAlphaType.Premul),
            onScreen);

        using var fromScreen = SKBitmap.FromImage(image);

        var differing = 0;
        var worst = 0;
        var samples = new List<string>();

        for (var y = 0; y < 200; y++)
        {
            for (var x = 0; x < 320; x++)
            {
                var expected = fromScreen.GetPixel(x, y);
                var actual = decoded.GetPixel(x, y);

                var delta = Math.Max(
                    Math.Max(Math.Abs(expected.Red - actual.Red), Math.Abs(expected.Green - actual.Green)),
                    Math.Max(Math.Abs(expected.Blue - actual.Blue), Math.Abs(expected.Alpha - actual.Alpha)));

                if (delta > 2)
                {
                    differing++;
                    worst = Math.Max(worst, delta);

                    if (samples.Count < 3)
                    {
                        samples.Add($"({x},{y}) screen={expected} export={actual}");
                    }
                }
            }
        }

        Assert.True(
            differing == 0,
            $"{differing} of {320 * 200} pixels differ (worst channel delta {worst}). " +
            $"surface={surface.Width}x{surface.Height} request={surface.Request!.Width}x{surface.Request.Height} " +
            $"pixels={view.PixelWidth}x{view.PixelHeight} scale={view.Scale} " +
            $"screen bbox={BoundingBox(fromScreen)} export bbox={BoundingBox(decoded)}. {string.Join("; ", samples)}");
    }

    [Fact]
    public void The_page_renders_data_for_a_type_and_clears_the_empty_choice()
    {
        RunOnSta(() =>
        {
            using var application = new TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping.OgBootstrapper().Create();
            var page = new TheTechIdea.Beep.OilandGas.Desktop.Views.ChartsPageView(application);

            Assert.True(page.RenderType("line"));
            Assert.Equal(2, page.RenderedSeriesCount);

            Assert.False(page.RenderType("not-a-chart"));
            Assert.Equal(2, page.RenderedSeriesCount);

            page.RenderEmpty();
            Assert.Equal(0, page.RenderedSeriesCount);

            // Asking for a type again must bring the data back, not keep the "no data" choice.
            Assert.True(page.RenderType("bar"));
            Assert.Equal(2, page.RenderedSeriesCount);
        });
    }

    [Fact]
    public async Task An_export_writes_the_data_not_a_no_data_picture()
    {
        var renderer = new OgSkiaChartRenderer { Palette = [Red, Green], Background = SKColors.White };
        var folder = Path.Combine(
            TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping.OgPaths.DataRoot,
            "charts-" + Guid.NewGuid().ToString("n")[..8]);

        Directory.CreateDirectory(folder);

        var withData = Path.Combine(folder, "data.png");
        var empty = Path.Combine(folder, "empty.png");

        var data = await renderer.ExportChartAsync(
            new ExportChartRequest
            {
                ChartType = "line",
                ExportFormat = "png",
                FilePath = withData,
                Width = 400,
                Height = 300,
                ChartData = SeriesData(),
            },
            "test");

        var none = await renderer.ExportChartAsync(
            new ExportChartRequest
            {
                ChartType = "line",
                ExportFormat = "png",
                FilePath = empty,
                Width = 400,
                Height = 300,
                ChartData = new Dictionary<string, object>
                {
                    ["labels"] = Array.Empty<string>(),
                    ["series"] = new List<OgChartSeries>(),
                },
            },
            "test");

        Assert.True(data.IsSuccess, data.ErrorMessage);
        Assert.True(none.IsSuccess, none.ErrorMessage);

        // If the exporter ignored the data, both pictures would be the "No data" placeholder.
        Assert.NotEqual(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(withData))),
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(empty))));
    }

    [Fact]
    public void An_unsupported_chart_type_is_a_failure_that_names_it()
    {
        var renderer = new OgSkiaChartRenderer();
        var (result, _) = Draw(renderer, "radar", Series());

        Assert.False(result.IsSuccess);
        Assert.Contains("radar", result.ErrorMessage!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("unsupported", renderer.LastOutcome);
    }

    [Fact]
    public async Task Configurations_round_trip_in_session()
    {
        var renderer = new OgSkiaChartRenderer();

        var saved = await renderer
            .SaveChartConfigurationAsync(
                new SaveChartConfigurationRequest
                {
                    ChartType = "line",
                    ConfigurationName = "Monthly",
                    ConfigurationData = "{}",
                    IsDefault = true,
                },
                "tester");

        Assert.False(string.IsNullOrWhiteSpace(saved.CHART_CONFIGURATION_ID));

        Assert.NotNull(await renderer.GetChartConfigurationAsync(saved.CHART_CONFIGURATION_ID));
        Assert.Single(await renderer.GetChartConfigurationsAsync("line"));
        Assert.NotNull(await renderer.GetDefaultChartConfigurationAsync("line"));
        Assert.True(await renderer.DeleteChartConfigurationAsync(saved.CHART_CONFIGURATION_ID, "tester"));
        Assert.Null(await renderer.GetChartConfigurationAsync(saved.CHART_CONFIGURATION_ID));
    }

    [Fact]
    public void The_desktop_never_touches_the_retired_svg_helper()
    {
        // ADR-001: Models still declares SkiaSharp.Svg for one helper it owns. The desktop excludes its assets
        // so only SkiaSharp 4.154.0 loads; a reference here would silently bring the 1.x binding back.
        var offenders = SourceRoot.AllSources()
            .Where(path => File.ReadAllText(path).Contains("SkiaSharp.Extended.Svg", StringComparison.Ordinal)
                           || File.ReadAllText(path).Contains("new SKSvg", StringComparison.Ordinal))
            .Select(Path.GetFileName)
            .ToArray();

        Assert.True(
            offenders.Length == 0,
            $"ADR-001: the desktop must not use SkiaSharp.Svg. Offenders: {string.Join(", ", offenders)}");
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (failure is not null)
        {
            throw failure;
        }
    }

    private static (RenderChartResult Result, byte[] Frame) Draw(
        OgSkiaChartRenderer renderer,
        string type,
        IReadOnlyList<OgChartSeries> series)
    {
        renderer.Palette = renderer.Palette.Count > 0 ? renderer.Palette : [Red, Green];
        renderer.Background = SKColors.White;

        using var surface = SKSurface.Create(new SKImageInfo(400, 300, SKColorType.Bgra8888, SKAlphaType.Premul));

        var result = renderer
            .RenderChartAsync(Request(type, series), surface.Canvas, "test")
            .GetAwaiter()
            .GetResult();

        surface.Canvas.Flush();

        using var image = surface.Snapshot();
        using var bitmap = SKBitmap.FromImage(image);
        var frame = new byte[400 * 300 * 4];
        bitmap.GetPixelSpan().CopyTo(frame);

        return (result, frame);
    }

    private static string BoundingBox(SKBitmap bitmap)
    {
        var left = int.MaxValue;
        var top = int.MaxValue;
        var right = int.MinValue;
        var bottom = int.MinValue;

        for (var y = 0; y < bitmap.Height; y++)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, y) == SKColors.White)
                {
                    continue;
                }

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        return left == int.MaxValue ? "(blank)" : $"{left},{top}..{right},{bottom}";
    }

    private static IEnumerable<SKColor> Pixels(byte[] frame)    {
        for (var index = 0; index < frame.Length; index += 4)
        {
            yield return new SKColor(frame[index + 2], frame[index + 1], frame[index], frame[index + 3]);
        }
    }

    private static RenderChartRequest Request(string type, IReadOnlyList<OgChartSeries> series) => new()
    {
        ChartType = type,
        Width = 400,
        Height = 300,
        ChartData = new Dictionary<string, object>
        {
            ["labels"] = new[] { "Jan", "Feb", "Mar", "Apr" },
            ["series"] = series,
        },
    };

    private static Dictionary<string, object> SeriesData() => new()
    {
        ["labels"] = new[] { "Jan", "Feb", "Mar", "Apr" },
        ["series"] = Series(),
    };

    private static IReadOnlyList<OgChartSeries> Series() =>
    [
        new("Oil", [10, 20, 15, 25]),
        new("Gas", [8, 12, 18, 14]),
    ];

    private sealed class ThrowingRenderer : IRenderingService
    {
        public Task<RenderChartResult> RenderChartAsync(
            RenderChartRequest request,
            SKCanvas canvas,
            string userId,
            string connectionName = "PPDM39") =>
            throw new InvalidOperationException("probe: the chart renderer failed");

        public Task<ExportChartResult> ExportChartAsync(ExportChartRequest request, string userId, string connectionName = "PPDM39") =>
            throw new NotSupportedException();

        public Task<CHART_CONFIGURATION> SaveChartConfigurationAsync(SaveChartConfigurationRequest request, string userId, string connectionName = "PPDM39") =>
            throw new NotSupportedException();

        public Task<CHART_CONFIGURATION?> GetChartConfigurationAsync(string configurationId, string connectionName = "PPDM39") =>
            throw new NotSupportedException();

        public Task<List<CHART_CONFIGURATION>> GetChartConfigurationsAsync(string? chartType, string connectionName = "PPDM39") =>
            throw new NotSupportedException();

        public Task<CHART_CONFIGURATION?> GetDefaultChartConfigurationAsync(string chartType, string connectionName = "PPDM39") =>
            throw new NotSupportedException();

        public Task<bool> DeleteChartConfigurationAsync(string configurationId, string userId, string connectionName = "PPDM39") =>
            throw new NotSupportedException();
    }

    private sealed class RecordingFailures : IFailureReporter
    {
        public List<string> Reported { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Reported.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
