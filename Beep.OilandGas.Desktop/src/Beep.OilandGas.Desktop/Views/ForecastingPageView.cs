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
/// RES-07: production forecasting with scenarios - a profile generated from a RES-06 fit or from typed parameters, saved
/// as a version and approved by a second person.
/// </summary>
/// <remarks>
/// The generator is disabled with the reason when there is neither a saved fit nor a typed rate (the state table's "no
/// source"). A ceiling that the profile breaks is refused rather than clamped (AC-5). Every save is a new version and no
/// version is ever overwritten (AC-3). A forecast records its method and the fit it came from (AC-2) and carries a unit
/// per stream (AC-6).
/// </remarks>
public sealed class ForecastingPageView : OgPageBase
{
    public const string Route = "forecasting";

    private const string ManualSource = "(manual parameters)";

    private readonly OgApplication _application;
    private readonly IOgForecast _forecasts;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _sourceNotice;
    private readonly TextBlock _summary;
    private readonly TextBlock _approval;
    private readonly TextBlock _refusal;
    private readonly TextBlock _detail;
    private readonly TextBox _name;
    private readonly TextBox _well;
    private readonly ComboBox _method;
    private readonly ComboBox _fit;
    private readonly TextBox _start;
    private readonly TextBox _months;
    private readonly TextBox _maxRate;
    private readonly TextBox _economicLimit;
    private readonly ComboBox _risk;
    private readonly TextBox _manualQi;
    private readonly TextBox _manualDi;
    private readonly TextBox _manualB;
    private readonly ComboBox _manualFluid;
    private readonly TextBox _manualUnit;
    private readonly DataGrid _points;
    private readonly DataGrid _saved;
    private readonly Button _generateButton;
    private readonly Button _saveButton;
    private readonly Button _approveButton;
    private readonly Button _retryButton;

    private OgForecastWorkspace? _workspace;
    private OgForecast? _forecast;

