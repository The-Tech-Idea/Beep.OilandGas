using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-06: design a directional well plan - its tie-on, its target, the dogleg it is built at - and check the resulting path.
/// </summary>
/// <remarks>
/// The plan is the owner's minimum-curvature construction, so a build-and-hold design lands on its target (AC-1); the dogleg each
/// station shows is the minimum-curvature walk's own figure and is checked against the configured limit (AC-2); the station table,
/// the plan view and the section view are all built from one station list, so they agree (AC-3); every geometric column names its
/// unit (AC-4); the handoff passes the computed trajectory rather than a copy of the record (AC-5); and a saved plan records its
/// design method (AC-6).
/// </remarks>
public sealed class WellPlanPageView : OgPageBase
{
    public const string Route = "wellplan";

    private readonly OgApplication _application;
    private readonly IOgWellPlan _plans;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _target;
    private readonly TextBlock _dogleg;
    private readonly TextBlock _tortuosity;
    private readonly TextBlock _views;
    private readonly TextBlock _saved;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _field;
    private readonly TextBox _planName;
    private readonly TextBox _tieOnMd;
    private readonly TextBox _tieOnNorth;
    private readonly TextBox _tieOnEast;
    private readonly TextBox _tieOnTvd;
    private readonly TextBox _tieOnInclination;
    private readonly TextBox _tieOnAzimuth;
    private readonly TextBox _targetNorth;
    private readonly TextBox _targetEast;
    private readonly TextBox _targetTvd;
    private readonly TextBox _tolerance;
    private readonly TextBox _doglegDesign;
    private readonly TextBox _sections;
    private readonly ComboBox _method;
    private readonly ComboBox _unit;
    private readonly DataGrid _stationGrid;
    private readonly Button _designButton;
    private readonly Button _saveButton;
    private readonly Button _handoffButton;
    private readonly Button _loadButton;

    private OgWellPlanWorkspace? _workspace;
    private IReadOnlyList<OgPathPoint> _planView = [];
    private IReadOnlyList<OgPathPoint> _sectionView = [];

    public WellPlanPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _plans = application.WellPlan;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _target = Body(string.Empty);
        _target.TextWrapping = TextWrapping.Wrap;
        _target.Margin = new Thickness(0, 0, 0, 4);

        _dogleg = Body(string.Empty);
        _dogleg.TextWrapping = TextWrapping.Wrap;
        _dogleg.Margin = new Thickness(0, 0, 0, 4);

        _tortuosity = Body(string.Empty);
        _tortuosity.TextWrapping = TextWrapping.Wrap;
        _tortuosity.Margin = new Thickness(0, 0, 0, 4);

        _views = Body(string.Empty);
        _views.TextWrapping = TextWrapping.Wrap;
        _views.Margin = new Thickness(0, 0, 0, 4);

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
        _field = Field("Field", 110);
        _planName = Field("Plan name", 160);
        _tieOnMd = Field("Tie-on MD", 90);
        _tieOnNorth = Field("Tie-on N", 90);
        _tieOnEast = Field("Tie-on E", 90);
        _tieOnTvd = Field("Tie-on TVD", 90);
        _tieOnInclination = Field("Tie-on inclination", 120);
        _tieOnAzimuth = Field("Tie-on azimuth", 110);
        _targetNorth = Field("Target N", 90);
        _targetEast = Field("Target E", 90);
        _targetTvd = Field("Target TVD", 90);
        _tolerance = Field("Tolerance", 90);
        _doglegDesign = Field("Design dogleg", 110);

        _tieOnInclination.Text = "0";
        _tieOnAzimuth.Text = "0";
        _tolerance.Text = "30";
        _doglegDesign.Text = "3";

