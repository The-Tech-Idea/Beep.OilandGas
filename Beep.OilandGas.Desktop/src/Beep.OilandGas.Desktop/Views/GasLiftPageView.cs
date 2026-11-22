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
/// LFT-02: gas lift design - the potential recommendation, the placed valves and every violated design limit, with the
/// owning project's own message.
/// </summary>
/// <remarks>
/// The limits are the GasLift module's (`GasLiftDesignLimits` over `GasLiftDesignLimitMessages`); this view prints them and
/// never writes one of its own (AC-1). A blocking limit refuses the save with its catalog message (AC-2). Port sizes come
/// from the reference set (AC-4), and injection and production units are stated (AC-5).
/// </remarks>
public sealed class GasLiftPageView : OgPageBase
{
    public const string Route = "gaslift";

    private readonly OgApplication _application;
    private readonly IOgGasLift _gasLift;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _summary;
    private readonly TextBlock _units;
    private readonly TextBlock _recommendation;
    private readonly TextBlock _limits;
    private readonly TextBlock _evaluated;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly TextBox _depth;
    private readonly TextBox _wellheadPressure;
    private readonly TextBox _bottomHolePressure;
    private readonly TextBox _reservoirPressure;
    private readonly TextBox _gor;
    private readonly TextBox _desiredRate;
    private readonly TextBox _oilGravity;
    private readonly TextBox _waterCut;
    private readonly TextBox _wellheadTemperature;
    private readonly TextBox _bottomHoleTemperature;
    private readonly TextBox _gasGravity;
    private readonly TextBox _injectionPressure;
    private readonly TextBox _injectionRate;
    private readonly TextBox _valves;
    private readonly ComboBox _portSize;
    private readonly ComboBox _spacingMethod;
    private readonly DataGrid _valveGrid;
    private readonly DataGrid _history;
    private readonly Button _designButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgGasLiftWorkspace? _workspace;
    private OgGasLiftDesign? _design;

    public GasLiftPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _gasLift = application.GasLift;

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

        _units = Body(string.Empty);
        _units.TextWrapping = TextWrapping.Wrap;
        _units.Margin = new Thickness(0, 0, 0, 4);

        _recommendation = Body(string.Empty);
        _recommendation.TextWrapping = TextWrapping.Wrap;
        _recommendation.Margin = new Thickness(0, 0, 0, 4);

        _limits = Body(string.Empty);
        _limits.TextWrapping = TextWrapping.Wrap;
        _limits.Margin = new Thickness(0, 4, 0, 4);

        _evaluated = Body(string.Empty);
        _evaluated.TextWrapping = TextWrapping.Wrap;
        _evaluated.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _depth = Field("Well depth ft", 110);
        _wellheadPressure = Field("Wellhead pressure psia", 130);
        _bottomHolePressure = Field("Bottom hole pressure psia", 140);
        _reservoirPressure = Field("Reservoir pressure psia", 130);
        _gor = Field("Gas-oil ratio scf/STB", 130);
        _desiredRate = Field("Desired rate STB/d", 120);
        _oilGravity = Field("Oil gravity API", 110);
        _waterCut = Field("Water cut 0-1", 100);
        _wellheadTemperature = Field("Wellhead temperature degF", 150);
        _bottomHoleTemperature = Field("Bottom hole temperature degF", 150);
        _gasGravity = Field("Gas specific gravity", 130);
        _injectionPressure = Field("Injection pressure psia", 140);
        _injectionRate = Field("Injection rate Mscf/d", 130);
        _valves = Field("Number of valves", 110);
        _valves.Text = "5";

        _portSize = Picker("Valve port size", 110);
        _spacingMethod = Picker("Spacing method", 200);

