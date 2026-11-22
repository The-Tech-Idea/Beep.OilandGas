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
/// WFL-03: one document's passage through its process - what happened, when, by whom and what is expected next.
/// </summary>
/// <remarks>
/// The instance is opened by its exact identity and an ambiguity is refused (AC-1); a step with no SLA is unscheduled, never
/// overdue (AC-2); a timestamp without a confirmed zone says so (AC-3); the timeline holds every step and every event in order
/// (AC-4); every entry states its actor or says none is recorded (AC-5); and the originating record is resolved, with its absence
/// shown (AC-6).
/// </remarks>
public sealed class InstancesPageView : OgPageBase
{
    public const string Route = "instances";

    private readonly OgApplication _application;
    private readonly IOgProcessInstances _instances;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _state;
    private readonly TextBlock _subject;
    private readonly TextBlock _blocks;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _instance;
    private readonly TextBox _subjectKind;
    private readonly TextBox _subjectId;
    private readonly DataGrid _instanceGrid;
    private readonly DataGrid _timelineGrid;
    private readonly DataGrid _historyGrid;
    private readonly Button _loadButton;
    private readonly Button _openButton;

    private OgProcessInstanceWorkspace? _workspace;

    public InstancesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _instances = application.ProcessInstances;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _state = Body(string.Empty);
        _state.TextWrapping = TextWrapping.Wrap;
        _state.Margin = new Thickness(0, 0, 0, 4);

        _subject = Body(string.Empty);
        _subject.TextWrapping = TextWrapping.Wrap;
        _subject.Margin = new Thickness(0, 0, 0, 4);

        _blocks = Body(string.Empty);
        _blocks.TextWrapping = TextWrapping.Wrap;
        _blocks.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 110);
        _instance = Field("Instance id", 190);
        _subjectKind = Field("Subject kind", 110);
        _subjectId = Field("Subject id", 120);

        _instanceGrid = Grid("Instances", ("Instance", "Id", 190), ("Definition", "DefinitionName", 170),
            ("Subject", "SubjectText", 150), ("State", "State", 110), ("Step", "CurrentStepName", 140),
            ("Assignee", "Assignee", 100), ("Age", "AgeText", 100), ("Due", "DueText", 120),
            ("SLA", "SlaState", 100));

        _timelineGrid = Grid("Timeline", ("At", "At", 150), ("Zone", "ZoneText", 90), ("Kind", "Kind", 70),
            ("Step", "StepKey", 160), ("Outcome", "Outcome", 120), ("Actor", "Actor", 110),
            ("Comment", "Comment", 220));

        _historyGrid = Grid("History", ("At", "At", 150), ("Event", "StepKey", 150), ("Outcome", "Outcome", 110),
            ("Actor", "Actor", 110), ("Comment", "Comment", 240));

        _instanceGrid.SelectionChanged += (_, _) => OpenSelected();

        _loadButton = Button("Refresh", (_, _) => Load());
        _openButton = Button("Open instance", (_, _) => OpenByIdentity());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Field"));
        loadBar.Children.Add(_field);
        loadBar.Children.Add(Caption("Instance"));
        loadBar.Children.Add(_instance);
        loadBar.Children.Add(_openButton);
        loadBar.Children.Add(_loadButton);

        var subjectBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        subjectBar.Children.Add(Caption("By subject"));
        subjectBar.Children.Add(_subjectKind);
        subjectBar.Children.Add(_subjectId);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(loadBar);
        panel.Children.Add(subjectBar);
        panel.Children.Add(Caption("Instances"));
        panel.Children.Add(_instanceGrid);
        panel.Children.Add(Caption("Selected instance"));
        panel.Children.Add(_state);
        panel.Children.Add(_subject);
        panel.Children.Add(_blocks);
        panel.Children.Add(Caption("Timeline"));
        panel.Children.Add(_timelineGrid);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(_historyGrid);
        panel.Children.Add(_failure);

        ShowContent(panel);

        _field.Text = application.Session.FieldId ?? string.Empty;

        Load();
    }

    public OgProcessInstanceWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string StateText => _state.Text;

    public string SubjectText => _subject.Text;

    public string BlocksText => _blocks.Text;

    public string FailureText => _failure.Text;

    public string RefreshText => _refreshed.Text;

    public bool Load() => Read(null, null, null);

    /// <summary>Opens the instance whose identity is typed, so a grid row is never the only way in.</summary>
    public bool OpenByIdentity() =>
        Read(Empty(_instance.Text), Empty(_subjectKind.Text), Empty(_subjectId.Text));

    private bool Read(string? instanceId, string? subjectKind, string? subjectId)
    {
        _header.Text = "Processes · instances";

        var loaded = _instances
            .LoadAsync(Empty(_field.Text), instanceId, subjectKind, subjectId, DateTime.UtcNow)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _counts.Text = string.Empty;
            _state.Text = string.Empty;
            _subject.Text = string.Empty;
            _blocks.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _instanceGrid.ItemsSource = null;
            _timelineGrid.ItemsSource = null;
            _historyGrid.ItemsSource = null;
            _failure.Text = loaded.Message ?? "The process instances could not be read.";
            ShowState(OgViewState.Failed, "The process instances are not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    private void Apply(OgProcessInstanceWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.ReadText;
        _instanceGrid.ItemsSource = workspace.Instances.ToList();
        _instanceGrid.SelectedItem = workspace.Selected;

        _timelineGrid.ItemsSource = workspace.Timeline.ToList();
        _historyGrid.ItemsSource = workspace.History.ToList();

        if (workspace.Selected is { } selected)
        {
            _state.Text = selected.Describe();
            _instance.Text = selected.Id;
        }
        else
        {
            _state.Text = "No instance is open: choose one from the grid, or open it by its identity.";
        }

        _subject.Text = workspace.SubjectText;
        _blocks.Text = workspace.BlocksText;

        _failure.Text = string.Empty;

        ShowState(workspace.Instances.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void OpenSelected()
    {
        if (_instanceGrid.SelectedItem is not OgProcessInstanceView selected)
        {
            return;
        }

        // The grid row opens by its own identity, so the detail is the record that was clicked rather than a positional guess.
        Read(selected.Id, null, null);
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 170,
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
