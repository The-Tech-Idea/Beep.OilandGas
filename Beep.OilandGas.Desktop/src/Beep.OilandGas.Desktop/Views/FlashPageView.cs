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
/// RES-03: the isothermal flash and the phase envelope, with convergence reported as the engine reported it.
/// </summary>
/// <remarks>
/// A run the solver did not converge is a failed run carrying its iteration count, and no composition is shown from it
/// (AC-2). The mole fractions must sum to one within the engine's own tolerance (AC-3). The equation of state is chosen
/// from the codes the mapping supplies, so the view carries none of them as literals (AC-4). The envelope shows only the
/// points the solver computed and states what it left out (AC-5). Same inputs, same output (AC-6).
/// </remarks>
public sealed class FlashPageView : OgPageBase
{
    public const string Route = "flash";

    private readonly OgApplication _application;
    private readonly IOgFlash _flash;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _eosSource;
    private readonly TextBlock _summary;
    private readonly TextBlock _failure;
    private readonly TextBlock _stability;
    private readonly TextBlock _envelopeText;
    private readonly TextBlock _envelopeLimit;
    private readonly TextBlock _detail;
    private readonly TextBox _feed;
    private readonly TextBox _pressure;
    private readonly TextBox _temperature;
    private readonly ComboBox _eos;
    private readonly DataGrid _composition;
    private readonly DataGrid _history;
    private readonly TextBox _minTemperature;
    private readonly TextBox _maxTemperature;
    private readonly TextBox _steps;
    private readonly Button _runButton;
    private readonly Button _saveButton;

    private OgFlashRun? _lastRun;
    private OgFlashEnvelope? _envelope;

    public FlashPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _flash = application.Flash;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _eosSource = Body(string.Empty);
        _eosSource.TextWrapping = TextWrapping.Wrap;
        _eosSource.Margin = new Thickness(0, 4, 0, 8);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 6, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.Margin = new Thickness(0, 0, 0, 4);

        _stability = Body(string.Empty);
        _stability.TextWrapping = TextWrapping.Wrap;
        _stability.Margin = new Thickness(0, 0, 0, 8);

        _envelopeText = Body(string.Empty);
        _envelopeText.TextWrapping = TextWrapping.Wrap;
        _envelopeText.Margin = new Thickness(0, 6, 0, 4);

