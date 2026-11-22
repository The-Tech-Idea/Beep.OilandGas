using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// RES-05: reserves classification and reservoir characterization - the two questions an auditor and an investor both
/// ask, with the exclusions, the sources and the disclosure gate stated rather than implied.
/// </summary>
/// <remarks>
/// Totals exclude unclassified entries and say how many were excluded (AC-1). Every entry carries a classifier and an
/// effective date, and an entry without a classifier is flagged and blocks the disclosure (AC-2, AC-5). A revision is an
/// append, so history is never overwritten (AC-3). Every characterization fact names the record it came from (AC-4).
/// </remarks>
public sealed class ReservesPageView : OgPageBase
{
    public const string Route = "reserves";

    private readonly OgApplication _application;
    private readonly IOgReserves _reserves;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _unclassified;
    private readonly TextBlock _disclosureState;
    private readonly TextBlock _characterization;
    private readonly TextBlock _maturation;
    private readonly TextBlock _preview;
    private readonly TextBlock _detail;
    private readonly TextBox _pool;
    private readonly ComboBox _category;
    private readonly TextBox _gross;
    private readonly TextBox _net;
    private readonly ComboBox _unit;
    private readonly TextBox _effective;
    private readonly TextBox _classifier;
    private readonly TextBox _basis;
    private readonly TextBox _period;
    private readonly DataGrid _entries;
    private readonly DataGrid _totals;
    private readonly DataGrid _disclosures;
    private readonly Button _approveButton;

    private OgReserveWorkspace? _workspace;

    public ReservesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _reserves = application.Reserves;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _unclassified = Body(string.Empty);
        _unclassified.TextWrapping = TextWrapping.Wrap;
        _unclassified.Margin = new Thickness(0, 0, 0, 4);

        _disclosureState = Body(string.Empty);
        _disclosureState.TextWrapping = TextWrapping.Wrap;
        _disclosureState.Margin = new Thickness(0, 0, 0, 8);

        _characterization = Body(string.Empty);
        _characterization.TextWrapping = TextWrapping.Wrap;
        _characterization.Margin = new Thickness(0, 4, 0, 4);

        _maturation = Body(string.Empty);
        _maturation.TextWrapping = TextWrapping.Wrap;
        _maturation.Margin = new Thickness(0, 0, 0, 8);

        _preview = Body(string.Empty);
        _preview.TextWrapping = TextWrapping.Wrap;
        _preview.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _pool = Field("Pool id", 110);
        _gross = Field("Gross volume", 110);
        _net = Field("Net volume", 110);
        _effective = Field("Effective date yyyy-MM-dd", 150);
        _effective.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _classifier = Field("Classifier", 110);
        _classifier.Text = _application.Session.UserId;
        _basis = Field("Basis for the revision", 240);
        _period = Field("Disclosure period yyyy-MM", 140);
        _period.Text = DateTime.Today.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        _category = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        Named(_category, "Reserves category");

        _unit = new ComboBox
        {
            MinWidth = 90,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = ReserveUnits.Units,
            SelectedIndex = 0,
        };

        Named(_unit, "Volume unit");

