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
/// RES-01: gas properties - Z-factor, viscosity, density, formation volume factor and pseudo-pressure, with the
/// correlation named and the inputs preserved so any result can be reproduced.
/// </summary>
/// <remarks>
/// The run button is disabled with the missing field named rather than calling the engine with a guessed value (the state
/// table). A correlation the engine refuses is a failed run with the reason, never a number (AC-3). The correlation is
/// displayed beside every result (AC-2). The units are converted in one place and both the typed value and the converted
/// psia/Rankine pair are recorded (AC-4). Saving records the inputs, the correlations and the actor (AC-5), and the same
/// inputs always produce the same result (AC-6).
/// </remarks>
public sealed class GasPropertiesPageView : OgPageBase
{
    public const string Route = "gasproperties";

    private readonly OgApplication _application;
    private readonly IOgGasProperties _gas;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _results;
    private readonly TextBlock _correlation;
    private readonly TextBlock _engine;
    private readonly TextBlock _warnings;
    private readonly TextBlock _detail;
    private readonly DataGrid _history;

    private readonly TextBox _pressure;
    private readonly ComboBox _pressureUnit;
    private readonly TextBox _temperature;
    private readonly ComboBox _temperatureUnit;
    private readonly TextBox _gravity;
    private readonly ComboBox _zCorrelation;
    private readonly ComboBox _viscosityCorrelation;
    private readonly Button _runButton;
    private readonly Button _save;

    private OgGasPropertyRun? _lastRun;

    public GasPropertiesPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gas = application.GasProperties;

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

        _correlation = Body(string.Empty);
        _correlation.TextWrapping = TextWrapping.Wrap;
        _correlation.Margin = new Thickness(0, 0, 0, 4);

        _engine = Body(string.Empty);
        _engine.TextWrapping = TextWrapping.Wrap;
        _engine.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _pressure = Field("Pressure", 100);
        _temperature = Field("Temperature", 100);
        _gravity = Field("Specific gravity", 90);
        _gravity.Text = "0.65";

        _pressureUnit = Picker("Pressure unit", GasUnits.PressureUnits, 90);
        _temperatureUnit = Picker("Temperature unit", GasUnits.TemperatureUnits, 90);

        _zCorrelation = Picker("Z correlation", GasCorrelations.Z, 160);
        _viscosityCorrelation = Picker("Viscosity correlation", GasCorrelations.Viscosity, 190);

