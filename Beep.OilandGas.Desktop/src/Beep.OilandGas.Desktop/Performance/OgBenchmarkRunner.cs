using System.Diagnostics;
using System.Threading;
using System.Globalization;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Performance;

/// <summary>
/// REL-04: measures the budgets this build can measure, on the machine it is running on, and produces the
/// report the diagnostics page and <c>--report-benchmark</c> share.
/// </summary>
/// <remarks>
/// Every measurement is a median of three frames after one warm-up, because the first frame pays JIT and font
/// costs that a person panning a log does not. Operations that do not exist yet are left unmeasured by
/// <see cref="OgBudgets"/> rather than invented, and the report says why.
/// </remarks>
public static class OgBenchmarkRunner
{
    private const int Frames = 3;

    /// <summary>Runs every measurable budget. Safe to call from the UI thread; it does not show windows.</summary>
    public static OgBenchmarkReport Run(OgApplication application, bool includeShell = true)
    {
        ArgumentNullException.ThrowIfNull(application);

        var measured = new Dictionary<string, (double Milliseconds, string? Note)>(StringComparer.OrdinalIgnoreCase);
        var notes = new List<string>();

        // Startup: a mark the process made on the way in, not a number measured now.
        // The gate mark is "start to sign-in prompt" in a windowed run; a headless run returns before the gate,
        // so it reports the application build and says which number it is.
        var reachedGate = OgStartupTimeline.At("gate") is not null;
        var startup = OgStartupTimeline.At("gate") ?? OgStartupTimeline.At("bootstrap");

        if (startup is { } value)
        {
            measured["startup"] = (
                value,
                reachedGate
                    ? "from the first desktop code (OgBootstrapper.Create) to the startup gate decision"
                    : "application build only: this run returned before the gate (headless), so the sign-in prompt was not reached");
        }
        else
        {
            notes.Add("startup was not marked in this process, so it is not reported.");
        }

        // WPF windows and pages require an STA thread. A headless verb and the smoke run have one; a console
        // caller or a test may not, and that is a measurement this run cannot make rather than a failure.
        var sta = Thread.CurrentThread.GetApartmentState() == ApartmentState.STA;

        if (includeShell && sta)
        {
            measured["shell"] = MeasureShell(application);
        }
        else
        {
            notes.Add("shell was not measured: constructing a window needs an STA thread.");
        }

        measured["grid-page"] = MeasureGridPage();
        measured["log-20k"] = MeasureLog(20_000);
        measured["log-100k"] = MeasureLog(100_000);

        if (sta)
        {
            var schematic = MeasureSchematic(application);

            if (schematic is { } frame)
            {
                measured["schematic"] = frame;
            }
            else
            {
                notes.Add("The schematic could not be drawn for the benchmark; the frame is not reported.");
            }
        }
        else
        {
            notes.Add("schematic was not measured: constructing the page needs an STA thread.");
        }

        var latency = OgCancellationProbe.MeasureLatency();
        notes.Add(
            double.IsPositiveInfinity(latency)
                ? "The cancellation probe never observed the cancel."
                : $"Cancel latency: {latency:0.0} ms after the request (budget {OgCancellationProbe.BudgetMilliseconds:0} ms). A single synchronous render cannot be interrupted mid-frame.");

        notes.Add(
            $"Process working set now: {Environment.WorkingSet / 1024d / 1024d:0} MB. " +
            "The 30-minute memory budget is not claimed from a single run.");

        notes.Add("Budgets are REL-04 proposals; the hardware line above is what the numbers belong to.");

        return OgBenchmarkReport.Build(measured, notes);
    }

