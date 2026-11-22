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
/// RES-06: decline curve analysis - a fit to real production history with its parameters, its fit quality, its
/// exclusions, the bounds it was held inside and an independent cross-check.
/// </summary>
/// <remarks>
/// The bounds are shown with every fit, and a b the data pushes to the upper bound is refused rather than reported at the
/// bound (AC-2, SG-06-6). Excluded points are listed with their reasons (AC-3). The cross-check is shown beside the fit
/// whether it agrees or not (AC-4). A refused or non-converged fit draws no curve (AC-5). Nothing here is random, so the
/// same history gives the same fit (AC-6).
/// </remarks>
public sealed class DeclineCurvePageView : OgPageBase
{
    public const string Route = "declinecurve";

    private readonly OgApplication _application;
    private readonly IOgDeclineCurve _decline;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _summary;
    private readonly TextBlock _bounds;
    private readonly TextBlock _refusal;
    private readonly TextBlock _crossCheck;
    private readonly TextBlock _exclusions;
    private readonly TextBlock _gaps;
    private readonly TextBlock _curve;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _fluid;
    private readonly TextBox _from;
    private readonly TextBox _to;
    private readonly ComboBox _model;
    private readonly TextBox _fixedB;
    private readonly TextBox _economicLimit;
    private readonly DataGrid _history;
    private readonly Button _saveButton;

    private OgDeclineWorkspace? _workspace;
    private OgDeclineFit? _fit;

    public DeclineCurvePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _decline = application.DeclineCurve;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _bounds = Body(string.Empty);
        _bounds.TextWrapping = TextWrapping.Wrap;
        _bounds.Margin = new Thickness(0, 0, 0, 4);

        _refusal = Body(string.Empty);
        _refusal.TextWrapping = TextWrapping.Wrap;
        _refusal.Margin = new Thickness(0, 0, 0, 4);

        _crossCheck = Body(string.Empty);
        _crossCheck.TextWrapping = TextWrapping.Wrap;
        _crossCheck.Margin = new Thickness(0, 0, 0, 4);

        _exclusions = Body(string.Empty);
        _exclusions.TextWrapping = TextWrapping.Wrap;
        _exclusions.Margin = new Thickness(0, 0, 0, 4);

        _gaps = Body(string.Empty);
        _gaps.TextWrapping = TextWrapping.Wrap;
        _gaps.Margin = new Thickness(0, 0, 0, 8);

