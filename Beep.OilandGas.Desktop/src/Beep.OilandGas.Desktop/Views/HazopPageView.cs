using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// HSE-04: a HAZOP study - its nodes, deviations, actions and legal transitions.
/// </summary>
/// <remarks>
/// The study is opened by its recorded id only (AC-1); a missing or mismatched acknowledgement is reported as unconfirmed
/// (AC-2); duplicate node keys are refused (AC-3); an action needs its node and its deviation (AC-4, SG-11-5); a field change
/// clears the study and discards the response (AC-5); and only the legal transitions are offered (AC-6).
/// </remarks>
public sealed class HazopPageView : OgPageBase
{
    public const string Route = "hazop";

    private readonly OgApplication _application;
    private readonly IOgHazop _hazop;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _reference;
    private readonly TextBlock _failure;
    private readonly TextBox _studyId;
    private readonly TextBox _nodeName;
    private readonly TextBox _nodeIntent;
    private readonly TextBox _parameter;
    private readonly TextBox _cause;
    private readonly TextBox _consequence;
    private readonly TextBox _safeguard;
    private readonly TextBox _action;
    private readonly TextBox _owner;
    private readonly TextBox _due;
    private readonly ComboBox _guideWord;
    private readonly ComboBox _risk;
    private readonly ComboBox _nextState;
    private readonly DataGrid _nodeGrid;
    private readonly DataGrid _deviationGrid;
    private readonly Button _loadButton;
    private readonly Button _nodeButton;
    private readonly Button _deviationButton;
    private readonly Button _actionButton;
    private readonly Button _transitionButton;

    private OgHazopWorkspace? _workspace;
    private OgHazopNode? _node;
    private OgHazopDeviation? _deviation;
    private long _version;

    public HazopPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _hazop = application.Hazop;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _studyId = Field("Study id", 160);
        _nodeName = Field("Node name", 160);
        _nodeIntent = Field("Design intent", 240);
        _parameter = Field("Parameter", 120);
        _cause = Field("Cause", 200);
        _consequence = Field("Consequence", 220);
        _safeguard = Field("Safeguard", 200);
        _action = Field("Action", 220);
        _owner = Field("Owner", 110);
        _due = Field("Due date yyyy-MM-dd", 150);

        _guideWord = Picker("Guide word", 130);
        _risk = Picker("Risk", 100);
        _nextState = Picker("Next state", 140);

