using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WRK-04: the contractor register, the permit request, its issue and the execution-start gate.
/// </summary>
/// <remarks>
/// Execution cannot start until every required readiness item is satisfied (AC-1); an expired competency or permit leaves its
/// item unsatisfied with the expiry named (AC-2); the requester cannot issue their own permit (AC-3); the validity is a window
/// with both ends (AC-4); every readiness item carries its evidence reference (AC-5); and a retried request lands on the permit
/// it already wrote, so a lost acknowledgement never issues two (AC-6).
/// </remarks>
public sealed class PermitToWorkPageView : OgPageBase
{
    public const string Route = "permittowork";

    private readonly OgApplication _application;
    private readonly IOgPermitsToWork _permits;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _readiness;
    private readonly TextBlock _blocks;
    private readonly TextBlock _start;
    private readonly TextBlock _refreshed;
    private readonly TextBox _workOrder;
    private readonly TextBox _scope;
    private readonly TextBox _hazards;
    private readonly TextBox _isolation;
    private readonly TextBox _personnel;
    private readonly TextBox _materials;
    private readonly TextBox _from;
    private readonly TextBox _to;
    private readonly TextBox _intent;
    private readonly TextBox _conditions;
    private readonly DataGrid _contractorGrid;
    private readonly DataGrid _permitGrid;
    private readonly Button _loadButton;
    private readonly Button _requestButton;
    private readonly Button _issueButton;

    private OgPermitToWorkWorkspace? _workspace;
    private OgWorkPermit? _selected;

    public PermitToWorkPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _permits = application.PermitsToWork;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _readiness = Body(string.Empty);
        _readiness.TextWrapping = TextWrapping.Wrap;
        _readiness.Margin = new Thickness(0, 4, 0, 4);

        _blocks = Body(string.Empty);
        _blocks.TextWrapping = TextWrapping.Wrap;
        _blocks.Margin = new Thickness(0, 0, 0, 4);

