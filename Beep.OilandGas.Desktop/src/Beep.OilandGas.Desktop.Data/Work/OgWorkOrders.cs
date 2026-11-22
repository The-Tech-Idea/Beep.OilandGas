using System.Globalization;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Work;

/// <summary>One work order as the dashboard shows it (WRK-01's <c>OgWorkOrderSummary</c>).</summary>
/// <param name="Priority">
/// The owner records urgency as <c>RUSH_IND</c>, not a priority scale, so this is <c>RUSH</c> or <c>NORMAL</c> and says so.
/// </param>
/// <param name="State">The latest recorded state of the order's controlled workflow, or empty when none is recorded.</param>
public sealed record OgWorkOrderSummary(
    string Id,
    string Number,
    string Kind,
    string? FieldId,
    string? AssetId,
    string Priority,
    string State,
    string? Assignee,
    DateTime? PlannedStart,
    DateTime? PlannedEnd,
    DateTime? CompletedAt,
    bool Overdue)
{
    public bool IsComplete => string.Equals(State, WorkOrderState.Completed, StringComparison.OrdinalIgnoreCase) ||
                              string.Equals(State, WorkOrderState.Cancelled, StringComparison.OrdinalIgnoreCase);

    public bool IsUnassigned => string.IsNullOrWhiteSpace(Assignee);

    public bool AwaitingApproval =>
        string.Equals(State, WorkOrderState.UnderReview, StringComparison.OrdinalIgnoreCase);

    public string StateText => string.IsNullOrWhiteSpace(State) ? "no recorded state" : WorkState.Label(State);

    /// <summary>AC-3: the planned end is the owner's due date, and "overdue" is computed against the as-of date.</summary>
    public string DueText => PlannedEnd is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + (Overdue ? " (overdue)" : string.Empty)
        : "no planned end";

    public string CostText => "not recorded";

    public string Describe() =>
        Number + " · " + Kind + " · " + StateText + " · " + Priority.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(AssetId) ? string.Empty : " · " + AssetId) +
        (string.IsNullOrWhiteSpace(FieldId) ? string.Empty : " · field " + FieldId) +
        (IsUnassigned ? " · unassigned" : " · " + Assignee) +
        " · due " + DueText;
}

/// <summary>One queue of the dashboard (WRK-01, AC-1).</summary>
public sealed record OgWorkQueue(string Key, string Title, int Count, string Detail)
{
    public string Describe() => Title + " " + Count.ToString(CultureInfo.InvariantCulture);
}

/// <summary>Two orders whose planned spans overlap for the same assignee (WRK-01, AC-2).</summary>
public sealed record OgWorkConflict(string Assignee, string First, string Second, DateTime From, DateTime To)
{
    public string Describe() =>
        Assignee + " is planned on " + First + " and " + Second + " between " +
        From.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " and " +
        To.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

/// <summary>One day of the work calendar (WRK-01, AC-4).</summary>
public sealed record OgWorkDay(DateTime Date, IReadOnlyList<string> Orders)
{
    public int Count => Orders.Count;

