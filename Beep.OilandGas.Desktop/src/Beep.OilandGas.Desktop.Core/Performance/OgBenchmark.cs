using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>One performance budget from REL-04 §4.</summary>
/// <param name="Key">Stable identifier used by the report and by comparisons between runs.</param>
/// <param name="Operation">What is measured, in words.</param>
/// <param name="TargetMilliseconds">The budget, in milliseconds.</param>
/// <param name="Available">
/// False when this build has nothing to measure for the budget — a feature that does not exist yet, or a
/// measurement that needs a live database. The report says so rather than omitting the row.
/// </param>
/// <param name="Why">Why an unavailable budget cannot be measured here.</param>
public sealed record OgBudget(
    string Key,
    string Operation,
    double TargetMilliseconds,
    bool Available = true,
    string? Why = null);

/// <summary>One measured (or unmeasurable) budget in a report.</summary>
public sealed record OgMeasurement(
    string Key,
    string Operation,
    double TargetMilliseconds,
    double? MeasuredMilliseconds,
    string? Note = null)
{
    /// <summary>True when a value was measured; an unmeasured budget is neither a pass nor a failure.</summary>
    [JsonIgnore]
    public bool Measured => MeasuredMilliseconds is not null;

    [JsonIgnore]
    public bool? Passed => MeasuredMilliseconds is { } measured ? measured <= TargetMilliseconds : null;

    [JsonIgnore]
    public double? Ratio => MeasuredMilliseconds is { } measured && TargetMilliseconds > 0
        ? measured / TargetMilliseconds
        : null;

    /// <summary>One line for a console table or the diagnostics page.</summary>
    public string Describe() =>
        MeasuredMilliseconds is { } measured
            ? $"{Key,-26} {measured,9:0.0} ms  of {TargetMilliseconds,8:0} ms  {Ratio,5:0.00}x  {(Passed == true ? "pass" : "OVER")}"
            : $"{Key,-26} {"—",9}     of {TargetMilliseconds,8:0} ms         not measured";
}

/// <summary>
/// REL-04: the budgets this build commits to, with the two facts a report needs to be comparable — a stable key
/// and an unchanged target.
/// </summary>
public static class OgBudgets
{
    public const int Schema = 1;

    /// <summary>Every budget in REL-04 §4, including the ones this build cannot measure yet.</summary>
    public static IReadOnlyList<OgBudget> All { get; } =
    [
        new("startup", "Application start to sign-in prompt", 3_000),
        new("shell", "Sign-in to shell with navigation", 2_000),
        new("well-search", "Well search over 5 000 wells", 1_000, Available: false,
            Why: "There is no well table until DAT-02; the search would be over a list this build does not have."),
        new("grid-page", "Table workbench first page (200 rows)", 800),
        new("log-20k", "Log render, 20 000 samples", 250),
        new("log-100k", "Log render, 100 000 samples (decimated)", 400),
        new("schematic", "Schematic render", 200),
        new("contour", "Contour map, 10 000 cells", 300, Available: false,
            Why: "No contour map exists in this build (INT-07)."),
        new("scene-3d", "3D orbit, 10 000 cells", 500, Available: false,
            Why: "No 3D scene exists in this build (INT-08)."),
        new("module-setup", "Module setup of 29 modules on an empty SQLite database", 60_000, Available: false,
            Why: "Module registration is DAT-02; the desktop host does not assemble the 29 modules."),
        new("memory", "Memory after 30 minutes of typical use", 800, Available: false,
            Why: "A 30-minute soak is not run by the benchmark; the report records the current process instead."),
    ];

    public static OgBudget? Find(string key) =>
        All.FirstOrDefault(budget => string.Equals(budget.Key, key, StringComparison.OrdinalIgnoreCase));
}

/// <summary>A description of the machine a report came from, so two runs can be compared honestly.</summary>
public sealed record OgMachine(
    string OperatingSystem,
    string Architecture,
    int ProcessorCount,
    double TotalMemoryMegabytes,
    string Framework,
    string ProcessArchitecture)
{
    public static OgMachine Capture()
    {
        // GetGCMemoryInfo is a managed call and does not need a guard; a machine that reports 0 is reported
        // as 0 rather than guessed at.
        var total = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024d / 1024d;

        return new OgMachine(
            OperatingSystem: RuntimeInformation.OSDescription,
            Architecture: RuntimeInformation.OSArchitecture.ToString(),
            ProcessorCount: Environment.ProcessorCount,
            TotalMemoryMegabytes: Math.Round(Math.Max(0, total), 0),
            Framework: RuntimeInformation.FrameworkDescription,
            ProcessArchitecture: RuntimeInformation.ProcessArchitecture.ToString());
    }

    public string Describe() =>
        $"{OperatingSystem} ({Architecture}, {ProcessorCount} cores, {TotalMemoryMegabytes:0} MB visible) · " +
        $"{Framework} · {ProcessArchitecture}";
}

