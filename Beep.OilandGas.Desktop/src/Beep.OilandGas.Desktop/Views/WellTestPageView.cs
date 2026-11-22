using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WTA-01: well test analysis - a build-up or drawdown fitted over the radial-flow window the engineer selects, with the
/// derivative's verdict and an independent Horner cross-check.
/// </summary>
/// <remarks>
/// The analysis type and method are recorded tokens (AC-2). A missing radial-flow window is refused with the window asked
/// for, never guessed (AC-3). A derivative that does not converge is printed as unusable and never as a series (SG-08-3).
/// The Horner cross-check shows both slopes and the difference (AC-4). The straight line and the derivative are printed
/// rather than drawn, because these screens have no chart host (AC-5, stated substitution).
/// </remarks>
public sealed class WellTestPageView : OgPageBase
{
    public const string Route = "welltest";

    private readonly OgApplication _application;
    private readonly IOgWellTest _wellTest;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _supported;
    private readonly TextBlock _summary;
    private readonly TextBlock _window;
    private readonly TextBlock _derivative;
    private readonly TextBlock _crossCheck;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _type;
    private readonly ComboBox _method;
    private readonly TextBox _timeUnit;
    private readonly TextBox _pressureUnit;
    private readonly TextBox _rateUnit;
    private readonly TextBox _windowStart;
    private readonly TextBox _windowEnd;
    private readonly TextBox _flowRate;
    private readonly TextBox _wellboreRadius;
    private readonly TextBox _thickness;
    private readonly TextBox _porosity;
    private readonly TextBox _compressibility;
    private readonly TextBox _viscosity;
    private readonly TextBox _fvf;
    private readonly TextBox _producingTime;
    private readonly TextBox _initialPressure;
    private readonly TextBox _readings;
    private readonly DataGrid _history;
    private readonly Button _analyseButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgWellTestWorkspace? _workspace;
    private OgWellTestResult? _result;

    public WellTestPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _wellTest = application.WellTests;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _supported = Body(string.Empty);
        _supported.TextWrapping = TextWrapping.Wrap;
        _supported.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _window = Body(string.Empty);
        _window.TextWrapping = TextWrapping.Wrap;
        _window.Margin = new Thickness(0, 0, 0, 4);

        _derivative = Body(string.Empty);
        _derivative.TextWrapping = TextWrapping.Wrap;
        _derivative.Margin = new Thickness(0, 0, 0, 4);

        _crossCheck = Body(string.Empty);
        _crossCheck.TextWrapping = TextWrapping.Wrap;
        _crossCheck.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _timeUnit = Field("Time unit", 80);
        _timeUnit.Text = "h";
        _pressureUnit = Field("Pressure unit", 90);
        _pressureUnit.Text = "psia";
        _rateUnit = Field("Rate unit", 90);
        _rateUnit.Text = "STB/d";
        _windowStart = Field("Window start h", 110);
        _windowStart.Text = "1";
        _windowEnd = Field("Window end h", 110);
        _windowEnd.Text = "60";
        _flowRate = Field("Flow rate", 110);
        _flowRate.Text = "500";
        _wellboreRadius = Field("Wellbore radius ft", 130);
        _wellboreRadius.Text = "0.35";
        _thickness = Field("Thickness ft", 110);
        _thickness.Text = "40";
        _porosity = Field("Porosity", 100);
        _porosity.Text = "0.2";
        _compressibility = Field("Total compressibility", 140);
        _compressibility.Text = "0.000012";
        _viscosity = Field("Viscosity cP", 110);
        _viscosity.Text = "1.5";
        _fvf = Field("Formation volume factor", 150);
        _fvf.Text = "1.2";
        _producingTime = Field("Producing time h", 130);
        _producingTime.Text = "200";
        _initialPressure = Field("Initial pressure psia", 140);
        _initialPressure.Text = "3000";

        _type = Picker("Analysis type", 150);
        _method = Picker("Method", 120);

