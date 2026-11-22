using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// DEC-01: the decommissioning coordinator's landing - what is in scope, its recorded state, what it is estimated to cost and
/// what the records do not say.
/// </summary>
/// <remarks>
/// Every count comes from the records and a failed read shows none, never zero (AC-1); an asset with no estimate is counted
/// and excluded from the totals (AC-2); the source links are the shell's own page gate (AC-3); costs are decimal with their
/// currency (AC-4); and overdue is computed from a planned year at read time (AC-5) - the owner's tables record none, and the
/// page says so rather than inventing a schedule.
/// </remarks>
public sealed class DecommissioningPageView : OgPageBase
{
    public const string Route = "decommissioning";

    private readonly OgApplication _application;
    private readonly IOgDecommissioning _workspace;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _states;
    private readonly TextBlock _planned;
    private readonly TextBlock _costs;
    private readonly TextBlock _restoration;
    private readonly TextBlock _links;
    private readonly TextBlock _inbox;
    private readonly TextBlock _failure;
    private readonly TextBlock _refreshed;
    private readonly DataGrid _scopeGrid;
    private readonly DataGrid _restorationGrid;
    private readonly Button _refreshButton;

    private OgDecomWorkspace? _snapshot;

    public DecommissioningPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _workspace = application.Decommissioning;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _states = Body(string.Empty);
        _states.TextWrapping = TextWrapping.Wrap;
        _states.Margin = new Thickness(0, 0, 0, 4);

        _planned = Body(string.Empty);
        _planned.TextWrapping = TextWrapping.Wrap;
        _planned.Margin = new Thickness(0, 0, 0, 4);

        _costs = Body(string.Empty);
        _costs.TextWrapping = TextWrapping.Wrap;
        _costs.Margin = new Thickness(0, 4, 0, 4);

        _restoration = Body(string.Empty);
        _restoration.TextWrapping = TextWrapping.Wrap;
        _restoration.Margin = new Thickness(0, 4, 0, 4);

        _links = Body(string.Empty);
        _links.TextWrapping = TextWrapping.Wrap;
        _links.Margin = new Thickness(0, 4, 0, 4);

        _inbox = Body(string.Empty);
        _inbox.TextWrapping = TextWrapping.Wrap;
        _inbox.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;
        _refreshed.Margin = new Thickness(0, 0, 0, 4);

        _scopeGrid = new DataGrid
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
                Text("Asset", "AssetId", 170),
                Text("Kind", "Kind", 90),
                Text("State", "StateText", 150),
                Text("Planned year", "PlannedYear", 110),
                Text("Estimated", "EstimatedText", 170),
            },
        };

        Named(_scopeGrid, "Decommissioning scope");

        _restorationGrid = new DataGrid
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
                Text("Site", "SiteId", 160),
                Text("Type", "Type", 130),
                Text("Status", "Status", 130),
                Text("Certification", "Certification", 130),
                Text("Completed", "Completed", 120),
            },
        };

        Named(_restorationGrid, "Environmental restoration");

        _refreshButton = Button("Refresh", (_, _) => Load());

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(_counts);
        panel.Children.Add(_states);
        panel.Children.Add(_planned);
        panel.Children.Add(_failure);
        panel.Children.Add(_refreshButton);
        panel.Children.Add(Caption("Scope"));
        panel.Children.Add(_scopeGrid);
        panel.Children.Add(Caption("Cost"));
        panel.Children.Add(_costs);
        panel.Children.Add(Caption("Environmental restoration"));
        panel.Children.Add(_restoration);
        panel.Children.Add(_restorationGrid);
        panel.Children.Add(Caption("Your assigned work"));
        panel.Children.Add(_inbox);
        panel.Children.Add(Caption("Sources"));
        panel.Children.Add(_links);

        ShowContent(panel);

        Load();
    }

    public OgDecomWorkspace? Snapshot => _snapshot;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string StatesText => _states.Text;

    public string PlannedText => _planned.Text;

    public string CostsText => _costs.Text;

    public string RestorationText => _restoration.Text;

    public string LinksText => _links.Text;

    public string InboxText => _inbox.Text;

    public string FailureText => _failure.Text;

    public string RefreshedText => _refreshed.Text;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _workspace.LoadAsync(
            session.FieldId,
            session.PersonaCode,
            _application.Permissions.Roles,
            DateTime.Today,
            session.TimeZoneId,
            route => _application.Navigation.CanAccess(route, _application.Permissions)).GetAwaiter().GetResult();

        _header.Text = "Decommissioning";

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _snapshot = null;

            // AC-1: a failed read shows no figure at all - there is nothing here a zero could stand for.
            _counts.Text = string.Empty;
            _states.Text = string.Empty;
            _planned.Text = string.Empty;
            _costs.Text = string.Empty;
            _restoration.Text = string.Empty;
            _links.Text = string.Empty;
            _inbox.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _scopeGrid.ItemsSource = null;
            _restorationGrid.ItemsSource = null;
            _failure.Text = loaded.Message ?? "The decommissioning workspace could not be read.";

            ShowState(OgViewState.Failed, "The decommissioning workspace could not be read.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    private void Apply(OgDecomWorkspace workspace)
    {
        _snapshot = workspace;

        _counts.Text = workspace.CountsText;
        _states.Text = workspace.StateStrip;
        _planned.Text = workspace.PlannedText;
        _costs.Text = workspace.CostText;
        _restoration.Text = workspace.RestorationText;
        _links.Text = workspace.LinksText;
        _inbox.Text = workspace.InboxText;

        _refreshed.Text = "last refreshed " +
                          workspace.ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
                          " · as of " + workspace.AsOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                          (string.IsNullOrWhiteSpace(workspace.FieldId) ? " · no field selected" : " · field " + workspace.FieldId);

        _scopeGrid.ItemsSource = workspace.Scopes.ToList();
        _restorationGrid.ItemsSource = workspace.Restoration.ToList();

        _failure.Text = string.Empty;

        ShowState(workspace.Scopes.Count == 0 && workspace.Restoration.Count == 0
            ? OgViewState.Empty
            : OgViewState.Ready, workspace.CountsText);
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
            Margin = new Thickness(0, 6, 8, 0),
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
