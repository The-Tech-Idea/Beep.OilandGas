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
/// WFL-06: the workflow configuration every approval depends on - versions, dependencies, triggers, segregation rules,
/// delegations, compensating controls and diagnostics.
/// </summary>
/// <remarks>
/// A dependency that would close a cycle is refused before saving (AC-1); every change records actor, reason and the value it
/// replaced (AC-2); a delegation outside its window is inactive and marked (AC-3); the segregation tester answers pass or fail
/// and names the rule (AC-4); orphaned instances are listed with their cause (AC-5); and the page is gated to administrators
/// (AC-6).
/// </remarks>
public sealed class WorkflowAdminPageView : OgPageBase
{
    public const string Route = "workflowadmin";

    private readonly OgApplication _application;
    private readonly IOgWorkflowAdmin _admin;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _diagnostics;
    private readonly TextBlock _sodTest;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _reason;
    private readonly TextBox _confirmation;
    private readonly TextBox _dependent;
    private readonly TextBox _prerequisite;
    private readonly TextBox _triggerId;
    private readonly TextBox _ruleId;
    private readonly TextBox _definition;
    private readonly TextBox _version;
    private readonly TextBox _delegator;
    private readonly TextBox _delegate;
    private readonly TextBox _delegationFrom;
    private readonly TextBox _delegationTo;
    private readonly TextBox _limit;
    private readonly TextBox _currency;
    private readonly TextBox _testA;
    private readonly TextBox _testB;
    private readonly DataGrid _versionGrid;
    private readonly DataGrid _dependencyGrid;
    private readonly DataGrid _triggerGrid;
    private readonly DataGrid _sodGrid;
    private readonly DataGrid _delegationGrid;
    private readonly DataGrid _controlGrid;
    private readonly DataGrid _changeGrid;
    private readonly Button _loadButton;
    private readonly Button _addDependencyButton;
    private readonly Button _toggleTriggerButton;
    private readonly Button _toggleRuleButton;
    private readonly Button _activateButton;
    private readonly Button _addDelegationButton;
    private readonly Button _testButton;

    private OgAdminWorkspace? _workspace;

    public WorkflowAdminPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _admin = application.WorkflowAdmin;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _diagnostics = Body(string.Empty);
        _diagnostics.TextWrapping = TextWrapping.Wrap;
        _diagnostics.Margin = new Thickness(0, 0, 0, 4);

        _sodTest = Body(string.Empty);
        _sodTest.TextWrapping = TextWrapping.Wrap;
        _sodTest.FontWeight = FontWeights.SemiBold;
        _sodTest.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _reason = Field("Reason", 240);
        _confirmation = Field("Confirmation naming the affected definitions", 300);
        _dependent = Field("Dependent definition", 140);
        _prerequisite = Field("Prerequisite definition", 150);
        _triggerId = Field("Trigger id", 140);
        _ruleId = Field("SoD rule id", 140);
        _definition = Field("Definition id", 140);
        _version = Field("Version id", 140);
        _delegator = Field("Delegator", 110);
        _delegate = Field("Delegate", 110);
        _delegationFrom = Field("From yyyy-MM-dd", 130);
        _delegationTo = Field("To yyyy-MM-dd", 130);
        _limit = Field("Authority limit", 120);
        _currency = Field("Currency", 80);
        _testA = Field("Permission A", 150);
        _testB = Field("Permission B", 150);

        _versionGrid = Grid("Versions", ("Definition", "DefinitionName", 160), ("Version", "Number", 80),
            ("Change", "Change", 220), ("State", "StateText", 200));

        _dependencyGrid = Grid("Dependencies", ("From", "From", 150), ("To", "To", 150), ("Type", "Type", 110),
            ("Condition", "Condition", 180));

        _triggerGrid = Grid("Triggers", ("Name", "Name", 170), ("Event", "EventType", 130),
            ("Entity", "EntityType", 110), ("Target", "TargetDefinitionId", 140), ("State", "StateText", 90),
            ("Priority", "Priority", 70));

        _sodGrid = Grid("Segregation", ("Rule", "Name", 190), ("A", "PermissionA", 140), ("B", "PermissionB", 140),
            ("Severity", "Severity", 90), ("State", "StateText", 140));

        _delegationGrid = Grid("Delegations", ("Name", "Name", 200), ("Delegator", "Delegator", 100),
            ("Delegate", "Delegate", 100), ("From", "From", 100), ("To", "To", 100), ("Limit", "Limit", 90),
            ("State", "StateText", 220));