        _method = new ComboBox { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
        _method.ItemsSource = WellPlanRules.Methods.ToList();
        _method.SelectedIndex = 0;
        Named(_method, "Method");

        _unit = new ComboBox { MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
        _unit.ItemsSource = new List<string> { "m", "ft" };
        _unit.SelectedIndex = 0;
        Named(_unit, "Unit");

        _sections = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 60,
            MaxHeight = 110,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One section per line: 'hold <length>' or 'curve <dogleg>, <toolface>, <length>'",
        };

        _sections.Text = "hold 1000\ncurve 3, 0, 900\nhold 2000";
        Named(_sections, "Sections");

        _stationGrid = Grid("Stations", ("#", "Index", 40), ("MD", "Md", 90), ("Incl.", "Inclination", 80),
            ("Azim.", "Azimuth", 80), ("TVD", "Tvd", 90), ("N", "North", 90), ("E", "East", 90),
            ("Dogleg", "Dogleg", 90), ("Build", "Build", 80), ("Turn", "Turn", 80), ("Section", "Section", 90));

        _designButton = Button("Design", (_, _) => Design());
        _saveButton = Button("Save plan", (_, _) => _ = SaveAsync());
        _handoffButton = Button("Hand off to torque & drag", (_, _) => _ = HandoffAsync());
        _loadButton = Button("Load saved plan", (_, _) => Load());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Field"));
        wellBar.Children.Add(_field);
        wellBar.Children.Add(Caption("Plan"));
        wellBar.Children.Add(_planName);
        wellBar.Children.Add(_method);
        wellBar.Children.Add(_unit);
        wellBar.Children.Add(_loadButton);

        var tieOnBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        tieOnBar.Children.Add(Caption("Tie-on MD"));
        tieOnBar.Children.Add(_tieOnMd);
        tieOnBar.Children.Add(Caption("N"));
        tieOnBar.Children.Add(_tieOnNorth);
        tieOnBar.Children.Add(Caption("E"));
        tieOnBar.Children.Add(_tieOnEast);
        tieOnBar.Children.Add(Caption("TVD"));
        tieOnBar.Children.Add(_tieOnTvd);
        tieOnBar.Children.Add(Caption("Incl."));
        tieOnBar.Children.Add(_tieOnInclination);
        tieOnBar.Children.Add(Caption("Azim."));
        tieOnBar.Children.Add(_tieOnAzimuth);

        var targetBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        targetBar.Children.Add(Caption("Target N"));
        targetBar.Children.Add(_targetNorth);
        targetBar.Children.Add(Caption("E"));
        targetBar.Children.Add(_targetEast);
        targetBar.Children.Add(Caption("TVD"));
        targetBar.Children.Add(_targetTvd);
        targetBar.Children.Add(Caption("Tolerance"));
        targetBar.Children.Add(_tolerance);
        targetBar.Children.Add(Caption("Dogleg"));
        targetBar.Children.Add(_doglegDesign);
        targetBar.Children.Add(_designButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(tieOnBar);
        panel.Children.Add(targetBar);
        panel.Children.Add(Caption("Explicit sections (used by EXPLICIT)"));
        panel.Children.Add(_sections);
        panel.Children.Add(_summary);
        panel.Children.Add(_target);
        panel.Children.Add(_dogleg);
        panel.Children.Add(_tortuosity);
        panel.Children.Add(_views);
        panel.Children.Add(Caption("Stations"));
        panel.Children.Add(_stationGrid);
        panel.Children.Add(_saved);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_saveButton);
        panel.Children.Add(_handoffButton);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well and the target, then design the plan.");
    }

    public OgWellPlanWorkspace? Workspace => _workspace;

    public IReadOnlyList<OgPathPoint> PlanView => _planView;

    public IReadOnlyList<OgPathPoint> SectionView => _sectionView;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string TargetText => _target.Text;

    public string DoglegText => _dogleg.Text;

    public string TortuosityText => _tortuosity.Text;

    public string ViewsText => _views.Text;

