using TheTechIdea.Beep.OilandGas.Desktop.Data.Work;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WRK-03: the next-due arithmetic, the derived compliance state, the frequency-unit rule, the findings gate and the generation
/// rule - the parts that need no database. The plan, job and inspection rows are proven in the gateway test.
/// </summary>
public sealed class MaintenancePlanTests
{
    private const string PageFileName = "MaintenancePageView.cs";

    // The plan's overdue days are counted against the clock, so the fixture takes its date from the clock rather than
    // pinning one: a fixed date turns this into a test that fails the next morning.
    private static readonly DateTime AsOf = DateTime.UtcNow.Date;

    [Fact]
    public void Next_due_is_last_done_plus_the_frequency_in_its_unit()
    {
        // AC-1: one subtraction from the last done date, in the unit the frequency is expressed in.
        var last = new DateTime(2026, 1, 15);

        Assert.Equal(new DateTime(2026, 2, 14), WorkMaintenanceRules.NextDue(last, 30, "DAY"));
        Assert.Equal(new DateTime(2026, 1, 29), WorkMaintenanceRules.NextDue(last, 2, "WEEK"));
        Assert.Equal(new DateTime(2026, 2, 15), WorkMaintenanceRules.NextDue(last, 1, "MONTH"));
        Assert.Equal(new DateTime(2027, 1, 15), WorkMaintenanceRules.NextDue(last, 1, "YEAR"));

        // No schedule, or no last done, means no next due - never a date invented from today.
        Assert.Null(WorkMaintenanceRules.NextDue(null, 30, "DAY"));
        Assert.Null(WorkMaintenanceRules.NextDue(last, null, "DAY"));
        Assert.Null(WorkMaintenanceRules.NextDue(last, 30, null));
        Assert.Null(WorkMaintenanceRules.NextDue(last, 30, "FORTNIGHT"));
    }

    [Fact]
    public void Compliance_is_derived_from_the_computed_due_date()
    {
        // AC-4: nothing is stored; the state follows the date against today.
        Assert.Equal(WorkMaintenanceRules.Overdue,
            WorkMaintenanceRules.Compliance(AsOf.AddDays(-1), isScheduled: true, AsOf));
        Assert.Equal(WorkMaintenanceRules.Due,
            WorkMaintenanceRules.Compliance(AsOf, isScheduled: true, AsOf));
        Assert.Equal(WorkMaintenanceRules.Current,
            WorkMaintenanceRules.Compliance(AsOf.AddDays(1), isScheduled: true, AsOf));
        Assert.Equal(WorkMaintenanceRules.NotScheduled,
            WorkMaintenanceRules.Compliance(null, isScheduled: false, AsOf));

        var overdue = Plan(nextDue: AsOf.AddDays(-3), compliance: WorkMaintenanceRules.Overdue);

        Assert.Contains("(3 day(s) overdue)", overdue.Describe(), StringComparison.Ordinal);
        Assert.Equal(3, overdue.DaysOverdue);
    }

    [Fact]
    public void A_frequency_always_carries_its_unit()
    {
        // AC-5.
        Assert.Null(WorkMaintenanceRules.ValidateFrequency(null, null));
        Assert.Null(WorkMaintenanceRules.ValidateFrequency(30, "DAY"));

        var noUnit = WorkMaintenanceRules.ValidateFrequency(30, null);

        Assert.Equal(WorkMaintenanceRules.CodeFrequencyUnitRequired, noUnit!.Value.Code);
        Assert.Contains("needs its unit", noUnit.Value.Message, StringComparison.Ordinal);

        var noFrequency = WorkMaintenanceRules.ValidateFrequency(null, "DAY");

        Assert.Equal(WorkMaintenanceRules.CodeFrequencyUnitRequired, noFrequency!.Value.Code);

        var badUnit = WorkMaintenanceRules.ValidateFrequency(30, "FORTNIGHT");

        Assert.Equal(WorkMaintenanceRules.CodeFrequencyUnitRequired, badUnit!.Value.Code);
        Assert.Contains("MONTH", badUnit.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_inspection_with_findings_cannot_close_without_a_linked_action()
    {
        // AC-2.
        Assert.Null(WorkMaintenanceRules.CheckClosure(hasFindings: false, hasLinkedAction: false, "PASS"));
        Assert.Null(WorkMaintenanceRules.CheckClosure(hasFindings: true, hasLinkedAction: true, "FAIL"));

        var refused = WorkMaintenanceRules.CheckClosure(hasFindings: true, hasLinkedAction: false, "FAIL");

        Assert.Equal(WorkMaintenanceRules.CodeFindingsNeedAction, refused!.Value.Code);
        Assert.Contains("cannot close as 'FAIL'", refused.Value.Message, StringComparison.Ordinal);
        Assert.Contains("corrective action", refused.Value.Message, StringComparison.Ordinal);

        var blocked = new OgInspectionRow(
            "I-1", "WO-1", "CLOSE_OUT", "FAIL", "Seal leaking", "omar", AsOf, AsOf,
            HasFindings: true, HasLinkedAction: false, IsClosed: false);

        Assert.True(blocked.IsBlocked);
        Assert.Contains("findings need an action", blocked.StateText, StringComparison.Ordinal);
        Assert.False((blocked with { HasLinkedAction = true }).IsBlocked);
    }

    [Fact]
    public void A_job_is_generated_only_from_a_plan_that_is_due()
    {
        // AC-3's other half: nothing is generated from a plan that is not due.
        Assert.Null(WorkMaintenanceRules.CheckGeneratable(Plan(AsOf, WorkMaintenanceRules.Overdue)));
        Assert.Null(WorkMaintenanceRules.CheckGeneratable(Plan(AsOf, WorkMaintenanceRules.Due)));

        var current = WorkMaintenanceRules.CheckGeneratable(Plan(AsOf.AddDays(10), WorkMaintenanceRules.Current));

        Assert.Equal(WorkMaintenanceRules.CodeNotDue, current!.Value.Code);
        Assert.Contains("no job is generated", current.Value.Message, StringComparison.Ordinal);

        Assert.Equal(WorkMaintenanceRules.CodeNotDue,
            WorkMaintenanceRules.CheckGeneratable(Plan(null, WorkMaintenanceRules.NotScheduled))!.Value.Code);
    }

    [Fact]
    public void The_page_binds_the_plans_the_jobs_and_the_inspections()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.Plans", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Jobs", page, StringComparison.Ordinal);
        Assert.Contains("workspace.Inspections", page, StringComparison.Ordinal);
        Assert.Contains("WorkMaintenanceRules.Units", page, StringComparison.Ordinal);

        // A failed read clears every grid rather than showing an empty plan list.
        Assert.Contains("_planGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    private static OgMaintenancePlan Plan(DateTime? nextDue, string compliance) =>
        new(
            "PLAN-1",
            "UWI-1",
            "WORKOVER",
            "PLANNED",
            30,
            "DAY",
            new DateTime(2026, 10, 1),
            nextDue,
            compliance,
            "Inspect the seal",
            "sara",
            new DateTime(2026, 10, 1));
}
