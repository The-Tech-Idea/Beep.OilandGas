using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// INT-07: a field's values on a map - structure, isochore or net pay - gridded, contoured, and exported in a projected coordinate
/// system.
/// </summary>
/// <remarks>
/// The geometry is the tools': a pick is placed where the well really is, the surface comes from <c>Gridding.Run</c>, the levels from
/// <c>Contouring</c>, and the drawing and the three exports from <c>ContourMapView</c> and <c>MapExport</c> (AC-1). The projection is
/// chosen from the ones the environment defines, each with its measured round-trip error, and a system that cannot return a point it
/// was given is called out (AC-2). The quality check is the gridding result's own, and a well it could not place is listed with its
/// reason rather than dropped silently (AC-3, AC-4). The contour interval is the engine's unless one is set (AC-5). The exports are
/// written by the owner and reported with their size (AC-6).
/// </remarks>
public sealed class MapPageView : OgPageBase
{
    public const string Route = "map";

    private readonly OgApplication _application;
    private readonly IOgMap _map;
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _projection;
    private readonly TextBlock _qc;
    private readonly TextBlock _excluded;
    private readonly TextBlock _notes;
    private readonly TextBlock _export;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _top;
    private readonly TextBox _lowerTop;
    private readonly TextBox _interval;
    private readonly TextBox _maxWells;
    private readonly TextBox _exportPath;
    private readonly ComboBox _projectionBox;
    private readonly ComboBox _method;
    private readonly ComboBox _format;
    private readonly DataGrid _wellGrid;
    private readonly DataGrid _projectionGrid;
    private readonly Button _buildButton;
    private readonly Button _projectionsButton;
    private readonly Button _exportButton;

    private OgMapWorkspace? _workspace;
    private IReadOnlyList<OgMapProjection> _projections = [];

