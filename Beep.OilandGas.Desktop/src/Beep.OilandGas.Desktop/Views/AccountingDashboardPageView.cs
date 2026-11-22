using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ACC-01: the accounting dashboard - the balances that need attention, the queues awaiting action and the period state,
/// read from the records the other seven accounting features keep.
/// </summary>
/// <remarks>
/// A balance whose records could not be read is shown as unavailable with the reason, never as zero (AC-1). Every count is
/// taken from the same records, and the same predicates on them, that the queue's own page lists, so the two cannot disagree
/// (AC-2). The period strip is the session's period, not the latest in the database (AC-3). A queue the signed-in account
/// cannot open is withheld with the reason instead of being shown empty (AC-4), and the last-refreshed time is always
/// printed, including when the refresh failed (AC-5).
/// </remarks>
public sealed class AccountingDashboardPageView : OgPageBase
{
    public const string Route = "accounting";

    private readonly OgApplication _application;
    private readonly IOgAccountingDashboard _dashboard;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBlock _period;
    private readonly TextBlock _balances;
    private readonly TextBlock _cash;
    private readonly TextBlock _failure;
    private readonly TextBlock _queues;
    private readonly TextBlock _withheld;
    private readonly TextBlock _attention;
    private readonly DataGrid _queueGrid;
    private readonly DataGrid _attentionGrid;
    private readonly Button _refreshButton;

    private OgAccountingDashboard? _dashboardValue;

    public AccountingDashboardPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _dashboard = application.AccountingDashboard;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 4, 0, 4);

        _period = Body(string.Empty);
        _period.TextWrapping = TextWrapping.Wrap;
        _period.Margin = new Thickness(0, 0, 0, 4);

        _balances = Body(string.Empty);
        _balances.TextWrapping = TextWrapping.Wrap;
        _balances.Margin = new Thickness(0, 4, 0, 4);

        _cash = Body(string.Empty);
        _cash.TextWrapping = TextWrapping.Wrap;
        _cash.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _queues = Body(string.Empty);
        _queues.TextWrapping = TextWrapping.Wrap;
        _queues.Margin = new Thickness(0, 4, 0, 4);

        _withheld = Body(string.Empty);
        _withheld.TextWrapping = TextWrapping.Wrap;
        _withheld.Margin = new Thickness(0, 4, 0, 4);

        _attention = Body(string.Empty);
        _attention.TextWrapping = TextWrapping.Wrap;
        _attention.Margin = new Thickness(0, 4, 0, 4);

        _queueGrid = new DataGrid
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
                Text("Queue", "Title", 240),
                Text("Count", "CountText", 90),
                Text("State", "StateText", 380),
            },
        };

        Named(_queueGrid, "Queues awaiting action");

        _attentionGrid = new DataGrid
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
                Text("Kind", "Kind", 170),
                Text("Subject", "Subject", 190),
                Text("Detail", "Detail", 200),
                Text("Amount", "Amount", 120),
                Text("Due", "Due", 110),
            },
        };

        Named(_attentionGrid, "Items needing attention");

        _refreshButton = Button("Refresh", (_, _) => Load());

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_notice);
        panel.Children.Add(_period);
        panel.Children.Add(Caption("Balances"));
        panel.Children.Add(_balances);
        panel.Children.Add(_cash);
        panel.Children.Add(_failure);
        panel.Children.Add(_refreshButton);
        panel.Children.Add(Caption("Queues awaiting action"));
        panel.Children.Add(_queueGrid);
        panel.Children.Add(_queues);
        panel.Children.Add(Caption("Withheld"));
        panel.Children.Add(_withheld);
        panel.Children.Add(Caption("Needs attention"));
        panel.Children.Add(_attentionGrid);
        panel.Children.Add(_attention);

        ShowContent(panel);

        Load();
    }

    /// <summary>The dashboard as the last successful read returned it (ACC-01).</summary>
    public OgAccountingDashboard? Dashboard => _dashboardValue;

    public bool LastFailed { get; private set; }

    /// <summary>AC-5: stamped on every refresh, whether or not the read worked.</summary>
    public DateTime RefreshedAt { get; private set; } = DateTime.UtcNow;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string RefreshedText => _refreshed.Text;

    public string PeriodText => _period.Text;

    public string BalancesText => _balances.Text;

    public string CashText => _cash.Text;

    public string FailureText => _failure.Text;

    public string QueuesText => _queues.Text;

    public string WithheldText => _withheld.Text;

    public string AttentionText => _attention.Text;

    public bool ReadFailed => _dashboardValue?.ReadFailed ?? LastFailed;

    public IReadOnlyList<OgDashboardQueue> ShownQueues => _dashboardValue?.Shown ?? [];

    public IReadOnlyList<OgDashboardQueue> WithheldQueues => _dashboardValue?.Withheld ?? [];

    public bool Load()
    {
        RefreshedAt = DateTime.UtcNow;

        var context = OgDashboardContext.FromSession(_application.Session, _application.Permissions);
        var loaded = _dashboard.LoadAsync(context).GetAwaiter().GetResult();

        _header.Text = "Accounting · dashboard";
        _refreshed.Text = "Last refreshed " +
                          RefreshedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _dashboardValue = null;

            // AC-1: no balance is shown at all when the read failed - an empty number is not a zero.
            _notice.Text = loaded.Message ?? "The accounting dashboard could not be read.";
            _period.Text = "Period: unknown - the ledger could not be read";
            _balances.Text = string.Empty;
            _cash.Text = string.Empty;
            _queues.Text = string.Empty;
            _withheld.Text = string.Empty;
            _attention.Text = string.Empty;
            _queueGrid.ItemsSource = null;
            _attentionGrid.ItemsSource = null;
            _failure.Text = "Nothing on this page is a balance: the read failed, so no figure is inferred.";

            ShowState(OgViewState.Failed, "The accounting dashboard could not be read.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    private void Apply(OgAccountingDashboard dashboard)
    {
        _dashboardValue = dashboard;

        _notice.Text = dashboard.Kpis.AnyUnavailable
            ? "Some figures are unavailable; each says which records could not be read."
            : dashboard.SummaryText;

        _period.Text = "Period: " + dashboard.Period.Describe();

        _balances.Text =
            "Receivable " + dashboard.Kpis.OpenArText + " · Payable " + dashboard.Kpis.OpenApText +
            " · Overdue " + dashboard.Kpis.OverdueText + " · Unposted journals " +
            dashboard.Kpis.UnpostedJournalsText + " · Period " + dashboard.Kpis.PeriodState;

        _cash.Text = "Cash: " + dashboard.Kpis.CashText + " · " + dashboard.Kpis.CashBreakdownText;

        _failure.Text = dashboard.FailureText;

        _queueGrid.ItemsSource = dashboard.Queues.ToList();
        _attentionGrid.ItemsSource = dashboard.Attention.ToList();

        _queues.Text = dashboard.Shown.Count == 0
            ? "no queue is available to you"
            : string.Join(Environment.NewLine, dashboard.Shown.Select(queue =>
                queue.Title + " · " + queue.CountText + " · " + queue.StateText));

        // AC-4: the withheld queues are named with the reason they are not shown.
        _withheld.Text = dashboard.Withheld.Count == 0
            ? "every queue is available to your account"
            : string.Join(Environment.NewLine, dashboard.Withheld.Select(queue =>
                queue.Title + " - " + queue.WithheldReason));

        _attention.Text = dashboard.AttentionText;

        _refreshed.Text = dashboard.RefreshedText;

        ShowState(dashboard.Attention.Count > 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
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
            Margin = new Thickness(0, 6, 8, 6),
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
