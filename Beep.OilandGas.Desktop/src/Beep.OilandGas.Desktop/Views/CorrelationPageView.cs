using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-06: the correlation panel — several wells side by side, flattened on a chosen marker, with the checks
/// that say whether the alignment can be trusted.
/// </summary>
/// <remarks>
/// The wells are the deterministic demo set (a third well deliberately lacks the datum), because PPDM data is
/// blocked (G-08). The panel, the flattening and the drawing are the tools'; this page picks the datum, shows
/// the checks and exports the panel as a paginated PDF.
/// </remarks>
public sealed class CorrelationPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly CorrelationView _view = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly CrossSectionView _sectionView = new();
    private readonly SkiaViewSurfaceHost _sectionHost;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _checks;
    private readonly ComboBox _datum = new() { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };

    private CorrelationPanel _panel;
    private IReadOnlyList<OgCorrelationIssue> _issues = [];
    private CancellationTokenSource? _renderCancellation;
    private int _renderGeneration;

    public CorrelationPageView(OgApplication application)
        : base(application, "correlation")
    {
        _application = application;
        _ = application.Wellbore;

        var wells = OgCorrelation.DemoWells();
        _panel = OgCorrelation.Build(wells, OgCorrelation.DemoDatum);

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _checks = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 110,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Severity", nameof(OgCorrelationIssue.Severity), 80),
                Text("Well", nameof(OgCorrelationIssue.Well), 110),
                Text("Code", nameof(OgCorrelationIssue.Code), 190),
                Text("Finding", nameof(OgCorrelationIssue.Message), 430),
            },
        };

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 280,
        };

        // VSB-06 AC-4: the section is a second view of the same wells, placed by their coordinates.
        _sectionHost = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 280,
        };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("Datum"));
        toolbar.Children.Add(_datum);
        _datum.SelectionChanged += (_, _) => Load();

        var reset = new Button { Content = "Reset alignment", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 0, 8, 0) };
        reset.Click += (_, _) =>
        {
            _datum.SelectedIndex = 0;
            Load();
        };
        toolbar.Children.Add(reset);

        var export = new Button { Content = "Export PDF", Padding = new Thickness(10, 3, 10, 3) };
        export.Click += (_, _) => ExportPdf();
        toolbar.Children.Add(export);

        var panel = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        DockPanel.SetDock(_checks, Dock.Bottom);
        panel.Children.Add(_checks);

        var views = new TabControl { IsTabStop = true };
        views.Items.Add(new TabItem { Header = "Correlation", Content = _host });
        views.Items.Add(new TabItem { Header = "Cross-section", Content = _sectionHost });
        Named(views, "Correlation views");
        panel.Children.Add(views);

        Named(_datum, "Datum marker");
        Named(_checks, "Correlation checks");
        ShowContent(panel);
        Loaded += (_, _) => Load();
    }

    public IReadOnlyList<OgCorrelationWellRow> Wells { get; private set; } = [];

    public IReadOnlyList<OgCorrelationIssue> Issues => _issues;

    public CorrelationPanel Panel => _panel;

    /// <summary>The cross-section built from the wells' coordinates, or null when a position is missing.</summary>
    public CrossSectionPanel? Section { get; private set; }

    public IReadOnlyList<OgCorrelationIssue> SectionIssues { get; private set; } = [];

    /// <summary>The number of pinch-outs the checks report (AC-3).</summary>
    public int PinchOuts => _issues.Count(issue => issue.Code == OgCorrelation.CodePinchOut);

    /// <summary>True when a render was superseded or cancelled before it could be shown (AC-6).</summary>
    public bool LastRenderCancelled { get; private set; }

    /// <summary>
    /// Cancels the render in flight. Returns true when there was one to cancel.
    /// </summary>
    public bool CancelPendingRender()
    {
        if (_renderCancellation is null || _renderCancellation.IsCancellationRequested)
        {
            return false;
        }

        _renderCancellation.Cancel();
        return true;
    }

    /// <summary>
    /// Starts a render of <paramref name="wells"/> and cancels the one before it. The result is not applied: the
    /// caller shows it with <see cref="Show"/>, so a test can drive a render without a dispatcher.
    /// </summary>
    public async Task<OgCorrelation.OgCorrelationRender?> RenderAsync(
        IReadOnlyList<(string Name, TheTechIdea.OilGas.Wellbore.WellboreDocument Document)> wells,
        string? datum,
        int width = 900,
        int height = 560)
    {
        ArgumentNullException.ThrowIfNull(wells);

        CancelPendingRender();

        var cancellation = new CancellationTokenSource();
        _renderCancellation = cancellation;

        _wells = wells;
        var generation = ++_renderGeneration;
        var token = cancellation.Token;

        var work = Task.Run(
            () => OgCorrelation.RenderChunked(wells, datum, width, height, token),
            token);

        // A cancel is an ordinary answer here, not a failure: it is read from the task's state rather than
        // caught, so nothing is written to the failure store each time a person changes the well set.
        await Task.WhenAny(work).ConfigureAwait(false);

        if (work.IsCanceled)
        {
            LastRenderCancelled = true;
            return null;
        }

        if (work.IsFaulted)
        {
            // A real failure is not swallowed: it reaches the caller's reporter with its own exception.
            throw work.Exception!.GetBaseException();
        }

        if (generation != _renderGeneration)
        {
            LastRenderCancelled = true;
            return null;
        }

        return work.Result;
    }

    /// <summary>
    /// Replaces the well set: the pending render is cancelled and the new one is shown when it finishes.
    /// </summary>
    public void ChangeWellSet(
        IReadOnlyList<(string Name, TheTechIdea.OilGas.Wellbore.WellboreDocument Document)> wells,
        string? datum)
    {
        var generationTask = RenderAsync(wells, datum);
        _ = ShowWhenReady(generationTask);
    }

    /// <summary>Applies a rendered panel to the view; the caller must be on the UI thread.</summary>
    public void Show(OgCorrelation.OgCorrelationRender render)
    {
        ArgumentNullException.ThrowIfNull(render);

        _panel = render.Panel;
        _issues = render.Issues;
        Wells = render.Columns;
        _checks.ItemsSource = _issues;

        _view.Panel = _panel;
        _host.Surface = _view;
        _host.Redraw();

        var summary = OgCorrelation.Summarise(_panel, _issues);

        _summary.Text =
            $"{summary.Wells} well(s) · {summary.SharedTops} shared top(s) · datum {summary.Datum} · " +
            $"{summary.MissingDatum} without the datum · {summary.WithoutTops} without tops · " +
            $"{PinchOuts} pinch-out(s)";

        _notice.Text = _issues.Count == 0
            ? string.Empty
            : string.Join("  ·  ", _issues.Select(issue => issue.Message));
    }

    private async Task ShowWhenReady(Task<OgCorrelation.OgCorrelationRender?> pending)
    {
        var render = await pending.ConfigureAwait(true);

        if (render is null || Dispatcher is null)
        {
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            Show(render);
        }
        else
        {
            Dispatcher.Invoke(() => Show(render));
        }
    }

    public string SummaryText => _summary.Text;

    /// <summary>Rebuilds the panel for the chosen datum, shows the checks and redraws.</summary>
    public bool Load()
    {
        try
        {
            // A render already in flight is for the previous well set; it must not land on this one.
            CancelPendingRender();

            var datum = _datum.SelectedItem as string;
            var wells = _wells ?? OgCorrelation.DemoWells();

            _panel = OgCorrelation.Build(wells, datum == "(none)" ? null : datum);

            // AC-6: a well-set change invalidates whatever was in flight; the page renders only the newest
            // request, so a stale panel can never overwrite the current one.
            _renderGeneration++;
            var generation = _renderGeneration;

            var columns = OgCorrelation.Columns(_panel);

            // AC-4: the section uses the wells' positions; AC-3: its horizon terminations are checks.
            var section = OgCorrelation.BuildSection(wells, OgCorrelation.DemoPositions());
            Section = section.Success ? section.Value : null;
            SectionIssues = section.Success
                ? OgCorrelation.SectionChecks(section.Value!)
                : [new OgCorrelationIssue("Warning", section.Reference ?? OgCorrelation.CodeNoCoordinates, section.Message ?? "No section.", null)];

            _issues = OgCorrelation.Checks(_panel, _panel.FlattenOn).Concat(SectionIssues).ToList();

            if (generation != _renderGeneration)
            {
                return false;
            }

            Wells = columns;
            _checks.ItemsSource = _issues;

            _view.Panel = _panel;
            _host.Surface = _view;
            _host.Redraw();

            if (Section is not null)
            {
                _sectionView.Panel = Section;
                _sectionHost.Surface = _sectionView;
                _sectionHost.Redraw();
            }

            var summary = OgCorrelation.Summarise(_panel, _issues);
            _summary.Text =
                $"{summary.Wells} well(s) · {summary.SharedTops} shared top(s) · datum {summary.Datum} · " +
                $"{summary.MissingDatum} without the datum · {summary.WithoutTops} without tops · " +
                $"{PinchOuts} pinch-out(s)";

            _notice.Text = _issues.Count == 0
                ? string.Empty
                : string.Join("  ·  ", _issues.Select(issue => issue.Message));

            FillDatumChoices();
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "drawing the correlation panel",
                "the panel is not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The correlation panel could not be drawn.", reference);
            return false;
        }
    }

    private IReadOnlyList<(string Name, TheTechIdea.OilGas.Wellbore.WellboreDocument Document)>? _wells;

    /// <summary>Writes the panel as a paginated PDF under the data root's export folder.</summary>
    public bool ExportPdf()
    {
        try
        {
            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"correlation-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");

            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            using var stream = File.Create(path);
            var pages = OgCorrelation.ExportPdf(_panel, stream);
            stream.Flush();

            var bytes = new FileInfo(path).Length;
            _summary.Text = $"{pages} page(s) · {bytes / 1024.0:0.#} KB · {path}";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the correlation panel",
                "no PDF was produced",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The correlation panel could not be exported.", reference);
            return false;
        }
    }

    private void FillDatumChoices()
    {
        if (_datum.Items.Count > 0)
        {
            return;
        }

        var choices = new List<string> { "(none)" };
        choices.AddRange(_panel.SharedTops());
        choices.Add(OgCorrelation.DemoDatum);

        _datum.ItemsSource = choices.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        _datum.SelectedItem = OgCorrelation.DemoDatum;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 4, 0),
        FontSize = 11.5,
    };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new System.Windows.Data.Binding(path),
        Width = new DataGridLength(width),
    };
}