        _envelopeLimit = Body(string.Empty);
        _envelopeLimit.TextWrapping = TextWrapping.Wrap;
        _envelopeLimit.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _feed = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 120,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Text = FlashFeedText.DefaultText,
        };

        Named(_feed, "Feed composition, one component per line");
        _feed.ToolTip = "name, z, Tc (R), Pc (psia), omega, MW";

        _pressure = Field("Pressure psia", 110);
        _pressure.Text = "500";

        _temperature = Field("Temperature R", 110);
        _temperature.Text = "600";

        _eos = new ComboBox { MinWidth = 140, Margin = new Thickness(0, 0, 8, 0) };
        Named(_eos, "Equation of state");

        _minTemperature = Field("Envelope from R", 110);
        _minTemperature.Text = "500";

        _maxTemperature = Field("Envelope to R", 110);
        _maxTemperature.Text = "1000";

        _steps = Field("Envelope steps", 80);
        _steps.Text = "20";

        _runButton = Button("Run flash", (_, _) => _ = RunAsync());
        _saveButton = Button("Save run", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;

        _composition = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Component", "Name", 110),
                Text("z", "Z", 80),
                Text("K", "K", 100),
                Text("x (liquid)", "X", 100),
                Text("y (vapour)", "Y", 100),
            },
        };

        Named(_composition, "Phase composition");

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Saved", "SavedText", 200),
                Text("EOS", "EosCode", 90),
                Text("Vapour", "VapourFractionText", 100),
                Text("Iterations", "IterationsText", 110),
                Text("Status", "StatusCode", 150),
                Text("Phase", "PhaseState", 110),
            },
        };

        Named(_history, "Saved flash runs");
        _history.SelectionChanged += (_, _) => SelectHistory();

        var conditions = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        conditions.Children.Add(Caption("Pressure"));
        conditions.Children.Add(_pressure);
        conditions.Children.Add(Caption("Temperature"));
        conditions.Children.Add(_temperature);
        conditions.Children.Add(Caption("EOS"));
        conditions.Children.Add(_eos);
        conditions.Children.Add(_runButton);
        conditions.Children.Add(_saveButton);

        var envelopeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        envelopeBar.Children.Add(Caption("Envelope from"));
        envelopeBar.Children.Add(_minTemperature);
        envelopeBar.Children.Add(Caption("to"));
        envelopeBar.Children.Add(_maxTemperature);
        envelopeBar.Children.Add(Caption("steps"));
        envelopeBar.Children.Add(_steps);
        envelopeBar.Children.Add(Button("Compute envelope", (_, _) => _ = EnvelopeAsync()));

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_eosSource);
        panel.Children.Add(Caption("Feed composition"));
        panel.Children.Add(_feed);
        panel.Children.Add(conditions);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_failure);
        panel.Children.Add(_stability);
        panel.Children.Add(Caption("Phase composition"));
        panel.Children.Add(_composition);
        panel.Children.Add(Caption("Phase envelope"));
        panel.Children.Add(envelopeBar);
        panel.Children.Add(_envelopeText);
        panel.Children.Add(_envelopeLimit);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        _feed.TextChanged += (_, _) => RefreshEnabled();
        _pressure.TextChanged += (_, _) => RefreshEnabled();
        _temperature.TextChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgFlashWorkspace? Workspace { get; private set; }

    public OgFlashRun? Run => _lastRun;

    public OgFlashEnvelope? Envelope => _envelope;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string EosSourceText => _eosSource.Text;

    public string SummaryText => _summary.Text;

    public string FailureText => _failure.Text;

    public string StabilityText => _stability.Text;

    public string EnvelopeText => _envelopeText.Text;

    public string EnvelopeLimitText => _envelopeLimit.Text;

    public string DetailText => _detail.Text;

    public bool CanRun => _lastRun is null || !_lastRun.IsFailed || true;

    public bool CanSave => _lastRun is { IsFailed: false, IsSaved: false };

    public bool HistoryUnavailable => Workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _flash.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            Workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Flash calculations";
            _notice.Text = loaded.Message ?? "The flash history could not be read.";
            ShowState(OgViewState.Failed, "The flash history could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Workspace = loaded.Value;

        _eos.ItemsSource = loaded.Value.EosCodes.ToList();
        _eos.SelectedIndex = _eos.SelectedIndex < 0 ? 0 : _eos.SelectedIndex;

        _history.ItemsSource = loaded.Value.History.ToList();

        _header.Text = "Flash calculations · isothermal flash and the phase envelope";
        _eosSource.Text = loaded.Value.EosText;
        _notice.Text = loaded.Value.HistoryFailed
            ? "The saved history is unavailable: " + loaded.Value.HistoryReason + ". New runs are still allowed."
            : loaded.Value.HistoryStateText;

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RunAsync()
    {
        if (!FlashFeedText.TryParse(_feed.Text, out var components, out var feedError))
        {
            LastFailed = true;
            _notice.Text = feedError ?? "The feed could not be read.";
            ShowState(OgViewState.Failed, "The feed could not be read.", FlashRules.CodeFeedLineInvalid);
            return false;
        }

        var inputs = Inputs(components);

        var run = await _flash.RunAsync(inputs).ConfigureAwait(true);

        if (!run.Success || run.Value is null)
        {
            LastFailed = true;
            _lastRun = null;
            _composition.ItemsSource = null;
            _summary.Text = string.Empty;
            _failure.Text = string.Empty;
            _stability.Text = string.Empty;
            _notice.Text = run.Message ?? "The flash could not be run.";
            ShowState(OgViewState.Failed, "The flash could not be run.", run.Reference);
            RefreshEnabled();
            return false;
        }

        LastFailed = false;
        _lastRun = run.Value;

        _composition.ItemsSource = _lastRun.Components.ToList();
        _summary.Text = _lastRun.Describe();
        _failure.Text = _lastRun.IsFailed ? _lastRun.FailureText : string.Empty;
        _stability.Text = string.IsNullOrWhiteSpace(_lastRun.StabilityText)
            ? string.Empty
            : "Stability: " + _lastRun.StabilityText +
              (string.Equals(_lastRun.PhaseState, FlashPhaseStates.AllVapour, StringComparison.Ordinal)
                  ? " - a single vapour phase, so there is no phase split to show."
                  : string.Empty);

        _notice.Text = _lastRun.IsFailed
            ? "The run did not converge; it is recorded as failed with its iteration count and no composition is " +
              "shown."
            : _lastRun.IsTwoPhase
                ? "Two phases. Save the run to keep its feed, model and actor."
                : "A single phase at these conditions. Save the run to keep its feed, model and actor.";

        _detail.Text = _lastRun.Describe();
        ShowState(_lastRun.IsFailed ? OgViewState.Failed : OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return !_lastRun.IsFailed;
    }

    public async Task<bool> EnvelopeAsync()
    {
        if (!FlashFeedText.TryParse(_feed.Text, out var components, out var feedError))
        {
            _notice.Text = feedError ?? "The feed could not be read.";
            return false;
        }

        var built = await _flash
            .EnvelopeAsync(
                Inputs(components),
                Parse(_minTemperature.Text) ?? 400m,
                Parse(_maxTemperature.Text) ?? 1200m,
                (int)(Parse(_steps.Text) ?? 20m))
            .ConfigureAwait(true);

        if (!built.Success || built.Value is null)
        {
            _envelope = null;
            _envelopeText.Text = string.Empty;
            _envelopeLimit.Text = string.Empty;
            _notice.Text = built.Message ?? "The envelope could not be computed.";
            return false;
        }

        _envelope = built.Value;
        _envelopeText.Text = built.Value.Text;
        _envelopeLimit.Text = built.Value.LimitText;

        _notice.Text =
            $"Envelope computed with {built.Value.EosUsed}: " +
            $"{built.Value.Points.Count.ToString(CultureInfo.InvariantCulture)} point(s), " +
            $"cricondentherm {built.Value.CricondenthermR.ToString("#,0.##", CultureInfo.InvariantCulture)} R, " +
            $"cricondenbar {built.Value.CricondenbarPsia.ToString("#,0.##", CultureInfo.InvariantCulture)} psia.";

        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_lastRun is not { IsFailed: false } run)
        {
            _notice.Text = "Run a converged flash before saving it.";
            return false;
        }

        var saved = await _flash.SaveAsync(run, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The run could not be saved.";
            ShowState(OgViewState.Failed, "The run could not be saved.", saved.Reference);
            return false;
        }

        Workspace = saved.Value;
        _history.ItemsSource = saved.Value.History.ToList();
        _lastRun = run with { Actor = _application.Session.UserId, RecordedAt = DateTime.UtcNow };

        _application.AuditLog.Add(
            "properties.flash.save",
            "reservoir",
            run.EosCode,
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its feed, model and actor. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    private void SelectHistory()
    {
        if (_history.SelectedItem is not OgFlashRun saved)
        {
            return;
        }

        _eos.SelectedItem = saved.EosCode;
        _pressure.Text = saved.Pressure?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _temperature.Text = saved.Temperature?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        if (saved.Feed.Count > 0)
        {
            _feed.Text = string.Join(
                Environment.NewLine,
                saved.Feed.Select(component =>
                    $"{component.Name}, {component.MoleFraction.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{component.CriticalTemperature.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{component.CriticalPressure.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{component.AcentricFactor.ToString(CultureInfo.InvariantCulture)}, " +
                    $"{component.MolecularWeight.ToString(CultureInfo.InvariantCulture)}"));
        }

        _detail.Text = saved.Describe();
        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        _runButton.IsEnabled = true;
        _saveButton.IsEnabled = CanSave;
    }

    private OgFlashInputs Inputs(IReadOnlyList<OgFlashComponent> components) =>
        new(
            components,
            Parse(_pressure.Text),
            Parse(_temperature.Text),
            _eos.SelectedItem as string);

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
