using System.Diagnostics;
using System.Threading;
using System.Globalization;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.Performance;

/// <summary>One memory sample during a soak.</summary>
public sealed record OgSoakSample(TimeSpan At, double WorkingSetMegabytes, double ManagedMegabytes);

/// <summary>What a soak measured. <see cref="WithinBudget"/> is the REL-04 AC-5 verdict.</summary>
public sealed record OgSoakReport(
    TimeSpan Duration,
    TimeSpan SampleInterval,
    int Cycles,
    double StartMegabytes,
    double EndMegabytes,
    double PeakMegabytes,
    double GrowthMegabytes,
    double GrowthPerMinute,
    bool WithinBudget,
    IReadOnlyList<OgSoakSample> Samples,
    IReadOnlyList<string> Notes)
{
    public string Describe() =>
        $"{Duration.TotalMinutes:0.#} min · {Cycles} cycle(s) · start {StartMegabytes:0} MB · " +
        $"end {EndMegabytes:0} MB · peak {PeakMegabytes:0} MB · growth {GrowthMegabytes:+0;-0;0} MB " +
        $"({GrowthPerMinute:+0.0;-0.0;0.0} MB/min) · budget {OgSoakRunner.BudgetMegabytes:0} MB · " +
        (WithinBudget ? "within budget" : "OVER BUDGET");
}

/// <summary>
/// REL-04 AC-5: runs scripted typical use for a set time and records the working set, so "memory stays within
/// budget after 30 minutes" is a measurement rather than a hope.
/// </summary>
/// <remarks>
/// <para>
/// The scripted use is the application's own render and read paths — a 20 000-sample log frame, a schematic
/// frame, a chart, a paged grid read and an audit read — repeated. The surfaces are created once and re-rendered,
/// which is the shape a person produces by panning a log for half an hour.
/// </para>
/// <para>
/// The workload runs on the calling (UI) thread: WPF surfaces are not free-threaded, and a soak that moved them
/// to a pool thread would measure a different application.
/// </para>
/// </remarks>
public static class OgSoakRunner
{
    /// <summary>The AC-5 budget for the working set.</summary>
    public const double BudgetMegabytes = 800;

    /// <summary>Runs scripted use for <paramref name="duration"/> and returns the measurement.</summary>
    public static OgSoakReport Run(
        OgApplication application,
        TimeSpan duration,
        TimeSpan? sampleEvery = null,
        CancellationToken cancellationToken = default,
        Action<int>? cycleCompleted = null)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "A soak needs a positive duration.");
        }

        var interval = sampleEvery ?? TimeSpan.FromMinutes(1);

        if (interval <= TimeSpan.Zero)
        {
            interval = TimeSpan.FromSeconds(1);
        }

        var notes = new List<string>();
        var samples = new List<OgSoakSample>();

        // The workload, built once so the measurement is about repeated use rather than repeated setup.
        var logModel = OgBenchmarkData.SyntheticLog(20_000);
        var logDocument = WellLogDocuments.FromModel(logModel);
        var logView = new WellLogView { Document = logDocument };

        using var logRenderer = new WellTools.Hosting.ViewSurfaceRenderer { Surface = logView };

        var chart = new WellTools.Reporting.ChartSurface
        {
            Renderer = application.Charts,
            Request = new global::Beep.OilandGas.Models.Data.Rendering.RenderChartRequest
            {
                ChartType = "line",
                Width = 640,
                Height = 360,
                ChartData = new Dictionary<string, object>
                {
                    ["labels"] = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                    ["series"] = new List<WellTools.Reporting.OgChartSeries>
                    {
                        new("Oil", [120, 132, 128, 141, 139, 152]),
                        new("Gas", [80, 86, 92, 91, 97, 103]),
                    },
                },
            },
        };

        using var chartRenderer = new WellTools.Hosting.ViewSurfaceRenderer { Surface = chart };

        var gridRows = Enumerable.Range(0, 5_000)
            .Select(index => new OgBenchmarkRunner.OgBenchmarkRow(index, $"Row {index}", index * 1.5))
            .ToList();

        // A WPF page needs an STA thread. The console verb and the smoke run have one; a test may not, and
        // then the soak runs the other five workloads and says the schematic was left out.
        Views.WellSchematicPageView? schematic = null;

        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
        {
            schematic = new Views.WellSchematicPageView(application);
            schematic.Load();
        }
        else
        {
            notes.Add("The schematic frame was not measured: constructing a page needs an STA thread.");
        }

        var clock = Stopwatch.StartNew();
        var nextSample = TimeSpan.Zero;
        var cycles = 0;

        void Sample() => samples.Add(new OgSoakSample(
            clock.Elapsed,
            Environment.WorkingSet / 1024d / 1024d,
            GC.GetTotalMemory(forceFullCollection: false) / 1024d / 1024d));

        notes.Add(
            "Scripted use: a 20 000-sample log frame, a schematic frame, a chart, a paged grid read and an audit " +
            "read, repeated with a 50 ms pause between cycles. Surfaces are created once and re-rendered.");

        Sample();

        while (clock.Elapsed < duration)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // A cancelled soak is a shorter measurement, not a crash: the samples taken still describe the
                // run, and the note says it was stopped early.
                notes.Add($"The soak was stopped early at {clock.Elapsed.TotalMinutes:0.##} min.");
                break;
            }

            logRenderer.Render(900, 600);
            schematic?.RenderFrame(600, 800);
            chartRenderer.Render(640, 360);
            _ = OgPager.Read(gridRows, OgPageRequest.First(200, OgPageRequest.DefaultCeiling));
            _ = application.Audit.Recent(200);

            cycles++;
            cycleCompleted?.Invoke(cycles);

            if (clock.Elapsed >= nextSample)
            {
                Sample();
                nextSample = clock.Elapsed + interval;
            }

            Thread.Sleep(50);
        }

        Sample();

        return OgSoakMath.Summarise(duration, interval, cycles, samples, notes);
    }

    /// <summary>Runs the soak and writes the report where the diagnostics page and the evidence expect it.</summary>
    public static (OgSoakReport Report, string Path) RunAndSave(
        OgApplication application,
        TimeSpan duration,
        TimeSpan? sampleEvery = null,
        CancellationToken cancellationToken = default)
    {
        var report = Run(application, duration, sampleEvery, cancellationToken);

        Directory.CreateDirectory(Path.GetDirectoryName(OgPaths.SoakReportPath)!);
        File.WriteAllText(OgPaths.SoakReportPath, OgSoakMath.ToJson(report));

        return (report, OgPaths.SoakReportPath);
    }


}

