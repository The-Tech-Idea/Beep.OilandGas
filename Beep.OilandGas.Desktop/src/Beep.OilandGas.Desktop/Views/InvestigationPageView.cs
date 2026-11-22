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
/// HSE-03: the investigation - assign the investigator, record the cause chain, drive corrective actions to verified
/// closure.
/// </summary>
/// <remarks>
/// An existing assignment is retained and a second one is blocked (AC-1) and cannot be cleared by another field context
/// (AC-2) or by a roster change (AC-3); an action is closed out only when it is completed <b>and</b> verified (AC-4); the
/// investigator cannot close (AC-5); and every action carries its owner and due date (AC-6).
/// </remarks>
public sealed class InvestigationPageView : OgPageBase
{
    public const string Route = "investigation";

    private readonly OgApplication _application;
    private readonly IOgInvestigation _investigation;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _investigator;
    private readonly TextBlock _checklist;
    private readonly TextBlock _failure;
    private readonly TextBox _incident;
    private readonly TextBox _participant;
    private readonly TextBox _role;
    private readonly TextBox _rationale;
    private readonly TextBox _causeDescription;
    private readonly TextBox _planName;
    private readonly TextBox _action;
    private readonly TextBox _owner;
    private readonly TextBox _due;
    private readonly TextBox _verifier;
    private readonly TextBox _decision;
    private readonly TextBox _reason;
    private readonly ComboBox _causeType;
    private readonly ComboBox _causeCategory;
    private readonly DataGrid _causeGrid;
    private readonly DataGrid _actionGrid;
    private readonly Button _loadButton;
    private readonly Button _assignButton;
    private readonly Button _causeButton;
    private readonly Button _actionButton;
    private readonly Button _completeButton;
    private readonly Button _verifyButton;
    private readonly Button _closeButton;

    private OgInvestigationWorkspace? _workspace;
    private OgCorrectiveAction? _selectedAction;

    public InvestigationPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _investigation = application.Investigation;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _investigator = Body(string.Empty);
        _investigator.TextWrapping = TextWrapping.Wrap;
        _investigator.Margin = new Thickness(0, 4, 0, 4);

        _checklist = Body(string.Empty);
        _checklist.TextWrapping = TextWrapping.Wrap;
        _checklist.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _incident = Field("Incident id", 150);
        _participant = Field("Participant id", 130);
        _role = Field("Role", 120);
        _rationale = Field("Rationale", 220);
        _causeDescription = Field("Cause description", 260);
        _planName = Field("Plan name", 170);
        _action = Field("Action", 240);
        _owner = Field("Owner", 110);
        _due = Field("Due date yyyy-MM-dd", 150);
        _verifier = Field("Verified by", 120);
        _decision = Field("Closing decision", 160);
        _reason = Field("Closing reason", 220);

        _causeType = Picker("Cause type", 130);
        _causeCategory = Picker("Cause category", 130);

