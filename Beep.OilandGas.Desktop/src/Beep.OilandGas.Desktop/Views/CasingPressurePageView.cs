using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-04: classify annular casing pressure against API RP 90 and show the MAWOP and MAASP that decide whether the pressure is
/// safe - each with the basis it was computed from.
/// </summary>
/// <remarks>
/// The class and the rule that decided it come from the owner's diagnostic (AC-1); MAWOP and MAASP are shown per annulus with
/// their governing term and every limit (AC-2); a pressure above the MAASP is CRITICAL whatever the class says (AC-3); a test with
/// no valve-closed time is accepted and marked incomplete (AC-4); the trend is the recorded tests (AC-5); and every threshold is
/// the configured one (AC-6).
/// </remarks>
public sealed class CasingPressurePageView : OgPageBase
{
    public const string Route = "casingpressure";

    private readonly OgApplication _application;
    private readonly IOgCasingPressure _pressure;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _annulus;
    private readonly TextBlock _limits;
    private readonly TextBlock _classification;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _bleedStart;
    private readonly TextBox _valveClosed;
    private readonly TextBox _temperature;
    private readonly TextBox _temperatureUnit;
    private readonly TextBox _bleedOff;
    private readonly TextBox _readings;
    private readonly ComboBox _annulusPicker;
    private readonly DataGrid _historyGrid;
    private readonly Button _loadButton;
    private readonly Button _recordButton;

    private OgCasingPressureWorkspace? _workspace;

    public CasingPressurePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _pressure = application.CasingPressure;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _annulus = Body(string.Empty);
        _annulus.TextWrapping = TextWrapping.Wrap;
        _annulus.Margin = new Thickness(0, 0, 0, 4);

        _limits = Body(string.Empty);
        _limits.TextWrapping = TextWrapping.Wrap;
        _limits.Margin = new Thickness(0, 0, 0, 4);

        _classification = Body(string.Empty);
        _classification.TextWrapping = TextWrapping.Wrap;
        _classification.FontWeight = FontWeights.SemiBold;
        _classification.Margin = new Thickness(0, 4, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _well = Field("Well UWI", 140);
        _bleedStart = Field("Bleed start yyyy-MM-dd HH:mm", 170);
        _valveClosed = Field("Valve closed yyyy-MM-dd HH:mm", 170);
        _temperature = Field("Temperature", 100);
        _temperatureUnit = Field("Unit", 60);
        _bleedOff = Field("Bleed-off behaviour", 260);

        _bleedOff.Text = "bled to zero within the allowance";
        _temperatureUnit.Text = "degC";

        _annulusPicker = new ComboBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        Named(_annulusPicker, "Annulus");

        _readings = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 80,
            MaxHeight = 130,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "One reading per line: yyyy-MM-dd HH:mm, pressure, unit",
        };

        Named(_readings, "Readings");

        _historyGrid = Grid("History", ("Tested", "At", 130), ("Pressure", "MaxPressure", 90),
            ("Unit", "PressureUnit", 60), ("Duration h", "DurationHours", 90), ("Temperature", "Temperature", 90),
            ("Class", "Class", 130), ("Severity", "Severity", 90), ("State", "StateText", 150),
            ("By", "RecordedBy", 100));

        _annulusPicker.SelectionChanged += (_, _) =>
        {
            if (_annulusPicker.SelectedItem is string chosen &&
                !string.Equals(_workspace?.Selected?.Name, chosen, StringComparison.OrdinalIgnoreCase))
            {
                Read(chosen);
            }
        };

        _loadButton = Button("Load annuli", (_, _) => Read(null));
        _recordButton = Button("Record test", (_, _) => _ = RecordAsync());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Annulus"));
        wellBar.Children.Add(_annulusPicker);
        wellBar.Children.Add(_loadButton);

        var testBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        testBar.Children.Add(Caption("Bleed start"));
        testBar.Children.Add(_bleedStart);
        testBar.Children.Add(Caption("Valve closed"));
        testBar.Children.Add(_valveClosed);
        testBar.Children.Add(Caption("Temperature"));
        testBar.Children.Add(_temperature);
        testBar.Children.Add(_temperatureUnit);