    public string Describe() =>
        Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " · " +
        Count.ToString(CultureInfo.InvariantCulture) + " order(s)" +
        (Count == 0 ? string.Empty : ": " + string.Join(", ", Orders));
}

/// <summary>The work-order dashboard for one field and filter (WRK-01).</summary>
public sealed record OgWorkOrderWorkspace(
    string? FieldId,
    string User,
    string? StateFilter,
    string? KindFilter,
    IReadOnlyList<OgWorkOrderSummary> Orders,
    IReadOnlyList<OgWorkQueue> Queues,
    IReadOnlyList<OgWorkConflict> Conflicts,
    IReadOnlyList<OgWorkDay> Calendar,
    DateTime AsOf,
    DateTime ReadAt)
{
    public int Total => Orders.Count;

    public bool IsEmpty => Orders.Count == 0;

    /// <summary>AC-1: every queue is a filter over these same orders, so its count is the grid's total for that filter.</summary>
    public IReadOnlyList<OgWorkOrderSummary> In(string queueKey) => queueKey switch
    {
        WorkRules.QueueMine => Orders.Where(order => !order.IsUnassigned &&
                                                      string.Equals(order.Assignee, User, StringComparison.OrdinalIgnoreCase))
            .ToList(),
        WorkRules.QueueUnassigned => Orders.Where(order => order.IsUnassigned).ToList(),
        WorkRules.QueueOverdue => Orders.Where(order => order.Overdue).ToList(),
        WorkRules.QueueApproval => Orders.Where(order => order.AwaitingApproval).ToList(),
        _ => Orders.ToList(),
    };

    public string CountsText => string.Join(" · ", Queues.Select(queue => queue.Describe()));

    public string GridText => Orders.Count == 0
        ? "no work in this view"
        : string.Join(Environment.NewLine, Orders.Select(order => order.Describe()));

    public string ConflictText => Conflicts.Count == 0
        ? "No resource conflict in this view."
        : string.Join(Environment.NewLine, Conflicts.Select(conflict => conflict.Describe()));

    public string CalendarText => Calendar.Count == 0
        ? "No planned work in this view."
        : string.Join(Environment.NewLine, Calendar.Where(day => day.Count > 0).Select(day => day.Describe()));

    /// <summary>
    /// AC-4: the calendar holds the same orders the grid lists, counted once each rather than once per day they span - so the
    /// two totals are comparable.
    /// </summary>
    public int CalendarOrders => Calendar
        .SelectMany(day => day.Orders)
        .Distinct(StringComparer.Ordinal)
        .Count();

    public string RefreshText =>
        "last refreshed " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        (IsEmpty ? " · no work in this view" : " · " + Total.ToString(CultureInfo.InvariantCulture) + " order(s)");
}

/// <summary>The work-order rules WRK-01 states once.</summary>
public static class WorkRules
{
    public const string CodeNoConnection = "work-no-connection";
    public const string CodeReadFailed = "work-read-failed";
    public const string CodeUserRequired = "work-user-required";

    public const string QueueMine = "mine";
    public const string QueueUnassigned = "unassigned";
    public const string QueueOverdue = "overdue";
    public const string QueueApproval = "approval";
    public const string QueueAll = "all";

    /// <summary>
    /// AC-3: overdue is computed from the planned end, not stored - and only for work that is not already complete or
    /// cancelled, because a finished order cannot be late.
    /// </summary>
    public static bool IsOverdue(DateTime? plannedEnd, bool isComplete, DateTime asOf) =>
        !isComplete && plannedEnd is { } due && due.Date < asOf.Date;

    /// <summary>AC-2: two orders of one assignee conflict when their planned spans overlap.</summary>
    public static IReadOnlyList<OgWorkConflict> ConflictsOf(IEnumerable<OgWorkOrderSummary> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var conflicts = new List<OgWorkConflict>();

        foreach (var group in orders
                     .Where(order => !order.IsUnassigned && !order.IsComplete &&
                                     order.PlannedStart is not null && order.PlannedEnd is not null)
                     .GroupBy(order => order.Assignee!, StringComparer.OrdinalIgnoreCase))
        {
            var spans = group
                .OrderBy(order => order.PlannedStart)
                .ThenBy(order => order.PlannedEnd)
                .ToList();

            for (var first = 0; first < spans.Count; first++)
            {
                for (var second = first + 1; second < spans.Count; second++)
                {
                    var a = spans[first];
                    var b = spans[second];

                    if (b.PlannedStart!.Value.Date > a.PlannedEnd!.Value.Date)
                    {
                        // The spans are ordered by start, so nothing later can overlap this one either.
                        break;
                    }

                    conflicts.Add(new OgWorkConflict(
                        group.Key,
                        a.Number,
                        b.Number,
                        a.PlannedStart.Value.Date > b.PlannedStart.Value.Date
                            ? a.PlannedStart.Value.Date
                            : b.PlannedStart.Value.Date,
                        a.PlannedEnd.Value.Date < b.PlannedEnd.Value.Date
                            ? a.PlannedEnd.Value.Date
                            : b.PlannedEnd.Value.Date));
                }
            }
        }

        return conflicts;
    }

