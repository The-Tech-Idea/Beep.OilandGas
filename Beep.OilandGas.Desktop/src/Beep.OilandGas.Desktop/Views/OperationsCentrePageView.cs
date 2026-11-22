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
/// PRD-01: the daily operations centre - today's volumes, what is down, what changed and what needs a decision.
/// </summary>
/// <remarks>
/// A failed read renders the failure panel, never a page of zero cards (AC-1). "Not yet reported" is a state of its own
/// and is never drawn as zero (AC-2). Every volume carries its unit and a row in another unit is named, never converted
/// (AC-3). The completeness indicator counts the field's active wells that did not report (AC-4). Top movers are hidden
/// with a reason until seven days of history exist (AC-5). Deferment (PRD-03) and allocation (PRD-04) are named as not
/// yet available rather than shown as zero.
/// </remarks>
public sealed class OperationsCentrePageView : OgPageBase
{
    public const string Route = "operations";

    private readonly OgApplication _application;
    private readonly IOgOperationsCentre _centre;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _kpis;
    private readonly TextBlock _strip;
    private readonly TextBlock _completeness;
    private readonly TextBlock _movers;
    private readonly TextBlock _detail;
    private readonly TextBox _day;
    private readonly DataGrid _wells;
    private readonly DataGrid _alerts;
    private readonly DataGrid _moverGrid;

    public OperationsCentrePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _centre = application.OperationsCentre;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _kpis = Body(string.Empty);
        _kpis.TextWrapping = TextWrapping.Wrap;
        _kpis.Margin = new Thickness(0, 6, 0, 6);

        _strip = Body(string.Empty);
        _strip.TextWrapping = TextWrapping.Wrap;
        _strip.Margin = new Thickness(0, 0, 0, 6);

        _completeness = Body(string.Empty);
        _completeness.TextWrapping = TextWrapping.Wrap;
        _completeness.Margin = new Thickness(0, 0, 0, 8);

        _movers = Body(string.Empty);
        _movers.TextWrapping = TextWrapping.Wrap;
        _movers.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _day = Field("Day yyyy-MM-dd", 120);
        _day.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        _wells = new DataGrid
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
                Text("Well", "WellId", 140),
                Text("Name", "NameText", 140),
                Text("Oil", "OilText", 140),
                Text("Gas", "GasText", 140),
                Text("Water", "WaterText", 140),
            },
        };

        Named(_wells, "Wells reporting for the day");

        _alerts = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "WellId", 140),
                Text("Name", "Name", 140),
                Text("Alert", "Message", 320),
            },
        };

        Named(_alerts, "Wells with no measurement");

        _moverGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Well", "WellId", 140),
                Text("Name", "Name", 140),
                Text("Today", "TodayText", 130),
                Text("Average", "AverageText", 130),
                Text("Change", "ChangeText", 130),
            },
        };

        Named(_moverGrid, "Top movers");

        var top = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        top.Children.Add(Caption("Day"));
        top.Children.Add(_day);
        top.Children.Add(Button("Refresh", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(top);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Key figures"));
        panel.Children.Add(_kpis);
        panel.Children.Add(_completeness);
        panel.Children.Add(Caption("Last 30 days"));
        panel.Children.Add(_strip);
        panel.Children.Add(Caption("Wells"));
        panel.Children.Add(_wells);
        panel.Children.Add(Caption("No measurement"));
        panel.Children.Add(_alerts);
        panel.Children.Add(Caption("Top movers"));
        panel.Children.Add(_movers);
        panel.Children.Add(_moverGrid);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgOperationsCentre? Centre { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string KpiText => _kpis.Text;

    public string StripText => _strip.Text;

    public string CompletenessText => _completeness.Text;

    public string MoversText => _movers.Text;

    public string DetailText => _detail.Text;

    public bool MoversGridVisible => _moverGrid.Visibility == Visibility.Visible;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _centre
            .LoadAsync(session.FieldId, session.FieldName, Date(_day.Text))
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            // AC-1: a failed read is the failure panel, never zero cards.
            LastFailed = true;
            Centre = null;
            _wells.ItemsSource = null;
            _alerts.ItemsSource = null;
            _moverGrid.ItemsSource = null;
            _header.Text = "Operations centre";
            _kpis.Text = string.Empty;
            _strip.Text = string.Empty;
            _completeness.Text = string.Empty;
            _movers.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The operations centre could not be read.";
            ShowState(OgViewState.Failed, "The operations centre could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Centre = loaded.Value;

        var centre = loaded.Value;

        _wells.ItemsSource = centre.Wells.ToList();
        _alerts.ItemsSource = centre.Alerts.ToList();
        _moverGrid.ItemsSource = centre.Gainers.Concat(centre.Decliners).ToList();
        _moverGrid.Visibility = centre.TopMoversAvailable ? Visibility.Visible : Visibility.Collapsed;

        _header.Text =
            $"{centre.FieldName} · {centre.DayStateText} · {centre.WellsText}";

        _notice.Text = centre.IsReported
            ? centre.MissingWellCount == 0
                ? "Every active well reported for the day."
                : $"{centre.MissingWellCount.ToString(CultureInfo.InvariantCulture)} well(s) did not report; " +
                  "they are listed rather than counted as zero."
            : centre.DayStateText + ".";

        _kpis.Text =
            $"Oil {centre.OilText} · Gas {centre.GasText} · Water {centre.WaterText} · " +
            $"water cut {centre.WaterCutText} · deferred {centre.DeferredText} · " +
            $"allocation {centre.AllocationText}";

        _strip.Text = centre.StripText;

        _completeness.Text = centre.CompletenessText;

        _movers.Text = (centre.TopMoversAvailable ? "Shown: " : string.Empty) + centre.TopMoversReason;

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                centre.Today.Describe(),
                centre.DowntimeText,
                centre.NotComparableText,
                centre.ExcludedText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));

        ShowState(centre.IsReported ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
        return true;
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