    public string SavedText => _saved.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool Design()
    {
        _header.Text = "Engineering · well plan";

        var request = Request(out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var designed = _plans.DesignAsync(request).GetAwaiter().GetResult();

        if (!designed.Success || designed.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = designed.Message ?? "The plan could not be designed.";
            ShowState(OgViewState.Failed, "The plan is not designed.", designed.Reference);
            return false;
        }

        LastFailed = false;
        Apply(designed.Value);
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

        var saved = await _plans.SaveAsync(request, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The plan was refused.";
            ShowState(OgViewState.Failed, "The plan was refused.", saved.Reference);
            return false;
        }

        LastFailed = false;
        Apply(saved.Value);

        _application.AuditLog.Add("engineering.wellplan.write", "engineering", saved.Value.PlanId ?? "plan", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Plan saved.";

        return true;
    }

    public async Task<bool> HandoffAsync()
    {
        if (_workspace?.PlanId is not { } planId)
        {
            LastFailed = true;
            _failure.Text = "Save the plan before handing its trajectory on - the handoff passes the computed path of a saved plan.";
            return false;
        }

        var handed = await _plans.HandoffAsync(planId, _application.Session.UserId).ConfigureAwait(true);

        if (!handed.Success || handed.Value is null)
        {
            LastFailed = true;
            _failure.Text = handed.Message ?? "The handoff was refused.";
            ShowState(OgViewState.Failed, "The handoff was refused.", handed.Reference);
            return false;
        }

        LastFailed = false;
        Apply(handed.Value);

        _application.AuditLog.Add("engineering.wellplan.write", "engineering", planId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Trajectory handed to torque & drag: " +
                       (handed.Value.Trajectory?.Survey.Count ?? 0).ToString(CultureInfo.InvariantCulture) + " station(s).";

        return true;
    }

    public bool Load()
    {
        _header.Text = "Engineering · well plan";

        var loaded = _plans.LoadAsync(_well.Text?.Trim() ?? string.Empty).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The saved plan could not be read.";
            ShowState(OgViewState.Failed, "The saved plan is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    private void Apply(OgWellPlanWorkspace plan)
    {
        _workspace = plan;

        _well.Text = plan.WellId;
        _unit.Text = plan.Unit;

        _summary.Text = plan.SummaryText;
        _target.Text = plan.TargetText;
        _dogleg.Text = plan.DoglegText;
        _tortuosity.Text = plan.TortuosityText;
        _saved.Text = plan.SavedText;
        _warnings.Text = plan.WarningsText;
        _refreshed.Text = plan.ReadText;

        _stationGrid.ItemsSource = plan.Stations.ToList();

        // AC-4: every geometric column names its unit, taken from the plan rather than assumed.
        var course = TheTechIdea.OilGas.Wellbore.Data.Trajectory.DoglegCourseLabel(plan.Unit);

        Headers(("MD (" + plan.Unit + ")", 1), ("TVD (" + plan.Unit + ")", 4), ("N (" + plan.Unit + ")", 5),
            ("E (" + plan.Unit + ")", 6), ("Dogleg (" + course + ")", 7), ("Build (" + course + ")", 8),
            ("Turn (" + course + ")", 9));

        // AC-3: the two views are built from the same station list the grid shows, through the shared path maths.
        var stations = plan.Stations
            .Select(station => new OgSurveyStation(
                station.Md, station.Inclination, station.Azimuth, station.Tvd, station.North, station.East, station.Dogleg))
            .ToList();

        _planView = OgWellPath.PlanPath(stations);

        var trajectory = TheTechIdea.OilGas.Wellbore.Data.Trajectory.Compute(
            plan.Stations.Select(station => (station.Md, station.Inclination, station.Azimuth)),
            plan.Unit);

        var (sectionAzimuth, assumed, note) = OgWellPath.SectionAzimuth(trajectory);
        _sectionView = OgWellPath.SectionPath(stations, sectionAzimuth);

        _views.Text =
            "Plan view: " + _planView.Count.ToString(CultureInfo.InvariantCulture) + " point(s) · N " +
            _planView[0].Y.ToString("0.#", CultureInfo.InvariantCulture) + " → " +
            _planView[^1].Y.ToString("0.#", CultureInfo.InvariantCulture) + " · E " +
            _planView[0].X.ToString("0.#", CultureInfo.InvariantCulture) + " → " +
            _planView[^1].X.ToString("0.#", CultureInfo.InvariantCulture) + " " + plan.Unit +
            Environment.NewLine +
            "Vertical section at " + sectionAzimuth.ToString("0.#", CultureInfo.InvariantCulture) + "°" +
            (assumed ? " (assumed" + (note is null ? string.Empty : ": " + note) + ")" : string.Empty) + ": " +
            _sectionView.Count.ToString(CultureInfo.InvariantCulture) + " point(s) · TVD " +
            _sectionView[0].Y.ToString("0.#", CultureInfo.InvariantCulture) + " → " +
            _sectionView[^1].Y.ToString("0.#", CultureInfo.InvariantCulture) + " " + plan.Unit;

        _handoffButton.IsEnabled = plan.PlanId is not null;
        _failure.Text = string.Empty;

        ShowState(plan.Stations.Count == 0 ? OgViewState.Empty : OgViewState.Ready, plan.SummaryText);
    }

    private void Headers(params (string Text, int Column)[] headers)
    {
        foreach (var (text, column) in headers)
        {
            if (column < _stationGrid.Columns.Count)
            {
                _stationGrid.Columns[column].Header = text;
            }
        }
    }

    private OgPlanRequest Request(out string? problem)
    {
        problem = null;

        var unit = _unit.SelectedItem as string ?? "m";
        var course = TheTechIdea.OilGas.Wellbore.Data.Trajectory.DoglegCourse(unit);
        var sections = new List<OgPlanSection>();

        foreach (var line in (_sections.Text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var (section, refusal) = WellPlanRules.ParseSection(line, unit, course);

            if (refusal is not null)
            {
                problem = "Section '" + line + "' cannot be read: " + refusal + ".";
                return Empty();
            }

            sections.Add(section!);
        }

        return new OgPlanRequest(
            _well.Text?.Trim() ?? string.Empty,
            Empty(_field.Text),
            Empty(_planName.Text),
            _method.SelectedItem as string ?? WellPlanRules.BuildAndHold,
            _unit.SelectedItem as string ?? "m",
            Number(_tieOnMd.Text),
            Number(_tieOnNorth.Text),
            Number(_tieOnEast.Text),
            Number(_tieOnTvd.Text),
            Number(_tieOnInclination.Text),
            Number(_tieOnAzimuth.Text),
            Nullable(_targetNorth.Text),
            Nullable(_targetEast.Text),
            Nullable(_targetTvd.Text),
            Number(_tolerance.Text),
            Number(_doglegDesign.Text),
            sections);
    }

    private static OgPlanRequest Empty() =>
        new(string.Empty, null, null, WellPlanRules.BuildAndHold, "m", 0, 0, 0, 0, 0, 0, null, null, null, 0, 0, []);

    private void Clear()
    {
        _summary.Text = string.Empty;
        _target.Text = string.Empty;
        _dogleg.Text = string.Empty;
        _tortuosity.Text = string.Empty;
        _views.Text = string.Empty;
        _saved.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _stationGrid.ItemsSource = null;
        _planView = [];
        _sectionView = [];
        _handoffButton.IsEnabled = false;
        _notice.Text = string.Empty;
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
            MinHeight = 120,
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
