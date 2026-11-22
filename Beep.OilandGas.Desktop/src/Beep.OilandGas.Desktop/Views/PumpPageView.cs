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
/// LFT-03: ESP and pump performance - a duty point checked against the pump's published H-Q curve, with the NPSH margin
/// from the module's own minimum, an affinity result that says when it is extrapolated, and a unit on every number.
/// </summary>
/// <remarks>
/// A duty rate outside the published curve is refused with the range the curve covers (AC-1, SG-07-4). The NPSH minimum is
/// <c>PumpConstants.DefaultNpshSafetyMarginFeet</c> (AC-2). An affinity result outside the curve is labelled extrapolated
/// (AC-3). The curves are the owner's calculations, not the view's (AC-4). Every result states its unit (AC-5), and the
/// saved design records the curve source and the stage count (AC-6).
/// </remarks>
public sealed class PumpPageView : OgPageBase
{
    public const string Route = "pumps";

    private readonly OgApplication _application;
    private readonly IOgPump _pump;

    private readonly TextBlock _header;
    private readonly TextBlock _notice;
    private readonly TextBlock _missing;
    private readonly TextBlock _curveNote;
    private readonly TextBlock _summary;
    private readonly TextBlock _stages;
    private readonly TextBlock _npsh;
    private readonly TextBlock _affinity;
    private readonly TextBlock _notes;
    private readonly TextBlock _curveText;
    private readonly TextBlock _detail;
    private readonly TextBox _well;
    private readonly ComboBox _pumpType;
    private readonly TextBox _curve;
    private readonly TextBox _stagesBox;
    private readonly TextBox _dutyRate;
    private readonly TextBox _specificGravity;
    private readonly TextBox _speed;
    private readonly TextBox _newSpeed;
    private readonly TextBox _atmospheric;
    private readonly TextBox _vapour;
    private readonly TextBox _suctionHead;
    private readonly TextBox _suctionFriction;
    private readonly DataGrid _history;
    private readonly Button _analyseButton;
    private readonly Button _saveButton;
    private readonly Button _retryButton;

    private OgPumpWorkspace? _workspace;
    private OgPumpResult? _result;

    public PumpPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _pump = application.Pumps;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _missing = Body(string.Empty);
        _missing.TextWrapping = TextWrapping.Wrap;
        _missing.Margin = new Thickness(0, 4, 0, 4);

        _curveNote = Body(string.Empty);
        _curveNote.TextWrapping = TextWrapping.Wrap;
        _curveNote.Margin = new Thickness(0, 0, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _stages = Body(string.Empty);
        _stages.TextWrapping = TextWrapping.Wrap;
        _stages.Margin = new Thickness(0, 0, 0, 4);

        _npsh = Body(string.Empty);
        _npsh.TextWrapping = TextWrapping.Wrap;
        _npsh.Margin = new Thickness(0, 0, 0, 4);

        _affinity = Body(string.Empty);
        _affinity.TextWrapping = TextWrapping.Wrap;
        _affinity.Margin = new Thickness(0, 0, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 0, 0, 8);

        _curveText = Body(string.Empty);
        _curveText.TextWrapping = TextWrapping.Wrap;
        _curveText.Margin = new Thickness(0, 4, 0, 8);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 6, 0, 8);

        _well = Field("Well id", 120);
        _stagesBox = Field("Stages", 70);
        _stagesBox.Text = "1";
        _dutyRate = Field("Duty rate GPM", 110);
        _specificGravity = Field("Specific gravity", 110);
        _specificGravity.Text = "1";
        _speed = Field("Pump speed rpm", 110);
        _speed.Text = "3500";
        _newSpeed = Field("Affinity speed rpm", 120);
        _atmospheric = Field("Atmospheric pressure psia", 150);
        _atmospheric.Text = "14.696";
        _vapour = Field("Vapour pressure psia", 130);
        _vapour.Text = "0.256";
        _suctionHead = Field("Suction head ft", 120);
        _suctionFriction = Field("Suction friction ft", 130);
        _suctionFriction.Text = "0";

        _pumpType = new ComboBox
        {
            MinWidth = 180,
            Margin = new Thickness(0, 0, 8, 0),
        };

        Named(_pumpType, "Pump type");

        _curve = new TextBox
        {
            AcceptsReturn = true,
            IsTabStop = true,
            MinHeight = 80,
            MaxHeight = 120,
            Margin = new Thickness(0, 0, 0, 6),
            TextWrapping = TextWrapping.NoWrap,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "rate GPM, head ft, power hp - one point per line",
        };

