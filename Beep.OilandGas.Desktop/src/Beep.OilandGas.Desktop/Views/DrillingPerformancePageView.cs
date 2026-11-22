using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Drilling;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-09: drilling performance from the daily reports - the time-depth curve, non-productive time by cause, rate of penetration by
/// run, and the same measures on offset wells.
/// </summary>
/// <remarks>
/// The analysis is the owner's <c>DrillingPerformance.Analyze</c>, and the curve is drawn by the owner's own <c>TimeDepthView</c>
/// inside the shared Skia host (AC-1); the non-productive time adds up by cause with an explicit unclassified bucket (AC-2, AC-3); a
/// CSV line with an unrecognised code is rejected with its line number (AC-4); every offset row comes from the same analysis
/// (AC-5); and the export is the grid's rows and the cause table field for field (AC-6).
/// </remarks>
public sealed class DrillingPerformancePageView : OgPageBase
{
    public const string Route = "drillingperformance";

    private readonly OgApplication _application;
    private readonly IOgDrillingPerformance _performance;
    private readonly TimeDepthView _timeDepth = new();
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _summary;
    private readonly TextBlock _curve;
    private readonly TextBlock _npt;
    private readonly TextBlock _rop;
    private readonly TextBlock _codes;
    private readonly TextBlock _cost;
    private readonly TextBlock _offsets;
    private readonly TextBlock _import;
    private readonly TextBlock _warnings;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _well;
    private readonly TextBox _offsetWells;
    private readonly TextBox _csv;
    private readonly TextBox _export;
    private readonly DataGrid _rowGrid;
    private readonly Button _loadButton;
    private readonly Button _importButton;
    private readonly Button _compareButton;

    private OgPerformanceWorkspace? _workspace;