        _controlGrid = Grid("Compensating controls", ("User", "UserId", 100), ("Type", "Type", 120),
            ("Description", "Description", 260), ("Status", "Status", 90));

        _changeGrid = Grid("Recorded changes", ("At", "At", 140), ("Area", "Area", 100), ("Target", "TargetId", 150),
            ("Detail", "TargetDetail", 100), ("Actor", "Actor", 100), ("Reason", "Reason", 220),
            ("Was", "Previous", 160), ("Now", "New", 140));

        _loadButton = Button("Refresh", (_, _) => Load());
        _addDependencyButton = Button("Add dependency", (_, _) => _ = AddDependencyAsync());
        _toggleTriggerButton = Button("Enable / disable trigger", (_, _) => _ = ToggleTriggerAsync());
        _toggleRuleButton = Button("Make blocking / advisory", (_, _) => _ = ToggleRuleAsync());
        _activateButton = Button("Activate version", (_, _) => _ = ActivateAsync());
        _addDelegationButton = Button("Add delegation", (_, _) => _ = AddDelegationAsync());
        _testButton = Button("Test combination", (_, _) => TestCombination());

        var changeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        changeBar.Children.Add(Caption("Reason"));
        changeBar.Children.Add(_reason);
        changeBar.Children.Add(Caption("Confirm"));
        changeBar.Children.Add(_confirmation);
        changeBar.Children.Add(_loadButton);

        var dependencyBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        dependencyBar.Children.Add(Caption("Dependent"));
        dependencyBar.Children.Add(_dependent);
        dependencyBar.Children.Add(Caption("Prerequisite"));
        dependencyBar.Children.Add(_prerequisite);
        dependencyBar.Children.Add(_addDependencyButton);

        var triggerBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        triggerBar.Children.Add(Caption("Trigger"));
        triggerBar.Children.Add(_triggerId);
        triggerBar.Children.Add(_toggleTriggerButton);
        triggerBar.Children.Add(Caption("Rule"));
        triggerBar.Children.Add(_ruleId);
        triggerBar.Children.Add(_toggleRuleButton);

        var versionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        versionBar.Children.Add(Caption("Definition"));
        versionBar.Children.Add(_definition);
        versionBar.Children.Add(Caption("Version"));
        versionBar.Children.Add(_version);
        versionBar.Children.Add(_activateButton);

        var delegationBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        delegationBar.Children.Add(_delegator);
        delegationBar.Children.Add(_delegate);
        delegationBar.Children.Add(Caption("From"));
        delegationBar.Children.Add(_delegationFrom);
        delegationBar.Children.Add(Caption("To"));
        delegationBar.Children.Add(_delegationTo);

        var delegationBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        delegationBar2.Children.Add(Caption("Limit"));
        delegationBar2.Children.Add(_limit);
        delegationBar2.Children.Add(_currency);
        delegationBar2.Children.Add(_addDelegationButton);

        var sodBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sodBar.Children.Add(_testA);
        sodBar.Children.Add(_testB);
        sodBar.Children.Add(_testButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(changeBar);
        panel.Children.Add(Caption("Versions"));
        panel.Children.Add(_versionGrid);
        panel.Children.Add(versionBar);
        panel.Children.Add(Caption("Dependencies"));
        panel.Children.Add(_dependencyGrid);
        panel.Children.Add(dependencyBar);
        panel.Children.Add(Caption("Triggers"));
        panel.Children.Add(_triggerGrid);
        panel.Children.Add(triggerBar);
        panel.Children.Add(Caption("Segregation of duties"));
        panel.Children.Add(_sodGrid);
        panel.Children.Add(sodBar);
        panel.Children.Add(_sodTest);
        panel.Children.Add(Caption("Delegations"));
        panel.Children.Add(_delegationGrid);
        panel.Children.Add(delegationBar);
        panel.Children.Add(delegationBar2);
        panel.Children.Add(Caption("Compensating controls"));
        panel.Children.Add(_controlGrid);
        panel.Children.Add(Caption("Diagnostics"));
        panel.Children.Add(_diagnostics);
        panel.Children.Add(Caption("Recorded changes"));
        panel.Children.Add(_changeGrid);
        panel.Children.Add(_failure);
        panel.Children.Add(_notice);

        ShowContent(panel);

        Load();
    }

    public OgAdminWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string DiagnosticsText => _diagnostics.Text;

    public string SodTestText => _sodTest.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string RefreshText => _refreshed.Text;

    public bool Load() => Read();

