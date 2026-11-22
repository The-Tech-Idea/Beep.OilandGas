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
/// HSE-05: the compliance calendar and obligation register.
/// </summary>
/// <remarks>
/// Due dates come from the records and are never interpreted (AC-1); a missing due date reads "unscheduled" and is never
/// overdue (AC-2); the jurisdiction is explicit and never defaulted (AC-3); an obligation without components cannot be
/// fulfilled (AC-4); the report process states that no regulator delivery is established (AC-5); and the calendar and the list
/// count the same rows (AC-6).
/// </remarks>
public sealed class CompliancePageView : OgPageBase
{
    public const string Route = "compliance";

    private readonly OgApplication _application;
    private readonly IOgCompliance _compliance;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _counts;
    private readonly TextBlock _reportProcess;
    private readonly TextBlock _failure;
    private readonly TextBlock _selected;
    private readonly TextBox _filterField;
    private readonly TextBox _month;
    private readonly TextBox _jurisdiction;
    private readonly TextBox _due;
    private readonly TextBox _description;
    private readonly TextBox _components;
    private readonly TextBox _fulfilDate;
    private readonly TextBox _evidence;
    private readonly TextBox _reason;
    private readonly ComboBox _kind;
    private readonly DataGrid _calendarGrid;
    private readonly DataGrid _obligationGrid;
    private readonly DataGrid _componentGrid;
    private readonly Button _loadButton;
    private readonly Button _createButton;
    private readonly Button _fulfilComponentButton;
    private readonly Button _fulfilButton;
    private readonly Button _waiveButton;
    private readonly Button _reportButton;

    private OgComplianceWorkspace? _workspace;
    private OgObligation? _obligation;
    private OgObligationComponent? _component;

    public CompliancePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _compliance = application.Compliance;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _reportProcess = Body(string.Empty);
        _reportProcess.TextWrapping = TextWrapping.Wrap;
        _reportProcess.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _selected = Body(string.Empty);
        _selected.TextWrapping = TextWrapping.Wrap;
        _selected.Margin = new Thickness(0, 4, 0, 4);

        _filterField = Field("Field", 110);
        _month = Field("Month yyyy-MM", 120);
        _jurisdiction = Field("Jurisdiction", 120);
        _due = Field("Due date yyyy-MM-dd", 140);
        _description = Field("Description", 260);
        _components = Field("Components, comma separated", 280);
        _fulfilDate = Field("Fulfilled date yyyy-MM-dd", 160);
        _evidence = Field("Evidence", 200);
        _reason = Field("Waiver reason", 240);

        _kind = Picker("Obligation type", 220);