    /// <summary>AC-4: the calendar groups the same orders by the days their planned span covers.</summary>
    public static IReadOnlyList<OgWorkDay> CalendarOf(IEnumerable<OgWorkOrderSummary> orders)
    {
        ArgumentNullException.ThrowIfNull(orders);

        var days = new SortedDictionary<DateTime, List<string>>();

        foreach (var order in orders)
        {
            if (order.PlannedStart is not { } start)
            {
                continue;
            }

            var end = order.PlannedEnd is { } planned ? planned.Date : start.Date;

            if (end < start.Date)
            {
                end = start.Date;
            }

            // A span longer than a year is clamped, so a mis-keyed date cannot flood the calendar.
            for (var day = start.Date; day <= end && day < start.Date.AddYears(1); day = day.AddDays(1))
            {
                if (!days.TryGetValue(day, out var list))
                {
                    list = [];
                    days[day] = list;
                }

                list.Add(order.Number);
            }
        }

        return days.Select(pair => new OgWorkDay(pair.Key, pair.Value)).ToList();
    }

    /// <summary>The state's name for a person, from the owner's own state vocabulary.</summary>
    public static string Label(string? state) => state?.Trim().ToUpperInvariant() switch
    {
        WorkOrderState.Draft => "draft",
        WorkOrderState.Scoped => "scoped",
        WorkOrderState.Planned => "planned",
        WorkOrderState.InProgress => "in progress",
        WorkOrderState.UnderReview => "awaiting approval",
        WorkOrderState.Completed => "completed",
        WorkOrderState.Cancelled => "cancelled",
        null or "" => "no recorded state",
        _ => state!.Trim().ToLowerInvariant(),
    };
}

/// <summary>A small alias so the record above and the rule read the same state names.</summary>
public static class WorkState
{
    public static string Label(string? state) => WorkRules.Label(state);
}

/// <summary>WRK-01: the work-order dashboard and calendar.</summary>
public interface IOgWorkOrders
{
    /// <param name="user">The signed-in person, so "mine" is their own orders and nothing else.</param>
    Task<OgResult<OgWorkOrderWorkspace>> LoadAsync(
        string? fieldId,
        string? stateFilter,
        string? kindFilter,
        string user,
        DateTime asOf,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// WRK-01 over the owner's <c>WORK_ORDER</c>, <c>WORK_ORDER_COMPONENT</c>, <c>R_WO_TYPE</c> and <c>PROJECT_STATUS</c>.
/// </summary>
/// <remarks>
/// <para>
/// The owner's work-order header carries the number, the type, the request and due dates, the rush flag and the instructions;
/// its assets and people are on <c>WORK_ORDER_COMPONENT</c>; and - as the owner's own <c>WorkOrderService</c> says - the
/// header <b>does not establish operational state</b>, so the state is the latest <c>PROJECT_STATUS</c> of the project the
/// component links (<c>PROJECT_ID</c>), which is the controlled execution workflow.
/// </para>
/// <para>
/// Nothing is invented for what the tables do not record: there is no priority scale (the rush flag is shown as the owner's
/// urgency marker), no cost column (WRK-02 rolls that up from labour and materials), and an order with no state row says so
/// rather than being shown as draft.
/// </para>
/// </remarks>
public sealed class OgWorkOrders : IOgWorkOrders
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgWorkOrders(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgWorkOrderWorkspace>> LoadAsync(
        string? fieldId,
        string? stateFilter,
        string? kindFilter,
        string user,
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgWorkOrderWorkspace>.Failed(
                "No connection is open, so the work-order dashboard cannot be read.", WorkRules.CodeNoConnection);
        }

        try
        {
            var orders = await OgUow.ReadAllAsync<WORK_ORDER>(source).ConfigureAwait(false);
            var components = await OgUow.ReadAllAsync<WORK_ORDER_COMPONENT>(source).ConfigureAwait(false);
            var types = await OgUow.ReadAllAsync<R_WO_TYPE>(source).ConfigureAwait(false);
            var statuses = await OgUow.ReadAllAsync<PROJECT_STATUS>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var typeNames = types
                .Where(row => !string.IsNullOrWhiteSpace(row.WORK_ORDER_TYPE))
                .GroupBy(row => row.WORK_ORDER_TYPE.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => string.IsNullOrWhiteSpace(group.First().LONG_NAME)
                        ? group.Key
                        : group.First().LONG_NAME!,
                    StringComparer.OrdinalIgnoreCase);

            var byOrder = components
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.IsNullOrWhiteSpace(row.WORK_ORDER_ID))
                .GroupBy(row => row.WORK_ORDER_ID, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            // The state of an order is the latest status of the project its component links - the owner's controlled
            // workflow, not a column on the header.
            var projectState = statuses
                .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                .Where(row => !string.IsNullOrWhiteSpace(row.PROJECT_ID))
                .GroupBy(row => row.PROJECT_ID!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(row => row.EFFECTIVE_DATE)
                        .ThenByDescending(row => row.ROW_CREATED_DATE)
                        .First().STATUS ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);

            var mapped = new List<OgWorkOrderSummary>();

            foreach (var order in orders
                         .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                         .Where(row => !string.IsNullOrWhiteSpace(row.WORK_ORDER_ID)))
            {
                var mine = byOrder.TryGetValue(order.WORK_ORDER_ID, out var list) ? list : [];

                var primary = mine.FirstOrDefault();

                var asset = primary is null
                    ? null
                    : First(primary.UWI, primary.FACILITY_ID, primary.EQUIPMENT_ID);

                var assignee = mine
                    .Select(row => First(row.EMPLOYEE_BA_ID, row.BUSINESS_ASSOCIATE_ID))
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

                var project = mine
                    .Select(row => row.PROJECT_ID)
                    .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

                var state = project is not null && projectState.TryGetValue(project, out var recorded)
                    ? recorded
                    : string.Empty;

                var summary = new OgWorkOrderSummary(
                    order.WORK_ORDER_ID,
                    string.IsNullOrWhiteSpace(order.WORK_ORDER_NUMBER) ? order.WORK_ORDER_ID : order.WORK_ORDER_NUMBER!,
                    typeNames.TryGetValue(order.WORK_ORDER_TYPE?.Trim() ?? string.Empty, out var kind)
                        ? kind
                        : order.WORK_ORDER_TYPE ?? "(no type)",
                    primary?.FIELD_ID,
                    asset,
                    string.Equals(order.RUSH_IND?.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "RUSH" : "NORMAL",
                    state,
                    assignee,
                    order.REQUEST_DATE,
                    order.DUE_DATE,
                    order.COMPLETE_DATE,
                    false);

                mapped.Add(summary with
                {
                    Overdue = WorkRules.IsOverdue(summary.PlannedEnd, summary.IsComplete, asOf),
                });
            }

            var scoped = mapped
                .Where(order => string.IsNullOrWhiteSpace(fieldId) ||
                                string.Equals(order.FieldId?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(order => string.IsNullOrWhiteSpace(stateFilter) ||
                                string.Equals(order.State, stateFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(order => string.IsNullOrWhiteSpace(kindFilter) ||
                                string.Equals(order.Kind, kindFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(order => order.PlannedEnd ?? DateTime.MaxValue)
                .ThenBy(order => order.Number, StringComparer.Ordinal)
                .ToList();

            var workspace = new OgWorkOrderWorkspace(
                fieldId,
                user ?? string.Empty,
                stateFilter,
                kindFilter,
                scoped,
                BuildQueues(scoped, user ?? string.Empty),
                WorkRules.ConflictsOf(scoped),
                WorkRules.CalendarOf(scoped),
                asOf,
                DateTime.UtcNow);

            return OgResult<OgWorkOrderWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the work-order dashboard",
                "the dashboard is not shown; no queue is inferred", FailureSeverity.Error);

            return OgResult<OgWorkOrderWorkspace>.Failed(
                "The work-order dashboard could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? WorkRules.CodeReadFailed);
        }
    }

    private static IReadOnlyList<OgWorkQueue> BuildQueues(
        IReadOnlyList<OgWorkOrderSummary> orders,
        string user) =>
    [
        new(WorkRules.QueueMine, "Mine",
            orders.Count(order => !order.IsUnassigned &&
                                  string.Equals(order.Assignee, user, StringComparison.OrdinalIgnoreCase)),
            "assigned to you and not complete"),
        new(WorkRules.QueueUnassigned, "Unassigned",
            orders.Count(order => order.IsUnassigned), "no person is recorded on the order"),
        new(WorkRules.QueueOverdue, "Overdue",
            orders.Count(order => order.Overdue), "planned end has passed and the order is not complete"),
        new(WorkRules.QueueApproval, "Awaiting approval",
            orders.Count(order => order.AwaitingApproval), "the recorded state is under review"),
        new(WorkRules.QueueAll, "All", orders.Count, "everything in this view"),
    ];

    private static string? First(params string?[] values) =>
        values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
}
