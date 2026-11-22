using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ECO-01: economic evaluation - a forecast and a fiscal regime turned into cash flow, NPV, IRR and payback, with the
/// drivers that move the answer.
/// </summary>
/// <remarks>
/// The reference case's NPV is the closed-form annuity (AC-1) and an IRR that does not exist reads "undefined", never 0
/// (AC-2). Every money value is decimal end to end (AC-3). A missing fiscal term blocks the run with the term named
/// (AC-4), the sensitivity re-uses the base engine (AC-5), and an approved evaluation is immutable and states the forecast
/// version it used (AC-6).
/// </remarks>
public sealed class EconomicEvaluationPageView : OgPageBase
{
    public const string Route = "economics";

    private readonly OgApplication _application;
    private readonly IOgEconomics _economics;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _regime;
    private readonly TextBlock _regimeBlocked;
    private readonly TextBlock _summary;
    private readonly TextBlock _answer;
    private readonly TextBlock _scenario;
    private readonly TextBlock _comparison;
    private readonly TextBlock _detail;
    private readonly TextBox _entity;
    private readonly ComboBox _scenarioBox;
    private readonly TextBox _forecastVersion;
    private readonly ComboBox _currency;
    private readonly TextBox _discount;
    private readonly TextBox _regimeName;
    private readonly TextBox _royalty;
    private readonly TextBox _tax;
    private readonly TextBox _periods;
    private readonly TextBox _startYear;
    private readonly TextBox _volume;
    private readonly ComboBox _volumeUnit;
    private readonly TextBox _price;
    private readonly TextBox _priceUnit;
    private readonly TextBox _opex;
    private readonly TextBox _capex;
    private readonly TextBox _escalation;
    private readonly TextBox _abandonment;
    private readonly DataGrid _periodGrid;
    private readonly DataGrid _driverGrid;
    private readonly DataGrid _history;
    private readonly Button _evaluateButton;
    private readonly Button _saveButton;
    private readonly Button _approveButton;
    private readonly Button _retryButton;

    private OgEconomicsWorkspace? _workspace;
    private OgEconomicResult? _result;

    public EconomicEvaluationPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _economics = application.Economics;

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

        _regime = Body(string.Empty);
        _regime.TextWrapping = TextWrapping.Wrap;
        _regime.Margin = new Thickness(0, 0, 0, 4);

        _regimeBlocked = Body(string.Empty);
        _regimeBlocked.TextWrapping = TextWrapping.Wrap;
        _regimeBlocked.Margin = new Thickness(0, 0, 0, 6);
        _regimeBlocked.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _answer = Body(string.Empty);
        _answer.TextWrapping = TextWrapping.Wrap;
        _answer.Margin = new Thickness(0, 0, 0, 4);

        _scenario = Body(string.Empty);
        _scenario.TextWrapping = TextWrapping.Wrap;
        _scenario.Margin = new Thickness(0, 0, 0, 4);

        _comparison = Body(string.Empty);
        _comparison.TextWrapping = TextWrapping.Wrap;
        _comparison.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _entity = Field("Well or entity", 120);
        _forecastVersion = Field("Forecast version", 130);
        _forecastVersion.Text = "FC-2026-01";
        _discount = Field("Discount rate", 110);
        _discount.Text = "0.10";
        _regimeName = Field("Fiscal regime", 140);
        _regimeName.Text = "Concession";
        _royalty = Field("Royalty", 90);
        _tax = Field("Tax", 90);
        _periods = Field("Periods", 80);
        _periods.Text = "10";
        _startYear = Field("Start year", 100);
        _startYear.Text = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
        _volume = Field("Annual volume", 120);
        _volume.Text = "365000";
        _price = Field("Unit price", 100);
        _price.Text = "70";
        _priceUnit = Field("Price unit", 100);
        _priceUnit.Text = "USD/bbl";
        _opex = Field("Opex per unit", 120);
        _opex.Text = "12";
        _capex = Field("Annual capex", 120);
        _capex.Text = "5000000";
        _escalation = Field("Escalation", 100);
        _escalation.Text = "0.02";
        _abandonment = Field("Abandonment", 120);
        _abandonment.Text = "750000";