        _curve = Body(string.Empty);
        _curve.TextWrapping = TextWrapping.Wrap;
        _curve.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 140);
        _from = Field("From yyyy-MM-dd", 130);
        _to = Field("To yyyy-MM-dd", 130);
        _fixedB = Field("Fixed b", 80);
        _economicLimit = Field("Economic limit rate", 130);

        _fluid = new ComboBox
        {
            MinWidth = 100,
            Margin = new Thickness(0, 0, 8, 0),
            ItemsSource = DeclineFluids.All,
            SelectedIndex = 0,
        };

        Named(_fluid, "Fluid");

        _model = new ComboBox
        {
            MinWidth = 200,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(_model, "Decline model");

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
                Text("Saved", "RecordedAt", 140),
                Text("Well", "WellId", 130),
                Text("Model", "ModelText", 170),
                Text("qi", "QiText", 90),
                Text("Di", "DiText", 90),
                Text("b", "BText", 70),
                Text("EUR", "EurText", 120),
                Text("Points", "PointsText", 130),
                Text("Cross-check", "CrossCheck", 220),
            },
        };

        Named(_history, "Saved decline fits");

        _saveButton = Button("Save fit", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;

        var where = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        where.Children.Add(Caption("Well"));
        where.Children.Add(_well);
        where.Children.Add(Caption("Fluid"));
        where.Children.Add(_fluid);
        where.Children.Add(Caption("From"));
        where.Children.Add(_from);
        where.Children.Add(Caption("To"));
        where.Children.Add(_to);

        var modelBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        modelBar.Children.Add(Caption("Model"));
        modelBar.Children.Add(_model);
        modelBar.Children.Add(Caption("b"));
        modelBar.Children.Add(_fixedB);
        modelBar.Children.Add(Caption("Economic limit"));
        modelBar.Children.Add(_economicLimit);
        modelBar.Children.Add(Button("Fit", (_, _) => _ = FitAsync()));
        modelBar.Children.Add(_saveButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("History"));
        panel.Children.Add(where);
        panel.Children.Add(Caption("Fit"));
        panel.Children.Add(modelBar);
        panel.Children.Add(_summary);
        panel.Children.Add(_bounds);
        panel.Children.Add(_refusal);
        panel.Children.Add(Caption("Cross-check"));
        panel.Children.Add(_crossCheck);
        panel.Children.Add(Caption("Excluded points"));
        panel.Children.Add(_exclusions);
        panel.Children.Add(Caption("Gaps"));
        panel.Children.Add(_gaps);
        panel.Children.Add(Caption("Curve"));
        panel.Children.Add(_curve);
        panel.Children.Add(Caption("Saved fits"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        Load();
    }

    public OgDeclineWorkspace? Workspace => _workspace;

    public OgDeclineFit? Fit => _fit;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string SummaryText => _summary.Text;

    public string BoundsText => _bounds.Text;

    public string RefusalText => _refusal.Text;

    public string CrossCheckText => _crossCheck.Text;

    public string ExclusionsText => _exclusions.Text;

    public string GapsText => _gaps.Text;

    public string CurveText => _curve.Text;

    public string DetailText => _detail.Text;

    public bool CanSave => _fit is { IsRefused: false, HasCurve: true, IsSaved: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _decline.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Decline curve";
            _notice.Text = loaded.Message ?? "The saved decline fits could not be read.";
            ShowState(OgViewState.Failed, "The saved decline fits could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        _workspace = loaded.Value;

        _model.ItemsSource = loaded.Value.Models.ToList();
        _model.SelectedIndex = _model.SelectedIndex < 0 ? 2 : _model.SelectedIndex;

        _fluid.ItemsSource = loaded.Value.Fluids.ToList();
        _fluid.SelectedIndex = _fluid.SelectedIndex < 0 ? 0 : _fluid.SelectedIndex;

        _history.ItemsSource = loaded.Value.History.ToList();

        _header.Text = "Decline curve · fit the well's own production history";
        _bounds.Text = loaded.Value.BoundsDisplay;
        _notice.Text = loaded.Value.HistoryFailed
            ? "The saved fits are unavailable: " + loaded.Value.HistoryReason + ". New fits are still allowed."
            : loaded.Value.HistoryStateText + " · " + DeclineRules.DeterminismText;

        ShowState(OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return true;
    }

    public async Task<bool> FitAsync()
    {
        var inputs = new OgDeclineInputs(
            Empty(_well.Text),
            _fluid.SelectedItem as string,
            Date(_from.Text),
            Date(_to.Text),
            _model.SelectedItem as string,
            Parse(_fixedB.Text),
            Parse(_economicLimit.Text));

        var analyzed = await _decline.AnalyzeAsync(inputs).ConfigureAwait(true);

        if (!analyzed.Success || analyzed.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _fit = null;
            _history.ItemsSource = null;
            _summary.Text = string.Empty;
            _refusal.Text = string.Empty;
            _crossCheck.Text = string.Empty;
            _exclusions.Text = string.Empty;
            _gaps.Text = string.Empty;
            _curve.Text = string.Empty;
            _notice.Text = analyzed.Message ?? "The fit could not be run.";
            ShowState(OgViewState.Failed, "The fit could not be run.", analyzed.Reference);
            RefreshEnabled();
            return false;
        }

        _workspace = analyzed.Value;
        _fit = analyzed.Value.Fit;

        _history.ItemsSource = analyzed.Value.History.ToList();
        _bounds.Text = analyzed.Value.BoundsDisplay;

        if (_fit is null)
        {
            _summary.Text = "no fit was produced";
            RefreshEnabled();
            return false;
        }

        LastFailed = _fit.IsRefused;

        _summary.Text = _fit.IsRefused
            ? $"REFUSED · {_fit.ModelText}"
            : $"{_fit.ModelText} · qi {_fit.QiText} · Di {_fit.DiText} · b {_fit.BText} · EUR {_fit.EurText} · " +
              $"R2 {_fit.R2Text} · {_fit.PointsText}";

        _refusal.Text = _fit.IsRefused ? _fit.RefusalText : _fit.BoundsText;

        _crossCheck.Text = _fit.CrossCheck.Text;

        _exclusions.Text = _fit.PointsExcluded == 0
            ? "no history point was excluded"
            : _fit.ExclusionsText;

        _gaps.Text = _fit.GapsText;
        _curve.Text = _fit.CurveText;
        _detail.Text = _fit.Describe();

        _notice.Text = _fit.IsRefused
            ? "The fit was refused; no curve is drawn and nothing can be saved."
            : _fit.CrossCheck.Available && !_fit.CrossCheck.Agrees
                ? "The two fitters disagree beyond the stated tolerance; both are shown and the disagreement stands."
                : "Fit complete. Save it to keep its parameters, exclusions and cross-check.";

        ShowState(_fit.IsRefused ? OgViewState.Failed : OgViewState.Ready, _notice.Text);
        RefreshEnabled();
        return !_fit.IsRefused;
    }

    public async Task<bool> SaveAsync()
    {
        if (_fit is not { } fit || !CanSave)
        {
            _notice.Text = "Run a fit that succeeds before saving it.";
            return false;
        }

        var saved = await _decline.SaveAsync(fit, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The fit could not be saved.";
            ShowState(OgViewState.Failed, "The fit could not be saved.", saved.Reference);
            return false;
        }

        _workspace = saved.Value;
        _history.ItemsSource = saved.Value.History.ToList();
        _fit = fit with { Actor = _application.Session.UserId, RecordedAt = DateTime.UtcNow };

        _application.AuditLog.Add(
            "forecasting.dca.save",
            "reservoir",
            fit.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its parameters, exclusions and cross-check. " + saved.Value.HistoryStateText;
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

    private static DateTime? Date(string? text) =>
        DateTime.TryParse(text?.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
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
