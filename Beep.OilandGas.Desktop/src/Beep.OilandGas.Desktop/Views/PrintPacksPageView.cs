using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// RPT-02: a pack defined once - content, scope and print options - previewed, run with progress and cancellable, and manifested.
/// </summary>
/// <remarks>
/// The document is the owners': a well view printed by <c>WellboreExport</c> with the pack's paper, scale, header and legend (AC-5), or an
/// RPT-01 report printed through its own service. A well that cannot produce a document is listed as skipped with the reason (AC-2); a
/// cancellation keeps what completed and still writes the manifest (AC-3); file names are deterministic and unique (AC-4); and the
/// manifest's pages are read back out of each file it describes (AC-6).
/// </remarks>
public sealed class PrintPacksPageView : OgPageBase
{
    public const string Route = "printpacks";

    private readonly OgApplication _application;
    private readonly IOgPrintPacks _packs;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _estimate;
    private readonly TextBlock _names;
    private readonly TextBlock _skippedPreview;
    private readonly TextBlock _progress;
    private readonly TextBlock _manifest;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _field;
    private readonly TextBox _wells;
    private readonly TextBox _title;
    private readonly TextBox _folder;
    private readonly ComboBox _content;
    private readonly ComboBox _scope;
    private readonly ComboBox _paper;
    private readonly TextBox _scale;
    private readonly CheckBox _headerBlock;
    private readonly CheckBox _legend;
    private readonly CheckBox _stopOnError;
    private readonly DataGrid _itemGrid;
    private readonly Button _estimateButton;
    private readonly Button _runButton;
    private readonly Button _cancelButton;

    private OgPackEstimate? _estimate_;
    private OgPackManifest? _manifest_;
    private CancellationTokenSource? _running;

    public PrintPacksPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _packs = application.PrintPacks;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _estimate = Body(string.Empty);
        _estimate.TextWrapping = TextWrapping.Wrap;
        _estimate.Margin = new Thickness(0, 0, 0, 4);

        _names = Body(string.Empty);
        _names.FontFamily = new System.Windows.Media.FontFamily("Consolas");
        _names.TextWrapping = TextWrapping.Wrap;
        _names.Margin = new Thickness(0, 0, 0, 4);

        _skippedPreview = Body(string.Empty);
        _skippedPreview.TextWrapping = TextWrapping.Wrap;
        _skippedPreview.Margin = new Thickness(0, 0, 0, 4);

        _progress = Body(string.Empty);
        _progress.TextWrapping = TextWrapping.Wrap;
        _progress.Margin = new Thickness(0, 4, 0, 4);

