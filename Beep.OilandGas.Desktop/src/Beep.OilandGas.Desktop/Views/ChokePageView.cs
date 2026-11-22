using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// LFT-07: choke analysis - the rate or the pressure a choke gives, with the correlation and the flow regime named and the
/// erosion assessment beside it.
/// </summary>
/// <remarks>
/// The regime and the correlation are printed with every result (AC-2); a correlation the module has no model for is shown
/// with the one it actually ran (the state table's "correlation chosen but out of range"). The erosion threshold always
/// names its source (AC-3). The choke types and regimes come from the owning enums through the workspace (AC-4).
/// </remarks>
public sealed class ChokePageView : OgPageBase
{
    public const string Route = "choke";

    private readonly OgApplication _application;
    private readonly IOgChoke _choke;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _correlation;
    private readonly TextBlock _regime;
    private readonly TextBlock _erosion;
    private readonly TextBlock _notes;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _correlationBox;
    private readonly ComboBox _chokeType;
    private readonly TextBox _diameter;
    private readonly TextBox _dischargeCoefficient;
    private readonly TextBox _upstream;
    private readonly TextBox _downstream;
    private readonly TextBox _temperature;
    private readonly TextBox _gasGravity;
    private readonly TextBox _zFactor;
    private readonly TextBox _liquidRate;
    private readonly TextBox _gasRate;
    private readonly TextBox _glr;
    private readonly TextBox _sand;
    private readonly DataGrid _history;
    private readonly Button _analyseButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgChokeWorkspace? _workspace;
    private OgChokeResult? _result;

    public ChokePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _choke = application.Choke;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _reference = Body(string.Empty);
        _reference.TextWrapping = TextWrapping.Wrap;
        _reference.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _correlation = Body(string.Empty);
        _correlation.TextWrapping = TextWrapping.Wrap;
        _correlation.Margin = new Thickness(0, 0, 0, 4);

        _regime = Body(string.Empty);
        _regime.TextWrapping = TextWrapping.Wrap;
        _regime.Margin = new Thickness(0, 0, 0, 4);

