using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Performance;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-04: the budgets are complete and comparable, the report round-trips, a grid read is bounded and states
/// its ceiling, long curves are thinned with a stated ratio, and a cancel is observed inside the budget.
/// </summary>
public sealed class PerformanceTests
{
    [Fact]
    public void The_budget_list_is_unique_ordered_and_includes_every_section_4_operation()
    {
        var keys = OgBudgets.All.Select(budget => budget.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(11, keys.Count);
        Assert.Equal(OgBudgets.Schema, OgBenchmarkReport.Build(new Dictionary<string, (double, string?)>()).SchemaVersion);

        foreach (var key in new[] { "startup", "shell", "well-search", "grid-page", "log-20k", "log-100k", "schematic", "contour", "scene-3d", "module-setup", "memory" })
        {
            Assert.True(OgBudgets.Find(key) is not null, $"Budget '{key}' is missing.");
        }

        // A budget this build cannot measure says why; it is not silently dropped.
        Assert.All(
            OgBudgets.All.Where(budget => !budget.Available),
            budget => Assert.False(string.IsNullOrWhiteSpace(budget.Why)));
    }

    [Fact]
    public void A_report_keeps_unmeasured_budgets_and_marks_only_measured_overruns()
    {
        var report = OgBenchmarkReport.Build(new Dictionary<string, (double, string?)>
        {
            ["log-20k"] = (120, "median of 3"),
            ["grid-page"] = (900, "over"),
        });

        Assert.Equal(OgBudgets.All.Count, report.Measurements.Count);
        Assert.Equal(2, report.MeasuredCount);
        Assert.Equal(1, report.OverBudgetCount);
        Assert.False(report.Passed);

        var log = Assert.Single(report.Measurements, row => row.Key == "log-20k");
        Assert.True(log.Passed);
        Assert.Equal(0.48, log.Ratio!.Value, 2);
        Assert.Contains("PASS", log.Describe(), StringComparison.OrdinalIgnoreCase);

        var grid = Assert.Single(report.Measurements, row => row.Key == "grid-page");
        Assert.False(grid.Passed);
        Assert.Contains("OVER", grid.Describe(), StringComparison.Ordinal);

        var search = Assert.Single(report.Measurements, row => row.Key == "well-search");
        Assert.False(search.Measured);
        Assert.Null(search.Passed);
        Assert.Contains("DAT-02", search.Note!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_report_round_trips_and_two_runs_compare_by_key_and_target()
    {
        var measured = new Dictionary<string, (double, string?)> { ["log-20k"] = (100, "a") };

        var first = OgBenchmarkReport.Build(measured, capturedAt: DateTimeOffset.UnixEpoch);
        var second = OgBenchmarkReport.Build(measured, capturedAt: DateTimeOffset.UnixEpoch.AddHours(1));

        var reloaded = OgBenchmarkReport.FromJson(first.ToJson());

        Assert.Equal(first.Measurements.Count, reloaded.Measurements.Count);

        // Comparable: the same keys, in the same order, with the same targets.
        Assert.Equal(
            first.Measurements.Select(row => (row.Key, row.TargetMilliseconds)),
            reloaded.Measurements.Select(row => (row.Key, row.TargetMilliseconds)));

        Assert.Equal(
            first.Measurements.Select(row => row.Key),
            second.Measurements.Select(row => row.Key));

        Assert.NotEqual(first.CapturedAtUtc, second.CapturedAtUtc);
    }

    [Fact]
    public void A_value_measured_for_an_operation_that_is_not_budgeted_is_reported_as_a_note()
    {
        var report = OgBenchmarkReport.Build(new Dictionary<string, (double, string?)>
        {
            ["chart"] = (42, null),
        });

        Assert.Contains(report.Notes, note => note.Contains("chart", StringComparison.Ordinal));
    }

    [Fact]
    public void A_page_read_returns_one_page_and_never_the_whole_source()
    {
        var rows = Enumerable.Range(0, 5_000).ToList();

        var page = OgPager.Read(rows, OgPageRequest.First(200, 20_000));

        Assert.Equal(200, page.Items.Count);
        Assert.Equal(5_000, page.Total);
        Assert.Equal(0, page.PageIndex);
        Assert.False(page.CeilingReached);
        Assert.Null(page.Message);
        Assert.True(page.HasMore);

        var second = OgPager.Read(rows, new OgPageRequest(2, 200, 20_000));
        Assert.Equal(400, second.Items[0]);
        Assert.Equal(200, second.Items.Count);
    }

    [Fact]
    public void A_query_above_the_ceiling_says_so_and_asks_the_person_to_narrow_it()
    {
        var rows = Enumerable.Range(0, 5_000).ToList();

        var page = OgPager.Read(rows, OgPageRequest.First(200, 1_000));

        Assert.True(page.CeilingReached);
        Assert.Contains("1,000", page.Message!, StringComparison.Ordinal);
        Assert.Contains("Narrow", page.Message!, StringComparison.Ordinal);
        Assert.Equal(200, page.Items.Count);
    }

    [Fact]
    public void A_store_backed_grid_states_when_it_shows_only_its_most_recent_rows()
    {
        // AC-3: a bounded read says what it is not showing, and what to do about it.
        Assert.Null(OgPager.RecentCeiling(120, 120));

        var message = OgPager.RecentCeiling(200, 4_321);

        Assert.NotNull(message);
        Assert.Contains("200", message!, StringComparison.Ordinal);
        Assert.Contains("4,321", message!, StringComparison.Ordinal);
        Assert.Contains("Narrow", message!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_nonsense_page_request_is_normalised_rather_than_throwing()
    {
        var normalised = new OgPageRequest(-3, 0, 0).Normalised();

        Assert.Equal(0, normalised.PageIndex);
        Assert.Equal(OgPageRequest.DefaultPageSize, normalised.PageSize);
        Assert.Equal(OgPageRequest.DefaultCeiling, normalised.Ceiling);
    }

    [Fact]
    public void Decimation_applies_above_the_threshold_and_states_the_ratio()
    {
        var big = OgBenchmarkData.Plan(100_000, OgBenchmarkData.DisplaySampleThreshold, plotHeight: 600);
        var small = OgBenchmarkData.Plan(20_000, OgBenchmarkData.DisplaySampleThreshold, plotHeight: 600);

        // AC-2
        Assert.True(big.Applied);
        Assert.Equal(2_400, big.Bound);
        Assert.Equal(100_000d / 2_400d, big.Ratio, 2);
        Assert.Contains("Decimated", big.Note!, StringComparison.Ordinal);
        Assert.Contains("41.7:1", big.Note!, StringComparison.Ordinal);

        // The threshold is "above", not "at".
        Assert.False(small.Applied);
        Assert.Null(small.Note);
    }

    [Fact]
    public void The_renderers_thinning_stays_within_the_bound_and_keeps_the_ends_and_the_peak()
    {
        var model = OgBenchmarkData.SyntheticLog(100_000);
        var points = OgBenchmarkData.Points(model, plotHeight: 600);
        var thinned = OgBenchmarkData.Decimate(points);

        var plan = OgBenchmarkData.Plan(points.Count, plotHeight: 600);

        // Four points per pixel row is the renderer's own bound.
        Assert.True(thinned.Count <= plan.Bound, $"{thinned.Count} points exceeds the bound of {plan.Bound}.");
        Assert.True(thinned.Count > 0);

        // The polyline must still start and end where the curve does.
        Assert.Equal(points[0], thinned[0]);
        Assert.Equal(points[^1], thinned[^1]);

        // And the peak must survive: that is the reason for keeping leftmost/rightmost per row.
        var peak = points.MaxBy(point => point.x).x;
        Assert.Contains(thinned, point => Math.Abs(point.x - peak) < 1e-6);
    }

    [Fact]
    public void A_cancellable_operation_stops_within_the_budget()
    {
        var latency = OgCancellationProbe.MeasureLatency();

        // AC-4
        Assert.True(
            latency < OgCancellationProbe.BudgetMilliseconds,
            $"Cancel was observed after {latency:0.0} ms, over the {OgCancellationProbe.BudgetMilliseconds:0} ms budget.");
    }

    [Fact]
    public void A_report_saved_to_disk_is_the_report_read_back()
    {
        var path = Path.Combine(OgPaths.BenchmarkDirectory, $"report-{Guid.NewGuid():n}"[..20] + ".json");
        var report = OgBenchmarkReport.Build(new Dictionary<string, (double, string?)> { ["log-20k"] = (99, "test") });

        var written = OgBenchmarkRunner.Save(report, path);
        var read = OgBenchmarkRunner.LoadLast(path);

        Assert.True(File.Exists(written));
        Assert.NotNull(read);
        Assert.Equal(report.Measurements.Count, read!.Measurements.Count);
        Assert.Equal(report.MeasuredCount, read.MeasuredCount);

        File.Delete(written);
    }

    [Fact]
    public void Seeding_writes_one_row_per_sample_plus_the_header()
    {
        var path = OgBenchmarkRunner.SeedData(samples: 1_000);
        var lines = File.ReadAllLines(path);

        Assert.Equal(1_001, lines.Length);
        Assert.Equal("DEPT,GR", lines[0]);
        Assert.Contains(",", lines[1], StringComparison.Ordinal);

        File.Delete(path);
    }

    [Fact]
    public void The_startup_timeline_records_marks_relative_to_the_first_desktop_code()
    {
        OgStartupTimeline.Mark("test-mark");

        Assert.NotNull(OgStartupTimeline.At("test-mark"));
        Assert.Contains("test-mark", OgStartupTimeline.Describe(), StringComparison.Ordinal);
        Assert.Null(OgStartupTimeline.At("never-marked"));
    }

    [Fact]
    public void The_performance_page_is_declared_as_an_administration_page()
    {
        var page = OgModuleCatalog.Find("performance");

        Assert.NotNull(page);
        Assert.Equal(OgModuleKeys.Administration, page!.ModuleKey);
        Assert.Contains(OgRoleNames.SystemAdmin, page.RequiredRoles);
    }
}
