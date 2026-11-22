using System.Windows;
using System.Windows.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-07: the well path — plan, section, the anti-collision ladder plot and the travelling cylinder.
/// </summary>
/// <remarks>
/// Plan and section draw the <b>same</b> station list (one source, two projections), so they cannot disagree.
/// The ladder and the cylinder are the tools' own surfaces over the tools' own
/// <c>AntiCollision.Scan</c> result — the maths belongs to ENG-01 and is called, never copied.
/// </remarks>
public sealed class WellPathPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly OgPathSurface _pathSurface = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _stations;
    private readonly ComboBox _view = new() { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
    private readonly ComboBox _offsetSet = new() { MinWidth = 180, Margin = new Thickness(0, 0, 8, 0) };

    private IReadOnlyList<OgSurveyStation> _stationRows = [];
    private TheTechIdea.OilGas.Wellbore.Data.AntiCollisionResult? _collision;
    private IReadOnlyList<OgOffsetWell> _offsets = OgWellPath.DemoOffsetWells;
    private CancellationTokenSource? _offsetCancellation;
    private int _offsetGeneration;

    public WellPathPageView(OgApplication application)
        : base(application, "wellpath")
    {
        _application = application;
        _ = application.Wellbore;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _view.ItemsSource = new[] { "Plan", "Section", "Ladder plot", "Travelling cylinder" };
        _view.SelectedIndex = 0;
        _view.SelectionChanged += (_, _) => ShowSelectedView();

        _stations = new DataGrid
        {
            IsTabStop = true, // REL-03 AC-1: a read-only grid must be reachable by Tab.
            AutoGenerateColumns = false,
            IsReadOnly = true,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("MD", nameof(OgSurveyStation.Md), 70),
                Text("Incl", nameof(OgSurveyStation.Inclination), 60),
                Text("Azi", nameof(OgSurveyStation.Azimuth), 60),
                Text("TVD", nameof(OgSurveyStation.Tvd), 75),
                Text("North", nameof(OgSurveyStation.North), 75),
                Text("East", nameof(OgSurveyStation.East), 75),
                Text("Dogleg", nameof(OgSurveyStation.Dogleg), 70),
            },
        };

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 280,
        };

        _offsetSet.ItemsSource = OgWellPath.OffsetSets.Keys.ToList();
        _offsetSet.SelectedIndex = 0;
        _offsetSet.SelectionChanged += (_, _) =>
        {
            if (_offsetSet.SelectedItem is string name)
            {
                ChangeOffsetSet(name);
            }
        };

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Caption("View"));
        toolbar.Children.Add(_view);
        toolbar.Children.Add(Caption("Offsets"));
        toolbar.Children.Add(_offsetSet);

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
        DockPanel.SetDock(_stations, Dock.Bottom);
        panel.Children.Add(_stations);
        panel.Children.Add(_host);

        Named(_view, "View");
        Named(_offsetSet, "Offset wells");
        Named(_stations, "Survey stations");
        ShowContent(panel);
        Loaded += (_, _) => Load();
    }

    public IReadOnlyList<OgSurveyStation> Stations => _stationRows;

    public IReadOnlyList<OgPathPoint> PlanPoints { get; private set; } = [];

    public IReadOnlyList<OgPathPoint> SectionPoints { get; private set; } = [];

    public (double Azimuth, bool Assumed, string? Note) SectionAzimuth { get; private set; }

    public OgPathUncertainty Uncertainty { get; private set; } = new(false, string.Empty, []);

    public (string Offset, double Factor, double Distance)? Minimum { get; private set; }

    public string SummaryText => _summary.Text;

    /// <summary>The offset wells in the current set.</summary>
    public IReadOnlyList<OgOffsetWell> Offsets => _offsets;

    public string OffsetSetName => _offsetSet.SelectedItem as string ?? "One offset (demo)";

    /// <summary>True when an offset computation was superseded or cancelled before it could be applied (AC-6).</summary>
    public bool LastComputationCancelled { get; private set; }

    /// <summary>Cancels the offset computation in flight; true when there was one.</summary>
    public bool CancelPendingOffsetComputation()
    {
        if (_offsetCancellation is null || _offsetCancellation.IsCancellationRequested)
        {
            return false;
        }

        _offsetCancellation.Cancel();
        return true;
    }

    /// <summary>
    /// Scans <paramref name="offsets"/> while cancelling the scan before it, without touching the UI, so a test
    /// can drive it. A cancelled or superseded scan returns null.
    /// </summary>
    public async Task<(TheTechIdea.OilGas.Wellbore.Data.AntiCollisionResult Result, IReadOnlyList<string> Warnings)?> ComputeOffsetAsync(
        IReadOnlyList<OgOffsetWell> offsets)
    {
        ArgumentNullException.ThrowIfNull(offsets);

        CancelPendingOffsetComputation();

        var cancellation = new CancellationTokenSource();
        _offsetCancellation = cancellation;

        var generation = ++_offsetGeneration;
        var token = cancellation.Token;

        var work = Task.Run(() => OgWellPath.ComputeOffsets(offsets, token), token);

        // A cancel is an ordinary answer, read from the task's state rather than caught: changing the offset
        // set must not write a failure record each time.
        await Task.WhenAny(work).ConfigureAwait(false);

        if (work.IsCanceled)
        {
            LastComputationCancelled = true;
            return null;
        }

        if (work.IsFaulted)
        {
            throw work.Exception!.GetBaseException();
        }

        if (generation != _offsetGeneration)
        {
            LastComputationCancelled = true;
            return null;
        }

        _offsets = offsets;
        return work.Result;
    }

    /// <summary>Replaces the offset set: the pending scan is cancelled and the new one is shown when it finishes.</summary>
    public void ChangeOffsetSet(string name)
    {
        if (!OgWellPath.OffsetSets.TryGetValue(name, out var offsets))
        {
            return;
        }

        _offsets = offsets;
        _ = ShowOffsetsWhenReady(ComputeOffsetAsync(offsets));
    }

    /// <summary>Applies an offset scan to the panel; the caller must be on the UI thread.</summary>
    public void ShowOffsets(
        (TheTechIdea.OilGas.Wellbore.Data.AntiCollisionResult Result, IReadOnlyList<string> Warnings) scan)
    {
        _collision = scan.Result;
        Minimum = OgWellPath.Minimum(scan.Result);

        UpdateSummary();

        _notice.Text = string.Join(
            "  ·  ",
            new[]
            {
                SectionAzimuth.Note,
                Uncertainty.Statement,
                scan.Warnings.Count == 0 ? null : string.Join(" ", scan.Warnings),
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        ShowSelectedView();
    }

    private async Task ShowOffsetsWhenReady(
        Task<(TheTechIdea.OilGas.Wellbore.Data.AntiCollisionResult Result, IReadOnlyList<string> Warnings)?> pending)
    {
        var scan = await pending.ConfigureAwait(true);

        if (scan is null || Dispatcher is null)
        {
            return;
        }

        if (Dispatcher.CheckAccess())
        {
            ShowOffsets(scan.Value);
        }
        else
        {
            Dispatcher.Invoke(() => ShowOffsets(scan.Value));
        }
    }

    private void UpdateSummary()
    {
        if (_stationRows.Count == 0)
        {
            return;
        }

        _summary.Text =
            $"{_stationRows.Count} station(s) · MD {_stationRows[0].Md:0}-{_stationRows[^1].Md:0} m · " +
            $"TVD {_stationRows[^1].Tvd:0} m · section {SectionAzimuth.Azimuth:0.#}°" +
            (Minimum is { } min
                ? $" · closest offset {min.Offset} SF {min.Factor:0.##} at {min.Distance:0.#} m"
                : " · no offset separation");
    }

    /// <summary>Loads the survey, the paths and the collision result, then shows the chosen view.</summary>
    public bool Load()
    {
        try
        {
            CancelPendingOffsetComputation();

            _stationRows = OgWellPath.Stations();
            SectionAzimuth = OgWellPath.SectionAzimuth();

            PlanPoints = OgWellPath.PlanPath(_stationRows);
            SectionPoints = OgWellPath.SectionPath(_stationRows, SectionAzimuth.Azimuth);

            // AC-6: the scan is chunked per offset well; a synchronous load passes no token, so it always
            // completes, while a changing offset set goes through ChangeOffsetSet below.
            var (result, warnings) = OgWellPath.ComputeOffsets(_offsets, CancellationToken.None);
            _collision = result;
            Minimum = OgWellPath.Minimum(result);
            Uncertainty = OgWellPath.Uncertainty();

            _stations.ItemsSource = _stationRows;

            _summary.Text =
                $"{_stationRows.Count} station(s) · MD {_stationRows[0].Md:0}–{_stationRows[^1].Md:0} m · " +
                $"TVD {_stationRows[^1].Tvd:0} m · section {SectionAzimuth.Azimuth:0.#}°" +
                (Minimum is { } min
                    ? $" · closest offset {min.Offset} SF {min.Factor:0.##} at {min.Distance:0.#} m"
                    : " · no offset separation");

            _notice.Text = string.Join(
                "  ·  ",
                new[]
                {
                    SectionAzimuth.Note,
                    Uncertainty.Statement,
                    warnings.Count == 0 ? null : string.Join(" ", warnings),
                }.Where(part => !string.IsNullOrWhiteSpace(part)));

            ShowSelectedView();
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "drawing the well path",
                "the path is not shown",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The well path could not be drawn.", reference);
            return false;
        }
    }

    /// <summary>Switches the hosted surface to the selected view and redraws.</summary>
    public void ShowSelectedView()
    {
        var choice = _view.SelectedItem as string ?? "Plan";

        switch (choice)
        {
            case "Section":
                _pathSurface.Title = $"Section {SectionAzimuth.Azimuth:0.#}°";
                _pathSurface.Points = SectionPoints;

                // A section has no horizontal ellipse: the uncertainty is a band, not drawn here.
                _pathSurface.Ellipses = [];
                _host.Surface = _pathSurface;
                break;

            case "Ladder plot":
                _host.Surface = _collision is null
                    ? _pathSurface
                    : new TheTechIdea.OilGas.Wellbore.LadderPlotView { Result = _collision };
                break;

            case "Travelling cylinder":
                _host.Surface = _collision is null
                    ? _pathSurface
                    : new TheTechIdea.OilGas.Wellbore.TravellingCylinderView { Result = _collision };
                break;

            default:
                _pathSurface.Title = "Plan (north / east)";
                _pathSurface.Points = PlanPoints;
                _pathSurface.Ellipses = Uncertainty.Ellipses;
                _host.Surface = _pathSurface;
                break;
        }

        _host.Redraw();
    }

    /// <summary>Exports the station table as a small PDF so the numbers leave with the picture.</summary>
    public bool ExportPdf()
    {
        try
        {
            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"wellpath-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");

            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var document = OgDocumentMapper.TripleComboDemo();
            document.Name = "Well path stations";
            document.Markers.Clear();

            foreach (var station in _stationRows)
            {
                document.Markers.Add(new TheTechIdea.OilGas.Wellbore.DepthMarker
                {
                    Name = $"MD {station.Md:0} · inc {station.Inclination:0.#}° · TVD {station.Tvd:0}",
                    Depth = (float)station.Tvd,
                    Kind = TheTechIdea.OilGas.Wellbore.MarkerKind.Top,
                });
            }

            using var stream = File.Create(path);
            var pages = TheTechIdea.OilGas.Wellbore.WellboreExport.ToPdf(document, stream);

            _summary.Text = $"{pages} page(s) · {new FileInfo(path).Length / 1024.0:0.#} KB · {path}";
            return true;
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the well path",
                "no PDF was produced",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The well path could not be exported.", reference);
            return false;
        }
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
        Binding = new System.Windows.Data.Binding(path) { StringFormat = "{0:0.##}" },
        Width = new DataGridLength(width),
    };
}