        _scenarioBox = Picker("Scenario", 140);
        _currency = Picker("Currency", 100);
        _volumeUnit = Picker("Volume unit", 100);

        _periodGrid = new DataGrid
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
                Text("Period", "Period", 70),
                Text("Year", "Year", 70),
                Text("Revenue", "Revenue", 110),
                Text("Royalty", "Royalty", 100),
                Text("Opex", "Opex", 100),
                Text("Capex", "Capex", 100),
                Text("Tax", "Tax", 90),
                Text("Net", "Net", 110),
                Text("Discounted", "Discounted", 110),
                Text("Cumulative", "Cumulative", 120),
            },
        };

        Named(_periodGrid, "Cash flow by period");

        _driverGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Driver", "Name", 130),
                Text("Low NPV", "LowNpv", 130),
                Text("High NPV", "HighNpv", 130),
                Text("Swing", "Swing", 130),
            },
        };

        Named(_driverGrid, "Sensitivity drivers, largest swing first");

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
                Text("Entity", "EntityId", 100),
                Text("Scenario", "Scenario", 90),
                Text("Forecast", "ForecastVersion", 110),
                Text("NPV", "NpvText", 120),
                Text("IRR", "IrrStoredText", 110),
                Text("Status", "Status", 100),
            },
        };

        Named(_history, "Saved economic evaluations");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _evaluateButton = Button("Evaluate", (_, _) => _ = EvaluateAsync());
        _saveButton = Button("Save draft", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _approveButton = Button("Approve", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var scenarioBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        scenarioBar.Children.Add(Caption("Entity"));
        scenarioBar.Children.Add(_entity);
        scenarioBar.Children.Add(Caption("Scenario"));
        scenarioBar.Children.Add(_scenarioBox);
        scenarioBar.Children.Add(Caption("Forecast"));
        scenarioBar.Children.Add(_forecastVersion);
        scenarioBar.Children.Add(Caption("Currency"));
        scenarioBar.Children.Add(_currency);

        var fiscalBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fiscalBar.Children.Add(Caption("Regime"));
        fiscalBar.Children.Add(_regimeName);
        fiscalBar.Children.Add(Caption("Royalty"));
        fiscalBar.Children.Add(_royalty);
        fiscalBar.Children.Add(Caption("Tax"));
        fiscalBar.Children.Add(_tax);

        var timingBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        timingBar.Children.Add(Caption("Discount"));
        timingBar.Children.Add(_discount);
        timingBar.Children.Add(Caption("Periods"));
        timingBar.Children.Add(_periods);
        timingBar.Children.Add(Caption("Start year"));
        timingBar.Children.Add(_startYear);

        var volumeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        volumeBar.Children.Add(Caption("Volume"));
        volumeBar.Children.Add(_volume);
        volumeBar.Children.Add(_volumeUnit);
        volumeBar.Children.Add(Caption("Price"));
        volumeBar.Children.Add(_price);
        volumeBar.Children.Add(_priceUnit);
        volumeBar.Children.Add(Caption("Opex/unit"));
        volumeBar.Children.Add(_opex);

        var costBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        costBar.Children.Add(Caption("Capex"));
        costBar.Children.Add(_capex);
        costBar.Children.Add(Caption("Escalation"));
        costBar.Children.Add(_escalation);
        costBar.Children.Add(Caption("Abandonment"));
        costBar.Children.Add(_abandonment);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_evaluateButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_approveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Scenario"));
        panel.Children.Add(scenarioBar);
        panel.Children.Add(Caption("Fiscal regime"));
        panel.Children.Add(fiscalBar);
        panel.Children.Add(Caption("Timing"));
        panel.Children.Add(timingBar);
        panel.Children.Add(Caption("Volumes and price"));
        panel.Children.Add(volumeBar);
        panel.Children.Add(Caption("Costs"));
        panel.Children.Add(costBar);
        panel.Children.Add(_regime);
        panel.Children.Add(_regimeBlocked);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Answer"));
        panel.Children.Add(_summary);
        panel.Children.Add(_answer);
        panel.Children.Add(_scenario);
        panel.Children.Add(Caption("Cash flow"));
        panel.Children.Add(_periodGrid);
        panel.Children.Add(Caption("Sensitivity"));
        panel.Children.Add(_driverGrid);
        panel.Children.Add(Caption("Scenario comparison"));
        panel.Children.Add(_comparison);
        panel.Children.Add(Caption("Saved evaluations"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _entity, _forecastVersion, _discount, _regimeName, _royalty, _tax, _periods, _startYear,
                     _volume, _price, _priceUnit, _opex, _capex, _escalation, _abandonment,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _scenarioBox.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgEconomicsWorkspace? Workspace => _workspace;

    public OgEconomicResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string RegimeText => _regime.Text;

    public string RegimeBlockedText => _regimeBlocked.Text;

    public string SummaryText => _summary.Text;

    public string AnswerText => _answer.Text;

    public string ScenarioText => _scenario.Text;

    public string ComparisonText => _comparison.Text;

    public string DetailText => _detail.Text;

    public bool CanEvaluate => EconomicRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, IsApproved: false };

    public bool CanApprove => _result is { IsSaved: true, IsApproved: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _economics.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Economics";
            _notice.Text = loaded.Message ?? "The saved evaluations could not be read.";
            ShowState(OgViewState.Failed, "The saved evaluations could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> EvaluateAsync()
    {
        var inputs = Inputs();

        if (EconomicRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var evaluated = await _economics.EvaluateAsync(inputs).ConfigureAwait(true);

        if (!evaluated.Success || evaluated.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _answer.Text = string.Empty;
            _scenario.Text = string.Empty;
            _periodGrid.ItemsSource = null;
            _driverGrid.ItemsSource = null;
            _notice.Text = evaluated.Message ?? "The evaluation could not be run.";
            ShowState(OgViewState.Failed, "The evaluation could not be run.", evaluated.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(evaluated.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Run an evaluation before saving it.";
            return false;
        }

        var saved = await _economics.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The evaluation was not saved.";
            ShowState(OgViewState.Failed, "The evaluation was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "economics.evaluation.save",
            "economics",
            result.EntityId ?? "(no entity)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved as a draft with its cash flow and drivers. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> ApproveAsync()
    {
        if (_result is not { IsSaved: true } result)
        {
            _notice.Text = "Save the evaluation before approving it.";
            return false;
        }

        var approved = await _economics.ApproveAsync(result.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!approved.Success || approved.Value is null)
        {
            LastFailed = true;
            _notice.Text = approved.Message ?? "The evaluation was not approved.";
            ShowState(OgViewState.Failed, "The evaluation was not approved.", approved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(approved.Value);

        _application.AuditLog.Add(
            "economics.evaluation.approve",
            "economics",
            result.EntityId ?? "(no entity)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Approved. An approved evaluation is immutable - run a new one to change it.";
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _economics.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!retried.Success || retried.Value is null)
        {
            LastFailed = true;
            _notice.Text = retried.Message ?? "There is no evaluation waiting to be retried.";
            ShowState(OgViewState.Failed, "There is no evaluation waiting to be retried.", retried.Reference);
            return false;
        }

        Apply(retried.Value);
        _notice.Text = "The retained evaluation was saved on the retry.";
        RefreshEnabled();
        return true;
    }

    private void Apply(OgEconomicsWorkspace workspace)
    {
        _workspace = workspace;

        _scenarioBox.ItemsSource = workspace.Scenarios.ToList();
        _scenarioBox.SelectedIndex = _scenarioBox.SelectedIndex < 0 && workspace.Scenarios.Count > 0
            ? 0
            : _scenarioBox.SelectedIndex;

        _currency.ItemsSource = workspace.Currencies.ToList();
        _currency.SelectedIndex = _currency.SelectedIndex < 0 ? 0 : _currency.SelectedIndex;

        _volumeUnit.ItemsSource = workspace.VolumeUnits.ToList();
        _volumeUnit.SelectedIndex = _volumeUnit.SelectedIndex < 0 ? 0 : _volumeUnit.SelectedIndex;

        _history.ItemsSource = workspace.Results.ToList();

        _header.Text = "Economics · evaluation";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved evaluations are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;
        _regime.Text = "Fiscal regime: " + workspace.RegimeText;
        _regimeBlocked.Text = workspace.RegimeBlockedText;
        _comparison.Text = workspace.ComparisonText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgEconomicResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no evaluation is selected";
            _answer.Text = string.Empty;
            _scenario.Text = string.Empty;
            _periodGrid.ItemsSource = null;
            _driverGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = result.Describe();
        _answer.Text =
            $"NPV {result.NpvText} · IRR {result.IrrText} · payback {result.PaybackText} · " +
            $"peak funding {result.PeakFundingText} · undiscounted total {result.TotalText}";
        _scenario.Text = "Scenario: " + result.ScenarioText;
        _periodGrid.ItemsSource = result.Periods.ToList();
        _driverGrid.ItemsSource = result.Drivers.ToList();
        _detail.Text = result.StatusText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgEconomicResult selected)
        {
            return;
        }

        ShowResult(selected);

        // A saved row's inputs are read back, so approving it works on the stored evaluation.
        if (selected.Inputs is { } inputs)
        {
            _entity.Text = inputs.EntityId ?? string.Empty;
            _forecastVersion.Text = inputs.ForecastVersion ?? string.Empty;
            _discount.Text = Text(inputs.DiscountRate);
            _regimeName.Text = inputs.RegimeName ?? string.Empty;
            _royalty.Text = Text(inputs.RoyaltyRate);
            _tax.Text = Text(inputs.TaxRate);
            _periods.Text = inputs.PeriodCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _startYear.Text = inputs.StartYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            _volume.Text = Text(inputs.AnnualVolume);
            _price.Text = Text(inputs.UnitPrice);
            _priceUnit.Text = inputs.PriceUnit ?? string.Empty;
            _opex.Text = Text(inputs.OpexPerUnit);
            _capex.Text = Text(inputs.AnnualCapex);
            _escalation.Text = Text(inputs.EscalationRate);
            _abandonment.Text = Text(inputs.AbandonmentCost);
            _scenarioBox.SelectedItem = selected.Scenario;
            _currency.SelectedItem = selected.Currency;
        }

        RefreshEnabled();
    }

    private void RefreshEnabled()
    {
        var missing = EconomicRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The evaluation needs " + string.Join(", ", missing) + ".";

        var regime = Inputs();
        var blocked = regime.RoyaltyRate is null || regime.TaxRate is null;

        _regimeBlocked.Text = blocked
            ? "The fiscal regime is not configured: " +
              string.Join(", ", new[] { regime.RoyaltyRate is null ? "royalty rate" : null, regime.TaxRate is null ? "tax rate" : null }
                  .Where(term => term is not null)) +
              " missing. Set the term on the regime before running the evaluation."
            : string.Empty;

        _evaluateButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _approveButton.IsEnabled = CanApprove;
        _retryButton.IsEnabled = _economics.Pending is not null;
    }

    private OgEconomicInputs Inputs() =>
        new(
            Empty(_entity.Text),
            _scenarioBox.SelectedItem as string,
            Empty(_forecastVersion.Text),
            _currency.SelectedItem as string,
            Parse(_discount.Text),
            Empty(_regimeName.Text),
            Parse(_royalty.Text),
            Parse(_tax.Text),
            (int?)Parse(_periods.Text),
            (int?)Parse(_startYear.Text),
            Parse(_volume.Text),
            _volumeUnit.SelectedItem as string,
            Parse(_price.Text),
            Empty(_priceUnit.Text),
            Parse(_opex.Text),
            Parse(_capex.Text),
            Parse(_escalation.Text),
            Parse(_abandonment.Text));

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static string Text(decimal? value) =>
        value?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

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
