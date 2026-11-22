using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-05: the well schematic — casing, completion, barriers and annuli drawn by the tools' renderer, with
/// the panels a person checks before trusting the picture.
/// </summary>
/// <remarks>
/// Every fact on this page comes from the tools: the validator, the annulus screening, the barrier set and
/// the tally. The page's own job is to show them and to say "not computed" where the tools have no value.
/// The demo document is used because no PPDM tubular records exist yet (G-08).
/// </remarks>
public sealed class WellSchematicPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly WellSchematicView _view = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly TabControl _panels;

    private WellSchematicDocument _document = OgSchematicMapper.Demo();

    public WellSchematicPageView(OgApplication application)
        : base(application, "schematic")
    {
        _application = application;

        // VSB-02: the environment must be loaded before a schematic surface is constructed.
        _ = application.Wellbore;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 280,
        };

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);
        _panels = new TabControl { Height = 190, Margin = new Thickness(0, 8, 0, 0), IsTabStop = true };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

        var fit = new Button { Content = "Fit", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
        fit.Click += (_, _) =>
        {
            // ResetView already invalidates the surface through the tools' own event.
            _view.ResetView();
            _host.Redraw();
        };
        toolbar.Children.Add(fit);

        var reload = new Button { Content = "Reload", Padding = new Thickness(10, 4, 10, 4) };
        reload.Click += (_, _) => Load();
        toolbar.Children.Add(reload);

        var panel = new DockPanel();
        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_panels, Dock.Bottom);
        panel.Children.Add(_panels);
        panel.Children.Add(_host);

        Named(_panels, "Panels");
        ShowContent(panel);
        Loaded += (_, _) => Load();
    }

    public IReadOnlyList<OgSchematicStringRow> Strings { get; private set; } = [];

    public IReadOnlyList<OgAnnulusRow> Annuli { get; private set; } = [];

    public IReadOnlyList<OgBarrierRow> Barriers { get; private set; } = [];

    public IReadOnlyList<SchematicIssue> Findings { get; private set; } = [];

    public TallyTable? Tally { get; private set; }

    /// <summary>The barriers drawn with the "no evidence" hatch.</summary>
    public IReadOnlyList<OgUnverifiedBarrier> UnverifiedBarriers { get; private set; } = [];

    public string SummaryText => _summary.Text;

    /// <summary>How long the last <see cref="RenderFrame"/> took, in milliseconds (REL-04).</summary>
    public double LastFrameMilliseconds { get; private set; }

    /// <summary>
    /// REL-04 AC-1: draws one frame at a fixed size and reports its cost. The benchmark times a frame, not a
    /// window: constructing the page and its theme is a startup cost, not a render cost.
    /// </summary>
    public bool RenderFrame(int width, int height)
    {
        using var renderer = new WellTools.Hosting.ViewSurfaceRenderer { Surface = _view };
        var started = System.Diagnostics.Stopwatch.StartNew();

        var drawn = renderer.Render(width, height);

        started.Stop();
        LastFrameMilliseconds = started.Elapsed.TotalMilliseconds;

        return drawn;
    }

    /// <summary>Loads the document and redraws. Kept public so the smoke run and tests can drive it.</summary>
    public bool Load()
    {
        try
        {
            _view.Schematic = _document;
            _view.Document = WellSchematicView.Build(_document);
            _host.Surface = _view;

            Strings = OgSchematicMapper.Strings(_document);
            Annuli = OgSchematicMapper.Annuli(_document);
            Barriers = OgSchematicMapper.Barriers(_document);
            Findings = OgSchematicMapper.Findings(_document);
            Tally = OgSchematicMapper.Tally(_document);

            // AC-3: the tools draw nothing for an unverified barrier, so the page marks it on the drawing.
            var unverified = OgSchematicBarrierOverlay.Unverified(_document, _view.LastPlan);
            UnverifiedBarriers = unverified;
            _host.Overlay = canvas =>
                OgSchematicBarrierOverlay.Draw(canvas, _document, unverified);

            FillPanels();
            var summary = OgSchematicMapper.Summarise(_document);
            _summary.Text =
                $"{summary.Name} · {summary.Strings} strings · {summary.Equipment} components · " +
                $"{summary.Annuli} annuli · {summary.Barriers} barriers ({summary.UnverifiedBarriers} unverified) · " +
                $"{summary.Findings} finding(s) · {summary.DepthRange}" +
                (UnverifiedBarriers.Count > 0
                    ? $"   ·   {UnverifiedBarriers.Count} barrier(s) hatched: no evidence"
                    : string.Empty);

            var warnings = OgSchematicMapper.Warnings(_document);
            _notice.Text = warnings.Count == 0 ? string.Empty : string.Join("  ·  ", warnings);

            return _host.Redraw();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "loading the well schematic",
                "the schematic was not drawn",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The well schematic could not be drawn.", reference);
            return false;
        }
    }

    private void FillPanels()
    {
        _panels.Items.Clear();

        _panels.Items.Add(Tab("Annuli", Grid(
            new[] { "Annulus", "Fluid", "MAASP", "Screening" },
            Annuli.Select(row => new[] { row.Name, row.Fluid, row.Maasp, row.Status }))));

        _panels.Items.Add(Tab("Barriers", Grid(
            new[] { "Element", "Role", "Interval", "Status", "Evidence" },
            Barriers.Select(row => new[] { row.Element, row.Role, row.Interval, row.Status, row.Evidence }))));

        _panels.Items.Add(Tab("Validation", Grid(
            new[] { "Severity", "Code", "Finding", "Element" },
            Findings.Select(issue => new[]
            {
                issue.Severity.ToString(),
                issue.Code,
                issue.Message,
                issue.ItemId ?? "(document)",
            }))));

        if (Tally is { } tally)
        {
            var headers = tally.Columns.Select(column => column.header).ToArray();
            _panels.Items.Add(Tab("Tally", Grid(headers, tally.Rows)));
        }
    }

    private static TabItem Tab(string header, FrameworkElement content) => new()
    {
        Header = header,
        Content = content,
    };

    /// <summary>A read-only grid built from a real <see cref="System.Data.DataTable"/>, so the columns come
    /// from the headers rather than from a dictionary's keys.</summary>
    private static DataGrid Grid(IReadOnlyList<string> headers, IEnumerable<string[]> rows)
    {
        var table = new System.Data.DataTable();

        foreach (var header in headers)
        {
            table.Columns.Add(header);
        }

        foreach (var row in rows)
        {
            var values = new object[headers.Count];

            for (var i = 0; i < headers.Count; i++)
            {
                values[i] = i < row.Length ? row[i] : string.Empty;
            }

            table.Rows.Add(values);
        }

        return new DataGrid
        {
            AutoGenerateColumns = true,
            IsReadOnly = true,
            ItemsSource = table.DefaultView,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };
    }
}
