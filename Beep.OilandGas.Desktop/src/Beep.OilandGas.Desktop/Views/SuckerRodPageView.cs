using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// LFT-04: sucker rod pumping - a rod string designed against the module's own loads and fatigue analysis, with the
/// safety factor, the limiting section and the pump card.
/// </summary>
/// <remarks>
/// An unsafe string is refused and the limiting section is named with its stress and Goodman ratio (AC-1, SG-07-5). The
/// grades, diameters, stroke lengths and strokes per minute are the owning constants (AC-2), never literals here (AC-2).
/// The card is the owner's and its extremes are the computed peak and minimum loads (AC-3). Lengths and diameters carry
/// their units (AC-5), and a saved design records the grade, the taper and the safety factor (AC-6).
/// </remarks>
public sealed class SuckerRodPageView : OgPageBase
{
    public const string Route = "suckerrod";

    private readonly OgApplication _application;
    private readonly IOgSuckerRod _suckerRod;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _reference;
    private readonly TextBlock _summary;
    private readonly TextBlock _fatigue;
    private readonly TextBlock _card;
    private readonly TextBlock _density;
    private readonly TextBlock _units;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _pumpDiameter;
    private readonly ComboBox _strokeLength;
    private readonly ComboBox _strokesPerMinute;
    private readonly TextBox _pumpDepth;
    private readonly TextBox _fluidLevel;
    private readonly TextBox _tubing;
    private readonly TextBox _oilGravity;
    private readonly TextBox _waterCut;
    private readonly TextBox _gor;
    private readonly TextBox _gasGravity;
    private readonly TextBox _rodDensity;
    private readonly CheckBox _acceptDensity;
    private readonly ComboBox _grade;
    private readonly TextBox _taper;
    private readonly DataGrid _sections;
    private readonly DataGrid _history;
    private readonly Button _designButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgSrpWorkspace? _workspace;
    private OgSrpResult? _result;

    public SuckerRodPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _suckerRod = application.SuckerRod;

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

        _fatigue = Body(string.Empty);
        _fatigue.TextWrapping = TextWrapping.Wrap;
        _fatigue.Margin = new Thickness(0, 0, 0, 4);

        _card = Body(string.Empty);
        _card.TextWrapping = TextWrapping.Wrap;
        _card.Margin = new Thickness(0, 0, 0, 4);

        _density = Body(string.Empty);
        _density.TextWrapping = TextWrapping.Wrap;
        _density.Margin = new Thickness(0, 0, 0, 4);

        _units = Body(string.Empty);
        _units.TextWrapping = TextWrapping.Wrap;
        _units.Margin = new Thickness(0, 0, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _pumpDepth = Field("Pump depth ft", 110);
        _fluidLevel = Field("Fluid level ft", 110);
        _tubing = Field("Tubing diameter in", 130);
        _tubing.Text = "2.875";
        _oilGravity = Field("Oil gravity API", 110);
        _oilGravity.Text = "35";
        _waterCut = Field("Water cut 0-1", 100);
        _waterCut.Text = "0.3";
        _gor = Field("Gas-oil ratio scf/STB", 130);
        _gor.Text = "300";
        _gasGravity = Field("Gas specific gravity", 130);
        _gasGravity.Text = "0.65";
        _rodDensity = Field("Rod density lb/ft3", 120);

        _acceptDensity = new CheckBox
        {
            Content = "Accept the default steel density",
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };

        Named(_acceptDensity, "Accept the default steel density");

        _pumpDiameter = Picker("Pump diameter", 110);
        _strokeLength = Picker("Stroke length", 110);
        _strokesPerMinute = Picker("Strokes per minute", 130);
        _grade = Picker("Rod grade", 180);

        _taper = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 70,
            MaxHeight = 110,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "diameter in, length ft - one section per line, top first",
        };

        Named(_taper, "Rod taper, one diameter and length per line");
        _taper.Text =
            "# diameter in, length ft - top section first" + Environment.NewLine +
            "1.0, 2500" + Environment.NewLine +
            "0.875, 1500" + Environment.NewLine +
            "0.75, 1000";

