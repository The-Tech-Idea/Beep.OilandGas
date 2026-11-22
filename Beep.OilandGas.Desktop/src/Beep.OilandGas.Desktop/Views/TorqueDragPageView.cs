using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-07: predict the hook load, the surface torque and the limiting component along the planned well, with the friction factors
/// the run used in sight.
/// </summary>
/// <remarks>
/// The run is the owner's soft-string model on the trajectory the well plan hands off (AC-1); the limiting factor is named with
/// its component (AC-2); the cased and open-hole factors are per interval and printed on every station (AC-3); the stations are
/// ordered by measured depth and cover the string (AC-4); a component written in another unit is refused (AC-5); and a saved run
/// records the scenario and the friction factors (AC-6).
/// </remarks>
public sealed class TorqueDragPageView : OgPageBase
{
    public const string Route = "torquedrag";

    private readonly OgApplication _application;
    private readonly IOgTorqueDrag _torqueDrag;
    private readonly OgTorqueDragChart _chart = new();
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _limit;
    private readonly TextBlock _friction;
    private readonly TextBlock _counts;
    private readonly TextBlock _saved;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _casedFriction;
    private readonly TextBox _openHoleFriction;
    private readonly TextBox _shoe;
    private readonly TextBox _mud;
    private readonly TextBox _element;
    private readonly TextBox _wob;
    private readonly TextBox _bitTension;
    private readonly TextBox _bitTorque;
    private readonly TextBox _string;
    private readonly ComboBox _scenario;
    private readonly DataGrid _rowGrid;
    private readonly Button _runButton;
    private readonly Button _saveButton;
    private readonly Button _lastButton;

    private OgTorqueDragWorkspace? _workspace;

