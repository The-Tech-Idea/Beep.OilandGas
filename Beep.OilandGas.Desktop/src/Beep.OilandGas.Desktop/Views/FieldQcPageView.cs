using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// INT-06: a whole field's defects before they reach a calculation - the engine's QC codes per well, how much of the model is
/// populated, the mapping gaps the engine proposes for, and the wells it thinks may be duplicates.
/// </summary>
/// <remarks>
/// The run is <c>FieldQc</c>'s own (AC-5: every finding carries the engine's code, the item it is about and the well where it was
/// attributed). Coverage read from no well is shown as unknown rather than zero (AC-2); a cancelled run reports the scope it actually
/// covered (AC-3); a duplicate stays a proposal until a person decides (AC-4); a proposal is applied only by the accept command
/// (AC-1); and the export is the grid's current filter (AC-6). The run is started on a background thread so cancelling is possible
/// while it is going.
/// </remarks>
public sealed class FieldQcPageView : OgPageBase
{
    public const string Route = "fieldqc";

    private readonly OgApplication _application;
    private readonly IOgFieldQc _qc;

    private readonly TextBlock _header;
    private readonly TextBlock _summary;
    private readonly TextBlock _counts;
    private readonly TextBlock _coverage;
    private readonly TextBlock _unresolved;
    private readonly TextBlock _duplicates;
    private readonly TextBlock _failed;
    private readonly TextBlock _thresholds;
    private readonly TextBlock _notes;
    private readonly TextBlock _failure;
    private readonly TextBlock _notice;
    private readonly TextBlock _refreshed;
    private readonly TextBox _source;
    private readonly TextBox _maxWells;
    private readonly TextBox _needle;
    private readonly TextBox _export;
    private readonly ComboBox _severity;
    private readonly CheckBox _withDuplicates;
    private readonly DataGrid _findingGrid;
    private readonly DataGrid _suggestionGrid;
    private readonly DataGrid _duplicateGrid;
    private readonly Button _runButton;
    private readonly Button _cancelButton;
    private readonly Button _acceptButton;
    private readonly Button _confirmButton;
    private readonly Button _rejectButton;

    private CancellationTokenSource? _running;
    private OgFieldQcWorkspace? _workspace;

    public FieldQcPageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _qc = application.FieldQc;

        _header = Body(string.Empty);
        _header.FontSize = 15;
        _header.FontWeight = FontWeights.SemiBold;

        _summary = Body(string.Empty);
        _summary.TextWrapping = TextWrapping.Wrap;
        _summary.FontWeight = FontWeights.SemiBold;
        _summary.Margin = new Thickness(0, 4, 0, 4);

        _counts = Body(string.Empty);
        _counts.TextWrapping = TextWrapping.Wrap;
        _counts.Margin = new Thickness(0, 0, 0, 4);

        _coverage = Body(string.Empty);
        _coverage.TextWrapping = TextWrapping.Wrap;
        _coverage.Margin = new Thickness(0, 0, 0, 4);

        _unresolved = Body(string.Empty);
        _unresolved.TextWrapping = TextWrapping.Wrap;
        _unresolved.Margin = new Thickness(0, 0, 0, 4);

        _duplicates = Body(string.Empty);
        _duplicates.TextWrapping = TextWrapping.Wrap;
        _duplicates.Margin = new Thickness(0, 0, 0, 4);

        _failed = Body(string.Empty);
        _failed.TextWrapping = TextWrapping.Wrap;
        _failed.Margin = new Thickness(0, 0, 0, 4);

        _thresholds = Body(string.Empty);
        _thresholds.TextWrapping = TextWrapping.Wrap;
        _thresholds.Margin = new Thickness(0, 0, 0, 4);

        _notes = Body(string.Empty);
        _notes.TextWrapping = TextWrapping.Wrap;
        _notes.Margin = new Thickness(0, 0, 0, 4);

        _failure = Body(string.Empty);
        _failure.TextWrapping = TextWrapping.Wrap;
        _failure.FontWeight = FontWeights.SemiBold;
        _failure.Margin = new Thickness(0, 4, 0, 4);

