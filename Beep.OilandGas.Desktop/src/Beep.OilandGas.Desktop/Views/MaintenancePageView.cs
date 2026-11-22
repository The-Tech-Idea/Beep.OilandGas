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
/// WRK-03: maintenance plans, the jobs generated from them and the inspections against their assets.
/// </summary>
/// <remarks>
/// The next due date is last done plus the frequency in its unit and is never stored (AC-1, AC-4); an inspection with findings
/// cannot close without a linked corrective action (AC-2); a generated job records its plan and the generation date (AC-3); a
/// frequency always carries its unit (AC-5); and every recorded change names its actor and date (AC-6).
/// </remarks>
public sealed class MaintenancePageView : OgPageBase
{
    public const string Route = "maintenance";

    private readonly OgApplication _application;
    private readonly IOgMaintenance _maintenance;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _asset;
    private readonly TextBox _plan;
    private readonly TextBox _frequency;
    private readonly TextBox _lastDone;
    private readonly TextBox _tasks;
    private readonly TextBox _inspection;
    private readonly TextBox _result;
    private readonly TextBox _findings;
    private readonly ComboBox _unit;
    private readonly DataGrid _planGrid;
    private readonly DataGrid _jobGrid;
    private readonly DataGrid _inspectionGrid;
    private readonly Button _loadButton;
    private readonly Button _savePlanButton;
    private readonly Button _generateButton;
    private readonly Button _recordButton;

    private OgMaintenanceWorkspace? _workspace;
    private OgMaintenancePlan? _selectedPlan;
    private OgInspectionRow? _selectedInspection;

    public MaintenancePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _maintenance = application.Maintenance;

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

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _asset = Field("Asset", 130);
        _plan = Field("Plan id", 150);
        _frequency = Field("Frequency", 90);
        _lastDone = Field("Last done yyyy-MM-dd", 150);
        _tasks = Field("Tasks", 260);
        _inspection = Field("Inspection id", 150);
        _result = Field("Result", 90);
        _findings = Field("Findings", 240);

        _unit = new ComboBox { MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };
        _unit.ItemsSource = WorkMaintenanceRules.Units.ToList();
        Named(_unit, "Frequency unit");

        _planGrid = Grid("Plans", ("Asset", "AssetId", 110), ("Type", "Kind", 140), ("Frequency", "FrequencyText", 110),
            ("Last done", "LastDone", 100), ("Next due", "NextDueText", 170), ("State", "Compliance", 100));

        _jobGrid = Grid("Service jobs", ("Asset", "AssetId", 110), ("Job", "JobType", 140), ("Status", "Status", 90),
            ("Plan", "PlanId", 130), ("Generated", "GeneratedOn", 110));

        _inspectionGrid = Grid("Inspections", ("Kind", "Kind", 120), ("Result", "Result", 80),
            ("State", "StateText", 200), ("Due", "DueDate", 110), ("Inspector", "Inspector", 100));

        _planGrid.SelectionChanged += (_, _) => SelectPlan();
        _inspectionGrid.SelectionChanged += (_, _) => SelectInspection();

        _loadButton = Button("Refresh", (_, _) => Load());
        _savePlanButton = Button("Save schedule", (_, _) => _ = SavePlanAsync());
        _generateButton = Button("Generate job", (_, _) => _ = GenerateAsync());
        _recordButton = Button("Record result", (_, _) => _ = RecordAsync());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Asset"));
        loadBar.Children.Add(_asset);
        loadBar.Children.Add(_loadButton);

        var planBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        planBar.Children.Add(Caption("Plan"));
        planBar.Children.Add(_plan);
        planBar.Children.Add(Caption("Every"));
        planBar.Children.Add(_frequency);
        planBar.Children.Add(_unit);
        planBar.Children.Add(Caption("Last done"));
        planBar.Children.Add(_lastDone);
        planBar.Children.Add(Caption("Tasks"));
        planBar.Children.Add(_tasks);
        planBar.Children.Add(_savePlanButton);
        planBar.Children.Add(_generateButton);

        var inspectionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        inspectionBar.Children.Add(Caption("Inspection"));
        inspectionBar.Children.Add(_inspection);
        inspectionBar.Children.Add(Caption("Result"));
        inspectionBar.Children.Add(_result);
        inspectionBar.Children.Add(Caption("Findings"));
        inspectionBar.Children.Add(_findings);
        inspectionBar.Children.Add(_recordButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Plans"));
        panel.Children.Add(_planGrid);
        panel.Children.Add(planBar);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Service jobs"));
        panel.Children.Add(_jobGrid);
        panel.Children.Add(Caption("Inspections"));
        panel.Children.Add(_inspectionGrid);
        panel.Children.Add(inspectionBar);
        panel.Children.Add(_notice);

        ShowContent(panel);

        _asset.Text = application.Session.FieldId ?? string.Empty;

        Load();
    }

