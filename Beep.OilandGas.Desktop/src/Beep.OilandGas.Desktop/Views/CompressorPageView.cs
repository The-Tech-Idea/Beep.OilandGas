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
/// LFT-08: compressor analysis - a centrifugal or reciprocating machine checked against its duty, with the staging, the
/// head, the power and every reason it cannot meet the duty.
/// </summary>
/// <remarks>
/// The staging is the module's own plan and prints its interstage temperature limit in both units (AC-2). An infeasible
/// machine lists every reason and shows no power figure (AC-3). The kinds and tokens come from
/// <c>CompressorAnalysisWellKnown</c> through the workspace (AC-4), and power carries its unit (AC-5).
/// </remarks>
public sealed class CompressorPageView : OgPageBase
{
    public const string Route = "compressor";

    private readonly OgApplication _application;
    private readonly IOgCompressor _compressor;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _staging;
    private readonly TextBlock _reasons;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _kind;
    private readonly TextBox _suctionPressure;
    private readonly TextBox _dischargePressure;
    private readonly TextBox _suctionTemperature;
    private readonly TextBox _dischargeTemperature;
    private readonly TextBox _gasFlow;
    private readonly TextBox _gasGravity;
    private readonly TextBox _polytropicEfficiency;
    private readonly TextBox _heatRatio;
    private readonly TextBox _mechanicalEfficiency;
    private readonly TextBox _stages;
    private readonly TextBox _speed;
    private readonly TextBox _cylinderDiameter;
    private readonly TextBox _strokeLength;
    private readonly TextBox _rotationalSpeed;
    private readonly TextBox _cylinders;
    private readonly DataGrid _stagingGrid;
    private readonly DataGrid _history;
    private readonly Button _analyseButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgCompressorWorkspace? _workspace;
    private OgCompressorResult? _result;

    public CompressorPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _compressor = application.Compressors;

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

        _staging = Body(string.Empty);
        _staging.TextWrapping = TextWrapping.Wrap;
        _staging.Margin = new Thickness(0, 0, 0, 4);

        _reasons = Body(string.Empty);
        _reasons.TextWrapping = TextWrapping.Wrap;
        _reasons.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _suctionPressure = Field("Suction pressure psia", 150);
        _suctionPressure.Text = "300";
        _dischargePressure = Field("Discharge pressure psia", 150);
        _dischargePressure.Text = "900";
        _suctionTemperature = Field("Suction temperature degF", 150);
        _suctionTemperature.Text = "100";
        _dischargeTemperature = Field("Discharge temperature degF", 160);
        _dischargeTemperature.Text = "120";
        _gasFlow = Field("Gas flow MMscf/d", 130);
        _gasFlow.Text = "5";
        _gasGravity = Field("Gas specific gravity", 130);
        _gasGravity.Text = "0.65";
        _polytropicEfficiency = Field("Polytropic efficiency", 140);
        _polytropicEfficiency.Text = "0.75";
        _heatRatio = Field("Specific heat ratio", 130);
        _heatRatio.Text = "1.3";
        _mechanicalEfficiency = Field("Mechanical efficiency", 140);
        _mechanicalEfficiency.Text = "0.95";
        _stages = Field("Stages (blank = the plan decides)", 180);
        _speed = Field("Speed rpm", 110);
        _speed.Text = "10000";
        _cylinderDiameter = Field("Cylinder diameter in", 140);
        _cylinderDiameter.Text = "10";
        _strokeLength = Field("Stroke length in", 120);
        _strokeLength.Text = "12";
        _rotationalSpeed = Field("Rotational speed rpm", 140);
        _rotationalSpeed.Text = "900";
        _cylinders = Field("Cylinders", 90);
        _cylinders.Text = "2";

        _kind = Picker("Compressor kind", 170);

