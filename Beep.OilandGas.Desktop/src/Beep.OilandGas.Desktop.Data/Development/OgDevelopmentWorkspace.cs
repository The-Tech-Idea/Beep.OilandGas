using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One plan state and how many plans carry it (DEV-01).</summary>
public sealed record OgDevelopmentCounts(string PlanState, int Count);

/// <summary>Spend to date in one currency, summed from dated cost rows (DEV-01).</summary>
public sealed record OgDevelopmentSpend(decimal Amount, string Currency)
{
    public string Text => Amount.ToString("#,0.###", CultureInfo.InvariantCulture) + " MM " + Currency;
}

/// <summary>One source page the development workspace links to; availability is the page gate (AC-3).</summary>
public sealed record OgDevelopmentLink(string Route, string Label, bool Available, string? WhyNot);

/// <summary>One attention item, derived from dates or from the missing approved program (AC-2).</summary>
public sealed record OgDevelopmentAttention(
    string Kind,
    string Subject,
    string Detail,
    string Route,
    bool Overdue)
{
    public const string PlanReview = "PLAN_REVIEW";
    public const string ProgramMissing = "PROGRAM_MISSING";

    public string Describe() => $"{Subject}: {Detail}";
}

/// <summary>The development landing: plans, wells, facilities, spend, sources and attention.</summary>
public sealed record OgDevelopmentSnapshot(
    string FieldName,
    IReadOnlyList<OgDevelopmentCounts> Plans,
    int WellsPlanned,
    int WellsDrilling,
    int FacilitiesInConstruction,
    IReadOnlyList<OgDevelopmentSpend> Spend,
    IReadOnlyList<OgDevelopmentLink> Links,
    IReadOnlyList<OgDevelopmentAttention> Attention,
    bool AttentionAvailable,
    string? AttentionNote,
    DateTime AsOf)
{
    public int PlanCount => Plans.Sum(plan => plan.Count);

    public bool HasActivity =>
        PlanCount > 0
        || WellsPlanned > 0
        || WellsDrilling > 0
        || FacilitiesInConstruction > 0;

    public string PlanText => Plans.Count == 0
        ? "no plan state recorded"
        : string.Join(
            " · ",
            Plans.Select(plan => $"{plan.PlanState} {plan.Count.ToString(CultureInfo.InvariantCulture)}"));

    public string SpendText => Spend.Count == 0
        ? "no spend recorded"
        : string.Join(" · ", Spend.Select(spend => spend.Text));

    public string KpiText =>
        $"{PlanCount.ToString(CultureInfo.InvariantCulture)} plan(s) · " +
        $"{WellsPlanned.ToString(CultureInfo.InvariantCulture)} well(s) planned · " +
        $"{WellsDrilling.ToString(CultureInfo.InvariantCulture)} drilling · " +
        $"{FacilitiesInConstruction.ToString(CultureInfo.InvariantCulture)} facilit(y/ies) in construction · " +
        SpendText;

    public string AttentionText => Attention.Count == 0
        ? (AttentionAvailable ? "Nothing needs attention." : AttentionNote ?? "Attention data is unavailable.")
        : string.Join(" · ", Attention.Select(item => item.Describe()));
}

/// <summary>DEV-01: the development persona's landing.</summary>
public interface IOgDevelopmentWorkspace
{
    Task<OgResult<OgDevelopmentSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The workspace over the DevelopmentPlanning-owned <c>FIELD_DEVELOPMENT_PLAN</c>, <c>DEVELOPMENT_WELL_SCHEDULE</c>,
/// <c>FACILITY_INVESTMENT</c> and <c>DEVELOPMENT_COSTS</c>, with drilling programs read for the attention strip.
/// </summary>
/// <remarks>
/// Every count is a count of rows; a failed read is a failed result, never zero (AC-1). Attention is derived:
/// a submitted plan past the review window and a scheduled well without an approved program (AC-2). Each source
/// link is decided by the page gate (AC-3). The strip is hidden when the program table cannot be read, rather than
/// shown empty.
/// </remarks>
public sealed class OgDevelopmentWorkspace : IOgDevelopmentWorkspace
{
    public const string CodeNoConnection = "development-workspace-no-connection";
    public const string CodeReadFailed = "development-workspace-read-failed";

    /// <summary>The review window after which an unapproved submitted plan becomes attention (AC-2).</summary>
    public const int ReviewWindowDays = 30;

