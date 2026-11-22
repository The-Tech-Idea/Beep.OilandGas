using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-01: the SLA state rule, the median dwell, the period refusal, the "no figure after a failed read" behaviour and the
/// per-definition reconciliation - the parts that need no database. The reads over real rows are proven in the gateway test.
/// </summary>
public sealed class ProcessDashboardTests
{
    private const string PageFileName = "ProcessDashboardPageView.cs";

    private static readonly DateTime AsOf = new(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void No_sla_is_its_own_state_and_never_met()
    {
        // AC-2: an SLA that is not configured is reported as absent, not as satisfied.
        Assert.Equal(ProcessDashboardRules.NoSla, ProcessDashboardRules.SlaState(null, AsOf.AddHours(-1), AsOf));
        Assert.Equal(ProcessDashboardRules.NoSla, ProcessDashboardRules.SlaState(0, AsOf.AddHours(-1), AsOf));

        // A step that has not started is unscheduled, not overdue and not met.
        Assert.Equal(ProcessDashboardRules.Unscheduled,
            ProcessDashboardRules.SlaState(24, null, AsOf));

        // The three configured states, from the same SLA.
        Assert.Equal(ProcessDashboardRules.WithinSla,
            ProcessDashboardRules.SlaState(24, AsOf.AddHours(-1), AsOf));
        Assert.Equal(ProcessDashboardRules.AtRisk,
            ProcessDashboardRules.SlaState(24, AsOf.AddHours(-13), AsOf));
        Assert.Equal(ProcessDashboardRules.Breached,
            ProcessDashboardRules.SlaState(24, AsOf.AddHours(-25), AsOf));

        Assert.Contains(ProcessDashboardRules.NoSla, ProcessDashboardRules.States);
        Assert.Contains(ProcessDashboardRules.Unscheduled, ProcessDashboardRules.States);
    }

    [Fact]
    public void The_breach_is_the_owners_own_rule()
    {
        // The owner's monitor escalates a step whose elapsed time exceeds SLA_HOURS: equality is not yet a breach.
        Assert.Equal(ProcessDashboardRules.AtRisk,
            ProcessDashboardRules.SlaState(24, AsOf.AddHours(-23.99), AsOf));
        Assert.Equal(ProcessDashboardRules.Breached,
            ProcessDashboardRules.SlaState(24, AsOf.AddHours(-24), AsOf));
    }

    [Fact]
    public void The_bottleneck_is_a_median_not_an_average()
    {
        // AC-4: one very long instance must not name the wrong step.
        Assert.Equal(3m, ProcessDashboardRules.Median([1m, 3m, 3m, 9m, 100m]));
        Assert.Equal(2.5m, ProcessDashboardRules.Median([1m, 2m, 3m, 4m]));
        Assert.Equal(7m, ProcessDashboardRules.Median([7m]));

        // An average would have been 23.2 for the first set, which is precisely the defect a median avoids.
        Assert.NotEqual(23.2m, ProcessDashboardRules.Median([1m, 3m, 3m, 9m, 100m]));

        Assert.Throws<ArgumentException>(() => ProcessDashboardRules.Median([]));
    }

    [Fact]
    public void Elapsed_days_are_null_without_both_dates()
    {
        Assert.Equal(2m, ProcessDashboardRules.DaysBetween(AsOf.AddDays(-2), AsOf));
        Assert.Null(ProcessDashboardRules.DaysBetween(null, AsOf));
        Assert.Null(ProcessDashboardRules.DaysBetween(AsOf, null));

        // A completion before its start is not a negative dwell, it is no dwell.
        Assert.Null(ProcessDashboardRules.DaysBetween(AsOf, AsOf.AddDays(-1)));
    }

    [Fact]
    public void A_period_that_ends_before_it_begins_is_refused()
    {
        Assert.Null(ProcessDashboardRules.ValidatePeriod(AsOf.AddDays(-7), AsOf));

        var reversed = ProcessDashboardRules.ValidatePeriod(AsOf, AsOf.AddDays(-7));

        Assert.Equal(ProcessDashboardRules.CodePeriodInvalid, reversed!.Value.Code);
        Assert.Contains("before it begins", reversed.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_failed_read_is_a_failure_and_never_zero()
    {
        // AC-5.
        var offline = new OgProcessDashboardService(() => null, new Quiet());

        var loaded = await offline.LoadAsync("F-1", AsOf.AddDays(-30), AsOf);

        Assert.False(loaded.Success);
        Assert.Null(loaded.Value);
        Assert.Contains("No connection", loaded.Message!, StringComparison.Ordinal);

        // The refused period is refused before any read is attempted.
        var badPeriod = await offline.LoadAsync("F-1", AsOf, AsOf.AddDays(-1));

        Assert.False(badPeriod.Success);
        Assert.Equal(ProcessDashboardRules.CodePeriodInvalid, badPeriod.Reference);
    }

    [Fact]
    public void The_page_states_the_sla_rule_and_shows_no_figure_after_a_failed_read()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-3's caveat is on the page, so the derived at-risk rule is never presented as configured.
        Assert.Contains("half of its SLA window", page, StringComparison.Ordinal);
        Assert.Contains("no separate at-risk threshold", page, StringComparison.Ordinal);

        // AC-5: the failed branch clears every figure.
        Assert.Contains("_kpis.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_definitionGrid.ItemsSource = null", page, StringComparison.Ordinal);

        Assert.Contains("dashboard.Definitions", page, StringComparison.Ordinal);
        Assert.Contains("dashboard.Ageing", page, StringComparison.Ordinal);
        Assert.Contains("dashboard.BottleneckText", page, StringComparison.Ordinal);
    }

    private sealed class Quiet : TheTechIdeaWeb.Diagnostics.IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            TheTechIdeaWeb.Diagnostics.FailureSeverity severity = TheTechIdeaWeb.Diagnostics.FailureSeverity.Error) =>
            TheTechIdeaWeb.Diagnostics.FailureReferenceFormat.New();
    }
}
