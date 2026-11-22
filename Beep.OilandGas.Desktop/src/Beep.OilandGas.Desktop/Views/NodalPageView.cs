using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// LFT-01: nodal analysis - where the inflow and the outflow meet, with both correlations named, the diagnostics that
/// explain a missing intersection, an independent cross-check and a tubing sweep.
/// </summary>
/// <remarks>
/// No value is inherited from a default: a missing input disables the run with the missing field named (the stage's
/// "no default-to-zero" rule and the feature's open question). A well that cannot lift is reported as **no intersection**
/// with the gap between the curves, never as a zero rate (AC-2). The curves are the owner's calculators'; this view holds
/// none of the arithmetic (AC-4). A cancelled sweep reports the points it completed (AC-6).
/// </remarks>
public sealed class NodalPageView : OgPageBase
{
    public const string Route = "nodal";

    private readonly OgApplication _application;
    private readonly IOgNodal _nodal;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _summary;
    private readonly TextBlock _binding;
    private readonly TextBlock _diagnostics;
    private readonly TextBlock _crossCheck;
    private readonly TextBlock _curves;
    private readonly TextBlock _sensitivityState;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly TextBox _reservoirPressure;
    private readonly TextBox _bubblePoint;
    private readonly TextBox _temperature;
    private readonly TextBox _productivityIndex;
    private readonly TextBox _waterCut;
    private readonly TextBox _gor;
    private readonly TextBox _oilGravity;
    private readonly TextBox _fvf;
    private readonly TextBox _viscosity;
    private readonly TextBox _depth;
    private readonly TextBox _tubing;
    private readonly TextBox _wellheadPressure;
    private readonly TextBox _gasGravity;
    private readonly TextBox _roughness;
    private readonly ComboBox _iprModel;
    private readonly ComboBox _vlpCorrelation;
    private readonly TextBox _points;
    private readonly TextBox _sweepFrom;
    private readonly TextBox _sweepTo;
    private readonly TextBox _sweepSteps;
    private readonly DataGrid _sweepGrid;
    private readonly DataGrid _history;
    private readonly Button _runButton;
    private readonly Button _saveButton;
    private readonly Button _sweepButton;

    private OgNodalWorkspace? _workspace;
    private OgNodalStudy? _study;
    private OgNodalSensitivity? _sensitivity;

    public NodalPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _nodal = application.Nodal;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _binding = Body(string.Empty);
        _binding.TextWrapping = TextWrapping.Wrap;
        _binding.Margin = new Thickness(0, 0, 0, 4);

        _diagnostics = Body(string.Empty);
        _diagnostics.TextWrapping = TextWrapping.Wrap;
        _diagnostics.Margin = new Thickness(0, 0, 0, 4);

        _crossCheck = Body(string.Empty);
        _crossCheck.TextWrapping = TextWrapping.Wrap;
        _crossCheck.Margin = new Thickness(0, 0, 0, 4);

        _curves = Body(string.Empty);
        _curves.TextWrapping = TextWrapping.Wrap;
        _curves.Margin = new Thickness(0, 4, 0, 8);