        _sections = new DataGrid
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
                Text("Section", "Order", 70),
                Text("Diameter", "DiameterText", 90),
                Text("Length", "LengthText", 90),
                Text("Max stress", "StressText", 110),
                Text("Safety factor", "SafetyText", 100),
                Text("Goodman", "GoodmanText", 90),
            },
        };

        Named(_sections, "Rod sections and their fatigue");

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
                Text("Grade", "Grade", 70),
                Text("Sections", "Sections.Count", 80),
                Text("Peak", "PeakText", 100),
                Text("Max stress", "MaxStressText", 110),
                Text("Safety factor", "MinSafetyFactor", 110),
                Text("Rate", "RateText", 100),
            },
        };

        Named(_history, "Saved sucker rod designs");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _designButton = Button("Design", (_, _) => _ = DesignAsync());
        _saveButton = Button("Save design", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(Caption("Pump depth"));
        wellBar.Children.Add(_pumpDepth);
        wellBar.Children.Add(Caption("Fluid level"));
        wellBar.Children.Add(_fluidLevel);
        wellBar.Children.Add(Caption("Tubing"));
        wellBar.Children.Add(_tubing);

        var pumpBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pumpBar.Children.Add(Caption("Pump"));
        pumpBar.Children.Add(_pumpDiameter);
        pumpBar.Children.Add(Caption("Stroke"));
        pumpBar.Children.Add(_strokeLength);
        pumpBar.Children.Add(Caption("SPM"));
        pumpBar.Children.Add(_strokesPerMinute);
        pumpBar.Children.Add(_grade);

        var fluidBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        fluidBar.Children.Add(Caption("API"));
        fluidBar.Children.Add(_oilGravity);
        fluidBar.Children.Add(Caption("Water cut"));
        fluidBar.Children.Add(_waterCut);
        fluidBar.Children.Add(Caption("GOR"));
        fluidBar.Children.Add(_gor);
        fluidBar.Children.Add(Caption("Gas SG"));
        fluidBar.Children.Add(_gasGravity);

        var densityBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        densityBar.Children.Add(Caption("Rod density"));
        densityBar.Children.Add(_rodDensity);
        densityBar.Children.Add(_acceptDensity);

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
        panel.Children.Add(Caption("Pump and stroke"));
        panel.Children.Add(pumpBar);
        panel.Children.Add(Caption("Fluids"));
        panel.Children.Add(fluidBar);
        panel.Children.Add(Caption("Rod density"));
        panel.Children.Add(densityBar);
        panel.Children.Add(Caption("Rod string"));
        panel.Children.Add(_taper);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Result"));
        panel.Children.Add(_summary);
        panel.Children.Add(_fatigue);
        panel.Children.Add(_card);
        panel.Children.Add(_density);
        panel.Children.Add(_units);
        panel.Children.Add(Caption("Sections"));
        panel.Children.Add(_sections);
        panel.Children.Add(Caption("Saved designs"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _pumpDepth, _fluidLevel, _tubing, _oilGravity, _waterCut, _gor, _gasGravity,
                     _rodDensity, _taper,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        _acceptDensity.Checked += (_, _) => RefreshEnabled();
        _acceptDensity.Unchecked += (_, _) => RefreshEnabled();

        Load();
    }

    public OgSrpWorkspace? Workspace => _workspace;

    public OgSrpResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string ReferenceText => _reference.Text;

    public string SummaryText => _summary.Text;

    public string FatigueText => _fatigue.Text;

    public string CardText => _card.Text;

    public string DensityText => _density.Text;

    public string UnitsText => _units.Text;

    public string DetailText => _detail.Text;

    public bool CanDesign => SrpRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, Sections.Count: > 0, IsUnsafe: false };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _suckerRod.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Sucker rod";
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

        if (SrpRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var designed = await _suckerRod.DesignAsync(inputs).ConfigureAwait(true);

        if (!designed.Success || designed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _fatigue.Text = string.Empty;
            _card.Text = string.Empty;
            _density.Text = string.Empty;
            _units.Text = string.Empty;
            _sections.ItemsSource = null;

            // An unsafe string is the state table's own case; the message names the limiting section.
            _notice.Text = designed.Message ?? "The design could not be run.";
            ShowState(OgViewState.Failed, "The design could not be run.", designed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(designed.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = _result is { IsUnsafe: true } unsafeResult
                ? "An unsafe string cannot be saved: its safety factor is " +
                  unsafeResult.MinSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture) +
                  " against the required " +
                  unsafeResult.RequiredSafetyFactor.ToString("0.###", CultureInfo.InvariantCulture) + "."
                : "Design the string before saving it.";
            return false;
        }

        var saved = await _suckerRod.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

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
            "lift.srp.save",
            "lift",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with the grade, the taper and the safety factor achieved. " +
                       saved.Value.HistoryStateText;
        RefreshEnabled();
        return true;
    }

    public async Task<bool> RetryAsync()
    {
        var retried = await _suckerRod.RetrySaveAsync(_application.Session.UserId).ConfigureAwait(true);

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

    private void Apply(OgSrpWorkspace workspace)
    {
        _workspace = workspace;

        _grade.ItemsSource = workspace.Grades.ToList();
        _grade.SelectedIndex = _grade.SelectedIndex < 0 ? 1 : _grade.SelectedIndex;

        _pumpDiameter.ItemsSource = workspace.StandardPumpDiameters.ToList();
        _pumpDiameter.SelectedIndex = _pumpDiameter.SelectedIndex < 0 ? 3 : _pumpDiameter.SelectedIndex;

        _strokeLength.ItemsSource = workspace.StandardStrokeLengths.ToList();
        _strokeLength.SelectedIndex = _strokeLength.SelectedIndex < 0 ? 8 : _strokeLength.SelectedIndex;

        _strokesPerMinute.ItemsSource = workspace.StandardStrokesPerMinute.ToList();
        _strokesPerMinute.SelectedIndex = _strokesPerMinute.SelectedIndex < 0 ? 3 : _strokesPerMinute.SelectedIndex;

        _history.ItemsSource = workspace.Designs.ToList();

        _header.Text = "Sucker rod · rod string, load and fatigue";
        _reference.Text = workspace.ReferenceText;
        _notice.Text = workspace.HistoryFailed
            ? "The saved designs are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgSrpResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no design is selected";
            _fatigue.Text = string.Empty;
            _card.Text = string.Empty;
            _density.Text = string.Empty;
            _units.Text = string.Empty;
            _sections.ItemsSource = null;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = result.Describe();
        _fatigue.Text =
            $"peak {result.PeakText} · minimum {result.MinLoadText} · fluid load {result.FluidLoadText} · " +
            $"max stress {result.MaxStressText} · safety factor {result.SafetyText} · risk {result.Risk.ToLowerInvariant()}";
        _card.Text = "pump card: " + result.CardText;
        _density.Text = result.DensityText;
        _units.Text = "Units: " + result.UnitsText + " · grade: " + result.GradeText;
        _sections.ItemsSource = result.SectionResults.ToList();
        _detail.Text = result.SectionsText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgSrpResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = SrpRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The design needs " + string.Join(", ", missing) + ".";

        _designButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _suckerRod.Pending is not null;
    }

    private OgSrpInputs Inputs() =>
        new(
            Empty(_well.Text),
            _pumpDiameter.SelectedItem is decimal pump ? pump : null,
            _strokeLength.SelectedItem is decimal stroke ? stroke : null,
            _strokesPerMinute.SelectedItem is decimal spm ? spm : null,
            Parse(_pumpDepth.Text),
            Parse(_fluidLevel.Text),
            Parse(_tubing.Text),
            Parse(_oilGravity.Text),
            Parse(_waterCut.Text),
            Parse(_gor.Text),
            Parse(_gasGravity.Text),
            Parse(_rodDensity.Text),
            _acceptDensity.IsChecked == true,
            _grade.SelectedItem as string,
            Sections());

    /// <summary>Reads the taper editor: one <c>diameter, length</c> pair per line, top section first.</summary>
    private IReadOnlyList<OgRodSection> Sections()
    {
        var sections = new List<OgRodSection>();

        foreach (var line in (_taper.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 2
                || !decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var diameter)
                || !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var length))
            {
                continue;
            }

            sections.Add(new OgRodSection(diameter, length));
        }

        return sections;
    }

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