    public TorqueDragPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _torqueDrag = application.TorqueDrag;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 260,
        };

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _limit = Body(string.Empty);
        _limit.TextWrapping = TextWrapping.Wrap;
        _limit.FontWeight = FontWeights.SemiBold;
        _limit.Margin = new Thickness(0, 0, 0, 4);

        _friction = Body(string.Empty);
        _friction.TextWrapping = TextWrapping.Wrap;
        _friction.Margin = new Thickness(0, 0, 0, 4);

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 0, 0, 4);

        _saved = Body(string.Empty);
        _saved.TextWrapping = TextWrapping.Wrap;
        _saved.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _well = Field("Well UWI", 120);
        _casedFriction = Field("Cased friction", 100);
        _openHoleFriction = Field("Open-hole friction", 110);
        _shoe = Field("Shoe MD", 90);
        _mud = Field("Mud ppg", 80);
        _element = Field("Element m", 90);
        _wob = Field("WOB kN", 90);
        _bitTension = Field("Bit tension kN", 100);
        _bitTorque = Field("Bit torque kN.m", 110);

        _casedFriction.Text = "0.2";
        _openHoleFriction.Text = "0.3";
        _element.Text = "10";
        _mud.Text = "10";

        _scenario = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        _scenario.ItemsSource = TorqueDragRules.Scenarios.ToList();
        _scenario.SelectedIndex = 0;
        Named(_scenario, "Scenario");

        _string = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 70,
            MaxHeight = 120,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One component per line, from the bit up: name, OD in, ID in, weight lb/ft, length, grade",
        };

        _string.Text = "drill pipe, 5, 4.276, 19.5, 2000, S135";
        Named(_string, "String");

        _rowGrid = Grid("Stations", ("#", "Index", 40), ("MD", "Md", 80), ("Incl.", "Inclination", 70),
            ("Azim.", "Azimuth", 70), ("Load kN", "LoadKn", 90), ("Torque kN.m", "TorqueKnm", 110),
            ("Drag kN", "DragKn", 90), ("Side kN", "SideForceKn", 90), ("Limit kN", "LimitKn", 90),
            ("Interval", "Interval", 90), ("Component", "Component", 110), ("Over", "OverLimit", 60));

        _runButton = Button("Run", (_, _) => _ = RunAsync());
        _saveButton = Button("Save run", (_, _) => _ = SaveAsync());
        _lastButton = Button("Load last run", (_, _) => LoadLast());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Scenario"));
        wellBar.Children.Add(_scenario);
        wellBar.Children.Add(_runButton);
        wellBar.Children.Add(_saveButton);
        wellBar.Children.Add(_lastButton);

        var frictionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        frictionBar.Children.Add(Caption("Cased"));
        frictionBar.Children.Add(_casedFriction);
        frictionBar.Children.Add(Caption("Open hole"));
        frictionBar.Children.Add(_openHoleFriction);
        frictionBar.Children.Add(Caption("Shoe"));
        frictionBar.Children.Add(_shoe);
        frictionBar.Children.Add(Caption("Mud"));
        frictionBar.Children.Add(_mud);
        frictionBar.Children.Add(Caption("Element"));
        frictionBar.Children.Add(_element);

        var operationBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        operationBar.Children.Add(Caption("WOB"));
        operationBar.Children.Add(_wob);
        operationBar.Children.Add(Caption("Bit tension"));
        operationBar.Children.Add(_bitTension);
        operationBar.Children.Add(Caption("Bit torque"));
        operationBar.Children.Add(_bitTorque);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(frictionBar);
        panel.Children.Add(operationBar);
        panel.Children.Add(Caption("String, from the bit up"));
        panel.Children.Add(_string);
        panel.Children.Add(_summary);
        panel.Children.Add(_limit);
        panel.Children.Add(_friction);
        panel.Children.Add(_counts);
        panel.Children.Add(Caption("Load against depth and the string limit"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Stations"));
        panel.Children.Add(_rowGrid);
        panel.Children.Add(_saved);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well whose plan the run follows, then run it.");
    }

    public OgTorqueDragWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string LimitText => _limit.Text;

    public string FrictionText => _friction.Text;

    public string CountsText => _counts.Text;

    public string SavedText => _saved.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public int ChartPoints => _chart.Points.Count;

    public async Task<bool> RunAsync()
    {
        _header.Text = "Engineering · torque & drag";

        var request = Request(out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var run = await _torqueDrag.RunAsync(request).ConfigureAwait(true);

        if (!run.Success || run.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = run.Message ?? "The run was refused.";
            ShowState(OgViewState.Failed, "The run is not shown.", run.Reference);
            return false;
        }

        LastFailed = false;
        Apply(run.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        var request = Request(out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var saved = await _torqueDrag.SaveAsync(request, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The run was refused.";
            ShowState(OgViewState.Failed, "The run was refused.", saved.Reference);
            return false;
        }

        LastFailed = false;
        Apply(saved.Value);

        _application.AuditLog.Add("engineering.torquedrag.write", "engineering", saved.Value.WellId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Run saved.";

        return true;
    }

    public bool LoadLast()
    {
        _header.Text = "Engineering · torque & drag";

        var loaded = _torqueDrag.LoadLastAsync(_well.Text?.Trim() ?? string.Empty).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The saved run could not be read.";
            ShowState(OgViewState.Failed, "The saved run is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    private void Apply(OgTorqueDragWorkspace workspace)
    {
        _workspace = workspace;

        _well.Text = workspace.WellId;
        _summary.Text = workspace.SummaryText;
        _limit.Text = workspace.LimitText;
        _friction.Text = workspace.FrictionText;
        _counts.Text = workspace.CountsText;
        _saved.Text = workspace.SavedText;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _rowGrid.ItemsSource = workspace.Rows.ToList();

        _chart.Points.Clear();
        _chart.Points.AddRange(workspace.Chart.Select(point => (point.Md, point.LoadKn, point.LimitKn)));
        _chart.DepthUnit = workspace.Unit;
        _chart.LoadUnit = "kN";
        _chart.Caption = workspace.Scenario + " · " + workspace.SummaryText;
        _host.Surface = _chart;
        _host.Redraw();

        _failure.Text = string.Empty;

        ShowState(workspace.Rows.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _limit.Text = string.Empty;
        _friction.Text = string.Empty;
        _counts.Text = string.Empty;
        _saved.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _rowGrid.ItemsSource = null;
        _chart.Points.Clear();
        _chart.Caption = string.Empty;
        _host.Surface = _chart;
        _host.Redraw();
        _notice.Text = string.Empty;
    }

    private OgTorqueDragRequest Request(out string? problem)
    {
        problem = null;

        var unit = _workspace?.Unit ?? "m";
        var components = Components(_string.Text, unit, out problem);

        if (problem is not null)
        {
            return new OgTorqueDragRequest(string.Empty, string.Empty, 0, 0, null, null, 0, null, 0, 0, []);
        }

        return new OgTorqueDragRequest(
            _well.Text?.Trim() ?? string.Empty,
            _scenario.SelectedItem as string ?? TorqueDragRules.PickUp,
            Number(_casedFriction.Text),
            Number(_openHoleFriction.Text),
            Nullable(_shoe.Text),
            Nullable(_mud.Text),
            Number(_element.Text) is var element && element > 0 ? element : 10,
            Nullable(_wob.Text),
            Number(_bitTension.Text),
            Number(_bitTorque.Text),
            components);
    }

    /// <summary>
    /// Parses the string, from the bit up: 'name, OD in, ID in, weight lb/ft, length, grade' per line. A length written in a unit
    /// other than the trajectory's is refused (AC-5); the diameters and the weight carry their own oilfield units.
    /// </summary>
    public static IReadOnlyList<OgDrillStringComponent> Components(string? text, string unit, out string? problem)
    {
        problem = null;

        var components = new List<OgDrillStringComponent>();
        var number = 0;

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            number++;

            var parts = line.Split(',', StringSplitOptions.TrimEntries);
            var name = parts.Length > 0 && parts[0].Length > 0
                ? parts[0]
                : "component " + number.ToString(CultureInfo.InvariantCulture);

            var (od, odProblem) = WithUnit(parts, 1, "in", null);
            var (id, idProblem) = WithUnit(parts, 2, "in", null);
            var (weight, weightProblem) = WithUnit(parts, 3, "lbm/ft", null);
            var (length, lengthProblem) = WithUnit(parts, 4, unit, unit);
            var grade = parts.Length > 5 ? parts[5] : null;

            problem = odProblem ?? idProblem ?? weightProblem ?? lengthProblem;

            if (problem is not null)
            {
                return [];
            }

            if (od is null || id is null || length is null)
            {
                problem = "Component '" + line + "' needs an outside diameter, an inside diameter and a length.";
                return [];
            }

            components.Add(new OgDrillStringComponent(name, od.Value, id.Value, weight ?? 0d, length.Value, grade));
        }

        if (components.Count == 0)
        {
            problem = "The run needs a drill string, listed from the bit up; none is recorded.";
        }

        return components;
    }

    /// <summary>
    /// Reads one value and its optional unit suffix. A length written in another unit than the trajectory is refused, which is
    /// AC-5's mixed-unit case; the diameters and the weight always carry their own oilfield units.
    /// </summary>
    private static (double? Value, string? Problem) WithUnit(string[] parts, int index, string fallback, string? mustMatch)
    {
        if (parts.Length <= index || string.IsNullOrWhiteSpace(parts[index]))
        {
            return (null, null);
        }

        var text = parts[index];
        var split = text.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var number = split[0];
        var suffix = split.Length > 1 ? split[1] : fallback;

        if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return (null, "'" + text + "' is not a number");
        }

        if (mustMatch is not null)
        {
            var (_, problem) = TorqueDragRules.CheckUnit(suffix, mustMatch);

            if (problem is not null)
            {
                return (null, problem);
            }
        }

        return (value, null);
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static double Number(string? text) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d;

    private static double? Nullable(string? text) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 240,
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
