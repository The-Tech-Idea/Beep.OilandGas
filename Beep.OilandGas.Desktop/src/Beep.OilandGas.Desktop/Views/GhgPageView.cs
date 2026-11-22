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
/// HSE-06: the GHG factor catalog, its review state and the emissions report.
/// </summary>
/// <remarks>
/// A report without usable factors, without reviewed revisions or without recorded activity is <b>unavailable with its
/// reasons, never zero</b> (AC-1, AC-5); every line names its factor, unit, source and revision (AC-2); a catalog or
/// environment change since the review forces a re-review (AC-3); an unrepresentable value is refused (AC-4); and the export
/// text is built from the same figures the grid shows (AC-6).
/// </remarks>
public sealed class GhgPageView : OgPageBase
{
    public const string Route = "ghg";

    private readonly OgApplication _application;
    private readonly IOgGhg _ghg;

    private readonly TextBlock _header;
    private readonly TextBlock _scope;
    private readonly TextBlock _catalog;
    private readonly TextBlock _review;
    private readonly TextBlock _status;
    private readonly TextBlock _total;
    private readonly TextBlock _unavailable;
    private readonly TextBlock _failure;
    private readonly TextBox _year;
    private readonly TextBox _jurisdiction;
    private readonly TextBox _rationale;
    private readonly TextBox _export;
    private readonly DataGrid _activityGrid;
    private readonly DataGrid _lineGrid;
    private readonly Button _loadButton;
    private readonly Button _selectButton;

    private OgGhgWorkspace? _workspace;

    public GhgPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _ghg = application.Ghg;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _scope = Body(string.Empty);
        _scope.TextWrapping = TextWrapping.Wrap;
        _scope.Margin = new Thickness(0, 4, 0, 4);

        _catalog = Body(string.Empty);
        _catalog.TextWrapping = TextWrapping.Wrap;
        _catalog.Margin = new Thickness(0, 4, 0, 4);

        _review = Body(string.Empty);
        _review.TextWrapping = TextWrapping.Wrap;
        _review.Margin = new Thickness(0, 0, 0, 4);

        _status = Body(string.Empty);
        _status.FontWeight = FontWeights.SemiBold;
        _status.TextWrapping = TextWrapping.Wrap;
        _status.Margin = new Thickness(0, 4, 0, 4);

        _total = Body(string.Empty);
        _total.TextWrapping = TextWrapping.Wrap;
        _total.Margin = new Thickness(0, 0, 0, 4);

        _unavailable = Body(string.Empty);
        _unavailable.TextWrapping = TextWrapping.Wrap;
        _unavailable.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _year = Field("Year", 90);
        _jurisdiction = Field("Jurisdiction", 120);
        _rationale = Field("Review rationale", 260);

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 110,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Report export");

