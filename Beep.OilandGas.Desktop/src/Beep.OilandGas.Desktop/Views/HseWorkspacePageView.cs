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
/// HSE-01: the safety officer's landing - the counts from recorded facts, the incident state strip, the assigned work and the
/// sources this account may open.
/// </summary>
/// <remarks>
/// The open figure is the five recognised active states and nothing else (AC-1); closed, cancelled and any unrecognised state
/// have their own rows (AC-2); a failed read shows the failure panel and no counts at all, so a zero always means the records
/// say zero (AC-3); nothing on this page is a hardcoded alert (AC-4); each source link is offered only when the shell's own
/// page gate will open it (AC-5); and the HSE persona's landing route is the RBAC configuration's (AC-6).
/// </remarks>
public sealed class HseWorkspacePageView : OgPageBase
{
    public const string Route = "hse";

    private readonly OgApplication _application;
    private readonly IOgHseWorkspace _workspace;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBlock _jurisdiction;
    private readonly TextBlock _counts;
    private readonly TextBlock _strip;
    private readonly TextBlock _inbox;
    private readonly TextBlock _links;
    private readonly TextBlock _failure;
    private readonly DataGrid _stateGrid;
    private readonly Button _refreshButton;

    private OgHseSnapshot? _snapshot;

    public HseWorkspacePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _workspace = application.Hse;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 4, 0, 4);

        _jurisdiction = Body(string.Empty);
        _jurisdiction.TextWrapping = TextWrapping.Wrap;
        _jurisdiction.Margin = new Thickness(0, 0, 0, 4);

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _strip = Body(string.Empty);
        _strip.TextWrapping = TextWrapping.Wrap;
        _strip.Margin = new Thickness(0, 0, 0, 4);

        _inbox = Body(string.Empty);
        _inbox.TextWrapping = TextWrapping.Wrap;
        _inbox.Margin = new Thickness(0, 4, 0, 4);

        _links = Body(string.Empty);
        _links.TextWrapping = TextWrapping.Wrap;
        _links.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _stateGrid = new DataGrid
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
                Text("State", "Label", 190),
                Text("Count", "Count", 90),
                Text("Counts as open", "Active", 120),
            },
        };

        Named(_stateGrid, "Incidents by recorded state");

        _refreshButton = Button("Refresh", (_, _) => Load());

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_notice);
        panel.Children.Add(_jurisdiction);
        panel.Children.Add(Caption("Counts from the records"));
        panel.Children.Add(_counts);
        panel.Children.Add(_failure);
        panel.Children.Add(_refreshButton);
        panel.Children.Add(Caption("Incident state strip"));
        panel.Children.Add(_strip);
        panel.Children.Add(_stateGrid);
        panel.Children.Add(Caption("Your assigned work"));
        panel.Children.Add(_inbox);
        panel.Children.Add(Caption("Sources"));
        panel.Children.Add(_links);

        ShowContent(panel);

        Load();
    }

    /// <summary>The workspace as the last successful read returned it.</summary>
    public OgHseSnapshot? Snapshot => _snapshot;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string RefreshedText => _refreshed.Text;

    public string JurisdictionText => _jurisdiction.Text;

    public string CountsText => _counts.Text;

    public string StripText => _strip.Text;

    public string InboxText => _inbox.Text;

    public string LinksText => _links.Text;

    public string FailureText => _failure.Text;

    public IReadOnlyList<OgHseLink> AvailableLinks => _snapshot?.AvailableLinks ?? [];

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _workspace.LoadAsync(
            session.FieldId,
            session.FieldName,
            null,
            session.PersonaCode,
            _application.Permissions.Roles,
            DateTime.Today,
            session.TimeZoneId,
            route => _application.Navigation.CanAccess(route, _application.Permissions)).GetAwaiter().GetResult();

        _header.Text = "HSE";

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _snapshot = null;

            // AC-3: a failed read shows the failure panel and no count of any kind - an empty figure is not a zero.
            _refreshed.Text = string.Empty;
            _jurisdiction.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The HSE workspace could not be read.";
            _counts.Text = string.Empty;
            _strip.Text = string.Empty;
            _inbox.Text = string.Empty;
            _links.Text = string.Empty;
            _stateGrid.ItemsSource = null;
            _failure.Text = "No count is shown: the read failed, so nothing on this page is a zero.";

            ShowState(OgViewState.Failed, "The HSE workspace could not be read.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    private void Apply(OgHseSnapshot snapshot)
    {
        _snapshot = snapshot;

        _refreshed.Text = snapshot.RefreshedText;
        _jurisdiction.Text = snapshot.JurisdictionText;
        _notice.Text = snapshot.NoticeText;
        _counts.Text = snapshot.Counts.Summary;
        _strip.Text = snapshot.IncidentStrip;
        _stateGrid.ItemsSource = snapshot.IncidentStates.ToList();

        _inbox.Text = snapshot.Inbox.Available
            ? "Task inbox: " + snapshot.Inbox.Text
            : "Task inbox unavailable: " + snapshot.Inbox.Text;

        _links.Text = snapshot.LinksText;
        _failure.Text = string.Empty;

        ShowState(snapshot.Inbox.Available ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
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