/// <summary>The pure arithmetic of a soak report, kept separate so it can be tested without waiting for one.</summary>
public static class OgSoakMath
{
    public static OgSoakReport Summarise(
        TimeSpan duration,
        TimeSpan interval,
        int cycles,
        IReadOnlyList<OgSoakSample> samples,
        IReadOnlyList<string>? notes = null)
    {
        ArgumentNullException.ThrowIfNull(samples);

        var collected = notes?.ToList() ?? [];

        if (samples.Count == 0)
        {
            return new OgSoakReport(
                duration, interval, cycles, 0, 0, 0, 0, 0, false, [],
                [.. collected, "No memory samples were taken; the soak proves nothing."]);
        }

        var start = samples[0].WorkingSetMegabytes;
        var end = samples[^1].WorkingSetMegabytes;
        var peak = samples.Max(sample => sample.WorkingSetMegabytes);
        var growth = end - start;
        var minutes = Math.Max(1.0 / 60.0, duration.TotalMinutes);

        return new OgSoakReport(
            duration,
            interval,
            cycles,
            start,
            end,
            peak,
            growth,
            growth / minutes,
            peak <= OgSoakRunner.BudgetMegabytes,
            samples,
            collected);
    }

    /// <summary>The report as JSON (invariant numbers, one sample per entry).</summary>
    public static string ToJson(OgSoakReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var samples = report.Samples.Select(sample => new
        {
            atSeconds = Math.Round(sample.At.TotalSeconds, 1),
            workingSetMb = Math.Round(sample.WorkingSetMegabytes, 1),
            managedMb = Math.Round(sample.ManagedMegabytes, 1),
        });

        var payload = new
        {
            schema = 1,
            durationMinutes = Math.Round(report.Duration.TotalMinutes, 3),
            sampleIntervalSeconds = Math.Round(report.SampleInterval.TotalSeconds, 1),
            cycles = report.Cycles,
            startMb = Math.Round(report.StartMegabytes, 1),
            endMb = Math.Round(report.EndMegabytes, 1),
            peakMb = Math.Round(report.PeakMegabytes, 1),
            growthMb = Math.Round(report.GrowthMegabytes, 1),
            growthMbPerMinute = Math.Round(report.GrowthPerMinute, 2),
            budgetMb = OgSoakRunner.BudgetMegabytes,
            withinBudget = report.WithinBudget,
            samples,
            notes = report.Notes,
        };

        return System.Text.Json.JsonSerializer.Serialize(
            payload,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