        _start = Body(string.Empty);
        _start.TextWrapping = TextWrapping.Wrap;
        _start.FontWeight = FontWeights.SemiBold;
        _start.Margin = new Thickness(0, 0, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _workOrder = Field("Work order", 130);
        _scope = Field("Scope", 200);
        _hazards = Field("Hazards", 160);
        _isolation = Field("Isolation reference", 150);
        _personnel = Field("Personnel reference", 150);
        _materials = Field("Material reference", 150);
        _from = Field("Valid from yyyy-MM-dd", 140);
        _to = Field("Valid to yyyy-MM-dd", 140);
        _intent = Field("Intent key", 130);
        _conditions = Field("Conditions", 200);

        _contractorGrid = Grid("Contractors", ("Name", "Name", 170), ("Kind", "Kind", 110),
            ("State", "StateText", 140), ("Competency", "Competencies", 260));

        _permitGrid = Grid("Permits", ("Work order", "WorkOrderId", 110), ("State", "StateText", 160),
            ("Window", "WindowText", 260), ("Issuer", "Issuer", 100), ("Requester", "Requester", 100));

        _permitGrid.SelectionChanged += (_, _) => SelectPermit();

        _loadButton = Button("Refresh", (_, _) => Load());
        _requestButton = Button("Request permit", (_, _) => _ = RequestAsync());
        _issueButton = Button("Issue permit", (_, _) => _ = IssueAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Work order"));
        loadBar.Children.Add(_workOrder);
        loadBar.Children.Add(_loadButton);

        var requestBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        requestBar.Children.Add(Caption("Scope"));
        requestBar.Children.Add(_scope);
        requestBar.Children.Add(Caption("Hazards"));
        requestBar.Children.Add(_hazards);
        requestBar.Children.Add(Caption("Isolation"));
        requestBar.Children.Add(_isolation);

        var requestBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        requestBar2.Children.Add(Caption("Personnel"));
        requestBar2.Children.Add(_personnel);
        requestBar2.Children.Add(Caption("Materials"));
        requestBar2.Children.Add(_materials);
        requestBar2.Children.Add(Caption("From"));
        requestBar2.Children.Add(_from);
        requestBar2.Children.Add(Caption("To"));
        requestBar2.Children.Add(_to);

        var requestBar3 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        requestBar3.Children.Add(Caption("Intent"));
        requestBar3.Children.Add(_intent);
        requestBar3.Children.Add(_requestButton);

        var issueBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        issueBar.Children.Add(Caption("Conditions"));
        issueBar.Children.Add(_conditions);
        issueBar.Children.Add(_issueButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Contractors"));
        panel.Children.Add(_contractorGrid);
        panel.Children.Add(Caption("Permits"));
        panel.Children.Add(_permitGrid);
        panel.Children.Add(Caption("Readiness"));
        panel.Children.Add(_readiness);
        panel.Children.Add(Caption("Execution start"));
        panel.Children.Add(_blocks);
        panel.Children.Add(_start);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Request a permit"));
        panel.Children.Add(requestBar);
        panel.Children.Add(requestBar2);
        panel.Children.Add(requestBar3);
        panel.Children.Add(Caption("Issue"));
        panel.Children.Add(issueBar);
        panel.Children.Add(_notice);

        ShowContent(panel);

        _workOrder.Text = application.Session.FieldId ?? string.Empty;
        _intent.Text = "permit-" + Guid.NewGuid().ToString("N")[..8];

        Load();
    }

    public OgPermitToWorkWorkspace? Workspace => _workspace;

    public OgWorkPermit? Selected => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ReadinessText => _readiness.Text;

    public string BlocksText => _blocks.Text;

    public string StartText => _start.Text;

    public bool Load()
    {
        _header.Text = "Work · permit to work";

        var loaded = _permits
            .LoadAsync(Empty(_workOrder.Text), DateTime.UtcNow)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _counts.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _readiness.Text = string.Empty;
            _blocks.Text = string.Empty;
            _start.Text = string.Empty;
            _contractorGrid.ItemsSource = null;
            _permitGrid.ItemsSource = null;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The permit-to-work register could not be read.";
            ShowState(OgViewState.Failed, "The permit-to-work register is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> RequestAsync() => RunAsync(() => _permits.RequestAsync(
            _workOrder.Text?.Trim() ?? string.Empty,
            _scope.Text ?? string.Empty,
            Empty(_hazards.Text),
            Empty(_isolation.Text),
            Empty(_personnel.Text),
            Empty(_materials.Text),
            Date(_from.Text) ?? DateTime.UtcNow,
            Date(_to.Text) ?? DateTime.UtcNow,
            _intent.Text ?? string.Empty,
            _application.Session.UserId,
            DateTime.UtcNow),
        "work.permit.write",
        "Permit requested");

    public Task<bool> IssueAsync()
    {
        if (_selected is not { } permit)
        {
            _failure.Text = "Select the permit to issue.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _permits.IssueAsync(
                permit.Id,
                _conditions.Text ?? string.Empty,
                _application.Session.UserId,
                DateTime.UtcNow),
            "work.permit.issue",
            "Permit issued");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgPermitToWorkWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The permit command was refused.";
            ShowState(OgViewState.Failed, "The permit command was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "work", outcome.Value.Selected?.Id ?? "permit", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgPermitToWorkWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.ReadText;

        _contractorGrid.ItemsSource = workspace.Contractors.ToList();
        _permitGrid.ItemsSource = workspace.Permits.ToList();

        _selected = workspace.Selected;
        _permitGrid.SelectedItem = _selected;

        if (_selected is { } permit)
        {
            _workOrder.Text = permit.WorkOrderId;
            _scope.Text = permit.Scope;
            _hazards.Text = permit.Hazards ?? string.Empty;
            _isolation.Text = permit.IsolationRef ?? string.Empty;
            _personnel.Text = permit.PersonnelRef ?? string.Empty;
            _materials.Text = permit.MaterialRef ?? string.Empty;
            _from.Text = permit.ValidFromUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            _to.Text = permit.ValidToUtc?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

            _readiness.Text = permit.ReadinessText;
            _blocks.Text = permit.BlocksText;
            _start.Text = permit.StartText;
        }
        else
        {
            _readiness.Text = "No permit is selected, so no readiness is recorded.";
            _blocks.Text = "Execution cannot start: no permit is selected.";
            _start.Text = "No permit is selected.";
        }

        _issueButton.IsEnabled = _selected is { IsIssued: false };

        _failure.Text = string.Empty;

        ShowState(workspace.Permits.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void SelectPermit()
    {
        if (_permitGrid.SelectedItem is not OgWorkPermit permit)
        {
            return;
        }

        _selected = permit;
        _readiness.Text = permit.ReadinessText;
        _blocks.Text = permit.BlocksText;
        _start.Text = permit.StartText;
        _issueButton.IsEnabled = !permit.IsIssued;
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
}
