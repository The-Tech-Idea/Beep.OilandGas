using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// INT-08: a field in three dimensions - the surface from INT-07's gridding, the well paths, the picks, orbit and click-to-pick - and
/// its export to glTF.
/// </summary>
/// <remarks>
/// The scene, the camera, the drawing and the exporter are the tools', so the picture on screen and the file written are the same
/// scene (AC-1). Picking projects every node the view draws through the same camera and answers with the nearest inside a tolerance,
/// so a click on nothing reports nothing (AC-2). The exaggeration is applied to the view and written into the export with its own
/// extras and scale (AC-3), and the GLB is the spec's container (AC-4). The thinning the view applies is stated with the numbers that
/// produced it (AC-5).
/// </remarks>
public sealed class ScenePageView : OgPageBase
{
    public const string Route = "scene3d";

    private readonly OgApplication _application;
    private readonly IOgScene _scenes;
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _decimation;
    private readonly TextBlock _lighting;
    private readonly TextBlock _exaggeration;
    private readonly TextBlock _pick;
    private readonly TextBlock _excluded;
    private readonly TextBlock _notes;
    private readonly TextBlock _export;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _top;
    private readonly TextBox _maxWells;
    private readonly TextBox _exportPath;
    private readonly ComboBox _projectionBox;
    private readonly ComboBox _method;
    private readonly ComboBox _format;
    private readonly Slider _vertical;
    private readonly DataGrid _wellGrid;
    private readonly Button _buildButton;
    private readonly Button _fitButton;
    private readonly Button _projectionsButton;
    private readonly Button _exportButton;

    private OgSceneWorkspace? _workspace;
    private IReadOnlyList<OgMapProjection> _projections = [];