        _stagingGrid = new DataGrid
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
                Text("Stages", "Stages", 80),
                Text("Stage ratio", "RatioText", 110),
                Text("Hottest stage", "TemperatureText", 130),
                Text("Outcome", "OutcomeText", 100),
            },
        };

        Named(_stagingGrid, "Staging options");

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
                Text("Kind", "KindText", 120),
                Text("Ratio", "RatioText", 80),
                Text("Power", "PowerText", 190),
                Text("Verdict", "VerdictText", 110),
                Text("Saved by", "Actor", 100),
            },
        };

        Named(_history, "Saved compressor results");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _analyseButton = Button("Analyse", (_, _) => _ = AnalyseAsync());
        _saveButton = Button("Save result", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Kind"));
        wellBar.Children.Add(_kind);
        wellBar.Children.Add(Caption("Stages"));
        wellBar.Children.Add(_stages);

        var pressureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pressureBar.Children.Add(Caption("Suction"));
        pressureBar.Children.Add(_suctionPressure);
        pressureBar.Children.Add(Caption("Discharge"));
        pressureBar.Children.Add(_dischargePressure);
        pressureBar.Children.Add(Caption("T1"));
        pressureBar.Children.Add(_suctionTemperature);
        pressureBar.Children.Add(Caption("T2"));
        pressureBar.Children.Add(_dischargeTemperature);

        var gasBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        gasBar.Children.Add(Caption("Flow"));
        gasBar.Children.Add(_gasFlow);
        gasBar.Children.Add(Caption("Gas SG"));
        gasBar.Children.Add(_gasGravity);

        var machineBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        machineBar.Children.Add(Caption("ηp"));
        machineBar.Children.Add(_polytropicEfficiency);
        machineBar.Children.Add(Caption("k"));
        machineBar.Children.Add(_heatRatio);
        machineBar.Children.Add(Caption("ηm"));
        machineBar.Children.Add(_mechanicalEfficiency);
        machineBar.Children.Add(Caption("Speed"));
        machineBar.Children.Add(_speed);

        var reciprocatingBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reciprocatingBar.Children.Add(Caption("Cylinder"));
        reciprocatingBar.Children.Add(_cylinderDiameter);
        reciprocatingBar.Children.Add(Caption("Stroke"));
        reciprocatingBar.Children.Add(_strokeLength);
        reciprocatingBar.Children.Add(Caption("RPM"));
        reciprocatingBar.Children.Add(_rotationalSpeed);
        reciprocatingBar.Children.Add(Caption("Cylinders"));
        reciprocatingBar.Children.Add(_cylinders);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_analyseButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Machine"));
        panel.Children.Add(wellBar);
        panel.Children.Add(Caption("Duty"));
        panel.Children.Add(pressureBar);
        panel.Children.Add(Caption("Gas"));
        panel.Children.Add(gasBar);
        panel.Children.Add(Caption("Efficiencies"));
        panel.Children.Add(machineBar);
        panel.Children.Add(Caption("Reciprocating"));
        panel.Children.Add(reciprocatingBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Staging"));
        panel.Children.Add(_staging);
        panel.Children.Add(_stagingGrid);
        panel.Children.Add(Caption("Reasons"));
        panel.Children.Add(_reasons);
        panel.Children.Add(Caption("Saved results"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _suctionPressure, _dischargePressure, _suctionTemperature, _dischargeTemperature,
                     _gasFlow, _gasGravity, _polytropicEfficiency, _heatRatio, _mechanicalEfficiency, _stages,
                     _speed, _cylinderDiameter, _strokeLength, _rotationalSpeed, _cylinders,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _kind.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgCompressorWorkspace? Workspace => _workspace;

    public OgCompressorResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string StagingText => _staging.Text;

    public string ReasonsText => _reasons.Text;

    public string DetailText => _detail.Text;

    public bool CanAnalyse => CompressorRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _compressor.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Compressor";
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

        if (CompressorRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var analysed = await _compressor.AnalyseAsync(inputs).ConfigureAwait(true);

        if (!analysed.Success || analysed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _staging.Text = string.Empty;
            _reasons.Text = string.Empty;
            _stagingGrid.ItemsSource = null;
            _notice.Text = analysed.Message ?? "The analysis could not be run.";
            ShowState(OgViewState.Failed, "The analysis could not be run.", analysed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(analysed.Value);

        // An infeasible machine is a completed analysis with a verdict, not a failed run.
        LastFailed = false;
        ShowState(_result is { Feasible: false } ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Analyse the machine before saving the result.";
            return false;
        }

        var saved = await _compressor.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

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
            "lift.compressor.save",
            "lift",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its kind, its staging and its efficiency. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _compressor.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

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

    private void Apply(OgCompressorWorkspace workspace)
    {
        _workspace = workspace;

        _kind.ItemsSource = workspace.Kinds.ToList();
        _kind.SelectedIndex = _kind.SelectedIndex < 0 ? 0 : _kind.SelectedIndex;

        _history.ItemsSource = workspace.Results.ToList();

        _header.Text = "Compressor · duty, staging and power";
        _reference.Text = workspace.ReferenceText + " · " + workspace.KindNote;
        _notice.Text = workspace.HistoryFailed
            ? "The saved results are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgCompressorResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no result is selected";
            _staging.Text = string.Empty;
            _reasons.Text = string.Empty;
            _stagingGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text =
            $"{result.VerdictText} · {result.Describe()}";
        _staging.Text = result.StageText + " · final discharge " + result.DischargeTemperatureText +
                        " · head " + result.HeadText + " · " + result.EfficiencyText;
        _reasons.Text = result.ReasonsText;
        _stagingGrid.ItemsSource = result.Staging.ToList();
        _detail.Text = result.Feasible
            ? "the machine meets the duty: " + result.PowerText
            : "no power figure is presented for a machine that cannot meet the duty";
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgCompressorResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = CompressorRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The analysis needs " + string.Join(", ", missing) + ".";

        _analyseButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _compressor.Pending is not null;
    }

    private OgCompressorInputs Inputs() =>
        new(
            Empty(_well.Text),
            _kind.SelectedItem as string,
            Parse(_suctionPressure.Text),
            Parse(_dischargePressure.Text),
            Parse(_suctionTemperature.Text),
            Parse(_dischargeTemperature.Text),
            Parse(_gasFlow.Text),
            Parse(_gasGravity.Text),
            Parse(_polytropicEfficiency.Text),
            Parse(_heatRatio.Text),
            Parse(_mechanicalEfficiency.Text),
            (int?)Parse(_stages.Text),
            Parse(_speed.Text),
            Parse(_cylinderDiameter.Text),
            Parse(_strokeLength.Text),
            Parse(_rotationalSpeed.Text),
            (int?)Parse(_cylinders.Text),
            null,
            null);

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