    public ForecastingPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _forecasts = application.Forecasting;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _sourceNotice = Body(string.Empty);
        _sourceNotice.TextWrapping = TextWrapping.Wrap;
        _sourceNotice.Margin = new Thickness(0, 4, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _approval = Body(string.Empty);
        _approval.TextWrapping = TextWrapping.Wrap;
        _approval.Margin = new Thickness(0, 0, 0, 4);

        _refusal = Body(string.Empty);
        _refusal.TextWrapping = TextWrapping.Wrap;
        _refusal.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _name = Field("Forecast name", 160);
        _name.Text = "Base case";
        _well = Field("Well or field id", 130);
        _start = Field("Start date yyyy-MM-dd", 140);
        _start.Text = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        _months = Field("Horizon months", 90);
        _months.Text = "24";
        _maxRate = Field("Maximum rate (ceiling)", 120);
        _economicLimit = Field("Economic limit rate", 120);
        _manualQi = Field("Manual qi", 90);
        _manualDi = Field("Manual Di per day", 110);
        _manualB = Field("Manual b", 80);
        _manualUnit = Field("Manual unit", 80);
        _manualUnit.Text = "bbl";

        _method = Picker("Forecast method", 150);
        _fit = Picker("Source fit", 260);
        _risk = Picker("Risk rating", 110);
        _manualFluid = Picker("Manual fluid", 100);

        _points = new DataGrid
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
                Text("Month", "DateText", 100),
                Text("Oil", "OilText", 90),
                Text("Gas", "GasText", 90),
                Text("Water", "WaterText", 90),
                Text("Volume", "VolumeText", 120),
                Text("Cumulative", "CumulativeText", 130),
            },
        };

        Named(_points, "Forecast points, one per month");

        _saved = new DataGrid
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
                Text("Name", "Name", 130),
                Text("Entity", "EntityText", 110),
                Text("Method", "MethodText", 110),
                Text("Version", "VersionText", 70),
                Text("Status", "StatusText", 90),
                Text("Months", "Months", 70),
                Text("Total", "TotalText", 120),
                Text("Saved by", "Actor", 110),
            },
        };

        Named(_saved, "Saved forecast versions");
        _saved.SelectionChanged += (_, _) => SelectSaved();

        _generateButton = Button("Generate", (_, _) => _ = GenerateAsync());
        _saveButton = Button("Save version", (_, _) => _ = SaveAsync());
        _generateButton.IsEnabled = false;
        _saveButton.IsEnabled = false;

        _approveButton = Button("Approve", (_, _) => _ = ApproveAsync());
        _approveButton.IsEnabled = false;

        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var where = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        where.Children.Add(Caption("Name"));
        where.Children.Add(_name);
        where.Children.Add(Caption("Entity"));
        where.Children.Add(_well);
        where.Children.Add(Caption("Method"));
        where.Children.Add(_method);

        var sourceBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sourceBar.Children.Add(Caption("Source fit"));
        sourceBar.Children.Add(_fit);
        sourceBar.Children.Add(Caption("Start"));
        sourceBar.Children.Add(_start);
        sourceBar.Children.Add(Caption("Months"));
        sourceBar.Children.Add(_months);
        sourceBar.Children.Add(Caption("Ceiling"));
        sourceBar.Children.Add(_maxRate);
        sourceBar.Children.Add(Caption("Economic limit"));
        sourceBar.Children.Add(_economicLimit);

        var manualBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        manualBar.Children.Add(Caption("Manual qi"));
        manualBar.Children.Add(_manualQi);
        manualBar.Children.Add(Caption("Di /day"));
        manualBar.Children.Add(_manualDi);
        manualBar.Children.Add(Caption("b"));
        manualBar.Children.Add(_manualB);
        manualBar.Children.Add(_manualFluid);
        manualBar.Children.Add(_manualUnit);
        manualBar.Children.Add(Caption("Risk"));
        manualBar.Children.Add(_risk);
        manualBar.Children.Add(_generateButton);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_approveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Forecast"));
        panel.Children.Add(where);
        panel.Children.Add(sourceBar);
        panel.Children.Add(manualBar);
        panel.Children.Add(_sourceNotice);
        panel.Children.Add(Caption("Selected forecast"));
        panel.Children.Add(_summary);
        panel.Children.Add(_approval);
        panel.Children.Add(_refusal);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Points"));
        panel.Children.Add(_points);
        panel.Children.Add(Caption("Saved versions"));
        panel.Children.Add(_saved);
        panel.Children.Add(_detail);

        ShowContent(panel);

        _manualQi.TextChanged += (_, _) => RefreshEnabled();
        _fit.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgForecastWorkspace? Workspace => _workspace;

    public OgForecast? Forecast => _forecast;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SourceNoticeText => _sourceNotice.Text;

    public string SummaryText => _summary.Text;

    public string ApprovalText => _approval.Text;

    public string RefusalText => _refusal.Text;

    public string DetailText => _detail.Text;

    public bool CanGenerate => _workspace is { HasFits: true } || !string.IsNullOrWhiteSpace(_manualQi.Text);

    public bool CanSave => _forecast is { IsRefused: false, IsSaved: false, Points.Count: > 0 };

    public bool CanApprove => _forecast is { IsSaved: true, IsApproved: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _forecasts
            .LoadAsync(_application.Session.FieldId, _application.Session.FieldName)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _saved.ItemsSource = null;
            _points.ItemsSource = null;
            _header.Text = "Forecasting";
            _notice.Text = loaded.Message ?? "The saved forecasts could not be read.";
            ShowState(OgViewState.Failed, "The saved forecasts could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> GenerateAsync()
    {
        var inputs = new OgForecastInputs(
            Empty(_name.Text),
            Empty(_well.Text),
            _application.Session.FieldId,
            _method.SelectedItem as string,
            SelectedFitId(),
            Date(_start.Text),
            (int)(Parse(_months.Text) ?? 0m),
            Parse(_maxRate.Text),
            Parse(_economicLimit.Text),
            _risk.SelectedItem as string,
            Parse(_manualQi.Text),
            Parse(_manualDi.Text),
            Parse(_manualB.Text),
            _manualFluid.SelectedItem as string,
            Empty(_manualUnit.Text));

        var generated = await _forecasts.GenerateAsync(inputs).ConfigureAwait(true);

        if (!generated.Success || generated.Value is null)
        {
            LastFailed = true;
            _forecast = null;
            _points.ItemsSource = null;
            _summary.Text = string.Empty;
            _approval.Text = string.Empty;
            _refusal.Text = string.Empty;
            _notice.Text = generated.Message ?? "The forecast could not be generated.";
            ShowState(OgViewState.Failed, "The forecast could not be generated.", generated.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(generated.Value);
        return _forecast is { IsRefused: false };
    }

    public async Task<bool> SaveAsync()
    {
        if (_forecast is not { } forecast || !CanSave)
        {
            _notice.Text = "Generate a profile before saving it.";
            return false;
        }

        var saved = await _forecasts.SaveAsync(forecast, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The forecast was not saved.";
            ShowState(OgViewState.Failed, "The forecast was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "forecasting.save",
            "reservoir",
            forecast.Name ?? "(no name)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved as version " +
                       (_forecast?.VersionText ?? "(unknown)") +
                       "; an earlier version is kept beside it. " +
                       (saved.Value.HistoryStateText);

        RefreshEnabled();
        return true;
    }

    public async Task<bool> ApproveAsync()
    {
        if (_forecast is not { IsSaved: true } forecast)
        {
            _notice.Text = "Save the forecast before approving it.";
            return false;
        }

        var approved = await _forecasts
            .ApproveAsync(forecast.Id, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!approved.Success || approved.Value is null)
        {
            LastFailed = true;
            _notice.Text = approved.Message ?? "The approval was not recorded.";
            ShowState(OgViewState.Failed, "The approval was not recorded.", approved.Reference);
            return false;
        }

        Apply(approved.Value);

        _application.AuditLog.Add(
            "forecasting.approve",
            "reservoir",
            forecast.Id,
            "ok",
            _application.Session.UserId);

        _notice.Text = "Approved; the forecast is now immutable and a later save creates a new version.";
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _forecasts.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!retried.Success || retried.Value is null)
        {
            LastFailed = true;
            _notice.Text = retried.Message ?? "There is no forecast waiting to be retried.";
            ShowState(OgViewState.Failed, "There is no forecast waiting to be retried.", retried.Reference);
            return false;
        }

        Apply(retried.Value);
        _notice.Text = "The retained forecast was saved on the retry.";
        RefreshEnabled();
        return true;
    }

    private void Apply(OgForecastWorkspace workspace)
    {
        _workspace = workspace;
        LastFailed = false;

        _method.ItemsSource = workspace.Methods.ToList();
        if (_method.SelectedIndex < 0 && _method.Items.Count > 0)
        {
            var decline = workspace.Methods
                .Select((name, index) => (name, index))
                .FirstOrDefault(pair => string.Equals(pair.name, "Decline", StringComparison.OrdinalIgnoreCase));

            _method.SelectedIndex = decline.name is null ? 0 : decline.index;
        }

        _risk.ItemsSource = workspace.RiskRatings.ToList();
        _risk.SelectedIndex = _risk.SelectedIndex < 0 && _risk.Items.Count > 0 ? 0 : _risk.SelectedIndex;

        _manualFluid.ItemsSource = ForecastStreams.All.ToList();
        _manualFluid.SelectedIndex = _manualFluid.SelectedIndex < 0 ? 0 : _manualFluid.SelectedIndex;

        var fits = new List<string> { ManualSource };
        fits.AddRange(workspace.Fits.Select(fit => fit.Id));

        _fit.ItemsSource = fits;
        if (_fit.SelectedIndex < 0)
        {
            _fit.SelectedIndex = workspace.Fits.Count > 0 ? 1 : 0;
        }

        _saved.ItemsSource = workspace.Forecasts.ToList();
        _header.Text = "Production forecasting · scenarios, versions and approval";
        _sourceNotice.Text = workspace.HasFits
            ? $"{workspace.Fits.Count.ToString(CultureInfo.InvariantCulture)} saved decline fit(s) on offer, or type the " +
              "parameters by hand. The method list is " + workspace.ReferenceText + "."
            : workspace.NoSourceReason;

        _notice.Text = workspace.HistoryFailed
            ? "The saved forecasts are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowForecast(workspace.Selected ?? workspace.Forecasts.FirstOrDefault());

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
    }

    private void ShowForecast(OgForecast? forecast)
    {
        _forecast = forecast;

        if (forecast is null)
        {
            _points.ItemsSource = null;
            _summary.Text = "no forecast is selected";
            _approval.Text = string.Empty;
            _refusal.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _points.ItemsSource = forecast.Points.ToList();
        _summary.Text = forecast.IsRefused ? $"REFUSED · {forecast.Name ?? "(no name)"}" : forecast.Describe();
        _approval.Text = forecast.IsSaved
            ? "Units: " + forecast.UnitsText + " · source: " + forecast.SourceText + " · " + forecast.ApprovalText
            : "Not saved yet. Units: " + forecast.UnitsText + " · source: " + forecast.SourceText;
        _refusal.Text = forecast.IsRefused ? forecast.RefusalText : string.Empty;
        _detail.Text = forecast.IsRefused ? forecast.RefusalText : forecast.PointsText;
    }

    private void SelectSaved()
    {
        if (_saved.SelectedItem is not OgForecast selected)
        {
            return;
        }

        ShowForecast(selected);
    }

    private string? SelectedFitId() =>
        _fit.SelectedIndex <= 0 ? null : _fit.SelectedItem as string;

    private void RefreshEnabled()
    {
        _generateButton.IsEnabled = CanGenerate;
        _saveButton.IsEnabled = CanSave;
        _approveButton.IsEnabled = CanApprove;
        _retryButton.IsEnabled = _forecasts.Pending is not null;

        if (_workspace is { HasFits: false } workspace && string.IsNullOrWhiteSpace(_manualQi.Text))
        {
            _sourceNotice.Text = workspace.NoSourceReason;
        }
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

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? Parse(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : null;

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
