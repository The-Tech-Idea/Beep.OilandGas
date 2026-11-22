using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Performance;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Views;

/// <summary>One budget as the performance page shows it.</summary>
public sealed record OgPerformanceRow(
    string Key,
    string Operation,
    string Target,
    string Measured,
    string Ratio,
    string Verdict,
    string? Note)
{
    public bool OverBudget => Verdict == "OVER BUDGET";
}

/// <summary>
/// REL-04: the measured budgets, the machine they belong to, and a button to measure again.
/// </summary>
/// <remarks>
/// The page reads the report file rather than holding numbers in memory, so what a person sees after a restart
/// is what was written. An over-budget row is red <b>and</b> says "OVER BUDGET" in words — colour alone would
/// fail REL-03's rule.
/// </remarks>
public sealed class PerformancePageView : OgPageBase
{
    private readonly OgApplication _application;
    private readonly TextBlock _summary;
    private readonly TextBlock _notice;
    private readonly DataGrid _grid;

    public PerformancePageView(OgApplication application)
        : base(application, "performance")
    {
        _application = application;

        _summary = Body(string.Empty);
        _notice = Body(string.Empty);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsTabStop = true,
            Margin = new Thickness(0, 8, 0, 8),
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowStyle = OverBudgetRowStyle(),
            Columns =
            {
                Text("Budget", nameof(OgPerformanceRow.Operation), 300),
                Text("Target", nameof(OgPerformanceRow.Target), 100),
                Text("Measured", nameof(OgPerformanceRow.Measured), 100),
                Text("Of budget", nameof(OgPerformanceRow.Ratio), 90),
                Text("Verdict", nameof(OgPerformanceRow.Verdict), 110),
                Star("Note", nameof(OgPerformanceRow.Note)),
            },
        };

        Named(_grid, "Performance budgets");

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        toolbar.Children.Add(Button("Run benchmark", (_, _) => RunBenchmark()));
        toolbar.Children.Add(Button("Re-read report", (_, _) => Load()));

        var panel = new StackPanel();
        panel.Children.Add(toolbar);
        panel.Children.Add(_summary);
        panel.Children.Add(_notice);
        panel.Children.Add(_grid);

        ShowContent(panel);

        Load();
    }

    /// <summary>What the page is showing; the smoke run and tests read these.</summary>
    public IReadOnlyList<OgPerformanceRow> Rows { get; private set; } = [];

    public OgBenchmarkReport? Report { get; private set; }

    public string ReportPath => OgPaths.BenchmarkReportPath;

    public string SummaryText => _summary.Text;

    /// <summary>The stored report, if one exists; a missing report is a state, not a failure.</summary>
    public bool Load()
    {
        try
        {
            Report = OgBenchmarkRunner.LoadLast();

            if (Report is null)
            {
                Rows = OgBudgets.All
                    .Select(budget => new OgPerformanceRow(
                        budget.Key,
                        budget.Operation,
                        $"{budget.TargetMilliseconds:0} ms",
                        "—",
                        "—",
                        budget.Available ? "not measured" : "not applicable",
                        budget.Why ?? "Run the benchmark to measure this budget."))
                    .ToList();

                _summary.Text = "No benchmark has been run on this installation yet.";
                _notice.Text = $"Run the benchmark, or `--report-benchmark` from a console. Report path: {ReportPath}";
                _grid.ItemsSource = Rows;
                return false;
            }

            Rows = Report.Measurements
                .Select(measurement => new OgPerformanceRow(
                    measurement.Key,
                    measurement.Operation,
                    $"{measurement.TargetMilliseconds:0} ms",
                    measurement.MeasuredMilliseconds is { } measured ? $"{measured:0.0} ms" : "—",
                    measurement.Ratio is { } ratio ? $"{ratio:0.00}x" : "—",
                    measurement.Passed switch
                    {
                        true => "pass",
                        false => "OVER BUDGET",
                        null => "not measured",
                    },
                    measurement.Note))
                .ToList();

            _summary.Text =
                $"{Report.CapturedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm} · {Report.Machine.Describe()} · " +
                $"{Report.MeasuredCount} measured · {Report.OverBudgetCount} over budget";

            _notice.Text = Rows.Any(row => row.OverBudget)
                ? "A red row measured over its budget. The value is what this machine did, not an estimate."
                : string.Empty;

            _grid.ItemsSource = Rows;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or System.Text.Json.JsonException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "reading the benchmark report",
                "the page shows the budget list without measurements",
                FailureSeverity.Degraded);

            ShowState(OgViewState.Failed, "The benchmark report could not be read.", reference);
            return false;
        }
    }

    /// <summary>Measures now, stores the report, and refreshes the grid.</summary>
    public bool RunBenchmark()
    {
        try
        {
            var report = OgBenchmarkRunner.Run(_application);
            var path = OgBenchmarkRunner.Save(report);

            _notice.Text = report.Passed
                ? $"Measured {report.MeasuredCount} budget(s), all within budget. Written to {path}."
                : $"Measured {report.MeasuredCount} budget(s); {report.OverBudgetCount} over budget. Written to {path}.";

            return Load();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = _application.Diagnostics.ReportHandled(
                ex,
                "running the benchmark from the performance page",
                "no report was written",
                FailureSeverity.Error);

            ShowState(OgViewState.Failed, "The benchmark could not run.", reference);
            return false;
        }
    }

    private static Style OverBudgetRowStyle()
    {
        var style = new Style(typeof(DataGridRow));
        var over = new DataTrigger
        {
            Binding = new Binding(nameof(OgPerformanceRow.OverBudget)),
            Value = true,
        };

        over.Setters.Add(new Setter(Control.ForegroundProperty, new DynamicResourceExtension("BeepDangerBrush")));
        over.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.SemiBold));
        style.Triggers.Add(over);

        return style;
    }

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

    private static DataGridTextColumn Star(string header, string path) => new()
    {
        Header = header,
        Binding = new Binding(path),
        Width = new DataGridLength(1, DataGridLengthUnitType.Star),
    };
}
