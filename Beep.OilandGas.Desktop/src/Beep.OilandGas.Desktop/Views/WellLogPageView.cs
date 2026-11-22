using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// VSB-04: the well log screen. A triple combo drawn through <see cref="SkiaViewSurfaceHost"/>, the curve
/// list, a lithology standard, an undoable depth shift and splice, and the two crossplots the tools produce.
/// </summary>
/// <remarks>
/// The log starts from the tools' demo documents, because PPDM module setup is blocked (G-08); the model path
/// is the same one a database read will take, so switching the source is one call. The page says which source
/// it is showing rather than passing a demo off as a well.
/// </remarks>
public sealed class WellLogPageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly WellLogView _view = new();
    private readonly SkiaViewSurfaceHost _host;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _curves;

    private readonly OgLogEditHistory _history = new();
    private WellModel _model = OgLogDemo.BuildWell();
    private string? _standard;
    private bool _fromModel;
    private bool _withImageAndCement;

    public WellLogPageView(OgApplication application)
        : base(application, "logs")
    {
        _application = application;

        // VSB-02: the environment must be loaded before a log surface is constructed.
        _ = application.Wellbore;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            MinHeight = 300,
        };

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _curves = new DataGrid
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
                Text("Mnemonic", nameof(OgLogCurveRow.Mnemonic), 90),
                Text("Unit", nameof(OgLogCurveRow.UnitLabel), 110),
                Text("Description", nameof(OgLogCurveRow.Description), 170),
                Text("Scale", nameof(OgLogCurveRow.Scale), 80),
                Text("Samples", nameof(OgLogCurveRow.Samples), 70),
            },
        };

        var panel = new DockPanel();
        var toolbar = BuildToolbar();
        DockPanel.SetDock(toolbar, Dock.Top);
        panel.Children.Add(toolbar);
        DockPanel.SetDock(_notice, Dock.Top);
        panel.Children.Add(_notice);
        DockPanel.SetDock(_summary, Dock.Top);
        panel.Children.Add(_summary);
        DockPanel.SetDock(_curves, Dock.Bottom);
        panel.Children.Add(_curves);
        panel.Children.Add(_host);

        Named(_curves, "Curves");
        ShowContent(panel);
        Loaded += (_, _) => Load();
    }

    /// <summary>The curves currently listed; the smoke run and tests read this.</summary>
    public IReadOnlyList<OgLogCurveRow> CurveRows { get; private set; } = [];

    /// <summary>The summary line, for tests and the smoke run.</summary>
    public string SummaryText => _summary.Text;

    public IReadOnlyList<OgLogEdit> History => _history.Entries;

    private FrameworkElement BuildToolbar()
    {
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };

        var source = new ComboBox
        {
            ItemsSource = new[] { "Triple-combo demo", "Demo model log", "Demo with image & cement" },
            SelectedIndex = 0,
            MinWidth = 160,
            Margin = new Thickness(0, 0, 10, 0),
        };
        source.SelectionChanged += (_, _) =>
        {
            _fromModel = source.SelectedIndex == 1;
            _withImageAndCement = source.SelectedIndex == 2;
            Load();
        };
        bar.Children.Add(source);

        var standards = new ComboBox
        {
            ItemsSource = OgLogStandards.Available.ToList(),
            SelectedIndex = 0,
            MinWidth = 130,
            Margin = new Thickness(0, 0, 10, 0),
        };
        standards.SelectionChanged += (_, _) =>
        {
            var requested = standards.SelectedItem as string;
            var (standard, notice) = OgLogStandards.Resolve(requested);

            // RPT-03/VSB-04 AC-2: the standard is a lithology layer, so setting it changes the fills on the
            // next draw without reloading the data.
            _standard = standard;
            OgLogStandards.Apply(standard, Path.Combine(OgPaths.DataRoot, "Standards"));
            _notice.Text = notice ?? string.Empty;
            Load();
        };
        Named(source, "Log source");
        Named(standards, "Lithology standard");
        bar.Children.Add(standards);

        var shift = new Button { Content = "Shift +2 m", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
        shift.Click += (_, _) => ShiftFirstCurve();
        bar.Children.Add(shift);

        var splice = new Button { Content = "Splice first two curves", Padding = new Thickness(10, 4, 10, 4), Margin = new Thickness(0, 0, 8, 0) };
        splice.Click += (_, _) => SpliceFirstTwo();
        bar.Children.Add(splice);

        var undo = new Button { Content = "Undo", Padding = new Thickness(10, 4, 10, 4) };
        undo.Click += (_, _) =>
        {
            _history.Undo();
            Refresh();
        };
        bar.Children.Add(undo);

        return bar;
    }

    /// <summary>Loads the chosen source and redraws.</summary>
    public bool Load()
    {
        try
        {
            var document = _withImageAndCement
                ? OgLogDemo.WithImageAndCement()
                : _fromModel
                    ? WellLogDocuments.FromModel(_model)
                    : WellLogTemplates.CreateTripleComboDemo();

            _view.Document = document;
            _host.Surface = _view;

            CurveRows = ReadCurves(document);
            _curves.ItemsSource = CurveRows;

            Refresh();
            return _host.Redraw();
        }
        catch (Exception ex)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "loading the well log",
                "the log was not drawn",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The well log could not be drawn.", reference);
            return false;
        }
    }

    private void Refresh()
    {
        var document = _view.Document;

        if (document is null)
        {
            return;
        }

        var (panel, warnings) = OgLogCrossplots.DensityNeutron(CurvesOfLogSet());

        _summary.Text =
            $"{document.Name} · {document.Tracks.Count} tracks · {CurveRows.Count} curves · " +
            $"{document.Axis.Top:0.#}–{document.Axis.Bottom:0.#} {DepthUnits.Symbol(document.Axis.Unit)}   |   " +
            $"standard: {OgLogStandards.Active}   |   " +
            $"density-neutron points: {panel.Points.Count}   |   " +
            $"{OgLogAvailability.Image(document)} {OgLogAvailability.Cement(document)}   |   " +
            $"edits: {_history.Entries.Count}";

        if (warnings.Count > 0 && _notice.Text.Length == 0)
        {
            _notice.Text = string.Join("  ·  ", warnings);
        }
    }

    /// <summary>VSB-04 AC-3: shifts the first curve and records the edit.</summary>
    public bool ShiftFirstCurve()
    {
        if (_view.Document is not { } document || CurveRows.Count == 0)
        {
            return false;
        }

        var curve = FirstCurve(document);
        if (curve is null)
        {
            return false;
        }

        var result = OgLogEditing.Shift(
            curve,
            2f,
            DepthUnits.Symbol(document.Axis.Unit),
            document.Axis.Top,
            document.Axis.Bottom,
            _history);

        if (!result.Success)
        {
            _notice.Text = result.Message ?? "The shift was refused.";
            return false;
        }

        _notice.Text = result.Value!.Detail;
        _host.Redraw();
        Refresh();
        return true;
    }

    /// <summary>VSB-04 AC-4: splices the first two curves and records both intervals.</summary>
    public bool SpliceFirstTwo()
    {
        if (_view.Document is not { } document)
        {
            return false;
        }

        var tracks = document.Tracks.OfType<WellLogTrack>().ToList();
        var pairs = tracks.Where(track => track.Curves.Count > 0).SelectMany(track => track.Curves).Take(2).ToList();

        if (pairs.Count < 2)
        {
            _notice.Text = "Two curves are needed to splice.";
            return false;
        }

        var spliceDepth = (float)((document.Axis.Top + document.Axis.Bottom) / 2);
        var result = OgLogEditing.Splice(pairs, 0, 1, spliceDepth, "SPLICE", _history);

        if (!result.Success)
        {
            _notice.Text = result.Message ?? "The splice was refused.";
            return false;
        }

        _notice.Text = result.Value!.Detail;
        _host.Redraw();
        Refresh();
        return true;
    }

    private static IReadOnlyList<OgLogCurveRow> ReadCurves(WellboreDocument document) =>
        document.Tracks
            .OfType<WellLogTrack>()
            .SelectMany(track => track.Curves)
            .Select(curve => new OgLogCurveRow(
                curve.Mnemonic,
                curve.Unit,
                curve.Name,
                curve.MinimumValue,
                curve.MaximumValue,
                curve.ScaleType.ToString(),
                curve.Samples.Count,
                !string.IsNullOrWhiteSpace(curve.Unit)))
            .ToList();

    private static WellLogCurve? FirstCurve(WellboreDocument document) =>
        document.Tracks.OfType<WellLogTrack>().SelectMany(track => track.Curves).FirstOrDefault();

    private LogSet CurvesOfLogSet() =>
        _model.LogSets.Count > 0
            ? _model.LogSets[0]
            : new LogSet { Name = "empty", Index = [], Curves = [] };

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
