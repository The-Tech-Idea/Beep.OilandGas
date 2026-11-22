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
/// WTA-03: pipeline analysis and integrity - the capacity a line can carry, its flow regime, the friction and the erosion
/// threshold with the standard that states it.
/// </summary>
/// <remarks>
/// The regime is printed with the result and names the Reynolds thresholds it was decided against (AC-3). The erosion
/// threshold always carries its source, and a line with none says "threshold source not configured" (AC-2). The
/// experimental tabs are listed as disabled with the reason (AC-4). The exported text is built from the same properties
/// the panels print, so it cannot disagree with them (AC-5).
/// </remarks>
public sealed class PipelinePageView : OgPageBase
{
    public const string Route = "pipeline";

    private readonly OgApplication _application;
    private readonly IOgPipeline _pipeline;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _regime;
    private readonly TextBlock _flow;
    private readonly TextBlock _erosion;
    private readonly TextBlock _notes;
    private readonly TextBlock _disabled;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly TextBox _pipelineId;
    private readonly ComboBox _mode;
    private readonly TextBox _diameter;
    private readonly TextBox _length;
    private readonly TextBox _roughness;
    private readonly TextBox _inlet;
    private readonly TextBox _outlet;
    private readonly TextBox _temperature;
    private readonly TextBox _gasGravity;
    private readonly TextBox _basePressure;
    private readonly TextBox _baseTemperature;
    private readonly TextBox _liquidGravity;
    private readonly TextBox _viscosity;
    private readonly TextBox _liquidRate;
    private readonly TextBox _sandRate;
    private readonly TextBox _particle;
    private readonly ComboBox _erosionService;
    private readonly DataGrid _history;
    private readonly Button _analyseButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgPipelineWorkspace? _workspace;
    private OgPipelineResult? _result;

    public PipelinePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _pipeline = application.Pipelines;

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

        _regime = Body(string.Empty);
        _regime.TextWrapping = TextWrapping.Wrap;
        _regime.Margin = new Thickness(0, 0, 0, 4);

        _flow = Body(string.Empty);
        _flow.TextWrapping = TextWrapping.Wrap;
        _flow.Margin = new Thickness(0, 0, 0, 4);

        _erosion = Body(string.Empty);
        _erosion.TextWrapping = TextWrapping.Wrap;
        _erosion.Margin = new Thickness(0, 4, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 0, 0, 4);

        _disabled = Body(string.Empty);
        _disabled.TextWrapping = TextWrapping.Wrap;
        _disabled.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well or line id", 130);
        _pipelineId = Field("Pipeline id", 120);
        _diameter = Field("Diameter in", 110);
        _diameter.Text = "12";
        _length = Field("Length mi", 100);
        _length.Text = "10";
        _roughness = Field("Roughness ft", 120);
        _roughness.Text = "0.00015";
        _inlet = Field("Inlet psia", 110);
        _inlet.Text = "1000";
        _outlet = Field("Outlet psia", 110);
        _outlet.Text = "800";
        _temperature = Field("Average temperature R", 160);
        _temperature.Text = "530";
        _gasGravity = Field("Gas specific gravity", 140);
        _gasGravity.Text = "0.65";
        _basePressure = Field("Base pressure psia", 140);
        _basePressure.Text = "14.7";
        _baseTemperature = Field("Base temperature R", 150);
        _baseTemperature.Text = "520";
        _liquidGravity = Field("Liquid specific gravity", 150);
        _liquidGravity.Text = "0.85";
        _viscosity = Field("Liquid viscosity cP", 140);
        _viscosity.Text = "5";
        _liquidRate = Field("Liquid rate bbl/d", 140);
        _liquidRate.Text = "5000";
        _sandRate = Field("Sand rate bbl/d", 130);
        _particle = Field("Particle diameter micron", 160);
        _particle.Text = "250";

        _mode = Picker("Mode", 190);
        _erosionService = Picker("Erosion service", 150);