        _readings = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "test time (h), pressure (psia) - one reading per line",
        };

        Named(_readings, "Pressure-time readings, one per line");
        _readings.Text =
            "# time h, pressure psia" + Environment.NewLine +
            "1, 2842.48" + Environment.NewLine +
            "2, 2820.45" + Environment.NewLine +
            "4, 2798.42" + Environment.NewLine +
            "8, 2776.39" + Environment.NewLine +
            "12, 2764.32" + Environment.NewLine +
            "20, 2748.99" + Environment.NewLine +
            "30, 2737.08" + Environment.NewLine +
            "45, 2723.71" + Environment.NewLine +
            "60, 2713.72";

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
                Text("Well", "WellId", 100),
                Text("Type", "AnalysisType", 100),
                Text("Method", "Method", 90),
                Text("k (md)", "PermeabilityText", 100),
                Text("Skin", "SkinText", 80),
                Text("Window", "WindowText", 190),
                Text("Cross-check", "CrossCheck", 130),
            },
        };

        Named(_history, "Saved well test analyses");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _analyseButton = Button("Analyse", (_, _) => _ = AnalyseAsync());
        _saveButton = Button("Save result", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var testBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        testBar.Children.Add(Caption("Well"));
        testBar.Children.Add(_well);
        testBar.Children.Add(Caption("Type"));
        testBar.Children.Add(_type);
        testBar.Children.Add(Caption("Method"));
        testBar.Children.Add(_method);

        var unitBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        unitBar.Children.Add(Caption("Time"));
        unitBar.Children.Add(_timeUnit);
        unitBar.Children.Add(Caption("Pressure"));
        unitBar.Children.Add(_pressureUnit);
        unitBar.Children.Add(Caption("Rate"));
        unitBar.Children.Add(_rateUnit);

        var windowBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        windowBar.Children.Add(Caption("Radial-flow window"));
        windowBar.Children.Add(_windowStart);
        windowBar.Children.Add(Caption("to"));
        windowBar.Children.Add(_windowEnd);
        windowBar.Children.Add(Caption("Flow rate"));
        windowBar.Children.Add(_flowRate);

        var rockBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        rockBar.Children.Add(Caption("rw"));
        rockBar.Children.Add(_wellboreRadius);
        rockBar.Children.Add(Caption("h"));
        rockBar.Children.Add(_thickness);
        rockBar.Children.Add(Caption("phi"));
        rockBar.Children.Add(_porosity);
        rockBar.Children.Add(Caption("ct"));
        rockBar.Children.Add(_compressibility);

        var fluidBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fluidBar.Children.Add(Caption("mu"));
        fluidBar.Children.Add(_viscosity);
        fluidBar.Children.Add(Caption("B"));
        fluidBar.Children.Add(_fvf);
        fluidBar.Children.Add(Caption("tp"));
        fluidBar.Children.Add(_producingTime);
        fluidBar.Children.Add(Caption("pi"));
        fluidBar.Children.Add(_initialPressure);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_analyseButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_supported);
        panel.Children.Add(Caption("Test"));
        panel.Children.Add(testBar);
        panel.Children.Add(Caption("Units"));
        panel.Children.Add(unitBar);
        panel.Children.Add(Caption("Window and rate"));
        panel.Children.Add(windowBar);
        panel.Children.Add(Caption("Reservoir and rock"));
        panel.Children.Add(rockBar);
        panel.Children.Add(Caption("Fluid and time"));
        panel.Children.Add(fluidBar);
        panel.Children.Add(Caption("Readings"));
        panel.Children.Add(_readings);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_window);
        panel.Children.Add(_derivative);
        panel.Children.Add(Caption("Cross-check"));
        panel.Children.Add(_crossCheck);
        panel.Children.Add(Caption("Saved analyses"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _timeUnit, _pressureUnit, _rateUnit, _windowStart, _windowEnd, _flowRate,
                     _wellboreRadius, _thickness, _porosity, _compressibility, _viscosity, _fvf, _producingTime,
                     _initialPressure, _readings,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _type.SelectionChanged += (_, _) => RefreshEnabled();
        _method.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgWellTestWorkspace? Workspace => _workspace;

    public OgWellTestResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string SupportedText => _supported.Text;

    public string SummaryText => _summary.Text;

    public string WindowText => _window.Text;

    public string DerivativeText => _derivative.Text;

    public string CrossCheckText => _crossCheck.Text;

    public string DetailText => _detail.Text;

    public bool CanAnalyse => WellTestRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, Converged: true };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _wellTest.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Well test";
            _notice.Text = loaded.Message ?? "The saved analyses could not be read.";
            ShowState(OgViewState.Failed, "The saved analyses could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> AnalyseAsync()
    {
        var inputs = Inputs();

        if (WellTestRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var analysed = await _wellTest.AnalyseAsync(inputs).ConfigureAwait(true);

        if (!analysed.Success || analysed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _window.Text = string.Empty;
            _derivative.Text = string.Empty;
            _crossCheck.Text = string.Empty;
            _notice.Text = analysed.Message ?? "The analysis could not be run.";
            ShowState(OgViewState.Failed, "The analysis could not be run.", analysed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(analysed.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Run an analysis that converges before saving it.";
            return false;
        }

        var saved = await _wellTest.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The result was not saved.";
            ShowState(OgViewState.Failed, "The result was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "welltest.analysis.save",
            "welltest",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its window, method and cross-check. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _wellTest.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!retried.Success || retried.Value is null)
        {
            LastFailed = true;
            _notice.Text = retried.Message ?? "There is no result waiting to be retried.";
            ShowState(OgViewState.Failed, "There is no result waiting to be retried.", retried.Reference);
            return false;
        }

        Apply(retried.Value);
        _notice.Text = "The retained result was saved on the retry.";
        RefreshEnabled();
        return true;
    }

    private void Apply(OgWellTestWorkspace workspace)
    {
        _workspace = workspace;

        _type.ItemsSource = workspace.Types.ToList();
        _type.SelectedIndex = _type.SelectedIndex < 0 ? 0 : _type.SelectedIndex;

        _method.ItemsSource = WellTestAnalyses.MethodsFor(_type.SelectedItem as string).ToList();
        _method.SelectedIndex = _method.SelectedIndex < 0 ? 0 : _method.SelectedIndex;

        _history.ItemsSource = workspace.Results.ToList();

        _header.Text = "Well test · pressure transient analysis";
        _supported.Text = workspace.SupportedText + " · " + workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved analyses are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgWellTestResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no analysis is selected";
            _window.Text = string.Empty;
            _derivative.Text = string.Empty;
            _crossCheck.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text =
            $"{result.Describe()} · p* {result.PressureText} · slope {result.SlopeText} · {result.FitText}";
        _window.Text = "Radial-flow window: " + result.WindowText + " · model " + result.ModelText;
        _derivative.Text = result.DerivativeText;
        _crossCheck.Text = result.CrossCheck.AgreeText + " · " + result.CrossCheck.Text;
        _detail.Text = "units " + result.UnitsText + " · " + result.SavedText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgWellTestResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = WellTestRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The analysis needs " + string.Join(", ", missing) + ".";

        var methods = WellTestAnalyses.MethodsFor(_type.SelectedItem as string);

        if (_method.ItemsSource is not null && methods.Count != _method.Items.Count)
        {
            _method.ItemsSource = methods.ToList();
            _method.SelectedIndex = 0;
        }

        _analyseButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _wellTest.Pending is not null;
    }

    private OgWellTestInputs Inputs() =>
        new(
            Empty(_well.Text),
            _type.SelectedItem as string,
            _method.SelectedItem as string,
            Empty(_timeUnit.Text),
            Empty(_pressureUnit.Text),
            Empty(_rateUnit.Text),
            Readings(),
            Parse(_windowStart.Text),
            Parse(_windowEnd.Text),
            Parse(_flowRate.Text),
            Parse(_wellboreRadius.Text),
            Parse(_thickness.Text),
            Parse(_porosity.Text),
            Parse(_compressibility.Text),
            Parse(_viscosity.Text),
            Parse(_fvf.Text),
            Parse(_producingTime.Text),
            Parse(_initialPressure.Text));

    /// <summary>Reads the readings editor: one <c>time, pressure</c> pair per line; <c>#</c> starts a comment.</summary>
    private IReadOnlyList<OgWellTestReading> Readings()
    {
        var readings = new List<OgWellTestReading>();

        foreach (var line in (_readings.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 2
                || !decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var time)
                || !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var pressure))
            {
                continue;
            }

            readings.Add(new OgWellTestReading(time, pressure));
        }

        return readings;
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

    private static ComboBox Picker(string name, double width)
    {
        var picker = new ComboBox
        {
            MinWidth = width,
            Margin = new Thickness(0, 0, 8, 0),
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
