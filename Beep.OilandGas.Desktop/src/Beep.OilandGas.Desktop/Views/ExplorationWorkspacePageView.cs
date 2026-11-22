using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One row of the funnel grid.</summary>
public sealed record OgFunnelRow(string Stage, int Count, string Order);

/// <summary>
/// EXP-01: the exploration persona's landing page - the funnel, the field's plays/leads/wells, the prospects
/// with incomplete risk data, and the sources this account may open.
/// </summary>
/// <remarks>
/// Every number on this page comes from <see cref="IOgExplorationWorkspace"/>, which counts rows. A failed read
/// shows a failure panel and <b>no</b> counts, so a zero is never a stand-in for "unknown" (AC-1). The source
/// buttons are built from the shell's page-permission answer, so a button that appears will open (AC-2). The
/// warning strip counts only prospects missing risk elements (AC-3). The header states whether this really is
/// this persona's landing route (AC-4).
/// </remarks>
public sealed class ExplorationWorkspacePageView : OgPageBase
{
    public const string Route = "exploration";

    private readonly Bootstrapping.OgApplication _application;
    private readonly IOgExplorationWorkspace _workspace;

    private readonly TextBlock _header;
    private readonly TextBlock _landing;
    private readonly TextBlock _notice;
    private readonly TextBlock _kpis;
    private readonly TextBlock _risk;
    private readonly TextBlock _sources;
    private readonly TextBlock _inbox;
    private readonly DataGrid _funnel;
    private readonly WrapPanel _links;