        _activityGrid = new DataGrid
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
                Text("Entity", "Entity", 170),
                Text("Activity", "ActivityType", 150),
                Text("Volume", "Volume", 100),
                Text("Unit", "Unit", 80),
                Text("Period", "PeriodText", 190),
            },
        };

        Named(_activityGrid, "Recorded activity");

        _lineGrid = new DataGrid
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
                Text("Activity", "Activity", 250),
                Text("Quantity", "Quantity", 100),
                Text("Unit", "Unit", 80),
                Text("Factor", "Factor", 100),
                Text("Factor unit", "FactorUnit", 100),
                Text("Emissions", "Emissions", 110),
                Text("Factor id", "FactorId", 110),
                Text("Revision", "Revision", 110),
                Text("Source", "SourceReference", 200),
            },
        };

        Named(_lineGrid, "Report lines");

        _loadButton = Button("Load", (_, _) => Load());
        _selectButton = Button("Review and select catalog", (_, _) => _ = SelectAsync());

        var bar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        bar.Children.Add(Caption("Year"));
        bar.Children.Add(_year);
        bar.Children.Add(Caption("Jurisdiction"));
        bar.Children.Add(_jurisdiction);
        bar.Children.Add(_loadButton);

        var selectBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        selectBar.Children.Add(Caption("Rationale"));
        selectBar.Children.Add(_rationale);
        selectBar.Children.Add(_selectButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_scope);
        panel.Children.Add(bar);
        panel.Children.Add(Caption("Factor catalog"));
        panel.Children.Add(_catalog);
        panel.Children.Add(Caption("Review"));
        panel.Children.Add(_review);
        panel.Children.Add(selectBar);
        panel.Children.Add(Caption("Report"));
        panel.Children.Add(_status);
        panel.Children.Add(_failure);
        panel.Children.Add(_unavailable);
        panel.Children.Add(_total);
        panel.Children.Add(_lineGrid);
        panel.Children.Add(Caption("Recorded activity"));
        panel.Children.Add(_activityGrid);
        panel.Children.Add(Caption("Export"));
        panel.Children.Add(_export);

        ShowContent(panel);

        _year.Text = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
        _jurisdiction.Text = string.Empty;

        Load();
    }

    public OgGhgWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string ScopeText => _scope.Text;

    public string CatalogText => _catalog.Text;

    public string ReviewText => _review.Text;

    public string StatusText => _status.Text;

    public string TotalText => _total.Text;

    public string UnavailableText => _unavailable.Text;

    public string FailureText => _failure.Text;

    public string ExportText => _export.Text;

    public bool Load()
    {
        _header.Text = "HSE · GHG";

        var year = int.TryParse(_year.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : DateTime.Today.Year;

        var session = _application.Session;

        var loaded = _ghg.LoadAsync(
            OgHseScope.FromSession(session),
            year,
            Empty(_jurisdiction.Text)).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;

            // A failed read shows the reason and no figures at all - there is nothing here a zero could stand for (AC-1).
            _scope.Text = string.Empty;
            _catalog.Text = string.Empty;
            _review.Text = string.Empty;
            _status.Text = string.Empty;
            _total.Text = string.Empty;
            _unavailable.Text = string.Empty;
            _export.Text = string.Empty;
            _activityGrid.ItemsSource = null;
            _lineGrid.ItemsSource = null;
            _failure.Text = loaded.Message ?? "The GHG report could not be read.";

            ShowState(OgViewState.Failed, "The GHG report is not shown.", loaded.Reference);

            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);

        return true;
    }

    public async Task<bool> SelectAsync()
    {
        var session = _application.Session;

        var year = int.TryParse(_year.Text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : DateTime.Today.Year;

        var outcome = await _ghg
            .SelectAsync(
                OgHseScope.FromSession(session),
                year,
                _jurisdiction.Text ?? string.Empty,
                _rationale.Text ?? string.Empty,
                session.UserId)
            .ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The catalog was not selected.";
            ShowState(OgViewState.Failed, "The catalog was not selected.", outcome.Reference);

            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("hse.ghg.select", "hse", outcome.Value.Jurisdiction ?? "(ghg)", "ok", session.UserId);

        _failure.Text = string.Empty;

        return true;
    }

    private void Apply(OgGhgWorkspace workspace)
    {
        _workspace = workspace;

        _scope.Text = workspace.ScopeText + " · " + workspace.ReadText;
        _catalog.Text = workspace.Catalog.Describe();
        _review.Text = workspace.Review.Sentence;

        // AC-1 and AC-5: the state says unavailable, and the reason says why - never a bare zero.
        _status.Text = workspace.StatusText;
        _unavailable.Text = workspace.UnavailableText;
        _total.Text = "Total: " + workspace.TotalText;

        _lineGrid.ItemsSource = workspace.Report?.Lines.ToList();
        _activityGrid.ItemsSource = workspace.Activity.ToList();

        // AC-6: the export is the report's own text, so it cannot disagree with the grid above it.
        _export.Text = workspace.ExportText;

        _failure.Text = string.Empty;

        ShowState(workspace.IsAvailable ? OgViewState.Ready : OgViewState.Empty, workspace.StatusText);
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
