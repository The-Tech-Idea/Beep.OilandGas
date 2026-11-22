using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-02: design and check casing to API 5C3 - burst, collapse and tension per section with the design factors that come from
/// configuration, and the governing case named.
/// </summary>
/// <remarks>
/// Utilisation is computed per section and per case (AC-2); the governing case is named (AC-3); the factors are the configured
/// ones, printed rather than edited (AC-4); an unknown grade and a missing wall are refused with their reason (AC-5); and a run
/// records the grades, walls, factors and every verdict (AC-6).
/// </remarks>
public sealed class CasingDesignPageView : OgPageBase
{
    public const string Route = "casingdesign";

    private readonly OgApplication _application;
    private readonly IOgCasingDesign _design;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _factors;
    private readonly TextBlock _governing;
    private readonly TextBlock _warnings;
    private readonly TextBlock _run;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _designId;
    private readonly TextBox _gradient;
    private readonly TextBox _testPressure;
    private readonly TextBox _overpull;
    private readonly TextBox _sectionName;
    private readonly TextBox _od;
    private readonly TextBox _wall;
    private readonly TextBox _weight;
    private readonly TextBox _grade;
    private readonly TextBox _top;
    private readonly TextBox _bottom;
    private readonly TextBox _unit;
    private readonly DataGrid _sectionGrid;
    private readonly DataGrid _verdictGrid;
    private readonly Button _loadButton;
    private readonly Button _checkButton;
    private readonly Button _addButton;

    private OgCasingDesignWorkspace? _workspace;

    public CasingDesignPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _design = application.CasingDesign;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _factors = Body(string.Empty);
        _factors.TextWrapping = TextWrapping.Wrap;
        _factors.Margin = new Thickness(0, 0, 0, 4);

        _governing = Body(string.Empty);
        _governing.TextWrapping = TextWrapping.Wrap;
        _governing.FontWeight = FontWeights.SemiBold;
        _governing.Margin = new Thickness(0, 4, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _run = Body(string.Empty);
        _run.TextWrapping = TextWrapping.Wrap;
        _run.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _designId = Field("Design id", 190);
        _gradient = Field("Mud density ppg", 110);
        _testPressure = Field("Test pressure psi", 120);
        _overpull = Field("Overpull lbf", 110);

        _sectionName = Field("Section", 100);
        _od = Field("OD in", 80);
        _wall = Field("Wall in", 80);
        _weight = Field("Weight lb/ft", 100);
        _grade = Field("Grade", 80);
        _top = Field("Top", 80);
        _bottom = Field("Shoe", 80);
        _unit = Field("Unit", 60);

        _gradient.Text = "10";
        _testPressure.Text = "5000";
        _overpull.Text = "0";
        _unit.Text = "ft";

        _sectionGrid = Grid("Sections", ("#", "Order", 40), ("Section", "Name", 120), ("OD", "Od", 80),
            ("Wall", "Wall", 80), ("Weight", "Weight", 90), ("Grade", "Grade", 80), ("Top", "Top", 80),
            ("Shoe", "Bottom", 80), ("Unit", "Unit", 60));

        _verdictGrid = Grid("Verdicts", ("Section", "Section", 120), ("Case", "LoadCase", 100),
            ("Load", "Load", 100), ("Resistance", "Resistance", 110), ("Utilisation", "Utilisation", 100),
            ("Required", "RequiredFactor", 90), ("Unit", "Unit", 60), ("Verdict", "VerdictText", 90));

        _loadButton = Button("Load design", (_, _) => Load());
        _checkButton = Button("Run check", (_, _) => _ = CheckAsync());
        _addButton = Button("Record section", (_, _) => _ = AddSectionAsync());

        var designBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        designBar.Children.Add(Caption("Design"));
        designBar.Children.Add(_designId);
        designBar.Children.Add(_loadButton);

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Mud"));
        loadBar.Children.Add(_gradient);
        loadBar.Children.Add(Caption("Test"));
        loadBar.Children.Add(_testPressure);
        loadBar.Children.Add(Caption("Overpull"));
        loadBar.Children.Add(_overpull);
        loadBar.Children.Add(_checkButton);

        var sectionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sectionBar.Children.Add(_sectionName);
        sectionBar.Children.Add(_od);
        sectionBar.Children.Add(_wall);
        sectionBar.Children.Add(_weight);
        sectionBar.Children.Add(_grade);
        sectionBar.Children.Add(_top);
        sectionBar.Children.Add(_bottom);
        sectionBar.Children.Add(_unit);
        sectionBar.Children.Add(_addButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(designBar);
        panel.Children.Add(_counts);
        panel.Children.Add(Caption("Design factors"));
        panel.Children.Add(_factors);
        panel.Children.Add(Caption("Sections"));
        panel.Children.Add(_sectionGrid);
        panel.Children.Add(Caption("Load case"));
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Verdicts"));
        panel.Children.Add(_verdictGrid);
        panel.Children.Add(_governing);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_run);
        panel.Children.Add(Caption("Record a section"));
        panel.Children.Add(sectionBar);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the design to check, then run the check.");
    }

