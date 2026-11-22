using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DEC-03: facility decommissioning, its versioned cost estimate and its environmental restoration.
/// </summary>
/// <remarks>
/// The total is computed in decimal and shown step by step (AC-1); a revision is a new record and never rewrites the one it
/// replaces (AC-2); the restoration state is its own record that gates completion without sharing the facility's state (AC-3);
/// the provision preview names the accounting period (AC-4); every amount carries its currency (AC-5); and an approval records
/// the actor, the date and the conditions, with the author kept out of it (AC-6).
/// </remarks>
public sealed class FacilityCostPageView : OgPageBase
{
    public const string Route = "facilitycost";

    private readonly OgApplication _application;
    private readonly IOgFacilityCost _facilityCost;

    private readonly TextBlock _header;
    private readonly TextBlock _scope;
    private readonly TextBlock _checklist;
    private readonly TextBlock _steps;
    private readonly TextBlock _provision;
    private readonly TextBlock _restoration;
    private readonly TextBlock _notice;
    private readonly TextBlock _links;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBox _facility;
    private readonly TextBox _plannedDate;
    private readonly TextBox _method;
    private readonly TextBox _currency;
    private readonly TextBox _lines;
    private readonly TextBox _rate;
    private readonly TextBox _years;
    private readonly TextBox _contingency;
    private readonly TextBox _reason;
    private readonly TextBox _conditions;
    private readonly TextBox _restorationType;
    private readonly TextBox _restorationMethod;
    private readonly TextBox _evidence;
    private readonly TextBox _rationale;
    private readonly DataGrid _revisionGrid;
    private readonly DataGrid _restorationGrid;
    private readonly Button _loadButton;
    private readonly Button _saveButton;
    private readonly Button _approveButton;
    private readonly Button _planRestorationButton;
    private readonly Button _completeRestorationButton;
    private readonly Button _completeButton;

    private OgFacilityWorkspace? _workspace;
    private OgCostRevision? _revision;
    private OgRestoration? _restorationItem;

    public FacilityCostPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _facilityCost = application.FacilityCost;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _scope = Body(string.Empty);
        _scope.TextWrapping = TextWrapping.Wrap;
        _scope.Margin = new Thickness(0, 4, 0, 4);

        _checklist = Body(string.Empty);
        _checklist.TextWrapping = TextWrapping.Wrap;
        _checklist.Margin = new Thickness(0, 0, 0, 4);

        _steps = Body(string.Empty);
        _steps.TextWrapping = TextWrapping.Wrap;
        _steps.Margin = new Thickness(0, 4, 0, 4);

        _provision = Body(string.Empty);
        _provision.TextWrapping = TextWrapping.Wrap;
        _provision.Margin = new Thickness(0, 0, 0, 4);

