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
/// ENG-08: the circulating system - pressures, annular velocity and ECD per section, and the rate that reaches a target ECD.
/// </summary>
/// <remarks>
/// The reference case is the owner's arithmetic on the plan's trajectory (AC-1); velocity and loss are per section (AC-2); an ECD
/// above the recorded fracture gradient is critical, with the depth (AC-3); nozzles are shown in 32nds (AC-4); the optimiser's
/// recommendation moves the ECD monotonically in its search range (AC-5); and a saved run records the mud model and the rate
/// (AC-6). No threshold is drawn where none is recorded - the feature's own open question.
/// </remarks>
public sealed class HydraulicsPageView : OgPageBase
{
    public const string Route = "hydraulics";

    private readonly OgApplication _application;
    private readonly IOgHydraulics _hydraulics;
    private readonly OgEcdProfileChart _chart = new();
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _mud;
    private readonly TextBlock _nozzles;
    private readonly TextBlock _counts;
    private readonly TextBlock _critical;
    private readonly TextBlock _fracture;
    private readonly TextBlock _profile;
    private readonly TextBlock _recommendation;
    private readonly TextBlock _saved;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _density;
    private readonly TextBox _theta600;
    private readonly TextBox _theta300;
    private readonly TextBox _flowRate;
    private readonly TextBox _surfaceLoss;
    private readonly TextBox _nozzleBox;
    private readonly TextBox _sections;
    private readonly TextBox _targetEcd;
    private readonly ComboBox _rateUnit;
    private readonly DataGrid _sectionGrid;
    private readonly Button _runButton;
    private readonly Button _saveButton;
    private readonly Button _lastButton;
    private readonly Button _optimiseButton;

    private OgHydraulicsWorkspace? _workspace;

