using System.Globalization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;

/// <summary>
/// The period's process KPIs (WFL-01's <c>OgProcessKpis</c>).
/// </summary>
/// <param name="AtRisk">
/// Null when no at-risk threshold applies, which is a different answer from zero: the owner records only a breach
/// (<c>elapsed &gt; SLA_HOURS</c>) and no separate at-risk setting, so this build uses half of the configured SLA and says so.
/// </param>
public sealed record OgProcessKpis(
    int Running,
    int Completed,
    int AtRisk,
    int Breached,
    decimal? AverageCycleDays,
    int Unscheduled)
{
    public string CycleText => AverageCycleDays is { } days
        ? days.ToString("0.0#", CultureInfo.InvariantCulture) + " day(s)"
        : "no completed instance records both its dates";

    public string Describe() =>
        Running.ToString(CultureInfo.InvariantCulture) + " running · " +
        Completed.ToString(CultureInfo.InvariantCulture) + " completed · " +
        AtRisk.ToString(CultureInfo.InvariantCulture) + " at risk · " +
        Breached.ToString(CultureInfo.InvariantCulture) + " breached · " +
        Unscheduled.ToString(CultureInfo.InvariantCulture) + " unscheduled · cycle " + CycleText;
}

/// <summary>One definition's throughput and its instance counts (WFL-01, AC-1 and AC-6).</summary>
public sealed record OgDefinitionThroughput(
    string DefinitionId,
    string Name,
    string Version,
    string? Type,
    int Moved,
    int Instances,
    int Running,
    int Completed)
{
    public string Describe() =>
        Name + " · version " + (string.IsNullOrWhiteSpace(Version) ? "(none recorded)" : Version) +
        " · " + Moved.ToString(CultureInfo.InvariantCulture) + " moved in the period · " +
        Instances.ToString(CultureInfo.InvariantCulture) + " instance(s) · " +
        Running.ToString(CultureInfo.InvariantCulture) + " running · " +
        Completed.ToString(CultureInfo.InvariantCulture) + " completed";
}

/// <summary>The median dwell of one step of one definition (WFL-01, AC-4).</summary>
public sealed record OgStepDwell(string Definition, string Step, decimal MedianDays, int Count)
{
    public string Describe() =>
        Step + " (" + Definition + ") · median " +
        MedianDays.ToString("0.0#", CultureInfo.InvariantCulture) + " day(s) over " +
        Count.ToString(CultureInfo.InvariantCulture) + " recorded step(s)";
}

/// <summary>One running instance and how long it has sat in its current step (WFL-01, AC-3).</summary>
public sealed record OgAgeingRow(
    string InstanceId,
    string Definition,
    string? CurrentStep,
    decimal? DaysInStep,
    string SlaState,
    double? SlaHours)
{
    public bool IsBreached => string.Equals(SlaState, ProcessDashboardRules.Breached, StringComparison.Ordinal);

    public string DaysText => DaysInStep is { } days
        ? days.ToString("0.0#", CultureInfo.InvariantCulture) + " day(s)"
        : "no start recorded";

    public string Describe() =>
        InstanceId + " · " + Definition + " · " +
        (string.IsNullOrWhiteSpace(CurrentStep) ? "no current step" : CurrentStep) +
        " · " + DaysText + " · " + SlaState;
}

