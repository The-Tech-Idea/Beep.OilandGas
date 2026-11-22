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
/// RES-04: EOR screening - each method judged against the reservoir and fluid conditions, with the rule that decided it,
/// the module's own recovery factor, and the economics only as far as the module actually states them.
/// </summary>
/// <remarks>
/// Every method states the rule that decided eligibility (AC-1). A method whose window needs a value the inputs do not
/// carry is "insufficient data", never ineligible (AC-2). No threshold appears in this file: the windows, the recovery
/// factors and the chemical costs all come from the EnhancedRecovery module's constants (AC-3). The economics panel shows
/// a cost only where the module states one and says plainly that a cost per incremental barrel belongs to ECO-01 (AC-4).
/// </remarks>
public sealed class EorScreeningPageView : OgPageBase
{
    public const string Route = "eorscreening";

    private readonly OgApplication _application;
    private readonly IOgEorScreening _screening;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _economics;
    private readonly TextBlock _recommendation;
    private readonly TextBlock _detail;
    private readonly TextBox _field;
    private readonly TextBox _pool;
    private readonly TextBox _api;
    private readonly TextBox _viscosity;
    private readonly TextBox _depth;
    private readonly TextBox _temperature;
    private readonly ComboBox _chosen;
    private readonly DataGrid _candidates;
    private readonly DataGrid _history;
    private readonly Button _saveButton;

    private OgEorWorkspace? _workspace;

    public EorScreeningPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _screening = application.EorScreening;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _economics = Body(string.Empty);
        _economics.TextWrapping = TextWrapping.Wrap;
        _economics.Margin = new Thickness(0, 0, 0, 4);

        _recommendation = Body(string.Empty);
        _recommendation.TextWrapping = TextWrapping.Wrap;
        _recommendation.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _field = Field("Field id", 100);
        _pool = Field("Pool id", 100);
        _api = Field("API gravity", 90);
        _viscosity = Field("Oil viscosity cp", 110);
        _depth = Field("Depth ft", 100);
        _temperature = Field("Temperature F", 110);

        _chosen = new ComboBox { MinWidth = 170, Margin = new Thickness(0, 0, 8, 0) };
        Named(_chosen, "Method saved as this pool's scheme");