    public OgCasingDesignWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string FactorsText => _factors.Text;

    public string GoverningText => _governing.Text;

    public string WarningsText => _warnings.Text;

    public string RunText => _run.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool Load()
    {
        _header.Text = "Engineering · casing design";

        var loaded = _design
            .LoadAsync(_designId.Text?.Trim() ?? string.Empty)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The casing design could not be read.";
            ShowState(OgViewState.Failed, "The casing design is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> CheckAsync()
    {
        var outcome = await _design.CheckAsync(
            _designId.Text?.Trim() ?? string.Empty,
            Number(_gradient.Text),
            Number(_testPressure.Text),
            Number(_overpull.Text),
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The casing check was refused.";
            ShowState(OgViewState.Failed, "The casing check was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("engineering.casing.write", "engineering", outcome.Value.DesignId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = outcome.Value.Passed
            ? "Check recorded: every section meets its design factor."
            : "Check recorded: " + outcome.Value.Failed.ToString(CultureInfo.InvariantCulture) +
              " verdict(s) fall outside their design factor.";

        return true;
    }

    public async Task<bool> AddSectionAsync()
    {
        var outcome = await _design.AddSectionAsync(
            _designId.Text?.Trim() ?? string.Empty,
            _sectionName.Text ?? string.Empty,
            Number(_od.Text),
            Number(_wall.Text),
            Number(_weight.Text),
            _grade.Text?.Trim() ?? string.Empty,
            Number(_top.Text),
            Number(_bottom.Text),
            string.IsNullOrWhiteSpace(_unit.Text) ? "ft" : _unit.Text.Trim(),
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The section was refused.";
            ShowState(OgViewState.Failed, "The section was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("engineering.casing.write", "engineering", outcome.Value.DesignId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Section recorded with its wall, so it can be rated.";

        return true;
    }

    private void Apply(OgCasingDesignWorkspace workspace)
    {
        _workspace = workspace;

        if (string.IsNullOrWhiteSpace(_designId.Text))
        {
            _designId.Text = workspace.DesignId;
        }

        _counts.Text = (workspace.WellName ?? workspace.DesignId) + " · " + workspace.CountsText;
        _factors.Text = workspace.Factors.Describe();
        _governing.Text = workspace.GoverningText;
        _warnings.Text = workspace.WarningsText;
        _run.Text = workspace.RunText;
        _refreshed.Text = workspace.ReadText;

        _sectionGrid.ItemsSource = workspace.Sections.ToList();
        _verdictGrid.ItemsSource = workspace.Verdicts.ToList();

        _failure.Text = string.Empty;

        ShowState(workspace.Sections.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _counts.Text = string.Empty;
        _factors.Text = string.Empty;
        _governing.Text = string.Empty;
        _warnings.Text = string.Empty;
        _run.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _sectionGrid.ItemsSource = null;
        _verdictGrid.ItemsSource = null;
        _notice.Text = string.Empty;
    }

    private static decimal Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 180,
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