    public ScenePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _scenes = application.Scenes;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 420,
        };

        // AC-2: a click on the scene is a pick, not a resize; the host reports the surface coordinates the camera projected into.
        _host.Clicked += (_, pointer) => _ = PickAsync(pointer.SurfacePoint);

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _decimation = Body(string.Empty);
        _decimation.TextWrapping = TextWrapping.Wrap;
        _decimation.Margin = new Thickness(0, 0, 0, 4);

        _lighting = Body(string.Empty);
        _lighting.TextWrapping = TextWrapping.Wrap;
        _lighting.Margin = new Thickness(0, 0, 0, 4);

        _exaggeration = Body(string.Empty);
        _exaggeration.TextWrapping = TextWrapping.Wrap;
        _exaggeration.Margin = new Thickness(0, 0, 0, 4);

        _pick = Body(string.Empty);
        _pick.TextWrapping = TextWrapping.Wrap;
        _pick.FontWeight = FontWeights.SemiBold;
        _pick.Margin = new Thickness(0, 4, 0, 4);

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
        _maxWells = Field("At most wells", 90);
        _maxWells.Text = "200";

        _exportPath = Field("Export path", 300);

        _projectionBox = new ComboBox { MinWidth = 200, Margin = new Thickness(0, 0, 8, 0) };
        Named(_projectionBox, "Coordinate system");

        _method = new ComboBox { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
        _method.ItemsSource = OgMapRules.Methods.ToList();
        _method.SelectedIndex = 0;
        Named(_method, "Gridding method");

        _format = new ComboBox { MinWidth = 90, Margin = new Thickness(0, 0, 8, 0) };
        _format.ItemsSource = OgSceneRules.Formats.ToList();
        _format.SelectedIndex = 0;
        Named(_format, "Export format");

        // AC-3: the slider's value is shown beside it, so the picture's scale is never a guess.
        _vertical = new Slider
        {
            Minimum = OgSceneRules.MinExaggeration,
            Maximum = OgSceneRules.MaxExaggeration,
            Value = 1,
            Width = 220,
            TickFrequency = 0.5,
            IsSnapToTickEnabled = false,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
            ToolTip = "Vertical exaggeration: 1 is true scale",
        };

        _vertical.ValueChanged += (_, _) => VerticalChanged();

        Named(_vertical, "Vertical exaggeration");

        _wellGrid = Grid("Scene wells", ("Well", "Well", 140), ("Placed", "Included", 70),
            ("Easting", "X", 100), ("Northing", "Y", 100), ("Value", "Value", 90), ("Reason", "Reason", 300));

        _buildButton = Button("Build scene", (_, _) => _ = BuildAsync());
        _fitButton = Button("Fit to scene", (_, _) => Fit());
        _projectionsButton = Button("List projections", (_, _) => Projections());
        _exportButton = Button("Export", (_, _) => _ = ExportAsync());

        var valueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        valueBar.Children.Add(Caption("Field"));
        valueBar.Children.Add(_field);
        valueBar.Children.Add(Caption("Top"));
        valueBar.Children.Add(_top);
        valueBar.Children.Add(Caption("At most"));
        valueBar.Children.Add(_maxWells);

        var gridBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        gridBar.Children.Add(Caption("Projection"));
        gridBar.Children.Add(_projectionBox);
        gridBar.Children.Add(_projectionsButton);
        gridBar.Children.Add(Caption("Method"));
        gridBar.Children.Add(_method);
        gridBar.Children.Add(_buildButton);

        var viewBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        viewBar.Children.Add(Caption("Vertical exaggeration"));
        viewBar.Children.Add(_vertical);
        viewBar.Children.Add(_exaggeration);
        viewBar.Children.Add(_fitButton);

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
        panel.Children.Add(viewBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("The scene - drag to orbit, wheel to zoom, click to pick a value"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Picked cell"));
        panel.Children.Add(_pick);
        panel.Children.Add(Caption("Decimation"));
        panel.Children.Add(_decimation);
        panel.Children.Add(Caption("Lighting"));
        panel.Children.Add(_lighting);
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

        ShowState(OgViewState.Empty, "Name a formation top and build the field in three dimensions.");
    }

    public OgSceneWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string DecimationText => _decimation.Text;

    public string LightingText => _lighting.Text;

    public string ExaggerationText => _exaggeration.Text;

    public string PickText => _pick.Text;

    public string ExcludedText => _excluded.Text;

    public string NotesText => _notes.Text;

    public string ExportText => _export.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public double VerticalExaggeration => _vertical.Value;

    public bool SceneDrawn => _host.Surface is not null && _workspace?.View is not null;

    /// <summary>Lists the coordinate systems with their measured round-trip error, and selects the first usable one.</summary>
    public bool Projections()
    {
        _header.Text = "Data · 3D view";

        var listed = _scenes.ProjectionsAsync([]).GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            LastFailed = true;
            _failure.Text = listed.Message ?? "The projections could not be read.";
            return false;
        }

        LastFailed = false;
        _projections = listed.Value;
        _projectionBox.ItemsSource = _projections.Select(projection => projection.Name).ToList();
        _projectionBox.SelectedItem =
            (_projections.FirstOrDefault(projection => projection.WithinTolerance) ?? _projections.FirstOrDefault())?.Name;

        _failure.Text = string.Empty;
        _notice.Text = _projections.Count.ToString(CultureInfo.InvariantCulture) + " coordinate system(s) available.";

        return true;
    }

    public async Task<bool> BuildAsync()
    {
        _header.Text = "Data · 3D view";

        if (_projections.Count == 0)
        {
            Projections();
        }

        var request = new OgSceneRequest(
            string.IsNullOrWhiteSpace(_field.Text) ? null : _field.Text.Trim(),
            _top.Text?.Trim() ?? string.Empty,
            _projectionBox.SelectedItem as string ?? string.Empty,
            _method.SelectedItem as string ?? OgMapRules.Methods[0],
            _vertical.Value,
            (int)Number(_maxWells.Text));

        var built = await _scenes.BuildAsync(request).ConfigureAwait(true);

        if (!built.Success || built.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = built.Message ?? "The scene could not be built.";
            ShowState(OgViewState.Failed, "The scene is not shown.", built.Reference);
            return false;
        }

        LastFailed = false;
        Apply(built.Value);

        _failure.Text = string.Empty;
        _notice.Text = built.Value.SceneBuilt
            ? "Scene built from the wells in scope."
            : "No scene: the reason is on the page, and the wells it did and did not place are below.";

        return built.Value.SceneBuilt;
    }

    public async Task<bool> PickAsync(SKPoint point)
    {
        if (_workspace is null)
        {
            return false;
        }

        var picked = await _scenes.PickAsync(_workspace, point.X, point.Y).ConfigureAwait(true);

        if (!picked.Success || picked.Value is null)
        {
            // The feature's own state: no readout rather than a wrong one.
            LastFailed = false;
            _pick.Text = picked.Message ?? "No value at that point.";
            return false;
        }

        LastFailed = false;
        _pick.Text = picked.Value.Describe();

        return true;
    }

    public bool Fit()
    {
        if (_workspace?.View is null)
        {
            return false;
        }

        _workspace.View.FitToScene();
        _host.Redraw();

        return true;
    }

    public async Task<bool> ExportAsync()
    {
        if (_workspace is null)
        {
            LastFailed = true;
            _failure.Text = "There is no scene to export: build one first.";
            return false;
        }

        var format = _format.SelectedItem as string ?? OgSceneRules.Formats[0];
        var path = string.IsNullOrWhiteSpace(_exportPath.Text)
            ? Path.Combine(OgPaths.ExportsDirectory, "scene-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + "." + format)
            : _exportPath.Text!.Trim();

        var exported = await _scenes.ExportAsync(_workspace, format, path).ConfigureAwait(true);

        if (!exported.Success || exported.Value is null)
        {
            LastFailed = true;
            _export.Text = exported.Message ?? "The scene was not written.";
            _failure.Text = exported.Message ?? "The scene was not written.";
            ShowState(OgViewState.Failed, "The scene was not exported.", exported.Reference);
            return false;
        }

        LastFailed = false;
        _exportPath.Text = exported.Value.Path;
        _export.Text = exported.Value.Message + Environment.NewLine + exported.Value.Summary +
                       Environment.NewLine + exported.Value.WarningsText;
        _failure.Text = string.Empty;

        _application.AuditLog.Add("interop.scene.export", "interop", exported.Value.Path, "ok",
            _application.Session.UserId);

        _notice.Text = "Scene written to " + exported.Value.Path + ".";

        return true;
    }

    /// <summary>AC-3: the slider's value is printed as it changes, and applied to the view when there is one.</summary>
    private void VerticalChanged()
    {
        _exaggeration.Text = "×" + _vertical.Value.ToString("0.##", CultureInfo.InvariantCulture) +
                             (_vertical.Value <= 1 ? " (true scale)" : string.Empty);

        if (_workspace?.View is not { } view)
        {
            return;
        }

        view.VerticalExaggeration = _vertical.Value;
        view.FitToScene();

        _workspace = _workspace with { VerticalExaggeration = _vertical.Value };
        _summary.Text = _workspace.SummaryText;

        _host.Redraw();
    }

    private void Apply(OgSceneWorkspace workspace)
    {
        _workspace = workspace;

        _vertical.Value = Math.Clamp(workspace.VerticalExaggeration, OgSceneRules.MinExaggeration, OgSceneRules.MaxExaggeration);

        _summary.Text = workspace.SummaryText;
        _decimation.Text = workspace.DecimationText;
        _lighting.Text = workspace.LightingText;
        _exaggeration.Text = "×" + workspace.VerticalExaggeration.ToString("0.##", CultureInfo.InvariantCulture) +
                             (workspace.VerticalExaggeration <= 1 ? " (true scale)" : string.Empty);
        _excluded.Text = workspace.ExcludedText;
        _notes.Text = workspace.NotesText;
        _refreshed.Text = workspace.ReadText;
        _pick.Text = workspace.SceneBuilt ? "Click the surface to read a value." : string.Empty;

        _wellGrid.ItemsSource = workspace.Wells.ToList();

        _host.Surface = workspace.View;
        _host.Redraw();

        ShowState(workspace.SceneBuilt ? OgViewState.Ready : OgViewState.Empty, workspace.SummaryText);
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _decimation.Text = string.Empty;
        _lighting.Text = string.Empty;
        _pick.Text = string.Empty;
        _excluded.Text = string.Empty;
        _notes.Text = string.Empty;
        _export.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _wellGrid.ItemsSource = null;

        // A failed build takes the last scene off the screen, so a picture never stands for data it was not built from.
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
            MaxHeight = 200,
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
