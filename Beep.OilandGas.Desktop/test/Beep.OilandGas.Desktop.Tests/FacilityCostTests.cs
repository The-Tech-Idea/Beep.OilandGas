using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEC-03: the cost arithmetic and its steps, the explicit currency, the approval's conditions and segregation of duties, and
/// the restoration gate that keeps the two states apart - the parts that need no database. The revisions themselves are proven
/// in the gateway test.
/// </summary>
public sealed class FacilityCostTests
{
    private const string PageFileName = "FacilityCostPageView.cs";

    [Fact]
    public void The_total_is_the_escalated_base_plus_its_contingency_step_by_step()
    {
        // AC-1: escalation composes multiplicatively and the contingency is a percentage of the escalated base.
        var computation = FacilityCostRules.Compute(
            [new OgCostLine("ENGINEERING", 400m), new OgCostLine("REMOVAL", 600m)],
            0.10m,
            2m,
            10m,
            "USD");

        Assert.Equal(1_000m, computation.Base);
        Assert.Equal(1_210m, computation.Escalated);
        Assert.Equal(121m, computation.ContingencyAmount);
        Assert.Equal(1_331m, computation.Total);
        Assert.True(computation.HasEscalationBasis);

        // The steps are shown, not only the result.
        Assert.Contains("base 1000.00 USD", computation.StepsText, StringComparison.Ordinal);
        Assert.Contains("× (1 + 10 %) ^ 2 year(s) = 1210.00 USD", computation.StepsText, StringComparison.Ordinal);
        Assert.Contains("+ contingency 10 % of 1210.00 USD = 121.00 USD", computation.StepsText, StringComparison.Ordinal);
        Assert.Contains("total 1331.00 USD", computation.StepsText, StringComparison.Ordinal);
        Assert.Equal("1331.00 USD", computation.TotalText);

        // A fractional period is applied as its own fraction of a year rather than rounded away.
        var halfYear = FacilityCostRules.Compute(1_000m, 0.10m, 1.5m, 0m, "USD");

        Assert.Equal(1_155m, halfYear.Escalated);

        // No escalation period is the feature's own "missing basis" state: base + contingency, and it says so.
        var none = FacilityCostRules.Compute(1_000m, 0.10m, 0m, 10m, "USD");

        Assert.False(none.HasEscalationBasis);
        Assert.Equal(1_000m, none.Escalated);
        Assert.Equal(1_100m, none.Total);
        Assert.Contains("no escalation basis", none.StepsText, StringComparison.Ordinal);
        Assert.Contains("base + contingency only", none.StepsText, StringComparison.Ordinal);
    }

    [Fact]
    public void Money_is_rounded_once_to_the_currency_minor_unit()
    {
        var computation = FacilityCostRules.Compute(
            [new OgCostLine("REMOVAL", 1_000.005m)], 0m, 0m, 0m, "USD");

        // Away from zero, once, at the total - not at every intermediate step.
        Assert.Equal(1_000.01m, computation.Total);
        Assert.Equal(2, decimal.GetBits(computation.Total)[3] >> 16 & 0xFF);
    }

    [Fact]
    public void An_estimate_needs_its_currency_and_a_reason_from_the_second_revision()
    {
        // AC-5 and the stage's open question: nothing is defaulted.
        Assert.Null(FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 10m, null, 1));

