using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Beep.OilandGas.Models.Data.WorkOrder;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WRK-01: every maintenance and service job in one place - by queue and by date.
/// </summary>
/// <remarks>
/// Each queue is a filter over the same orders the grid lists, so its count is that grid's total (AC-1); a resource conflict is
/// stated with both orders (AC-2); overdue is computed from the planned end (AC-3); the calendar and the grid count the same
/// work (AC-4); a failed read shows no queue at all rather than an empty one (AC-5); and unassigned work is distinguishable from
/// assigned work (AC-6).
/// </remarks>
public sealed class WorkOrderDashboardPageView : OgPageBase
{
    public const string Route = "work";

    private readonly OgApplication _application;
    private readonly IOgWorkOrders _work;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _refreshed;
    private readonly TextBlock _notice;
    private readonly TextBlock _conflicts;
    private readonly TextBlock _calendar;
    private readonly TextBlock _failure;
    private readonly TextBox _field;
    private readonly ComboBox _state;
    private readonly TextBox _kind;
    private readonly DataGrid _grid;
    private readonly Button _loadButton;

    private OgWorkOrderWorkspace? _workspace;

    public WorkOrderDashboardPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _work = application.WorkOrders;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 0, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _conflicts = Body(string.Empty);
        _conflicts.TextWrapping = TextWrapping.Wrap;
        _conflicts.Margin = new Thickness(0, 4, 0, 4);

        _calendar = Body(string.Empty);
        _calendar.TextWrapping = TextWrapping.Wrap;
        _calendar.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _field = Field("Field", 120);
        _state = Picker("State", 160);
        _kind = Field("Work-order type", 200);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 140,
            MaxHeight = 240,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Number", "Number", 140),
                Text("Type", "Kind", 160),
                Text("Asset", "AssetId", 120),
                Text("Urgency", "Priority", 90),
                Text("State", "StateText", 130),
                Text("Assignee", "Assignee", 110),
                Text("Due", "DueText", 150),
            },
        };

        Named(_grid, "Work orders");

        _loadButton = Button("Refresh", (_, _) => Load());

        var filterBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filterBar.Children.Add(Caption("Field"));
        filterBar.Children.Add(_field);
        filterBar.Children.Add(Caption("State"));
        filterBar.Children.Add(_state);
        filterBar.Children.Add(Caption("Type"));
        filterBar.Children.Add(_kind);
        filterBar.Children.Add(_loadButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(_notice);
        panel.Children.Add(filterBar);
        panel.Children.Add(_grid);
        panel.Children.Add(_failure);
        panel.Children.Add(Caption("Resource conflicts"));
        panel.Children.Add(_conflicts);
        panel.Children.Add(Caption("Calendar"));
        panel.Children.Add(_calendar);

        ShowContent(panel);

        var session = application.Session;
        _field.Text = session.FieldId ?? string.Empty;

        // The state list is the owner's own vocabulary, not a second one written here.
        _state.ItemsSource = new[]
        {
            "(any)",
            WorkOrderState.Draft,
            WorkOrderState.Scoped,
            WorkOrderState.Planned,
            WorkOrderState.InProgress,
            WorkOrderState.UnderReview,
            WorkOrderState.Completed,
            WorkOrderState.Cancelled,
        };

        _state.SelectedIndex = 0;
        _kind.Text = string.Empty;

        Load();
    }

    public OgWorkOrderWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string RefreshText => _refreshed.Text;

    public string NoticeText => _notice.Text;

    public string ConflictText => _conflicts.Text;

    public string CalendarText => _calendar.Text;

    public string FailureText => _failure.Text;

    public bool Load()
    {
        _header.Text = "Work · work orders";

        var session = _application.Session;
        var state = _state.SelectedItem as string;

        if (string.Equals(state, "(any)", StringComparison.Ordinal))
        {
            state = null;
        }

        var loaded = _work.LoadAsync(
            Empty(_field.Text),
            state,
            Empty(_kind.Text),
            session.UserId,
            DateTime.Today).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;

            // AC-5: a failed read shows no queue at all, because an empty queue and an unread one are different facts.
            _counts.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _conflicts.Text = string.Empty;
            _calendar.Text = string.Empty;
            _grid.ItemsSource = null;
            _notice.Text = string.Empty;
            _failure.Text = loaded.Message ?? "The work-order dashboard could not be read.";

            ShowState(OgViewState.Failed, "The work-order dashboard is not shown.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    private void Apply(OgWorkOrderWorkspace workspace)
    {
        _workspace = workspace;

        _counts.Text = workspace.CountsText;
        _refreshed.Text = workspace.RefreshText;
        _conflicts.Text = workspace.ConflictText;
        _calendar.Text = workspace.CalendarText;
        _grid.ItemsSource = workspace.Orders.ToList();

        // AC-1 and AC-4: the notice states the two totals the page is built from, so they are visible side by side.
        _notice.Text = workspace.Total.ToString(CultureInfo.InvariantCulture) + " order(s) in the grid · " +
                       workspace.CalendarOrders.ToString(CultureInfo.InvariantCulture) + " order(s) on the calendar" +
                       (workspace.Total == workspace.CalendarOrders
                           ? " (the same work)"
                           : " (orders without a planned start appear on the grid only)");

        _failure.Text = string.Empty;

        ShowState(workspace.IsEmpty ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
