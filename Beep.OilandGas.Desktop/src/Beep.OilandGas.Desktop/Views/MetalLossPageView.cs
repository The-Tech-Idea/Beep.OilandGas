using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-03: measure wall loss from a multi-finger caliper log, re-rate the pipe at the wall it has left, and state the
/// remaining-life basis - or say that no basis is recorded.
/// </summary>
/// <remarks>
/// The joints are the tools' own analysis of the caliper curves (AC-1); the damage classes are the configured ones, and their
/// absence is stated rather than filled with an invented band (AC-2); the re-rating uses the measured wall and names its load
/// case (AC-3); remaining life is projected only from a dated earlier inspection at the same depth and a configured limit
/// (AC-4); the export is the grid's rows (AC-5); and a recorded result keeps the log, the fingers, the wall basis and the classes
/// that produced it (AC-6).
/// </remarks>
public sealed class MetalLossPageView : OgPageBase
{
    public const string Route = "metalloss";

    private readonly OgApplication _application;
    private readonly IOgMetalLoss _metalLoss;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _log;
    private readonly TextBlock _worst;
    private readonly TextBlock _life;
    private readonly TextBlock _classes;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _caliper;
    private readonly TextBox _gradient;
    private readonly TextBox _testPressure;
    private readonly TextBox _export;
    private readonly DataGrid _stringGrid;
    private readonly DataGrid _jointGrid;
    private readonly DataGrid _rerateGrid;
    private readonly Button _loadButton;
    private readonly Button _analyseButton;

    private OgMetalLossWorkspace? _workspace;

    public MetalLossPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _metalLoss = application.MetalLoss;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _log = Body(string.Empty);
        _log.TextWrapping = TextWrapping.Wrap;
        _log.Margin = new Thickness(0, 0, 0, 4);

        _worst = Body(string.Empty);
        _worst.TextWrapping = TextWrapping.Wrap;
        _worst.FontWeight = FontWeights.SemiBold;
        _worst.Margin = new Thickness(0, 0, 0, 4);

        _life = Body(string.Empty);
        _life.TextWrapping = TextWrapping.Wrap;
        _life.Margin = new Thickness(0, 4, 0, 4);

        _classes = Body(string.Empty);
        _classes.TextWrapping = TextWrapping.Wrap;
        _classes.Margin = new Thickness(0, 0, 0, 4);

        _warnings = Body(string.Empty);
        _warnings.TextWrapping = TextWrapping.Wrap;
        _warnings.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _well = Field("Well UWI", 140);
        _caliper = Field("Caliper LAS file", 320);
        _gradient = Field("Mud density ppg", 110);
        _testPressure = Field("Test pressure psi", 120);

        _gradient.Text = "10";
        _testPressure.Text = "5000";

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 150,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Metal loss export");

        _stringGrid = Grid("Strings", ("String", "Name", 150), ("OD", "Od", 70), ("ID", "InnerDiameter", 70),
            ("Wall", "NominalWall", 70), ("Grade", "Grade", 80), ("Top", "Top", 70), ("Shoe", "Bottom", 70),
            ("Unit", "Unit", 60));

        _jointGrid = Grid("Joints", ("Joint", "Joint", 50), ("Top", "Top", 70), ("Shoe", "Bottom", 70),
            ("Nominal wall", "NominalWall", 90), ("Minimum", "MinRemainingWall", 80), ("Loss", "LossText", 80),
            ("Damage", "DamageClass", 100), ("Grade", "Grade", 70), ("Verdict", "VerdictText", 90));

        _rerateGrid = Grid("Re-rating", ("Case", "Case", 100), ("Resistance", "Resistance", 110),
            ("Allowed", "Rating", 110), ("Utilisation", "Utilisation", 100), ("Unit", "Unit", 60),
            ("Verdict", "Passes", 80));

        _loadButton = Button("Load record", (_, _) => Load());
        _analyseButton = Button("Analyse caliper", (_, _) => _ = AnalyseAsync());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(_loadButton);