        var testBar2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        testBar2.Children.Add(Caption("Bleed-off"));
        testBar2.Children.Add(_bleedOff);
        testBar2.Children.Add(_recordButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(_counts);
        panel.Children.Add(Caption("Annulus"));
        panel.Children.Add(_annulus);
        panel.Children.Add(Caption("Limits"));
        panel.Children.Add(_limits);
        panel.Children.Add(Caption("Classification"));
        panel.Children.Add(_classification);
        panel.Children.Add(Caption("Recorded tests"));
        panel.Children.Add(_historyGrid);
        panel.Children.Add(Caption("Record a test"));
        panel.Children.Add(testBar);
        panel.Children.Add(testBar2);
        panel.Children.Add(Caption("Readings"));
        panel.Children.Add(_readings);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well, then record a pressure test.");
    }

    public OgCasingPressureWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string AnnulusText => _annulus.Text;

    public string LimitsText => _limits.Text;

    public string ClassificationText => _classification.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public bool CanRecord => _recordButton.IsEnabled;

    public bool Load() => Read(null);

    public bool SelectAnnulus(string name) => Read(name);

    private bool Read(string? annulusName)
    {
        _header.Text = "Engineering · casing pressure";

        var loaded = _pressure
            .LoadAsync(_well.Text?.Trim() ?? string.Empty, annulusName)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The casing pressure could not be read.";
            ShowState(OgViewState.Failed, "The annuli are not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> RecordAsync()
    {
        var readings = Readings(_readings.Text, out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var outcome = await _pressure.RecordTestAsync(
            _well.Text?.Trim() ?? string.Empty,
            _workspace?.Selected?.Name ?? (_annulusPicker.SelectedItem as string ?? string.Empty),
            Date(_bleedStart.Text) ?? DateTime.UtcNow,
            Date(_valveClosed.Text),
            Number(_temperature.Text),
            _temperatureUnit.Text ?? "degC",
            _bleedOff.Text ?? string.Empty,
            readings,
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The test was refused.";
            ShowState(OgViewState.Failed, "The test was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("integrity.casingpressure.write", "integrity",
            outcome.Value.Selected?.Name ?? "casing-pressure", "ok", _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = outcome.Value.Classification is { } classification
            ? "Test recorded: " + classification.Class + " (" + classification.Severity.ToLowerInvariant() + ")."
            : "Test recorded.";

        return true;
    }

    private void Apply(OgCasingPressureWorkspace workspace)
    {
        _workspace = workspace;

        if (string.IsNullOrWhiteSpace(_well.Text))
        {
            _well.Text = workspace.WellId;
        }

        _counts.Text = workspace.WellId + " · " + workspace.CountsText;
        _annulus.Text = workspace.Selected?.Describe() ?? workspace.AnnuliText;
        _limits.Text = workspace.Selected?.LimitsText ?? "No annulus is selected, so no limit is shown.";
        _classification.Text = workspace.ClassificationText;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _annulusPicker.ItemsSource = workspace.Annuli.Select(annulus => annulus.Name).ToList();
        _annulusPicker.SelectedItem = workspace.Selected?.Name;

        _historyGrid.ItemsSource = workspace.History.ToList();

        // The feature's state: no annulus data disables the screen, and a MAWOP that cannot be computed disables the recorder.
        _recordButton.IsEnabled = workspace.Selected is { CanClassify: true };

        _failure.Text = string.Empty;

        ShowState(workspace.Annuli.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _counts.Text = string.Empty;
        _annulus.Text = string.Empty;
        _limits.Text = string.Empty;
        _classification.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _annulusPicker.ItemsSource = null;
        _historyGrid.ItemsSource = null;
        _recordButton.IsEnabled = false;
        _notice.Text = string.Empty;
    }

    /// <summary>Parses "yyyy-MM-dd HH:mm, pressure, unit" per line; an unreadable line is a refusal with the line named.</summary>
    public static IReadOnlyList<OgPressureReading> Readings(string? text, out string? problem)
    {
        problem = null;
        var readings = new List<OgPressureReading>();
        var line = 0;

        foreach (var raw in (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            line++;

            var parts = raw.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 2 ||
                !DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var at) ||
                !decimal.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var pressure))
            {
                problem = "Reading '" + raw + "' is not 'yyyy-MM-dd HH:mm, pressure, unit', so the test is not recorded.";
                return [];
            }

            readings.Add(new OgPressureReading(pressure, parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : "psi", at));
        }

        if (readings.Count == 0)
        {
            problem = "Record at least one pressure reading before recording the test.";
        }

        return readings;
    }

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value) ? value : null;

    private static decimal Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 190,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
    }

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
}