    private bool Read()
    {
        _header.Text = "Processes · administration";

        var loaded = _admin.LoadAsync(DateTime.UtcNow).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _counts.Text = string.Empty;
            _diagnostics.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _versionGrid.ItemsSource = null;
            _dependencyGrid.ItemsSource = null;
            _triggerGrid.ItemsSource = null;
            _sodGrid.ItemsSource = null;
            _delegationGrid.ItemsSource = null;
            _controlGrid.ItemsSource = null;
            _changeGrid.ItemsSource = null;
            _failure.Text = loaded.Message ?? "The workflow configuration could not be read.";
            ShowState(OgViewState.Failed, "The workflow configuration is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> AddDependencyAsync() => RunAsync(() => _admin.AddDependencyAsync(
            _dependent.Text ?? string.Empty,
            _prerequisite.Text ?? string.Empty,
            "REQUIRES",
            null,
            _reason.Text ?? string.Empty,
            _confirmation.Text ?? string.Empty,
            _application.Session.UserId),
        "workflow.admin.dependency",
        "Dependency added");

    public Task<bool> ToggleTriggerAsync() => RunAsync(() =>
    {
        var trigger = _workspace?.Triggers.FirstOrDefault(row =>
            string.Equals(row.Id, _triggerId.Text?.Trim(), StringComparison.OrdinalIgnoreCase));

        return trigger is null
            ? Task.FromResult(OgResult<OgAdminWorkspace>.Failed("No trigger with that id is listed.", "workflow-admin-unknown-target"))
            : _admin.SetTriggerActiveAsync(trigger.Id, !trigger.Active, _reason.Text ?? string.Empty,
                _confirmation.Text ?? string.Empty, _application.Session.UserId);
    },
        "workflow.admin.trigger",
        "Trigger changed");

    public Task<bool> ToggleRuleAsync() => RunAsync(() =>
    {
        var rule = _workspace?.SodRules.FirstOrDefault(row =>
            string.Equals(row.Id, _ruleId.Text?.Trim(), StringComparison.OrdinalIgnoreCase));

        return rule is null
            ? Task.FromResult(OgResult<OgAdminWorkspace>.Failed("No segregation rule with that id is listed.", "workflow-admin-unknown-target"))
            : _admin.SetSodRuleBlockingAsync(rule.Id, !rule.Blocking, _reason.Text ?? string.Empty,
                _confirmation.Text ?? string.Empty, _application.Session.UserId);
    },
        "workflow.admin.sod",
        "Segregation rule changed");

    public Task<bool> ActivateAsync() => RunAsync(() => _admin.ActivateVersionAsync(
            _definition.Text ?? string.Empty,
            _version.Text ?? string.Empty,
            _reason.Text ?? string.Empty,
            _confirmation.Text ?? string.Empty,
            _application.Session.UserId),
        "workflow.admin.version",
        "Version activated");

    public Task<bool> AddDelegationAsync() => RunAsync(() => _admin.AddDelegationAsync(
            _delegator.Text ?? string.Empty,
            _delegate.Text ?? string.Empty,
            Date(_delegationFrom.Text) ?? DateTime.UtcNow,
            Date(_delegationTo.Text) ?? DateTime.UtcNow,
            decimal.TryParse(_limit.Text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var limit) ? limit : 0m,
            _currency.Text ?? string.Empty,
            _reason.Text ?? string.Empty,
            _confirmation.Text ?? string.Empty,
            _application.Session.UserId),
        "workflow.admin.delegation",
        "Delegation added");

    public void TestCombination()
    {
        if (_workspace is null)
        {
            _failure.Text = "Load the configuration before testing a combination.";
            return;
        }

        var test = _admin.TestSod(_workspace, _testA.Text ?? string.Empty, _testB.Text ?? string.Empty);

        _sodTest.Text = test.Describe();
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgAdminWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The change was refused.";
            ShowState(OgViewState.Failed, "The change was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "workflow", "configuration", "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgAdminWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.ReadText;
        _diagnostics.Text = workspace.DiagnosticsText;

        _versionGrid.ItemsSource = workspace.Versions.ToList();
        _dependencyGrid.ItemsSource = workspace.Dependencies.ToList();
        _triggerGrid.ItemsSource = workspace.Triggers.ToList();
        _sodGrid.ItemsSource = workspace.SodRules.ToList();
        _delegationGrid.ItemsSource = workspace.Delegations.ToList();
        _controlGrid.ItemsSource = workspace.Controls.ToList();
        _changeGrid.ItemsSource = workspace.Changes.ToList();

        _failure.Text = string.Empty;

        ShowState(workspace.Versions.Count == 0 && workspace.SodRules.Count == 0
            ? OgViewState.Empty
            : OgViewState.Ready, workspace.CountsText);
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 6, 0),
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