        Assert.Equal(FacilityCostRules.CodeCurrencyRequired, FacilityCostRules.ValidateEstimate(
            "FAC-1", "  ", [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 10m, null, 1)!.Value.Code);

        Assert.Contains("None is assumed", FacilityCostRules.ValidateEstimate(
            "FAC-1", null, [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 10m, null, 1)!.Value.Message,
            StringComparison.Ordinal);

        Assert.Equal(FacilityCostRules.CodeFacilityRequired, FacilityCostRules.ValidateEstimate(
            null, "USD", [new OgCostLine("REMOVAL", 100m)], 0m, 0m, 0m, null, 1)!.Value.Code);

        Assert.Equal(FacilityCostRules.CodeLineInvalid, FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [], 0m, 0m, 0m, null, 1)!.Value.Code);

        Assert.Equal(FacilityCostRules.CodeLineInvalid, FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", -1m)], 0m, 0m, 0m, null, 1)!.Value.Code);

        Assert.Equal(FacilityCostRules.CodeRateInvalid, FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", 100m)], 1.5m, 2m, 10m, null, 1)!.Value.Code);

        Assert.Equal(FacilityCostRules.CodeContingencyInvalid, FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 150m, null, 1)!.Value.Code);

        // A first estimate may explain itself in the record; a revision must.
        Assert.Null(FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 10m, null, 1));

        var revision = FacilityCostRules.ValidateEstimate(
            "FAC-1", "USD", [new OgCostLine("REMOVAL", 100m)], 0.03m, 2m, 10m, " ", 2);

        Assert.Equal(FacilityCostRules.CodeReasonRequired, revision!.Value.Code);
        Assert.Contains("why the estimate changed", revision.Value.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_approval_records_conditions_and_cannot_be_given_by_its_author()
    {
        // AC-6.
        var draft = Revision(author: "sara", status: FacilityCostRules.Draft);

        var self = FacilityCostRules.ValidateApproval(draft, "sara", "no conditions");

        Assert.Equal(FacilityCostRules.CodeSelfApproval, self!.Value.Code);
        Assert.Contains("cannot approve it", self.Value.Message, StringComparison.Ordinal);

        Assert.Equal(FacilityCostRules.CodeConditionsRequired,
            FacilityCostRules.ValidateApproval(draft, "nadia", " ")!.Value.Code);

        Assert.Null(FacilityCostRules.ValidateApproval(draft, "nadia", "not to be exceeded without a new revision"));

        var approved = Revision(author: "sara", status: FacilityCostRules.Approved);

        Assert.Equal(FacilityCostRules.CodeAlreadyApproved,
            FacilityCostRules.ValidateApproval(approved, "nadia", "again")!.Value.Code);
    }

    [Fact]
    public void Restoration_state_is_its_own_record_and_gates_completion()
    {
        // AC-3: neither state is derived from the other; the restoration only gates the facility's completion.
        var planned = Workspace(state: FacilityCostRules.Planned, restoration: []);

        var noRestoration = FacilityCostRules.CompletionBlocks(planned);

        Assert.Contains(noRestoration, block => block.Contains("No environmental restoration", StringComparison.Ordinal));

        var open = Workspace(state: FacilityCostRules.Planned,
        [
            Restoration(completed: null),
            Restoration(completed: new DateTime(2026, 10, 1)),
        ]);

        Assert.Contains(FacilityCostRules.CompletionBlocks(open),
            block => block.Contains("1 environmental restoration record(s) are not complete", StringComparison.Ordinal));

        // Restoration complete does not make the facility complete: its own state still says planned.
        var restored = Workspace(state: FacilityCostRules.Planned, [Restoration(completed: new DateTime(2026, 10, 1))]);

        Assert.Empty(FacilityCostRules.CompletionBlocks(restored));
        Assert.False(restored.IsComplete);
        Assert.Equal("planned", restored.Facility!.StateLabel);

        // And a facility completed while a restoration record is open is not reachable through the command: the rule blocks it.
        var completedWithOpenRestoration = Workspace(state: FacilityCostRules.Completed, [Restoration(completed: null)]);

        Assert.True(completedWithOpenRestoration.IsComplete);
        Assert.Contains(FacilityCostRules.CompletionBlocks(completedWithOpenRestoration),
            block => block.Contains("already completed", StringComparison.Ordinal));
    }

    [Fact]
    public void The_provision_preview_names_its_accounting_period()
    {
        // AC-4: the preview states the amount and the period, and says that ACC-02 owns the posting.
        var approved = Revision(author: "sara", status: FacilityCostRules.Approved);

        var workspace = Workspace(
            state: FacilityCostRules.Planned,
            [Restoration(completed: new DateTime(2026, 10, 1))],
            plannedDate: new DateTime(2027, 3, 15),
            revisions: [approved]);

        Assert.Equal("2027-03", workspace.ProvisionPeriod);
        Assert.Contains("provision preview", workspace.ProvisionText, StringComparison.Ordinal);
        Assert.Contains("1331.00 USD", workspace.ProvisionText, StringComparison.Ordinal);
        Assert.Contains("2027-03", workspace.ProvisionText, StringComparison.Ordinal);
        Assert.Contains("ACC-02 owns the posting", workspace.ProvisionText, StringComparison.Ordinal);

        // An unapproved current revision cannot be provided against, and the sentence says why.
        var draftOnly = Workspace(
            state: FacilityCostRules.Planned,
            [Restoration(completed: new DateTime(2026, 10, 1))],
            plannedDate: new DateTime(2027, 3, 15),
            revisions: [Revision(author: "sara", status: FacilityCostRules.Draft)]);

        Assert.Contains("no approved estimate", draftOnly.ProvisionText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_shows_the_steps_the_provision_and_the_currency_and_defaults_nothing()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("workspace.CostText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ProvisionText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.ChecklistText", page, StringComparison.Ordinal);

        // AC-5 and the open question: no currency, rate, period or contingency is filled in for the operator.
        Assert.DoesNotContain("_currency.Text = ", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_rate.Text = ", page, StringComparison.Ordinal);
        Assert.DoesNotContain("_contingency.Text = ", page, StringComparison.Ordinal);
    }

    private static OgCostRevision Revision(string author, string status) =>
        new(
            "REV-1",
            1,
            null,
            "First estimate.",
            "USD",
            status,
            author,
            status == FacilityCostRules.Approved ? "nadia" : null,
            status == FacilityCostRules.Approved ? new DateTime(2026, 11, 2) : null,
            status == FacilityCostRules.Approved ? "no conditions" : null,
            [new OgCostLine("REMOVAL", 1_331m)],
            FacilityCostRules.Compute(1_331m, 0m, 0m, 0m, "USD"));

    private static OgRestoration Restoration(DateTime? completed) =>
        new("R-1", "FAC-1", "SOIL", completed is null ? FacilityCostRules.Planned : FacilityCostRules.Completed,
            completed is null ? "PENDING" : "CERTIFIED", completed);

    private static OgFacilityWorkspace Workspace(
        string state,
        IReadOnlyList<OgRestoration> restoration,
        DateTime? plannedDate = null,
        IReadOnlyList<OgCostRevision>? revisions = null)
    {
        var workspace = new OgFacilityWorkspace(
            new OgHseScope("SMOKE", "F-1"),
            "FAC-1",
            new OgFacilityScope("FAC-1", state, FacilityCostRules.Label(state), plannedDate, "sara", "REMOVAL"),
            revisions ?? [],
            restoration,
            [],
            [],
            DateTime.UtcNow);

        return workspace with { Blocks = FacilityCostRules.CompletionBlocks(workspace) };
    }
}