    public DrillingPerformancePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _performance = application.DrillingPerformance;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 260,
        };

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.Margin = new Thickness(0, 0, 0, 4);

        _curve = Body(string.Empty);
        _curve.TextWrapping = TextWrapping.Wrap;
        _curve.Margin = new Thickness(0, 0, 0, 4);

        _npt = Body(string.Empty);
        _npt.TextWrapping = TextWrapping.Wrap;
        _npt.Margin = new Thickness(0, 0, 0, 4);

        _rop = Body(string.Empty);
        _rop.TextWrapping = TextWrapping.Wrap;
        _rop.Margin = new Thickness(0, 0, 0, 4);

        _codes = Body(string.Empty);
        _codes.TextWrapping = TextWrapping.Wrap;
        _codes.Margin = new Thickness(0, 0, 0, 4);

        _cost = Body(string.Empty);
        _cost.TextWrapping = TextWrapping.Wrap;
        _cost.Margin = new Thickness(0, 0, 0, 4);

        _offsets = Body(string.Empty);
        _offsets.TextWrapping = TextWrapping.Wrap;
        _offsets.Margin = new Thickness(0, 4, 0, 4);

        _import = Body(string.Empty);
        _import.TextWrapping = TextWrapping.Wrap;
        _import.Margin = new Thickness(0, 4, 0, 4);

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

        _well = Field("Well id", 130);

        _offsetWells = Field("Offset wells, comma separated", 260);
        _offsetWells.Text = "W-2, W-3";

        _csv = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 80,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            ToolTip = "start, end, code, depth_from, depth_to, description, npt - ISO dates, one header line",
        };

        _csv.Text =
            "start,end,code,depth_from,depth_to,description,npt\n" +
            "2026-10-01T06:00:00Z,2026-10-01T12:00:00Z,2,1000,1250,drilled 8.5in hole,false\n" +
            "2026-10-01T12:00:00Z,2026-10-01T18:00:00Z,6,1250,1250,trip for bit,false\n" +
            "2026-10-01T18:00:00Z,2026-10-02T02:00:00Z,7,1250,1250,pump failed,true";

        Named(_csv, "CSV");

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 160,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Performance export");

        _rowGrid = Grid("Lines", ("#", "Line", 40), ("Report", "ReportId", 120), ("Start", "Start", 130),
            ("Code", "Code", 60), ("Operation", "Name", 150), ("Hours", "Hours", 70), ("From", "From", 80),
            ("To", "To", 80), ("NPT", "Npt", 60), ("Cause", "CauseText", 110),
            ("Description", "Description", 200));

        _loadButton = Button("Analyse", (_, _) => Load());
        _importButton = Button("Import CSV", (_, _) => _ = ImportAsync());
        _compareButton = Button("Compare offsets", (_, _) => _ = CompareAsync());

        var wellBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        wellBar.Children.Add(Caption("Well"));
        wellBar.Children.Add(_well);
        wellBar.Children.Add(_loadButton);

        var offsetBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        offsetBar.Children.Add(Caption("Offsets"));
        offsetBar.Children.Add(_offsetWells);
        offsetBar.Children.Add(_compareButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(wellBar);
        panel.Children.Add(_counts);
        panel.Children.Add(_summary);
        panel.Children.Add(_curve);
        panel.Children.Add(Caption("Time against depth"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Non-productive time by cause"));
        panel.Children.Add(_npt);
        panel.Children.Add(Caption("Rate of penetration by run"));
        panel.Children.Add(_rop);
        panel.Children.Add(Caption("Time by operation"));
        panel.Children.Add(_codes);
        panel.Children.Add(Caption("Cost"));
        panel.Children.Add(_cost);
        panel.Children.Add(Caption("Lines"));
        panel.Children.Add(_rowGrid);
        panel.Children.Add(offsetBar);
        panel.Children.Add(_offsets);
        panel.Children.Add(Caption("Import report lines"));
        panel.Children.Add(_csv);
        panel.Children.Add(_importButton);
        panel.Children.Add(_import);
        panel.Children.Add(Caption("Export"));
        panel.Children.Add(_export);
        panel.Children.Add(Caption("Warnings"));
        panel.Children.Add(_warnings);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the well whose daily reports are analysed.");
    }

    public OgPerformanceWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string SummaryText => _summary.Text;

    public string CurveText => _curve.Text;

    public string NptText => _npt.Text;

    public string RopText => _rop.Text;

    public string CodesText => _codes.Text;

    public string CostText => _cost.Text;

    public string OffsetsText => _offsets.Text;

    public string ImportText => _import.Text;

    public string WarningsText => _warnings.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ExportText => _export.Text;

    public int CurvePoints => _timeDepth.Result?.TimeDepth.Count ?? 0;

    public int ImportedLines { get; private set; }

    public bool Load()
    {
        _header.Text = "Engineering · drilling performance";

        var loaded = _performance.LoadAsync(_well.Text?.Trim() ?? string.Empty).GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The performance could not be read.";
            ShowState(OgViewState.Failed, "The performance is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> ImportAsync()
    {
        var outcome = await _performance
            .ImportCsvAsync(_well.Text?.Trim() ?? string.Empty, _csv.Text ?? string.Empty,
                _application.Session.UserId)
            .ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _import.Text = outcome.Message ?? "The lines were refused.";
            _failure.Text = outcome.Message ?? "The lines were refused.";
            ShowState(OgViewState.Failed, "The import is refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        ImportedLines = outcome.Value.Accepted;

        _import.Text = outcome.Value.Describe() + Environment.NewLine + outcome.Value.RejectedText;

        _application.AuditLog.Add("engineering.performance.write", "engineering", _well.Text?.Trim() ?? "well", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = outcome.Value.Rejected.Count == 0
            ? "Lines imported."
            : "Lines imported, and " + outcome.Value.Rejected.Count.ToString(CultureInfo.InvariantCulture) +
              " rejected with their line numbers.";

        return Load();
    }

    public async Task<bool> CompareAsync()
    {
        var offsets = Offsets(_offsetWells.Text, out var problem);

        if (problem is not null)
        {
            LastFailed = true;
            _failure.Text = problem;
            return false;
        }

        var compared = await _performance
            .OffsetsAsync(_well.Text?.Trim() ?? string.Empty, offsets)
            .ConfigureAwait(true);

        if (!compared.Success || compared.Value is null)
        {
            LastFailed = true;
            _offsets.Text = compared.Message ?? "The comparison was refused.";
            _failure.Text = compared.Message ?? "The comparison was refused.";
            return false;
        }

        LastFailed = false;
        _offsets.Text = string.Join(Environment.NewLine, compared.Value.Select(row => row.Describe())) +
                        Environment.NewLine +
                        "Every row is the same analysis: days from the first line, TD from the time-depth curve, NPT as a " +
                        "share of all reported hours, and ROP as footage over drilling-code hours.";
        _failure.Text = string.Empty;

        return true;
    }

    /// <summary>AC-5: the offset ids, trimmed and de-duplicated, or a refusal naming what was written.</summary>
    public static IReadOnlyList<string> Offsets(string? text, out string? problem)
    {
        problem = null;

        var wells = (text ?? string.Empty)
            .Split([',', ';', ' '], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (wells.Count == 0)
        {
            problem = "An offset comparison needs at least one offset well; none is named.";
        }

        return wells;
    }

    private void Apply(OgPerformanceWorkspace workspace)
    {
        _workspace = workspace;

        if (string.IsNullOrWhiteSpace(_well.Text))
        {
            _well.Text = workspace.WellId;
        }

        _counts.Text = workspace.WellId + " · " + workspace.CountsText;
        _summary.Text = workspace.SummaryText;
        _curve.Text = workspace.CurveText;
        _npt.Text = workspace.NptText;
        _rop.Text = workspace.RopText;
        _codes.Text = workspace.CodeText;
        _cost.Text = workspace.CostText;
        _warnings.Text = workspace.WarningsText;
        _refreshed.Text = workspace.ReadText;

        _rowGrid.ItemsSource = workspace.Rows.ToList();

        // AC-6: the export is the grid's rows and the cause table, from the same collections the screen binds.
        _export.Text = workspace.RowsCsv() + Environment.NewLine + workspace.CausesCsv();

        // The curve is the owner's own view: the analysis result is what it draws.
        _timeDepth.Result = workspace.Owner;
        _timeDepth.DepthUnit = workspace.Unit;
        _host.Surface = _timeDepth;
        _host.Redraw();

        _failure.Text = string.Empty;

        ShowState(workspace.Empty ? OgViewState.Empty : OgViewState.Ready, workspace.CountsText);
    }

    private void Clear()
    {
        _counts.Text = string.Empty;
        _summary.Text = string.Empty;
        _curve.Text = string.Empty;
        _npt.Text = string.Empty;
        _rop.Text = string.Empty;
        _codes.Text = string.Empty;
        _cost.Text = string.Empty;
        _offsets.Text = string.Empty;
        _warnings.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _rowGrid.ItemsSource = null;
        _export.Text = string.Empty;
        _import.Text = string.Empty;

        // A failed read clears the curve rather than leaving the last well's performance on screen.
        _timeDepth.Result = null;
        _host.Surface = _timeDepth;
        _host.Redraw();

        _notice.Text = string.Empty;
    }

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 130,
            MaxHeight = 240,
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
