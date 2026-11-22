using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// PRD-02: the register of producing fields and pools with their recorded performance.
/// </summary>
/// <remarks>
/// Every figure on this page comes from a record (AC-1) - there is no demo constant anywhere in the feature (AC-5). A
/// failed read is the failure panel and never a page of zeroed figures (AC-2). Aggregates are summed only within one
/// unit per fluid, and a row whose unit is not recorded is excluded and counted (AC-3). The field grid pages, so a
/// register beyond 500 fields is reachable (AC-4).
/// </remarks>
public sealed class FieldPoolRegisterPageView : OgPageBase
{
    public const string Route = "fieldsandpools";

    private readonly OgApplication _application;
    private readonly IOgFieldPoolRegister _register;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _paging;
    private readonly TextBlock _excluded;
    private readonly TextBlock _trend;
    private readonly TextBlock _detail;
    private readonly TextBox _day;
    private readonly DataGrid _fields;
    private readonly DataGrid _pools;
    private readonly Button _previous;
    private readonly Button _next;

    private int _pageIndex;

    public FieldPoolRegisterPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _register = application.FieldPoolRegister;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _paging = Body(string.Empty);
        _paging.TextWrapping = TextWrapping.Wrap;
        _paging.Margin = new Thickness(0, 4, 0, 4);

        _excluded = Body(string.Empty);
        _excluded.TextWrapping = TextWrapping.Wrap;
        _excluded.Margin = new Thickness(0, 0, 0, 8);

