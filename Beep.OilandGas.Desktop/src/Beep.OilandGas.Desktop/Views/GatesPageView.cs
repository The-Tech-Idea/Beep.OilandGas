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
/// WFL-04: the one approval surface every stage calls - evidence, decision, rationale and conditions.
/// </summary>
/// <remarks>
/// Approve is offered only when the checklist is complete and no blocking segregation conflict exists (AC-1, SG-14-3); a reject or
/// a return without a rationale is refused (AC-2, SG-14-4); a delegated decision shows the delegator and the delegate (AC-3,
/// SG-14-7); a decided gate is read-only and the original decision is retained (AC-4); conditions are recorded and reach the
/// subject (AC-5); and the author of the subject cannot approve it (AC-6).
/// </remarks>
public sealed class GatesPageView : OgPageBase
{
    public const string Route = "gates";

    private readonly OgApplication _application;
    private readonly IOgGateReviews _gates;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _delegation;
    private readonly TextBlock _checklist;
    private readonly TextBlock _blocks;
    private readonly TextBlock _decisionState;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _approval;
    private readonly TextBox _rationale;
    private readonly TextBox _conditions;
    private readonly ComboBox _kind;
    private readonly DataGrid _gateGrid;
    private readonly DataGrid _checklistGrid;
    private readonly DataGrid _priorGrid;
    private readonly Button _loadButton;
    private readonly Button _openButton;
    private readonly Button _decideButton;

    private OgGateWorkspace? _workspace;

    public GatesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gates = application.GateReviews;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _delegation = Body(string.Empty);
        _delegation.TextWrapping = TextWrapping.Wrap;
        _delegation.Margin = new Thickness(0, 0, 0, 4);

        _checklist = Body(string.Empty);
        _checklist.TextWrapping = TextWrapping.Wrap;
        _checklist.Margin = new Thickness(0, 0, 0, 4);

        _blocks = Body(string.Empty);
        _blocks.TextWrapping = TextWrapping.Wrap;
        _blocks.Margin = new Thickness(0, 0, 0, 4);