        _causeGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("#", "Seq", 50),
                Text("Type", "Type", 110),
                Text("Category", "Category", 110),
                Text("Description", "Description", 380),
            },
        };

        Named(_causeGrid, "Cause chain");

        _actionGrid = new DataGrid
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
                Text("Action", "Action", 250),
                Text("Owner", "Owner", 110),
                Text("Due", "DueText", 150),
                Text("State", "State", 100),
                Text("Verified by", "VerifiedBy", 120),
            },
        };

        Named(_actionGrid, "Corrective actions");
        _actionGrid.SelectionChanged += (_, _) => SelectAction();

        _loadButton = Button("Load", (_, _) => Load());
        _assignButton = Button("Assign investigator", (_, _) => _ = AssignAsync());
        _causeButton = Button("Add cause", (_, _) => _ = AddCauseAsync());
        _actionButton = Button("Add action", (_, _) => _ = AddActionAsync());
        _completeButton = Button("Complete action", (_, _) => _ = CompleteAsync());
        _verifyButton = Button("Verify effectiveness", (_, _) => _ = VerifyAsync());
        _closeButton = Button("Close incident", (_, _) => _ = CloseAsync());

        var incidentBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        incidentBar.Children.Add(Caption("Incident"));
        incidentBar.Children.Add(_incident);
        incidentBar.Children.Add(_loadButton);

        var assignmentBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        assignmentBar.Children.Add(Caption("Participant"));
        assignmentBar.Children.Add(_participant);
        assignmentBar.Children.Add(Caption("Role"));
        assignmentBar.Children.Add(_role);
        assignmentBar.Children.Add(Caption("Rationale"));
        assignmentBar.Children.Add(_rationale);
        assignmentBar.Children.Add(_assignButton);

        var causeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        causeBar.Children.Add(_causeType);
        causeBar.Children.Add(_causeCategory);
        causeBar.Children.Add(_causeDescription);
        causeBar.Children.Add(_causeButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(Caption("Plan"));
        actionBar.Children.Add(_planName);
        actionBar.Children.Add(Caption("Action"));
        actionBar.Children.Add(_action);
        actionBar.Children.Add(Caption("Owner"));
        actionBar.Children.Add(_owner);

        var actionBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar2.Children.Add(Caption("Due"));
        actionBar2.Children.Add(_due);
        actionBar2.Children.Add(_actionButton);
        actionBar2.Children.Add(_completeButton);
        actionBar2.Children.Add(Caption("Verified by"));
        actionBar2.Children.Add(_verifier);
        actionBar2.Children.Add(_verifyButton);

        var closeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        closeBar.Children.Add(Caption("Decision"));
        closeBar.Children.Add(_decision);
        closeBar.Children.Add(Caption("Reason"));
        closeBar.Children.Add(_reason);
        closeBar.Children.Add(_closeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);
        panel.Children.Add(incidentBar);
        panel.Children.Add(Caption("Investigator"));
        panel.Children.Add(_investigator);
        panel.Children.Add(assignmentBar);
        panel.Children.Add(Caption("Root cause analysis"));
        panel.Children.Add(causeBar);
        panel.Children.Add(_causeGrid);
        panel.Children.Add(Caption("Corrective action plan"));
        panel.Children.Add(actionBar);
        panel.Children.Add(actionBar2);
        panel.Children.Add(_actionGrid);
        panel.Children.Add(Caption("Closure checklist"));
        panel.Children.Add(_checklist);
        panel.Children.Add(closeBar);

        ShowContent(panel);

        _due.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ShowState(OgViewState.Empty, "Enter an incident id and load it.");

        // Nothing is loaded until an incident is named; a page with no context shows no investigation rather than a guess.
        _loadButton.IsEnabled = true;
        _assignButton.IsEnabled = false;
        _causeButton.IsEnabled = false;
        _actionButton.IsEnabled = false;
        _completeButton.IsEnabled = false;
        _verifyButton.IsEnabled = false;
        _closeButton.IsEnabled = false;
    }

    public OgInvestigationWorkspace? Workspace => _workspace;

    public OgCorrectiveAction? SelectedAction => _selectedAction;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string InvestigatorText => _investigator.Text;

    public string ChecklistText => _checklist.Text;

    public string FailureText => _failure.Text;

    public bool Load()
    {
        _header.Text = "HSE · investigation";

        var incident = _incident.Text?.Trim();

        if (string.IsNullOrWhiteSpace(incident))
        {
            _workspace = null;
            _causeGrid.ItemsSource = null;
            _actionGrid.ItemsSource = null;
            _investigator.Text = string.Empty;
            _checklist.Text = string.Empty;
            _notice.Text = "Enter the incident id to investigate.";
            _failure.Text = string.Empty;
            RefreshEnabled();
            return false;
        }

        var loaded = _investigation.LoadAsync(incident).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _causeGrid.ItemsSource = null;
            _actionGrid.ItemsSource = null;
            _investigator.Text = string.Empty;
            _checklist.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The investigation could not be read.";
            _notice.Text = string.Empty;
            ShowState(OgViewState.Failed, "The investigation could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> AssignAsync() =>
        RunAsync(() => _investigation.AssignAsync(
                IncidentId(),
                _participant.Text ?? string.Empty,
                _role.Text ?? string.Empty,
                _rationale.Text ?? string.Empty,
                OgHseScope.FromSession(_application.Session),
                _application.Session.UserId),
            "hse.investigation.assign",
            "Investigator assigned");

    public Task<bool> AddCauseAsync() =>
        RunAsync(() => _investigation.AddCauseAsync(
                IncidentId(),
                _causeType.SelectedItem as string ?? string.Empty,
                _causeCategory.SelectedItem as string,
                _causeDescription.Text ?? string.Empty,
                _application.Session.UserId),
            "hse.investigation.write",
            "Cause recorded");

    public Task<bool> AddActionAsync()
    {
        var due = Date(_due.Text);

        if (due is null)
        {
            _failure.Text = "An action needs its due date.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _investigation.AddActionAsync(
                IncidentId(),
                string.IsNullOrWhiteSpace(_planName.Text) ? "Corrective action plan" : _planName.Text!,
                _action.Text ?? string.Empty,
                _owner.Text ?? string.Empty,
                due.Value,
                OgHseScope.FromSession(_application.Session),
                _application.Session.UserId),
            "hse.action.write",
            "Action added");
    }

    public Task<bool> CompleteAsync()
    {
        if (_selectedAction is not { } action)
        {
            _failure.Text = "Select the action to complete.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _investigation.CompleteActionAsync(
                IncidentId(), action.StepId, _application.Session.UserId),
            "hse.action.write",
            "Action completed");
    }

    public Task<bool> VerifyAsync()
    {
        if (_selectedAction is not { } action)
        {
            _failure.Text = "Select the action to verify.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _investigation.VerifyActionAsync(
                IncidentId(), action.StepId, _verifier.Text ?? string.Empty, _application.Session.UserId),
            "hse.action.write",
            "Effectiveness verified");
    }

    public Task<bool> CloseAsync() =>
        RunAsync(() => _investigation.CloseAsync(
                IncidentId(),
                _decision.Text ?? string.Empty,
                _reason.Text ?? string.Empty,
                _application.Session.UserId),
            "hse.incident.close",
            "Incident closed");

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgInvestigationWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The investigation command was refused.";
            ShowState(OgViewState.Failed, "The investigation command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "hse", outcome.Value.IncidentId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgInvestigationWorkspace workspace)
    {
        _workspace = workspace;

        _investigator.Text = "Investigator: " + workspace.InvestigatorText +
                             " · state " + (string.IsNullOrWhiteSpace(workspace.IncidentState)
                                 ? "not recorded"
                                 : workspace.IncidentState.ToLowerInvariant());

        _causeGrid.ItemsSource = workspace.Causes.ToList();
        _actionGrid.ItemsSource = workspace.Actions.ToList();

        _causeType.ItemsSource = InvestigationRules.CauseTypes.ToList();
        _causeType.SelectedIndex = _causeType.SelectedIndex < 0 ? 0 : _causeType.SelectedIndex;

        _causeCategory.ItemsSource = InvestigationRules.CauseCategories.ToList();
        _causeCategory.SelectedIndex = _causeCategory.SelectedIndex < 0 ? 0 : _causeCategory.SelectedIndex;

        _planName.Text = workspace.PlanName ?? _planName.Text;

        _checklist.Text = workspace.ChecklistText;

        _notice.Text = (workspace.RootCauseComplete ? "Root cause recorded" : "Root cause not complete") +
                       " · " + workspace.Unverified.Count.ToString(CultureInfo.InvariantCulture) +
                       " action(s) awaiting completion and verification";

        _failure.Text = string.Empty;

        RefreshEnabled();
        ShowState(OgViewState.Ready, _notice.Text);
    }

    private void SelectAction()
    {
        if (_actionGrid.SelectedItem is not OgCorrectiveAction selected)
        {
            return;
        }

        _selectedAction = selected;
        RefreshEnabled();
    }

    private string IncidentId() => _incident.Text?.Trim() ?? string.Empty;

    private void RefreshEnabled()
    {
        var loaded = _workspace is not null;

        _assignButton.IsEnabled = loaded;
        _causeButton.IsEnabled = loaded;
        _actionButton.IsEnabled = loaded;
        _completeButton.IsEnabled = loaded && _selectedAction is { IsCompleted: false };
        _verifyButton.IsEnabled = loaded && _selectedAction is { IsCompleted: true, IsVerified: false };
        _closeButton.IsEnabled = loaded;
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