        _restoration = Body(string.Empty);
        _restoration.TextWrapping = TextWrapping.Wrap;
        _restoration.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _links = Body(string.Empty);
        _links.TextWrapping = TextWrapping.Wrap;
        _links.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 0, 0, 4);

        _facility = Field("Facility", 140);
        _plannedDate = Field("Planned date yyyy-MM-dd", 160);
        _method = Field("Method", 140);
        _currency = Field("Currency", 80);
        _lines = Field("Lines (CATEGORY=amount; ...)", 320);
        _rate = Field("Escalation rate (0.03)", 130);
        _years = Field("Escalation years", 110);
        _contingency = Field("Contingency % (10)", 120);
        _reason = Field("Revision reason", 200);
        _conditions = Field("Approval conditions", 220);
        _restorationType = Field("Restoration type", 150);
        _restorationMethod = Field("Restoration method", 150);
        _evidence = Field("Restoration evidence", 190);
        _rationale = Field("Completion rationale", 200);

        _revisionGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Revision", "RevisionNo", 80),
                Text("Total", "Computation", 120),
                Text("Currency", "Currency", 80),
                Text("Status", "Status", 90),
                Text("Author", "Author", 100),
                Text("Reason", "Reason", 220),
            },
        };

        Named(_revisionGrid, "Cost revisions");
        _revisionGrid.SelectionChanged += (_, _) => SelectRevision();

        _restorationGrid = new DataGrid
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
                Text("Type", "Type", 150),
                Text("Status", "Status", 110),
                Text("Certification", "Certification", 120),
                Text("Completed", "Completed", 110),
            },
        };

        Named(_restorationGrid, "Environmental restoration");
        _restorationGrid.SelectionChanged += (_, _) => SelectRestoration();

        _loadButton = Button("Load", (_, _) => Load());
        _saveButton = Button("Save revision", (_, _) => _ = SaveAsync());
        _approveButton = Button("Approve selected revision", (_, _) => _ = ApproveAsync());
        _planRestorationButton = Button("Plan restoration", (_, _) => _ = PlanRestorationAsync());
        _completeRestorationButton = Button("Complete selected restoration", (_, _) => _ = CompleteRestorationAsync());
        _completeButton = Button("Complete facility", (_, _) => _ = CompleteAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Facility"));
        loadBar.Children.Add(_facility);
        loadBar.Children.Add(_loadButton);

        var estimateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        estimateBar.Children.Add(Caption("Planned"));
        estimateBar.Children.Add(_plannedDate);
        estimateBar.Children.Add(Caption("Method"));
        estimateBar.Children.Add(_method);
        estimateBar.Children.Add(Caption("Currency"));
        estimateBar.Children.Add(_currency);
        estimateBar.Children.Add(Caption("Lines"));
        estimateBar.Children.Add(_lines);

        var estimateBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        estimateBar2.Children.Add(Caption("Rate"));
        estimateBar2.Children.Add(_rate);
        estimateBar2.Children.Add(Caption("Years"));
        estimateBar2.Children.Add(_years);
        estimateBar2.Children.Add(Caption("Contingency"));
        estimateBar2.Children.Add(_contingency);

        var estimateBar3 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        estimateBar3.Children.Add(Caption("Reason"));
        estimateBar3.Children.Add(_reason);
        estimateBar3.Children.Add(_saveButton);

        var approveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        approveBar.Children.Add(Caption("Conditions"));
        approveBar.Children.Add(_conditions);
        approveBar.Children.Add(_approveButton);

        var restorationBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        restorationBar.Children.Add(_restorationType);
        restorationBar.Children.Add(_restorationMethod);
        restorationBar.Children.Add(_planRestorationButton);

        var restorationBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        restorationBar2.Children.Add(Caption("Evidence"));
        restorationBar2.Children.Add(_evidence);
        restorationBar2.Children.Add(_completeRestorationButton);

        var completeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        completeBar.Children.Add(Caption("Rationale"));
        completeBar.Children.Add(_rationale);
        completeBar.Children.Add(_completeButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_scope);
        panel.Children.Add(_checklist);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Cost estimate"));
        panel.Children.Add(_steps);
        panel.Children.Add(_provision);
        panel.Children.Add(estimateBar);
        panel.Children.Add(estimateBar2);
        panel.Children.Add(estimateBar3);
        panel.Children.Add(Caption("Revisions"));
        panel.Children.Add(_revisionGrid);
        panel.Children.Add(approveBar);
        panel.Children.Add(Caption("Environmental restoration"));
        panel.Children.Add(_restoration);
        panel.Children.Add(_restorationGrid);
        panel.Children.Add(restorationBar);
        panel.Children.Add(restorationBar2);
        panel.Children.Add(Caption("Complete"));
        panel.Children.Add(completeBar);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Sources"));
        panel.Children.Add(_links);

        ShowContent(panel);

        // The planned date is a starting point; the currency, the rate, the period and the contingency are deliberately left
        // empty, because the repository holds no defaults for them and AC-5 wants the currency stated on every amount.
        _plannedDate.Text = DateTime.Today.AddMonths(6).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        ShowState(OgViewState.Empty, "Enter the facility and load its decommissioning record.");
        RefreshEnabled();
    }

    public OgFacilityWorkspace? Workspace => _workspace;

    public OgCostRevision? Revision => _revision;

    public OgRestoration? RestorationItem => _restorationItem;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string ScopeText => _scope.Text;

    public string ChecklistText => _checklist.Text;

    public string StepsText => _steps.Text;

    public string ProvisionText => _provision.Text;

    public string RestorationText => _restoration.Text;

    public string NoticeText => _notice.Text;

    public string LinksText => _links.Text;

    public string FailureText => _failure.Text;

    public string RefreshedText => _refreshed.Text;

    public bool Load()
    {
        var facilityId = _facility.Text?.Trim();

        _header.Text = "Decommissioning · facilities and cost";

        if (string.IsNullOrWhiteSpace(facilityId))
        {
            _workspace = null;
            _revisionGrid.ItemsSource = null;
            _restorationGrid.ItemsSource = null;
            _scope.Text = string.Empty;
            _checklist.Text = string.Empty;
            _steps.Text = string.Empty;
            _provision.Text = string.Empty;
            _restoration.Text = string.Empty;
            _links.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _notice.Text = "Enter the facility whose decommissioning you are opening.";
            _failure.Text = string.Empty;
            RefreshEnabled();
            return false;
        }

        var loaded = _facilityCost
            .LoadAsync(OgHseScope.FromSession(_application.Session), facilityId)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _revisionGrid.ItemsSource = null;
            _restorationGrid.ItemsSource = null;
            _scope.Text = string.Empty;
            _checklist.Text = string.Empty;
            _steps.Text = string.Empty;
            _provision.Text = string.Empty;
            _restoration.Text = string.Empty;
            _links.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The facility decommissioning record could not be read.";
            ShowState(OgViewState.Failed, "The facility decommissioning record is not shown.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> SaveAsync()
    {
        var planned = Date(_plannedDate.Text);

        if (planned is null)
        {
            _failure.Text = "A revision needs the planned date it is estimated for.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _facilityCost.SaveEstimateAsync(
                OgHseScope.FromSession(_application.Session),
                FacilityId(),
                planned.Value,
                _method.Text ?? string.Empty,
                _currency.Text ?? string.Empty,
                Lines(_lines.Text),
                Number(_rate.Text) ?? 0m,
                Number(_years.Text) ?? 0m,
                Number(_contingency.Text) ?? 0m,
                _reason.Text,
                _application.Session.UserId),
            "decommissioning.cost.write",
            "Revision saved");
    }

    public Task<bool> ApproveAsync()
    {
        if (_revision is not { } revision)
        {
            _failure.Text = "Select the revision to approve.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _facilityCost.ApproveAsync(
                OgHseScope.FromSession(_application.Session),
                FacilityId(),
                revision.Id,
                _conditions.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.cost.approve",
            "Revision approved");
    }

    public Task<bool> PlanRestorationAsync() =>
        RunAsync(() => _facilityCost.PlanRestorationAsync(
                OgHseScope.FromSession(_application.Session),
                FacilityId(),
                _restorationType.Text ?? string.Empty,
                _restorationMethod.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.cost.write",
            "Restoration planned");

    public Task<bool> CompleteRestorationAsync()
    {
        if (_restorationItem is not { } record)
        {
            _failure.Text = "Select the restoration to complete.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _facilityCost.CompleteRestorationAsync(
                OgHseScope.FromSession(_application.Session),
                FacilityId(),
                record.Id,
                _evidence.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.cost.write",
            "Restoration completed");
    }

    public Task<bool> CompleteAsync() =>
        RunAsync(() => _facilityCost.CompleteFacilityAsync(
                OgHseScope.FromSession(_application.Session),
                FacilityId(),
                _rationale.Text ?? string.Empty,
                _application.Session.UserId),
            "decommissioning.cost.write",
            "Facility completed");

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgFacilityWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The decommissioning command was refused.";
            ShowState(OgViewState.Failed, "The decommissioning command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "decommissioning", outcome.Value.FacilityId, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgFacilityWorkspace workspace)
    {
        _workspace = workspace;

        _scope.Text = workspace.Facility?.Describe() ?? (workspace.FacilityId + " · no decommissioning record");
        _checklist.Text = workspace.ChecklistText;
        _steps.Text = workspace.CostText;
        _provision.Text = workspace.ProvisionText;
        _restoration.Text = workspace.RestorationText;
        _links.Text = workspace.LinksText;

        _refreshed.Text = workspace.ReadText;

        _revisionGrid.ItemsSource = workspace.Revisions
            .OrderByDescending(revision => revision.RevisionNo)
            .Select(revision => new
            {
                revision.RevisionNo,
                revision.Currency,
                revision.Status,
                revision.Author,
                revision.Reason,
                revision.Id,
                Computation = revision.Computation.TotalText,
            })
            .ToList();

        _revision = workspace.Current;
        _revisionGrid.SelectedItem = _revision;

        _restorationGrid.ItemsSource = workspace.Restoration.ToList();
        _restorationItem = workspace.Restoration.FirstOrDefault();
        _restorationGrid.SelectedItem = _restorationItem;

        if (workspace.Facility?.PlannedDate is { } planned)
        {
            _plannedDate.Text = planned.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        _notice.Text = workspace.Revisions.Count.ToString(CultureInfo.InvariantCulture) + " revision(s) · " +
                       (workspace.LatestApproved is { } approved
                           ? "latest approved " + approved.Computation.TotalText
                           : "no approved revision") +
                       " · " + workspace.RestorationOpen.ToString(CultureInfo.InvariantCulture) +
                       " restoration record(s) open";

        _failure.Text = string.Empty;

        RefreshEnabled();

        ShowState(workspace.IsComplete ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
    }

    private void SelectRevision()
    {
        if (_revisionGrid.SelectedItem is null)
        {
            return;
        }

        // The grid binds an anonymous shape, so the revision is resolved from the record by its number.
        var row = _revisionGrid.SelectedItem;
        var id = row.GetType().GetProperty("Id")?.GetValue(row) as string;

        _revision = _workspace?.Revisions.FirstOrDefault(revision =>
            string.Equals(revision.Id, id, StringComparison.OrdinalIgnoreCase)) ?? _workspace?.Current;

        RefreshEnabled();
    }

    private void SelectRestoration()
    {
        if (_restorationGrid.SelectedItem is not OgRestoration selected)
        {
            return;
        }

        _restorationItem = selected;
        RefreshEnabled();
    }

    private string FacilityId() => _facility.Text?.Trim() ?? string.Empty;

    private void RefreshEnabled()
    {
        var loaded = _workspace is not null;

        _saveButton.IsEnabled = true;
        _approveButton.IsEnabled = loaded && _revision is { IsApproved: false };
        _planRestorationButton.IsEnabled = loaded;
        _completeRestorationButton.IsEnabled = loaded && _restorationItem is { } record && !record.IsCertified;
        _completeButton.IsEnabled = loaded && !_workspace!.IsComplete;
    }

    private static IReadOnlyList<OgCostLine> Lines(string? text)
    {
        var lines = new List<OgCostLine>();

        foreach (var part in (text ?? string.Empty).Split([';'], StringSplitOptions.RemoveEmptyEntries |
                                                          StringSplitOptions.TrimEntries))
        {
            var separator = part.IndexOf('=');

            if (separator <= 0)
            {
                continue;
            }

            var category = part[..separator].Trim();
            var amount = part[(separator + 1)..].Trim();

            if (decimal.TryParse(amount, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                lines.Add(new OgCostLine(category, value));
            }
        }

        return lines;
    }

    private static decimal? Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;

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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
