using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEC-01: the state classification, the cost totals that exclude an unestimated asset, the computed overdue, the reference
/// labels and the gated links - the parts that need no database. The workspace over real records is proven in the gateway test.
/// </summary>
public sealed class DecommissioningWorkspaceTests
{
    private const string PageFileName = "DecommissioningPageView.cs";

    private static readonly DateTime Today = new(2026, 11, 8);

    [Fact]
    public void An_unestimated_asset_is_counted_and_left_out_of_the_totals()
    {
        // AC-2 and AC-4: money is decimal, kept in its currency, and a missing estimate is not a zero estimate.
        var scopes = new List<OgDecomScope>
        {
            Scope("FAC-1", "FACILITY", "PLANNED", estimated: 1_500_000.55m, currency: "USD", actual: 250_000.25m),
            Scope("FAC-2", "FACILITY", "PLANNED", estimated: null, currency: "USD", actual: null),
        };

        var costs = DecomRules.CostsOf(scopes);

        var usd = Assert.Single(costs);

        Assert.Equal(1_500_000.55m, usd.Estimated);
        Assert.Equal(250_000.25m, usd.Actual);
        Assert.Equal(1, usd.Unestimated);
        Assert.Contains("1 asset(s) not estimated", usd.Describe(), StringComparison.Ordinal);

        // Two currencies are never added together.
        var mixed = DecomRules.CostsOf(scopes.Append(
            Scope("FAC-3", "FACILITY", "PLANNED", estimated: 10m, currency: "NOK", actual: null)));

        Assert.Equal(2, mixed.Count);
        Assert.Equal(10m, mixed.Single(cost => cost.Currency == "NOK").Estimated);

        // A facility with no cost at all says "not estimated" rather than a figure.
        Assert.Equal("not estimated", scopes[1].EstimatedText);
        Assert.Contains("1500000.55 USD", scopes[0].EstimatedText, StringComparison.Ordinal);
    }

