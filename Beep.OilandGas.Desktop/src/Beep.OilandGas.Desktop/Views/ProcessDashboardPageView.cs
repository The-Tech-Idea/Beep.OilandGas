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
/// WFL-01: whether the organisation's processes are moving - throughput, ageing, SLA breaches and the bottleneck.
/// </summary>
/// <remarks>
/// Throughput is counted from the history record (AC-1); "no SLA" is its own state, never "met" (AC-2); at risk and breached
/// follow the configured SLA with unscheduled handling (AC-3); the bottleneck names the step and its median dwell (AC-4); a
/// failed read shows no figure at all (AC-5); and the per-definition counts add up to the instances listed (AC-6).
/// </remarks>
public sealed class ProcessDashboardPageView : OgPageBase
{
    public const string Route = "processes";

    private readonly OgApplication _application;
    private readonly IOgProcessDashboard _dashboard;

    private readonly TextBlock _header;
    private readonly TextBlock _kpis;
    private readonly TextBlock _states;
    private readonly TextBlock _bottleneck;
    private readonly TextBlock _slaRule;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _from;
    private readonly TextBox _to;
    private readonly DataGrid _definitionGrid;
    private readonly DataGrid _ageingGrid;
    private readonly Button _loadButton;

    private OgProcessDashboard? _snapshot;

    public ProcessDashboardPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _dashboard = application.ProcessDashboard;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _kpis = Body(string.Empty);
        _kpis.TextWrapping = TextWrapping.Wrap;
        _kpis.Margin = new Thickness(0, 4, 0, 4);

        _states = Body(string.Empty);
        _states.TextWrapping = TextWrapping.Wrap;
        _states.Margin = new Thickness(0, 0, 0, 4);

        _bottleneck = Body(string.Empty);
        _bottleneck.TextWrapping = TextWrapping.Wrap;
        _bottleneck.Margin = new Thickness(0, 0, 0, 4);

        _slaRule = Body(string.Empty);
        _slaRule.TextWrapping = TextWrapping.Wrap;
        _slaRule.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 120);
        _from = Field("From yyyy-MM-dd", 130);
        _to = Field("To yyyy-MM-dd", 130);

        _definitionGrid = Grid("Definitions", ("Definition", "Name", 200), ("Version", "Version", 90),
            ("Type", "Type", 150), ("Moved", "Moved", 80), ("Instances", "Instances", 90),
            ("Running", "Running", 80), ("Completed", "Completed", 90));

        _ageingGrid = Grid("Ageing", ("Instance", "InstanceId", 200), ("Definition", "Definition", 170),
            ("Current step", "CurrentStep", 160), ("In step", "DaysText", 130), ("SLA", "SlaState", 110));

        _loadButton = Button("Refresh", (_, _) => Load());

        var loadBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        loadBar.Children.Add(Caption("Field"));
        loadBar.Children.Add(_field);
        loadBar.Children.Add(Caption("From"));
        loadBar.Children.Add(_from);
        loadBar.Children.Add(Caption("To"));
        loadBar.Children.Add(_to);
        loadBar.Children.Add(_loadButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_kpis);
        panel.Children.Add(_states);
        panel.Children.Add(loadBar);
        panel.Children.Add(Caption("Throughput by definition"));
        panel.Children.Add(_definitionGrid);
        panel.Children.Add(Caption("Ageing"));
        panel.Children.Add(_ageingGrid);
        panel.Children.Add(Caption("Bottleneck"));
        panel.Children.Add(_bottleneck);
        panel.Children.Add(Caption("SLA rule"));
        panel.Children.Add(_slaRule);
        panel.Children.Add(_failure);

        ShowContent(panel);

        _field.Text = application.Session.FieldId ?? string.Empty;
        _from.Text = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _to.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        Load();
    }

    public OgProcessDashboard? Snapshot => _snapshot;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string KpiText => _kpis.Text;

    public string StatesText => _states.Text;

    public string BottleneckText => _bottleneck.Text;

    public string SlaRuleText => _slaRule.Text;

    public string FailureText => _failure.Text;

    public string RefreshText => _refreshed.Text;

    public bool Load()
    {
        _header.Text = "Processes · dashboard";

        var from = Date(_from.Text) ?? DateTime.Today.AddDays(-30);
        var to = Date(_to.Text) ?? DateTime.Today;

        var loaded = _dashboard
            .LoadAsync(Empty(_field.Text), from, to)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _snapshot = null;

            // AC-5: a failed read shows no figure at all - there is nothing here a zero could stand for.
            _kpis.Text = string.Empty;
            _states.Text = string.Empty;
            _bottleneck.Text = string.Empty;
            _slaRule.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _definitionGrid.ItemsSource = null;
            _ageingGrid.ItemsSource = null;
            _failure.Text = loaded.Message ?? "The process dashboard could not be read.";
            ShowState(OgViewState.Failed, "The process dashboard is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    private void Apply(OgProcessDashboard dashboard)
    {
        _snapshot = dashboard;

        _kpis.Text = dashboard.KpiText + " · period " + dashboard.PeriodText;
        _states.Text = "SLA states: " + dashboard.StateText;

        _bottleneck.Text = dashboard.BottleneckText;
        _refreshed.Text = dashboard.ReadText;

        // AC-3's own caveat, stated on the page: the owner configures a breach, so at risk comes from half the same SLA.
        _slaRule.Text =
            "A step is breached when its elapsed time exceeds its recorded SLA_HOURS (the owner's own rule). The owner " +
            "configures no separate at-risk threshold, so a step past half of its SLA window is shown as at risk; a step " +
            "with no SLA is 'no SLA' and never 'met'; a step that has not started is 'unscheduled'.";

        _definitionGrid.ItemsSource = dashboard.Definitions.ToList();
        _ageingGrid.ItemsSource = dashboard.Ageing.ToList();

        _failure.Text = string.Empty;

        ShowState(dashboard.IsEmpty ? OgViewState.Empty : OgViewState.Ready, dashboard.KpiText);
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 190,
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