        _candidates = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 220,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Method", "MethodName", 150),
                Text("State", "StateText", 120),
                Text("Score", "ScoreText", 70),
                Text("Rule", "DecidingRule", 420),
                Text("Recovery", "RecoveryText", 170),
                Text("Cost", "CostText", 240),
            },
        };

        Named(_candidates, "Screened methods");

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Saved", "RecordedAt", 140),
                Text("Pool", "PoolId", 100),
                Text("API", "ApiGravity", 70),
                Text("Viscosity", "ViscosityCp", 90),
                Text("Eligible", "EligibleCount", 80),
                Text("Chosen", "ChosenMethod", 140),
                Text("By", "Actor", 110),
            },
        };

        Named(_history, "Saved screening runs");

        var where = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        where.Children.Add(Caption("Field"));
        where.Children.Add(_field);
        where.Children.Add(Caption("Pool"));
        where.Children.Add(_pool);

        var inputs = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        inputs.Children.Add(Caption("API"));
        inputs.Children.Add(_api);
        inputs.Children.Add(Caption("Viscosity"));
        inputs.Children.Add(_viscosity);
        inputs.Children.Add(Caption("Depth"));
        inputs.Children.Add(_depth);
        inputs.Children.Add(Caption("Temperature"));
        inputs.Children.Add(_temperature);
        inputs.Children.Add(Button("Screen methods", (_, _) => _ = ScreenAsync()));

        var saveBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        saveBar.Children.Add(Caption("Save as the pool's scheme"));
        saveBar.Children.Add(_chosen);
        _saveButton = Button("Save scheme", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        saveBar.Children.Add(_saveButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Pool and conditions"));
        panel.Children.Add(where);
        panel.Children.Add(inputs);
        panel.Children.Add(Caption("Screen"));
        panel.Children.Add(_summary);
        panel.Children.Add(_candidates);
        panel.Children.Add(Caption("Economics"));
        panel.Children.Add(_economics);
        panel.Children.Add(Caption("Recommendation"));
        panel.Children.Add(_recommendation);
        panel.Children.Add(saveBar);
        panel.Children.Add(Caption("Saved runs"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        _api.TextChanged += (_, _) => RefreshEnabled();
        _viscosity.TextChanged += (_, _) => RefreshEnabled();
        _depth.TextChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgEorWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string EconomicsText => _economics.Text;

    public string RecommendationText => _recommendation.Text;

    public string DetailText => _detail.Text;

    public bool CanSave => _workspace is { Candidates.Count: > 0 };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _screening.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "EOR screening";
            _notice.Text = loaded.Message ?? "The EOR screening history could not be read.";
            ShowState(OgViewState.Failed, "The EOR screening history could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        _workspace = loaded.Value;
        _history.ItemsSource = loaded.Value.History.ToList();

        _header.Text = "EOR screening · " + loaded.Value.MethodsText.Split(',')[0] + " and the other module methods";
        _notice.Text = loaded.Value.HistoryFailed
            ? "The saved history is unavailable: " + loaded.Value.HistoryReason + ". New screens are still allowed."
            : loaded.Value.HistoryStateText;

        _economics.Text = loaded.Value.EconomicsText;

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> ScreenAsync()
    {
        var inputs = new OgEorInputs(
            Empty(_field.Text),
            Empty(_pool.Text),
            Parse(_api.Text),
            Parse(_viscosity.Text),
            Parse(_depth.Text),
            Parse(_temperature.Text));

        var screened = await _screening.ScreenAsync(inputs).ConfigureAwait(true);

        if (!screened.Success || screened.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _candidates.ItemsSource = null;
            _summary.Text = string.Empty;
            _recommendation.Text = string.Empty;
            _notice.Text = screened.Message ?? "The screen could not be run.";
            ShowState(OgViewState.Failed, "The screen could not be run.", screened.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        _workspace = screened.Value;

        _candidates.ItemsSource = screened.Value.Candidates.ToList();
        _summary.Text = screened.Value.Summary;
        _economics.Text = screened.Value.EconomicsText;

        _chosen.ItemsSource = screened.Value.Candidates
            .Where(candidate => candidate.IsEligible)
            .Select(candidate => candidate.Method)
            .ToList();

        _chosen.SelectedIndex = _chosen.Items.Count > 0 ? 0 : -1;

        _recommendation.Text = screened.Value.Best is { } best
            ? $"Best of the eligible methods by this screen's own margin score: {best.MethodName} " +
              $"({best.ScoreText}); {best.RecoveryText}. The score orders the eligible methods, it is not a forecast."
            : screened.Value.InsufficientCount > 0 && screened.Value.EligibleCount == 0
                ? "No method could be judged: the screen is missing " +
                  "information the windows need. That is not the same as every method failing."
                : "No method is eligible at these conditions.";

        _detail.Text = screened.Value.CandidateText;
        _notice.Text = "Screened. Choose an eligible method and save it as the pool's scheme to keep the record.";
        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_workspace is not { Candidates.Count: > 0 } workspace)
        {
            _notice.Text = "Screen the pool before saving a scheme.";
            return false;
        }

        var saved = await _screening
            .SaveAsync(workspace, _chosen.SelectedItem as string, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The scheme could not be saved.";
            ShowState(OgViewState.Failed, "The scheme could not be saved.", saved.Reference);
            return false;
        }

        _workspace = saved.Value;
        _candidates.ItemsSource = saved.Value.Candidates.ToList();
        _history.ItemsSource = saved.Value.History.ToList();

        _application.AuditLog.Add(
            "reservoir.eor.save",
            "reservoir",
            saved.Value.RunId ?? "(no run)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved: the run, every candidate and its rule, and the chosen scheme. " +
                       saved.Value.HistoryStateText;

        RefreshEnabled();
        return true;
    }

    private void RefreshEnabled()
    {
        _saveButton.IsEnabled = CanSave;
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
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
