using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-01: the queues as filters over the same orders, the computed overdue, the resource conflict, the calendar's agreement
/// with the grid and the unassigned/assigned distinction - the parts that need no database. The read over real rows is proven
/// in the gateway test.
/// </summary>
public sealed class WorkOrderDashboardTests
{
    private const string PageFileName = "WorkOrderDashboardPageView.cs";

    private static readonly DateTime AsOf = new(2026, 11, 8);

    [Fact]
    public void Every_queue_count_is_the_grid_it_filters()
    {
        // AC-1: a queue is a filter over the same orders, so its count cannot disagree with what it lists.
        var workspace = Workspace(
        [
            Order("WO-1", due: new DateTime(2026, 11, 5), assignee: "omar", state: "IN_PROGRESS"),
            Order("WO-2", due: new DateTime(2026, 11, 20), assignee: null, state: "PLANNED"),
            Order("WO-3", due: new DateTime(2026, 11, 10), assignee: "omar", state: "UNDER_REVIEW"),
            Order("WO-4", due: new DateTime(2026, 10, 1), assignee: "nadia", state: "COMPLETED"),
        ], user: "omar");

        foreach (var queue in workspace.Queues)
        {
            Assert.Equal(queue.Count, workspace.In(queue.Key).Count);
        }

        Assert.Equal(2, workspace.In(WorkRules.QueueMine).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueUnassigned).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueOverdue).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueApproval).Count);
        Assert.Equal(4, workspace.In(WorkRules.QueueAll).Count);

        // The completed order is the only one past its due date that is not overdue (AC-3's other half).
        Assert.False(workspace.Orders.Single(order => order.Number == "WO-4").Overdue);

        Assert.Contains("Mine 2", workspace.CountsText, StringComparison.Ordinal);
        Assert.Contains("All 4", workspace.CountsText, StringComparison.Ordinal);
    }

    [Fact]
    public void Overdue_is_computed_from_the_planned_end_and_only_for_open_work()
    {
        // AC-3.
        Assert.True(WorkRules.IsOverdue(new DateTime(2026, 11, 7), isComplete: false, AsOf));
        Assert.False(WorkRules.IsOverdue(new DateTime(2026, 11, 8), isComplete: false, AsOf));
        Assert.False(WorkRules.IsOverdue(new DateTime(2026, 11, 9), isComplete: false, AsOf));
        Assert.False(WorkRules.IsOverdue(new DateTime(2026, 11, 1), isComplete: true, AsOf));
        Assert.False(WorkRules.IsOverdue(null, isComplete: false, AsOf));

        var overdue = Order("WO-1", due: new DateTime(2026, 11, 5), assignee: "omar", state: "IN_PROGRESS");

        Assert.Contains("(overdue)", overdue.DueText, StringComparison.Ordinal);
        Assert.Contains("no planned end", (overdue with { PlannedEnd = null }).DueText, StringComparison.Ordinal);
    }

    [Fact]
    public void Two_orders_of_one_person_on_overlapping_days_conflict()
    {
        // AC-2: the conflict names both orders, the person and the overlap.
        var conflicting = new List<OgWorkOrderSummary>
        {
            Order("WO-1", start: new DateTime(2026, 11, 1), due: new DateTime(2026, 11, 5), assignee: "omar", state: "IN_PROGRESS"),
            Order("WO-3", start: new DateTime(2026, 11, 3), due: new DateTime(2026, 11, 10), assignee: "omar", state: "PLANNED"),
        };

        var conflict = Assert.Single(WorkRules.ConflictsOf(conflicting));

        Assert.Equal("omar", conflict.Assignee);
        Assert.Equal("WO-1", conflict.First);
        Assert.Equal("WO-3", conflict.Second);
        Assert.Equal(new DateTime(2026, 11, 3), conflict.From);
        Assert.Equal(new DateTime(2026, 11, 5), conflict.To);
        Assert.Contains("omar is planned on WO-1 and WO-3", conflict.Describe(), StringComparison.Ordinal);

        // Different people never conflict, and back-to-back days do not overlap.
        Assert.Empty(WorkRules.ConflictsOf(
        [
            conflicting[0],
            conflicting[1] with { Assignee = "nadia" },
        ]));

        Assert.Empty(WorkRules.ConflictsOf(
        [
            conflicting[0],
            conflicting[1] with { PlannedStart = new DateTime(2026, 11, 6) },
        ]));

        // A completed order is not a resource demand, and neither is an unassigned one.
        Assert.Empty(WorkRules.ConflictsOf(
        [
            conflicting[0],
            conflicting[1] with { State = "COMPLETED" },
        ]));

        Assert.Empty(WorkRules.ConflictsOf(
        [
            conflicting[0],
            conflicting[1] with { Assignee = null },
        ]));
    }

    [Fact]
    public void The_calendar_and_the_grid_hold_the_same_work()
    {
        // AC-4: every order with a planned start is on the calendar, on each day of its span.
        var orders = new List<OgWorkOrderSummary>
        {
            Order("WO-1", start: new DateTime(2026, 11, 1), due: new DateTime(2026, 11, 3), assignee: "omar", state: "IN_PROGRESS"),
            Order("WO-2", start: new DateTime(2026, 11, 3), due: new DateTime(2026, 11, 3), assignee: null, state: "PLANNED"),
        };

        var calendar = WorkRules.CalendarOf(orders);

        Assert.Equal(3, calendar.Count);
        Assert.Equal(2, calendar.Single(day => day.Date.Day == 3).Count);

        // The third day carries four order-days in total, but two distinct orders - which is what the grid also lists.
        Assert.Equal(4, calendar.Sum(day => day.Count));
        Assert.Equal(2, Workspace(orders).CalendarOrders);

        // An order with no planned start is on the grid only, and the page says so.
        var undated = orders.Append(Order("WO-9", start: null, due: null, assignee: "omar", state: "DRAFT")).ToList();
        var workspace = Workspace(undated);

        Assert.Equal(3, workspace.Total);
        Assert.Equal(2, workspace.CalendarOrders);
    }

    [Fact]
    public void Unassigned_work_is_not_merged_with_assigned_work()
    {
        // AC-6.
        var unassigned = Order("WO-2", due: new DateTime(2026, 11, 20), assignee: null, state: "PLANNED");
        var assigned = unassigned with { Assignee = "omar" };

        Assert.True(unassigned.IsUnassigned);
        Assert.False(assigned.IsUnassigned);
        Assert.Contains("unassigned", unassigned.Describe(), StringComparison.Ordinal);
        Assert.DoesNotContain("unassigned", assigned.Describe(), StringComparison.Ordinal);

        var workspace = Workspace([unassigned, assigned], user: "omar");

        Assert.Equal(1, workspace.In(WorkRules.QueueUnassigned).Count);
        Assert.Equal(1, workspace.In(WorkRules.QueueMine).Count);
    }

    [Fact]
    public void An_order_with_no_recorded_state_says_so()
    {
        // The header does not establish operational state, so an order with no state row is not assumed to be a draft.
        var order = Order("WO-1", due: new DateTime(2026, 11, 20), assignee: "omar", state: string.Empty);

        Assert.Equal("no recorded state", order.StateText);
        Assert.False(order.IsComplete);
        Assert.Contains("no recorded state", order.Describe(), StringComparison.Ordinal);

        Assert.Equal("awaiting approval", WorkRules.Label("UNDER_REVIEW"));
        Assert.Equal("in progress", WorkRules.Label("IN_PROGRESS"));
        Assert.Equal("no recorded state", WorkRules.Label(null));
    }

    [Fact]
    public void The_page_shows_no_queue_after_a_failed_read_and_names_both_totals()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-5: the failed branch clears every figure rather than showing an empty queue.
        Assert.Contains("_counts.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_calendar.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_grid.ItemsSource = null", page, StringComparison.Ordinal);

        // AC-4: both totals are stated, and the refresh stamp is from the workspace.
        Assert.Contains("workspace.CalendarOrders", page, StringComparison.Ordinal);
        Assert.Contains("workspace.RefreshText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ConflictText", page, StringComparison.Ordinal);
    }

    private static OgWorkOrderSummary Order(
        string number,
        DateTime? due,
        string? assignee,
        string state,
        DateTime? start = null) =>
        new(
            "ID-" + number,
            number,
            "Corrective maintenance",
            "F-1",
            "UWI-1",
            "RUSH",
            state,
            assignee,
            start,
            due,
            null,
            WorkRules.IsOverdue(due, isComplete: state is "COMPLETED" or "CANCELLED", AsOf));

    private static OgWorkOrderWorkspace Workspace(IReadOnlyList<OgWorkOrderSummary> orders, string user = "") =>
        new(
            "F-1",
            user,
            null,
            null,
            orders,
            [
                new OgWorkQueue(WorkRules.QueueMine, "Mine",
                    orders.Count(order => !order.IsUnassigned &&
                                          string.Equals(order.Assignee, user, StringComparison.OrdinalIgnoreCase)),
                    "assigned to you"),
                new OgWorkQueue(WorkRules.QueueUnassigned, "Unassigned",
                    orders.Count(order => order.IsUnassigned), "no person is recorded"),
                new OgWorkQueue(WorkRules.QueueOverdue, "Overdue",
                    orders.Count(order => order.Overdue), "planned end has passed"),
                new OgWorkQueue(WorkRules.QueueApproval, "Awaiting approval",
                    orders.Count(order => order.AwaitingApproval), "under review"),
                new OgWorkQueue(WorkRules.QueueAll, "All", orders.Count, "everything in this view"),
            ],
            WorkRules.ConflictsOf(orders),
            WorkRules.CalendarOf(orders),
            AsOf,
            DateTime.UtcNow);
}
