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
/// LFT-06: hydraulic pumps - a jet or piston design checked by the module's own calculators, with its cavitation index and
/// every finding listed by severity.
/// </summary>
/// <remarks>
/// The nozzle and throat (or piston and stroke) come from the module's standard sets (AC-5). Cavitation is printed with its
/// index and threshold (AC-2), and a clamped system efficiency is a finding rather than a silent answer (AC-3). Every
/// finding states its severity, and an error finding blocks the save (AC-4). A saved design keeps its kind and all its
/// inputs (AC-6).
/// </remarks>
public sealed class HydraulicPumpPageView : OgPageBase
{
    public const string Route = "hydraulicpump";

    private readonly OgApplication _application;
    private readonly IOgHydraulicPump _pump;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _efficiency;
    private readonly TextBlock _clamp;
    private readonly TextBlock _cavitation;
    private readonly TextBlock _jet;
    private readonly TextBlock _pistonResult;
    private readonly TextBlock _findings;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _kind;
    private readonly TextBox _wellDepth;
    private readonly TextBox _tubing;
    private readonly TextBox _casing;
    private readonly TextBox _wellheadPressure;
    private readonly TextBox _bottomHolePressure;
    private readonly TextBox _wellheadTemperature;
    private readonly TextBox _bottomHoleTemperature;
    private readonly TextBox _oilGravity;
    private readonly TextBox _waterCut;
    private readonly TextBox _gor;
    private readonly TextBox _gasGravity;
    private readonly TextBox _desiredRate;
    private readonly TextBox _pumpDepth;
    private readonly ComboBox _nozzle;
    private readonly ComboBox _throat;
    private readonly TextBox _diffuser;
    private readonly ComboBox _piston;
    private readonly TextBox _rod;
    private readonly ComboBox _stroke;
    private readonly ComboBox _strokesPerMinute;
    private readonly TextBox _powerFluidPressure;
    private readonly TextBox _powerFluidRate;
    private readonly TextBox _powerFluidGravity;
    private readonly DataGrid _history;
    private readonly Button _designButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgHydraulicWorkspace? _workspace;
    private OgHydraulicDesign? _design;

    public HydraulicPumpPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _pump = application.HydraulicPumps;

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

        _efficiency = Body(string.Empty);
        _efficiency.TextWrapping = TextWrapping.Wrap;
        _efficiency.Margin = new Thickness(0, 0, 0, 4);

        _clamp = Body(string.Empty);
        _clamp.TextWrapping = TextWrapping.Wrap;
        _clamp.Margin = new Thickness(0, 0, 0, 4);

        _cavitation = Body(string.Empty);
        _cavitation.TextWrapping = TextWrapping.Wrap;
        _cavitation.Margin = new Thickness(0, 0, 0, 4);

        _jet = Body(string.Empty);
        _jet.TextWrapping = TextWrapping.Wrap;
        _jet.Margin = new Thickness(0, 0, 0, 4);

        _pistonResult = Body(string.Empty);
        _pistonResult.TextWrapping = TextWrapping.Wrap;
        _pistonResult.Margin = new Thickness(0, 0, 0, 4);

        _findings = Body(string.Empty);
        _findings.TextWrapping = TextWrapping.Wrap;
        _findings.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _wellDepth = Field("Well depth ft", 110);
        _wellDepth.Text = "8000";
        _tubing = Field("Tubing diameter in", 130);
        _tubing.Text = "2.875";
        _casing = Field("Casing diameter in", 130);
        _casing.Text = "7";
        _wellheadPressure = Field("Wellhead pressure psia", 140);
        _wellheadPressure.Text = "200";
        _bottomHolePressure = Field("Bottom hole pressure psia", 150);
        _bottomHolePressure.Text = "2500";
        _wellheadTemperature = Field("Wellhead temperature R", 150);
        _wellheadTemperature.Text = "560";
        _bottomHoleTemperature = Field("Bottom hole temperature R", 160);
        _bottomHoleTemperature.Text = "660";
        _oilGravity = Field("Oil gravity API", 110);
        _oilGravity.Text = "35";
        _waterCut = Field("Water cut 0-1", 100);
        _waterCut.Text = "0.3";
        _gor = Field("Gas-oil ratio scf/STB", 130);
        _gor.Text = "300";
        _gasGravity = Field("Gas specific gravity", 130);
        _gasGravity.Text = "0.65";
        _desiredRate = Field("Desired rate bbl/d", 120);
        _desiredRate.Text = "400";
        _pumpDepth = Field("Pump depth ft", 110);
        _pumpDepth.Text = "8000";
        _diffuser = Field("Diffuser diameter in", 130);
        _diffuser.Text = "0.75";
        _rod = Field("Rod diameter in", 110);
        _rod.Text = "0.75";
        _powerFluidPressure = Field("Power fluid pressure psia", 150);
        _powerFluidPressure.Text = "3000";
        _powerFluidRate = Field("Power fluid rate bbl/d", 140);
        _powerFluidRate.Text = "600";
        _powerFluidGravity = Field("Power fluid SG", 110);
        _powerFluidGravity.Text = "1.07";