        _entries = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 120,
            MaxHeight = 200,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Pool", "PoolName", 140),
                Text("Category", "CategoryText", 100),
                Text("Gross", "GrossText", 130),
                Text("Net", "NetText", 130),
                Text("Effective", "EffectiveText", 100),
                Text("Revision", "RevisionText", 90),
                Text("Classifier", "ClassifierText", 150),
            },
        };

        Named(_entries, "Reserves entries, current revision and history");

        _totals = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Category", "Category", 110),
                Text("Entries", "Count", 70),
                Text("Gross", "GrossText", 180),
                Text("Net", "NetText", 180),
            },
        };

        Named(_totals, "Totals by category");

        _disclosures = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 120,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Period", "PeriodText", 90),
                Text("Gross", "GrossTotal", 140),
                Text("Net", "NetTotal", 140),
                Text("Entries", "EntryCount", 70),
                Text("Excluded", "ExcludedCount", 80),
                Text("Prepared by", "PreparedBy", 140),
                Text("Approved by", "ApprovedBy", 140),
            },
        };

        Named(_disclosures, "Approved disclosures");

        var entryBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        entryBar.Children.Add(Caption("Pool"));
        entryBar.Children.Add(_pool);
        entryBar.Children.Add(_category);
        entryBar.Children.Add(_gross);
        entryBar.Children.Add(_net);
        entryBar.Children.Add(_unit);
        entryBar.Children.Add(_effective);
        entryBar.Children.Add(Button("Record revision", (_, _) => _ = AddRevisionAsync()));

        var classifierBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        classifierBar.Children.Add(Caption("Classifier"));
        classifierBar.Children.Add(_classifier);
        classifierBar.Children.Add(_basis);

        var disclosureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        disclosureBar.Children.Add(Caption("Disclosure period"));
        disclosureBar.Children.Add(_period);
        _approveButton = Button("Approve disclosure", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;
        disclosureBar.Children.Add(_approveButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_summary);
        panel.Children.Add(_unclassified);
        panel.Children.Add(_disclosureState);
        panel.Children.Add(Caption("Entries"));
        panel.Children.Add(_entries);
        panel.Children.Add(Caption("New revision"));
        panel.Children.Add(entryBar);
        panel.Children.Add(classifierBar);
        panel.Children.Add(Caption("Totals"));
        panel.Children.Add(_totals);
        panel.Children.Add(Caption("Characterization"));
        panel.Children.Add(_characterization);
        panel.Children.Add(Caption("Maturation"));
        panel.Children.Add(_maturation);
        panel.Children.Add(Caption("Disclosure"));
        panel.Children.Add(_preview);
        panel.Children.Add(disclosureBar);
        panel.Children.Add(_disclosures);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgReserveWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string UnclassifiedText => _unclassified.Text;

    public string DisclosureStateText => _disclosureState.Text;

    public string CharacterizationText => _characterization.Text;

    public string MaturationText => _maturation.Text;

    public string PreviewText => _preview.Text;

    public string DetailText => _detail.Text;

    public bool DisclosureBlocked => _workspace?.DisclosureBlocked ?? true;

    public bool Load()
    {
        var session = _application.Session;

        var loaded = _reserves
            .LoadAsync(session.FieldId, session.FieldName, null)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _entries.ItemsSource = null;
            _totals.ItemsSource = null;
            _disclosures.ItemsSource = null;
            _header.Text = "Reserves";
            _summary.Text = string.Empty;
            _unclassified.Text = string.Empty;
            _disclosureState.Text = string.Empty;
            _characterization.Text = string.Empty;
            _maturation.Text = string.Empty;
            _preview.Text = string.Empty;
            _detail.Text = string.Empty;
            _notice.Text = loaded.Message ?? "The reserves could not be read.";
            ShowState(OgViewState.Failed, "The reserves could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        _workspace = loaded.Value;

        Apply(loaded.Value);
        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> AddRevisionAsync()
    {
        var applied = await _reserves
            .AddRevisionAsync(
                new OgReserveEntryValues(
                    Empty(_pool.Text),
                    _category.SelectedItem as string,
                    Parse(_gross.Text),
                    Parse(_net.Text),
                    _unit.SelectedItem as string,
                    Date(_effective.Text),
                    Empty(_classifier.Text),
                    Empty(_basis.Text)),
                _application.Session.UserId,
                _application.Session.FieldName)
            .ConfigureAwait(true);

        if (!applied.Success || applied.Value is null)
        {
            LastFailed = true;
            _notice.Text = applied.Message ?? "The entry could not be saved.";
            ShowState(OgViewState.Failed, "The entry could not be saved.", applied.Reference);
            return false;
        }

        _workspace = applied.Value;
        Apply(applied.Value);

        _application.AuditLog.Add(
            "reservoir.reserves.revision",
            "reservoir",
            applied.Value.FieldId ?? "(no field)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Recorded as a new revision; the previous one is kept.";
        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> ApproveAsync()
    {
        if (_workspace is not { } workspace)
        {
            _notice.Text = "There is no reserves entry to disclose.";
            return false;
        }

        var period = Date(_period.Text) ?? DateTime.Today;

        var approved = await _reserves
            .ApproveDisclosureAsync(workspace.FieldId, period, _application.Session.UserId, _application.Session.FieldName)
            .ConfigureAwait(true);

        if (!approved.Success || approved.Value is null)
        {
            LastFailed = true;
            _notice.Text = approved.Message ?? "The disclosure could not be approved.";
            ShowState(OgViewState.Failed, "The disclosure could not be approved.", approved.Reference);
            return false;
        }

        _workspace = approved.Value;
        Apply(approved.Value);

        _application.AuditLog.Add(
            "reservoir.reserves.disclose",
            "reservoir",
            approved.Value.FieldId ?? "(no field)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Disclosure approved and recorded with the totals it was approved under.";
        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    private void Apply(OgReserveWorkspace workspace)
    {
        _category.ItemsSource = ReserveUnits.Categories.ToList();
        if (_category.SelectedIndex < 0 && _category.Items.Count > 0)
        {
            _category.SelectedIndex = 0;
        }

        _entries.ItemsSource = workspace.Entries.ToList();
        _totals.ItemsSource = workspace.Totals.ToList();
        _disclosures.ItemsSource = workspace.Disclosures.ToList();

        _header.Text = $"{workspace.FieldName} · {workspace.Summary}";
        _summary.Text = workspace.Summary;
        _unclassified.Text = workspace.UnclassifiedText;
        _disclosureState.Text = workspace.DisclosureStateText;
        _characterization.Text = workspace.CharacterizationText;
        _maturation.Text = workspace.MaturationText;
        _preview.Text = workspace.Preview;

        _detail.Text = string.Join(
            Environment.NewLine,
            new[]
            {
                workspace.TotalsText,
                workspace.EntriesText,
                workspace.DisclosuresText,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private void RefreshEnabled()
    {
        _approveButton.IsEnabled = _workspace is { HasEntries: true, DisclosureBlocked: false };
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

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