        _sensitivityState = Body(string.Empty);
        _sensitivityState.TextWrapping = TextWrapping.Wrap;
        _sensitivityState.Margin = new Thickness(0, 4, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _reservoirPressure = Field("Reservoir pressure psia", 130);
        _bubblePoint = Field("Bubble point psia", 120);
        _temperature = Field("Temperature degF", 110);
        _productivityIndex = Field("Productivity index STB/d/psi", 150);
        _waterCut = Field("Water cut 0-1", 100);
        _gor = Field("GOR scf/STB", 100);
        _oilGravity = Field("Oil gravity API", 110);
        _fvf = Field("Formation volume factor", 140);
        _viscosity = Field("Oil viscosity cp", 110);
        _depth = Field("Depth ft", 100);
        _tubing = Field("Tubing diameter in", 120);
        _wellheadPressure = Field("Wellhead pressure psia", 140);
        _gasGravity = Field("Gas specific gravity", 130);
        _roughness = Field("Tubing roughness ft", 130);
        _points = Field("Curve points", 90);
        _points.Text = "20";
        _sweepFrom = Field("Sweep from in", 100);
        _sweepFrom.Text = "1.995";
        _sweepTo = Field("Sweep to in", 100);
        _sweepTo.Text = "4.5";
        _sweepSteps = Field("Sweep steps", 90);
        _sweepSteps.Text = "6";

        _iprModel = Picker("Inflow model", 200);
        _vlpCorrelation = Picker("Outflow correlation", 180);

        _sweepGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Tubing (in)", "DiameterText", 100),
                Text("Operating point", "ResultText", 220),
            },
        };

        Named(_sweepGrid, "Tubing sweep results");

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
                Text("Well", "WellId", 120),
                Text("Inflow", "IprText", 200),
                Text("Outflow", "VlpText", 120),
                Text("Status", "StatusText", 120),
                Text("Operating point", "OperatingText", 220),
                Text("Saved by", "Actor", 110),
            },
        };

        Named(_history, "Saved nodal studies");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _runButton = Button("Run study", (_, _) => _ = RunAsync());
        _saveButton = Button("Save study", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _sweepButton = Button("Sweep tubing", (_, _) => _ = SweepAsync());

        var reservoir = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        reservoir.Children.Add(Caption("Well"));
        reservoir.Children.Add(_well);
        reservoir.Children.Add(Caption("Pr"));
        reservoir.Children.Add(_reservoirPressure);
        reservoir.Children.Add(Caption("Pb"));
        reservoir.Children.Add(_bubblePoint);
        reservoir.Children.Add(Caption("T"));
        reservoir.Children.Add(_temperature);
        reservoir.Children.Add(Caption("PI"));
        reservoir.Children.Add(_productivityIndex);

        var fluids = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fluids.Children.Add(Caption("Water cut"));
        fluids.Children.Add(_waterCut);
        fluids.Children.Add(Caption("GOR"));
        fluids.Children.Add(_gor);
        fluids.Children.Add(Caption("API"));
        fluids.Children.Add(_oilGravity);
        fluids.Children.Add(Caption("FVF"));
        fluids.Children.Add(_fvf);
        fluids.Children.Add(Caption("Viscosity"));
        fluids.Children.Add(_viscosity);
        fluids.Children.Add(Caption("Gas SG"));
        fluids.Children.Add(_gasGravity);

        var wellbore = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellbore.Children.Add(Caption("Depth"));
        wellbore.Children.Add(_depth);
        wellbore.Children.Add(Caption("Tubing"));
        wellbore.Children.Add(_tubing);
        wellbore.Children.Add(Caption("Roughness"));
        wellbore.Children.Add(_roughness);
        wellbore.Children.Add(Caption("Wellhead"));
        wellbore.Children.Add(_wellheadPressure);
        wellbore.Children.Add(Caption("Points"));
        wellbore.Children.Add(_points);

        var models = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        models.Children.Add(Caption("Inflow"));
        models.Children.Add(_iprModel);
        models.Children.Add(Caption("Outflow"));
        models.Children.Add(_vlpCorrelation);
        models.Children.Add(_runButton);
        models.Children.Add(_saveButton);

        var sweepBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        sweepBar.Children.Add(Caption("Sweep tubing from"));
        sweepBar.Children.Add(_sweepFrom);
        sweepBar.Children.Add(Caption("to"));
        sweepBar.Children.Add(_sweepTo);
        sweepBar.Children.Add(Caption("steps"));
        sweepBar.Children.Add(_sweepSteps);
        sweepBar.Children.Add(_sweepButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Reservoir"));
        panel.Children.Add(reservoir);
        panel.Children.Add(Caption("Fluids"));
        panel.Children.Add(fluids);
        panel.Children.Add(Caption("Wellbore"));
        panel.Children.Add(wellbore);
        panel.Children.Add(Caption("Models"));
        panel.Children.Add(models);
        panel.Children.Add(_missing);
        panel.Children.Add(Caption("Operating point"));
        panel.Children.Add(_summary);
        panel.Children.Add(_binding);
        panel.Children.Add(_diagnostics);
        panel.Children.Add(Caption("Cross-check"));
        panel.Children.Add(_crossCheck);
        panel.Children.Add(Caption("Curves"));
        panel.Children.Add(_curves);
        panel.Children.Add(Caption("Tubing sweep"));
        panel.Children.Add(sweepBar);
        panel.Children.Add(_sensitivityState);
        panel.Children.Add(_sweepGrid);
        panel.Children.Add(Caption("Saved studies"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _reservoirPressure, _bubblePoint, _temperature, _productivityIndex, _waterCut, _gor,
                     _oilGravity, _fvf, _viscosity, _depth, _tubing, _wellheadPressure, _gasGravity, _roughness,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgNodalWorkspace? Workspace => _workspace;

    public OgNodalStudy? Study => _study;

    public OgNodalSensitivity? Sensitivity => _sensitivity;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string SummaryText => _summary.Text;

    public string BindingText => _binding.Text;

    public string DiagnosticsText => _diagnostics.Text;

    public string CrossCheckText => _crossCheck.Text;

    public string CurvesText => _curves.Text;

    public string SensitivityStateText => _sensitivityState.Text;

    public string DetailText => _detail.Text;

    public bool CanRun => NodalRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _study is { IsSaved: false, IprCurve.Count: > 0 };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _nodal
            .LoadAsync(_application.Session.FieldId is null ? null : Inputs().WellId)
            .GetAwaiter()
            .GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Nodal analysis";
            _notice.Text = loaded.Message ?? "The saved nodal studies could not be read.";
            ShowState(OgViewState.Failed, "The saved nodal studies could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> RunAsync()
    {
        var inputs = Inputs();

        if (NodalRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var run = await _nodal.RunAsync(inputs).ConfigureAwait(true);

        if (!run.Success || run.Value is null)
        {
            LastFailed = true;
            _study = null;
            _summary.Text = string.Empty;
            _binding.Text = string.Empty;
            _diagnostics.Text = string.Empty;
            _crossCheck.Text = string.Empty;
            _curves.Text = string.Empty;
            _notice.Text = run.Message ?? "The nodal study could not be run.";
            ShowState(OgViewState.Failed, "The nodal study could not be run.", run.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(run.Value);

        // AC-2: no intersection is a state of the study, not a failure of the run.
        LastFailed = _study is { Converged: false };
        ShowState(_study is { Converged: false } ? OgViewState.Empty : OgViewState.Ready, _notice.Text);

        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_study is not { } study || study.IprCurve.Count == 0)
        {
            _notice.Text = "Run the study before saving it.";
            return false;
        }

        var saved = await _nodal.SaveAsync(study, _application.Session.UserId).ConfigureAwait(true);

        if (!saved.Success || saved.Value is null)
        {
            LastFailed = true;
            _notice.Text = saved.Message ?? "The study was not saved.";
            ShowState(OgViewState.Failed, "The study was not saved.", saved.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(saved.Value);

        _application.AuditLog.Add(
            "lift.nodal.save",
            "lift",
            study.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with both correlations, the inputs and the curves. " +
                       (saved.Value.HistoryStateText);

        RefreshEnabled();
        return true;
    }

    public async Task<bool> SweepAsync()
    {
        var inputs = Inputs();

        if (NodalRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var swept = await _nodal
            .SensitivityAsync(
                inputs,
                Parse(_sweepFrom.Text) ?? 1.995m,
                Parse(_sweepTo.Text) ?? 4.5m,
                (int)(Parse(_sweepSteps.Text) ?? 6m))
            .ConfigureAwait(true);

        if (!swept.Success || swept.Value is null)
        {
            _sensitivity = null;
            _sweepGrid.ItemsSource = null;
            _sensitivityState.Text = string.Empty;
            _notice.Text = swept.Message ?? "The sweep could not be run.";
            return false;
        }

        _sensitivity = swept.Value;
        _sweepGrid.ItemsSource = swept.Value.Points.ToList();
        _sensitivityState.Text = swept.Value.StateText + " · " + swept.Value.BestText;
        _notice.Text = "Swept the tubing diameter over the range; each point is a full study at that diameter.";

        return true;
    }

    private void Apply(OgNodalWorkspace workspace)
    {
        _workspace = workspace;

        _iprModel.ItemsSource = workspace.IprModels.ToList();
        _iprModel.SelectedIndex = _iprModel.SelectedIndex < 0 ? 0 : _iprModel.SelectedIndex;

        _vlpCorrelation.ItemsSource = workspace.VlpCorrelations.ToList();
        _vlpCorrelation.SelectedIndex = _vlpCorrelation.SelectedIndex < 0 ? 0 : _vlpCorrelation.SelectedIndex;

        _history.ItemsSource = workspace.Studies.ToList();

        _header.Text = "Nodal analysis · inflow against outflow";
        _notice.Text = workspace.HistoryFailed
            ? "The saved studies are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowStudy(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowStudy(OgNodalStudy? study)
    {
        _study = study;

        if (study is null)
        {
            _summary.Text = "no study is selected";
            _binding.Text = string.Empty;
            _diagnostics.Text = string.Empty;
            _crossCheck.Text = string.Empty;
            _curves.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text =
            $"{(study.HasOperatingPoint ? "operating point" : "NO INTERSECTION")} · {study.IprText} · " +
            $"outflow {study.VlpText} · {study.OperatingText}";
        _binding.Text = "Binding: " + study.BindingText;
        _diagnostics.Text = study.HasOperatingPoint
            ? study.BindingText
            : study.Diagnostics;
        _crossCheck.Text = study.CrossCheckText;
        _curves.Text = study.CurveText;
        _detail.Text = study.PointsText;

        if (!study.HasOperatingPoint && _workspace is { } workspace)
        {
            _diagnostics.Text = study.Diagnostics + Environment.NewLine + workspace.LiftRankingText;
        }
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgNodalStudy selected)
        {
            return;
        }

        ShowStudy(selected);
    }

    private void RefreshEnabled()
    {
        var missing = NodalRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The study needs " + string.Join(", ", missing) + ".";

        _runButton.IsEnabled = missing.Count == 0;
        _sweepButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave && _study is { IprCurve.Count: > 0 };
    }

    private OgNodalInputs Inputs() =>
        new(
            Empty(_well.Text),
            Parse(_reservoirPressure.Text),
            Parse(_bubblePoint.Text),
            Parse(_temperature.Text),
            Parse(_productivityIndex.Text),
            Parse(_waterCut.Text),
            Parse(_gor.Text),
            Parse(_oilGravity.Text),
            Parse(_fvf.Text),
            Parse(_viscosity.Text),
            Parse(_depth.Text),
            Parse(_tubing.Text),
            Parse(_wellheadPressure.Text),
            Parse(_gasGravity.Text),
            Parse(_roughness.Text),
            _iprModel.SelectedItem as string,
            _vlpCorrelation.SelectedItem as string,
            (int)(Parse(_points.Text) ?? 20m));

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