        _history = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Line", "PipelineId", 100),
                Text("Mode", "ModeText", 160),
                Text("Capacity", "CapacityText", 130),
                Text("Regime", "Regime", 110),
                Text("Verdict", "VerdictText", 100),
                Text("Saved by", "Actor", 100),
            },
        };

        Named(_history, "Saved pipeline analyses");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _analyseButton = Button("Analyse", (_, _) => _ = AnalyseAsync());
        _saveButton = Button("Save result", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var identityBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        identityBar.Children.Add(Caption("Line"));
        identityBar.Children.Add(_well);
        identityBar.Children.Add(Caption("Id"));
        identityBar.Children.Add(_pipelineId);
        identityBar.Children.Add(Caption("Mode"));
        identityBar.Children.Add(_mode);

        var geometryBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        geometryBar.Children.Add(Caption("Diameter"));
        geometryBar.Children.Add(_diameter);
        geometryBar.Children.Add(Caption("Length"));
        geometryBar.Children.Add(_length);
        geometryBar.Children.Add(Caption("Roughness"));
        geometryBar.Children.Add(_roughness);

        var pressureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pressureBar.Children.Add(Caption("Inlet"));
        pressureBar.Children.Add(_inlet);
        pressureBar.Children.Add(Caption("Outlet"));
        pressureBar.Children.Add(_outlet);
        pressureBar.Children.Add(Caption("Temperature"));
        pressureBar.Children.Add(_temperature);

        var gasBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        gasBar.Children.Add(Caption("Gas SG"));
        gasBar.Children.Add(_gasGravity);
        gasBar.Children.Add(Caption("Base P"));
        gasBar.Children.Add(_basePressure);
        gasBar.Children.Add(Caption("Base T"));
        gasBar.Children.Add(_baseTemperature);

        var liquidBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        liquidBar.Children.Add(Caption("Liquid SG"));
        liquidBar.Children.Add(_liquidGravity);
        liquidBar.Children.Add(Caption("Viscosity"));
        liquidBar.Children.Add(_viscosity);
        liquidBar.Children.Add(Caption("Rate"));
        liquidBar.Children.Add(_liquidRate);

        var erosionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        erosionBar.Children.Add(Caption("Service"));
        erosionBar.Children.Add(_erosionService);
        erosionBar.Children.Add(Caption("Sand"));
        erosionBar.Children.Add(_sandRate);
        erosionBar.Children.Add(Caption("Particle"));
        erosionBar.Children.Add(_particle);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_analyseButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Line"));
        panel.Children.Add(identityBar);
        panel.Children.Add(Caption("Geometry"));
        panel.Children.Add(geometryBar);
        panel.Children.Add(Caption("Pressures and temperature"));
        panel.Children.Add(pressureBar);
        panel.Children.Add(Caption("Gas"));
        panel.Children.Add(gasBar);
        panel.Children.Add(Caption("Liquid"));
        panel.Children.Add(liquidBar);
        panel.Children.Add(Caption("Erosion"));
        panel.Children.Add(erosionBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_regime);
        panel.Children.Add(_flow);
        panel.Children.Add(Caption("Erosion threshold"));
        panel.Children.Add(_erosion);
        panel.Children.Add(Caption("Notes"));
        panel.Children.Add(_notes);
        panel.Children.Add(Caption("Tabs"));
        panel.Children.Add(_disabled);
        panel.Children.Add(Caption("Saved analyses"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _pipelineId, _diameter, _length, _roughness, _inlet, _outlet, _temperature,
                     _gasGravity, _basePressure, _baseTemperature, _liquidGravity, _viscosity, _liquidRate,
                     _sandRate, _particle,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _mode.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgPipelineWorkspace? Workspace => _workspace;

    public OgPipelineResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string RegimeText => _regime.Text;

    public string FlowText => _flow.Text;

    public string ErosionText => _erosion.Text;

    public string NotesText => _notes.Text;

    public string DisabledText => _disabled.Text;

    public string DetailText => _detail.Text;

    public string ExportText => _result?.ExportText ?? string.Empty;

    public bool CanAnalyse => PipelineRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, RegimeDetermined: true };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _pipeline.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Pipeline";
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

        if (PipelineRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var analysed = await _pipeline.AnalyseAsync(inputs).ConfigureAwait(true);

        if (!analysed.Success || analysed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _regime.Text = string.Empty;
            _flow.Text = string.Empty;
            _erosion.Text = string.Empty;
            _notes.Text = string.Empty;
            _notice.Text = analysed.Message ?? "The analysis could not be run.";
            ShowState(OgViewState.Failed, "The analysis could not be run.", analysed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(analysed.Value);

        // A line above its erosional limit is a completed analysis with a verdict, not a failed run.
        LastFailed = false;
        ShowState(_result is { Feasible: false } ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Run an analysis that decides the regime before saving it.";
            return false;
        }

        var saved = await _pipeline.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

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
            "pipeline.analysis.save",
            "welltest",
            result.PipelineId ?? result.WellId ?? "(no line)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its mode and inputs. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _pipeline.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

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

    private void Apply(OgPipelineWorkspace workspace)
    {
        _workspace = workspace;

        _mode.ItemsSource = workspace.Modes.ToList();
        _mode.SelectedIndex = _mode.SelectedIndex < 0 ? 0 : _mode.SelectedIndex;

        _erosionService.ItemsSource = workspace.ErosionServices.ToList();
        _erosionService.SelectedIndex = _erosionService.SelectedIndex < 0 ? 0 : _erosionService.SelectedIndex;

        _history.ItemsSource = workspace.Results.ToList();

        _header.Text = "Pipeline · capacity, regime and the erosion threshold";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved analyses are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;
        _disabled.Text = workspace.DisabledText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgPipelineResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no analysis is selected";
            _regime.Text = string.Empty;
            _flow.Text = string.Empty;
            _erosion.Text = string.Empty;
            _notes.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = $"{result.Describe()} · pressure drop {result.PressureDropText}";
        _regime.Text = "Flow regime: " + result.RegimeText;
        _flow.Text =
            $"Reynolds {result.ReynoldsText} · friction factor {result.FrictionText} · velocity " +
            (result.Velocity is { } velocity
                ? velocity.ToString("#,0.###", CultureInfo.InvariantCulture) + " " + OgPipelineResult.VelocityUnit
                : "not computed");
        _erosion.Text = result.ThresholdText;
        _notes.Text = result.NotesText;
        _detail.Text = result.SavedText + " · " + (result.Feasible
            ? "the line carries this capacity inside its limits"
            : "the line does not carry this capacity inside its limits");
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgPipelineResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = PipelineRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The analysis needs " + string.Join(", ", missing) + ".";

        _analyseButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _pipeline.Pending is not null;
    }

    private OgPipelineInputs Inputs() =>
        new(
            Empty(_well.Text),
            Empty(_pipelineId.Text),
            _mode.SelectedItem as string,
            Parse(_diameter.Text),
            Parse(_length.Text),
            Parse(_roughness.Text),
            Parse(_inlet.Text),
            Parse(_outlet.Text),
            Parse(_temperature.Text),
            Parse(_gasGravity.Text),
            Parse(_basePressure.Text),
            Parse(_baseTemperature.Text),
            Parse(_liquidGravity.Text),
            Parse(_viscosity.Text),
            Parse(_liquidRate.Text),
            Parse(_sandRate.Text),
            Parse(_particle.Text),
            _erosionService.SelectedItem as string);

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
