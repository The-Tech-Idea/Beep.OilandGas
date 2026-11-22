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
/// LFT-05: plunger lift - whether the well suits it, and if so the cycle, with every criterion shown and the two
/// non-feasible states kept apart.
/// </summary>
/// <remarks>
/// Every criterion is evaluated and printed with its deciding value (AC-1). An infeasible assessment shows **no rate**
/// (AC-2), while "feasible but uneconomic" is a third state with its basis (AC-3). The plunger types come from the owning
/// constants (AC-4), the three phase times add up to the cycle (AC-5), and a saved design keeps the type and the criteria
/// outcome (AC-6).
/// </remarks>
public sealed class PlungerPageView : OgPageBase
{
    public const string Route = "plunger";

    private readonly OgApplication _application;
    private readonly IOgPlunger _plunger;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _verdict;
    private readonly TextBlock _rate;
    private readonly TextBlock _cycle;
    private readonly TextBlock _economic;
    private readonly TextBlock _criteria;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _plungerType;
    private readonly TextBox _depth;
    private readonly ComboBox _tubing;
    private readonly TextBox _casingPressure;
    private readonly TextBox _linePressure;
    private readonly TextBox _gasRate;
    private readonly TextBox _liquidRate;
    private readonly TextBox _glr;
    private readonly DataGrid _history;
    private readonly Button _assessButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgPlungerWorkspace? _workspace;
    private OgPlungerResult? _result;

    public PlungerPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _plunger = application.Plunger;

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

        _verdict = Body(string.Empty);
        _verdict.TextWrapping = TextWrapping.Wrap;
        _verdict.Margin = new Thickness(0, 4, 0, 4);

        _rate = Body(string.Empty);
        _rate.TextWrapping = TextWrapping.Wrap;
        _rate.Margin = new Thickness(0, 0, 0, 4);

        _cycle = Body(string.Empty);
        _cycle.TextWrapping = TextWrapping.Wrap;
        _cycle.Margin = new Thickness(0, 0, 0, 4);

        _economic = Body(string.Empty);
        _economic.TextWrapping = TextWrapping.Wrap;
        _economic.Margin = new Thickness(0, 0, 0, 4);

        _criteria = Body(string.Empty);
        _criteria.TextWrapping = TextWrapping.Wrap;
        _criteria.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _depth = Field("Depth ft", 110);
        _depth.Text = "6000";
        _casingPressure = Field("Casing pressure psia", 140);
        _casingPressure.Text = "200";
        _linePressure = Field("Line pressure psia", 130);
        _linePressure.Text = "150";
        _gasRate = Field("Gas rate Mscf/d", 120);
        _gasRate.Text = "30";
        _liquidRate = Field("Liquid rate bbl/d", 120);
        _liquidRate.Text = "10";
        _glr = Field("GLR scf/bbl", 110);
        _glr.Text = "3000";