        _calendarGrid = new DataGrid
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
                Text("Date", "Date", 110),
                Text("Due", "Due", 80),
                Text("Overdue", "Overdue", 90),
            },
        };

        Named(_calendarGrid, "Compliance calendar");

        _obligationGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 230,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Id", "Id", 190),
                Text("Type", "Kind", 180),
                Text("Jurisdiction", "JurisdictionText", 110),
                Text("Due", "DueText", 110),
                Text("State", "State", 110),
                Text("Components", "Components", 100),
            },
        };

        Named(_obligationGrid, "Obligations");
        _obligationGrid.SelectionChanged += (_, _) => SelectObligation();

        _componentGrid = new DataGrid
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
                Text("#", "ObsNo", 50),
                Text("Name", "Name", 240),
                Text("Required", "Required", 90),
                Text("Fulfilled", "FulfilledDate", 120),
                Text("Evidence", "Evidence", 200),
            },
        };

        Named(_componentGrid, "Components of the selected obligation");
        _componentGrid.SelectionChanged += (_, _) => SelectComponent();

        _loadButton = Button("Load", (_, _) => Load());
        _createButton = Button("Create obligation", (_, _) => _ = CreateAsync());
        _fulfilComponentButton = Button("Fulfil component", (_, _) => _ = FulfilComponentAsync());
        _fulfilButton = Button("Fulfil obligation", (_, _) => _ = FulfilAsync());
        _waiveButton = Button("Waive", (_, _) => _ = WaiveAsync());
        _reportButton = Button("Start report process", (_, _) => _ = ReportAsync());

        var filterBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filterBar.Children.Add(Caption("Field"));
        filterBar.Children.Add(_filterField);
        filterBar.Children.Add(Caption("Month"));
        filterBar.Children.Add(_month);
        filterBar.Children.Add(_loadButton);

        var createBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        createBar.Children.Add(_kind);
        createBar.Children.Add(Caption("Jurisdiction"));
        createBar.Children.Add(_jurisdiction);
        createBar.Children.Add(Caption("Due"));
        createBar.Children.Add(_due);

        var createBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        createBar2.Children.Add(Caption("Description"));
        createBar2.Children.Add(_description);
        createBar2.Children.Add(Caption("Components"));
        createBar2.Children.Add(_components);
        createBar2.Children.Add(_createButton);

        var fulfilBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fulfilBar.Children.Add(Caption("Fulfilled"));
        fulfilBar.Children.Add(_fulfilDate);
        fulfilBar.Children.Add(Caption("Evidence"));
        fulfilBar.Children.Add(_evidence);
        fulfilBar.Children.Add(_fulfilComponentButton);
        fulfilBar.Children.Add(_fulfilButton);

        var waiveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        waiveBar.Children.Add(Caption("Reason"));
        waiveBar.Children.Add(_reason);
        waiveBar.Children.Add(_waiveButton);
        waiveBar.Children.Add(_reportButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_counts);
        panel.Children.Add(_reportProcess);
        panel.Children.Add(_failure);
        panel.Children.Add(filterBar);
        panel.Children.Add(Caption("Calendar"));
        panel.Children.Add(_calendarGrid);
        panel.Children.Add(Caption("Obligations"));
        panel.Children.Add(_obligationGrid);
        panel.Children.Add(Caption("Selected obligation"));
        panel.Children.Add(_selected);
        panel.Children.Add(_componentGrid);
        panel.Children.Add(Caption("Record fulfilment or a waiver"));
        panel.Children.Add(fulfilBar);
        panel.Children.Add(waiveBar);
        panel.Children.Add(Caption("New obligation"));
        panel.Children.Add(createBar);
        panel.Children.Add(createBar2);

        ShowContent(panel);

        var session = application.Session;
        _filterField.Text = session.FieldId ?? string.Empty;
        _month.Text = DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        _due.Text = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _fulfilDate.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        Load();
    }

    public OgComplianceWorkspace? Workspace => _workspace;

    public OgObligation? Obligation => _obligation;

    public OgObligationComponent? Component => _component;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string CountsText => _counts.Text;

    public string ReportProcessText => _reportProcess.Text;

    public string FailureText => _failure.Text;

    public string SelectedText => _selected.Text;

    public bool Load()
    {
        _header.Text = "HSE · compliance";

        var month = Date(_month.Text + "-01") ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var loaded = _compliance
            .LoadAsync(Empty(_filterField.Text), new DateTime(month.Year, month.Month, 1))
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _calendarGrid.ItemsSource = null;
            _obligationGrid.ItemsSource = null;
            _componentGrid.ItemsSource = null;
            _counts.Text = string.Empty;
            _selected.Text = string.Empty;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The compliance register could not be read.";
            ShowState(OgViewState.Failed, "The compliance register could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public Task<bool> CreateAsync() =>
        RunAsync(() => _compliance.CreateAsync(
                new OgObligationInputs(
                    _kind.SelectedItem as string,
                    Empty(_jurisdiction.Text),
                    Empty(_filterField.Text),
                    Date(_due.Text),
                    _description.Text ?? string.Empty,
                    Rows(_components.Text)),
                _application.Session.UserId),
            "compliance.write",
            "Obligation created");

    public Task<bool> FulfilComponentAsync()
    {
        if (_obligation is not { } obligation)
        {
            _failure.Text = "Select the obligation first.";
            return Task.FromResult(false);
        }

        if (_component is not { } component)
        {
            _failure.Text = "Select the component to fulfil.";
            return Task.FromResult(false);
        }

        var date = Date(_fulfilDate.Text);

        if (date is null)
        {
            _failure.Text = "A fulfilment needs its date.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _compliance.FulfilComponentAsync(
                obligation.Id,
                obligation.SeqNo,
                component.ObsNo,
                date.Value,
                Empty(_evidence.Text),
                _application.Session.UserId),
            "compliance.write",
            "Component fulfilled");
    }

    public Task<bool> FulfilAsync()
    {
        if (_obligation is not { } obligation)
        {
            _failure.Text = "Select the obligation to fulfil.";
            return Task.FromResult(false);
        }

        var date = Date(_fulfilDate.Text);

        if (date is null)
        {
            _failure.Text = "A fulfilment needs its date.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _compliance.FulfilAsync(
                obligation.Id, obligation.SeqNo, date.Value, _application.Session.UserId),
            "compliance.write",
            "Obligation fulfilled");
    }

    public Task<bool> WaiveAsync()
    {
        if (_obligation is not { } obligation)
        {
            _failure.Text = "Select the obligation to waive.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _compliance.WaiveAsync(
                obligation.Id, obligation.SeqNo, _reason.Text ?? string.Empty, _application.Session.UserId),
            "compliance.write",
            "Obligation waived");
    }

    public Task<bool> ReportAsync()
    {
        if (_obligation is not { } obligation)
        {
            _failure.Text = "Select the obligation whose report process is started.";
            return Task.FromResult(false);
        }

        return RunAsync(() => _compliance.RequestReportAsync(
                obligation.Id, obligation.SeqNo, _application.Session.UserId),
            "compliance.report",
            "Report process requested");
    }

    private async Task<bool> RunAsync(
        Func<Task<OgResult<OgComplianceWorkspace>>> action,
        string auditAction,
        string done)
    {
        var outcome = await action().ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The compliance command was refused.";
            ShowState(OgViewState.Failed, "The compliance command was refused.", outcome.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;

        // Re-read through the page's own filter and month, so a command does not silently widen the register.
        Apply(outcome.Value);
        Load();

        _application.AuditLog.Add(auditAction, "hse", outcome.Value.FieldId ?? "(compliance)", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = done + ".";

        return true;
    }

    private void Apply(OgComplianceWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText + " · calendar due " +
                       workspace.CalendarDue.ToString(CultureInfo.InvariantCulture) + " · calendar overdue " +
                       workspace.CalendarOverdue.ToString(CultureInfo.InvariantCulture);

        // AC-5: the sentence is shown whatever else the page says.
        _reportProcess.Text = OgComplianceWorkspace.ReportProcessText;

        _kind.ItemsSource = workspace.Kinds.ToList();
        _kind.SelectedIndex = _kind.SelectedIndex < 0 ? 0 : _kind.SelectedIndex;

        _calendarGrid.ItemsSource = workspace.Calendar.ToList();
        _obligationGrid.ItemsSource = workspace.Obligations.ToList();

        _obligation = workspace.Obligations.FirstOrDefault();
        _obligationGrid.SelectedItem = _obligation;

        ShowComponents();

        _notice.Text = workspace.Obligations.Count.ToString(CultureInfo.InvariantCulture) + " obligation(s) for " +
                       workspace.Month.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        _failure.Text = string.Empty;

        RefreshEnabled();
        ShowState(OgViewState.Ready, _notice.Text);
    }

    private void ShowComponents()
    {
        var components = _obligation?.Components ?? [];

        _componentGrid.ItemsSource = components.ToList();
        _component = components.FirstOrDefault();
        _componentGrid.SelectedItem = _component;

        _selected.Text = _obligation is { } obligation
            ? obligation.Describe() + Environment.NewLine +
              "Due: " + obligation.DueText + " · jurisdiction " + obligation.JurisdictionText +
              " · " + obligation.Components.Count.ToString(CultureInfo.InvariantCulture) + " component(s)"
            : "no obligation is selected";
    }

    private void SelectObligation()
    {
        if (_obligationGrid.SelectedItem is not OgObligation selected)
        {
            return;
        }

        _obligation = selected;
        ShowComponents();
        RefreshEnabled();
    }

    private void SelectComponent()
    {
        if (_componentGrid.SelectedItem is not OgObligationComponent selected)
        {
            return;
        }

        _component = selected;
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var loaded = _workspace is not null;

        _createButton.IsEnabled = loaded;
        _fulfilComponentButton.IsEnabled = loaded && _obligation is not null && _component is not null;
        _fulfilButton.IsEnabled = loaded && _obligation is { Components.Count: > 0 };
        _waiveButton.IsEnabled = loaded && _obligation is not null;
        _reportButton.IsEnabled = loaded && _obligation is not null;
    }

    private static IReadOnlyList<string> Rows(string? text) =>
        (text ?? string.Empty)
            .Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

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
