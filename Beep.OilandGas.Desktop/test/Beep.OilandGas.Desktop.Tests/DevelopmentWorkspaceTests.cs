using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-01: the plan counts, the date-derived attention, the program gap, the spend arithmetic and the source gates -
/// the parts that do not need a database. The real workspace read is proven in the gateway test.
/// </summary>
public sealed class DevelopmentWorkspaceTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);

    [Fact]
    public void Counts_come_from_the_records_and_a_blank_state_is_unclassified()
    {
        var plans = new[]
        {
            Plan("P-1", "APPROVED"),
            Plan("P-2", "APPROVED"),
            Plan("P-3", "DRAFT"),
            Plan("P-4", null),
        };

        var counts = OgDevelopmentWorkspace.CountPlans(plans);

        Assert.Equal(3, counts.Count);
        Assert.Equal(2, counts.Single(count => count.PlanState == "APPROVED").Count);
        Assert.Equal(1, counts.Single(count => count.PlanState == "DRAFT").Count);
        Assert.Equal(1, counts.Single(count => count.PlanState == "(unclassified)").Count);
    }

    [Fact]
    public void Attention_is_derived_from_the_review_window_not_a_stored_flag()
    {
        var plans = new[]
        {
            Plan("P-old", "DRAFT", AsOf.AddDays(-45)),
            Plan("P-new", "DRAFT", AsOf.AddDays(-5)),
            Plan("P-approved", "APPROVED", AsOf.AddDays(-45)),
        };

        var attention = OgDevelopmentWorkspace.PlanAttention(plans, AsOf);

        var item = Assert.Single(attention);

        Assert.Equal("P-old", item.Subject);
        Assert.True(item.Overdue);
        Assert.Contains("45 d", item.Detail, StringComparison.Ordinal);
        Assert.Equal("fdpwizard", item.Route);
    }

    [Fact]
    public void Exactly_the_review_window_is_attention()
    {
        var plan = Plan("P-edge", "DRAFT", AsOf.AddDays(-OgDevelopmentWorkspace.ReviewWindowDays));

        Assert.Single(OgDevelopmentWorkspace.PlanAttention([plan], AsOf));
    }

    [Fact]
    public void A_scheduled_well_without_an_approved_program_is_attention()
    {
        var schedules = new[]
        {
            Schedule("S-1", "WELL-1", "PLANNED"),
            Schedule("S-2", "WELL-2", "DRILLING"),
            Schedule("S-3", "WELL-3", "COMPLETED"),
        };

        var programs = new[]
        {
            Program("PR-1", "WELL-2", "APPROVED"),
        };

        var attention = OgDevelopmentWorkspace.ProgramAttention(schedules, programs);

        var item = Assert.Single(attention);

        Assert.Equal("WELL-1", item.Subject);
        Assert.Equal(OgDevelopmentAttention.ProgramMissing, item.Kind);
        Assert.Equal("drillingprogram", item.Route);
    }

    [Fact]
    public void Each_source_link_is_gated_independently()
    {
        var links = OgDevelopmentWorkspace.BuildLinks(route =>
            string.Equals(route, "wells", StringComparison.OrdinalIgnoreCase));

        var wells = links.Single(link => link.Route == "wells");
        var design = links.Single(link => link.Route == "welldesign");

        Assert.True(wells.Available);
        Assert.Null(wells.WhyNot);

        // DEV-03's page now exists, so the link is refused by the page gate, not by a missing feature.
        Assert.False(design.Available);
        Assert.Equal("Not available to this account.", design.WhyNot);
    }

    [Fact]
    public void Spend_is_dated_and_grouped_by_currency()
    {
        var costs = new[]
        {
            Cost("C-1", AsOf.AddDays(-10), "USD", total: 100m),
            Cost("C-2", AsOf.AddDays(10), "USD", total: 900m),
            Cost("C-3", null, "EUR", total: 20m),
            Cost("C-4", AsOf.AddDays(-2), "USD", total: null, drilling: 10m, equipment: 5m),
        };

        var spend = OgDevelopmentWorkspace.SumSpend(costs, AsOf);

        Assert.Equal(2, spend.Count);
        Assert.Equal(115m, spend.Single(item => item.Currency == "USD").Amount);
        Assert.Equal(20m, spend.Single(item => item.Currency == "EUR").Amount);
        Assert.Equal("115 MM USD", spend.Single(item => item.Currency == "USD").Text);
    }

    [Fact]
    public void The_development_landing_route_is_the_seeded_persona_route_and_is_in_the_catalog()
    {
        Assert.Equal("development", OgPersonaLanding.RouteFor("/lifecycle/development/dashboard"));
        Assert.Equal("facilities", OgPersonaLanding.RouteFor("/facilities/dashboard"));
        Assert.True(OgModuleCatalog.IsKnownRoute("development"));
        Assert.Contains(OgDevelopmentWorkspace.Sources, source => source.Route == "wells" && source.ArrivesWith is null);
    }

    [Fact]
    public async Task No_connection_is_a_failure_not_zero_counts()
    {
        var workspace = new OgDevelopmentWorkspace(() => null, new Quiet());

        var loaded = await workspace.LoadAsync(null, string.Empty, AsOf, _ => true);

        Assert.False(loaded.Success);
        Assert.Equal(OgDevelopmentWorkspace.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    private static FIELD_DEVELOPMENT_PLAN Plan(string id, string? status, DateTime? submitted = null) =>
        new()
        {
            FDP_ID = id,
            FIELD_ID = "F-1",
            FDP_NAME = id,
            FDP_STATUS = status,
            SUBMISSION_DATE = submitted,
        };

    private static DEVELOPMENT_WELL_SCHEDULE Schedule(string id, string wellId, string status) =>
        new()
        {
            SCHEDULE_ID = id,
            FIELD_ID = "F-1",
            PLANNED_WELL_ID = wellId,
            WELL_NAME = wellId,
            SCHEDULE_STATUS = status,
        };

    private static DRILLING_PROGRAM Program(string id, string wellId, string status) =>
        new()
        {
            PROGRAM_ID = id,
            WELL_ID = wellId,
            FIELD_ID = "F-1",
            PROGRAM_STATUS = status,
        };

    private static DEVELOPMENT_COSTS Cost(
        string id,
        DateTime? date,
        string currency,
        decimal? total,
        decimal? drilling = null,
        decimal? equipment = null) =>
        new()
        {
            DEVELOPMENT_COST_ID = id,
            FIELD_ID = "F-1",
            COST_DATE = date,
            COST_CURRENCY = currency,
            TOTAL_DEVELOPMENT_COSTS = total,
            DEVELOPMENT_WELL_DRILLING_COSTS = drilling,
            DEVELOPMENT_WELL_EQUIPMENT = equipment,
        };

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