    /// <summary>The source pages and the feature that adds a page this build does not have.</summary>
    public static IReadOnlyList<(string Route, string Label, string? ArrivesWith)> Sources { get; } =
    [
        ("fdpwizard", "Plan review", null),
        ("welldesign", "Well design", null),
        ("poolsfacilities", "Pools and facilities", null),
        ("drillingprogram", "Drilling programs", null),
        ("construction", "Construction progress", "DEV-08"),
        ("wells", "Well index", null),
    ];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgDevelopmentWorkspace(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgDevelopmentSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        DateTime asOf,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        var source = _source();

        if (source is null)
        {
            return OgResult<OgDevelopmentSnapshot>.Failed(
                "No connection is open, so the development workspace cannot be read.", CodeNoConnection);
        }

        try
        {
            var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);
            var schedules = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_SCHEDULE>(source).ConfigureAwait(false);
            var facilities = await OgUow.ReadAllAsync<FACILITY_INVESTMENT>(source).ConfigureAwait(false);
            var costs = await OgUow.ReadAllAsync<DEVELOPMENT_COSTS>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var scopedPlans = plans.Where(plan => FieldMatches(plan.FIELD_ID, fieldId, fieldName)).ToList();
            var scopedSchedules = schedules.Where(row => FieldMatches(row.FIELD_ID, fieldId, fieldName)).ToList();
            var scopedFacilities = facilities.Where(row => FieldMatches(row.FIELD_ID, fieldId, fieldName)).ToList();
            var scopedCosts = costs.Where(row => FieldMatches(row.FIELD_ID, fieldId, fieldName)).ToList();

            // The program table belongs to another module; when it is absent the strip is hidden, not shown empty.
            IReadOnlyList<DRILLING_PROGRAM> programs;
            var attentionAvailable = true;
            string? attentionNote = null;

            try
            {
                programs = await OgUow.ReadAllAsync<DRILLING_PROGRAM>(source).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
            {
                var reference = _failures.ReportHandled(
                    ex,
                    "reading the drilling programs for the development attention strip",
                    "the drilling-program attention items are hidden",
                    FailureSeverity.Degraded);

                programs = [];
                attentionAvailable = false;
                attentionNote = $"Drilling-program attention is unavailable ({reference ?? "no reference"}).";
            }

            var attention = PlanAttention(scopedPlans, asOf.Date).ToList();

            // When the program table cannot be read, the program half of the strip is hidden rather than
            // claiming every scheduled well has no approved program.
            if (attentionAvailable)
            {
                attention.AddRange(ProgramAttention(scopedSchedules, programs));
            }

            return OgResult<OgDevelopmentSnapshot>.Ok(new OgDevelopmentSnapshot(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                CountPlans(scopedPlans),
                scopedSchedules.Count(row => string.Equals(row.SCHEDULE_STATUS?.Trim(), "PLANNED", StringComparison.OrdinalIgnoreCase)),
                scopedSchedules.Count(row => string.Equals(row.SCHEDULE_STATUS?.Trim(), "DRILLING", StringComparison.OrdinalIgnoreCase)),
                scopedFacilities.Count(InConstruction(at: asOf.Date)),
                SumSpend(scopedCosts, asOf.Date),
                BuildLinks(canOpen),
                attention,
                attentionAvailable,
                attentionNote,
                asOf.Date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the development workspace",
                "the development workspace is not shown",
                FailureSeverity.Error);

            return OgResult<OgDevelopmentSnapshot>.Failed(
                "The development workspace could not be read.", reference ?? CodeReadFailed);
        }
    }

    /// <summary>AC-1: the plan counts, one row per recorded state.</summary>
    public static IReadOnlyList<OgDevelopmentCounts> CountPlans(IEnumerable<FIELD_DEVELOPMENT_PLAN> plans) =>
        plans
            .GroupBy(
                plan => string.IsNullOrWhiteSpace(plan.FDP_STATUS) ? "(unclassified)" : plan.FDP_STATUS.Trim().ToUpperInvariant(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgDevelopmentCounts(group.Key, group.Count()))
            .OrderBy(count => count.PlanState, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>AC-2: a submitted plan past the review window and still not approved.</summary>
    public static IReadOnlyList<OgDevelopmentAttention> PlanAttention(
        IEnumerable<FIELD_DEVELOPMENT_PLAN> plans,
        DateTime asOf,
        int reviewWindowDays = ReviewWindowDays) =>
        plans
            .Where(plan => plan.SUBMISSION_DATE is { } submitted
                           && !string.Equals(plan.FDP_STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase)
                           && submitted.Date <= asOf.Date.AddDays(-Math.Max(0, reviewWindowDays)))
            .Select(plan =>
            {
                var days = (asOf.Date - plan.SUBMISSION_DATE!.Value.Date).Days;

                return new OgDevelopmentAttention(
                    OgDevelopmentAttention.PlanReview,
                    string.IsNullOrWhiteSpace(plan.FDP_NAME) ? plan.FDP_ID ?? "(unnamed plan)" : plan.FDP_NAME,
                    $"submitted {days.ToString(CultureInfo.InvariantCulture)} d ago and still " +
                    $"{plan.FDP_STATUS ?? "unclassified"}",
                    "fdpwizard",
                    Overdue: true);
            })
            .OrderBy(item => item.Subject, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>AC-2: a scheduled well with no approved drilling program.</summary>
    public static IReadOnlyList<OgDevelopmentAttention> ProgramAttention(
        IEnumerable<DEVELOPMENT_WELL_SCHEDULE> schedules,
        IEnumerable<DRILLING_PROGRAM> programs)
    {
        var approved = programs
            .Where(program => string.Equals(program.PROGRAM_STATUS, "APPROVED", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return schedules
            .Where(schedule => !IsFinished(schedule.SCHEDULE_STATUS))
            .Where(schedule => !approved.Any(program => MatchesWell(program, schedule)))
            .Select(schedule => new OgDevelopmentAttention(
                OgDevelopmentAttention.ProgramMissing,
                string.IsNullOrWhiteSpace(schedule.WELL_NAME) ? schedule.PLANNED_WELL_ID ?? "(unnamed well)" : schedule.WELL_NAME,
                "no approved drilling program",
                "drillingprogram",
                Overdue: true))
            .OrderBy(item => item.Subject, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>AC-3: the links, each decided by the page's own gate.</summary>
    public static IReadOnlyList<OgDevelopmentLink> BuildLinks(Func<string, bool> canOpen)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        return Sources
            .Select(source =>
            {
                var available = canOpen(source.Route);

                return new OgDevelopmentLink(
                    source.Route,
                    source.Label,
                    available,
                    available
                        ? null
                        : source.ArrivesWith is { } feature
                            ? $"Not available in this build; the page arrives with {feature}."
                            : "Not available to this account.");
            })
            .ToList();
    }

    /// <summary>Spend to date: only cost rows dated on or before the as-of date, grouped by currency (AC-1).</summary>
    public static IReadOnlyList<OgDevelopmentSpend> SumSpend(
        IEnumerable<DEVELOPMENT_COSTS> costs,
        DateTime asOf) =>
        costs
            .Where(cost => cost.COST_DATE is null || cost.COST_DATE.Value.Date <= asOf.Date)
            .GroupBy(
                cost => string.IsNullOrWhiteSpace(cost.COST_CURRENCY) ? "(currency not recorded)" : cost.COST_CURRENCY.Trim().ToUpperInvariant(),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgDevelopmentSpend(
                group.Sum(CostAmount),
                group.Key))
            .OrderByDescending(spend => spend.Amount)
            .ToList();

    private static decimal CostAmount(DEVELOPMENT_COSTS cost) =>
        cost.TOTAL_DEVELOPMENT_COSTS
        ?? (cost.DEVELOPMENT_WELL_DRILLING_COSTS ?? 0m)
        + (cost.DEVELOPMENT_WELL_EQUIPMENT ?? 0m)
        + (cost.SUPPORT_EQUIPMENT_AND_FACILITIES ?? 0m)
        + (cost.SERVICE_WELL_COSTS ?? 0m);

    /// <summary>A facility is in construction when it has started and has not completed by the as-of date.</summary>
    private static Func<FACILITY_INVESTMENT, bool> InConstruction(DateTime at) =>
        facility => facility.PLANNED_START_DATE is { } start
                    && start.Date <= at.Date
                    && (facility.PLANNED_COMPLETION_DATE is null || facility.PLANNED_COMPLETION_DATE.Value.Date > at.Date);

    private static bool IsFinished(string? status) =>
        string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, "ABANDONED", StringComparison.OrdinalIgnoreCase);

    private static bool MatchesWell(DRILLING_PROGRAM program, DEVELOPMENT_WELL_SCHEDULE schedule) =>
        (!string.IsNullOrWhiteSpace(program.WELL_ID)
         && string.Equals(program.WELL_ID, schedule.PLANNED_WELL_ID, StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(program.PROGRAM_NAME)
            && string.Equals(program.PROGRAM_NAME, schedule.WELL_NAME, StringComparison.OrdinalIgnoreCase));

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));
}
