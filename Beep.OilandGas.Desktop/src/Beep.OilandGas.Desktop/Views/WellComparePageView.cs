using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>
/// WEL-06: two to six wells side by side on named properties, with units explicit and differences only where they
/// are comparable.
/// </summary>
/// <remarks>
/// A unit mismatch leaves the cells marked and the difference column empty (AC-1); a missing value is "-" and never
/// zero (AC-2). The export is built from the same matrix the grid shows (AC-3). The overlay panel states why it is
/// disabled when a well lacks the geometry (AC-4). A new selection cancels the load in flight (AC-5).
/// </remarks>
public sealed class WellComparePageView : OgPageBase
{
    public const string Route = "compare";

    private readonly OgApplication _application;
    private readonly IOgWellComparison _comparison;
    private readonly OgCompareGate _gate = new();

    private readonly TextBlock _notice;
    private readonly TextBlock _overlay;
    private readonly ListBox _wells;
    private readonly DataGrid _grid;

    public WellComparePageView(OgApplication application)
        : base(application, Route)
    {
        _application = application;
        _comparison = application.WellComparison;

        _notice = Body(string.Empty);
        _overlay = Body(string.Empty);

        _wells = new ListBox
        {
            SelectionMode = SelectionMode.Multiple,
            MinWidth = 260,
            MinHeight = 240,
            IsTabStop = true,
            Margin = new Thickness(0, 6, 8, 6),
        };

        Named(_wells, "Wells to compare");
        _wells.SelectionChanged += (_, _) => SelectionChanged();

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            Margin = new Thickness(0, 6, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            MinHeight = 260,
        };

        Named(_grid, "Comparison matrix");

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 6) };
        toolbar.Children.Add(Button("Compare selected", (_, _) => _ = CompareAsync()));
        toolbar.Children.Add(Button("Clear", (_, _) => ClearSelection()));
        toolbar.Children.Add(Button("Export CSV", (_, _) => ExportCsv()));
        toolbar.Children.Add(Button("Refresh wells", (_, _) => LoadWells()));

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        var left = new StackPanel();
        left.Children.Add(Caption("Wells (2-6)"));
        left.Children.Add(_wells);

        Grid.SetColumn(left, 0);
        Grid.SetColumn(_grid, 1);
        columns.Children.Add(left);
        columns.Children.Add(_grid);

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_notice);
        panel.Children.Add(_overlay);
        panel.Children.Add(columns);

        ShowContent(panel);

        LoadWells();
    }

    public OgComparisonMatrix? Matrix { get; private set; }

    public IReadOnlyList<OgCompareWell> Wells { get; private set; } = [];

    public string NoticeText => _notice.Text;

    public string OverlayText => _overlay.Text;

    public bool LastFailed { get; private set; }

    public string? ExportedPath { get; private set; }

    /// <summary>True while a comparison load is in flight and has not been superseded.</summary>
    public bool IsLoading => _loading;

    private bool _loading;

    public bool LoadWells()
    {
        var listed = _comparison.ListWellsAsync().GetAwaiter().GetResult();

        if (!listed.Success || listed.Value is null)
        {
            Wells = [];
            _wells.ItemsSource = null;
            LastFailed = true;
            _notice.Text = listed.Message ?? "The wells could not be listed.";
            ShowState(OgViewState.Failed, "The wells could not be listed.", listed.Reference);
            return false;
        }

        Wells = listed.Value;
        _wells.ItemsSource = Wells.Select(well => well.Name ?? well.Uwi).ToList();
        _notice.Text = Wells.Count == 0
            ? "There are no wells to compare in this database."
            : "Select two to six wells, then Compare.";
        _overlay.Text = "The schematic overlay is offered when every selected well has a depth and coordinates.";

        ShowState(Wells.Count == 0 ? OgViewState.Empty : OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-5: cancels a comparison that is still loading.</summary>
    public void SelectionChanged() => _gate.SelectionChanged();

    public void ClearSelection()
    {
        _wells.SelectedItems.Clear();
        Matrix = null;
        _grid.ItemsSource = null;
        _grid.Columns.Clear();
        _overlay.Text = "No comparison yet.";
        _notice.Text = "Select two to six wells, then Compare.";
    }

    /// <summary>Runs the comparison for the selected wells.</summary>
    public async Task<bool> CompareAsync()
    {
        var selected = _wells.SelectedItems
            .Cast<string>()
            .Select(SelectedUwi)
            .Where(uwi => !string.IsNullOrWhiteSpace(uwi))
            .ToList();

        var (token, version) = _gate.BeginLoad();
        _loading = true;

        var compared = await _comparison.CompareAsync(selected, token).ConfigureAwait(true);

        // A newer selection has superseded this result.
        if (!_gate.IsCurrent(version))
        {
            return false;
        }

        _loading = false;

        if (!compared.Success || compared.Value is null)
        {
            Matrix = null;
            LastFailed = true;
            _grid.ItemsSource = null;
            _grid.Columns.Clear();
            _overlay.Text = "No overlay: there is no comparison.";
            _notice.Text = compared.Message ?? "The wells could not be compared.";
            ShowState(OgViewState.Failed, "The wells could not be compared.", compared.Reference);
            return false;
        }

        LastFailed = false;
        Matrix = compared.Value;
        Render(Matrix);

        _notice.Text = $"{Matrix.Rows.Count} property row(s) across {Matrix.Wells.Count} well(s).";
        _overlay.Text = Matrix.CanOverlay
            ? "Schematic overlay available: every selected well has a depth with its unit and surface coordinates."
            : Matrix.OverlayReason ?? "Overlay unavailable.";

        ShowState(OgViewState.Ready, _notice.Text);
        return true;
    }

    /// <summary>AC-3: writes exactly the matrix that is displayed.</summary>
    public bool ExportCsv()
    {
        if (Matrix is null)
        {
            _notice.Text = "There is no comparison to export.";
            return false;
        }

        try
        {
            Directory.CreateDirectory(OgPaths.ExportsDirectory);

            var path = Path.Combine(
                OgPaths.ExportsDirectory,
                $"well-comparison-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

            File.WriteAllText(path, Matrix.ToCsv());
            ExportedPath = path;
            _notice.Text = $"{Matrix.Rows.Count} row(s) across {Matrix.Wells.Count} well(s) exported to {path}.";
            _application.AuditLog.Add("well.compare.export", "comparison", string.Join("+", Matrix.Wells.Select(w => w.Uwi)), "ok", Path.GetFileName(path));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "exporting the well comparison",
                "no file was written",
                FailureSeverity.Error);

            _notice.Text = $"The comparison could not be exported. Reference: {reference}";
            return false;
        }
    }

    private void Render(OgComparisonMatrix matrix)
    {
        _grid.Columns.Clear();
        _grid.Columns.Add(Text("Property", nameof(OgComparisonRow.Label), 150));
        _grid.Columns.Add(Text("Category", nameof(OgComparisonRow.Category), 100));
        _grid.Columns.Add(Text("Unit", nameof(OgComparisonRow.Unit), 70));

        for (var index = 0; index < matrix.Wells.Count; index++)
        {
            var well = matrix.Wells[index];
            _grid.Columns.Add(Text(well.Name ?? well.Uwi, $"Values[{index}].Text", 130));
        }

        _grid.Columns.Add(Text("Difference", nameof(OgComparisonRow.DifferenceText), 220));
        _grid.ItemsSource = matrix.Rows;
    }

    private string SelectedUwi(string display)
    {
        var well = Wells.FirstOrDefault(candidate =>
            string.Equals(candidate.Name ?? candidate.Uwi, display, StringComparison.Ordinal));

        return well?.Uwi ?? string.Empty;
    }

    private static DataGridTextColumn Text(string header, string path, double width) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(width),
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

    private static TextBlock Caption(string text) => new()
    {
        Text = text,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 6, 0),
        FontSize = 11.5,
    };
}