        _notice = Body(string.Empty);
        _notice.TextWrapping = TextWrapping.Wrap;

        _refreshed = Body(string.Empty);
        _refreshed.TextWrapping = TextWrapping.Wrap;

        _source = Field("Data source", 150);
        _maxWells = Field("At most wells", 90);
        _maxWells.Text = "500";
        _needle = Field("Filter", 200);

        _severity = new ComboBox { MinWidth = 110, Margin = new Thickness(0, 0, 8, 0) };

        _severity.ItemsSource = new List<string> { "(all)" }.Concat(OgFieldQcRules.Severities).ToList();
        _severity.SelectedIndex = 0;
        _severity.SelectionChanged += (_, _) => RefreshExport();
        Named(_severity, "Severity");

        _needle.TextChanged += (_, _) => RefreshExport();

        _withDuplicates = new CheckBox { Content = "Check duplicates", IsChecked = true, Margin = new Thickness(0, 0, 12, 0) };
        Named(_withDuplicates, "Check duplicates");

        _export = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            MinHeight = 90,
            MaxHeight = 170,
            Margin = new Thickness(0, 6, 0, 8),
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        Named(_export, "Findings export");

        _findingGrid = Grid("Findings", ("Scope", "Scope", 70), ("Well", "WellId", 130), ("Entity", "Entity", 80),
            ("Field", "Field", 100), ("Rule", "Rule", 90), ("Severity", "Severity", 80),
            ("Times", "Occurrences", 60), ("Message", "Message", 520));

        _suggestionGrid = Grid("Suggestions", ("Area", "Area", 80), ("Where", "Where", 130), ("Value", "Value", 150),
            ("Target", "Target", 170), ("Score", "Score", 70), ("By", "Source", 110));

        _duplicateGrid = Grid("Duplicates", ("Well A", "NameA", 150), ("Well B", "NameB", 150),
            ("Score", "Score", 70), ("Decision", "Decision", 100));

        _runButton = Button("Run QC", (_, _) => _ = RunAsync());
        _cancelButton = Button("Cancel", (_, _) => Cancel());
        _acceptButton = Button("Accept proposal", (_, _) => _ = AcceptAsync());
        _confirmButton = Button("Confirm duplicate", (_, _) => _ = DecideAsync(confirmed: true));
        _rejectButton = Button("Reject duplicate", (_, _) => _ = DecideAsync(confirmed: false));

        _cancelButton.IsEnabled = false;

        var scopeBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        scopeBar.Children.Add(Caption("Source"));
        scopeBar.Children.Add(_source);
        scopeBar.Children.Add(Caption("At most"));
        scopeBar.Children.Add(_maxWells);
        scopeBar.Children.Add(_withDuplicates);
        scopeBar.Children.Add(_runButton);
        scopeBar.Children.Add(_cancelButton);

        var filterBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        filterBar.Children.Add(Caption("Show"));
        filterBar.Children.Add(_severity);
        filterBar.Children.Add(_needle);

        var suggestionBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        suggestionBar.Children.Add(_acceptButton);

        var duplicateBar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        duplicateBar.Children.Add(_confirmButton);
        duplicateBar.Children.Add(_rejectButton);

        var panel = new StackPanel();
        panel.Children.Add(_header);
        panel.Children.Add(_refreshed);
        panel.Children.Add(scopeBar);
        panel.Children.Add(_summary);
        panel.Children.Add(_counts);
        panel.Children.Add(filterBar);
        panel.Children.Add(Caption("Findings"));
        panel.Children.Add(_findingGrid);
        panel.Children.Add(Caption("Coverage - how much of the model the wells carry"));
        panel.Children.Add(_coverage);
        panel.Children.Add(Caption("Unresolved values (the proposals below are what the engine could match)"));
        panel.Children.Add(_unresolved);
        panel.Children.Add(Caption("Mapping proposals - never applied without acceptance"));
        panel.Children.Add(_suggestionGrid);
        panel.Children.Add(suggestionBar);
        panel.Children.Add(Caption("Duplicate proposals - never merged without a decision"));
        panel.Children.Add(_duplicateGrid);
        panel.Children.Add(duplicateBar);
        panel.Children.Add(_duplicates);
        panel.Children.Add(Caption("Wells that did not load"));
        panel.Children.Add(_failed);
        panel.Children.Add(Caption("Thresholds this run used"));
        panel.Children.Add(_thresholds);
        panel.Children.Add(Caption("Notes"));
        panel.Children.Add(_notes);
        panel.Children.Add(Caption("Export the filtered findings"));
        panel.Children.Add(_export);
        panel.Children.Add(_notice);
        panel.Children.Add(_failure);