        _decisionState = Body(string.Empty);
        _decisionState.TextWrapping = TextWrapping.Wrap;
        _decisionState.FontWeight = FontWeights.SemiBold;
        _decisionState.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 110);
        _approval = Field("Gate", 160);
        _rationale = Field("Rationale", 260);
        _conditions = Field("Conditions", 260);

        _kind = new ComboBox { MinWidth = 190, Margin = new Thickness(0, 0, 8, 0) };
        _kind.ItemsSource = GateRules.Kinds.ToList();
        _kind.SelectedIndex = 0;
        Named(_kind, "Decision");

        _gateGrid = Grid("Gates", ("Gate", "Gate", 120), ("Subject", "SubjectText", 150),
            ("Definition", "DefinitionName", 160), ("Step", "StepName", 130), ("Requested by", "RequestedBy", 110),
            ("Age", "AgeDays", 80), ("Due", "DueState", 120), ("Decision", "DecisionState", 110));

        _checklistGrid = Grid("Checklist", ("Item", "Key", 170), ("Required", "Required", 80),
            ("Satisfied", "Satisfied", 90), ("Evidence", "EvidenceRef", 130), ("Why", "Why", 240));

        _priorGrid = Grid("Prior decisions", ("At", "At", 140), ("Decision", "Kind", 150),
            ("By", "Actor", 110), ("Acting for", "ActingFor", 110), ("Rationale", "Rationale", 240));

        _gateGrid.SelectionChanged += (_, _) => OpenSelected();

        _loadButton = Button("Refresh", (_, _) => Load());
        _openButton = Button("Open gate", (_, _) => OpenTyped());
        _decideButton = Button("Record decision", (_, _) => _ = DecideAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Field"));
        loadBar.Children.Add(_field);
        loadBar.Children.Add(Caption("Gate"));
        loadBar.Children.Add(_approval);
        loadBar.Children.Add(_openButton);
        loadBar.Children.Add(_loadButton);

        var decideBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        decideBar.Children.Add(_kind);
        decideBar.Children.Add(Caption("Rationale"));
        decideBar.Children.Add(_rationale);
        decideBar.Children.Add(Caption("Conditions"));
        decideBar.Children.Add(_conditions);
        decideBar.Children.Add(_decideButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Gates"));
        panel.Children.Add(_gateGrid);
        panel.Children.Add(Caption("Delegation"));
        panel.Children.Add(_delegation);
        panel.Children.Add(Caption("Checklist"));
        panel.Children.Add(_checklistGrid);
        panel.Children.Add(_checklist);
        panel.Children.Add(Caption("Blocks"));
        panel.Children.Add(_blocks);
        panel.Children.Add(_decisionState);
        panel.Children.Add(Caption("Decision"));
        panel.Children.Add(decideBar);
        panel.Children.Add(Caption("Prior decisions"));
        panel.Children.Add(_priorGrid);
        panel.Children.Add(_failure);
        panel.Children.Add(_notice);

        ShowContent(panel);

        _field.Text = application.Session.FieldId ?? string.Empty;

        Load();
    }

    public OgGateWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string DelegationText => _delegation.Text;

    public string ChecklistText => _checklist.Text;

    public string BlocksText => _blocks.Text;

    public string DecisionStateText => _decisionState.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string RefreshText => _refreshed.Text;

    public bool ApproveEnabled => _decideButton.IsEnabled;

    public bool Load() => Read(null);

    public bool OpenTyped() => Read(Empty(_approval.Text));

    private bool Read(string? approvalId)
    {
        _header.Text = "Processes · gates";

        var loaded = _gates
            .LoadAsync(Empty(_field.Text), approvalId, _application.Session.UserId)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _counts.Text = string.Empty;
            _delegation.Text = string.Empty;
            _checklist.Text = string.Empty;
            _blocks.Text = string.Empty;
            _decisionState.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _gateGrid.ItemsSource = null;
            _checklistGrid.ItemsSource = null;
            _priorGrid.ItemsSource = null;
            _notice.Text = string.Empty;
            _decideButton.IsEnabled = false;
            _failure.Text = loaded.Message ?? "The gates could not be read.";
            ShowState(OgViewState.Failed, "The gates are not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> DecideAsync()
    {
        if (_workspace?.Selected is not { } gate)
        {
            _failure.Text = "Open the gate you are deciding.";
            return false;
        }

        var kind = _kind.SelectedItem as string ?? GateRules.Approve;

        var outcome = await _gates.DecideAsync(
            gate.ApprovalId,
            kind,
            _rationale.Text ?? string.Empty,
            _conditions.Text ?? string.Empty,
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The decision was refused.";
            ShowState(OgViewState.Failed, "The decision was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("workflow.gate.decide", "workflow", gate.ApprovalId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Decision recorded: " + kind.ToLowerInvariant().Replace('_', ' ') + ".";

        return true;
    }

    private void Apply(OgGateWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.ReadText;
        _gateGrid.ItemsSource = workspace.Gates.ToList();
        _gateGrid.SelectedItem = workspace.Selected;

        if (workspace.Selected is not { } gate)
        {
            _delegation.Text = "No gate is open.";
            _checklist.Text = "No gate is open, so no checklist is shown.";
            _blocks.Text = string.Empty;
            _decisionState.Text = "No gate is open.";
            _checklistGrid.ItemsSource = null;
            _priorGrid.ItemsSource = null;
            _decideButton.IsEnabled = false;
            ShowState(OgViewState.Empty, workspace.CountsText);
            return;
        }

        _approval.Text = gate.ApprovalId;
        _delegation.Text = gate.DelegationText;
        _checklist.Text = gate.ChecklistText;
        _blocks.Text = gate.BlocksText;
        _decisionState.Text = gate.DecisionText;

        _checklistGrid.ItemsSource = gate.Checklist.ToList();
        _priorGrid.ItemsSource = gate.PriorDecisions.ToList();

        // AC-1 and SG-14-3: the button follows the same answer the blocks text gives.
        _decideButton.IsEnabled = gate.ApproveAllowed;

        _failure.Text = string.Empty;

        ShowState(OgViewState.Ready, gate.Describe());
    }

    private void OpenSelected()
    {
        if (_gateGrid.SelectedItem is not OgGateReview gate)
        {
            return;
        }

        Read(gate.ApprovalId);
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