        _valveGrid = new DataGrid
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
                Text("Valve", "Index", 60),
                Text("Service", "ServiceText", 100),
                Text("Depth (ft)", "DepthText", 100),
                Text("Opens (psia)", "OpeningText", 110),
                Text("Closes (psia)", "ClosingText", 110),
            },
        };

        Named(_valveGrid, "Placed valves");

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
                Text("Method", "MethodText", 150),
                Text("Valves", "ValveCount", 70),
                Text("Injection", "InjectionText", 110),
                Text("Expected", "ExpectedText", 110),
                Text("Port", "PortText", 80),
                Text("Saved by", "Actor", 110),
            },
        };

        Named(_history, "Saved gas lift designs");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _designButton = Button("Design", (_, _) => _ = DesignAsync());
        _saveButton = Button("Save design", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Depth"));
        wellBar.Children.Add(_depth);
        wellBar.Children.Add(Caption("Pwh"));
        wellBar.Children.Add(_wellheadPressure);
        wellBar.Children.Add(Caption("Pwf"));
        wellBar.Children.Add(_bottomHolePressure);
        wellBar.Children.Add(Caption("Pr"));
        wellBar.Children.Add(_reservoirPressure);

        var fluidBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fluidBar.Children.Add(Caption("GOR"));
        fluidBar.Children.Add(_gor);
        fluidBar.Children.Add(Caption("Desired"));
        fluidBar.Children.Add(_desiredRate);
        fluidBar.Children.Add(Caption("API"));
        fluidBar.Children.Add(_oilGravity);
        fluidBar.Children.Add(Caption("Water cut"));
        fluidBar.Children.Add(_waterCut);
        fluidBar.Children.Add(Caption("Gas SG"));
        fluidBar.Children.Add(_gasGravity);

        var temperatureBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        temperatureBar.Children.Add(Caption("Twh"));
        temperatureBar.Children.Add(_wellheadTemperature);
        temperatureBar.Children.Add(Caption("Twf"));
        temperatureBar.Children.Add(_bottomHoleTemperature);

        var designBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        designBar.Children.Add(Caption("Injection pressure"));
        designBar.Children.Add(_injectionPressure);
        designBar.Children.Add(Caption("Injection rate"));
        designBar.Children.Add(_injectionRate);
        designBar.Children.Add(Caption("Valves"));
        designBar.Children.Add(_valves);
        designBar.Children.Add(_portSize);
        designBar.Children.Add(_spacingMethod);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_designButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(Caption("Well"));
        panel.Children.Add(wellBar);
        panel.Children.Add(Caption("Fluids"));
        panel.Children.Add(fluidBar);
        panel.Children.Add(Caption("Temperature"));
        panel.Children.Add(temperatureBar);
        panel.Children.Add(Caption("Design"));
        panel.Children.Add(designBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Design result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_units);
        panel.Children.Add(_recommendation);
        panel.Children.Add(Caption("Design limits"));
        panel.Children.Add(_limits);
        panel.Children.Add(_evaluated);
        panel.Children.Add(Caption("Valves"));
        panel.Children.Add(_valveGrid);
        panel.Children.Add(Caption("Saved designs"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _depth, _wellheadPressure, _bottomHolePressure, _reservoirPressure, _gor, _desiredRate,
                     _oilGravity, _waterCut, _wellheadTemperature, _bottomHoleTemperature, _gasGravity,
                     _injectionPressure, _injectionRate, _valves,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgGasLiftWorkspace? Workspace => _workspace;

    public OgGasLiftDesign? Design => _design;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string SummaryText => _summary.Text;

    public string UnitsText => _units.Text;

    public string RecommendationText => _recommendation.Text;

    public string LimitsText => _limits.Text;

    public string EvaluatedText => _evaluated.Text;

    public string DetailText => _detail.Text;

    public bool CanDesign => GasLiftRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _design is { IsSaved: false, Valves.Count: > 0, HasBlockingLimit: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool PortSizesFromReference => _workspace?.PortSizesFromReference ?? false;

    public bool Load()
    {
        var loaded = _gasLift.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Gas lift";
            _notice.Text = loaded.Message ?? "The saved gas lift designs could not be read.";
            ShowState(OgViewState.Failed, "The saved gas lift designs could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> DesignAsync()
    {
        var inputs = Inputs();

        if (GasLiftRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var designed = await _gasLift.DesignAsync(inputs).ConfigureAwait(true);

        if (!designed.Success || designed.Value is null)
        {
            LastFailed = true;
            _design = null;
            _summary.Text = string.Empty;
            _units.Text = string.Empty;
            _recommendation.Text = string.Empty;
            _limits.Text = string.Empty;
            _evaluated.Text = string.Empty;
            _valveGrid.ItemsSource = null;
            _notice.Text = designed.Message ?? "The design could not be run.";
            ShowState(OgViewState.Failed, "The design could not be run.", designed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(designed.Value);
        LastFailed = _design is { HasBlockingLimit: true };
        ShowState(_design is { HasBlockingLimit: true } ? OgViewState.Failed : OgViewState.Ready, _notice.Text);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_design is not { } design || !CanSave)
        {
            _notice.Text = _design is { HasBlockingLimit: true } blocked
                ? "A blocking design limit prevents the save: " +
                  string.Join(" ", blocked.BlockingLimits.Select(limit => limit.Message))
                : "Design the installation before saving it.";
            return false;
        }

        var saved = await _gasLift.SaveAsync(design, _application.Session.UserId).ConfigureAwait(true);

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
            "lift.gaslift.save",
            "lift",
            design.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with the method, the valve list and every evaluated limit. " +
                       (saved.Value.HistoryStateText);

        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _gasLift.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

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

    private void Apply(OgGasLiftWorkspace workspace)
    {
        _workspace = workspace;

        _spacingMethod.ItemsSource = workspace.SpacingMethods.ToList();
        _spacingMethod.SelectedIndex = _spacingMethod.SelectedIndex < 0 ? 0 : _spacingMethod.SelectedIndex;

        _portSize.ItemsSource = workspace.PortSizes.ToList();
        _portSize.SelectedIndex = _portSize.SelectedIndex < 0 && workspace.PortSizes.Count > 0
            ? 0
            : _portSize.SelectedIndex;

        _history.ItemsSource = workspace.Designs.ToList();

        _header.Text = "Gas lift · potential, valves and design limits";
        _notice.Text = workspace.HistoryFailed
            ? "The saved designs are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        _evaluated.Text = workspace.PortSizesText;

        ShowDesign(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowDesign(OgGasLiftDesign? design)
    {
        _design = design;

        if (design is null)
        {
            _summary.Text = "no design is selected";
            _units.Text = string.Empty;
            _recommendation.Text = string.Empty;
            _limits.Text = string.Empty;
            _valveGrid.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = design.Describe();
        _units.Text =
            $"injection in {design.InjectionUnit}, production in {design.ProductionUnit} · " +
            $"{design.MethodText} · port {design.PortText} · efficiency {design.EfficiencyText}";
        _recommendation.Text = design.RecommendationText;
        _limits.Text = design.LimitsText;
        _valveGrid.ItemsSource = design.Valves.ToList();
        _detail.Text = design.ValvesText + Environment.NewLine + design.EvaluatedText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgGasLiftDesign selected)
        {
            return;
        }

        ShowDesign(selected);
    }

    private void RefreshEnabled()
    {
        var missing = GasLiftRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The design needs " + string.Join(", ", missing) + ".";

        _designButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _gasLift.Pending is not null;
    }

    private OgGasLiftInputs Inputs() =>
        new(
            Empty(_well.Text),
            Parse(_depth.Text),
            Parse(_wellheadPressure.Text),
            Parse(_bottomHolePressure.Text),
            Parse(_reservoirPressure.Text),
            Parse(_gor.Text),
            Parse(_desiredRate.Text),
            Parse(_oilGravity.Text),
            Parse(_waterCut.Text),
            Parse(_wellheadTemperature.Text),
            Parse(_bottomHoleTemperature.Text),
            Parse(_gasGravity.Text),
            Parse(_injectionPressure.Text),
            Parse(_injectionRate.Text),
            (int)(Parse(_valves.Text) ?? 0m),
            _portSize.SelectedItem is decimal port ? port : null,
            _spacingMethod.SelectedItem as string);

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
