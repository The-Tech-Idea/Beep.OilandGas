using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DEV-06: running an approved drilling program - the execution header, daily reports with their activity breakdown,
/// the reporting responsibility and the time-depth points.
/// </summary>
/// <remarks>
/// A report is one day and one shift, and a second report or a second submission for the same period is refused (AC-1).
/// A submitted report is read-only; a correction is a new revision that carries its reason (AC-2). A submit that is not
/// acknowledged leaves the report a draft and is retained for retry (AC-3). A responsibility transfer records the actor,
/// the receiver and the reason, and the previous holder can no longer write (AC-4). An NPT line needs a code from the
/// reference set, and with no codes recorded the page says so instead of accepting free text (AC-5). A start depth that
/// does not continue from the previous end depth is flagged, not adjusted (AC-6).
/// </remarks>
public sealed class DrillingExecutionPageView : OgPageBase
{
    public const string Route = "drillingexecution";

    private readonly OgApplication _application;
    private readonly IOgDrillingExecution _execution;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _responsibility;
    private readonly TextBlock _timeDepth;
    private readonly TextBlock _detail;
    private readonly ComboBox _programPicker;
    private readonly ComboBox _executionPicker;
    private readonly DataGrid _reports;

    private readonly TextBox _date;
    private readonly ComboBox _shift;
    private readonly TextBox _startDepth;
    private readonly TextBox _endDepth;
    private readonly TextBox _depthUnit;
    private readonly TextBox _cost;
    private readonly TextBox _currency;
    private readonly TextBox _hse;
    private readonly TextBox _personnel;
    private readonly TextBox _equipment;

    private readonly TextBox _activityCode;
    private readonly TextBox _activityHours;
    private readonly TextBox _activityDescription;
    private readonly CheckBox _isNpt;
    private readonly ComboBox _nptCode;

    private readonly TextBox _correctionReason;
    private readonly TextBox _transferTo;
    private readonly TextBox _transferRole;
    private readonly TextBox _transferReason;

    public DrillingExecutionPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _execution = application.DrillingExecution;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _responsibility = Body(string.Empty);
        _responsibility.TextWrapping = TextWrapping.Wrap;
        _responsibility.Margin = new Thickness(0, 4, 0, 4);

        _timeDepth = Body(string.Empty);
        _timeDepth.TextWrapping = TextWrapping.Wrap;
        _timeDepth.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _programPicker = new ComboBox { MinWidth = 260, Margin = new Thickness(0, 0, 8, 0) };
        Named(_programPicker, "Approved program");

        _executionPicker = new ComboBox { MinWidth = 240, Margin = new Thickness(0, 0, 8, 0) };
        Named(_executionPicker, "Drilling execution");
        _executionPicker.SelectionChanged += (_, _) => ShowSelectedExecution();

