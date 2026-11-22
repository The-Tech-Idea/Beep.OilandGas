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
/// DEV-01: the development persona's landing.
/// </summary>
/// <remarks>
/// The KPI cards are counts of records, and a failed read is a failure panel rather than zeroes (AC-1). Attention is
/// derived - a submitted plan past the review window and a scheduled well without an approved program (AC-2). Each
/// source link is decided by the page guard independently (AC-3), and the route is the one the development persona's
/// configured landing maps to (AC-4).
/// </remarks>
public sealed class DevelopmentWorkspacePageView : OgPageBase
{
    public const string Route = "development";

    private readonly OgApplication _application;
    private readonly IOgDevelopmentWorkspace _workspace;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _kpis;
    private readonly TextBlock _plans;
    private readonly TextBlock _spend;
    private readonly TextBlock _attention;
    private readonly WrapPanel _links;
    private readonly DataGrid _grid;

    public DevelopmentWorkspacePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _workspace = application.DevelopmentWorkspace;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _kpis = Body(string.Empty);
        _kpis.TextWrapping = TextWrapping.Wrap;
        _kpis.Margin = new Thickness(0, 0, 0, 6);

        _plans = Body(string.Empty);
        _plans.TextWrapping = TextWrapping.Wrap;

        _spend = Body(string.Empty);
        _spend.TextWrapping = TextWrapping.Wrap;

        _attention = Body(string.Empty);
        _attention.TextWrapping = TextWrapping.Wrap;

        _links = new WrapPanel { Margin = new Thickness(0, 6, 0, 8) };

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 220,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Plan state", "PlanState", 200),
                Text("Plans", "Count", 90),
            },
        };

        Named(_grid, "Development plans by state");

        var actions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actions.Children.Add(Button("Refresh", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(actions);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_kpis);
        panel.Children.Add(_grid);
        panel.Children.Add(_plans);
        panel.Children.Add(_spend);
        panel.Children.Add(Caption("Sources"));
        panel.Children.Add(_links);
        panel.Children.Add(Caption("Attention"));
        panel.Children.Add(_attention);

        ShowContent(panel);

        Load();
    }

    public OgDevelopmentSnapshot? Snapshot { get; private set; }

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string KpiText => _kpis.Text;

    public string PlansText => _plans.Text;

    public string SpendText => _spend.Text;

    public string AttentionText => _attention.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _workspace
            .LoadAsync(
                session.FieldId,
                session.FieldName,
                DateTime.Today,
                route => _application.Navigation.CanAccess(route, _application.Permissions))
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Snapshot = null;
            _grid.ItemsSource = null;
            _header.Text = "Development workspace";
            _kpis.Text = string.Empty;
            _plans.Text = string.Empty;
            _spend.Text = string.Empty;
            _attention.Text = string.Empty;
            _links.Children.Clear();
            _notice.Text = loaded.Message ?? "The development workspace could not be read.";
            ShowState(OgViewState.Failed, "The development workspace could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Snapshot = loaded.Value;

        _grid.ItemsSource = Snapshot.Plans.ToList();

        _header.Text =
            $"Development workspace · " +
            $"{(string.IsNullOrWhiteSpace(session.FieldName) ? "(no field selected)" : session.FieldName)} · " +
            $"as of {Snapshot.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}";

        _kpis.Text = Snapshot.KpiText;
        _plans.Text = "Plans by state: " + Snapshot.PlanText;
        _spend.Text = "Spend to date: " + Snapshot.SpendText;

        _notice.Text = Snapshot.HasActivity
            ? Snapshot.AttentionAvailable
                ? "Counts are read from the plans, schedules, facilities and cost rows."
                : Snapshot.AttentionNote ?? "Drilling-program attention is unavailable."
            : "No development plan is recorded for this field.";

        _attention.Text = Snapshot.AttentionText;

        RenderLinks(Snapshot.Links);
        ShowState(Snapshot.HasActivity ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
        return true;
    }

    /// <summary>AC-3: a source is offered only when its own page gate allows it.</summary>
    public bool OpenLink(string route)
    {
        if (Snapshot?.Links.FirstOrDefault(link =>
                string.Equals(link.Route, route, StringComparison.OrdinalIgnoreCase)) is not { Available: true } link)
        {
            _notice.Text = Snapshot?.Links.FirstOrDefault(candidate =>
                string.Equals(candidate.Route, route, StringComparison.OrdinalIgnoreCase))?.WhyNot
                ?? "That source is not available.";
            return false;
        }

        _notice.Text = $"Opening {link.Label}.";
        return _application.Pages.Navigate(link.Route);
    }

    private void RenderLinks(IReadOnlyList<OgDevelopmentLink> links)
    {
        _links.Children.Clear();

        foreach (var link in links)
        {
            var button = Button(link.Label, (_, _) => OpenLink(link.Route));
            button.IsEnabled = link.Available;
            button.ToolTip = link.WhyNot;

            _links.Children.Add(button);
        }
    }

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        FontWeight = FontWeights.SemiBold,
        Margin = new Thickness(0, 4, 0, 2),
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