    [Fact]
    public void Overdue_is_computed_from_the_planned_year_at_read_time()
    {
        // AC-5: the same record is overdue in one year and not in another, because nothing is stored.
        var planned = Scope("FAC-1", "FACILITY", "PLANNED", estimated: 100m, currency: "USD") with { PlannedYear = 2025 };
        var future = planned with { PlannedYear = 2027 };
        var undated = planned with { PlannedYear = null };

        Assert.True(planned.IsOverdue(new DateTime(2026, 1, 1)));
        Assert.False(future.IsOverdue(new DateTime(2026, 1, 1)));

        // No planned year means nothing says it is late, so it is not overdue.
        Assert.False(undated.IsOverdue(new DateTime(2026, 1, 1)));

        // The workspace's own count uses the as-of date it was read with.
        var workspace = Workspace([planned, future, undated]);

        Assert.Equal(1, workspace.Overdue);
        Assert.Contains("planned years are recorded for 2 of 3 asset(s)", workspace.PlannedText,
            StringComparison.Ordinal);

        // With no planned year anywhere, the page says so instead of reporting zero overdue as a fact.
        var none = Workspace([undated]);

        Assert.Equal(0, none.Overdue);
        Assert.Contains("no planned year is recorded", none.PlannedText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_state_strip_is_the_recorded_states_not_invented_ones()
    {
        // AC-1: one row per recorded state, plus the unclassified one, with the owner's reference label.
        var reference = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DECOMMISSIONING_STATUS\u001fPLANNED"] = "Planned for decommissioning",
        };

        var scopes = new List<OgDecomScope>
        {
            Scope("FAC-1", "FACILITY", "PLANNED", estimated: 1m, currency: "USD"),
            Scope("FAC-2", "FACILITY", "PLANNED", estimated: 2m, currency: "USD"),
            Scope("WELL-1", "WELL", "SUSPENDED", estimated: null, currency: string.Empty),
            Scope("WELL-2", "WELL", string.Empty, estimated: null, currency: string.Empty),
        };

        var rows = DecomRules.Classify(scopes, reference, "DECOMMISSIONING_STATUS");

        Assert.Equal(3, rows.Count);
        Assert.Equal(2, rows.Single(row => row.State == "PLANNED").Count);
        Assert.Equal("Planned for decommissioning", rows.Single(row => row.State == "PLANNED").Label);

        // A state the reference set does not name is shown as its recorded code, not translated by guesswork.
        Assert.Equal("SUSPENDED", rows.Single(row => row.State == "SUSPENDED").Label);

        Assert.Equal("unclassified", rows.Single(row => row.State == string.Empty).Label);

        var workspace = Workspace(scopes);

        Assert.Equal(2, workspace.Wells);
        Assert.Equal(2, workspace.Facilities);
        Assert.Equal(1, workspace.Unclassified);
        Assert.Contains("unclassified", workspace.StateStrip, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_source_link_is_gated_on_its_own()
    {
        // AC-3: a page this build has is offered only when the shell would open it; a future page names its feature.
        var links = DecomRules.LinksFor(route => route == "wells");

        Assert.True(links.Single(link => link.Route == "wells").Available);
        Assert.Contains("may not open", links.Single(link => link.Route == "tasks").WhyNot!, StringComparison.Ordinal);

        // The well P&A page exists now, so it is refused by the account like the others.
        Assert.Contains("may not open", links.Single(link => link.Route == "wellpa").WhyNot!,
            StringComparison.Ordinal);

        // The facilities-and-cost page exists now too, so it is refused by the account rather than by its absence.
        Assert.Contains("may not open", links.Single(link => link.Route == "facilitycost").WhyNot!,
            StringComparison.Ordinal);

        // Every listed source now exists, so a link the account may open is offered and every other one is refused with the
        // route named - a different answer from a page this build does not have.
        Assert.All(links, link => Assert.True(
            link.Available || (link.WhyNot ?? string.Empty).Contains("may not open", StringComparison.Ordinal),
            link.Route));
    }

    [Fact]
    public void A_failed_read_prints_no_figure_at_all()
    {
        // AC-1: the failed workspace carries nothing a zero could stand for.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("_counts.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_costs.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_scopeGrid.ItemsSource = null", page, StringComparison.Ordinal);

        // The figures and the links come from the workspace, and the counts are never a literal.
        Assert.Contains("workspace.CountsText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.CostText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.LinksText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.InboxText", page, StringComparison.Ordinal);
        Assert.DoesNotMatch(new System.Text.RegularExpressions.Regex("Count\\s*=\\s*\\d"), page);
    }

    [Fact]
    public async Task A_workspace_without_a_connection_is_a_failure_not_an_empty_portfolio()
    {
        var workspace = new OgDecommissioning(() => null, new Inbox(), new Quiet());

        var loaded = await workspace.LoadAsync(
            "F-1", "DECOMMISSIONING_COORDINATOR", ["DecommissioningCoordinator"], Today, "UTC", _ => true);

        Assert.False(loaded.Success);
        Assert.Null(loaded.Value);
        Assert.Contains("No connection", loaded.Message!, StringComparison.Ordinal);
    }

    private static OgDecomScope Scope(
        string asset,
        string kind,
        string state,
        decimal? estimated,
        string currency,
        decimal? actual = null) =>
        new(asset, kind, state, state, null, estimated, actual, currency, "F-1");

    private static OgDecomWorkspace Workspace(IReadOnlyList<OgDecomScope> scopes) =>
        new(
            "F-1",
            scopes,
            DecomRules.Classify(scopes, new Dictionary<string, string>(), "DECOMMISSIONING_STATUS"),
            [],
            DecomRules.CostsOf(scopes),
            [],
            new OgHseInbox(true, 0, 0, 0, "overdue 0 · today 0 · all 0"),
            Today,
            DateTime.UtcNow);

    private sealed class Inbox : TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.IOgTaskInbox
    {
        public Task<TheTechIdea.Beep.OilandGas.Desktop.Core.OgResult<
                TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.OgTaskInboxSnapshot>> LoadAsync(
            string personaCode,
            IReadOnlyCollection<string> roles,
            string? fieldId,
            string fieldName,
            TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.OgTaskTab tab,
            DateTime asOf,
            string timeZoneId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TheTechIdea.Beep.OilandGas.Desktop.Core.OgResult<
                TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.OgTaskInboxSnapshot>.Ok(
                new TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.OgTaskInboxSnapshot(
                    personaCode,
                    tab,
                    [],
                    new TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow.OgTaskCounts(0, 0, 0, 0, 0, 0),
                    asOf)));

        public long BeginLoad() => 1;

        public bool IsCurrent(long version) => version == 1;
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