    public OgMaintenanceWorkspace? Workspace => _workspace;

    public OgMaintenancePlan? SelectedPlan => _selectedPlan;

    public OgInspectionRow? SelectedInspection => _selectedInspection;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string PlansText => _workspace?.PlansText ?? string.Empty;

    public string InspectionsText => _workspace?.InspectionsText ?? string.Empty;

    public bool Load()
    {
        _header.Text = "Work · maintenance";

        var loaded = _maintenance
            .LoadAsync(Empty(_asset.Text), DateTime.Today)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _counts.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _planGrid.ItemsSource = null;
            _jobGrid.ItemsSource = null;
            _inspectionGrid.ItemsSource = null;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The maintenance workspace could not be read.";
            ShowState(OgViewState.Failed, "The maintenance workspace is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> SavePlanAsync()
    {
        if (_selectedPlan is not { } plan)
        {
            _failure.Text = "Select the plan to edit.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _maintenance.EditPlanAsync(
                plan.Id,
                int.TryParse(_frequency.Text?.Trim(), out var frequency) ? frequency : null,
                _unit.SelectedItem as string,
                Date(_lastDone.Text),
                _tasks.Text,
                Empty(_asset.Text) ?? string.Empty,
                _application.Session.UserId,
                DateTime.Today),
            "work.maintenance.write",
            "Schedule saved");
    }

    public Task<bool> GenerateAsync()
    {
        if (_selectedPlan is not { } plan)
        {
            _failure.Text = "Select the plan to generate a job from.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _maintenance.GenerateJobAsync(
                plan.Id, Empty(_asset.Text) ?? string.Empty, _application.Session.UserId, DateTime.Today),
            "work.maintenance.generate",
            "Job generated");
    }

    public Task<bool> RecordAsync()
    {
        if (_selectedInspection is not { } inspection)
        {
            _failure.Text = "Select the inspection to record.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _maintenance.RecordInspectionAsync(
                inspection.Id,
                _result.Text ?? string.Empty,
                _findings.Text ?? string.Empty,
                Empty(_asset.Text) ?? string.Empty,
                _application.Session.UserId,
                DateTime.Today),
            "work.inspection.write",
            "Inspection recorded");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgMaintenanceWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The maintenance command was refused.";
            ShowState(OgViewState.Failed, "The maintenance command was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add(auditAction, "work", _selectedPlan?.Id ?? "maintenance", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgMaintenanceWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.ReadText;

        _planGrid.ItemsSource = workspace.Plans.ToList();
        _jobGrid.ItemsSource = workspace.Jobs.ToList();
        _inspectionGrid.ItemsSource = workspace.Inspections.ToList();

        _selectedPlan = workspace.Plans.FirstOrDefault();
        _planGrid.SelectedItem = _selectedPlan;

        _selectedInspection = workspace.Inspections.FirstOrDefault();
        _inspectionGrid.SelectedItem = _selectedInspection;

        if (_selectedPlan is { } plan)
        {
            _plan.Text = plan.Id;
            _frequency.Text = plan.Frequency?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _unit.SelectedItem = plan.Unit;
            _lastDone.Text = plan.LastDone?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
            _tasks.Text = plan.Tasks ?? string.Empty;
        }

        if (_selectedInspection is { } inspection)
        {
            _inspection.Text = inspection.Id;
            _result.Text = inspection.Result;
            _findings.Text = inspection.Findings ?? string.Empty;
        }

        _failure.Text = string.Empty;

        ShowState(workspace.Plans.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void SelectPlan()
    {
        if (_planGrid.SelectedItem is not OgMaintenancePlan plan)
        {
            return;
        }

        _selectedPlan = plan;
        _plan.Text = plan.Id;
        _frequency.Text = plan.Frequency?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _unit.SelectedItem = plan.Unit;
        _lastDone.Text = plan.LastDone?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _tasks.Text = plan.Tasks ?? string.Empty;
    }

    private void SelectInspection()
    {
        if (_inspectionGrid.SelectedItem is not OgInspectionRow inspection)
        {
            return;
        }

        _selectedInspection = inspection;
        _inspection.Text = inspection.Id;
        _result.Text = inspection.Result;
        _findings.Text = inspection.Findings ?? string.Empty;
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