/// <summary>
/// REL-04 AC-6: the machine-readable benchmark report. Comparable across runs because the measurements are
/// keyed and ordered by <see cref="OgBudgets.All"/>, so a diff between two files shows only values.
/// </summary>
public sealed record OgBenchmarkReport(
    int SchemaVersion,
    DateTimeOffset CapturedAtUtc,
    OgMachine Machine,
    IReadOnlyList<OgMeasurement> Measurements,
    IReadOnlyList<string> Notes)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };

    public int MeasuredCount => Measurements.Count(measurement => measurement.Measured);

    public int OverBudgetCount => Measurements.Count(measurement => measurement.Passed == false);

    public bool Passed => OverBudgetCount == 0;

    public string ToJson() => JsonSerializer.Serialize(this, Json);

    /// <summary>Reads a report back; a malformed file throws and the caller reports it.</summary>
    public static OgBenchmarkReport FromJson(string json) =>
        JsonSerializer.Deserialize<OgBenchmarkReport>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new FormatException("The benchmark report is empty.");

    /// <summary>The console table: one line per budget, ordered as the budget list.</summary>
    public string Describe()
    {
        var lines = new List<string>
        {
            $"benchmark schema {SchemaVersion} · {CapturedAtUtc:yyyy-MM-ddTHH:mm:ssZ}",
            $"machine: {Machine.Describe()}",
        };

        lines.AddRange(Measurements.Select(measurement => measurement.Describe()));

        lines.Add(Passed
            ? $"result: {MeasuredCount} measured, all within budget"
            : $"result: {OverBudgetCount} over budget of {MeasuredCount} measured");

        lines.AddRange(Notes.Select(note => $"note: {note}"));

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Builds a report from the budget list, so a budget without a value still appears. Anything measured that
    /// is not in the budget list is kept as a note rather than silently dropped.
    /// </summary>
    public static OgBenchmarkReport Build(
        IReadOnlyDictionary<string, (double Milliseconds, string? Note)> measured,
        IReadOnlyList<string>? notes = null,
        DateTimeOffset? capturedAt = null,
        OgMachine? machine = null)
    {
        ArgumentNullException.ThrowIfNull(measured);

        var rows = new List<OgMeasurement>();
        var extra = new List<string>(notes ?? []);

        foreach (var budget in OgBudgets.All)
        {
            if (measured.TryGetValue(budget.Key, out var value))
            {
                rows.Add(new OgMeasurement(budget.Key, budget.Operation, budget.TargetMilliseconds, value.Milliseconds, value.Note));
            }
            else
            {
                rows.Add(new OgMeasurement(
                    budget.Key,
                    budget.Operation,
                    budget.TargetMilliseconds,
                    MeasuredMilliseconds: null,
                    budget.Available ? "not measured in this run" : budget.Why));
            }
        }

        foreach (var (key, value) in measured)
        {
            if (OgBudgets.Find(key) is null)
            {
                extra.Add($"{key} measured at {value.Milliseconds:0.0} ms (not a budgeted operation)");
            }
        }

        return new OgBenchmarkReport(
            OgBudgets.Schema,
            capturedAt ?? DateTimeOffset.UtcNow,
            machine ?? OgMachine.Capture(),
            rows,
            extra);
    }
}

/// <summary>One page request, bounded by a ceiling so a grid can never ask for an unbounded result.</summary>
/// <param name="PageIndex">Zero-based page.</param>
/// <param name="PageSize">Rows per page.</param>
/// <param name="Ceiling">The most rows a query may materialise before the grid asks the person to narrow it.</param>
public sealed record OgPageRequest(int PageIndex, int PageSize, int Ceiling)
{
    public const int DefaultPageSize = 200;
    public const int DefaultCeiling = 20_000;

    public static OgPageRequest First(int pageSize = DefaultPageSize, int ceiling = DefaultCeiling) =>
        new(0, pageSize, ceiling);

    /// <summary>Validates and normalises: a caller cannot ask for page size 0 or a negative page.</summary>
    public OgPageRequest Normalised() =>
        new(
            PageIndex: Math.Max(0, PageIndex),
            PageSize: Math.Clamp(PageSize <= 0 ? DefaultPageSize : PageSize, 1, 5_000),
            Ceiling: Math.Clamp(Ceiling <= 0 ? DefaultCeiling : Ceiling, 1, 1_000_000));
}

/// <summary>One page of rows, plus what the grid must tell the person when the ceiling was met.</summary>
public sealed record OgPage<T>(
    IReadOnlyList<T> Items,
    int PageIndex,
    int PageSize,
    int Total,
    int Ceiling)
{
    public bool HasMore => (PageIndex + 1) * PageSize < Total;

    public bool CeilingReached => Total > Ceiling;

    /// <summary>The sentence a grid shows when it is showing less than the query matched (AC-3).</summary>
    public string? Message => CeilingReached
        ? $"Showing {Items.Count} of the first {Ceiling:N0} rows ({Total:N0} match). Narrow the search to see the rest."
        : null;
}

/// <summary>
/// REL-04 AC-3: the only way this build materialises a grid page. Reading a page never returns more than
/// <see cref="OgPageRequest.PageSize"/> rows, and a source larger than the ceiling is reported so the grid can
/// ask the person to narrow the query instead of loading everything.
/// </summary>
public static class OgPager
{
    /// <summary>
    /// The sentence a store-backed grid shows when it can only offer its most recent rows (AC-3): what is shown,
    /// what exists, and the action that reveals the rest. Null when everything fits.
    /// </summary>
    public static string? RecentCeiling(int shown, int total) =>
        total > shown
            ? $"Showing the {shown:N0} most recent rows of {total:N0}. Narrow the range to see older ones."
            : null;

    public static OgPage<T> Read<T>(IReadOnlyList<T> source, OgPageRequest request)
    {
        ArgumentNullException.ThrowIfNull(source);

        var bounded = request.Normalised();
        var offset = (long)bounded.PageIndex * bounded.PageSize;

        // The ceiling bounds work, not just the returned rows: a query over a huge source is refused the
        // moment it exceeds the budget instead of paging through it.
        var limit = Math.Min(source.Count, bounded.Ceiling);
        var items = offset >= limit
            ? []
            : source
                .Skip((int)Math.Min(offset, int.MaxValue))
                .Take((int)Math.Min(bounded.PageSize, limit - offset))
                .ToList();

        return new OgPage<T>(items, bounded.PageIndex, bounded.PageSize, source.Count, bounded.Ceiling);
    }
}