        _erosion = Body(string.Empty);
        _erosion.TextWrapping = TextWrapping.Wrap;
        _erosion.Margin = new Thickness(0, 0, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _diameter = Field("Choke diameter in", 130);
        _diameter.Text = "0.5";
        _dischargeCoefficient = Field("Discharge coefficient", 140);
        _upstream = Field("Upstream pressure psia", 150);
        _upstream.Text = "3000";
        _downstream = Field("Downstream pressure psia", 150);
        _downstream.Text = "1000";
        _temperature = Field("Temperature degF", 130);
        _temperature.Text = "200";
        _gasGravity = Field("Gas specific gravity", 130);
        _gasGravity.Text = "0.65";
        _zFactor = Field("Z factor", 90);
        _zFactor.Text = "0.9";
        _liquidRate = Field("Liquid rate bbl/d", 130);
        _liquidRate.Text = "1000";
        _gasRate = Field("Gas rate MMscf/d", 130);
        _gasRate.Text = "0.6";
        _glr = Field("GLR scf/bbl", 110);
        _glr.Text = "600";
        _sand = Field("Sand rate lb/d", 110);

        _correlationBox = Picker("Correlation", 220);
        _chokeType = Picker("Choke type", 130);

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
                Text("Choke", "ChokeText", 150),
                Text("Result", "RateText", 120),
                Text("Regime", "RegimeText", 110),
                Text("Correlation", "CorrelationText", 260),
                Text("Saved by", "Actor", 100),
            },
        };

        Named(_history, "Saved choke results");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _analyseButton = Button("Analyse", (_, _) => _ = AnalyseAsync());
        _saveButton = Button("Save result", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Choke"));
        wellBar.Children.Add(_diameter);
        wellBar.Children.Add(_chokeType);
        wellBar.Children.Add(Caption("Cd"));
        wellBar.Children.Add(_dischargeCoefficient);

        var pressureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pressureBar.Children.Add(Caption("Upstream"));
        pressureBar.Children.Add(_upstream);
        pressureBar.Children.Add(Caption("Downstream"));
        pressureBar.Children.Add(_downstream);
        pressureBar.Children.Add(Caption("Temperature"));
        pressureBar.Children.Add(_temperature);

        var gasBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        gasBar.Children.Add(Caption("Gas SG"));
        gasBar.Children.Add(_gasGravity);
        gasBar.Children.Add(Caption("Z"));
        gasBar.Children.Add(_zFactor);

        var rateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        rateBar.Children.Add(Caption("Liquid"));
        rateBar.Children.Add(_liquidRate);
        rateBar.Children.Add(Caption("Gas"));
        rateBar.Children.Add(_gasRate);
        rateBar.Children.Add(Caption("GLR"));
        rateBar.Children.Add(_glr);
        rateBar.Children.Add(Caption("Sand"));
        rateBar.Children.Add(_sand);

        var methodBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        methodBar.Children.Add(Caption("Correlation"));
        methodBar.Children.Add(_correlationBox);
        methodBar.Children.Add(_analyseButton);
        methodBar.Children.Add(_saveButton);
        methodBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Well and choke"));
        panel.Children.Add(wellBar);
        panel.Children.Add(Caption("Pressures"));
        panel.Children.Add(pressureBar);
        panel.Children.Add(Caption("Gas"));
        panel.Children.Add(gasBar);
        panel.Children.Add(Caption("Rates"));
        panel.Children.Add(rateBar);
        panel.Children.Add(Caption("Method"));
        panel.Children.Add(methodBar);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_correlation);
        panel.Children.Add(_regime);
        panel.Children.Add(Caption("Erosion"));
        panel.Children.Add(_erosion);
        panel.Children.Add(Caption("Notes"));
        panel.Children.Add(_notes);
        panel.Children.Add(Caption("Saved results"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _diameter, _dischargeCoefficient, _upstream, _downstream, _temperature,
                     _gasGravity, _zFactor, _liquidRate, _gasRate, _glr, _sand,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _correlationBox.SelectionChanged += (_, _) => RefreshEnabled();
        _chokeType.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgChokeWorkspace? Workspace => _workspace;

    public OgChokeResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string CorrelationText => _correlation.Text;

    public string RegimeText => _regime.Text;

    public string ErosionText => _erosion.Text;

    public string NotesText => _notes.Text;

    public string DetailText => _detail.Text;

    public bool CanAnalyse => ChokeRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _choke.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Choke analysis";
            _notice.Text = loaded.Message ?? "The saved results could not be read.";
            ShowState(OgViewState.Failed, "The saved results could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> AnalyseAsync()
    {
        var inputs = Inputs();

        if (ChokeRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var analysed = await _choke.AnalyseAsync(inputs).ConfigureAwait(true);

        if (!analysed.Success || analysed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _correlation.Text = string.Empty;
            _regime.Text = string.Empty;
            _erosion.Text = string.Empty;
            _notes.Text = string.Empty;
            _notice.Text = analysed.Message ?? "The analysis could not be run.";
            ShowState(OgViewState.Failed, "The analysis could not be run.", analysed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(analysed.Value);

        // Erosion above the limit is a warning about a completed analysis, not a failed run.
        LastFailed = false;
        ShowState(_result is { ErosionAboveThreshold: true } ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Analyse a choke before saving the result.";
            return false;
        }

        var saved = await _choke.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

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
            "lift.choke.save",
            "lift",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its correlation, its regime and the erosion threshold's source. " +
                       saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _choke.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

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

    private void Apply(OgChokeWorkspace workspace)
    {
        _workspace = workspace;

        _correlationBox.ItemsSource = workspace.Correlations.ToList();
        _correlationBox.SelectedIndex = _correlationBox.SelectedIndex < 0 ? 0 : _correlationBox.SelectedIndex;

        _chokeType.ItemsSource = workspace.ChokeTypes.ToList();
        _chokeType.SelectedIndex = _chokeType.SelectedIndex < 0 ? 0 : _chokeType.SelectedIndex;

        _history.ItemsSource = workspace.Results.ToList();

        _header.Text = "Choke analysis · rate or pressure, with its regime";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved results are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgChokeResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no result is selected";
            _correlation.Text = string.Empty;
            _regime.Text = string.Empty;
            _erosion.Text = string.Empty;
            _notes.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = result.Describe() + " · " + result.PressuresText;
        _correlation.Text = "Correlation: " + result.CorrelationText;
        _regime.Text = "Flow regime: " + result.RegimeText + " · " + result.RatiosText;
        _erosion.Text = result.ErosionText;
        _notes.Text = result.NotesText;
        _detail.Text =
            "result in " + result.RateUnit + " · choke " + result.ChokeText +
            (result.HasWarning ? " · this result carries a warning" : " · no warning");
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgChokeResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = ChokeRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The analysis needs " + string.Join(", ", missing) + ".";

        _analyseButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _choke.Pending is not null;
    }

    private OgChokeInputs Inputs() =>
        new(
            Empty(_well.Text),
            _correlationBox.SelectedItem as string,
            _chokeType.SelectedItem as string,
            Parse(_diameter.Text),
            Parse(_dischargeCoefficient.Text),
            Parse(_upstream.Text),
            Parse(_downstream.Text),
            Parse(_temperature.Text),
            Parse(_gasGravity.Text),
            Parse(_zFactor.Text),
            Parse(_liquidRate.Text),
            Parse(_gasRate.Text),
            Parse(_glr.Text),
            Parse(_sand.Text));

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