        _trend = Body(string.Empty);
        _trend.TextWrapping = TextWrapping.Wrap;
        _trend.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _day = Field("Day yyyy-MM-dd", 120);
        _day.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _fields = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 210,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Field", "NameText", 150),
                Text("Area", "BasinText", 110),
                Text("Discovery", "DiscoveryText", 95),
                Text("First production", "FirstProductionText", 105),
                Text("Status", "StatusText", 110),
                Text("Wells", "WellCount", 60),
                Text("Pools", "PoolCount", 55),
                Text("Oil (30 d)", "OilText", 110),
                Text("Gas (30 d)", "GasText", 110),
                Text("Water (30 d)", "WaterText", 110),
                Text("Water cut (30 d)", "WaterCutText", 100),
                Text("Uptime", "UptimeText", 75),
            },
        };

        Named(_fields, "Fields");
        _fields.SelectionChanged += (_, _) => SelectField();

        _pools = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 180,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Pool", "NameText", 150),
                Text("Reservoir", "ReservoirText", 120),
                Text("Drive", "DriveText", 110),
                Text("Status", "StatusText", 110),
                Text("Wells", "WellCountText", 60),
                Text("Current rate", "RateText", 120),
                Text("Cumulative", "CumulativeText", 120),
            },
        };

        Named(_pools, "Pools of the selected field");
        _pools.SelectionChanged += (_, _) => SelectPool();

        _previous = Button("Previous page", (_, _) => Page(-1));
        _previous.MinWidth = 110;
        _next = Button("Next page", (_, _) => Page(1));
        _next.MinWidth = 110;

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Caption("Day"));
        top.Children.Add(_day);
        top.Children.Add(_previous);
        top.Children.Add(_next);
        top.Children.Add(Button("Refresh", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_paging);
        panel.Children.Add(Caption("Fields"));
        panel.Children.Add(_fields);
        panel.Children.Add(_excluded);
        panel.Children.Add(Caption("Pools"));
        panel.Children.Add(_pools);
        panel.Children.Add(Caption("Trend"));
        panel.Children.Add(_trend);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgFieldRegister? Register { get; private set; }

    public OgFieldSummary? Selected { get; private set; }

    public OgPoolSummary? SelectedPool { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string PagingText => _paging.Text;

    public string ExcludedText => _excluded.Text;

    public string TrendText => _trend.Text;

    public string DetailText => _detail.Text;

    public int PageIndex => _pageIndex;

    public bool PreviousEnabled => _previous.IsEnabled;

    public bool NextEnabled => _next.IsEnabled;

    public void Page(int direction)
    {
        var target = _pageIndex + direction;

        if (Register is { } register && (target < 0 || target >= register.PageCount))
        {
            return;
        }

        _pageIndex = Math.Max(0, target);
        Load();
    }

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _register
            .LoadAsync(
                session.FieldId,
                session.FieldName,
                SelectedPool?.Id,
                _pageIndex,
                FieldRegisterRules.DefaultPageSize,
                Date(_day.Text))
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            // AC-2: a failed read is the failure panel, never zeroed figures.
            LastFailed = true;
            Register = null;
            Selected = null;
            SelectedPool = null;
            _fields.ItemsSource = null;
            _pools.ItemsSource = null;
            _previous.IsEnabled = false;
            _next.IsEnabled = false;
            _header.Text = "Fields and pools";
            _paging.Text = string.Empty;
            _excluded.Text = string.Empty;
            _trend.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The field and pool register could not be read.";
            ShowState(OgViewState.Failed, "The field and pool register could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Register = loaded.Value;
        _pageIndex = loaded.Value.PageIndex;

        var register = loaded.Value;

        Selected = register.Selected;
        SelectedPool = register.SelectedPool;

        _fields.ItemsSource = register.Fields.ToList();
        _pools.ItemsSource = register.Pools.ToList();

        _previous.IsEnabled = register.HasPrevious;
        _next.IsEnabled = register.HasNext;

        _header.Text = register.Selected is { } field
            ? $"{field.NameText} · area {field.BasinText} · {field.StatusText} · " +
              $"{field.OilText} oil · water cut {field.WaterCutText} · uptime {field.UptimeText}"
            : "Fields and pools";

        _notice.Text = register.Selected is { } chosen && chosen.IsPreProduction
            ? $"{chosen.NameText} has not produced yet; its rates are shown as an em dash, not as zero."
            : register.TotalFields == 0
                ? "No field is recorded in this database."
                : register.SelectedPoolsText;

        _paging.Text = register.PagingText;

        _excluded.Text = register.ExcludedUnitRows == 0
            ? "Every volume on this page carries its unit."
            : $"{register.ExcludedUnitRows.ToString(CultureInfo.InvariantCulture)} row(s) on this page were excluded " +
              "from the totals because no unit is recorded.";

        _trend.Text = register.TrendText;

        _detail.Text = register.Selected is { } shown
            ? string.Join(
                Environment.NewLine,
                new[]
                {
                    shown.Describe(),
                    shown.ExcludedText,
                    register.SelectedPool is { } pool
                        ? "Selected pool: " + pool.Describe()
                        : register.SelectedPoolsText,
                    $"{register.Summary} · {register.PagingText}",
                }.Where(part => !string.IsNullOrWhiteSpace(part)))
            : string.Empty;

        ShowState(register.TotalFields == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    private void SelectField()
    {
        if (_fields.SelectedItem is not OgFieldSummary field)
        {
            return;
        }

        Selected = field;
        SelectedPool = null;
        _pageIndex = 0;

        var reloaded = _register
            .LoadAsync(
                field.Id,
                field.NameText,
                null,
                0,
                FieldRegisterRules.DefaultPageSize,
                Date(_day.Text))
            .GetAwaiter()
            .GetResult();

        if (reloaded.Success && reloaded.Value is not null)
        {
            Register = reloaded.Value;
            _pools.ItemsSource = reloaded.Value.Pools.ToList();
            _trend.Text = reloaded.Value.TrendText;
            _detail.Text = reloaded.Value.Selected?.Describe() ?? string.Empty;
        }

        Load();
    }

    private void SelectPool()
    {
        if (_pools.SelectedItem is not OgPoolSummary pool || Selected is not { } field)
        {
            return;
        }

        SelectedPool = pool;

        var reloaded = _register
            .LoadAsync(field.Id, field.NameText, pool.Id, _pageIndex, FieldRegisterRules.DefaultPageSize, Date(_day.Text))
            .GetAwaiter()
            .GetResult();

        if (reloaded.Success && reloaded.Value is not null)
        {
            Register = reloaded.Value;
            _trend.Text = reloaded.Value.TrendText;
            _detail.Text = string.Join(
                Environment.NewLine,
                new[] { field.Describe(), pool.Describe() }.Where(part => !string.IsNullOrWhiteSpace(part)));
        }
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
