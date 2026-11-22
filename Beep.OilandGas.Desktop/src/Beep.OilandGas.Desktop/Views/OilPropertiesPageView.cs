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
/// RES-02: oil properties - black-oil screening (bubble point, solution GOR, Bo, dead and saturated viscosity) with the
/// correlation named and the screening nature of the result stated on every state of the page.
/// </summary>
/// <remarks>
/// The banner is set once in the constructor and is never cleared, so it is present when the page is empty, when a run
/// succeeds, when a run is refused and when the history cannot be read (AC-4). A gas gravity is never supplied silently:
/// either the operator types one or ticks the explicit acceptance of the default (AC-3). A result outside the
/// correlation's range is still returned, marked and explained (AC-2). Saving records the inputs and the correlation
/// (AC-5).
/// </remarks>
public sealed class OilPropertiesPageView : OgPageBase
{
    public const string Route = "oilproperties";

    private readonly OgApplication _application;
    private readonly IOgOilProperties _oil;

    private readonly TextBlock _banner;
    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _results;
    private readonly TextBlock _range;
    private readonly TextBlock _defaultNote;
    private readonly TextBlock _detail;
    private readonly DataGrid _history;

    private readonly TextBox _api;
    private readonly TextBox _gasGravity;
    private readonly CheckBox _acceptDefault;
    private readonly TextBox _temperature;
    private readonly ComboBox _temperatureUnit;
    private readonly TextBox _pressure;
    private readonly TextBox _gor;
    private readonly TextBlock _correlationText;
    private readonly Button _runButton;
    private readonly Button _saveButton;

    private OgOilPropertyResult? _lastResult;
    private decimal _defaultGasGravity = 0.65m;

    public OilPropertiesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _oil = application.OilProperties;

        // AC-4: written once, here, and never assigned again anywhere in this type.
        _banner = Body(OilPropertyRules.ScreeningBanner);
        _banner.TextWrapping = TextWrapping.Wrap;
        _banner.FontWeight = FontWeights.SemiBold;
        _banner.Margin = new Thickness(0, 0, 0, 8);

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _results = Body(string.Empty);
        _results.TextWrapping = TextWrapping.Wrap;
        _results.Margin = new Thickness(0, 6, 0, 4);

        _range = Body(string.Empty);
        _range.TextWrapping = TextWrapping.Wrap;
        _range.Margin = new Thickness(0, 0, 0, 8);

        _defaultNote = Body(string.Empty);
        _defaultNote.TextWrapping = TextWrapping.Wrap;
        _defaultNote.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _api = Field("API gravity", 90);
        _api.Text = "35";

        _gasGravity = Field("Gas specific gravity", 110);

        _acceptDefault = new CheckBox
        {
            Content = "Accept the default gas gravity",
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };

        Named(_acceptDefault, "Accept the default gas gravity");

        _temperature = Field("Temperature", 100);
        _temperature.Text = "200";

        _temperatureUnit = new ComboBox
        {
            MinWidth = 90,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = OilUnits.TemperatureUnits,
            SelectedIndex = 0,
        };

        Named(_temperatureUnit, "Temperature unit");

        _pressure = Field("Pressure psia", 110);
        _pressure.Text = "3000";

        _gor = Field("Solution GOR scf/STB", 130);
        _gor.Text = "600";

        _correlationText = Body(OilCorrelations.Combined);
        _correlationText.VerticalAlignment = VerticalAlignment.Center;
        _correlationText.Margin = new Thickness(0, 0, 8, 0);
        _correlationText.ToolTip = "The correlation set this screen implements";