    public HydraulicsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _hydraulics = application.Hydraulics;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 250,
        };

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _mud = Body(string.Empty);
        _mud.TextWrapping = TextWrapping.Wrap;
        _mud.Margin = new Thickness(0, 0, 0, 4);

        _nozzles = Body(string.Empty);
        _nozzles.TextWrapping = TextWrapping.Wrap;
        _nozzles.Margin = new Thickness(0, 0, 0, 4);

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 0, 0, 4);

        _critical = Body(string.Empty);
        _critical.TextWrapping = TextWrapping.Wrap;
        _critical.FontWeight = FontWeights.SemiBold;
        _critical.Margin = new Thickness(0, 4, 0, 4);

        _fracture = Body(string.Empty);
        _fracture.TextWrapping = TextWrapping.Wrap;
        _fracture.Margin = new Thickness(0, 0, 0, 4);

        _profile = Body(string.Empty);
        _profile.TextWrapping = TextWrapping.Wrap;
        _profile.Margin = new Thickness(0, 0, 0, 4);

        _recommendation = Body(string.Empty);
        _recommendation.TextWrapping = TextWrapping.Wrap;
        _recommendation.Margin = new Thickness(0, 4, 0, 4);

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
        _density = Field("Mud ppg", 90);
        _theta600 = Field("600 rpm", 90);
        _theta300 = Field("300 rpm", 90);
        _flowRate = Field("Flow rate", 90);
        _surfaceLoss = Field("Surface loss psi", 110);
        _nozzleBox = Field("Nozzles /32", 150);
        _targetEcd = Field("Target ECD ppg", 110);

        _density.Text = "10";
        _theta600.Text = "40";
        _theta300.Text = "25";
        _flowRate.Text = "600";
        _surfaceLoss.Text = "100";
        _nozzleBox.Text = "12, 12, 12";
        _targetEcd.Text = "10.5";

        _rateUnit = new ComboBox { MinWidth = 80, Margin = new Thickness(0, 0, 8, 0) };
        _rateUnit.ItemsSource = new List<string> { "gpm", "L/min" };
        _rateUnit.SelectedIndex = 0;
        Named(_rateUnit, "Rate unit");

        _sections = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 70,
            MaxHeight = 120,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One flow path per line, lengths in the plan's unit: name, top, bottom, pipe ID or hole ID, pipe OD for an annulus",
        };

        _sections.Text = "drill pipe, 0, 2000, 4.276\nannulus, 0, 1000, 8.681, 5\nannulus, 1000, 2000, 8.5, 5";
        Named(_sections, "Sections");

        _sectionGrid = Grid("Sections", ("#", "Index", 40), ("Section", "Name", 110), ("Flow path", "Interval", 90),
            ("Top", "TopMd", 80), ("Bottom", "BottomMd", 80), ("Velocity ft/s", "VelocityFtS", 100),
            ("Re", "Reynolds", 80), ("Regime", "Turbulent", 70), ("Loss psi", "LossPsi", 90),
            ("ECD ppg", "EcdPpg", 90), ("State", "Computed", 70));

        _runButton = Button("Run", (_, _) => _ = RunAsync());
        _saveButton = Button("Save run", (_, _) => _ = SaveAsync());
        _lastButton = Button("Load last run", (_, _) => LoadLast());
        _optimiseButton = Button("Solve rate for the target ECD", (_, _) => _ = OptimiseAsync());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(_runButton);
        wellBar.Children.Add(_saveButton);
        wellBar.Children.Add(_lastButton);

        var mudBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        mudBar.Children.Add(Caption("Mud"));
        mudBar.Children.Add(_density);
        mudBar.Children.Add(Caption("600 rpm"));
        mudBar.Children.Add(_theta600);
        mudBar.Children.Add(Caption("300 rpm"));
        mudBar.Children.Add(_theta300);

        var rateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        rateBar.Children.Add(Caption("Rate"));
        rateBar.Children.Add(_flowRate);
        rateBar.Children.Add(_rateUnit);
        rateBar.Children.Add(Caption("Surface loss"));
        rateBar.Children.Add(_surfaceLoss);
        rateBar.Children.Add(Caption("Nozzles /32"));
        rateBar.Children.Add(_nozzleBox);

        var optimiseBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        optimiseBar.Children.Add(Caption("Target ECD"));
        optimiseBar.Children.Add(_targetEcd);
        optimiseBar.Children.Add(_optimiseButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(mudBar);
        panel.Children.Add(rateBar);
        panel.Children.Add(Caption("Flow paths, from surface to the bit"));
        panel.Children.Add(_sections);
        panel.Children.Add(_summary);
        panel.Children.Add(_mud);
        panel.Children.Add(_nozzles);
        panel.Children.Add(_counts);
        panel.Children.Add(_critical);
        panel.Children.Add(_fracture);
        panel.Children.Add(Caption("ECD against depth"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Sections"));
        panel.Children.Add(_sectionGrid);
        panel.Children.Add(Caption("ECD profile"));
        panel.Children.Add(_profile);
        panel.Children.Add(optimiseBar);
        panel.Children.Add(_recommendation);
        panel.Children.Add(_saved);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well whose plan the run follows, then run the circulating system.");
    }

    public OgHydraulicsWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string MudText => _mud.Text;

    public string NozzlesText => _nozzles.Text;

    public string CountsText => _counts.Text;

    public string CriticalText => _critical.Text;

    public string FractureText => _fracture.Text;

    public string ProfileText => _profile.Text;

    public string RecommendationText => _recommendation.Text;

    public string SavedText => _saved.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public int ChartPoints => _chart.Profile.Count;

    public bool FractureDrawn => _chart.Fracture.Count > 0;

    public async Task<bool> RunAsync()
    {
        _header.Text = "Engineering · hydraulics";

        var request = Request(out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var run = await _hydraulics.RunAsync(request).ConfigureAwait(true);

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

        var saved = await _hydraulics.SaveAsync(request, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The run was refused.";
            ShowState(OgViewState.Failed, "The run was refused.", saved.Reference);
            return false;
        }

        LastFailed = false;
        Apply(saved.Value);

        _application.AuditLog.Add("engineering.hydraulics.write", "engineering", saved.Value.WellId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Run saved with its mud model and rate.";

        return true;
    }

    public bool LoadLast()
    {
        _header.Text = "Engineering · hydraulics";

        var loaded = _hydraulics.LoadLastAsync(_well.Text?.Trim() ?? string.Empty).GetAwaiter().GetResult();

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

    public async Task<bool> OptimiseAsync()
    {
        _header.Text = "Engineering · hydraulics";

        var request = Request(out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var target = Number(_targetEcd.Text);

        if (target <= 0d)
        {
            LastFailed = true;
            _failure.Text = "The optimiser needs a target ECD above zero; none was recorded.";
            return false;
        }

        var solved = await _hydraulics.OptimiseAsync(request, target).ConfigureAwait(true);

        if (!solved.Success || solved.Value is null)
        {
            LastFailed = true;
            _recommendation.Text = solved.Message ?? "The optimiser was refused.";
            _failure.Text = solved.Message ?? "The optimiser was refused.";
            return false;
        }

        LastFailed = false;
        _recommendation.Text = solved.Value.Describe() + Environment.NewLine + string.Join("; ", solved.Value.Trace);
        _failure.Text = string.Empty;
        _notice.Text = solved.Value.Feasible
            ? "Rate solved for the target ECD."
            : "No rate reaches the target ECD, and the reason is stated.";

        return true;
    }

    private void Apply(OgHydraulicsWorkspace workspace)
    {
        _workspace = workspace;

        _well.Text = workspace.WellId;

        _summary.Text = workspace.SummaryText;
        _mud.Text = workspace.MudText;
        _nozzles.Text = workspace.NozzlesText;
        _counts.Text = workspace.CountsText;
        _critical.Text = workspace.CriticalText;
        _fracture.Text = workspace.FractureText;
        _profile.Text = workspace.ProfileText;
        _saved.Text = workspace.SavedText;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _sectionGrid.ItemsSource = workspace.Results.ToList();

        _chart.Profile.Clear();
        _chart.Profile.AddRange(workspace.Profile.Select(point => (point.Depth, point.Value)));
        _chart.Fracture.Clear();
        _chart.Fracture.AddRange(workspace.FracturePoints.Select(point => (point.Depth, point.Value)));
        _chart.StaticPpg = workspace.MudDensityPpg;
        _chart.DepthUnit = workspace.Unit;
        _chart.Caption = workspace.CountsText + " · " + workspace.SummaryText;
        _host.Surface = _chart;
        _host.Redraw();

        _failure.Text = string.Empty;

        ShowState(workspace.Results.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _mud.Text = string.Empty;
        _nozzles.Text = string.Empty;
        _counts.Text = string.Empty;
        _critical.Text = string.Empty;
        _fracture.Text = string.Empty;
        _profile.Text = string.Empty;
        _recommendation.Text = string.Empty;
        _saved.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _sectionGrid.ItemsSource = null;
        _chart.Profile.Clear();
        _chart.Fracture.Clear();
        _chart.Caption = string.Empty;
        _host.Surface = _chart;
        _host.Redraw();
        _notice.Text = string.Empty;
    }

    private OgHydraulicsRequest Request(out string? problem)
    {
        problem = null;
        var sections = Sections(_sections.Text, out problem);

        if (problem is not null)
        {
            return Empty();
        }

        var nozzles = Nozzles(_nozzleBox.Text, out problem);

        if (problem is not null)
        {
            return Empty();
        }

        return new OgHydraulicsRequest(
            _well.Text?.Trim() ?? string.Empty,
            Number(_density.Text),
            Number(_theta600.Text),
            Number(_theta300.Text),
            Number(_flowRate.Text),
            _rateUnit.SelectedItem as string ?? "gpm",
            Number(_surfaceLoss.Text),
            nozzles,
            sections);
    }

    /// <summary>
    /// AC-4: nozzles are entered in 32nds and every one is checked against the range 1-32, because "12" means 12/32 in and nothing
    /// else. An empty box is the feature's "no nozzle" state, which the run accepts and flags.
    /// </summary>
    public static IReadOnlyList<double> Nozzles(string? text, out string? problem)
    {
        problem = null;
        var nozzles = new List<double>();

        foreach (var part in (text ?? string.Empty).Split([',', ' ', ';', '+'],
                     StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            // '12/32' and '12' both mean twelve thirty-seconds; the suffix is cut at the slash rather than trimmed as characters,
            // so a size that ends in 2 or 3 (22, 23) is not eaten.
            var slash = part.IndexOf('/');
            var cleaned = slash > 0 ? part[..slash] : part;

            if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out var size))
            {
                problem = "'" + part + "' is not a nozzle size; nozzles are written in 32nds, like 12 or 12/32.";
                return [];
            }

            if (size <= 0d || size > 32d)
            {
                problem = "A nozzle of " + size.ToString("0.##", CultureInfo.InvariantCulture) +
                          "/32 is outside the range: nozzles run from 1 to 32 thirty-seconds of an inch.";
                return [];
            }

            nozzles.Add(size);
        }

        return nozzles;
    }

    /// <summary>
    /// The flow paths, one per line: <c>name, top, bottom, outer, inner</c>. <c>outer</c> is the pipe ID for a pipe section and the
    /// hole or casing ID for an annulus, and <c>inner</c> is the pipe OD of an annulus, omitted for flow inside pipe. Lengths are in
    /// the plan's own unit, which the run names in its refusals.
    /// </summary>
    public static IReadOnlyList<OgFlowSection> Sections(string? text, out string? problem)
    {
        problem = null;
        var sections = new List<OgFlowSection>();
        var number = 0;

        foreach (var line in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            number++;

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 4)
            {
                problem = "Section '" + line + "' needs at least a name, a top, a bottom and a diameter.";
                return [];
            }

            if (!double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var top) ||
                !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var bottom) ||
                !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var outer))
            {
                problem = "Section '" + line + "' has a value that is not a number.";
                return [];
            }

            double? inner = null;

            if (parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]))
            {
                if (!double.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var od))
                {
                    problem = "Section '" + line + "' has a pipe outside diameter that is not a number.";
                    return [];
                }

                inner = od;
            }

            sections.Add(new OgFlowSection(
                string.IsNullOrWhiteSpace(parts[0]) ? "section " + number.ToString(CultureInfo.InvariantCulture) : parts[0],
                top,
                bottom,
                outer,
                inner));
        }

        if (sections.Count == 0)
        {
            problem = "The run needs the flow sections of the circulating system; none is recorded.";
        }

        return sections;
    }

    private static OgHydraulicsRequest Empty() =>
        new(string.Empty, 0, 0, 0, 0, "gpm", 0, [], []);

    private static double Number(string? text) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 230,
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