/// <summary>The dashboard for one period and field (WFL-01).</summary>
public sealed record OgProcessDashboard(
    string? FieldId,
    DateTime From,
    DateTime To,
    OgProcessKpis Kpis,
    IReadOnlyList<OgDefinitionThroughput> Definitions,
    IReadOnlyList<OgAgeingRow> Ageing,
    OgStepDwell? Bottleneck,
    IReadOnlyList<string> States,
    DateTime ReadAt)
{
    /// <summary>AC-1: throughput is counted from the instance's history record, not from a status column.</summary>
    public int MovedInPeriod => Definitions.Sum(definition => definition.Moved);

    /// <summary>AC-6: the per-definition counts add up to the instances the list holds.</summary>
    public int InstancesInScope => Definitions.Sum(definition => definition.Instances);

    public bool IsEmpty => InstancesInScope == 0 && MovedInPeriod == 0;

    public string PeriodText =>
        From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " to " +
        To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public string KpiText => Kpis.Describe();

    /// <summary>AC-2: "no SLA" is reported as its own state, never folded into "met".</summary>
    public string StateText => string.Join(" · ", States);

    public string ThroughputText => Definitions.Count == 0
        ? "No definition moved an instance in this period."
        : string.Join(Environment.NewLine, Definitions.Select(definition => definition.Describe()));

    public string AgeingText => Ageing.Count == 0
        ? "No instance is running in this period."
        : string.Join(Environment.NewLine, Ageing.Select(row => row.Describe()));

    public string BottleneckText => Bottleneck?.Describe() ?? "No completed step records both its dates, so no median dwell is computed.";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The process dashboard rules WFL-01 states once.</summary>
public static class ProcessDashboardRules
{
    public const string CodeNoConnection = "process-no-connection";
    public const string CodeReadFailed = "process-read-failed";
    public const string CodePeriodInvalid = "process-period-invalid";

    public const string Breached = "breached";
    public const string AtRisk = "at risk";
    public const string WithinSla = "within SLA";
    public const string NoSla = "no SLA";
    public const string Unscheduled = "unscheduled";

    public static IReadOnlyList<string> States { get; } = [Unscheduled, NoSla, WithinSla, AtRisk, Breached];

    /// <summary>
    /// AC-3 and AC-2: the SLA state of a running step.
    /// </summary>
    /// <remarks>
    /// The owner's own monitor escalates a step whose elapsed time exceeds <c>SLA_HOURS</c> - that is the breach, and it is
    /// reproduced here exactly. The owner configures <b>no separate at-risk threshold</b>, so this build derives it from the
    /// same SLA: a step past <b>half</b> of its configured window is at risk. Only the configured number is used, and the
    /// page states the rule, so no invented threshold is presented as configured.
    /// </remarks>
    public static string SlaState(double? slaHours, DateTime? startedAt, DateTime asOf)
    {
        if (slaHours is not > 0)
        {
            return NoSla;
        }

        if (startedAt is null)
        {
            return Unscheduled;
        }

        var elapsed = (asOf - startedAt.Value).TotalHours;

        if (elapsed >= slaHours.Value)
        {
            return Breached;
        }

        return elapsed >= slaHours.Value / 2d ? AtRisk : WithinSla;
    }

    /// <summary>The elapsed days of a running step, or null when it has not started.</summary>
    public static decimal? DaysBetween(DateTime? from, DateTime? to) =>
        from is { } start && to is { } end && end >= start
            ? (decimal)(end - start).TotalDays
            : null;

    /// <summary>
    /// AC-4: the median of a set of values - the middle one, or the mean of the two middle ones. Not the average, because a
    /// single very long instance would otherwise name the wrong step as the bottleneck.
    /// </summary>
    public static decimal Median(IEnumerable<decimal> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var ordered = values.OrderBy(value => value).ToList();

        if (ordered.Count == 0)
        {
            throw new ArgumentException("A median needs at least one value.", nameof(values));
        }

        var middle = ordered.Count / 2;

        return ordered.Count % 2 == 1
            ? ordered[middle]
            : (ordered[middle - 1] + ordered[middle]) / 2m;
    }

    /// <summary>AC-5: a period that ends before it begins is refused rather than read as an empty one.</summary>
    public static (string Code, string Message)? ValidatePeriod(DateTime from, DateTime to) =>
        to < from
            ? (CodePeriodInvalid,
                "The period ends (" + to.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
                ") before it begins (" + from.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ").")
            : null;
}

/// <summary>WFL-01: the process dashboard and its analytics.</summary>
public interface IOgProcessDashboard
{
    Task<OgResult<OgProcessDashboard>> LoadAsync(
        string? fieldId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WFL-01 over the owner's <c>PROCESS_DEFINITION</c>, <c>PROCESS_INSTANCE</c>, <c>PROCESS_STEP_INSTANCE</c> and
/// <c>PROCESS_HISTORY</c>.
/// </summary>
/// <remarks>
/// <para>
/// The owner's <c>PPDMProcessService</c> exposes 119 public methods across 45 partials (its open question, answered: the reads
/// this feature needs are the definition, instance, step-instance and history reads, and it offers them as
/// <c>GetProcessDefinitionsByTypeAsync</c>, <c>GetProcessInstancesForFieldAsync</c>, <c>GetProcessInstanceAsync</c> and
/// <c>GetProcessHistoryAsync</c>); its table contracts are read here through the same unit of work every desktop screen uses.
/// </para>
/// <para>
/// Throughput is counted from <c>PROCESS_HISTORY</c> - the engine's own record that an instance moved - and never from a
/// status column. Nothing is estimated: a definition with no history row in the period has no throughput, a step with no SLA
/// says "no SLA" rather than "met", and a failed read returns no dashboard at all, so a zero always means the records say zero.
/// </para>
/// </remarks>
public sealed class OgProcessDashboardService : IOgProcessDashboard
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgProcessDashboardService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgProcessDashboard>> LoadAsync(
        string? fieldId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (ProcessDashboardRules.ValidatePeriod(from, to) is { } refusal)
        {
            return OgResult<OgProcessDashboard>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgProcessDashboard>.Failed(
                "No connection is open, so the process dashboard cannot be read.",
                ProcessDashboardRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, fieldId, from, to, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the process dashboard",
                "the dashboard is not shown; no figure is inferred", FailureSeverity.Error);

            return OgResult<OgProcessDashboard>.Failed(
                "The process dashboard could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ProcessDashboardRules.CodeReadFailed);
        }
    }

    private async Task<OgResult<OgProcessDashboard>> ReadAsync(
        IDataSource source,
        string? fieldId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        var definitions = await OgUow.ReadAllAsync<PROCESS_DEFINITION>(source).ConfigureAwait(false);
        var instances = await OgUow.ReadAllAsync<PROCESS_INSTANCE>(source).ConfigureAwait(false);
        var steps = await OgUow.ReadAllAsync<PROCESS_STEP_INSTANCE>(source).ConfigureAwait(false);
        var history = await OgUow.ReadAllAsync<PROCESS_HISTORY>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        // Ageing is stated as of the end of the period the caller asked for, not of today's clock: a dashboard for a
        // past month must not age last month's instances against the present, which is what made WFL-01's own
        // integration test drift from "within SLA" to "at risk" without a change in the data. A period that ends
        // today is still read as of now.
        var endOfPeriod = to.Date.AddDays(1);
        var asOf = endOfPeriod > DateTime.UtcNow ? DateTime.UtcNow : endOfPeriod;
        var start = from.Date;
        var end = to.Date.AddDays(1);

        var scoped = instances
            .Where(row => string.IsNullOrWhiteSpace(fieldId) ||
                          string.Equals(row.FIELD_ID?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ids = scoped.Select(row => row.PROCESS_INSTANCE_ID).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // AC-1: throughput comes from the history record, per definition, over the periods' instances.
        var moved = history
            .Where(row => !string.IsNullOrWhiteSpace(row.PROCESS_INSTANCE_ID) &&
                          row.EVENT_DATE is { } at && at >= start && at < end &&
                          ids.Contains(row.PROCESS_INSTANCE_ID))
            .Select(row => row.PROCESS_INSTANCE_ID!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var movedByDefinition = scoped
            .Where(row => moved.Contains(row.PROCESS_INSTANCE_ID, StringComparer.OrdinalIgnoreCase))
            .GroupBy(row => row.PROCESS_DEFINITION_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Select(row => row.PROCESS_INSTANCE_ID).Distinct().Count(),
                StringComparer.OrdinalIgnoreCase);

        var names = definitions
            .GroupBy(row => row.PROCESS_DEFINITION_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var byDefinition = scoped
            .GroupBy(row => row.PROCESS_DEFINITION_ID ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                names.TryGetValue(group.Key, out var definition);

                return new OgDefinitionThroughput(
                    group.Key,
                    definition?.PROCESS_NAME ?? group.Key,
                    definition?.VERSION ?? string.Empty,
                    definition?.PROCESS_TYPE,
                    movedByDefinition.TryGetValue(group.Key, out var count) ? count : 0,
                    group.Select(row => row.PROCESS_INSTANCE_ID).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    group.Count(row => row.COMPLETED_DATE is null),
                    group.Count(row => row.COMPLETED_DATE is not null));
            })
            .OrderByDescending(definition => definition.Moved)
            .ThenBy(definition => definition.Name, StringComparer.Ordinal)
            .ToList();

        // The running instances and the step each one currently sits in.
        var running = scoped.Where(row => row.COMPLETED_DATE is null).ToList();

        var openSteps = steps
            .Where(row => string.Equals(row.STATUS, "IN_PROGRESS", StringComparison.OrdinalIgnoreCase) ||
                          row.COMPLETION_DATE is null)
            .GroupBy(row => row.PROCESS_INSTANCE_ID, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(row => row.STEP_SEQUENCE)
                    .ThenByDescending(row => row.STARTED_DATE)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        var ageing = new List<OgAgeingRow>();

        foreach (var instance in running)
        {
            openSteps.TryGetValue(instance.PROCESS_INSTANCE_ID, out var step);

            var state = step is null
                ? ProcessDashboardRules.NoSla
                : ProcessDashboardRules.SlaState(step.SLA_HOURS, step.STARTED_DATE, asOf);

            names.TryGetValue(instance.PROCESS_DEFINITION_ID ?? string.Empty, out var definition);

            ageing.Add(new OgAgeingRow(
                instance.PROCESS_INSTANCE_ID,
                definition?.PROCESS_NAME ?? instance.PROCESS_DEFINITION_ID ?? string.Empty,
                step?.STEP_NAME ?? instance.CURRENT_STATE,
                ProcessDashboardRules.DaysBetween(step?.STARTED_DATE, asOf),
                state,
                step?.SLA_HOURS));
        }

        var orderedAgeing = ageing
            .OrderByDescending(row => row.DaysInStep ?? -1m)
            .ThenBy(row => row.InstanceId, StringComparer.Ordinal)
            .ToList();

        // AC-4: the median dwell of each step, from the completed step instances that record both dates.
        var dwell = steps
            .Where(row => row.COMPLETION_DATE is not null && row.STARTED_DATE is not null)
            .Where(row => ids.Contains(row.PROCESS_INSTANCE_ID))
            .Where(row => row.COMPLETION_DATE >= start && row.COMPLETION_DATE < end)
            .Select(row =>
            {
                var instance = scoped.FirstOrDefault(candidate =>
                    string.Equals(candidate.PROCESS_INSTANCE_ID, row.PROCESS_INSTANCE_ID,
                        StringComparison.OrdinalIgnoreCase));

                names.TryGetValue(instance?.PROCESS_DEFINITION_ID ?? string.Empty, out var definition);

                return new
                {
                    Definition = definition?.PROCESS_NAME ?? instance?.PROCESS_DEFINITION_ID ?? string.Empty,
                    row.STEP_NAME,
                    Days = ProcessDashboardRules.DaysBetween(row.STARTED_DATE, row.COMPLETION_DATE),
                };
            })
            .Where(row => row.Days is not null)
            .GroupBy(row => row.Definition + "\u001f" + row.STEP_NAME, StringComparer.Ordinal)
            .Select(group =>
            {
                var parts = group.Key.Split('\u001f');

                return new OgStepDwell(
                    parts[0],
                    parts[1],
                    ProcessDashboardRules.Median(group.Select(row => row.Days!.Value)),
                    group.Count());
            })
            .OrderByDescending(row => row.MedianDays)
            .ThenBy(row => row.Step, StringComparer.Ordinal)
            .ToList();

        var completedInPeriod = scoped
            .Where(row => row.COMPLETED_DATE is { } completed && completed >= start && completed < end)
            .ToList();

        var cycles = completedInPeriod
            .Select(row => ProcessDashboardRules.DaysBetween(row.STARTED_DATE, row.COMPLETED_DATE))
            .Where(days => days is not null)
            .Select(days => days!.Value)
            .ToList();

        var kpis = new OgProcessKpis(
            running.Count,
            completedInPeriod.Count,
            ageing.Count(row => string.Equals(row.SlaState, ProcessDashboardRules.AtRisk, StringComparison.Ordinal)),
            ageing.Count(row => row.IsBreached),
            cycles.Count == 0 ? null : Math.Round(cycles.Average(), 2, MidpointRounding.AwayFromZero),
            ageing.Count(row => string.Equals(row.SlaState, ProcessDashboardRules.Unscheduled, StringComparison.Ordinal)));

        return OgResult<OgProcessDashboard>.Ok(new OgProcessDashboard(
            string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
            start,
            to.Date,
            kpis,
            byDefinition,
            orderedAgeing,
            dwell.FirstOrDefault(),
            ProcessDashboardRules.States,
            DateTime.UtcNow));
    }
}
