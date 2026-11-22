using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Beep.OilandGas.Models.Data.Rendering;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Reporting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// RPT-03: the chart page. One control, one renderer, every chart type the domain contract supports, and a
/// palette taken from the theme so a skin change is a repaint.
/// </summary>
/// <remarks>
/// The request is the domain's <see cref="RenderChartRequest"/>; this page does not define a chart contract of
/// its own. The series are the demo production numbers — the point of the page is the rendering path, not the
/// data, and the smoke run uses it as the end-to-end evidence.
/// </remarks>
public sealed class ChartsPageView : OgPageBase
{
    private static readonly string[] Types = ["line", "area", "bar", "scatter", "pie", "donut"];

    private readonly OgApplication _application;
    private readonly OgSkiaChartRenderer _renderer;
    private readonly ChartHost _host;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly ComboBox _type = new() { MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
    private readonly CheckBox _empty = new() { Content = "Empty series", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };

    private RenderChartRequest? _request;

    public ChartsPageView(OgApplication application)
        : base(application, "charts")
    {
        _application = application;
        _renderer = application.Charts;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _type.ItemsSource = Types;
        _type.SelectedIndex = 0;
        _type.SelectionChanged += (_, _) => Render();

        _empty.Checked += (_, _) => Render();
        _empty.Unchecked += (_, _) => Render();

        _host = new ChartHost
        {
            Renderer = _renderer,
            FailureReporter = application.Diagnostics,
            MinHeight = 320,
            Palette = ThemePalette(),
            ChartBackground = ThemeBackground(),
        };

        _host.PointSelected += (_, point) =>
        {
            LastSelected = point;
            _notice.Text = $"{point.Series} · {point.Label} · {point.Value:0.##}";
        };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Chart"));
        toolbar.Children.Add(_type);
        toolbar.Children.Add(_empty);
        toolbar.Children.Add(Button("Export PNG", (_, _) => Export()));

        var panel = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        panel.Children.Add(_host);

        Named(_type, "Chart type");
        ShowContent(panel);

        Loaded += (_, _) => Render();
    }

    /// <summary>What the renderer said about the last draw: "rendered", "no data", or a failure message.</summary>
    public string? LastMessage { get; private set; }

    public OgChartPointSelected? LastSelected { get; private set; }

    public string? ExportedPath { get; private set; }

    public string SummaryText => _summary.Text;

    /// <summary>How many series the current request carries; the smoke run and tests read this.</summary>
    public int RenderedSeriesCount =>
        _request?.ChartData is { } data
        && data.TryGetValue("series", out var series)
        && series is System.Collections.ICollection collection
            ? collection.Count
            : -1;

    /// <summary>Renders the selected type. Returns false only when the render itself failed.</summary>
    public bool Render()
    {
        var type = _type.SelectedItem as string ?? "line";
        var request = new RenderChartRequest
        {
            ChartType = type,
            Width = 640,
            Height = 360,
            ChartData = _empty.IsChecked == true ? NoData() : DemoData(),
        };

        _request = request;
        _host.Request = request;
        _host.Palette = ThemePalette();
        _host.ChartBackground = ThemeBackground();
        _host.Redraw();

        // Evaluate the same request off-screen: the control cannot draw until WPF has laid it out, and the
        // page must be able to say what happened even when it is constructed headlessly (smoke, tests).
        _renderer.Palette = ThemePalette() ?? OgSkiaChartRenderer.DefaultPalette;
        _renderer.Background = ThemeBackground();

        using (var probe = SKSurface.Create(new SKImageInfo(
                   (int)Math.Round(request.Width),
                   (int)Math.Round(request.Height),
                   SKColorType.Bgra8888,
                   SKAlphaType.Premul)))
        {
            _renderer.RenderChartAsync(request, probe.Canvas, "desktop").GetAwaiter().GetResult();
        }

        LastMessage = _renderer.LastOutcome;
        LastSelected = null;
        _summary.Text = $"{type} · {LastMessage}";
        _notice.Text = LastMessage == "no data"
            ? "The series is empty, so the chart area says so instead of drawing empty axes."
            : string.Empty;

        return LastMessage == "rendered";
    }

    /// <summary>Renders a named type; the smoke run drives every supported chart through this.</summary>
    public bool RenderType(string type)
    {
        if (!Types.Contains(type, StringComparer.Ordinal))
        {
            // Silently rendering whatever was selected before would make a typo look like success.
            _notice.Text = $"'{type}' is not a chart type this build draws.";
            return false;
        }

        // "Render this type" means render it with data: an empty-series choice from a moment ago must not
        // silently turn the export into a "No data" picture.
        _empty.IsChecked = false;
        _type.SelectedItem = type;
        return Render();
    }

    /// <summary>Renders with an empty series, to show the "no data" state (AC-3).</summary>
    public bool RenderEmpty()
    {
        _empty.IsChecked = true;
        return Render();
    }

    /// <summary>Writes the current chart as PNG; the same draw is used, so the file matches the screen.</summary>
    public bool Export()
    {
        try
        {
            if (_request is null)
            {
                return false;
            }

            var folder = Path.Combine(OgPaths.ExportsDirectory, "charts");
            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, $"chart-{_request.ChartType}-{DateTime.Now:yyyyMMdd-HHmmss}.png");

            var request = new ExportChartRequest
            {
                ChartType = _request.ChartType,
                ExportFormat = "png",
                FilePath = path,
                Width = _request.Width,
                Height = _request.Height,
                ChartData = _request.ChartData,
            };

            _renderer.Palette = ThemePalette() ?? OgSkiaChartRenderer.DefaultPalette;
            _renderer.Background = ThemeBackground();

            var result = _renderer.ExportChartAsync(request, "desktop").GetAwaiter().GetResult();

            if (!result.IsSuccess)
            {
                _notice.Text = result.ErrorMessage ?? "The chart was not exported.";
                return false;
            }

            ExportedPath = path;
            _notice.Text = $"Exported {new FileInfo(path).Length / 1024.0:0.#} KB · {path}";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting a chart",
                "no image was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The chart could not be exported.", reference);
            return false;
        }
    }

    private static Dictionary<string, object> DemoData() => new()
    {
        ["labels"] = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
        ["series"] = new List<OgChartSeries>
        {
            new("Oil", [120, 132, 128, 141, 139, 152]),
            new("Gas", [80, 86, 92, 91, 97, 103]),
        },
    };

    private static Dictionary<string, object> NoData() => new()
    {
        ["labels"] = Array.Empty<string>(),
        ["series"] = new List<OgChartSeries>(),
    };

    private static IReadOnlyList<SKColor>? ThemePalette()
    {
        var palette = new List<SKColor>();
        var application = System.Windows.Application.Current;

        for (var index = 1; index <= 8; index++)
        {
            if (application?.TryFindResource($"BeepChartBrush{index}") is SolidColorBrush brush)
            {
                palette.Add(new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A));
            }
        }

        return palette.Count > 0 ? palette : null;
    }

    private static SKColor ThemeBackground() =>
        System.Windows.Application.Current?.TryFindResource("BeepSurfaceBrush") is SolidColorBrush brush
            ? new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : SKColors.White;

    private static Button Button(string text, RoutedEventHandler onClick)
    {
        var button = new Button
        {
            Content = text,
            Padding = new Thickness(10, 3, 10, 3),
            Margin = new Thickness(0, 0, 8, 0),
        };

        button.Click += onClick;
        return button;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };
}