        _nodeGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("#", "Seq", 50),
                Text("Node", "Name", 180),
                Text("Design intent", "DesignIntent", 300),
            },
        };

        Named(_nodeGrid, "HAZOP nodes");
        _nodeGrid.SelectionChanged += (_, _) => SelectNode();

        _deviationGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("#", "CondSeq", 50),
                Text("Guide word", "GuideWord", 100),
                Text("Parameter", "Parameter", 110),
                Text("Consequence", "Consequence", 190),
                Text("Risk", "Risk", 80),
                Text("State", "State", 120),
                Text("Action", "Action", 190),
            },
        };

        Named(_deviationGrid, "Deviations on the selected node");
        _deviationGrid.SelectionChanged += (_, _) => SelectDeviation();

        _loadButton = Button("Load study", (_, _) => Load());
        _nodeButton = Button("Add node", (_, _) => _ = AddNodeAsync());
        _deviationButton = Button("Add deviation", (_, _) => _ = AddDeviationAsync());
        _actionButton = Button("Raise action", (_, _) => _ = AddActionAsync());
        _transitionButton = Button("Apply transition", (_, _) => _ = TransitionAsync());

        var studyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        studyBar.Children.Add(Caption("Study"));
        studyBar.Children.Add(_studyId);
        studyBar.Children.Add(_loadButton);

        var nodeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        nodeBar.Children.Add(Caption("Node"));
        nodeBar.Children.Add(_nodeName);
        nodeBar.Children.Add(Caption("Design intent"));
        nodeBar.Children.Add(_nodeIntent);
        nodeBar.Children.Add(_nodeButton);

        var deviationBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        deviationBar.Children.Add(_guideWord);
        deviationBar.Children.Add(Caption("Parameter"));
        deviationBar.Children.Add(_parameter);
        deviationBar.Children.Add(Caption("Cause"));
        deviationBar.Children.Add(_cause);
        deviationBar.Children.Add(Caption("Consequence"));
        deviationBar.Children.Add(_consequence);

        var deviationBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        deviationBar2.Children.Add(Caption("Safeguard"));
        deviationBar2.Children.Add(_safeguard);
        deviationBar2.Children.Add(_risk);
        deviationBar2.Children.Add(_deviationButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(Caption("Action"));
        actionBar.Children.Add(_action);
        actionBar.Children.Add(Caption("Owner"));
        actionBar.Children.Add(_owner);
        actionBar.Children.Add(Caption("Due"));
        actionBar.Children.Add(_due);
        actionBar.Children.Add(_actionButton);

        var transitionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        transitionBar.Children.Add(Caption("Study transition"));
        transitionBar.Children.Add(_nextState);
        transitionBar.Children.Add(_transitionButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(_failure);
        panel.Children.Add(studyBar);
        panel.Children.Add(Caption("Nodes"));
        panel.Children.Add(nodeBar);
        panel.Children.Add(_nodeGrid);
        panel.Children.Add(Caption("Deviations"));
        panel.Children.Add(deviationBar);
        panel.Children.Add(deviationBar2);
        panel.Children.Add(_deviationGrid);
        panel.Children.Add(Caption("Action for the selected deviation"));
        panel.Children.Add(actionBar);
        panel.Children.Add(transitionBar);

        ShowContent(panel);

        _due.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ShowState(OgViewState.Empty, "Enter the study id and load it.");

        RefreshEnabled();
    }

    public OgHazopWorkspace? Workspace => _workspace;

    public OgHazopNode? Node => _node;

    public OgHazopDeviation? Deviation => _deviation;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ReferenceText => _reference.Text;

    public string FailureText => _failure.Text;

    public bool Load()
    {
        _header.Text = "HSE · HAZOP";

        var studyId = _studyId.Text?.Trim();

        if (string.IsNullOrWhiteSpace(studyId))
        {
            _workspace = null;
            _nodeGrid.ItemsSource = null;
            _deviationGrid.ItemsSource = null;
            _notice.Text = "Enter the recorded study id.";
            _failure.Text = string.Empty;
            RefreshEnabled();
            return false;
        }

        var session = _application.Session;
        var scope = OgHseScope.FromSession(session);

        _version = _hazop.BeginLoad();

        var loaded = _hazop.LoadAsync(
            _version,
            studyId,
            scope,
            () => OgHseScope.FromSession(_application.Session)).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _nodeGrid.ItemsSource = null;
            _deviationGrid.ItemsSource = null;
            _notice.Text = string.Empty;
            _reference.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The HAZOP study could not be read.";
            ShowState(OgViewState.Failed, "The HAZOP study is not shown.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> AddNodeAsync() =>
        RunAsync(() => _hazop.AddNodeAsync(
                StudyId(), _nodeName.Text ?? string.Empty, _nodeIntent.Text ?? string.Empty,
                _application.Session.UserId),
            "hse.hazop.write",
            "Node added");

    public Task<bool> AddDeviationAsync()
    {
        if (_node is not { } node)
        {
            _failure.Text = "Select the node the deviation belongs to.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _hazop.AddDeviationAsync(
                StudyId(),
                node.StepId,
                _guideWord.SelectedItem as string ?? string.Empty,
                _parameter.Text ?? string.Empty,
                _cause.Text ?? string.Empty,
                _consequence.Text ?? string.Empty,
                _safeguard.Text ?? string.Empty,
                _risk.SelectedItem as string ?? string.Empty,
                _application.Session.UserId),
            "hse.hazop.write",
            "Deviation added");
    }

    public Task<bool> AddActionAsync()
    {
        if (_node is not { } node)
        {
            _failure.Text = "Select the node first.";
            return Task.FromResult(false);
        }

        if (_deviation is not { } deviation)
        {
            _failure.Text = "Select the deviation the action answers.";
            return Task.FromResult(false);
        }

        var due = Date(_due.Text);

        if (due is null)
        {
            _failure.Text = "An action needs its due date.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _hazop.AddActionAsync(
                StudyId(), node.StepId, deviation.CondSeq, _action.Text ?? string.Empty,
                _owner.Text ?? string.Empty, due.Value, _application.Session.UserId),
            "hse.hazop.write",
            "Action raised");
    }

    public Task<bool> TransitionAsync()
    {
        if (_workspace is not { } workspace)
        {
            _failure.Text = "Load a study first.";
            return Task.FromResult(false);
        }

        var target = _nextState.SelectedItem as string;

        if (string.IsNullOrWhiteSpace(target))
        {
            _failure.Text = "There is no legal transition to apply from this state.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _hazop.TransitionStudyAsync(
                StudyId(), target, workspace.Study.State, _application.Session.UserId),
            "hse.hazop.write",
            "Study moved to " + target);
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgHazopWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The HAZOP command was refused.";
            ShowState(OgViewState.Failed, "The HAZOP command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "hse", outcome.Value.Study.Id, "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgHazopWorkspace workspace)
    {
        _workspace = workspace;

        _header.Text = workspace.Study.HeaderText;
        _reference.Text = workspace.ReferenceText + " · " + workspace.ReadText;

        _nodeGrid.ItemsSource = workspace.Study.Nodes.ToList();
        _node = workspace.SelectedNode;
        _nodeGrid.SelectedItem = _node;

        _guideWord.ItemsSource = workspace.GuideWords.ToList();
        _guideWord.SelectedIndex = _guideWord.SelectedIndex < 0 ? 0 : _guideWord.SelectedIndex;

        _risk.ItemsSource = workspace.RiskRatings.ToList();
        _risk.SelectedIndex = _risk.SelectedIndex < 0 ? 0 : _risk.SelectedIndex;

        // AC-6: only the legal next states are offered, and none at all from a closed study.
        _nextState.ItemsSource = workspace.LegalNextStates.ToList();
        _nextState.SelectedIndex = workspace.LegalNextStates.Count > 0
            ? (_nextState.SelectedIndex < 0 ? 0 : _nextState.SelectedIndex)
            : -1;

        ShowDeviations();

        _notice.Text = workspace.Study.NodeCount.ToString(CultureInfo.InvariantCulture) + " node(s) · " +
                       workspace.Study.DeviationCount.ToString(CultureInfo.InvariantCulture) + " deviation(s)";

        RefreshEnabled();
        ShowState(OgViewState.Ready, _notice.Text);
    }

    private void ShowDeviations()
    {
        var deviations = _node?.Deviations ?? [];

        _deviationGrid.ItemsSource = deviations.ToList();
        _deviation = deviations.FirstOrDefault();
        _deviationGrid.SelectedItem = _deviation;
    }

    private void SelectNode()
    {
        if (_nodeGrid.SelectedItem is not OgHazopNode selected)
        {
            return;
        }

        _node = selected;
        ShowDeviations();
        RefreshEnabled();
    }

    private void SelectDeviation()
    {
        if (_deviationGrid.SelectedItem is not OgHazopDeviation selected)
        {
            return;
        }

        _deviation = selected;
        RefreshEnabled();
    }

    private string StudyId() => _studyId.Text?.Trim() ?? string.Empty;

    private void RefreshEnabled()
    {
        var loaded = _workspace is not null;

        _nodeButton.IsEnabled = loaded;
        _deviationButton.IsEnabled = loaded && _node is not null;
        _actionButton.IsEnabled = loaded && _node is not null && _deviation is not null;
        _transitionButton.IsEnabled = loaded && _workspace?.LegalNextStates.Count > 0;
    }

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

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

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(picker, name);
        return picker;
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
        TextWrapping = TextWrapping.Wrap,
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