        Named(_curve, "Published H-Q curve, one point per line");
        _curve.Text =
            "# rate GPM, head ft, power hp" + Environment.NewLine +
            "0, 120, 20" + Environment.NewLine +
            "500, 110, 30" + Environment.NewLine +
            "1000, 90, 40" + Environment.NewLine +
            "1500, 60, 50";

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
                Text("Type", "TypeText", 160),
                Text("Rate", "RateText", 100),
                Text("Head", "HeadText", 90),
                Text("Total head", "TotalHeadText", 110),
                Text("Stages", "Stages", 70),
                Text("Efficiency", "EfficiencyText", 100),
                Text("NPSH", "NpshText", 320),
            },
        };

        Named(_history, "Saved duty points");
        _history.SelectionChanged += (_, _) => SelectSaved();

        _analyseButton = Button("Analyse", (_, _) => _ = AnalyseAsync());
        _saveButton = Button("Save design", (_, _) => _ = SaveAsync());
        _saveButton.IsEnabled = false;
        _retryButton = Button("Retry save", (_, _) => _ = RetryAsync());
        _retryButton.IsEnabled = false;

        var pumpBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        pumpBar.Children.Add(Caption("Well"));
        pumpBar.Children.Add(_well);
        pumpBar.Children.Add(Caption("Type"));
        pumpBar.Children.Add(_pumpType);
        pumpBar.Children.Add(Caption("Stages"));
        pumpBar.Children.Add(_stagesBox);
        pumpBar.Children.Add(Caption("Duty rate"));
        pumpBar.Children.Add(_dutyRate);
        pumpBar.Children.Add(Caption("SG"));
        pumpBar.Children.Add(_specificGravity);

        var speedBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        speedBar.Children.Add(Caption("Speed"));
        speedBar.Children.Add(_speed);
        speedBar.Children.Add(Caption("Affinity speed"));
        speedBar.Children.Add(_newSpeed);
        speedBar.Children.Add(Caption("Atmospheric"));
        speedBar.Children.Add(_atmospheric);
        speedBar.Children.Add(Caption("Vapour"));
        speedBar.Children.Add(_vapour);

        var suctionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        suctionBar.Children.Add(Caption("Suction head"));
        suctionBar.Children.Add(_suctionHead);
        suctionBar.Children.Add(Caption("Suction friction"));
        suctionBar.Children.Add(_suctionFriction);

        var actionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        actionBar.Children.Add(_analyseButton);
        actionBar.Children.Add(_saveButton);
        actionBar.Children.Add(_retryButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_notice);
        panel.Children.Add(_curveNote);
        panel.Children.Add(Caption("Published curve"));
        panel.Children.Add(_curve);
        panel.Children.Add(Caption("Pump"));
        panel.Children.Add(pumpBar);
        panel.Children.Add(Caption("Speed and suction"));
        panel.Children.Add(speedBar);
        panel.Children.Add(suctionBar);
        panel.Children.Add(_missing);
        panel.Children.Add(actionBar);
        panel.Children.Add(Caption("Duty point"));
        panel.Children.Add(_summary);
        panel.Children.Add(_stages);
        panel.Children.Add(_npsh);
        panel.Children.Add(_affinity);
        panel.Children.Add(_notes);
        panel.Children.Add(Caption("Curve"));
        panel.Children.Add(_curveText);
        panel.Children.Add(Caption("Saved duty points"));
        panel.Children.Add(_history);
        panel.Children.Add(_detail);

        ShowContent(panel);

        foreach (var box in new[]
                 {
                     _well, _curve, _stagesBox, _dutyRate, _specificGravity, _speed, _newSpeed, _atmospheric,
                     _vapour, _suctionHead, _suctionFriction,
                 })
        {
            box.TextChanged += (_, _) => RefreshEnabled();
        }

        Load();
    }

    public OgPumpWorkspace? Workspace => _workspace;

    public OgPumpResult? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string NoticeText => _notice.Text;

    public string MissingText => _missing.Text;

    public string CurveNoteText => _curveNote.Text;

    public string SummaryText => _summary.Text;

    public string StagesText => _stages.Text;

    public string NpshText => _npsh.Text;

    public string AffinityText => _affinity.Text;

    public string NotesText => _notes.Text;

    public string CurveText => _curveText.Text;

    public string DetailText => _detail.Text;

    public bool CanAnalyse => PumpRules.Missing(Inputs()).Count == 0;

    public bool CanSave => _result is { IsSaved: false, Curve.Count: > 0 };

    public bool HistoryUnavailable => _workspace?.HistoryFailed ?? false;

    public bool Load()
    {
        var loaded = _pump.LoadAsync().GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            _history.ItemsSource = null;
            _header.Text = "Pump performance";
            _notice.Text = loaded.Message ?? "The saved duty points could not be read.";
            ShowState(OgViewState.Failed, "The saved duty points could not be read.", loaded.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> AnalyseAsync()
    {
        var inputs = Inputs();

        if (PumpRules.Missing(inputs).Count > 0)
        {
            RefreshEnabled();
            _notice.Text = _missing.Text;
            return false;
        }

        var analysed = await _pump.AnalyseAsync(inputs).ConfigureAwait(true);

        if (!analysed.Success || analysed.Value is null)
        {
            LastFailed = true;
            _result = null;
            _summary.Text = string.Empty;
            _stages.Text = string.Empty;
            _npsh.Text = string.Empty;
            _affinity.Text = string.Empty;
            _notes.Text = string.Empty;
            _curveText.Text = string.Empty;

            // A refused duty point is the state table's own case; the message names the range the curve covers.
            _notice.Text = analysed.Message ?? "The duty point could not be checked.";
            ShowState(OgViewState.Failed, "The duty point could not be checked.", analysed.Reference);
            RefreshEnabled();
            return false;
        }

        Apply(analysed.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result || !CanSave)
        {
            _notice.Text = "Analyse a duty point before saving it.";
            return false;
        }

        var saved = await _pump.SaveAsync(result, _application.Session.UserId).ConfigureAwait(true);

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
            "lift.pump.save",
            "lift",
            result.WellId ?? "(no well)",
            "ok",
            _application.Session.UserId);

        _notice.Text = "Saved with the curve source and the stage count. " + saved.Value.HistoryStateText;
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

    private void Apply(OgPumpWorkspace workspace)
    {
        _workspace = workspace;

        _pumpType.ItemsSource = workspace.PumpTypes.ToList();
        _pumpType.SelectedIndex = _pumpType.SelectedIndex < 0 ? 0 : _pumpType.SelectedIndex;

        _history.ItemsSource = workspace.Designs.ToList();

        _header.Text = "Pump performance · duty point against the published curve";
        _curveNote.Text = workspace.CurveSourceNote;
        _notice.Text = workspace.HistoryFailed
            ? "The saved duty points are unavailable: " + workspace.HistoryReason + "."
            : workspace.HistoryStateText;

        ShowResult(workspace.Selected);
        RefreshEnabled();
    }

    private void ShowResult(OgPumpResult? result)
    {
        _result = result;

        if (result is null)
        {
            _summary.Text = "no duty point is selected";
            _stages.Text = string.Empty;
            _npsh.Text = string.Empty;
            _affinity.Text = string.Empty;
            _notes.Text = string.Empty;
            _curveText.Text = string.Empty;
            _detail.Text = string.Empty;
            return;
        }

        _summary.Text = result.Describe();
        _stages.Text = result.StageText;
        _npsh.Text = result.NpshText;
        _affinity.Text = result.AffinitySummary;
        _notes.Text = result.NotesText;
        _curveText.Text = result.CurveText;
        _detail.Text = "curve covers " + result.RangeText;
    }

    private void SelectSaved()
    {
        if (_history.SelectedItem is not OgPumpResult selected)
        {
            return;
        }

        ShowResult(selected);
    }

    private void RefreshEnabled()
    {
        var missing = PumpRules.Missing(Inputs());

        _missing.Text = missing.Count == 0
            ? string.Empty
            : "The analysis needs " + string.Join(", ", missing) + ".";

        _analyseButton.IsEnabled = missing.Count == 0;
        _saveButton.IsEnabled = CanSave;
        _retryButton.IsEnabled = _pump.Pending is not null;
    }

    private OgPumpInputs Inputs() =>
        new(
            Empty(_well.Text),
            _pumpType.SelectedItem as string,
            Curve(),
            (int)(Parse(_stagesBox.Text) ?? 0m),
            Parse(_dutyRate.Text),
            Parse(_specificGravity.Text),
            Parse(_speed.Text),
            Parse(_newSpeed.Text),
            Parse(_atmospheric.Text),
            Parse(_vapour.Text),
            Parse(_suctionHead.Text),
            Parse(_suctionFriction.Text));

    /// <summary>Reads the curve editor: one <c>rate, head, power</c> triple per line; <c>#</c> starts a comment.</summary>
    private IReadOnlyList<OgPumpCurvePoint> Curve()
    {
        var points = new List<OgPumpCurvePoint>();

        foreach (var line in (_curve.Text ?? string.Empty)
                     .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.StartsWith('#'))
            {
                continue;
            }

            var parts = line.Split(',', StringSplitOptions.TrimEntries);

            if (parts.Length < 2
                || !decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var rate)
                || !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var head))
            {
                continue;
            }

            decimal? power = parts.Length > 2
                             && decimal.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture,
                                 out var value)
                ? value
                : null;

            points.Add(new OgPumpCurvePoint(rate, head, power));
        }

        return points;
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