    public ExplorationWorkspacePageView(Bootstrapping.OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _workspace = application.ExplorationWorkspace;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _landing = Body(string.Empty);
        _notice = Body(string.Empty);
        _kpis = Body(string.Empty);
        _kpis.FontSize = 13;
        _risk = Body(string.Empty);
        _sources = Body(string.Empty);
        _inbox = Body(string.Empty);

        _funnel = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MaxHeight = 200,
            MinHeight = 90,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Stage", nameof(OgFunnelRow.Stage), 260),
                Text("Prospects", nameof(OgFunnelRow.Count), 100),
            },
        };

        Named(_funnel, "Prospect funnel by stage");

        _links = new WrapPanel { Margin = new Thickness(0, 2, 0, 8) };

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Button("Refresh", (_, _) => _ = LoadAsync()));
        toolbar.Children.Add(Button("Well index", (_, _) => _application.Pages.Navigate(WellSearchPageView.Route)));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_header);
        panel.Children.Add(_landing);
        panel.Children.Add(_notice);
        panel.Children.Add(_kpis);
        panel.Children.Add(_risk);
        panel.Children.Add(_links);
        panel.Children.Add(_sources);
        panel.Children.Add(_funnel);
        panel.Children.Add(_inbox);

        ShowContent(panel);

        Load();
    }

    /// <summary>The snapshot last read, or null before the first successful load.</summary>
    public OgExplorationSnapshot? Snapshot { get; private set; }

    public IReadOnlyList<OgFunnelRow> FunnelRows { get; private set; } = [];

    /// <summary>True when the last read failed; the funnel is then empty <b>and</b> failed (AC-1).</summary>
    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string LandingText => _landing.Text;

    public string NoticeText => _notice.Text;

    public string KpisText => _kpis.Text;

    public string RiskText => _risk.Text;

    public string SourcesText => _sources.Text;

    public string InboxText => _inbox.Text;

    /// <summary>The routes actually offered as buttons (AC-2).</summary>
    public IReadOnlyList<string> AvailableLinkRoutes =>
        Snapshot?.AvailableLinks.Select(link => link.Route).ToList() ?? [];

    /// <summary>Loads the workspace for the signed-in session.</summary>
    public bool Load() => LoadAsync().GetAwaiter().GetResult();

    /// <summary>Loads the workspace for the signed-in session.</summary>
    public async Task<bool> LoadAsync()
    {
        var session = _application.Session;

        var loaded = await _workspace
            .LoadAsync(
                session.FieldId,
                session.FieldName,
                session.PeriodName,
                session.PersonaCode,
                CanOpen)
            .ConfigureAwait(true);

        if (!loaded.Success || loaded.Value is null)
        {
            Snapshot = null;
            FunnelRows = [];
            LastFailed = true;
            _funnel.ItemsSource = null;
            _links.Children.Clear();
            _header.Text = "Exploration workspace";
            _kpis.Text = string.Empty;
            _risk.Text = string.Empty;
            _sources.Text = string.Empty;
            _landing.Text = string.Empty;
            _inbox.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The exploration workspace could not be read.";
            ShowState(OgViewState.Failed, "The exploration workspace could not be read.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Snapshot = loaded.Value;

        FunnelRows = Snapshot.Funnel
            .Select(stage => new OgFunnelRow(
                stage.Stage,
                stage.Count,
                stage.Sequence.ToString(CultureInfo.InvariantCulture)))
            .ToList();

        _funnel.ItemsSource = FunnelRows;
        _header.Text =
            $"{Snapshot.FieldName} · {Snapshot.Persona} · {Snapshot.PeriodName}";
        _landing.Text = LandingSentence();

        _kpis.Text =
            $"{Snapshot.ProspectsInFunnel.ToString("N0", CultureInfo.InvariantCulture)} prospect(s) in the funnel · " +
            $"{Snapshot.Kpis.Plays.ToString("N0", CultureInfo.InvariantCulture)} play(s) · " +
            $"{Snapshot.Kpis.Leads.ToString("N0", CultureInfo.InvariantCulture)} lead(s) · " +
            $"{Snapshot.Kpis.WellsPlanned.ToString("N0", CultureInfo.InvariantCulture)} well(s) planned · " +
            $"{Snapshot.Kpis.WellsDrilling.ToString("N0", CultureInfo.InvariantCulture)} well(s) drilling";

        _risk.Text = RiskSentence();
        _sources.Text = SourcesSentence();
        _inbox.Text = "The assigned-work inbox arrives with WFL-05; this build does not invent items.";

        _links.Children.Clear();

        foreach (var link in Snapshot.AvailableLinks)
        {
            _links.Children.Add(Button(link.Label, (_, _) => _application.Pages.Navigate(link.Route)));
        }

        _notice.Text = Snapshot.HasActivity
            ? "Counts are read from the prospect, play, lead and well records for this field."
            : "No exploration activity recorded for this field.";

        ShowState(
            Snapshot.HasActivity ? OgViewState.Ready : OgViewState.Empty,
            _notice.Text);

        return true;
    }

    /// <summary>The shell's own page-permission answer; the one gate the source links use (AC-2).</summary>
    public bool CanOpen(string route) =>
        _application.Navigation.CanAccess(route, _application.Permissions);

    private string LandingSentence()
    {
        var landing = OgPersonaLanding.Resolve(_application.Permissions.Roles, CanOpen);

        return string.Equals(landing.Route, Route, StringComparison.OrdinalIgnoreCase)
            ? "This is the persona's landing page."
            : $"This persona's landing page is '{landing.Route}', not this page.";
    }

    private string RiskSentence()
    {
        if (Snapshot is null || Snapshot.IncompleteRiskCount == 0)
        {
            return "No prospect is missing risk elements.";
        }

        var names = string.Join(", ", Snapshot.IncompleteRiskProspects.Take(10));
        var rest = Snapshot.IncompleteRiskCount > 10
            ? $" and {Snapshot.IncompleteRiskCount - 10} more"
            : string.Empty;

        return
            $"{Snapshot.IncompleteRiskCount.ToString("N0", CultureInfo.InvariantCulture)} prospect(s) are missing " +
            $"risk elements ({string.Join(", ", OgExplorationWorkspace.RiskElements)}): {names}{rest}.";
    }

    private string SourcesSentence()
    {
        if (Snapshot is null)
        {
            return string.Empty;
        }

        var unavailable = Snapshot.Links.Where(link => !link.Available).ToList();

        return unavailable.Count == 0
            ? string.Empty
            : "Not available here: " +
              string.Join(
                  "; ",
                  unavailable.Select(link => $"{link.Label} - {link.WhyNot}"));
    }

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
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
