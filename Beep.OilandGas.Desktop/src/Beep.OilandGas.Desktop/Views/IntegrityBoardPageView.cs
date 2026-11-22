using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Hosting;
using TheTechIdea.OilGas.Integrity;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// ENG-05: which wells are at risk right now, worst first, with the guideline-117 category and the findings behind it.
/// </summary>
/// <remarks>
/// The category is the owner's <c>IntegrityStatus</c> reading and nothing else; an unassessed well is labelled as such rather than
/// shown as green (AC-1); the order is the owner's worst-first rule with a documented tie-break (AC-2); the counts are the owner's
/// over the field (AC-3); every finding names the input that produced it (AC-4); the CSV is the owner's over the same wells
/// (AC-5); and a failed read clears the board rather than showing it green (AC-6). The board itself is the owner's
/// <c>IntegrityBoardView</c> drawn through the shared Skia host, which is what the feature's open question asked for.
/// </remarks>
public sealed class IntegrityBoardPageView : OgPageBase
{
    public const string Route = "integrityboard";

    private readonly OgApplication _application;
    private readonly IOgIntegrityBoard _boardService;
    private readonly IntegrityBoardView _board = new();
    private readonly SkiaViewSurfaceHost _host;

    private readonly TextBlock _header;
    private readonly TextBlock _counts;
    private readonly TextBlock _ordering;
    private readonly TextBlock _detail;
    private readonly TextBlock _findings;
    private readonly TextBlock _reasons;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _olderThan;
    private readonly TextBox _export;
    private readonly ComboBox _category;
    private readonly DataGrid _wellGrid;
    private readonly Button _loadButton;
    private readonly Button _recordButton;

    private OgIntegrityBoard? _workspace;
    private OgWellIntegrity? _selected;

    public IntegrityBoardPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _boardService = application.IntegrityBoard;

