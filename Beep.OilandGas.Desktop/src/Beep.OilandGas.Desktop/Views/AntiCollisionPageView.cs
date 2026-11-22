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
/// ENG-01: prove the wells do not collide - the separation factor and MASD against the offset wells, with the rule stated and
/// the evidence exportable.
/// </summary>
/// <remarks>
/// The rule the scan applied is printed from the settings (AC-2); an offset without a survey is excluded and named (AC-3);
/// violations are counted and marked in the grid (AC-4); the export is built from the same rows (AC-5); and a saved run records
/// the error model, sigma and environment (AC-6).
/// </remarks>
public sealed class AntiCollisionPageView : OgPageBase
{
    public const string Route = "anticollision";

    private readonly OgApplication _application;
    private readonly IOgAntiCollision _scan;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _rule;
    private readonly TextBlock _excluded;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _reference;
    private readonly TextBox _offsets;
    private readonly TextBox _export;
    private readonly DataGrid _grid;
    private readonly Button _scanButton;
    private readonly Button _saveButton;

    private OgAntiCollisionScan? _result;

    public AntiCollisionPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _scan = application.AntiCollision;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _rule = Body(string.Empty);
        _rule.TextWrapping = TextWrapping.Wrap;
        _rule.Margin = new Thickness(0, 0, 0, 4);

        _excluded = Body(string.Empty);
        _excluded.TextWrapping = TextWrapping.Wrap;
        _excluded.Margin = new Thickness(0, 0, 0, 4);

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

        _reference = Field("Reference well", 140);
        _offsets = Field("Offset wells (comma separated)", 320);

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 100,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Scan export");

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 140,
            MaxHeight = 260,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            Columns =
            {
                Text("Offset", "Offset", 120),
                Text("MD", "Md", 90),
                Text("Distance", "Distance", 100),
                Text("Sigma", "SigmaS", 90),
                Text("SF", "SeparationFactor", 90),
                Text("MASD", "Masd", 90),
                Text("Status", "Status", 110),
            },
        };

        Named(_grid, "Separation");

        _scanButton = Button("Scan", (_, _) => Scan());
        _saveButton = Button("Save run", (_, _) => _ = SaveAsync());

        var bar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        bar.Children.Add(Caption("Reference"));
        bar.Children.Add(_reference);
        bar.Children.Add(Caption("Offsets"));
        bar.Children.Add(_offsets);
        bar.Children.Add(_scanButton);
        bar.Children.Add(_saveButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(bar);
        panel.Children.Add(Caption("Rule applied"));
        panel.Children.Add(_rule);
        panel.Children.Add(_counts);
        panel.Children.Add(_excluded);
        panel.Children.Add(Caption("Separation"));
        panel.Children.Add(_grid);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(Caption("Export"));
        panel.Children.Add(_export);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name a reference well and its offsets, then scan.");
    }

    public OgAntiCollisionScan? Result => _result;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string RuleText => _rule.Text;

    public string ExcludedText => _excluded.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ExportText => _export.Text;

    public bool Scan()
    {
        _header.Text = "Engineering · anti-collision";

        var offsets = (_offsets.Text ?? string.Empty)
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var scanned = _scan
            .ScanAsync(_reference.Text?.Trim() ?? string.Empty, offsets)
            .GetAwaiter().GetResult();

        if (!scanned.Success || scanned.Value is null)
        {
            LastFailed = true;
            _result = null;
            _counts.Text = string.Empty;
            _rule.Text = string.Empty;
            _excluded.Text = string.Empty;
            _warnings.Text = string.Empty;
            _refreshed.Text = string.Empty;
            _grid.ItemsSource = null;
            _export.Text = string.Empty;
            _failure.Text = scanned.Message ?? "The scan could not be computed.";
            ShowState(OgViewState.Failed, "The scan is not shown.", scanned.Reference);
            return false;
        }

        LastFailed = false;
        Apply(scanned.Value);
        return true;
    }

    public async Task<bool> SaveAsync()
    {
        if (_result is not { } result)
        {
            _failure.Text = "Scan before saving a run.";
            return false;
        }

        var saved = await _scan
            .SaveRunAsync(result, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!saved.Success || string.IsNullOrWhiteSpace(saved.Value))
        {
            LastFailed = true;
            _failure.Text = saved.Message ?? "The run was not saved.";
            ShowState(OgViewState.Failed, "The run was not saved.", saved.Reference);
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("engineering.anticollision.write", "engineering", saved.Value, "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Run saved: " + result.ErrorModel + " · σ " +
                       result.Sigma.ToString(CultureInfo.InvariantCulture) + " · " + result.Environment + ".";

        return true;
    }

    private void Apply(OgAntiCollisionScan result)
    {
        _result = result;

        _counts.Text = result.CountsText;
        _rule.Text = result.Rule.Describe() + Environment.NewLine +
                     "levels: " + string.Join(" · ", result.Rule.Levels);
        _excluded.Text = result.ExcludedText;
        _warnings.Text = result.WarningsText;
        _refreshed.Text =
            "reference " + result.Reference + " · model " + result.ErrorModel +
            " · sigma " + result.Sigma.ToString(CultureInfo.InvariantCulture) +
            " · environment " + result.Environment + " · run " +
            result.RunAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        _grid.ItemsSource = result.Rows.ToList();

        // AC-5: the export is the same rows, so it cannot disagree with the grid.
        _export.Text = result.Csv();

        _failure.Text = string.Empty;
        _notice.Text = result.Violations == 0
            ? "No violation at the configured level."
            : result.Violations.ToString(CultureInfo.InvariantCulture) + " violation(s) at the configured level.";

        ShowState(result.Rows.Count == 0 ? OgViewState.Empty : OgViewState.Ready, result.CountsText);
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

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
    };
}
