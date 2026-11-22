using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WFL-02: what a process definition does - its ordered steps, its versions, its dependencies and its cycles.
/// </summary>
/// <remarks>
/// The steps are ordered and each names its required role (AC-1); a cycle is shown, not hidden (AC-2); versions are shown with
/// the steps they added and removed (AC-3); the screen is read-only and says so (AC-4); the instance counts match WFL-01 (AC-5);
/// and the graph is derived from the step list itself (AC-6).
/// </remarks>
public sealed class DefinitionsPageView : OgPageBase
{
    public const string Route = "definitions";

    private readonly OgApplication _application;
    private readonly IOgDefinitionCatalogue _definitions;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _notice;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBlock _state;
    private readonly TextBlock _cycles;
    private readonly TextBlock _graph;
    private readonly TextBox _field;
    private readonly DataGrid _definitionGrid;
    private readonly DataGrid _stepGrid;
    private readonly DataGrid _versionGrid;
    private readonly DataGrid _dependencyGrid;
    private readonly Button _loadButton;

    private OgDefinitionCatalogue? _catalogue;

    public DefinitionsPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _definitions = application.DefinitionCatalogue;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;
        _notice.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _state = Body(string.Empty);
        _state.TextWrapping = TextWrapping.Wrap;
        _state.Margin = new Thickness(0, 4, 0, 4);

        _cycles = Body(string.Empty);
        _cycles.TextWrapping = TextWrapping.Wrap;
        _cycles.Margin = new Thickness(0, 0, 0, 4);

        _graph = Body(string.Empty);
        _graph.TextWrapping = TextWrapping.Wrap;
        _graph.Margin = new Thickness(0, 0, 0, 4);

        _field = Field("Field", 120);

        _definitionGrid = Grid("Definitions", ("Name", "Name", 190), ("Category", "Category", 120),
            ("Version", "Version", 80), ("Steps", "Steps", 60), ("Instances", "Instances", 80),
            ("State", "StateText", 150));

        _stepGrid = Grid("Steps", ("#", "Sequence", 40), ("Step", "Name", 180), ("Role", "RequiresRole", 140),
            ("Decisions", "Decisions", 210), ("Next", "NextStepKeys", 170), ("SLA", "SlaHours", 60));

        _versionGrid = Grid("Versions", ("Version", "Number", 80), ("Change", "Change", 220),
            ("Difference", "DifferenceText", 280), ("By", "CreatedBy", 100));

        _dependencyGrid = Grid("Dependencies", ("Dependent", "DependentDefinitionId", 160),
            ("Prerequisite", "PrerequisiteDefinitionId", 170), ("Type", "Type", 110),
            ("Condition", "Condition", 160));

        _definitionGrid.SelectionChanged += (_, _) => SelectDefinition();

        _loadButton = Button("Refresh", (_, _) => Load());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Field"));
        loadBar.Children.Add(_field);
        loadBar.Children.Add(_loadButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(_notice);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Definitions"));
        panel.Children.Add(_definitionGrid);
        panel.Children.Add(_state);
        panel.Children.Add(Caption("Steps, in order"));
        panel.Children.Add(_stepGrid);
        panel.Children.Add(Caption("Graph"));
        panel.Children.Add(_graph);
        panel.Children.Add(Caption("Cycles"));
        panel.Children.Add(_cycles);
        panel.Children.Add(Caption("Versions"));
        panel.Children.Add(_versionGrid);
        panel.Children.Add(Caption("Dependencies"));
        panel.Children.Add(_dependencyGrid);
        panel.Children.Add(_failure);

        ShowContent(panel);

        _field.Text = application.Session.FieldId ?? string.Empty;

        Load();
    }

    public OgDefinitionCatalogue? Catalogue => _catalogue;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string NoticeText => _notice.Text;

    public string FailureText => _failure.Text;

    public string StateText => _state.Text;

    public string CyclesText => _cycles.Text;

    public string GraphText => _graph.Text;

    public bool Load()
    {
        _header.Text = "Processes · definitions";

        var loaded = _definitions
            .LoadAsync(Empty(_field.Text), null)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _catalogue = null;
            _counts.Text = string.Empty;
            _state.Text = string.Empty;
            _cycles.Text = string.Empty;
            _graph.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _definitionGrid.ItemsSource = null;
            _stepGrid.ItemsSource = null;
            _versionGrid.ItemsSource = null;
            _dependencyGrid.ItemsSource = null;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The definition catalogue could not be read.";
            ShowState(OgViewState.Failed, "The definition catalogue is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    private void Apply(OgDefinitionCatalogue catalogue)
    {
        _catalogue = catalogue;

        _counts.Text = catalogue.CountsText;
        _refreshed.Text = catalogue.ReadText;

        // AC-4, and the feature's own open question, stated on the page.
        _notice.Text = OgDefinitionCatalogue.ReadOnlyNotice;

        _definitionGrid.ItemsSource = catalogue.Definitions.ToList();
        _definitionGrid.SelectedItem = catalogue.Selected;

        ShowSelected(catalogue.Selected);

        _versionGrid.ItemsSource = catalogue.Versions.ToList();
        _dependencyGrid.ItemsSource = catalogue.Dependencies.ToList();

        _failure.Text = string.Empty;

        ShowState(catalogue.Definitions.Count == 0 ? OgViewState.Empty : OgViewState.Ready, catalogue.CountsText);
    }

    private void SelectDefinition()
    {
        if (_definitionGrid.SelectedItem is not OgProcessDefinitionView selected)
        {
            return;
        }

        // The detail is reloaded by identity rather than resolved by position, so the versions belong to the row selected.
        var loaded = _definitions
            .LoadAsync(Empty(_field.Text), selected.Id)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            _failure.Text = loaded.Message ?? "The definition could not be read.";
            return;
        }

        _catalogue = loaded.Value;
        _counts.Text = loaded.Value.CountsText;

        ShowSelected(loaded.Value.Selected);

        _versionGrid.ItemsSource = loaded.Value.Versions.ToList();
        _dependencyGrid.ItemsSource = loaded.Value.Dependencies.ToList();
    }

    private void ShowSelected(OgProcessDefinitionView? selected)
    {
        if (selected is null)
        {
            _state.Text = "No definition is selected.";
            _cycles.Text = "No definition is selected, so no cycle is checked.";
            _graph.Text = "No definition is selected.";
            _stepGrid.ItemsSource = null;
            return;
        }

        _state.Text = selected.Describe();
        _stepGrid.ItemsSource = selected.Steps.ToList();
        _graph.Text = selected.GraphText;

        var cycles = _catalogue?.Cycles ?? [];
        var mine = cycles
            .Where(cycle => cycle.Path.Any(key =>
                selected.Steps.Any(step => string.Equals(step.Key, key, StringComparison.Ordinal))))
            .ToList();

        _cycles.Text = mine.Count == 0
            ? "No cycle is recorded in this definition's steps."
            : string.Join(Environment.NewLine, mine.Select(cycle => cycle.Describe()));
    }

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

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