        _runButton = Button("Run", (_, _) => _ = RunAsync());
        _save = Button("Save result", (_, _) => _ = SaveAsync());
        _save.IsEnabled = false;

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
                Text("Saved", "SavedText", 210),
                Text("Status", "StatusText", 80),
                Text("Pressure", "PressureText", 120),
                Text("Temperature", "TemperatureText", 120),
                Text("Z", "ZText", 100),
                Text("Viscosity", "ViscosityText", 110),
                Text("Correlations", "CorrelationText", 260),
            },
        };

        Named(_history, "Saved gas-property runs");
        _history.SelectionChanged += (_, _) => SelectHistory();

        var inputs = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        inputs.Children.Add(Caption("Pressure"));
        inputs.Children.Add(_pressure);
        inputs.Children.Add(_pressureUnit);
        inputs.Children.Add(Caption("Temperature"));
        inputs.Children.Add(_temperature);
        inputs.Children.Add(_temperatureUnit);

        var correlations = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        correlations.Children.Add(Caption("Specific gravity"));
        correlations.Children.Add(_gravity);
        correlations.Children.Add(_zCorrelation);
        correlations.Children.Add(_viscosityCorrelation);
        correlations.Children.Add(_runButton);
        correlations.Children.Add(_save);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Inputs"));
        panel.Children.Add(inputs);
        panel.Children.Add(correlations);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Results"));
        panel.Children.Add(_results);
        panel.Children.Add(_correlation);
        panel.Children.Add(_engine);
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        _pressure.TextChanged += (_, _) => RefreshEnabled();
        _temperature.TextChanged += (_, _) => RefreshEnabled();
        _gravity.TextChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgResult<OgGasPropertyWorkspace>? Workspace { get; private set; }

    public OgGasPropertyRun? Run => _lastRun;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ResultsText => _results.Text;

    public string CorrelationText => _correlation.Text;

    public string EngineText => _engine.Text;

    public string WarningsText => _warnings.Text;

    public string DetailText => _detail.Text;

    /// <summary>The state table's "missing input": the run is disabled until every input is present.</summary>
    public bool CanRun => GasPropertyRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _lastRun is { IsFailed: false, IsSaved: false };

    /// <summary>True when the saved history could not be read (the page still allows new runs).</summary>
    public bool HistoryUnavailable => Workspace?.Value?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _gas.LoadAsync().GetAwaiter().GetResult();

        Workspace = loaded;

        if (loaded.Value is not null)
        {
            _zCorrelation.ItemsSource = loaded.Value.ZCorrelations.ToList();
            _viscosityCorrelation.ItemsSource = loaded.Value.ViscosityCorrelations.ToList();
            _pressureUnit.ItemsSource = loaded.Value.PressureUnits.ToList();
            _temperatureUnit.ItemsSource = loaded.Value.TemperatureUnits.ToList();

            _zCorrelation.SelectedIndex = _zCorrelation.SelectedIndex < 0 ? 0 : _zCorrelation.SelectedIndex;
            _viscosityCorrelation.SelectedIndex =
                _viscosityCorrelation.SelectedIndex < 0 ? 0 : _viscosityCorrelation.SelectedIndex;
            _pressureUnit.SelectedIndex = _pressureUnit.SelectedIndex < 0 ? 0 : _pressureUnit.SelectedIndex;
            _temperatureUnit.SelectedIndex = _temperatureUnit.SelectedIndex < 0 ? 0 : _temperatureUnit.SelectedIndex;

            _history.ItemsSource = loaded.Value.History.ToList();

            _notice.Text = loaded.Value.HistoryFailed
                ? "The saved history is unavailable: " + loaded.Value.HistoryReason +
                  ". New runs are still allowed."
                : loaded.Value.HistoryStateText + " · " + loaded.Value.DeterminismText;

            ShowState(OgViewState.Ready, _notice.Text);
        }
        else
        {
            LastFailed = true;
            _history.ItemsSource = null;
            _notice.Text = loaded.Message ?? "The gas-property history could not be read.";
            ShowState(OgViewState.Failed, "The gas-property history could not be read.", loaded.Reference);
        }

        RefreshEnabled();
        return loaded.Success;
    }

    public async Task<bool> RunAsync()
    {
        var inputs = Inputs();

        if (GasPropertyRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var run = await _gas.RunAsync(inputs).ConfigureAwait(true);

        if (!run.Success || run.Value is null)
        {
            LastFailed = true;
            _lastRun = null;
            _results.Text = string.Empty;
            _correlation.Text = string.Empty;
            _engine.Text = string.Empty;
            _warnings.Text = string.Empty;
            _notice.Text = run.Message ?? "The calculation could not be run.";
            ShowState(OgViewState.Failed, "The calculation could not be run.", run.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        _lastRun = run.Value;

        _results.Text = _lastRun.IsFailed
            ? $"FAILED · {_lastRun.FailureText}"
            : $"Z {_lastRun.ZText} · viscosity {_lastRun.ViscosityText} · density {_lastRun.DensityText} · " +
              $"Bg {_lastRun.FormationVolumeFactorText} · pseudo-pressure {_lastRun.PseudoPressureText}";

        _correlation.Text = "Correlation used: " + _lastRun.CorrelationText;
        _engine.Text = _lastRun.EngineText;
        _warnings.Text = "Applicability: " + _lastRun.WarningsText;
        _detail.Text = _lastRun.Describe();

        _notice.Text = _lastRun.IsFailed
            ? "The calculation did not produce a result; it is recorded as a failed run, not as a number."
            : "Calculated. Save the result to keep its inputs, correlations and actor.";

        ShowState(_lastRun.IsFailed ? OgViewState.Failed : OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return !_lastRun.IsFailed;
    }

    public async Task<bool> SaveAsync()
    {
        if (_lastRun is not { IsFailed: false } run)
        {
            _notice.Text = "Run a successful calculation before saving it.";
            return false;
        }

        var saved = await _gas
            .SaveAsync(run, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The result could not be saved.";
            ShowState(OgViewState.Failed, "The result could not be saved.", saved.Reference);
            return false;
        }

        Workspace = saved;
        _history.ItemsSource = saved.Value.History.ToList();
        _lastRun = run with { Actor = _application.Session.UserId, RecordedAt = DateTime.UtcNow };

        _application.AuditLog.Add(
            "properties.gas.save",
            "reservoir",
            run.ZCorrelation,
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its inputs, correlations and actor. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void SelectHistory()
    {
        if (_history.SelectedItem is not OgGasPropertyRun saved)
        {
            return;
        }

        // "Load inputs" to rerun: the saved run restores the values it was made with.
        _pressure.Text = saved.Pressure?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _pressureUnit.SelectedItem = saved.PressureUnit;
        _temperature.Text = saved.Temperature?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _temperatureUnit.SelectedItem = saved.TemperatureUnit;
        _gravity.Text = saved.SpecificGravity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _zCorrelation.SelectedItem = saved.ZCorrelation;
        _viscosityCorrelation.SelectedItem = saved.ViscosityCorrelation;

        _detail.Text = saved.Describe();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = GasPropertyRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The run needs " + string.Join(", ", missing) + ".";

        _runButton.IsEnabled = missing.Count == 0;
        _save.IsEnabled = CanSave;
    }

    private OgGasPropertyInputs Inputs() =>
        new(
            Parse(_pressure.Text),
            _pressureUnit.SelectedItem as string,
            Parse(_temperature.Text),
            _temperatureUnit.SelectedItem as string,
            Parse(_gravity.Text),
            _zCorrelation.SelectedItem as string,
            _viscosityCorrelation.SelectedItem as string);

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

    private static ComboBox Picker(string name, IReadOnlyList<string> items, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = items,
            SelectedIndex = 0,
        };

        Named(picker, name);
        return picker;
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