        _runButton = Button("Run", (_, _) => _ = RunAsync());
        _saveButton = Button("Save result", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;

        _history = new DataGrid
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
                Text("Saved", "SavedText", 200),
                Text("API", "ApiGravity", 60),
                Text("Pb", "BubblePointText", 110),
                Text("Rs", "SolutionGorText", 110),
                Text("Bo", "FormationVolumeFactorText", 110),
                Text("Viscosity", "SaturatedViscosityText", 100),
                Text("Range", "RangeText", 220),
            },
        };

        Named(_history, "Saved oil screening runs");
        _history.SelectionChanged += (_, _) => SelectHistory();

        var first = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        first.Children.Add(Caption("API"));
        first.Children.Add(_api);
        first.Children.Add(Caption("Gas gravity"));
        first.Children.Add(_gasGravity);
        first.Children.Add(_acceptDefault);

        var second = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        second.Children.Add(Caption("Temperature"));
        second.Children.Add(_temperature);
        second.Children.Add(_temperatureUnit);
        second.Children.Add(Caption("Pressure"));
        second.Children.Add(_pressure);
        second.Children.Add(Caption("Rs"));
        second.Children.Add(_gor);
        second.Children.Add(_correlationText);
        second.Children.Add(_runButton);
        second.Children.Add(_saveButton);

        var panel = new StackPanel();
        panel.Children.Add(_banner);
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Inputs"));
        panel.Children.Add(first);
        panel.Children.Add(second);
        panel.Children.Add(_missing);
        panel.Children.Add(_defaultNote);
        panel.Children.Add(Caption("Results"));
        panel.Children.Add(_results);
        panel.Children.Add(_range);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        _api.TextChanged += (_, _) => RefreshEnabled();
        _gasGravity.TextChanged += (_, _) => RefreshEnabled();
        _temperature.TextChanged += (_, _) => RefreshEnabled();
        _pressure.TextChanged += (_, _) => RefreshEnabled();
        _gor.TextChanged += (_, _) => RefreshEnabled();
        _acceptDefault.Checked += (_, _) => RefreshEnabled();
        _acceptDefault.Unchecked += (_, _) => RefreshEnabled();

        Load();
    }

    public OgOilPropertyWorkspace? Workspace { get; private set; }

    public OgOilPropertyResult? Result => _lastResult;

    public bool LastFailed { get; private set; }

    /// <summary>AC-4: the banner text, whatever state the page is in.</summary>
    public string ScreeningBannerText => _banner.Text;

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ResultsText => _results.Text;

    public string RangeText => _range.Text;

    public string DetailText => _detail.Text;

    public bool CanRun => OilPropertyRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _lastResult is { IsFailed: false, IsSaved: false };

    public bool HistoryUnavailable => Workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _oil.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Oil properties";
            _notice.Text = loaded.Message ?? "The oil screening history could not be read.";
            ShowState(OgViewState.Failed, "The oil screening history could not be read.", loaded.Reference);
            RefreshEnabled();

            // The banner is deliberately not touched here: AC-4 requires it on every state.
            return false;
        }

        Workspace = loaded.Value;
        _defaultGasGravity = loaded.Value.DefaultGasSpecificGravity;

        _temperatureUnit.ItemsSource = loaded.Value.TemperatureUnits.ToList();
        _temperatureUnit.SelectedIndex = _temperatureUnit.SelectedIndex < 0 ? 0 : _temperatureUnit.SelectedIndex;

        _history.ItemsSource = loaded.Value.History.ToList();

        _header.Text = "Oil properties · black-oil screening";

        _notice.Text = loaded.Value.HistoryFailed
            ? "The saved history is unavailable: " + loaded.Value.HistoryReason + ". New runs are still allowed."
            : loaded.Value.HistoryStateText;

        _defaultNote.Text = loaded.Value.DefaultGasGravityText;
        _acceptDefault.Content = $"Accept the default gas gravity ({_defaultGasGravity.ToString("0.####", CultureInfo.InvariantCulture)})";

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RunAsync()
    {
        var inputs = Inputs();

        if (OilPropertyRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var run = await _oil.RunAsync(inputs).ConfigureAwait(true);

        if (!run.Success || run.Value is null)
        {
            LastFailed = true;
            _lastResult = null;
            _results.Text = string.Empty;
            _range.Text = string.Empty;
            _notice.Text = run.Message ?? "The screening could not be run.";
            ShowState(OgViewState.Failed, "The screening could not be run.", run.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        _lastResult = run.Value;

        _results.Text =
            $"Pb {_lastResult.BubblePointText} · Rs at pressure {_lastResult.SolutionGorText} · " +
            $"Bo {_lastResult.FormationVolumeFactorText} · dead viscosity {_lastResult.DeadOilViscosityText} · " +
            $"saturated viscosity {_lastResult.SaturatedViscosityText} · density {_lastResult.DensityText}";

        _range.Text = _lastResult.RangeText;
        _detail.Text = _lastResult.Describe() + Environment.NewLine +
                       "gas gravity: " + _lastResult.GasGravityText + " · correlation: " + _lastResult.Correlation;

        _notice.Text = _lastResult.OutsideRange
            ? "The screening completed, but a result is outside the correlation's stated range; it is marked rather " +
              "than refused."
            : "Screening completed. Save it to keep its inputs and correlation.";

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_lastResult is not { IsFailed: false } result)
        {
            _notice.Text = "Run a successful screening before saving it.";
            return false;
        }

        var saved = await _oil.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The result could not be saved.";
            ShowState(OgViewState.Failed, "The result could not be saved.", saved.Reference);
            return false;
        }

        Workspace = saved.Value;
        _history.ItemsSource = saved.Value.History.ToList();
        _lastResult = result with { Actor = _application.Session.UserId, RecordedAt = DateTime.UtcNow };

        _application.AuditLog.Add(
            "properties.oil.save",
            "reservoir",
            result.Correlation,
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its inputs and correlation. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void SelectHistory()
    {
        if (_history.SelectedItem is not OgOilPropertyResult saved)
        {
            return;
        }

        _api.Text = saved.ApiGravity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _gasGravity.Text = saved.GasSpecificGravity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _acceptDefault.IsChecked = saved.GasGravityAssumed;
        _temperature.Text = saved.Temperature?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _temperatureUnit.SelectedItem = saved.TemperatureUnit;
        _pressure.Text = saved.Pressure?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _gor.Text = saved.SolutionGor?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        _detail.Text = saved.Describe();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = OilPropertyRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The screening needs " + string.Join(", ", missing) + ".";

        _runButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
    }

    private OgOilPropertyInputs Inputs() =>
        new(
            Parse(_api.Text),
            Parse(_gasGravity.Text),
            _acceptDefault.IsChecked == true,
            Parse(_temperature.Text),
            _temperatureUnit.SelectedItem as string,
            Parse(_pressure.Text),
            Parse(_gor.Text),
            OilCorrelations.Combined);

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