        _kind = Picker("Pump kind", 170);
        _nozzle = Picker("Nozzle diameter", 130);
        _throat = Picker("Throat diameter", 130);
        _piston = Picker("Piston diameter", 130);
        _stroke = Picker("Stroke length", 120);
        _strokesPerMinute = Picker("Strokes per minute", 140);

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
                Text("Kind", "KindText", 160),
                Text("Production", "ProductionText", 110),
                Text("Efficiency", "EfficiencyText", 190),
                Text("Cavitation", "CavitationMargin", 100),
                Text("Saved by", "Actor", 110),
            },
        };

        Named(_history, "Saved hydraulic pump designs");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _designButton = Button("Design", (_, _) => _ = DesignAsync());
        _saveButton = Button("Save design", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Kind"));
        wellBar.Children.Add(_kind);
        wellBar.Children.Add(Caption("Depth"));
        wellBar.Children.Add(_wellDepth);
        wellBar.Children.Add(Caption("Tubing"));
        wellBar.Children.Add(_tubing);
        wellBar.Children.Add(Caption("Casing"));
        wellBar.Children.Add(_casing);

        var pressureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pressureBar.Children.Add(Caption("Wellhead"));
        pressureBar.Children.Add(_wellheadPressure);
        pressureBar.Children.Add(Caption("Bottom hole"));
        pressureBar.Children.Add(_bottomHolePressure);
        pressureBar.Children.Add(Caption("Twh"));
        pressureBar.Children.Add(_wellheadTemperature);
        pressureBar.Children.Add(Caption("Twf"));
        pressureBar.Children.Add(_bottomHoleTemperature);

        var fluidBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fluidBar.Children.Add(Caption("API"));
        fluidBar.Children.Add(_oilGravity);
        fluidBar.Children.Add(Caption("Water cut"));
        fluidBar.Children.Add(_waterCut);
        fluidBar.Children.Add(Caption("GOR"));
        fluidBar.Children.Add(_gor);
        fluidBar.Children.Add(Caption("Gas SG"));
        fluidBar.Children.Add(_gasGravity);
        fluidBar.Children.Add(Caption("Desired"));
        fluidBar.Children.Add(_desiredRate);
        fluidBar.Children.Add(Caption("Pump depth"));
        fluidBar.Children.Add(_pumpDepth);

        var jetBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        jetBar.Children.Add(Caption("Nozzle"));
        jetBar.Children.Add(_nozzle);
        jetBar.Children.Add(Caption("Throat"));
        jetBar.Children.Add(_throat);
        jetBar.Children.Add(Caption("Diffuser"));
        jetBar.Children.Add(_diffuser);

        var pistonBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pistonBar.Children.Add(Caption("Piston"));
        pistonBar.Children.Add(_piston);
        pistonBar.Children.Add(Caption("Rod"));
        pistonBar.Children.Add(_rod);
        pistonBar.Children.Add(Caption("Stroke"));
        pistonBar.Children.Add(_stroke);
        pistonBar.Children.Add(Caption("SPM"));
        pistonBar.Children.Add(_strokesPerMinute);

        var powerBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        powerBar.Children.Add(Caption("Power fluid pressure"));
        powerBar.Children.Add(_powerFluidPressure);
        powerBar.Children.Add(Caption("Rate"));
        powerBar.Children.Add(_powerFluidRate);
        powerBar.Children.Add(Caption("SG"));
        powerBar.Children.Add(_powerFluidGravity);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_designButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_reference);
        panel.Children.Add(Caption("Well"));
        panel.Children.Add(wellBar);
        panel.Children.Add(Caption("Pressures and temperatures"));
        panel.Children.Add(pressureBar);
        panel.Children.Add(Caption("Fluids"));
        panel.Children.Add(fluidBar);
        panel.Children.Add(Caption("Jet pump"));
        panel.Children.Add(jetBar);
        panel.Children.Add(Caption("Piston pump"));
        panel.Children.Add(pistonBar);
        panel.Children.Add(Caption("Power fluid"));
        panel.Children.Add(powerBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_efficiency);
        panel.Children.Add(_clamp);
        panel.Children.Add(_cavitation);
        panel.Children.Add(_jet);
        panel.Children.Add(_pistonResult);
        panel.Children.Add(Caption("Findings"));
        panel.Children.Add(_findings);
        panel.Children.Add(Caption("Saved designs"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _wellDepth, _tubing, _casing, _wellheadPressure, _bottomHolePressure,
                     _wellheadTemperature, _bottomHoleTemperature, _oilGravity, _waterCut, _gor, _gasGravity,
                     _desiredRate, _pumpDepth, _diffuser, _rod, _powerFluidPressure, _powerFluidRate,
                     _powerFluidGravity,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _kind.SelectionChanged += (_, _) => RefreshEnabled();

        Load();
    }

    public OgHydraulicWorkspace? Workspace => _workspace;

    public OgHydraulicDesign? Design => _design;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string EfficiencyText => _efficiency.Text;

    public string ClampText => _clamp.Text;

    public string CavitationText => _cavitation.Text;

    public string JetText => _jet.Text;

    public string PistonText => _pistonResult.Text;

    public string FindingsText => _findings.Text;

    public string DetailText => _detail.Text;

    public bool IsJetSelected => HydraulicRules.IsJet(_kind.SelectedItem as string);

    public bool CanDesign => HydraulicRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _design is { IsSaved: false, HasBlockingFinding: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _pump.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Hydraulic pumps";
            _notice.Text = loaded.Message ?? "The saved designs could not be read.";
            ShowState(OgViewState.Failed, "The saved designs could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> DesignAsync()
    {
        var inputs = Inputs();

        if (HydraulicRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var designed = await _pump.DesignAsync(inputs).ConfigureAwait(true);

        if (!designed.Success || designed.Value is null)
        {
            LastFailed = true;
            _design = null;
            _summary.Text = string.Empty;
            _efficiency.Text = string.Empty;
            _clamp.Text = string.Empty;
            _cavitation.Text = string.Empty;
            _jet.Text = string.Empty;
            _pistonResult.Text = string.Empty;
            _findings.Text = string.Empty;
            _notice.Text = designed.Message ?? "The design could not be run.";
            ShowState(OgViewState.Failed, "The design could not be run.", designed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(designed.Value);

        // A blocking finding is a completed design with a refused input, not a failed run.
        LastFailed = _design is { HasBlockingFinding: true };
        ShowState(_design is { HasBlockingFinding: true } ? OgViewState.Failed : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_design is not { } design || !CanSave)
        {
            _notice.Text = _design is { HasBlockingFinding: true } blocked
                ? "The design cannot be saved: " +
                  string.Join(" ", blocked.Blocking.Select(finding => finding.Message))
                : "Design the pump before saving it.";
            return false;
        }

        var saved = await _pump.SaveAsync(design, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The design was not saved.";
            ShowState(OgViewState.Failed, "The design was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "lift.hydraulic.save",
            "lift",
            design.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with its kind, every input and the findings. " + saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _pump.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

        if (!retried.Success || retried.Value is null)
        {
            LastFailed = true;
            _notice.Text = retried.Message ?? "There is no design waiting to be retried.";
            ShowState(OgViewState.Failed, "There is no design waiting to be retried.", retried.Reference);
            return false;
        }

        Apply(retried.Value);
        _notice.Text = "The retained design was saved on the retry.";
        RefreshEnabled();
        return true;
    }

    private void Apply(OgHydraulicWorkspace workspace)
    {
        _workspace = workspace;

        _kind.ItemsSource = workspace.Kinds.ToList();
        _kind.SelectedIndex = _kind.SelectedIndex < 0 ? 0 : _kind.SelectedIndex;

        _nozzle.ItemsSource = workspace.StandardNozzleDiameters.ToList();
        _nozzle.SelectedIndex = _nozzle.SelectedIndex < 0 ? 1 : _nozzle.SelectedIndex;

        _throat.ItemsSource = workspace.StandardThroatDiameters.ToList();
        _throat.SelectedIndex = _throat.SelectedIndex < 0 ? 2 : _throat.SelectedIndex;

        _piston.ItemsSource = workspace.StandardPistonDiameters.ToList();
        _piston.SelectedIndex = _piston.SelectedIndex < 0 ? 3 : _piston.SelectedIndex;

        _stroke.ItemsSource = workspace.StandardStrokeLengths.ToList();
        _stroke.SelectedIndex = _stroke.SelectedIndex < 0 ? 4 : _stroke.SelectedIndex;

        _strokesPerMinute.ItemsSource = workspace.StandardStrokesPerMinute.ToList();
        _strokesPerMinute.SelectedIndex = _strokesPerMinute.SelectedIndex < 0 ? 3 : _strokesPerMinute.SelectedIndex;

        _history.ItemsSource = workspace.Designs.ToList();

        _header.Text = "Hydraulic pumps · jet and piston, with their findings";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved designs are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowDesign(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowDesign(OgHydraulicDesign? design)
    {
        _design = design;

        if (design is null)
        {
            _summary.Text = "no design is selected";
            _efficiency.Text = string.Empty;
            _clamp.Text = string.Empty;
            _cavitation.Text = string.Empty;
            _jet.Text = string.Empty;
            _piston.Text = string.Empty;
            _findings.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = design.Describe() + " · power fluid " + design.PowerFluidText;
        _efficiency.Text = "Efficiency: " + design.EfficiencyText;
        _clamp.Text = design.ClampText;
        _cavitation.Text = "Cavitation index: " + design.CavitationText;
        _jet.Text = design.JetText;
        _pistonResult.Text = design.PistonText;
        _findings.Text = design.FindingsText;
        _detail.Text =
            $"worst severity {design.WorstSeverity.ToLowerInvariant()} · " +
            (design.Findings.Count == 0
                ? "nothing to report"
                : $"{design.Findings.Count.ToString(CultureInfo.InvariantCulture)} finding(s), " +
                  $"{design.Blocking.Count.ToString(CultureInfo.InvariantCulture)} blocking");
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgHydraulicDesign selected)
        {
            return;
        }

        ShowDesign(selected);
    }

    private void RefreshEnabled()
    {
        var missing = HydraulicRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The design needs " + string.Join(", ", missing) + ".";

        _designButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _pump.Pending is not null;
    }

    private OgHydraulicInputs Inputs() =>
        new(
            Empty(_well.Text),
            _kind.SelectedItem as string,
            Parse(_wellDepth.Text),
            Parse(_tubing.Text),
            Parse(_casing.Text),
            Parse(_wellheadPressure.Text),
            Parse(_bottomHolePressure.Text),
            Parse(_wellheadTemperature.Text),
            Parse(_bottomHoleTemperature.Text),
            Parse(_oilGravity.Text),
            Parse(_waterCut.Text),
            Parse(_gor.Text),
            Parse(_gasGravity.Text),
            Parse(_desiredRate.Text),
            Parse(_pumpDepth.Text),
            _nozzle.SelectedItem is decimal nozzle ? nozzle : null,
            _throat.SelectedItem is decimal throat ? throat : null,
            Parse(_diffuser.Text),
            _piston.SelectedItem is decimal piston ? piston : null,
            Parse(_rod.Text),
            _stroke.SelectedItem is decimal stroke ? stroke : null,
            _strokesPerMinute.SelectedItem is decimal spm ? spm : null,
            Parse(_powerFluidPressure.Text),
            Parse(_powerFluidRate.Text),
            Parse(_powerFluidGravity.Text));

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