    public MapPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _map = application.Map;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 380,
        };

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _projection = Body(string.Empty);
        _projection.TextWrapping = TextWrapping.Wrap;
        _projection.Margin = new Thickness(0, 0, 0, 4);

        _qc = Body(string.Empty);
        _qc.TextWrapping = TextWrapping.Wrap;
        _qc.Margin = new Thickness(0, 0, 0, 4);

        _excluded = Body(string.Empty);
        _excluded.TextWrapping = TextWrapping.Wrap;
        _excluded.Margin = new Thickness(0, 0, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 0, 0, 4);

        _export = Body(string.Empty);
        _export.TextWrapping = TextWrapping.Wrap;
        _export.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 120);
        _top = Field("Formation top", 150);
        _lowerTop = Field("Lower top (isochore)", 150);
        _interval = Field("Contour interval", 110);
        _maxWells = Field("At most wells", 90);
        _maxWells.Text = "200";

        _exportPath = Field("Export path", 300);

        _projectionBox = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_projectionBox, "Coordinate system");

        _method = new ComboBox { MinWidth = 160, Margin = new Thickness(0, 0, 8, 0) };
        _method.ItemsSource = OgMapRules.Methods.ToList();
        _method.SelectedIndex = 0;
        Named(_method, "Gridding method");

        _format = new ComboBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0) };
        _format.ItemsSource = OgMapRules.Formats.ToList();
        _format.SelectedIndex = 0;
        Named(_format, "Export format");

        _wellGrid = Grid("Map wells", ("Well", "Well", 150), ("Placed", "Included", 70),
            ("Easting", "X", 100), ("Northing", "Y", 100), ("Value", "Value", 90),
            ("MD", "Md", 80), ("TVD", "Tvd", 80), ("Reason", "Reason", 300));

        _projectionGrid = Grid("Projections", ("System", "Name", 200), ("Method", "Method", 150),
            ("Axes", "Axes", 70), ("Round trip m", "RoundTripMetres", 110), ("Usable", "WithinTolerance", 70));

        _buildButton = Button("Build map", (_, _) => _ = BuildAsync());
        _projectionsButton = Button("List projections", (_, _) => Projections());
        _exportButton = Button("Export", (_, _) => _ = ExportAsync());

        var valueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        valueBar.Children.Add(Caption("Field"));
        valueBar.Children.Add(_field);
        valueBar.Children.Add(Caption("Top"));
        valueBar.Children.Add(_top);
        valueBar.Children.Add(Caption("Lower"));
        valueBar.Children.Add(_lowerTop);
        valueBar.Children.Add(Caption("At most"));
        valueBar.Children.Add(_maxWells);

        var gridBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        gridBar.Children.Add(Caption("Projection"));
        gridBar.Children.Add(_projectionBox);
        gridBar.Children.Add(_projectionsButton);
        gridBar.Children.Add(Caption("Method"));
        gridBar.Children.Add(_method);
        gridBar.Children.Add(Caption("Interval"));
        gridBar.Children.Add(_interval);
        gridBar.Children.Add(_buildButton);

        var exportBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        exportBar.Children.Add(Caption("Format"));
        exportBar.Children.Add(_format);
        exportBar.Children.Add(Caption("To"));
        exportBar.Children.Add(_exportPath);
        exportBar.Children.Add(_exportButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(valueBar);
        panel.Children.Add(gridBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("The map"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Projection"));
        panel.Children.Add(_projection);
        panel.Children.Add(Caption("Projections this environment defines"));
        panel.Children.Add(_projectionGrid);
        panel.Children.Add(Caption("Grid quality"));
        panel.Children.Add(_qc);
        panel.Children.Add(Caption("Wells"));
        panel.Children.Add(_wellGrid);
        panel.Children.Add(Caption("Left out"));
        panel.Children.Add(_excluded);
        panel.Children.Add(Caption("Notes"));
        panel.Children.Add(_notes);
        panel.Children.Add(exportBar);
        panel.Children.Add(_export);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name a formation top and build the map of its structure.");
    }

    public OgMapWorkspace? Workspace => _workspace;

    public IReadOnlyList<OgMapProjection> Projections_ => _projections;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string ProjectionText => _projection.Text;

    public string QcText => _qc.Text;

    public string ExcludedText => _excluded.Text;

    public string NotesText => _notes.Text;

    public string ExportText => _export.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public int ChartPoints => _workspace?.View?.Wells.Count ?? 0;

    public bool MapDrawn => _host.Surface is not null && _workspace?.View is not null;

    /// <summary>Lists the coordinate systems with their measured round-trip error, and selects the first usable one.</summary>
    public bool Projections()
    {
        _header.Text = "Data · map";

        var listed = _map.ProjectionsAsync([]).GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            LastFailed = true;
            _failure.Text = listed.Message ?? "The projections could not be read.";
            return false;
        }

        LastFailed = false;
        _projections = listed.Value;
        _projectionGrid.ItemsSource = _projections.ToList();

        var chosen = _projectionBox.SelectedItem as string;

        _projectionBox.ItemsSource = _projections.Select(projection => projection.Name).ToList();

        if (chosen is not null && _projections.Any(projection => projection.Name == chosen))
        {
            _projectionBox.SelectedItem = chosen;
        }
        else
        {
            _projectionBox.SelectedItem =
                (_projections.FirstOrDefault(projection => projection.WithinTolerance) ?? _projections.FirstOrDefault())?.Name;
        }

        _failure.Text = string.Empty;
        _notice.Text = _projections.Count.ToString(CultureInfo.InvariantCulture) + " coordinate system(s); " +
                       _projections.Count(projection => projection.WithinTolerance).ToString(CultureInfo.InvariantCulture) +
                       " return a point to within " + OgMapRules.Figure(OgMapRules.RoundTripToleranceMetres) + " m.";

        return true;
    }

    public async Task<bool> BuildAsync()
    {
        _header.Text = "Data · map";

        if (string.IsNullOrWhiteSpace(_projectionBox.SelectedItem as string) && _projections.Count == 0)
        {
            Projections();
        }

        var request = new OgMapRequest(
            string.IsNullOrWhiteSpace(_field.Text) ? null : _field.Text.Trim(),
            _top.Text?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(_lowerTop.Text) ? null : _lowerTop.Text.Trim(),
            _projectionBox.SelectedItem as string ?? string.Empty,
            _method.SelectedItem as string ?? OgMapRules.Methods[0],
            Number(_interval.Text) > 0d ? Number(_interval.Text) : null,
            (int)Number(_maxWells.Text));

        var built = await _map.BuildAsync(request).ConfigureAwait(true);

        if (!built.Success || built.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = built.Message ?? "The map could not be built.";
            ShowState(OgViewState.Failed, "The map is not shown.", built.Reference);
            return false;
        }

        LastFailed = false;
        Apply(built.Value);

        _failure.Text = string.Empty;
        _notice.Text = built.Value.MapPossible
            ? "Map built from the wells in scope."
            : "No surface: the reason is on the page, and the wells it did and did not place are below.";

        return built.Value.MapPossible;
    }

    public async Task<bool> ExportAsync()
    {
        if (_workspace is null)
        {
            LastFailed = true;
            _failure.Text = "There is no map to export: build one first.";
            return false;
        }

        var format = _format.SelectedItem as string ?? OgMapRules.Formats[0];
        var path = string.IsNullOrWhiteSpace(_exportPath.Text)
            ? Path.Combine(OgPaths.ExportsDirectory, "map-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "." + format)
            : _exportPath.Text!.Trim();

        var exported = await _map.ExportAsync(_workspace, format, path).ConfigureAwait(true);

        if (!exported.Success || exported.Value is null)
        {
            LastFailed = true;
            _export.Text = exported.Message ?? "The map was not written.";
            _failure.Text = exported.Message ?? "The map was not written.";
            ShowState(OgViewState.Failed, "The map was not exported.", exported.Reference);
            return false;
        }

        LastFailed = false;
        _exportPath.Text = exported.Value.Path;
        _export.Text = exported.Value.Message + Environment.NewLine + exported.Value.Summary;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("interop.map.export", "interop", exported.Value.Path, "ok", _application.Session.UserId);

        _notice.Text = "Map written to " + exported.Value.Path + ".";

        return true;
    }

    /// <summary>
    /// AC-5's interval as the page reads it: a positive number is an interval the person set, and anything else leaves the engine to
    /// choose one.
    /// </summary>
    public static double? Interval(string? text) => Number(text) > 0d ? Number(text) : null;

    private void Apply(OgMapWorkspace workspace)
    {
        _workspace = workspace;

        _summary.Text = workspace.SummaryText;
        _qc.Text = workspace.QcText;
        _excluded.Text = workspace.ExcludedText;
        _notes.Text = workspace.NotesText;
        _refreshed.Text = workspace.ReadText;

        _projection.Text = workspace.Projection + " · axes " + workspace.ProjectionAxes + " · " +
                           workspace.Included.ToString(CultureInfo.InvariantCulture) + " well(s) placed over " +
                           OgMapRules.Figure(workspace.East) + " x " + OgMapRules.Figure(workspace.North) + " m" +
                           (workspace.MapPossible ? string.Empty : " · no surface was drawn");

        _wellGrid.ItemsSource = workspace.Wells.ToList();

        _host.Surface = workspace.View;

        if (workspace.View is not null)
        {
            workspace.View.Width = 900;
            workspace.View.Height = 620;
        }

        _host.Redraw();

        ShowState(workspace.MapPossible ? OgViewState.Ready : OgViewState.Empty, workspace.SummaryText);
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _projection.Text = string.Empty;
        _qc.Text = string.Empty;
        _excluded.Text = string.Empty;
        _notes.Text = string.Empty;
        _export.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _wellGrid.ItemsSource = null;

        // A failed build takes the last surface off the screen, so a map is never left standing for data it was not built from.
        _host.Surface = null;
        _host.Redraw();

        _notice.Text = string.Empty;
    }

    private static double Number(string? text) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 210,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

    private static TextBox Field(string name, double width)
    {
        var box = new TextBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = name,
        };

        Named(box, name);
        return box;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
    };

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
}