        _manifest = Body(string.Empty);
        _manifest.TextWrapping = TextWrapping.Wrap;
        _manifest.Margin = new Thickness(0, 4, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _field = Field("Field", 120);
        _wells = Field("Wells, comma separated", 260);
        _title = Field("Title, or the report key", 200);
        _folder = Field("Output folder", 320);
        _folder.Text = OgPaths.ExportsDirectory;

        _scale = Field("Scale 1:n (0 fits a page)", 120);
        _scale.Text = "0";

        _content = new ComboBox { MinWidth = 150, Margin = new Thickness(0, 0, 8, 0) };
        _content.ItemsSource = OgPackRules.ContentTypes.Select(type => type.Key).ToList();
        _content.SelectedIndex = 0;
        _content.SelectionChanged += (_, _) => ShowContent();
        Named(_content, "Content type");

        _scope = new ComboBox { MinWidth = 130, Margin = new Thickness(0, 0, 8, 0) };
        _scope.ItemsSource = new List<string> { OgPackScope.All, OgPackScope.FieldScope, OgPackScope.Selected };
        _scope.SelectedIndex = 0;
        Named(_scope, "Scope");

        _paper = new ComboBox { MinWidth = 100, Margin = new Thickness(0, 0, 8, 0) };
        _paper.ItemsSource = OgPackRules.Papers.ToList();
        _paper.SelectedIndex = 0;
        Named(_paper, "Paper");

        _headerBlock = new CheckBox { Content = "Header block", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };
        _legend = new CheckBox { Content = "Legend", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };
        _stopOnError = new CheckBox { Content = "Stop on the first failure", Margin = new Thickness(0, 0, 12, 0) };

        Named(_headerBlock, "Header block");
        Named(_legend, "Legend");
        Named(_stopOnError, "Stop on error");

        _itemGrid = Grid("Manifest", ("Well", "WellId", 140), ("File", "File", 260), ("Pages", "Pages", 70),
            ("Bytes", "Bytes", 90), ("Status", "Status", 80), ("Reason", "Reason", 300));

        _estimateButton = Button("Estimate", (_, _) => _ = EstimateAsync());
        _runButton = Button("Run the pack", (_, _) => _ = RunAsync());
        _cancelButton = Button("Cancel", (_, _) => Cancel());

        _cancelButton.IsEnabled = false;

        var contentBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        contentBar.Children.Add(Caption("Content"));
        contentBar.Children.Add(_content);
        contentBar.Children.Add(Caption("Scope"));
        contentBar.Children.Add(_scope);
        contentBar.Children.Add(Caption("Field"));
        contentBar.Children.Add(_field);

        var scopeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        scopeBar.Children.Add(Caption("Wells"));
        scopeBar.Children.Add(_wells);
        scopeBar.Children.Add(Caption("Title / report"));
        scopeBar.Children.Add(_title);

        var optionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        optionBar.Children.Add(Caption("Paper"));
        optionBar.Children.Add(_paper);
        optionBar.Children.Add(Caption("Scale"));
        optionBar.Children.Add(_scale);
        optionBar.Children.Add(_headerBlock);
        optionBar.Children.Add(_legend);
        optionBar.Children.Add(_stopOnError);

        var runBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        runBar.Children.Add(Caption("To"));
        runBar.Children.Add(_folder);
        runBar.Children.Add(_estimateButton);
        runBar.Children.Add(_runButton);
        runBar.Children.Add(_cancelButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(contentBar);
        panel.Children.Add(scopeBar);
        panel.Children.Add(optionBar);
        panel.Children.Add(runBar);
        panel.Children.Add(_summary);
        panel.Children.Add(Caption("Preview"));
        panel.Children.Add(_estimate);
        panel.Children.Add(_names);
        panel.Children.Add(_skippedPreview);
        panel.Children.Add(Caption("Progress"));
        panel.Children.Add(_progress);
        panel.Children.Add(Caption("Manifest"));
        panel.Children.Add(_manifest);
        panel.Children.Add(_itemGrid);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Define a pack and estimate it before running.");
    }

    public OgPackEstimate? Estimate => _estimate_;

    public OgPackManifest? Manifest => _manifest_;

    public bool LastFailed { get; private set; }

    public bool Running => _running is not null;

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string EstimateText => _estimate.Text;

    public string NamesText => _names.Text;

    public string SkippedText => _skippedPreview.Text;

    public string ProgressText => _progress.Text;

    public string ManifestText => _manifest.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ContentType => _content.SelectedItem as string ?? "wellview";

    public string Scope => _scope.SelectedItem as string ?? OgPackScope.All;

    public OgPrintPack Pack() => new(
        ContentType,
        new OgPackScope(
            Scope,
            string.IsNullOrWhiteSpace(_field.Text) ? null : _field.Text!.Trim(),
            (_wells.Text ?? string.Empty).Split([',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)),
        new OgPackOptions(
            _paper.SelectedItem as string ?? "A4",
            float.TryParse(_scale.Text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var scale) ? scale : 0f,
            _headerBlock.IsChecked == true,
            _legend.IsChecked == true,
            string.IsNullOrWhiteSpace(_title.Text) ? null : _title.Text!.Trim()));

    public bool Load()
    {
        _header.Text = "Reporting · print packs";

        ShowContent();

        _refreshed.Text = OgPackRules.ContentTypes.Count.ToString(CultureInfo.InvariantCulture) + " content type(s) · " +
                          OgPackRules.Papers.Count.ToString(CultureInfo.InvariantCulture) + " paper size(s)";

        ShowState(OgViewState.Ready, _refreshed.Text);

        return true;
    }

    public async Task<bool> EstimateAsync()
    {
        var estimated = await _packs.EstimateAsync(Pack(), _application.Permissions).ConfigureAwait(true);

        if (!estimated.Success || estimated.Value is null)
        {
            LastFailed = true;
            _estimate_ = null;
            ClearPreview();
            _failure.Text = estimated.Message ?? "The pack could not be estimated.";
            ShowState(OgViewState.Failed, "The pack is not estimated.", estimated.Reference);
            return false;
        }

        LastFailed = false;
        _estimate_ = estimated.Value;

        _summary.Text = Pack().Describe();
        _estimate.Text = estimated.Value.Describe();
        _names.Text = estimated.Value.NamesText;
        _skippedPreview.Text = estimated.Value.SkippedText + (estimated.Value.Warnings.Count == 0
            ? string.Empty
            : Environment.NewLine + string.Join(Environment.NewLine, estimated.Value.Warnings.Select(warning => "! " + warning)));

        _failure.Text = string.Empty;
        _notice.Text = "Nothing was written: this is the preview.";

        ShowState(estimated.Value.Files > 0 ? OgViewState.Ready : OgViewState.Empty, estimated.Value.Describe());

        return estimated.Value.Files > 0;
    }

    public async Task<bool> RunAsync()
    {
        if (_running is not null)
        {
            return false;
        }

        _running = new CancellationTokenSource();
        _cancelButton.IsEnabled = true;
        _runButton.IsEnabled = false;

        try
        {
            var progress = new Progress<OgPackProgress>(report => _progress.Text = report.Describe());

            // The run is synchronous work over a folder, so it goes to a background thread; the token reaches the service, which takes it
            // at a well boundary (AC-3).
            var ran = await Task.Run(
                () => _packs.RunAsync(Pack(), _folder.Text?.Trim() ?? string.Empty, _application.Permissions,
                    _stopOnError.IsChecked == true, progress, _running.Token),
                CancellationToken.None).ConfigureAwait(true);

            if (!ran.Success || ran.Value is null)
            {
                LastFailed = true;
                _manifest_ = null;
                _itemGrid.ItemsSource = null;
                _manifest.Text = ran.Message ?? "The pack was not run.";
                _failure.Text = ran.Message ?? "The pack was not run.";
                ShowState(OgViewState.Failed, "The pack is not run.", ran.Reference);
                return false;
            }

            LastFailed = false;
            _manifest_ = ran.Value;

            _manifest.Text = ran.Value.SummaryText + Environment.NewLine +
                             (ran.Value.ManifestPath is null
                                 ? "The manifest could not be written."
                                 : "manifest " + ran.Value.ManifestPath + Environment.NewLine + "csv " + ran.Value.CsvPath);
            _itemGrid.ItemsSource = ran.Value.Items.ToList();
            _failure.Text = string.Empty;

            _application.AuditLog.Add("reporting.pack.run", "reporting", ran.Value.Folder, "ok",
                _application.Session.UserId);

            _notice.Text = ran.Value.Cancelled
                ? "Cancelled: " + ran.Value.Written.ToString(CultureInfo.InvariantCulture) +
                  " document(s) were kept and the manifest records them."
                : "Pack written: " + ran.Value.Written.ToString(CultureInfo.InvariantCulture) + " document(s).";

            ShowState(OgViewState.Ready, ran.Value.SummaryText);

            return true;
        }
        finally
        {
            _running?.Dispose();
            _running = null;
            _cancelButton.IsEnabled = false;
            _runButton.IsEnabled = true;
        }
    }

    public void Cancel()
    {
        if (_running is { IsCancellationRequested: false })
        {
            _running.Cancel();
            _progress.Text = "Cancelling: the documents already written will be kept and manifested.";
        }
    }

    private void ShowContent()
    {
        if (OgPackRules.ContentTypes.FirstOrDefault(type =>
                string.Equals(type.Key, ContentType, StringComparison.OrdinalIgnoreCase)) is not { } contentType)
        {
            return;
        }

        _summary.Text = contentType.Describe();

        if (contentType.IsReport)
        {
            _title.Text = string.IsNullOrWhiteSpace(_title.Text) ? contentType.ReportKey : _title.Text;
        }
    }

    private void ClearPreview()
    {
        _estimate.Text = string.Empty;
        _names.Text = string.Empty;
        _skippedPreview.Text = string.Empty;
    }

    /// <summary>
    /// The packs a run can be cancelled between: named here so the page can say what the granularity is without the service's internals.
    /// </summary>
    public const string CancellationGranularity = "well";

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