        var logBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        logBar.Children.Add(Caption("Caliper"));
        logBar.Children.Add(_caliper);
        logBar.Children.Add(Caption("Mud"));
        logBar.Children.Add(_gradient);
        logBar.Children.Add(Caption("Test"));
        logBar.Children.Add(_testPressure);
        logBar.Children.Add(_analyseButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(_counts);
        panel.Children.Add(_log);
        panel.Children.Add(Caption("Casing and tubing"));
        panel.Children.Add(_stringGrid);
        panel.Children.Add(logBar);
        panel.Children.Add(Caption("Joints"));
        panel.Children.Add(_jointGrid);
        panel.Children.Add(_worst);
        panel.Children.Add(Caption("Re-rating of the worst joint"));
        panel.Children.Add(_rerateGrid);
        panel.Children.Add(Caption("Remaining life"));
        panel.Children.Add(_life);
        panel.Children.Add(Caption("Damage classes"));
        panel.Children.Add(_classes);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("Export"));
        panel.Children.Add(_export);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well, choose a caliper file and analyse it.");
    }

    public OgMetalLossWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string LogText => _log.Text;

    public string WorstText => _worst.Text;

    public string LifeText => _life.Text;

    public string ClassesText => _classes.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ExportText => _export.Text;

    public bool Load()
    {
        _header.Text = "Engineering · metal loss";

        var loaded = _metalLoss
            .LoadAsync(_well.Text?.Trim() ?? string.Empty)
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The metal-loss record could not be read.";
            ShowState(OgViewState.Failed, "The metal-loss record is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> AnalyseAsync()
    {
        var outcome = await _metalLoss.AnalyseAsync(
            _well.Text?.Trim() ?? string.Empty,
            _caliper.Text?.Trim() ?? string.Empty,
            Number(_gradient.Text),
            Number(_testPressure.Text),
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The caliper log was refused.";
            ShowState(OgViewState.Failed, "The caliper log was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("integrity.mfc.write", "integrity", outcome.Value.ResultId ?? "metal-loss", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = outcome.Value.Failed == 0
            ? "Analysis recorded: every measured joint still meets its design factor."
            : "Analysis recorded: " + outcome.Value.Failed.ToString(CultureInfo.InvariantCulture) +
              " joint(s) no longer meet their design factor.";

        return true;
    }

    private void Apply(OgMetalLossWorkspace workspace)
    {
        _workspace = workspace;

        if (string.IsNullOrWhiteSpace(_well.Text))
        {
            _well.Text = workspace.WellId;
        }

        _counts.Text = workspace.WellId + " · " + workspace.CountsText;
        _log.Text = workspace.LogText;
        _worst.Text = workspace.WorstText;
        _life.Text = workspace.Life.Describe();
        _classes.Text = workspace.DamageClassNote;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _stringGrid.ItemsSource = workspace.Strings.ToList();
        _jointGrid.ItemsSource = workspace.Joints.ToList();
        _rerateGrid.ItemsSource = workspace.Rerate.ToList();

        _export.Text = MetalLossRules.Csv(workspace.Joints);

        _failure.Text = string.Empty;

        ShowState(workspace.Joints.Count == 0 ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _counts.Text = string.Empty;
        _log.Text = string.Empty;
        _worst.Text = string.Empty;
        _life.Text = string.Empty;
        _classes.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _stringGrid.ItemsSource = null;
        _jointGrid.ItemsSource = null;
        _rerateGrid.ItemsSource = null;
        _export.Text = string.Empty;
        _notice.Text = string.Empty;
    }

    private static decimal Number(string? text) =>
        decimal.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0m;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 90,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
        };

        foreach (var (header, path, width) in columns)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = header,
                Binding = new Binding(path),
                Width = new DataGridLength(width),
            });
        }

        Named(grid, name);
        return grid;
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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 6, 0, 0),
        FontSize = 11.5,
        FontWeight = FontWeights.SemiBold,
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
}