        _host = new SkiaViewSurfaceHost
        {
            FailureReporter = application.Diagnostics,
            MinHeight = 240,
        };

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 4, 0, 4);

        _ordering = Body(OgIntegrityBoard.OrderingText);
        _ordering.TextWrapping = TextWrapping.Wrap;
        _ordering.FontSize = 11.5;
        _ordering.Margin = new Thickness(0, 0, 0, 4);

        _detail = Body(string.Empty);
        _detail.TextWrapping = TextWrapping.Wrap;
        _detail.Margin = new Thickness(0, 0, 0, 4);

        _findings = Body(string.Empty);
        _findings.TextWrapping = TextWrapping.Wrap;
        _findings.Margin = new Thickness(0, 0, 0, 4);

        _reasons = Body(string.Empty);
        _reasons.TextWrapping = TextWrapping.Wrap;
        _reasons.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 130);
        _olderThan = Field("Assessed older than (days)", 150);

        _category = new ComboBox { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
        _category.ItemsSource = new List<string> { "All" }.Concat(IntegrityBoardRules.Categories).ToList();
        _category.SelectedIndex = 0;
        Named(_category, "Category");

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 80,
            MaxHeight = 140,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Board export");

        _wellGrid = Grid("Wells", ("Well", "WellId", 130), ("Name", "WellName", 140), ("Field", "Field", 110),
            ("Category", "Category", 90), ("Compliant", "Compliant", 80), ("Assessed", "AgeText", 130),
            ("State", "StateText", 200));

        _wellGrid.SelectionChanged += (_, _) => SelectWell();

        _loadButton = Button("Load board", (_, _) => Load());
        _recordButton = Button("Record assessment", (_, _) => _ = RecordAsync());

        var filterBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filterBar.Children.Add(Caption("Field"));
        filterBar.Children.Add(_field);
        filterBar.Children.Add(Caption("Category"));
        filterBar.Children.Add(_category);
        filterBar.Children.Add(Caption("Older than"));
        filterBar.Children.Add(_olderThan);
        filterBar.Children.Add(_loadButton);
        filterBar.Children.Add(_recordButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(filterBar);
        panel.Children.Add(_counts);
        panel.Children.Add(_ordering);
        panel.Children.Add(Caption("Board"));
        panel.Children.Add(_host);
        panel.Children.Add(Caption("Wells, worst first"));
        panel.Children.Add(_wellGrid);
        panel.Children.Add(Caption("Selected well"));
        panel.Children.Add(_detail);
        panel.Children.Add(Caption("Findings"));
        panel.Children.Add(_findings);
        panel.Children.Add(Caption("The assessment's reasons"));
        panel.Children.Add(_reasons);
        panel.Children.Add(Caption("Export"));
        panel.Children.Add(_export);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name a field, then load the board.");
    }

    public OgIntegrityBoard? Workspace => _workspace;

    public OgWellIntegrity? Selected => _selected;

    public bool LastFailed { get; private set; }

    public string HeaderText => _header.Text;

    public string CountsText => _counts.Text;

    public string OrderingText => _ordering.Text;

    public string DetailText => _detail.Text;

    public string FindingsText => _findings.Text;

    public string ReasonsText => _reasons.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ExportText => _export.Text;

    public int BoardWells => _board.Wells.Count;

    public bool Load()
    {
        _header.Text = "Engineering · integrity board";

        var loaded = _boardService
            .LoadAsync(Empty(_field.Text), CategoryFilter(), AgeFilter())
            .GetAwaiter().GetResult();

        if (!loaded.Success || loaded.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = loaded.Message ?? "The integrity board could not be read.";
            ShowState(OgViewState.Failed, "The board is not shown.", loaded.Reference);
            return false;
        }

        LastFailed = false;
        Apply(loaded.Value);
        return true;
    }

    public async Task<bool> RecordAsync()
    {
        var outcome = await _boardService.RecordAsync(
            Empty(_field.Text),
            CategoryFilter(),
            AgeFilter(),
            _application.Session.UserId).ConfigureAwait(true);

        if (!outcome.Success || outcome.Value is null)
        {
            LastFailed = true;
            _failure.Text = outcome.Message ?? "The assessment was refused.";
            ShowState(OgViewState.Failed, "The assessment was refused.", outcome.Reference);
            return false;
        }

        LastFailed = false;
        Apply(outcome.Value);

        _application.AuditLog.Add("integrity.assess", "integrity", outcome.Value.Field ?? "field", "ok",
            _application.Session.UserId);

        _failure.Text = string.Empty;
        _notice.Text = "Assessment recorded for " + outcome.Value.Wells.Count.ToString(CultureInfo.InvariantCulture) +
                       " well(s).";

        return true;
    }

    private void Apply(OgIntegrityBoard board)
    {
        _workspace = board;

        _counts.Text = board.CountsText;
        _refreshed.Text = board.ReadText;

        _wellGrid.ItemsSource = board.Wells.ToList();
        _wellGrid.SelectedItem = board.Wells.FirstOrDefault();
        SelectWell();

        _export.Text = board.Csv();

        // The owner's view is the board: the same wells, in the same order, in its category colours.
        _board.Wells.Clear();
        _board.Wells.AddRange(board.Board);
        _host.Surface = _board;
        _host.Redraw();

        _failure.Text = string.Empty;

        ShowState(board.Wells.Count == 0 ? OgViewState.Empty : OgViewState.Ready, board.CountsText);
    }

    private void SelectWell()
    {
        _selected = _wellGrid.SelectedItem as OgWellIntegrity;

        if (_selected is not { } well)
        {
            _detail.Text = "No well is selected.";
            _findings.Text = string.Empty;
            _reasons.Text = string.Empty;
            return;
        }

        _detail.Text = well.Describe() + Environment.NewLine + well.MissingText + Environment.NewLine + well.EnvelopesText;
        _findings.Text = well.FindingsText;
        _reasons.Text = well.ReasonsText;
    }

    private string? CategoryFilter() =>
        _category.SelectedItem as string is { } chosen && !string.Equals(chosen, "All", StringComparison.Ordinal)
            ? chosen
            : null;

    private double? AgeFilter() =>
        double.TryParse(_olderThan.Text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var days) && days > 0
            ? days
            : null;

    private void Clear()
    {
        _counts.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _detail.Text = string.Empty;
        _findings.Text = string.Empty;
        _reasons.Text = string.Empty;
        _export.Text = string.Empty;
        _wellGrid.ItemsSource = null;
        _notice.Text = string.Empty;

        // AC-6: a failed read clears the board rather than leaving a stale - or all-green - one on screen.
        _board.Wells.Clear();
        _host.Surface = _board;
        _host.Redraw();
    }

    private static string? Empty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 110,
            MaxHeight = 200,
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