    /// <summary>Trims the report for storage: the same object the page and the verb read.</summary>
    public static string Save(OgBenchmarkReport report, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(report);

        var target = path ?? OgPaths.BenchmarkReportPath;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(target))!);
        File.WriteAllText(target, report.ToJson());

        return target;
    }

    /// <summary>Reads the last stored report, or null when none has been written.</summary>
    public static OgBenchmarkReport? LoadLast(string? path = null)
    {
        var target = path ?? OgPaths.BenchmarkReportPath;

        return File.Exists(target) ? OgBenchmarkReport.FromJson(File.ReadAllText(target)) : null;
    }

    /// <summary>Seeds the deterministic inputs the benchmark uses, so a run needs no customer data.</summary>
    public static string SeedData(int samples = 100_000)
    {
        Directory.CreateDirectory(OgPaths.BenchmarkDirectory);

        var model = OgBenchmarkData.SyntheticLog(samples);
        var path = Path.Combine(OgPaths.BenchmarkDirectory, $"log-{samples}.csv");

        using var writer = new StreamWriter(path);

        writer.WriteLine("DEPT,GR");

        var set = model.LogSets[0];

        for (var i = 0; i < set.Index.Length; i++)
        {
            writer.WriteLine(
                $"{set.Index[i].ToString("0.0", CultureInfo.InvariantCulture)}," +
                $"{(set.Curves[0].Values[i] ?? 0).ToString("0.###", CultureInfo.InvariantCulture)}");
        }

        return path;
    }

    private static (double Milliseconds, string? Note) MeasureShell(OgApplication application)
    {
        var clock = Stopwatch.StartNew();
        var shell = new Shell.OgShellWindow(application, onClosed: () => { });

        // Loading is part of "shell with navigation": it builds the navigation and selects the landing page.
        shell.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.FrameworkElement.LoadedEvent));
        clock.Stop();

        var pages = shell.Landing.Route;
        shell.Close();

        return (clock.Elapsed.TotalMilliseconds, $"navigation built and landed on '{pages}'");
    }

    private static (double Milliseconds, string? Note) MeasureGridPage()
    {
        // 5 000 rows is above any page; the pager must touch only one page and stay under the ceiling.
        var rows = Enumerable.Range(0, 5_000)
            .Select(index => new OgBenchmarkRow(index, $"Row {index}", index * 1.5))
            .ToList();

        var clock = Stopwatch.StartNew();
        var page = OgPager.Read(rows, OgPageRequest.First(OgPageRequest.DefaultPageSize, OgPageRequest.DefaultCeiling));
        clock.Stop();

        return (
            clock.Elapsed.TotalMilliseconds,
            $"{page.Items.Count} of {page.Total} rows, ceiling {page.Ceiling}; message={(page.Message is null ? "none" : "shown")}");
    }

    private static (double Milliseconds, string? Note) MeasureLog(int samples)
    {
        var model = OgBenchmarkData.SyntheticLog(samples);
        var document = WellLogDocuments.FromModel(model);
        var view = new WellLogView { Document = document };

        using var renderer = new WellTools.Hosting.ViewSurfaceRenderer { Surface = view };

        // One warm-up frame: JIT, font cache and the first SKSurface allocation are not a panning cost.
        renderer.Render(900, 600);

        var times = new List<double>(Frames);

        for (var frame = 0; frame < Frames; frame++)
        {
            var clock = Stopwatch.StartNew();
            renderer.Render(900, 600);
            clock.Stop();
            times.Add(clock.Elapsed.TotalMilliseconds);
        }

        times.Sort();
        var plan = OgBenchmarkData.Plan(samples, plotHeight: 600);

        return (
            times[times.Count / 2],
            $"median of {Frames} frames at 900x600; {plan.Note ?? "no decimation"}");
    }

    private static (double Milliseconds, string? Note)? MeasureSchematic(OgApplication application)
    {
        if (ViewFor(application) is not { } page)
        {
            return null;
        }

        page.Load();

        // One warm-up frame, then the median of three.
        if (!page.RenderFrame(600, 800))
        {
            return null;
        }

        var times = new List<double>(Frames);

        for (var frame = 0; frame < Frames; frame++)
        {
            if (!page.RenderFrame(600, 800))
            {
                return null;
            }

            times.Add(page.LastFrameMilliseconds);
        }

        times.Sort();
        return (times[times.Count / 2], $"median of {Frames} frames at 600x800");
    }

    private static Views.WellSchematicPageView? ViewFor(OgApplication application)
    {
        try
        {
            return new Views.WellSchematicPageView(application);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // A schematic that cannot be built is a benchmark gap, reported by the caller, not a crash.
            application.Diagnostics.ReportHandled(
                ex,
                "building the schematic for the benchmark",
                "the schematic budget is not measured",
                FailureSeverity.Degraded);

            return null;
        }
    }

    /// <summary>A row of the synthetic table the grid budget is measured over.</summary>
    public sealed record OgBenchmarkRow(int Id, string Name, double Value);
}