        _plungerType = Picker("Plunger type", 140);
        _tubing = Picker("Tubing diameter", 130);

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
                Text("Well", "WellId", 110),
                Text("Type", "PlungerType", 100),
                Text("Verdict", "VerdictText", 100),
                Text("Rate", "RateText", 160),
                Text("Cycle", "CycleText", 260),
                Text("Saved by", "Actor", 110),
            },
        };

        Named(_history, "Saved plunger assessments");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _assessButton = Button("Assess", (_, _) => _ = AssessAsync());
        _saveButton = Button("Save design", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Depth"));
        wellBar.Children.Add(_depth);
        wellBar.Children.Add(Caption("Tubing"));
        wellBar.Children.Add(_tubing);
        wellBar.Children.Add(Caption("Type"));
        wellBar.Children.Add(_plungerType);

        var pressureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pressureBar.Children.Add(Caption("Casing"));
        pressureBar.Children.Add(_casingPressure);
        pressureBar.Children.Add(Caption("Line"));
        pressureBar.Children.Add(_linePressure);

        var rateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        rateBar.Children.Add(Caption("Gas"));
        rateBar.Children.Add(_gasRate);
        rateBar.Children.Add(Caption("Liquid"));
        rateBar.Children.Add(_liquidRate);
        rateBar.Children.Add(Caption("GLR"));
        rateBar.Children.Add(_glr);
        rateBar.Children.Add(_assessButton);
        rateBar.Children.Add(_saveButton);
        rateBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Well"));
        panel.Children.Add(wellBar);
        panel.Children.Add(Caption("Pressures"));
        panel.Children.Add(pressureBar);
        panel.Children.Add(Caption("Rates"));
        panel.Children.Add(rateBar);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Verdict"));
        panel.Children.Add(_verdict);
        panel.Children.Add(_rate);
        panel.Children.Add(_cycle);
        panel.Children.Add(_economic);
        panel.Children.Add(Caption("Criteria"));
        panel.Children.Add(_criteria);
        panel.Children.Add(Caption("Saved assessments"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _depth, _casingPressure, _linePressure, _gasRate, _liquidRate, _glr,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgPlungerWorkspace? Workspace => _workspace;

    public OgPlungerResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string VerdictText => _verdict.Text;

    public string RateText => _rate.Text;

    public string CycleText => _cycle.Text;

    public string EconomicText => _economic.Text;

    public string CriteriaText => _criteria.Text;

    public string DetailText => _detail.Text;

    public bool CanAssess => PlungerRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, IsInfeasible: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _plunger.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Plunger lift";
            _notice.Text = loaded.Message ?? "The saved assessments could not be read.";
            ShowState(OgViewState.Failed, "The saved assessments could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> AssessAsync()
    {
        var inputs = Inputs();

        if (PlungerRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var assessed = await _plunger.AssessAsync(inputs).ConfigureAwait(true);

        if (!assessed.Success || assessed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _verdict.Text = string.Empty;
            _rate.Text = string.Empty;
            _cycle.Text = string.Empty;
            _economic.Text = string.Empty;
            _criteria.Text = string.Empty;
            _notice.Text = assessed.Message ?? "The assessment could not be run.";
            ShowState(OgViewState.Failed, "The assessment could not be run.", assessed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(assessed.Value);

        // Infeasible and uneconomic are states of a completed assessment, not failures of the run.
        LastFailed = _result is { IsFeasible: false };
        ShowState(_result is { IsFeasible: true } ? OgViewState.Ready : OgViewState.Empty, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = _result is { IsInfeasible: true } infeasible
                ? "An infeasible assessment has nothing to install: " + infeasible.FailedText + "."
                : "Assess the well before saving a design.";
            return false;
        }

        var saved = await _plunger.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The assessment was not saved.";
            ShowState(OgViewState.Failed, "The assessment was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "lift.plunger.save",
            "lift",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with the plunger type and the criteria outcome. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _plunger.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!retried.Success || retried.Value is null)
        {
            LastFailed = true;
            _notice.Text = retried.Message ?? "There is no assessment waiting to be retried.";
            ShowState(OgViewState.Failed, "There is no assessment waiting to be retried.", retried.Reference);
            return false;
        }

        Apply(retried.Value);
        _notice.Text = "The retained assessment was saved on the retry.";
        RefreshEnabled();
        return true;
    }

    private void Apply(OgPlungerWorkspace workspace)
    {
        _workspace = workspace;

        _plungerType.ItemsSource = workspace.PlungerTypes.ToList();
        _plungerType.SelectedIndex = _plungerType.SelectedIndex < 0 ? 0 : _plungerType.SelectedIndex;

        _tubing.ItemsSource = workspace.StandardTubingDiameters.ToList();
        _tubing.SelectedIndex = _tubing.SelectedIndex < 0 ? 5 : _tubing.SelectedIndex;

        _history.ItemsSource = workspace.Designs.ToList();

        _header.Text = "Plunger lift · feasibility and cycle";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved assessments are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgPlungerResult? result)
    {
        _result = result;

        if (result is null)
        {
            _verdict.Text = "no assessment is selected";
            _rate.Text = string.Empty;
            _cycle.Text = string.Empty;
            _economic.Text = string.Empty;
            _criteria.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _verdict.Text = result.Describe();
        _rate.Text = result.IsInfeasible
            ? "No rate: the well is infeasible (" + result.FailedText + ")."
            : $"Rate {result.RateText} · {result.PerCycleText} per cycle · {result.RatioText}";
        _cycle.Text = result.CycleText + " · fall " + result.FallText + " · rise " + result.RiseText;
        _economic.Text = result.EconomicText;
        _criteria.Text = result.CriteriaText;
        _detail.Text = "failed: " + result.FailedText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgPlungerResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = PlungerRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The assessment needs " + string.Join(", ", missing) + ".";

        _assessButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _plunger.Pending is not null;
    }

    private OgPlungerInputs Inputs() =>
        new(
            Empty(_well.Text),
            _plungerType.SelectedItem as string,
            Parse(_depth.Text),
            _tubing.SelectedItem is decimal tubing ? tubing : null,
            Parse(_casingPressure.Text),
            Parse(_linePressure.Text),
            Parse(_gasRate.Text),
            Parse(_liquidRate.Text),
            Parse(_glr.Text));

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