        _reports = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Period", "ShiftText", 130),
                Text("Rev", "Revision", 50),
                Text("State", "StateText", 90),
                Text("Depth", "DepthText", 150),
                Text("Metres", "MetresText", 100),
                Text("Cost", "CostText", 140),
                Text("NPT", "NptHours", 70),
                Text("Lines", "ActivityCount", 60),
                Text("Depth note", "DepthFlag", 260),
            },
        };

        Named(_reports, "Daily reports");
        _reports.SelectionChanged += (_, _) => ShowSelectedReport();

        _date = Field("Report date yyyy-MM-dd", 110);
        _shift = new ComboBox
        {
            MinWidth = 90,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OgDrillingExecutionService.Shifts,
            SelectedIndex = 0,
        };

        Named(_shift, "Shift");

        _startDepth = Field("Depth at start", 90);
        _endDepth = Field("Depth at end", 90);
        _depthUnit = Field("Depth unit (m/ft)", 90);
        _cost = Field("Cost", 90);
        _currency = Field("Currency", 70);
        _hse = Field("HSE events", 180);
        _personnel = Field("Personnel", 160);
        _equipment = Field("Equipment", 160);

        _activityCode = Field("Activity code (IADC)", 120);
        _activityHours = Field("Hours", 70);
        _activityDescription = Field("Description", 180);
        _isNpt = new CheckBox { Content = "NPT", Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
        Named(_isNpt, "Non-productive time");

        _nptCode = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        Named(_nptCode, "NPT reason code");

        _correctionReason = Field("Correction reason", 200);
        _transferTo = Field("Transfer to", 120);
        _transferRole = Field("Role", 100);
        _transferReason = Field("Transfer reason", 180);

        var start = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        start.Children.Add(Caption("Approved program"));
        start.Children.Add(_programPicker);
        start.Children.Add(Button("Start execution", (_, _) => _ = StartAsync()));
        start.Children.Add(Button("Refresh", (_, _) => Load()));

        var report = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        report.Children.Add(Caption("Report"));
        report.Children.Add(_date);
        report.Children.Add(_shift);
        report.Children.Add(Caption("Start"));
        report.Children.Add(_startDepth);
        report.Children.Add(Caption("End"));
        report.Children.Add(_endDepth);
        report.Children.Add(_depthUnit);
        report.Children.Add(Caption("Cost"));
        report.Children.Add(_cost);
        report.Children.Add(_currency);
        report.Children.Add(_hse);
        report.Children.Add(_personnel);
        report.Children.Add(_equipment);
        report.Children.Add(Button("Create report", (_, _) => _ = CreateReportAsync()));
        report.Children.Add(Button("Save report", (_, _) => _ = SaveReportAsync()));
        report.Children.Add(Button("Submit", (_, _) => _ = SubmitAsync()));
        report.Children.Add(Button("Retry submit", (_, _) => _ = RetryAsync()));

        var activity = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        activity.Children.Add(Caption("Activity"));
        activity.Children.Add(_activityCode);
        activity.Children.Add(_activityHours);
        activity.Children.Add(_activityDescription);
        activity.Children.Add(_isNpt);
        activity.Children.Add(_nptCode);
        activity.Children.Add(Button("Add line", (_, _) => _ = AddActivityAsync()));

        var correction = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        correction.Children.Add(Caption("Correction"));
        correction.Children.Add(_correctionReason);
        correction.Children.Add(Button("Correct report", (_, _) => _ = CorrectAsync()));

        var transfer = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        transfer.Children.Add(Caption("Responsibility"));
        transfer.Children.Add(_transferTo);
        transfer.Children.Add(_transferRole);
        transfer.Children.Add(_transferReason);
        transfer.Children.Add(Button("Transfer", (_, _) => _ = TransferAsync()));

        var panel = new StackPanel();
        panel.Children.Add(start);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_executionPicker);
        panel.Children.Add(_responsibility);
        panel.Children.Add(Caption("Daily reports"));
        panel.Children.Add(_reports);
        panel.Children.Add(report);
        panel.Children.Add(activity);
        panel.Children.Add(correction);
        panel.Children.Add(transfer);
        panel.Children.Add(Caption("Time-depth"));
        panel.Children.Add(_timeDepth);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgExecutionWorkspace? Workspace { get; private set; }

    public OgExecutionHeader? Selected { get; private set; }

    public OgDailyReport? SelectedReport { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string ResponsibilityText => _responsibility.Text;

    public string TimeDepthText => _timeDepth.Text;

    public string DetailText => _detail.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _execution
            .LoadAsync(session.FieldId, session.FieldName, Selected?.Id)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            Selected = null;
            SelectedReport = null;
            _reports.ItemsSource = null;
            _executionPicker.ItemsSource = null;
            _programPicker.ItemsSource = null;
            _header.Text = "Drilling execution";
            _responsibility.Text = string.Empty;
            _timeDepth.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The drilling execution could not be read.";
            ShowState(OgViewState.Failed, "The drilling execution could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Workspace = loaded.Value;

        _programPicker.ItemsSource = Workspace.StartablePrograms.ToList();
        _programPicker.SelectedIndex = Workspace.StartablePrograms.Count > 0 ? 0 : -1;

        _executionPicker.ItemsSource = Workspace.Executions.ToList();
        _executionPicker.SelectedIndex = Workspace.Selected is null
            ? -1
            : Workspace.Executions.ToList().FindIndex(header => header.Id == Workspace.Selected.Id);

        _reports.ItemsSource = Workspace.Reports.ToList();

        _nptCode.ItemsSource = Workspace.NptCodes.ToList();

        _header.Text = Workspace.Selected is { } selected
            ? $"{selected.WellId} · program v{selected.ProgramVersion.ToString(CultureInfo.InvariantCulture)} · " +
              $"{selected.Status.ToLowerInvariant()} · depth {selected.DepthText} · cost {selected.CostText} · " +
              $"{selected.DaysOnWell?.ToString(CultureInfo.InvariantCulture) ?? "-"} day(s)"
            : "Drilling execution";

        _notice.Text = !Workspace.IsEmpty
            ? Workspace.NptReferenceMissing
                ? "No NPT reason code is recorded in the reference table, so a non-productive time line cannot be accepted."
                : "A submitted report is read-only; a correction is a new revision with a reason."
            : Workspace.StartablePrograms.Count == 0
                ? "No approved drilling program is available to run."
                : "No execution is running. Choose an approved program and start it.";

        _responsibility.Text = Workspace.Selected is { } header
            ? "Reporting responsibility: " + header.ResponsibilityText
            : string.Empty;

        _timeDepth.Text = Workspace.Selected is { } depth ? depth.TimeDepthText : string.Empty;

        ShowState(Workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> StartAsync()
    {
        if (_programPicker.SelectedItem is not OgExecutionProgramOption program)
        {
            _notice.Text = "Choose an approved program.";
            return false;
        }

        var started = await _execution
            .StartAsync(
                _application.Session.CompanyId,
                program.Id,
                DateTime.Today,
                _application.Session.UserId,
                "COMPANY_MAN",
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(started, "development.drillingexecution.start", program.Id);
    }

    public async Task<bool> CreateReportAsync()
    {
        if (Selected is not { } execution)
        {
            _notice.Text = "Start or select an execution first.";
            return false;
        }

        var created = await _execution
            .CreateReportAsync(execution.Id, ReadValues(), _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(created, "development.drillingexecution.report.create", execution.Id);
    }

    public async Task<bool> SaveReportAsync()
    {
        if (SelectedReport is not { } report)
        {
            _notice.Text = "Select a report first.";
            return false;
        }

        var saved = await _execution
            .SaveReportAsync(report.Id, ReadValues(), _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(saved, "development.drillingexecution.report.save", report.Id);
    }

    public async Task<bool> AddActivityAsync()
    {
        if (SelectedReport is not { } report)
        {
            _notice.Text = "Select a report first.";
            return false;
        }

        var added = await _execution
            .AddActivityAsync(
                report.Id,
                new OgDailyActivityValues(
                    _activityCode.Text,
                    decimal.TryParse(_activityHours.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out var hours)
                        ? hours
                        : null,
                    _activityDescription.Text,
                    _isNpt.IsChecked == true,
                    _nptCode.SelectedItem as string,
                    report.StartDepth),
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(added, "development.drillingexecution.activity", report.Id);
    }

    public async Task<bool> SubmitAsync()
    {
        if (SelectedReport is not { } report)
        {
            _notice.Text = "Select a report first.";
            return false;
        }

        var submitted = await _execution
            .SubmitReportAsync(report.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(submitted, "development.drillingexecution.submit", report.Id);
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _execution.RetrySubmitAsync().ConfigureAwait(true);
        return AfterWrite(retried, "development.drillingexecution.submit", "retry");
    }

    public async Task<bool> CorrectAsync()
    {
        if (SelectedReport is not { } report)
        {
            _notice.Text = "Select a report first.";
            return false;
        }

        var corrected = await _execution
            .ReviseReportAsync(report.Id, _correctionReason.Text, _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(corrected, "development.drillingexecution.correct", report.Id);
    }

    public async Task<bool> TransferAsync()
    {
        if (Selected is not { } execution)
        {
            _notice.Text = "Start or select an execution first.";
            return false;
        }

        var transferred = await _execution
            .TransferResponsibilityAsync(
                execution.Id,
                _transferTo.Text,
                _transferRole.Text,
                _transferReason.Text,
                _application.Session.UserId)
            .ConfigureAwait(true);

        return AfterWrite(transferred, "development.drillingexecution.transfer", execution.Id);
    }

    private OgDailyReportValues ReadValues() =>
        new(
            Date(_date.Text),
            _shift.SelectedItem as string,
            null,
            null,
            decimal.TryParse(_startDepth.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var start)
                ? start
                : null,
            decimal.TryParse(_endDepth.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var end)
                ? end
                : null,
            _depthUnit.Text,
            decimal.TryParse(_cost.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var cost)
                ? cost
                : null,
            _currency.Text,
            _hse.Text,
            _personnel.Text,
            _equipment.Text);

    private bool AfterWrite(OgResult<OgExecutionWorkspace> result, string verb, string subject)
    {
        _notice.Text = result.Success
            ? result.Message ?? "Saved."
            : result.Message ?? "The change was not applied.";

        if (result.Success)
        {
            _application.AuditLog.Add(verb, "development", subject, "ok", _application.Session.UserId);
        }

        var reportId = SelectedReport?.Id;
        Load();

        if (reportId is not null)
        {
            var index = (Workspace?.Reports ?? []).ToList().FindIndex(report => report.Id == reportId);

            if (index >= 0)
            {
                _reports.SelectedIndex = index;
            }
        }

        return result.Success;
    }

    private void ShowSelectedExecution()
    {
        if (_executionPicker.SelectedItem is OgExecutionHeader header)
        {
            Selected = header;
            _detail.Text = header.DepthText == "no depth reported"
                ? "No report is recorded for this execution yet."
                : $"Current depth {header.DepthText} · current activity {header.CurrentActivity ?? "(none)"} · " +
                  $"cost to date {header.CostText}";
        }
    }

    private void ShowSelectedReport()
    {
        if (_reports.SelectedItem is not OgDailyReport report)
        {
            return;
        }

        SelectedReport = report;

        _date.Text = report.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        _shift.SelectedItem = report.Shift;
        _startDepth.Text = report.StartDepth.ToString(CultureInfo.InvariantCulture);
        _endDepth.Text = report.EndDepth.ToString(CultureInfo.InvariantCulture);
        _depthUnit.Text = report.DepthUnit ?? string.Empty;
        _cost.Text = report.Cost?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _currency.Text = report.Currency ?? string.Empty;
        _hse.Text = report.HseEvents ?? string.Empty;
        _personnel.Text = report.Personnel ?? string.Empty;
        _equipment.Text = report.Equipment ?? string.Empty;

        var lines = new List<string> { report.Describe() };

        if (report.Activities.Count > 0)
        {
            lines.Add("Activity:");
            lines.AddRange(report.Activities.Select(activity => "  " + activity.Describe()));
        }

        if (report.CorrectsReportId is not null)
        {
            lines.Add($"Corrects {report.CorrectsReportId}: {report.CorrectionReason ?? "no reason recorded"}");
        }

        lines.Add(report.DepthFlagged
            ? $"Depth flag: {report.DepthFlag}"
            : "Depth continues from the previous report.");

        _detail.Text = string.Join(Environment.NewLine, lines);
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
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
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