        ShowContent(panel);

        ShowState(OgViewState.Empty, "Name the data source and run the QC over its wells.");
    }

    public OgFieldQcWorkspace? Workspace => _workspace;

    public bool LastFailed { get; private set; }

    public bool Running => _running is not null;

    public string HeaderText => _header.Text;

    public string SummaryText => _summary.Text;

    public string CountsText => _counts.Text;

    public string CoverageText => _coverage.Text;

    public string UnresolvedText => _unresolved.Text;

    public string DuplicatesText => _duplicates.Text;

    public string FailedText => _failed.Text;

    public string ThresholdsText => _thresholds.Text;

    public string NotesText => _notes.Text;

    public string FailureText => _failure.Text;

    public string NoticeText => _notice.Text;

    public string ExportText => _export.Text;

    /// <summary>The findings the grid is showing: the run's, through the page's own filter.</summary>
    public IReadOnlyList<OgQcFinding> Filtered => OgFieldQcRules.Filter(
        _workspace?.Findings ?? [],
        _severity.SelectedItem as string is "(all)" or null ? null : _severity.SelectedItem as string,
        _needle.Text);

    public async Task<bool> RunAsync()
    {
        if (_running is not null)
        {
            return false;
        }

        _header.Text = "Data · field QC";

        var request = new OgFieldQcRequest(
            _source.Text?.Trim() ?? string.Empty,
            Number(_maxWells.Text),
            _withDuplicates.IsChecked == true);

        _running = new CancellationTokenSource();
        _cancelButton.IsEnabled = true;
        _runButton.IsEnabled = false;

        try
        {
            // The engine's run is synchronous, so it goes to a background thread: otherwise the window would freeze and Cancel
            // could never be pressed. The token goes to the service rather than to Task.Run, because cancellation is the service's
            // answer to report (the covered scope) and not an exception for the page to catch.
            var ran = await Task.Run(() => _qc.RunAsync(request, _running.Token), CancellationToken.None)
                .ConfigureAwait(true);

            return Settle(ran, "QC run finished.");
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
            _notice.Text = "Cancelling: the findings from the wells already read will be reported.";
        }
    }

    public async Task<bool> AcceptAsync()
    {
        if (_suggestionGrid.SelectedItem is not OgSetupSuggestion suggestion)
        {
            LastFailed = true;
            _failure.Text = "Select the proposal to accept; a proposal is never applied without one being chosen.";
            return false;
        }

        var accepted = await _qc
            .AcceptSuggestionAsync(_source.Text?.Trim() ?? string.Empty, suggestion, _application.Session.UserId)
            .ConfigureAwait(true);

        if (!accepted.Success)
        {
            LastFailed = true;
            _failure.Text = accepted.Message ?? "The proposal was refused.";
            return false;
        }

        LastFailed = false;

        _application.AuditLog.Add("settings.wellbore.setup", "settings", "field-qc", "ok", _application.Session.UserId);

        _suggestionGrid.ItemsSource = accepted.Value?.ToList();
        _failure.Text = string.Empty;
        _notice.Text = "Proposal accepted and applied to the environment.";

        return true;
    }

    public async Task<bool> DecideAsync(bool confirmed)
    {
        if (_duplicateGrid.SelectedItem is not OgQcDuplicate duplicate)
        {
            LastFailed = true;
            _failure.Text = "Select the proposed pair to decide on; a duplicate is never merged without one being chosen.";
            return false;
        }

        var decided = await _qc.DecideDuplicateAsync(
            new OgFieldQcRequest(
                _source.Text?.Trim() ?? string.Empty,
                Number(_maxWells.Text),
                WithDuplicates: true),
            duplicate.KeyA,
            duplicate.KeyB,
            confirmed,
            _application.Session.UserId).ConfigureAwait(true);

        return Settle(decided, confirmed ? "Pair confirmed as the same well." : "Pair rejected: they are different wells.");
    }

    private bool Settle(OgResult<OgFieldQcWorkspace> result, string notice)
    {
        if (!result.Success || result.Value is null)
        {
            LastFailed = true;
            _workspace = null;
            Clear();
            _failure.Text = result.Message ?? "The run was refused.";
            ShowState(OgViewState.Failed, "The findings are not shown.", result.Reference);
            return false;
        }

        LastFailed = false;
        Apply(result.Value);

        _failure.Text = string.Empty;
        _notice.Text = notice;

        return true;
    }

    private void Apply(OgFieldQcWorkspace workspace)
    {
        _workspace = workspace;

        _summary.Text = workspace.SummaryText;
        _counts.Text = workspace.CountsText;
        _coverage.Text = workspace.CoverageText;
        _unresolved.Text = workspace.UnresolvedText;
        _duplicates.Text = workspace.DuplicatesText;
        _failed.Text = workspace.FailedText;
        _thresholds.Text = workspace.ThresholdsText;
        _notes.Text = workspace.NotesText;
        _refreshed.Text = workspace.ReadText;

        _findingGrid.ItemsSource = OgFieldQcRules.Filter(
            workspace.Findings,
            _severity.SelectedItem as string is "(all)" or null ? null : _severity.SelectedItem as string,
            _needle.Text).ToList();

        _suggestionGrid.ItemsSource = workspace.Suggestions.ToList();
        _duplicateGrid.ItemsSource = workspace.Duplicates.ToList();

        _acceptButton.IsEnabled = workspace.Suggestions.Count > 0;
        _confirmButton.IsEnabled = workspace.Duplicates.Any(duplicate => !duplicate.Decided);
        _rejectButton.IsEnabled = _confirmButton.IsEnabled;
        _runButton.IsEnabled = !workspace.SourceConfigured || true;

        RefreshExport();

        ShowState(!workspace.SourceConfigured
            ? OgViewState.Empty
            : workspace.HasFindings ? OgViewState.Ready : OgViewState.Empty, workspace.CountsText);
    }

    /// <summary>AC-6: the export is the grid's current filter, rebuilt whenever the filter changes.</summary>
    private void RefreshExport()
    {
        var filtered = Filtered;

        _export.Text = OgFieldQcRules.Csv(filtered);

        if (_workspace is not null)
        {
            _findingGrid.ItemsSource = filtered.ToList();
            _counts.Text = _workspace.CountsText + " · showing " + filtered.Count.ToString(CultureInfo.InvariantCulture);
        }
    }

    private void Clear()
    {
        _summary.Text = string.Empty;
        _counts.Text = string.Empty;
        _coverage.Text = string.Empty;
        _unresolved.Text = string.Empty;
        _duplicates.Text = string.Empty;
        _failed.Text = string.Empty;
        _thresholds.Text = string.Empty;
        _notes.Text = string.Empty;
        _refreshed.Text = string.Empty;
        _findingGrid.ItemsSource = null;
        _suggestionGrid.ItemsSource = null;
        _duplicateGrid.ItemsSource = null;
        _export.Text = string.Empty;
        _acceptButton.IsEnabled = false;
        _confirmButton.IsEnabled = false;
        _rejectButton.IsEnabled = false;
        _notice.Text = string.Empty;
    }

    private static int Number(string? text) =>
        int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static DataGrid Grid(string name, params (string Header, string Path, double Width)[] columns)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            MinHeight = 100,
            MaxHeight = 190,
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
